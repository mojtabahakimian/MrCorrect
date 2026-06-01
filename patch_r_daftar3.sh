#!/bin/bash
cat << 'PATCH' > r_daftar.patch
--- Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs
+++ Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs
@@ -107,6 +107,22 @@
         //Header Window End;
         #endregion
         public ObservableCollection<MOIN_CUSTOM> DAFTAR_DATA { get; set; } = new ObservableCollection<MOIN_CUSTOM>();
+
+        private void R_DAFTAR_MOIN_LIST_Closed(object sender, EventArgs e)
+        {
+            try
+            {
+                if (OPEN_ARG != null)
+                {
+                    string openArgStr = OPEN_ARG.ToString();
+                    string tableName = openArgStr.Split(' ')[0];
+                    if (tableName.StartsWith("MOIN"))
+                    {
+                        dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') DROP TABLE dbo.{tableName}");
+                    }
+                }
+            }
+            catch { }
+        }
+
         UniversControl universControl = new UniversControl();
         public object OPEN_ARG { get; set; }
         public string FULLHESAB_NAME { get; set; }
PATCH

patch -p0 < r_daftar.patch
