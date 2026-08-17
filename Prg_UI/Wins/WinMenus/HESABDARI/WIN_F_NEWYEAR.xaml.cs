using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_SendInvoice.CNNMANAGER;
using ScriptSqly.Migrations;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for WIN_F_NEWYEAR.xaml
    /// </summary>
    public partial class WIN_F_NEWYEAR : Window
    {
        public WIN_F_NEWYEAR()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private string currentDbName = string.Empty;

        UniversControl universControl = new UniversControl();

        private class BackupFileModel
        {
            public string LogicalName { get; set; }
            public string Type { get; set; }
        }
        private class ColumnMetaModel
        {
            public string ColumnName { get; set; }
            public bool IsIdentity { get; set; }
            public int ColumnOrder { get; set; }
        }
        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "NEWYEAR", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            try
            {
                currentDbName = dbms.DoGetDataSQL<string>("SELECT DB_NAME()").FirstOrDefault() ?? string.Empty;
                txtCurrentDbName.Text = currentDbName;

                string physicalPath = dbms.DoGetDataSQL<string>(
                    $"SELECT physical_name FROM sys.master_files WHERE database_id = DB_ID('{currentDbName}') AND type = 0"
                ).FirstOrDefault() ?? string.Empty;

                txtCurrentDbPath.Text = physicalPath;

                string currentDbFolder = string.Empty;
                string databasesRootFolder = string.Empty;

                if (!string.IsNullOrWhiteSpace(physicalPath))
                {
                    // مثال:
                    // physicalPath = E:\Databases\YAZD2025\YAZD2025_Data.mdf
                    // currentDbFolder = E:\Databases\YAZD2025
                    currentDbFolder = Path.GetDirectoryName(physicalPath) ?? string.Empty;

                    // ریشه دیتابیس‌ها:
                    // databasesRootFolder = E:\Databases
                    try
                    {
                        databasesRootFolder = Directory.GetParent(currentDbFolder)?.FullName ?? string.Empty;
                    }
                    catch
                    {
                        databasesRootFolder = string.Empty;
                    }

                    // جستجوی هوشمند برای فایل پشتیبان خام
                    string potentialTemplate = Path.Combine(currentDbFolder, "files", "neginsql.bak");
                    if (!File.Exists(potentialTemplate))
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(databasesRootFolder))
                            {
                                potentialTemplate = Path.Combine(databasesRootFolder, "files", "neginsql.bak");
                            }
                        }
                        catch
                        {
                        }
                    }

                    txtTemplateBackupFile.Text = potentialTemplate;
                }

                int nextYear;
                string baseName;

                Match match = Regex.Match(currentDbName, @"\d{2,4}$");
                if (match.Success && int.TryParse(match.Value, out int currentYear))
                {
                    nextYear = currentYear + 1;
                    baseName = currentDbName.Substring(0, match.Index);
                }
                else
                {
                    nextYear = (int)(CL_HESABDARI.FARSIDATE() / 10000 + 1);
                    baseName = currentDbName;
                }

                int CurretYear = new PersianCalendar().GetYear(DateTime.Now);

                if (CurretYear <= Baseknow.YEA)
                {
                    while (CurretYear <= Baseknow.YEA)
                    {
                        YEA.Text = (CurretYear + 1).ToString();
                    }
                }
                else
                {
                    YEA.Text = (CurretYear).ToString(); //تاریخ سال جدید هست
                }
                txtNewDbName.Text = $"{baseName}{nextYear}";

                // مسیر درست:
                // E:\Databases\YAZD2026\
                if (!string.IsNullOrWhiteSpace(databasesRootFolder) && !string.IsNullOrWhiteSpace(txtNewDbName.Text))
                {
                    txtNewDbPath.Text = Path.Combine(databasesRootFolder, txtNewDbName.Text) + "\\";
                }
                else if (!string.IsNullOrWhiteSpace(currentDbFolder))
                {
                    // fallback
                    txtNewDbPath.Text = currentDbFolder + "\\";
                }
                else
                {
                    txtNewDbPath.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری اطلاعات دیتابیس:\n" + ex.Message).ShowDialog();
                this.Close();
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        #region PROGRESSBAR
        private static readonly System.Windows.Media.Brush NormalStatusBrush = new System.Windows.Media.SolidColorBrush(
        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0F172A"));

        private static readonly System.Windows.Media.Brush ErrorStatusBrush = System.Windows.Media.Brushes.Red;

        private int _progressCurrentStep = 0;
        private int _progressTotalSteps = 0;

        private int GetProgressTotalSteps()
        {
            // شمارش بر اساس لیست‌های فعلی همین فایل
            const int standardTablesCount = 79;
            const int customTablesCount = 34;
            const int conditionalTablesCount = 2;
            const int specialTransferCount = 3;   // REMAINDER + PAY_GETD + PAY_GETP
            const int identityTablesCount = 14;
            const int nonCriticalTablesCount = 2; // GENERAL_OPTIONS + USER_PERSONEL_ORDER
            const int initialAndFinalSteps = 7;   // پوشه + prebackup + restore + disable FK + STUFFSK + enable FK + update SAZMAN

            return standardTablesCount
                 + customTablesCount
                 + conditionalTablesCount
                 + specialTransferCount
                 + identityTablesCount
                 + nonCriticalTablesCount
                 + initialAndFinalSteps;
        }

        private void BeginProgress()
        {
            _progressCurrentStep = 0;
            _progressTotalSteps = GetProgressTotalSteps();

            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = "در حال آماده‌سازی عملیات...";
                txtStatus.Foreground = NormalStatusBrush;

                prgBar.Visibility = Visibility.Visible;
                ProgressPanel.Visibility = Visibility.Visible;

                prgLinear.Minimum = 0;
                prgLinear.Maximum = 100;
                prgLinear.Value = 0;

                txtProgressPercent.Text = "0%";
                txtProgressMeta.Text = $"0 از {_progressTotalSteps:N0} مرحله";
                txtRemainingSteps.Text = $"مراحل باقیمانده: {_progressTotalSteps:N0}";
            });
        }

        private void AdvanceProgress(string completedMessage)
        {
            if (_progressTotalSteps <= 0)
            {
                _progressTotalSteps = 1;
            }

            if (_progressCurrentStep < _progressTotalSteps)
            {
                _progressCurrentStep++;
            }

            int percent = (int)Math.Round((_progressCurrentStep * 100.0) / _progressTotalSteps, MidpointRounding.AwayFromZero);
            int remaining = _progressTotalSteps - _progressCurrentStep;

            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = completedMessage;
                txtStatus.Foreground = NormalStatusBrush;

                prgLinear.Value = percent;
                txtProgressPercent.Text = $"{percent}%";
                txtProgressMeta.Text = $"{_progressCurrentStep:N0} از {_progressTotalSteps:N0} مرحله";
                txtRemainingSteps.Text = $"مراحل باقیمانده: {remaining:N0}";
            });
        }

        private void CompleteProgress(string message)
        {
            if (_progressTotalSteps <= 0)
            {
                _progressTotalSteps = 1;
            }

            _progressCurrentStep = _progressTotalSteps;

            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = message;
                txtStatus.Foreground = NormalStatusBrush;

                prgLinear.Value = 100;
                txtProgressPercent.Text = "100%";
                txtProgressMeta.Text = $"{_progressTotalSteps:N0} از {_progressTotalSteps:N0} مرحله";
                txtRemainingSteps.Text = "مراحل باقیمانده: 0";
            });
        }

        private void FailProgress(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = message;
                txtStatus.Foreground = ErrorStatusBrush;
            });
        }

        #endregion

        private void CopyTableAllRowsSmart(string oldDb, string newDb, string tableName)
        {
            string safeOld = QuoteDbName(oldDb);
            string safeNew = QuoteDbName(newDb);

            // خواندن ستون‌های دیتابیس مقصد (جدید)
            string getNewColumnsSql = $@"
SELECT 
    c.name AS ColumnName,
    c.is_identity AS IsIdentity,
    c.column_id AS ColumnOrder
FROM {safeNew}.sys.columns c
INNER JOIN {safeNew}.sys.tables t ON c.object_id = t.object_id
INNER JOIN {safeNew}.sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'dbo'
  AND t.name = @TableName
ORDER BY c.column_id;";

            var newColumns = dbms.DoGetDataSQL<ColumnMetaModel>(getNewColumnsSql, new { TableName = tableName }).ToList();
            if (newColumns == null || newColumns.Count == 0)
                throw new Exception($"ساختار جدول [{tableName}] در دیتابیس مقصد پیدا نشد.");

            // خواندن ستون‌های دیتابیس مبدا (قدیم)
            string getOldColumnsSql = $@"
SELECT 
    c.name AS ColumnName,
    c.is_identity AS IsIdentity,
    c.column_id AS ColumnOrder
FROM {safeOld}.sys.columns c
INNER JOIN {safeOld}.sys.tables t ON c.object_id = t.object_id
INNER JOIN {safeOld}.sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'dbo'
  AND t.name = @TableName
ORDER BY c.column_id;";

            var oldColumns = dbms.DoGetDataSQL<ColumnMetaModel>(getOldColumnsSql, new { TableName = tableName }).ToList();
            if (oldColumns == null || oldColumns.Count == 0)
                throw new Exception($"ساختار جدول [{tableName}] در دیتابیس مبدا پیدا نشد.");

            // پیدا کردن ستون‌های مشترک (فقط ستون‌هایی که در هر دو دیتابیس هستند)
            var oldColumnNames = new HashSet<string>(oldColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);
            var commonColumns = newColumns.Where(c => oldColumnNames.Contains(c.ColumnName)).ToList();

            if (commonColumns.Count == 0)
                throw new Exception($"هیچ ستون مشترکی بین دو دیتابیس برای جدول [{tableName}] پیدا نشد.");

            bool hasIdentity = commonColumns.Any(c => c.IsIdentity);
            string allColumnList = string.Join(", ", commonColumns.Select(c => $"[{c.ColumnName}]"));
            string nonIdentityColumnList = string.Join(", ", commonColumns.Where(c => !c.IsIdentity).Select(c => $"[{c.ColumnName}]"));

            if (!hasIdentity)
            {
                string insertSql = $@"
INSERT INTO {safeNew}.dbo.[{tableName}] ({allColumnList})
SELECT {allColumnList}
FROM {safeOld}.dbo.[{tableName}];";

                dbms.DoExecuteSQL(insertSql);
            }
            else
            {
                string identitySql = $@"
BEGIN TRY
    BEGIN TRAN;

    SET IDENTITY_INSERT {safeNew}.dbo.[{tableName}] ON;

    INSERT INTO {safeNew}.dbo.[{tableName}] ({allColumnList})
    SELECT {allColumnList}
    FROM {safeOld}.dbo.[{tableName}];

    SET IDENTITY_INSERT {safeNew}.dbo.[{tableName}] OFF;

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    BEGIN TRY
        SET IDENTITY_INSERT {safeNew}.dbo.[{tableName}] OFF;
    END TRY
    BEGIN CATCH
        -- Ignore nested error
    END CATCH;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;";

                dbms.DoExecuteSQL(identitySql);
            }
        }

        private string ValidateBeforeCreateNewYear(string newDbName, string newDbDirectory)
        {
            List<string> problems = new List<string>();

            string safeDbName = newDbName?.Trim() ?? string.Empty;
            string safeDirectory = newDbDirectory?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(safeDbName))
            {
                problems.Add("نام دیتابیس جدید مشخص نیست.");
            }

            if (string.IsNullOrWhiteSpace(safeDirectory))
            {
                problems.Add("مسیر ذخیره دیتابیس جدید مشخص نیست.");
            }

            if (problems.Count > 0)
            {
                return "اطلاعات ورودی کامل نیست:\n\n- " + string.Join("\n- ", problems);
            }

            // 1) چک وجود دیتابیس روی SQL Server
            int dbExists = dbms.DoGetDataSQL<int>(
                "SELECT COUNT(1) FROM sys.databases WHERE name = @DbName",
                new { DbName = safeDbName }
            ).FirstOrDefault();

            if (dbExists > 0)
            {
                problems.Add($"دیتابیس [{safeDbName}] از قبل روی SQL Server وجود دارد.");
            }

            // 2) چک فایل‌های فیزیکی محتمل در مسیر مقصد
            string mdfPath = Path.Combine(safeDirectory, safeDbName + "_Data.mdf");
            string ldfPath = Path.Combine(safeDirectory, safeDbName + "_Log.ldf");

            if (Directory.Exists(safeDirectory))
            {
                if (File.Exists(mdfPath))
                {
                    problems.Add($"فایل دیتای دیتابیس از قبل وجود دارد:\n{mdfPath}");
                }

                if (File.Exists(ldfPath))
                {
                    problems.Add($"فایل لاگ دیتابیس از قبل وجود دارد:\n{ldfPath}");
                }

                // آثار احتمالی اجرای ناقص قبلی
                string[] suspiciousFiles = Array.Empty<string>();
                try
                {
                    suspiciousFiles = Directory
                        .GetFiles(safeDirectory, safeDbName + "*", SearchOption.TopDirectoryOnly)
                        .Where(f =>
                            !string.Equals(f, mdfPath, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(f, ldfPath, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }
                catch
                {
                    suspiciousFiles = Array.Empty<string>();
                }

                if (suspiciousFiles.Length > 0)
                {
                    string fileList = string.Join("\n", suspiciousFiles.Take(10));
                    if (suspiciousFiles.Length > 10)
                    {
                        fileList += "\n...";
                    }

                    problems.Add(
                        "در مسیر مقصد فایل‌های مشکوک مرتبط با این نام دیتابیس پیدا شد که می‌تواند نشانه اجرای ناقص قبلی باشد:\n" +
                        fileList
                    );
                }
            }

            // 3) چک ثبت مسیر فایل‌ها در SQL Server (اگر قبلاً attach/restore شده باشند)
            var registeredFiles = dbms.DoGetDataSQL<string>(
                @"SELECT physical_name
          FROM sys.master_files
          WHERE physical_name = @MdfPath OR physical_name = @LdfPath",
                new
                {
                    MdfPath = mdfPath,
                    LdfPath = ldfPath
                }
            ).ToList();

            if (registeredFiles.Any())
            {
                string fileList = string.Join("\n", registeredFiles);
                problems.Add(
                    "مسیر فایل‌های مقصد قبلاً در SQL Server ثبت شده است:\n" + fileList
                );
            }

            if (problems.Count == 0)
            {
                return string.Empty;
            }

            return
                "امکان شروع عملیات ایجاد سال جدید وجود ندارد.\n\n" +
                string.Join("\n\n", problems) +
                "\n\nلطفاً قبل از ادامه، دیتابیس/فایل‌های باقیمانده از اجرای قبلی را بررسی و پاکسازی کنید یا نام/مسیر جدید انتخاب نمایید.";
        }

        private async void Command3_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewDbName.Text) || string.IsNullOrWhiteSpace(YEA.Text) || string.IsNullOrWhiteSpace(txtNewDbPath.Text) || string.IsNullOrWhiteSpace(txtTemplateBackupFile.Text))
            {
                new Msgwin(false, "لطفاً تمامی فیلدها را پر کنید.").ShowDialog();
                return;
            }

            string newDbName = txtNewDbName.Text.Trim();
            string oldDbName = currentDbName;
            string backupFilePath = txtTemplateBackupFile.Text.Trim();
            string newDbDirectory = txtNewDbPath.Text.Trim();
            string currentYearVal = YEA.Text.Trim();

            if (newDbName.Equals(oldDbName, StringComparison.OrdinalIgnoreCase))
            {
                new Msgwin(false, "نمی‌توانید دیتابیس جاری را به عنوان دیتابیس سال جدید بازنویسی کنید!").ShowDialog();
                return;
            }

            if (!File.Exists(backupFilePath))
            {
                new Msgwin(false, "فایل بکاپ خام انتخاب‌شده وجود ندارد یا مسیر آن نامعتبر است.").ShowDialog();
                return;
            }

            string validationMessage = ValidateBeforeCreateNewYear(newDbName, newDbDirectory);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                new Msgwin(false, validationMessage).ShowDialog();
                return;
            }

            Msgwin MSG2 = new Msgwin(true, "آیا اسکریپت اجرا شود ؟"); MSG2.ShowDialog();
            if (MSG2.DialogResult == true)
            {
                ScriptSqly.Migrations.ScriptSqly.LetsGo(CL_CCNNMANAGER.CONNECTION_STR, true);
            }

            Command3.IsEnabled = false;
            BeginProgress();

            try
            {
                await Task.Run(() =>
                {
                    UpdateStatus("در حال آماده‌سازی پوشه دیتابیس جدید...");
                    if (!Directory.Exists(newDbDirectory))
                    {
                        Directory.CreateDirectory(newDbDirectory);
                    }
                    AdvanceProgress("پوشه دیتابیس جدید آماده شد.");

                    UpdateStatus("در حال پشتیبان‌گیری اولیه اطلاعات انبار...");
                    ExecutePreBackupTrTables(oldDbName);
                    AdvanceProgress("پشتیبان‌گیری اولیه اطلاعات انبار انجام شد.");

                    UpdateStatus($"در حال بازیابی دیتابیس خام ({newDbName})...");
                    RestoreDatabaseFromTemplate(newDbName, backupFilePath, newDbDirectory);
                    AdvanceProgress($"بازیابی دیتابیس خام {newDbName} انجام شد.");

                    ExecuteDataTransfers(oldDbName, newDbName, currentYearVal);

                    UpdateStatus("در حال بروزرسانی سال مالی در جدول SAZMAN...");
                    string safeNewDb = QuoteDbName(newDbName);
                    dbms.DoExecuteSQL($"UPDATE {safeNewDb}.dbo.SAZMAN SET YEA = @Year", new { Year = currentYearVal });
                    AdvanceProgress("سال مالی جدید در جدول SAZMAN ثبت شد.");
                });

                CompleteProgress("عملیات با موفقیت به پایان رسید.");
                new Msgwin(false, "عملیات ایجاد سال مالی با موفقیت انجام شد.").ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                FailProgress("خطا در عملیات!");
                new Msgwin(false, "خطای بحرانی در عملیات ایجاد سال:\n" + ex.Message).ShowDialog();
            }
            finally
            {
                Command3.IsEnabled = true;
                prgBar.Visibility = Visibility.Collapsed;
                Mouse.OverrideCursor = null;
            }
        }

        private void ExecutePreBackupTrTables(string currentDb)
        {
            string safeCurrentDb = QuoteDbName(currentDb);

            double upTime = DateTime.Now.ToOADate();
            long upDate = CL_HESABDARI.FARSIDATE();
            string upUser = "System" + (CL_HESABDARI.UCurrentUser()?.ToString() ?? string.Empty);

            // گرفتن نام کامپیوتر و آی‌پی (اگر متدهای CL_HESABDARI در دسترس نبود از System.Net استفاده می‌کنیم)
            string pcName = string.Empty;
            string ipAdd = string.Empty;
            try { pcName = Environment.MachineName; } catch { pcName = "UnknownPC"; }
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var ipInfo = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                ipAdd = ipInfo != null ? ipInfo.ToString() : "127.0.0.1";
            }
            catch { ipAdd = "127.0.0.1"; }

            var parameters = new
            {
                UpTime = upTime,
                UpDate = upDate,
                UpUser = upUser,
                PcName = pcName,
                IpAdd = ipAdd
            };

            // 1. TR_STUF_DEF
            string sqlDef = $@"
                INSERT INTO {safeCurrentDb}.dbo.TR_STUF_DEF 
                (CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, UP_TIME, UP_DATE, UP_USER_NAME, PC_NAME, IPADD) 
                SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, @UpTime, @UpDate, @UpUser, @PcName, @IpAdd 
                FROM {safeCurrentDb}.dbo.STUF_DEF";
            dbms.DoExecuteSQL(sqlDef, parameters);

            // 2. TR_STUF_FSK
            string sqlFsk = $@"
                INSERT INTO {safeCurrentDb}.dbo.TR_STUF_FSK 
                (CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, IDD, POSITION, B_SEF, N_SEF, MIN_M, MAX_M, UP_TIME, UP_DATE) 
                SELECT CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, IDD, POSITION, B_SEF, N_SEF, MIN_M, MAX_M, @UpTime, @UpDate 
                FROM {safeCurrentDb}.dbo.STUF_FSK";
            dbms.DoExecuteSQL(sqlFsk, parameters);
        }

        private void RestoreDatabaseFromTemplate(string newDbName, string backupFilePath, string targetDirectory)
        {
            string getFileListSql = $@"
                IF OBJECT_ID('tempdb..#FileList') IS NOT NULL DROP TABLE #FileList;
                CREATE TABLE #FileList (LogicalName NVARCHAR(128), PhysicalName NVARCHAR(260), Type CHAR(1), FileGroupName NVARCHAR(128), Size NUMERIC(20,0), MaxSize NUMERIC(20,0), FileId BIGINT, CreateLSN NUMERIC(25,0), DropLSN NUMERIC(25,0), UniqueId UNIQUEIDENTIFIER, ReadOnlyLSN NUMERIC(25,0), ReadWriteLSN NUMERIC(25,0), BackupSizeInBytes BIGINT, SourceBlockSize INT, FileGroupId INT, LogGroupGUID UNIQUEIDENTIFIER, DifferentialBaseLSN NUMERIC(25,0), DifferentialBaseGUID UNIQUEIDENTIFIER, IsReadOnly BIT, IsPresent BIT, TDEThumbprint VARBINARY(32), SnapshotUrl NVARCHAR(360));
                
                INSERT INTO #FileList EXEC('RESTORE FILELISTONLY FROM DISK = ''{backupFilePath}''');
                
                SELECT LogicalName, Type FROM #FileList;
            ";

            var fileList = dbms.DoGetDataSQL<BackupFileModel>(getFileListSql).ToList();
            if (fileList.Count == 0) throw new Exception("فایل پشتیبان معتبر نیست یا سرور به مسیر فایل دسترسی ندارد.");

            string dataLogicalName = fileList.FirstOrDefault(f => f.Type == "D")?.LogicalName ?? "TemplateDB_Data";
            string logLogicalName = fileList.FirstOrDefault(f => f.Type == "L")?.LogicalName ?? "TemplateDB_Log";

            string mdfPath = Path.Combine(targetDirectory, newDbName + "_Data.mdf");
            string ldfPath = Path.Combine(targetDirectory, newDbName + "_Log.ldf");

            string restoreSql = $@"
                RESTORE DATABASE {QuoteDbName(newDbName)} 
                FROM DISK = '{backupFilePath}'
                WITH REPLACE,
                     MOVE '{dataLogicalName}' TO '{mdfPath}',
                     MOVE '{logLogicalName}' TO '{ldfPath}',
                     MAXTRANSFERSIZE = 4194304,
                     BUFFERCOUNT = 50,
                     BLOCKSIZE = 65536;

                ALTER DATABASE {QuoteDbName(newDbName)} SET MULTI_USER;
            ";

            dbms.DoExecuteSQL(restoreSql);
        }

        private void ExecuteDataTransfers(string oldDb, string newDb, string newYear)
        {
            string safeOld = QuoteDbName(oldDb);
            string safeNew = QuoteDbName(newDb);

            try
            {
                // 0. خاموش کردن هوشمند تمام کلیدهای خارجی در دیتابیس مقصد
                UpdateStatus("در حال آماده‌سازی دیتابیس مقصد و غیرفعال‌سازی موقت کلیدهای خارجی...");
                string disableFkSql = $@"
                USE {safeNew};
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL; '
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id;
                EXEC sp_executesql @disableSql;";
                dbms.DoExecuteSQL(disableFkSql);
                AdvanceProgress("کلیدهای خارجی دیتابیس مقصد موقتاً غیرفعال شدند.");


                // گروه ۱: جداول استاندارد (TCOD_OSTAN از این لیست حذف شد چون در VBA کامنت بود - BLOCKNON_HES در VBA اجرا می‌شود و باید باشد)
                string[] standardTables = {
                "TFORMS", "sal_CHEK", "TCOD_HESKIND", "TCOD_HESGROUP", "TCOD_HESVAZ", "tota_hes", "DEta_hes",
                "TCOD_ANBAR_KIND", "TCOD_DPS", "TCODE_MADRAK", "TCOD_DPSKIND", "TCOD_STUFGROUP", "TCOD_VAHEDS",
                "STUF_STK", "MODULE_D", "HEAD_MANF", "DTL_MANF", "ROOM", "SHIFT", "SUD", "SURAT_MALI_HEAD", "SURAT_MALI",
                "TAGCOD", "PERSONEL", "PSANDUGH_KARKONAN", "TARAZHES", "CUSTKIND", "TAKHPERS", "TALARS", "PENDJOB",
                "FPFILEDS", "FPFORMAT", "FPFORMATFILELD", "COD_HESAB", "FORMAT", "FORMAT_HESAB", "TCOD_ZARMALIAT",
                "DAFT_ASN", "PKINDCODING", "PTAMIRAT", "BLOCK_CUSTOMER", "BLOCK_HES", "BLOCKNON_HES", "TCOD_MAP",
                "TCOD_MAP_GRP", "TCOD_MARKAZHAZ", "Travelreason", "Relations", "Job", "Countries",
                "Locations", "tblAmaken", "desks", "TCODE_MENUITEM", "DARSAD_TAKHFIF", "Visit_route", "user_print",
                "SMS_FORMATS", "VISITORS_PORSANT", "VISITORS_PORSANT_KALA", "TAKHFIF_DEF", "TAKHFIF_DEF_DTL", "amval",
                "PRICE_GRP", "PRICE_LIST", "PRICE_PAYNO", "PRICE_ELAMIE", "PRICE_ELAMIE_DTL", "PRICE_ELAMIETF",
                "PRICE_ELAMIETF_DTL", "BUGET_DEFAULT", "USEROPTION", "SSM_CUST_ANALYS", "SSM_CUST_RASPAY", "TCOD_Countries",
                "ENHESAR_MOSHTARI", "ENHESAR_KALA", "MORINFO", "MORINFO_SUB" , "PRICE_ELAMIETF_EXCEPTION"
            };

                //foreach (string tbl in standardTables)
                //{
                //    UpdateStatus($"انتقال کامل جدول {tbl}...");
                //    dbms.DoExecuteSQL($"INSERT INTO {safeNew}.dbo.[{tbl}] SELECT * FROM {safeOld}.dbo.[{tbl}]");
                //}

                foreach (string tbl in standardTables)
                {
                    UpdateStatus($"انتقال کامل جدول {tbl}...");
                    CopyTableAllRowsSmart(oldDb, newDb, tbl);
                    AdvanceProgress($"جدول {tbl} با موفقیت منتقل شد.");
                }


                // جداول غیر حیاتی (در صورت خطا از آنها چشم پوشی می‌شود)
                string[] nonCriticalTables = { "GENERAL_OPTIONS", "USER_PERSONEL_ORDER" };
                foreach (string tbl in nonCriticalTables)
                {
                    try
                    {
                        UpdateStatus($"انتقال جدول (غیر حیاتی) {tbl}...");
                        CopyTableAllRowsSmart(oldDb, newDb, tbl);
                    }
                    catch
                    {
                        // خطا نادیده گرفته می‌شود
                    }
                    finally
                    {
                        AdvanceProgress($"جدول {tbl} با موفقیت منتقل شد.");
                    }
                }

                // گروه ۲: جداول نیازمند تعریف دقیق ستون‌ها
                var customSelects = new Dictionary<string, string>
                {
                    { "SAZMAN", "UNIVERSITY_CO, NAME, CITY, MANAGER, MOAVEN, ZIHESAB, AMINAMVAL, SANAD, GHAYM, KALA, PERSON, DIG, WAR, LST, TFTPAGE, TFSAZMAN, TFADDRESS, TFTEL, TFCODE_E, WIDTH_D, HIGH_D, CPI, SANDOGH, BANKHA, BESTANKAR, BEDEHKAR, KHARID, MKHARID, TKHARID, HKHARID, FROSH, MFROSH, TFROSH, HFROSH, MOGODIA, MOGODIP, DARAM, HDARAM, HKOL, ADA, APA, ADV, HAVALAH, CTRL_TS, F_ANBARF, GH_PK, L_NUMBER, SF_G, TAR_KM, BACKPATH, TKHF, HAZ_TOL, PJHAZ_TOL1, PHAZ_TOL, GHEYMAT, PPDAST, PPSAR, AMALKARD, PERSONEL, PERVAM, CONKAL, EMZA, HNAH, HEZA, HPAD, HOLA, HKHA, HJAZ, HRAN, HSAY, HCON, HSHI, HAZEDAR, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, SAGHFH, MAND, MOJU, SA_HOGH, SA_40EZ, SA_EZAF, SA_PADA, SA_HOLA, SA_KHAR, SA_NAHA, SA_JAZB, SA_RAND, SA_COND, SA_SAYE, SA_23BI, HAZTOLID, HAZFROOSH, HAZKHADAMAT, PISHDAR, DEFANB, DEFTKH, ECONM, FRUP, UPDDATE, FINALS, PSANDHES, SANAVP, BON, ISO_FROOSH, ISO_KHAREED, ISO_MAVAD, ISO_TOLID, ISO_MAVADSAYER, SANAT, CODEVIEW, PKHARID, SIGN, BARCOD, SAGHF, SERVERNAM, TENDAR, LECOL1, LECOL2, LECOL3, LECOL4, LKCOL1, HESMBAA, ECODE, PCODE, IYALAT, MCODEM, HPOR, SAGHF2, OPTIONSS, CTL_DT, LOCKFAP, LOCKFSI, TRANSF, OKF, ARSESH, RMOG, APV, HOTCOD, STFR, STKH, STHFR, STHKH, STENT, STKHS, STKHH, STTOL, STFRB, STBKH, STMO, STKHA, SNDKH, SMS_USERNAME, SMS_PASSWORD, SMS_LIBKEY, SMS_TSMSHOST, SMS_ProxyUserName, SMS_ProxyPassword, SMS_ProxyServer, SMS_ProxyPort, SMS_FirewallUserName, SMS_FirewallPassword, SMS_FirewallHost, SMS_FirewallPort, SMS_FirewallType, DSMS, SMS_OWNER, PRMFR, SMSACT, HESDESK, ISO_DTOLID, SERFACB, HBON, pishpross, version, hesnaghd, IT1, IT2, IT3, IT4, IT5, IT6, IT7, IT8, IT9, IS1, IS2, IS3, IS4, IS5, IS6, IS7, IS8, IS9, IS10, IS11, IS12, IS13, IS14, IS15, IS16, SSMTRTAKM, SSMTRTAGM, SSMSNDAUTO, SSMTBMON, SSMDARSAD, HDARKASRTAKHF, PUBLICKEY, PRIVIATEKEY, MEMORYID, MEMORYIDsand, Dcertificate, SMSTYPE, CRT, UID" },
                    { "salA_dtl", "SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD, HES, PORID, EMZA, menup" },
                    { "TDETA_HES", "N_KOL, NUMBER, TNUMBER, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID,tob" },
                    { "TDETA_HES2", "N_KOL, NUMBER, TNUMBER, TNUMBER2, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID,tob" },
                    { "TDETA_HES3", "N_KOL, NUMBER, TNUMBER, TNUMBER2, TNUMBER3, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID,tob" },
                    { "TDETA_HES4", "N_KOL, NUMBER, TNUMBER, TNUMBER2, TNUMBER3, TNUMBER4, NAME, TOZIH, BED_BES, ADDRESS, TEL, CODE_E, ECODE, PCODE, IYALAT, CITY, MCODEM, CUST_COD, MOBILE, ROUTE_NAME, Longitude, Latitude, OSTANID, SHAHRID,tob" },
                    { "TCOD_ANBAR", "CODE, NAMES, KIND" },
                    { "TCOD_BANKS", "CODE, NAMES, TEJ_C, MEL_C, SAD_C, MLA_C, REF_C, TOS_C, KES_C, KAR_C, POS_C, TAT_C, SEP_C, TSA_C, SAN_C, MAS_C, EGH_C, PAR_C, PAS_C, DEY_C, SAM_C, SAR_C, SIN_C, SHA_C" },
                    { "STUF_DEF", "CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE,sstid,mu,vra" },
                    { "DEPART", "DEPATMAN, DEPNAME, DEPART, PCODE,BBC" },
                    { "SHARH", "SHARH" },
                    { "OPANBACCESS", "USERCO, ANBCO" },
                    { "SIGN_TRAN", "NUMBER, TAG, SGN_ID, SGN_USER_NAME, SGN_DATE, SGN_TIME" },
                    { "AZAE", "HES, TOPETEB, TOPASN, ENDIS, TOPMEGH, JAMZARIB, EMTIAZ, gradeid, DTTIM" },
                    { "PTVAZ", "PIDT, PVAZ, PDATE, PTIME" },
                    { "SQLSTATE", "SQLST, TITEL, USER_NAME, LETOTHER, CR_DATE" },
                    { "Visit_route_dtl", "ROUTE_NAME, COUST_NO, RACTIVE, CLASS" },
                    { "tab_job", "Job_Code, Job_Desc" },
                    { "CHARTSAZMANI", "USERCO, SUBUSERCO" },
                    { "telef", "name, TEL" },
                    { "DEFAULTDEP", "TFSAZMAN, SHIFT, USERID" },
                    { "GRADE_FORMAT", "IDD, GFNAME, GFDATE, TOZIH, USERNAME, JAMZARIB, EMTIAZ" },
                    { "GRADE_TAB_FT", "GFID, GFTID, GFNAMEFT, GFGZARIB" },
                    { "GSCALE", "GSCACOD, GSCANAME, GSCAKIND" },
                    { "GSCADTL", "GSCADTCOD, GSCANAME, GSCAGRADE, GSCAFROM, GSCATO, GSCACOD" },
                    { "GRADE_GRP_FT", "GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE" },
                    { "GRADE_CUST_TAB", "GCTABID, GCNAME, GCZARIB, GCCUST_HES, GCDATE, USERNAME" },
                    { "GRADE_CUST_GRP", "GCGRPID, GCGRPNAME, GCGRPZARIB, GCGRPGRADE, GCTABID, GCVALUE, GCVALUESCAL" },
                    { "GRADE_RANGE", "GID, GNAME, GAZ, GTA, GTOZIH" },
                    { "GRADE_SHART_FUNC", "GSHARTID, GSHNAME, GSHFUNC, TOZIH" },
                    { "GRADE_SHART", "GSID, GID, GSHARTID, GSVALUE, GSVALUE2, GSVALUE3, TOZI" },
                    { "TCOD_CITY", "OSCODE, CITYCODE, CITYNAME" },
                    { "sudmah", "DATE_S, BEDS, BESS, sudday, SUD" },

                    //SIGN:
                    { "SIGN", "USERCO, FFR_FROOSH, FFR_HESAB, FFR_MODIR, KFR_BAZAR, KFR_HESAB, KFR_MODIR, ANB_RASID_ANB, ANB_RASID_QC," +
                    " ANB_HAVL_FROSH, ANB_HAVL_ANB, ANB_HAVL_MODIR, ANB_TOLID_MODIRT, ANB_TOLID_QC, ANB_TOLID_ANB, ANB_KHOROG_ANB, ANB_KHOROG_MODIRT," +
                    " ANB_KHOROGS_ANB, ANB_KHOROGS_MODIRT, SND_TAHI, SND_MALI, SND_MODIR, FFRB_FROOSH, FFRB_ANB, FFRB_HESAB, FFRB_MODIR, KFRB_BAZAR," +
                    " KFRB_ANB, KFRB_HESAB, KFRB_MODIR, FFRP_FROOSH, FFRP_ANB, FFRP_HESAB, FFRP_MODIR, RASM_HESAB, RASM_MODIR, PAY_MVAHED, PAY_HESAB, " +
                    "PAY_MAMEL, ENTGH_AZANB, ENTGH_BEANB, ENTGH_MODIR, PAZ_PAZ, PAZ_HES, PAZ_CEO, SGN0124, SGN0224, SGN0324, SGN0126, SGN0226, SGN0326," +
                    " SGN0133, SGN0233, SGN0333, SGN0132, SGN0232, SGN0332, SGN0134, SGN0234, SGN0334, SGN0135, SGN0235, SGN0335, SGN0136, SGN0236, SGN0336," +
                    " FFR_FROOSHTX, FFR_HESABTX, FFR_MODIRTX, KFR_BAZARTX, KFR_HESABTX, KFR_MODIRTX, ANB_RASID_ANBTX, ANB_RASID_QCTX, ANB_HAVL_FROSHTX, " +
                    "ANB_HAVL_ANBTX, ANB_HAVL_MODIRTX, ANB_TOLID_MODIRTTX, ANB_TOLID_QCTX, ANB_TOLID_ANBTX, ANB_KHOROG_ANBTX, ANB_KHOROGS_ANBTX ,ANB_KHOROG_MODIRTTX," +
                    " ANB_KHOROGS_MODIRTTX, SND_TAHITX, SND_MALITX, SND_MODIRTX, FFRB_FROOSHTX, FFRB_ANBTX, FFRB_HESABTX, FFRB_MODIRTX," +
                    " KFRB_BAZARTX, KFRB_ANBTX, KFRB_HESABTX, KFRB_MODIRTX, FFRP_FROOSHTX, FFRP_ANBTX, FFRP_HESABTX, FFRP_MODIRTX, RASM_HESABTX, " +
                    "RASM_MODIRTX, PAY_MVAHEDTX, PAY_HESABTX, PAY_MAMELTX, ENTGH_AZANBTX, ENTGH_BEANBTX, ENTGH_MODIRTX, PAZ_PAZTX, PAZ_HESTX, PAZ_CEOTX," +
                    " SGN0124TX, SGN0224TX, SGN0324TX, SGN0126TX, SGN0226TX, SGN0326TX, SGN0133TX, SGN0233TX, SGN0333TX, SGN0132TX, SGN0232TX, SGN0332TX," +
                    " SGN0134TX, SGN0234TX, SGN0334TX, SGN0135TX, SGN0235TX, SGN0335TX, SGN0136TX, SGN0236TX, SGN0336TX, SGN0137, SGN0237, SGN0337," +
                    " SGN0137TX, SGN0237TX, SGN0337TX, SGN0140, SGN0240, SGN0340, SGN0140TX, SGN0240TX, SGN0340TX, SGN0141, SGN0241, SGN0341, SGN0141TX, " +
                    "SGN0241TX, SGN0341TX, SGN0142, SGN0242, SGN0342, SGN0142TX, SGN0242TX, SGN0342TX, CRT, UID" }
                };

                foreach (var kvp in customSelects)
                {
                    UpdateStatus($"انتقال ساختار جدول {kvp.Key}...");
                    if (kvp.Key == "SAZMAN")
                        dbms.DoExecuteSQL($"INSERT INTO {safeNew}.dbo.SAZMAN ({kvp.Value}, YEA) SELECT {kvp.Value}, {newYear} FROM {safeOld}.dbo.SAZMAN");
                    else
                        dbms.DoExecuteSQL($"INSERT INTO {safeNew}.dbo.[{kvp.Key}] ({kvp.Value}) SELECT {kvp.Value} FROM {safeOld}.dbo.[{kvp.Key}]");

                    AdvanceProgress($"جدول {kvp.Key} با موفقیت منتقل شد.");
                }

                // گروه ۳: جداول شرطی
                var conditionalTables = new Dictionary<string, string>
                {
                    { "MEHMAN", "KDATE IS NULL" },
                    { "PAZIRESH", "ENDIT = 1" }
                };

                foreach (var kvp in conditionalTables)
                {
                    UpdateStatus($"انتقال شرطی جدول {kvp.Key}...");
                    dbms.DoExecuteSQL($"INSERT INTO {safeNew}.dbo.[{kvp.Key}] SELECT * FROM {safeOld}.dbo.[{kvp.Key}] WHERE {kvp.Value}");

                    AdvanceProgress($"انتقال شرطی جدول {kvp.Key} انجام شد.");
                }

                // --- اصلاحیه مهم: تعیین دقیق ۱۰ فیلد برای جدول REMAINDER ---
                UpdateStatus($"انتقال پیام‌های REMAINDER...");
                dbms.DoExecuteSQL($@"
                INSERT INTO {safeNew}.dbo.REMAINDER (PERSONEL, PAYAM, STATUS, CTDATE, CTTIME, USERNAME, COMP_COD, STDATE, STTIME, SMSOK) 
                SELECT PERSONEL, PAYAM, STATUS, CTDATE, CTTIME, USERNAME, COMP_COD, STDATE, STTIME, SMSOK 
                FROM {safeOld}.dbo.REMAINDER WHERE STATUS = 1
            ");
                AdvanceProgress("پیام‌های REMAINDER منتقل شد.");

                // --- اصلاحیه مهم: مپینگ دقیق فیلدهای NULL AS Expr6 (NUMBER) و NULL AS Expr7 (TAG) برای اسناد ---
                UpdateStatus($"انتقال اسناد خزانه‌داری...");
                dbms.DoExecuteSQL($@"
                INSERT INTO {safeNew}.dbo.PAY_GETD (N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3,N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ,LIST_NO,KIND,SANDUGH,HES1,HES2,HES3,ESTELAM,SAYADI) 
                SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NULL, NULL, ANBAR, RADIF, CUST_NO, VAZ,LIST_NO,KIND,SANDUGH,HES1,HES2,HES3,ESTELAM,SAYADI 
                FROM {safeOld}.dbo.PAY_GETD 
                WHERE ((N_KOL = (SELECT TOP 1 BANKHA FROM {safeOld}.dbo.SAZMAN) AND N_KOL <> 911) OR N_KOL IS NULL) 
                  AND N_KOL2 IS NULL AND N_MOIN2 IS NULL AND N_TAF2 IS NULL AND N_KOL3 IS NULL AND N_MOIN3 IS NULL AND N_TAF3 IS NULL
            ");

                dbms.DoExecuteSQL($@"
                INSERT INTO {safeNew}.dbo.PAY_GETP (N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO,HES1,HES2,HES3,SAYADI) 
                SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NULL, NULL, ANBAR, NULL, CUST_NO,HES1,HES2,HES3,SAYADI 
                FROM {safeOld}.dbo.PAY_GETP 
                WHERE N_KOL2 IS NULL AND N_KOL <> 911 AND N_MOIN2 IS NULL AND N_TAF2 IS NULL AND N_KOL3 IS NULL AND N_MOIN3 IS NULL AND N_TAF3 IS NULL
            ");
                AdvanceProgress("اسناد PAY_GETD منتقل شد.");

                // گروه ۴: جداول کلید خودکار (IDENTITY TABLES)
                var identityTables = new Dictionary<string, string>
                {
                    { "COPMANES", "id, COMPANY_NAME, CITY, MANAGER, FACT_TEL, MOBILE, PERNUM, STATUS_FACT, PRODUCTS, ADDR, ACCOUNTANT, SOFTWARE, ESP_PERSON, REAGENT, STATUS, COMMENT, date_sabt, USER_NAME, pic, dt, userid, Longitude, Latitude, OSTANID, SHAHRID, CRT, UID" },
                    { "TASKS", "IDNUM, GR, PERSONEL, TASK, PERIORITY, STATUS, STDATE, STTIME, ENDATE, ENTIME, USERNAME, COMP_COD, SUMTIME, pic, ss, skid, num, tg, CTIM, USERCO, SEE, SEET, CRT, UID" },
                    { "EVENTS", "IDNUM, IDD, EVENTS, STDATE, STTIME, USERNAME, COMPANY, SUMTIME, pic, skid, num, tg, CRT, UID" },
                    { "PM_location", "IDD, LO_NAME, LO_IDD, CR, US, CRT, UID" },
                    { "PM_ASSETS", "IDD, AS_NAME, AS_SERIAL, AS_MODEL, AS_GRP, AS_Longitude, AS_Latitude, AS_IDDMAIN, AS_IDAMVAL, AS_IMAGE, AS_HESAB, AS_GARANTI, AS_GARANTICOMP, AS_LOCATION, AS_STATUS, AS_CODING, SGN1, SGN2, SGN3, SGN4, sgn1usid, sgn2usid, sgn3usid, CR, US, CRT, UID" },
                    { "PM_RUMAINTE", "IDD, AS_IDD, RM_RSANGESH, RM_PERIOD, RM_MEGHDAR, RM_PERSON, RM_JOB, RM_STDATE, RM_STTIME, RM_TAKHIRMO, RM_TAKHVAHED, RM_ELAMBE, TM_TIMENEED, TM_HESAB_HAZINE, CR, US, CRT, UID" },
                    { "PM_EM_FAILURE", "IDD, FI_NAME, CR, US, CRT, UID" },
                    { "PM_EM_FAILURE_EVENTS", "IDD, FI_IDD, FIO_JOB, AS_IDD, FIO_DATE, FIO_TIME, FIO_PERIO, FIO_AS_VAS, FIO_DESCRIPTION, FIO_ZAMEM, FIO_EVKIND, FIO_JOBIDD, SGN1, SGN2, SGN3, SGN4, sgn1usid, sgn2usid, sgn3usid, CR, US, CRT, UID" },
                    { "PM_JOB", "IDD, RM_IDD, AS_IDD, JO_JOB, JO_RSANGESH, JO_PERIOD, JO_MEGHDAR, JO_PERSON, JO_STDATE, JO_STTIME, JO_DONE, JO_ENDATE, JO_ENTIME, JO_HESAB_HAZINE, JO_LOCATION, JO_TAKHIRMO, JO_TAKHVAHED, JO_ELAMBE, JO_TIMENEED, JO_KIND, CR, US, CRT, UID" },
                    { "PM_JOBTITLE", "IDD, JOB_TITLE, CR, US, CRT, UID" },
                    { "PM_MVAD", "IDD, JOB_IDD, MV_ANBAR, MV_CODE, MV_MEGH, MV_MEGHk, MV_VAHED_K, MV_DESCRIPTION, CR, US, CRT, UID" },
                    { "PM_MVT", "IDD, RM_IDD, MV_CODE, MV_MEGH, MV_MEGHk, MV_VAHED_K, MV_DESCRIPTION, CR, US, CRT, UID" },
                    { "crmevents", "idde, COMPANY_NAME, INFO_DATE, INFO_TIME, SALER, BUYER, COMMENT, NEXT_DATE, NEXT_TIME, STATUS, pic, idc, PAYAM, miting, USERID, CDATETI, CRT, UID" },
                    { "Notes", "idd, Note, Ndate, Ntime, userid, Ndone, CRT, UID" }
                };

                foreach (var kvp in identityTables)
                {
                    UpdateStatus($"انتقال امن جدول Identity {kvp.Key}...");
                    string whereClause = "";

                    if (kvp.Key == "TASKS") whereClause = " WHERE STATUS = 1 AND (NUM IS NULL OR num = 101)";
                    if (kvp.Key == "EVENTS") whereClause = $" WHERE IDNUM IN (SELECT IDNUM FROM {safeOld}.dbo.TASKS WHERE STATUS = 1 AND (NUM IS NULL OR num = 101))";

                    string identitySql = $@"
                    BEGIN TRY
                        BEGIN TRAN;
                        SET IDENTITY_INSERT {safeNew}.dbo.[{kvp.Key}] ON;
                        
                        INSERT INTO {safeNew}.dbo.[{kvp.Key}] ({kvp.Value}) 
                        SELECT {kvp.Value} FROM {safeOld}.dbo.[{kvp.Key}] {whereClause};
                        
                        SET IDENTITY_INSERT {safeNew}.dbo.[{kvp.Key}] OFF;
                        COMMIT;
                    END TRY
                    BEGIN CATCH
                        IF OBJECT_ID('{safeNew}.dbo.[{kvp.Key}]') IS NOT NULL
                            SET IDENTITY_INSERT {safeNew}.dbo.[{kvp.Key}] OFF;
                        
                        IF @@TRANCOUNT > 0 ROLLBACK;
                        THROW;
                    END CATCH;
                ";
                    dbms.DoExecuteSQL(identitySql);

                    AdvanceProgress($"جدول Identity {kvp.Key} منتقل شد.");
                }

                //-- پر کردن جدول کدینگ استان در صورت خالی بودن:
                EnsureTcodOstanExists(safeNew);

                // --- اصلاحیه مهم: اصلاح پارامترهای lastavrage به شکل (AK.CODE, A.CODE) ---
                UpdateStatus("انتقال و محاسبه اولیه موجودی‌های انبار...");
                string stuffSql = $@"
                IF OBJECT_ID('{safeNew}.dbo.STUFFSK', 'U') IS NOT NULL DROP TABLE {safeNew}.dbo.STUFFSK;
                
                SELECT 
                    AK.ANBAR, 
                    AK.CODE, 
                    AK.MAND, 
                    ISNULL(({safeOld}.dbo.lastavrage(AK.CODE, AK.ANBAR, 99999999)), 0) AS FII
                INTO #TempSTUFFSK 
                FROM {safeOld}.dbo.TCOD_ANBAR AS A 
                CROSS APPLY {safeOld}.dbo.AKMOGUDI_KOL_ANBAR_ONE(99999999, A.CODE) AS AK;

                SELECT 
                    ANBAR, 
                    CODE, 
                    MAND, 
                    FII, 
                    CAST((MAND * FII) AS BIGINT) AS MABLK 
                INTO {safeNew}.dbo.STUFFSK
                FROM #TempSTUFFSK;
                
                INSERT INTO {safeNew}.dbo.STUF_FSK (CODE, MOGODI_A, FI_A, MABL_A, ANBAR, MANDAH_A, VAZ, POSITION, B_SEF, N_SEF, MIN_M, MAX_M)
                SELECT S.CODE, S.MAND, S.FII, S.MABLK, S.ANBAR, 0 AS MANDAH_A, F.VAZ, F.POSITION, F.B_SEF, F.N_SEF, F.MIN_M, F.MAX_M
                FROM {safeNew}.dbo.STUFFSK S
                INNER JOIN {safeOld}.dbo.STUF_FSK F ON S.CODE = F.CODE AND S.ANBAR = F.ANBAR;
            ";
                dbms.DoExecuteSQL(stuffSql);
                AdvanceProgress("موجودی‌های اولیه انبار محاسبه و ثبت شد.");

                UpdateStatus("انتقال جداول قیمت‌گذاری و آپدیت قیمت انبار...");
                dbms.DoExecuteSQL($@"
                IF OBJECT_ID('{safeNew}.dbo.GHEYMAT', 'U') IS NOT NULL DROP TABLE {safeNew}.dbo.GHEYMAT;
                SELECT CODE, GHEMAT INTO {safeNew}.dbo.GHEYMAT FROM {safeOld}.dbo.GHEYMAT_TAMAM;
            ");

                UpdateStatus("محاسبه ارزش ریالی کالاهای موجود...");
                string updateFskPricesSql = $@"
                UPDATE F
                SET 
                    F.FI_A = G.GHEMAT,
                    F.MABL_A = G.GHEMAT * F.MOGODI_A
                FROM {safeNew}.dbo.STUF_FSK F
                INNER JOIN {safeNew}.dbo.GHEYMAT G ON F.CODE = G.CODE
                WHERE F.MOGODI_A > 0;
            ";
                dbms.DoExecuteSQL(updateFskPricesSql);

                UpdateStatus("انتقال و تسویه مانده وام‌ها...");
                string pvamSql = $@"
                INSERT INTO {safeNew}.dbo.PVAM (CODE, VAM_ID, SHARH, DATE_BP, NUM, MABL, MABLBZ)
                SELECT P.CODE, P.VAM_ID, P.SHARH, P.DATE_BP, P.NUM, P.MABL, P.MABLBZ
                FROM {safeOld}.dbo.PVAM P
                LEFT JOIN {safeOld}.dbo.PVAM_BAZ PB ON P.CODE = PB.CODE AND P.VAM_ID = PB.VAM_ID
                GROUP BY P.CODE, P.VAM_ID, P.SHARH, P.DATE_BP, P.NUM, P.MABL, P.MABLBZ
                HAVING (P.MABL <> SUM(ISNULL(PB.MABL, 0))) OR (SUM(ISNULL(PB.MABL, 0)) IS NULL);

                INSERT INTO {safeNew}.dbo.PVAM_BAZ
                SELECT PB.* FROM {safeOld}.dbo.PVAM_BAZ PB
                INNER JOIN {safeNew}.dbo.PVAM PNEW ON PB.CODE = PNEW.CODE AND PB.VAM_ID = PNEW.VAM_ID;
            ";
                dbms.DoExecuteSQL(pvamSql);
            }
            finally
            {
                // 2. روشن کردن مجدد کلیدهای خارجی به صورت هوشمند و بدون بروز خطای دیتای قدیمی (Legacy Check)
                UpdateStatus("در حال اعمال مجدد قوانین کلیدهای خارجی و یکپارچه‌سازی پایگاه داده...");
                string enableFkSql = $@"
                        USE {safeNew};
                        DECLARE @enableSql NVARCHAR(MAX) = N'';
                        -- استفاده از WITH NOCHECK برای جلوگیری از توقف عملیات به دلیل وجود داده‌های کثیف قدیمی (Orphaned records)
                        SELECT @enableSql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH NOCHECK CHECK CONSTRAINT ALL; '
                        FROM sys.tables t
                        JOIN sys.schemas s ON t.schema_id = s.schema_id;
                        EXEC sp_executesql @enableSql;
                    ";
                dbms.DoExecuteSQL(enableFkSql);
                AdvanceProgress("قوانین کلیدهای خارجی مجدداً اعمال شد.");

                UpdateStatus("انتقال با موفقیت به پایان رسید.");
            }
        }

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = message;
            });
        }

        private void EnsureTcodOstanExists(string safeNew)
        {
            try
            {
                dbms.DoExecuteSQL($@"
        IF NOT EXISTS (SELECT 1 FROM {safeNew}.dbo.TCOD_OSTAN)
        BEGIN
            INSERT INTO {safeNew}.dbo.TCOD_OSTAN (OSCODE, OSNAME)
            SELECT V.OSCODE, V.OSNAME
            FROM (VALUES
                ( 1,   N'آذربايجان شرقي' ),
                ( 2,   N'آذربايجان غربي' ),
                ( 3,   N'اردبيل' ),
                ( 4,   N'اصفهان' ),
                ( 5,   N'ايلام' ),
                ( 6,   N'بوشهر' ),
                ( 7,   N'تهران' ),
                ( 8,   N'چهارمحال و بختياري' ),
                ( 9,   N'خراسان جنوبي' ),
                ( 10,  N'خراسان رضوي' ),
                ( 11,  N'خراسان شمالي' ),
                ( 12,  N'خوزستان' ),
                ( 13,  N'زنجان' ),
                ( 14,  N'سمنان' ),
                ( 15,  N'سيستان و بلوچستان' ),
                ( 16,  N'فارس' ),
                ( 17,  N'قزوين' ),
                ( 18,  N'قم' ),
                ( 19,  N'کردستان' ),
                ( 20,  N'کرمان' ),
                ( 21,  N'کرمانشاه' ),
                ( 22,  N'کهگيلويه و بويراحمد' ),
                ( 23,  N'گلستان' ),
                ( 24,  N'گيلان' ),
                ( 25,  N'لرستان' ),
                ( 26,  N'مازندران' ),
                ( 27,  N'مرکزي' ),
                ( 28,  N'هرمزگان' ),
                ( 29,  N'همدان' ),
                ( 30,  N'يزد' ),
                ( 31,  N'البرز' ),
                ( 92,  N'پاکستان' ),
                ( 93,  N'افغانستان' ),
                ( 964, N'عراق' ),
                ( 965, N'کویت' ),
                ( 968, N'عمان' ),
                ( 971, N'امارات' )
            ) AS V(OSCODE, OSNAME)
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM {safeNew}.dbo.TCOD_OSTAN T
                WHERE T.OSCODE = V.OSCODE
            );
        END
    ");
            }
            catch { }
        }


        private void Command4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static string QuoteDbName(string dbName)
        {
            return $"[{dbName.Replace("]", "]]", StringComparison.Ordinal)}]";
        }

        private void BTN_BROWSE_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "*.bak|*.bak",
                Title = "neginsql.bak را انتخاب کنید",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    txtTemplateBackupFile.Text = openFileDialog.FileName;
                }
                else
                {
                    universControl.PopNotifyShowUp($"فایل معتبر انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
            }
        }

        private void BTN_BROWSEFOLDER_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "پوشه مورد نظر را انتخاب کنید",
                Multiselect = false
            };

            // بررسی اینکه آیا تکست‌باکس مقداری دارد و آیا آن مسیر واقعاً روی سیستم وجود دارد
            string currentPath = txtNewDbPath.Text.Trim();
            if (!string.IsNullOrWhiteSpace(currentPath) && System.IO.Directory.Exists(currentPath))
            {
                openFolderDialog.InitialDirectory = currentPath;
            }

            if (openFolderDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(openFolderDialog.FolderName))
                {
                    string selectedPath = openFolderDialog.FolderName;

                    // بررسی وجود بک‌اسلش در انتهای مسیر و اضافه کردن آن در صورت نیاز
                    if (!selectedPath.EndsWith(@"\"))
                    {
                        selectedPath += @"\";
                    }

                    txtNewDbPath.Text = selectedPath;
                }
                else
                {
                    universControl.PopNotifyShowUp($"پوشه معتبر انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                }
            }
        }

    }
}
