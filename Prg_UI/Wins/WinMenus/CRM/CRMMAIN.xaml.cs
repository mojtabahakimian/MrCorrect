using System;
using System.Collections.Generic;
using System.Linq;
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
using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Collections.ObjectModel;
using static Prg_UI.Functions.CL_LMethods;
using Prg_Proccessy.SQLMODELS;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Interop;
using Prg_Proccessy.FUNCTIONS;
using Functions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Prg_Proccessy.MODELS;
using System.Windows.Threading;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace Prg_UI.Wins.WinMenus.CRM
{
    /// <summary>
    /// Interaction logic for CRMMAIN.xaml
    /// </summary>
    public partial class CRMMAIN : Window
    {
        #region Header Window Begin
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
        #endregion

        public CRMMAIN()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ObservableCollection<COPMANES> CUSTOMER_DATA { get; set; } = new ObservableCollection<COPMANES>();
        public ObservableCollection<CRMEVENTS> EVENT_DATA { get; set; } = new ObservableCollection<CRMEVENTS>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public string SQL_HOLD_DATA { get; set; }
        //متغیر برای نگهداری فیلتر بر اساس وضعیت
        public List<Status_List> StatusItems { get; set; }

        // متغیرها برای نگهداری وضعیت فیلترها
        private string _companyNameFilter = string.Empty;
        private string _phoneFilter = string.Empty;
        private string _dateFilter = string.Empty;
        private int? _selectedStatus = null; // null به معنی "همه" است

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {
                _bl = value;
                IntPtr handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
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
                CRM_MASTER_SUB.IsReadOnly = !ican;
            }
        }

        public class Status_List
        {
            public int CODE { get; set; }
            public string NAME { get; set; }
        }
        public class Fac_List
        {
            public string? STATUS_FACT { get; set; }
        }
        public class Saler_List
        {
            public string? SALER { get; set; }
        }
        public class Buyer_List
        {
            public string? BUYER { get; set; }
        }

        public bool NowIsReady { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {

                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);

            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.LETSGO(this.GetType().Name, "NOTE");
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();
            ReGetData();
        }

        private void FILL_ALL_COMBOBOXES()
        {
            // نام این متغیر را از items به statusList تغییر می‌دهیم تا با Property جدید تداخل نداشته باشد
            List<Status_List> statusList = new List<Status_List>()
            {
                new Status_List() { NAME = Baseknow.IT1.ToString(), CODE = 1 },
                new Status_List() { NAME = Baseknow.IT2.ToString(), CODE = 2 },
                new Status_List() { NAME = Baseknow.IT3.ToString(), CODE = 3 },
                new Status_List() { NAME = Baseknow.IT4.ToString(), CODE = 4 },
                new Status_List() { NAME = Baseknow.IT5.ToString(), CODE = 5 },
                new Status_List() { NAME = Baseknow.IT6.ToString(), CODE = 6 },
                new Status_List() { NAME = Baseknow.IT7.ToString(), CODE = 7 },
                new Status_List() { NAME = Baseknow.IT8.ToString(), CODE = 8 },
                new Status_List() { NAME = Baseknow.IT9.ToString(), CODE = 9 },
            };

            // مقداردهی Property جدید
            StatusItems = statusList;

            // بایند کردن به ItemsControl (چون DataContext خود پنجره است، این کار می‌کند)
            StatusFilterItemsControl.ItemsSource = StatusItems;

            STATUS_COLUMN.ItemsSource = statusList.ToList();
            STATUS2_COLUMN.ItemsSource = statusList.ToList();

            STATUS_FAC_COLUMN.ItemsSource = dbms.DoGetDataSQL<Fac_List>("SELECT COPMANES.STATUS_FACT FROM COPMANES GROUP BY COPMANES.STATUS_FACT");
            SALER_COLUMN.ItemsSource = dbms.DoGetDataSQL<Saler_List>("SELECT SALER FROM CRMEVENTS GROUP BY SALER ORDER BY SALER").ToList();
            BUYER_COLUMN.ItemsSource = dbms.DoGetDataSQL<Buyer_List>("SELECT BUYER FROM CRMEVENTS GROUP BY BUYER ORDER BY BUYER").ToList();
        }

        private void ReGetData()
        {
            CUSTOMER_DATA?.Clear();
            //برای لیست فعالیت ها و بررسی نام شرکت
            SQL_HOLD_DATA = $@"SELECT COPMANES.*, eventscount.idcn FROM COPMANES LEFT OUTER JOIN eventscount ON COPMANES.id = eventscount.idc where  COPMANES.userid={Baseknow.USERCOD.ToString()} ORDER BY COPMANES.id ";

            var customers = dbms.DoGetDataSQL<COPMANES>($@"SELECT COPMANES.*, eventscount.idcn FROM COPMANES LEFT OUTER JOIN eventscount ON COPMANES.id = eventscount.idc where  COPMANES.userid={Baseknow.USERCOD.ToString()} ORDER BY COPMANES.id ").ToList();
            foreach (var item in customers)
            {
                CUSTOMER_DATA.Add(item);
            }
        }
        private void LoadDetail()
        {
            EVENT_DATA?.Clear();

            var current = CRM_MASTER_SUB.SelectedItem as COPMANES;
            if (current != null && current.ID != null)
            {
                var dets = dbms.DoGetDataSQL<CRMEVENTS>($"SELECT * FROM CRMEVENTS WHERE idc = {current.ID} ORDER BY idde").ToList();
                foreach (var item in dets)
                {
                    EVENT_DATA.Add(item);
                }
                DETAIL_CRM_SUB.IsEnabled = true;
            }
            else
            {
                DETAIL_CRM_SUB.IsEnabled = false;
            }
        }

        private void CRM_MASTER_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) return;
            LoadDetail();
        }

        private void CRM_MASTER_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (sender is not DataGrid grid) return;
            var New_Record = e.Row.IsNewItem;


            var binding = (e.Column as DataGridBoundColumn)?.Binding as Binding;
            var mappingName = binding?.Path?.Path;

            var record = e.Row.Item as COPMANES;
            if (record == null || mappingName == null) return;

            switch (mappingName)
            {
                case nameof(COPMANES.COMPANY_NAME):
                    if (New_Record)
                    {
                        var cOMP_name = e.EditingElement as TextBox;
                        string? text = cOMP_name.Text;
                        if (string.IsNullOrWhiteSpace(text)) return;
                        string shart = BuildShart(text, "NAME");
                        string shart2 = BuildShart(text, "TNAME");
                        if (shart.Length > 0 && shart2.Length > 0)
                            shart = $"(({shart}) or ({shart2}))";
                        var cnt = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM cust_hesab_dtl WHERE {shart}").FirstOrDefault();
                        if (cnt > 0)
                        {
                            new Msgwin(false, "مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد").Show();
                            new COMPANS(SQL_HOLD_DATA).ShowDialog();
                            return;
                        }
                        var shartCust = shart.Replace("TNAME", "NAME").Replace("NAME", "COMPANY_NAME");
                        var cnt2 = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM COPMANES WHERE {shartCust}").FirstOrDefault();
                        if (cnt2 > 0)
                        {
                            new Msgwin(false, "مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد").Show();
                            new COMPANS(SQL_HOLD_DATA).ShowDialog();
                            return;
                        }
                    }
                    break;
                case nameof(COPMANES.FACT_TEL):
                    CheckFactTel(record.FACT_TEL);
                    break;
                case nameof(COPMANES.MOBILE):
                    CheckMobile(record.MOBILE);
                    break;
            }

        }

        private void CRM_MASTER_SUB_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is COPMANES row)
            {
                var today = Tarikh.FullCurrentDate;
                row.DT = int.TryParse(today, out var dtVal) ? (int?)dtVal : null;
                row.DATE_SABT = Tarikh.GetRawGregorianDateTime(today);

                row.STATUS = 1;
                row.USER_NAME = Baseknow.UUSER.ToString();
            }
        }

        private void CRM_MASTER_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.Escape)) return;
            if (e.EditAction == DataGridEditAction.Cancel) return;

            var row = e.Row.Item as COPMANES;
            if (row == null) return;

            if (row.DATE_SABT == null || row.DT == null)
            {
                var today = Tarikh.FullCurrentDate;
                row.DT = row.DT ?? (int.TryParse(today, out var dtVal) ? (int?)dtVal : null);
                row.DATE_SABT = row.DATE_SABT ?? Tarikh.GetRawGregorianDateTime(today);
            }
            var baseParams = new
            {
                row.COMPANY_NAME,
                row.CITY,
                row.MANAGER,
                row.FACT_TEL,
                row.MOBILE,
                row.PERNUM,
                row.STATUS_FACT,
                row.PRODUCTS,
                row.ADDR,
                row.ACCOUNTANT,
                row.SOFTWARE,
                row.ESP_PERSON,
                row.REAGENT,
                row.STATUS,
                row.COMMENT,
                DATE_SABT = row.DATE_SABT,
                row.USER_NAME,
                row.PIC,
                row.DT,
                row.USERID,
                row.LONGITUDE,
                row.LATITUDE,
                row.OSTANID,
                row.SHAHRID
            };
            if (row.ID == null || row.ID <= 0)
            {
                var sql = @"INSERT INTO COPMANES (COMPANY_NAME, CITY, MANAGER, FACT_TEL, MOBILE, PERNUM, STATUS_FACT, PRODUCTS, ADDR, ACCOUNTANT, SOFTWARE, ESP_PERSON, REAGENT, STATUS, COMMENT, date_sabt, USER_NAME, pic, dt, userid, Longitude, Latitude, OSTANID, SHAHRID)
                            VALUES (@COMPANY_NAME, @CITY, @MANAGER, @FACT_TEL, @MOBILE, @PERNUM, @STATUS_FACT, @PRODUCTS, @ADDR, @ACCOUNTANT, @SOFTWARE, @ESP_PERSON, @REAGENT, @STATUS, @COMMENT, @DATE_SABT, @USER_NAME, @PIC, @DT, @USERID, @LONGITUDE, @LATITUDE, @OSTANID, @SHAHRID);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var newId = dbms.DoGetDataSQL<int>(sql, baseParams).FirstOrDefault();
                if (newId > 0) row.ID = newId;
            }
            else
            {
                var sql = @"UPDATE COPMANES SET COMPANY_NAME=@COMPANY_NAME, CITY=@CITY, MANAGER=@MANAGER, FACT_TEL=@FACT_TEL, MOBILE=@MOBILE, PERNUM=@PERNUM, STATUS_FACT=@STATUS_FACT, PRODUCTS=@PRODUCTS, ADDR=@ADDR, ACCOUNTANT=@ACCOUNTANT, SOFTWARE=@SOFTWARE, ESP_PERSON=@ESP_PERSON, REAGENT=@REAGENT, STATUS=@STATUS, COMMENT=@COMMENT, date_sabt=@DATE_SABT, USER_NAME=@USER_NAME, pic=@PIC, dt=@DT, userid=@USERID, Longitude=@LONGITUDE, Latitude=@LATITUDE, OSTANID=@OSTANID, SHAHRID=@SHAHRID WHERE ID=@ID";
                var updateParams = new { row.ID, baseParams.COMPANY_NAME, baseParams.CITY, baseParams.MANAGER, baseParams.FACT_TEL, baseParams.MOBILE, baseParams.PERNUM, baseParams.STATUS_FACT, baseParams.PRODUCTS, baseParams.ADDR, baseParams.ACCOUNTANT, baseParams.SOFTWARE, baseParams.ESP_PERSON, baseParams.REAGENT, baseParams.STATUS, baseParams.COMMENT, baseParams.DATE_SABT, baseParams.USER_NAME, baseParams.PIC, baseParams.DT, baseParams.USERID, baseParams.LONGITUDE, baseParams.LATITUDE, baseParams.OSTANID, baseParams.SHAHRID };
                dbms.DoExecuteSQL(sql, updateParams);
            }
        }

        private void DETAIL_CRM_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            if (e.EditAction == DataGridEditAction.Cancel) return;
            // commit edit so the row item reflects latest user changes
            if (e.Row.Item == null) return;
            var row = e.Row.Item as CRMEVENTS;
            var master = CRM_MASTER_SUB.SelectedItem as COPMANES;
            if (row == null || master == null || master.ID == null) return;
            row.IDC = master.ID;
            var baseParams = new
            {
                row.COMPANY_NAME,
                row.INFO_DATE,
                row.INFO_TIME,
                row.SALER,
                row.BUYER,
                row.COMMENT,
                row.NEXT_DATE,
                row.NEXT_TIME,
                row.STATUS,
                row.PIC,
                row.IDC,
                row.PAYAM,
                row.MITING,
                row.USERID,
                row.CDATETI
            };
            if (row.IDDE == null || row.IDDE <= 0)
            {
                var sql = @"INSERT INTO CRMEVENTS (COMPANY_NAME, INFO_DATE, INFO_TIME, SALER, BUYER, COMMENT, NEXT_DATE, NEXT_TIME, STATUS, pic, idc, PAYAM, miting, USERID, CDATETI)
                            VALUES (@COMPANY_NAME, @INFO_DATE, @INFO_TIME, @SALER, @BUYER, @COMMENT, @NEXT_DATE, @NEXT_TIME, @STATUS, @PIC, @IDC, @PAYAM, @MITING, @USERID, @CDATETI);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var newId = dbms.DoGetDataSQL<int>(sql, baseParams).FirstOrDefault();
                if (newId > 0) row.IDDE = newId;
            }
            else
            {
                var sql = @"UPDATE CRMEVENTS SET COMPANY_NAME=@COMPANY_NAME, INFO_DATE=@INFO_DATE, INFO_TIME=@INFO_TIME, SALER=@SALER, BUYER=@BUYER, COMMENT=@COMMENT, NEXT_DATE=@NEXT_DATE, NEXT_TIME=@NEXT_TIME, STATUS=@STATUS, pic=@PIC, idc=@IDC, PAYAM=@PAYAM, miting=@MITING, USERID=@USERID, CDATETI=@CDATETI WHERE idde=@IDDE";
                var updateParams = new { row.IDDE, baseParams.COMPANY_NAME, baseParams.INFO_DATE, baseParams.INFO_TIME, baseParams.SALER, baseParams.BUYER, baseParams.COMMENT, baseParams.NEXT_DATE, baseParams.NEXT_TIME, baseParams.STATUS, baseParams.PIC, baseParams.IDC, baseParams.PAYAM, baseParams.MITING, baseParams.USERID, baseParams.CDATETI };
                dbms.DoExecuteSQL(sql, updateParams);
            }
            LoadDetail();
        }

        private void CheckCompanyName(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            string shart = BuildShart(text, "NAME");
            string shart2 = BuildShart(text, "TNAME");
            if (shart.Length > 0 && shart2.Length > 0)
                shart = $"(({shart}) or ({shart2}))";
            //var cnt = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM cust_hesab_dtl WHERE {shart}").FirstOrDefault();
            //if (cnt > 0)
            //{
            //    new Msgwin(false, "مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد").Show();
            //}
            var shartCust = shart.Replace("TNAME", "NAME").Replace("NAME", "COMPANY_NAME");
            var cnt2 = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM COPMANES WHERE {shartCust}").FirstOrDefault();
            if (cnt2 > 0)
            {
                // new Msgwin(false, "مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد").Show();
            }
        }

        private string BuildShart(string text, string field)
        {
            var words = text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> parts = new List<string>();
            foreach (var w in words)
            {
                var w1 = w.Replace('ی', 'ي').Replace("˜", "ß");
                var w2 = w.Replace('ي', 'ی').Replace("˜", "ß");
                parts.Add($"({field} LIKE N'%{w}%' OR {field} LIKE N'%{w1}%' OR {field} LIKE N'%{w2}%')");
            }
            return string.Join(" and ", parts);
        }

        private void CheckFactTel(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return;
            var cnt = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM cust_hesab WHERE TEL Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
        }

        private void CheckMobile(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return;
            var cnt = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM cust_hesab WHERE TEL Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
            var cnt2 = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM COPMANES WHERE MOBILE Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt2 > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
        }

        private void DETAIL_CRM_SUB_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is CRMEVENTS row)
            {
                row.INFO_DATE = Convert.ToInt32(Tarikh.FullCurrentDate);

                row.SALER = Baseknow.UUSER.ToString();

                var time24 = DateTime.Now.ToString("HHmm", System.Globalization.CultureInfo.InvariantCulture);
                row.INFO_TIME = Convert.ToInt32(time24);

                row.CDATETI = DateTime.Now;
            }
        }

        private void CRM_MASTER_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete || Keyboard.Modifiers != ModifierKeys.None) return;
            e.Handled = true;
            //if (!AllowDeletions) return;

            var rows = CRM_MASTER_SUB.SelectedItems.Cast<COPMANES>().ToList();
            if (rows.Count == 0) return;

            bool blocked = false;
            foreach (var row in rows)
            {
                if (row == null) continue;
                if (row.ID != null)
                {
                    var dep = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM CRMEVENTS WHERE idc=@id", new { id = row.ID }).FirstOrDefault();
                    if (dep > 0)
                    {
                        blocked = true;
                        continue;
                    }
                    dbms.DoExecuteSQL("DELETE FROM COPMANES WHERE ID=@id", new { id = row.ID });
                }
                CUSTOMER_DATA.Remove(row);
            }
            EVENT_DATA.Clear();
            if (blocked)
            {
                new Msgwin(false, "شما نمي توانيداطلاعاتي كه در جاي ديگر استفاده شده است راحذف كنيد. براي حذف آن ابتدا بايد اطلاعات  وابسته را حذف كنيد").ShowDialog();
            }
        }

        private void DETAIL_CRM_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete || Keyboard.Modifiers != ModifierKeys.None) return;
            e.Handled = true;
            //if (!AllowDeletions) return;

            var rows = DETAIL_CRM_SUB.SelectedItems.Cast<CRMEVENTS>().ToList();
            if (rows.Count == 0) return;

            var master = CRM_MASTER_SUB.SelectedItem as COPMANES;
            int removed = 0;
            foreach (var row in rows)
            {
                if (row == null) continue;
                if (row.IDDE != null)
                {
                    dbms.DoExecuteSQL("DELETE FROM CRMEVENTS WHERE idde=@id", new { id = row.IDDE });
                    removed++;
                }
                EVENT_DATA.Remove(row);
            }

            if (master != null && master.IDCN != null)
            {
                master.IDCN -= removed;
                if (master.IDCN < 0) master.IDCN = 0;
                CRM_MASTER_SUB.Items.Refresh();
            }
        }

        private void ApplyCombinedFilters()
        {
            if (!NowIsReady) return;

            var view = CollectionViewSource.GetDefaultView(CUSTOMER_DATA);
            if (view == null) return;

            view.Filter = item =>
            {
                if (item is not COPMANES customer) return false;

                // شرط اول: فیلتر وضعیت
                bool statusMatch = _selectedStatus == null || customer.STATUS == _selectedStatus;

                // شرط دوم: فیلتر نام شرکت
                bool companyNameMatch = string.IsNullOrWhiteSpace(_companyNameFilter) ||
                                        (customer.COMPANY_NAME?.Contains(_companyNameFilter, StringComparison.OrdinalIgnoreCase) ?? false);

                // شرط سوم: فیلتر تلفن کارخانه
                bool phoneMatch = string.IsNullOrWhiteSpace(_phoneFilter) ||
                                  (customer.FACT_TEL?.Contains(_phoneFilter) ?? false);

                // جدید: شرط چهارم: فیلتر تاریخ
                // ابتدا کاراکترهای اضافه ماسک را حذف می‌کنیم
                string dateFilterNumeric = _dateFilter.Replace("/", "").Replace("_", "").Trim();
                bool dateMatch = string.IsNullOrWhiteSpace(dateFilterNumeric) ||
                                 (customer.DT.HasValue && customer.DT.Value.ToString().StartsWith(dateFilterNumeric));

                // نتیجه نهایی: رکورد باید تمام شرایط را داشته باشد
                return statusMatch && companyNameMatch && phoneMatch && dateMatch;
            };
        }
        // رویداد برای دکمه رادیویی "همه"
        private void AllStatus_Checked(object sender, RoutedEventArgs e)
        {
            _selectedStatus = null; // null به معنی "همه"
            ApplyCombinedFilters();
        }

        // رویداد برای سایر وضعیت‌ها
        private void Status_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is int statusCode)
            {
                _selectedStatus = statusCode;
                ApplyCombinedFilters();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // از as استفاده می‌کنیم تا اگر sender یکی از این نوع‌ها نبود، خطا ندهد
            var control = sender as Control;
            if (control == null) return;

            if (control.Name == "CompanyNameFilterTextBox")
            {
                _companyNameFilter = (control as TextBox).Text;
            }
            else if (control.Name == "PhoneFilterTextBox")
            {
                _phoneFilter = (control as TextBox).Text;
            }
            else if (control.Name == "DateFilterMaskedTextBox")
            {
                // برای MaskedTextBox باید به نوع خودش کست شود
                _dateFilter = (control as Xceed.Wpf.Toolkit.MaskedTextBox).Text;
            }

            ApplyCombinedFilters();
        }
    }
}