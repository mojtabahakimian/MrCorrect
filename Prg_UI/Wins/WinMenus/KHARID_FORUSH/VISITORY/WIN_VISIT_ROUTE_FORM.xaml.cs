using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
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
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Syncfusion.Data.Extensions;
using System.Windows.Media;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Wins.WinOther;
using static Interfaces.INavigator;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    public partial class WIN_VISIT_ROUTE_FORM : Window, ISearchableWindow
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
        public WIN_VISIT_ROUTE_FORM()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        private NavigationManager<VISIT_ROUTE> _navigationManager;
        public ObservableCollection<VISIT_ROUTE_DTL> VISIT_ROUTE_DATA { get; set; } = new ObservableCollection<VISIT_ROUTE_DTL>();
        public ObservableCollection<CLASS_MODEL_COMBO> ClassSource { get; } = new ObservableCollection<CLASS_MODEL_COMBO>();

        #region LOCAL_MODEL
        public class DistrictComboModel
        {
            public string? District { get; set; }
        }
        public class CLASS_MODEL_COMBO : INotifyPropertyChanged, ICloneable
        {
            public object Clone() { return this.MemberwiseClone(); }
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

            private string? _CLASS;
            public string? CLASS
            {
                get => _CLASS;
                set
                {
                    if (_CLASS == value) return;
                    _CLASS = value;
                    OnPropertyChanged(nameof(CLASS));
                }
            }
        }
        #endregion
        public bool NowIsReady { get; private set; }
        public bool DG_SUB_IsFocused { get; private set; }

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                return _newrecord;
            }
            set { _newrecord = value; }
        }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int DG_SUB_DEF_INDEX_COL
        {
            get
            {
                if (DG_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = DG_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NAME_HES")?.DisplayIndex;
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

                ROUTE_NAME.IsReadOnly = !ican; //مسیر ویزیت
                DG_SUB.IsReadOnly = !ican;

                HES.IsEnabled = ican;
                HES2.IsEnabled = ican;
                District.IsEnabled = ican;

                OSTANID.IsEnabled = ican;
                if (OSTANID.SelectedValue != null)
                {
                    SHAHRID.IsEnabled = ican;
                }

                BTN_SAVE.IsEnabled = ican;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid DG = DG_SUB;
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (DG_SUB.IsKeyboardFocusWithin)
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

                                    //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                    var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                    if (focusedWindow != null)
                                    {
                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            focusedWindow.Activate();
                                            focusedWindow.Focus();
                                        }), DispatcherPriority.Background);
                                    }

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
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

                if (!DG_SUB.IsKeyboardFocusWithin && !DG_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
                {
                    if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        e.Handled = true;
                        var searchWindow = new EnhancedSearchWindow(this);
                        searchWindow.Owner = this;
                        searchWindow.ShowDialog();
                    }
                }
            }
            catch { }


            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VISIT_ROUTE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            _navigationManager = new NavigationManager<VISIT_ROUTE>(
                dbms,
                x => x?.ROUTE_NAME?.ToString(),
                $"SELECT ROUTE_NAME, HES, IYALAT, CITY, District, CDATE, USERNAME, RACTIVE, RACTIVE, CRT, UID FROM Visit_route ORDER BY CRT",
                x => $"SELECT ROUTE_NAME, HES, IYALAT, CITY, District, CDATE, USERNAME, RACTIVE, RACTIVE, CRT, UID " +
                $"FROM Visit_route WHERE ROUTE_NAME = N'{x?.ROUTE_NAME?.ToString()}' ",
                default);

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            if (!NewRecord)
            {
                AllowEdits = false;
            }

            CL_LMethods.SetTabIndexes(
             ROUTE_NAME,
             HES,
             OSTANID,
             SHAHRID,
             District,
             BTN_SAVE,
             DG_SUB
             );

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(VISIT_ROUTE record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<VISIT_ROUTE>($"SELECT ROUTE_NAME, HES, IYALAT, CITY, District, CDATE, USERNAME, RACTIVE, RACTIVE, CRT, UID" +
                    $" FROM Visit_route WHERE ROUTE_NAME = N'{ROUTE_NAME.Text}'").FirstOrDefault();
                record = itemtoadd;
                NewRecord = false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(VISIT_ROUTE HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, VISIT_ROUTE_DATA);
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین آیتمی ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                Command106.IsEnabled = true;

                ROUTE_NAME.Text = HEADER_FAC.ROUTE_NAME; //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USERNAME.ToStringNullSafe(); //کاربر

                string thevalue = HEADER_FAC.HES;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();
                if (!string.IsNullOrEmpty(data?.NAME))
                {
                    if (HES.ItemsSource == null)
                    {
                        HES.ItemsSource = new List<Custom_CUST_HESAB>();
                    }

                    if (!((List<Custom_CUST_HESAB>)HES.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)HES.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    HES.SelectedValue = HEADER_FAC.HES; //مشتری
                    HES.Items.Refresh();
                }
                RACTIVE.IsChecked = HEADER_FAC.RACTIVE;

                //OSTANID.SelectionChanged -= OSTANID_SelectionChanged;

                // after loading ALL_OSTAN:
                var match = ALL_OSTAN.FirstOrDefault(x => x.OSNAME?.FixPersianChars() == HEADER_FAC?.IYALAT?.FixPersianChars());
                if (match != null)
                    OSTANID.SelectedValue = match.OSCODE;


                //OSTANID.SelectionChanged += OSTANID_SelectionChanged;

                var matchcity = ALL_SHAHR.FirstOrDefault(x => x.CITYNAME?.FixPersianChars() == HEADER_FAC?.CITY?.FixPersianChars());
                if (matchcity != null)
                    SHAHRID.SelectedValue = matchcity.CITYCODE;

                District.SelectedValue = HEADER_FAC.District; District.Items.Refresh();
                District.Text = HEADER_FAC.District;

                ESLAH.IsEnabled = true;

                Form_Current();

                DG_SUB_ReGetData();
            }
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is VISIT_ROUTE item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.ROUTE_NAME.Equals(item.ROUTE_NAME));
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
                new SearchableProperty { DisplayName = "نام مسیر ویزیت", PropertyPath = "ROUTE_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "HES", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USERNAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "استان", PropertyPath = "IYALAT", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "شهر", PropertyPath = "CITY", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "منطقه", PropertyPath = "District", PropertyType = typeof(string) },
            };
        }
        #endregion

        private void Form_Current()
        {
            AllowDeletions = false;
            AllowEdits = false;
        }

        private void RefreshAfterUpdate()
        {
            NewRecord = false;

            var CURRENT_HEADER = dbms.DoGetDataSQL<VISIT_ROUTE>($"SELECT ROUTE_NAME, HES, IYALAT, CITY, District, CDATE, USERNAME, RACTIVE, RACTIVE, CRT, UID" +
               $" FROM Visit_route WHERE ROUTE_NAME = N'{ROUTE_NAME.Text}'").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //کد استان
            ALL_OSTAN = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();
            foreach (var item in ALL_OSTAN) { item.OSNAME = item.OSNAME?.FixPersianChars(); }
            OSTANID.ItemsSource = ALL_OSTAN;
            //کد شهر
            ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();
            SHAHRID.ItemsSource = ALL_SHAHR;

            //منطقه
            District.ItemsSource = dbms.DoGetDataSQL<DistrictComboModel>("SELECT District FROM Visit_route GROUP BY District ORDER BY District").ToList();

            HES.ItemsSource = new List<Custom_CUST_HESAB>();
            HES.DisplayMemberPath = "NAME";
            HES.SelectedValuePath = "hes";

            //حساب ویزیتوری
            HES2.ItemsSource = HES.ItemsSource;
            HES2.DisplayMemberPath = "hes";
            HES2.SelectedValuePath = "hes";

            //کلاس مشتری حالت اتوکامپلت
            //CLASS_COLUMN.ItemsSource = dbms.DoGetDataSQL<CLASS_MODEL_COMBO>($"SELECT CLASS FROM Visit_route_dtl GROUP BY CLASS ORDER BY CLASS").ToList();

            // کلاس مشتری
            var existing = dbms.DoGetDataSQL<CLASS_MODEL_COMBO>("SELECT DISTINCT CLASS FROM Visit_route_dtl WHERE CLASS IS NOT NULL ORDER BY CLASS").ToList();

            ClassSource?.Clear();
            foreach (var c in existing)
                ClassSource.Add(c);

            CLASS_COLUMN.ItemsSource = ClassSource;
        }
        private void MakeDefaultFocuseReady()
        {
            ROUTE_NAME.Focus();
            ROUTE_NAME.SelectAll();
        }
        private void DataGridActivation()
        {
            if (NewRecord)
            {
                DG_SUB.IsReadOnly = true;
            }
            else
            {
                DG_SUB.IsReadOnly = false;
            }
        }
        private void ClearFreshAll()
        {
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری
            HES.SelectedIndex = -1; HES.Items.Refresh();
            RACTIVE.IsChecked = false;
            NewRecord = true;
            ROUTE_NAME.Text = null; //مسیر ویزیت

            OSTANID.SelectedItem = null;
            District.SelectedItem = null; District.Text = null;
            SHAHRID.SelectedItem = null;
            ESLAH.IsEnabled = false;
            Command106.IsEnabled = false;

            VISIT_ROUTE_DATA?.Clear(); //دیتاگرید فاکتور فروش
            AllowEdits = true;

            DG_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "NAME_HES", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "HENTER", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //CL_HESABDARI.SETSECURITYSUB(DG_SUB, "HENTER");

            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}
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

            if (string.IsNullOrEmpty(ROUTE_NAME.Text) || string.IsNullOrWhiteSpace(ROUTE_NAME.Text)) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مسیر ویزیت نمیتواند خالی باشد" });
            }

            if (IsNull(this.HES.SelectedValue) || this.HES.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " ویزیتور مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.HES2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب ویزیتور مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(HES.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(HES.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
            }

            if (OSTANID.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "اُستان نمیتواند خالی باشد" });
            }
            if (SHAHRID.SelectedValue == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شهر نمیتواند خالی باشد" });
            }

            if (string.IsNullOrEmpty(District.Text) || string.IsNullOrWhiteSpace(District.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "منطقه نمیتواند خالی باشد" });
            }
            else if (District.Text.Length > 40)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول متن وارد شده برای منطقه بیش از 40 کاراکتر است !" });
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

        private bool BodyIsValid(VISIT_ROUTE_DTL TheRow)
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

            if (string.IsNullOrEmpty(TheRow?.COUST_NO) || string.IsNullOrWhiteSpace(TheRow?.COUST_NO))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری نمیتواند خالی باشد" });
            }

            if (TheRow.CLASS?.Length > 40)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تعداد کاراکتر وارد شده برای کلاس مشتری بیش از 40 کاراکتر است !" });
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
        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }
        public Visual I_AM_VISIT_ROUTE { get; private set; }
        public VISIT_ROUTE_DTL? CURRENT_ROW_ITEMS { get; private set; }
        public VISIT_ROUTE_DTL? WAS_ROW_ITEM { get; private set; } = new VISIT_ROUTE_DTL();

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
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"این مسیر قبلا تعریف شده نمیتوان مسیر تکراری تعریف کرد").Show();
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

            this.RACTIVE.IsChecked = true;

            this.DG_SUB.IsReadOnly = false;
            ESLAH.IsEnabled = true;

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (VISIT_ROUTE_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!NewRecord)
            {
                SecurityAllCheck();

                var dt = DateTime.Now;
                //CL_HESABDARI.TR("VISIT_ROUTE", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12

                HES.IsEnabled = true; //Lock true
                DG_SUB.IsReadOnly = false;
                AllowDeletions = true;
                AllowEdits = true;
                DG_SUB.IsReadOnly = false; // UnLocked
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (NewRecord || DG_SUB.IsEnabled == false || !BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (VISIT_ROUTE_DATA.Count > 0)
            {
                if (DG_SUB.IsReadOnly) { return; }

                try
                {
                    var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(DG_SUB.ItemsSource);
                    if (view.IsAddingNew && view.CanCancelEdit)
                    {
                        //view.CancelNew();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else if (view.IsEditingItem && view.CanCancelEdit)
                    {
                        //view.CancelEdit();
                        return; //Get out to avoid delete for deleting part of text inside the cell in DataGrid to conflict with Delete Row !
                    }
                    else
                    {
                        //Cancel Any Editting to avoid conflict during remove
                        DG_SUB.CommitEdit(DataGridEditingUnit.Cell, true);
                        DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                    }
                }
                catch { }
            }


            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                bool IsDeletedSomething = false;

                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف مسیر ویزیت",
                    recordId: ROUTE_NAME.Text,
                    oldValue: "",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                if (VISIT_ROUTE_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    //CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //1
                    #endregion

                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                    {
                        var item = DG_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                        {
                            continue; // Skip deletion for new placeholder items
                        }

                        var _id_ = item.GetType().GetProperty("IDR").GetValue(item);

                        if (_id_ != null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"DELETE FROM dbo.Visit_route_dtl WHERE IDR = {_id_}");

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
                        DG_SUB_ReGetData();
                    }
                }
                else
                {
                    if (!NewRecord)
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.Visit_route WHERE ROUTE_NAME = N'{ROUTE_NAME.Text}' ");

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
                                new Msgwin(false, "این مسیر ویزیت دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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

        private bool DoCmdHeaderSave(bool DisplayMsg = true)
        {
            string routeName = ROUTE_NAME.Text.Trim();

            // 1. Create or Update the Master record (VISIT_ROUTE)
            var masterRecord = new VISIT_ROUTE
            {
                ROUTE_NAME = routeName,
                RACTIVE = true,
                HES = HES.SelectedValue.ToString(),
                IYALAT = OSTANID.Text.ToString(),
                CITY = SHAHRID.Text.ToString(),
                District = District.Text,
                CDATE = DateTime.Now,
                USERNAME = USER_NAME.Text,
            };

            //var RowExisting = dbms.DoGetDataSQL<string?>($"SELECT 1 FROM VISIT_ROUTE WHERE ROUTE_NAME = N'{ROUTE_NAME.Text}' ").FirstOrDefault();
            var RowExisting = dbms.DoGetDataSQL<string?>($"SELECT 1 FROM VISIT_ROUTE WHERE ROUTE_NAME = @ROUTE_NAME", new { ROUTE_NAME = ROUTE_NAME.Text }).FirstOrDefault();
            if (RowExisting == null) //Insert
            {
                _ = dbms.DoExecuteSQL($@"INSERT INTO VISIT_ROUTE (ROUTE_NAME, HES, IYALAT, CITY, District, CDATE, USERNAME, RACTIVE)
                                     VALUES (@ROUTE_NAME, @HES, @IYALAT, @CITY, @District, @CDATE, @USERNAME, @RACTIVE);", masterRecord);
                RefreshAfterUpdate();
            }
            else
            {
                Msgwin msgwin = new Msgwin(true, $"این مسیر ویزیت به نام '{ROUTE_NAME.Text}' وجود دارد , آیا از بروز کردن اطلاعات آن مطمئن هستید ؟ ");
                if (ROUTE_NAME.Text.Trim().FixPersianChars() != _navigationManager.CurrentRecord.ROUTE_NAME.FixPersianChars())
                {
                    msgwin.ShowDialog();
                }
                if (msgwin?.DialogResult == false)
                {
                    _ = msgwin;
                }
                else
                {
                    _ = dbms.DoExecuteSQL($@"UPDATE VISIT_ROUTE SET RACTIVE=@RACTIVE, HES=@HES, IYALAT=@IYALAT, CITY=@CITY, District=@District, CDATE=@CDATE, USERNAME=@USERNAME
                            WHERE ROUTE_NAME = @ROUTE_NAME;", masterRecord);
                }
            }

            return true;
        }

        public void DG_SUB_ReGetData()
        {
            if (!NewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>(@$"
                                    SELECT 
                                      C.NAME AS NAME_HES, 
                                      D.ROUTE_NAME,
                                      D.COUST_NO,
                                      D.RACTIVE, 
                                      D.IDR, 
                                      D.CLASS, 
                                      D.CRT, 
                                      D.UID
                                    FROM dbo.CUST_HESAB AS C
                                    RIGHT JOIN dbo.Visit_route_dtl AS D
                                      ON C.hes = D.COUST_NO
                                    WHERE D.ROUTE_NAME = @RouteName", new { RouteName = ROUTE_NAME.Text }).ToList();


                VISIT_ROUTE_DATA?.Clear();
                foreach (var item in QRE_LST)
                    VISIT_ROUTE_DATA?.Add(item);
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
                Task.Delay(400, cts.Token);
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

            // The DataContext here is the underlying INVO_LST row.
            if (!(combo.DataContext is VISIT_ROUTE_DTL currentRow)) return;

            // Update the row with the selected item.
            if (combo.SelectedItem is CUST_HESAB_COMBINED selectedStuf)
            {
                // CODE is already bound via SelectedValue.
                //currentRow.HES = selectedStuf.hes;
                currentRow.NAME_HES = selectedStuf.NAME;
            }
        }
        private void HESEditCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ComboBox combo)) return;

            // Delay focus setting to ensure that the control is ready.
            Dispatcher.BeginInvoke(new Action(() => combo.Focus()), DispatcherPriority.Input);

            if (!(combo.DataContext is VISIT_ROUTE_DTL currentRow)) return;

            if (!string.IsNullOrEmpty(currentRow.COUST_NO))
            {
                try
                {
                    string sql = "SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE hes = @pCode";
                    var parameters = new { pCode = currentRow.COUST_NO };
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

        private void OSTANID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (!NowIsReady) { return; }

            if (OSTANID.SelectedValue is not null)
            {
                //شهرستان
                ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>($"SELECT CITYCODE, CITYNAME FROM TCOD_CITY WHERE OSCODE = {OSTANID.SelectedValue} ORDER BY CITYNAME").ToList();
                foreach (var item in ALL_SHAHR) { item.CITYNAME = item.CITYNAME?.FixPersianChars(); }
                SHAHRID.ItemsSource = ALL_SHAHR;
                SHAHRID.IsEnabled = true;
            }
            else
            {
                SHAHRID.IsEnabled = false;
            }
        }
        private void HES_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)HES.Template.FindName("PART_EditableTextBox", HES);

            if (HES.SelectedValue is not null)
            {
                if ((HES.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HES, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"ویزیتور نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }

            if (HES.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(HES.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    HES.SelectedValue = null;
                }
                if (CL_HESABDARI.BLOCKEDCUST(HES.SelectedValue.ToString()))
                {
                    HES.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }


        }

        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || VISIT_ROUTE_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Visitory.R_LIST_MASIR.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["ROUTE_PARAM1"] = ROUTE_NAME.Text;
            report["ROUTE_PARAM2"] = ROUTE_NAME.Text.FixPersianChars();
            report["HES_PARAM"] = HES.SelectedValue;

            (report.GetComponentByName("VISITOR_TXT") as StiText).Text = HES.Text;
            (report.GetComponentByName("DATEEMROOZ") as StiText).Text = Tarikh.FullCurrentDate;

            new WINRPT(report, "مسیر ویزیت").Show();
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.Invoke(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    DG_SUB.CancelEdit();
                    //DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                }
                else
                {
                    DG_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                    //DG_SUB.CommitEdit((DataGridEditingUnit)_RC_, true);
                }
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }

        public string GetHesLevel(string HES)
        {
            double? KOL = null, MOIN = null, TAF = null, TAF2 = null, TAF3 = null, TAF4 = null;

            // فرض بر اینکه متد GETTAF3 مقادیر را با ref برمی‌گرداند
            CL_HESABDARI.GETTAF3(HES, ref KOL, ref MOIN, ref TAF, ref TAF2, ref TAF3, ref TAF4);

            if (!string.IsNullOrEmpty(TAF4?.ToString()))
                return "TDETA_HES4";
            else if (!string.IsNullOrEmpty(TAF3?.ToString()))
                return "TDETA_HES3";
            else if (!string.IsNullOrEmpty(TAF2?.ToString()))
                return "TDETA_HES2";
            else if (!string.IsNullOrEmpty(TAF?.ToString()))
                return "TDETA_HES";
            else
                return null;
        }
        private string BuildExprWhereCondition(string tableName)
        {
            return tableName switch
            {
                "TDETA_HES" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) = @COUST_NO",
                "TDETA_HES2" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR)) = @COUST_NO",
                "TDETA_HES3" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER3 AS NVARCHAR)) = @COUST_NO",
                "TDETA_HES4" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER3 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER4 AS NVARCHAR)) = @COUST_NO",
                _ => throw new Exception("سطح حساب نامعتبر است.")
            };
        }
        private string BuildExprSelect(string tableName)
        {
            string expr = tableName switch
            {
                "TDETA_HES" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR))",
                "TDETA_HES2" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR))",
                "TDETA_HES3" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER3 AS NVARCHAR))",
                "TDETA_HES4" => "RTRIM(CAST(N_KOL AS NVARCHAR)) + '-' + RTRIM(CAST(NUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER2 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER3 AS NVARCHAR)) + '-' + RTRIM(CAST(TNUMBER4 AS NVARCHAR))",
                _ => throw new Exception("سطح حساب ناشناخته است.")
            };

            return $"SELECT {expr} AS Expr1, ROUTE_NAME FROM {tableName} WHERE {expr} = @COUST_NO";
        }

        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && DG_SUB.SelectedItem is not null)
            {
                if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((VISIT_ROUTE_DTL)DG_SUB.SelectedItem).Clone() as VISIT_ROUTE_DTL;
                }
            }
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.EditingElement == null || e.Column == null)
            {
                return;
            }

            #region REFILL_CURRENTS
            ComboBox Comboval = null; TextBox TexboVal = null; CheckBox? CheckVal = null;
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
                ENTERED_VALUE_ROW = TexboVal?.Text?.Trim();
            }

            CURRENT_ROW_ITEMS = e.Row.Item as VISIT_ROUTE_DTL;
            if (CURRENT_ROW_ITEMS == null)
            {
                return;
            }

            if (!(e.EditingElement is null))
            {
                CheckVal = e.EditingElement as CheckBox;
            }

            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                ENTERED_VALUE_ROW = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();

            ComboBox HES_COMBO = null;
            if (e.EditingElement is ContentPresenter contentPresenter)
            {
                HES_COMBO = contentPresenter.ContentTemplate.FindName("EditCombo", contentPresenter) as ComboBox;

                if (HES_COMBO == null)
                {
                    HES_COMBO = DataGridHelper.FindVisualChild<ComboBox>(contentPresenter);
                }
                if (HES_COMBO != null)
                {
                    ENTERED_VALUE_ROW = HES_COMBO.Text;
                }
            }
            #endregion

            //نام مشتری
            if (e.Column.SortMemberPath == "NAME_HES" || e.Column.Header.ToString() == "نام مشتری")
            {
                if (HES_COMBO?.SelectedValue is null || ENTERED_VALUE_ROW != CURRENT_ROW_ITEMS?.NAME_HES) //if is different then
                {
                    var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(HES_COMBO, dbms);
                    if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
                    {
                        CURRENT_ROW_ITEMS.COUST_NO = WAS_ROW_ITEM.COUST_NO;
                        CURRENT_ROW_ITEMS.NAME_HES = WAS_ROW_ITEM.NAME_HES;
                        universControl.PopNotifyShowUp($"حساب نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                    }
                    else
                    {
                        CURRENT_ROW_ITEMS.COUST_NO = _SelectedHesab_.hes;
                        CURRENT_ROW_ITEMS.NAME_HES = _SelectedHesab_.NAME;


                        //COUST_NO_BeforeUpdate
                        // تعیین سطح حساب برای اعمال فیلتر مناسب
                        UpdatePathForOthers(CURRENT_ROW_ITEMS);
                    }
                }
            }

            if (e.Column.SortMemberPath == "CLASS")
            {
                var newVal = Comboval.Text?.Trim();
                if (string.IsNullOrEmpty(newVal) || string.IsNullOrWhiteSpace(newVal))
                    return;

                // اگر در کالکشن نبود، اضافه‌اش کن
                if (!ClassSource.Any(x => x.CLASS.Equals(newVal, StringComparison.OrdinalIgnoreCase)))
                {
                    ClassSource.Add(new CLASS_MODEL_COMBO { CLASS = newVal });
                }
                CURRENT_ROW_ITEMS.CLASS = newVal;
            }
        }

        private void UpdatePathForOthers(VISIT_ROUTE_DTL CurrentRow)
        {
            var routeName = ROUTE_NAME.Text;
            string coustNo = CurrentRow.COUST_NO.ToStringNullSafe();
            string TBH = GetHesLevel(coustNo);

            if (string.IsNullOrEmpty(TBH))
                return;

            string exprSelect = BuildExprSelect(TBH);
            var rst = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>(exprSelect, new { COUST_NO = coustNo }).FirstOrDefault();

            if (rst != null)
            {
                string currentRouteName = Convert.ToString(rst.ROUTE_NAME);

                if (string.IsNullOrEmpty(currentRouteName)) //اگرمشتری به هيچ مسيری مرتبط نيست
                {
                    string updateSql = $"UPDATE {TBH} SET ROUTE_NAME = @ROUTE_NAME WHERE {BuildExprWhereCondition(TBH)}";
                    dbms.DoExecuteSQL(updateSql, new { ROUTE_NAME = routeName, COUST_NO = coustNo });
                }
                else if (currentRouteName != routeName)
                {
                    //اگرمشتری به  مسيری مرتبط است
                    string routeDtlSql = "SELECT * FROM Visit_route_dtl WHERE COUST_NO = @COUST_NO AND RACTIVE = 1";
                    var existingDtl = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>(routeDtlSql, new { COUST_NO = coustNo }).FirstOrDefault();

                    if (existingDtl != null)
                    {
                        Msgwin msgwin = new Msgwin(true, $"این مشتری زیر مجموعه مسیر ویزیتوری : {currentRouteName} قبلا ثبت شده است. آیا مایلید در آن مسیر غیر فعال شود و به این مسیر اضافه شود؟");
                        var result = msgwin.ShowDialog();
                        if (result == true)
                        {
                            dbms.DoExecuteSQL("UPDATE Visit_route_dtl SET RACTIVE = 0 WHERE IDR = @IDR", new { IDR = existingDtl.IDR });

                            string updateSql = $"UPDATE {TBH} SET ROUTE_NAME = @ROUTE_NAME WHERE {BuildExprWhereCondition(TBH)}";
                            dbms.DoExecuteSQL(updateSql, new { ROUTE_NAME = routeName, COUST_NO = coustNo });
                        }
                        else
                        {
                            DG_SUB_CANCEL_EDIT();
                        }
                    }
                    else
                    {
                        string updateSql = $"UPDATE {TBH} SET ROUTE_NAME = @ROUTE_NAME WHERE {BuildExprWhereCondition(TBH)}";
                        dbms.DoExecuteSQL(updateSql, new { ROUTE_NAME = routeName, COUST_NO = coustNo });
                    }
                }
            }
        }

        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (!HeaderIsValid())
            {
                return;
            }

            var ROW = e.Row.Item as VISIT_ROUTE_DTL;
            if (e.Row.Item == null || ROW is null)
            {
                return;
            }

            if (!BodyIsValid(ROW))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            int? idd = null;
            try
            {
                if (ROW?.IDR is null) //INSERT
                {
                    var DetailRecord = new VISIT_ROUTE_DTL
                    {
                        ROUTE_NAME = ROUTE_NAME.Text,
                        COUST_NO = ROW.COUST_NO,
                        RACTIVE = ROW.RACTIVE,
                        CLASS = ROW.CLASS
                    };
                    idd = dbms.DoGetDataSQL<int?>(@$"INSERT INTO VISIT_ROUTE_DTL(ROUTE_NAME, COUST_NO, RACTIVE, CLASS)
                                                     OUTPUT INSERTED.IDR
                                                     VALUES(@ROUTE_NAME, @COUST_NO, @RACTIVE, @CLASS)", DetailRecord).FirstOrDefault();
                }
                else //UPDATE
                {
                    var DetailRecord = new VISIT_ROUTE_DTL
                    {
                        ROUTE_NAME = ROUTE_NAME.Text,
                        COUST_NO = ROW.COUST_NO,
                        RACTIVE = ROW.RACTIVE,
                        CLASS = ROW.CLASS,
                        IDR = ROW.IDR
                    };

                    dbms.DoExecuteSQL(@$"UPDATE dbo.VISIT_ROUTE_DTL
                                          SET ROUTE_NAME = @ROUTE_NAME,
                                              COUST_NO = @COUST_NO,
                                              RACTIVE = @RACTIVE,
                                              CLASS = @CLASS
                                          WHERE IDR = @IDR", DetailRecord);
                }

                UpdatePathForOthers(ROW); //AfterUpdate
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام مشتری تکراری است آنرا اصلاح کنید").ShowDialog();
                }

                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }
            if (idd != null) //So Much Important
            {
                ROW.IDR = idd;
            }

        }

        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MouseRightButtonUp="DG_SUB_MouseRightButtonUp"
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
        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && DG_SUB.SelectedItem != null)
            {
                if (!(e is null) && DG_SUB.SelectedItem is not null)
                {
                    if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((VISIT_ROUTE_DTL)DG_SUB.SelectedItem).Clone() as VISIT_ROUTE_DTL;
                    }
                }
            }
        }
        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && BTN_DELETE.IsEnabled)
            {
                try
                {
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
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {

        }

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RACTIVE_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            DG_SUB.BeginEdit();
        }
    }
}