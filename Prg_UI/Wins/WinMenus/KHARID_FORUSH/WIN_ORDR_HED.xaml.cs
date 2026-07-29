using DocumentFormat.OpenXml.Bibliography;
using Functions;
using Interfaces;
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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_ORDR_HED : Window, ISearchableWindow
    {
        public WIN_ORDR_HED(double? number_to_open = null, bool _isAutomasion_ = false)
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

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool IsOpenedFromAutomation { get; } = false;

        InventoryManager IVM = new InventoryManager();

        public ObservableCollection<ORDR_LST> ORDR_LST_DATA { get; } = new ObservableCollection<ORDR_LST>();

        public bool NowIsReady { get; private set; }
        public bool ORDER_LST_SUB_IsFocused { get; private set; }

        public double? NUMBER_TO_OPEN { get; set; }
        private int? OriginalContractID { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO => GetSgnInfo(SGN1usid, "SEFARESH_SGN1");

        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO => GetSgnInfo(SGN2usid, "SEFARESH_SGN2");

        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO => GetSgnInfo(SGN3usid, "SEFARESH_SGN3");

        private SGN_IMODEL GetSgnInfo(TextBox sgnUsid, string sematKey)
        {
            var info = new SGN_IMODEL();
            if (sgnUsid.Tag is not null)
            {
                info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgnUsid.Tag), sematKey);
                info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgnUsid.Tag)));
            }
            return info;
        }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;

        private int datagridname_tbox_def_index_col;
        public int ORDER_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (ORDER_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = ORDER_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NAME_CODE")?.DisplayIndex;
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
        public ORDR_LST? CURRENT_ITEMS_ROW { get; private set; }
        public ORDR_LST? WAS_ROW_ITEM { get; private set; } = new ORDR_LST();

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
                CUST_NO.IsEnabled = ican;

                DATE_N.IsReadOnly = !ican;
                MOLAH.IsReadOnly = !ican;
            }
        }

        private NavigationManager<ORDR_HED> _navigationManager;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            #region SecuritCheck
            try
            {
                string Formname = "SEFARESH";
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                CL_HESABDARI.SETSECURITYSUB(ORDER_LST_SUB, "SEFARESH");
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            if (!this.IsLoaded) { this.Close(); return; }
            #endregion

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            FILL_ALL_COMBOBOXES();

            ApplySignatureSettings();

            string WhereCondition = "";
            if (IsOpenedFromAutomation)
            {
                WhereCondition = $" WHERE id = {NUMBER.Text}";
            }

            _navigationManager = new NavigationManager<ORDR_HED>(
                dbms,
                x => x.id.ToString(),
                $"SELECT * FROM ORDR_HED {WhereCondition} ORDER BY id",
                x => $"SELECT * FROM ORDR_HED WHERE id = {x?.id}",
                Convert.ToDouble(NUMBER.Text)
            );

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            Form_Current();

            CL_LMethods.SetTabIndexes(DATE_N, CUST_NO, MOLAH, ContractID, BTN_SAVE, ORDER_LST_SUB);

            MakeDefaultFocuseReady();
        }
        private bool OnInsertRecord(ORDR_HED record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<ORDR_HED>($"SELECT TOP 1 * FROM ORDR_HED  WHERE id = {NUMBER.Text}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(ORDR_HED HEADER)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
            }
            else if (HEADER == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                _navigationManager.IsNewRecord = false;
                NUMBER.Text = HEADER.id.ToString();
                NUMBER.Tag = HEADER.id.ToString();

                DATE_N.Text = HEADER.DATE.ToStringNullSafe();
                USER_NAME.Text = HEADER.USER_NAME.ToStringNullSafe();

                string thevalue = HEADER.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE hes = @hes", new { hes = thevalue }).FirstOrDefault();
                if (data != null)
                {
                    if (CUST_NO.ItemsSource == null) CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                    var list = (List<Custom_CUST_HESAB>)CUST_NO.ItemsSource;
                    if (!list.Any(item => item?.hes == thevalue))
                    {
                        list.Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = HEADER.CUST_NO;
                    CUST_NO.Items.Refresh();
                }
                else if (!string.IsNullOrEmpty(HEADER.CUST_NO))
                {
                    CUST_NO.SelectedValue = HEADER.CUST_NO;
                }

                MOLAH.Text = HEADER.MOLAH;
                OriginalContractID = HEADER.ContractID;
                ContractID.SelectedValue = HEADER.ContractID;

                SGN1.IsChecked = HEADER.SGN1;
                SGN2.IsChecked = HEADER.SGN2;
                SGN3.IsChecked = HEADER.SGN3;

                SGN1usid.Tag = HEADER.sgn1usid;
                SGN2usid.Tag = HEADER.sgn2usid;
                SGN3usid.Tag = HEADER.sgn3usid;

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null;
                PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                BTN_SAVE.IsEnabled = false;
                ItwasNewFirstTime = false;

                UpdatePrintButtonsState();
                ORDER_LST_SUB_ReGetData();
                Form_Current();

            }
        }
        private void ClearFreshAll()
        {
            NUMBER.Text = "0";
            NUMBER.Tag = null;
            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = Baseknow.UUSER;
            CUST_NO.SelectedIndex = -1;
            CUST_NO.Items.Refresh();
            MOLAH.Text = null;
            OriginalContractID = null;
            ContractID.SelectedValue = null;

            ORDR_LST_DATA?.Clear();

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedValue = null;
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            SGN1.IsChecked = false; SGN1usid.Text = ""; SGN1usid.Tag = null;
            SGN2.IsChecked = false; SGN2usid.Text = ""; SGN2usid.Tag = null;
            SGN3.IsChecked = false; SGN3usid.Text = ""; SGN3usid.Tag = null;

            AllowEdits = true;
            ORDER_LST_SUB.IsReadOnly = true;
            UpdatePrintButtonsState();
            MakeDefaultFocuseReady();
            Form_Current();
        }
        private void MakeDefaultFocuseReady()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = ORDER_LST_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (ORDER_LST_SUB_IsFocused)
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

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[1]);

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

            if (!ORDER_LST_SUB.IsKeyboardFocusWithin && !ORDER_LST_SUB.IsFocused)
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
                    DataGridExtension.HandleKeyPress(sender, e, ORDER_LST_SUB);
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

        #region ISearchableWindow
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            if (selectedItem is ORDR_HED item && item != null)
            {
                var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.id == item.id);
                if (itemfound != null)
                {
                    _navigationManager.IsNewRecord = false;
                    int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                    if (idx >= 0) _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                }
            }
        }

        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره سفارش", PropertyPath = "id", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "سفارش دهنده", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
            };
        }
        #endregion

        private List<COMBOPERSONEL> rst_personel;
        private void FILL_ALL_COMBOBOXES()
        {
            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            var contracts = dbms.DoGetDataSQL<ContractOrderLookup>(@"
SELECT ContractID, IsClosed,
       DisplayName = CONCAT(ContractNo, N' - ', BrandName, CASE WHEN IsClosed=1 THEN N' (مختومه)' ELSE N'' END)
FROM dbo.CONTRACT_HED ORDER BY IsClosed, ContractDate DESC, ContractID DESC").ToList();
            contracts.Insert(0, new ContractOrderLookup { ContractID = null, DisplayName = "بدون قرارداد" });
            ContractID.ItemsSource = contracts;
            ContractID_COLUMN.ItemsSource = contracts;

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
        }
        private void ApplySignatureSettings()
        {
            // Logic converted from Form_Open
            // Assumes Baseknow.SIGN is a boolean property available globally
            if ((bool)Baseknow.SIGN)
            {
                // Make Signature Controls Visible
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
                SGN3.Visibility = Visibility.Visible;

                SGN1usid.Visibility = Visibility.Visible;
                SGN2usid.Visibility = Visibility.Visible;
                SGN3usid.Visibility = Visibility.Visible;

                UpdatePrintButtonsState();
            }
            else
            {
                // Optional: If SIGN is false, you might want to hide them or keep them hidden
                // Based on VBA, we strictly follow the 'If True' block. 
                // Default visibility in XAML should likely be Collapsed if strict adherence is needed,
                // otherwise set them to Collapsed here if needed.
            }
        }

        private void UpdatePrintButtonsState()
        {
            if ((bool)!Baseknow.SIGN) return;

            bool isAnySigned = (SGN1.IsChecked == true) ||
                               (SGN2.IsChecked == true) ||
                               (SGN3.IsChecked == true);

            if (isAnySigned)
            {
                BTN_PRINT_TOLID.IsEnabled = true;  // Command12
                BTN_PRINT.IsEnabled = true;  // Command100
                //BTN_C.IsEnabled = true;            // Command13
            }
            else
            {
                BTN_PRINT_TOLID.IsEnabled = false;
                BTN_PRINT.IsEnabled = false;
                //BTN_C.IsEnabled = false;
            }
        }
        public bool ItwasNewFirstTime { get; set; } = false;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            int? selectedContractID = ContractID.SelectedValue is int value ? value : null;
            if (!BTN_SAVE.IsEnabled) return;
            if (ContractID.SelectedItem is ContractOrderLookup selectedContract &&
                selectedContract.IsClosed && selectedContractID != OriginalContractID)
            {
                new Msgwin(false, "نمی‌توان سفارش جدیدی به قرارداد مختومه متصل کرد.").ShowDialog();
                return;
            }

            var errors = (from object i in ORDER_LST_SUB.ItemsSource
                          let c = ORDER_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // Validations
            if (!Tarikh.IsValidedDate(DATE_N.Text.ToRawTarikh()))
            {
                new Msgwin(false, "تاریخ صحیح نمی باشد").ShowDialog();
            }
            else
            {
                if (!Tarikh.IsSyncedDateNow(DATE_N.Text, (bool)Baseknow.CTL_DT))
                {
                    new Msgwin(false, "تاریخ مربوط به سال جاری نیست").ShowDialog();
                }
            }

            if (CUST_NO.SelectedValue == null && string.IsNullOrEmpty(CUST_NO.Text))
            {
                new Msgwin(false, "سفارش دهنده مشخص نشده است").ShowDialog();
                return;
            }
            if (CUST_NO.SelectedValue != null && CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
            {
                new Msgwin(false, "حساب مشتری مسدود است").ShowDialog();
                return;
            }


            if (string.IsNullOrWhiteSpace(NUMBER.Text) || NUMBER.Text == "0")
            {
                try
                {
                    const string insertHeaderSql = @"
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
DECLARE @NewID INT;
SELECT @NewID = ISNULL(MAX(id), 0) + 1 FROM dbo.ORDR_HED WITH (TABLOCKX, HOLDLOCK);
INSERT dbo.ORDR_HED (id, DATE, MOLAH, CUST_NO, USER_NAME, SGN1, SGN2, SGN3, ContractID)
VALUES (@NewID, @date, @molah, @custNo, @user, 0, 0, 0, @contractID);
COMMIT TRANSACTION;
SELECT @NewID;";
                    int newId = dbms.DoGetDataSQL<int>(insertHeaderSql, new
                    {
                        date = DATE_N.Text.ToRawTarikh(),
                        molah = MOLAH.Text,
                        custNo = CUST_NO.SelectedValue ?? CUST_NO.Text,
                        user = USER_NAME.Text,
                        contractID = selectedContractID
                    }).Single();
                    NUMBER.Text = newId.ToString();
                    ItwasNewFirstTime = true;
                    _navigationManager.IsNewRecord = false;
                    RefreshAfterUpdate();
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در ایجاد سفارش: " + ex.Message).ShowDialog();
                    return;
                }
            }

            // Save Header Update
            try
            {
                string qry = "UPDATE ORDR_HED SET DATE = @date, MOLAH = @molah, CUST_NO = @custNo, USER_NAME = @user, ContractID = @contractID WHERE id = @id";
                dbms.DoExecuteSQL(qry, new { id = NUMBER.Text, date = DATE_N.Text.ToRawTarikh(), molah = MOLAH.Text, custNo = CUST_NO.SelectedValue ?? CUST_NO.Text, user = USER_NAME.Text, contractID = selectedContractID });
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره هدر: " + ex.Message).ShowDialog();
                return;
            }

            ORDER_LST_SUB.IsReadOnly = false;

            if (ORDR_LST_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            universControl.PopNotifyShow("اطلاعات ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

        }

        private void RefreshAfterUpdate()
        {
            _navigationManager.IsNewRecord = false;
            var header = dbms.DoGetDataSQL<ORDR_HED>($"SELECT * FROM ORDR_HED WHERE id = {NUMBER.Text}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(header);
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (NUMBER.Text == "0") return;

            // Check Signatures
            if ((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false))
            {
                new Msgwin(false, "اول امضا را بردارید").ShowDialog();
                ORDER_LST_SUB.IsReadOnly = true;
                AllowEdits = false;
                AllowDeletions = false;
            }
            else
            {
                CL_HESABDARI.TR("ORDR_HED", $"(ID = {NUMBER.Text})", DateTime.Now, 1);
                CL_HESABDARI.TR("ORDR_LST", $"(ID = {NUMBER.Text})", DateTime.Now, 1);

                ORDER_LST_SUB.IsReadOnly = false;
                AllowEdits = true;
                AllowDeletions = true;
            }
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible || !AllowEdits) { return; }
            if (!BTN_DELETE.IsEnabled || _navigationManager.IsNewRecord) return;

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "ثبت سفارشات کالا",
                    recordId: NUMBER.Text,
                    oldValue: $"",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (ORDR_LST_DATA.Count > 0 && ORDER_LST_SUB.SelectedItems != null && ORDER_LST_SUB.SelectedItems.Count > 0)
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("ORDR_HED", $"(ID = {NUMBER.Text})", dt, 1);
                    CL_HESABDARI.TR("ORDR_LST", $"(ID = {NUMBER.Text})", dt, 1);
                    #endregion

                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < ORDER_LST_SUB.SelectedItems.Count; i++)
                    {
                        var item = ORDER_LST_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(ORDER_LST_SUB, item))
                        {
                            continue; // Skip deletion for new placeholder items
                        }

                        var _idd_ = item.GetType().GetProperty("idd").GetValue(item);

                        if (_idd_ != null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($"DELETE FROM ORDR_LST WHERE idd = {_idd_}");

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

                    ORDER_LST_SUB_ReGetData();
                }
                else
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($"DELETE FROM ORDR_HED WHERE id = {NUMBER.Text}");
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
                        ORDER_LST_SUB_ReGetData();
                    }
                }
            }
        }
        private void GetFocusOnDefaultCell()
        {
            var DG = ORDER_LST_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "NAME_CODE", DEFINDX, true);
        }
        public void ORDER_LST_SUB_ReGetData()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                ORDR_LST_DATA.Clear();
                return;
            }

            string sql = @"SELECT L.*, S.NAME AS NAME_CODE 
                           FROM ORDR_LST L 
                           LEFT JOIN STUF_DEF S ON L.CODE = S.CODE 
                           WHERE L.ID = @id";
            var list = dbms.DoGetDataSQL<ORDR_LST>(sql, new { id = int.Parse(NUMBER.Text) }).ToList();

            ORDR_LST_DATA.Clear();
            foreach (var item in list) ORDR_LST_DATA.Add(item);
        }

        private void Form_Current()
        {
            if (NUMBER.Text == "0")
            {
                ORDER_LST_SUB.IsReadOnly = true;
            }
            else
            {
                ORDER_LST_SUB.IsReadOnly = true;
                if (ItwasNewFirstTime) ORDER_LST_SUB.IsReadOnly = false;
            }

            if ((SGN1.IsChecked ?? false) || (SGN2.IsChecked ?? false) || (SGN3.IsChecked ?? false))
            {
                ORDER_LST_SUB.IsReadOnly = true;
                AllowEdits = false;
            }
            else if (!_navigationManager.IsNewRecord && !ItwasNewFirstTime)
            {
                ORDER_LST_SUB.IsReadOnly = true;
                AllowEdits = false;
            }
        }


        private void UpdateMeghK(ORDR_LST row)
        {
            if (row.VAHED_K != null && !string.IsNullOrEmpty(row.CODE))
            {
                var ratio = dbms.DoGetDataSQL<double?>($"SELECT NESBAT FROM VAHEDS WHERE CODE='{row.CODE}' AND VAHED={row.VAHED_K}").FirstOrDefault();
                if (ratio != null)
                {
                    row.MEGHK = row.MEGH * ratio.Value;
                }
            }
        }


        // Boilerplate handlers
        private void ORDER_LST_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ORDER_LST_SUB_IsFocused = (bool)e.NewValue;
        }
        private void ORDER_LST_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { /* Context menu logic */ }
        private void ORDER_LST_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
        private void ORDER_LST_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void ORDER_LST_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
        }
        private void ORDER_LST_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            ORDER_LST_SUB.Dispatcher.InvokeAsync(() =>
            {
                ORDER_LST_SUB.CellEditEnding -= ORDER_LST_SUB_CellEditEnding;
                ORDER_LST_SUB.RowEditEnding -= ORDER_LST_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    ORDER_LST_SUB.CancelEdit();
                }
                else
                {
                    ORDER_LST_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                ORDER_LST_SUB.RowEditEnding += ORDER_LST_SUB_RowEditEnding;
                ORDER_LST_SUB.CellEditEnding += ORDER_LST_SUB_CellEditEnding;
            });
        }

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        private void ORDER_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var row = e.Row.Item as ORDR_LST;
            if (row == null) return;

            // Populate Units Dropdown
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                var cb = e.EditingElement as ComboBox;
                if (cb != null)
                {
                    var units = dbms.DoGetDataSQL<Custom_VAHEDK>($"SELECT V.CODE as VAHED, T.NAMES FROM VAHEDS V JOIN TCOD_VAHEDS T ON V.VAHED=T.CODE WHERE V.CODE='{row.CODE}'").ToList();
                    cb.ItemsSource = units;
                }
            }

            var CurrentRow = e.Row.Item as ORDR_LST;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || string.IsNullOrEmpty(CurrentRow?.CODE))
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
        private void ORDER_LST_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && ORDER_LST_SUB.SelectedItem is not null)
            {
                if (ORDER_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((ORDR_LST)ORDER_LST_SUB.SelectedItem).Clone() as ORDR_LST;
                    if (ORDER_LST_SUB.SelectedItem is ORDR_LST row && row.idd <= 0 &&
                        row.ContractID is null && ContractID.SelectedValue is int defaultContractID)
                        row.ContractID = defaultContractID;
                }
            }
        }
        private void ORDER_LST_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            var row = e.Row.Item as ORDR_LST;
            if (row == null) return;

            ComboBox Comboval = e.EditingElement as ComboBox;
            TextBox TexboVal = e.EditingElement as TextBox;
            string enteredVal = Comboval?.SelectedValue?.ToString() ?? TexboVal?.Text;

            #region REFILL_CURRENTS
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
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

            CURRENT_ITEMS_ROW = row;
            #endregion

            // CODE change
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                // Logic to update CODE, Name, Unit
                if (string.IsNullOrEmpty(enteredVal)) return;

                if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.NAME_CODE.ToStringNullSafe().Trim() ||
                  string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM?.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM?.NAME_CODE;
                        return;
                    }

                    // Search product logic similar to HAVALAH_EXIT
                    var rstKala = CL_LMethods.GetKalaBySearch(dbms, default, enteredVal);
                    if (rstKala != null)
                    {
                        row.CODE = rstKala.CODE;
                        row.NAME_CODE = rstKala.NAME_CODE;
                        row.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, row.CODE);

                        // Get MOGU (Inventory)
                        var mogu = dbms.DoGetDataSQL<double?>($"SELECT MANDAH FROM MOGUDI_KOL_ANBARHA WHERE CODE = N'{row.CODE}'").FirstOrDefault();
                        MOGU.Text = "موجودی: " + (mogu ?? 0).ToString();

                        // Copy Description if needed and not already set
                        if (!string.IsNullOrEmpty(rstKala.MANDAH))
                        {
                            row.CUST_NO = rstKala.MANDAH;
                        }
                    }
                    else
                    {
                        new Msgwin(false, "کالا یافت نشد").ShowDialog();
                    }
                }

            }

            // MEGH Change
            if (e.Column.SortMemberPath == "MEGH")
            {
                // Calculate MEGHK
                if (double.TryParse(enteredVal, out double megh))
                {
                    row.MEGH = megh;
                    UpdateMeghK(row);
                }
            }

            // VAHED_K Change
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                // Update Ratio
                if (int.TryParse(enteredVal, out int vahedK))
                {
                    row.VAHED_K = vahedK;
                    UpdateMeghK(row);
                }
            }


        }

        private bool ValidateRowContract(ORDR_LST row)
        {
            if (!row.ContractID.HasValue) return true;

            PersistedContractLink? persisted = row.idd > 0
                ? dbms.DoGetDataSQL<PersistedContractLink>(
                    "SELECT TOP (1) ContractID, CODE FROM dbo.ORDR_LST WHERE idd = @idd", new { row.idd }).FirstOrDefault()
                : null;

            var contract = dbms.DoGetDataSQL<ContractRowValidation>(@"
SELECT TOP (1) H.IsClosed,
       ProductExists = CONVERT(BIT, CASE WHEN EXISTS
       (
           SELECT 1 FROM dbo.CONTRACT_DTL AS D
           WHERE D.ContractID = H.ContractID AND D.CODE = @Code
       ) THEN 1 ELSE 0 END)
FROM dbo.CONTRACT_HED AS H
WHERE H.ContractID = @ContractID",
                new { ContractID = row.ContractID.Value, Code = row.CODE }).FirstOrDefault();

            if (contract is null)
            {
                new Msgwin(false, "قرارداد انتخاب‌شده وجود ندارد.").ShowDialog();
                return false;
            }
            if (contract.IsClosed &&
                (persisted?.ContractID != row.ContractID ||
                 !string.Equals(persisted.CODE, row.CODE, StringComparison.OrdinalIgnoreCase)))
            {
                new Msgwin(false, "اتصال ردیف جدید به قرارداد مختومه مجاز نیست.").ShowDialog();
                return false;
            }
            if (!contract.ProductExists)
            {
                new Msgwin(false, $"کالای {row.CODE} در ریز قرارداد انتخاب‌شده تعریف نشده است.").ShowDialog();
                return false;
            }
            return true;
        }

        private void ORDER_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) return;
            var row = e.Row.Item as ORDR_LST;
            if (row == null) return;

            int headerId = int.Parse(NUMBER.Text);

            if (!BodyIsValid(row))
            {
                ORDER_LST_SUB_CANCEL_EDIT();
                return;
            }

            if (!ValidateRowContract(row))
            {
                e.Cancel = true;
                return;
            }

            if (row?.ID == null || row.ID == 0)
            {
                // INSERT
                string sql = @"INSERT INTO ORDR_LST (ID, CODE, VAHED_K, MEGH, MEGHk, CUST_NO, DATE, ContractID)
                               OUTPUT INSERTED.idd
                               VALUES (@ID, @CODE, @VAHED_K, @MEGH, @MEGHk, @CUST_NO, @DATE, @ContractID)";

                int newRowID = dbms.DoGetDataSQL<int>(sql, new
                {
                    ID = headerId,
                    row.CODE,
                    row.VAHED_K,
                    row.MEGH,
                    row.MEGHK,
                    CUST_NO = row.CUST_NO ?? "",
                    DATE = DATE_N.Text.ToRawTarikh(),
                    row.ContractID
                }).FirstOrDefault();
                if (newRowID <= 0)
                    throw new InvalidOperationException("شناسه ردیف سفارش پس از ثبت برگردانده نشد.");
                row.ID = headerId;
                row.idd = newRowID;
            }
            else
            {
                // UPDATE
                string sql = @"UPDATE ORDR_LST SET CODE=@CODE, VAHED_K=@VAHED_K, MEGH=@MEGH, MEGHk=@MEGHk,
                                      CUST_NO=@CUST_NO, ContractID=@ContractID
                               WHERE idd = @idd";

                dbms.DoExecuteSQL(sql, new
                {
                    row.CODE,
                    row.VAHED_K,
                    row.MEGH,
                    row.MEGHK,
                    CUST_NO = row.CUST_NO ?? "",
                    row.ContractID,
                    row.idd
                });
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
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"سفارش دهنده نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
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
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { }

        // Signatures
        private void SGN1_Click(object sender, RoutedEventArgs e) { HandleSignature(SGN1, SGN1usid, "1"); }
        private void SGN2_Click(object sender, RoutedEventArgs e) { HandleSignature(SGN2, SGN2usid, "2"); }
        private void SGN3_Click(object sender, RoutedEventArgs e) { HandleSignature(SGN3, SGN3usid, "3"); }

        private void HandleSignature(CheckBox chk, TextBox txt, string sgnNum)
        {
            // Safety Check
            if (string.IsNullOrWhiteSpace(NUMBER.Text) || NUMBER.Text == "0") return;

            bool isSigned = chk.IsChecked ?? false;
            int skid = 35;
            int headerId = Convert.ToInt32(NUMBER.Text);
            int userCod = (int)Baseknow.USERCOD;
            string userName = (string)CL_HESABDARI.UCurrentUser();

            // ساخت پیام ایونت دقیقاً مشابه VBA
            string eventMsg = CL_HESABDARI.GETUSERNAME(userCod)
                              + (isSigned ? $" :امضا شد{sgnNum} "
                                          : $" :امضا برداشته شد{sgnNum}:");

            string taskText = $"سفارش  شماره: {headerId} مورخ {DATE_N.Text}  به نام: {CUST_NO.Text}";
            string compCod = CL_HESABDARI.GETUSERHES(userCod);
            string farsiDate = Tarikh.FullCurrentDate;
            int timeVal = DateTime.Now.Hour * 100 + DateTime.Now.Minute;

            var dbms = new CL_CCNNMANAGER();
            long mid = CL_HESABDARI.Gettaskid(headerId, skid);

            if (mid > 0)
            {
                // ── Task Exists: Insert Event & Update Task ──
                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.events (IDNUM, USERNAME, EVENTS, STDATE, STTIME, SKID, NUM, TG)
              VALUES (@IDNUM, @USERNAME, @EVENTS, @STDATE, @STTIME, @SKID, @NUM, @TG)",
                    new
                    {
                        IDNUM = mid,
                        USERNAME = userName,
                        EVENTS = eventMsg,
                        STDATE = farsiDate,
                        STTIME = timeVal,
                        SKID = skid,
                        NUM = headerId,
                        TG = skid
                    });

                string nextPersonel = CL_HESABDARI.GETUSERTASK(mid);
                dbms.DoExecuteSQL(
                    "UPDATE dbo.TASKS SET PERSONEL = @PERSONEL, STATUS = 1 WHERE IDNUM = @IDNUM",
                    new { PERSONEL = nextPersonel, IDNUM = mid });
            }
            else
            {
                // ── Task Missing: Insert Task -> Get MID -> Insert Event ──
                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.TASKS 
              (PERSONEL, USERNAME, TASK, COMP_COD, STDATE, STTIME, SKID, NUM, TG, CTIM, USERCO)
              VALUES 
              (@PERSONEL, @USERNAME, @TASK, @COMP_COD, @STDATE, @STTIME, @SKID, @NUM, @TG, GETDATE(), @USERCO)",
                    new
                    {
                        PERSONEL = userCod,
                        USERNAME = userName,
                        TASK = taskText,
                        COMP_COD = compCod,
                        STDATE = farsiDate,
                        STTIME = timeVal,
                        SKID = skid,
                        NUM = headerId,
                        TG = skid, // In VBA 'TG' was passed as 35 explicitly
                        USERCO = userCod
                    });

                mid = CL_HESABDARI.Gettaskid(headerId, skid);

                dbms.DoExecuteSQL(
                    @"INSERT INTO dbo.events (IDNUM, USERNAME, EVENTS, STDATE, STTIME, SKID, NUM, TG)
              VALUES (@IDNUM, @USERNAME, @EVENTS, @STDATE, @STTIME, @SKID, @NUM, @TG)",
                    new
                    {
                        IDNUM = mid,
                        USERNAME = userName,
                        EVENTS = eventMsg,
                        STDATE = farsiDate,
                        STTIME = timeVal,
                        SKID = skid,
                        NUM = headerId,
                        TG = skid
                    });
            }

            // ── UI Logic Replications (Missing in your snippet) ──
            if (PERSONEL != null) PERSONEL.Visibility = Visibility.Visible; // معادل Me.PERSONEL.Visible = True

            // ذخیره در متغیر کلاس (اگر در فرم تعریف شده است)
            // Meidnum = mid; 

            // ── Update Main Table (ORDR_HED assumption) ──
            dbms.DoExecuteSQL(
                $"UPDATE dbo.ORDR_HED SET SGN{sgnNum} = @SGN, sgn{sgnNum}usid = @SGNUSID WHERE id = @ID",
                new
                {
                    SGN = isSigned ? 1 : 0,
                    // نکته: اگر امضا برداشته شود، در VBA آی‌دی یوزر ست می‌شد، اما اینجا بهتر است نال شود مگر اینکه لاگ بخواهید
                    // در اینجا طبق منطق استاندارد اگر امضا نیست، یوزر هم نال می‌شود
                    SGNUSID = isSigned ? (object)userCod : DBNull.Value,
                    ID = headerId
                });

            // ── Update Display TextBox ──
            if (isSigned)
            {
                txt.Tag = userCod;
                // فرض بر این است که rst_personel یک لیست لود شده در حافظه است
                var person = rst_personel?.FirstOrDefault(x => x.IDD == userCod);
                txt.Text = person?.SAL_NAME ?? CL_HESABDARI.GETUSERNAME(userCod);
            }
            else
            {
                txt.Tag = null;
                txt.Text = "";
            }

            UpdatePrintButtonsState();
            Form_Current(); // اگر متدی برای رفرش فرم دارید
        }


        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PERSONEL.SelectedItem != null && !_navigationManager.IsNewRecord && NUMBER.Text != "0")
            {
                CL_HESABDARI.PERSONELUpdate(38, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'ثبت سفارشات کالا  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");
                universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            else
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }
        }

        private void BTN_PRINT_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord || ORDR_LST_DATA.Count == 0)
            {
                return;
            }

            GenerateReport(1);
        }

        private void GenerateReport(byte param)
        {
            var report = new StiReport(); //E:\prg\MrCorrect\Prg_UI\Rpts\Factors
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Factors.ORDER_LIST.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
            ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;

            report["NUMBER_PARM"] = NUMBER.Text;

            (report.GetComponentByName("TITLE_RPT") as StiText).Text = param == 1 ? "سفارشات کالا" : "درخواست تولید";
            (report.GetComponentByName("SAZNAME") as StiText).Text = Baseknow.WIDTH_D; //نام شرکت


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

            new WINRPT(report, LABEL_HEADER.Content.ToString()).Show();
        }

        private void BTN_PRINT_TOLID_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord || ORDR_LST_DATA.Count == 0)
            {
                return;
            }

            GenerateReport(2);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (ORDER_LST_SUB.Items.Count > 0 && ORDER_LST_SUB.SelectedItem is ORDR_LST row)
            {
                if (!string.IsNullOrEmpty(row.CODE))
                {
                    // F_MENU_KART ...
                }
            }
        }

        private bool BodyIsValid(ORDR_LST TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in ORDER_LST_SUB.ItemsSource
                          let c = ORDER_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

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
            if (!double.TryParse(TheRow.MEGH.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHK.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            // Validate MANDAH
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

        private sealed class ContractOrderLookup
        {
            public int? ContractID { get; set; }
            public bool IsClosed { get; set; }
            public string DisplayName { get; set; } = string.Empty;
        }
        private sealed class ContractRowValidation
        {
            public bool IsClosed { get; set; }
            public bool ProductExists { get; set; }
        }
        private sealed class PersistedContractLink
        {
            public int? ContractID { get; set; }
            public string CODE { get; set; } = string.Empty;
        }

    }
}
