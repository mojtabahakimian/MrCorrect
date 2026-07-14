using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Prg_Proccessy.MODELS;
using System.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using System.Windows.Media;
using System.Text;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Functions;
using System.Threading.Tasks;
using System.Threading;
using Wins.WinOther;
using System.ComponentModel;
using System.Windows.Data;
using static Functions.DataGridClipboardManager;
using System.Windows.Controls.Primitives;

namespace Wins.WinSetting
{
    public partial class F_USER_PERMITION_FORMS : Window
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
        public class COMBOYMODELBYTE
        {
            public string NAME { get; set; }
            public byte ID { get; set; }
        }
        public class ABNAR_KEY_MODEL
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
            public string? ANB_KIND { get; set; }
        }
        public class PERMIS_MODEL
        {
            public string? CAPTION { get; set; }
            public int? OBJECT { get; set; }
            public bool? RUN { get; set; }
            public bool? SEE { get; set; }
            public bool? INP { get; set; }
            public bool? UPD { get; set; }
            public bool? DEL { get; set; }
            public int? USERCO { get; set; }
            public short? GRP { get; set; }
            public string? SAL_NAME { get; set; }
        }
        #endregion
        public F_USER_PERMITION_FORMS(string? openargs = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(openargs))
            {
                OpenArgs = openargs;
            }

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public ObservableCollection<SALS_PERMIS> PERMISIONS_DATA { get; set; } = new ObservableCollection<SALS_PERMIS>();
        public ObservableCollection<SALA_DTL> ALL_USERS { get; set; } = new ObservableCollection<SALA_DTL>();

        public ObservableCollection<SALA_DTL> SECOND_ALL_USERS { get; set; } = new ObservableCollection<SALA_DTL>(); //مخصوص بقیه کاربری ها

        public ObservableCollection<OPANBACCESS> OPANBACCESS_DATA { get; set; } = new ObservableCollection<OPANBACCESS>(); //کلید انبار ها

        public ObservableCollection<BLOCK_CUSTOMER> BLOCK_CUSTOMER_DATA { get; set; } = new ObservableCollection<BLOCK_CUSTOMER>();

        public ObservableCollection<SALGROUP_MODEL> SALGROUP_DATA { get; set; } = new ObservableCollection<SALGROUP_MODEL>(); // گروه کاربری

        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();

        private List<SALS_PERMIS> initialPermissionsState;
        private List<SALA_DTL> initialUsersState;
        private bool initialRunState;
        private bool initialReadState;
        private bool initialInsertState;
        private bool initialUpdateState;
        private bool initialDeleteState;

        public bool NowIsReady { get; private set; }
        public bool NewRecord { get; set; }
        public bool ChangeIsHappend { get; private set; }

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

                //TextBox.IsReadOnly = !ican;

                //ComboBox.IsEnabled = ican;
            }
        }

        public string OpenArgs { get; }
        public bool AnbarKeyTabItem_IsFocused { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (OPANBACCESS_SUB_IsFocused)
                {
                    if (false /*OPANBACCESS_SUB.CurrentColumn != null*/)
                    {
                        try
                        {
                            int currentColumnIndex = OPANBACCESS_SUB.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == OPANBACCESS_SUB.Columns.Count - 1;
                            bool isLastRow = OPANBACCESS_SUB.SelectedIndex == OPANBACCESS_SUB.Items.Count - 2; //Last Row that is new Empty
                            if (isLastColumn)
                            {
                                if (isLastRow)
                                {
                                    OPANBACCESS_SUB.SelectedIndex++;
                                    OPANBACCESS_SUB.CurrentCell = new DataGridCellInfo(OPANBACCESS_SUB.SelectedItem, OPANBACCESS_SUB.Columns[OPANBACCESS_SUB_DEF_INDEX_COL]);
                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        //OPANBACCESS_SUB.BeginEdit();
                                    }), DispatcherPriority.Background);
                                    return;
                                }
                            }
                        }
                        catch { /*ignore*/ }
                    }

                    return;
                }
                else if (BLOCKED_SUB_IsFocused)
                {
                    return;
                }
                else if (UNBLOCKED_SUB_IsFocused)
                {
                    return;
                }
                else if (CHARTSAZMANI_SUB_IsFocused)
                {
                    return;
                }

                if (BLOCK_CUSTOMER_SUB.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    var DG = BLOCK_CUSTOMER_SUB;

                    if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        if (DG?.CurrentColumn != null && DG?.SelectedItem != null && DG.CanUserAddRows && DG.IsEnabled && !DG.IsReadOnly)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = DG.CurrentColumn.SortMemberPath == "EUSER";
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[BLOCK_CUSTOMER_SUB_INDEX_DEF_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        if (DG.SelectedItem != null)
                                        {
                                            DG.BeginEdit();
                                        }
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    else
                    {
                        if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                        {
                            DataGridExtension.HandleKeyPress(sender, e, DG);
                        }
                    }

                }

                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            I_AM_SATEHDAST = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "F_USER_PERMITION FORMS", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "permi", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            #region Form_Open
            //Form_Open
            //if (OpenArgs == "1")
            //{
            //    this.Page52.Visible = false;
            //    this.Page53.Visible = false;
            //    this.Page58.Visible = false;
            //    this.Page65.Visible = false;
            //    this.Page69.Visible = false;
            //    this.Page83.Visible = false;
            //    this.page93.Visible = false;
            //}
            //else if (OpenArgs == "BLACK")
            //{
            //    this.Page52.Visible = false;
            //    this.Page53.Visible = false;
            //    this.Page58.Visible = false;
            //    this.Page65.Visible = false;
            //    this.Page73.Visible = false;
            //    this.Page83.Visible = false;
            //    this.page93.Visible = false;
            //}
            #endregion

            FILL_ALL_COMBOBOXES();

            //---------------------------------------------دسترسی ها
            LoadUsersData(); //کاربران
            LoadPermissionsData(); //دسترسی ها

            StoreInitialState(); //ذخیره حالت اولیه

            //---------------------------------------------کلید انبار ها


            navigationControl.ReGetDataAction = () => //Realod Data
            {
                BLOCK_CUSTOMER_ReGetData();
            };
            navigationControl.NewRecordAction = () => //Go Focus New Row Cell
            {
                navigationControl.GoNewPlaceholder(BLOCK_CUSTOMER_SUB, NAME_HES_COLUMN);
            };

            //گروه کاربری
            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                USER_ENDN_ReGetData();
            };
            //GR_NAV_DATAGRID.NewRecordAction = () => //Go Focus New Row Cell
            //{
            //    navigationControl.GoNewPlaceholder(USER_ENDN_SUB, SAL_NAME_COLUMN);
            //};
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //انبار ها
            ANBAR_COLUMN.ItemsSource = dbms.DoGetDataSQL<ABNAR_KEY_MODEL>($@"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, TCOD_ANBAR_KIND.ANB_KIND FROM dbo.TCOD_ANBAR INNER JOIN dbo.TCOD_ANBAR_KIND ON TCOD_ANBAR.KIND = TCOD_ANBAR_KIND.CODE").ToList();

            //var RST = dbms.DoGetDataSQL<ABNAR_KEY_MODEL>($@"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, TCOD_ANBAR_KIND.ANB_KIND FROM dbo.TCOD_ANBAR INNER JOIN dbo.TCOD_ANBAR_KIND ON TCOD_ANBAR.KIND = TCOD_ANBAR_KIND.CODE").ToList();

            //foreach (var item in RST)
            //{
            //    ANBARS_DATA.Add(item);
            //}

            //گروه کاربری
            GRSAL_COLUMN.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "مديران" },
                new COMBOYMODEL { ID = 2, NAME = "حسابداران" },
                new COMBOYMODEL { ID = 3, NAME = "بازرگاني" },
                new COMBOYMODEL { ID = 4, NAME = "انبار" },
                new COMBOYMODEL { ID = 5, NAME = "كارگزيني" },
                new COMBOYMODEL { ID = 6, NAME = "هتل" },
                new COMBOYMODEL { ID = 7, NAME = "تالار" },
                new COMBOYMODEL { ID = 8, NAME = "ريموت" },
                new COMBOYMODEL { ID = 9, NAME = "آفلاين" },
                new COMBOYMODEL { ID = 11, NAME = "تبلت " }
            };

            //وضعیت
            ENABL_COLUMN.ItemsSource = new List<COMBOYMODELBYTE>
            {
                new COMBOYMODELBYTE { ID = 0, NAME = "فعال" },
                new COMBOYMODELBYTE { ID = 1, NAME = "غیر فعال" },
            };

            //منوی پیش فرض
            menup_COLUMN.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "استاندارد" },
                new COMBOYMODEL { ID = 1, NAME = "تصویری" },
                new COMBOYMODEL { ID = 2, NAME = "پنلی" },
            };

            //پیش فرض الگوی پرداخت پورسانت
            PORID_COLUMN.ItemsSource = dbms.DoGetDataSQL<DEFAULT_PURSANT_MODEL>($@"SELECT VISITORS_PORSANT.PORID, CAST(VISITORS_PORSANT.PORID AS NVARCHAR)+N' - '+CAST(VISITORS_PORSANT.VDATE AS NVARCHAR)+N' - '+ISNULL(CUSTKIND.CUSTKNAME, N'بدون گروه (همه)')+N' - '+ISNULL(VISITORS_PORSANT.COMMENT, N' ')+N' - '+CUST_HESAB.NAME AS Expr1
                                                              FROM VISITORS_PORSANT
                                                                   INNER JOIN CUST_HESAB ON VISITORS_PORSANT.HES=CUST_HESAB.hes
                                                                   LEFT OUTER JOIN CUSTKIND ON VISITORS_PORSANT.CUST_COD=CUSTKIND.CUST_COD;").ToList();

            //پیش فرض نحوه پرداخت
            DEFAULT_NAHVA_COLUMN.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();

            // پیش فرض واحد و شیفت
            //VAHEDJARI
            DEFAULT_TFSAZMAN_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            DEFAULT_TFSAZMAN_COLUMN.SelectedValuePath = "DEPATMAN";
            DEFAULT_TFSAZMAN_COLUMN.DisplayMemberPath = "DEPNAME";

            //SHIFT
            DEFAULT_SHIFT_COLUMN.ItemsSource = dbms.DoGetDataSQL<SHIFT>("SELECT SHIFT_ID,SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            DEFAULT_SHIFT_COLUMN.SelectedValuePath = "SHIFT_ID";
            DEFAULT_SHIFT_COLUMN.DisplayMemberPath = "SHNAME";

        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (MLT.IsChecked ?? false) //انتخاب تکی
            {
                Msgwin msgwin = new Msgwin(true, "آیا از ذخیره مطمئن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }
            }

            SavePermissions();

            ChangeIsHappend = false;
        }
        private void UpdateDataGridFocus(DataGrid DG, string DEFAULT_COLUMN)
        {
            if (DG == null || DG.Items.Count <= 0 || DG.SelectedIndex < 0)
            {
                return;
            }

            //int lastIndex = DG.Items.Count - 1;
            //int targetRowIndex = Math.Max(0, Math.Min(lastIndex, DG.SelectedIndex));

            var targetRowIndex = Math.Max(0, DG.SelectedIndex);

            CL_LMethods.FocusCellReadyToEdit(DG, DEFAULT_COLUMN, targetRowIndex, false);
        }
        private string? GetNameHes(string? hesy)
        {
            if (string.IsNullOrEmpty(hesy))
            {
                return null;
            }

            try
            {
                return dbms.DoGetDataSQL<string?>($"SELECT TOP 1 NAME FROM dbo.CUST_HESAB WHERE hes = N'{hesy}'").FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }
        private void LoadPermissionsData()
        {
            PERMISIONS_DATA?.Clear();
            //دسترسی ها
            var RST_PERSMITION = dbms.DoGetDataSQL<SALS_PERMIS>(@$"SELECT dbo.SAL_CHEK.UID, dbo.SAL_CHEK.CRT, dbo.SAL_CHEK.DEL, dbo.SAL_CHEK.UPD, dbo.SAL_CHEK.INP, 
                                                                   dbo.SAL_CHEK.SEE, dbo.SAL_CHEK.RUN, dbo.SAL_CHEK.OBJECT, dbo.SAL_CHEK.USERCO, dbo.TFORMS.CAPTION,
                                                                   dbo.TFORMS.IDH, dbo.TFORMS.GRP, dbo.TFORMS.kind, dbo.TFORMS.FORMNAME
                                                                   FROM dbo.TFORMS
                                                                        INNER JOIN dbo.SAL_CHEK ON dbo.TFORMS.IDH=dbo.SAL_CHEK.OBJECT").ToList();

            foreach (var item in RST_PERSMITION)
            {
                if (!string.IsNullOrEmpty(item.CAPTION))
                {
                    item.CAPTION = item.CAPTION.FixPersianChars();
                }
                item.IsModified = false;
                PERMISIONS_DATA.Add(item);
            }
        }
        private void LoadUsersData()
        {
            ALL_USERS?.Clear();
            //کاربران
            var RST_PERSONEL = dbms.DoGetDataSQL<SALA_DTL>("SELECT SAL_NAME, GRSAL, ENABL, IDD, HES, PORID, menup, CRT, UID, erjabe FROM dbo.SALA_DTL WHERE (ENABL=0) ORDER BY IDD").ToList();
            foreach (var rows in RST_PERSONEL)
            {
                if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }
            foreach (var item in RST_PERSONEL)
            {
                ALL_USERS.Add(item);
            }

            //کمبوباکس کاربران در تب زیرمجموعه
            var item0 = new SALA_DTL { SAL_NAME = "همه", IDD = 0 };
            RST_PERSONEL.Add(item0.Clone() as SALA_DTL);
            COLUMN_SUBUSER.ItemsSource = RST_PERSONEL;


            SECOND_ALL_USERS?.Clear();
            foreach (var item in RST_PERSONEL)
            {
                if (item.IDD != 0)
                {
                    SECOND_ALL_USERS.Add(item.Clone() as SALA_DTL);
                }
            }

            //ارجاع پیش فاکتور به این کاربر
            erjabe_COLUMN.ItemsSource = RST_PERSONEL;
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems)
            {
                if (added is TabItem addedTab)
                {
                    if (addedTab.Tag != null)
                    {
                        if (addedTab.Tag.ToStringNullSafe().Equals("BlackList"))
                        {
                            BLOCK_CUSTOMER_ReGetData();
                        }
                        if (addedTab.Tag.ToStringNullSafe().Equals("GroupUsers"))
                        {
                            USER_ENDN_ReGetData();
                        }

                        if (NowIsReady)
                        {

                        }
                    }
                }
            }
        }

        #region PERMISSIONS
        private void srch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (!PermissionListBox.IsEnabled) { return; }

            SearchFilterPermissions();
        }
        private void serus_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            string searchText = serus.Text;
            var filteredUsers = FilterByFuzzyText(ALL_USERS, u => u.SAL_NAME, searchText)
                .DistinctBy(p => p.SAL_NAME)
                .ToList();
            // Assuming you have a ListBox for users named UserListBox
            UserListBox.ItemsSource = filteredUsers;
        }
        private void SearchFilterPermissions()
        {
            string searchText = srch.Text;

            var LastUserSelected = GetLastSelectedItemSafely<SALA_DTL>(UserListBox);

            if (LastUserSelected == null)
            {
                return;
            }

            var filteredPermissions = FilterByFuzzyText(
                    PERMISIONS_DATA.Where(p => p.CAPTION != null && p.USERCO == LastUserSelected.IDD),
                    p => p.CAPTION,
                    searchText)
                .DistinctBy(p => p.CAPTION)
                .ToList();

            PermissionListBox.ItemsSource = filteredPermissions;
        }

        private static IEnumerable<T> FilterByFuzzyText<T>(IEnumerable<T> source, Func<T, string?> textSelector, string searchText)
        {
            var normalizedSearchText = NormalizeSearchText(searchText);
            if (string.IsNullOrEmpty(normalizedSearchText))
            {
                return source;
            }

            return source
                .Select(item => new
                {
                    Item = item,
                    NormalizedText = NormalizeSearchText(textSelector(item))
                })
                .Where(x => x.NormalizedText.Contains(normalizedSearchText) || IsFuzzyMatch(x.NormalizedText, normalizedSearchText))
                .OrderBy(x => GetFuzzySearchRank(x.NormalizedText, normalizedSearchText))
                .Select(x => x.Item);
        }

        private static int GetFuzzySearchRank(string normalizedText, string normalizedSearchText)
        {
            if (normalizedText.Contains(normalizedSearchText)) return 0;

            return GetBestTokenWindowDistance(normalizedText, normalizedSearchText);
        }

        private static string NormalizeSearchText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedBuilder = new StringBuilder(text.Length);
            var previousWasWhiteSpace = true;

            foreach (var currentChar in text.Trim().ToLowerInvariant())
            {
                var normalizedChar = NormalizePersianChar(currentChar);
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(normalizedChar);

                if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(normalizedChar))
                {
                    normalizedBuilder.Append(normalizedChar);
                    previousWasWhiteSpace = false;
                    continue;
                }

                if (!previousWasWhiteSpace)
                {
                    normalizedBuilder.Append(' ');
                    previousWasWhiteSpace = true;
                }
            }

            return normalizedBuilder.ToString().Trim();
        }

        private static char NormalizePersianChar(char currentChar)
        {
            switch (currentChar)
            {
                case 'ي':
                case 'ى':
                    return 'ی';
                case 'ك':
                    return 'ک';
                case 'أ':
                case 'إ':
                case 'آ':
                    return 'ا';
                case 'ة':
                    return 'ه';
                default:
                    return currentChar;
            }
        }

        private static bool IsFuzzyMatch(string normalizedText, string normalizedSearchText)
        {
            if (string.IsNullOrEmpty(normalizedText) || string.IsNullOrEmpty(normalizedSearchText)) return false;

            var distance = GetBestTokenWindowDistance(normalizedText, normalizedSearchText);
            return distance <= GetAllowedFuzzyDistance(normalizedSearchText.Length);
        }

        private static int GetBestTokenWindowDistance(string normalizedText, string normalizedSearchText)
        {
            var textWords = normalizedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var searchWords = normalizedSearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (textWords.Length == 0 || searchWords.Length == 0) return int.MaxValue;

            var windowSize = Math.Min(searchWords.Length, textWords.Length);
            var bestDistance = int.MaxValue;

            for (int i = 0; i <= textWords.Length - windowSize; i++)
            {
                var candidate = string.Join(" ", textWords.Skip(i).Take(windowSize));
                bestDistance = Math.Min(bestDistance, CalculateDamerauLevenshteinDistance(candidate, normalizedSearchText));
            }

            bestDistance = Math.Min(bestDistance, CalculateDamerauLevenshteinDistance(normalizedText, normalizedSearchText));
            return bestDistance;
        }

        private static int GetAllowedFuzzyDistance(int searchTextLength)
        {
            if (searchTextLength <= 4) return 1;
            if (searchTextLength <= 12) return 2;

            return Math.Max(2, searchTextLength / 5);
        }

        private static int CalculateDamerauLevenshteinDistance(string source, string target)
        {
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            var distances = new int[source.Length + 1, target.Length + 1];
            for (int i = 0; i <= source.Length; i++) distances[i, 0] = i;
            for (int j = 0; j <= target.Length; j++) distances[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    var deletionDistance = distances[i - 1, j] + 1;
                    var insertionDistance = distances[i, j - 1] + 1;
                    var substitutionDistance = distances[i - 1, j - 1] + cost;

                    distances[i, j] = Math.Min(Math.Min(deletionDistance, insertionDistance), substitutionDistance);

                    if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                    {
                        distances[i, j] = Math.Min(distances[i, j], distances[i - 2, j - 2] + 1);
                    }
                }
            }

            return distances[source.Length, target.Length];
        }

        private void BTN_MAKE_ACTIVE_ALL_Click(object sender, RoutedEventArgs e)
        {
            SALA_DTL? CurrentUser = GetLastSelectedItemSafely<SALA_DTL>(UserListBox);
            if (CurrentUser != null && CurrentUser?.SAL_NAME != null)
            {
                Msgwin msgwin = new Msgwin(true, $@"از باز کردن همه دسترسی ها برای فقط این کاربر [{CurrentUser.SAL_NAME}] اطمینان دارید ؟ ");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    dbms.DoExecuteSQL(@$"UPDATE dbo.SAL_CHEK SET RUN = 1, SEE = 1, INP = 1, UPD = 1, DEL = 1 WHERE USERCO = {CurrentUser.IDD}");

                    PermissionListBox.ItemsSource = null;
                    LoadPermissionsData();

                    UserListBox.SelectedItem = null;
                    UserListBox.SelectedItem = CurrentUser; UserListBox.Items.Refresh();
                    PermissionListBox.Items.Refresh();
                }
                _ = new Msgwin(false, $@"کلیه تیک ها برای این کاربر [{CurrentUser.SAL_NAME}] فعال و ذخیره شد. ").ShowDialog();
            }
        }
        private void BTN_MAKE_DEACTIVE_ALL_Click(object sender, RoutedEventArgs e)
        {
            SALA_DTL? CurrentUser = GetLastSelectedItemSafely<SALA_DTL>(UserListBox);
            if (CurrentUser != null && CurrentUser?.SAL_NAME != null)
            {
                Msgwin msgwin = new Msgwin(true, $@"از بستن همه دسترسی ها برای فقط این کاربر [{CurrentUser.SAL_NAME}] اطمینان دارید ؟ ");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    dbms.DoExecuteSQL(@$"UPDATE dbo.SAL_CHEK SET RUN = 0, SEE = 0, INP = 0, UPD = 0, DEL = 0 WHERE USERCO = {CurrentUser.IDD}");

                    PermissionListBox.ItemsSource = null;
                    LoadPermissionsData();

                    UserListBox.SelectedItem = null;
                    UserListBox.SelectedItem = CurrentUser; UserListBox.Items.Refresh();
                    PermissionListBox.Items.Refresh();
                }
                _ = new Msgwin(false, $@"کلیه تیک ها برای این کاربر [{CurrentUser.SAL_NAME}] غــیر فعال و ذخیره شد. ").ShowDialog();
            }
        }
        private void SavePermissions()
        {
            var selectedUsers = ALL_USERS.Where(u => u.IsSelected).ToList();
            List<SALS_PERMIS> selectedPermissions;
            if (SS.IsChecked ?? false)
            {
                selectedPermissions = PERMISIONS_DATA.Where(p => p.IsSelected || p.IsModified).ToList();
            }
            else
            {
                selectedPermissions = PERMISIONS_DATA.Where(p => p.IsSelected).ToList();
            }

            bool Happenned = false;
            int PSC = 0;
            List<MsgModel> InfoMessages = new List<MsgModel>();

            var SingleUserApplyedName = "";

            foreach (var user in selectedUsers)
            {
                foreach (var permission in selectedPermissions)
                {
                    if (SS.IsChecked ?? false) //انتخاب تکی
                    {
                        if (permission.USERCO == user.IDD)
                        {
                            PSC++;
                            Happenned = true;
                            SingleUserApplyedName = user.SAL_NAME;

                            dbms.DoExecuteSQL(@$"UPDATE dbo.SAL_CHEK
                            SET RUN = {Convert.ToByte(permission.RUN)}, SEE = {Convert.ToByte(permission.SEE)}, 
                            INP = {Convert.ToByte(permission.INP)}, UPD = {Convert.ToByte(permission.UPD)}, DEL = {Convert.ToByte(permission.DEL)}
                            WHERE USERCO = {user.IDD} AND OBJECT = {permission.OBJECT}");

                            permission.IsSelected = false; //reset selection
                            permission.IsModified = false;
                        }
                    }
                    else
                    {
                        Happenned = true;

                        dbms.DoExecuteSQL(@$"UPDATE dbo.SAL_CHEK
                            SET RUN = {Convert.ToByte(permission.RUN)}, SEE = {Convert.ToByte(permission.SEE)}, 
                            INP = {Convert.ToByte(permission.INP)}, UPD = {Convert.ToByte(permission.UPD)}, DEL = {Convert.ToByte(permission.DEL)}
                            WHERE USERCO = {user.IDD} AND OBJECT = {permission.OBJECT}");

                        permission.IsSelected = false; //reset selection
                        permission.IsModified = false;
                    }

                    _ = AuditLogger.LogActionAsync(
                        actionType: "SaveRow",
                        tableName: "تعیین سطح دسترسی : دسترسی کاربران",
                        recordId: Baseknow.UUSER,
                        oldValue: default,
                        newValue: permission,
                        additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");
                }

                if (Happenned)
                {
                    if (SS.IsChecked ?? false) //انتخاب تکی
                    {
                    }
                    else
                    {
                        InfoMessages.Add(new MsgModel { MessageText_U = $"دسترسی های کاربر '{user.SAL_NAME}' بروز شد." });
                    }
                }
            }

            if (Happenned)
            {
                if (SS.IsChecked ?? false)
                {
                    Pop1Text1.FlowDirection = FlowDirection.RightToLeft;
                    universControl.PopNotifyShow($"{PSC} مورد از دسترسی های کاربر '{SingleUserApplyedName}' بروز شد. ", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
                else
                {
                    if (InfoMessages.Count > 0)
                    {
                        new MsgListwin(false, InfoMessages.Distinct().ToList()).ShowDialog();
                        InfoMessages?.Clear();
                    }

                    // Reset IsSelected for all permissions
                    foreach (var p in PERMISIONS_DATA) { p.IsSelected = false; p.IsModified = false; }
                    // Reset IsSelected for all users
                    foreach (var p in ALL_USERS) { p.IsSelected = false; }

                    universControl.PopNotifyShow("ذخیره دسترسی ها انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            else
            {
                universControl.PopNotifyShowUp("تغییری انجام نشد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue);
            }
        }
        private void StoreInitialState()
        {
            initialPermissionsState = PERMISIONS_DATA.Select(p => p.Clone() as SALS_PERMIS).ToList();
            initialUsersState = ALL_USERS.Select(u => u.Clone() as SALA_DTL).ToList();
            initialRunState = runn.IsChecked ?? false;
            initialReadState = read.IsChecked ?? false;
            initialInsertState = insert.IsChecked ?? false;
            initialUpdateState = update.IsChecked ?? false;
            initialDeleteState = delete.IsChecked ?? false;
        }
        private void RestoreInitialState()
        {
            // Restore permissions
            foreach (var permission in PERMISIONS_DATA)
            {
                var initialPermission = initialPermissionsState.FirstOrDefault(p => p.IDH == permission.IDH);
                if (initialPermission != null)
                {
                    permission.IsSelected = initialPermission.IsSelected;
                }
            }

            // Restore users
            foreach (var user in ALL_USERS)
            {
                var initialUser = initialUsersState.FirstOrDefault(u => u.IDD == user.IDD);
                if (initialUser != null)
                {
                    user.IsSelected = initialUser.IsSelected;
                }
            }

            // Reset IsModified on restored permissions
            foreach (var permission in PERMISIONS_DATA)
            {
                permission.IsModified = false;
            }

            // Restore checkbox states
            runn.IsChecked = initialRunState;
            read.IsChecked = initialReadState;
            insert.IsChecked = initialInsertState;
            update.IsChecked = initialUpdateState;
            delete.IsChecked = initialDeleteState;

            // Refresh ListBoxes
            PermissionListBox.Items.Refresh();
            UserListBox.Items.Refresh();
        }
        private void BTN_UNDO_Click(object sender, RoutedEventArgs e)
        {
            RestoreInitialState();
            new Msgwin(false, "دسترسی کاربران به حالت اولیه باز گردانی شد اما ذخیره هنوز انجام نشده , اما برای (ذخیره) ثبت این وضعیت دکمه ذخیره را بزنید").ShowDialog();
        }
        private void ManageModifyerStatusPermission()
        {
            //مشاهده داده ها و اجرا و ...
            if (PermissionListBox.SelectedItems.Count > 0)
            {
                read.IsEnabled = true;
                insert.IsEnabled = true;
                update.IsEnabled = true;
                delete.IsEnabled = true;
                runn.IsEnabled = true;

                BTN_ITEM_PERCHAP.IsEnabled = true;
            }
            else
            {
                read.IsEnabled = false;
                insert.IsEnabled = false;
                update.IsEnabled = false;
                delete.IsEnabled = false;
                runn.IsEnabled = false;
            }

            if (MLT.IsChecked ?? false)
            {
            }

            var LastUserSelected = GetLastSelectedItemSafely<SALA_DTL>(UserListBox);
            var LastPermissionSelected = GetLastSelectedItemSafely<SALS_PERMIS>(PermissionListBox);

            //if (LastUserSelected != null && LastPermissionSelected != null)
            //{
            //    var FoundUsersPermisons = PERMISIONS_DATA.Where(p => p.USERCO == LastUserSelected.IDD && p.OBJECT == LastPermissionSelected.OBJECT).FirstOrDefault();

            //    //Update the Selected Permission State Properties:
            //    LastPermissionSelected.SEE = FoundUsersPermisons.SEE;
            //    LastPermissionSelected.RUN = FoundUsersPermisons.RUN;
            //    LastPermissionSelected.UPD = FoundUsersPermisons.UPD;
            //    LastPermissionSelected.DEL = FoundUsersPermisons.DEL;
            //    LastPermissionSelected.INP = FoundUsersPermisons.INP;

            //    PermissionListBox.Items.Refresh();
            //}

        }

        private void SS_Click(object sender, RoutedEventArgs e)
        {
            //انتخاب تکی
            ManageSelectionWay();
        }
        private void MLT_Click(object sender, RoutedEventArgs e)
        {
            //انتخاب چندگانه
            ManageSelectionWay();
        }
        private void ManageSelectionWay()
        {
            if (SS.IsChecked ?? false) //if is single then:
            {
                PermissionListBox.SelectionMode = SelectionMode.Single;
                UserListBox.SelectionMode = SelectionMode.Single;
            }
            else
            {
                UserListBox.SelectionMode = SelectionMode.Multiple;
                PermissionListBox.SelectionMode = SelectionMode.Multiple;
            }

            BTN_SELECT_ALLUSER.IsEnabled = MLT.IsChecked ?? false;
        }

        bool AllowFilterPermissions = true;
        private void UserListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!NowIsReady || !UserListBox.IsEnabled) { return; }

            ManageSelectionWay();

            if (UserListBox.SelectedItems.Count > 0)
            {
                if (!PermissionListBox.IsEnabled)
                {
                    PermissionListBox.IsEnabled = true;
                }

                if (AllowFilterPermissions)
                {
                    var CurrentUser = UserListBox.SelectedItem as SALA_DTL;
                    if (CurrentUser != null)
                    {
                        var filteredPermissions = PERMISIONS_DATA.Where(p => p.USERCO == CurrentUser.IDD).ToList();

                        if (filteredPermissions.Count == 0) //Salcheck اون درست نیست !
                        {
                            try
                            {
                                dbms.DoExecuteSQL($"INSERT INTO dbo.SAL_CHEK  (USERCO, OBJECT,RUN) SELECT " + CurrentUser.IDD + " AS USERCO, IDH AS OBJECT,0 AS RUN FROM  dbo.TFORMS");
                                new Msgwin(false, "تعریف این کاربر در سیستم ناقص بود آنرا تکمیل کردم , صرفا جهت اطلاع").Show();
                                return;
                            }
                            catch (Exception)
                            {
                                return;
                            }
                        }
                        //PermissionListBox.ItemsSource = filteredPermissions; //PERMISIONS_DATA<SALS_PERMIS>

                        SearchFilterPermissions();
                    }
                }
            }
            else
            {
                PermissionListBox.IsEnabled = false;
            }
        }

        private void UserListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // ManagePermissionListActive();
        }
        private void PermissionListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //دسترسی ها
            if (!NowIsReady || !PermissionListBox.IsEnabled || PermissionListBox.SelectedItem == null) { return; }

            ManageModifyerStatusPermission();
        }

        byte CC = 0;
        private void BTN_SELECT_ALLUSER_Click(object sender, RoutedEventArgs e)
        {
            if (!UserListBox.IsEnabled)
            {
                return;
            }

            MLT.IsChecked = true;
            ManageSelectionWay();

            bool SETY = false;
            if (CC % 2 == 0)
            {
                SETY = true;
                CC = 0;
            }
            else
            {
                UserListBox.SelectedItems?.Clear();
            }
            CC++;

            AllowFilterPermissions = false;
            foreach (var item in ALL_USERS)
            {
                bool isLast = item == ALL_USERS.Last(); // Check if this is the last item
                if (isLast)
                {
                    AllowFilterPermissions = true;
                }
                item.IsSelected = SETY;
            }
            UserListBox.Items.Refresh();
        }
        private void BTN_SELECT_ALLPERMIS_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled)
            {
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا از انتخاب همه دسترسی ها مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                e.Handled = true;
                return;
            }

            MLT.IsChecked = true;
            ManageSelectionWay();

            bool SETY = false;
            if (CC % 2 == 0)
            {
                SETY = true;
                CC = 0;
            }
            else
            {
                PermissionListBox.SelectedItems?.Clear();
            }
            CC++;

            foreach (var item in PERMISIONS_DATA)
            {
                item.IsSelected = SETY;
            }
            PermissionListBox.Items.Refresh();
        }
        public T? GetLastSelectedItemSafely<T>(ListBox? listBox) where T : class
        {
            // Null check for ListBox
            if (listBox == null)
            {
                throw new ArgumentNullException(nameof(listBox), "ListBox cannot be null.");
            }

            // Check if the ListBox allows multiple selections
            if (listBox.SelectedItems.Count > 0)
            {
                // Safely retrieve the last selected item from SelectedItems collection
                var lastSelectedItem = listBox.SelectedItems.Cast<T>().LastOrDefault();

                if (lastSelectedItem == null)
                {
                }

                return lastSelectedItem;
            }
            else
            {
                // Handle single-selection mode
                var singleSelectedItem = listBox.SelectedItem as T;

                if (singleSelectedItem == null)
                {
                }
                return singleSelectedItem;
            }
        }
        private void BTN_ITEM_PERCHAP_Click(object sender, RoutedEventArgs e)
        {
            //چاپ دسترسی آیتم 

            SALS_PERMIS? CSelectedPermission = GetLastSelectedItemSafely<SALS_PERMIS>(PermissionListBox);

            //var CSelectedPermission = PermissionListBox.SelectedItem as SALS_PERMIS;

            if (CSelectedPermission != null && CSelectedPermission?.USERCO != null)
            {

                var report = new StiReport();

                var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.SETPERMIS.userobjectpermision.mrt");
                report.Load(pathreport);

                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
                ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;

                (report.GetComponentByName("COMPANY_NAME") as StiText).Text = Baseknow.WIDTH_D;
                (report.GetComponentByName("CAPTION1") as StiText).Text = $@"سطوح دسترسی : {CSelectedPermission.CAPTION}";

                var RST_DATA = dbms.DoGetDataSQL<PERMIS_MODEL>(@$"SELECT dbo.TFORMS.CAPTION, dbo.SAL_CHEK.OBJECT, dbo.SAL_CHEK.RUN, dbo.SAL_CHEK.SEE, dbo.SAL_CHEK.INP, dbo.SAL_CHEK.UPD, dbo.SAL_CHEK.DEL, dbo.SAL_CHEK.USERCO, dbo.TFORMS.GRP, dbo.SALA_DTL.SAL_NAME
                                                FROM dbo.TFORMS
                                                     INNER JOIN dbo.SAL_CHEK ON dbo.TFORMS.IDH=dbo.SAL_CHEK.OBJECT
                                                     INNER JOIN dbo.SALA_DTL ON dbo.SAL_CHEK.USERCO=dbo.SALA_DTL.IDD
                                                
                                                WHERE(dbo.SAL_CHEK.OBJECT={CSelectedPermission.OBJECT})  AND (dbo.SALA_DTL.ENABL = 0) ORDER BY USERCO").ToList();

                foreach (var item in RST_DATA)
                {
                    if (item.USERCO == 142)
                    {

                    }
                    if (!string.IsNullOrEmpty(item.SAL_NAME))
                    {
                        item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME);
                    }
                }

                //important Because is takes the default data in design
                report.Dictionary.DataSources.Clear();

                //report.RegBusinessObject("Data", "DataSource1", RST_DATA);
                //report.Dictionary.DataSources.Add(dataSource);

                report.RegData("DataSource1", RST_DATA);
                //report.RegData("Table1", RST_DATA);

                // Ensure the report dictionary is synchronized with the registered data
                report.Dictionary.Synchronize();

                //report.Render();
                //report.Show();

                new Rpts.WINRPT(report, "دسترسی آیتم").Show();
            }
        }
        private void BTN_USERS_PERMIS_Click(object sender, RoutedEventArgs e)
        {
            //چاپ دسترسی کاربر
            if (UserListBox.SelectedItems.Count == 0)
            {
                return;
            }

            var SelectedUser = GetLastSelectedItemSafely<SALA_DTL>(UserListBox);

            //string USS = string.Join(",", SelectedUsers.Select(user => user.IDD.ToString()));
            string USS = SelectedUser.IDD.ToStringNullSafe();

            var RST_DATA = dbms.DoGetDataSQL<PERMIS_MODEL>(@$"SELECT dbo.TFORMS.CAPTION, dbo.SAL_CHEK.OBJECT, dbo.SAL_CHEK.RUN, dbo.SAL_CHEK.SEE, dbo.SAL_CHEK.INP, dbo.SAL_CHEK.UPD, dbo.SAL_CHEK.DEL, dbo.SAL_CHEK.USERCO, dbo.TFORMS.GRP, dbo.SALA_DTL.SAL_NAME
                                                FROM dbo.TFORMS
                                                     INNER JOIN dbo.SAL_CHEK ON dbo.TFORMS.IDH=dbo.SAL_CHEK.OBJECT
                                                     INNER JOIN dbo.SALA_DTL ON dbo.SAL_CHEK.USERCO=dbo.SALA_DTL.IDD
                                                
                                                WHERE ( dbo.SAL_CHEK.USERCO IN({USS}) ) ORDER BY USERCO").ToList();



            var report = new StiReport();

            var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.SETPERMIS.userobjectpermision.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
            ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;

            (report.GetComponentByName("COMPANY_NAME") as StiText).Text = Baseknow.WIDTH_D;
            (report.GetComponentByName("CAPTION1") as StiText).Text = $@"کاربر : {SelectedUser.SAL_NAME}";


            foreach (var item in RST_DATA)
            {
                if (!string.IsNullOrEmpty(item.SAL_NAME))
                {
                    item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME);
                }
            }

                (report.GetComponentByName("Table1_Cell13") as StiText).Text = "{DataSource1.CAPTION}";


            report.Dictionary.DataSources.Clear();
            report.RegData("DataSource1", RST_DATA);
            report.Dictionary.Synchronize();

            //report.Render();
            //report.Show();

            new Rpts.WINRPT(report, "دسترسی کاربر").Show();

        }
        private void BTN_CLEAN_PSELECT_Click(object sender, RoutedEventArgs e)
        {
            var selectedPermissions = PERMISIONS_DATA.Where(p => p.IsSelected).ToList();
            foreach (var item in selectedPermissions)
            {
                item.IsSelected = false;
            }
        }
        private void runn_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled || PermissionListBox.SelectedItems.Count == 0)
            {
                runn.IsChecked = !runn.IsChecked;
                e.Handled = true;
                return;
            }


            // وضعیت جدید از چک‌باکس 'runn' که کاربر با آن تعامل داشته.
            // این وضعیت باید به آیتم(های) انتخاب شده در PermissionListBox برای تمام کاربران انتخاب شده اعمال شود.
            bool newRunState = runn.IsChecked ?? false;

            // گرفتن تمام کاربران انتخاب شده در UserListBox
            var selectedUsersInList = UserListBox.SelectedItems.Cast<SALA_DTL>().ToList();
            if (!selectedUsersInList.Any()) return; // اگر هیچ کاربری انتخاب نشده، خارج شو

            // گرفتن تمام آیتم‌های SALS_PERMIS (فرم‌ها/آبجکت‌ها) که در PermissionListBox انتخاب شده‌اند
            var selectedPermissionObjectsInList = PermissionListBox.SelectedItems.Cast<SALS_PERMIS>().ToList();

            foreach (var user in selectedUsersInList) // برای هر کاربر انتخاب شده
            {
                foreach (var selectedObjectInfoFromUI in selectedPermissionObjectsInList) // برای هر فرم/آبجکت انتخاب شده در PermissionListBox
                {
                    // شیء SALS_PERMIS مربوطه را در مجموعه اصلی PERMISIONS_DATA پیدا کنید.
                    // این شیء، شیئی است که وضعیت آن در حافظه برای ذخیره‌سازی نهایی آپدیت می‌شود.
                    var permissionToUpdateInDataSource = PERMISIONS_DATA.FirstOrDefault(p_data =>
                        p_data.USERCO == user.IDD && p_data.OBJECT == selectedObjectInfoFromUI.OBJECT);

                    if (permissionToUpdateInDataSource != null)
                    {
                        if (permissionToUpdateInDataSource.RUN != newRunState) // فقط در صورت تغییر واقعی، به‌روزرسانی کنید
                        {
                            permissionToUpdateInDataSource.RUN = newRunState;
                            ChangeIsHappend = true; // علامت‌گذاری وقوع تغییر
                        }
                    }
                }
            }

        }
        private void read_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled || PermissionListBox.SelectedItems.Count == 0)
            {
                read.IsChecked = !read.IsChecked;
                e.Handled = true;
                return;
            }

            //مشاهده داده ها
            bool _READ_ = read.IsChecked ?? false;


            // گرفتن تمام کاربران انتخاب شده در UserListBox
            var selectedUsersInList = UserListBox.SelectedItems.Cast<SALA_DTL>().ToList();
            if (!selectedUsersInList.Any()) return; // اگر هیچ کاربری انتخاب نشده، خارج شو

            // گرفتن تمام آیتم‌های SALS_PERMIS (فرم‌ها/آبجکت‌ها) که در PermissionListBox انتخاب شده‌اند
            var selectedPermissionObjectsInList = PermissionListBox.SelectedItems.Cast<SALS_PERMIS>().ToList();

            foreach (var user in selectedUsersInList) // برای هر کاربر انتخاب شده
            {
                foreach (var selectedObjectInfoFromUI in selectedPermissionObjectsInList) // برای هر فرم/آبجکت انتخاب شده در PermissionListBox
                {
                    // شیء SALS_PERMIS مربوطه را در مجموعه اصلی PERMISIONS_DATA پیدا کنید.
                    // این شیء، شیئی است که وضعیت آن در حافظه برای ذخیره‌سازی نهایی آپدیت می‌شود.
                    var permissionToUpdateInDataSource = PERMISIONS_DATA.FirstOrDefault(p_data =>
                        p_data.USERCO == user.IDD && p_data.OBJECT == selectedObjectInfoFromUI.OBJECT);

                    if (permissionToUpdateInDataSource != null)
                    {
                        if (permissionToUpdateInDataSource.SEE != _READ_) // فقط در صورت تغییر واقعی، به‌روزرسانی کنید
                        {
                            permissionToUpdateInDataSource.SEE = _READ_;
                            ChangeIsHappend = true; // علامت‌گذاری وقوع تغییر
                        }
                    }
                }
            }
        }
        private void update_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled || PermissionListBox.SelectedItems.Count == 0)
            {
                update.IsChecked = !update.IsChecked;
                e.Handled = true;
                return;
            }

            //تغییر داده ها
            bool _UPDATE_ = update.IsChecked ?? false;

            // گرفتن تمام کاربران انتخاب شده در UserListBox
            var selectedUsersInList = UserListBox.SelectedItems.Cast<SALA_DTL>().ToList();
            if (!selectedUsersInList.Any()) return; // اگر هیچ کاربری انتخاب نشده، خارج شو

            // گرفتن تمام آیتم‌های SALS_PERMIS (فرم‌ها/آبجکت‌ها) که در PermissionListBox انتخاب شده‌اند
            var selectedPermissionObjectsInList = PermissionListBox.SelectedItems.Cast<SALS_PERMIS>().ToList();

            foreach (var user in selectedUsersInList) // برای هر کاربر انتخاب شده
            {
                foreach (var selectedObjectInfoFromUI in selectedPermissionObjectsInList) // برای هر فرم/آبجکت انتخاب شده در PermissionListBox
                {
                    // شیء SALS_PERMIS مربوطه را در مجموعه اصلی PERMISIONS_DATA پیدا کنید.
                    // این شیء، شیئی است که وضعیت آن در حافظه برای ذخیره‌سازی نهایی آپدیت می‌شود.
                    var permissionToUpdateInDataSource = PERMISIONS_DATA.FirstOrDefault(p_data =>
                        p_data.USERCO == user.IDD && p_data.OBJECT == selectedObjectInfoFromUI.OBJECT);

                    if (permissionToUpdateInDataSource != null)
                    {
                        if (permissionToUpdateInDataSource.UPD != _UPDATE_) // فقط در صورت تغییر واقعی، به‌روزرسانی کنید
                        {
                            permissionToUpdateInDataSource.UPD = _UPDATE_;
                            ChangeIsHappend = true; // علامت‌گذاری وقوع تغییر
                        }
                    }
                }
            }
        }
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled || PermissionListBox.SelectedItems.Count == 0)
            {
                delete.IsChecked = !delete.IsChecked;
                e.Handled = true;
                return;
            }

            //حذف داده ها
            bool _DELETE_ = delete.IsChecked ?? false;

            // گرفتن تمام کاربران انتخاب شده در UserListBox
            var selectedUsersInList = UserListBox.SelectedItems.Cast<SALA_DTL>().ToList();
            if (!selectedUsersInList.Any()) return; // اگر هیچ کاربری انتخاب نشده، خارج شو

            // گرفتن تمام آیتم‌های SALS_PERMIS (فرم‌ها/آبجکت‌ها) که در PermissionListBox انتخاب شده‌اند
            var selectedPermissionObjectsInList = PermissionListBox.SelectedItems.Cast<SALS_PERMIS>().ToList();

            foreach (var user in selectedUsersInList) // برای هر کاربر انتخاب شده
            {
                foreach (var selectedObjectInfoFromUI in selectedPermissionObjectsInList) // برای هر فرم/آبجکت انتخاب شده در PermissionListBox
                {
                    // شیء SALS_PERMIS مربوطه را در مجموعه اصلی PERMISIONS_DATA پیدا کنید.
                    // این شیء، شیئی است که وضعیت آن در حافظه برای ذخیره‌سازی نهایی آپدیت می‌شود.
                    var permissionToUpdateInDataSource = PERMISIONS_DATA.FirstOrDefault(p_data =>
                        p_data.USERCO == user.IDD && p_data.OBJECT == selectedObjectInfoFromUI.OBJECT);

                    if (permissionToUpdateInDataSource != null)
                    {
                        if (permissionToUpdateInDataSource.DEL != _DELETE_) // فقط در صورت تغییر واقعی، به‌روزرسانی کنید
                        {
                            permissionToUpdateInDataSource.DEL = _DELETE_;
                            ChangeIsHappend = true; // علامت‌گذاری وقوع تغییر
                        }
                    }
                }
            }
        }
        private void insert_Click(object sender, RoutedEventArgs e)
        {
            if (!PermissionListBox.IsEnabled || PermissionListBox.SelectedItems.Count == 0)
            {
                insert.IsChecked = !insert.IsChecked;
                e.Handled = true;
                return;
            }

            //اضافه کردن داده ها
            bool _INSERT_ = insert.IsChecked ?? false;


            // گرفتن تمام کاربران انتخاب شده در UserListBox
            var selectedUsersInList = UserListBox.SelectedItems.Cast<SALA_DTL>().ToList();
            if (!selectedUsersInList.Any()) return; // اگر هیچ کاربری انتخاب نشده، خارج شو

            // گرفتن تمام آیتم‌های SALS_PERMIS (فرم‌ها/آبجکت‌ها) که در PermissionListBox انتخاب شده‌اند
            var selectedPermissionObjectsInList = PermissionListBox.SelectedItems.Cast<SALS_PERMIS>().ToList();

            foreach (var user in selectedUsersInList) // برای هر کاربر انتخاب شده
            {
                foreach (var selectedObjectInfoFromUI in selectedPermissionObjectsInList) // برای هر فرم/آبجکت انتخاب شده در PermissionListBox
                {
                    // شیء SALS_PERMIS مربوطه را در مجموعه اصلی PERMISIONS_DATA پیدا کنید.
                    // این شیء، شیئی است که وضعیت آن در حافظه برای ذخیره‌سازی نهایی آپدیت می‌شود.
                    var permissionToUpdateInDataSource = PERMISIONS_DATA.FirstOrDefault(p_data =>
                        p_data.USERCO == user.IDD && p_data.OBJECT == selectedObjectInfoFromUI.OBJECT);

                    if (permissionToUpdateInDataSource != null)
                    {
                        if (permissionToUpdateInDataSource.INP != _INSERT_) // فقط در صورت تغییر واقعی، به‌روزرسانی کنید
                        {
                            permissionToUpdateInDataSource.INP = _INSERT_;
                            ChangeIsHappend = true; // علامت‌گذاری وقوع تغییر
                        }
                    }
                }
            }
        }
        #endregion


        #region ANBAR_KEYS

        public bool OPANBACCESS_SUB_IsFocused { get; private set; }
        public long? CURRENT_ROW_OPANBACCESS_INDEX { get; set; } = 0;

        private int datagridname_tbox_def_index_col;
        public int OPANBACCESS_SUB_DEF_INDEX_COL
        {
            get
            {
                if (OPANBACCESS_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = OPANBACCESS_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBCO")?.DisplayIndex;
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
        public string? OPANBACCESS_ENTERED_VALUE_ROW { get; private set; }
        public OPANBACCESS? OPANBACCESS_CURRENT_ROW_ITEMS { get; private set; }
        public OPANBACCESS? OPANBACCESS_WAS_ROW_ITEM { get; private set; }
        public bool RIGHT_SIGN_IsFocused { get; private set; }

        public bool OPANBACCESS_IsSaveSuccess { get; set; } = false;

        private void OPANBACCESS_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (!(OPANBACCESS_SUB.Items.Count < 1) && !(OPANBACCESS_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_OPANBACCESS_INDEX = OPANBACCESS_SUB.SelectedIndex;
                }
            }
        }
        private void OPANBACCESS_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }
        private void OPANBACCESS_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                OPANBACCESS_SUB_IsFocused = false;
            }
            else
            {
                OPANBACCESS_SUB_IsFocused = true;
            }
        }
        public void OPANBACCESS_SUB_ReGetData(int USERCOD)
        {
            OPANBACCESS_DATA?.Clear();
            var data = dbms.DoGetDataSQL<OPANBACCESS>($"SELECT USERCO, ANBCO, RDF, CRT, UID FROM dbo.OPANBACCESS  WHERE USERCO = {USERCOD}").ToList();
            foreach (var item in data)
            {
                OPANBACCESS_DATA?.Add(item);
            }
        }
        private void OPANBACCESS_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)OPANBACCESS_SUB.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)OPANBACCESS_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && OPANBACCESS_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<OPANBACCESS>(OPANBACCESS_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !OPANBACCESS_SUB.IsReadOnly && OPANBACCESS_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<OPANBACCESS>(OPANBACCESS_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }

            string CURRENT_COLUMN_NAME = "";
            if (OPANBACCESS_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = OPANBACCESS_SUB.CurrentCell.Column?.SortMemberPath;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_ANBAR_Click(null, null);
            }
        }
        private void OPANBACCESS_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            OPANBACCESS_SUB.CellEditEnding -= OPANBACCESS_SUB_CellEditEnding;
            OPANBACCESS_SUB.RowEditEnding -= OPANBACCESS_SUB_RowEditEnding;

            OPANBACCESS_IsSaveSuccess = false;

            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    OPANBACCESS_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    OPANBACCESS_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: OPANBACCESS_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                OPANBACCESS_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //OPANBACCESS_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            OPANBACCESS_SUB.RowEditEnding += OPANBACCESS_SUB_RowEditEnding;
            OPANBACCESS_SUB.CellEditEnding += OPANBACCESS_SUB_CellEditEnding;
        }
        private void OPANBACCESS_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                OPANBACCESS_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                OPANBACCESS_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            OPANBACCESS_CURRENT_ROW_ITEMS = e.Row.Item as OPANBACCESS;
            #endregion

            if (e.Column.SortMemberPath == "ANBCO")
            {
                if (string.IsNullOrEmpty(OPANBACCESS_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("انبار نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    //OPANBACCESS_CURRENT_ROW_ITEMS.ANBCO = OPANBACCESS_WAS_ROW_ITEM?.ANBCO;

                    OPANBACCESS_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else
                {
                    if (OPANBACCESS_CURRENT_ROW_ITEMS?.USERCO != null && OPANBACCESS_CURRENT_ROW_ITEMS.ANBCO != null)
                    {
                        if (OPANBACCESS_ENTERED_VALUE_ROW.ToStringNullSafe() != OPANBACCESS_CURRENT_ROW_ITEMS.ANBCO.ToStringNullSafe())
                        {
                            var RowExisting = dbms.DoGetDataSQL<int?>($@"SELECT TOP 1 USERCO FROM dbo.OPANBACCESS WHERE USERCO = {OPANBACCESS_CURRENT_ROW_ITEMS.USERCO} AND ANBCO = {OPANBACCESS_ENTERED_VALUE_ROW}").FirstOrDefault();
                            if (RowExisting != null)
                            {
                                universControl.PopNotifyShow("این انبار از قبل ثبت شده , لطفا آنرا تغییر دهید !", Pop1, Pop1Text1, Pop_Border1);
                                //OPANBACCESS_CURRENT_ROW_ITEMS.ANBCO = OPANBACCESS_WAS_ROW_ITEM?.ANBCO;
                                OPANBACCESS_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            }
                        }
                    }
                }
            }

        }

        private void OPANBACCESS_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && OPANBACCESS_SUB.SelectedItem != null)
            {
                if (OPANBACCESS_SUB.Items.Count > 0)
                    CURRENT_ROW_OPANBACCESS_INDEX = OPANBACCESS_SUB.SelectedIndex;

                if (!(e is null) && OPANBACCESS_SUB.SelectedItem is not null)
                {
                    if (OPANBACCESS_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        if (CL_LMethods.IsNewPlaceHolder(OPANBACCESS_SUB, OPANBACCESS_SUB.SelectedItem))
                        {
                        }

                        OPANBACCESS_WAS_ROW_ITEM = ((OPANBACCESS)OPANBACCESS_SUB.SelectedItem).Clone() as OPANBACCESS;
                    }
                }
            }
        }
        private void OPANBACCESS_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && OPANBACCESS_SUB.SelectedItem is not null)
            {
                if (OPANBACCESS_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    if (CL_LMethods.IsNewPlaceHolder(OPANBACCESS_SUB, OPANBACCESS_SUB.SelectedItem))
                    { }
                    OPANBACCESS_WAS_ROW_ITEM = ((OPANBACCESS)OPANBACCESS_SUB.SelectedItem).Clone() as OPANBACCESS;
                }
            }
        }
        private void OPANBACCESS_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as OPANBACCESS;
            if (!BodyIsValid(ROW))
            {
                OPANBACCESS_SUB_CANCEL_EDIT();
                return;
            }

            int? USERCO = null;
            try
            {
                OPANBACCESS_IsSaveSuccess = false;

                if (ROW?.USERCO == null) //INSERT
                {
                    var USERY = UserListBoxAnbar.SelectedItem as SALA_DTL;
                    //OUTPUT INSERTED.ID
                    USERCO = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.OPANBACCESS(USERCO, ANBCO) OUTPUT INSERTED.USERCO VALUES( {USERY.IDD} , {ROW?.ANBCO} )").FirstOrDefault();
                    if (USERCO != null)
                    {
                        ROW.USERCO = (int?)USERCO;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.OPANBACCESS SET USERCO = {ROW?.USERCO}, ANBCO = {ROW?.ANBCO} WHERE USERCO = {ROW?.USERCO} AND ANBCO = {OPANBACCESS_WAS_ROW_ITEM.ANBCO}");
                }

                OPANBACCESS_IsSaveSuccess = true;
            }
            catch (SqlException ex)
            {
                OPANBACCESS_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " خطا: انباری تکراری انتخاب شده است آنرا اصلاح کنید ").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
        }

        private bool BodyIsValid(OPANBACCESS _row)
        {
            var ROW = _row;

            var errors = (from object i in OPANBACCESS_SUB.ItemsSource
                          let c = OPANBACCESS_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var USERY = UserListBoxAnbar.SelectedItem as SALA_DTL;
            if (string.IsNullOrEmpty(USERY?.IDD.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کاربری برای کلید انبار انتخاب نشده است" });
            }

            if (string.IsNullOrEmpty(_row.ANBCO?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار در قسمت کلید انبار برای کاربر نمیتواند خالی باشد !" });
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
        private void TabItem_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                AnbarKeyTabItem_IsFocused = false;
            }
            else
            {
                AnbarKeyTabItem_IsFocused = true;
            }
        }
        private void UserListBoxAnbar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            var USERY = UserListBoxAnbar.SelectedItem as SALA_DTL;

            if (USERY != null && USERY?.IDD != null)
            {
                OPANBACCESS_SUB.IsEnabled = true;

                OPANBACCESS_SUB_ReGetData((int)USERY.IDD);

                UpdateDataGridFocus(OPANBACCESS_SUB, "ANBCO");
            }
            else
            {
                OPANBACCESS_SUB.IsEnabled = false;
            }

        }
        private void UserSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //جستجو گر کاربری در انبار ها
            if (!NowIsReady) { return; }

            string searchText = UserSearchBox.Text;
            var filteredUsers = FilterByFuzzyText(SECOND_ALL_USERS, u => u.SAL_NAME, searchText)
                .DistinctBy(p => p.SAL_NAME)
                .ToList();
            UserListBoxAnbar.ItemsSource = filteredUsers;
        }

        private void AnbarErrorMessageAdd(object anbco, List<MsgModel> errorMessages, string errorMessage)
        {
            var anbarName = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 NAMES FROM dbo.TCOD_ANBAR WHERE CODE = {anbco}").FirstOrDefault();
            errorMessages.Add(new MsgModel { MessageText_U = $"{errorMessage} ({anbarName})" });
        }
        private void BTN_DELETE_ANBAR_Click(object sender, RoutedEventArgs e)
        {

            var IsVisible = BTN_DELETE_ANBAR.Visibility == Visibility.Visible;
            if (!BTN_DELETE_ANBAR.IsEnabled || !IsVisible) { return; }

            if (!OPANBACCESS_SUB.IsEnabled || OPANBACCESS_SUB.IsReadOnly || OPANBACCESS_SUB.Items.Count == 0 || OPANBACCESS_SUB.SelectedItems.Count == 0)
                return;

            var msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
                return;

            List<MsgModel> errorMessages = new List<MsgModel>();

            _ = AuditLogger.LogActionAsync(
                              actionType: "DELETE",
                              tableName: "تعیین سطح دسترسی : حذف انبار",
                              recordId: null,
                              oldValue: null,
                              newValue: null,
                              additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var item in OPANBACCESS_SUB.SelectedItems)
            {
                if (CL_LMethods.IsNewPlaceHolder(OPANBACCESS_SUB, item)) continue; // Skip deletion for new placeholder items

                var userco = item.GetType().GetProperty("USERCO")?.GetValue(item);
                var anbco = item.GetType().GetProperty("ANBCO")?.GetValue(item);

                if (userco != null && anbco != null)
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.OPANBACCESS WHERE USERCO = {userco} AND ANBCO = {anbco}");
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        AnbarErrorMessageAdd(anbco, errorMessages, "این انبار دارای گردش است و نمی توان آنرا حذف کرد");
                    }
                    catch (SqlException)
                    {
                        AnbarErrorMessageAdd(anbco, errorMessages, "به دلیل بروز خطا در پایگاه داده این انبار حذف نشد !");
                    }
                    catch
                    {
                        AnbarErrorMessageAdd(anbco, errorMessages, "به دلیل بروز خطا این انبار حذف نشد !");
                    }
                }
            }
            if (errorMessages.Any())
            {
                var distinctMessages = errorMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, distinctMessages).ShowDialog();
            }
            else
            {
                var user = UserListBoxAnbar.SelectedItem as SALA_DTL;
                if (user?.IDD != null)
                {
                    OPANBACCESS_SUB_ReGetData((int)user.IDD);
                    UpdateDataGridFocus(OPANBACCESS_SUB, "ANBCO");
                }
            }
        }

        #endregion


        #region HagheEmza
        private void RIGHT_SIGN_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                RIGHT_SIGN_IsFocused = false;
            }
            else
            {
                RIGHT_SIGN_IsFocused = true;

                if (SV_SGINS.Visibility != Visibility.Visible)
                {
                    SV_SGINS.Visibility = Visibility.Visible;
                }
            }
        }
        private void UserSearchBoxSignRight_TextChanged(object sender, TextChangedEventArgs e)
        {
            //جستجو گر کاربری در انبار ها
            if (!NowIsReady) { return; }

            string searchText = UserSearchBoxSignRight.Text.ToLower();
            var filteredUsers = SECOND_ALL_USERS.Where(u => u.SAL_NAME.ToLower().Contains(searchText)).DistinctBy(p => p.SAL_NAME).ToList();
            UserListBoxSignRight.ItemsSource = filteredUsers;
        }
        public static byte[] StringToVarbinary(string text)
        {
            // Convert string to byte array
            return System.Text.Encoding.Unicode.GetBytes(text);
        }
        private void SV_SIGNS_ReGetData(int USERCOD)
        {
            var RST_SGING = dbms.DoGetDataSQL<SIGN>($"SELECT USERCO, FFR_FROOSH, FFR_HESAB, FFR_MODIR, KFR_BAZAR, KFR_HESAB, KFR_MODIR, ANB_RASID_ANB, ANB_RASID_QC, ANB_HAVL_FROSH, ANB_HAVL_ANB, ANB_HAVL_MODIR, ANB_TOLID_MODIRT, ANB_TOLID_QC, ANB_TOLID_ANB, ANB_KHOROG_ANB, ANB_KHOROG_MODIRT, ANB_KHOROGS_ANB, ANB_KHOROGS_MODIRT, SND_TAHI, SND_MALI, SND_MODIR, FFRB_FROOSH, FFRB_ANB, FFRB_HESAB, FFRB_MODIR, KFRB_BAZAR, KFRB_ANB, KFRB_HESAB, KFRB_MODIR, FFRP_FROOSH, FFRP_ANB, FFRP_HESAB, FFRP_MODIR, RASM_HESAB, RASM_MODIR, PAY_MVAHED, PAY_HESAB, PAY_MAMEL, ENTGH_AZANB, ENTGH_BEANB, ENTGH_MODIR, PAZ_PAZ, PAZ_HES, PAZ_CEO, SGN0124, SGN0224, SGN0324, SGN0126, SGN0226, SGN0326, SGN0133, SGN0233, SGN0333, SGN0132, SGN0232, SGN0332, SGN0134, SGN0234, SGN0334, SGN0135, SGN0235, SGN0335, SGN0136, SGN0236, SGN0336, FFR_FROOSHTX, FFR_HESABTX, FFR_MODIRTX, KFR_BAZARTX, KFR_HESABTX, KFR_MODIRTX, ANB_RASID_ANBTX, ANB_RASID_QCTX, ANB_HAVL_FROSHTX, ANB_HAVL_ANBTX, ANB_HAVL_MODIRTX, ANB_TOLID_MODIRTTX, ANB_TOLID_QCTX, ANB_TOLID_ANBTX, ANB_KHOROG_ANBTX, ANB_KHOROG_MODIRTTX, ANB_KHOROGS_ANBTX, ANB_KHOROGS_MODIRTTX, SND_TAHITX, SND_MALITX, SND_MODIRTX, FFRB_FROOSHTX, FFRB_ANBTX, FFRB_HESABTX," +
            $" FFRB_MODIRTX, KFRB_BAZARTX, KFRB_ANBTX, KFRB_HESABTX, KFRB_MODIRTX, FFRP_FROOSHTX, FFRP_ANBTX, FFRP_HESABTX, FFRP_MODIRTX, RASM_HESABTX, RASM_MODIRTX, PAY_MVAHEDTX, PAY_HESABTX, PAY_MAMELTX, ENTGH_AZANBTX, ENTGH_BEANBTX, ENTGH_MODIRTX, PAZ_PAZTX, PAZ_HESTX, PAZ_CEOTX, SGN0124TX, SGN0224TX, SGN0324TX, SGN0126TX, SGN0226TX, SGN0326TX, SGN0133TX, SGN0233TX, SGN0333TX, SGN0132TX, SGN0232TX, SGN0332TX, SGN0134TX, SGN0234TX, SGN0334TX, SGN0135TX, SGN0235TX, SGN0335TX, SGN0136TX, SGN0236TX, SGN0336TX, SGN0137, SGN0237," +
            $" SGN0337, SGN0137TX, SGN0237TX, SGN0337TX, SGN0140, SGN0240, SGN0340, SGN0140TX, SGN0240TX, SGN0340TX, SGN0141, SGN0241, SGN0341, SGN0141TX, SGN0241TX, SGN0341TX, SGN0142, SGN0242, SGN0342, SGN0142TX, SGN0242TX, SGN0342TX, CRT, UID FROM dbo.SIGN WHERE USERCO = {USERCOD}").FirstOrDefault();

            if (RST_SGING == null)
            {
                SV_SGINS.IsEnabled = false;
                return;
            }

            // Cache of control type assignments to manage future control types easily
            var controlAssigners = new Dictionary<Type, Action<Control, object>>
                {
                   { typeof(CheckBox), (ctrl, value) => ((CheckBox)ctrl).IsChecked = ParseNullableBool(value) },
                   { typeof(TextBox), (ctrl, value) =>
                       {
                           if (ctrl is TextBox textBox)
                           {
                               // Handling specific TextBox names
                               switch (textBox.Name)
                               {
                                   case "KFR_BAZARTX":
                                   case "KFR_HESABTX":
                                        //بازرگانی یا حسابداری مربوط به فاکتور خرید
                                       // Convert the VARBINARY back to string
                                       byte[] byteArray = value as byte[];
                                       textBox.Text = byteArray != null ? System.Text.Encoding.Unicode.GetString(byteArray) : string.Empty;
                                       break;

                                   default:
                                       // Default behavior for other TextBox controls
                                       textBox.Text = value?.ToString() ?? string.Empty;
                                       break;
                               }
                           }
                       }
                   }
                };

            // Get all properties from the data object using reflection
            var dataProperties = RST_SGING.GetType().GetProperties();

            foreach (var property in dataProperties)
            {
                if (property == null)
                {
                    continue;
                }

                string propertyName = property.Name;

                object propertyValue = property.GetValue(RST_SGING);

                var control = SV_SGINS.FindName(propertyName) as Control;
                // Attempt to find the control within SV_SGINS based on property name

                if (control == null)
                {
                    continue; // Control not found, continue to the next property
                }

                // Check if the control's type has an associated assigner, handle unknown types
                if (controlAssigners.TryGetValue(control.GetType(), out var assigner))
                {
                    try
                    {
                        assigner(control, propertyValue);
                    }
                    catch (Exception ex) { }
                }
            }
        }
        private List<Control> GetChildControls(DependencyObject parent)
        {
            var controls = new List<Control>();

            // Iterate through all children in the visual tree
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Control control)
                {
                    controls.Add(control);
                }

                // Recursively check for children in each child control
                controls.AddRange(GetChildControls(child));
            }

            return controls;
        }
        private static bool? ParseNullableBool(object value) // Helper function to parse nullable booleans safely
        {
            if (value == null) return null;
            if (bool.TryParse(value.ToString(), out var result)) return result;
            return null;
        }
        private void UserListBoxSignRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            var USERY = UserListBoxSignRight.SelectedItem as SALA_DTL;

            if (USERY != null && USERY?.IDD != null)
            {
                SV_SGINS.IsEnabled = true;

                SV_SIGNS_ReGetData((int)USERY.IDD);
            }
            else
            {
                SV_SGINS.IsEnabled = false;
            }
        }
        private void SAVE_BTN_SIGN_RIGHT_Click(object sender, RoutedEventArgs e)
        {
            var USERY = UserListBoxSignRight.SelectedItem as SALA_DTL;

            if (USERY == null || USERY?.IDD == null)
            {
                return;
            }

            // Create a dictionary to hold column names and values
            var dataDict = new Dictionary<string, object>
            {
                { "USERCO", USERY.IDD },
            };

            var SubElements = GetChildControls(UNG_SIGN);

            // Iterate through the children of the main container (e.g., SV_SGINS)
            foreach (var child in SubElements)
            {
                if (child is Control control)
                {
                    // Use control name as the SQL column name
                    string columnName = control.Name;

                    // Check if the control is a CheckBox and store its checked state
                    if (control is CheckBox checkBox)
                    {
                        dataDict[columnName] = checkBox.IsChecked ?? false;
                    }
                    // Check if the control is a TextBox and store its text value
                    else if (control is TextBox textBox)
                    {
                        // Convert specific fields to byte arrays
                        if (columnName == "KFR_HESABTX" || columnName == "KFR_BAZARTX")
                        {
                            dataDict[columnName] = Encoding.Unicode.GetBytes(textBox.Text ?? string.Empty);
                        }
                        else
                        {
                            dataDict[columnName] = textBox.Text ?? string.Empty;
                        }
                    }
                }
            }

            // Check if the row already exists
            var existingRecord = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM dbo.SIGN WHERE USERCO = @USERCO", new { USERCO = USERY.IDD }).FirstOrDefault();

            if (existingRecord > 0)
            {
                // If row exists, construct an UPDATE query
                string updateColumns = string.Join(", ", dataDict.Keys.Where(k => k != "USERCO").Select(k => $"{k} = @{k}"));
                string updateQuery = $"UPDATE dbo.SIGN SET {updateColumns} WHERE USERCO = @USERCO";

                // Execute the update query
                int? result = dbms.DoExecuteSQL(updateQuery, dataDict);
            }
            else
            {
                // Construct an INSERT query if no row exists
                string columns = string.Join(", ", dataDict.Keys);
                string values = string.Join(", ", dataDict.Keys.Select(k => "@" + k));
                string insertQuery = $"INSERT INTO dbo.SIGN ({columns}) VALUES ({values})";

                // Execute the insert query
                int? result = dbms.DoExecuteSQL(insertQuery, dataDict);
            }

            SubElements = null;

            universControl.PopNotifyShow("ذخیره حق امضا این کاربر انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        #endregion


        #region HesabBlockyUser
        public int CURRENT_ROW_BLOCK_HES_INDEX { get; private set; }
        public BLOCK_HES? BLOCK_HES_WAS_ROW_ITEM { get; private set; }// = new BLOCK_HES();
        public ObservableCollection<BLOCK_HES> BLOCK_HES_DATA { get; set; } = new ObservableCollection<BLOCK_HES>();
        public bool UserBlockyHesabTabItem_IsFocused { get; private set; }
        public string? BLOCK_HES_ENTERED_VALUE_ROW { get; private set; }
        public BLOCK_HES? BLOCK_HES_CURRENT_ROW_ITEMS { get; private set; }
        public bool BLOCKED_SUB_IsFocused { get; private set; }

        private void TabItem_IsKeyboardFocusWithinChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            //مسدودی حسابها
            if ((bool)e.NewValue == false)
            {
                UserBlockyHesabTabItem_IsFocused = false;
            }
            else
            {
                UserBlockyHesabTabItem_IsFocused = true;
            }
        }
        private void BLOCKED_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && BLOCKED_SUB.SelectedItem != null)
            {
                if (BLOCKED_SUB.Items.Count > 0)
                    CURRENT_ROW_BLOCK_HES_INDEX = BLOCKED_SUB.SelectedIndex;

                if (!(e is null) && BLOCKED_SUB.SelectedItem is not null)
                {
                    if (BLOCKED_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        BLOCK_HES_WAS_ROW_ITEM = ((BLOCK_HES)BLOCKED_SUB.SelectedItem).Clone() as BLOCK_HES;
                    }
                }
            }
        }
        private void BLOCKED_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(BLOCKED_SUB.Items.Count < 1) && !(BLOCKED_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_BLOCK_HES_INDEX = BLOCKED_SUB.SelectedIndex;
                }
            }
        }
        public void BLOCKED_SUB_ReGetData(int USERCO)
        {
            BLOCK_HES_DATA?.Clear();
            var data = dbms.DoGetDataSQL<BLOCK_HES>($"SELECT * FROM dbo.BLOCK_HES WHERE USERCO = {USERCO}").ToList();
            foreach (var item in data)
            {
                item.NAME_HES = CL_LMethods.GET_NAME_HES(item.HES, dbms);

                BLOCK_HES_DATA.Add(item);
            }
        }
        private void BLOCKED_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)BLOCKED_SUB.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)BLOCKED_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && BLOCKED_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<BLOCK_HES>(BLOCKED_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !BLOCKED_SUB.IsReadOnly && BLOCKED_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<BLOCK_HES>(BLOCKED_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }

            string CURRENT_COLUMN_NAME = "";
            if (BLOCKED_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = BLOCKED_SUB.CurrentCell.Column?.SortMemberPath;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_BLOCKYHES_Click(null, null);
            }
        }
        private void BLOCKED_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && BLOCKED_SUB.SelectedItem is not null)
            {
                if (BLOCKED_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    BLOCK_HES_WAS_ROW_ITEM = ((BLOCK_HES)BLOCKED_SUB.SelectedItem).Clone() as BLOCK_HES;
                }
            }
        }
        public bool BLOCKED_IsSaveSuccess { get; set; } = false;
        private void BLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            BLOCKED_SUB.CellEditEnding -= BLOCKED_SUB_CellEditEnding;
            BLOCKED_SUB.RowEditEnding -= BLOCKED_SUB_RowEditEnding;

            BLOCKED_IsSaveSuccess = false;
            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    BLOCKED_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    BLOCKED_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: BLOCKED_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                BLOCKED_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //BLOCKED_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            BLOCKED_SUB.RowEditEnding += BLOCKED_SUB_RowEditEnding;
            BLOCKED_SUB.CellEditEnding += BLOCKED_SUB_CellEditEnding;
        }
        private void BLOCKED_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                BLOCK_HES_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                BLOCK_HES_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            BLOCK_HES_CURRENT_ROW_ITEMS = e.Row.Item as BLOCK_HES;
            #endregion

            ComboBox CHESAB = new ComboBox() { IsEditable = true };
            CHESAB.ItemsSource = new List<Custom_CUST_HESAB>();
            CHESAB.DisplayMemberPath = "NAME";
            CHESAB.SelectedValuePath = "hes";

            if (e.Column.SortMemberPath == "HES")
            {
                if (string.IsNullOrEmpty(BLOCK_HES_ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(BLOCK_HES_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("حساب نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);

                    BLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else
                {
                    if (BLOCK_HES_ENTERED_VALUE_ROW.ToStringNullSafe() != BLOCK_HES_CURRENT_ROW_ITEMS.HES.ToStringNullSafe())
                    {
                        CHESAB.Text = BLOCK_HES_ENTERED_VALUE_ROW?.ToStringNullSafe();

                        CL_LMethods.GetSearchedValueCustomer(CHESAB, "F_USER_PERMITION_FORMS_BLOCKED_SUB", default, dbms, I_AM_SATEHDAST, true);

                        if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                        {
                            BLOCK_HES_CURRENT_ROW_ITEMS.HES = HESAB_FROM_SEARCH.FULL_HES;
                            BLOCK_HES_CURRENT_ROW_ITEMS.NAME_HES = HESAB_FROM_SEARCH.NAME;
                        }
                        else
                        {
                            universControl.PopNotifyShow("حسابی صحیح انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1);
                            BLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                        HESAB_FROM_SEARCH.DoClear();

                        //if (string.IsNullOrEmpty(GetNameHes(BLOCK_HES_CURRENT_ROW_ITEMS.HES)))
                        //{
                        //    universControl.PopNotifyShow("حساب وارد شده صحیح نیست !", Pop1, Pop1Text1, Pop_Border1);

                        //    BLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        //}
                    }
                }
            }
            CHESAB = null;
        }

        private void BLOCKED_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as BLOCK_HES;
            if (!BlockyBodyIsValid(ROW))
            {
                BLOCKED_SUB_CANCEL_EDIT();
                return;
            }


            int? USERCO = null;
            try
            {
                BLOCKED_IsSaveSuccess = false;

                var USERY = UserListBoxBlocky.SelectedItem as SALA_DTL;

                if (ROW?.USERCO == null) //INSERT
                {
                    //OUTPUT INSERTED.ID
                    USERCO = dbms.DoGetDataSQL<int?>(@"IF NOT EXISTS (SELECT 1 FROM dbo.BLOCK_HES WHERE USERCO = @UserCo AND HES = @Hes)
                                                          BEGIN
                                                              INSERT INTO dbo.BLOCK_HES(USERCO, HES) VALUES(@UserCo, @Hes);
                                                          END
                                                          SELECT USERCO FROM dbo.BLOCK_HES WHERE USERCO = @UserCo AND HES = @Hes;",
                                                   new { UserCo = USERY.IDD, Hes = ROW.HES }).FirstOrDefault();
                    if (USERCO != null)
                    {
                        ROW.USERCO = (int?)USERCO;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.BLOCK_HES SET HES = N'{ROW.HES}' WHERE USERCO = {USERY.IDD} AND HES = N'{BLOCK_HES_WAS_ROW_ITEM.HES}'");
                }

                BLOCKED_IsSaveSuccess = true;
            }
            catch (SqlException ex)
            {
                BLOCKED_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " این حساب برای مسدودی تکراری وارد شده!").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
        }

        private bool BlockyBodyIsValid(BLOCK_HES? row)
        {
            var errors = (from object i in BLOCKED_SUB.ItemsSource
                          let c = BLOCKED_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(row?.HES?.ToStringNullSafe()) || string.IsNullOrEmpty(row.HES?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب نمیتواند خالی باشد" });
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
        private void BLOCKED_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                BLOCKED_SUB_IsFocused = false;
            }
            else
            {
                BLOCKED_SUB_IsFocused = true;
            }
        }

        private void UserListBoxBlocky_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            var USERY = UserListBoxBlocky.SelectedItem as SALA_DTL;

            if (USERY != null && USERY?.IDD != null)
            {
                BLOCKED_SUB.IsEnabled = true;
                UNBLOCKED_SUB.IsEnabled = true;

                BLOCKED_SUB_ReGetData((int)USERY.IDD);
                UNBLOCKED_SUB_ReGetData((int)USERY.IDD);
            }
            else
            {
                BLOCKED_SUB.IsEnabled = false;
                UNBLOCKED_SUB.IsEnabled = false;
            }
        }

        private void BlockySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            string searchText = BlockySearchBox.Text.ToLower();
            var filteredUsers = SECOND_ALL_USERS.Where(u => u.SAL_NAME.ToLower().Contains(searchText)).DistinctBy(p => p.SAL_NAME).ToList();
            UserListBoxBlocky.ItemsSource = filteredUsers;
        }

        //------------------------------------------------------------------------------------------------------------
        public bool UNBLOCKED_SUB_IsFocused { get; private set; }
        public long? CURRENT_ROW_BLOCKNON_HES_INDEX { get; set; } = 0;
        public ObservableCollection<BLOCKNON_HES> BLOCKNON_HES_DATA { get; set; } = new ObservableCollection<BLOCKNON_HES>();
        public string? BLOCKNON_HES_ENTERED_VALUE_ROW { get; private set; }
        public BLOCKNON_HES? BLOCKNON_HES_CURRENT_ROW_ITEMS { get; private set; }
        public BLOCKNON_HES? BLOCKNON_HES_WAS_ROW_ITEM { get; private set; }

        private void UNBLOCKED_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && UNBLOCKED_SUB.SelectedItem != null)
            {
                if (UNBLOCKED_SUB.Items.Count > 0)
                    CURRENT_ROW_BLOCKNON_HES_INDEX = UNBLOCKED_SUB.SelectedIndex;

                if (!(e is null) && UNBLOCKED_SUB.SelectedItem is not null)
                {
                    if (UNBLOCKED_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        BLOCKNON_HES_WAS_ROW_ITEM = ((BLOCKNON_HES)UNBLOCKED_SUB.SelectedItem).Clone() as BLOCKNON_HES;
                    }
                }
            }
        }
        private void UNBLOCKED_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(UNBLOCKED_SUB.Items.Count < 1) && !(UNBLOCKED_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_BLOCKNON_HES_INDEX = UNBLOCKED_SUB.SelectedIndex;
                }
            }
        }
        private void UNBLOCKED_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }
        private void UNBLOCKED_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                UNBLOCKED_SUB_IsFocused = false;
            }
            else
            {
                UNBLOCKED_SUB_IsFocused = true;
            }
        }
        public void UNBLOCKED_SUB_ReGetData(int USERCO)
        {
            BLOCKNON_HES_DATA?.Clear();
            var data = dbms.DoGetDataSQL<BLOCKNON_HES>($"SELECT * FROM dbo.BLOCKNON_HES WHERE USERCO = {USERCO}").ToList();
            foreach (var item in data)
            {
                item.NAME_HES = CL_LMethods.GET_NAME_HES(item.HES, dbms);

                BLOCKNON_HES_DATA.Add(item);
            }
        }
        private void UNBLOCKED_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)UNBLOCKED_SUB.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)UNBLOCKED_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && UNBLOCKED_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<BLOCKNON_HES>(UNBLOCKED_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !UNBLOCKED_SUB.IsReadOnly && UNBLOCKED_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<BLOCKNON_HES>(UNBLOCKED_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }

            string CURRENT_COLUMN_NAME = "";
            if (UNBLOCKED_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = UNBLOCKED_SUB.CurrentCell.Column?.SortMemberPath;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_NONBLOCK_Click(null, null);
            }
        }
        public bool UNBLOCKED_IsSaveSuccess { get; set; } = false;
        private void UNBLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            UNBLOCKED_SUB.CellEditEnding -= UNBLOCKED_SUB_CellEditEnding;
            UNBLOCKED_SUB.RowEditEnding -= UNBLOCKED_SUB_RowEditEnding;

            UNBLOCKED_IsSaveSuccess = false;
            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    UNBLOCKED_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    UNBLOCKED_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: UNBLOCKED_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                UNBLOCKED_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //UNBLOCKED_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            UNBLOCKED_SUB.RowEditEnding += UNBLOCKED_SUB_RowEditEnding;
            UNBLOCKED_SUB.CellEditEnding += UNBLOCKED_SUB_CellEditEnding;
        }
        private void UNBLOCKED_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                BLOCKNON_HES_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                BLOCKNON_HES_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            BLOCKNON_HES_CURRENT_ROW_ITEMS = e.Row.Item as BLOCKNON_HES;
            #endregion

            if (e.Column.SortMemberPath == "")
            {
                if (string.IsNullOrEmpty(BLOCKNON_HES_ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    UNBLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //BLOCKNON_HES_CURRENT_ROW_ITEMS.MOGODI_A = BLOCKNON_HES_WAS_ROW_ITEM?.MOGODI_A;
                }
            }


            ComboBox CHESAB = new ComboBox() { IsEditable = true };
            CHESAB.ItemsSource = new List<Custom_CUST_HESAB>();
            CHESAB.DisplayMemberPath = "NAME";
            CHESAB.SelectedValuePath = "hes";

            if (e.Column.SortMemberPath == "HES")
            {
                if (string.IsNullOrEmpty(BLOCKNON_HES_ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(BLOCKNON_HES_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("حساب نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);

                    UNBLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else
                {
                    if (BLOCKNON_HES_CURRENT_ROW_ITEMS?.USERCO != null && BLOCKNON_HES_CURRENT_ROW_ITEMS.HES != null)
                    {
                        if (BLOCKNON_HES_ENTERED_VALUE_ROW.ToStringNullSafe() != BLOCKNON_HES_CURRENT_ROW_ITEMS.HES.ToStringNullSafe())
                        {
                            CHESAB.Text = BLOCKNON_HES_ENTERED_VALUE_ROW?.ToStringNullSafe();

                            CL_LMethods.GetSearchedValueCustomer(CHESAB, "F_USER_PERMITION_FORMS_UNBLOCKED_SUB", default, dbms, I_AM_SATEHDAST, true);

                            if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                            {
                                BLOCKNON_HES_CURRENT_ROW_ITEMS.HES = HESAB_FROM_SEARCH.FULL_HES;
                                BLOCKNON_HES_CURRENT_ROW_ITEMS.NAME_HES = HESAB_FROM_SEARCH.NAME;
                            }
                            else
                            {
                                universControl.PopNotifyShow("حسابی صحیح انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1);
                                UNBLOCKED_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            }
                            HESAB_FROM_SEARCH.DoClear();
                        }
                    }
                }
            }

            CHESAB = null;
        }
        private void UNBLOCKED_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && UNBLOCKED_SUB.SelectedItem is not null)
            {
                if (UNBLOCKED_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    BLOCKNON_HES_WAS_ROW_ITEM = ((BLOCKNON_HES)UNBLOCKED_SUB.SelectedItem).Clone() as BLOCKNON_HES;
                }
            }
        }
        private void UNBLOCKED_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as BLOCKNON_HES;
            if (!UNBLOCKED_BodyIsValid(ROW))
            {
                UNBLOCKED_SUB_CANCEL_EDIT();
                return;
            }


            int? USERCO = null;
            try
            {
                UNBLOCKED_IsSaveSuccess = false;

                var USERY = UserListBoxBlocky.SelectedItem as SALA_DTL;

                if (ROW?.USERCO == null) //INSERT
                {
                    USERCO = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.BLOCKNON_HES(USERCO, HES) OUTPUT INSERTED.USERCO VALUES({USERY.IDD},N'{ROW.HES}')").FirstOrDefault();
                    if (USERCO != null)
                    {
                        ROW.USERCO = (int?)USERCO;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.BLOCKNON_HES SET HES = N'{ROW.HES}' WHERE USERCO = {USERY.IDD} AND HES = N'{BLOCKNON_HES_WAS_ROW_ITEM.HES}'");
                }

                UNBLOCKED_IsSaveSuccess = true;
            }
            catch (SqlException ex)
            {
                UNBLOCKED_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " این حساب برای حسابهای مجاز تکراری وارد شده!").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }

            //UNBLOCKED_SUB.CommitEdit();
        }
        private bool UNBLOCKED_BodyIsValid(BLOCKNON_HES? row)
        {
            var errors = (from object i in UNBLOCKED_SUB.ItemsSource
                          let c = UNBLOCKED_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(row?.HES?.ToStringNullSafe()) || string.IsNullOrEmpty(row.HES?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب نمیتواند خالی باشد" });
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

        private void BTN_DELETE_BLOCKYHES_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE_BLOCKYHES.Visibility == Visibility.Visible;
            if (!BTN_DELETE_BLOCKYHES.IsEnabled || !IsVisible) { return; }

            //مسدودی
            if (!BLOCKED_SUB.IsEnabled || BLOCKED_SUB.IsReadOnly || BLOCKED_SUB.Items.Count == 0 || BLOCKED_SUB.SelectedItems.Count == 0)
                return;

            var msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
                return;

            List<MsgModel> errorMessages = new List<MsgModel>();

            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعیین سطح دسترسی : حذف مسدودی حساب",
                    recordId: null,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var item in BLOCKED_SUB.SelectedItems)
            {
                if (CL_LMethods.IsNewPlaceHolder(BLOCKED_SUB, item)) continue;

                var userco = item.GetType().GetProperty("USERCO")?.GetValue(item);
                var hesy = item.GetType().GetProperty("HES")?.GetValue(item).ToStringNullSafe();

                if (userco != null && hesy != null)
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.BLOCK_HES WHERE USERCO = {userco} AND HES = N'{hesy}'");
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل داشتن گردش حذف نشد !" });
                    }
                    catch (SqlException)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل بروز خطا در پایگاه داده حذف نشد !" });
                    }
                    catch
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل بروز خطا حذف نشد !" });
                    }

                }
            }
            if (errorMessages.Any())
            {
                var distinctMessages = errorMessages.Select(x => x.MessageText_U)
                    .Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, distinctMessages).ShowDialog();
            }
            else
            {
                var user = UserListBoxBlocky.SelectedItem as SALA_DTL;
                if (user?.IDD != null)
                {
                    BLOCKED_SUB_ReGetData((int)user.IDD);
                }
            }
        }

        private void BTN_DELETE_NONBLOCK_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE_NONBLOCK.Visibility == Visibility.Visible;
            if (!BTN_DELETE_NONBLOCK.IsEnabled || !IsVisible) { return; }

            //حساب های مجاز
            //مسدودی
            if (!UNBLOCKED_SUB.IsEnabled || UNBLOCKED_SUB.IsReadOnly || UNBLOCKED_SUB.Items.Count == 0 || UNBLOCKED_SUB.SelectedItems.Count == 0)
                return;

            var msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
                return;

            List<MsgModel> errorMessages = new List<MsgModel>();

            _ = AuditLogger.LogActionAsync(
                       actionType: "DELETE",
                       tableName: "تعیین سطح دسترسی : حذف حساب مجاز",
                       recordId: null,
                       oldValue: null,
                       newValue: null,
                       additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var item in UNBLOCKED_SUB.SelectedItems)
            {
                if (CL_LMethods.IsNewPlaceHolder(UNBLOCKED_SUB, item)) continue;

                var userco = item.GetType().GetProperty("USERCO")?.GetValue(item);
                var hesy = item.GetType().GetProperty("HES")?.GetValue(item).ToStringNullSafe();

                if (userco != null && hesy != null)
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.BLOCKNON_HES WHERE USERCO = {userco} AND HES = N'{hesy}'");
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل داشتن گردش حذف نشد !" });
                    }
                    catch (SqlException)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل بروز خطا در پایگاه داده حذف نشد !" });
                    }
                    catch
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این حساب با کد {hesy} و نام [{GetNameHes(hesy)}] به دلیل بروز خطا حذف نشد !" });
                    }

                }
            }
            if (errorMessages.Any())
            {
                var distinctMessages = errorMessages.Select(x => x.MessageText_U)
                    .Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, distinctMessages).ShowDialog();
            }
            else
            {
                var user = UserListBoxBlocky.SelectedItem as SALA_DTL;
                if (user?.IDD != null)
                {
                    UNBLOCKED_SUB_ReGetData((int)user.IDD);
                }
            }
        }

        #endregion

        #region Sub_User

        public ObservableCollection<CHARTSAZMANI> CHARTSAZMANI_DATA { get; set; } = new ObservableCollection<CHARTSAZMANI>();

        public bool CHARTSAZMANI_SUB_IsFocused { get; private set; }
        public long? CURRENT_ROW_CHARTSAZMANI_INDEX { get; set; } = 0;

        private int chartsazmani_sub_def_index_col;
        public int CHARTSAZMANI_SUB_DEF_INDEX_COL
        {
            get
            {
                if (CHARTSAZMANI_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = CHARTSAZMANI_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "SUBUSERCO")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        chartsazmani_sub_def_index_col = 0;
                    }
                    else
                    {
                        chartsazmani_sub_def_index_col = (int)defaultcolumnindex;
                    }
                }
                return chartsazmani_sub_def_index_col;
            }
        }
        public string? CHARTSAZMANI_ENTERED_VALUE_ROW { get; private set; }
        public CHARTSAZMANI? CHARTSAZMANI_CURRENT_ROW_ITEMS { get; private set; }
        public CHARTSAZMANI? CHARTSAZMANI_WAS_ROW_ITEM { get; private set; }
        public Visual I_AM_SATEHDAST { get; private set; }


        private void CHARTSAZMANI_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && CHARTSAZMANI_SUB.SelectedItem != null)
            {
                if (CHARTSAZMANI_SUB.Items.Count > 0)
                    CURRENT_ROW_CHARTSAZMANI_INDEX = CHARTSAZMANI_SUB.SelectedIndex;

                if (!(e is null) && CHARTSAZMANI_SUB.SelectedItem is not null)
                {
                    if (CHARTSAZMANI_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        CHARTSAZMANI_WAS_ROW_ITEM = ((CHARTSAZMANI)CHARTSAZMANI_SUB.SelectedItem).Clone() as CHARTSAZMANI;
                    }
                }
            }
        }

        private void CHARTSAZMANI_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(CHARTSAZMANI_SUB.Items.Count < 1) && !(CHARTSAZMANI_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_CHARTSAZMANI_INDEX = CHARTSAZMANI_SUB.SelectedIndex;
                }
            }
        }
        private void CHARTSAZMANI_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                CHARTSAZMANI_SUB_IsFocused = false;
            }
            else
            {
                CHARTSAZMANI_SUB_IsFocused = true;
            }
        }
        public void CHARTSAZMANI_SUB_ReGetData(int USERCO)
        {
            CHARTSAZMANI_DATA?.Clear();
            var data = dbms.DoGetDataSQL<CHARTSAZMANI>($"SELECT * FROM dbo.CHARTSAZMANI WHERE USERCO = {USERCO}").ToList();
            foreach (var item in data)
            {
                CHARTSAZMANI_DATA.Add(item);
            }
        }
        private void CHARTSAZMANI_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isEditing = ((IEditableCollectionView)CHARTSAZMANI_SUB.Items).IsEditingItem;
            var isNewEmpty = ((IEditableCollectionView)CHARTSAZMANI_SUB.Items).IsAddingNew;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C) //Copy
            {
                if (!isEditing && CHARTSAZMANI_SUB.IsEnabled)
                {
                    e.Handled = true;

                    DataGridClipboardManager.CopySelectedItems<CHARTSAZMANI>(CHARTSAZMANI_SUB);
                }
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V) //Paste
            {
                if (!isEditing && !isNewEmpty && !CHARTSAZMANI_SUB.IsReadOnly && CHARTSAZMANI_SUB.IsEnabled)
                {
                    e.Handled = true;
                    IsPastingRows = true;
                    DataGridClipboardManager.PasteItems<CHARTSAZMANI>(CHARTSAZMANI_SUB, ValidateDataGridRow, AddItemToDataSource);
                    IsPastingRows = false;
                }
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Suby_Click(null, null);
            }
        }

        public bool CHARTSAZMANI_IsSaveSuccess { get; set; } = false;
        private void CHARTSAZMANI_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            CHARTSAZMANI_SUB.CellEditEnding -= CHARTSAZMANI_SUB_CellEditEnding;
            CHARTSAZMANI_SUB.RowEditEnding -= CHARTSAZMANI_SUB_RowEditEnding;

            CHARTSAZMANI_IsSaveSuccess = false;
            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    CHARTSAZMANI_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    CHARTSAZMANI_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: CHARTSAZMANI_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                CHARTSAZMANI_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //OPANBACCESS_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            CHARTSAZMANI_SUB.RowEditEnding += CHARTSAZMANI_SUB_RowEditEnding;
            CHARTSAZMANI_SUB.CellEditEnding += CHARTSAZMANI_SUB_CellEditEnding;
        }
        private void CHARTSAZMANI_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                CHARTSAZMANI_ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                CHARTSAZMANI_ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CHARTSAZMANI_CURRENT_ROW_ITEMS = e.Row.Item as CHARTSAZMANI;
            #endregion


            if (e.Column.SortMemberPath == "SUBUSERCO")
            {
                if (string.IsNullOrEmpty(CHARTSAZMANI_ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("کاربر زیرمجموعه نمیتواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    CHARTSAZMANI_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
                else
                {
                    if (CHARTSAZMANI_CURRENT_ROW_ITEMS?.USERCO != null && CHARTSAZMANI_CURRENT_ROW_ITEMS.SUBUSERCO != null)
                    {
                        if (CHARTSAZMANI_ENTERED_VALUE_ROW.ToStringNullSafe() != CHARTSAZMANI_CURRENT_ROW_ITEMS.SUBUSERCO.ToStringNullSafe())
                        {
                            var USERY = UserListBoxSuby.SelectedItem as SALA_DTL;
                            var RowExisting = dbms.DoGetDataSQL<int?>($@"SELECT TOP 1 USERCO FROM dbo.CHARTSAZMANI WHERE USERCO = {USERY.IDD} AND SUBUSERCO = {CHARTSAZMANI_ENTERED_VALUE_ROW}").FirstOrDefault();
                            if (RowExisting != null)
                            {
                                universControl.PopNotifyShow("این کاربر زیرمجموعه از قبل ثبت شده , لطفا آنرا تغییر دهید !", Pop1, Pop1Text1, Pop_Border1);
                                CHARTSAZMANI_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            }
                        }
                    }
                }
            }
        }
        private void CHARTSAZMANI_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && CHARTSAZMANI_SUB.SelectedItem is not null)
            {
                if (CHARTSAZMANI_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    CHARTSAZMANI_WAS_ROW_ITEM = ((CHARTSAZMANI)CHARTSAZMANI_SUB.SelectedItem).Clone() as CHARTSAZMANI;
                    //if (CHARTSAZMANI_WAS_ROW_ITEM.SUBUSERCO == 0)
                    //{
                    //    e.Cancel = true;
                    //    CHARTSAZMANI_SUB.CancelEdit(DataGridEditingUnit.Row);
                    //    CHARTSAZMANI_SUB_CANCEL_EDIT();
                    //    CHARTSAZMANI_SUB.CommitEdit();
                    //}
                }
            }
        }
        private void CHARTSAZMANI_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var ROW = e.Row.Item as CHARTSAZMANI;
            if (!CHARTSAZMANI_BodyIsValid(ROW))
            {
                return;
            }

            int? RDF = null;
            var USERY = UserListBoxSuby.SelectedItem as SALA_DTL;
            try
            {
                CHARTSAZMANI_IsSaveSuccess = false;

                if (ROW?.RDF == null) //INSERT
                {
                    //OUTPUT INSERTED.RDF
                    RDF = dbms.DoGetDataSQL<int?>(@$"INSERT INTO dbo.CHARTSAZMANI (USERCO ,SUBUSERCO) OUTPUT INSERTED.RDF VALUES ({USERY.IDD}, {ROW.SUBUSERCO})").FirstOrDefault();
                    if (RDF != null)
                    {
                        ROW.RDF = RDF;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.CHARTSAZMANI SET SUBUSERCO = {ROW.SUBUSERCO} WHERE USERCO = {USERY.IDD} AND RDF = {ROW.RDF}");
                }

                CHARTSAZMANI_IsSaveSuccess = true;
            }
            catch (SqlException ex)
            {
                CHARTSAZMANI_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "کاربر تکراری برای زیر مجموعه انتخاب شده !").ShowDialog();
                    return;
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
            }
            catch
            {
                new Msgwin(false, "خطا در ذخیره کاربر زیر مجموعه").ShowDialog();
                return;
            }
        }
        private bool CHARTSAZMANI_BodyIsValid(CHARTSAZMANI? _row)
        {
            var ROW = _row;

            var errors = (from object i in CHARTSAZMANI_SUB.ItemsSource
                          let c = CHARTSAZMANI_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(_row.SUBUSERCO?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کاربر زیرمجموعه نمیتواند خالی باشد !" });
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
        private void UserSearchBoxSuby_TextChanged(object sender, TextChangedEventArgs e)
        {
            //جستجو گر کاربری در انبار ها
            if (!NowIsReady) { return; }

            string searchText = UserSearchBoxSuby.Text;
            var filteredUsers = FilterByFuzzyText(SECOND_ALL_USERS, u => u.SAL_NAME, searchText)
                .DistinctBy(p => p.SAL_NAME)
                .ToList();
            UserListBoxSuby.ItemsSource = filteredUsers;
        }

        private void UserListBoxSuby_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            var USERY = UserListBoxSuby.SelectedItem as SALA_DTL;

            if (USERY != null && USERY?.IDD != null)
            {
                CHARTSAZMANI_SUB.IsEnabled = true;
                BTN1_ADDALL_SUB.IsEnabled = true;

                CHARTSAZMANI_SUB_ReGetData((int)USERY.IDD);

                UpdateDataGridFocus(CHARTSAZMANI_SUB, "SUBUSERCO");
            }
            else
            {
                CHARTSAZMANI_SUB.IsEnabled = false;
                BTN1_ADDALL_SUB.IsEnabled = false;
            }
        }
        private void BTN_DELETE_Suby_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE_Suby.Visibility == Visibility.Visible;
            if (!BTN_DELETE_Suby.IsEnabled || !IsVisible) { return; }

            //مسدودی
            if (!CHARTSAZMANI_SUB.IsEnabled || CHARTSAZMANI_SUB.IsReadOnly || CHARTSAZMANI_SUB.Items.Count == 0 || CHARTSAZMANI_SUB.SelectedItems.Count == 0)
                return;

            var msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
                return;

            List<MsgModel> errorMessages = new List<MsgModel>();

            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعیین سطح دسترسی : حذف کاربر زیرمجموعه",
                    recordId: null,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var item in CHARTSAZMANI_SUB.SelectedItems)
            {
                if (CL_LMethods.IsNewPlaceHolder(CHARTSAZMANI_SUB, item)) continue;

                var SUBUSERCO = item.GetType().GetProperty("SUBUSERCO")?.GetValue(item);
                var RDF = item.GetType().GetProperty("RDF")?.GetValue(item).ToStringNullSafe();

                if (SUBUSERCO != null && RDF != null)
                {
                    try
                    {
                        dbms.DoExecuteSQL($@"DELETE FROM dbo.CHARTSAZMANI WHERE RDF = {RDF}");
                    }
                    catch (SqlException ex) when (ex.Number == 547)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این کاربر با کد : {SUBUSERCO} دارای گردش است و نمیتوان حذف کرد" });
                    }
                    catch (SqlException)
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"این کاربر {SUBUSERCO} به دلیل بروز خطا در پایگاه داده حذف نشد !" });
                    }
                    catch
                    {
                        errorMessages.Add(new MsgModel { MessageText_U = $"خطا در حذف این کاربر : {SUBUSERCO}" });
                    }

                }
            }
            if (errorMessages.Any())
            {
                var distinctMessages = errorMessages.Select(x => x.MessageText_U)
                    .Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, distinctMessages).ShowDialog();
            }
            else
            {
                var user = UserListBoxSuby.SelectedItem as SALA_DTL;
                if (user?.IDD != null)
                {
                    CHARTSAZMANI_SUB_ReGetData((int)user.IDD);
                }
            }
        }

        const string HAMEH_SUBS = "همه";
        private void BTN1_ADDALL_SUB_Click(object sender, RoutedEventArgs e)
        {
            if (!CHARTSAZMANI_SUB.IsEnabled || CHARTSAZMANI_SUB.IsReadOnly)
            {
                return;
            }

            var USERY = UserListBoxSuby.SelectedItem as SALA_DTL;
            if (USERY != null && USERY?.IDD != null)
            {
                //var isContainsHameh = CL_LMethods.CompareArabicPersianStrings(USERY.HES, HAMEH_SUBS);
                //var _Row_ = CHARTSAZMANI_DATA.Where(x => !string.IsNullOrEmpty(x.SUBUSERCO?.ToStringNullSafe()) && (CL_LMethods.CompareArabicPersianStrings(x.SUBUSERCO.ToStringNullSafe(), HAMEH_SUBS))).FirstOrDefault();

                var _Row_ = CHARTSAZMANI_DATA.Where(x => !string.IsNullOrEmpty(x.SUBUSERCO?.ToStringNullSafe()) && x.SUBUSERCO == 0).FirstOrDefault();
                var isContainsHameh = _Row_ != null && _Row_.SUBUSERCO != null;
                if (isContainsHameh)
                {
                    universControl.PopNotifyShow("این کاربر دارای دسترسی به همه کاربران است.", Pop1, Pop1Text1, Pop_Border1);
                }
                else
                {
                    var newRow = new CHARTSAZMANI { USERCO = USERY.IDD, SUBUSERCO = 0, };
                    CHARTSAZMANI_DATA.Add(newRow);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        CHARTSAZMANI_SUB.Focus();
                        CHARTSAZMANI_SUB.SelectedItem = newRow;
                        CHARTSAZMANI_SUB.CurrentCell = new DataGridCellInfo(newRow, CHARTSAZMANI_SUB.Columns[0]); // Ensure a specific cell is selected
                        CHARTSAZMANI_SUB.ScrollIntoView(newRow);

                        CHARTSAZMANI_SUB.BeginEdit();
                    }));
                }
            }
        }

        #endregion

        #region Black_List
        private void BlackListIsKeboardFoucsWithin(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Black List
            if ((bool)e.NewValue == false)
            {
                BlackListTabItem_IsFocused = false;
            }
            else
            {
                BlackListTabItem_IsFocused = true;
            }
        }
        private void DCH_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            BLOCK_CUSTOMER_SUB.BeginEdit();
        }
        public int BLOCK_CUSTOMER_INDEX { get; private set; }
        public BLOCK_CUSTOMER? BLOCK_CUSTOMER_WAS_ROW_ITEM { get; set; }
        public BLOCK_CUSTOMER? CURRENT_ROW_LOCK_CUSTOMER { get; set; } // = new();

        public int BLOCK_CUSTOMER_SUB_INDEX_DEF_INDEX_COL
        {
            get
            {
                if (BLOCK_CUSTOMER_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = BLOCK_CUSTOMER_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NAME_HES")?.DisplayIndex;
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

        public bool BlackListTabItem_IsFocused { get; set; }

        private void BLOCK_CUSTOMER_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && BLOCK_CUSTOMER_SUB.SelectedItem != null)
            {
                if (BLOCK_CUSTOMER_SUB.Items.Count > 0)
                    BLOCK_CUSTOMER_INDEX = BLOCK_CUSTOMER_SUB.SelectedIndex;

                if (!(e is null) && BLOCK_CUSTOMER_SUB.SelectedItem is not null)
                {
                    if (BLOCK_CUSTOMER_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        BLOCK_CUSTOMER_WAS_ROW_ITEM = ((BLOCK_CUSTOMER)BLOCK_CUSTOMER_SUB.SelectedItem).Clone() as BLOCK_CUSTOMER;
                    }
                }
            }
        }
        private void BLOCK_CUSTOMER_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (!(BLOCK_CUSTOMER_SUB.Items.Count < 1) && !(BLOCK_CUSTOMER_SUB.SelectedItem is null))
                {
                    BLOCK_CUSTOMER_INDEX = BLOCK_CUSTOMER_SUB.SelectedIndex;
                }
            }
        }
        private void BLOCK_CUSTOMER_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && BLOCK_CUSTOMER_SUB.SelectedItem is not null)
            {
                if (BLOCK_CUSTOMER_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    BLOCK_CUSTOMER_WAS_ROW_ITEM = ((BLOCK_CUSTOMER)BLOCK_CUSTOMER_SUB.SelectedItem).Clone() as BLOCK_CUSTOMER;
                }
            }
        }
        private void BLOCK_CUSTOMER_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            //{
            //    e.Handled = true;
            //    BTN_DELETE_Click(null, null);
            //}
        }
        private void BLOCK_CUSTOMER_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            BLOCK_CUSTOMER_SUB.CellEditEnding -= BLOCK_CUSTOMER_SUB_CellEditEnding;
            BLOCK_CUSTOMER_SUB.RowEditEnding -= BLOCK_CUSTOMER_SUB_RowEditEnding;

            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    BLOCK_CUSTOMER_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    BLOCK_CUSTOMER_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: BLOCK_CUSTOMER_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                BLOCK_CUSTOMER_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //OPANBACCESS_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            BLOCK_CUSTOMER_SUB.RowEditEnding += BLOCK_CUSTOMER_SUB_RowEditEnding;
            BLOCK_CUSTOMER_SUB.CellEditEnding += BLOCK_CUSTOMER_SUB_CellEditEnding;
        }
        private bool BLOCK_CUSTOMER_BodyIsValid(BLOCK_CUSTOMER? _row)
        {
            var ROW = _row;

            var errors = (from object i in BLOCK_CUSTOMER_SUB.ItemsSource
                          let c = BLOCK_CUSTOMER_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(_row?.HES?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب نمیتواند خالی باشد" });
            }

            //STBLKDT //تاریخ مسدود شدن
            if (string.IsNullOrEmpty(_row?.STBLKDT?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مسدود شدن نمیتواند خالی باشد" });
            }
            else
            {
                var (_IsValid_, _Msg_) = CL_LMethods.DATE_IS_VALID(_row.STBLKDT.ToString(), "تاریخ مسدود شدن", false);
                if (!_IsValid_)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = _Msg_ });
                }
            }

            //ENDBLK //تاریخ باز شدن
            if (string.IsNullOrEmpty(_row?.ENDBLKDT?.ToStringNullSafe()))
            {
            }
            else
            {
                var (_IsValid_, _Msg_) = CL_LMethods.DATE_IS_VALID(_row.ENDBLKDT.ToString(), "تاریخ باز شدن", false);
                if (!_IsValid_)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = _Msg_ });
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
        private void BLOCK_CUSTOMER_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            #region REFILL_CURRENTS
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

            string? ENTERED_VALUE_ROW = null;
            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                ENTERED_VALUE_ROW = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();

            ComboBox HES_COMBO = null;
            if (e.EditingElement is ContentPresenter contentPresenter)
            {
                HES_COMBO = contentPresenter.ContentTemplate.FindName("HESEditCombo", contentPresenter) as ComboBox;

                if (HES_COMBO == null)
                {
                    HES_COMBO = contentPresenter.ContentTemplate.FindName("EditCombo", contentPresenter) as ComboBox;
                }
                if (HES_COMBO == null)
                {
                    HES_COMBO = DataGridHelper.FindVisualChild<ComboBox>(contentPresenter);
                }
                if (HES_COMBO != null)
                {
                    ENTERED_VALUE_ROW = HES_COMBO.Text;
                    //ENTERED_VALUE_ROW = HES_COMBO.SelectedValue.ToStringNullSafe();
                }
            }

            CURRENT_ROW_LOCK_CUSTOMER = e.Row.Item as BLOCK_CUSTOMER;

            #endregion

            //نام حساب
            if (e.Column.SortMemberPath == "NAME_HES" || e.Column.Header.ToString() == "نام حساب")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    BLOCK_CUSTOMER_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    CURRENT_ROW_LOCK_CUSTOMER.HES = BLOCK_CUSTOMER_WAS_ROW_ITEM.HES;
                    CURRENT_ROW_LOCK_CUSTOMER.NAME_HES = BLOCK_CUSTOMER_WAS_ROW_ITEM.NAME_HES;
                    return;
                }
                else
                {
                    //if (HES_COMBO?.SelectedValue is null || HES_COMBO?.SelectedValue != CURRENT_ROW_LOCK_CUSTOMER?.HES) //if is different then
                    if (HES_COMBO?.SelectedValue is null || HES_COMBO.SelectedValue?.ToString() != CURRENT_ROW_LOCK_CUSTOMER?.HES) //if is different then
                    {
                        var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HES_COMBO, dbms);
                        if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
                        {
                            CURRENT_ROW_LOCK_CUSTOMER.HES = BLOCK_CUSTOMER_WAS_ROW_ITEM.HES;
                            CURRENT_ROW_LOCK_CUSTOMER.NAME_HES = BLOCK_CUSTOMER_WAS_ROW_ITEM.NAME_HES;
                            universControl.PopNotifyShowUp($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        }
                        else
                        {
                            CURRENT_ROW_LOCK_CUSTOMER.HES = _SelectedHesab_.hes;
                            CURRENT_ROW_LOCK_CUSTOMER.NAME_HES = _SelectedHesab_.NAME;
                        }
                    }
                }
            }

            //تاریخ مسدود شدن
            if (e.Column.SortMemberPath == "STBLKDT")
            {
                var MyTarikh = ENTERED_VALUE_ROW?.ToStringNullSafe();
                if (string.IsNullOrEmpty(MyTarikh))
                {
                    CURRENT_ROW_LOCK_CUSTOMER.STBLKDT = BLOCK_CUSTOMER_WAS_ROW_ITEM.STBLKDT;
                    universControl.PopNotifyShowUp($"تاریخ مسدود شدن نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
                else
                {
                    var (_IsValid_, _Msg_) = CL_LMethods.DATE_IS_VALID(MyTarikh, "تاریخ مسدود شدن", false);
                    if (!_IsValid_)
                    {
                        CURRENT_ROW_LOCK_CUSTOMER.STBLKDT = BLOCK_CUSTOMER_WAS_ROW_ITEM.STBLKDT;
                        universControl.PopNotifyShowUp(_Msg_, Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    }
                }
            }

            //تاریخ باز شدن
            if (e.Column.SortMemberPath == "ENDBLKDT")
            {
                var MyTarikh = ENTERED_VALUE_ROW?.ToStringNullSafe();
                if (string.IsNullOrEmpty(MyTarikh))
                {
                }
                else
                {
                    var (_IsValid_, _Msg_) = CL_LMethods.DATE_IS_VALID(MyTarikh, "تاریخ باز شدن", false);
                    if (!_IsValid_)
                    {
                        CURRENT_ROW_LOCK_CUSTOMER.ENDBLKDT = BLOCK_CUSTOMER_WAS_ROW_ITEM.ENDBLKDT;
                        universControl.PopNotifyShowUp(_Msg_, Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    }
                }
            }


        }
        private void BLOCK_CUSTOMER_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as BLOCK_CUSTOMER;
            if (e.Row.Item == null || ROW is null)
            {
                return;
            }

            if (!BLOCK_CUSTOMER_BodyIsValid(ROW))
            {
                BLOCK_CUSTOMER_SUB_CANCEL_EDIT();
                return;
            }

            try
            {
                // بررسی داده تکراری در پایگاه داده پیش از عملیات ثبت یا ویرایش
                string checkDuplicateSql = ROW.ID is null or 0
                    ? "SELECT COUNT(1) FROM dbo.BLOCK_CUSTOMER WHERE HES = @HES"
                    : "SELECT COUNT(1) FROM dbo.BLOCK_CUSTOMER WHERE HES = @HES AND ID != @ID";

                var checkParams = ROW.ID is null
                    ? (object)new { HES = ROW.HES }
                    : (object)new { HES = ROW.HES, ID = ROW.ID };

                int duplicateCount = dbms.DoGetDataSQL<int>(checkDuplicateSql, checkParams).FirstOrDefault();

                if (duplicateCount > 0)
                {
                    new Msgwin(false, $"حساب وارد شده ({ROW.NAME_HES}) قبلاً در لیست سیاه ثبت شده و تکراری می‌باشد!").ShowDialog();
                    BLOCK_CUSTOMER_SUB_CANCEL_EDIT();
                    return;
                }

                int? theidd = null;

                if (ROW?.ID is null) //Insert
                {
                    string sql = @"
                            INSERT INTO dbo.BLOCK_CUSTOMER 
                            (HES, STBLKDT, ENDBLKDT, ENDBLK, COMM, CUSER,EUSER, UID)
                            OUTPUT INSERTED.ID
                            VALUES 
                            (@HES, @STBLKDT, @ENDBLKDT, @ENDBLK, @COMM, @CUSER,@EUSER, @UID)";

                    var parameters = new
                    {
                        HES = ROW?.HES,
                        STBLKDT = ROW?.STBLKDT,
                        ENDBLKDT = ROW?.ENDBLKDT,
                        ENDBLK = ROW.ENDBLK.HasValue ? true : false,
                        COMM = ROW?.COMM,
                        CUSER = ROW?.CUSER,
                        EUSER = ROW?.EUSER,
                        UID = Baseknow.USERCOD
                    };

                    theidd = dbms.DoGetDataSQL<int?>(sql, parameters).FirstOrDefault();

                }
                else //Update
                {
                    string sql = @"
                                  UPDATE dbo.BLOCK_CUSTOMER
                                  SET 
                                      HES = @HES,
                                      STBLKDT = @STBLKDT,
                                      ENDBLKDT = @ENDBLKDT,
                                      ENDBLK = @ENDBLK,
                                      COMM = @COMM,
                                      EUSER = @EUSER
                                  WHERE ID = @ID";

                    var parameters = new
                    {
                        HES = ROW?.HES,
                        STBLKDT = ROW?.STBLKDT,
                        ENDBLKDT = ROW?.ENDBLKDT,
                        ENDBLK = ROW.ENDBLK.HasValue ? true : false,
                        COMM = ROW?.COMM,
                        EUSER = ROW?.EUSER,
                        ID = ROW?.ID
                    };

                    dbms.DoExecuteSQL(sql, parameters);
                }

                if (theidd != null)
                {
                    ROW.ID = theidd;
                }
            }
            catch (SqlException ex)
            {
                BLOCK_CUSTOMER_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این سطر حاوی اطلاعات تکراری است و نمیتواند ذخیره کرد").ShowDialog();
                }
                else
                {
                    throw;
                }
                return;
            }
            catch (Exception)
            {
                BLOCK_CUSTOMER_SUB_CANCEL_EDIT();
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog();
                return;
            }
        }

        private void BLOCK_CUSTOMER_ReGetData()
        {

            BLOCK_CUSTOMER_DATA?.Clear();

            var RST = dbms.DoGetDataSQL<BLOCK_CUSTOMER>(@$"SELECT dbo.BLOCK_CUSTOMER.HES,
                           dbo.CUST_HESAB.NAME AS NAME_HES,
                           dbo.BLOCK_CUSTOMER.STBLKDT,
                           dbo.BLOCK_CUSTOMER.ENDBLKDT,
                           dbo.BLOCK_CUSTOMER.ENDBLK,
                           dbo.BLOCK_CUSTOMER.COMM,
                           dbo.BLOCK_CUSTOMER.CUSER,
                           dbo.BLOCK_CUSTOMER.EUSER,
                           dbo.BLOCK_CUSTOMER.CRT,
                           dbo.BLOCK_CUSTOMER.UID,
                           dbo.BLOCK_CUSTOMER.ID
                    FROM dbo.BLOCK_CUSTOMER
                        LEFT OUTER JOIN dbo.CUST_HESAB
                            ON dbo.BLOCK_CUSTOMER.HES = dbo.CUST_HESAB.hes 
                            ORDER BY CRT").ToList();

            foreach (var item in RST)
            {
                BLOCK_CUSTOMER_DATA.Add(item);
            }

        }

        #region FastEditableDataGridComboBox
        /// <summary>
        /// Handles the KeyUp event for the ComboBox in edit mode.
        /// Uses asynchronous debouncing to avoid firing a query on every keystroke.
        /// </summary>
        private async void HES_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Ignore navigation and control keys
            if (e.Key == Key.Down || e.Key == Key.Up ||
                e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Escape)
            {
                return;
            }

            if (!(sender is ComboBox comboBox))
                return;

            string typedText = comboBox.Text;

            if (typedText.Length < 2)
            {
                comboBox.ItemsSource = null;
                comboBox.IsDropDownOpen = false;
                return;
            }
            // Manage debouncing: cancel any pending search operation
            if (comboBox.Tag is CancellationTokenSource oldCts)
            {
                oldCts.Cancel();
            }
            var cts = new CancellationTokenSource();
            comboBox.Tag = cts;
            try
            {
                // Wait for 300ms; if the user types again, the token will cancel this delay.
                await Task.Delay(300, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return; // A new keystroke has occurred; do not continue with this query.
            }

            if (comboBox.SelectedValue is not null)
            {
                if ((comboBox.SelectedItem as CUST_HESAB_COMBINED)?.NAME == comboBox.Text)
                {
                    return;
                }
            }

            try
            {
                List<CUST_HESAB_COMBINED>? results = null;

                //SELECT hes, NAME FROM dbo.CUST_HESAB WHERE NAME LIKE N'%{}%'
                // Parameterized SQL to prevent SQL injection.

                {
                    string sql = "SELECT DISTINCT TOP 50 hes, NAME FROM dbo.CUST_HESAB WHERE NAME LIKE @Name";
                    var parameters = new { Name = "%" + typedText + "%" };

                    // Offload the DB query to a background thread.
                    results = await Task.Run(() =>
                        dbms.DoGetDataSQL<CUST_HESAB_COMBINED>(sql, parameters).ToList()
                    );
                }

                //if (results.Count == 0)
                //{
                //    string sql = "SELECT TOP 50 hes, NAME FROM dbo.CUST_HESAB WHERE hes = @hes";
                //    var parameters = new { hes = typedText };

                //    results = await Task.Run(() =>
                //         dbms.DoGetDataSQL<CUST_HESAB_COMBINED>(sql, parameters).ToList()
                //     );
                //}

                // If not canceled in the meantime, update the ComboBox UI.
                if (!cts.Token.IsCancellationRequested)
                {
                    comboBox.ItemsSource = results;
                    comboBox.IsDropDownOpen = results.Any();
                    MoveCaretToEnd(comboBox);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void EditCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            // Update the row with the selected item.
            if (combo.SelectedItem is CUST_HESAB_COMBINED selectedStuf)
            {
                if (combo.DataContext is BLOCK_CUSTOMER blockCust)
                {
                    blockCust.HES = selectedStuf.hes;
                    blockCust.NAME_HES = selectedStuf.NAME;
                }
                else if (combo.DataContext is SALGROUP_MODEL salGrp)
                {
                    salGrp.HES = selectedStuf.hes;
                    salGrp.NAME_HES = selectedStuf.NAME;
                }
            }
        }
        private void EditCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            // Delay focus setting to ensure that the control is ready.
            Dispatcher.BeginInvoke(new Action(() => combo.Focus()), DispatcherPriority.Input);

            if (!(combo.DataContext is BLOCK_CUSTOMER currentRow)) return;

            if (!string.IsNullOrEmpty(currentRow.HES))
            {
                try
                {
                    string sql = "SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE hes = @pCode";
                    var parameters = new { pCode = currentRow.HES };
                    var existingItem = dbms.DoGetDataSQL<CUST_HESAB_COMBINED>(sql, parameters).FirstOrDefault();

                    if (existingItem != null)
                    {
                        // Set the ComboBox to display the existing item.
                        combo.ItemsSource = new List<CUST_HESAB_COMBINED> { existingItem };
                        combo.SelectedItem = existingItem;
                        //combo.Text = existingItem.NAME;
                    }
                    else
                    {
                        combo.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                combo.ItemsSource = null;
            }
        }
        private void MoveCaretToEnd(ComboBox comboBox)
        {
            if (comboBox.IsEditable)
            {
                // Get the internal TextBox from the ComboBox template.
                if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                {
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }
            }
        }

        #endregion

        #endregion


        #region GroupUsers
        public int SALGROUP_INDEX { get; private set; }
        public SALGROUP_MODEL? SALGROUP_WAS_ROW_ITEM { get; set; }
        public SALGROUP_MODEL? CURRENT_ROW_SALGROUP { get; set; }
        public int SALGROUP_INDEX_DEF_INDEX_COL
        {
            get
            {
                if (USER_ENDN_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = USER_ENDN_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "SAL_NAME")?.DisplayIndex;
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

        public bool IsPastingRows { get; private set; }

        private void USER_ENDN_SUB_CANCEL_EDIT(DataGridEditingUnit? editingUnit = null, EventArgs eventArgs = null, bool prevent_rowendedittingfire = true)
        {
            if (eventArgs != null && false) //This lead a serioes problem (new row will gone !)
            {
                if (eventArgs is DataGridCellEditEndingEventArgs cellEditArgs)
                {
                    cellEditArgs.Cancel = true; //the cell edit Cancel
                }
                else if (eventArgs is DataGridRowEditEndingEventArgs rowEditArgs)
                {
                    rowEditArgs.Cancel = true; //the row edit Cancel
                }
            }

            // Unsubscribe from event handlers to prevent re-entry during cancellation
            USER_ENDN_SUB.CellEditEnding -= USER_ENDN_SUB_CellEditEnding;
            USER_ENDN_SUB.RowEditEnding -= USER_ENDN_SUB_RowEditEnding;

            // Perform the cancellation based on the editing unit type
            switch (editingUnit)
            {
                case DataGridEditingUnit.Row:
                    USER_ENDN_SUB.CancelEdit();
                    break;

                case DataGridEditingUnit.Cell:
                    USER_ENDN_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    break;

                default: USER_ENDN_SUB.CancelEdit(); break;
            }

            if (editingUnit == null)
            {
                USER_ENDN_SUB.CancelEdit();
            }

            if (prevent_rowendedittingfire)
            {
                //OPANBACCESS_SUB.CommitEdit();
            }

            // Re-subscribe to event handlers
            USER_ENDN_SUB.RowEditEnding += USER_ENDN_SUB_RowEditEnding;
            USER_ENDN_SUB.CellEditEnding += USER_ENDN_SUB_CellEditEnding;
        }

        private void USER_ENDN_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                DataGridExtension.HandleKeyPress(sender, e, USER_ENDN_SUB);
            }
        }
        private void USER_ENDN_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (!(USER_ENDN_SUB.Items.Count < 1) && !(USER_ENDN_SUB.SelectedItem is null))
                {
                    SALGROUP_INDEX = USER_ENDN_SUB.SelectedIndex;
                }
            }
        }
        private void USER_ENDN_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && USER_ENDN_SUB.SelectedItem is not null)
            {
                if (USER_ENDN_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    SALGROUP_WAS_ROW_ITEM = ((SALGROUP_MODEL)USER_ENDN_SUB.SelectedItem).Clone() as SALGROUP_MODEL;
                }
            }
        }
        private void USER_ENDN_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && USER_ENDN_SUB.SelectedItem != null)
            {
                if (USER_ENDN_SUB.Items.Count > 0)
                    SALGROUP_INDEX = USER_ENDN_SUB.SelectedIndex;

                if (!(e is null) && USER_ENDN_SUB.SelectedItem is not null)
                {
                    if (USER_ENDN_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        SALGROUP_WAS_ROW_ITEM = ((SALGROUP_MODEL)USER_ENDN_SUB.SelectedItem).Clone() as SALGROUP_MODEL;
                    }
                }
            }
        }
        private void USER_ENDN_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            #region REFILL_CURRENTS
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

            string? ENTERED_VALUE_ROW = null;
            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                ENTERED_VALUE_ROW = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();

            ComboBox HES_COMBO = null;
            if (e.EditingElement is ContentPresenter contentPresenter)
            {
                HES_COMBO = contentPresenter.ContentTemplate.FindName("HESEditCombo", contentPresenter) as ComboBox;

                if (HES_COMBO == null)
                {
                    HES_COMBO = contentPresenter.ContentTemplate.FindName("EditCombo", contentPresenter) as ComboBox;
                }

                if (HES_COMBO == null)
                {
                    HES_COMBO = DataGridHelper.FindVisualChild<ComboBox>(contentPresenter);
                }
                if (HES_COMBO != null)
                {
                    ENTERED_VALUE_ROW = HES_COMBO.Text;
                    //ENTERED_VALUE_ROW = HES_COMBO.SelectedValue.ToStringNullSafe();
                }
            }

            CURRENT_ROW_SALGROUP = e.Row.Item as SALGROUP_MODEL;

            #endregion

            //نام حساب
            if (e.Column.SortMemberPath == "NAME_HES" || e.Column.Header.ToString() == "نام حساب")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    USER_ENDN_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    CURRENT_ROW_SALGROUP.HES = SALGROUP_WAS_ROW_ITEM.HES;
                    CURRENT_ROW_SALGROUP.NAME_HES = SALGROUP_WAS_ROW_ITEM.NAME_HES;
                    return;
                }
                else
                {
                    if (HES_COMBO?.SelectedValue is null || HES_COMBO?.SelectedValue != CURRENT_ROW_SALGROUP?.HES) //if is different then
                    {
                        var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HES_COMBO, dbms);
                        if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
                        {
                            CURRENT_ROW_SALGROUP.HES = SALGROUP_WAS_ROW_ITEM.HES;
                            CURRENT_ROW_SALGROUP.NAME_HES = SALGROUP_WAS_ROW_ITEM.NAME_HES;
                            universControl.PopNotifyShowUp($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        }
                        else
                        {
                            CURRENT_ROW_SALGROUP.HES = _SelectedHesab_.hes;
                            CURRENT_ROW_SALGROUP.NAME_HES = _SelectedHesab_.NAME;
                        }
                    }
                }
            }

            if (e.Column.SortMemberPath == "GRSAL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow($"گروه نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    USER_ENDN_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    CURRENT_ROW_SALGROUP.GRSAL = SALGROUP_WAS_ROW_ITEM.GRSAL;
                    return;
                }
                else
                {
                    //GRSAL_BeforeUpdate
                    if (ENTERED_VALUE_ROW == "11") //GRSAL == 11
                    {
                        var rst = dbms.DoGetDataSQL<int?>("SELECT     COUNT(SAL_NAME) AS sn FROM dbo.SALA_DTL WHERE (GRSAL = 11) GROUP BY SAL_NAME").ToList();
                        if (rst.Count > 0)
                        {
                            //#Check Matter
                            //if (Forms["BASEKNOW"]["tindata"] >= rst.Fields["sn"])
                            //{
                            //    DoCmd.OpenForm "mesageform", acNormal,,, acFormReadOnly, acDialog, "خطا:تعداد كاربران تبلت تكميل است نميتوانيد كاربر جديد تعريف كنيد";
                            //    USER_ENDN_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            //    CURRENT_ROW_SALGROUP.GRSAL = SALGROUP_WAS_ROW_ITEM.GRSAL;
                            //    return;
                            //}
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Baseknow.tindata))
                            {
                                universControl.PopNotifyShowUp("خطا:براي استفاده از تبلت لازم است آن را از شركت خريداري نماييد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red, 3);
                                USER_ENDN_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            }
                        }
                    }
                }

            }
        }
        private bool SALGROUP_MODEL_BodyIsValid(SALGROUP_MODEL? _row)
        {
            var ROW = _row;

            var errors = (from object i in BLOCK_CUSTOMER_SUB.ItemsSource
                          let c = BLOCK_CUSTOMER_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(_row?.GRSAL?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "گروه کاربری نمیتواند خالی باشد" });
            }

            if (string.IsNullOrEmpty(_row?.ENABL.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضیعت نمیتواند خالی باشد" });
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
        private void USER_ENDN_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var ROW = e.Row.Item as SALGROUP_MODEL;
            if (e.Row.Item == null || ROW is null)
            {
                return;
            }

            if (!SALGROUP_MODEL_BodyIsValid(ROW))
            {
                USER_ENDN_SUB_CANCEL_EDIT();
                return;
            }


            var OldRow = ((SALGROUP_MODEL)SALGROUP_WAS_ROW_ITEM).Clone() as SALGROUP_MODEL; OldRow.EMZA = null;
            var NewRow = ((SALGROUP_MODEL)ROW).Clone() as SALGROUP_MODEL; NewRow.EMZA = null;
            _ = AuditLogger.LogActionAsync(
                              actionType: "SaveRow",
                              tableName: "تعیین سطح دسترسی : گروه کاربری",
                              recordId: ROW?.SAL_NAME,
                              oldValue: OldRow,
                              newValue: NewRow,
                              additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            try
            {
                if (ROW?.IDD is not null) //Update
                {
                    string sql = @"
                        UPDATE [dbo].[SALA_DTL]
                        SET [GRSAL] = @GRSAL,
                            [ENABL] = @ENABL,
                            [HES] = @HES,
                            [PORID] = @PORID,
                            [EMZA] = @EMZA,
                            [menup] = @menup,
                            [DEFAULT_NAHVA] = @DEFAULT_NAHVA,
                            [erjabe] = @erjabe
                        WHERE [IDD] = @IDD";

                    var parameters = new
                    {
                        GRSAL = ROW?.GRSAL,
                        ENABL = ROW?.ENABL ?? 1,
                        HES = ROW?.HES,
                        PORID = ROW?.PORID,
                        EMZA = ROW?.EMZA,
                        menup = ROW?.menup,
                        erjabe = ROW?.erjabe,
                        DEFAULT_NAHVA = ROW?.DEFAULT_NAHVA,
                        IDD = ROW?.IDD
                    };
                    dbms.DoExecuteSQL(sql, parameters);


                    if (ROW?.DEFAULT_SHIFT != null || ROW?.DEFAULT_TFSAZMAN != null)
                    {
                        string sql2 = @"IF EXISTS (SELECT 1 FROM DEFAULTDEP WHERE USERID = @USERID)
                                            UPDATE DEFAULTDEP SET SHIFT = @SHIFT, TFSAZMAN = @TFSAZMAN WHERE USERID = @USERID;
                                        ELSE
                                           INSERT INTO DEFAULTDEP (USERID, SHIFT, TFSAZMAN) VALUES (@USERID, @SHIFT, @TFSAZMAN);";

                        var param = new
                        {
                            USERID = ROW.IDD,
                            SHIFT = ROW.DEFAULT_SHIFT,
                            TFSAZMAN = ROW.DEFAULT_TFSAZMAN
                        };

                        dbms.DoExecuteSQL(sql2, param);
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این سطر حاوی اطلاعات تکراری است و نمیتواند ذخیره کرد").ShowDialog();
                }
                else
                {
                    throw;
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog(); return;
            }
        }

        #region HesGrpEditableDataGridComboBox
        private async void HESGRP_ComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Ignore navigation and control keys
            if (e.Key == Key.Down || e.Key == Key.Up ||
                e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Escape)
            {
                return;
            }

            if (!(sender is ComboBox comboBox))
                return;

            string typedText = comboBox.Text;

            if (typedText.Length < 2)
            {
                comboBox.ItemsSource = null;
                comboBox.IsDropDownOpen = false;
                return;
            }
            // Manage debouncing: cancel any pending search operation
            if (comboBox.Tag is CancellationTokenSource oldCts)
            {
                oldCts.Cancel();
            }
            var cts = new CancellationTokenSource();
            comboBox.Tag = cts;
            try
            {
                // Wait for 300ms; if the user types again, the token will cancel this delay.
                await Task.Delay(500, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return; // A new keystroke has occurred; do not continue with this query.
            }

            if (comboBox.SelectedValue is not null)
            {
                if ((comboBox.SelectedItem as CUST_HESAB_COMBINED)?.NAME == comboBox.Text)
                {
                    return;
                }
            }

            try
            {
                List<CUST_HESAB_COMBINED>? results = null;

                {
                    string sql = "SELECT DISTINCT TOP 50 hes, NAME FROM dbo.CUST_HESAB WHERE NAME LIKE @Name";
                    var parameters = new { Name = "%" + typedText + "%" };

                    // Offload the DB query to a background thread.
                    results = await Task.Run(() =>
                        dbms.DoGetDataSQL<CUST_HESAB_COMBINED>(sql, parameters).ToList()
                    );
                }

                // If not canceled in the meantime, update the ComboBox UI.
                if (!cts.Token.IsCancellationRequested)
                {
                    comboBox.ItemsSource = results;
                    comboBox.IsDropDownOpen = results.Any();
                    MoveCaretToEnd(comboBox);
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void HESEditCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            // Update the row with the selected item.
            if (combo.SelectedItem is CUST_HESAB_COMBINED selectedStuf)
            {
                // CODE is already bound via SelectedValue.
                if (combo.DataContext is BLOCK_CUSTOMER blockCust)
                {
                    blockCust.HES = selectedStuf.hes;
                    blockCust.NAME_HES = selectedStuf.NAME;

                    //// Force binding refresh
                    //var bindingExpression = combo.GetBindingExpression(ComboBox.SelectedValueProperty);
                    //bindingExpression?.UpdateSource();
                }
                else if (combo.DataContext is SALGROUP_MODEL salGrp)
                {
                    salGrp.HES = selectedStuf.hes;
                    salGrp.NAME_HES = selectedStuf.NAME;

                    // Force binding refresh
                    var bindingExpression = combo.GetBindingExpression(ComboBox.SelectedValueProperty);
                    bindingExpression?.UpdateSource();
                }
                //else if (!string.IsNullOrEmpty(combo.Text))
                //{
                //    // Fallback: user typed a code directly
                //    currentRow.HES = combo.Text.Trim();
                //    currentRow.NAME_HES = GetNameHes(combo.Text.Trim());
                //}
            }
        }
        private void HESEditCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            // Delay focus setting to ensure that the control is ready.
            Dispatcher.BeginInvoke(new Action(() => combo.Focus()), DispatcherPriority.Input);

            string? hesCode = null;
            if (combo.DataContext is BLOCK_CUSTOMER blockCust)
            {
                hesCode = blockCust.HES;
            }
            else if (combo.DataContext is SALGROUP_MODEL salGrp)
            {
                hesCode = salGrp.HES;
            }
            else
            {
                return;
            }

            if (!string.IsNullOrEmpty(hesCode))
            {
                try
                {
                    string sql = "SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE hes = @pCode";
                    var parameters = new { pCode = hesCode };
                    var existingItem = dbms.DoGetDataSQL<CUST_HESAB_COMBINED>(sql, parameters).FirstOrDefault();

                    if (existingItem != null)
                    {
                        // Set the ComboBox to display the existing item.
                        combo.ItemsSource = new List<CUST_HESAB_COMBINED> { existingItem };
                        combo.SelectedItem = existingItem;
                    }
                    else
                    {
                        combo.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                combo.ItemsSource = null;
            }
        }

        #endregion

        private void USER_ENDN_ReGetData()
        {
            var RST = dbms.DoGetDataSQL<SALGROUP_MODEL>($@"SELECT  dbo.SALA_DTL.SAL_NAME, dbo.SALA_DTL.PSAL_NAME, dbo.SALA_DTL.GRSAL,
                                                           dbo.SALA_DTL.ENABL, dbo.SALA_DTL.IDD, dbo.SALA_DTL.HES, dbo.CUST_HESAB.NAME AS NAME_HES, 
                                                           dbo.SALA_DTL.PORID, dbo.SALA_DTL.EMZA, dbo.SALA_DTL.menup, dbo.SALA_DTL.erjabe
                                                           FROM dbo.SALA_DTL LEFT OUTER JOIN
                                                           dbo.CUST_HESAB ON dbo.SALA_DTL.HES = dbo.CUST_HESAB.hes
                                                           ORDER BY dbo.SALA_DTL.IDD").ToList();
            foreach (var item in RST)
            {
                if (!string.IsNullOrEmpty(item?.SAL_NAME))
                {
                    item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME);
                }

                SALGROUP_DATA?.Add(item);
            }

            //بروز رسانی شیفت و احد های پیش فرض
            var defaultDeps = dbms.DoGetDataSQL<DEFAULTDEP>("SELECT USERID, SHIFT, TFSAZMAN FROM DEFAULTDEP").ToDictionary(d => d.USERID, d => d);
            foreach (var user in SALGROUP_DATA)
            {
                if (user.IDD != null)
                {
                    if (defaultDeps.TryGetValue((int)user.IDD, out var def))
                    {
                        user.DEFAULT_SHIFT = def.SHIFT;
                        user.DEFAULT_TFSAZMAN = def.TFSAZMAN;
                    }
                }
            }
        }
        private void SIGN_COLUMN_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.Tag != null)) return;

            var CurrentRow = btn.Tag as SALGROUP_MODEL;
            if (CurrentRow != null)
            {
                new WIN_SIGN_CAPTURE(CurrentRow).ShowDialog();
            }

        }





        #endregion

        private void BLOCKED_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
                    var isEditing = ((IEditableCollectionView)dataGrid.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }
        private void UNBLOCKED_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
                    var isEditing = ((IEditableCollectionView)dataGrid.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }
        private void CHARTSAZMANI_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
                    var isEditing = ((IEditableCollectionView)dataGrid.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        //کلید انبار
        private void OPANBACCESS_SUB_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }
        private void OPANBACCESS_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
                    var isEditing = ((IEditableCollectionView)dataGrid.Items).IsEditingItem;
                    dataGrid.ContextMenu.IsOpen = true;
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            DataGrid? DG_SUB = null;

            if (OPANBACCESS_SUB.IsKeyboardFocusWithin) //کلید انبار
            {
                DG_SUB = OPANBACCESS_SUB;
                if (OPANBACCESS_SUB.Items.Count == 0)
                {
                    return;
                }
            }
            else if (BLOCKED_SUB.IsKeyboardFocusWithin) //مسدودی حساب
            {
                DG_SUB = BLOCKED_SUB;
                if (BLOCKED_SUB.Items.Count == 0)
                {
                    return;
                }
            }
            else if (UNBLOCKED_SUB.IsKeyboardFocusWithin) //مجاز بودن حساب
            {
                DG_SUB = UNBLOCKED_SUB;
                if (UNBLOCKED_SUB.Items.Count == 0)
                {
                    return;
                }
            }
            else if (CHARTSAZMANI_SUB.IsKeyboardFocusWithin) //زیر مجموعه
            {
                DG_SUB = CHARTSAZMANI_SUB;
                if (CHARTSAZMANI_SUB.Items.Count == 0)
                {
                    return;
                }
            }


            if (DG_SUB == null) { return; }
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

        private void COPY_CLICK(object sender, RoutedEventArgs e)
        {
            DataGrid? DG_SUB = null;
            if (OPANBACCESS_SUB.IsKeyboardFocusWithin) //کلید انبار
            {
                DG_SUB = OPANBACCESS_SUB;
            }
            else if (BLOCKED_SUB.IsKeyboardFocusWithin) //مسدودی حساب
            {
                DG_SUB = BLOCKED_SUB;
            }
            else if (UNBLOCKED_SUB.IsKeyboardFocusWithin) //مجاز بودن حساب
            {
                DG_SUB = UNBLOCKED_SUB;
            }
            else if (CHARTSAZMANI_SUB.IsKeyboardFocusWithin) //زیر مجموعه
            {
                DG_SUB = CHARTSAZMANI_SUB;
            }

            var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;

            if (!isEditing)
            {
                e.Handled = true;
                if (OPANBACCESS_SUB.IsKeyboardFocusWithin) //کلید انبار
                {
                    DataGridClipboardManager.CopySelectedItems<OPANBACCESS>(DG_SUB);
                }
                else if (BLOCKED_SUB.IsKeyboardFocusWithin) //مسدودی حساب
                {
                    DataGridClipboardManager.CopySelectedItems<BLOCK_HES>(DG_SUB);
                }
                else if (UNBLOCKED_SUB.IsKeyboardFocusWithin) //مجاز بودن حساب
                {
                    DataGridClipboardManager.CopySelectedItems<BLOCKNON_HES>(DG_SUB);
                }
                else if (CHARTSAZMANI_SUB.IsKeyboardFocusWithin) //زیر مجموعه
                {
                    DataGridClipboardManager.CopySelectedItems<CHARTSAZMANI>(DG_SUB);
                }
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
            DataGrid? DG_SUB = null;
            if (OPANBACCESS_SUB.IsKeyboardFocusWithin) //کلید انبار
            {
                DG_SUB = OPANBACCESS_SUB;
            }
            else if (BLOCKED_SUB.IsKeyboardFocusWithin) //مسدودی حساب
            {
                DG_SUB = BLOCKED_SUB;
            }
            else if (UNBLOCKED_SUB.IsKeyboardFocusWithin) //مجاز بودن حساب
            {
                DG_SUB = UNBLOCKED_SUB;
            }
            else if (CHARTSAZMANI_SUB.IsKeyboardFocusWithin) //زیر مجموعه
            {
                DG_SUB = CHARTSAZMANI_SUB;
            }

            if (DG_SUB.SelectedItem != null || DG_SUB.SelectedItems.Count > 0)
            {
                var isEditing = ((IEditableCollectionView)DG_SUB.Items).IsEditingItem;
                if (!isEditing && !DG_SUB.IsReadOnly && DG_SUB.IsEnabled)
                {
                    e.Handled = true;

                    IsPastingRows = true;

                    if (OPANBACCESS_SUB.IsKeyboardFocusWithin) //کلید انبار
                    {
                        DataGridClipboardManager.PasteItems<OPANBACCESS>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    }
                    else if (BLOCKED_SUB.IsKeyboardFocusWithin) //مسدودی حساب
                    {
                        DataGridClipboardManager.PasteItems<BLOCK_HES>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    }
                    else if (UNBLOCKED_SUB.IsKeyboardFocusWithin) //مجاز بودن حساب
                    {
                        DataGridClipboardManager.PasteItems<BLOCKNON_HES>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    }
                    else if (CHARTSAZMANI_SUB.IsKeyboardFocusWithin) //زیر مجموعه
                    {
                        DataGridClipboardManager.PasteItems<CHARTSAZMANI>(DG_SUB, ValidateDataGridRow, AddItemToDataSource);
                    }

                    IsPastingRows = false;

                    DG_SUB.CommitEdit();
                }
                else
                {
                    //System.Windows.Forms.SendKeys.SendWait("^v");
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

        private void ValidateDataGridRow(DataGridRowEditEndingEventArgs args, PasteValidationResult validationResult)
        {
            validationResult.IsRowValid = true; // Default to true

            if (args.Row.Item is OPANBACCESS item) //کلید انبار
            {
                item.RDF = null; //Reset id to be sure the new data will insert not update the same row existing before
                item.UID = null;
                item.USERCO = null;
                if (BodyIsValid(item))
                {
                    OPANBACCESS_SUB_RowEditEnding(OPANBACCESS_SUB, args);
                    validationResult.IsRowValid = OPANBACCESS_IsSaveSuccess;
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                }
            }
            else if (args.Row.Item is BLOCK_HES BLC) //مسدودی حساب
            {
                BLC.UID = null;
                BLC.USERCO = null;
                if (BlockyBodyIsValid(BLC))
                {
                    BLOCKED_SUB_RowEditEnding(BLOCKED_SUB, args);
                    validationResult.IsRowValid = BLOCKED_IsSaveSuccess;
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                }
            }
            else if (args.Row.Item is BLOCKNON_HES NONBLC) //مجاز بودن حساب
            {
                NONBLC.UID = null;
                NONBLC.USERCO = null;
                if (UNBLOCKED_BodyIsValid(NONBLC))
                {
                    UNBLOCKED_SUB_RowEditEnding(UNBLOCKED_SUB, args);
                    validationResult.IsRowValid = UNBLOCKED_IsSaveSuccess;
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                }
            }
            else if (args.Row.Item is CHARTSAZMANI CHARTITEM) //زیر مجموعه
            {
                CHARTITEM.RDF = null;
                CHARTITEM.UID = null;
                if (CHARTSAZMANI_BodyIsValid(CHARTITEM))
                {
                    CHARTSAZMANI_SUB_RowEditEnding(CHARTSAZMANI_SUB, args);
                    validationResult.IsRowValid = CHARTSAZMANI_IsSaveSuccess;
                }
                else
                {
                    args.Cancel = true;
                    validationResult.IsRowValid = false;
                }
            }
            else
            {
                args.Cancel = true;
                validationResult.IsRowValid = false;
            }
        }
        private void AddItemToDataSource(object inputitem)
        {
            if (inputitem is OPANBACCESS item) //کلید انبار
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OPANBACCESS_DATA.Add(item);
                });
            }
            else if (inputitem is BLOCK_HES item1) //مسدودی حساب
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BLOCK_HES_DATA.Add(item1);
                });
            }
            else if (inputitem is BLOCKNON_HES item2) //مجاز بودن حساب
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BLOCKNON_HES_DATA.Add(item2);
                });
            }
            else if (inputitem is CHARTSAZMANI item3) //زیر مجموعه کاربران
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CHARTSAZMANI_DATA.Add(item3);
                });
            }
        }
    }
}
