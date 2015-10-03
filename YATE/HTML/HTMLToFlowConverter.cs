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
    public class HTMLToFlowConverter
    {
        
        public static string ConvertFlowToXAML(Section s)
        {
          return  System.Windows.Markup.XamlWriter.Save(s);
        }

        /// <summary>
        /// Converts an html string into FlowDocument string.
        /// </summary>
        /// <param name="htmlString">
        /// Input html which may be badly formated xml.
        /// </param>
        /// <param name="asFlowDocument">
        /// true indicates that we need a FlowDocument as a root element;
        /// false means that Section or Span elements will be used
        /// dependeing on StartFragment/EndFragment comments locations.
        /// </param>
        /// <returns>
        /// Well-formed xml representing XAML equivalent for the input html string.
        /// </returns>
        public static Section ConvertHtmlToSection(string htmlString,double max_image_width =  -1)
        {
            HTMLToFlowConverter conv = new HTMLToFlowConverter();
            conv.max_image_width = max_image_width;
            conv.ConvertHTML(htmlString);
            //// Decide what name to use as a root
            //string rootElementName = asFlowDocument ? HtmlToXamlConverter.Xaml_FlowDocument : HtmlToXamlConverter.Xaml_Section;

            //// Create an XmlDocument for generated xaml
            //XmlDocument xamlTree = new XmlDocument();
            //XmlElement xamlFlowDocumentElement = xamlTree.CreateElement(null, rootElementName, _xamlNamespace);
            //// Source context is a stack of all elements - ancestors of a parentElement
            //List<XmlElement> sourceContext = new List<XmlElement>(10);

            //// Clear fragment parent
            //InlineFragmentParentElement = null;

            //// convert root html element
            //AddBlock(xamlFlowDocumentElement, htmlElement, new Hashtable(), stylesheet, sourceContext);

            //// In case if the selected fragment is inline, extract it into a separate Span wrapper
            //if (!asFlowDocument)
            //{
            //    xamlFlowDocumentElement = ExtractInlineFragment(xamlFlowDocumentElement);
            //}

            // Return a string representing resulting Xaml
        //    xamlFlowDocumentElement.SetAttribute("xml:space", "preserve");
       //     string xaml = xamlFlowDocumentElement.OuterXml;

            return conv.master;
        }


        public void ConvertHTML(string htmlString)
        {
          //  string NEW_LINE = Environment.NewLine;

            //htmlString.Replace()
            htmlMain = HtmlParser.ParseHtml(htmlString);
            CssStylesheet stylesheet = new CssStylesheet(htmlMain);
            // Source context is a stack of all elements - ancestors of a parentElement
            List<XmlElement> sourceContext = new List<XmlElement>(10);


            Block b =  AddBlock(htmlMain, new Hashtable(), stylesheet);

            if(b is Section)
            {
                master = (Section)b;
            }
            master = new Section();
            master.Blocks.Add(b);
        }

        private HTMLToFlowConverter()
        {
        //    master = new Section();
         //   current.Blocks.Add(current_paragraph);
        }

        double max_image_width = -1;
        Section master;
        /// <summary>
        /// The section
        /// </summary>
        XmlElement htmlMain;
        List<XmlElement> sourceContext = new List<XmlElement>(10);


        // Stores a parent xaml element for the case when selected fragment is inline.
        //private XmlElement InlineFragmentParentElement;
      
        private Block AddBlock(XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            if (htmlNode is XmlComment)
            {
                return null;
            }
            else if (htmlNode is XmlText)
            {
                return AddParagraphText( (XmlText)htmlNode, inheritedProperties, stylesheet);
            }
            else if (htmlNode is XmlElement)
            {
                // Identify element name
                XmlElement htmlElement = (XmlElement)htmlNode;
                string htmlElementName = htmlElement.LocalName; // Keep the name case-sensitive to check xml names
                string htmlElementNamespace = htmlElement.NamespaceURI;

                if (htmlElementNamespace != HtmlParser.XhtmlNamespace)
                {
                    // Non-html element. skip it
                    // Isn't it too agressive? What if this is just an error in html tag name?
                    // TODO: Consider skipping just a wparrer in recursing into the element tree,
                    // which may produce some garbage though coming from xml fragments.
                    // return htmlElement;
                    return null;
                }

                // Put source element to the stack
                sourceContext.Add(htmlElement);
                // Convert the name to lowercase, because html elements are case-insensitive
                htmlElementName = htmlElementName.ToLower();

                // Switch to an appropriate kind of processing depending on html element name
                switch (htmlElementName)
                {
                    // Sections:
                    case "html":
                    case "body": //this sections is not nececcary to inlcude, we add each section from body.
                    case "div":
                    case "form": // not a block according to xhtml spec
                    case "pre": // Renders text in a fixed-width font
                    case "blockquote":
                    case "caption":
                    case "center":
                    case "cite":
                    case "strong":
                    // case "span":
                          return  AddSection(htmlElement, inheritedProperties, stylesheet);
                    // Paragraphs:
                    case "p":
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                    case "nsrtitle":
                    case "textarea":
                    case "dd": // ???
                    case "dl": // ???
                    case "dt": // ???
                    case "tt": // ???
                    case "span":
                        return AddParagraph(htmlElement, inheritedProperties, stylesheet);
                    case "ol":
                    case "ul":
                    case "dir": //  treat as UL element
                    case "menu": //  treat as UL element
                        // List element conversion
                         return AddList(htmlElement, inheritedProperties, stylesheet);
                    case "li":
                        // LI outside of OL/UL
                        // Collect all sibling LIs, wrap them into a List and then proceed with the element following the last of LIs
                       // htmlNode = AddOrphanListItems(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                        break;

                    case "img":
                        return AddImage(htmlElement, inheritedProperties, stylesheet);
                    case "table":
                        // hand off to table parsing function which will perform special table syntax checks
                        // DISABLE TABLES (it seems like they don't work most of the time)
                        return AddTable(htmlElement, inheritedProperties, stylesheet);
                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                    case "td":
                    case "th":
                        // Table stuff without table wrapper
                        // TODO: add special-case processing here for elements that should be within tables when the
                        // parent element is NOT a table. If the parent element is a table they can be processed normally.
                        // we need to compare against the parent element here, we can't just break on a switch
                        goto default; // Thus we will skip this element as unknown, but still recurse into it.
                    case "style": // We already pre-processed all style elements. Ignore it now
                    case "meta":
                    case "head":
                    case "title":
                    case "script":
                        // Ignore these elements
                        break;
                    default:
                        // Wrap a sequence of inlines into an implicit paragraph
                        return AddParagraph(htmlElement, inheritedProperties, stylesheet);
                }
            }
            return null;
        }

        #region Table

        private Block AddTable(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            Hashtable localProperties = new Hashtable();
            GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);

            // Check if the table contains only one cell - we want to take only its content
            XmlElement singleCell = GetCellFromSingleCellTable(htmlElement);

            if (singleCell != null)
            {
                Table current = new Table();
                ApplyLocalProperties(current, localProperties,/*isBlock*/true);

                TableRowGroup trg = new TableRowGroup();
                current.RowGroups.Add(trg);

                //  Need to push skipped table elements onto sourceContext
                for (XmlNode htmlChildNode = singleCell.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode != null ? htmlChildNode.NextSibling : null)
                {
                    Block b = AddBlock(htmlChildNode, localProperties, stylesheet);
                    if(b!=null)
                    {
                        TableRow tr = new TableRow();
                        tr.Cells.Add(new TableCell(b));
                    }
                }
                return current;
            }
            else
            {
                Table current = new Table();
                ApplyLocalProperties(current, localProperties,/*isBlock*/true);

                // Analyze table structure for column widths and rowspan attributes
                ArrayList columnStarts = AnalyzeTableStructure(htmlElement, stylesheet);

                // Process COLGROUP & COL elements
                AddColumnInformation(current, htmlElement,  columnStarts, localProperties, stylesheet);

                // Process table body - TBODY and TR elements
                XmlNode htmlChildNode = htmlElement.FirstChild;

                while (htmlChildNode != null)
                {
                    string htmlChildName = htmlChildNode.LocalName.ToLower();

                    // Process the element
                    if (htmlChildName == "tbody" || htmlChildName == "thead" || htmlChildName == "tfoot")
                    {
                        TableRowGroup trg = new TableRowGroup();

                        //  Add more special processing for TableHeader and TableFooter
                        //XmlElement xamlTableBodyElement = xamlTableElement.OwnerDocument.CreateElement(null, Xaml_TableRowGroup, _xamlNamespace);
                        //xamlTableElement.AppendChild(xamlTableBodyElement);

                        sourceContext.Add((XmlElement)htmlChildNode);

                        // Get properties of Html tbody element

                        Hashtable childProperties = new Hashtable();
                        GetElementProperties(htmlElement, localProperties, childProperties, stylesheet);
                        ApplyLocalProperties(trg, childProperties,/*isBlock*/true);


                        // Process children of htmlChildNode, which is tbody, for tr elements
                        AddTableRowsToTableBody(trg, htmlChildNode.FirstChild, childProperties, columnStarts, stylesheet);

                        if(trg.Rows.Count>0)
                        {
                            current.RowGroups.Add(trg);
                        }


                        //if (xamlTableBodyElement.HasChildNodes)
                        //{
                        //    xamlTableElement.AppendChild(xamlTableBodyElement);
                        //    // else: if there is no TRs in this TBody, we simply ignore it
                        //}
                        //Debug.Assert(sourceContext.Count > 0 && sourceContext[sourceContext.Count - 1] == htmlChildNode);
                        sourceContext.RemoveAt(sourceContext.Count - 1);

                        htmlChildNode = htmlChildNode.NextSibling;
                    }
                    else if (htmlChildName == "tr")
                    {
                        // Tbody is not present, but tr element is present. Tr is wrapped in tbody
                        //XmlElement xamlTableBodyElement = xamlTableElement.OwnerDocument.CreateElement(null, Xaml_TableRowGroup, _xamlNamespace);
                        TableRowGroup trg = new TableRowGroup();

                        // We use currentProperties of xamlTableElement when adding rows since the tbody element is artificially created and has 
                        // no properties of its own
                        htmlChildNode = AddTableRowsToTableBody(trg, htmlChildNode, localProperties, columnStarts, stylesheet);
                       
                        if(trg.Rows.Count>0)
                        {
                            current.RowGroups.Add(trg);
                        }

                    }
                    else
                    {
                        // Element is not tbody or tr. Ignore it.
                        // TODO: add processing for thead, tfoot elements and recovery for td elements
                        htmlChildNode = htmlChildNode.NextSibling;
                    }
                }

                return current;
            }
        }

        /// <summary>
        /// Adds TableRow elements to xamlTableBodyElement. The rows are converted from Html tr elements that
        /// may be the children of an Html tbody element or an Html table element with tbody missing
        /// </summary>
        /// <param name="xamlTableBodyElement">
        /// XmlElement representing Xaml TableRowGroup element to which the converted rows should be added
        /// </param>
        /// <param name="htmlTRStartNode">
        /// XmlElement representing the first tr child of the tbody element to be read
        /// </param>
        /// <param name="currentProperties">
        /// Hashtable representing current properties of the tbody element that are generated and applied in the
        /// AddTable function; to be used as inheritedProperties when adding tr elements
        /// </param>
        /// <param name="columnStarts"></param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        /// <returns>
        /// XmlNode representing the current position of the iterator among tr elements
        /// </returns>
        private XmlNode AddTableRowsToTableBody(
            TableRowGroup trg, XmlNode htmlTRStartNode, Hashtable currentProperties, ArrayList columnStarts, CssStylesheet stylesheet)
        {
            // Parameter validation
            Debug.Assert(currentProperties != null);

            // Initialize child node for iteratimg through children to the first tr element
            XmlNode htmlChildNode = htmlTRStartNode;
            ArrayList activeRowSpans = null;
            if (columnStarts != null)
            {
                activeRowSpans = new ArrayList();
                InitializeActiveRowSpans(activeRowSpans, columnStarts.Count);
            }

            while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tbody")
            {
                if (htmlChildNode.LocalName.ToLower() == "tr")
                {
                    TableRow tr = new TableRow();

                    //XmlElement xamlTableRowElement = xamlTableBodyElement.OwnerDocument.CreateElement(null, Xaml_TableRow, _xamlNamespace);
                    //sourceContext.Add((XmlElement)htmlChildNode);

                    // Get tr element properties
                    Hashtable trElementCurrentProperties = new Hashtable();
                    GetElementProperties(htmlChildNode as XmlElement, currentProperties, trElementCurrentProperties, stylesheet);
                    ApplyLocalProperties(tr, trElementCurrentProperties,/*isBlock*/true);

                    // TODO: apply local properties to tr element
                    AddTableCellsToTableRow(tr, htmlChildNode.FirstChild, trElementCurrentProperties, columnStarts, activeRowSpans, stylesheet);

                    if (tr.Cells.Count>0)
                    {
                        trg.Rows.Add(tr);
                       // xamlTableBodyElement.AppendChild(xamlTableRowElement);
                    }

                    //Debug.Assert(sourceContext.Count > 0 && sourceContext[sourceContext.Count - 1] == htmlChildNode);
                    //sourceContext.RemoveAt(sourceContext.Count - 1);

                    // Advance
                    htmlChildNode = htmlChildNode.NextSibling;

                }
                else if (htmlChildNode.LocalName.ToLower() == "td")
                {
                    // Tr element is not present. We create one and add td elements to it
                    TableRow tr = new TableRow();


                    // This is incorrect formatting and the column starts should not be set in this case
                    Debug.Assert(columnStarts == null);

                    htmlChildNode = AddTableCellsToTableRow(tr, htmlChildNode, currentProperties, columnStarts, activeRowSpans, stylesheet);

                   if( tr.Cells.Count >0)
                    {
                        trg.Rows.Add(tr);
                    }
                }
                else
                {
                    // Not a tr or td  element. Ignore it.
                    // TODO: consider better recovery here
                    htmlChildNode = htmlChildNode.NextSibling;
                }
            }
            return htmlChildNode;
        }

        /// <summary>
        /// Adds TableCell elements to xamlTableRowElement.
        /// </summary>
        /// <param name="xamlTableRowElement">
        /// XmlElement representing Xaml TableRow element to which the converted cells should be added
        /// </param>
        /// <param name="htmlTDStartNode">
        /// XmlElement representing the child of tr or tbody element from which we should start adding td elements
        /// </param>
        /// <param name="currentProperties">
        /// properties of the current html tr element to which cells are to be added
        /// </param>
        /// <returns>
        /// XmlElement representing the current position of the iterator among the children of the parent Html tbody/tr element
        /// </returns>
        private XmlNode AddTableCellsToTableRow(TableRow tr, XmlNode htmlTDStartNode, Hashtable currentProperties, ArrayList columnStarts, ArrayList activeRowSpans, CssStylesheet stylesheet)
        {
            // parameter validation
            Debug.Assert(currentProperties != null);
            if (columnStarts != null)
            {
                Debug.Assert(activeRowSpans.Count == columnStarts.Count);
            }

            XmlNode htmlChildNode = htmlTDStartNode;
            double columnStart = 0;
            double columnWidth = 0;
            int columnIndex = 0;
            int columnSpan = 0;

            while (htmlChildNode != null && htmlChildNode.LocalName.ToLower() != "tr" && htmlChildNode.LocalName.ToLower() != "tbody" && htmlChildNode.LocalName.ToLower() != "thead" && htmlChildNode.LocalName.ToLower() != "tfoot")
            {
                if (htmlChildNode.LocalName.ToLower() == "td" || htmlChildNode.LocalName.ToLower() == "th")
                {
                    TableCell tc = new TableCell();


                   // sourceContext.Add((XmlElement)htmlChildNode);

                    Hashtable tcElementLocalProperties = new Hashtable();
                    GetElementProperties((XmlElement)htmlChildNode, currentProperties, tcElementLocalProperties, stylesheet);
                    ApplyLocalProperties(tc, tcElementLocalProperties,/*isBlock*/true);


                    if (columnStarts != null)
                    {
                        Debug.Assert(columnIndex < columnStarts.Count - 1);
                        while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
                        {
                            activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
                            Debug.Assert((int)activeRowSpans[columnIndex] >= 0);
                            columnIndex++;
                        }
                        Debug.Assert(columnIndex < columnStarts.Count - 1);
                        columnStart = (double)columnStarts[columnIndex];
                        columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
                        columnSpan = CalculateColumnSpan(columnIndex, columnWidth, columnStarts);
                        int rowSpan = GetRowSpan((XmlElement)htmlChildNode);

                        // Column cannot have no span
                        Debug.Assert(columnSpan > 0);
                        Debug.Assert(columnIndex + columnSpan < columnStarts.Count);

                        tc.ColumnSpan = columnSpan;

                        // Apply row span
                        for (int spannedColumnIndex = columnIndex; spannedColumnIndex < columnIndex + columnSpan; spannedColumnIndex++)
                        {
                            Debug.Assert(spannedColumnIndex < activeRowSpans.Count);
                            activeRowSpans[spannedColumnIndex] = (rowSpan - 1);
                            Debug.Assert((int)activeRowSpans[spannedColumnIndex] >= 0);
                        }

                        columnIndex = columnIndex + columnSpan;
                    }

                    AddDataToTableCell(tc, htmlChildNode.FirstChild, tcElementLocalProperties, stylesheet);


                    if (tc.Blocks.Count>0)
                    {
                        tr.Cells.Add(tc);
                    }

                   // Debug.Assert(sourceContext.Count > 0 && sourceContext[sourceContext.Count - 1] == htmlChildNode);
                   // sourceContext.RemoveAt(sourceContext.Count - 1);

                    htmlChildNode = htmlChildNode.NextSibling;
                }
                else
                {
                    // Not td element. Ignore it.
                    // TODO: Consider better recovery
                    htmlChildNode = htmlChildNode.NextSibling;
                }
            }
            return htmlChildNode;
        }

        /// <summary>
        /// adds table cell data to xamlTableCellElement
        /// </summary>
        /// <param name="xamlTableCellElement">
        /// XmlElement representing Xaml TableCell element to which the converted data should be added
        /// </param>
        /// <param name="htmlDataStartNode">
        /// XmlElement representing the start element of data to be added to xamlTableCellElement
        /// </param>
        /// <param name="currentProperties">
        /// Current properties for the html td/th element corresponding to xamlTableCellElement
        /// </param>
        private void AddDataToTableCell(TableCell tc, XmlNode htmlDataStartNode, Hashtable currentProperties, CssStylesheet stylesheet)
        {
            // Parameter validation
            Debug.Assert(currentProperties != null);

            for (XmlNode htmlChildNode = htmlDataStartNode; htmlChildNode != null; htmlChildNode = htmlChildNode != null ? htmlChildNode.NextSibling : null)
            {
                // Process a new html element and add it to the td element
                Block b = AddBlock(htmlChildNode, currentProperties, stylesheet);
                if(b!=null)
                {
                    tc.Blocks.Add(b);
                }
            }
        }
        

        /// <summary>
        /// Used for initializing activeRowSpans array in the before adding rows to tbody element
        /// </summary>
        /// <param name="activeRowSpans">
        /// ArrayList representing currently active row spans
        /// </param>
        /// <param name="count">
        /// Size to be give to array list
        /// </param>
        private static void InitializeActiveRowSpans(ArrayList activeRowSpans, int count)
        {
            for (int columnIndex = 0; columnIndex < count; columnIndex++)
            {
                activeRowSpans.Add(0);
            }
        }



        /// <summary>
        /// Processes the information about table columns - COLGROUP and COL html elements.
        /// </summary>
        /// <param name="htmlTableElement">
        /// XmlElement representing a source html table.
        /// </param>
        /// <param name="xamlTableElement">
        /// XmlElement repesenting a resulting xaml table.
        /// </param>
        /// <param name="columnStartsAllRows">
        /// Array of doubles - column start coordinates.
        /// Can be null, which means that column size information is not available
        /// and we must use source colgroup/col information.
        /// In case wneh it's not null, we will ignore source colgroup/col information.
        /// </param>
        /// <param name="currentProperties"></param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        private void AddColumnInformation(Table current, XmlElement htmlTableElement,  ArrayList columnStartsAllRows, Hashtable currentProperties, CssStylesheet stylesheet)
        {
            // Add column information
            if (columnStartsAllRows != null)
            {
                // We have consistent information derived from table cells; use it
                // The last element in columnStarts represents the end of the table
                for (int columnIndex = 0; columnIndex < columnStartsAllRows.Count - 1; columnIndex++)
                {
                   // XmlElement xamlColumnElement;

                    TableColumn tc = new TableColumn();


                    //xamlColumnElement = xamlTableElement.OwnerDocument.CreateElement(null, Xaml_TableColumn, _xamlNamespace);
                    double w = ((double)columnStartsAllRows[columnIndex + 1] - (double)columnStartsAllRows[columnIndex]);
                    tc.Width = new GridLength(w,GridUnitType.Star);

                    //xamlColumnElement.SetAttribute(Xaml_Width,
                    //    .ToString());
                    // xamlTableElement.AppendChild(xamlColumnElement);
                    current.Columns.Add(tc);
                }
            }
            else
            {
                // We do not have consistent information from table cells;
                // Translate blindly colgroups from html.                
                for (XmlNode htmlChildNode = htmlTableElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
                {
                    if (htmlChildNode.LocalName.ToLower() == "colgroup")
                    {
                        // TODO: add column width information to this function as a parameter and process it
                        AddTableColumnGroup(current, (XmlElement)htmlChildNode, currentProperties, stylesheet);
                    }
                    else if (htmlChildNode.LocalName.ToLower() == "col")
                    {
                        AddTableColumn(current, (XmlElement)htmlChildNode, currentProperties, stylesheet);
                    }
                    else if (htmlChildNode is XmlElement)
                    {
                        // Some element which belongs to table body. Stop column loop.
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts htmlColgroupElement into Xaml TableColumnGroup element, and appends it to the parent
        /// xamlTableElement
        /// </summary>
        /// <param name="xamlTableElement">
        /// XmlElement representing Xaml Table element to which the converted column group should be added
        /// </param>
        /// <param name="htmlColgroupElement">
        /// XmlElement representing Html colgroup element to be converted
        /// <param name="inheritedProperties">
        /// Properties inherited from parent context
        /// </param>
        private void AddTableColumnGroup(Table table, XmlElement htmlColgroupElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            Hashtable currentProperties = new Hashtable();
            GetElementProperties((XmlElement)htmlColgroupElement, inheritedProperties, currentProperties, stylesheet);
            // TODO: process local properties for colgroup

            // Process children of colgroup. Colgroup may contain only col elements.
            for (XmlNode htmlNode = htmlColgroupElement.FirstChild; htmlNode != null; htmlNode = htmlNode.NextSibling)
            {
                if (htmlNode is XmlElement && htmlNode.LocalName.ToLower() == "col")
                {
                    AddTableColumn(table, (XmlElement)htmlNode, currentProperties, stylesheet);
                }
            }
        }

        /// <summary>
        /// Converts htmlColElement into Xaml TableColumn element, and appends it to the parent
        /// xamlTableColumnGroupElement
        /// </summary>
        /// <param name="xamlTableElement"></param>
        /// <param name="htmlColElement">
        /// XmlElement representing Html col element to be converted
        /// </param>
        /// <param name="inheritedProperties">
        /// properties inherited from parent context
        /// </param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        private void AddTableColumn(Table table, XmlElement htmlColElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            //Hashtable currentProperties = new Hashtable();
          //  GetElementProperties((XmlElement)htmlColElement, inheritedProperties, currentProperties, stylesheet);

            TableColumn tcol = new TableColumn();

            //  ApplyLocalProperties(tcol, currentProperties,/*isBlock*/true);
            // TODO: process local properties for TableColumn element

            // Col is an empty element, with no subtree 
            //  xamlTableElement.AppendChild(xamlTableColumnElement);
            table.Columns.Add(tcol);
        }



        private static XmlElement GetCellFromSingleCellTable(XmlElement htmlTableElement)
        {
            XmlElement singleCell = null;
            for (XmlNode tableChild = htmlTableElement.FirstChild; tableChild != null; tableChild = tableChild.NextSibling)
            {
                string elementName = tableChild.LocalName.ToLower();
                if (elementName == "tbody" || elementName == "thead" || elementName == "tfoot")
                {
                    if (singleCell != null)
                    {
                        return null;
                    }
                    for (XmlNode tbodyChild = tableChild.FirstChild; tbodyChild != null; tbodyChild = tbodyChild.NextSibling)
                    {
                        if (tbodyChild.LocalName.ToLower() == "tr")
                        {
                            if (singleCell != null)
                            {
                                return null;
                            }
                            for (XmlNode trChild = tbodyChild.FirstChild; trChild != null; trChild = trChild.NextSibling)
                            {
                                string cellName = trChild.LocalName.ToLower();
                                if (cellName == "td" || cellName == "th")
                                {
                                    if (singleCell != null)
                                    {
                                        return null;
                                    }
                                    singleCell = (XmlElement)trChild;
                                }
                            }
                        }
                    }
                }
                else if (tableChild.LocalName.ToLower() == "tr")
                {
                    if (singleCell != null)
                    {
                        return null;
                    }
                    for (XmlNode trChild = tableChild.FirstChild; trChild != null; trChild = trChild.NextSibling)
                    {
                        string cellName = trChild.LocalName.ToLower();
                        if (cellName == "td" || cellName == "th")
                        {
                            if (singleCell != null)
                            {
                                return null;
                            }
                            singleCell = (XmlElement)trChild;
                        }
                    }
                }
            }

            return singleCell;
        }


        /// <summary>
        /// Performs a parsing pass over a table to read information about column width and rowspan attributes. This information
        /// is used to determine the starting point of each column. 
        /// </summary>
        /// <param name="htmlTableElement">
        /// XmlElement representing Html table whose structure is to be analyzed
        /// </param>
        /// <returns>
        /// ArrayList of type double which contains the function output. If analysis is successful, this ArrayList contains
        /// all the points which are the starting position of any column in the table, ordered from left to right.
        /// In case if analisys was impossible we return null.
        /// </returns>
        private static ArrayList AnalyzeTableStructure(XmlElement htmlTableElement, CssStylesheet stylesheet)
        {
            // Parameter validation
            Debug.Assert(htmlTableElement.LocalName.ToLower() == "table");
            if (!htmlTableElement.HasChildNodes)
            {
                return null;
            }

            bool columnWidthsAvailable = true;

            ArrayList columnStarts   = new ArrayList();
            ArrayList activeRowSpans = new ArrayList();
            Debug.Assert(columnStarts.Count == activeRowSpans.Count);

            XmlNode htmlChildNode = htmlTableElement.FirstChild;
            double tableWidth = 0;  // Keep track of table width which is the width of its widest row

            // Analyze tbody and tr elements
            while (htmlChildNode != null && columnWidthsAvailable)
            {
                Debug.Assert(columnStarts.Count == activeRowSpans.Count);

                switch (htmlChildNode.LocalName.ToLower())
                {
                    case "tbody":
                        // Tbody element, we should analyze its children for trows
                        double tbodyWidth = AnalyzeTbodyStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
                        if (tbodyWidth > tableWidth)
                        {
                            // Table width must be increased to supported newly added wide row
                            tableWidth = tbodyWidth;
                        }
                        else if (tbodyWidth == 0)
                        {
                            // Tbody analysis may return 0, probably due to unprocessable format. 
                            // We should also fail.
                            columnWidthsAvailable = false; // interrupt the analisys
                        }
                        break;
                    case "tr":
                        // Table row. Analyze column structure within row directly
                        double trWidth = AnalyzeTRStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tableWidth, stylesheet);
                        if (trWidth > tableWidth)
                        {
                            tableWidth = trWidth;
                        }
                        else if (trWidth == 0)
                        {
                            columnWidthsAvailable = false; // interrupt the analisys
                        }
                        break;
                    case "td":
                        // Incorrect formatting, too deep to analyze at this level. Return null.
                        // TODO: implement analysis at this level, possibly by creating a new tr
                        columnWidthsAvailable = false; // interrupt the analisys
                        break;
                    default:
                        // Element should not occur directly in table. Ignore it.
                        break;
                }

                htmlChildNode = htmlChildNode.NextSibling;
            }

            if (columnWidthsAvailable)
            {
                // Add an item for whole table width
                columnStarts.Add(tableWidth);
                VerifyColumnStartsAscendingOrder(columnStarts);
            }
            else
            {
                columnStarts = null;
            }

            return columnStarts;
        }



        /// <summary>
        /// Verifies that values in columnStart, which represent starting coordinates of all columns, are arranged
        /// in ascending order
        /// </summary>
        /// <param name="columnStarts">
        /// ArrayList representing starting coordinates of all columns
        /// </param>
        private static void VerifyColumnStartsAscendingOrder(ArrayList columnStarts)
        {
            Debug.Assert(columnStarts != null);

            double columnStart;
            columnStart = -0.01;

            for (int columnIndex = 0; columnIndex < columnStarts.Count; columnIndex++)
            {
                Debug.Assert(columnStart < (double)columnStarts[columnIndex]);
                columnStart = (double)columnStarts[columnIndex];
            }
        }



        /// <summary>
        /// Performs a parsing pass over a tbody to read information about column width and rowspan attributes. Information read about width
        /// attributes is stored in the reference ArrayList parameter columnStarts, which contains a list of all starting
        /// positions of all columns in the table, ordered from left to right. Row spans are taken into consideration when 
        /// computing column starts
        /// </summary>
        /// <param name="htmlTbodyElement">
        /// XmlElement representing Html tbody whose structure is to be analyzed
        /// </param>
        /// <param name="columnStarts">
        /// ArrayList of type double which contains the function output. If analysis fails, this parameter is set to null
        /// </param>
        /// <param name="tableWidth">
        /// Current width of the table. This is used to determine if a new column when added to the end of table should
        /// come after the last column in the table or is actually splitting the last column in two. If it is only splitting
        /// the last column it should inherit row span for that column
        /// </param>
        /// <returns>
        /// Calculated width of a tbody.
        /// In case of non-analizable column width structure return 0;
        /// </returns>
        private static double AnalyzeTbodyStructure(XmlElement htmlTbodyElement, ArrayList columnStarts, ArrayList activeRowSpans, double tableWidth, CssStylesheet stylesheet)
        {
            // Parameter validation
            Debug.Assert(htmlTbodyElement.LocalName.ToLower() == "tbody");
            Debug.Assert(columnStarts != null);

            double tbodyWidth = 0;
            bool columnWidthsAvailable = true;

            if (!htmlTbodyElement.HasChildNodes)
            {
                return tbodyWidth;
            }

            // Set active row spans to 0 - thus ignoring row spans crossing tbody boundaries
            ClearActiveRowSpans(activeRowSpans);

            XmlNode htmlChildNode = htmlTbodyElement.FirstChild;

            // Analyze tr elements
            while (htmlChildNode != null && columnWidthsAvailable)
            {
                switch (htmlChildNode.LocalName.ToLower())
                {
                    case "tr":
                        double trWidth = AnalyzeTRStructure((XmlElement)htmlChildNode, columnStarts, activeRowSpans, tbodyWidth, stylesheet);
                        if (trWidth > tbodyWidth)
                        {
                            tbodyWidth = trWidth;
                        }
                        break;
                    case "td":
                        columnWidthsAvailable = false; // interrupt the analisys
                        break;
                    default:
                        break;
                }
                htmlChildNode = htmlChildNode.NextSibling;
            }

            // Set active row spans to 0 - thus ignoring row spans crossing tbody boundaries
            ClearActiveRowSpans(activeRowSpans);

            return columnWidthsAvailable ? tbodyWidth : 0;
        }


        /// <summary>
        /// Used for clearing activeRowSpans array in the beginning/end of each tbody
        /// </summary>
        /// <param name="activeRowSpans">
        /// ArrayList representing currently active row spans
        /// </param>
        private static void ClearActiveRowSpans(ArrayList activeRowSpans)
        {
            for (int columnIndex = 0; columnIndex < activeRowSpans.Count; columnIndex++)
            {
                activeRowSpans[columnIndex] = 0;
            }
        }


        /// <summary>
        /// Performs a parsing pass over a tr element to read information about column width and rowspan attributes.  
        /// </summary>
        /// <param name="htmlTRElement">
        /// XmlElement representing Html tr element whose structure is to be analyzed
        /// </param>
        /// <param name="columnStarts">
        /// ArrayList of type double which contains the function output. If analysis is successful, this ArrayList contains
        /// all the points which are the starting position of any column in the tr, ordered from left to right. If analysis fails,
        /// the ArrayList is set to null
        /// </param>
        /// <param name="activeRowSpans">
        /// ArrayList representing all columns currently spanned by an earlier row span attribute. These columns should
        /// not be used for data in this row. The ArrayList actually contains notation for all columns in the table, if the
        /// active row span is set to 0 that column is not presently spanned but if it is > 0 the column is presently spanned
        /// </param>
        /// <param name="tableWidth">
        /// Double value representing the current width of the table.
        /// Return 0 if analisys was insuccessful.
        /// </param>
        private static double AnalyzeTRStructure(XmlElement htmlTRElement, ArrayList columnStarts, ArrayList activeRowSpans, double tableWidth, CssStylesheet stylesheet)
        {
            double columnWidth;

            // Parameter validation
            Debug.Assert(htmlTRElement.LocalName.ToLower() == "tr");
            Debug.Assert(columnStarts != null);
            Debug.Assert(activeRowSpans != null);
            Debug.Assert(columnStarts.Count == activeRowSpans.Count);

            if (!htmlTRElement.HasChildNodes)
            {
                return 0;
            }

            bool columnWidthsAvailable = true;

            double columnStart = 0; // starting position of current column
            XmlNode htmlChildNode = htmlTRElement.FirstChild;
            int columnIndex = 0;
            double trWidth = 0;

            // Skip spanned columns to get to real column start
            if (columnIndex < activeRowSpans.Count)
            {
                Debug.Assert((double)columnStarts[columnIndex] >= columnStart);
                if ((double)columnStarts[columnIndex] == columnStart)
                {
                    // The new column may be in a spanned area
                    while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
                    {
                        activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
                        Debug.Assert((int)activeRowSpans[columnIndex] >= 0);
                        columnIndex++;
                        columnStart = (double)columnStarts[columnIndex];
                    }
                }
            }

            while (htmlChildNode != null && columnWidthsAvailable)
            {
                Debug.Assert(columnStarts.Count == activeRowSpans.Count);

                VerifyColumnStartsAscendingOrder(columnStarts);

                switch (htmlChildNode.LocalName.ToLower())
                {
                    case "td":
                        Debug.Assert(columnIndex <= columnStarts.Count);
                        if (columnIndex < columnStarts.Count)
                        {
                            Debug.Assert(columnStart <= (double)columnStarts[columnIndex]);
                            if (columnStart < (double)columnStarts[columnIndex])
                            {
                                columnStarts.Insert(columnIndex, columnStart);
                                // There can be no row spans now - the column data will appear here
                                // Row spans may appear only during the column analysis
                                activeRowSpans.Insert(columnIndex, 0);
                            }
                        }
                        else
                        {
                            // Column start is greater than all previous starts. Row span must still be 0 because
                            // we are either adding after another column of the same row, in which case it should not inherit
                            // the previous column's span. Otherwise we are adding after the last column of some previous
                            // row, and assuming the table widths line up, we should not be spanned by it. If there is
                            // an incorrect tbale structure where a columns starts in the middle of a row span, we do not
                            // guarantee correct output
                            columnStarts.Add(columnStart);
                            activeRowSpans.Add(0);
                        }
                        columnWidth = GetColumnWidth((XmlElement)htmlChildNode);
                        if (columnWidth != -1)
                        {
                            int nextColumnIndex;
                            int rowSpan = GetRowSpan((XmlElement)htmlChildNode);

                            nextColumnIndex = GetNextColumnIndex(columnIndex, columnWidth, columnStarts, activeRowSpans);
                            if (nextColumnIndex != -1)
                            {
                                // Entire column width can be processed without hitting conflicting row span. This means that
                                // column widths line up and we can process them
                                Debug.Assert(nextColumnIndex <= columnStarts.Count);

                                // Apply row span to affected columns
                                for (int spannedColumnIndex = columnIndex; spannedColumnIndex < nextColumnIndex; spannedColumnIndex++)
                                {
                                    activeRowSpans[spannedColumnIndex] = rowSpan - 1;
                                    Debug.Assert((int)activeRowSpans[spannedColumnIndex] >= 0);
                                }

                                columnIndex = nextColumnIndex;

                                // Calculate columnsStart for the next cell
                                columnStart = columnStart + columnWidth;

                                if (columnIndex < activeRowSpans.Count)
                                {
                                    Debug.Assert((double)columnStarts[columnIndex] >= columnStart);
                                    if ((double)columnStarts[columnIndex] == columnStart)
                                    {
                                        // The new column may be in a spanned area
                                        while (columnIndex < activeRowSpans.Count && (int)activeRowSpans[columnIndex] > 0)
                                        {
                                            activeRowSpans[columnIndex] = (int)activeRowSpans[columnIndex] - 1;
                                            Debug.Assert((int)activeRowSpans[columnIndex] >= 0);
                                            columnIndex++;
                                            columnStart = (double)columnStarts[columnIndex];
                                        }
                                    }
                                    // else: the new column does not start at the same time as a pre existing column
                                    // so we don't have to check it for active row spans, it starts in the middle
                                    // of another column which has been checked already by the GetNextColumnIndex function
                                }
                            }
                            else
                            {
                                // Full column width cannot be processed without a pre existing row span.
                                // We cannot analyze widths
                                columnWidthsAvailable = false;
                            }
                        }
                        else
                        {
                            // Incorrect column width, stop processing
                            columnWidthsAvailable = false;
                        }
                        break;
                    default:
                        break;
                }

                htmlChildNode = htmlChildNode.NextSibling;
            }

            // The width of the tr element is the position at which it's last td element ends, which is calculated in
            // the columnStart value after each td element is processed
            if (columnWidthsAvailable)
            {
                trWidth = columnStart;
            }
            else
            {
                trWidth = 0;
            }

            return trWidth;
        }


        /// <summary>
        /// Gets row span attribute from htmlTDElement. Returns an integer representing the value of the rowspan attribute.
        /// Default value if attribute is not specified or if it is invalid is 1
        /// </summary>
        /// <param name="htmlTDElement">
        /// Html td element to be searched for rowspan attribute
        /// </param>
        private static int GetRowSpan(XmlElement htmlTDElement)
        {
            string rowSpanAsString;
            int rowSpan;

            rowSpanAsString = GetAttribute((XmlElement)htmlTDElement, "rowspan");
            if (rowSpanAsString != null)
            {
                if (!Int32.TryParse(rowSpanAsString, out rowSpan))
                {
                    // Ignore invalid value of rowspan; treat it as 1
                    rowSpan = 1;
                }
            }
            else
            {
                // No row span, default is 1
                rowSpan = 1;
            }
            return rowSpan;
        }

        /// <summary>
        /// Gets index at which a column should be inseerted into the columnStarts ArrayList. This is
        /// decided by the value columnStart. The columnStarts ArrayList is ordered in ascending order.
        /// Returns an integer representing the index at which the column should be inserted
        /// </summary>
        /// <param name="columnStarts">
        /// Array list representing starting coordinates of all columns in the table
        /// </param>
        /// <param name="columnStart">
        /// Starting coordinate of column we wish to insert into columnStart
        /// </param>
        /// <param name="columnIndex">
        /// Int representing the current column index. This acts as a clue while finding the insertion index.
        /// If the value of columnStarts at columnIndex is the same as columnStart, then this position alrady exists
        /// in the array and we can jsut return columnIndex.
        /// </param>
        /// <returns></returns>
        private static int GetNextColumnIndex(int columnIndex, double columnWidth, ArrayList columnStarts, ArrayList activeRowSpans)
        {
            double columnStart;
            int spannedColumnIndex;

            // Parameter validation
            Debug.Assert(columnStarts != null);
            Debug.Assert(0 <= columnIndex && columnIndex <= columnStarts.Count);
            Debug.Assert(columnWidth > 0);

            columnStart = (double)columnStarts[columnIndex];
            spannedColumnIndex = columnIndex + 1;

            while (spannedColumnIndex < columnStarts.Count && (double)columnStarts[spannedColumnIndex] < columnStart + columnWidth && spannedColumnIndex != -1)
            {
                if ((int)activeRowSpans[spannedColumnIndex] > 0)
                {
                    // The current column should span this area, but something else is already spanning it
                    // Not analyzable
                    spannedColumnIndex = -1;
                }
                else
                {
                    spannedColumnIndex++;
                }
            }

            return spannedColumnIndex;
        }


        private static double GetColumnWidth(XmlElement htmlTDElement)
        {
            string columnWidthAsString;
            double columnWidth;

            columnWidthAsString = null;
            columnWidth = -1;

            // Get string valkue for the width
            columnWidthAsString = GetAttribute(htmlTDElement, "width");
            if (columnWidthAsString == null)
            {
                columnWidthAsString = 
                    
                    
                    GetCssAttribute(GetAttribute(htmlTDElement, "style"), "width");
            }

            // We do not allow column width to be 0, if specified as 0 we will fail to record it
            if (!TryGetLengthValue(columnWidthAsString, out columnWidth) || columnWidth == 0)
            {
                columnWidth = -1;
            }
            return columnWidth;
        }

        /// <summary>
        /// Extracts a value of css attribute from css style definition.
        /// </summary>
        /// <param name="cssStyle">
        /// Source csll style definition
        /// </param>
        /// <param name="attributeName">
        /// A name of css attribute to extract
        /// </param>
        /// <returns>
        /// A string rrepresentation of an attribute value if found;
        /// null if there is no such attribute in a given string.
        /// </returns>
        private static string GetCssAttribute(string cssStyle, string attributeName)
        {
            //  This is poor man's attribute parsing. Replace it by real css parsing
            if (cssStyle != null)
            {
                string[] styleValues;

                attributeName = attributeName.ToLower();

                // Check for width specification in style string
                styleValues = cssStyle.Split(';');

                for (int styleValueIndex = 0; styleValueIndex < styleValues.Length; styleValueIndex++)
                {
                    string[] styleNameValue;

                    styleNameValue = styleValues[styleValueIndex].Split(':');
                    if (styleNameValue.Length == 2)
                    {
                        if (styleNameValue[0].Trim().ToLower() == attributeName)
                        {
                            return styleNameValue[1].Trim();
                        }
                    }
                }
            }

            return null;
        }



        /// <summary>
        /// Calculates column span based the column width and the widths of all other columns. Returns an integer representing 
        /// the column span
        /// </summary>
        /// <param name="columnIndex">
        /// Index of the current column
        /// </param>
        /// <param name="columnWidth">
        /// Width of the current column
        /// </param>
        /// <param name="columnStarts">
        /// ArrayList repsenting starting coordinates of all columns
        /// </param>
        private static int CalculateColumnSpan(int columnIndex, double columnWidth, ArrayList columnStarts)
        {
            // Current status of column width. Indicates the amount of width that has been scanned already
            double columnSpanningValue;
            int columnSpanningIndex;
            int columnSpan;
            double subColumnWidth; // Width of the smallest-grain columns in the table

            Debug.Assert(columnStarts != null);
            Debug.Assert(columnIndex < columnStarts.Count - 1);
            Debug.Assert((double)columnStarts[columnIndex] >= 0);
            Debug.Assert(columnWidth > 0);

            columnSpanningIndex = columnIndex;
            columnSpanningValue = 0;
            columnSpan = 0;
            subColumnWidth = 0;

            while (columnSpanningValue < columnWidth && columnSpanningIndex < columnStarts.Count - 1)
            {
                subColumnWidth = (double)columnStarts[columnSpanningIndex + 1] - (double)columnStarts[columnSpanningIndex];
                Debug.Assert(subColumnWidth > 0);
                columnSpanningValue += subColumnWidth;
                columnSpanningIndex++;
            }

            // Now, we have either covered the width we needed to cover or reached the end of the table, in which
            // case the column spans all the columns until the end
            columnSpan = columnSpanningIndex - columnIndex;
            Debug.Assert(columnSpan > 0);

            return columnSpan;
        }






        #endregion

        /// <summary>
        /// Generates Section or Paragraph element from DIV depending whether it contains any block elements or not
        /// </summary>
        /// <param name="xamlParentElement">
        /// XmlElement representing Xaml parent to which the converted element should be added
        /// </param>
        /// <param name="htmlElement">
        /// XmlElement representing Html element to be converted
        /// </param>
        /// <param name="inheritedProperties">
        /// properties inherited from parent context
        /// </param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        /// true indicates that a content added by this call contains at least one block element
        /// </param>
        private Block AddSection(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            // Analyze the content of htmlElement to decide what xaml element to choose - Section or Paragraph.
            // If this Div has at least one block child then we need to use Section, otherwise use Paragraph
            bool htmlElementContainsBlocks = false;
            for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
            {
                if (htmlChildNode is XmlElement)
                {
                    string htmlChildName = ((XmlElement)htmlChildNode).LocalName.ToLower();
                    if (HtmlSchema.IsBlockElement(htmlChildName))
                    {
                        htmlElementContainsBlocks = true;
                        break;
                    }
                }
            }
            if (!htmlElementContainsBlocks)
            {
                // The Div does not contain any block elements, so we can treat it as a Paragraph
               return AddParagraph(htmlElement, inheritedProperties, stylesheet);
            }
            else
            {
                Hashtable localProperties = new Hashtable();
                GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);

                Section current = new Section();
                /*Apply style for the current Section.*/
                ApplyLocalProperties(current, localProperties,/*isBlock*/true);



                // Recurse into element subtree
                for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode != null ? htmlChildNode.NextSibling : null)
                {
                   Block b =  AddBlock(htmlChildNode, localProperties, stylesheet);
                   if (b != null)
                        current.Blocks.Add(b);
                }
                return current;
            }
        }

        private Paragraph AddParagraphText(XmlText htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            //    Inline i = AddInline()
            //  GetElementProperties()
            //  p.Inlines.Add( )
            //   htmlElement.ChildNodes
            if (htmlElement.Value.Trim().Length > 0)
            {
                Paragraph p = new Paragraph();
                Run r = AddTextRun(htmlElement.Value);
                if (r.Text.Length > 0)
                {
                    p.Inlines.Add(r);
                }
                return p;
            }
            return null;
        }

        /// <summary>
        /// Generates Paragraph element from P, H1-H7, Center etc.
        /// </summary>
        /// <param name="xamlParentElement">
        /// XmlElement representing Xaml parent to which the converted element should be added
        /// </param>
        /// <param name="htmlElement">
        /// XmlElement representing Html element to be converted
        /// </param>
        /// <param name="inheritedProperties">
        /// properties inherited from parent context
        /// </param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        /// true indicates that a content added by this call contains at least one block element
        /// </param>
        private Paragraph AddParagraph(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet )
        {
            Paragraph p = new Paragraph();
            // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
            Hashtable localProperties = new Hashtable();
            GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);

            // Create a XAML element corresponding to this html element
            ApplyLocalProperties(p, localProperties, /*isBlock:*/true);

            // Recurse into element subtree
            for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
            {
                Inline i = AddInline(htmlChildNode, inheritedProperties, stylesheet);
                if (i != null)
                {
                    p.Inlines.Add(i);
                }
            }

            // Add the new element to the parent.
            // xamlParentElement.AppendChild(xamlElement);
            if (p.Inlines.Count > 0)
            {
                return p;
            }
            else return null;
        }

        private Block AddList(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            //  string htmlListElementName = htmlListElement.LocalName.ToLower();
            Hashtable localProperties = new Hashtable();
            GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);


            List l = new List();
            // Create Xaml List element
            string htmlListElementName = htmlElement.LocalName.ToLower();
            // Set default list markers
            if (htmlListElementName == "ol")
            {
                // Ordered list
                l.MarkerStyle = TextMarkerStyle.Decimal;
            }
            else
            {
                // Unordered list - all elements other than OL treated as unordered lists
                l.MarkerStyle = TextMarkerStyle.Disc;
            }

            // Apply local properties to list to set marker attribute if specified
            ApplyLocalProperties(l, localProperties, /*isBlock:*/true);

           
            ListItem current = null;
            for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
            {
                if (htmlChildNode is XmlElement && htmlChildNode.LocalName.ToLower() == "li")
                {
                    current = AddListItem(htmlChildNode as XmlElement, inheritedProperties, stylesheet);
                    if (current != null)
                    {
                        l.ListItems.Add(current);
                    }
                  
                } else
                {
                    if(current!=null)
                    {
                        Block b = AddBlock(htmlChildNode, localProperties, stylesheet);
                        if (b != null)
                            current.Blocks.Add(b);
                    }
                }
            }

            // Add the new element to the parent.
            // xamlParentElement.AppendChild(xamlElement);
            if (l.ListItems.Count > 0)
            {
                return l;
            }
            else return null;
        }


        private ListItem AddListItem(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            // Parameter validation

            Hashtable localProperties = new Hashtable();
            GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);

            ListItem li = new ListItem();

            // Process children of the ListItem
            for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
            { 
                Block b = AddBlock(htmlChildNode, localProperties, stylesheet);
                if (b != null)
                    li.Blocks.Add(b);
            }

            // Apply local properties to list to set marker attribute if specified
            ApplyLocalProperties(li, localProperties, /*isBlock:*/true);
            return li;
        }



        // .............................................................
        //
        // Inline Elements
        //
        // .............................................................
        private Inline AddInline(XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            if (htmlNode is XmlComment)
            {
                //    DefineInlineFragmentParent((XmlComment)htmlNode, xamlParentElement);
                return null;
            }
            else if (htmlNode is XmlText)
            {
                return AddTextRun(htmlNode.Value);
            }
            else if (htmlNode is XmlElement)
            {
                XmlElement htmlElement = (XmlElement)htmlNode;
                // Check whether this is an html element
                if (htmlElement.NamespaceURI != HtmlParser.XhtmlNamespace)
                {
                    return null; // Skip non-html elements
                }

                // Identify element name
                string htmlElementName = htmlElement.LocalName.ToLower();
                switch (htmlElementName)
                {
                    case "a":

                      //  Paragraph s = new Paragraph();
                      //  s.Inlines.Add(new InlineImage2());



                        // DISABLE LINKS
                        return  AddHyperlink(htmlNode,  inheritedProperties, stylesheet);
                        // AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet, sourceContext);
                    case "img":
                        return  AddInlineImage(htmlNode, inheritedProperties, stylesheet);
                    case "br":
                    case "hr":
                        //  return AddBreak(htmlElementName);
                        return new LineBreak();
                    case "span":
                    default:
                        return AddSpan(htmlNode, inheritedProperties, stylesheet);
                        //if (HtmlSchema.IsInlineElement(htmlElementName) || HtmlSchema.IsBlockElement(htmlElementName))
                        //{
                        //    // Note: actually we do not expect block elements here,
                        //    // but if it happens to be here, we will treat it as a Span.
                        //   // return AddSpanOrRun(xamlParentElement, htmlElement, inheritedProperties, stylesheet);
                        //}
                }
                // Ignore all other elements non-(block/inline/image)
                // Remove the element from the stack
                // Debug.Assert(sourceContext.Count > 0 && sourceContext[sourceContext.Count - 1] == htmlElement);
                // sourceContext.RemoveAt(sourceContext.Count - 1);
            }
            return null;
        }


   

        /// <summary>
        /// Creates a Paragraph element and adds all nodes starting from htmlNode
        /// converted to appropriate Inlines.
        /// </summary>
        /// <param name="xamlParentElement">
        /// XmlElement representing Xaml parent to which the converted element should be added
        /// </param>
        /// <param name="htmlNode">
        /// XmlNode starting a collection of implicitly wrapped inlines.
        /// </param>
        /// <param name="inheritedProperties">
        /// properties inherited from parent context
        /// </param>
        /// <param name="stylesheet"></param>
        /// <param name="sourceContext"></param>
        /// true indicates that a content added by this call contains at least one block element
        /// </param>
        /// <returns>
        /// The last htmlNode added to the inside Paragraph.
        /// </returns>
        private Inline AddSpan(XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            XmlNode lastNodeProcessed = null;
            while (htmlNode != null)
            {
                if (htmlNode is XmlComment)
                {
                    return null;
                }
                else if (htmlNode is XmlText)
                {
                    if (htmlNode.Value.Trim().Length > 0)
                    {
                        Run r = AddTextRun(htmlNode.Value);
                        if (r.Text.Length > 0) {
                            return r;
                         }
                        
                    }
                    return null;
                }
                else if (htmlNode is XmlElement)
                {
                    Span p = new Span();

                    //ApplyDecoration(p, inheritedProperties);
                    Hashtable localProperties = new Hashtable();
                    GetElementProperties((XmlElement)htmlNode, inheritedProperties, localProperties, stylesheet);

                    // Create a XAML element corresponding to this html element
                    ApplyLocalProperties(p, localProperties, /*isBlock:*/false);

                    for (XmlNode htmlChildNode = htmlNode.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
                    {
                        Inline i = AddInline(htmlChildNode, inheritedProperties, stylesheet);
                        if (i != null)
                        {
                            p.Inlines.Add(i);
                        }
                    }

                    if (p.Inlines.Count > 0)
                    {
                        return p;
                    }
                    return null;
                    //Inline i = AddInline((XmlElement)htmlNode, inheritedProperties, stylesheet);
                    //if (i != null)
                    //{
                    //    p.Inlines.Add(i);
                    //}

                    //string htmlChildName = ((XmlElement)htmlNode).LocalName.ToLower();
                    //if (HtmlSchema.IsBlockElement(htmlChildName))
                    //{
                    //    Span s = AddSpan((XmlElement)htmlNode, inheritedProperties, stylesheet);
                    //    p.Inlines.Add(s);
                    //    // The sequence of non-blocked inlines ended. Stop implicit loop here.
                    //    break;
                    //}
                    //else
                    //{
                    //   }
                }
                // Store last processed node to return it at the end
                lastNodeProcessed = htmlNode;
                htmlNode = htmlNode.NextSibling;
            }
            return null;
            // Add the Paragraph to the parent
            // If only whitespaces and commens have been encountered,
            // then we have nothing to add in implicit paragraph; forget it.
            //if (xamlParagraph.FirstChild != null)
            //{
            //    xamlParentElement.AppendChild(xamlParagraph);
            //}
            //// Need to return last processed node
            //return lastNodeProcessed;
        }


        private Inline AddHyperlink(XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            XmlElement htmlElement = (XmlElement)htmlNode;
            // Convert href attribute into NavigateUri and TargetName
            string href = GetAttribute(htmlElement, "href");
            if (href == null)
            {
                // When href attribute is missing - ignore the hyperlink
                return  AddSpan(htmlNode, inheritedProperties, stylesheet);
            }
            else
            {
                Hyperlink h = new Hyperlink();
                h.Cursor = System.Windows.Input.Cursors.Hand;
                // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
                Hashtable localProperties = new Hashtable();
                //   Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);

                GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);
                ApplyLocalProperties(h, localProperties, /*isBlock:*/false);

                // Create a XAML element corresponding to this html element
                //XmlElement xamlElement = xamlParentElement.OwnerDocument.CreateElement(/*prefix:*/null, /*localName:*/HtmlToXamlConverter.Xaml_Hyperlink, _xamlNamespace);
                //ApplyLocalProperties(xamlElement, localProperties, /*isBlock:*/false);



                string[] hrefParts = href.Split(new char[] { '#' });

                if (hrefParts.Length > 0)
                {
                    h.NavigateUri = new Uri(hrefParts[0].Trim());

                    if (hrefParts.Length > 1)
                    {
                        h.TargetName = hrefParts[1].Trim();
                    }
                }

                Semagsoft.HyperlinkHelper.SubscribellHyperlink(h);



                for (XmlNode htmlChildNode = htmlElement.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode.NextSibling)
                {
                    Inline i = AddInline(htmlChildNode, inheritedProperties, stylesheet);
                    if (i != null)
                    {
                        h.Inlines.Add(i);
                    }
                }

                return h;
                // Add the new element to the parent.
                // xamlParentElement.AppendChild(xamlElement);
            }
        }


        // Adds a text run to a xaml tree
        private static Run AddTextRun(string textData)
        {
            // Remove control characters
            for (int i = 0; i < textData.Length; i++)
            {
                if (Char.IsControl(textData[i]) && textData[i] != '\n')
                {
                    textData = textData.Remove(i--, 1);  // decrement i to compensate for character removal
                }
            }
            // Replace No-Breaks by spaces (160 is a code of &nbsp; entity in html)
            //  This is a work around the bug in Avalon which does not render nbsp.
            textData = textData.Replace((char)160, ' ');
            return new Run(textData);
        }

        private Inline AddInlineImage(XmlNode htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            XmlElement htmlElement = (XmlElement)htmlNode;

            Hashtable localProperties = new Hashtable();
            //   Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);


            string src = GetAttribute(htmlElement, "src");
            string w = GetAttribute(htmlElement, "width");
            string h = GetAttribute(htmlElement, "height");

            if (src != null && src.Length > 0)
            {
                byte[] r = URLDownload(src);
                if (r.Length > 0)
                {
                    double dw = -1;
                    if (w != null && w.Length > 0)
                    {
                        Double.TryParse(w, out dw);
                    }

                    double dh = -1;
                    if (h != null && h.Length > 0)
                    {
                        Double.TryParse(h, out dh);
                    }


                    byte[] b = URLDownload(src);
                    if (b.Length > 0)
                    {
                        Span ii =  YATE.YATEHelper.GetImage(YATE.YATEHelper.ConvertToImage(b));
                        if (ii == null) return null;
                        System.Windows.Controls.Image i = YATE.YATEHelper.ImageFromSpan(ii);

                        if(dw>0)
                        {
                            i.Width = dw;
                        }
                        if (max_image_width > 0 && i.Width > max_image_width)
                        {
                            YATE.YATEHelper.AdjustWidth(i, max_image_width);

                        }
                        if(dh>0)
                        {
                            i.Height = dh;
                        }
                        i.Stretch = Stretch.Uniform;
                      
                        return ii;
                    }



                }
            }

            return null;
            // i.Width  

            // URLDownloadOut

        }




        private Block AddImage(XmlElement htmlNode, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            XmlElement htmlElement = (XmlElement)htmlNode;

            Hashtable localProperties = new Hashtable();
            //   Hashtable currentProperties = GetElementProperties(htmlElement, inheritedProperties, out localProperties, stylesheet, sourceContext);

          
            string src = GetAttribute(htmlElement, "src");
            string w = GetAttribute(htmlElement, "width");
            string h = GetAttribute(htmlElement, "height");

            if(src!=null && src.Length>0)
            {
              byte[] r = URLDownload(src);
                if (r.Length > 0)
                {
                    double dw = -1;
                    if (w != null && w.Length > 0)
                    {
                        Double.TryParse(w, out dw);
                    }

                    double dh = -1;
                    if (h != null && h.Length > 0)
                    {
                        Double.TryParse(h, out dh);
                    }


                    byte[] b = URLDownload(src);
                    if (b.Length > 0)
                    {
                        Span ii = YATE.YATEHelper.GetImage(YATE.YATEHelper.ConvertToImage(b));
                        if (ii == null) return null;
                        System.Windows.Controls.Image i = YATE.YATEHelper.ImageFromSpan(ii);

                        if (dw > 0)
                        {
                            i.Width = dw;
                        }
                        if (max_image_width > 0 && i.Width > max_image_width)
                        {
                            YATE.YATEHelper.AdjustWidth(i, max_image_width);
                        }
                        if (dh > 0)
                        {
                            i.Height = dh;
                        }
                        i.Stretch = Stretch.Uniform;

                        Paragraph p = new Paragraph(ii);
                        return p;
                    }

                  

                }
            }

            return null;
            // i.Width  

            // URLDownloadOut

        }


        //public byte[] URLDownload(string url)
        //{
        //    System.Net.HttpWebRequest request = null;
        //    System.Net.HttpWebResponse response = null;
        //    byte[] b = null;

        //    request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        //    response = (System.Net.HttpWebResponse)request.GetResponse();

        //    if (request.HaveResponse)
        //    {
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            Stream receiveStream = response.GetResponseStream();
        //            using (BinaryReader br = new BinaryReader(receiveStream))
        //            {
        //                b = br.ReadBytes(500000);
        //                br.Close();
        //            }
        //        }
        //    }
        //    return b;
        //}

        const string DATA_IMAGE = "data:image";
        const string DATA_BASE64 = "base64,";

        //"data:image/jpeg;base64,"

        static public byte[] URLDownload(string url)
        {
            if(url.Length> DATA_IMAGE.Length && url.Substring(0,DATA_IMAGE.Length).Equals(DATA_IMAGE))
            {
                int i = url.IndexOf("base64,");
                if (i > 0)
                {
                    url = url.Substring(i + DATA_BASE64.Length, url.Length - (i + DATA_BASE64.Length)).Trim();
                    return Convert.FromBase64String(url);

                }
                return null;
            }


            WebClient wb = new System.Net.WebClient();
            string _UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            wb.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);
            return wb.DownloadData(url);
        }


        static public int URLDownloadOut(string url)
        {
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> responseT = client.GetAsync(url);
            responseT.Wait();
            var response = responseT.Result;


            var filetype = response.Content.Headers.ContentType.MediaType;

            //    Task<HttpResponseMessage> responseC = client.GetAsync("https://encrypted-tbn2.gstatic.com/images?q=tbn%3aANd9GcTw4P3HxyHR8wumE3lY3TOlGworijj2U2DawhY9wnmcPKnbmGHg");

            //    responseC.Wait();
            var imageArray = responseT.Result.Content.ReadAsByteArrayAsync();

            return 1;
        }



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
            double borderThicknessLeft =0;
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
                            element.FontStyle =  (FontStyle) propertyEnumerator.Value ;
                        }
                        break;
                    case "font-variant":
                        //  Convert from font-variant into xaml property
                        break;
                    case "font-weight":
                        {
                            // xamlElement.SetAttribute(Xaml_FontWeight, (string)propertyEnumerator.Value);
                            element.FontWeight = (FontWeight) propertyEnumerator.Value;
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

                        if (v > 0) {


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

                    // NOTE: css names for elementary border styles have side indications in the middle (top/bottom/left/right)
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
                if(borderColorSet)
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
                    localProperties["font-size"] =  Xaml_FontSize_XXSmall;
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

        public const double Xaml_FontSize_XXLarge = 22*(96.0/72.0);// "22pt"; // "XXLarge";
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
