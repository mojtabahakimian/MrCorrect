#!/bin/bash
cat << 'PATCH' > f_menu.patch
--- Prg_UI/Wins/WinMenus/HESABDARI/F_MENU_KOL_MOIN_TAFZIL.xaml.cs
+++ Prg_UI/Wins/WinMenus/HESABDARI/F_MENU_KOL_MOIN_TAFZIL.xaml.cs
@@ -462,17 +462,31 @@

                     if (true)
                     {
-                        dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Baseknow.USERCOD + "')   DROP TABLE " + "MOIN" + Baseknow.USERCOD);
+                        // Cleanup old orphaned tables for this user
+                        try
+                        {
+                            var oldTables = dbms.DoGetDataSQL<string>($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'MOIN{Baseknow.USERCOD}\\_%' ESCAPE '\\'").ToList();
+                            foreach (var oldTable in oldTables)
+                            {
+                                dbms.DoExecuteSQL($"DROP TABLE dbo.{oldTable}");
+                            }
+                        }
+                        catch { }
+
+                        // Generate unique table name
+                        string uniqueTableName = $"MOIN{Baseknow.USERCOD}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
+
+                        dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + uniqueTableName + "')   DROP TABLE " + uniqueTableName);
                         //string QRE = "SELECT    N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES INTO dbo.MOIN" + Baseknow.USERCOD + " FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1.Text.ToRawTarikh() + " , " + this.DT2.Text.ToRawTarikh() + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT;
-                        string QRE = "SELECT TOP(2000000000000)   N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES, N'بد' AS TSH INTO dbo.MOIN" + Baseknow.USERCOD + "  FROM   " +
+                        string QRE = "SELECT TOP(2000000000000)   N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES, N'بد' AS TSH INTO dbo." + uniqueTableName + "  FROM   " +
                             "      dbo.QDAFTARTAFZIL2_H(" + AZ_DT_PARAM + " , " + TA_DT_PARAM + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT;
                         dbms.DoExecuteSQL(QRE);
                         MAN = 0d;
-                        var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM MOIN{Baseknow.USERCOD}").ToList();
+                        var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM {uniqueTableName} {SORTT}").ToList();
                         for (int rst_EOF = 0; rst_EOF < rst.Count; rst_EOF++)
                         {
                             string tashkhis = "";
                             MAN = (double)(MAN + rst[rst_EOF].MAND);
                             if (MAN < 0)
                             {
                                 tashkhis = "بس";
@@ -487,11 +501,11 @@
                             }

                             rst[rst_EOF].MAND = MAN;
-                            dbms.DoExecuteSQL($"UPDATE MOIN{Baseknow.USERCOD} SET MAND = {MAN} , TSH = N'{tashkhis}' WHERE id = {rst[rst_EOF].id}");
+                            dbms.DoExecuteSQL($"UPDATE {uniqueTableName} SET MAND = {MAN} , TSH = N'{tashkhis}' WHERE id = {rst[rst_EOF].id}");
                         }

-                        R_DAFTAR_MOIN_LIST r_DAFTAR_MOIN_LIST = new R_DAFTAR_MOIN_LIST($"MOIN{Baseknow.USERCOD} {SORTT}", Combo34.SelectedValue.ToStringNullSafe() + $" {Combo36.Text} ");
+                        R_DAFTAR_MOIN_LIST r_DAFTAR_MOIN_LIST = new R_DAFTAR_MOIN_LIST($"{uniqueTableName} {SORTT}", Combo34.SelectedValue.ToStringNullSafe() + $" {Combo36.Text} ");
                         ProcLoader.Stop(Prc);

                         if (OPEN_ARG is not null)
                         {
PATCH

patch -p0 < f_menu.patch
