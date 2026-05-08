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

            // قبل از switch اصلی، در ابتدای متد
            if (ex.Message.Contains("Cannot generate SSPI context", StringComparison.OrdinalIgnoreCase))
                return "خطای Kerberos: SSPI Context تولید نشد.\n\nراه‌حل پیشنهادی:\n• ساعت سیستم را با Domain هماهنگ کنید.\n• از SQL Authentication استفاده کنید.\n• اگر VPN فعال است قطع/وصل کنید.";

            // قبل از switch، بعد از null check
            if (ex.Message.Contains("certificate chain", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("SSL Provider", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("pre-login handshake", StringComparison.OrdinalIgnoreCase))
            {
                return $"خطای رمزنگاری/TLS با سرور «{serverName}».\n\nراه‌حل پیشنهادی:\n• مطمئن شوید TrustServerCertificate=True در رشته اتصال هست.\n• نسخه TLS سرور را با کلاینت هماهنگ کنید (TLS 1.2 به بعد).\n• اگر سرور قدیمی است، Encrypt=False امتحان کنید.\n• گواهی SQL Server را به Trusted Root اضافه کنید.";
            }

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
                    1722 => $"سرویس RPC یا یکی از سرویس‌های وابسته روی سرور «{serverName}» در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید سرور روشن و در شبکه در دسترس است.\n• سرویس SQL Server و سرویس‌های وابسته را بررسی کنید.\n• تنظیمات فایروال و دسترسی شبکه را بررسی کنید.",
                    5 => $"دسترسی به سرور «{serverName}» رد شد.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید حساب کاربری جاری مجاز به اتصال است.\n• تنظیمات فایروال سرور را بررسی کنید.",
                    10013 => $"اتصال به سرور «{serverName}» توسط سیستم یا فایروال رد شد.\n\nراه‌حل پیشنهادی:\n• فایروال ویندوز و آنتی‌ویروس را بررسی کنید.\n• مطمئن شوید برنامه اجازه اتصال خروجی به پورت SQL Server را دارد.",
                    10049 => $"آدرس سرور «{serverName}» برای این سیستم معتبر نیست.\n\nراه‌حل پیشنهادی:\n• IP یا نام سرور را بررسی کنید.\n• اگر IP دستی وارد شده، مطمئن شوید اشتباه تایپی ندارد.",
                    10051 => $"شبکه مقصد برای اتصال به سرور «{serverName}» در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• اتصال شبکه، Gateway و VPN را بررسی کنید.\n• مطمئن شوید کلاینت و سرور در یک مسیر شبکه قابل دسترسی هستند.",
                    10065 => $"مسیر شبکه‌ای برای رسیدن به سرور «{serverName}» وجود ندارد.\n\nراه‌حل پیشنهادی:\n• Routing، VPN و Gateway را بررسی کنید.\n• اگر از IP بیرونی استفاده می‌کنید، Port Forwarding را بررسی کنید.",
                    11001 => $"نام سرور «{serverName}» در DNS پیدا نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور را بررسی کنید.\n• با IP سرور تست کنید.\n• تنظیمات DNS سیستم را بررسی کنید.",

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
                    var loginState = ex.Errors.Count > 0 ? ex.Errors[0].State : 0;
                    return loginState switch
                    {
                        2 or 5 => "کاربر در SQL Server تعریف نشده است.\n\nراه‌حل پیشنهادی:\n• نام کاربری را بررسی کنید.\n• از مدیر دیتابیس بخواهید Login جدید ایجاد کند.",
                        6 => "از حساب کاربری Windows برای SQL Authentication استفاده شده است.\n\nراه‌حل پیشنهادی:\n• گزینه Windows Authentication را انتخاب کنید.",
                        7 => "حساب کاربری غیرفعال است.\n\nراه‌حل پیشنهادی:\n• با مدیر SQL Server تماس بگیرید تا حساب را فعال کند.",
                        8 => "رمز عبور اشتباه است.\n\nراه‌حل پیشنهادی:\n• رمز عبور را با دقت وارد کنید (به Caps Lock و زبان کیبورد دقت کنید).",
                        9 => "ورود ناموفق بود به دلیل مشکل در اعتبارسنجی رمز عبور.\n\nراه‌حل پیشنهادی:\n• رمز عبور را دوباره بررسی کنید.\n• اگر مشکل ادامه داشت با مدیر SQL Server تماس بگیرید.",
                        11 or 12 => "اعتبارسنجی موفق بود اما کاربر اجازه دسترسی به سرور را ندارد.\n\nراه‌حل پیشنهادی:\n• از مدیر دیتابیس بخواهید کاربر را به Server Roles اضافه کند.",
                        18 => "رمز عبور حساب کاربری باید تغییر کند.\n\nراه‌حل پیشنهادی:\n• یک بار از طریق SSMS وارد شوید و رمز جدید تعیین کنید.",
                        38 => $"دیتابیس «{databaseName}» وجود ندارد یا کاربر دسترسی ندارد.\n\nراه‌حل پیشنهادی:\n• نام دیتابیس را بررسی کنید.\n• از مدیر دیتابیس درخواست دسترسی کنید.",
                        58 => "سرور فقط Windows Authentication را قبول می‌کند.\n\nراه‌حل پیشنهادی:\n• گزینه Windows Authentication را انتخاب کنید.\n• یا از مدیر دیتابیس بخواهید Mixed Mode را فعال کند.",
                        102 or 104 => "آدرس IP کلاینت در فایروال SQL Server مجاز نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس برای اضافه کردن IP به Whitelist هماهنگ کنید.",
                        _ => isWindowsAuth
                            ? "ورود ناموفق بود (Windows Authentication).\n\nراه‌حل پیشنهادی:\n• حساب ویندوز فعلی باید روی SQL Server دسترسی داشته باشد."
                            : "ورود به SQL Server ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری و رمز عبور را بررسی کنید."
                    };


                case 17806:  // SSPI handshake failed
                case 17807:  // Could not find an entry for SQL Server in service principal name
                case 17808:  // SSPI authentication error
                    return "خطای SSPI/Kerberos در احراز هویت Windows.\n\nراه‌حل پیشنهادی:\n• ساعت سیستم کلاینت را با Domain Controller هماهنگ کنید.\n• SPN در Active Directory را بررسی کنید.\n• به جای Windows Authentication از SQL Authentication استفاده کنید.\n• اگر سرور با IP وصل می‌شود، با نام DNS تست کنید.";

                case 18452:
                    return "ورود با Windows Authentication ناموفق بود زیرا حساب کاربری جاری برای احراز هویت یکپارچه (Integrated Security) مورد اعتماد SQL Server نیست.\n\nراه‌حل پیشنهادی:\n• اگر از شبکه/Domain دیگر یا IP مستقیم استفاده می‌کنید، SQL Authentication را انتخاب کنید.\n• در صورت نیاز از مدیر شبکه بخواهید Trust بین Domainها را بررسی کند.\n• اگر باید Windows Authentication استفاده شود، برنامه را با کاربر دامنه مجاز اجرا کنید.";

                case 18470:
                    return "حساب کاربری SQL غیرفعال شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر SQL Server تماس بگیرید تا حساب کاربری را فعال کند.";

                case 18487:
                    return "رمز عبور حساب کاربری منقضی شده است.\n\nراه‌حل پیشنهادی:\n• رمز عبور SQL را از طریق SQL Server Management Studio تغییر دهید.\n• با مدیر SQL Server تماس بگیرید.";

                case 18488:
                    return "رمز عبور حساب کاربری باید تغییر کند (اولین ورود).\n\nراه‌حل پیشنهادی:\n• یک بار از طریق SSMS با این کاربر وارد شوید و رمز عبور جدید تعیین کنید.";

                case 18401:
                    return $"سرور «{serverName}» در حالت تک‌کاربره (Single-User Mode) است.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید تا SQL Server را به حالت عادی برگردانند.\n• سرویس SQL Server را ریستارت کنید.";

                case 1326:
                    return "اعتبارسنجی کاربر ناموفق بود (نام کاربری یا رمز عبور نادرست است).\n\nراه‌حل پیشنهادی:\n• نام کاربری و رمز عبور را بررسی کنید.\n• اگر از Windows Authentication استفاده می‌کنید، حساب دامنه/ویندوز را بررسی کنید.";

                case 701:   // Out of memory
                case 802:   // Insufficient memory
                case 8645:  // Timeout waiting for memory resources
                case 8651:  // Low memory condition
                    return "حافظه سرور SQL Server کافی نیست.\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کرده و دوباره تست کنید.\n• با مدیر سرور برای افزایش RAM یا تنظیم max server memory هماهنگ کنید.";

                case 1105:  // Could not allocate space (filegroup full)
                case 1101:  // Could not allocate new page
                    return $"فضای فایل/Filegroup دیتابیس «{databaseName}» پر شده است.\n\nراه‌حل پیشنهادی:\n• فضای دیسک سرور را بررسی کنید.\n• Auto-Growth فایل دیتابیس را فعال یا افزایش دهید.\n• با مدیر دیتابیس تماس بگیرید.";

                case 9001:  // Log not available
                case 9004:  // Error while processing the log
                    return $"لاگ تراکنش دیتابیس «{databaseName}» دچار مشکل شده است.\n\nراه‌حل پیشنهادی:\n• فوراً با مدیر دیتابیس تماس بگیرید.\n• ممکن است نیاز به Restore یا Repair باشد.";

                case 109:   // Insufficient system memory in resource pool
                    return "منابع Resource Pool روی سرور کافی نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس برای تنظیم Resource Governor هماهنگ کنید.";

                case 6:     // Specified SQL server not found (legacy DBNETLIB)
                    return $"سرور «{serverName}» پیدا نشد (DBNETLIB).\n\nراه‌حل پیشنهادی:\n• نام سرور یا Instance را دوباره بررسی کنید.";

                case 4063:  // Cannot open user default database (variant of 4064)
                    return $"دیتابیس پیش‌فرض کاربر در دسترس نیست یا حذف شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس برای تغییر Default Database کاربر تماس بگیرید.";

                case 7202:  // Could not find server in sys.servers (Linked Server)
                    return "Linked Server مورد نیاز در سرور پیدا نشد.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس برای پیکربندی Linked Server هماهنگ کنید.";

                case 18406: // Login failed (DBNETLIB legacy)
                    return "ورود به SQL Server ناموفق بود (DBNETLIB).\n\nراه‌حل پیشنهادی:\n• از یک Driver جدیدتر استفاده کنید یا نام کاربری/رمز را بررسی کنید.";

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

                case 945: // Database cannot be opened
                    return $"دیتابیس «{databaseName}» قابل باز شدن نیست (ممکن است در حال Restore یا Suspect باشد).";

                case 5120: // File access error
                    return "خطا در دسترسی به فایل‌های دیتابیس (مجوز فایل).";

                case 18450:
                    return "ورود با Windows Authentication ناموفق بود.\n\nراه‌حل پیشنهادی:\n• حساب کاربری ویندوز/دامنه را بررسی کنید.\n• در صورت نیاز از SQL Authentication استفاده کنید.\n• اگر مشکل ادامه داشت با مدیر شبکه یا SQL Server تماس بگیرید.";

                case 18451:
                    return "ورود ناموفق بود. SQL Server اجازه ورود به این حساب کاربری را نداد.\n\nراه‌حل پیشنهادی:\n• وضعیت حساب کاربری و مجوز Login را بررسی کنید.\n• با مدیر Active Directory یا SQL Server تماس بگیرید.";

                case 18463:
                    return "ورود ناموفق بود. حساب کاربری برای ورود معتبر نیست یا منقضی شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر Active Directory یا SQL Server تماس بگیرید.";

                case 18464:
                    return "ورود ناموفق بود. رمز عبور حساب کاربری منقضی شده است.\n\nراه‌حل پیشنهادی:\n• رمز عبور را از طریق IT Helpdesk یا SSMS تغییر دهید.";

                case 18465:
                    return "ورود ناموفق بود. حساب کاربری در بازه زمانی مجاز برای ورود نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید.";

                // ─── خطاهای Azure SQL / Cloud (Transient Faults) ───
                case 40143:
                    return "سرویس SQL Server/Cloud موقتاً در دسترس نیست (Transient Fault).\n\nراه‌حل پیشنهادی:\n• چند ثانیه صبر کرده و دوباره تلاش کنید.\n• اگر مشکل ادامه داشت با پشتیبانی Azure تماس بگیرید.";

                // ─── خطاهای شبکه و ارتباطی عمومی ───
                case 11:
                    return $"خطای عمومی شبکه در ارتباط با سرور «{serverName}» رخ داد.\n\nراه‌حل پیشنهادی:\n• پایداری شبکه و تنظیمات کارت شبکه را بررسی کنید.";

                case 20:
                    return $"خطای رمزنگاری (Encryption) در ارتباط با سرور «{serverName}» رخ داد.\n\nراه‌حل پیشنهادی:\n• کلاینت از رمزنگاری پشتیبانی نمی‌کند یا تنظیمات SSL/TLS سرور نیاز به بازبینی دارد.";

                // ─── محدودیت‌های سرور ───
                case 10928:
                case 10929:
                    return $"سرور «{serverName}» به حداکثر ظرفیت اتصالات خود رسیده است.\n\nراه‌حل پیشنهادی:\n• چند دقیقه صبر کنید تا اتصالات قبلی آزاد شوند.\n• با مدیر دیتابیس برای افزایش سقف اتصالات (Resource Limits) هماهنگ کنید.";

                // ─── دیتابیس در دسترس نیست (وضعیت‌های خاص) ───
                case 922:
                    return $"دیتابیس «{databaseName}» در حال بازیابی (Recovery) است و فعلاً در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• چند دقیقه صبر کنید تا فرآیند Recovery سرور تکمیل شود و دوباره تلاش کنید.";

                case 942:
                    return $"دیتابیس «{databaseName}» در وضعیت Offline قرار دارد.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید تا دیتابیس را Online کند.";

                case 952:
                    return $"دیتابیس «{databaseName}» در حال انتقال وضعیت (Transition) است.\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کنید و دوباره تلاش کنید.";
                
                case 18486:
                    return "حساب کاربری SQL قفل شده است (تلاش‌های ناموفق متعدد).\n\nراه‌حل پیشنهادی:\n• از طریق SSMS حساب کاربری را Unlock کنید.\n• با مدیر SQL Server تماس بگیرید.";

                case 4062:
                    return $"دیتابیس «{databaseName}» در حال حاضر در دسترس نیست (آفلاین یا در حال Restore).\n\nراه‌حل پیشنهادی:\n• وضعیت دیتابیس را در SSMS بررسی کنید.\n• منتظر بمانید تا عملیات Restore/Recovery تمام شود.";

                case 17:
                    return $"سرور SQL Server «{serverName}» وجود ندارد یا دسترسی به آن امکان‌پذیر نیست.\n\nراه‌حل پیشنهادی:\n• نام سرور را بررسی کنید.\n• اگر از Instance استفاده می‌کنید، نام آن را دقیق وارد کنید.\n• TCP/IP و SQL Server Browser را بررسی کنید.";

                case 10013:
                    return $"اتصال به سرور «{serverName}» توسط فایروال یا تنظیمات امنیتی سیستم رد شد.\n\nراه‌حل پیشنهادی:\n• فایروال ویندوز، آنتی‌ویروس یا Policy شبکه را بررسی کنید.\n• مطمئن شوید پورت SQL Server باز است.";

                case 10049:
                    return $"آدرس سرور «{serverName}» معتبر نیست.\n\nراه‌حل پیشنهادی:\n• IP یا نام سرور را بررسی کنید.\n• اگر از IP استفاده می‌کنید، مطمئن شوید فرمت آن درست است.";

                case 10051:
                    return $"شبکه مقصد برای اتصال به سرور «{serverName}» در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• اتصال شبکه، Gateway و VPN را بررسی کنید.\n• مطمئن شوید مسیر شبکه بین کلاینت و سرور برقرار است.";

                case 10065:
                    return $"هیچ مسیر شبکه‌ای برای رسیدن به سرور «{serverName}» وجود ندارد.\n\nراه‌حل پیشنهادی:\n• Routing، VPN، Gateway و Port Forwarding را بررسی کنید.";

                case 927:
                    return $"دیتابیس «{databaseName}» در حال Recovery است و فعلاً قابل استفاده نیست.\n\nراه‌حل پیشنهادی:\n• چند دقیقه صبر کنید و دوباره تست کنید.\n• اگر وضعیت Recovery طولانی شد، با مدیر دیتابیس تماس بگیرید.";

                case 926:
                    return $"دیتابیس «{databaseName}» در وضعیت Suspect قرار دارد.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید.\n• احتمال خرابی فایل، قطع برق، مشکل دیسک یا پر شدن لاگ وجود دارد.";

                case 823:
                case 824:
                case 825:
                    return $"خطای جدی I/O یا خرابی احتمالی در فایل‌های دیتابیس «{databaseName}» رخ داده است.\n\nراه‌حل پیشنهادی:\n• سریعاً با مدیر دیتابیس تماس بگیرید.\n• وضعیت دیسک، Storage و Backup بررسی شود.";

                case 0:
                    return $"خطای عمومی اتصال به SQL Server رخ داد.\n\nجزئیات:\n{ex.Message}\n\nراه‌حل پیشنهادی:\n• نام سرور «{serverName}» را بررسی کنید.\n• TCP/IP و SQL Server Browser را بررسی کنید.\n• فایروال، پورت و تنظیمات Instance را بررسی کنید.";

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
