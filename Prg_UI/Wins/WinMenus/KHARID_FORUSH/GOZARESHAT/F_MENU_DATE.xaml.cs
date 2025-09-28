using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Rpts;
using static Prg_UI.Functions.CL_LMethods;
using System.Diagnostics;
using Wins.WinMenus.HESABDARI.GOZARESHAT;
using DocumentFormat.OpenXml.Spreadsheet;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using System.Windows.Controls;
using Prg_UI.Wins.WinMenus.SANATI;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class F_MENU_DATE : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
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
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion
        public F_MENU_DATE(string _TAG_)
        {
            InitializeComponent();

            this.Tag = _TAG_;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }

        public string _sqlquery_ { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            switch (this.Tag.ToString())
            {
                case "SUDZIAN":
                    //this.HelpContext = 301;
                    break;
                case "ZIANHADE":
                    //this.HelpContext = 302;
                    break;
                case "BEDBESM":
                    DT1.Visibility = Visibility.Collapsed;
                    LABEL_WIN_HEADER.Content = "لیست بدهکاران و بستانکاران محدود شده";
                    break;
                case "TDBARG":
                    DT1.Visibility = Visibility.Hidden;
                    DT1.IsEnabled = false;

                    DT2.Text = Tarikh.FullCurrentDate.ToString();
                    DT2.Focus();
                    break;
                case "FRCUST":
                    LABEL_WIN_HEADER.Content = "گزارش ارزش افزوده فروش - گزارش فصلی";
                    break;
                case "FASLIBR":
                    LABEL_WIN_HEADER.Content = "گزارش فصلی برگشت فروش";
                    break;
                case "FASLIKHBR":
                    LABEL_WIN_HEADER.Content = "گزارش فصلی برگشت خرید";
                    break;
                case "KHCUST":
                    LABEL_WIN_HEADER.Content = "گزارش ارزش افزوده خرید - گزارش فصلی";
                    break;
                case "KHLS":
                    LABEL_WIN_HEADER.Content = "گزارش خرید به تفکیک فاکتور";
                    break;
                case "LFACT":
                    LABEL_WIN_HEADER.Content = "گزارش فروش به تفکیک فاکتور";
                    break;
            }


            if (DT1.Visibility == Visibility.Visible)
            {
                DT1.Focus();
                DT1.SelectAll();
                DT1.Text = Baseknow.YEA + "01" + "01";
            }
            else
            {
                DT2.Focus();
                DT2.SelectAll();
                DT2.Text = Tarikh.LastDayOfCurrentMonth;
            }

        }
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            string dt1 = DT1.Text.ToRawTarikh();
            string dt2 = DT2.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(dt1) || !string.IsNullOrEmpty(dt2))
            {
                if (!Tarikh.IsValidedDate(dt1) || !Tarikh.IsValidedDate(dt2))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(DT1.Text))
                {
                    if (this.Tag.ToString() == "HBGHB")
                    {
                        DT1.Text = Tarikh.FullCurrentDate;
                    }
                    else
                    {
                        DT1.Text = Baseknow.YEA + "0101";
                    }
                }
                if (string.IsNullOrEmpty(DT2.Text))
                {
                    DT2.Text = Tarikh.FullCurrentDate;
                }

                this.Close(); //OpenForm("F_MENU_DATE", Visibility.Hidden);

                Process Prc = ProcLoader.Start();

                switch (this.Tag.ToString())
                {
                    case "DFTROOS":
                        dbms.DoExecuteSQL($"DELETE FROM dbo.DEAD_DTL_PRINT WHERE     (UNAME = '{CL_HESABDARI.UCurrentUser()}')");
                        dbms.DoExecuteSQL($"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'{CL_HESABDARI.UCurrentUser()}' AS Expr1 FROM dbo.DEAD_WITH_GRP WHERE     (DATE_S BETWEEN {DT1.Text.ToRawTarikh()} AND {DT2.Text.ToRawTarikh()})");
                        dbms.DoExecuteSQL($"INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'{CL_HESABDARI.UCurrentUser()}' AS Expr1 FROM dbo.DEAD_WITH_GRP1 WHERE    (DATE_S BETWEEN {DT1.Text.ToRawTarikh()} AND {DT2.Text.ToRawTarikh()})");
                        Open_Report();
                        //OpenForm("F_MENU_DATE", Visibility.Hidden);
                        break;

                    case "DPDAY":
                        if (string.IsNullOrEmpty(DT1.Text) && string.IsNullOrEmpty(DT2.Text))
                        {
                            Open_Report2();
                        }
                        else
                        {
                            Open_Report3();
                        }
                        break;

                    case "EXIT":
                        //OpenReport("R_EXIT");
                        break;

                    case "CROS":
                        new GOZARESH_FROOSH_MAHSUL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //گزارش فروش روزانه 
                        break;

                    case "CFRALL":
                        new FROOSH_COUNTALL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //لیست فروش روزانه به تفکیک نوع کالا
                        break;

                    case "AMTOL":
                        //OpenForm("AMAR_TOLID");
                        new AMAR_TOLID(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //لیست فروش روزانه به تفکیک نوع کالا
                        break;

                    case "CAMTOL":
                        //OpenForm("AMAR_TOLID", FormViewMode.PivotChart);
                        break;

                    case "AMMAS":
                        //OpenForm("GOZARESH_MASRAF_MAVAD");
                        new GOZARESH_MASRAF_MAVAD(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //لیست آمار مواد مصرف

                        break;

                    case "COMP":
                        //OpenForm("SUDZIANGHEMAT");
                        break;

                    case "AMFDAY":
                        new AMAR_FROOSH_DAYLY_MAIN(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //آمار فروش کالا ها به تفکیک روز
                        break;

                    case "LFACT":
                        //OpenForm("Q_LIST_DALY");
                        new WIN_Q_LIST_DALY(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "LFACT").Show(); //گزارش فروش به تفکیک فاکتور
                        break;

                    case "FRCUST":
                        new WIN_Q_FROOSH_CUSTOMER(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "FRCUST").Show(); //فروش مشتریان // گزارش ارزش افزوده فروش - گزارش فصلی
                        break;

                    case "HBGHB":
                        new HAVALAH_WITHUT_BASKOOL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //گزارش ارزش افزوده خرید - گزارش فصلی
                        break;

                    case "KHCUST":
                        //OpenForm("Q_KHARID_CUSTOMER");
                        new WIN_Q_FROOSH_CUSTOMER(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "KHCUST").Show(); //گزارش ارزش افزوده خرید - گزارش فصلی
                        break;

                    case "BEDBESM":
                        ProcessBEDBESM();
                        break;

                    case "CUSTNP":
                        new CUSTOMER_DIDNOT_PURCHASE().Show(); //لیست فروش روزانه به تفکیک نوع کالا
                        break;

                    case "KHLS":
                        //OpenForm("LIST_KHRID_KOL_SUB");
                        new WIN_FACTOR_SPLITS(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "KHLS").Show(); //گزارش خرید به تفکیک فاکتور
                        break;

                    case "FASLIBR":
                        //QSL_FASLI_BARGASHTI
                        new WIN_Q_FROOSH_CUSTOMER(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "FASLIBR").Show(); //برگشت فروش مشتریان // گزارش فصلی برگشت فروش
                        break;

                    case "FASLIKHBR":
                        //OpenForm("QSL_FASLI_BARGASHTI_KH");
                        new WIN_Q_FROOSH_CUSTOMER(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), "FASLIKHBR").Show(); //گزارش فصلی برگشت خرید
                        break;

                    case "FONAR":
                        //OpenForm("froosh_naraftah");
                        break;

                    case "SUDFR":
                        //OpenForm("SOUD_FROOSH");
                        break;

                    case "CKRALL":
                        new KHAREED_COUNTALL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); //گزارش فصلی برگشت خرید
                        break;

                    case "TDBARG":
                        //OpenReport("TODAYBARGIRI");

                        Open_Report4();
                        break;

                    default:
                        //HAVALAH_ENTER_mavad_genereat(DT1.Text, DT2.Text); //Should be uncomment later
                        break;
                }

                ProcLoader.Stop(Prc);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
            }
        }
        private void ProcessBEDBESM()
        {
            var userCod = Baseknow.USERCOD.ToString();
            //var blockHesRecords = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCK_HES WHERE USERCO = {userCod}");
            //var blockNonHesRecords = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCKNON_HES WHERE USERCO = {userCod}");

            //string sh = BuildShString(blockHesRecords, blockNonHesRecords);
            //string sqlQuery = BuildSqlQuery(sh, userCod);
            dbms.DoExecuteSQL($"IF OBJECT_ID('dbo.BEDBESMAH{userCod}', 'U') IS NOT NULL   DROP TABLE dbo.BEDBESMAH{userCod}");

            //dbms.DoExecuteSQL(sqlQuery, new { DT2 = Convert.ToInt64(DT2.Text.ToRawTarikh()) });

            CreateTempTableData();
            new TARAZ_4("0", DT2.Text.ToRawTarikh(), true).Show();
        }

        private void CreateTempTableData()
        {
            int userCod = int.Parse(Baseknow.USERCOD.ToString());
            string dateVal = DT2.Text.ToRawTarikh();

            string shCondition = "";

            var rst2 = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCK_HES WHERE USERCO = {userCod}").ToList();
            if (rst2.Count > 0)
            {
                string hes = rst2[0].HES;
                shCondition = CL_HESABDARI.ISHESAB3(hes)
                    ? $"HES NOT LIKE '{hes}'"
                    : $"HES NOT LIKE '{hes}-%'";

                for (int i = 1; i < rst2.Count; i++)
                {
                    hes = rst2[i].HES;
                    shCondition += CL_HESABDARI.ISHESAB3(hes)
                        ? $" AND HES NOT LIKE '{hes}'"
                        : $" AND HES NOT LIKE '{hes}-%'";
                }
            }

            var rst3 = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCKNON_HES WHERE USERCO = {userCod}").ToList();
            if (rst3.Count > 0)
            {
                string nonCondition = "";
                foreach (var row in rst3)
                {
                    string hes = row.HES;
                    nonCondition += nonCondition == ""
                        ? (CL_HESABDARI.ISHESAB3(hes) ? $"HES LIKE '{hes}'" : $"HES LIKE '{hes}-%'")
                        : (CL_HESABDARI.ISHESAB3(hes) ? $" OR HES LIKE '{hes}'" : $" OR HES LIKE '{hes}-%'");
                }
                if (!string.IsNullOrEmpty(shCondition))
                {
                    shCondition = $"({shCondition}) OR ({nonCondition})";
                }
                else
                {
                    shCondition = $"({nonCondition})";
                }
            }

            string tableName = $"BEDBESMAH{userCod}";
            dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') DROP TABLE {tableName}");

            string sqlCommon = $@"
                                  SELECT   Q.TAFZIL, Q.HES_K, Q.HES_M, Q.SumOfBED, Q.SumOfBES, Q.BEDBES, Q.NAME, Q.MOIN,
                                           dbo.UIIF(Q.BEDBES, '>', 0, Q.BEDBES, 0) AS BEDM,
                                           dbo.UIIF(Q.BEDBES, '<', 0, Q.BEDBES * -1, 0) AS BESM,
                                           Q.HES_T, Q.ADDRESS, Q.TEL, Q.CODE_E, Q.TOZIH, Q.HES, Q.ECODE, Q.CUST_COD, Q.ROUTE_NAME,
                                           dbo.Visit_route.HES AS VCOD, dbo.CUST_HESAB.NAME AS VNAME,
                                           Q.HES_T2, Q.HES_T3, Q.HES_T4, CUST_HESAB_1.NAME AS tafname
                                  INTO     {tableName}
                                  FROM     dbo.CUST_HESAB CUST_HESAB_1
                                  INNER JOIN dbo.Q_BEDEHBESTANHA_SUB({dateVal}) Q ON CUST_HESAB_1.hes = Q.HES
                                  LEFT OUTER JOIN dbo.CUST_HESAB
                                      INNER JOIN dbo.Visit_route ON dbo.CUST_HESAB.hes = dbo.Visit_route.HES
                                      ON Q.ROUTE_NAME = dbo.Visit_route.ROUTE_NAME";

            string finalSql = !string.IsNullOrWhiteSpace(shCondition)
                ? sqlCommon + $"\nWHERE {shCondition.Replace("HES", "Q.HES")}" // bind to the correct alias
                : sqlCommon;

            dbms.DoExecuteSQL(finalSql);
        }

        private string BuildShString(IEnumerable<dynamic> blockHesRecords, IEnumerable<dynamic> blockNonHesRecords)
        {
            string sh = "";
            foreach (var record in blockHesRecords)
            {
                if (string.IsNullOrEmpty(sh))
                {
                    sh = CL_HESABDARI.ISHESAB3(record.HES)
                        ? $"Q_BEDEHBESTANHA_SUB.HES NOT LIKE '{record.HES}'"
                        : $"Q_BEDEHBESTANHA_SUB.HES NOT LIKE '{record.HES}-%'";
                }
                else
                {
                    sh += CL_HESABDARI.ISHESAB3(record.HES)
                        ? $" AND Q_BEDEHBESTANHA_SUB.HES NOT LIKE '{record.HES}'"
                        : $" AND Q_BEDEHBESTANHA_SUB.HES NOT LIKE '{record.HES}-%'";
                }
            }

            if (blockNonHesRecords.Any())
            {
                sh = !string.IsNullOrEmpty(sh) ? $"({sh}) OR (" : "(";
                sh += string.Join(" OR ", blockNonHesRecords.Select(record =>
                    CL_HESABDARI.ISHESAB3(record.HES)
                        ? $"Q_BEDEHBESTANHA_SUB.HES LIKE '{record.HES}'"
                        : $"Q_BEDEHBESTANHA_SUB.HES LIKE '{record.HES}-%'"));
                sh += ")";
            }

            return sh;
        }

        private string BuildSqlQuery(string sh, string userCod)
        {
            string baseQuery = $@"SELECT Q_BEDEHBESTANHA_SUB.TAFZIL, Q_BEDEHBESTANHA_SUB.HES_K, Q_BEDEHBESTANHA_SUB.HES_M, 
                              Q_BEDEHBESTANHA_SUB.SumOfBED, Q_BEDEHBESTANHA_SUB.SumOfBES, Q_BEDEHBESTANHA_SUB.BEDBES, 
                              Q_BEDEHBESTANHA_SUB.NAME, Q_BEDEHBESTANHA_SUB.MOIN,
                              CASE WHEN Q_BEDEHBESTANHA_SUB.BEDBES > 0 THEN Q_BEDEHBESTANHA_SUB.BEDBES ELSE 0 END AS BEDM, 
                              CASE WHEN Q_BEDEHBESTANHA_SUB.BEDBES < 0 THEN ABS(Q_BEDEHBESTANHA_SUB.BEDBES) ELSE 0 END AS BESM, 
                              Q_BEDEHBESTANHA_SUB.HES_T, Q_BEDEHBESTANHA_SUB.ADDRESS, Q_BEDEHBESTANHA_SUB.TEL, 
                              Q_BEDEHBESTANHA_SUB.CODE_E, Q_BEDEHBESTANHA_SUB.TOZIH, Q_BEDEHBESTANHA_SUB.HES,
                              Q_BEDEHBESTANHA_SUB.ECODE, Q_BEDEHBESTANHA_SUB.CUST_COD, Q_BEDEHBESTANHA_SUB.ROUTE_NAME, 
                              dbo.Visit_route.HES AS VCOD, dbo.CUST_HESAB.NAME AS VNAME, Q_BEDEHBESTANHA_SUB.HES_T2, 
                              Q_BEDEHBESTANHA_SUB.HES_T3, Q_BEDEHBESTANHA_SUB.HES_T4, CUST_HESAB_1.NAME AS tafname 
                              INTO BEDBESMAH{userCod} 
                              FROM dbo.CUST_HESAB CUST_HESAB_1 
                              INNER JOIN dbo.Q_BEDEHBESTANHA_SUB(@DT2) Q_BEDEHBESTANHA_SUB 
                              ON CUST_HESAB_1.hes = Q_BEDEHBESTANHA_SUB.HES 
                              LEFT OUTER JOIN dbo.CUST_HESAB 
                              INNER JOIN dbo.Visit_route ON dbo.CUST_HESAB.hes = dbo.Visit_route.HES 
                              ON Q_BEDEHBESTANHA_SUB.ROUTE_NAME = dbo.Visit_route.ROUTE_NAME";

            if (!string.IsNullOrEmpty(sh))
            {
                baseQuery += $" WHERE ({sh})";
            }

            return baseQuery;
        }

        private void DT1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DT1.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void DT2_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DT2.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_GO.IsFocused) //Not Focused on this button
                {
                    e.Handled = true;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        public void Open_Report()
        {

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.DAFTAR_ROOZNAMEH.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1;
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2;

            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_WIN_HEADER.Content.ToStringNullSafe()).Show();
        }


        public void Open_Report2()
        {
            _sqlquery_ = $"SELECT FKNAME, FMNAME, FTNAME, NAMES, SHARH, MABL, nonames, TKNAME, TMNAME, TTNAME, DT, KK FROM dbo.PGET_HED_REP";


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DP_DAYLY.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report.Dictionary.Variables.Add("Q_PARM", _sqlquery_);

            var dt1 = $"از تاریخ {DT1.Text}";
            var dt2 = $"تا تاریخ {DT2.Text}";


            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1;
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2;
            (report.GetComponentByName("DT3_N") as StiText).Text = Tarikh.FullCurrentDate.ToString();

            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_WIN_HEADER.Content.ToStringNullSafe()).Show();
        }

        public void Open_Report3()
        {
            _sqlquery_ = $"SELECT FKNAME, FMNAME, FTNAME, NAMES, SHARH, MABL, nonames, TKNAME, TMNAME, TTNAME, DT, KK FROM dbo.PGET_HED_REP WHERE (DT >= {DT1.Text.ToRawTarikh()} And DT <= {DT2.Text.ToRawTarikh()})";


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DP_DAYLY.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report.Dictionary.Variables.Add("Q_PARM", _sqlquery_);

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = DT1.Text.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = DT2.Text.ToString();
            (report.GetComponentByName("DT3_N") as StiText).Text = Tarikh.FullCurrentDate.ToString();

            //report.Render();

            //report.Show();

            new WINRPT(report, LABEL_WIN_HEADER.Content.ToStringNullSafe()).Show();
        }

        public void Open_Report4()
        {
            _sqlquery_ = $"SELECT STUF_DEF.CODE, STUF_DEF.NAME, TCOD_VAHEDS.NAMES, SUM(INVO_LST.MEGH) AS smegh, SUM(INVO_LST.MEGHk) AS smeghk, INVO_LST.ANBAR, stuf_def_nfani.col9 FROM HEAD_LST INNER JOIN head_lst_log ON HEAD_LST.NUMBER = head_lst_log.NUMBER AND HEAD_LST.TAG = head_lst_log.TAGG INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG INNER JOIN TCOD_VAHEDS ON INVO_LST.VAHED_K = TCOD_VAHEDS.CODE INNER JOIN STUF_DEF ON INVO_LST.CODE = STUF_DEF.CODE LEFT OUTER JOIN stuf_def_nfani ON STUF_DEF.CODE = stuf_def_nfani.CODE WHERE (HEAD_LST.TAG <> 20) GROUP BY STUF_DEF.CODE, STUF_DEF.NAME, TCOD_VAHEDS.NAMES, INVO_LST.ANBAR, stuf_def_nfani.col9 HAVING (MAX(head_lst_log.UDATEF) = {DT2.Text.ToRawTarikh()})";


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.TODAYBARGIRI.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sqlquery_);

            (report.GetComponentByName("DATE_N") as StiText).Text = Tarikh.FullCurrentDate;
            (report.GetComponentByName("USER_N") as StiText).Text = Baseknow.UUSER.ToString();

            (report.GetComponentByName("TNAME") as StiText).Text = Baseknow.WIDTH_D.ToString();

            //report["DATE_S"] = DT2.Text.ToString();
            //report["DATE_F"] = DT2.Text.ToString();
            //report["ANBAR_F"] = ANBAR.Text.ToString();

            //report["AZDATE"] = Baseknow.YEA + "0101";
            //report["ANBAR"] = ANBAR.SelectedValue.ToString();

            //string TaTarikh = "99999999";
            //if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            //report["TADATE"] = TaTarikh;

            //report["KALACODE"] = KALA.SelectedValue.ToString();
            //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;

            //Report_Open:
            //(report.GetComponentByName("Table1_Cell17") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEG
            //(report.GetComponentByName("Table1_Cell14") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK

            //report.Render();
            //report.Show();

            new WINRPT(report, "شمارش روزانه").Show();
        }
    }
}
