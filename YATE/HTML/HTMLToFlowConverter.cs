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
            try {
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
                        Block b = AddBlock(htmlChildNode, localProperties, stylesheet);
                        if (b != null)
                            current.Blocks.Add(b);
                    }
                    return current;
                }
            }
            catch (Exception)
            {
                return null;
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
            try {
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

            } catch(Exception)
            {
                return null; 
            }
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
            try {
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
            }catch(Exception)
            { return null; }
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



     



    }
}
