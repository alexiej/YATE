using Semagsoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;


namespace YATE
{
    /// <summary>
    /// Interaction logic for YATEditor.xaml
    /// </summary>
    public partial class YATEditor
    {
        private bool _cmdIsRun = false;

        /// <summary>
        /// Gets a value indicating whether [command is run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [command is run]; otherwise, <c>false</c>.
        /// </value>
        public bool cmdIsRun { get { return _cmdIsRun; } }

        public void cmdUnderline()
        {
            cmdExecute(EditingCommands.ToggleUnderline);
        }


        /// <summary>
        /// Toggle italic.
        /// </summary>
        public void cmdItalic()
        {
            cmdExecute(EditingCommands.ToggleItalic);
        }

        /// <summary>
        /// Toggle bold.
        /// </summary>
        public void cmdBold()
        {
            cmdExecute(EditingCommands.ToggleBold);
        }

        /// <summary>
        /// insert paragraph break.
        /// </summary>
        public void cmdInsertParagraphBreak()
        {
            cmdExecute(EditingCommands.EnterParagraphBreak);

        }

        /// <summary>
        /// Commands the insert line.
        /// </summary>
        public void cmdInsertLine()
        {
            cmdExecute(EditingCommands.EnterLineBreak);

        }

        /// <summary>
        /// Commands the delete.
        /// </summary>
        public void cmdDelete()
        {
            cmdExecute(EditingCommands.Delete);
        }

        /// <summary>
        /// Commands the undo.
        /// </summary>
        public void cmdUndo()
        {
            this.rtb_Main.Undo();
        }

        /// <summary>
        /// Commands the redo.
        /// </summary>
        public void cmdRedo()
        {
            this.rtb_Main.Redo();
        }

        /// <summary>
        /// Commands the select all.
        /// </summary>
        public void cmdSelectAll()
        {
            this.rtb_Main.SelectAll();
        }

        /// <summary>
        /// Commands the paste text.
        /// </summary>
        public void cmdPasteText()
        {
            if (Clipboard.ContainsText())
            {
                this.cmdInsertText((string)Clipboard.GetData(DataFormats.Text));
            }
        }

        /// <summary>
        /// Commands the paste.
        /// </summary>
        public void cmdPaste()
        {
            this.rtb_Main.Paste();
        }

        /// <summary>
        /// Commands the cut.
        /// </summary>
        public void cmdCut()
        {
            this.rtb_Main.Cut();
        }

        /// <summary>
        /// Commands the copy.
        /// </summary>
        public void cmdCopy()
        {
            this.rtb_Main.Copy();
        }

        /// <summary>
        /// Commands the insert text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void cmdInsertText(string text)
        {
            this.rtb_Main.CaretPosition.InsertTextInRun(text);
        }


        /// <summary>
        /// Commands the execute.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="par">The par.</param>
        public void cmdExecute(RoutedUICommand command, object par = null)
        {
            _cmdIsRun = true;
            command.Execute(par, rtb_Main);
            _cmdIsRun = false;

        }

        /// <summary>
        /// Commands the insert inline.
        /// </summary>
        /// <param name="inline">The inline.</param>
        public void cmdInsertInline(Inline inline)
        {
            this.cmdDelete();
            MemoryStream ms = new MemoryStream();
            TextRange tr = new TextRange(inline.ContentStart, inline.ContentEnd);
            tr.Save(ms, DataFormats.XamlPackage);
            ms.Position = 0;

            TextRange tr3 = new TextRange(rtb_Main.CaretPosition, rtb_Main.CaretPosition.GetInsertionPosition(LogicalDirection.Backward));

            tr3.Load(ms, DataFormats.XamlPackage);
            ms.Flush();
            ms.Close();
            ms.Dispose();

        }




        public void cmdScroll(TextPointer tp)
        {
            var fce = tp.Parent as FrameworkContentElement;
            if (fce != null)
                fce.BringIntoView();    // ostensibly reliable
        }

        /// <summary>
        /// Commands the select.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="DoubleSelect">if set to <c>true</c> [double select].</param>
        public void cmdSelect(TextElement block, bool DoubleSelect = false)
        {
            if (!DoubleSelect)
            {
                DependencyObject st = rtb_Main.Selection.Start.GetAdjacentElement(LogicalDirection.Forward);
                DependencyObject end = rtb_Main.Selection.End.GetAdjacentElement(LogicalDirection.Backward);

                if (st == end && st == block)
                {
                    return;
                }
            }
            rtb_Main.Selection.Select(block.ContentStart, block.ContentEnd);
        }

