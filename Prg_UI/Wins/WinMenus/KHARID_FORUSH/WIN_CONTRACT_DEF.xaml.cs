using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Wins.WinOther;
using Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Wins.WinOther;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_CONTRACT_DEF : Window, ISearchableWindow
    {
        private readonly CL_CCNNMANAGER dbms = new();
        private readonly ObservableCollection<ContractDtlModel> ContractDetails = new();
        private int? CurrentContractID;
        private bool isLoading;
        private bool can;
        public bool AllowEdits
        {
            get => can;
            private set
            {
                can = value;
                ApplyEditingState();
            }
        }
        private NavigationManager<ContractHeaderModel>? navigationManager;

        public WIN_CONTRACT_DEF()
        {
            InitializeComponent();
            DG_DTL.ItemsSource = ContractDetails;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CL_MenuManager.IsContractTrackingEnabled)
            {
                new Msgwin(false, "قابلیت قراردادها برای این شرکت فعال نیست.").ShowDialog();
                Close();
                return;
            }

            try
            {
                ContractDate.Text = FormatDate(Tarikh.FullCurrentDate);
                var customers = new List<Custom_CUST_HESAB>();
                CUST_NO.ItemsSource = customers;
                CUST_NO2.ItemsSource = customers;
                LoadBrands();
                InitializeNavigation();
            }
            catch (Exception ex)
            {
                ShowError("بارگذاری اطلاعات قراردادها انجام نشد.", ex);
            }
        }

        private void InitializeNavigation()
        {
            navigationManager = new NavigationManager<ContractHeaderModel>(
                dbms,
                x => x.ContractID.ToString(CultureInfo.InvariantCulture),
                @"SELECT ContractID, ContractNo, ContractDate, CUST_NO, BrandName, TotalQty, MOLAH, IsClosed
FROM dbo.CONTRACT_HED ORDER BY ContractDate, ContractID",
                x => $@"SELECT TOP (1) ContractID, ContractNo, ContractDate, CUST_NO, BrandName, TotalQty, MOLAH, IsClosed
FROM dbo.CONTRACT_HED WHERE ContractID={x.ContractID}");
            navigationManager.CurrentRecordChanged += OnCurrentContractChanged;
            navigatorControl.NavigationManager = navigationManager;
            navigationManager.RaiseInitializationEvents();
        }

        private void OnCurrentContractChanged(ContractHeaderModel? header)
        {
            if (header is null) BeginNewContract();
            else LoadContract(header);
        }

        private void RefreshNavigation(int? selectContractID = null)
        {
            if (navigationManager is null) return;
            navigationManager.ReloadData();
            if (!selectContractID.HasValue) return;
            int index = navigationManager.RecordsData.ToList().FindIndex(x => x.ContractID == selectContractID.Value);
            if (index >= 0) navigationManager.MoveReGetData(global::Interfaces.INavigator.Jahat.CustomPosition, index);
        }

        object ISearchableWindow.GetSearchSource() => navigationManager?.RecordsData ?? Enumerable.Empty<ContractHeaderModel>();

        public void OnSearchResultSelected(object selectedItem)
        {
            if (selectedItem is not ContractHeaderModel selected) return;
            if (navigationManager is null) return;
            int index = navigationManager.RecordsData.ToList().FindIndex(x => x.ContractID == selected.ContractID);
            if (index >= 0) navigationManager.MoveReGetData(global::Interfaces.INavigator.Jahat.CustomPosition, index);
        }

        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
                new SearchableProperty { DisplayName = "شماره قرارداد", PropertyPath = nameof(ContractHeaderModel.ContractNo), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "تاریخ قرارداد", PropertyPath = nameof(ContractHeaderModel.ContractDate), PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = nameof(ContractHeaderModel.CUST_NO), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "برند", PropertyPath = nameof(ContractHeaderModel.BrandName), PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "مقدار تعهد", PropertyPath = nameof(ContractHeaderModel.TotalQty), PropertyType = typeof(decimal) },
                new SearchableProperty { DisplayName = "مختومه", PropertyPath = nameof(ContractHeaderModel.IsClosed), PropertyType = typeof(bool) }
            };
        }

        private void LoadBrands(string? selectedBrand = null)
        {
            var brands = dbms.DoGetDataSQL<BrandLookup>(@"
SELECT NAMES = BrandName
FROM dbo.CONTRACT_HED
WHERE NULLIF(LTRIM(RTRIM(BrandName)), N'') IS NOT NULL
GROUP BY BrandName
ORDER BY BrandName").ToList();
            BrandName.ItemsSource = brands;
            if (!string.IsNullOrWhiteSpace(selectedBrand))
                BrandName.Text = selectedBrand;
        }

        private void BeginNewContract()
        {
            isLoading = true;
            try
            {
                CurrentContractID = null;
                ContractNo.Clear();
                ContractDate.Text = FormatDate(Tarikh.FullCurrentDate);
                BrandName.SelectedValue = null;
                BrandName.Text = string.Empty;
                MOLAH.Clear();
                CUST_NO.SelectedIndex = -1;
                CUST_NO.Text = string.Empty;
                CUST_NO2.SelectedIndex = -1;
                CUST_NO2.Text = string.Empty;
                IsClosed.IsChecked = false;
                ContractDetails.Clear();
                AllowEdits = true;
                CalculateTotal();
                LBL_STATUS.Text = "ابتدا سربرگ قرارداد را ذخیره کنید؛ سپس امکان ثبت طرح‌ها فعال می‌شود.";
                ContractNo.Focus();
            }
            finally { isLoading = false; }
        }

        private void LoadContract(ContractHeaderModel header)
        {
            isLoading = true;
            try
            {
                CurrentContractID = header.ContractID;
                ContractNo.Text = header.ContractNo;
                ContractDate.Text = FormatDate(header.ContractDate.ToString(CultureInfo.InvariantCulture));
                LoadBrands(header.BrandName);
                MOLAH.Text = header.MOLAH ?? string.Empty;
                SelectCustomer(header.CUST_NO);
                IsClosed.IsChecked = header.IsClosed;
                ContractDetails.Clear();
                foreach (var detail in dbms.DoGetDataSQL<ContractDtlModel>(
                    @"SELECT D.ID, D.CODE, D.Qty, NAME_CODE = COALESCE(S.NAME, N''), UnitName = COALESCE(U.NAMES, N'واحد پایه')
FROM dbo.CONTRACT_DTL AS D
LEFT JOIN dbo.STUF_DEF AS S ON S.CODE = D.CODE
LEFT JOIN dbo.TCOD_VAHEDS AS U ON U.CODE = S.VAHED
WHERE D.ContractID = @ContractID
ORDER BY D.ID",
                    new { header.ContractID }))
                    ContractDetails.Add(detail);
                AllowEdits = false;
                CalculateTotal();
                LBL_STATUS.Text = $"مشاهده قرارداد {header.ContractNo}؛ برای تغییر اطلاعات دکمه اصلاح را انتخاب کنید.";
            }
            finally { isLoading = false; }
        }

        private void SelectCustomer(string customerCode)
        {
            if (string.IsNullOrWhiteSpace(customerCode)) return;
            var customer = dbms.DoGetDataSQL<Custom_CUST_HESAB>(
                "SELECT TOP (1) hes, NAME = COALESCE(NAME, N'') FROM dbo.CUST_HESAB WHERE hes = @hes",
                new { hes = customerCode }).FirstOrDefault();
            if (customer is null) return;
            AddCustomerToLookup(customer);
            CUST_NO.SelectedValue = customer.hes;
            CUST_NO2.SelectedValue = customer.hes;
            CUST_NO.Items.Refresh();
            CUST_NO2.Items.Refresh();
        }

        private void AddCustomerToLookup(Custom_CUST_HESAB customer)
        {
            if (CUST_NO.ItemsSource is not List<Custom_CUST_HESAB> customers)
            {
                customers = new List<Custom_CUST_HESAB>();
                CUST_NO.ItemsSource = customers;
                CUST_NO2.ItemsSource = customers;
            }
            if (!customers.Any(x => string.Equals(x.hes, customer.hes, StringComparison.OrdinalIgnoreCase)))
                customers.Add(customer);
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable && e.OriginalSource is not TextBox) return;
            if (CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO) is not TextBox editor) return;
            string enteredValue = editor.Text.Trim();
            if (CUST_NO.SelectedItem is Custom_CUST_HESAB selected && selected.NAME == enteredValue) return;

            try
            {
                Custom_CUST_HESAB? customer = null;
                if (enteredValue is "+" or "++")
                {
                    var search = new ComboSearch("HEAD_LST_PISHFROOSH2", this);
                    search.ShowDialog();
                    customer = search.SELECTED_HESAB;
                }
                else if (int.TryParse(new string(enteredValue.Select(ToEnglishDigit).ToArray()), out int detailNumber))
                {
                    customer = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"
SELECT TOP (1) C.hes, NAME = COALESCE(C.NAME, N'')
FROM dbo.TDETA_HES AS T
INNER JOIN dbo.CUST_HESAB AS C
    ON C.hes = CONCAT(T.N_KOL, N'-', T.NUMBER, N'-', T.TNUMBER)
WHERE T.N_KOL = @N_KOL AND T.NUMBER = 1 AND T.TNUMBER = @TNUMBER",
                        new { N_KOL = Baseknow.BEDEHKAR, TNUMBER = detailNumber }).FirstOrDefault();
                }
                else if (!string.IsNullOrWhiteSpace(enteredValue))
                {
                    customer = dbms.DoGetDataSQL<Custom_CUST_HESAB>(
                        "SELECT TOP (1) hes, NAME = COALESCE(NAME, N'') FROM dbo.CUST_HESAB WHERE hes = @hes",
                        new { hes = enteredValue }).FirstOrDefault();
                }

                if (customer is null || string.IsNullOrWhiteSpace(customer.hes))
                {
                    CUST_NO.SelectedValue = null;
                    CUST_NO.Text = string.Empty;
                    CUST_NO2.SelectedValue = null;
                    CUST_NO2.Text = string.Empty;
                    return;
                }

                AddCustomerToLookup(customer);
                CUST_NO.SelectedValue = customer.hes;
                CUST_NO2.SelectedValue = customer.hes;
                CUST_NO.Items.Refresh();
                CUST_NO2.Items.Refresh();
            }
            catch (Exception ex) { ShowError("انتخاب مشتری انجام نشد.", ex); }
        }

        private void CUST_NO2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO2.IsEditable && e.OriginalSource is not TextBox) return;
            if (CUST_NO2.Template.FindName("PART_EditableTextBox", CUST_NO2) is not TextBox editor) return;
            string customerCode = editor.Text.Trim();
            if (CUST_NO2.SelectedValue?.ToString() == customerCode) return;

            try
            {
                var customer = string.IsNullOrWhiteSpace(customerCode)
                    ? null
                    : dbms.DoGetDataSQL<Custom_CUST_HESAB>(
                        "SELECT TOP (1) hes, NAME = COALESCE(NAME, N'') FROM dbo.CUST_HESAB WHERE hes = @hes",
                        new { hes = customerCode }).FirstOrDefault();
                if (customer is null)
                {
                    CUST_NO.SelectedValue = null;
                    CUST_NO.Text = string.Empty;
                    CUST_NO2.SelectedValue = null;
                    CUST_NO2.Text = string.Empty;
                    return;
                }

                AddCustomerToLookup(customer);
                CUST_NO.SelectedValue = customer.hes;
                CUST_NO2.SelectedValue = customer.hes;
                CUST_NO.Items.Refresh();
                CUST_NO2.Items.Refresh();
            }
            catch (Exception ex) { ShowError("انتخاب کد مشتری انجام نشد.", ex); }
        }

        private void CUST_NO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading) return;
            string? selectedCode = (sender as ComboBox)?.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(selectedCode)) return;
            isLoading = true;
            try
            {
                CUST_NO.SelectedValue = selectedCode;
                CUST_NO2.SelectedValue = selectedCode;
            }
            finally { isLoading = false; }
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!AllowEdits) return;
            if (!TryValidateHeader(out long contractDate, out string customerCode))
                return;

            const string saveHeaderSql = @"
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
DECLARE @SavedContractID INT = @ContractID;

