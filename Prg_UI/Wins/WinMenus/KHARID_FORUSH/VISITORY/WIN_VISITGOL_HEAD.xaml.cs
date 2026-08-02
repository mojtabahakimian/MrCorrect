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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for WIN_VISITGOL_HEAD.xaml
    /// </summary>
    public partial class WIN_VISITGOL_HEAD : Window, ISearchableWindow, IComboLookupProvider
    {
        public WIN_VISITGOL_HEAD(double? number_to_open = null)
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private NavigationManager<VISITGOL_HEAD> _navigationManager;
        public ObservableCollection<VISITGOL_DTL> VISIT_GOL_DATA { get; set; } = new ObservableCollection<VISITGOL_DTL>();

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

                MAH.IsEnabled = ican;
                CUST_NO.IsEnabled = ican;
                CUST_NO2.IsEnabled = ican;
                DG_SUB.IsReadOnly = !ican;
            }
        }

        public nint WINDOW_ID { get; private set; }
        public string? ENTERED_VALUE_ROW { get; private set; }
        public VISITGOL_DTL? CURRENT_ITEMS_ROW { get; private set; }
        public VISITGOL_DTL? WAS_ROW_ITEM { get; private set; }
        public List<Custom_VAHEDK> RST_KALAVAHED_LST { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WINDOW_ID = new WindowInteropHelper(this).Handle;

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "VISITGOL", WINDOW_ID, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            FILL_ALL_COMBOBOXES();

            string WhereCondition = "";
            //if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            //{
            //    WhereCondition = $" WHERE GSCACOD = {GSCACOD.Text} ";
            //}

            _navigationManager = new NavigationManager<VISITGOL_HEAD>(
                dbms,
                x => x?.HES,
                $"SELECT HES, MAH, CDATE, OKF, FDATE, TODATE, USERNAME, CRT, UID FROM dbo.VISITGOL_HEAD ORDER BY CRT",
                x => $"SELECT HES, MAH, CDATE, OKF, FDATE, TODATE, USERNAME, CRT, UID FROM dbo.VISITGOL_HEAD WHERE HES = N'{x?.HES}' AND MAH = {x?.MAH} ",
                default);

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            CL_LMethods.SetTabIndexes(
                MAH,
                CUST_NO,
                BTN_SAVE,
                DG_SUB
                );

            MakeDefaultFocuseReady();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                var DG = DG_SUB;
                if (DG.IsKeyboardFocusWithin && DG != null)
                {
                    if (DG?.CurrentColumn != null && DG.SelectedItem != null)
                    {
                        // 1. جستجو برای پیدا کردن پنجره پیام (چه فعال باشد چه نباشد)
                        var messageWindow = Application.Current.Windows.OfType<Window>()
                            .FirstOrDefault(w => w is Prg_UI.HelperWins.Msgwin || w is Prg_UI.HelperWins.MsgListwin);

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
                        bool isLastColumn = DG.CurrentColumn?.SortMemberPath == "MEGHk";
                        bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

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
                                    if (DG.SelectedItem != null && DG.Columns.Count > 0)
                                    {
                                        DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[0]);

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
                else if (BTN_SAVE.IsFocused)
                {
                    BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                CL_LMethods.SendKey_US(Key.Tab, true);
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
            //حساب یا کد مشتریان
            //CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            var RST_HES = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@$"SELECT Visit_route.HES AS hes, CUST_HESAB.NAME
                                                         FROM Visit_route
                                                             INNER JOIN CUST_HESAB
                                                                 ON Visit_route.HES = CUST_HESAB.hes
                                                         GROUP BY Visit_route.HES, CUST_HESAB.NAME
                                                         ORDER BY CUST_HESAB.NAME").ToList();
            foreach (var item in RST_HES)
            {
                if (!string.IsNullOrEmpty(item?.NAME))
                {
                    item.NAME = item.NAME.FixPersianChars();
                }
            }
            CUST_NO.ItemsSource = RST_HES;
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            MAH.ItemsSource = new List<COMBOYMODEL>()
            {
                new COMBOYMODEL { ID = 1, NAME = "فروردین" },
                new COMBOYMODEL { ID = 2, NAME = "اردیبهشت" },
                new COMBOYMODEL { ID = 3, NAME = "خرداد" },
                new COMBOYMODEL { ID = 4, NAME = "تیر" },
                new COMBOYMODEL { ID = 5, NAME = "مرداد" },
                new COMBOYMODEL { ID = 6, NAME = "شهریور" },
                new COMBOYMODEL { ID = 7, NAME = "مهر" },
                new COMBOYMODEL { ID = 8, NAME = "آبان" },
                new COMBOYMODEL { ID = 9, NAME = "آذر" },
                new COMBOYMODEL { ID = 10, NAME = "دی" },
                new COMBOYMODEL { ID = 11, NAME = "بهمن" },
                new COMBOYMODEL { ID = 12, NAME = "اسفند" },
            };
        }
        private void MakeDefaultFocuseReady()
        {
            MAH.Focus();
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

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
        private bool OnInsertRecord(VISITGOL_HEAD record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<VISITGOL_HEAD>($"SELECT TOP 1 * FROM VISITGOL WHERE HES = N'{CUST_NO.SelectedValue}' AND MAH = {MAH.SelectedValue}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void OnCurrentRecordChanged(VISITGOL_HEAD HEADER)
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
                USER_NAME.Text = HEADER.USERNAME.ToStringNullSafe();
                CUST_NO.SelectedValue = HEADER.HES;

                int mahIndex = HEADER.MAH;
                if (mahIndex >= 0 && mahIndex < MAH.Items.Count)
                {
                    MAH.SelectedValue = mahIndex;
                }
                else
                {
                    MAH.SelectedIndex = -1;
                }

                FDATE.Text = HEADER.FDATE.ToString();
                TODATE.Text = HEADER.TODATE.ToString();

                DG_SUB_ReGetData();

                AllowEdits = false;

                BTN_SAVE.IsEnabled = false;
            }
        }

        private void ClearFreshAll()
        {
            USER_NAME.Text = Baseknow.UUSER;
            CUST_NO.SelectedIndex = -1;
            CUST_NO2.SelectedIndex = -1;
            MAH.SelectedIndex = -1;
            VISIT_GOL_DATA.Clear();
            AllowEdits = true;
            DG_SUB.IsReadOnly = true;
            MakeDefaultFocuseReady();
            BTN_SAVE.IsEnabled = true;
        }

        public void DG_SUB_ReGetData()
        {
            if (!_navigationManager.IsNewRecord && CUST_NO.SelectedValue != null && MAH.SelectedIndex >= 0)
            {
                int mah = (int)MAH.SelectedValue;
                string hes = CUST_NO.SelectedValue.ToString();

                var items = dbms.DoGetDataSQL<VISITGOL_DTL>(@$"
                    SELECT D.*, S.NAME AS NAME_CODE
                    FROM VISITGOL_DTL D
                    LEFT JOIN STUF_DEF S ON D.CODE = S.CODE
                    WHERE D.HES = @HES AND D.MAH = @MAH", new { HES = hes, MAH = mah }).ToList();

                VISIT_GOL_DATA.Clear();
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.NAME_CODE)) item.NAME_CODE = "نامشخص";
                    item.NAME_CODE = item.NAME_CODE.FixPersianChars();
                    VISIT_GOL_DATA.Add(item);
                }
            }
        }

        private void MAH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MAH.SelectedIndex >= 0)
            {
                int mah = Convert.ToInt32(MAH.SelectedValue);
                int yea = (int)Baseknow.YEA;

                long fdate = yea * 10000 + mah * 100 + 1;
                long todate = yea * 10000 + mah * 100 + 31;

                FDATE.Text = fdate.ToString();
                TODATE.Text = todate.ToString();
            }
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (CUST_NO.SelectedValue == null || MAH.SelectedIndex < 0)
            {
                universControl.PopNotifyShow("لطفا ویزیتور و ماه را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

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

            string hes = CUST_NO.SelectedValue.ToString();
            int mah = Convert.ToInt32(MAH.SelectedValue);

            long fdate = 0; long.TryParse(FDATE.Text, out fdate);
            long todate = 0; long.TryParse(TODATE.Text, out todate);

            try
            {
                // Save Header
                //var existing = dbms.DoGetDataSQL<VISITGOL_HEAD>("SELECT HES FROM VISITGOL_HEAD WHERE HES = @HES AND MAH = @MAH", new { HES = hes, MAH = mah }).FirstOrDefault();

                if (_navigationManager.IsNewRecord) //existing == null
                {
                    dbms.DoExecuteSQL(@"INSERT INTO VISITGOL_HEAD (HES, MAH, CDATE, OKF, FDATE, TODATE, USERNAME, CRT, UID)
                                  VALUES (@HES, @MAH, GETDATE(), 0, @FDATE, @TODATE, @USERNAME, GETDATE(), @UID)",
                                        new { HES = hes, MAH = mah, FDATE = fdate, TODATE = todate, USERNAME = USER_NAME.Text, UID = Baseknow.USERCOD });

                    AllowEdits = true;
                }
                else
                {
                    dbms.DoExecuteSQL(@"UPDATE VISITGOL_HEAD SET FDATE = @FDATE, TODATE = @TODATE, USERNAME = @USERNAME 
                                  WHERE HES = @HES AND MAH = @MAH",
                                        new { HES = hes, MAH = mah, FDATE = fdate, TODATE = todate, USERNAME = USER_NAME.Text });
                }

                universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                _navigationManager.IsNewRecord = false;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری است آنرا اصلاح کنید").ShowDialog();
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

            if (VISIT_GOL_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private void GetFocusOnDefaultCell()
        {
            var DG = DG_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "NAME_CODE", DEFINDX, true);
        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (CUST_NO.SelectedValue == null || MAH.SelectedIndex < 0) return;

            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }
            if (_navigationManager.IsNewRecord || !AllowEdits) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                var hes = CUST_NO.SelectedValue.ToString();
                var mah = Convert.ToInt32(MAH.SelectedValue);

                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف اهداف برای ویزیتور",
                    recordId: CUST_NO.Text,
                    oldValue: $"",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (VISIT_GOL_DATA.Count > 0 && DG_SUB.SelectedItems != null && DG_SUB.SelectedItems.Count > 0)
                {
                    List<MsgModel> ErrosMessages = new List<MsgModel>();
                    for (int i = 0; i < DG_SUB.SelectedItems.Count; i++)
                    {
                        var item = DG_SUB.SelectedItems[i];

                        if (CL_LMethods.IsNewPlaceHolder(DG_SUB, item))
                        {
                            continue; // Skip deletion for new placeholder items
                        }

                        if (item is VISITGOL_DTL RowItemy)
                        {
                            try
                            {
                                if (RowItemy?.CODE != null)
                                {
                                    dbms.DoExecuteSQL("DELETE FROM VISITGOL_DTL" +
                                        " WHERE HES = @HES AND MAH = @MAH " +
                                        "AND CODE = @CODE AND MEGHk = @MEGHk", new { HES = hes, MAH = mah, CODE = RowItemy.CODE, MEGHk = RowItemy.MEGHk });
                                }
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
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();
                    }

                    DG_SUB_ReGetData();
                }
                else
                {
                    try
                    {
                        dbms.DoExecuteSQL("DELETE FROM VISITGOL_HEAD WHERE HES = @HES AND MAH = @MAH", new { HES = hes, MAH = mah });

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

        private void Command25_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) { return; }

            // Add products based on group
            if (CUST_NO.SelectedValue == null || MAH.SelectedIndex < 0)
            {
                universControl.PopNotifyShow("لطفا ابتدا ویزیتور و ماه را مشخص کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var win = new WIN_visit_STUFGR_SEL_KALA();
            win.LBL1.Visibility = Visibility.Hidden;
            win.DARSAD.Visibility = Visibility.Hidden;
            win.ShowDialog();

            // Check selected items in win.ItemsData
            var selectedGroups = win.ItemsData.Where(x => x.TIC == true).Select(x => x.CODE).ToList();
            if (selectedGroups.Count > 0)
            {
                var products = dbms.DoGetDataSQL<STUF_DEF_CSHARP>($"SELECT CODE, NAME, VAHED FROM STUF_DEF WHERE MENUIT IN @GROUPS", new { GROUPS = selectedGroups }).ToList();

                foreach (var prod in products)
                {
                    string code = prod.CODE;
                    string name = prod.NAME;
                    int vahed = Convert.ToInt32(prod.VAHED ?? 0);

                    // Check duplicate in local list
                    if (!VISIT_GOL_DATA.Any(x => x.CODE == code))
                    {
                        // 1. Create your list object
                        var newItem = new VISITGOL_DTL
                        {
                            HES = CUST_NO.SelectedValue.ToString(),
                            MAH = Convert.ToByte(MAH.SelectedValue),
                            CODE = code,
                            NAME_CODE = name,
                            VAHED_K = vahed,
                            MEGH = 0,
                            MEGHk = 0,
                            USERNAME = USER_NAME.Text,
                            CDATE = DateTime.Now,
                            RADIF = VISIT_GOL_DATA.Count + 1
                        };

                        VISIT_GOL_DATA.Add(newItem);

                        // 2. Execute SQL. Note how we pass the anonymous object properties directly.
                        dbms.DoExecuteSQL(@"INSERT INTO visitgol_dtl (HES, MAH, CODE, CDATE, RADIF, MEGH, MEGHk, VAHED_K, USERNAME, CRT, UID)
                        VALUES (@HES, @MAH, @CODE, GETDATE(), @RADIF, @MEGH, @MEGHk, @VAHED_K, @USERNAME, GETDATE(), @UID)",
                            new
                            {
                                HES = newItem.HES,
                                MAH = newItem.MAH,
                                CODE = newItem.CODE,
                                RADIF = newItem.RADIF,
                                MEGH = newItem.MEGH,
                                MEGHk = newItem.MEGHk,
                                VAHED_K = newItem.VAHED_K,
                                USERNAME = newItem.USERNAME,
                                UID = Baseknow.USERCOD
                            });
                    }
                }
            }


        }

        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            if (!Command100.IsEnabled || Command100.Visibility != Visibility.Visible || _navigationManager.IsNewRecord)
            {
                return;
            }

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.TAARIF.VISIT_GOL_HEAD_KALA.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;

            report["MAH_PARM"] = MAH.SelectedValue;
            report["HES_PARM"] = CUST_NO.SelectedValue;

            (report.GetComponentByName("VNAME") as StiText).Text = CUST_NO.Text;
            (report.GetComponentByName("VCODE") as StiText).Text = CUST_NO.SelectedValue.ToString();
            (report.GetComponentByName("VDATE") as StiText).Text = Tarikh.FullCurrentDate;

            new WINRPT(report, "تعيين اهداف براي ويزيتور ها").Show();
        }

        #region ISearchableWindow
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "ویزیتور", PropertyPath = "HES", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ماه", PropertyPath = "MAH", PropertyType = typeof(byte) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USERNAME", PropertyType = typeof(string) },
            };
        }
        public IEnumerable<ComboLookupSpec> GetComboLookups()
        {
            yield return new ComboLookupSpec { DisplayName = "ویزیتور", KeyPropertyPath = "HES", Combo = CUST_NO };
            yield return new ComboLookupSpec { DisplayName = "ماه", KeyPropertyPath = "MAH", Combo = MAH };
        }

        public object GetSearchSource()
        {
            return _navigationManager.RecordsData;
        }
        public void OnSearchResultSelected(object selectedItem)
        {
            if (selectedItem is VISITGOL_HEAD item)
            {
                var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.HES == item.HES && x.MAH == item.MAH);
                if (itemfound != null)
                {
                    _navigationManager.IsNewRecord = false;
                    int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                    if (idx >= 0)
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                }
            }
        }
        #endregion

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

            //var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            //if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            //{
            //    universControl.PopNotifyShow($"ویزیتور نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            //    e.Handled = true;
            //}

            //if (CUST_NO.SelectedValue is not null)
            //{
            //    if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
            //    {
            //        Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
            //        msgwin.ShowDialog();
            //        CUST_NO.SelectedValue = null;
            //    }
            //    if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
            //    {
            //        CUST_NO.SelectedItem = null;
            //        universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
            //        return;
            //    }
            //}

        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (ESLAH.Visibility != Visibility.Visible || !ESLAH.IsEnabled || _navigationManager.IsNewRecord)
            {
                return;
            }

            AllowEdits = true;

            BTN_SAVE.IsEnabled = true;
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
            var CurrentRow = e.Row.Item as VISITGOL_DTL;
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
        private void DG_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && DG_SUB.SelectedItem is not null)
            {
                if (DG_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((VISITGOL_DTL)DG_SUB.SelectedItem).Clone() as VISITGOL_DTL;
                }
            }
        }
        private void DG_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (CUST_NO.SelectedValue == null)
            {
                CUST_NO.Focus();
                new Msgwin(false, "مسئول شیفت نمیتواند خالی باشد!").ShowDialog();
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

            CURRENT_ITEMS_ROW = e.Row.Item as VISITGOL_DTL;
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.NAME_CODE.ToStringNullSafe().Trim() ||
                    string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    #region CODE_NotInList

                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        return;
                    }

                    var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, ENTERED_VALUE_ROW);
                    if (RST_KALA != null)
                    {
                        CURRENT_ITEMS_ROW.CODE = RST_KALA.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = RST_KALA.NAME_CODE;

                        CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        CURRENT_ITEMS_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                        DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                        return;
                    }

                    VAHED_K_AfterUpdate();
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
                if ((e.Row.Item as VISITGOL_DTL).CODE is null)
                {
                    return;
                }
                if (((e.Row.Item as VISITGOL_DTL)?.VAHED_K is null) || (((e.Row.Item as VISITGOL_DTL).CODE is null))
                    || ((e.Row.Item as VISITGOL_DTL).NAME_CODE is null))
                {
                    DG_SUB_CANCEL_EDIT();
                    (e.Row.Item as VISITGOL_DTL).VAHED_K = WAS_ROW_ITEM.VAHED_K;
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
                }
                #endregion
            }
            #endregion


            //مقدار
            #region MEGH
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (CURRENT_ITEMS_ROW.CODE is null || CURRENT_ITEMS_ROW.VAHED_K is null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGH = 0;
                    return;
                }
                if ((e.Row.Item as VISITGOL_DTL).CODE is null || (e.Row.Item as VISITGOL_DTL).VAHED_K is null)
                {
                    return;
                }
                CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(ENTERED_VALUE_ROW);

                MEGH_AfterUpdate();
            }
            #endregion

            //مقدار
            #region MEGHk
            if (e.Column.SortMemberPath == "MEGHk")
            {
                if (CURRENT_ITEMS_ROW.CODE is null || CURRENT_ITEMS_ROW.VAHED_K is null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGH = 0;
                    return;
                }
                if ((e.Row.Item as VISITGOL_DTL).CODE is null || (e.Row.Item as VISITGOL_DTL).VAHED_K is null)
                {
                    return;
                }
                CURRENT_ITEMS_ROW.MEGHk = Convert.ToDouble(ENTERED_VALUE_ROW);

                MEGH_AfterUpdate();
            }
            #endregion

        }
        private void DG_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) return;
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            if (e.Row.Item == null) return;

            var TheRow = e.Row.Item as VISITGOL_DTL;

            if (ConstructorRowDetector.IsPristine(TheRow)) { DG_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(TheRow)) { DG_SUB_CANCEL_EDIT(); return; }

            // مقادیر هدر را روی سطر اعمال کن
            TheRow.HES = CUST_NO.SelectedValue.ToString();
            TheRow.MAH = Convert.ToByte(MAH.SelectedValue);
            TheRow.USERNAME = USER_NAME.Text;
            TheRow.UID = Baseknow.USERCOD;
            TheRow.CDATE = DateTime.Now;

            try
            {
                if (e.Row.IsNewItem)
                {
                    TheRow.RADIF = VISIT_GOL_DATA.Count;

                    dbms.DoExecuteSQL(@"
                INSERT INTO VISITGOL_DTL (HES, MAH, CODE, CDATE, RADIF, MEGH, MEGHk, VAHED_K, USERNAME, CRT, UID)
                VALUES (@HES, @MAH, @CODE, GETDATE(), @RADIF, @MEGH, @MEGHk, @VAHED_K, @USERNAME, GETDATE(), @UID)",
                        new
                        {
                            TheRow.HES,
                            TheRow.MAH,
                            TheRow.CODE,
                            TheRow.RADIF,
                            TheRow.MEGH,
                            TheRow.MEGHk,
                            TheRow.VAHED_K,
                            TheRow.USERNAME,
                            UID = (int?)TheRow.UID
                        });
                }
                else
                {

                    dbms.DoExecuteSQL(@"
                UPDATE VISITGOL_DTL 
                SET    MEGH     = @MEGH,
                       MEGHk    = @MEGHk,
                       USERNAME = @USERNAME
                WHERE  HES = @HES AND MAH = @MAH AND CODE = @CODE",
                        new { TheRow.MEGH, TheRow.MEGHk, TheRow.USERNAME, TheRow.HES, TheRow.MAH, TheRow.CODE });
                }

                universControl.PopNotifyShow("سطر با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                ChangeIsHappend = true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده تکراری است آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, $"خطا در ذخیره سطر: {ex.Message}").ShowDialog();
                    return;
                }
                return;
            }
            catch (Exception ex)
            {
                DG_SUB_CANCEL_EDIT();
                new Msgwin(false, $"خطا در ذخیره سطر: {ex.Message}").ShowDialog();
            }
        }

        void VAHED_K_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.VAHED_K is null) { return; }
            if (CURRENT_ITEMS_ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST.FirstOrDefault().NESBAT;
            }

            MEGH_AfterUpdate();
        }
        void MEGH_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.MEGHk is null || CURRENT_ITEMS_ROW?.CODE is null)
            {
                return;
            }

            var RST0 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
            if (RST0.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
                return;
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST0.FirstOrDefault()?.NESBAT ?? 1;
            }
        }

        private bool BodyIsValidAll(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrorMessages = new List<MsgModel>();

            // ── ۱. لیست خالی ─────────────────────────────────────────────
            if (!VISIT_GOL_DATA.Any())
            {
                ErrorMessages.Add(new MsgModel { MessageText_U = "حداقل یک کالا باید وارد شود" });

                if (_DisplayErrors)
                    new MsgListwin(false, ErrorMessages).ShowDialog();

                return false; // ادامه بررسی بی‌معناست
            }

            // ── ۲. ردیف‌های بدون کد ──────────────────────────────────────
            var emptyCodeRows = VISIT_GOL_DATA
                .Select((row, idx) => new { row, radif = idx + 1 })
                .Where(x => string.IsNullOrWhiteSpace(x.row.CODE))
                .ToList();

            foreach (var item in emptyCodeRows)
                ErrorMessages.Add(new MsgModel
                {
                    MessageText_U = $"ردیف {item.radif}: کد کالا نمی‌تواند خالی باشد"
                });

            // ── ۳. کالاهای تکراری (بر اساس CODE) ────────────────────────
            var duplicateGroups = VISIT_GOL_DATA
                .Where(x => !string.IsNullOrWhiteSpace(x.CODE))
                .GroupBy(x => x.CODE.Trim())
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var grp in duplicateGroups)
            {
                var radifs = VISIT_GOL_DATA
                    .Select((row, idx) => new { row, radif = idx + 1 })
                    .Where(x => x.row.CODE?.Trim() == grp.Key)
                    .Select(x => x.radif.ToString())
                    .ToList();

                ErrorMessages.Add(new MsgModel
                {
                    MessageText_U = $"کالای «{grp.First().NAME_CODE} ({grp.Key})» در ردیف‌های {string.Join(" و ", radifs)} تکرار شده است"
                });
            }

            // ── ۴. بررسی مقادیر هر ردیف ──────────────────────────────────
            var validRows = VISIT_GOL_DATA
                .Select((row, idx) => new { row, radif = idx + 1 })
                .Where(x => !string.IsNullOrWhiteSpace(x.row.CODE))
                .ToList();

            foreach (var item in validRows)
            {
                var row = item.row;
                string label = $"ردیف {item.radif} ({row.CODE})";

                // MEGH (هدف)
                if (row.MEGH < 0)
                    ErrorMessages.Add(new MsgModel { MessageText_U = $"{label}: مقدار هدف نمی‌تواند منفی باشد" });

                if (row.MEGH == 0)
                    ErrorMessages.Add(new MsgModel { MessageText_U = $"{label}: مقدار هدف نمی‌تواند صفر باشد" });

                // MEGHk (مقدار کالا)
                if (row.MEGHk < 0)
                    ErrorMessages.Add(new MsgModel { MessageText_U = $"{label}: مقدار کالا نمی‌تواند منفی باشد" });

                // VAHED_K
                if (row.VAHED_K <= 0)
                    ErrorMessages.Add(new MsgModel { MessageText_U = $"{label}: واحد کالا معتبر نیست" });
            }

            // ── ۵. نمایش خطاها ───────────────────────────────────────────
            if (ErrorMessages.Any())
            {
                if (_DisplayErrors)
                {
                    var distinct = ErrorMessages
                        .Select(x => x.MessageText_U)
                        .Distinct()
                        .Select(msg => new MsgModel { MessageText_U = msg })
                        .ToList();

                    new MsgListwin(false, distinct).ShowDialog();
                }
                return false;
            }

            return true;
        }
        private bool BodyIsValid(VISITGOL_DTL row, bool _DisplayErrors = true)
        {
            List<MsgModel> ErrorMessages = new List<MsgModel>();

            // ── ۱. کد کالا ───────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(row.CODE))
                ErrorMessages.Add(new MsgModel { MessageText_U = "کد کالا نمی‌تواند خالی باشد" });

            // ── ۲. تکراری بودن CODE در لیست ─────────────────────────────
            if (!string.IsNullOrWhiteSpace(row.CODE))
            {
                var isDuplicate = VISIT_GOL_DATA
                    .Count(x => x.CODE?.Trim() == row.CODE.Trim()) > 1;

                if (isDuplicate)
                    ErrorMessages.Add(new MsgModel
                    {
                        MessageText_U = $"کالای «{row.NAME_CODE} ({row.CODE})» قبلاً در لیست وجود دارد"
                    });
            }

            // ── ۴. مقدار کالا (MEGHk) ────────────────────────────────────
            if (row.MEGHk < 0)
                ErrorMessages.Add(new MsgModel { MessageText_U = "مقدار کالا نمی‌تواند منفی باشد" });

            // ── ۵. واحد کالا ─────────────────────────────────────────────
            if (row.VAHED_K <= 0)
                ErrorMessages.Add(new MsgModel { MessageText_U = "واحد کالا معتبر نیست" });

            // ── ۶. نمایش خطاها ───────────────────────────────────────────
            if (ErrorMessages.Any())
            {
                if (_DisplayErrors)
                    new MsgListwin(false, ErrorMessages).ShowDialog();

                return false;
            }

            return true;
        }

    }
}
