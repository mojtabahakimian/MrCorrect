using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using Wins.WinMenus.ANBAR;
using Syncfusion.Data.Extensions;
using Rpts;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using System.Windows.Data;
using System.Threading.Tasks;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_UI.Functions.CL_LMethods;
using System.Windows.Controls.Primitives;

namespace Wins.WinMenus.SANATI
{
    public partial class HAVALAH_ENTER : Window, ISearchableWindow
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
        public HAVALAH_ENTER(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER.Text = number_to_open.ToString();
                NUMBER.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
        #region LOCALMODEL

        public class DeedHedData
        {
            public string BASE { get; set; }
            public bool GHATEI { get; set; }
        }
        public class VKSQRE1
        {
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public double? MABLKs { get; set; }
        }
        public class VKSQRE2
        {
            public string? CODE { get; set; }
            public int? FNUMB { get; set; }
            public string? CODB { get; set; }
            public int? ANBAR { get; set; }
            public double? MEGHk { get; set; }
            public int? VAHED_K { get; set; }
            public double? MEGH { get; set; }
            public double? PERT { get; set; }
            public double? smabl { get; set; }
            public double? MABLK { get; set; }
        }

        public class FSAKHT_COMBO
        {
            public double? FNUMB { get; set; }
            public string? Expr1 { get; set; }
        }
        #endregion

        private double sum_of_megh_k = 0;
        public double SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MEGHk ?? 0);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        private NavigationManager<HEAD_LST> _navigationManager;
        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        /// <summary>
        /// TAG = 9
        /// </summary>
        public byte FTAG { get; } = 9;

        public bool NowIsReady { get; private set; }
        public bool INVO_LST_SUB_IsFocused { get; private set; }

