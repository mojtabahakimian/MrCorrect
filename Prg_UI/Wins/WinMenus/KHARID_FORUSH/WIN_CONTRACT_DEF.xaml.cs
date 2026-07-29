using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_CONTRACT_DEF : Window
    {
        private readonly CL_CCNNMANAGER dbms = new();
        private readonly ObservableCollection<ContractDtlModel> ContractDetails = new();
        private int? CurrentContractID;
        private bool isLoading;

        public WIN_CONTRACT_DEF()
        {
            InitializeComponent();
            DG_DTL.ItemsSource = ContractDetails;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ContractDate.Text = FormatDate(Tarikh.FullCurrentDate);
                CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                LoadContracts();
                BeginNewContract();
            }
            catch (Exception ex)
            {
                ShowError("بارگذاری اطلاعات قراردادها انجام نشد.", ex);
            }
        }

        private void LoadContracts(int? selectContractID = null)
        {
            var contracts = dbms.DoGetDataSQL<ContractHeaderModel>(@"
SELECT ContractID, ContractNo, ContractDate, CUST_NO, BrandName, TotalQty, MOLAH, IsClosed
FROM dbo.CONTRACT_HED
ORDER BY ContractDate DESC, ContractID DESC").ToList();
            DG_CONTRACTS.ItemsSource = contracts;
            if (selectContractID.HasValue)
                DG_CONTRACTS.SelectedItem = contracts.FirstOrDefault(x => x.ContractID == selectContractID.Value);
        }

        private void BeginNewContract()
        {
            isLoading = true;
            try
            {
                CurrentContractID = null;
                DG_CONTRACTS.SelectedItem = null;
                ContractNo.Clear();
                ContractDate.Text = FormatDate(Tarikh.FullCurrentDate);
                BrandName.Clear();
                MOLAH.Clear();
                CUST_NO.SelectedIndex = -1;
                CUST_NO.Text = string.Empty;
                IsClosed.IsChecked = false;
                ContractDetails.Clear();
                CalculateTotal();
                LBL_STATUS.Text = "قرارداد جدید";
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
                BrandName.Text = header.BrandName;
                MOLAH.Text = header.MOLAH ?? string.Empty;
                SelectCustomer(header.CUST_NO);
                IsClosed.IsChecked = header.IsClosed;
                ContractDetails.Clear();
                foreach (var detail in dbms.DoGetDataSQL<ContractDtlModel>(
                    @"SELECT D.ID, D.CODE, D.Qty, NAME_CODE = COALESCE(S.NAME, N'')
FROM dbo.CONTRACT_DTL AS D
LEFT JOIN dbo.STUF_DEF AS S ON S.CODE = D.CODE
WHERE D.ContractID = @ContractID
ORDER BY D.ID",
                    new { header.ContractID }))
                    ContractDetails.Add(detail);
                CalculateTotal();
                LBL_STATUS.Text = $"ویرایش قرارداد {header.ContractNo}";
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
            CUST_NO.Items.Refresh();
        }

        private void AddCustomerToLookup(Custom_CUST_HESAB customer)
        {
            if (CUST_NO.ItemsSource is not List<Custom_CUST_HESAB> customers)
            {
                customers = new List<Custom_CUST_HESAB>();
                CUST_NO.ItemsSource = customers;
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
                    return;
                }

                AddCustomerToLookup(customer);
                CUST_NO.SelectedValue = customer.hes;
                CUST_NO.Items.Refresh();
            }
            catch (Exception ex) { ShowError("انتخاب مشتری انجام نشد.", ex); }
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            DG_DTL.CommitEdit(DataGridEditingUnit.Cell, true);
            DG_DTL.CommitEdit(DataGridEditingUnit.Row, true);

            if (!TryValidate(out long contractDate, out string customerCode, out List<ContractDtlModel> details))
                return;

            var detailJson = JsonSerializer.Serialize(details.Select(x => new { x.CODE, x.Qty }));
            const string saveSql = @"
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
DECLARE @SavedContractID INT = @ContractID;

IF EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractNo = @ContractNo AND ContractID <> COALESCE(@ContractID, -1))
    THROW 51001, N'شماره قرارداد تکراری است.', 1;

IF @SavedContractID IS NULL
BEGIN
    INSERT dbo.CONTRACT_HED (ContractNo, ContractDate, CUST_NO, BrandName, TotalQty, MOLAH, IsClosed, UID)
    VALUES (@ContractNo, @ContractDate, @CUST_NO, @BrandName, @TotalQty, NULLIF(@MOLAH, N''), @IsClosed, @UID);
    SET @SavedContractID = CONVERT(INT, SCOPE_IDENTITY());
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractID = @SavedContractID)
        THROW 51002, N'قرارداد مورد نظر دیگر وجود ندارد.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM dbo.INVO_LST AS I
        WHERE I.ContractID = @SavedContractID
          AND NOT EXISTS
          (
              SELECT 1 FROM OPENJSON(@DetailsJson)
              WITH (CODE NVARCHAR(15) '$.CODE') AS J
              WHERE J.CODE = I.CODE
          )
    )
        THROW 51006, N'طرح دارای گردش تولید یا فروش را نمی‌توان از قرارداد حذف کرد.', 1;
    IF EXISTS
    (
        SELECT 1
        FROM dbo.ORDR_LST AS O
        WHERE O.ContractID = @SavedContractID
          AND NOT EXISTS
          (
              SELECT 1 FROM OPENJSON(@DetailsJson)
              WITH (CODE NVARCHAR(15) '$.CODE') AS J
              WHERE J.CODE = O.CODE
          )
    )
        THROW 51006, N'طرح استفاده‌شده در سفارش را نمی‌توان از قرارداد حذف کرد.', 1;
    UPDATE dbo.CONTRACT_HED
       SET ContractNo=@ContractNo, ContractDate=@ContractDate, CUST_NO=@CUST_NO, BrandName=@BrandName,
           TotalQty=@TotalQty, MOLAH=NULLIF(@MOLAH, N''), IsClosed=@IsClosed, UID=@UID
     WHERE ContractID=@SavedContractID;
