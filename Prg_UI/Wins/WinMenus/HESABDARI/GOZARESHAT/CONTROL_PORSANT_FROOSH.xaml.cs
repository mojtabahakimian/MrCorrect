using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        /// <summary>یک سطر گزارش: وضعیت فعلیِ پورسانتِ یک فاکتور در برابر آنچه باید باشد.</summary>
        public class PorsantAuditRow
        {
            public long? ID { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string? CUST_NO { get; set; }
            public long? DATE_N { get; set; }
            public string? CUST_NAME { get; set; }
            public int? PORID { get; set; }
            public bool HAS_PATTERN { get; set; }
            public double? DARSAD { get; set; }
            public double? OLD_PURSANT { get; set; }
            public double? NEW_PURSANT { get; set; }
            public double? NET_BASE { get; set; }
            public double? PATTERN_AMOUNT { get; set; }
            public string? WARNING { get; set; }

            public double DIFF => (NEW_PURSANT ?? 0) - (OLD_PURSANT ?? 0);
            public string HAS_PATTERN_TEXT => HAS_PATTERN ? "دارد" : "ندارد";
        }

        public ObservableCollection<PorsantAuditRow> AUDIT_DATA { get; set; } = new ObservableCollection<PorsantAuditRow>();
        public bool NowIsReady { get; private set; }

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

        /// <summary>
        /// اجرای dbo.RecalcVisitorPorsant_ByDarsad در حالت پیش‌نمایش و پر کردن گرید.
        /// </summary>
        private void LoadAudit(bool showEmptyMessage)
        {
            AUDIT_DATA.Clear();
            List<PorsantAuditRow> rows;
            try
            {
                rows = dbms.DoGetDataSQL<PorsantAuditRow>(
                    @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=@pNUMBER, @TAG=@pTAG, @FromDate=@pFromDate, @ToDate=@pToDate, @PREVIEW_ONLY=@pPreview",
                    new
                    {
                        pNUMBER = (double?)null,
                        pTAG = (double?)2,
                        pFromDate = (long?)null,
                        pToDate = (long?)null,
                        pPreview = true
                    }).ToList();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در محاسبه‌ی پورسانت‌های نادرست. اگر تازه نصب/به‌روزرسانی کرده‌اید، ابتدا برنامه را یک‌بار ببندید و دوباره باز کنید تا مهاجرت‌های پایگاه‌داده اجرا شوند.\n" + ex.Message).ShowDialog();
                this.Close();
                return;
            }

            foreach (var item in rows)
            {
                AUDIT_DATA.Add(item);
            }

            UpdateSummary();

            if (AUDIT_DATA.Count == 0 && showEmptyMessage)
            {
                new Msgwin(false, "هیچ فاکتوری با پورسانت نادرست پیدا نشد.").ShowDialog();
                this.Close();
            }
        }

        private void UpdateSummary()
        {
            SummaryLabel.Content = AUDIT_DATA.Count == 0
                ? "هیچ مغایرتی نمانده است."
                : $"{AUDIT_DATA.Count} سطر پورسانت نادرست، در {AUDIT_DATA.Select(x => x.NUMBER).Distinct().Count()} فاکتور.";

            FixButton.IsEnabled = AUDIT_DATA.Count > 0;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAudit(showEmptyMessage: true);
        }

        /// <summary>
        /// دکمه‌ی اصلاح: با تاییدِ کاربر، مبلغ پورسانتِ همه‌ی سطرهای فهرست‌شده را درست می‌کند
        /// و سند حسابداری فاکتورهایی که واقعاً تغییر کردند را دوباره صادر می‌کند.
        /// </summary>
        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            if (AUDIT_DATA.Count == 0) return;

            var invoiceCount = AUDIT_DATA.Select(x => x.NUMBER).Distinct().Count();
            var confirm = new Msgwin(true,
                $"مبلغ پورسانت {AUDIT_DATA.Count} سطر (در {invoiceCount} فاکتور) اصلاح و سند حسابداری همان فاکتورها دوباره صادر می‌شود.\n" +
                "این عملیات مبلغ پورسانتِ ذخیره‌شده و سند حسابداری را تغییر می‌دهد. آیا ادامه می‌دهید؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true) return;

            var targetInvoices = AUDIT_DATA.Select(x => x.NUMBER).Where(n => n.HasValue).Select(n => n.Value).Distinct().ToList();
            var succeeded = new List<double>();
            var failed = new List<(double Number, string Error)>();

            foreach (var invNumber in targetInvoices)
            {
                try
                {
                    using (var ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        dbms.DoExecuteSQL(
                            @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=@pNUMBER, @TAG=@pTAG, @FromDate=@pFromDate, @ToDate=@pToDate, @PREVIEW_ONLY=@pPreview",
                            new
                            {
                                pNUMBER = (double?)invNumber,
                                pTAG = (double?)2,
                                pFromDate = (long?)null,
                                pToDate = (long?)null,
                                pPreview = false
                            });

                        var (sanadNumber, isSuccessfully) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(Convert.ToInt64(invNumber), Convert.ToInt64(invNumber), false);

                        if (isSuccessfully)
                        {
                            ts.Complete();
                            succeeded.Add(invNumber);
                        }
                        else
                        {
                            failed.Add((invNumber, "صدور سند ناموفق بود"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    failed.Add((invNumber, ex.Message));
                }
            }

            var resultMsg = new StringBuilder();
            if (succeeded.Count > 0)
            {
                resultMsg.AppendLine($"اصلاح پورسانت و صدور مجدد سند برای {succeeded.Count} فاکتور با موفقیت انجام شد.");
            }
            if (failed.Count > 0)
            {
                resultMsg.AppendLine($"عملیات برای {failed.Count} فاکتور ناموفق بود و تغییرات آن‌ها بازگردانده شد:");
                foreach (var f in failed.Take(20))
                {
                    resultMsg.AppendLine($"  فاکتور {f.Number}: {f.Error}");
                }
            }

            new Msgwin(false, resultMsg.ToString()).ShowDialog();

            // بازخوانی فهرست تا وضعیت نهایی نشان داده شود
            LoadAudit(showEmptyMessage: false);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}
