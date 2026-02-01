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
using Prg_UI.Wins.WinOther;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;


namespace Prg_UI.Wins.WinMenus.Taarif
{
    public partial class ENHESAR_WIN : Window
    {
        public ENHESAR_WIN(string? number_to_open = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER_TO_OPEN = (string)number_to_open;
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

        #region LOCALMODEL
        public class CNT_COMBO
        {
            public int? Code { get; set; }
            public string? CountriesName { get; set; }
        }
        public class HESAB_CMB_MODEL
        {
            public string? hes { get; set; }
            public string? NAME { get; set; }
            //public override string ToString() => NAME;
        }
        #endregion

        public bool NowIsReady { get; private set; }
        public string? NUMBER_TO_OPEN { get; set; }
        public bool IsOpenedFromAutomation { get; }
        public bool ChangeIsHappend { get; private set; }

        private int IRAN_CODE { get; } = 100038;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = WINDOW_ID;

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
        private List<COMBOPERSONEL> rst_personel;
        public nint WINDOW_ID { get; private set; }

        private NavigationManager<ENHESAR_MOSHTARI> _navigationManager;
        public ObservableCollection<ENHESAR_KALA> DetailData { get; set; } = new ObservableCollection<ENHESAR_KALA>();

        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                EN_STDATE.IsEnabled = ican;
                EN_ENDEN.IsEnabled = ican;
                EN_CITY.IsEnabled = ican;
                EN_IYALAT.IsEnabled = ican;
                EN_COUNTRY.IsEnabled = ican;
                USERID_REM.IsEnabled = ican;
                EN_CUST_CO.IsEnabled = ican;
                EN_CUST_CO2.IsEnabled = ican;

                ENHESAR_KALA_GRID.IsReadOnly = !ican;

                if (BTN_SAVE != null)
                    BTN_SAVE.Visibility = ican ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            WINDOW_ID = new WindowInteropHelper(this).Handle;

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "ENHESAR", WINDOW_ID, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            string WhereCondition = "";
            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE EN_CUST_CO = {NUMBER_TO_OPEN} ";
            }

            _navigationManager = new NavigationManager<ENHESAR_MOSHTARI>(
                dbms,
                x => x?.EN_CUST_CO.ToString(),
                $"SELECT * FROM dbo.ENHESAR_MOSHTARI {WhereCondition} ORDER BY CRT",
                x => $"SELECT * FROM dbo.ENHESAR_MOSHTARI WHERE EN_CUST_CO = N'{x?.EN_CUST_CO}' ",
                Convert.ToString(EN_CUST_CO.SelectedValue));

            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            CL_LMethods.SetTabIndexes(
                EN_STDATE,
                EN_ENDEN,
                EN_CUST_CO,
                EN_COUNTRY,
                EN_IYALAT,
                EN_CITY,
                USERID_REM,
                BTN_SAVE,
                ENHESAR_KALA_GRID
                );

            MakeDefaultFocuseReady();

            // Set defaults
            USERID.Text = Baseknow.USERCOD.ToString();
            USERID_REM.SelectedValue = Baseknow.USERCOD.ToString();
            EN_STDATE.CurrentDate = Tarikh.FullCurrentDate;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
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

        private void MakeDefaultFocuseReady()
        {
            EN_STDATE.GetFocus();
        }

        private void DataGridActivation()
        {
            if (_navigationManager.IsNewRecord)
            {
                ENHESAR_KALA_GRID.IsReadOnly = true;
            }
            else
            {
                ENHESAR_KALA_GRID.IsReadOnly = false;
            }
        }
        private void ClearFreshAll()
        {
            EN_CUST_CO.SelectedItem = null;
            USERID.Text = Baseknow.USERCOD.ToString();
            USERID_REM.SelectedValue = Baseknow.USERCOD;
            EN_STDATE.CurrentDate = Tarikh.FullCurrentDate;
            EN_ENDEN.CurrentDate = null;

            EN_COUNTRY.SelectedValue = IRAN_CODE;
            EN_IYALAT.SelectedValue = null;
            EN_CITY.SelectedValue = null;


            BTN_EDIT.IsEnabled = false; //ESLAH

            DetailData?.Clear();

            AllowEdits = true;

            ENHESAR_KALA_GRID.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }

