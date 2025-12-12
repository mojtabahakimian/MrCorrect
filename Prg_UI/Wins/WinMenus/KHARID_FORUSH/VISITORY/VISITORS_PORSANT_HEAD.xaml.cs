using Dapper;
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
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for VISITORS_PORSANT_HEAD.xaml
    /// </summary>
    public partial class VISITORS_PORSANT_HEAD : Window, ISearchableWindow
    {
        // نویگیشن منیجر برای حرکت بین رکوردها
        private NavigationManager<VISITORS_PORSANT> _navigationManager;

        // کالکشن برای گرید جزئیات
        public ObservableCollection<VISITORS_PORSANT_KALA> SUB_DATA { get; } = new ObservableCollection<VISITORS_PORSANT_KALA>();

        public bool ChangeIsHappend { get; private set; } = false;
        public bool IsNewRecord => _navigationManager?.IsNewRecord ?? true;

        // برای نگهداری سطر جاری جهت ویرایش
        public VISITORS_PORSANT_KALA? CURRENT_ITEMS_ROW { get; private set; }
        public VISITORS_PORSANT_KALA? WAS_ROW_ITEM { get; private set; } = new();

        public VISITORS_PORSANT_HEAD(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
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

        public class HES_MODEL
        {
            public string HES { get; set; }
            public string NAME { get; set; }
        }

        public class CUST_COD_MODEL
        {
            public int CUST_COD { get; set; }
            public string CUSTKNAME { get; set; }
        }

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }


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

                VDATE.IsReadOnly = !ican;
                //CDATE.IsReadOnly = !ican;
                COMMENT.IsReadOnly = !ican;
                DG_SUB.IsReadOnly = !ican;

                HES.IsEnabled = ican;
                HES2.IsEnabled = ican;
                CUST_COD.IsEnabled = ican;
                BTN_SAVE.IsEnabled = ican;
            }
        }

        public string? ENTERED_VALUE_ROW { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "VISIT_POR", new WindowInteropHelper(this).Handle);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            // 2. پر کردن کمبوباکس‌ها
            FILL_ALL_COMBOBOXES();

            // 3. راه‌اندازی نویگیشن
            _navigationManager = new NavigationManager<VISITORS_PORSANT>(
                dbms,
                x => x.PORID.ToString(),
                "SELECT * FROM VISITORS_PORSANT ORDER BY PORID",
                x => $"SELECT TOP 1 * FROM VISITORS_PORSANT WHERE PORID = {x.PORID}",
                NUMBER_TO_OPEN
            );

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            _navigationManager.CanChangeRecord = CheckForUnsavedChanges;

            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            // 4. تنظیم فوکوس اولیه
            HES.Focus();
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
                        if (DG?.CurrentColumn != null && DG.SelectedItem != null)
                        {
                            // 1. جستجو برای پیدا کردن پنجره پیام (چه فعال باشد چه نباشد)
                            var messageWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is Prg_UI.HelperWins.Msgwin || w is Prg_UI.HelperWins.MsgListwin);
                            if (messageWindow != null)
                            {
                                try
                                {
                                    // استفاده از Dispatcher با اولویت Input برای اطمینان از اعمال فوکوس
                                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                    {
                                        // 2. اگر پنجره مینیمایز شده است، آن را به حالت عادی برگردان
                                        if (messageWindow.WindowState == WindowState.Minimized)
                                        {
                                            messageWindow.WindowState = WindowState.Normal;
                                        }

                                        // 3. آوردن پنجره به جلوترین حالت
                                        messageWindow.Activate();
                                        var was = messageWindow.Topmost;
                                        messageWindow.Topmost = true;  // موقتا روترین پنجره شود
                                        messageWindow.Topmost = was; // به حالت عادی برگردد (اختیاری)

                                        // 4. فوکوس نهایی
                                        messageWindow.Focus();
                                    }));
                                }
                                catch { }
                                // اگر این کد در رویداد دکمه‌ای مثل Enter است، اینجا ریترن می‌کنیم
                                return;
                            }

                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = DG.CurrentColumn?.SortMemberPath == "NAME_CODE";
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            isLastColumn = currentColumnIndex == (DG.Columns.Count - 1);

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Make sure next row exists before trying to select it
                                    if (DG.Items.Count > DG.SelectedIndex + 1)
                                    {
                                        DG.SelectedIndex++;

                                        // Verify the new selection is valid
                                        if (DG.SelectedItem != null)
                                        {
                                            DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[1]);

                                            Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                if (DG.SelectedItem != null)
                                                {
                                                    DG.BeginEdit();
                                                }
                                            }), DispatcherPriority.Background);
                                        }
                                    }
                                    return;
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

            if (!DG_SUB.IsKeyboardFocusWithin && !DG_SUB.IsFocused)
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
                    DataGridExtension.HandleKeyPress(sender, e, DG_SUB);
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
        private void FILL_ALL_COMBOBOXES()
        {
            // پر کردن لیست ویزیتورها (HES)
            // SQL from VBA: SELECT Visit_route.HES, CUST_HESAB.NAME ...
            string sqlHes = @"
                SELECT Visit_route.HES, CUST_HESAB.NAME 
                FROM Visit_route 
                INNER JOIN CUST_HESAB ON Visit_route.HES = CUST_HESAB.hes 
                GROUP BY Visit_route.HES, CUST_HESAB.NAME";
            var listHes = dbms.DoGetDataSQL<HES_MODEL>(sqlHes).ToList();
            HES.ItemsSource = listHes;
            HES2.ItemsSource = HES.ItemsSource;

            // پر کردن گروه مشتری (CUST_COD)
            string sqlCust = @"
                SELECT CUST_COD, CUSTKNAME FROM CUSTKIND 
                UNION 
                SELECT 0 AS CUST_COD, 'بدون گروه(همه)' AS CUSTKNAME";
            CUST_COD.ItemsSource = dbms.DoGetDataSQL<CUST_COD_MODEL>(sqlCust).ToList();
        }


        private void OnCurrentRecordChanged(VISITORS_PORSANT record)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                DG_SUB.IsReadOnly = true;
                Command23.IsEnabled = false;

                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    return;
                }
            }
            else if (record == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                try
                {
                    PORID.Text = record.PORID.ToString();
                    VDATE.Text = record.VDATE;
                    HES.SelectedValue = record.HES;
                    HES2.SelectedValue = record.HES;
                    CUST_COD.SelectedValue = record.CUST_COD;
                    //CDATE.Text = record.CDATE;
                    COMMENT.Text = record.COMMENT;
                    USERNAME.Text = record.USERNAME;

                    AllowEdits = false;
                    Command23.IsEnabled = true;
                    BTN_SAVE.IsEnabled = false;
                    Command23.IsEnabled = false;

                    // بارگذاری گرید جزئیات
                    DG_SUB_ReGetData();
                }
                catch (Exception ex)
                {
                    // Log and show error
                    new Msgwin(false, $"خطا در بارگذاری اطلاعات").ShowDialog();
                }
            }

            ChangeIsHappend = false;
        }
        private bool OnInsertRecord(VISITORS_PORSANT record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<VISITORS_PORSANT>($"SELECT TOP 1 * FROM VISITORS_PORSANT  WHERE PORID = {PORID.Text} ").FirstOrDefault();
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
            var CURRENT_HEADER = dbms.DoGetDataSQL<VISITORS_PORSANT>($"SELECT * FROM VISITORS_PORSANT WHERE PORID = {PORID.Text} ").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            if (selectedItem is VISITORS_PORSANT item)
            {
                var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.PORID.Equals(Convert.ToInt32(item.PORID)));

                if (itemfound != null)
                {
                    _navigationManager.IsNewRecord = false;

                    int idx = _navigationManager.RecordsData.IndexOf(itemfound);

                    if (idx < 0)
                    {
                        new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                        return;
                    }

                    _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                }
                else
                {
                    new Msgwin(false, "رکورد مورد نظر در لیست بارگذاری شده یافت نشد.").Show();
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "PORID", PropertyType = typeof(int) },
                new SearchableProperty { DisplayName = "ویزیتور", PropertyPath = "HES", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "VDATE", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "گروه مشتری", PropertyPath = "CUST_COD", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "توضیحات", PropertyPath = "COMMENT", PropertyType = typeof(string) },
            };
        }
        #endregion


        public void DG_SUB_ReGetData()
        {
            if (string.IsNullOrEmpty(PORID.Text) || PORID.Text == "0")
            {
                SUB_DATA.Clear();
                return;
            }

            // فرض بر این است که نام جدول جزئیات VISITORS_PORSANT_KALAF است
            // و شامل فیلدهای CODE, PORSANT, MABL است که به STUF_DEF جوین می‌شود
            string sql = @"
                SELECT D.CODE,
                       D.ID,
                       D.PORID,
                       D.PORSANT,
                       D.CRT,
                       D.UID,
                       S.NAME AS NAME_CODE
                FROM VISITORS_PORSANT_KALA D
                    LEFT JOIN STUF_DEF S
                        ON D.CODE = S.CODE
                WHERE D.PORID = @PORID";

            var rows = dbms.DoGetDataSQL<VISITORS_PORSANT_KALA>(sql, new { PORID = int.Parse(PORID.Text) }).ToList();

            SUB_DATA.Clear();
            foreach (var item in rows)
            {
                SUB_DATA.Add(item);
            }
        }

        private bool CheckForUnsavedChanges()
        {
            if (!ChangeIsHappend) return true;

            var res = new Msgwin(true, "تغییرات ذخیره نشده است. آیا ذخیره می‌کنید؟", default, default,
                new MSGCAPTIONMODEL { YES_CAPTION = "ذخیره و ادامه", NO_CAPTION = "خیر" }).ShowDialog();

            if (res == true)
            {
                BTN_SAVE_Click(null, null);
                return !ChangeIsHappend; // اگر ذخیره موفق بود true برمی‌گرداند
            }
            return res == false; // اگر خیر زد اجازه دهد، اگر بست اجازه ندهد
        }

        private void ApplyDataGridItems()
        {
            if (DG_SUB.Items is IEditableCollectionView editableCollectionView)
            {
                if (editableCollectionView.IsAddingNew)
                {
                    editableCollectionView.CancelNew(); // discard the new item
                }
                if (editableCollectionView.IsEditingItem)
                {
                    editableCollectionView.CommitEdit(); // commit the edit transaction
                }
            }
        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                DG_SUB_CANCEL_EDIT(); ApplyDataGridItems(); DG_SUB.Items.Refresh();
                return;
            }

            if (!HeaderIsValid()) return;

            try
            {
                DoCmdHeaderSave();
            }
            catch (SqlException ex)
            {
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

            // فعال‌سازی گرید و دکمه‌ها پس از ذخیره موفق (معادل Form_BeforeUpdate)
            DG_SUB.IsReadOnly = false;
            Command23.IsEnabled = true;

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            ChangeIsHappend = false;
        }

        private bool HeaderIsValid()
        {
            if (string.IsNullOrEmpty(HES.SelectedValue?.ToString()))
            {
                new Msgwin(false, "لطفا ویزیتور را انتخاب کنید").ShowDialog();
                return false;
            }
            if (!Tarikh.IsValidedDate(VDATE.Text.ToRawTarikh()))
            {
                new Msgwin(false, "تاریخ نامعتبر است").ShowDialog();
                return false;
            }
            return true;
        }

        private void DoCmdHeaderSave()
        {
            if (PORID.Text == "0" || string.IsNullOrWhiteSpace(PORID.Text))
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        db.Execute("SELECT TOP 1 PORID FROM dbo.VISITORS_PORSANT WITH (TABLOCKX, HOLDLOCK)", null, transaction);

                        var MaxID = db.Query<double?>($"SELECT Max(VISITORS_PORSANT.PORID) AS MaxOfPORID FROM VISITORS_PORSANT", null, transaction).FirstOrDefault();
                        if (MaxID == 0 || ReferenceEquals(MaxID, null))
                        {
                            PORID.Text = "1";
                            PORID.UpdateLayout();
                        }
                        else
                        {
                            PORID.Text = Convert.ToDouble(MaxID + 1).ToString();
                            PORID.UpdateLayout();
                        }

                        if (string.IsNullOrWhiteSpace(PORID.Text) || PORID.Text == "0")
                        {
                            throw new Exception("خطایی رخ داده , شماره صفر است !");
                        }

                        string insertSql = @"
                                INSERT INTO VISITORS_PORSANT (PORID, VDATE, HES, CUST_COD, COMMENT, UID, OKF,CDATE,USERNAME)
                                VALUES (@PORID, @VDATE, @HES, @CUST_COD, @COMMENT, @UID, 1,GETDATE(),@USERNAME)";

                        db.Execute(insertSql, new
                        {
                            PORID = PORID.Text,
                            VDATE = Convert.ToInt32(VDATE.Text.ToRawTarikh()),
                            HES = HES.SelectedValue,
                            CUST_COD = CUST_COD.SelectedValue ?? 0,
                            COMMENT = COMMENT.Text,
                            USERNAME = USERNAME.Text,
                            UID = Baseknow.USERCOD
                        }, transaction);

                        transaction.Commit();
                        db?.Close();
                        _navigationManager.IsNewRecord = false;
                        RefreshAfterUpdate();
                    }
                }
            }
            else
            {
                string updateSql = @"
                                UPDATE VISITORS_PORSANT 
                                SET VDATE = @VDATE, HES = @HES, CUST_COD = @CUST_COD, COMMENT = @COMMENT
                                WHERE PORID = @PORID";

                dbms.DoExecuteSQL(updateSql, new
                {
                    PORID = int.Parse(PORID.Text),
                    VDATE = VDATE.Text.ToRawTarikh(),
                    HES = HES.SelectedValue,
                    CUST_COD = CUST_COD.SelectedValue ?? 0,
                    COMMENT = COMMENT.Text
                });
            }
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !isVisible) return;

            if (_navigationManager.IsNewRecord) return;

            if (SUB_DATA.Count > 0)
            {
                try
                {
                    var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(DG_SUB.ItemsSource);
                    if (view != null)
                    {
                        if (view.IsAddingNew && view.CanCancelEdit) return; // جلوگیری از تداخل با درج
                        if (view.IsEditingItem && view.CanCancelEdit) return; // جلوگیری از تداخل با ویرایش

                        DG_SUB.CommitEdit(DataGridEditingUnit.Cell, true);
                        DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                    }
                }
                catch { }
            }

            var msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true) return;

            // ثبت لاگ (Fire and forget)
            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: LABEL_HEADER.Content.ToString(),
                    recordId: PORID.Text,
                    oldValue: "",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            if (SUB_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
            {

                bool isDeletedSomething = false;
                var errorMessages = new List<MsgModel>();
                int selectedCount = DG_SUB.SelectedItems.Count;

                for (int i = 0; i < selectedCount; i++)
                {
                    var item = DG_SUB.SelectedItems[i] as VISITORS_PORSANT_KALA;

                    if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item)) continue;

                    if (item != null)
                    {
                        try
                        {
                            dbms.DoExecuteSQL($"DELETE FROM VISITORS_PORSANT_KALA WHERE PORID = {PORID.Text} AND CODE = N'{item.CODE}' AND PORSANT = {item.PORSANT}");
                            isDeletedSomething = true;
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 547)
                                errorMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                            else
                                errorMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                        }
                        catch
                        {
                            errorMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                        }
                    }
                }

                if (errorMessages.Any())
                {
                    errorMessages = errorMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, errorMessages).ShowDialog();
                }
                else if (isDeletedSomething)
                {
                    DG_SUB_ReGetData();
                }
            }
            else
            {
                try
                {
                    dbms.DoExecuteSQL("DELETE FROM VISITORS_PORSANT WHERE PORID = " + PORID.Text);
                    _navigationManager?.DeleteCurrentRecord();
                }
                catch (SqlException ex)
                {
                    if (e != null) e.Handled = true;

                    if (ex.Number == 547)
                        new Msgwin(false, "این برگه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                    else
                        new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog();
                }
                catch
                {
                    new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog();
                }
            }
        }

        private bool BodyIsValid(VISITORS_PORSANT_KALA TheRow)
        {
            // 1. اعتبارسنجی بصری (WPF Validation Rules)
            // بررسی اینکه آیا کنترل‌های گرید قرمز شده‌اند یا خیر
            var errors = (from object i in DG_SUB.ItemsSource
                          let c = DG_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده در سطرها معتبر نیستند", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(TheRow?.CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد کالا نمیتواند خالی باشد" });
            }

            if (TheRow?.PORID == null || TheRow?.PORID <= 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شناسه پورسانت معتبر وارد نشده است" });
            }

            if (TheRow?.PORSANT == null || TheRow?.PORSANT < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد پورسانت نمیتواند منفی باشد" });
            }

            // 3. نمایش خطاها
            if (ErrosMessages.Count > 0)
            {
                // حذف پیام‌های تکراری و نمایش در پنجره لیست خطاها
                ErrosMessages = ErrosMessages
                    .Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();

                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        // سایر رویدادهای گرید برای مدیریت UX (مشابه فایل قبلی)
        private void DG_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_SUB.SelectedItem is VISITORS_PORSANT_KALA item)
                CURRENT_ITEMS_ROW = item;
        }

        private void DG_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && DG_SUB.SelectedItem != null)
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
                BTN_DELETE_Click(default, default);
                e.Handled = true;
            }
        }

        private void DG_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }
        private void DG_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e) { }
        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) { }

        private void DG_SUB_ContextMenuOpening(object sender, ContextMenuEventArgs e) { }
        private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) { }
        private void DG_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { }

        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ITEMS_ROW = e.Row.Item as VISITORS_PORSANT_KALA;
            #endregion

            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.NAME_CODE.ToStringNullSafe().Trim() ||
                    string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM?.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        return;
                    }

                    var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, ENTERED_VALUE_ROW);
                    if (RST_KALA != null)
                    {
                        CURRENT_ITEMS_ROW.CODE = RST_KALA.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = RST_KALA.NAME_CODE;
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM?.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                        return;
                    }
                }
            }
            #endregion
        }

        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            DG_SUB.Dispatcher.BeginInvoke(() =>
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
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var row = e.Row.Item as VISITORS_PORSANT_KALA;
            if (ConstructorRowDetector.IsPristine(row)) { DG_SUB_CANCEL_EDIT(); return; }

            row.PORID = int.Parse(PORID.Text);

            if (!BodyIsValid(row))
            {
                DG_SUB_CANCEL_EDIT();
                return;
            }

            try
            {

                if (row?.ID == null || row.ID == 0) // Insert
                {
                    string insertSql = @"
                        INSERT INTO VISITORS_PORSANT_KALA (PORID, CODE, PORSANT)
                        VALUES (@PORID, @CODE, @PORSANT)";
                    dbms.DoExecuteSQL(insertSql, row);
                }
                else // Update
                {
                    string updateSql = @"
                        UPDATE VISITORS_PORSANT_KALA
                        SET CODE = @CODE, PORSANT = @PORSANT
                        WHERE ID = @ID";
                    dbms.DoExecuteSQL(updateSql, row);
                }
                ChangeIsHappend = true;
            }
            catch (SqlException ex)
            {
                DG_SUB_CANCEL_EDIT();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "سطر تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در ذخیره سطر").ShowDialog();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره سطر").ShowDialog();
                e.Cancel = true;
            }
        }

        private void COPY_CLICK(object sender, RoutedEventArgs e) { /* Logic Copy */ }
        private void PASTE_CLICK(object sender, RoutedEventArgs e) { /* Logic Paste */ }
        private void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e) { /* Logic Excel */ }

        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;

            var report = new StiReport();
            using var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.Visitory.OLGU_POORSANT.mrt");
            if (stream != null)
            {
                report.Load(stream);
                ((StiSqlDatabase)report.Dictionary.Databases["MS SQL"]).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

                report["NUMBER_PARAM"] = int.Parse(PORID.Text);

                if (report.GetComponentByName("CUST_COD_TEXT") is StiText STITEXT)
                {
                    STITEXT.Text = CUST_COD.Text;
                }

                new WINRPT(report, "چاپ پورسانت").Show();
            }
            else
            {
                new Msgwin(false, "فایل گزارش یافت نشد").ShowDialog();
            }
        }

        // Command23_Click: دریافت لیست کالاها از طریق گروه کالا
        private void Command23_Click11111111(object sender, RoutedEventArgs e)
        {
            // 1. نمایش فرم انتخاب گروه کالا به صورت دیالوگ
            var selectiveGrpWin = new WIN_visit_STUFGR_SEL_KALA();
            selectiveGrpWin.ShowDialog();

            // 2. بررسی اینکه آیا فرم بسته شده و داده‌ای دارد یا خیر
            // نکته: چون ItemsData در حافظه فرم نگهداری می‌شود، حتی بعد از Close هم قابل دسترسی است
            var selectedGroups = selectiveGrpWin.ItemsData
                .Where(x => x.TIC == true)
                .Select(x => x.CODE)
                .ToList();

            if (selectedGroups.Count == 0) return;

            try
            {
                int currentPorId = int.Parse(PORID.Text);

                // ============================================================
                // گام اول: دریافت لیست کالاهای موجود برای جلوگیری از تکرار (C# Memory Check)
                // ============================================================
                string sqlExisting = "SELECT CODE FROM VISITORS_PORSANT_KALA WHERE PORID = @PORID";
                var existingCodes = dbms.DoGetDataSQL<string>(sqlExisting, new { PORID = currentPorId }).ToHashSet();

                // ============================================================
                // گام دوم: دریافت لیست کالاها بر اساس گروه‌های انتخاب شده
                // ============================================================
                // استفاده از ویژگی Dapper برای ارسال لیست به IN Clause
                string sqlCandidates = "SELECT CODE, NAME FROM STUF_DEF WHERE MENUIT IN @GroupCodes";
                var candidates = dbms.DoGetDataSQL<STUF_DEF_SHORT_MODEL>(sqlCandidates, new { GroupCodes = selectedGroups });

                if (candidates == null || !candidates.Any())
                {
                    new Msgwin(false, "هیچ کالایی در گروه‌های انتخاب شده یافت نشد").ShowDialog();
                    return;
                }

                // ============================================================
                // گام سوم: حلقه بررسی و درج
                // ============================================================
                List<string> skippedNames = new List<string>();
                int insertedCount = 0;

                foreach (var item in candidates)
                {
                    // بررسی تکراری بودن در حافظه (بدون کوئری زدن به SQL برای هر سطر)
                    if (existingCodes.Contains(item.CODE))
                    {
                        skippedNames.Add(item.NAME);
                    }
                    else
                    {
                        // درج رکورد جدید
                        // نکته: مقدار PORSANT را 0 در نظر گرفتیم چون در UI تکست‌باکس MEGH وجود نداشت
                        string insertSql = @"
                            INSERT INTO VISITORS_PORSANT_KALA (PORID, CODE, PORSANT) 
                            VALUES (@PORID, @CODE, 0)";

                        dbms.DoExecuteSQL(insertSql, new
                        {
                            PORID = currentPorId,
                            CODE = item.CODE
                        });

                        insertedCount++;
                    }
                }

                // ============================================================
                // گام چهارم: رفرش و گزارش‌دهی
                // ============================================================

                // رفرش کردن گرید
                DG_SUB_ReGetData();

                // نمایش پیام نهایی
                if (skippedNames.Count > 0)
                {
                    string msg = $"تعداد {insertedCount} کالا با موفقیت اضافه شد.\n\n" +
                                 $"کالاهای زیر قبلاً ثبت شده بودند و نادیده گرفته شدند:\n" +
                                 string.Join("، ", skippedNames.Take(20)); // نمایش حداکثر 20 نام برای جلوگیری از شلوغی

                    if (skippedNames.Count > 20) msg += "\nو ...";

                    new Msgwin(true, msg).ShowDialog();
                }
                else
                {
                    universControl.PopNotifyShow($"تعداد {insertedCount} کالا با موفقیت اضافه شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }

            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در عملیات افزودن کالا").ShowDialog();
            }
        }
        private void Command23_Click(object sender, RoutedEventArgs e)
        {
            // 1. نمایش فرم انتخاب گروه کالا
            var selectiveGrpWin = new WIN_visit_STUFGR_SEL_KALA();
            selectiveGrpWin.ShowDialog();

            // 2. دریافت گروه‌های انتخاب شده
            var selectedGroups = selectiveGrpWin.ItemsData
                .Where(x => x.TIC == true)
                .Select(x => x.CODE)
                .ToList();

            // 3. دریافت درصد پورسانت وارد شده توسط کاربر
            double porsantToInsert = selectiveGrpWin.SelectedPercent;

            if (selectedGroups.Count == 0) return;

            try
            {
                int currentPorId = int.Parse(PORID.Text);

                // الف) دریافت لیست کالاهای موجود برای جلوگیری از تکرار
                string sqlExisting = "SELECT CODE FROM VISITORS_PORSANT_KALA WHERE PORID = @PORID";
                var existingCodes = dbms.DoGetDataSQL<string>(sqlExisting, new { PORID = currentPorId }).ToHashSet();

                // ب) دریافت لیست کاندیداها از گروه‌های انتخاب شده
                string sqlCandidates = "SELECT CODE, NAME FROM STUF_DEF WHERE MENUIT IN @GroupCodes";
                var candidates = dbms.DoGetDataSQL<STUF_DEF_SHORT_MODEL>(sqlCandidates, new { GroupCodes = selectedGroups });

                if (candidates == null || !candidates.Any())
                {
                    new Msgwin(false, "هیچ کالایی در گروه‌های انتخاب شده یافت نشد").ShowDialog();
                    return;
                }

                List<string> skippedNames = new List<string>();
                int insertedCount = 0;

                // ج) حلقه درج
                foreach (var item in candidates)
                {
                    if (existingCodes.Contains(item.CODE))
                    {
                        skippedNames.Add(item.NAME);
                    }
                    else
                    {
                        // درج با استفاده از درصد وارد شده
                        string insertSql = @"
                            INSERT INTO VISITORS_PORSANT_KALA (PORID, CODE, PORSANT) 
                            VALUES (@PORID, @CODE, @PORSANT)";

                        dbms.DoExecuteSQL(insertSql, new
                        {
                            PORID = currentPorId,
                            CODE = item.CODE,
                            PORSANT = porsantToInsert // استفاده از متغیر درصد
                        });

                        insertedCount++;
                    }
                }

                // د) رفرش و نمایش نتیجه
                DG_SUB_ReGetData();

                if (skippedNames.Count > 0)
                {
                    string msg = $"تعداد {insertedCount} کالا با موفقیت اضافه شد (با درصد {porsantToInsert}).\n\n" +
                                 $"کالاهای تکراری نادیده گرفته ({skippedNames.Count}) شدند:\n" +
                                 string.Join("، ", skippedNames.Take(20));

                    if (skippedNames.Count > 20) msg += "\nو ...";

                    new Msgwin(false, msg).ShowDialog();
                }
                else
                {
                    universControl.PopNotifyShow($"تعداد {insertedCount} کالا با درصد {porsantToInsert} اضافه شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در عملیات افزودن کالا: " + ex.Message).ShowDialog();
            }
        }
        // کلاس کمکی برای نگهداری نتیجه کوئری کالاها
        public class STUF_DEF_SHORT_MODEL
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!CheckForUnsavedChanges())
                e.Cancel = true;
        }
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!Tarikh.IsValidedDate(VDATE.Text))
            {
                // Handle Invalid Date
            }
        }

        private void ClearFreshAll()
        {
            AllowEdits = true;
            PORID.Text = "0"; // Will be calculated on save
            VDATE.Text = CL_HESABDARI.FARSIDATE().ToString();
            HES.SelectedIndex = -1;
            HES2.SelectedIndex = -1;
            CUST_COD.SelectedIndex = -1;
            COMMENT.Text = "";
            USERNAME.Text = Baseknow.UUSER;
            SUB_DATA.Clear();
            BTN_SAVE.IsEnabled = true;

            Command23.IsEnabled = false;
            DG_SUB.IsReadOnly = true;

            HES.Focus();
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!_navigationManager.IsNewRecord)
            {
                DG_SUB.IsReadOnly = false;
                Command23.IsEnabled = true;
                this.AllowEdits = true;
                this.AllowDeletions = true;
            }
        }
    }
}
