using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

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
                CUST_NO.ItemsSource = dbms.DoGetDataSQL<CustomerLookup>(
                    "SELECT hes, DisplayName = CONCAT(hes, N' - ', COALESCE(NAME, N'')) FROM dbo.CUST_HESAB ORDER BY NAME, hes").ToList();
                Col_Kala.ItemsSource = dbms.DoGetDataSQL<ProductLookup>(
                    "SELECT CODE, DisplayName = CONCAT(CODE, N' - ', COALESCE(NAME, N'')) FROM dbo.STUF_DEF ORDER BY NAME, CODE").ToList();
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
                CUST_NO.SelectedValue = header.CUST_NO;
                IsClosed.IsChecked = header.IsClosed;
                ContractDetails.Clear();
                foreach (var detail in dbms.DoGetDataSQL<ContractDtlModel>(
                    "SELECT ID, CODE, Qty FROM dbo.CONTRACT_DTL WHERE ContractID = @ContractID ORDER BY ID",
                    new { header.ContractID }))
                    ContractDetails.Add(detail);
                CalculateTotal();
                LBL_STATUS.Text = $"ویرایش قرارداد {header.ContractNo}";
            }
            finally { isLoading = false; }
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
        private void DG_DTL_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => Dispatcher.BeginInvoke(new Action(CalculateTotal), DispatcherPriority.Background);
        private void DG_DTL_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) => Dispatcher.BeginInvoke(new Action(CalculateTotal), DispatcherPriority.Background);
        private void DG_CONTRACTS_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (!isLoading && DG_CONTRACTS.SelectedItem is ContractHeaderModel h) LoadContract(h); }
        private void BTN_NEW_Click(object sender, RoutedEventArgs e) => BeginNewContract();
        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e) { LoadContracts(CurrentContractID); LBL_STATUS.Text = "اطلاعات به‌روز شد."; }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }
        private void ShowError(string message, Exception ex) { LBL_STATUS.Text = message; MessageBox.Show($"{message}\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error); }

        public sealed class ContractDtlModel { public long ID { get; set; } public string CODE { get; set; } = string.Empty; public decimal Qty { get; set; } }
        private sealed class ContractHeaderModel { public int ContractID { get; set; } public string ContractNo { get; set; } = string.Empty; public long ContractDate { get; set; } public string CUST_NO { get; set; } = string.Empty; public string BrandName { get; set; } = string.Empty; public decimal TotalQty { get; set; } public string? MOLAH { get; set; } public bool IsClosed { get; set; } }
        private sealed class CustomerLookup { public string hes { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }
        private sealed class ProductLookup { public string CODE { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }
    }
}
