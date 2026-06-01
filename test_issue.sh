#!/bin/bash
# Review the loop calculating MAN.
cat ./Prg_UI/Wins/WinMenus/HESABDARI/F_MENU_KOL_MOIN_TAFZIL.xaml.cs | grep -A 30 "var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>(\$\"SELECT \* FROM MOIN{Baseknow.USERCOD}\").ToList();"
