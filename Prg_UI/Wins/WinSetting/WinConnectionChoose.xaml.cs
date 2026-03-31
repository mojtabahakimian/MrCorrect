using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Syncfusion.Windows.Shared;
using System;
using System.ComponentModel;
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
        internal bool GoTestConnectionOK()
        {
            string _result = null;
            string _cnn = null;
            Dispatcher.Invoke(() =>
            {
                ServerChooser.UpdateLayout();
                var ServerChooser_TEXBOX = (TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser);
                ServerChooser.SelectedValue = ServerChooser_TEXBOX.Text.Trim();
                var ServerChooser_TEX = ServerChooser_TEXBOX.Text.Trim();

                DbChooser.UpdateLayout();
                var DbChooser_TEXBOX = (TextBox)DbChooser.Template.FindName("PART_EditableTextBox", DbChooser);
                DbChooser.SelectedValue = DbChooser_TEXBOX.Text.Trim();
                var DbChooser_TEX = DbChooser_TEXBOX.Text.Trim();

                if (rd_WinAuth.IsChecked is true) //Windows Authentication
                    _cnn = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"; //WIN
                else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                    _cnn = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Password};Integrated Security=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"; // SQL
            });

            CL_CCNNMANAGER tsdb = new CL_CCNNMANAGER();
            CL_CCNNMANAGER.CONNECTION_STR = _cnn;
            try
            {
                _result = tsdb.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN --TEST").FirstOrDefault();
            }
            catch (Exception ex)
            {
                return false;
            }

            if (_result is not null)
                return true;
            else
                return false;
        }

        private sealed class ConnectionTestResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

        private ConnectionTestResult TestConnectionWithDetails()
        {
            string serverName = string.Empty;
            string databaseName = string.Empty;
            string username = string.Empty;
            bool isWindowsAuth = false;
            string connectionString = null;

            Dispatcher.Invoke(() =>
            {
                ServerChooser.UpdateLayout();
                var serverChooserTextBox = ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser) as TextBox;
                serverName = serverChooserTextBox?.Text?.Trim() ?? ServerChooser.Text?.Trim() ?? string.Empty;

                DbChooser.UpdateLayout();
                var dbChooserTextBox = DbChooser.Template.FindName("PART_EditableTextBox", DbChooser) as TextBox;
                databaseName = dbChooserTextBox?.Text?.Trim() ?? DbChooser.Text?.Trim() ?? string.Empty;

                username = Textbox_DataUsername.Text?.Trim() ?? string.Empty;
                isWindowsAuth = rd_WinAuth.IsChecked is true;
            });

            if (string.IsNullOrWhiteSpace(serverName))
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = "نام سرور خالی است.\nراهنما: نام یا IP سرور SQL را وارد کنید (مثال: SERVER\\SQL2019 یا 192.168.1.10)."
                };
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = "نام دیتابیس خالی است.\nراهنما: ابتدا سرور را انتخاب کنید و سپس یک دیتابیس معتبر را انتخاب/وارد کنید."
                };
            }

            if (!isWindowsAuth)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new ConnectionTestResult
                    {
                        IsSuccess = false,
                        Message = "نام کاربری SQL خالی است.\nراهنما: در حالت SQL Authentication باید نام کاربری معتبر وارد شود."
                    };
                }

                if (string.IsNullOrWhiteSpace(Textbox_Datapass.Password))
                {
                    return new ConnectionTestResult
                    {
                        IsSuccess = false,
                        Message = "رمز عبور SQL خالی است.\nراهنما: در حالت SQL Authentication باید رمز عبور معتبر وارد شود."
                    };
                }
            }

            if (isWindowsAuth)
                connectionString = $@"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            else
                connectionString = $@"Data Source={serverName};Initial Catalog={databaseName};User ID={username};Password={Textbox_Datapass.Password};Integrated Security=False;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT TOP (1) SERVERNAM FROM dbo.SAZMAN", connection))
                    {
                        command.CommandTimeout = 15;
                        command.ExecuteScalar();
                    }
                }

                return new ConnectionTestResult
                {
                    IsSuccess = true,
                    Message = "تست اتصال موفق بود و ارتباط با دیتابیس برقرار شد."
                };
            }
            catch (SqlException ex)
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = BuildSqlErrorMessage(ex, serverName, databaseName, isWindowsAuth)
                };
            }
            catch (InvalidOperationException ex)
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = $"تنظیمات اتصال معتبر نیست یا وضعیت اتصال صحیح نیست.\nجزئیات: {ex.Message}\nراهنما: اطلاعات سرور/دیتابیس/احراز هویت را بررسی کنید و دوباره تست بگیرید."
                };
            }
            catch (TimeoutException)
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = "اتصال به دلیل تاخیر زیاد سرور (Timeout) برقرار نشد.\nراهنما: شبکه/VPN را بررسی کنید، از روشن بودن SQL Server مطمئن شوید و دوباره تلاش کنید."
                };
            }
            catch (Exception ex)
            {
                return new ConnectionTestResult
                {
                    IsSuccess = false,
                    Message = BuildFallbackErrorMessage(ex)
                };
            }
        }

        private string BuildSqlErrorMessage(SqlException ex, string serverName, string databaseName, bool isWindowsAuth)
        {
            if (ex == null)
                return "خطا در اتصال به دیتابیس رخ داد.\nراهنما: اطلاعات اتصال را بررسی کرده و مجدد تلاش کنید.";

            if (ex.InnerException is Win32Exception win32)
            {
                return win32.NativeErrorCode switch
                {
                    10061 => "سرویس SQL Server در حال اجرا نیست یا اتصال رد شد.\n\nراه‌حل پیشنهادی:\n• از طریق Services.msc سرویس SQL Server را راه‌اندازی کنید.\n• مطمئن شوید پورت 1433 در فایروال باز است.",
                    53 => $"مسیر شبکه به سرور «{serverName}» یافت نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور را بررسی کنید.\n• مطمئن شوید سرور روشن و در شبکه قرار دارد.",
                    64 => $"اتصال شبکه به سرور «{serverName}» قطع شد.\n\nراه‌حل پیشنهادی:\n• کابل شبکه یا Wi-Fi را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.",
                    10060 => $"اتصال به سرور «{serverName}» با خطای وقفه زمانی مواجه شد.\n\nراه‌حل پیشنهادی:\n• آدرس IP یا نام سرور را بررسی کنید.\n• فایروال را بررسی کنید.",
                    _ => $"خطای شبکه (کد: {win32.NativeErrorCode}):\n{win32.Message}\n\nراه‌حل: با مدیر شبکه تماس بگیرید."
                };
            }

            switch (ex.Number)
            {
                case 2:
                case 53:
                case 40:
                case 11001:
                    return $"سرور SQL Server «{serverName}» یافت نشد یا در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• نام سرور را دوباره بررسی کنید.\n• مطمئن شوید سرور روشن است و در شبکه قرار دارد.\n• پورت 1433 باید در فایروال باز باشد.";

                case -2:
                case 121:
                case 258:
                    return $"اتصال به سرور «{serverName}» با وقفه زمانی (Timeout) قطع شد.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید سرور در دسترس است.\n• شبکه و VPN را بررسی کنید.\n• فایروال ممکن است اتصال را مسدود کرده باشد.";

                case 26:
                    return "نام سرور یا Instance پیدا نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور و Instance را بررسی کنید (مثال: SERVER\\SQLEXPRESS).\n• مطمئن شوید سرویس SQL Server Browser روشن است.";

                case 4060:
                    return $"دیتابیس «{databaseName}» یافت نشد یا دسترسی ندارید.\n\nراه‌حل پیشنهادی:\n• نام دیتابیس را بررسی کنید.\n• مطمئن شوید کاربر به این دیتابیس دسترسی دارد.";

                case 4064:
                    return "دیتابیس پیش‌فرض کاربر در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید تا دیتابیس پیش‌فرض کاربر را تنظیم کند.";

                case 18456:
                    if (isWindowsAuth)
                        return "ورود ناموفق بود (Windows Authentication).\n\nراه‌حل پیشنهادی:\n• حساب ویندوز فعلی باید روی SQL Server دسترسی داشته باشد.\n• در صورت نیاز از SQL Authentication استفاده کنید.";

                    return "ورود به SQL Server ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری SQL را بررسی کنید.\n• رمز عبور را مجدداً وارد کنید.\n• مطمئن شوید کاربر در SQL Server تعریف شده است.";

                case 18452:
                    return "احراز هویت ناموفق - نوع ورود نادرست است.\n\nراه‌حل پیشنهادی:\n• بین Windows Authentication و SQL Authentication انتخاب درستی داشته باشید.";

                case 18470:
                    return "حساب کاربری SQL غیرفعال شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر SQL Server تماس بگیرید تا حساب کاربری را فعال کند.";

                case 64:
                    return $"اتصال به سرور «{serverName}» قطع شد.\n\nراه‌حل پیشنهادی:\n• شبکه را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.";

                case 232:
                case 233:
                    return "خطای ارتباط با Named Pipe/Transport سرور رخ داد.\n\nراه‌حل پیشنهادی:\n• Named Pipes و TCP/IP را در SQL Server Configuration Manager فعال کنید.\n• سرویس SQL Server را ریستارت کنید.";

                case 17142:
                    return "سرویس SQL Server موقتاً متوقف شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید تا سرویس SQL Server را راه‌اندازی کند.";

                case 1205:
                    return "تداخل در تراکنش‌های دیتابیس (Deadlock) رخ داده است.\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کنید و دوباره تست کنید.\n• اگر مشکل ادامه داشت با مدیر دیتابیس تماس بگیرید.";

                case 229:
                case 916:
                    return "کاربر دسترسی لازم به دیتابیس یا اشیای آن را ندارد.\n\nراه‌حل پیشنهادی:\n• از مدیر دیتابیس بخواهید دسترسی‌های لازم (حداقل خواندن جدول SAZMAN) را اعطا کند.";

                case 208:
                    return "جدول «dbo.SAZMAN» در دیتابیس انتخابی یافت نشد.\n\nراه‌حل پیشنهادی:\n• دیتابیس صحیح برنامه را انتخاب کنید.\n• اسکریپت‌های ایجاد/به‌روزرسانی جداول را اجرا کنید.";

                case 207:
                    return "ساختار دیتابیس با نسخه برنامه سازگار نیست (ستون لازم وجود ندارد).\n\nراه‌حل پیشنهادی:\n• اسکریپت‌های به‌روزرسانی دیتابیس را اجرا کنید یا نسخه برنامه را همسان کنید.";

                case 1326:
                    return "اعتبارسنجی کاربر در شبکه/دامنه ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری و رمز عبور ویندوز/دامنه را بررسی کنید.\n• از دسترسی شبکه و Domain Controller مطمئن شوید.";

                default:
                    return $"خطای SQL Server (کد: {ex.Number}):\n{ex.Message}\n\nراه‌حل: تنظیمات سرور/شبکه/دسترسی را بررسی کنید یا با مدیر سیستم تماس بگیرید.";
            }
        }

        private string BuildFallbackErrorMessage(Exception ex)
        {
            if (ex is ArgumentException || ex is FormatException)
                return $"فرمت اطلاعات اتصال صحیح نیست.\nجزئیات: {ex.Message}\nراهنما: نام سرور/دیتابیس و اطلاعات احراز هویت را با فرمت درست وارد کنید.";

            if (ex is COMException)
                return $"خطای سیستمی هنگام ارتباط با SQL رخ داد.\nجزئیات: {ex.Message}\nراهنما: سرویس‌های SQL Server و SQL Browser را بررسی کرده و سیستم را مجدد تست کنید.";

            if (ex is AccessViolationException || ex is OutOfMemoryException)
                return "خطای سطح سیستم رخ داد و اتصال کامل نشد.\nراهنما: برنامه را ببندید و دوباره اجرا کنید؛ اگر ادامه داشت با پشتیبانی سیستم تماس بگیرید.";

            return $"امکان اتصال به دیتابیس وجود ندارد.\nجزئیات: {ex.Message}\nراهنما: تنظیمات سرور، شبکه، فایروال، احراز هویت و وجود دیتابیس صحیح را بررسی کنید.";
        }

        private async void Btn_TestConnection_Click(object sender, RoutedEventArgs e)
        {
            lblconnecting.Visibility = Visibility.Visible;
            this.IsEnabled = false;

            ConnectionTestResult testResult = null;
            await Task.Run(() =>
            {
                testResult = TestConnectionWithDetails();
            });

            if (testResult?.IsSuccess is true)
                new Msgwin(false, testResult.Message).ShowDialog();
            else
                new Msgwin(false, testResult?.Message ?? "امکان اتصال به دیتابیس وجود ندارد.\nراهنما: اطلاعات اتصال را مجدد بررسی کنید و دوباره تلاش کنید.").ShowDialog();

            lblconnecting.Visibility = Visibility.Hidden;
            this.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CL_CCNNMANAGER.ConnectedToSQLDB == true)
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
