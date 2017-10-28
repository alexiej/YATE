//---------------------------------------------------------------------------
// 
// File: HtmlXamlConverter.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Prototype for Html - Xaml conversion 
//
//---------------------------------------------------------------------------

namespace HTMLConverter
{
    using System;
    using System.Xml;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    using System.Windows; // DependencyProperty
    using System.Windows.Documents; // TextElement
    using System.Windows.Media;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DocEdLib;
    using System.Net;

    /// <summary>
    public partial class HTMLToFlowConverter
    {


        /// <summary>
        /// Returns a value for an attribute by its name (ignoring casing)
        /// </summary>
        /// <param name="element">
        /// XmlElement in which we are trying to find the specified attribute
        /// </param>
        /// <param name="attributeName">
        /// String representing the attribute name to be searched for
        /// </param>
        /// <returns></returns>
        public static string GetAttribute(XmlElement element, string attributeName)
        {
            attributeName = attributeName.ToLower();

            for (int i = 0; i < element.Attributes.Count; i++)
            {
                if (element.Attributes[i].Name.ToLower() == attributeName)
                {
                    return element.Attributes[i].Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns string extracted from quotation marks
        /// </summary>
        /// <param name="value">
        /// String representing value enclosed in quotation marks
        /// </param>
        internal static string UnQuote(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\"") || value.StartsWith("'") && value.EndsWith("'"))
            {
                value = value.Substring(1, value.Length - 2).Trim();
            }
            return value;
        }

        // .............................................................
        //
        // Attributes and Properties
        //
        // .............................................................

        /// <summary>
        /// Analyzes local properties of Html element, converts them into Xaml equivalents, and applies them to xamlElement
        /// </summary>
        /// <param name="xamlElement">
        /// XmlElement representing Xaml element to which properties are to be applied
        /// </param>
        /// <param name="localProperties">
        /// Hashtable representing local properties of Html element that is converted into xamlElement
        /// </param>
        private static void ApplyLocalProperties(TextElement element, Hashtable localProperties, bool isBlock)
        {
            double marginTop = 0;
            double marginBottom = 0;
            double marginLeft = 0;
            double marginRight = 0;

            double paddingTop = 0;
            double paddingBottom = 0;
            double paddingLeft = 0;
            double paddingRight = 0;

            bool borderColorSet = false;
            Brush borderColor = null;
            bool marginSet = false;
            bool borderThicknessSet = false;
            bool paddingSet = false;

            double borderThicknessTop = 0;
            double borderThicknessBottom = 0;
            double borderThicknessLeft = 0;
            double borderThicknessRight = 0;

            IDictionaryEnumerator propertyEnumerator = localProperties.GetEnumerator();
            while (propertyEnumerator.MoveNext())
            {
                switch ((string)propertyEnumerator.Key)
                {
                    case "text-decoration-underline":
                        {
                            if (element is Paragraph)
                            {
                                Paragraph p = ((Paragraph)element);
                                if (p.TextDecorations == null) { p.TextDecorations = new TextDecorationCollection(); }
                                p.TextDecorations.Add(TextDecorations.Underline);
                            }
                            else if (element is Inline)
                            {
                                Inline i = (Inline)element;
                                if (i.TextDecorations == null) { i.TextDecorations = new TextDecorationCollection(); }
                                i.TextDecorations.Add(TextDecorations.Underline);
                            }
                        }
                        break;
                    //  TextDecoration()
                    case "font-family":
                        //  Convert from font-family value list into xaml FontFamily value
                        // DISABLE FONT-FAMILY
                        //   xamlElement.SetAttribute(Xaml_FontFamily, (string)propertyEnumerator.Value);
                        //     element.FontFamily = FontFa
                        element.FontFamily = (FontFamily)propertyEnumerator.Value;
                        break;
                    case "font-style":
                        //  xamlElement.SetAttribute(Xaml_FontStyle,);
                        {
                            element.FontStyle = (FontStyle)propertyEnumerator.Value;
                        }
                        break;
                    case "font-variant":
                        //  Convert from font-variant into xaml property
                        break;
                    case "font-weight":
                        {
                            // xamlElement.SetAttribute(Xaml_FontWeight, (string)propertyEnumerator.Value);
                            element.FontWeight = (FontWeight)propertyEnumerator.Value;
                            //string ls = ((string)propertyEnumerator.Value).ToLower();
                            //switch (ls)
                            //{
                            //    case "normal":
                            //       = FontWeights.Normal;

                            //        break;
                            //    case "bold":
                            //        element.FontWeight = FontWeights.Bold;

                            //        break;
                            //    case "bolder":
                            //        element.FontWeight = FontWeights.ExtraBold;
                            //        break;
                            //    case "lighter":
                            //        element.FontWeight = FontWeights.Light;

                            //        break;
                            //    case "100":
                            //        element.FontWeight = FontWeights.Thin;

                            //        break;
                            //    case "200":
                            //        element.FontWeight = FontWeights.ExtraLight;

                            //        break;
                            //    case "300":
                            //        element.FontWeight = FontWeights.Light;

                            //        break;
                            //    case "400":
                            //        element.FontWeight = FontWeights.Normal;

                            //        break;

                            //    case "500":
                            //        element.FontWeight = FontWeights.Medium;

                            //        break;
                            //    case "600":
                            //        element.FontWeight = FontWeights.DemiBold;

                            //        break;
                            //    case "700":
                            //        element.FontWeight = FontWeights.ExtraBold;

                            //        break;
                            //    case "800":
                            //        element.FontWeight = FontWeights.Black;
                            //        break;
                            //    case "900":
                            //        element.FontWeight = FontWeights.ExtraBlack;

                            //        break;
                            //}
                        }
                        break;
                    case "font-size":
                        double v = (double)

                            Convert.ToDouble(
                            propertyEnumerator.Value);

                        if (v > 0)
                        {


                            element.FontSize = v;
                        }
                        //  Convert from css size into FontSize
                        /* DISABLE FONT-SIZE
                        double length = 0;
                        if (!TryGetLengthValue((string)propertyEnumerator.Value, out length))
                        {
                            TryGetLengthValue(Xaml_FontSize_Medium, out length);
                        }
                        xamlElement.SetAttribute(Xaml_FontSize, length.ToString());
                         */
                        break;
                    case "color":
                        element.Foreground = (Brush)propertyEnumerator.Value;
                        //  SetPropertyValue(xamlElement, TextElement.ForegroundProperty, (string)propertyEnumerator.Value);
                        break;
                    case "background-color":
                        element.Background = (Brush)propertyEnumerator.Value;
                        //   SetPropertyValue(xamlElement, TextElement.BackgroundProperty, (string)propertyEnumerator.Value);
                        break;
                    //case "text-decoration-underline":
                    //    if (!isBlock)
                    //    {
                    //        //Paragraph p = new Paragraph();
                    //        // p.TextDecorations = TextDecorations.Underline;
                    //        //element.TextEffects = 
                    //        //element.FontStyle |= FontStyles.Oblique = (Brush)propertyEnumerator.Value;
                    //        //if ((string)propertyEnumerator.Value == "true")
                    //        //{
                    //        //    // DISABLE UNDERLINES
                    //        //    // xamlElement.SetAttribute(Xaml_TextDecorations, Xaml_TextDecorations_Underline);
                    //        //}
                    //    }
                    //    break;
                    //case "text-decoration-none":
                    //case "text-decoration-overline":
                    //case "text-decoration-line-through":
                    //case "text-decoration-blink":
                    //    //  Convert from all other text-decorations values
                    //    if (!isBlock)
                    //    {
                    //    }
                    //    break;
                    case "text-transform":
                        //  Convert from text-transform into xaml property
                        break;

                    case "text-indent":
                        if (isBlock)
                        {
                            // DISABLE TEXT-INDENT
                            // xamlElement.SetAttribute(Xaml_TextIndent, (string)propertyEnumerator.Value);
                        }
                        break;

                    case "text-align":
                        if (isBlock)
                        {
                            //  xamlElement.SetAttribute(Xaml_TextAlignment, (string)propertyEnumerator.Value);
                        }
                        break;

                    case "width":
                        //if (xamlElement.Name == Xaml_Image)
                        //{
                        //    double width = double.PositiveInfinity;
                        //    if (TryGetLengthValue((string)propertyEnumerator.Value, out width))
                        //    {
                        //        xamlElement.SetAttribute(Xaml_MaxWidth, width.ToString());
                        //    }
                        //}
                        break;

                    case "height":
                        //if (xamlElement.Name == Xaml_Image)
                        //{
                        //    double Height = double.PositiveInfinity;
                        //    if (TryGetLengthValue((string)propertyEnumerator.Value, out Height))
                        //    {
                        //        xamlElement.SetAttribute(Xaml_MaxHeight, Height.ToString());
                        //    }
                        //}
                        break;

                    case "src":
                        //   xamlElement.SetAttribute(Xaml_Source, (string)propertyEnumerator.Value);
                        break;
                    case "margin-top":
                        marginSet = true;
                        marginTop = (double)propertyEnumerator.Value;
                        break;
                    case "margin-right":
                        marginSet = true;
                        marginRight = (double)propertyEnumerator.Value;
                        break;
                    case "margin-bottom":
                        marginSet = true;
                        marginBottom = (double)propertyEnumerator.Value;
                        break;
                    case "margin-left":
                        marginSet = true;
                        marginLeft = (double)propertyEnumerator.Value;
                        break;

                    case "padding-top":
                        paddingSet = true;
                        paddingTop = (double)propertyEnumerator.Value;
                        break;
                    case "padding-right":
                        paddingSet = true;
                        paddingRight = (double)propertyEnumerator.Value;
                        break;
                    case "padding-bottom":
                        paddingSet = true;
                        paddingBottom = (double)propertyEnumerator.Value;
                        break;
                    case "padding-left":
                        paddingSet = true;
                        paddingLeft = (double)propertyEnumerator.Value;
                        break;

                    // NOTE: css names for elementary border styles have side indications in the middle (top/bottom/left/right) id:320 gh:321
                    // In our internal notation we intentionally put them at the end - to unify processing in ParseCssRectangleProperty method
                    case "border-color-top":
                        borderColorSet = true;
                        borderColor = (Brush)propertyEnumerator.Value;
                        break;
                    case "border-color-right":
                        borderColorSet = true;

                        borderColor = (Brush)propertyEnumerator.Value;
                        break;
                    case "border-color-bottom":
                        borderColorSet = true;

                        borderColor = (Brush)propertyEnumerator.Value;
                        break;
                    case "border-color-left":
                        borderColorSet = true;
                        borderColor = (Brush)propertyEnumerator.Value;
                        break;
                    case "border-style-top":
                    case "border-style-right":
                    case "border-style-bottom":
                    case "border-style-left":
                        //  Implement conversion from border style
                        break;
                    case "border-width-top":
                        borderThicknessSet = true;
                        borderThicknessTop = (double)propertyEnumerator.Value;
                        break;
                    case "border-width-right":
                        borderThicknessSet = true;
                        borderThicknessRight = (double)propertyEnumerator.Value;
                        break;
                    case "border-width-bottom":
                        borderThicknessSet = true;
                        borderThicknessBottom = (double)propertyEnumerator.Value;
                        break;
                    case "border-width-left":
                        borderThicknessSet = true;
                        borderThicknessLeft = (double)propertyEnumerator.Value;
                        break;
                    case "list-style-type":
                        //if (xamlElement.LocalName == Xaml_List)
                        //{
                        //    string markerStyle;
                        //    switch (((string)propertyEnumerator.Value).ToLower())
                        //    {
                        //        case "disc":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Disc;
                        //            break;
                        //        case "circle":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Circle;
                        //            break;
                        //        case "none":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_None;
                        //            break;
                        //        case "square":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Square;
                        //            break;
                        //        case "box":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Box;
                        //            break;
                        //        case "lower-latin":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_LowerLatin;
                        //            break;
                        //        case "upper-latin":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_UpperLatin;
                        //            break;
                        //        case "lower-roman":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_LowerRoman;
                        //            break;
                        //        case "upper-roman":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_UpperRoman;
                        //            break;
                        //        case "decimal":
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Decimal;
                        //            break;
                        //        default:
                        //            markerStyle = HtmlToXamlConverter.Xaml_List_MarkerStyle_Disc;
                        //            break;
                        //    }
                        //    xamlElement.SetAttribute(HtmlToXamlConverter.Xaml_List_MarkerStyle, markerStyle);
                        //}
                        break;

                    case "float":
                    case "clear":
                        if (isBlock)
                        {
                            //  Convert float and clear properties
                        }
                        break;
                    case "display":
                        break;
                }
            }

            if (isBlock && element is Block)
            {
                Block b = (Block)element;

                if (marginSet)
                {
                    b.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
                }
                if (paddingSet)
                {
                    b.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);
                }
                if (borderThicknessSet)
                {
                    b.BorderThickness = new Thickness(borderThicknessLeft, borderThicknessTop, borderThicknessRight, borderThicknessBottom);
                }
                if (borderColorSet)
                {
                    b.BorderBrush = borderColor;
                }
            }
        }





