using Semagsoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Xml;

namespace YATE
{
    /// <summary>
    /// Interaction logic for YATEditor.xaml
    /// </summary>
    public partial class YATEditor : UserControl
    {
        public YATEditor()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(rtb_Main, new DataObjectPastingEventHandler(OnPaste));
            // Document.PageWidth = 1024;
            PreviewMouseDown += YATE_PreviewMouseDown;
            PreviewMouseUp += YATE_PreviewMouseUp;
            rtb_Main.PreviewKeyDown += Rtb_Main_PreviewKeyDown;


            this.SizeChanged += YATEditor_SizeChanged;

            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            rtb_Main.Document.Resources.Add(typeof(Paragraph), style);


            rtb.Document.PagePadding = PAGE_PADDING;
            rtb.Document.ColumnWidth = rtb.Document.PageWidth;

            search_helper = new SearchHelper(rtb);

        }

       
        public void ClearAdorners()
        {
            search_helper.ClearSearchHelper();
            image_helper.ClearImageResizers(rtb);
        }


        #region Search Highligh

        private void UpdateAdorners()
        {
            search_helper.UpdateSearchHelper();
        }


        public void SearchClear()
        {
            search_helper.ClearSearchHelper();
        }

        public void Search(string text)
        {
            search_helper.InitSearchHelper();
            string t = this.PlainText;
            TextRange tr =   YATEHelper.FindText(Document.ContentStart, Document.ContentEnd, text,YATEHelper.FindFlags.None,System.Globalization.CultureInfo.CurrentCulture);


            if (tr != null)
                cmdScroll(tr.Start);
            while (tr!=null)
            {
               
                search_helper.AddSearchHelper(tr);
                 tr =  YATEHelper.FindText(tr.End, Document.ContentEnd, text, YATEHelper.FindFlags.None, System.Globalization.CultureInfo.CurrentCulture);
            }

        //for (TextPointer position = this.Document.ContentStart;
        //position != null && position.CompareTo(Document.ContentEnd) <= 0;
        //position = position.GetNextContextPosition(LogicalDirection.Forward))
        //    {
        //        if (position.CompareTo(Document.ContentEnd) == 0)
        //        {
        //            return;
        //        }
        //        String textRun = position.GetTextInRun(LogicalDirection.Forward);
        //        StringComparison stringComparison = StringComparison.CurrentCulture;
        //        Int32 indexInRun = textRun.IndexOf(text, stringComparison);
        //        if (indexInRun >= 0)
        //        {
        //            position = position.GetPositionAtOffset(indexInRun);
        //            if (position != null)
        //            {
        //                TextPointer nextPointer = position.GetPositionAtOffset(text.Length);
        //                TextRange textRange = new TextRange(position, nextPointer);
        //                textRange.ApplyPropertyValue(TextElement.BackgroundProperty,
        //                              new SolidColorBrush(Colors.Yellow));
        //            }
        //        }
        //    }
        }


        #endregion

        public static readonly Thickness PAGE_PADDING = new Thickness(5);


        public RichTextBox rtb
        {
            get
            {
                return rtb_Main;
            }
        }

        private ImageHelper image_helper = new ImageHelper();
        private SearchHelper search_helper;



        FixedDocument _current = null;
        public FixedDocument DocumentFixedGet()
        {
                FlowDocument fd = rtb.Document;
                double pw = rtb.Document.PageWidth;

                fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
                fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
                fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
                fd.PagePadding = PrintLayout.A4Narrow.Margin;


                var paginator = ((IDocumentPaginatorSource)rtb.Document).DocumentPaginator;

                    var package = System.IO.Packaging.Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
                    var packUri = new Uri("pack://temporary.xps");
                    System.IO.Packaging.PackageStore.RemovePackage(packUri);
                    System.IO.Packaging.PackageStore.AddPackage(packUri, package);
                    var xps = new System.Windows.Xps.Packaging.XpsDocument(package, System.IO.Packaging.CompressionOption.NotCompressed, packUri.ToString());
                    System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xps).Write(paginator);
                    _current = xps.GetFixedDocumentSequence().References[0].GetDocument(true);

                    fd.PageWidth = pw;
                    fd.PagePadding = PAGE_PADDING;

