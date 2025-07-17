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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

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

            if (OpenArgs == "FK")
            {
                SHIFT.Visibility = Visibility.Hidden;
                USERR.Visibility = Visibility.Hidden;
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            DEPART.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>($"SELECT DEPATMAN, DEPNAME FROM dbo.DEPART").ToList();
            SHIFT.ItemsSource = dbms.DoGetDataSQL<SHIFT>($"SELECT SHIFT_ID, SHNAME FROM dbo.SHIFT").ToList();
            USERR.ItemsSource = dbms.DoGetDataSQL<SHIFT>($"SELECT USER_NAME FROM dbo.HEAD_LST GROUP BY USER_NAME ORDER BY USER_NAME").ToList();
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
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
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
                    case "F":
                        if (SSHIFT == "%")
                        {
                            //OpenReport("GOZARESH_FROOSH_USER3");
                        }
                        else
                        {
                            //OpenReport("GOZARESH_FROOSH_USER");
                        }
                        //OpenForm(this.Name, true);
                        break;

                    case "FR":
                        //OpenReport("LIST_FROOSH_ANBARS_DTL");
                        //OpenForm(this.Name, true);
                        break;

                    case "FK":
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
                            //OpenReport("GOZARESH_FROOSH_USER3");//Last Point Coding

                            
                            var report = new StiReport();
                            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_22.mrt");
                            report.Load(pathreport);

                            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                            report.Dictionary.Databases.Clear();
                            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                            //((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 300;


                            #region OpenReport_GroupFooter0_Format
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<int?>("SELECT COUNT(M_NAGHD) AS SumOfM_NAGHD FROM dbo.HEAD_LST WHERE     (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + " ) AND (TAG = 13)   AND  (DEPATMAN = " + DEPART.SelectedValue + " ) AND (M_NAGHD > 0)").FirstOrDefault();
                                if (rst != null && rst > 0)
                                {
                                    (report.GetComponentByName("CNAGHD") as StiText).Enabled = true; //  this.CNAGHD.Visible = true;
                                    (report.GetComponentByName("CNAGHD") as StiText).Text = "( " + rst + " )";
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<int?>("SELECT COUNT(TAKHFIF) AS MM FROM dbo.HEAD_LST WHERE     (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + " ) AND (TAG = 13)  AND  (DEPATMAN = " + DEPART.SelectedValue + " ) AND (TAKHFIF > 0)").FirstOrDefault();
                                if (rst != null && rst > 0)
                                {
                                    (report.GetComponentByName("MANDG") as StiText).Enabled = true; // this.CTAKH.Visible = true;
                                    (report.GetComponentByName("CTAKH") as StiText).Text = "( " + rst + " )";
                                    (report.GetComponentByName("Label137") as StiText).Enabled = true;
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<FMG1>("SELECT SUM(M_NAGHD) AS naghd, COUNT(M_NAGHD) AS countn FROM dbo.HEAD_LST WHERE     (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + ") AND (TAG = 4) AND   (DEPATMAN = " + DEPART.SelectedValue + ") AND (M_NAGHD > 0)").FirstOrDefault();

                                if (rst != null)
                                {
                                    //(report.GetComponentByName("MNAGHD") as StiText).Enabled = true; // this.MNAGHD.Visible = true;
                                    //(report.GetComponentByName("MNAGHD") as StiText).Text = "( " + rst + " )";
                                    //this.LMNAGHD.Visible = true;
                                    //(report.GetComponentByName("LMNAGHD") as StiText).Enabled = true;
                                    //(report.GetComponentByName("SANDOGH") as StiText).Enabled = true;
                                    //(report.GetComponentByName("LSANDOGH") as StiText).Enabled = true;
                                    //var SumOfM_NAGHD = Convert.ToDouble((report.GetComponentByName("Text18") as StiText).Text);
                                    //this.SANDOGH.CAPTION = (SumOfM_NAGHD - rst);
                                    //(report.GetComponentByName("MNAGHD") as StiText).Text = ;
                                    //(report.GetComponentByName("SANDOGH") as StiText).Text = ;
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<int?>("SELECT     COUNT(MABL_VAR) AS MM FROM dbo.HEAD_LST WHERE     (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + " ) AND (TAG = 13)  AND  (DEPATMAN = " + DEPART.SelectedValue + " ) AND (MABL_VAR > 0)").FirstOrDefault();
                                if (rst != null && rst > 0)
                                {
                                    //(report.GetComponentByName("MANDG") as StiText).Enabled = true; // this.CVAR.Visible = true;
                                    //this.CVAR.CAPTION = "( " + rst + " )";
                                    //(report.GetComponentByName("_YOUR_") as StiText).Text = ;
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<int?>("SELECT COUNT(MABL_HAV) AS MM FROM dbo.HEAD_LST WHERE (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + " ) AND (TAG = 13) AND  (DEPATMAN = " + DEPART.SelectedValue + " ) AND (MABL_HAV > 0)").FirstOrDefault();
                                if (rst != null && rst > 0)
                                {
                                    //(report.GetComponentByName("MANDG") as StiText).Enabled = true; //this.CHAV.Visible = true;
                                    //this.CHAV.CAPTION = "( " + rst + " )";
                                    //(report.GetComponentByName("_YOUR_") as StiText).Text = ;
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<int?>("SELECT COUNT(dbo.PAY_GETD.MABL) AS SumOfMABL FROM dbo.HEAD_LST INNER JOIN   dbo.PAY_GETD ON dbo.HEAD_LST.TAG = dbo.PAY_GETD.TAG AND dbo.HEAD_LST.NUMBER = dbo.PAY_GETD.NUMBER AND  dbo.HEAD_LST.TAG = dbo.PAY_GETD.TAG AND dbo.HEAD_LST.NUMBER = dbo.PAY_GETD.NUMBER AND dbo.HEAD_LST.TAG = dbo.PAY_GETD.TAG AND   dbo.HEAD_LST.NUMBER = dbo.PAY_GETD.NUMBER AND dbo.HEAD_LST.TAG = dbo.PAY_GETD.TAG AND  dbo.HEAD_LST.NUMBER = dbo.PAY_GETD.NUMBER WHERE     (DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " AND " + DT2.Text.ToRawTarikh() + " ) AND  (DEPATMAN = " + DEPART.SelectedValue + " ) AND   (dbo.HEAD_LST.TAG = 2)").FirstOrDefault();
                                if (rst != null && rst > 0)
                                {
                                    //(report.GetComponentByName("MANDG") as StiText).Enabled = true; //this.CASN.Visible = true;
                                    //this.CASN.CAPTION = "( " + rst + " )";
                                    //(report.GetComponentByName("_YOUR_") as StiText).Text = ;
                                }
                            }
                            if (true)
                            {
                                var rst = dbms.DoGetDataSQL<FMG2>("SELECT TOP 100 PERCENT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) AS MAND, COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) As TEDAD FROM         dbo.GUSER_FACT_PERDEPART_SUB3(" + DT1.Text.ToRawTarikh() + ", " + DT2.Text.ToRawTarikh() + ", " + DEPART.SelectedValue + ", '%', '%') GUSER_FACT_PERDEPART_SUB3 WHERE     (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD > 0)").FirstOrDefault();
                                if (rst != null)
                                {
                                    //this.BED.CAPTION = Strings.Format(rst.Fields("mand"), "#,###");
                                    //this.CBED.CAPTION = "( " + rst.Fields("tedad") + " )";
                                    //this.CBED.Visible = true;
                                    //(report.GetComponentByName("MANDG") as StiText).Enabled = true;
                                    //(report.GetComponentByName("_YOUR_") as StiText).Text = ;
                                }
                            }
                            if (true)
                            {
                                //var rst = dbms.DoGetDataSQL<FMG2>("SELECT TOP 100 PERCENT SUM(SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) AS MAND, COUNT(smab + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD) As TEDAD FROM         dbo.GUSER_FACT_PERDEPART_SUB3(" + DT1.Text.ToRawTarikh() + ", " + DT2.Text.ToRawTarikh() + ", " + DEPART.SelectedValue + ", '%', '%') GUSER_FACT_PERDEPART_SUB3 WHERE     (SMAB + MBAA - TAKHFIF - MABL_VAR - MABL_HAV - CHK + HAZ - M_NAGHD < 0)").FirstOrDefault();
                                //if (rst != null)
                                //{
                                //    this.BES.CAPTION = Strings.Format(rst.Fields("mand") * -1, "#,###");
                                //    this.cbes.CAPTION = "( " + rst.Fields("tedad") + " )";
                                //    this.cbes.Visible = true;
                                //    (report.GetComponentByName("MANDG") as StiText).Enabled = true;
                                //    (report.GetComponentByName("_YOUR_") as StiText).Text = ;
                                //}
                            }
                            #endregion

                            //report.Render();
                            //report.Show();

                            new Rpts.WINRPT(report, "گزارش فروش روزانه واحد ها").Show();
                        }
                        //OpenForm(this.Name, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
            }
        }
    }
}