IF EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractNo = @ContractNo AND ContractID <> COALESCE(@ContractID, -1))
    THROW 51001, N'شماره قرارداد تکراری است.', 1;

IF @SavedContractID IS NULL
BEGIN
    INSERT dbo.CONTRACT_HED (ContractNo, ContractDate, CUST_NO, BrandName, TotalQty, MOLAH, IsClosed, UID)
    VALUES (@ContractNo, @ContractDate, @CUST_NO, @BrandName, 0, NULLIF(@MOLAH, N''), @IsClosed, @UID);
    SET @SavedContractID = CONVERT(INT, SCOPE_IDENTITY());
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractID = @SavedContractID)
        THROW 51002, N'قرارداد مورد نظر دیگر وجود ندارد.', 1;
    DECLARE @OldContractDate BIGINT, @OldCustomerCode NVARCHAR(40);
    SELECT @OldContractDate=ContractDate, @OldCustomerCode=CUST_NO
    FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK)
    WHERE ContractID=@SavedContractID;

    IF @ContractDate<>@OldContractDate AND
    (
        EXISTS
        (
            SELECT 1
            FROM dbo.INVO_LST AS I
            INNER JOIN dbo.HEAD_LST AS H ON H.NUMBER=I.NUMBER AND H.TAG=I.TAG
            WHERE I.ContractID=@SavedContractID AND H.DATE_N<@ContractDate
        )
        OR EXISTS
        (
            SELECT 1
            FROM dbo.ORDR_LST AS O
            INNER JOIN dbo.ORDR_HED AS OH ON OH.id=O.ID
            WHERE O.ContractID=@SavedContractID AND OH.DATE<@ContractDate
        )
    )
        THROW 51011, N'تاریخ قرارداد نمی‌تواند بعد از تاریخ اسناد متصل به آن قرار گیرد.', 1;

    IF @CUST_NO<>@OldCustomerCode AND
    (
        EXISTS
        (
            SELECT 1
            FROM dbo.INVO_LST AS I
            INNER JOIN dbo.CONTRACT_FLOW_TAG AS F ON F.TAG=I.TAG AND F.FlowType=2
            INNER JOIN dbo.HEAD_LST AS H ON H.NUMBER=I.NUMBER AND H.TAG=I.TAG
            WHERE I.ContractID=@SavedContractID AND H.CUST_NO<>@CUST_NO
        )
        OR EXISTS
        (
            SELECT 1
            FROM dbo.ORDR_LST AS O
            INNER JOIN dbo.ORDR_HED AS OH ON OH.id=O.ID
            WHERE O.ContractID=@SavedContractID AND OH.CUST_NO<>@CUST_NO
        )
    )
        THROW 51012, N'مشتری قرارداد با مشتری اسناد فروش یا سفارش‌های متصل یکسان نیست.', 1;

    UPDATE dbo.CONTRACT_HED
       SET ContractNo=@ContractNo, ContractDate=@ContractDate, CUST_NO=@CUST_NO, BrandName=@BrandName,
           MOLAH=NULLIF(@MOLAH, N''), IsClosed=@IsClosed, UID=@UID
     WHERE ContractID=@SavedContractID;
