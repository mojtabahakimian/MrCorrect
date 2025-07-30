using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.UiTools;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System.Diagnostics;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for F_MENU_ANBAR_FRKH_bar.xaml
    /// </summary>
    public partial class F_MENU_ANBAR_FRKH_bar : Window
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

        public F_MENU_ANBAR_FRKH_bar()
        {
            InitializeComponent();
        }

        public string _sql_query { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
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

        public class Q1
        {
            public string? DRIVER { get; set; }
        }

        UniversControl universControl = new UniversControl();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DT1.Text = Tarikh.FullCurrentDate.ToString();
            DT2.Text = Tarikh.FullCurrentDate.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Fill_ComboBoxes();
        }

        public void Fill_ComboBoxes()
        {
            driver.ItemsSource = dbms.DoGetDataSQL<Q1>($"SELECT DRIVER FROM OTHER_DTL GROUP BY DRIVER ORDER BY DRIVER").ToList();
            driver.SelectedValuePath = "DRIVER";
            driver.DisplayMemberPath = "DRIVER";
        }

        private void OpenReport()
        {

            _sql_query = $"SELECT DATE_N, CAMIUN, DRIVER, KALA, SMEGH, SMEGHK, vahed, smabk FROM dbo.DRIVER_HAVALAH('{DT1.Text.ToRawTarikh()}', '{DT2.Text.ToRawTarikh()}', N'{(driver.SelectedValue is null ? "NULL" : driver.SelectedValue)}');";

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.LIST_FROOSH_ANBARS_BAR.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            (report.GetComponentByName("DATE_N") as StiText).Text = Tarikh.FullCurrentDate;
            //(report.GetComponentByName("MTN") as StiText).Text = MATN.Text;

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

            new WINRPT(report, "حواله گروهی جهت بارگیری").Show();
        }

        private void Commnd5_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DT1.Text.ToRawTarikh()) || string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
            {
                Msgwin msgwin = new Msgwin(false, "پارامتر ها کافی نیست");
                msgwin.ShowDialog();
            }
            else
            {
                OpenReport();
            }
        }
    }
}