        /// <summary>
        /// Analyzes the tag of the htmlElement and infers its associated formatted properties.
        /// After that parses style attribute and adds all inline css styles.
        /// The resulting style attributes are collected in output parameter localProperties.
        /// </summary>
        /// <param name="htmlElement">
        /// </param>
        /// <param name="inheritedProperties">
        /// set of properties inherited from ancestor elements. Currently not used in the code. Reserved for the future development.
        /// </param>
        /// <param name="localProperties">
        /// returns all formatting properties defined by this element - implied by its tag, its attributes, or its css inline style
        /// </param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        /// <returns>
        /// returns a combination of previous context with local set of properties.
        /// This value is not used in the current code - inntended for the future development.
        /// </returns>
        private void GetElementProperties(XmlElement htmlElement, Hashtable inheritedProperties, Hashtable localProperties, CssStylesheet stylesheet)
        {
            // Start with context formatting properties
            //Hashtable currentProperties = new Hashtable();
            //IDictionaryEnumerator propertyEnumerator = inheritedProperties.GetEnumerator();
            //while (propertyEnumerator.MoveNext())
            //{
            //    currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
            //}
            // Identify element name
            string elementName = htmlElement.LocalName.ToLower();
            string elementNamespace = htmlElement.NamespaceURI;
            // update current formatting properties depending on element tag
            //  localProperties = new Hashtable();

            // System.Drawing.Image

            switch (elementName)
            {
                // Character formatting
                case "i":
                case "italic":
                case "em":
                    localProperties["font-style"] = FontStyles.Italic;
                    break;
                case "b":
                case "bold":
                case "strong":
                case "dfn":
                    localProperties["font-weight"] = FontWeights.Bold;
                    break;
                case "u":
                case "underline":
                    localProperties["text-decoration-underline"] = true;
                    break;
                case "font":
                    /*
                    // DISABLE FONT ATTRIBUTES
                    string attributeValue = GetAttribute(htmlElement, "face");
                    if (attributeValue != null)
                    {
                        localProperties["font-family"] = attributeValue;
                    }
                    attributeValue = GetAttribute(htmlElement, "size");
                    if (attributeValue != null)
                    {
                        double fontSize = double.Parse(attributeValue) * (12.0 / 3.0);
                        if (fontSize < 1.0)
                        {
                            fontSize = 1.0;
                        }
                        else if (fontSize > 1000.0)
                        {
                            fontSize = 1000.0;
                        }
                        localProperties["font-size"] = fontSize.ToString();
                    }
                                        string attributeValue = GetAttribute(htmlElement, "color");
                    if (attributeValue != null)
                    {
                        localProperties["color"] = attributeValue;
                    } */
                    break;
                case "samp":
                    localProperties["font-family"] = new FontFamily("Courier New"); // code sample
                    localProperties["font-size"] = Xaml_FontSize_XXSmall;
                    localProperties["text-align"] = TextAlignment.Left;
                    break;
                case "sub":
                    break;
                case "sup":
                    break;
                // Hyperlinks
                case "a": // href, hreflang, urn, methods, rel, rev, title
                    //  Set default hyperlink properties
                    break;
                case "acronym":
                    break;
                // Paragraph formatting:
                case "p":
                    //  Set default paragraph properties
                    break;
                case "div":
                    //  Set default div properties
                    break;
                case "pre":
                    localProperties["font-family"] = new FontFamily("Courier New");// "Courier New"; // renders text in a fixed-width font
                    localProperties["font-size"] = Xaml_FontSize_XXSmall;
                    //  t.T
                    localProperties["text-align"] = TextAlignment.Left;
                    break;
                case "blockquote":
                    localProperties["margin-left"] = (double)16;
                    break;

                case "h1":
                    localProperties["font-size"] = Xaml_FontSize_XXLarge;
                    break;
                case "h2":
                    localProperties["font-size"] = Xaml_FontSize_XLarge;
                    break;
                case "h3":
                    localProperties["font-size"] = Xaml_FontSize_Large;
                    break;
                case "h4":
                    localProperties["font-size"] = Xaml_FontSize_Medium;
                    break;
                case "h5":
                    localProperties["font-size"] = Xaml_FontSize_Small;
                    break;
                case "h6":
                    localProperties["font-size"] = Xaml_FontSize_XSmall;
                    break;
                // List properties
                case "ul":
                    localProperties["list-style-type"] = "disc";
                    break;
                case "ol":
                    localProperties["list-style-type"] = "decimal";
                    break;
                case "img":
                    //localProperties["src"] = GetAttribute(htmlElement, "src");
                    //localProperties["width"] = GetAttribute(htmlElement, "width");
                    //localProperties["height"] = GetAttribute(htmlElement, "height");
                    break;
                case "table":
                case "body":
                case "html":
                    break;
            }
            // Override html defaults by css attributes - from stylesheets and inline settings
            HtmlCssParser.GetElementPropertiesFromCssAttributes(htmlElement, elementName, stylesheet, localProperties, sourceContext);

            //// Combine local properties with context to create new current properties
            //propertyEnumerator = localProperties.GetEnumerator();
            //while (propertyEnumerator.MoveNext())
            //{
            //    currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
            //}
            return;
        }