END;
COMMIT TRANSACTION;
SELECT @SavedContractID;";

            try
            {
                CurrentContractID = dbms.DoGetDataSQL<int>(saveHeaderSql, new
                {
                    ContractID = CurrentContractID,
                    ContractNo = ContractNo.Text.Trim(),
                    ContractDate = contractDate,
                    CUST_NO = customerCode,
                    BrandName = BrandName.Text.Trim(),
                    MOLAH = MOLAH.Text.Trim(),
                    IsClosed = IsClosed.IsChecked == true,
                    UID = Baseknow.USERCOD
                }).Single();
                navigationManager!.IsNewRecord = false;
                RefreshNavigation(CurrentContractID);
                LoadBrands(BrandName.Text.Trim());
                AllowEdits = true;
                LBL_STATUS.Text = DG_DTL.IsReadOnly
                    ? "سربرگ قرارداد ذخیره شد؛ قرارداد مختومه است و ردیف‌ها قابل ویرایش نیستند."
                    : "سربرگ قرارداد ذخیره شد؛ اکنون طرح‌ها را سطربه‌سطر ثبت کنید.";
                if (!DG_DTL.IsReadOnly && ContractDetails.Count == 0)
                    FocusNewDetailRow();
            }
            catch (Exception ex) { ShowError("ذخیره سربرگ قرارداد انجام نشد.", ex); }
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentContractID.HasValue) return;
            AllowEdits = true;
            LBL_STATUS.Text = IsClosed.IsChecked == true
                ? "اصلاح سربرگ قرارداد فعال شد؛ برای بازکردن ردیف‌ها ابتدا وضعیت مختومه را بردارید و سربرگ را ذخیره کنید."
                : "حالت اصلاح فعال است؛ تغییرات سربرگ را ذخیره کنید و ردیف‌ها را سطربه‌سطر ویرایش کنید.";
            if (!DG_DTL.IsReadOnly)
                FocusNewDetailRow();
            else
            {
                ContractNo.Focus();
                ContractNo.SelectAll();
            }
        }

        private void ApplyEditingState()
        {
            bool canEditHeader = AllowEdits;
            ContractNo.IsReadOnly = !canEditHeader;
            ContractDate.IsReadOnly = !canEditHeader;
            BrandName.IsEnabled = canEditHeader;
            CUST_NO.IsEnabled = canEditHeader;
            CUST_NO2.IsEnabled = canEditHeader;
            MOLAH.IsReadOnly = !canEditHeader;
            IsClosed.IsEnabled = canEditHeader && CurrentContractID.HasValue;

            BTN_SAVE.IsEnabled = canEditHeader;
            ESLAH.IsEnabled = CurrentContractID.HasValue;
            BTN_DELETE.IsEnabled = CurrentContractID.HasValue && AllowEdits;
            DG_DTL.IsReadOnly = !CurrentContractID.HasValue || !AllowEdits || IsClosed.IsChecked == true;
        }

        private bool TryValidateHeader(out long contractDate, out string customerCode)
        {
            contractDate = 0;
            customerCode = (CUST_NO.SelectedValue?.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(ContractNo.Text) || ContractNo.Text.Trim().Length > 50)
                return ValidationError("شماره قرارداد الزامی و حداکثر ۵۰ کاراکتر است.");
            if (!TryParsePersianDate(ContractDate.Text, out contractDate))
                return ValidationError("تاریخ قرارداد معتبر نیست. تاریخ را به صورت 1405/05/07 وارد کنید.");
            if (string.IsNullOrWhiteSpace(customerCode))
                return ValidationError("انتخاب مشتری الزامی است.");
            if (string.IsNullOrWhiteSpace(BrandName.Text) || BrandName.Text.Trim().Length > 100)
                return ValidationError("نام برند الزامی و حداکثر ۱۰۰ کاراکتر است.");
            return true;
        }

        private bool ValidationError(string message)
        {
            new Msgwin(false, message).ShowDialog();
            return false;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentContractID.HasValue) { ValidationError("ابتدا یک قرارداد ثبت‌شده را انتخاب کنید."); return; }
            var confirmation = new Msgwin(true, "قرارداد انتخاب‌شده حذف شود؟");
            confirmation.ShowDialog();
            if (confirmation.DialogResult != true) return;
            try
            {
                const string sql = @"
SET XACT_ABORT ON; BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM dbo.ORDR_HED WHERE ContractID=@ContractID)
    THROW 51004, N'این قرارداد در سفارش استفاده شده و قابل حذف نیست؛ آن را مختومه کنید.', 1;
