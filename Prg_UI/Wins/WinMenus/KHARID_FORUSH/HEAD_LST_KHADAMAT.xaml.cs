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
using static Wins.WinMenus.ANBAR.HEAD_LST_ENTEGHAL_WIN;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using Custom_VAHEDK = Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL.Custom_VAHEDK;
using Rpts;
using System.Windows.Controls.Primitives;

namespace Wins.WinMenus.KHARID_FORUSH
{
    /// <summary>
    /// Interaction logic for HEAD_LST_KHADAMAT.xaml
    /// </summary>
    public partial class HEAD_LST_KHADAMAT : Window
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
        public class DeedHedData
        {
            public string BASE { get; set; }
            public bool GHATEI { get; set; }
        }
        public class SignData
        {
            public bool KFR_BAZAR { get; set; }
            public bool KFR_HESAB { get; set; }
            public bool KFR_MODIR { get; set; }
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
        public class KHAD_QRE_MODEL1
        {
            public double? VAS { get; set; }
            public bool? TICMBAA { get; set; }
            public double? MaxOfNUMBER { get; set; }
        }
        #endregion
        public HEAD_LST_KHADAMAT(double? number_to_open = null, string _OpenArg_ = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER.Text = number_to_open.ToString(); //شماره رسید
                NUMBER.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;
            }

