using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_UI.Wins.WinOther;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Functions.Jostejoo
{
    public static class CL_KALA_SEARCH
    {
        public static INVO_LST_FACTOR22 Go_Search_Kala(string matn, string ANBARCODE, Visual THEWIN = null)
        {
            INVO_LST_FACTOR22? _SELECTED_KALA_ = null;

            string shart = "";
            string shart2 = "";

            if (!string.IsNullOrEmpty(matn))
            {
                int i = 0;
                int K = 1;
                while (i < matn.Length)
                {
                    if (matn.Substring(i, 1) == " ")
                    {
                        if (Strings.Len(shart) > 0)
                        {
                            shart = shart + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K + 1, i - K + 1 - 1), "ی", "ي"), "ک", "ك").Trim() + "%')";
                            shart2 = shart2 + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K + 1, i - K + 1 - 1), "ي", "ی"), "ك", "ک").Trim() + "%')";
                            K = i;
                        }
                        else
                        {
                            shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                            shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                            K = i;
                        }
                    }
                    i = i + 1;
                }
                if (i != K)
                {
                    if (Strings.Len(shart) > 0)
                    {
                        shart = shart + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K + 1, i - K + 1 - 1), "ی", "ي"), "ک", "ك").Trim() + "%')";
                        shart2 = shart2 + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K + 1, i - K + 1 - 1), "ي", "ی"), "ك", "ک").Trim() + "%')";
                        K = i;
                    }
                    else
                    {
                        string test = Strings.Mid(matn, K, i - K + 1 + 1);
                        shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                        shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                        K = i;
                    }
                }

                if (Strings.Len(matn) == 1)
                {
                    shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                    shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(matn, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                }
                if (Strings.Len(shart) > 1)
                {
                    shart = "(( " + shart + ") or (" + shart2 + "))";
                }
            }

            string CallerWindowName = "";

            if (Baseknow.KALA.ToString() == "2" && CallerWindowName != "ORDR_HED" && CallerWindowName != "VISITORS_PORSANT_HEAD")
            {
                if (string.IsNullOrEmpty(ANBARCODE))
                {
                    //shart = shart + "MOG > 0";
                    shart = shart + "ROUND(ISNULL(ISNULL(mogudi_1_1.SMEGH, 0)-ISNULL(mogudi_2_2.SMEGH, 0), 0), 2) > 0";
                    if (string.IsNullOrEmpty(ANBARCODE) || string.IsNullOrEmpty(ANBARCODE))
                    {
                        //DoCmd.OpenForm(shart, default, default, nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, null, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();

                        if (DoCmdOpenForm?.SELECTED_KALA != null)
                        {
                            _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                        }
                    }
                    else
                    {
                        // DoCmd.OpenForm("SERCHKAL",  default, shart + "and anbar = " + ANBARCODE, default,  nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBARCODE, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();

                        if (DoCmdOpenForm?.SELECTED_KALA != null)
                        {
                            _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                        }
                    }
                }
                else
                {
                    //shart = shart + "and MOG > 0";
                    shart = shart + "AND ROUND(ISNULL(ISNULL(mogudi_1_1.SMEGH, 0)-ISNULL(mogudi_2_2.SMEGH, 0), 0), 2) > 0";
                    if (string.IsNullOrEmpty(ANBARCODE) || string.IsNullOrEmpty(ANBARCODE))
                    {
                        //DoCmd.OpenForm("SERCHKAL",   shart,   nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, null, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();

                        if (DoCmdOpenForm?.SELECTED_KALA != null)
                        {
                            _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                        }
                    }
                    else
                    {
                        //DoCmd.OpenForm("SERCHKAL",   shart + " and anbar = " + ANBARCODE,   nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBARCODE, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();

                        if (DoCmdOpenForm?.SELECTED_KALA != null)
                        {
                            _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                        }
                    }
                }
            }
            else if (string.IsNullOrEmpty(ANBARCODE))
            {
                if (string.IsNullOrEmpty(ANBARCODE))
                {
                    //DoCmd.OpenForm("SERCHKAL",      nam);
                    SERCHKAL DoCmdOpenForm = new SERCHKAL("", "", THEWIN, CallerWindowName);
                    DoCmdOpenForm.ShowDialog();

                    if (DoCmdOpenForm?.SELECTED_KALA != null)
                    {
                        _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                    }
                }
                else
                {
                    //DoCmd.OpenForm("SERCHKAL",   "anbar = " + ANBARCODE,   nam);
                    SERCHKAL DoCmdOpenForm = new SERCHKAL("", ANBARCODE, THEWIN, (string)CallerWindowName);
                    DoCmdOpenForm.ShowDialog();

                    if (DoCmdOpenForm?.SELECTED_KALA != null)
                    {
                        _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                    }
                }
            }
            else if (string.IsNullOrEmpty(ANBARCODE) || string.IsNullOrEmpty(ANBARCODE))
            {
                //DoCmd.OpenForm("SERCHKAL",   shart,   nam);
                SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, "", THEWIN, CallerWindowName);
                DoCmdOpenForm.ShowDialog();

                if (DoCmdOpenForm?.SELECTED_KALA != null)
                {
                    _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                }
            }
            else
            {
                //DoCmd.OpenForm("SERCHKAL",   shart + " and anbar = " + ANBARCODE,  default, nam);
                SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBARCODE, THEWIN, CallerWindowName);
                DoCmdOpenForm.ShowDialog();

                if (DoCmdOpenForm?.SELECTED_KALA != null)
                {
                    _SELECTED_KALA_ = DoCmdOpenForm.SELECTED_KALA;
                }
            }
            //DoCmd.Close(acForm, this.NAME);

            if (_SELECTED_KALA_ != null)
            {
                return _SELECTED_KALA_;
            }

            return null;
        }
    }
}