IF EXISTS (SELECT 1 FROM dbo.ORDR_LST WHERE ContractID=@ContractID)
    THROW 51004, N'این قرارداد در ردیف‌های سفارش استفاده شده و قابل حذف نیست؛ آن را مختومه کنید.', 1;
IF EXISTS (SELECT 1 FROM dbo.HEAD_LST WHERE ContractID=@ContractID)
    THROW 51004, N'این قرارداد در اسناد انبار یا فروش استفاده شده و قابل حذف نیست؛ آن را مختومه کنید.', 1;
IF EXISTS
(
    SELECT 1
    FROM dbo.INVO_LST AS I
    WHERE I.ContractID=@ContractID
)
    THROW 51004, N'این قرارداد دارای گردش تولید یا فروش است و قابل حذف نیست؛ آن را مختومه کنید.', 1;
DELETE dbo.CONTRACT_DTL WHERE ContractID=@ContractID;
DELETE dbo.CONTRACT_HED WHERE ContractID=@ContractID;
IF @@ROWCOUNT = 0 THROW 51005, N'قرارداد پیدا نشد.', 1;
COMMIT TRANSACTION;";
                dbms.DoExecuteSQL(sql, new { ContractID = CurrentContractID.Value });
                LoadBrands();
                RefreshNavigation();
                LBL_STATUS.Text = "قرارداد حذف شد.";
            }
            catch (Exception ex) { ShowError("حذف قرارداد انجام نشد.", ex); }
        }

        private static bool TryParsePersianDate(string text, out long rawDate)
        {
            rawDate = 0;
            string digits = new(text.Where(char.IsDigit).Select(ToEnglishDigit).ToArray());
            if (digits.Length != 8 || !int.TryParse(digits[..4], out int year) || !int.TryParse(digits.Substring(4, 2), out int month) || !int.TryParse(digits.Substring(6, 2), out int day)) return false;
            try { _ = new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0); }
            catch (ArgumentOutOfRangeException) { return false; }
            return long.TryParse(digits, out rawDate);
        }

        private static char ToEnglishDigit(char value) => value switch
        {
            '۰' => '0', '۱' => '1', '۲' => '2', '۳' => '3', '۴' => '4',
            '۵' => '5', '۶' => '6', '۷' => '7', '۸' => '8', '۹' => '9',
            '٠' => '0', '١' => '1', '٢' => '2', '٣' => '3', '٤' => '4',
            '٥' => '5', '٦' => '6', '٧' => '7', '٨' => '8', '٩' => '9', _ => value
        };

        private static string FormatDate(string raw)
        {
            string digits = new(raw.Where(char.IsDigit).ToArray());
            return digits.Length == 8 ? $"{digits[..4]}/{digits.Substring(4, 2)}/{digits.Substring(6, 2)}" : raw;
        }

        private void CalculateTotal()
        {
            var totals = ContractDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.CODE))
                .GroupBy(x => string.IsNullOrWhiteSpace(x.UnitName) ? "واحد پایه" : x.UnitName)
                .Select(x => $"{x.Sum(y => y.Qty):N4} {x.Key}");
            LBL_TotalQty.Text = $"جمع مقدار تعهد: {string.Join(" + ", totals)}";
        }
        private void DG_DTL_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.SortMemberPath == nameof(ContractDtlModel.NAME_CODE) &&
                e.Row.Item is ContractDtlModel detail && e.EditingElement is TextBox editor)
            {
                string enteredValue = editor.Text.Trim();
                try
                {
                    ProductLookup? product = null;
                    if (enteredValue is "+" or "++")
                    {
                        var search = new SERCHK(this);
                        search.ShowDialog();
                        if (search.SELECTED_KALA is not null)
                            product = new ProductLookup { CODE = search.SELECTED_KALA.CODE ?? string.Empty, NAME = search.SELECTED_KALA.NAME_CODE ?? string.Empty };
                    }
                    else if (!string.IsNullOrWhiteSpace(enteredValue))
                    {
                        product = dbms.DoGetDataSQL<ProductLookup>(@"
SELECT TOP (1) CODE, NAME = COALESCE(NAME, N'')
FROM dbo.STUF_DEF
WHERE CODE = @Value OR NAME = @Value
ORDER BY CASE WHEN CODE = @Value THEN 0 ELSE 1 END, CODE",
                            new { Value = enteredValue }).FirstOrDefault();
                    }

                    if (product is null || string.IsNullOrWhiteSpace(product.CODE))
                    {
                        detail.CODE = string.Empty;
                        detail.NAME_CODE = string.Empty;
                        detail.UnitName = string.Empty;
                        editor.Text = string.Empty;
                        if (!string.IsNullOrWhiteSpace(enteredValue))
                            ValidationError("کالای واردشده پیدا نشد؛ برای جست‌وجوی کالا علامت + را وارد کنید.");
                    }
                    else
                    {
                        detail.CODE = product.CODE;
                        detail.NAME_CODE = product.NAME;
                        detail.UnitName = dbms.DoGetDataSQL<string>(@"
SELECT TOP (1) COALESCE(U.NAMES, N'واحد پایه')
FROM dbo.STUF_DEF AS S
LEFT JOIN dbo.TCOD_VAHEDS AS U ON U.CODE = S.VAHED
WHERE S.CODE = @Code", new { Code = product.CODE }).FirstOrDefault() ?? "واحد پایه";
                        editor.Text = product.NAME;
                    }
                }
                catch (Exception ex) { ShowError("انتخاب کالا انجام نشد.", ex); }
            }
            Dispatcher.BeginInvoke(new Action(CalculateTotal), DispatcherPriority.Background);
        }
        private void DG_DTL_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel || e.Row.Item is not ContractDtlModel detail) return;
            if (IsCompletelyEmptyDetail(detail))
            {
                e.Cancel = true;
                CancelEmptyDetailRow(detail);
                return;
            }
            if (!CurrentContractID.HasValue)
            {
                e.Cancel = true;
                ValidationError("ابتدا سربرگ قرارداد را ذخیره کنید.");
                return;
            }
            if (!ValidateContractDetailBeforeLeaving(detail))
            {
                e.Cancel = true;
                FocusInvalidContractDetail(detail);
                return;
            }
            try
            {
                SaveContractDetail(detail);
                CalculateTotal();
                LBL_STATUS.Text = $"ردیف کالای {detail.CODE} ذخیره شد.";
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                ShowError("ذخیره ردیف قرارداد انجام نشد.", ex);
                FocusInvalidContractDetail(detail);
            }
        }
        private void DG_DTL_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (CurrentContractID.HasValue && IsClosed.IsChecked != true) return;
            e.Cancel = true;
            ValidationError(CurrentContractID.HasValue
                ? "قرارداد مختومه است و ردیف‌های آن قابل ویرایش نیستند."
                : "ابتدا سربرگ قرارداد را ذخیره کنید.");
        }

        private void SaveContractDetail(ContractDtlModel detail)
        {
            const string sql = @"
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractID=@ContractID)
    THROW 51002, N'قرارداد مورد نظر دیگر وجود ندارد.', 1;
