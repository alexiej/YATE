using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for AddTable.xaml
    /// 
    /// 
    /// </summary>
    public partial class AddTable : UserControl
    {
        public AddTable()
        {
            InitializeComponent();
            InitializeControl();
           
            CommandBindings.Add(new CommandBinding(SelectTableCommand, SelectTableCommandExecute));
        }

        public static readonly int WIDTH = 10;
        public static readonly int HEIGHT = 10;


        public static readonly double SizeWH = 15.0;
        public static readonly GridLength SizeG = new GridLength(SizeWH+3.0);
        public static readonly Thickness tick = new Thickness(2);
        public static readonly Thickness tick0 = new Thickness(0);

        public static readonly SolidColorBrush background = new SolidColorBrush(Colors.White);

        public static readonly SolidColorBrush BorderNo = new SolidColorBrush(Colors.Silver);
        public static readonly SolidColorBrush BorderYes = new SolidColorBrush(Colors.DarkOrange);


        private Button[,] Buttons = new Button[WIDTH, HEIGHT];


        private void InitializeControl()
        {
            for (int i = 0; i < WIDTH; i++) {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = SizeG;
                gd_Grid.ColumnDefinitions.Add(cd);
           }

            for (int i = 0; i < HEIGHT; i++)
            {
                RowDefinition rw = new RowDefinition();
                rw.Height = SizeG;
                gd_Grid.RowDefinitions.Add(rw);
            }


            ///*
            //          <Button Grid.Column="0"
            //                                       Margin="1"
            //                                       Background="#FFFFFF"
            //                                       Click="Button_Click"
            //                                       Command="{x:Static cc:ColorPicker.SelectColorCommand}"
            //                                       CommandParameter="#00b050"
            //                                       Style="{StaticResource ColorButtonStyle}" />
            //       */


          
            for (int i =0;i< WIDTH; i++)
                for(int j=0;j< HEIGHT; j++)
                {
                    KeyValuePair<int,int> pos = new KeyValuePair<int, int>(i+1, j+1);

                    Button b = new Button();
                    b.Tag = pos;
                    b.Click += B_Click;
                   // b.Margin = tick;
                    b.Background = background;
                    b.Command = SelectTableCommand;
                    b.CommandParameter = pos;
                    b.Style = Resources["ColorButtonStyle"] as Style;

                    b.MouseEnter += B_MouseEnter;
                    gd_Grid.Children.Add(b);
                    Grid.SetColumn(b, i);
                    Grid.SetRow(b, j);

                    Buttons[i, j] = b;

                }
            ClearContent();
            gd_Grid.LostFocus += Gd_Grid_LostFocus;
          
        }


        private void Gd_Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            ClearContent();
        }

        private void ClearContent()
        {
            for (int i = 0; i < WIDTH; i++)
                for (int j = 0; j < HEIGHT; j++)
                {
                    Buttons[i, j].BorderBrush = BorderNo;
                }

            lb_Name.Content = "Insert Table";
        }

        

        private void B_MouseEnter(object sender, MouseEventArgs e)
        {
            KeyValuePair<int, int>? pos = (sender as Button).Tag as KeyValuePair<int, int>?;


            int posi = pos.Value.Key-1;
            int posj = pos.Value.Value-1;

            for (int i = 0; i < WIDTH; i++)
                for (int j = 0; j < HEIGHT; j++)
                {
                    if(i<=posi && j<=posj)
                    {
                        Buttons[i, j].BorderBrush = BorderYes;
                    }
                    else
                    {
                        Buttons[i, j].BorderBrush = BorderNo;
                    }
                  
                }

            lb_Name.Content = string.Format("{0}x{1} Table", posi+1, posj+1);
        }



        private void AddTableButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearContent();
        }

       
        private void B_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false; ClearContent();
            e.Handled = false;
        }


       public KeyValuePair<int, int>? CurrentSize = new KeyValuePair<int, int>(0, 0);
       public static RoutedUICommand SelectTableCommand = new RoutedUICommand("SelectTableCommand", "SelectTableCommand", typeof(AddTable));


        private void SelectTableCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentSize = e.Parameter as KeyValuePair<int, int>?;//new SolidColorBrush((Color)ColorConverter.ConvertFromString(e.Parameter.ToString()));
            TableChange_Click(this, e);
        }

        public event RoutedEventHandler TableChange;
        private void TableChange_Click(object sender, RoutedEventArgs e)
        {
            if (TableChange != null) TableChange(sender, e);
        }

    }
}
