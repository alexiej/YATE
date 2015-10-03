using System;
using System.Windows;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Semagsoft
{
    public static class HyperlinkHelper
    {
        public static void SubscribeToAllHyperlinks(FlowDocument flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
            {
                SubscribellHyperlink(link);
            }
        }

        public static void SubscribellHyperlink(Hyperlink link)
        {
            link.Cursor = System.Windows.Input.Cursors.Hand;
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

        static void link_Click(object sender, RoutedEventArgs e)
        {
           System.Diagnostics.Process.Start(new ProcessStartInfo(
                  (sender as Hyperlink).NavigateUri.AbsoluteUri));

        }

        static void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}