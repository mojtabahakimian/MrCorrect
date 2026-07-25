using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.Checkha;
using Prg_UI.Wins.WinOther;
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.HESABDARI;
using Wins.WinOther;
using static Functions.DataGridClipboardManager;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;
using static Prg_UI.Wins.WinMenus.Checkha.GETCHEK;
using ComboBox = System.Windows.Controls.ComboBox;
using DataGrid = System.Windows.Controls.DataGrid;
using TextBox = System.Windows.Controls.TextBox;

//using Convert = System.Convert;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// ObservableCollection بهینه‌شده که AddRange را با یک اعلان واحد UI پشتیبانی می‌کند.
    /// جایگزین مستقیم ObservableCollection استاندارد در تمام DataGrid های پروژه.
    /// </summary>
    public sealed class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// تمام آیتم‌ها را یکجا اضافه کرده و فقط یک بار UI را آپدیت می‌کند
        /// </summary>
        public void AddRange(IEnumerable<T> list)
        {
            if (list == null) return;

            _suppressNotification = true;
            foreach (var item in list)
                Add(item);
            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Clear + AddRange در یک تراکنش UI واحد
        /// </summary>
        public void ReplaceAll(IEnumerable<T> list)
        {
            _suppressNotification = true;
            Clear();
            if (list != null)
                foreach (var item in list)
                    Add(item);
            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
        }
    }
    public partial class PGET_HED : Window, INotifyPropertyChanged, ISearchableWindow
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }

        private double _sum_of_mabl;
        public double SUM_OF_MABL
        {
            get
            {
                _sum_of_mabl = Convert.ToDouble(KHAZANEH_DATA.Sum(row => row.MABL ?? 0));
                return _sum_of_mabl;
            }
            set { _sum_of_mabl = value; OnPropertyChanged("SUM_OF_MABL"); }
        }
        //public ObservableCollection<PGET_LST> KHAZANEH_DATA { get; set; } = new ObservableCollection<PGET_LST>();

        public RangeObservableCollection<PGET_LST> KHAZANEH_DATA { get; } = new RangeObservableCollection<PGET_LST>();
        private DEED_HED _currentDeedData = null;

        // Session-level cache: hes code → account NAME.
        // CUST_HESAB lookups are expensive (full scan of TDETA_HES ~36 k rows due to
        // non-sargable CONVERT key). Caching eliminates the OUTER APPLY cost after the
        // first navigation that encounters a given hes value.
        private static readonly Dictionary<string, string?> _hesNameCache = new();
        private sealed class HesNameRow { public string? hes { get; set; } public string? NAME { get; set; } }

        // Full query with OUTER APPLY — used by ReGetData() (after-save path, not hot-path).
        private static readonly string PGET_LST_SQL = @"
    SELECT
        p.ID, p.DATE, p.RADIF, p.NO_AM, p.NAHVA, p.FHES_K, p.FHES_M, p.FHES_T,
        p.THES_K, p.THES_M, p.THES_T, p.SHARH, p.MABL, p.N_SERI, p.BANK,
        p.MHAZ_NO, p.IDH, p.FHES, p.THES, p.ARZD, p.FHES_T2, p.THES_T2,
        p.FHES_T3, p.THES_T3, p.FHES_T4, p.THES_T4, p.CRT, p.UID,
        CAST(CASE WHEN tk.num IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasAttachment,
        cf.NAME AS NAME_FHES,
        ct.NAME AS NAME_THES
    FROM dbo.PGET_LST AS p WITH (NOLOCK)
    LEFT JOIN (SELECT DISTINCT num FROM dbo.TASKS WITH (NOLOCK) WHERE tg = 34) AS tk ON tk.num = p.IDH
    OUTER APPLY (SELECT TOP 1 NAME FROM dbo.CUST_HESAB WITH (NOLOCK) WHERE hes = p.FHES) AS cf
    OUTER APPLY (SELECT TOP 1 NAME FROM dbo.CUST_HESAB WITH (NOLOCK) WHERE hes = p.THES) AS ct
    WHERE p.ID = @ID ORDER BY p.IDH
    OPTION (OPTIMIZE FOR (@ID UNKNOWN));";

        // Navigation query — no OUTER APPLY. Names come from _hesNameCache instead.
        private static readonly string PGET_LST_SQL_BASE = @"
    SELECT
        p.ID, p.DATE, p.RADIF, p.NO_AM, p.NAHVA, p.FHES_K, p.FHES_M, p.FHES_T,
        p.THES_K, p.THES_M, p.THES_T, p.SHARH, p.MABL, p.N_SERI, p.BANK,
        p.MHAZ_NO, p.IDH, p.FHES, p.THES, p.ARZD, p.FHES_T2, p.THES_T2,
        p.FHES_T3, p.THES_T3, p.FHES_T4, p.THES_T4, p.CRT, p.UID,
        CAST(CASE WHEN tk.num IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS HasAttachment
    FROM dbo.PGET_LST AS p WITH (NOLOCK)
    LEFT JOIN (SELECT DISTINCT num FROM dbo.TASKS WITH (NOLOCK) WHERE tg = 34) AS tk ON tk.num = p.IDH
    WHERE p.ID = @ID ORDER BY p.IDH;";

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                if (string.IsNullOrEmpty(ID.Text) || Convert.ToInt32(ID.Text) == 0)
                {
                    _newrecord = true;
                }
                else
                {
                    _newrecord = false;
                }
                return _newrecord;
            }
            set { _newrecord = value; }
        }
        public bool ChangeIsHappend { get; set; } = false;

        private int _CurrentPosition = 0;
        public int CurrentPosition
        {
            get
            {

                return _CurrentPosition;
            }
            set
            {
                if (_CurrentPosition == value) return;
                _CurrentPosition = value;
                OnPropertyChanged(nameof(CurrentPosition));
            }
        }

        List<COMBOPERSONEL> rst_personel = null;
        public class _QR1
        {
            public double? NAGHD { get; set; }
            public int? FHES_K { get; set; }
            public int? FHES_M { get; set; }
            public int? FHES_T { get; set; }
            public string? FHES { get; set; }
            public string? NAME { get; set; }
            public string? MOLAH { get; set; }
        }
        public class _QR2
        {
            public double? CHK { get; set; }
            public int? FHES_K { get; set; }
            public int? FHES_M { get; set; }
            public int? FHES_T { get; set; }
            public int? TEDAD { get; set; }
            public string? NAME { get; set; }
            public string? MOLAH { get; set; }
        }

        private bool PLUS;

        bool PERSONEL_First_Open = true;
        public bool PAYCHEK_EXIT_BTN { get; set; }
        public bool GETCHEK_EXIT_BTN { get; set; }
        public bool FORCHEK_EXIT_BTN { get; set; }
        public bool BAKCHEKP_EXIT_BTN { get; set; }
        public bool BAKCHEK_EXIT_BTN { get; set; }
        public bool IsExitChkButtonPressed { get; set; }

        public Visual I_AM_KHAZANEH { get; set; }
        public double Meidnum { get; set; }

        //private bool _newrecord;
        //public bool NewRecord
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(ID.Text) || Convert.ToInt32(ID.Text) == 0)
        //        {
        //            _newrecord = true;
        //        }
        //        else
        //        {
        //            _newrecord = false;
        //        }
        //        return _newrecord;
        //    }
        //}

        public class Search_Model
        {
            public string HES { get; set; }
            public string NAME { get; set; }
        }
        public Search_Model FROM_SEARCH { get; set; } = new Search_Model();
        public string FOCUSED_COLUMN_NAME { get; set; }
        public bool LETSANAD { get; private set; }
        public bool CANCEL { get; private set; }
        public string DTDT { get; private set; }
        public bool DTCHK { get; private set; }
        public string BEFOREDATEN { get; private set; }
        public bool NowIsReady { get; private set; }

        public PGET_LST? WAS_ROW_ITEM { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }
        public int CURRENT_COLUMN_INDEX { get; private set; }
        public DataGridCell CURRENT_CELL_ROW { get; private set; }
        public object ENTERED_VALUE_ROW { get; private set; }
        public PGET_LST? CURRENT_ITMES_ROW { get; private set; }


        class KIND_COMBO
        {
            public int KIND_ID { get; set; }
            public string KIND_NAME { get; set; }
        }
        public class SGN_IMODEL
        {
            public string USER_SEMAT { get; set; }
            public string USER_HESAB_NAME { get; set; }
        }
        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (sgn1usid.Tag is not null)
                {
                    _sgn1_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn1usid.Tag), "SGN0137TX");
                    _sgn1_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn1usid.Tag)));
                }
                return _sgn1_info;
            }
        }
        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO
        {
            get
            {
                if (sgn2usid.Tag is not null)
                {
                    _sgn2_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn2usid.Tag), "SGN0237TX");
                    _sgn2_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn2usid.Tag)));
                }
                return _sgn2_info;
            }
        }
        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO
        {
            get
            {
                if (sgn3usid.Tag is not null)
                {
                    _sgn3_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn3usid.Tag), "SGN0337TX");
                    _sgn3_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        public List<PGET_LST> HESABHA_LIST { get; set; } = new List<PGET_LST>();

        private bool can;
        public bool AllowEdits
        {
            get { return can; }
            set
            {
                can = value;
                DATE.IsReadOnly = !can;
                MOLAH.IsReadOnly = !can;

                MOLAH.IsEnabled = can;
                DATE.IsEnabled = can;
                DEPATMAN.IsEnabled = can;
                KIND.IsEnabled = can;
                SHIFT.IsEnabled = can;
                SAVEBTN.IsEnabled = can;

                if (Convert.ToInt32(ID.Text) > 0)
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 34, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                }
                else
                {
                    this.SGN1.IsEnabled = false;
                    this.SGN2.IsEnabled = false;
                    this.SGN3.IsEnabled = false;
                }
            }
        }

        public double? NUMBER_TO_OPEN { get; set; } = null;

        private int datagridname_tbox_def_index_col;
        public int PGET_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (PGET_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NO_AM")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        datagridname_tbox_def_index_col = 0;
                    }
                    else
                    {
                        datagridname_tbox_def_index_col = (int)defaultcolumnindex;
                    }
                }
                return datagridname_tbox_def_index_col;
            }
        }

        public PGET_HED(double? nUMBER_TO_OPEN = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;
            if (nUMBER_TO_OPEN != null && nUMBER_TO_OPEN > 0)
            {
                NUMBER_TO_OPEN = Convert.ToDouble(nUMBER_TO_OPEN);
                IsOpenedFromAutomation = _isAutomasion_;
            }
            this.Owner = PublicVRB.WINBASE;//#OWNER
        }
        public bool IsOpenedFromAutomation { get; } = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_KHAZANEH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            USER_NAME.Text = Baseknow.UUSER;

            #region Form_Load
            //Form_Load
            // Me.lock2.Password = "neginsedighahmad"
            // Me.lock2.Connected = True
            // If Me.lock2.ErrorCode <> 0 And Me.lock2.ErrorCode <> 7 Then
            // DoCmd.OpenForm "lockok"
            // End If
            if (Strings.Mid(Baseknow.OPTIONSS, 42, 1) == "5")
            {
                this.MANDB.Visibility = Visibility.Visible;
                this.MANDS.Visibility = Visibility.Visible;
            }
            LETSANAD = true;

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "PGETD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            #endregion

            //Form_Load();
            Form_Open();

            FillAllComboBoxes();

            ReGetMasterData();

            Form_Current();

            //DataGrid SUB EVETNS LOADINGS:
            #region SUB_LOADING
            //Check Matter
            if (Strings.Mid(Baseknow.OPTIONSS, 14, 1) == "5")
            {
                this.aRZDColumn.Visibility = Visibility.Visible;
                //  this.aRZDColumn.ColumnWidth = 600;
            }
            else
            {
                this.aRZDColumn.Visibility = Visibility.Hidden;
            }
            CL_HESABDARI.SETSECURITYSUB(PGET_LST_SUB, "PGET_HED");
            PLUS = false;
            #endregion

            #region SUB_ON_OPEN
            //if (Strings.Mid(Baseknow.OPTIONSS, 26, 1) == "5")
            //{
            //    this.PGET_LST_SUB.FontSize = Convert.ToInt32(Strings.Mid(Baseknow.OPTIONSS, 27, 2));
            //}
            //else
            //{
            //    this.PGET_LST_SUB.FontSize = 8;
            //    //this.DatasheetFontHeight = 8;
            //}
            #endregion

            #region SUB_LOAD
            if (Strings.Mid(Baseknow.OPTIONSS, 14, 1) == "5")
            {
                //this.ARZD.ColumnHidden = false;
                this.aRZDColumn.Visibility = Visibility.Hidden;
                this.aRZDColumn.Width = 600;
            }
            else
            {
                this.aRZDColumn.Width = 0;
            }
            CL_HESABDARI.SETSECURITYSUB(PGET_LST_SUB, "PGET_HED");
            PLUS = false;

            #endregion

            if (!string.IsNullOrEmpty(ID.Text) && Convert.ToDouble(ID.Text) > 0)
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            DATE.Focus();
        }
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                DataGrid DG = PGET_LST_SUB;

                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (IsDataGrid_IsFocused && DG != null)
                    {
                        if (DG?.CurrentColumn != null && DG.SelectedItem != null)
                        {
                            // 1. جستجو برای پیدا کردن پنجره پیام (چه فعال باشد چه نباشد)
                            var messageWindow = Application.Current.Windows.OfType<Window>()
                                .FirstOrDefault(w => w is Prg_UI.HelperWins.Msgwin || w is Prg_UI.HelperWins.MsgListwin);

                            if (messageWindow != null)
                            {
                                try
                                {
                                    // استفاده از Dispatcher با اولویت Input برای اطمینان از اعمال فوکوس
                                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                    {
                                        // 2. اگر پنجره مینیمایز شده است، آن را به حالت عادی برگردان
                                        if (messageWindow.WindowState == WindowState.Minimized)
                                        {
                                            messageWindow.WindowState = WindowState.Normal;
                                        }

                                        // 3. آوردن پنجره به جلوترین حالت
                                        messageWindow.Activate();
                                        var was = messageWindow.Topmost;
                                        messageWindow.Topmost = true;  // موقتا روترین پنجره شود
                                        messageWindow.Topmost = was; // به حالت عادی برگردد (اختیاری)

                                        // 4. فوکوس نهایی
                                        messageWindow.Focus();
                                    }));
                                }
                                catch { }

                                // اگر این کد در رویداد دکمه‌ای مثل Enter است، اینجا ریترن می‌کنیم
                                return;
                            }
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = DG.CurrentColumn?.SortMemberPath == "MABL";
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Make sure next row exists before trying to select it
                                    if (DG.Items.Count > DG.SelectedIndex + 1)
                                    {
                                        DG.SelectedIndex++;

                                        // Verify the new selection is valid
                                        if (DG.SelectedItem != null && DG.Columns.Count > PGET_LST_SUB_DEF_INDEX_COL)
                                        {
                                            DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[PGET_LST_SUB_DEF_INDEX_COL]);

                                            Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                if (DG.SelectedItem != null)
                                                {
                                                    DG.BeginEdit();
                                                }
                                            }), DispatcherPriority.Background);


                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    else if (SAVEBTN.IsFocused)
                    {
                        SAVEBTN.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        return;
                    }

                    CL_LMethods.SendKey_US(Key.Tab, true);
                }

                if (!PGET_LST_SUB.IsKeyboardFocusWithin && !PGET_LST_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
                {
                    if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        e.Handled = true;
                        var searchWindow = new EnhancedSearchWindow(this);
                        searchWindow.Owner = this;
                        searchWindow.ShowDialog();
                    }
                }
                else
                {
                    if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        DataGridExtension.HandleKeyPress(sender, e, PGET_LST_SUB);
                    }
                }


                // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
                var nonDataKeys = new[]
                {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
                if (!nonDataKeys.Contains(e.Key))
                {
                    var focused = Keyboard.FocusedElement as DependencyObject;
                    if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                    {
                        ChangeIsHappend = true;
                    }
                    else
                    {
                        var focusedElement = Keyboard.FocusedElement;
                        if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                        {
                            ChangeIsHappend = true;
                        }
                    }
                }
            }
            catch { }
        }

        //public string GetRestrictedSqlQueryForPGET_HED(string DEF_VALUE = " WHERE ")
        public string GetRestrictedSqlQueryForPGET_HED(string DEF_VALUE = "")
        {
            string WhereCondition = DEF_VALUE;

            string GetAnd() => WhereCondition.Trim().Length > 6 ? " AND " : " WHERE ";

            bool CanSeeAll = CL_HESABDARI.LETSGO("DPDEED"); // اجازه دیدن تمام اسناد دریافت/پرداخت
            bool OnlyUserDept = CL_HESABDARI.LETSGO("DEPEMAL");
            bool IsZirMajmoehChart = CL_HESABDARI.LETSGO("chartfilter");
            //bool IsDateLimited = !CL_HESABDARI.LETSGO("DECD");

            if (!CanSeeAll)
            {
                //if (IsDateLimited)
                //{
                //    var sqlQuery = $"SELECT TOP 100 PERCENT DATE FROM dbo.PGET_HED WHERE DEPATMAN = {CL_Generaly.VAHED_OF_USER} AND USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' GROUP BY DATE ORDER BY DATE DESC";
                //    var result = dbms.DoGetDataSQL<long>(sqlQuery).ToList();

                //    if (result.Count > 0 && Convert.ToDouble(Baseknow.CPI) > 0)
                //    {
                //        long? dateResult = null;
                //        int index = Convert.ToInt32(Baseknow.CPI);
                //        dateResult = index >= 0 && index < result.Count ? result[index] : result.FirstOrDefault();

                //        if (dateResult > 0)
                //            WhereCondition += $"{GetAnd()} USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' AND DATE >= {dateResult} ";
                //    }
                //}
                //else
                {
                    if (OnlyUserDept)
                    {
                        WhereCondition += $"{GetAnd()} DEPATMAN = {CL_Generaly.VAHED_OF_USER}";

                        if (IsZirMajmoehChart && Convert.ToBoolean(Baseknow.mrcorrect))
                        {
                            string vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                            if (!string.IsNullOrEmpty(vs?.Trim()))
                                WhereCondition += $" AND {vs}";
                        }
                    }
                    else if (IsZirMajmoehChart && Convert.ToBoolean(Baseknow.mrcorrect))
                    {
                        string vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                        if (!string.IsNullOrEmpty(vs?.Trim()))
                            WhereCondition += $"{GetAnd()} {vs}";
                    }
                    else
                    {
                        string currentUser = Baseknow.UUSER;
                        string persianUser = CL_LMethods.NormalizeArabicPersian(currentUser);
                        string arabicUser = CL_LMethods.ReplacePerArab(currentUser, true);
                        WhereCondition += $"{GetAnd()} (USER_NAME = N'{currentUser}' OR USER_NAME = N'{persianUser}' OR USER_NAME = N'{arabicUser}')";
                    }
                }
            }

            WhereCondition = WhereCondition.Trim();
            if (WhereCondition.EndsWith("=") || WhereCondition.EndsWith("AND") || WhereCondition.EndsWith("OR"))
                WhereCondition = string.Empty;

            return WhereCondition;
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is Prg_Proccessy.SQLMODELS.PGET_HED item)
            {
                if (item != null)
                {
                    var itemfound = RecordsData.View.Cast<Prg_Proccessy.SQLMODELS.PGET_HED>().FirstOrDefault(x => x.ID == item.ID);
                    if (itemfound != null)
                    {
                        // Set the CurrentItem to the found item
                        RecordsData.View.MoveCurrentTo(itemfound);

                        MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                    }
                }
                else
                {
                    // Update your window with the selected item
                    MoveReGetData(INavigator.Jahat.LastItem);
                }

            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
               new SearchableProperty { DisplayName = "شماره خزانه", PropertyPath = "ID", PropertyType = typeof(double) },
               new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE", PropertyType = typeof(long) },
               new SearchableProperty { DisplayName = "شماره برگه", PropertyPath = "IDK", PropertyType = typeof(double) },
               new SearchableProperty { DisplayName = "شماره سند", PropertyPath = "N_S", PropertyType = typeof(double) },
               new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
               new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
               // Add other searchable properties
            };
        }
        #endregion

        private void ReGetMasterData()
        {
            string whereClause = GetRestrictedSqlQueryForPGET_HED();

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                whereClause = $" WHERE ID = {NUMBER_TO_OPEN} ";
            }

            var query = $@"
                        SELECT ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, RPLICA,
                               SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, CRT, UID
                        FROM dbo.PGET_HED
                        {whereClause}
                        ORDER BY DATE, ID";

            var MasterHead = dbms.DoGetDataSQL<Prg_Proccessy.SQLMODELS.PGET_HED>(query).ToList();
            RecordsData.Source = MasterHead;

            //var MasterHead = dbms.DoGetDataSQL<Prg_Proccessy.SQLMODELS.PGET_HED>($"SELECT ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.PGET_HED  ORDER BY DATE, ID ").ToList(); // WHERE ID = {ID.Text}
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN > 0)
            {
                var item = RecordsData.View.Cast<Prg_Proccessy.SQLMODELS.PGET_HED>().FirstOrDefault(x => x.ID == NUMBER_TO_OPEN);
                if (item != null)
                {
                    RecordsData.View.MoveCurrentTo(item);
                    MoveReGetData(Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                }
            }
            else
            {
                MoveReGetData(Jahat.LastItem);
            }

        }
        public void ReGetData()
        {
            //Claude5
            // ──────────────────────────────────────────────────────────────────
            // FAST PATH: اعتبارسنجی ورودی قبل از هر عملیات دیگری
            // ──────────────────────────────────────────────────────────────────
            if (!int.TryParse(ID.Text?.Trim(), out int parsedId) || parsedId <= 0)
            {
                KHAZANEH_DATA.ReplaceAll(null);
                this.MABL.Text = "0";
                return;
            }

            // ──────────────────────────────────────────────────────────────────
            // ✅ BOTTLENECK #4 FIX: AsList() از Dapper — بدون کپی اضافی حافظه
            //    (ToList() یک List جدید می‌سازد؛ AsList() از بافر داخلی استفاده می‌کند)
            // ──────────────────────────────────────────────────────────────────
            var result = dbms.DoGetDataSQL<PGET_LST>(PGET_LST_SQL, new { ID = parsedId })
                             ?.AsList();

            // Warm the cache from these results so navigations that follow skip CUST_HESAB
            if (result != null)
                foreach (var r in result)
                {
                    if (r.FHES != null) _hesNameCache[r.FHES] = r.NAME_FHES;
                    if (r.THES != null) _hesNameCache[r.THES] = r.NAME_THES;
                }

            // ──────────────────────────────────────────────────────────────────
            // ✅ BOTTLENECK #5 FIX: ReplaceAll → یک CollectionChanged برای کل لیست
            //    (به جای N بار CollectionChanged در foreach معمولی)
            // ──────────────────────────────────────────────────────────────────
            KHAZANEH_DATA.ReplaceAll(result);

            this.MABL.Text = SUM_OF_MABL.ToString();
        }

        private bool _navigationBusy = false;
        private async void MoveReGetData(Jahat jahat, int? custom_postiion = null)
        {
            if (_navigationBusy) return;
            _navigationBusy = true;

            try
            {
                int RecordCount() { return ((System.Windows.Data.ListCollectionView)RecordsData.View)?.Count ?? 0; }

                void DisplayCounts()
                {
                    var RVC = RecordsData.View?.CurrentPosition;
                    if (RVC is not null && RecordsData.View?.CurrentItem is not null)
                    {
                        //Current Record
                        if (RecordsData.View.CurrentPosition + 1 <= RecordCount())
                        {
                            Current_Rec.Text = Convert.ToString(RVC + 1); // to display number of record in normal way to user, not displaying zero (1)
                        }
                        else
                        {
                            Current_Rec.Text = RVC.ToString();
                        }
                    }

                    RecCount.Text = (RecordCount()).ToString(); //Record Count
                }

                if ((ChangeIsHappend) && !ConfirmExitWithoutSaving())
                {
                    return;
                }

                switch (jahat)
                {
                    case Jahat.FirstItem: //اولین
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToFirst();
                        break;
                    case Jahat.BackItem: //قبلی
                        if (RecordsData.View.CurrentPosition > 0) //Possible To Back
                        {
                            if (NewRecord)
                            {
                                jahat = Jahat.LastItem;
                                RecordsData.View.MoveCurrentToLast();
                            }
                            else
                            {
                                RecordsData.View.MoveCurrentToPrevious();
                            }
                            NewRecord = false;
                        }
                        break;

                    case Jahat.NextItem: //بعدی
                        if (RecordsData.View.CurrentPosition < RecordCount() - 1)
                        {
                            NewRecord = false;
                            RecordsData.View.MoveCurrentToNext();
                        }
                        break;

                    case Jahat.LastItem: //آخرین
                        RecordsData.View.MoveCurrentToLast();
                        break;

                    case Jahat.CustomPosition:
                        if (custom_postiion > -1)
                        {
                            NewRecord = false;
                            RecordsData.View.MoveCurrentToPosition((int)custom_postiion);
                        }
                        break;

                    case Jahat.NewItem: //جدید خالی
                        NewRecord = true;
                        RecordsData.View.MoveCurrentToLast();
                        Clear_PGET_HED();
                        break;
                }

                // Fire all 3 queries in parallel — total latency = slowest query, not sum
                if (jahat != Jahat.NewItem && RecordsData.View.CurrentItem != null)
                {
                    var HEADER = RecordsData.View.CurrentItem as Prg_Proccessy.SQLMODELS.PGET_HED;
                    int currentId = HEADER.ID ?? 0;
                    double? currentNS = HEADER.N_S;

                    var taskHeader = dbms.DoGetDataSQLAsync<Prg_Proccessy.SQLMODELS.PGET_HED>(
                        "SELECT TOP 1 ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.PGET_HED WHERE ID = @ID",
                        new { ID = currentId });

                    var taskDeed = currentNS != null
                        ? dbms.DoGetDataSQLAsync<DEED_HED>("SELECT * FROM DBO.DEED_HED WITH (NOLOCK) WHERE N_S = @NS", new { NS = currentNS })
                        : Task.FromResult<IEnumerable<DEED_HED>>(Enumerable.Empty<DEED_HED>());

                    // Use base SQL (no OUTER APPLY) — names resolved from _hesNameCache below
                    var taskLst = dbms.DoGetDataSQLAsync<PGET_LST>(PGET_LST_SQL_BASE, new { ID = currentId });

                    await Task.WhenAll(taskHeader, taskDeed, taskLst);

                    // Apply PGET_HED header
                    var DBData = taskHeader.Result.FirstOrDefault();
                    if (HEADER != null && DBData != null)
                    {
                        HEADER.ID = DBData.ID;
                        HEADER.DATE = DBData.DATE;
                        HEADER.MOLAH = DBData.MOLAH;
                        HEADER.N_S = DBData.N_S;
                        HEADER.DEPATMAN = DBData.DEPATMAN;
                        HEADER.SHIFT = DBData.SHIFT;
                        HEADER.CUST_KIND = DBData.CUST_KIND;
                        HEADER.USER_NAME = DBData.USER_NAME;
                        HEADER.KIND = DBData.KIND;
                        HEADER.IDK = DBData.IDK;
                        HEADER.OKF = DBData.OKF;
                        HEADER.RPLICA = DBData.RPLICA;
                        HEADER.SGN1 = DBData.SGN1;
                        HEADER.SGN2 = DBData.SGN2;
                        HEADER.SGN3 = DBData.SGN3;
                        HEADER.sgn1usid = DBData.sgn1usid;
                        HEADER.sgn2usid = DBData.sgn2usid;
                        HEADER.sgn3usid = DBData.sgn3usid;
                        HEADER.CRT = DBData.CRT;
                        HEADER.UID = DBData.UID;
                        RecordsData.View.Refresh();
                    }

                    // Cache deed data for Form_Current and UiDataUpdate (no re-query needed)
                    _currentDeedData = taskDeed.Result.FirstOrDefault();

                    // Resolve account names via cache — batch-fetch any not yet cached
                    var lstRows = taskLst.Result?.AsList() ?? new List<PGET_LST>();
                    var uncachedHes = lstRows
                        .SelectMany(r => new[] { r.FHES, r.THES })
                        .Where(h => h != null && !_hesNameCache.ContainsKey(h!))
                        .Distinct()
                        .ToList();
                    if (uncachedHes.Count > 0)
                    {
                        var fetched = await dbms.DoGetDataSQLAsync<HesNameRow>(
                            "SELECT hes, MIN(NAME) AS NAME FROM dbo.CUST_HESAB WITH (NOLOCK) WHERE hes IN @hes GROUP BY hes",
                            new { hes = uncachedHes });
                        foreach (var r in fetched)
                            if (r.hes != null) _hesNameCache[r.hes] = r.NAME;
                        foreach (var h in uncachedHes.Where(h => !_hesNameCache.ContainsKey(h!)))
                            _hesNameCache[h!] = null;
                    }
                    foreach (var row in lstRows)
                    {
                        row.NAME_FHES = row.FHES != null && _hesNameCache.TryGetValue(row.FHES, out var fn) ? fn : null;
                        row.NAME_THES = row.THES != null && _hesNameCache.TryGetValue(row.THES, out var tn) ? tn : null;
                    }

                    KHAZANEH_DATA.ReplaceAll(lstRows);
                    this.MABL.Text = SUM_OF_MABL.ToString();
                }

                DisplayCounts();

                UiDataUpdate(jahat);

                if (jahat == Jahat.NewItem)
                {
                    Clear_PGET_HED();
                }
                else
                {
                    Form_Current();
                }

                ChangeIsHappend = false; // Reset it
            }
            finally
            {
                _navigationBusy = false;
            }
        }
        private void UiDataUpdate(Jahat jahat)
        {
            ApplyDataGridItems();

            if (RecordsData.View?.CurrentItem is not null && jahat != Jahat.NewItem) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as Prg_Proccessy.SQLMODELS.PGET_HED;

                DATE.Text = HEADER.DATE.ToString();
                ID.Text = HEADER.ID.ToString();
                IDK.Text = HEADER.IDK.ToString();
                USER_NAME.Text = HEADER.USER_NAME;
                MOLAH.Text = HEADER.MOLAH;

                KIND.SelectedValue = HEADER.KIND; KIND.Items.Refresh();
                DEPATMAN.SelectedValue = HEADER.DEPATMAN; DEPATMAN.Items.Refresh();
                SHIFT.SelectedValue = HEADER.SHIFT; SHIFT.Items.Refresh();

                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3);

                sgn1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                sgn2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                sgn3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);

                sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn1usid)?.SAL_NAME;
                sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                N_S.Text = HEADER.N_S.ToStringNullSafe();

                if (_currentDeedData != null)
                {
                    MABNA.Text = _currentDeedData.@base.ToString();
                }

                //OKF.IsChecked = false;
                if (HEADER.OKF is not null)
                {
                    OKF.IsChecked = HEADER.OKF;
                }
                PGET_LST_SUB.IsReadOnly = true;
            }
        }
        private bool ConfirmExitWithoutSaving()
        {
            Msgwin msgwin = new Msgwin(true, "آیتم جدید را ذخیره نکرده اید , آیا از خروج از این آیتم اطمینان دارید ؟");
            msgwin.ShowDialog();
            return msgwin.DialogResult == true;
        }
        public void RefreshAfterInsert()
        {
            var itemtoadd = dbms.DoGetDataSQL<Prg_Proccessy.SQLMODELS.PGET_HED>($"SELECT TOP 1 ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.PGET_HED WHERE ID = {ID.Text}").FirstOrDefault();

            var underlyingCollection = RecordsData.Source as List<Prg_Proccessy.SQLMODELS.PGET_HED>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);

            }
        }
        public void RefreshAfterUpdate()
        {
            var freshData = dbms.DoGetDataSQL<Prg_Proccessy.SQLMODELS.PGET_HED>($"SELECT TOP 1 ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.PGET_HED WHERE ID = {ID.Text}").FirstOrDefault();
            var underlyingCollection = RecordsData.Source as List<Prg_Proccessy.SQLMODELS.PGET_HED>;
            if (freshData != null && underlyingCollection != null)
            {
                var existing = underlyingCollection.FirstOrDefault(x => x.ID == freshData.ID);
                if (existing != null)
                {
                    existing.ID = freshData.ID;
                    existing.DATE = freshData.DATE;
                    existing.MOLAH = freshData.MOLAH;
                    existing.N_S = freshData.N_S;
                    existing.DEPATMAN = freshData.DEPATMAN;
                    existing.SHIFT = freshData.SHIFT;
                    existing.CUST_KIND = freshData.CUST_KIND;
                    existing.USER_NAME = freshData.USER_NAME;
                    existing.KIND = freshData.KIND;
                    existing.IDK = freshData.IDK;
                    existing.OKF = freshData.OKF;
                    existing.RPLICA = freshData.RPLICA;
                    existing.SGN1 = freshData.SGN1;
                    existing.SGN2 = freshData.SGN2;
                    existing.SGN3 = freshData.SGN3;
                    existing.sgn1usid = freshData.sgn1usid;
                    existing.sgn2usid = freshData.sgn2usid;
                    existing.sgn3usid = freshData.sgn3usid;
                    existing.CRT = freshData.CRT;
                    existing.UID = freshData.UID;
                    RecordsData.View.Refresh();
                }
            }
        }
        public void RefreshAfterDelete()
        {
            var LastCurrentPosition = RecordsData.View.CurrentPosition;

            if (RecordsData.View.CurrentItem != null)
            {
                var itemToRemove = RecordsData.View.CurrentItem as Prg_Proccessy.SQLMODELS.PGET_HED;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<Prg_Proccessy.SQLMODELS.PGET_HED>;
                    if (underlyingCollection != null)
                    {
                        underlyingCollection.Remove(itemToRemove);
                        RecordsData.View.Refresh(); // Refresh the view to reflect the removal
                    }
                }
            }

            //Move to next exiting item
            if (LastCurrentPosition - 1 > 0)
            {
                MoveReGetData(INavigator.Jahat.CustomPosition, LastCurrentPosition - 1);
                //MoveReGetData(INavigator.Jahat.BackItem);
            }
            else if (LastCurrentPosition + 1 <= ((System.Windows.Data.ListCollectionView)RecordsData.View).Count - 1)
            {
                //MoveReGetData(INavigator.Jahat.NextItem);
                MoveReGetData(INavigator.Jahat.CustomPosition, LastCurrentPosition + 1);
            }
            else
            {
                MoveReGetData(INavigator.Jahat.NewItem);
            }
        }

        private void FillAllComboBoxes()
        {
            //کبموباکس مجری پرسنل
            //rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL=0)").ToList();
            string sql = @"
                           SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD
                           FROM SALA_DTL sd
                           LEFT JOIN USER_PERSONEL_ORDER uo 
                                ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
                           WHERE sd.ENABL = 0
                           ORDER BY
                                CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                                uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();

            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);

            //کمبوباکس ارجاع
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";

            //کموبباکس نوع برگه 
            List<KIND_COMBO> kindComboList = new List<KIND_COMBO>
            {
                new KIND_COMBO { KIND_ID = 0, KIND_NAME = "عادی" },
                new KIND_COMBO { KIND_ID = 2, KIND_NAME = "سند دریافت" },
                new KIND_COMBO { KIND_ID = 3, KIND_NAME = "سند پرداخت" }
            };
            KIND.ItemsSource = kindComboList.ToList();
            KIND.DisplayMemberPath = "KIND_NAME";
            KIND.SelectedValuePath = "KIND_ID";
            KIND.SelectedIndex = 0;

            //واحد ها
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            DEPATMAN.ItemsSource = RST; DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectedValue = null;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;

            //شیفت
            SHIFT.ItemsSource = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID,SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            SHIFT.DisplayMemberPath = "SHNAME";
            SHIFT.SelectedValuePath = "SHIFT_ID";
            SHIFT.SelectedValue = null;
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER;

            //DataGrids ComboBoxes: //KHAZANEH_DATA
            //نوع عملیات
            nO_AMColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_DPS.CODE, TCOD_DPS.NAMES FROM TCOD_DPS ORDER BY TCOD_DPS.CODE, TCOD_DPS.NAMES").ToList();
            nO_AMColumn.DisplayMemberPath = "NAMES";
            nO_AMColumn.SelectedValuePath = "CODE";

            //نحوه
            nAHVAColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_DPSKIND>("SELECT TCOD_DPSKIND.CODE, TCOD_DPSKIND.NAMES FROM TCOD_DPSKIND ORDER BY TCOD_DPSKIND.CODE, TCOD_DPSKIND.NAMES").ToList();
            nAHVAColumn.DisplayMemberPath = "NAMES";
            nAHVAColumn.SelectedValuePath = "CODE";

            ////از حساب
            //FHES_COLUMN.ItemsSource = KHAZANEH_DATA.Select(item => new { item.NAME_FHES, item.FHES }).ToList();

            ////به حساب
            //tHESColumn.ItemsSource = KHAZANEH_DATA.Select(item => new { item.NAME_THES, item.THES }).ToList();

            //مرکز هزینه
            mHAZ_NOColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_MARKAZHAZ>("SELECT MHAZ_NO, MHAZNAME FROM TCOD_MARKAZHAZ").ToList();
            mHAZ_NOColumn.DisplayMemberPath = "MHAZNAME";
            mHAZ_NOColumn.SelectedValuePath = "MHAZ_NO";

        }

        private void BTN_ATTACH_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag is PGET_LST currentRow)) return;

            if (currentRow.IDH == null || currentRow.IDH <= 0)
            {
                new Msgwin(false, "ابتدا سطر را ذخیره کنید تا امکان ضمیمه تصویر فراهم شود.").ShowDialog();
                return;
            }

            double khazanehNumber = Convert.ToDouble(ID.Text);
            int? currentIDH = currentRow.IDH;

            if (currentRow.HasAttachment)
            {
                // View Attachment
                try
                {
                    var query = @"SELECT E.pic 
                                  FROM dbo.TASKS T
                                  INNER JOIN dbo.EVENTS E ON T.IDNUM = E.IDNUM
                                  WHERE T.num = @id AND T.tg = 34";

                    var imageData = dbms.DoGetDataSQL<byte[]>(query, new { id = currentRow.IDH }).FirstOrDefault();

                    if (imageData != null && imageData.Length > 0)
                    {
                        new ImagePreviewWindow(imageData).ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "تصویری برای نمایش یافت نشد.").ShowDialog();
                        currentRow.HasAttachment = false;
                    }
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در دریافت تصویر: " + ex.Message).ShowDialog();
                }
            }
            else
            {
                // Attach New Image
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "انتخاب تصویر سند",
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                        if (fileBytes.Length > 0)
                        {

                            long taskId = CL_HESABDARI.Gettaskid((double)currentIDH, 34);
                            if (taskId <= 0)
                            {
                                var insertSql = @"
INSERT INTO dbo.TASKS (PERSONEL,TASK,PERIORITY,STATUS,STDATE,STTIME,ENDATE,ENTIME,USERNAME,COMP_COD,SUMTIME,pic,ss,skid,num,tg,CTIM,USERCO,SEE)
VALUES (@PERSONEL,@TASK,@PERIORITY,1,@STDATE,@STTIME,NULL,NULL,@USERNAME,@COMP_COD,NULL,NULL,NULL,@skid,@num,@tg,GETDATE(),@USERCO,0);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                                var newTaskId = dbms.DoGetDataSQL<int>(insertSql, new
                                {
                                    PERSONEL = Baseknow.USERCOD,
                                    COMP_COD = currentRow.FHES,
                                    TASK = $"تصویر چک خزانه {khazanehNumber} مورخ {Strings.Format(Convert.ToInt64(DATE.Text.ToRawTarikh()), "####/##/##")}  ردیف {currentRow.RADIF}",
                                    PERIORITY = 2,
                                    STDATE = Tarikh.GoGetPersianDate(true),
                                    STTIME = Convert.ToInt32(DateTime.Now.ToString("HHmm")),
                                    USERNAME = Baseknow.UUSER,
                                    skid = khazanehNumber,
                                    num = currentIDH,
                                    tg = 34,
                                    USERCO = Baseknow.USERCOD
                                }).FirstOrDefault();

                                taskId = newTaskId;
                            }

                            if (taskId > 0)
                            {
                                var fxType = Path.GetExtension(openFileDialog.FileName)?.ToLower();

                                var eventId = GetLatestCheckAttachment((int)taskId, 34);
                                string normalizedExt = string.IsNullOrWhiteSpace(fxType) ? ".jpg" : (fxType.StartsWith(".") ? fxType : $".{fxType}");
                                int today = Convert.ToInt32(Tarikh.GoGetPersianDate(true));
                                int nowTime = Convert.ToInt32(DateTime.Now.ToString("HHmm"));

                                // --- ساخت شرح کامل رویداد ---
                                string opType = currentRow.NO_AM == 1 ? "دریافت" : (currentRow.NO_AM == 2 ? "پرداخت" : "نامشخص");

                                string nahvaStr = currentRow.NAHVA switch
                                {
                                    1 => "نقد",
                                    2 => "چک",
                                    3 => "سایر",
                                    4 => "واگذاری",
                                    5 => "برگشتی",
                                    6 => "مسترد",
                                    _ => "نامشخص"
                                };

                                // استفاده از نام حساب (در صورت وجود) یا کد حساب
                                string fromAcc = !string.IsNullOrEmpty(currentRow.NAME_FHES) ? currentRow.NAME_FHES : currentRow.FHES;
                                string toAcc = !string.IsNullOrEmpty(currentRow.NAME_THES) ? currentRow.NAME_THES : currentRow.THES;
                                string amountStr = currentRow.MABL?.ToString("N0") ?? "0";
                                string sharhStr = currentRow.SHARH ?? "-";

                                //string eventText = $"تصویر چک خزانه {khazanehNumber} ردیف {(currentRow.RADIF ?? currentIDH)} " + $"به شرح {currentRow.SHARH}";
                                // ترکیب رشته نهایی
                                string eventText = $"تصویر چک خزانه {khazanehNumber} ردیف {(currentRow.RADIF ?? currentIDH)} | " + $"{opType} - {nahvaStr} - از: {fromAcc} - به: {toAcc} - شرح: {sharhStr} - مبلغ: {amountStr}";

                                if (eventId is null)
                                {
                                    const string insertSql = @"INSERT INTO dbo.EVENTS(IDNUM, EVENTS, STDATE, STTIME, USERNAME, SUMTIME, skid, num, tg, FXTYPE, pic)
                                           VALUES(@TaskId, @Events, @StDate, @StTime, @UserName, @SumTime, @SkId, @Num, @RowId, @FxType, @Pic)";
                                    dbms.DoExecuteSQL(insertSql, new
                                    {
                                        TaskId = taskId,
                                        Events = eventText,
                                        StDate = today,
                                        StTime = nowTime,
                                        UserName = Baseknow.UUSER ?? CL_HESABDARI.UCurrentUser(),
                                        SumTime = 0,
                                        SkId = khazanehNumber,
                                        Num = currentIDH,
                                        RowId = 34,
                                        FxType = normalizedExt,
                                        Pic = fileBytes
                                    });
                                }
                                else
                                {
                                    const string updateSql = @"UPDATE dbo.EVENTS
                                           SET EVENTS = @Events, STDATE = @StDate, STTIME = @StTime, USERNAME = @UserName,
                                               SUMTIME = @SumTime, skid = @SkId, num = @Num, tg = @RowId, FXTYPE = @FxType, pic = @Pic
                                           WHERE IDNUM = @TaskId AND IDD = @EventId";
                                    dbms.DoExecuteSQL(updateSql, new
                                    {
                                        TaskId = taskId,
                                        Events = eventText,
                                        StDate = today,
                                        StTime = nowTime,
                                        UserName = Baseknow.UUSER ?? CL_HESABDARI.UCurrentUser(),
                                        SumTime = 0,
                                        SkId = khazanehNumber,
                                        Num = currentIDH,
                                        RowId = 34,
                                        FxType = normalizedExt,
                                        Pic = fileBytes,
                                        EventId = eventId
                                    });
                                }

                                currentRow.HasAttachment = true;
                                universControl.PopNotifyShow("تصویر با موفقیت ضمیمه شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                            }
                            else
                            {
                                new Msgwin(false, "خطا در ایجاد رکورد اتوماسیون (TASKS).").ShowDialog();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        new Msgwin(false, "خطا در ذخیره تصویر: " + ex.Message).ShowDialog();
                    }
                }
            }
        }

        private AutomasionEVNT? GetLatestCheckAttachment(int automationId, int rowId)
        {
            var sql = @"SELECT TOP 1 IDNUM,IDD,EVENTS,STDATE,STTIME,USERNAME,COMPANY,SUMTIME,pic,skid,num,tg,FXTYPE 
                        FROM dbo.EVENTS 
                        WHERE IDNUM = @idnum AND tg = @tg AND pic IS NOT NULL 
                        ORDER BY IDD DESC";
            return dbms.DoGetDataSQL<AutomasionEVNT>(sql, new { idnum = automationId, tg = rowId }).FirstOrDefault();
        }

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

        private void Form_Current()
        {
            if (IsNull(this.ID.Text) || this.ID.Text == "0")
            {
                PGET_LST_SUB.IsReadOnly = true;
            }
            else
            {
                PGET_LST_SUB.IsReadOnly = false;
            }
            this.MABNA.Text = null;
            if (IsNull(this.ID.Text) || this.ID.Text == "0")
            {
                PGET_LST_SUB.IsReadOnly = true;
            }
            else
            {
                PGET_LST_SUB.IsReadOnly = false;
                if (string.IsNullOrEmpty(this.N_S.Text))
                {
                    new Msgwin(false, "خزانه داري جاري سند نخورده است بنابر اين براي آن سند جديد صادر ميگردد").ShowDialog();
                    SANAD();
                }
                else
                {
                    if (_currentDeedData != null)
                    {
                        this.MABNA.Text = _currentDeedData.@base.ToString();
                        if (_currentDeedData.GHATEI)
                        {
                            LETSANAD = false;
                            this.InvokeWhenHandleReady(hwnd =>
                            {
                                CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);
                            });
                            AllowEdits = false;
                            this.PGET_LST_SUB.IsReadOnly = true;
                            this.PGET_LST_SUB.CanUserAddRows = false;
                            this.PGET_LST_SUB.CanUserDeleteRows = false;
                        }
                        else
                        {
                            LETSANAD = true;
                            this.InvokeWhenHandleReady(hwnd =>
                            {
                                CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                            });
                            AllowEdits = true;
                            this.PGET_LST_SUB.IsReadOnly = false;
                            this.PGET_LST_SUB.CanUserAddRows = true;
                            this.PGET_LST_SUB.CanUserDeleteRows = true;
                        }
                    }
                }

            }
            if (this.OKF.IsChecked == true)
            {
                if (Convert.ToInt32(ID.Text) > 0)
                {
                    //this.AllowDeletions = false;
                    //this.AllowEdits = false;
                    this.InvokeWhenHandleReady(hwnd =>
                    {
                        CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);
                    });
                    AllowEdits = false;
                    PGET_LST_SUB.IsReadOnly = true;
                    this.ESLAH.IsEnabled = true;
                    DATE.IsEnabled = false;
                    ID.IsEnabled = false;
                    KIND.IsEnabled = false;
                    IDK.IsEnabled = false;
                    USER_NAME.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    DEPATMAN.IsEnabled = false;
                    SHIFT.IsEnabled = false;
                    Text10.IsEnabled = false;
                    Text8.IsEnabled = false;
                    MANDS.IsEnabled = false;
                    MANDB.IsEnabled = false;
                    SGN1.IsEnabled = false;
                    sgn1usid.IsEnabled = false;
                    SGN2.IsEnabled = false;
                    sgn2usid.IsEnabled = false;
                    SGN3.IsEnabled = false;
                    sgn3usid.IsEnabled = false;
                    MABL.IsEnabled = false;
                    //PERSONEL.IsEnabled = false;
                    DELETE_FACTOR22.IsEnabled = false;
                    SAVEBTN.IsEnabled = false;
                }

            }


            //#Left ی و ک عربی باید درست بشه
            if (!this.NewRecord && CL_HESABDARI.UCurrentUser() != this.USER_NAME.Text)
            {
                if (!CL_HESABDARI.LETSGO("DPSEE"))
                {
                    this.ESLAH.IsEnabled = false;
                }
                else
                {
                    this.ESLAH.IsEnabled = true;
                }
            }
            else
            {
                this.ESLAH.IsEnabled = true;
            }
            if ((bool)Baseknow.SIGN)
            {
                if (this.SGN1.IsChecked == true || this.SGN2.IsChecked == true || this.SGN3.IsChecked == true)
                {
                    this.Command12.IsEnabled = true;
                    this.Command23.IsEnabled = true;
                    this.Command24.IsEnabled = true;
                }
                else
                {
                    this.Command12.IsEnabled = false;
                    this.Command23.IsEnabled = false;
                    this.Command24.IsEnabled = false;
                }
            }
            this.PERSONEL.Visibility = Visibility.Visible;

            if (Convert.ToInt32(ID.Text) > 0)
            {
                this.InvokeWhenHandleReady(hwnd =>
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 34, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                });
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }
        }

        public void SANAD(IDbTransaction externalTransaction = null)
        {
            try
            {
                var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADKHAZ(Convert.ToInt64(ID.Text), Convert.ToInt64(ID.Text), false, externalTransaction);

                if (SanadNumber != null)
                {
                    N_S.Text = SanadNumber.ToString();
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات صدور سند خزانه داری").ShowDialog();
            }
        }

        private void SANAD_OUTDATED()
        {
            //منسوخ شده
            return;
            double max_ns, MABL_CHK;
            string SHART, SHRH;
            var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            //SHRST.Open("DEED_HED", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            SHRH = "خزانه داري شماره " + this.ID.Text + "مورخ " + DATE.Text;
            if (!IsNull(this.N_S.Text))
            {
                dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + DATE.Text.ToRawTarikh() + ",SHARH_S = '" + SHRH + "',GHATEI = 0,NO_S = 5,OKF=-1,USER_NAME ='" + USER_NAME.Text + "' WHERE N_S =" + N_S.Text);
                //        DoCmd.OpenForm("BUN");
                max_ns = Convert.ToDouble(N_S.Text);
            }
            else
            {
                max_ns = CL_HESABDARI.Createsanad(Convert.ToInt64(DATE.Text.ToRawTarikh()), SHRH, 0, 5, -1, USER_NAME.Text);
            }
            if (IsNull(N_S.Text) || Convert.ToDouble(N_S.Text) != max_ns)
            {
                N_S.Text = max_ns.ToString();
            }

            if (!IsNull(N_S.Text))
            {
                dbms.DoExecuteSQL("DELETE FROM dbo.DEED_DTL WHERE (N_S = " + N_S.Text + ")");
            }
            try
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 55, 1) == "5")
                {
                    dbms.DoExecuteSQL("INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BED, N_SERI, BANK, N_S, HES,ARZD) SELECT     THES_K, THES_M, THES_T, THES_T2, THES_T3, THES_T4, LEFT(SHARH + ' - ' + '" + CL_HESABDARI.GETDEPART(Convert.ToInt64(DEPATMAN.SelectedValue)) + "',100), MABL, N_SERI, BANK," + N_S.Text + " AS Expr1, THES,ARZD FROM dbo.PGET_LST WHERE  (ID = " + ID.Text + ")");
                    dbms.DoExecuteSQL("INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BES, N_SERI, BANK, N_S, HES,ARZD) SELECT     FHES_K, FHES_M, FHES_T, FHES_T2, FHES_T3, FHES_T4, LEFT(SHARH + ' - ' + '" + CL_HESABDARI.GETDEPART(Convert.ToInt64(DEPATMAN.SelectedValue)) + "',100), MABL, N_SERI, BANK," + N_S.Text + " AS Expr1, FHES,ARZD FROM dbo.PGET_LST WHERE  (ID = " + ID.Text + ")");
                }
                else
                {
                    dbms.DoExecuteSQL("INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BED, N_SERI, BANK, N_S, HES,ARZD) SELECT     THES_K, THES_M, THES_T, THES_T2, THES_T3, THES_T4, SHARH, MABL, N_SERI, BANK," + N_S.Text + " AS Expr1, THES,ARZD FROM dbo.PGET_LST WHERE  (ID = " + ID.Text + ")");
                    dbms.DoExecuteSQL("INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BES, N_SERI, BANK, N_S, HES,ARZD) SELECT     FHES_K, FHES_M, FHES_T, FHES_T2, FHES_T3, FHES_T4, SHARH, MABL, N_SERI, BANK," + N_S.Text + " AS Expr1, FHES,ARZD FROM dbo.PGET_LST WHERE  (ID = " + ID.Text + ")");
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در صدور سند رخ داده است بعضي لز حساب ها وجود ندارد ").ShowDialog();
            }
            //rst.Close();
            //SHRST.Close();
            //DoCmd.Close(acForm, "bun");

            //DoCmd.RunCommand(acCmdSave);
            //this.PGET_LST_SUB.Requery();
        }

        //#گرفتن آی دی جدید
        //Used_SAVEBTN_Click
        private bool DoCmdSaveHeader()
        {
            bool SuccessSave = false;

            bool isNewRecordAtStart = NewRecord;
            string originalId = ID.Text;
            string originalIdk = IDK.Text;
            string originalNs = N_S.Text;

            //Form_BeforeUpdate
            OKF.IsChecked = true;

            if (isNewRecordAtStart)
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        // TABLOCK+UPDLOCK acquires a table-level update lock at the very start,
                        // serializing all concurrent saves. The second session blocks here until the
                        // first commits — so both MAX(ID) and MAX(IDK) are read after the previous
                        // insert is visible, preventing duplicate IDs, duplicate IDKs, and deadlocks.
                        // GetNewIDD is intentionally not used: it commits its own transaction and
                        // releases its lock before our INSERT, leaving a race window.
                        var _maxId = db.Query<long?>(
                            "SELECT MAX(ID) FROM dbo.PGET_HED WITH (TABLOCK, UPDLOCK)",
                            null, transaction).FirstOrDefault();
                        var _id = (_maxId ?? 0) + 1;
                        ID.Text = _id.ToString();

                        var RST_M = db.Query<string>(
                            $"SELECT MAX(IDK) AS MaxOfidK FROM dbo.PGET_HED WHERE (KIND = {KIND.SelectedValue})",
                            null, transaction).ToList();
                        if (RST_M.Count == 0 || string.IsNullOrEmpty(RST_M.FirstOrDefault()))
                        {
                            IDK.Text = "1";
                        }
                        else
                        {
                            IDK.Text = Convert.ToString(Convert.ToInt64(RST_M.FirstOrDefault()) + 1);
                        }

                        // N_S must be set before INSERT because PGET_HED has a UNIQUE constraint on N_S
                        // and SQL Server allows only one NULL per unique index — a second NULL causes error 2627.
                        var _dateRaw = DATE.Text.ToRawTarikh();
                        var _sharhd = "خزانه داري شماره " + _id + " مورخ " + Strings.Format(Convert.ToInt64(_dateRaw), "####/##/##");
                        var _ns = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.Createsanad(Convert.ToInt64(_dateRaw), _sharhd, 0, 5, 1, USER_NAME.Text);
                        N_S.Text = _ns.ToString();

                        try
                        {
                            const string insertSql = @"
                                INSERT INTO dbo.PGET_HED(ID, DATE, MOLAH, DEPATMAN, SHIFT, USER_NAME, KIND, OKF, IDK, UID, N_S)
                                VALUES (@ID, @DATE, @MOLAH, @DEPATMAN, @SHIFT, @USER_NAME, @KIND, @OKF, @IDK, @UID, @N_S)";
                            var insertParameters = new
                            {
                                ID = _id,
                                DATE = _dateRaw,
                                MOLAH = MOLAH.Text.Trim(),
                                DEPATMAN = DEPATMAN.SelectedValue,
                                SHIFT = SHIFT.SelectedValue,
                                USER_NAME = USER_NAME.Text,
                                KIND = KIND.SelectedValue,
                                OKF = Convert.ToByte(OKF.IsChecked),
                                IDK = IDK.Text,
                                UID = Baseknow.USERCOD,
                                N_S = _ns
                            };
                            db.Execute(insertSql, insertParameters, transaction);

                            SANAD(transaction); //برای اینکه در زمان صدور خزانه جدید برای اینکه همزمان دوتا شماره سند خالی نخوره برای خزانه جدید , سریع میگیم سند بزنه توی خزانه جدید که تداخل ایجاد نشه

                            transaction.Commit(); db?.Close();

                            RefreshAfterInsert();
                            SuccessSave = true;
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("duplicate key value is (<NULL>)", StringComparison.OrdinalIgnoreCase))
                            {
                                new Msgwin(false, "در جدول خزانه یک رکورد با شماره سند خالی (NULL) مانده بود. سیستم برای رکورد جدید شماره موقت داد؛ دوباره ذخیره را انجام دهید.").ShowDialog();
                            }
                            else if (ex.Number == 2627)
                            {
                                new Msgwin(false, "خزانه با این تاریخ (تاریخ تکراری) قبلا ثبت شده , تاریخ را اصلاح کنید").ShowDialog();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            if (!SuccessSave)
                            {
                                try { transaction?.Rollback(); }
                                catch
                                {
                                    // The transaction may already be completed/invalid after a SQL error.
                                }
                                // Restore Reset
                                ID.Text = originalId;
                                IDK.Text = originalIdk;
                                N_S.Text = originalNs;

                                db?.Close();
                            }
                        }
                    }
                }
            }
            else //UPDATE HEAD
            {
                byte _SGN1_ = Convert.ToByte(SGN1.IsChecked);
                byte _SGN2_ = Convert.ToByte(SGN2.IsChecked);
                byte _SGN3_ = Convert.ToByte(SGN3.IsChecked);

                try
                {
                    const string updateSql = @"
                        UPDATE dbo.PGET_HED
                        SET DATE = @DATE,
                            MOLAH = @MOLAH,
                            DEPATMAN = @DEPATMAN,
                            SHIFT = @SHIFT,
                            KIND = @KIND,
                            OKF = @OKF,
                            IDK = @IDK,
                            SGN1 = @SGN1,
                            SGN2 = @SGN2,
                            SGN3 = @SGN3
                        WHERE ID = @ID";
                    var updateParameters = new
                    {
                        DATE = DATE.Text.ToRawTarikh(),
                        MOLAH = MOLAH.Text.Trim(),
                        DEPATMAN = DEPATMAN.SelectedValue,
                        SHIFT = SHIFT.SelectedValue,
                        KIND = KIND.SelectedValue,
                        OKF = Convert.ToByte(OKF.IsChecked),
                        IDK = IDK.Text,
                        SGN1 = _SGN1_,
                        SGN2 = _SGN2_,
                        SGN3 = _SGN3_,
                        ID = ID.Text
                    };
                    dbms.DoExecuteSQL(updateSql, updateParameters);

                    SuccessSave = true;
                    RefreshAfterUpdate();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                    {
                        new Msgwin(false, "خزانه با این تاریخ (تاریخ تکراری) قبلا ثبت شده , تاریخ را اصلاح کنید").ShowDialog();
                    }
                }

            }
            if (SuccessSave)
            {
                if (Convert.ToInt32(ID.Text) > 0)
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 34, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                }
                else
                {
                    this.SGN1.IsEnabled = false;
                    this.SGN2.IsEnabled = false;
                    this.SGN3.IsEnabled = false;
                }
            }

            return SuccessSave;
        }


        //#صدور سند
        //Used_SAVEBTN_Click
        //private void SANAD()
        //{
        //    //SANAD();
        //    //Form_AfterUpdate
        //    try
        //    {
        //        AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADKHAZ(Convert.ToInt64(ID.Text), Convert.ToInt64(ID.Text), false);
        //    }
        //    catch (Exception)
        //    {
        //        new Msgwin(false, "خطا در انجام عملیات صدور سند خزانه داری").ShowDialog();
        //    }
        //    LETSANAD = false;
        //}

        private void Form_Delete()
        {
            SANAD();
            var RecordsetClone = dbms.DoGetDataSQL<PGET_HED>("SELECT PGET_HED.* FROM PGET_HED ORDER BY DATE, ID").ToList();
            if (RecordsetClone.Count > 0)// یعنی که باید هدر را سلکت و بررسی کنیم
            {
                new Msgwin(false, "اين خزانه داري داراي اطلاعات  مي باشد .ابتدا اطلاعات سطرهاي زير را حذف كنيد سپس خزانه داري را حذف نمائيد.جهت مشاهده توضيحات بيشتر روي فرم كليد F1  را فشار دهيد.").ShowDialog();
                CANCEL = true;
            }
        }

        private void Form_Open()
        {
            //DoCmd.GoToRecord(acDataForm, this.NAME, acLast);
            //DoCmd.Maximize();
            //Forms["BASEKNOW"]["hhwin"] = this.hWnd;
            if (!CL_HESABDARI.LETSGO("ESLAHK"))
            {
                this.ESLAH.Visibility = Visibility.Hidden;
            }
            else
            {
                this.ESLAH.Visibility = Visibility.Visible;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                //#Left
                // this.OKF.DefaultValue = true;
                //this.OKF.IsChecked = true;
            }
            else
            {
                this.OKF.IsChecked = false;
            }
            //if (!CL_HESABDARI.LETSGO("DPDEED")) //برای مشاهده خزانه استفاده میشود نه اینجا
            //{
            //    this.RecordSource = "SELECT     PGET_HED.* FROM PGET_HED WHERE     (USER_NAME = N'" + UCurrentUser() + "') ORDER BY DATE, ID";
            //}
            if ((bool)Baseknow.SIGN)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
                this.sgn1usid.Visibility = Visibility.Visible;
                this.sgn2usid.Visibility = Visibility.Visible;
                this.sgn3usid.Visibility = Visibility.Visible;
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    this.Command12.IsEnabled = true;
                    this.Command23.IsEnabled = true;
                    this.Command24.IsEnabled = true;
                }
                else
                {
                    this.Command12.IsEnabled = false;
                    this.Command23.IsEnabled = false;
                    this.Command24.IsEnabled = false;
                }
            }
            //this.PERSONEL.DataContext = dbms.;
            //this.sgn1usid.RowSource = GetUserList;
            //this.sgn2usid.RowSource = GetUserList;
            //this.sgn3usid.RowSource = GetUserList;


        }



        private void Form_KeyPress(int KeyAscii)
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 48, 1) == "5")
            {
                switch (KeyAscii)
                {
                    case 1610:
                    case 1609:
                    case 1656:
                    case 1744:
                    case 1741:
                        {
                            KeyAscii = 1740;
                            break;
                        }
                    case 1603:
                    case 1706:
                    case 1890:
                    case 1708:
                    case 1707:
                        {
                            KeyAscii = 1705;
                            break;
                        }
                }
            }


        }

        private void DATE_AfterUpdate()
        {
            DTDT = this.DATE.Text.ToRawTarikh();
            if (IsNull(this.DATE.Text.ToRawTarikh()))
            {
                PGET_LST_SUB.IsReadOnly = true;
            }
            else
            {
                PGET_LST_SUB.IsReadOnly = false;
                DTCHK = true;
            }
        }

        private void DATE_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE.Text.ToRawTarikh();
            DATE.SelectAll();
        }

        private void DATE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //string date_n_val = DATE.Text.ToRawTarikh();
            //if (!string.IsNullOrEmpty(date_n_val))
            //{
            //    if (!Tarikh.IsValidedDate(date_n_val))
            //    {
            //        DATE.Text = BEFOREDATEN;
            //        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
            //        return;
            //    }
            //    else
            //    {
            //        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
            //        {
            //            DATE.Text = BEFOREDATEN;
            //            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
            //            return;
            //        }
            //    }
            //}
            //else
            //{
            //    DATE.Text = BEFOREDATEN;
            //    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
            //    return;
            //}
        }



        private void id_DblClick(int CANCEL)
        {
            if (!(IsNull(Convert.ToInt32(ID.Text) != 0)))
            {
                SANAD();
            }
        }

        private void KIND_BeforeUpdate()
        {
            //    var rst = new ADODB.Recordset();
            if (/*KIND != KIND.OldValue*/true)
            {
                if (IsNull(IDK.Text) || IDK.Text == "0")
                {
                    var rst = dbms.DoGetDataSQL<int?>("SELECT     MAX(IDK) AS MaxOfidK FROM dbo.PGET_HED WHERE     (KIND = " + KIND.SelectedValue + ")").ToList();
                    if (rst.Count == 0 || IsNull(rst.FirstOrDefault()))
                    {
                        IDK.Text = "1";
                    }
                    else
                    {
                        IDK.Text = Convert.ToString(rst.FirstOrDefault() + 1);
                    }
                    //rst.Close();
                }
                else
                {
                    Msgwin msgwin = new Msgwin(true, "با تغيير نوع برگه شماره جديد به آن اختصاص مي يابد آيا اين عمل را تائيد مي نمائيد؟");
                    msgwin.ShowDialog();

                    if (msgwin.DialogResult is true)
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT     MAX(IDK) AS MaxOfidK FROM dbo.PGET_HED WHERE     (KIND = " + this.KIND + ")").ToList();
                        if (rst.Count == 0 || IsNull(rst.FirstOrDefault()))
                        {
                            IDK.Text = "1";
                        }
                        else
                        {
                            IDK.Text = Convert.ToString(rst.FirstOrDefault()) + 1;
                        }
                        //rst.Close();
                    }
                    else
                    {
                        CANCEL = true;
                    }
                }
            }
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (string.IsNullOrEmpty(ID.Text))
            {
                ID.Text = "0";
            }
            if (Convert.ToDouble(ID.Text) > 0)
            {
                dt = DateTime.Now;
                // If Forms![baseknow]![TRANSF] Then
                CL_HESABDARI.TR("PGET_HED", "(ID = " + ID.Text + " )", dt, 1);
                CL_HESABDARI.TR("PGET_LST", "(ID = " + ID.Text + " )", dt, 2);
                // DoCmd.RunSQL ("INSERT INTO dbo.TR_PGET_HED   (ID, DATE,MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF, UP_TIME, UP_DATE,UP_USER_NAME,PC_NAME,IPADD) SELECT  ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, KIND, IDK, OKF," && CDbl(dt) && "   AS Expr1," && FARSIDATE(Now()) && " AS Expr2,'" && UCurrentUser() && "','" && CurrentMachineName() && "' , '" && GETIPADD() && "'   FROM dbo.PGET_HED WHERE (ID = " && Me.ID && " ) ")
                // DoCmd.RunSQL ("INSERT INTO dbo.TR_PGET_LST   (ID, DATE, RADIF, NO_AM, NAHVA, FHES_K, FHES_M, FHES_T, THES_K, THES_M, THES_T, SHARH, MABL, N_SERI, BANK, FHES, THES, UP_TIME, UP_DATE) SELECT     ID, DATE, RADIF, NO_AM, NAHVA, FHES_K, FHES_M, FHES_T, THES_K, THES_M, THES_T, SHARH, MABL, N_SERI, BANK, FHES, THES," && CDbl(dt) && "   AS Expr1," && FARSIDATE(Now()) && " AS Expr2  FROM dbo.PGET_LST WHERE     (ID = " && Me.ID && ")")
                // End If
                //this.AllowDeletions = true;
                //this.AllowEdits = true;
                CL_LMethods.AllowDeletions(this.GetType().Name, false, new WindowInteropHelper(this).Handle);

                ApplyDataGridItems();
                AllowEdits = false;
                //PGET_LST_SUB.IsReadOnly = false;
                //this.PGET_LST_SUB.CanUserAddRows = true;
                //this.AllowDeletions = true;
                CL_LMethods.AllowDeletions(this.GetType().Name, true, new WindowInteropHelper(this).Handle);
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    Msgwin msgwin = new Msgwin(false, " اول امضاء را برداريد ...");
                    msgwin.Show();
                    //SGN1.IsEnabled = true;
                    //SGN2.IsEnabled = true;
                    //SGN3.IsEnabled = true;
                    //PERSONEL.IsEnabled = true;

                    //PGET_LST_SUB.IsReadOnly = false; //New Added Line Code
                    //PGET_LST_SUB.IsReadOnly = true; //New Added Line Code
                    ////  dbms.doge("mesageform", default, default, default, default, acDialog, " اول امضاء را برداريد ...");
                    //this.DATE.IsReadOnly = true;
                    //this.MOLAH.IsReadOnly = true;
                    //this.AllowEdits = true;
                }
                else
                {
                    this.PGET_LST_SUB.IsReadOnly = false;
                    this.MOLAH.IsReadOnly = false;
                    this.DATE.IsReadOnly = false;
                    AllowEdits = true;
                    this.PGET_LST_SUB.IsReadOnly = false;
                    this.PGET_LST_SUB.CanUserAddRows = true;
                    PGET_LST_SUB.IsReadOnly = false;
                    DATE.IsEnabled = true;
                    ID.IsEnabled = true;
                    KIND.IsEnabled = true;
                    IDK.IsEnabled = true;
                    USER_NAME.IsEnabled = true;
                    MOLAH.IsEnabled = true;
                    DEPATMAN.IsEnabled = true;
                    SHIFT.IsEnabled = true;
                    Text10.IsEnabled = true;
                    Text8.IsEnabled = true;
                    MANDS.IsEnabled = true;
                    MANDB.IsEnabled = true;
                    SGN1.IsEnabled = true;
                    sgn1usid.IsEnabled = true;
                    SGN2.IsEnabled = true;
                    sgn2usid.IsEnabled = true;
                    SGN3.IsEnabled = true;
                    sgn3usid.IsEnabled = true;
                    MABL.IsEnabled = true;
                    PERSONEL.IsEnabled = true;
                    DELETE_FACTOR22.IsEnabled = true;
                    SAVEBTN.IsEnabled = true;
                }
                // If UCurrentUser() <> "َAdminister" And UCurrentUser() <> Me.USER_NAME Then
                // Me.USER_NAME = UCurrentUser()
                // DoCmd.RunCommand acCmdSaveRecord
                // End If
            }
            CL_HESABDARI.LetSigneTick(this.GetType().Name, 34, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);

        }

        private void PERSONEL_AfterUpdate()
        {
            Meidnum = CL_HESABDARI.PERSONELUpdate(34, Convert.ToDouble(ID.Text), Convert.ToInt32(PERSONEL.Text), "'خزانه داري   شماره: " + ID.Text + " مورخ " + Strings.Format(DATE.Text, "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'");

            Msgwin msgwin = new Msgwin(false, "ارجاع داده شد.");
            msgwin.Show();
        }

        private void DATE_BeforeUpdate()
        {
            CANCEL = CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT));
            if (!this.NewRecord && Baseknow.WAR == 1)
            {
                Msgwin msgwin = new Msgwin(true, "تغيرات داده شده ثبت شود؟ در صورتيكه مايليد تغييرات ثبت نشود بعداز بستن اين پنجره كليد  اسكيپ را فشار دهيد.");
                msgwin.ShowDialog();
                //Forms["BASEKNOW"]["Text44"] = false;
                //DoCmd.OpenForm("MSGDIALOG", default, default, default, default, acDialog, "تغيرات داده شده ثبت شود؟ در صورتيكه مايليد تغييرات ثبت نشود بعداز بستن اين پنجره كليد  اسكيپ را فشار دهيد.");
                if (!msgwin.DialogResult is true)
                {
                    CANCEL = true;
                }
            }
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            //var rst = new ADODB.Recordset();
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(ID.Text), 34);
            if (MID > 0d)
            {                                                                                                                                                                                                                                                                                                           //CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute)                                                         
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'خزانه داري   شماره: " + ID.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToInt32(ID.Text), 34);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                //this.OKF = true;
                OKF.IsChecked = true;

            sgn1usid.Tag = Baseknow.USERCOD;
            sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    this.Command12.IsEnabled = true;
                    this.Command23.IsEnabled = true;
                    this.Command24.IsEnabled = true;

                    ApplyDataGridItems();

                    PGET_LST_SUB.IsReadOnly = true;
                    this.ESLAH.IsEnabled = true;
                    DATE.IsEnabled = false;
                    ID.IsEnabled = false;
                    KIND.IsEnabled = false;
                    IDK.IsEnabled = false;
                    USER_NAME.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    DEPATMAN.IsEnabled = false;
                    SHIFT.IsEnabled = false;
                    Text10.IsEnabled = false;
                    Text8.IsEnabled = false;
                    MANDS.IsEnabled = false;
                    MANDB.IsEnabled = false;
                    sgn1usid.IsEnabled = false;
                    sgn2usid.IsEnabled = false;
                    sgn3usid.IsEnabled = false;
                    MABL.IsEnabled = false;
                    PERSONEL.IsEnabled = true;
                    DELETE_FACTOR22.IsEnabled = false;
                    SAVEBTN.IsEnabled = false;
                }
            }
            else
            {
                this.Command12.IsEnabled = false;
                this.Command23.IsEnabled = false;
                this.Command24.IsEnabled = false;
            }
            dbms.DoExecuteSQL($"UPDATE PGET_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE ID = {ID.Text}");
        }

        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(ID.Text), 34);

            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'خزانه داري   شماره: " + ID.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL($"insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values ({Convert.ToInt32(Baseknow.USERCOD)},'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(ID.Text), 34);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
            }

            PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                OKF.IsChecked = true;

            sgn2usid.Tag = Baseknow.USERCOD;
            sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    this.Command12.IsEnabled = true;
                    this.Command23.IsEnabled = true;
                    this.Command24.IsEnabled = true;

                    ApplyDataGridItems();

                    PGET_LST_SUB.IsReadOnly = true;
                    this.ESLAH.IsEnabled = true;
                    DATE.IsEnabled = false;
                    ID.IsEnabled = false;
                    KIND.IsEnabled = false;
                    IDK.IsEnabled = false;
                    USER_NAME.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    DEPATMAN.IsEnabled = false;
                    SHIFT.IsEnabled = false;
                    Text10.IsEnabled = false;
                    Text8.IsEnabled = false;
                    MANDS.IsEnabled = false;
                    MANDB.IsEnabled = false;
                    sgn1usid.IsEnabled = false;
                    sgn2usid.IsEnabled = false;
                    sgn3usid.IsEnabled = false;
                    MABL.IsEnabled = false;
                    PERSONEL.IsEnabled = true;
                    DELETE_FACTOR22.IsEnabled = false;
                    SAVEBTN.IsEnabled = false;
                }
            }
            else
            {
                this.Command12.IsEnabled = false;
                this.Command23.IsEnabled = false;
                this.Command24.IsEnabled = false;
            }


            dbms.DoExecuteSQL($"UPDATE PGET_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE ID = {ID.Text}");

        }

        private void ApplyDataGridItems()
        {
            try
            {
                if (PGET_LST_SUB.Items is IEditableCollectionView editableCollectionView)
                {
                    if (editableCollectionView.IsAddingNew)
                    {
                        editableCollectionView.CancelNew(); // discard the new item
                    }
                    if (editableCollectionView.IsEditingItem)
                    {
                        editableCollectionView.CommitEdit(); // commit the edit transaction
                    }
                }
            }
            catch { }

        }

        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(ID.Text), 34);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'خزانه داري   شماره: " + ID.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(ID.Text), 34);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + DateTime.Now.Hour * (100 + DateTime.Now.Minute) + ",34," + ID.Text + ",34 )");
            }
            PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                OKF.IsChecked = true;
            sgn3usid.Tag = Baseknow.USERCOD;
            sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;
            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    this.Command12.IsEnabled = true;
                    this.Command23.IsEnabled = true;
                    this.Command24.IsEnabled = true;

                    ApplyDataGridItems();

                    PGET_LST_SUB.IsReadOnly = true;
                    this.ESLAH.IsEnabled = true;
                    DATE.IsEnabled = false;
                    ID.IsEnabled = false;
                    KIND.IsEnabled = false;
                    IDK.IsEnabled = false;
                    USER_NAME.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    DEPATMAN.IsEnabled = false;
                    SHIFT.IsEnabled = false;
                    Text10.IsEnabled = false;
                    Text8.IsEnabled = false;
                    MANDS.IsEnabled = false;
                    MANDB.IsEnabled = false;
                    sgn1usid.IsEnabled = false;
                    sgn2usid.IsEnabled = false;
                    sgn3usid.IsEnabled = false;
                    MABL.IsEnabled = false;
                    PERSONEL.IsEnabled = true;
                    DELETE_FACTOR22.IsEnabled = false;
                    SAVEBTN.IsEnabled = false;
                }
            }
            else
            {
                this.Command12.IsEnabled = false;
                this.Command23.IsEnabled = false;
                this.Command24.IsEnabled = false;
            }

            dbms.DoExecuteSQL($"UPDATE PGET_HED SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE ID = {ID.Text}");

        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }


        private void PGET_LST_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && PGET_LST_SUB?.SelectedItem != null && PGET_LST_SUB?.SelectedItem?.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (PGET_LST_SUB?.Items.Count > 0)
                {
                    if (!(PGET_LST_SUB.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = PGET_LST_SUB.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = PGET_LST_SUB.SelectedIndex;
                }
            }

            //// Cast the sender to a DataGrid
            //var dataGrid = sender as DataGrid;

            //if (dataGrid == null) return;

            //// Get the mouse click position relative to the DataGrid
            //Point mousePosition = e.GetPosition(dataGrid);

            //// Perform hit testing to get the target element
            //HitTestResult hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);

            //if (hitTestResult != null)
            //{
            //    // Traverse the visual tree to find the DataGridCell
            //    var targetCell = FindParent<DataGridCell>(hitTestResult.VisualHit);

            //    if (targetCell != null)
            //    {
            //        // Set the current cell to the clicked cell
            //        dataGrid.CurrentCell = new DataGridCellInfo(targetCell.DataContext, targetCell.Column);

            //        // Optionally, set focus to the clicked cell
            //        targetCell.Focus();

            //        CURRENT_CELL_ROW = targetCell;
            //    }
            //}
        }
        private void UpdateCurrentCellBasedOnMousePosition(DataGrid grid)
        {
            // Get mouse position relative to the DataGrid
            Point mousePosition = Mouse.GetPosition(grid);

            // Perform hit testing to find the visual element under the mouse
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(grid, mousePosition);

            if (hitTestResult?.VisualHit != null)
            {
                // Traverse the visual tree to locate the DataGridCell
                DataGridCell targetCell = FindParent<DataGridCell>(hitTestResult.VisualHit);

                if (targetCell != null)
                {
                    // Update CurrentCell to the cell under the mouse
                    grid.CurrentCell = new DataGridCellInfo(targetCell.DataContext, targetCell.Column);

                    // Set focus on the target cell
                    targetCell.Focus();
                }
            }
        }




        private bool Exit_Request()
        {
            if (PAYCHEK_EXIT_BTN == true)
            {
                //e.Cancel = true;
                PAYCHEK_EXIT_BTN = false; //RESET
                return true;
            }
            if (GETCHEK_EXIT_BTN == true)
            {
                //e.Cancel = true;
                GETCHEK_EXIT_BTN = false; //RESET
                return true;

            }
            if (FORCHEK_EXIT_BTN == true)
            {
                //e.Cancel = true;
                FORCHEK_EXIT_BTN = false; //RESET
                return true;

            }
            if (BAKCHEKP_EXIT_BTN == true)
            {
                //e.Cancel = true;
                BAKCHEKP_EXIT_BTN = false; //RESET
                return true;

            }
            if (BAKCHEK_EXIT_BTN == true)
            {
                //e.Cancel = true;
                BAKCHEK_EXIT_BTN = false; //RESET
                return true;
            }

            return false;
        }

        //private DateTime lastEscapeKeyPressTime = DateTime.MinValue;

        private void PGET_LST_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (PGET_LST_SUB?.SelectedItem is not null)
                {
                    if (PGET_LST_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((PGET_LST)PGET_LST_SUB.SelectedItem).Clone() as PGET_LST;
                    }
                }
            }
        }

        private void PGET_LST_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Additional Safety Check:
            if (PGET_LST_SUB == null || PGET_LST_SUB?.SelectedItem == null)
            {
                return;
            }

            var selectedItem = PGET_LST_SUB.SelectedItem as PGET_LST;
            if (selectedItem == null)
            {
                e.Handled = true;  // Stop further execution
                MANDB.Text = "";
                MANDS.Text = "";
                return;
            }

            if (NowIsReady && !(e is null))
            {
                if (PGET_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    if (!(PGET_LST_SUB?.CurrentCell.Column is null))
                        CURRENT_COLUMN_INDEX = PGET_LST_SUB.CurrentCell.Column.DisplayIndex;

                    CURRENT_ROW_INDEX = PGET_LST_SUB.SelectedIndex;

                    var _satr = (PGET_LST_SUB.SelectedItem as PGET_LST);
                    if (_satr != null)
                    {
                        if (Strings.Mid(Baseknow.OPTIONSS, 42, 1) == "5")
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(_satr?.FHES))
                                {
                                    MANDB.Text = CL_HESABDARI.GETMANDAH(_satr?.FHES);
                                }
                                else
                                {
                                    MANDB.Text = "";
                                }

                                if (!string.IsNullOrEmpty(_satr?.THES))
                                {
                                    MANDS.Text = CL_HESABDARI.GETMANDAH(_satr?.THES);
                                }
                                else
                                {
                                    MANDS.Text = "";
                                }
                            }
                            catch (Microsoft.Data.SqlClient.SqlException)
                            {
                                MANDB.Text = "";
                                MANDS.Text = "";
                            }
                        }
                    }
                }
            }
        }
        private void PGET_HED_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            PGET_LST_SUB.Dispatcher.Invoke(() =>
            {
                PGET_LST_SUB.CellEditEnding -= PGET_LST_SUB_CellEditEnding;
                PGET_LST_SUB.RowEditEnding -= PGET_LST_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    PGET_LST_SUB.CancelEdit();
                }
                else
                {
                    PGET_LST_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                PGET_LST_SUB.RowEditEnding += PGET_LST_SUB_RowEditEnding;
                PGET_LST_SUB.CellEditEnding += PGET_LST_SUB_CellEditEnding;
            });
        }

        private DateTime lastEscapeKeyPressTime;

        private DataGridCellInfo? _editingCellInfo;
        bool JustnowforcheckOpnned = false; //متغیری که برای جلوگیری از باز شدن مجددا پنجره مشخصات چک در ثبت واگذاری چک , چون بعد از حساب پنجره خودکار باز میشه و لازم نیست توی مبلغ که فوکوس میکنه دوباره باز بشه !

        private async Task ShowDialogAfterCurrentDispatcherOperationAsync(Window dialog)
        {
            // CellEditEnding can run while WPF has dispatcher processing disabled
            // (for example, when focus changes because another window is closing).
            // ShowDialog pushes a nested dispatcher frame, so defer it until WPF
            // has completed the current input/edit operation.
            await Dispatcher.InvokeAsync(dialog.ShowDialog, DispatcherPriority.Background);
        }

        private async void PGET_LST_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!NowIsReady || PGET_LST_SUB == null || PGET_LST_SUB.Items.Count == 0) return;

            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }

            DataGrid dataGrid = PGET_LST_SUB;
            int row_index = dataGrid?.ItemContainerGenerator.IndexFromContainer(e.Row) ?? -1;

            try
            {
                if (e != null)
                {
                    if (row_index < 0 || row_index >= dataGrid.Items.Count)
                    {
                        return;
                    }
                    int col_index = e.Column.DisplayIndex;
                    if (col_index < 0 || col_index >= dataGrid.Columns.Count) { }
                    else
                    {
                        CURRENT_COLUMN_INDEX = col_index;
                    }
                    CURRENT_ROW_INDEX = row_index;

                    DataGridRow rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
                    if (rowContainer == null)
                    {
                        dataGrid.ScrollIntoView(dataGrid.Items[row_index]);
                        rowContainer = dataGrid.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
                    }
                    if (rowContainer != null)
                    {
                        DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
                        if (presenter == null)
                        {
                            dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[col_index]);
                            presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
                        }
                        DataGridCell cell = presenter?.ItemContainerGenerator.ContainerFromIndex(col_index) as DataGridCell;
                        if (cell != null)
                        {
                            CURRENT_CELL_ROW = cell;
                        }
                    }

                    _editingCellInfo = new DataGridCellInfo(e.Row.Item, e.Column);
                }
            }
            catch
            {
                if (PGET_LST_SUB.SelectedIndex > -1)
                {
                    CURRENT_ROW_INDEX = PGET_LST_SUB.SelectedIndex;
                    row_index = CURRENT_ROW_INDEX;
                }
            }

            // Determine entered value
            object enteredValue = null;
            if (e.EditingElement is TextBox textBox)
            {
                enteredValue = textBox.Text.Trim();
            }
            else if (e.EditingElement is ComboBox comboBox)
            {
                enteredValue = comboBox.SelectedValue;
            }

            if (enteredValue != null)
            {
                ENTERED_VALUE_ROW = enteredValue;
            }

            // Safely cast item to expected type
            if (e.Row.Item is PGET_LST item)
            {
                CURRENT_ITMES_ROW = item;
            }

            //نوع عمليات
            if (e.Column.SortMemberPath == "NO_AM")
            {
                string? _NO_AM_ = null;

                var NO_AM_COMBOBOX = (e.EditingElement as ComboBox);
                TextBox NO_AM_COMBOBOX_TEX = (TextBox)NO_AM_COMBOBOX.Template.FindName("PART_EditableTextBox", NO_AM_COMBOBOX);

                if (NO_AM_COMBOBOX.SelectedValue is not null)
                {
                    _NO_AM_ = NO_AM_COMBOBOX.SelectedValue.ToString();
                }
                else
                {
                    _NO_AM_ = NO_AM_COMBOBOX_TEX.Text.Trim();
                }

                if (!int.TryParse(_NO_AM_, out int _)) //نوع عددی نیــــست
                {
                    CURRENT_ITMES_ROW.NO_AM = WAS_ROW_ITEM.NO_AM;
                }
                else
                {
                    switch (_NO_AM_.Trim())
                    {
                        case "1":
                            (e.EditingElement as ComboBox).SelectedValue = 1;
                            CURRENT_ITMES_ROW.NO_AM = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);

                            break;
                        case "2":
                            (e.EditingElement as ComboBox).SelectedValue = 2;
                            CURRENT_ITMES_ROW.NO_AM = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);

                            break;

                        default:
                            CURRENT_ITMES_ROW.NO_AM = null;
                            universControl.PopNotifyShow("چنین نوع عملیاتی وجود ندارد", Pop1, Pop1Text1, Pop_Border1);
                            break;
                    }
                }
                CURRENT_ITMES_ROW.NO_AM = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);


                #region NO_AM_BeforeUpdate
                if ((WAS_ROW_ITEM.NAHVA/*OldValue*/ == 4 || WAS_ROW_ITEM.NAHVA/*OldValue*/ == 5 || CURRENT_ITMES_ROW.NAHVA == 4 || CURRENT_ITMES_ROW.NAHVA == 5) && !this.NewRecord)
                {
                    Msgwin msgwin = new Msgwin(false, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                    msgwin.Show();
                    //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                    //CANCEL = Conversions.ToInteger(true);`
                    PGET_HED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    return;
                }
                #endregion

                #region NO_AM_AfterUpdate
                NO_AM_AfterUpdate(row_index);
                #endregion

                #region NO_AM_NotInList
                //try
                //{
                //    if ((WAS_ROW_ITEM.NO_AM/*.OldValue*/ == 4 || WAS_ROW_ITEM.NO_AM/*.OldValue*/ == 5 || CURRENT_ITMES_ROW.NO_AM == 4 || CURRENT_ITMES_ROW.NO_AM == 5) && !this.NewRecord)
                //    {
                //        Msgwin msgwin = new Msgwin(false, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                //        msgwin.ShowDialog();
                //        //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                //    }
                //    else
                //    {
                //        CURRENT_ITMES_ROW.NO_AM = Convert.ToInt32(ENTERED_VALUE_ROW);
                //    }
                //}
                //catch (Exception)
                //{
                //    Msgwin msgwin = new Msgwin(false, "مقدار وارد شده مجاز نيست...");
                //    msgwin.ShowDialog();
                //    //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "مقدار وارد شده مجاز نيست...");
                //}

                //NO_AM_AfterUpdate(row_index);

                //CURRENT_ITMES_ROW.NAHVA.SetFocus();
                if (CL_LMethods.IsValidIndex(PGET_LST_SUB, CURRENT_ROW_INDEX))
                {
                    var TheCol = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAHVA").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(PGET_LST_SUB.Items[CURRENT_ROW_INDEX], PGET_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                }
                //THECELL.Focus();
                #endregion

                #region NO_AM_OnClick
                //#Check Matter
                //اینا گزارشات 
                //if (CURRENT_ITMES_ROW.NO_AM == 1)
                //{
                //    DoCmd.OpenReport("SANAD_DARYAFT_DTL", acPreview, "", "ID =" + this.ID + " and FHES = '" + this.FHES + "'");
                //    if (Forms["BASEKNOW"]["LOCKFAP"])
                //    {
                //        Forms["PGET_HED"]["OKF"] = true;
                //    }
                //}
                //if (this.NO_AM == 2)
                //{
                //    DoCmd.OpenReport("SANAD_PARDAKHT_DTL", acPreview, "", "ID =" + this.ID + " and THES = '" + this.THES + "'");
                //    if (Forms["BASEKNOW"]["LOCKFAP"])
                //    {
                //        Forms["PGET_HED"]["OKF"] = true;
                //    }
                //}
                #endregion

            }

            //نحوه
            if (e.Column.SortMemberPath == "NAHVA")
            {

                var NAHVA_COMBOBOX = (e.EditingElement as ComboBox);
                TextBox NAHVA_COMBOBOX_TEX = (TextBox)NAHVA_COMBOBOX.Template.FindName("PART_EditableTextBox", NAHVA_COMBOBOX);

                if (NAHVA_COMBOBOX.SelectedValue is null)
                {
                    if (!int.TryParse(NAHVA_COMBOBOX_TEX.Text, out int _)) //نوع عددی نیــــست
                    {
                        (e.EditingElement as ComboBox).SelectedValue = WAS_ROW_ITEM.NAHVA;
                    }
                }
                switch (NAHVA_COMBOBOX_TEX.Text)
                {
                    case "1": (e.EditingElement as ComboBox).SelectedValue = 1; break;
                    case "2": (e.EditingElement as ComboBox).SelectedValue = 2; break;
                    case "3": (e.EditingElement as ComboBox).SelectedValue = 3; break;
                    case "4": (e.EditingElement as ComboBox).SelectedValue = 4; break;
                    case "5": (e.EditingElement as ComboBox).SelectedValue = 5; break;
                    case "6": (e.EditingElement as ComboBox).SelectedValue = 6; break;

                    default: break;
                }

                if (Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue) < 1 || Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue) > 6)
                {
                    #region NAHVA_NotInList
                    try
                    {
                        if ((WAS_ROW_ITEM.NAHVA == 4 || WAS_ROW_ITEM.NAHVA == 5 || CURRENT_ITMES_ROW.NAHVA == 4 || CURRENT_ITMES_ROW.NAHVA == 5) && CURRENT_ITMES_ROW.IDH > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                            msgwin.Show();
                            CANCEL = true;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    if (CURRENT_ITMES_ROW.NO_AM == 1)
                    {
                        Msgwin msgwin = new Msgwin(false, "مقدار وارد شده مجاز نيست...");
                        msgwin.Show();
                    }

                    NO_AM_AfterUpdate(row_index);
                    //CURRENT_ITMES_ROW.NAHVA.SetFocus();
                    if (CL_LMethods.IsValidIndex(PGET_LST_SUB, CURRENT_ROW_INDEX))
                    {
                        var TheCol = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAHVA").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(PGET_LST_SUB.Items[CURRENT_ROW_INDEX], PGET_LST_SUB.Columns[TheCol]);
                        var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                        THECELL.Focus();
                    }
                    //Response = acDataErrContinue;
                    #endregion
                }
                var combo = e.Column.GetCellContent(e.Row) as ComboBox;
                if (Convert.ToInt32(combo.SelectedValue) == 4 || Convert.ToInt32(combo.SelectedValue) == 5)
                {
                }
                #region NAHVA_BeforeUpdate
                if ((CURRENT_ITMES_ROW.NAHVA == 4 || CURRENT_ITMES_ROW.NAHVA == 5 || WAS_ROW_ITEM.NAHVA == 4 || WAS_ROW_ITEM.NAHVA == 5) && CURRENT_ITMES_ROW.IDH > 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                    msgwin.Show();
                    // DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "واگذاري يا برگشتي را نمي توانيد اصلاح كنيد بايد سطر آن را بطور كامل حذف كرده و سطر جديد اضافه كنيد.");
                    CANCEL = true;
                }
                #endregion
                CURRENT_ITMES_ROW.NAHVA = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                #region NAHVA_AfterUpdate
                switch (CURRENT_ITMES_ROW.NO_AM)
                {
                    case 1:
                        {
                            switch (CURRENT_ITMES_ROW.NAHVA) // دريافت
                            {
                                case 1:
                                    {
                                        CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(Baseknow.SANDOGH);
                                        if (Strings.Mid(Baseknow.OPTIONSS, 38, 1) == "5")
                                        {
                                            if (!IsNull(DEPATMAN.SelectedValue) && !IsNull(SHIFT.SelectedValue))
                                            {
                                                CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(DEPATMAN.SelectedValue);
                                                CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(SHIFT.SelectedValue);
                                            }
                                            else
                                            {
                                                CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                                CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.THES_M)));
                                            }
                                        }
                                        else
                                        {
                                            CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                            CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.THES_M)));
                                        }
                                        this.tHES_KColumn.IsReadOnly = true;
                                        this.tHES_MColumn.IsReadOnly = true;
                                        this.tHES_TColumn.IsReadOnly = true;
                                        //this.THES_K.TabStop = false;
                                        SetIsTabStopCell("THES_K", false, row_index);
                                        //this.THES_M.TabStop = false;
                                        SetIsTabStopCell("THES_M", false, row_index);
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        CURRENT_ITMES_ROW.THES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_T));
                                        //this.THES.TabStop = false;
                                        SetIsTabStopCell("THES", false, row_index);
                                        this.tHESColumn.IsReadOnly = true;
                                        this.tHES_T2Column = null;
                                        this.tHES_T3Column = null;
                                        this.tHES_T4Column = null;
                                        break;
                                    }
                                case 2:
                                    {
                                        CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                        this.tHES_KColumn.IsReadOnly = true;
                                        this.tHES_MColumn.IsReadOnly = true;
                                        this.tHES_TColumn.IsReadOnly = true;
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        //this.THES_K.TabStop = false;
                                        SetIsTabStopCell("THES_K", false, row_index);
                                        //this.THES_M.TabStop = false;
                                        SetIsTabStopCell("THES_M", false, row_index);
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        CURRENT_ITMES_ROW.THES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_T));
                                        //this.THES.TabStop = false;
                                        SetIsTabStopCell("THES", false, row_index);
                                        this.tHESColumn.IsReadOnly = true;
                                        this.tHES_T2Column = null;
                                        this.tHES_T3Column = null;
                                        this.tHES_T4Column = null;
                                        break;
                                    }
                                case 6:
                                    {
                                        CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADV));
                                        CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADV));
                                        CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADV));
                                        this.tHES_KColumn.IsReadOnly = true;
                                        this.tHES_MColumn.IsReadOnly = true;
                                        this.tHES_TColumn.IsReadOnly = true;
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        //this.THES_K.TabStop = false;
                                        SetIsTabStopCell("THES_K", false, row_index);
                                        //this.THES_M.TabStop = false;
                                        SetIsTabStopCell("THES_M", false, row_index);
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        CURRENT_ITMES_ROW.THES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_T));
                                        //this.THES.TabStop = false;
                                        SetIsTabStopCell("THES", false, row_index);
                                        this.tHESColumn.IsReadOnly = true;
                                        this.tHES_T2Column = null;
                                        this.tHES_T3Column = null;
                                        this.tHES_T4Column = null;
                                        break;
                                    }
                                case 5:
                                    {
                                        CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APA));
                                        CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APA));
                                        CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APA));
                                        this.tHES_KColumn.IsReadOnly = true;
                                        this.tHES_MColumn.IsReadOnly = true;
                                        this.tHES_TColumn.IsReadOnly = true;
                                        //this.THES_K.TabStop = false;
                                        SetIsTabStopCell("THES_K", false, row_index);
                                        //this.THES_M.TabStop = false;
                                        SetIsTabStopCell("THES_M", false, row_index);
                                        //this.THES_T.TabStop = false;
                                        SetIsTabStopCell("THES_T", false, row_index);
                                        CURRENT_ITMES_ROW.THES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_T));
                                        //this.THES.TabStop = false;
                                        SetIsTabStopCell("THES", false, row_index);
                                        this.tHESColumn.IsReadOnly = true;
                                        this.tHES_T2Column = null;
                                        this.tHES_T3Column = null;
                                        this.tHES_T4Column = null;
                                        break;
                                    }
                                case 4:
                                    {
                                        Msgwin msgwin = new Msgwin(false, "مقدار وارده مجاز نيست");
                                        msgwin.Show();
                                        // DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "مقدار وارده مجاز نيست");
                                        CURRENT_ITMES_ROW.NAHVA = null;
                                        break;
                                    }

                                default:
                                    {
                                        this.tHES_KColumn.IsReadOnly = false;
                                        this.tHES_TColumn.IsReadOnly = false;
                                        this.tHES_MColumn.IsReadOnly = false;
                                        //this.THES_T.TabStop = true;
                                        SetIsTabStopCell("THES_T", true, row_index);
                                        //this.THES_K.TabStop = true;
                                        SetIsTabStopCell("THES_K", true, row_index);
                                        //this.THES_M.TabStop = true;
                                        SetIsTabStopCell("THES_M", true, row_index);
                                        //this.THES.TabStop = true;
                                        SetIsTabStopCell("THES", true, row_index);
                                        this.tHESColumn.IsReadOnly = false;
                                        break;
                                    }
                            }
                            this.fHES_KColumn.IsReadOnly = false;
                            this.fHES_MColumn.IsReadOnly = false;
                            //this.FHES_T.TabStop = true;
                            SetIsTabStopCell("FHES_T", true, row_index);
                            this.fHES_TColumn.IsReadOnly = false;
                            //this.FHES_K.TabStop = true;
                            SetIsTabStopCell("FHES_K", true, row_index);
                            //this.FHES_M.TabStop = true;
                            SetIsTabStopCell("FHES_M", true, row_index);
                            //this.FHES.TabStop = true;
                            SetIsTabStopCell("FHES", true, row_index);
                            this.FHES_COLUMN.IsReadOnly = false;

                            break;
                        }
                    case 2:
                        {
                            switch (CURRENT_ITMES_ROW.NAHVA) // چک
                            {
                                case 1:
                                    {
                                        CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(Baseknow.SANDOGH);
                                        if (Strings.Mid(Baseknow.OPTIONSS, 38, 1) == "5")
                                        {
                                            if (!IsNull(DEPATMAN.SelectedValue) && !IsNull(SHIFT.SelectedValue))
                                            {
                                                CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(DEPATMAN.SelectedValue);
                                                CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(SHIFT.SelectedValue);
                                            }
                                            else
                                            {
                                                CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                                CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.FHES_M)));
                                            }
                                        }
                                        else
                                        {
                                            CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                            CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.FHES_M)));
                                        }
                                        this.fHES_KColumn.IsReadOnly = true;
                                        this.fHES_MColumn.IsReadOnly = true;
                                        //this.FHES_T.TabStop = false;
                                        SetIsTabStopCell("FHES_T", false, row_index);
                                        this.fHES_TColumn.IsReadOnly = true;
                                        //this.FHES_K.TabStop = false;
                                        SetIsTabStopCell("FHES_K", false, row_index);
                                        //this.FHES_M.TabStop = false;
                                        SetIsTabStopCell("FHES_M", false, row_index);
                                        CURRENT_ITMES_ROW.FHES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_T));
                                        //this.FHES.TabStop = false;
                                        SetIsTabStopCell("FHES", false, row_index);
                                        this.FHES_COLUMN.IsReadOnly = true;
                                        this.fHES_T2Column = null;
                                        this.fHES_T3Column = null;
                                        this.fHES_T4Column = null;
                                        break;
                                    }
                                case 2:
                                    {
                                        CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APA));
                                        CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APA));
                                        CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APA));
                                        this.fHES_KColumn.IsReadOnly = true;
                                        this.fHES_MColumn.IsReadOnly = true;
                                        //this.FHES_K.TabStop = false;
                                        SetIsTabStopCell("FHES_K", false, row_index);
                                        this.fHES_TColumn.IsReadOnly = true;
                                        //this.FHES_T.TabStop = false;
                                        SetIsTabStopCell("FHES_T", false, row_index);
                                        //this.FHES_M.TabStop = false;
                                        SetIsTabStopCell("FHES_M", false, row_index);
                                        CURRENT_ITMES_ROW.FHES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_T));
                                        //this.FHES.TabStop = false;
                                        SetIsTabStopCell("FHES", false, row_index);
                                        this.FHES_COLUMN.IsReadOnly = true;
                                        this.fHES_T2Column = null;
                                        this.fHES_T3Column = null;
                                        this.fHES_T4Column = null;
                                        break;
                                    }
                                case 6:
                                    {
                                        CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APV));
                                        CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APV));
                                        CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APV));
                                        this.fHES_KColumn.IsReadOnly = true;
                                        this.fHES_MColumn.IsReadOnly = true;
                                        //this.FHES_K.TabStop = false;
                                        SetIsTabStopCell("FHES_K", false, row_index);
                                        this.fHES_TColumn.IsReadOnly = true;
                                        //this.FHES_T.TabStop = false;
                                        SetIsTabStopCell("FHES_T", false, row_index);
                                        //this.FHES_M.TabStop = false;
                                        SetIsTabStopCell("FHES_M", false, row_index);
                                        CURRENT_ITMES_ROW.FHES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_T));
                                        //this.FHES.TabStop = false;
                                        SetIsTabStopCell("FHES", false, row_index);
                                        this.FHES_COLUMN.IsReadOnly = true;
                                        this.fHES_T2Column = null;
                                        this.fHES_T3Column = null;
                                        this.fHES_T4Column = null;
                                        break;
                                    }
                                case 4:
                                    {
                                        CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                        this.fHES_KColumn.IsReadOnly = true;
                                        this.fHES_MColumn.IsReadOnly = true;
                                        this.fHES_TColumn.IsReadOnly = true;
                                        //this.FHES_K.TabStop = false;
                                        SetIsTabStopCell("FHES_K", false, row_index);
                                        //this.FHES_T.TabStop = false;
                                        SetIsTabStopCell("FHES_T", false, row_index);
                                        //this.FHES_M.TabStop = false;
                                        SetIsTabStopCell("FHES_M", false, row_index);
                                        CURRENT_ITMES_ROW.FHES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_T));
                                        //this.FHES.TabStop = false;
                                        SetIsTabStopCell("FHES", false, row_index);
                                        this.FHES_COLUMN.IsReadOnly = true;
                                        this.fHES_T2Column = null;
                                        this.fHES_T3Column = null;
                                        this.fHES_T4Column = null;
                                        break;
                                    }
                                case 5:
                                    {
                                        CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                        CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                        this.fHES_KColumn.IsReadOnly = true;
                                        this.fHES_MColumn.IsReadOnly = true;
                                        this.fHES_TColumn.IsReadOnly = true;
                                        //this.FHES_K.TabStop = false;
                                        SetIsTabStopCell("FHES_K", false, row_index);
                                        //this.FHES_T.TabStop = false;
                                        SetIsTabStopCell("FHES_T", false, row_index);
                                        //this.FHES_M.TabStop = false;
                                        SetIsTabStopCell("FHES_M", false, row_index);
                                        CURRENT_ITMES_ROW.FHES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.FHES_T));
                                        //this.FHES.TabStop = false;
                                        SetIsTabStopCell("FHES", false, row_index);
                                        this.FHES_COLUMN.IsReadOnly = true;
                                        this.fHES_T2Column = null;
                                        this.fHES_T3Column = null;
                                        this.fHES_T4Column = null;
                                        break;
                                    }

                                default:
                                    {
                                        this.fHES_KColumn.IsReadOnly = false;
                                        this.fHES_MColumn.IsReadOnly = false;
                                        //this.FHES_K.TabStop = true;
                                        SetIsTabStopCell("FHES_K", true, row_index);
                                        this.fHES_TColumn.IsReadOnly = false;
                                        //this.FHES_T.TabStop = true;
                                        SetIsTabStopCell("FHES_T", true, row_index);
                                        //this.FHES_M.TabStop = true;
                                        SetIsTabStopCell("FHES_M", true, row_index);
                                        //this.FHES.TabStop = true;
                                        SetIsTabStopCell("FHES", true, row_index);
                                        this.FHES_COLUMN.IsReadOnly = false;
                                        break;
                                    }
                            }
                            this.tHES_KColumn.IsReadOnly = false;
                            this.tHES_MColumn.IsReadOnly = false;
                            //this.THES_K.TabStop = true;
                            SetIsTabStopCell("THES_K", true, row_index);
                            this.tHES_TColumn.IsReadOnly = false;
                            //this.THES_T.TabStop = true;
                            SetIsTabStopCell("THES_T", true, row_index);
                            //this.THES_M.TabStop = true;
                            SetIsTabStopCell("THES_M", true, row_index);
                            //this.THES.TabStop = true;
                            SetIsTabStopCell("THES", true, row_index);
                            this.tHESColumn.IsReadOnly = false;
                            //return;
                            break;
                        }

                }
                #endregion

                //اگر سطر از قبل ثبت شده بود برش گردون به حال قبلیش
                if ((e.EditingElement as ComboBox).SelectedValue is null)
                {
                    (e.EditingElement as ComboBox).SelectedValue = WAS_ROW_ITEM.NAHVA;
                }
                #region NAHVA_OnDClick
                //Check Matter
                //DoCmd.OpenForm("CREATE_CHEKDP", default, default, default, default, acDialog);
                #endregion

                #region IS_TAB_STOPS
                var CDI = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "THES").DisplayIndex;
                var DCI = new DataGridCellInfo(CURRENT_ROW_INDEX, PGET_LST_SUB.Columns[CDI]);
                var The_Cell = CL_LMethods.GetCell(PGET_LST_SUB, CURRENT_ROW_INDEX, CDI);

                if (CURRENT_ITMES_ROW.NAHVA == 2 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "THES"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                if (CURRENT_ITMES_ROW.NAHVA == 4 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "THES"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                if (CURRENT_ITMES_ROW.NAHVA == 5 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "THES"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                if (CURRENT_ITMES_ROW.NAHVA == 6 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "THES"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                if (CURRENT_ITMES_ROW.NAHVA == 1 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "THES"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                #endregion
            }

            //از حساب
            if (e.Column.SortMemberPath == "FHES")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe().Trim()))
                {
                    universControl.PopNotifyShow("فیلد از حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                double? KOL = null, MOIN = null, TAF = null, TAF2 = null, TAF3 = null, TAF4 = null;

                if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("PGET_HED", I_AM_KHAZANEH);//Search Plusy Form Specialy for Customers
                    await ShowDialogAfterCurrentDispatcherOperationAsync(CMBSearch);

                    //string?[] HESAB_SPLITED = null;
                    if (FROM_SEARCH.HES is not null)
                    {
                        CURRENT_ITMES_ROW.FHES = FROM_SEARCH.HES;
                        CURRENT_ITMES_ROW.NAME_FHES = FROM_SEARCH.NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.FHES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.FHES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.FHES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.FHES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.FHES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    else
                    {
                        CURRENT_ITMES_ROW.FHES = null;
                        CURRENT_ITMES_ROW.NAME_FHES = null;

                        CURRENT_ITMES_ROW.FHES_T2 = null; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T3 = null; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T4 = null; //تفضیلی2

                        universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                    }
                    FROM_SEARCH.HES = null;
                    FROM_SEARCH.NAME = null;

                }
                else
                {
                    var RES_HESAB = dbms.DoGetDataSQL<QueryT2>("SELECT TOP(1) NAME,hes FROM dbo.CUST_HESAB WHERE hes = @hes", new { hes = ENTERED_VALUE_ROW.ToStringNullSafe().Trim() }).ToList();

                    if (RES_HESAB.Count > 0)
                    {
                        CURRENT_ITMES_ROW.FHES = RES_HESAB.FirstOrDefault().hes;
                        CURRENT_ITMES_ROW.NAME_FHES = RES_HESAB.FirstOrDefault().NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.FHES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.FHES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.FHES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.FHES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.FHES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    //جستجو متن در حساب ها________________________________*******___________________________________________________________
                    else
                    {
                        //لسن جستجو رو نمایش و بعد از انتخاب کزینه , پراپرتی های زیر رو پر میکنه که میشه بررسی کرد آیا چیزی پر شده یا نه ؟
                        CL_HESAB_SEARCH.Go_Search_Hesab(ENTERED_VALUE_ROW.ToString(), "PGET_HED", I_AM_KHAZANEH);

                        //string?[] _HESAB_SPLITED_ = null;
                        if (FROM_SEARCH.HES is not null)
                        {
                            CURRENT_ITMES_ROW.FHES = FROM_SEARCH.HES;
                            CURRENT_ITMES_ROW.NAME_FHES = FROM_SEARCH.NAME;

                            CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.FHES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                            CURRENT_ITMES_ROW.FHES_K = (int?)KOL; //کل
                            CURRENT_ITMES_ROW.FHES_M = (int?)MOIN; //معین
                            CURRENT_ITMES_ROW.FHES_T = (int?)TAF; //تفضیلی

                            CURRENT_ITMES_ROW.FHES_T2 = (int?)TAF2; //تفضیلی2
                            CURRENT_ITMES_ROW.FHES_T3 = (int?)TAF3; //تفضیلی2
                            CURRENT_ITMES_ROW.FHES_T4 = (int?)TAF4; //تفضیلی2

                        }
                        else
                        {
                            CURRENT_ITMES_ROW.FHES = null;
                            CURRENT_ITMES_ROW.NAME_FHES = null;

                            CURRENT_ITMES_ROW.FHES_T2 = null; //تفضیلی2
                            CURRENT_ITMES_ROW.FHES_T3 = null; //تفضیلی2
                            CURRENT_ITMES_ROW.FHES_T4 = null; //تفضیلی2

                            universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                        }
                        FROM_SEARCH.HES = null;
                        FROM_SEARCH.NAME = null;

                    }

                }

                if (CL_HESABDARI.ISTAF(CURRENT_ITMES_ROW.FHES))
                {
                    universControl.PopNotifyShow("حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ITMES_ROW.FHES = WAS_ROW_ITEM?.FHES;
                    RestoreFocusCell(e);
                    return;
                }

                if (CURRENT_ITMES_ROW.NAHVA == 5 && CURRENT_ITMES_ROW.NO_AM == 1)
                {
                    if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                    {
                        CURRENT_ITMES_ROW.N_SERI = 0;
                        CURRENT_ITMES_ROW.BANK = 0;
                    }
                    string _ServerFilter = null;
                    if (CURRENT_ITMES_ROW.N_SERI is not null && CURRENT_ITMES_ROW.N_SERI > 0)
                    {
                        _ServerFilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                    }
                    if (CURRENT_ITMES_ROW.FHES is not null && PGET_LST_SUB.SelectedItem != null)
                    {
                        BAKCHEKP bAKCHEKP = new BAKCHEKP(I_AM_KHAZANEH, _ServerFilter, CURRENT_ROW_INDEX);
                        await ShowDialogAfterCurrentDispatcherOperationAsync(bAKCHEKP);
                    }
                }
                #region IS_TAB_STOPS
                var CDI = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "THES").DisplayIndex;
                var DCI = new DataGridCellInfo(CURRENT_ROW_INDEX, PGET_LST_SUB.Columns[CDI]);
                var The_Cell = CL_LMethods.GetCell(PGET_LST_SUB, CURRENT_ROW_INDEX, CDI);
                if (CURRENT_ITMES_ROW.NAHVA == 2 && CURRENT_ITMES_ROW.NO_AM == 1)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "MABL"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                #endregion

                //بررسی وجود حساب انتخاب شده :
                var TheHesab = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = N'{CURRENT_ITMES_ROW.THES}'").FirstOrDefault();
                if (string.IsNullOrEmpty(TheHesab))
                {
                    PGET_HED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    universControl.PopNotifyShow($"حساب متناظر برای قسمت \"از حساب\" , وارد شده {CURRENT_ITMES_ROW.THES} , در سیستم وجود ندارد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B", 4);
                    return;
                }


            }

            //به حساب
            if (e.Column.SortMemberPath == "THES")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe().Trim()))
                {
                    universControl.PopNotifyShow("فیلد به حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                double? KOL = null, MOIN = null, TAF = null, TAF2 = null, TAF3 = null, TAF4 = null;
                if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                {
                    ComboSearch CMBSearch = new ComboSearch("PGET_HED", I_AM_KHAZANEH);//Search Plusy Form Specialy for Customers
                    await ShowDialogAfterCurrentDispatcherOperationAsync(CMBSearch);

                    if (FROM_SEARCH.HES is not null)
                    {
                        CURRENT_ITMES_ROW.THES = FROM_SEARCH.HES;
                        CURRENT_ITMES_ROW.NAME_THES = FROM_SEARCH.NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.THES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.THES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.THES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.THES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.THES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    else
                    {
                        CURRENT_ITMES_ROW.THES = null;
                        CURRENT_ITMES_ROW.NAME_THES = null;

                        CURRENT_ITMES_ROW.THES_T2 = null; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T3 = null; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T4 = null; //تفضیلی2

                        universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                    }
                    FROM_SEARCH.HES = null;
                    FROM_SEARCH.NAME = null;
                }
                else
                {
                    var RES_HESAB = dbms.DoGetDataSQL<QueryT2>("SELECT TOP(1) NAME,hes FROM dbo.CUST_HESAB WHERE hes = @hes", new { hes = ENTERED_VALUE_ROW.ToStringNullSafe().Trim() }).ToList();
                    if (RES_HESAB.Count > 0)
                    {
                        CURRENT_ITMES_ROW.THES = RES_HESAB.FirstOrDefault().hes;
                        CURRENT_ITMES_ROW.NAME_THES = RES_HESAB.FirstOrDefault().NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.THES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.THES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.THES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.THES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.THES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    else
                    {
                        /////////////////////////////////////////////////////////////////////////////////////////
                        //جستجو متن در حساب ها________________________________*******___________________________________________________________
                        //لسن جستجو رو نمایش و بعد از انتخاب کزینه , پراپرتی های زیر رو پر میکنه که میشه بررسی کرد آیا چیزی پر شده یا نه ؟
                        CL_HESAB_SEARCH.Go_Search_Hesab(ENTERED_VALUE_ROW.ToString(), "PGET_HED", I_AM_KHAZANEH);

                        if (FROM_SEARCH.HES is not null)
                        {
                            CURRENT_ITMES_ROW.THES = FROM_SEARCH.HES;
                            CURRENT_ITMES_ROW.NAME_THES = FROM_SEARCH.NAME;

                            CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.THES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                            CURRENT_ITMES_ROW.THES_K = (int?)KOL; //کل
                            CURRENT_ITMES_ROW.THES_M = (int?)MOIN; //معین
                            CURRENT_ITMES_ROW.THES_T = (int?)TAF; //تفضیلی

                            CURRENT_ITMES_ROW.THES_T2 = (int?)TAF2; //تفضیلی2
                            CURRENT_ITMES_ROW.THES_T3 = (int?)TAF3; //تفضیلی2
                            CURRENT_ITMES_ROW.THES_T4 = (int?)TAF4; //تفضیلی2
                        }
                        else
                        {
                            CURRENT_ITMES_ROW.THES = null;
                            CURRENT_ITMES_ROW.NAME_THES = null;

                            CURRENT_ITMES_ROW.THES_T2 = null; //تفضیلی2
                            CURRENT_ITMES_ROW.THES_T3 = null; //تفضیلی2
                            CURRENT_ITMES_ROW.THES_T4 = null; //تفضیلی2

                            universControl.PopNotifyShow("چنین حسابی وجود ندارد.", Pop1, Pop1Text1, Pop_Border1);
                        }
                        FROM_SEARCH.HES = null;
                        FROM_SEARCH.NAME = null;
                    }
                }

                if (PGET_LST_SUB.SelectedItem == null)
                {
                    return;
                }

                if (CL_HESABDARI.ISTAF(CURRENT_ITMES_ROW.THES))
                {
                    universControl.PopNotifyShow("حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!", Pop1, Pop1Text1, Pop_Border1);
                    CURRENT_ITMES_ROW.THES = WAS_ROW_ITEM?.THES;
                    RestoreFocusCell(e);
                    return;
                }

                if (CURRENT_ITMES_ROW.NAHVA == 5 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                    {
                        CURRENT_ITMES_ROW.N_SERI = 0;
                        CURRENT_ITMES_ROW.BANK = 0;
                    }
                    string _ServerFilter = null;
                    if (CURRENT_ITMES_ROW.N_SERI is not null && CURRENT_ITMES_ROW.N_SERI > 0)
                    {
                        _ServerFilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                    }
                    if (CURRENT_ITMES_ROW.THES is not null && PGET_LST_SUB.SelectedItem != null)
                    {
                        BAKCHEK bAKCHEK = new BAKCHEK(I_AM_KHAZANEH, _ServerFilter, CURRENT_ROW_INDEX);
                        await ShowDialogAfterCurrentDispatcherOperationAsync(bAKCHEK);
                    }
                }

                if (CURRENT_ITMES_ROW.NAHVA == 4)
                {
                    if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                    {
                        CURRENT_ITMES_ROW.N_SERI = 0;
                        CURRENT_ITMES_ROW.BANK = 0;
                    }
                    string _ServerFilter = null;
                    if (CURRENT_ITMES_ROW.N_SERI is not null && CURRENT_ITMES_ROW.N_SERI > 0)
                    {
                        _ServerFilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                    }
                    if (CURRENT_ITMES_ROW.THES is not null && PGET_LST_SUB.SelectedItem != null)
                    {

                        FORCHEK fORCHEK = new FORCHEK(I_AM_KHAZANEH, _ServerFilter, CURRENT_ROW_INDEX);
                        await ShowDialogAfterCurrentDispatcherOperationAsync(fORCHEK);

                        JustnowforcheckOpnned = true;
                    }
                }

                #region IS_TAB_STOPS
                var CDI = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "THES").DisplayIndex;
                var DCI = new DataGridCellInfo(CURRENT_ROW_INDEX, PGET_LST_SUB.Columns[CDI]);
                var The_Cell = CL_LMethods.GetCell(PGET_LST_SUB, CURRENT_ROW_INDEX, CDI);
                if (CURRENT_ITMES_ROW.NAHVA == 2 && CURRENT_ITMES_ROW.NO_AM == 2)
                {
                    if (!(The_Cell is null))
                    {
                        FocusCell(CURRENT_ROW_INDEX, "MABL"); // برای اینکه از روی یک سلول بره سلول بعدی 
                    }
                }
                #endregion

            }

            //مبلغ
            if (e.Column.SortMemberPath == "MABL")
            {
                ENTERED_VALUE_ROW = ENTERED_VALUE_ROW.ToString().RemoveQut();
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !long.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    RestoreFocusCell(e);
                    universControl.PopNotifyShow("مبلغ صحیح وارد نشده", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                //بررسی وجود حساب انتخاب شده :
                var TheHesab = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = N'{CURRENT_ITMES_ROW.THES}'").FirstOrDefault();
                if (string.IsNullOrEmpty(TheHesab))
                {
                    universControl.PopNotifyShow($"حساب متناظر برای قسمت \"به حساب\" , وارد شده {CURRENT_ITMES_ROW.THES} , در سیستم وجود ندارد !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B", 4);
                    return;
                }

                if (PGET_LST_SUB.SelectedItem == null)
                {
                    return;
                }

                CURRENT_ITMES_ROW.MABL = Convert.ToDouble(ENTERED_VALUE_ROW);

                switch (CURRENT_ITMES_ROW.NO_AM)
                {
                    case 1:
                        {
                            switch (CURRENT_ITMES_ROW.NAHVA) // دريافت
                            {
                                case 1:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        else
                                        {
                                            if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = 0;
                                                CURRENT_ITMES_ROW.BANK = 0;
                                            }
                                            if (CURRENT_ITMES_ROW?.ID is null || CURRENT_ITMES_ROW?.ID <= 1)
                                            {
                                                if (Exit_Request())
                                                {
                                                    PGET_HED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                                    return;
                                                }
                                            }
                                            GETCHEK gETCHEK = new GETCHEK(I_AM_KHAZANEH, CURRENT_ITMES_ROW.MABL.ToString(), CURRENT_ROW_INDEX, default, WAS_ROW_ITEM?.MABL);
                                            await ShowDialogAfterCurrentDispatcherOperationAsync(gETCHEK);
                                            if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                            }
                                        }

                                        break;
                                    }
                                case 6:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        else
                                        {
                                            if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = 0;
                                                CURRENT_ITMES_ROW.BANK = 0;
                                            }
                                            GETCHEK gETCHEK = new GETCHEK(I_AM_KHAZANEH, CURRENT_ITMES_ROW.MABL.ToString(), CURRENT_ROW_INDEX);
                                            await ShowDialogAfterCurrentDispatcherOperationAsync(gETCHEK);

                                            if (CURRENT_CELL_ROW != null)
                                            {
                                                CURRENT_CELL_ROW.Focus();
                                            }
                                            if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                        {
                                            CURRENT_ITMES_ROW.N_SERI = 0;
                                            CURRENT_ITMES_ROW.BANK = 0;
                                        }
                                        if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0 || IsNull(CURRENT_ITMES_ROW.N_SERI))
                                        {
                                            CURRENT_ITMES_ROW.N_SERI = null;
                                            CURRENT_ITMES_ROW.BANK = null;
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case 2:
                        {
                            switch (CURRENT_ITMES_ROW.NAHVA) // پرداخت
                            {
                                case 1:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        else
                                        {
                                            if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = 0;
                                                CURRENT_ITMES_ROW.BANK = 0;
                                            }
                                            var _serverfilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                                            PAYCHEK pAYCHEK = new PAYCHEK(_serverfilter, I_AM_KHAZANEH, CURRENT_ITMES_ROW.MABL.ToString(), CURRENT_ROW_INDEX, default, WAS_ROW_ITEM?.MABL);
                                            await ShowDialogAfterCurrentDispatcherOperationAsync(pAYCHEK);
                                            if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                            }
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                            CANCEL = true;
                                        }
                                        else
                                        {
                                            if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = 0;
                                                CURRENT_ITMES_ROW.BANK = 0;
                                            }
                                            var _serverfilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                                            PAYCHEK pAYCHEK = new PAYCHEK(_serverfilter, I_AM_KHAZANEH, CURRENT_ITMES_ROW.MABL.ToString(), CURRENT_ROW_INDEX);
                                            if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0)
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                            }
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        if (JustnowforcheckOpnned)
                                        {
                                            //Get out cuz it has been already open
                                            JustnowforcheckOpnned = false;
                                        }
                                        else
                                        {
                                            if (IsNull(CURRENT_ITMES_ROW?.N_SERI) || IsNull(CURRENT_ITMES_ROW?.BANK))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = 0;
                                                CURRENT_ITMES_ROW.BANK = 0;
                                            }
                                            if (Convert.ToString(CURRENT_ITMES_ROW.N_SERI) == "" || Convert.ToString(CURRENT_ITMES_ROW.BANK) == "" || Convert.ToString(CURRENT_ITMES_ROW.MABL) == "")
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                                CURRENT_ITMES_ROW.MABL = null;
                                            }
                                            var _serverfilter = "N_SERI = " + CURRENT_ITMES_ROW.N_SERI + " AND BANK = " + CURRENT_ITMES_ROW.BANK + " AND MABL = " + CURRENT_ITMES_ROW.MABL;
                                            FORCHEK fORCHEK4 = new FORCHEK(I_AM_KHAZANEH, _serverfilter, CURRENT_ROW_INDEX);
                                            await ShowDialogAfterCurrentDispatcherOperationAsync(fORCHEK4);
                                            if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0 || IsNull(CURRENT_ITMES_ROW.N_SERI))
                                            {
                                                CURRENT_ITMES_ROW.N_SERI = null;
                                                CURRENT_ITMES_ROW.BANK = null;
                                            }
                                        }
                                        break;
                                    }
                                case 5:
                                    {
                                        if (IsNull(CURRENT_ITMES_ROW.N_SERI) || IsNull(CURRENT_ITMES_ROW.BANK))
                                        {
                                            CURRENT_ITMES_ROW.N_SERI = 0;
                                            CURRENT_ITMES_ROW.BANK = 0;
                                        }
                                        if (CURRENT_ITMES_ROW.N_SERI == 0 || CURRENT_ITMES_ROW.BANK == 0 || IsNull(CURRENT_ITMES_ROW.N_SERI))
                                        {
                                            CURRENT_ITMES_ROW.N_SERI = null;
                                            CURRENT_ITMES_ROW.BANK = null;
                                            PGET_HED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        if (CURRENT_ITMES_ROW.MABL == 0 || IsNull(CURRENT_ITMES_ROW.MABL))
                                        {
                                            Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                                            msgwin.Show();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }

                //if (!BodyIsValid(CURRENT_ITMES_ROW))
                //{
                //    RestoreFocusCell(e);
                //}
            }

            //مرکز هزینه
            if (e.Column.SortMemberPath == "MHAZ_NO")
            {
                var MHAZ_NO_COMBOBOX = (e.EditingElement as ComboBox);
                if (MHAZ_NO_COMBOBOX?.SelectedValue is not null)
                {
                    CURRENT_ITMES_ROW.MHAZ_NO = Convert.ToInt32(MHAZ_NO_COMBOBOX.SelectedValue);
                }
                else
                {
                    CURRENT_ITMES_ROW.MHAZ_NO = null;
                }
            }

            //شرح
            if (e.Column.SortMemberPath == "SHARH")
            {

            }

        }

        private void RestoreFocusCell(DataGridCellEditEndingEventArgs e)
        {
            try
            {
                e.Cancel = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PGET_LST_SUB.CurrentCell = _editingCellInfo.Value;
                    PGET_LST_SUB.BeginEdit();
                    if (e.EditingElement is TextBox tb)
                    {
                        tb.SelectAll();
                        tb.Focus();
                        Keyboard.Focus(tb);
                    }
                }));
                //}), DispatcherPriority.Background);
            }
            catch (Exception)
            {
            }
        }

        bool IsSaveSuccess = true;
        private void PGET_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null) { return; }
            var ROW = e.Row.Item as PGET_LST;
            if (ConstructorRowDetector.IsPristine(ROW)) { PGET_HED_SUB_CANCEL_EDIT(); return; }
            if (ROW is null) { return; }

            IsSaveSuccess = false;
            JustnowforcheckOpnned = false;


            //Form_BeforeUpdate
            PGET_LST? THE_ROW_ITEM = (e.Row.Item as PGET_LST);
            if (e.Row.IsNewItem)
            {
                bool IsReallyNull = false;

                if ((THE_ROW_ITEM?.NO_AM is null || THE_ROW_ITEM.NO_AM == 0) || (THE_ROW_ITEM.NAHVA is null || THE_ROW_ITEM.NAHVA == 0) || THE_ROW_ITEM.MABL is null)
                {
                    IsReallyNull = true;
                }

                if (IsReallyNull == true)
                {
                    //e.Cancel = true;
                    PGET_HED_SUB_CANCEL_EDIT();
                    return;
                }
            }
            if (THE_ROW_ITEM is not null)
            {
                var HaveErrors = (from object i in PGET_LST_SUB.ItemsSource
                                  let c = PGET_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                                  where c != null && Validation.GetHasError(c)
                                  select c).Any();

                if (HaveErrors)
                {
                    //e.Cancel = true;
                    PGET_HED_SUB_CANCEL_EDIT();
                    return;
                }

                if (THE_ROW_ITEM.MABL == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "مبلغ نمي تواند داراي مقدار خالي باشد");
                    msgwin.Show();

                    CANCEL = true;
                    return;
                }
                if (!this.NewRecord && Baseknow.WAR == 1)
                {
                    //Msgwin msgwin = new Msgwin(true, "تغيرات داده شده ثبت شود؟");
                    //msgwin.ShowDialog();
                    //if (msgwin.DialogResult != true)
                    //{
                    //    e.Cancel = true;
                    //    CANCEL = true;
                    //}
                }
                this.MABL.Text = SUM_OF_MABL.ToString();


                if (CmdSaveRecord(THE_ROW_ITEM) is false)
                {
                    //e.Cancel = true;
                    PGET_HED_SUB_CANCEL_EDIT();
                }
                else //Success
                {
                    IsSaveSuccess = true;

                    SANAD();

                    if (!IsPastingRows)
                    {
                        //ReGetData();
                        //RestoreFcousOnDataGrid();
                    }


                    ChangeIsHappend = false;

                    universControl.PopNotifyShow("ذخیره با موفقیت انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C", 1);
                }
            }
        }

        private void RestoreFcousOnDataGrid()
        {
            try
            {
                if (PGET_LST_SUB.SelectedItem != null && CL_LMethods.IsNewPlaceHolder(PGET_LST_SUB, PGET_LST_SUB.SelectedItem))
                {
                    return;
                }

                // Step 1: Focus the DataGrid itself
                PGET_LST_SUB.Focus(); // Replace PGET_LST_SUB with your actual DataGrid name

                if (CURRENT_ROW_INDEX >= 0 && CURRENT_ROW_INDEX < PGET_LST_SUB.Items.Count)
                {
                    // Step 4: Explicitly get the SelectedItem from Items[SelectedIndex] (avoids null if binding lags)
                    object selectedItem = PGET_LST_SUB.Items[CURRENT_ROW_INDEX]; // Safer retrieval
                    if (selectedItem != null && selectedItem is PGET_LST item) // Type check
                    {
                        PGET_LST_SUB.SelectedIndex = CURRENT_ROW_INDEX;
                        PGET_LST_SUB.ScrollIntoView(PGET_LST_SUB.Items[CURRENT_ROW_INDEX]); // Scroll to the item using Items[] to ensure it's retrieved

                        // Update CURRENT_ITMES_ROW if needed
                        CURRENT_ITMES_ROW = item;

                        // Step 5: Focus on the specific cell (e.g., the "MABL" column for مبلغ)
                        var mablColumn = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL");
                        if (mablColumn != null)
                        {
                            var cellInfo = new DataGridCellInfo(selectedItem, mablColumn);
                            PGET_LST_SUB.CurrentCell = cellInfo; // Set current cell
                            Keyboard.Focus(PGET_LST_SUB); // Ensure keyboard focus

                            //Begin editing the cell to mimic entering data mode and force to get update selecteditem
                            PGET_LST_SUB.CellEditEnding -= PGET_LST_SUB_CellEditEnding;
                            PGET_LST_SUB.RowEditEnding -= PGET_LST_SUB_RowEditEnding;

                            PGET_LST_SUB.BeginEdit();
                            PGET_LST_SUB.CancelEdit();

                            PGET_LST_SUB.RowEditEnding += PGET_LST_SUB_RowEditEnding;
                            PGET_LST_SUB.CellEditEnding += PGET_LST_SUB_CellEditEnding;
                        }
                    }
                    else
                    {
                        // Fallback: If still null, just focus the DataGrid without cell selection
                        Keyboard.Focus(PGET_LST_SUB);
                    }
                }
                else
                {
                    if (PGET_LST_SUB.Items.Count > 0)
                    {
                        PGET_LST_SUB.SelectedIndex = 0; // Or PGET_LST_SUB.Items.Count - 1 for last
                        PGET_LST_SUB.ScrollIntoView(PGET_LST_SUB.SelectedItem);
                    }
                    Keyboard.Focus(PGET_LST_SUB);
                }

                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //}), DispatcherPriority.Render);
            }
            catch { }
        }

        public bool IsPastingRows { get; private set; } = false;
        private void PGET_LST_SUB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check if Ctrl key is pressed and the pressed key is double quote
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (PGET_LST_SUB.CurrentCell != null)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = PGET_LST_SUB.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = PGET_LST_SUB.Items.IndexOf(currentCell.Item);
                            int columnIndex = PGET_LST_SUB.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = PGET_LST_SUB.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < PGET_LST_SUB.Columns.Count)
                                {
                                    // Get the column information
                                    var column = PGET_LST_SUB.Columns[columnIndex];

                                    // Ensure that the column has a valid SortMemberPath
                                    if (!string.IsNullOrEmpty(column.SortMemberPath))
                                    {
                                        // Use reflection to get and set the property values
                                        var propertyInfo = valueAbove.GetType().GetProperty(column.SortMemberPath);

                                        // Ensure that the property exists and is not null
                                        if (propertyInfo != null)
                                        {
                                            // Get the value from the above cell
                                            object valueAboveCellValue = propertyInfo.GetValue(valueAbove);

                                            // Cast currentCell.Item to the actual data type
                                            var currentItem = currentCell.Item;

                                            // Use reflection to set the value on the current item
                                            if (currentItem.GetType().GetProperty(column.SortMemberPath) is PropertyInfo currentCellProperty)
                                            {
                                                // Set the value on the current cell's item
                                                currentCellProperty.SetValue(currentItem, valueAboveCellValue);

                                                PGET_LST_SUB.BeginEdit();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        e.Handled = true;
                    }
                }
                catch { }
            }

            #region COPYPASTE
            var isEditing = ((IEditableCollectionView)PGET_LST_SUB.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)PGET_LST_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && PGET_LST_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<PGET_LST>(PGET_LST_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !PGET_LST_SUB.IsReadOnly && PGET_LST_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<PGET_LST>(PGET_LST_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }
            #endregion

            if (PGET_LST_SUB.CurrentColumn is not null)
            {
                if (e.Key == Key.Add)
                {
                    if (PGET_LST_SUB.CurrentColumn.SortMemberPath is "MABL")
                    {
                        e.Handled = true;
                        var text = "000";
                        var target = Keyboard.FocusedElement;
                        var routedEvent = TextCompositionManager.TextInputEvent;

                        target.RaiseEvent(
                            new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, target, text))
                            { RoutedEvent = routedEvent });
                    }
                }
                if (e.Key == Key.Subtract)
                {
                    if (PGET_LST_SUB.CurrentColumn.SortMemberPath is "MABL")
                    {
                        e.Handled = true;
                        var text = "00";
                        var target = Keyboard.FocusedElement;
                        var routedEvent = TextCompositionManager.TextInputEvent;

                        target.RaiseEvent(
                            new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, target, text))
                            { RoutedEvent = routedEvent });
                    }
                }
            }
            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                DELETE_FACTOR22_Click(null, null);
            }
        }
        private void ValidateDataGridRow(DataGridRowEditEndingEventArgs args, PasteValidationResult validationResult)
        {
            // Default to true
            validationResult.IsRowValid = true;

            if (args.Row.Item is PGET_LST item)
            {
                //Reset id to be sure the new data will insert not update the same row existing before
                item.ID = null;
                item.IDH = null;
                item.RADIF = null;
                item.UID = null;
                item.CRT = null;
                CURRENT_ITMES_ROW = item;

                //نوع عملیات
                {
                    var DB_NO_AM = dbms.DoGetDataSQL<TCOD_DPS>($"SELECT CODE, NAMES FROM dbo.TCOD_DPS").ToList();
                    if (CL_LMethods.IsNumeric(item.NO_AM.ToStringNullSafe()))
                    {
                        var DBROW = DB_NO_AM.Where(x => x.CODE.Equals(item.NO_AM)).FirstOrDefault();
                        if (DBROW == null)
                        {
                            args.Cancel = true;
                            validationResult.IsRowValid = false;
                            validationResult.RowMessage = "فیلد نوع عملیات مجاز نیست";
                        }
                    }
                    else
                    {
                        var _Input_NO_AM_ = CL_LMethods.NormalizeArabicPersian(item.NO_AM.ToStringNullSafe().Trim());

                        var DBROW = DB_NO_AM.Where(x => CL_LMethods.NormalizeArabicPersian(x.NAMES).Equals(_Input_NO_AM_)).FirstOrDefault();

                        if (DBROW != null)
                        {
                            CURRENT_ITMES_ROW.NO_AM = DBROW.CODE;
                        }
                        else
                        {
                            args.Cancel = true;
                            validationResult.IsRowValid = false;
                            validationResult.RowMessage = "فیلو نوع عملیات درست نیست";
                        }
                    }
                }

                //نوع نحوه
                {
                    var DBNAHVA = dbms.DoGetDataSQL<TCOD_DPSKIND>($"SELECT CODE, NAMES FROM dbo.TCOD_DPSKIND").ToList();
                    if (CL_LMethods.IsNumeric(item.NAHVA.ToStringNullSafe()))
                    {
                        //case "نقد": CURRENT_ITMES_ROW.NAHVA = 1; break;
                        //case "سایر": CURRENT_ITMES_ROW.NAHVA = 3; break;
                        //اگر عددی است و نوع نقدر و سایر است مجاز است در غیر این صورت غیر مجاز
                        var DBROW = DBNAHVA.Where(x => x.CODE.Equals(item.NAHVA)).FirstOrDefault();
                        if (DBROW == null)
                        {
                            args.Cancel = true;
                            validationResult.IsRowValid = false;
                            validationResult.RowMessage = "فقط سطر هایی با نحوه نقد یا سایر مجاز به انتقال کپی هستند";
                        }
                    }
                    else
                    {
                        var _Input_Nahva_ = CL_LMethods.NormalizeArabicPersian(item.NAHVA.ToStringNullSafe().Trim());

                        var DBROW = DBNAHVA.Where(x => CL_LMethods.NormalizeArabicPersian(x.NAMES).Equals(_Input_Nahva_)).FirstOrDefault();

                        if (DBROW != null)
                        {
                            CURRENT_ITMES_ROW.NAHVA = DBROW.CODE;
                        }
                        else
                        {
                            args.Cancel = true;
                            validationResult.IsRowValid = false;
                            validationResult.RowMessage = "فقط سطر هایی با نحوه نقد یا سایر مجاز به انتقال کپی هستند";
                        }
                    }
                }

                //از حساب FHES
                if (!string.IsNullOrEmpty(item.FHES))
                {
                    double? KOL = null, MOIN = null, TAF = null, TAF2 = null, TAF3 = null, TAF4 = null;

                    var RES_HESAB = dbms.DoGetDataSQL<QueryT2>("SELECT TOP(1) NAME,hes FROM dbo.CUST_HESAB WHERE hes = @hes", new { hes = item.FHES }).ToList();
                    if (RES_HESAB.Count > 0)
                    {
                        CURRENT_ITMES_ROW.FHES = RES_HESAB.FirstOrDefault().hes;
                        CURRENT_ITMES_ROW.NAME_FHES = RES_HESAB.FirstOrDefault().NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.FHES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.FHES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.FHES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.FHES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.FHES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.FHES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    else
                    {
                        args.Cancel = true;
                        validationResult.IsRowValid = false;
                        validationResult.RowMessage = "فیلد از حساب وارد شده در سیستم موجود نیست";
                    }
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "فیلد از حساب نمیتواند خالی باشد";
                }

                //به حساب THES
                if (!string.IsNullOrEmpty(item.THES))
                {
                    double? KOL = null, MOIN = null, TAF = null, TAF2 = null, TAF3 = null, TAF4 = null;

                    var RES_HESAB = dbms.DoGetDataSQL<QueryT2>("SELECT TOP(1) NAME,hes FROM dbo.CUST_HESAB WHERE hes = @hes", new { hes = item.THES }).ToList();
                    if (RES_HESAB.Count > 0)
                    {
                        CURRENT_ITMES_ROW.THES = RES_HESAB.FirstOrDefault().hes;
                        CURRENT_ITMES_ROW.NAME_THES = RES_HESAB.FirstOrDefault().NAME;

                        CL_HESABDARI.GETTAF3(CURRENT_ITMES_ROW.THES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

                        CURRENT_ITMES_ROW.THES_K = (int?)KOL; //کل
                        CURRENT_ITMES_ROW.THES_M = (int?)MOIN; //معین
                        CURRENT_ITMES_ROW.THES_T = (int?)TAF; //تفضیلی

                        CURRENT_ITMES_ROW.THES_T2 = (int?)TAF2; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T3 = (int?)TAF3; //تفضیلی2
                        CURRENT_ITMES_ROW.THES_T4 = (int?)TAF4; //تفضیلی2
                    }
                    else
                    {
                        args.Cancel = true;
                        validationResult.IsRowValid = false;
                        validationResult.RowMessage = "فیلد به حساب وارد شده در سیستم موجود نیست";
                    }
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "فیلد به حساب نمیتواند خالی باشد";
                }

                //شرح
                if (item?.SHARH?.Length > 255)
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "تعداد کاراکتر (255) وارد شده برای شرح بیش از حد مجاز است";
                }

                //مبلغ
                var _MABL_ = item.MABL.ToStringNullSafe();
                if (!string.IsNullOrEmpty(_MABL_))
                {
                    var MABL_NUMBER = NumberExtractor.ExtractNumbersLine(_MABL_);
                    if (!string.IsNullOrEmpty(MABL_NUMBER))
                    {
                        item.MABL = Convert.ToDouble(MABL_NUMBER);
                    }
                    else
                    {
                        args.Cancel = true;
                        validationResult.IsRowValid = false;
                        validationResult.RowMessage = "فیلد مبغ وارد شده صحیح نیست";
                    }
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "فیلد مبغ نمیتواند خالی باشد";
                }

                //Final Validation
                if (validationResult.IsRowValid) //Yet
                {
                    PGET_LST_SUB_RowEditEnding(PGET_LST_SUB, args);
                    validationResult.IsRowValid = IsSaveSuccess;
                }
            }
            else
            {
                // If the item is not of type CUSTOM_MODEL, invalidate the row
                args.Cancel = true;
                validationResult.IsRowValid = false;
            }
        }
        private void AddItemToDataSource(PGET_LST item)
        {
            // Ensure thread safety if MY_ALL_DATA is accessed from multiple threads
            Application.Current.Dispatcher.Invoke(() =>
            {
                KHAZANEH_DATA.Add(item);
            });
        }

        public bool BodyIsValid(PGET_LST final_lst, bool _DisplayMsg_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            //Validation Checks...
            #region VALIDATION
            //HEAD_VALID_CHECK 
            //SUB_VALID_CHECK
            if (final_lst.NO_AM is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع عملیات خالی است." });
            }
            if (final_lst.NAHVA is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نحوه خالی است." });
            }
            if (final_lst.SHARH is not null)
            {
                if (final_lst.SHARH.Length > 255)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شرح عملیات بیش از اندازه مجاز است." });
                }
            }

            if (final_lst.MABL < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح نیست !" });
            }
            if (final_lst.FHES is null || final_lst.FHES == "")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فیلد از حساب خالی است." });
            }
            else
            {
                var TheHesab = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = N'{final_lst.FHES}'").FirstOrDefault();
                if (string.IsNullOrEmpty(TheHesab))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"حساب متناظر برای قسمت \"از حساب\" , وارد شده [{final_lst.FHES}] , در سیستم وجود ندارد !" });
                }
            }
            if (final_lst.THES is null || final_lst.THES == "")
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فیلد به حساب خالی است." });
            }
            else
            {
                var TheHesab = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = N'{final_lst.THES}'").FirstOrDefault();
                if (string.IsNullOrEmpty(TheHesab))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"حساب متناظر برای قسمت \"به حساب\" , وارد شده [{final_lst.THES}] , در سیستم وجود ندارد !" });
                }
            }
            if (final_lst.NAHVA != 1 && final_lst.NAHVA != 3) //اگر نقد و سایر نیست
            {
                if (final_lst.NO_AM == 2 && final_lst.NAHVA == 4) //واگذاری چک به صورت صحیح است
                {
                }
                else
                {
                    if (final_lst.N_SERI is null)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "مشخصات چک خالی است." });
                    }
                    if (final_lst.BANK is null)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "بانک مربوط به چک خالی است." });
                    }
                }
            }
            else
            {
                if (final_lst.MABL is null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ خالی است." });
                }
            }
            if (final_lst.MHAZ_NO is not null)
            {
                var markazHaz = dbms.DoGetDataSQL<int?>("SELECT TOP 1 MHAZ_NO FROM dbo.TCOD_MARKAZHAZ WHERE MHAZ_NO = " + final_lst.MHAZ_NO).FirstOrDefault();
                if (markazHaz is null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مرکز هزینه انتخاب شده معتبر نیست." });
                }
            }
            //if (final_lst.ARZD is null)
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "داده های دیتابیس برای نوع چک ,فیلد نوع ارز خالی است." });
            //}


            #endregion

            if (ErrosMessages.Any() && _DisplayMsg_)
            {
                new MsgListwin(false, ErrosMessages).Show();
                return false;
            }
            return true;
        }

        public bool CmdSaveRecord(PGET_LST final_lst)
        {
            if (BodyIsValid(final_lst) is false) return false;

            //Saving...

            #region DETAIL
            string? FHES_T2 = string.IsNullOrEmpty(final_lst?.FHES_T2?.ToStringNullSafe()) ? "NULL" : final_lst.FHES_T2.ToString();
            string? THES_T2 = string.IsNullOrEmpty(final_lst?.THES_T2?.ToStringNullSafe()) ? "NULL" : final_lst.THES_T2.ToString();
            string? FHES_T3 = string.IsNullOrEmpty(final_lst?.FHES_T3?.ToStringNullSafe()) ? "NULL" : final_lst.FHES_T3.ToString();
            string? THES_T3 = string.IsNullOrEmpty(final_lst?.THES_T3?.ToStringNullSafe()) ? "NULL" : final_lst.THES_T3.ToString();
            string? FHES_T4 = string.IsNullOrEmpty(final_lst?.FHES_T4?.ToStringNullSafe()) ? "NULL" : final_lst.FHES_T4.ToString();
            string? THES_T4 = string.IsNullOrEmpty(final_lst?.THES_T4?.ToStringNullSafe()) ? "NULL" : final_lst.THES_T4.ToString();


            try
            {
                if (final_lst.N_SERI == 0 || final_lst.BANK == 0)
                {
                    final_lst.N_SERI = null;
                    final_lst.BANK = null;
                }

                if (final_lst.IDH is null || final_lst.IDH <= 0) //INSERT
                {
                    final_lst.ID = Convert.ToInt32(ID.Text);
                    var IDH_RESULT_INSERT = dbms.DoGetDataSQL<int>($@"INSERT INTO dbo.PGET_LST(ID, DATE, RADIF, NO_AM, NAHVA, FHES_K, FHES_M, FHES_T, THES_K, THES_M, THES_T, SHARH, MABL, N_SERI, BANK, FHES, THES, ARZD, FHES_T2, THES_T2, FHES_T3, THES_T3, FHES_T4, THES_T4, MHAZ_NO)
                                         OUTPUT INSERTED.IDH
                                         VALUES(
                                         {final_lst.ID} ,
                                         {DATE.Text.ToRawTarikh()} ,
                                         (ISNULL((SELECT MAX(RADIF) FROM dbo.PGET_LST WHERE ID = {final_lst.ID}), 0) + 1),
                                         {final_lst.NO_AM}   ,
                                         {final_lst.NAHVA} ,
                                         {(final_lst.FHES_K is null ? "NULL" : final_lst.FHES_K)}   ,
                                         {(final_lst.FHES_M is null ? "NULL" : final_lst.FHES_M)}   ,
                                         {(final_lst.FHES_T is null ? "NULL" : final_lst.FHES_T)}   ,
                                         {(final_lst.THES_K is null ? "NULL" : final_lst.THES_K)}   ,
                                         {(final_lst.THES_M is null ? "NULL" : final_lst.THES_M)}   ,
                                         {(final_lst.THES_T is null ? "NULL" : final_lst.THES_T)}   ,
                                         N'{final_lst.SHARH}' ,
                                         {(final_lst.MABL is null ? 0 : final_lst.MABL)} ,
                                         {(final_lst.N_SERI is null ? "NULL" : final_lst.N_SERI)} ,
                                         {(final_lst.BANK is null ? "NULL" : final_lst.BANK)}   ,
                                         N'{final_lst.FHES}' ,
                                         N'{final_lst.THES}' ,
                                         {(final_lst.ARZD is null ? "NULL" : final_lst.ARZD)},
                                         {(FHES_T2 is null ? "NULL" : FHES_T2)}   ,
                                         {(THES_T2 is null ? "NULL" : THES_T2)}   ,
                                         {(FHES_T3 is null ? "NULL" : FHES_T3)}   ,
                                         {(THES_T3 is null ? "NULL" : THES_T3)}   ,
                                         {(FHES_T4 is null ? "NULL" : FHES_T4)}   ,
                                         {(THES_T4 is null ? "NULL" : THES_T4)}   ,
                                         {(final_lst.MHAZ_NO is null ? "NULL" : final_lst.MHAZ_NO.ToString())}
                                          )").FirstOrDefault();

                    CURRENT_ITMES_ROW.IDH = Convert.ToInt32(IDH_RESULT_INSERT);
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.PGET_LST
                                        SET DATE = {DATE.Text.ToRawTarikh()},
                                           NO_AM = {final_lst.NO_AM},
                                           NAHVA = {final_lst.NAHVA},
                                           FHES_K = {(final_lst.FHES_K is null ? "NULL" : final_lst.FHES_K)},
                                           FHES_M = {(final_lst.FHES_M is null ? "NULL" : final_lst.FHES_M)},
                                           FHES_T = {(final_lst.FHES_T is null ? "NULL" : final_lst.FHES_T)},
                                           THES_K = {final_lst.THES_K},
                                           THES_M = {final_lst.THES_M},
                                           THES_T = {final_lst.THES_T},
                                           SHARH = N'{final_lst.SHARH}',
                                           MABL = {(final_lst.MABL is null ? "0" : final_lst.MABL)},
                                           N_SERI = {(final_lst.N_SERI is null ? "NULL" : final_lst.N_SERI)},
                                           BANK = {(final_lst.BANK is null ? "NULL" : final_lst.BANK)},
                                           FHES = N'{final_lst.FHES}',
                                           THES = N'{final_lst.THES}',
                                           ARZD = {final_lst.ARZD},
                                           FHES_T2 = {(FHES_T2 is null ? "NULL" : FHES_T2)},
                                           THES_T2 = {THES_T2},
                                           FHES_T3 = {(FHES_T3 is null ? "NULL" : FHES_T3)},
                                           THES_T3 = {THES_T3},
                                           FHES_T4 = {(FHES_T4 is null ? "NULL" : FHES_T4)},
                                           THES_T4 = {THES_T4},
                                           MHAZ_NO = {(final_lst.MHAZ_NO is null ? "NULL" : final_lst.MHAZ_NO.ToString())}
                                        WHERE  IDH = {final_lst.IDH}");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // 2627 & 2601 : duplicate key
                {
                    new Msgwin(false, "این سطر تکراری است و نمی‌توان آن را ثبت کرد").ShowDialog();
                    return false;
                }
                else if (ex.Number == 547)   // 547 : foreign key constraint violation
                {
                    new Msgwin(false, "ابتدا سربرگ مربوطه را ذخیره کنید، سپس جزئیات را ثبت نمایید.").ShowDialog();
                    return false;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات ثبت سطر ! اطلاعات ذخیره نشده است.").ShowDialog();
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }

            #endregion
            this.MABL.Text = SUM_OF_MABL.ToString();
            return true;
        }

        //#سیو کردن بخش بالایی فرم
        //Used_SAVEBTN_Click

        private void _____Out____CmdSaveHeader()
        {
            // جلوگیری از اجرا شدن مجدد در لحظه این رویداد:
            //PGET_LST_SUB.PreviewGotKeyboardFocus -= PGET_LST_SUB_PreviewGotKeyboardFocus;
            if (string.IsNullOrEmpty(ID.Text) || Convert.ToInt32(ID.Text) == 0)
            {
                //_newrecord = true;
            }
            else
            {
                //_newrecord = false;
            }
            if (NewRecord) //INSERT HEAD
            {
                var _id = CL_HESABDARI.GetNewIDD("ID", "PGET_HED", "MOLAH");
                ID.Text = _id.ToString();
                IDK.Text = _id.ToString();
                dbms.DoExecuteSQL($@"INSERT INTO dbo.PGET_HED(ID, DATE, MOLAH, DEPATMAN, SHIFT, USER_NAME, KIND, IDK)
                                         VALUES({_id},
                                         {DATE.Text.ToRawTarikh()}   ,
                                         N'{MOLAH.Text.Trim()}' ,
                                         {DEPATMAN.SelectedValue}   ,
                                         {SHIFT.SelectedValue}   ,
                                         N'{USER_NAME.Text}' ,
                                         {KIND.SelectedValue}   ,
                                         {IDK.Text})
                                         ");

            }
            else //UPDATE HEAD
            {
                byte _SGN1_ = Convert.ToByte(SGN1.IsChecked);
                byte _SGN2_ = Convert.ToByte(SGN2.IsChecked);
                byte _SGN3_ = Convert.ToByte(SGN3.IsChecked);

                dbms.DoExecuteSQL($@"UPDATE dbo.PGET_HED
                                        SET DATE = {DATE.Text.ToRawTarikh()}, MOLAH = N'{MOLAH.Text.Trim()}', 
                                        DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue},
                                        KIND = {KIND.SelectedValue}, IDK = {IDK.Text}, SGN1 = {_SGN1_}, SGN2 = {_SGN2_}, SGN3 = {_SGN3_}
                                        WHERE ID = {ID.Text}");
            }
            //PGET_LST_SUB.PreviewGotKeyboardFocus += PGET_LST_SUB_PreviewGotKeyboardFocus;
        }

        private void NO_AM_AfterUpdate(int row_index)
        {
            switch (CURRENT_ITMES_ROW.NO_AM)
            {
                case 1:
                    {
                        switch (CURRENT_ITMES_ROW.NAHVA) // دريافت
                        {
                            case 1:
                                {
                                    CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(Baseknow.SANDOGH);
                                    if (Strings.Mid(Baseknow.OPTIONSS, 38, 1) == "5")
                                    {
                                        if (!IsNull(DEPATMAN.SelectedValue) && !IsNull(SHIFT.SelectedValue))
                                        {
                                            CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(DEPATMAN.SelectedValue);
                                            CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(SHIFT.SelectedValue);
                                        }
                                        else
                                        {
                                            CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToInt32(Baseknow.SANDOGH)));
                                            CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToInt32(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.THES_M)));
                                        }
                                    }
                                    else
                                    {
                                        CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToInt32(Baseknow.SANDOGH)));
                                        CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToInt32(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.THES_M)));
                                    }
                                    CURRENT_ITMES_ROW.THES = CURRENT_ITMES_ROW.THES_K + "-" + CURRENT_ITMES_ROW.THES_M + "-" + CURRENT_ITMES_ROW.THES_T;



                                    this.tHES_KColumn.IsReadOnly = true;
                                    this.tHES_MColumn.IsReadOnly = true;
                                    this.tHES_TColumn.IsReadOnly = true;
                                    //this.THES_T.TabStop = false;

                                    //this.THES_K.TabStop = false;
                                    SetIsTabStopCell("THES_K", false, row_index);
                                    //this.THES_M.TabStop = false;
                                    SetIsTabStopCell("THES_M", false, row_index);
                                    //this.THES.TabStop = false;
                                    SetIsTabStopCell("THES", false, row_index);
                                    this.tHESColumn.IsReadOnly = true;
                                    this.tHES_T2Column = null;
                                    this.tHES_T3Column = null;
                                    this.tHES_T4Column = null;
                                    break;
                                }
                            case 2:
                                {
                                    CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                    this.tHES_KColumn.IsReadOnly = true;
                                    this.tHES_MColumn.IsReadOnly = true;
                                    this.tHES_TColumn.IsReadOnly = true;
                                    //this.THES_T.TabStop = false;
                                    SetIsTabStopCell("THES_T", false, row_index);
                                    //this.THES_K.TabStop = false;
                                    SetIsTabStopCell("THES_K", false, row_index);
                                    //this.THES_M.TabStop = false;
                                    SetIsTabStopCell("THES_M", false, row_index);
                                    CURRENT_ITMES_ROW.THES = CURRENT_ITMES_ROW.THES_K + "-" + CURRENT_ITMES_ROW.THES_M + "-" + CURRENT_ITMES_ROW.THES_T;
                                    //this.THES.TabStop = false;
                                    SetIsTabStopCell("THES", false, row_index);
                                    this.tHESColumn.IsReadOnly = true;
                                    this.tHES_T2Column = null;
                                    this.tHES_T3Column = null;
                                    this.tHES_T4Column = null;
                                    break;
                                }
                            case 6:
                                {
                                    CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADV));
                                    CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADV));
                                    CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADV));
                                    this.tHES_KColumn.IsReadOnly = true;
                                    this.tHES_MColumn.IsReadOnly = true;
                                    this.tHES_TColumn.IsReadOnly = true;
                                    //this.THES_T.TabStop = false;
                                    SetIsTabStopCell("THES_T", false, row_index);
                                    //this.THES_K.TabStop = false;
                                    SetIsTabStopCell("THES_K", false, row_index);
                                    //this.THES_M.TabStop = false;
                                    SetIsTabStopCell("THES_M", false, row_index);
                                    CURRENT_ITMES_ROW.THES = CURRENT_ITMES_ROW.THES_K + "-" + CURRENT_ITMES_ROW.THES_M + "-" + CURRENT_ITMES_ROW.THES_T;
                                    //this.THES.TabStop = false;
                                    SetIsTabStopCell("THES", false, row_index);
                                    this.tHESColumn.IsReadOnly = true;
                                    this.tHES_T2Column = null;
                                    this.tHES_T3Column = null;
                                    this.tHES_T4Column = null;
                                    break;
                                }
                            case 5:
                                {
                                    CURRENT_ITMES_ROW.THES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APA));
                                    CURRENT_ITMES_ROW.THES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APA));
                                    CURRENT_ITMES_ROW.THES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APA));
                                    this.tHES_KColumn.IsReadOnly = true;
                                    this.tHES_MColumn.IsReadOnly = true;
                                    this.tHES_TColumn.IsReadOnly = true;
                                    //this.THES_K.TabStop = false;
                                    SetIsTabStopCell("THES_K", false, row_index);
                                    //this.THES_M.TabStop = false;
                                    SetIsTabStopCell("THES_M", false, row_index);
                                    //this.THES_T.TabStop = false;
                                    SetIsTabStopCell("THES_T", false, row_index);
                                    CURRENT_ITMES_ROW.THES = Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_K)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_M)) + "-" + Strings.Trim(Conversion.Str(CURRENT_ITMES_ROW.THES_T));
                                    //this.THES.TabStop = false;
                                    SetIsTabStopCell("THES", false, row_index);
                                    this.tHESColumn.IsReadOnly = true;
                                    this.tHES_T2Column = null;
                                    this.tHES_T3Column = null;
                                    this.tHES_T4Column = null;
                                    break;
                                }
                            case 4:
                                {
                                    Msgwin msgwin = new Msgwin(false, "مقدار وارده مجاز نيست");
                                    msgwin.Show();
                                    //DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "مقدار وارده مجاز نيست");
                                    //this.NAHVA = Null;
                                    break;
                                }

                            default:
                                {
                                    this.tHES_KColumn.IsReadOnly = false;
                                    this.tHES_MColumn.IsReadOnly = false;
                                    this.tHES_TColumn.IsReadOnly = false;
                                    //this.THES_T.TabStop = true;
                                    SetIsTabStopCell("THES_T", true, row_index);
                                    //this.THES_K.TabStop = true;
                                    SetIsTabStopCell("THES_K", true, row_index);
                                    //this.THES_M.TabStop = true;
                                    SetIsTabStopCell("THES_M", true, row_index);
                                    //this.THES.TabStop = true;
                                    SetIsTabStopCell("THES", true, row_index);
                                    this.tHESColumn.IsReadOnly = false;
                                    break;
                                }
                        }
                        this.fHES_KColumn.IsReadOnly = false;
                        this.fHES_MColumn.IsReadOnly = false;
                        this.fHES_TColumn.IsReadOnly = false;
                        //this.FHES_K.TabStop = true;
                        SetIsTabStopCell("FHES_K", true, row_index);
                        //this.FHES_T.TabStop = true;
                        SetIsTabStopCell("FHES_T", true, row_index);
                        //this.FHES_M.TabStop = true;
                        SetIsTabStopCell("FHES_M", true, row_index);
                        //this.FHES.TabStop = true;
                        SetIsTabStopCell("FHES", true, row_index);
                        this.FHES_COLUMN.IsReadOnly = false;
                        break;
                    }
                case 2:
                    {
                        switch (CURRENT_ITMES_ROW.NAHVA) // پرداخت
                        {
                            case 1:
                                {
                                    CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(Baseknow.SANDOGH);
                                    if (Strings.Mid(Baseknow.OPTIONSS, 38, 1) == "5")
                                    {
                                        if (!IsNull(DEPATMAN.SelectedValue) && !IsNull(SHIFT.SelectedValue))
                                        {
                                            CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(DEPATMAN.SelectedValue);
                                            CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(SHIFT.SelectedValue);
                                        }
                                        else
                                        {
                                            CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                            CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.FHES_M)));
                                        }
                                    }
                                    else
                                    {
                                        CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.FIRSTM(Convert.ToDouble(Baseknow.SANDOGH)));
                                        CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.FIRSTT(Convert.ToDouble(Baseknow.SANDOGH), Convert.ToDouble(CURRENT_ITMES_ROW.FHES_M)));
                                    }
                                    this.fHES_KColumn.IsReadOnly = true;
                                    this.fHES_MColumn.IsReadOnly = true;
                                    // this.FHES_K.TabStop = false;
                                    SetIsTabStopCell("FHES_K", false, row_index);
                                    this.fHES_TColumn.IsReadOnly = true;
                                    //this.FHES_T.TabStop = false;
                                    SetIsTabStopCell("FHES_T", false, row_index);
                                    //this.FHES_M.TabStop = false;
                                    SetIsTabStopCell("FHES_M", false, row_index);
                                    CURRENT_ITMES_ROW.FHES = CURRENT_ITMES_ROW.FHES_K + "-" + CURRENT_ITMES_ROW.FHES_M + "-" + CURRENT_ITMES_ROW.FHES_T;
                                    //this.FHES.TabStop = false;
                                    SetIsTabStopCell("FHES", false, row_index);
                                    this.FHES_COLUMN.IsReadOnly = true;
                                    this.fHES_T2Column = null;
                                    this.fHES_T3Column = null;
                                    this.fHES_T4Column = null;
                                    break;
                                }
                            case 2:
                                {
                                    CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APA));
                                    CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APA));
                                    CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APA));
                                    this.fHES_KColumn.IsReadOnly = true;
                                    this.fHES_MColumn.IsReadOnly = true;
                                    //this.FHES_T.TabStop = false;
                                    SetIsTabStopCell("FHES_T", false, row_index);
                                    this.fHES_TColumn.IsReadOnly = true;
                                    //this.FHES_K.TabStop = false;
                                    SetIsTabStopCell("FHES_K", false, row_index);
                                    //this.FHES_M.TabStop = false;
                                    SetIsTabStopCell("FHES_M", false, row_index);
                                    CURRENT_ITMES_ROW.FHES = CURRENT_ITMES_ROW.FHES_K + "-" + CURRENT_ITMES_ROW.FHES_M + "-" + CURRENT_ITMES_ROW.FHES_T;
                                    //this.FHES.TabStop = false;
                                    SetIsTabStopCell("FHES", false, row_index);
                                    this.FHES_COLUMN.IsReadOnly = true;
                                    this.fHES_T2Column = null;
                                    this.fHES_T3Column = null;
                                    this.fHES_T4Column = null;
                                    break;
                                }
                            case var @case when @case == 2:
                                {
                                    CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.APV));
                                    CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.APV));
                                    CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.APV));
                                    this.fHES_KColumn.IsReadOnly = true;
                                    this.fHES_MColumn.IsReadOnly = true;
                                    //this.FHES_T.TabStop = false;
                                    SetIsTabStopCell("FHES_T", false, row_index);
                                    this.fHES_TColumn.IsReadOnly = true;
                                    //this.FHES_K.TabStop = false;
                                    SetIsTabStopCell("FHES_K", false, row_index);
                                    //this.FHES_M.TabStop = false;
                                    SetIsTabStopCell("FHES_M", false, row_index);
                                    CURRENT_ITMES_ROW.FHES = CURRENT_ITMES_ROW.FHES_K + "-" + CURRENT_ITMES_ROW.FHES_M + "-" + CURRENT_ITMES_ROW.FHES_T;
                                    //this.FHES.TabStop = false;
                                    SetIsTabStopCell("FHES", false, row_index);
                                    this.FHES_COLUMN.IsReadOnly = true;
                                    this.fHES_T2Column = null;
                                    this.fHES_T3Column = null;
                                    this.fHES_T4Column = null;
                                    break;
                                }
                            case 4:
                                {
                                    CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                    this.fHES_KColumn.IsReadOnly = true;
                                    this.fHES_MColumn.IsReadOnly = true;
                                    //this.FHES_T.TabStop = false;
                                    SetIsTabStopCell("FHES_T", false, row_index);
                                    this.fHES_TColumn.IsReadOnly = true;
                                    //this.FHES_K.TabStop = false;
                                    SetIsTabStopCell("FHES_K", false, row_index);
                                    //this.FHES_M.TabStop = false;
                                    SetIsTabStopCell("FHES_M", false, row_index);
                                    CURRENT_ITMES_ROW.FHES = CURRENT_ITMES_ROW.FHES_K + "-" + CURRENT_ITMES_ROW.FHES_M + "-" + CURRENT_ITMES_ROW.FHES_T;
                                    //this.FHES.TabStop = false;
                                    SetIsTabStopCell("FHES", false, row_index);
                                    this.FHES_COLUMN.IsReadOnly = true;
                                    this.fHES_T2Column = null;
                                    this.fHES_T3Column = null;
                                    this.fHES_T4Column = null;
                                    break;
                                }
                            case 5:
                                {
                                    CURRENT_ITMES_ROW.FHES_K = Convert.ToInt32(CL_HESABDARI.GETKOL(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.FHES_M = Convert.ToInt32(CL_HESABDARI.GETMOIN(Baseknow.ADA));
                                    CURRENT_ITMES_ROW.FHES_T = Convert.ToInt32(CL_HESABDARI.GETTAF(Baseknow.ADA));
                                    this.fHES_KColumn.IsReadOnly = true;
                                    this.fHES_MColumn.IsReadOnly = true;
                                    //this.FHES_T.TabStop = false;
                                    SetIsTabStopCell("FHES_T", false, row_index);
                                    this.fHES_TColumn.IsReadOnly = true;
                                    //this.FHES_K.TabStop = false;
                                    SetIsTabStopCell("FHES_K", false, row_index);
                                    //this.FHES_M.TabStop = false;
                                    SetIsTabStopCell("FHES_M", false, row_index);
                                    CURRENT_ITMES_ROW.FHES = CURRENT_ITMES_ROW.FHES_K + "-" + CURRENT_ITMES_ROW.FHES_M + "-" + CURRENT_ITMES_ROW.FHES_T;
                                    //this.FHES.TabStop = false;
                                    SetIsTabStopCell("FHES", false, row_index);
                                    this.FHES_COLUMN.IsReadOnly = true;
                                    this.fHES_T2Column = null;
                                    this.fHES_T3Column = null;
                                    this.fHES_T4Column = null;
                                    break;
                                }

                            default:
                                {
                                    this.fHES_KColumn.IsReadOnly = false;
                                    this.fHES_MColumn.IsReadOnly = false;
                                    //this.FHES_K.TabStop = true;
                                    SetIsTabStopCell("FHES_K", true, row_index);
                                    this.fHES_TColumn.IsReadOnly = false;
                                    //this.FHES_T.TabStop = true;
                                    SetIsTabStopCell("FHES_T", true, row_index);
                                    //this.FHES_M.TabStop = true;
                                    SetIsTabStopCell("FHES_M", true, row_index);
                                    //this.FHES.TabStop = true;
                                    SetIsTabStopCell("FHES", true, row_index);
                                    this.FHES_COLUMN.IsReadOnly = false;
                                    break;
                                }
                        }
                        this.tHES_KColumn.IsReadOnly = false;
                        this.tHES_MColumn.IsReadOnly = false;
                        this.tHES_TColumn.IsReadOnly = false;
                        //this.THES_K.TabStop = true;
                        SetIsTabStopCell("THES_K", true, row_index);
                        //this.THES_T.TabStop = true;
                        SetIsTabStopCell("THES_T", true, row_index);
                        //this.THES_M.TabStop = true;
                        SetIsTabStopCell("THES_M", true, row_index);
                        //this.FHES.TabStop = true;
                        SetIsTabStopCell("FHES", true, row_index);
                        this.FHES_COLUMN.IsReadOnly = false;
                        break;
                    }
            }
        }

        private void SetIsTabStopCell(string COLUMNNAME, bool _stopable, int row_index)
        {
            if (CL_LMethods.IsValidIndex(PGET_LST_SUB, row_index))
            {
                var DGCInf = new DataGridCellInfo(PGET_LST_SUB.Items[row_index], PGET_LST_SUB.Columns[PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == COLUMNNAME).DisplayIndex]);
            }


            var THECELL = CL_LMethods.GetCell(PGET_LST_SUB, CURRENT_ROW_INDEX, PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == COLUMNNAME).DisplayIndex);
            if (!(THECELL is null))
            {
                THECELL.IsTabStop = _stopable;
            }
        }

        public string DefaultReFocusColumn { get; set; } = "NO_AM";
        public bool IsDataGrid_IsFocused { get; private set; }

        void ResotreLastFocusOnRow()
        {
            if (PGET_LST_SUB.Items.Count > 0)
            {
                PGET_LST_SUB.Focus();
                DataGridRow row = PGET_LST_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX) as DataGridRow;
                if (row is null)
                {
                    object item = PGET_LST_SUB.Items[CURRENT_ROW_INDEX];
                    PGET_LST_SUB.ScrollIntoView(PGET_LST_SUB.Items[CURRENT_ROW_INDEX]);
                    row = (DataGridRow)PGET_LST_SUB.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
                    PGET_LST_SUB.SelectedItem = item;

                    //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    var col_index = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == DefaultReFocusColumn).DisplayIndex;
                    DataGridCell cell = CL_LMethods.GetCell(PGET_LST_SUB, row, Convert.ToInt32(col_index));
                    if (cell != null)
                        cell.Focus();
                }
                else
                {
                    object item = PGET_LST_SUB.Items[CURRENT_ROW_INDEX];
                    PGET_LST_SUB.SelectedItem = item;
                    PGET_LST_SUB.ScrollIntoView(item);
                    //ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    var col_index = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == DefaultReFocusColumn).DisplayIndex;
                    DataGridCell cell = CL_LMethods.GetCell(PGET_LST_SUB, row, Convert.ToInt32(col_index));
                    if (cell != null)
                        cell.Focus();
                }
            }
        }

        private void DELETE_FACTOR22_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_FACTOR22.Visibility == Visibility.Visible;
            if (!DELETE_FACTOR22.IsEnabled || !IsVisible) { return; }

            if (!IsAllowEditDataGrid())
            {
                return;
            }

            ApplyDataGridItems();

            PGET_LST_SUB.CommitEdit();

            var dt = DateTime.Now;
            // If Forms![baseknow]![TRANSF] Then
            CL_HESABDARI.TR("PGET_HED", "(ID = " + ID.Text + " )", dt, 1);
            CL_HESABDARI.TR("PGET_LST", "(ID = " + ID.Text + " )", dt, 2);

            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "خزانه داری",
                    recordId: ID.Text,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید؟");
            msgwin.ShowDialog();

            if (KHAZANEH_DATA?.Count == 0)
            {
                if (!string.IsNullOrEmpty(ID.Text) && ID.Text != "0")
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.PGET_HED WHERE ID = {ID.Text}");
                        RefreshAfterDelete();
                    }
                    catch (SqlException ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }

                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این  خزانه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                            return;
                        }
                        else
                        {
                            new Msgwin(false, "به دلیل بروز خطا در پایگاه داده این خزانه حذف نشد").ShowDialog();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }

                        new Msgwin(false, "خطا در انجام علملیات حذف خزانه").ShowDialog();
                        return;
                    }
                    ReGetData();
                }
            }
            else
            {
                List<PGET_LST> AllRows = new List<PGET_LST>();

                for (int i = 0; i < PGET_LST_SUB.SelectedItems.Count; i++)
                {
                    if (CL_LMethods.IsNewPlaceHolder(PGET_LST_SUB, PGET_LST_SUB.SelectedItems[i])) { continue; }

                    AllRows.Add(PGET_LST_SUB.SelectedItems[i] as PGET_LST);
                }

                if (AllRows.Count <= 0 || AllRows is null)
                    return;

                if (msgwin.DialogResult is true)
                {
                    foreach (PGET_LST item in AllRows)
                    {
                        if (item == null) { continue; }

                        if (CL_LMethods.IsNewPlaceHolder(PGET_LST_SUB, item)) { continue; }

                        switch (item.NO_AM)
                        {
                            case 2:
                                {
                                    switch (item.NAHVA) // پرداخت
                                    {
                                        case 1:
                                            {
                                                KHAZANE_Row_Deleter(item);

                                                break;
                                            } // پرداخت نقد
                                        case 2:
                                            {
                                                if (!IsNull(item.N_SERI))
                                                {
                                                    var rst = dbms.DoGetDataSQL<PAY_GETD>("select * from PAY_GETP where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK).ToList();
                                                    if (rst.Count > 0)
                                                    {
                                                        if ((!IsNull(rst.FirstOrDefault().N_KOL2) && rst.FirstOrDefault().N_KOL2 != 911) || !IsNull(rst.FirstOrDefault().N_KOL3))
                                                        {
                                                            Msgwin msgwin1 = new Msgwin(false, "چكي كه وصولي يا  برگشتي خورده قابل حذف نيست");
                                                            msgwin1.ShowDialog();

                                                            // DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "چكي كه وصولي يا  برگشتي خورده قابل حذف نيست");
                                                            // CANCEL = Conversions.ToInteger(true);
                                                        }
                                                        else
                                                        {
                                                            // rst.Fields("N_KOL2") = 911
                                                            rst.FirstOrDefault().N_KOL = 911;
                                                            // rst.Fields("N_moin2") = 1
                                                            rst.FirstOrDefault().N_MOIN = 1;
                                                            // rst.Fields("N_taf2") = 1
                                                            rst.FirstOrDefault().N_TAF = 1;
                                                            rst.FirstOrDefault().HES1 = "911-1-1";

                                                            string _where = " where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK;
                                                            dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL = 911 ,N_MOIN = 1 , N_TAF = 1 , hes1 = N'911-1-1' {_where} ");
                                                            KHAZANE_Row_Deleter(item);
                                                            //rst.update();
                                                        }
                                                    }
                                                    //rst.Close();
                                                }

                                                break;
                                            } // پرداخت چک
                                        case 3:
                                            {
                                                KHAZANE_Row_Deleter(item);

                                                break;
                                            } // پرداخت پرداخت سایر
                                        case 4:
                                            {
                                                if (IsNull(item.N_SERI) || IsNull(item.BANK))
                                                {
                                                    item.N_SERI = 0;
                                                    item.BANK = 0;
                                                }
                                                var rst = dbms.DoGetDataSQL<PAY_GETD>("select * from PAY_GETD where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK).ToList();
                                                if (rst.Count == 0)
                                                {
                                                    item.N_SERI = null;
                                                    item.BANK = null;
                                                }
                                                else
                                                {
                                                    string _where = " where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK;
                                                    rst.FirstOrDefault().N_KOL = null;
                                                    rst.FirstOrDefault().N_MOIN = null;
                                                    rst.FirstOrDefault().N_TAF = null;
                                                    rst.FirstOrDefault().HES1 = null;
                                                    dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL = NULL ,N_MOIN = NULL , N_TAF = NULL , hes1 =NULL {_where} ");
                                                    KHAZANE_Row_Deleter(item);

                                                    // rst.update();
                                                    CL_HESABDARI.GETDLOG(1, item.N_SERI.ToString(), (int)item.BANK, rst.FirstOrDefault().DATE_S, (int)rst.FirstOrDefault().SANDUGH);
                                                }
                                                //rst.Close();
                                                break;
                                            } // پرداخت واگذاری چک
                                        case 5:
                                            {
                                                List<PAY_GETD> rst = null;
                                                if (IsNull(item.N_SERI) || IsNull(item.BANK))
                                                {
                                                    item.N_SERI = 0;
                                                    item.BANK = 0;
                                                }
                                                if (item.N_SERI == 0 && item.BANK == 0)
                                                {
                                                    rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE ID = {item.ID}").ToList();

                                                }
                                                else
                                                {
                                                    rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK).ToList();

                                                }
                                                if (rst.Count == 0)
                                                {
                                                    item.N_SERI = null;
                                                    item.BANK = null;
                                                }
                                                else
                                                {
                                                    string _where = " WHERE N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK;

                                                    rst.FirstOrDefault().N_KOL2 = null;
                                                    rst.FirstOrDefault().N_MOIN2 = null;
                                                    rst.FirstOrDefault().N_TAF2 = null;
                                                    rst.FirstOrDefault().HES2 = null;
                                                    //rst.update();

                                                    dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL2 = Null ,N_MOIN2 = Null , N_TAF2 = Null , HES2 = Null {_where} ");
                                                    KHAZANE_Row_Deleter(item);

                                                    if (!string.IsNullOrEmpty(item.N_SERI.ToStringNullSafe()) && !string.IsNullOrEmpty(item.BANK.ToStringNullSafe()) && !string.IsNullOrEmpty(rst.FirstOrDefault().DATE_S.ToStringNullSafe()))
                                                    {
                                                        CL_HESABDARI.GETDLOG(1, item.N_SERI.ToString(), (int)item.BANK, rst.FirstOrDefault().DATE_S, (int)rst.FirstOrDefault().SANDUGH);

                                                    }
                                                }
                                                //rst.Close();
                                                break;
                                            } // پرداخت برگشت چک
                                    }

                                    break;
                                }
                            case 1:
                                {
                                    switch (item.NAHVA) // دريافت
                                    {
                                        case 1:
                                            {
                                                KHAZANE_Row_Deleter(item);

                                                break;
                                            } // دریافت نقد
                                        case 2:
                                            {
                                                if (!IsNull(item.N_SERI))
                                                {
                                                    var rst = dbms.DoGetDataSQL<PAY_GETD>("select * from PAY_GETD where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK).ToList();
                                                    if (rst.Count > 0)
                                                    {
                                                        if ((!IsNull(rst.FirstOrDefault().N_KOL2) && rst.FirstOrDefault().N_KOL2 != 911) || !IsNull(rst.FirstOrDefault().N_KOL3))
                                                        {
                                                            Msgwin msgwin1 = new Msgwin(false, "چكي كه وصولي يا واگذاري يا برگشتي خورده قابل حذف نيست");
                                                            msgwin1.ShowDialog();
                                                            //DoCmd.OpenForm("MESAGEFORM", default, default, default, default, acDialog, "چكي كه وصولي يا واگذاري يا برگشتي خورده قابل حذف نيست");
                                                            CANCEL = true;
                                                        }
                                                        else
                                                        {
                                                            var test = rst.FirstOrDefault();
                                                            if ((rst.FirstOrDefault().N_KOL == Baseknow.BANKHA || rst.FirstOrDefault().N_KOL == 911) || IsNull(rst.FirstOrDefault().N_KOL))
                                                            {
                                                                //were here
                                                            }

                                                            string _where = " where  N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK;

                                                            // rst.Fields("N_KOL2") = 911
                                                            rst.FirstOrDefault().N_KOL = 911;
                                                            // rst.Fields("N_moin2") = 1
                                                            rst.FirstOrDefault().N_MOIN = 1;
                                                            // rst.Fields("N_taf2") = 1
                                                            rst.FirstOrDefault().N_TAF = 1;
                                                            rst.FirstOrDefault().HES1 = "911-1-1";
                                                            //rst.update();

                                                            dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET N_KOL = 911 , N_MOIN = 1 , N_TAF = 1 , HES1 = N'911-1-1' {_where} ");
                                                            KHAZANE_Row_Deleter(item);

                                                        }
                                                    }
                                                    CL_HESABDARI.GETDLOG(1, item.N_SERI.ToString(), (int)item.BANK, rst.FirstOrDefault().DATE_S, (int)rst.FirstOrDefault().SANDUGH);
                                                    //rst.Close();
                                                    //return;
                                                }
                                                else
                                                {
                                                    KHAZANE_Row_Deleter(item);
                                                }
                                                break;
                                            } // دریافت چک
                                        case 3:
                                            {
                                                KHAZANE_Row_Deleter(item);

                                                break;
                                            } // دریافت سایر
                                        case 5:
                                            {
                                                if (IsNull(item.N_SERI) || IsNull(item.BANK))
                                                {
                                                    item.N_SERI = 0;
                                                    item.BANK = 0;
                                                }
                                                var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK).ToList();
                                                if (rst.Count == 0)
                                                {
                                                    item.N_SERI = null;
                                                    item.BANK = null;
                                                }
                                                else
                                                {
                                                    string _where = " WHERE N_SERI=" + item.N_SERI + " AND BANK = " + item.BANK;

                                                    rst.FirstOrDefault().N_KOL2 = null;
                                                    rst.FirstOrDefault().N_MOIN2 = null;
                                                    rst.FirstOrDefault().N_TAF2 = null;
                                                    rst.FirstOrDefault().HES2 = null;
                                                    //rst.update();

                                                    dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL2 = Null , N_MOIN2 = Null , N_TAF2 = Null , HES2 = Null {_where} ");
                                                    KHAZANE_Row_Deleter(item);
                                                }
                                                //rst.Close();
                                                break;
                                            } // دریافت برگشت چک
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

        }

        private void SAVEBTN_Click(object sender, RoutedEventArgs e)
        {
            //Process Prc = ProcLoader.Start();

            #region Header_Validatation
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            string date_n_val = DATE.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = ".تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد." });
            }
            if (KIND.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع برگه خالی است." });
            }
            if (DEPATMAN.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد نمیتواند خالی باشد" });
            }
            if (SHIFT.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شیفت نمی تواند خالی باشد" });
            }


            if (ErrosMessages.Count > 0)
            {
                //ProcLoader.Stop(Prc);
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return;
            }
            #endregion

            var IsSavedSuccess = DoCmdSaveHeader();

            if (!IsSavedSuccess)
            {
                DATE.Focus();
                DATE.SelectAll();
                return;
            }

            SANAD();

            PGET_LST_SUB.IsReadOnly = false;

            N_S.Text = dbms.DoGetDataSQL<string>($"SELECT TOP 1 N_S FROM PGET_HED WHERE ID = {ID.Text}").FirstOrDefault();
            if (!string.IsNullOrEmpty(N_S.Text))
            {
                MABNA.Text = dbms.DoGetDataSQL<string>($"SELECT TOP 1 BASE FROM DEED_HED WHERE N_S = {N_S.Text}").FirstOrDefault();
            }

            //Form_Current();

            //var col_index = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NO_AM").DisplayIndex;
            //PGET_LST_SUB.CurrentCell = new DataGridCellInfo(PGET_LST_SUB.SelectedItem, PGET_LST_SUB.Columns[col_index]);
            //PGET_LST_SUB.BeginEdit();
            if (KHAZANEH_DATA.Count == 0)
            {
                var DEFINDX = (PGET_LST_SUB.SelectedIndex < 0) ? 0 : PGET_LST_SUB.SelectedIndex;
                CL_LMethods.FocusCellReadyToEdit(PGET_LST_SUB, "NO_AM", DEFINDX, true);
            }

            //CL_LMethods.GetCellByIndexAndSortMemberPath(PGET_LST_SUB, DEFINDX, "NO_AM").Focus();
            //CL_LMethods.GetCellToFocus(PGET_LST_SUB, DEFINDX, "NO_AM").Focus();
            //PGET_LST_SUB.BeginEdit();

            if (Convert.ToInt32(ID.Text) > 0)
            {
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            //ProcLoader.Stop(Prc);

            this.MABL.Text = SUM_OF_MABL.ToString();

            universControl.PopNotifyShow("داده ها ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            ChangeIsHappend = false;
        }

        private void PGET_LST_SUB_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        int escapePressedCount = 0;




        private void MOLAH_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(MOLAH.Text))
            //{
            //    CURRENT_ITMES_ROW.SHARH = MOLAH.Text;
            //}
        }

        private void PGET_LST_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var grid = sender as DataGrid;
                if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null && PGET_LST_SUB.SelectedIndex > -1)
                {
                    if (PGET_LST_SUB.IsReadOnly == true)
                    {
                        var CurrentData = PGET_LST_SUB.Items[PGET_LST_SUB.SelectedIndex] as PGET_LST;
                        if (CurrentData != null && grid?.CurrentCell.Column?.SortMemberPath == "MABL")
                        {
                            CURRENT_ROW_INDEX = PGET_LST_SUB.SelectedIndex;
                            if (CurrentData.NO_AM == 1 && CurrentData.NAHVA == 2)
                            {
                                GETCHEK gETCHEK = new GETCHEK(I_AM_KHAZANEH, CurrentData.MABL.ToString(), CURRENT_ROW_INDEX, true);
                                gETCHEK.ShowDialog();
                            }
                            if (CurrentData.NO_AM == 2 && (CurrentData.NAHVA == 2 || CurrentData.NAHVA == 1))
                            {
                                var _serverfilter = "N_SERI = " + CurrentData.N_SERI + " AND BANK = " + CurrentData.BANK + " AND MABL = " + CurrentData.MABL;
                                PAYCHEK pAYCHEK = new PAYCHEK(_serverfilter, I_AM_KHAZANEH, CurrentData.MABL.ToString(), CURRENT_ROW_INDEX, true);
                                pAYCHEK.ShowDialog();
                            }
                            if (CurrentData.NO_AM == 2 && CurrentData.NAHVA == 4)
                            {
                                var _serverfilter = "N_SERI = " + CurrentData.N_SERI + " AND BANK = " + CurrentData.BANK + " AND MABL = " + CurrentData.MABL;
                                FORCHEK fORCHEK = new FORCHEK(I_AM_KHAZANEH, _serverfilter, CURRENT_ROW_INDEX, true);
                                fORCHEK.ShowDialog();
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public bool IsCancelSavingRequested(DataGridRowEditEndingEventArgs e = null)
        {
            if (CURRENT_ITMES_ROW is not null) //Just in Case
            {
                if (CURRENT_ITMES_ROW?.ID <= 0 || CURRENT_ITMES_ROW.ID is null) //رکورد جدید است INSERT
                {
                    if (IsExitChkButtonPressed is true) //درخواست لغو عملیات
                    {
                        if (e is not null) { e.Cancel = true; } //نذار ردیف جدید باز بشه (RowEditEnding)

                        IsExitChkButtonPressed = false; //ریست کردن 
                        return true; //سیو را انجام نده
                    }
                }
                else //رکورد قبلا سیو شده UPDATE
                {
                    if (IsExitChkButtonPressed is true) //درخواست لغو عملیات
                    {
                        CURRENT_ITMES_ROW = WAS_ROW_ITEM; //مقدار های قبلی رو برگردون توش #ERROR 
                        //ReGetData IF WAS NOT WORKED LINE BEFORE

                        if (e is not null) { e.Cancel = true; } //نذار ردیف جدید باز بشه (RowEditEnding)
                        IsExitChkButtonPressed = false; //ریست کردن 
                        return true; //سیو را انجام نده
                    }
                }
            }
            return false;
        }

        private bool KHAZANE_Row_Deleter(PGET_LST item)
        {
            bool isDeleteSomething = false;

            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
            {
                if (item.ID is null)
                {
                    KHAZANEH_DATA.Remove(item as PGET_LST);
                }
                else
                {
                    // YOUR_CODE_HERE
                    var _id = item.ID;
                    var _idh = item.IDH;
                    dbms.DoExecuteSQL($"DELETE FROM PGET_LST WHERE ID = {(_id is null ? "NULL" : _id)} AND IDH = {(_idh is null ? "NULL" : _idh)}");
                    SANAD();
                    // YOUR_CODE_HERE
                    isDeleteSomething = true;
                    ReGetData();
                    if (KHAZANEH_DATA.Count == 0)
                    {
                        PGET_LST_SUB.CanUserAddRows = false;
                        PGET_LST_SUB.CanUserAddRows = true;
                    }
                }
            }
            else
            {
                Msgwin msgwin1 = new Msgwin(false, "چیزی برای حذف وجود ندارند");
                msgwin1.ShowDialog();
                return false;
            }

            return isDeleteSomething;
        }

        #region Reports
        private void Command12_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                return;
            }

            Process Prc = ProcLoader.Start();
            //CL_PRC_LOADER.ShowPreloader();

            #region TheReport0StiLicKey


            var report = new StiReport();

            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Khazane_Reports.KHAZANE_AMALKARD.mrt");

            report.Load(pathreport);

            report.Dictionary.Databases.Clear();


            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            //((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 6000;
            //Parameters

            report["NAMBER_PARM"] = IDK.Text;

            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D;
            (report.GetComponentByName("Text28") as StiText).Text = ID.Text;

            //report.Compile();
            #region EMZA
            if (SGN1.IsChecked == true)
            {

                //var SAL_NAME = ((TextBox)sgn1usid.Template.FindName("PART_EditableTextBox", sgn1usid)).Text;
                var SAL_NAME = sgn1usid.Text;


                (report.GetComponentByName("nemz1") as StiText).Enabled = true;

                (report.GetComponentByName("semat1") as StiText).Enabled = true;

                (report.GetComponentByName("SDI1") as StiImage).Enabled = true;



                (report.GetComponentByName("nemz1") as StiText).Text = SGN1_INFO.USER_HESAB_NAME;

                (report.GetComponentByName("semat1") as StiText).Text = SGN1_INFO.USER_SEMAT;


            }
            else
            {
                (report.GetComponentByName("nemz1") as StiText).Enabled = false;
                (report.GetComponentByName("semat1") as StiText).Enabled = false;
                (report.GetComponentByName("SDI1") as StiImage).Enabled = false;
            }

            if (SGN2.IsChecked == true)
            {

                var SAL_NAME = sgn2usid.Text;

                (report.GetComponentByName("nemz2") as StiText).Enabled = true;
                (report.GetComponentByName("semat2") as StiText).Enabled = true;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = true;


                (report.GetComponentByName("nemz2") as StiText).Text = SGN2_INFO.USER_HESAB_NAME;

                (report.GetComponentByName("semat2") as StiText).Text = SGN2_INFO.USER_SEMAT;

            }
            else
            {
                (report.GetComponentByName("nemz2") as StiText).Enabled = false;
                (report.GetComponentByName("semat2") as StiText).Enabled = false;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = false;
            }

            if (SGN3.IsChecked == true)
            {

                var SAL_NAME = sgn3usid.Text;

                (report.GetComponentByName("nemz3") as StiText).Enabled = true;
                (report.GetComponentByName("semat3") as StiText).Enabled = true;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = true;

                (report.GetComponentByName("nemz3") as StiText).Text = SGN3_INFO.USER_HESAB_NAME;

                (report.GetComponentByName("semat3") as StiText).Text = SGN3_INFO.USER_SEMAT;

            }

            else
            {
                (report.GetComponentByName("nemz3") as StiText).Enabled = false;
                (report.GetComponentByName("semat3") as StiText).Enabled = false;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = false;
            }
            #endregion

            //report.Render(false);

            //report.Render();
            //CL_PRC_LOADER.HidePreloader();

            //report.ShowWithWpf();

            ProcLoader.Stop(Prc);

            new WINRPT(report, "عملکرد خزانه").Show();
            #endregion
        }

        private void Command23_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                return;
            }

            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Khazane_Reports.SANAD_DARYAFT.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));
            //Parameters

            #region EMZA
            if (SGN1.IsChecked == true)
            {
                //var SAL_NAME = ((TextBox)sgn1usid.Template.FindName("PART_EditableTextBox", sgn1usid)).Text;
                var SAL_NAME = sgn1usid.Text;

                (report.GetComponentByName("nem1") as StiText).Enabled = true;
                (report.GetComponentByName("semat1") as StiText).Enabled = true;
                (report.GetComponentByName("SDI1") as StiImage).Enabled = true;



                (report.GetComponentByName("nem1") as StiText).Text = SGN1_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat1") as StiText).Text = SGN1_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem1") as StiText).Enabled = false;
                (report.GetComponentByName("semat1") as StiText).Enabled = false;
                (report.GetComponentByName("SDI1") as StiImage).Enabled = false;
            }

            if (SGN2.IsChecked == true)
            {
                var SAL_NAME = sgn2usid.Text;

                (report.GetComponentByName("nem2") as StiText).Enabled = true;
                (report.GetComponentByName("semat2") as StiText).Enabled = true;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = true;

                (report.GetComponentByName("nem2") as StiText).Text = SGN2_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat2") as StiText).Text = SGN2_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem2") as StiText).Enabled = false;
                (report.GetComponentByName("semat2") as StiText).Enabled = false;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = false;
            }

            if (SGN3.IsChecked == true)
            {
                var SAL_NAME = sgn3usid.Text;

                (report.GetComponentByName("nem3") as StiText).Enabled = true;
                (report.GetComponentByName("semat3") as StiText).Enabled = true;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = true;

                (report.GetComponentByName("nem3") as StiText).Text = SGN3_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat3") as StiText).Text = SGN3_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem3") as StiText).Enabled = false;
                (report.GetComponentByName("semat3") as StiText).Enabled = false;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = false;
            }
            #endregion

            var Saman_Name = dbms.DoGetDataSQL<string>("SELECT NAME FROM SAZMAN").FirstOrDefault();
            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D;
            (report.GetComponentByName("Text11") as StiText).Text = $"تاریخ : {DATE.Text.ToRawTarikh()}";


            report["NAMBER_PARM"] = ID.Text;

            #region Report_Open

            double JCHK;
            double jamf;
            double HAZ, NAGHD, VAR, HAV, taf;
            jamf = 0d;
            HAZ = 0d;
            NAGHD = 0d;
            VAR = 0d;
            HAV = 0d;
            taf = 0d;
            var rst = dbms.DoGetDataSQL<_QR1>("SELECT SUM(dbo.PGET_LST.MABL) AS naghd, dbo.PGET_LST.FHES_K, dbo.PGET_LST.FHES_M, dbo.PGET_LST.FHES_T, dbo.PGET_LST.FHES, dbo.CUST_HESAB.NAME , dbo.PGET_HED.MOLAH FROM dbo.PGET_LST INNER JOIN dbo.PGET_HED ON dbo.PGET_LST.ID = dbo.PGET_HED.ID AND dbo.PGET_LST.DATE = dbo.PGET_HED.DATE INNER JOIN dbo.CUST_HESAB ON dbo.PGET_LST.FHES = dbo.CUST_HESAB.hes WHERE     (dbo.PGET_LST.NO_AM = 1) AND (dbo.PGET_LST.ID = " + ID.Text + ") AND (dbo.PGET_LST.N_SERI IS NULL) AND (dbo.PGET_LST.BANK IS NULL) GROUP BY dbo.PGET_LST.FHES_K, dbo.PGET_LST.FHES_M, dbo.PGET_LST.FHES_T, dbo.PGET_LST.FHES, dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH").ToList();
            if (rst.Count > 0)
            {
                jamf = Convert.ToDouble(rst.FirstOrDefault().NAGHD);
            }
            else
            {
                jamf = 0d;
            }
            var JST = dbms.DoGetDataSQL<_QR2>("SELECT SUM(dbo.PGET_LST.MABL) AS CHK, dbo.PGET_LST.FHES_K, dbo.PGET_LST.FHES_M, dbo.PGET_LST.FHES_T, COUNT(dbo.PGET_LST.ID) AS TEDAD,  dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH FROM dbo.PGET_LST INNER JOIN dbo.PGET_HED ON dbo.PGET_LST.ID = dbo.PGET_HED.ID AND dbo.PGET_LST.DATE = dbo.PGET_HED.DATE INNER JOIN dbo.CUST_HESAB ON dbo.PGET_LST.FHES = dbo.CUST_HESAB.hes WHERE     (dbo.PGET_LST.NO_AM = 1) AND ((dbo.PGET_LST.NAHVA = 2)OR (dbo.PGET_LST.NAHVA > 3)) AND (dbo.PGET_LST.ID = " + ID.Text + ")GROUP BY dbo.PGET_LST.FHES_K, dbo.PGET_LST.FHES_M, dbo.PGET_LST.FHES_T, dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH").ToList();
            if (JST.Count > 0)
            {
                JCHK = Convert.ToDouble(JST.FirstOrDefault().CHK);
            }
            else
            {
                JCHK = 0d;
            }
            (report.GetComponentByName("JF") as StiText).Text = Strings.Format(JCHK + jamf, "#,##0;#,##0-");

            if (jamf != 0d & JCHK != 0d)
            {
                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(jamf, "#,###") + "  " + CL_HESABDARI.ALPHANUM(Convert.ToInt64(jamf)) + " " + " ريال نـــقد        و    مبلغ: " + Strings.Format(JCHK, "#,###") + "  " + CL_HESABDARI.ALPHANUM(Convert.ToInt64(JCHK)) + " ريال چـــك       طي " + JST.FirstOrDefault().TEDAD + " فقره  به شرح ذيل از " + JST.FirstOrDefault().NAME + Interaction.IIf(JST.FirstOrDefault().MOLAH == "" || IsNull(JST.FirstOrDefault().MOLAH), " ", " بابت " + JST.FirstOrDefault().MOLAH) + " دريافت گرديد ";
            }
            else if (jamf == 0d & JCHK != 0d)
            {
                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(JCHK, "#,###") + "  " + CL_HESABDARI.ALPHANUM(Convert.ToInt64(JCHK)) + " ريال چـــك      طي " + JST.FirstOrDefault().TEDAD + "  فقره به شرح ذيل از " + JST.FirstOrDefault().NAME + Interaction.IIf(JST.FirstOrDefault().MOLAH == "" || IsNull(JST.FirstOrDefault().MOLAH == ""), " ", " بابت " + JST.FirstOrDefault().MOLAH) + " دريافت گرديد ";
            }
            else if (jamf != 0d & JCHK == 0d)
            {
                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(jamf, "#,###") + "  " + CL_HESABDARI.ALPHANUM(Convert.ToInt64(jamf)) + " " + " ريال نـــقد         از " + rst.FirstOrDefault().NAME + Interaction.IIf(rst.FirstOrDefault().MOLAH == "" || IsNull(rst.FirstOrDefault().MOLAH), " ", " بابت " + rst.FirstOrDefault().MOLAH) + " دريافت گرديد ";

                //غیر فعال شدن جدول چک

                (report.GetComponentByName("DataBand1") as StiDataBand).Enabled = false;
                (report.GetComponentByName("Text18") as StiText).Enabled = false;
                (report.GetComponentByName("Text9") as StiText).Enabled = false;
                (report.GetComponentByName("Text8") as StiText).Enabled = false;
                (report.GetComponentByName("Text7") as StiText).Enabled = false;
                (report.GetComponentByName("Text23") as StiText).Enabled = false;
                (report.GetComponentByName("Text17") as StiText).Enabled = false;
                (report.GetComponentByName("Text5") as StiText).Enabled = false;
                (report.GetComponentByName("RectanglePrimitive3") as StiRectanglePrimitive).Enabled = false;
                (report.GetComponentByName("JF") as StiText).Enabled = false;
                (report.GetComponentByName("HR2") as StiText).Enabled = false;
                (report.GetComponentByName("Label") as StiText).Enabled = false;
                (report.GetComponentByName("Text3") as StiText).Enabled = false;
                (report.GetComponentByName("VerticalLinePrimitive13") as StiVerticalLinePrimitive).Enabled = false;

            }
            string path = @"C:\CORRECT\test.txt";
            File.AppendAllText(path, $"\n   JCHK{JCHK} | jamf {jamf}"); //Log

            string TESTI = CL_HESABDARI.ALPHANUM(Convert.ToInt64(JCHK) + Convert.ToInt64(jamf));
            File.AppendAllText(path, $"\n  {TESTI}"); //Log

            (report.GetComponentByName("HR2") as StiText).Text = CL_HESABDARI.ALPHANUM(Convert.ToInt64(JCHK) + Convert.ToInt64(jamf)) + " " + "ريال ";
            // Set rst = New ADODB.Recordset
            // rst.Open "SELECT     dbo.SALA_DTL.EMZA AS emza1, SALA_DTL_1.EMZA AS emza2, SALA_DTL_2.EMZA AS emza3 FROM dbo.SALA_DTL RIGHT OUTER JOIN  dbo.SALA_DTL SALA_DTL_1 RIGHT OUTER JOIN dbo.PGET_HED LEFT OUTER JOIN dbo.SALA_DTL SALA_DTL_2 ON dbo.PGET_HED.sgn3usid = SALA_DTL_2.IDD ON SALA_DTL_1.IDD = dbo.PGET_HED.sgn2usid ON dbo.SALA_DTL.IDD = dbo.PGET_HED.sgn1usid where id = " & Forms![PGET_HED]![ID], CurrentProject.Connection, adOpenKeyset, adLockOptimistic
            // If rst.RecordCount > 0 Then
            // Me.EMZA1 = rst.Fields("emza1")
            // Me.EMZA2 = rst.Fields("emza2")
            // Me.EMZA3 = rst.Fields("emza3")
            // End If
            #endregion

            //report.Compile();

            report.Render(false);

            ProcLoader.Stop(Prc);
            report.ShowWithWpf();

        }

        private void Command24_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                return;
            }

            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Khazane_Reports.SANAD_PARDAKHT.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            //Parameters
            report["NAMBER_PARM"] = ID.Text;
            (report.GetComponentByName("SAL_N") as StiText).Text = Baseknow.WIDTH_D;
            (report.GetComponentByName("Text11") as StiText).Text = $"تاریخ : {DATE.Text.ToRawTarikh()}";

            #region EMZA
            if (SGN1.IsChecked == true)
            {
                //var SAL_NAME = ((TextBox)sgn1usid.Template.FindName("PART_EditableTextBox", sgn1usid)).Text;
                var SAL_NAME = sgn1usid.Text;

                (report.GetComponentByName("nem1") as StiText).Enabled = true;
                (report.GetComponentByName("semat1") as StiText).Enabled = true;
                (report.GetComponentByName("SDI1") as StiImage).Enabled = true;



                (report.GetComponentByName("nem1") as StiText).Text = SGN1_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat1") as StiText).Text = SGN1_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem1") as StiText).Enabled = false;
                (report.GetComponentByName("semat1") as StiText).Enabled = false;
                (report.GetComponentByName("SDI1") as StiImage).Enabled = false;
            }

            if (SGN2.IsChecked == true)
            {
                var SAL_NAME = sgn2usid.Text;

                (report.GetComponentByName("nem2") as StiText).Enabled = true;
                (report.GetComponentByName("semat2") as StiText).Enabled = true;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = true;

                (report.GetComponentByName("nem2") as StiText).Text = SGN2_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat2") as StiText).Text = SGN2_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem2") as StiText).Enabled = false;
                (report.GetComponentByName("semat2") as StiText).Enabled = false;
                (report.GetComponentByName("SDI2") as StiImage).Enabled = false;
            }

            if (SGN3.IsChecked == true)
            {
                var SAL_NAME = sgn3usid.Text;

                (report.GetComponentByName("nem3") as StiText).Enabled = true;
                (report.GetComponentByName("semat3") as StiText).Enabled = true;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = true;

                (report.GetComponentByName("nem3") as StiText).Text = SGN3_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat3") as StiText).Text = SGN3_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nem3") as StiText).Enabled = false;
                (report.GetComponentByName("semat3") as StiText).Enabled = false;
                (report.GetComponentByName("SDI3") as StiImage).Enabled = false;
            }
            #endregion


            #region Report_Open
            double JCHK;
            double jamf;
            double HAZ, NAGHD, VAR, HAV, taf;
            jamf = 0d;
            HAZ = 0d;
            NAGHD = 0d;
            VAR = 0d;
            HAV = 0d;
            taf = 0d;
            var rst = dbms.DoGetDataSQL<_QR1>("SELECT SUM(dbo.PGET_LST.MABL) AS naghd, dbo.PGET_LST.THES_K, dbo.PGET_LST.THES_M, dbo.PGET_LST.THES_T, dbo.PGET_LST.THES, dbo.CUST_HESAB.NAME , dbo.PGET_HED.MOLAH FROM dbo.PGET_LST INNER JOIN dbo.PGET_HED ON dbo.PGET_LST.ID = dbo.PGET_HED.ID AND dbo.PGET_LST.DATE = dbo.PGET_HED.DATE INNER JOIN dbo.CUST_HESAB ON dbo.PGET_LST.THES = dbo.CUST_HESAB.hes WHERE     (dbo.PGET_LST.NO_AM = 2) And (dbo.PGET_LST.ID = " + ID.Text + ") And (dbo.PGET_LST.N_SERI Is Null) And (dbo.PGET_LST.BANK Is Null) GROUP BY dbo.PGET_LST.THES_K, dbo.PGET_LST.THES_M, dbo.PGET_LST.THES_T, dbo.PGET_LST.THES, dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH").ToList();

            if (rst.Count > 0)
            {
                jamf = Convert.ToDouble(rst.FirstOrDefault().NAGHD);
            }
            else
            {
                jamf = 0d;
            }
            var JST = dbms.DoGetDataSQL<_QR2>("SELECT SUM(dbo.PGET_LST.MABL) AS CHK, dbo.PGET_LST.THES_K, dbo.PGET_LST.THES_M, dbo.PGET_LST.THES_T, COUNT(dbo.PGET_LST.ID) AS TEDAD,  dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH FROM dbo.PGET_LST INNER JOIN dbo.PGET_HED ON dbo.PGET_LST.ID = dbo.PGET_HED.ID AND dbo.PGET_LST.DATE = dbo.PGET_HED.DATE INNER JOIN dbo.CUST_HESAB ON dbo.PGET_LST.THES = dbo.CUST_HESAB.hes WHERE     (dbo.PGET_LST.NO_AM = 2) AND ((dbo.PGET_LST.NAHVA = 2)OR (dbo.PGET_LST.NAHVA > 3)) AND (dbo.PGET_LST.ID = " + ID.Text + ")GROUP BY dbo.PGET_LST.THES_K, dbo.PGET_LST.THES_M, dbo.PGET_LST.THES_T, dbo.CUST_HESAB.NAME, dbo.PGET_HED.MOLAH").ToList();
            if (JST.Count > 0)
            {
                JCHK = Convert.ToDouble(JST.FirstOrDefault().CHK);
            }
            else
            {
                JCHK = 0d;
            }
            (report.GetComponentByName("JF") as StiText).Text = Strings.Format(JCHK + jamf, "#,##0;#,##0-");

            if (jamf != 0d & JCHK != 0d)
            {
                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(jamf, "#,###") + "  " + CL_HESABDARI.ALPHANUM(jamf) + " " + " ريال نـــقد        و    مبلغ: " + Strings.Format(JCHK, "#,###") + "  " + CL_HESABDARI.ALPHANUM(JCHK) + " ريال چـــك       طي " + JST.FirstOrDefault().TEDAD + " فقره  به شرح ذيل به " + JST.FirstOrDefault().NAME + Interaction.IIf(JST.FirstOrDefault().MOLAH == "" || IsNull(JST.FirstOrDefault().MOLAH), " ", " بابت " + JST.FirstOrDefault().MOLAH) + " پرداخت گرديد ";
            }
            else if (jamf == 0d & JCHK != 0d)
            {
                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(JCHK, "#,###") + "  " + CL_HESABDARI.ALPHANUM(JCHK) + " ريال چـــك      طي " + JST.FirstOrDefault().TEDAD + "  فقره به شرح ذيل به " + JST.FirstOrDefault().NAME + Interaction.IIf(JST.FirstOrDefault().MOLAH == "" || IsNull(JST.FirstOrDefault().MOLAH == ""), " ", " بابت " + JST.FirstOrDefault().MOLAH) + " پرداخت گرديد ";
            }
            else if (jamf != 0d & JCHK == 0d)
            {
                string temp = (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(jamf, "#,###") + "  " + CL_HESABDARI.ALPHANUM(jamf) + " " + " ريال نـــقد         به " + rst.FirstOrDefault().NAME + Interaction.IIf(rst.FirstOrDefault().MOLAH == "" || IsNull(rst.FirstOrDefault().MOLAH), " ", " بابت " + rst.FirstOrDefault().MOLAH) + " پرداخت گرديد ";

                (report.GetComponentByName("HR") as StiText).Text = "مبلغ: " + Strings.Format(jamf, "#,###") + "  " + CL_HESABDARI.ALPHANUM(jamf) + " " + " ريال نـــقد         به " + rst.FirstOrDefault().NAME + Interaction.IIf(rst.FirstOrDefault().MOLAH == "" || IsNull(rst.FirstOrDefault().MOLAH), " ", " بابت " + rst.FirstOrDefault().MOLAH) + " پرداخت گرديد ";

                //غیر فعال شدن جدول چک
                (report.GetComponentByName("DataBand1") as StiDataBand).Enabled = false;
                (report.GetComponentByName("Text18") as StiText).Enabled = false;
                (report.GetComponentByName("Text9") as StiText).Enabled = false;
                (report.GetComponentByName("Text8") as StiText).Enabled = false;
                (report.GetComponentByName("Text7") as StiText).Enabled = false;
                (report.GetComponentByName("Text23") as StiText).Enabled = false;
                (report.GetComponentByName("Text17") as StiText).Enabled = false;
                (report.GetComponentByName("Text5") as StiText).Enabled = false;
                (report.GetComponentByName("RectanglePrimitive3") as StiRectanglePrimitive).Enabled = false;
                (report.GetComponentByName("JF") as StiText).Enabled = false;
                (report.GetComponentByName("HR2") as StiText).Enabled = false;
                (report.GetComponentByName("Label") as StiText).Enabled = false;
                (report.GetComponentByName("Text3") as StiText).Enabled = false;
                (report.GetComponentByName("VerticalLinePrimitive13") as StiVerticalLinePrimitive).Enabled = false;

            }
            #endregion

            //report.Compile();

            report.Render(false);

            ProcLoader.Stop(Prc);
            report.ShowWithWpf();
        }
        #endregion

        private void PERSONEL_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (PERSONEL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            //CL_HESABDARI.PERSONELUpdate(34, Convert.ToDouble(ID.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'خزانه داري   شماره: " + ID.Text + " مورخ " + string.Format(DATE.Text.ToRawTarikh(), "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'");
            //Msgwin msgwin = new Msgwin(false, "ارجاع داده شد.");
            //msgwin.ShowDialog();
        }

        private void FocusCell(int rowIndex, string columnName)
        {
            //PGET_LST_SUB.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (CL_LMethods.IsValidIndex(PGET_LST_SUB, rowIndex))
            //    {
            //        PGET_LST_SUB.SelectedIndex = rowIndex;
            //        PGET_LST_SUB.CurrentCell = new DataGridCellInfo(PGET_LST_SUB.Items[rowIndex], PGET_LST_SUB.Columns.First(c => c.SortMemberPath == columnName));
            //        //PGET_LST_SUB.BeginEdit();

            //    }
            //}), DispatcherPriority.Background);
        }

        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.NewItem);
            DATE.Focus();
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.LastItem);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.NextItem);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(Jahat.BackItem);
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(Jahat.FirstItem);
        }
        private void SERVERRELOAD_Btn_Click(object sender, RoutedEventArgs e)
        {
            ReGetMasterData();
        }

        /// <summary>
        /// 0 = First  |
        /// 1 = Back ↑ |
        /// 2 = Next ↓ |
        /// 3 = Last   |
        /// </summary>
        /// <param name="dtg"></param>
        /// <param name="wich"></param>
        /// 
        private void MovingDG(DataGrid DTG, byte? ArrowDirect, int? CustomRow = null)
        {
            if (DTG == null || DTG.Items.Count == 0) return;

            int targetIndex = 0;

            // Adjust for the extra empty row at the end
            int adjustedItemCount = DTG.Items.Count - 1;

            switch (ArrowDirect)
            {
                case 0: // First record.
                    targetIndex = 0;
                    break;
                case 1: // Previous record.
                    targetIndex = Math.Max(0, DTG.SelectedIndex - 1);
                    break;
                case 2: // Next record.
                    targetIndex = Math.Min(adjustedItemCount - 1, DTG.SelectedIndex + 1);
                    break;
                case 3: // Last record.
                    targetIndex = adjustedItemCount - 1;
                    break;
            }

            if (CustomRow is not null) //Custom Row Index Called
            {
                if (CustomRow > 0)
                {
                    targetIndex = Convert.ToInt32(CustomRow - 1);
                }
            }

            // Check if the targetIndex is within the valid range
            if (targetIndex >= 0 && targetIndex < DTG.Items.Count)
            {
                try
                {
                    // Scroll the item into view and select it.
                    DTG.SelectedIndex = targetIndex;
                    DTG.ScrollIntoView(DTG.Items[targetIndex]);

                    // Ensure selection and focus are appropriately set.
                    DTG.Dispatcher.InvokeAsync(() =>
                    {
                        DTG.Focus();
                        if ((DataGridRow)DTG.ItemContainerGenerator.ContainerFromIndex(targetIndex) is null)
                        {
                            DTG.UpdateLayout(); // Force layout update
                        }
                        DTG.SelectedItem = DTG.Items[targetIndex];

                        var ColumnIndexy = 0;
                        if (DTG.CurrentColumn is not null)
                        {
                            ColumnIndexy = DTG.CurrentColumn.DisplayIndex;
                        }
                        else
                        {
                            int? defaultcolumnindex = DTG.Columns.FirstOrDefault(c => c.Visibility == Visibility.Visible && !c.IsReadOnly)?.DisplayIndex;
                            ColumnIndexy = Convert.ToInt32(defaultcolumnindex);
                        }
                        DTG.CurrentCell = new DataGridCellInfo(DTG.SelectedItem, DTG.Columns[ColumnIndexy]);

                        // It may not be always necessary, or even desired, to force the DataGrid row to focus.
                        // This logic attempts to focus the row only if the DataGrid is supposed to have focus.
                        //if (DTG.IsKeyboardFocusWithin)
                        //{
                        //}
                        DataGridRow dgRow = (DataGridRow)DTG.ItemContainerGenerator.ContainerFromIndex(targetIndex);
                        dgRow?.Focus();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch { }
            }
        }

        private void Clear_PGET_HED()
        {
            DATE.Text = null;
            ID.Text = "0";
            N_S.Text = "0";
            MABNA.Text = "0";
            KIND.SelectedValue = null;
            IDK.Text = "";
            USER_NAME.Text = Baseknow.UUSER;
            MOLAH.Text = null;
            KIND.SelectedIndex = 0; KIND.Items.Refresh();
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER; SHIFT.Items.Refresh(); //شیفت
            Text10.Text = null;
            Text8.Text = null;
            MANDS.Text = null;
            MANDB.Text = null;

            MABL.Text = "0";
            OKF.IsChecked = false;

            _sgn1_info.USER_SEMAT = null;
            _sgn1_info.USER_HESAB_NAME = null;
            _sgn2_info.USER_SEMAT = null;
            _sgn2_info.USER_HESAB_NAME = null;
            _sgn3_info.USER_SEMAT = null;
            _sgn3_info.USER_HESAB_NAME = null;

            sgn1usid.Text = null; sgn1usid.Tag = null; SGN1.IsChecked = false;
            sgn2usid.Text = null; sgn2usid.Tag = null; SGN2.IsChecked = false;
            sgn3usid.Text = null; sgn3usid.Tag = null; SGN3.IsChecked = false;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            KHAZANEH_DATA.Clear();

            PGET_LST_SUB.IsReadOnly = true;

            AllowEdits = true;
        }

        private void New_PGET_HED_Click(object sender, RoutedEventArgs e)
        {
            Clear_PGET_HED();

            Window_Loaded(null, null);

            DATE.Focus();
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            if (ID.Text is null || ID.Text == "0" || PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }
            CL_HESABDARI.PERSONELUpdate(34, Convert.ToDouble(ID.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'خزانه داري   شماره: " + ID.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + "','" + CL_HESABDARI.GETUSERHES(Convert.ToInt32(Baseknow.USERCOD)) + "'");

            universControl.PopNotifyShow("ارجاع داده شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void Khazane_List_Button_Click(object sender, RoutedEventArgs e)
        {
            //PGET_HED_LIST pGET_HED_LIST = new PGET_HED_LIST();
            //this.Close();
            //pGET_HED_LIST.ShowDialog();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_LST_SEARCH, this);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private bool IsAllowEditDataGrid()
        {
            if (PGET_LST_SUB.IsEnabled && PGET_LST_SUB.IsReadOnly == false)
            {
                return true;
            }

            return false;
        }
        private void GPAYCHECK_Click(object sender, RoutedEventArgs e)
        {
            if (PGET_LST_SUB.IsEnabled == true && !NewRecord && IsAllowEditDataGrid())
            {
                CREATE_CHEKPDP cREATE_CHEKPDP = new CREATE_CHEKPDP(I_AM_KHAZANEH, CURRENT_ROW_INDEX);
                cREATE_CHEKPDP.ShowDialog();

                //var grid = sender as DataGrid;
                //if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null && PGET_LST_SUB.SelectedIndex > -1)
                //{
                //    if (grid.CurrentCell.Column.SortMemberPath == "SHARH") //چک پرداختی گروهی
                //    {

                //    }
                //}
            }
        }
        private void GDCHECK_Click(object sender, RoutedEventArgs e)
        {
            if (PGET_LST_SUB.IsEnabled == true && !NewRecord && IsAllowEditDataGrid())
            {
                CREATE_CHEKDP cREATE_CHEKDP = new CREATE_CHEKDP(I_AM_KHAZANEH, CURRENT_ROW_INDEX);
                cREATE_CHEKDP.ShowDialog();
            }
        }
        private bool IsSubDataNull()
        {
            if (PGET_LST_SUB != null && PGET_LST_SUB?.Items?.Count > 0 && KHAZANEH_DATA?.Count > 0)
            {
                return false;
            }

            return true;
        }
        private void F8_CUSTOMER_Click(object sender, RoutedEventArgs e)
        {
            var grid = PGET_LST_SUB;

            if (grid == null || !grid.IsEnabled) return;

            // Update CurrentCell based on mouse position
            UpdateCurrentCellBasedOnMousePosition(grid);

            if (grid?.CurrentCell == null || grid.CurrentCell.Column == null || grid.SelectedIndex < 0) return;

            if (!CL_LMethods.IsValidIndex(grid, grid.SelectedIndex)) return;

            var CurrentData = grid.Items[grid.SelectedIndex] as PGET_LST;

            if (CurrentData != null)
            {
                if (grid.CurrentCell.Column.SortMemberPath == "THES") // لیست صورت حساب به حساب
                {
                    if (CurrentData.THES is not null)
                    {
                        new F_MENU_KOL_MOIN_TAFZIL(CurrentData.THES.ToString());
                    }
                }
                else if (grid.CurrentCell.Column.SortMemberPath == "FHES") // لیست صورت حساب از حساب
                {
                    if (CurrentData.FHES is not null)
                    {
                        new F_MENU_KOL_MOIN_TAFZIL(CurrentData.FHES.ToString());
                    }
                }
            }
        }



        private void PGET_LST_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                IsDataGrid_IsFocused = false;
            }
            else
            {
                IsDataGrid_IsFocused = true;
            }
        }

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            if (IsSubDataNull())
            {
                return;
            }

            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(PGET_LST_SUB, "DGExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }

        private void COPY_CLICK(object sender, RoutedEventArgs e)
        {
            if (IsSubDataNull())
            {
                return;
            }

            var isEditing = ((IEditableCollectionView)PGET_LST_SUB.Items).IsEditingItem;
            if (!isEditing)
            {
                e.Handled = true;
                DataGridClipboardManager.CopySelectedItems<PGET_LST>(PGET_LST_SUB);
            }
            else
            {
                var editingElement = CL_LMethods.FindChild<TextBox>(PGET_LST_SUB);
                if (editingElement != null)
                {
                    if (!string.IsNullOrEmpty(editingElement.SelectedText))
                    {
                        Clipboard.SetText(editingElement.SelectedText);
                    }
                }
            }
        }

        private void PASTE_CLICK(object sender, RoutedEventArgs e)
        {
            if (PGET_LST_SUB.SelectedItem != null || PGET_LST_SUB.SelectedItems.Count > 0)
            {
                var isEditing = ((IEditableCollectionView)PGET_LST_SUB.Items).IsEditingItem;
                if (!isEditing && !PGET_LST_SUB.IsReadOnly && PGET_LST_SUB.IsEnabled)
                {
                    e.Handled = true;

                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<PGET_LST>(PGET_LST_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;

                    PGET_LST_SUB.CommitEdit();
                }
                else
                {
                    //System.Windows.Forms.SendKeys.SendWait("^v");

                    // Execute the Paste command on the currently focused element
                    if (ApplicationCommands.Paste.CanExecute(null, Keyboard.FocusedElement as IInputElement))
                    {
                        ApplicationCommands.Paste.Execute(null, Keyboard.FocusedElement as IInputElement);
                    }
                }
            }
            else
            {
                universControl.PopNotifyShowUp("عمل انتقال کپی را باید با راست کلیک روی یک سطر خالی انجام بدید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
            }
        }

        private void PGET_LST_SUB_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }

        private void PGET_LST_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null) return;

            try
            {
                // Find the row under the mouse
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;
                if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
                {
                    // Select the row under the mouse
                    dataGrid.SelectedItem = row.Item;

                    // Show the context menu
                    dataGrid.ContextMenu.IsOpen = true;

                    // Mark the event as handled to prevent the default context menu behavior
                    e.Handled = true;
                }
                else
                {
                    // No valid row, don't show context menu
                    var isEditing = ((IEditableCollectionView)PGET_LST_SUB.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void N_S_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
            {
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), false);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. یافتن سطر انتخاب شده
                // اگر دکمه داخل دیتاگرید است:
                var btn = sender as FrameworkElement; // یا MenuItem اگر در کلیک راست است
                var rowItem = btn?.DataContext as PGET_LST;

                // اگر سطر انتخاب شده نال بود (مثلا از طریق ContextMenu روی سطر خالی کلیک شده)
                if (rowItem == null)
                {
                    if (PGET_LST_SUB.SelectedItem is PGET_LST selected)
                    {
                        rowItem = selected;
                    }
                    else
                    {
                        return;
                    }
                }

                // 2. اعتبارسنجی
                if (rowItem.IDH == null || rowItem.IDH <= 0)
                {
                    new Msgwin(false, "این سطر هنوز ذخیره نشده و تصویری ندارد.").ShowDialog();
                    return;
                }

                long rowIdH = Convert.ToInt64(rowItem.IDH);
                //double currentHeadId = Convert.ToDouble(ID.Text);

                // 3. پیدا کردن شناسه تسک (پرونده اتوماسیون)
                long taskId = CL_HESABDARI.Gettaskid(rowIdH, 34); // 34 = کد فرم خزانه

                if (taskId <= 0)
                {
                    new Msgwin(false, "پرونده اتوماسیون برای این سند یافت نشد.").ShowDialog();
                    return;
                }

                string sqlCheck = $"SELECT TOP 1 IDD FROM dbo.EVENTS WHERE IDNUM = {taskId} AND num = {rowIdH} AND pic IS NOT NULL";
                var eventId = dbms.DoGetDataSQL<int?>(sqlCheck).FirstOrDefault();

                if (eventId == null || eventId <= 0)
                {
                    new Msgwin(false, "تصویری برای این سطر یافت نشد.").ShowDialog();
                    return;
                }

                // 5. گرفتن تأییدیه از کاربر
                Msgwin confirmDlg = new Msgwin(true, "آیا از حذف تصویر ضمیمه شده برای این سطر اطمینان دارید؟");
                confirmDlg.ShowDialog();

                if (confirmDlg.DialogResult == true)
                {
                    _ = AuditLogger.LogActionAsync(
                          actionType: "DELETE",
                          tableName: "خزانه داری : حذف تصویر سطر",
                          recordId: rowItem.IDH.ToString(),
                          oldValue: $"ID = {ID.Text}",
                          newValue: null,
                          additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    dbms.DoExecuteSQL($"UPDATE dbo.EVENTS SET pic = NULL WHERE IDD = {eventId}");

                    // 7. بروزرسانی UI
                    rowItem.HasAttachment = false; // تغییر پراپرتی برای آپدیت شدن آیکون در گرید

                    universControl.PopNotifyShow("تصویر با موفقیت حذف شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در حذف تصویر: " + ex.Message).ShowDialog();
            }
        }

    }
}
