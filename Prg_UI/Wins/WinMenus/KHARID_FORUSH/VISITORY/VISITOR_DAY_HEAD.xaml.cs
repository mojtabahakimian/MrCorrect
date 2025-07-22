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
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    public partial class VISITOR_DAY_HEAD : Window, ISearchableWindow
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
        public VISITOR_DAY_HEAD()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();


        private NavigationManager<VISITORS_DAY> _navigationManager;
        public ObservableCollection<VISITORS_DAY_DTL> VISIT_DAY_DATA { get; set; } = new ObservableCollection<VISITORS_DAY_DTL>();
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

                DG_SUB.IsReadOnly = !ican;
                VDATE.IsReadOnly = !ican;

                HES.IsEnabled = ican;
                HES2.IsEnabled = ican;

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

            _navigationManager = new NavigationManager<VISITORS_DAY>(
                dbms,
                x => x?.HES?.ToString(),
                $"SELECT HES, VDATE, CDATE, USERNAME, OKF, CRT, UID FROM dbo.VISITORS_DAY ORDER BY CRT",
                x => $"SELECT HES, VDATE, CDATE, USERNAME, OKF, CRT, UID " +
                $"FROM VISITORS_DAY WHERE HES = N'{x?.HES?.ToString()}' ",
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
             VDATE,
             HES,
             BTN_SAVE,
             DG_SUB
             );

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(VISITORS_DAY record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<VISITORS_DAY>($"SELECT HES, VDATE, CDATE, USERNAME, OKF, CRT, UID " +
                    $" FROM VISITORS_DAY WHERE HES = N'{HES.SelectedValue}'").FirstOrDefault();
                record = itemtoadd;
                NewRecord = false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(VISITORS_DAY HEADER_FAC)
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

                USER_NAME.Text = HEADER_FAC.USERNAME.ToStringNullSafe(); //کاربر
                VDATE.Text = HEADER_FAC.VDATE.ToString();

                HES.SelectedValue = HEADER_FAC.HES;
                HES.Items.Refresh();

                OKF.IsChecked = HEADER_FAC.OKF;

                BTN_GETMASIR.IsEnabled = true;
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
            if (selectedItem is VISITORS_DAY item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.HES.Equals(item.HES));
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
                new SearchableProperty { DisplayName = "ویزیتور", PropertyPath = "HES", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تاریخ ویزیت", PropertyPath = "VDATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "نام کاربری", PropertyPath = "USERNAME", PropertyType = typeof(string) },
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

            var CURRENT_HEADER = dbms.DoGetDataSQL<VISITORS_DAY>($"SELECT HES, VDATE, CDATE, USERNAME, OKF, CRT, UID " +
               $" FROM VISITORS_DAY WHERE HES = N'{HES.SelectedValue}' AND VDATE = {VDATE.Text.ToRawTarikh()}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            HES.ItemsSource = new List<Custom_CUST_HESAB>();
            HES.DisplayMemberPath = "NAME";
            HES.SelectedValuePath = "hes";
            var RST_HES = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@$"SELECT Visit_route.HES AS hes,
                                                                CUST_HESAB.NAME
                                                         FROM Visit_route
                                                             INNER JOIN CUST_HESAB
                                                                 ON Visit_route.HES = CUST_HESAB.hes
                                                         GROUP BY Visit_route.HES,
                                                                  CUST_HESAB.NAME,
                                                                  CUST_HESAB.MOBILE").ToList();

            foreach (var item in RST_HES)
            {
                if (!string.IsNullOrEmpty(item?.NAME))
                {
                    item.NAME = item.NAME.FixPersianChars();
                }
            }

            HES.ItemsSource = RST_HES;

            //حساب ویزیتوری
            HES2.ItemsSource = HES.ItemsSource;
            HES2.DisplayMemberPath = "hes";
            HES2.SelectedValuePath = "hes";

            // کلاس مشتری
            var existing = dbms.DoGetDataSQL<CLASS_MODEL_COMBO>("SELECT DISTINCT CLASS FROM Visit_route_dtl WHERE CLASS IS NOT NULL ORDER BY CLASS").ToList();

            ClassSource?.Clear();
            foreach (var c in existing)
                ClassSource?.Add(c);

            CLASS_COLUMN.ItemsSource = ClassSource;
        }
        private void MakeDefaultFocuseReady()
        {
            VDATE.Focus();
            VDATE.SelectAll();
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
            OKF.IsChecked = false;
            NewRecord = true;
            HES.Text = null; //مسیر ویزیت
            VDATE.Text = Tarikh.FullCurrentDate;
            ESLAH.IsEnabled = false;
            BTN_GETMASIR.IsEnabled = false;
            Command106.IsEnabled = false;

            VISIT_DAY_DATA?.Clear(); //دیتاگرید فاکتور فروش
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

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!Tarikh.IsValidedDate(VDATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
            }
            else
            {
                if (!Tarikh.IsSyncedDateNow(VDATE.Text, Baseknow.CTL_DT ?? false))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                }
            }

            if (HES.SelectedValue == null || this.HES.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " ویزیتور مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.HES2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب ویزیتور مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (HES.SelectedValue != null)
            {
                if (CL_HESABDARI.ISTAF(HES.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
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
        private bool BodyIsValid(VISITORS_DAY_DTL TheRow)
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
            else
            {
                if (CL_HESABDARI.BLOCKEDCUST(TheRow?.COUST_NO?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"حساب {TheRow?.COUST_NO} مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
                }
            }

            if (TheRow?.CLASS?.Length > 40)
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

        public Visual I_AM_VISIT_ROUTE { get; private set; }
        public VISITORS_DAY_DTL? CURRENT_ROW_ITEMS { get; private set; }
        public VISITORS_DAY_DTL? WAS_ROW_ITEM { get; private set; } = new VISITORS_DAY_DTL();

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
                if (!DoCmdHeaderSave())
                {
                    return;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"این ویزیت با این تاریخ قبلا تعریف شده نمیتوان مسیر تکراری تعریف کرد").Show();
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

            this.DG_SUB.IsReadOnly = false;
            BTN_GETMASIR.IsEnabled = true;
            ESLAH.IsEnabled = true;

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (VISIT_DAY_DATA.Count == 0)
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
                //CL_HESABDARI.TR("VISITORS_DAY", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12

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

            if (VISIT_DAY_DATA.Count > 0)
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
                    tableName: "تعریف ویزیت روزانه",
                    recordId: HES.SelectedValue.ToString(),
                    oldValue: "",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                if (VISIT_DAY_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
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
                            VISIT_DAY_DATA.Remove(item as VISITORS_DAY_DTL);
                            continue; // Skip deletion for new placeholder items
                        }

                        var _HES_ = item.GetType().GetProperty("HES").GetValue(item);
                        var _VDATE_ = item.GetType().GetProperty("VDATE").GetValue(item);
                        var _COUST_NO_ = item.GetType().GetProperty("COUST_NO").GetValue(item);

                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.VISITORS_DAY_DTL WHERE HES = @HES AND VDATE = @VDATE AND COUST_NO = @COUST_NO",
                                new { HES = _HES_, VDATE = Convert.ToInt64(_VDATE_), COUST_NO = _COUST_NO_ });

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

                    if (ErrosMessages.Any())
                    {
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).Show();
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
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.VISITORS_DAY WHERE HES = @HES AND VDATE = @VDATE",
                                new { HES = HES.SelectedValue, VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh()) });

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
                                new Msgwin(false, "این ویزیتور دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
            var masterRecord = new VISITORS_DAY
            {
                HES = HES.SelectedValue.ToString(),
                VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh()),
                CDATE = DateTime.Now,
                OKF = true,
                USERNAME = USER_NAME.Text,
            };

            var RowExisting = dbms.DoGetDataSQL<string?>($"SELECT 1 FROM VISITORS_DAY WHERE HES = @HES AND VDATE = @VDATE",
                new
                {
                    HES = HES.SelectedValue,
                    VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh())
                }).FirstOrDefault();


            if (NewRecord && RowExisting != null)
            {
                Msgwin msgwin0 = new Msgwin(true, $"این ویزیت به نام '{HES.Text}' در تاریخ ویزیت {VDATE.Text} از قبل وجود دارد , امکان اضافه کردن اطلاعات تکراری نیست! ");
                _ = msgwin0.ShowDialog();
                return false;
            }
            else
            {
                bool HesVisitorChanged = _navigationManager?.CurrentRecord?.HES != null && HES.SelectedValue.ToString() != _navigationManager.CurrentRecord.HES;

                if (HesVisitorChanged && RowExisting != null)
                {
                    Msgwin msgwin0 = new Msgwin(true, $"این ویزیت به نام '{HES.Text}' در تاریخ ویزیت {VDATE.Text} از قبل وجود دارد , امکان ذخیره اطلاعات تکراری نیست! ");
                    _ = msgwin0.ShowDialog();
                    return false;
                }
            }

            if (RowExisting == null) //Insert
            {
                _ = dbms.DoExecuteSQL($@"INSERT INTO VISITORS_DAY (HES, CDATE, USERNAME, OKF,VDATE)
                                     VALUES (@HES, @CDATE, @USERNAME, @OKF,@VDATE);", masterRecord);
                RefreshAfterUpdate();
            }
            else
            {
                _ = dbms.DoExecuteSQL($@"UPDATE VISITORS_DAY SET HES=@HES , VDATE = @VDATE
                            WHERE HES = @HES AND VDATE = @VDATE", masterRecord);
            }

            return true;
        }

        public void DG_SUB_ReGetData()
        {
            if (!NewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<VISITORS_DAY_DTL>(@$"
                                    SELECT 
                                      C.NAME AS NAME_HES, 
                                      D.HES,
                                      D.VDATE,
                                      D.COUST_NO,
                                      D.RACTIVE, 
                                      D.TOPLACE, 
                                      D.CLASS, 
                                      D.CRT, 
                                      D.UID
                                    FROM dbo.CUST_HESAB AS C
                                    RIGHT JOIN dbo.VISITORS_DAY_DTL AS D
                                      ON C.hes = D.COUST_NO
                                    WHERE D.HES = @RouteName", new { RouteName = HES.SelectedValue }).ToList();

                VISIT_DAY_DATA?.Clear();
                foreach (var item in QRE_LST)
                    VISIT_DAY_DATA?.Add(item);
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
            if (!(combo.DataContext is VISITORS_DAY_DTL currentRow)) return;

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

            if (!(combo.DataContext is VISITORS_DAY_DTL currentRow)) return;

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

            if (string.IsNullOrEmpty(HES.SelectedValue?.ToStringNullSafe()))
            {
                universControl.PopNotifyShow($"ویزیتور نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }

            if (HES.SelectedValue is not null)
            {
                //if (CL_HESABDARI.ISTAF(HES.SelectedValue.ToString()))
                //{
                //    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                //    msgwin.ShowDialog();
                //    HES.SelectedValue = null;
                //}
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
            if (NewRecord || VISIT_DAY_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Visitory.R_LIST_VISIT_DAY.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["HES_PARAM"] = HES.SelectedValue;
            report["VDATE"] = Convert.ToInt64(VDATE.Text.ToRawTarikh());

            (report.GetComponentByName("VISITOR_TXT") as StiText).Text = HES.Text;
            (report.GetComponentByName("DATEEMROOZ") as StiText).Text = Tarikh.FullCurrentDate;

            new WINRPT(report, "ليست مشتريان ويزيتور در اين تاريخ").Show();
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.Invoke(() =>
            {
                DG_SUB.CellEditEnding -= DG_SUB_CellEditEnding;
                DG_SUB.RowEditEnding -= DG_SUB_RowEditEnding;
                DG_SUB.CancelEdit();
                DG_SUB.RowEditEnding += DG_SUB_RowEditEnding;
                DG_SUB.CellEditEnding += DG_SUB_CellEditEnding;
            });
        }

        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && DG_SUB.SelectedItem is not null)
            {
                if (CL_LMethods.IsNewPlaceHolder(DG_SUB, DG_SUB.SelectedItem))
                {
                    WAS_ROW_ITEM = ((VISITORS_DAY_DTL)DG_SUB.SelectedItem).Clone() as VISITORS_DAY_DTL;
                }
            }
        }
        private DataGridCellInfo? editingCellInfo;
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

            CURRENT_ROW_ITEMS = e.Row.Item as VISITORS_DAY_DTL;
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

            editingCellInfo = new DataGridCellInfo(e.Row.Item, e.Column);

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

        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.EditAction != DataGridEditAction.Commit || e.Cancel) return;
            if (e.Row.Item == null) { return; }
            var ROW = e.Row.Item as VISITORS_DAY_DTL;
            if (ROW is null) { return; }
            if (ConstructorRowDetector.IsPristine(e.Row.Item)) { DG_SUB_CANCEL_EDIT(); return; } //اگر سطر «دست‌نخورده» است، بدون خطا عمل کن
            if (!HeaderIsValid()) { return; }

            if (!BodyIsValid(ROW))
            {
                //DG_SUB_CANCEL_EDIT();
                DG_CANCEL_CURRENT(e, ROW);
                return;
            }

            ROW.HES = HES.SelectedValue.ToString();
            ROW.VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh());

            try
            {
                if (e.Row.IsNewItem)
                {
                    // بررسی وجود رکورد با کلید جدید
                    var duplicate = dbms.DoGetDataSQL<VISITORS_DAY_DTL>(
                        "SELECT TOP 1 * FROM dbo.VISITORS_DAY_DTL WHERE HES = @HES AND VDATE = @VDATE AND COUST_NO = @COUST_NO",
                        new { HES = ROW.HES, VDATE = ROW.VDATE, COUST_NO = ROW.COUST_NO }).FirstOrDefault();

                    if (duplicate != null)
                    {
                        DG_CANCEL_CURRENT(e, ROW);
                        universControl.PopNotifyShow("مشتری با این مشخصات قبلاً ثبت شده است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    // درج رکورد جدید
                    dbms.DoExecuteSQL(@$"
                        INSERT INTO VISITORS_DAY_DTL(HES, COUST_NO, VDATE, CDATE, RACTIVE, CLASS, UID)
                        VALUES(@HES, @COUST_NO, @VDATE, @CDATE, @RACTIVE, @CLASS, @UID)",
                        new
                        {
                            HES = ROW.HES,
                            COUST_NO = ROW.COUST_NO,
                            VDATE = ROW.VDATE,
                            CDATE = DateTime.Now,
                            RACTIVE = ROW.RACTIVE,
                            CLASS = ROW.CLASS,
                            UID = Baseknow.USERCOD
                        });
                }
                else
                {
                    // فقط آپدیت اگر کلید تغییر نکرده
                    dbms.DoExecuteSQL(@$"UPDATE dbo.VISITORS_DAY_DTL
                                        SET RACTIVE = @RACTIVE, TOPLACE = @TOPLACE, CLASS = @CLASS
                        WHERE HES = @HES AND VDATE = @VDATE AND COUST_NO = @COUST_NO",
                        new
                        {
                            HES = ROW.HES,
                            VDATE = ROW.VDATE,
                            COUST_NO = ROW.COUST_NO,
                            RACTIVE = ROW.RACTIVE,
                            TOPLACE = ROW.TOPLACE,
                            CLASS = ROW.CLASS
                        });
                }
            }
            catch (SqlException ex)
            {
                DG_CANCEL_CURRENT(e, ROW);

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "نام مشتری تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در ذخیره سطر").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
            }


        }

        private void DG_CANCEL_CURRENT(DataGridRowEditEndingEventArgs e, VISITORS_DAY_DTL? ROW)
        {
            e.Cancel = true;

            var DG = DG_SUB;
            DG.Dispatcher.BeginInvoke(new Action(() =>
            {
                //DG.CellEditEnding -= DG_SUB_CellEditEnding;
                //DG.RowEditEnding -= DG_SUB_RowEditEnding;

                DG.SelectedItem = ROW;
                DG.ScrollIntoView(ROW);
                if (editingCellInfo.HasValue)
                    DG.CurrentCell = editingCellInfo.Value;
                else
                    DG.CurrentCell = new DataGridCellInfo(ROW, DG.Columns[0]);
                DG.BeginEdit();

                //DG.RowEditEnding += DG_SUB_RowEditEnding;
                //DG.CellEditEnding += DG_SUB_CellEditEnding;

            }), System.Windows.Threading.DispatcherPriority.Background);
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
                        WAS_ROW_ITEM = ((VISITORS_DAY_DTL)DG_SUB.SelectedItem).Clone() as VISITORS_DAY_DTL;
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

        private void RACTIVE_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            DG_SUB.BeginEdit();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            DG_SUB.BeginEdit();
        }

        private void BTN_GETMASIR_Click(object sender, RoutedEventArgs e)
        {
            if (HES.SelectedValue != null && !NewRecord)
            {
                var win = new WIN_VISITSELECT(HES.SelectedValue.ToString());
                bool? ok = win.ShowDialog();
                if (ok == true && win.SelectedVisit != null && !string.IsNullOrEmpty(win.SelectedVisit.ROUTE_NAME))
                {
                    // دادهٔ انتخابی را نمایش/استفاده کنید
                    var MyROUTE_NAME = win.SelectedVisit.ROUTE_NAME;

                    var RST = dbms.DoGetDataSQL<VISIT_ROUTE_DTL>($"SELECT COUST_NO, RACTIVE, CLASS FROM  dbo.Visit_route_dtl WHERE (RACTIVE = 1 AND ROUTE_NAME = @ROUTE_NAME", new { ROUTE_NAME = MyROUTE_NAME }).ToList();

                    var existing = dbms.DoGetDataSQL<string>(
                                    @"SELECT COUST_NO
                                      FROM   dbo.VISITORS_DAY_DTL
                                      WHERE  HES = @HES AND VDATE = @VDATE", new { HES = HES.SelectedValue, VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh()) }).ToHashSet();

                    bool AnyDuplicat = false;

                    if (RST.Any())
                    {
                        const string sql = @"
                                             INSERT INTO dbo.VISITORS_DAY_DTL
                                                    (HES,       VDATE,  COUST_NO,  CDATE, RACTIVE,   CLASS,  TOPLACE)
                                             VALUES (@HES,      @VDATE, @COUST_NO, @CDATE, 1,        @CLASS, 0);";

                        foreach (var rc in RST)
                        {
                            if (existing.Contains(rc?.COUST_NO))
                            {
                                AnyDuplicat = true;
                                continue; // رد می‌کنیم چون تکراری است
                            }

                            dbms.DoExecuteSQL(sql, new
                            {
                                HES = HES.SelectedValue,      // ویزیتور جاری
                                VDATE = Convert.ToInt64(VDATE.Text.ToRawTarikh()),        // 8-digit Persian date as BIGINT
                                COUST_NO = rc.COUST_NO,  // حساب مشتری
                                CDATE = DateTime.Now,        // وقت ایجاد رکورد
                                CLASS = rc.CLASS,     // کلاس مسیر (ممکن است NULL باشد)
                            });
                        }

                        DG_SUB_ReGetData();

                        if (AnyDuplicat)
                        {
                            universControl.PopNotifyShowUp("لیست مشتریان از مسیر ویزیت انتخاب شده بارگذاری شد , اما یکسری مشتری از قبل اضافه شده بودند.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                        }
                        else
                        {
                            universControl.PopNotifyShowUp("لیست مشتریان از مسیر ویزیت انتخاب شده بارگذاری شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                        }
                    }
                }
            }
        }
    }
}
