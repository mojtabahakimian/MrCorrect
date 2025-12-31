using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Report.Helpers;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Controls.PivotGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Wins.WinMenus.KHARID_FORUSH;
using Xceed.Wpf.Toolkit.Primitives;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Prg_UI.Wins.WinMenus.SANATI.HAVALE_EXIT_SAYER;
using static Wins.WinMenus.SANATI.HAVALAH_ENTER;
using static Wins.WinMenus.Taarif.AUTOMATIC;

namespace Prg_UI.Wins.WinMenus.TR
{
    public partial class TR_SAZMAN : Window
    {
        #region Header Window Begin
        //Header Window Begin
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
        public TR_SAZMAN()
        {
            InitializeComponent();

            this.DataContext = this;

            //SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        #region LOCALMODEL
        public class N_RASID_MODEL
        {
            public string? FNUMB { get; set; }
            public string? nam { get; set; }
            public int? Expr1 { get; set; }
        }
        public class SN_MODEL_TR
        {
            public double? MHAZ_NO { get; set; }
            public string? MHAZNAME { get; set; }
        }
        #endregion
        UniversControl universControl = new UniversControl();
        public ObservableCollection<SAZMAN> SAZMAN_HISTORY_DATA { get; set; } = new ObservableCollection<SAZMAN>();
        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string Formname = "TR_SAZ";

            #region SecuritCheck
            if (!string.IsNullOrWhiteSpace(Formname))
            {
                try
                {
                    var helper = new WindowInteropHelper(this);
                    helper.EnsureHandle(); // Critical: Ensures handle exists before access
                                           // 2. Run Security:
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                    // 3. Final State Check:
                    if (!this.IsLoaded) { this.Close(); return; }
                }
                catch { try { this.Close(); } catch { } }
            }
            #endregion

            FILL_ALL_COMBOBOXES();

            ReGetHeadMaster();

            GenerateAutomaticSummary(SYNCFUSION_DG);

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            //AttachRecordCountUpdater(SYNCFUSION_DG, ROWCOUNT_TEXTBLK1);
            SetupGridNavigation();
            SetFieldsToReadOnly();
        }

        private void ReGetHeadMaster()
        {
            var historyList = dbms.DoGetDataSQL<SAZMAN>($@"
                SELECT 
                    UP_DATE,UP_TIME,UP_USER_NAME,PC_NAME,IPADD,IDD
                FROM dbo.TR_SAZMAN").ToList();

            SAZMAN_HISTORY_DATA?.Clear();

            foreach (var item in historyList)
            {
                SAZMAN_HISTORY_DATA?.Add(item);
            }
            //SYNCFUSION_DG.ItemsSource = historyList;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {

            }
        }

        public ObservableCollection<TOTA_HES> TOTA_HES_SOURCE { get; set; } = new ObservableCollection<TOTA_HES>();
        public ObservableCollection<HESAB_PAIR_HES> HESAB_SOURCE { get; set; } = new ObservableCollection<HESAB_PAIR_HES>();
        public ObservableCollection<AMC1> HESAB_SOURCE2 { get; set; } = new ObservableCollection<AMC1>();
        private void FILL_ALL_COMBOBOXES()
        {
            //پیش فرض انبار
            DEFANB.ItemsSource = dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT CODE, NAMES FROM TCOD_ANBAR").ToList();

            //پیش فرض نوع مشتری
            DEFTKH.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();

            //تب حقوق حساب های حقوق دستمزد:
            var hesabList = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
            var comboBoxes = new[] { HAZEDAR, HAZTOLID, HAZFROOSH, HAZKHADAMAT, EDABIM, HAZBIM, BESHO, BEDMOS, PARDAKH, HAZMALI, PSANDHES, HDARKASRTAKHF };
            foreach (var cmb in comboBoxes)
            {
                cmb.ItemsSource = hesabList;
            }

            var RST = dbms.DoGetDataSQL<TOTA_HES>($"SELECT NUMBER, NAME FROM TOTA_HES ORDER BY NAME").ToList();
            foreach (var item in RST)
            {
                TOTA_HES_SOURCE.Add(item);
            }

            var HST = dbms.DoGetDataSQL<HESAB_PAIR_HES>($"SELECT hes, NAME FROM CUST_HESAB").ToList();
            foreach (var row in HST)
            {
                HESAB_SOURCE.Add(row);
            }

            var HST2 = dbms.DoGetDataSQL<AMC1>($"SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS Expr1, NAME FROM TDETA_HES ORDER BY NAME").ToList();
            foreach (var row in HST2)
            {
                HESAB_SOURCE2.Add(row);
            }

            // تنظیم منبع داده برای تمامی کنترل‌های مرتبط با TOTA_HES_SOURCE
            SANDOGH.ItemsSource = SANDOGH2.ItemsSource = TOTA_HES_SOURCE;
            BANKHA.ItemsSource = BANKHA2.ItemsSource = TOTA_HES_SOURCE;
            BESTANKAR.ItemsSource = BESTANKAR2.ItemsSource = TOTA_HES_SOURCE;
            BEDEHKAR.ItemsSource = BEDEHKAR2.ItemsSource = TOTA_HES_SOURCE;
            KHARID.ItemsSource = KHARID2.ItemsSource = TOTA_HES_SOURCE;
            MKHARID.ItemsSource = MKHARID2.ItemsSource = TOTA_HES_SOURCE;
            TKHARID.ItemsSource = TKHARID2.ItemsSource = TOTA_HES_SOURCE;
            HKHARID.ItemsSource = HKHARID2.ItemsSource = TOTA_HES_SOURCE;
            FROSH.ItemsSource = FROSH2.ItemsSource = TOTA_HES_SOURCE;
            MFROSH.ItemsSource = MFROSH2.ItemsSource = TOTA_HES_SOURCE;
            TFROSH.ItemsSource = TFROSH2.ItemsSource = TOTA_HES_SOURCE;
            HFROSH.ItemsSource = HFROSH2.ItemsSource = TOTA_HES_SOURCE;
            MOGODIA.ItemsSource = MOGODIA2.ItemsSource = TOTA_HES_SOURCE;
            DARAM.ItemsSource = DARAM2.ItemsSource = TOTA_HES_SOURCE;
            HDARAM.ItemsSource = HDARAM2.ItemsSource = TOTA_HES_SOURCE;
            HAVALAH.ItemsSource = HAVALAH2.ItemsSource = TOTA_HES_SOURCE;
            HAZ_TOL.ItemsSource = HAZ_TOL2.ItemsSource = TOTA_HES_SOURCE;
            PHAZ_TOL.ItemsSource = PHAZ_TOL2.ItemsSource = TOTA_HES_SOURCE;
            PJHAZ_TOL1.ItemsSource = PJHAZ_TOL12.ItemsSource = TOTA_HES_SOURCE;
            PPDAST.ItemsSource = PPDAST1.ItemsSource = TOTA_HES_SOURCE;
            PPSAR.ItemsSource = PPSAR1.ItemsSource = TOTA_HES_SOURCE;
            CONKAL.ItemsSource = CONKAL2.ItemsSource = TOTA_HES_SOURCE;
            GHEYMAT.ItemsSource = GHEYMAT1.ItemsSource = TOTA_HES_SOURCE;
            AMALKARD.ItemsSource = AMALKARD1.ItemsSource = TOTA_HES_SOURCE;
            PKHARID.ItemsSource = PKHARID2.ItemsSource = TOTA_HES_SOURCE;

            // تنظیم منبع داده برای بخش پرسنل و حساب‌های مرتبط
            PERSONEL.ItemsSource = HESAB_SOURCE;
            PERVAM.ItemsSource = HESAB_SOURCE;
            HESMBAA.ItemsSource = HESAB_SOURCE;

            // تنظیم منبع داده برای حساب‌های متفرقه و نقدی
            ADA.ItemsSource = APA.ItemsSource = ADV.ItemsSource = APV.ItemsSource = HESAB_SOURCE2;
            hesnaghd.ItemsSource = HESAB_SOURCE2;
            HPOR.ItemsSource = HESAB_SOURCE2;
        }

