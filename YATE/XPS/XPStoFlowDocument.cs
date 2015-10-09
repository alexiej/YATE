using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Xps.Serialization;

namespace DocEdLib.XPS
{
    public partial class XPStoFlowDocument
    {
        public static FlowDocument Convert(string filename)
        {
            FlowDocument fdoc = new FlowDocument();

            Package pkg = Package.Open(filename, FileMode.Open, FileAccess.Read);
            string pack = "pack://temp.xps";
            PackageStore.AddPackage(new Uri(pack), pkg);
            XpsDocument _doc = new XpsDocument(pkg, CompressionOption.Fast, pack);

            /*foreach (DocumentReference dr in _doc.GetFixedDocumentSequence().References)
            {
                foreach (PageContent pc in dr.GetDocument(false).Pages)
                {
                    FixedPage fp = pc.GetPageRoot(false);
                    BlockUIContainer cont = new BlockUIContainer();
                    cont.Child = fp;
                    fdoc.Blocks.Add(cont);
                }
            }
            viewer.Document = fdoc;
            this.Content = viewer;
            return;*/

            IXpsFixedDocumentSequenceReader fixedDocSeqReader = _doc.FixedDocumentSequenceReader;


            
            Dictionary<string, string> fontList = new Dictionary<string, string>();

            foreach (IXpsFixedDocumentReader docReader in fixedDocSeqReader.FixedDocuments)
            {
                foreach (IXpsFixedPageReader fixedPageReader in docReader.FixedPages)
                {
                    while (fixedPageReader.XmlReader.Read())
                    {

                        string page = fixedPageReader.XmlReader.ReadOuterXml();
                        string path = string.Empty;

                        foreach (XpsFont font in fixedPageReader.Fonts)
                        {
                            
                            string name = font.Uri.GetFileName();
                            path = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), name);

                            if (!fontList.ContainsKey(font.Uri.OriginalString))
                            {
                                fontList.Add(font.Uri.OriginalString, path);
                                font.SaveToDisk(path);
                            }
                        }

                        foreach (XpsImage image in fixedPageReader.Images)
                        {
                            //here to get images
                        }

                        foreach(KeyValuePair<string,string> val in fontList) 
                        {
                            page = page.ReplaceAttribute("FontUri", val.Key,val.Value); 
                        }


                        FixedPage fp = XamlReader.Load(new MemoryStream(Encoding.Default.GetBytes(page))) as FixedPage;

                        /*fp.Children.OfType<Glyphs>().ToList().ForEach(glyph =>
                            {
                                Binding b = new Binding();
                                b.Source = glyph;
                                b.Path = new PropertyPath(Glyphs.UnicodeStringProperty);
                                glyph.SetBinding(TextSearch.TextProperty, b);
                            });*/

                        BlockUIContainer cont = new BlockUIContainer();
                        cont.Child = fp;
                        fdoc.Blocks.Add(cont);

                        //string outp = XamlWriter.Save(fp);
                    }

                }
            }

            return fdoc;

        }
    }
}
