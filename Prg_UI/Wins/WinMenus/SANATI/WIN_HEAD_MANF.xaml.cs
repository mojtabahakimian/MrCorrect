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
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using Wins.WinOther;
using static Functions.DataGridClipboardManager;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;

namespace Prg_UI.Wins.WinMenus.SANATI
{
    public partial class WIN_HEAD_MANF : Window, ISearchableWindow
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

        public WIN_HEAD_MANF(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = number_to_open;
                FNUMB.Text = number_to_open.ToString();
                FNUMB.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        public ObservableCollection<DTL_MANF> SUB_DATA { get; } = new ObservableCollection<DTL_MANF>();
        public bool NowIsReady { get; private set; }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int DG_SUB_DEF_INDEX_COL
        {
            get
            {
                if (DG_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = DG_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODE")?.DisplayIndex;
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
        public DTL_MANF? CURRENT_ITEMS_ROW { get; private set; }
        public DTL_MANF? WAS_ROW_ITEM { get; private set; } = new DTL_MANF();
        public DTL_MANF FROM_SEARCH_KAL { get; set; } = new DTL_MANF();

        #region LOCAL_MODEL
        public class CODE_MODEL
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
        }
        #endregion

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;

        private decimal sum_of_megh_k = 0;
        public decimal SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (decimal)SUB_DATA.Sum(r => r.MEGHk);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        private double _SUM_OF_MABL_K = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _SUM_OF_MABL_K = (double)SUB_DATA.Sum(r => r.MABLK ?? 0);
                if (_SUM_OF_MABL_K == 0) _SUM_OF_MABL_K = 0;
                return _SUM_OF_MABL_K;
            }
            set { _SUM_OF_MABL_K = value; }
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

        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                BTN_SAVE.IsEnabled = ican;

                // --- Header Input Controls ---
                DATE_ACTIV.IsReadOnly = !ican;
                TOZIH.IsReadOnly = !ican;
                SA_HOUR.IsReadOnly = !ican;
                SA_NHOU.IsReadOnly = !ican;
                IMBIBE_MANF.IsReadOnly = !ican;
                IMBIBE_SAR.IsReadOnly = !ican;

                GHEYMAT.IsEnabled = ican;
                CODE.IsEnabled = ican;

                // --- DataGrid Control ---
                if (true)
                {

                }
                DG_SUB.IsReadOnly = !ican;
            }
        }

        public int ANBARDefaultValue { get; private set; }
        public double Meidnum { get; private set; }
        public Visual IAM_HEAD_MANF { get; private set; }

