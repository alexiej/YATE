using DocEdLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Semagsoft
{
    public class ResizingAdorner : Adorner
    {
        // Resizing adorner uses Thumbs for visual elements.  
        // The Thumbs have built-in mouse input handling.
        Thumb topLeft, topRight, bottomLeft, bottomRight;


        bool ResizeStarted = false;
        //  Thumb current_resize;
        // To store and manage the adorner's visual children.
        VisualCollection visualChildren;

        Brush renderBrush = new SolidColorBrush(Colors.Transparent);
        Pen renderPen = new Pen(new SolidColorBrush(Colors.Blue), .5);


        Rect rect = new Rect(0,0,0,0);

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!ResizeStarted) return;
            //   Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            // Some arbitrary drawing implements.
            /// double renderRadius = 5.0;
            // Draw a circle at each corner.
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);

            drawingContext.PushOpacity(0.7);
                drawingContext.DrawImage( (AdornedElement as Image).Source, rect);
            drawingContext.Pop();

            drawingContext.DrawRectangle(null, renderPen, rect);
            // Rect selectionStartPos_scr, selectionEndPos_scr;
            //selectionStartPos_scr = start.GetCharacterRect(LogicalDirection.Backward);
            //selectionEndPos_scr = end.GetCharacterRect(LogicalDirection.Backward);
            //   selectionEndPos_scr.Union(selectionStartPos_scr);

            //   drawingContext.DrawImage(,)

            //draw box around the word

            //if (!selectionEndPos_scr.IsEmpty)
            //{
            //    selectionEndPos_scr.Inflate(2, 2);

            //    if (selectionEndPos_scr.Bottom < AdornedElement.RenderSize.Height - ((AdornedElement as TextBoxBase).HorizontalScrollBarVisibility == ScrollBarVisibility.Visible ? 16 : 1)
            //     && selectionEndPos_scr.Top > 0
            //     && selectionEndPos_scr.Left > 0 && selectionEndPos_scr.Right < AdornedElement.RenderSize.Width)
            //}

        }


        // Initialize the ResizingAdorner.
        public ResizingAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);


            Image a = (adornedElement as Image);
            if (a == null) return;

            a.StretchDirection = StretchDirection.Both;
            a.Stretch = Stretch.Fill;

            //preview = new Image();
            //preview.Source = (adornedElement as Image).Source;
            //preview.Opacity = 0.9;
            //preview.Visibility = Visibility.Visible;

            //preview.Width = (adornedElement as Image).Width;
            //preview.Height = (adornedElement as Image).Height;


            //   visualChildren.Add(preview);

            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);


            // Add handlers for resizing.
            bottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            topLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            topRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);

            bottomLeft.DragStarted += OnDragStarted;
            bottomRight.DragStarted += OnDragStarted;
            topLeft.DragStarted += OnDragStarted;
            topRight.DragStarted += OnDragStarted;

            bottomLeft.DragCompleted += OnDragCompleted;
            bottomRight.DragCompleted += OnDragCompleted;
            topLeft.DragCompleted += OnDragCompleted;
            topRight.DragCompleted += OnDragCompleted;
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            ResizeStarted = true;
            wh = 0;
            wv = 0;

            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;

            rect.X = 0;
            rect.Y = 0;
            rect.Width = adornedElement.Width;
            rect.Height = adornedElement.Height;

        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
          //  Debug.WriteLine("TopLeft_DragCompleted");

            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;

            double w = adornedElement.Width + wh;
            double h = adornedElement.Height + wv;

            w = (w < MIN_SIZE ? MIN_SIZE : w);
            h = (h < MIN_SIZE ? MIN_SIZE : h);

            adornedElement.Width = w;
            adornedElement.Height = h;
            var parent = adornedElement.Parent;

            //if (parent is InlineImage)
            //{
            //    (parent as InlineImage).Width = w;
            //    (parent as InlineImage).Height = h;


            //}
            //else if (parent is BlockImage)
            //{
            //    (parent as BlockImage).Width = w;
            //    (parent as BlockImage).Height = h;
            //}


               //adornedElement.Parent  =
               //  adornedElement.Height = 

               ResizeStarted = false;

        }



        public void ClearTumb()
        {
           this.visualChildren.Remove(bottomLeft);
            this.visualChildren.Remove(bottomRight);
            this.visualChildren.Remove(topLeft);
            this.visualChildren.Remove(topRight);
        }

        double wv = 0;
        double wh = 0;

        public static readonly double MIN_SIZE = 5;


        // // Handler for resizing from the bottom-left.
        void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;


            wv =  1* args.VerticalChange;
            wh = -1* args.HorizontalChange;


            double w = adornedElement.Width + wh;
            double h = adornedElement.Height + wv;

            rect.X = (w < MIN_SIZE ? adornedElement.Width - MIN_SIZE : -wh);
            rect.Y = 0;


            rect.Width  = (w < MIN_SIZE ? MIN_SIZE : w);
            rect.Height = (h < MIN_SIZE ? MIN_SIZE : h);



            this.InvalidateVisual();
        }

        // Handler for resizing from the bottom-right.
        void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;


            wv =  1* args.VerticalChange;
            wh =  1* args.HorizontalChange;

            rect.X = 0;
            rect.Y = 0;

            double w = adornedElement.Width + wh;
            double h = adornedElement.Height + wv;



            rect.Width = (w < MIN_SIZE ? MIN_SIZE : w); 
            rect.Height = (h < MIN_SIZE ? MIN_SIZE : h);

            this.InvalidateVisual();

            // Ensure that the Width and Height are properly initialized after the resize.
            //        EnforceSize(adornedElement);
            //    FrameworkElement parentElement = adornedElement.Parent as FrameworkElement;
            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            //  adornedElement.Width = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            //  adornedElement.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            //preview.Width         = Math.Max(adornedElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            //preview.Height = Math.Max(args.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
        }



        // Handler for resizing from the top-left.
        void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;


            wv = -1*args.VerticalChange;
            wh = -1*args.HorizontalChange;

            double w = adornedElement.Width + wh;
            double h = adornedElement.Height + wv;

            rect.X = (w < MIN_SIZE ? adornedElement.Width - MIN_SIZE : -wh);
            rect.Y = (h < MIN_SIZE ? adornedElement.Height - MIN_SIZE : -wv);

            rect.Width = (w < MIN_SIZE ? MIN_SIZE : w);
            rect.Height = (h < MIN_SIZE ? MIN_SIZE : h);

            this.InvalidateVisual();
        }


        // Handler for resizing from the top-right.
        void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            wv =  -1 * args.VerticalChange;
            wh =   1 * args.HorizontalChange;

            double w = adornedElement.Width + wh;
            double h = adornedElement.Height + wv;


            rect.X = 0;
            rect.Y = (h < MIN_SIZE ? adornedElement.Height - MIN_SIZE : -wv);

            rect.Width = (w < MIN_SIZE ? MIN_SIZE : w);
            rect.Height = (h < MIN_SIZE ? MIN_SIZE : h);



            this.InvalidateVisual();
        }


       

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element.  
            //double desiredWidth = AdornedElement.DesiredSize.Width;
            //double desiredHeight = AdornedElement.DesiredSize.Height;
            //// adornerWidth & adornerHeight are used for placement as well.
            //double adornerWidth = this.DesiredSize.Width;
            //double adornerHeight = this.DesiredSize.Height;

            //topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            //topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            //bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            //bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            //// Return the final size.
            //return finalSize;


            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element.  
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;

            //Orginal Microsoft code
            //topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            //topRight.Arrange(new Rect(desiredWidth - (adornerWidth / 2), - adornerHeight / 2, adornerWidth, adornerHeight));
            //bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            //bottomRight.Arrange(new Rect(desiredWidth - (adornerWidth / 2), desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));


            topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));


            //topLeft.Arrange(new Rect(-topLeft.Width / 2, -topLeft.Height / 2, topLeft.Width, topLeft.Height));

            //bottomRight.Arrange(new Rect(desiredWidth - bottomRight.Width / 2, desiredHeight - bottomRight.Height / 2, bottomRight.Width, bottomRight.Height));

            //  if(adornerWidth   )
            //   preview.Arrange(new Rect(0, 0, preview.Width, preview.Height));
            //    bottomRight.Arrange(new Rect(0, 0, preview.Width, preview.Height));


            // Return the final size.
            return finalSize;


        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property, 
        // set some appearance properties, and add the elements to the visual tree.
        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            if (cornerThumb != null) return;

            cornerThumb = new Thumb();

            // Set some arbitrary visual characteristics.
            cornerThumb.Cursor = customizedCursor;
            cornerThumb.Height = cornerThumb.Width = 10;
            //cornerThumb.Opacity = 1;
            cornerThumb.Background = new SolidColorBrush(Colors.Blue);
            cornerThumb.BorderThickness = new Thickness(0);
            //cornerThumb.BorderBrush = new SolidColorBrush(Colors.Blue);

            visualChildren.Add(cornerThumb);
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            if (parent != null)
            {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }
        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }
    }

}
