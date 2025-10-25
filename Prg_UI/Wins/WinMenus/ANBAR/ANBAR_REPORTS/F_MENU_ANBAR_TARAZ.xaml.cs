using AUTO_BAZ.HelperWins;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
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
using static Functions.InventoryManager;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Functions;
using Rpts;

namespace Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR_TARAZ.xaml
    /// </summary>
    public partial class F_MENU_ANBAR_TARAZ : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

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
        public F_MENU_ANBAR_TARAZ(string _OpenArg_)
        {
            OpenArgs = _OpenArg_;
            InitializeComponent();
        }
        public string OpenArgs { get; private set; } = "R";

        public FULL_HESAB HESAB_FROM_SEARCH { get; set; }
        public Visual I_AM_MENU_ANBAR { get; set; }
        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();
        public class Q1
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
        }

        public class Q2
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public class Q3
        {
            public int? CODE { get; set; }
        }

        public string MANBAR { get; set; }
        public string DTT { get; set; }
        public string MKALA { get; set; }
        public string sql_data = null;
        public string sql_data_last = null;

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

        private void CANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            ANBAR.SelectedValue = CANBAR.SelectedValue;
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            CANBAR.SelectedValue = ANBAR.SelectedValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_MENU_ANBAR = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DT2.Text = Tarikh.FullCurrentDate;

            Fill_ComboBoxes();

            CANBAR.Focus();
        }

        public void Fill_ComboBoxes()
        {
            ANBAR.ItemsSource = dbms.DoGetDataSQL<Q2>($"SELECT CODE, NAMES FROM TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = {Baseknow.USERCOD})))").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            CANBAR.ItemsSource = dbms.DoGetDataSQL<Q3>($"SELECT CODE FROM TCOD_ANBAR WHERE     (CODE <> 0) AND (CODE IN (SELECT  ANBCO  FROM dbo.OPANBACCESS  WHERE     (USERCO = {Baseknow.USERCOD})))").ToList();
            CANBAR.SelectedValuePath = "CODE";
            CANBAR.DisplayMemberPath = "CODE";
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string PATH;
            int i;
            if (string.IsNullOrEmpty(this.DT2.Text.ToRawTarikh()))
            {
                this.DT2.Text = Convert.ToString(Baseknow.YEA + "0101");
            }
            if (this.OpenArgs == "FD" || this.OpenArgs == "TANBGRP")
            {
                if (IsNull(this.ANBAR.SelectedValue))
                {
                    MANBAR = "%";
                }
            }
            if (this.DT2.Text.ToRawTarikh().Length < 8)
            {
                Msgwin msgwin = new Msgwin(false, "تاریخ صحیح نیست!");
                msgwin.ShowDialog();
                return;
            }
            if (IsNull(this.ANBAR.SelectedValue) && this.OpenArgs != "TANBGRP")
            {
                Msgwin msgwin = new Msgwin(false, "پارامترها کافی نیست!");
                msgwin.ShowDialog();
                return;
            }
            if (ANBAR.SelectedValue is not null && !string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
            {
                sql_data = @$"SELECT 
                                    [MABKH] / CASE WHEN [MEGHKH] = 0 THEN 1 ELSE [MEGHKH] END AS FII_AFZAYESH,
                                    [MABFR] / CASE WHEN [MEGFR] = 0 THEN 1 ELSE [MEGFR] END AS FII_KAHESH,
                                    FLOOR([MABKH] + [SumOfMABL_A] - [MABFR]) / 
                                    CASE 
                                        WHEN ([MEG] + [MEGHKH] - [MEGFR]) = 0 THEN 1 
                                        ELSE ([MEG] + [MEGHKH] - [MEGFR]) 
                                    END AS FII_MOGUDI,
                                    [SumOfMABL_A] / CASE WHEN [MEG] = 0 THEN 1 ELSE [MEG] END AS FII_FIRST,
                                    [MEG] + [MEGHKH] - [MEGFR] AS MOGUDI_MEGH,
                                    FLOOR([MABKH] + [SumOfMABL_A] - [MABFR]) AS MOGUDI_MABL ,*
                                FROM TARAZ_ANBAR_KHAS({DT2.Text.ToRawTarikh()},{ANBAR.SelectedValue});
                                ";        
                
                sql_data_last = @$"SELECT 
                                        [MABVARED] / CASE WHEN [MEGHVARED] = 0 THEN 1 ELSE [MEGHVARED] END AS FII_AFZAYESH,
                                        [MABSADER] / CASE WHEN [MEGHSADER] = 0 THEN 1 ELSE [MEGHSADER] END AS FII_KAHESH,
                                        FLOOR([MABLM]) / CASE WHEN [MOG] = 0 THEN 1 ELSE [MOG] END AS FII_MOGUDI,
                                        [MABAVM] / CASE WHEN [MEGHAVM] = 0 THEN 1 ELSE [MEGHAVM] END AS FIRST_FII, *
                                    FROM C_TARAZ_ANBAR_KHAS({DT2.Text.ToRawTarikh()},{ANBAR.SelectedValue})
                                    ";
            }
            else
            {
                MANBAR = this.ANBAR.SelectedValue.ToString();
            }
            switch (this.OpenArgs)
            {
                case "R":
                    {
                        OpenReport();
                        break;
                    }
                case "F":
                    {
                        new TARAZ_ANBAR_KHAS(sql_data).ShowDialog();
                        break;
                    }
                case "FD":
                    {
                        CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.C_TARAZ_ANBAR_KHAS, null , sql_data_last);
                        break;
                    }
                case "TANBGRP":
                    {
                        OpenReport2();
                        break;
                    }

            }
        }

        private void OpenReport()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_TARAZ_ANBARHA_KHAS.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["DATE_PARM"] = DT2.Text.ToRawTarikh().ToString();
            report["ANBAR_PARM"] = ANBAR.SelectedValue.ToString();

            (report.GetComponentByName("DT1") as StiText).Text = DT2.Text.ToString();

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

            new WINRPT(report, "گزارش موجودی انبار").Show();
        }
        private void OpenReport2()
        {
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_TARAZ_ANBARHA_KHAS_GRP.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["ANBAR_PARM"] = ANBAR.SelectedValue.ToString();

            (report.GetComponentByName("DT1") as StiText).Text = DT2.Text.ToString();

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

            new WINRPT(report, "گزارش موجودی انبار").Show();
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
    }
}
