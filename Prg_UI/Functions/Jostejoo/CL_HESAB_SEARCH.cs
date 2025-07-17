using Microsoft.VisualBasic;
using Prg_UI.Wins.WinOther;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Prg_UI.Functions.Jostejoo
{
    public static class CL_HESAB_SEARCH
    {
        public static void Go_Search_Hesab(string _TXT_, string FRMNAME, Visual thevisu)
        {
            var MyTextSearch = _TXT_;
            var shart = "";
            var shart2 = "";
            if (string.IsNullOrEmpty(MyTextSearch))
            {
                ResOfSearch resOfSearch = new ResOfSearch(null, FRMNAME, null, thevisu);
                resOfSearch.ShowDialog();
            }
            else
            {
                int i = 0;
                int K = 1;
                while (i < MyTextSearch.Length)
                {
                    if (MyTextSearch.Substring(i, 1) == " ")
                    {
                        if (shart.Length > 0)
                        {
                            if (Strings.Len(shart) > 0)
                            {
                                shart = shart + " and (NAME LIKE N'%" + Strings.Mid(MyTextSearch, K + 1, i - K + 1 - 1).Trim() + "%' or NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K + 1, i - K + 1 - 1), "ی", "ي"), "ک", "ك").Trim() + "%' or  NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K + 1, i - K + 1 - 1), "ي", "ی"), "ک", "ك").Trim() + "%')";
                                shart2 = shart2 + " and (TNAME LIKE N'%" + Strings.Mid(MyTextSearch, K + 1, i - K + 1 - 1).Trim() + "%' or TNAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K + 1, i - K + 1 - 1), "ي", "ی"), "ک", "ك").Trim() + "%')";
                                K = i;
                            }
                            else
                            {
                                shart = "(NAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1).Trim() + "%' or  NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                                shart2 = "(TNAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1).Trim() + "%' or TNAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1), "ي", "ی").Trim(), "ک", "ك") + "%')";
                                K = i;
                            }
                        }
                        else
                        {
                            string s = Strings.Mid(MyTextSearch, K, i - K + 1 + 1).ToString();
                            shart = "(NAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1).Trim() + "%' or  NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1), "ی", "ي"), "ک", "ك").Trim() + "%')";
                            shart2 = "(TNAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1).Trim() + "%' or TNAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1), "ي", "ی"), "ک", "ك").Trim() + "%')";
                            K = i;
                        }
                    }
                    i = i + 1;
                }
                if (i != K)
                {
                    if (Strings.Len(shart) > 0)
                    {
                        shart = shart + " and (NAME LIKE N'%" + Strings.Mid(MyTextSearch, K + 1, i - K + 1 + 1).Trim() + "%' or  NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K + 1, i - K + 1 + 1), "ی", "ي"), "ک", "ك").Trim() + "%' or  NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1 + 1), "ي", "ی"), "ک", "ك").Trim() + "%')";
                        shart2 = shart2 + " and (TNAME LIKE N'%" + Strings.Mid(MyTextSearch, K + 1, i - K + 1 + 1).Trim() + "%' or  TNAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K + 1, i - K + 1 + 1), "ي", "ی"), "ک", "ك").Trim() + "%')";
                        K = i;
                    }
                    else
                    {
                        string test = Strings.Mid(MyTextSearch, K, i - K + 1 + 1);
                        shart = "NAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1 + 1).Trim() + "%' or  NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1 + 1).Trim(), "ی", "ي"), "ک", "ك") + "%' OR NAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1 + 1).Trim() + "%' or  NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1 + 1), "ي", "ی"), "ک", "ك").Trim() + "%'";
                        shart2 = "TNAME LIKE N'%" + Strings.Mid(MyTextSearch, K, i - K + 1 + 1).Trim() + "%' or TNAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MyTextSearch, K, i - K + 1 + 1).Trim(), "ي", "ی"), "ک", "ك") + "%'";
                        K = i;
                    }
                }

                if (Strings.Len(MyTextSearch) == 1)
                {
                    shart = "NAME LIKE N'%" + Strings.Replace(Strings.Replace(MyTextSearch, "ی", "ي"), "ک", "ك").Trim() + "%'";
                    shart2 = "TNAME LIKE N'%" + Strings.Replace(Strings.Replace(MyTextSearch, "ي", "ی"), "ک", "ك").Trim() + "%'";
                }
                if (Strings.Len(shart) > 1)
                {
                    shart = "(( " + shart + ") or (" + shart2 + "))";
                }

                ResOfSearch resOfSearch = new ResOfSearch(shart, FRMNAME, null, thevisu);
                resOfSearch.ShowDialog();
            }
        }
    }
}
