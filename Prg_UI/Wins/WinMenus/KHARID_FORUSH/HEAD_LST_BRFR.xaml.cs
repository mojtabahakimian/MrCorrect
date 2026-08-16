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
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Base;
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
using Prg_UI.Wins.WinMenus.HESABDARI;
using System.ComponentModel;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using Microsoft.IdentityModel.Tokens;
using Wins.WinOther;
using static Interfaces.INavigator;
using Wins.WinMenus.ANBAR;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using static Prg_UI.Functions.CL_LMethods;
using System.Windows.Threading;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class HEAD_LST_BRFR : Window, ISearchableWindow, IComboLookupProvider
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
        public IEnumerable<ComboLookupSpec> GetComboLookups()
        {
            yield return new ComboLookupSpec { DisplayName = "فروشنده", KeyPropertyPath = "CUST_NO", Combo = CUST_NO };
        }
        public HEAD_LST_BRFR(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER.Text = number_to_open.ToString(); //شماره رسید
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله


        private NavigationManager<HEAD_LST> _navigationManager;

        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<PAY_GETP_MODEL> PAY_GETP_SUB_DATA { get; set; } = new ObservableCollection<PAY_GETP_MODEL>();
        public ObservableCollection<VISITOR_DTL> SAYER_VISITOR_DATA { get; set; } = new ObservableCollection<VISITOR_DTL>();

        /// <summary>
        /// 24
        /// </summary>
        public byte HTAG { get; } = 24; //Row : INVO_LST ---- PAY_GETP ---- VISITOR_DTL : TAG = 24


        /// <summary>
        /// 25
        /// </summary>
        public byte FTAG { get; } = 25; //Header Master : TAG = 25 


        public int? ANBAR { get; set; }


        private double _sum_of_mabl_k = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _sum_of_mabl_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MABL_K ?? 0);
                if (_sum_of_mabl_k == 0) _sum_of_mabl_k = 0;
                return _sum_of_mabl_k;
            }
            set { _sum_of_mabl_k = value; }
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
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "FFRB_FROOSHTX");
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
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "FFRB_ANBTX");
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
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "FFRB_HESABTX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        public bool NowIsReady { get; private set; }

        //private bool _newrecord;
        //public bool NewRecord
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
        //        {
        //            _newrecord = true;
        //        }
        //        else
        //        {
        //            _newrecord = false;
        //        }
        //        return _newrecord;
        //    }
        //    set { _newrecord = value; }
        //}

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
                    int? defaultcolumnindex = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "MABL")?.DisplayIndex;
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
                ////NUMBER.IsReadOnly = !ican;// شماره حواله
                CUST_KIND.IsReadOnly = !ican;// نوع مشتری
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ
                SHIFT.IsReadOnly = !ican;// شیفت

                //__ENABLEY
                DEPATMAN.IsEnabled = ican;
                TICMBAA.IsEnabled = ican;

                DATE_N.IsEnabled = ican;// تاریخ
                NUMBER.IsEnabled = ican;// شماره رسید
                CUST_KIND.IsEnabled = ican;// نوع مشتری
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ
                SHIFT.IsEnabled = ican;// شیفت
                //فاکتور END
                //Page58.IsEnabled = ican;// تب پشت فاکتور

                CMB_MOIN_HAZ.IsEnabled = ican; //معین خدمت
                CMB_HMBAA.IsEnabled = ican;
                M_NAGHD.IsReadOnly = !ican; //مبلغ نقد
                HMBAA.IsReadOnly = !ican; //مبلغ نقد

                //TAKHFIF.IsReadOnly = !ican; //مبلغ تخفیف
                //TAKHFIF_PERCENT.IsReadOnly = !ican; //درصد تخفیف

                MABL_HAZ.IsReadOnly = !ican; //خدمات
                MOIN_HAZ.IsReadOnly = !ican; //معین خدمت
                PAY_GETP_SUB.IsReadOnly = !ican; //چک ها
                INVO_LST_SUB.IsReadOnly = !ican;

                // --- New Fields ---
                MODAT_PPID.IsEnabled = ican;
                PEPID.IsEnabled = ican;
                PEID.IsEnabled = ican;
                // Special logic for GHAYM=7
                if (CL_Generaly.IsGHAYM_7)
                {
                    if (!CL_HESABDARI.LETSGO("elamghe"))
                    {
                        this.PEPID.IsEnabled = false;
                        this.PEID.IsEnabled = false;
                    }
                }

                bool AllowedToSavePursantVisitor = CL_HESABDARI.LETSGO("VISITORS"); //ثبت پورسانت ویزیتور
                if (AllowedToSavePursantVisitor)
                {
                    VISITOR_DTL_SUB.IsReadOnly = !ican;
                }
                else
                {
                    VISITOR_DTL_SUB.IsReadOnly = true;
                }

                BTN_SAVE.IsEnabled = ican;

            }
        }

        public double Meidnum { get; private set; }
        public double? NUMBER1_TAG { get; private set; } = null;
        public int ANBARDefaultValue { get; private set; }
        public nint WINDOW_ID { get; private set; }
        public Visual I_AM_BARGASHT_NORMAL { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            WINDOW_ID = new WindowInteropHelper(this).Handle;
            I_AM_BARGASHT_NORMAL = CL_LMethods.GetTheWindow(WINDOW_ID);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            string WhereCondition = FTAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) " : "  ";
            _restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(FTAG, WhereCondition);
            WhereCondition = _restrictionInfo.WhereClause;

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

            if (!IsOpenedFromAutomation && !string.IsNullOrEmpty(NUMBER1_TAG?.ToStringNullSafe()) && _navigationManager.NUMBER_TO_OPEN != null) //Had a paramter passed
            {
                //یعنی این شماره رو پیدا نکرده که اون رو ریست کنه
                new Msgwin(false, $"شما به این شماره {_navigationManager.NUMBER_TO_OPEN} دسترسی ندارید ").Show();
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

            //NUMBER.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var originalSource = e.OriginalSource as DependencyObject;
                DataGrid activeGrid = FindParent<DataGrid>(originalSource);

                if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    // Smart Detection: Are we inside a DataGrid?
                    if (activeGrid != null)
                    {
                        // Logic specific to DataGrid Navigation
                        if (activeGrid.CurrentColumn != null)
                        {
                            int currentColumnIndex = activeGrid.CurrentColumn.DisplayIndex;
                            // Check actual visible columns count, not just count
                            bool isLastColumn = currentColumnIndex >= activeGrid.Columns.Count - 1;
                            bool isLastRow = activeGrid.SelectedIndex == activeGrid.Items.Count - 2; // -2 assumes "CanUserAddRows" is true creating a placeholder

                            if (isLastColumn)
                            {
                                if (isLastRow)
                                {
                                    // Move to new row
                                    activeGrid.SelectedIndex++; // Move to the placeholder row

                                    // Determine which column to focus based on Grid Name (Legacy Logic Preservation)
                                    int targetColIndex = 0;
                                    if (activeGrid.Name == "VISITOR_DTL_SUB")
                                    {
                                        targetColIndex = 0;
                                    }
                                    else if (activeGrid.Name == "INVO_LST_SUB")
                                    {
                                        // Ensure the constant INVO_LST_SUB_DEF_INDEX_COL exists in your class
                                        targetColIndex = INVO_LST_SUB_DEF_INDEX_COL;
                                    }

                                    // Safety check for column index
                                    if (targetColIndex < activeGrid.Columns.Count)
                                    {
                                        activeGrid.CurrentCell = new DataGridCellInfo(activeGrid.SelectedItem, activeGrid.Columns[targetColIndex]);
                                    }

                                    // Enter Edit Mode on the new cell
                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        activeGrid.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; // Stop here, do not tab
                                }
                            }
                        }
                    }

                    CL_LMethods.SendKey_US(Key.Tab);
                }

                // Only trigger if NOT editing inside the main grid (prevent interrupting data entry)
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    // Check if we are "busy" editing inside the specific grid
                    bool isEditingMainGrid = (activeGrid != null && activeGrid.Name == "INVO_LST_SUB" && activeGrid.IsKeyboardFocusWithin);

                    if (!isEditingMainGrid)
                    {
                        e.Handled = true;
                        var searchWindow = new EnhancedSearchWindow(this);
                        searchWindow.Owner = this;
                        searchWindow.ShowDialog();
                    }
                }

                // --- DIRTY FLAG (CHANGE TRACKING) LOGIC ---
                var nonDataKeys = new[]
                {
                    Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                    Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                    Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                    Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                    Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                    Key.Escape, Key.Insert, Key.Home, Key.End,
                    Key.PageUp, Key.PageDown, Key.System // Alt key combinations
                };

                if (!nonDataKeys.Contains(e.Key))
                {
                    var focused = Keyboard.FocusedElement as DependencyObject;

                    if (focused != null)
                    {
                        if (CL_LMethods.IsInside<TextBoxBase>(focused) ||
                            CL_LMethods.IsInside<ComboBox>(focused) ||
                            CL_LMethods.IsInside<CheckBox>(focused) ||
                            focused is Xceed.Wpf.Toolkit.MaskedTextBox) // Direct check for Xceed
                        {
                            ChangeIsHappend = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
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
                    Command108.IsEnabled = true;
                }
                else
                {
                    Command100.IsEnabled = false;
                    Command108.IsEnabled = false;
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
                        Page58.IsEnabled = false;
                        Page155.IsEnabled = false;
                        //lsanad.Foreground = Brushes.Red;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        INVO_LST_SUB.IsReadOnly = false;
                        Page58.IsEnabled = true;
                        Page155.IsEnabled = true;
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

            if (_navigationManager.IsNewRecord)
            {
                Page58.IsEnabled = false;
                Page155.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                if (!ghat)
                {
                    INVO_LST_SUB.IsReadOnly = false;
                    Page58.IsEnabled = true;
                    Page155.IsEnabled = true;
                }
                else
                {
                    //Page58.IsEnabled = false;
                    //Page155.IsEnabled = false;
                    INVO_LST_SUB.IsReadOnly = true;
                }
            }

            SecurityAllCheck();

            //if ((bool)Baseknow.SIGN)
            //{
            //    var signResult = dbms.DoGetDataSQL<SignData>($"SELECT FFRB_FROOSH , FFRB_ANB , FFRB_HESAB FROM dbo.SIGN WHERE USERCO = {Baseknow.USERCOD}").FirstOrDefault();
            //    if (signResult != null)
            //    {
            //        _ = (signResult.FFRB_FROOSHTX ? (SGN1.Visibility = Visibility.Visible) : (SGN1.Visibility = Visibility.Hidden));
            //        _ = (signResult.FFRB_ANBTX ? (SGN2.Visibility = Visibility.Visible) : (SGN2.Visibility = Visibility.Hidden));
            //        _ = (signResult.FFRB_HESABTX ? (SGN3.Visibility = Visibility.Visible) : (SGN3.Visibility = Visibility.Hidden));
            //    }
            //}

            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                this.OKF.IsChecked = true;
            }
            else
            {
                this.OKF.IsChecked = false;
            }

            if (OKF.IsChecked != null && OKF.IsChecked == true && !_navigationManager.IsNewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_SUB.IsReadOnly = true;
                //Page58.IsEnabled = false;
                //Page155.IsEnabled = false;
                ESLAH.IsEnabled = true;

                NUMBER1.IsEnabled = false;
            }

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 4, Convert.ToInt32(Baseknow.USERCOD), WINDOW_ID); //مثل برگشت فروش عادی معمولی نرمال استاندارد
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            if (!CL_HESABDARI.LETSGO("elamghe"))
            {
                this.PEPID.IsEnabled = false;
                this.PEID.IsEnabled = false;
            }
            else
            {
                this.PEPID.IsEnabled = true;
                this.PEID.IsEnabled = true;
            }

            if (Baseknow.GHAYM == 7)
            {
                MODAT_PPID.Visibility = Visibility.Visible;
                PEPID.Visibility = Visibility.Visible;
                PEID.Visibility = Visibility.Visible;
            }
            else
            {
                MODAT_PPID.Visibility = Visibility.Hidden;
                PEPID.Visibility = Visibility.Hidden;
                PEID.Visibility = Visibility.Hidden;
            }

            AllowEdits = false;
        }

        private CL_LMethods.RestrictionInfo _restrictionInfo;
        private string GetAccessDeniedMessage()
        {
            if (_restrictionInfo?.RestrictionMessages?.Any() == true)
            {
                // ایجاد لیست محدودیت‌ها
                var restrictions = string.Join("، ", _restrictionInfo.RestrictionMessages);

                return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                    $"به آخرین شماره مجاز هدایت خواهید شد. (محدودیت: {restrictions})";
            }

            return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                $"شما به آخرین شماره مجاز هدایت خواهید شد.";
        }
        private void ShowMissingOrRestrictedMessage()
        {
            if (_navigationManager?.NUMBER_TO_OPEN == null)
            {
                return;
            }

            double requestedNumber = Convert.ToDouble(_navigationManager.NUMBER_TO_OPEN);
            bool recordExists = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 NUMBER FROM HEAD_LST WHERE NUMBER = {requestedNumber} AND TAG = {FTAG}").FirstOrDefault() != null;
            string message = recordExists ? GetAccessDeniedMessage() : "چنین شماره ای وجود ندارد";
            new Msgwin(false, message).ShowDialog();
            _navigationManager.ClearNumberToOpen();
        }
        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshNew(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    ShowMissingOrRestrictedMessage();
                    return;
                }
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    //new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    ShowMissingOrRestrictedMessage();
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

                NUMBER1.Text = HEADER_FAC.NUMBER1.ToString();
                NUMBER.Text = HEADER_FAC.NUMBER.ToString();

                if (!string.IsNullOrEmpty(NUMBER1.Text.ToStringNullSafe()))
                {
                    if (Convert.ToDouble(NUMBER1.Text) > 0)
                    {
                        ReGetDataMaster(false);

                        ReGetDataAll();

                        Summer();

                        GetBalancePerson();

                        GetUpdateTakhfif();
                        //TAKHFIF_MABL_PRICE();

                        ActivateChaps();

                        AllowEdits = false;
                        BTN_SAVE.IsEnabled = false;
                        INVO_LST_SUB.IsReadOnly = true;
                        BTN_DELETE.IsEnabled = false;
                    }
                }
                //NewRecord = false; //Currrent Record is not new              

                Form_Current();

                ChangeIsHappend = false;
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

        private void RefreshReceiptNumbers()
        {
            //شماره رسید سایر انبار 
            NUMBER.ItemsSource = dbms.DoGetDataSQL<QRE_LST_BARGASHT>($"SELECT NUMBER FROM HEAD_LST WHERE (TAG = {HTAG /*24*/}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG /*25*/}))))) ORDER BY NUMBER").ToList();
        }
        public void ClearFreshNew()
        {
            RefreshReceiptNumbers();

            NUMBER.SelectedIndex = -1; NUMBER.Items.Refresh(); //شماره فاکتور

            NUMBER.Text = "0"; //شماره حواله
            NUMBER.Tag = null;
            NUMBER1.Tag = null;
            NUMBER1.Text = "0";
            NUMBER1_TAG = null;

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();
            MOLAH.Text = null;
            FNUMCO.Text = null;

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد

            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 
            MAS.Text = "0"; //

            OKF.IsChecked = false; //تایید فاکتور

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            MOGU.Text = null; //موجودی

            TEDADM.Text = "0"; //جمع مقادیر
            TAKHFIF_PERCENT.Text = "0"; //درصد تخفیف
            JJKOL.Text = "0"; //جمع فاکتور

            MANDAH.Text = null;
            N_S.Text = "0"; //ثبت در سند
            MABNA.Text = "0"; //ثبت در سند

            //پشت فاکتور

            M_NAGHD.Text = "0"; //مبلغ نقد

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

            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
            MODAT_PPID.SelectedIndex = -1; MODAT_PPID.Items.Refresh();
            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

            PEPID.SelectedIndex = -1;
            PEID.SelectedIndex = -1;

            MODAT_PPID_Enter(); // Logic to refresh payment methods

            INVO_LST_FACTOR22_DATA.Clear();
            PAY_GETP_SUB_DATA?.Clear(); //چک
            SAYER_VISITOR_DATA?.Clear();

            AllowEdits = true;

            GetDefaultFocus();

            INVO_LST_SUB.IsReadOnly = true;
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
                new SearchableProperty { DisplayName = "شماره فاکتور برگشت فروش", PropertyPath = "NUMBER1", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "شماره (سایر) رسید انبار", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion


        /// <summary>
        /// بروزرسانی پورسانت ویزیتور بعد از تغییر مبلغ کالا
        /// فقط برای آیتم‌هایی که تیک "مبلغ ثابت" ندارند
        /// </summary>
        private void UpdateVisitorCommissions()
        {
            if (SAYER_VISITOR_DATA == null || SAYER_VISITOR_DATA.Count == 0)
                return;

            // اگر JF یا TAKHFIF خالی یا نامعتبر باشند، از محاسبه خارج شویم
            if (!double.TryParse(JF.Text, out double jfValue) || !double.TryParse(TAKHFIF.Text, out double takhfifValue))
                return;

            // برای هر ویزیتور که مبلغش ثابت نیست، پورسانت را بروز کنیم
            foreach (var visitor in SAYER_VISITOR_DATA)
            {
                // فقط آیتم‌هایی که STAT = false (مبلغ ثابت نیست)
                if (visitor.STAT == false && visitor.DARSAD.HasValue && visitor.ID.HasValue)
                {
                    // فرمول: PURSANT = (JF - TAKHFIF) * DARSAD / 100
                    visitor.PURSANT = Math.Round((jfValue - takhfifValue) * visitor.DARSAD.Value / 100);

                    try
                    {
                        dbms.DoExecuteSQL(@"UPDATE dbo.VISITOR_DTL SET 
                            DARSAD = @DARSAD,
                            PURSANT = @PURSANT,
                            TOZIH = @TOZIH,
                            STAT = @STAT,
                            PORID = @PORID
                            WHERE ID = @ID", new
                        {
                            DARSAD = visitor.DARSAD,
                            PURSANT = visitor.PURSANT,
                            TOZIH = visitor.TOZIH,
                            STAT = visitor.STAT ?? false,
                            PORID = visitor.PORID,
                            ID = visitor.ID
                        });
                    }
                    catch (SqlException ex) when (ex.Number == 2627)
                    {
                        new Msgwin(false, "ویزیتور تکراری است. ردیف‌های پورسانت را بررسی کنید.").ShowDialog();
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در عملیات پورسانت.").ShowDialog();
                    }
                }
            }

            double sum = SAYER_VISITOR_DATA.Sum(item => item.PURSANT ?? 0.0);
            Text190.Text = sum.ToString();
        }
        private void GetDefaultFocus()
        {
            NUMBER1.Focus();
        }

        private void ReGetDataMaster(bool IsNumberSelectedNow)
        {
            //DATE_N_AfterUpdate
            if (!IsNumberSelectedNow) //Is Not IsNumberSelectedNow
            {
                if (string.IsNullOrWhiteSpace(NUMBER.Text))
                {
                    return;
                }

                if (!double.TryParse(NUMBER.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var numberValue))
                {
                    return;
                }

                var HEADER = dbms.DoGetDataSQL<HEAD_LST>(
                    "SELECT * FROM HEAD_LST WHERE NUMBER = @Number AND TAG = @Tag",
                    new { Number = numberValue, Tag = FTAG })
                    .FirstOrDefault();

                if (HEADER == null)
                {
                    new Msgwin(false, "شماره مورد نظر یافت نشد !").Show();
                    return;
                }

                //فاکتور : NUMBER1
                //رسید : NUMBER
                if (!((List<QRE_LST_BARGASHT>)NUMBER.ItemsSource).Any(item => item?.NUMBER == HEADER?.NUMBER))
                {
                    ((List<QRE_LST_BARGASHT>)NUMBER.ItemsSource).Add(new QRE_LST_BARGASHT { NUMBER = HEADER.NUMBER });
                }
                NUMBER.SelectedValue = HEADER.NUMBER; NUMBER.Items.Refresh();
                NUMBER.Text = HEADER.NUMBER.ToString();

                if (HEADER?.TICMBAA != null)
                {
                    TICMBAA.IsChecked = Convert.ToBoolean(HEADER.TICMBAA);
                }
                if (HEADER?.MAS != null)
                {
                    MAS.Text = HEADER.MAS.ToString();
                }
                if (HEADER?.M_NAGHD != null)
                {
                    M_NAGHD.Text = HEADER.M_NAGHD.ToStringNullSafe(); //مبلغ نقد
                }
                if (HEADER?.TAKHFIF != null)
                {
                    TAKHFIF.Text = HEADER.TAKHFIF.ToStringNullSafe(); //مبلغ تخفیف
                }

                // --- Map New Fields ---
                MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                MODAT_PPID.SelectedValue = HEADER.MODAT_PPID;
                MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

                PEPID.SelectedValue = HEADER.PEPID;
                PEID.SelectedValue = HEADER.PEID;

                MODAT_PPID_Enter(); // Refresh list based on selections

                //پشت فاکتور
                MABL_HAZ.Text = (string.IsNullOrEmpty(HEADER.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER.MABL_HAZ.ToStringNullSafe()); //مبلغ خدمات
                MOIN_HAZ.Text = HEADER.MOIN_HAZ; //معین خدمات
                MBAA.Text = HEADER?.MBAA.ToStringNullSafe(); //مالیات و عوارض مبلغ
                HMBAA.Text = HEADER?.HMBAA; //معین مالیات

                //NUMBER.Text = HEADER.NUMBER.ToStringNullSafe();

                //نوع مشتری
                CUST_KIND.SelectedValue = HEADER?.CUST_KIND; CUST_KIND.Items.Refresh();

                DEPATMAN.SelectedValue = HEADER.DEPATMAN; DEPATMAN.Items.Refresh(); //واحد

                SGN1.IsChecked = Convert.ToBoolean(HEADER.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER.SGN3);

                SGN1usid.Tag = Convert.ToInt32(HEADER.sgn1usid);
                SGN2usid.Tag = Convert.ToInt32(HEADER.sgn2usid);
                SGN3usid.Tag = Convert.ToInt32(HEADER.sgn3usid);

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER?.sgn3usid)?.SAL_NAME;


                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;


                OKF.IsChecked = HEADER.OKF; //تایید فاکتور
                MOLAH.Text = HEADER.MOLAH; //ملاحظات
                SHIFT.SelectedValue = HEADER.SHIFT; //شیفت
            }

            if (NUMBER.Text == null)
            {
                return;
            }

            byte TAG_TARGET = FTAG;
            if (_navigationManager.IsNewRecord) //مقدار جدیدی انتخاب شده یا وارد شده
            {
                TAG_TARGET = HTAG;
            }

            var HEADER_HAV = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE NUMBER = " + NUMBER.Text + $" AND TAG = {TAG_TARGET}").FirstOrDefault();
            if (HEADER_HAV != null)
            {
                DATE_N.Text = HEADER_HAV.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_HAV.USER_NAME.ToStringNullSafe(); //کاربر

                //مشتری
                string thevalue = HEADER_HAV.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();

                if (CUST_NO.ItemsSource == null)
                {
                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                }

                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                {
                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                }
                CUST_NO.SelectedValue = HEADER_HAV.CUST_NO; CUST_NO.Items.Refresh();


                if (HEADER_HAV?.FNUMCO != null) //شماره داخلی
                {
                    FNUMCO.Text = HEADER_HAV.FNUMCO.ToStringNullSafe();
                }
            }

            NUMBER1_TAG = Convert.ToDouble(NUMBER.Text); //Save Last Valid Number

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
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "BRFR", new WindowInteropHelper(this).Handle, this.GetType().Name);

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            if ((bool)Baseknow.UPDDATE)
            {
                this.DATE_N.IsEnabled = true;
            }
            else
            {
                this.DATE_N.IsEnabled = false;
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
        private void GET_MODAT_PPID_SOURCE()
        {
            MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";
        }
        private void MODAT_PPID_Enter()
        {
            if (Baseknow.GHAYM.ToString() != "7")
                return;

            int currentSelectedPPID = -1;
            if (_navigationManager?.CurrentRecord?.MODAT_PPID != null)
            {
                currentSelectedPPID = Convert.ToInt32(_navigationManager.CurrentRecord.MODAT_PPID);
            }

            if (DEPATMAN.SelectedItem == null)
            {
                universControl.PopNotifyShow("واحد نميتواند خالي باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            string tarikhRaw = DATE_N.Text.ToRawTarikh();
            if (!long.TryParse(tarikhRaw, out long tarikhValue))
            {
                // Handle invalid date if necessary
                return;
            }

            List<PRICE_PAYNO_MODATP> filteredList;
            if (PEID.SelectedValue != null)
            {
                filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM PRICE_PAYNO INNER JOIN PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID WHERE (PRICE_ELAMIETF_DTL.PEID = " + this.PEID.SelectedValue + ") union SELECT 0, 'آزاد', 0").ToList();
            }
            else
            {
                int departId = Convert.ToInt32(DEPATMAN.SelectedValue);
                string sqlGetPEID = "SELECT TOP (1) PEID FROM dbo.PRICE_ELAMIETF WHERE (PEDATE <= " + tarikhRaw + ") AND (PEPDEPART = " + departId + ") ORDER BY PEID DESC";
                int? lastPEID = dbms.DoGetDataSQL<int?>(sqlGetPEID).FirstOrDefault();

                if (lastPEID != null)
                {
                    string sqlFiltered = "SELECT P.PPID, P.PPAME, P.MODAT FROM PRICE_PAYNO P INNER JOIN PRICE_ELAMIETF_DTL D ON P.PPID = D.PPID WHERE D.PEID = " + lastPEID + " UNION SELECT 0, 'آزاد', 0";
                    filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlFiltered).ToList();
                }
                else
                {
                    if (_navigationManager?.CurrentRecord?.PEID != null)
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM PRICE_PAYNO INNER JOIN PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID WHERE (PRICE_ELAMIETF_DTL.PEID = " + _navigationManager.CurrentRecord.PEID + ") union SELECT 0, 'آزاد', 0").ToList();
                    }
                    else
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
                    }
                }
            }

            if (currentSelectedPPID > -1 && !filteredList.Any(p => p.PPID == currentSelectedPPID))
            {
                string sqlGetSaved = $"SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO WHERE PPID = {currentSelectedPPID}";
                PRICE_PAYNO_MODATP savedItem = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlGetSaved).FirstOrDefault();

                if (savedItem != null)
                {
                    savedItem.IsTempyDisplay = true;
                    filteredList.Add(savedItem);
                }
                else
                {
                    currentSelectedPPID = -1;
                }
            }

            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
            MODAT_PPID.ItemsSource = filteredList;
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";

            if (currentSelectedPPID > -1)
            {
                MODAT_PPID.SelectedValue = currentSelectedPPID;
            }
            else
            {
                MODAT_PPID.SelectedIndex = -1;
            }
            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
        }
        private void MODAT_PPID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (Baseknow.GHAYM == 7)
            {
                if (!CL_HESABDARI.LETSGO("AZADPAY") && Convert.ToInt32(MODAT_PPID.SelectedValue) == 0)
                {
                    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                    if (!_navigationManager.IsNewRecord)
                    {
                        MODAT_PPID.SelectedValue = _navigationManager?.CurrentRecord?.MODAT_PPID;
                    }
                    else
                    {
                        MODAT_PPID.SelectedIndex = -1;
                    }
                    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                    universControl.PopNotifyShow($"شما اجازه قيمت گذاري آزاد نداريد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                }

                if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP selectedItem && selectedItem.IsTempyDisplay)
                {
                    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                    if (e != null && e.RemovedItems.Count > 0)
                    {
                        MODAT_PPID.SelectedItem = e.RemovedItems[0] as PRICE_PAYNO_MODATP;
                    }
                    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                    universControl.PopNotifyShowUp($"این گزینه قابل انتخاب نیست : {selectedItem?.PPAME}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                }

                GetModatValueDays();
            }
        }

        private void MODAT_PPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!(e.NewFocus is ComboBox)) return;
            GetModatValueDays();
            GoGheymateUpdator();
        }

        private void MODAT_PPID_LostFocus(object sender, RoutedEventArgs e)
        {
            // Similar logic to FROOSH22 to focus grid if needed
            if (!MODAT_PPID.IsKeyboardFocusWithin && INVO_LST_FACTOR22_DATA.Count == 0)
            {
                INVO_LST_SUB.Dispatcher.BeginInvoke(() =>
                {
                    INVO_LST_SUB.SelectedIndex = INVO_LST_SUB.Items.Count - 1;
                    int idx = INVO_LST_SUB_DEF_INDEX_COL; // Default MABL
                    if (INVO_LST_SUB.Columns.Count > idx)
                    {
                        INVO_LST_SUB.CurrentCell = new DataGridCellInfo(INVO_LST_SUB.SelectedItem, INVO_LST_SUB.Columns[idx]);
                        INVO_LST_SUB.BeginEdit();
                    }
                });
            }
        }

        private void PEPID_DropDownOpened(object sender, EventArgs e) { /* Optional logic */ }
        private void PEID_DropDownOpened(object sender, EventArgs e) { /* Optional logic */ }

        private void PEPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEPID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter();
            }
            GoGheymateUpdator();
        }

        private void PEID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter();
            }
            GoGheymateUpdator();
        }
        private void GetModatValueDays(bool FocusonMAS = true)
        {
            if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP SelectedModatItem)
            {
                if ((bool)(SelectedModatItem?.PPAME?.Trim().Equals("آزاد")))
                {
                    if (string.IsNullOrWhiteSpace(MAS.Text) || MAS.Text == "0")
                    {
                        MAS.Text = "1";
                    }
                }
                else
                {
                    int modt = CL_HESABDARI.Getmodat(Convert.ToInt32(MODAT_PPID.SelectedValue));
                    if (modt != Convert.ToInt32(MAS.Text))
                    {
                        this.MAS.Text = modt.ToString();
                    }
                }
            }
            if (Convert.ToInt32(MODAT_PPID.SelectedValue) == 0)
            {
                this.MAS.IsReadOnly = false;
                if (FocusonMAS)
                {
                    Dispatcher.BeginInvoke(new Action(() => { this.MAS.Focus(); }));
                }
            }
            else
            {
                this.MAS.IsReadOnly = true;
            }
            IF_AZAD_THENLOCK();
        }

        private void GoGheymateUpdator()
        {
            // Note: FTAG is 25 for Sales Return Open
            var IsSavedBefore = CL_LMethods.IsNumeric(NUMBER.Text) && NUMBER.Text != "0";

            if (NowIsReady && CL_Generaly.IsGHAYM_7 && IsSavedBefore)
            {
                if (SGN1.IsChecked is false && SGN2.IsChecked is false && SGN3.IsChecked is false)
                {
                    if (CUST_KIND.SelectedValue != null && DEPATMAN.SelectedValue != null && MODAT_PPID.SelectedValue != null)
                    {
                        // ChangeIsHappend = true; // Optional: Track changes

                        if (Convert.ToInt32(MODAT_PPID.SelectedValue) == 0)
                        {
                            return;
                        }

                        int retVal = ExecutePricingUpdate(
                            Convert.ToInt32(NUMBER.Text),
                            FTAG, // 25
                            PEPID.SelectedValue is null ? 0 : Convert.ToInt32(PEPID.SelectedValue),
                            PEID.SelectedValue is null ? 0 : Convert.ToInt32(PEID.SelectedValue),
                            Convert.ToInt32(MODAT_PPID.SelectedValue),
                            TICMBAA.IsChecked == true,
                            Convert.ToInt32(CUST_KIND.SelectedValue),
                            Convert.ToInt32(DATE_N.Text.ToRawTarikh()),
                            Convert.ToInt32(DEPATMAN.SelectedValue));

                        // اگر اعلامیه تخفیف خالی یا صفر است، مقادیر تخفیف را در دیتابیس صفر کن
                        if (PEID.SelectedValue == null || Convert.ToInt32(PEID.SelectedValue) == 0)
                        {
                            // HTAG = 24 (سطرهای فاکتور برگشت فروش)
                            // The VAT (IMBAA) should be preserved or recalculated based on TICMBAA
                            dbms.DoExecuteSQL($@"UPDATE il
                                               SET 
                                                   il.N_KOL = 0, il.N_MOIN = 0, il.TKHN = 0,
                                                   il.IMBAA = CASE 
                                                       WHEN {(TICMBAA.IsChecked == true ? 1 : 0)} = 1 AND sd.CMBAA = 1 AND sd.vra IS NOT NULL THEN 
                                                           FLOOR(il.MABL_K * sd.vra / 100.0)
                                                       ELSE 0 
                                                   END
                                               FROM dbo.INVO_LST il
                                               JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE
                                               WHERE il.NUMBER = {NUMBER.Text} AND il.TAG = {HTAG}");
                            DoCmdHeaderSave();
                        }

                        string? strSpecificError = default;
                        if (retVal != 0)
                        {
                            switch (retVal)
                            {
                                case -1: strSpecificError = "خطا: اعلامیه قیمت فعال یافت نشد."; break;
                                case -2: strSpecificError = "خطا: قیمت برای یک یا چند کالا در اعلامیه قیمت مشخص، تعریف نشده است."; break;
                                case -99: strSpecificError = "خطا: یک خطای عمومی در پایگاه داده رخ داد."; break;
                                default: strSpecificError = "خطا: عملیات ناموفق بود. کد خطای ناشناخته: " + retVal; break;
                            }
                            new Msgwin(false, strSpecificError).ShowDialog();
                        }

                        ReGetDataAll(); // Refresh Grids

                        if (!string.IsNullOrEmpty(strSpecificError))
                        {
                            return;
                        }

                        universControl.PopNotifyShowUp("قیمت بروز شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green, 1);
                        IF_AZAD_THENLOCK();
                    }
                }
            }
        }

        private int ExecutePricingUpdate(int numb, int tgg, int pepid, int peid, int modat_ppid, bool ticmbaa, int cust_kind, int dtt, int depatman)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@numb", numb);
            param.Add("@tgg", tgg);
            param.Add("@PEPID_In", pepid);
            param.Add("@PEID_In", peid);
            param.Add("@MODAT_PPID_In", modat_ppid);
            param.Add("@TICMBAA_In", ticmbaa);
            param.Add("@CUST_KIND_In", cust_kind);
            param.Add("@DTT_In", dtt);
            param.Add("@DEPATMAN_In", depatman);
            param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            dbms.OpenStoredProcedure("sp_UpdateInvoicePricingAndDiscount", param);

            return param.Get<int>("@ReturnValue");
        }

        private void IF_AZAD_THENLOCK()
        {
            if (Baseknow.GHAYM == 7)
            {
                if (MODAT_PPID.SelectedIndex == 0) // Azad
                {
                    MABL_COLUMN.IsReadOnly = false;
                    MABL_K_COLUMN.IsReadOnly = false;
                    N_KOL_COLUMN.IsReadOnly = false;
                    N_MOIN_COLUMN.IsReadOnly = false;
                }
                else
                {
                    MABL_COLUMN.IsReadOnly = true;
                    MABL_K_COLUMN.IsReadOnly = true;
                    N_KOL_COLUMN.IsReadOnly = true;
                    N_MOIN_COLUMN.IsReadOnly = true;
                }
            }
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

            // --- NEW CODE START ---
            // نحوه پرداخت و مدت
            GET_MODAT_PPID_SOURCE();

            // اعلامیه قیمت
            PEPID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_CSHARP>("SELECT PEPID, PEPNAME, PEPDATE, PEPDEPART FROM PRICE_ELAMIE ORDER BY PEPNAME DESC").ToList();
            PEPID.DisplayMemberPath = "PEPNAME";
            PEPID.SelectedValuePath = "PEPID";

            // اعلامیه تخفیف
            PEID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_ETF_TAKHFIF__CSHARP>("SELECT PEID, PENAME, PEDATE, PEPDEPART FROM PRICE_ELAMIETF").ToList();
            PEID.DisplayMemberPath = "PENAME";
            PEID.SelectedValuePath = "PEID";

            //شماره رسید سایر انبار 
            //NUMBER.ItemsSource = dbms.DoGetDataSQL<QRE_LST_BARGASHT>($"SELECT NUMBER FROM HEAD_LST WHERE (TAG = {HTAG /*24*/}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG /*25*/}))))) ORDER BY NUMBER").ToList();
            RefreshReceiptNumbers();

            //پشت فاکتور بخش چک:
            #region POSHTE_FACTOR

            //کمبوباکس های پشت فاکتور
            bANKColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();


            var HESNAMELST = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT N_KOL,NUMBER,TNUMBER, RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) AS hes, NAME FROM TDETA_HES").ToList();
            CMB_MOIN_HAZ.ItemsSource = HESNAMELST.ToList(); //معين خدمات
            CMB_HMBAA.ItemsSource = HESNAMELST.ToList(); //معین مالیات

            //دریافت چک:
            //Giving All Data as Master:
            //معین بانک
            n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {Baseknow.BANKHA})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //تفضیلی
            n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.N_KOL) ={Baseknow.BANKHA}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();

            #endregion

            //الگوی پورسانت:
            PORID_COLUMN.ItemsSource = dbms.DoGetDataSQL<PORD_COL_MODEL>("SELECT VISITORS_PORSANT.PORID, CAST(VISITORS_PORSANT.PORID AS nvarchar) + N' - ' + CAST(VISITORS_PORSANT.VDATE AS nvarchar) + N' - ' + ISNULL(CUSTKIND.CUSTKNAME, N'بدون گروه (همه)') + N' - ' + ISNULL(VISITORS_PORSANT.COMMENT, N' ') + N' - ' + CUST_HESAB.NAME AS Expr1 FROM VISITORS_PORSANT INNER JOIN CUST_HESAB ON VISITORS_PORSANT.HES = CUST_HESAB.hes LEFT OUTER JOIN CUSTKIND ON VISITORS_PORSANT.CUST_COD = CUSTKIND.CUST_COD").ToList();
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
            if (NUMBER.Text != null)
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
	                 WHERE	(dbo.INVO_LST.TAG = {HTAG}) AND (dbo.INVO_LST.NUMBER={NUMBER.Text}) ").ToList(); //-- NUMBER1

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
                            SERCHK sERCHK = new SERCHK(I_AM_BARGASHT_NORMAL, CURRENT_ITEMS_ROW.ANBAR.ToString());
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
                                CURRENT_ITEMS_ROW.VAHED_K = null; //Reset VAHED_K

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
                                CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITEMS_ROW.ANBAR.ToString(), I_AM_BARGASHT_NORMAL);
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
                if ((e.Row.Item as INVO_LST_FACTOR22)?.ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22)?.CODE is null)
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

            //تخفیف
            #region N_KOL
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.N_KOL = 0;
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ITEMS_ROW.N_KOL) * Convert.ToDouble(CURRENT_ITEMS_ROW.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ITEMS_ROW.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ITEMS_ROW.N_KOL) * Convert.ToDouble(CURRENT_ITEMS_ROW.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ITEMS_ROW.TKHN) / 100);
                    return;
                }
                if (
                    CURRENT_ITEMS_ROW.ANBAR is null ||
                    CURRENT_ITEMS_ROW.CODE is null ||
                    CURRENT_ITEMS_ROW.VAHED_K is null ||
                    CURRENT_ITEMS_ROW.MEGH is null ||
                    CURRENT_ITEMS_ROW.MEGHk is null ||
                    CURRENT_ITEMS_ROW.MABL is null ||
                    CURRENT_ITEMS_ROW.MABL_K is null
                    )
                {
                    return;
                }
                else // IF ALL IS RIGHT ABOUT THIS ↓
                {
                    var nkol = CURRENT_ITEMS_ROW.N_KOL;
                    if (string.IsNullOrEmpty(nkol.ToStringNullSafe()))
                    {
                        CURRENT_ITEMS_ROW.N_KOL = 0;
                        nkol = 0;
                    }

                    #region N_KOL_AfterUpdate
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100));
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                        if (RST.Count > 0)
                        {
                            if ((bool)RST.FirstOrDefault().CMBAA)
                            {
                                if (CURRENT_ITEMS_ROW.IMBAA != Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - CURRENT_ITEMS_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITEMS_ROW.CODE) / 100)))
                                {
                                    CURRENT_ITEMS_ROW.IMBAA = Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - CURRENT_ITEMS_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITEMS_ROW.CODE) / 100));
                                }
                            }
                            else if (CURRENT_ITEMS_ROW.IMBAA != 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                msgwin.ShowDialog();
                                if (msgwin.DialogResult is true)
                                {
                                    CURRENT_ITEMS_ROW.IMBAA = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.IMBAA = 0;
                    }
                    #endregion
                }
            }
            #endregion

            //مبلغ تخفیف
            #region N_MOIN
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("N_MOIN").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
                    return;
                }
                if (
                    CURRENT_ITEMS_ROW.ANBAR is null ||
                    CURRENT_ITEMS_ROW.CODE is null ||
                    CURRENT_ITEMS_ROW.VAHED_K is null ||
                    CURRENT_ITEMS_ROW.MEGH is null ||
                    CURRENT_ITEMS_ROW.MEGHk is null ||
                    CURRENT_ITEMS_ROW.MABL is null ||
                    CURRENT_ITEMS_ROW.MABL_K is null
                    )
                {
                    return;
                }
                else // IF ALL IS RIGHT ABOUT THIS ↓
                {
                    #region N_MOIN_AfterUpdate
                    if (CURRENT_ITEMS_ROW.MABL_K > 0)
                    {
                        CURRENT_ITEMS_ROW.N_KOL = CURRENT_ITEMS_ROW.N_MOIN * 100 / CURRENT_ITEMS_ROW.MABL_K;
                        CURRENT_ITEMS_ROW.TKHN = 0;
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.N_MOIN = 0;
                        CURRENT_ITEMS_ROW.N_KOL = 0;
                        CURRENT_ITEMS_ROW.TKHN = 0;
                    }
                    if ((bool)TICMBAA.IsChecked)
                    {
                        var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                        if (RST.Count > 0)
                        {
                            if ((bool)RST.FirstOrDefault().CMBAA)
                            {
                                if (CURRENT_ITEMS_ROW.IMBAA != Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - CURRENT_ITEMS_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITEMS_ROW.CODE) / 100)))
                                {
                                    CURRENT_ITEMS_ROW.IMBAA = Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - CURRENT_ITEMS_ROW.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ITEMS_ROW.CODE) / 100));
                                }
                            }
                            else if (CURRENT_ITEMS_ROW.IMBAA != 0)
                            {
                                Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                msgwin.ShowDialog();
                                if (msgwin.DialogResult is true)
                                {
                                    CURRENT_ITEMS_ROW.IMBAA = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.IMBAA = 0;
                    }
                    #endregion
                }
            }
            #endregion

            TEDADM.Text = SUM_OF_MEGH_K.ToStringNullSafe();

        }
        private void INVO_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            INVO_LST_FACTOR22? TheRow = e.Row.Item as INVO_LST_FACTOR22;

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
                _qre = $@"UPDATE dbo.INVO_LST
                   SET ANBAR = {TheRow.ANBAR}, CODE = N'{TheRow.CODE}',
                   MEGH = {TheRow.MEGH}, MEGHk = {TheRow.MEGHk}, MEGH_MAR = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},
                   MANDAH = N'{TheRow.MANDAH}', MABL = {TheRow.MABL}, MABL_K = {TheRow.MABL_K},
                   N_RASID = N'{(TheRow.N_RASID is null ? "NULL" : TheRow.N_RASID)}',
                   MEGH_R = {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)}, 
                   RADAH = {(TheRow.RADAH is null ? "NULL" : TheRow.RADAH)}, 
                   SANAD_NO = {(TheRow.SANAD_NO is null ? "NULL" : TheRow.SANAD_NO)},
                   ANBARF = {(TheRow.ANBARF is null ? "NULL" : TheRow.ANBARF)}, 
                   VAHED_K = {TheRow.VAHED_K}, N_KOL = {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)}, 
                   N_MOIN = {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)}, N_TAF = {(TheRow.N_TAF is null ? "NULL" : TheRow.N_TAF)},
                   IMBAA = {TheRow.IMBAA}, 
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
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }
            //بررسی صحیحی بودن مبلغ
            if (TheRow.MABL_K != Math.Round((double)(TheRow.MABL * TheRow.MEGHk)))
            {
                var _mablk = Math.Round((double)(TheRow.MABL * TheRow.MEGHk));
                if (TheRow.MABL_K != _mablk)
                {
                    TheRow.MABL_K = Math.Round((double)(TheRow.MABL * TheRow.MEGHk));
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mablk} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
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
            // After a row edit, the header discount should be the sum of the line item discounts.
            GetUpdateTakhfif();
            // Call Summer() again to update totals like "GHABEL" (payable amount) based on the new header discount.
            Summer();

            //// از بازتوزیع خودکار تخفیف زمانی که کاربر مستقیماً مقدار سطر را تغییر داده جلوگیری می‌کنیم
            //TAKHFIF_MABL_PRICE(isTakhfifFocus: true, shouldDistributeToRows: false);

            UpdateVisitorCommissions();

            MasterTopErrorMessages.AddRange(ErrosMessages);

            SANAD();

            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

            PAY_GETP_SUB_SUB_ReGetData();
            VISITOR_DTL_SUB_ReGetData();
        }

        private void GetUpdateTakhfif()
        {
            double total_line_discount = INVO_LST_FACTOR22_DATA.Sum(r => r.N_MOIN ?? 0);
            TAKHFIF.Text = total_line_discount.ToString();
            // Recalculate the percentage based on the new total discount
            if (SUM_OF_MABL_K > 0)
            {
                // Note: SUM_OF_MABL_K is a property that calculates the sum on the fly.
                double percentage = (total_line_discount / SUM_OF_MABL_K) * 100;
                TAKHFIF_PERCENT.Text = Math.Round(percentage, 2).ToString();
            }
            else
            {
                TAKHFIF_PERCENT.Text = "0";
            }
        }

        public void AVRAGE_UPDATE()
        {
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
            if (((bool)SGN1.IsChecked || (bool)SGN2.IsChecked) || (bool)SGN3.IsChecked)
            {
                this.Command100.IsEnabled = true;
                this.Command108.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command108.IsEnabled = false;
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
        /// <summary>
        /// کنترل اینکه محتوای یک فیلد حساب (پشت فاکتور) واقعاً یک حساب موجود در سرفصل است.
        ///
        /// چرا لازم است: این فیلدها TextBox آزادند و اگر کاربر متن دلخواه تایپ کند
        /// (نمونه‌ی واقعی: «خمات» در فیلد معین خدمات) رکورد ذخیره می‌شود ولی بعداً
        /// موتور صدور سند نمی‌تواند آن را به کل/معین/تفصیلی تفکیک کند و سند صادر نمی‌شود.
        /// فیلد خالی خطا نیست؛ کنترل «خالی بودن در برابر مبلغ» جداگانه انجام می‌شود.
        /// </summary>
        private void ValidateHesabField(List<MsgModel> errors, string? hesText, string fieldTitle)
        {
            if (string.IsNullOrWhiteSpace(hesText)) { return; }

            if (!CL_HESABDARI.ISHESAB3(hesText.Trim()))
            {
                errors.Add(new MsgModel { MessageText_U = $"مقدار «{hesText}» در فیلد {fieldTitle} یک حساب معتبر نیست؛ حساب را از لیست انتخاب کنید." });
            }
        }

        /// <summary>
        /// همان کنترل بالا، ولی لحظه‌ی خروج فوکوس از فیلدهای حساب پشت فاکتور.
        /// مقدار نامعتبر همان‌جا پاک و پیام داده می‌شود، پس مقدار خراب در فرم نمی‌ماند.
        /// </summary>
        private void HesabField_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) { return; }

            string val = (tb.Text ?? "").Trim();
            if (string.IsNullOrEmpty(val)) { return; }
            if (CL_HESABDARI.ISHESAB3(val)) { return; }

            tb.Text = "";
            universControl.PopNotifyShow($"«{val}» حساب معتبری نیست ! حساب را از لیست انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1);
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(NUMBER.Text.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره رسید نمیتواند خالی باشد!" });
            }
            else if (CL_LMethods.IsNumeric(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره رسید انتخاب شده معتبر نیست!" });
            }

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

            if (NUMBER.Text == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره فاکتور نميتواند  خالي باشد." });
            }
            else
            {
                if (_navigationManager.IsNewRecord)
                {
                    var RST = dbms.DoGetDataSQL<double?>($"SELECT HEAD_LST.NUMBER1 FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {HTAG})) GROUP BY HEAD_LST.NUMBER1 HAVING (((HEAD_LST.NUMBER1)= " + NUMBER.Text + "))").FirstOrDefault();
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

            ValidateHesabField(ErrosMessages, MOIN_HAZ.Text, "معین خدمات (هزینه) در پشت فاکتور");
            ValidateHesabField(ErrosMessages, HMBAA.Text, "معین مالیات بر ارزش افزوده");

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
        bool isSavedSuccess = false;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            isSavedSuccess = false;

            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            errors = (from object i in PAY_GETP_SUB.ItemsSource
                      let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                if (string.IsNullOrWhiteSpace(NUMBER1.Text) || NUMBER1.Text == "0")
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

                            var rst_11 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER1) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={FTAG}))", null, transaction).FirstOrDefault();
                            if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                            {
                                NUMBER1.Text = Baseknow.STHFR.ToString();
                                NUMBER.UpdateLayout();
                            }
                            else
                            {
                                NUMBER1.Text = Convert.ToDouble(rst_11 + 1).ToString();
                                NUMBER.UpdateLayout();
                            }

                            db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER1,  NUMBER   ,    TAG,     DATE_N,  MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF)
                                                        VALUES ({NUMBER1.Text}, {NUMBER.Text} , {FTAG},    0,    0,   0,       0,        0,        0,        0,    0   )", null, transaction);

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

            GoGheymateUpdator();

            Summer();
            UpdateVisitorCommissions();

            this.OKF.IsChecked = true;

            this.INVO_LST_SUB.IsReadOnly = false;
            this.INVO_LST_SUB.IsReadOnly = false;
            this.Page58.IsEnabled = true;

            SANAD();

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 4, Convert.ToInt32(Baseknow.USERCOD), WINDOW_ID);
            }
            else
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }

            DataGridActivation();
            Page57.IsEnabled = true;
            Page155.IsEnabled = true;

            ChangeIsHappend = false;

            isSavedSuccess = true;
        }

        private void GetBalancePerson()
        {
            //کادر سبز و سند و مانده حساب
            var SANAD_NUMBER = dbms.DoGetDataSQL<string>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (SANAD_NUMBER != null)
            {
                //var _rst_ = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + ")").FirstOrDefault();
                //if (_rst_ is null) // if (rst.Count == 0)
                //    MANDAH.Text = "0";
                //else
                //{
                //    if (_rst_ > 0)
                //        MANDAH.Text = Strings.Format(_rst_, "#,### ريال بدهكار");
                //    else
                //        MANDAH.Text = Strings.Format((_rst_ * -1), "#,### ريال بستانكار");
                //}

                if (CUST_NO.SelectedValue != null)
                {
                    MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                }

                N_S.Text = SANAD_NUMBER?.ToString();
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S = 4 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
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

            var hmbaaValue = CMB_HMBAA.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(hmbaaValue))
            {
                hmbaaValue = HMBAA.Text;
            }

            _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, NUMBER1 = {NUMBER1.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()}, TICMBAA = {Convert.ToByte(TICMBAA.IsChecked)}, 
                    N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', 
                    M_NAGHD = {M_NAGHD.Text},TAKHFIF = {TAKHFIF.Text},
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = N'{hmbaaValue}', MAS = {MAS.Text},
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    ANBAR =  {(ANBAR is null ? "NULL" : ANBAR)},
                    USER_NAME = N'{USER_NAME.Text}',
                    MODAT_PPID = {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)}, 
                    PEPID = {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)},
                    PEID = {(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)},
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";

            _ = dbms.DoExecuteSQL(_qre);

            var originalRecord = _navigationManager?.CurrentRecord;
            if (originalRecord != null)
            {
                int? currentKind = (int?)CUST_KIND.SelectedValue;
                string? currentCustomer = (string?)CUST_NO.SelectedValue; // This is safe even if null

                bool isDifferent = (currentKind != originalRecord.CUST_KIND) || (currentCustomer != originalRecord.CUST_NO);

                if (isDifferent)
                {
                    try
                    {
                        var sqlParams = new
                        {
                            CUST_NO = currentCustomer,
                            CUST_KIND = currentKind,
                            NUMBER = Convert.ToDouble(NUMBER.Text),
                            TAG = HTAG //24
                        };
                        dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET CUST_NO = @CUST_NO, CUST_KIND = @CUST_KIND " +
                                                     $"WHERE NUMBER = @NUMBER AND TAG = @TAG", sqlParams);

                        universControl.PopNotifyShow("مشتری و نوع آن در رسید انبار متناظر بروز شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C", 3);
                    }
                    catch (Exception ex)
                    {
                        new Msgwin(false, "خطا در بروزرسانی رسید انبار متناظر.").ShowDialog();
                    }
                }
            }

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
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                        CL_HESABDARI.TR("VISITOR_DTL", $"(TAG = {HTAG} and NUMBER = " + NUMBER.Text + ")", dt, 1);


                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        this.INVO_LST_SUB.IsReadOnly = false;
                        this.Page58.IsEnabled = true;
                        this.Page155.IsEnabled = true;
                        NUMBER1.IsEnabled = true;

                        if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                        {
                            this.INVO_LST_SUB.IsReadOnly = true; //.Locked = true;
                            this.PAY_GETP_SUB.IsEnabled = false;
                            this.VISITOR_DTL_SUB.IsEnabled = false;
                            this.DATE_N.IsReadOnly = true;
                            this.MOLAH.IsReadOnly = true;
                            this.AllowEdits = true;
                        }
                        else
                        {
                            this.INVO_LST_SUB.IsReadOnly = false;
                            this.PAY_GETP_SUB.IsEnabled = true;
                            this.VISITOR_DTL_SUB.IsEnabled = true;
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

            if (!BTN_DELETE.IsEnabled || _navigationManager.IsNewRecord) { return; }

            if (PAY_GETP_SUB_DATA.Count > 0)
            {
                new Msgwin(false, "این فاکتور دارای اطلاعات چک است , ابتدا آنرا حذف کنید سپس مجددا اقدام کنید.").ShowDialog();
                return;
            }
            if (SAYER_VISITOR_DATA.Count > 0)
            {
                new Msgwin(false, "این فاکتور دارای اطلاعات بخش سایر است , ابتدا آنرا حذف کنید سپس مجددا اقدام کنید.").ShowDialog();
                return;
            }

            if (SUM_OF_MABL_K > 0)
            {
                new Msgwin(false, "ابتدا باید مبالغ رو صفر کنید سپس مجددا اقدام به حذف کنید..").ShowDialog();
                return;
            }
            var TAKHF_MABL = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.N_MOIN ?? 0);
            var TAKHF_DARSAD = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.N_KOL ?? 0);
            if (TAKHF_MABL > 0 || TAKHF_DARSAD > 0)
            {
                new Msgwin(false, "ابتدا باید تخفیفات رو صفر کنید سپس مجددا اقدام به حذف کنید..").ShowDialog();
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                #region SABEGHEH
                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                CL_HESABDARI.TR("VISITOR_DTL", $"(TAG = {HTAG} and NUMBER = " + NUMBER.Text + ")", dt, 1);
                #endregion

                _ = AuditLogger.LogActionAsync(
                        actionType: "DELETE",
                        tableName: "فاکتور برگشت فروش (آزاد) رسید شده",
                        recordId: NUMBER.Text,
                        oldValue: "TAG = 25",
                        newValue: null,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER1.Text) && NUMBER1.Text != "0")
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER1 = {NUMBER1.Text} AND TAG = {FTAG}");

                        SANAD();

                        _navigationManager?.DeleteCurrentRecord(); //Refresh Record Source
                    }
                    catch (SqlException ex)
                    {
                        if (e != null)
                        {
                            e.Handled = true;
                        }

                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این فاکتور دارای اطلاعات (سند) وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
            //if (!LETSGO("FRSKB"))
            //{
            //    this.RecordSource = "SELECT TOP 100 PERCENT NUMBER, TAG AS htag, 24 AS Dtag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, USER_NAME, TICMBAA, MBAA, HMBAA, DEPATMAN, SHIFT, CUST_KIND, OKF, SADER,UID FROM HEAD_LST WHERE (TAG = 25) and (USER_NAME = '" + UCurrentUser() + "') ORDER BY NUMBER";
            //}
            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 954 AND TAG = 25 --فاکتور برگشت فروش رسید شده : آزاد Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 954 AND TAG = 24
            new FACTORS_LST(FTAG).Show();
            if (_navigationManager.IsNewRecord)
            {
                this.Close();
            }
        }

        private void Summer()
        {
            JJKOL.Text = SUM_OF_MABL_K.ToString(); //SMABLK //جمع فاکتور : Sum(MABMAR)
            HKH.Text = MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL.Text; //جمع کل فاکتور برای فسمت روی فاکتور

            TEDADM.Text = SUM_OF_MEGH_K.ToString(); //جمع مقادیر مرجوعی :

            if (SUM_OF_MABL_K == 0)
            {
                TAKHFIF_PERCENT.Text = "0";
            }

            NCHK.Text = PAY_GETP_SUB_DATA.Sum(x => x.MABL)?.ToString(); //جمع مبالغ چکهای پرداختی

            ////مبلغ قابل پرداخت: //= [JF] + [HKH] - [NTKHFIF] + [MBAA]
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            ////جمع مبالغ پرداختی
            ////=[M_NAGHD]+[MABL_VAR]+[MABL_HAV]+[NCHK]
            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(NCHK.Text);
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

            if (!_navigationManager.IsNewRecord && PERSONEL.SelectedItem != null)
            {
                string SelectedTextCMB = ((COMBOPERSONEL)PERSONEL.SelectedItem).SAL_NAME.ToStringNullSafe();

                Meidnum = CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور برگشت فروش. آزاد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

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
                SHARH = "'فاکتور برگشت فروش. آزاد شماره: " + NUMBER.Text + " مورخ " + DATE_N.Text.ToRawTarikh() + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
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

            WinSignActivator();
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
                SHARH = "'فاکتور برگشت فروش. آزاد  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
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

            WinSignActivator();
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
                SHARH = "'فاکتور برگشت فروش. آزاد  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG} )");
            }
            ////CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خريد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            this.SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();
            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(this.SGN3.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void WinSignActivator()
        {
            if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
            {
                AllowEdits = false;
                AllowDeletions = false;

                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                AllowEdits = true;
            }
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
            if (CUTSNO_TEX is null) { return; }

            if (CUST_NO.SelectedValue is not null)
            {
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text) //Text Really Not Changed
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"مشتری نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }

            if (CUST_NO.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    CUST_NO.SelectedValue = null;
                }
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

        }
        private void SANAD()
        {
            //throw new Exception();
            try
            {
                AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.gensanadbargashfroosh2(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات صدور سند , مجددا تلاش کنید").Show();
                return;
            }

            Summer();
        }

        #region POSHTE_FACTOR
        public PAY_GETP_MODEL? PAY_GETP_SUB_WAS_ROW_ITEM { get; set; }
        public void PAY_GETP_SUB_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //PAY_GETP_SUB_DATA
                PAY_GETP_SUB_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<PAY_GETP_MODEL>($@"SELECT * FROM PAY_GETP WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        PAY_GETP_SUB_DATA.Add(item);
                    }
                    NCHK.Text = PAY_GETP_SUB_DATA.Sum(x => x.MABL)?.ToString(); //جمع مبالغ چکهای پرداختی
                }
            }
        }
        private bool PayGetpBodyIsValid(PAY_GETP_MODEL TheRow)
        {
            var errors = (from object i in PAY_GETP_SUB.ItemsSource
                          let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }


            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(TheRow.N_HESAB))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب وارد نشده !" });
            }
            if (!double.TryParse(TheRow.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(TheRow?.N_SERI?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
            }
            if (!int.TryParse(TheRow.DATE?.ToString(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
            }
            if (!double.TryParse(TheRow.BANK?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.BANK?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
            }
            if (!double.TryParse(TheRow.DATE_S?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
            }
            if (!int.TryParse(TheRow.MABL?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.MABL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
            }
            if (!int.TryParse(TheRow.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_KOL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
            }
            if (!int.TryParse(TheRow.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_MOIN?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
            }
            if (TheRow.N_TAF is not null)
            {
                if (!int.TryParse(TheRow.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_TAF?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
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

        private void PAY_GETP_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (PAY_GETP_SUB.SelectedItem != null)
            {
                if (PAY_GETP_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    PAY_GETP_SUB_WAS_ROW_ITEM = ((PAY_GETP_MODEL)PAY_GETP_SUB.SelectedItem).Clone() as PAY_GETP_MODEL;
                }
            }
        }
        private void PAY_GETP_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS_
            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

            var PAY_GETP_SUB_ROW_INDEX = row_index;

            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            object PAY_GETP_SUB_ENTERED_VALUE;
            if (!ReferenceEquals(Comboval, null))
                PAY_GETP_SUB_ENTERED_VALUE = Comboval.SelectedValue;
            else
                PAY_GETP_SUB_ENTERED_VALUE = TexboVal.Text.Trim();

            var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;
            #endregion

            #region SET_NULL_IF_ROW_IS_NOT_VALID
            //بررسی در صورت تغییر نال کردن برای جلوگیری از اشتباه
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (PAY_GETP_SUB_WAS_ROW_ITEM.N_KOL != PAY_GETP_SUB_ROW_ITEMS.N_KOL) //تغییر یافته
                {
                    //معین بانک
                    var comboBox = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_MOIN").GetCellContent(e.Row) as ComboBox;
                    comboBox.ItemsSource = null;

                    //تفضیلی
                    var comboBox1 = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (PAY_GETP_SUB_WAS_ROW_ITEM.N_MOIN != PAY_GETP_SUB_ROW_ITEMS.N_MOIN) //تغییر یافته
                {
                    //تفضیلی
                    var comboBox1 = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
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
                if (!IsNull(PAY_GETP_SUB_ROW_ITEMS?.N_SERI) && !IsNull(PAY_GETP_SUB_ROW_ITEMS?.BANK))
                {
                    if (PAY_GETP_SUB_ROW_ITEMS?.ID == null || PAY_GETP_SUB_ROW_ITEMS?.BANK != PAY_GETP_SUB_WAS_ROW_ITEM?.BANK || PAY_GETP_SUB_ROW_ITEMS?.N_SERI != PAY_GETP_SUB_WAS_ROW_ITEM?.N_SERI)
                    {
                        var filter = "N_SERI=" + PAY_GETP_SUB_ROW_ITEMS.N_SERI + " AND BANK = " + PAY_GETP_SUB_ROW_ITEMS.BANK;
                        var rst = dbms.DoGetDataSQL<PAY_GETP>($"SELECT * FROM PAY_GETP WHERE {filter} ").FirstOrDefault();
                        if (rst != null)
                        {
                            new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود").ShowDialog();

                            var rst2 = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.ADA + "' OR HES = '" + Baseknow.ADV + "' ) AND (BES > 0) AND (BANK = "
                                + PAY_GETP_SUB_ROW_ITEMS?.BANK + ") AND (N_SERI = " + PAY_GETP_SUB_ROW_ITEMS.N_SERI + ")").FirstOrDefault();
                            if (rst2 != null)
                            {
                                new Msgwin(false, "اين چك در سند شماره " + rst2 + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را تغییر دهید").ShowDialog();
                            }
                            else
                            {
                                PAY_GETP_SUB_ROW_ITEMS.ID = rst.ID; //برای اینکه آپدیت بشه نه INSERT

                                PAY_GETP_SUB_ROW_ITEMS.N_SERI = rst.N_SERI;
                                PAY_GETP_SUB_ROW_ITEMS.BANK = rst.BANK;
                                PAY_GETP_SUB_ROW_ITEMS.DATE_S = rst.DATE_S;
                                PAY_GETP_SUB_ROW_ITEMS.RADIF = rst.RADIF;
                                PAY_GETP_SUB_ROW_ITEMS.SHOBEH = rst.SHOBEH;
                                PAY_GETP_SUB_ROW_ITEMS.DATE = rst.DATE;
                                PAY_GETP_SUB_ROW_ITEMS.NAME_TAH = rst.NAME_TAH;
                                PAY_GETP_SUB_ROW_ITEMS.N_HESAB = rst.N_HESAB;
                                PAY_GETP_SUB_ROW_ITEMS.MABL = rst.MABL;
                                PAY_GETP_SUB_ROW_ITEMS.N_KOL = rst.N_KOL;
                                PAY_GETP_SUB_ROW_ITEMS.N_MOIN = rst.N_MOIN;

                                if (rst?.N_KOL != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL = rst?.N_KOL;
                                if (rst?.N_MOIN != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN = rst?.N_MOIN;
                                if (rst?.N_TAF != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF = rst?.N_TAF;
                                if (rst?.N_TAF2 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF2 = rst?.N_TAF2;
                                if (rst?.N_TAF3 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF3 = rst?.N_TAF3;

                                if (PAY_GETP_SUB_ROW_ITEMS?.N_KOL?.ToString() == "911") //از نوع حذف شده انتظامی
                                {
                                    if (PAY_GETP_SUB_ROW_ITEMS?.N_KOL?.ToStringNullSafe() != Baseknow.BANKHA?.ToStringNullSafe())
                                    {
                                        if (rst?.N_KOL != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL = null;
                                        if (rst?.N_MOIN != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN = null;
                                        if (rst?.N_TAF != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF = null;
                                    }

                                    if (rst?.N_KOL2 != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL2 = null;
                                    if (rst?.N_MOIN2 != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN2 = null;
                                    if (rst?.N_TAF2 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF2 = null;

                                    if (rst?.N_KOL3 != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL3 = null;
                                    if (rst?.N_MOIN3 != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN3 = null;
                                    if (rst?.N_TAF3 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF3 = null;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            if (e.Column.SortMemberPath == "DATE_S") //تاریخ سررسید
            {
                //if (CL_HESABDARI.CHEKDATEM((long)PAY_GETP_SUB_ROW_ITEMS.DATE_S, false) is true) //تاریخ صحیح نیست
                //{
                //    PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                //}
                string date_n_val = PAY_GETP_SUB_ROW_ITEMS.DATE_S.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                        universControl.PopNotifyShow("تاریخ سررسید صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
                else
                {
                    PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                    universControl.PopNotifyShow("تاریخ سررسید نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            if (e.Column.SortMemberPath == "DATE") //تاريخ دريافت
            {
                string date_n_val = PAY_GETP_SUB_ROW_ITEMS.DATE.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                        universControl.PopNotifyShow("تاريخ دريافت صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                    else
                    {
                        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                        {
                            PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                            universControl.PopNotifyShow(".تاريخ دريافت به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                            return;
                        }
                    }
                }
                else
                {
                    PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                    universControl.PopNotifyShow("تاريخ دريافت نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                //if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(PAY_GETP_SUB_ROW_ITEMS.DATE), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                //{
                //    PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                //}
            }
            if (e.Column.SortMemberPath == "SANDUGH")
            {
                //در RowEnd لاگ میزنم
                //rst.Open("dbo.PAY_GETP_SUB_LOG", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //rst.AddNew();
                //rst.update();
            }
            if (e.Column.SortMemberPath == "VAZ")
            {
            }
            if (e.Column.SortMemberPath == "SAYADI")
            {
                if (!PayGetpBodyIsValid(PAY_GETP_SUB_ROW_ITEMS))
                {
                    PAY_GETP_SUB.Dispatcher.InvokeAsync(() =>
                    {
                        PAY_GETP_SUB.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                        PAY_GETP_SUB.CancelEdit(DataGridEditingUnit.Cell);
                        PAY_GETP_SUB.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                    });
                    return;
                }
            }
            //DATE - تاريخ دريافت   |   DATE_S - تاريخ سررسيد
        }
        private void PAY_GETP_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            #region WORKS
            //var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;
            //if (n_MOINColumn.ItemsSource is null) //MOIN
            //{
            //    if (PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null)
            //    {
            //        //معین بانک
            //        n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETP_SUB_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //    }
            //}
            //if (n_TAFColumn.ItemsSource is null) //TAFZILY
            //{
            //    if (PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null && PAY_GETP_SUB_ROW_ITEMS.N_MOIN is not null)
            //    {
            //        //تفضیلی
            //        n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) ={PAY_GETP_SUB_ROW_ITEMS.N_MOIN}) AND ((TDETA_HES.N_KOL) ={PAY_GETP_SUB_ROW_ITEMS.N_KOL}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
            //    }
            //}
            #endregion

            var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;

            int DefVale = 0;
            ComboBox THE_COMBO = e.EditingElement as ComboBox;

            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (!(e.EditingElement is null) && PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //معین بانک
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE (((DETA_HES.N_KOL) = {PAY_GETP_SUB_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
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
                if (!(e.EditingElement is null) && PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null && PAY_GETP_SUB_ROW_ITEMS.N_MOIN is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //تفضیلی
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) =" + PAY_GETP_SUB_ROW_ITEMS.N_MOIN + ") AND ((TDETA_HES.N_KOL) =" + PAY_GETP_SUB_ROW_ITEMS.N_KOL + "))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
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
        private void PAY_GETP_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (PAY_GETP_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = PAY_GETP_SUB.CurrentCell.Column.SortMemberPath;
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
        private void PAY_GETP_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var FINAL_CROW_ITEM = (e.Row.Item as PAY_GETP_MODEL);

            var DG = PAY_GETP_SUB;

            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (!PayGetpBodyIsValid(FINAL_CROW_ITEM))
            {
                DG.Dispatcher.InvokeAsync(() =>
                {
                    DG.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                    DG.RowEditEnding -= PAY_GETP_SUB_RowEditEnding;
                    DG.CancelEdit(DataGridEditingUnit.Cell);
                    DG.RowEditEnding += PAY_GETP_SUB_RowEditEnding;
                    DG.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                });
                return;
            }

            #region Form_BeforeInsert
            var rst = dbms.DoGetDataSQL<string?>("SELECT TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.TNUMBER) = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + " ) And ((TDETA_HES.NUMBER) = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") And ((TDETA_HES.N_KOL) = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + " )) GROUP BY TDETA_HES.NAME").ToList();
            if (rst.Count > 0)
            {
                FINAL_CROW_ITEM.NAME_TAH = rst.FirstOrDefault();
            }
            #endregion

            var N_SERI = FINAL_CROW_ITEM.N_SERI;
            var BANK = FINAL_CROW_ITEM.BANK;
            var DATE_S = FINAL_CROW_ITEM.DATE_S;


            long? ID = null;
            try
            {
                if (FINAL_CROW_ITEM?.ID == null) //INSERT
                {
                    ID = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.PAY_GETP 
                                                   (
                                                    N_SERI, 
                                                    BANK, 
                                                    DATE_S, 
                                                    DATE, 
                                                    SHOBEH, 
                                                    MABL, 
                                                    NAME_TAH, 
                                                    N_HESAB, 
                                                    N_KOL, 
                                                    N_MOIN, 
                                                    N_TAF, 
                                                    NUMBER, 
                                                    TAG, 
                                                    KIND, 
                                                    SAYADI ) 
                                                    OUTPUT INSERTED.ID 
                                                    VALUES (
                                                    {N_SERI},
                                                    {BANK},
                                                    {DATE_S},
                                                    {FINAL_CROW_ITEM.DATE},
                                                    N'{FINAL_CROW_ITEM.SHOBEH}',
                                                    {FINAL_CROW_ITEM.MABL},
                                                    N'{FINAL_CROW_ITEM.NAME_TAH}',
                                                    N'{FINAL_CROW_ITEM.N_HESAB}',
                                                    {(FINAL_CROW_ITEM.N_KOL is null ? "NULL" : FINAL_CROW_ITEM.N_KOL)},
                                                    {(FINAL_CROW_ITEM.N_MOIN is null ? "NULL" : FINAL_CROW_ITEM.N_MOIN)},
                                                    {(FINAL_CROW_ITEM.N_TAF is null ? "NULL" : FINAL_CROW_ITEM.N_TAF)},
                                                    {NUMBER.Text},
                                                    {HTAG},
                                                    {FINAL_CROW_ITEM.KIND},
                                                    N'{FINAL_CROW_ITEM.SAYADI}'
                                                )").FirstOrDefault();

                    //OUTPUT INSERTED.ID (place is exactly before 'VALUES')
                    if (ID != null)
                    {
                        FINAL_CROW_ITEM.ID = ID;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP
                                         SET N_SERI = {N_SERI}, BANK = {BANK}, DATE_S = {DATE_S}, DATE = {FINAL_CROW_ITEM.DATE},
                                         SHOBEH = N'{FINAL_CROW_ITEM.SHOBEH}',
                                         MABL = {FINAL_CROW_ITEM.MABL}, NAME_TAH = N'{FINAL_CROW_ITEM.NAME_TAH}',
                                         N_HESAB = N'{FINAL_CROW_ITEM.N_HESAB}',
                                         N_KOL = {(FINAL_CROW_ITEM.N_KOL is null ? "NULL" : FINAL_CROW_ITEM.N_KOL)},
                                         N_MOIN = {(FINAL_CROW_ITEM.N_MOIN is null ? "NULL" : FINAL_CROW_ITEM.N_MOIN)},
                                         N_TAF = {(FINAL_CROW_ITEM.N_TAF is null ? "NULL" : FINAL_CROW_ITEM.N_TAF)}, 
                                         NUMBER = {NUMBER.Text}, TAG = {HTAG},
                                         KIND = {FINAL_CROW_ITEM.KIND},
                                         SAYADI = N'{FINAL_CROW_ITEM.SAYADI}'
                                         WHERE ID = {FINAL_CROW_ITEM.ID}");
                }

            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " این چک تکراری است!!").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog();
                }
                //InvokeAsync
                DG.Dispatcher.Invoke(() =>
                {
                    DG.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                    DG.RowEditEnding -= PAY_GETP_SUB_RowEditEnding;
                    DG.CancelEdit();
                    DG.RowEditEnding += PAY_GETP_SUB_RowEditEnding;
                    DG.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                });
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ").ShowDialog();
                return;
            }

            TAKHFIF.Text = INVO_LST_FACTOR22_DATA.Sum(x => x.N_MOIN)?.ToString(); //جمع مبلغ تخفیف

            SANAD();

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

            if (PAY_GETP_SUB.Items.Count > 0 && PAY_GETP_SUB.SelectedItem != null)
            {
                if (!(PAY_GETP_SUB.SelectedItems is null))
                {
                    bool errors = default;
                    errors = (from object i in PAY_GETP_SUB.ItemsSource
                              let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                        var dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور برگشت فروش (آزاد) رسید شده => چک های پشت فاکتور",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 25",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        for (int i = 0; i < PAY_GETP_SUB.SelectedItems.Count; i++)
                        {
                            var item = PAY_GETP_SUB.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                if (CL_LMethods.IsNewPlaceHolder(PAY_GETP_SUB, item))
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                var _ID_ = item.GetType().GetProperty("ID").GetValue(item);

                                if (_ID_ != null)
                                {

                                    var THE_N_SERI = item.GetType().GetProperty("N_SERI").GetValue(item);
                                    var THE_BANK = item.GetType().GetProperty("BANK").GetValue(item);

                                    var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE  ID = " + _ID_).ToList();
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
                                                string _where = " WHERE  ID = " + _ID_;

                                                rst.FirstOrDefault().N_KOL = 911;
                                                rst.FirstOrDefault().N_MOIN = 1;
                                                rst.FirstOrDefault().N_TAF = 1;
                                                rst.FirstOrDefault().HES1 = "911-1-1";

                                                dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL = 911 , N_MOIN = 1 , N_TAF = 1 , HES1 = N'911-1-1' {_where} ");
                                                IsDeleteSomthing = true;
                                            }
                                        }
                                    }
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
                            PAY_GETP_SUB_SUB_ReGetData();
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

        #region Sayer
        class QRE_VISIT1
        {
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
        }
        public VISITOR_DTL _VISITOR_DTL_WAS_ROW_ITEM { get; set; }
        public void VISITOR_DTL_SUB_ReGetData()
        {
            //SAYER_VISITOR_DATA

            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                SAYER_VISITOR_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<VISITOR_DTL>($@"SELECT * FROM VISITOR_DTL WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        var CUSTDATA = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + item.CUST_NO + "'").FirstOrDefault();
                        item.CUST_NO_NAME = CUSTDATA.NAME;

                        if (item?.DARSAD != null)
                        {
                            item.DARSAD = (double)item?.DARSAD;
                        }

                        SAYER_VISITOR_DATA.Add(item);
                    }
                }
            }
            //SAYER_VISITOR_DATA.ItemsSource = PAY_GETD_SUB22_DATA;
            Text190.Text = SAYER_VISITOR_DATA.Sum(r => r.PURSANT).ToStringNullSafe();
        }
        public VISITOR_DTL CURRENT_ROW_VISITOR { get; set; }
        public long OKDATE { get; private set; }
        public long OKTIME { get; private set; }

        private void VISITOR_DTL_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid.CurrentCell.Column is null) return;

            if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null)
            {
                if (grid.CurrentCell.Item is VISITOR_DTL VisitorRow && VisitorRow != null)
                {
                    var Hes = VisitorRow.CUST_NO;
                    var Kol = CL_HESABDARI.GETKOL(Hes);
                    var Moin = CL_HESABDARI.GETMOIN(Hes);
                    var Taf = CL_HESABDARI.GETTAF(Hes);

                    double MAN;
                    if (CL_HESABDARI.BLOCKED(Kol, Moin, Taf))
                    {
                        new Msgwin(false, "حساب مورد نظر مسدود مي باشد!").ShowDialog();
                        return;
                    }

                    new F_MENU_KOL_MOIN_TAFZIL(Hes);
                }
            }
        }
        private void VISITOR_DTL_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (VISITOR_DTL_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    _VISITOR_DTL_WAS_ROW_ITEM = ((VISITOR_DTL)VISITOR_DTL_SUB.SelectedItem).Clone() as VISITOR_DTL;
                }
            }
        }
        private void VISITOR_DTL_SUB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VISITOR_DTL_SUB.IsKeyboardFocusWithin) { return; }

            IEditableCollectionView itemsView = VISITOR_DTL_SUB.Items as IEditableCollectionView;
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
        private void VISITOR_DTL_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (VISITOR_DTL_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = VISITOR_DTL_SUB.CurrentCell.Column.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_SAYER_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "PURSANT")
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
                if (CURRENT_COLUMN_NAME is "PURSANT")
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
        private void VISITOR_DTL_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_
            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            // = e.Column.SortMemberPath;
            var PAY_GETD_SUB22_ROW_INDEX = row_index;
            ComboBox Comboval = null;
            TextBox TexboVal = null;
            CheckBox CheckVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!(e.EditingElement is null))
            {
                CheckVal = e.EditingElement as CheckBox;
            }

            string? VISITOR_DTL_SUB_ENTERED_VALUE = null;
            if (!ReferenceEquals(Comboval, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = TexboVal.Text.Trim();

            CURRENT_ROW_VISITOR = e.Row.Item as VISITOR_DTL;
            #endregion

            if (e.Column.SortMemberPath == "CUST_NO_NAME") //CUST_NO_NAME == CUST_NO 112-1-1 محمدی دهقان تستی
            {
                #region CUST_NO_NotInList
                if (_VISITOR_DTL_WAS_ROW_ITEM.CUST_NO_NAME != VISITOR_DTL_SUB_ENTERED_VALUE) // نام مشتری جدید وارد شده
                {
                    if (VISITOR_DTL_SUB_ENTERED_VALUE == "+" || VISITOR_DTL_SUB_ENTERED_VALUE == "-")
                    {
                        ComboSearch CMBSearch = new ComboSearch("POSHTE_FACTOR_AZADF", I_AM_BARGASHT_NORMAL);//Search Plusy Form Specialy for Customers
                        CMBSearch.ShowDialog();

                        if (!string.IsNullOrEmpty(HESAB_POSHTEF_FROM_SEARCH.FULL_HES))
                        {
                            CURRENT_ROW_VISITOR.CUST_NO = HESAB_POSHTEF_FROM_SEARCH.FULL_HES;
                            CURRENT_ROW_VISITOR.CUST_NO_NAME = HESAB_POSHTEF_FROM_SEARCH.NAME;
                        }
                        else
                        {
                            new Msgwin(false, "حسابی انتخاب نشده!").ShowDialog();
                            return;
                        }
                        HESAB_POSHTEF_FROM_SEARCH.DoClear();
                    }
                    else
                    {

                        var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE HES = N'" + VISITOR_DTL_SUB_ENTERED_VALUE + "'").FirstOrDefault();
                        if (data is not null && !string.IsNullOrEmpty(data.hes))
                        {
                            CURRENT_ROW_VISITOR.CUST_NO = data.hes;
                            CURRENT_ROW_VISITOR.CUST_NO_NAME = data.NAME;
                        }
                        else
                        {
                            CURRENT_ROW_VISITOR.CUST_NO = null;
                            CURRENT_ROW_VISITOR.CUST_NO_NAME = null;
                            new Msgwin(false, "این حساب وجود ندارد!").ShowDialog();
                            return;
                        }
                    }

                    var tozihdata = dbms.DoGetDataSQL<string?>("SELECT  TOZIH FROM dbo.TDETA_HES WHERE RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) = N'" + CURRENT_ROW_VISITOR.CUST_NO + "'").ToList();
                    if (tozihdata.Count > 0)
                    {
                        if (Information.IsNumeric(tozihdata.FirstOrDefault()))
                        {
                            CURRENT_ROW_VISITOR.DARSAD = Convert.ToDouble(tozihdata.FirstOrDefault().Replace("%", ""));
                            CURRENT_ROW_VISITOR.PURSANT = Math.Round((double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * CURRENT_ROW_VISITOR.DARSAD / 100));
                            if ((bool)CURRENT_ROW_VISITOR.STAT)
                            {
                                CURRENT_ROW_VISITOR.STAT = false;
                            }
                        }
                    }
                }
                #endregion

                #region CUST_NO_AfterUpdate
                var rst = dbms.DoGetDataSQL<string?>("SELECT  TOZIH FROM dbo.CUST_HESAB WHERE     (hes = N'" + CURRENT_ROW_VISITOR.CUST_NO + "')").ToList();
                if (rst.Count > 0)
                {
                    if (Information.IsNumeric(rst.FirstOrDefault()))
                    {
                        CURRENT_ROW_VISITOR.DARSAD = Convert.ToDouble(rst.FirstOrDefault());
                        CURRENT_ROW_VISITOR.PURSANT = Math.Round((double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * CURRENT_ROW_VISITOR.DARSAD / 100));
                        if ((bool)CURRENT_ROW_VISITOR.STAT)
                        {
                            CURRENT_ROW_VISITOR.STAT = false;
                        }
                    }
                }
                #endregion
            }
            if (e.Column.SortMemberPath == "DARSAD")
            {
                var (isvalid, _) = CL_LMethods.IsValidPercentage(VISITOR_DTL_SUB_ENTERED_VALUE);
                if (!isvalid)
                {
                    CURRENT_ROW_VISITOR.DARSAD = null;
                    new Msgwin(false, "درصد صحیح نیست").ShowDialog();
                    return;
                }
                //DARSAD_AfterUpdate
                if (!IsNull(CURRENT_ROW_VISITOR.PORID))
                {
                    Msgwin msgwin = new Msgwin(true, "باتوجه به اينكه اين سطر دراي الگوي پرداخت پورسانت ميباشد با زدن درصد الگوي آن حذف ميشود آيا از ادامه عمليات اطمينان داريد.");
                    if (msgwin.DialogResult is true)
                    {
                        CURRENT_ROW_VISITOR.PORID = null;
                    }
                }
                CURRENT_ROW_VISITOR.PURSANT = Math.Round((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * Convert.ToDouble(VISITOR_DTL_SUB_ENTERED_VALUE) / 100);

            }
            if (e.Column.SortMemberPath == "PURSANT")
            {
                #region PURSANT_BeforeUpdate
                if (!IsNull(CURRENT_ROW_VISITOR.PORID))
                {
                    Msgwin msgwin1 = new Msgwin(true, "باتوجه به اينكه اين سطر دراي الگوي پرداخت پورسانت ميباشد با زدن مبلغ الگوي آن حذف ميشود آيا از ادامه عمليات اطمينان داريد.");
                    msgwin1.ShowDialog();
                    if (msgwin1.DialogResult is true)
                    {
                        CURRENT_ROW_VISITOR.PORID = null;
                    }
                }
                #endregion
            }

            if (e.Column.SortMemberPath == "PORID")
            {
                #region PORID_AfterUpdate
                //الگوي پرداخت پورسانت
                long prs;
                var MBK = default(long);
                prs = 0L;
                if (!IsNull(CURRENT_ROW_VISITOR?.PORID))
                {
                    var ROWS = dbms.DoGetDataSQL<QRE_VISIT1>("SELECT CODE ,MABL_K - N_MOIN AS MABLK FROM INVO_LST WHERE TAG = 24 AND NUMBER = " + NUMBER.Text).ToList();
                    for (int I = 0; I < ROWS.Count; I++) //while (!ROWS.EOF)
                    {
                        var RST2 = dbms.DoGetDataSQL<double?>("SELECT     PORSANT FROM dbo.VISITORS_PORSANT_KALA WHERE     (PORID = " + CURRENT_ROW_VISITOR.PORID + ") and (code = '" + ROWS[I].CODE + "')").ToList();
                        if (RST2.Count == 1)
                        {
                            prs = (long)(prs + Math.Round((double)(ROWS[I].MABLK * RST2.FirstOrDefault() / 100)));
                            MBK = (long)(MBK + ROWS[I].MABLK);
                        }
                        else
                        {
                            new Msgwin(false, "تذكر مهم :اين كالا فاقد الگو براي اين ويزيتور است و پورسانت محاسبه نشد.درصورت لزوم براي آن تعريف كنيد و همينجا مجددا الگو را انتخاب كنيد  : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(ROWS[I].CODE))).ShowDialog();
                        }
                    }
                    CURRENT_ROW_VISITOR.PURSANT = Math.Round((double)(prs));
                    if (MBK > 0L & prs > 0L)
                    {
                        CURRENT_ROW_VISITOR.DARSAD = CURRENT_ROW_VISITOR.PURSANT / MBK * 100;

                        CURRENT_ROW_VISITOR.DARSAD = (double)CURRENT_ROW_VISITOR.DARSAD;
                    }
                    else
                    {
                        CURRENT_ROW_VISITOR.DARSAD = 0;
                    }
                }
                #endregion
            }
        }
        private void VISITOR_DTL_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null) { return; }
            var FINAL_CROW_ITEM = (e.Row.Item as VISITOR_DTL);
            if (ConstructorRowDetector.IsPristine(FINAL_CROW_ITEM))
            {
                VISITOR_DTL_SUB.Dispatcher.Invoke(() =>
                {
                    VISITOR_DTL_SUB.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    VISITOR_DTL_SUB.RowEditEnding -= VISITOR_DTL_SUB_RowEditEnding;
                    VISITOR_DTL_SUB.CancelEdit();
                    VISITOR_DTL_SUB.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                    VISITOR_DTL_SUB.RowEditEnding += VISITOR_DTL_SUB_RowEditEnding;
                }); return;
            }

            #region Validations
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (string.IsNullOrEmpty(FINAL_CROW_ITEM.CUST_NO))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام شخص خالی است" });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.DARSAD?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.DARSAD?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد خالی است!" });
            }
            if (FINAL_CROW_ITEM.DARSAD < 0 || FINAL_CROW_ITEM.DARSAD > 100)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد باید بین 0 تا 100 باشد." });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.PURSANT?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.PURSANT?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ پورسانت صحیح نیست!" });
            }
            if (FINAL_CROW_ITEM.PURSANT < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ پورسانت منقی نمیتواند باشد!" });
            }

            if (FINAL_CROW_ITEM.TOZIH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول توضیح بیش از اندازه است!" });
            }
            if (FINAL_CROW_ITEM.STAT == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تیک مبلغ ثابت خالی است!" });
            }

            var DG = VISITOR_DTL_SUB;
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
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                DG.Dispatcher.InvokeAsync(() =>
                {
                    DG.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    DG.CancelEdit();
                    DG.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                });
                return;
            }
            #endregion


            long? _id_ = null;
            try
            {
                if (FINAL_CROW_ITEM?.ID is null)
                {

                    _id_ = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.VISITOR_DTL(NUMBER, TAG, CUST_NO, DARSAD, PURSANT, TOZIH, STAT, PORID)
                           OUTPUT INSERTED.ID
                           VALUES({NUMBER.Text},
                           {HTAG} ,
                           N'{FINAL_CROW_ITEM.CUST_NO}' ,
                           {FINAL_CROW_ITEM?.DARSAD} ,
                           {FINAL_CROW_ITEM?.PURSANT} ,
                           N'{FINAL_CROW_ITEM.TOZIH}' ,
                           {Convert.ToByte(FINAL_CROW_ITEM.STAT)},
                           {(string.IsNullOrEmpty(FINAL_CROW_ITEM?.PORID.ToStringNullSafe()) ? "NULL" : FINAL_CROW_ITEM?.PORID)})").FirstOrDefault();
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.VISITOR_DTL SET 
                                         NUMBER = {NUMBER.Text}, CUST_NO = N'{FINAL_CROW_ITEM.CUST_NO}' , DARSAD = {FINAL_CROW_ITEM?.DARSAD} ,
                                         PURSANT = {FINAL_CROW_ITEM?.PURSANT} , TOZIH = N'{FINAL_CROW_ITEM.TOZIH}' , STAT = {Convert.ToByte(FINAL_CROW_ITEM.STAT)},
                                         PORID = {(string.IsNullOrEmpty(FINAL_CROW_ITEM?.PORID.ToStringNullSafe()) ? "NULL" : FINAL_CROW_ITEM?.PORID)}
                                         WHERE ID = {FINAL_CROW_ITEM?.ID}");
                }
            }
            catch (SqlException ex)
            {
                VISITOR_DTL_SUB.Dispatcher.Invoke(() =>
                {
                    VISITOR_DTL_SUB.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    VISITOR_DTL_SUB.CancelEdit();
                    VISITOR_DTL_SUB.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                });

                if (ex.Number == 2627)
                {
                    new Msgwin(false, "سطر تکراری است آنرا اصلاح کنید").ShowDialog();
                    return;
                }
            }

            if (_id_ != null)
            {
                FINAL_CROW_ITEM.ID = _id_;
            }


            if (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text) + Convert.ToDouble(MBAA.Text) != 0)
            {
                FINAL_CROW_ITEM.DARSAD = FINAL_CROW_ITEM.PURSANT / (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * 100;

                FINAL_CROW_ITEM.DARSAD = (double)FINAL_CROW_ITEM.DARSAD;
            }
            else
            {
                FINAL_CROW_ITEM.DARSAD = 0;
            }

            double sum = SAYER_VISITOR_DATA.Sum(item => item.PURSANT ?? 0.0);
            Text190.Text = sum.ToString();

            SANAD();

            Text190.Text = SAYER_VISITOR_DATA.Sum(r => r.PURSANT).ToStringNullSafe();
        }
        private void DELETE_SAYER_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_SAYER.Visibility == Visibility.Visible;
            if (!DELETE_SAYER.IsEnabled || !IsVisible) { return; }


            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            //if (VISITOR_DTL_SUB.IsEditing()) return;

            if (VISITOR_DTL_SUB.Items.Count > 0 && VISITOR_DTL_SUB.SelectedItem != null)
            {
                if (!(VISITOR_DTL_SUB.SelectedItems is null))
                {
                    bool errors = default;

                    errors = (from object i in VISITOR_DTL_SUB.ItemsSource
                              let c = VISITOR_DTL_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                        var dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1);
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1);
                        CL_HESABDARI.TR("VISITOR_DTL", $"(TAG = {HTAG} and NUMBER = " + NUMBER.Text + ")", dt, 1);

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور برگشت فروش (آزاد) رسید شده => پورسانت ویزیتور ها",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 25",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < VISITOR_DTL_SUB.SelectedItems.Count; i++)
                        {
                            var item = VISITOR_DTL_SUB.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                var _CUST_NO = item.GetType().GetProperty("CUST_NO").GetValue(item);
                                var _DARSAD = item.GetType().GetProperty("DARSAD").GetValue(item);
                                var _PURSANT = item.GetType().GetProperty("PURSANT").GetValue(item);
                                var _TOZIH = item.GetType().GetProperty("TOZIH").GetValue(item);
                                var _STAT = item.GetType().GetProperty("STAT").GetValue(item);
                                var _PORID = item.GetType().GetProperty("PORID").GetValue(item);

                                _DARSAD = _DARSAD is null ? "NULL" : _DARSAD;
                                _PURSANT = _PURSANT is null ? "NULL" : _PURSANT;
                                _TOZIH = _TOZIH is null ? "NULL" : _TOZIH;
                                _STAT = _STAT is null ? "NULL" : _STAT;
                                _PORID = _PORID is null ? "NULL" : _PORID;

                                dbms.DoExecuteSQL($"DELETE FROM dbo.VISITOR_DTL WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} AND " +
                            $"CUST_NO = N'{_CUST_NO}' AND DARSAD = {_DARSAD} AND PURSANT = {_PURSANT} AND TOZIH = N'{_TOZIH}' AND STAT = {Convert.ToByte(_STAT)} AND PORID is {_PORID}");

                                IsDeleteSomthing = true;
                            }
                        }
                        if (IsDeleteSomthing is true)
                        {
                            VISITOR_DTL_SUB_ReGetData();

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
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            VISITOR_DTL_SUB.BeginEdit();
        }
        #endregion

        private void MABL_HAZ_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAZ.Text) != 0 && (IsNull(this.MOIN_HAZ.Text) || MOIN_HAZ.Text == "0"))
            {
                var RST = dbms.DoGetDataSQL<string?>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes FROM dbo.TDETA_HES WHERE (N_KOL = " + Baseknow.HKHARID + $") AND (NUMBER = {HTAG})").FirstOrDefault();
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
                CMB_HMBAA.SelectedValue = HMBAA.Text;
            }
        }
        private void TAKHFIF_PERCENT_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            return;
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
            return;
            TAKHFIF_MABL_PRICE(true);
        }
        private void TAKHFIF_MABL_PRICE(bool isTakhfifFocus = true, bool shouldDistributeToRows = true)
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

            if (Convert.ToDouble(TAKHFIF.Text) > 0 && NowIsReady && shouldDistributeToRows)
            {
                double jamf;
                int i = 0;
                double jammoin = 0;
                if (INVO_LST_FACTOR22_DATA.Count > 0 && SUM_OF_MABL_K != null)
                {
                    jamf = SUM_OF_MABL_K;
                }
                else
                {
                    jamf = 0;
                }
                foreach (var item in INVO_LST_FACTOR22_DATA)
                {
                    i++;
                    if (i == INVO_LST_FACTOR22_DATA.Count)
                    {
                        item.N_MOIN = Convert.ToDouble(TAKHFIF.Text) - jammoin;
                        if (item.MABL_K != 0)
                        {
                            item.N_KOL = (Convert.ToDouble(TAKHFIF.Text) - jammoin) / item.MABL_K * 100;
                        }
                        else
                        {
                            item.N_KOL = 0;
                        }
                    }
                    else
                    {
                        item.N_MOIN = Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF.Text)));
                        if (item.MABL_K != 0)
                        {
                            item.N_KOL = Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF.Text))) / item.MABL_K * 100; //Here was the bug fixed becuase of 100
                        }
                        else
                        {
                            item.N_KOL = 0;
                        }
                    }
                    jammoin += Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF.Text)));

                    dbms.DoExecuteSQL($@"UPDATE INVO_LST SET N_MOIN = {item.N_MOIN},
                                                             N_KOL = {item.N_KOL}
                                                             WHERE id = {item.id} AND TAG = 24");
                }


                INVO_LST_SUB_ReGetData();
            }
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

            if (string.IsNullOrWhiteSpace(NUMBER1.Text) || NUMBER1.Text == "0")
            {
                return;
            }

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_BRFR.mrt");
            report.Load(pathreport);


            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            var THEHEADERNUM = NUMBER.Text;
            double JCHK = 0, JAMF = 0, HAZ = 0, NAGHD = 0, VAR = 0, HAV = 0, taf = 0, MBAA = 0;
            // Fetch check data
            var rst = dbms.DoGetDataSQL<CheckData>($@"
                                                      SELECT PAY_GETP.N_SERI, TCOD_BANKS.NAMES, PAY_GETP.SHOBEH, PAY_GETP.DATE, PAY_GETP.DATE_S, 
                                                      PAY_GETP.MABL, PAY_GETP.NUMBER, PAY_GETP.TAG 
                                                      FROM TCOD_BANKS 
                                                      INNER JOIN PAY_GETP ON TCOD_BANKS.CODE = PAY_GETP.BANK 
                                                      WHERE PAY_GETP.NUMBER = {NUMBER.Text} AND PAY_GETP.TAG = {HTAG} AND (N_KOL IS NULL OR N_KOL <> 911)").ToList();

            if (rst.Any())
            {
                string commCaption = $"چكهاي پرداخت شده {rst.Count} فقره جمعاًبه مبلغ : {NCHK.Text:### ريال}  ";
                foreach (var check in rst)
                {
                    commCaption += $"ـ سريال:{check.N_SERI} بانك:{check.NAMES} شعبه:{check.SHOBEH.Trim()}";
                    JCHK += Convert.ToDouble(check.MABL);
                }
                (report.GetComponentByName("COMM") as StiText).Text = commCaption;
            }

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
                                                    WHERE INVO_LST.NUMBER = {THEHEADERNUM} AND INVO_LST.TAG = {FTAG}").FirstOrDefault();

            JAMF = jst ?? 0;

            // Fetch HEAD_LST data
            var headLst = dbms.DoGetDataSQL<HeadLstData>($@"
                                                           SELECT NUMBER, TAG AS htag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, 
                                                           M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, 
                                                           MOIN_KHF, ANBARF, FNUMCO, MBAA 
                                                           FROM HEAD_LST 
                                                           WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();

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
            if (HKH.Text != "0")
            {
                (report.GetComponentByName("HKH") as StiText).Text = Convert.ToDouble(HKH.Text).ToString("#,##0;#,##0-");
            }
            (report.GetComponentByName("MBAA") as StiText).Text = INVO_LST_FACTOR22_DATA.Sum(i => i.IMBAA).ToString();

            (report.GetComponentByName("TF") as StiText).Text = NTKHFIF.Text;
            (report.GetComponentByName("GABEL") as StiText).Text = GHABEL.Text;
            (report.GetComponentByName("JPAY") as StiText).Text = NPAR.Text;
            (report.GetComponentByName("MAN") as StiText).Text = MAN.Text;


            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(MAN.Text));


            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked)
            {
                (report.GetComponentByName("FIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
                (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN2.IsChecked)
            {
                (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER;
                (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN3.IsChecked)
            {
                (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER;
                (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER;
            }

            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            //(report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            (report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            //(report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "فاکتور برگشت فروش (آزاد) رسید شده").Show();
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
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.HAVLAH_ENTER_BACK.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
                                                                                   //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            (report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;


            new Rpts.WINRPT(report, "رسید انبار برگشت از فروش (آزاد)").Show();

        }
        private void Command108_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }

            if (string.IsNullOrWhiteSpace(NUMBER1.Text) || NUMBER1.Text == "0")
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FRBK_3_BMA.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FactorMBA"]).CommandTimeout = 900;

            double JCHK = 0, JAMF = 0, HAZ = 0, NAGHD = 0, VAR = 0, HAV = 0, taf = 0, MBAA = 0;



            // Calculate JAMF
            var jst = dbms.DoGetDataSQL<double?>($@"
                                                    SELECT SUM(INVO_LST.MABL_K) AS SumOfMABL_K 
                                                    FROM INVO_LST 
                                                    WHERE INVO_LST.NUMBER = {NUMBER.Text} AND INVO_LST.TAG = {HTAG}").FirstOrDefault();

            JAMF = jst ?? 0;

            // Fetch HEAD_LST data
            var headLst = dbms.DoGetDataSQL<HeadLstData>($@"
                                                           SELECT NUMBER, TAG AS htag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, 
                                                           M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, 
                                                           MOIN_KHF, ANBARF, FNUMCO, MBAA 
                                                           FROM HEAD_LST 
                                                           WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();

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
            //(report.GetComponentByName("JF") as StiText).Text = JAMF.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("MBAA") as StiText).Text = MBAA.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("TF") as StiText).Text = taf.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("GABEL") as StiText).Text = (JAMF + HAZ - taf + MBAA).ToString("#,##0;#,##0-");
            //(report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString("#,##0;#,##0-");
            //(report.GetComponentByName("MAN") as StiText).Text = (JAMF + HAZ + MBAA - (NAGHD + VAR + HAV + JCHK + taf)).ToString("#,##0;#,##0-");


            //(report.GetComponentByName("HR") as StiText).Text = $"{CL_HESABDARI.ALPHANUM(JAMF + HAZ - taf + MBAA)} ريال";
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(JAMF));


            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked)
            {
                if (report.GetComponentByName("FIMG") is StiImage sti1) sti1.Enabled = true;

                if (report.GetComponentByName("FS") is StiText sti2) sti2.Text = SGN1_INFO.SEMAT_USER;
                if (report.GetComponentByName("FU") is StiText sti3) sti3.Text = SGN1_INFO.NAME_HESAB_USER;

                //(report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
                //(report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;

            }
            if ((bool)SGN2.IsChecked)
            {
                if (report.GetComponentByName("HIMG") is StiImage sti1) sti1.Enabled = true;

                if (report.GetComponentByName("HS") is StiText sti3) sti3.Text = SGN2_INFO.SEMAT_USER;
                if (report.GetComponentByName("HU") is StiText sti4) sti4.Text = SGN2_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN3.IsChecked)
            {
                if (report.GetComponentByName("MIMG") is StiImage sti1) sti1.Enabled = true;


                if (report.GetComponentByName("MS") is StiText sti3) sti3.Text = SGN3_INFO.SEMAT_USER;
                if (report.GetComponentByName("MU") is StiText sti4) sti4.Text = SGN3_INFO.NAME_HESAB_USER;
            }

            //(report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;
            if (report.GetComponentByName("TEXT_HESAB") is StiText stiText1) stiText1.Text = CUST_NO.Text;


            //JF جمع مبلغ
            if (report.GetComponentByName("JF") is StiText stiText11) stiText11.Text = JF.Text;

            //NTKHFIF تخفیف
            if (report.GetComponentByName("NTKHFIF") is StiText stiText2) stiText2.Text = NTKHFIF.Text;

            //MBAA ارزش افزوده
            var _MBAA_ = INVO_LST_FACTOR22_DATA.Sum(i => i.IMBAA);
            if (report.GetComponentByName("MBAA") is StiText stiText3) stiText3.Text = _MBAA_.ToStringNullSafe();

            //GHABEL قابل پرداخت
            if (report.GetComponentByName("GHABEL") is StiText stiText4) stiText4.Text = GHABEL.Text;


            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "فاکتور برگشت فروش (آزاد) رسید شده").Show();
        }
        private void NUMBER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NUMBER.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (string.IsNullOrWhiteSpace(NUMBER.Text.Trim()))
            {
                e.Handled = true;
                NUMBER.Text = NUMBER1_TAG?.ToString();
                universControl.PopNotifyShow("چنین شماره وجود ندارد!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            string title = "سایر رسید انبار";
            if (_navigationManager.IsNewRecord)
            {
                var selected = NUMBER.Text;
                // Check if this NUMBER1 is already used and get the NUMBER of the record that uses it.
                var existingRecordNumber = dbms.DoGetDataSQL<double?>($"SELECT NUMBER FROM HEAD_LST WHERE TAG = {HTAG} AND NUMBER1 = {selected}").FirstOrDefault(); //HTAG
                if (existingRecordNumber != null)
                {
                    // A record with this NUMBER1 already exists. Inform the user and navigate to it.
                    new Msgwin(false, $"این {title} قبلاً برای فاکتور شماره {existingRecordNumber} استفاده شده است. در حال بارگذاری آن برگه...").ShowDialog();

                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER == existingRecordNumber);
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx >= 0)
                        {
                            _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                        }
                    }
                    else
                    {
                        // The record exists in DB but not in the current navigation list (perhaps due to filters).
                        new Msgwin(false, $"فاکتور شماره {existingRecordNumber} در لیست فعلی شما یافت نشد، ممکن است به آن دسترسی نداشته باشید.").ShowDialog();
                    }

                    // Reset the selection to the previous valid one.
                    NUMBER.SelectedValue = NUMBER1_TAG;
                    NUMBER.Text = NUMBER1_TAG.ToString();
                    NUMBER.Items.Refresh();
                    e.Handled = true; // Prevent further focus changes as we are navigating away.
                    return;
                }
            }
            else
            {
                if (Convert.ToDouble(NUMBER.Text) != NUMBER1_TAG)
                {
                    new Msgwin(false, $"نمیتوانید {title} ی که قبلا ثبت کرده اید را تغییر دهید , تنها میتوانید این فاکتور را حذف نمایید , انتخاب سایر رسید انبار تنها در فاکتور جدید ممکن است").ShowDialog();
                    NUMBER.SelectedValue = NUMBER1_TAG; NUMBER.Items.Refresh();
                    NUMBER.Text = NUMBER1_TAG.ToString();
                    return;
                }
            }

            double? SumOfMEGH_MAR = null;
            bool BargashtExistBefore = false;

            SumOfMEGH_MAR = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER.Text + $" ) AND ((INVO_LST.TAG)={HTAG}))").FirstOrDefault();
            //NUMBER =" + NUMBER.Text این درسته , اینجا توی سی شارپ اشتباه شده اسم NUMBER1 در واقع باید باشه NUMBER ولی خب مشکلی نیست صرفا ظاهر اسم با اکسس فرق داره
            var _NUMBER_ = dbms.DoGetDataSQL<double?>($"SELECT NUMBER FROM HEAD_LST WHERE TAG = {FTAG} AND NUMBER =" + NUMBER.Text).FirstOrDefault();  //این خط اوکیه
            if (_NUMBER_ > 0)
            {
                BargashtExistBefore = true;
            }

            if (NUMBER1_TAG > 0 && _NUMBER_ != NUMBER1_TAG)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از تغییر شماره حواله انبار مطمئن هستید"); msgwin.ShowDialog();
                if (msgwin.DialogResult == false) //NO
                {
                    NUMBER.SelectedValue = NUMBER1_TAG; NUMBER.Items.Refresh();
                    NUMBER.Text = NUMBER1_TAG.ToString(); return;
                }
            }

            if ((SumOfMEGH_MAR > 0 || BargashtExistBefore) && _navigationManager.IsNewRecord)
            {
                new Msgwin(false, "اين فاكتور داراي اطلاعات مي باشديا اينكه قبلا مرجوعي آن ثبت شده. براي حذف فاكتور بايد كليه رديفهاي ستونهاي تعداد مرجوعي صفر باشد").ShowDialog();
                NUMBER.SelectedValue = NUMBER1_TAG; NUMBER.Items.Refresh();
                NUMBER.Text = NUMBER1_TAG.ToString();
                return;
            }
            else if (_NUMBER_ != NUMBER1_TAG) //آیا شماره فاکتور مرجع تغییر کرده ؟!
            {
                new Msgwin(false, "اين فاكتور داراي اطلاعات مي باشديا اينكه قبلا مرجوعي آن ثبت شده. براي حذف فاكتور بايد كليه رديفهاي ستونهاي تعداد مرجوعي صفر باشد").ShowDialog();
                NUMBER.SelectedValue = NUMBER1_TAG; NUMBER.Items.Refresh();
                NUMBER.Text = NUMBER1_TAG.ToString();
                return;
            }
            else //IsSuccessfully
            {
                NUMBER1_TAG = Convert.ToDouble(NUMBER.Text);
                ReGetDataMaster(true);
                ReGetDataAll();

                //BTN_SAVE_Click(null, null);
            }


        }
        private void ReGetDataAll()
        {
            INVO_LST_SUB_ReGetData();
            PAY_GETP_SUB_SUB_ReGetData();
            VISITOR_DTL_SUB_ReGetData();
        }

        private void NavigateToExistingWarehouseReceipt(object selectedNumber)
        {
            if (_navigationManager == null || selectedNumber == null)
            {
                return;
            }

            if (!double.TryParse(selectedNumber.ToString(), out double parsedNumber))
            {
                return;
            }

            var match = _navigationManager.RecordsData
                .Select((record, index) => new { record, index })
                .FirstOrDefault(x => x.record != null &&
                                     (x.record.NUMBER == parsedNumber ||
                                      (x.record.NUMBER1.HasValue && x.record.NUMBER1.Value == parsedNumber)));

            if (match != null)
            {
                _navigationManager.IsNewRecord = false;
                _navigationManager.MoveReGetData(Jahat.CustomPosition, match.index);
            }
        }

        private void TICMBAA_Click(object sender, RoutedEventArgs e)
        {
            BTN_SAVE_Click(null, null);
            if (!isSavedSuccess)
            {
                return;
            }

            CalculateIMBAA();
            BTN_SAVE_Click(null, null);
            INVO_LST_SUB_ReGetData();
        }

        public void CalculateIMBAA()
        {
            double smbaa = 0d;
            var number = this.NUMBER.Text;
            const int tag = 24;

            if (_navigationManager.IsNewRecord)
            {
                return;
            }

            var invoiceList = dbms.DoGetDataSQL<INVO_LST_CSHARP>(
                "SELECT * FROM INVO_LST WHERE NUMBER = @NUMBER AND TAG = @TAG",
                new { NUMBER = number, TAG = tag }).ToList();

            if (TICMBAA.IsChecked == true)
            {
                double.TryParse(MBAA.Text, out var currentMbaa);

                var codes = invoiceList.Select(x => x.CODE).Distinct().ToList();
                if (!codes.Any())
                {
                    ClearTaxFields();
                    this.HMBAA.IsReadOnly = true;
                    this.MBAA.IsReadOnly = true;
                    CMB_HMBAA.IsEnabled = false;
                    return;
                }

                var cmbaaMap = dbms.DoGetDataSQL<HLF2>(
                    "SELECT CMBAA, CODE FROM STUF_DEF WHERE CODE IN @Codes",
                    new { Codes = codes }).ToDictionary(x => x.CODE, x => x.CMBAA);

                foreach (var row in invoiceList)
                {
                    bool cmbaa = cmbaaMap.TryGetValue(row.CODE, out var cmbaaObj) && Convert.ToBoolean(cmbaaObj);
                    if (cmbaa)
                    {
                        row.IMBAA = Math.Round((double)((row.MABL_K - row.N_MOIN) * CL_HESABDARI.GetArzesh(row.CODE) / 100));
                        smbaa += Convert.ToDouble(row.IMBAA);
                    }
                    else
                    {
                        row.IMBAA = 0;
                    }

                    var itemInCollection = INVO_LST_FACTOR22_DATA.FirstOrDefault(x => x.id == row.id);
                    if (itemInCollection != null)
                    {
                        itemInCollection.IMBAA = row.IMBAA;
                    }
                }

                foreach (var row in invoiceList)
                {
                    dbms.DoExecuteSQL(
                        "UPDATE dbo.INVO_LST SET IMBAA = @IMBAA WHERE NUMBER = @NUMBER AND TAG = @TAG AND id = @ID",
                        new { IMBAA = row.IMBAA, NUMBER = number, TAG = tag, ID = row.id });
                }

                if (smbaa > 0d)
                {
                    if (smbaa != currentMbaa)
                    {
                        this.MBAA.Text = smbaa.ToString();
                    }

                    this.HMBAA.Text = Baseknow.HESMBAA;
                    this.CMB_HMBAA.SelectedValue = this.HMBAA.Text;
                }
                else
                {
                    ClearTaxFields();
                }
            }
            else
            {
                foreach (var row in invoiceList)
                {
                    row.IMBAA = 0;

                    var itemInCollection = INVO_LST_FACTOR22_DATA.FirstOrDefault(x => x.id == row.id);
                    if (itemInCollection != null)
                    {
                        itemInCollection.IMBAA = 0;
                    }

                    dbms.DoExecuteSQL(
                        "UPDATE dbo.INVO_LST SET IMBAA = 0 WHERE NUMBER = @NUMBER AND TAG = @TAG AND id = @ID",
                        new { NUMBER = number, TAG = tag, ID = row.id });
                }

                if (Convert.ToDouble(MBAA.Text) > 0)
                {
                    this.MBAA.Text = "0";
                    this.HMBAA.Text = null;
                }

                ClearTaxFields();
            }

            this.HMBAA.IsReadOnly = TICMBAA.IsChecked == true;
            this.MBAA.IsReadOnly = TICMBAA.IsChecked == true;
            CMB_HMBAA.IsEnabled = TICMBAA.IsChecked != true;
        }

        private void ClearTaxFields()
        {
            MBAA.Text = "0"; //مبلغ مالیات
            HMBAA.Text = null; //معین مالیات

            if (CMB_HMBAA is not null)
            {
                CMB_HMBAA.SelectedIndex = -1;
                CMB_HMBAA.SelectedValue = null;
                CMB_HMBAA.Text = null;
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


        private void N_S_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
            {
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), false);
            }
        }

        private void Label_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //اعلامیه قیمت
            if (PEPID.SelectedItem is PRICELIST_CSHARP SelectedPEPIDValue)
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_GHEYMAT, this, SelectedPEPIDValue.PEPID);
            }
        }

        private void Label_PreviewMouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (PEID.SelectedItem is PRICELIST_ETF_TAKHFIF__CSHARP SelectedPEIDValue)
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_TAKHFIF, this, SelectedPEIDValue.PEID);
            }
        }
    }
}
