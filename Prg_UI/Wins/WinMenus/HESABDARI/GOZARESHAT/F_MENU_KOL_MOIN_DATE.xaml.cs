using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
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
using MaterialDesignThemes.Wpf;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_KOL_MOIN_DATE.xaml
    /// </summary>
    public partial class F_MENU_KOL_MOIN_DATE : Window
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

        public F_MENU_KOL_MOIN_DATE()
        {
            InitializeComponent();
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

        bool First_Open = true;

        UniversControl universControl = new UniversControl();

        public Visual I_AM_KOL_MOIN_DATE { get; set; }

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

        public class Q3
        {
            public int? NUMBER { get; set; }
            public int? N_KOL { get; set; }
        }

        public class Q4
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
            public int? N_KOL { get; set; }
        }

        public int BPERSON { get; set; }

        public void Fill_ComboBoxes()
        {
            HKOL.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT TOTA_HES.NUMBER FROM TOTA_HES ORDER BY TOTA_HES.NUMBER;").ToList();
            HKOL.DisplayMemberPath = "NUMBER";
            HKOL.SelectedValuePath = "NUMBER";

            HHKOL.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT TOTA_HES.NUMBER, TOTA_HES.NAME FROM TOTA_HES ORDER BY TOTA_HES.NAME;").ToList();
            HHKOL.DisplayMemberPath = "NAME";
            HHKOL.SelectedValuePath = "NUMBER";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_KOL_MOIN_DATE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            HKOL.Focus();
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

            if (First_Open == true)
            {
                First_Open = false;
                return;
            }

            if (HKOL.SelectedValue is null)
            {
                return;
            }

            HHKOL.SelectedValue = HKOL.SelectedValue;

            HMOIN.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT DETA_HES.NUMBER, DETA_HES.N_KOL FROM DETA_HES WHERE (((DETA_HES.N_KOL) = {HKOL.SelectedValue} Or (DETA_HES.N_KOL) = {HKOL.SelectedValue} AND ((DETA_HES.CODE_E) Like {BPERSON} Or (DETA_HES.CODE_E) Is Null)));").ToList();
            HMOIN.DisplayMemberPath = "NUMBER";
            HMOIN.SelectedValuePath = "NUMBER";

            HHMOIN.ItemsSource = dbms.DoGetDataSQL<Q4>($"SELECT DETA_HES.NUMBER, DETA_HES.NAME, DETA_HES.N_KOL FROM DETA_HES WHERE DETA_HES.N_KOL = {HKOL.SelectedValue}").ToList();
            HHMOIN.DisplayMemberPath = "NAME";
            HHMOIN.SelectedValuePath = "NUMBER";
        }

        private void HHKOL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHKOL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HKOL.SelectedValue = HHKOL.SelectedValue;


            HMOIN.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT DETA_HES.NUMBER, DETA_HES.N_KOL FROM DETA_HES WHERE (((DETA_HES.N_KOL) = {HKOL.SelectedValue} Or (DETA_HES.N_KOL) = {HKOL.SelectedValue} AND ((DETA_HES.CODE_E) Like {BPERSON} Or (DETA_HES.CODE_E) Is Null)));").ToList();
            HMOIN.DisplayMemberPath = "NUMBER";
            HMOIN.SelectedValuePath = "NUMBER";

            HHMOIN.ItemsSource = dbms.DoGetDataSQL<Q4>($"SELECT DETA_HES.NUMBER, DETA_HES.NAME, DETA_HES.N_KOL FROM DETA_HES WHERE DETA_HES.N_KOL = {HKOL.SelectedValue}").ToList();
            HHMOIN.DisplayMemberPath = "NAME";
            HHMOIN.SelectedValuePath = "NUMBER";
        }

        private void HMOIN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HMOIN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HHMOIN.SelectedValue = HMOIN.SelectedValue;
        }

        private void HHMOIN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHMOIN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HMOIN.SelectedValue = HHMOIN.SelectedValue;
        }

        public void BFACTOR()
        {
            if (Baseknow.PERSON == 0)
            {
                BPERSON = 0;
            }
            else
            {
                BPERSON = Convert.ToInt32(Baseknow.PERSON);
            }
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
            if (IsNull(this.HKOL.SelectedValue) || IsNull(this.HMOIN.SelectedValue) || IsNull(this.DT1.Text.ToRawTarikh()) || IsNull(this.DT2.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("پارامتر ها کافی نیست!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            //DoCmd.OpenReport("R_DAFTAR_MOIN", acViewPreview);
            OpenReport();
            
            this.Close();
        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DAFTAR_MOIN.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["FDATE_PARM"] = DT1.Text.ToRawTarikh().ToString();
            report["EDATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report["HKOL_PARM"] = HKOL.SelectedValue.ToString();
            report["MOIN_PARM"] = HMOIN.SelectedValue.ToString();

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();

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
            new Rpts.WINRPT(report, "گزارش دفتر معین").Show();
        }
    }
}
