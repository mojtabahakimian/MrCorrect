using AUTO_BAZ.Functions;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Prg_UI.Wins.WinMenus.HESABDARI.GOZARESHAT
{
    /// <summary>
    /// کنترل پورسانت فاکتور فروش: فاکتورهایی که مبلغ پورسانت ثبت‌شده‌شان با قاعده‌ی
    /// درست (الگو → جمع نرخ کالا، بدون‌الگو → درصد × مبنای فاکتور) نمی‌خواند را
    /// فهرست می‌کند و با تایید کاربر اصلاح می‌کند و سند حسابداری همان فاکتورها را
    /// دوباره صادر می‌کند.
    ///
    /// همه‌ی محاسبه‌ها از CL_PORSANT_RULE می‌آید — همان قاعده‌ای که خودِ صدور سند
    /// (GENSANADFROOSH) استفاده می‌کند. پیش از این، این پنجره از پروسیجر جداگانه‌ی
    /// dbo.RecalcVisitorPorsant_ByDarsad استفاده می‌کرد که فرمولش با صدور سند فرق
    /// داشت؛ نتیجه این بود که اصلاح انجام می‌شد، بلافاصله صدور سند مبلغ دیگری
    /// می‌نوشت و همان سطرها دوباره مغایر برمی‌گشتند.
    /// </summary>
    public partial class CONTROL_PORSANT_FROOSH : Window
    {
        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null) packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null) packIcon.Kind = PackIconKind.WindowRestore;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        public ObservableCollection<CL_PORSANT_RULE.PorsantAuditRow> AUDIT_DATA { get; set; }
            = new ObservableCollection<CL_PORSANT_RULE.PorsantAuditRow>();

        public bool NowIsReady { get; private set; }

        /// <summary>تعداد سطرهایی که «اصلاح و صدور مجدد» واقعاً می‌تواند درستشان کند.</summary>
        private int FixableCount => AUDIT_DATA.Count(x => x.CAN_FIX);

        public CONTROL_PORSANT_FROOSH()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAudit(showEmptyMessage: true);
        }

        /// <summary>خواندن مغایرت‌ها از دیتابیس و پر کردن گرید.</summary>
        private void LoadAudit(bool showEmptyMessage)
        {
            List<CL_PORSANT_RULE.PorsantAuditRow> rows;
            try
            {
                rows = CL_PORSANT_RULE.Audit();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در محاسبه‌ی پورسانت‌های نادرست.\n" + ex.Message).ShowDialog();
                this.Close();
                return;
            }

            ShowRows(rows);

            if (AUDIT_DATA.Count == 0 && showEmptyMessage)
            {
                // پنجره بسته نمی‌شود: کاربر ممکن است بخواهد ریز محاسبه‌ی یک فاکتور مشخص را
                // ببیند («چرا پورسانتِ این فاکتور این‌قدر کم شد؟»)، و آن فاکتور دقیقاً به این
                // دلیل در فهرست نیست که مبلغش با قاعده می‌خواند.
                new Msgwin(false,
                    "هیچ فاکتوری با پورسانت نادرست پیدا نشد.\n" +
                    "برای دیدن ریز محاسبه‌ی پورسانت یک فاکتور، شماره‌اش را در پایین وارد کنید و «این مبلغ از کجا آمده؟» را بزنید.")
                    .ShowDialog();
            }
        }

        /// <summary>نشاندن یک فهرستِ از پیش خوانده‌شده در گرید (بدون رفت‌وبرگشت دوباره به دیتابیس).</summary>
        private void ShowRows(IEnumerable<CL_PORSANT_RULE.PorsantAuditRow> rows)
        {
            AUDIT_DATA.Clear();
            foreach (var item in rows)
            {
                AUDIT_DATA.Add(item);
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (AUDIT_DATA.Count == 0)
            {
                SummaryLabel.Content = "هیچ مغایرتی نمانده است.";
            }
            else
            {
                var invoices = AUDIT_DATA.Select(x => x.NUMBER).Distinct().Count();
                var manual = AUDIT_DATA.Count - FixableCount;

                SummaryLabel.Content = manual == 0
                    ? $"{AUDIT_DATA.Count} سطر پورسانت نادرست، در {invoices} فاکتور."
                    : $"{AUDIT_DATA.Count} سطر پورسانت نادرست، در {invoices} فاکتور — {manual} سطر نیاز به بررسی دستی دارد و اصلاح خودکار نمی‌شود.";
            }

            FixButton.IsEnabled = FixableCount > 0;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAudit(showEmptyMessage: true);
        }

        /// <summary>
        /// «این مبلغ از کجا آمده؟» — ریز محاسبه‌ی سطرِ انتخاب‌شده: مبنای هر قلم، نرخِ همان
        /// قلم در الگو، سهمش از پورسانت، و مهم‌تر از همه فهرست اقلامی که در الگو نرخ ندارند
        /// و هیچ پورسانتی نمی‌دهند. پرتکرارترین شکایت («۲٪ گفتیم ولی مبلغ خیلی کمتر است»)
        /// دقیقاً همین است و تا امروز هیچ‌جای برنامه جوابش را نشان نمی‌داد.
        /// </summary>
        private void WhyButton_Click(object sender, RoutedEventArgs e)
        {
            // اگر کاربر شماره‌ی فاکتور نوشته باشد، همان فاکتور بررسی می‌شود — حتی اگر در این
            // فهرست نباشد. فاکتوری که مبلغش دقیقاً همان چیزی است که قاعده می‌گوید، مغایرت
            // ندارد و اینجا فهرست نمی‌شود؛ ولی پرسشِ «چرا این‌قدر کم شد؟» درست درباره‌ی همین
            // فاکتورهاست.
            if (!string.IsNullOrWhiteSpace(InvoiceNumberBox.Text))
            {
                ExplainInvoiceByNumber(InvoiceNumberBox.Text.Trim());
                return;
            }

            var row = SYNCFUSION_DG.SelectedItem as CL_PORSANT_RULE.PorsantAuditRow;
            if (row is null)
            {
                new Msgwin(false, "ابتدا یک سطر را انتخاب کنید یا شماره فاکتور را وارد کنید.").ShowDialog();
                return;
            }

            ExplainRow(row);
        }

        /// <summary>ریز محاسبه‌ی همه‌ی سطرهای پورسانتِ یک فاکتور، با شماره‌ی واردشده‌ی کاربر.</summary>
        private void ExplainInvoiceByNumber(string text)
        {
            if (!double.TryParse(text, out double number))
            {
                new Msgwin(false, "شماره فاکتور را درست وارد کنید.").ShowDialog();
                return;
            }

            List<CL_PORSANT_RULE.PorsantAuditRow> rows;
            try
            {
                rows = CL_PORSANT_RULE.InspectInvoice(number);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خواندن اطلاعات این فاکتور ممکن نشد.\n" + ex.Message).ShowDialog();
                return;
            }

            if (rows.Count == 0)
            {
                new Msgwin(false, $"برای فاکتور {number:0} هیچ سطر پورسانتی پیدا نشد (یا فاکتور سربرگ فروش ندارد).").ShowDialog();
                return;
            }

            foreach (var row in rows)
            {
                ExplainRow(row);
            }
        }

        /// <summary>متن «این مبلغ از کجا آمده؟» برای یک سطر پورسانت.</summary>
        private void ExplainRow(CL_PORSANT_RULE.PorsantAuditRow row)
        {
            if (!row.HAS_PATTERN)
            {
                new Msgwin(false,
                    $"فاکتور {row.NUMBER:0} الگوی پورسانت ندارد؛ مبلغ = درصد سطر × مبنای فاکتور.\n" +
                    $"مبنای فاکتور: {row.NET_BASE:N0}\nدرصد: {row.DARSAD:0.###}\nمبلغ درست: {row.NEW_PURSANT:N0}").ShowDialog();
                return;
            }

            try
            {
                var breakdown = CL_PORSANT_RULE.GetPatternBreakdown(row.NUMBER ?? 0, row.TAG ?? 2, row.PORID ?? 0);
                var report = $"ویزیتور: {row.CUST_NAME} ({row.CUST_NO})\n" +
                             $"پورسانت ثبت‌شده در حال حاضر: {row.OLD_PURSANT ?? 0:N0}\n\n" +
                             breakdown.BuildReport(row.NET_BASE);

                new Msgwin(false, report).ShowDialog();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خواندن ریز محاسبه‌ی الگو ممکن نشد.\n" + ex.Message).ShowDialog();
            }
        }

        /// <summary>
        /// دکمه‌ی اصلاح: با تاییدِ کاربر، مبلغ پورسانتِ سطرهای قابل اصلاح را در یک
        /// تراکنش درست می‌کند، سند حسابداری همان فاکتورها را دوباره صادر می‌کند و
        /// در پایان فهرست را از دیتابیس بازمی‌خواند. پیام موفقیت فقط برای فاکتورهایی
        /// داده می‌شود که همین بازخوانی ثابت کند مغایرتشان واقعاً رفته است.
        /// </summary>
        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            var targets = AUDIT_DATA.Where(x => x.CAN_FIX).ToList();
            if (targets.Count == 0) return;

            var invoiceCount = targets.Select(x => x.NUMBER).Distinct().Count();
            var confirm = new Msgwin(true,
                $"مبلغ پورسانت {targets.Count} سطر (در {invoiceCount} فاکتور) اصلاح و سند حسابداری همان فاکتورها دوباره صادر می‌شود.\n" +
                "این عملیات مبلغ پورسانتِ ذخیره‌شده و سند حسابداری را تغییر می‌دهد. آیا ادامه می‌دهید؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true) return;

            CL_PORSANT_RULE.PorsantFixResult result;

            FixButton.IsEnabled = false;
            RefreshButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                result = CL_PORSANT_RULE.FixAndReissue(targets);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "اصلاح پورسانت انجام نشد.\n" + ex.Message).ShowDialog();
                LoadAudit(showEmptyMessage: false);
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
                RefreshButton.IsEnabled = true;
            }

            // فهرست تازه‌ای که خودِ عملیات از دیتابیس خوانده؛ همین معیار موفقیت است
            ShowRows(result.Remaining);

            new Msgwin(false, BuildResultMessage(result)).ShowDialog();
        }

        /// <summary>
        /// گزارش نتیجه بر اساس وضعیتی که از دیتابیس بازخوانی شده، نه بر اساس
        /// تعداد سطرهایی که قرار بود اصلاح شوند.
        /// </summary>
        private static string BuildResultMessage(CL_PORSANT_RULE.PorsantFixResult result)
        {
            var msg = new StringBuilder();
            var failed = result.Failed.ToList();

            if (result.SucceededInvoices > 0)
            {
                msg.AppendLine($"پورسانت {result.SucceededInvoices} فاکتور اصلاح شد و سند حسابداری همان فاکتورها دوباره صادر شد.");
            }

            if (failed.Count > 0)
            {
                msg.AppendLine($"{failed.Count} فاکتور اصلاح نشد و همچنان در فهرست می‌ماند:");
                foreach (var item in failed.Take(20))
                {
                    var reason = !string.IsNullOrEmpty(item.Error)
                        ? item.Error
                        : (item.RowsUpdated == 0
                            ? "هیچ سطری به‌روزرسانی نشد"
                            : "پس از صدور مجدد سند، مبلغ پورسانت دوباره با قاعده نخواند");

                    msg.AppendLine($"  فاکتور {item.NUMBER}: {reason}");
                }

                if (failed.Count > 20)
                {
                    msg.AppendLine($"  ... و {failed.Count - 20} فاکتور دیگر.");
                }
            }

            if (result.Outcomes.Count == 0)
            {
                msg.AppendLine("هیچ سطر قابل اصلاحی وجود نداشت.");
            }

            var manual = result.Remaining.Count(r => !r.CAN_FIX);
            if (manual > 0)
            {
                msg.AppendLine($"{manual} سطر نیاز به بررسی دستی دارد (ستون «هشدار» را ببینید) و با این عملیات اصلاح نمی‌شود.");
            }

            return msg.ToString();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        /// <summary>
        /// Ctrl+L روی جدول: جمعِ سطرهای انتخاب‌شده. برخلاف پنجره‌های دیگر که فقط ستونِ
        /// جاری را جمع می‌زنند، اینجا هر سه ستونِ مبلغ با هم داده می‌شود — چون کلِ کاربردِ
        /// این جمع، دیدنِ «مجموع اختلافِ پورسانت» است و کاربر ناچار می‌شد دو بار جمع بگیرد
        /// و خودش تفریق کند.
        /// </summary>
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                ShowSelectionSums();
                e.Handled = true;
            }
        }

        private void SumSelected_Click(object sender, RoutedEventArgs e) => ShowSelectionSums();

        private void ShowSelectionSums()
        {
            // اگر چیزی انتخاب نشده باشد، جمعِ کلِ فهرست داده می‌شود (نه هیچ‌چیز)؛ کاربر
            // معمولاً همین را می‌خواهد و Ctrl+A هم دقیقاً همین نتیجه را می‌دهد.
            var rows = SYNCFUSION_DG.SelectedItems?.OfType<CL_PORSANT_RULE.PorsantAuditRow>().ToList();
            bool wholeList = rows is null || rows.Count == 0;

            if (wholeList)
            {
                rows = AUDIT_DATA.ToList();
            }

            if (rows.Count == 0)
            {
                new Msgwin(false, "سطری برای جمع زدن وجود ندارد.").ShowDialog();
                return;
            }

            double oldSum = rows.Sum(r => r.OLD_PURSANT ?? 0);
            double newSum = rows.Sum(r => r.NEW_PURSANT);
            double diffSum = rows.Sum(r => r.DIFF);
            var invoices = rows.Select(r => r.NUMBER).Distinct().Count();

            var title = wholeList
                ? $"جمع کل فهرست ({rows.Count} سطر در {invoices} فاکتور):"
                : $"جمع {rows.Count} سطر انتخاب‌شده (در {invoices} فاکتور):";

            new Msgwin(false,
                $"{title}\n\n" +
                $"پورسانت وضعیت فعلی: {oldSum:N0}\n" +
                $"پورسانت باید باشد: {newSum:N0}\n" +
                $"اختلاف (باید باشد − فعلی): {diffSum:N0}").ShowDialog();
        }

        /// <summary>خروجی اکسل از همان چیزی که در جدول دیده می‌شود (با فیلتر و ترتیب فعلی).</summary>
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ControlPorsantFroosh");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
    }
}
