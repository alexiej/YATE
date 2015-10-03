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
    /// Interaction logic for ButtonImg48.xaml
    /// </summary>
    public partial class ButtonImg48 : UserControl
    {
        public ButtonImg48()
        {
            InitializeComponent();
        }

        public Canvas Image
        {
            get { return (Canvas)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(Canvas), typeof(ButtonImg48), new UIPropertyMetadata(null));

        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null) Click(sender, e);
        }
    }
}
