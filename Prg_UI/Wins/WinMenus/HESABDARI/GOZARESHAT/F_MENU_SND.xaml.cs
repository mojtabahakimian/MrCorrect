using MaterialDesignThemes.Wpf;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
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
using static Stimulsoft.Base.StiDbType;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_SND.xaml
    /// </summary>
    public partial class F_MENU_SND : Window
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
        public F_MENU_SND()
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
        public string _sql_query { get; set; }
        public string _sql_query_2 { get; set; }

        public class Q1
        {
            public double? N_S { get; set; }
        }

        public class Q2
        {
            public double? N_S { get; set; }
            public long? DATE_S { get; set; }
            public string? SHARH_S { get; set; }
        }

        UniversControl universControl = new UniversControl();

        public Visual I_AM_MENU_SND { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_MENU_SND = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();
        }

        public void Fill_ComboBoxes()
        {
            DT1.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT N_S, DATE_S, SHARH_S FROM DEED_HED ORDER BY N_S DESC").ToList();
            //DT1.SelectedValuePath = "N_S";
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

        private void OpenReport2()
        {
            _sql_query = $"SELECT NUMBER, NAM, CODE, NAMES, daraee, GRP FROM TARAZNAMAH({DT1.SelectedValue}) TARAZNAMAH WHERE (GRP = 1) AND (daraee <> 0)";
            _sql_query_2 = $"SELECT NUMBER, NAM, CODE, NAMES, BEDEHE, GRP FROM TARAZNAMAH({DT1.SelectedValue}) TARAZNAMAH WHERE (GRP = 2) AND (BEDEHE <> 0)";

            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.TARAZNAMAH2.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM1", _sql_query);
            report.Dictionary.Variables.Add("Q_PARM2", _sql_query_2);

            string dt1 = $"کلیه اسناد تا سند : {DT1.Text}";

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();
            (report.GetComponentByName("DT1_N") as StiText).Text = dt1.ToString();

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "ترازنامه").Show();
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            if (DT1.SelectedValue == null)
            {
                DT1.SelectedValue = 9999999;
            }

            OpenReport2();
        }

        private void DT1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (this.DT1.Text.ToRawTarikh().Length >= 6)
            {
                DT1.Text = Strings.Replace(this.DT1.Text, "/", "");
                var rst = dbms.DoGetDataSQL<Q1>("SELECT TOP 1 PERCENT N_S FROM dbo.DEED_HED WHERE (DATE_S <= " + this.DT1.Text.ToRawTarikh() + ") ORDER BY N_S DESC").ToList();
                if (rst.Count > 0)
                {
                    this.DT1.Text = rst.FirstOrDefault().ToString();
                }
            }
        }
    }
}
