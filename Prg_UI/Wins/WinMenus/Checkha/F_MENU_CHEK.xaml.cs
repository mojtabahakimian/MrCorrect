using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Prg_UI.HelperWins;
using System;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_Proccessy.Generaly;
using static Functions.InventoryManager;
using Stimulsoft.Base;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using Stimulsoft.Report.Components;
using Prg_UI.Wins.WinMenus.Checkha;

namespace Wins.WinMenus.Checkha
{
    public partial class F_MENU_CHEK : Window
    {
        #region Header Window Begin
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
        #endregion
        public F_MENU_CHEK(object openargs, string _Title_)
        {
            InitializeComponent();

            this.DataContext = this;

            OpenArgs = openargs;

            HEADER_TITLE.Content = _Title_;
        }


        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //DETAIL_VOSUL_SUB.IsReadOnly = !ican;
            }
        }

        public static bool IsNull(object p)
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

        public bool NowIsReady { get; private set; }
        public object OpenArgs { get; set; }

        public string _sql_query { get; set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (!Command5.IsFocused)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            DT1.Text = null;
            DT2.Text = Tarikh.FullCurrentDate;

            DT1.Focus(); DT1.SelectAll();
        }

        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DT1.Text.ToRawTarikh()))
            {
                DT1.Text = $"{Baseknow.YEA}0101"; // Assuming BASEKNOW.YEA provides the year
            }

            if (string.IsNullOrEmpty(HHMOIN.Text))
            {
                HHMOIN.Text = null;
            }

            // Validate inputs
            if (string.IsNullOrEmpty(DT1.Text.ToRawTarikh()) || string.IsNullOrEmpty(DT2.Text.ToRawTarikh()))
            {
                new Msgwin(false, "پارامترها كافي نيست!").Show();
                return;
            }


            // Handle actions based on OpenArgs
            switch (OpenArgs.ToStringNullSafe().ToLower())
            {
                //case "pchs":
                //    OpenForm("CHEK_PLISTS", 1, 800601);
                //    break;
                //case "dchs":
                //    OpenForm("CHEK_DLISTS", 1, 800501);
                //    break;
                case "pchss": //چک های پرداختی سررسید شده
                    _sql_query = $" SELECT * FROM dbo.CHEK_PLIST WHERE (DATE_S >= {DT1.Text.ToRawTarikh()} AND DATE_S <= {DT2.Text.ToRawTarikh()}  AND (NAME_TAH LIKE '%%%' or NAME_TAH  is null)) ";
                    new CHEK_PLISTS(_sql_query).Show();
                    break;

                case "dchss":
                    _sql_query = $"SELECT * FROM (SELECT PAY_GETD.N_SERI, PAY_GETD.BANK, PAY_GETD.DATE_S, PAY_GETD.DATE, PAY_GETD.SHOBEH, PAY_GETD.MABL, PAY_GETD.NAME_TAH,  PAY_GETD.N_HESAB, PAY_GETD.N_S, TCOD_BANKS.NAMES, PAY_GETD.RADIF, PAY_GETD.N_KOL, PAY_GETD.N_MOIN, PAY_GETD.N_KOL2, PAY_GETD.N_MOIN2, PAY_GETD.N_KOL3, PAY_GETD.N_MOIN3, PAY_GETD.N_TAF, PAY_GETD.N_TAF2, PAY_GETD.N_TAF3, TDETA_HES.NAME FROM TCOD_BANKS INNER JOIN  PAY_GETD ON TCOD_BANKS.CODE = PAY_GETD.BANK LEFT OUTER JOIN TDETA_HES ON PAY_GETD.N_KOL = TDETA_HES.N_KOL AND PAY_GETD.N_MOIN = TDETA_HES.NUMBER AND   PAY_GETD.N_TAF = TDETA_HES.TNUMBER WHERE     (PAY_GETD.N_KOL = 112 OR  PAY_GETD.N_KOL IS NULL) AND (PAY_GETD.N_KOL2 IS NULL) AND (PAY_GETD.N_KOL3 IS NULL) AND  DATE_S >= {DT1.Text.ToRawTarikh()} AND DATE_S <= {DT2.Text.ToRawTarikh()}  AND (NAME_TAH LIKE '%{(HHMOIN.Text is null ? "%" : HHMOIN.Text)}%' or NAME_TAH  is null)) AS DRVD_TBL WHERE (DATE_S >= {DT1.Text.ToRawTarikh()} AND DATE_S <= {DT2.Text.ToRawTarikh()}  AND (NAME_TAH LIKE '%%%' or NAME_TAH  is null)) ";
                    new CHEK_DLISTS(_sql_query).Show();
                    break;

                case "rchekd":

                    var report = new StiReport();
                    var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Checkha.CHEK_DLISTS.mrt");
                    report.Load(pathreport);
                    string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                    report.Dictionary.Databases.Clear();
                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
                    //((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 300;

                    report["KOL_PARAM"] = Baseknow.BANKHA;
                    report["DT1"] = DT1.Text.ToRawTarikh();
                    report["DT2"] = DT2.Text.ToRawTarikh();

                    (report.GetComponentByName("DT1TEXT") as StiText).Text = DT1.Text;
                    (report.GetComponentByName("DT2TEXT") as StiText).Text = DT2.Text;

                    //report.Render();
                    //report.ShowWithWpf();
                    //pathreport?.Dispose();

                    new Rpts.WINRPT(report, HEADER_TITLE.Content.ToStringNullSafe()).Show();
                    break;
                //case "rchekp":
                //    OpenReport("CHEK_PLIST");
                //    break;
                case "chkm":
                    _sql_query = $"SELECT * FROM dbo.CHEK_MOGUD WHERE (DATE >= {DT1.Text.ToRawTarikh()} AND DATE <= {DT2.Text.ToRawTarikh()})";
                    new CHEK_MOGUD(_sql_query).Show();
                    break;
                case "chkb": new CHEK_BARGASHTI_MAIN(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show(); break;

                case "chkv":
                    new CHEK_VOSUL_LES(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show();
                    break;

                //لیست چکهای نزد بانک و صندوق
                case "chkva":
                    //OpenForm("CHEK_VLISTALL", 1, 800503);
                    new CHEK_VLISTALL(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh(), HHMOIN.Text).Show();
                    break;
                //case "chkp":
                //    OpenForm("CHK_V_PRINT", 1, 8016);
                //    break;
                case "cvp":
                    //Calendar = System.Globalization.Calendar.CurrentEra; // Assuming your intent
                    //OpenForm("CHECK_PVLIST", 0, null);
                    new CHECK_PVLIST(DT1.Text.ToRawTarikh(), DT2.Text.ToRawTarikh()).Show();
                    break;

                default: break;
            }

            this.Close();
        }
    }
}
