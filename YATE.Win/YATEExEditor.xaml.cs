﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for YATEExEditor.xaml 
    /// </summary>
    public partial class YATEExEditor : UserControl 
    {
        public YATEExEditor()
        {
            InitializeComponent();
            yate.rtb.SelectionChanged += Rtb_SelectionChanged;


            _CurrentAlign = bn_AlignLeft;
            yate.PreviewKeyDown += Yate_PreviewKeyDown;
            UpdateVisualState();
        }

        public YATEditor Editor
        {
            get { return this.yate; }
        }



        public bool isCtrl()
        {
            return (Keyboard.Modifiers == ModifierKeys.Control);
        }


        public bool isCtrlShift()
        {
            return (Keyboard.Modifiers == 
                (ModifierKeys.Control | ModifierKeys.Shift));
        }

        private ButtonToggle _CurrentAlign ;
        public ButtonToggle CurrentAlign
        {
            get { return _CurrentAlign;  }
            set {
                _CurrentAlign.isChecked = false; value.isChecked = true;
                _CurrentAlign = value;
            }
        }

        public void Switch(ButtonToggle bt)
        {
            bt.isChecked = !bt.isChecked;
        }

        public void SwitchMarkerStyle(TextMarkerStyle tms)
        {
            switch(tms)
            {

                case TextMarkerStyle.Disc:
                    bn_Numeric.isChecked = false;
                    bn_Bullet.isChecked = true;
                    break;
                case TextMarkerStyle.Decimal:
                    bn_Numeric.isChecked = true;
                    bn_Bullet.isChecked = false;
                    break;
                default:
                    bn_Numeric.isChecked = false;
                    bn_Bullet.isChecked = false;
                    break;
            }
        }


        private void Yate_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isCtrl())
            {
                switch (e.Key)
                {
                    case Key.B:
                        Switch(bn_Bold);
                        break;
                    case Key.I:
                        Switch(bn_Italic); 
                        break;
                    case Key.U:
                        Switch(bn_Underline);
                        break;
                    case Key.L:
                        CurrentAlign = bn_AlignLeft;
                        break;
                    case Key.E:
                        CurrentAlign = bn_AlignCenter;
                        break;
                    case Key.R:
                        CurrentAlign = bn_AlignRight;
                        break;
                    case Key.J:
                        CurrentAlign = bn_AlignJustify;
                        break;
                }
            }
            else if (isCtrlShift())
            {
                switch(e.Key)
                {
                    case Key.L:
                        SwitchMarkerStyle(TextMarkerStyle.Disc);
                        break;
                    case Key.N:
                        SwitchMarkerStyle(TextMarkerStyle.Decimal);
                        break;
                }
            
            }
        }

       

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisualState();
        }

      

        #region Update View

        bool isUpdateVisualStyle = false;

        private void UpdateVisualState()
        {
            isUpdateVisualStyle = true;
               UpdateToggleButtonState();
               UpdateAlignState();
               SwitchMarkerStyle(yate.SelectionMarkerStyle);
               cb_FontName.SelectedItem = yate.SelectionFontFamily;
               cb_FontSIze.SelectedItem = yate.SelectionFontSizePoints;
            isUpdateVisualStyle = false;

        }

       

        private void UpdateAlignState()
        {
            switch(yate.SelectionAligment)
            {
                case TextAlignment.Left: CurrentAlign = bn_AlignLeft; return;
                case TextAlignment.Center: CurrentAlign = bn_AlignCenter; return;
                case TextAlignment.Right: CurrentAlign = bn_AlignRight; return;
                case TextAlignment.Justify: CurrentAlign = bn_AlignJustify; return;
            }
        }

        private void UpdateToggleButtonState()
        {
            bn_Bold.isChecked = yate.isBold;
            bn_Italic.isChecked = yate.isItalic;
            bn_Underline.isChecked = yate.isUnderline;
        }

        #endregion


        #region Buttons

        private void cb_FontName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdateVisualStyle) return;
            yate.SelectionFontFamily = (FontFamily)cb_FontName.SelectedItem;
            yate.Focus();

        }

        private void cb_FontSIze_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdateVisualStyle) return;

            yate.SelectionFontSizePoints = cb_FontSIze.SelectedItem;
            yate.Focus();
        }

        private void bn_Cut_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdCut();
           
        }

        private void bn_Copy_Click(object sender, RoutedEventArgs e)
        {
            yate.cmdCopy();
            yate.Focus();
        }

        private void bn_Paste_Click(object sender, RoutedEventArgs e)
        {
            yate.cmdPaste();
            yate.Focus();

        }

        private void bn_PasteText_Click(object sender, RoutedEventArgs e)
        {
            yate.cmdPasteText();
            yate.Focus();
        }

        private void bn_Undo_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();

            yate.cmdUndo();
        }


        private void bn_Redo_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdRedo();
       

        }

        private void bn_Bold_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();

            yate.cmdBold();
        }

        private void bn_Italic_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdItalic();
        }

        private void bn_Underline_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdUnderline();
        }

        private void bn_AlignLeft_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            CurrentAlign = bn_AlignLeft;
            yate.cmdExecute(EditingCommands.AlignLeft); 
        }

        private void bn_AlignCenter_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            CurrentAlign = bn_AlignCenter;
            yate.cmdExecute(EditingCommands.AlignCenter); 
        }

        private void bn_AlignRight_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            CurrentAlign = bn_AlignRight;
            yate.cmdExecute(EditingCommands.AlignRight);
        }


        private void bn_AlignJustify_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            CurrentAlign = bn_AlignJustify;
            yate.cmdExecute(EditingCommands.AlignJustify);
 
        }

        private void bn_Numeric_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdExecute(EditingCommands.ToggleNumbering);

            bn_Bullet.isChecked = false;


        }

        private void bn_Bullet_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdExecute(EditingCommands.ToggleBullets);
            bn_Numeric.isChecked = false;
        }

        private void bn_DecIntend_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdExecute(EditingCommands.DecreaseIndentation);

        }


        private void bn_IncIntend_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.cmdExecute(EditingCommands.IncreaseIndentation);
        }

        private void bn_FontColorChange_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus();
            yate.SelectionForeground = bn_FontColorChange.CurrentColor;
           

        }

        private void bn_BackColorChange_Click(object sender, RoutedEventArgs e)
        {
            yate.Focus(); yate.SelectionBackground = bn_BackColorChange.CurrentColor;
        }

        #endregion

        private void bn_Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.DefaultExt = ".*";
            //dlg.Filter = "ALL Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();


            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                FileStream fs = new FileStream(filename,FileMode.Open,FileAccess.Read);
                yate.Load(fs);
            }
        }

        private void bn_Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();


            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                string filename = dlg.FileName;
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                yate.Save(fs);
            }

        }
    }
}
