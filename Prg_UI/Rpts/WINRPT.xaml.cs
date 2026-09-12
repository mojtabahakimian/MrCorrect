using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.UiTools;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace Rpts
{
    public partial class WINRPT : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
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

        /// <summary>
        /// Prg_UI.Rpts.{ReportAssebmlyPath}.mrt
        /// </summary>
        /// <param name="_ReportAssebmlyPath_"></param>
        /// <param name="_WIN_"></param>
        public WINRPT(string _ReportAssebmlyPath_, Visual _WIN_, string _RerportTitle_)
        {
            InitializeComponent();

            this.DataContext = this;

            ReportAssebmlyPath = _ReportAssebmlyPath_;
            THEWIN = _WIN_;
            LBL_HEADER.Content = _RerportTitle_;
        }
        public WINRPT(StiReport _MyReport_, string _RerportTitle_)
        {
            InitializeComponent();

            this.DataContext = this;

            MyReport = _MyReport_;

            LBL_HEADER.Content = _RerportTitle_;

            #region MyRegion
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            // پیش از این فقط نام کلاس «WINRPT» ثبت می‌شد و معلوم نبود کدام
            // گزارش باز شده. حالا عنوان واقعی گزارش ثبت می‌شود و چاپ واقعی و
            // خروجی گرفتن هم از رویدادهای خودِ Stimulsoft تشخیص داده می‌شوند،
            // نه از باز شدن پنجره.
            try
            {
                Prg_Proccessy.AUDIT.Audit.Print(_RerportTitle_, isPreview: true, formName: this.GetType().Name);
                AttachReportAuditEvents(_RerportTitle_);
            }
            catch { }

            //
            //var report = new StiReport();

            System.IO.Stream? pathreport = null;
            if (!string.IsNullOrEmpty(ReportAssebmlyPath)) //Remort Managing Passed
            {
                pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"Prg_UI.Rpts.{ReportAssebmlyPath}.mrt");
                MyReport.Load(pathreport);
                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                MyReport.Dictionary.Databases.Clear();
                MyReport.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                //report["ANBAR"] = ANBAR.SelectedValue.ToString();
                //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 900;

              
            }
            else //Already Report Passed
            {

            }

            ////MyReport.Render();
            RenderReportWithRetry(MyReport);


            TheReportViewer.Report = MyReport;

            pathreport?.Dispose();
            #endregion
        }

        /// <summary>
        /// اتصال ثبت سابقه به رویدادهای خودِ گزارش.
        ///
        /// «باز کردن گزارش» با «چاپ گرفتن» یکی نیست؛ کاربر ممکن است گزارش را
        /// ببیند و چاپ نکند. این رویدادها همان لحظه‌ی واقعی چاپ و خروجی را
        /// می‌دهند.
        ///
        /// لامبداها عمداً دو پارامتری و بدون نوع صریح نوشته شده‌اند تا به نوع
        /// دقیق delegate در نسخه‌ی نصب‌شده‌ی Stimulsoft وابسته نباشند.
        /// همچنین هیچ ارجاعی به this نگه نمی‌دارند تا پنجره را زنده نگه ندارند.
        ///
        /// ⚠ هرگز به رویدادهای خودِ StiWpfViewerControl وصل نشوید — مخصوصاً
        /// ProcessExport. آن رویداد «اعلان» نیست، «جایگزین» است. کد خودِ
        /// Stimulsoft (از دیس‌اسمبلِ Stimulsoft.Report.Wpf.dll) این است:
        ///
        ///     void InvokeProcessExport(object o) {
        ///         if (this.ProcessExport != null) { this.ProcessExport(o, EventArgs.Empty); return; }
        ///         this.OnProcessExport(o);   // ← خروجی گرفتنِ واقعی همین‌جاست
        ///     }
        ///
        /// یعنی به‌محض اینکه یک handler وصل شود، ویور نتیجه می‌گیرد که میزبان
        /// خودش خروجی را می‌سازد و OnProcessExport (که StiExportService.Export
        /// را صدا می‌زند) اصلاً اجرا نمی‌شود. handler ما فقط یک ردیف سابقه
        /// می‌نوشت و برمی‌گشت، پس Save ▸ Adobe PDF File هیچ کاری نمی‌کرد.
        ///
        /// همین اتفاق در کامیت b9694f0 افتاد و اینجا برگردانده شد.
        /// </summary>
        private void AttachReportAuditEvents(string reportTitle)
        {
            var report = MyReport;
            if (report is null) return;

            var formName = nameof(WINRPT);

            try
            {
                report.Printed += (s, e) =>
                {
                    try { Prg_Proccessy.AUDIT.Audit.Print(reportTitle, isPreview: false, formName: formName); }
                    catch { }
                };
            }
            catch { }

            try
            {
                report.Exported += (s, e) =>
                {
                    try { Prg_Proccessy.AUDIT.Audit.Export("فایل", reportTitle, formName: formName); }
                    catch { }
                };
            }
            catch { }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        private string ReportAssebmlyPath { get; set; }
        private Visual THEWIN { get; set; }
        public bool NowIsReady { get; private set; }
        private StiReport MyReport { get; set; } = new StiReport();

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;

            //    CL_LMethods.SendKey_US(Key.Tab);
            //}
        }
        private void BTN_PRINT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RenderReportWithRetry(StiReport report, int maxAttempts = 3, int baseDelayMilliseconds = 300)
        {
            if (report is null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (maxAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            }

            if (baseDelayMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseDelayMilliseconds));
            }

            foreach (StiDataSource dataSource in MyReport.Dictionary.DataSources)
            {
                if (dataSource is StiSqlSource sqlSource)
                {
                    sqlSource.CommandTimeout = 900;
                }
            }

            var attempt = 0;
            while (true)
            {
                try
                {
                    attempt++;
                    report.Render();
                    return;
                }
                catch (SqlException ex) when (IsDeadlockException(ex) && attempt < maxAttempts)
                {
                    var delay = baseDelayMilliseconds * attempt;
                    //Debug.WriteLine($"Deadlock detected while rendering report (attempt {attempt}). Retrying in {delay} ms.");
                    Thread.Sleep(delay);
                }
            }
        }

        private static bool IsDeadlockException(SqlException exception)
        {
            foreach (SqlError error in exception.Errors)
            {
                if (error.Number == 1205)
                {
                    return true;
                }
            }

            return exception.Number == 1205;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // 1) گزارشِ نمایشگر را از کنترل جدا کن
                if (TheReportViewer != null)
                {
                    TheReportViewer.Report = null;
                    TheReportViewer.Dispose();      // اختیاری اگر کنترل IDisposable است
                }

                // 2) خودِ StiReport را Dispose کن
                if (MyReport != null)
                {
                    MyReport.Dispose();
                    MyReport = null;
                }

                // 3) حذف ارجاع اضافی
                THEWIN = null;
            }
            catch { }
        }
    }
}
