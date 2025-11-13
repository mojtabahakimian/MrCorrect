using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
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
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using System.ComponentModel;
using Syncfusion.Data.Extensions;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using Rpts;
using Wins.WinOther;
using static Interfaces.INavigator;
using Wins.WinMenus.ANBAR;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class HEAD_LST_KH_BACK : Window, ISearchableWindow
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

        #region LOCALMODEL
        public class QVIS3
        {
            public string? CODE { get; set; }
            public double? avrage2 { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? mabk { get; set; }
            public int? ANBAR { get; set; }
            public double? RADAH { get; set; }
            public string? nam { get; set; }
        }
        public class QVIS2
        {
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
        }
        public class QRE_LST_BARGASHT
        {
            public double? NUMBER { get; set; }
        }
        public class SGN_IMODEL
        {
            public string SEMAT_USER { get; set; }
            public string NAME_HESAB_USER { get; set; }
        }
        public class DeedHedData
        {
            public string BASE { get; set; }
            public bool GHATEI { get; set; }
        }
        public class SignData
        {
            public bool FFRB_FROOSHTX { get; set; }
            public bool FFRB_ANBTX { get; set; }
            public bool FFRB_HESABTX { get; set; }
        }
        public class CheckData
        {
            public double? N_SERI { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public long? DATE { get; set; }
            public long? DATE_S { get; set; }
            public double? MABL { get; set; }
            public int? NUMBER { get; set; }
            public int? TAG { get; set; }
        }

        public class HeadLstData
        {
            public int NUMBER { get; set; }
            public int htag { get; set; }
            public double MABL_HAZ { get; set; }
            public double MABL_VAR { get; set; }
            public double MABL_HAV { get; set; }
            public double M_NAGHD { get; set; }
            public double TAKHFIF { get; set; }
            public double MBAA { get; set; }
        }
        #endregion

        /// <summary>
        /// شماره فاكتور برگشت NUMBER
        /// </summary>
        /// <param name="number_to_open"></param>
        public HEAD_LST_KH_BACK(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                OpenArgs = number_to_open.ToString();
                NUMBER.Text = number_to_open.ToString(); //شماره رسید
                NUMBER.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله
        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<PAY_GETD_SUB22_MODEL> PAY_GETD_SUB22_DATA { get; set; } = new ObservableCollection<PAY_GETD_SUB22_MODEL>();

        private NavigationManager<HEAD_LST> _navigationManager;

        /// <summary>
        /// 3
        /// </summary>
        public byte FTAG { get; } = 3; //هدر برگشت خرید

        /// <summary>
        /// 1
        /// </summary>
        public byte FAC1TAG { get; } = 1; //رسید انبار

        public int? ANBAR { get; set; }


        private double _sum_of_megh_mar = 0;
        public double SUM_OF_MEGH_MAR
        {
            get
            {
                _sum_of_megh_mar = (double)INVO_LST_FACTOR22_DATA.Sum(selector: r => r.MEGH_MAR);
                if (_sum_of_megh_mar == 0) _sum_of_megh_mar = 0;
                return _sum_of_megh_mar;
            }
            set { _sum_of_megh_mar = value; }
        }

        private double _sum_of_mabmar = 0;
        public double SUM_OF_MABMAR_MABLK
        {
            get
            {
                //=Sum([MABMAR])
                _sum_of_mabmar = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MABMAR);
                if (_sum_of_mabmar == 0) _sum_of_mabmar = 0;
                return _sum_of_mabmar;
            }
            set { _sum_of_mabmar = value; }
        }

        private double sum_of_megh_k = 0;
        public double SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MEGHk);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }


        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "KFRB_BAZARTX");
                    _sgn1_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN1usid.Tag)));
                }
                return _sgn1_info;
            }
        }
        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO
        {
            get
            {
                if (SGN2usid.Tag is not null)
                {
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "KFRB_ANBTX");
                    _sgn2_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN2usid.Tag)));
                }
                return _sgn2_info;
            }
        }
        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO
        {
            get
            {
                if (SGN3usid.Tag is not null)
                {
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "KFRB_HESABTX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        public bool NowIsReady { get; private set; }

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

        List<COMBOPERSONEL> rst_personel = null;
        public bool INVO_LST_SUB_IsFocused { get; private set; }

        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "MEGH_MAR")?.DisplayIndex;
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
        public FULL_HESAB HESAB_POSHTEF_FROM_SEARCH { get; set; } = new FULL_HESAB();

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

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
                CUST_KIND.IsReadOnly = !ican;// نوع مشتری
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ
                SHIFT.IsReadOnly = !ican;// شیفت
                FNUMCO.IsReadOnly = !ican;

                //__ENABLEY
                DEPATMAN.IsEnabled = ican;

                DATE_N.IsEnabled = ican;// تاریخ
                NUMBER1.IsEnabled = ican;// شماره حواله
                CUST_KIND.IsEnabled = ican;// نوع مشتری
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ
                SHIFT.IsEnabled = ican;// شیفت
                //فاکتور END
                //Page58.IsEnabled = ican;// تب پشت فاکتور

                //پشت فاکتور
                MABL_VAR.IsReadOnly = !ican; //مبلغ واریزی
                MOIN_VAR.IsReadOnly = !ican; //معین واریزی
                MABL_HAV.IsReadOnly = !ican; //مبلغ حواله
                MOIN_HAV.IsReadOnly = !ican; //معین حواله
                TAKHFIF.IsReadOnly = !ican; //مبلغ تخفیف
                TAKHFIF_PERCENT.IsReadOnly = !ican; //درصد تخفیف
                MABL_HAZ.IsReadOnly = !ican; //مبلغ خدمات
                MOIN_HAZ.IsReadOnly = !ican; //مبلغ خدمات
                MBAA.IsReadOnly = !ican; //مالیات
                HMBAA.IsReadOnly = !ican; //معین مالیات

                CMB_MOIN_VAR.IsEnabled = ican; //معین واریزی کمبوباکس
                CMB_MOIN_HAV.IsEnabled = ican; //معین حواله کمبوباکس
                CMB_MOIN_HAZ.IsEnabled = ican; //معین خدمات
                CMB_HMBAA.IsEnabled = ican; //معین خدمات


                BTN_SAVE.IsEnabled = ican;
            }
        }

        public double Meidnum { get; private set; }
        public double? NUMBER1_TAG { get; private set; } = null;
        public int ANBARDefaultValue { get; private set; }
        public Visual I_AM_BARGASHT_KH { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_BARGASHT_KH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();


            string WhereCondition = FTAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) " : "  ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(FTAG, WhereCondition);

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

            if (!IsOpenedFromAutomation && !string.IsNullOrEmpty(OpenArgs) && _navigationManager.NUMBER_TO_OPEN != null) //Had a paramter passed
            {
                //یعنی این شماره رو پیدا نکرده که اون رو ریست کنه
                new Msgwin(false, $"شما به شماره {_navigationManager.NUMBER_TO_OPEN} دسترسی ندارید ").Show();
                try { this?.Close(); } catch { }
                return;
            }

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            NUMBER.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (INVO_LST_SUB_IsFocused)
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

                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            catch { /*ignore*/ }

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
        public void Form_Current()
        {
            bool ghat = false;

            if ((bool)Baseknow.SIGN)
            {
                if (SGN2.IsChecked == true)
                {
                    Command100.IsEnabled = true;
                    Command106.IsEnabled = true;
                }
                else
                {
                    Command100.IsEnabled = false;
                    Command106.IsEnabled = false;
                }
            }

            if (string.IsNullOrEmpty(N_S.Text))
            {
                //this.AllowDeletions = true;
                //this.AllowEdits = true;
                //INVO_LST_SUB.IsReadOnly = false;
                //Page58.IsEnabled = true;
                //lsanad.Foreground = Brushes.Yellow;
                //MABNA.Text = null;
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
                        //Page58.IsEnabled = false;
                        //lsanad.Foreground = Brushes.Red;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        INVO_LST_SUB.IsReadOnly = false;
                        //Page58.IsEnabled = true;
                        //lsanad.Foreground = Brushes.Yellow;
                    }
                }
            }

            if (Baseknow.MAND)
            {
                if (!CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue.ToStringNullSafe()) && CUST_NO.SelectedValue != null)
                {
                    if (CUST_NO.SelectedValue != null)
                    {
                        MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                    }
                }
                else
                {
                    MANDAH.Text = "مسدود است";
                }
            }

            if (NewRecord)
            {
                //Page58.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                if (!ghat)
                {
                    INVO_LST_SUB.IsReadOnly = false;
                    //Page58.IsEnabled = true;
                }
                else
                {
                    //Page58.IsEnabled = false;
                    INVO_LST_SUB.IsReadOnly = true;
                }
            }

            SecurityAllCheck();


            if (OKF.IsChecked != null && OKF.IsChecked == true && !NewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_SUB.IsReadOnly = true;
                //Page58.IsEnabled = false;
                ESLAH.IsEnabled = true;
            }

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, FTAG, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

        }

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
           new SearchableProperty { DisplayName = "شماره فاکتور برگشت خرید", PropertyPath = "NUMBER1", PropertyType = typeof(double) },
           new SearchableProperty { DisplayName = "شماره رسید", PropertyPath = "NUMBER", PropertyType = typeof(double) },
           new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
           new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
           new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
           new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
           // Add other searchable properties
       };
        }
        #endregion

        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshNew(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
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
                if (HEADER_FAC is null)
                {
                    new Msgwin(false, "این فاکتور خالی است").Show();
                    return;
                }
                //NewRecord = false; //Currrent Record is not new

                NUMBER.Text = HEADER_FAC.NUMBER.ToString();

                ReGetDataMaster(false);

                ReGetDataAll();

                Summer();

                GetBalancePerson();

                TAKHFIF_MABL_PRICE();

                ActivateChaps();


                AllowEdits = false;
                BTN_SAVE.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
                BTN_DELETE.IsEnabled = false;

                Form_Current();
            }
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
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        public void ClearFreshNew()
        {
            NUMBER1.SelectedIndex = -1; NUMBER1.Items.Refresh(); //شماره فاکتور

            NUMBER.Text = "0"; //شماره حواله

            NUMBER.Tag = null;
            NUMBER1.Tag = null;
            NUMBER1_TAG = null;

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER; SHIFT.Items.Refresh(); //شیفت این کاربر
            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 

            OKF.IsChecked = false; //تایید فاکتور

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOLAH.Text = null;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            MOGU.Text = null; //موجودی

            TEDADM.Text = "0"; //جمع مقادیر
            JJKOL.Text = "0"; //جمع فاکتور
            FNUMCO.Text = "0";

            MANDAH.Text = null;
            N_S.Text = "0"; //ثبت در سند
            MABNA.Text = "0"; //ثبت در سند

            //پشت فاکتور
            M_NAGHD.Text = "0"; //مبلغ نقد
            TAKHFIF_PERCENT.Text = "0"; //Reset درصد تخفیف برای جلوگیری از تداخل و محاسبه اشتباه

            TAKHFIF.Text = "0"; //مبلغ تخفیف
            MABL_HAZ.Text = "0"; //مبلغ خدمات
            MOIN_HAZ.Text = null; //معین خدمات
            MBAA.Text = "0"; //مبلغ مالیات
            HMBAA.Text = null; //معین مالیات

            JF.Text = "0"; //جمع کل فاکتور
            HKH.Text = "0"; //هزینه خدمات
            NTKHFIF.Text = "0"; //تخفیفات
            GHABEL.Text = "0";//مبلغ قابل پرداخت
            NPAR.Text = "0"; //جمع مبالغ پرداختی
            MAN.Text = "0"; //مانده

            NCHK.Text = "0";

            INVO_LST_FACTOR22_DATA.Clear();
            PAY_GETD_SUB22_DATA?.Clear(); //چک

            GetResids();

            Form_Current();

            AllowEdits = true;
        }
        private void ReGetDataMaster(bool IsNumberSelectedNow)
        {
            //DATE_N_AfterUpdate
            if (!IsNumberSelectedNow) //Is Not IsNumberSelectedNow
            {
                //از رسید انبار خرید
                var HEADER = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {FTAG}").FirstOrDefault(); //SELECT * FROM HEAD_LST WHERE NUMBER = 1 AND TAG = 3

                if (HEADER == null)
                {
                    new Msgwin(false, "چنین شماره فاکتور برگشت خریدی وجود ندارد !").ShowDialog();
                    this.Close(); return;
                }

                if (!((List<QRE_LST_BARGASHT>)NUMBER1.ItemsSource).Any(item => item?.NUMBER == HEADER.NUMBER1))
                {
                    ((List<QRE_LST_BARGASHT>)NUMBER1.ItemsSource).Add(new QRE_LST_BARGASHT { NUMBER = HEADER.NUMBER1 });
                }
                NUMBER1.SelectedValue = HEADER.NUMBER1; NUMBER1.Items.Refresh(); //NUMBER.Text = HEADER.NUMBER.ToStringNullSafe();

                string? TheCustomer = HEADER?.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + TheCustomer + "'").FirstOrDefault();
                if (CUST_NO.ItemsSource == null)
                {
                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                }
                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == TheCustomer))
                {
                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = TheCustomer, NAME = data.NAME });
                }
                CUST_NO.SelectedValue = HEADER?.CUST_NO; CUST_NO.Items.Refresh();
                //نوع مشتری
                CUST_KIND.SelectedValue = HEADER?.CUST_KIND; CUST_KIND.Items.Refresh();

                DATE_N.Text = HEADER.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER.USER_NAME.ToStringNullSafe(); //کاربر
                DEPATMAN.SelectedValue = HEADER.DEPATMAN; DEPATMAN.Items.Refresh(); //واحد

                TAKHFIF_PERCENT.Text = "0"; //Reset درصد تخفیف برای جلوگیری از تداخل و محاسبه اشتباه

                //مستقیما از فاکتور خرید
                var _FNUMCO_ = dbms.DoGetDataSQL<double?>($"SELECT FNUMCO FROM dbo.HEAD_LST WHERE NUMBER1 = {NUMBER1.SelectedValue} AND TAG = 12").FirstOrDefault();
                if (_FNUMCO_ > 0)
                {
                    FNUMCO.Text = _FNUMCO_.ToStringNullSafe();
                }

                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3);

                SGN1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                SGN2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                SGN3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;

                OKF.IsChecked = HEADER.OKF; //تایید فاکتور
                MOLAH.Text = HEADER.MOLAH; //ملاحظات
                SHIFT.SelectedValue = HEADER.SHIFT; //شیفت


                TAKHFIF.Text = HEADER.TAKHFIF.ToStringNullSafe(); //مبلغ تخفیف

                //پشت فاکتور
                M_NAGHD.Text = HEADER.M_NAGHD.ToStringNullSafe(); //مبلغ نقد

                MABL_HAZ.Text = (string.IsNullOrEmpty(HEADER.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER.MABL_HAZ.ToStringNullSafe()); //مبلغ خدمات
                MOIN_HAZ.Text = HEADER.MOIN_HAZ; //معین خدمات
                MBAA.Text = HEADER.MBAA.ToStringNullSafe(); //مالیات و عوارض مبلغ
                HMBAA.Text = HEADER.HMBAA; //معین مالیات

                //مبلغ واریزی : MABL_VAR ============ معین واریزی : MOIN_VAR
                MABL_VAR.Text = HEADER.MABL_VAR.ToStringNullSafe();
                MOIN_VAR.Text = HEADER.MOIN_VAR.ToStringNullSafe();

                // مبلغ حواله : MABL_HAV ============== معین حواله : MOIN_HAV
                MABL_HAV.Text = HEADER.MABL_HAV.ToStringNullSafe();
                MOIN_HAV.Text = HEADER.MOIN_HAV.ToStringNullSafe();
            }
            else
            {

            }

            if (NUMBER1.SelectedValue == null)
            {
                return;
            }

            if (_navigationManager.IsNewRecord) //مقدار جدیدی انتخاب شده یا وارد شده
            {
                var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + NUMBER1.SelectedValue + $" AND TAG = {FAC1TAG}").FirstOrDefault(); //FAC1TAG
                string thevalue = HEADER_FAC?.CUST_NO;
                if (!string.IsNullOrEmpty(thevalue))
                {
                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();

                    if (CUST_NO.ItemsSource == null)
                    {
                        CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                    }

                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = HEADER_FAC.CUST_NO; CUST_NO.Items.Refresh();

                    //نوع مشتری
                    CUST_KIND.SelectedValue = HEADER_FAC.CUST_KIND; CUST_KIND.Items.Refresh();
                }
            }

          


            NUMBER1_TAG = Convert.ToDouble(NUMBER1.SelectedValue); //Save Last Valid Number

        }
        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(NUMBER1.Text) || NUMBER1.Text == "0")
            {
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                INVO_LST_SUB.IsReadOnly = false;
            }

            SecurityAllCheck();
        }

        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "FACTBKH", new WindowInteropHelper(this).Handle, this.GetType().Name);

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
            //نوع مشتری
            CUST_KIND.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND.DisplayMemberPath = "CUSTKNAME";
            CUST_KIND.SelectedValuePath = "CUST_COD";
            CUST_KIND.SelectedIndex = 0;

            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //واحد ها
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            DEPATMAN.ItemsSource = RST; DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectedIndex = 0;
            DEPATMAN.SelectedItem = 0;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;

            //انبار کالا
            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();


            //شماره رسید انبار 
            GetResids();


            //شیفت
            SHIFT.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT.SHIFT_ID, SHIFT.SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            SHIFT.DisplayMemberPath = "SHNAME";
            SHIFT.SelectedValuePath = "SHIFT_ID";
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER;

            //کبموباکس مجری پرسنل
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

            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";


            //پشت فاکتور بخش چک:
            #region POSHTE_FACTOR

            vAZColumn.ItemsSource = new List<VAZ_MODEL_CHECK>()
            {
                 new VAZ_MODEL_CHECK { VAZ = 1, NAME_VAZ = "نزد صندوق" },
                 new VAZ_MODEL_CHECK { VAZ = 2, NAME_VAZ = "نزد بانك" },
                 new VAZ_MODEL_CHECK { VAZ = 3, NAME_VAZ = "وصول شده" },
                 new VAZ_MODEL_CHECK { VAZ = 4, NAME_VAZ = "واگذار شده" },
                 new VAZ_MODEL_CHECK { VAZ = 5, NAME_VAZ = "برگشت شده" },
                 new VAZ_MODEL_CHECK { VAZ = 6, NAME_VAZ = "مسترد شده" }
            };

            //کمبوباکس های پشت فاکتور
            bANKColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();

            var HESNAMELST = dbms.DoGetDataSQL<CUSTOM_HESABHA>("SELECT N_KOL,NUMBER,TNUMBER, RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) AS hes, NAME FROM TDETA_HES").ToList();
            CMB_MOIN_VAR.ItemsSource = HESNAMELST.Where(w => w.N_KOL == Baseknow.BANKHA).ToList(); //معین واریزی
            CMB_MOIN_HAV.ItemsSource = HESNAMELST.ToList(); //معين حواله
            CMB_MOIN_HAZ.ItemsSource = HESNAMELST.ToList(); //معين خدمات
            CMB_HMBAA.ItemsSource = HESNAMELST.ToList(); //معین مالیات


            //دریافت چک:
            //به حساب کل
            n_KOLColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>("SELECT     NUMBER, NAME FROM TOTA_HES WHERE (NUMBER = " + Baseknow.BANKHA + ")ORDER BY NAME").ToList();
            //Giving All Data as Master:
            //معین بانک
            n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {Baseknow.BANKHA})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //تفضیلی
            n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.N_KOL) ={Baseknow.BANKHA}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME\r\n").ToList();

            //موقعیت چک
            sANDUGHColumn.ItemsSource = dbms.DoGetDataSQL<TDETA_HES_CHECK>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = 1)").ToList();

            #endregion
        }

        private void GetResids()
        {
            NUMBER1.ItemsSource = dbms.DoGetDataSQL<QRE_LST_BARGASHT>($"SELECT NUMBER FROM dbo.HEAD_LST WHERE (((HEAD_LST.TAG)={FAC1TAG})) ORDER BY HEAD_LST.NUMBER").ToList();
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

                    var Row = INVO_LST_SUB.SelectedItem as INVO_LST_FACTOR22;
                    if (Row != null)
                    {
                        var data = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE, ANBAR, MOGODI_A, MOGODI, MABL_M FROM dbo.STUF_STK WHERE CODE = N'{Row.CODE}' AND ANBAR = {Row.ANBAR}").FirstOrDefault();
                        if (data != null)
                        {
                            MOGU.Text = data.MOGODI.ToStringNullSafe();

                            var RST_NESBAT = dbms.DoGetDataSQL<double?>("SELECT NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + Row.CODE + "' AND ((VAHEDS.VAHED)= " + Row.VAHED_K + ")))").FirstOrDefault();
                            if (RST_NESBAT == null)
                            {
                                new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.").ShowDialog();
                            }
                            else
                            {
                                //Row.MEGHk = Row.MEGH * RST_NESBAT; ///*RST.Fields(2)*/ //MEGHKG
                            }
                        }
                    }
                }
            }
        }
        private void INVO_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as INVO_LST_FACTOR22;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || CurrentRow?.ANBAR == null || string.IsNullOrEmpty(CurrentRow?.CODE))
            {
                return;
            }

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


        public void INVO_LST_SUB_ReGetData()
        {
            if (NUMBER1.SelectedValue != null)
            {
                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
	                 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K,dbo.INVO_LST.MABL * dbo.INVO_LST.MEGH_MAR AS MABMAR, dbo.INVO_LST.FROM_A, 
	                 dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
	                 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
	                 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
	                 FROM	dbo.INVO_LST LEFT OUTER JOIN
	                 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
	                 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
	                 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
	                 WHERE	(dbo.INVO_LST.TAG = {FAC1TAG}) AND (dbo.INVO_LST.NUMBER={NUMBER1.SelectedValue}) ").ToList(); //-- NUMBER1

                INVO_LST_FACTOR22_DATA?.Clear();
                foreach (var item in QRE_LST)
                    INVO_LST_FACTOR22_DATA.Add(item);

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
                new Msgwin(false, "نام مشتری نمیتواند خالی باشد!").ShowDialog();
                return;
            }
            if (CUST_KIND.SelectedValue == null)
            {
                CUST_KIND.Focus();
                new Msgwin(false, "نوع مشتری نمیتواند خالی باشد!").ShowDialog();
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


            if (IsNull(CURRENT_ITEMS_ROW.ANBAR))
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
                            MOGU.Text = null;
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                        else
                        {
                            MOGU.Text = (Rst1.FirstOrDefault().MOGODI + Rst1.FirstOrDefault().MOGODI_A).ToString();
                        }
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
                            CURRENT_ITEMS_ROW.MABL_K = 0;
                            SERCHK sERCHK = new SERCHK(I_AM_BARGASHT_KH, CURRENT_ITEMS_ROW.ANBAR.ToString());
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
                                CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITEMS_ROW.ANBAR.ToString(), I_AM_BARGASHT_KH);
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

                        var RST00 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                        if (RST00.Count == 0)
                        {
                            MOGU.Text = null;
                        }
                        else
                        {
                            MOGU.Text = ((double)RST00.FirstOrDefault().MOGODI + RST00.FirstOrDefault().MOGODI_A).ToString();
                        }
                        //var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                        //if (RST.Count != 0)
                        //{
                        //    CURRENT_ITEMS_ROW.VAHED_K = RST.FirstOrDefault().VAHED;
                        //}

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

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
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
                    if (CURRENT_ITEMS_ROW.MABL == 0)
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = true;
                    }
                    else
                    {
                        var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                        var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                        if (!(THECELL1 is null))
                            THECELL1.IsTabStop = true;

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
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

                if (CURRENT_ITEMS_ROW.MABL_K != CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)
                {
                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
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
                    if (CURRENT_ITEMS_ROW.MABL == 0)
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = true;
                    }
                    else
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = false;

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
                #endregion
            }
            #endregion

            //مبلغ
            #region MABL
            if (e.Column.SortMemberPath == "MABL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABL = WAS_ROW_ITEM.MABL;
                    return;
                }
                if (
                    CURRENT_ITEMS_ROW.ANBAR is null ||
                    CURRENT_ITEMS_ROW.CODE is null ||
                    CURRENT_ITEMS_ROW.VAHED_K is null ||
                    CURRENT_ITEMS_ROW.MEGH is null ||
                    CURRENT_ITEMS_ROW.MEGHk is null
                    )
                {
                    return;
                }

                #region MABL_AfterUpdate
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;

                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
                else
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = false;

                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
                #endregion

            }
            #endregion

            //مبلغ کل
            #region MABL_K
            if (e.Column.SortMemberPath == "MABL_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                    return;
                }
                if (
                   CURRENT_ITEMS_ROW.ANBAR is null ||
                   CURRENT_ITEMS_ROW.CODE is null ||
                   CURRENT_ITEMS_ROW.VAHED_K is null ||
                   CURRENT_ITEMS_ROW.MEGH is null ||
                   CURRENT_ITEMS_ROW.MEGHk is null ||
                   CURRENT_ITEMS_ROW.MABL is null
                   )
                {
                    return;
                }

                if (Math.Round((double)CURRENT_ITEMS_ROW.MABL_K) != CURRENT_ITEMS_ROW.MABL_K)
                {
                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)CURRENT_ITEMS_ROW.MABL_K);
                }
                if (CURRENT_ITEMS_ROW.MEGHk == 0)
                {
                    CURRENT_ITEMS_ROW.MABL_K = 0;
                }
                else
                {
                    CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk;

                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = false;
                }


                if (CURRENT_ITEMS_ROW.MABL == 0 && !IsNull(CURRENT_ITEMS_ROW.CODE))
                {
                    if (CURRENT_ITEMS_ROW.MEGHk == 0)
                    {
                        if (CURRENT_ITEMS_ROW.MABL_K != 0)
                        {
                            CURRENT_ITEMS_ROW.MABL_K = 0;
                        }
                    }
                    else
                    {
                        if (CURRENT_ITEMS_ROW.MABL != CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk)
                        {
                            CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk;
                        }

                        var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                        var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                        if (!(THECELL1 is null))
                            THECELL1.IsTabStop = false;
                    }
                }
            }
            #endregion

            #region MEGH_MAR
            if (e.Column.SortMemberPath == "MEGH_MAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW))
                {
                    CURRENT_ITEMS_ROW.MEGH_MAR = WAS_ROW_ITEM.MEGH_MAR;
                    universControl.PopNotifyShow("مقدار مرجوعی نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else if (Math.Round((double)(Convert.ToDouble(ENTERED_VALUE_ROW /*MEGH_MAR*/) - CURRENT_ITEMS_ROW.MEGHk), 5) > 0)
                {
                    CURRENT_ITEMS_ROW.MEGH_MAR = WAS_ROW_ITEM.MEGH_MAR;
                    universControl.PopNotifyShow("مقدار مرجوعی بیش از مقدار مقدار فروش باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else if (CURRENT_ITEMS_ROW.MABL != null)
                {
                    //dbo.INVO_LST.MABL* dbo.INVO_LST.MEGH_MAR AS MABMAR
                    CURRENT_ITEMS_ROW.MABMAR = CURRENT_ITEMS_ROW.MABL * Convert.ToDouble(ENTERED_VALUE_ROW /*MEGH_MAR*/);
                }
            }
            #endregion

            TEDADM.Text = SUM_OF_MEGH_MAR.ToStringNullSafe();

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
            if (!BodyIsValid(TheRow))
            {
                return;
            }

            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
            }
            else //UPDATE
            {
                _qre = $@"UPDATE dbo.INVO_LST SET MEGH_MAR = {(TheRow.MEGH_MAR)} WHERE id = {TheRow.id}";

                var (errorMsgs, _, _, _) = IVM.CheckInventoryAndExecuteQuery<int>(new List<object> { TheRow }, _qre, null, false, isBarGashti: true);
                ErrosMessages.AddRange(errorMsgs);
            }

            List<STUF_STK_CSHARP> RST_STUF_STK = null;

            //انبار خالی نباشد
            if (TheRow?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {TheRow.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (!IsNull(TheRow.CODE))
            {
                RST_STUF_STK = IVM.TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).ToList();
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
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }

            if (TheRow.MEGH_MAR != null && TheRow.MEGHk != null)
            {
                if (Math.Round((double)(TheRow.MEGH_MAR - TheRow.MEGHk), 5) > 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $" این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مقدار مرجوعي از مقدار فروش بيشتر است" });
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

            Summer();

            TAKHFIF_MABL_PRICE();


            MasterTopErrorMessages.AddRange(ErrosMessages);

            SANAD();

            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

            PAY_GETD_SUB_ReGetData();

        }
        void VAHED_K_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.VAHED_K is null) { return; }
            if (CURRENT_ITEMS_ROW.MABL is null || CURRENT_ITEMS_ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST.FirstOrDefault().NESBAT;
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;
                }
                else
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;

                    if (CURRENT_ITEMS_ROW.MABL is not null && CURRENT_ITEMS_ROW.MEGHk is not null)
                    {
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
            }
        }
        void MEGH_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW.MABL is null || CURRENT_ITEMS_ROW.MEGHk is null)
            {
                return;
            }

            double min;
            long Temp;
            double MAND;
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
                CURRENT_ITEMS_ROW.MEGH_R = CURRENT_ITEMS_ROW.MEGH * RST0.FirstOrDefault().NESBAT;
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_SUB, (int)CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = true;

                }
                else
                {
                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_SUB, (int)CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = false;

                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
            }
            if (Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR != 0)
            {
                min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);
                if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                {
                    var RSTM0 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                    if (RSTM0.Count > 0)
                    {
                        MAND = (double)RSTM0.FirstOrDefault();
                        if (Math.Round((double)(RSTM0.FirstOrDefault() - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITEMS_ROW.ANBAR != 0 && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL = WAS_ROW_ITEM.MABL/*.TAG*/;
                            var RSTM1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                            string _where = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                            if (RSTM1.Count > 0)
                            {
                                RSTM1.FirstOrDefault().MOGODI = MAND;
                                RSTM1.FirstOrDefault().MOGODI_A = 0;
                            }
                        }
                        else
                        {
                            var RSTM2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                            var _where = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                            if (RSTM2.Count > 0)
                            {
                                RSTM2.FirstOrDefault().MOGODI = MAND - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR));
                                RSTM2.FirstOrDefault().MOGODI_A = 0;
                            }
                        }
                    }
                }
                else
                {
                    var _where = "CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                    var RSTM3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM dbo.STUF_STK {_where}").ToList();
                    if (RSTM3.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if (CURRENT_ITEMS_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                    {
                        if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR)) < min && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                        }
                    }
                    else if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) < min && Baseknow.MOJU)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                        msgwin.ShowDialog();
                        CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                        CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                        CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                    }
                }
            }
        }
        private void ActivateChaps()
        {
            if (((bool)SGN1.IsChecked && (bool)SGN2.IsChecked) || (bool)SGN3.IsChecked)
            {
                this.Command100.IsEnabled = true;
                this.Command106.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command106.IsEnabled = false;
            }
        }

        private bool IsNull(object? hTAF2)
        {
            string _inputy = hTAF2.ToStringNullSafe();
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

            if (NUMBER1.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره فاکتور نميتواند  خالي باشد." });
            }
            else
            {
                if (NewRecord)
                {
                    var RST = dbms.DoGetDataSQL<double?>($"SELECT HEAD_LST.NUMBER1 FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG})) GROUP BY HEAD_LST.NUMBER1 HAVING (((HEAD_LST.NUMBER1)= " + NUMBER1.SelectedValue + "))").FirstOrDefault();
                    if (RST != null)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "براي اين فاكتور قبلا فاكتور مرجوعي صادر گرديده است . آن را جستجو نموده و مقدار مرجوعي را در همانجا ثبت نمائيد و در فيلد توضيحات تاريخ مرجوع دوم را درج نمائيد" });
                    }
                }
            }

            if (DEPATMAN.SelectedValue is null)  //واحد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد نمیتواند خالی باشد." });
            }
            if (CUST_KIND.SelectedValue is null) //نوع مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع مشتری نمیتواند خالی باشد." });
            }
            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری نمیتواند خالی باشد." });
            }
            if (SHIFT.SelectedValue is null) //شیفت
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شیفت نمیتواند خالی باشد." });
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
                ErrosMessages.Add(new MsgModel { MessageText_U = " مشتري مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
            }

            if (!IsNull(this.CMB_HMBAA.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.HMBAA.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
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

            //MABL_VAR   -----------  MOIN_VAR  {معین واریزی}
            if (string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
            }
            if (!IsNull(this.CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }

            if (IsNull(this.CUST_KIND.SelectedValue) || CUST_KIND.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع  مشتري مشخص نشده است ....!" });
            }
            if (IsNull(this.SHIFT.SelectedValue) || SHIFT.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شيفت مشخص نشده است ....!" });
            }
            if (IsNull(this.DEPATMAN.SelectedValue) || DEPATMAN.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد فروش مشخص نشده است ....!" });
            }

            if (string.IsNullOrEmpty(CMB_MOIN_VAR.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_VAR.Text) > 0) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین واریزی مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_VAR.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_VAR.Text) || MABL_VAR.Text == "0")) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ واریزی مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_MOIN_HAV.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAV.Text) > 0) //معین حواله
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین حواله مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_HAV.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_HAV.Text) || MABL_HAV.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ حواله مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MBAA.Text) > 0) //مالیات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مالیات مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MBAA.Text) || MBAA.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ مالیات مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_HAZ.Text) || MABL_HAZ.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ خدمات مشخص نشده!" });
            }
            if (!IsNull(MOIN_HAZ.Text))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }
            if (!IsNull(this.HMBAA.Text))
            {
                if (CL_HESABDARI.ISTAF(this.HMBAA.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
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
            else
            {
                //if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                //{
                //    if (TheRow.MEGH == 0)
                //    {
                //        ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صفر نمیتواند باشد" });
                //    }
                //}
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
            // Validate MABL
            if (!double.TryParse(TheRow.MABL?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کالا صحیح وارد نشده" });
            }
            // Validate MABL_K
            if (!double.TryParse(TheRow.MABL_K?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کل,  کالا صحیح وارد نشده" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (TheRow.MEGHk != null)
            {
                if (Math.Round((double)(TheRow.MEGH_MAR - TheRow.MEGHk), 5) > 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $" این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مقدار مرجوعي از مقدار فروش بيشتر است" });
                }
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
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            errors = (from object i in PAY_GETD_SUB22.ItemsSource
                      let c = PAY_GETD_SUB22.ItemContainerGenerator.ContainerFromItem(i)
                      where c != null && Validation.GetHasError(c)
                      select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            try
            {
                if (NUMBER.Text == "0")
                {
                    //Max Of Number TAG -----4
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
                                NUMBER.Text = Baseknow.STHFR.ToString();
                                NUMBER.UpdateLayout();
                            }
                            else
                            {
                                NUMBER.Text = Convert.ToDouble(rst_11 + 1).ToString();
                                NUMBER.UpdateLayout();
                            }

                            db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER,         TAG,     DATE_N,  MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF)
                                                        VALUES ({NUMBER.Text},  {FTAG},    0,    0,   0,       0,        0,        0,        0,    0   )", null, transaction);

                            transaction.Commit();
                            db?.Close();

                            RefreshAfterUpdate();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"در حال حاضر شماره {NUMBER.Text} توسط کاربر دیگری ثبت شده , شماره دیگری انتخاب کنید").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }

            try
            {
                DoCmdHeaderSave();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"شماره برگه ای را که تغییر داده اید {NUMBER.Text} توسط کاربر دیگری ثبت شده , شماره دیگری انتخاب کنید").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }



            this.OKF.IsChecked = true;

            this.INVO_LST_SUB.IsReadOnly = false;
            this.INVO_LST_SUB.IsReadOnly = false;
            this.Page58.IsEnabled = true;


            SANAD();

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, FTAG, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }

            DataGridActivation();
            Page57.IsEnabled = true;
            ChangeIsHappend = false;
        }

        private void GetBalancePerson()
        {
            //کادر سبز و سند و مانده حساب
            var SANAD_NUMBER = dbms.DoGetDataSQL<string>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (SANAD_NUMBER != null)
            {
                if (CUST_NO.SelectedValue != null)
                {
                    MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                }
                N_S.Text = SANAD_NUMBER?.ToString();
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S  = 3 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
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
                    SET NUMBER = {NUMBER.Text}, NUMBER1 = {NUMBER1.SelectedValue}, DATE_N = {DATE_N.Text.ToRawTarikh()}, 
                    N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    FNUMCO = {(string.IsNullOrEmpty(FNUMCO.Text) ? "0" : FNUMCO.Text)}, 
                    MABL_VAR = {MABL_VAR.Text},
                    MOIN_VAR = N'{CMB_MOIN_VAR.SelectedValue}',
                    MABL_HAV = {MABL_HAV.Text},
                    MOIN_HAV = N'{CMB_MOIN_HAV.SelectedValue}',
                    MABL_HAZ = {MABL_HAZ.Text},
                    MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}',
                    TAKHFIF = {TAKHFIF.Text},
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = N'{CMB_HMBAA.SelectedValue}', 
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    ANBAR =  {(ANBAR is null ? "NULL" : ANBAR)},
                    USER_NAME = N'{USER_NAME.Text}',
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";

            _ = dbms.DoExecuteSQL(_qre);


            return true;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!IsNull(this.NUMBER.Text) && NUMBER.Text != "0")
            {
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                    return;
                }

                SecurityAllCheck();

                if (!IsNull(this.NUMBER.Text) && NUMBER.Text != "0")
                {

                    DateTime dt = DateTime.Now;
                    if (!IsNull(this.NUMBER.Text))
                    {
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FAC1TAG})", dt, 1);
                        CL_HESABDARI.TR("PAY_GETD", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);

                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        this.INVO_LST_SUB.IsReadOnly = false;
                        this.Page58.IsEnabled = true;

                        if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                        {
                            this.INVO_LST_SUB.IsReadOnly = true; //.Locked = true;
                            this.PAY_GETD_SUB22.IsEnabled = false;
                            this.DATE_N.IsReadOnly = true;
                            this.MOLAH.IsReadOnly = true;
                            this.AllowEdits = true;

                        }
                        else
                        {
                            this.INVO_LST_SUB.IsReadOnly = false;
                            this.PAY_GETD_SUB22.IsEnabled = true;
                            this.MOLAH.IsReadOnly = false;
                            this.DATE_N.IsReadOnly = false;
                            this.AllowEdits = true;
                        }
                    }
                }
            }

        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord) { return; }

            if (PAY_GETD_SUB22_DATA.Count > 0)
            {
                new Msgwin(false, "این فاکتور دارای اطلاعات چک است , ابتدا آنرا حذف کنید سپس مجددا اقدام کنید.").ShowDialog();
                return;
            }

            if (SUM_OF_MEGH_MAR > 0)
            {
                new Msgwin(false, "برای حذف کردن این برگشتی ابتدا باید تمامی مقادیر مرجوعی را صفر کنید و سپس مجددا اقدام کنید").ShowDialog();
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                #region SABEGHEH
                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FAC1TAG})", dt, 1);
                CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                #endregion

                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "فاکتور برگشت خرید - عادی",
                        recordId: NUMBER.Text,
                        oldValue: "TAG = 3",
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER1.Text) && NUMBER1.Text != "0")
                {
                    try
                    {
                        SANAD();

                        dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER1 = {NUMBER1.Text} AND TAG = {FTAG}");

                        _navigationManager?.DeleteCurrentRecord(); //Refresh Record Source //ClearFreshNew();
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
        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            new FACTORS_LST(FTAG).Show();
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void Summer()
        {
            JJKOL.Text = SUM_OF_MABMAR_MABLK.ToString(); //SMABLK //جمع فاکتور : Sum(MABMAR)
            HKH.Text = MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL.Text; //جمع کل فاکتور برای فسمت روی فاکتور

            TEDADM.Text = SUM_OF_MEGH_MAR.ToString(); //جمع مقادیر مرجوعی :

            NCHK.Text = PAY_GETD_SUB22_DATA.Sum(x => x.MABL)?.ToString(); //جمع مبالغ چکهای پرداختی

            ////مبلغ قابل پرداخت: //= [JF] + [HKH] - [NTKHFIF] + [MBAA]
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            ////جمع مبالغ پرداختی
            ////=[M_NAGHD]+[MABL_VAR]+[MABL_HAV]+[NCHK]
            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(NCHK.Text) + Convert.ToInt64(MABL_VAR.Text) + Convert.ToInt64(MABL_HAV.Text);
            NPAR.Text = RMP.ToString();


            ////=[GHABEL]-[NPAR]
            MAN.Text = Convert.ToString(Convert.ToInt64(GHABEL.Text) - Convert.ToInt64(NPAR.Text)); //مانده

            GetBalancePerson();
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null; PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (!NewRecord && PERSONEL.SelectedItem != null)
            {
                string SelectedTextCMB = ((COMBOPERSONEL)PERSONEL.SelectedItem).SAL_NAME.ToStringNullSafe();

                Meidnum = CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور برگشت خرید عادی  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($"ارجاع داده به {SelectedTextCMB} شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }
        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + NUMBER.Text + $",{FTAG} )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور برگشت خرید عادی شماره: " + NUMBER.Text + " مورخ " + DATE_N.Text.ToRawTarikh() + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + NUMBER.Text + $",{FTAG} )");
            }

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if ((bool)!this.OKF.IsChecked)
                this.OKF.IsChecked = true;

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();
            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(this.SGN1.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            Form_Current(); //WinSignActivator();
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + NUMBER.Text + $",{FTAG} )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور برگشت خرید عادی  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + NUMBER.Text + $",{FTAG} )");
            }


            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;
            this.SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();

            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(this.SGN2.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            Form_Current(); //WinSignActivator();
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG} )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور برگشت خرید عادی  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG} )");
            }
            ////CL_HESABDARI.PERSONELUpdate(HTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خريد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            this.SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();
            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(this.SGN3.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            Form_Current(); //WinSignActivator();
        }
        private void WinSignActivator()
        {
            if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
            {
                AllowEdits = false;
                AllowDeletions = false;

                //Page58.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                AllowEdits = true;
            }
        }

        private void SANAD()
        {
            #region SANAD
            string SHART;
            int i;
            double KHMAVAV;
            double? max_ns, MABL_CHK = null, JAMF, JAMCH, CKOL = null, CMOIN = null,
                CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null,
                HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null;

            double KHNIM;
            double KHSAKHT;
            double KHSAY;
            var BAZAR = default(double);
            var HS = new double[8];
            string shart = "";
            double? N_S = null;

            List<DEED_HED> SHRST = null;
            var HEDRST = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE (TAG={FTAG}) AND (NUMBER >=" + NUMBER.Text + ") AND (NUMBER <=" + NUMBER.Text + ")").FirstOrDefault();
            if (HEDRST == null)
            {
                return;
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                CL_HESABDARI.GETTAF3(CUST_NO.SelectedValue.ToStringNullSafe(), ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
            }
            if (HEDRST?.N_S == null || HEDRST?.N_S == 0)
            {
                var SHARH_S = Strings.Right(" فاكتور برگشت خرید شماره " + NUMBER.Text + " مورخ " + Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##") + " فروشنده: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()), 100);
                max_ns = CL_HESABDARI.Createsanad(Convert.ToInt64(HEDRST.DATE_N), SHARH_S, 0, FTAG, Convert.ToByte(true), HEDRST.USER_NAME);
                HEDRST.N_S = max_ns;
            }
            else
            {
                shart = "NO_S = 3 AND N_S = " + HEDRST.N_S;
                SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart}").ToList();

                max_ns = SHRST.FirstOrDefault().N_S;
            }
            if (IsNull(HEDRST.N_S) || HEDRST.N_S != max_ns)
            {
                HEDRST.N_S = max_ns;
            }
            //if (SHRST.RecordCount == 0 || Information.Err() != 0)
            //{
            //    Information.Err().Clear();
            //    SHRST.AddNew();
            //    SHRST.Fields("N_S") = max_ns;
            //    SHRST.Fields("DATE_S") = this.DATE_N;
            //    SHRST.Fields("SHARH_S") = Strings.Left("فاكتور برگشت خريدشماره " + this.NUMBER + " مورخ " + Strings.Format(this.DATE_N, "####/##/##"), 255);
            //    SHRST.Fields("GHATEI") = 0;
            //    SHRST.Fields("NO_S") = 3;
            //    SHRST.Fields("OKF") = true;
            //    SHRST.Fields("USER_NAME") = UCurrentUser();
            //    SHRST.Fields("CRT") = DateTime.Now;
            //    SHRST.Fields("UID") = Forms["BASEKNOW"]["USERCOD"];
            //    SHRST.update();
            //    MABNA = SHRST.Fields("BASE");
            //}
            //else
            //{
            //    max_ns = SHRST.Fields("N_S");
            //    SHRST.Fields("DATE_S") = this.DATE_N;
            //    SHRST.Fields("SHARH_S") = Strings.Left("فاكتور برگشت خريد شماره" + this.NUMBER + " مورخ " + Strings.Format(this.DATE_N, "####/##/##"), 255);
            //    SHRST.Fields("GHATEI") = 0;
            //    SHRST.Fields("NO_S") = 3;
            //    SHRST.Fields("OKF") = true;
            //    SHRST.Fields("USER_NAME") = UCurrentUser();
            //    SHRST.Fields("CRT") = DateTime.Now;
            //    SHRST.Fields("UID") = Forms["BASEKNOW"]["USERCOD"];
            //    SHRST.update();
            //    MABNA = SHRST.Fields("BASE");
            //}
            //if (IsNull(this.N_S) | this.N_S != max_ns)
            //{
            //    this.N_S = max_ns;
            //}

            var JST0 = dbms.DoGetDataSQL<double?>("SELECT Sum([MEGH_MAR]*[mabl]) AS mabk  FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER1.SelectedValue + ") AND ((INVO_LST.TAG)=1))").FirstOrDefault();
            if (JST0 != null)
            {
                JAMF = JST0;
            }
            else
            {
                JAMF = 0d;
            }

            var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETD.MABL) AS SumOfMABL FROM PAY_GETD WHERE (((PAY_GETD.TAG)=3) AND ((PAY_GETD.NUMBER)= " + NUMBER.Text + " ))").FirstOrDefault();
            if (JST != null)
            {
                JAMCH = JST;
            }
            else
            {
                JAMCH = 0d;
            }

            dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + NUMBER.Text + ") AND ((DEED_DTL.TAG)= " + FTAG + "))");
            //SDRST.Open("deed_DTL", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            if (JAMF + Convert.ToDouble(MBAA.Text) > 0)
            {
                //SDRST.AddNew(); // كل بدهكاري شخص بابت فاكتور

                string insertSQL = @"
                INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, RADIF)
                VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @hes, @SHARH, @BED, @NUMBER, @TAG, @RADIF)";
                var parameters = new
                {
                    N_S = max_ns,
                    HES_K = CKOL,
                    HES_M = CMOIN,
                    HES_T = CTAF,
                    HES_T2 = (object)CTAF2 ?? DBNull.Value,
                    HES_T3 = (object)CTAF3 ?? DBNull.Value,
                    HES_T4 = (object)CTAF4 ?? DBNull.Value,
                    hes = CUST_NO.SelectedValue.ToStringNullSafe(),
                    SHARH = Strings.Right("فاكتور برگشت خريد شماره " + NUMBER.Text + " مورخ" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##"), 255),
                    BED = JAMF + Convert.ToDouble(MBAA.Text),
                    NUMBER = NUMBER.Text,
                    TAG = FTAG, //3
                    RADIF = NUMBER.Text
                };
                dbms.DoExecuteSQL(insertSQL, parameters);


            }
            if (this.MABL_HAZ.Text != "0")
            {
                //SDRST.AddNew(); // كل بدهكاري شخص بابت خدمات
                var _SHARH_ = Strings.Right("خدمات فاكتور برگشت خريد  شماره " + NUMBER.Text + "-" + this.FNUMCO.Text + " مورخ" + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##"), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,               hes,              SHARH,              BED,     NUMBER,      TAG)
		                                             VALUES ({max_ns},   {CKOL}, {CMOIN}, {CTAF},{(CTAF2 is null ? "NULL" : CTAF2)},{(CTAF3 is null ? "NULL" : CTAF3)},{(CTAF4 is null ? "NULL" : CTAF4)}, N'{CUST_NO.SelectedValue}', N'{_SHARH_}', {MABL_HAZ.Text}, {NUMBER.Text}   ,3)");

            }
            if (this.MABL_HAZ.Text != "0")
            {
                //SDRST.AddNew(); // كرايه حمل يا غيره
                if (!IsNull(this.MOIN_HAZ.Text))
                {
                    CL_HESABDARI.GETTAF3(this.MOIN_HAZ.Text, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                }
                var SHARH = Strings.Right("خدمات فاكتور برگشت خريد شماره " + NUMBER.Text + " - " + CL_HESABDARI.GETTAFNAME(this.MOIN_HAZ.Text), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,       hes,              SHARH,        BES,         NUMBER,      TAG)
		                                             VALUES ({max_ns},   {HKOL}, {HMOIN}, {HTAF},{(HTAF2 is null ? "NULL" : HTAF2)},{(HTAF3 is null ? "NULL" : HTAF3)},{(HTAF4 is null ? "NULL" : HTAF4)}, N'{MOIN_HAZ.Text}', N'{SHARH}', {MABL_HAZ.Text}, {NUMBER.Text}   ,3)");

            }

            if (JAMCH != 0d) // چكهاي دريافتي
            {
                var CHRST = dbms.DoGetDataSQL<PAY_GETD>("SELECT PAY_GETD.N_SERI, PAY_GETD.BANK, PAY_GETD.DATE_S, PAY_GETD.DATE, PAY_GETD.SHOBEH, PAY_GETD.MABL, PAY_GETD.NAME_TAH, PAY_GETD.N_HESAB, PAY_GETD.N_S, PAY_GETD.N_KOL, PAY_GETD.N_MOIN, PAY_GETD.N_TAF, PAY_GETD.N_KOL2, PAY_GETD.N_MOIN2, PAY_GETD.N_TAF2, PAY_GETD.N_KOL3, PAY_GETD.N_MOIN3, PAY_GETD.N_TAF3, PAY_GETD.NUMBER, PAY_GETD.TAG, PAY_GETD.ANBAR, PAY_GETD.RADIF, PAY_GETD.CUST_NO, PAY_GETD.VAZ FROM PAY_GETD WHERE (((PAY_GETD.NUMBER)=" + NUMBER.Text + ") AND ((PAY_GETD.TAG)=" + FTAG + "))").ToList();
                if (CHRST.Count > 0 && !IsNull(CHRST.FirstOrDefault().NUMBER))
                {
                    foreach (var row in CHRST) //while (!CHRST.EOF)
                    {
                        MABL_CHK = MABL_CHK + row.MABL;

                        if (true) //SDRST.AddNew(); // اسناد دريافتني
                        {
                            var HES_K = CL_HESABDARI.GETKOL(Baseknow.ADA);
                            var HES_M = CL_HESABDARI.GETMOIN(Baseknow.ADA);
                            var HES_T = CL_HESABDARI.GETTAF(Baseknow.ADA);
                            var hes = Baseknow.ADA;
                            var SHARH = Strings.Right("چك " + row.N_SERI + "بانك " + CL_HESABDARI.GETBANK(row.BANK) + " " + row.SHOBEH + " مورخ " + Strings.Format(row.DATE_S, "####/##/##"), 255);
                            var BED = row.MABL;
                            var N_SERI = row.N_SERI;
                            var BANK = row.BANK;
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T,  hes,   N_SERI ,   BANK   ,   SHARH,     BED,     NUMBER,      TAG)
                                                            VALUES ({max_ns},   {HES_K}, {HES_M}, {HES_T},{hes},  {N_SERI},  {BANK}  , N'{SHARH}', {BED}, {NUMBER.Text}   ,3)");
                        }


                        if (true) //SDRST.AddNew(); // چكهاي دريافتي
                        {
                            var SHARH = Strings.Right("ف.ف." + NUMBER.Text + " - " + "چك " + row.N_SERI + "بانك " + CL_HESABDARI.GETBANK(row.BANK) + " " + row.SHOBEH + " مورخ " + Strings.Format(row.DATE_S, "####/##/##"), 255);
                            var BES = row.MABL;
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,            hes,              SHARH,      BES,     NUMBER,      TAG)
                                                                    VALUES ({max_ns}, {CKOL}, {CMOIN}, {CTAF},{(CTAF2 is null ? "NULL" : CTAF2)},{(CTAF3 is null ? "NULL" : CTAF3)},{(CTAF4 is null ? "NULL" : CTAF4)}, N'{CUST_NO.SelectedValue}', N'{SHARH}', {BES}, {NUMBER.Text}   ,3)");
                        }

                    }
                }
            }

            if (this.M_NAGHD.Text != "0")
            {
                //SDRST.AddNew(); // مبلغ نقدشخص
                dbms.DoExecuteSQL(@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @hes, @SHARH, @BES, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = CKOL,
                        HES_M = CMOIN,
                        HES_T = CTAF,
                        HES_T2 = (object)CTAF2 ?? DBNull.Value,
                        HES_T3 = (object)CTAF3 ?? DBNull.Value,
                        HES_T4 = (object)CTAF4 ?? DBNull.Value,
                        hes = CUST_NO.SelectedValue.ToStringNullSafe(),
                        SHARH = Strings.Right($"مبلغ نقد فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BES = M_NAGHD.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });

                // مبلغ نقدصندوق
                dbms.DoExecuteSQL(@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @hes, @SHARH, @BED, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = Baseknow.SANDOGH,
                        HES_M = DEPATMAN.SelectedValue,
                        HES_T = SHIFT.SelectedValue,
                        hes = $"{Baseknow.SANDOGH}-{DEPATMAN.SelectedValue}-{SHIFT.SelectedValue}",
                        SHARH = Strings.Right($"مبلغ نقد فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BED = M_NAGHD.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });
            }

            if (this.TAKHFIF.Text != "0")
            {
                // تخفيف برگشت خريد
                dbms.DoExecuteSQL(@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @hes, @SHARH, @BED, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = Baseknow.TKHARID,
                        HES_M = 1,
                        HES_T = 1,
                        hes = $"{Baseknow.TKHARID}-1-1",
                        SHARH = Strings.Right($"مبلغ تخفيف فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BED = TAKHFIF.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });

                // مبلغ تخفيف شخص
                dbms.DoExecuteSQL(@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @hes, @SHARH, @BES, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = CKOL,
                        HES_M = CMOIN,
                        HES_T = CTAF,
                        HES_T2 = (object)CTAF2 ?? DBNull.Value,
                        HES_T3 = (object)CTAF3 ?? DBNull.Value,
                        HES_T4 = (object)CTAF4 ?? DBNull.Value,
                        hes = CUST_NO.SelectedValue,
                        SHARH = Strings.Right($"مبلغ تخفيف فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BES = TAKHFIF.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });
            }

            if (this.MABL_HAV.Text != "0")
            {
                // مبلغ حواله
                if (!string.IsNullOrEmpty(MOIN_HAV.Text))
                {
                    CL_HESABDARI.GETTAF3(MOIN_HAV.Text, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                }

                dbms.DoExecuteSQL(@"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @hes, @SHARH, @BED, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = HKOL,
                        HES_M = HMOIN,
                        HES_T = HTAF,
                        HES_T2 = (object)HTAF2 ?? DBNull.Value,
                        HES_T3 = (object)HTAF3 ?? DBNull.Value,
                        HES_T4 = (object)HTAF4 ?? DBNull.Value,
                        hes = MOIN_HAV.Text,
                        SHARH = Strings.Right($"مبلغ حواله فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BED = MABL_HAV.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });

                // مبلغ حواله شخص
                dbms.DoExecuteSQL(@"
                     INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG)
                     VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @hes, @SHARH, @BES, @NUMBER, @TAG)",
                    new
                    {
                        N_S = max_ns,
                        HES_K = CKOL,
                        HES_M = CMOIN,
                        HES_T = CTAF,
                        HES_T2 = (object)CTAF2 ?? DBNull.Value,
                        HES_T3 = (object)CTAF3 ?? DBNull.Value,
                        HES_T4 = (object)CTAF4 ?? DBNull.Value,
                        hes = CUST_NO.SelectedValue,
                        SHARH = Strings.Right($"مبلغ حواله فاكتور برگشت خريد شماره {NUMBER.Text} مورخ{Strings.Format(DATE_N.Text.ToRawTarikh(), "####/##/##")}", 255),
                        BES = MABL_HAV.Text,
                        NUMBER = NUMBER.Text,
                        TAG = FTAG
                    });
            }

            if (this.MABL_VAR.Text != "0")
            {
                //SDRST.AddNew(); // مبلغ واريزي

                if (!string.IsNullOrEmpty(MOIN_VAR.Text))
                {
                    CL_HESABDARI.GETTAF3(MOIN_HAV.Text, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                }
                var SHARH = Strings.Right("مبلغ واريزي فاكتور برگشت خريد شماره " + NUMBER.Text + " مورخ" + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##"), 255);
                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,       hes,              SHARH,        BED,     NUMBER,      TAG)
                                                   VALUES ({max_ns},   {HKOL}, {HMOIN}, {HTAF},{(HTAF2 is null ? "NULL" : HTAF2)},{(HTAF3 is null ? "NULL" : HTAF3)},{(HTAF4 is null ? "NULL" : HTAF4)},       N'{MOIN_VAR.Text}',  N'{SHARH}', {MABL_VAR.Text}, {NUMBER.Text}   ,3)");

            }
            if (this.MABL_VAR.Text != "0")
            {
                //SDRST.AddNew(); // مبلغ واريزي شخص
                var SHARH = Strings.Right("مبلغ واريزي فاكتور برگشت خريد شماره " + NUMBER.Text + " مورخ" + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##"), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,       hes,              SHARH,        BES,     NUMBER,      TAG)
                                                   VALUES ({max_ns},   {CKOL}, {CMOIN}, {CTAF},{(CTAF2 is null ? "NULL" : CTAF2)},{(CTAF3 is null ? "NULL" : CTAF3)},{(CTAF4 is null ? "NULL" : CTAF4)}, N'{CUST_NO.SelectedValue}',  N'{SHARH}', {MABL_VAR.Text}, {NUMBER.Text}   ,3)");
            }
            KHMAVAV = 0d;
            KHNIM = 0d;
            KHSAKHT = 0d;
            KHSAY = 0d;
            var JSTQ = dbms.DoGetDataSQL<QVIS3>("SELECT dbo.INVO_LST.CODE,dbo.INVO_LST.avrage2, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MEGH_MAR * dbo.INVO_LST.MABL AS mabk, dbo.INVO_LST.ANBAR, dbo.STUF_DEF.RADAH , dbo.STUF_DEF.name as nam  FROM dbo.INVO_LST INNER JOIN  dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.NUMBER = " + this.NUMBER1.SelectedValue + ") AND (dbo.INVO_LST.TAG = 1)").ToList();
            foreach (var row in JSTQ) //while (!JSTQ.EOF())
            {
                if (row.mabk != 0)
                {
                    //SDRST.AddNew(); // خريد
                    var hes = Baseknow.MOGODIA + "-" + row.ANBAR + "-" + row.CODE;
                    var SHARH = Strings.Right("برگشت خريد فاكتور شماره " + NUMBER.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);
                    var BES = Math.Round((double)(row.MEGH_MAR * row.avrage2));
                    switch (row.RADAH)
                    {
                        case 1:
                            {
                                KHMAVAV = (double)(KHMAVAV + row.mabk);
                                break;
                            }
                        case 2:
                            {
                                KHNIM = (double)(KHNIM + row.mabk);
                                break;
                            }
                        case 3:
                            {
                                KHSAKHT = (double)(KHSAKHT + row.mabk);
                                break;
                            }
                        case 4:
                            {
                                BAZAR = (double)(BAZAR + row.mabk);
                                break;
                            }
                        case 5:
                            {
                                HS[1] = (double)(HS[1] + row.mabk);
                                break;
                            }
                        case 6:
                            {
                                HS[2] = (double)(HS[2] + row.mabk);
                                break;
                            }
                        case 7:
                            {
                                HS[3] = (double)(HS[3] + row.mabk);
                                break;
                            }
                        case 8:
                            {
                                HS[4] = (double)(HS[4] + row.mabk);
                                break;
                            }
                        case 9:
                            {
                                HS[5] = (double)(HS[5] + row.mabk);
                                break;
                            }
                        case 10:
                            {
                                HS[6] = (double)(HS[6] + row.mabk);
                                break;
                            }

                        default:
                            {
                                KHSAY = (double)(KHSAY + row.mabk);
                                break;
                            }
                    }


                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,       HES_K,           HES_M,       HES_T,       hes,     SHARH,     BES,       NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.MOGODIA}, {row.ANBAR}, {row.CODE}, N'{hes}', N'{SHARH}', {BES}, {NUMBER.Text}      ,3)");
                }
                if (row.mabk != Math.Round((double)(row.MEGH_MAR * row.avrage2)))
                {
                    if (!CL_HESABDARI.ISHESAB(Baseknow.AMALKARD, 99999, Convert.ToInt64(row.CODE)))
                    {
                        try
                        {
                            CL_HESABDARI.CREATHES(Baseknow.AMALKARD, 99999, Convert.ToInt64(row.CODE), row.nam);
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "اخطار مهم ...! حساب متناظر كالا در عملكرد معين 99999 وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.").ShowDialog();
                        }
                    }

                    //SDRST.AddNew(); // خريد
                    var HES_K = Baseknow.AMALKARD;
                    var HES_M = 99999;
                    var HES_T = row.CODE;
                    var hes = Baseknow.AMALKARD + "-99999-" + row.CODE;
                    var SHARH = Strings.Right("برگشت خريد فاكتور شماره " + NUMBER.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                    double BESBED = 0;
                    string TASH = "";
                    if (row.mabk > row.MEGH_MAR * row.avrage2)
                    {
                        BESBED = (double)(row.mabk - Math.Round((double)(row.MEGH_MAR * row.avrage2)));
                        TASH = "BES";
                    }
                    else
                    {
                        BESBED = (double)(Math.Round((double)(row.MEGH_MAR * row.avrage2)) - row.mabk);
                        TASH = "BED";
                    }

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,   HES_K,    HES_M,  HES_T,     hes,     SHARH,     {TASH},      NUMBER,    TAG)
		                                             VALUES ({max_ns},   {HES_K}, {HES_M}, {HES_T}, N'{hes}', N'{SHARH}', {BESBED}, {NUMBER.Text}   ,3)");
                }
                //JSTQ.MoveNext();
            }

            if (KHMAVAV != 0d)
            {
                // كنترل خريد '
                //SDRST.AddNew(); // خريد
                var hes = Baseknow.KHARID + "-1-2";
                var SHARH = Strings.Right(" برگشت خريد مواد اوليه فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,     HES_M,  HES_T, hes,      SHARH,         BES,       NUMBER,      TAG)
		                                                 VALUES ({max_ns},   {Baseknow.KHARID}, {1}, {2}, N'{hes}', N'{SHARH}', {KHMAVAV}, {NUMBER.Text}   ,3)");
            }
            if (KHNIM != 0d)
            {
                // كنترل خريد '
                //SDRST.AddNew(); // خريد
                var hes = Baseknow.KHARID + "-2-2";
                var SHARH = Strings.Right("برگشت خريد نيمه ساخته فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,    HES_M, HES_T,   hes,       SHARH,    BES,     NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.KHARID}, {2}, {2}, N'{hes}', N'{SHARH}', {KHNIM}, {NUMBER.Text}   ,3)");

            }
            if (KHSAKHT != 0d)
            {
                // كنترل خريد '
                //SDRST.AddNew(); // خريد
                var hes = Baseknow.KHARID + "-3-2";
                var SHARH = Strings.Right("برگشت خريد ساخته شده فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,    HES_M,  HES_T, hes,       SHARH,      BES,      NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.KHARID}, {3}, {2}, N'{hes}', N'{SHARH}', {KHSAKHT}, {NUMBER.Text}   ,3)");
            }
            if (BAZAR != 0d)
            {
                // كنترل خريد '
                //SDRST.AddNew(); // خريد
                var hes = Baseknow.KHARID + "-4-2";
                var SHARH = Strings.Right("برگشت خريد بازرگاني فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,   HES_M,  HES_T,  hes,       SHARH,     BES,     NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.KHARID}, {4}, {2}, N'{hes}', N'{SHARH}', {BAZAR}, {NUMBER.Text}   ,3)");
            }
            if (KHSAY != 0d)
            {
                // كنترل خريد '
                if (!CL_HESABDARI.ISHESAB(Baseknow.KHARID, 11, 2))
                {
                    try
                    {
                        CL_HESABDARI.CREATHES(Baseknow.KHARID, 11, 2, "برگشت ساير 2");
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "اخطار مهم ...! حساب " + Baseknow.KHARID + "-11-2" + " اشكال دارد لطفا بررسي كنيد").ShowDialog();
                    }
                }
                //SDRST.AddNew(); // خريد
                var hes = Baseknow.KHARID + "-11-2";
                var SHARH = Strings.Right("برگشت خريد ساير فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,     HES_M,  HES_T,    hes,    SHARH,      BES,     NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.KHARID}, {11}, {2},  N'{hes}', N'{SHARH}', {KHSAY}, {NUMBER.Text}   ,3)");

            }
            for (i = 1; i <= 6; i++)
            {
                if (HS[i] != 0d)
                {
                    // كنترل خريد '
                    if (!CL_HESABDARI.ISHESAB(Baseknow.KHARID, i + 4, 2))
                    {
                        try
                        {
                            CL_HESABDARI.CREATHES(Baseknow.KHARID, i + 4, 2, "برگشت " + CL_HESABDARI.GETGRPKALA(i + 4));
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "اخطار مهم ...! حساب " + Baseknow.KHARID + "-" + (i + 4) + "-2" + " اشكال دارد لطفا بررسي كنيد").ShowDialog();
                        }
                    }
                    //SDRST.AddNew(); // خريد
                    var HES_M = i + 4;
                    var hes = Baseknow.KHARID + "-" + (i + 4) + "-2";
                    var SHARH = Strings.Right("برگشت خريد " + CL_HESABDARI.GETGRPKALA(i + 4) + " فاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);
                    var BES = HS[i];
                    HS[7] = HS[7] + HS[i];

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,    HES_M, HES_T,  hes,      SHARH,     BES,    NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.KHARID}, {HES_M}, {2}, N'{hes}', N'{SHARH}', {BES}, {NUMBER.Text}   ,3)");
                }
            }
            if (KHSAY + KHSAKHT + KHNIM + KHMAVAV + BAZAR + HS[7] > 0d)
            {
                //SDRST.AddNew(); // پاياپاي خريد
                var hes = Baseknow.PKHARID + "-1-1";
                var SHARH = Strings.Right("خريدفاكتورشماره " + this.NUMBER1.SelectedValue + "-" + this.FNUMCO.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "فروشنده: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToStringNullSafe()), 255);
                var BED = KHSAY + KHSAKHT + KHNIM + KHMAVAV + BAZAR + HS[7];

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,            HES_K,   HES_M,  HES_T,  hes,        SHARH,    BED,    NUMBER,      TAG)
		                                             VALUES ({max_ns},   {Baseknow.PKHARID}, {1}, {1}, N'{hes}', N'{SHARH}', {BED}, {NUMBER.Text}   ,3)");
            }
            if (this.MBAA.Text != "0")
            {
                //SDRST.AddNew(); // مالليات بر ارزش افزوده
                if (!IsNull(this.HMBAA.Text))
                {
                    CL_HESABDARI.GETTAF3(this.HMBAA.Text, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                }
                var SHARH = Strings.Right("% ماليات بر ارزش افزوده فاكتور خريد شماره " + this.NUMBER1.SelectedValue + " مورخ" + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##"), 255);

                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,   HES_T, HES_T2, HES_T3, HES_T4,       hes,              SHARH,          BES,     NUMBER,      TAG)
                                                          VALUES ({max_ns},   {HKOL}, {HMOIN}, {HTAF},{(HTAF2 is null ? "NULL" : HTAF2)},{(HTAF3 is null ? "NULL" : HTAF3)},{(HTAF4 is null ? "NULL" : HTAF4)}, N'{HMBAA.Text}', N'{SHARH}', {MBAA.Text}, {NUMBER.Text}   ,3)");
            }

            dbms.DoExecuteSQL($"UPDATE TOP (1) dbo.HEAD_LST SET N_S = {HEDRST.N_S} WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}");

            #endregion

            Summer();

            GetBalancePerson();
        }
        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            return;

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
                ComboSearch CMBSearch = new ComboSearch("HEAD_LST_KHAREED1", I_AM_BARGASHT_KH);//Search Plusy Form Specialy for Customers
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
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " AND NUMBER = 1 and TNUMBER = " + CUTSNO_TEX.Text).ToList();
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

            #region CUST_NO_Exit
            if (!IsNull(this.CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                }
            }
            #endregion

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

            ReGetDataMaster(true);
        }
        private void DATE_N_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE_N.Text.ToRawTarikh();
        }


        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_KH_BACK.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            double JCHK = 0, JAMF = 0, HAZ = 0, NAGHD = 0, VAR = 0, HAV = 0, taf = 0, MBAA = 0;


            if (Baseknow.TFSAZMAN != "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = true;
            }
            else
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
            }
            if (Baseknow.TFSAZMAN != "2")
            {
                var rst_0 = dbms.DoGetDataSQL<double?>("SELECT SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE (HES_K = " + CL_HESABDARI.GETKOL(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(this.CUST_NO.SelectedValue.ToString()) + ")").ToList();
                if (rst_0.Count == 0)
                {
                    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                }
                else
                {
                    var _mandah = Interaction.IIf(rst_0.FirstOrDefault() > 0, Strings.Format(rst_0.FirstOrDefault(), "##,# ريال بدهكار"), Strings.Format(rst_0.FirstOrDefault() * -1, "##,# ريال بستانكار"));
                    (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                }
            }

            // Calculate JAMF
            var jst = dbms.DoGetDataSQL<double?>($@"
                                            SELECT SUM(INVO_LST.MABL_K) AS SumOfMABL_K 
                                            FROM INVO_LST 
                                            WHERE INVO_LST.NUMBER = {NUMBER.Text} AND INVO_LST.TAG = {FAC1TAG}").FirstOrDefault();

            JAMF = jst ?? 0;

            // Fetch HEAD_LST data
            var headLst = dbms.DoGetDataSQL<HeadLstData>($@"
                                                   SELECT NUMBER, TAG AS htag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, 
                                                   M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, 
                                                   MOIN_KHF, ANBARF, FNUMCO, MBAA 
                                                   FROM HEAD_LST 
                                                   WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();

            if (headLst != null)
            {
                HAZ = headLst.MABL_HAZ;
                VAR = headLst.MABL_VAR;
                HAV = headLst.MABL_HAV;
                NAGHD = headLst.M_NAGHD;
                taf = headLst.TAKHFIF;
                MBAA = headLst.MBAA;
            }

            // Update report components
            (report.GetComponentByName("JF") as StiText).Text = JF.Text;
            var test = Convert.ToDouble(HKH.Text).ToString("#,#");
            if (HKH.Text != "0")
            {
                (report.GetComponentByName("HKH") as StiText).Text = Convert.ToDouble(HKH.Text).ToString("#,#");
            }
            (report.GetComponentByName("TF") as StiText).Text = NTKHFIF.Text;
            (report.GetComponentByName("MBAA") as StiText).Text = MBAA.ToString("#,##0;#,##0-");

            (report.GetComponentByName("GABEL") as StiText).Text = GHABEL.Text;
            (report.GetComponentByName("JPAY") as StiText).Text = NPAR.Text;
            //(report.GetComponentByName("NPAR") as StiText).Text = NPAR.Text;
            (report.GetComponentByName("MAN") as StiText).Text = MAN.Text;



            var rst03 = dbms.DoGetDataSQL<double?>("SELECT  SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = " + FAC1TAG /*TAG = 9 */ + ") AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").ToList();
            if (rst03.Count > 0)
            {
                if (!IsNull(rst03.FirstOrDefault()))
                {
                    var _VAZN_ = Math.Round((double)rst03.FirstOrDefault());
                    if (_VAZN_ > 0)
                    {
                        if (report.GetComponentByName("VAZN") is StiText vazn) vazn.Text = "وزن كل به كيلو : " + _VAZN_;
                    }
                    else
                    {
                        if (report.GetComponentByName("VAZN") is StiText vazn) vazn.Enabled = false;
                    }
                }
            }

            //(report.GetComponentByName("HR") as StiText).Text = $"{CL_HESABDARI.ALPHANUM(JAMF + HAZ - taf + MBAA)} ريال";
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(GHABEL.Text));


            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked)
            {
                (report.GetComponentByName("FIMG") as StiImage).Enabled = true;

                #region Sepratly_Get_Image_Emza
                var imageData = dbms.DoGetDataSQL<byte[]>($"SELECT TOP 1 EMZA FROM dbo.SALA_DTL WHERE IDD = {SGN1usid.Tag}").FirstOrDefault();
                if (imageData != null)
                {
                    var BitIMG = CL_LMethods.ByteArrayToBitmapImage(imageData);
                    if (BitIMG != null)
                    {
                        var DrawIMG = CL_LMethods.ConvertBitmapSourceToDrawingImage(BitIMG);
                        if (DrawIMG != null)
                        {
                            (report.GetComponentByName("FIMG") as StiImage).Image = DrawIMG;
                        }
                    }
                }
                #endregion

                (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
                (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN2.IsChecked)
            {
                (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

                #region Sepratly_Get_Image_Emza
                var imageData = dbms.DoGetDataSQL<byte[]>($"SELECT TOP 1 EMZA FROM dbo.SALA_DTL WHERE IDD = {SGN2usid.Tag}").FirstOrDefault();
                if (imageData != null)
                {
                    var BitIMG = CL_LMethods.ByteArrayToBitmapImage(imageData);
                    if (BitIMG != null)
                    {
                        var DrawIMG = CL_LMethods.ConvertBitmapSourceToDrawingImage(BitIMG);
                        if (DrawIMG != null)
                        {
                            (report.GetComponentByName("HIMG") as StiImage).Image = DrawIMG;
                        }
                    }
                }
                #endregion

                (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER;
                (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN3.IsChecked)
            {
                (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

                #region Sepratly_Get_Image_Emza
                var imageData = dbms.DoGetDataSQL<byte[]>($"SELECT TOP 1 EMZA FROM dbo.SALA_DTL WHERE IDD = {SGN3usid.Tag}").FirstOrDefault();
                if (imageData != null)
                {
                    var BitIMG = CL_LMethods.ByteArrayToBitmapImage(imageData);
                    if (BitIMG != null)
                    {
                        var DrawIMG = CL_LMethods.ConvertBitmapSourceToDrawingImage(BitIMG);
                        if (DrawIMG != null)
                        {
                            (report.GetComponentByName("MIMG") as StiImage).Image = DrawIMG;
                        }
                    }
                }
                #endregion

                (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER;
                (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER;
            }

            //(report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            //(report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            //(report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;

            //TXT_SELLER = فروشنده ======== =[Forms]![Baseknow]![NAME]
            if (report.GetComponentByName("TXT_SELLER") is StiText stiText1) stiText1.Text = Baseknow.NAME;

            //TXT_ADDRESS = آدرس =[Forms]![Baseknow]![TFADDRESS]
            if (report.GetComponentByName("TXT_ADDRESS") is StiText stiText2) stiText2.Text = Baseknow.TFADDRESS;

            //TXT_TELEPHONE = تلفن =[Forms]![Baseknow]![TFTEL]
            if (report.GetComponentByName("TXT_TELEPHONE") is StiText stiText3) stiText3.Text = Baseknow.TFTEL;

            //TXT_HIGH_D = متن پایین =[Forms]![Baseknow]![HIGH_D]
            if (report.GetComponentByName("TXT_HIGH_D") is StiText stiText4) stiText4.Text = Baseknow.HIGH_D;


            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();

        }
        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.HAVLAH_KHBACK.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
                                                                                   //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            //(report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;

            new WINRPT(report, LABEL_HEADER.Content.ToString()).Show();
            //report.Render(false);
            //report.Show();
        }
        private void NUMBER1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NUMBER1.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (NUMBER1.SelectedValue == null)
            {
                universControl.PopNotifyShow("چنین شماره رسید انباری وجود ندارد!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }


            //if (SUM_OF_MEGH_MAR > 0)
            //{
            //    new Msgwin(false, "اطلاعات سطرهاي فاكتور در ستون تعداد مرجوعي براي اعمال تغييرات صفر نمي باشد").ShowDialog();
            //    return;
            //}

            string title = "شماره رسید انبار";
            if (NewRecord)
            {
                var selected = NUMBER1.SelectedValue;
                bool alreadyUsed = dbms.DoGetDataSQL<int>($"SELECT COUNT(*) FROM HEAD_LST WHERE TAG = {FTAG} AND NUMBER = {selected}").First() > 0;
                if (alreadyUsed)
                {
                    new Msgwin(false, $"نمیتوانید {title} که قبلا ثبت کرده ای استفاده کنید").ShowDialog();
                    NUMBER1.SelectedValue = NUMBER1_TAG; NUMBER1.Items.Refresh();
                    return;
                }
            }
            else
            {
                if (Convert.ToDouble(NUMBER1.SelectedValue) != NUMBER1_TAG)
                {
                    new Msgwin(false, $"نمیتوانید {title} ی که قبلا ثبت کرده اید را تغییر دهید , تنها میتوانید این فاکتور را حذف نمایید , انتخاب{title} تنها در فاکتور جدید ممکن است").ShowDialog();
                    NUMBER1.SelectedValue = NUMBER1_TAG; NUMBER1.Items.Refresh();
                    return;
                }
            }

            #region NUMBER1_BeforeUpdate
            //if (this.NUMBER.TAG == 0)
            //{
            //    RST.Open("SELECT HEAD_LST.NUMBER1 FROM HEAD_LST WHERE (((HEAD_LST.TAG) = 3)) GROUP BY HEAD_LST.NUMBER1 HAVING (((HEAD_LST.NUMBER1)= " + this.NUMBER1 + "))", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            //    if (RST.RecordCount == 0 || IsNull(RST.Fields(0)))
            //    {
            //    }
            //    else
            //    {
            //        DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "براي اين فاكتور قبلا فاكتور مرجوعي صادر گرديده است . آن را جستجو نموده و مقدار مرجوعي را در همانجا ثبت نمائيد و در فيلد توضيحات تاريخ مرجوع دوم را درج نمائيد");
            //        CANCEL = Conversions.ToInteger(true);
            //        this.Undo();
            //        return;
            //    }
            //}
            #endregion


            double? SumOfMEGH_MAR = null;
            bool BargashtExistBefore = false;

            SumOfMEGH_MAR = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER1.SelectedValue + $" ) AND ((INVO_LST.TAG)={FAC1TAG}))").FirstOrDefault();
            var _NUMBER_ = dbms.DoGetDataSQL<double?>($"SELECT NUMBER1 FROM HEAD_LST WHERE TAG = {FTAG} AND NUMBER1 =" + NUMBER1.SelectedValue).FirstOrDefault();
            if (_NUMBER_ > 0)
            {
                BargashtExistBefore = true;
            }

            //if (NUMBER1_TAG > 0 && _NUMBER_ != NUMBER1_TAG)
            //{
            //    Msgwin msgwin = new Msgwin(true, "آیا از تغییر شماره حواله انبار مطمئن هستید"); msgwin.ShowDialog();
            //    if (msgwin.DialogResult == false) //NO
            //    {
            //        NUMBER1.SelectedValue = NUMBER1_TAG; NUMBER1.Items.Refresh(); return;
            //    }
            //}

            if ((SumOfMEGH_MAR > 0 || BargashtExistBefore) && NewRecord)
            {
                new Msgwin(false, $"{title}ی که انتخاب کرده اید قبلا مرجوعی آن ثبت شده است").ShowDialog();
                //new Msgwin(false, "اين فاكتور داراي اطلاعات مي باشديا اينكه قبلا مرجوعي آن ثبت شده. براي حذف فاكتور بايد كليه رديفهاي ستونهاي تعداد مرجوعي صفر باشد").ShowDialog();
                NUMBER1.SelectedValue = NUMBER1_TAG; NUMBER1.Items.Refresh();
                return;
            }
            else if (_NUMBER_ != null && _NUMBER_ != NUMBER1_TAG) //آیا شماره فاکتور مرجع تغییر کرده ؟!
            {
                new Msgwin(false, "اين فاكتور داراي اطلاعات مي باشديا اينكه قبلا مرجوعي آن ثبت شده. براي حذف فاكتور بايد كليه رديفهاي ستونهاي تعداد مرجوعي صفر باشد").ShowDialog();
                NUMBER1.SelectedValue = NUMBER1_TAG; NUMBER1.Items.Refresh();
                return;
            }
            else //IsSuccessfully
            {
                NUMBER1_TAG = (double)NUMBER1.SelectedValue;
                ReGetDataMaster(true);
                ReGetDataAll();

                //BTN_SAVE_Click(null, null);
            }


        }
        private void ReGetDataAll()
        {
            INVO_LST_SUB_ReGetData();
            PAY_GETD_SUB_ReGetData();
        }

        #region POSHTE_FACTOR
        private void MOIN_HAV_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region MOIN_HAV_Exit
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(MOIN_HAV.Text))
            {
                new Msgwin(false, "حساب معين مبلغ  وارد شده حتما بايد مشخص شود يا مبلغ صفر گردد").ShowDialog();
            }
            #endregion
        }
        private void MABL_HAZ_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAZ.Text) != 0 && (IsNull(this.MOIN_HAZ.Text) || MOIN_HAZ.Text == "0"))
            {
                var RST = dbms.DoGetDataSQL<string?>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes FROM dbo.TDETA_HES WHERE (N_KOL = " + Baseknow.HKHARID + $") AND (NUMBER = {FTAG})").FirstOrDefault();
                if (RST != null)
                {
                    MOIN_HAZ.Text = RST;
                }
                else
                {
                    new Msgwin(false, "حساب معين براي خدمات تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل هزينه خريد معين تعريف نمائيد.").ShowDialog();
                }
            }
        }
        private void MABL_HAZ_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            MABL_HAZ_AfterUpdate();
        }
        private void MOIN_HAZ_BeforeUpdate()
        {
            //MOIN_HAZ_BeforeUpdate
            if (!IsNull(this.CMB_MOIN_HAZ.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!").ShowDialog();
                }
            }
        }
        private void MOIN_HAZ_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            MOIN_HAZ_BeforeUpdate();
        }
        private void MBAA_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MBAA_AfterUpdate
            if (Strings.Right(this.MBAA.Text, 1) == "%")
            {
                MBAA.Text = Math.Round((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * Convert.ToDouble(MBAA.Text)).ToString();
            }
            if (Convert.ToDouble(MBAA.Text) - Math.Round(Convert.ToDouble(MBAA.Text)) != 0)
            {
                MBAA.Text = Math.Round(Convert.ToDouble(MBAA.Text)).ToString();
            }

            if (Convert.ToDouble(MBAA.Text) > 0 & IsNull(HMBAA.Text))
            {
                HMBAA.Text = Baseknow.HESMBAA;
            }
        }
        private void TAKHFIF_PERCENT_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TAKHFIF_PERCENT.Text))
            {
                var (isvalid, msg) = CL_LMethods.IsValidPercentage(TAKHFIF_PERCENT.Text);
                if (!isvalid)
                {
                    new Msgwin(false, msg).ShowDialog();
                }
                else
                {
                    TAKHFIF_MABL_PRICE(false);
                }
            }
        }
        private void TAKHFIF_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            TAKHFIF_MABL_PRICE(true);
        }
        private void TAKHFIF_MABL_PRICE(bool isTakhfifFocus = true)
        {
            Summer();

            if (!string.IsNullOrEmpty(TAKHFIF.Text) && TAKHFIF.Text != "0" && JF.Text != "0" && isTakhfifFocus) //درصد تخفیف
            {
                var TAKHFIF_TXT = Convert.ToDouble(TAKHFIF.Text);
                var JF_TXT = Convert.ToDouble(JF.Text);

                TAKHFIF_PERCENT.Text = Math.Round(TAKHFIF_TXT * 100 / JF_TXT, 2).ToString(); //Text101
            }
            else if (!string.IsNullOrEmpty(TAKHFIF_PERCENT.Text)) //مبلغ تخفیف
            {
                var DARSAD_TXT = Convert.ToDouble(TAKHFIF_PERCENT.Text); //Text101
                var JF_TXT = Convert.ToDouble(JF.Text);

                TAKHFIF.Text = Math.Round(JF_TXT * DARSAD_TXT / 100).ToString();
            }
        }
        public PAY_GETD_SUB22_MODEL? PAY_GETD_WAS_ROW_ITEM { get; set; }
        public string? OpenArgs { get; }

        public void PAY_GETD_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //PAY_GETD_SUB22_DATA
                PAY_GETD_SUB22_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<PAY_GETD_SUB22_MODEL>($@"SELECT * FROM PAY_GETD WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        PAY_GETD_SUB22_DATA.Add(item);
                    }
                }
            }
        }
        private void PAY_GETD_SUB22_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (PAY_GETD_SUB22.SelectedItem != null)
            {
                if (PAY_GETD_SUB22.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    PAY_GETD_WAS_ROW_ITEM = ((PAY_GETD_SUB22_MODEL)PAY_GETD_SUB22.SelectedItem).Clone() as PAY_GETD_SUB22_MODEL;
                }
            }
        }
        private void PAY_GETD_SUB22_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_

            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

            // = e.Column.SortMemberPath;
            var PAY_GETD_SUB22_ROW_INDEX = row_index;
            // = e.Column.DisplayIndex;

            //CELL
            //var rowContainer = INVO_LST_sub.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
            //DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
            //DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            //if (cell == null)
            //{
            //    INVO_LST_sub.ScrollIntoView(rowContainer, INVO_LST_sub.Columns[CURRENT_COLUMN_INDEX]);
            //    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            //}
            //var PAY_GETD_SUB22_CELL_ROW = cell;
            //CELL

            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            object PAY_GETD_SUB22_ENTERED_VALUE;
            if (!ReferenceEquals(Comboval, null))
                PAY_GETD_SUB22_ENTERED_VALUE = Comboval.SelectedValue;
            else
                PAY_GETD_SUB22_ENTERED_VALUE = TexboVal.Text.Trim();

            var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;
            #endregion

            #region SET_NULL_IF_ROW_IS_NOT_VALID
            //بررسی در صورت تغییر نال کردن برای جلوگیری از اشتباه
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (PAY_GETD_WAS_ROW_ITEM.N_KOL != PAY_GETD_SUB22_ROW_ITEMS.N_KOL) //تغییر یافته
                {
                    //معین بانک
                    var comboBox = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_MOIN").GetCellContent(e.Row) as ComboBox;
                    comboBox.ItemsSource = null;

                    //تفضیلی
                    var comboBox1 = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (PAY_GETD_WAS_ROW_ITEM.N_MOIN != PAY_GETD_SUB22_ROW_ITEMS.N_MOIN) //تغییر یافته
                {
                    //تفضیلی
                    var comboBox1 = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            #endregion

            //,N_MOIN,N_TAF 
            if (e.Column.SortMemberPath == "N_KOL")
            {
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
            }
            if (e.Column.SortMemberPath == "N_TAF")
            {
            }
            if (e.Column.SortMemberPath == "BANK")
            {
                #region BAN_AfterUpdate
                if (!IsNull(PAY_GETD_SUB22_ROW_ITEMS?.N_SERI) && !IsNull(PAY_GETD_SUB22_ROW_ITEMS?.BANK))
                {
                    if (PAY_GETD_SUB22_ROW_ITEMS?.ID == null || PAY_GETD_SUB22_ROW_ITEMS?.BANK != PAY_GETD_WAS_ROW_ITEM?.BANK || PAY_GETD_SUB22_ROW_ITEMS?.N_SERI != PAY_GETD_WAS_ROW_ITEM?.N_SERI)
                    {
                        var filter = "N_SERI=" + PAY_GETD_SUB22_ROW_ITEMS.N_SERI + " AND BANK = " + PAY_GETD_SUB22_ROW_ITEMS.BANK;
                        var rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {filter} ").FirstOrDefault();
                        if (rst != null)
                        {
                            new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود").ShowDialog();

                            var rst2 = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.ADA + "' OR HES = '" + Baseknow.ADV + "' ) AND (BES > 0) AND (BANK = "
                                + PAY_GETD_SUB22_ROW_ITEMS?.BANK + ") AND (N_SERI = " + PAY_GETD_SUB22_ROW_ITEMS.N_SERI + ")").FirstOrDefault();
                            if (rst2 != null)
                            {
                                new Msgwin(false, "اين چك در سند شماره " + rst2 + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را تغییر دهید").ShowDialog();
                            }
                            else
                            {
                                PAY_GETD_SUB22_ROW_ITEMS.ID = rst.ID; //برای اینکه آپدیت بشه نه INSERT

                                PAY_GETD_SUB22_ROW_ITEMS.N_SERI = rst.N_SERI;
                                PAY_GETD_SUB22_ROW_ITEMS.BANK = rst.BANK;

                                PAY_GETD_SUB22_ROW_ITEMS.DATE_S = rst.DATE_S;
                                PAY_GETD_SUB22_ROW_ITEMS.RADIF = rst.RADIF;
                                PAY_GETD_SUB22_ROW_ITEMS.SHOBEH = rst.SHOBEH;
                                PAY_GETD_SUB22_ROW_ITEMS.DATE = rst.DATE;
                                PAY_GETD_SUB22_ROW_ITEMS.NAME_TAH = rst.NAME_TAH;
                                PAY_GETD_SUB22_ROW_ITEMS.N_HESAB = rst.N_HESAB;
                                PAY_GETD_SUB22_ROW_ITEMS.MABL = rst.MABL;

                                if (rst?.N_KOL != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL = rst?.N_KOL;
                                if (rst?.N_MOIN != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN = rst?.N_MOIN;
                                if (rst?.N_TAF != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF = rst?.N_TAF;
                                if (rst?.N_TAF2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF2 = rst?.N_TAF2;
                                if (rst?.N_TAF3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF3 = rst?.N_TAF3;

                                if (PAY_GETD_SUB22_ROW_ITEMS?.N_KOL?.ToString() == "911") //از نوع حذف شده انتظامی
                                {
                                    if (PAY_GETD_SUB22_ROW_ITEMS?.N_KOL?.ToStringNullSafe() != Baseknow.BANKHA?.ToStringNullSafe())
                                    {
                                        if (rst?.N_KOL != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL = null;
                                        if (rst?.N_MOIN != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN = null;
                                        if (rst?.N_TAF != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF = null;
                                    }

                                    if (rst?.N_KOL2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL2 = null;
                                    if (rst?.N_MOIN2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN2 = null;
                                    if (rst?.N_TAF2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF2 = null;

                                    if (rst?.N_KOL3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL3 = null;
                                    if (rst?.N_MOIN3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN3 = null;
                                    if (rst?.N_TAF3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF3 = null;
                                }
                            }

                        }
                    }
                }
                #endregion
            }
            if (e.Column.SortMemberPath == "DATE_S") //تاریخ سررسید
            {
                //if (CL_HESABDARI.CHEKDATEM((long)PAY_GETD_SUB22_ROW_ITEMS.DATE_S, false) is true) //تاریخ صحیح نیست
                //{
                //    PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                //}
                string date_n_val = PAY_GETD_SUB22_ROW_ITEMS.DATE_S.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                        universControl.PopNotifyShow("تاریخ سررسید صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
                else
                {
                    PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                    universControl.PopNotifyShow("تاریخ سررسید نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            if (e.Column.SortMemberPath == "DATE") //تاريخ دريافت
            {
                string date_n_val = PAY_GETD_SUB22_ROW_ITEMS.DATE.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                        universControl.PopNotifyShow("تاريخ دريافت صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                    else
                    {
                        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                        {
                            PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                            universControl.PopNotifyShow(".تاريخ دريافت به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                            return;
                        }
                    }
                }
                else
                {
                    PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                    universControl.PopNotifyShow("تاريخ دريافت نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                //if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(PAY_GETD_SUB22_ROW_ITEMS.DATE), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                //{
                //    PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                //}
            }
            if (e.Column.SortMemberPath == "SANDUGH")
            {
                //در RowEnd لاگ میزنم
                //rst.Open("dbo.PAY_GETD_LOG", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //rst.AddNew();
                //rst.update();
            }
            if (e.Column.SortMemberPath == "VAZ")
            {
            }
            if (e.Column.SortMemberPath == "SAYADI")
            {
                List<MsgModel> ErrosMessages = new List<MsgModel>();
                var FINAL_CROW_ITEM = PAY_GETD_SUB22_ROW_ITEMS;
                var DG = PAY_GETD_SUB22;

                if (!double.TryParse(FINAL_CROW_ITEM.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_SERI?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.DATE?.ToString(), out int _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.BANK?.ToString(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
                }
                if (string.IsNullOrEmpty(FINAL_CROW_ITEM.BANK?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.DATE_S?.ToString(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.MABL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.MABL?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_KOL?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_MOIN?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_TAF?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.VAZ?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.VAZ?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت چک صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.SANDUGH?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.SANDUGH?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقعیت چک صحیح نیست" });
                }

                if (ErrosMessages.Count > 0)
                {
                    if (!Keyboard.IsKeyDown(Key.Escape))
                    {
                        e.Cancel = true;

                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();

                        return;
                    }
                }
            }
            //DATE - تاريخ دريافت   |   DATE_S - تاريخ سررسيد
        }
        private void PAY_GETD_SUB22_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            #region WORKS
            //var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;
            //if (n_MOINColumn.ItemsSource is null) //MOIN
            //{
            //    if (PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null)
            //    {
            //        //معین بانک
            //        n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETD_SUB22_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //    }
            //}
            //if (n_TAFColumn.ItemsSource is null) //TAFZILY
            //{
            //    if (PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null && PAY_GETD_SUB22_ROW_ITEMS.N_MOIN is not null)
            //    {
            //        //تفضیلی
            //        n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) ={PAY_GETD_SUB22_ROW_ITEMS.N_MOIN}) AND ((TDETA_HES.N_KOL) ={PAY_GETD_SUB22_ROW_ITEMS.N_KOL}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
            //    }
            //}
            #endregion

            var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;

            int DefVale = 0;
            ComboBox THE_COMBO = e.EditingElement as ComboBox;

            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (!(e.EditingElement is null) && PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //معین بانک
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETD_SUB22_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
                    if (DefVale <= 0)
                    {
                        THE_COMBO.SelectedIndex = 0;
                    }
                    else
                    {
                        THE_COMBO.SelectedValue = DefVale;
                    }
                }
            }
            if (e.Column.SortMemberPath == "N_TAF")
            {
                if (!(e.EditingElement is null) && PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null && PAY_GETD_SUB22_ROW_ITEMS.N_MOIN is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //تفضیلی
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) =" + PAY_GETD_SUB22_ROW_ITEMS.N_MOIN + ") AND ((TDETA_HES.N_KOL) =" + PAY_GETD_SUB22_ROW_ITEMS.N_KOL + "))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
                    if (DefVale is 0)
                    {
                        THE_COMBO.SelectedIndex = 0;
                    }
                    else
                    {
                        THE_COMBO.SelectedValue = DefVale;
                    }
                }
            }

        }
        private void PAY_GETD_SUB22_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (PAY_GETD_SUB22.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = PAY_GETD_SUB22.CurrentCell.Column.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_CHKPOSHT_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "MABL")
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
                if (CURRENT_COLUMN_NAME is "MABL")
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
        private void PAY_GETD_SUB22_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var FINAL_CROW_ITEM = (e.Row.Item as PAY_GETD_SUB22_MODEL);

            //Validations:
            #region Validations
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (string.IsNullOrEmpty(FINAL_CROW_ITEM.N_HESAB))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب وارد نشده !" });
            }
            //string nHesabPattern = @"^\d{10}$";
            //if (FINAL_CROW_ITEM?.N_HESAB is not null)
            //{
            //    if (!Regex.IsMatch(FINAL_CROW_ITEM?.N_HESAB, nHesabPattern)) ErrosMessages.Add(new MsgModel { MessageText_U = "فرمت حساب وارد شده صحیح نیست !" });
            //}
            if (!double.TryParse(FINAL_CROW_ITEM.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_SERI?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.DATE?.ToString(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.BANK?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
            }
            if (string.IsNullOrEmpty(FINAL_CROW_ITEM.BANK?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.DATE_S?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.MABL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.MABL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_KOL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_MOIN?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_TAF?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.VAZ?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.VAZ?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت چک صحیح نیست" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.SANDUGH?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.SANDUGH?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقعیت چک صحیح نیست" });
            }


            var DG = PAY_GETD_SUB22;
            var hasError = false;
            var erg = e.Row.GetIndex();

            DataGridRow row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            if (row == null)
            {
                DG.UpdateLayout();
                DG.ScrollIntoView(DG.Items[erg]);
                row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            }
            if (row != null && Validation.GetHasError(row))
            {
                hasError = true;
            }
            hasError = (from object i in DG.ItemsSource
                        let c = row
                        where c != null && Validation.GetHasError(c)
                        select c).Any();
            if (ErrosMessages.Count > 0 || hasError)
            {
                DG.Dispatcher.InvokeAsync(() =>
                {
                    DG.CellEditEnding -= PAY_GETD_SUB22_CellEditEnding;
                    DG.CancelEdit();
                    DG.CellEditEnding += PAY_GETD_SUB22_CellEditEnding;
                });
                return;
            }
            #endregion

            #region Form_BeforeInsert
            var rst = dbms.DoGetDataSQL<string?>("SELECT TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.TNUMBER) = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + " ) And ((TDETA_HES.NUMBER) = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") And ((TDETA_HES.N_KOL) = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + " )) GROUP BY TDETA_HES.NAME").ToList();
            if (rst.Count > 0)
            {
                FINAL_CROW_ITEM.NAME_TAH = rst.FirstOrDefault();
            }
            #endregion

            #region Form_BeforeUpdate
            long dfn;
            long rdn;
            if (FINAL_CROW_ITEM?.RADIF is null)
            {
                var RST2 = dbms.DoGetDataSQL<DAFT_ASN>("SELECT     TOP 100 PERCENT FIRSTNUM, BOOKNUM FROM dbo.DAFT_ASN ORDER BY BOOKNUM DESC").ToList();
                if (RST2.Count > 0)
                {
                    rdn = (long)RST2.FirstOrDefault().FIRSTNUM;
                    dfn = (long)RST2.FirstOrDefault().BOOKNUM;
                }
                else
                {
                    new Msgwin(false, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع :1 شماره دفتر :1").ShowDialog();
                    //RST2.Open("DAFT_ASN", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                    //RST2.AddNew();
                    //RST2.Fields(0) = 1;
                    //RST2.Fields(1) = 1;
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DAFT_ASN(FIRSTNUM, BOOKNUM)
                                         VALUES({RST2.FirstOrDefault().FIRSTNUM},
                                         {RST2.FirstOrDefault().BOOKNUM})");
                    //RST2.update();
                    rdn = 1L;
                    dfn = 1L;
                }

                var rst_1 = dbms.DoGetDataSQL<double?>("SELECT Max(PAY_GETD.RADIF) AS MaxOfRADIF  FROM PAY_GETD WHERE ANBAR = " + dfn).ToList();
                if (rst_1.Count == 0 || rst_1.FirstOrDefault() is null)
                {
                    FINAL_CROW_ITEM.RADIF = rdn;
                    FINAL_CROW_ITEM.ANBAR = dfn;
                }
                else
                {
                    FINAL_CROW_ITEM.RADIF = rst_1.FirstOrDefault() + 1;
                    FINAL_CROW_ITEM.ANBAR = dfn;
                }
                // DoCmd.OpenForm "MESAGEFORM", , , , , acDialog, "شماره دفتر :" & Me.RADIF
            }
            #endregion

            //SANDUGH_AfterUpdate , VAZ_AfterUpdate {
            var N_SERI = FINAL_CROW_ITEM.N_SERI;
            var BANK = FINAL_CROW_ITEM.BANK;
            var DATE_S = FINAL_CROW_ITEM.DATE_S;
            var DATE_V = CL_HESABDARI.FARSIDATE();
            var DATETIM = DateTime.Now;
            var VAZ = FINAL_CROW_ITEM.VAZ;
            var SANDUGH = FINAL_CROW_ITEM.SANDUGH;
            var USER_NAME = CL_HESABDARI.UCurrentUser();

            dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                     VALUES({N_SERI},
                                     {BANK}   ,
                                     {DATE_S}   ,
                                     {DATE_V}   ,
                                     GETDATE(),
                                     {VAZ} ,
                                     {SANDUGH}   ,
                                     N'{USER_NAME}'
                                     )");
            //SANDUGH_AfterUpdate , VAZ_AfterUpdate }

            dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                     VALUES({N_SERI},
                                     {BANK}   ,
                                     {DATE_S}   ,
                                     {DATE_V}   ,
                                     GETDATE(),
                                     {VAZ} ,
                                     {SANDUGH}   ,
                                     N'{USER_NAME}'
                                     )");


            //CUST_NO : 
            FINAL_CROW_ITEM.CUST_NO = CUST_NO.SelectedValue.ToString();

            //Final Saving ...
            if (FINAL_CROW_ITEM.ID is not null && FINAL_CROW_ITEM?.ID > 0) //Update
            {
                var PayGetD_VAZ = dbms.DoGetDataSQL<double?>($"SELECT TOP(1) VAZ FROM dbo.PAY_GETD WHERE ID = {FINAL_CROW_ITEM.ID}").FirstOrDefault();
                if (PayGetD_VAZ is not null)
                {
                    if (PayGetD_VAZ != VAZ)
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                                      VALUES({N_SERI},
                                                      {BANK}   ,
                                                      {DATE_S}   ,
                                                      {DATE_V}   ,
                                                      GETDATE(),
                                                      {VAZ} ,
                                                      {SANDUGH}   ,
                                                      N'{USER_NAME}'
                                                      )");
                    }
                }

                dbms.DoExecuteSQL($@"UPDATE PAY_GETD
                            SET N_SERI = {N_SERI}, BANK = {BANK}, DATE_S = {DATE_S}, DATE = {FINAL_CROW_ITEM.DATE},
                            SHOBEH = N'{FINAL_CROW_ITEM.SHOBEH}', MABL = {FINAL_CROW_ITEM.MABL}, NAME_TAH = N'{FINAL_CROW_ITEM.NAME_TAH}', 
                            N_HESAB = N'{FINAL_CROW_ITEM.N_HESAB}', N_KOL = {(FINAL_CROW_ITEM.N_KOL is null ? "NULL" : FINAL_CROW_ITEM.N_KOL)}, 
                            N_MOIN = {(FINAL_CROW_ITEM.N_MOIN is null ? "NULL" : FINAL_CROW_ITEM.N_MOIN)},
                            N_TAF = {(FINAL_CROW_ITEM.N_TAF is null ? "NULL" : FINAL_CROW_ITEM.N_TAF)}, NUMBER = {NUMBER.Text}, 
                            TAG = {FTAG}, ANBAR = 1, VAZ = {FINAL_CROW_ITEM.VAZ}, KIND = {FINAL_CROW_ITEM.KIND},
                            SANDUGH = {FINAL_CROW_ITEM.SANDUGH}, SAYADI = N'{FINAL_CROW_ITEM.SAYADI}'
                            WHERE ID = {FINAL_CROW_ITEM.ID}");
            }
            else //Insert
            {
                string dbtest = $@"INSERT INTO PAY_GETD (N_SERI,                   BANK,                   DATE_S,                   DATE,                    SHOBEH,                     MABL,                    NAME_TAH,                      N_HESAB,                    N_KOL,                   N_MOIN,                    N_TAF,  NUMBER,        TAG, ANBAR,                                                                   RADIF,                                             VAZ,                    KIND,               SANDUGH,                        SAYADI) 
                                                                OUTPUT INSERTED.ID
                                                                VALUES ({FINAL_CROW_ITEM.N_SERI}, {FINAL_CROW_ITEM.BANK}, {FINAL_CROW_ITEM.DATE_S}, {FINAL_CROW_ITEM.DATE}, N'{FINAL_CROW_ITEM.SHOBEH}', {FINAL_CROW_ITEM.MABL}, N'{FINAL_CROW_ITEM.NAME_TAH}', N'{FINAL_CROW_ITEM.N_HESAB}', {FINAL_CROW_ITEM.N_KOL}, {FINAL_CROW_ITEM.N_MOIN}, {FINAL_CROW_ITEM.N_TAF}, {NUMBER.Text}, {FTAG}, 1,    (SELECT TOP(1) RADIF+1 FROM dbo.PAY_GETD WHERE N_SERI = {N_SERI} AND BANK = {BANK} AND DATE_S = {DATE_S} AND NUMBER = {NUMBER.Text} AND TAG = {FTAG}), {FINAL_CROW_ITEM.VAZ}, {FINAL_CROW_ITEM.KIND}, {FINAL_CROW_ITEM.SANDUGH}, N'{FINAL_CROW_ITEM.SAYADI}')";

                var GOTID = dbms.DoGetDataSQL<long?>($@"INSERT INTO PAY_GETD (N_SERI,                   BANK,                   DATE_S,                   DATE,                    SHOBEH,                     MABL,                    NAME_TAH,                      N_HESAB,                    N_KOL,                                                         N_MOIN,                    N_TAF,  NUMBER,        TAG, ANBAR,           RADIF,                                             VAZ,                    KIND,               SANDUGH,                        SAYADI) 
                                                                OUTPUT INSERTED.ID
                                                                VALUES ({FINAL_CROW_ITEM.N_SERI}, {FINAL_CROW_ITEM.BANK}, {FINAL_CROW_ITEM.DATE_S}, {FINAL_CROW_ITEM.DATE}, N'{FINAL_CROW_ITEM.SHOBEH}', {FINAL_CROW_ITEM.MABL}, N'{FINAL_CROW_ITEM.NAME_TAH}', N'{FINAL_CROW_ITEM.N_HESAB}', {(FINAL_CROW_ITEM.N_KOL is null ? "NULL" : FINAL_CROW_ITEM.N_KOL)}, {(FINAL_CROW_ITEM.N_MOIN is null ? "NULL" : FINAL_CROW_ITEM.N_MOIN)}, {(FINAL_CROW_ITEM.N_TAF is null ? "NULL" : FINAL_CROW_ITEM.N_TAF)}, {NUMBER.Text}, {FTAG}, 1, {FINAL_CROW_ITEM.RADIF}   , {FINAL_CROW_ITEM.VAZ}, {FINAL_CROW_ITEM.KIND}, {FINAL_CROW_ITEM.SANDUGH}, N'{FINAL_CROW_ITEM.SAYADI}')").FirstOrDefault();
                FINAL_CROW_ITEM.ID = GOTID;
            }

            SANAD();
        }

        private void PAY_GETD_SUB22_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PAY_GETD_SUB22.IsKeyboardFocusWithin) { return; }

            IEditableCollectionView itemsView = PAY_GETD_SUB22.Items as IEditableCollectionView;
            if (itemsView.IsAddingNew || itemsView.IsEditingItem)
            {
                // Retrieve the new item/edited item
                //object NewRecordFresh = itemsView.IsAddingNew ? itemsView.CurrentAddItem : itemsView.CurrentEditItem;
                if (itemsView.IsAddingNew)
                {
                    itemsView.CommitNew();
                }
                else if (itemsView.IsEditingItem)
                {
                    itemsView.CommitEdit();
                }
            }
        }

        private void DELETE_CHKPOSHT_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_CHKPOSHT.Visibility == Visibility.Visible;
            if (!DELETE_CHKPOSHT.IsEnabled || !IsVisible) { return; }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            //if (PAY_GETD_SUB22.IsEditing()) return;

            if (PAY_GETD_SUB22.Items.Count > 0 && PAY_GETD_SUB22.SelectedItem != null)
            {
                if (!(PAY_GETD_SUB22.SelectedItems is null))
                {
                    bool errors = default;
                    errors = (from object i in PAY_GETD_SUB22.ItemsSource
                              let c = PAY_GETD_SUB22.ItemContainerGenerator.ContainerFromItem(i)
                              where c != null && Validation.GetHasError(c)
                              select c).Any();

                    if (errors)
                    {
                        universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        ESLAH_Click(null, null);

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور برگشت خرید - عادی => چک های دریافتی پشت فاکتور",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 3",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < PAY_GETD_SUB22.SelectedItems.Count; i++)
                        {
                            var item = PAY_GETD_SUB22.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                {
                                    PAY_GETD_SUB22_DATA.Remove(item as PAY_GETD_SUB22_MODEL);

                                    //var before = PAY_GETD_SUB22.CanUserAddRows;
                                    PAY_GETD_SUB22.CanUserAddRows = false;
                                    PAY_GETD_SUB22.CanUserAddRows = true;
                                }
                                else
                                {
                                    var THE_N_SERI = item.GetType().GetProperty("N_SERI").GetValue(item);
                                    var THE_BANK = item.GetType().GetProperty("BANK").GetValue(item);

                                    var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE  N_SERI=" + THE_N_SERI + " AND BANK = " + THE_BANK + " AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                                    if (rst.Count > 0)
                                    {
                                        if ((!IsNull(rst.FirstOrDefault().N_KOL2) && rst.FirstOrDefault().N_KOL2 != 911) || !IsNull(rst.FirstOrDefault().N_KOL3))
                                        {
                                            Msgwin msgwin1 = new Msgwin(false, "چكي كه وصولي يا واگذاري يا برگشتي خورده قابل حذف نيست");
                                            msgwin1.ShowDialog();
                                        }
                                        else
                                        {
                                            if ((rst.FirstOrDefault().N_KOL == Baseknow.BANKHA || rst.FirstOrDefault().N_KOL == 911) || IsNull(rst.FirstOrDefault().N_KOL))
                                            {
                                                string _where = " WHERE  N_SERI=" + THE_N_SERI + " AND BANK = " + THE_BANK;

                                                rst.FirstOrDefault().N_KOL = 911;
                                                rst.FirstOrDefault().N_MOIN = 1;
                                                rst.FirstOrDefault().N_TAF = 1;
                                                rst.FirstOrDefault().HES1 = "911-1-1";

                                                dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL = 911 , N_MOIN = 1 , N_TAF = 1 , HES1 = N'911-1-1' {_where} ");
                                                IsDeleteSomthing = true;
                                            }

                                        }
                                    }
                                    CL_HESABDARI.GETDLOG(1, THE_N_SERI.ToString(), (int)THE_BANK, rst.FirstOrDefault().DATE_S, (int)rst.FirstOrDefault().SANDUGH);
                                }
                            }
                            else
                            {
                                universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
                                return;
                            }
                        }
                        if (IsDeleteSomthing is true)
                        {
                            PAY_GETD_SUB_ReGetData();

                            SANAD();
                        }
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
            }
        }

        #endregion

        private void MABL_VAR_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_VAR_AfterUpdate
            if (IsNull(this.MOIN_VAR.Text) && MABL_VAR.Text != "0")
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT Min(DETA_HES.NUMBER) AS MinOfNUMBER FROM DETA_HES WHERE (((DETA_HES.N_KOL)= " + Baseknow.BANKHA + "))").FirstOrDefault();
                if (RST != null)
                {
                    if (IsNull(MOIN_VAR.Text))
                    {
                        MOIN_VAR.Text = Baseknow.BANKHA + "-1-1";
                    }
                }
                else
                {
                    universControl.PopNotifyShow("حساب معين براي بانك تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل بانكها معين تعريف نمائيد.", Pop1, Pop1Text1, Pop_Border1);
                }
            }
        }
        private void MABL_HAV_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_HAV_AfterUpdate
            if (IsNull(this.MOIN_HAV.Text) && MABL_HAV.Text != "0")
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT Min(DETA_HES.NUMBER) AS MinOfNUMBER FROM DETA_HES WHERE (((DETA_HES.N_KOL)= " + Baseknow.HAVALAH + "))").FirstOrDefault();
                if (RST != null)
                {
                    if (IsNull(MOIN_HAV.Text))
                    {
                        MOIN_HAV.Text = Baseknow.BANKHA + "-1-1";
                    }
                }
                else
                {
                    universControl.PopNotifyShow("حساب معين براي خدمات تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل بانكها معين تعريف نمائيد.", Pop1, Pop1Text1, Pop_Border1);
                }
            }
        }

        private void BTN_NEW_FACTOR_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeIsHappend)
            {
                ClearFreshNew();

                AllowEdits = true;
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
        private void INVO_LST_sub_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

    }
}
