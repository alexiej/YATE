using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace YATE
{
    public class DocumentPaginatorWrapper : DocumentPaginator
    {
        Size m_PageSize;
        Size m_Margin;
        DocumentPaginator m_Paginator;
        Typeface m_Typeface;

        public DocumentPaginatorWrapper(DocumentPaginator paginator, Size pageSize, Size margin)
        {
            m_PageSize = pageSize;
            m_Margin = margin;
            m_Paginator = paginator;

            m_Paginator.PageSize = new Size(m_PageSize.Width - margin.Width * 2,
                                            m_PageSize.Height - margin.Height * 2);
        }

        Rect Move(Rect rect)
        {
            if (rect.IsEmpty)
            {
                return rect;
            }
            else
            {
                return new Rect(rect.Left + m_Margin.Width, rect.Top + m_Margin.Height,
                                rect.Width, rect.Height);
            }
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            DocumentPage page = m_Paginator.GetPage(pageNumber);

            // Create a wrapper visual for transformation and add extras
            ContainerVisual newpage = new ContainerVisual();

            DrawingVisual title = new DrawingVisual();

            using (DrawingContext ctx = title.RenderOpen())
            {
                if (m_Typeface == null)
                {
                    m_Typeface = new Typeface("Times New Roman");
                }

                FormattedText text = new FormattedText("Page " + (pageNumber + 1),
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    m_Typeface, 14, Brushes.Black);

                ctx.DrawText(text, new Point(0, -96 / 4)); // 1/4 inch above page content
            }

            DrawingVisual background = new DrawingVisual();

            using (DrawingContext ctx = background.RenderOpen())
            {
                ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 240, 240)), null, page.ContentBox);
            }

            newpage.Children.Add(background); // Scale down page and center

            ContainerVisual smallerPage = new ContainerVisual();
            smallerPage.Children.Add(page.Visual);
            smallerPage.Transform = new MatrixTransform(0.95, 0, 0, 0.95,
                0.025 * page.ContentBox.Width, 0.025 * page.ContentBox.Height);

            newpage.Children.Add(smallerPage);
            newpage.Children.Add(title);

            newpage.Transform = new TranslateTransform(m_Margin.Width, m_Margin.Height);

            return new DocumentPage(newpage, m_PageSize, Move(page.BleedBox), Move(page.ContentBox));
        }

        public override bool IsPageCountValid
        {
            get
            {
                return m_Paginator.IsPageCountValid;
            }
        }

        public override int PageCount
        {
            get
            {
                return m_Paginator.PageCount;
            }
        }

        public override Size PageSize
        {
            get
            {
                return m_Paginator.PageSize;
            }

            set
            {
                m_Paginator.PageSize = value;
            }
        }

        public override IDocumentPaginatorSource Source
        {
            get
            {
                return m_Paginator.Source;
            }
        }
    }
}
