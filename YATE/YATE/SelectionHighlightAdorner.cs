using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace YATE
{
    public class SelectionHighlightAdorner : Adorner
    {
        RichTextBox _editor;
        TextRange _tr;
        Rect _rect = new Rect();


        public static readonly SolidColorBrush HighlightBrush = new SolidColorBrush(Colors.DarkOrange);



        public SelectionHighlightAdorner(RichTextBox editor,TextRange tr)
            : base(editor)
        {
            _editor = editor;
            _tr = tr;

            _Visuals = new VisualCollection(this);
            _ContentPresenter = new ContentPresenter();
            _Visuals.Add(_ContentPresenter);

        }


        private VisualCollection _Visuals;
        private ContentPresenter _ContentPresenter;


        protected override Size MeasureOverride(Size constraint)
        {
            _ContentPresenter.Measure(constraint);
            return _ContentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _ContentPresenter.Arrange(new Rect(0, 0,
                 finalSize.Width, finalSize.Height));
            return _ContentPresenter.RenderSize;
        }

        protected override Visual GetVisualChild(int index)
        { return _Visuals[index]; }

        protected override int VisualChildrenCount
        { get { return _Visuals.Count; } }

        public object Content
        {
            get { return _ContentPresenter.Content; }
            set { _ContentPresenter.Content = value; }
        }

        private bool isOutside(Rect rect)
        {
            return (rect.X < 0 || rect.Y < 0 || rect.X > _editor.ActualWidth || rect.Y > _editor.ActualHeight);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
           Rect left = _tr.Start.GetCharacterRect(LogicalDirection.Forward);

            Rect right = _tr.End.GetCharacterRect(LogicalDirection.Backward);



          
            _rect.X = left.X;
            _rect.Width = right.Right - left.Left;
            _rect.Height = right.Bottom - left.Top;
            _rect.Y = right.Y;

            if (isOutside(_rect) ) return;


            drawingContext.PushOpacity(0.7);
                drawingContext.DrawRectangle(HighlightBrush, null, _rect);
            drawingContext.Pop();



            //if (_editor.rtb.SelectionLength > 0 && !_editor.IsKeyboardFocused && !_editor.ShouldSuppressSelectionHighlightAdorner)
            //{
            //drawingContext.PushClip(new RectangleGeometry(new System.Windows.Rect(0, 0, _editor.ActualWidth, _editor.ActualHeight)));



            //int firstCharIndex = _editor.SelectionStart;
            //int lastCharIndex = firstCharIndex + _editor.SelectionLength;
            //var firstCharRect = _editor.GetRectFromCharacterIndex(firstCharIndex);
            //var lastCharRect = _editor.GetRectFromCharacterIndex(lastCharIndex);

            //var highlightGeometry = new GeometryGroup();
            //if (firstCharRect.Top == lastCharRect.Top)
            //{
            //    // single line selection
            //    highlightGeometry.Children.Add(new RectangleGeometry(new Rect(firstCharRect.TopLeft, lastCharRect.BottomRight)));
            //}
            //else
            //{
            //    int firstVisibleLine = _editor.GetFirstVisibleLineIndex();
            //    int lastVisibleLine = _editor.GetLastVisibleLineIndex();
            //    if (_editor.GetLineIndexFromCharacterIndex(firstCharIndex) < firstVisibleLine)
            //    {
            //        firstCharIndex = _editor.GetCharacterIndexFromLineIndex(firstVisibleLine - 1);
            //        firstCharRect = _editor.GetRectFromCharacterIndex(firstCharIndex);
            //    }
            //    if (_editor.GetLineIndexFromCharacterIndex(lastCharIndex) > lastVisibleLine)
            //    {
            //        lastCharIndex = _editor.GetCharacterIndexFromLineIndex(lastVisibleLine + 1);
            //        lastCharRect = _editor.GetRectFromCharacterIndex(lastCharIndex);
            //    }

            //    var lineHeight = firstCharRect.Height;
            //    var lineCount = (int)Math.Round((lastCharRect.Top - firstCharRect.Top) / lineHeight);
            //    var lineLeft = firstCharRect.Left;
            //    var lineTop = firstCharRect.Top;
            //    var currentCharIndex = firstCharIndex;
            //    for (int i = 0; i <= lineCount; i++)
            //    {
            //        var lineIndex = _editor.GetLineIndexFromCharacterIndex(currentCharIndex);
            //        var firstLineCharIndex = _editor.GetCharacterIndexFromLineIndex(lineIndex);
            //        var lineLength = _editor.GetLineLength(lineIndex);
            //        var lastLineCharIndex = firstLineCharIndex + lineLength - 1;
            //        if (lastLineCharIndex > lastCharIndex)
            //        {
            //            lastLineCharIndex = lastCharIndex;
            //        }
            //        var lastLineCharRect = _editor.GetRectFromCharacterIndex(lastLineCharIndex);
            //        var lineWidth = lastLineCharRect.Right - lineLeft;
            //        if (Math.Round(lineWidth) <= 0)
            //        {
            //            lineWidth = 5;
            //        }
            //        highlightGeometry.Children.Add(new RectangleGeometry(new Rect(lineLeft, lineTop, lineWidth, lineHeight)));
            //        currentCharIndex = firstLineCharIndex + lineLength;
            //        var nextLineFirstCharRect = _editor.GetRectFromCharacterIndex(currentCharIndex);
            //        lineLeft = nextLineFirstCharRect.Left;
            //        lineTop = nextLineFirstCharRect.Top;
            //    }
            //}

            //drawingContext.PushOpacity(0.4);
            //drawingContext.DrawGeometry(System.Windows.SystemColors.HighlightBrush, null, highlightGeometry);
        }
    
    }
}