IF EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WHERE ContractID=@ContractID AND IsClosed=1)
    THROW 51007, N'قرارداد مختومه است و ردیف جدید قابل ثبت نیست.', 1;
IF EXISTS (SELECT 1 FROM dbo.CONTRACT_DTL WHERE ContractID=@ContractID AND CODE=@CODE AND ID<>@ID)
    THROW 51008, N'این کالا قبلاً در قرارداد ثبت شده است.', 1;
IF EXISTS
(
    SELECT 1
    FROM dbo.CONTRACT_DTL AS D
    INNER JOIN dbo.STUF_DEF AS ExistingProduct ON ExistingProduct.CODE=D.CODE
    INNER JOIN dbo.STUF_DEF AS NewProduct ON NewProduct.CODE=@CODE
    WHERE D.ContractID=@ContractID AND D.ID<>@ID
      AND ISNULL(ExistingProduct.VAHED, 0)<>ISNULL(NewProduct.VAHED, 0)
)
    THROW 51010, N'واحد پایه همه کالاهای یک قرارداد باید یکسان باشد.', 1;

IF @ID = 0
BEGIN
    INSERT dbo.CONTRACT_DTL (ContractID, CODE, Qty, UID) VALUES (@ContractID, @CODE, @Qty, @UID);
    SET @ID = CONVERT(BIGINT, SCOPE_IDENTITY());
