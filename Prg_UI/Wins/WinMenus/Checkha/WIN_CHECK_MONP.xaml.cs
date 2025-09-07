using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Prg_Proccessy.SQLMODELS;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;
using System.Text;
using Syncfusion.Data;
using Prg_UI.HelperWins;

namespace Wins.WinMenus.Checkha
{
    public partial class WIN_CHECK_MONP : Window
    {
        public WIN_CHECK_MONP()
        {
            InitializeComponent();

            this.DataContext = this;
        }

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
        public ObservableCollection<TOHESAB> TOHESAB_DATA { get; set; } = new ObservableCollection<TOHESAB>();

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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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

        public bool NowIsReady { get; private set; }

        private double _sum_of_mabl = 0;
        public double SUM_OF_MABL
        {
            get
            {
                _sum_of_mabl = (double)TOHESAB_DATA.Sum(r => r.MABL);
                if (_sum_of_mabl == 0) _sum_of_mabl = 0;
                return _sum_of_mabl;
            }
            set { _sum_of_mabl = value; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}
            this.Dispatcher.Invoke(() =>
            {
                YEAR.Text = Tarikh.FullCurrentDate.Substring(0, 4);
            });

            ReGetData();

        }
        
        int LastButtonValue = 01;
        private void ReGetData(bool CalledFromButtons = false)
        {
            int GRP = 1; //وضعیت چک ها
            string _YEAR_ = Tarikh.FullCurrentDate.Substring(0, 4); //YEAR.Text : Text35.Text
            int _MONTH_ = LastButtonValue; //Month Buttons : Frame13 UniformGrid //دکمه ماه ها
            this.Dispatcher.Invoke(() =>
            {
                _YEAR_ = YEAR.Text;

                GRP = Convert.ToInt32(((RadioButton)GRO.Children.Cast<UIElement>().FirstOrDefault(r => r is RadioButton rb && rb.IsChecked == true))?.Tag);

                if (CalledFromButtons)
                {
                    _MONTH_ = Convert.ToInt32(((Button)MONTH_BUTTONS_UG.Children.Cast<UIElement>().FirstOrDefault(r => r is Button BT && BT.IsFocused))?.Tag);
                    LastButtonValue = _MONTH_;
                }
            });

            string RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND
                                                          (
                                                              dbo.PAY_GETP.N_KOL <> 911
                                                              OR dbo.PAY_GETP.N_KOL IS NULL
                                                          );";

            switch (GRP)
            {
                case 1:
                    {
                        // this["PAY_GETP_mon"].Form.RecordSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) AS YE, SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) AS MO FROM PAY_GETP WHERE     (LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) = " + this.Text35 + ") AND (SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) = " + this.Frame13 + " AND ( N_KOL <> 911 OR N_KOL IS NULL ))";
                        // break;

                        RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND
                                                          (
                                                              dbo.PAY_GETP.N_KOL <> 911
                                                              OR dbo.PAY_GETP.N_KOL IS NULL
                                                          );";
                        break;
                    }
                case 2:
                    {
                        // this["PAY_GETP_mon"].Form.RecordSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) AS YE, SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) AS MO FROM PAY_GETP WHERE     (LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) = " + this.Text35 + ") AND (SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) = " + this.Frame13 + ") AND (N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (N_KOL IS NULL OR N_KOL = " + Forms["BASEKNOW"]["BANKHA"] + ")";
                        // break;

                        RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND (N_KOL3 IS NULL) AND (N_KOL2 IS NULL)
                                                          AND (N_KOL IS NULL OR N_KOL = {Baseknow.BANKHA})";
                        break;
                    }
                case 3:
                    {
                        // this["PAY_GETP_mon"].Form.RecordSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) AS YE, SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) AS MO FROM PAY_GETP WHERE     (LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) = " + this.Text35 + ") AND (SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) = " + this.Frame13 + ") AND (NOT (N_KOL3 IS NULL))";
                        // break;

                        RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND (NOT (N_KOL3 IS NULL))";
                        break;
                    }
                case 4:
                    {
                        // this["PAY_GETP_mon"].Form.RecordSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) AS YE, SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) AS MO FROM PAY_GETP WHERE     (LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) = " + this.Text35 + ") AND (SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) = " + this.Frame13 + ") AND (N_KOL3 IS NULL) AND (NOT (N_KOL IS NULL) AND N_KOL <> " + Forms["BASEKNOW"]["BANKHA"] + ")AND N_KOL <> 911";
                        // break;

                        RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND (N_KOL3 IS NULL) 
                                                          AND (NOT (N_KOL IS NULL) 
                                                          AND N_KOL <> {Baseknow.BANKHA}) 
                                                          AND N_KOL <> 911";
                        break;
                    }
                case 5:
                    {
                        // this["PAY_GETP_mon"].Form.RecordSource = "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) AS YE, SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) AS MO FROM PAY_GETP WHERE     (LEFT(CAST(DATE_S AS NVARCHAR(8)), 4) = " + this.Text35 + ") AND (SUBSTRING(CAST(DATE_S AS NVARCHAR(8)), 5, 2) = " + this.Frame13 + ") AND (NOT (N_KOL2 IS NULL) )AND N_KOL <> 911";
                        // break;

                        RecordSource = @$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK,
                                                           dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                           dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE,
                                                           dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL,
                                                           dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB,
                                                           dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL,
                                                           dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF,
                                                           dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2,
                                                           dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3,
                                                           dbo.PAY_GETP.N_MOIN3, dbo.PAY_GETP.N_TAF3,
                                                           dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG,
                                                           dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF,
                                                           dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ,
                                                           LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) AS YE,
                                                           SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) AS MO
                                                    FROM dbo.PAY_GETP
                                                        LEFT OUTER JOIN dbo.TCOD_BANKS
                                                            ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                    WHERE (LEFT(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 4) = {_YEAR_})
                                                          AND (SUBSTRING(CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR(8)), 5, 2) = {_MONTH_})
                                                          AND(NOT(N_KOL2 IS NULL))AND N_KOL<> 911";
                        break;
                    }