              return _current;
      
        }


        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            return YATEHelper.TryFindParent<T>(child);
        }

        public static DependencyObject GetParentObject(DependencyObject child)
        {
            return YATEHelper.GetParentObject(child);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            return YATEHelper.FindVisualChildren<T>(depObj);
        }

        public Span GetImage(
            BitmapSource image,
            ImageContentType contnt_type = ImageContentType.ImagePngContentType,
            Stretch stretch = Stretch.Fill)
        {
            return YATEHelper.GetImage(image, contnt_type, stretch);
        }

        //./images enforce*/
        public FlowDocument Document
        {
            get { return rtb_Main.Document; }

            set { this.rtb_Main.Document = value; }
        }


        private void YATEditor_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double nw = e.NewSize.Width - 10;
            this.PageWidth = (nw < this.MinWidth ? MinWidth : nw);
            UpdateAdorners();
        }

        private void DetectURL()
        {
            TextPointer position = this.CaretPosition;
            string textRun = position.GetTextInRun(LogicalDirection.Backward);
            int pos = 0;
            for (int i = textRun.Length - 1; i >= 0; i--)
            {
                char c = textRun[i];
                if (char.IsWhiteSpace(c))
                {
                    pos = i + 1; break;
                }
            }

            int offs = textRun.Length - pos;
            string original; string urlp = original = textRun.Substring(pos, offs).Trim();
            Uri uriResult;
            if (YATEHelper.IsHyperlink(ref urlp, out uriResult))
            {
                //Replace URL with hyperlink

                TextPointer tb = position.GetPositionAtOffset(-offs);
                tb.DeleteTextInRun(offs);
                // rtb_Main.Selection.Select(tb, position);

                //TextRange tr = new TextRange(position, tb);
                //this.CaretPosition.Ins
                Hyperlink h = new Hyperlink(tb, tb);
                h.Cursor = Cursors.Hand;
                h.NavigateUri = uriResult;
                h.Inlines.Add(new Run(original));
                h.FontSize = 16;
                this.CaretPosition = h.ElementEnd;
                //rtb_Main.Selection.Select(tb, position);
                //cmdDelete();
                //cmdInsertInline(h);
            }
        }

        private void Rtb_Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //DetectURL();
                    //if (Keyboard.Modifiers == ModifierKeys.Control)
                    //{
                    //    this.cmdInsertParagraphBreak();
                    //}
                    //else
                    //{
                    //    this.cmdInsertLine();
                    //}
                    //e.Handled = true;

                    break;
                case Key.Space:
                case Key.Tab:
                    DetectURL();
                    break;
                case Key.B:

                    break;
            }
        }

        public TextPointer CaretPosition
        {
            get { return rtb_Main.CaretPosition; }
            set { this.rtb_Main.CaretPosition = value; }
        }

        public int CurrentPoistionValue
        {
            get
            {
                return rtb_Main.Document.ContentStart.GetOffsetToPosition(rtb_Main.CaretPosition);
            }
            set
            {
                rtb_Main.CaretPosition = rtb_Main.Document.ContentStart.GetPositionAtOffset(value, LogicalDirection.Forward);
            }
        }


        public void LoadFile( string filename)
        {
            try {
                string ext = Path.GetExtension(filename).ToUpper();
                switch (ext)
                {
                    case ".DOCX":
                        LoadFileDocX(filename);
                        break;
                    case ".XPS":
                        LoadFileXPS(filename);
                        break;
                    case ".XAML":
                    default:
                        LoadFileXAML(filename);
                        break;
                }
            }catch(Exception)
            {

            }

        }

        public void LoadFileDocX(string filename)
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {

                var flowDocumentConverter = new DocxReaderApplication.DocxToFlowDocumentConverter(stream);
                flowDocumentConverter.Read();
                this.Document = flowDocumentConverter.Document;
            }
        }

        public void LoadFileXPS(string filename)
        {
            FlowDocument doc = DocEdLib.XPS.XPStoFlowDocument.Convert(filename);
            if (doc != null)
            {
                rtb_Main.Document = doc;
            }
        }

        public void LoadFileXAML(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            this.Load(fs);
        }


        public void LoadHTML(string html)
        {

            Load();
            Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(html, rtb_Main.Document.PageWidth);
            this.cmdInsertBlock(s);
            Semagsoft.HyperlinkHelper.SubscribeToAllHyperlinks(rtb_Main.Document);
        }

        /**/
        public void Load()
        {
            rtb.Document.Blocks.Clear();
        }

        public void Load(Stream stream)
        {
            var content = new TextRange(rtb.Document.ContentStart, rtb_Main.Document.ContentEnd);
            content.Load(stream, System.Windows.DataFormats.XamlPackage);
        }

        public void Save(Stream stream)
        {
            byte[] b = this.ComproessedDocument;
            stream.Write(b, 0, b.Length);

            stream.Flush();
            stream.Close();
        }
        

       
        public string SaveFile(string filename)
        {
            string ext = Path.GetExtension(filename).ToUpper();
            switch (ext)
            {
                case ".PDF":
                    SaveFilePDF(filename);
                    return ".PDF";
                case ".XPS":
                    SaveFileXPS(filename);
                    return ".XPS";
                case ".XAML":
                default:
                    SaveFileXAML(filename);
                    break;
            }
            return ".XAML";
        }

        public void SaveFileXAML(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                TextRange textRange = new TextRange(Document.ContentStart, Document.ContentEnd);
                textRange.Save(fs, DataFormats.XamlPackage);
            }
        }


        public void SaveFilePDF(string fileName)
        {
            FlowDocument fd = rtb.Document;
            double pw = rtb.Document.PageWidth;

            MemoryStream lMemoryStream = new MemoryStream();

            fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
            fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
            fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
            fd.PagePadding = PrintLayout.A4Narrow.Margin;

            using (Package container = Package.Open(lMemoryStream, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    var paginator = ((IDocumentPaginatorSource)rtb.Document).DocumentPaginator;
                    rsm.SaveAsXaml(paginator);
                }
            }

            fd.PageWidth = pw;
            fd.PagePadding = PAGE_PADDING;

            PdfSharp.Xps.XpsModel.XpsDocument pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
            PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, fileName);
        }

        public void SaveFileXPS(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);


            FlowDocument fd = rtb.Document;
            double pw = rtb.Document.PageWidth;

            fd.PageWidth = PrintLayout.A4Narrow.Size.Width;
            fd.ColumnWidth = PrintLayout.A4Narrow.ColumnWidth;
            fd.PageHeight = PrintLayout.A4Narrow.Size.Height;
            fd.PagePadding = PrintLayout.A4Narrow.Margin;

            using (Package container = Package.Open(fileName, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    var paginator = ((IDocumentPaginatorSource)rtb.Document).DocumentPaginator;
                    rsm.SaveAsXaml(paginator);
                }
            }

            fd.PageWidth = pw;
            fd.PagePadding = PAGE_PADDING;
        }



        public void Load(string XAML)
        {
            StringReader stringReader = new StringReader(XAML);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            var v = System.Windows.Markup.XamlReader.Load(xmlReader);
            if (v is FlowDocument)
            {
                rtb_Main.Document = v as FlowDocument;
            }
            else if (v is Section)
            {
                Section sec = v as Section;
                FlowDocument doc = new FlowDocument();
                while (sec.Blocks.Count > 0)
                    doc.Blocks.Add(sec.Blocks.FirstBlock);
                rtb_Main.Document = doc;
            }
        }

        public object LoadXAMLElement(string value)
        {
            return LoadXAMLElement(
                System.Xml.XmlReader.Create(new StringReader(value)));
        }

        public object LoadXAMLElement(Stream stream)
        {
            return System.Windows.Markup.XamlReader.Load(stream);
        }

        public object LoadXAMLElement(XmlReader stream)
        {
            return System.Windows.Markup.XamlReader.Load(stream);
        }

        public string XAML
        {
            get
            {
                return System.Windows.Markup.XamlWriter.Save(rtb_Main.Document);
            }

            set
            {
                var v = LoadXAMLElement(value);
                if (v is FlowDocument)
                {
                    rtb_Main.Document = v as FlowDocument;
                }
                else if (v is Section)
                {
                    Section sec = v as Section;
                    FlowDocument doc = new FlowDocument();
                    while (sec.Blocks.Count > 0)
                        doc.Blocks.Add(sec.Blocks.FirstBlock);
                    rtb_Main.Document = doc;
                }
            }
        }

        public string PlainText
        {
            get
            {
                TextRange tr = new TextRange(rtb_Main.Document.ContentStart, rtb_Main.Document.ContentEnd);
                return tr.Text;
            }
        }

        private byte[] SaveAllContent(RichTextBox rtb)
        {
            var content = new TextRange(rtb.Document.ContentStart, rtb_Main.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                content.Save(ms, DataFormats.XamlPackage, true);
                return ms.ToArray();
            }
        }

        private void LoadAllContent(byte[] bd, RichTextBox rtb)
        {
            var content = new TextRange(rtb.Document.ContentStart, rtb_Main.Document.ContentEnd);
            MemoryStream ms = new MemoryStream(bd);
            content.Load(ms, System.Windows.DataFormats.XamlPackage);
        }

        public byte[] ComproessedDocument
        {
            get
            {
                return SaveAllContent(rtb_Main);
            }

            set
            {
                LoadAllContent(value, rtb_Main);
            }
        }

        public bool isTable
        {
            get
            {
                return TryFindParent<Table>(rtb.CaretPosition.Parent as DependencyObject) != null;
            }
        }

        public Thickness TableBorderTickness
        {
            get
            {
                Thickness tick = new Thickness(0);
                Table t;
                List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);
                if (ltc.Count <= 0) return tick;

                tick = ltc[0].BorderThickness;
                foreach(TableCell tc in ltc)
                {
                    if(tc.BorderThickness.Left!= tick.Left)
                    {
                        tick.Left = 0;
                    }
                    if (tc.BorderThickness.Right != tick.Right)
                    {
                        tick.Right = 0;
                    }

                    if (tc.BorderThickness.Top != tick.Top)
                    {
                        tick.Top = 0;
                    }
                    if (tc.BorderThickness.Bottom != tick.Bottom)
                    {
                        tick.Bottom = 0;
                    }
                }

                return tick;
            }
        }

      



        public bool isBold
        {
            get
            {
                return (SelectionFontWeight == FontWeights.Bold);
            }
        }


        public bool isItalic
        {
            get
            {
                return (SelectionFontStyle == FontStyles.Italic);
            }
        }

        public bool isUnderline
        {
            get
            {
                TextDecorationCollection tdc = this.SelectionTextDecoration;
                if (tdc == null) return false;
                return tdc.Contains(TextDecorations.Underline[0]);
            }
        }

        public void CloneRun(Run source, Run dest)
        {
            dest.FontFamily = source.FontFamily;
            dest.FontSize = source.FontSize;
            dest.FontWeight = source.FontWeight;
            dest.Foreground = source.Foreground;
            dest.Background = source.Background;
            dest.TextDecorations = source.TextDecorations;
        }

        public static double PointsToPixels(double points)
        {
            return points * (96.0 / 72.0);
        }

        public static double PixelsToPoints(double pixels)
        {
            return pixels * (72.0 / 96.0);
        }


        public string SelectionFontSizePoints
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return "12";
                }
                double v = Convert.ToDouble(n);
                return Convert.ToString((int)Math.Round(PixelsToPoints(v)));
            }
            set
            {

                int v = (int)Math.Round(PointsToPixels(Convert.ToDouble(value)));
                rtb_Main.Selection.ApplyPropertyValue(TextElement.FontSizeProperty,

                    Convert.ToString(v)
                    );
            }
        }

        public string SelectionFontSize
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return "12";
                }
                return (string)n;
            }
            set
            {
                rtb_Main.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, value);
            }
        }

        public FontFamily SelectionFontFamily
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                FontFamily ff = (FontFamily)n;

                //string[] fn = ff.FamilyNames;
                return ff;

            }
            set
            {
                rtb_Main.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, value);

            }
        }

        public FontWeight SelectionFontWeight
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.FontWeightProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return FontWeights.Normal;
                }
                return (FontWeight)n;
            }
            set
            {
                //if (value == SelectionFontWeight) return;
                //if (rtb_Main.Selection.IsEmpty)
                //{
                //    DependencyObject se = rtb_Main.Selection.Start.Parent;
                //    Run r = new Run("", rtb_Main.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.FontWeight = value;
                //}
                //else
                //{
                //}
                rtb_Main.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, value);
            }
        }

        public TextDecorationCollection SelectionTextDecoration
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (TextDecorationCollection)n;
            }
        }


        public Brush SelectionBackground
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (Brush)n;
            }
            set
            {
                //if (rtb_Main.Selection.IsEmpty)
                //{
                //    DependencyObject se = rtb_Main.Selection.Start.Parent;
                //    Run r = new Run("", rtb_Main.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.Background = value;
                //}
                //else
                //{

                //}

                rtb_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, value);
            }
        }

        public Brush SelectionForeground
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (Brush)n;
            }
            set
            {
                //if (rtb_Main.Selection.IsEmpty)
                //{
                //    DependencyObject se = rtb_Main.Selection.Start.Parent;
                //    Run r = new Run("", rtb_Main.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.Foreground = value;
                //}
                //else
                //{

                //}

                rtb_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, value);
            }
        }




        public TextMarkerStyle SelectionMarkerStyle
        {
            get
            {
                Paragraph startParagraph = rtb.Selection.Start.Paragraph;
                Paragraph endParagraph = rtb.Selection.End.Paragraph;
                if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
                {
                    TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                    return markerStyle;
                }
                return TextMarkerStyle.None;
            }

        }

        public FontStyle SelectionFontStyle
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(TextElement.FontStyleProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return FontStyles.Normal;
                }
                return (FontStyle)n;
            }
            set
            {
                //if (value == SelectionFontStyle) return;
                //if (rtb_Main.Selection.IsEmpty)
                //{
                //    DependencyObject se = rtb_Main.Selection.Start.Parent;
                //    Run r = new Run("", rtb_Main.Selection.End);

                //    if (se is Run)
                //    {
                //        CloneRun((se as Run), r);
                //    }
                //    r.FontStyle = value;
                //}
                //else
                //{
                //}

                rtb_Main.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, value);
            }
        }


        public TextAlignment SelectionAligment
        {
            get
            {
                object n = rtb_Main.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
                if (n == DependencyProperty.UnsetValue)
                {
                    return TextAlignment.Left;
                }
                return (TextAlignment)n;
            }
            set
            {
                switch (value)
                {
                    case TextAlignment.Left:
                        cmdExecute(EditingCommands.AlignLeft);
                        return;
                    case TextAlignment.Center:
                        cmdExecute(EditingCommands.AlignCenter);
                        return;
                    case TextAlignment.Right:
                        cmdExecute(EditingCommands.AlignRight);
                        return;
                    case TextAlignment.Justify:
                        cmdExecute(EditingCommands.AlignJustify);
                        return;
                }
            }
        }



        public double PageWidth
        {
            get { return rtb_Main.Document.PageWidth; }
            set { rtb_Main.Document.PageWidth = value; }
        }

     
        private void YATE_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            TextPointer tp = rtb_Main.GetPositionFromPoint(e.GetPosition(rtb_Main), false);
            if (tp != null && tp.Parent is DependencyObject)
            {
                Hyperlink h = TryFindParent<Hyperlink>(tp.Parent as DependencyObject);
                if (h != null && h.NavigateUri != null)
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo(h.NavigateUri.AbsoluteUri));
                }
            }
        }

        private void YATE_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextPointer tp = rtb_Main.GetPositionFromPoint(e.GetPosition(rtb_Main), false);
            if (tp == null)
            {
                image_helper.ClearImageResizers(rtb_Main);
                return;
            }

            if (tp.Parent is InlineUIContainer)
            {
                UIElement ue = (tp.Parent as InlineUIContainer).Child;
                if (ue == null) return;

                if (ue is Image)
                {
                    cmdSelect((tp.Parent as InlineUIContainer));
                    image_helper.ChangeImageResizers(rtb_Main, (ue as Image));
                    e.Handled = true;
                    return;
                }
            }

            if (tp.Parent is BlockUIContainer)
            {
                // Get the BlockUIContainer or InlilneUIContainer's  full
                // XAML markup.

                UIElement ue = (tp.Parent as BlockUIContainer).Child;
                if (ue == null) return;

                if (ue is Image)
                {
                    cmdSelect((tp.Parent as BlockUIContainer));

                    image_helper.ChangeImageResizers(rtb_Main, (ue as Image));
                    e.Handled = true;
                    return;
                }
            }
            image_helper.ClearImageResizers(rtb_Main);
        }

        #region Paste

        private bool _isBusy = false;
        public bool isBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy) return;
                _isBusy = value;
                if (_isBusy)
                {
                    progressGrid.Visibility = Visibility.Visible;
                   // rtb.IsEnabled = false;
                }
                else
                {

                    progressGrid.Visibility = Visibility.Collapsed;
                }

            }
        }

        public new bool Focus()
        {
            return rtb_Main.Focus();
        }

        /*Paste data like HTML*/
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            string[] formats = e.DataObject.GetFormats();


            for (int i = formats.Length - 1; i >= 0; i--)
            {
                string str = formats[i];
                switch (str)
                {
                    case "HTML Format":
                        if (e.DataObject.GetDataPresent(DataFormats.Html))
                        {

                                string html = (string)e.DataObject.GetData(DataFormats.Html);
                                Section s = HTMLConverter.HTMLToFlowConverter.ConvertHtmlToSection(html, rtb_Main.Document.PageWidth);
                                this.cmdInsertBlock(s,true);

                                Semagsoft.HyperlinkHelper.SubscribeToAllHyperlinks(rtb_Main.Document);
                                e.CancelCommand();
                                e.Handled = true;


                            return;
                        }
                        break;
                    case "Rich Text Format":
                        if (e.DataObject.GetDataPresent(DataFormats.Rtf))
                        {
                           
                            object o = e.DataObject.GetData(DataFormats.Rtf);
                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(o as string));
                            rtb.Selection.Load(ms, DataFormats.Rtf);
                            Semagsoft.HyperlinkHelper.SubscribeToAllHyperlinks(rtb_Main.Document);


                            Block b = null; Section s = null;
                            TextPointer insert = rtb.Selection.Start;
                            b = TryFindParent<Block>(insert.Parent as DependencyObject);
                            s = TryFindParent<Section>(insert.Parent as DependencyObject);
                            Paragraph newItem = new Paragraph(new Run(""));
                            if (s == null)
                            {
                                if (b != null) this.Document.Blocks.InsertAfter(b, newItem);
                                else this.Document.Blocks.Add(newItem);
                            }
                            else
                            {
                                if (b != null) s.Blocks.InsertAfter(b, newItem);
                                else s.Blocks.Add(newItem);
                            }
                            rtb_Main.Selection.Select(newItem.ContentStart, newItem.ContentStart);

                            e.CancelCommand();
                            e.Handled = true;
                            return;
                        }
                        break;
                    case "DeviceIndependentBitmap":
                    case "System.Windows.Media.Imaging.BitmapSource":
                    case "System.Drawing.Bitmap":
                    case "Bitmap":
                        if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
                        {

                                BitmapSource image = Clipboard.GetImage();
                                // = (BitmapSource)e.DataObject.GetData(DataFormats.Bitmap);
                                cmdInsertImage(image, ImageContentType.ImageJpegContentType);
                                e.CancelCommand();
                                e.Handled = true;

                            return;
                        }
                        break;
                    case "Text":
                        if (e.DataObject.GetDataPresent(DataFormats.Text))
                        {
                            string text = (string)e.DataObject.GetData(DataFormats.Text);
                            Uri uriResult;
                            string original = text;
                            if (YATEHelper.IsHyperlink(ref text, out uriResult))
                            {

                                Hyperlink h = new Hyperlink(rtb.Selection.Start, rtb.Selection.End);
                                h.Cursor = Cursors.Hand;
                                h.NavigateUri = uriResult;
                                h.Inlines.Add(new Run(original));
                                h.FontSize = 16;
                                // this.cmdInsertInline(h);

                            }
                            else
                            {
                                this.cmdInsertText(text);
                            }
                            e.CancelCommand();
                            e.Handled = true;


                            return;
                        }
                        break;
                }
            }
            #endregion Paste
        }

        public event ExecutedRoutedEventHandler PreviewCommandExecuted;


        private void rtb_Main_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (PreviewCommandExecuted != null)
            {
                PreviewCommandExecuted.Invoke(sender, e);
            }
        }

        private void rtb_ScrollChanged(object sender, RoutedEventArgs e)
        {
            UpdateAdorners();
        }
    }
}