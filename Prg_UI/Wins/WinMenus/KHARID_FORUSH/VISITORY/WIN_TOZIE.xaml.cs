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
using Syncfusion.Data.Extensions;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Wins.WinOther;
using static Interfaces.INavigator;
using Dapper;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Stimulsoft.Data.Expressions.NCalc;
using Microsoft.VisualBasic;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using System.Windows.Controls.Primitives;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    public partial class WIN_TOZIE : Window, ISearchableWindow
    {
        public WIN_TOZIE()
        {
            InitializeComponent();

            this.DataContext = this;
        }

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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        private NavigationManager<TOZIE> _navigationManager;
        public ObservableCollection<TOZIE_SUB> TOZIE_SUB_DATA { get; set; } = new ObservableCollection<TOZIE_SUB>();

        #region MyRegion
        public class TDRIVER_COMBO
        {
            //نام راننده
            public string? TDRIVER { get; set; }
        }
        public class TMAMUR_COMBO
        {
            //مامور توزيع
            public string? TMAMUR { get; set; }
        }
        public class TCITY_COMBO
        {
            //مامور توزيع
            public string? TCITY { get; set; }
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

                ////DG_SUB.IsReadOnly = !ican;

                TID.IsReadOnly = !ican;
                TDATE.IsReadOnly = !ican;

                TDRIVER.IsEnabled = ican;
                TMAMUR.IsEnabled = ican;
                TCITY.IsEnabled = ican;

                BTN_SAVE.IsEnabled = ican;
                BTN_HAVLAH_KALA.IsEnabled = ican;
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
                        //if (DG.CurrentColumn != null)
                        //{
                        //    int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                        //    bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                        //    bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                        //    if (isLastColumn)
                        //    {
                        //        // If it's the last column, move focus to the first cell of next row
                        //        if (isLastRow)
                        //        {
                        //            // Add focus to new row if needed
                        //            DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                        //            DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DG_SUB_DEF_INDEX_COL]);

                        //            Dispatcher.BeginInvoke(new Action(() =>
                        //            {
                        //                DG.BeginEdit();
                        //            }), DispatcherPriority.Background);

                        //            //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                        //            var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                        //            if (focusedWindow != null)
                        //            {
                        //                Dispatcher.BeginInvoke(new Action(() =>
                        //                {
                        //                    focusedWindow.Activate();
                        //                    focusedWindow.Focus();
                        //                }), DispatcherPriority.Background);
                        //            }

                        //            return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                        //        }
                        //    }
                        //}
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
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VISIT_ROUTE = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();

            _navigationManager = new NavigationManager<TOZIE>(
                dbms,
                x => x?.TID?.ToString(),
                $"SELECT * FROM TOZIE ORDER BY CRT",
                x => $"SELECT * FROM TOZIE WHERE TID = {x?.TID?.ToString()} ", default);

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            if (!NewRecord)
            {
                AllowEdits = false;
            }

            CL_LMethods.SetTabIndexes(
            TDATE,    /*تاریخ برگه*/
            TDRIVER, /*نام راننده*/
            TMAMUR, /*مامور توضیع*/
            TCITY, /*شهر*/
            BTN_SAVE,
            BTN_HAVLAH_KALA
            );

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(TOZIE record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<TOZIE>($"SELECT * FROM TOZIE  WHERE TID = {TID.Text} ").FirstOrDefault();
                record = itemtoadd;
                NewRecord = false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(TOZIE HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, TOZIE_SUB_DATA);
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

                TID.Text = HEADER_FAC.TID.ToStringNullSafe();
                TDATE.Text = HEADER_FAC.TDATE.ToStringNullSafe();
                TDRIVER.SelectedValue = HEADER_FAC.TDRIVER.ToStringNullSafe(); TDRIVER.Items.Refresh();
                TMAMUR.SelectedValue = HEADER_FAC.TMAMUR.ToStringNullSafe(); TMAMUR.Items.Refresh();
                TCITY.SelectedValue = HEADER_FAC.TCITY.ToStringNullSafe(); TCITY.Items.Refresh();
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر

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
            if (selectedItem is TOZIE item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.TID.Equals(item.TID));
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

            var CURRENT_HEADER = dbms.DoGetDataSQL<TOZIE>($"SELECT * FROM TOZIE  WHERE TID = {TID.Text} ").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //نام راننده
            TDRIVER.ItemsSource = dbms.DoGetDataSQL<TDRIVER_COMBO>($"SELECT TDRIVER FROM TOZIE GROUP BY TDRIVER ORDER BY TDRIVER").ToList();

            //مامور توزيع
            TMAMUR.ItemsSource = dbms.DoGetDataSQL<TMAMUR_COMBO>($"SELECT TMAMUR FROM TOZIE GROUP BY TMAMUR ORDER BY TMAMUR").ToList();

            //شهر
            TCITY.ItemsSource = dbms.DoGetDataSQL<TCITY_COMBO>("SELECT TCITY FROM TOZIE GROUP BY TCITY ORDER BY TCITY").ToList();
        }
        private void MakeDefaultFocuseReady()
        {
            TDATE.Focus();
            TDATE.SelectAll();
        }
        private void DataGridActivation()
        {
            //if (NewRecord)
            //{
            //    DG_SUB.IsReadOnly = true;
            //}
            //else
            //{
            //    DG_SUB.IsReadOnly = false;
            //}
        }
        private void ClearFreshAll()
        {
            NewRecord = true;
            ESLAH.IsEnabled = false;
            Command106.IsEnabled = false;

            TID.Text = "0";
            TDATE.Text = Tarikh.FullCurrentDate;
            TDRIVER.SelectedItem = null; TDRIVER.Items.Refresh();
            TMAMUR.SelectedItem = null; TMAMUR.Items.Refresh();
            TCITY.SelectedItem = null; TCITY.Items.Refresh();
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            TOZIE_SUB_DATA?.Clear(); //دیتاگرید فاکتور فروش
            AllowEdits = true;
            ////DG_SUB.IsReadOnly = true; // Locked

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

            if (!Tarikh.IsValidedDate(TDATE.Text.ToRawTarikh()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
            }
            else
            {
                if (!Tarikh.IsSyncedDateNow(TDATE.Text, Baseknow.CTL_DT ?? false))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                }
            }

            var _TID_ = TID.Text.Trim();
            if (string.IsNullOrEmpty(_TID_) || string.IsNullOrWhiteSpace(_TID_))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره برگه نمیتواند خالی باشد." });
            }
            else if (CL_LMethods.IsNumeric(_TID_) && Convert.ToDouble(_TID_) <= 0)
            {
                if (!_navigationManager.IsNewRecord) //اگر کاربر اومده مقدار رو به صفر تغییر داده توی رکوردی که از قبل ثبت بوده !
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شماره برگه نمیتواند صفر یا منفی باشد." });
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

        private bool BodyIsValid(TOZIE_SUB TheRow)
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

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }
        public Visual I_AM_VISIT_ROUTE { get; private set; }
        public TOZIE_SUB? CURRENT_ROW_ITEMS { get; private set; }
        public TOZIE_SUB? WAS_ROW_ITEM { get; private set; } = new TOZIE_SUB();

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
                    new Msgwin(false, $"نمیتوان اطلاعات تکراری تعریف کرد").Show();
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

            ////this.DG_SUB.IsReadOnly = false;
            ESLAH.IsEnabled = true;

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (TOZIE_SUB_DATA.Count == 0)
            {
                //GetFocusOnDefaultCell();
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
                //CL_HESABDARI.TR("TOZIE", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12

                AllowDeletions = true;
                AllowEdits = true;
                ////DG_SUB.IsReadOnly = false; // UnLocked
            }
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (NewRecord || DG_SUB.IsEnabled == false || !BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (TOZIE_SUB_DATA.Count > 0)
            {
                if (_navigationManager.IsNewRecord) { return; }

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
                    tableName: "تنظیم لیست دستی توضیع",
                    recordId: TID.Text,
                    oldValue: "",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


                if (TOZIE_SUB_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
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

                        var _id_ = item.GetType().GetProperty("TID").GetValue(item);
                        var _number_ = item.GetType().GetProperty("NUMBER").GetValue(item);

                        if (_id_ != null)
                        {
                            try
                            {
                                dbms.DoExecuteSQL($@"DELETE FROM dbo.TOZIE_SUB WHERE TID = {_id_} AND NUMBER = {_number_}");

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
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.TOZIE WHERE TID = {TID.Text} ");

                            //ClearFreshAll();
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
                                new Msgwin(false, "این لیست توضیع دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
            var masterRecord = new
            {
                TID = Convert.ToInt32(TID.Text),
                TDATE = Convert.ToInt64(TDATE.Text.ToRawTarikh()),
                TDRIVER = TDRIVER.Text.Trim(),
                TCITY = TCITY.Text.Trim(),
                TMAMUR = TMAMUR.Text.Trim(),
                CDATE = DateTime.Now,
                USER_NAME = USER_NAME.Text,
                UID = Baseknow.USERCOD
            };

            var RowExisting = dbms.DoGetDataSQL<string?>($"SELECT 1 TID FROM TOZIE WHERE TID = @TID", new { TID = TID.Text }).FirstOrDefault();

            if (_navigationManager.IsNewRecord && RowExisting != null)
            {
                Msgwin msgwin0 = new Msgwin(false, $"این شماره برگه {TID.Text} از قبل وجود دارد , آنرا تغییر دهید");
                _ = msgwin0.ShowDialog();
                return false;
            }
            else
            {
                bool TIDChanged = _navigationManager?.CurrentRecord?.TID != null && _navigationManager?.CurrentRecord?.TID != Convert.ToInt32(TID.Text);

                if (TIDChanged && RowExisting != null)
                {
                    Msgwin msgwin0 = new Msgwin(false, $"این شماره برگه '{TID.Text}' از قبل وجود دارد , امکان ذخیره اطلاعات تکراری نیست!");
                    _ = msgwin0.ShowDialog();
                    return false;
                }
            }

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var tran = db.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int newTid = masterRecord.TID;

                        // 2) اگر TID داده نشده مقدار جدید تولید کن (با lock)
                        if (masterRecord?.TID == null || masterRecord?.TID == 0)
                        {
                            // lock کامل جدول برای جلوگیری از race
                            string maxTidSql = "SELECT ISNULL(MAX(TID),0)+1 FROM TOZIE WITH (TABLOCKX)";
                            newTid = db.ExecuteScalar<int>(maxTidSql, null, tran);
                        }
                        else
                        {
                            // بررسی وجود رکورد با همین TID
                            string existSql = "SELECT COUNT(*) FROM TOZIE WHERE TID = @TID";
                            var existCount = db.ExecuteScalar<int>(existSql, new { TID = masterRecord.TID }, tran);

                            if (existCount > 0)
                            {
                                // UPDATE
                                string updateSql = @"
                                        UPDATE TOZIE
                                           SET TDATE = @TDATE, TDRIVER = @TDRIVER, TCITY = @TCITY, TMAMUR = @TMAMUR,
                                               USER_NAME = @USER_NAME WHERE TID = @TID";
                                db.Execute(updateSql, new
                                {
                                    TID = masterRecord.TID,
                                    TDATE = masterRecord.TDATE,
                                    TDRIVER = masterRecord.TDRIVER,
                                    TCITY = masterRecord.TCITY,
                                    TMAMUR = masterRecord.TMAMUR,
                                    USER_NAME = masterRecord.USER_NAME,
                                }, tran);

                                tran.Commit();
                                universControl.PopNotifyShowUp($"رکورد با شماره {masterRecord.TID} با موفقیت بروزرسانی شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                                return true;
                            }
                        }

                        // اگر هنوز insert نشده (newTid تعیین شده)
                        string insertSql = @"
                            INSERT INTO TOZIE (TID, TDATE, TDRIVER, TCITY, TMAMUR, CDATE, USER_NAME, CRT, UID)
                            VALUES (@TID, @TDATE, @TDRIVER, @TCITY, @TMAMUR, @CDATE, @USER_NAME, GETDATE(), @UID)";
                        db.Execute(insertSql, new
                        {
                            TID = newTid,
                            TDATE = masterRecord.TDATE,
                            TDRIVER = masterRecord.TDRIVER,
                            TCITY = masterRecord.TCITY,
                            TMAMUR = masterRecord.TMAMUR,
                            CDATE = masterRecord.CDATE,
                            USER_NAME = masterRecord.USER_NAME,
                            UID = masterRecord.UID
                        }, tran);

                        tran.Commit();

                        TID.Text = newTid.ToString();
                        RefreshAfterUpdate();
                        universControl.PopNotifyShowUp($"رکورد جدید با شماره {newTid} با موفقیت ثبت شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                        return true;
                    }
                    catch (SqlException ex)
                    {
                        if (tran.Connection != null)
                            tran.Rollback();

                        if (ex.Message.Contains("PRIMARY KEY") || ex.Message.Contains("UNIQUE"))
                            universControl.PopNotifyShowUp($"شماره برگه تکراری است", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        else
                            universControl.PopNotifyShowUp($"خطا در ذخیره اطلاعات", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);

                        return false;
                    }
                    catch (Exception ex)
                    {
                        if (tran.Connection != null)
                            tran.Rollback();
                        universControl.PopNotifyShowUp($"خطا در ذخیره اطلاعات", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                        return false;
                    }
                }
            }
        }

        public void DG_SUB_ReGetData()
        {
            if (!NewRecord)
            {
                var QRE_LST = dbms.DoGetDataSQL<TOZIE_SUB>(@$"
                                    SELECT dbo.TOZIE_SUB.TID,
                                           dbo.TOZIE_SUB.NUMBER,
                                           dbo.HEAD_LST.DATE_N,
                                           dbo.HEAD_LST.CUST_NO,
                                           dbo.CUST_HESAB.NAME AS NAME_HES
                                    FROM dbo.TOZIE_SUB
                                        INNER JOIN dbo.HEAD_LST
                                            ON dbo.TOZIE_SUB.NUMBER = dbo.HEAD_LST.NUMBER
                                        LEFT OUTER JOIN dbo.CUST_HESAB
                                            ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                                    WHERE (dbo.HEAD_LST.TAG = @TAG) AND (dbo.TOZIE_SUB.TID = @TID) ORDER BY dbo.HEAD_LST.NUMBER"
                                    , new { TAG = 2, TID = TID.Text }).ToList();

                TOZIE_SUB_DATA?.Clear();
                foreach (var item in QRE_LST)
                    TOZIE_SUB_DATA?.Add(item);
            }
        }

        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || TOZIE_SUB_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Visitory.tozie_dasti.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["TID_PARAM"] = TID.Text;
            //report["ROUTE_PARAM2"] = ROUTE_NAME.Text.FixPersianChars();
            //report["HES_PARAM"] = HES.SelectedValue;

            //(report.GetComponentByName("DATEEMROOZ") as StiText).Text = Tarikh.FullCurrentDate;

            new WINRPT(report, "لیست توضیع : گزارش مامور پخش").Show();
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

        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e == null || !(e.Row.Item is TOZIE_SUB rowItem)) return;
            if (rowItem == null) return;
            if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
            var view = DG_SUB.Items as IEditableCollectionView;
            if (view.IsAddingNew) { return; }

            WAS_ROW_ITEM = rowItem.Clone() as TOZIE_SUB;

            //// اگر قبلاً ذخیره نشده، اضافه کن
            //if (!WasRowKeys.ContainsKey(rowItem))
            //{
            //    WasRowKeys[rowItem] = (
            //        rowItem.ROUTE_NAME?.Trim() ?? string.Empty,
            //        rowItem.COUST_NO?.Trim() ?? string.Empty
            //    );
            //}
        }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
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

            CURRENT_ROW_ITEMS = e.Row.Item as TOZIE_SUB;
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
            }

        }
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            //if (!HeaderIsValid()) { return; }

            //var ROW = e.Row.Item as TOZIE_SUB;
            //if (e.Row.Item == null || ROW is null) { return; }

            //if (!BodyIsValid(ROW))
            //{
            //    DG_SUB_CANCEL_EDIT();
            //    return;
            //}

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
            //if (NowIsReady && DG_SUB.SelectedItem != null)
            //{
            //    if (!(e is null) && DG_SUB.SelectedItem is not null)
            //    {
            //        if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            //        {
            //            WAS_ROW_ITEM = ((TOZIE_SUB)DG_SUB.SelectedItem).Clone() as TOZIE_SUB;
            //        }
            //    }
            //}
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
                }
                catch { }

                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }
        }

        private void BTN_HAVLAH_KALA_Click(object sender, RoutedEventArgs e)
        {
            if (!NewRecord)
            {
                var win = new WIN_TOZIESELECT(Convert.ToInt32(TID.Text));
                bool? ok = win.ShowDialog();
                DG_SUB_ReGetData();
                universControl.PopNotifyShowUp("لیست بارگذاری شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }
        }

        private void TDRIVER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is ComboBox MyComboBox)
            {
                string thevalue = TDRIVER.Text;
                if (MyComboBox.ItemsSource == null)
                {
                    MyComboBox.ItemsSource = new List<TDRIVER_COMBO>();
                }
                if (!((List<TDRIVER_COMBO>)MyComboBox.ItemsSource).Any(item => item?.TDRIVER == thevalue))
                {
                    ((List<TDRIVER_COMBO>)MyComboBox.ItemsSource).Add(new TDRIVER_COMBO { TDRIVER = thevalue });
                }
                MyComboBox.SelectedValue = thevalue;
                MyComboBox.Items.Refresh();
            }
        }
        private void TMAMUR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is ComboBox MyComboBox)
            {
                string thevalue = TDRIVER.Text;
                if (MyComboBox.ItemsSource == null)
                {
                    MyComboBox.ItemsSource = new List<TMAMUR_COMBO>();
                }
                if (!((List<TMAMUR_COMBO>)MyComboBox.ItemsSource).Any(item => item?.TMAMUR == thevalue))
                {
                    ((List<TMAMUR_COMBO>)MyComboBox.ItemsSource).Add(new TMAMUR_COMBO { TMAMUR = thevalue });
                }
                MyComboBox.SelectedValue = thevalue;
                MyComboBox.Items.Refresh();
            }
        }
        private void TCITY_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is ComboBox MyComboBox)
            {
                string thevalue = TDRIVER.Text;
                if (MyComboBox.ItemsSource == null)
                {
                    MyComboBox.ItemsSource = new List<TCITY_COMBO>();
                }
                if (!((List<TCITY_COMBO>)MyComboBox.ItemsSource).Any(item => item?.TCITY == thevalue))
                {
                    ((List<TCITY_COMBO>)MyComboBox.ItemsSource).Add(new TCITY_COMBO { TCITY = thevalue });
                }
                MyComboBox.SelectedValue = thevalue;
                MyComboBox.Items.Refresh();
            }
        }

        private void Command106_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || TOZIE_SUB_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Visitory.TOZIE_FROOSH_ANBARS_HAVALA.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["TID_PARAM"] = TID.Text;
            //report["ROUTE_PARAM2"] = ROUTE_NAME.Text.FixPersianChars();
            //report["HES_PARAM"] = HES.SelectedValue;

            //(report.GetComponentByName("DATEEMROOZ") as StiText).Text = Tarikh.FullCurrentDate;

            new WINRPT(report, "حواله از لیست توضیع").Show();
        }

        private void BTN_FACTOS_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || TOZIE_SUB_DATA.Count == 0) { return; }

            List<double?> NUMBERS = TOZIE_SUB_DATA.Select(x => x.NUMBER).ToList();

            string NumberListLine = string.Join(",", NUMBERS);

            var report = new StiReport();
            using var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.Visitory.TOZIE_FACTORS.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            if (Baseknow.TFSAZMAN != "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = true; //کل مانده حساب
                //(report.GetComponentByName("MANDG") as StiText).Enabled = true;
            }
            else
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false; //کل مانده حساب
                (report.GetComponentByName("MANDG") as StiText).Enabled = false; //مانده حساب قبلی
            }

            if (Baseknow.TFCODE_E != "" & !IsNull(Baseknow.TFCODE_E)) //SELECT TFCODE_E,ARSESH FROM SAZMAN
            {
                //فیلد خدمات
                (report.GetComponentByName("Label179") as StiText).Text = Baseknow.TFCODE_E;
            }

            if (Baseknow.TFSAZMAN == "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
                (report.GetComponentByName("MANDG") as StiText).Enabled = false;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 2, 1) == "5")
            {
                (report.GetComponentByName("Label197") as StiText).Enabled = false;
            }

            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) != "5")
            {
                (report.GetComponentByName("TKHN") as StiText).Enabled = false;
                (report.GetComponentByName("Line219") as StiHorizontalLinePrimitive).Enabled = false;
            }
            else
            {
                (report.GetComponentByName("Label180") as StiText).Text = " تخفيف:";
            }

            (report.GetComponentByName("USERNAME") as StiText).Text = Baseknow.UUSER;

            report.Dictionary.Variables.Add("NUMBERS_PARAM", NumberListLine);

            new WINRPT(report, "فاکتور های فروش از لیست توضیع").Show();
        }
    }
}