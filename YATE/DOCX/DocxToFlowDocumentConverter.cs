using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml;

namespace DocxReaderApplication
{
    public class DocxToFlowDocumentConverter : DocxReader
    {
        private const string

            // Run properties elements
            BoldElement = "b",
            ItalicElement = "i",
            UnderlineElement = "u",
            StrikeElement = "strike",
            DoubleStrikeElement = "dstrike",
            VerticalAlignmentElement = "vertAlign",
            ColorElement = "color",
            HighlightElement = "highlight",
            FontElement = "rFonts",
            FontSizeElement = "sz",
            RightToLeftTextElement = "rtl",

            // Paragraph properties elements
            AlignmentElement = "jc",
            PageBreakBeforeElement = "pageBreakBefore",
            SpacingElement = "spacing",
            IndentationElement = "ind",
            ShadingElement = "shd",

            // Attributes
            IdAttribute = "id",
            ValueAttribute = "val",
            ColorAttribute = "color",
            AsciiFontFamily = "ascii",
            SpacingAfterAttribute = "after",
            SpacingBeforeAttribute = "before",
            LeftIndentationAttribute = "left",
            RightIndentationAttribute = "right",
            HangingIndentationAttribute = "hanging",
            FirstLineIndentationAttribute = "firstLine",
            FillAttribute = "fill";
        // Note: new members should also be added to nameTable in CreateNameTable method.

        private FlowDocument document;
        private TextElement current;
        private bool hasAnyHyperlink;

        public FlowDocument Document
        {
            get { return this.document; }
        }

        public DocxToFlowDocumentConverter(Stream stream)
            : base(stream)
        {
        }

        protected override XmlNameTable CreateNameTable()
        {
            var nameTable = base.CreateNameTable();

            nameTable.Add(BoldElement);
            nameTable.Add(ItalicElement);
            nameTable.Add(UnderlineElement);
            nameTable.Add(StrikeElement);
            nameTable.Add(DoubleStrikeElement);
            nameTable.Add(VerticalAlignmentElement);
            nameTable.Add(ColorElement);
            nameTable.Add(HighlightElement);
            nameTable.Add(FontElement);
            nameTable.Add(FontSizeElement);
            nameTable.Add(RightToLeftTextElement);
            nameTable.Add(AlignmentElement);
            nameTable.Add(PageBreakBeforeElement);
            nameTable.Add(SpacingElement);
            nameTable.Add(IndentationElement);
            nameTable.Add(ShadingElement);
            nameTable.Add(IdAttribute);
            nameTable.Add(ValueAttribute);
            nameTable.Add(ColorAttribute);
            nameTable.Add(AsciiFontFamily);
            nameTable.Add(SpacingAfterAttribute);
            nameTable.Add(SpacingBeforeAttribute);
            nameTable.Add(LeftIndentationAttribute);
            nameTable.Add(RightIndentationAttribute);
            nameTable.Add(HangingIndentationAttribute);
            nameTable.Add(FirstLineIndentationAttribute);
            nameTable.Add(FillAttribute);

            return nameTable;
        }

        protected override void ReadDocument(XmlReader reader)
        {
            this.document = new FlowDocument();
            this.document.BeginInit();
            this.document.ColumnWidth = double.NaN;

            base.ReadDocument(reader);

            if (this.hasAnyHyperlink)
                this.document.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler((sender, e) => Process.Start(e.Uri.ToString())));

            this.document.EndInit();
        }

        protected override void ReadParagraph(XmlReader reader)
        {
            using (this.SetCurrent(new Paragraph()))
                base.ReadParagraph(reader);
        }

        protected override void ReadTable(XmlReader reader)
        {
            // Skip table for now.
        }