        private void AttachRecordCountUpdater1(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            // متد داخلی برای به‌روزرسانی متن
            void UpdateLabel()
            {
                var count = dataGrid.View?.Records?.Count ?? 0;
                targetTextBlock.Text = count.ToString("N0"); // فرمت عددی
            }

            // رویداد لود شدن گرید
            dataGrid.Loaded += (s, e) => UpdateLabel();

            // رویداد تغییر فیلتر
            dataGrid.FilterChanged += (s, e) => UpdateLabel();

            // رویداد تغییر منبع داده (مثلاً وقتی سطر جدید اضافه یا حذف می‌شود)
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                UpdateLabel();
                // اگر منبع داده ObservableCollection باشد، باید به تغییرات آن هم گوش داد
                if (dataGrid.View != null)
                {
                    dataGrid.View.CollectionChanged += (sender, args) => UpdateLabel();
                    // همچنین گوش دادن به تغییرات رکوردها (مثل فیلتر شدن توسط View)
                    dataGrid.View.Records.CollectionChanged += (sender, args) => UpdateLabel();
                }
            };

            // فراخوانی اولیه در صورت لود بودن
            if (dataGrid.View != null)
            {
                dataGrid.View.Records.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            }
        }
        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            // متد داخلی برای به‌روزرسانی متن بر اساس منبع داده
            void UpdateLabel()
            {
                int count = 0;

                // اولویت با بررسی مستقیم منبع داده است (چون دقیق‌تر و سریع‌تر از View است)
                if (dataGrid.ItemsSource is ICollection collection)
                {
                    count = collection.Count;
                }
                else if (dataGrid.View != null && dataGrid.View.Records != null)
                {
                    // اگر منبع داده مستقیم نبود، سراغ ویو می‌رویم
                    count = dataGrid.View.Records.Count;
                }

                // چون ممکن است این فراخوانی از ترد دیگری باشد، از Dispatcher استفاده می‌کنیم
                Dispatcher.Invoke(() =>
                {
                    targetTextBlock.Text = count.ToString("N0");
                });
            }

            // متد برای اتصال به رویداد تغییرات کالکشن
            void SubscribeToCollection(object source)
            {
                if (source is INotifyCollectionChanged notifyingCollection)
                {
                    notifyingCollection.CollectionChanged += (s, e) => UpdateLabel();
                }
            }

