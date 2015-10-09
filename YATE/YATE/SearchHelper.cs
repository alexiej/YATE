using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace YATE
{
    public class SearchHelper
    {
        RichTextBox editor;
        AdornerLayer al;

        List<SelectionHighlightAdorner> selection_adoner = new List<SelectionHighlightAdorner>();

        public SearchHelper(RichTextBox editor)
        {
            this.editor = editor;
        }

        public void InitSearchHelper()
        {
            ClearSearchHelper();
            al = AdornerLayer.GetAdornerLayer(editor);
        }

        public void AddSearchHelper(TextRange tr)
        {
            SelectionHighlightAdorner sha = new SelectionHighlightAdorner(editor, tr);
            selection_adoner.Add(sha);
            al.Add(sha);
            al.Update();
        }

        public void UpdateSearchHelper()
        {
            if (al == null) al = AdornerLayer.GetAdornerLayer(editor); ;
            al.InvalidateVisual();
            al.Update();
        }


        public void ClearSearchHelper()
        {
            if(al==null) al = AdornerLayer.GetAdornerLayer(editor); ;
            foreach (SelectionHighlightAdorner sha in selection_adoner)
            {
                al.Remove(sha);
            }
            selection_adoner.Clear();
            al.Update();
        }


    }
}