                default: break;
            }

            MONTH_LABEL.Content = CL_LMethods.GetPersianMonthName(_MONTH_);

            TOHESAB_DATA?.Clear();
            var RST = dbms.DoGetDataSQL<TOHESAB>(RecordSource).ToList();
            foreach (var item in RST)
            {
                TOHESAB_DATA.Add(item);
            }

            GenerateAutomaticSummary(TOHESAB_SUB);
            //// Refresh the grid's view to update the summary
            TOHESAB_SUB.View.Refresh();

            ROWCOUNT_LABEL.Content = "تعداد رکورد ها: " + TOHESAB_DATA.Count;
            MABL_JAM.Text = SUM_OF_MABL.ToStringNullSafe();
        }

        private void HAMEH_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void VOSULED_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void NON_VOSUL_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void TRANSFER_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void BARGASHT_Click(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        #region _SfDataGrid_
        private readonly FilterService<TOHESAB> filterService = new FilterService<TOHESAB>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void TOHESAB_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void TOHESAB_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //// Get the selected row and column index
            var currentCell = TOHESAB_SUB.SelectionController.CurrentCellManager.CurrentCell;
            if (currentCell != null)
            {
                var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
                UpdateCurrentCellValue(rowColumnIndex);
            }

        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.TOHESAB_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.TOHESAB_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.TOHESAB_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.TOHESAB_SUB.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell

            if (!string.IsNullOrEmpty(selectedText))
            {
                // Add the Contains filter to the filter service (inclusion filter)
                filterService.AddFilter(columnName, selectedText, isExclusion: false); // False means it's an inclusion filter
                ActiveFilters.Add($"{columnName} Contains {selectedText}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
            else
            {
                if (filterValue != null)
                {
                    // Add the filter to the filter service
                    filterService.AddFilter(columnName, filterValue);
                    // Add the filter to the list of active filters
                    ActiveFilters.Add($"{columnName} = {filterValue}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
            }

        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
                if (filterValue != null)
                {
                    // Add the Not Contains filter to the filter service (exclusion filter)
                    filterService.AddFilter(columnName, selectedText, isExclusion: true); // True means it's an exclusion filter
                                                                                          // Add the exclusion filter to the list of active filters
                    ActiveFilters.Add($"{columnName} Does Not Contain {selectedText}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
            }
            else
            {
                var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
                if (filterValue != null)
                {
                    // Add the exclusion filter to the filter service
                    filterService.AddFilter(columnName, filterValue, isExclusion: true);
                    // Add the filter to the list of active filters
                    ActiveFilters.Add($"{columnName} != {filterValue}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
            }
        }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails()
        {
            // Check if there is a current cell selected in the data grid
            if (TOHESAB_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = TOHESAB_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                        // Return the column name and the current cell value
                                                                                                                        //if (CurrentCellValue == null)
                                                                                                                        //{
                                                                                                                        //    return (columnName, SelectedSfDgTextCell);
                                                                                                                        //}
                                                                                                                        //else
                {
                    return (columnName, CurrentCellValue);
                }
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter()
        {
            // Set the filter for the data grid view using the filter service
            TOHESAB_SUB.View.Filter = item => filterService.ApplyFilter(item as TOHESAB);
            // Refresh the filter to update the view
            TOHESAB_SUB.View.RefreshFilter();
        }
        private void TOHESAB_SUB_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }

        }
        private string GetSelectedText()
        {
            var dataGrid = TOHESAB_SUB;
            var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;

            if (currentCell != null && currentCell.IsEditing)
            {
                // Find the editing element (which will be a TextBox in edit mode)
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    return editingElement.SelectedText; // Return the selected text
                }
            }
            return string.Empty;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShow("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }
            }
            catch { return; }


            // Check if there are selected rows
            if (TOHESAB_SUB.SelectedItems == null || !TOHESAB_SUB.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in TOHESAB_SUB.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in TOHESAB_SUB.SelectedItems)
                {
                    foreach (var column in TOHESAB_SUB.Columns)
                    {
                        if (!column.IsHidden) // Include only columns that are not hidden
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{TOHESAB_SUB.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void TOHESAB_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(TOHESAB_SUB);

                e.Handled = true; // Mark event as handled
            }
        }
        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            // Ensure rows are selected
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0)
            {
                return;
            }

            // Detect the current column
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null)
            {
                return;
            }

            string columnName = currentColumn.MappingName; // Get the column name
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            decimal sum = 0;
            bool isNumericColumn = false;

            // Iterate through the selected rows
            foreach (var selectedItem in _DG_.SelectedItems)
            {
                // Get the cell value for the detected column
                var cellValue = GetCellValue(selectedItem, columnName);

                if (cellValue != null && decimal.TryParse(cellValue.ToStringNullSafe(), out decimal numericValue))
                {
                    sum += numericValue;
                    isNumericColumn = true;
                }
            }

            if (isNumericColumn)
            {
                string formattedSum = sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                new Msgwin(false, $"جمع سطر های انتخاب شده در ستون [{currentColumn.HeaderText}] برار است با : {formattedSum}").ShowDialog();

            }
        }
        private object GetCellValue(object record, string columnName)
        {
            try
            {
                // Use reflection to get the property value from the record
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch
            {
                return null;
            }
        }
        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                TOHESAB_SUB.TableSummaryRows.Clear();
            }
            else
            {
                // Check if a summary row already exists
                if (_DG_.TableSummaryRows.Count > 0)
                {
                    return; // Exit the method if a summary row already exists
                }
            }

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            // Loop through each text column in the grid to create summary columns for numeric properties
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(TOHESAB).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                // Check if the property is numeric and create a summary column if it is
                if (column.MappingName == "MABL" && IsNumericType(propertyInfo.PropertyType))
                {
                    var summaryColumn = new GridSummaryColumn
                    {
                        Name = column.MappingName + "Sum",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                        Format = "{Sum:N0}" // Formatting the summary as numeric
                    };
                    summaryColumns.Add(summaryColumn);
                }
            }

            summaryRow.SummaryColumns = summaryColumns;

            // Add the constructed summary row to the grid's summary rows collection
            _DG_.TableSummaryRows.Add(summaryRow);
        }

        // Helper method to determine if a property type is numeric
        private bool IsNumericType(Type type)
        {
            if (type == null)
                return false;

            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type that might represent a number
            if (type == typeof(object))
            {
                return true; // Assume it might be numeric
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(TOHESAB_SUB, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion

        private void YEAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(YEAR.Text) || string.IsNullOrWhiteSpace(YEAR.Text) || YEAR.Text.Length > 4)
            {
                e.Handled = true;
            }
            else
            {
                ReGetData();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReGetData(true);
        }

    }
}