        /// <summary>
        /// Converts a length value from string representation to a double.
        /// </summary>
        /// <param name="lengthAsString">
        /// Source string value of a length.
        /// </param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static bool TryGetLengthValue(string lengthAsString, out double length)
        {
            length = Double.NaN;

            if (lengthAsString != null)
            {
                lengthAsString = lengthAsString.Trim().ToLower();

                // We try to convert currentColumnWidthAsString into a double. This will eliminate widths of type "50%", etc.
                if (lengthAsString.EndsWith("pt"))
                {
                    lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
                    if (Double.TryParse(lengthAsString, out length))
                    {
                        length = (length * 96.0) / 72.0; // convert from points to pixels
                    }
                    else
                    {
                        length = Double.NaN;
                    }
                }
                else if (lengthAsString.EndsWith("px"))
                {
                    lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 2);
                    if (!Double.TryParse(lengthAsString, out length))
                    {
                        length = Double.NaN;
                    }
                }
                else if (lengthAsString.EndsWith("%"))
                {
                    lengthAsString = lengthAsString.Substring(0, lengthAsString.Length - 1);
                    if (!Double.TryParse(lengthAsString, out length))
                    {
                        length = Double.NaN;
                    }
                    double medium = 0;
                    TryGetLengthValue("16pt", out medium);
                    length = medium * (length / 100);
                }
                else
                {
                    if (!Double.TryParse(lengthAsString, out length)) // Assuming pixels
                    {
                        length = Double.NaN;
                    }
                }
            }

