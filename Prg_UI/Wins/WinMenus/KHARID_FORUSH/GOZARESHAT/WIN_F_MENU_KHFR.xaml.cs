using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using System.Diagnostics;
using Stimulsoft.Report.Components;
using Prg_UI.HelperWins;
using System.Collections.Generic;
using System;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class WIN_F_MENU_KHFR : Window
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

        public WIN_F_MENU_KHFR(string openargs)
        {
            InitializeComponent();

            OpenArgs = openargs;

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public Visual I_AM_WIN_F_MENU_KHFR { get; set; }
        public string OpenArgs { get; }

        UniversControl universControl = new UniversControl();

        public class Q1
        {
            public string? hes { get; set; }
        }

        public class Q2
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            var WINHANDLE = new WindowInteropHelper(this).Handle;
            I_AM_WIN_F_MENU_KHFR = CL_LMethods.GetTheWindow(WINHANDLE);

            switch (OpenArgs)
            {
                //case "F":
                case "FROOSHDAY":
                    WIN_HEADER_NAME.Content = "خلاصه فروش روزانه به تفكيك اشخاص";
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, "FROOSHDAY", WINHANDLE, this.GetType().Name);
                    break;

                //case "K":
                case "FKHAREDAY":
                    WIN_HEADER_NAME.Content = "خلاصه خـريد روزانه به تفكيك اشخاص";
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, "FKHAREDAY", WINHANDLE, this.GetType().Name);
                    break;

                default: break;
            }

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            DT1.Text = Tarikh.FirstDayOfCurrentMonth;
            DT2.Text = Tarikh.LastDayOfCurrentMonth;

            HMOIN.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            var dataSource = new List<Custom_CUST_HESAB>();

            // کد حساب
            HHMOIN.ItemsSource = dataSource;
            // نام حساب
            HMOIN.ItemsSource = dataSource;
        }
        private void HMOIN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HMOIN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (!NowIsReady) { return; }


            TextBox CUTSNO_TEX = (TextBox)HMOIN.Template.FindName("PART_EditableTextBox", HMOIN);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (HMOIN.SelectedValue is not null)
            {
                if ((HMOIN.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HMOIN, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                //universControl.PopNotifyShow($"حساب خالی است", Pop1, Pop1Text1, Pop_Border1);
            }

            if (HMOIN.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HMOIN.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    HMOIN.SelectedValue = null;
                }
                //if (CL_HESABDARI.BLOCKEDCUST(HMOIN.SelectedValue.ToString()))
                //{
                //    HMOIN.SelectedItem = null;
                //    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                //    return;
                //}
            }
        }
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            //string mmoin = "%";
            string mmoin = HMOIN.SelectedValue?.ToString() ?? "%";

            string dt1 = DT1.Text.ToRawTarikh();
            string dt2 = DT2.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(dt1) || !string.IsNullOrEmpty(dt2))
            {
                if (!Tarikh.IsValidedDate(dt1) || !Tarikh.IsValidedDate(dt2))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    //if (!Tarikh.IsSyncedDateNow(dt1, (bool)Baseknow.CTL_DT) || !Tarikh.IsSyncedDateNow(dt2, (bool)Baseknow.CTL_DT))
                    //{
                    //    universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                    //    return;
                    //}
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            byte kindp = 1;
            if (Option23.IsChecked == true)
            {
                kindp = 1;
            }
            else
            {
                kindp = 2;
            }

            this.Close();

            Process Prc = ProcLoader.Start();

            switch (OpenArgs)
            {
                //case "F": //خلاصه فروش روزانه به تفكيك اشخاص
                case "FROOSHDAY":
                    if (mmoin == "%")
                    {
                        //OpenReport("R_FROOSH_DAYLY1");


                        var report = new StiReport();
                        var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.R_FROOSH_DAYLY_1.mrt");
                        report.Load(pathreport);
                        string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                        report.Dictionary.Databases.Clear();
                        report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                        report["AZDATE"] = DT1.Text.ToRawTarikh();
                        report["TADATE"] = DT2.Text.ToRawTarikh();
                        ((StiSqlSource)report.Dictionary.DataSources["Q_FROOSH_DAYLY1"]).CommandTimeout = 900;

                        //report.Render();
                        //report.Show();

                        new Rpts.WINRPT(report, WIN_HEADER_NAME.Content.ToStringNullSafe()).Show();
                    }
                    else
                    {
                        //OpenReport("R_FROOSH_DAYLY1", $"hes = '{mmoin}'");
                    }
                    break;

                //case "K": //خلاصه خـريد روزانه به تفكيك اشخاص 
                case "FKHAREDAY":
                    if (true)
                    {
                        //OpenReport("R_KHARED_DAYLY");


                        var report = new StiReport();
                        var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.R_KHARED_DAYLY_1.mrt");
                        report.Load(pathreport);
                        string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                        report.Dictionary.Databases.Clear();
                        report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                        report["AZDATE"] = DT1.Text.ToRawTarikh();
                        report["TADATE"] = DT2.Text.ToRawTarikh();
                        ((StiSqlSource)report.Dictionary.DataSources["q_khreed_dayly"]).CommandTimeout = 900;

                        //report.Render();
                        //report.Show();

                        new Rpts.WINRPT(report, WIN_HEADER_NAME.Content.ToStringNullSafe()).Show();
                    }
                    break;

                case "DP":
                    //OpenReport("R_DP_DAYLY_ASHKHAS", $"DT >= '{dt1}' AND DT <= '{dt2}'");
                    break;

                case "PCHS":
                    //OpenForm("CHEK_PLISTS");
                    break;

                case "FLIST": //گزارش فاکتور های فروش به اشخاص
                    int tkhf = (int)Baseknow.TKHF;
                    if (tkhf < 3)
                    {
                        //OpenReport("LIST_INVOICE_FROOSH");
                    }
                    else
                    {
                        if (kindp == 1)
                        {
                            //OpenReport("INVOICE_FROOSH_3GRP3");

                            //if (HMOIN.SelectedValue is null)
                            //{
                            //    universControl.PopNotifyShow("لطفا یک حساب را انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1);
                            //    return;
                            //}

                            var report = new StiReport();
                            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_3GRP3.mrt");
                            report.Load(pathreport);
                            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                            report.Dictionary.Databases.Clear();
                            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                            report["FDATE_PARM"] = DT1.Text.ToRawTarikh();
                            report["EDATE_PARM"] = DT2.Text.ToRawTarikh();
                            if (HMOIN.SelectedValue is null)
                            {
                                report["CUST_PARM"] = "%";
                            }
                            else
                            {
                                report["CUST_PARM"] = HMOIN.SelectedValue.ToString();
                            }
                            (report.GetComponentByName("Text73") as StiText).Text = $"تاریخ : {Tarikh.FullCurrentDate}";
                            (report.GetComponentByName("F_SELL_N") as StiText).Text = Baseknow.NAME.ToString();
                            (report.GetComponentByName("F_GLOBALNUMBER_N") as StiText).Text = Baseknow.ECODE.ToString();
                            (report.GetComponentByName("F_CODE_N") as StiText).Text = Baseknow.MCODEM.ToString();
                            //(report.GetComponentByName("F_OSTAN_N") as StiText).Text = Baseknow.IYALAT.ToString();
                            //(report.GetComponentByName("F_SHAHR_N") as StiText).Text = Baseknow.CITY.ToString();
                            //(report.GetComponentByName("F_POSTAL_N") as StiText).Text = Baseknow.PCODE.ToString();
                            (report.GetComponentByName("F_ADDRESS_N") as StiText).Text = Baseknow.TFADDRESS.ToString();
                            (report.GetComponentByName("F_TEL_N") as StiText).Text = Baseknow.TFTEL.ToString();

                            //((StiSqlSource)report.Dictionary.DataSources["q_khreed_dayly"]).CommandTimeout = 900;

                            //report.Render();
                            //report.Show();

                            new Rpts.WINRPT(report, WIN_HEADER_NAME.Content.ToStringNullSafe()).Show();
                        }
                        else
                        {
                            sql = $"SELECT * FROM HEAD_LST WHERE SADER = 0 AND tag = 13 AND date_n >= '{dt1}' AND date_n <= '{dt2}' AND cust_no LIKE '{mmoin}' ORDER BY NUMBER1";
                            //var results = dbms.DoGetDataSQL<HEAD_LST>(sql);

                            //foreach (var result in results)
                            //{
                            //    //OpenReport("INVOICE_FROOSH_2GR", $"(NUMBER = {result.NUMBER}) AND htag = 2");
                            //}
                        }
                    }
                    break;

                case "KLIST":
                    //OpenReport("LIST_INVOICE_KHARED");

                    Open_Report();
                    break;

                case "KHFR":
                    //OpenReport("KHOLASAH_FROOSH");
                    break;

                case "FONARP":
                    //OpenForm("froosh_naraftah_PERS", mmoin);
                    new FROOSH_NARAFTAH_PERS($"{HMOIN.SelectedValue.ToString()}").ShowDialog();
                    break;

                default: break;
            }
            ProcLoader.Stop(Prc);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (BTN_GO.IsFocused)
                {
                    BTN_GO.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        private void Open_Report()
        {
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.LIST_INVOICE_KHARED.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh();
            if (HMOIN.SelectedValue is null)
            {
                report["CUST_PARM"] = "%";
            }
            else
            {
                report["CUST_PARM"] = HMOIN.SelectedValue.ToString();
            }

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();

            //((StiSqlSource)report.Dictionary.DataSources["q_khreed_dayly"]).CommandTimeout = 900;

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, WIN_HEADER_NAME.Content.ToStringNullSafe()).Show();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => NowIsReady = true), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
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
