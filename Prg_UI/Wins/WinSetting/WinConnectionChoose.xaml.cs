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
                    _cnn = $@"Data Source={serverName};Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=15;"; //WIN
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
                cmd.CommandTimeout = 15;
                var result = cmd.ExecuteScalar();
                return (true, null);
            }
            catch (SqlException ex)
            {
                return (false, GetSqlConnectionErrorMessage(ex));
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

        private static string GetSqlConnectionErrorMessage(SqlException ex)
        {
            // بررسی خطای Win32 (سطح شبکه/سیستم‌عامل)
            if (ex.InnerException is System.ComponentModel.Win32Exception win32)
            {
                return win32.NativeErrorCode switch
                {
                    10061 => "سرویس SQL Server در حال اجرا نیست یا اتصال رد شد.\n\nراه‌حل پیشنهادی:\n• از طریق Services.msc سرویس SQL Server را راه‌اندازی کنید.\n• مطمئن شوید پورت 1433 در فایروال باز است.",
                    53    => "مسیر شبکه به سرور یافت نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور را بررسی کنید.\n• مطمئن شوید سرور روشن و در شبکه قرار دارد.",
                    64    => "اتصال شبکه به سرور قطع شد.\n\nراه‌حل پیشنهادی:\n• کابل شبکه یا Wi-Fi را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.",
                    10060 => "اتصال به سرور با خطای وقفه زمانی مواجه شد.\n\nراه‌حل پیشنهادی:\n• آدرس IP یا نام سرور را بررسی کنید.\n• فایروال را بررسی کنید.",
                    _     => $"خطای شبکه (کد: {win32.NativeErrorCode}):\n{win32.Message}\n\nراه‌حل: با مدیر شبکه تماس بگیرید.",
                };
            }

            return ex.Number switch
            {
                2 or 53   => "سرور SQL Server یافت نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور را دوباره بررسی کنید.\n• مطمئن شوید سرور روشن است و در شبکه قرار دارد.\n• پورت 1433 باید در فایروال باز باشد.",
                26        => "نام سرور یا اینستنس پیدا نشد.\n\nراه‌حل پیشنهادی:\n• نام سرور و اینستنس را بررسی کنید (مثال: SERVER\\SQLEXPRESS).\n• مطمئن شوید سرویس SQL Server Browser روشن است.",
                -2        => "اتصال به سرور با وقفه زمانی قطع شد.\n\nراه‌حل پیشنهادی:\n• مطمئن شوید سرور در دسترس است.\n• شبکه را بررسی کنید.\n• فایروال ممکن است اتصال را مسدود کرده باشد.",
                4060      => "دیتابیس مورد نظر یافت نشد یا دسترسی ندارید.\n\nراه‌حل پیشنهادی:\n• نام دیتابیس را بررسی کنید.\n• مطمئن شوید کاربر به این دیتابیس دسترسی دارد.",
                4064      => "دیتابیس پیش‌فرض کاربر در دسترس نیست.\n\nراه‌حل پیشنهادی:\n• با مدیر دیتابیس تماس بگیرید تا دیتابیس پیش‌فرض کاربر را تنظیم کند.",
                18456     => "ورود به SQL Server ناموفق بود.\n\nراه‌حل پیشنهادی:\n• نام کاربری SQL را بررسی کنید.\n• رمز عبور را مجدداً وارد کنید.\n• مطمئن شوید کاربر در SQL Server تعریف شده است.",
                18452     => "احراز هویت ناموفق - نوع ورود نادرست است.\n\nراه‌حل پیشنهادی:\n• بین Windows Authentication و SQL Authentication انتخاب درستی داشته باشید.",
                18470     => "حساب کاربری غیرفعال شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر SQL Server تماس بگیرید تا حساب کاربری را فعال کند.",
                64        => "اتصال به سرور قطع شد.\n\nراه‌حل پیشنهادی:\n• شبکه را بررسی کنید.\n• مطمئن شوید سرویس SQL Server در حال اجرا است.",
                121 or
                232 or
                233       => "خطای ارتباط با Named Pipe سرور.\n\nراه‌حل پیشنهادی:\n• Named Pipes و TCP/IP را در SQL Server Configuration Manager فعال کنید.\n• سرویس SQL Server را ریستارت کنید.",
                17142     => "سرویس SQL Server موقتاً متوقف شده است.\n\nراه‌حل پیشنهادی:\n• با مدیر سیستم تماس بگیرید تا سرویس SQL Server را راه‌اندازی کند.",
                1205      => "تداخل در تراکنش‌های دیتابیس (Deadlock).\n\nراه‌حل پیشنهادی:\n• چند لحظه صبر کنید و دوباره تست کنید.\n• اگر مشکل ادامه داشت با مدیر دیتابیس تماس بگیرید.",
                _         => $"خطای SQL Server (کد: {ex.Number}):\n{ex.Message}\n\nراه‌حل: با مدیر سیستم تماس بگیرید.",
            };
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
