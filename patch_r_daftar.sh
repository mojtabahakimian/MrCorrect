#!/bin/bash
cat << 'PATCH' > r_daftar.patch
--- Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs
+++ Prg_UI/Wins/WinMenus/HESABDARI/R_DAFTAR_MOIN_LIST.xaml.cs
@@ -48,6 +48,7 @@

             Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
             GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
+            this.Closed += R_DAFTAR_MOIN_LIST_Closed;
         }
         #region Header Window Begin
         //Header Window Begin
@@ -107,6 +108,22 @@
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
+                        dbms.DoExecuteSQL($"DROP TABLE dbo.{tableName}");
+                    }
+                }
+            }
+            catch { }
+        }
         UniversControl universControl = new UniversControl();
         public object OPEN_ARG { get; set; }
         public string FULLHESAB_NAME { get; set; }
PATCH

patch -p0 < r_daftar.patch
