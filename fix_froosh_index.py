import sys

file_path = "Prg_UI/Wins/WinMenus/KHARID_FORUSH/HEAD_LST_FROOSH22.xaml.cs"
with open(file_path, "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace("INVO_LST_SUB_DEF_INDEX_COL", "2")

with open(file_path, "w", encoding="utf-8") as f:
    f.write(content)
