using Functions;
using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.CUC;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UiTools;
using Wins.WinMenus.WinAutomasion;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Prg_UI.Wins.WinMenus.WinAutomasion
{
    public partial class MAIN : Window
    {
        #region HeaderWindow
        //Header Window Begin
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            packIcon = null;
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
        private void lbl_budget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        //Header Window End;
        #endregion

        #region LCLMODEL
        public class CutsomPeriority_Model
        {
            public int? PERIORITY { get; set; }
            public string PERIORITY_NAME { get; set; }
        }
        public class CutsomStatus_Model
        {
            public int? STATUS { get; set; }
            public string STATUS_NAME { get; set; }
        }
        public class ATMC1
        {
            public string? SAL_NAME { get; set; }
            public int? SUBUSERCO { get; set; }
            public int? USERCO { get; set; }
        }
        #endregion



        UniversControl universControl = new UniversControl();
        public BulkObservableCollection<TASKS> TASK_DATA { get; set; } = new BulkObservableCollection<TASKS>();
        public ObservableCollection<CutsomPeriority_Model> PERIORITY_COMBO_DATA { get; set; } = new ObservableCollection<CutsomPeriority_Model>();
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; set; } = new ObservableCollection<CutsomStatus_Model>();
        public ObservableCollection<CUST_HESAB> COMP_COD_DATA { get; set; } = new ObservableCollection<CUST_HESAB>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        // Singleton instance
        private static MAIN _instance;
        // Public property to get the singleton instance
        public static MAIN MAIN_INST => _instance ??= new MAIN();
        public string? DEFAULTVAL_COMPCODE { get; private set; }
        public object LastValidPERSONEL { get; private set; }
        public TASKS? WAS_ROW_ITEM { get; private set; }

        // Static method to get the instance and show the window
        //public static void ShowWindow()
        //{
        //    MAIN_INST.Show();
        //}
        // Static method to close the window and set the instance to null
        private MAIN()
        {
            InitializeComponent();
            this.DataContext = this;
            ////this.Owner = PublicVRB.WINBASE;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            await DoLoadKartabl(true);

            await CheckRemindersAndMessages();

            ConfigTimers();

            PERSONEL.Focus();

            LoadRefresherSettings();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                DataGrid dg = tASKSDataGrid;
                UIElement uie = e.OriginalSource as UIElement;
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;

                    if (DG_TASKS_IsFocused)
                    {
                        OpenWinBaseRow();
                    }

                    CL_LMethods.SendKey_US(Key.Tab);
                }
                else
                {
                    if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        var Before = AutoRefreshToggle.IsChecked;
                        AutoRefreshToggle.IsChecked = false;
                        DataGridExtension.HandleKeyPress(sender, e, tASKSDataGrid);
                        AutoRefreshToggle.IsChecked = Before;
                    }

                    //if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F8 || e.SystemKey == Key.F8))
                    //{
                    //    e.Handled = true;
                    //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
                    //}
                }
            }
            catch { }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _Cts_?.Cancel();
            _Cts_?.Dispose();

            MRPTimer?.Stop(); MRPTimer = null;
            KartablTimer?.Stop(); KartablTimer = null;

            _Semaphore_?.Dispose(); _isMainDisposed = true;
            _KartablSemaphore?.Dispose(); _isKartablDisposed = true;

            MAIN_INST?.Close();
            _instance = null;
        }

        public static Visual I_AM_MAIN { get; set; }

        public List<COMBOPERSONEL> rst_personel = null;
        public bool NowIsReady { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; } = 0;

        private bool _can;

        public string WAS_COMP_COD { get; set; }
        public bool CanUisBeActive
        {
            get
            {
                return _can;
            }
            set
            {
                _can = value;
                if (_can is true)
                {
                    STK_RIGHTPANEL.IsEnabled = true;
                    LBL_STATE_RESULT.Visibility = Visibility.Hidden;
                    tASKSDataGrid.Visibility = Visibility.Visible;
                    RefloadBtn.IsEnabled = true;
                    SaveBtn.IsEnabled = true;
                }
                else
                {
                    STK_RIGHTPANEL.IsEnabled = false;
                    LBL_STATE_RESULT.Visibility = Visibility.Visible;
                    tASKSDataGrid.Visibility = Visibility.Hidden;
                    RefloadBtn.IsEnabled = false;
                    SaveBtn.IsEnabled = false;
                }
            }
        }

        public bool DoesTextedOnCOMP_COD { get; set; } = false;
        public bool DG_TASKS_IsFocused { get; private set; }

        private bool ISHEADOK(bool _ShowMessage_ = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(PERSONEL.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مجری نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(COMP_COD.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تماس گیرنده نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(TASK.Text.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وظیفه نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(PERIORITY.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "اولویت نمیتواند خالی باشد" });
            }
            if (string.IsNullOrEmpty(STATUS.SelectedValue?.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت نمیتواند خالی باشد" });
            }

            if (ErrosMessages.Count > 0)
            {
                if (_ShowMessage_)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }

        #region Timers
        private DispatcherTimer MRPTimer; //Reminders and Message Payam
        private readonly SemaphoreSlim _Semaphore_ = new SemaphoreSlim(1, 1);
        private bool _isMainDisposed = false;
        private readonly CancellationTokenSource _Cts_ = new CancellationTokenSource();
        private int _refreshCounter = 0;

        private const int MsgAndRemindersInterval = 1;

        private DispatcherTimer KartablTimer;
        private readonly SemaphoreSlim _KartablSemaphore = new SemaphoreSlim(1, 1);
        private bool _isKartablDisposed = false;
        private int _kartablIntervalSeconds = 30;
        private void ConfigTimers()
        {
            //نوتیفیکشن پیام ها و یادآوری ها
            MRPTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(MsgAndRemindersInterval)
            };
            MRPTimer.Tick += async (sender, e) => await MRP_TimerTick();
            MRPTimer.Start();

            //سطر های داخل اتوماسیون :کارتابل
            KartablTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_kartablIntervalSeconds)
            };
            KartablTimer.Tick += async (sender, e) => await KartablTimer_Tick();
            KartablTimer.Start();
        }
        private async Task MRP_TimerTick()
        {
            if (_isMainDisposed)
            {
                // Safeguard against tick event after disposal
                return;
            }


            if (/*!Convert.ToBoolean(AutoRefreshToggle.IsChecked) ||*/ !_Semaphore_.Wait(0))
            {
                return; // Another instance is running or auto-refresh is disabled
            }

            try
            {
                //Interlocked.Increment(ref _refreshCounter);
                //await Dispatcher.InvokeAsync(() => LBL_COUNTER.Content = _refreshCounter.ToString());

                var tasks = new Task[]
                {
                    CheckRemindersAndMessages(), // _Cts_.Token
                    //DoLoadKartabl()
                };
                await Task.WhenAll(tasks);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlEx && CL_CCNNMANAGER.IsConnectionRelated(sqlEx))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    throw;
                }
                await Dispatcher.InvokeAsync(() =>
                {
                    universControl.PopNotifyShow($".خطا در بروز رسانی خودکار", Pop1, Pop1Text1, Pop_Border1);
                });
            }
            finally
            {
                if (!_isKartablDisposed)
                {
                    _Semaphore_.Release();
                }
            }
        }
        private async Task KartablTimer_Tick()
        {
            if (_isKartablDisposed)
            {
                // Safeguard against tick event after disposal
                return;
            }

            if (Hameh.IsChecked ?? false)
            {
                return;
            }

            if (!Convert.ToBoolean(AutoRefreshToggle.IsChecked) || !_KartablSemaphore.Wait(0))
            {
                return; // Another instance is running
            }

            try
            {
                //Interlocked.Increment(ref _refreshCounter);
                //await Dispatcher.InvokeAsync(() => LBL_COUNTER.Content = _refreshCounter.ToString());

                await DoLoadKartabl();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlEx && CL_CCNNMANAGER.IsConnectionRelated(sqlEx))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    throw;
                }
                await Dispatcher.InvokeAsync(() =>
                {
                    universControl.PopNotifyShow($".خطا در بروز رسانی کارتابل خودکار: {ex.Message}", Pop1, Pop1Text1, Pop_Border1);
                });
            }
            finally
            {
                if (!_isKartablDisposed)
                {
                    _KartablSemaphore.Release();
                }
            }
        }
        private void RefreshIntervalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RefreshIntervalCombo.SelectedValue is int selectedInterval)
            {
                _kartablIntervalSeconds = selectedInterval;
                if (KartablTimer != null)
                {
                    KartablTimer.Interval = TimeSpan.FromSeconds(_kartablIntervalSeconds);
                }

                if (NowIsReady) //Avoid to run this code at startup
                {
                    SaveRefresherSettings();
                }
            }
        }
        #endregion

        private bool CheckIfAnyRelatedWindowIsOpen()
        {
            // This could be optimized further based on application needs.
            return Application.Current.Windows.OfType<INBOXPAN>().Any() ||
                   Application.Current.Windows.OfType<REMINDERPANEL>().Any() ||
                   //Application.Current.Windows.OfType<WIN_MESSAGEPANEL>().Any() ||
                   Application.Current.Windows.OfType<Msgwin>().Any();
        }

        /// <summary>
        /// تنظیمات مربوط به سیستم Snooze پیام‌ها
        /// </summary>
        public static class SnoozeSettings
        {
            /// <summary>
            /// حداکثر تعداد دفعات نمایش مجدد پیام (بعد از اولین بار)
            /// با این تنظیم، پیام در مجموع 4 بار نمایش داده می‌شود (1 بار اصلی + 3 بار Snooze)
            /// </summary>
            public const int MAX_SNOOZE_COUNT = 3;

            /// <summary>
            /// فاصله زمانی بین نمایش‌های مجدد (به دقیقه) بر اساس تعداد Snooze های انجام شده
            /// </summary>
            public static int GetSnoozeInterval(int snoozeCount)
            {
                switch (snoozeCount)
                {
                    // بعد از نمایش اول (snoozeCount = 0)، برای 15 دقیقه Snooze می‌شود
                    case 0: return 15;
                    // بعد از Snooze اول (snoozeCount = 1)، برای 30 دقیقه Snooze می‌شود
                    case 1: return 30;
                    // بعد از Snooze دوم (snoozeCount = 2)، برای 60 دقیقه Snooze می‌شود
                    case 2: return 60;
                    // بعد از رسیدن به حداکثر، دیگر نمایش داده نمی‌شود
                    default: return int.MaxValue;
                }
            }
        }
        private static bool _isCheckingMessages = false;
        private static readonly object _lock = new object();

        private async Task CheckRemindersAndMessages()
        {
            // اطمینان از اینکه تنها یک نمونه از این تابع در حال اجراست
            lock (_lock)
            {
                if (_isCheckingMessages)
                {
                    return; // اگر در حال اجرا بود، خارج شو
                }
                _isCheckingMessages = true;
            }

            try
            {
                bool shouldExit = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    shouldExit = CheckIfAnyRelatedWindowIsOpen();
                });

                if (shouldExit) return;

                // ==================== بخش 1: یادآوری‌ها (بدون تغییر) ====================
                var remainders = await dbms.SqlQueryAsync<REMAINDER>(@"
                    SELECT IDNUM, PERSONEL, PAYAM, STATUS, CTDATE, CTTIME, USERNAME, COMP_COD, STDATE, STTIME, SMSOK, CRT, UID 
                    FROM dbo.REMAINDER 
                    WHERE STATUS = 1 
                    AND PERSONEL = @UserCod 
                    AND STDATE = @CurrentDate",
                    new { UserCod = Baseknow.USERCOD, CurrentDate = Tarikh.GoGetPersianDate(true) });

                var currentTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                var processedReminderIds = new List<int>();

                foreach (var remainder in remainders)
                {
                    TimeSpan? reminderTime = null;
                    if (remainder.STTIME.HasValue)
                    {
                        reminderTime = new TimeSpan(remainder.STTIME.Value.Hour, remainder.STTIME.Value.Minute, 0);
                    }

                    if (reminderTime != null && reminderTime <= currentTime)
                    {
                        bool windowIsOpen = false;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            windowIsOpen = CheckIfAnyRelatedWindowIsOpen();
                        });

                        if (!windowIsOpen)
                        {
                            var updateResult = await dbms.DoExecuteSQLAsync("UPDATE REMAINDER SET STATUS = 2 WHERE IDNUM = @IdNum AND STATUS = 1", new { IdNum = remainder.IDNUM });

                            if (updateResult > 0)
                            {
                                var newMessage = new MESAGEP
                                {
                                    PERSONEL = remainder.PERSONEL,
                                    COMP_COD = remainder.COMP_COD,
                                    PAYAM = @$".....................:::::::::::::::|||||||||||||||||||||(     يادآوري     )|||||||||||||||||||||:::::::::::::::.....................
                                    {remainder.PAYAM}",
                                    STATUS = 1,
                                    STDATE = remainder.STDATE,
                                    STTIME = DateTime.Now.Hour * 100 + DateTime.Now.Minute,
                                    USERNAME = remainder.USERNAME
                                };

                                await dbms.DoExecuteSQLAsync(@"
                                INSERT INTO dbo.MESAGEP (PERSONEL, COMP_COD, PAYAM, STATUS, STDATE, STTIME, USERNAME) 
                                VALUES (@PERSONEL, @COMP_COD, @PAYAM, @STATUS, @STDATE, @STTIME, @USERNAME)", newMessage);

                                if (remainder.SMSOK == true)
                                {
                                    var SMSAC = new CL_SMSAC();
                                    await SMSAC.ErselSmsAsync(
                                        CL_HESABDARI.GETUSERCO((int)remainder.PERSONEL),
                                        $"ياد آوري {remainder.PAYAM} - {remainder.USERNAME}",
                                        (long)remainder.IDNUM, 2, false);
                                }
                                processedReminderIds.Add((int)remainder.IDNUM);
                            }
                        }
                    }
                }

                var activeRemainderCount = remainders.Count(r => r.STATUS == 1 && !processedReminderIds.Contains((int)r.IDNUM));
                await Dispatcher.InvokeAsync(() => yad.Content = activeRemainderCount);
                await Dispatcher.InvokeAsync(() => tas.Content = tASKSDataGrid.Items.Count);

                // ==================== بخش 2: پیام‌ها Snooze (نسخه بهینه‌سازی شده) ====================
                var allMessages = await dbms.SqlQueryAsync<MESAGEP>(@"
                SELECT 
                    IDNUM, PERSONEL, PAYAM, STATUS, STDATE, STTIME, USERNAME, COMP_COD, 
                    CRT, UID, ISNULL(IsNotifyCalled, 0) as IsNotifyCalled,
                    ISNULL(SNOOZE_COUNT, 0) as SNOOZE_COUNT,
                    LAST_NOTIFY_TIME
                FROM dbo.MESAGEP
                WHERE STATUS = 1 AND PERSONEL = @UserCod
                ORDER BY IDNUM DESC", new { UserCod = Baseknow.USERCOD });

                var messagesToNotify = new List<MESAGEP>();
                var currentDateTime = DateTime.Now;

                foreach (var message in allMessages)
                {
                    if (message.STATUS != 1) continue; //خوانده شده بیا بیرون

                    bool shouldNotify = false;

                    if (!Convert.ToBoolean(message.IsNotifyCalled))
                    {
                        shouldNotify = true;
                    }
                    else if (message.SNOOZE_COUNT < SnoozeSettings.MAX_SNOOZE_COUNT)
                    {
                        if (message.LAST_NOTIFY_TIME.HasValue)
                        {
                            var requiredInterval = SnoozeSettings.GetSnoozeInterval(message.SNOOZE_COUNT ?? 0);
                            var minutesSinceLastNotify = (currentDateTime - message.LAST_NOTIFY_TIME.Value).TotalMinutes;

                            if (minutesSinceLastNotify >= requiredInterval)
                            {
                                shouldNotify = true;
                            }
                        }
                    }

                    if (shouldNotify)
                    {
                        messagesToNotify.Add(message);
                    }
                }

                await Dispatcher.InvokeAsync(() => nor.Content = allMessages.Count());

                if (!messagesToNotify.Any()) return;

                foreach (var item in messagesToNotify)
                {
                    string cleanedText = item.PAYAM
                        .Replace(".....................:::::::::::::::|||||||||||||||||||||(     يادآوري     )|||||||||||||||||||||:::::::::::::::.....................", "")
                        .Replace("\n", " ").Trim();

                    bool isReminder = item.PAYAM.Contains("|||||||||||||||||||||(");
                    string notificationTitle;
                    PackIconKind iconKind;

                    if (isReminder)
                    {
                        notificationTitle = (item.SNOOZE_COUNT > 0)
                            ? $"یادآوری (یادآوری #{item.SNOOZE_COUNT + 1})"
                            : "یادآوری";
                        iconKind = PackIconKind.Clock;
                    }
                    else
                    {
                        var fromPersonName = rst_personel?.FirstOrDefault(i => i.USERCO == item.PERSONEL)?.SAL_NAME ?? "پیام";
                        notificationTitle = (item.SNOOZE_COUNT > 0)
                            ? $"{fromPersonName} (یادآوری #{item.SNOOZE_COUNT + 1})"
                            : fromPersonName;
                        iconKind = PackIconKind.Message;
                    }

                    await Dispatcher.InvokeAsync(() =>
                    {
                        NotificationManager.Instance.EnqueueNotification(
                            notificationTitle,
                            cleanedText,
                            iconKind,
                            15, default, item.IDNUM ?? 0);
                    });
                }

                var firstTimeNotifyIds = messagesToNotify
                    .Where(m => !Convert.ToBoolean(m.IsNotifyCalled))
                    .Select(m => m.IDNUM)
                    .ToList();

                var snoozeNotifyIds = messagesToNotify
                    .Where(m => Convert.ToBoolean(m.IsNotifyCalled))
                    .Select(m => m.IDNUM)
                    .ToList();

                if (firstTimeNotifyIds.Any())
                {
                    await dbms.DoExecuteSQLAsync(@"
                    UPDATE dbo.MESAGEP 
                    SET IsNotifyCalled = 1, SNOOZE_COUNT = 0, LAST_NOTIFY_TIME = GETDATE()
                    WHERE IDNUM IN @Ids",
                        new { Ids = firstTimeNotifyIds });
                }

                if (snoozeNotifyIds.Any())
                {
                    await dbms.DoExecuteSQLAsync(@"
                    UPDATE dbo.MESAGEP 
                    SET SNOOZE_COUNT = ISNULL(SNOOZE_COUNT, 0) + 1, LAST_NOTIFY_TIME = GETDATE()
                    WHERE IDNUM IN @Ids",
                        new { Ids = snoozeNotifyIds });
                }
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlEx && CL_CCNNMANAGER.IsConnectionRelated(sqlEx))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    throw;
                }

                await Dispatcher.InvokeAsync(() => universControl.PopNotifyShow($"خطا در بروز رسانی پیام/یادآوری", Pop1, Pop1Text1, Pop_Border1));
            }
            finally
            {
                lock (_lock)
                {
                    _isCheckingMessages = false;
                }
            }
        }

        private async Task DoLoadKartabl(bool _IsFirstTime_ = false)
        {
            CanUisBeActive = false;
            try
            {
                //int gronum = Convert.ToInt32(((RadioButton)gro.Children.Cast<UIElement>().FirstOrDefault(r => r is RadioButton rb && rb.IsChecked == true))?.Tag);
                //int gronum = Convert.ToInt32(((CheckBox)gro.Children.Cast<UIElement>().FirstOrDefault(r => r is CheckBox rb && rb.IsChecked == true))?.Tag);

                string checkedTags = string.Join(",",
                                                 gro.Children.OfType<CheckBox>() // Get all CheckBox controls
                                                     .Where(cb => cb.IsChecked == true) // Filter only checked ones
                                                     .Select(cb => cb.Tag?.ToString()) // Select the Tag value as a string
                                                     .Where(tag => !string.IsNullOrEmpty(tag))); // Exclude null or empty tags

                if (string.IsNullOrEmpty(checkedTags) || checkedTags == ",")
                {
                    checkedTags = "1000";
                    CHBX_HAMEH.IsChecked = true;
                }

                string status = "1";
                if (AnjamNashodeh.IsChecked == true)
                    status = "(TSK.STATUS = 1) AND ";
                if (AnjamShode.IsChecked == true)
                    status = "(TSK.STATUS = 2) AND ";
                if (LaghvShodeh.IsChecked == true)
                    status = "(TSK.STATUS = 3) AND ";
                if (Hameh.IsChecked == true)
                    status = "";

                string personelValue = UNVDER_PERSONEL.SelectedValue != null ? UNVDER_PERSONEL.SelectedValue.ToString() : Baseknow.USERCOD.ToString();

                string personelQueryCondition = $" AND (TSK.PERSONEL = {personelValue}) ";
                if (personelValue == "0" || personelValue == "همه")
                {
                    personelQueryCondition = string.Empty;
                }

                string query = @$" SELECT TSK.IDNUM, CH.NAME, TSK.GR,
                                   TSK.PERSONEL, TSK.TASK,
                                   TSK.PERIORITY, TSK.STATUS,
                                   TSK.STDATE, TSK.STTIME,
                                   TSK.ENDATE, TSK.ENTIME,
                                   TSK.USERNAME, TSK.COMP_COD,
                                   TSK.SUMTIME, TSK.pic,
                                   TSK.ss, TSK.skid,
                                   TSK.num, TSK.tg,
                                   TSK.CTIM, TSK.USERCO, TSK.SEE
                                FROM dbo.TASKS AS TSK WITH (INDEX(IX_TASKS_Status1))
                                     LEFT HASH JOIN dbo.CUST_HESAB AS CH
                                         ON CH.hes = TSK.COMP_COD
                                WHERE {status} ({(checkedTags == "1000" ? "TSK.IDNUM > 0" : $"TSK.skid IN ({checkedTags}) ")}) {personelQueryCondition}
                                ORDER BY TSK.IDNUM";


                // Fetch data asynchronously
                //******************************
                var RowsTask = await dbms.DoGetDataSQLAsync<TASKS>(query);
                //******************************

                await Task.Delay(100); // Small delay to allow UI to update
                await this.Dispatcher.InvokeAsync(() =>
                {
                    TASK_DATA.ReplaceAll(RowsTask);
                });
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlEx && CL_CCNNMANAGER.IsConnectionRelated(sqlEx))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    throw;
                }

                await this.Dispatcher.InvokeAsync(() =>
                {
                    new Msgwin(false, "بروز رسانی به دلیل خطا کامل انجام نشد!").Show();
                });
            }
            finally
            {
                UpdateDataGridFocus(_IsFirstTime_);
                CanUisBeActive = true;
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //کبموباکس مجری
            //List<COMBOPERSONEL> sub_rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, SUBUSERCO AS [USERCO] FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD} ").ToList();
            const string sqlSub = @"
                 SELECT
                      sd.SAL_NAME,
                      sd.PSAL_NAME,
                      sd.GRSAL,
                      sd.ENABL,
                      cs.SUBUSERCO     AS IDD,   
                      cs.SUBUSERCO     AS USERCO 
                 FROM dbo.CHARTSAZMANI        cs
                 LEFT JOIN SALA_DTL           sd  ON cs.SUBUSERCO = sd.IDD
                 LEFT JOIN USER_PERSONEL_ORDER uo  ON cs.SUBUSERCO = uo.PERSONEL_ID
                                                  AND uo.USER_ID   = @UserId
                 WHERE cs.USERCO  = @UserId
                 ORDER BY
                      CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,  -- اولويات كاربر
                      uo.SORT_ORDER,
                      sd.SAL_NAME;";

            List<COMBOPERSONEL> sub_rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sqlSub, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var rows in sub_rst_personel)
            {
                rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
            }

            //rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();

            //کبموباکس مجری
            string sql = @"
           SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD as USERCO
           FROM SALA_DTL sd
           LEFT JOIN USER_PERSONEL_ORDER uo 
                ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
           WHERE sd.ENABL = 0
           ORDER BY
                CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var rows in rst_personel)
            {
                if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }

            if (sub_rst_personel.Any(x => x?.SAL_NAME == null || string.IsNullOrEmpty(x?.SAL_NAME))) //یعنی گزینه همه رو داره
            {
                sub_rst_personel.Remove(sub_rst_personel.FirstOrDefault(x => x?.SAL_NAME == null || string.IsNullOrEmpty(x?.SAL_NAME)));

                sub_rst_personel = sub_rst_personel.Where(i => i.ENABL == 0).ToList();
                sub_rst_personel.Add(new COMBOPERSONEL { SUBUSERCO = 0, USERCO = 0, SAL_NAME = "همه", IDD = 0, ENABL = 0, GRSAL = 11 }); //برای دسترسی داشتن به کارتابل همه کاربران
            }

            UNVDER_PERSONEL.ItemsSource = sub_rst_personel;

            UNVDER_PERSONEL.SelectionChanged -= UNVDER_PERSONEL_SelectionChanged;
            UNVDER_PERSONEL.SelectedValue = null;
            UNVDER_PERSONEL.SelectedValue = Baseknow.USERCOD;
            UNVDER_PERSONEL.Items.Refresh();
            UNVDER_PERSONEL.SelectionChanged += UNVDER_PERSONEL_SelectionChanged;


            //مجری در دیتاگرید
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.SelectedValue = null;
            PERSONEL.SelectedValue = Baseknow.USERCOD;
            LastValidPERSONEL = PERSONEL.SelectedValue;

            //کمبوباکس مجری در دیتاگرید
            PERSONEL_CMB.ItemsSource = rst_personel;

            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "انجام نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "انجام شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "لغو شده" });

            //اولویت در دیتاگرید
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 1, PERIORITY_NAME = "فوری" });
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 2, PERIORITY_NAME = "معمولی" });

            STATUS.ItemsSource = STATUS_COMBO_DATA;
            PERIORITY.ItemsSource = PERIORITY_COMBO_DATA;

            DEFAULTVAL_COMPCODE = dbms.DoGetDataSQL<string>($"SELECT HES FROM SALA_DTL WHERE IDD = {Baseknow.USERCOD}").FirstOrDefault();
            COMP_COD_DATA?.Clear();
            var compcod = dbms.DoGetDataSQL<CUST_HESAB>($"SELECT TOP(1) hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'{DEFAULTVAL_COMPCODE}' ORDER BY NAME ").ToList();
            for (int i = 0; i < compcod.Count; i++)
            {
                COMP_COD_DATA.Add(compcod[i]);
                WAS_COMP_COD = compcod[i].NAME;
            }
            COMP_COD.SelectedValue = DEFAULTVAL_COMPCODE; COMP_COD.Items.Refresh();

            var NESBAT_MODEL = new List<COMBOYMODEL>()
            {
                new COMBOYMODEL { NAME = "1 ثانیه",  ID = 1   },
                new COMBOYMODEL { NAME = "5 ثانیه",  ID = 5   },
                new COMBOYMODEL { NAME = "15 ثانیه", ID = 15  },
                new COMBOYMODEL { NAME = "30 ثانیه", ID = 30  },
                new COMBOYMODEL { NAME = "1 دقیقه",  ID = 60  },
                new COMBOYMODEL { NAME = "5 دقیقه",  ID = 300 },
                new COMBOYMODEL { NAME = "15 دقیقه", ID = 900 },
            };
            RefreshIntervalCombo.ItemsSource = NESBAT_MODEL; RefreshIntervalCombo.SelectedValue = 30; RefreshIntervalCombo.Items.Refresh();
        }
        private void ESLAH_SATR(int _idnum)
        {
            DateTime dt = DateTime.MinValue;
            dt = DateTime.Now;
            if (Convert.ToBoolean(Baseknow.TRANSF))
            {
                CL_HESABDARI.TR("TASKS", "(IDNUM = " + _idnum + " )", dt, 1);
                CL_HESABDARI.TR("EVENTS", "(IDNUM = " + _idnum + " )", dt, 1);
            }
        }
        private int GoInsertAutomasion()
        {
            string test2 = $@"INSERT INTO dbo.TASKS (PERSONEL,TASK,PERIORITY,STATUS,STDATE,STTIME,ENDATE,ENTIME,USERNAME,COMP_COD,SUMTIME,pic,ss,skid,num,tg,CTIM,USERCO,SEE)
                                                        VALUES
                                                        (   {PERSONEL.SelectedValue},
                                                            N'{TASK.Text.Trim()}',
                                                            {PERIORITY.SelectedValue},
                                                            1,
                                                            {Tarikh.GoGetPersianDate(true)},
                                                            {DateTime.Now:HHmm},
                                                            NULL,
                                                            NULL,
                                                            N'{Baseknow.UUSER}',
                                                            N'{COMP_COD.SelectedValue}',
                                                            NULL,
                                                            NULL,
                                                            NULL,
                                                            0,
                                                            0,
                                                            0,
                                                            GETDATE(),
                                                            {Baseknow.USERCOD},
                                                            0
                                                            )";
            int Cresult = (int)dbms.DoExecuteSQL(test2);
            if (Cresult >= 0)
            {
                return Cresult;
            }
            return -1;
        }
        private int GoUpdateAutomasion(TASKS? task = null)
        {
            string MyQuerySql = "";

            if (task != null)
            {
                //بروز رسانی رویداد ها بابت ارجاع به پرسنل : مجری
                #region PERSONEL_AfterUpdate
                try
                {
                    string selectedPersonName = "";
                    if (PERSONEL.SelectedItem is COMBOPERSONEL PersonelValue)
                    {
                        selectedPersonName = PersonelValue?.SAL_NAME ?? "";
                    }
                    string currentUser = Baseknow.UUSER ?? "Unknown";
                    string eventDescription = $"ارجاع شد به : {selectedPersonName}";
                    string sql = @"
                    INSERT INTO events 
                    (IDNUM, USERNAME, EVENTS, STDATE, STTIME, SKID, NUM, TG) 
                    VALUES 
                    (@IdNum, @UserName, @EventDesc, @StDate, @StTime, @SkId, @Num, @Tg)";
                    var parameters = new
                    {
                        IdNum = task?.IDNUM,
                        UserName = currentUser,
                        EventDesc = eventDescription,
                        StDate = Convert.ToInt32(Tarikh.GoGetPersianDate(true)),
                        StTime = Convert.ToInt32(DateTime.Now.ToString("HHmm")),
                        SkId = task?.skid,
                        Num = task?.num,
                        Tg = task?.tg
                    };
                    dbms.DoExecuteSQL(sql, parameters);
                }
                catch { }
                #endregion

                MyQuerySql = $"UPDATE dbo.TASKS SET PERSONEL = {PERSONEL.SelectedValue}, PERIORITY = {PERIORITY.SelectedValue}, STATUS = {STATUS.SelectedValue} WHERE IDNUM = {task.IDNUM}";
                ESLAH_SATR(Convert.ToInt32(task.IDNUM));
            }
            else
            {
                //بروز رسانی رویداد ها بابت ارجاع به پرسنل : مجری
                #region PERSONEL_AfterUpdate
                try
                {
                    string selectedPersonName = "";
                    if (PERSONEL.SelectedItem is COMBOPERSONEL PersonelValue)
                    {
                        selectedPersonName = PersonelValue?.SAL_NAME ?? "";
                    }

                    var SelectedTask = tASKSDataGrid.SelectedItem as TASKS;
                    string currentUser = Baseknow.UUSER ?? "Unknown";
                    string eventDescription = $"ارجاع شد به : {selectedPersonName}";
                    string sql = @"
                    INSERT INTO events 
                    (IDNUM, USERNAME, EVENTS, STDATE, STTIME, SKID, NUM, TG) 
                    VALUES 
                    (@IdNum, @UserName, @EventDesc, @StDate, @StTime, @SkId, @Num, @Tg)";
                    var parameters = new
                    {
                        IdNum = Convert.ToInt64(IDNUM1.Text),
                        UserName = currentUser,
                        EventDesc = eventDescription,
                        StDate = Convert.ToInt32(Tarikh.GoGetPersianDate(true)),
                        StTime = Convert.ToInt32(DateTime.Now.ToString("HHmm")),
                        SkId = SelectedTask?.skid,
                        Num = SelectedTask?.num,
                        Tg = SelectedTask?.tg
                    };
                    dbms.DoExecuteSQL(sql, parameters);
                }
                catch { }
                #endregion

                MyQuerySql = $"UPDATE dbo.TASKS SET TASK = N'{TASK.Text.Trim()}', PERSONEL = {PERSONEL.SelectedValue}, PERIORITY = {PERIORITY.SelectedValue}, STATUS = {STATUS.SelectedValue}, COMP_COD = N'{COMP_COD.SelectedValue}' WHERE IDNUM = {IDNUM1.Text}";

                ESLAH_SATR(Convert.ToInt32(IDNUM1.Text));
            }


            int Cresult = (int)dbms.DoExecuteSQL(MyQuerySql);
            if (Cresult >= 0)
            {
                return Cresult;
            }
            return -1;
        }

        private void UpdateDataGridFocus1(bool _IsFirstTime_)
        {
            if (tASKSDataGrid == null || TASK_DATA.Count == 0 || tASKSDataGrid.Items.Count <= 0)
            {
                return;
            }

            var _rowindex_ = (tASKSDataGrid.Items.Count - 1) < 0 ? 0 : (tASKSDataGrid.Items.Count - 1);
            if (_rowindex_ < 0 || _rowindex_ >= tASKSDataGrid.Items.Count) // Validate row index
            {
                return;
            }
            try
            {
                var item = tASKSDataGrid.Items[_rowindex_];  // Get the item at the specified index
                if (item == null) { return; }
            }
            catch { return; }


            if (_IsFirstTime_)
            {

                tASKSDataGrid.ScrollIntoView(tASKSDataGrid.Items[_rowindex_]);

                CL_LMethods.FocusCellReadyToEdit(tASKSDataGrid, "PERSONEL", _rowindex_, false);
            }
            else
            {
                if (CURRENT_ROW_INDEX == 0)
                {
                    CL_LMethods.FocusCellReadyToEdit(tASKSDataGrid, "PERSONEL", CURRENT_ROW_INDEX, false);
                }
                else
                {
                    CL_LMethods.FocusCellReadyToEdit(tASKSDataGrid, "PERSONEL", CURRENT_ROW_INDEX, false);
                }
            }
        }

        private void UpdateDataGridFocus(bool _IsFirstTime_)
        {
            if (AnjamNashodeh.IsChecked == false)
            {
                return;
            }

            // Null check for DataGrid and its data source
            if (tASKSDataGrid == null || TASK_DATA.Count == 0 || tASKSDataGrid.Items.Count <= 0)
            {
                return;
            }

            // Calculate the row index safely
            var _rowindex_ = Math.Max(0, tASKSDataGrid.Items.Count - 1);

            // Validate row index bounds
            if (_rowindex_ < 0 || _rowindex_ >= tASKSDataGrid.Items.Count)
            {
                return; // Prevent out-of-range index access
            }

            try //Just in case
            {
                if (CURRENT_ROW_INDEX > -1)
                {
                    // Try to retrieve the item from the DataGrid at the calculated index
                    var item = tASKSDataGrid.Items[CURRENT_ROW_INDEX];
                    if (item == null)
                    {
                        return; // Item is null, exit method
                    }
                }
            }
            catch
            {
                return;
            }

            // Use the dispatcher to delay the operation until UI rendering is completed
            Dispatcher.InvokeAsync(() =>
            {
                //// Check if it's the first time and focus the DataGrid accordingly
                if (_IsFirstTime_)
                {
                    //// Scroll the DataGrid to the specified row
                    tASKSDataGrid.ScrollIntoView(tASKSDataGrid.Items[_rowindex_]);

                    //// Focus on the "PERSONEL" column and prepare it for editing
                    CL_LMethods.FocusCellReadyToEdit(tASKSDataGrid, "PERSONEL", _rowindex_, false, true);
                }
                else
                {
                    //// Handle the current row index for non-first time
                    var targetRowIndex = Math.Max(0, CURRENT_ROW_INDEX); // Ensure it's a valid row index
                    CL_LMethods.FocusCellReadyToEdit(tASKSDataGrid, "PERSONEL", targetRowIndex, false, true);
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }


        public void RestoreLastFocus(DataGrid dtgrd)
        {
            if (CURRENT_ROW_INDEX is 0)
                CURRENT_ROW_INDEX = TASK_DATA.Count - 1;
            //Re Focus On Last Row was Focused - 1
            #region Way1
            //dtgrd.Focus();
            //dtgrd.SelectedIndex = CURRENT_ROW_INDEX;
            //dtgrd.CurrentCell = new DataGridCellInfo(dtgrd.SelectedItem, dtgrd.Columns[0]);
            #endregion

            //Re Focus On Last Row was Focused - 2
            if (dtgrd.Items.Count > 0 && NowIsReady)
            {
                dtgrd.Focus();
                DataGridRow row = dtgrd.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX) as DataGridRow;
                if (row is null)
                {
                    object item = dtgrd.Items[CURRENT_ROW_INDEX];
                    dtgrd.ScrollIntoView(dtgrd.Items[CURRENT_ROW_INDEX]);
                    row = (DataGridRow)dtgrd.ItemContainerGenerator.ContainerFromIndex(CURRENT_ROW_INDEX);
                    dtgrd.SelectedItem = item;

                    ////ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    //var col_index = dtgrd.Columns.FirstOrDefault(c => c.SortMemberPath == "TASK").DisplayIndex;
                    //DataGridCell cell = LocalFunctions.GetCell(dtgrd, row, Convert.ToInt32(col_index));
                    //if (cell != null)
                    //    cell.Focus();
                }
                else
                {
                    object item = dtgrd.Items[CURRENT_ROW_INDEX];
                    dtgrd.SelectedItem = item;
                    dtgrd.ScrollIntoView(item);

                    ////ستون که میخوای باتوجه به ردیفی که خودم میدونم روش فوکوس کنم
                    //var col_index = dtgrd.Columns.FirstOrDefault(c => c.SortMemberPath == "TASK").DisplayIndex;
                    //DataGridCell cell = LocalFunctions.GetCell(dtgrd, row, Convert.ToInt32(col_index));
                    //if (cell != null)
                    //    cell.Focus();
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ISHEADOK() is false)
            {
                universControl.PopNotifyShow(".مقادیر را صحیح وارد کنید", Pop1, Pop1Text1, Pop_Border1);

                return;
            }

            var selectedTasks = TASK_DATA.Where(task => task.IsSelected).ToList();
            if (selectedTasks?.Count > 1)
            {
                new Msgwin(false, "چندین سطر باهم انتخاب شده , برای ذخیره فقط یک سطر میتوان انتخاب کرد , ابتدا این مورد را اصلاح کنید").Show();
                return;
            }


            bool AnyChangeHappend = false;


            if (string.IsNullOrEmpty(IDNUM1.Text) || IDNUM1.Text == "0")
            {
                _ = GoInsertAutomasion(); AnyChangeHappend = true;
            }
            else
            {
                _ = GoUpdateAutomasion(); AnyChangeHappend = true;
            }

            if (AnyChangeHappend)
            {
                //universControl.PopNotifyShow(".ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1,"#FF1AAA2C");
            }

            ClearSelectedRowsAsGroup();

            //TASK_LST.Add(cL_Automasion);
            DoLoadKartabl();
        }
        private async void RefloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox _THECHECKBOX_)
            {
                if (_THECHECKBOX_?.Tag.ToString() == "1000")
                {
                    var CHECKLISTS = gro.Children.Cast<CheckBox>().Where(r => r is CheckBox rb).ToList();
                    foreach (var item in CHECKLISTS)
                    {
                        if (item?.Tag.ToString() != "1000")
                        {
                            item.IsChecked = false;
                        }
                    }
                }
                else if (_THECHECKBOX_?.Tag.ToString() != "1000")
                {
                    CHBX_HAMEH.IsChecked = false;
                }

            }

            ClearSelectedRowsAsGroup();

            await DoLoadKartabl();
            await CheckRemindersAndMessages();

        }
        private void UNVDER_PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tASKSDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (tASKSDataGrid.SelectedItem == null) return;

                //IF IS NOT NULL
                if (!(tASKSDataGrid.Items.Count < 1) && !(tASKSDataGrid.SelectedItem is null))
                {
                    /////PERSONEL.SelectedValue = (tASKSDataGrid.SelectedItem as TASKS).PERSONEL;
                    CURRENT_ROW_INDEX = tASKSDataGrid.SelectedIndex;

                    var ROW = tASKSDataGrid.SelectedItem as TASKS;
                    if (!COMP_COD_DATA.Any(item => item?.hes == ROW.COMP_COD))
                    {
                        COMP_COD_DATA.Add(new CUST_HESAB { hes = ROW.COMP_COD, NAME = ROW.NAME });
                    }
                    COMP_COD.SelectedValue = ROW.COMP_COD;
                    COMP_COD.Items.Refresh();

                    if (ROW?.PERSONEL != null)
                    {
                        PERSONEL.SelectedValue = ROW.PERSONEL; PERSONEL.Items.Refresh();

                        LastValidPERSONEL = ROW?.PERSONEL;
                    }
                }
            }
        }
        private void tASKSDataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && tASKSDataGrid.SelectedItem != null)
            {
                if (tASKSDataGrid.Items.Count > 0)
                    CURRENT_ROW_INDEX = tASKSDataGrid.SelectedIndex;

                if (!(e is null) && tASKSDataGrid.SelectedItem is not null)
                {
                    if (tASKSDataGrid.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((TASKS)tASKSDataGrid.SelectedItem).Clone() as TASKS;
                    }
                }
            }
        }
        private void tASKSDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
        private void tASKSDataGrid_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                DG_TASKS_IsFocused = false;
            }
            else
            {
                DG_TASKS_IsFocused = true;
            }
        }

        private void AnjamNashodeh_Click(object sender, RoutedEventArgs e)
        {
            LoadRefresherSettings();

            DoLoadKartabl();
        }
        private void AnjamShode_Click(object sender, RoutedEventArgs e)
        {
            AutoRefreshToggle.IsChecked = false; //to avoid nested loading cause slowing

            DoLoadKartabl();
        }
        private void LaghvShodeh_Click(object sender, RoutedEventArgs e)
        {
            AutoRefreshToggle.IsChecked = false; //to avoid nested loading cause slowing

            DoLoadKartabl();
        }
        private void Hameh_Click(object sender, RoutedEventArgs e)
        {
            AutoRefreshToggle.IsChecked = false; //to avoid nested loading cause slowing

            DoLoadKartabl();
        }
        private void EVENT_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IDNUM1.Text) && IDNUM1.Text != "0")
            {
                var _idnum = Convert.ToInt64((tASKSDataGrid.SelectedItem as TASKS).IDNUM);
                new WinEVENTS(_idnum).ShowDialog();
            }
        }
        private void Label_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TASK,PERIORITY,STATUS,COMP_COD

            TASK.Text = null;
            PERIORITY.SelectedIndex = 0;
            STATUS.SelectedIndex = 0;

        }
        private void PERSONEL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PERSONEL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            var PERSONEL_TEX = (TextBox)PERSONEL.Template.FindName("PART_EditableTextBox", PERSONEL);
            if (PERSONEL.Items.OfType<COMBOPERSONEL>().Any(cbi => cbi.SAL_NAME.FixPersianChars().Equals(PERSONEL_TEX.Text.FixPersianChars())))
            {
                return;
            }
            else
            {
                /*Continue*/
            }

            if (!string.IsNullOrEmpty(PERSONEL_TEX.Text.Trim()))
            {
                new SelectUser(PERSONEL_TEX.Text, new WindowInteropHelper(this).Handle).ShowDialog();
            }

            if (PERSONEL.SelectedValue is null)
            {
                PERSONEL.SelectedValue = LastValidPERSONEL;
                PERSONEL.Items.Refresh();

                try
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PERSONEL.PreviewLostKeyboardFocus -= PERSONEL_PreviewLostKeyboardFocus;
                        if (!PERSONEL.IsKeyboardFocusWithin)
                        {
                            PERSONEL.Focus();
                            Keyboard.Focus(PERSONEL);
                        }
                        PERSONEL.PreviewLostKeyboardFocus += PERSONEL_PreviewLostKeyboardFocus;
                    }), DispatcherPriority.ContextIdle);
                }
                catch { }
            }

            if (PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow("مجری نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
        }
        private void FRESH_BTN_Click(object sender, RoutedEventArgs e)
        {
            ClearMasterTop();

            TASK.Focus();
        }

        private void ClearMasterTop()
        {
            ClearSelectedRowsAsGroup();

            tASKSDataGrid.SelectedItem = null;
            tASKSDataGrid.SelectedIndex = -1;

            PERSONEL.SelectedValue = null;
            PERSONEL.SelectedValue = UNVDER_PERSONEL.SelectedValue != null ? UNVDER_PERSONEL.SelectedValue : Baseknow.USERCOD;

            COMP_COD.SelectedValue = null;
            COMP_COD.SelectedValue = DEFAULTVAL_COMPCODE;

            TASK.Text = null;

            STATUS.SelectedValue = null;
            PERIORITY.SelectedValue = null;

            STATUS.SelectedValue = 1; STATUS.Items.Refresh();
            PERIORITY.SelectedValue = 2; PERIORITY.Items.Refresh();
        }
        private void ClearSelectedRowsAsGroup()
        {
            var selectedTasks = TASK_DATA.Where(task => task.IsSelected).ToList();
            if (selectedTasks.Any())
            {
                foreach (var item in selectedTasks)
                {
                    item.IsSelected = false;
                }
            }
        }
        private void COMP_COD_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (COMP_COD.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            var CUTSNO_TEX = (TextBox)COMP_COD.Template.FindName("PART_EditableTextBox", COMP_COD);

            if (CUTSNO_TEX.Text.Trim() == "+") // با مثبت
            {
                ComboSearch CMBSearch = new ComboSearch("MAIN", I_AM_MAIN);
                CMBSearch.ShowDialog();
            }
            //else if (WAS_COMP_COD != CUTSNO_TEX.Text) // متنش با متن قبلی مورد تایید فرق کرده و میخوایم با فقط تایپ شدن جستجو کنه
            else if (DoesTextedOnCOMP_COD is true)
            {
                CL_HESAB_SEARCH.Go_Search_Hesab(CUTSNO_TEX.Text, "MAIN", I_AM_MAIN);
            }

            if (COMP_COD.SelectedValue is null)
            {
                universControl.PopNotifyShow("تماس گیرنده نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
            }
            else
            {
                WAS_COMP_COD = COMP_COD.Text; //اگر انتخاب جدید درست بوده بیا مقداری که باهاش مقدار مورد تایید قبلی رو برای جستجو مقایسه میکنی رو بروز کن
            }
            DoesTextedOnCOMP_COD = false;
        }
        private void COMP_COD_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Delete || e.Key is Key.Back)
            {
                DoesTextedOnCOMP_COD = true;
            }
        }
        private void COMP_COD_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DoesTextedOnCOMP_COD = true;
        }
        private void Taeed_automasionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null) && (!string.IsNullOrEmpty(IDNUM1.Text) && IDNUM1.Text != "0"))
                {
                    if ((btn.Tag as TASKS)?.IDNUM is not null)
                    {
                        var Row = btn.Tag as TASKS;
                        if (Row != null && Row?.IDNUM > 0)
                        {
                            new WinEVENTS((long)Row.IDNUM).ShowDialog();
                        }
                    }
                }
            }
        }
        private void PERSONEL_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (NowIsReady)
            //{
            //    var comboBox = sender as ComboBox;
            //    if (comboBox != null)
            //    {
            //        var text = comboBox.Text;
            //        // Check if the entered text is valid
            //        bool isValid = comboBox.Items.OfType<COMBOPERSONEL>().Any(item => item.SAL_NAME.FixPersianChars().Equals(text.FixPersianChars()));
            //        LastValidPERSONEL = comboBox.SelectedValue;
            //    }
            //}
        }

        private void OpenWinBaseRow()
        {
            if (!CanUisBeActive) //Wait till Refreshing be done
            {
                return;
            }

            var SelectedRow = tASKSDataGrid.SelectedItem as TASKS;

            if (dbms != null && SelectedRow != null && SelectedRow.IDNUM > 0 && SelectedRow.skid != null && SelectedRow.num != null)
            {
                if (SelectedRow.num == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        universControl.PopNotifyShow($"شماره سند درج شده صفر است !", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    });
                }
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, (int)SelectedRow.skid, SelectedRow.num, true, _isCalledFromAutomasion_: true);
            }
        }
        private void BTN_SENDPM_Click(object sender, RoutedEventArgs e)
        {
            bool isMESSAGEPANELOpen = Application.Current.Windows.OfType<WIN_MESSAGEPANEL>().Any();
            bool isInboxPanOpen = Application.Current.Windows.OfType<INBOXPAN>().Any();

            if (!isMESSAGEPANELOpen && !isInboxPanOpen)
            {
                new WIN_MESSAGEPANEL(Convert.ToInt64(PERSONEL.SelectedValue)).Show();
            }
        }
        private void BTN_REMIND_Click(object sender, RoutedEventArgs e)
        {
            bool isREMINDERPANELOpen = Application.Current.Windows.OfType<REMINDERPANEL>().Any();
            bool isInboxPanOpen = Application.Current.Windows.OfType<INBOXPAN>().Any();

            if (!isREMINDERPANELOpen && !isInboxPanOpen)
            {
                new REMINDERPANEL(Convert.ToInt64(PERSONEL.SelectedValue)).Show();
            }
        }
        private void BTN_SMSSEND_Click(object sender, RoutedEventArgs e)
        {

            bool isMESSAGEPANEL_CUSTOMEROpen = Application.Current.Windows.OfType<MESSAGEPANEL_CUSTOMER>().Any();
            bool isInboxPanOpen = Application.Current.Windows.OfType<INBOXPAN>().Any();

            if (!isMESSAGEPANEL_CUSTOMEROpen && !isInboxPanOpen)
            {
                new MESSAGEPANEL_CUSTOMER(Convert.ToInt64(PERSONEL.SelectedValue)).Show();
            }

        }
        private void BTN_SENTS_Click(object sender, RoutedEventArgs e)
        {
            bool isWIN_EVENTS_SENDSOpen = Application.Current.Windows.OfType<WIN_EVENTS_SENDS>().Any();
            if (!isWIN_EVENTS_SENDSOpen)
            {
                new WIN_EVENTS_SENDS().Show();
            }
        }

        private void BTN_SEEMSG_Click(object sender, RoutedEventArgs e)
        {
            bool isMESSAGE_DTLOpen = Application.Current.Windows.OfType<WIN_MESSAGE_DTL>().Any();
            if (!isMESSAGE_DTLOpen)
            {
                new WIN_MESSAGE_DTL().Show();
            }
        }

        private void BTN_SEEREMIND_Click(object sender, RoutedEventArgs e)
        {
            bool isREMIND_DTLOpen = Application.Current.Windows.OfType<WIN_REMIND_DTL>().Any();
            if (!isREMIND_DTLOpen)
            {
                new WIN_REMIND_DTL().Show();
            }
        }

        // Methods to save and load settings
        private void SaveRefresherSettings()
        {
            try
            {
                Properties.Settings.Default.IsAutoRefreshEnabled = (AutoRefreshToggle.IsChecked ?? false);
                if (RefreshIntervalCombo.SelectedValue != null)
                {
                    Properties.Settings.Default.RefreshInterval = Convert.ToInt32(RefreshIntervalCombo.SelectedValue);
                }
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        private void LoadRefresherSettings()
        {
            try
            {
                AutoRefreshToggle.IsChecked = Properties.Settings.Default.IsAutoRefreshEnabled;
                RefreshIntervalCombo.SelectedValue = Properties.Settings.Default.RefreshInterval; RefreshIntervalCombo.Items.Refresh();
            }
            catch { }
        }

        private void AutoRefreshToggle_Click(object sender, RoutedEventArgs e)
        {
            if (NowIsReady)
            {
                SaveRefresherSettings();
            }
        }

        private void tASKSDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (tASKSDataGrid?.SelectedItem == null)
            {
                e.Handled = true; // Prevent the context menu from opening
            }
        }

        private void EnsureDataGridEditsCommitted(DataGrid grid)
        {
            if (grid == null) return;

            // Attempt to commit any edit at the cell and row level
            grid.CommitEdit(DataGridEditingUnit.Row, true);

            // More thorough: Check the IEditableCollectionView
            if (grid.Items is IEditableCollectionView editableItems)
            {
                if (editableItems.IsAddingNew)
                {
                    editableItems.CommitNew();
                }
                else if (editableItems.IsEditingItem)
                {
                    editableItems.CommitEdit();
                }
            }
        }

        private async void UpdateSelectedRowsBTN_Click(object sender, RoutedEventArgs e)
        {
            var BEFORE_TOGGLE = AutoRefreshToggle.IsChecked;
            AutoRefreshToggle.IsChecked = false;
            CanUisBeActive = false;

            if (!TASK_DATA.Any())
            {
                return;
            }

            try
            {
                var selectedTasks = TASK_DATA.Where(task => task.IsSelected).ToList();

                if (!selectedTasks.Any())
                {
                    new Msgwin(false, "هیچ ردیفی انتخاب نشده است.").Show();
                    return;
                }

                // Get selected PERSONEL from ComboBox
                if (PERSONEL.SelectedValue == null)
                {
                    new Msgwin(false, "لطفاً مجری را انتخاب کنید.").Show();
                    return;
                }


                Msgwin msgwin = new Msgwin(true, "آیا از بروز رسانی گروهی مطمئن هستید ؟"); msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }

                EnsureDataGridEditsCommitted(tASKSDataGrid);

                // Update each selected row's PERSONEL
                foreach (TASKS? task in selectedTasks)
                {
                    _ = GoUpdateAutomasion(task);
                }

                await DoLoadKartabl();
                new Msgwin(false, "سطر های انتخاب شده برای مجری , اولویت , وضعیت تغییر یافته و ذخیره شد.").ShowDialog();
            }
            catch (Exception ex)
            {
                if (ex is SqlException sqlEx && CL_CCNNMANAGER.IsConnectionRelated(sqlEx))
                {
                    CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    throw;
                }
                new Msgwin(false, "خطا در انجام بروز رسانی گروهی").ShowDialog();
            }
            finally
            {
                AutoRefreshToggle.IsChecked = BEFORE_TOGGLE;
                CanUisBeActive = true;
            }
        }

        private void COLUMN_SELECTIVE_Click(object sender, RoutedEventArgs e)
        {
            if (tASKSDataGrid.IsEnabled && TASK_DATA.Count != 0)
            {
                //var selectedTasks = TASK_DATA.Where(task => task.IsSelected).ToList();
                //if (!selectedTasks.Any())
                //{
                //    ClearMasterTop();
                //}


                AutoRefreshToggle.IsChecked = false;
            }
        }

        private void UNVDER_PERSONEL_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (UNVDER_PERSONEL.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (NowIsReady && UNVDER_PERSONEL.SelectedIndex > -1)
            {
                DoLoadKartabl();
            }
        }

        private void tASKSDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
            }

            // Find the row under the mouse
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            DataGridRow row = dep as DataGridRow;
            if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
            {
                // Select the row under the mouse
                dataGrid.SelectedItem = row.Item;

                // Show the context menu
                dataGrid.ContextMenu.IsOpen = true;

                // Mark the event as handled to prevent the default context menu behavior
                e.Handled = true;
            }
            else
            {
                // No valid row, don't show context menu
                e.Handled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string compCod = COMP_COD.SelectedValue?.ToStringNullSafe();

            if (string.IsNullOrEmpty(compCod) || compCod == "0")
            {
                new Msgwin(false, "مشتري مشخص نشده است ....!").ShowDialog();
                return;
            }

            if (CL_HESABDARI.BLOCKEDMK(compCod))
            {
                new Msgwin(false, "حساب مورد نظر مسدود مي باشد!").ShowDialog();
                return;
            }

            try
            {
                string[] codeParts = compCod.Split('-');
                string tNumber = codeParts.LastOrDefault();

                if (!string.IsNullOrEmpty(tNumber))
                {
                    new TASKS_COMPANY(compCod).ShowDialog();
                }
                else
                {
                    new Msgwin(false, "کد مشتری معتبر نمی باشد.").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در باز کردن پرونده مشتری").ShowDialog();
            }
        }
    }
}