            return !Double.IsNaN(length);
        }

        // ----------------------------------------------------------------
        //
        // Internal Constants
        //
        // ----------------------------------------------------------------

        // The constants reprtesent all Xaml names used in a conversion
        public const string Xaml_FlowDocument = "FlowDocument";

        public const string Xaml_Run = "Run";
        public const string Xaml_Span = "Span";
        public const string Xaml_Hyperlink = "Hyperlink";
        public const string Xaml_Hyperlink_NavigateUri = "NavigateUri";
        public const string Xaml_Hyperlink_TargetName = "TargetName";

        public const string Xaml_Section = "Section";

        public const string Xaml_List = "List";

        public const string Xaml_List_MarkerStyle = "MarkerStyle";
        public const string Xaml_List_MarkerStyle_None = "None";
        public const string Xaml_List_MarkerStyle_Decimal = "Decimal";
        public const string Xaml_List_MarkerStyle_Disc = "Disc";
        public const string Xaml_List_MarkerStyle_Circle = "Circle";
        public const string Xaml_List_MarkerStyle_Square = "Square";
        public const string Xaml_List_MarkerStyle_Box = "Box";
        public const string Xaml_List_MarkerStyle_LowerLatin = "LowerLatin";
        public const string Xaml_List_MarkerStyle_UpperLatin = "UpperLatin";
        public const string Xaml_List_MarkerStyle_LowerRoman = "LowerRoman";
        public const string Xaml_List_MarkerStyle_UpperRoman = "UpperRoman";

