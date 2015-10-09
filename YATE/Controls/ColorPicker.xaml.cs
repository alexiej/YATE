using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace YATE
{

    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
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

       
        public SolidColorBrush CurrentColor
        {
            get { return (SolidColorBrush)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        public static DependencyProperty CurrentColorProperty =
            DependencyProperty.Register("CurrentColor", typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(Brushes.Black));

        public static RoutedUICommand SelectColorCommand = new RoutedUICommand("SelectColorCommand", "SelectColorCommand", typeof(ColorPicker));
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
