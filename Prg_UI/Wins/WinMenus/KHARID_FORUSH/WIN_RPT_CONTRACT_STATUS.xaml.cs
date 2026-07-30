using Functions;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_RPT_CONTRACT_STATUS : Window
    {
        private readonly CL_CCNNMANAGER dbms = new();
        private readonly InventoryManager inventoryManager = new();
        private List<ContractStatusModel> currentReportData = new();

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
                TXT_FROM_DATE.CurrentDate = GetFirstDayOfCurrentPersianMonth();
                LoadReport();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void LoadReport()
        {
            long? fromDate = null;
            string digits = new(TXT_FROM_DATE.CurrentDate.Where(char.IsDigit).Select(ToEnglishDigit).ToArray());
            if (digits.Length > 0)
            {
                if (!TryParsePersianDate(digits, out long parsed))
                {
                    new Msgwin(false, "تاریخ شروع معتبر نیست و باید به صورت 1405/05/07 وارد شود.").ShowDialog();
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

            bool inventoryComplete = LoadActualInventory(contractID, data);
            currentReportData = data;
            DG_REPORT.ItemsSource = data;
            UpdateSummary(data, contractID, inventoryComplete);
        }

        private bool LoadActualInventory(int? contractID, List<ContractStatusModel> data)
        {
            foreach (var row in data) row.ActualInventoryQty = null;
            if (!contractID.HasValue || data.Count == 0) return false;

            var inventoryItems = dbms.DoGetDataSQL<ContractInventoryItem>(@"
SELECT DISTINCT D.CODE, F.ANBAR
FROM dbo.CONTRACT_DTL AS D
INNER JOIN dbo.STUF_FSK AS F ON F.CODE = D.CODE
WHERE D.ContractID = @ContractID AND F.ANBAR <> 0",
                new { ContractID = contractID.Value }).ToList();
            if (inventoryItems.Count == 0) return false;

            var (_, _, inventoryDetails) = inventoryManager.GetKalaMogudi(
                dbms, inventoryItems.Cast<object>());
            var inventoryByProduct = inventoryDetails
                .GroupBy(x => x.CODE, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => Convert.ToDecimal(x.Sum(y => y.CURRENT_MOGUDI)), StringComparer.OrdinalIgnoreCase);

            foreach (var row in data)
                if (inventoryByProduct.TryGetValue(row.CODE, out decimal inventory))
                    row.ActualInventoryQty = inventory;

            return inventoryDetails.Count == inventoryItems.Count && data.All(x => x.ActualInventoryQty.HasValue);
        }

        private void UpdateSummary(List<ContractStatusModel> data, int? contractID, bool inventoryComplete)
        {
            decimal contracted = data.Sum(x => x.ContractedQty);
            decimal produced = data.Sum(x => x.ProducedQty);
            decimal remainToProduce = data.Sum(x => x.RemainToProduce);
            decimal sold = data.Sum(x => x.SoldQty);
            decimal contractBalance = data.Sum(x => x.RemainInWarehouse);
            decimal progress = contracted <= 0 ? 0 : produced * 100 / contracted;

            ContractStatusModel? first = data.FirstOrDefault();
            TXT_SELECTED_TITLE.Text = contractID.HasValue && first is not null
                ? $"قرارداد: {first.ContractNo}     |     برند: {first.BrandName}"
                : "نمایش تجمیعی قراردادها";
            TXT_SELECTED_META.Text = contractID.HasValue && first is not null
                ? $"مشتری: {first.CustName}     تاریخ ثبت: {FormatPersianDate(first.ContractDate)}     وضعیت: {(first.IsClosed ? "مختومه" : "باز")}"
                : "برای مشاهده موجودی واقعی و مشخصات کامل، یک قرارداد را انتخاب کنید.";

            TXT_CONTRACTED.Text = $"{contracted:N4} متر";
            TXT_PRODUCED.Text = $"{produced:N4} متر";
            TXT_REMAIN.Text = $"{remainToProduce:N4} متر";
            TXT_PROGRESS.Text = $"{progress:N2}٪";
            TXT_SOLD.Text = $"{sold:N4} متر";
            TXT_CONTRACT_BALANCE.Text = $"{contractBalance:N4} متر";

            if (contractID.HasValue && inventoryComplete)
            {
                decimal actualInventory = data
                    .GroupBy(x => x.CODE, StringComparer.OrdinalIgnoreCase)
                    .Sum(x => x.First().ActualInventoryQty ?? 0);
                TXT_ACTUAL_INVENTORY.Text = $"{actualInventory:N4} متر";
                TXT_INVENTORY_NOTE.Text = "موجودی واقعی از InventoryManager و مجموع انبارهای تعریف‌شده برای کالا محاسبه شده است؛ مانده قراردادی = تولید منتسب − فروش خالص منتسب.";
            }
            else
            {
                TXT_ACTUAL_INVENTORY.Text = "—";
                TXT_INVENTORY_NOTE.Text = contractID.HasValue
                    ? "اطلاعات موجودی واقعی برای همه کالاها/انبارها کامل نیست؛ مانده قراردادی مستقل و بر اساس اسناد متصل محاسبه شده است."
                    : "موجودی واقعی در حالت همه قراردادها جمع نمی‌شود تا کالای مشترک بین چند قرارداد چندبار محاسبه نشود.";
            }
        }

        private static string FormatPersianDate(long value)
        {
            string digits = value.ToString(CultureInfo.InvariantCulture).PadLeft(8, '0');
            return digits.Length == 8 ? $"{digits[..4]}/{digits.Substring(4, 2)}/{digits.Substring(6, 2)}" : value.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetFirstDayOfCurrentPersianMonth()
        {
            var calendar = new PersianCalendar();
            DateTime today = DateTime.Today;
            return $"{calendar.GetYear(today):0000}/{calendar.GetMonth(today):00}/01";
        }

        private void BTN_REFRESH_Click(object sender, RoutedEventArgs e)
        {
            try { LoadReport(); }
            catch (Exception ex) { ShowError(ex); }
        }

        private void BTN_PRINT_Click(object sender, RoutedEventArgs e)
        {
            if (currentReportData.Count == 0)
            {
                new Msgwin(false, "اطلاعاتی برای چاپ گزارش وجود ندارد.").ShowDialog();
                return;
            }

            Process? loader = null;
            try
            {
                loader = CL_LMethods.ProcLoader.Start();
                StiReport report = BuildPrintableReport(currentReportData);
                new global::Rpts.WINRPT(report, "گزارش وضعیت قراردادهای تولید و فروش").Show();
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"ساخت گزارش چاپی انجام نشد.\n{ex.Message}").ShowDialog();
            }
            finally
            {
                CL_LMethods.ProcLoader.Stop(loader);
            }
        }

        private StiReport BuildPrintableReport(IReadOnlyCollection<ContractStatusModel> data)
        {
            DataTable table = CreatePrintDataTable(data);
            var report = new StiReport();
            using var template = Assembly.GetEntryAssembly()?.GetManifestResourceStream("Prg_UI.Rpts.CONTRACT_STATUS.mrt");
            if (template is null)
                throw new InvalidOperationException("قالب چاپی CONTRACT_STATUS.mrt در منابع برنامه پیدا نشد.");

            report.Load(template);
            SetReportCompanyName(report);
            report.Dictionary.DataSources.Clear();
            report.RegData("DataSource1", table);
            report.Dictionary.Synchronize();
            return report;
        }

        private void SetReportCompanyName(StiReport report)
        {
            string? companyNameValue = dbms.DoGetDataSQL<string>(@"
SELECT TOP (1) NULLIF(LTRIM(RTRIM([NAME])), N'')
FROM dbo.SAZMAN").FirstOrDefault();

            if (report.GetComponentByName("COMPANY_NAME") is StiText companyName)
                companyName.Text = companyNameValue ?? string.Empty;
        }

        private static DataTable CreatePrintDataTable(IEnumerable<ContractStatusModel> data)
        {
            var table = new DataTable("DataSource1");
            foreach (string column in new[]
            {
                "ContractNo", "CustName", "BrandName", "CODE", "ProductName", "ContractedQtyText",
                "ProducedQtyText", "RemainToProduceText", "OverProducedQtyText", "SoldQtyText",
                "ContractBalanceText", "ActualInventoryText"
            }) table.Columns.Add(column, typeof(string));

            foreach (ContractStatusModel row in data)
                table.Rows.Add(row.ContractNo, row.CustName, row.BrandName, row.CODE, row.ProductName,
                    row.ContractedQty.ToString("N4"), row.ProducedQty.ToString("N4"), row.RemainToProduce.ToString("N4"),
                    row.OverProducedQty.ToString("N4"), row.SoldQty.ToString("N4"), row.RemainInWarehouse.ToString("N4"),
                    row.ActualInventoryQty?.ToString("N4") ?? "—");
            return table;
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

        private static void ShowError(Exception ex) => new Msgwin(false, $"گزارش وضعیت قراردادها بارگذاری نشد.\n{ex.Message}").ShowDialog();
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
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                return;
            }
            if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None) return;

            e.Handled = true;
            if (Keyboard.FocusedElement is Button button)
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            else
                CL_LMethods.SendKey_US(Key.Tab);
        }

        private sealed class ContractLookup { public int ContractID { get; set; } public string DisplayName { get; set; } = string.Empty; }
        private sealed class ContractInventoryItem
        {
            public string CODE { get; set; } = string.Empty;
            public int ANBAR { get; set; }
            public double MEGHk { get; set; }
            public double MEGH_MAR { get; set; }
        }
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
            public decimal? ActualInventoryQty { get; set; }
        }
    }
}
