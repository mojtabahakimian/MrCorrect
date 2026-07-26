import re
import os

files = [
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

def get_config(f_content, f_path):
    match = re.search(r'private void ([A-Za-z0-9_]+)_RowEditEnding\(object sender, DataGridRowEditEndingEventArgs e\)', f_content)
    if not match: return None
    grid_base = match.group(1)

    row_event = f"{grid_base}_RowEditEnding"
    cell_event = f"{grid_base}_CellEditEnding"
    grid_name = grid_base
    row_var = "ROW"
    if "PGET_HED" in f_path:
        grid_name = "PGET_LST_SUB"
        row_var = "THE_ROW_ITEM"
        cell_event = "PGET_LST_SUB_CellEditEnding"
        row_event = "PGET_LST_SUB_RowEditEnding"

    index_col = "2"
    if "INVO_LST_SUB_DEF_INDEX_COL" in f_content:
        index_col = "INVO_LST_SUB_DEF_INDEX_COL"

    return grid_name, row_event, cell_event, row_var, index_col

for file_path in files:
    if not os.path.exists(file_path):
        continue
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    cfg = get_config(content, file_path)
    if not cfg:
        continue

    grid_name, row_event, cell_event, row_var, index_col = cfg

    # Check for REND_ROW override
    start_idx = content.find(f"private void {row_event}(object sender, DataGridRowEditEndingEventArgs e)")
    match_end = re.search(r'\n        private void ', content[start_idx+100:])
    end_idx = start_idx + 100 + match_end.start() if match_end else len(content)
    sub_content = content[start_idx:end_idx]

    if "var REND_ROW =" in sub_content:
        row_var = "REND_ROW"

    block = f"""e.Cancel = true;
                #region NEWWAY
                System.Windows.Controls.DataGrid DG = {grid_name};
                DG.Dispatcher.BeginInvoke(new System.Action(() =>
                {{
                    DG.CellEditEnding -= {cell_event};
                    DG.RowEditEnding -= {row_event};

                    DG.SelectedItem = {row_var};
                    DG.ScrollIntoView({row_var});
                    DG.CurrentCell = new System.Windows.Controls.DataGridCellInfo({row_var}, DG.Columns[{index_col}]);
                    DG.BeginEdit();

                    DG.RowEditEnding += {row_event};
                    DG.CellEditEnding += {cell_event};

                }}), System.Windows.Threading.DispatcherPriority.Background);
                #endregion"""

    # Direct replacement of `e.Cancel = true;` that are not inside NEWWAY
    lines = sub_content.split('\n')
    new_lines = []
    i = 0
    while i < len(lines):
        line = lines[i]
        if "e.Cancel = true;" in line and "var DG =" not in '\n'.join(lines[max(0, i-3):min(len(lines), i+3)]) and "System.Windows.Controls.DataGrid DG =" not in '\n'.join(lines[max(0, i-3):min(len(lines), i+3)]):
            if not line.strip().startswith("//"):
                new_lines.append(line.replace("e.Cancel = true;", block))
            else:
                new_lines.append(line)
        else:
            new_lines.append(line)
        i += 1
    sub_content = '\n'.join(new_lines)

    content = content[:start_idx] + sub_content + content[end_idx:]
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Processed {file_path}")
