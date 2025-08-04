using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Interop;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using static Prg_UI.Functions.CL_LMethods;
using System.Diagnostics;

namespace Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for F_MENU_ASNAD_PRINT.xaml
    /// </summary>
    public partial class F_MENU_ASNAD_PRINT : Window
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
        public F_MENU_ASNAD_PRINT()
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

        UniversControl universControl = new UniversControl();

        public Visual I_AM_ASNAD_PRINT { get; set; }

        public string DTT { get; set; }

        public class Q1
        {
            public double? N_S { get; set; }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_GO.IsFocused)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_ASNAD_PRINT = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            Fill_ComboBoxes();

            SNDNUM1.Focus();
        }

        public void Fill_ComboBoxes()
        {
            SNDNUM1.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT DEED_HED.N_S FROM DEED_HED GROUP BY DEED_HED.N_S ORDER BY DEED_HED.N_S;").ToList();
            SNDNUM1.DisplayMemberPath = "N_S";
            SNDNUM1.SelectedValuePath = "N_S";

            SNDNUM2.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT DEED_HED.N_S FROM DEED_HED GROUP BY DEED_HED.N_S ORDER BY DEED_HED.N_S;").ToList();
            SNDNUM2.DisplayMemberPath = "N_S";
            SNDNUM2.SelectedValuePath = "N_S";
        }

        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            int AZ_START = 0;
            int TA_END = 929292929;

            if (SNDNUM1.SelectedValue != null)
            {
                AZ_START = (int)SNDNUM1.SelectedValue;
            }
            if (SNDNUM2.SelectedValue != null)
            {
                TA_END = (int)SNDNUM2.SelectedValue;
            }

            Process Prc = ProcLoader.Start();

            dbms.DoExecuteSQL("DELETE FROM dbo.DEAD_DTL_PRINT");
            dbms.DoExecuteSQL("INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + "' AS Expr1 FROM dbo.DEAD_WITH_GRP WHERE     (N_S BETWEEN " + AZ_START + " AND " + TA_END + ")");
            dbms.DoExecuteSQL("INSERT INTO dbo.DEAD_DTL_PRINT (DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,UNAME) SELECT     DATE_S, SHARH_S, N_S, HES, TNUMBER, NUMBER, HNAME, N_KOL, BED, BES, SHARH, kk, BASE, RADIF, GR, TNAME, MNAME, KNAME,'" + CL_HESABDARI.UCurrentUser() + "' AS Expr1 FROM dbo.DEAD_WITH_GRP1 WHERE    (N_S BETWEEN " + AZ_START + " AND " + TA_END + ")");

            #region Report_Opening

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_SANAD_PRINT_GRP.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D.ToString();


            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "چاپ گروهی اسناد").Show();
            #endregion

            ProcLoader.Stop(Prc);
        }
    }
}
