using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static Dapper.SqlMapper;

namespace Prg_UI.Wins.WinSetting
{
    public partial class WinConnectionChoose : Window
    {
        #region Header Window Begin
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
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗

                    WindowState = WindowState.Normal;
                    TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
                    break;
                case WindowState.Normal:

                    WindowState = WindowState.Maximized;
                    TitleDrawBar.CornerRadius = new CornerRadius(0);
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
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WinConnectionChoose()
        {
            InitializeComponent();
        }
        private string CNN_STR { get; set; } = null;
        public static List<string> GetAllSqlServerNames()
        {
            // Create a new list to hold the server names
            List<string> servers = new List<string>();
            servers = SqlServerScanner.GetAllSqlServerNames();
            return servers;
        }

        private void Btn_SaveConnection_Click(object sender, RoutedEventArgs e)
        {
            var serverChooserTextBox = ServerChooser.Template?.FindName("PART_EditableTextBox", ServerChooser) as TextBox;
            string ServerChooser_TEX = serverChooserTextBox?.Text?.Trim() ?? ServerChooser.Text?.Trim() ?? string.Empty;

            var DbChooser_TEXBOX = DbChooser.Template?.FindName("PART_EditableTextBox", DbChooser) as TextBox;
            var DbChooser_TEX = DbChooser_TEXBOX?.Text?.Trim() ?? DbChooser.Text?.Trim() ?? string.Empty;
            DbChooser.SelectedValue = DbChooser_TEX;

            if (string.IsNullOrEmpty(ServerChooser_TEX))
            {
                new Msgwin(false, "نام سرور خالی است").ShowDialog();
                return;
            }
            if (string.IsNullOrEmpty(DbChooser_TEX.ToStringNullSafe()))
            {
                new Msgwin(false, "نام دیتابیس خالی است").ShowDialog();
                return;
            }

            try
            {
                if ((CL_Generaly.General_Servername ?? string.Empty).Trim() == ServerChooser_TEX.Trim() && (CL_Generaly.General_DBname ?? string.Empty).Trim() == DbChooser_TEX.Trim())
                {
                    this.Close();
                    return;
                }
            }
            catch { /*ignore*/ return; }

            if (rd_WinAuth.IsChecked is true) //Windows Authentication
                CNN_STR = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"; //WIN
            else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                CNN_STR = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Password};Integrated Security=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"; // SQL

            try
            {
                CL_CCNNMANAGER.CONNECTION_STR = CNN_STR; // قرار دادن رشته اتصال نهایی به کلاس اصلی

                var TestCnn = dbms.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN").FirstOrDefault(); //تست نهایی برای اطمینان

                CL_CCNNMANAGER.ConnectedToSQLDB = true;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در اتصال به دیتابیس !").ShowDialog();
                return;
            }
            dbms.CreateConnectionSpecifyNameApp(CNN_STR);
            new Msgwin(false, "با موفق به دیتابیس متصل شد , برنامه باید یکبار ری استارت شود.").ShowDialog();
            this.Close();

            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExecutablePath);

            CL_LMethods.CleanupBeforeExiting();

