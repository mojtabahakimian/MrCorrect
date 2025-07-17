using AUTO_BAZ.Functions;
using AUTO_BAZ.HelperWins;
using Dapper;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AUTO_BAZ
{
    public class SqlLocator
    {
        [DllImport("odbc32.dll")]
        private static extern short SQLAllocHandle(short hType, IntPtr inputHandle, out IntPtr outputHandle);
        [DllImport("odbc32.dll")]
        private static extern short SQLSetEnvAttr(IntPtr henv, int attribute, IntPtr valuePtr, int strLength);
        [DllImport("odbc32.dll")]
        private static extern short SQLFreeHandle(short hType, IntPtr handle);
        [DllImport("odbc32.dll", CharSet = CharSet.Ansi)]
        private static extern short SQLBrowseConnect(IntPtr hconn, StringBuilder inString,
            short inStringLength, StringBuilder outString, short outStringLength,
            out short outLengthNeeded);

        private const short SQL_HANDLE_ENV = 1;
        private const short SQL_HANDLE_DBC = 2;
        private const int SQL_ATTR_ODBC_VERSION = 200;
        private const int SQL_OV_ODBC3 = 3;
        private const short SQL_SUCCESS = 0;

        private const short SQL_NEED_DATA = 99;
        private const short DEFAULT_RESULT_SIZE = 1024;
        private const string SQL_DRIVER_STR = "DRIVER=SQL SERVER";

        private SqlLocator() { }

        public static string[] GetServers()
        {
            string[] retval = null;
            string txt = string.Empty;
            IntPtr henv = IntPtr.Zero;
            IntPtr hconn = IntPtr.Zero;
            StringBuilder inString = new StringBuilder(SQL_DRIVER_STR);
            StringBuilder outString = new StringBuilder(DEFAULT_RESULT_SIZE);
            short inStringLength = (short)inString.Length;
            short lenNeeded = 0;

            try
            {
                if (SQL_SUCCESS == SQLAllocHandle(SQL_HANDLE_ENV, henv, out henv))
                {
                    if (SQL_SUCCESS == SQLSetEnvAttr(henv, SQL_ATTR_ODBC_VERSION, (IntPtr)SQL_OV_ODBC3, 0))
                    {
                        if (SQL_SUCCESS == SQLAllocHandle(SQL_HANDLE_DBC, henv, out hconn))
                        {
                            if (SQL_NEED_DATA == SQLBrowseConnect(hconn, inString, inStringLength, outString,
                                DEFAULT_RESULT_SIZE, out lenNeeded))
                            {
                                if (DEFAULT_RESULT_SIZE < lenNeeded)
                                {
                                    outString.Capacity = lenNeeded;
                                    if (SQL_NEED_DATA != SQLBrowseConnect(hconn, inString, inStringLength, outString,
                                        lenNeeded, out lenNeeded))
                                    {
                                        throw new ApplicationException("Unabled to aquire SQL Servers from ODBC driver.");
                                    }
                                }
                                txt = outString.ToString();
                                int start = txt.IndexOf("{") + 1;
                                int len = txt.IndexOf("}") - start;
                                if ((start > 0) && (len > 0))
                                {
                                    txt = txt.Substring(start, len);
                                }
                                else
                                {
                                    txt = string.Empty;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Throw away any error if we are not in debug mode
                //MessageBox.Show(ex.Message, "Acquire SQL Servier List Error");
                txt = string.Empty;
            }
            finally
            {
                if (hconn != IntPtr.Zero)
                {
                    SQLFreeHandle(SQL_HANDLE_DBC, hconn);
                }
                if (henv != IntPtr.Zero)
                {
                    SQLFreeHandle(SQL_HANDLE_ENV, hconn);
                }
            }

            if (txt.Length > 0)
            {
                retval = txt.Split(",".ToCharArray());
            }

            return retval;
        }
    }

    public partial class WinConnectionChoose : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WinConnectionChoose()
        {
            InitializeComponent();
        }
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
        private string CNN_STR { get; set; } = null;
        //public static List<string> GetAllSqlServerNames()
        //{
        //    // Create a new list to hold the server names
        //    List<string> servers = new List<string>();

        //    // Use the SqlDataSourceEnumerator instance to get information about data sources
        //    SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;

        //    DataTable serversTable = instance.GetDataSources();

        //    // Iterate through the returned table and populate the server names list
        //    foreach (DataRow row in serversTable.Rows)
        //    {
        //        string serverName = row["ServerName"].ToString();
        //        string instanceName = row["InstanceName"].ToString();

        //        // If the instance name is empty, just use the server name
        //        if (string.IsNullOrEmpty(instanceName))
        //        {
        //            servers.Add(serverName);
        //        }
        //        else // If the instance name is not empty, use both the server name and instance name
        //        {
        //            servers.Add($"{serverName}\\{instanceName}");
        //        }
        //    }

        //    return servers;
        //}

        public static List<string> GetAllSqlServerNames()
        {
            // Create a new list to hold the server names
            List<string> servers = new List<string>();
            servers = SqlServerScanner.GetAllSqlServerNames();
            return servers;
        }

        public static List<string> GetAllSqlServerNames_Fast()
        {
            // Use a HashSet to store the server names
            HashSet<string> servers = new HashSet<string>();

            // Use a parallel loop to iterate through the rows
            Parallel.ForEach(SqlDataSourceEnumerator.Instance.GetDataSources().AsEnumerable(), row =>
            {
                string serverName = row.Field<string>("ServerName");
                string instanceName = row.Field<string>("InstanceName");

                // Use a StringBuilder to construct the server name
                StringBuilder sb = new StringBuilder(serverName);
                if (!string.IsNullOrEmpty(instanceName))
                {
                    sb.Append("\\").Append(instanceName);
                }

                // Use the Add method of the HashSet to avoid duplicates
                servers.Add(sb.ToString());
            });

            // Convert the HashSet to a List and return it
            return servers.ToList();
        }
        private void Btn_SaveConnection_Click(object sender, RoutedEventArgs e)
        {
            string ServerChooser_TEX = ((TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser)).Text;

            var DbChooser_TEXBOX = (TextBox)DbChooser.Template.FindName("PART_EditableTextBox", DbChooser);
            DbChooser.SelectedValue = DbChooser_TEXBOX.Text.Trim();
            var DbChooser_TEX = DbChooser_TEXBOX.Text.Trim();

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

            if (rd_WinAuth.IsChecked is true) //Windows Authentication
                CNN_STR = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};Integrated Security=True;TrustServerCertificate=True;"; //WIN
            else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                CNN_STR = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Text};Integrated Security=False;"; // SQL

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
            new Msgwin(false, "با موفقیت به دیتابیس متصل شد , برنامه باید یکبار ری استارت شود.").ShowDialog();
            this.Close();

            var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(currentExecutablePath);
            Application.Current.Shutdown();
            Environment.Exit(0);
            return;

            //Restart Application
            //try
            //{
            //    // Get the path to the current executable
            //    string appPath = Assembly.GetEntryAssembly().Location;
            //    // Start a new process using the current executable
            //    Process.Start(new ProcessStartInfo
            //    {
            //        FileName = "cmd.exe",
            //        Arguments = $"/c start \"\" \"{appPath}\"",
            //        UseShellExecute = false,
            //        CreateNoWindow = true
            //    });
            //    // Terminate the current process
            //    Process.GetCurrentProcess().Kill();
            //}
            //catch (Exception) { Environment.Exit(0); }
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
                    _cnn = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};Integrated Security=True;TrustServerCertificate=True;"; //WIN
                else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                    _cnn = $@"Data Source={ServerChooser_TEX};Initial Catalog={DbChooser_TEX};User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Text};Integrated Security=False;"; // SQL
            });

            CL_CCNNMANAGER tsdb = new CL_CCNNMANAGER();
            CL_CCNNMANAGER.CONNECTION_STR = _cnn;
            try
            {
                _result = tsdb.DoGetDataSQL<string>("SELECT SERVERNAM FROM dbo.SAZMAN").FirstOrDefault();
            }
            catch (Exception)
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
        private void Btn_GetServers_Click(object sender, RoutedEventArgs e)
        {
            ServerChooser.SelectionChanged -= ServerChooser_SelectionChanged; //برای ایکه رویداد الکی اجرا نشه تداخل درست کنه

            ServerChooser.ItemsSource = null;
            ServerChooser.Items?.Clear();
            ServerChooser.ItemsSource = GetAllSqlServerNames();
            ServerChooser.SelectedIndex = 0;

            ServerChooser.SelectionChanged += ServerChooser_SelectionChanged; //برای ایکه رویداد الکی اجرا نشه تداخل درست کنه

            DbChooser.ItemsSource = null;
            DbChooser.Items?.Clear();
            DbChooser.SelectedIndex = -1;
        }
        private void GetDBSFromServer()
        {
            try
            {

                if (string.IsNullOrEmpty(ServerChooser.SelectedValue.ToStringNullSafe()))
                {
                    var ServerChooser_TEX = (TextBox)ServerChooser.Template.FindName("PART_EditableTextBox", ServerChooser);
                    ServerChooser.SelectedValue = ServerChooser_TEX.Text;
                }

                var _TMPCNN = "";
                if (rd_WinAuth.IsChecked is true) //Windows Authentication
                    _TMPCNN = $@"Data Source={ServerChooser.SelectedValue};Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;"; //WIN
                else if (rd_SqlAuth.IsChecked is true) //SQL Authentication
                    _TMPCNN = $@"Data Source={ServerChooser.SelectedValue};Initial Catalog=master;User ID={Textbox_DataUsername.Text.Trim()};Password={Textbox_Datapass.Text};Integrated Security=False;"; // SQL

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

        private void ServerChooser_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
