using AUTO_BAZ.HelperWins;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for FMENU_TARAZ_4.xaml
    /// </summary>
    public partial class FMENU_TARAZ_4 : Window
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

        public FMENU_TARAZ_4(string _OpenArgs_)
        {
            OpenArgs = _OpenArgs_;
            InitializeComponent();
        }

        string OpenArgs = null;

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string _sql_query { get; set; }
        public string _sql_query_2 { get; set; }

        UniversControl universControl = new UniversControl();

        public Visual I_AM_TARAZ4 { get; set; }

        public int DTT { get; set; }
        public int MMOIN { get; set; }

        public class Q1
        {
            public int? USERCO { get; set; }
            public string? HES { get; set; }
        }

        public class Q2
        {
            public int? NUMBER { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_TARAZ4 = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            DT1.Text = Convert.ToString(Baseknow.YEA + "0101");

            DT1.Focus();

            #region LOADING...
            this.HHHS.Visibility = Visibility.Hidden;
            switch (this.OpenArgs)
            {
                case "FT4":
                    {
                        //this.HelpContextId = 5009;
                        LABEL_HEADER.Content = "ليست تراز آزمايشي چهارستوني کل";
                        break;
                    }
                case "RT4":
                    {
                        //this.HelpContextId = 5010;
                        break;
                    }
                case "FT4T":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        L_HHHM.Visibility = Visibility.Visible;
                        this.HHHM.Visibility = Visibility.Visible;
                        LABEL_HEADER.Content = "ليست تراز آزمايشي چهارستوني تفصيلي";
                        break;
                    }
                case "FT4M2":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        break;
                    }
                case "FT4M":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        LABEL_HEADER.Content = "ليست تراز آزمايشي چهار ستوني معين";
                        break;
                    }
                case "RFT4T2":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        L_HHHM.Visibility = Visibility.Visible;
                        this.HHHM.Visibility = Visibility.Visible;
                        break;
                    }
                case "RFT4T":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        L_HHHM.Visibility = Visibility.Visible;
                        this.HHHM.Visibility = Visibility.Visible;
                        break;
                    }
                case "OTHER":
                    {
                        L_HHHS.Visibility = Visibility.Visible;
                        this.HHHS.Visibility = Visibility.Visible;
                        L_HHHM.Visibility = Visibility.Visible;
                        this.HHHM.Visibility = Visibility.Visible;
                        break;
                    }
            }
            #endregion
        }

        public void Fill_ComboBoxes()
        {
            HHHS.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT NUMBER FROM TOTA_HES ORDER BY NUMBER").ToList();
            HHHS.DisplayMemberPath = "NUMBER";
            HHHS.SelectedValuePath = "NUMBER";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Command5.IsFocused)
                {
                    //Enter Key Continue
                }
                else
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {


            string sql;
            string PATH;
            int i;
            if (string.IsNullOrEmpty(this.DT1.Text.ToRawTarikh()))
            {
                DT1.Text = Convert.ToString(Baseknow.YEA + "0101");
            }
            if (string.IsNullOrEmpty(this.DT1.Text.ToRawTarikh()) || string.IsNullOrEmpty(this.DT2.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            if (this.HHHS.Visibility == Visibility.Visible && IsNull(this.HHHS.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "پارامتر ها کافی نیست");
                msgwin.ShowDialog();
                return;
                //this.HHHS.SelectedValue = "%";

            }
            if (this.HHHM.Visibility == Visibility.Visible && string.IsNullOrEmpty(this.HHHM.Text))
            {
                this.HHHM.Text = "%";
            }
            if (string.IsNullOrEmpty(this.SNDNUM1.Text) && string.IsNullOrEmpty(this.SNDNUM2.Text))
            {
                this.SNDNUM1.Text = "0";
                this.SNDNUM2.Text = "929292929";
            }
            if (string.IsNullOrEmpty(this.SNDNUM1.Text) && !string.IsNullOrEmpty(this.SNDNUM2.Text))
            {
                this.SNDNUM1.Text = "0";
            }
            if (!string.IsNullOrEmpty(this.SNDNUM1.Text) && string.IsNullOrEmpty(this.SNDNUM2.Text))
            {
                this.SNDNUM2.Text = "929292929";
            }
            switch (this.OpenArgs.ToUpper())
            {
                case "FT4":
                    {
                        //لیست تراز چهار ستونی کل
                        new TARAZ_4(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show();
                        break;
                    }
                case "FT4M":
                    {
                        new TARAZ_4_MOIN(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), HHHS.SelectedValue.ToString()).Show();
                        //ليست تراز آزمايشي چهار ستوني معين
                        break;
                    }
                case "FT4M2":
                    {
                        OpenReport();
                        break;
                    }
                case "FT4T":
                    {
                        //ليست تراز آزمايشي چهارستوني تفصيلي
                        if (this.HHHS.SelectedValue == "%")
                        {
                            var RST2 = dbms.DoGetDataSQL<Q1>("SELECT USERCO,HES FROM BLOCK_HES WHERE USERCO = " + Baseknow.USERCOD).ToList();
                            if (RST2.Count > 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "بعضي از حسابها براي شما مسدود است بنابر اين تراز را نمي توانيد ببينيد در صورت لزوم حساب كل مورد نظر كه مسدود نيست را انتخاب و مشاهده نماييد!");
                                msgwin.ShowDialog();

                                return;
                            }
                        }
                        else
                        {
                            new TARAZ_TAF_DIRECT(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), HHHS.SelectedValue.ToString(), HHHM.Text).Show();
                        }
                        if (this.HHHM.Text == "%")
                        {
                            //DoCmd.OpenForm("TARAZ_4_TAFZ2");
                        }
                        else
                        {
                            //DoCmd.OpenForm("TARAZ_4_TAFZ");
                        }
                        break;
                    }
                case "RFT4T":
                    {
                        OpenReport5();
                        break;
                    }
                case "RFT4T2":
                    {
                        if (this.HHHM.Text == "%")
                        {
                            //DoCmd.OpenReport("TARAZ4_TAFZIL22", acViewPreview);
                        }
                        else
                        {
                            //DoCmd.OpenReport("TARAZ4_TAFZIL2", acViewPreview);
                        }
                        break;
                    }
                case "RT4":
                    {
                        OpenReport4();
                        break;
                    }
                case "SKHOA":
                    {
                        //DoCmd.OpenReport("KHOLASAH", acViewPreview);
                        break;
                    }
                case "MARKAZ":
                    {
                        //DoCmd.OpenForm("MARKAZ", acFormDS);
                        new MARKAZ($"{DT1.Text.ToRawTarikh()}", $"{DT2.Text.ToRawTarikh()}", $"{SNDNUM1.Text}", $"{SNDNUM1.Text}").Show();
                        break;
                    }
                case "OTHER":
                    {
                        //DoCmd.DeleteObject(acTable, "TARAZ_DTL_1");
                        //dbms.DoExecuteSQL("SELECT dbo.TDETA_HES2.N_KOL, dbo.TDETA_HES2.NUMBER, dbo.TDETA_HES2.TNUMBER, dbo.DEED_DTL.HES_T2, dbo.DEED_DTL.HES_T3, dbo.DEED_DTL.HES_T4,   dbo.DEED_DTL.BED, dbo.DEED_DTL.BES, dbo.TDETA_HES2.ECODE INTO            dbo.TARAZ_DTL_1 FROM         dbo.DEED_HED INNER JOIN                       dbo.DEED_DTL ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S INNER JOIN                       dbo.TDETA_HES2 ON dbo.DEED_DTL.HES_K = dbo.TDETA_HES2.N_KOL AND dbo.DEED_DTL.HES_M = dbo.TDETA_HES2.NUMBER AND                       dbo.DEED_DTL.HES_T = dbo.TDETA_HES2.TNUMBER AND dbo.DEED_DTL.HES_T2 = dbo.TDETA_HES2.TNUMBER2 WHERE     (dbo.DEED_HED.DATE_S >= " + this.DT1.Text.ToRawTarikh() + ") AND (dbo.DEED_HED.DATE_S <= " + this.DT2.Text.ToRawTarikh() + ") AND (dbo.DEED_HED.N_S BETWEEN " + this.SNDNUM1.Text + " AND " + this.SNDNUM2.Text + ")");
                        OpenReport3();
                        break;
                    }
                case "MANABE":
                    {
                        OpenReport2();
                        //DoCmd.OpenReport("SURATMABEVAMASAREF", acViewPreview);
                        break;
                    }        
            }

            this.Close();
        }

        private void OpenReport()
        {
            //گزارش تراز آزمایشی حسابهای معین
            #region SecuritCheck
            try
            {
                string Formname = "TARAZ4M2";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion


            _sql_query = $"EXEC dbo.TARAZ4_MOIN '{DT1.Text.ToRawTarikh()}', '{DT2.Text.ToRawTarikh()}', '{SNDNUM1.Text}', '{SNDNUM2.Text}', '{HHHS.SelectedValue}';";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TARAZ_moin2.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            //report.Render();
            //report.Show();
            //تفکیک معین
            new Rpts.WINRPT(report, "تفکیک معین").Show();
        }

        private void OpenReport2()
        {
            _sql_query = $"SELECT NUMBER, NAM, CODE, NAMES, daraee, grp, GR, DARAEECH, daraeeE, BEDEHECH, DARAEECH + BEDEHECH * - 1 AS change FROM SURATMANABE({SNDNUM1.Text}, {DT1.Text.ToRawTarikh()}, {SNDNUM2.Text}, {DT2.Text.ToRawTarikh()}) SURATMANABE WHERE (GR = 1) AND (DARAEECH >= 0) OR (GR = 2) AND (BEDEHECH < 0) ORDER BY CODE";
            _sql_query_2 = $"SELECT NUMBER, NAM, CODE, NAMES, BEDEHE, grp, GR, BEDEHECH, BEDEHEE, DARAEECH, BEDEHECH + DARAEECH * - 1 AS change FROM SURATMANABE({SNDNUM1.Text}, {DT1.Text.ToRawTarikh()}, {SNDNUM2.Text}, {DT2.Text.ToRawTarikh()}) SURATMANABE WHERE     (GR = 2) AND (BEDEHECH >= 0) OR  (GR = 1) AND (DARAEECH < 0)";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.SURATMABEVAMASAREF.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM1", _sql_query);
            report.Dictionary.Variables.Add("Q_PARM2", _sql_query_2);

            string dt1 = $"کلیه اسناد تا سند : {SNDNUM2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();

            //report.Render();
            //report.Show();
            new Rpts.WINRPT(report, "تفکیک معین").Show();
        }

        private void OpenReport3()
        {
            //گزارش تراز آزمایشی  چهار ستونی تفصیلی
            //گزارش تراز کل ، معین ، تفضیلی و بالاتر
            #region SecuritCheck
            try
            {
                string Formname = "RTARAZ4T2";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

            _sql_query = $"SELECT KOLNAM, MOINAME, TAFNAME, SumOfBED, SumOfBES, bed, bes, N_KOL, NUMBER, TNUMBER, MOINJAM, KOLjam FROM dbo.TARAZ4_TAFZ('{DT1.Text.ToRawTarikh()}', '{DT2.Text.ToRawTarikh()}', '{SNDNUM1.Text}', '{SNDNUM2.Text}', '{HHHS.SelectedValue}', '{HHHM.Text}') OPTION (HASH JOIN, QUERYTRACEON 2312);";

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TARAZ4_TAFZIL_other.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            new Rpts.WINRPT(report, "تراز").Show();
        }

        private void OpenReport4()
        {
            //گزارش تراز آزمایشی چهار ستونی کل
            #region SecuritCheck
            try
            {
                string Formname = "RTARAZ4";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                if (!this.IsLoaded) { this.Close(); return; }//Final State Check
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion


            _sql_query = $"EXEC dbo.TARAZ_4 '{DT1.Text.ToRawTarikh()}', '{DT2.Text.ToRawTarikh()}', '{SNDNUM1.Text}', '{SNDNUM2.Text}';";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_TARAZ_4.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            new Rpts.WINRPT(report, "تراز").Show();
        }

        private void OpenReport5()
        {
            //گزارش تراز آزمایشی حسابهای  کل-معین-تفصیلی
            #region SecuritCheck
            try
            {
                string Formname = "RTARAZ4T";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                // 3. Final State Check:
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion


            _sql_query = $"SELECT KOLNAM, MOINAME, TAFNAME, SumOfBED, SumOfBES, bed, bes, MOINJAM, KOLjam, N_KOL, NUMBER, TNUMBER FROM dbo.TARAZ4_TAFZ('{DT1.Text.ToRawTarikh()}','{DT2.Text.ToRawTarikh()}','{SNDNUM1.Text}','{SNDNUM2.Text}','{HHHS.SelectedValue}','{HHHM.Text}') OPTION (HASH JOIN, QUERYTRACEON 2312);";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TARAZ4_TAFZIL.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "تراز").Show();
        }

        private void OpenReport6()
        {
            _sql_query = $"SELECT KOLNAM, MOINAME, TAFNAME, SumOfBED, SumOfBES, bed, bes, MOINJAM, KOLjam, N_KOL, NUMBER, TNUMBER FROM dbo.TARAZ4_TAFZ('{DT1.Text.ToRawTarikh()}','{DT2.Text.ToRawTarikh()}','{SNDNUM1.Text}','{SNDNUM2.Text}','{HHHS.SelectedValue}','{HHHM.Text}') OPTION (HASH JOIN, QUERYTRACEON 2312);";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TARAZ4_TAFZIL2.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            string dt1 = $"کلیه اسناد از تاریخ : {DT1.Text}";
            string dt2 = $"تا تاریخ : {DT2.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = dt2.ToString();

            new Rpts.WINRPT(report, "تراز").Show();
        }
    }
}
