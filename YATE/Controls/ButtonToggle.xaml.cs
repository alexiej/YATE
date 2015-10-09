using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YATE
{
    /// <summary>
    /// Interaction logic for ButtonToggle.xaml
    /// </summary>
    public partial class ButtonToggle : UserControl
    {
        public ButtonToggle()
        {
            InitializeComponent();
        }

            public Canvas Image
        {
            get { return (Canvas)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(Canvas), typeof(ButtonToggle), new UIPropertyMetadata(null));


        public bool isChecked
        {
            get { return (bool)GetValue(isCheckedProperty); }
            set { SetValue(isCheckedProperty, value); }
        }

        public static readonly DependencyProperty isCheckedProperty =
           DependencyProperty.Register("isChecked", typeof(bool), typeof(ButtonToggle), new UIPropertyMetadata(false));



        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null) Click(sender, e);
        }
    }
}
