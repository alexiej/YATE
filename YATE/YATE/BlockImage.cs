using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;


//[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "DocEdLib")]

//namespace DocEdLib
//{
//    [ContentProperty("Base64Source")]
//    public class BlockImage : BlockUIContainer
//    {

//        [Browsable(false),
//      Bindable(false),
//      DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
//      EditorBrowsable(EditorBrowsableState.Never)]
//        public new UIElement Child
//        {
//            get { return base.Child; }
//            set { base.Child = value; }
//        }



//        public BlockImage()
//        {
//        }

//        public BlockImage(Uri uri, double w, double h)
//        {
//            using (var outStream = new MemoryStream())
//            {

//                BitmapEncoder enc = new BmpBitmapEncoder();
//                BitmapFrame bf = BitmapFrame.Create(uri);
//                enc.Frames.Add(bf);
//                enc.Save(outStream);
//                if (w > 0) this.Width = w;
//                else this.Width = bf.Width;
//                if (h > 0) this.Height = h;
//                else this.Height = bf.Height;
//                this.Base64Source = Convert.ToBase64String(outStream.ToArray());
//            }
//        }

//        public BlockImage(Stream b, double w, double h)
//        {
//            using (var outStream = new MemoryStream())
//            {

//                var img = new System.Windows.Media.Imaging.BitmapImage();
//                img.BeginInit();
//                img.StreamSource = b;
//                img.EndInit();

//                BitmapEncoder enc = new BmpBitmapEncoder();
//                BitmapFrame bf = BitmapFrame.Create(img);
//                enc.Frames.Add(bf);
//                enc.Save(outStream);
//                if (w > 0) this.Width = w;
//                else this.Width = bf.Width;
//                if (h > 0) this.Height = h;
//                else this.Height = bf.Height;
//                this.Base64Source = Convert.ToBase64String(outStream.ToArray());
//            }
//        }

//        public BlockImage(byte[] b, double w, double h)
//        {
//            if (w <= 0 || h <= 0)
//            {
//                using (var outStream = new MemoryStream())
//                {
//                    var img = new System.Windows.Media.Imaging.BitmapImage();
//                    img.BeginInit();
//                    img.StreamSource = new MemoryStream(b);
//                    img.EndInit();

//                    BitmapEncoder enc = new BmpBitmapEncoder();
//                    BitmapFrame bf = BitmapFrame.Create(img);
//                    enc.Frames.Add(bf);
//                    enc.Save(outStream);
//                    if (w > 0) this.Width = w;
//                    else this.Width = bf.Width;


//                    if (h > 0) this.Height = h;
//                    else this.Height = bf.Height;
//                    this.Stretch = Stretch.Fill;

//                    this.Base64Source = Convert.ToBase64String(outStream.ToArray());
//                }
//            }
//            else
//            {
//                this.Base64Source = Convert.ToBase64String(b);
//                this.Stretch = Stretch.Fill;
//                this.Width = w;
//                this.Height = h;
//            }
//        }


//        public BlockImage(BitmapSource img)
//        {
//            //  this.Base64Source = @"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAB3RJTUUH2AQPSFlzAAALEgAACxIB0t1+/AAAAARnQU1BAACxjwv8YQUAAAnOSURBVHjaxVcLcBvVFT1vVki3Hju3GCQnGjkObONQkJkxCSIHQQGnIdEr5TFs+LaGl7RRCSUvDp8nglH4mDGQ6EwZIm=";

//            using (var outStream = new MemoryStream())
//            {
//                BitmapEncoder enc = new BmpBitmapEncoder();
//                enc.Frames.Add(BitmapFrame.Create(img));
//                enc.Save(outStream);
//                this.Width = img.Width;
//                this.Height = img.Height;
//                this.Base64Source = Convert.ToBase64String(outStream.ToArray());
//            }
//            //    System.Convert.ToBase64String(img);
//            //   var stream = new MemoryStream(Convert.FromBase64String());
//        }


//        #region DependencyProperty 'Width'

