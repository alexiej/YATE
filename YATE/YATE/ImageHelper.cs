using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Semagsoft
{
    public class ImageHelper
    {
        AdornerLayer al;

        static ResizingAdorner current = null;

        public void ChangeImageResizers(RichTextBox editor, Image im)
        {
            AdornerLayer al  = AdornerLayer.GetAdornerLayer(editor);

            if (current != null)
            {
                al.Remove(current);
            }
            current = new ResizingAdorner(im);
            al.Add(current);
            al.Update();

        }

        public void ClearImageResizers(RichTextBox editor)
        {
            if (current != null)
            {
                current.ClearTumb();
                AdornerLayer al = AdornerLayer.GetAdornerLayer(editor);
                al.Remove(current);
                al.Update();
                current = null;
            }

        }

        public void AddImageResizers(RichTextBox editor)
        {
            var images = GetVisuals(editor).OfType<Image>();
            al = AdornerLayer.GetAdornerLayer(editor);
            foreach (var image in images)
            {
                current = new ResizingAdorner(new ResizingAdorner(image));
                //ResizingAdorner ral = new ResizingAdorner(image);
                al.Add(current);
                al.Update();
                //LIBTODO:
            }
        }

        static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

       
    }
}