            Application.Current.Shutdown();
            CL_LMethods.GoExitTheApplication(); //#NABILOO#
            return;
        }
        internal (bool Success, string ErrorMessage) GoTestConnectionOK()
        {
            string _cnn = null;
            string serverName = null;
            string dbName = null;
            bool isWindowsAuth = false;

            Dispatcher.Invoke(() =>
            {
                ServerChooser.UpdateLayout();
                var ServerChooser_TEXBOX = (TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser);
                ServerChooser.SelectedValue = ServerChooser_TEXBOX.Text.Trim();
                serverName = ServerChooser_TEXBOX.Text.Trim();

                DbChooser.UpdateLayout();
                var DbChooser_TEXBOX = (TextBox)DbChooser.Template.FindName("PART_EditableTextBox", DbChooser);
                DbChooser.SelectedValue = DbChooser_TEXBOX.Text.Trim();
                dbName = DbChooser_TEXBOX.Text.Trim();

                if (rd_WinAuth.IsChecked is true) //Windows Authentication
                {
                    isWindowsAuth = true;
                    _cnn = $@"Data Source={serverName};Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=15;"; //WIN
                }
                else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                    _cnn = $@"Data Source={serverName};Initial Catalog={dbName};User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Password};Integrated Security=False;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=15;"; // SQL
            });

            if (string.IsNullOrWhiteSpace(serverName))
                return (false, "نام سرور خالی است.\nلطفاً نام سرور SQL Server را وارد کنید.");

            if (string.IsNullOrWhiteSpace(dbName))
                return (false, "نام دیتابیس خالی است.\nلطفاً نام دیتابیس را وارد کنید.");

            try
            {
                using var conn = new SqlConnection(_cnn);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT SERVERNAM FROM dbo.SAZMAN";
                cmd.CommandTimeout = 30;
                var result = cmd.ExecuteScalar();
                return (true, null);
            }
            catch (SqlException ex)
            {
                return (false, BuildSqlErrorMessage(ex, serverName, dbName, isWindowsAuth));
            }
            catch (InvalidOperationException)
            {
                return (false, "تنظیمات اتصال ناقص است.\nلطفاً نام سرور و دیتابیس را کامل وارد کنید.");
            }
            catch (ArgumentException)
            {
                return (false, "فرمت رشته اتصال نامعتبر است.\nنام سرور یا دیتابیس حاوی کاراکتر غیرمجاز است.");
            }
            catch (Exception ex)
            {
                return (false, $"خطا در برقراری اتصال:\n{ex.Message}");
            }
        }

        private static string BuildSqlErrorMessage(SqlException ex, string serverName, string databaseName, bool isWindowsAuth)
        {
            if (ex == null)
                return "خطا در اتصال به دیتابیس رخ داد.\nراهنما: اطلاعات اتصال را بررسی کرده و مجدد تلاش کنید.";

            // بررسی خطاهای Win32 (سطح شبکه / سیستم‌عامل)
            if (ex.InnerException is System.ComponentModel.Win32Exception win32)
            {
                return win32.NativeErrorCode switch
                {
                    10061 => $"سرویس SQL Server روی «{serverName}» در حال اجرا نیست یا اتصال رد شد.\n\nراه‌حل پیشنهادی:\n• از طریق Services.msc سرویس SQL Server را راه‌اندازی کنید.\n• مطمئن شوید پورت 1433 در فایروال باز است.",
                    53 => $"مسیر شبکه به سرور «{serverName}» یافت نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور را بررسی کنید.\n• مطمئن شوید سرور روشن و در شبکه قرار دارد.",
                    64 => $"اتصال شبکه به سرور «{serverName}» قطع شد.\n\nراه‌حل پیشنهادی:\n• کابل شبکه یا Wi-Fi را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.",
                    10060 => $"اتصال به سرور «{serverName}» با خطای وقفه زمانی مواجه شد.\n\nراه‌حل پیشنهادی:\n• آدرس IP یا نام سرور را بررسی کنید.\n• فایروال را بررسی کنید.",
                    10054 => $"اتصال به سرور «{serverName}» توسط طرف مقابل Reset شد.\n\nراه‌حل پیشنهادی:\n• سرویس SQL Server را ریستارت کنید.\n• تنظیمات شبکه و فایروال را بررسی کنید.",
                    10053 => $"اتصال به سرور «{serverName}» توسط شبکه قطع شد.\n\nراه‌حل پیشنهادی:\n• پایداری شبکه را بررسی کنید.\n• VPN یا Proxy را بررسی کنید.",
                    1722 => $"سرویس RPC روی سرور «{serverName}» در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید سرویس SQL Server در حال اجرا است.\n• Named Pipes را در SQL Server Configuration Manager فعال کنید.",
                    5 => $"دسترسی به سرور «{serverName}» رد شد.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید حساب کاربری جاری مجاز به اتصال است.\n• تنظیمات فایروال سرور را بررسی کنید.",
                    _ => $"خطای شبکه (کد: {win32.NativeErrorCode}):\n{win32.Message}\n\nراه حل : با مدیر شبکه تماس بگیرید (اگر هیچ کاربری نمیتواند متصل شود , ممکن است سرویس اصلی مربوط به SQL Server متوقف شده باشد).",
                };
            }

            switch (ex.Number)
            {
                // ─── یافت نشدن سرور ───
                case 2:
                case 53:
                case 40:
                case 11001:
                    return $"سرور SQL Server «{serverName}» یافت نشد یا در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• نام سرور را دوباره بررسی کنید.\n• مطمئن شوید سرور روشن است و در شبکه قرار دارد.\n• پورت 1433 باید در فایروال باز باشد.";

                // ─── نام یا Instance اشتباه ───
                case 26:
                    return $"نام سرور یا Instance «{serverName}» پیدا نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور و Instance را بررسی کنید (مثال: SERVER\\SQLEXPRESS).\n• مطمئن شوید سرویس SQL Server Browser روشن است.";

                // ─── وقفه زمانی ───
                case -2:
                case 121:
                case 258:
                    return $"اتصال به سرور «{serverName}» با وقفه زمانی (Timeout) قطع شد.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید سرور در دسترس است.\n• شبکه و VPN را بررسی کنید.\n• فایروال ممکن است اتصال را مسدود کرده باشد.";

                // ─── قطع شدن اتصال ───
                case 64:
                    return $"اتصال به سرور «{serverName}» قطع شد.\n\nراه‌حل پیشنهادی:\n• شبکه را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.";

                // ─── Named Pipe / Transport ───
                case 232:
                case 233:
                    return "خطای ارتباط با Named Pipe/Transport سرور رخ داد.\n\nراه‌حل پیشنهادی:\n• Named Pipes و TCP/IP را در SQL Server Configuration Manager فعال کنید.\n• سرویس SQL Server را ریستارت کنید.";

                // ─── دیتابیس یافت نشد یا دسترسی ندارید ───
                case 4060:
                    return $"دیتابیس «{databaseName}» یافت نشد یا دسترسی ندارید.\n\nراه‌حل پیشنهادی:\n• نام دیتابیس را بررسی کنید.\n• مطمئن شوید کاربر به این دیتابیس دسترسی دارد.";

                case 4064:
                    return "دیتابیس پیش‌فرض کاربر در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید تا دیتابیس پیش‌فرض کاربر را تنظیم کند.";

                // ─── خطاهای ورود / احراز هویت ───
                case 18456:
                    if (isWindowsAuth)
                        return "ورود ناموفق بود (Windows Authentication).\n\nراه‌حل پیشنهادی:\n• حساب ویندوز فعلی باید روی SQL Server دسترسی داشته باشد.\n• در صورت نیاز از SQL Authentication استفاده کنید.";
                    return "ورود به SQL Server ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری SQL را بررسی کنید.\n• رمز عبور را مجدداً وارد کنید.\n• مطمئن شوید کاربر در SQL Server تعریف شده است.";

                case 18452:
                    return "ورود با Windows Authentication ناموفق بود چون کلاینت در Domain مورد اعتماد SQL Server نیست.\n\nراه‌حل پیشنهادی:\n• اگر سرور با IP یا از شبکه/Domain دیگر وصل می‌شود، SQL Authentication را انتخاب کنید.\n• در صورت نیاز از مدیر شبکه بخواهید Trust بین Domainها را تنظیم کند.\n• اگر باید Windows Auth استفاده شود، برنامه را با کاربر دامنه مجاز اجرا کنید.";

                case 18470:
                    return "حساب کاربری SQL غیرفعال شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر SQL Server تماس بگیرید تا حساب کاربری را فعال کند.";

                case 18487:
                    return "رمز عبور حساب کاربری منقضی شده است.\n\nراه‌حل پیشنهادی:\n• رمز عبور SQL را از طریق SQL Server Management Studio تغییر دهید.\n• با مدیر SQL Server تماس بگیرید.";

                case 18488:
                    return "رمز عبور حساب کاربری باید تغییر کند (اولین ورود).\n\nراه‌حل پیشنهادی:\n• یک بار از طریق SSMS با این کاربر وارد شوید و رمز عبور جدید تعیین کنید.";

                case 18401:
                    return $"سرور «{serverName}» در حالت تک‌کاربره (Single-User Mode) است.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید تا SQL Server را به حالت عادی برگردانند.\n• سرویس SQL Server را ریستارت کنید.";

                case 1326:
                    return "اعتبارسنجی کاربر در شبکه/دامنه ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری و رمز عبور ویندوز/دامنه را بررسی کنید.\n• از دسترسی شبکه و Domain Controller مطمئن شوید.";

                // ─── دسترسی به اشیای دیتابیس ───
                case 229:
                case 916:
                    return $"کاربر دسترسی لازم به دیتابیس «{databaseName}» یا اشیای آن را ندارد.\n\nراه‌حل پیشنهادی:\n• از مدیر دیتابیس بخواهید دسترسی‌های لازم (حداقل خواندن جدول SAZMAN) را اعطا کند.";

                // ─── ساختار دیتابیس ───
                case 208:
                    return $"جدول «dbo.SAZMAN» در دیتابیس «{databaseName}» یافت نشد.\n\nراه‌حل پیشنهادی:\n• دیتابیس صحیح برنامه را انتخاب کنید.\n• اسکریپت‌های ایجاد/به‌روزرسانی جداول را اجرا کنید.";

                case 207:
                    return $"ساختار دیتابیس «{databaseName}» با نسخه برنامه سازگار نیست (ستون لازم وجود ندارد).\n\nراه‌حل پیشنهادی:\n• اسکریپت‌های به‌روزرسانی دیتابیس را اجرا کنید یا نسخه برنامه را همسان کنید.";

                // ─── تداخل / قفل ───
                case 1205:
                    return "تداخل در تراکنش‌های دیتابیس (Deadlock) رخ داده است.\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کنید و دوباره تست کنید.\n• اگر مشکل ادامه داشت با مدیر دیتابیس تماس بگیرید.";

                case 1222:
                    return "درخواست قفل دیتابیس با وقفه زمانی مواجه شد.\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کرده و دوباره تست کنید.\n• اگر مشکل تکرار شد با مدیر دیتابیس تماس بگیرید.";

                // ─── وضعیت سرور ───
                case 17142:
                    return "سرویس SQL Server موقتاً متوقف شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید تا سرویس SQL Server را راه‌اندازی کند.";

                case 9002:
                    return $"فضای لاگ تراکنش دیتابیس «{databaseName}» پر شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید تا فضای لاگ را آزاد کنند.\n• پشتیبان‌گیری از لاگ یا تغییر Recovery Model می‌تواند کمک کند.";

                default:
                    return $"خطای SQL Server (کد: {ex.Number}):\n{ex.Message}\n\nراه‌حل: تنظیمات سرور/شبکه/دسترسی را بررسی کنید یا با مدیر سیستم تماس بگیرید.";
            }
        }

        private async void Btn_TestConnection_Click(object sender, RoutedEventArgs e)
        {
            lblconnecting.Visibility = Visibility.Visible;
            this.IsEnabled = false;

            (bool Success, string ErrorMessage) result = default;
            await Task.Run(() =>
            {
                result = GoTestConnectionOK();
            });

            if (result.Success)
                new Msgwin(false, "اتصال با موفقیت برقرار شد!", "#FF1A7A3C").ShowDialog();
            else
                new Msgwin(false, result.ErrorMessage, "", true).ShowDialog();

            lblconnecting.Visibility = Visibility.Hidden;
            this.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            {
                ServerChooser.SelectionChanged -= ServerChooser_SelectionChanged; //برای ایکه رویداد الکی اجرا نشه تداخل درست کنه

                ServerChooser.Items.Add(CL_Generaly.General_Servername);
                DbChooser.Items.Add(CL_Generaly.General_DBname);

                if (!string.IsNullOrWhiteSpace(CL_Generaly.General_Username))
                {
                    Textbox_DataUsername.Text = CL_Generaly.General_Username;
                }
                if (!string.IsNullOrWhiteSpace(CL_Generaly.General_Password))
                {
                    Textbox_Datapass.Password = CL_Generaly.General_Password;
                }

                ServerChooser.SelectionChanged += ServerChooser_SelectionChanged; //برای ایکه رویداد الکی اجرا نشه تداخل درست کنه
            }
        }

        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string sname = dbms.DoGetDataSQL<string>("Select @@servername as [ServerName]").FirstOrDefault().ToString();
                string dname = dbms.DoGetDataSQL<string>("SELECT DB_NAME() AS [Current Database]").FirstOrDefault().ToString();
                new Msgwin(false, $"Your ServerName Conneted : {sname} \n Your Database Name Connected : {dname}").Show();
            }
            catch (Exception) { }
        }

        private async void Btn_GetServers_Click(object sender, RoutedEventArgs e)
        {
            Btn_GetServers.IsEnabled = false;
            PgbScanLoading.Visibility = Visibility.Visible;

            ServerChooser.SelectionChanged -= ServerChooser_SelectionChanged;
            ServerChooser.ItemsSource = null;
            ServerChooser.Items?.Clear();
            DbChooser.ItemsSource = null;
            DbChooser.Items?.Clear();
            DbChooser.SelectedIndex = -1;

            List<string> servers = null;
            try
            {
                await Task.Run(() => { servers = SqlServerScanner.GetAllSqlServerNames(); });

                ServerChooser.ItemsSource = servers;
                if (servers?.Count > 0)
                    ServerChooser.SelectedIndex = 0;

                var found = servers?.Count > 0;
                ShowToast(
                    found ? $"{servers.Count} سرور SQL پیدا شد" : "هیچ سرور SQL یافت نشد",
                    found ? "#DD1A7A3C" : "#DDB71C1C"
                );
            }
            catch { /* scanner errors are non-fatal; UI is always reset in finally */ }
            finally
            {
                ServerChooser.SelectionChanged += ServerChooser_SelectionChanged;
                PgbScanLoading.Visibility = Visibility.Collapsed;
                Btn_GetServers.IsEnabled = true;
            }
        }

        private void ShowToast(string message, string colorHex)
        {
            TxtToast.Text = message;
            BrdToast.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            BrdToast.Visibility = Visibility.Visible;

            var storyboard = new Storyboard();

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            Storyboard.SetTarget(fadeIn, BrdToast);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromSeconds(3.5)
            };
            Storyboard.SetTarget(fadeOut, BrdToast);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            fadeOut.Completed += (s, ev) => BrdToast.Visibility = Visibility.Collapsed;

            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(fadeOut);
            storyboard.Begin(this);
        }

        private void GetDBSFromServer()
        {
            try
            {
                string ServerChooser_TEX = ((TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser)).Text;

                var _TMPCNN = "";
                if (rd_WinAuth.IsChecked is true) //Windows Authentication
                    _TMPCNN = $@"Data Source={ServerChooser_TEX};Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;"; //WIN
                else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                    _TMPCNN = $@"Data Source={ServerChooser_TEX};Initial Catalog=master;User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Password};Integrated Security=False;"; // SQL

                using (IDbConnection db = new SqlConnection(_TMPCNN))
                {
                    db.Open();
                    var LastSelectedDatabase = DbChooser.SelectedValue;

                    var commandDefinition = new CommandDefinition("SELECT name from sys.databases where name not in ('master', 'model', 'tempdb', 'msdb') order by name", parameters: null, commandTimeout: 300);
                    var results = db.Query<string>(commandDefinition).ToList();
                    DbChooser.ItemsSource = null;
                    DbChooser.Items?.Clear();
                    DbChooser.ItemsSource = results;
                    //DbChooser.SelectedIndex = 0;
                    DbChooser.SelectedValue = LastSelectedDatabase;
                    db?.Close();
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا عدم دسترسی به سرور").ShowDialog();
                DbChooser.ItemsSource = null;
                DbChooser.Items?.Clear();
                DbChooser.SelectedIndex = -1;
            }
        }
        private void ServerChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ServerChooser.SelectedIndex > -1)
            //{
            //    GetDBSFromServer();
            //}
        }

        private void DbChooser_DropDownOpened(object sender, EventArgs e)
        {
            var ServerChooser_TEX = (TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser);
            if (!string.IsNullOrEmpty(ServerChooser_TEX.Text.Trim()))
            {
                GetDBSFromServer();
            }
            else
            {
                DbChooser.ClearValue(ItemsControl.ItemsSourceProperty);
                DbChooser.ItemsSource = null;
                //DbChooser.Items?.Clear();
            }
        }
    }
}