        private NavigationManager<HEAD_MANF_MODEL> _navigationManager;

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is HEAD_MANF_MODEL item)
            {
                if (item != null)
                {
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.FNUMB.Equals(item.FNUMB));
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;
                        // Find its index in the master list
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx < 0)
                        {
                            // not found (perhaps filtered out?), bail out
                            new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                            return;
                        }
                        // Tell the navigation manager to move to that position
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                    }
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره فرمول", PropertyPath = "FNUMB", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "کد کالا", PropertyPath = "CODE", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "نام کالا", PropertyPath = "NAME_CODE", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تاریخ فعال", PropertyPath = "DATE_ACTIV", PropertyType = typeof(long?) },
                new SearchableProperty { DisplayName = "ضریب ساخت", PropertyPath = "IMBIBE_MANF", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "ضریب سربار", PropertyPath = "IMBIBE_SAR", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "قیمت", PropertyPath = "GHEYMAT", PropertyType = typeof(double?) },
                new SearchableProperty { DisplayName = "توضیحات", PropertyPath = "TOZIH", PropertyType = typeof(string) },
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

            IAM_HEAD_MANF = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            const string REPLACEMENT_VALUE = "dbo.HEAD_LST.";

            string WhereCondition = ""; //= CL_LMethods.GetRestrictedSqlQuery(0).Replace(REPLACEMENT_VALUE, null);

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE FNUMB = {NUMBER_TO_OPEN}";
            }

            _navigationManager = new NavigationManager<HEAD_MANF_MODEL>(
                dbms,
                x => x.FNUMB.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_MANF {WhereCondition} ORDER BY FNUMB", //All Record of The Table
              /*on navigation get ever record where*/ x => $"SELECT TOP 1 FNUMB, CODE, DATE_ACTIV, IMBIBE_MANF, IMBIBE_SAR, GHEYMAT, NAMES, N_KOL, NUMBER, TNUMBER, SA_HOUR, SA_NHOU, TOZIH, CRT, UID, ID FROM HEAD_MANF WHERE FNUMB = {x.FNUMB} ", //On Change for One Record
            Convert.ToDouble(FNUMB.Text)
            );

            _navigationManager.CanChangeRecord = CheckForUnsavedChanges;


            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;


            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            Form_Current();

            //AllowEdits = false;

            CL_LMethods.SetTabIndexes(
             CODE, DATE_ACTIV, SA_HOUR, SA_NHOU, IMBIBE_MANF, IMBIBE_SAR, TOZIH,
             BTN_SAVE,
             DG_SUB
             );

            MakeDefaultFocuseReady();
        }
        private void Form_Current()
        {
            if (_navigationManager.IsNewRecord || string.IsNullOrEmpty(CODE.SelectedValue?.ToStringNullSafe()))
            {
                this.DG_SUB.IsReadOnly = true;
            }
            else
            {
                this.DG_SUB.IsReadOnly = false;
            }
            this.AllowDeletions = false;
            this.AllowEdits = false;
        }

        private bool OnInsertRecord(HEAD_MANF_MODEL record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_MANF_MODEL>($"SELECT TOP 1 * FROM HEAD_MANF WHERE FNUMB = {FNUMB.Text} ").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(HEAD_MANF_MODEL HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
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
                FNUMB.Text = HEADER_FAC.FNUMB.ToString();
                FNUMB.Tag = HEADER_FAC.FNUMB.ToString();

                CODE.SelectedValue = HEADER_FAC.CODE;

                DATE_ACTIV.Text = HEADER_FAC.DATE_ACTIV.ToStringNullSafe();

                SA_HOUR.Text = HEADER_FAC.SA_HOUR.ToString();
                SA_NHOU.Text = HEADER_FAC.SA_NHOU.ToString();
                IMBIBE_MANF.Text = HEADER_FAC.IMBIBE_MANF.ToString();
                IMBIBE_SAR.Text = HEADER_FAC.IMBIBE_SAR.ToString();

                GHEYMAT.SelectedValue = HEADER_FAC.GHEYMAT;

                TOZIH.Text = HEADER_FAC.TOZIH;

                //SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1);
                //SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2);
                //SGN3.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN3);

                //SGN1usid.Tag = Convert.ToInt32(HEADER_FAC.sgn1usid);
                //SGN2usid.Tag = Convert.ToInt32(HEADER_FAC.sgn2usid);
                //SGN3usid.Tag = Convert.ToInt32(HEADER_FAC.sgn3usid);

                //SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn1usid)?.SAL_NAME;
                //SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.SAL_NAME;
                //SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn3usid)?.SAL_NAME;

                //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                //PERSONEL.Text = null;
                //PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                BTN_SAVE.IsEnabled = false;
                ItwasNewFirstTime = false; //Reset for Sanad Concurrency at first insert

                DG_SUB_ReGetData();

                Form_Current();
            }
        }
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_MANF_MODEL>($"SELECT TOP 1 * FROM HEAD_MANF WHERE FNUMB = {FNUMB.Text} ").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private int _saveErrorCount = 0;
        private bool CheckForUnsavedChanges()
        {
            if (!ChangeIsHappend)
            {
                return true;
            }

            var MSGCAP = new MSGCAPTIONMODEL()
            {
                YES_CAPTION = "ذخـیره و ادامه",
                NO_CAPTION = "بدون ذخیره ادامه بده"
            };

            string message = "تغییرات شما ذخیره نشده است. آیا مایل به ذخیره کردن هستید؟";

            Msgwin msgwin = new Msgwin(true, message, default, default, MSGCAP);
            bool? dialogResult = msgwin.ShowDialog();

            if (dialogResult == true)
            {
                // کاربر درخواست ذخیره کرده است
                try
                {
                    // اگر BTN_SAVE_Click از sender استفاده می‌کند،
                    // اینجا "this" را می‌فرستیم نه default تا NullReferenceException نگیریم.
                    BTN_SAVE_Click(this, new RoutedEventArgs());

                    // اگر ذخیره موفق بود، فلگ تغییرات را پاک می‌کنیم
                    ChangeIsHappend = false;
                    _saveErrorCount = 0; // ریست شمارنده خطا

                    return true; // ذخیره موفق بود، اجازه ادامه عملیات
                }
                catch (Exception ex)
                {
                    _saveErrorCount++;
                    new Msgwin(false, "در هنگام ذخیره‌سازی خطایی رخ داد.\n").Show();
                    return false; // ذخیره ناموفق، جلوگیری از ادامه
                }
            }
            else if (dialogResult == false)
            {
                // کاربر "بدون ذخیره ادامه بده" را زده
                ChangeIsHappend = false;
                _saveErrorCount = 0;
                return true;
            }
            else // dialogResult == null
            {
                // کاربر دیالوگ را بسته (انصراف)
                return false;
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // اگر هیچ تغییری انجام نشده، نیازی به دیالوگ نیست
            if (!ChangeIsHappend)
            {
                return;
            }

            var MSGCAP = new MSGCAPTIONMODEL()
            {
                YES_CAPTION = "ذخـیره و خروج",
                NO_CAPTION = "صرفا خارج شو"
            };

            string message = "تغییرات شما ذخیره نشده است. آیا مایل به ذخیره کردن هستید؟";

            Msgwin msgwin = new Msgwin(true, message, default, default, MSGCAP);
            bool? dialogResult = msgwin.ShowDialog();

            if (dialogResult == true)
            {
                // کاربر درخواست ذخیره کرده است
                try
                {

                    // اگر BTN_SAVE_Click از sender استفاده می‌کند،
                    // اینجا "this" را می‌فرستیم نه default تا NullReferenceException نگیریم.
                    BTN_SAVE_Click(this, new RoutedEventArgs());

                    // اگر ذخیره موفق بود، فلگ تغییرات را پاک می‌کنیم
                    ChangeIsHappend = false;

                    _saveErrorCount = 0; // ریست شمارنده خطا برای دفعات بعدی
                    // نکته مهم:
                    // در اینجا e.Cancel را دست نمی‌زنیم → بستن فرم ادامه پیدا می‌کند.
                    // اگر ذخیره داخل BTN_SAVE_Click شکست بخورد و Exception بدهد،
                    // catch زیر مانع خروج می‌شود.
                }
                catch (Exception ex)
                {
                    // 1. شمارنده‌ی خطا را افزایش بده
                    _saveErrorCount++;

                    new Msgwin(false, "در هنگام ذخیره‌سازی خطایی رخ داد و عملیات بستن پنجره لغو شد.\n").Show();
                    if (_saveErrorCount < 2)
                    {
                        // در اولین خطا اجازه‌ی بسته شدن پنجره را نمی‌دهیم
                        e.Cancel = true;
                        // ChangeIsHappend را دست نمی‌زنیم تا کاربر بداند هنوز ذخیره نشده
                        return;
                    }
                    else
                    {
                        // در این مرحله، کاربر عملاً می‌خواهد از این خطا خلاص شود
                        // و ما می‌پذیریم که بدون ذخیره پنجره بسته شود.
                        ChangeIsHappend = false;

                        // نکته‌ی مهم:
                        // این‌جا e.Cancel را true نمی‌کنیم.
                        // مقدار پیش‌فرض e.Cancel = false است، پس پنجره بسته می‌شود.
                        return;
                    }
                }
            }
            else if (dialogResult == false)
            {
                // کاربر "صرفا خارج شو" را زده → خروج بدون ذخیره
                ChangeIsHappend = false;
                _saveErrorCount = 0;
            }
            else // dialogResult == null
            {
                // کاربر دیالوگ را بسته (ضربدر، ESC، Alt+F4 روی دیالوگ و ...)
                // این یعنی می‌خواهد از عملیات بستن "انصراف" بدهد.
                e.Cancel = true;
            }
        }

        private void MakeDefaultFocuseReady()
        {
            CODE.Focus();
        }
        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(FNUMB.Text) || FNUMB.Text == "0")
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                DG_SUB.IsReadOnly = false;
            }

            //SecurityAllCheck();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = DG_SUB;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (DG_SUB.IsKeyboardFocusWithin)
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

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DG_SUB_DEF_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

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

            // F7 Search Dialog - Only when DataGrid is not focused
            if (DG_SUB != null && !DG_SUB.IsKeyboardFocusWithin && !DG_SUB.IsFocused)
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

        private void CODE_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CODE.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CODE_TEX = (TextBox)CODE.Template.FindName("PART_EditableTextBox", CODE);
            if (CODE_TEX is null)
            {
                return;
            }
            if (CODE.SelectedValue is not null)
            {
                if ((CODE.SelectedItem as CODE_MODEL)?.NAME == CODE_TEX.Text)
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(CODE_TEX.Text))
            {
                return;
            }

            var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, CODE_TEX.Text);
            if (RST_KALA != null)
            {
                CODE.SelectedValue = RST_KALA.CODE; CODE.Items.Refresh();
            }
            else
            {
                universControl.PopNotifyShowUp("چنین کالایی وجود ندارد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return;
            }

            //string currentCode = CODE.SelectedValue?.ToString();
            //if (string.IsNullOrWhiteSpace(currentCode))
            //{
            //    DG_SUB.IsReadOnly = true;
            //    return;
            //}
            //else
            //{
            //    DG_SUB.IsReadOnly = false;
            //}
        }
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (!DATE_IS_VALID())
            {
                //e.Handled = true;
            }
        }


        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "FORMOL", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(DG_SUB, "HEAD_MANF");

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
                var rst = dbms.DoGetDataSQL<int?>("SELECT ANBCO FROM dbo.OPANBACCESS WHERE (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
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
            //کالا
            CODE.ItemsSource = dbms.DoGetDataSQL<CODE_MODEL>($"SELECT CODE, NAME + N' ' + CODE AS NAME FROM STUF_DEF WHERE (RADAH > 1) ORDER BY NAME + N' ' + CODE").ToList();

            //انبار کالا
            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            List<COMBOYMODEL> persianMonths = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1,  NAME = "فروردین" },
                new COMBOYMODEL { ID = 2,  NAME = "اردیبهشت" },
                new COMBOYMODEL { ID = 3,  NAME = "خرداد" },
                new COMBOYMODEL { ID = 4,  NAME = "تیر" },
                new COMBOYMODEL { ID = 5,  NAME = "مرداد" },
                new COMBOYMODEL { ID = 6,  NAME = "شهریور" },
                new COMBOYMODEL { ID = 7,  NAME = "مهر" },
                new COMBOYMODEL { ID = 8,  NAME = "آبان" },
                new COMBOYMODEL { ID = 9,  NAME = "آذر" },
                new COMBOYMODEL { ID = 10, NAME = "دی" },
                new COMBOYMODEL { ID = 11, NAME = "بهمن" },
                new COMBOYMODEL { ID = 12, NAME = "اسفند" }
            };
            //ماه
            GHEYMAT.ItemsSource = persianMonths;
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            string date_n_val = DATE_ACTIV.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_ACTIV.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (_navigationManager.IsNewRecord)
                    {
                        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                        {
                            DATE_ACTIV.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
                            ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                        }
                    }
                    else
                    {
                        universControl.PopNotifyShowUp("هشدار : تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow, 1);
                    }
                }
            }
            else
            {
                DATE_ACTIV.Text = _navigationManager.CurrentRecord?.DATE_ACTIV?.ToStringNullSafe();
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (CODE.SelectedValue is null) //کالا
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا نمیتواند خالی باشد." });
            }
            if (!CL_LMethods.IsNumeric(SA_HOUR.Text)) //جذب ساعت کار
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب ساعت کار معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(SA_NHOU.Text)) //نرخ ساعت کار استاندارد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار نرخ ساعت کار استاندارد معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(IMBIBE_MANF.Text)) //جذب هزینه دستمزد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب هزینه دستمزد معتبر نیست" });
            }
            if (!CL_LMethods.IsNumeric(IMBIBE_SAR.Text)) //جذب هزینه سربار
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار جذب هزینه سربار معتبر نیست" });
            }

            int FnumbCode = Convert.ToInt32(FNUMB.Text);
            string sql = "SELECT FNUMB FROM HEAD_MANF WHERE CODE = @CODE";
            var parameters = new { CODE = CODE.SelectedValue };
            var existingFormula = dbms.DoGetDataSQL<HEAD_MANF_MODEL>(sql, parameters).FirstOrDefault();
            if (existingFormula != null)
            {
                bool AllowedToMultipleFormula = false;
                if (!string.IsNullOrEmpty(Baseknow.OPTIONSS) && Baseknow.OPTIONSS.Length > 55 && Baseknow.OPTIONSS.Substring(55, 1) == "5") //چند فرموله عمل کند تیک شماره 36 Check56
                {
                    AllowedToMultipleFormula = true;
                }

                if (!AllowedToMultipleFormula)
                {
                    if (_navigationManager.IsNewRecord)
                    {
                        if (existingFormula.FNUMB != null)
                        {
                            ErrosMessages.Add(new MsgModel { MessageText_U = "کاربر گرامی برای این کالا قبلا فرمول تعریف شده است. دقت کنید که عملیات را بصورت صحیح انجام داده باشید , \"چند فرموله عمل کند\" برای سیستم فعال نیست" });
                        }
                    }
                    else if (FnumbCode != existingFormula.FNUMB)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "کاربر گرامی برای این کالا قبلا فرمول تعریف شده است. دقت کنید که عملیات را بصورت صحیح انجام داده باشید , \"چند فرموله عمل کند\" برای سیستم فعال نیست" });
                        //universControl.PopNotifyShowUp("کاربر گرامی برای این کالا قبلا فرمول تعریف شده است. دقت کنید که عملیات را بصورت صحیح انجام داده باشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow, 1);
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
        private bool BodyIsValid(DTL_MANF TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // Validate ANBAR
            if (!int.TryParse(TheRow.ANBAR.ToStringNullSafe(), out int _))
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
            if (!double.TryParse(TheRow.MEGH.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }

            if (!double.TryParse(TheRow.MEGHk.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            if (!int.TryParse(TheRow.VAHED_K.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (!double.TryParse(TheRow.PERT.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "پِرت صحیح وارد نشده" });
            }

            if (!double.TryParse(TheRow.SMABL.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ استاندارد صحیح وارد نشده" });
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        public bool ItwasNewFirstTime { get; set; } = false;
        public object NUMBER_TO_OPEN { get; private set; }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                DoCmdHeaderSave();
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "اطلاعات سربرگ تکراری وارد شده است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات ذخیره سربرگ!").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره سربرگ!").ShowDialog(); return;
            }

            this.DG_SUB.IsReadOnly = false;

            Summer();

            SANAD();

            if (!ItwasNewFirstTime) //برای جلوگیری از درج داده در صورت فوق همزمان برای درج جدید خالی در درجه اول سند نزنه
            {
            }
            ItwasNewFirstTime = false; //ریست کردن این متفیری

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (SUB_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }

        private void SANAD()
        {
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!string.IsNullOrEmpty(FNUMB.Text) && FNUMB.Text != "0")
            {
                SecurityAllCheck();

                GET_TR();

                //var _SGN1_ = Convert.ToBoolean(SGN1.IsChecked ?? false);
                //var _SGN2_ = Convert.ToBoolean(SGN2.IsChecked ?? false);
                //var _SGN3_ = Convert.ToBoolean(SGN3.IsChecked ?? false);

                //if (_SGN1_ || _SGN2_ || _SGN3_)
                //{
                //    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                //    DG_SUB.IsReadOnly = true;
                //    this.AllowEdits = false;
                //    this.AllowDeletions = false;
                //}
                //else
                {
                    DG_SUB.IsReadOnly = false;
                    this.AllowEdits = true;
                    this.AllowDeletions = true;
                }
            }
        }

        private void GET_TR()
        {
            var dt = DateTime.Now;
            CL_HESABDARI.TR("HEAD_MANF", "(FNUMB = " + FNUMB.Text + $") ", dt, 1);
            CL_HESABDARI.TR("DTL_MANF", "(FNUMB = " + FNUMB.Text + $") ", dt, 1);
        }
        public bool DATE_IS_VALID(bool DisplayMsg = false)
        {
            bool Date_Is_Valid = true;

            var DATE = DATE_ACTIV.Text.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    }
                    Date_Is_Valid = false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        if (DisplayMsg)
                        {
                            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        }
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible || _navigationManager.IsNewRecord) { return; }

            var editableCollectionView = DG_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
            {
                //to avoid any error because user might leave edited (not cimmitted) cell in DataGrid
                try { editableCollectionView.CancelEdit(); } catch { }
            }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {

                if (SUB_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
                {
                    if (DG_SUB.SelectedItem is not DTL_MANF) { return; }
                    var NewRow = ((DTL_MANF)DG_SUB.SelectedItem).Clone() as DTL_MANF;
                    _ = AuditLogger.LogActionAsync(
                                      actionType: "Delete",
                                      tableName: "ایجاد فرمول ساخت : سطر ها",
                                      recordId: FNUMB.Text,
                                      oldValue: default,
                                      newValue: NewRow,
                                      additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    GET_TR();

                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                    {
                        var item = DG_SUB.SelectedItems[i] as DTL_MANF;

                        if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                        {
                            continue; // Skip deletion for new placeholder items
                        }

                        if (item?.FNUMB != null && item?.CODE != null)
                        {
                            try
                            {
                                const string delSql = @"DELETE FROM dbo.DTL_MANF
                                                        WHERE FNUMB = @P_FNUMB
                                                        AND CODE  = @P_CODE;";
                                dbms.DoExecuteSQL(delSql, new
                                {
                                    P_FNUMB = (int)item.FNUMB,
                                    P_CODE = (item.CODE).Trim()
                                });
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

                    DG_SUB_ReGetData();
                    SANAD();
                }
                else
                {
                    _ = AuditLogger.LogActionAsync(
                                   actionType: "Delete",
                                   tableName: "ایجاد فرمول ساخت : سربرگ",
                                   recordId: FNUMB.Text,
                                   oldValue: default,
                                   newValue: CODE.SelectedValue?.ToString(),
                                   additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    if (!string.IsNullOrEmpty(FNUMB.Text) && FNUMB.Text != "0" && !string.IsNullOrEmpty(FNUMB.Text) && FNUMB.Text != "0")
                    {
                        string sql = @"SELECT NUMBER, TAG, N_KOL 
                           FROM dbo.INVO_LST 
                           WHERE TAG = 9 AND N_KOL = @FNUMB";
                        var result = dbms.DoGetDataSQL<INVO_LST>(sql, new { FNUMB = FNUMB.Text }).ToList();
                        if (result.Count > 0)
                        {
                            string number = result.First().NUMBER.ToString();
                            string msg = $"اين فرمول در توليد شماره {number} به کار رفته و قابل حذف نيست";
                            new Msgwin(false, msg).ShowDialog();
                            return;
                        }

                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_MANF WHERE FNUMB = {FNUMB.Text} ");

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
                                new Msgwin(false, "این برگه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
                        DG_SUB_ReGetData();
                    }
                }
            }
        }
        private bool DoCmdHeaderSave()
        {
            if (FNUMB.Text == "0" || string.IsNullOrWhiteSpace(FNUMB.Text)) //Is Really New
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        //Fake Query for Lock Table
                        db.Execute("SELECT TOP 1 FNUMB FROM dbo.HEAD_MANF WITH (TABLOCKX, HOLDLOCK)", null, transaction);
                        //Fake Query for Lock Table

                        var rst_11 = db.Query<double?>($"SELECT Max(HEAD_MANF.FNUMB) AS MaxOfFNUMB FROM HEAD_MANF", null, transaction).FirstOrDefault();
                        if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                        {
                            FNUMB.Text = "1";
                            FNUMB.UpdateLayout();
                        }
                        else
                        {
                            FNUMB.Text = Convert.ToDouble(rst_11 + 1).ToString();
                            FNUMB.UpdateLayout();
                        }

                        if (string.IsNullOrWhiteSpace(FNUMB.Text) || FNUMB.Text == "0")
                        {
                            throw new Exception("خطایی رخ داده , شماره فرمول صفر است !");
                        }

                        const string insertSql = @"
                            INSERT INTO dbo.HEAD_MANF (
                                FNUMB, CODE, DATE_ACTIV, IMBIBE_MANF, IMBIBE_SAR, GHEYMAT, NAMES, 
                                N_KOL, NUMBER, TNUMBER, SA_HOUR, SA_NHOU, TOZIH, UID
                            ) VALUES (
                                @FnumValue, @CodeValue, @DateActivValue, 0.0, 0.0, 0.0, NULL, 
                                NULL, NULL, NULL, 0.0, 0, @TozihValue, @UserIdValue
                            )";
                        var parameters = new
                        {
                            FnumValue = int.Parse(FNUMB.Text),
                            CodeValue = CODE.SelectedValue.ToString(),
                            DateActivValue = long.Parse(DATE_ACTIV.Text.ToRawTarikh()),
                            TozihValue = TOZIH.Text.Trim(),
                            UserIdValue = Baseknow.USERCOD
                        };

                        db.Execute(insertSql, parameters, transaction);

                        transaction.Commit();
                        db?.Close();

                        ItwasNewFirstTime = true;

                        _navigationManager.IsNewRecord = false;
                        RefreshAfterUpdate();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(FNUMB.Text) || FNUMB.Text == "0")
            {
                throw new Exception("خطایی رخ داده , شماره فرمول صفر است !");
            }

            const string updateQuery = @"
                UPDATE dbo.HEAD_MANF 
                SET 
                    CODE = @Code,
                    DATE_ACTIV = @DateActiv,
                    IMBIBE_MANF = @ImbibeManf,
                    SA_NHOU = @SaNhou,
                    SA_HOUR = @SaHour,
                    IMBIBE_SAR = @ImbibeSar,
                    GHEYMAT = @GHEYMAT,
                    TOZIH = @Tozih
                WHERE FNUMB = @Fumb";
            var parameters2 = new
            {
                Fumb = int.Parse(FNUMB.Text),
                Code = CODE.SelectedValue?.ToString(),
                DateActiv = long.Parse(DATE_ACTIV.Text.ToRawTarikh()),
                ImbibeManf = decimal.Parse(IMBIBE_MANF.Text),
                SaNhou = decimal.Parse(SA_NHOU.Text),
                SaHour = decimal.Parse(SA_HOUR.Text),
                ImbibeSar = decimal.Parse(IMBIBE_SAR.Text),
                GHEYMAT = Convert.ToInt32(GHEYMAT.SelectedValue),
                Tozih = TOZIH.Text.Trim(),
            };
            _ = dbms.DoExecuteSQL(updateQuery, parameters2);

            return true;
        }
        public void DG_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(FNUMB.Text) && FNUMB.Text != "0")
            {
                // The SQL query is updated to target DTL_MANF and join STUF_DEF for the name.
                const string SQL_QUERY = @"
                    SELECT 
                        D.FNUMB, 
                        D.CODE, 
                        S.NAME AS NAME_CODE, 
                        D.ANBAR, 
                        D.VAHED_K, 
                        D.MEGH, 
                        D.MEGHk, 
                        D.TOZIH,
                        D.PERT,
                        D.SMABL, D.MABLK ,
                        D.ID, 
                        D.CRT, 
                        D.UID
                    FROM dbo.DTL_MANF D
                    LEFT JOIN dbo.STUF_DEF S ON D.CODE = S.CODE
                    WHERE D.FNUMB = @Fumb";
                var parameters = new Dictionary<string, object>
                {
                    { "@Fumb", int.Parse(FNUMB.Text) } // Use 'Fumb' to match the query and parse to int
                };
                var QRE_LST = dbms.DoGetDataSQL<DTL_MANF>(SQL_QUERY, parameters).ToList();
                SUB_DATA?.Clear();
                foreach (var item in QRE_LST)
                {
                    SUB_DATA.Add(item);
                }

                Summer();
            }
            else
            {
                SUB_DATA?.Clear();
            }
        }

        private void Summer()
        {
            SUM_AVALIEH.Text = SUM_OF_MEGH_K.ToString(); //جمع مواد اولیه مصرفی
            SUM_TAMAMSHODEH.Text = (SUM_OF_MABL_K + Convert.ToDouble(IMBIBE_MANF.Text) + Convert.ToDouble(IMBIBE_SAR.Text)).ToString(); //قیمت تمام شده استاندارد
        }

        public bool IsPastingRows { get; private set; } = false;
        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || DG_SUB == null || DG_SUB.CurrentCell == null)
                return;

            string CURRENT_COLUMN_NAME = "";
            if (DG_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = DG_SUB.CurrentCell.Column?.SortMemberPath;
            }

            var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;
            if (e.Key == Key.Delete)
            {
                try
                {
                    if (isEditing) { return; }
                    // 1) اگر داخل یک TextBox در حالت ویرایش هستیم، کاری نکنیم
                    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
                    {
                        // اجازه بدهید Delete عادی متن کارش رو بکنه
                        return;
                    }
                    //else
                    //{
                    //    // اگر داخل حالت ویرایش سلول هستیم، از رفتار پیش‌فرض Delete (حذف کاراکتر) استفاده کن
                    //    var cell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                    //    if (cell != null && cell.IsEditing)
                    //        return;
                    //}
                }
                catch { }

                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME == "SMABL" || CURRENT_COLUMN_NAME == "MABLK")
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
                if (CURRENT_COLUMN_NAME == "SMABL" || CURRENT_COLUMN_NAME == "MABLK")
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

            #region COPYPASTE
            var isNewEmpty = ((IEditableCollectionView)DG_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && DG_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<DTL_MANF>(DG_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !DG_SUB.IsReadOnly && DG_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<DTL_MANF>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }
            #endregion
        }
        private void ValidateDataGridRow(DataGridRowEditEndingEventArgs args, PasteValidationResult validationResult)
        {
            // Default to true
            validationResult.IsRowValid = true;

            if (args.Row.Item is DTL_MANF item)
            {
                //Reset id to be sure the new data will insert not update the same row existing before
                item.UID = null;
                item.CRT = null;
                CURRENT_ITEMS_ROW = item;

                //مبلغ استاندارد
                var _SMABL_ = item.SMABL.ToStringNullSafe();
                if (!string.IsNullOrEmpty(_SMABL_))
                {
                    var MABL_NUMBER = NumberExtractor.ExtractNumbersLine(_SMABL_);
                    if (!string.IsNullOrEmpty(MABL_NUMBER))
                    {
                        item.SMABL = Convert.ToDouble(MABL_NUMBER);
                    }
                    else
                    {
                        args.Cancel = true;
                        validationResult.IsRowValid = false;
                        validationResult.RowMessage = "فیلد مبلغ استاندارد وارد شده صحیح نیست";
                    }
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "فیلد مبلغ استاندارد نمیتواند خالی باشد";
                }

                //مبلغ کل
                var _MABLK_ = item.MABLK.ToStringNullSafe();
                if (!string.IsNullOrEmpty(_MABLK_))
                {
                    var MABL_NUMBER = NumberExtractor.ExtractNumbersLine(_MABLK_);
                    if (!string.IsNullOrEmpty(MABL_NUMBER))
                    {
                        item.SMABL = Convert.ToDouble(MABL_NUMBER);
                    }
                    else
                    {
                        args.Cancel = true;
                        validationResult.IsRowValid = false;
                        validationResult.RowMessage = "فیلد مبلغ کل وارد شده صحیح نیست";
                    }
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                    validationResult.RowMessage = "فیلد مبلغ کل نمیتواند خالی باشد";
                }

                //Final Validation
                if (validationResult.IsRowValid) //Yet
                {
                    DG_SUB_RowEditEnding(DG_SUB, args);
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
        private void AddItemToDataSource(DTL_MANF item)
        {
            // Ensure thread safety if MY_ALL_DATA is accessed from multiple threads
            Application.Current.Dispatcher.Invoke(() =>
            {
                SUB_DATA.Add(item);
            });
        }
        private bool IsSubDataNull()
        {
            if (DG_SUB != null && DG_SUB?.Items?.Count > 0 && SUB_DATA?.Count > 0)
            {
                return false;
            }
            return true;
        }
        private void COPY_CLICK(object sender, RoutedEventArgs e)
        {
            if (IsSubDataNull())
            {
                return;
            }

            var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;
            if (!isEditing)
            {
                e.Handled = true;
                DataGridClipboardManager.CopySelectedItems<DTL_MANF>(DG_SUB);
            }
            else
            {
                var editingElement = CL_LMethods.FindChild<TextBox>(DG_SUB);
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
            if (DG_SUB.SelectedItem != null || DG_SUB.SelectedItems.Count > 0)
            {
                var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;
                if (!isEditing && !DG_SUB.IsReadOnly && DG_SUB.IsEnabled)
                {
                    e.Handled = true;

                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<DTL_MANF>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;

                    DG_SUB.CommitEdit();
                }
                else
                {
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
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            if (IsSubDataNull())
            {
                return;
            }

            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(DG_SUB, "DGExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }


        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(DG_SUB.Items.Count < 1) && !(DG_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = DG_SUB.SelectedIndex;
                }
            }
        }
        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && DG_SUB.SelectedItem != null)
            {
                if (DG_SUB.Items.Count > 0)
                {
                    CURRENT_ROW_INDEX = DG_SUB.SelectedIndex;
                }

                if (!(e is null) && DG_SUB.SelectedItem is not null)
                {
                    var view = DG_SUB.Items as IEditableCollectionView;
                    if (view.IsAddingNew) { return; }

                    if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((DTL_MANF)DG_SUB.SelectedItem).Clone() as DTL_MANF;
                    }
                }
            }
        }
        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is TOZIE_SUB rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = DG_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as DTL_MANF;
        }
        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.InvokeAsync(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_SUB.CancelEdit();
                }
                else
                {
                    DG_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as DTL_MANF;
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
        }

        private void ValidateUpdateCurrentMeghk()
        {
            var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT TOP 1 VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
            if (RSTV1.Count == 0)
            {
                universControl.PopNotifyShowUp("واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = (double)(CURRENT_ITEMS_ROW.MEGH * (double)RSTV1.FirstOrDefault().NESBAT);
                CURRENT_ITEMS_ROW.MABLK = ((CURRENT_ITEMS_ROW?.PERT ?? 0) + (CURRENT_ITEMS_ROW?.MEGHk ?? 0)) * (CURRENT_ITEMS_ROW?.SMABL ?? 0);
            }
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.EditingElement == null || e.Column == null) { return; }

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

            CURRENT_ITEMS_ROW = e.Row.Item as DTL_MANF;
            #endregion

            //انبار
            #region ANBAR
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                            DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                    }
                }
            }
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.NAME_CODE.ToStringNullSafe().Trim() ||
                    string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    #region CODE_NotInList
                    if (CURRENT_ITEMS_ROW?.ANBAR is null) // انبار خالی نیست
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                    {
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        return;
                    }

                    var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, Convert.ToString(CURRENT_ITEMS_ROW.ANBAR), ENTERED_VALUE_ROW);
                    if (RST_KALA != null)
                    {
                        CURRENT_ITEMS_ROW.CODE = RST_KALA.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = RST_KALA.NAME_CODE;

                        CURRENT_ITEMS_ROW.VAHED_K = (int)CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        CURRENT_ITEMS_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        universControl.PopNotifyShowUp("چنین کدی وجود ندارد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        return;
                    }

                    if (!string.IsNullOrEmpty(CURRENT_ITEMS_ROW?.CODE.ToStringNullSafe()))
                    {
                        var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT TOP 1 * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                        if (RST.Count == 0)
                        {
                            universControl.PopNotifyShowUp("كالا به انبار فوق تعلق ندارد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                            DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
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
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe())) //واحد
                {
                    return;
                }
                if (CURRENT_ITEMS_ROW?.ANBAR == null || CURRENT_ITEMS_ROW?.CODE is null)
                {
                    return;
                }
                if ((CURRENT_ITEMS_ROW?.VAHED_K is null) || (CURRENT_ITEMS_ROW?.CODE is null) || (CURRENT_ITEMS_ROW?.NAME_CODE is null))
                {
                    CURRENT_ITEMS_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                    DG_SUB_CANCEL_EDIT();
                    return;
                }

                ValidateUpdateCurrentMeghk();
            }
            #endregion

            //مقدار
            #region MEGH
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (CURRENT_ITEMS_ROW?.ANBAR is null || CURRENT_ITEMS_ROW?.CODE is null || CURRENT_ITEMS_ROW?.VAHED_K is null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGH = 0;
                    return;
                }
                if (CURRENT_ITEMS_ROW?.ANBAR is null || CURRENT_ITEMS_ROW?.CODE is null || CURRENT_ITEMS_ROW?.VAHED_K is null)
                {
                    return;
                }

                ValidateUpdateCurrentMeghk();
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

                ValidateUpdateCurrentMeghk();
            }
            #endregion

            //مبلغ کل
            if (e.Column.SortMemberPath == "MABL_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABLK = 0;
                }
            }

            CURRENT_ITEMS_ROW.MABLK = ((CURRENT_ITEMS_ROW?.PERT ?? 0) + (CURRENT_ITEMS_ROW?.MEGHk ?? 0)) * (CURRENT_ITEMS_ROW?.SMABL ?? 0);
        }

        bool IsSaveSuccess = true;
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var TheRow = e.Row.Item as DTL_MANF;
            if (TheRow == null) { return; }

            if (ConstructorRowDetector.IsPristine(TheRow)) { DG_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(TheRow))
            {
                IsSaveSuccess = false;
                DG_SUB_CANCEL_EDIT();
                return;
            }

            if (string.IsNullOrWhiteSpace(FNUMB.Text) || FNUMB.Text == "0")
            {
                throw new Exception("خطایی رخ داده , شماره فرمول صفر است !");
            }

            IsSaveSuccess = false;
            #region Re_Validate
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            //انبار خالی نباشد
            if (TheRow?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {TheRow.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (!string.IsNullOrWhiteSpace(TheRow.CODE))
            {
                var RST_STUF_STK = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).ToList();
                if (RST_STUF_STK.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {TheRow.CODE} به انبار {TheRow.ANBAR} فوق تعلق ندارد." });
                }
            }
            //بررسی صحیح بودن واحد کالا نسبت به خود کالا
            var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + TheRow.CODE + "' AND ((VAHEDS.VAHED)= " + TheRow.VAHED_K + ")))").ToList();
            if (RSTV1.Count == 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });
            }
            //واحد کالا بررسی مقدار کل باتوجه به نسبت
            else
            {
                var NesbatMegh = RSTV1.FirstOrDefault()?.NESBAT * TheRow.MEGH;
                if (NesbatMegh != TheRow.MEGHk)
                {

                    TheRow.MEGHk = (double)NesbatMegh;
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }

            SANAD();

            if (ErrosMessages.Any())
            {
                DG_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(ErrosMessages);
                return;
            }

            //Re Calculate Just In Case
            if (CURRENT_ITEMS_ROW != null)
            {
                CURRENT_ITEMS_ROW.MABLK = ((CURRENT_ITEMS_ROW?.PERT ?? 0) + (CURRENT_ITEMS_ROW?.MEGHk ?? 0)) * (CURRENT_ITEMS_ROW?.SMABL ?? 0);
            }
            TheRow.MABLK = ((TheRow?.PERT ?? 0) + (TheRow?.MEGHk ?? 0)) * (TheRow?.SMABL ?? 0);
            #endregion

            TheRow.FNUMB = Convert.ToInt32(FNUMB.Text); //Get Master Into Detail Value

            try
            {
                if (e.Row.IsNewItem || IsPastingRows)
                {
                    const string insertSql = @"
                    INSERT INTO dbo.DTL_MANF 
                        (FNUMB, CODE, ANBAR, VAHED_K, MEGH, MEGHk, PERT, SMABL, MABLK, TOZIH, CRT, UID)
                    VALUES 
                    (@FNUMB, @CODE, @ANBAR, @VAHED_K, @MEGH, @MEGHk, @PERT, @SMABL, @MABLK, @TOZIH, GETDATE(), @UID)";

                    dbms.DoExecuteSQL(insertSql, TheRow);
                }
                else
                {
                    const string updateSql = @"
                    UPDATE dbo.DTL_MANF SET
                        ANBAR = @ANBAR,
                        VAHED_K = @VAHED_K,
                        MEGH = @MEGH,
                        MEGHk = @MEGHk,
                        PERT = @PERT,
                        CODE = @CODE,
                        SMABL = @SMABL,
                        MABLK = @MABLK,
                        TOZIH = @TOZIH,
                        UID = @UID
                    WHERE FNUMB = @FNUMB AND ID = @ID";

                    dbms.DoExecuteSQL(updateSql, TheRow);
                }
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }

            #region CODE_AfterUpdate
            int TopHeadCode = Convert.ToInt32(CODE.SelectedValue); //HEAD_MANF_CODE
            double CurrentRowCode = Convert.ToDouble(TheRow.CODE); //Me.CODE

            var stuf = dbms.DoGetDataSQL<STUF_DEF>(@"SELECT TOP (1) VAHED, NAME FROM dbo.STUF_DEF WHERE CODE = @Code", new { Code = CurrentRowCode }).FirstOrDefault();
            if (stuf != null && !string.IsNullOrWhiteSpace(stuf.VAHED.ToStringNullSafe()))
            {
                TheRow.VAHED_K = stuf.VAHED;
            }

            int hazTol = (int)Baseknow.HAZ_TOL;
            int conKal = (int)Baseknow.CONKAL;
            int amalKard = (int)Baseknow.AMALKARD;

            string kalaName = CL_HESABDARI.GETKALANAME(CurrentRowCode) ?? CurrentRowCode.ToString();
            string rateName = "نرخ " + kalaName;

            // rst.Open "SELECT * FROM TDETA_HES WHERE (N_KOL=HAZ_TOL) And (NUMBER=HEAD_MANF_CODE) And (TNUMBER=Me.CODE)"
            bool existsHazTol = dbms.DoGetDataSQL<int>(@"SELECT COUNT(1) FROM dbo.TDETA_HES 
                      WHERE N_KOL = @N_KOL AND [NUMBER] = @NUMBER AND TNUMBER = @TNUMBER",
                new { N_KOL = hazTol, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode }).FirstOrDefault() > 0;

            if (!existsHazTol)
            {
                // rst.AddNew ... rst.Update
                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.TDETA_HES (N_KOL, [NUMBER], TNUMBER, [NAME], BED_BES)
                          VALUES (@N_KOL, @NUMBER, @TNUMBER, @NAME, -1)",
                    new { N_KOL = hazTol, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = kalaName });
            }
            else
            {
                // If rst.Fields(...) = ... Then rst.Fields(...)=... : Update همه فیلدها
                dbms.DoExecuteSQL(
                    @"UPDATE dbo.TDETA_HES
                          SET N_KOL=@N_KOL, [NUMBER]=@NUMBER, TNUMBER=@TNUMBER, [NAME]=@NAME, BED_BES=-1
                          WHERE N_KOL=@N_KOL AND [NUMBER]=@NUMBER AND TNUMBER=@TNUMBER",
                    new { N_KOL = hazTol, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = kalaName }
                );
            }

            // كنترل كالاي درجريان ساخت  → (N_KOL = CONKAL)
            bool existsConKal = dbms.DoGetDataSQL<int>(
                @"SELECT COUNT(1) FROM dbo.TDETA_HES 
                      WHERE N_KOL = @N_KOL AND [NUMBER] = @NUMBER AND TNUMBER = @TNUMBER",
                new { N_KOL = conKal, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode }
            ).FirstOrDefault() > 0;

            if (!existsConKal)
            {
                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.TDETA_HES (N_KOL, [NUMBER], TNUMBER, [NAME], BED_BES)
                          VALUES (@N_KOL, @NUMBER, @TNUMBER, @NAME, -1)",
                    new { N_KOL = conKal, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = kalaName }
                );
            }
            else
            {
                dbms.DoExecuteSQL(
                    @"UPDATE dbo.TDETA_HES
                          SET N_KOL=@N_KOL, [NUMBER]=@NUMBER, TNUMBER=@TNUMBER, [NAME]=@NAME, BED_BES=-1
                          WHERE N_KOL=@N_KOL AND [NUMBER]=@NUMBER AND TNUMBER=@TNUMBER",
                    new { N_KOL = conKal, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = kalaName }
                );
            }

            //عملكرد ماده → (N_KOL = AMALKARD) و NAME = "نرخ " & GETKALANAME(Me.CODE)
            bool existsAmalKard = dbms.DoGetDataSQL<int>(
                @"SELECT COUNT(1) FROM dbo.TDETA_HES 
                      WHERE N_KOL = @N_KOL AND [NUMBER] = @NUMBER AND TNUMBER = @TNUMBER",
                new { N_KOL = amalKard, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode }
            ).FirstOrDefault() > 0;

            if (!existsAmalKard)
            {
                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.TDETA_HES (N_KOL, [NUMBER], TNUMBER, [NAME], BED_BES)
                          VALUES (@N_KOL, @NUMBER, @TNUMBER, @NAME, -1)",
                    new { N_KOL = amalKard, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = rateName }
                );
            }
            else
            {
                dbms.DoExecuteSQL(
                    @"UPDATE dbo.TDETA_HES
                          SET N_KOL=@N_KOL, [NUMBER]=@NUMBER, TNUMBER=@TNUMBER, [NAME]=@NAME, BED_BES=-1
                          WHERE N_KOL=@N_KOL AND [NUMBER]=@NUMBER AND TNUMBER=@TNUMBER",
                    new { N_KOL = amalKard, NUMBER = TopHeadCode, TNUMBER = CurrentRowCode, NAME = rateName }
                );
            }
            #endregion

            Summer();

            IsSaveSuccess = true;
        }

        private void ClearFreshAll()
        {
            FNUMB.Text = "0";
            FNUMB.Tag = null;

            CODE.SelectedIndex = -1;
            GHEYMAT.SelectedIndex = -1;

            DATE_ACTIV.Text = Tarikh.FullCurrentDate;
            SA_HOUR.Text = "0";
            SA_NHOU.Text = "0";
            IMBIBE_MANF.Text = "0";
            IMBIBE_SAR.Text = "0";
            TOZIH.Text = null;

            SUB_DATA?.Clear();

            SUM_AVALIEH.Text = "0";
            SUM_TAMAMSHODEH.Text = "0";

            //PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.Text = null;
            //PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            //PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            //SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            //SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            //SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            Form_Current();

            AllowEdits = true;

            DG_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (DG_SUB.Items.Count > 0)
            {
                if (DG_SUB.SelectedItem is not null)
                {
                    var Row = DG_SUB.SelectedItem as DTL_MANF;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }
        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid == null) return;

            try
            {
                // اطمینان از فوکوس بودن DataGrid قبل از باز کردن Context Menu
                if (!dataGrid.IsKeyboardFocusWithin)
                {
                    dataGrid.Focus();
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

                    // تنظیم فوکوس روی سطر
                    row.Focus();

                    // Show the context menu
                    dataGrid.ContextMenu.IsOpen = true;

                    // Mark the event as handled to prevent the default context menu behavior
                    e.Handled = true;
                }
                else
                {
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }
        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            bool isWindowOpen = Application.Current.Windows.OfType<WIN_FOMULA_LST>().Any();
            if (!isWindowOpen)
            {
                new WIN_FOMULA_LST().Show();

                if (_navigationManager.IsNewRecord)
                {
                    this.Close();
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ليست فرمولها و مواد
            if (_navigationManager.IsNewRecord) { return; }

            new HEAD_MANF_DTL().Show();
        }

        #region GheymatTamamShodehRooz
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //قيمت تمام شده روز
            if (_navigationManager.IsNewRecord) { return; }
            if (!HeaderIsValid()) { return; }

            if (!long.TryParse(FNUMB.Text, out var formulaNumber) || formulaNumber == 0)
            {
                universControl.PopNotifyShow("شماره فرمول معتبر نیست.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            long persianDate = CL_HESABDARI.FARSIDATE();
            long prgId;

            using var connection = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
            connection.Open();

            SqlTransaction? transaction = null;

            Process Prc = ProcLoader.Start();

            try
            {
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                prgId = connection.ExecuteScalar<long>(
                    @"INSERT INTO dbo.PRGHEAD (PCODE, NUM, GRP, PRG_DATE, CRT, UID)
                      OUTPUT INSERTED.PRGID
                      VALUES (@PCODE, 1, 1010, @PRG_DATE, GETDATE(), @UID);",
                    new
                    {
                        PCODE = CODE.SelectedValue?.ToString(),
                        PRG_DATE = persianDate,
                        UID = Baseknow.USERCOD
                    },
                    transaction);

                connection.Execute(
                    @"INSERT INTO dbo.PROGPAS1 (PRGID, CODA, CODB, VAHED, MEGH, MEGHK, PERT, KOLMAV, MABL, MABLK, RADAH, PASED)
                      SELECT  @PRGID,
                              h.CODE,
                              d.CODE,
                              d.VAHED_K,
                              d.MEGH,
                              d.MEGHk,
                              d.PERT,
                              d.MEGHk + d.PERT,
                              d.SMABL,
                              d.MABLK,
                              s.RADAH,
                              0
                      FROM dbo.STUF_DEF AS s
                      INNER JOIN dbo.DTL_MANF AS d ON s.CODE = d.CODE
                      INNER JOIN dbo.HEAD_MANF AS h ON h.FNUMB = d.FNUMB
                      WHERE h.FNUMB = @FNUMB;",
                    new { PRGID = prgId, FNUMB = formulaNumber },
                    transaction);

                // -1 VBA ||| 1 C# 
                /*
                 * 5. PASED = 0 یا -1؟
                    در C# برای چک کردن رکوردهای پردازش نشده از ISNULL(PASED, 0) = 0 استفاده شده، اما اگر در update مقدار -1 استفاده شود، باید شرط را تغییر داد.
                 */
                //ISNULL(RADAH, 0) این یعنی اگری RADAH نال بود موقتا فرض کن صفره نه نال و پردازش کن
                //WHERE PRGID = @PRGID AND RADAH IS NOT NULL AND RADAH NOT IN (2,3);
                connection.Execute(
                    @"UPDATE dbo.PROGPAS1
                      SET PASED = 1
                      WHERE PRGID = @PRGID AND ISNULL(RADAH, 0) NOT IN (2, 3);", //UPDATE dbo.PROGPAS1 SET PASED = 1 WHERE RADAH = NULL (0) TOO 
                    new { PRGID = prgId },
                    transaction);

                while (true)
                {
                    var pendingRows = connection.Query<ProgpasPendingRow>(
                        @"SELECT CODA, CODB, ISNULL(MEGHK, 0) AS MEGHK
                          FROM dbo.PROGPAS1
                          WHERE PRGID = @PRGID AND ISNULL(PASED, 0) = 0;",
                        new { PRGID = prgId },
                        transaction).ToList();

                    if (!pendingRows.Any())
                    {
                        break;
                    }

                    foreach (var row in pendingRows)
                    {
                        double multiplier = row.MEGHK;
                        double? frNumber = null;

                        if (multiplier != 0)
                        {
                            var candidate = CL_HESABDARI.GETLASTFR(row.CODB, persianDate);
                            if (candidate != 0)
                            {
                                frNumber = candidate;

                                connection.Execute(
                                    @"INSERT INTO dbo.PROGPAS1 (PRGID, CODA, CODB, VAHED, MEGH, MEGHK, PERT, KOLMAV, MABL, MABLK, RADAH, PASED)
                                      SELECT  @PRGID,
                                              h.CODE,
                                              d.CODE,
                                              d.VAHED_K,
                                              d.MEGH * @Multiplier,
                                              d.MEGHk * @Multiplier,
                                              d.PERT * @Multiplier,
                                              (d.MEGHk + d.PERT) * @Multiplier,
                                              d.SMABL,
                                              d.MABLK * @Multiplier,
                                              s.RADAH,
                                              0
                                      FROM dbo.STUF_DEF AS s
                                      INNER JOIN dbo.DTL_MANF AS d ON s.CODE = d.CODE
                                      INNER JOIN dbo.HEAD_MANF AS h ON h.FNUMB = d.FNUMB
                                      WHERE h.FNUMB = @FR;",
                                    new
                                    {
                                        PRGID = prgId,
                                        Multiplier = multiplier,
                                        FR = candidate
                                    },
                                    transaction);
                            }
                        }
                        connection.Execute(
                            @"UPDATE TOP (1) dbo.PROGPAS1
                              SET PASED = 1, FR = @FR
                              WHERE PRGID = @PRGID AND CODA = @CODA AND CODB = @CODB AND ISNULL(PASED, 0) = 0;",
                            new
                            {
                                PRGID = prgId,
                                row.CODA,
                                row.CODB,
                                FR = frNumber
                            },
                            transaction);
                    }
                }

                var costRows = connection.Query<ProgpasCostRow>(
                    @"SELECT CODA, CODB, ISNULL(MEGHK, 0) AS MEGHK
                      FROM dbo.PROGPAS1
                      WHERE PRGID = @PRGID;",
                    new { PRGID = prgId },
                    transaction).ToList();

                foreach (var row in costRows)
                {
                    double unitPrice = GetLastPriceNz(connection, transaction, row.CODB, persianDate);
                    double totalPrice = unitPrice * row.MEGHK; ////, 2, MidpointRounding.AwayFromZero
                    connection.Execute(
                        @"UPDATE TOP (1) dbo.PROGPAS1
                          SET MABL = @MABL, MABLK = @MABLK
                          WHERE PRGID = @PRGID AND CODA = @CODA AND CODB = @CODB;",
                        new
                        {
                            PRGID = prgId,
                            row.CODA,
                            row.CODB,
                            MABL = unitPrice,
                            MABLK = totalPrice
                        },
                        transaction);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                ProcLoader.Stop(Prc);
                transaction?.Rollback();
                connection.Close();
                new Msgwin(false, $"خطا در محاسبه قیمت تمام شده", "#FFFF0000", true).ShowDialog();
                return;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection?.Close();
                }
            }

            var summary = BuildCostSummary(prgId, persianDate);

            //if (!string.IsNullOrWhiteSpace(summary.SummaryText))
            //{
            //    new Msgwin(false, summary.SummaryText, "", true).ShowDialog();
            //}

            ProcLoader.Stop(Prc);

            OpenHeadManfDayWindow(prgId, summary);
        }
        private static double GetLastPriceNz(SqlConnection connection, SqlTransaction transaction, string code, long date)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return 0d;
            }
            var price = CL_HESABDARI.LASTPRICENZ(code, date);
            return price;
        }
        private CostSummary BuildCostSummary(long prgId, long persianDate)
        {
            var builder = new StringBuilder();

            builder.AppendLine("----------------------------------------------------------------------");
            builder.Append("فرمول ساخت                                     ");
            builder.AppendLine($"  {CODE.SelectedValue?.ToString()}");
            builder.AppendLine("----------------------------------------------------------------------");

            var detailRows = dbms.DoGetDataSQL<ProgpasSummaryRow>(
                @"SELECT     q.CODB,
                           q.VAHED,
                           q.SumOfMEGH AS MEGH,
                           q.SumOfMEGHK AS MEGHK,
                           q.SumOfPERT AS PERT,
                           q.SumOfKOLMAV AS KOLMAV,
                           q.RADAH,
                           s.NAME
                  FROM dbo.QPROGPAS2 AS q
                  INNER JOIN dbo.STUF_DEF AS s ON q.CODB = s.CODE
                  WHERE q.PRGID = @PRGID
                  ORDER BY q.RADAH, q.CODB;",
                new { PRGID = prgId }).ToList();

            double totalMaterial = 0d;

            foreach (var row in detailRows)
            {
                double unitPrice = CL_HESABDARI.LASTPRICENZ(row.CODB, persianDate);

                //double totalLine = Math.Round((row.MEGHK ?? 0d) * unitPrice);
                double totalLine = (row.MEGHK ?? 0d) * unitPrice;

                string codePart = (row.CODB ?? string.Empty).PadLeft(15);
                string name = row.NAME ?? string.Empty;
                if (name.Length > 70)
                {
                    name = name[..70];
                }
                string namePart = name.PadRight(70);
                string qtyPart = (row.MEGHK ?? 0d).ToString("N4", CultureInfo.CurrentCulture).PadLeft(12);
                string unitPricePart = unitPrice.ToString("N0", CultureInfo.CurrentCulture).PadLeft(12);
                string totalPart = totalLine.ToString("N0", CultureInfo.CurrentCulture).PadLeft(12);

                builder.AppendLine($"{codePart}  {namePart}      {qtyPart}       {unitPricePart}      {totalPart}");

                totalMaterial += totalLine;
            }

            var laborInfo = dbms.DoGetDataSQL<ProgpasLaborRow>(
                @"SELECT SUM(h.IMBIBE_MANF * p.MEGHK) AS MANF,
                         SUM(h.IMBIBE_SAR * p.MEGHK) AS SAR
                  FROM dbo.PROGPAS1 AS p
                  INNER JOIN dbo.HEAD_MANF AS h ON p.FR = h.FNUMB
                  WHERE p.PRGID = @PRGID AND ISNULL(p.RADAH, 0) <> 1;",
                new { PRGID = prgId }).FirstOrDefault();

            double baseLabor = Convert.ToDouble(IMBIBE_MANF.Text);
            double baseOverhead = Convert.ToDouble(IMBIBE_SAR.Text);

            double nestedLabor = laborInfo?.MANF ?? 0d;
            double nestedOverhead = laborInfo?.SAR ?? 0d;

            bool hasNested = laborInfo is not null && (laborInfo.MANF.HasValue || laborInfo.SAR.HasValue);

            double totalLabor = hasNested ? baseLabor + nestedLabor : baseLabor;
            double totalOverhead = hasNested ? baseOverhead + nestedOverhead : baseOverhead;

            double roundedMaterial = Math.Round(totalMaterial, 0);
            double roundedLabor = Math.Round(totalLabor, 0);
            double roundedOverhead = Math.Round(totalOverhead, 0);
            double totalCost = roundedMaterial + roundedLabor + roundedOverhead;

            builder.AppendLine("----------------------------------------------------------------------");
            builder.AppendLine($"جمع مواد : {roundedMaterial:N0}");
            builder.AppendLine($"دستمزد  : {roundedLabor:N0}");
            builder.AppendLine($"سربار   : {roundedOverhead:N0}");
            builder.AppendLine("----------------------------------------------------------------------");
            builder.AppendLine($"قيمت تمام شده   : {totalCost:N0}");
            builder.AppendLine();
            builder.AppendLine();

            return new CostSummary(roundedMaterial, roundedLabor, roundedOverhead, builder.ToString());
        }
        private void OpenHeadManfDayWindow(long prgId, CostSummary summary)
        {
            try
            {
                var MAVADR = summary.TotalMaterial; //جمع مواد
                var DASTR = summary.TotalLabor; //جمع دستمزد
                var SARR = summary.TotalOverhead; //جمع سربار

                var window = new WIN_HEAD_MANF_DAY(prgId, MAVADR, DASTR, SARR);
                window.Show();
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در نمایش ریز اطلاعات", "#FFFF0000", true).ShowDialog();
            }
        }
        private sealed record CostSummary(double TotalMaterial, double TotalLabor, double TotalOverhead, string SummaryText);
        private sealed class ProgpasPendingRow
        {
            public string CODA { get; set; } = string.Empty;
            public string CODB { get; set; } = string.Empty;
            public double MEGHK { get; set; }
        }
        private sealed class ProgpasCostRow
        {
            public string CODA { get; set; } = string.Empty;
            public string CODB { get; set; } = string.Empty;
            public double MEGHK { get; set; }
        }
        private sealed class ProgpasSummaryRow
        {
            public string CODB { get; set; } = string.Empty;
            public string? NAME { get; set; }
            public double? MEGHK { get; set; }
        }
        private sealed class ProgpasLaborRow
        {
            public double? MANF { get; set; }
            public double? SAR { get; set; }
        }
        #endregion

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //چاپ فرمول 2
            if (_navigationManager.IsNewRecord) { return; }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.SANATI.PRINT_FORMULA_2.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["FNUMB_PARAM"] = FNUMB.Text;

            (report.GetComponentByName("IMBIBE_MANF") as StiText).Text = IMBIBE_MANF.Text; //جذب هزینه دستمزد
            (report.GetComponentByName("IMBIBE_SAR") as StiText).Text = IMBIBE_SAR.Text; //جذب هزینه سربار

            (report.GetComponentByName("Text2") as StiText).Text = Baseknow.WIDTH_D; //نام شرکت
            (report.GetComponentByName("smb") as StiText).Text = SUM_OF_MEGH_K.ToString(); //جمع مواد مصرفی

            //Text228: قیمت تمام شده

            new WINRPT(report, "چاپ 2 فرمول ساخت").Show();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //چاپ فرمول
            if (_navigationManager.IsNewRecord) { return; }
            //#,##0.######################

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.SANATI.PRINT_FORMULA.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["FNUMB_PARAM"] = FNUMB.Text;
            (report.GetComponentByName("Text2") as StiText).Text = Baseknow.WIDTH_D; //نام شرکت

            new WINRPT(report, "چاپ فرمول ساخت").Show();
        }
        private void ReCalcAfter()
        {
            // e.g SA_HOUR_AfterUpdate
            IMBIBE_MANF.Text = Math.Round(Convert.ToDouble(SA_HOUR.Text ?? "0") / 3600 * Convert.ToDouble(SA_NHOU.Text ?? "0")).ToString();
        }
        private void SA_HOUR_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            ReCalcAfter();
        }
        private void DG_SUB_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid? dg = sender as DataGrid;
            if (dg == null) return;

            // اطمینان از فوکوس بودن DataGrid
            if (!dg.IsKeyboardFocusWithin)
            {
                dg.Focus();
            }

            if (dg.CurrentItem == null || dg.CurrentItem == CollectionView.NewItemPlaceholder)
            {
                e.Handled = true; // Cancel opening the menu. Avoids the crash.
                return;
            }
            if (dg?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            else if (dg?.ContextMenu == null)
            {
                e.Handled = true;
                return;
            }

            base.OnContextMenuOpening(e);
        }
        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            // پیدا کردن سطری که روی آن کلیک شده
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);

            if (row != null)
            {
                // انتخاب سطر
                if (!row.IsSelected)
                {
                    row.IsSelected = true;
                    dataGrid.SelectedItem = row.Item;
                }

                // تنظیم فوکوس روی سطر و DataGrid
                row.Focus();

                // اطمینان از اینکه DataGrid خودش هم فوکوس دارد
                dataGrid.Focus();

                // اگر سلول خاصی زیر موس است، آن را هم فوکوس کنیم
                var cell = FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
                if (cell != null)
                {
                    cell.Focus();
                }
            }
            else
            {
                // اگر روی header یا جای دیگری کلیک شد، حداقل DataGrid را فوکوس کنیم
                dataGrid.Focus();
            }
        }



        private void SA_NHOU_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            ReCalcAfter();
        }
    }
}