        private bool OnInsertRecord(ENHESAR_MOSHTARI record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<ENHESAR_MOSHTARI>($"SELECT * FROM dbo.ENHESAR_MOSHTARI WHERE EN_CUST_CO = {EN_CUST_CO.SelectedValue}").FirstOrDefault();
                record = itemtoadd;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void OnCurrentRecordChanged(ENHESAR_MOSHTARI record)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
                //_navigationManager.ClearFreshNew(default, default, default, PRICE_ELAMIE_DTL_DATA);
            }
            else if (record == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین آیتمی ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                EN_STDATE.CurrentDate = record.EN_STDATE.ToString();
                EN_ENDEN.CurrentDate = record.EN_ENDEN?.ToString();
                EN_COUNTRY.SelectedValue = record.EN_COUNTRY;
                EN_IYALAT.SelectedValue = record.EN_IYALAT;
                EN_CITY.SelectedValue = record.EN_CITY;
                USERID_REM.SelectedValue = record.USERID_REM;

                string thevalue = record.EN_CUST_CO;
                var data = dbms.DoGetDataSQL<HESAB_CMB_MODEL>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();
                if (data != null)
                {
                    if (EN_CUST_CO.ItemsSource == null)
                    {
                        EN_CUST_CO.ItemsSource = new List<HESAB_CMB_MODEL>();
                    }

                    var sourceList = (List<HESAB_CMB_MODEL>)EN_CUST_CO.ItemsSource;
                    if (!sourceList.Any(item => item?.hes == thevalue))
                    {
                        sourceList.Add(new HESAB_CMB_MODEL { hes = thevalue, NAME = data.NAME });
                    }
                    EN_CUST_CO.Items.Refresh();
                }
                EN_CUST_CO.SelectedValue = thevalue;

                BTN_EDIT.IsEnabled = true; //ESLAH

                AllowEdits = false;

                DG_SUB_ReGetData();
            }
        }
        private void RefreshAfterUpdate()
        {
            var CURRENT_HEADER = dbms.DoGetDataSQL<ENHESAR_MOSHTARI>($"SELECT * FROM ENHESAR_MOSHTARI WHERE EN_CUST_CO = {EN_CUST_CO.SelectedValue}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }
        private void DG_SUB_ReGetData()
        {
            DetailData?.Clear();
            const string SQL_QUERY = @"
                    SELECT  I.EN_IDD,
                            I.EN_CUST_CO,
                            I.EN_KALA_COD,
                            S.NAME AS KALA_NAME,
                            I.EN_KALA_GR,
                            I.EN_GRCODE1,
                            I.EN_GRCODE2,
                            I.EN_GRCODE3,
                            I.EN_GRCODE4,
                            I.EN_GRCODE5,
                            I.EN_GRCODE6,
                            I.EN_GRCODE7,
                            I.EN_GRCODE8,
                            I.EN_GRCODE9,
                            I.EN_GRCODE10,
                            I.USERID,
                            I.EN_CDATE,
                            I.CRT,
                            I.UID
                    FROM  dbo.ENHESAR_KALA I
                    LEFT JOIN dbo.STUF_DEF S ON I.EN_KALA_COD = S.CODE
                    WHERE   I.EN_CUST_CO = @ECC";

            var QRE_LST = dbms.DoGetDataSQL<ENHESAR_KALA>(SQL_QUERY, new { ECC = Convert.ToString(EN_CUST_CO.SelectedValue) }).ToList();
            foreach (var item in QRE_LST)
            {
                DetailData?.Add(item);
            }
        }

        public List<TCOD_OSTAN> ALL_OSTAN { get; private set; }
        public List<TCOD_CITY> ALL_SHAHR { get; private set; }
        public string? ENTERED_VALUE_ROW { get; private set; }
        public ENHESAR_KALA? CURRENT_ITEMS_ROW { get; private set; }
        public ENHESAR_KALA? WAS_ROW_ITEM { get; private set; } = new();

        private void FILL_ALL_COMBOBOXES()
        {
            EN_CUST_CO.ItemsSource = new List<HESAB_CMB_MODEL>();
            //حساب یا کد مشتریان
            EN_CUST_CO2.ItemsSource = EN_CUST_CO.ItemsSource;
            EN_CUST_CO2.DisplayMemberPath = "hes";
            EN_CUST_CO2.SelectedValuePath = "hes";

            // Countries
            var countries = dbms.DoGetDataSQL<CNT_COMBO>("SELECT Code, CountriesName FROM TCOD_Countries ORDER BY CountriesName").ToList();
            EN_COUNTRY.ItemsSource = countries;
            EN_COUNTRY.SelectedValue = IRAN_CODE; // Default to Iran/User's country if known (Access had 100038)

            //کد استان
            ALL_OSTAN = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();
            foreach (var item in ALL_OSTAN) { item.OSNAME = item.OSNAME?.FixPersianChars(); }
            //کد شهر
            ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();
            EN_IYALAT.ItemsSource = ALL_OSTAN;
            EN_CITY.ItemsSource = ALL_SHAHR;

            // Reminder Users (SALA_DTL)
            string sql = @"
                SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD as USERCO
                FROM SALA_DTL sd
                LEFT JOIN USER_PERSONEL_ORDER uo 
                     ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
                WHERE sd.ENABL = 0
                ORDER BY
                     CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                     uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var rows in rst_personel)
            {
                if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }
            USERID_REM.ItemsSource = rst_personel;
            USERID_REM.SelectedValue = Baseknow.USERCOD;


            try
            {
                // Access Logic:
                // rst.Open "TCOD_MAP_GRP" ...
                // Me("LCOL1").CAPTION = rst.Fields("MPNAME")
                // Me("EN_GRCODE1").RowSource = ... WHERE (TCOD_MAP.MPP = 1)

                var groups = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT * FROM TCOD_MAP_GRP ORDER BY MPP").ToList();
                int activeGroupsCount = 0;
                if (groups.Count > 0)
                {
                    // Access loops from 2 to Count for columns, but let's just map MPP 1 to 10
                    // Access Code: 
                    // 1. MPP=1 -> EN_GRCODE1
                    // 2. Loop 2 to RecordCount -> EN_GRCODE i

                    // We will map based on MPP value
                    foreach (var grp in groups)
                    {
                        if (grp.MPP.HasValue && grp.MPP.Value >= 1 && grp.MPP.Value <= 10)
                        {
                            int idx = grp.MPP.Value;
                            activeGroupsCount = Math.Max(activeGroupsCount, idx);

                            // Find column by name (COL_GR1 ... COL_GR10)
                            System.Windows.Controls.DataGridComboBoxColumn col = this.FindName($"COL_GR{idx}") as System.Windows.Controls.DataGridComboBoxColumn;
                            if (col != null)
                            {
                                col.Header = grp.MPNAME;

                                // Load RowSource
                                // SELECT RIGHT('0000' + CAST(TCOD_MAP.MPCODE AS NVARCHAR), TCOD_MAP_GRP.SIZEF) AS Expr1, TCOD_MAP.MPNAME 
                                // FROM TCOD_MAP INNER JOIN TCOD_MAP_GRP ON TCOD_MAP.MPP = TCOD_MAP_GRP.MPP 
                                // WHERE (TCOD_MAP.MPP = @MPP)

                                // We need a simple list for ComboBox: MPCODE (Value), MPNAME (Display)
                                // Note: Access uses String for EN_GRCODE columns? The table definition says nvarchar(20).
                                // So SelectedValuePath should be capable of handling the formatted string if needed, 
                                // but the table stores just the code part? 
                                // SQL Table: [EN_GRCODE1] [nvarchar] (20)
                                // VBA Logic: SUBSTRING(N_FANI, ...)

                                // Replicating logic: RIGHT('0000' + CAST(TCOD_MAP.MPCODE AS NVARCHAR), TCOD_MAP_GRP.SIZEF)
                                var mapItems = dbms.DoGetDataSQL<dynamic>(
                                    $@"SELECT RIGHT('0000' + CAST(TCOD_MAP.MPCODE AS NVARCHAR(20)), TCOD_MAP_GRP.SIZEF) AS MPCODE, TCOD_MAP.MPNAME 
                                       FROM TCOD_MAP 
                                       INNER JOIN TCOD_MAP_GRP ON TCOD_MAP.MPP = TCOD_MAP_GRP.MPP 
                                       WHERE (TCOD_MAP.MPP = {idx}) 
                                       ORDER BY TCOD_MAP.MPCODE").ToList();

                                if (idx == 1)
                                {
                                    // Add "All" option for MPP 1, matching VBA behavior
                                    mapItems.Insert(0, new { MPCODE = "-1", MPNAME = "همه" });
                                }

                                col.ItemsSource = mapItems;
                            }
                        }
                    }

                    //مخفی کردن ستون‌های اضافی
                    for (int i = 1; i <= 10; i++)
                    {
                        var col = this.FindName($"COL_GR{i}") as DataGridColumn;
                        if (col != null)
                        {
                            col.Visibility = (i <= activeGroupsCount) ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    // Hide columns or set default? Access sets them to " "
                    // We can leave them as "Code Group X" or hide them. 
                    // For now, leave as XAML defaults.
                }
            }
            catch (Exception ex)
            {
                // Log or ignore
            }
        }
        private void EN_COUNTRY_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (EN_COUNTRY.SelectedItem is CNT_COMBO selectedCountry)
            {
                if (selectedCountry.Code != IRAN_CODE)
                {
                    EN_COUNTRY.SelectedValue = IRAN_CODE;
                    new Msgwin(false, "در حال حاضر امکان انتخاب کشوری جز ایران مقدور نیست").ShowDialog();
                }
            }
        }

        private void EN_COUNTRY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EN_COUNTRY.SelectedValue is null)
            {
                EN_IYALAT.IsEnabled = false;
                EN_CITY.IsEnabled = false;
            }
            else if (AllowEdits)
            {
                EN_IYALAT.IsEnabled = true;
                EN_CITY.IsEnabled = true;
            }
        }
        private void EN_IYALAT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EN_IYALAT.SelectedValue is null)
            {
                EN_CITY.IsEnabled = false;
            }
            else if (AllowEdits)
            {
                EN_CITY.IsEnabled = true;
            }
        }
        private void EN_IYALAT_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (EN_IYALAT.SelectedItem is TCOD_OSTAN selectedostan)
            {
                if (selectedostan?.OSCODE != null)
                {
                    var FILTERRED_CITY = dbms.DoGetDataSQL<TCOD_CITY>($"SELECT  CITYCODE, CITYNAME, OSCODE FROM TCOD_CITY WHERE (OSCODE ={selectedostan.OSCODE}) ORDER BY CITYNAME").ToList();

                    foreach (var item in FILTERRED_CITY) { item.CITYNAME = item.CITYNAME?.FixPersianChars(); }

                    int? LastCityValue = Convert.ToInt32(EN_CITY.SelectedValue ?? 0);

                    EN_CITY.ItemsSource = FILTERRED_CITY; //Reset ItemsSource to Filtered City base on Province

                    if (LastCityValue != null && LastCityValue > -1)
                    {
                        EN_CITY.SelectedValue = LastCityValue;
                    }
                    else
                    {
                        EN_CITY.SelectedIndex = 0;
                    }
                }
            }
        }
        private void EN_STDATE_DateChanged(object sender, string e)
        {
            //DT1 = e.ToRawTarikh();
        }
        private void EN_ENDEN_DateChanged(object sender, string e)
        {

        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var BEGIN_DATE = EN_STDATE.CurrentDate.ToRawTarikh(); string BG_CP = " تاریخ شروع انحصار ";
            var END_DATE = EN_ENDEN.CurrentDate.ToRawTarikh(); string END_CP = " تاریخ پایان انحصار ";

            if (!Tarikh.IsValidedDate(BEGIN_DATE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"{BG_CP} صحیح نمی باشد" });
            }
            else
            {
                if (!Tarikh.IsSyncedDateNow(BEGIN_DATE, Baseknow.CTL_DT ?? false))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"{BG_CP} مربوط به سال جاری نیست" });
                }
            }

            if (!string.IsNullOrWhiteSpace(END_DATE))
            {
                if (!Tarikh.IsValidedDate(END_DATE))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"{END_CP} صحیح نمی باشد" });
                }
                else if (!Tarikh.IsSyncedDateNow(END_DATE, Baseknow.CTL_DT ?? false))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"{END_CP} مربوط به سال جاری نیست" });
                }


                if (Convert.ToInt32(BEGIN_DATE) > Convert.ToInt32(END_DATE))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"{BG_CP} نمیتواند بزرگتر از {END_CP} باشد" });
                }

            }

            if (EN_CUST_CO.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"مشتری نمیتواند خالی باشد" });
            }

            if (EN_COUNTRY.SelectedValue is null)
            {
                EN_COUNTRY.SelectedValue = IRAN_CODE;
            }

            if (EN_IYALAT.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"استان نمیتواند خالی باشد" });
            }

            if (USERID_REM.SelectedValue is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"کاربر یادآوری کننده نمیتواند خالی باشد" });
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

        #region Customer Logic

        private void EN_CUST_CO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (EN_CUST_CO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)EN_CUST_CO.Template.FindName("PART_EditableTextBox", EN_CUST_CO);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (EN_CUST_CO.SelectedValue is not null)
            {
                if ((EN_CUST_CO.SelectedItem as HESAB_CMB_MODEL)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(CUTSNO_TEX?.Text))
            {
                var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(default, dbms, CUTSNO_TEX.Text);
                if (!string.IsNullOrEmpty(_SelectedHesab_?.hes))
                {
                    var FoundCust = new HESAB_CMB_MODEL() { hes = _SelectedHesab_.hes, NAME = _SelectedHesab_.NAME };
                    SetCustomer(FoundCust);
                }
            }
            else if (EN_CUST_CO.SelectedItem is HESAB_CMB_MODEL CustomerValueHes)
            {
                string? StoredCustomer = _navigationManager?.CurrentRecord?.EN_CUST_CO;
                if (!string.IsNullOrWhiteSpace(StoredCustomer))
                {
                    universControl.PopNotifyShow($"مشتری نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                }
            }


            if (EN_CUST_CO.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(EN_CUST_CO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    EN_CUST_CO.SelectedValue = null;
                }
                if (CL_HESABDARI.BLOCKEDCUST(EN_CUST_CO.SelectedValue.ToString()))
                {
                    EN_CUST_CO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

            return;
            if (EN_CUST_CO.IsEditable)
            {
                //TextBox textBox = EN_CUST_CO.Template.FindName("PART_EditableTextBox", EN_CUST_CO) as TextBox;
                //if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
                //{
                //    string text = textBox.Text.Trim();

                //    // Prevent reloading if the selection matches the text
                //    if (EN_CUST_CO.SelectedItem is CTABLES.Custom_CUST_HESAB selected && selected.NAME == text)
                //        return;

                //    if (text == "+" || text == "++")
                //    {
                //        // Open Search
                //        ComboSearch searchWin = new ComboSearch("ENHESAR", this);
                //        searchWin.ShowDialog();
                //        // Handle result if ComboSearch updates something or returns value
                //        return;
                //    }

                //    if (CL_LMethods.IsNumeric(text))
                //    {
                //        // Search by Code logic (mimicking HEAD_LST_FROOSH22)
                //        // Access logic: select n_kol , NUMBER,tNUMBER from tdeta_hes where n_kol = BEDEHKAR ...
                //        // Assuming standard Customer Search query
                //        var cust = dbms.DoGetDataSQL<CTABLES.Custom_CUST_HESAB>($"SELECT TOP 1 hes, NAME FROM CUST_HESAB WHERE hes LIKE '%{text}%' OR NAME LIKE N'%{text}%'").FirstOrDefault();
                //        if (cust != null)
                //        {
                //            SetCustomer(cust);
                //        }
                //    }
                //    else
                //    {
                //        // Search by Name
                //        var cust = dbms.DoGetDataSQL<CTABLES.Custom_CUST_HESAB>($"SELECT TOP 1 hes, NAME FROM CUST_HESAB WHERE NAME LIKE N'%{text}%'").FirstOrDefault();
                //        if (cust != null)
                //        {
                //            SetCustomer(cust);
                //        }
                //    }
                //}
            }
        }

        private void EN_CUST_CO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Trigger lost focus logic or custom search
                // For now, rely on LostFocus or selection change
            }
        }

        private void SetCustomer(HESAB_CMB_MODEL cust)
        {
            var list = EN_CUST_CO.ItemsSource as List<HESAB_CMB_MODEL>;
            if (list == null) list = new List<HESAB_CMB_MODEL>();

            if (!list.Any(x => x.hes == cust.hes))
            {
                list.Add(cust);
                EN_CUST_CO.ItemsSource = null;
                EN_CUST_CO.ItemsSource = list;
            }

            EN_CUST_CO.SelectedValue = cust.hes;
            //LoadMasterData(cust.hes);
        }

        #endregion

        #region Actions

        private void BTN_EDIT_Click(object sender, RoutedEventArgs e)
        {
            if (EN_CUST_CO.SelectedValue == null) return;

            CallTR();
            AllowEdits = true;
        }

        private void CallTR()
        {
            if (EN_CUST_CO.SelectedValue == null) return;
            string custCo = EN_CUST_CO.SelectedValue.ToString();
            string cityCondition = EN_CITY.SelectedValue != null ? $" and (EN_CITY = {EN_CITY.SelectedValue})" : "";

            string where = $"(EN_CUST_CO = '{custCo}'){cityCondition}";

            try
            {
                CL_HESABDARI.TR("ENHESAR_KALA", where, DateTime.Now, 1);
                CL_HESABDARI.TR("ENHESAR_MOSHTARI", where, DateTime.Now, 1);
            }
            catch { /* Ignore logging errors */ }
        }

        private void BTN_PRINT_Click(object sender, RoutedEventArgs e)
        {
            if (EN_CUST_CO.SelectedValue == null) return;

            // Report logic: "GRADE_REP"
            // DoCmd.OpenReport "GRADE_REP", acViewPreview, , "EN_CUST_CO='" & Trim(Me.EN_CUST_CO) & "'"
            // I need to check if there is a Stimulsoft report for this. If not, I'll show a message.
            new Msgwin(false, "گزارش مربوطه یافت نشد (GRADE_REP)").Show();
        }

        #endregion

        private void SaveMaster()
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in ENHESAR_KALA_GRID.ItemsSource
                          let c = ENHESAR_KALA_GRID.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                DG_SUB_CANCEL_EDIT(); ApplyDataGridItems(); ENHESAR_KALA_GRID.Items.Refresh();
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            string custCo = EN_CUST_CO.SelectedValue.ToString();

            try
            {
                // Check if exists
                //var exists = dbms.DoGetDataSQL<int>("SELECT COUNT(*) FROM ENHESAR_MOSHTARI WHERE EN_CUST_CO = @CustCo", new { CustCo = custCo }).FirstOrDefault() > 0;

                string sql = "";

                int stDate = string.IsNullOrEmpty(EN_STDATE.CurrentDate) ? 0 : Convert.ToInt32(EN_STDATE.CurrentDate.ToRawTarikh());
                int? enDate = string.IsNullOrEmpty(EN_ENDEN.CurrentDate) ? (int?)null : Convert.ToInt32(EN_ENDEN.CurrentDate.ToRawTarikh());
                int? city = EN_CITY.SelectedValue as int?;
                int? iyalat = EN_IYALAT.SelectedValue as int?;
                int? country = EN_COUNTRY.SelectedValue as int?;
                int? userRem = USERID_REM.SelectedValue as int?;

                var parameters = new
                {
                    CustCo = custCo,
                    StDate = stDate,
                    EnDate = enDate,
                    City = city,
                    Iyalat = iyalat,
                    Country = country,
                    UserRem = userRem ?? 0,
                    UserId = Baseknow.USERCOD
                };

                if (_navigationManager.IsNewRecord)
                {
                    sql = @"INSERT INTO ENHESAR_MOSHTARI (EN_CUST_CO, EN_STDATE, EN_ENDEN, EN_CITY, EN_IYALAT, EN_COUNTRY, USERID_REM, USERID, EN_CDATE)
                            VALUES (@CustCo, @StDate, @EnDate, 
                                    @City, @Iyalat, 
                                    @Country, @UserRem, 
                                    @UserId, GETDATE())";
                }
                else
                {
                    sql = @"UPDATE ENHESAR_MOSHTARI SET 
                            EN_STDATE = @StDate, 
                            EN_ENDEN = @EnDate,
                            EN_CITY = @City,
                            EN_IYALAT = @Iyalat,
                            EN_COUNTRY = @Country,
                            USERID_REM = @UserRem,
                            USERID = @UserId
                            WHERE EN_CUST_CO = @CustCo";
                }

                dbms.DoExecuteSQL(sql, parameters);

                if (_navigationManager.IsNewRecord)
                {
                    RefreshAfterUpdate();
                }

                // Remainder Logic
                if (enDate.HasValue)
                {
                    CheckAndAddRemainder(custCo, enDate.Value, userRem);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"این مورد قبلا تعریف شده نمیتوان تکراری تعریف کرد").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره اطلاعات: " + ex.Message).Show();
            }
        }
        private void SaveOnLostFocus(object sender, RoutedEventArgs e)
        {
            SaveMaster();
        }
        private void CheckAndAddRemainder(string custCo, int enDate, int? userRem)
        {
            // Access: EN_ENDEN_AfterUpdate
            // rst.Open "REMAINDER"...
            if (userRem == null || userRem == 0) return;

            string payam = $"پايان انحصار محصولات براي {custCo} {CL_HESABDARI.GETHESNAME(custCo)} در تاريخ : {Strings.Format(enDate)}";

            // Check if already exists to avoid dupes? Access code blindly adds. I'll add blindly too.
            string sql = @"INSERT INTO REMAINDER (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, CTDATE, CTTIME, SMSOK, USERNAME)
                            VALUES (@Personel, @CompCod, @Payam, 1, @StDate, @StTime, 
                                    @CtDate, GETDATE(), 1, @UserName)";

            var parameters = new
            {
                Personel = userRem,
                CompCod = Baseknow.USERCOD,
                Payam = payam,
                StDate = enDate,
                StTime = DateTime.Now.ToString("HH:mm:ss"),
                CtDate = Tarikh.FullCurrentDate,
                UserName = Baseknow.UUSER
            };

            dbms.DoExecuteSQL(sql, parameters);
        }

        private void ApplyDataGridItems()
        {
            if (ENHESAR_KALA_GRID.Items is IEditableCollectionView editableCollectionView)
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
        private void DG_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            ENHESAR_KALA_GRID.Dispatcher.Invoke(() =>
            {
                ENHESAR_KALA_GRID.CellEditEnding -= ENHESAR_KALA_GRID_CellEditEnding;
                ENHESAR_KALA_GRID.RowEditEnding -= ENHESAR_KALA_GRID_RowEditEnding;
                if (_RC_ is null)
                {
                    ENHESAR_KALA_GRID.CancelEdit();
                    //DG_SUB.CommitEdit(DataGridEditingUnit.Row, true);
                }
                else
                {
                    ENHESAR_KALA_GRID.CancelEdit((DataGridEditingUnit)_RC_);
                    //DG_SUB.CommitEdit((DataGridEditingUnit)_RC_, true);
                }
                ENHESAR_KALA_GRID.RowEditEnding += ENHESAR_KALA_GRID_RowEditEnding;
                ENHESAR_KALA_GRID.CellEditEnding += ENHESAR_KALA_GRID_CellEditEnding;
            });
        }
        private void ENHESAR_KALA_GRID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && AllowEdits)
            {
                if (ENHESAR_KALA_GRID.SelectedItems.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از حذف ردیف(های) انتخاب شده مطمئن هستید؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        var itemsToDelete = ENHESAR_KALA_GRID.SelectedItems.Cast<ENHESAR_KALA>().ToList();
                        foreach (var item in itemsToDelete)
                        {
                            if (item.EN_IDD != 0)
                            {
                                dbms.DoExecuteSQL("DELETE FROM ENHESAR_KALA WHERE EN_IDD = @EN_IDD", new { EN_IDD = item.EN_IDD });
                            }
                            DetailData.Remove(item);
                        }
                    }
                    e.Handled = true;
                }
            }
        }
        private void ENHESAR_KALA_GRID_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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

            CURRENT_ITEMS_ROW = e.Row.Item as ENHESAR_KALA;
            #endregion


            if (e.EditAction == DataGridEditAction.Commit)
            {
                var row = e.Row.Item as ENHESAR_KALA;
                string colHeader = e.Column.Header.ToString();

                // Validation Rule: EN_KALA_COD vs Groups
                if (e.Column.SortMemberPath == "EN_KALA_COD" || e.Column.SortMemberPath == "KALA_NAME")
                {
                    //کالا
                    if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.KALA_NAME.ToStringNullSafe().Trim() ||
                             string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                        {
                            CURRENT_ITEMS_ROW.EN_KALA_COD = WAS_ROW_ITEM.EN_KALA_COD;
                            CURRENT_ITEMS_ROW.KALA_NAME = WAS_ROW_ITEM.KALA_NAME;
                            DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            return;
                        }

                        var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, default, ENTERED_VALUE_ROW);
                        if (RST_KALA != null)
                        {
                            CURRENT_ITEMS_ROW.EN_KALA_COD = RST_KALA.CODE;
                            CURRENT_ITEMS_ROW.KALA_NAME = RST_KALA.NAME_CODE;
                            //CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                        }
                        else
                        {
                            CURRENT_ITEMS_ROW.EN_KALA_COD = WAS_ROW_ITEM.EN_KALA_COD;
                            CURRENT_ITEMS_ROW.KALA_NAME = WAS_ROW_ITEM.KALA_NAME;

                            //CURRENT_ITEMS_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;

                            DG_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                            return;
                        }
                    }


                    //var tb = e.EditingElement as TextBox;
                    //string newVal = tb?.Text;

                    //if (!string.IsNullOrWhiteSpace(newVal))
                    //{
                    //    // Handle +/-
                    //    if (newVal == "+" || newVal == "++" || newVal == "-")
                    //    {
                    //        tb.Text = "";
                    //        ComboSearch searchWin = new ComboSearch("ENHESAR", this);
                    //        searchWin.ShowDialog();
                    //    }
                    //    else
                    //    {
                    //        // Validate Numeric & Name
                    //        if (CL_LMethods.IsNumeric(newVal))
                    //        {
                    //            string kName = CL_HESABDARI.GETKALANAME(Convert.ToDouble(newVal));
                    //            if (string.IsNullOrWhiteSpace(kName) || kName == " ")
                    //            {
                    //                // Invalid
                    //                // Access logic: If GETKALANAME(...) <> " " Then Me.EN_KALA_COD = NewData
                    //                // So if invalid, we should probably revert or warn.
                    //                // I'll show a warning.
                    //                new Msgwin(false, "کالایی با این کد پیدا نشد").Show();
                    //                // e.Cancel = true; // Optional: Force stay in cell
                    //            }
                    //        }
                    //    }
                    //}

                }

                // Group Columns Validation
                // Access: If Not IsNull(Me.EN_KALA_COD) Then Msg "Cannot set group if Kala is set"
                // Must check SortMemberPath because Header is dynamic
                if (e.Column.SortMemberPath != null && e.Column.SortMemberPath.StartsWith("EN_GRCODE"))
                {
                    if (!string.IsNullOrWhiteSpace(row.EN_KALA_COD))
                    {
                        new Msgwin(false, "زماني که کالا را مشخص ميکنيد نميتوانيد گروه مشخص کنيد يا کالا يا ساير گروه بندي ها").Show();
                        e.Cancel = true; // Cancel the edit
                    }
                }

                // Vice Versa: If setting Kala, check if Groups are set? 
                // Access only checks Group updates. But logical reciprocal is valid too.
                // However, strictly following VBA: "EN_GRCODE..._BeforeUpdate" checks "EN_KALA_COD".
                // I'll stick to that.
            }
        }

        private void ENHESAR_KALA_GRID_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var row = e.Row.Item as ENHESAR_KALA;
                if (row == null) return;
                if (EN_CUST_CO.SelectedValue == null) return;

                // Save Row
                SaveDetailRow(row);
            }
        }

        private void SaveDetailRow(ENHESAR_KALA row)
        {
            try
            {
                row.EN_CUST_CO = EN_CUST_CO.SelectedValue.ToString();
                row.USERID = Baseknow.USERCOD ?? 0;

                if (row.EN_IDD == 0)
                {
                    //var maxId = dbms.DoGetDataSQL<int?>("SELECT MAX(EN_IDD) FROM ENHESAR_KALA").FirstOrDefault();
                    long maxId = CL_HESABDARI.GetLIDD("ENHESAR_KALA", "EN_IDD");

                    row.EN_IDD = (int)maxId;

                    string sql = @"INSERT INTO ENHESAR_KALA (EN_IDD, EN_CUST_CO, EN_KALA_COD, EN_KALA_GR, 
                                    EN_GRCODE1, EN_GRCODE2, EN_GRCODE3, EN_GRCODE4, EN_GRCODE5, 
                                    EN_GRCODE6, EN_GRCODE7, EN_GRCODE8, EN_GRCODE9, EN_GRCODE10, 
                                    USERID, EN_CDATE)
                                    VALUES (@EN_IDD, @EN_CUST_CO, @EN_KALA_COD, @EN_KALA_GR,
                                    @EN_GRCODE1, @EN_GRCODE2, @EN_GRCODE3, @EN_GRCODE4, @EN_GRCODE5,
                                    @EN_GRCODE6, @EN_GRCODE7, @EN_GRCODE8, @EN_GRCODE9, @EN_GRCODE10,
                                    @USERID, GETDATE())";
                    dbms.DoExecuteSQL(sql, row);
                }
                else
                {
                    string sql = @"UPDATE ENHESAR_KALA SET 
                                    EN_KALA_COD = @EN_KALA_COD, 
                                    EN_KALA_GR = @EN_KALA_GR,
                                    EN_GRCODE1 = @EN_GRCODE1,
                                    EN_GRCODE2 = @EN_GRCODE2,
                                    EN_GRCODE3 = @EN_GRCODE3,
                                    EN_GRCODE4 = @EN_GRCODE4,
                                    EN_GRCODE5 = @EN_GRCODE5,
                                    EN_GRCODE6 = @EN_GRCODE6,
                                    EN_GRCODE7 = @EN_GRCODE7,
                                    EN_GRCODE8 = @EN_GRCODE8,
                                    EN_GRCODE9 = @EN_GRCODE9,
                                    EN_GRCODE10 = @EN_GRCODE10
                                    WHERE EN_IDD = @EN_IDD";
                    dbms.DoExecuteSQL(sql, row);
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره ردیف: " + ex.Message).Show();
            }
        }


    }
}
