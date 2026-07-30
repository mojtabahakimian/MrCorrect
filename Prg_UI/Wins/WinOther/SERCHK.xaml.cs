using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinOther
{
    public partial class SERCHK : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        //Header Window End;
        #endregion
        public string shart;
        public string shart2;
        public bool searchStarted = false;
        string MainRawText = "";
        //object nam = "";
        string CallerWindowName = "";

        public Visual THEWIN { get; }

        string ANBRNAME = "";
        delegate void TestDelegate();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public INVO_LST_FACTOR22 SELECTED_KALA { get; set; }
        public SERCHK(Visual WhoCallMe = null, string AnbarName = "")
        {
            if (WhoCallMe != null)
            {
                CallerWindowName = WhoCallMe.GetType().Name;
                THEWIN = WhoCallMe;
            }
            ANBRNAME = AnbarName;
            InitializeComponent();
            Text0.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchK_Btn_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        private void SearchK_Btn_Click(object sender, RoutedEventArgs e)
        {
            MainRawText = Text0.Text.Trim();
            searchStarted = true;
            if (!string.IsNullOrEmpty(MainRawText))
            {
                int i = 0;
                int K = 1;
                while (i < MainRawText.Length)
                {
                    if (MainRawText.Substring(i, 1) == " ")
                    {
                        if (Strings.Len(shart) > 0)
                        {
                            shart = shart + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K + 1, i - K + 1 - 1), "ی", "ي"), "ک", "ك").Trim() + "%')";
                            shart2 = shart2 + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K + 1, i - K + 1 - 1), "ي", "ی"), "ك", "ک").Trim() + "%')";
                            K = i;
                        }
                        else
                        {
                            shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                            shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                            K = i;
                        }
                    }
                    i = i + 1;
                }
                if (i != K)
                {
                    if (Strings.Len(shart) > 0)
                    {
                        shart = shart + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K + 1, i - K + 1 - 1), "ی", "ي"), "ک", "ك").Trim() + "%')";
                        shart2 = shart2 + " and (NAME LIKE  N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K + 1, i - K + 1 - 1), "ي", "ی"), "ك", "ک").Trim() + "%')";
                        K = i;
                    }
                    else
                    {
                        string test = Strings.Mid(MainRawText, K, i - K + 1 + 1);
                        shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                        shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                        K = i;
                    }
                }

                if (Strings.Len(MainRawText) == 1)
                {
                    shart = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ی", "ي").Trim(), "ک", "ك") + "%')";
                    shart2 = "(NAME LIKE N'%" + Strings.Replace(Strings.Replace(Strings.Mid(MainRawText, K, i - K + 1), "ي", "ی").Trim(), "ك", "ک") + "%')";
                }
                if (Strings.Len(shart) > 1)
                {
                    // تطبیق بدون درنظرگرفتن فاصله: مثلا تایپ "عقابکوه" نام ذخیره‌شدهٔ "عقاب کوه" را هم پیدا کند
                    string compact = MainRawText.Replace("'", "''").Replace(" ", "");
                    string shart3 = "(REPLACE(NAME, N' ', N'') LIKE N'%" + Strings.Replace(Strings.Replace(compact, "ی", "ي"), "ک", "ك") + "%' OR REPLACE(NAME, N' ', N'') LIKE N'%" + Strings.Replace(Strings.Replace(compact, "ي", "ی"), "ك", "ک") + "%')";
                    shart = "(( " + shart + ") or (" + shart2 + ") or (" + shart3 + "))";
                }
            }

            if (Baseknow.KALA.ToString() == "2" && CallerWindowName != "ORDR_HED" && CallerWindowName != "VISITORS_PORSANT_HEAD" && CallerWindowName != "WIN_CONTRACT_DEF")
            {
                if (string.IsNullOrEmpty(MainRawText))
                {
                    //shart = shart + "MOG > 0";
                    shart = shart + "ROUND(ISNULL(ISNULL(mogudi_1_1.SMEGH, 0)-ISNULL(mogudi_2_2.SMEGH, 0), 0), 2) > 0";
                    if (string.IsNullOrEmpty(ANBRNAME) || string.IsNullOrEmpty(ANBRNAME))
                    {
                        //DoCmd.OpenForm(shart, default, default, nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, null, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();
                    }
                    else
                    {
                        // DoCmd.OpenForm("SERCHKAL",  default, shart + "and anbar = " + ANBRNAME, default,  nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBRNAME, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();
                    }
                }
                else
                {
                    //shart = shart + "and MOG > 0";
                    shart = shart + "AND ROUND(ISNULL(ISNULL(mogudi_1_1.SMEGH, 0)-ISNULL(mogudi_2_2.SMEGH, 0), 0), 2) > 0";
                    if (string.IsNullOrEmpty(ANBRNAME) || string.IsNullOrEmpty(ANBRNAME))
                    {
                        //DoCmd.OpenForm("SERCHKAL",   shart,   nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, null, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();
                    }
                    else
                    {
                        //DoCmd.OpenForm("SERCHKAL",   shart + " and anbar = " + ANBRNAME,   nam);
                        SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBRNAME, THEWIN, (string)CallerWindowName);
                        DoCmdOpenForm.ShowDialog();
                    }
                }
            }
            else if (string.IsNullOrEmpty(MainRawText))
            {
                if (string.IsNullOrEmpty(ANBRNAME))
                {
                    //DoCmd.OpenForm("SERCHKAL",      nam);
                    SERCHKAL DoCmdOpenForm = new SERCHKAL("", "", THEWIN, CallerWindowName);
                    DoCmdOpenForm.ShowDialog();

                    if (DoCmdOpenForm?.SELECTED_KALA != null)
                    {
                        SELECTED_KALA = DoCmdOpenForm.SELECTED_KALA;
                    }

                }
                else
                {
                    //DoCmd.OpenForm("SERCHKAL",   "anbar = " + ANBRNAME,   nam);
                    SERCHKAL DoCmdOpenForm = new SERCHKAL("", ANBRNAME, THEWIN, (string)CallerWindowName);
                    DoCmdOpenForm.ShowDialog();

                    if (DoCmdOpenForm?.SELECTED_KALA != null)
                    {
                        SELECTED_KALA = DoCmdOpenForm.SELECTED_KALA;
                    }
                }
            }
            else if (string.IsNullOrEmpty(ANBRNAME) || string.IsNullOrEmpty(ANBRNAME))
            {
                //DoCmd.OpenForm("SERCHKAL",   shart,   nam);
                SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, "", THEWIN, CallerWindowName);
                DoCmdOpenForm.ShowDialog();

                if (DoCmdOpenForm?.SELECTED_KALA != null)
                {
                    SELECTED_KALA = DoCmdOpenForm.SELECTED_KALA;
                }
            }
            else
            {
                //DoCmd.OpenForm("SERCHKAL",   shart + " and anbar = " + ANBRNAME,  default, nam);
                SERCHKAL DoCmdOpenForm = new SERCHKAL(shart, ANBRNAME, THEWIN, CallerWindowName);
                DoCmdOpenForm.ShowDialog();

                if (DoCmdOpenForm?.SELECTED_KALA != null)
                {
                    SELECTED_KALA = DoCmdOpenForm.SELECTED_KALA;
                }
            }
            //DoCmd.Close(acForm, this.NAME);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PublicVRB.PlusingSearching = false;
            if ((string.IsNullOrEmpty(Text0.Text) || string.IsNullOrWhiteSpace(Text0.Text)) && searchStarted == false)
            {
                if (CallerWindowName == "HEAD_LST_HAVL")
                {
                    (THEWIN as HEAD_LST_HAVL).FROM_SAERCH_KAL.NAME_CODE = null;
                    (THEWIN as HEAD_LST_HAVL).FROM_SAERCH_KAL.CODE = null;
                }
                if (CallerWindowName == "HEAD_LST_RASID")
                {
                    (THEWIN as HEAD_LST_RASID).FROM_SAERCH_KAL.CODE = null;
                    (THEWIN as HEAD_LST_RASID).FROM_SAERCH_KAL.NAME_CODE = null;
                }
            }
            searchStarted = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PublicVRB.PlusingSearching = true;
            Text0.Focus();
        }
        private void Text0_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    e.Handled = true;
            //    if (Text0.Text.Length > 0)
            //        SearchK_Btn_Click(null, null);
            //}
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SearchK_Btn_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
