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

            //
            //var report = new StiReport();

            System.IO.Stream? pathreport = null;
            if (!string.IsNullOrEmpty(ReportAssebmlyPath)) //Remort Managing Passed
            {
                pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"Prg_UI.Rpts.{ReportAssebmlyPath}.mrt");
                MyReport.Load(pathreport);
                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                MyReport.Dictionary.Databases.Clear();
                MyReport.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                //report["ANBAR"] = ANBAR.SelectedValue.ToString();
                //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;
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
