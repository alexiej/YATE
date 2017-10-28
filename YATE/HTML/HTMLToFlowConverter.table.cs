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
        #region Table

        private void FillTableWithEmptyCells(Table t)
        {
                    foreach(TableRowGroup trg in t.RowGroups)
            {
                foreach(TableRow tr in trg.Rows)
                {
                    int addcells  = maxc - tr.Cells.Count ;
                    for (int i = 0; i < addcells;i++ )
                    {
                        tr.Cells.Add(new TableCell());
                    }
                }
            }
        }
        int maxc = 0;

        private Block AddTable(XmlElement htmlElement, Hashtable inheritedProperties, CssStylesheet stylesheet)
        {
            try
            {
                Hashtable localProperties = new Hashtable();
                GetElementProperties(htmlElement, inheritedProperties, localProperties, stylesheet);

                // Check if the table contains only one cell - we want to take only its content
                XmlElement singleCell = GetCellFromSingleCellTable(htmlElement);

                if (singleCell != null)
                {
                    Table current = new Table(); maxc = 0;
                    ApplyLocalProperties(current, localProperties,/*isBlock*/true);

                    TableRowGroup trg = new TableRowGroup();
                    current.RowGroups.Add(trg);

                    //  Need to push skipped table elements onto sourceContext
                    for (XmlNode htmlChildNode = singleCell.FirstChild; htmlChildNode != null; htmlChildNode = htmlChildNode != null ? htmlChildNode.NextSibling : null)
                    {
                        Block b = AddBlock(htmlChildNode, localProperties, stylesheet);
                        if (b != null)
                        {
                            TableRow tr = new TableRow();
                            tr.Cells.Add(new TableCell(b));
                        }
                    }
                    return current;
                }
                else
                {
                    Table current = new Table(); maxc = 0;
                    ApplyLocalProperties(current, localProperties,/*isBlock*/true);

                    // Analyze table structure for column widths and rowspan attributes
                    ArrayList columnStarts = AnalyzeTableStructure(htmlElement, stylesheet);

                    // Process COLGROUP & COL elements
                    AddColumnInformation(current, htmlElement, columnStarts, localProperties, stylesheet);

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

                            if (trg.Rows.Count > 0)
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

                            if (trg.Rows.Count > 0)
                            {
                                current.RowGroups.Add(trg);
                            }

                        }
                        else
                        {
                            // Element is not tbody or tr. Ignore it.
                            // TODO: add processing for thead, tfoot elements and recovery for td elements id:260 gh:261
                            htmlChildNode = htmlChildNode.NextSibling;
                        }
                    }
                    FillTableWithEmptyCells(current);
                    return current;
                }
            }
            catch (Exception)
            {
                return null;
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

                    // TODO: apply local properties to tr element id:280 gh:281
                    AddTableCellsToTableRow(tr, htmlChildNode.FirstChild, trElementCurrentProperties, columnStarts, activeRowSpans, stylesheet);

                    if (tr.Cells.Count > 0)
                    {
                        trg.Rows.Add(tr);
                        maxc = (maxc < tr.Cells.Count ? tr.Cells.Count : maxc);
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

                    if (tr.Cells.Count > 0)
                    {
                        trg.Rows.Add(tr);
                        maxc = (maxc < tr.Cells.Count ? tr.Cells.Count : maxc);
                    }
                }
                else
                {
                    // Not a tr or td  element. Ignore it.
                    // TODO: consider better recovery here id:291 gh:292
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


                    if (tc.Blocks.Count > 0)
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
                    // TODO: Consider better recovery id:300 gh:301
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
                if (b != null)
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
        private void AddColumnInformation(Table current, XmlElement htmlTableElement, ArrayList columnStartsAllRows, Hashtable currentProperties, CssStylesheet stylesheet)
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
                    tc.Width = new GridLength(w, GridUnitType.Star);

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
                        // TODO: add column width information to this function as a parameter and process it id:321 gh:322
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
            // TODO: process local properties for colgroup id:261 gh:262

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
            // TODO: process local properties for TableColumn element id:281 gh:282

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

            ArrayList columnStarts = new ArrayList();
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
                        // TODO: implement analysis at this level, possibly by creating a new tr id:292 gh:293
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

    }
}