END;

UPDATE D
   SET D.Qty = J.Qty, D.UID = @UID
FROM dbo.CONTRACT_DTL AS D
INNER JOIN OPENJSON(@DetailsJson)
    WITH (CODE NVARCHAR(15) '$.CODE', Qty DECIMAL(19,4) '$.Qty') AS J ON J.CODE = D.CODE
WHERE D.ContractID = @SavedContractID;

INSERT dbo.CONTRACT_DTL (ContractID, CODE, Qty, UID)
SELECT @SavedContractID, J.CODE, J.Qty, @UID
FROM OPENJSON(@DetailsJson) WITH (CODE NVARCHAR(15) '$.CODE', Qty DECIMAL(19,4) '$.Qty') AS J
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.CONTRACT_DTL AS D
    WHERE D.ContractID = @SavedContractID AND D.CODE = J.CODE
);

DELETE D
FROM dbo.CONTRACT_DTL AS D
WHERE D.ContractID = @SavedContractID
  AND NOT EXISTS
  (
      SELECT 1 FROM OPENJSON(@DetailsJson)
      WITH (CODE NVARCHAR(15) '$.CODE') AS J
      WHERE J.CODE = D.CODE
  );

IF (SELECT SUM(Qty) FROM dbo.CONTRACT_DTL WHERE ContractID=@SavedContractID) <> @TotalQty
    THROW 51003, N'جمع ردیف‌های قرارداد با جمع هدر برابر نیست.', 1;
