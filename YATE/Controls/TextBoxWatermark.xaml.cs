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
    /// Interaction logic for TextBoxWatermark.xaml
    /// </summary>
    public partial class TextBoxWatermark : UserControl
    {
        public TextBoxWatermark()
        {
            InitializeComponent();


        }

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public new void Focus()
        {
            SearchTermTextBox.Focus();
        }


        public string Text
        {
            get { return SearchTermTextBox.Text; }
        }

        public static readonly DependencyProperty WatermarkProperty =
           DependencyProperty.Register("Watermark", typeof(string), typeof(TextBoxWatermark), new UIPropertyMetadata(""));



        public event TextChangedEventHandler TextChanged;

        public event EventHandler EnterCommand;


        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChanged != null)
                TextChanged(this, e);
        }

        private void SearchTermTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && EnterCommand!=null)
            {
                EnterCommand(this, e);
            }
        }
    }
}