        public void cmdTableAdd(int columns, int rows)
        {
            cmdTableAdd(columns, rows, YATEHelper.Thickness1, YATEHelper.BlackBrush, YATEHelper.WhiteBrush);
        }

        public void cmdTableAdd(int columns, int rows, Thickness tn, SolidColorBrush BorderBrush)
        {
            cmdTableAdd(columns, rows, tn, BorderBrush, YATEHelper.WhiteBrush);
        }

        public void cmdTableAdd(int columns,int rows, Thickness tn, SolidColorBrush BorderBrush, SolidColorBrush Background)
        {
            this.cmdDelete();

            Table t = new Table();
            t.CellSpacing = 0.0;
            TableRowGroup trg = new TableRowGroup();
            for (int i=0;i< rows;i++)
            {
                TableRow tr = new TableRow();

                for(int j=0;j<columns;j++)
                {
                    TableCell tc = new TableCell();
                    tc.Background = Background;
                    tc.BorderBrush = BorderBrush;
                    tc.BorderThickness = new Thickness
                            (
                              (j == 0 || tn.Right < 0.01 ? tn.Left : 0.0),
                              (i == 0 || tn.Bottom < 0.01 ? tn.Top : 0.0),
                              tn.Right,
                              tn.Bottom
                            );

                    tr.Cells.Add(tc);
                }

                trg.Rows.Add(tr);
                
            }
            t.RowGroups.Add(trg);
            cmdInsertBlock2(t, true);
        }

        private void cmdTableRowAdd(int shift)
        {
            Table ctable = TryFindParent<Table>(rtb.Selection.Start.Parent as DependencyObject);
            if (ctable == null) return;
            TableRow current = TryFindParent<TableRow>(rtb.Selection.Start.Parent as DependencyObject);

            Thickness thickness;
            Brush border;
            Brush back;



            TableRowGroup crg = null;
            int pos = 0; int cells;

            if (current == null)
            {
                if (ctable.RowGroups.Count > 0)
                {
                    crg = ctable.RowGroups[0];
                }
                else
                {
                    crg = new TableRowGroup();
                    ctable.RowGroups.Add(crg);
                }
                pos = 0;
                cells = 1;
                thickness = YATEHelper.Thickness1;
                border = YATEHelper.BlackBrush;
                back = YATEHelper.WhiteBrush;
            }
            else
            {
                crg = current.Parent as TableRowGroup;
                pos = crg.Rows.IndexOf(current) + shift;
                if (current.Cells.Count > 0)
                {
                    thickness = current.Cells[0].BorderThickness;
                    border = current.Cells[0].BorderBrush;
                    back = current.Cells[0].Background;
                    cells = current.Cells.Count;
                }
                else
                {
                    cells = 1; 
                    thickness = YATEHelper.Thickness1;
                    border = YATEHelper.BlackBrush;
                    back = YATEHelper.WhiteBrush;
                }


            }
            TableRow tr = new TableRow();
            for (int i = 0; i < cells; i++)
            {
                TableCell tc = new TableCell();
                tc.BorderThickness = thickness;
                tc.BorderBrush = border;
                tc.Background = back;

                tr.Cells.Add(tc);
            }
            crg.Rows.Insert(pos, tr);
        }

        public void cmdTableRowAddAbove()
        {
            cmdTableRowAdd(0);
        }

        public void cmdTableRowAddBelow()
        {
            cmdTableRowAdd(1);
        }


        public void cmdTableRowRemove()
        {
            TableRow current = TryFindParent<TableRow>(rtb.Selection.Start.Parent as DependencyObject);
            if (current == null) return;
            TableRowGroup crg = current.Parent as TableRowGroup;
            crg.Rows.Remove(current);
        }

        public void cmdTableCopyCellStyle(TableCell source,TableCell dest)
        {
            dest.Background = source.Background;
            dest.BorderThickness = source.BorderThickness;
            dest.BorderBrush = source.BorderBrush;
        }

