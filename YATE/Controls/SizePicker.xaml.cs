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
    /// Interaction logic for SizePicker.xaml
    /// </summary>
    public partial class SizePicker : UserControl
    {
        //
        // Summary:
        //     Occurs when the selection of a System.Windows.Controls.Primitives.Selector changes.
        [System.ComponentModel.Category("Behavior")]
        public event SelectionChangedEventHandler SelectionChanged;


        public static readonly string[] FontSize =
        {
            "8","9","10","11","12","14","16","18","20","22","24","26","28","36","48","72"
        };

        public string SelectedItem
        {
            get
            {
                return (string)cb_list.SelectedItem;
            }
            set
            {
                cb_list.SelectedItem = (string)value;
            }
        }



        public SizePicker()
        {
            InitializeComponent();

           foreach(string fs in FontSize)
            {
                cb_list.Items.Add(fs);
            }

            cb_list.SelectedItem = "12";
        }

        private void cb_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(this, e);
            }
        }
    }
}
