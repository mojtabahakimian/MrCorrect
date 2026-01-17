using Functions;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.Taarif;
using Prg_UI.Wins.WinOther;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Controls.PivotGrid;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wins.WinOther;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;

namespace Wins.WinMenus.Taarif
{
    public partial class STUF_DEF_WIN : Window, INavigator, ISearchableWindow
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
        enum Jahat
        {
            FirstItem,
            BackItem,
            NextItem,
            LastItem,
            NewItem,
            CustomPosition
        }
        public class _KALA_QRE_0
        {
            public string? N_FANI { get; set; }
            public string? CODE { get; set; }
        }
        public class _KALA_QRE_1
        {
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
        }
        public class _KALA_QRE_2
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
            public string? ANB_KIND { get; set; }
        }
        public class _KALA_QRE_3
        {
            public double? CODE { get; set; }
            public string? NAMES { get; set; }
        }
        public class _KALA_QRE_4
        {
            public int? PGID { get; set; }
            public string? PGNAME { get; set; }
        }
        public class _KALA_QRE_5
        {
            public int? CODE { get; set; }
            public int? KIND { get; set; }
        }
        public class _KALA_QRE_6
        {
            public string? CODE { get; set; }
            public int? IDD { get; set; }
        }
        #endregion

        public STUF_DEF_WIN(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
            }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<STUF_FSK> FSK_DATA { get; set; } = new ObservableCollection<STUF_FSK>();
        public ObservableCollection<MODULE_D> MODULE_D_DATA { get; set; } = new ObservableCollection<MODULE_D>();
        public ObservableCollection<TAKHPERS> TAKHPERS_DATA { get; set; } = new ObservableCollection<TAKHPERS>();
        public ObservableCollection<RewardRules> REWARDS_DATA { get; set; } = new ObservableCollection<RewardRules>();

        public CollectionViewSource RecordsData { get; set; } = new CollectionViewSource();
        public double? NUMBER_TO_OPEN { get; set; }
        public bool NowIsReady { get; private set; }
        public Visual I_AM_STUF_DEF { get; private set; }

        /// <summary>
        /// آیدی کالا و اگر پر باشه یعنی ذخیره شده و اگر خالی باشه یعنی جدیده
        /// </summary>
        public long? MASTER_IDD { get; set; } = null;

        public short? KINDK { get; set; } = 1;

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                //if (string.IsNullOrEmpty(CODE.Text))
                //{

                //}
                return _newrecord;
            }
            set { _newrecord = value; }
        }
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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;
                if (_ican is true) // Is Enable and ReadOnly = False
                {
                    VAHED.IsEnabled = true; // واحد
                    RADAH.IsEnabled = true; //گروه کالا
                    CMBAA.IsEnabled = true; // مشمول مالیات ب.ا.ا

                    if (MASTER_IDD != null)
                    {
                        STUF_FSK_sub.IsEnabled = true; //کالا در انبار
                        MODULE_D_SUB.IsEnabled = true; //سایر واحد ها
                        TAKHPERS_SUB.IsEnabled = true; //تخفیفات پیشرفته
                        INVOICE_REWARDS_SUB.IsEnabled = true;
                    }

                    MENUIT.IsEnabled = true;
                    NBARCODE_BTN.IsEnabled = true;

                    VAZN.IsReadOnly = false;
                    MIN_M.IsReadOnly = false;
                    MEGHTA.IsReadOnly = false;
                    MEGHJAY.IsReadOnly = false;

                    PGID.IsEnabled = true; //گروه قیمت گذاری
                    mu.IsEnabled = true; //واحد مودیان
                    SAVE_KALA.IsEnabled = true; //ذخیره
                    //Command39.IsEnabled = true; //مرتب سازی
                    Command50.IsEnabled = true; //کدینگ فنی
                    Command55.IsEnabled = true; //لیست تخفیفات
                    NBARCODE_BTN.IsEnabled = true;

                    CODE.IsReadOnly = false; // کد کالا
                    NAM.IsReadOnly = false; //نام کالا
                    N_FANI.IsReadOnly = false; //شماره فنی
                    Barcode.IsReadOnly = false; //بارکد
                    TOZIH.IsReadOnly = false; //توضیح
                    sstid.IsReadOnly = false; //شناسه کالا
                    N_SEF.IsReadOnly = false; //نقطه سفارش
                    MABL_F.IsReadOnly = false; //فی عمده فروش
                    B_SEF.IsReadOnly = false; //فی خرده فروش
                    MAX_M.IsReadOnly = false; //قیمیت مصرف کننده
                    vra.IsReadOnly = false; //درصد مودیان
                }
                else
                {
                    NBARCODE_BTN.IsEnabled = false;
                    VAHED.IsEnabled = false; // واحد
                    RADAH.IsEnabled = false; //گروه کالا
                    CMBAA.IsEnabled = false; // مشمول مالیات ب.ا.ا
                    STUF_FSK_sub.IsEnabled = false; //کالا در انبار
                    MODULE_D_SUB.IsEnabled = false; //سایر واحد ها
                    TAKHPERS_SUB.IsEnabled = false; //تخفیفات پیشرفته
                    INVOICE_REWARDS_SUB.IsEnabled = false; //شروط جوایز


                    PGID.IsEnabled = false; //گروه قیمت گذاری
                    mu.IsEnabled = false; //واحد مودیان
                    SAVE_KALA.IsEnabled = false; //ذخیره
                    //Command39.IsEnabled = false; //مرتب سازی
                    Command50.IsEnabled = false; //کدینگ فنی
                    Command55.IsEnabled = false; //لیست تخفیفات

                    MENUIT.IsEnabled = false;

                    VAZN.IsReadOnly = true;
                    MIN_M.IsReadOnly = true;
                    MEGHTA.IsReadOnly = true;
                    MEGHJAY.IsReadOnly = true;

                    CODE.IsReadOnly = true; // کد کالا
                    NAM.IsReadOnly = true; //نام کالا
                    N_FANI.IsReadOnly = true; //شماره فنی
                    Barcode.IsReadOnly = true; //بارکد
                    TOZIH.IsReadOnly = true; //توضیح
                    sstid.IsReadOnly = true; //شناسه کالا
                    N_SEF.IsReadOnly = true; //نقطه سفارش
                    MABL_F.IsReadOnly = true; //فی عمده فروش
                    B_SEF.IsReadOnly = true; //فی خرده فروش
                    MAX_M.IsReadOnly = true; //قیمیت مصرف کننده
                    vra.IsReadOnly = true; //درصد مودیان
                }
            }
        }

        public string? FSK_ENTERED_VALUE_ROW { get; private set; }
        public STUF_FSK? FSK_CURRENT_ROW_ITEMS { get; private set; }
        public STUF_FSK? FSK_WAS_ROW_ITEM { get; private set; }
        public bool STUF_FSK_sub_IsFocusedIn { get; private set; }
        public bool ChangeIsHappend { get; private set; } = false;

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (STUF_FSK_sub.Columns.Count > 0)
                {
                    int? defaultcolumnindex = STUF_FSK_sub.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _name_code_index = 0;
                    }
                    else
                    {
                        _name_code_index = (int)defaultcolumnindex;
                    }
                }
                return _name_code_index;
            }
        }

        private int decription_IDEX_COL;
        public int QT_IDEX_COL
        {
            get
            {
                if (INVOICE_REWARDS_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVOICE_REWARDS_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "Quantity_Threshold")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        decription_IDEX_COL = 0;
                    }
                    else
                    {
                        decription_IDEX_COL = (int)defaultcolumnindex;
                    }
                }
                return decription_IDEX_COL;
            }
        }

        public MODULE_D? MODULE_D_WAS_ROW_ITEM { get; set; }
        public MODULE_D? MODULE_D_CURRENT_ROW_ITEMS { get; private set; }
        public string? MODULE_D_ENTERED_VALUE_ROW { get; private set; }

        public TAKHPERS? TAKHPERS_CURRENT_ROW { get; private set; }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => RecordsData;

        public void OnSearchResultSelected(object selectedItem)
        {
            if (selectedItem is not STUF_DEF target || RecordsData?.View is null)
            {
                return;
            }

            var itemfound = RecordsData.View.Cast<STUF_DEF>()
                .FirstOrDefault(x => string.Equals(x.CODE, target.CODE, StringComparison.OrdinalIgnoreCase));

            if (itemfound != null)
            {
                RecordsData.View.MoveCurrentTo(itemfound);
                MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
            }
            else
            {
                MoveReGetData(INavigator.Jahat.LastItem);
            }
        }

        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "کد کالا", PropertyPath = nameof(STUF_DEF.CODE), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "نام کالا", PropertyPath = nameof(STUF_DEF.NAME), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "توضیحات", PropertyPath = nameof(STUF_DEF.TOZIH), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "واحد", PropertyPath = nameof(STUF_DEF.VAHED), PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "حداقل موجودی", PropertyPath = nameof(STUF_DEF.MIN_M), PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "حداکثر موجودی", PropertyPath = nameof(STUF_DEF.MAX_M), PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "بارکد", PropertyPath = nameof(STUF_DEF.BARCODE), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "نوع", PropertyPath = nameof(STUF_DEF.KINDK), PropertyType = typeof(short?) },
                new SearchableProperty { DisplayName = "فی عمده", PropertyPath = nameof(STUF_DEF.MABL_F), PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "فی خرده", PropertyPath = nameof(STUF_DEF.B_SEF), PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "گروه قیمت", PropertyPath = nameof(STUF_DEF.PGID), PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "شناسه مودیان", PropertyPath = nameof(STUF_DEF.sstid), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کد فنی", PropertyPath = nameof(STUF_DEF.N_FANI), PropertyType = typeof(string) },
            };
        }
        #endregion

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;

            I_AM_STUF_DEF = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //Form_Load
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "KALA", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            //Form_Current For Sub
            CL_HESABDARI.SETSECURITYSUB(STUF_FSK_sub, "KALA"); //دسترسی به انبار و ابتدای دوره

            CL_HESABDARI.SETSECURITYSUB(MODULE_D_SUB, "KALA"); //دسترسی به سایر واحد ها

            CL_HESABDARI.SETSECURITYSUB(TAKHPERS_SUB, "KALA"); //دسترسی به تخفیفات

            //Form_Open
            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {
                Frame58.Visibility = Visibility.Visible;
                MEGHTA.Visibility = Visibility.Visible;
                MEGHJAY.Visibility = Visibility.Visible;
                Label54.Visibility = Visibility.Visible;
            }
            else
            {
                Frame58.Visibility = Visibility.Hidden;
                MEGHTA.Visibility = Visibility.Hidden;
                MEGHJAY.Visibility = Visibility.Hidden;
                Label54.Visibility = Visibility.Hidden;
            }
            //if (!IsLoaded("stuf_def_list"))
            //{
            //    DoCmd.GoToRecord(acDataForm, this.NAME, acNewRec);
            //}

            FILL_ALL_COMBOBOXES();

            ReGetMasterData();

            if (MASTER_IDD is null)
            {
                ActivateDataGrids(false);
            }

            NAM.Focus();

            ChangeIsHappend = false;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = STUF_FSK_sub;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (STUF_FSK_sub_IsFocusedIn)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int DefaultColumnIndex = CL_LMethods.GetLastColumn(STUF_FSK_sub).DisplayIndex;
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

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[NAME_CODE_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    else if (INVOICE_REWARDS_SUB.IsKeyboardFocusWithin)
                    {
                        if (INVOICE_REWARDS_SUB.CurrentColumn != null)
                        {
                            int DefaultColumnIndex = CL_LMethods.GetLastColumn(INVOICE_REWARDS_SUB).DisplayIndex;
                            int currentColumnIndex = INVOICE_REWARDS_SUB.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == INVOICE_REWARDS_SUB.Columns.Count - 1;
                            bool isLastRow = INVOICE_REWARDS_SUB.SelectedIndex == INVOICE_REWARDS_SUB.Items.Count - 2; //Last Row that is new Empty
                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    INVOICE_REWARDS_SUB.SelectedIndex++; // INVOICE_REWARDS_SUB.SelectedIndex = INVOICE_REWARDS_SUB.Items.Count - 1;

                                    INVOICE_REWARDS_SUB.CurrentCell = new DataGridCellInfo(INVOICE_REWARDS_SUB.SelectedItem, INVOICE_REWARDS_SUB.Columns[QT_IDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        INVOICE_REWARDS_SUB.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }

                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            catch { /*ignore*/ }

            bool isDataGridFocused =
              (STUF_FSK_sub?.IsKeyboardFocusWithin ?? false) || (STUF_FSK_sub?.IsFocused ?? false) ||
              (MODULE_D_SUB?.IsKeyboardFocusWithin ?? false) || (MODULE_D_SUB?.IsFocused ?? false) ||
              (TAKHPERS_SUB?.IsKeyboardFocusWithin ?? false) || (TAKHPERS_SUB?.IsFocused ?? false) ||
              (INVOICE_REWARDS_SUB?.IsKeyboardFocusWithin ?? false) || (INVOICE_REWARDS_SUB?.IsFocused ?? false);

            if (!isDataGridFocused && e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                var searchWindow = new EnhancedSearchWindow(this);
                searchWindow.Owner = this;
                searchWindow.ShowDialog();
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
        private void FILL_ALL_COMBOBOXES()
        {
            //انبار کالا
            ANBAR_COLUMN.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_2>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, TCOD_ANBAR_KIND.ANB_KIND FROM TCOD_ANBAR_KIND  INNER JOIN TCOD_ANBAR ON TCOD_ANBAR_KIND.CODE=TCOD_ANBAR.KIND WHERE(TCOD_ANBAR.CODE>0) ORDER BY TCOD_ANBAR.NAMES").ToList();

            //واحد
            VAHED.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //گروه کالا
            RADAH.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_3>("SELECT TCOD_STUFGROUP.CODE, TCOD_STUFGROUP.NAMES FROM TCOD_STUFGROUP WHERE (((TCOD_STUFGROUP.CODE)<>0)) ORDER BY TCOD_STUFGROUP.NAMES").ToList();

            //گروه قیمتی
            PGID.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_4>("SELECT PGID, PGNAME FROM PRICE_GRP").ToList();

            //زیر مجموعه منو
            MENUIT.ItemsSource = dbms.DoGetDataSQL<TCODE_MENUITEM>("SELECT CODE, NAMES FROM dbo.TCODE_MENUITEM").ToList();

            //واحد فرعی
            //VAHED_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
            VAHED_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //نوع کد مشتری
            CUST_CO_COLUMN.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM dbo.CUSTKIND").ToList();

            //واحد مودیان
            mu.ItemsSource = dbms.DoGetDataSQL<TCOD_VAHED_EXTENDED>("SELECT IDD, NAME_MO FROM TCOD_VAHED_EXTENDED ORDER BY NAME_MO").ToList();

            ////جایزه____________________________________________________________________________________________________________________________________
            //کالای جایزه
            Reward_ProductID_COLUMN.ItemsSource = dbms.DoGetDataSQL<STUF_DEF>($"SELECT CODE, NAME FROM STUF_DEF ORDER BY STUF_DEF.NAME").ToList();

            //نوع جایزه
            Reward_Type_COLUMN.ItemsSource = new List<ComboBoxItemData>
            {
                new ComboBoxItemData{ Value = "Discount", Display = "تخفیف" },
                new ComboBoxItemData{ Value = "Product", Display = "محصول" },
            };


        }
        private void ActivateDataGrids(bool _YN_)
        {
            STUF_FSK_sub.IsEnabled = _YN_;
            MODULE_D_SUB.IsEnabled = _YN_;
            TAKHPERS_SUB.IsEnabled = _YN_;
            INVOICE_REWARDS_SUB.IsEnabled = _YN_;
        }

        private async void LoadMatchingImageAsync(string productCode)
        {
            try
            {
                // First, check if the shared folder path exists to avoid unnecessary delay
                var pathExists = await Task.Run(() => Directory.Exists(Baseknow.BACKPATH));
                if (!pathExists)
                {
                    return;
                }

                // If the shared folder exists, proceed to search for the image file
                string imagePath = await Task.Run(() => CL_LMethods.FindImageFile(Baseknow.BACKPATH, productCode));
                if (!string.IsNullOrEmpty(imagePath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 200; // Adjust based on your UI needs
                    image.UriSource = new Uri(imagePath);
                    image.EndInit();
                    image.Freeze(); // Improve performance for UI binding

                    // Assuming 'ProductImage' is an Image control in XAML
                    Dispatcher.Invoke(() => PIC.Source = image);
                }
            }
            catch { }
        }
        public void Form_Current()
        {
            PIC.Source = null;
            if (!this.NewRecord)
            {
                if (string.IsNullOrEmpty(OKF.IsChecked?.ToStringNullSafe()) || OKF.IsChecked == false)
                {
                    this.OKF.IsChecked = true;
                }

                if (!string.IsNullOrEmpty(Baseknow.BACKPATH))
                {
                    LoadMatchingImageAsync(CODE.Text);
                }

            }
            if (!this.NewRecord)
            {
                if (string.IsNullOrEmpty(OKF.IsChecked?.ToStringNullSafe()) || OKF.IsChecked == false)
                {
                    this.OKF.IsChecked = true;
                }
            }
            if (OKF.IsChecked ?? false)
            {
                AllowDeletions = false;
                AllowEdits = false;
                STUF_FSK_sub.IsEnabled = false;
                STUF_FSK_sub.IsEnabled = false;
                MODULE_D_SUB.IsEnabled = false;
                ESLAH.IsEnabled = true;
            }
            else
            {
                AllowDeletions = true;
                AllowEdits = true;
                STUF_FSK_sub.IsEnabled = true;
                STUF_FSK_sub.IsEnabled = true;
                MODULE_D_SUB.IsEnabled = true;
                ESLAH.IsEnabled = true;
            }


        }
        private bool CodeKalaExist()
        {
            if (!string.IsNullOrEmpty(CODE.Text)) //اگر خالی نیست
            {
                var KALA = dbms.DoGetDataSQL<_KALA_QRE_6>($"SELECT TOP 1 CODE,IDD FROM dbo.STUF_DEF WHERE CODE = N'{CODE.Text}'").FirstOrDefault();

                //CODE_AfterUpdate
                if (MASTER_IDD is not null) //آیدی دارد (Update)
                {
                    if (KALA != null)
                    {
                        if (KALA.IDD != MASTER_IDD) //اگر کدی که پیدا کردی با کد کالایی که ذخیره شده فرق داره
                        {
                            return true; //Code is Duplicate !
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(CODE.Text)) //Insert
                {
                    if (KALA != null)
                    {
                        return true; //Code is Duplicate !
                    }
                }
            }

            return false;
        }
        private void GetMaxKalaCode()
        {
            //Form_BeforeUpdate
            if (string.IsNullOrEmpty(CODE.Text) || CODE.Text == "0")
            {
                var _row = dbms.DoGetDataSQL<string?>("SELECT Max(CAST([CODE] as int)) AS Expr1 FROM dbo.STUF_DEF").FirstOrDefault();
                if (_row is null)
                {
                    CODE.Text = "1";
                }
                else
                {
                    CODE.Text = Convert.ToString(Convert.ToInt32(_row) + 1);
                }
            }
            //
        }

        public void ReGetMasterData()
        {
            var MasterHead = dbms.DoGetDataSQL<STUF_DEF>($"SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra FROM dbo.STUF_DEF ORDER BY CRT").ToList();
            RecordsData.Source = MasterHead;

            if (NUMBER_TO_OPEN != null)
            {
                var item = RecordsData.View.Cast<STUF_DEF>().FirstOrDefault(x => x.CODE.Equals(NUMBER_TO_OPEN.ToString()));
                if (item != null)
                {
                    // Set the CurrentItem to the found item
                    RecordsData.View.MoveCurrentTo(item);

                    MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View?.CurrentPosition);
                }
            }
            else
            {
                MoveReGetData(INavigator.Jahat.LastItem);
            }
        }
        public void MoveReGetData(INavigator.Jahat jahat, int? custom_postiion = null)
        {
            int RecordCount()
            {
                return ((System.Windows.Data.ListCollectionView)RecordsData.View)?.Count ?? 0;
            }
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

            if (NewRecord && !ConfirmExitWithoutSaving())
            {
                return;
            }

            switch (jahat)
            {
                case INavigator.Jahat.FirstItem: //اولین
                    NewRecord = false;
                    RecordsData.View.MoveCurrentToFirst();
                    break;
                case INavigator.Jahat.BackItem: //قبلی
                    if (RecordsData.View.CurrentPosition > 0) //Possible To Back
                    {
                        if (NewRecord)
                        {
                            jahat = INavigator.Jahat.LastItem;
                            RecordsData.View.MoveCurrentToLast();
                        }
                        else
                        {
                            RecordsData.View.MoveCurrentToPrevious();
                        }
                        NewRecord = false;
                    }
                    break;

                case INavigator.Jahat.NextItem: //بعدی
                    if (RecordsData.View.CurrentPosition < RecordCount() - 1)  //[ RecordCount() - 1 ] : just ensure that stand on existing real item
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToNext();
                    }
                    break;

                case INavigator.Jahat.LastItem: //آخرین
                    RecordsData.View.MoveCurrentToLast();
                    break;

                case INavigator.Jahat.CustomPosition:
                    if (custom_postiion > -1)
                    {
                        NewRecord = false;
                        RecordsData.View.MoveCurrentToPosition((int)custom_postiion);
                    }
                    break;

                case INavigator.Jahat.NewItem: //جدید خالی
                    NewRecord = true;
                    RecordsData.View.MoveCurrentToLast();
                    ClearFreshNew();
                    break;
            }

            //Update CurrentViewItem
            if (RecordsData.View.CurrentItem != null)
            {
                var HEADER = RecordsData.View.CurrentItem as STUF_DEF;
                var DBData = dbms.DoGetDataSQL<STUF_DEF>($"SELECT IDD,CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra FROM dbo.STUF_DEF WHERE IDD = {HEADER.IDD}").FirstOrDefault();
                if (HEADER != null && DBData != null)
                {
                    var properties = typeof(STUF_DEF).GetProperties();
                    foreach (var property in properties)
                    {
                        if (property.CanWrite)
                        {
                            var value = property.GetValue(DBData);
                            property.SetValue(HEADER, value);
                        }
                    }
                    RecordsData.View.Refresh();
                }
            }


            DisplayCounts();

            if (RecordCount() == 0)
                NEWRECORD_BTN.IsEnabled = false;
            else
                NEWRECORD_BTN.IsEnabled = true;

            int RDCount = RecordsData.View != null ? RecordsData.View.Cast<object>().Count() : 0;
            if (jahat == INavigator.Jahat.NewItem || RDCount == 0)
            {
                ClearFreshNew();

                OKF.IsChecked = false; //تیک تایید
                Form_Current();

                ActivateDataGrids(false);
            }
            else
            {
                UiDataUpdate();
            }
        }
        public void ClearFreshNew()
        {
            MASTER_IDD = null;

            FSK_DATA.Clear();
            MODULE_D_DATA.Clear();
            TAKHPERS_DATA.Clear();
            REWARDS_DATA?.Clear();

            VAHED.SelectedIndex = -1; VAHED.Items.Refresh(); // واحد
            RADAH.SelectedIndex = -1; RADAH.Items.Refresh(); //گروه کالا
            PGID.SelectedIndex = -1; PGID.Items.Refresh(); //گروه قیمت گذاری
            MENUIT.SelectedIndex = -1; MENUIT.Items.Refresh(); //گروه قیمت گذاری

            CMBAA.IsChecked = false; // مشمول مالیات ب.ا.ا

            mu.SelectedIndex = -1; //واحد مودیان

            CODE.Text = null; // کد کالا
            NAM.Text = null; //نام کالا
            N_FANI.Text = null; //شماره فنی
            Barcode.Text = null; //بارکد
            TOZIH.Text = null; //توضیح
            sstid.Text = null; //شناسه کالا
            N_SEF.Text = "0"; //نقطه سفارش
            MABL_F.Text = "0"; //فی عمده فروش
            B_SEF.Text = "0"; //فی خرده فروش
            MAX_M.Text = "0"; //قیمیت مصرف کننده
            vra.Text = "0"; //درصد مودیان

            MIN_M.Text = "0";
            VAZN.Text = "0";

            MEGHTA.Text = "0";
            MEGHJAY.Text = "0";

            NBARCODE_BTN.IsEnabled = true;

        }
        public void UiDataUpdate()
        {
            if (RecordsData.View?.CurrentItem is not null) //Load Master data
            {
                var HEADER = RecordsData.View.CurrentItem as STUF_DEF;

                MASTER_IDD = HEADER.IDD;

                VAHED.SelectedValue = HEADER.VAHED; VAHED.Items.Refresh(); // واحد
                RADAH.SelectedValue = HEADER.RADAH; RADAH.Items.Refresh(); //گروه کالا
                PGID.SelectedValue = HEADER.PGID; PGID.Items.Refresh(); //گروه قیمت گذاری
                MENUIT.SelectedValue = HEADER.MENUIT; MENUIT.Items.Refresh(); //گروه قیمت گذاری

                CMBAA.IsChecked = Convert.ToBoolean(HEADER.CMBAA); // مشمول مالیات ب.ا.ا

                OKF.IsChecked = HEADER.OKF;

                mu.SelectedValue = HEADER.mu; //واحد مودیان

                CODE.Text = HEADER.CODE; // کد کالا
                NAM.Text = HEADER.NAME; //نام کالا
                N_FANI.Text = HEADER.N_FANI; //شماره فنی
                Barcode.Text = HEADER.BARCODE; //بارکد
                TOZIH.Text = HEADER.TOZIH; //توضیح
                sstid.Text = HEADER.sstid; //شناسه کالا
                N_SEF.Text = HEADER.N_SEF.ToStringNullSafe(); //نقطه سفارش
                MABL_F.Text = HEADER.MABL_F.ToStringNullSafe(); //فی عمده فروش
                B_SEF.Text = HEADER.B_SEF.ToStringNullSafe(); //فی خرده فروش
                MAX_M.Text = HEADER.MAX_M.ToStringNullSafe(); //قیمیت مصرف کننده
                vra.Text = HEADER.vra.ToStringNullSafe(); //درصد مودیان

                MIN_M.Text = HEADER.MIN_M.ToStringNullSafe();
                VAZN.Text = HEADER.VAZN.ToStringNullSafe();

                MEGHTA.Text = HEADER.MEGHTA.ToStringNullSafe();
                MEGHJAY.Text = HEADER.MEGHJAY.ToStringNullSafe();


                ReGetData(); //Load DataGrid's data
                MODULE_D_ReGetData();
                TAKHPERS_ReGetData();
                REWARDS_ReGetData();

                if (MASTER_IDD != null)
                {
                    Form_Current();
                }
            }
        }

        private void REWARDS_ReGetData()
        {
            REWARDS_DATA?.Clear();
            var data = dbms.DoGetDataSQL<RewardRules>($"SELECT * FROM RewardRules WHERE ProductID_Target = N'{CODE.Text}' ").ToList();
            foreach (var item in data)
            {
                REWARDS_DATA?.Add(item);
            }
        }

        public bool ConfirmExitWithoutSaving()
        {
            Msgwin msgwin = new Msgwin(true, "آیتم جدید را ذخیره نکرده اید , آیا از خروج از این آیتم اطمینان دارید ؟");
            msgwin.ShowDialog();
            return msgwin.DialogResult == true;
        }
        public void RefreshAfterDelete()
        {
            var LastCurrentPosition = RecordsData.View.CurrentPosition;

            if (RecordsData.View.CurrentItem != null)
            {
                var itemToRemove = RecordsData.View.CurrentItem as STUF_DEF;
                if (itemToRemove != null)
                {
                    // Assuming the underlying collection is a List<T>, adjust if it's a different type
                    var underlyingCollection = RecordsData.Source as List<STUF_DEF>;
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
        public void RefreshAfterInsert()
        {
            var itemtoadd = dbms.DoGetDataSQL<STUF_DEF>($"SELECT IDD,CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra FROM dbo.STUF_DEF WHERE IDD = {MASTER_IDD}").FirstOrDefault();

            var underlyingCollection = RecordsData.Source as List<STUF_DEF>; // Assuming the underlying collection is a List<T>, adjust if it's a different type
            if (itemtoadd != null && underlyingCollection != null)
            {
                underlyingCollection.Add(itemtoadd);
                RecordsData.View.Refresh();
                RecordsData.View.MoveCurrentTo(itemtoadd);
                NewRecord = false;
                ////MoveReGetData(INavigator.Jahat.CustomPosition, RecordsData.View.CurrentPosition);
            }
        }

        private bool N_FaniIsDuplicate(bool _DisplayMsg = true)
        {
            //N_FANI_AfterUpdate
            if (!string.IsNullOrEmpty(N_FANI.Text) && !string.IsNullOrEmpty(CODE.Text))
            {
                var rst = dbms.DoGetDataSQL<_KALA_QRE_0>("SELECT N_FANI,CODE FROM STUF_DEF WHERE N_FANI = '" + N_FANI.Text + "'").FirstOrDefault();
                if (rst is not null)
                {
                    if (rst.CODE != CODE.Text)
                    {
                        if (_DisplayMsg)
                        {
                            new Msgwin(false, "كد فني تكراري است قبلا در كالاي با كد " + rst.CODE + " ثبت شده است!").ShowDialog();
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!string.IsNullOrEmpty(CODE.Text) && CODE.Text != "0")
            {
                //CODE_BeforeUpdate
                if (Convert.ToInt32(CODE.Text) > int.MaxValue)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "طول کد کالا غیر مجاز است" });
                }
            }
            if (N_FaniIsDuplicate(false))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد فنی تکراری است و قبلا در یک کالایی دیگری وارد شده" });
            }
            if (CodeKalaExist())
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد کالای وارد شده تکراری است" });
            }
            if (string.IsNullOrEmpty(NAM.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کالا وارد نشده !" });
            }
            if (VAHED.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد پیش فرض انتخاب نشده !" });
            }
            if (RADAH.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه کالا انتخاب نشده !" });
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

        private void SAVE_KALA_Click(object sender, RoutedEventArgs e) //SAVE -----------------------------------------------------------------------------------------------------------------------------------
        {
            if (!HeaderIsValid())
            {
                //اگر اطلاعات درست نیست خارج شو
                return;
            }

            //ذخیره سربرگ کالا ***
            #region HeaderDoSaveSuccess
            long? headeridd = null;
            try
            {
                OKF.IsChecked = true;

                GetMaxKalaCode(); //گرفتن کد کالا درصورت خالی بودن

                if (MASTER_IDD is null) //Insert
                {
                    headeridd = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.STUF_DEF(CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, mu, sstid, vra)
                                                  OUTPUT INSERTED.IDD
                                                  VALUES(N'{CODE.Text}',
                                                  N'{NAM.Text.FixPersianChars()}' ,
                                                  N'{N_FANI.Text}' ,
                                                  N'{TOZIH.Text.FixPersianChars()}' ,
                                                  {VAHED.SelectedValue},
                                                  {B_SEF.Text} ,
                                                  {N_SEF.Text} ,
                                                  {MIN_M.Text} ,
                                                  {MAX_M.Text} ,
                                                  {RADAH.SelectedValue} ,
                                                  {KINDK}   ,
                                                  {MABL_F.Text} ,
                                                  {Convert.ToByte(CMBAA.IsChecked)},
                                                  {VAZN.Text} ,
                                                  {Convert.ToByte(OKF.IsChecked)},
                                                  {(MENUIT.SelectedValue is null ? "NULL" : MENUIT.SelectedValue)} ,
                                                  {MEGHTA.Text} ,
                                                  {MEGHJAY.Text} ,
                                                  {(PGID.SelectedValue is null ? "NULL" : PGID.SelectedValue)} ,
                                                  N'{Barcode.Text}' ,
                                                  {(mu.SelectedValue is null ? "NULL" : mu.SelectedValue)} ,
                                                  N'{sstid.Text}' ,
                                                  {vra.Text} )").FirstOrDefault();

                    if (headeridd != null)
                    {
                        MASTER_IDD = headeridd;
                    }

                    RefreshAfterInsert();
                }
                else //Update
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.STUF_DEF
                                      SET CODE = N'{CODE.Text}', NAME = N'{NAM.Text.FixPersianChars()}', N_FANI = N'{N_FANI.Text}', 
                                      TOZIH = N'{TOZIH.Text.FixPersianChars()}', VAHED = {VAHED.SelectedValue}, B_SEF = {B_SEF.Text},
                                      N_SEF = {N_SEF.Text}, MIN_M = {MIN_M.Text}, MAX_M = {MAX_M.Text}, RADAH = {RADAH.SelectedValue},
                                      KINDK = {KINDK}, MABL_F = {MABL_F.Text}, CMBAA = {Convert.ToByte(CMBAA.IsChecked)}, VAZN = {VAZN.Text}, 
                                      OKF = {Convert.ToByte(OKF.IsChecked)}, MENUIT = {(MENUIT.SelectedValue is null ? "NULL" : MENUIT.SelectedValue)},
                                      MEGHTA = {MEGHTA.Text}, MEGHJAY = {MEGHJAY.Text}, PGID = {(PGID.SelectedValue is null ? "NULL" : PGID.SelectedValue)},
                                      BARCODE = N'{Barcode.Text}', mu = {(mu.SelectedValue is null ? "NULL" : mu.SelectedValue)}, sstid = N'{sstid.Text}', vra = {vra.Text}
                                      WHERE IDD = {MASTER_IDD} ");
                }
            }
            catch (SqlException ex)
            {
                OKF.IsChecked = false;
                //Msg 2601, Level 14, State 1, Line 1
                //Cannot insert duplicate key row in object 'dbo.STUF_DEF' with unique index 'NAME'.

                //Msg 2627, Level 14, State 1, Line 1
                //Violation of PRIMARY KEY constraint 'aaaaaSTUF_FSK_PK'. Cannot insert duplicate key in object 'dbo.STUF_FSK'.

                if (ex.Number == 2601)
                {
                    new Msgwin(false, "نام کالا تکراری است ! , کالایی دیگر با این نام وجود دارد!");
                }
                else if (ex.Number == 2627)
                {
                    new Msgwin(false, "کد کالا تکراری است کالایی دیگر با این کد وجود دارد!");
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            #endregion

            NewRecord = false; // at the end of opration


            NAM_AfterUpdate(); //ایجاد حساب های کالا

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            ReGetData();
            MODULE_D_ReGetData();
            TAKHPERS_ReGetData();

            ActivateDataGrids(true);

        }

        private void N_FANI_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //N_FANI_AfterUpdate
            if (N_FANI.Text == "+" || N_FANI.Text == "++")
            {
                //#Error↓
                //DoCmd.OpenForm("STUF_DEF_NEW", default, default, default, default, acDialog);
            }
            else
            {
                N_FaniIsDuplicate();
            }
        }

        private void NAM_AfterUpdate()
        {
            if (!this.NewRecord)
            {
                var RST2 = dbms.DoGetDataSQL<_KALA_QRE_1>("SELECT ANBAR , CODE FROM STUF_FSK WHERE CODE = '" + CODE.Text + "'").ToList();
                if (RST2.Count > 0)
                {
                    for (int i = 0; i < RST2.Count; i++) //while (!RST2.EOF())
                    {
                        var rst = dbms.DoGetDataSQL<string?>("SELECT  NAME FROM TDETA_HES WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + RST2[i].ANBAR + " AND TNUMBER = " + CODE.Text).FirstOrDefault();
                        if (rst is not null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}' WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + RST2[i].ANBAR + " AND TNUMBER = " + CODE.Text);
                            }
                            catch { }

                            //rst.Fields("NAME") = NAM.Text;
                            //rst.update();
                        }
                        //rst.Close();
                        //RST2.MoveNext();
                    }
                }
                if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                {
                    //rst.Close();
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = "فروش " + NAM.Text;
                        try
                        {
                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}' WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst.update();
                    }
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM DETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = "فروش " + NAM.Text;
                        try
                        {
                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}' WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text);
                        }
                        catch { }


                        //rst.update();
                    }
                    //rst.Close();
                    var rst1 = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst1 is not null)
                    {
                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}' WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst1.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL  = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}'  WHERE N_KOL  = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM DETA_HES WHERE N_KOL  = " + Baseknow.HAZ_TOL + " AND  NUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = "مواد مصرفي " + NAM.Text;
                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}'  WHERE N_KOL  = " + Baseknow.HAZ_TOL + " AND  NUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM DETA_HES WHERE N_KOL  =" + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = " قيمت تمام شده  " + NAM.Text;
                        try
                        {
                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}'  WHERE N_KOL  =" + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL  =" + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = " قيمت تمام شده  " + NAM.Text;

                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}'  WHERE N_KOL  =" + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = " + CODE.Text);
                        }
                        catch { }

                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM DETA_HES WHERE N_KOL  =" + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        //rst.Fields("NAME") = NAM.Text; //Forms["STUF_DEF"]["nam"];

                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}'   WHERE N_KOL  =" + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text);
                        }
                        catch { }

                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM DETA_HES WHERE N_KOL  =" + Baseknow.AMALKARD + " AND  NUMBER = " + CODE.Text).FirstOrDefault();
                    if (rst is not null)
                    {
                        var _NAME_ = " عملكرد  " + NAM.Text;
                        try
                        {

                            dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}'  WHERE N_KOL  =" + Baseknow.AMALKARD + " AND  NUMBER = " + CODE.Text);
                        }
                        catch { }
                        //rst.update();
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL  = " + Baseknow.HAZ_TOL + " And TNUMBER = " + CODE.Text).ToList();
                    if (rst is not null)
                    {
                        for (int i = 0; i < rst.Count; i++) // while (!rst.EOF())
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}'  WHERE N_KOL  = " + Baseknow.HAZ_TOL + " And TNUMBER = " + CODE.Text);
                            }
                            catch (Exception)
                            {
                                return;
                            }
                            //rst.MoveNext();
                        }
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL  = " + Baseknow.CONKAL + " And TNUMBER = " + CODE.Text).ToList();
                    if (rst is not null)
                    {
                        for (int i = 0; i < rst.Count; i++) //while (!rst.EOF())
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{NAM.Text}'  WHERE N_KOL  = " + Baseknow.CONKAL + " And TNUMBER = " + CODE.Text);
                            }
                            catch (Exception)
                            {
                                return;
                            }
                        }
                    }
                }
                //rst.Close();
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<string?>("SELECT NAME FROM TDETA_HES WHERE N_KOL  = " + Baseknow.AMALKARD + " And TNUMBER = " + CODE.Text).ToList();
                    if (rst is not null)
                    {
                        for (int i = 0; i < rst.Count; i++) //while (!rst.EOF())
                        {
                            try
                            {
                                var _NAME_ = "نرخ " + NAM.Text;

                                dbms.DoExecuteSQL($@"UPDATE TDETA_HES SET NAME = N'{_NAME_}'  WHERE N_KOL  = " + Baseknow.AMALKARD + " And TNUMBER = " + CODE.Text);
                            }
                            catch (Exception)
                            {
                                return;
                            }
                        }
                    }
                }
                //rst.Close();
                //DoCmd.RunCommand(acCmdSaveRecord);
            }
        }
        private void NAM_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            NAM.Text = NAM.Text.Trim();
        }
        private void PGID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PGID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox PGID_TEX = (TextBox)PGID.Template.FindName("PART_EditableTextBox", PGID);

            if (PGID.SelectedIndex == -1)
            {
                //PGID_NotInList
                //#Error↓
                //DoCmd.OpenForm("PRICE_GRP_FORM", acFormDS, default, "PGNAME like N'%" + this.PGID.Text + "%'", default, default, 1);
            }
        }

        //FSK:
        public void ReGetData()
        {
            FSK_DATA?.Clear();
            var data = dbms.DoGetDataSQL<STUF_FSK>($"SELECT CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, IDD, POSITION, B_SEF, N_SEF, MIN_M, MAX_M, CRT, UID FROM dbo.STUF_FSK WHERE CODE = '{CODE.Text}'").ToList();
            foreach (var item in data)
            {
                FSK_DATA.Add(item);
            }

            Summing();
        }

        private void Summing()
        {
            //جمع ها:
            MEGHMOG_SM.Text = FSK_DATA.Sum(i => i.MOGODI_A).ToStringNullSafe();
            FI_SM.Text = FSK_DATA.Sum(i => i.FI_A).ToStringNullSafe();
            MABLK_SM.Text = FSK_DATA.Sum(i => i.MABL_A).ToStringNullSafe();
        }

        private void STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            STUF_FSK_sub.Dispatcher.Invoke(() =>
            {
                STUF_FSK_sub.CellEditEnding -= STUF_FSK_sub_CellEditEnding;
                STUF_FSK_sub.RowEditEnding -= STUF_FSK_sub_RowEditEnding;
                if (_RC_ is null)
                {
                    STUF_FSK_sub.CancelEdit();
                }
                else
                {
                    STUF_FSK_sub.CancelEdit((DataGridEditingUnit)_RC_);
                }
                STUF_FSK_sub.RowEditEnding += STUF_FSK_sub_RowEditEnding;
                STUF_FSK_sub.CellEditEnding += STUF_FSK_sub_CellEditEnding;
            });
        }
        private void STUF_FSK_sub_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

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
                FSK_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                FSK_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            FSK_CURRENT_ROW_ITEMS = e.Row.Item as STUF_FSK;
            #endregion

            //انبار
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(FSK_ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("انبار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    FSK_CURRENT_ROW_ITEMS.ANBAR = FSK_WAS_ROW_ITEM?.ANBAR;
                }
                else
                {
                    if (RADAH.SelectedIndex > -1)
                    {
                        //ANBAR_BeforeUpdate
                        if (Convert.ToInt32(RADAH.SelectedValue) == 1)
                        {
                            var rst = dbms.DoGetDataSQL<_KALA_QRE_5>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.KIND FROM TCOD_ANBAR WHERE (((TCOD_ANBAR.CODE)=" + FSK_ENTERED_VALUE_ROW + ") AND ((TCOD_ANBAR.KIND)<>0))").ToList();
                            if (rst.Count > 0)
                            {
                                STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                new Msgwin(false, "كالاي فوق متعلق به انبار مواد اوليه مي باشد و نمي تواند در اين انبار قرار گيرد").ShowDialog();
                            }
                        }
                    }
                }
            }

            //مقدار
            if (e.Column.SortMemberPath == "MOGODI_A")
            {
                if (string.IsNullOrEmpty(FSK_ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    FSK_CURRENT_ROW_ITEMS.MOGODI_A = FSK_WAS_ROW_ITEM?.MOGODI_A;
                }
            }

            //فی
            if (e.Column.SortMemberPath == "FI_A")
            {
                if (string.IsNullOrEmpty(FSK_ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("فی نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    FSK_CURRENT_ROW_ITEMS.FI_A = FSK_WAS_ROW_ITEM?.FI_A;
                }
                else
                {
                    //FI_A_AfterUpdate
                    if (Convert.ToDouble(FSK_ENTERED_VALUE_ROW) == 0)
                    {
                        //this.MABL_A.TabStop = true; 
                        int? colindex = STUF_FSK_sub.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "MABL_A")?.DisplayIndex;
                        CL_LMethods.GetCell(STUF_FSK_sub, row_index, (int)colindex).IsTabStop = true;
                    }
                    else
                    {
                        FSK_CURRENT_ROW_ITEMS.MABL_A = Convert.ToDouble(FSK_ENTERED_VALUE_ROW) * FSK_CURRENT_ROW_ITEMS.MOGODI_A;
                    }
                }

            }

            //مبلغ
            if (e.Column.SortMemberPath == "MABL_A")
            {
                if (string.IsNullOrEmpty(FSK_ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مبلغ نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    FSK_CURRENT_ROW_ITEMS.MABL_A = FSK_WAS_ROW_ITEM?.MABL_A;
                }
                else
                {
                    //MABL_A_BeforeUpdate
                    if (FSK_CURRENT_ROW_ITEMS.MOGODI_A == 0)
                    {
                        STUF_FSK_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        new Msgwin(false, "موجودي صفر است بنابر اين سطر در انبار , مبلغ نمي تواند داشته باشد").ShowDialog();
                    }
                    else
                    {
                        var _MOGODI_A_ = Convert.ToDouble(FSK_CURRENT_ROW_ITEMS.MOGODI_A);
                        //MABL_A_AfterUpdate
                        if (_MOGODI_A_ != 0)
                        {
                            FSK_CURRENT_ROW_ITEMS.FI_A = FSK_CURRENT_ROW_ITEMS.MABL_A / _MOGODI_A_;
                        }
                    }
                }

            }

        }
        private void STUF_FSK_sub_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && STUF_FSK_sub.SelectedItem is not null)
            {
                if (STUF_FSK_sub.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    FSK_WAS_ROW_ITEM = ((STUF_FSK)STUF_FSK_sub.SelectedItem).Clone() as STUF_FSK;
                }
            }
        }

        TransactionManagement TM;
        private void STUF_FSK_sub_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as STUF_FSK;

            if (!FSK_BodyIsValid(ROW))
            {
                return;
            }

            try
            {
                #region Saving

                TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);
                bool KalaisOK = true;

                int? theidd = null;

                if (ROW?.IDD is null) //Insert
                {
                    //OUTPUT INSERTED.IDD
                    theidd = TM.SqlQueryCtc<int?>($@"INSERT INTO dbo.STUF_FSK(CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, POSITION, B_SEF, N_SEF, MIN_M, MAX_M)
                                         
                                         VALUES(N'{CODE.Text}',
                                         {ROW.ANBAR},
                                         {ROW.MOGODI_A} ,
                                         {ROW.FI_A} ,
                                         {ROW.MABL_A} ,
                                         {ROW.MANDAH_A} ,
                                         {(ROW.VAZ is null ? "NULL" : ROW.VAZ)} ,
                                         N'{ROW.POSITION}' ,
                                         {(ROW.B_SEF is null ? "NULL" : ROW.B_SEF)} ,
                                         {(ROW.N_SEF is null ? "NULL" : ROW.N_SEF)} ,
                                         {(ROW.MIN_M is null ? "NULL" : ROW.MIN_M)} ,
                                         {(ROW.MAX_M is null ? "NULL" : ROW.MAX_M)} )").FirstOrDefault();

                    theidd = TM.SqlQueryCtc<int?>("SELECT SCOPE_IDENTITY()").FirstOrDefault();
                }
                else //Update
                {
                    TM.ExecuteSqlCommandCtc($@"UPDATE dbo.STUF_FSK
                                         SET CODE = N'{CODE.Text}', ANBAR = {ROW.ANBAR}, MOGODI_A = {ROW.MOGODI_A},
                                         FI_A = {ROW.FI_A}, MABL_A = {ROW.MABL_A}, MANDAH_A = {ROW.MANDAH_A}, 
                                         VAZ = {(ROW.VAZ is null ? "NULL" : ROW.VAZ)},
                                         POSITION = N'{ROW.POSITION}',
                                         B_SEF = {(ROW.B_SEF is null ? "NULL" : ROW.B_SEF)}, 
                                         N_SEF = {(ROW.N_SEF is null ? "NULL" : ROW.N_SEF)},
                                         MIN_M = {(ROW.MIN_M is null ? "NULL" : ROW.MIN_M)},
                                         MAX_M = {(ROW.MAX_M is null ? "NULL" : ROW.MAX_M)}
                                         WHERE IDD = {ROW?.IDD}");
                }

                if ((bool)Baseknow.RMOG)
                {
                    var min = Convert.ToDouble(ROW.MIN_M);

                    var RSTM0 = TM.SqlQueryCtc<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM " +
                        "dbo.AK_MOGO_AVL_KOL(99999999," + ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN " +
                        " dbo.AK_MOGO_FR(99999999," + ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR" +
                        " WHERE (dbo.STUF_FSK.CODE = N'" + CODE.Text + "') AND (dbo.STUF_FSK.ANBAR = " + ROW.ANBAR + ")").ToList();

                    if (RSTM0.Count > 0)
                    {
                        if (Math.Round((double)(RSTM0.FirstOrDefault() - (ROW.MOGODI_A)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && ROW.ANBAR != 0 && Baseknow.MOJU)
                        {
                            KalaisOK = false;
                        }
                    }
                }

                if (!KalaisOK)
                {
                    TM.DoRollback(); //لغو عملیات
                    STUF_FSK_SUB_CANCEL_EDIT();

                    var AnbarName = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAMES FROM dbo.TCOD_ANBAR WHERE CODE = {ROW.ANBAR}").FirstOrDefault();
                    new Msgwin(false, $"مقدار {ROW.MOGODI_A} برای انبار \" {AnbarName} \" موجودی را در این انبار به مقدار غیر مجاز کاهش میدهد !").ShowDialog();
                }
                else //AllisWell
                {
                    if (theidd != null)
                    {
                        ROW.IDD = theidd;
                    }

                    TM.DoCommit(); //ذخیره و اعمال نهایی
                }

                #endregion
            }
            catch (SqlException ex)
            {

                STUF_FSK_SUB_CANCEL_EDIT();

                TM.DoRollback(); //لغو عملیات

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این انبار برای این کالا تکراری وارد شده !").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }
            Form_AfterUpdate(ROW); // ایجاد مقادیر در جدول موجودی

            ANBAR_AfterUpdate(ROW); //بروز رسانی یا ایجاد حساب های این انبار


            Summing(); //جمع ها
        }

        private bool FSK_BodyIsValid(STUF_FSK Row)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var errors = (from object i in STUF_FSK_sub.ItemsSource
                          let c = STUF_FSK_sub.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");

                return false;
            }

            if (Row.ANBAR is null) //*
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار انتخاب نشده!" });
            }
            if (Row.MOGODI_A < 0) //مقدار *
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار موجودی ابتدای دوره در اطلاعات انبار کمتر صفر است!" });
            }
            if (Row.FI_A < 0) //فی *
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فی سطر انبار کمتر از صفر است" });
            }
            if (Row.MABL_A < 0) //مبلغ *
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ سطر انبار کمتر از صفر است" });
            }
            if (!double.TryParse(Row?.MOGODI_A.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار موجودی ابتدای دوره در اطلاعات انبار مجاز نیست!" });
            }
            if (!double.TryParse(Row?.FI_A.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "فی سطر انبار مجاز نیست" });
            }
            if (!double.TryParse(Row?.MABL_A.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ سطر انبار مجاز نیست" });
            }
            //MANDAH_A; //مانده اولیه *
            if (!string.IsNullOrEmpty(Row?.VAZ.ToStringNullSafe()) && !double.TryParse(Row?.VAZ.ToStringNullSafe(), out _)) //وضعیت
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار وضعیت سطر انبار مجاز نیست" });
            }
            if (!string.IsNullOrEmpty(Row?.B_SEF.ToStringNullSafe()) && !double.TryParse(Row?.B_SEF.ToStringNullSafe(), out _)) //بهینه سفارش
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار بهینه سفارش سطر انبار مجاز نیست" });
            }
            if (!string.IsNullOrEmpty(Row?.N_SEF.ToStringNullSafe()) && !double.TryParse(Row?.N_SEF.ToStringNullSafe(), out _))  //نقطه سفارش
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار نقطه سفارش سطر انبار مجاز نیست" });
            }
            if (!double.TryParse(Row?.MIN_M.ToStringNullSafe(), out _))  //حداقل موجودی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار حداقل موجودی سطر انبار مجاز نیست" });
            }
            if (!string.IsNullOrEmpty(Row?.MAX_M.ToStringNullSafe()) && !double.TryParse(Row?.MAX_M.ToStringNullSafe(), out _))  //حداکثر موجودی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار حداکثر موجودی سطر انبار مجاز نیست" });
            }

            if (RADAH.SelectedIndex > -1)
            {
                //ANBAR_BeforeUpdate
                if (Convert.ToInt32(RADAH.SelectedValue) == 1 && Row?.ANBAR is not null)
                {
                    var rst = dbms.DoGetDataSQL<_KALA_QRE_5>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.KIND FROM TCOD_ANBAR WHERE (((TCOD_ANBAR.CODE)=" + Row.ANBAR + ") AND ((TCOD_ANBAR.KIND)<>0))").ToList();
                    if (rst.Count > 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "كالاي فوق متعلق به انبار مواد اوليه مي باشد و نمي تواند در اين انبار قرار گيرد" });
                    }
                }
            }

            if (Row?.MABL_A is not null && Row?.MABL_A > 0)
            {
                //MABL_A_BeforeUpdate
                if (Row.MOGODI_A == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "موجودی ابتدای دوره صفر وارد شده در حالی که مبلغ دارد , لطفا آنرا اصلاح کنید" });
                }
            }



            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                STUF_FSK_SUB_CANCEL_EDIT();
                return false;
            }

            return true;
        }
        private void ANBAR_AfterUpdate(STUF_FSK Row)
        {
            if (!string.IsNullOrEmpty(Row?.ANBAR.ToStringNullSafe()) && !string.IsNullOrEmpty(CODE.Text) && !string.IsNullOrEmpty(NAM.Text) && MASTER_IDD is not null)
            {
                if (Row.ANBAR != 0) //FSK_WAS_ROW_ITEM.ANBAR != Row.ANBAR
                {
                    var rst = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR).ToList(); //S-TK
                    if (rst.Count == 0)
                    {
                        STUF_FSK_SUB_CANCEL_EDIT();
                        new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی در اتباط باشید.").ShowDialog();
                        //this.ANBAR = this.ANBAR.TAG;
                    }
                    else if (rst.FirstOrDefault().MOGODI + rst.FirstOrDefault().MOGODI_A - Row.MOGODI_A < Convert.ToDouble(MIN_M.Text))
                    {
                        //this.ANBAR = this.ANBAR.TAG;
                        STUF_FSK_SUB_CANCEL_EDIT();
                        new Msgwin(false, "تغيير كد انبار مقدار موجودي را به كمتر از حد مجاز مي رساند!").ShowDialog();
                    }
                }
                if (Convert.ToDouble(RADAH.SelectedValue) == 1)
                {
                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + Row.ANBAR + " AND TNUMBER = " + CODE.Text).ToList();
                        var _N_KOL_ = Baseknow.MOGODIA;
                        var _NUMBER_ = Row.ANBAR;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                            VALUES({_N_KOL_},
                            {_NUMBER_} ,
                            {_TNUMBER_} ,
                            N'{_NAME_}' ,
                            {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                          SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                          NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                          WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + Row.ANBAR + " AND TNUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }
                    // فروش
                    if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM  TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.FROSH;
                        var _NUMBER_ = 1;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text);
                        }

                    }
                    else
                    {
                        if (true)
                        {
                            var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text).ToList();

                            var _N_KOL_ = Baseknow.FROSH;
                            var _NUMBER_ = CODE.Text;
                            var _NAME_ = "فروش " + NAM.Text;
                            var _BED_BES_ = -1;

                            if (rst.Count == 0)
                            {
                                //rst.AddNew();
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                            }
                            else
                            {
                                dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text);
                            }
                        }

                        if (true)
                        {
                            var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM  TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text).ToList();

                            var _N_KOL_ = Baseknow.FROSH;
                            var _NUMBER_ = CODE.Text;
                            var _TNUMBER_ = CODE.Text;
                            var _NAME_ = NAM.Text;
                            var _BED_BES_ = -1;
                            if (rst.Count == 0)
                            {
                                //rst.AddNew();
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                            }
                            else
                            {
                                dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text);
                            }
                            //rst.update();
                        }
                    }
                    // پاياپاي هزينه توليد-پاياپاي مواد مصرفي
                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.PHAZ_TOL;
                        var _NUMBER_ = 1;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }
                }
                else
                {
                    if (Convert.ToDouble(RADAH.SelectedValue) == 3)
                    {
                        // پاياپاي هزينه توليد-پاياپاي مواد مصرفي
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.PHAZ_TOL;
                        var _NUMBER_ = 1;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.PHAZ_TOL + " AND  NUMBER = 1  AND TNUMBER = " + CODE.Text);
                        }

                        //rst.update();
                    }
                    // فروش
                    if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM  TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.FROSH;
                        var _NUMBER_ = 1;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = 1 AND TNUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }
                    else
                    {
                        if (true)
                        {
                            var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text).ToList();

                            var _N_KOL_ = Baseknow.FROSH;
                            var _NUMBER_ = CODE.Text;
                            var _NAME_ = "فروش " + NAM.Text;
                            var _BED_BES_ = -1;

                            if (rst.Count == 0)
                            {
                                //rst.AddNew();
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                            }
                            else
                            {
                                dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text);
                            }
                            //rst.update();

                        }

                        if (true)
                        {
                            var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM  TDETA_HES WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text).ToList();

                            var _N_KOL_ = Baseknow.FROSH;
                            var _NUMBER_ = CODE.Text;
                            var _TNUMBER_ = CODE.Text;
                            var _NAME_ = NAM.Text;
                            var _BED_BES_ = -1;

                            if (rst.Count == 0)
                            {
                                //rst.AddNew();
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                            }
                            else
                            {
                                dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.FROSH + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = " + CODE.Text);
                            }
                            //rst.update();
                        }
                    }

                    if (true)
                    {
                        // انبارمحصول يا نيمه ساخته
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + Row.ANBAR + " AND TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.MOGODIA;
                        var _NUMBER_ = Row.ANBAR;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.MOGODIA + " AND  NUMBER = " + Row.ANBAR + " AND TNUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        // كنترل مواد
                        var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.HAZ_TOL + " AND  NUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.HAZ_TOL;
                        var _NUMBER_ = CODE.Text;
                        var _NAME_ = "مواد مصرفي " + NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.HAZ_TOL + " AND  NUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.GHEYMAT;
                        var _NUMBER_ = CODE.Text;
                        var _NAME_ = " قيمت تمام شده  " + NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text);
                        }

                        //rst.update();
                    }

                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.GHEYMAT;
                        var _NUMBER_ = CODE.Text;
                        var _TNUMBER_ = CODE.Text;
                        var _NAME_ = " قيمت تمام شده  " + NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.GHEYMAT + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        //كنترل كالاي در جريان ساخت
                        var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.CONKAL;
                        var _NUMBER_ = CODE.Text;
                        var _NAME_ = NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        //دستمزد
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = 99999996").ToList();

                        var _N_KOL_ = Baseknow.CONKAL;
                        var _NUMBER_ = CODE.Text;
                        var _TNUMBER_ = 99999996;
                        var _NAME_ = "دستمزد";
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = 99999996");
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = 99999997").ToList();

                        var _N_KOL_ = Baseknow.CONKAL;
                        var _NUMBER_ = CODE.Text;
                        var _TNUMBER_ = 99999997;
                        var _NAME_ = "سربار";
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = 99999997");
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        //جذب دستمزد
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = 99999999").ToList();

                        var _N_KOL_ = Baseknow.CONKAL;
                        var _NUMBER_ = CODE.Text;
                        var _TNUMBER_ = 99999999;
                        var _NAME_ = "جذب دستمزد";
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND TNUMBER = 99999999");
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = 99999998").ToList();

                        var _N_KOL_ = Baseknow.CONKAL;
                        var _NUMBER_ = CODE.Text;
                        var _TNUMBER_ = 99999998;
                        var _NAME_ = "جذب سربار";
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                                                 VALUES({_N_KOL_},
                                                 {_NUMBER_} ,
                                                 {_TNUMBER_} ,
                                                 N'{_NAME_}' ,
                                                 {_BED_BES_} )");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                 SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                 NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                 WHERE N_KOL = " + Baseknow.CONKAL + " AND  NUMBER = " + CODE.Text + " AND  TNUMBER = 99999998");
                        }
                        //rst.update();
                    }

                    if (true)
                    {
                        var rst = dbms.DoGetDataSQL<DETA_HES>("SELECT * FROM DETA_HES WHERE N_KOL = " + Baseknow.AMALKARD + " AND  NUMBER = " + CODE.Text).ToList();

                        var _N_KOL_ = Baseknow.AMALKARD;
                        var _NUMBER_ = CODE.Text;
                        var _NAME_ = " عملكرد  " + NAM.Text;
                        var _BED_BES_ = -1;

                        if (rst.Count == 0)
                        {
                            //rst.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME, BED_BES)
                                                     VALUES({_N_KOL_},
                                                     {_NUMBER_} ,
                                                     N'{_NAME_}' ,
                                                     {_BED_BES_} ) ");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.DETA_HES
                                                     SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, 
                                                     NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                                     WHERE N_KOL = " + Baseknow.AMALKARD + " AND  NUMBER = " + CODE.Text);
                        }
                        //rst.update();
                    }
                }
            }
        }
        private void Form_AfterUpdate(STUF_FSK Row)
        {
            if (!string.IsNullOrEmpty(Row?.ANBAR.ToStringNullSafe()) && !string.IsNullOrEmpty(CODE.Text) && !string.IsNullOrEmpty(NAM.Text) && MASTER_IDD is not null)
            {
                var rst0 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR).ToList();
                if (Row.ANBAR != 0)
                {
                    if (rst0.Count == 0)
                    {
                    }
                    // DoCmd.OpenForm "mesageform", acNormal, , , acFormReadOnly, acDialog, "اطلاعات ناقص مي باشد. با شركت قائم رايانه عرش تماس بگيريد."
                    // Me.ANBAR = Me.ANBAR.TAG
                    else
                    {
                        var _MOGODI_A_ = rst0.FirstOrDefault().MOGODI_A - FSK_WAS_ROW_ITEM.MOGODI_A /*.TAG*/;

                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK
                                             SET MOGODI_A={_MOGODI_A_}
                                             WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR);
                    }

                    var rst1 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR).ToList();
                    if (rst1.Count == 0)
                    {
                        //rst1.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.STUF_STK(CODE, ANBAR, MOGODI_A)
                                             VALUES(N'{CODE.Text}',
                                             {Row.ANBAR} ,
                                             {(Row.MOGODI_A is null ? "0" : Row.MOGODI_A)}) ");
                    }
                    else
                    {
                        var _MOGODI_A_ = rst1.FirstOrDefault().MOGODI_A + Row.MOGODI_A;

                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK
                                             SET MOGODI_A = {_MOGODI_A_}
                                             WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR);
                    }
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR).ToList();
                    if (rst.Count == 0)
                    {
                        //rst.AddNew();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.STUF_STK(CODE, ANBAR, MOGODI_A)
                                             VALUES(N'{CODE.Text}',
                                             {Row.ANBAR} ,
                                             {(Row.MOGODI_A is null ? "0" : Row.MOGODI_A)}) ");
                    }
                    else
                    {
                        var _MOGODI_A_ = rst.FirstOrDefault().MOGODI_A - (FSK_WAS_ROW_ITEM.MOGODI_A/*.TAG*/ - Row.MOGODI_A);

                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK
                                             SET MOGODI_A = {_MOGODI_A_}
                                             WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + Row.ANBAR);
                    }
                }
            }
        }

        private void STUF_FSK_sub_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (STUF_FSK_sub.Items.Count > 0 && STUF_FSK_sub.SelectedItem != null)
                {
                    if (!(STUF_FSK_sub.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف کالا => قسمت موجودی و انبار",
                                    recordId: STUF_FSK_sub.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < STUF_FSK_sub.SelectedItems.Count; i++)
                            {
                                var item = STUF_FSK_sub.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("IDD").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _idd = item.GetType().GetProperty("IDD").GetValue(item);

                                        var AnbarCode = Convert.ToDouble(item.GetType().GetProperty("ANBAR").GetValue(item));
                                        var AnbarName = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAMES FROM dbo.TCOD_ANBAR WHERE CODE = {AnbarCode}").FirstOrDefault();
                                        var _MIN_M_ = item.GetType().GetProperty("MIN_M").GetValue(item);
                                        var _MOGODI_A_ = Convert.ToDouble(item.GetType().GetProperty("MOGODI_A").GetValue(item));

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            bool OverStockHappned = false;
                                            bool IncompletedSTK = false;

                                            TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR);

                                            TM.ExecuteSqlCommandCtc($@"DELETE FROM dbo.STUF_FSK WHERE IDD = {_idd}"); //F

                                            #region Form_Delete
                                            var rst = TM.SqlQueryCtc<string?>("SELECT CODE FROM STUF_STK WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + AnbarCode).ToList();
                                            if (rst.Count == 0)
                                            {
                                                //IncompletedSTK = true;
                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"انبار \" {AnbarName} \" برای این کالا اطلاعات ناقص است با پشتیبانی در ارتباط باشید !" });
                                            }
                                            else
                                            {
                                                TM.ExecuteSqlCommandCtc($@"UPDATE dbo.STUF_STK SET MOGODI_A = 0 WHERE CODE = '" + CODE.Text + "' AND ANBAR = " + AnbarCode); //S
                                                                                                                                                                             //rst.Fields("MOGODI_A") = rst.Fields("MOGODI_A") - this.MOGODI_A; //rst.update();
                                            }
                                            #endregion

                                            if ((bool)Baseknow.RMOG) //بررسی کردن موجودی
                                            {
                                                var min = Convert.ToDouble(_MIN_M_);
                                                var RSTM0 = TM.SqlQueryCtc<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM " +
                                                    "dbo.AK_MOGO_AVL_KOL(99999999," + AnbarCode + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN " +
                                                    " dbo.AK_MOGO_FR(99999999," + AnbarCode + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR" +
                                                    " WHERE (dbo.STUF_FSK.CODE = N'" + CODE.Text + "') AND (dbo.STUF_FSK.ANBAR = " + AnbarCode + ")").ToList();
                                                if (RSTM0.Count > 0)
                                                {
                                                    if (Math.Round((double)(RSTM0.FirstOrDefault() - (_MOGODI_A_)), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && AnbarCode != 0 && Baseknow.MOJU)
                                                    {
                                                        OverStockHappned = true;
                                                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار {_MOGODI_A_} برای انبار \" {AnbarName} \" موجودی را در این انبار به مقدار غیر مجاز کاهش میدهد !" });
                                                    }
                                                }
                                            }

                                            if (OverStockHappned || IncompletedSTK)
                                            {
                                                TM.DoRollback(); //Cancel Opration
                                            }
                                            else //FINALLY ♥
                                            {
                                                TM.DoCommit(); //Apply Everything
                                            }

                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                TM.DoRollback(); //Cancel Opration

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"انبار \" {AnbarName} \" دارای گردش است و نمیتوان آنرا پاک کرد !" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                  .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }

                        //After Opration:
                        if (IsDeletedSomething)
                        {
                            ReGetData();
                            universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                        }
                    }
                }
            }

            string CURRENT_COLUMN_NAME = "";
            if (STUF_FSK_sub.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = STUF_FSK_sub.CurrentCell.Column?.SortMemberPath;
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "FI_A" || CURRENT_COLUMN_NAME is "MABL_A")
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
                if (CURRENT_COLUMN_NAME is "FI_A" || CURRENT_COLUMN_NAME is "MABL_K")
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
        private void STUF_FSK_sub_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                STUF_FSK_sub_IsFocusedIn = false;
            }
            else //Is Focus inside of STUF_FSK_sub
            {
                STUF_FSK_sub_IsFocusedIn = true;
            }
        }

        //MODULE_D:
        private void MODULE_D_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            MODULE_D_SUB.Dispatcher.InvokeAsync(() =>
            {
                MODULE_D_SUB.CellEditEnding -= MODULE_D_SUB_CellEditEnding;
                MODULE_D_SUB.RowEditEnding -= MODULE_D_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    MODULE_D_SUB.CancelEdit();
                }
                else
                {
                    MODULE_D_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                MODULE_D_SUB.RowEditEnding += MODULE_D_SUB_RowEditEnding;
                MODULE_D_SUB.CellEditEnding += MODULE_D_SUB_CellEditEnding;
            });
        }
        private bool MODULE_D_BodyIsValid(bool _DisplayMsg_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var errors = (from object i in MODULE_D_SUB.ItemsSource
                          let c = MODULE_D_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                if (_DisplayMsg_)
                {
                    universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                }
                return false;
            }

            foreach (var Row in MODULE_D_DATA)
            {
                if (Row?.VAHED is null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "واحد فرعی نمیتواند خالی باشد" });
                }

                if (Row?.NESBAT is null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نسبت نمیتواند خالی باشد" });
                }
                if (!double.TryParse(Row?.NESBAT.ToStringNullSafe(), out _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار نسبت مجاز نیست" });
                }

            }

            if (ErrosMessages.Count > 0)
            {
                if (_DisplayMsg_)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                MODULE_D_SUB_CANCEL_EDIT();
                return false;
            }

            return true;
        }
        private void MODULE_D_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (MODULE_D_SUB.Items.Count > 0 && MODULE_D_SUB.SelectedItem != null)
                {
                    if (!(MODULE_D_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف کالا => قسمت واحد های فرعی",
                                    recordId: MODULE_D_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < MODULE_D_SUB.SelectedItems.Count; i++)
                            {
                                var item = MODULE_D_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _id = item.GetType().GetProperty("ID").GetValue(item);
                                        var _vahed = item.GetType().GetProperty("VAHED").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM MODULE_D WHERE ID = {_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"واحد با کد {_vahed} دارای گردش است و نمیتوان آنرا پاک کرد" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }

                    }
                }
            }
        }
        private void MODULE_D_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && MODULE_D_SUB.SelectedItem is not null)
            {
                if (MODULE_D_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    MODULE_D_WAS_ROW_ITEM = ((MODULE_D)MODULE_D_SUB.SelectedItem).Clone() as MODULE_D;
                }
            }
        }
        private void MODULE_D_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
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
                MODULE_D_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                MODULE_D_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            MODULE_D_CURRENT_ROW_ITEMS = e.Row.Item as MODULE_D;
            #endregion
        }
        private void MODULE_D_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as MODULE_D;

            if (!MODULE_D_BodyIsValid()) { return; }

            long? tmpid = null;
            try
            {
                if (ROW?.ID is null) //INSERT
                {
                    tmpid = dbms.DoGetDataSQL<long?>($@"INSERT INTO  dbo.MODULE_D (CODE, VAHED, NESBAT, MABL_F)
                                            OUTPUT INSERTED.ID
                                            VALUES ('{CODE.Text}', {ROW.VAHED}, {ROW.NESBAT}, {(ROW.MABL_F is null ? 0 : ROW.MABL_F)})").FirstOrDefault();
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.MODULE_D SET CODE = '{CODE.Text}' , NESBAT = {ROW.NESBAT},VAHED = {ROW.VAHED}, MABL_F = {(ROW.MABL_F is null ? 0 : ROW.MABL_F)} WHERE ID = {ROW.ID}");
                }
            }
            catch (SqlException ex)
            {
                MODULE_D_SUB_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "واحد فرعی تکراری وارد شده");
                }
                else
                {
                    throw;
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            if (tmpid != null)
            {
                ROW.ID = tmpid;
            }

            MODULE_D_ReGetData();
        }
        private void MODULE_D_ReGetData()
        {
            MODULE_D_DATA?.Clear();
            var data = dbms.DoGetDataSQL<MODULE_D>($"SELECT CODE, VAHED, RADIF, NESBAT, MABL_F, CRT, UID, ID FROM dbo.MODULE_D WHERE CODE = '{CODE.Text}' ").ToList();
            foreach (var item in data)
            {
                MODULE_D_DATA.Add(item);
            }
        }


        //TAKHPERS:
        private void TAKHPERS_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            TAKHPERS_SUB.Dispatcher.InvokeAsync(() =>
            {
                TAKHPERS_SUB.CellEditEnding -= TAKHPERS_SUB_CellEditEnding;
                TAKHPERS_SUB.RowEditEnding -= TAKHPERS_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    TAKHPERS_SUB.CancelEdit();
                }
                else
                {
                    TAKHPERS_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                TAKHPERS_SUB.RowEditEnding += TAKHPERS_SUB_RowEditEnding;
                TAKHPERS_SUB.CellEditEnding += TAKHPERS_SUB_CellEditEnding;
            });
        }
        private bool TAKHPERS_BodyIsValid(TAKHPERS Row)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var errors = (from object i in TAKHPERS_SUB.ItemsSource
                          let c = TAKHPERS_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");

                return false;
            }

            //کد نوع مشتری
            if (string.IsNullOrEmpty(Row?.CUST_CO.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد نوع مشتری خالی است!" });
            }

            //% تخفيف *
            if (string.IsNullOrEmpty(Row?.TAFPER.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار % تخفيف خالی است" });
            }
            else if (!short.TryParse(Row?.TAFPER.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار % تخفيف غیر مجاز است" });
            }
            else
            {
                var _TAFPER_ = Convert.ToInt16(Row?.TAFPER);
                if (_TAFPER_ < 0 || _TAFPER_ > 100)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار % تخفيف صحیح نیست" });
                }
            }

            //قيمت
            if (!string.IsNullOrEmpty(Row?.PRICE_M.ToStringNullSafe()) && !int.TryParse(Row?.PRICE_M.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار قيمت غیر مجاز است" });
            }
            //درصد +-
            if (!string.IsNullOrEmpty(Row?.PERS.ToStringNullSafe()) && !double.TryParse(Row?.PERS.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار درصد +- غیر مجاز است" });
            }
            //بالانس+-
            if (!string.IsNullOrEmpty(Row?.BLNS.ToStringNullSafe()) && !int.TryParse(Row?.BLNS.ToStringNullSafe(), out _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار بالانس+- غیر مجاز است" });
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                TAKHPERS_SUB_CANCEL_EDIT();
                return false;
            }

            return true;
        }
        private void TAKHPERS_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }
        private void TAKHPERS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (TAKHPERS_SUB.Items.Count > 0 && TAKHPERS_SUB.SelectedItem != null)
                {
                    if (!(TAKHPERS_SUB.SelectedItems is null))
                    {
                        bool IsDeletedSomething = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "تعریف کالا => قسمت تخفیفات",
                                    recordId: TAKHPERS_SUB.SelectedItem.ToStringNullSafe(),
                                    oldValue: null,
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            for (int i = 0; i < TAKHPERS_SUB.SelectedItems.Count; i++)
                            {
                                var item = TAKHPERS_SUB.SelectedItems[i];

                                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                                {
                                    if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                    {
                                    }
                                    else
                                    {
                                        var _id = item.GetType().GetProperty("ID").GetValue(item);
                                        var _cust_co = item.GetType().GetProperty("CUST_CO").GetValue(item);

                                        try
                                        {
                                            IsDeletedSomething = true;

                                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TAKHPERS WHERE ID = {_id}");
                                        }
                                        catch (SqlException ex)
                                        {
                                            if (ex.Number == 547)
                                            {
                                                e.Handled = true;

                                                ErrosMessages.Add(new MsgModel { MessageText_U = $"این نوع مشتری با کد {_cust_co} تکراری وارد شده" });
                                            }
                                            else
                                            {
                                                ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Handled = true;
                        }

                        if (ErrosMessages.Count > 0)
                        {
                            ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, ErrosMessages).ShowDialog();

                            return;
                        }
                    }
                }
            }
        }
        private void TAKHPERS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_
            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            var PAY_GETD_SUB22_ROW_INDEX = row_index;
            var rowContainer = TAKHPERS_SUB.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
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

            string? TAKHPERS_ENTERED_VALUE = null;
            if (!ReferenceEquals(Comboval, null))
                TAKHPERS_ENTERED_VALUE = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                TAKHPERS_ENTERED_VALUE = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                TAKHPERS_ENTERED_VALUE = TexboVal.Text.Trim();

            TAKHPERS_CURRENT_ROW = e.Row.Item as TAKHPERS;
            #endregion
        }
        private void TAKHPERS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as TAKHPERS;
            ROW.TAKH_COD = CODE.Text;

            if (!TAKHPERS_BodyIsValid(ROW))
            {
                return;
            }

            long? tmpid = null;
            try
            {
                if (ROW.ID is null) //INSERT
                {
                    tmpid = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.TAKHPERS (TAKH_COD,   CUST_CO, TAFPER, PRICE_M, PERS, BLNS, PUT)
                                                     OUTPUT INSERTED.ID
							                         VALUES ('{CODE.Text}', {ROW.CUST_CO}, {ROW.TAFPER}, {ROW.PRICE_M},{ROW.PERS}, {ROW.BLNS},{(ROW.PUT is null ? "NULL" : ROW.PUT)})").FirstOrDefault();
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TAKHPERS
                                     SET TAKH_COD = N'{CODE.Text}', CUST_CO = {ROW.CUST_CO}, TAFPER = {ROW.TAFPER},
                                     PRICE_M = {ROW.PRICE_M}, PERS = {ROW.PERS}, BLNS = {ROW.BLNS}, PUT = {(ROW.PUT is null ? "NULL" : ROW.PUT)}
                                     WHERE ID = {ROW.ID}");
                }
            }
            catch (SqlException ex)
            {
                TAKHPERS_SUB_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری برای قیمیت مصوب , نوع مشتری تکراری وارد شده!");
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            if (tmpid != null)
            {
                ROW.ID = tmpid;
            }

            //Form_AfterUpdate TAKHPER_SUB
            if (!(Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5"))
            {
                var rst = dbms.DoGetDataSQL<string?>("SELECT NUMBER FROM dbo.TDETA_HES WHERE " + "N_KOL = " + Baseknow.TFROSH + " AND " + "NUMBER = " + ROW.CUST_CO + " AND " + "TNUMBER = " + ROW.TAKH_COD).ToList();

                var _N_KOL_ = Baseknow.TFROSH;
                var _NUMBER_ = ROW.CUST_CO;
                var _TNUMBER_ = ROW.TAKH_COD;
                var _NAME_ = "تخفيف " + NAM.Text;
                var _BED_BES_ = -1;

                if (rst.Count == 0)
                {
                    //rst.AddNew();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME, BED_BES)
                            VALUES({_N_KOL_},
                            {_NUMBER_} ,
                            {_TNUMBER_} ,
                            N'{_NAME_}' ,
                            {_BED_BES_} )");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.TDETA_HES
                                          SET N_KOL = {_N_KOL_}, NUMBER = {_NUMBER_}, TNUMBER = {_TNUMBER_}, 
                                          NAME = N'{_NAME_}', BED_BES = {_BED_BES_}
                                          WHERE N_KOL = " + Baseknow.TFROSH + " AND " + "NUMBER = " + ROW.CUST_CO + " AND " + "TNUMBER = " + ROW.TAKH_COD);
                }
            }

            TAKHPERS_ReGetData();
        }
        private void Command55_Click(object sender, RoutedEventArgs e)
        {
            new LIST_KALA_TAKHPERS().Show();
        }
        private void TAKHPERS_ReGetData()
        {
            TAKHPERS_DATA?.Clear();
            var data = dbms.DoGetDataSQL<TAKHPERS>($"SELECT TAKH_COD, CUST_CO, TAFPER, PRICE_M, PERS, BLNS, PUT, CRT, UID, ID FROM dbo.TAKHPERS WHERE TAKH_COD = '{CODE.Text}' ").ToList();
            foreach (var item in data)
            {
                TAKHPERS_DATA.Add(item);
            }
        }

        //REWARDS:


        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.CODE.Text) && MASTER_IDD != null)
            {
                var dt = DateTime.Now;
                CL_HESABDARI.TR("STUF_DEF", "(CODE = '" + this.CODE.Text + "')", dt, 1);
                CL_HESABDARI.TR("STUF_FSK", "(CODE = '" + this.CODE.Text + "')", dt, 1);
                CL_HESABDARI.TR("TAKHPERS", "(TAKH_COD = '" + this.CODE.Text + "')", dt, 1);
                CL_HESABDARI.TR("MODULE_D", "(CODE = '" + this.CODE.Text + "')", dt, 1);

                this.AllowDeletions = true;
                this.AllowEdits = true;

                //ActivateDataGrids(true);
            }
        }

        private void DELETE_KALA_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_KALA.Visibility == Visibility.Visible;
            if (!DELETE_KALA.IsEnabled || !IsVisible) { return; }

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (FSK_DATA.Count > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "این کالا دارای اطلاعات انبار است" });
            }
            if (MODULE_D_DATA.Count > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "این کالا دارای واحد های فرعی است" });
            }
            if (TAKHPERS_DATA.Count > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "این کالا دارای تخفیفات مصوب است" });
            }
            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return;
            }

            if (!string.IsNullOrEmpty(CODE.Text) && MASTER_IDD != null)
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف کامل این کالا هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    try
                    {
                        ESLAH_Click(null, null);
                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "تعریف کالا - قسمت مشخصات سربرگ کالا",
                                recordId: MASTER_IDD.ToStringNullSafe(),
                                oldValue: null,
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        dbms.DoExecuteSQL($"DELETE FROM dbo.STUF_DEF WHERE IDD = {MASTER_IDD}");
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            if (ex.Message.Contains("ANBGRD_LST"))
                            {
                                var ExistingInAnbarGrd = dbms.DoGetDataSQL<int?>($"SELECT DISTINCT GRD_NUM FROM ANBARGRD_SUB1 WHERE CODE = {CODE.Text} ORDER BY GRD_NUM ASC").ToList();
                                ErrosMessages.Add(new MsgModel { MessageText_U = "این کالا در انبار گردانی های زیر وجود دارد و نمیتوان حذف کرد :" });

                                foreach (var item in ExistingInAnbarGrd)
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = $"انبار گردانی شماره : {item}" });
                                }
                                if (ErrosMessages.Count > 0)
                                {
                                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U)
                                                                 .Distinct()
                                                                 .Select(message => new MsgModel { MessageText_U = message })
                                                                 .ToList();
                                    _ = new MsgListwin(false, ErrosMessages).ShowDialog();
                                    ErrosMessages?.Clear();
                                }
                                //new Msgwin(false, "این کالا در انبار گردانی دارای گردش است و نمیتوان آنرا پاک کرد").ShowDialog();
                            }
                            else
                            {
                                new Msgwin(false, "این کالا دارای گردش است و نمیتوان آنرا پاک کرد").ShowDialog();
                            }
                            return;
                        }
                        else
                        {
                            new Msgwin(false, "خطا حذف انجام نشد!");
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                    }
                    RefreshAfterDelete();
                }
            }
        }
        private void Command50_Click(object sender, RoutedEventArgs e) //کدینگ شماره فنی
        {
            new TCOD_MAPF_WIN().Show();
        }

        private void NEWRECORD_BTN_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NewItem);
            NAM.Focus();
        }
        private void End_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(INavigator.Jahat.LastItem);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.NextItem);
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MoveReGetData(INavigator.Jahat.BackItem);
        }
        private void First_Click(object sender, RoutedEventArgs e)
        {
            NewRecord = false;
            MoveReGetData(INavigator.Jahat.FirstItem);
        }
        private void SERVERRELOAD_Btn_Click(object sender, RoutedEventArgs e)
        {
            ReGetMasterData();
        }

        private void Command39_Click(object sender, RoutedEventArgs e)
        {
            RecordsData.Source = RecordsData.View.Cast<STUF_DEF>().OrderBy(item => Convert.ToInt64(item.CODE)).ToList();

            MoveReGetData(INavigator.Jahat.FirstItem);
        }

        private void ALL_KALA_LIST_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                Msgwin msgwin = new Msgwin(true, "ذخیره را انجام نداده اید آیا از خروج مطمئن هستید؟  \n زیرا این پنجره بستجه خواهد شد و پنجره جستجو جای آن خواهد آمد");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is false)
                {
                    return;
                }
            }
            STUF_DEF_LST KALALIST = new STUF_DEF_LST();
            Close();
            KALALIST.ShowDialog();
        }

        private void NBARCODE_BTN_Click(object sender, RoutedEventArgs e)
        {
            new STUF_DEF_NEW(this).ShowDialog();
        }

        private void N_FANI_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeIsHappend = true; //Additional for Barcode window
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            INVOICE_REWARDS_SUB.BeginEdit();
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            INVOICE_REWARDS_SUB.BeginEdit();
        }


        public INVO_LST_FACTOR22 FROM_SEARCH_KAL { get; set; } = new INVO_LST_FACTOR22();
        public RewardRules? REWARDS_WAS_ROW_ITEM { get; private set; }
        private void INVOICE_REWARDS_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVOICE_REWARDS_SUB.Dispatcher.InvokeAsync(() =>
            {
                INVOICE_REWARDS_SUB.CellEditEnding -= INVOICE_REWARDS_SUB_CellEditEnding;
                INVOICE_REWARDS_SUB.RowEditEnding -= INVOICE_REWARDS_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVOICE_REWARDS_SUB.CancelEdit();
                }
                else
                {
                    INVOICE_REWARDS_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVOICE_REWARDS_SUB.RowEditEnding += INVOICE_REWARDS_SUB_RowEditEnding;
                INVOICE_REWARDS_SUB.CellEditEnding += INVOICE_REWARDS_SUB_CellEditEnding;
            });
        }
        private void INVOICE_REWARDS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            string? ENTERED_VALUE_ROW = null;

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

            RewardRules? CURRENT_ITEMS_ROW = e.Row.Item as RewardRules;
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "Reward_ProductID")
            {
                ENTERED_VALUE_ROW = Comboval?.Text;

                if (Comboval?.SelectedValue == null || REWARDS_WAS_ROW_ITEM?.Reward_ProductID != Comboval?.SelectedValue)
                {
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //Cleaning
                            Comboval.SelectedValue = REWARDS_WAS_ROW_ITEM.Reward_ProductID;
                            return;
                        }

                        if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //اگر عدد وارد کرده برم سرغ کد کالا
                            var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}')").FirstOrDefault();
                            if (!ReferenceEquals(FoundKala, null))
                            {
                                CURRENT_ITEMS_ROW.Reward_ProductID = FoundKala.CODE;
                            }
                            else
                            {
                                //شماره فنی
                                var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'')").ToList();
                                if (rstfani.Count > 0)
                                {
                                    CURRENT_ITEMS_ROW.Reward_ProductID = rstfani.FirstOrDefault().CODE;
                                }
                                else
                                {
                                    new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                                    INVOICE_REWARDS_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

        }
        private bool RewardRowIsValid(RewardRules rule, out List<string> errors)
        {
            errors = new List<string>();

            var errorssub = (from object i in STUF_FSK_sub.ItemsSource
                             let c = STUF_FSK_sub.ItemContainerGenerator.ContainerFromItem(i)
                             where c != null && Validation.GetHasError(c)
                             select c).Any();
            if (errorssub)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            if (rule.Quantity_Threshold <= 0)
                errors.Add("تعداد آستانه باید بیشتر از صفر باشد.");

            if (string.IsNullOrWhiteSpace(rule.Reward_Type) || !(rule.Reward_Type == "Product" || rule.Reward_Type == "Discount"))
                errors.Add("نوع جایزه نامعتبر است.");

            if (string.IsNullOrWhiteSpace(rule.Reward_ProductID))
                errors.Add("کالای جایزه انتخاب نشده است.");

            if (rule.Reward_Type == "Product")
            {
                if (rule.Reward_Quantity is null || rule.Reward_Quantity <= 0)
                    errors.Add("تعداد جایزه برای نوع 'محصول' باید بیشتر از صفر باشد.");
            }
            else if (rule.Reward_Type == "Discount")
            {
                //if (rule.Reward_Discount_Percentage is null || rule.Reward_Discount_Percentage <= 0 || rule.Reward_Discount_Percentage > 100)
                //    errors.Add("درصد تخفیف باید بین 1 تا 100 باشد.");
            }

            if (rule.StartDate != null)
            {
                if (rule.StartDate?.ToString().Length != 8 || !long.TryParse(rule.StartDate.ToString(), out _))
                    errors.Add("تاریخ شروع نامعتبر است. (فرمت صحیح: YYYYMMDD)");
            }

            if (rule.EndDate != null)
            {
                if (rule.EndDate?.ToString().Length != 8 || !long.TryParse(rule.EndDate.ToString(), out _))
                    errors.Add("تاریخ پایان نامعتبر است. (فرمت صحیح: YYYYMMDD)");

                if (rule.StartDate != null && rule.EndDate < rule.StartDate)
                    errors.Add("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
            }

            return errors.Count == 0;
        }
        private void INVOICE_REWARDS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }
            var ROW = e.Row.Item as RewardRules;

            if (ConstructorRowDetector.IsPristine(ROW)) { INVOICE_REWARDS_CANCEL_EDIT(); return; }

            if (!RewardRowIsValid(ROW, out var errorList))
            {
                new MsgListwin(false, errorList.Select(msg => new MsgModel { MessageText_U = msg }).ToList()).ShowDialog();
                INVOICE_REWARDS_CANCEL_EDIT();
                return;
            }

            try
            {
                var parameters = new
                {
                    RuleID = ROW.RuleID,
                    ProductID_Target = CODE.Text, // مقدار از تکست‌باکس
                    ROW.Quantity_Threshold,
                    ROW.Reward_Type,
                    ROW.Reward_ProductID,
                    ROW.Reward_Quantity,
                    ROW.Reward_Discount_Percentage,
                    IsActive = ROW.IsActive ?? false, // تبدیل Nullable Bool به Bool معمولی
                    ROW.StartDate,
                    ROW.EndDate,
                    ROW.Description,
                    UID = Baseknow.USERCOD
                };

                if (ROW.RuleID == null || ROW.RuleID == 0) // INSERT
                {
                    string sqlInsert = @"
                     INSERT INTO RewardRules (
                         ProductID_Target, Quantity_Threshold, Reward_Type, Reward_ProductID,
                         Reward_Quantity, Reward_Discount_Percentage, IsActive, StartDate, EndDate, Description, UID
                     )
                     OUTPUT INSERTED.RuleID
                     VALUES (
                         @ProductID_Target, @Quantity_Threshold, @Reward_Type, @Reward_ProductID,
                         @Reward_Quantity, @Reward_Discount_Percentage, @IsActive, @StartDate, @EndDate, @Description, @UID
                     )";

                    // اجرا و دریافت ID جدید
                    var insertedId = dbms.DoGetDataSQL<int>(sqlInsert, parameters).FirstOrDefault();
                    ROW.RuleID = insertedId;
                }
                else // UPDATE
                {
                    string sqlUpdate = @"
                     UPDATE RewardRules SET 
                         ProductID_Target = @ProductID_Target,
                         Quantity_Threshold = @Quantity_Threshold,
                         Reward_Type = @Reward_Type,
                         Reward_ProductID = @Reward_ProductID,
                         Reward_Quantity = @Reward_Quantity,
                         Reward_Discount_Percentage = @Reward_Discount_Percentage,
                         IsActive = @IsActive,
                         StartDate = @StartDate,
                         EndDate = @EndDate,
                         Description = @Description,
                         UID = @UID
                     WHERE RuleID = @RuleID";

                    // اجرای آپدیت
                    dbms.DoExecuteSQL(sqlUpdate, parameters);
                }
            }
            catch (SqlException ex)
            {
                INVOICE_REWARDS_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری برای جایزه , سطر تکراری وارد شده!");
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره جایزه: " + ex.Message).ShowDialog();
            }


        }
        private void INVOICE_REWARDS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete)
            {
                if (INVOICE_REWARDS_SUB.Items.Count > 0 && INVOICE_REWARDS_SUB.SelectedItems.Count > 0)
                {
                    bool IsDeletedSomething = false;
                    List<MsgModel> ErrosMessages = new List<MsgModel>();

                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "تعریف کالا => قسمت شروط جایزه",
                                recordId: INVOICE_REWARDS_SUB.SelectedItem.ToStringNullSafe(),
                                oldValue: null,
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        for (int i = 0; i < INVOICE_REWARDS_SUB.SelectedItems.Count; i++)
                        {
                            var item = INVOICE_REWARDS_SUB.SelectedItems[i];

                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                var _id = item.GetType().GetProperty("RuleID").GetValue(item);

                                if (_id != null)
                                {
                                    try
                                    {
                                        IsDeletedSomething = true;

                                        dbms.DoExecuteSQL($"DELETE FROM RewardRules WHERE RuleID = {_id}");
                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number == 547)
                                        {
                                            e.Handled = true;

                                            ErrosMessages.Add(new MsgModel { MessageText_U = $"دارای گردش است و نمیتوان آنرا پاک کرد" });
                                        }
                                        else
                                        {
                                            ErrosMessages.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!" });
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }

                    if (ErrosMessages.Count > 0)
                    {
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();

                        return;
                    }
                }
            }
        }
        private void INVOICE_REWARDS_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVOICE_REWARDS_SUB.SelectedItem is not null)
            {
                if (INVOICE_REWARDS_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    REWARDS_WAS_ROW_ITEM = ((RewardRules)INVOICE_REWARDS_SUB.SelectedItem).Clone() as RewardRules;
                }
            }
        }

        private void Label_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PGID.SelectedValue != null && !string.IsNullOrEmpty(CODE.Text))
            {
                int pgid = Convert.ToInt32(PGID.SelectedValue);
                // Note: Discount Declarations are fetched by Product Code (Exceptions) as they don't support Price Groups directly in the database.
                new WIN_SHOW_IN_DECLARATIONS(pgid, CODE.Text).ShowDialog();
            }
        }
    }
}
