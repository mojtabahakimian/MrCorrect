using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.UiTools;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System.Reflection;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.FUNCTIONS;
using Rpts;
using Prg_UI.Functions;
using Prg_Proccessy.Generaly;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for F_MENU_AJNAS.xaml
    /// </summary>
    public partial class F_MENU_AJNAS : Window
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

        public F_MENU_AJNAS()
        {
            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        public string OpenArgs { get; set; } = "VISITDLV";
        public string _sql_query { get; set; }
        public string Condition { get; private set; } = "";
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

        public Visual I_AM_F_MENU_AJNAS { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CL_HESABDARI.LETSGO("DEPEMAL"))
            {
                if (Condition == "")
                {
                    Condition = " AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
                else
                {
                    Condition = Condition + " AND  (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }

            }
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
            if (M0.IsChecked == true)
            {
                _sql_query = $"SELECT NAM, MABL_F, MANDAH, Expr1 FROM (SELECT STUF_DEF.NAME + ' - ' + STUF_DEF.CODE AS NAM, STUF_DEF.MABL_F, 1 AS Expr1, STUF_DEF.B_SEF, MOGUDI_KOL_ANBARHA.MANDAH FROM STUF_DEF INNER JOIN MOGUDI_KOL_ANBARHA ON STUF_DEF.CODE = MOGUDI_KOL_ANBARHA.CODE) AS DRVD_TBL";
            }
            else
            {
                _sql_query = $"SELECT NAM, MABL_F, MANDAH, Expr1 FROM (SELECT STUF_DEF.NAME + ' - ' + STUF_DEF.CODE AS NAM, STUF_DEF.MABL_F, 1 AS Expr1, STUF_DEF.B_SEF, MOGUDI_KOL_ANBARHA.MANDAH, HEAD_LST.DEPATMAN FROM STUF_DEF INNER JOIN MOGUDI_KOL_ANBARHA ON STUF_DEF.CODE = MOGUDI_KOL_ANBARHA.CODE INNER JOIN INVO_LST ON STUF_DEF.CODE = INVO_LST.CODE INNER JOIN HEAD_LST ON INVO_LST.NUMBER = HEAD_LST.NUMBER AND INVO_LST.TAG = HEAD_LST.TAG) AS DRVD_TBL WHERE MANDAH > 0 {Condition}";

            }

            OpenReport();
        }

        private void OpenReport()
        {
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.STUF_DEF.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            (report.GetComponentByName("DT1_N") as StiText).Text = Tarikh.FullCurrentDate;
            (report.GetComponentByName("MTN") as StiText).Text = MATN.Text;

            (report.GetComponentByName("TNAME_N") as StiText).Text = Baseknow.WIDTH_D.ToString();

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

            new WINRPT(report, "لیست کالا ها جهت ویزیت").Show();
        }

    }
}
