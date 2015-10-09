using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;

namespace YATE
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public class PrintPreview : DocumentViewer
    {
        public PrintPreview()
        {
        //   DefaultStyleKeyProperty.OverrideMetadata(typeof(PrintPreview), new FrameworkPropertyMetadata(typeof(DocumentViewer)));
           Loaded += new RoutedEventHandler(PrintDocumentViewer_Loaded);
        }


        PageOrientation _pageOrientation = PageOrientation.Portrait;
        public PageOrientation PageOrientation
        {
            get { return _pageOrientation; }
            set { _pageOrientation = value; }
        }

        Visibility _findControlVisibility = Visibility.Collapsed;
        public Visibility FindControlVisibility
        {
            get
            {
                return _findControlVisibility;
            }
            set
            {
                _findControlVisibility = value;
                UpdateFindControlVisibility();
            }
        }

        private void UpdateFindControlVisibility()
        {
                object toolbar = this.Template.FindName("PART_FindToolBarHost", this as DocumentViewer);
      //      object toolbar  =  this.FindName("PART_FindToolBarHost");

            ContentControl cc = toolbar as ContentControl;
            if (cc != null)
            {
                HeaderedItemsControl itemsControl = cc.Content as HeaderedItemsControl;
                if (itemsControl != null)
                    itemsControl.Visibility = FindControlVisibility;
            }
        }

      

        void PrintDocumentViewer_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFindControlVisibility();
        }

        protected override void OnPrintCommand()
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            printDialog.PrintTicket = printDialog.PrintQueue.DefaultPrintTicket;

            printDialog.PrintTicket.PageOrientation = PageOrientation;

            if (printDialog.ShowDialog() == true)
            {
                // Code assumes this.Document will either by a FixedDocument or a FixedDocumentSequence
                FixedDocument fixedDocument = this.Document as FixedDocument;
                FixedDocumentSequence fixedDocumentSequence = this.Document as FixedDocumentSequence;

                if (fixedDocument != null)
                    fixedDocument.PrintTicket = printDialog.PrintTicket;

                if (fixedDocumentSequence != null)
                    fixedDocumentSequence.PrintTicket = printDialog.PrintTicket;

                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);

                if (fixedDocument != null)
                    writer.WriteAsync(fixedDocument, printDialog.PrintTicket);

                if (fixedDocumentSequence != null)
                    writer.WriteAsync(fixedDocumentSequence, printDialog.PrintTicket);
            }
        }
    }





    public class PrintLayout
    {
        public static readonly PrintLayout A4 = new PrintLayout("21cm", "29.7cm", "3.18cm", "2.54cm");
        public static readonly PrintLayout A4Narrow = new PrintLayout("21cm", "29.7cm", "1.27cm", "1.27cm");
        public static readonly PrintLayout A4Moderate = new PrintLayout("21cm", "29.7cm", "1.91cm", "2.54cm");

        private Size _Size;
        private Thickness _Margin;

        public PrintLayout(string w, string h, string leftright, string topbottom)
            : this(w, h, leftright, topbottom, leftright, topbottom)
        {
        }

        public PrintLayout(string w, string h, string left, string top, string right, string bottom)
        {
            var converter = new LengthConverter();
            var width = (double)converter.ConvertFromInvariantString(w);
            var height = (double)converter.ConvertFromInvariantString(h);
            var marginLeft = (double)converter.ConvertFromInvariantString(left);
            var marginTop = (double)converter.ConvertFromInvariantString(top);
            var marginRight = (double)converter.ConvertFromInvariantString(right);
            var marginBottom = (double)converter.ConvertFromInvariantString(bottom);
            this._Size = new Size(width, height);
            this._Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);

        }


        public Thickness Margin
        {
            get { return _Margin; }
            set { _Margin = value; }
        }

        public Size Size
        {
            get { return _Size; }
        }

        public double ColumnWidth
        {
            get
            {
                var column = 0.0;
                column = this.Size.Width - Margin.Left - Margin.Right;
                return column;
            }
        }
    }
}
