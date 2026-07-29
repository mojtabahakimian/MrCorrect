using Functions;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Rpts;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
                new WINRPT(report, "گزارش وضعیت قراردادهای تولید و فروش").Show();
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
            decimal contracted = data.Sum(x => x.ContractedQty);
            decimal produced = data.Sum(x => x.ProducedQty);
            decimal remain = data.Sum(x => x.RemainToProduce);
            decimal sold = data.Sum(x => x.SoldQty);
            decimal balance = data.Sum(x => x.RemainInWarehouse);

            var report = new StiReport
            {
                ReportName = "ContractStatusReport",
                ReportAlias = "گزارش وضعیت قراردادهای تولید و فروش",
                ReportUnit = StiReportUnitType.Centimeters
            };
            report.RegData("ContractRows", table);
            report.Dictionary.Synchronize();

            var page = new StiPage
            {
                Name = "ContractStatusPage",
                Orientation = StiPageOrientation.Landscape,
                PageWidth = 29.7,
                PageHeight = 21,
                Margins = new StiMargins(0.7, 0.7, 0.7, 0.7)
            };

            var titleBand = new StiReportTitleBand { Name = "TitleBand", Height = 2.15 };
            AddReportText(titleBand, 0, 0, 28.3, 0.75, "گزارش وضعیت قراردادهای تولید و فروش", 15, true, StiTextHorAlignment.Center, Color.FromArgb(20, 93, 123), Color.Transparent, false);
            AddReportText(titleBand, 0, 0.82, 28.3, 0.55, TXT_SELECTED_TITLE.Text, 10, true, StiTextHorAlignment.Center, Color.Black, Color.Transparent, false);
            AddReportText(titleBand, 0, 1.38, 28.3, 0.5, TXT_SELECTED_META.Text, 9, false, StiTextHorAlignment.Center, Color.DimGray, Color.Transparent, false);

            var summaryBand = new StiHeaderBand { Name = "SummaryBand", Height = 1.25 };
            string[] summaryTexts =
            {
                $"متراژ قرارداد: {contracted:N4}", $"تولید شده: {produced:N4}",
                $"مانده تولید: {remain:N4}", $"فروش خالص: {sold:N4}",
                $"مانده قراردادی: {balance:N4}"
            };
            double summaryWidth = 28.3 / summaryTexts.Length;
            for (int index = 0; index < summaryTexts.Length; index++)
                AddReportText(summaryBand, index * summaryWidth, 0.18, summaryWidth - 0.08, 0.75,
                    summaryTexts[index], 9, true, StiTextHorAlignment.Center, Color.FromArgb(45, 55, 72),
                    index % 2 == 0 ? Color.FromArgb(238, 246, 255) : Color.FromArgb(239, 251, 246), true);

            string[] headers = { "شماره قرارداد", "مشتری", "برند", "کد طرح", "نام طرح / کالا", "متراژ تعهد", "تولید", "مانده تولید", "مازاد تولید", "فروش خالص", "مانده قراردادی", "موجودی واقعی" };
            string[] fields = { "ContractNo", "CustName", "BrandName", "CODE", "ProductName", "ContractedQtyText", "ProducedQtyText", "RemainToProduceText", "OverProducedQtyText", "SoldQtyText", "ContractBalanceText", "ActualInventoryText" };
            double[] widths = { 2.0, 3.7, 2.3, 1.35, 5.0, 2.0, 1.8, 2.0, 1.8, 1.8, 2.1, 2.45 };

            var headerBand = new StiHeaderBand { Name = "ColumnHeaderBand", Height = 0.85 };
            var dataBand = new StiDataBand { Name = "ContractDataBand", Height = 0.72, DataSourceName = "ContractRows" };
            double x = 0;
            for (int index = 0; index < headers.Length; index++)
            {
                AddReportText(headerBand, x, 0, widths[index], 0.85, headers[index], 8, true,
                    StiTextHorAlignment.Center, Color.White, Color.FromArgb(27, 153, 198), true);
                AddReportText(dataBand, x, 0, widths[index], 0.72, $"{{ContractRows.{fields[index]}}}", 8, false,
                    index is 1 or 4 ? StiTextHorAlignment.Right : StiTextHorAlignment.Center,
                    Color.Black, Color.White, true);
                x += widths[index];
            }

            var footerBand = new StiFooterBand { Name = "FooterBand", Height = 0.85 };
            AddReportText(footerBand, 0, 0.08, 28.3, 0.6, TXT_INVENTORY_NOTE.Text, 8, false,
                StiTextHorAlignment.Right, Color.DimGray, Color.FromArgb(248, 248, 248), true);

            page.Components.AddRange(new StiComponent[] { titleBand, summaryBand, headerBand, dataBand, footerBand });
            report.Pages.Add(page);
            return report;
        }

        private static DataTable CreatePrintDataTable(IEnumerable<ContractStatusModel> data)
        {
            var table = new DataTable("ContractRows");
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

        private static void AddReportText(StiBand band, double x, double y, double width, double height,
            string text, float fontSize, bool bold, StiTextHorAlignment horizontalAlignment,
            Color textColor, Color backgroundColor, bool showBorder)
        {
            var component = new StiText
            {
                ClientRectangle = new RectangleD(x, y, width, height),
                Text = text,
                Font = new Font("IRANYekanFN", fontSize,
                    bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
                HorAlignment = horizontalAlignment,
                VertAlignment = StiVertAlignment.Center,
                TextBrush = new StiSolidBrush(textColor),
                Brush = new StiSolidBrush(backgroundColor),
                TextOptions = new StiTextOptions(true, false, false, 0F, System.Drawing.Text.HotkeyPrefix.None, StringTrimming.EllipsisCharacter),
                Border = showBorder
                    ? new StiBorder(StiBorderSides.All, Color.FromArgb(205, 214, 223), 1, StiPenStyle.Solid,
                        false, 4, new StiSolidBrush(Color.FromArgb(205, 214, 223)), false)
                    : new StiBorder(StiBorderSides.None, Color.Black, 1, StiPenStyle.Solid,
                        false, 4, new StiSolidBrush(Color.Black), false)
            };
            band.Components.Add(component);
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