        public const string Xaml_ListItem = "ListItem";

        public const string Xaml_LineBreak = "LineBreak";

        public const string Xaml_Paragraph = "Paragraph";
        public const string Xaml_Image = "Image";

        public const string Xaml_Margin = "Margin";
        public const string Xaml_Padding = "Padding";
        public const string Xaml_BorderBrush = "BorderBrush";
        public const string Xaml_BorderThickness = "BorderThickness";

        public const string Xaml_Table = "Table";

        public const string Xaml_TableColumn = "TableColumn";
        public const string Xaml_TableRowGroup = "TableRowGroup";
        public const string Xaml_TableRow = "TableRow";

        public const string Xaml_TableCell = "TableCell";
        public const string Xaml_TableCell_BorderThickness = "BorderThickness";
        public const string Xaml_TableCell_BorderBrush = "BorderBrush";

        public const string Xaml_TableCell_ColumnSpan = "ColumnSpan";
        public const string Xaml_TableCell_RowSpan = "RowSpan";

        public const string Xaml_Width = "Width";
        public const string Xaml_Height = "Height";
        public const string Xaml_MaxWidth = "MaxWidth";
        public const string Xaml_MaxHeight = "MaxHeight";

        public const string Xaml_Brushes_Black = "Black";
        public const string Xaml_FontFamily = "FontFamily";

