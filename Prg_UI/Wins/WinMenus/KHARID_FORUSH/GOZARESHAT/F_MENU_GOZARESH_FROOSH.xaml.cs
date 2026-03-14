using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Rpts;
using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinOther;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Stimulsoft.Base.StiDbType;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class F_MENU_GOZARESH_FROOSH : Window
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
        public F_MENU_GOZARESH_FROOSH(string _openargs_)
        {
            InitializeComponent();

            OpenArgs = _openargs_;

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public Visual I_AM_WIN_F_MENU_KHFR { get; set; }
        public string OpenArgs { get; }

        UniversControl universControl = new UniversControl();

        #region LOCAL
        public class FMG1
        {
            public double? naghd { get; set; }
            public int? countn { get; set; }
        }
        public class FMG2
        {
            public double? MAND { get; set; }
            public int? TEDAD { get; set; }
        }
        public class FMG3
        {
            public string? USER_NAME { get; set; }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //I_AM_WIN_F_MENU_KHFR = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);
            FILL_ALL_COMBOBOXES();

            //For_Open:
            DEPART.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPART.Items.Refresh();

            if (Baseknow.UGRP == "3")
            {
                DEPART.IsEnabled = false;
            }

            if (OpenArgs.ToUpper() == "FK")
            {
                SHIFT.Visibility = Visibility.Hidden;
                USERR.Visibility = Visibility.Hidden;
                WIN_HEADER_NAME.Content = "گزارش فروش روزانه کاربران";
            }
            else if (OpenArgs.ToUpper() == "F")
            {

                WIN_HEADER_NAME.Content = "گزارش خلاصه فروش روزانه ";
            }

            DEPART.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            DEPART.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>($"SELECT DEPATMAN, DEPNAME FROM dbo.DEPART").ToList();
            SHIFT.ItemsSource = dbms.DoGetDataSQL<SHIFT>($"SELECT SHIFT_ID, SHNAME FROM dbo.SHIFT").ToList();
            USERR.ItemsSource = dbms.DoGetDataSQL<FMG3>($"SELECT DISTINCT USER_NAME FROM dbo.HEAD_LST ORDER BY USER_NAME").ToList();
        }
        private void DTL_Click(object sender, RoutedEventArgs e)
        {
            bool DTL_Is_Checked = (bool)DTL.IsChecked;
            if (DTL_Is_Checked)
            {
                SHIFT_LABEL.Visibility = Visibility.Visible;
                SHIFT.Visibility = Visibility.Visible;
                USERNAME_LABEL.Visibility = Visibility.Visible;
                USERR.Visibility = Visibility.Visible;
            }
            else
            {
                SHIFT_LABEL.Visibility = Visibility.Hidden;
                SHIFT.Visibility = Visibility.Hidden;
                USERNAME_LABEL.Visibility = Visibility.Hidden;
                USERR.Visibility = Visibility.Hidden;
            }
        }

        private void OpenReport()
        {
            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.GOZARESH_FROOSH_USER.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            (report.GetComponentByName("DT1_N") as StiText).Text = DT1.Text.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = DT2.Text.ToString();

            report["USER_PARM"] = USERR.SelectedValue.ToString();
            report["SHIFT_PARM"] = DT2.Text.ToString();
            report["VAHED_PARM"] = DEPART.SelectedValue.ToString();
            report["FDATE_PARM"] = DT1.Text.ToRawTarikh();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh();

            new WINRPT(report, "لیست کالا ها جهت ویزیت").Show();
        }

        private void OpenReport_2()
        {
            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.LIST_FROOSH_ANBARS_DTL.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            (report.GetComponentByName("DT1_N") as StiText).Text = DT1.Text.ToString();
            (report.GetComponentByName("DT2_N") as StiText).Text = DT2.Text.ToString();


            report["USER_PARM"] = USERR.SelectedValue.ToString();
            report["SHIFT_PARM"] = SHIFT.SelectedValue.ToString();
            report["VAHED_PARM"] = DEPART.SelectedValue.ToString();
            report["FDATE_PARM"] = DT1.Text.ToRawTarikh();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh();

            new WINRPT(report, "لیست کالا ها جهت ویزیت").Show();
        }

        private void OpenReport_3()
        {
            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.GOZARESH_FROOSH_USER3.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            (report.GetComponentByName("AZDATE") as StiText).Text = DT1.Text.ToString();
            (report.GetComponentByName("TADATE") as StiText).Text = DT2.Text.ToString();


            var depart = DEPART.SelectedValue.ToString();
            var dt1 = DT1.Text.ToRawTarikh();
            var dt2 = DT2.Text.ToRawTarikh();

            report["DEPART_PARM"] = depart;
            report["FDATE_PARM"] = dt1;
            report["EDATE_PARM"] = dt2;

            // تجمیع تمامی کوئری‌های رویداد GroupFooter0_Format اکسس در یک دستور SQL بهینه‌شده
            string sqlAggregates = @"
                SELECT
                    ISNULL((SELECT COUNT(M_NAGHD) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND M_NAGHD > 0), 0) AS NaghdCount,
                    ISNULL((SELECT COUNT(TAKHFIF) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND TAKHFIF > 0), 0) AS TakhfifCount,
                    ISNULL((SELECT COUNT(MABL_VAR) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND MABL_VAR > 0), 0) AS VarCount,
                    ISNULL((SELECT COUNT(MABL_HAV) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND MABL_HAV > 0), 0) AS HavCount,
                    ISNULL((SELECT COUNT(P.MABL) FROM dbo.HEAD_LST H INNER JOIN dbo.PAY_GETD P ON H.TAG = P.TAG AND H.NUMBER = P.NUMBER WHERE (H.DATE_N BETWEEN @DT1 AND @DT2) AND H.DEPATMAN = @DEPART AND H.TAG = 2), 0) AS AsnCount,
                    ISNULL((SELECT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD > 0)), 0) AS BedMand,
                    ISNULL((SELECT COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD > 0)), 0) AS BedTedad,
                    ISNULL((SELECT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD < 0)), 0) AS BesMand,
                    ISNULL((SELECT COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD < 0)), 0) AS BesTedad
            ";

            // 1. دریافت مقادیر Footer گزارش
            var footerData = dbms.DoGetDataSQL<ReportFooterDto>(sqlAggregates, new { DT1 = dt1, DT2 = dt2, DEPART = depart }).FirstOrDefault();

            // 3. تزریق مقادیر محاسبه شده دیتابیس به متغیرهای گزارش
            if (footerData != null)
            {
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Naghd_Count", footerData.NaghdCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Takhfif_Count", footerData.TakhfifCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Var_Count", footerData.VarCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Hav_Count", footerData.HavCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Asn_Count", footerData.AsnCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bed_MAND", footerData.BedMand));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bed_TEDAD", footerData.BedTedad));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bes_MAND", footerData.BesMand));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bes_TEDAD", footerData.BesTedad));
            }

            // در اینجا می‌توانید داده‌های اصلی رکوردها را نیز Bind کنید
            // var mainData = dbms.DoGetDataSQL<YourMainModel>("SELECT * FROM dbo.GOZARESH_FROOSH_ROZANEH3(@DEPART, @DT1, @DT2)", new { DT1 = dt1, DT2 = dt2, DEPART = depart });
            // report.RegData("GOZARESH_FROOSH_ROZANEH3", mainData);


            new WINRPT(report, "گزارش خلاصه فروش روزانه کاربران").Show();
        }

        #region MyRegion
        // مدل کلاس برای دریافت مقادیر Footer از دیتابیس (حفظ نام‌گذاری‌های لگاسی)
        public class ReportFooterDto
        {
            public int NaghdCount { get; set; }
            public int TakhfifCount { get; set; }
            public int VarCount { get; set; }
            public int HavCount { get; set; }
            public int AsnCount { get; set; }
            public double BedMand { get; set; }
            public int BedTedad { get; set; }
            public double BesMand { get; set; }
            public int BesTedad { get; set; }
        }
        /// <summary>
        /// متد بارگذاری و نمایش گزارش فروش اداری
        /// </summary>
        private void PrintGozarehFroosh(long dt1, long dt2, int depart)
        {
            // تجمیع تمامی کوئری‌های رویداد GroupFooter0_Format اکسس در یک دستور SQL بهینه‌شده
            string sqlAggregates = @"
                SELECT
                    ISNULL((SELECT COUNT(M_NAGHD) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND M_NAGHD > 0), 0) AS NaghdCount,
                    ISNULL((SELECT COUNT(TAKHFIF) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND TAKHFIF > 0), 0) AS TakhfifCount,
                    ISNULL((SELECT COUNT(MABL_VAR) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND MABL_VAR > 0), 0) AS VarCount,
                    ISNULL((SELECT COUNT(MABL_HAV) FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN @DT1 AND @DT2) AND TAG = 13 AND DEPATMAN = @DEPART AND MABL_HAV > 0), 0) AS HavCount,
                    ISNULL((SELECT COUNT(P.MABL) FROM dbo.HEAD_LST H INNER JOIN dbo.PAY_GETD P ON H.TAG = P.TAG AND H.NUMBER = P.NUMBER WHERE (H.DATE_N BETWEEN @DT1 AND @DT2) AND H.DEPATMAN = @DEPART AND H.TAG = 2), 0) AS AsnCount,
                    ISNULL((SELECT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD > 0)), 0) AS BedMand,
                    ISNULL((SELECT COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD > 0)), 0) AS BedTedad,
                    ISNULL((SELECT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD < 0)), 0) AS BesMand,
                    ISNULL((SELECT COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) FROM dbo.GUSER_FACT_PERDEPART_SUB3(@DT1, @DT2, @DEPART, '%', '%') WHERE (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD < 0)), 0) AS BesTedad
            ";

            // 1. دریافت مقادیر Footer گزارش
            var footerData = dbms.DoGetDataSQL<ReportFooterDto>(sqlAggregates, new { DT1 = dt1, DT2 = dt2, DEPART = depart }).FirstOrDefault();

            // 2. مقداردهی فایل استیمول سافت
            StiReport report = new StiReport();
            report.Load("GOZARESH_FROOSH_USER3.mrt");

            // 3. تزریق مقادیر محاسبه شده دیتابیس به متغیرهای گزارش
            if (footerData != null)
            {
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Naghd_Count", footerData.NaghdCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Takhfif_Count", footerData.TakhfifCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Var_Count", footerData.VarCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Hav_Count", footerData.HavCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Asn_Count", footerData.AsnCount));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bed_MAND", footerData.BedMand));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bed_TEDAD", footerData.BedTedad));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bes_MAND", footerData.BesMand));
                report.Dictionary.Variables.Add(new StiVariable("", "Var_Bes_TEDAD", footerData.BesTedad));
            }

            // در اینجا می‌توانید داده‌های اصلی رکوردها را نیز Bind کنید
            // var mainData = dbms.DoGetDataSQL<YourMainModel>("SELECT * FROM dbo.GOZARESH_FROOSH_ROZANEH3(@DEPART, @DT1, @DT2)", new { DT1 = dt1, DT2 = dt2, DEPART = depart });
            // report.RegData("GOZARESH_FROOSH_ROZANEH3", mainData);

            report.Compile();
            report.Show();
        }
        #endregion

        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            if (USERR.Visibility == Visibility.Visible)
            {
                if (USERR.SelectedItem is null)
                {
                    new Msgwin(false, "کاربر نمیتواند خالی باشد").ShowDialog();
                    return;
                }

                if (SHIFT.SelectedItem is null)
                {
                    new Msgwin(false, "شیفت نمیتواند خالی باشد").ShowDialog();
                    return;
                }
            }

            try
            {
                string sql = string.Empty;
                string PATH = string.Empty;
                int i = 0;
                string SSHIFT = SHIFT.Text;

                // Handle null values
                if (string.IsNullOrEmpty(DT1.Text))
                {
                    DT1.Text = $"{Baseknow.YEA}0101";
                }

                if (string.IsNullOrEmpty(SHIFT.Text))
                {
                    SSHIFT = "%";
                }

                if (OpenArgs == "FR" && string.IsNullOrEmpty(USERR.Text))
                {
                    USERR.Text = "%";
                }

                if (string.IsNullOrEmpty(DT1.Text) || string.IsNullOrEmpty(DT2.Text) || string.IsNullOrEmpty(DEPART.Text))
                {
                    new Msgwin(false, "پارامترها كافي نيست").ShowDialog();
                    return;
                }
                string _DT1_ = DT1.Text.ToRawTarikh();
                string _DT2_ = DT2.Text.ToRawTarikh();
                if (!string.IsNullOrEmpty(_DT1_) || !string.IsNullOrEmpty(_DT2_))
                {
                    if (!Tarikh.IsValidedDate(_DT1_) || !Tarikh.IsValidedDate(_DT2_))
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

                // Handle different cases for OpenArgs
                switch (OpenArgs)
                {
                    case "F": // گزارش خلاصه فروش روزانه Case "F": GOZARESH_FROOSH_USER3

                        if (SSHIFT == "%")
                        {
                            //OpenReport("GOZARESH_FROOSH_USER3");

                            OpenReport_3();
                        }
                        else
                        {
                            //OpenReport("GOZARESH_FROOSH_USER");

                            OpenReport();

                        }
                        break;

                    case "FR":
                        //OpenReport("LIST_FROOSH_ANBARS_DTL");

                        OpenReport_2();

                        break;

                    case "FK": //گزارش فروش روزانه کاربران //Case "FK": GOZARESH_FROOSH_USER3
                        if (DTL.IsChecked == true)
                        {
                            if (USERR.SelectedValue == null)
                            {
                                new Msgwin(false, "نام کاربر وارد نشده").ShowDialog();
                                return;
                            }
                            //OpenReport("R_FROOSH_DAYLY2");
                        }
                        else
                        {
                            //OpenReport("GOZARESH_FROOSH_USER3");

                            OpenReport_3();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
            }
        }

        private void DT1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT1.SelectAll();
        }
        private void DT2_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DT2.SelectAll();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (BTN_GO.IsFocused)
                    {
                        BTN_GO_Click(null, null);
                        return;
                    }
                    else
                    {
                        e.Handled = true;

                        try
                        {
                            CL_LMethods.SendKey_US(Key.Tab);
                        }
                        catch { /*ignore*/ }
                    }
                }
            }
            catch { }
        }
    }
}