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
    /// Interaction logic for FontPicker.xaml
    /// </summary>
    public partial class FontPicker : UserControl
    {
        //
        // Summary:
        //     Occurs when the selection of a System.Windows.Controls.Primitives.Selector changes.
        [System.ComponentModel.Category("Behavior")]
        public event SelectionChangedEventHandler SelectionChanged;

        public object SelectedItem
        {
            get
            {
                return cb_Combo.SelectedItem;
            }
            set
            {
                cb_Combo.SelectedItem = value;
            }
        }

        public FontPicker()
        {
            InitializeComponent();

            
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged!=null)
            {
                SelectionChanged.Invoke(this, e);

               
            }
        }
    }
}
