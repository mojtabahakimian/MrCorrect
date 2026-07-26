import re
import os

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
    "Prg_UI/Wins/WinMenus/HESABDARI/DEED_HEAD.xaml.cs",
    "Prg_UI/Wins/WinMenus/Khazaneh/PGET_HED.xaml.cs"
]

def analyze(f_path):
    if not os.path.exists(f_path):
        return
    with open(f_path, "r", encoding="utf-8") as f:
        content = f.read()

    match = re.search(r'private void ([A-Za-z0-9_]+)_RowEditEnding\(object sender, DataGridRowEditEndingEventArgs e\)', content)
    if not match: return
    grid_name = match.group(1)

    print(f"File: {f_path}, Grid Name: {grid_name}")

for f in target_files:
    analyze(f)
