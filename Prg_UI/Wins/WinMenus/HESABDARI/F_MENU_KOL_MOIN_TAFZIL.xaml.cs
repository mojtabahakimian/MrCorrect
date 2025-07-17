using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Components;
using Prg_UI.Functions.Jostejoo;
using static Prg_UI.Functions.CL_LMethods;
using Rpts;
using Prg_UI.UiTools;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    public partial class F_MENU_KOL_MOIN_TAFZIL : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public object OPEN_ARG { get; set; }
        public object HKOL { get; private set; }
        public object HMOIN { get; private set; }
        public object HTAF { get; private set; }
        public object HTAF2 { get; private set; }
        public object HTAF3 { get; private set; }
        public object HTAF4 { get; private set; }
        public string HTTAF { get; private set; }
        public bool NowReady { get; private set; }
        public Visual I_AM_F_MENU_KOL_MOIN_TAFZIL { get; set; }

        public F_MENU_KOL_MOIN_TAFZIL(object open_arg = null)
        {
            InitializeComponent();

            if (!(open_arg is null))
            {
                OPEN_ARG = open_arg;

                if (open_arg == "TAF")
                {
                    LBL_WIN.Content = "گزارش چاپی دفتر تفضیلی";
                }
                else
                {
                    this.Hide();
                    DT2.Text = Tarikh.FullCurrentDate;

                    Combo34.ItemsSource = new List<Custom_CUST_HESAB>();
                    Combo34.DisplayMemberPath = "NAME";
                    Combo34.SelectedValuePath = "hes";
                    ((List<Custom_CUST_HESAB>)Combo34.ItemsSource).Add(new Custom_CUST_HESAB { hes = OPEN_ARG.ToString() });
                    Combo34.SelectedIndex = 0;

                    Command5_Click(null, null);
                }
            }
            //this.Owner = PublicVRB.WINBASE; // برای اینکه وقتی هرچند بار که پنجره رو باز میکنی روی پنجره اصلی باز بشه و باز بمونه نره اون پشت
        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            if (Combo34.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "لطفا یک نام مشتری را انتخاب کنید"); msgwin.ShowDialog();
                return;
            }

            //double KOL = default, MOIN = default, taf = default, TAF2 = default, taf3 = default, taf4 = default;
            double? KOL = null, MOIN = null, taf = null, TAF2 = null, taf3 = null, taf4 = null;
            if (!string.IsNullOrEmpty(this.Combo34.SelectedValue.ToStringNullSafe()))
            {
                var test = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GETTAF3(this.Combo34.SelectedValue.ToString(), ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
                this.HKOL = KOL;
                this.HMOIN = MOIN;
                this.HTAF = taf;
                this.HTAF2 = TAF2;
                this.HTAF3 = taf3;
                this.HTAF4 = taf4;
            }

            string SQL;
            //object rst = null;
            //object RST2 = null;
            string PATH;
            var SORTT = default(string);
            int i;
            double MAN;
            if (IsNull(this.HTAF2))
            {
                this.HTTAF = this.HKOL + "-" + this.HMOIN + "-" + this.HTAF;
            }
            else
            {
                this.HTTAF = this.HKOL + "-" + this.HMOIN + "-" + this.HTAF + "-" + this.HTAF2;
            }
            if (string.IsNullOrEmpty(DT1.Text.ToRawTarikh()))
            {
                DT1.Text = Baseknow.YEA + "0101";
                //DT1 = Forms["baseknow"]["YEA"] + "0101";
            }
            if (string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
            {
                DT2.Text = Tarikh.FullCurrentDate;
                //DT1 = Forms["baseknow"]["YEA"] + "0101";
            }

            if (OPEN_ARG is null)
            {
                if (Combo34.SelectedIndex < 0 || Combo36.SelectedIndex < 0 || string.IsNullOrEmpty(DT1.Text.ToRawTarikh()) || string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
                {
                    Msgwin msgwin = new Msgwin(false, "تاریخ یا حساب خالی است لطفا اصلاح کنید");
                    msgwin.ShowDialog();
                    return;
                }
            }

            byte Frame34 = 0;
            if ((bool)Option40.IsChecked)
                Frame34 = 1;
            if ((bool)Option42.IsChecked)
                Frame34 = 2;
            if ((bool)Option45.IsChecked)
                Frame34 = 3;

            switch (Frame34)
            {
                case 1:
                    {
                        SORTT = "ORDER BY N_S, BED DESC";
                        break;
                    }
                case 2:
                    {
                        SORTT = "ORDER BY base, BED DESC";
                        break;
                    }
                case 3:
                    {
                        SORTT = "ORDER BY DATE_S, BED DESC";
                        break;
                    }
            }
            if (CL_HESABDARI.BLOCKEDMK(this.Combo34.SelectedValue.ToString()))
            {
                Msgwin msgwin = new Msgwin(false, "حساب مورد نظر مسدود مي باشد!");
                msgwin.ShowDialog();
                if (sender is null)
                {
                    this.Close();
                }
                return;
            }
           
            try
            {
                if (!AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.Frisok(Combo34.SelectedValue.ToString(), false))
                {
                    Msgwin msgwin = new Msgwin(false, @"بعضي از فاکتور هاي اين مشتري داراي مغايرت باسند حسابداري است فاکتورهاي داراي اشکال در فايل errorfr  در C:\CORRECT\errorfr.txt ميتوانيد ببينيد");
                    msgwin.ShowDialog();
                }

                if (!CL_HESABDARI.Khisok(Combo34.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "بعضي از فاکتور هاي خريد اين فروشنده داراي مغايرت باسند حسابداري است");
                    msgwin.ShowDialog();
                }
            }
            catch (Exception) { }


            Process Prc = ProcLoader.Start();
            #region LATER_WILL_FIX
            //Report
            if (OPEN_ARG == "TAF")
            {
                if (Frame34 == 1)
                {
                    //DoCmd.OpenReport("R_DAFTAR_TAFZILY__2", acViewPreview);
                }
                else if (Frame34 == 2)
                {
                    //DoCmd.OpenReport("R_DAFTAR_TAFZILY2__2", acViewPreview);
                }
                else
                {


                    var report = new StiReport();
                    var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.R_DAFTAR_TAFZILY_2_2.mrt");
                    report.Load(pathreport);
                    ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR; //#Left

                    #region PREPARING

                    //dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Baseknow.USERCOD + "')   DROP TABLE " + "MOIN" + Baseknow.USERCOD);
                    //dbms.DoExecuteSQL("SELECT TOP(2000000000000)   N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES, N'بد' AS TSH INTO dbo.MOIN" + Baseknow.USERCOD + "  FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1.Text.ToRawTarikh() + " , " + this.DT2.Text.ToRawTarikh() + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT); 
                    //MAN = 0d;
                    //var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM MOIN{Baseknow.USERCOD}").ToList();
                    //for (int rst_EOF = 0; rst_EOF < rst.Count; rst_EOF++)
                    //{
                    //    string tashkhis = "";
                    //    MAN = (double)(MAN + rst[rst_EOF].MAND);
                    //    if (MAN < 0)
                    //    {
                    //        tashkhis = "بس";
                    //    }
                    //    else if (MAN > 0)
                    //    {
                    //        tashkhis = "بد";
                    //    }
                    //    else
                    //    {
                    //        tashkhis = "--";
                    //    }

                    //    rst[rst_EOF].MAND = MAN;
                    //    dbms.DoExecuteSQL($"UPDATE MOIN{Baseknow.USERCOD} SET MAND = {MAN} , TSH = N'{tashkhis}' WHERE id = {rst[rst_EOF].id}");
                    //}
                    //string PARAM = $"SELECT * FROM MOIN{Baseknow.USERCOD} {SORTT}";
                    //string PARAM = $"SELECT * FROM MOIN{Baseknow.USERCOD} {SORTT}";

                    //report.Dictionary.Variables.Add("Variable1", PARAM);
                    //(@DT1, @DT2, @HESAB);

                    report["DT1"] = DT1.Text.ToRawTarikh();
                    report["DT2"] = DT2.Text.ToRawTarikh();
                    report["HESAB"] = Combo34.SelectedValue.ToString();

                    (report.GetComponentByName("KARBAR") as StiText).Text = Baseknow.UUSER;
                    (report.GetComponentByName("COMPANY_NAME") as StiText).Text = Baseknow.NAME;

                    (report.GetComponentByName("HESABFULL") as StiText).Text = $"حساب : {Combo34.SelectedValue} | {(Combo34.SelectedItem as Custom_CUST_HESAB).NAME}";

                    (report.GetComponentByName("AZ_DT") as StiText).Text = $"از تاریخ : {DT1.Text}";
                    (report.GetComponentByName("TA_DT") as StiText).Text = $"تا تاریخ : {DT2.Text}";

                    //report.Render();
                    ProcLoader.Stop(Prc);

                    new WINRPT(report, "گزارش دفتر تفضیلی").Show();
                    //report.Show();

                    #endregion


                    //DoCmd.OpenReport("R_DAFTAR_TAFZILY_2_2", acViewPreview);
                }
                //DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
                //DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
            }
            //if (this.OpenArgs == "VAZ")
            //{
            //    DoCmd.OpenReport("R_GARDESH_KHFR_DAFTAR", acViewPreview);
            //    DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
            //}
            //if (this.OpenArgs == "FRKMA4")
            //{
            //    DoCmd.OpenReport("R_GARDESH_KHFR_DAFTAR_A4", acViewPreview);
            //    DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
            //}

            if (OPEN_ARG != "TAF")  //if (this.OpenArgs == "RMOIN")
            {
                if (true)
                {
                    dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Baseknow.USERCOD + "')   DROP TABLE " + "MOIN" + Baseknow.USERCOD);
                    //string QRE = "SELECT    N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES INTO dbo.MOIN" + Baseknow.USERCOD + " FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1.Text.ToRawTarikh() + " , " + this.DT2.Text.ToRawTarikh() + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT;
                    string QRE = "SELECT TOP(2000000000000)   N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES, N'بد' AS TSH INTO dbo.MOIN" + Baseknow.USERCOD + "  FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1.Text.ToRawTarikh() + " , " + this.DT2.Text.ToRawTarikh() + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT;
                    dbms.DoExecuteSQL(QRE);
                    MAN = 0d;
                    var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM MOIN{Baseknow.USERCOD}").ToList();
                    for (int rst_EOF = 0; rst_EOF < rst.Count; rst_EOF++)
                    {
                        string tashkhis = "";
                        MAN = (double)(MAN + rst[rst_EOF].MAND);
                        if (MAN < 0)
                        {
                            tashkhis = "بس";
                        }
                        else if (MAN > 0)
                        {
                            tashkhis = "بد";
                        }
                        else
                        {
                            tashkhis = "--";
                        }

                        rst[rst_EOF].MAND = MAN;
                        dbms.DoExecuteSQL($"UPDATE MOIN{Baseknow.USERCOD} SET MAND = {MAN} , TSH = N'{tashkhis}' WHERE id = {rst[rst_EOF].id}");
                    }

                    R_DAFTAR_MOIN_LIST r_DAFTAR_MOIN_LIST = new R_DAFTAR_MOIN_LIST($"MOIN{Baseknow.USERCOD} {SORTT}", Combo34.SelectedValue.ToStringNullSafe() + $" {Combo36.Text} ");
                    ProcLoader.Stop(Prc);

                    if (OPEN_ARG is not null)
                    {
                        this.Close();
                    }

                    r_DAFTAR_MOIN_LIST.Show();
                }
                //else if (!IsLoaded("R_DAFTAR_MOIN_LIST2"))
                //{
                //    dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Baseknow.USERCOD + "_2')   DROP TABLE " + "MOIN" + Baseknow.USERCOD + "_2");
                //    dbms.DoExecuteSQL("SELECT    N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES INTO dbo.MOIN" + Baseknow.USERCOD + "_2 FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1.Text.ToRawTarikh() + " , " + this.DT2.Text.ToRawTarikh() + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT);
                //    MAN = 0d;
                //    rst.Open("MOIN" + Baseknow.USERCOD + "_2");
                //    while (!rst.EOF())
                //    {
                //        MAN = MAN + rst.Fields("MAND");
                //        rst.Fields("MAND") = MAN;
                //        rst.update();
                //        rst.MoveNext();
                //    }
                //    DoCmd.OpenForm("R_DAFTAR_MOIN_LIST2", acFormDS);
                //    DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
                //}
                //else
                //{
                //    dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Forms["baseknow"]["USERCOD"] + "_3')   DROP TABLE " + "MOIN" + Forms["baseknow"]["USERCOD"] + "_3");
                //    dbms.DoExecuteSQL("SELECT    N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES INTO dbo.MOIN" + Forms["baseknow"]["USERCOD"] + "_3 FROM         dbo.QDAFTARTAFZIL2_H(" + this.DT1 + " , " + this.DT2 + " , '" + this.Combo34 + "') QDAFTARTAFZIL2_H " + SORTT);
                //    MAN = 0d;
                //    rst.Open("MOIN" + Forms["baseknow"]["USERCOD"] + "_3");
                //    while (!rst.EOF())
                //    {
                //        MAN = MAN + rst.Fields("MAND");
                //        rst.Fields("MAND") = MAN;
                //        rst.update();
                //        rst.MoveNext();
                //    }
                //    DoCmd.OpenForm("R_DAFTAR_MOIN_LIST3", acFormDS);
                //    DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
                //}
                //DoCmd.OpenForm(this.Name, default, default, default, default, acHidden);
            }
            //if (this.OpenArgs == "NABZMOSH")
            //{
            //    DoCmd.OpenForm("NABZ_MOSHTARI", default, default, default, default, default, this.Combo34);
            //}
            //if (this.OpenArgs == "NABZKAR")
            //{
            //    DoCmd.OpenForm("NABZ_KARSHENAS", default, default, default, default, default, this.Combo34);
            //}
            #endregion

        }

        private bool IsNull(object hTAF2)
        {
            if (hTAF2 is null)
            {
                return true;
            }
            if (!(hTAF2 is null))
            {
                return false;
            }
            return true;
        }

        private void Combo34_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo34.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)Combo34.Template.FindName("PART_EditableTextBox", Combo34);
            if (CUTSNO_TEX.Text.Trim() == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("F_MENU_KOL_MOIN_TAFZIL", I_AM_F_MENU_KOL_MOIN_TAFZIL);
                CMBSearch.ShowDialog();
            }


            //____Combo34_AfterUpdate();

            //this.Combo36 = this.Combo34;
            //this.Combo36.Requery();

            //____Combo34_Exit
            //if (!IsNull(this.Combo34))
            //{
            //    if (ISTAF(this.Combo34))
            //    {
            //        DoCmd.OpenForm("MESAGEFORM", acNormal, default, default, acFormReadOnly, acDialog, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
            //        CANCEL = Conversions.ToInteger(true);
            //    }
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (OPEN_ARG is not null && OPEN_ARG != "TAF")
            {
                return;
            }

            I_AM_F_MENU_KOL_MOIN_TAFZIL = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);


            Process Prc = ProcLoader.Start();

            Combo36.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT hes,NAME FROM CUST_HESAB").ToList();
            Combo36.DisplayMemberPath = "NAME";
            Combo36.SelectedValuePath = "hes";

            //CODE (HES)
            Combo34.ItemsSource = Combo36.ItemsSource;
            Combo34.DisplayMemberPath = "hes";
            Combo34.SelectedValuePath = "hes";

            DT2.Text = Tarikh.FullCurrentDate;
            ProcLoader.Stop(Prc);


        }

        private void Command6_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Combo36_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Combo36.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            TextBox TexBo = (TextBox)Combo36.Template.FindName("PART_EditableTextBox", Combo36);
            if (TexBo.Text == "+" || TexBo.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("F_MENU_KOL_MOIN_TAFZIL");//Search Plusy Form Specialy for Customers
                CMBSearch.ShowDialog();
                return;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (NowReady && e.Key == Key.Enter)
            {
                if (!Command5.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            //Command5
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowReady = true;

            Keyboard.Focus(Combo34);
            Combo34.Focus();
        }
    }
}
