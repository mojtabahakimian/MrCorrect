with open("Prg_UI/Wins/WinMenus/HESABDARI/F_MENU_KOL_MOIN_TAFZIL.xaml.cs", "r") as f:
    content = f.read()

old_cleanup = """                        // Cleanup old orphaned tables for this user
                        try
                        {
                            var oldTables = dbms.DoGetDataSQL<string>($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'MOIN{Baseknow.USERCOD}\\_%' ESCAPE '\\\\'").ToList();
                            foreach (var oldTable in oldTables)
                            {
                                dbms.DoExecuteSQL($"DROP TABLE dbo.{oldTable}");
                            }
                        }
                        catch { }"""

new_cleanup = """                        // Cleanup old orphaned tables for this user safely (older than 1 day)
                        try
                        {
                            string cleanupQuery = $@"
                                DECLARE @Sql NVARCHAR(MAX) = '';
                                SELECT @Sql += 'DROP TABLE dbo.' + QUOTENAME(name) + ';'
                                FROM sys.tables
                                WHERE name LIKE 'MOIN{Baseknow.USERCOD}\\_%' ESCAPE '\\'
                                  AND create_date < DATEADD(day, -1, GETDATE());
                                IF @Sql <> '' EXEC sp_executesql @Sql;";
                            dbms.DoExecuteSQL(cleanupQuery);
                        }
                        catch { }"""

content = content.replace(old_cleanup, new_cleanup)

with open("Prg_UI/Wins/WinMenus/HESABDARI/F_MENU_KOL_MOIN_TAFZIL.xaml.cs", "w") as f:
    f.write(content)
