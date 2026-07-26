import os
import re

target_files = [
    "Prg_UI/Wins/WinMenus/KHARID_FORUSH/HEAD_LST_FROOSH22.xaml.cs",
    "Prg_UI/Wins/WinMenus/KHARID_FORUSH/HEAD_LST_KH_BACK.xaml.cs",
    "Prg_UI/Wins/WinMenus/KHARID_FORUSH/HEAD_LST_KH_BACK_AZAD.xaml.cs",
    "Prg_UI/Wins/WinMenus/KHARID_FORUSH/HEAD_LST_FROOSH_BACK2.xaml.cs",
    "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_HAVL.xaml.cs",
    "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_RASID.xaml.cs",
    "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_HAV_OTHER_WIN.xaml.cs",
    "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_RASID_OTHER_WIN.xaml.cs",
    "Prg_UI/Wins/WinMenus/ANBAR/HEAD_LST_ENTEGHAL_WIN.xaml.cs",
    "Prg_UI/Wins/WinMenus/Khazaneh/PGET_HED.xaml.cs"
]

def replace_in_file(file_path):
    if not os.path.exists(file_path):
        return

    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    match = re.search(r'private void ([A-Za-z0-9_]+)_RowEditEnding\(object sender, DataGridRowEditEndingEventArgs e\)', content)
    if not match:
        return

    start_idx = match.start()
    match_end = re.search(r'\n        private void ', content[start_idx+100:])
    end_idx = start_idx + 100 + match_end.start() if match_end else len(content)
    sub_content = content[start_idx:end_idx]

    grid_name = match.group(1)
    cell_edit_event = f"{grid_name}_CellEditEnding"
    row_edit_event = f"{grid_name}_RowEditEnding"

    index_col = "INVO_LST_SUB_DEF_INDEX_COL" if "INVO_LST_SUB_DEF_INDEX_COL" in content else "2"
    if "PGET_HED" in file_path:
        index_col = "2"
        grid_name = "PGET_LST_SUB"
        cell_edit_event = "PGET_LST_SUB_CellEditEnding"
        row_edit_event = "PGET_LST_SUB_RowEditEnding"

    row_var = "ROW"
    if "var REND_ROW =" in sub_content:
        row_var = "REND_ROW"
    if "PGET_HED" in file_path:
        row_var = "THE_ROW_ITEM"

    replacement_block = f"""e.Cancel = true;
                #region NEWWAY
                System.Windows.Controls.DataGrid DG = {grid_name};
                DG.Dispatcher.BeginInvoke(new System.Action(() =>
                {{
                    DG.CellEditEnding -= {cell_edit_event};
                    DG.RowEditEnding -= {row_edit_event};

                    DG.SelectedItem = {row_var};
                    DG.ScrollIntoView({row_var});
                    DG.CurrentCell = new System.Windows.Controls.DataGridCellInfo({row_var}, DG.Columns[{index_col}]);
                    DG.BeginEdit();

                    DG.RowEditEnding += {row_edit_event};
                    DG.CellEditEnding += {cell_edit_event};

                }}), System.Windows.Threading.DispatcherPriority.Background);
                #endregion"""

    # Replace ONLY `e.Cancel = true;` that is isolated on its own line
    lines = sub_content.split('\n')
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        # Ignore if it's already followed by #region NEWWAY
        if "e.Cancel = true;" in line and "var DG =" not in '\n'.join(lines[max(0, i-3):min(len(lines), i+3)]) and "System.Windows.Controls.DataGrid DG =" not in '\n'.join(lines[max(0, i-3):min(len(lines), i+3)]):
            # Only replace if it's not a commented line
            if not line.strip().startswith("//"):
                new_lines.append(line.replace("e.Cancel = true;", replacement_block))
            else:
                new_lines.append(line)
        else:
            new_lines.append(line)
        i += 1

    sub_content = '\n'.join(new_lines)
    content = content[:start_idx] + sub_content + content[end_idx:]

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Refactored {file_path} with NEWWAY block")

for f in target_files:
    replace_in_file(f)
