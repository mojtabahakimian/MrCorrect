using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Dapper;
using Microsoft.Data.SqlClient;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for WIN_GETFIRSTMOG.xaml
    /// </summary>
    public partial class WIN_GETFIRSTMOG : Window
    {
        public WIN_GETFIRSTMOG()
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
        private readonly List<string> validDbNames = new List<string>();

        private class DB_NAME_MODEL
        {
            public string name { get; set; }
        }

        UniversControl universControl = new UniversControl();

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

            try
            {
                // Preserve legacy security and initializations
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "DMSGH", new WindowInteropHelper(this).Handle, this.GetType().Name);

                LoadYearSources();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری فرم:\n" + ex.Message).ShowDialog();
                this.Close();
            }

            FILL_ALL_COMBOBOXES();
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

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }
            return true;
        }
        private void LoadYearSources()
        {
            YEA.Items.Clear();
            validDbNames.Clear();

            // Fetch only user databases (exclude system databases: master/model/msdb/tempdb)
            var databases = dbms.DoGetDataSQL<DB_NAME_MODEL>(@"
                SELECT [name]
                FROM sys.databases
                WHERE database_id > 4
                  AND [state] = 0
                ORDER BY [name]").ToList();


            foreach (var db in databases)
            {
                if (string.IsNullOrWhiteSpace(db.name))
                    continue;

                string dbName = db.name.Trim();
                string safeDbName = QuoteDbName(dbName);

                try
                {
                    // Check if the database has the required 'SAZMAN' table (Legacy logic adaptation)
                    var hasSazman = dbms.DoGetDataSQL<int>($"SELECT TOP 1 1 FROM {safeDbName}.dbo.SAZMAN").FirstOrDefault();
                    if (hasSazman == 1)
                    {
                        YEA.Items.Add(dbName);
                        validDbNames.Add(dbName); // Store in-memory for security validation later
                    }
                }
                catch
                {
                    // Ignore inaccessible or irrelevant databases silently (like original 'On Error Resume Next')
                }
            }

            if (YEA.Items.Count > 0)
            {
                YEA.SelectedIndex = 0;
            }
        }

        private async void Command3_Click(object sender, RoutedEventArgs e)
        {
            if (YEA.SelectedItem == null)
            {
                new Msgwin(false, "لطفاً سال مالی را انتخاب کنید.").ShowDialog();
                return;
            }

            string selectedDb = YEA.SelectedItem.ToString() ?? string.Empty;

            // Security Check: Ensure selected DB is within our verified list to prevent injection
            if (string.IsNullOrWhiteSpace(selectedDb) || !validDbNames.Contains(selectedDb))
            {
                new Msgwin(false, "سال مالی انتخاب شده معتبر نیست.").ShowDialog();
                return;
            }

            // Validation Check: Prevent user from selecting the current connected database
            string currentDb = dbms.DoGetDataSQL<string>("SELECT DB_NAME()").FirstOrDefault() ?? string.Empty;
            if (selectedDb.Equals(currentDb, StringComparison.OrdinalIgnoreCase))
            {
                new Msgwin(false, "نمی‌توانید دیتابیس جاری (سال مالی فعلی) را به عنوان دیتابیس سال قبل انتخاب کنید.").ShowDialog();
                return;
            }

            string message = $"شما در حال انتقال موجودی از سال مالی [{selectedDb}] به دیتابیس جاری خود [{currentDb}] , این عملیات غیر قابل باز گشت است , آیا از این کار اطمینان دارید ؟";
            Msgwin msgwin = new Msgwin(true, message);
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true) { return; }

            // UI Constraint: Prevent double clicks and show progress
            Command3.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;
            ProgBar.Value = 0;
            ProgText.Text = "در حال آماده‌سازی...";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // Gather parameters
                DateTime dt = DateTime.Now;
                double upTime = Tarikh.GET_OADATE_DAO(); // Using GET_OADATE_DAO as per memory
                long upDate = CL_HESABDARI.FARSIDATE();
                string upUserName = "System" + (CL_HESABDARI.UCurrentUser()?.ToString() ?? string.Empty);
                string pcName = CL_HESABDARI.CurrentMachineName();
                string ipAddress = CL_HESABDARI.GETIPADD();
                string safeDbName = QuoteDbName(selectedDb);

                await Task.Run(() =>
                {
                    // Action helper to update UI
                    void UpdateProgress(int percentage, string messageText)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ProgBar.Value = percentage;
                            ProgText.Text = messageText;
                        });
                    }

                    using (var connection = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                UpdateProgress(5, "در حال دریافت لیست انبارها...");

                                // Create/Recreate empty STUFFSK temp table within the transaction
                                string sqlCreateTemp = @"
                                IF OBJECT_ID(N'dbo.STUFFSK', N'U') IS NOT NULL DROP TABLE dbo.STUFFSK;
                                CREATE TABLE dbo.STUFFSK (CODE NVARCHAR(50), MAND FLOAT, FII FLOAT, MABLK FLOAT, ANBAR NVARCHAR(50));";
                                connection.Execute(sqlCreateTemp, transaction: transaction, commandTimeout: 0);

                                // Fetch list of warehouses
                                var anbars = connection.Query<string>($"SELECT CODE FROM {safeDbName}.dbo.TCOD_ANBAR", transaction: transaction, commandTimeout: 0).ToList();

                                if (anbars.Count > 0)
                                {
                                    for (int i = 0; i < anbars.Count; i++)
                                    {
                                        string anbarCode = anbars[i] ?? string.Empty;
                                        if (string.IsNullOrWhiteSpace(anbarCode)) continue;

                                        int progressValue = 5 + (int)(((float)(i + 1) / anbars.Count) * 75); // 5% to 80%
                                        UpdateProgress(progressValue, $"در حال محاسبه موجودی انبار {anbarCode} ({i + 1} از {anbars.Count})...");

                                        // Execute original AKMOGUDI_KOL_ANBAR to ensure perfect decimal/float precision and legacy MSACCESS compatibility
                                        string sqlCalc = $@"
                                        INSERT INTO dbo.STUFFSK (CODE, MAND, FII, MABLK, ANBAR)
                                        SELECT AK.CODE, AK.MAND, AK.FII, AK.MABLK, AK.ANBAR
                                        FROM {safeDbName}.dbo.AKMOGUDI_KOL_ANBAR(99999999, @AnbarCode) AS AK;";

                                        connection.Execute(sqlCalc, new { AnbarCode = anbarCode }, transaction: transaction, commandTimeout: 0);
                                    }
                                }

                                UpdateProgress(85, "در حال پشتیبان‌گیری و بروزرسانی اطلاعات سال جاری...");

                                // Update and Backup phase
                                string sqlUpdateTransaction = @"
                                SET NOCOUNT ON;

                                -- 1. Backup STUF_DEF
                                INSERT INTO dbo.TR_STUF_DEF
                                (
                                    CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F,
                                    DEPART, IDD, CMBAA, VAZN, OKF, UP_TIME, UP_DATE, UP_USER_NAME, PC_NAME, IPADD
                                )
                                SELECT
                                    CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F,
                                    DEPART, IDD, CMBAA, VAZN, OKF, @UpTime, @UpDate, @UpUserName, @PcName, @IpAdd
                                FROM dbo.STUF_DEF;

                                -- 2. Backup STUF_FSK
                                INSERT INTO dbo.TR_STUF_FSK
                                (
                                    CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, IDD, POSITION,
                                    B_SEF, N_SEF, MIN_M, MAX_M, UP_TIME, UP_DATE
                                )
                                SELECT
                                    CODE, ANBAR, MOGODI_A, FI_A, MABL_A, MANDAH_A, VAZ, IDD, POSITION,
                                    B_SEF, N_SEF, MIN_M, MAX_M, @UpTime, @UpDate
                                FROM dbo.STUF_FSK;

                                -- 3. Update Current Year Inventory
                                UPDATE F
                                SET
                                    F.MOGODI_A = S.MAND,
                                    F.FI_A = S.FII,
                                    F.MABL_A = S.MABLK
                                FROM dbo.STUF_FSK AS F
                                INNER JOIN dbo.STUFFSK AS S
                                    ON S.CODE = F.CODE
                                   AND S.ANBAR = F.ANBAR;";

                                connection.Execute(sqlUpdateTransaction, new
                                {
                                    UpTime = upTime,
                                    UpDate = upDate,
                                    UpUserName = upUserName,
                                    PcName = pcName,
                                    IpAdd = ipAddress
                                }, transaction: transaction, commandTimeout: 0);

                                transaction.Commit();
                                UpdateProgress(100, "عملیات با موفقیت پایان یافت");
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw; // Re-throw to be caught by outer catch block
                            }
                        }
                    }
                });
                string finalMessage = $"موجودی‌ها با موفقیت به‌روزرسانی شد.\n\nزمان کل اجرای عملیات: {stopwatch.Elapsed.TotalSeconds:F2} ثانیه";
                new Msgwin(false, finalMessage).ShowDialog();

                new Msgwin(false, "موجودی‌ها با موفقیت به‌روزرسانی شد.").ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در انتقال موجودی:\n" + ex.Message).ShowDialog();
            }
            finally
            {
                // Restore UI state
                Command3.IsEnabled = true;
                ProgressPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Command4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void YEA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YEA.SelectedItem == null) return;

            string selected = YEA.SelectedItem.ToString() ?? string.Empty;

            // Replicating legacy behavior where "+" or "-" opened another form
            if (selected == "+" || selected == "-")
            {
                new Msgwin(false, "انتخاب سال با علائم +/- در این فرم پشتیبانی نمی‌شود.").ShowDialog();
            }
        }

        // Helper method to safely escape Database Names for T-SQL
        private static string QuoteDbName(string dbName)
        {
            return $"[{dbName.Replace("]", "]]", StringComparison.Ordinal)}]";
        }
    }
}