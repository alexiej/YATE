using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace YATE
{
    public class DialogEventArgs : EventArgs
    {
        public System.Windows.Forms.DialogResult DialogResult { get; set; }
        public SolidColorBrush SelectedColor { get; set; }
    }

    /// <summary>
    /// Interaction logic for ColorPickerImage.xaml
    /// </summary>
    public partial class ColorPickerImage : UserControl
    {
        public ColorPickerImage()
        {
            DataContext = this;
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SelectColorCommand, SelectColorCommandExecute));
        }

        public event RoutedEventHandler ColorChange;
        private void ColorChange_Click(object sender, RoutedEventArgs e)
        {
            if (ColorChange != null) ColorChange(sender, e);
        }

        public Canvas Image
        {
            get { return (Canvas)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(Canvas), typeof(ColorPickerImage), new UIPropertyMetadata(null));

        public SolidColorBrush CurrentColor
        {
            get { return (SolidColorBrush)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        public static DependencyProperty CurrentColorProperty =
            DependencyProperty.Register("CurrentColor", typeof(SolidColorBrush), typeof(ColorPickerImage), new PropertyMetadata(Brushes.Black));

        public static RoutedUICommand SelectColorCommand = new RoutedUICommand("SelectColorCommand", "SelectColorCommand", typeof(ColorPickerImage));
        private Window _advancedPickerWindow = null;



        private void SelectColorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(e.Parameter.ToString()));
            ColorChange_Click(this, e);
        }

        private static void ShowModal(Window advancedColorWindow)
        {
            advancedColorWindow.Owner = System.Windows.Application.Current.MainWindow;
            advancedColorWindow.ShowDialog();
        }

        void AdvancedPickerPopUpKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                _advancedPickerWindow.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            e.Handled = false;
        }

        void AdvancedColorPickerDialogDrag(object sender, DragDeltaEventArgs e)
        {
            _advancedPickerWindow.DragMove();
        }

        void AdvancedColorPickerDialogDialogResultEvent(object sender, EventArgs e)
        {
            _advancedPickerWindow.Close();
            var dialogEventArgs = (DialogEventArgs)e;
            if (dialogEventArgs.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            CurrentColor = dialogEventArgs.SelectedColor;
        }

    }
}
