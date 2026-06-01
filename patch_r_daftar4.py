import re

with open("Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs", "r") as f:
    content = f.read()

new_code = """
        private void R_DAFTAR_MOIN_LIST_Closed(object sender, EventArgs e)
        {
            try
            {
                if (OPEN_ARG != null)
                {
                    string openArgStr = OPEN_ARG.ToString();
                    string tableName = openArgStr.Split(' ')[0];
                    if (tableName.StartsWith("MOIN"))
                    {
                        dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') DROP TABLE dbo.{tableName}");
                    }
                }
            }
            catch { }
        }
"""

content = content.replace("public ObservableCollection<MOIN_CUSTOM> DAFTAR_DATA { get; set; } = new ObservableCollection<MOIN_CUSTOM>();\n", "public ObservableCollection<MOIN_CUSTOM> DAFTAR_DATA { get; set; } = new ObservableCollection<MOIN_CUSTOM>();\n" + new_code)

with open("Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs", "w") as f:
    f.write(content)
