using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Prg_UI.Wins.WinOther;
using Rpts;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Components.TextFormats;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

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
        public string AZ_DT_PARAM { get; set; } = "0";
        public string TA_DT_PARAM { get; set; } = "9999999999";
        public Window? THEOWENER { get; set; }
        public F_MENU_KOL_MOIN_TAFZIL(object open_arg = null, string _AZ_TARIKH_ = "0", string _TA_TARIKH_ = "999999999999", Window? _ownerwin_ = null)
        {
            InitializeComponent();

            if (!(open_arg is null))
            {
                OPEN_ARG = open_arg;

                if (_ownerwin_ != null)
                {
                    THEOWENER = _ownerwin_;
                }

                if (!string.IsNullOrEmpty(_AZ_TARIKH_) && !string.IsNullOrEmpty(_TA_TARIKH_))
                {
                    AZ_DT_PARAM = _AZ_TARIKH_;
                    TA_DT_PARAM = _TA_TARIKH_;
                }

                if (open_arg == "TAF")
                {
                    LBL_WIN.Content = "گزارش چاپی دفتر تفضیلی";
                }
                else if (open_arg == "NABZMOSH")
                {
                    LBL_WIN.Content = "بررسی وضعیت مشتری";
                }
                else if (open_arg == "NABZKAR")
                {
                    LBL_WIN.Content = "بررسی وضعیت کارشناس";
                }
                else if (open_arg == "VAZ")
                {
                    LBL_WIN.Content = "صورت وضعیت معاملات اشخاص";

                    DT2.Text = Tarikh.FullCurrentDate;
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

                    BTN_PROCCESS_Click(null, null);
                }
            }
            //this.Owner = PublicVRB.WINBASE; // برای اینکه وقتی هرچند بار که پنجره رو باز میکنی روی پنجره اصلی باز بشه و باز بمونه نره اون پشت
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

            try
            {
                if (e.Key == Key.Escape)
                {
                    this?.Close();
                }
            }
            catch { }
            //Command5
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowReady = true;

            Keyboard.Focus(Combo34);
            Combo34.Focus();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if (OPEN_ARG is not null)
            {
                string PERNAME = "";
                if (OPEN_ARG == "NABZMOSH")
                {
                    PERNAME = "NABZKAR";
                }
                else if (OPEN_ARG == "NABZKAR")
                {
                    PERNAME = "NABZKAR";
                }
                else if (OPEN_ARG == "VAZ")
                {
                    PERNAME = "VAZ"; //صورت وضعیت معاملات اشخاص
                }
                else if (OPEN_ARG == "TAF")
                {
                    PERNAME = "HTAF"; //F8 چاپی
                }
                else if (OPEN_ARG != "TAF")
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(PERNAME))
                {
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, PERNAME, new WindowInteropHelper(this).Handle, this.GetType().Name);
                    if (!this.IsLoaded)
                    {
                        this.Close();
                        return;
                    }
                }
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

        private void BTN_PROCCESS_Click(object sender, RoutedEventArgs e)
        {
            if (Combo34.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "لطفا یک نام مشتری را انتخاب کنید"); msgwin.ShowDialog();
                return;
            }

            if (CL_HESABDARI.ISTAF(Combo34.SelectedValue.ToString()))
            {
                Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                msgwin.ShowDialog();
                return;
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

            //double KOL = default, MOIN = default, taf = default, TAF2 = default, taf3 = default, taf4 = default;
            double? KOL = null, MOIN = null, taf = null, TAF2 = null, taf3 = null, taf4 = null;
            if (!string.IsNullOrEmpty(this.Combo34.SelectedValue.ToStringNullSafe()))
            {
                _ = CL_HESABDARI.GETTAF3(this.Combo34.SelectedValue.ToString(), ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);
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
                        SORTT = "ORDER BY N_S, BED DESC, id";
                        break;
                    }
                case 2:
                    {
                        SORTT = "ORDER BY base, BED DESC, id";
                        break;
                    }
                case 3:
                    {
                        SORTT = "ORDER BY DATE_S, BED DESC, id";
                        break;
                    }
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

                    var SortPass = SORTT?.Replace("ORDER BY", null);
                    report.Dictionary.Variables.Add("SORTY", SortPass);

                    //report.Render();
                    ProcLoader.Stop(Prc);

                    new WINRPT(report, "گزارش دفتر تفضیلی").Show();
                    //report.Show();

                    #endregion


                    //DoCmd.OpenReport("R_DAFTAR_TAFZILY_2_2", acViewPreview);
                }
            }
            else if (OPEN_ARG == "VAZ")
            {
                //DoCmd.OpenReport("R_GARDESH_KHFR_DAFTAR", acViewPreview);
                GenerateReport(Prc);
            }
            else if (OPEN_ARG == "FRKMA4")
            {
                //DoCmd.OpenReport("R_GARDESH_KHFR_DAFTAR_A4", acViewPreview);


                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.R_GARDESH_KHFR_DAFTAR_A4.mrt");
                report.Load(pathreport);
                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
                report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
                report["CUST_PARM"] = Combo34.SelectedValue.ToString();

                string dt1 = $"از تاریخ : {DT1.Text}";
                string dt2 = $"تا تاریخ : {DT2.Text}";
                string dt3 = $"تاریخ : {Tarikh.FullCurrentDate.ToString()}";

                (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
                (report.GetComponentByName("DT1_N") as StiText).Text = dt1;
                (report.GetComponentByName("DT2_N") as StiText).Text = dt2;
                (report.GetComponentByName("DT2_N") as StiText).Text = dt3;

                new WINRPT(report, "صورت وضعیت معاملات تاریخ چک").Show();
            }
            else if (OPEN_ARG == "NABZMOSH") //بررسی وضعیت مشتری
            {
                new NABZ_MOSHTARI(Combo34.SelectedValue.ToString()).Show();
                ProcLoader.Stop(Prc);
            }
            else if (OPEN_ARG == "NABZKAR") //بررسی وضعیت کارشناس
            {
                new NABZKAR(Combo34.SelectedValue.ToString()).Show();
                ProcLoader.Stop(Prc);
            }
            else
            {
                if (OPEN_ARG != "TAF")  //if (this.OpenArgs == "RMOIN")
                {
                    var F_AZ = DT1.Text.ToRawTarikh();
                    var F_TA = DT2.Text.ToRawTarikh();
                    if (this.IsVisible && !string.IsNullOrEmpty(F_AZ) && !string.IsNullOrEmpty(F_TA))
                    {
                        AZ_DT_PARAM = F_AZ;
                        TA_DT_PARAM = F_TA;
                    }

                    if (true)
                    {
                        string tempTableName = $"MOIN{Baseknow.USERCOD}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}".Substring(0, 50);
                        string tempTableSqlName = $"dbo.[{tempTableName}]";
                        string QRE = "SELECT TOP(2000000000000)   N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES, N'بد' AS TSH INTO " + tempTableSqlName + "  FROM   " +
                            "      dbo.QDAFTARTAFZIL2_H(" + AZ_DT_PARAM + " , " + TA_DT_PARAM + " , '" + this.Combo34.SelectedValue + "') QDAFTARTAFZIL2_H " + SORTT;
                        dbms.DoExecuteSQL(QRE);
                        MAN = 0d;
                        var rst = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM {tempTableSqlName} {SORTT}").ToList();
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
                            dbms.DoExecuteSQL($"UPDATE {tempTableSqlName} SET MAND = {MAN} , TSH = N'{tashkhis}' WHERE id = {rst[rst_EOF].id}");
                        }

                        R_DAFTAR_MOIN_LIST r_DAFTAR_MOIN_LIST = new R_DAFTAR_MOIN_LIST($"{tempTableSqlName} {SORTT}", Combo34.SelectedValue.ToStringNullSafe() + $" {Combo36.Text} ", tempTableName);
                        ProcLoader.Stop(Prc);

                        if (OPEN_ARG is not null)
                        {
                            this.Close();
                        }

                        if (THEOWENER != null)
                        {
                            r_DAFTAR_MOIN_LIST.Owner = THEOWENER;
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
            }

            try { this?.Close(); } catch (Exception) { }
        }

        private void GenerateReport(Process Prc)
        {
            // -----------------------------
            // 1) Safety / Validation
            // -----------------------------
            if (DT1 == null || DT2 == null)
                throw new InvalidOperationException("DT1/DT2 is null.");

            var fdateRaw = DT1.Text.ToRawTarikh().ToString();
            var edateRaw = DT2.Text.ToRawTarikh().ToString();

            if (string.IsNullOrWhiteSpace(fdateRaw) || string.IsNullOrWhiteSpace(edateRaw))
                throw new InvalidOperationException("تاریخ شروع/پایان معتبر نیست.");

            if (Combo34?.SelectedValue == null || string.IsNullOrWhiteSpace(Combo34.SelectedValue.ToString()))
                throw new InvalidOperationException("شخص/تفصیلی انتخاب نشده است.");

            // این همان HTAF در Access است (تفصیلی)

            // این دو مقدار در Access از فرم می‌آید (HKOL/HMOIN)
            // اینجا باید از متغیرها/کنترل‌های واقعی پروژه خودتان بردارید:
            string CustomerHes = Combo34.SelectedValue.ToString();
            long hkol = CL_HESABDARI.GETKOL(CustomerHes);   // <-- اگر ندارید: از کنترل مربوطه بخوانید
            long hmoin = CL_HESABDARI.GETMOIN(CustomerHes); // <-- اگر ندارید: از کنترل مربوطه بخوانید
            long htaf = CL_HESABDARI.GETTAF(CustomerHes); // <-- اگر ندارید: از کنترل مربوطه بخوانید

            // -----------------------------
            // 2) Load report + DB
            // -----------------------------
            var report = new StiReport();
            using (var pathreport = Assembly.GetEntryAssembly()!
                       .GetManifestResourceStream("Prg_UI.Rpts.Factors.R_GARSESH_KHFR_DAFTAR.mrt"))
            {
                if (pathreport == null)
                    throw new InvalidOperationException("فایل گزارش (Embedded Resource) پیدا نشد: R_GARSESH_KHFR_DAFTAR.mrt");

                report.Load(pathreport);
            }

            string connstr = CL_CCNNMANAGER.CONNECTION_STR;
            if (!connstr.TrimEnd().EndsWith(";")) connstr += ";";
            connstr += "Connect Timeout=900;";

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            // -----------------------------
            // 3) Pass parameters (same as current code)
            // -----------------------------
            report["FDATE_PARM"] = fdateRaw;
            report["EDATE_PARM"] = edateRaw;
            report["CUST_PARM"] = CustomerHes;
            (report.GetComponentByName("DT3_N") as StiText).Text = Tarikh.FullCurrentDate;


            SafeSetText(report, "SAL_N", Baseknow.WIDTH_D.ToString());
            SafeSetText(report, "DT1_N", $"از تاریخ : {DT1.Text}");
            SafeSetText(report, "DT2_N", $"تا تاریخ : {DT2.Text}");

            // -----------------------------
            // 4) Access-equivalent "Ledger Balance" (SBK/SBS/SBSB/TASH)
            //    دقیقا مثل Report_Open در Access
            // -----------------------------
            var sums = dbms.DoGetDataSQL<SumBedBesModel>(
                @"SELECT 
             SUM(DEED_DTL.BED) AS SumOfBED,
             SUM(DEED_DTL.BES) AS SumOfBES
           FROM DEED_DTL
           WHERE 
             DEED_DTL.HES_K = @HKOL
             AND DEED_DTL.HES_M = @HMOIN
             AND DEED_DTL.HES_t = @HTAF",
                new { HKOL = hkol, HMOIN = hmoin, HTAF = htaf }
            ).FirstOrDefault();

            double sumBed = sums?.SumOfBED ?? 0;
            double sumBes = sums?.SumOfBES ?? 0;
            double diff = sumBed - sumBes;

            // معادل: SBK/SBS/SBSB/TASH در Access
            // در mrt شما این‌ها این اسم‌ها هستند (همان‌هایی که الآن 0 هستند):
            // Text50: SumBed (بدهکار)  | Text49: SumBes (بستانکار)
            // Text47: Abs(diff) (مانده) | Text48: بد/بس
            SafeSetText(report, "Text50", FormatMoney(sumBed));          // بدهکار
            SafeSetText(report, "Text49", FormatMoney(sumBes));          // بستانکار
            SafeSetText(report, "Text47", FormatMoney(Math.Abs(diff)));  // مانده
            SafeSetText(report, "Text48", diff > 0 ? "بد" : "بس");       // تش


            /*
             * SBK //بدهکار --Text50
               SBS //بستانکار --Text49
               TASH // تش --Text48
               SBSB //مانده --Text47
             */

            // -----------------------------
            // 5) DecimalDigits for MEGK like Access (DIG)
            //    Access: Me.MEGHk.DecimalPlaces = Forms![BASEKNOW]![DIG]
            // -----------------------------
            int dig = (int)Baseknow.DIG; // <-- همان DIG پروژه شما

            ApplyDecimalDigits(report, "Text5", dig);  // Text5 = {DataSource1.MEGK}

            // -----------------------------
            // 6) Show report (your way)
            // -----------------------------
            ProcLoader.Stop(Prc);
            new WINRPT(report, "صورت وضعیت معاملات اشخاص").Show();
        }
        private sealed class SumBedBesModel
        {
            public double? SumOfBED { get; set; }
            public double? SumOfBES { get; set; }
        }
        private static void SafeSetText(StiReport report, string componentName, string text)
        {
            var comp = report.GetComponentByName(componentName) as StiText;
            if (comp == null)
                return; // عمداً Silent: در پروژه شما شاید نام‌ها کمی فرق داشته باشد
            comp.Text = text ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(comp?.Text) && comp?.Text != "0")
            {
                comp.Enabled = true;
                var complabel = report.GetComponentByName("Text51") as StiText;
                complabel.Enabled = true;
            }
        }
        private static string FormatMoney(double value)
        {
            // شبیه Access "#,###" / Standard بدون اعشار
            return string.Format(CultureInfo.InvariantCulture, "{0:#,0}", value);
        }
        private static void ApplyDecimalDigits(StiReport report, string componentName, int decimalDigits)
        {
            if (decimalDigits < 0) decimalDigits = 0;
            if (decimalDigits > 6) decimalDigits = 6; // محافظه‌کارانه

            var comp = report.GetComponentByName(componentName) as StiText;
            if (comp == null) return;

            // اگر NumberFormat دارد همان را آپدیت می‌کنیم، در غیر اینصورت می‌سازیم
            var nf = comp.TextFormat as StiNumberFormatService;
            if (nf == null)
            {
                nf = new StiNumberFormatService();
                comp.TextFormat = nf;
            }

            nf.DecimalDigits = decimalDigits;
            nf.GroupSeparator = ",";
            nf.NegativePattern = 1;
            nf.State = StiTextFormatState.DecimalDigits;
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


            if (Combo34.SelectedValue is not null)
            {
                if ((Combo34.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text.Trim() == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("F_MENU_KOL_MOIN_TAFZIL", I_AM_F_MENU_KOL_MOIN_TAFZIL);
                CMBSearch.ShowDialog();
            }

            //var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(Combo34, dbms);
            //if (!string.IsNullOrEmpty(_SelectedHesab_?.hes))
            //{
            //    Combo34.SelectedValue = _SelectedHesab_.hes;
            //}


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
                ComboSearch CMBSearch = new ComboSearch("F_MENU_KOL_MOIN_TAFZIL", I_AM_F_MENU_KOL_MOIN_TAFZIL);//Search Plusy Form Specialy for Customers
                CMBSearch.ShowDialog();
                return;
            }
        }
        private void Label_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

        }
        private void Label_PreviewGotKeyboardFocus_1(object sender, KeyboardFocusChangedEventArgs e)
        {

        }
        private void DT1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT1.SelectAll();
        }
        private void DT2_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT2.SelectAll();
        }
    }
}
