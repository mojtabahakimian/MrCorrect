import re

file_path = "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_RASID_OTHER_WIN.xaml.cs"
with open(file_path, "r", encoding="utf-8") as f:
    content = f.read()

# Restore back
content = content.replace("""e.Cancel = true;
                #region NEWWAY
                System.Windows.Controls.DataGrid DG = INVO_LST_RASIDA_SUB;
                DG.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    DG.CellEditEnding -= INVO_LST_RASIDA_SUB_CellEditEnding;
                    DG.RowEditEnding -= INVO_LST_RASIDA_SUB_RowEditEnding;

                    DG.SelectedItem = ROW;
                    DG.ScrollIntoView(ROW);
                    DG.CurrentCell = new System.Windows.Controls.DataGridCellInfo(ROW, DG.Columns[2]);
                    DG.BeginEdit();

                    DG.RowEditEnding += INVO_LST_RASIDA_SUB_RowEditEnding;
                    DG.CellEditEnding += INVO_LST_RASIDA_SUB_CellEditEnding;

                }), System.Windows.Threading.DispatcherPriority.Background);
                #endregion""", "e.Cancel = true;")

content = content.replace("""e.Cancel = true;
                #region NEWWAY
                System.Windows.Controls.DataGrid DG = INVO_LST_RASIDA_SUB;
                DG.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    DG.CellEditEnding -= INVO_LST_RASIDA_SUB_CellEditEnding;
                    DG.RowEditEnding -= INVO_LST_RASIDA_SUB_RowEditEnding;

                    DG.SelectedItem = THE_ROW_ITEM;
                    DG.ScrollIntoView(THE_ROW_ITEM);
                    DG.CurrentCell = new System.Windows.Controls.DataGridCellInfo(THE_ROW_ITEM, DG.Columns[2]);
                    DG.BeginEdit();

                    DG.RowEditEnding += INVO_LST_RASIDA_SUB_RowEditEnding;
                    DG.CellEditEnding += INVO_LST_RASIDA_SUB_CellEditEnding;

                }), System.Windows.Threading.DispatcherPriority.Background);
                #endregion""", "e.Cancel = true;")

with open(file_path, "w", encoding="utf-8") as f:
    f.write(content)
