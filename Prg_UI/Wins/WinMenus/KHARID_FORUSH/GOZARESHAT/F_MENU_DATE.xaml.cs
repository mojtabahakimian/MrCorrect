using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Base;
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
                    break;
                case "TDBARG":
                    DT1.Visibility = Visibility.Collapsed;
                    DT2.Text = Tarikh.FullCurrentDate;
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

                            dt1 = $"از تاریخ {DT1.Text}";
                            dt2 = $"تا تاریخ {DT2.Text}";


                            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
                            (report.GetComponentByName("DT1_N") as StiText).Text = dt1;
                            (report.GetComponentByName("DT2_N") as StiText).Text = dt2;
                            (report.GetComponentByName("DT3_N") as StiText).Text = Tarikh.FullCurrentDate.ToString();

                            //report.Render();
                            //report.Show();

                            new WINRPT(report, LABEL_WIN_HEADER.Content.ToStringNullSafe()).Show();
                        }
                        else
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
                        break;

                    case "CAMTOL":
                        //OpenForm("AMAR_TOLID", FormViewMode.PivotChart);
                        break;

                    case "AMMAS":
                        //OpenForm("GOZARESH_MASRAF_MAVAD");
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
            var blockHesRecords = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCK_HES WHERE USERCO = {userCod}");
            var blockNonHesRecords = dbms.DoGetDataSQL<dynamic>($"SELECT USERCO, HES FROM BLOCKNON_HES WHERE USERCO = {userCod}");

            string sh = BuildShString(blockHesRecords, blockNonHesRecords);

            string sqlQuery = BuildSqlQuery(sh, userCod);
            dbms.DoExecuteSQL(sqlQuery);

            //OpenForm("BEDBESKOL");
            //OpenForm("F_MENU_DATE", Visibility.Hidden);
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
    }
}
