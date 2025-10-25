using MaterialDesignThemes.Wpf;
using System;
using Prg_UI.UiTools;
using Prg_SendInvoice.CNNMANAGER;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Prg_UI.Functions;
using System.Windows.Interop;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Prg_Proccessy.MODELS;
using DocumentFormat.OpenXml.Bibliography;
using Prg_Proccessy.Generaly;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_KOL_DATE.xaml
    /// </summary>
    public partial class F_MENU_KOL_DATE : Window
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

        public string OpenArgs { get; private set; } = "default";

        public F_MENU_KOL_DATE(string _OpenArg_)
        {
            InitializeComponent();

            OpenArgs = _OpenArg_;
        }

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

        UniversControl universControl = new UniversControl();

        public Visual I_AM_KOL_DATE { get; set; }

        public string DTT { get; set; }

        public class Q1
        {
            public int? NUMBER { get; set; }
        }

        public class Q2
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_KOL_DATE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            HKOL.Focus();
        }

        public void Fill_ComboBoxes()
        {
            HKOL.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NUMBER;").ToList();
            HKOL.DisplayMemberPath = "NUMBER";
            HKOL.SelectedValuePath = "NUMBER";

            HHKOL.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HHKOL.DisplayMemberPath = "NAME";
            HHKOL.SelectedValuePath = "NUMBER";

            HKOL2.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NUMBER;").ToList();
            HKOL2.DisplayMemberPath = "NUMBER";
            HKOL2.SelectedValuePath = "NUMBER";

            HHKOL2.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HHKOL2.DisplayMemberPath = "NAME";
            HHKOL2.SelectedValuePath = "NUMBER";
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

        private void HKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HHKOL.SelectedValue = HKOL.SelectedValue;
        }

        private void HHKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HKOL.SelectedValue = HHKOL.SelectedValue;
        }

        private void HKOL2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HKOL2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HHKOL2.SelectedValue = HKOL2.SelectedValue;
        }

        private void HHKOL2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHKOL2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HKOL2.SelectedValue = HHKOL2.SelectedValue;
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string PATH;
            int i;
            if (IsNull(this.DT1.Text.ToRawTarikh()))
            {
                DT1.Text = Convert.ToString(Baseknow.YEA + "0101");
            }
            if (this.OpenArgs == "2")
            {
                if (IsNull(this.HKOL.SelectedValue) || IsNull(this.DT2.Text.ToRawTarikh()))
                {
                    universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                if (IsNull(this.HKOL.SelectedValue) || IsNull(this.HKOL2.SelectedValue) || IsNull(this.DT2.Text.ToRawTarikh()))
                {
                    universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            else
            {
                if (IsNull(this.DT2.Text.ToRawTarikh()))
                {
                    universControl.PopNotifyShow("تا تاریخ را وارد کنید", Pop1, Pop1Text1, Pop_Border1);
                }

                if (IsNull(this.HKOL.SelectedValue) || IsNull(this.HKOL2.SelectedValue))
                {
                    universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            switch (this.OpenArgs)
            {
                case "2":
                    {
                        dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = 'chart')   DROP VIEW  chart");
                        dbms.DoExecuteSQL("CREATE VIEW  chart as  SELECT TOP 100 PERCENT dbo.Umonth(DEED_HED.DATE_S) AS mm, ABS(SUM(DEED_DTL.BED - DEED_DTL.BES)) AS sumofbed FROM DEED_DTL INNER JOIN DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S GROUP BY DEED_DTL.HES_K, dbo.Umonth(DEED_HED.DATE_S) HAVING (DEED_DTL.HES_K = " + this.HKOL.SelectedValue + ") ORDER BY dbo.Umonth(dbo.DEED_HED.DATE_S)");


                        //DoCmd.OpenReport("CHRT_HES_KOL", acViewPreview);

                        break;
                    }

                case "default":
                    {
                        //DoCmd.OpenReport("R_DAFTAR_KOL", acViewPreview);
                        OpenReport();
                        break;
                    }
            }
            this.Close();
        }

        private void OpenReport()
        {

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DAFTAR_KOL.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report["KOL_PARM"] = HKOL.SelectedValue.ToString();
            report["KOL_2_PARM"] = HKOL2.SelectedValue.ToString();

            var salN = report.GetComponentByName("SAL_N") as StiText;
            if (salN != null)
            {
                salN.Text = Baseknow.WIDTH_D.ToString();
            }
            //report["DATE_S"] = DT2.Text.ToString();
            //report["DATE_F"] = DT2.Text.ToString();
            //report["ANBAR_F"] = ANBAR.Text.ToString();

            //report["AZDATE"] = Baseknow.YEA + "0101";
            //report["ANBAR"] = ANBAR.SelectedValue.ToString();

            //string TaTarikh = "99999999";
            //if (!string.IsNullOrEmpty(DT2.Text.ToRawTarikh().ToStringNullSafe())) { TaTarikh = DT2.Text.ToRawTarikh(); }
            //report["TADATE"] = TaTarikh;

            //report["KALACODE"] = KALA.SelectedValue.ToString();
            //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 900;

            //Report_Open:
            //(report.GetComponentByName("Table1_Cell17") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEG
            //(report.GetComponentByName("Table1_Cell14") as StiTableCell).TextFormat = new Stimulsoft.Report.Components.TextFormats.StiNumberFormatService(2, ".", (int)Baseknow.DIG, ",", 3, true, false, ""); //MEGK

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "گزارش دفتر کل").Show();
        }
    }
}