            // 1. هر وقت کل منبع داده عوض شد (مثلا new ObservableCollection شد)
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                // به کالکشن جدید گوش بده
                if (e.NewItemsSource != null)
                {
                    SubscribeToCollection(e.NewItemsSource);
                }
                UpdateLabel();
            };

            // 2. اگر همین الان دیتایی دارد، به آن وصل شو و مقدار اولیه را ست کن
            if (dataGrid.ItemsSource != null)
            {
                SubscribeToCollection(dataGrid.ItemsSource);
                UpdateLabel();
            }
        }

        private async Task<SAZMAN> GetSazmanConfigAsync(SAZMAN TrRow)
        {
            var parameters = new
            {
                UpDate = TrRow.UP_DATE,
                UpTime = TrRow.UP_TIME ?? 0
            };

            const string sql = @"
        SELECT TOP 1 * FROM dbo.TR_SAZMAN 
        WHERE UP_DATE = @UpDate AND UP_TIME = @UpTime
        ORDER BY UP_TIME DESC";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
            return await db.QueryFirstOrDefaultAsync<SAZMAN>(sql, parameters);
        }
        private async void ReGetData(SAZMAN TrRow)
        {
            if (TrRow == null) return;

            var dataFetchTask = GetSazmanConfigAsync(TrRow);
            var delayTask = Task.Delay(300);
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);

            bool loaderShown = false;
            if (completedTask == delayTask)
            {
                BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                SAZMAN config = await dataFetchTask;
                if (config == null)
                {
                    if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
                    new Msgwin(false, "اطلاعات یافت نشد.").Show();
                    return;
                }
                //مخشصات سیسیتم
                if (config != null)
                {
                    #region GENERAL_TAB
                    WIDTH_D.Text = config?.WIDTH_D;
                    HIGH_D.Text = config?.HIGH_D;
                    BACKPATH.Text = config?.BACKPATH;
                    TFADDRESS.Text = config?.TFADDRESS;
                    TFTEL.Text = config?.TFTEL;
                    SERVERNAM.Text = config?.SERVERNAM;
                    OPTIONSS.Text = config?.OPTIONSS;

                    //// Try to load existing signature
                    //if (config?.EMZA != null && config.EMZA.Length > 0)
                    //{
                    //    try
                    //    {
                    //        // First try to load the image directly
                    //        BitmapImage bitmapSource = CL_LMethods.ByteArrayToBitmapImage(config.EMZA);

                    //        if (bitmapSource != null)
                    //        {
                    //            // We successfully loaded the image
                    //            ImageBrush brush = new ImageBrush(bitmapSource);
                    //            brush.Stretch = Stretch.Uniform;
                    //            EMZA_CANVAS.Background = brush;

                    //            // Disable drawing when showing an existing image
                    //            EMZA_CANVAS.EditingMode = InkCanvasEditingMode.None;
                    //            _uploadedImageData = config.EMZA;
                    //        }
                    //        else
                    //        {
                    //            // If loading fails, try to load as InkCanvas strokes if applicable
                    //            try
                    //            {
                    //                using (MemoryStream ms = new MemoryStream(config.EMZA))
                    //                {
                    //                    EMZA_CANVAS.Strokes.Clear();
                    //                    EMZA_CANVAS.Strokes = new System.Windows.Ink.StrokeCollection(ms);
                    //                }
                    //            }
                    //            catch
                    //            {
                    //                // If all attempts fail, just show a white background
                    //                EMZA_CANVAS.Background = Brushes.White;
                    //                EMZA_CANVAS.Strokes.Clear();
                    //            }
                    //        }
                    //    }
                    //    catch (Exception)
                    //    {
                    //        new Msgwin(false, "خطا در بارگذاری تصویر از دیتابیس").ShowDialog();
                    //        EMZA_CANVAS.Background = Brushes.White;
                    //        EMZA_CANVAS.Strokes.Clear();
                    //    }
                    //}
                    #endregion

                    #region ASNAD_VA_HESAB_TAB

                    SetGrpOption(GRP_PERSON, (int)(config?.PERSON ?? -1)); //اشخاص مشخص شده در حسابها
                    SetGrpOption(GRP_TKHF, config?.TKHF ?? -1); //تخفیفات فاکتورها
                    SetGrpOption(GRP_SANAD, config?.SANAD ?? -1); //سند زدن

                    SIGN.IsChecked = config?.SIGN ?? false; //گردش كاری و فرمها با امضاء عمل شود
                    CTL_DT.IsChecked = config?.CTL_DT ?? false; //تاريخ فقط مربوط به همين سال باشد
                    SNDKH.IsChecked = config?.SNDKH ?? false; //سند ها روزانه باشد
                    SAGHF.IsChecked = config?.SAGHF ?? false; //سقف اعتبار 1
                    SAGHF2.IsChecked = config?.SAGHF2 ?? false; //سقف اعتبار 2

                    CPI.Text = config?.CPI?.ToStringNullSafe(); //تاریخ قابل برگشت
                    ARSESH.Text = config?.ARSESH.ToStringNullSafe(); //درصد مالیات بر ارزش افزوده
                    TFTPAGE.Text = config?.TFTPAGE.ToStringNullSafe(); //فاصله از پائين كاغذ در چاپ سند :
                    #endregion

                    #region FACTOR_VA_ANBAR_TAB
                    SetGrpOption(GRP_GHAYM, config?.GHAYM ?? -1);
                    SetGrpOption(GRP_KALA, config?.KALA ?? -1);
                    SetGrpOption(GRP_FRUP, Convert.ToInt32(config?.FRUP));
                    SetGrpOption(GRP_WAR, (int)(config?.WAR));
                    SetGrpOption(GRP_LST, (int)(config?.LST));
                    SetGrpOption(GRP_UPDDATE, Convert.ToInt32(config?.UPDDATE));

                    MOJU.IsChecked = config.MOJU;
                    RMOG.IsChecked = config.RMOG;
                    DIG.Text = config.DIG?.ToString();
                    TFCODE_E.Text = config.TFCODE_E;
                    CODEVIEW.IsChecked = config.CODEVIEW == 1; // smallint to bool
                    MAND.IsChecked = config.MAND;
                    SERFACB.IsChecked = config.SERFACB;
                    LOCKFAP.IsChecked = config.LOCKFAP;
                    DEFANB.SelectedValue = config.DEFANB; DEFANB.Items.Refresh();
                    DEFTKH.SelectedValue = config.DEFTKH; DEFTKH.Items.Refresh();
                    TRANSF.IsChecked = config.TRANSF;
                    #endregion

                    #region SALARY_TAB
                    HAZEDAR.SelectedValue = config.HAZEDAR;
                    HAZTOLID.SelectedValue = config.HAZTOLID;
                    HAZFROOSH.SelectedValue = config.HAZFROOSH;
                    HAZKHADAMAT.SelectedValue = config.HAZKHADAMAT;
                    EDABIM.SelectedValue = config.EDABIM;
                    HAZBIM.SelectedValue = config.HAZBIM;
                    BESHO.SelectedValue = config.BESHO;
                    BEDMOS.SelectedValue = config.BEDMOS;
                    PARDAKH.SelectedValue = config.PARDAKH;
                    HAZMALI.SelectedValue = config.HAZMALI;
                    PSANDHES.SelectedValue = config.PSANDHES;

                    SANAVP.Text = config.SANAVP?.ToString();
                    SAGHFH.Text = config.SAGHFH?.ToString();

                    HEZA.IsChecked = config.HEZA;
                    HPAD.IsChecked = config.HPAD;
                    HOLA.IsChecked = config.HOLA;
                    HKHA.IsChecked = config.HKHA;
                    HNAH.IsChecked = config.HNAH;
                    HJAZ.IsChecked = config.HJAZ;
                    HRAN.IsChecked = config.HRAN;
                    HCON.IsChecked = config.HCON;
                    HSAY.IsChecked = config.HSAY;
                    HSHI.IsChecked = config.HSHI;
                    HBON.IsChecked = config.HBON ?? false;
                    HTAHOL.IsChecked = config.HTAHOL ?? false;
                    #endregion

                    #region SANATI_TAB
                    ECONM.IsChecked = config.ECONM;
                    SANAT.IsChecked = config.SANAT;
                    // شماره شروع‌ها
                    STFR.Text = config.STFR?.ToString();
                    STKH.Text = config.STKH?.ToString();
                    STHFR.Text = config.STHFR?.ToString();
                    STHKH.Text = config.STHKH?.ToString();
                    STENT.Text = config.STENT?.ToString();
                    STKHS.Text = config.STKHS?.ToString();
                    STKHH.Text = config.STKHH?.ToString();
                    STTOL.Text = config.STTOL?.ToString();
                    STFRB.Text = config.STFRB?.ToString();
                    STBKH.Text = config.STBKH?.ToString();
                    STMO.Text = config.STMO?.ToString();
                    STKHA.Text = config.STKHA?.ToString();

                    // چک‌باکس‌های عوامل حقوق
                    SA_HOGH.IsChecked = config.SA_HOGH;
                    SA_40EZ.IsChecked = config.SA_40EZ;
                    SA_EZAF.IsChecked = config.SA_EZAF;
                    SA_PADA.IsChecked = config.SA_PADA;
                    SA_HOLA.IsChecked = config.SA_HOLA;
                    SA_KHAR.IsChecked = config.SA_KHAR;
                    SA_NAHA.IsChecked = config.SA_NAHA;
                    SA_JAZB.IsChecked = config.SA_JAZB;
                    SA_RAND.IsChecked = config.SA_RAND;
                    SA_COND.IsChecked = config.SA_COND;
                    SA_SAYE.IsChecked = config.SA_SAYE;
                    SA_23BI.IsChecked = config.SA_23BI;

                    SetGrpOption(GRP_FINALS, Convert.ToInt32(config.FINALS));
                    #endregion

                    #region SMS_TAB
                    SMS_USERNAME.Text = config?.SMS_USERNAME;
                    SMS_PASSWORD.Password = config?.SMS_PASSWORD;
                    SMS_TSMSHOST.Text = config?.SMS_TSMSHOST; //Line Number
                    SMS_LIBKEY.Password = config?.SMS_LIBKEY; //Api Key

                    SMS_OWNER.Text = config?.SMS_OWNER;

                    DSMS.IsChecked = (config?.DSMS ?? false);
                    PRMFR.IsChecked = (config?.PRMFR ?? false);
                    SMSACT.IsChecked = (config?.SMSACT ?? false);

                    switch (config?.SMSTYPE)
                    {
                        case "TSMS":
                            RB_TUBA.IsChecked = true;
                            break;

                        case "SMSIR":
                            RB_SMSIR.IsChecked = true;
                            break;

                        default: break;
                    }
                    #endregion

                    #region ISO_TAB
                    ISO_FROOSH.Text = config.ISO_FROOSH;
                    ISO_KHAREED.Text = config.ISO_KHAREED;
                    ISO_MAVAD.Text = config.ISO_MAVAD;
                    ISO_TOLID.Text = config.ISO_TOLID;
                    ISO_MAVADSAYER.Text = config.ISO_MAVADSAYER;
                    ISO_DTOLID.Text = config.ISO_DTOLID;
                    #endregion

                    #region CRM_SETTING_TAB
                    // عناوین آیتم‌ها
                    IT1.Text = config.IT1; IT2.Text = config.IT2; IT3.Text = config.IT3;
                    IT4.Text = config.IT4; IT5.Text = config.IT5; IT6.Text = config.IT6;
                    IT7.Text = config.IT7; IT8.Text = config.IT8; IT9.Text = config.IT9;
                    // عناوین ستون‌ها
                    IS1.Text = config.IS1; IS2.Text = config.IS2; IS3.Text = config.IS3;
                    IS4.Text = config.IS4; IS5.Text = config.IS5; IS6.Text = config.IS6;
                    IS7.Text = config.IS7; IS8.Text = config.IS8; IS9.Text = config.IS9;
                    IS10.Text = config.IS10; IS11.Text = config.IS11; IS12.Text = config.IS12;
                    IS14.Text = config.IS14; IS15.Text = config.IS15; IS16.Text = config.IS16;
                    #endregion

                    #region SSM
                    SSMTRTAKM.Text = config.SSMTRTAKM?.ToString();
                    SSMTRTAGM.Text = config.SSMTRTAGM?.ToString();
                    SSMSNDAUTO.IsChecked = config.SSMSNDAUTO.HasValue && config.SSMSNDAUTO != 0;
                    SSMTBMON.Text = config.SSMTBMON?.ToString();
                    SSMDARSAD.Text = config.SSMDARSAD?.ToString();
                    HDARKASRTAKHF.SelectedValue = config.HDARKASRTAKHF;
                    #endregion

                    #region MOADIAN
                    PRIVIATEKEY.Text = config.PRIVIATEKEY;
                    Dcertificate.Text = config.Dcertificate;
                    MEMORYID.Text = config.MEMORYID;
                    MEMORYIDsand.Text = config.MEMORYIDsand;
                    #endregion
                }

                //حسابهای خودگردان
                if (config != null)
                {
                    // گروه اول: تنظیم مقادیر انتخاب شده
                    SANDOGH.SelectedValue = config.SANDOGH;
                    BANKHA.SelectedValue = config.BANKHA;
                    BESTANKAR.SelectedValue = config.BESTANKAR;
                    BEDEHKAR.SelectedValue = config.BEDEHKAR;
                    KHARID.SelectedValue = config.KHARID;
                    MKHARID.SelectedValue = config.MKHARID;
                    TKHARID.SelectedValue = config.TKHARID;
                    HKHARID.SelectedValue = config.HKHARID;
                    FROSH.SelectedValue = config.FROSH;
                    MFROSH.SelectedValue = config.MFROSH;
                    TFROSH.SelectedValue = config.TFROSH;
                    HFROSH.SelectedValue = config.HFROSH;
                    MOGODIA.SelectedValue = config.MOGODIA;
                    DARAM.SelectedValue = config.DARAM;
                    HDARAM.SelectedValue = config.HDARAM;
                    HAVALAH.SelectedValue = config.HAVALAH;
                    HAZ_TOL.SelectedValue = config.HAZ_TOL;
                    PHAZ_TOL.SelectedValue = config.PHAZ_TOL;
                    PJHAZ_TOL1.SelectedValue = config.PJHAZ_TOL1;
                    PPDAST.SelectedValue = config.PPDAST;
                    PPSAR.SelectedValue = config.PPSAR;
                    CONKAL.SelectedValue = config.CONKAL;
                    GHEYMAT.SelectedValue = config.GHEYMAT;
                    AMALKARD.SelectedValue = config.AMALKARD;
                    PKHARID.SelectedValue = config.PKHARID;

                    // گروه دوم
                    PERSONEL.SelectedValue = config.PERSONEL;
                    PERVAM.SelectedValue = config.PERVAM;
                    HESMBAA.SelectedValue = config.HESMBAA;

                    // گروه سوم
                    ADA.SelectedValue = config.ADA;
                    APA.SelectedValue = config.APA;
                    ADV.SelectedValue = config.ADV;
                    APV.SelectedValue = config.APV;
                    hesnaghd.SelectedValue = config.hesnaghd;
                    HPOR.SelectedValue = config.HPOR;
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری اطلاعات: {ex.Message}").ShowDialog();
            }
            finally
            {
                if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SetFieldsToReadOnly()
        {
            // --- Tab: General (عمومی) ---
            WIDTH_D.IsReadOnly = true;
            HIGH_D.IsReadOnly = true;
            BACKPATH.IsReadOnly = true;
            TFADDRESS.IsReadOnly = true;
            TFTEL.IsReadOnly = true;
            SERVERNAM.IsReadOnly = true;
            OPTIONSS.IsReadOnly = true;
            EMZA_CANVAS.EditingMode = InkCanvasEditingMode.None;
            BTN_MORE.IsEnabled = false;

            // --- Tab: Self-governing accounts (حساب های خودگردان) ---
            SANDOGH.IsEnabled = false;
            SANDOGH2.IsEnabled = false;
            BANKHA.IsEnabled = false;
            BANKHA2.IsEnabled = false;
            BESTANKAR.IsEnabled = false;
            BESTANKAR2.IsEnabled = false;
            BEDEHKAR.IsEnabled = false;
            BEDEHKAR2.IsEnabled = false;
            KHARID.IsEnabled = false;
            KHARID2.IsEnabled = false;
            MKHARID.IsEnabled = false;
            MKHARID2.IsEnabled = false;
            TKHARID.IsEnabled = false;
            TKHARID2.IsEnabled = false;
            HKHARID.IsEnabled = false;
            HKHARID2.IsEnabled = false;
            FROSH.IsEnabled = false;
            FROSH2.IsEnabled = false;
            MFROSH.IsEnabled = false;
            MFROSH2.IsEnabled = false;
            TFROSH.IsEnabled = false;
            TFROSH2.IsEnabled = false;
            HFROSH.IsEnabled = false;
            HFROSH2.IsEnabled = false;
            PERSONEL.IsEnabled = false;
            PERVAM.IsEnabled = false;
            MOGODIA.IsEnabled = false;
            MOGODIA2.IsEnabled = false;
            DARAM.IsEnabled = false;
            DARAM2.IsEnabled = false;
            HDARAM.IsEnabled = false;
            HDARAM2.IsEnabled = false;
            ADA.IsEnabled = false;
            APA.IsEnabled = false;
            ADV.IsEnabled = false;
            APV.IsEnabled = false;
            HAVALAH.IsEnabled = false;
            HAVALAH2.IsEnabled = false;
            HAZ_TOL.IsEnabled = false;
            HAZ_TOL2.IsEnabled = false;
            PHAZ_TOL.IsEnabled = false;
            PHAZ_TOL2.IsEnabled = false;
            PJHAZ_TOL1.IsEnabled = false;
            PJHAZ_TOL12.IsEnabled = false;
            PPDAST.IsEnabled = false;
            PPDAST1.IsEnabled = false;
            PPSAR.IsEnabled = false;
            PPSAR1.IsEnabled = false;
            CONKAL.IsEnabled = false;
            CONKAL2.IsEnabled = false;
            GHEYMAT.IsEnabled = false;
            GHEYMAT1.IsEnabled = false;
            AMALKARD.IsEnabled = false;
            AMALKARD1.IsEnabled = false;
            PKHARID.IsEnabled = false;
            PKHARID2.IsEnabled = false;
            HESMBAA.IsEnabled = false;
            HPOR.IsEnabled = false;
            hesnaghd.IsEnabled = false;

            // --- Tab: Documents and Accounts (اسناد و حساب) ---
            GRP_PERSON.IsEnabled = false;
            SIGN.IsEnabled = false;
            CPI.IsReadOnly = true;
            CTL_DT.IsEnabled = false;
            SNDKH.IsEnabled = false;
            GRP_SANAD.IsEnabled = false;
            GRP_TKHF.IsEnabled = false;
            ARSESH.IsReadOnly = true;
            TFTPAGE.IsReadOnly = true;
            SAGHF.IsEnabled = false;
            SAGHF2.IsEnabled = false;
            DAFT_ASN_SUB.IsReadOnly = true;

            // --- Tab: Invoice and Warehouse (فاکتور و انبار) ---
            GRP_GHAYM.IsEnabled = false;
            GRP_KALA.IsEnabled = false;
            GRP_FRUP.IsEnabled = false;
            GRP_WAR.IsEnabled = false;
            GRP_UPDDATE.IsEnabled = false;
            GRP_LST.IsEnabled = false;
            MOJU.IsEnabled = false;
            RMOG.IsEnabled = false;
            DIG.IsReadOnly = true;
            TFCODE_E.IsReadOnly = true;
            CODEVIEW.IsEnabled = false;
            MAND.IsEnabled = false;
            SERFACB.IsEnabled = false;
            LOCKFAP.IsEnabled = false;
            DEFANB.IsEnabled = false;
            DEFTKH.IsEnabled = false;
            TRANSF.IsEnabled = false;

            // --- Tab: Salary (حقوق) ---
            HAZEDAR.IsEnabled = false;
            HAZTOLID.IsEnabled = false;
            HAZFROOSH.IsEnabled = false;
            HAZKHADAMAT.IsEnabled = false;
            EDABIM.IsEnabled = false;
            HAZBIM.IsEnabled = false;
            BESHO.IsEnabled = false;
            BEDMOS.IsEnabled = false;
            PARDAKH.IsEnabled = false;
            HAZMALI.IsEnabled = false;
            PSANDHES.IsEnabled = false;
            SANAVP.IsReadOnly = true;
            SAGHFH.IsReadOnly = true;
            BTN_TABLEMALIAT.IsEnabled = false;
            BTN_MORE_SALARY.IsEnabled = false;
            HEZA.IsEnabled = false;
            HPAD.IsEnabled = false;
            HOLA.IsEnabled = false;
            HKHA.IsEnabled = false;
            HNAH.IsEnabled = false;
            HJAZ.IsEnabled = false;
            HRAN.IsEnabled = false;
            HCON.IsEnabled = false;
            HSAY.IsEnabled = false;
            HSHI.IsEnabled = false;
            HBON.IsEnabled = false;
            HTAHOL.IsEnabled = false;

            // --- Tab: Industrial (SANATI) ---
            ECONM.IsEnabled = false;
            SANAT.IsEnabled = false;
            STFR.IsReadOnly = true;
            STKH.IsReadOnly = true;
            STHFR.IsReadOnly = true;
            STHKH.IsReadOnly = true;
            STENT.IsReadOnly = true;
            STKHS.IsReadOnly = true;
            STKHH.IsReadOnly = true;
            STTOL.IsReadOnly = true;
            STFRB.IsReadOnly = true;
            STBKH.IsReadOnly = true;
            STMO.IsReadOnly = true;
            STKHA.IsReadOnly = true;
            SA_HOGH.IsEnabled = false;
            SA_40EZ.IsEnabled = false;
            SA_EZAF.IsEnabled = false;
            SA_PADA.IsEnabled = false;
            SA_HOLA.IsEnabled = false;
            SA_KHAR.IsEnabled = false;
            SA_NAHA.IsEnabled = false;
            SA_JAZB.IsEnabled = false;
            SA_RAND.IsEnabled = false;
            SA_COND.IsEnabled = false;
            SA_SAYE.IsEnabled = false;
            SA_23BI.IsEnabled = false;
            GRP_FINALS.IsEnabled = false;

            // --- Tab: ISO ---
            ISO_FROOSH.IsReadOnly = true;
            ISO_KHAREED.IsReadOnly = true;
            ISO_MAVAD.IsReadOnly = true;
            ISO_TOLID.IsReadOnly = true;
            ISO_MAVADSAYER.IsReadOnly = true;
            ISO_DTOLID.IsReadOnly = true;

            // --- Tab: SMS ---
            RB_TUBA.IsEnabled = false;
            RB_SMSIR.IsEnabled = false;
            SMS_USERNAME.IsReadOnly = true;
            SMS_TSMSHOST.IsReadOnly = true;
            SMS_PASSWORD.IsEnabled = false; // PasswordBox uses IsEnabled
            SMS_LIBKEY.IsEnabled = false;
            PRMFR.IsEnabled = false;
            DSMS.IsEnabled = false;
            SMS_OWNER.IsReadOnly = true;
            SMSACT.IsEnabled = false;
            BTN_SMSFORMAT.IsEnabled = false;

            // --- Tab: CRM ---
            IT1.IsReadOnly = true;
            IT2.IsReadOnly = true;
            IT3.IsReadOnly = true;
            IT4.IsReadOnly = true;
            IT5.IsReadOnly = true;
            IT6.IsReadOnly = true;
            IT7.IsReadOnly = true;
            IT8.IsReadOnly = true;
            IT9.IsReadOnly = true;
            IS1.IsReadOnly = true;
            IS2.IsReadOnly = true;
            IS3.IsReadOnly = true;
            IS4.IsReadOnly = true;
            IS5.IsReadOnly = true;
            IS6.IsReadOnly = true;
            IS7.IsReadOnly = true;
            IS8.IsReadOnly = true;
            IS9.IsReadOnly = true;
            IS10.IsReadOnly = true;
            IS11.IsReadOnly = true;
            IS12.IsReadOnly = true;
            IS14.IsReadOnly = true;
            IS15.IsReadOnly = true;
            IS16.IsReadOnly = true;

            // --- Tab: SSM ---
            SSMTRTAKM.IsReadOnly = true;
            SSMTRTAGM.IsReadOnly = true;
            HDARKASRTAKHF.IsEnabled = false;
            SSMSNDAUTO.IsEnabled = false;
            SSMTBMON.IsReadOnly = true;
            SSMDARSAD.IsReadOnly = true;

            // --- Tab: Taxpayers (مودیان) ---
            PRIVIATEKEY.IsReadOnly = true;
            Dcertificate.IsReadOnly = true;
            MEMORYID.IsReadOnly = true;
            MEMORYIDsand.IsReadOnly = true;
        }

        private void SetComboSourceAndValue(dynamic mainCombo, dynamic secondaryCombo, object source, object value)
        {
            if (mainCombo == null) return;
            mainCombo.ItemsSource = source;
            mainCombo.SelectedValue = value;
            if (secondaryCombo != null)
            {
                secondaryCombo.ItemsSource = source;
            }
        }
        private void SetGrpOption(GroupBox Grb, int tagValue)
        {
            var panel = Grb.Content as Panel;
            if (panel == null) return;

            var radioButton = panel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.Tag != null && Convert.ToInt32(rb.Tag) == tagValue);

            if (radioButton != null)
            {
                radioButton.IsChecked = true;
            }
        }

        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            if (!NowIsReady) return;

            if (SYNCFUSION_DG.SelectedItem is SAZMAN SelectedHead)
            {
                ReGetData(SelectedHead);
            }
            else
            {
                new Msgwin(false, "خطا در انجام عملیات").ShowDialog();
            }
            //// Get the selected row and column index
            //var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null)
            //{
            //    var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
            //    UpdateCurrentCellValue(rowColumnIndex);
            //}
        }
        #region _SfDataGrid_
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            //// Defensive checks
            //if (ROWCOUNT_TEXTBLK == null) return;
            //if (SYNCFUSION_DG?.View == null) return;

            //// Safely retrieve the record count
            //var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            //// Set the label content
            //ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<SAZMAN> filterService = new FilterService<SAZMAN>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool IsExporty { get; private set; } = false;

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);


            if (record == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                return;
            }
            var property = record.GetType().GetProperty(mappingName);
            if (property == null)
            {
                //Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
                return;
            }

            //CurrentCellValue = property.GetValue(record)?.ToString();
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController?.CurrentCellManager?.CurrentCell;

            if (currentCell == null)
                return string.Empty;

            // حالت 1: Edit Mode
            if (currentCell.IsEditing)
            {
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null && !string.IsNullOrEmpty(editingElement.SelectedText))
                {
                    return editingElement.SelectedText;
                }
            }

            // حالت 2: جستجوی ساده - بدون GetCellElement
            try
            {
                var gridCellElement = currentCell?.ColumnElement;
                if (gridCellElement != null)
                {
                    var textBox = FindVisualChild<TextBox>(gridCellElement);
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.SelectedText))
                    {
                        return textBox.SelectedText;
                    }
                }
            }
            catch { }

            return string.Empty;
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر Contains
                filterService.AddFilter(columnName, selectedText, isExclusion: false, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Contains \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است
            if (filterValue != null)
            {
                // فیلتر Exact Match
                filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: true);

                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} = {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // فیلتر برای null values
                filterService.AddFilter(columnName, null, isExclusion: false, isExactMatch: true);
                ActiveFilters.Add($"{columnName} = NULL");
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            // اگر ستون یا مقدار معتبر نیست، خروج
            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است (partial selection)
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر "Does Not Contain" - برای متن
                filterService.AddFilter(columnName, selectedText, isExclusion: true, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Does Not Contain \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است (exact value)
            if (filterValue != null)
            {
                // فیلتر Exclusion با Exact Match - برای مقدار دقیق
                filterService.AddFilter(columnName, filterValue, isExclusion: true, isExactMatch: true);

                // نمایش بهتر در لیست فیلترها
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} != {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // اگر مقدار null است
                filterService.AddFilter(columnName, null, isExclusion: true, isExactMatch: true);
                ActiveFilters.Add($"{columnName} != NULL");
                ApplyCumulativeFilter();
            }
        }
        private string FormatValueForDisplay(object value)
        {
            if (value == null)
                return "NULL";

            // برای مقادیر عددی، فرمت هزارگان اعمال می‌شود
            if (value is double || value is decimal || value is float)
            {
                try
                {
                    return Convert.ToDecimal(value).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (value is int || value is long || value is short || value is byte)
            {
                try
                {
                    return Convert.ToInt64(value).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }

        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e) // Event handler to remove all filters and sorting
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails() // Method to get the details of the selected cell
        {
            // Check if there is a current cell selected in the data grid
            if (SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                          // Return the column name and the current cell value
                return (columnName, CurrentCellValue);
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter() // Method to apply all cumulative filters to the data grid
        {
            // Set the filter for the data grid view using the filter service
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as SAZMAN);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();

            UpdateRowCountLabel();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShow("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }
            }
            catch { return; }

            // Check if there are selected rows
            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in SYNCFUSION_DG.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in SYNCFUSION_DG.SelectedItems)
                {
                    foreach (var column in SYNCFUSION_DG.Columns)
                    {
                        if (!column.IsHidden) // Include only columns that are not hidden
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{SYNCFUSION_DG.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }
        }
        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            // Ensure rows are selected
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0)
            {
                return;
            }

            // Detect the current column
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null)
            {
                return;
            }

            string columnName = currentColumn.MappingName; // Get the column name
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            decimal sum = 0;
            bool isNumericColumn = false;

            // Iterate through the selected rows
            foreach (var selectedItem in _DG_.SelectedItems)
            {
                // Get the cell value for the detected column
                var cellValue = GetCellValue(selectedItem, columnName);

                if (cellValue != null && decimal.TryParse(cellValue.ToStringNullSafe(), out decimal numericValue))
                {
                    sum += numericValue;
                    isNumericColumn = true;
                }
            }

            if (isNumericColumn)
            {
                string formattedSum = sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                new Msgwin(false, $"جمع سطر های انتخاب شده در ستون [{currentColumn.HeaderText}] برار است با : {formattedSum}").ShowDialog();

            }
        }
        private object GetCellValue(object record, string columnName)
        {
            try
            {
                // Use reflection to get the property value from the record
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch
            {
                return null;
            }
        }
        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                SYNCFUSION_DG.TableSummaryRows.Clear();
            }
            else
            {
                // Check if a summary row already exists
                if (_DG_.TableSummaryRows.Count > 0)
                {
                    return; // Exit the method if a summary row already exists
                }
            }

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            var dataType = typeof(SAZMAN);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(SAZMAN).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
                {
                    var summaryColumn = new GridSummaryColumn
                    {
                        Name = column.MappingName + "Sum",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                        //Format = "{Sum:N0}"
                        Format = "{Sum:N0}"
                    };
                    summaryColumns.Add(summaryColumn);
                }
            }

            summaryRow.SummaryColumns = summaryColumns;

            _DG_.TableSummaryRows.Add(summaryRow);


        }
        private bool IsNumericType(Type type)
        {
            if (type == null)
                return false;

            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type that might represent a number
            if (type == typeof(object))
            {
                return true; // Assume it might be numeric
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion

        #region Navigation Logic
        // 1. این متد را در انتهای Window_Loaded صدا بزنید
        // --- Safe Navigation Logic ---

        private void SetupGridNavigation()
        {
            // 1. بررسی ایمنی: اگر کنترل‌ها هنوز ساخته نشده‌اند، خارج شو
            // این خط جلوی خطای NullReference را می‌گیرد اگر XAML آپدیت نشده باشد
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null)
            {
                // جهت دیباگ: اگر این خط اجرا شد یعنی یکی از نام‌ها در XAML اشتباه است
                return;
            }

            // 2. اتصال رویداد تغییر انتخاب (Selection)
            // ابتدا حذف می‌کنیم تا دوبار متصل نشود (-=)
            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            // 3. اتصال رویداد تغییر تعداد رکوردها (Collection Changed)
            // نکته مهم: بررسی می‌کنیم که View آماده است یا نه
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                // اگر ویو آماده بود، وصل شو
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            else
            {
                // اگر ویو هنوز نال بود، به رویداد Loaded خود گرید وصل می‌شویم تا بعدا انجام دهیم
                SYNCFUSION_DG.Loaded -= OnGridLoadedForNav;
                SYNCFUSION_DG.Loaded += OnGridLoadedForNav;
            }

            // 4. آپدیت اولیه متن‌ها (با بررسی نال)
            UpdateNavigationText();
        }

        // اگر گرید در ابتدا آماده نبود، این متد بعداً صدا زده می‌شود
        private void OnGridLoadedForNav(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
                UpdateNavigationText();
            }
        }

        // هندلرهای کمکی برای جلوگیری از خطای ترد
        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            // بررسی ایمنی مجدد
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            int total = 0;
            int current = 0;

            try
            {
                // محاسبه تعداد کل (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
                {
                    total = SYNCFUSION_DG.View.Records.Count;
                }

                // محاسبه ایندکس جاری (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.SelectedIndex >= 0)
                {
                    current = SYNCFUSION_DG.SelectedIndex + 1;
                }
            }
            catch
            {
                // نادیده گرفتن خطا در شرایط خاص
            }

            // نمایش
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        // 3. رویدادهای کلیک دکمه‌ها
        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSfDataFilters(); // حذف فیلترها
                ReGetHeadMaster();
                Btn_First_Click(default, default);
            }
            catch { }
        }
        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    SYNCFUSION_DG.SelectedIndex = 0;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(1, 0));
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToHome();
                }
            }
            catch { }
        }

        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex > 0)
                {
                    SYNCFUSION_DG.SelectedIndex--;
                    // اسکرول به ایندکس جدید (ایندکس رکورد + هدرها)
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(SYNCFUSION_DG.SelectedIndex + 1, 0));
                    //var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    //SYNCFUSION_DG.GetVisualContainer().ScrollRows.ScrollInView(rowIndex, 0);

                    SYNCFUSION_DG.SelectedIndex--;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید (با احتساب هدرها و فیلترها)
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون قابل مشاهده (برای ساخت RowColumnIndex صحیح)
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                {
                    SYNCFUSION_DG.SelectedIndex++;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    var lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                    SYNCFUSION_DG.SelectedIndex = lastIndex;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(lastIndex + 1, 0));

                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSfDataFilters();
        }

        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                ApplyCumulativeFilter();
                SYNCFUSION_DG.ClearFilters();
            }
            catch (Exception)
            {
            }
        }
    }
}