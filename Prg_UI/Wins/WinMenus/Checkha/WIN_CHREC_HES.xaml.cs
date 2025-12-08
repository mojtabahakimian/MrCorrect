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
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Rpts;
using Stimulsoft.Controls.Wpf.ControlsV3;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Maps;
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
using static Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinMenus.Checkha
{
    public partial class WIN_CHREC_HES : Window, ISearchableWindow
    {
        private UniversControl universControl = new UniversControl();
        private NavigationManager<CHKREC_H> _navigationManager;
        private ObservableCollection<CHRE_LIST_Q> SubData = new ObservableCollection<CHRE_LIST_Q>();
        private CHKREC_H CurrentRecord;

        public WIN_CHREC_HES(double? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (double)number_to_open;
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

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool IsOpenedFromAutomation { get; }
        public bool ChangeIsHappend { get; private set; } = false;

        public int? MasterIDH { get; set; } = null;

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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CHKREC_HES
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            CHRE_LSTH_SUB.ItemsSource = SubData;

            string WhereCondition = "";
            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE IDH = {NUMBER_TO_OPEN}";
            }
            _navigationManager = new NavigationManager<CHKREC_H>(
                dbms,
                x => x.IDH.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM CHKREC_H {WhereCondition} ORDER BY DATE", //All Record of The Table
                /*on navigation get ever record where*/
                x => $"SELECT * FROM CHKREC_H WHERE IDH = {x.IDH} ", //On Change for One Record
                default
                );

            _navigationManager.CanChangeRecord = CheckForUnsavedChanges;

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            if (!CHRE_LSTH_SUB.IsKeyboardFocusWithin && !CHRE_LSTH_SUB.IsFocused)
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
                    DataGridExtension.HandleKeyPress(sender, e, CHRE_LSTH_SUB);
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
            var banks = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM CUST_HESAB");
            N_TAF.ItemsSource = banks;
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is CHKREC_H item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.IDH.Equals(Convert.ToInt32(item.IDH)));
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
              new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE", PropertyType = typeof(long) },
              new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
              new SearchableProperty { DisplayName = "شماره سند", PropertyPath = "N_S", PropertyType = typeof(double) },
              // Add other searchable properties
            };
        }
        #endregion

        private void OnCurrentRecordChanged(CHKREC_H HEADER_FAC)
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
                MasterIDH = HEADER_FAC.IDH;
                DATE.Text = HEADER_FAC.DATE?.ToString();
                MOLAH.Text = HEADER_FAC.MOLAH;
                N_S.Text = HEADER_FAC.N_S?.ToString();
                // CITY.Text = ""; // Not in DB model, strictly UI or linked to another table? Leaving empty or default
                // CHK.Text = "0"; // Default

                // We assume CITY and N_TAF might be stored in user prefs or derived, but based on VBA they seem unbound or loaded from context. 
                // VBA: Me.N_TAF is unbound.

                ReGetData(HEADER_FAC.DATE);
                CheckSanadStatus(HEADER_FAC.N_S);
            }
        }
        private bool OnInsertRecord(CHKREC_H record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<CHKREC_H>($"SELECT TOP 1 * FROM CHKREC_H WHERE MasterIDH = {MasterIDH} ").FirstOrDefault();
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
            var CURRENT_HEADER = dbms.DoGetDataSQL<CHKREC_H>($"SELECT TOP 1 * FROM CHKREC_H WHERE IDH = {MasterIDH} ").FirstOrDefault();
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
                    Btn_Save_Click(this, new RoutedEventArgs());

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
                    Btn_Save_Click(this, new RoutedEventArgs());

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

        private bool CanChangeRecord()
        {
            // Implement dirty check if needed
            return true;
        }


        private void ReGetData(long? date)
        {
            SubData.Clear();
            if (date == null) return;

            string query = $@"
                SELECT 
                    PAY_GETD.SHOBEH, 
                    PAY_GETD.MABL, 
                    PAY_GETD.N_KOL, 
                    PAY_GETD.N_MOIN, 
                    PAY_GETD.N_TAF, 
                    CHRE_LSPH.BANK, 
                    CHRE_LSPH.DATE_S, 
                    CHRE_LSPH.N_SERI, 
                    CHRE_LSPH.USER_NAME, 
                    CUST_HESAB.NAME AS HES1_NAME,
                    TCOD_BANKS.NAMES AS BANK_NAME
                FROM PAY_GETD 
                INNER JOIN CUST_HESAB ON PAY_GETD.HES1 = CUST_HESAB.hes 
                RIGHT OUTER JOIN CHRE_LSPH ON PAY_GETD.N_SERI = CHRE_LSPH.N_SERI AND PAY_GETD.BANK = CHRE_LSPH.BANK AND PAY_GETD.DATE_S = CHRE_LSPH.DATE_S
                LEFT JOIN TCOD_BANKS ON CHRE_LSPH.BANK = TCOD_BANKS.CODE
                WHERE CHRE_LSPH.DATE = {date}";

            var details = dbms.DoGetDataSQL<CHRE_LIST_Q>(query);
            // Note: Using CHRE_LIST_Q as DTO, might need to ensure properties match query columns
            // If CHRE_LIST_Q doesn't have USER_NAME, it will be ignored by Dapper.

            double total = 0;
            foreach (var item in details)
            {
                SubData.Add(item);
                total += item.MABL ?? 0;
            }
            Text10.Text = total.ToString("N0");
        }

        private void ClearFreshAll()
        {
            MasterIDH = null;
            DATE.Text = Tarikh.FullCurrentDate;
            MOLAH.Text = "";
            N_S.Text = "";
            CITY.Text = "";
            N_TAF.SelectedValue = null;
            SubData.Clear();
            Text10.Text = "0";
            CurrentRecord = new CHKREC_H();
        }

        private void CheckSanadStatus(double? n_s)
        {
            if (n_s.HasValue)
            {
                var rst = dbms.DoGetDataSQL<bool?>($"SELECT TOP 1 GHATEI FROM DEED_HED WHERE N_S = {n_s}").FirstOrDefault();
                bool isGhatei = rst ?? false;

                DATE.IsEnabled = !isGhatei;
                MOLAH.IsEnabled = !isGhatei;
                Btn_Save.IsEnabled = !isGhatei;
                Btn_Delete.IsEnabled = !isGhatei;

                if (isGhatei) N_S.Background = System.Windows.Media.Brushes.LightPink;
                else N_S.Background = System.Windows.Media.Brushes.LightYellow;
            }
            else
            {
                DATE.IsEnabled = true;
                MOLAH.IsEnabled = true;
                Btn_Save.IsEnabled = true;
                Btn_Delete.IsEnabled = true;
                N_S.Background = System.Windows.Media.Brushes.LightYellow;
            }
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DATE.Text.ToRawTarikh()))
            {
                universControl.PopNotifyShow("تاریخ را وارد کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // Generate/Update SANAD
            try
            {
                Sanad();
                universControl.PopNotifyShow("اطلاعات ثبت شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ثبت اطلاعات: " + ex.Message).ShowDialog();
            }
        }

        private void Sanad()
        {
            long dateVal = long.Parse(DATE.Text.ToRawTarikh());
            double n_s_val = 0;

            // 1. Handle DEED_HED (Sanad Header)
            if (string.IsNullOrEmpty(N_S.Text))
            {
                // Get Max N_S
                var maxNsObj = dbms.DoGetDataSQL<double?>("SELECT Max(N_S) FROM DEED_HED").FirstOrDefault();
                n_s_val = (maxNsObj ?? 0) + 1;

                string insertHed = @"
                    INSERT INTO DEED_HED (N_S, DATE_S, SHARH_S, GHATEI, NO_S, USER_NAME, CRT, UID)
                    VALUES (@N_S, @DATE_S, @SHARH_S, 0, 6, @USER_NAME, GETDATE(), @UID)";

                dbms.DoExecuteSQL(insertHed, new
                {
                    N_S = n_s_val,
                    DATE_S = dateVal,
                    SHARH_S = "اعلام وصول چكهاي دريافتي",
                    USER_NAME = Baseknow.UUSER, // Assuming Baseknow has USER_NAME or similar
                    UID = Baseknow.USERCOD
                });
            }
            else
            {
                n_s_val = double.Parse(N_S.Text);
                string updateHed = @"
                    UPDATE DEED_HED SET DATE_S = @DATE_S, SHARH_S = @SHARH_S, USER_NAME = @USER_NAME
                    WHERE N_S = @N_S AND NO_S = 6";
                dbms.DoExecuteSQL(updateHed, new
                {
                    N_S = n_s_val,
                    DATE_S = dateVal,
                    SHARH_S = "اعلام وصول چكهاي دريافتي",
                    USER_NAME = Baseknow.UUSER
                });

                // Delete existing Details
                dbms.DoExecuteSQL($"DELETE FROM DEED_DTL WHERE N_S = {n_s_val}");
            }

            // 2. Handle DEED_DTL (Sanad Details)
            // Need to fetch details again with all accounting fields
            string query = $@"
                SELECT 
                    L.N_SERI, L.BANK, B.NAMES AS BANK_NAME, L.DATE_S, L.MABL, 
                    P.SHOBEH, P.N_KOL, P.N_MOIN, P.N_TAF
                FROM CHRE_LST L
                INNER JOIN PAY_GETD P ON L.N_SERI = P.N_SERI AND L.BANK = P.BANK AND L.DATE_S = P.DATE_S
                LEFT JOIN TCOD_BANKS B ON L.BANK = B.CODE
                WHERE L.DATE = {dateVal}";

            var checks = dbms.DoGetDataSQL<dynamic>(query);

            foreach (var chk in checks)
            {
                // Credit Entry (Source) - Fixed Account
                // VBA: SHRST.Fields("hes") = [Forms]![BASEKNOW]![ADA] & "-1-1"
                string hes_cred = Baseknow.ADA + "-1-1";
                string sharh = $" چک {chk.N_SERI} بانک {chk.BANK_NAME} {chk.SHOBEH} مورخ {Strings.Format(Convert.ToInt64(chk.DATE_S), "####/##/##")}";

                string insertCred = @"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, N_SERI, BANK, BES, SHARH)
                    VALUES (@N_S, @HES_K, 1, 1, @hes, @N_SERI, @BANK, @BES, @SHARH)";

                dbms.DoExecuteSQL(insertCred, new
                {
                    N_S = n_s_val,
                    HES_K = Baseknow.ADA,
                    hes = hes_cred,
                    N_SERI = chk.N_SERI,
                    BANK = chk.BANK,
                    BES = chk.MABL,
                    SHARH = sharh
                });

                // Debit Entry (Destination) - Customer/Source of Check
                // VBA: SHRST.Fields("hes") = rst.Fields("N_KOL") & "-" & rst.Fields("N_MOIN") & "-" & rst.Fields("N_TAF")
                string hes_deb = $"{chk.N_KOL}-{chk.N_MOIN}-{chk.N_TAF}";

                string insertDeb = @"
                    INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, N_SERI, BANK, BED, SHARH)
                    VALUES (@N_S, @HES_K, @HES_M, @HES_T, @hes, @N_SERI, @BANK, @BED, @SHARH)";

                dbms.DoExecuteSQL(insertDeb, new
                {
                    N_S = n_s_val,
                    HES_K = chk.N_KOL,
                    HES_M = chk.N_MOIN,
                    HES_T = chk.N_TAF,
                    hes = hes_deb,
                    N_SERI = chk.N_SERI,
                    BANK = chk.BANK,
                    BED = chk.MABL,
                    SHARH = sharh
                });
            }

            // 3. Update/Insert Header Record (CHKREC_H)
            if (_navigationManager.IsNewRecord || CurrentRecord.IDH == null)
            {
                string insertChkRec = @"
                    INSERT INTO CHKREC_H (DATE, MOLAH, N_S, CRT, UID)
                    VALUES (@DATE, @MOLAH, @N_S, GETDATE(), @UID);
                    SELECT SCOPE_IDENTITY();";
                var newId = dbms.DoGetDataSQL<int>(insertChkRec, new
                {
                    DATE = dateVal,
                    MOLAH = MOLAH.Text,
                    N_S = n_s_val,
                    UID = Baseknow.USERCOD
                }).FirstOrDefault();

                CurrentRecord.IDH = newId;
                _navigationManager.IsNewRecord = false;
            }
            else
            {
                string updateChkRec = @"UPDATE CHKREC_H SET DATE=@DATE, MOLAH=@MOLAH, N_S=@N_S WHERE IDH=@IDH";
                dbms.DoExecuteSQL(updateChkRec, new
                {
                    DATE = dateVal,
                    MOLAH = MOLAH.Text,
                    N_S = n_s_val,
                    IDH = CurrentRecord.IDH
                });
            }

            N_S.Text = n_s_val.ToString();
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = Btn_Delete.Visibility == Visibility.Visible;
            if (!Btn_Delete.IsEnabled || !IsVisible || _navigationManager.IsNewRecord) { return; }

            var editableCollectionView = CHRE_LSTH_SUB.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
            {
                try { editableCollectionView.CancelEdit(); } catch { }
            }

            if (_navigationManager.CurrentRecord == null) return;

            //if (SubData.Count > 0)
            //{
            //    new Msgwin(false, "این برگه حاوی اطلاعات می باشد و نمی توانید آن را حذف کنید.").ShowDialog();
            //    return;
            //}

            if (SubData.Count > 0 && CHRE_LSTH_SUB.SelectedItems != null && CHRE_LSTH_SUB.SelectedItems.Count > 0)
            {
                List<MsgModel> ErrosMessages = new List<MsgModel>();
                for (int i = 0; i < CHRE_LSTH_SUB.SelectedItems.Count; i++)
                {
                    var item = CHRE_LSTH_SUB.SelectedItems[i] as CHRE_LIST_Q;

                    if (CL_LMethods.IsNewPlaceHolder(CHRE_LSTH_SUB, item))
                    {
                        continue; // Skip deletion for new placeholder items
                    }

                    bool IsDeletedAny = false;
                    try
                    {
                        IsDeletedAny = true;
                        // Prepare Parameters
                        var sqlParams = new
                        {
                            N_SERI = item.N_SERI,
                            BANK = item.BANK,
                            DATE_S = item.DATE_S
                        };

                        // Query 1: Update PAY_GETD
                        string updateSql = @"
                                UPDATE PAY_GETD 
                                SET N_S = NULL, 
                                    N_KOL = NULL, 
                                    N_MOIN = NULL, 
                                    N_TAF = NULL, 
                                    HES1 = NULL 
                                WHERE (N_SERI = @N_SERI) 
                                  AND (BANK = @BANK) 
                                  AND (DATE_S = @DATE_S)";

                        dbms.DoExecuteSQL(updateSql, sqlParams);
                        // Query 2: Delete from CHRE_LSPH
                        string deleteSql = @"
                            DELETE FROM dbo.CHRE_LSPH 
                            WHERE (N_SERI = @N_SERI) 
                              AND (BANK = @BANK) 
                              AND (DATE_S = @DATE_S)";

                        dbms.DoExecuteSQL(deleteSql, sqlParams);
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

                    if (ErrosMessages.Any())
                    {
                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();
                    }

                    if (IsDeletedAny)
                    {
                        ReGetData(_navigationManager.CurrentRecord.DATE);
                    }
                }
            }

            _ = AuditLogger.LogActionAsync(
                actionType: "DELETE",
                tableName: "به حساب گذاشتن چكهاي دریافتی",
                recordId: _navigationManager.CurrentRecord.IDH?.ToString(),
                oldValue: "",
                newValue: null,
                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");


            if (new Msgwin(true, "آیا از حذف این سند اطمینان دارید؟").DialogResult == true)
            {
                dbms.DoExecuteSQL($"DELETE FROM CHKREC_H WHERE IDH = {_navigationManager.CurrentRecord.IDH}");
                if (!string.IsNullOrEmpty(N_S.Text))
                {
                    dbms.DoExecuteSQL($"DELETE FROM DEED_DTL WHERE N_S = {N_S.Text}");
                    dbms.DoExecuteSQL($"DELETE FROM DEED_HED WHERE N_S = {N_S.Text} AND NO_S = 6");
                }
                _navigationManager.DeleteCurrentRecord();
            }
        }

        private void PRINTC_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentRecord == null) return;

            try
            {
                var report = new StiReport();
                // Loading report from resource - matching WIN_CHREC_H pattern but using P_HESAB_CHEKG as per VBA
                // If P_HESAB_CHEKG.mrt doesn't exist in resources, this will fail. 
                // However, WIN_CHREC_H used ELVSL_ADA.mrt. 
                // VBA says: DoCmd.OpenReport "P_HESAB_CHEKG"
                // I'll try to load "Prg_UI.Rpts.Checkha.P_HESAB_CHEKG.mrt" or fallback to default if not found.
                // Or simply stick to what existing code does if P_HESAB_CHEKG is not standard.
                // Let's assume P_HESAB_CHEKG exists or I should use ELVSL_ADA if they are equivalent.
                // The VBA code implies P_HESAB_CHEKG takes "DATE" and "hes" (N_TAF).

                var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.Checkha.P_HESAB_CHEKG.mrt");
                if (pathreport == null)
                    pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.Checkha.ELVSL_ADA.mrt"); // Fallback

                if (pathreport != null)
                {
                    report.Load(pathreport);
                    string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                    report.Dictionary.Databases.Clear();
                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                    // Params
                    // VBA: "DATE = " & Me.DATE & " and hes = '" & Me.N_TAF & "'"
                    // StiReport usually filters via Params or Filter string.

                    // If the report expects parameters:
                    if (report.Dictionary.Variables.Contains("DATE"))
                        report["DATE"] = long.Parse(DATE.Text.ToRawTarikh());

                    if (N_TAF.SelectedValue != null && report.Dictionary.Variables.Contains("hes"))
                        report["hes"] = N_TAF.SelectedValue.ToString();

                    // Pass City/Chk if report has them
                    if (report.Dictionary.Variables.Contains("CITY")) report["CITY"] = CITY.Text;

                    new WINRPT(report, "چاپ برگه").Show();
                }
                else
                {
                    new Msgwin(false, "گزارش یافت نشد").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در چاپ: ").ShowDialog();
            }
        }

        private void N_S_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(N_S.Text))
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this, double.Parse(N_S.Text));
            }
        }
    }
}
