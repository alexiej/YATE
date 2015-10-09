using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YATE
{
    class YATEHelper
    {
        public static readonly Thickness Thickness0 = new Thickness(0.0);
        public static readonly Thickness Thickness1 = new Thickness(1.0);
        public static readonly SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush SilverBrush = new SolidColorBrush(Colors.Silver);


        [Flags]
        public enum FindFlags
        {
            FindInReverse = 2,
            FindWholeWordsOnly = 4,
            MatchAlefHamza = 0x20,
            MatchCase = 1,
            MatchDiacritics = 8,
            MatchKashida = 0x10,
            None = 0
        }

        private static MethodInfo findMethod = null;


        public static TextRange FindText(TextPointer findContainerStartPosition, TextPointer findContainerEndPosition, String input, FindFlags flags, CultureInfo cultureInfo)
        {
            TextRange textRange = null;
            if (findContainerStartPosition.CompareTo(findContainerEndPosition) < 0)
            {
                try
                {
                    if (findMethod == null)
                    {
                        findMethod = typeof(FrameworkElement).Assembly.GetType("System.Windows.Documents.TextFindEngine").
                               GetMethod("Find", BindingFlags.Static | BindingFlags.Public);
                    }
                    Object result = findMethod.Invoke(null, new Object[] { findContainerStartPosition,
                    findContainerEndPosition,
                    input, flags, CultureInfo.CurrentCulture });
                    textRange = result as TextRange;
                }
                catch (ApplicationException)
                {
                    textRange = null;
                }
            }

            return textRange;
        }
    



    public static void AdjustWidth(Image i, double maxw)
        {
            if (i.Width < maxw) return;
            i.Height = (maxw / i.Width) * i.Height;
            i.Width = maxw;

        }


        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }


        public static Image ImageFromSpan(Span element)
        {
            return ((element).Inlines.First() as InlineUIContainer).Child as Image;
        }

        public static BitmapSource ConvertToImage(byte[] bytes)
        {
            BitmapImage image = null;
            //  System.Drawing.Bitmap bmpImage = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream(bytes));
            try {
                MemoryStream stream = new MemoryStream(bytes);
                stream.Seek(0, SeekOrigin.Begin);
                image = new BitmapImage();

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception)
            {
                return null;
            }
            return image;
        }


        //private static readonly Regex UrlRegex = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&amp;(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");

        public static bool IsHyperlink(ref string word,out Uri uriResult)
        {
           uriResult = null;
           word =  word.Trim();
            // First check to make sure the word has at least one of the characters we need to make a hyperlink
            if (word.IndexOfAny(@":.\/".ToCharArray()) != -1)
            {
                if (word.EndsWith(".")) return false;

                if (Uri.IsWellFormedUriString(word, UriKind.Absolute))
                {
                    if (!char.IsLetter(word[word.Length - 1])) return false;
                    // The string is an Absolute URI
                    return Uri.TryCreate(word, UriKind.RelativeOrAbsolute, out uriResult); 
                }
                bool result = Uri.TryCreate(word, UriKind.RelativeOrAbsolute, out uriResult);
                if (result == false) return false;
                if (uriResult.IsAbsoluteUri) return true;

                if (word.Contains("@"))
                {
                    word = @"mailto:" + word;
                }
                else
                {
                    if (word.StartsWith("www.") == false) return false;

                    word = @"http://" + word;
                }
                return Uri.TryCreate(word, UriKind.Absolute, out uriResult);

                //if (UrlRegex.IsMatch(word))
                //{
                //    //Uri uri = new Uri(word, UriKind.RelativeOrAbsolute);
                //    if (result == false) return false;
                //    if (!uri.IsAbsoluteUri)
                //    {
                //       
                //        bool result = Uri.TryCreate(word, UriKind.Absolute, out uriResult);
                //      //      uriResult.Scheme == Uri.UriSa;
                //        // rebuild it it with http to turn it into an Absolute URI
                //        //uri = new Uri(word, UriKind.Absolute);
                //        return true;
                //    }

                //    if (uri.IsAbsoluteUri)
                //    {
                //        return true;
                //    }
                //}
                //else
                //{
                //    return false;
                //    //Uri wordUri = new Uri(word);

                //    //// Check to see if URL is a network path
                //    //if (wordUri.IsUnc || wordUri.IsFile)
                //    //{
                //    //    return true;
                //    //}
                //}
            }

            return false;
        }

        public static Span GetImage(BitmapSource image,
            ImageContentType contnt_type = ImageContentType.ImagePngContentType,
            Stretch stretch = Stretch.Fill)
        {
            if (image == null) return null;
            Stream packagedImage = WpfPayload.SaveImage(image, contnt_type);
            object element = WpfPayload.LoadElement(packagedImage);
            return element as Span;
        }


        public static Stream GetImageStream(BitmapSource image,
           ImageContentType contnt_type = ImageContentType.ImagePngContentType,
           Stretch stretch = Stretch.Fill)
        {
            if (image == null) return null;
            return WpfPayload.SaveImage(image, contnt_type);
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