        protected override void ReadParagraphProperties(XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == WordprocessingMLNamespace)
                {
                    var paragraph = (Paragraph)this.current;
                    switch (reader.LocalName)
                    {
                        case AlignmentElement:
                            var textAlignment = ConvertTextAlignment(GetValueAttribute(reader));
                            if(textAlignment.HasValue)
                                paragraph.TextAlignment = textAlignment.Value;
                            break;
                        case PageBreakBeforeElement:
                            paragraph.BreakPageBefore = GetOnOffValueAttribute(reader);
                            break;
                        case SpacingElement:
                            paragraph.Margin = GetSpacing(reader, paragraph.Margin);
                            break;
                        case IndentationElement:
                            SetParagraphIndent(reader, paragraph);
                            break;
                        case ShadingElement:
                            var background = GetShading(reader);
                            if (background != null)
                                paragraph.Background = background;
                            break;
                    }
                }
        }

        protected override void ReadHyperlink(XmlReader reader)
        {
            var id = reader[IdAttribute, RelationshipsNamespace];
            if (!string.IsNullOrEmpty(id))
            {
                var relationship = this.MainDocumentPart.GetRelationship(id);
                if (relationship.TargetMode == TargetMode.External)
                {
                    this.hasAnyHyperlink = true;

                    var hyperlink = new Hyperlink()
                    {
                        NavigateUri = relationship.TargetUri
                    };

                    using (this.SetCurrent(hyperlink))
                        base.ReadHyperlink(reader);
                    return;
                }
            }

            base.ReadHyperlink(reader);
        }

        protected override void ReadRun(XmlReader reader)
        {
            using (this.SetCurrent(new Span()))
                base.ReadRun(reader);
        }

        protected override void ReadRunProperties(XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.NamespaceURI == WordprocessingMLNamespace)
                {
                    var inline = (Inline)this.current;
                    switch (reader.LocalName)
                    {
                        case BoldElement:
                            inline.FontWeight = GetOnOffValueAttribute(reader) ? FontWeights.Bold : FontWeights.Normal;
                            break;
                        case ItalicElement:
                            inline.FontStyle = GetOnOffValueAttribute(reader) ? FontStyles.Italic : FontStyles.Normal;
                            break;
                        case UnderlineElement:
                            var underlineTextDecorations = GetUnderlineTextDecorations(reader, inline);
                            if (underlineTextDecorations != null)
                                inline.TextDecorations.Add(underlineTextDecorations);
                            break;
                        case StrikeElement:
                            if (GetOnOffValueAttribute(reader))
                                inline.TextDecorations.Add(TextDecorations.Strikethrough);
                            break;
                        case DoubleStrikeElement:
                            if (GetOnOffValueAttribute(reader))
                            {
                                inline.TextDecorations.Add(new TextDecoration() { Location = TextDecorationLocation.Strikethrough, PenOffset = this.current.FontSize * 0.015 });
                                inline.TextDecorations.Add(new TextDecoration() { Location = TextDecorationLocation.Strikethrough, PenOffset = this.current.FontSize * -0.015 });
                            }
                            break;
                        case VerticalAlignmentElement:
                            var baselineAlignment = GetBaselineAlignment(GetValueAttribute(reader));
                            if (baselineAlignment.HasValue)
                            {
                                inline.BaselineAlignment = baselineAlignment.Value;
                                if (baselineAlignment.Value == BaselineAlignment.Subscript || baselineAlignment.Value == BaselineAlignment.Superscript)
                                    inline.FontSize *= 0.65; //MS Word 2002 size: 65% http://en.wikipedia.org/wiki/Subscript_and_superscript
                            }
                            break;
                        case ColorElement:
                            var color = GetColor(GetValueAttribute(reader));
                            if (color.HasValue)
                                inline.Foreground = new SolidColorBrush(color.Value);
                            break;
                        case HighlightElement:
                            var highlight = GetHighlightColor(GetValueAttribute(reader));
                            if (highlight.HasValue)
                                inline.Background = new SolidColorBrush(highlight.Value);
                            break;
                        case FontElement:
                            var fontFamily = reader[AsciiFontFamily, WordprocessingMLNamespace];
                            if (!string.IsNullOrEmpty(fontFamily))
                                inline.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString(fontFamily);
                            break;
                        case FontSizeElement:
                            var fontSize = reader[ValueAttribute, WordprocessingMLNamespace];
                            if (!string.IsNullOrEmpty(fontSize))
                                // Attribute Value / 2 = Points
                                // Points * (96 / 72) = Pixels
                                inline.FontSize = uint.Parse(fontSize) * 0.6666666666666667;
                            break;
                        case RightToLeftTextElement:
                            inline.FlowDirection = (GetOnOffValueAttribute(reader)) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                            break;
                    }
                }
        }

        protected override void ReadBreak(XmlReader reader)
        {
            this.AddChild(new LineBreak());
        }

        protected override void ReadTabCharacter(XmlReader reader)
        {
            this.AddChild(new Run("\t"));
        }

        protected override void ReadText(XmlReader reader)
        {
            this.AddChild(new Run(reader.ReadString()));
        }

        private void AddChild(TextElement textElement)
        {
            ((IAddChild)this.current ?? this.document).AddChild(textElement);
        }

        private static bool GetOnOffValueAttribute(XmlReader reader)
        {
            var value = GetValueAttribute(reader);

            switch (value)
            {
                case null:
                case "1":
                case "on":
                case "true":
                    return true;
                default:
                    return false;
            }
        }

        private static string GetValueAttribute(XmlReader reader)
        {
            return reader[ValueAttribute, WordprocessingMLNamespace];
        }

        private static Color? GetColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString == "auto")
                return null;

            return (Color)ColorConverter.ConvertFromString('#' + colorString);
        }

        private static Color? GetHighlightColor(string highlightString)
        {
            if (string.IsNullOrEmpty(highlightString) || highlightString == "auto")
                return null;

            return (Color)ColorConverter.ConvertFromString(highlightString);
        }

        private static BaselineAlignment? GetBaselineAlignment(string verticalAlignmentString)
        {
            switch (verticalAlignmentString)
            {
                case "baseline":
                    return BaselineAlignment.Baseline;
                case "subscript":
                    return BaselineAlignment.Subscript;
                case "superscript":
                    return BaselineAlignment.Superscript;
                default:
                    return null;
            }
        }

        private static double? ConvertTwipsToPixels(string twips)
        {
            if (string.IsNullOrEmpty(twips))
                return null;
            else
                return ConvertTwipsToPixels(double.Parse(twips, CultureInfo.InvariantCulture));
        }

        private static double ConvertTwipsToPixels(double twips)
        {
            return 96d / (72 * 20) * twips;
        }

        private static TextAlignment? ConvertTextAlignment(string value)
        {
            switch (value)
            {
                case "both":
                    return TextAlignment.Justify;
                case "left":
                    return TextAlignment.Left;
                case "right":
                    return TextAlignment.Right;
                case "center":
                    return TextAlignment.Center;
                default:
                    return null;
            }
        }

        private static Thickness GetSpacing(XmlReader reader, Thickness margin)
        {
            var after = ConvertTwipsToPixels(reader[SpacingAfterAttribute, WordprocessingMLNamespace]);
            if (after.HasValue)
                margin.Bottom = after.Value;

            var before = ConvertTwipsToPixels(reader[SpacingBeforeAttribute, WordprocessingMLNamespace]);
            if (before.HasValue)
                margin.Top = before.Value;

            return margin;
        }

        private static void SetParagraphIndent(XmlReader reader, Paragraph paragraph)
        {
            var margin = paragraph.Margin;

            var left = ConvertTwipsToPixels(reader[LeftIndentationAttribute, WordprocessingMLNamespace]);
            if (left.HasValue)
                margin.Left = left.Value;

            var right = ConvertTwipsToPixels(reader[RightIndentationAttribute, WordprocessingMLNamespace]);
            if (right.HasValue)
                margin.Right = right.Value;

            paragraph.Margin = margin;

            var firstLine = ConvertTwipsToPixels(reader[FirstLineIndentationAttribute, WordprocessingMLNamespace]);
            if (firstLine.HasValue)
                paragraph.TextIndent = firstLine.Value;

            var hanging = ConvertTwipsToPixels(reader[HangingIndentationAttribute, WordprocessingMLNamespace]);
            if (hanging.HasValue)
                paragraph.TextIndent -= hanging.Value;
        }

        private static Brush GetShading(XmlReader reader)
        {
            var color = GetColor(reader[FillAttribute, WordprocessingMLNamespace]);
            return color.HasValue ? new SolidColorBrush(color.Value) : null;
        }

        private static TextDecorationCollection GetUnderlineTextDecorations(XmlReader reader, Inline inline)
        {
            TextDecoration textDecoration;
            Brush brush;
            var color = GetColor(reader[ColorAttribute, WordprocessingMLNamespace]);

            if (color.HasValue)
                brush = new SolidColorBrush(color.Value);
            else
                brush = inline.Foreground;

            var textDecorations = new TextDecorationCollection()
            {
                (textDecoration = new TextDecoration()
                {
                    Location = TextDecorationLocation.Underline,
                    Pen = new Pen()
                    {
                        Brush = brush
                    }
                })
            };

            switch (GetValueAttribute(reader))
            {
                case "single":
                    break;
                case "double":
                    textDecoration.PenOffset = inline.FontSize * 0.05;
                    textDecoration = textDecoration.Clone();
                    textDecoration.PenOffset = inline.FontSize * -0.05;
                    textDecorations.Add(textDecoration);
                    break;
                case "dotted":
                    textDecoration.Pen.DashStyle = DashStyles.Dot;
                    break;
                case "dash":
                    textDecoration.Pen.DashStyle = DashStyles.Dash;
                    break;
                case "dotDash":
                    textDecoration.Pen.DashStyle = DashStyles.DashDot;
                    break;
                case "dotDotDash":
                    textDecoration.Pen.DashStyle = DashStyles.DashDotDot;
                    break;
                case "none":
                default:
                    // If underline type is none or unsupported then it will be ignored.
                    return null;
            }

            return textDecorations;
        }

        private IDisposable SetCurrent(TextElement current)
        {
            return new CurrentHandle(this, current);
        }

        private struct CurrentHandle : IDisposable
        {
            private readonly DocxToFlowDocumentConverter converter;
            private readonly TextElement previous;

            public CurrentHandle(DocxToFlowDocumentConverter converter, TextElement current)
            {
                this.converter = converter;
                this.converter.AddChild(current);
                this.previous = this.converter.current;
                this.converter.current = current;
            }

            public void Dispose()
            {
                this.converter.current = this.previous;
            }
        }
    }
}