        public const string Xaml_Source = "Source";
        public const string Xaml_FontSize = "FontSize";

        public const double Xaml_FontSize_XXLarge = 22 * (96.0 / 72.0);// "22pt"; // "XXLarge";
        public const double Xaml_FontSize_XLarge = 20 * (96.0 / 72.0);// "20pt"; // "XLarge";
        public const double Xaml_FontSize_Large = 18 * (96.0 / 72.0);//"18pt"; // "Large";
        public const double Xaml_FontSize_Medium = 16 * (96.0 / 72.0);//"16pt"; // "Medium";
        public const double Xaml_FontSize_Small = 12 * (96.0 / 72.0);// "12pt"; // "Small";
        public const double Xaml_FontSize_XSmall = 10 * (96.0 / 72.0);//"10pt"; // "XSmall";
        public const double Xaml_FontSize_XXSmall = 8 * (96.0 / 72.0);// "8pt"; // "XXSmall";

        public const string Xaml_FontWeight = "FontWeight";
        public const string Xaml_FontWeight_Bold = "Bold";

        public const string Xaml_FontStyle = "FontStyle";

        public const string Xaml_Foreground = "Foreground";
        public const string Xaml_Background = "Background";
        public const string Xaml_TextDecorations = "TextDecorations";
        public const string Xaml_TextDecorations_Underline = "Underline";

        public const string Xaml_TextIndent = "TextIndent";
        public const string Xaml_TextAlignment = "TextAlignment";

    }
}
