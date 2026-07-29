using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_RPT_CONTRACT_STATUS : Window
    {
        private readonly CL_CCNNMANAGER dbms = new();

        public WIN_RPT_CONTRACT_STATUS() => InitializeComponent();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var contracts = dbms.DoGetDataSQL<ContractLookup>(@"
SELECT ContractID, DisplayName = CONCAT(ContractNo, N' - ', BrandName)
FROM dbo.CONTRACT_HED ORDER BY ContractDate DESC, ContractID DESC").ToList();
                contracts.Insert(0, new ContractLookup { ContractID = 0, DisplayName = "همه قراردادها" });
                CMB_CONTRACT.ItemsSource = contracts;
                CMB_CONTRACT.SelectedIndex = 0;
                CMB_FLOW_TAG.ItemsSource = dbms.DoGetDataSQL<FlowTagLookup>(@"
SELECT TAG, DisplayName = CONCAT(CONVERT(NVARCHAR(20), TAG), N' - ', Description)
FROM dbo.CONTRACT_FLOW_TAG ORDER BY FlowType, TAG").ToList();
                CMB_FLOW_TAG.SelectedIndex = 0;
                LoadReport();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void LoadReport()
        {
            long? fromDate = null;
            string digits = new(TXT_FROM_DATE.Text.Where(char.IsDigit).Select(ToEnglishDigit).ToArray());
            if (digits.Length > 0)
            {
                if (!TryParsePersianDate(digits, out long parsed))
                {
                    MessageBox.Show("تاریخ شروع معتبر نیست و باید به صورت 1405/05/07 وارد شود.", "کنترل گزارش", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                fromDate = parsed;
            }

            int? contractID = CMB_CONTRACT.SelectedValue is int selected && selected > 0 ? selected : null;
            var data = dbms.DoGetDataSQL<ContractStatusModel>(@"
SELECT ContractID, ContractNo, ContractDate, CUST_NO, CustName, BrandName, IsClosed, CODE, ProductName,
       ContractedQty, ProducedQty, RemainToProduce, OverProducedQty, SoldQty, RemainInWarehouse
FROM dbo.VW_CONTRACT_STATUS
WHERE (@ContractID IS NULL OR ContractID=@ContractID)
  AND (@FromDate IS NULL OR ContractDate>=@FromDate)
  AND (@OpenOnly=0 OR IsClosed=0)
ORDER BY ContractDate DESC, ContractID DESC, ProductName, CODE", new
            {
                ContractID = contractID,
                FromDate = fromDate,
                OpenOnly = CHK_OPEN_ONLY.IsChecked == true
            }).ToList();

            DG_REPORT.ItemsSource = data;
            TXT_CONTRACTED.Text = $"قرارداد: {data.Sum(x => x.ContractedQty):N4}";
            TXT_PRODUCED.Text = $"تولید: {data.Sum(x => x.ProducedQty):N4}";
            TXT_REMAIN.Text = $"مانده تولید: {data.Sum(x => x.RemainToProduce):N4}";
            TXT_SOLD.Text = $"فروش خالص: {data.Sum(x => x.SoldQty):N4}";
            TXT_STOCK.Text = $"مانده تولید قرارداد: {data.Sum(x => x.RemainInWarehouse):N4}";
        }

        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e)
        {
            try { LoadReport(); }
            catch (Exception ex) { ShowError(ex); }
        }

        private void BTN_ASSIGN_DOCUMENT_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetAllocationKeys(out int contractID, out double documentNumber, out double tag)) return;
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.CONTRACT_HED WITH (UPDLOCK, HOLDLOCK) WHERE ContractID=@ContractID AND IsClosed=0)
    THROW 51010, N'قرارداد انتخاب‌شده وجود ندارد یا مختومه است.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.HEAD_LST WHERE NUMBER=@Number AND TAG=@Tag)
    THROW 51011, N'سند مورد نظر پیدا نشد.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.INVO_LST WHERE NUMBER=@Number AND TAG=@Tag)
    THROW 51012, N'سند مورد نظر فاقد ردیف کالا است.', 1;
IF EXISTS
(
    SELECT 1 FROM dbo.INVO_LST AS I
    WHERE I.NUMBER=@Number AND I.TAG=@Tag
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.CONTRACT_DTL AS D
          WHERE D.ContractID=@ContractID AND D.CODE=I.CODE
      )
)
    THROW 51013, N'حداقل یکی از کالاهای سند در ریز قرارداد تعریف نشده است.', 1;
IF EXISTS
(
    SELECT 1 FROM dbo.INVO_LST
    WHERE NUMBER=@Number AND TAG=@Tag
      AND ContractID IS NOT NULL AND ContractID<>@ContractID
)
    THROW 51014, N'این سند قبلاً به قرارداد دیگری تخصیص یافته است.', 1;
UPDATE dbo.INVO_LST SET ContractID=@ContractID
WHERE NUMBER=@Number AND TAG=@Tag;
COMMIT TRANSACTION;";
            try
            {
                dbms.DoExecuteSQL(sql, new { ContractID = contractID, Number = documentNumber, Tag = tag });
                MessageBox.Show("تمام ردیف‌های سند به قرارداد متصل شدند.", "تخصیص سند", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadReport();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void BTN_UNASSIGN_DOCUMENT_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetAllocationKeys(out int contractID, out double documentNumber, out double tag)) return;
            if (MessageBox.Show("اتصال این سند از قرارداد قطع شود؟", "تأیید", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                int affected = dbms.DoExecuteSQL(@"
UPDATE dbo.INVO_LST SET ContractID=NULL
WHERE NUMBER=@Number AND TAG=@Tag AND ContractID=@ContractID", new
                {
                    ContractID = contractID,
                    Number = documentNumber,
                    Tag = tag
                }) ?? 0;
                if (affected == 0)
                {
                    MessageBox.Show("هیچ ردیف متصلی برای سند و قرارداد انتخاب‌شده پیدا نشد.", "قطع اتصال", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show("اتصال سند از قرارداد قطع شد.", "قطع اتصال", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadReport();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private bool TryGetAllocationKeys(out int contractID, out double documentNumber, out double tag)
        {
            contractID = CMB_CONTRACT.SelectedValue is int selected ? selected : 0;
            tag = CMB_FLOW_TAG.SelectedValue is double selectedTag ? selectedTag : 0;
            string numberText = new(TXT_DOCUMENT_NUMBER.Text.Where(char.IsDigit).Select(ToEnglishDigit).ToArray());
            if (contractID <= 0)
            {
                MessageBox.Show("ابتدا یک قرارداد مشخص را انتخاب کنید.", "کنترل تخصیص", MessageBoxButton.OK, MessageBoxImage.Warning);
                documentNumber = 0;
                return false;
            }
            if (!double.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out documentNumber) || documentNumber <= 0)
            {
                MessageBox.Show("شماره سند معتبر وارد کنید.", "کنترل تخصیص", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (tag <= 0)
            {
                MessageBox.Show("نوع گردش سند را انتخاب کنید.", "کنترل تخصیص", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private static bool TryParsePersianDate(string digits, out long rawDate)
        {
            rawDate = 0;
            if (digits.Length != 8 ||
                !int.TryParse(digits[..4], out int year) ||
                !int.TryParse(digits.Substring(4, 2), out int month) ||
                !int.TryParse(digits.Substring(6, 2), out int day))
                return false;

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

        private static void ShowError(Exception ex) => MessageBox.Show($"گزارش وضعیت قراردادها بارگذاری نشد.\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }

        private sealed class FlowTagLookup { public double TAG { get; set; } public string DisplayName { get; set; } = string.Empty; }
        private sealed class ContractLookup { public int ContractID { get; set; } public string DisplayName { get; set; } = string.Empty; }
        private sealed class ContractStatusModel
        {
            public int ContractID { get; set; }
            public string ContractNo { get; set; } = string.Empty;
            public long ContractDate { get; set; }
            public string CUST_NO { get; set; } = string.Empty;
            public string CustName { get; set; } = string.Empty;
            public string BrandName { get; set; } = string.Empty;
            public bool IsClosed { get; set; }
            public string CODE { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public decimal ContractedQty { get; set; }
            public decimal ProducedQty { get; set; }
            public decimal RemainToProduce { get; set; }
            public decimal OverProducedQty { get; set; }
            public decimal SoldQty { get; set; }
            public decimal RemainInWarehouse { get; set; }
        }
    }
}