        private bool _newrecord;
        public bool NewRecord
        {
            get
            {
                if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
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

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODE")?.DisplayIndex;
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
        public string? ENTERED_VALUE_ROW { get; private set; }
        public INVO_LST_FACTOR22? CURRENT_ITEMS_ROW { get; private set; }
        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; }
        public INVO_LST_FACTOR22 FROM_SEARCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;

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

                //فاکتور
                DATE_N.IsReadOnly = !ican;// تاریخ
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ

                //INVO_LST_SUB.IsReadOnly = !ican;

                //__ENABLEY
                FNUMCO.IsEnabled = ican;

                DATE_N.IsEnabled = ican;// تاریخ
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ

                BTN_SAVE.IsEnabled = ican;

            }
        }

        public int ANBARDefaultValue { get; private set; }
        public double Meidnum { get; private set; }
        public Visual I_AM_VK_SAKHTEH { get; private set; }
        public List<FSAKHT_COMBO> N_KOL_ALL { get; private set; }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is HEAD_LST item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER.Equals(Convert.ToDouble(item.NUMBER)));
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;

                        // 1) Find its index in the master list
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx < 0)
                        {
                            // not found (perhaps filtered out?), bail out
                            new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                            return;
                        }

                        // 2) Tell the navigation manager to move to that position
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                        //OnCurrentRecordChanged(itemfound);
                    }
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره برگه", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مسئول شیفت", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VK_SAKHTEH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            #region Form_Open
            if (Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
            {
                N_KOL_COLUMN.Visibility = Visibility.Visible;
            }
            else
            {
                N_KOL_COLUMN.Visibility = Visibility.Hidden;
            }
            #endregion

            //Load Existing Factor
            if (!string.IsNullOrEmpty(NUMBER.Text))
            {
                if (Convert.ToDouble(NUMBER.Text) > 0)
                {
                    //var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();

                    //if (HEADER_FAC == null)
                    //{
                    //    new Msgwin(false, "چنین شماره ای وجود ندارد !").ShowDialog();
                    //    this.Close();
                    //    return;
                    //}



                }
            }

            string WhereCondition = $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(Convert.ToByte(FTAG), WhereCondition);
            
            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";
            }

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
            x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {FTAG}", //On Change for One Record
            Convert.ToDouble(NUMBER.Text)
            );

            // Link the navigation manager to the universal control
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            // Hook up the OnInsertRecord event
            _navigationManager.OnInsertRecord += OnInsertRecord;

            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            //Form_Current();

            if (!NewRecord)
            {
                AllowEdits = false;
            }

            CL_LMethods.SetTabIndexes(
             DATE_N,
             FNUMCO,
             CUST_NO,
             MOLAH,
             BTN_SAVE,
             INVO_LST_SUB
             );

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, INVO_LST_FACTOR22_DATA);
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                NUMBER.Text = HEADER_FAC.NUMBER.ToString();

                DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر

                FNUMCO.Text = string.IsNullOrEmpty(HEADER_FAC?.FNUMCO.ToStringNullSafe()) ? "0" : HEADER_FAC?.FNUMCO.ToStringNullSafe(); //شماره داخلی

                string thevalue = HEADER_FAC.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();
                if (!string.IsNullOrEmpty(data?.NAME))
                {
                    if (CUST_NO.ItemsSource == null)
                    {
                        CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                    }

                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = HEADER_FAC.CUST_NO; //مشتری
                    CUST_NO.Items.Refresh();
                }
                OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور
                MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات
                BTN_SAVE.IsEnabled = false;
                ItwasNewFirstTime = false; //Reset for Sanad Concurrency at first insert
                INVO_LST_SUB_ReGetData();
                GetBalanceInfo();

                Form_Current();
            }
        }
        private void RefreshAfterUpdate()
        {
            NewRecord = false;

            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void MakeDefaultFocuseReady()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }
        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                INVO_LST_SUB.IsReadOnly = false;
            }

            //SecurityAllCheck();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (INVO_LST_SUB_IsFocused)
                {
                    try
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[INVO_LST_SUB_DEF_INDEX_COL]);

                                    //Dispatcher.BeginInvoke(new Action(() =>
                                    //{
                                    //    DG.BeginEdit();
                                    //}), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    catch { /*ignore*/ }

                }
                else if (BTN_SAVE.IsFocused)
                {
                    BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.S || e.SystemKey == Key.S))
                {
                    e.Handled = true;
                    BTN_SAVE_Click(null, null);
                }
            }

            if (!INVO_LST_SUB.IsKeyboardFocusWithin && !INVO_LST_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
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

        private void GetFocusOnDefaultCell()
        {
            var DG = INVO_LST_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "HENTER", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_SUB, "HENTER");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
        }
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
                if (rst.Count > 0)
                {
                    ANBARDefaultValue = (int)rst.FirstOrDefault();

                    Baseknow.anbardef = ANBARDefaultValue;
                }
                else
                {
                    Baseknow.anbardef = Baseknow.DEFANB;
                }
            }
            else
            {
                Baseknow.anbardef = Baseknow.DEFANB;
            }
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            ANBAR_COLUMN.ItemsSource = ARST;
        }
        private void FILL_ALL_COMBOBOXES()
        {
            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //انبار کالا
            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //فرمول ساخت
            N_KOL_ALL = dbms.DoGetDataSQL<FSAKHT_COMBO>("SELECT HEAD_MANF.FNUMB, STUF_DEF.NAME + N' - ' + CAST(HEAD_MANF.DATE_ACTIV AS nvarchar) + N' :-' + ISNULL(HEAD_MANF.TOZIH, N' ') + CAST(HEAD_MANF.FNUMB AS char) AS Expr1 FROM HEAD_MANF INNER JOIN STUF_DEF ON HEAD_MANF.CODE = STUF_DEF.CODE").ToList();
            N_KOL_COLUMN.ItemsSource = N_KOL_ALL;

        }

        private bool IsNull(object? hTAF2)
        {
            string? _inputy = hTAF2?.ToStringNullSafe();
            if (string.IsNullOrEmpty(_inputy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //Validation
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = null;
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = null;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                DATE_N.Text = null;
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مسئول شیفت نمیتواند خالی باشد." });
            }

            if (!string.IsNullOrEmpty(DATE_N.Text?.ToRawTarikh()))
            {
                if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                {
                    //تاریخ صحیح نیست
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ فاکتور را بررسی کنید" });
                }
            }


            if (IsNull(this.CUST_NO.SelectedValue) || this.CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " مسئول شیفت مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مسئول شیفت مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
            }


            if (!IsNull(CUST_NO.SelectedValue))
            {
                if ((bool)Baseknow.SAGHF || (bool)(Baseknow.SAGHF2))
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false)
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.SelectedIndex = -1;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!" });
                    }
                }
            }

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }

            return true;
        }


        private bool BodyIsValid(INVO_LST_FACTOR22 TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // Validate ANBAR
            if (!int.TryParse(TheRow.ANBAR?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار صحیح انتخاب نشده" });
            }
            // Validate CODE
            if (string.IsNullOrEmpty(TheRow.CODE) || TheRow.CODE.Length > 15)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.NAME_CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کالا صحیح وارد نشده" });
            }
            // Validate MEGH
            if (!double.TryParse(TheRow.MEGH?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            // Validate MANDAH
            if (TheRow.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        public bool ItwasNewFirstTime { get; set; } = false;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();


            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            if (NUMBER.Text == "0")
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        //Fake Query for Lock Table
                        db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                        //Fake Query for Lock Table

                        var rst_11 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={FTAG}))", null, transaction).FirstOrDefault();
                        if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                        {
                            NUMBER.Text = Baseknow.STTOL.ToString(); //STTO ?
                            NUMBER.UpdateLayout();
                        }
                        else
                        {
                            NUMBER.Text = Convert.ToDouble(rst_11 + 1).ToString();
                            NUMBER.UpdateLayout();
                        }

                        db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER,         NUMBER1,           TAG,     DATE_N,  MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF)
                                               VALUES ({NUMBER.Text}, NULL    ,{FTAG},        0,    0,   0,       0,        0,        0,        0,    0   )", null, transaction);

                        transaction.Commit();
                        db?.Close();

                        ItwasNewFirstTime = true;

                        _navigationManager.IsNewRecord = false;
                        RefreshAfterUpdate();
                    }
                }
            }

            DoCmdHeaderSave();

            this.OKF.IsChecked = true;

            this.INVO_LST_SUB.IsReadOnly = false;

            if (!ItwasNewFirstTime) //برای جلوگیری از درج داده در صورت فوق همزمان برای درج جدید خالی در درجه اول سند نزنه
            {
                SANAD();
            }
            ItwasNewFirstTime = false; //ریست کردن این متفیری

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (INVO_LST_FACTOR22_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!IsNull(NUMBER.Text) && NUMBER.Text != "0")
            {
                SecurityAllCheck();

                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //1

                CUST_NO.IsEnabled = true; //Lock true
                INVO_LST_SUB.IsReadOnly = false;
                DATE_N.IsEnabled = true;
                NUMBER.IsEnabled = true;
                FNUMCO.IsEnabled = true;
                MOLAH.IsEnabled = true;

                BTN_SAVE.IsEnabled = true;

                this.AllowDeletions = true;
                this.AllowEdits = true;

                INVO_LST_SUB.IsReadOnly = false; // UnLocked
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                bool IsDeletedSomething = false;

                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "برگه ورود کالای ساخته شده",
                    recordId: NUMBER.Text,
                    oldValue: "TAG = 9",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                if (INVO_LST_FACTOR22_DATA.Count > 0 && INVO_LST_SUB.SelectedItems != null && INVO_LST_SUB.SelectedItems.Count > 0)
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                    CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //1
                    #endregion


                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < INVO_LST_SUB.SelectedItems.Count; i++)
                    {
                        var item = INVO_LST_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(INVO_LST_SUB, item))
                        {
                            continue; // Skip deletion for new placeholder items
                        }

                        var _id_ = item.GetType().GetProperty("id").GetValue(item);

                        if (_id_ != null)
                        {
                            try
                            {
                                var items = new List<object> { item }; // Wrap the item in a list
                                var (errorMessages, _, _, _) =
                                    IVM.CheckInventoryAndExecuteQuery<int>(items, $@"DELETE FROM dbo.INVO_LST WHERE id = {_id_}");

                                ErrosMessages.AddRange(errorMessages);

                                IsDeletedSomething = true;
                            }
                            catch (SqlException ex)
                            {
                                if (ex.Number == 547)
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                                }
                                else
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                                }
                            }
                            catch (Exception)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                            }

                        }
                    }

                    if (ErrosMessages.Any())
                    {
                        IVM.ShowErrorMessages(ErrosMessages);
                    }
                    else if (IsDeletedSomething)
                    {
                        INVO_LST_SUB_ReGetData();
                        SANAD();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER = {NUMBER.Text} AND TAG = {FTAG}");

                            SANAD();

                            //ClearFreshAll();
                            _navigationManager.DeleteCurrentRecord(); //Refresh Record Source
                        }
                        catch (SqlException ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این فاکتور دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                                return;
                            }
                            else
                            {
                                new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog(); return;
                            }
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                        }
                    }
                }
            }
        }
        private void GetBalanceInfo()
        {
            var _NUMBER1_ = dbms.DoGetDataSQL<double?>($"SELECT TOP (1) NUMBER1 FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (_NUMBER1_ != null)
            {
                NUMBER1.Text = _NUMBER1_?.ToString();
            }

            //کادر سبز و سند و مانده حساب
            var SANAD_NUMBER = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (SANAD_NUMBER != null)
            {
                N_S.Text = SANAD_NUMBER?.ToString();
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S = 9 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
            }
        }
        private bool DoCmdHeaderSave()
        {
            string _qre = null;

            string _n_s = "NULL";
            if (double.TryParse(N_S.Text, out var n_sVal) && n_sVal > 0)
            {
                _n_s = n_sVal.ToString();
            }

            _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()},
                    NUMBER1 = {(string.IsNullOrEmpty(NUMBER1.Text) ? "NULL" : NUMBER1.Text)},
                    N_S = {_n_s},
                    CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    FNUMCO = {(string.IsNullOrEmpty(FNUMCO.Text) ? "0" : FNUMCO.Text)},
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    USER_NAME = N'{USER_NAME.Text}'
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";

            _ = dbms.DoExecuteSQL(_qre);


            return true;
        }

        public void INVO_LST_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {
                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
																				 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
																			   	 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
																			 	 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
															FROM            dbo.INVO_LST LEFT OUTER JOIN
																				 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
																				 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
																				 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                         WHERE        (dbo.INVO_LST.TAG = {FTAG}) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                INVO_LST_FACTOR22_DATA?.Clear();
                foreach (var item in QRE_LST)
                    INVO_LST_FACTOR22_DATA?.Add(item);

            }
        }
        private void INVO_LST_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || INVO_LST_SUB == null || INVO_LST_SUB.CurrentCell == null)
                return;

            string CURRENT_COLUMN_NAME = "";
            if (INVO_LST_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = INVO_LST_SUB.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME == "MABL" || CURRENT_COLUMN_NAME == "MABL_K")
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
                if (CURRENT_COLUMN_NAME == "MABL" || CURRENT_COLUMN_NAME == "MABL_K")
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

        private void INVO_LST_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_SUB.SelectedItem != null)
            {
                if (INVO_LST_SUB.Items.Count > 0)
                    CURRENT_ROW_INDEX = INVO_LST_SUB.SelectedIndex;

                if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
                {
                    if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                    }
                }
            }
        }

        private void INVO_LST_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(INVO_LST_SUB.Items.Count < 1) && !(INVO_LST_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = INVO_LST_SUB.SelectedIndex;

                    GetCurrentMogudi();
                }
            }
        }
        private static readonly INVO_LST_FACTOR22 DefaultInvoice = new INVO_LST_FACTOR22(); //To avoid display error on new empty row that just clicked suddenly
        private void INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_SUB.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                INVO_LST_SUB.RowEditEnding -= INVO_LST_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVO_LST_SUB.CancelEdit();
                }
                else
                {
                    INVO_LST_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_SUB.RowEditEnding += INVO_LST_SUB_RowEditEnding;
                INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;
            });
        }
        private void INVO_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as INVO_LST_FACTOR22;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || CurrentRow?.ANBAR == null || string.IsNullOrEmpty(CurrentRow?.CODE))
            {
                return;
            }

            #region VAHED_K
            int? LastSelectedVahed = null; //پیش فرض واحد کالا انتخاب شده از قبل 
            if (CurrentRow?.VAHED_K != null)
            {
                LastSelectedVahed = (int)CurrentRow.VAHED_K;
            }

            if (e.Column.SortMemberPath == "VAHED_K") //اگر کاربر داخل واحد کالا بود
            {
                var COMBOBOX_VAHED_K = e.EditingElement as ComboBox;
                if (COMBOBOX_VAHED_K == null) return;

                // دریافت واحدهای فرعی کالا
                var filteredUnits = dbms.DoGetDataSQL<Custom_VAHEDK>(@$"SELECT DISTINCT VAHED, NAMES
                                                                FROM (
                                                                    SELECT dbo.TCOD_VAHEDS.CODE AS VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.TCOD_VAHEDS
                                                                    INNER JOIN dbo.STUF_DEF ON dbo.TCOD_VAHEDS.CODE = dbo.STUF_DEF.VAHED
                                                                    WHERE dbo.STUF_DEF.CODE = N'{CurrentRow.CODE}'
                                                                    UNION ALL
                                                                    SELECT dbo.MODULE_D.VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.MODULE_D
                                                                    INNER JOIN dbo.TCOD_VAHEDS ON dbo.MODULE_D.VAHED = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE dbo.MODULE_D.CODE = N'{CurrentRow.CODE}'
                                                                ) AS Combined").ToList();

                RST_KALAVAHED_LST = filteredUnits;

                // تنظیم آیتم‌های کمبوباکس
                COMBOBOX_VAHED_K.ItemsSource = RST_KALAVAHED_LST;

                // تنظیم مقدار انتخاب شده
                if (LastSelectedVahed.HasValue)
                {
                    COMBOBOX_VAHED_K.SelectedValue = LastSelectedVahed;
                }
                else if (filteredUnits.Any())
                {
                    COMBOBOX_VAHED_K.SelectedValue = filteredUnits.FirstOrDefault().VAHED;
                }

                // رفرش کردن آیتم‌ها
                COMBOBOX_VAHED_K.Items.Refresh();
            }
            #endregion


            #region N_KOL
            int? LastSelectedFormul = null; //پیش فرض واحد کالا انتخاب شده از قبل 
            if (CurrentRow?.N_KOL != null)
            {
                LastSelectedFormul = (int)CurrentRow.N_KOL;
            }
            if (e.Column.SortMemberPath == "N_KOL") //اگر کاربر داخل فرمول ساخت بود
            {
                var COMBOBOX_N_KOL = e.EditingElement as ComboBox;
                if (COMBOBOX_N_KOL == null) return;

                // دریافت فرمول ساخت کالا
                var filteredN_KOL = dbms.DoGetDataSQL<FSAKHT_COMBO>(@$"SELECT HEAD_MANF.FNUMB, STUF_DEF.NAME + N' - ' + CAST(HEAD_MANF.DATE_ACTIV AS nvarchar) + N' :-' + ISNULL(HEAD_MANF.TOZIH, N' ') + CAST(HEAD_MANF.FNUMB AS char) AS Expr1 FROM HEAD_MANF INNER JOIN STUF_DEF ON HEAD_MANF.CODE = STUF_DEF.CODE WHERE HEAD_MANF.CODE = '" + CurrentRow.CODE + "'").ToList();

                // تنظیم آیتم‌های کمبوباکس
                COMBOBOX_N_KOL.ItemsSource = filteredN_KOL;

                // تنظیم مقدار انتخاب شده
                if (LastSelectedFormul.HasValue)
                {
                    COMBOBOX_N_KOL.SelectedValue = LastSelectedFormul;
                }
                else if (filteredN_KOL.Any())
                {
                    COMBOBOX_N_KOL.SelectedValue = filteredN_KOL.FirstOrDefault().FNUMB;
                }

                // رفرش کردن آیتم‌ها
                COMBOBOX_N_KOL.Items.Refresh();
            }
            else
            {
                //var COMBOBOX_N_KOL = e.EditingElement as ComboBox;
                //if (COMBOBOX_N_KOL == null) return;

                //COMBOBOX_N_KOL.ItemsSource = N_KOL_ALL;
            }
            #endregion

        }
        private void INVO_LST_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
            {
                if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }
        private void INVO_LST_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (CUST_NO.SelectedValue == null)
            {
                CUST_NO.Focus();
                new Msgwin(false, "مسئول شیفت نمیتواند خالی باشد!").ShowDialog();
                return;
            }

            #region REFILL_CURRENTS
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
            {
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ITEMS_ROW = e.Row.Item as INVO_LST_FACTOR22;
            #endregion


            if (IsNull(CURRENT_ITEMS_ROW?.ANBAR))
            {
                Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد.");
                msgwin.ShowDialog();
            }
            else if (!IsNull(CURRENT_ITEMS_ROW.CODE))
            {
                var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //INVO_LST_FACTOR22_CURRENT_ROW_ITEMS.MOGODI_A = INVO_LST_FACTOR22_WAS_ROW_ITEM?.MOGODI_A;
                }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------------

            double min = 0;
            double MAND = 0;

            //انبار
            #region ANBAR
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.ANBAR = WAS_ROW_ITEM.ANBAR;
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    return;
                }
                else
                {
                    if (CURRENT_ITEMS_ROW.CODE != null)
                    {
                        var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE FROM STUF_STK WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' AND ANBAR = {ENTERED_VALUE_ROW}").ToList();
                        if (Rst1.Count == 0)
                        {
                            universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                            CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }

                        MEGH_AfterUpdate();
                    }
                }
            }
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim() ||
                    (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe())))
                {
                    #region CODE_NotInList
                    if (CURRENT_ITEMS_ROW.ANBAR is null) // انبار خالی نیست
                    {
                        return;
                    }
                    //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                    if (true)
                    {
                        //محاسبه موجودی واقعی این کالا
                        min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);

                        //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                        if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                        {
                            CURRENT_ITEMS_ROW.MEGH = 0;
                            CURRENT_ITEMS_ROW.MEGHk = 0;
                            SERCHK sERCHK = new SERCHK(I_AM_VK_SAKHTEH, CURRENT_ITEMS_ROW.ANBAR.ToString());
                            sERCHK.ShowDialog();

                            if (FROM_SEARCH_KAL.CODE is null)
                            {
                                //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                var TheCol00 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                var DGCInf00 = new DataGridCellInfo(INVO_LST_SUB.Items[row_index], INVO_LST_SUB.Columns[TheCol00]);
                                var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf00);
                                TheDGCell_MABL_K.Focus();

                                INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                return;
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.CODE = FROM_SEARCH_KAL.CODE;
                                CURRENT_ITEMS_ROW.NAME_CODE = FROM_SEARCH_KAL.NAME_CODE;

                                CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);

                                //Cleaning
                                FROM_SEARCH_KAL.CODE = null;
                                FROM_SEARCH_KAL.NAME_CODE = null;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                            {
                                //Cleaning
                                CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                                CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                return;
                            }

                            if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                            {
                                //اگر عدد وارد کرده برم سرغ کد کالا
                                var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ITEMS_ROW.ANBAR})").FirstOrDefault();
                                if (!ReferenceEquals(FoundKala, null))
                                {
                                    CURRENT_ITEMS_ROW.CODE = FoundKala.CODE;
                                    CURRENT_ITEMS_ROW.NAME_CODE = FoundKala.NAME;

                                    CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                                }
                                else
                                {
                                    //شماره فنی
                                    var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND dbo.STUF_FSK.ANBAR = {CURRENT_ITEMS_ROW.ANBAR}").ToList();
                                    if (rstfani.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.CODE = rstfani.FirstOrDefault().CODE;
                                        CURRENT_ITEMS_ROW.NAME_CODE = rstfani.FirstOrDefault().NAME;

                                        CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                                    }
                                    else
                                    {
                                        new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                                        INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);

                                        return;
                                    }
                                }
                            }
                            else
                            {
                                CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITEMS_ROW.ANBAR.ToString(), I_AM_VK_SAKHTEH);
                                if (FROM_SEARCH_KAL.CODE is null)
                                {
                                    INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                    INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;

                                    CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                    CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;

                                    return;
                                }
                                else
                                {
                                    CURRENT_ITEMS_ROW.CODE = FROM_SEARCH_KAL.CODE;
                                    CURRENT_ITEMS_ROW.NAME_CODE = FROM_SEARCH_KAL.NAME_CODE;

                                    CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);

                                    //Cleaning
                                    FROM_SEARCH_KAL.CODE = null;
                                    FROM_SEARCH_KAL.NAME_CODE = null;
                                }
                            }
                        }

                        if (CURRENT_ITEMS_ROW.ANBAR != 0)
                        {
                            if (CURRENT_ITEMS_ROW.id > 0)
                            {
                                var RSTCO1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                                if (RSTCO1.Count == 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                    msgwin.ShowDialog();
                                }
                                else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                                {
                                    var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                                    if (RSTCO2.Count > 0)
                                    {
                                        MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                                        if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ITEMS_ROW.MEGHk), 2) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                        {
                                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                            msgwin.ShowDialog();

                                            CURRENT_ITEMS_ROW = WAS_ROW_ITEM;

                                        }
                                        else
                                        {
                                            var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                                            var _WHERE = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                                            if (RSTCO3.Count > 0)
                                            {
                                                RSTCO3.FirstOrDefault().MOGODI = MAND - CURRENT_ITEMS_ROW.MEGHk;
                                                RSTCO3.FirstOrDefault().MOGODI_A = 0;
                                            }
                                        }
                                    }
                                }
                                else if (CURRENT_ITEMS_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                                {
                                    if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITEMS_ROW.MEGH_MAR)) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();
                                        CURRENT_ITEMS_ROW = WAS_ROW_ITEM;

                                    }
                                }
                                else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                    msgwin.ShowDialog();
                                    CURRENT_ITEMS_ROW = WAS_ROW_ITEM;
                                }
                            }
                        }
                        VAHED_K_AfterUpdate();


                        //AVRAGE
                        if (CURRENT_ITEMS_ROW.N_KOL == 0)
                        {
                            CURRENT_ITEMS_ROW.N_KOL = CL_HESABDARI.GETLASTFR(CURRENT_ITEMS_ROW.CODE, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                        }
                        if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.MEGHk != null && CURRENT_ITEMS_ROW?.N_KOL != null)
                        {
                            var rst = dbms.DoGetDataSQL<VKSQRE1>("SELECT dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + CURRENT_ITEMS_ROW.N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").FirstOrDefault();
                            if (rst != null)
                            {
                                CURRENT_ITEMS_ROW.AVRAGE = CURRENT_ITEMS_ROW.MABL;
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                            }
                        }

                    }
                    #endregion
                }
            }
            #endregion

            //واحد کالا
            #region VAHED_K
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    return;
                }
                if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null)
                {
                    return;
                }
                if (((e.Row.Item as INVO_LST_FACTOR22)?.VAHED_K is null) || (((e.Row.Item as INVO_LST_FACTOR22).CODE is null))
                        || ((e.Row.Item as INVO_LST_FACTOR22).NAME_CODE is null))
                {
                    INVO_LST_SUB_CANCEL_EDIT();
                    (e.Row.Item as INVO_LST_FACTOR22).VAHED_K = WAS_ROW_ITEM.VAHED_K;
                    return;
                }
                #region VAHED_K_AfterUpdate
                VAHED_K_AfterUpdate();
                #endregion

                #region VAHED_K_NotInList
                var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
                if (RSTV1.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                    CURRENT_ITEMS_ROW.VAHED_K = null;
                }
                else
                {
                    CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RSTV1.FirstOrDefault().NESBAT/*Fields(2)*/;
                }
                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = true;
                #endregion
            }
            #endregion

            //مقدار
            #region MEGH
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (CURRENT_ITEMS_ROW.ANBAR is null || CURRENT_ITEMS_ROW.CODE is null || CURRENT_ITEMS_ROW.VAHED_K is null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGH = 0;
                    return;
                }
                if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null)
                {
                    return;
                }
                CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(ENTERED_VALUE_ROW);

                MEGH_AfterUpdate();
            }
            #endregion

            //مقدار کل
            #region MEGHk
            if (e.Column.SortMemberPath == "MEGHk")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGHk = 0;
                    return;
                }
                if (CURRENT_ITEMS_ROW?.ANBAR is null || CURRENT_ITEMS_ROW?.CODE is null || CURRENT_ITEMS_ROW?.VAHED_K is null || CURRENT_ITEMS_ROW?.MEGH is null)
                {
                    return;
                }

                #region MEGHk_AfterUpdate
                long Temp;
                var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                }
                else
                {
                    CURRENT_ITEMS_ROW.MEGH = CURRENT_ITEMS_ROW.MEGHk / RST.FirstOrDefault().NESBAT;
                }
                #endregion
            }
            #endregion

            //فرمول ساخت
            #region N_TAF
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.N_KOL = WAS_ROW_ITEM.N_KOL;
                    universControl.PopNotifyShow("فرمول ساخت نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    return;
                }
                else
                {

                }
                //FSAKHT_COMBO
            }
            #endregion

        }

        private void INVO_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var TheRow = e.Row.Item as INVO_LST_FACTOR22;
            if (ConstructorRowDetector.IsPristine(TheRow)) { INVO_LST_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(TheRow))
            {
                INVO_LST_SUB_CANCEL_EDIT();
                return;
            }


            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH,FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                              OUTPUT INSERTED.id
                              VALUES({NUMBER.Text},
                              {FTAG} ,
                              {TheRow.ANBAR}   ,
                              NULL,
                              N'{TheRow.CODE}' ,
                              {TheRow.MEGH} ,
                              {TheRow.MEGHk} ,
                              {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)} ,
                              N'{TheRow.MANDAH}' ,
                              0,
                              N'{(TheRow.N_RASID is null ? "NULL" : TheRow.N_RASID)}' ,
                              {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)} ,
                              {(TheRow.RADAH is null ? "NULL" : TheRow.RADAH)} ,
                              {(TheRow.SANAD_NO is null ? "NULL" : TheRow.SANAD_NO)} ,
                              NULL,
                              {(TheRow.ANBARF is null ? "NULL" : TheRow.ANBARF)} ,
                              {TheRow.VAHED_K}   ,
                              {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)} ,
                              {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)} ,
                              {(TheRow.N_TAF is null ? "NULL" : TheRow.N_TAF)} ,
                              {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)} ,
                              {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)} ,
                              {TheRow.IMBAA} ,
                              {(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)} ,
                              N'{(TheRow.VISITOR is null ? "NULL" : TheRow.VISITOR)}' ,
                              {TheRow.TKHN} ,
                              {(TheRow.JAY?.ToString() is null ? "NULL" : TheRow.JAY.ToString())}   ,
                              {(TheRow.JAYO?.ToString() is null ? "NULL" : TheRow.JAYO.ToString())} )";

                var (errorMsgs, _, _, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { TheRow }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);

                if (queryOutputs.Any())
                {
                    TheRow.id = queryOutputs.FirstOrDefault(); // Update the list with the new ID
                                                               //اصلاح شماره ردیف
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG={FTAG}) FROM dbo.INVO_LST WHERE id = {TheRow.id}");
                }
            }
            else //UPDATE
            {
                _qre = $@"UPDATE dbo.INVO_LST
                   SET ANBAR = {TheRow.ANBAR}, CODE = N'{TheRow.CODE}',
                   MEGH = {TheRow.MEGH}, MEGHk = {TheRow.MEGHk}, MEGH_MAR = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},
                   MANDAH = N'{TheRow.MANDAH}',
                   N_RASID = N'{(TheRow.N_RASID is null ? "NULL" : TheRow.N_RASID)}',
                   MEGH_R = {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)}, 
                   RADAH = {(TheRow.RADAH is null ? "NULL" : TheRow.RADAH)}, 
                   SANAD_NO = {(TheRow.SANAD_NO is null ? "NULL" : TheRow.SANAD_NO)},
                   ANBARF = {(TheRow.ANBARF is null ? "NULL" : TheRow.ANBARF)}, 
                   VAHED_K = {TheRow.VAHED_K}, N_KOL = {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)}, 
                   N_MOIN = {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)}, N_TAF = {(TheRow.N_TAF is null ? "NULL" : TheRow.N_TAF)},
                   AVRAGE = {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)},
                   AVRAGE2 = {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)}, IMBAA = {TheRow.IMBAA}, 
                   TOTALARZ = {(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)}, VISITOR = N'{(TheRow.VISITOR is null ? "NULL" : TheRow.VISITOR)}',
                   TKHN = {TheRow.TKHN}, JAY = {(TheRow.JAY?.ToString() is null ? "NULL" : TheRow.JAY.ToString())}, JAYO = {(TheRow.JAYO?.ToString() is null ? "NULL" : TheRow.JAYO.ToString())}
                   WHERE id = {TheRow.id}";

                var (errorMsgs, _, _, _) = IVM.CheckInventoryAndExecuteQuery<int>(new List<object> { TheRow }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);
            }

            //انبار خالی نباشد
            if (TheRow?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {TheRow.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (!IsNull(TheRow.CODE))
            {
                var RST_STUF_STK = IVM.TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).ToList();
                if (RST_STUF_STK.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {TheRow.CODE} به انبار {TheRow.ANBAR} فوق تعلق ندارد." });
                }
            }

            //بررسی صحیح بودن واحد کالا نسبت به خود کالا
            var RSTV1 = IVM.TM.SqlQueryCtc<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + TheRow.CODE + "' AND ((VAHEDS.VAHED)= " + TheRow.VAHED_K + ")))").ToList();
            if (RSTV1.Count == 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });
                TheRow.VAHED_K = null;
            }
            //واحد کالا بررسی مقدار کل باتوجه به نسبت
            else
            {
                var NesbatMegh = RSTV1.FirstOrDefault()?.NESBAT * TheRow.MEGH;
                if (NesbatMegh != TheRow.MEGHk)
                {

                    TheRow.MEGHk = NesbatMegh;
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }

            if (ErrosMessages.Any())
            {
                IVM.RollbackTransaction(); //Undo
            }
            else
            {
                IVM.CommitTransaction(); // Commit Apply Save
            }
            MasterTopErrorMessages.AddRange(ErrosMessages);


            SANAD();

            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

        }
        void VAHED_K_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.VAHED_K is null) { return; }
            if (CURRENT_ITEMS_ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST.FirstOrDefault().NESBAT;
            }

            MEGH_AfterUpdate();
        }
        public void AVRAGE_UPDATE()
        {
            return; //Obsolete
            //CODE_AfterUpdate
            if (CURRENT_ITEMS_ROW?.MEGH > 0 && CURRENT_ITEMS_ROW?.MABL > 0 && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.id != null)
            {
                //var RST = dbms.DoGetDataSQL<STUF_DEF>($@"SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra
                //  FROM dbo.STUF_DEF WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' ").FirstOrDefault();

                //-- ANBAR , DATE , PARA id , COD (CODE)
                var rst3 = dbms.DoGetDataSQL<AVRAGE_MOG>($@"SELECT CODE, MOG, MABL, VMEGHK, VMABK, FMABK, FMEGHK 
                FROM dbo.AVRAGE_MOG('{CURRENT_ITEMS_ROW.ANBAR}', '{DATE_N.Text.ToRawTarikh()}', '{CURRENT_ITEMS_ROW.id}', '{CURRENT_ITEMS_ROW.CODE}')").FirstOrDefault();

                //میانگین
                if (rst3 != null && (rst3.MOG + CURRENT_ITEMS_ROW.MEGHk) != 0)
                {
                    long temp = (long)Math.Round((double)((rst3.MABL + CURRENT_ITEMS_ROW.MABL_K) / (rst3.MOG + CURRENT_ITEMS_ROW.MEGHk) * 100));
                    CURRENT_ITEMS_ROW.AVRAGE = temp / 100d;
                }
                else
                {
                    CURRENT_ITEMS_ROW.AVRAGE = 0;
                }
            }
        }
        void MEGH_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW.MEGHk is null ||
                CURRENT_ITEMS_ROW?.ANBAR is null ||
                CURRENT_ITEMS_ROW?.CODE is null)
            {
                return;
            }

            double min;
            double MAND = 0;
            var RST0 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
            if (RST0.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
                return;
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST0.FirstOrDefault().NESBAT;

            }

            #region MyRegion
            GetCurrentMogudi();
            #endregion

            if ((Convert.ToBoolean(Baseknow.RMOG) || Baseknow.MOJU) && CURRENT_ITEMS_ROW.ANBAR != 0)
            {
                if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.MEGHk != null)
                {
                    var _where = "WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                    var RSTM3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM dbo.STUF_STK {_where}").ToList();
                    if (RSTM3.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else
                    {
                        min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);

                        var RSTM0 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                        if (RSTM0.Count > 0)
                        {
                            MAND = Convert.ToDouble(RSTM0.FirstOrDefault());
                            var RequestMeghkDiff = Convert.ToDouble(Convert.ToDouble(WAS_ROW_ITEM?.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) - CURRENT_ITEMS_ROW.MEGHk);

                            double LeftMand = Math.Round(MAND - RequestMeghkDiff, Convert.ToInt32(Baseknow.DIG));
                            var AtLeastMand = Math.Round(min, Convert.ToInt32(Baseknow.DIG));

                            if (LeftMand < AtLeastMand)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                                msgwin.ShowDialog();
                                CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                            }
                            ////Update:
                            ////RSTM2.FirstOrDefault().MOGODI = MAND - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR));
                            ////RSTM2.FirstOrDefault().MOGODI_A = 0;
                        }
                    }
                }
            }

            if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.MEGHk != null && CURRENT_ITEMS_ROW?.N_KOL != null)
            {
                var rst = dbms.DoGetDataSQL<VKSQRE1>("SELECT     dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + CURRENT_ITEMS_ROW.N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").FirstOrDefault();
                if (rst != null)
                {
                    CURRENT_ITEMS_ROW.AVRAGE = CURRENT_ITEMS_ROW.MABL;
                }
                else
                {
                    CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                }
            }

        }


        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (CUST_NO.SelectedValue is not null)
            {
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("HAVALAH_ENTER", I_AM_VK_SAKHTEH);//Search Plusy Form Specialy for Customers
                CMBSearch.ShowDialog();
                if (CUST_NO.SelectedValue is null)
                {
                    return;
                }
            }
            else if (Information.IsNumeric(CUTSNO_TEX.Text))
            {
                try
                {
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " and NUMBER = 1 and tNUMBER = " + CUTSNO_TEX.Text).ToList();
                    if (rst.Count == 1)
                    {
                        var _data_hes = rst.FirstOrDefault()?.n_kol + "-" + rst.FirstOrDefault()?.NUMBER + "-" + rst.FirstOrDefault()?.tNUMBER;
                        var _data_name = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE hes = N'{_data_hes}'").FirstOrDefault();
                        if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == _data_hes))
                        {
                            ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = _data_hes, NAME = _data_name });
                        }
                        CUST_NO.Items.Refresh();
                        CUST_NO.SelectedValue = null;
                        this.CUST_NO2.SelectedValue = _data_hes;
                        //CUST_NO_AfterUpdate();
                    }
                    else
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.Text = null;
                        CUST_NO.Items.Refresh();
                        return;
                    }
                }
                catch (Exception) { }
            }
            else
            {
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + CUTSNO_TEX.Text + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;
                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = null;
                    CUST_NO.SelectedValue = thevalue;
                    CUST_NO.Items.Refresh();
                }
                else
                {
                    CUST_NO.SelectedValue = null;
                    CUST_NO.Text = null;
                    CUST_NO.Items.Refresh();
                    return;
                }
            }

            if (CUST_NO.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    CUST_NO.SelectedValue = null;
                }
                if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.CUST_NO.SelectedValue.ToString())) == false)
                    {
                        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                        msgwin.ShowDialog();
                        CUST_NO.SelectedValue = null;
                    }
                }
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO2.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

        }
        private void SANAD()
        {
            var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADVORUDSAKHT(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

            if (SanadNumber != null)
            {
                N_S.Text = SanadNumber.ToString();
            }

            if ((bool)Baseknow.ECONM)
            {
                double num = 0;
                long MABLTMP = 0;

                if (Strings.Mid(Baseknow.OPTIONSS, 56, 1) != "5")
                {
                    if (!IsNull(NUMBER1.Text) && NUMBER1.Text != "0")
                    {
                        var rst = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + NUMBER1.Text + " AND TAG = 10").ToList();
                        if (rst.Count == 1)
                        {
                            dbms.DoExecuteSQL("DELETE FROM INVO_LST WHERE NUMBER = " + this.NUMBER1.Text + " AND TAG = 10");
                            var RSTK = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE NUMBER =" + this.NUMBER.Text + " AND TAG = 9").ToList();
                            for (int i = 0; i < RSTK.Count; i++) //while (!RSTK.EOF())
                            {
                                var rstf = dbms.DoGetDataSQL<VKSQRE2>("SELECT dbo.HEAD_MANF.CODE, dbo.DTL_MANF.FNUMB, dbo.DTL_MANF.CODE AS CODB, dbo.DTL_MANF.ANBAR, dbo.DTL_MANF.MEGHk,  dbo.DTL_MANF.VAHED_K , dbo.DTL_MANF.MEGH, dbo.DTL_MANF.PERT, dbo.DTL_MANF.smabl, dbo.DTL_MANF.MABLK FROM  dbo.DTL_MANF INNER JOIN  dbo.HEAD_MANF ON dbo.DTL_MANF.FNUMB = dbo.HEAD_MANF.FNUMB WHERE  (dbo.HEAD_MANF.CODE = '" + RSTK[i].CODE + "')").ToList();
                                for (int S = 0; S < rstf.Count; S++) //while (!rstf.EOF())
                                {
                                    //RSTM.AddNew();
                                    var _NUMBER_ = this.NUMBER1.Text;
                                    var _TAG_ = 10;
                                    var _ANBAR_ = rstf[S].ANBAR;
                                    var _CODE_ = rstf[S].CODB; //N''
                                    var _VAHED_K_ = rstf[S].VAHED_K;
                                    var _MEGH_ = (rstf[S].MEGH + rstf[S].PERT) * RSTK[i].MEGHk;
                                    var _MEGHk_ = (rstf[S].MEGHk + rstf[S].PERT) * RSTK[i].MEGHk;
                                    var _N_RASID_ = rstf[S].FNUMB; //N''
                                    MABLTMP = (long)CL_HESABDARI.LASTAVRAGE(rstf[S].CODB, Convert.ToInt64(rstf[S].ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                                    var _MABL_ = MABLTMP;
                                    var _AVRAGE_ = MABLTMP;
                                    var _MABL_K_ = MABLTMP * (rstf[S].MEGHk + rstf[S].PERT) * RSTK[i].MEGHk;
                                    //RSTM.update();
                                    dbms.DoExecuteSQL(@$"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,CODE,VAHED_K,MEGH,MEGHk,N_RASID,MABL,AVRAGE,MABL_K) 
                                      VALUES ({_NUMBER_},{_TAG_},{_ANBAR_},N'{_CODE_}',{_VAHED_K_},{_MEGH_},{_MEGHk_},N'{_N_RASID_}',{_MABL_},{_AVRAGE_},{_MABL_K_})");
                                }
                            }
                            AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(Convert.ToInt64(NUMBER1.Text), Convert.ToInt64(NUMBER1.Text), false);
                        }
                    }
                    else
                    {
                        var rstq = dbms.DoGetDataSQL<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=10))").FirstOrDefault();
                        if (rstq == null || rstq == 0)
                        {
                            num = 1L;
                        }
                        else
                        {
                            num = (double)(rstq + 1);
                        }

                        {
                            //rst.AddNew();
                            var _NUMBER_ = num;
                            var _TAG_ = 10;
                            var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                            var _DATE_N_ = DATE_N.Text.ToRawTarikh();
                            var _FNUMCO_ = NUMBER.Text;
                            var _CUST_NO_ = CUST_NO.SelectedValue;
                            var _MOLAH_ = "بر اساس توليد";
                            var _OKF_ = true;
                            //rst.update();

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST(NUMBER,TAG,USER_NAME,DATE_N,FNUMCO,CUST_NO,MOLAH,OKF)
                                             VALUES({_NUMBER_},{_TAG_},N'{_USER_NAME_}',{_DATE_N_},{_FNUMCO_},N'{_CUST_NO_}',N'{_MOLAH_}',1)");
                        }

                        //RSTM.Open("INVO_LST", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);

                        var RSTK = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE NUMBER =" + NUMBER.Text + " AND TAG = 9").ToList();
                        for (int i = 0; i < RSTK.Count; i++) //while (!RSTK.EOF())
                        {
                            var rstf = dbms.DoGetDataSQL<VKSQRE2>("SELECT dbo.HEAD_MANF.CODE, dbo.DTL_MANF.FNUMB, dbo.DTL_MANF.CODE AS CODB, dbo.DTL_MANF.ANBAR, dbo.DTL_MANF.MEGHk,  dbo.DTL_MANF.VAHED_K , dbo.DTL_MANF.MEGH, dbo.DTL_MANF.PERT, dbo.DTL_MANF.smabl, dbo.DTL_MANF.MABLK FROM  dbo.DTL_MANF INNER JOIN  dbo.HEAD_MANF ON dbo.DTL_MANF.FNUMB = dbo.HEAD_MANF.FNUMB WHERE  (dbo.HEAD_MANF.CODE = '" + RSTK[i].CODE + "')").ToList();
                            for (int S = 0; S < rstf.Count; S++) //while (!rstf.EOF())
                            {
                                //RSTM.AddNew();
                                var _NUMBER_ = num;
                                var _TAG_ = 10;
                                var _ANBAR_ = rstf[i].ANBAR;
                                var _CODE_ = rstf[i].CODB;
                                var _VAHED_K_ = rstf[i].VAHED_K;
                                var _MEGH_ = (rstf[i].MEGH + rstf[i].PERT) * RSTK[i].MEGHk;
                                var _MEGHk_ = (rstf[i].MEGHk + rstf[i].PERT) * RSTK[i].MEGHk;
                                var _N_RASID_ = rstf[i].FNUMB;
                                MABLTMP = (long)CL_HESABDARI.LASTAVRAGE(rstf[i].CODB, Convert.ToInt64(rstf[i].ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                                var _MABL_ = MABLTMP;
                                var _AVRAGE_ = MABLTMP;
                                var _MABL_K_ = MABLTMP * (rstf[i].MEGHk + rstf[i].PERT) * RSTK[i].MEGHk;
                                //RSTM.update();

                                dbms.DoExecuteSQL(@$"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,CODE,VAHED_K,MEGH,MEGHk,N_RASID,MABL,AVRAGE,MABL_K) 
                                      VALUES ({_NUMBER_},{_TAG_},{_ANBAR_},N'{_CODE_}',{_VAHED_K_},{_MEGH_},{_MEGHk_},N'{_N_RASID_}',{_MABL_},{_AVRAGE_},{_MABL_K_})");
                            }
                        }

                        this.NUMBER1.Text = num.ToString();

                        AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(Convert.ToInt64(NUMBER1.Text), Convert.ToInt64(NUMBER1.Text), false);
                    }
                }
                else if (!IsNull(NUMBER1.Text) && NUMBER1.Text != "0")
                {
                    var rst = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + NUMBER1.Text + " AND TAG = 10").ToList();
                    if (rst.Count == 1)
                    {
                        dbms.DoExecuteSQL("DELETE FROM INVO_LST WHERE NUMBER = " + NUMBER1.Text + " AND TAG = 10");
                        //RSTM.Open("INVO_LST", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);

                        var RSTK = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE NUMBER =" + NUMBER.Text + " AND TAG = 9").ToList();
                        for (int i = 0; i < RSTK.Count; i++) //while (!RSTK.EOF())
                        {
                            var rstf = dbms.DoGetDataSQL<VKSQRE2>("SELECT dbo.HEAD_MANF.CODE, dbo.DTL_MANF.FNUMB, dbo.DTL_MANF.CODE AS CODB, dbo.DTL_MANF.ANBAR, dbo.DTL_MANF.MEGHk,  dbo.DTL_MANF.VAHED_K , dbo.DTL_MANF.MEGH, dbo.DTL_MANF.PERT, dbo.DTL_MANF.smabl, dbo.DTL_MANF.MABLK FROM  dbo.DTL_MANF INNER JOIN  dbo.HEAD_MANF ON dbo.DTL_MANF.FNUMB = dbo.HEAD_MANF.FNUMB WHERE  (dbo.HEAD_MANF.FNUMB = '" + (IsNull(RSTK[i].N_KOL) ? 0 : RSTK[i].N_KOL) + "')").ToList();
                            for (int O = 0; O < rstf.Count; O++) //while (!rstf.EOF())
                            {
                                //RSTM.AddNew();
                                var _NUMBER_ = NUMBER1.Text;
                                var _TAG_ = 10;
                                var _ANBAR_ = rstf[O].ANBAR;
                                var _CODE_ = rstf[O].CODB;
                                var _VAHED_K_ = rstf[O].VAHED_K;
                                var _MEGH_ = (rstf[O].MEGH + rstf[O].PERT * RSTK[i].MEGHk);
                                var _MEGHk_ = (rstf[O].MEGHk + rstf[O].PERT) * RSTK[i].MEGHk;
                                var _N_RASID_ = rstf[O].FNUMB;
                                MABLTMP = (long)CL_HESABDARI.LASTAVRAGE(rstf[O].CODB, Convert.ToInt64(rstf[O].ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                                var _MABL_ = MABLTMP;
                                var _AVRAGE_ = MABLTMP;
                                var _MABL_K_ = MABLTMP * (rstf[O].MEGHk + rstf[O].PERT) * RSTK[i].MEGHk;
                                //RSTM.update();

                                dbms.DoExecuteSQL(@$"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,CODE,VAHED_K,MEGH,MEGHk,N_RASID,MABL,AVRAGE,MABL_K) 
                                      VALUES ({_NUMBER_},{_TAG_},{_ANBAR_},N'{_CODE_}',{_VAHED_K_},{_MEGH_},{_MEGHk_},N'{_N_RASID_}',{_MABL_},{_AVRAGE_},{_MABL_K_})");
                            }
                        }
                        AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(Convert.ToInt64(NUMBER1.Text), Convert.ToInt64(NUMBER1.Text), false);
                    }
                }
                else
                {
                    var rst0 = dbms.DoGetDataSQL<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=10))").FirstOrDefault();
                    if (rst0 == null || rst0 == 0)
                    {
                        num = 1L;
                    }
                    else
                    {
                        num = (double)(rst0 + 1);
                    }

                    //rst.Open("HEAD_LST", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                    //rst.AddNew();
                    {
                        var _NUMBER_ = num;
                        var _TAG_ = 10;
                        var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                        var _DATE_N_ = DATE_N.Text.ToRawTarikh();
                        var _FNUMCO_ = NUMBER.Text;
                        var _CUST_NO_ = CUST_NO.SelectedValue;
                        var _MOLAH_ = "بر اساس توليد";
                        var _OKF_ = true;
                        //rst.update();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST(NUMBER,TAG,USER_NAME,DATE_N,FNUMCO,CUST_NO,MOLAH,OKF)
                                             VALUES({_NUMBER_},{_TAG_},N'{_USER_NAME_}',{_DATE_N_},{_FNUMCO_},N'{_CUST_NO_}',N'{_MOLAH_}',1)");
                    }

                    var RSTK = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST WHERE NUMBER =" + NUMBER.Text + " AND TAG = 9").ToList();
                    for (int E = 0; E < RSTK.Count; E++) //while (!RSTK.EOF())
                    {
                        var rstf = dbms.DoGetDataSQL<VKSQRE2>("SELECT dbo.HEAD_MANF.CODE, dbo.DTL_MANF.FNUMB, dbo.DTL_MANF.CODE AS CODB, dbo.DTL_MANF.ANBAR, dbo.DTL_MANF.MEGHk,  dbo.DTL_MANF.VAHED_K , dbo.DTL_MANF.MEGH, dbo.DTL_MANF.PERT, dbo.DTL_MANF.smabl, dbo.DTL_MANF.MABLK FROM  dbo.DTL_MANF INNER JOIN  dbo.HEAD_MANF ON dbo.DTL_MANF.FNUMB = dbo.HEAD_MANF.FNUMB WHERE  (dbo.HEAD_MANF.FNUMB = '" + RSTK[E].N_KOL + "')").ToList();
                        for (int i = 0; i < rstf.Count; i++) //while (!rstf.EOF())
                        {
                            //RSTM.AddNew();
                            var _NUMBER_ = num;
                            var _TAG_ = 10;
                            var _ANBAR_ = rstf[i].ANBAR;
                            var _CODE_ = rstf[i].CODB;
                            var _VAHED_K_ = rstf[i].VAHED_K;
                            var _MEGH_ = (rstf[i].MEGH + rstf[i].PERT) * RSTK[E].MEGHk;
                            var _MEGHk_ = (rstf[i].MEGHk + rstf[i].PERT) * RSTK[E].MEGHk;
                            var _N_RASID_ = rstf[i].FNUMB;
                            MABLTMP = (long)CL_HESABDARI.LASTAVRAGE(rstf[i].CODB, Convert.ToInt64(rstf[i].ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                            var _MABL_ = MABLTMP;
                            var _AVRAGE_ = MABLTMP;
                            var _MABL_K_ = MABLTMP * (rstf[i].MEGHk + rstf[i].PERT) * RSTK[E].MEGHk;
                            //RSTM.update();
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.INVO_LST(NUMBER,TAG,ANBAR,CODE,VAHED_K,MEGH,MEGHk,N_RASID,MABL,AVRAGE,MABL_K) 
                                      VALUES ({_NUMBER_},{_TAG_},{_ANBAR_},N'{_CODE_}',{_VAHED_K_},{_MEGH_},{_MEGHk_},N'{_N_RASID_}',{_MABL_},{_AVRAGE_},{_MABL_K_})");
                        }
                    }

                    NUMBER1.Text = num.ToString();
                    AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(Convert.ToInt64(num), Convert.ToInt64(num), false);

                }
            }

            DoCmdHeaderSave();

            GetBalanceInfo();
        }

        private string BEFOREDATEN;
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = BEFOREDATEN;
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = BEFOREDATEN;
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            else
            {
                DATE_N.Text = BEFOREDATEN;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

        }
        private void DATE_N_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE_N.Text.ToRawTarikh();
        }
        private void BTN_NEW_FACTOR_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeIsHappend)
            {
                ClearFreshAll();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "ذخیره را انجام نداده ای آیا از ادامه مطمئن هستید؟");
                if (msgwin.DialogResult != true)
                {
                    return;
                }
            }
        }
        private void ClearFreshAll(bool IsFromSelectionNumber = false)
        {
            NUMBER.Text = "0";

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();
            MOLAH.Text = null;

            FNUMCO.Text = "0"; //شماره داخلی

            OKF.IsChecked = false;

            NUMBER1.Text = ""; //ثبت در سند
            N_S.Text = ""; //ثبت در سند
            MABNA.Text = ""; //ثبت در سند

            INVO_LST_FACTOR22_DATA?.Clear(); //دیتاگرید فاکتور فروش

            Form_Current();

            AllowEdits = true;

            INVO_LST_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private void Form_Current()
        {
            bool ghat = false;

            if (INVO_LST_SUB.Items.Count > 0)
            {
                Command100.IsEnabled = true;
                Command106.IsEnabled = true;
            }
            else
            {
                Command100.IsEnabled = false;
                Command106.IsEnabled = false;
            }

            if (string.IsNullOrEmpty(N_S.Text))
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
                INVO_LST_SUB.IsReadOnly = false;
                MABNA.Text = null;
            }
            else
            {
                var rst = dbms.DoGetDataSQL<DeedHedData>($"SELECT BASE, GHATEI FROM DEED_HED WHERE N_S = {N_S.Text}").FirstOrDefault();
                if (rst != null)
                {
                    MABNA.Text = rst.BASE;
                    if (rst.GHATEI)
                    {
                        ghat = true;
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        INVO_LST_SUB.IsReadOnly = true;
                        ESLAH.IsEnabled = false;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        INVO_LST_SUB.IsReadOnly = false;
                    }
                }
            }

            if (NewRecord)
            {
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                if (!ghat)
                {
                    INVO_LST_SUB.IsReadOnly = false;
                }
                else
                {
                    INVO_LST_SUB.IsReadOnly = true;
                    ESLAH.Visibility = Visibility.Hidden;
                }
            }

            if (OKF.IsChecked != null && OKF.IsChecked == true && !NewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_SUB.IsReadOnly = true;
                ESLAH.IsEnabled = true;
            }

        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_SUB.Items.Count > 0)
            {
                if (INVO_LST_SUB.SelectedItem is not null)
                {
                    var Row = INVO_LST_SUB.SelectedItem as INVO_LST_FACTOR22;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void INVO_LST_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                INVO_LST_SUB_IsFocused = false;
            }
            else
            {
                INVO_LST_SUB_IsFocused = true;
            }
        }

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, FTAG);

            if (NewRecord)
            {
                this.Close();
            }
        }

        //چاپ فاکتور
        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            if (!Command100.IsEnabled || Command100.Visibility != Visibility.Visible)
            {
                return;
            }
            if (NewRecord || INVO_LST_FACTOR22_DATA.Count == 0)
            {
                return;
            }

            // Flag to prevent printing if any record shows invalid stock levels.
            bool notPrint = false;

            string invoSql = "SELECT * FROM invo_lst WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {FTAG}";
            var invoLstRecords = dbms.DoGetDataSQL<INVO_LST>(invoSql).ToList();

            foreach (var record in invoLstRecords)
            {
                // Build the SQL to get the 'mand' value.
                string innerSql =
                    "SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0), 2) AS mand " +
                    "FROM dbo.AK_MOGO_AVL_KOL(99999999, " + record.ANBAR + ") AK_MOGO_AVL_KOL " +
                    "RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR " +
                    "LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, " + record.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR " +
                    "WHERE dbo.STUF_FSK.CODE = N'" + record.CODE + "' AND dbo.STUF_FSK.ANBAR = " + record.ANBAR;

                // Execute the query and get the first result.
                var mandResult = dbms.DoGetDataSQL<double?>(innerSql).FirstOrDefault();

                if (mandResult != null)
                {
                    // If the available quantity ("mand") is negative, show a message.
                    if (mandResult < 0)
                    {
                        // GETKALANAME should return the name for the given code.
                        string kalaName = CL_HESABDARI.GETKALANAME(Convert.ToDouble(record.CODE));
                        string msg = " كالاي  " + kalaName + "داراي  موجودي غير مجاز  مي باشد.برگه قابل چاپ نيست";

                        new Msgwin(false, msg).ShowDialog();

                        // Mark that printing should not proceed.
                        notPrint = true;
                    }
                }
            }

            if (!notPrint)
            {
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.SANATI.HAVLAH_ENTER.mrt");
                report.Load(pathreport);
                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
                ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 300;

                report["NUMBER_PARAM"] = NUMBER.Text;
                (report.GetComponentByName("CUST_NO_NAME") as StiText).Text = (CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME;
                (report.GetComponentByName("COMPANY_NAME") as StiText).Text = Baseknow.WIDTH_D; //نام شرکت

                new WINRPT(report, LABEL_HEADER.Content.ToString()).Show();

                if ((bool)Baseknow.LOCKFAP)
                {
                    OKF.IsChecked = true;
                }

                if (OKF.IsChecked == true)
                {
                    this.AllowDeletions = false;
                    this.AllowEdits = false;

                    this.INVO_LST_SUB.IsReadOnly = true;

                    this.ESLAH.IsEnabled = true;
                }

                DoCmdHeaderSave();
            }
        }

        private void Command106_Click(object sender, RoutedEventArgs e)
        {

        }

        private void N_S_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Right N_S
            if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
            {
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), false);
            }
        }
        private void NUMBER1_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(NUMBER1.Text) && NUMBER1.Text != "0")
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_EXIT, this, Convert.ToDouble(NUMBER1.Text));
            }
        }

        private void INVO_LST_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
            }

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
                e.Handled = true;
            }
        }


        private void GetCurrentMogudi()
        {
            _ = Task.Run(() =>
            {
                INVO_LST_FACTOR22? currentRow = null;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    MOGUDI.IsEnabled = false;
                    currentRow = INVO_LST_SUB.SelectedItem as INVO_LST_FACTOR22;
                }));

                if (currentRow == null ||
                    currentRow.ANBAR == null ||
                    currentRow.CODE == null ||
                    currentRow.MEGHk == null)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        MOGUDI.IsEnabled = true;
                        MOGUDI.Text = null;
                    }));
                    return;
                }

                var (_ErrMsg_, _Msg_, _KalaInfo_) = IVM.GetKalaMogudi(dbms,
                 new List<INVO_LST_FACTOR22>
                 {
                       new INVO_LST_FACTOR22
                       {
                           ANBAR = currentRow.ANBAR,
                           CODE = currentRow.CODE,
                           MEGHk = currentRow.MEGHk
                       }
                 });

                this.Dispatcher.Invoke(new Action(() =>
                {
                    MOGUDI.Text = _KalaInfo_?.FirstOrDefault()?.CURRENT_MOGUDI.ToStringNullSafe();
                }));
                try
                {
                }
                catch { }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        MOGUDI.IsEnabled = true;
                    }));
                }
            });
        }
    }
}