END
ELSE
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.CONTRACT_DTL AS D
        WHERE D.ID=@ID AND D.ContractID=@ContractID AND D.CODE<>@CODE
          AND
          (
              EXISTS (SELECT 1 FROM dbo.INVO_LST AS I WHERE I.ContractID=@ContractID AND I.CODE=D.CODE)
              OR EXISTS (SELECT 1 FROM dbo.ORDR_LST AS O WHERE O.ContractID=@ContractID AND O.CODE=D.CODE)
          )
    )
        THROW 51006, N'کالای ردیف دارای گردش را نمی‌توان تغییر داد.', 1;
    UPDATE dbo.CONTRACT_DTL SET CODE=@CODE, Qty=@Qty, UID=@UID WHERE ID=@ID AND ContractID=@ContractID;
    IF @@ROWCOUNT = 0 THROW 51009, N'ردیف قرارداد دیگر وجود ندارد.', 1;
END;
UPDATE dbo.CONTRACT_HED
   SET TotalQty=COALESCE((SELECT SUM(Qty) FROM dbo.CONTRACT_DTL WHERE ContractID=@ContractID), 0), UID=@UID
 WHERE ContractID=@ContractID;
COMMIT TRANSACTION;
SELECT @ID;";
            detail.ID = dbms.DoGetDataSQL<long>(sql, new
            {
                ContractID = CurrentContractID!.Value,
                detail.ID,
                detail.CODE,
                detail.Qty,
                UID = Baseknow.USERCOD
            }).Single();
            RefreshContractTotalAndList();
        }

        private void RefreshContractTotalAndList()
        {
            if (!CurrentContractID.HasValue) return;
            decimal total = dbms.DoGetDataSQL<decimal>(
                "SELECT TotalQty FROM dbo.CONTRACT_HED WHERE ContractID=@ContractID",
                new { ContractID = CurrentContractID.Value }).Single();
            ContractHeaderModel? header = navigationManager?.RecordsData.FirstOrDefault(x => x.ContractID == CurrentContractID.Value);
            if (header is not null) header.TotalQty = total;
            CalculateTotal();
        }

        private void DG_DTL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete || DG_DTL.IsReadOnly || DG_DTL.SelectedItem is not ContractDtlModel detail) return;
            e.Handled = true;
            if (DG_DTL.CommitEdit(DataGridEditingUnit.Cell, true) &&
                DG_DTL.CommitEdit(DataGridEditingUnit.Row, true) &&
                ContractDetails.Contains(detail))
                DeleteContractDetail(detail);
        }

        private void DeleteContractDetail(ContractDtlModel detail)
        {
            if (detail.ID == 0)
            {
                ContractDetails.Remove(detail);
                CalculateTotal();
                return;
            }
            var confirmation = new Msgwin(true, $"ردیف کالای {detail.CODE} حذف شود؟");
            confirmation.ShowDialog();
            if (confirmation.DialogResult != true) return;
            try
            {
                const string sql = @"
SET XACT_ABORT ON; BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE ContractID=@ContractID AND CODE=@CODE)
    THROW 51010, N'ردیف دارای گردش تولید یا فروش قابل حذف نیست.', 1;
IF EXISTS (SELECT 1 FROM dbo.ORDR_LST WHERE ContractID=@ContractID AND CODE=@CODE)
    THROW 51010, N'ردیف استفاده‌شده در سفارش قابل حذف نیست.', 1;
DELETE dbo.CONTRACT_DTL WHERE ID=@ID AND ContractID=@ContractID;
IF @@ROWCOUNT = 0 THROW 51009, N'ردیف قرارداد دیگر وجود ندارد.', 1;
UPDATE dbo.CONTRACT_HED
   SET TotalQty=COALESCE((SELECT SUM(Qty) FROM dbo.CONTRACT_DTL WHERE ContractID=@ContractID), 0), UID=@UID
 WHERE ContractID=@ContractID;
COMMIT TRANSACTION;";
                dbms.DoExecuteSQL(sql, new
                {
                    ContractID = CurrentContractID!.Value,
                    detail.ID,
                    detail.CODE,
                    UID = Baseknow.USERCOD
                });
                ContractDetails.Remove(detail);
                RefreshContractTotalAndList();
                LBL_STATUS.Text = "ردیف قرارداد حذف شد.";
            }
            catch (Exception ex) { ShowError("حذف ردیف قرارداد انجام نشد.", ex); }
        }
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Btn_Max_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (e.ClickCount == 2) Btn_Max_Click(sender, e);
            else DragMove();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                var searchWindow = new EnhancedSearchWindow(this) { Owner = this };
                searchWindow.ShowDialog();
                return;
            }
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                if (DG_DTL.IsKeyboardFocusWithin)
                {
                    DG_DTL.CancelEdit(DataGridEditingUnit.Cell);
                    DG_DTL.CancelEdit(DataGridEditingUnit.Row);
                }
                return;
            }
            if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;

            e.Handled = true;
            if (DG_DTL.IsKeyboardFocusWithin)
            {
                MoveContractGridWithEnter();
                return;
            }
            if (BTN_SAVE.IsKeyboardFocusWithin)
            {
                BTN_SAVE_Click(BTN_SAVE, new RoutedEventArgs(Button.ClickEvent));
                return;
            }
            CL_LMethods.SendKey_US(Key.Tab);
        }

        private void MoveContractGridWithEnter()
        {
            var editableColumns = DG_DTL.Columns
                .Where(x => x.Visibility == Visibility.Visible && !x.IsReadOnly)
                .OrderBy(x => x.DisplayIndex)
                .ToList();
            if (editableColumns.Count == 0 || DG_DTL.CurrentCell.Item is null) return;

            int currentDisplayIndex = DG_DTL.CurrentColumn?.DisplayIndex ?? -1;
            DataGridColumn? nextColumn = editableColumns.FirstOrDefault(x => x.DisplayIndex > currentDisplayIndex);
            if (nextColumn is not null)
            {
                if (!DG_DTL.CommitEdit(DataGridEditingUnit.Cell, true)) return;
                MoveToContractCell(DG_DTL.CurrentCell.Item, nextColumn);
                return;
            }

            if (!DG_DTL.CommitEdit(DataGridEditingUnit.Cell, true)) return;
            if (DG_DTL.CurrentCell.Item is ContractDtlModel currentDetail)
            {
                if (IsCompletelyEmptyDetail(currentDetail))
                {
                    CancelEmptyDetailRow(currentDetail);
                    return;
                }
                if (!ValidateContractDetailBeforeLeaving(currentDetail))
                {
                    FocusInvalidContractDetail(currentDetail);
                    return;
                }
            }
            int currentRowIndex = DG_DTL.Items.IndexOf(DG_DTL.CurrentCell.Item);
            int nextRowIndex = currentRowIndex + 1;
            if (nextRowIndex < 0 || nextRowIndex >= DG_DTL.Items.Count) return;

            DG_DTL.SelectedIndex = nextRowIndex;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                object nextItem = DG_DTL.SelectedItem;
                if (nextItem is null) return;
                DG_DTL.ScrollIntoView(nextItem);
                DG_DTL.CurrentCell = new DataGridCellInfo(nextItem, editableColumns[0]);
                DG_DTL.BeginEdit();
            }), DispatcherPriority.Background);
        }

        private bool ValidateContractDetailBeforeLeaving(ContractDtlModel detail)
        {
            if (string.IsNullOrWhiteSpace(detail.CODE) || string.IsNullOrWhiteSpace(detail.NAME_CODE))
                return ValidationError("انتخاب کالای معتبر برای ردیف جاری الزامی است؛ برای جست‌وجو علامت + را وارد کنید.");
            if (detail.Qty <= 0 || detail.Qty > 999999999999999m)
                return ValidationError("مقدار تعهد ردیف جاری باید عددی بزرگ‌تر از صفر باشد.");
            return true;
        }

        private static bool IsCompletelyEmptyDetail(ContractDtlModel detail) =>
            detail.ID == 0 &&
            string.IsNullOrWhiteSpace(detail.CODE) &&
            string.IsNullOrWhiteSpace(detail.NAME_CODE) &&
            detail.Qty == 0;

        private void CancelEmptyDetailRow(ContractDtlModel detail)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DG_DTL.CancelEdit(DataGridEditingUnit.Cell);
                DG_DTL.CancelEdit(DataGridEditingUnit.Row);
                if (detail.ID == 0)
                    ContractDetails.Remove(detail);
                CalculateTotal();
            }), DispatcherPriority.Background);
        }

        private void FocusInvalidContractDetail(ContractDtlModel detail)
        {
            string targetProperty = string.IsNullOrWhiteSpace(detail.CODE) || string.IsNullOrWhiteSpace(detail.NAME_CODE)
                ? nameof(ContractDtlModel.NAME_CODE)
                : nameof(ContractDtlModel.Qty);
            DataGridColumn? targetColumn = DG_DTL.Columns.FirstOrDefault(x => x.SortMemberPath == targetProperty);
            if (targetColumn is null) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!DG_DTL.Items.Contains(detail)) return;
                DG_DTL.SelectedItem = detail;
                DG_DTL.ScrollIntoView(detail, targetColumn);
                DG_DTL.CurrentCell = new DataGridCellInfo(detail, targetColumn);
                DG_DTL.BeginEdit();
            }), DispatcherPriority.Background);
        }

        private void MoveToContractCell(object item, DataGridColumn column)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DG_DTL.SelectedItem = item;
                DG_DTL.ScrollIntoView(item, column);
                DG_DTL.CurrentCell = new DataGridCellInfo(item, column);
                DG_DTL.BeginEdit();
            }), DispatcherPriority.Background);
        }
        private void FocusNewDetailRow()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DataGridColumn? firstEditable = DG_DTL.Columns
                    .Where(x => x.Visibility == Visibility.Visible && !x.IsReadOnly)
                    .OrderBy(x => x.DisplayIndex)
                    .FirstOrDefault();
                if (firstEditable is null || DG_DTL.Items.Count == 0) return;

                ContractDtlModel? emptyDetail = ContractDetails.LastOrDefault(IsCompletelyEmptyDetail);
                object targetItem = emptyDetail ?? DG_DTL.Items[DG_DTL.Items.Count - 1];
                DG_DTL.SelectedItem = targetItem;
                DG_DTL.ScrollIntoView(targetItem, firstEditable);
                DG_DTL.CurrentCell = new DataGridCellInfo(targetItem, firstEditable);
                DG_DTL.Focus();
                DG_DTL.BeginEdit();
            }), DispatcherPriority.Background);
        }
        private void ShowError(string message, Exception ex)
        {
            LBL_STATUS.Text = message;
            new Msgwin(false, $"{message}\n{ex.Message}").ShowDialog();
        }

        public sealed class ContractDtlModel : INotifyPropertyChanged
        {
            private long id;
            private string code = string.Empty;
            private string nameCode = string.Empty;
            private decimal qty;
            private string unitName = string.Empty;

            public long ID { get => id; set => SetField(ref id, value); }
            public string CODE { get => code; set => SetField(ref code, value ?? string.Empty); }
            public string NAME_CODE { get => nameCode; set => SetField(ref nameCode, value ?? string.Empty); }
            public decimal Qty { get => qty; set => SetField(ref qty, value); }
            public string UnitName { get => unitName; set => SetField(ref unitName, value ?? string.Empty); }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return;
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private sealed class ContractHeaderModel { public int ContractID { get; set; } public string ContractNo { get; set; } = string.Empty; public long ContractDate { get; set; } public string CUST_NO { get; set; } = string.Empty; public string BrandName { get; set; } = string.Empty; public decimal TotalQty { get; set; } public string? MOLAH { get; set; } public bool IsClosed { get; set; } }
        private sealed class ProductLookup { public string CODE { get; set; } = string.Empty; public string NAME { get; set; } = string.Empty; }
        private sealed class BrandLookup { public string NAMES { get; set; } = string.Empty; }
    }
}