COMMIT TRANSACTION;
SELECT @SavedContractID;";

            try
            {
                CurrentContractID = dbms.DoGetDataSQL<int>(saveSql, new
                {
                    ContractID = CurrentContractID,
                    ContractNo = ContractNo.Text.Trim(),
                    ContractDate = contractDate,
                    CUST_NO = customerCode,
                    BrandName = BrandName.Text.Trim(),
                    TotalQty = details.Sum(x => x.Qty),
                    MOLAH = MOLAH.Text.Trim(),
                    IsClosed = IsClosed.IsChecked == true,
                    UID = Baseknow.USERCOD,
                    DetailsJson = detailJson
                }).Single();
                LoadContracts(CurrentContractID);
                LBL_STATUS.Text = "قرارداد با موفقیت و به‌صورت یک تراکنش کامل ذخیره شد.";
                MessageBox.Show(LBL_STATUS.Text, "ثبت قرارداد", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { ShowError("ذخیره قرارداد انجام نشد.", ex); }
        }

        private bool TryValidate(out long contractDate, out string customerCode, out List<ContractDtlModel> details)
        {
            contractDate = 0;
            customerCode = (CUST_NO.SelectedValue?.ToString() ?? string.Empty).Trim();
            details = ContractDetails.Where(x => !string.IsNullOrWhiteSpace(x.CODE) || x.Qty != 0).ToList();
            if (string.IsNullOrWhiteSpace(ContractNo.Text) || ContractNo.Text.Trim().Length > 50)
                return ValidationError("شماره قرارداد الزامی و حداکثر ۵۰ کاراکتر است.");
            if (!TryParsePersianDate(ContractDate.Text, out contractDate))
                return ValidationError("تاریخ قرارداد معتبر نیست. تاریخ را به صورت 1405/05/07 وارد کنید.");
            if (string.IsNullOrWhiteSpace(customerCode))
                return ValidationError("انتخاب مشتری الزامی است.");
            if (string.IsNullOrWhiteSpace(BrandName.Text) || BrandName.Text.Trim().Length > 100)
                return ValidationError("نام برند الزامی و حداکثر ۱۰۰ کاراکتر است.");
            if (details.Count == 0)
                return ValidationError("حداقل یک طرح برای قرارداد وارد کنید.");
            if (details.Any(x => string.IsNullOrWhiteSpace(x.CODE) || x.Qty <= 0 || x.Qty > 999999999999999m))
                return ValidationError("طرح و متراژ مثبت تمام ردیف‌ها الزامی است.");
            if (details.GroupBy(x => x.CODE, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
                return ValidationError("یک طرح در قرارداد نمی‌تواند بیش از یک بار تکرار شود.");
            return true;
        }

        private bool ValidationError(string message)
        {
            MessageBox.Show(message, "کنترل قرارداد", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentContractID.HasValue) { ValidationError("ابتدا یک قرارداد ثبت‌شده را انتخاب کنید."); return; }
            if (MessageBox.Show("قرارداد انتخاب‌شده حذف شود؟", "تأیید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
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
                LoadContracts();
                BeginNewContract();
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

        private void CalculateTotal() => LBL_TotalQty.Text = $"جمع متراژ: {ContractDetails.Sum(x => x.Qty):N4} متر مربع";
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
                        editor.Text = string.Empty;
                        if (!string.IsNullOrWhiteSpace(enteredValue))
                            ValidationError("کالای واردشده پیدا نشد؛ برای جست‌وجوی کالا علامت + را وارد کنید.");
                    }
                    else
                    {
                        detail.CODE = product.CODE;
                        detail.NAME_CODE = product.NAME;
                        editor.Text = product.NAME;
                    }
                }
                catch (Exception ex) { ShowError("انتخاب کالا انجام نشد.", ex); }
            }
            Dispatcher.BeginInvoke(new Action(CalculateTotal), DispatcherPriority.Background);
        }
        private void DG_DTL_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) => Dispatcher.BeginInvoke(new Action(CalculateTotal), DispatcherPriority.Background);
        private void DG_CONTRACTS_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!isLoading && DG_CONTRACTS.SelectedItem is ContractHeaderModel h) LoadContract(h); }
        private void BTN_NEW_Click(object sender, RoutedEventArgs e) => BeginNewContract();
        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e) { LoadContracts(CurrentContractID); LBL_STATUS.Text = "اطلاعات به‌روز شد."; }
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Btn_Max_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (e.ClickCount == 2) Btn_Max_Click(sender, e);
            else DragMove();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
        private void ShowError(string message, Exception ex) { LBL_STATUS.Text = message; MessageBox.Show($"{message}\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error); }

        public sealed class ContractDtlModel : INotifyPropertyChanged
        {
            private long id;
            private string code = string.Empty;
            private string nameCode = string.Empty;
            private decimal qty;

            public long ID { get => id; set => SetField(ref id, value); }
            public string CODE { get => code; set => SetField(ref code, value ?? string.Empty); }
            public string NAME_CODE { get => nameCode; set => SetField(ref nameCode, value ?? string.Empty); }
            public decimal Qty { get => qty; set => SetField(ref qty, value); }

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
    }
}
