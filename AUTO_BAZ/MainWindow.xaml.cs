using AUTO_BAZ.Functions;
using AUTO_BAZ.HelperWins;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static AUTO_BAZ.Functions.CL_LMethods;
using static AUTO_BAZ.LocalModles;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace AUTO_BAZ
{
    #region CustomLocalModel
    public class LocalModles
    {
        public class rst4_modelTIM
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? TAMIR { get; set; }
        }
        public class cm_model
        {
            public long? DATE_N { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
            public double? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public string? CODE { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public string? MANDAH { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public int? FROM_A { get; set; }
            public string? N_RASID { get; set; }
            public double? MEGH_R { get; set; }
            public double? RADAH { get; set; }
            public double? SANAD_NO { get; set; }
            public double? CUST_NO { get; set; }
            public double? ANBARF { get; set; }
            public int? VAHED_K { get; set; }
            public double? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
            public double? N_TAF { get; set; }
            public double? AVRAGE { get; set; }
            public long? ID { get; set; }
            public string? BARGAH { get; set; }

            /// <summary>ترتیب عددیِ نوع برگه داخل یک روز (TAGCOD.tartib). مبنای ORDER BY است؛
            /// BARGAH فقط برای سازگاری با شاخه‌ی قدیمی نگه داشته شده.</summary>
            public double? tartib { get; set; }
        }
        public class rst4_model
        {
            public double? CODE { get; set; }
            public string? NAMES { get; set; }
        }
        public class ANBGRD_LST
        {
            public int? GRD_NUM { get; set; }
            public string? CODE { get; set; }
            public double? MOG { get; set; }
            public double? NUM1 { get; set; }
            public double? NUM2 { get; set; }
            public double? NUM3 { get; set; }
            public double? MABL { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class THE_QUERY1
        {
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public double? MOGODI_A { get; set; }
            public double? FI_A { get; set; }
            public double? MABL_A { get; set; }
        }
        public class THE_QUERY2
        {
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public double? MABLKs { get; set; }
        }
        public class THE_QUERY3
        {
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public double? MABLKs { get; set; }
        }
    }
    #endregion
    public partial class MainWindow : Window
    {
        protected override void OnClosing(CancelEventArgs e)
        {
            DelayedDurabilityGuard.TryDisableForcefully();
            base.OnClosing(e);
        }

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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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

        #region LOCALMODEL
        public class ErrorSectionModel
        {
            public bool ErrorHappend { get; set; }
            public string SectionName { get; set; }
        }
        #endregion

        public List<ErrorSectionModel> ERTRACKLIST { get; set; } = new List<ErrorSectionModel>();

        /// <summary>
        /// برای اعلام خطا به برنامه مسترکارکت در بخش صدور اسناد
        /// </summary>
        public bool AnyErrorHappend { get; set; }

        public bool IsWorkisDone { get; set; } = false;



        bool hasExecutedToday = false;
        public bool runone { get; set; }

        System.Timers.Timer MyTimer = new System.Timers.Timer(300000); // Set interval to 5 minutes 300000

        private static CancellationTokenSource CancelerTOKEN = new CancellationTokenSource();
        // put this in the Task :      CancellationToken token = CancelerTOKEN.Token;         token.ThrowIfCancellationRequested();
        List<Task> tasks = new List<Task>();

        private List<(CheckBox checkBox, ProgressBar progressBar)> _taskControls = new List<(CheckBox, ProgressBar)>();
        public void UpdateOverallProgressBar()
        {
            if (_taskControls == null || _taskControls.Count == 0) return;

            var activeTasks = _taskControls.Where(x => x.checkBox.IsChecked == true).ToList();
            int activeCount = activeTasks.Count;

            double overallProgress = 0;

            if (activeCount > 0)
            {
                double totalValue = activeTasks.Sum(x => x.progressBar.Value);
                overallProgress = totalValue / activeCount;
            }

            // Update the text label
            TOGHER_PROGRESS.Value = overallProgress;
            COUNTER_TXBL.Content = $"{overallProgress:F1}%";

            UpdateDataInSharedViewModel();
        }

        public ObservableCollection<string> LST_DATA5 { get; set; } = new ObservableCollection<string>();
        public string DT { get; set; } = "800101";
        public static bool StillMethodIsWorking { get; set; } = false;
        public bool NowIsReady { get; private set; }

        public CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        #region DISPATCHERTIMER
        //public bool IsTheTimerStillWorking { get; private set; }

        //System.Windows.Threading.DispatcherTimer AutobazTimer = new System.Windows.Threading.DispatcherTimer();
        //private void TimerEstelam_TICK(object? sender, EventArgs e)
        //{
        //    if (IsTheTimerStillWorking) return; // جلوگیری از اینکه تایمر هنوز در حال کار است.

        //    //Begin{
        //    IsTheTimerStillWorking = true;

        //    IsTheTimerStillWorking = false;
        //    //}End
        //}
        #endregion

        private bool _iswanttocancel = false;
        public bool IsCancelRequestedBgWorker
        {
            get
            {
                //ThrowIfCancellationRequested 
                if (CancelerTOKEN.IsCancellationRequested)
                    _iswanttocancel = true;

                return _iswanttocancel;
            }
            set { _iswanttocancel = value; }
        }

        /// <summary>
        /// آیا از یک فرم جداگانه برای بازسازی یک بخش خاص صدا زده شده ؟
        /// </summary>
        public Boolean SingleCallerPart { get; set; }
        /// <summary>
        /// تیک هایی که باید بخورد
        /// </summary>
        public bool[] CHKITEMS { get; set; }
        /// <summary>
        /// مدل برای انتقال وضعیت میزنا پیشرفت
        /// </summary>
        private AutoBazBridge AutoBazBridgeViewModel = new AutoBazBridge();
        public MainWindow(/*bool[] booleanArray = null, bool singleCallerPart = false*/)
        {
            InitializeComponent();
            //SingleCallerPart = singleCallerPart;
            //CHKITEMS = booleanArray;

            LST_DATA5.CollectionChanged += LST_DATA5_CollectionChanged;

            DataContext = AutoBazBridgeViewModel;
        }
        //ایمجوری هم میشه استفاده کرد: برای صدا زدنش از جای دیگه
        //public MainWindow(bool[] booleanArray, bool singleCallerPart) : this()
        //{
        //    SingleCallerPart = singleCallerPart;
        //    CHKITEMS = booleanArray;
        //}
        public void UpdateDataInSharedViewModel()
        {
            AutoBazBridgeViewModel.LabelContent = (string)COUNTER_TXBL.Content;
            AutoBazBridgeViewModel.ProgressValue = Convert.ToDouble(TOGHER_PROGRESS.Value);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region PrepaireAndCheck
            if (SingleCallerPart == false)
            {
                Baseknow.GetInitTheApp();
                dbms = new CL_CCNNMANAGER();
                if (CL_CCNNMANAGER.ConnectedToSQLDB is false)
                {
                    WinConnectionChoose winConnectionChoose = new WinConnectionChoose();
                    this.Close();
                    winConnectionChoose.ShowDialog();
                }

                CL_LOCKWATCH Lockwatch = new CL_LOCKWATCH();

                //CL_LMethods.DoWriteMyLog($"Lockwatch.GoCheck()  : {Lockwatch.GoCheck()}");
                if (Lockwatch.GoCheck() == false)
                {
                    CL_LMethods.GoExitTheApplication();
                }
            }

            SERVER_NAME_LBL.Text = CL_Generaly.General_Servername + " | " + CL_Generaly.General_DBname;
            YEAR_LBL.Content = Baseknow.NAME + " " + Baseknow.YEA;
            //LBL_HEADER.Content = CL_CCNNMANAGER.CONNECTION_STR;
            #endregion

            _taskControls = new List<(CheckBox, ProgressBar)>
            {
                (C0, PRGR_C0),
                (C00, PRGR_C00),
                (c1, PRGR_C1),
                (c2, PRGR_C2),
                (c3, PRGR_C3),
                (c4, PRGR_C4),
                (c5, PRGR_C5),
                (c6, PRGR_C6),
                (c7, PRGR_C7),
                (c8, PRGR_C8),
                (c9, PRGR_C9),
                (c10, PRGR_C10),
                (c11, PRGR_C11)
            };

            // Load From Saved Data List
            if (!string.IsNullOrEmpty(Properties.Settings.Default.TheHistoryLST))
            {
                string[] messageArray = Properties.Settings.Default.TheHistoryLST.Split(';');
                LST_DATA5?.Clear();
                foreach (var item in messageArray)
                    LST_DATA5.Add(item);

                if (List5.Items.Count > 0)
                {
                    AutoScrollToCurrentItem(List5, List5.Items.Count);
                    List5.SelectedItem = List5.Items[List5.Items.Count - 1];
                }
            }

            LoadSettingStateOfCheckBoxes();

            MyTimer.Elapsed += new ElapsedEventHandler(MyTimer_Tick);
            MyTimer.Start();

            #region TMP
            //LogWriter.WriteLog("test");
            //try
            //{

            //_ = int.Parse("sdsd");
            //}
            //catch (Exception er)
            //{
            //    ExpectionLogWriter.WriteLog(er,"");
            //}
            #endregion

        } //Loaded
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void LST_DATA5_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (NowIsReady)
            {
                if (LST_DATA5.Count > 0)
                {
                    Properties.Settings.Default.TheHistoryLST = null;

                    string Str_Items = string.Join(";", LST_DATA5.ToList());

                    Properties.Settings.Default.TheHistoryLST = Str_Items;
                    Properties.Settings.Default.Save();

                    int lastIndex = List5.Items.Count - 1;
                    if (lastIndex >= 0)
                    {
                        AutoScrollToCurrentItem(List5, List5.Items.Count);
                        List5.SelectedItem = List5.Items[List5.Items.Count - 1];
                    }


                }
            }
        }

        private bool is_timer_running = false;
        private void MyTimer_Tick(object? sender, ElapsedEventArgs e)
        {
            if (is_timer_running) return;
            is_timer_running = true;



            if (DateTime.Now.Hour == 0 && runone) // Check if it is midnight and runone is true
            {
                runone = false;

                bool _CanGo = false; Dispatcher.Invoke(new Action(() => { _CanGo = IsAtLeasOnChecked(); }));
                if (_CanGo)
                {
                    // تنظیم تاریخ آخرین اجرا به تاریخ فعلی
                    LetsGoBtn_Click(null, null);
                }

                var rst = dbms.DoGetDataSQL<rst4_modelTIM>("SELECT     NUMBER , TAG , TAMIR FROM dbo.HEAD_LST WHERE  (TAG = 20) And (TAMIR = 1)").ToList();
                for (int i = 0; i < rst.Count; i++) //while (!rst.EOF)
                {
                    var RST2 = dbms.DoGetDataSQL<DateTime?>("SELECT UP_DATE FROM HEAD_LST_LOG WHERE TAGG = 20 AND NUMBER = " + rst[i].NUMBER + " ORDER BY IDD DESC ").ToList();
                    if (RST2.Count > 0)
                    {
                        TimeSpan timeDifference = DateTime.Now - Convert.ToDateTime(RST2.FirstOrDefault()); // if (DateDiff("h", RST2.Fields("UP_DATE"), DateTime.Now) > 96L)
                        if (timeDifference.TotalHours > 96)
                        {
                            dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST SET TAMIR = 0 WHERE NUMBER = {rst[i].NUMBER} AND TAG = {rst[i].TAG} "); //rst[i].TAMIR = 0;
                            //rst.update();
                            //RST2.AddNew();
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.head_lst_log(UP_DATE,NUMBER,TAGG,RESERVED,UP_USER_NAME)
                                             VALUES (GETDATE(),{rst[i].NUMBER},20,0,N'System')");
                        }
                    }
                }
            }
            else //Check if it is noon
            {
                if (DateTime.Now.Hour == 12) runone = true;
            }




            is_timer_running = false;
        }

        private void SaveCheckBoxesState()
        {
            Properties.Settings.Default.IsC0 = C0.IsChecked ?? false;
            Properties.Settings.Default.IsC00 = C00.IsChecked ?? false;
            Properties.Settings.Default.IsC1 = c1.IsChecked ?? false;
            Properties.Settings.Default.IsC2 = c2.IsChecked ?? false;
            Properties.Settings.Default.IsC3 = c3.IsChecked ?? false;
            Properties.Settings.Default.IsC4 = c4.IsChecked ?? false;
            Properties.Settings.Default.IsC5 = c5.IsChecked ?? false;
            Properties.Settings.Default.IsC6 = c6.IsChecked ?? false;
            Properties.Settings.Default.IsC7 = c7.IsChecked ?? false;
            Properties.Settings.Default.IsC8 = c8.IsChecked ?? false;
            Properties.Settings.Default.IsC9 = c9.IsChecked ?? false;
            Properties.Settings.Default.IsC10 = c10.IsChecked ?? false;
            Properties.Settings.Default.IsC11 = c11.IsChecked ?? false;
            Properties.Settings.Default.UseSmartThrottling = chkUseSmartThrottling.IsChecked ?? false;

            //Properties.Settings.Default.IsDefacc = defacc.IsChecked ?? false;
            Properties.Settings.Default.UseParallelProcessing = UseParallelProcessing.IsChecked ?? true;

            Properties.Settings.Default.Save();
        }
        private void LoadSettingStateOfCheckBoxes()
        {
            if (SingleCallerPart == false)
            {
                C0.IsChecked = Properties.Settings.Default.IsC0;
                C00.IsChecked = Properties.Settings.Default.IsC00;
                c1.IsChecked = Properties.Settings.Default.IsC1;
                c2.IsChecked = Properties.Settings.Default.IsC2;
                c3.IsChecked = Properties.Settings.Default.IsC3;
                c4.IsChecked = Properties.Settings.Default.IsC4;
                c5.IsChecked = Properties.Settings.Default.IsC5;
                c6.IsChecked = Properties.Settings.Default.IsC6;
                c7.IsChecked = Properties.Settings.Default.IsC7;
                c8.IsChecked = Properties.Settings.Default.IsC8;
                c9.IsChecked = Properties.Settings.Default.IsC9;
                c10.IsChecked = Properties.Settings.Default.IsC10;
                c11.IsChecked = Properties.Settings.Default.IsC11;
                UseParallelProcessing.IsChecked = Properties.Settings.Default.UseParallelProcessing;
                chkUseSmartThrottling.IsChecked = Properties.Settings.Default.UseSmartThrottling;
            }
            else
            {
                C0.IsChecked = CHKITEMS[0];
                c1.IsChecked = CHKITEMS[1];
                c2.IsChecked = CHKITEMS[2];
                c3.IsChecked = CHKITEMS[3];
                c4.IsChecked = CHKITEMS[4];
                c5.IsChecked = CHKITEMS[5];
                c6.IsChecked = CHKITEMS[6];
                c7.IsChecked = CHKITEMS[7];
                c8.IsChecked = CHKITEMS[8];
                c9.IsChecked = CHKITEMS[9];
                c10.IsChecked = CHKITEMS[10];
                c11.IsChecked = CHKITEMS[11];
                UseParallelProcessing.IsChecked = Properties.Settings.Default.UseParallelProcessing;
                chkUseSmartThrottling.IsChecked = Properties.Settings.Default.UseSmartThrottling;
            }

            CL_HESABDARI_AUTO_BAZ.UseSmartThrottlingByDefault = chkUseSmartThrottling.IsChecked ?? false;

            //defacc.IsChecked = Properties.Settings.Default.IsDefacc;
        }
        private void UseParallelProcessing_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private bool IsNull(object inputy)
        {
            if (string.IsNullOrEmpty(inputy.ToStringNullSafe())) // بله خالیه
                return true;
            else
                return false; // خیر خالی نیست
        }
        private void daysb_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(daysb.Text.Trim()) || string.IsNullOrWhiteSpace(daysb.Text.Trim()))
            {
                daysb.Text = "365";
            }
            else if (!Information.IsNumeric(daysb.Text.Trim()))
            {
                daysb.Text = "365";
            }

            Properties.Settings.Default.Daysb_Conf = daysb.Text;
            Properties.Settings.Default.Save();
        }

        private void BtnCNNConf_Click(object sender, RoutedEventArgs e)
        {

            new WinConnectionChoose().ShowDialog();
        }
        private void SayOprationsFinished()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                StillMethodIsWorking = false;

                // ───────────────────────────────────────────────────────────────────────────
                // پایان‌دهیِ تک‌معنا.
                //
                // قبلاً این متد سه کار متناقض پشت‌سرهم می‌کرد: اول ۱۰۰٪ می‌نوشت، بعد
                // DoResetCountersDisplay() همه‌ی نوارها و درصد کل را صفر می‌کرد، و در انتها
                // دوباره متن را "0.00%" می‌گذاشت. یعنی کاربر عملاً هیچ‌وقت ۱۰۰٪ را نمی‌دید.
                //
                // صفر کردن اینجا هم لازم نیست: ابتدای هر اجرای تازه در LetsGoBtn_Click
                // خودش DoResetCountersDisplay() را صدا می‌زند. پس نتیجه‌ی اجرا روی صفحه
                // می‌ماند تا کاربر ببیند کدام بخش‌ها کامل شده‌اند.
                // ───────────────────────────────────────────────────────────────────────────
                TOGHER_PROGRESS.Value = 100;
                COUNTER_TXBL.Content = $"100%";
                UpdateDataInSharedViewModel();

                C00.Foreground = Generaly.PutThisColor("#FF000000");
                C0.Foreground = Generaly.PutThisColor("#FF000000");
                c1.Foreground = Generaly.PutThisColor("#FF000000");
                c2.Foreground = Generaly.PutThisColor("#FF000000");
                c3.Foreground = Generaly.PutThisColor("#FF000000");
                c4.Foreground = Generaly.PutThisColor("#FF000000");
                c5.Foreground = Generaly.PutThisColor("#FF000000");
                c6.Foreground = Generaly.PutThisColor("#FF000000");
                c7.Foreground = Generaly.PutThisColor("#FF000000");
                c8.Foreground = Generaly.PutThisColor("#FF000000");
                c9.Foreground = Generaly.PutThisColor("#FF000000");
                c10.Foreground = Generaly.PutThisColor("#FF000000");
                c11.Foreground = Generaly.PutThisColor("#FF000000");
                Btn_DoCancel.Content = "لغو";
            }));
        }
        private void Btn_DoCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { Btn_DoCancel.Content = "تلاش برای توقف ... "; }));
            CancelerTOKEN.Cancel();
        }

        private void SetCheckboxesChecked(bool isChecked)
        {
            // Get all checkboxes in the visual tree of the Window
            IEnumerable<CheckBox> checkboxes = FindVisualChildren<CheckBox>(this);

            // Loop through each checkbox and set the IsChecked property
            foreach (CheckBox checkbox in checkboxes)
            {
                if (checkbox.Name == "FORMOL" || checkbox.Name == "defacc" || checkbox.Name == "C00" || checkbox.Name == "UseParallelProcessing" || checkbox.Name == "chkUseSmartThrottling")
                {
                }
                else
                {
                    checkbox.IsChecked = isChecked;
                }
            }
        }
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private bool IsAtLeasOnChecked()
        {
            IEnumerable<CheckBox> checkboxes = FindVisualChildren<CheckBox>(this);

            // Loop through each checkbox and set the IsChecked property
            foreach (CheckBox checkbox in checkboxes)
            {
                if (checkbox.Name == "FORMOL") { /*ignore*/ }
                else if (checkbox.Name == "defacc") { /*ignore*/ }
                else if (checkbox.Name == "chkUseSmartThrottling") { /*ignore*/ }
                else
                {
                    if ((bool)checkbox.IsChecked)
                        return true;
                }
            }
            return false;
        }
        public async void LetsGoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StillMethodIsWorking) return;
            int repeatCount = 1;
            bool enteredDurabilityScope = false;

            Dispatcher.Invoke(new Action(() =>
            {
                if (IsAtLeasOnChecked() is false) { return; }

                Generaly.DoResetCountersDisplay();

                if (!int.TryParse(repeatb.Text.Trim(), out repeatCount) || repeatCount <= 0)
                {
                    repeatCount = 1;
                }
            }));


            //{Begin---------------------------------
            try
            {
                DelayedDurabilityGuard.EnterRebuildScope();
                enteredDurabilityScope = true;

                for (int r = 0; r < repeatCount; r++)
                {
                    tasks = new List<Task>();
                    AnyErrorHappend = false;

                    // ───────────────────────────────────────────────────────────────────────
                    // باطل کردن گزارش‌های پیشرفتِ عقب‌مانده‌ی اجرای قبلی و صفر کردن نوارها.
                    //
                    // چرا: ThrottledProgressReporter با اولویت Background صف می‌کند، پس
                    // callback های آخرِ اجرای قبلی می‌توانند «بعد» از صفر شدن نوارها اجرا شوند
                    // و مقدار کهنه بنویسند. با جلو بردن نسل، همه‌ی آن‌ها بی‌اثر می‌شوند.
                    // ترتیب مهم است: اول نسل، بعد صفر کردن.
                    // ───────────────────────────────────────────────────────────────────────
                    CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
                    Generaly.DoResetCountersDisplay();

                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (LST_DATA5.Count > 30)
                        {
                            LST_DATA5.CollectionChanged -= LST_DATA5_CollectionChanged;
                            LST_DATA5?.Clear();
                            LST_DATA5.CollectionChanged += LST_DATA5_CollectionChanged;
                            Properties.Settings.Default.TheHistoryLST = null;
                            Properties.Settings.Default.Save();
                        }

                        LST_DATA5.Add("شروع" + Conversions.ToString(DateTime.Now));
                        StillMethodIsWorking = true;
                    }));

                    // کش خاموش است تا C0 و C00 تمام شوند.
                    //
                    // چرا مهم است: C0 (بازسازی نرخ میانگین) به dbo.DTL_MANF می‌نویسد و
                    // GETSTANDARDPRICE_MAVAD/DAST/SAR دقیقاً از همان جدول می‌خوانند. اگر کش
                    // در طول C0 روشن باشد و کسی روزی از داخل C0 یکی از آن توابع را صدا بزند،
                    // مقدارِ «قبل از اصلاح» تا پایان بازسازی در کش قفل می‌شود و بهای تمام‌شده
                    // غلط ثبت می‌گردد.
                    //
                    // امروز C0 هیچ‌کدام از توابع کش‌شده را صدا نمی‌زند (بررسی شد)، ولی اتکا به
                    // این موضوع شکننده است. با روشن‌کردن کش بعد از C0/C00، این وابستگی از بین
                    // می‌رود: هر چه کش می‌شود، حتماً بعد از نهایی‌شدن DTL_MANF خوانده شده است.
                    if (Generaly.C0) { await Task.Run(async () => { await C0_TASK(); }); } //باز سازی نرخ میانگین
                    if (Generaly.C00) { await Task.Run(async () => { await C00_TASK(); }); } //باز سازی موجودی انبار

                    // کش جستجوهای تکراری (نام حساب، نام دپارتمان، وجود حساب تفصیلی) فقط در
                    // طول همین بازسازی دسته‌ای فعال است و برای هر اجرا از نو ساخته می‌شود.
                    // بیرون از این محدوده خاموش می‌ماند، چون فرم‌های برنامه‌ی اصلی هم همین
                    // توابع را صدا می‌زنند و آنجا کاربر می‌تواند وسط کار نام حساب را عوض کند.
                    CL_HESABDARI_AUTO_BAZ.ClearLookupCaches();
                    CL_HESABDARI_AUTO_BAZ.LookupCacheEnabled = true;

                    if (Generaly.C1) { tasks.Add(C1_TASK()); } //سند فروش
                    if (Generaly.C2) { tasks.Add(C2_TASK()); } //سند خرید
                    if (Generaly.C3) { tasks.Add(C3_TASK()); } //سند خزانه
                    if (Generaly.C4) { tasks.Add(C4_TASK()); } //سند انتقالی
                    if (Generaly.C5) { tasks.Add(C5_TASK()); } //سند خروج مواد
                    if (Generaly.C6) { tasks.Add(C6_TASK()); } //سند خروج سایر
                    if (Generaly.C7) { tasks.Add(C7_TASK()); } //سند تولید ورود
                    if (Generaly.C8) { tasks.Add(C8_TASK()); } //سند برگشت فروش + آزاد
                    if (Generaly.C9) { tasks.Add(C9_TASK()); } //سند برگشت فروش + آزاد
                    if (Generaly.C10) { tasks.Add(C10_TASK()); } //سند برگشت فروش + آزاد
                    if (Generaly.C11) { tasks.Add(C11_TASK()); } // سند وصولی اسناد دریافتنی

                    // Start all tasks concurrently
                    var allTasks = Task.WhenAll(tasks); //Start and Wait until When all tasks are finished.

                    #region MyRegion
                    //این تیکه رو فقط برای دیباگ استفاده میکنم
                    //await allTasks;

                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    //    IsWorkisDone = true;
                    //    if (AnyErrorHappend)
                    //    {
                    //        foreach (var item in ERTRACKLIST)
                    //        {
                    //            LST_DATA5.Add(item.SectionName);
                    //        }
                    //        ERTRACKLIST?.Clear();
                    //        LogWriter.WriteLog($@"ERTRACKLIST : {ERTRACKLIST.Count} => {ERTRACKLIST.FirstOrDefault()}");
                    //        LST_DATA5.Add("پایان یافته با خطا :" + Conversions.ToString(DateTime.Now));
                    //    }
                    //    else //Successfull
                    //    {
                    //        LST_DATA5.Add("پايان :" + Conversions.ToString(DateTime.Now));
                    //    }

                    //}));
                    //SayOprationsFinished();
                    //return;
                    #endregion


                    try
                    {
                        await allTasks;

                        // ───────────────────────────────────────────────────────────────────
                        // تخلیه‌ی صف گزارش‌های پیشرفت پیش از پایان‌دهی.
                        //
                        // ادامه‌ی await با اولویت Normal اجرا می‌شود، ولی گزارش‌های پیشرفت با
                        // اولویت Background صف شده‌اند؛ پس بدون این خط، آخرین Complete()ها
                        // «بعد» از پایان‌دهی روی نوارها می‌نشستند. این InvokeAsync خالی صبر
                        // می‌کند تا نوبت اولویت Background برسد، یعنی تا آن لحظه هرچه در صف
                        // بوده اجرا شده است.
                        //
                        // (محافظ قطعی همان نسل است — این خط فقط باعث می‌شود مقادیر واقعی ۱۰۰٪
                        //  پیش از پایان‌دهی روی صفحه بنشینند، نه اینکه دور ریخته شوند.)
                        // ───────────────────────────────────────────────────────────────────
                        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            IsWorkisDone = true;
                            if (AnyErrorHappend)
                            {
                                lock (ERTRACKLIST)
                                {
                                    foreach (var item in ERTRACKLIST)
                                    {
                                        LST_DATA5.Add(item.SectionName);
                                    }
                                    LogWriter.WriteLog($@"ERTRACKLIST : {ERTRACKLIST.Count} => {ERTRACKLIST.FirstOrDefault()?.SectionName}");
                                    ERTRACKLIST?.Clear();
                                }
                                LST_DATA5.Add("پایان یافته با خطا :" + Conversions.ToString(DateTime.Now));
                            }
                            else //Successfull
                            {
                                LST_DATA5.Add("پايان :" + Conversions.ToString(DateTime.Now));
                            }

                        }));

                        // از این لحظه هیچ گزارش عقب‌مانده‌ای نباید نوارها را عوض کند.
                        CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
                        SayOprationsFinished();
                    }
                    catch (OperationCanceledException ecx)
                    {
                        CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
                        Dispatcher.Invoke(new Action(() =>
                        {
                            TOGHER_PROGRESS.Value = 0;
                            COUNTER_TXBL.Content = $"0.00%";
                            LST_DATA5.Add("لغو شده :" + Conversions.ToString(DateTime.Now));
                        }));

                        StillMethodIsWorking = false;

                        var taskInfo = string.Join(", ", tasks.Select(t => $"Id:{t.Id},Status:{t.Status}"));
                        LogWriter.WriteLog($"Operation canceled in LetsGoBtn_Click (iteration {r + 1} of {repeatCount}). Tasks: {taskInfo}");
                        ExpectionLogWriter.WriteLog(ecx, "OperationCanceledException in LetsGoBtn_Click");

                        Console.WriteLine("Operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
                        StillMethodIsWorking = false;

                        LST_DATA5.Add("به خاطر خطا لغو شد. :" + Conversions.ToString(DateTime.Now));
                        Btn_DoCancel.Content = "لغو";

                        var taskInfo = string.Join(", ", tasks.Select(t => $"Id:{t.Id},Status:{t.Status}"));
                        var errorSections = "";
                        lock (ERTRACKLIST)
                        {
                            errorSections = string.Join(" | ", ERTRACKLIST.Select(x => x.SectionName));
                        }
                        LogWriter.WriteLog($"Exception in LetsGoBtn_Click (iteration {r + 1} of {repeatCount}). AnyErrorHappend={AnyErrorHappend}. Tasks: {taskInfo}. ErrorSections: {errorSections}");
                        ExpectionLogWriter.WriteLog(ex, "Exception in LetsGoBtn_Click");

                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            finally
            {
                // کش جستجو فقط تا پایان بازسازی زنده می‌ماند؛ بیرون از آن باید خاموش و خالی شود
                // تا فرم‌های برنامه همیشه مقدار تازه از دیتابیس بخوانند.
                CL_HESABDARI_AUTO_BAZ.LookupCacheEnabled = false;
                CL_HESABDARI_AUTO_BAZ.ClearLookupCaches();

                // محافظ نهایی: اگر مسیری (مثلاً DelayedDurabilityGuard) استثنا بدهد و از
                // بلوک‌های catch بالا رد نشویم، گزارش‌های عقب‌مانده نباید نوارها را عوض کنند.
                CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();

                if (enteredDurabilityScope)
                {
                    DelayedDurabilityGuard.ExitRebuildScope();
                }

                // لاگ بافرشده است؛ پیش از پایان، صف باید روی دیسک بنشیند تا فایل لاگ کامل باشد.
                LogWriter.Flush();
            }


            //End}-----------------------------------
        }

        public async Task C00_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    int errorno;
                    string SQL;
                    long CON, i;
                    double MIAN;
                    double MBKM;
                    var MOGUDI = default(double);
                    double temp;

                    //باز سازی موجودی انبار
                    LogWriter.WriteLog("باز سازی موجودی انبار شروع");
                    if (IsCancelRequestedBgWorker) { return; }
                    var rst = dbms.DoGetDataSQL<double?>("SELECT INVO_LST.RADIF FROM INVO_LST ORDER BY INVO_LST.RADIF").ToList();
                    i = 1L;
                    VBMath.Randomize();

                    for (int st = 0; st < rst.Count; st++) //while (!rst.EOF())
                    {
                        rst[st] = i;
                        dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET RADIF = {i} WHERE RADIF = {rst[st]}");
                        //rst.update();
                        i = i + 1L;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            double progress = (st + 1) / ((double)rst.Count) * 100.0; // Calculate the progress percentage
                            PRGR_C00.Value = progress; // Update the progress bar
                            LBL_C00.Content = $"{progress:F2}%";
                            UpdateOverallProgressBar();
                        }));


                        // rst.MoveNext();
                    }
                    //rst.Close();
                    //DoCmd.SetWarnings(false);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (0 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.OpenStoredProcedure("BAZSAZI");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (1 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.OpenStoredProcedure("BAZSAZIA");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (2 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.OpenStoredProcedure("BAZSAZIF");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (3 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.OpenStoredProcedure("BAZMAR");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (4 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.OpenStoredProcedure("BAZMANDO");
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (5 + 1) / ((double)5) * 100.0; // Calculate the progress percentage
                        PRGR_C00.Value = progress; // Update the progress bar
                        LBL_C00.Content = $"{progress:F2}%";
                    }));
                    dbms.DoExecuteSQL("UPDATE    dbo.STUF_STK SET MOGODI_A = 0, MOGODI = 0");
                    var rst_STUF_STK = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK").ToList();
                    var RST2_B = dbms.DoGetDataSQL<B_MOG_ANBARHA>("SELECT * FROM B_MOG_ANBARHA").ToList();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Text23.Text = rst_STUF_STK.Count.ToString();
                    }));
                    for (int h = 0; h < rst_STUF_STK.Count; h++) // while (!rst_STUF_STK.EOF)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            co.Text = rst_STUF_STK[h].CODE.ToString();

                            double progress = (h + 1) / ((double)rst_STUF_STK.Count) * 100.0; // Calculate the progress percentage
                            PRGR_C00.Value = progress; // Update the progress bar
                            LBL_C00.Content = $"{progress:F2}%";
                        }));
                        var RST2_BFilter = RST2_B.Where(x => x.CODE == rst_STUF_STK[h].CODE && x.ANBAR == rst_STUF_STK[h].ANBAR).ToList();
                        //RST2_B.Filter = "CODE = '" + rst_STUF_STK[h].("CODE") + "' AND ANBAR = " + rst_STUF_STK[h].("ANBAR");
                        if (RST2_BFilter.Count == 0)
                        {
                        }
                        else
                        {
                            if (rst_STUF_STK[h].MOGODI_A + rst_STUF_STK[h].MOGODI != RST2_BFilter.FirstOrDefault().MAND)
                            {
                                rst_STUF_STK[h].MOGODI = (double)(RST2_BFilter.FirstOrDefault().MAND - rst_STUF_STK[h].MOGODI_A);

                                dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK SET MOGODI = {rst_STUF_STK[h].MOGODI} WHERE CODE = {rst_STUF_STK[h].CODE} AND ANBAR = {rst_STUF_STK[h].ANBAR}");
                                //rst_STUF_STK.update();
                            }
                            i = i + 1L;
                        }
                        //rst_STUF_STK[h].MOGODI = Math.Round(rst_STUF_STK[h].MOGODI * Math.Pow(10, (double)Baseknow.DIG)) / Math.Pow(10, (double)Baseknow.DIG);

                        dbms.DoExecuteSQL($@"UPDATE dbo.STUF_STK SET MOGODI = {rst_STUF_STK[h].MOGODI} WHERE CODE = {rst_STUF_STK[h].CODE} AND ANBAR = {rst_STUF_STK[h].ANBAR}");

                        //rst_STUF_STK.update();
                        //rst_STUF_STK.MoveNext();
                        //DoEvents();
                        Dispatcher.Invoke(new Action(() =>
                        {
                            Text19.Text = h.ToString();
                        }));
                    }
                    //RST2_B.Close();
                    //rst_STUF_STK.Close();
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در بازسازی موجودی انبار" });

                    LogWriter.WriteLog("باز سازی موجودی انبار خطا : " +
                        $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source}");
                }
                finally
                {
                    LogWriter.WriteLog("باز سازی موجودی انبار پایان");
                }

            });
            Dispatcher.Invoke(new Action(() => { C00.Foreground = Generaly.PutThisColor(); }));

        }
        /// <summary>
        /// همان منبع تراکنش‌ها برای شاخه‌ی «OPTIONSS[66] غیر از 5».
        /// تفاوتش با BuildAvgRebuildSourceSql فقط نبودِ شرط (tag &lt;&gt; 20 and tag &lt;&gt; 23) است؛
        /// عیناً از متن VIEW قبلی گرفته شده.
        ///
        /// ⚠️ چرا VIEW حذف شد: هر سه جای این شاخه یک VIEW با نام ثابت TMPAV101 می‌ساختند.
        /// اگر دو کاربر همزمان بازسازی بزنند، کاربر دوم VIEW کاربر اول را DROP و با فیلتر
        /// کالای خودش دوباره CREATE می‌کند؛ آنگاه SELECT کاربر اول داده‌ی کالای کاربر دوم را
        /// برمی‌گرداند و بدون هیچ خطایی میانگین اشتباه ثبت می‌شود.
        /// با اجرای مستقیم کوئری، دیگر شیء مشترکی وجود ندارد که تداخل کند.
        ///
        /// ⚠️ شرط (ISNULL(INVO_LST.MEGH_MAR, 0) &lt;&gt; 0) روی شاخه‌های HEAD_LST_FBK / HEAD_LST_KBK:
        /// اقلام برگشتی رکورد مستقل در INVO_LST ندارند و مقدار مرجوعی در ستون MEGH_MAR همان
        /// ردیفِ فاکتور اصلی نگهداری می‌شود. Join فقط با سرِ سند (NUMBER1/dtag) انجام می‌شود،
        /// بنابراین بدون این شرط «همه‌ی» ردیف‌های فاکتور مرجع — حتی ردیف‌هایی که اصلاً برگشت
        /// نخورده‌اند (MEGH_MAR = 0) — با TAG = 3/4 وارد جریان بازسازی می‌شدند و روی همان ردیف
        /// AVRAGE2 نوشته می‌شد؛ ضمناً اگر در آن لحظه MOGUDI صفر بود، شاخه‌ی «MOGUDI == 0»
        /// مقدار MBKM را صفر می‌کرد و نرخ میانگینِ تراکنش‌های بعدی هم منحرف می‌شد.
        /// همین شرط در تولید سند حسابداری برگشت فروش هم اعمال شده است
        /// (CL_HESABDARI_AUTO_BAZ: INVO_LST_TAKH.MEGH_MAR &lt;&gt; 0)، پس دو مسیر هم‌راستا می‌شوند.
        /// </summary>
        private static string BuildAvgRebuildSourceSqlAllTags(string? code, int anbar, string dt)
        {
            return "SELECT     TOP 100 PERCENT DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID,  MEGH_R , RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, ID, BARGAH FROM ( "
                 + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,  dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO,dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE WHERE  (dbo.INVO_LST.CODE = '" + code + "') AND (dbo.INVO_LST.ANBAR = " + anbar + ") AND (dbo.HEAD_LST.DATE_N > " + dt + ") UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1 = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + code + "') AND (dbo.INVO_LST.ANBARF = " + anbar + ") AND (dbo.HEAD_LST.DATE_N > " + dt + ")  AND (dbo.INVO_LST.TAG = 5) UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN       dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag INNER JOIN  dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + code + "') AND (dbo.INVO_LST.ANBAR = " + anbar + ") AND (dbo.HEAD_LST_FBK.DATE_N > " + dt + ") AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0) UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A,dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH  FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag INNER JOIN " + " dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE WHERE     (dbo.INVO_LST.CODE = '" + code + "') AND (dbo.INVO_LST.ANBAR = " + anbar + ") AND (dbo.HEAD_LST_KBK.DATE_N > " + dt + ") AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0) union " + " SELECT     TOP 100 PERCENT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK, 0 AS megh_mar,' ' AS mol, dbo.ANBGRD_LST.MABL, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK, 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no, 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage, 0 AS id, '17' AS Expr1 FROM   dbo.ANBGRD_LST INNER JOIN  dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM INNER JOIN  dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE " + " WHERE      (dbo.ANBGRD_LST.CODE = '" + code + "') AND (dbo.ANBGRD_HEAD.GRD_ANBAR = " + anbar + ") and ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * - 1 <> 0) AND (dbo.ANBGRD_HEAD.GRD_DATE > " + dt + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL)) "
                 + " ) AS AVGSRC ORDER BY DATE_N, BARGAH, ID";
        }


        /// <summary>
        /// منبع تراکنش‌های «بازسازی نرخ میانگین» برای یک (کالا، انبار) — یا برای «همه‌ی
        /// انبارهای یک کالا» وقتی <paramref name="anbar"/> برابر null باشد (پردازش
        /// ادغام‌شده‌ی کالاهای چرخه‌دار؛ نگاه کنید OrderAnbarsForTransferDependencies).
        ///
        /// قبلاً همین SQL به شکل «DROP VIEW ← CREATE VIEW ← SELECT از VIEW ← DROP VIEW» اجرا می‌شد؛
        /// یعنی ۴ رفت‌وبرگشت و دو دستور DDL به ازای هر کالا. DDL روی کاتالوگ سیستم قفل Sch-M
        /// می‌گیرد و Recompile ایجاد می‌کند، پس در حلقه‌ی موازی Thread ها پشت هم صف می‌کشیدند.
        /// اینجا همان متن SQL داخل یک Derived Table قرار گرفته و ORDER BY بیرونی روی آن اعمال می‌شود.
        /// UNION (و نه UNION ALL) عیناً حفظ شده تا حذف رکوردهای تکراری مثل قبل انجام شود.
        ///
        /// ⚠️ ترتیب داخل یک روز: TAGCOD.BARGAH (متن) نیست — TAGCOD.tartib است، یک ستون عددی
        /// که دقیقاً برای همین منظور در همین جدول وجود دارد. مقایسه‌ی متنیِ BARGAH به ترتیب
        /// الفبای فارسی و به فاصله‌های ابتدایی وابسته است، نه به ترتیب واقعیِ کسب‌وکار؛ مثلاً
        /// «انتقالی-ورود» (tartib=10) باید قبل از «برگشت خرید آزاد» (tartib=11) بیاید ولی این دو
        /// به‌عنوان متن با هم می‌آمیزند.
        ///
        /// ⚠️ ID به‌عنوان تای‌برک نهایی: بدون آن، ترتیب ردیف‌های هم‌روز و هم‌tartib (چند فاکتور
        /// فروش یا چند رسید خرید در یک روز) را SQL Server تضمین نمی‌کند. چون این پیمایش متوالی و
        /// حالت‌مند است (هر ردیف روی میانگین متحرکِ ردیف قبلی بنا می‌شود)، یک تای نامعین در ابتدای
        /// سال کل مسیر میانگین را تا انتهای سال منحرف می‌کند و نتیجه‌ی دو اجرای پیاپی فرق می‌کند.
        /// ID شناسه‌ی IDENTITY است، پس صعودی = ترتیب واقعیِ درج.
        ///
        /// ⚠️ شاخه‌های BACK_HEAD (برگشت فروش ta=2 و برگشت خرید ta=1): قبلاً اصلاً وجود نداشتند و
        /// برگشت‌ها فقط از HEAD_LST_FBK/HEAD_LST_KBK می‌آمدند؛ روی دیتابیس‌هایی که این دو جدول
        /// پشتیبان را ندارند (رول‌آور سال مالی که هرگز اجرا نشده) هیچ برگشتی روی نرخ میانگین اثر
        /// نمی‌گذاشت — case 3 و بخشی از case 4 کد داشتند ولی هرگز ردیفی به آن‌ها نمی‌رسید.
        /// BACK_HEAD.DATE_N تاریخ واقعیِ خودِ برگشت است (همان منبعی که گزارش کارت کالا از آن
        /// استفاده می‌کند)، پس برگشت در تاریخ خودش اثر می‌کند نه تاریخ سند اصلی.
        ///
        /// ⚠️ سنتینل tartib = 9999 روی شاخه‌ی برگشت فروش، فقط وقتی برگشت هم‌روزِ فروشِ اصلی باشد:
        /// case 4 مقدار line.AVRAGE (نرخ منجمدِ لحظه‌ی فروش) را می‌خواند. tartib واقعیِ TAGCOD برای
        /// کد ۴ برابر ۶ و برای کد ۲ برابر ۱۸ است، یعنی در حالت هم‌روز، برگشت پیش از خودِ فروش
        /// پردازش می‌شد و line.AVRAGE هنوز صفر بود (MBKM با صفر جمع می‌شد ولی MOGUDI بدون مقابل
        /// ارزشی رشد می‌کرد). با هل‌دادن به انتهای همان روز این حل می‌شود. وقتی تاریخ برگشت با
        /// تاریخ فروش یکی نیست، line.AVRAGE از قبل درست است و سنتینل لازم نیست — و اگر بی‌دلیل
        /// اعمال شود، ردیف بعد از رویدادهای نامرتبطِ همان روز می‌افتد و AVRAGE2 غلط می‌شود.
        ///
        /// ⚠️ شرط (ISNULL(INVO_LST.MEGH_MAR, 0) &lt;&gt; 0) روی شاخه‌های HEAD_LST_FBK / HEAD_LST_KBK:
        /// اقلام برگشتی رکورد مستقل در INVO_LST ندارند و مقدار مرجوعی در ستون MEGH_MAR همان
        /// ردیفِ فاکتور اصلی نگهداری می‌شود. Join فقط با سرِ سند (NUMBER1/dtag) انجام می‌شود،
        /// بنابراین بدون این شرط «همه‌ی» ردیف‌های فاکتور مرجع — حتی ردیف‌هایی که اصلاً برگشت
        /// نخورده‌اند — با TAG = 3/4 وارد جریان بازسازی می‌شدند. همین شرط روی دو شاخه‌ی
        /// BACK_HEAD هم اعمال شده است.
        /// </summary>
        private static string BuildAvgRebuildSourceSql(
            string? code, int? anbar, string dt,
            bool hasFbk, bool hasKbk, bool useBackHeadSaleReturn, bool useBackHeadPurchaseReturn)
        {
            var c = SqlText(code);

            // anbar == null یعنی «همه‌ی انبارهای این کالا یک‌جا».
            string ANB(string column) => anbar.HasValue ? $" AND ({column} = {anbar.Value})" : string.Empty;

            const string COLS = "DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, "
                              + "FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, "
                              + "N_TAF, AVRAGE, ID, BARGAH, tartib";

            const string INVO_COLS = "dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, "
                                   + "dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, "
                                   + "dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, "
                                   + "dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, "
                                   + "dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID";

            var parts = new List<string>();

            // ── اصلی: همه‌ی برگه‌های خودِ این انبار ─────────────────────────────
            parts.Add(
                " SELECT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, "
                + INVO_COLS + ", dbo.TAGCOD.BARGAH, dbo.TAGCOD.tartib"
                + " FROM dbo.INVO_LST"
                + " INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG"
                + " INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE"
                + " WHERE (dbo.INVO_LST.tag <> 20 AND dbo.INVO_LST.tag <> 23) AND (dbo.INVO_LST.CODE = '" + c + "')"
                + ANB("dbo.INVO_LST.ANBAR")
                + " AND (dbo.HEAD_LST.DATE_N > " + dt + ")");

            // ── انتقالیِ ورود: همان ردیف حواله، این بار به نام انبار مقصد ───────
            parts.Add(
                " SELECT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, "
                + INVO_COLS + ", dbo.TAGCOD.BARGAH, dbo.TAGCOD.tartib"
                + " FROM dbo.INVO_LST"
                + " INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG"
                + " INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1 = dbo.TAGCOD.CODE"
                + " WHERE (dbo.INVO_LST.CODE = '" + c + "')"
                + ANB("dbo.INVO_LST.ANBARF")
                + " AND (dbo.HEAD_LST.DATE_N > " + dt + ") AND (dbo.INVO_LST.TAG = 5)");

            // ── انبارگردانی (کسری = 17، اضافه = 18) ────────────────────────────
            //    tartib اینجا هاردکد است چون TAG این شاخه واقعی نیست و خودِ همین کوئری
            //    با CASE می‌سازدش؛ معادل عددیِ همان دو ردیف TAGCOD (کد ۱۷ ⇒ ۵، کد ۱۸ ⇒ ۱۳).
            parts.Add(
                " SELECT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,"
                + " dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,"
                + " (dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK,"
                + " 0 AS megh_mar, ' ' AS mol, dbo.ANBGRD_LST.MABL,"
                + " ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK,"
                + " 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no,"
                + " 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage,"
                + " 0 AS id, '17' AS Expr1,"
                + " CASE WHEN (dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) > 0 THEN 13 ELSE 5 END AS tartib"
                + " FROM dbo.ANBGRD_LST"
                + " INNER JOIN dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM"
                + " INNER JOIN dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE"
                + " WHERE (dbo.ANBGRD_LST.CODE = '" + c + "')"
                + ANB("dbo.ANBGRD_HEAD.GRD_ANBAR")
                + " AND ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * -1 <> 0)"
                + " AND (dbo.ANBGRD_HEAD.GRD_DATE > " + dt + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL))");

            if (useBackHeadSaleReturn)
            {
                // برگشت فروش (BACK_HEAD.ta = 2 ⇒ TAG = 4) در تاریخ واقعیِ خودِ برگشت.
                parts.Add(
                    " SELECT dbo.BACK_HEAD.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, "
                    + INVO_COLS + ", '' AS BARGAH,"
                    + " CASE WHEN dbo.BACK_HEAD.DATE_N = HS.DATE_N THEN 9999 ELSE 6 END AS tartib"
                    + " FROM dbo.BACK_HEAD"
                    + " INNER JOIN dbo.INVO_LST ON dbo.INVO_LST.TAG = dbo.BACK_HEAD.ta AND dbo.INVO_LST.NUMBER = dbo.BACK_HEAD.NUMBER1"
                    + " INNER JOIN dbo.HEAD_LST HS ON HS.NUMBER = dbo.INVO_LST.NUMBER AND HS.TAG = dbo.INVO_LST.TAG"
                    + " WHERE (dbo.BACK_HEAD.ta = 2) AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0)"
                    + " AND (dbo.INVO_LST.CODE = '" + c + "')"
                    + ANB("dbo.INVO_LST.ANBAR")
                    + " AND (dbo.BACK_HEAD.DATE_N > " + dt + ")");
            }

            if (useBackHeadPurchaseReturn)
            {
                // برگشت خرید (BACK_HEAD.ta = 1 ⇒ TAG = 3). اینجا سنتینل لازم نیست: case 3
                // از نرخ منجمدِ ردیف (line.AVRAGE) استفاده نمی‌کند، فقط از MIAN جاری — که با
                // هر ترتیبی در همان روز معتبر است. tartib واقعیِ TAGCOD برای کد ۳ = ۱۵.
                parts.Add(
                    " SELECT dbo.BACK_HEAD.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, "
                    + INVO_COLS + ", '' AS BARGAH, 15 AS tartib"
                    + " FROM dbo.BACK_HEAD"
                    + " INNER JOIN dbo.INVO_LST ON dbo.INVO_LST.TAG = dbo.BACK_HEAD.ta AND dbo.INVO_LST.NUMBER = dbo.BACK_HEAD.NUMBER1"
                    + " WHERE (dbo.BACK_HEAD.ta = 1) AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0)"
                    + " AND (dbo.INVO_LST.CODE = '" + c + "')"
                    + ANB("dbo.INVO_LST.ANBAR")
                    + " AND (dbo.BACK_HEAD.DATE_N > " + dt + ")");
            }

            if (hasFbk)
            {
                parts.Add(
                    " SELECT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, "
                    + INVO_COLS + ", dbo.TAGCOD.BARGAH, dbo.TAGCOD.tartib"
                    + " FROM dbo.INVO_LST"
                    + " INNER JOIN dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag"
                    + " INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE"
                    + " WHERE (dbo.INVO_LST.CODE = '" + c + "')"
                    + ANB("dbo.INVO_LST.ANBAR")
                    + " AND (dbo.HEAD_LST_FBK.DATE_N > " + dt + ") AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0)");
            }

            if (hasKbk)
            {
                parts.Add(
                    " SELECT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, "
                    + INVO_COLS + ", dbo.TAGCOD.BARGAH, dbo.TAGCOD.tartib"
                    + " FROM dbo.INVO_LST"
                    + " INNER JOIN dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag"
                    + " INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE"
                    + " WHERE (dbo.INVO_LST.CODE = '" + c + "')"
                    + ANB("dbo.INVO_LST.ANBAR")
                    + " AND (dbo.HEAD_LST_KBK.DATE_N > " + dt + ") AND (ISNULL(dbo.INVO_LST.MEGH_MAR, 0) <> 0)");
            }

            return "SELECT " + COLS + " FROM (" + string.Join(" UNION ", parts) + ") AS AVGSRC ORDER BY DATE_N, tartib, ID";
        }

        /// <summary>گریز ساده‌ی تک‌کوتیشن برای درج امنِ کد کالا در متن SQL.</summary>
        private static string SqlText(string? v) => (v ?? string.Empty).Replace("'", "''");

        /// <summary>قالب‌بندی عدد مستقل از Culture برای درج در متن SQL.</summary>
        private static string AvgN(double? v)
            => v.HasValue ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL";

        /// <summary>مانده‌ی متحرکِ یک (کالا، انبار) در طول پیمایش کاردکس.</summary>
        private sealed class AvgAnbarState
        {
            public double MBKM;
            public double MIAN;
            public double MOGUDI;
        }

        /// <summary>یک یالِ وابستگیِ حواله‌ی انتقالی: انبار مبدأ → انبار مقصد، برای یک کالا.</summary>
        private sealed class transfer_edge_model
        {
            public string? CODE { get; set; }
            public int? Src { get; set; }
            public int? Dst { get; set; }
        }

        /// <summary>
        /// انبارهای یک کالا را طوری مرتب می‌کند که مبدأ هر حواله‌ی انتقالی (TAG = 5) همیشه
        /// قبل از مقصدش (TAG = 6 در انبار دیگر) پردازش شود — topological sort ساده‌ی Kahn روی
        /// یال‌های مبدأ→مقصد. اگر انبارهای این کالا هیچ حواله‌ی انتقالی بینشان نداشته باشند
        /// (اکثر کالاها) فوراً همان ترتیب ورودی برمی‌گردد، بدون سربار.
        ///
        /// چرا لازم است: گروه‌بندی بر اساس «کالا» فقط تضمین می‌کند انبارهای یک کالا سریال و روی
        /// یک Thread پردازش شوند؛ ولی *ترتیبِ* آن‌ها همان ترتیب برگشتیِ کوئری (بدون ORDER BY)
        /// است. اگر انبار مقصد زودتر از مبدأ بیفتد، case 6 مقدار MABL_K ای را می‌خواند که case 5
        /// هنوز ننوشته است.
        ///
        /// اگر بین انبارهای این کالا چرخه باشد (هم از A به B و هم، در تاریخ دیگری، از B به A)،
        /// HasCycle = true برمی‌گردد تا فراخوان‌کننده به‌جای ترتیب‌دادن، کاردکسِ همه‌ی انبارها را
        /// در یک جریان زمانیِ مشترک ادغام کند.
        /// </summary>
        private static (List<THE_QUERY1> Ordered, bool HasCycle) OrderAnbarsForTransferDependencies(
            List<THE_QUERY1> codeGroup, List<(int Src, int Dst)>? edges)
        {
            if (edges == null || edges.Count == 0 || codeGroup.Count <= 1) { return (codeGroup, false); }

            var anbarSet = codeGroup.Where(r => r.ANBAR.HasValue).Select(r => r.ANBAR!.Value).ToHashSet();
            var relevantEdges = edges.Where(e => anbarSet.Contains(e.Src) && anbarSet.Contains(e.Dst)).Distinct().ToList();
            if (relevantEdges.Count == 0) { return (codeGroup, false); }

            var indegree = anbarSet.ToDictionary(a => a, _ => 0);
            var adjacency = anbarSet.ToDictionary(a => a, _ => new List<int>());
            foreach (var (src, dst) in relevantEdges)
            {
                adjacency[src].Add(dst);
                indegree[dst]++;
            }

            var queue = new Queue<int>(anbarSet.Where(a => indegree[a] == 0));
            var orderedAnbars = new List<int>(anbarSet.Count);
            while (queue.Count > 0)
            {
                var a = queue.Dequeue();
                orderedAnbars.Add(a);
                foreach (var next in adjacency[a])
                {
                    if (--indegree[next] == 0) { queue.Enqueue(next); }
                }
            }

            if (orderedAnbars.Count < anbarSet.Count) { return (codeGroup, true); }

            var rank = orderedAnbars.Select((a, i) => (a, i)).ToDictionary(x => x.a, x => x.i);
            return (codeGroup.OrderBy(r => r.ANBAR.HasValue && rank.TryGetValue(r.ANBAR.Value, out var i) ? i : int.MaxValue).ToList(), false);
        }

        /// <summary>مانده‌ی اول دوره‌ی یک (کالا، انبار)؛ اگر نرخ اول دوره صفر بود، نرخ استاندارد
        /// و بعد نرخ اولین ورود جایگزین می‌شود — عیناً همان ترتیبِ کد اصلی.</summary>
        private static AvgAnbarState BuildAvgAnbarState(THE_QUERY1 row)
        {
            var st = new AvgAnbarState
            {
                MBKM = row.MABL_A ?? 0,
                MIAN = row.FI_A ?? 0,
                MOGUDI = row.MOGODI_A ?? 0
            };

            if (st.MIAN == 0d)
            {
                st.MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE(row.CODE);
                if (st.MIAN == 0d)
                {
                    st.MIAN = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(row.CODE);
                }
            }

            return st;
        }

        /// <summary>
        /// اجرای دسته‌ای UPDATE های انباشته‌شده.
        ///
        /// ⚠️ بدون BEGIN TRANSACTION: دقیقاً مثل قبل هر دستور جداگانه Commit می‌شود، پس نه قفل
        /// طولانی‌مدت ایجاد می‌شود و نه رفتار در صورت خطا عوض می‌شود. همه‌ی این دستورها Idempotent
        /// اند (مقدار ثابت SET می‌کنند)، بنابراین اگر دسته دوباره اجرا شود مشکلی نیست.
        /// </summary>
        private void FlushAvgPending(List<string> pending)
        {
            const int updateChunkSize = 200;
            for (int off = 0; off < pending.Count; off += updateChunkSize)
            {
                var batch = new StringBuilder();
                var endAt = Math.Min(off + updateChunkSize, pending.Count);
                for (int k = off; k < endAt; k++) { batch.Append(pending[k]).AppendLine(";"); }
                dbms.DoExecuteSQL(batch.ToString());
            }
        }

        /// <summary>
        /// پردازش یک ردیف کاردکس روی مانده‌ی متحرکِ یک انبار.
        ///
        /// از دو مسیر صدا زده می‌شود: حلقه‌ی سریالِ انبار‌به‌انبار (اکثر کالاها) و حلقه‌ی
        /// ادغام‌شده‌ی کالاهای چرخه‌دار. فرمول هر Case دقیقاً همان کد اصلی است؛ فقط
        /// MBKM/MIAN/MOGUDI محلی جایش را به <paramref name="st"/> داده است.
        /// </summary>
        private void ProcessAvgKardexRow(
            string? code,
            cm_model t,
            AvgAnbarState st,
            List<string> pending,
            Dictionary<long, INVO_LST> rst3ById,
            List<ANBGRD_LST> anbgrdRows,
            HashSet<long> touchedByCase5)
        {
            // ⚠️ اگر ID مقدار نداشته باشد نباید کلید ۰ جستجو شود.
            //
            // ⚠️ اینجا نباید «اگر null بود continue» گذاشت: شاخه‌ی انبارگردانیِ کوئری منبع،
            //    ستون id را «0 AS id» می‌دهد، پس ردیف‌های TAG = 17/18 هیچ‌وقت در rst3ById پیدا
            //    نمی‌شوند. با continue کل «کسری/اضافه انبار» از بازسازی حذف می‌شد. این دو case
            //    اصلاً line لازم ندارند.
            INVO_LST? line = null;
            if (t.ID.HasValue && rst3ById.TryGetValue(t.ID.Value, out var invoRow))
            {
                line = invoRow;
            }

            switch (t.TAG)
            {
                case 1: // خريد
                    {
                        st.MBKM = st.MBKM + (t.MABL_K ?? 0);
                        st.MOGUDI = st.MOGUDI + (t.MEGHk ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        line.AVRAGE = st.MIAN;
                        pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        break;
                    }
                case 22: // برگشت فروش سال قبل
                    {
                        if (st.MBKM <= 0d)
                        {
                            st.MBKM = (t.MABL ?? 0) * (t.MEGH_MAR ?? 0);
                        }
                        else
                        {
                            st.MBKM = st.MBKM + st.MIAN * (t.MEGH_MAR ?? 0);
                        }
                        st.MOGUDI = st.MOGUDI + (t.MEGH_MAR ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        line.AVRAGE = st.MIAN;
                        pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        break;
                    }
                case 24: // برگشت فروش سال قبل
                    {
                        if (st.MBKM <= 0d)
                        {
                            st.MBKM = t.MABL_K ?? 0;
                        }
                        else
                        {
                            st.MBKM = st.MBKM + (t.MEGHk ?? 0) * st.MIAN;
                        }
                        st.MOGUDI = st.MOGUDI + (t.MEGHk ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        line.AVRAGE = st.MIAN;
                        pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        break;
                    }
                case 2: // فروش
                    {
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        line.AVRAGE = st.MIAN;
                        pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        break;
                    }
                case 3: // برگشت خريد
                    {
                        st.MBKM = st.MBKM - (t.MEGH_MAR ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGH_MAR ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        if (line != null)
                        {
                            line.AVRAGE2 = st.MIAN;
                            pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE2 = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        }
                        break;
                    }
                case 4: // برگشت فروش
                    {
                        st.MBKM = st.MBKM + (t.MEGH_MAR ?? 0) * (line?.AVRAGE ?? 0);
                        st.MOGUDI = st.MOGUDI + (t.MEGH_MAR ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        if (line != null)
                        {
                            line.AVRAGE2 = st.MIAN;
                            pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE2 = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        }
                        break;
                    }
                case 5: // انتقالي خروج
                    {
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        line.AVRAGE = st.MIAN;
                        line.MABL = st.MIAN;
                        line.MABL_K = Math.Round(st.MIAN * (t.MEGHk ?? 0));
                        // ⚠️ علامت‌گذاری برای case 6: از این لحظه به بعد مقدار زنده‌ی
                        //    line.MABL_K معتبر است، نه t.MABL_K که عکسِ لحظه‌ی fetch است.
                        touchedByCase5.Add(line.id);

                        pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} ,
                                                                 MABL = {AvgN(st.MIAN)} ,
                                                                 MABL_K = {AvgN(line.MABL_K)}
                                        WHERE ID = {line.id}");
                        break;
                    }
                case 6: // انتقالي ورود
                    {
                        // ⚠️ اگر case 5 همین ردیف را در همین اجرا لمس کرده باشد، مقدار
                        //    زنده‌ی line.MABL_K درست است و t.MABL_K عکسِ قدیمیِ لحظه‌ی
                        //    خواندنِ کوئری. برای کالاهای بدون چرخه هر دو یکی‌اند (انبار
                        //    مبدأ پیش از انبار مقصد flush شده)، ولی در پردازش ادغام‌شده‌ی
                        //    کالاهای چرخه‌دار همه‌ی انبارها یک‌جا خوانده شده‌اند و فقط
                        //    مقدار زنده معتبر است.
                        var mablKForCase6 = (line != null && touchedByCase5.Contains(line.id))
                            ? line.MABL_K
                            : (t.MABL_K ?? 0);
                        st.MBKM = st.MBKM + mablKForCase6;
                        st.MOGUDI = st.MOGUDI + (t.MEGHk ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        if (line != null)
                        {
                            line.AVRAGE2 = st.MIAN;
                            pending.Add($"UPDATE dbo.INVO_LST SET AVRAGE2 = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        }
                        break;
                    }
                case 10: // مواد خروج
                    {
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        line.AVRAGE = st.MIAN;
                        line.MABL = st.MIAN;
                        line.MABL_K = Math.Round(st.MIAN * (t.MEGHk ?? 0));

                        pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} ,
                                                                 MABL = {AvgN(st.MIAN)} ,
                                                                 MABL_K = {AvgN(line.MABL_K)}
                                        WHERE ID = {line.id}");
                        break;
                    }
                case 11:    // موادساير خروج
                    {
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        line.AVRAGE = st.MIAN;
                        line.MABL = st.MIAN;
                        line.MABL_K = Math.Round(st.MIAN * (t.MEGHk ?? 0));

                        pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} ,
                                                                 MABL = {AvgN(st.MIAN)} ,
                                                                 MABL_K = {AvgN(line.MABL_K)}
                                        WHERE ID = {line.id}");
                        break;
                    }
                case 9:    // توليد
                    {
                        if (t.N_KOL != 0 & !IsNull(t.N_KOL) & Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
                        {
                            List<THE_QUERY2> RST7 = dbms.DoGetDataSQL<THE_QUERY2>("SELECT  dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + t.N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").ToList();
                            if (RST7.Count > 0)
                            {
                                line.MABL = (RST7.FirstOrDefault().IMBIBE_MANF ?? 0) + (RST7.FirstOrDefault().IMBIBE_SAR ?? 0) + (RST7.FirstOrDefault().MABLKs ?? 0);
                                line.MABL_K = Math.Round(((RST7.FirstOrDefault().IMBIBE_MANF ?? 0) + (RST7.FirstOrDefault().IMBIBE_SAR ?? 0) + (RST7.FirstOrDefault().MABLKs ?? 0)) * (t.MEGHk ?? 0));
                                pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                 MABL = {AvgN(line.MABL)} ,
                                                                 MABL_K = {AvgN(line.MABL_K)}
                                        WHERE ID = {line.id}");
                            }
                        }
                        else
                        {
                            line.MABL = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE_KOL(code, t.DATE_N ?? 0L);
                            if (line.MABL == 0)
                            {
                                line.MABL = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(code);
                            }
                            line.MABL_K = Math.Round(line.MABL * (t.MEGHk ?? 0));

                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                 MABL = {AvgN(line.MABL)} ,
                                                                 MABL_K = {AvgN(line.MABL_K)}
                                        WHERE ID = {line.id}");
                        }
                        // ⚠️ عمداً line.MABL_K (مقدارِ تازه محاسبه‌شده) و نه t.MABL_K
                        //    (مقدارِ قبل از بازسازی که هنگام خواندن کوئری در حافظه آمده) استفاده می‌شود.
                        st.MBKM = st.MBKM + line.MABL_K;
                        st.MOGUDI = st.MOGUDI + (t.MEGHk ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        line.AVRAGE = st.MIAN;
                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                 AVRAGE = {AvgN(st.MIAN)}
                                        WHERE ID = {line.id}");
                        break;
                    }

                case 17: // كسري انبار
                    {
                        st.MBKM = st.MBKM + st.MIAN * (t.MEGHk ?? 0);
                        st.MOGUDI = st.MOGUDI + (t.MEGHk ?? 0);
                        if (st.MBKM == 0d)
                        {
                        }
                        // st.MIAN = 0
                        else if (st.MOGUDI == 0d)
                        {
                            // st.MIAN = 0
                            st.MBKM = 0d;
                        }
                        else
                        {
                            st.MIAN = st.MBKM / st.MOGUDI;
                        }
                        // If st.MIAN < 0 Then
                        // st.MIAN = 0
                        // End If
                        var grdRow = anbgrdRows.Where(x => x.CODE == t.CODE && x.GRD_NUM == t.NUMBER).FirstOrDefault();
                        if (grdRow != null) { grdRow.MABL = st.MIAN; }
                        // ⚠️ شرط CODE جا افتاده بود: ANBGRD_LST به ازای هر (GRD_NUM, CODE) یک ردیف
                        //    دارد، پس این UPDATE نرخ همین کالا را روی «همه‌ی کالاهای آن برگه‌ی
                        //    انبارگردانی» می‌نوشت.
                        pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {AvgN(st.MIAN)} WHERE CODE = '{t.CODE}' AND GRD_NUM = {AvgN(t.NUMBER)}");
                        break;
                    }
                case 18: // اضافه انبار
                    {
                        // ⚠️ اینجا عمداً st.MIAN دوباره محاسبه نمی‌شود (برخلاف case 17)؛
                        //    عیناً همان چیزی است که کد اصلی انجام می‌داد.
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        var grdRow = anbgrdRows.Where(x => x.CODE == t.CODE && x.GRD_NUM == t.NUMBER).FirstOrDefault();
                        if (grdRow != null) { grdRow.MABL = st.MIAN; }
                        pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {AvgN(st.MIAN)} WHERE CODE = '{t.CODE}' AND GRD_NUM = {AvgN(t.NUMBER)}");
                        break;
                    }
                case 26: // برگشت خريد
                    {
                        st.MBKM = st.MBKM - (t.MEGHk ?? 0) * st.MIAN;
                        st.MOGUDI = st.MOGUDI - (t.MEGHk ?? 0);
                        line.AVRAGE = st.MIAN;

                        pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {AvgN(st.MIAN)} WHERE ID = {line.id}");
                        break;
                    }
            }
        }


        public async Task C0_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    LogWriter.WriteLog("باز سازی نرخ میانگین شروع");

                    double progress = 0;
                    double rcount = 0;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PRGR_C0.Value = progress; // Update the progress bar
                        UpdateOverallProgressBar();
                    }));

                    //باز سازی نرخ میانگین

                    if (IsCancelRequestedBgWorker) { return; }

                    var RST6_0 = dbms.DoGetDataSQL<Int64>("select count(id) from invo_lst where tag <> 20 and tag <> 23").ToList(); LogWriter.WriteLog($"RST6_0.Count = {RST6_0.Count}");

                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.Text23.Text = RST6_0.FirstOrDefault().ToString();
                    }));

                    var RST6_1 = dbms.DoGetDataSQL<Int64>("SELECT     COUNT(dbo.ANBGRD_HEAD.GRD_NUM) AS Expr1 FROM         dbo.ANBGRD_HEAD INNER JOIN  dbo.ANBGRD_LST ON dbo.ANBGRD_HEAD.GRD_NUM = dbo.ANBGRD_LST.GRD_NUM WHERE (Not (dbo.ANBGRD_HEAD.N_S Is Null)) And (dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3 <> 0)").ToList(); LogWriter.WriteLog($"RST6_1.Count = {RST6_1.Count}");

                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.Text23.Text = Convert.ToString(Convert.ToInt64(Text23.Text) + RST6_1.FirstOrDefault() * 2);
                        rcount = Convert.ToInt64(Text23.Text);
                    }));
                    var RST4 = dbms.DoGetDataSQL<rst4_model>("SELECT TCOD_STUFGROUP.CODE, TCOD_STUFGROUP.NAMES FROM TCOD_STUFGROUP WHERE (((TCOD_STUFGROUP.CODE)<>0)) ORDER BY TCOD_STUFGROUP.NAMES").ToList(); LogWriter.WriteLog($"RST4.Count = {RST4.Count}");
                    var rst3 = dbms.DoGetDataSQL<INVO_LST>("SELECT CODE,ANBAR,ID FROM INVO_LST").ToList(); LogWriter.WriteLog($"rst3.Count = {rst3.Count}");
                    var RST6 = dbms.DoGetDataSQL<ANBGRD_LST>("SELECT * FROM ANBGRD_LST").ToList(); LogWriter.WriteLog($"RST6.Count = {RST6.Count}");

                    // ─────────────────────────────────────────────────────────────────────
                    // جدول‌های اختیاریِ منبع کاردکس.
                    //
                    // HEAD_LST_FBK/HEAD_LST_KBK (پشتیبان سال قبل) روی همه‌ی دیتابیس‌ها نیستند —
                    // شرکتی که رول‌آور سال مالی برایش اجرا نشده این دو را ندارد و کوئری منبع
                    // با خطای «Invalid object name» می‌افتاد. BACK_HEAD هم روی نسخه‌های قدیمی
                    // ممکن است نباشد. با این پرچم‌ها کوئری فقط شاخه‌های موجود را می‌سازد.
                    // ─────────────────────────────────────────────────────────────────────
                    bool TableExists(string name) =>
                        dbms.DoGetDataSQL<int>($"SELECT 1 FROM sys.tables WHERE name = '{name}'").Any();

                    var hasFbk = TableExists("HEAD_LST_FBK");
                    var hasKbk = TableExists("HEAD_LST_KBK");
                    var hasBackHead = TableExists("BACK_HEAD");

                    // ─────────────────────────────────────────────────────────────────────
                    // منبع «برگشت» — BACK_HEAD یا HEAD_LST_FBK/KBK، نه هر دو.
                    //
                    // BACK_HEAD (ta = 2 برگشت فروش، ta = 1 برگشت خرید، DATE_N تاریخ واقعیِ
                    // خودِ برگشت) همان منبعی است که گزارش کارت کالا از آن استفاده می‌کند.
                    // HEAD_LST_FBK/HEAD_LST_KBK جدول‌های پشتیبان سال قبل‌اند و همان مفهوم را
                    // به شکل دیگری نگه می‌دارند. روی دیتابیسی که هر دو وجود دارند، افزودن
                    // هم‌زمانِ هر دو شاخه یک برگشت را دوبار روی نرخ میانگین اثر می‌دهد.
                    //
                    // پس BACK_HEAD فقط جایی به کار می‌رود که معادلِ پشتیبانش وجود ندارد —
                    // دقیقاً همان حالتی که تا امروز باعث می‌شد case 3 (برگشت خرید) کد داشته
                    // باشد ولی هیچ ردیفی به آن نرسد.
                    //
                    // ⚠️ اگر روی دیتابیس شما تأیید شد که این دو منبع هم‌پوشانی ندارند
                    //    (یعنی FBK/KBK فقط برگشت‌های سال قبل و BACK_HEAD فقط سال جاری را
                    //    نگه می‌دارند)، این دو شرط را به «hasBackHead» تنها تغییر دهید.
                    // ─────────────────────────────────────────────────────────────────────
                    var useBackHeadSaleReturn = hasBackHead && !hasFbk;
                    var useBackHeadPurchaseReturn = hasBackHead && !hasKbk;

                    LogWriter.WriteLog($"hasFbk = {hasFbk} , hasKbk = {hasKbk} , hasBackHead = {hasBackHead} , " +
                                       $"useBackHeadSaleReturn = {useBackHeadSaleReturn} , useBackHeadPurchaseReturn = {useBackHeadPurchaseReturn}");

                    // ─────────────────────────────────────────────────────────────────────
                    // نگاشت ID → ردیف INVO_LST.
                    // قبلاً به ازای هر تراکنش «rst3.Where(x => x.id == ...).ToList()» اجرا می‌شد:
                    // پویش خطی روی ۳۹ هزار ردیف + یک List تازه، برای هر یک از ~۴۰ هزار تراکنش
                    // (در مجموع ≈ ۱٫۵ میلیارد مقایسه).
                    // ⚠️ عمداً همان شیء‌های داخل rst3 نگه داشته می‌شوند و کپی گرفته نمی‌شود:
                    //    منطق TAG=4 (برگشت فروش) مقدار AVRAGE ای را می‌خواند که هنگام پردازش
                    //    TAG=2 (فروش) روی همان شیء نوشته شده است.
                    //    ContainsKey هم برای این است که در صورت وجود ID تکراری، مثل
                    //    FirstOrDefault اولین ردیف انتخاب شود.
                    // ─────────────────────────────────────────────────────────────────────
                    var rst3ById = new Dictionary<long, INVO_LST>(rst3.Count);
                    foreach (var invoLine in rst3)
                    {
                        if (!rst3ById.ContainsKey(invoLine.id)) { rst3ById[invoLine.id] = invoLine; }
                    }

                    // قالب‌بندی عدد مستقل از Culture. قبلاً «{MIAN}» مستقیم داخل SQL درج می‌شد و
                    // اگر Culture جاری جداکننده‌ی اعشار غیر از نقطه داشته باشد، SQL خراب می‌شود.
                    // فرمت پیش‌فرض double عیناً حفظ شده تا مقدار درج‌شده تغییر نکند.
                    static string N(double? v) => v.HasValue ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL";

                    // ─────────────────────────────────────────────────────────────────────
                    // گزارش پیشرفت غیرمسدودکننده.
                    // قبلاً به ازای هر تراکنش یک Dispatcher.Invoke مسدودکننده اجرا می‌شد
                    // (خواندن متن TextBlock، جمع، نوشتن). یعنی ~۴۰ هزار بار قفل شدن پشت
                    // تک‌Thread رابط کاربری؛ همین به‌تنهایی حلقه‌ی موازی را سریال می‌کرد.
                    // ─────────────────────────────────────────────────────────────────────
                    long processedRows = 0;
                    long totalRowsForUi = (long)rcount;
                    string? currentCode = string.Empty;

                    var uiProgress = new CL_HESABDARI_AUTO_BAZ.ThrottledProgressReporter(
                        (int)Math.Max(1d, rcount),
                        Dispatcher,
                        value =>
                        {
                            var done = Interlocked.Read(ref processedRows);
                            PRGR_C0.Value = value;
                            Text19.Text = done.ToString();
                            Text23.Text = Math.Max(0L, totalRowsForUi - done).ToString();
                            // ⚠️ این Action روی Thread رابط کاربری و با BeginInvoke اجرا می‌شود،
                            //    پس برخلاف Dispatcher.Invoke قبلی، استثنای آن به حلقه برنمی‌گردد و
                            //    توسط try/catch بیرونی گرفته نمی‌شود. پس اینجا نباید null بدهیم.
                            co.Text = currentCode ?? string.Empty;
                            UpdateOverallProgressBar();
                        });

                    if (Strings.Mid(Baseknow.OPTIONSS, 66, 1) == "5")
                    {
                        var rst = dbms.DoGetDataSQL<THE_QUERY1>("SELECT     dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A FROM         dbo.STUF_DEF INNER JOIN                      dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE GROUP BY dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A").ToList(); LogWriter.WriteLog($" After    if (Strings.Mid(Baseknow.OPTIONSS, 66, 1) == \"5\") rst.Count = {rst.Count}");

                        //قبل از شروع حلقه، کالاهایی که هیچ تراکنشی ندارند را از لیست حذف می‌کنیم تا کوئری بی‌مورد زده نشود.
                        // کالاهایی که فاکتور دارند (INVO_LST)
                        var rst3Lookup = rst3.Select(x => (x.CODE?.Trim(), x.ANBAR)).ToHashSet();
                        // کالاهایی که انبارگردانی دارند (ANBGRD_LST) — بدون ANBAR چون در جدول نیست
                        var rst6CodeLookup = RST6.Select(x => x.CODE?.Trim()).ToHashSet();

                        // ─────────────────────────────────────────────────────────────────────
                        // گروه‌بندی بر اساس «کالا» و نه (کالا، انبار).
                        //
                        // چرا: حواله‌ی انتقالی بین انبارها یک وابستگی ترتیبی می‌سازد؛ TAG = 5
                        // (انتقالي خروج) مقدار MABL_K را روی ردیف INVO_LST می‌نویسد و همان ردیف
                        // بعداً به‌عنوان TAG = 6 (انتقالي ورود) در گروهِ انبار مقصد خوانده می‌شود.
                        // اگر دو انبارِ یک کالا هم‌زمان روی دو Thread پردازش شوند، انبار مقصد
                        // ممکن است مقدار قدیمی را بخواند. با این گروه‌بندی، انبارهای یک کالا
                        // پشت‌سرهم و روی یک Thread پردازش می‌شوند و آپدیت هر انبار پیش از شروع
                        // انبار بعدی روی دیتابیس نشسته است.
                        // (کالاهای مختلف همچنان کاملاً موازی پیش می‌روند.)
                        // ─────────────────────────────────────────────────────────────────────
                        var groupedByCode = rst
                            .Where(r => rst3Lookup.Contains((r.CODE?.Trim(), r.ANBAR ?? 0))   // دارای فاکتور
                                        || rst6CodeLookup.Contains(r.CODE?.Trim()))          // یا دارای انبارگردانی
                            .GroupBy(x => x.CODE)
                            .ToList();
                        LogWriter.WriteLog($"groupedByCode.Count = {groupedByCode.Count}");

                        // ─────────────────────────────────────────────────────────────────────
                        // یال‌های وابستگیِ حواله‌ی انتقالی (انبار مبدأ → انبار مقصد) به ازای هر کالا.
                        // یک بار برای کل اجرا خوانده می‌شود و مبنای ترتیب‌دهی انبارهای هر کالاست
                        // (نگاه کنید OrderAnbarsForTransferDependencies).
                        // ─────────────────────────────────────────────────────────────────────
                        var transferEdgesByCode = dbms.DoGetDataSQL<transfer_edge_model>(
                            @"SELECT DISTINCT i.CODE, i.ANBAR AS Src, CAST(i.ANBARF AS INT) AS Dst
                              FROM dbo.INVO_LST i
                              WHERE i.TAG = 5 AND i.ANBARF IS NOT NULL AND i.ANBAR <> CAST(i.ANBARF AS INT)")
                            .Where(e => e.CODE != null && e.Src.HasValue && e.Dst.HasValue)
                            .GroupBy(e => e.CODE!.Trim())
                            .ToDictionary(g => g.Key, g => g.Select(e => (Src: e.Src!.Value, Dst: e.Dst!.Value)).ToList());
                        LogWriter.WriteLog($"transferEdgesByCode.Count = {transferEdgesByCode.Count}");

                        var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(groupedByCode.Count);
                        CL_HESABDARI_AUTO_BAZ.ExecuteWithPreferredLoop(0, groupedByCode.Count, dbParallelOptions, groupIndex =>
                        {
                            var codeGroup = groupedByCode[groupIndex];
                            currentCode = codeGroup.Key;

                            var codeKey = (codeGroup.Key ?? string.Empty).Trim();
                            var anbarRows = codeGroup.ToList();
                            transferEdgesByCode.TryGetValue(codeKey, out var codeEdges);
                            var (orderedAnbars, hasCycle) = OrderAnbarsForTransferDependencies(anbarRows, codeEdges);

                            // ردیف‌هایی که case 5 (انتقالیِ خروج) در همین اجرا MABL_K شان را نوشته
                            // است؛ case 6 باید مقدار زنده را بخواند نه عکسِ لحظه‌ی fetch.
                            // محدود به همین کالاست، چون انبارهای یک کالا روی یک Thread می‌مانند.
                            var touchedByCase5 = new HashSet<long>();

                            if (hasCycle)
                            {
                                // ─────────────────────────────────────────────────────────────
                                // چرخه‌ی وابستگی (هم از A به B و هم از B به A انتقال داده شده):
                                // هیچ ترتیبِ خطیِ ثابتی هر دو طرف را درست نمی‌کند. به‌جای حدس زدن،
                                // کاردکسِ همه‌ی انبارهای این کالا یک‌جا خوانده و در یک جریان زمانیِ
                                // واحد پردازش می‌شود؛ هر انبار مانده‌ی متحرکِ مستقل خودش را دارد.
                                // چون رویدادها به ترتیب زمانیِ واقعی می‌آیند، case 5 هر حواله همیشه
                                // قبل از case 6 متناظرش پردازش می‌شود.
                                // ─────────────────────────────────────────────────────────────
                                LogWriter.WriteLog($"کالا {codeKey}: چرخه‌ی وابستگی حواله انتقالی — پردازش ادغام‌شده");

                                var states = new Dictionary<int, AvgAnbarState>();
                                foreach (var r in anbarRows)
                                {
                                    if (r.ANBAR.HasValue) { states[r.ANBAR.Value] = BuildAvgAnbarState(r); }
                                }

                                var mergedKardex = dbms.DoGetDataSQL<cm_model>(
                                    BuildAvgRebuildSourceSql(codeGroup.Key, null, this.DT, hasFbk, hasKbk, useBackHeadSaleReturn, useBackHeadPurchaseReturn)).ToList();

                                var pendingMerged = new List<string>(mergedKardex.Count);
                                for (int eof = 0; eof < mergedKardex.Count; eof++)
                                {
                                    if (IsCancelRequestedBgWorker) { return; }
                                    Interlocked.Increment(ref processedRows);
                                    uiProgress.ReportOne();

                                    var anbarKey = (int)(mergedKardex[eof].ANBAR ?? 0);
                                    if (!states.TryGetValue(anbarKey, out var stMerged)) { continue; }

                                    ProcessAvgKardexRow(codeGroup.Key, mergedKardex[eof], stMerged, pendingMerged, rst3ById, RST6, touchedByCase5);
                                }

                                FlushAvgPending(pendingMerged);
                                return;
                            }

                            // انبارهای این کالا سریال و به ترتیبِ وابستگیِ حواله‌ی انتقالی پردازش
                            // می‌شوند: مبدأ همیشه قبل از مقصد، و آپدیت هر انبار پیش از شروع انبار
                            // بعدی روی دیتابیس نشسته است. (کالاهای مختلف همچنان کاملاً موازی‌اند.)
                            foreach (var rRow in orderedAnbars)
                            {
                                var st = BuildAvgAnbarState(rRow);

                                // یک رفت‌وبرگشت به‌جای DROP VIEW + CREATE VIEW + SELECT + DROP VIEW.
                                var RST2 = dbms.DoGetDataSQL<cm_model>(
                                    BuildAvgRebuildSourceSql(rRow.CODE, rRow.ANBAR ?? 0, this.DT, hasFbk, hasKbk, useBackHeadSaleReturn, useBackHeadPurchaseReturn)).ToList();

                                // UPDATE ها انباشته می‌شوند تا به‌جای یک رفت‌وبرگشت برای هر تراکنش،
                                // دسته‌ای اجرا شوند. ترتیب اجرا دقیقاً ترتیب تولید است.
                                var pending = new List<string>(RST2.Count);

                                for (int eof = 0; eof < RST2.Count; eof++)
                                {
                                    if (IsCancelRequestedBgWorker) { return; }
                                    Interlocked.Increment(ref processedRows);
                                    uiProgress.ReportOne();

                                    ProcessAvgKardexRow(rRow.CODE, RST2[eof], st, pending, rst3ById, RST6, touchedByCase5);
                                }

                                FlushAvgPending(pending);
                            }
                        });
                    }
                    else
                    {
                        for (int EOF = 0; EOF < RST4.Count; EOF++)
                        {
                            if (IsCancelRequestedBgWorker) { return; }

                            var rst = dbms.DoGetDataSQL<_QRE_3>("SELECT     DISTINCT  dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A FROM         dbo.STUF_DEF INNER JOIN                dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE INNER JOIN              dbo.INVO_LST ON dbo.STUF_FSK.CODE = dbo.INVO_LST.CODE AND dbo.STUF_FSK.ANBAR = dbo.INVO_LST.ANBAR WHERE (dbo.STUF_DEF.RADAH =  " + RST4[EOF].CODE + "  ) ORDER BY dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR").ToList();

                            // مثل شاخه‌ی بالا: گروه‌بندی بر اساس کالا تا انبارهای یک کالا سریال بمانند.
                            var groupedByCode = rst.GroupBy(x => x.CODE).ToList();
                            var groupCode = RST4[EOF].CODE.ToString();
                            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(groupedByCode.Count);

                            CL_HESABDARI_AUTO_BAZ.ExecuteWithPreferredLoop(0, groupedByCode.Count, dbParallelOptions, groupIndex =>
                            {
                                var codeGroup = groupedByCode[groupIndex];
                                currentCode = codeGroup.Key;

                                foreach (var rRow in codeGroup)
                                {
                                    double MIAN;
                                    double MBKM;
                                    var MOGUDI = default(double);
                                    var pending = new List<string>();

                                    if (groupCode == "2" || groupCode == "3")
                                    {
                                        MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE(rRow.CODE);
                                        if (MIAN == 0d)
                                        {
                                            MIAN = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(rRow.CODE);
                                        }
                                        if (MIAN == 0d)
                                        {
                                            pending.Add("UPDATE    dbo.INVO_LST SET  AVRAGE = 0, AVRAGE2 = 0 WHERE     (CODE = N'" + rRow.CODE + "') AND (dbo.INVO_LST.ANBAR = " + rRow.ANBAR + ")");
                                            pending.Add("update dbo.INVO_LST SET MABL = 0, MABL_K = 0 WHERE    ((TAG = 5) OR (TAG = 6) OR (TAG = 7) OR (TAG = 8) OR (TAG = 9) OR (TAG = 10) OR (TAG = 11) OR (TAG = 16) OR (TAG = 17) OR (TAG = 18)) AND ((CODE = N'" + rRow.CODE + "') AND (dbo.INVO_LST.ANBAR = " + rRow.ANBAR + "))");
                                        }
                                        else
                                        {
                                            // یک رفت‌وبرگشت به‌جای DROP VIEW + CREATE VIEW + SELECT (و بدون شیء مشترک بین کاربران).
                                            var RST2 = dbms.DoGetDataSQL<cm_model>(BuildAvgRebuildSourceSqlAllTags(rRow.CODE, rRow.ANBAR ?? 0, this.DT)).ToList();
                                            for (int f = 0; f < RST2.Count; f++)
                                            {
                                                if (IsCancelRequestedBgWorker) { return; }
                                                Interlocked.Increment(ref processedRows);
                                                uiProgress.ReportOne();

                                                MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE_KOL(rRow.CODE, RST2[f].DATE_N ?? 0L);

                                                INVO_LST? rst3Filter = null;
                                                if (RST2[f].ID.HasValue && rst3ById.TryGetValue(RST2[f].ID.Value, out var invoRow))
                                                {
                                                    rst3Filter = invoRow;
                                                }

                                                switch (RST2[f].TAG)
                                                {
                                                    case 1:  // خريد
                                                    case 22: // برگشت فروش سال قبل
                                                    case 24: // برگشت فروش سال قبل
                                                    case 2:  // فروش
                                                        {
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {N(MIAN)} , AVRAGE2 = {N(MIAN)} WHERE ID = {rst3Filter.id}");
                                                            break;
                                                        }
                                                    case 5: // انتقالي خروج
                                                        {
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round(MIAN * (RST2[f].MEGHk ?? 0));
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET AVRAGE = {N(MIAN)} ,
                                                                                    AVRAGE2 = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)},
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            break;
                                                        }
                                                    case 6: // انتقالي ورود
                                                        {
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round(MIAN * (RST2[f].MEGHk ?? 0));
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE2 = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)},
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            break;
                                                        }
                                                    case 10: // مواد خروج
                                                    case 11: // موادساير خروج
                                                    case 26: // برگشت خريد
                                                        {
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round(MIAN * (RST2[f].MEGHk ?? 0));
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)},
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            break;
                                                        }
                                                    case 9:    // توليد
                                                        {
                                                            if (RST2[f].N_KOL != 0 & !IsNull(RST2[f].N_KOL) & Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
                                                            {
                                                                var RST7 = dbms.DoGetDataSQL<THE_QUERY3>("SELECT  dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + RST2[f].N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").ToList();
                                                                if (RST7.Count > 0)
                                                                {
                                                                    MIAN = (RST7.FirstOrDefault().IMBIBE_MANF ?? 0) + (RST7.FirstOrDefault().IMBIBE_SAR ?? 0) + (RST7.FirstOrDefault().MABLKs ?? 0);
                                                                }
                                                            }
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round(MIAN * (RST2[f].MEGHk ?? 0));
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)},
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            break;
                                                        }
                                                    case 17: // كسري انبار
                                                        {
                                                            MOGUDI = MOGUDI + (RST2[f].MEGHk ?? 0);
                                                            // ⚠️ شرط CODE جا افتاده بود؛ بدون آن نرخ این کالا روی همه‌ی
                                                            //    کالاهای همان برگه‌ی انبارگردانی نوشته می‌شد.
                                                            pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {N(MIAN)} WHERE CODE = '{RST2[f].CODE}' AND GRD_NUM = {N(RST2[f].NUMBER)}");
                                                            break;
                                                        }
                                                    case 18: // اضافه انبار
                                                        {
                                                            MOGUDI = MOGUDI - (RST2[f].MEGHk ?? 0);
                                                            pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {N(MIAN)} WHERE CODE = '{RST2[f].CODE}' AND GRD_NUM = {N(RST2[f].NUMBER)}");
                                                            break;
                                                        }
                                                }
                                            }

                                            bool isFormolChecked = false;
                                            Dispatcher.Invoke(new Action(() => { isFormolChecked = FORMOL.IsChecked is true; }));
                                            if (isFormolChecked)
                                            {
                                                pending.Add("UPDATE    dbo.DTL_MANF  SET  MABLK = (MEGHk + PERT) * " + N(MIAN) + ", SMABL = " + N(MIAN) + " WHERE     (CODE = N'" + rRow.CODE + "')");
                                            }
                                        }
                                    }
                                    else // ساير گروه‌ها
                                    {
                                        MBKM = rRow.MABL_A ?? 0;
                                        MIAN = rRow.FI_A ?? 0;
                                        MOGUDI = rRow.MOGODI_A ?? 0;

                                        var RST2 = dbms.DoGetDataSQL<cm_model>(BuildAvgRebuildSourceSqlAllTags(rRow.CODE, rRow.ANBAR ?? 0, this.DT)).ToList();
                                        for (int e = 0; e < RST2.Count; e++)
                                        {
                                            if (IsCancelRequestedBgWorker) { return; }
                                            Interlocked.Increment(ref processedRows);
                                            uiProgress.ReportOne();

                                            INVO_LST? rst3Filter = null;
                                            if (RST2[e].ID.HasValue && rst3ById.TryGetValue(RST2[e].ID.Value, out var invoRow))
                                            {
                                                rst3Filter = invoRow;
                                            }

                                            switch (RST2[e].TAG)
                                            {
                                                case 1: // خريد
                                                    {
                                                        MBKM = MBKM + (RST2[e].MABL_K ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGHk ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        rst3Filter.AVRAGE = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 22: // برگشت فروش سال قبل
                                                    {
                                                        MBKM = MBKM + MIAN * (RST2[e].MEGH_MAR ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGH_MAR ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        rst3Filter.AVRAGE = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 24: // برگشت فروش سال قبل
                                                    {
                                                        MBKM = MBKM + (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI + (RST2[e].MEGHk ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        rst3Filter.AVRAGE = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 2: // فروش
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGHk ?? 0);
                                                        rst3Filter.AVRAGE = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 3: // برگشت خريد
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGH_MAR ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGH_MAR ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        if (rst3Filter != null)
                                                        {
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                        AVRAGE2 = {N(MIAN)}
                                                                                        WHERE ID = {rst3Filter.id}");
                                                        }
                                                        break;
                                                    }
                                                case 4: // برگشت فروش
                                                    {
                                                        MBKM = MBKM + (RST2[e].MEGH_MAR ?? 0) * (rst3Filter?.AVRAGE ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGH_MAR ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        if (rst3Filter != null)
                                                        {
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                        AVRAGE2 = {N(MIAN)}
                                                                                        WHERE ID = {rst3Filter.id}");
                                                        }
                                                        break;
                                                    }
                                                case 5: // انتقالي خروج
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGHk ?? 0);
                                                        rst3Filter.AVRAGE = MIAN;
                                                        rst3Filter.MABL = MIAN;
                                                        rst3Filter.MABL_K = Math.Round(MIAN * (RST2[e].MEGHk ?? 0));
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)} ,
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 6: // انتقالي ورود
                                                    {
                                                        MBKM = MBKM + (RST2[e].MABL_K ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGHk ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        rst3Filter.AVRAGE2 = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE2 = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 10: // مواد خروج
                                                case 11: // موادساير خروج
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGHk ?? 0);
                                                        rst3Filter.AVRAGE = MIAN;
                                                        rst3Filter.MABL = MIAN;
                                                        rst3Filter.MABL_K = Math.Round(MIAN * (RST2[e].MEGHk ?? 0));
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)} ,
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 9:    // توليد
                                                    {
                                                        MBKM = MBKM + (RST2[e].MABL_K ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGHk ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        else { MIAN = MBKM / MOGUDI; }
                                                        rst3Filter.AVRAGE = MIAN;
                                                        rst3Filter.MABL = MIAN;
                                                        rst3Filter.MABL_K = Math.Round(MIAN * (RST2[e].MEGHk ?? 0));
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)} ,
                                                                                    MABL = {N(MIAN)} ,
                                                                                    MABL_K = {N(rst3Filter.MABL_K)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                                case 17: // كسري انبار
                                                    {
                                                        // ⚠️ عیناً مثل کد اصلی: در این شاخه MIAN هرگز به MBKM/MOGUDI
                                                        //    بازمحاسبه نمی‌شود (آن خط در کد اصلی کامنت است).
                                                        MBKM = MBKM + MIAN * (RST2[e].MEGHk ?? 0);
                                                        MOGUDI = MOGUDI + (RST2[e].MEGHk ?? 0);
                                                        if (MBKM == 0d) { MIAN = 0d; }
                                                        else if (MOGUDI == 0d) { MIAN = 0d; MBKM = 0d; }
                                                        if (MIAN < 0d) { MIAN = 0d; }
                                                        var _RST6Filter_ = RST6.Where(x => x.CODE == RST2[e].CODE && x.GRD_NUM == RST2[e].NUMBER).FirstOrDefault();
                                                        if (_RST6Filter_ != null) { _RST6Filter_.MABL = MIAN; }
                                                        pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {N(MIAN)} WHERE CODE = '{RST2[e].CODE}' AND GRD_NUM = {N(RST2[e].NUMBER)}");
                                                        break;
                                                    }
                                                case 18: // اضافه انبار
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGHk ?? 0);
                                                        var _RST6Filter_ = RST6.Where(x => x.CODE == RST2[e].CODE && x.GRD_NUM == RST2[e].NUMBER).FirstOrDefault();
                                                        if (_RST6Filter_ != null) { _RST6Filter_.MABL = MIAN; }
                                                        pending.Add($@"UPDATE dbo.ANBGRD_LST SET MABL = {N(MIAN)} WHERE CODE = '{RST2[e].CODE}' AND GRD_NUM = {N(RST2[e].NUMBER)}");
                                                        break;
                                                    }
                                                case 26: // برگشت خريد
                                                    {
                                                        MBKM = MBKM - (RST2[e].MEGHk ?? 0) * MIAN;
                                                        MOGUDI = MOGUDI - (RST2[e].MEGHk ?? 0);
                                                        rst3Filter.AVRAGE = MIAN;
                                                        pending.Add($@"UPDATE dbo.INVO_LST SET
                                                                                    AVRAGE = {N(MIAN)}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                        break;
                                                    }
                                            }
                                        }
                                    }

                                    // اجرای دسته‌ای آپدیت‌های این (کالا، انبار) پیش از رفتن به انبار بعدی
                                    const int updateChunkSize = 200;
                                    for (int off = 0; off < pending.Count; off += updateChunkSize)
                                    {
                                        var batch = new StringBuilder();
                                        var endAt = Math.Min(off + updateChunkSize, pending.Count);
                                        for (int k = off; k < endAt; k++) { batch.Append(pending[k]).AppendLine(";"); }
                                        dbms.DoExecuteSQL(batch.ToString());
                                    }
                                }
                            });
                        }
                    }

                    uiProgress.Complete();
                    //Generaly.DoResetCountersDisplay();
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در بازسازی نرخ میانگین" });

                    ExpectionLogWriter.WriteLog(er, "باز سازی نرخ میانگین خطا");

                    string method_source = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    LogWriter.WriteLog("باز سازی نرخ میانگین خطا : " +
                       $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n method_source : {method_source}" +
                        $"\n Method Name: {er.TargetSite.Name} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                        $"\n Help Link: {er.HelpLink} \n  ExceptionType: {er.GetType().FullName} \n" +
                        $"{CL_CCNNMANAGER.CONNECTION_STR}");

                }
                finally
                {
                    LogWriter.WriteLog("باز سازی نرخ میانگین پایان");
                }

            });
            Dispatcher.Invoke(new Action(() => { C0.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C1_TASK()
        {
            await Task.Run(() =>
            {
                long DT = 0;
                long dt2;
                Int64 HF1 = 1;
                Int64 HF2 = 9999999999;
                Dispatcher.Invoke(new Action(() =>
                {
                    DT = System.Convert.ToInt64(CL_HESABDARI_AUTO_BAZ.PersianDateLong(DateAndTime.DateAdd("d", System.Convert.ToDouble(Convert.ToInt32(daysb.Text) * -1), DateTime.Now)));
                }));
                var rst_0 = dbms.DoGetDataSQL<Int64?>("SELECT NUMBER FROM HEAD_LST WHERE TAG = 2 AND DATE_N >" + System.Convert.ToString(DT) + " ORDER BY NUMBER").ToList();
                if (rst_0.Count > 0)
                {
                    //HF1 = rst.FirstOrDefault().NUMBER;
                    HF1 = (long)rst_0.FirstOrDefault();
                }
                else
                {
                    HF1 = 1;
                }

                if (IsCancelRequestedBgWorker) { return; }

                try
                {
                    CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند فروش" });

                    ExpectionLogWriter.WriteLog(er, "سند فروش خطا");
                }

            });
            Dispatcher.Invoke(new Action(() => { c1.Foreground = Generaly.PutThisColor(); }));
            //Generaly.DoResetCountersDisplay();
        }
        public async Task C2_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    LogWriter.WriteLog("سند خرید شروع");
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        DT = System.Convert.ToInt64(CL_HESABDARI_AUTO_BAZ.PersianDateLong(DateAndTime.DateAdd("d", System.Convert.ToDouble(Convert.ToInt32(daysb.Text) * -1), DateTime.Now)));
                    }));
                    var rst = dbms.DoGetDataSQL<Int64?>("SELECT NUMBER FROM HEAD_LST WHERE TAG = 1 AND DATE_N >" + System.Convert.ToString(DT) + " ORDER BY NUMBER ").ToList();
                    if (rst.Count > 0)
                    {
                        //HF1 = rst.FirstOrDefault().NUMBER;
                        HF1 = (long)rst.FirstOrDefault();
                    }
                    else
                    {
                        HF1 = 1;
                    }

                    if (IsCancelRequestedBgWorker) { return; }

                    CL_HESABDARI_AUTO_BAZ.GENSANADKHAREED(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند خرید" });

                    ExpectionLogWriter.WriteLog(er, "سند خرید خطا");
                }

            });
            Dispatcher.Invoke(new Action(() => { c2.Foreground = Generaly.PutThisColor(); }));
            //DoResetCountersDisplay();
        }
        public async Task C3_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }

                    CL_HESABDARI_AUTO_BAZ.GENSANADKHAZ(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند خزانه" });

                    ExpectionLogWriter.WriteLog(er, "سند خزانه خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c3.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C4_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }
                    CL_HESABDARI_AUTO_BAZ.SANADENTEGHAL(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند انتقالی از انبار به انبار" });

                    ExpectionLogWriter.WriteLog(er, "سند انتقال خطا");
                }

            });
            Dispatcher.Invoke(new Action(() => { c4.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C5_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }
                    CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند خروج مواد" });

                    ExpectionLogWriter.WriteLog(er, "سند خروج مواد خطا");
                }


            });
            Dispatcher.Invoke(new Action(() => { c5.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C6_TASK()
        {
            await Task.Run(() =>
            {
                long DT = 0;
                long dt2;
                Int64 HF1 = 1;
                Int64 HF2 = 9999999999;

                if (IsCancelRequestedBgWorker) { return; }

                try
                {
                    CL_HESABDARI_AUTO_BAZ.SANADKHORUGSAYER(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند خروج سایر" });

                    ExpectionLogWriter.WriteLog(er, "سند خروج سایر خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c6.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C7_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }
                    CL_HESABDARI_AUTO_BAZ.SANADVORUDSAKHT(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند ورود ساخته شده" });

                    ExpectionLogWriter.WriteLog(er, "سند ورود ساخته شده خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c7.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C8_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }
                    CL_HESABDARI_AUTO_BAZ.gensanadbargashfroosh(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند برگشت فروش" });

                    ExpectionLogWriter.WriteLog(er, "سند برگشت فروش خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c8.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C9_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (Generaly.C9 is true)
                    {
                        if (IsCancelRequestedBgWorker) { return; }
                        CL_HESABDARI_AUTO_BAZ.SANADKHAD(HF1, HF2);
                    }
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند خدمات" });

                    ExpectionLogWriter.WriteLog(er, "سند خدمات خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c9.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C10_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    long DT = 0;
                    long dt2;
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;

                    if (IsCancelRequestedBgWorker) { return; }
                    CL_HESABDARI_AUTO_BAZ.GENSANADANBARGARD(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند انبار گردانی" });

                    ExpectionLogWriter.WriteLog(er, "سند انبار گردانی خطا");
                }
            });
            Dispatcher.Invoke(new Action(() => { c10.Foreground = Generaly.PutThisColor(); }));
        }
        public async Task C11_TASK()
        {
            await Task.Run(() =>
            {
                try
                {
                    Int64 HF1 = 1;
                    Int64 HF2 = 9999999999;
                    CL_HESABDARI_AUTO_BAZ.GENSANADVD(HF1, HF2);
                }
                catch (Exception er)
                {
                    AnyErrorHappend = true;
                    ERTRACKLIST.Add(new ErrorSectionModel { ErrorHappend = true, SectionName = "خطا در سند وصولی چکهای دریافتی" });

                    ExpectionLogWriter.WriteLog(er, "سند وصولی خطا");
                }

            });
            Dispatcher.Invoke(new Action(() => { c11.Foreground = Generaly.PutThisColor(); }));
        }

        private void C0_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        sbyte CC = -1;
        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CC++;
            if (CC % 2 is 0)
            {
                SetCheckboxesChecked(true);
                CC = 0;
            }
            else
            {
                SetCheckboxesChecked(false);
            }

            SaveCheckBoxesState();
        }
        private void C00_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c1_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c2_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c3_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c4_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c5_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c6_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c7_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c8_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c9_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c10_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void c11_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void defacc_Click(object sender, RoutedEventArgs e)
        {
            SaveCheckBoxesState();
        }
        private void chkUseSmartThrottling_Click(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI_AUTO_BAZ.UseSmartThrottlingByDefault = chkUseSmartThrottling.IsChecked ?? false;
            SaveCheckBoxesState();
        }


        private void HRSTK1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LST_DATA5.CollectionChanged -= LST_DATA5_CollectionChanged;
            LST_DATA5?.Clear();
            LST_DATA5.CollectionChanged += LST_DATA5_CollectionChanged;
            Properties.Settings.Default.TheHistoryLST = null;
            Properties.Settings.Default.Save();
        }

        private void repeatb_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(repeatb.Text.Trim()) || !Information.IsNumeric(repeatb.Text.Trim()))
            {
                repeatb.Text = "1";
            }

            Properties.Settings.Default.RepeatCount = repeatb.Text;
            Properties.Settings.Default.Save();
        }

    }
}