        private void cmdTableColAdd(int shift)
        {
            Table ctable = TryFindParent<Table>(rtb.Selection.Start.Parent as DependencyObject);
            if (ctable == null) return;

            TableCell cell = TryFindParent<TableCell>(rtb.Selection.Start.Parent as DependencyObject);
            TableRow row;
            TableRowGroup rowgroup = null;


            if (cell == null)
            {
                if (ctable.RowGroups.Count > 0)
                {
                    rowgroup = ctable.RowGroups[0];
                }
                else
                {
                    rowgroup = new TableRowGroup();
                    ctable.RowGroups.Add(rowgroup);
                }
                row = new TableRow();
                cell = new TableCell();

                cell.BorderThickness = YATEHelper.Thickness1;
                cell.BorderBrush = YATEHelper.BlackBrush;

                row.Cells.Add(cell);
                rowgroup.Rows.Add(row);

            }
            else
            {
                row = cell.Parent as TableRow;
                rowgroup = row.Parent as TableRowGroup;
            }
            int posc = row.Cells.IndexOf(cell) + shift;
            int colc = row.Cells.Count;

            foreach (TableRow tr in rowgroup.Rows)
            {
                TableCell tc = new TableCell();
                cmdTableCopyCellStyle(cell, tc);
                tr.Cells.Insert(posc, tc);
            }
        }


        public void cmdTableColAddLeft()
        {
            cmdTableColAdd(0);
        }

        public void cmdTableColAddRight()
        {
            cmdTableColAdd(1);
        }


        public void cmdTableColRemove()
        {
            TableCell cell = TryFindParent<TableCell>(rtb.Selection.Start.Parent as DependencyObject);
            TableRow row;
            TableRowGroup rowgroup;
            if (cell == null)
            {
                Table t = TryFindParent<Table>(rtb.Selection.Start.Parent as DependencyObject);
                if (t.RowGroups.Count == 0) return;
                rowgroup = t.RowGroups[0];
                if (rowgroup.Rows.Count == 0) return;
                row = rowgroup.Rows[0];
                if (row.Cells.Count == 0) return;
                cell = row.Cells[0];
            }
            else
            {
                row = cell.Parent as TableRow;
                rowgroup = row.Parent as TableRowGroup;
            }
            int posc = row.Cells.IndexOf(cell);

            foreach (TableRow tr in rowgroup.Rows)
            {
                if (posc < tr.Cells.Count)
                {
                    tr.Cells.RemoveAt(posc);
                }
            }
        }

        int minc = 0 ; int maxc = int.MaxValue;
        int minr = 0; int maxr = int.MaxValue;


        public List<TableCell> GetSelectedCells(TextSelection selection, out Table t)
        {
            t = TryFindParent<Table>(rtb.Selection.Start.Parent as DependencyObject);
            List<TableCell> selectedCells = new List<TableCell>();
            if (t == null) return selectedCells;

            minc = 0;
            maxc = int.MaxValue;

            minr = 0;
            maxr = int.MaxValue;



            TableCell find;


            if (rtb.Selection.Start.Parent is TableCell) { find = (rtb.Selection.Start.Parent as TableCell); }
            else
            {
                find = TryFindParent<TableCell>(rtb.Selection.Start.Parent as DependencyObject);
            }
            if(find!=null)
            {
                TableRow findr = (find.Parent as TableRow);
                minc = findr.Cells.IndexOf(find);
                minr = (findr.Parent as TableRowGroup).Rows.IndexOf(findr);
            }

            if (rtb.Selection.End.Parent is TableCell) { find = (rtb.Selection.End.Parent as TableCell); }
            else
            {
                find = TryFindParent<TableCell>(rtb.Selection.End.Parent as DependencyObject);
            }
            if (find != null)
            {
                maxc = (find.Parent as TableRow).Cells.IndexOf(find);
                int ssc= rtb.Selection.End.CompareTo(find.ContentStart);
                if(ssc==1 && maxc>0)
                {
                    maxc = maxc-1;
                }
            }


            foreach (TableRowGroup rg in t.RowGroups)
            {
                foreach (TableRow tr in rg.Rows)
                {
                    for (int i= 0; i < tr.Cells.Count;i++)
                    {
                        TableCell tc = tr.Cells[i];
                        if (i >= minc &&  i <= maxc)
                        {

                            int ss = rtb.Selection.Start.CompareTo(tc.ContentStart);
                            int se = rtb.Selection.Start.CompareTo(tc.ContentEnd);

                            int es = rtb.Selection.End.CompareTo(tc.ContentStart);
                            int ee = rtb.Selection.End.CompareTo(tc.ContentEnd);

                            if (
                                    (ss >= 0 && se <= 0)
                                 || (es > 0 && ee < 0)
                                 || (ss < 0 && ee > 0)
                                )
                            {
                                selectedCells.Add(tc);
                            }
                        }
                    }
                }


            }
            return selectedCells;

            }

        

