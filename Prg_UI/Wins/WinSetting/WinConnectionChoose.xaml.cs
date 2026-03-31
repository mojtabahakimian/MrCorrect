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
                CNN_STR = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};User ID={Textbox_DataUsername.Text?.Trim() ?? string.Empty};Password={Textbox_Datapass.Password ?? string.Empty};Integrated Security=False;TrustServerCertificate=True;MultipleActiveResultSets=True;"; // SQL

            try
            {
                CL_CCNNMANAGER.CONNECTION_STR = CNN_STR; // قرار دادن رشته اتصال نهایی به کلاس اصلی

                var TestCnn = dbms.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN")?.FirstOrDefault(); //تست نهایی برای اطمینان

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

            var currentExecutablePath = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath;
            if (!string.IsNullOrEmpty(currentExecutablePath))
            {
                Process.Start(currentExecutablePath);
            }

            CL_LMethods.CleanupBeforeExiting();

            Application.Current?.Shutdown();
            try { CL_LMethods.GoExitTheApplication(); } catch { } //#NABILOO#
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
        private async void Btn_TestConnection_Click(object sender, RoutedEventArgs e)
        {
            lblconnecting.Visibility = Visibility.Visible;
            this.IsEnabled = false;

            var _isconnect = false;
            await Task.Run(() =>
            {
                _isconnect = GoTestConnectionOK();
            });

            if (_isconnect is true)
                new Msgwin(false, "تست موفقیت آمیز بودارتباط بر قرار شد").ShowDialog();
            else
                new Msgwin(false, "تست ناموفق بود !").ShowDialog();

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