//        /// <summary>
//        /// Gets or sets the width.
//        /// </summary>
//        public double Width
//        {
//            get { return (double)GetValue(WidthProperty); }
//            set { SetValue(WidthProperty, value); }
//        }

//        /// <summary>
//        /// Registers a dependency property to get or set the width
//        /// </summary>
//        public static readonly DependencyProperty WidthProperty =
//            DependencyProperty.Register("Width", typeof(double),
//            typeof(BlockImage),
//            new FrameworkPropertyMetadata(Double.NaN));

//        #endregion

//        #region DependencyProperty 'Height'

//        /// <summary>
//        /// Gets or sets the height.
//        /// </summary>
//        public double Height
//        {
//            get { return (double)GetValue(HeightProperty); }
//            set { SetValue(HeightProperty, value); }
//        }

//        /// <summary>
//        /// Registers a dependency property to get or set the height
//        /// </summary>
//        public static readonly DependencyProperty HeightProperty =
//            DependencyProperty.Register("Height", typeof(double),
//            typeof(BlockImage),
//            new FrameworkPropertyMetadata(Double.NaN));

//        #endregion

//        #region DependencyProperty 'Stretch'

//        /// <summary>
//        /// Gets or sets the stretch behavior.
//        /// </summary>
//        public Stretch Stretch
//        {
//            get { return (Stretch)GetValue(StretchProperty); }
//            set { SetValue(StretchProperty, value); }
//        }

//        /// <summary>
//        /// Registers a dependency property to get or set the stretch behavior
//        /// </summary>
//        public static readonly DependencyProperty StretchProperty =
//            DependencyProperty.Register("Stretch", typeof(Stretch),
//            typeof(BlockImage),
//            new FrameworkPropertyMetadata(Stretch.Uniform));

//        #endregion

//        #region DependencyProperty 'StretchDirection'

//        /// <summary>
//        /// Gets or sets the stretch direction.
//        /// </summary>
//        public StretchDirection StretchDirection
//        {
//            get { return (StretchDirection)GetValue(StretchDirectionProperty); }
//            set { SetValue(StretchDirectionProperty, value); }
//        }

//        /// <summary>
//        /// Registers a dependency property to get or set the stretch direction
//        /// </summary>
//        public static readonly DependencyProperty StretchDirectionProperty =
//            DependencyProperty.Register("StretchDirection", typeof(StretchDirection),
//            typeof(BlockImage),
//            new FrameworkPropertyMetadata(StretchDirection.Both));

//        #endregion

//        #region DependencyProperty 'Base64Source'

//        /// <summary>
//        /// Gets or sets the base64 source.
//        /// </summary>

//        public string Base64Source
//        {
//            get { return (string)GetValue(Base64SourceProperty); }
//            set { SetValue(Base64SourceProperty, value); }
//        }

//        /// <summary>
//        /// Registers a dependency property to get or set the base64 source
//        /// </summary>
//        public static readonly DependencyProperty Base64SourceProperty =
//            DependencyProperty.Register("Base64Source", typeof(string), typeof(BlockImage),
//            new FrameworkPropertyMetadata(null, OnBase64SourceChanged));

//        #endregion

//        #region Private Members

//        private static void OnBase64SourceChanged(DependencyObject sender,
//            DependencyPropertyChangedEventArgs e)
//        {
//            var BlockImage = (BlockImage)sender;
//            var stream = new MemoryStream(Convert.FromBase64String(BlockImage.Base64Source));


//            var bitmapImage = new BitmapImage();
//            bitmapImage.BeginInit();
//            bitmapImage.StreamSource = stream;
//            bitmapImage.EndInit();

//            var image = new Image
//            {
//                Source = bitmapImage,
//                Stretch = BlockImage.Stretch,
//                StretchDirection = BlockImage.StretchDirection,
//            };

//            if (!double.IsNaN(BlockImage.Width))
//            {
//                image.Width = BlockImage.Width;
//            }

//            if (!double.IsNaN(BlockImage.Height))
//            {
//                image.Height = BlockImage.Height;
//            }

//            BlockImage.Child = image;

//        }

//        #endregion

//    }

//}