        public void cmdTableSetBorderColor(Brush color)
        {

            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);

            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.BorderBrush = color;
                   
                }
                t.CellSpacing = 0;
            }
        }

        public void cmdTableSetCellColor( Brush color)
        {
            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);

            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.Background = color;
                }
                t.CellSpacing = 0;
            }
        }


        public void cmdTableSetBorderLeft(double v, Brush color = null)
        {
            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);
            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.BorderThickness = new Thickness(
                        v,
                        b.BorderThickness.Top,
                        b.BorderThickness.Right,
                        b.BorderThickness.Bottom);
                    if (color != null)
                    {
                        b.BorderBrush = color;
                    }
                }

                t.CellSpacing = 0;
            }
        }

        public void cmdTableSetBorderRight(double v, Brush color = null)
        {
            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);
            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.BorderThickness = new Thickness(
                        b.BorderThickness.Left,
                        b.BorderThickness.Top,
                        v,
                        b.BorderThickness.Bottom);
                    if (color != null)
                    {
                        b.BorderBrush = color;
                    }
                }

                t.CellSpacing = 0;
            }
        }

        public void cmdTableSetBorderTop(double v, Brush color = null)
        {
            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);
            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.BorderThickness = new Thickness(
                        b.BorderThickness.Left,
                        v,
                        b.BorderThickness.Right,
                        b.BorderThickness.Bottom);
                    if (color != null)
                    {
                        b.BorderBrush = color;
                    }
                }

                t.CellSpacing = 0;
            }
        }

        public void cmdTableSetBorderBottom (double v, Brush color = null)
        {
            Table t;
            List<TableCell> ltc = GetSelectedCells(rtb.Selection, out t);
            if (t != null)
            {
                foreach (TableCell b in ltc)
                {
                    b.BorderThickness = new Thickness(
                        b.BorderThickness.Left,
                        b.BorderThickness.Top,
                        b.BorderThickness.Right,
                        v);
                    if (color != null)
                    {
                        b.BorderBrush = color;
                    }
                }

                t.CellSpacing = 0;
            }
        }




        public void cmdTableSetBorder(Thickness border , bool AvoidDoubleBorder = true , Brush color = null)
        {
            Table t;
          
            List<TableCell> ltc = GetSelectedCells(rtb.Selection,out t);
            if (t != null)
            {
                t.BorderThickness = new Thickness(0);
                foreach (TableCell b in ltc)
                {
                    if (AvoidDoubleBorder)
                    {
                        TableRow tr = b.Parent as TableRow;
                        TableRowGroup trg = tr.Parent as TableRowGroup;

                        int cellc = tr.Cells.Count;
                        int cellp = tr.Cells.IndexOf(b);

                        int rowc = trg.Rows.Count;
                        int rowp = trg.Rows.IndexOf(tr);

                        double left = border.Left;
                        double right = border.Right;
                        double top = border.Top;
                        double bottom = border.Bottom;


                        b.BorderThickness = new Thickness
                            (
                              (cellp == minc || border.Right<  0.01  ? border.Left : 0.0),
                              (rowp == minr || border.Bottom< 0.01 ? border.Top : 0.0),
                              border.Right,
                              border.Bottom
                            );
                    }
                    else
                    {
                        b.BorderThickness = border;
                    }
                    if (color != null)
                    {
                        b.BorderBrush = color;
                    }
                }

                t.CellSpacing = 0;
            }
        }

        /// <summary>
        /// Commands the insert block.
        /// </summary>
        /// <param name="block">The block.</param>
        public void cmdInsertBlock2(Block block, bool EmptySectionAfter = false)
        {
             this.cmdDelete();

            TextRange tr3 = new TextRange(rtb_Main.CaretPosition, rtb_Main.CaretPosition.GetInsertionPosition(LogicalDirection.Backward));

            try
            {
            Block b = null; Section s = null;
            TextPointer insert = tr3.Start;
            b = TryFindParent<Block>(insert.Parent as DependencyObject);
            s = TryFindParent<Section>(insert.Parent as DependencyObject);

                if (EmptySectionAfter)
                {
                    Paragraph newItem = new Paragraph(new Run(""));
                    if (s == null)
                    {
                        if (b != null) { this.Document.Blocks.InsertAfter(b, block); this.Document.Blocks.InsertAfter(block, newItem); }
                        else           { this.Document.Blocks.Add(block); this.Document.Blocks.Add(newItem); }
                    }
                    else
                    {
                        if (b != null) { this.Document.Blocks.InsertAfter(b, block); this.Document.Blocks.InsertAfter(block, newItem); }
                        else           { this.Document.Blocks.Add(block); this.Document.Blocks.Add(newItem); }
                    }
                    rtb_Main.Selection.Select(newItem.ContentStart, newItem.ContentStart);
                } else
                {
                    if (s == null)
                    {
                        if (b != null) { this.Document.Blocks.InsertAfter(b, block);   }
                        else { this.Document.Blocks.Add(block);   }
                    }
                    else
                    {
                        if (b != null) { this.Document.Blocks.InsertAfter(b, block);  }
                        else { this.Document.Blocks.Add(block);   }
                    }
                    rtb_Main.Selection.Select(block.ContentEnd, block.ContentEnd);
                }


            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Commands the insert block.
        /// </summary>
        /// <param name="block">The block.</param>
        public void cmdInsertBlock(Block block, bool EmptySectionAfter=false)
        {
            this.cmdDelete();
            MemoryStream ms = new MemoryStream();
            TextRange tr = new TextRange(block.ContentStart, block.ContentEnd);

            tr.Save(ms, DataFormats.XamlPackage);
            ms.Position = 0;

            TextRange tr3 = new TextRange(rtb_Main.CaretPosition, rtb_Main.CaretPosition.GetInsertionPosition(LogicalDirection.Backward));

            tr3.Load(ms, DataFormats.XamlPackage);
            ms.Flush(); 
            ms.Close();
            ms.Dispose();

            TextPointer tp = tr3.End;
            rtb_Main.Selection.Select(tp, tp);
           
            if(EmptySectionAfter)
            {
                try {
                    Block b = null; Section s = null;
                    TextPointer insert = tr3.Start;
                    b = TryFindParent<Block>(insert.Parent as DependencyObject);
                    s = TryFindParent<Section>(insert.Parent as DependencyObject);

                    Paragraph newItem = new Paragraph(new Run(""));
                    if (s == null)
                    {
                        if (b != null) this.Document.Blocks.InsertAfter(b, newItem);
                        else this.Document.Blocks.Add(newItem);
                    }
                    else
                    {
                        if (b != null) s.Blocks.InsertAfter(b, newItem);
                        else s.Blocks.Add(newItem);
                    }
                    rtb_Main.Selection.Select(newItem.ContentStart, newItem.ContentStart);
                } catch(Exception)
                {

                }
            }

        }

        /// <summary>
        /// Commands the insert xaml.
        /// </summary>
        /// <param name="xaml">The xaml.</param>
        /// <returns></returns>
        public bool cmdInsertXAML(string xaml)
        {
            try
            {
                var r = LoadXAMLElement(xaml);
                if (r is FlowDocument)
                {
                    foreach (Block b in (r as FlowDocument).Blocks)
                    {
                        cmdInsertBlock(b);
                    }
                }
                else if (r is Block)
                {
                    cmdInsertBlock(r as Block);
                }
                else if (r is Inline)
                {
                    cmdInsertInline(r as Inline);
                }
                return true;
            }
            catch (System.Windows.Markup.XamlParseException)
            {
                return false;
            }
        }

        /// <summary>
        /// Commands the insert image block.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="contnt_type">The contnt_type.</param>
        /// <param name="stretch">The stretch.</param>
        public void cmdInsertImageBlock(BitmapSource image,
              ImageContentType contnt_type = ImageContentType.ImagePngContentType,
            Stretch stretch = Stretch.Uniform)
        {
            Span element = YATEHelper.GetImage(image, contnt_type, stretch);
            if (element == null) return;
            Image i = YATEHelper.ImageFromSpan(element);

            YATEHelper.AdjustWidth(i, this.PageWidth);
            i.Stretch = stretch;
            cmdInsertBlock(new Paragraph(element));
        }

        /// <summary>
        /// Commands the insert image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="contnt_type">The contnt_type.</param>
        /// <param name="stretch">The stretch.</param>
        public void cmdInsertImage(
            BitmapSource image,
            ImageContentType contnt_type = ImageContentType.ImagePngContentType,
            Stretch stretch = Stretch.Uniform)
        {
            Span element = YATEHelper.GetImage(image, contnt_type, stretch);
            if (element == null) return;

            Image i = YATEHelper.ImageFromSpan(element);
            YATEHelper.AdjustWidth(i, this.PageWidth);
            i.Stretch = stretch;

            cmdInsertInline(element as Span);
        }

        public void cmdClearSelection()
        {
            rtb_Main.Selection.Select(rtb_Main.Selection.Start, rtb_Main.Selection.Start);
        }



    }
}