            if (!string.IsNullOrEmpty(_OpenArg_))
            {
                OpenArgs = _OpenArg_;
            }

        }
        public bool IsOpenedFromAutomation { get; } = false;
        public string OpenArgs { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله
        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<PAY_GETD_SUB22_MODEL> PAY_GETD_SUB22_DATA { get; set; } = new ObservableCollection<PAY_GETD_SUB22_MODEL>();
        public ObservableCollection<TAKHFIF_APLAY> TAKHFIF_APLAY_DATA { get; set; } = new ObservableCollection<TAKHFIF_APLAY>();

        /// <summary>
        /// TAG = 14
        /// </summary>
        public byte HTAG { get; } = 14; //برگه رسید

        public int? ANBAR { get; set; }

        private double _sum_of_mabl_k = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _sum_of_mabl_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MABL_K);
                if (_sum_of_mabl_k == 0) _sum_of_mabl_k = 0;
                return _sum_of_mabl_k;
            }
            set { _sum_of_mabl_k = value; }
        }

        private double _sim_of_n_moin = 0;
        public double SUM_OF_N_MOIN
        {
            get
            {
                _sim_of_n_moin = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.N_MOIN);
                return _sim_of_n_moin;
            }
            set { _sim_of_n_moin = value; }
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

        List<COMBOPERSONEL> rst_personel = null;
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
                MAS.IsReadOnly = !ican;// مدت
                NUMBER.IsReadOnly = !ican;// شماره حواله
                CUST_KIND.IsReadOnly = !ican;// نوع مشتری
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ
                SHIFT.IsReadOnly = !ican;// شیفت
                SPER.IsReadOnly = !ican;
                MABL_HAZ_FRONT.IsReadOnly = !ican;


                //__ENABLEY
                DEPATMAN.IsEnabled = ican;
                CMB_MOIN_HAZ_FRONT.IsEnabled = ican;
                DATE_N.IsEnabled = ican;// تاریخ
                MAS.IsEnabled = ican;// مدت
                NUMBER.IsEnabled = ican;// شماره حواله
                CUST_KIND.IsEnabled = ican;// نوع مشتری
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ
                SHIFT.IsEnabled = ican;// شیفت
                                       //فاکتور END

                Page58.IsEnabled = ican;// تب پشت فاکتور
                TICMBAA.IsEnabled = ican;
                BTN_SAVE.IsEnabled = ican;
            }
        }

        public double Meidnum { get; private set; }
        public int ANBARDefaultValue { get; private set; }
        public Visual I_AM_KHADAMAT { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_KHADAMAT = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            if (!string.IsNullOrEmpty(NUMBER.Text))
            {
                if (Convert.ToDouble(NUMBER.Text) > 0)
                {
                    var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();

                    if (HEADER_FAC == null)
                    {
                        new Msgwin(false, "چنین شماره ای وجود ندارد !").ShowDialog();
                        this.Close();
                        return;
                    }

                    DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                    USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر
                    MAS.Text = HEADER_FAC.MAS.ToStringNullSafe(); //مدت
                    DEPATMAN.SelectedValue = HEADER_FAC.DEPATMAN; DEPATMAN.Items.Refresh(); //واحد
                    CUST_KIND.SelectedValue = HEADER_FAC.CUST_KIND; CUST_KIND.Items.Refresh(); //نوع مشتری

                    string thevalue = HEADER_FAC.CUST_NO;
                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();

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
                    TAKHFIF_PERCENT.Text = "0"; //Reset درصد تخفیف برای جلوگیری از تداخل و محاسبه اشتباه

                    OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور
                    TICMBAA.IsChecked = HEADER_FAC.TICMBAA; //تایید فاکتور

                    MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات
                    SHIFT.SelectedValue = HEADER_FAC.SHIFT; //شیفت

                    M_NAGHD.Text = HEADER_FAC.M_NAGHD.ToStringNullSafe(); //مبلغ نقد
                    MABL_VAR.Text = HEADER_FAC.MABL_VAR.ToStringNullSafe(); //مبلغ کارت بانک
                    MABL_HAV.Text = HEADER_FAC.MABL_HAV.ToStringNullSafe(); //مبلغ بن یا حواله
                    TAKHFIF.Text = HEADER_FAC.TAKHFIF.ToStringNullSafe(); //مبلغ تخفیف

                    MOIN_VAR.Text = HEADER_FAC.MOIN_VAR.ToStringNullSafe(); //معین کارت
                    MOIN_HAV.Text = HEADER_FAC.MOIN_HAV.ToStringNullSafe(); //معین بن

                    //پشت فاکتور
                    //TAKHFIF.Text; //مبلغ تخفیف
                    MABL_HAZ.Text = (string.IsNullOrEmpty(HEADER_FAC.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER_FAC.MABL_HAZ.ToStringNullSafe()); //مبلغ خدمات
                    MOIN_HAZ.Text = HEADER_FAC.MOIN_HAZ; //معین خدمات
                    MBAA.Text = HEADER_FAC.MBAA.ToStringNullSafe(); //مالیات و عوارض مبلغ
                    HMBAA.Text = HEADER_FAC.HMBAA; //معین مالیات

                    BTN_SAVE.IsEnabled = false;

                    INVO_LST_SUB_ReGetData();
                    PAY_GETD_SUB_ReGetData();

                    GetBalancePerson();

                    TAKHFIF_MABL_PRICE();

                }

            }


            #region From_Load
            //Form_Load
            //#Error Check Matter
            //if (Baseknow.UGRP == "1")
            //{
            //    this.ServerFilter = "TAG = 14";
            //}
            //else
            //{
            //    this.ServerFilter = $"(TAG = {HTAG}) AND (depatman = " + TFSAZMAN + ") AND (USER_NAME = '" + UCurrentUser() + "')";
            //}
            //if (!IsNull(this.OpenArgs))
            //{
            //    if (this.OpenArgs == "FNUMCO")
            //    {
            //        this.ServerFilter = this.ServerFilter + " AND  FNUMCO = " + Forms["PTAMIR"].Form.PIDT;
            //    }
            //    else
            //    {
            //        this.ServerFilter = this.ServerFilter + " AND  NUMBER = " + this.OpenArgs;
            //    }
            //}
            #endregion

            #region Form_Open
            //Form_Open
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                this.OKF.IsChecked = true;
            }
            else
            {
                this.OKF.IsChecked = false;
            }
            if ((bool)Baseknow.UPDDATE)
            {
                this.DATE_N.IsReadOnly = false; //.Locked = false;
            }
            else
            {
                this.DATE_N.IsReadOnly = true;
            }
            if (!CL_HESABDARI.LETSGO("CUSTEN"))
            {
                this.CUST_KIND.IsReadOnly = true;
            }
            else
            {
                this.CUST_KIND.IsReadOnly = false;
            }
            if (CL_HESABDARI.LETSGO("ESLAHKHAD"))
            {
                this.ESLAH.Visibility = Visibility.Visible;
            }
            else
            {
                this.ESLAH.Visibility = Visibility.Hidden;
            }
            if (!CL_HESABDARI.LETSGO("CUSTEN"))
            {
                this.CUST_KIND.IsReadOnly = true;
            }
            else
            {
                this.CUST_KIND.IsReadOnly = false;
            }
            if (!CL_HESABDARI.LETSGO("BFAC"))
            {
                this.Page58.Visibility = Visibility.Hidden;
            }

            if (!CL_HESABDARI.LETSGO("TKHPISH"))
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
            }
            else
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Visible;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 58, 1) != "5")
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
            }
            #endregion

            #region Form_Open_Sub
            //Form_Open
            if ((bool)Baseknow.FRUP)
            {
                MABL_COLUMN.IsReadOnly = true;
                MABL_K_COLUMN.IsReadOnly = true;
            }
            else
            {
                MABL_COLUMN.IsReadOnly = false;
                MABL_K_COLUMN.IsReadOnly = false;
            }
            #endregion

            if (!NewRecord)
            {
                Form_Current();
            }

            LastTICMBAAChecked = (bool)TICMBAA.IsChecked;

            CUST_NO.Focus();

        }

        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                INVO_LST_SUB.IsEnabled = false;
            }
            else
            {
                var WasDataGridEnbaled = INVO_LST_SUB.IsEnabled;

                INVO_LST_SUB.IsEnabled = true;


                if (!WasDataGridEnbaled)
                {
                    INVO_LST_SUB.Focus();
                    var DEFINDX = (INVO_LST_SUB.SelectedIndex < 0) ? 0 : INVO_LST_SUB.SelectedIndex;
                    CL_LMethods.FocusCellReadyToEdit(INVO_LST_SUB, "ANBAR", DEFINDX, true);

                    //Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    INVO_LST_SUB.BeginEdit();
                    //}), DispatcherPriority.Background);
                }

            }

            SecurityAllCheck();
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

        private void ClearFreshAll()
        {
            NUMBER.Text = "0"; //شماره فاکتور

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();

            MAS.Text = "0"; //مدت

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER; SHIFT.Items.Refresh(); //شیفت این کاربر
            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 

            OKF.IsChecked = false; //تایید فاکتور
            TICMBAA.IsChecked = false; //مالیات ب.ا.ا

            JJKOL.Text = "0"; //جمع فاکتور

            MANDAH.Text = null;
            N_S.Text = "0"; //ثبت در سند

            //پشت فاکتور
            M_NAGHD.Text = "0"; //مبلغ نقد
            MABL_VAR.Text = "0"; //مبلغ واریزی

            MOIN_VAR.Text = null; //معین واریزی
            MABL_HAV.Text = "0"; //مبلغ حواله
            MOIN_HAV.Text = null; //معین حواله

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

            Form_Current();
        }

        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "FKHAD", new WindowInteropHelper(this).Handle, this.GetType().Name);

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            //Call SETSECURITYSUB("HEAD_LST_KHADAMAT", Me.NAME, "FKHAD", 3)
        }
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) AND (CODE = 0) ORDER BY TCOD_ANBAR.CODE";

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
            DEPATMAN.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            DEPATMAN.DisplayMemberPath = "DEPNAME";
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
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL=0)").ToList();
            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);



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
        public void TAKHFIF_APLAY_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //Erro Check Matter مشکل اینجا کوئری و نمایش داده های کمبوباکس هست که نجوه jion جدول درست نیست 
                var QRE_LST = dbms.DoGetDataSQL<TAKHFIF_APLAY>($@"SELECT dbo.TAKHFIF_APLAY.TID, dbo.TAKHFIF_APLAY.NUMBER, dbo.TAKHFIF_APLAY.KIND, dbo.TAKHFIF_DEF.TSHARH
                                                              FROM dbo.TAKHFIF_APLAY
                                                                   RIGHT OUTER JOIN dbo.TAKHFIF_DEF ON dbo.TAKHFIF_APLAY.TID=dbo.TAKHFIF_DEF.TID
                                                              WHERE (dbo.TAKHFIF_APLAY.NUMBER={NUMBER.Text}) ").ToList();

                //ComboBox:
                Combo6Column.ItemsSource = dbms.DoGetDataSQL<TAKHFIF_DEF>("SELECT TID, TSHARH FROM TAKHFIF_DEF").ToList();

                TAKHFIF_APLAY_DATA?.Clear();
                foreach (var item in QRE_LST)
                    TAKHFIF_APLAY_DATA.Add(item);

                TAKHFIF_APLAY_SUB.ItemsSource = TAKHFIF_APLAY_DATA;
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
        public void Form_Current()
        {
            bool ghat = false;
            if (SUM_OF_MABL_K != 0)
            {
                this.SPER.Text = Convert.ToString(Convert.ToDouble(MABL_HAZ.Text) / (SUM_OF_MABL_K / 100));
            }
            else
            {
                this.SPER.Text = "0";
            }

            if (Baseknow.TKHF == 1)
            {
                this.TAKHFIF.IsReadOnly = false; //Locked = false;
                this.TAKHFIF_PERCENT.IsReadOnly = false;
            }
            else
            {
                this.TAKHFIF.IsReadOnly = true;
                this.TAKHFIF_PERCENT.IsReadOnly = true;
            }
            if (!NewRecord)
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
                this.INVO_LST_SUB.IsEnabled = true;
                this.Page58.IsEnabled = true;
            }
            else
            {
                var rst = dbms.DoGetDataSQL<DeedHedData>($"SELECT BASE, GHATEI FROM DEED_HED WHERE N_S = {N_S.Text}").FirstOrDefault();
                if (rst != null)
                {
                    if (rst.GHATEI)
                    {
                        ghat = true;
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        this.INVO_LST_SUB.IsEnabled = false;
                        this.Page58.IsEnabled = false;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        this.INVO_LST_SUB.IsEnabled = true;
                        this.Page58.IsEnabled = true;
                    }
                }
            }
            if (this.NewRecord)
            {
                this.Page58.IsEnabled = false;
                this.INVO_LST_SUB.IsEnabled = false;
            }
            else
            {
                if (!ghat)
                {
                    this.INVO_LST_SUB.IsEnabled = true;
                    this.Page58.IsEnabled = true;
                }
                else
                {
                    this.Page58.IsEnabled = false;
                    this.INVO_LST_SUB.IsEnabled = false;
                }
            }
            if (Convert.ToBoolean(OKF.IsChecked))
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.INVO_LST_SUB.IsEnabled = false;
                this.Page58.IsEnabled = false;
                this.ESLAH.IsEnabled = true;
            }

            if (INVO_LST_FACTOR22_DATA.Count > 0 && !NewRecord)
            {
                this.Command100.IsEnabled = true;
                this.Command106.IsEnabled = true;
                this.Command108.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command106.IsEnabled = false;
                this.Command108.IsEnabled = false;
            }

        }
        public void ClearFreshNew()
        {
            NUMBER.Text = "0"; //شماره فاکتور

            NUMBER.Text = "0"; //شماره حواله

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();

            MAS.Text = "0"; //مدت

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد

            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 

            OKF.IsChecked = false; //تایید فاکتور

            JJKOL.Text = "0"; //جمع فاکتور

            MANDAH.Text = null;
            N_S.Text = "0"; //ثبت در سند
            TAKHFIF_PERCENT.Text = "0"; //Reset درصد تخفیف برای جلوگیری از تداخل و محاسبه اشتباه
            //پشت فاکتور

            M_NAGHD.Text = "0"; //مبلغ نقد
            MABL_VAR.Text = "0"; //مبلغ واریزی

            MOIN_VAR.Text = null; //معین واریزی
            MABL_HAV.Text = "0"; //مبلغ حواله
            MOIN_HAV.Text = null; //معین حواله

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

            Form_Current();
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
                                                         WHERE        (dbo.INVO_LST.TAG = {HTAG}) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

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
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                    if (true /*ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim()*/)
                    {
                        //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                        if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                        {
                            CURRENT_ITEMS_ROW.MEGH = 0;
                            CURRENT_ITEMS_ROW.MEGHk = 0;
                            CURRENT_ITEMS_ROW.MABL_K = 0;
                            SERCHK sERCHK = new SERCHK(I_AM_KHADAMAT, CURRENT_ITEMS_ROW.ANBAR.ToString());
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
                                CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITEMS_ROW.ANBAR.ToString(), I_AM_KHADAMAT);
                                if (FROM_SEARCH_KAL.CODE is null)
                                {
                                    INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                                    INVO_LST_SUB.CancelEdit();
                                    INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;

                                    CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                    CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;

                                    //INVO_LST_SUB_Cancel_Edit(sender, e);
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
                        if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5") //در فاكتور فروش قيمت مصرف كننده نشان داده شود
                        {
                            if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                            {
                                var RSTCC1 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                                if (RSTCC1.Count > 0)
                                {
                                    CURRENT_ITEMS_ROW.SANAD_NO = RSTCC1.FirstOrDefault().MAX_M;
                                }
                            }
                            else if (CURRENT_ITEMS_ROW.SANAD_NO == 0 || IsNull(CURRENT_ITEMS_ROW.SANAD_NO))
                            {
                                var RSTCC2 = dbms.DoGetDataSQL<double?>("SELECT  TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 14) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')  GROUP BY SANAD_NO HAVING (NOT (SANAD_NO IS NULL))").ToList();
                                if (RSTCC2.Count > 0)
                                {
                                    CURRENT_ITEMS_ROW.SANAD_NO = RSTCC2.FirstOrDefault();
                                }
                            }
                        }

                        if (Strings.Len(ENTERED_VALUE_ROW.ToString()) >= 9)
                        {
                            var RSTCC3 = dbms.DoGetDataSQL<_NFANI_>("SELECT N_FANI,CODE FROM STUF_DEF WHERE N_FANI = '" + ENTERED_VALUE_ROW.ToString() + "'").ToList();
                            if (RSTCC3.Count > 0)
                            {
                                CURRENT_ITEMS_ROW.CODE = RSTCC3.FirstOrDefault().CODE;

                                if (CURRENT_ITEMS_ROW.MEGH == 0)
                                {
                                    CURRENT_ITEMS_ROW.MEGH = 1;
                                    CURRENT_ITEMS_ROW.MEGHk = 1;
                                }
                            }
                            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
                            {
                                if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                                {
                                    var RSTCC4 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                                    if (RSTCC4.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.SANAD_NO = RSTCC4.FirstOrDefault().MAX_M;
                                    }
                                }
                                else if (CURRENT_ITEMS_ROW.SANAD_NO == 0 || IsNull(CURRENT_ITEMS_ROW.SANAD_NO))
                                {
                                    var RSTCC5 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 14) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                    if (RSTCC5.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.SANAD_NO = RSTCC5.FirstOrDefault();
                                    }
                                }
                            }
                            string CC = "";
                            if (Strings.Mid(Baseknow.OPTIONSS, 34, 1) == "5")
                            {
                                switch (Strings.Mid(Baseknow.OPTIONSS, 35, 2) ?? "")
                                {
                                    case "03":
                                        {
                                            CC = "";
                                            CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 18, 6)));
                                            CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 4, 3) + "." + Strings.Mid(CURRENT_ITEMS_ROW.CODE, 7, 3));
                                            CURRENT_ITEMS_ROW.MABL = Convert.ToDouble(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 10, 8));
                                            CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH;
                                            CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                                            CURRENT_ITEMS_ROW.CODE = CC;

                                            break;
                                        }

                                    default:
                                        {
                                            CC = "";
                                            CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 3, 5)));
                                            if (Convert.ToDouble(Strings.Left(CURRENT_ITEMS_ROW.CODE, 2)) == Convert.ToDouble("27"))
                                            {
                                                CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 8, 2) + "." + Strings.Mid(CURRENT_ITEMS_ROW.CODE, 10, 3));
                                                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH;
                                            }
                                            else
                                            {
                                                CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITEMS_ROW.CODE, 8, 5));
                                                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH;
                                            }
                                            CURRENT_ITEMS_ROW.CODE = CC;

                                            break;
                                        }
                                }
                            }
                        }

                        var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                        if (RST.Count == 0)
                        {
                        }
                        else
                        {
                            //CURRENT_ITEMS_ROW.VAHED_K = RST.FirstOrDefault().VAHED;
                            if (Baseknow.GHAYM == 2)
                            {
                                CURRENT_ITEMS_ROW.MABL = RST.FirstOrDefault().MABL_F;
                            }
                            else if (Baseknow.GHAYM == 5)
                            {
                                var RST11 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                                if (RST11.Count > 0)
                                {
                                    if (CURRENT_ITEMS_ROW.MABL != RST11.FirstOrDefault().PRICE_M && RST11.FirstOrDefault().PRICE_M != 0)
                                    {
                                        CURRENT_ITEMS_ROW.MABL = RST11.FirstOrDefault().PRICE_M;
                                        CURRENT_ITEMS_ROW.N_KOL = RST11.FirstOrDefault().TAFPER;
                                    }
                                    if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                                    {
                                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                                    }
                                }
                            }
                            else if (Baseknow.GHAYM == 4)
                            {
                                var RSTCO0 = dbms.DoGetDataSQL<MXNF>("SELECT     TOP 100 PERCENT MAX(dbo.INVO_LST.NUMBER) AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.HEAD_LST.CUST_NO = '" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.TAG = 14) AND (dbo.INVO_LST.CODE = '" + CURRENT_ITEMS_ROW.CODE + "')GROUP BY dbo.INVO_LST.MABL ORDER BY MAX(dbo.INVO_LST.NUMBER) DESC").FirstOrDefault();
                                if (RSTCO0 == null)
                                {
                                    CURRENT_ITEMS_ROW.MABL = 0;
                                }
                                else
                                {
                                    CURRENT_ITEMS_ROW.MABL = RSTCO0.MABL;
                                }
                            }
                            else if (Baseknow.GHAYM == 6)
                            {
                                var RSTCO1 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                                if (RSTCO1.Count > 0)
                                {
                                    if (CURRENT_ITEMS_ROW.MABL != RSTCO1.FirstOrDefault().PRICE_M && RSTCO1.FirstOrDefault().PRICE_M != 0)
                                    {
                                        CURRENT_ITEMS_ROW.MABL = RSTCO1.FirstOrDefault().PRICE_M;
                                        CURRENT_ITEMS_ROW.N_KOL = RSTCO1.FirstOrDefault().TAFPER;
                                    }
                                    if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                                    {
                                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                                    }
                                }
                            }
                            if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                            {
                                this.MANDAH.Text = RST.FirstOrDefault().TOZIH;
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
                            }
                        }
                        if (Baseknow.GHAYM == 1)
                        {
                            var RSTCO4 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ITEMS_ROW.CODE + "')) GROUP BY INVO_LST.MABL").ToList();
                            if (IsNull(RSTCO4.FirstOrDefault().MABL) || RSTCO4.Count == 0)
                            {
                                CURRENT_ITEMS_ROW.MABL = 0;
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.MABL = RSTCO4.FirstOrDefault().MABL;
                            }
                        }
                        else if (Baseknow.GHAYM == 3)
                        {
                            CURRENT_ITEMS_ROW.MABL = 0;
                        }

                        VAHED_K_AfterUpdate();
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                        if (Baseknow.TKHF == 2)
                        {
                            var RSTCO5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                            if (RSTCO5.Count > 0)
                            {
                                CURRENT_ITEMS_ROW.N_KOL = RSTCO5.FirstOrDefault().TAFPER;
                                if (Baseknow.GHAYM == 6)
                                {
                                    if (CURRENT_ITEMS_ROW.MABL != RSTCO5.FirstOrDefault().PRICE_M && RSTCO5.FirstOrDefault().PRICE_M != 0)
                                    {
                                        CURRENT_ITEMS_ROW.MABL = RSTCO5.FirstOrDefault().PRICE_M;
                                    }
                                    if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                                    {
                                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                                    }
                                }
                            }
                        }
                        CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100));

                        var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(INVO_LST_SUB.Items[row_index], INVO_LST_SUB.Columns[TheCol]);
                        var TheDGCell_MEGH = CL_LMethods.GetDataGridCell(DGCInf);
                        TheDGCell_MEGH.Focus();
                    }

                    #endregion

                    #region CODE_AfterUpdate
                    CODE_AfterUpdate(out min, out MAND);
                    #endregion

                    #region CODE_Exit
                    if (!(IsNull(CURRENT_ITEMS_ROW.CODE) || CURRENT_ITEMS_ROW.CODE == ""))
                    {
                        if (CURRENT_ITEMS_ROW.CODE == CURRENT_ITEMS_ROW.CODEO && (CURRENT_ITEMS_ROW.id <= 0))
                        {
                            CODE_AfterUpdate(out min, out MAND);

                            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
                            {
                                if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                                {
                                    var RSTE0 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                                    if (RSTE0.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.SANAD_NO = RSTE0.FirstOrDefault().MAX_M;
                                    }
                                }
                                else if (CURRENT_ITEMS_ROW.SANAD_NO == 0 || IsNull(CURRENT_ITEMS_ROW.SANAD_NO))
                                {
                                    var RSTE1 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 14) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                    if (RSTE1.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.SANAD_NO = RSTE1.FirstOrDefault();
                                    }
                                }
                            }
                        }
                        if (Baseknow.GHAYM == 1)
                        {
                            var RSTE2 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ITEMS_ROW.CODE + "')) GROUP BY INVO_LST.MABL").ToList();
                            if (IsNull(RSTE2.FirstOrDefault().MABL))
                            {
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.MABL = RSTE2.FirstOrDefault().MABL;
                                CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                            }
                        }
                        else if (Baseknow.GHAYM == 2)
                        {
                            var RSTE3 = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM dbo.STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                            if (RSTE3.Count == 0)
                            {
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.MABL = RSTE3.FirstOrDefault().MABL_F;
                                CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                            }
                        }
                        else if (Baseknow.GHAYM == 4)
                        {
                            var RSTE4 = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 14) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + this.NUMBER.Text + ") ORDER BY dbo.INVO_LST.NUMBER DESC").ToList();
                            if (RSTE4.Count > 0 && !IsNull(RSTE4.FirstOrDefault().MABL))
                            {
                                CURRENT_ITEMS_ROW.MABL = RSTE4.FirstOrDefault().MABL;
                                CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                            }
                            else
                            {
                                Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                                msgwin.ShowDialog();
                                CURRENT_ITEMS_ROW.MABL = 0;
                                CURRENT_ITEMS_ROW.MABL_K = 0;
                            }
                        }
                        else if (Baseknow.GHAYM == 5)
                        {
                            var RSTE5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                            if (RSTE5.Count > 0)
                            {
                                if (CURRENT_ITEMS_ROW.N_KOL != RSTE5.FirstOrDefault().TAFPER)
                                {
                                    CURRENT_ITEMS_ROW.N_KOL = RSTE5.FirstOrDefault().TAFPER;
                                }
                                if (CURRENT_ITEMS_ROW.MABL != RSTE5.FirstOrDefault().PRICE_M && RSTE5.FirstOrDefault().PRICE_M != 0)
                                {
                                    CURRENT_ITEMS_ROW.MABL = RSTE5.FirstOrDefault().PRICE_M;
                                }
                                if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                                {
                                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                                }
                            }
                            else
                            {
                                Msgwin msgwin = new Msgwin(false, "اين كالا داراي قيمت مصوب نيست است.");
                                msgwin.ShowDialog();
                                CURRENT_ITEMS_ROW.MABL = 0;
                                CURRENT_ITEMS_ROW.MABL_K = 0;
                            }
                        }
                    }
                    #endregion
                }
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
                    //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MEGH").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));
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
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("MEGHk").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
                    return;
                }
                if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null || (e.Row.Item as INVO_LST_FACTOR22).MEGH is null)
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
                    CURRENT_ITEMS_ROW.MEGH = CURRENT_ITEMS_ROW.MEGHk / RST.FirstOrDefault().NESBAT/*(2)*/;
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
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("MABL").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
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
                long Temp;
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;
                    //MABL_K.Text.TabStop = true;
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
                CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100));
                var RSTMB0 = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 14) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                if (RSTMB0.Count == 0)
                {
                }
                else if (RSTMB0.FirstOrDefault().MABL/*(0)*/ > CURRENT_ITEMS_ROW.MABL)
                {
                    Msgwin msgwin = new Msgwin(false, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RSTMB0.FirstOrDefault().MABL);
                    msgwin.ShowDialog();
                }
                CURRENT_ITEMS_ROW.AVRAGE = 0;

                var RSTMB1 = dbms.DoGetDataSQL<DTLMANF_QRE1>("SELECT Sum(DTL_MANF.MABLK) AS SumOfMABLK, HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR FROM HEAD_MANF INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) WHERE (((HEAD_MANF.CODE) = '" + CURRENT_ITEMS_ROW.CODE + "')) GROUP BY HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR").ToList();
                if (RSTMB1.Count > 0)
                {
                    CURRENT_ITEMS_ROW.AVRAGE = RSTMB1.FirstOrDefault().SumOfMABLK/*(0)*/ + RSTMB1.FirstOrDefault().IMBIBE_MANF/*(1)*/ + RSTMB1.FirstOrDefault().IMBIBE_SAR/*(2)*/;

                }
                else
                {
                    var RSTMB2 = dbms.DoGetDataSQL<QRE_FAC_01>("SELECT RADAH,CODE FROM STUF_DEF  WHERE (STUF_DEF.CODE = '" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                    if (RSTMB2.Count > 0)
                    {
                        if (RSTMB2.FirstOrDefault().RADAH == 2 || RSTMB2.FirstOrDefault().RADAH == 3)
                        {

                            CURRENT_ITEMS_ROW.AVRAGE = 0;
                        }
                    }
                }
                CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
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

                #region MABL_Exit
                if (!(IsNull(CURRENT_ITEMS_ROW.CODE) || CURRENT_ITEMS_ROW.CODE == "") && Baseknow.GHAYM != 5)
                {
                    var RST = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 14) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                    if (RST.Count == 0)
                    {
                    }
                    else if (RST.FirstOrDefault().MABL/*(0)*/ > CURRENT_ITEMS_ROW.MABL && CURRENT_ITEMS_ROW.MABL != 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RST.FirstOrDefault().MABL);
                        msgwin.ShowDialog();

                    }
                }
                if (CURRENT_ITEMS_ROW.N_MOIN != Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100)))
                {
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((double)(CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100));
                }

                //MABL_KeyPress
                if (Baseknow.TKHF == 2)
                {
                    var RST = dbms.DoGetDataSQL<short?>("SELECT TAFPER FROM dbo.TAKHPERS WHERE (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").FirstOrDefault();
                    if (RST != null)
                    {
                        if (CURRENT_ITEMS_ROW.N_KOL != RST) //TAFPER
                        {
                            CURRENT_ITEMS_ROW.N_KOL = RST;
                        }
                    }
                    else if (CURRENT_ITEMS_ROW.N_KOL != 0)
                    {
                        CURRENT_ITEMS_ROW.N_KOL = 0;
                    }
                }
                if (CURRENT_ITEMS_ROW.N_MOIN != Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)))
                {
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100));
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
                else if (CURRENT_ITEMS_ROW.IMBAA != 0)
                {
                    CURRENT_ITEMS_ROW.IMBAA = 0;
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
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("MABL_K").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
                    return;
                }
                if (
                   (e.Row.Item as INVO_LST_FACTOR22).ANBAR is null ||
                   (e.Row.Item as INVO_LST_FACTOR22).CODE is null ||
                   (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null ||
                   (e.Row.Item as INVO_LST_FACTOR22).MEGH is null ||
                   (e.Row.Item as INVO_LST_FACTOR22).MEGHk is null ||
                   (e.Row.Item as INVO_LST_FACTOR22).MABL is null
                   )
                {
                    return;
                }

                #region MABL_K_AfterUpdate
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

                #region MABL_K_Exit
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
                if (CURRENT_ITEMS_ROW.N_MOIN != Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100)))
                {
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW.N_KOL * CURRENT_ITEMS_ROW.MABL_K / 100))) * CURRENT_ITEMS_ROW.TKHN / 100));
                }
                #endregion
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

            //var MABL_TAKHFIF = Convert.ToDouble(INVO_LST_FACTOR22_DATA.Sum(r => r.N_MOIN is null ? 0 : r.N_MOIN)); //جمع مبلغ تخفیف دیتاگرید
            //var CTT_TAKHFIF = Convert.ToDouble(TAKHFIF2.Text); //مجموع مبلغ تخفیف کل
            //if (MABL_TAKHFIF != CTT_TAKHFIF)
            //{
            //    if (MABL_TAKHFIF >= 0)
            //    {
            //        TAKHFIF2.Text = MABL_TAKHFIF.ToStringNullSafe();
            //    }
            //}


        }
        public void AVRAGE_UPDATE()
        {
            return;
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
        void CODE_AfterUpdate(out double min, out double MAND)
        {
            min = 0;
            MAND = 0;
            long Temp;
            //var RST2 = dbms.DoGetDataSQL<int?>($"SELECT TOP(1) VAHED FROM STUF_DEF WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' ORDER BY VAHED").ToList();
            //if (RST2.Count == 0)
            //{
            //}
            //else
            //{
            //    CURRENT_ITEMS_ROW.VAHED_K = RST2.FirstOrDefault();
            //}
            if (Baseknow.GHAYM == 7)
            {
            }
            else
            {
                if (Baseknow.GHAYM == 1)
                {
                    var RSTC1 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ITEMS_ROW.CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                    if (IsNull(RSTC1.MABL))
                    {
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.MABL = RSTC1.MABL;
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
                else if (Baseknow.GHAYM == 2)
                {
                    var RSTC2 = dbms.DoGetDataSQL<double?>($"SELECT TOP(1) MABL_F FROM STUF_DEF WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' ORDER BY VAHED").ToList();
                    if (RSTC2.Count == 0)
                    {
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.MABL = RSTC2.FirstOrDefault();
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
                else if (Baseknow.GHAYM == 4)
                {
                    var RSTC3 = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 14) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + this.NUMBER.Text + ") ORDER BY dbo.INVO_LST.NUMBER DESC").ToList();
                    if (RSTC3.Count > 0 && !IsNull(RSTC3.FirstOrDefault().MABL))
                    {
                        CURRENT_ITEMS_ROW.MABL = RSTC3.FirstOrDefault().MABL;
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                    else
                    {
                        Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                        msgwin.ShowDialog();
                        CURRENT_ITEMS_ROW.MABL = 0;
                        CURRENT_ITEMS_ROW.MABL_K = 0;
                    }
                }
                else if (Baseknow.GHAYM == 5)
                {
                    var RSTC4 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                    if (RSTC4.Count > 0)
                    {
                        if (CURRENT_ITEMS_ROW.N_KOL != RSTC4.FirstOrDefault().TAFPER)
                        {
                            CURRENT_ITEMS_ROW.N_KOL = RSTC4.FirstOrDefault().TAFPER;
                        }
                        if (CURRENT_ITEMS_ROW.MABL != RSTC4.FirstOrDefault().PRICE_M && RSTC4.FirstOrDefault().PRICE_M != 0)
                        {
                            CURRENT_ITEMS_ROW.MABL = RSTC4.FirstOrDefault().PRICE_M;
                        }
                        if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                        {
                            CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                        }
                    }
                    else
                    {
                        universControl.PopNotifyShow("اين كالا داراي قيمت مصوب نيست است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");

                        CURRENT_ITEMS_ROW.MABL = 0;
                        CURRENT_ITEMS_ROW.MABL_K = 0;
                    }
                }
                if (Baseknow.TKHF == 2)
                {
                    var RSTC5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                    if (RSTC5.Count > 0)
                    {
                        CURRENT_ITEMS_ROW.N_KOL = RSTC5.FirstOrDefault().TAFPER;
                        if (Baseknow.GHAYM == 5)
                        {
                            if (CURRENT_ITEMS_ROW.MABL != RSTC5.FirstOrDefault().PRICE_M && RSTC5.FirstOrDefault().PRICE_M != 0)
                            {
                                CURRENT_ITEMS_ROW.MABL = RSTC5.FirstOrDefault().PRICE_M;
                            }
                            if (CURRENT_ITEMS_ROW.MABL_K != Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)))
                            {
                                CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                            }
                        }
                    }
                }
            }

            if (CURRENT_ITEMS_ROW?.N_MOIN != null && CURRENT_ITEMS_ROW?.N_KOL != null && CURRENT_ITEMS_ROW?.MABL_K != null && CURRENT_ITEMS_ROW?.TKHN != null) //For Nullable Check to avoid error
            {
                if (CURRENT_ITEMS_ROW?.N_MOIN != Math.Round((double)(CURRENT_ITEMS_ROW?.N_KOL * CURRENT_ITEMS_ROW?.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW?.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW?.N_KOL * CURRENT_ITEMS_ROW?.MABL_K / 100))) * CURRENT_ITEMS_ROW?.TKHN / 100)))
                {
                    CURRENT_ITEMS_ROW.N_MOIN = Math.Round((double)(CURRENT_ITEMS_ROW?.N_KOL * CURRENT_ITEMS_ROW?.MABL_K / 100)) + Math.Round((double)((CURRENT_ITEMS_ROW?.MABL_K - Math.Round((double)(CURRENT_ITEMS_ROW?.N_KOL * CURRENT_ITEMS_ROW?.MABL_K / 100))) * CURRENT_ITEMS_ROW?.TKHN / 100));
                }
            }

            if ((bool)TICMBAA.IsChecked)
            {
                var RSTC6 = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("select CMBAA ,code from STUF_DEF where code = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                if (RSTC6.Count > 0)
                {
                    if ((bool)RSTC6.FirstOrDefault().CMBAA)
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
            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                {
                    var RSTC7 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')").ToList();
                    if (RSTC7.Count > 0)
                    {
                        if (CURRENT_ITEMS_ROW.SANAD_NO != RSTC7.FirstOrDefault().MAX_M)
                        {
                            CURRENT_ITEMS_ROW.SANAD_NO = RSTC7.FirstOrDefault().MAX_M;
                        }
                    }
                }
                else if (CURRENT_ITEMS_ROW?.SANAD_NO == 0 || IsNull(CURRENT_ITEMS_ROW?.SANAD_NO))
                {
                    var RSTC8 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 14) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ITEMS_ROW.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                    if (RSTC8.Count > 0)
                    {
                        if (CURRENT_ITEMS_ROW.SANAD_NO != RSTC8.FirstOrDefault())
                        {
                            CURRENT_ITEMS_ROW.SANAD_NO = RSTC8.FirstOrDefault();
                        }
                    }
                }
            }
            min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW?.ANBAR, CURRENT_ITEMS_ROW?.CODE);
            if (CURRENT_ITEMS_ROW?.ANBAR != 0)
            {
                if (CURRENT_ITEMS_ROW.id > 0)
                {
                }
                if (CURRENT_ITEMS_ROW?.id > 0 && !IsNull(CURRENT_ITEMS_ROW?.CODE))
                {
                    var RSTC9 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                    if (RSTC9.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                    {
                        var RSTD = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                        if (RSTD.Count > 0)
                        {
                            MAND = (double)RSTD.FirstOrDefault();
                            if (Math.Round((double)(RSTD.FirstOrDefault() - CURRENT_ITEMS_ROW.MEGHk), 2) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                msgwin.ShowDialog();
                                CURRENT_ITEMS_ROW = WAS_ROW_ITEM;
                            }
                            else
                            {
                                var RSTD2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                                var _where = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                                if (RSTD2.Count > 0)
                                {
                                    RSTD2.FirstOrDefault().MOGODI = MAND - CURRENT_ITEMS_ROW.MEGHk;
                                    //RSTD2.Fields("MOGODI_A") = 0;
                                    //dbms.DoExecuteSQL($" UPDATE dbo.STUF_STK SET MOGODI_A = 0, MOGODI = {RSTD2.FirstOrDefault().MOGODI} {_where} ");
                                    //در اینجا موجودی بروز نمیشود فقط بررسی میشود
                                    //RSTD2.update();
                                }
                            }
                        }
                    }
                    else if (CURRENT_ITEMS_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                    {
                        if (RSTC9.FirstOrDefault().MOGODI + RSTC9.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITEMS_ROW.MEGH_MAR)) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                            msgwin.ShowDialog();
                            CURRENT_ITEMS_ROW = WAS_ROW_ITEM;

                        }
                    }
                    else if (RSTC9.FirstOrDefault().MOGODI + RSTC9.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();
                        CURRENT_ITEMS_ROW = WAS_ROW_ITEM;
                    }
                }
            }
            VAHED_K_AfterUpdate();
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
                INVO_LST_SUB_CANCEL_EDIT();
                return;
            }


            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                              OUTPUT INSERTED.id
                              VALUES({NUMBER.Text},
                              {HTAG} ,
                              {TheRow.ANBAR}   ,
                              NULL,
                              N'{TheRow.CODE}' ,
                              {TheRow.MEGH} ,
                              {TheRow.MEGHk} ,
                              {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)} ,
                              N'{TheRow.MANDAH}' ,
                              {TheRow.MABL} ,
                              {TheRow.MABL_K} ,
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
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG={HTAG}) FROM dbo.INVO_LST WHERE id = {TheRow.id}");
                }
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

                SET_SPECIAL_TAKHFIF();
            }

            MasterTopErrorMessages.AddRange(ErrosMessages);


            SANAD();

            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

            //AVRAGE_UPDATE();

            TAKHFIF.Text = SUM_OF_N_MOIN.ToString();
            TAKHFIF_MABL_PRICE();

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

            if (string.IsNullOrEmpty(MAS.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مدت را وارد کنید " });
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
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ خدمات (سرویس) مشخص نشده!" });
            }

            if (!IsNull(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }

            if (!IsNull(CMB_HMBAA.SelectedValue.ToStringNullSafe()))
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
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
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
            // Validate N_KOL
            if (!double.TryParse(TheRow.N_KOL?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تخفیف صحیح وارد نشده" });
            }
            if (!(TheRow.N_KOL >= 0 && TheRow.N_KOL <= 100))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "محدوده وارد شده تخفیف صحیح نیست" });
            }
            // Validate N_MOIN
            if (!double.TryParse(TheRow.N_MOIN?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ تخفیف صحیح وارد نشده" });
            }
            if (MABL_HAZ.Text != "0" && IsNull(MOIN_HAZ.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مربوط به سرويس مشخص نشده است حتما بايد حساب مربوط به سرويس مشخص شود" });
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

            if (NUMBER.Text == "0")
            {
                //Max Of Number1 TAG -----12
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        //Fake Query for Lock Table
                        db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                        //Fake Query for Lock Table

                        var rst_11 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={HTAG}))", null, transaction).FirstOrDefault();
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
                                                        VALUES ({NUMBER.Text},  {HTAG},    0,    0,   0,       0,        0,        0,        0,    0   )", null, transaction);

                        transaction.Commit();
                        db?.Close();
                    }
                }

            }

            #region Form_BeforeUpdate
            //Form_BeforeUpdate
            if (Convert.ToDouble(Strings.Mid(Baseknow.OPTIONSS, 19, 1)) == 5d)
            {
                var RST = dbms.DoGetDataSQL<string?>("SELECT CUST_COD FROM dbo.CUST_HESAB WHERE (hes = N'" + CUST_NO.SelectedValue + "')").FirstOrDefault();
                if (RST != null)
                {
                    if (CUST_KIND.SelectedValue.ToStringNullSafe() != RST) //CUST_COD
                    {
                        this.CUST_KIND.SelectedValue = RST; CUST_KIND.Items.Refresh();
                    }
                }
            }
            CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), HTAG);
            #endregion

            this.OKF.IsChecked = true;

            DoCmdHeaderSave();

            TICMBAA_Click(null, null);

            SANAD();

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            this.INVO_LST_SUB.IsReadOnly = false;
            this.INVO_LST_SUB.IsEnabled = true;
            this.Page58.IsEnabled = true;


            //Form_AfterUpdat
            if (INVO_LST_FACTOR22_DATA.Count > 0)
            {
                Command100.IsEnabled = true;
                Command106.IsEnabled = true;
                Command108.IsEnabled = true;
            }
            else
            {
                Command100.IsEnabled = false;
                Command106.IsEnabled = false;
                Command108.IsEnabled = false;
            }

            ChangeIsHappend = false;

            isSavedSuccess = true;
        }

        private void GetBalancePerson()
        {
            //کادر سبز و سند و مانده حساب
            if (Baseknow.MAND && !NewRecord)
            {
                var SANAD_NUMBER = dbms.DoGetDataSQL<string>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();
                if (SANAD_NUMBER != null)
                {
                    if (!CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue?.ToString()))
                    {
                        if (CUST_NO.SelectedValue != null)
                        {
                            MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                        }
                        N_S.Text = SANAD_NUMBER?.ToString();
                    }
                    else
                    {
                        this.MANDAH.Text = "مسدود است";
                    }
                }
            }

        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!IsNull(this.NUMBER.Text) && NUMBER.Text != "0")
            {
                DateTime dt;
                if (!IsNull(this.NUMBER.Text))
                {
                    dt = DateTime.Now;
                    dbms.DoExecuteSQL("INSERT INTO dbo.TR_HEAD_LST   (NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, UP_TIME,UP_DATE,OKF,UP_USER_NAME,PC_NAME,IPADD) SELECT     NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF," + Tarikh.GET_OADATE_DAO() + "   AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2,OKF,'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.CurrentMachineName() + "' , '" + CL_HESABDARI.GETIPADD() + "'   FROM dbo.HEAD_LST WHERE (NUMBER = " + this.NUMBER.Text + $" ) And (TAG = {HTAG})");
                    dbms.DoExecuteSQL("INSERT INTO dbo.TR_INVO_LST   (UP_TIME, UP_DATE, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA) SELECT    " + Tarikh.GET_OADATE_DAO() + "   AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K , FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA FROM dbo.INVO_LST WHERE (NUMBER = " + this.NUMBER.Text + $") And (TAG = {HTAG})");
                    dbms.DoExecuteSQL("INSERT INTO dbo.TR_PAY_GETD   (N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3,N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, UP_TIME, UP_DATE) SELECT     N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3,N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ," + Tarikh.GET_OADATE_DAO() + "   AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2 FROM dbo.PAY_GETD WHERE (NUMBER = " + this.NUMBER.Text + $") And (TAG = {HTAG})");

                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                    this.INVO_LST_SUB.IsEnabled = true;
                    this.Page58.IsEnabled = true;
                }
                SecurityAllCheck();
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord) { return; }

            var editableCollectionView = INVO_LST_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem)
            {
                editableCollectionView.CommitEdit();
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                if (INVO_LST_SUB.Items.Count > 0)
                {
                    if (!(INVO_LST_SUB.SelectedItems is null))
                    {
                        #region SABEGHEH
                        var dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //12
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                        #endregion

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور خدمات",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 14",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

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
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER1 = {NUMBER.Text} AND TAG = {HTAG}");

                            SANAD();

                            ClearFreshNew();
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
                        INVO_LST_SUB_ReGetData();
                    }
                }
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
                    TAH = N'{TAH.Text}', MAS = {MAS.Text}, N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', 
                    M_NAGHD = {M_NAGHD.Text}, MABL_VAR = {MABL_VAR.Text}, MOIN_VAR = N'{CMB_MOIN_VAR.SelectedValue}', 
                    MABL_HAV = {MABL_HAV.Text}, MOIN_HAV = N'{CMB_MOIN_HAV.SelectedValue}',TAKHFIF = {TAKHFIF.Text},

                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    MBAA = {MBAA.Text}, HMBAA = N'{CMB_HMBAA.SelectedValue}', 
                    ANBAR =  {(ANBAR is null ? "NULL" : ANBAR)},
                    OKF = {Convert.ToByte(OKF.IsChecked)}, TICMBAA = {Convert.ToByte(TICMBAA.IsChecked)},
                    USER_NAME = N'{USER_NAME.Text}'
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} ";

            _ = dbms.DoExecuteSQL(_qre);

            return true;
        }
        private void Summer()
        {
            JJKOL.Text = SUM_OF_MABL_K.ToString(); //SMABLK //جمع فاکتور :
            HKH.Text = MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL.Text; //جمع کل فاکتور برای فسمت روی فاکتور

            NCHK.Text = PAY_GETD_SUB22_DATA.Sum(x => x.MABL)?.ToString(); //جمع مبالغ چکهای پرداختی

            //مبلغ قابل پرداخت: //= [JF] + [HKH] - [NTKHFIF] + [MBAA]
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            //جمع مبالغ پرداختی
            //=[M_NAGHD]+[MABL_VAR]+[MABL_HAV]+[NCHK]

            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(MABL_VAR.Text) + Convert.ToInt64(MABL_HAV.Text) + Convert.ToInt64(NCHK.Text);
            NPAR.Text = RMP.ToString();

            if (!string.IsNullOrEmpty(MABL_HAZ.Text) && MABL_HAZ.Text != "0")
            {
                this.SPER.Text = (Convert.ToDouble(MABL_HAZ.Text) * 100 / SUM_OF_MABL_K).ToString();
            }

            //=[GHABEL]-[NPAR]
            MAN.Text = Convert.ToString(Convert.ToInt64(GHABEL.Text) - Convert.ToInt64(NPAR.Text)); //مانده
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
                ComboSearch CMBSearch = new ComboSearch(this.GetType().Name, I_AM_KHADAMAT);//Search Plusy Form Specialy for Customers
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

            CUST_NO_AfterUpdate();

            #region CUST_NO_Exit
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
            #endregion

        }
        private void SANAD()
        {
            AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHAD(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

            Summer();

            GetBalancePerson();
        }

        #region POSHTE_FACTOR
        public PAY_GETD_SUB22_MODEL? PAY_GETD_WAS_ROW_ITEM { get; set; }
        public void PAY_GETD_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //PAY_GETD_SUB22_DATA
                PAY_GETD_SUB22_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<PAY_GETD_SUB22_MODEL>($@"SELECT * FROM PAY_GETD WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
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
            //var rowContainer = INVO_LST_SUB.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
            //DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
            //DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            //if (cell == null)
            //{
            //    INVO_LST_SUB.ScrollIntoView(rowContainer, INVO_LST_SUB.Columns[CURRENT_COLUMN_INDEX]);
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
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب وارد (جاری چک) نشده!" });
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
            else
            {
                if (!int.TryParse(FINAL_CROW_ITEM.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_MOIN?.ToString()))
                {
                    //ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
                }
                else
                {
                    if (!int.TryParse(FINAL_CROW_ITEM.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_TAF?.ToString()))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
                    }
                }

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
                            TAG = {HTAG}, ANBAR = 1, VAZ = {FINAL_CROW_ITEM.VAZ}, KIND = {FINAL_CROW_ITEM.KIND},
                            SANDUGH = {FINAL_CROW_ITEM.SANDUGH}, SAYADI = N'{FINAL_CROW_ITEM.SAYADI}'
                            WHERE ID = {FINAL_CROW_ITEM.ID}");
            }
            else //Insert
            {
                string dbtest = $@"INSERT INTO PAY_GETD (N_SERI,                   BANK,                   DATE_S,                   DATE,                    SHOBEH,                     MABL,                    NAME_TAH,                      N_HESAB,                    N_KOL,                   N_MOIN,                    N_TAF,  NUMBER,        TAG, ANBAR,                                                                   RADIF,                                             VAZ,                    KIND,               SANDUGH,                        SAYADI) 
                                                                OUTPUT INSERTED.ID
                                                                VALUES ({FINAL_CROW_ITEM.N_SERI}, {FINAL_CROW_ITEM.BANK}, {FINAL_CROW_ITEM.DATE_S}, {FINAL_CROW_ITEM.DATE}, N'{FINAL_CROW_ITEM.SHOBEH}', {FINAL_CROW_ITEM.MABL}, N'{FINAL_CROW_ITEM.NAME_TAH}', N'{FINAL_CROW_ITEM.N_HESAB}', {FINAL_CROW_ITEM.N_KOL}, {FINAL_CROW_ITEM.N_MOIN}, {FINAL_CROW_ITEM.N_TAF}, {NUMBER.Text}, {HTAG}, 1,    (SELECT TOP(1) RADIF+1 FROM dbo.PAY_GETD WHERE N_SERI = {N_SERI} AND BANK = {BANK} AND DATE_S = {DATE_S} AND NUMBER = {NUMBER.Text} AND TAG = {HTAG}), {FINAL_CROW_ITEM.VAZ}, {FINAL_CROW_ITEM.KIND}, {FINAL_CROW_ITEM.SANDUGH}, N'{FINAL_CROW_ITEM.SAYADI}')";

                var GOTID = dbms.DoGetDataSQL<long?>($@"INSERT INTO PAY_GETD (N_SERI,                   BANK,                   DATE_S,                   DATE,                    SHOBEH,                     MABL,                    NAME_TAH,                      N_HESAB,                    N_KOL,                                                         N_MOIN,                    N_TAF,  NUMBER,        TAG, ANBAR,           RADIF,                                             VAZ,                    KIND,               SANDUGH,                        SAYADI) 
                                                                OUTPUT INSERTED.ID
                                                                VALUES ({FINAL_CROW_ITEM.N_SERI}, {FINAL_CROW_ITEM.BANK}, {FINAL_CROW_ITEM.DATE_S}, {FINAL_CROW_ITEM.DATE}, N'{FINAL_CROW_ITEM.SHOBEH}', {FINAL_CROW_ITEM.MABL}, N'{FINAL_CROW_ITEM.NAME_TAH}', N'{FINAL_CROW_ITEM.N_HESAB}', {(FINAL_CROW_ITEM.N_KOL is null ? "NULL" : FINAL_CROW_ITEM.N_KOL)}, {(FINAL_CROW_ITEM.N_MOIN is null ? "NULL" : FINAL_CROW_ITEM.N_MOIN)}, {(FINAL_CROW_ITEM.N_TAF is null ? "NULL" : FINAL_CROW_ITEM.N_TAF)}, {NUMBER.Text}, {HTAG}, 1, {FINAL_CROW_ITEM.RADIF}   , {FINAL_CROW_ITEM.VAZ}, {FINAL_CROW_ITEM.KIND}, {FINAL_CROW_ITEM.SANDUGH}, N'{FINAL_CROW_ITEM.SAYADI}')").FirstOrDefault();
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
                                tableName: "فاکتور خدمات=> چک های دریافتی پشت فاکتور",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 14",
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

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            new FACTORS_LST(HTAG).Show(); //فاکتور خرید
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void MABL_HAV_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(this.MOIN_HAV.Text))
            {
                new Msgwin(false, "حساب مربوط به برگه رسید مشخص نشده است حتما بايد حساب مربوط به رسید مشخص شود ").ShowDialog();
                this.MOIN_HAV.Focus();
            }
            if (Convert.ToDouble(MABL_HAV.Text) == 0)
            {
                this.MOIN_HAV.Text = "";
            }
            //CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text), Convert.ToDouble(MABL_VAR.Text), Convert.ToDouble(MABL_HAV.Text), (bool)TICMBAA.IsChecked); //#CheckMatter
        }
        private void MABL_HAZ_AfterUpdate()
        {
            //MABL_HAZ2_AfterUpdate
            if (MABL_HAZ.Text != "0" && IsNull(MOIN_HAZ.Text))
            {
                new Msgwin(false, "حساب مربوط به سرويس مشخص نشده است حتما بايد حساب مربوط به سرويس مشخص شود ").ShowDialog();
                MOIN_HAZ.Focus();
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
        private void PARAMS_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!IsNull(CURRENT_ITEMS_ROW?.id != null))
            {
                e.Handled = true;
                var button = (Button)sender;
                var ROW = button.Tag as INVO_LST_FACTOR22;

                if (ROW?.id != null)
                {
                    var _id_ = dbms.DoGetDataSQL<long?>("SELECT ID FROM dbo.IVO_EXTENDED WHERE id=" + ROW?.id).SingleOrDefault();
                    if (_id_ == null)
                    {
                        dbms.DoExecuteSQL("INSERT INTO [dbo].[IVO_EXTENDED] VALUES(" + _id_ + ",0,0,0,0,0,0,0,0,0,0,GETDATE()," + Baseknow.USERCOD + ")");
                    }
                    new ZF_IVO_EXTENDED((int)ROW?.id, I_AM_KHADAMAT).ShowDialog();
                }

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

        private void TAKHFIF_MABL_PRICE(bool isTakhfifFocuse = true)
        {
            Summer();

            if (!string.IsNullOrEmpty(TAKHFIF.Text) && TAKHFIF.Text != "0" && JF.Text != "0" && isTakhfifFocuse) //درصد تخفیف
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
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_KHADAMAT.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 300;

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
                var rst_0 = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(this.CUST_NO.SelectedValue.ToString()) + ")").ToList();
                if (rst_0.Count == 0)
                {
                    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                }
                else
                {
                    var _mandah = Interaction.IIf(rst_0.FirstOrDefault() > 0, Strings.Format(rst_0.FirstOrDefault(), "##,# ريال بدهكار"), Strings.Format(rst_0.FirstOrDefault() * -1, "##,# ريال بستانكار"));
                    (report.GetComponentByName("MANDAH") as StiText).Text = _mandah.ToString();
                }
            }


            double JCHK = default, HAZ, NAGHD, VAR, HAV, taf, MBA, JAMF;
            double GB;
            var rst_3 = dbms.DoGetDataSQL<RPT_MODEL2>("SELECT     dbo.PAY_GETD.N_SERI, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.DATE, dbo.PAY_GETD.DATE_S , dbo.PAY_GETD.MABL, dbo.PAY_GETD.NUMBER, dbo.PAY_GETD.TAG FROM         dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK WHERE (dbo.PAY_GETD.NUMBER = " + NUMBER.Text + ") AND (dbo.PAY_GETD.N_KOL IS NULL OR N_KOL <> 911) AND (dbo.PAY_GETD.TAG = " + HTAG + ")").ToList(); //Forms(FRF)["Dtag"]
            if (rst_3.Count > 0)
            {
                JCHK = 0d;
                (report.GetComponentByName("COMM") as StiText).Text = "چكهاي دريافت شده " + rst_3.Count + " فقره جمعاًبه مبلغ :" + Strings.Format(Convert.ToInt64(NCHK.Text), "### ريال") + "  ";

                for (int o = 0; o < rst_3.Count; o++) //while (!rst_3.EOF())
                {
                    (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + "ـ سريال:" + rst_3[o].N_SERI + " بانك:" + rst_3[o].NAMES + " شعبه:" + Strings.Trim(rst_3[o].SHOBEH);
                    JCHK = (double)(JCHK + rst_3[o].MABL);
                }
            }
            else
            {
                (report.GetComponentByName("COMM") as StiText).Enabled = false;
                (report.GetComponentByName("SHARAYET") as StiText).Enabled = true;
            }

            JAMF = 0d;
            HAZ = 0d;
            NAGHD = 0d;
            VAR = 0d;
            HAV = 0d;
            taf = 0d;
            MBA = 0d;
            var JST0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER.Text + $" ) AND ((INVO_LST.TAG)={HTAG}))").ToList();
            if (JST0.Count > 0 && !IsNull(JST0.FirstOrDefault()))
            {
                JAMF = (double)JST0.FirstOrDefault();
            }
            var JST = dbms.DoGetDataSQL<RPT_MODEL3>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG AS htag, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.MBAA FROM HEAD_LST WHERE (((HEAD_LST.NUMBER)= " + NUMBER.Text + $" ) AND  ((HEAD_LST.TAG)={HTAG}))").ToList();
            if (JST.Count > 0 && !IsNull(JST.FirstOrDefault().NUMBER))
            {
                HAZ = (double)JST.FirstOrDefault().MABL_HAZ;
                VAR = (double)JST.FirstOrDefault().MABL_VAR;
                HAV = (double)JST.FirstOrDefault().MABL_HAV;
                NAGHD = (double)JST.FirstOrDefault().M_NAGHD;
                taf = (double)JST.FirstOrDefault().TAKHFIF;
                MBA = (double)JST.FirstOrDefault().MBAA;
            }

            GB = JAMF - HAZ + MBA - taf;

            (report.GetComponentByName("JF") as StiText).Text = JAMF.ToString();
            (report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString();
            (report.GetComponentByName("MBAA") as StiText).Text = MBA.ToString();
            (report.GetComponentByName("GABEL") as StiText).Text = (JAMF + HAZ + MBA - taf).ToString();

            if (taf == 0)
            {
                (report.GetComponentByName("TF") as StiText).Enabled = false;
            }
            else
            {
                (report.GetComponentByName("TF") as StiText).Enabled = true;
                (report.GetComponentByName("TF") as StiText).Text = taf.ToString();
            }

            (report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString();
            (report.GetComponentByName("MAN") as StiText).Text = (JAMF + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)).ToString();



            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            (report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            (report.GetComponentByName("TELEPHONE") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده
            (report.GetComponentByName("USERNAME") as StiText).Text = Baseknow.UUSER;
            (report.GetComponentByName("TXT_DEPARTMAN") as StiText).Text = DEPATMAN.Text;

            (report.GetComponentByName("TheCUST_NO") as StiText).Text = CUST_NO.Text;

            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(JAMF + HAZ + MBA - taf));

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
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_KHADAMAT2.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["SmallFactor"]).CommandTimeout = 300;


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
                var rst_0 = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(this.CUST_NO.SelectedValue.ToString()) + ")").ToList();
                if (rst_0.Count == 0)
                {
                    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                }
                else
                {
                    var _mandah = Interaction.IIf(rst_0.FirstOrDefault() > 0, Strings.Format(rst_0.FirstOrDefault(), "##,# ريال بدهكار"), Strings.Format(rst_0.FirstOrDefault() * -1, "##,# ريال بستانكار"));
                    (report.GetComponentByName("MANDAH") as StiText).Text = _mandah.ToString();
                }
            }



            double JCHK = default, HAZ, NAGHD, VAR, HAV, taf, MBA, JAMF;
            double GB;
            var rst_3 = dbms.DoGetDataSQL<RPT_MODEL2>("SELECT dbo.PAY_GETD.N_SERI, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.DATE, dbo.PAY_GETD.DATE_S , dbo.PAY_GETD.MABL, dbo.PAY_GETD.NUMBER, dbo.PAY_GETD.TAG FROM         dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK WHERE (dbo.PAY_GETD.NUMBER = " + NUMBER.Text + ") AND (dbo.PAY_GETD.N_KOL IS NULL OR N_KOL <> 911) AND (dbo.PAY_GETD.TAG = " + HTAG + ")").ToList(); //Forms(FRF)["Dtag"]
            if (rst_3.Count > 0)
            {
                JCHK = 0d;
                (report.GetComponentByName("COMM") as StiText).Text = "چكهاي دريافت شده " + rst_3.Count + " فقره جمعاًبه مبلغ :" + Strings.Format(Convert.ToInt64(NCHK.Text), "### ريال") + "  ";

                for (int o = 0; o < rst_3.Count; o++) //while (!rst_3.EOF())
                {
                    (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + "ـ سريال:" + rst_3[o].N_SERI + " بانك:" + rst_3[o].NAMES + " شعبه:" + Strings.Trim(rst_3[o].SHOBEH);
                    JCHK = (double)(JCHK + rst_3[o].MABL);
                }
            }
            else
            {
                (report.GetComponentByName("COMM") as StiText).Enabled = false;
            }

            JAMF = 0d;
            HAZ = 0d;
            NAGHD = 0d;
            VAR = 0d;
            HAV = 0d;
            taf = 0d;
            MBA = 0d;
            var JST0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER.Text + $" ) AND ((INVO_LST.TAG)={HTAG}))").ToList();
            if (JST0.Count > 0 && !IsNull(JST0.FirstOrDefault()))
            {
                JAMF = (double)JST0.FirstOrDefault();
            }
            var JST = dbms.DoGetDataSQL<RPT_MODEL3>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG AS htag, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.MBAA FROM HEAD_LST WHERE (((HEAD_LST.NUMBER)= " + NUMBER.Text + $" ) AND  ((HEAD_LST.TAG)={HTAG}))").ToList();
            if (JST.Count > 0 && !IsNull(JST.FirstOrDefault().NUMBER))
            {
                HAZ = (double)JST.FirstOrDefault().MABL_HAZ;
                VAR = (double)JST.FirstOrDefault().MABL_VAR;
                HAV = (double)JST.FirstOrDefault().MABL_HAV;
                NAGHD = (double)JST.FirstOrDefault().M_NAGHD;
                taf = (double)JST.FirstOrDefault().TAKHFIF;
                MBA = (double)JST.FirstOrDefault().MBAA;
            }

            GB = JAMF - HAZ + MBA - taf;

            if (taf == 0)
            {
                (report.GetComponentByName("TF") as StiText).Enabled = false;
            }
            else
            {
                (report.GetComponentByName("TF") as StiText).Enabled = true;
                (report.GetComponentByName("TF") as StiText).Text = taf.ToString();
            }

            (report.GetComponentByName("JF") as StiText).Text = JAMF.ToString();
            (report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString();
            (report.GetComponentByName("MBAA") as StiText).Text = MBA.ToString();
            (report.GetComponentByName("GABEL") as StiText).Text = (JAMF + HAZ + MBA - taf).ToString();
            (report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString();

            (report.GetComponentByName("MAN") as StiText).Text = (JAMF + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)).ToString();



            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("TXT_FOROOSHANDEH") as StiText).Text = Baseknow.NAME; // نام فروشنده
            (report.GetComponentByName("TXT_ADDRESSF") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            (report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده
            (report.GetComponentByName("TXT_SHIFT") as StiText).Text = SHIFT.Text.ToString();
            (report.GetComponentByName("TXT_VAHEDY") as StiText).Text = DEPATMAN.Text.ToString();
            (report.GetComponentByName("TXT_KARBAR") as StiText).Text = USER_NAME.Text;
            (report.GetComponentByName("TheCUST_NO") as StiText).Text = CUST_NO.Text; //نام مشتری

            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(JAMF + HAZ + MBA - taf));


            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
        }
        private void Command108_Click(object sender, RoutedEventArgs e)
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
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_KHAD_2_MBA.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FactorMBA"]).CommandTimeout = 300;

            //var SUMMABL = (report.GetComponentByName("Text67") as StiText).Text;
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(SUM_OF_MABL_K));

            if ((report.GetComponentByName("DEPART") as StiText).Text == "" || IsNull((report.GetComponentByName("DEPART") as StiText).Text))
            {
                (report.GetComponentByName("DEPART") as StiText).Enabled = false;
                (report.GetComponentByName("DEPNAME") as StiText).Enabled = false;
            }
            (report.GetComponentByName("TheCUST_NO") as StiText).Text = CUST_NO.Text;

            //report.Render(false);
            //report.Show();

            new Rpts.WINRPT(report, "فاکتور خدمات").Show();
        }

        private void CUST_KIND_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //CUST_KIND_AfterUpdate
            SET_SPECIAL_TAKHFIF();
        }
        private void CUST_NO_AfterUpdate()
        {
            if (Convert.ToDouble(Strings.Mid(Baseknow.OPTIONSS, 19, 1)) == 5d)
            {
                var rst = dbms.DoGetDataSQL<HES_QRE>("SELECT     hes, CUST_COD FROM dbo.CUST_HESAB WHERE     (hes = N'" + CUST_NO.SelectedValue + "')").FirstOrDefault();
                if (!(rst is null))
                {
                    CUST_KIND.SelectedValue = null;
                    CUST_KIND.SelectedValue = rst.CUST_COD;
                    CUST_KIND.Items.Refresh();
                }
            }
        }
        private void SET_SPECIAL_TAKHFIF()
        {
            if (CUST_KIND.SelectedValue != null && !NewRecord)
            {
                CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), HTAG);
                CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), HTAG, Convert.ToInt64(M_NAGHD.Text), Convert.ToInt64(MABL_VAR.Text), Convert.ToInt64(MABL_HAV.Text), Convert.ToBoolean(this.TICMBAA.IsChecked));
            }
        }

        bool LastTICMBAAChecked;
        private void TICMBAA_Click(object sender, RoutedEventArgs e)
        {
            if (!isSavedSuccess)
            {
                return;
            }

            var SMBAA = default(double);
            if (!NewRecord)
            {
                if (TICMBAA.IsChecked is true)
                {
                    LastTICMBAAChecked = (bool)TICMBAA.IsChecked;

                    var rst = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST WHERE NUMBER = " + this.NUMBER.Text + $" AND TAG = {HTAG}").ToList();
                    var _where = " WHERE NUMBER = " + this.NUMBER.Text + $" AND TAG = {HTAG}";
                    for (int i = 0; i < rst.Count; i++)
                    {
                        var RST2 = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + rst[i].CODE + "'").FirstOrDefault();
                        if (!(RST2 is null))
                        {
                            if ((bool)RST2.CMBAA)
                            {
                                rst[i].IMBAA = Math.Round((double)((rst[i].MABL_K - rst[i].N_MOIN) * CL_HESABDARI.GetArzesh(rst[i].CODE) / 100));
                                SMBAA = SMBAA + Math.Round((double)((rst[i].MABL_K - rst[i].N_MOIN) * CL_HESABDARI.GetArzesh(rst[i].CODE) / 100));
                            }
                            else
                            {
                                rst[i].IMBAA = 0;
                            }
                        }
                        dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET IMBAA = {rst[i].IMBAA} {_where} AND id = {rst[i].id} ");
                    }
                    if (SMBAA != Convert.ToDouble(MBAA.Text) && SMBAA > 0d)
                    {
                        this.MBAA.Text = SMBAA.ToString();
                        this.HMBAA.Text = Baseknow.HESMBAA;
                    }
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT IMBAA FROM dbo.INVO_LST WHERE NUMBER = " + this.NUMBER.Text + $" AND TAG = {HTAG}").ToList();
                    var _where = " WHERE NUMBER = " + this.NUMBER.Text + " AND TAG = 14";
                    for (int i = 0; i < rst.Count; i++)
                    {
                        rst[i].IMBAA = 0;
                        dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET IMBAA = {0} {_where} ");
                    }
                    if (Convert.ToDouble(MBAA.Text) > 0)
                    {
                        this.MBAA.Text = "0";
                        this.HMBAA.Text = null;
                    }
                }
                if (this.TICMBAA.IsChecked is false)
                {
                    this.HMBAA.IsReadOnly = false;
                }
                else
                {
                    this.MBAA.IsReadOnly = true;
                    this.HMBAA.IsReadOnly = true;
                }
                if (sender != null)
                {
                    BTN_SAVE_Click(null, null);
                }

                INVO_LST_SUB_ReGetData();
            }
            else
            {
                //e.Handled = true;
                //TICMBAA.IsChecked = LastTICMBAAChecked;
            }
        }

        private void MABL_VAR_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_VAR_AfterUpdate
            if (MOIN_VAR.Text != "0")
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT Min(DETA_HES.NUMBER) AS MinOfNUMBER FROM DETA_HES WHERE (((DETA_HES.N_KOL)= " + Baseknow.BANKHA + "))").FirstOrDefault();
                if (RST != null)
                {
                    MOIN_VAR.Text = Baseknow.BANKHA + "-1-1";
                }
                else
                {
                    new Msgwin(false, "حساب معين براي خدمات تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل بانكها معين تعريف نمائيد.").ShowDialog();
                }
            }
        }

        private void MABL_HAV_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_HAV_AfterUpdate
            if (!string.IsNullOrEmpty(this.MOIN_HAV.Text) && this.MABL_HAV.Text != "0")
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT Min(DETA_HES.NUMBER) AS MinOfNUMBER FROM DETA_HES WHERE (((DETA_HES.N_KOL)= " + Baseknow.HAVALAH + "))").FirstOrDefault();
                if (RST != null)
                {
                    MOIN_HAV.Text = Baseknow.BANKHA + "-1-1";
                }
                else
                {
                    new Msgwin(false, "حساب معين براي خدمات تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل بانكها معين تعريف نمائيد.").ShowDialog();
                }
            }
        }

        private void SPER_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            return;
            if (!string.IsNullOrEmpty(SPER.Text))
            {
                var (isvalid, msg) = CL_LMethods.IsValidPercentage(SPER.Text);
                if (!isvalid)
                {
                    new Msgwin(false, msg).ShowDialog();
                }
                else
                {
                    //SPER_AfterUpdate
                    MABL_HAZ.Text = Math.Round(SUM_OF_MABL_K / 100 * Convert.ToDouble(SPER.Text)).ToString();
                    if (MABL_HAZ.Text != "0" && IsNull(this.MOIN_HAZ.Text))
                    {
                        new Msgwin(false, "حساب مربوط به سرويس مشخص نشده است حتما بايد حساب مربوط به سرويس مشخص شود ").ShowDialog();
                        this.MOIN_HAZ.Focus();
                    }
                }
            }
        }
        private void MABL_HAZ_FRONT_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            this.SPER.Text = (Convert.ToDouble(MABL_HAZ.Text) * 100 / SUM_OF_MABL_K).ToString();
            if (this.MABL_HAZ.Text != "0" && IsNull(this.MOIN_HAZ.Text))
            {
                new Msgwin(false, "حساب مربوط به سرويس مشخص نشده است حتما بايد حساب مربوط به سرويس مشخص شود ").ShowDialog();
                this.MOIN_HAZ.Focus();
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
    }
}
