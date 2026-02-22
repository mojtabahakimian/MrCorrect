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
                TOGHER_PROGRESS.Value = 100;
                COUNTER_TXBL.Content = $"100%";

                Generaly.DoResetCountersDisplay();
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

                COUNTER_TXBL.Content = $"0.00%";
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
                if (checkbox.Name == "FORMOL" || checkbox.Name == "defacc" || checkbox.Name == "C00" || checkbox.Name == "chkUseSmartThrottling")
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
            for (int r = 0; r < repeatCount; r++)
            {
                tasks = new List<Task>();
                AnyErrorHappend = false;

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

                if (Generaly.C0) { await Task.Run(async () => { await C0_TASK(); }); } //باز سازی نرخ میانگین
                if (Generaly.C00) { await Task.Run(async () => { await C00_TASK(); }); } //باز سازی موجودی انبار


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

                    Dispatcher.Invoke(new Action(() =>
                    {
                        IsWorkisDone = true;
                        if (AnyErrorHappend)
                        {
                            foreach (var item in ERTRACKLIST)
                            {
                                LST_DATA5.Add(item.SectionName);
                            }
                            ERTRACKLIST?.Clear();
                            LogWriter.WriteLog($@"ERTRACKLIST : {ERTRACKLIST.Count} => {ERTRACKLIST.FirstOrDefault()}");
                            LST_DATA5.Add("پایان یافته با خطا :" + Conversions.ToString(DateTime.Now));
                        }
                        else //Successfull
                        {
                            LST_DATA5.Add("پايان :" + Conversions.ToString(DateTime.Now));
                        }

                    }));
                    SayOprationsFinished();
                }
                catch (OperationCanceledException ecx)
                {
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
                    StillMethodIsWorking = false;

                    LST_DATA5.Add("به خاطر خطا لغو شد. :" + Conversions.ToString(DateTime.Now));
                    Btn_DoCancel.Content = "لغو";

                    var taskInfo = string.Join(", ", tasks.Select(t => $"Id:{t.Id},Status:{t.Status}"));
                    var errorSections = string.Join(" | ", ERTRACKLIST.Select(x => x.SectionName));
                    LogWriter.WriteLog($"Exception in LetsGoBtn_Click (iteration {r + 1} of {repeatCount}). AnyErrorHappend={AnyErrorHappend}. Tasks: {taskInfo}. ErrorSections: {errorSections}");
                    ExpectionLogWriter.WriteLog(ex, "Exception in LetsGoBtn_Click");

                    Console.WriteLine(ex.ToString());
                }
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
                        rst_STUF_STK[h].MOGODI = Math.Round(rst_STUF_STK[h].MOGODI * Math.Pow(10, (double)Baseknow.DIG)) / Math.Pow(10, (double)Baseknow.DIG);

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
                        UpdateOverallProgressBar();                                   // auto_run.LBL_C2.Content = $"{progress:F2}%";
                    }));

                    //باز سازی نرخ میانگین

                    if (IsCancelRequestedBgWorker) { return; }
                    //Forms["auto_run"].List5.AddItem("شروع :" + Conversions.ToString(DateTime.Now));


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
                    var rst3 = dbms.DoGetDataSQL<INVO_LST>("SELECT * FROM INVO_LST").ToList(); LogWriter.WriteLog($"rst3.Count = {rst3.Count}");
                    var RST6 = dbms.DoGetDataSQL<ANBGRD_LST>("SELECT * FROM ANBGRD_LST").ToList(); LogWriter.WriteLog($"RST6.Count = {RST6.Count}");
                    if (Strings.Mid(Baseknow.OPTIONSS, 66, 1) == "5")
                    {
                        var rst = dbms.DoGetDataSQL<THE_QUERY1>("SELECT     dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A FROM         dbo.STUF_DEF INNER JOIN                      dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE GROUP BY dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A").ToList(); LogWriter.WriteLog($" After    if (Strings.Mid(Baseknow.OPTIONSS, 66, 1) == \"5\") rst.Count = {rst.Count}");
                        //CL_HESABDARI_AUTO_BAZ.ExecuteWithPreferredLoop(0, rst.Count, r =>
                        var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(rst.Count);
                        CL_HESABDARI_AUTO_BAZ.ExecuteWithPreferredLoop(0, rst.Count, dbParallelOptions, r => //while (!rst.EOF)
                        {
                            List<cm_model> RST2 = null;
                            int errorno;
                            string SQL;
                            double MIAN;
                            double MBKM;
                            var MOGUDI = default(double);
                            double temp;

                            //this.Repaint();

                            MBKM = (double)rst[r].MABL_A;
                            MIAN = (double)rst[r].FI_A;
                            if (MIAN == 0d)
                            {
                                MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE(rst[r].CODE);
                                if (MIAN == 0d)
                                {
                                    MIAN = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(rst[r].CODE);
                                }
                            }
                            string TBLTMP = "T" + rst[r].ANBAR + "MPAV101" + rst[r].CODE;
                            MOGUDI = (double)rst[r].MOGODI_A;
                            dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS  WHERE TABLE_NAME = '{TBLTMP}')   DROP VIEW {TBLTMP}");
                            dbms.DoExecuteSQL($"CREATE VIEW  {TBLTMP} as " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,  dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO,dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE WHERE  (dbo.INVO_LST.tag <> 20 and dbo.INVO_LST.tag <> 23) and (dbo.INVO_LST.CODE = '" + rst[r].CODE + "') AND " + " (dbo.INVO_LST.ANBAR = " + rst[r].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH , dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1  = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[r].CODE + "') AND (dbo.INVO_LST.ANBARF = " + rst[r].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ")  AND (dbo.INVO_LST.TAG = 5) ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN       dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag INNER JOIN  dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[r].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[r].ANBAR + ") AND (dbo.HEAD_LST_FBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_FBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A,dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH  FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag INNER JOIN " + " dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE WHERE     (dbo.INVO_LST.CODE = '" + rst[r].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[r].ANBAR + ") AND (dbo.HEAD_LST_KBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_KBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER  union " + " SELECT     TOP 100 PERCENT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK, 0 AS megh_mar,' ' AS mol, dbo.ANBGRD_LST.MABL, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK, 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no, 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage, 0 AS id, '17' AS Expr1 FROM   dbo.ANBGRD_LST INNER JOIN  dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM INNER JOIN  dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE " + " WHERE      (dbo.ANBGRD_LST.CODE = '" + rst[r].CODE + "') AND (dbo.ANBGRD_HEAD.GRD_ANBAR = " + rst[r].ANBAR + ") and ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * - 1 <> 0) AND (dbo.ANBGRD_HEAD.GRD_DATE > " + this.DT + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL)) ORDER BY dbo.ANBGRD_HEAD.GRD_DATE, dbo.ANBGRD_LST.GRD_NUM ");

                            RST2 = dbms.DoGetDataSQL<cm_model>($"SELECT     TOP 100 PERCENT DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID,  MEGH_R , RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, ID, BARGAH FROM dbo.{TBLTMP} ORDER BY DATE_N, BARGAH").ToList();

                            for (int eof = 0; eof < RST2.Count; eof++) //while (!RST2.EOF)
                            {
                                if (IsCancelRequestedBgWorker) { return; }
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    this.Text23.Text = Convert.ToString(Convert.ToInt64(Text23.Text) - 1);
                                    Text19.Text = Convert.ToString(Convert.ToInt64(Text19.Text) + 1);
                                    this.co.Text = rst[r].CODE;
                                    progress++;
                                    PRGR_C0.Value = progress / rcount * 100.0;// Update the progress bar
                                    UpdateOverallProgressBar();
                                }));
                                var rst3Filter_ = rst3.Where(x => x.id == RST2[eof].ID).ToList();

                                switch (RST2[eof].TAG)
                                {
                                    case 1: // خريد
                                        {
                                            MBKM = (double)(MBKM + RST2[eof].MABL_K);
                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[eof].MEGHk), 6);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}"); //rst3Filter_.update();
                                            break;
                                        }
                                    case 22: // برگشت فروش سال قبل
                                        {
                                            if (MBKM <= 0d)
                                            {
                                                MBKM = (double)(RST2[eof].MABL * RST2[eof].MEGH_MAR);
                                            }
                                            else
                                            {
                                                MBKM = (double)(MBKM + MIAN * RST2[eof].MEGH_MAR);
                                            }
                                            MOGUDI = (double)(MOGUDI + RST2[eof].MEGH_MAR);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}"); //rst3Filter_.update();
                                            break;
                                        }
                                    case 24: // برگشت فروش سال قبل
                                        {
                                            if (MBKM <= 0d)
                                            {
                                                MBKM = (double)RST2[eof].MABL_K;
                                            }
                                            else
                                            {
                                                MBKM = (double)(MBKM + RST2[eof].MEGHk * MIAN);
                                            }
                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[eof].MEGHk), 6);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}"); //rst3Filter_.update();
                                            break;
                                        }
                                    case 2: // فروش
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 3: // برگشت خريد
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGH_MAR * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGH_MAR);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE2 = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE2 = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 4: // برگشت فروش
                                        {
                                            MBKM = (double)(MBKM + RST2[eof].MEGH_MAR * rst3Filter_.FirstOrDefault().AVRAGE);
                                            MOGUDI = (double)(MOGUDI + RST2[eof].MEGH_MAR);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE2 = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE2 = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 5: // انتقالي خروج
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL_K = Math.Round((double)(MIAN * RST2[eof].MEGHk));

                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , 
                                                                                     MABL = {MIAN} ,
                                                                                     MABL_K = {rst3Filter_.FirstOrDefault().MABL_K}
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 6: // انتقالي ورود
                                        {
                                            MBKM = (double)(MBKM + RST2[eof].MABL_K);
                                            MOGUDI = Math.Round(Math.Round((double)(MOGUDI + RST2[eof].MEGHk), 6), 6);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE2 = MIAN;
                                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET AVRAGE2 = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 10: // مواد خروج
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL_K = Math.Round((double)(MIAN * RST2[eof].MEGHk));

                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , 
                                                                                     MABL = {MIAN} ,
                                                                                     MABL_K = {rst3Filter_.FirstOrDefault().MABL_K}
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 11:    // موادساير خروج
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL = MIAN;
                                            rst3Filter_.FirstOrDefault().MABL_K = Math.Round((double)(MIAN * RST2[eof].MEGHk));

                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , 
                                                                                     MABL = {MIAN} ,
                                                                                     MABL_K = {rst3Filter_.FirstOrDefault().MABL_K}
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                    case 9:    // توليد
                                        {
                                            if (RST2[eof].N_KOL != 0 & !IsNull(RST2[eof].N_KOL) & Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
                                            {
                                                List<THE_QUERY2> RST7 = dbms.DoGetDataSQL<THE_QUERY2>("SELECT  dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + RST2[eof].N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").ToList();
                                                if (RST7.Count > 0)
                                                {
                                                    //rst3Filter_.FirstOrDefault().MABL = RST7.Fields(0) + RST7.Fields(1) + RST7.Fields(2);
                                                    rst3Filter_.FirstOrDefault().MABL = (double)(RST7.FirstOrDefault().IMBIBE_MANF + RST7.FirstOrDefault().IMBIBE_SAR + RST7.FirstOrDefault().MABLKs);
                                                    rst3Filter_.FirstOrDefault().MABL_K = Math.Round((double)((RST7.FirstOrDefault().IMBIBE_MANF + RST7.FirstOrDefault().IMBIBE_SAR + RST7.FirstOrDefault().MABLKs) * RST2[eof].MEGHk));
                                                    dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                                     MABL = {rst3Filter_.FirstOrDefault().MABL} ,
                                                                                     MABL_K = {rst3Filter_.FirstOrDefault().MABL_K}
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                                    //rst3Filter_.update();
                                                }
                                            }
                                            else
                                            {
                                                rst3Filter_.FirstOrDefault().MABL = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE_KOL(rst[r].CODE, (long)RST2[eof].DATE_N);
                                                if (rst3Filter_.FirstOrDefault().MABL == 0)
                                                {
                                                    rst3Filter_.FirstOrDefault().MABL = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(rst[r].CODE);
                                                }
                                                rst3Filter_.FirstOrDefault().MABL_K = Math.Round((double)(rst3Filter_.FirstOrDefault().MABL * RST2[eof].MEGHk));

                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                                     MABL = {rst3Filter_.FirstOrDefault().MABL} ,
                                                                                     MABL_K = {rst3Filter_.FirstOrDefault().MABL_K}
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                                //rst3Filter_.update();
                                            }
                                            MBKM = MBKM + rst3Filter_.FirstOrDefault().MABL_K;
                                            MOGUDI = Math.Round(Math.Round((double)(MOGUDI + RST2[eof].MEGHk), 6), 6);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;
                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET
                                                                                     AVRAGE = {MIAN} 
                                                            WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }

                                    case 17: // كسري انبار
                                        {
                                            MBKM = (double)(MBKM + MIAN * RST2[eof].MEGHk);
                                            MOGUDI = Math.Round(Math.Round((double)(MOGUDI + RST2[eof].MEGHk), 6), 6);
                                            if (MBKM == 0d)
                                            {
                                            }
                                            // MIAN = 0
                                            else if (MOGUDI == 0d)
                                            {
                                                // MIAN = 0
                                                MBKM = 0d;
                                            }
                                            else
                                            {
                                                MIAN = MBKM / MOGUDI;
                                            }
                                            // If MIAN < 0 Then
                                            // MIAN = 0
                                            // End If
                                            var RST6Filter = RST6.Where(x => x.CODE == RST2[eof].CODE && x.GRD_NUM == RST2[eof].NUMBER).FirstOrDefault();
                                            //RST6.Filter = "CODE = '" + RST2[eof].CODE + "' AND GRD_NUM = " + RST2[eof].NUMBER;
                                            //RST6.Fields("MABL") = MIAN;
                                            RST6.FirstOrDefault().MABL = MIAN;
                                            dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE CODE = '{RST2[eof].CODE}' AND GRD_NUM = {RST2[eof].NUMBER}");
                                            //RST6.update();
                                            break;
                                        }
                                    case 18: // فروش
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            var RST6Filter = RST6.Where(x => x.CODE == RST2[eof].CODE && x.GRD_NUM == RST2[eof].NUMBER).FirstOrDefault();
                                            //RST6.Filter = "CODE = '" + RST2[eof].CODE + "' AND GRD_NUM = " + RST2[eof].NUMBER;
                                            RST6.FirstOrDefault().MABL = MIAN;
                                            dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE CODE = '{RST2[eof].CODE}' AND GRD_NUM = {RST2[eof].NUMBER}");
                                            //RST6.update();
                                            break;
                                        }
                                    case 26: // برگشت خريد
                                        {
                                            MBKM = (double)(MBKM - RST2[eof].MEGHk * MIAN);
                                            MOGUDI = (double)(MOGUDI - RST2[eof].MEGHk);
                                            rst3Filter_.FirstOrDefault().AVRAGE = MIAN;

                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} WHERE ID = {rst3Filter_.FirstOrDefault().id}");
                                            //rst3Filter_.update();
                                            break;
                                        }
                                }

                            }
                            dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS  WHERE TABLE_NAME = '{TBLTMP}')   DROP VIEW {TBLTMP}");

                        });
                    }
                    else
                    {
                        List<cm_model> RST2 = null;
                        int errorno;
                        string SQL;
                        double MIAN;
                        double MBKM;
                        var MOGUDI = default(double);
                        double temp;
                        for (int EOF = 0; EOF < RST4.Count; EOF++)
                        {
                            var rst = dbms.DoGetDataSQL<_QRE_3>("SELECT     DISTINCT  dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR, dbo.STUF_FSK.MOGODI_A, dbo.STUF_FSK.FI_A, dbo.STUF_FSK.MABL_A FROM         dbo.STUF_DEF INNER JOIN                dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE INNER JOIN              dbo.INVO_LST ON dbo.STUF_FSK.CODE = dbo.INVO_LST.CODE AND dbo.STUF_FSK.ANBAR = dbo.INVO_LST.ANBAR WHERE (dbo.STUF_DEF.RADAH =  " + RST4[EOF].CODE /*RST4[EOF].Fields(0)*/ + "  ) ORDER BY dbo.STUF_FSK.CODE, dbo.STUF_FSK.ANBAR").ToList();
                            switch (RST4[EOF].CODE.ToString())
                            {
                                case "2":
                                    {
                                        // while (!rst.EOF)
                                        for (int rstI = 0; rstI < rst.Count; rstI++)
                                        {


                                            //this.Repaint();
                                            MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE(rst[rstI].CODE);
                                            if (MIAN == 0d)
                                            {
                                                MIAN = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(rst[rstI].CODE);
                                            }
                                            if (MIAN == 0d)
                                            {
                                                dbms.DoExecuteSQL("UPDATE    dbo.INVO_LST SET  AVRAGE = 0, AVRAGE2 = 0 WHERE     (CODE = N'" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[rstI].ANBAR + ")");
                                                dbms.DoExecuteSQL("update dbo.INVO_LST SET MABL = 0, MABL_K = 0 WHERE    ((TAG = 5) OR (TAG = 6) OR (TAG = 7) OR (TAG = 8) OR (TAG = 9) OR (TAG = 10) OR (TAG = 11) OR (TAG = 16) OR (TAG = 17) OR (TAG = 18)) AND ((CODE = N'" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[rstI].ANBAR + "))");
                                            }
                                            else
                                            {
                                                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS  WHERE TABLE_NAME = 'TMPAV101')   DROP VIEW TMPAV101");
                                                dbms.DoExecuteSQL("CREATE VIEW  TMPAV101 as " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,  dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO,dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE WHERE  (dbo.INVO_LST.CODE = '" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[rstI].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH , dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1 = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBARF = " + rst[rstI].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ")  AND (dbo.INVO_LST.TAG = 5) ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN       dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag INNER JOIN  dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[rstI].ANBAR + ") AND (dbo.HEAD_LST_FBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_FBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A,dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH  FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag INNER JOIN " + " dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE WHERE     (dbo.INVO_LST.CODE = '" + rst[rstI].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[rstI].ANBAR + ") AND (dbo.HEAD_LST_KBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_KBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER union " + " SELECT     TOP 100 PERCENT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK, 0 AS megh_mar,' ' AS mol, dbo.ANBGRD_LST.MABL, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK, 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no, 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage, 0 AS id, '17' AS Expr1 FROM   dbo.ANBGRD_LST INNER JOIN  dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM INNER JOIN  dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE " + " WHERE      (dbo.ANBGRD_LST.CODE = '" + rst[rstI].CODE + "') AND (dbo.ANBGRD_HEAD.GRD_ANBAR = " + rst[rstI].ANBAR + ") and ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * - 1 <> 0) AND (dbo.ANBGRD_HEAD.GRD_DATE > " + this.DT + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL)) ORDER BY dbo.ANBGRD_HEAD.GRD_DATE, dbo.ANBGRD_LST.GRD_NUM ");
                                                RST2 = dbms.DoGetDataSQL<cm_model>($"SELECT     TOP 100 PERCENT DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID,  MEGH_R , RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, ID, BARGAH FROM dbo.TMPAV101 ORDER BY DATE_N, BARGAH").ToList();
                                                for (int f = 0; f < RST2.Count; f++)
                                                {
                                                    if (IsCancelRequestedBgWorker) { return; }
                                                    Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        this.co.Text = rst[rstI].CODE;
                                                        Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            progress++;
                                                            PRGR_C0.Value = progress / rcount * 100.0;// Update the progress bar
                                                            UpdateOverallProgressBar();

                                                        }));
                                                        UpdateOverallProgressBar();

                                                    }));
                                                    MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE_KOL(rst[rstI].CODE, (long)RST2[f].DATE_N);

                                                    Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        Text19.Text = Convert.ToString(Convert.ToInt64(Text19.Text) + 1);
                                                    }));
                                                    //this.Repaint();

                                                    var rst3Filter = rst3.Where(x => x.id == RST2[f].ID).FirstOrDefault();
                                                    //var rst[rstI].3Filter = "ID = " + RST2[f].("ID");
                                                    switch (RST2[f].TAG)
                                                    {
                                                        case 1: // خريد
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , AVRAGE2 = {MIAN} WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 22: // برگشت فروش سال قبل
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , AVRAGE2 = {MIAN} WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 24: // برگشت فروش سال قبل
                                                            {
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                rst3Filter.AVRAGE = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , AVRAGE2 = {MIAN} WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 2: // فروش
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} , AVRAGE2 = {MIAN} WHERE ID = {rst3Filter.id}");
                                                                //rst[rstI].3Filter.update();
                                                                break;
                                                            }
                                                        case 5: // انتقالي خروج
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 6: // انتقالي خروج
                                                            {
                                                                rst3Filter.AVRAGE2 = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE2 = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 10: // مواد خروج
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 11:    // موادساير خروج
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 9:    // موادساير خروج
                                                            {
                                                                if (RST2[f].N_KOL != 0 & !IsNull(RST2[f].N_KOL) & Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
                                                                {
                                                                    var RST7 = dbms.DoGetDataSQL<THE_QUERY3>("SELECT  dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + RST2[f].N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").ToList();
                                                                    if (RST7.Count > 0)
                                                                    {
                                                                        //MIAN = RST7.Fields(0) + RST7.Fields(1) + RST7.Fields(2);
                                                                        MIAN = (double)(RST7.FirstOrDefault().IMBIBE_MANF + RST7.FirstOrDefault().IMBIBE_SAR + RST7.FirstOrDefault().MABLKs);
                                                                    }
                                                                }
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                        case 17: // كسري انبار
                                                            {
                                                                // MBKM = MBKM + rst[rstI].2.Fields("MABL_K")
                                                                var RST6Filter = RST6.Where(x => x.CODE == RST2[f].CODE && x.GRD_NUM == RST2[f].NUMBER).ToList();
                                                                //RST6.Filter = "code = '" + RST2[f].("code") + "' and GRD_NUM = " + RST2[f].NUMBER;
                                                                MOGUDI = Math.Round(Math.Round((double)(MOGUDI + RST2[f].MEGHk), 6), 6);
                                                                RST6Filter.FirstOrDefault().MABL = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE GRD_NUM = {RST6Filter.FirstOrDefault().GRD_NUM}");
                                                                //RST6.update();
                                                                break;
                                                            }
                                                        case 18: // فروش
                                                            {
                                                                // MBKM = MBKM - (rst[rstI].2.FieldsMEGHk * MIAN)
                                                                MOGUDI = (double)(MOGUDI - RST2[f].MEGHk);
                                                                var RST6Filter = RST6.Where(x => x.CODE == RST2[f].CODE && x.GRD_NUM == RST2[f].NUMBER).ToList();
                                                                //RST6.Filter = "code = '" + RST2[f].("code") + "' and GRD_NUM = " + RST2[f].("NUMBER");
                                                                RST6Filter.FirstOrDefault().MABL = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE GRD_NUM = {RST6Filter.FirstOrDefault().GRD_NUM}");
                                                                //RST6.update();
                                                                break;
                                                            }
                                                        case 26:    // موادساير خروج
                                                            {
                                                                rst3Filter.AVRAGE = MIAN;
                                                                rst3Filter.MABL = MIAN;
                                                                rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[f].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN},
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                                //rst3Filter.update();
                                                                break;
                                                            }
                                                    }
                                                    //RST2.MoveNext();
                                                }

                                                Dispatcher.Invoke(new Action(() =>
                                                {
                                                    if (this.FORMOL.IsChecked is true)
                                                    {
                                                        dbms.DoExecuteSQL("UPDATE    dbo.DTL_MANF  SET  MABLK = (MEGHk + PERT) * " + MIAN + ", SMABL = " + MIAN + " WHERE     (CODE = N'" + rst[rstI].CODE + "')");
                                                    }
                                                }));
                                            }
                                            //rst[rstI]..MoveNext();
                                        }
                                        break;
                                    }
                                case "3":
                                    {
                                        //while (!rst.EOF)
                                        for (int ef = 0; ef < rst.Count; ef++)
                                        {

                                            //this.Repaint();
                                            MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE(rst[ef].CODE);
                                            if (MIAN == 0d)
                                            {
                                                MIAN = CL_HESABDARI_AUTO_BAZ.GETFIRSTPRICE(rst[ef].CODE);
                                            }
                                            if (MIAN == 0d)
                                            {
                                                dbms.DoExecuteSQL("UPDATE    dbo.INVO_LST SET  AVRAGE = 0, AVRAGE2 = 0 WHERE     (CODE = N'" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[ef].ANBAR + ")");
                                                dbms.DoExecuteSQL("update dbo.INVO_LST SET MABL = 0, MABL_K = 0 WHERE    ((TAG = 5) OR (TAG = 6) OR (TAG = 7) OR (TAG = 8) OR (TAG = 9) OR (TAG = 10) OR (TAG = 11) OR (TAG = 16) OR (TAG = 17) OR (TAG = 18)) AND ((CODE = N'" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[ef].ANBAR + "))");
                                            }
                                            else
                                            {
                                                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS  WHERE TABLE_NAME = 'TMPAV101')   DROP VIEW TMPAV101");
                                                dbms.DoExecuteSQL("CREATE VIEW  TMPAV101 as " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,  dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO,dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE WHERE  (dbo.INVO_LST.CODE = '" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[ef].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH , dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1 = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBARF = " + rst[ef].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ")  AND (dbo.INVO_LST.TAG = 5) ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN       dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag INNER JOIN  dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[ef].ANBAR + ") AND (dbo.HEAD_LST_FBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_FBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A,dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH  FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag INNER JOIN " + " dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE WHERE     (dbo.INVO_LST.CODE = '" + rst[ef].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[ef].ANBAR + ") AND (dbo.HEAD_LST_KBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_KBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER union " + " SELECT     TOP 100 PERCENT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK, 0 AS megh_mar,' ' AS mol, dbo.ANBGRD_LST.MABL, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK, 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no, 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage, 0 AS id, '17' AS Expr1 FROM   dbo.ANBGRD_LST INNER JOIN  dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM INNER JOIN  dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE " + " WHERE      (dbo.ANBGRD_LST.CODE = '" + rst[ef].CODE + "') AND (dbo.ANBGRD_HEAD.GRD_ANBAR = " + rst[ef].ANBAR + ") and ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * - 1 <> 0) AND (dbo.ANBGRD_HEAD.GRD_DATE > " + this.DT + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL)) ORDER BY dbo.ANBGRD_HEAD.GRD_DATE, dbo.ANBGRD_LST.GRD_NUM ");
                                                RST2 = dbms.DoGetDataSQL<cm_model>($"SELECT     TOP 100 PERCENT DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID,  MEGH_R , RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, ID, BARGAH FROM dbo.TMPAV101 ORDER BY DATE_N, BARGAH").ToList();
                                                for (int w = 0; w < RST2.Count; w++) //while (!RST2.EOF)
                                                {
                                                    if (IsCancelRequestedBgWorker) { return; }
                                                    Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        this.co.Text = rst[ef].CODE;
                                                        Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            progress++;
                                                            PRGR_C0.Value = progress / rcount * 100.0;// Update the progress bar
                                                            UpdateOverallProgressBar();

                                                        }));
                                                        UpdateOverallProgressBar();

                                                    }));
                                                    MIAN = CL_HESABDARI_AUTO_BAZ.GETSTANDARDPRICE_KOL(rst[ef].CODE, (long)RST2[w].DATE_N);
                                                    Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        Text19.Text = Convert.ToString(Convert.ToInt64(Text19.Text) + 1);
                                                    }));
                                                    //this.Repaint();
                                                    var rst3_Filter = rst3.Where(x => x.id == RST2[w].ID).FirstOrDefault();
                                                    //rst3.Filter = "ID = " + RST2[w].("ID");
                                                    switch (RST2[w].TAG)
                                                    {
                                                        case 1: // خريد
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} 
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 22: // برگشت فروش سال قبل
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} 
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 24: // برگشت فروش سال قبل
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} 
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 2: // فروش
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} 
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 6: // انتقالي خروج
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 5: // انتقالي خروج
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.AVRAGE2 = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));

                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    AVRAGE2 = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 10: // مواد خروج
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 11:    // موادساير خروج
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));

                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 9:    // موادساير خروج
                                                            {
                                                                if (RST2[w].N_KOL != 0 & !IsNull(RST2[w].N_KOL) & Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5")
                                                                {
                                                                    var RST7Open = dbms.DoGetDataSQL<THE_QUERY2>("SELECT     dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + RST2[w].N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").ToList();
                                                                    if (RST7Open.Count > 0)
                                                                    {
                                                                        MIAN = (double)(RST7Open.FirstOrDefault().IMBIBE_MANF + RST7Open.FirstOrDefault().IMBIBE_SAR + RST7Open.FirstOrDefault().MABLKs);
                                                                        //MIAN = RST7Open.Fields(0) + RST7Open.Fields(1) + RST7Open.Fields(2);
                                                                    }
                                                                }
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                        case 17: // كسري انبار
                                                            {
                                                                // MBKM = MBKM + rst2.Fields("MABL_K")
                                                                var RST6Filter_ = RST6.Where(x => x.CODE == RST2[w].CODE && x.GRD_NUM == RST2[w].NUMBER).FirstOrDefault();
                                                                //RST6.Filter = "code = '" + RST2[w].CODE + "' and GRD_NUM = " + RST2[w].NUMBER;
                                                                MOGUDI = Math.Round(Math.Round((double)(MOGUDI + RST2[w].MEGHk), 6), 6);
                                                                RST6Filter_.MABL = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE GRD_NUM = {RST6Filter_.GRD_NUM}");
                                                                //RST6Filter_.update();
                                                                break;
                                                            }
                                                        case 18: // فروش
                                                            {
                                                                // MBKM = MBKM - (rst2.FieldsMEGHk * MIAN)
                                                                MOGUDI = (double)(MOGUDI - RST2[w].MEGHk);
                                                                //RST6.Filter = "code = '" + RST2[w].CODE + "' and GRD_NUM = " + RST2[w].NUMBER;
                                                                var RST6Filter_ = RST6.Where(x => x.CODE == RST2[w].CODE && x.GRD_NUM == RST2[w].NUMBER).FirstOrDefault();
                                                                RST6Filter_.MABL = MIAN;
                                                                dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE GRD_NUM = {RST6Filter_.GRD_NUM}");
                                                                //RST6Filter_.update();
                                                                break;
                                                            }
                                                        case 26:    // موادساير خروج
                                                            {
                                                                rst3_Filter.AVRAGE = MIAN;
                                                                rst3_Filter.MABL = MIAN;
                                                                rst3_Filter.MABL_K = Math.Round((double)(MIAN * RST2[w].MEGHk));

                                                                dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3_Filter.MABL_K}
                                                                                    WHERE ID = {rst3_Filter.id}");
                                                                //rst3_Filter.update();
                                                                break;
                                                            }
                                                    }
                                                    //RST2.MoveNext();
                                                }
                                                Dispatcher.Invoke(new Action(() =>
                                                {
                                                    if (FORMOL.IsChecked is true)
                                                    {
                                                        dbms.DoExecuteSQL("UPDATE    dbo.DTL_MANF  SET  MABLK = (MEGHk + PERT) * " + MIAN + ", SMABL = " + MIAN + " WHERE     (CODE = N'" + rst[ef].CODE + "')");
                                                    }
                                                }));
                                            }
                                            //rst.MoveNext();
                                        }
                                        break;
                                    }

                                default:
                                    {

                                        for (int o = 0; o < rst.Count; o++) //while (!rst.EOF)
                                        {

                                            //this.Repaint();
                                            MBKM = (double)rst[o].MABL_A;
                                            MIAN = (double)rst[o].FI_A;
                                            MOGUDI = (double)rst[o].MOGODI_A;
                                            dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS  WHERE TABLE_NAME = 'TMPAV101')   DROP VIEW TMPAV101");
                                            dbms.DoExecuteSQL("CREATE VIEW  TMPAV101 as " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, dbo.INVO_LST.TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,  dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO,dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG = dbo.TAGCOD.CODE WHERE  (dbo.INVO_LST.CODE = '" + rst[o].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[o].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH , dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST.DATE_N, 6 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBARF AS ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG INNER JOIN dbo.TAGCOD ON dbo.HEAD_LST.TAG + 1  = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[o].CODE + "') AND (dbo.INVO_LST.ANBARF = " + rst[o].ANBAR + ") AND (dbo.HEAD_LST.DATE_N > " + this.DT + ")  AND (dbo.INVO_LST.TAG = 5) ORDER BY dbo.HEAD_LST.DATE_N,  dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_FBK.DATE_N, 4 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF,dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL,dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE , dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH FROM         dbo.INVO_LST INNER JOIN       dbo.HEAD_LST_FBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_FBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_FBK.dtag INNER JOIN  dbo.TAGCOD ON dbo.HEAD_LST_FBK.htag = dbo.TAGCOD.CODE " + " WHERE     (dbo.INVO_LST.CODE = '" + rst[o].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[o].ANBAR + ") AND (dbo.HEAD_LST_FBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_FBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER UNION " + " SELECT     TOP 100 PERCENT dbo.HEAD_LST_KBK.DATE_N, 3 AS TAG, dbo.INVO_LST.NUMBER, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A,dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.ID, dbo.TAGCOD.BARGAH  FROM         dbo.INVO_LST INNER JOIN   dbo.HEAD_LST_KBK ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST_KBK.NUMBER1 AND dbo.INVO_LST.TAG = dbo.HEAD_LST_KBK.dtag INNER JOIN " + " dbo.TAGCOD ON dbo.HEAD_LST_KBK.htag = dbo.TAGCOD.CODE WHERE     (dbo.INVO_LST.CODE = '" + rst[o].CODE + "') AND (dbo.INVO_LST.ANBAR = " + rst[o].ANBAR + ") AND (dbo.HEAD_LST_KBK.DATE_N > " + this.DT + ") ORDER BY dbo.HEAD_LST_KBK.DATE_N, dbo.TAGCOD.BARGAH, dbo.INVO_LST.NUMBER  union " + " SELECT     TOP 100 PERCENT dbo.ANBGRD_HEAD.GRD_DATE, dbo.UIIF(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3, N'>', 0, 18, 17) AS TAG,dbo.ANBGRD_LST.GRD_NUM, dbo.ANBGRD_HEAD.GRD_ANBAR, 1 AS radif, dbo.ANBGRD_LST.CODE,(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEG, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) AS MEGK, 0 AS megh_mar,' ' AS mol, dbo.ANBGRD_LST.MABL, ABS(dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * dbo.ANBGRD_LST.MABL AS MABLK, 0 AS froma, '' AS nrasid, 0 AS MEGH_R, dbo.STUF_DEF.RADAH, 0 AS sanadno, '  ' AS cust_no, 0 AS anbarf, dbo.STUF_DEF.VAHED, 0 AS n_kol, 0 AS n_moin, 0 AS n_taf, dbo.ANBGRD_LST.MABL AS avrage, 0 AS id, '17' AS Expr1 FROM   dbo.ANBGRD_LST INNER JOIN  dbo.ANBGRD_HEAD ON dbo.ANBGRD_LST.GRD_NUM = dbo.ANBGRD_HEAD.GRD_NUM INNER JOIN  dbo.STUF_DEF ON dbo.ANBGRD_LST.CODE = dbo.STUF_DEF.CODE " + " WHERE      (dbo.ANBGRD_LST.CODE = '" + rst[o].CODE + "') AND (dbo.ANBGRD_HEAD.GRD_ANBAR = " + rst[o].ANBAR + ") and ((dbo.ANBGRD_LST.MOG - dbo.ANBGRD_LST.NUM3) * - 1 <> 0) AND (dbo.ANBGRD_HEAD.GRD_DATE > " + this.DT + ") AND (NOT (dbo.ANBGRD_HEAD.N_S IS NULL)) ORDER BY dbo.ANBGRD_HEAD.GRD_DATE, dbo.ANBGRD_LST.GRD_NUM ");
                                            RST2 = dbms.DoGetDataSQL<cm_model>($"SELECT     TOP 100 PERCENT DATE_N, TAG, NUMBER, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID,  MEGH_R , RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, ID, BARGAH FROM dbo.TMPAV101 ORDER BY DATE_N, BARGAH").ToList();
                                            for (int e = 0; e < RST2.Count; e++) // while (!RST2.EOF)
                                            {
                                                if (IsCancelRequestedBgWorker) { return; }
                                                Dispatcher.Invoke(new Action(() =>
                                                {
                                                    Text19.Text = Convert.ToString(Convert.ToInt64(Text19.Text) + 1);
                                                }));
                                                if (IsCancelRequestedBgWorker) { return; }
                                                Dispatcher.Invoke(new Action(() =>
                                                {
                                                    this.co.Text = rst[o].CODE;
                                                    Dispatcher.Invoke(new Action(() =>
                                                    {
                                                        progress++;
                                                        PRGR_C0.Value = progress / rcount * 100.0;// Update the progress bar
                                                        UpdateOverallProgressBar();

                                                    }));
                                                    UpdateOverallProgressBar();

                                                }));
                                                //this.Repaint();
                                                var rst3Filter = rst3.Where(x => x.id == RST2[e].ID).FirstOrDefault();
                                                //rst3.Filter = "ID = " + RST2[e].("ID");
                                                switch (RST2[e].TAG)
                                                {
                                                    case 1: // خريد
                                                        {
                                                            MBKM = (double)(MBKM + RST2[e].MABL_K);
                                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[e].MEGHk), 6);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 22: // برگشت فروش سال قبل
                                                        {
                                                            MBKM = (double)(MBKM + MIAN * RST2[e].MEGH_MAR);
                                                            MOGUDI = (double)(MOGUDI + RST2[e].MEGH_MAR);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 24: // برگشت فروش سال قبل
                                                        {
                                                            MBKM = (double)(MBKM + RST2[e].MEGHk * MIAN);
                                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[e].MEGHk), 6);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 2: // فروش
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);
                                                            rst3Filter.AVRAGE = MIAN;

                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");

                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 3: // برگشت خريد
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGH_MAR * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGH_MAR);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE2 = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 4: // برگشت فروش
                                                        {
                                                            MBKM = (double)(MBKM + RST2[e].MEGH_MAR * rst3Filter.AVRAGE);
                                                            MOGUDI = (double)(MOGUDI + RST2[e].MEGH_MAR);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE2 = {MIAN}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 5: // انتقالي خروج
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[e].MEGHk));

                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 6: // انتقالي ورود
                                                        {
                                                            MBKM = (double)(MBKM + RST2[e].MABL_K);
                                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[e].MEGHk), 6);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE2 = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE2 = {MIAN} 
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 10: // مواد خروج
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[e].MEGHk));

                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 11:    // موادساير خروج
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[e].MEGHk));

                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 9:    // توليد
                                                        {
                                                            MBKM = (double)(MBKM + RST2[e].MABL_K);
                                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[e].MEGHk), 6);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                            }
                                                            else
                                                            {
                                                                MIAN = MBKM / MOGUDI;
                                                            }
                                                            rst3Filter.AVRAGE = MIAN;
                                                            rst3Filter.MABL = MIAN;
                                                            rst3Filter.MABL_K = Math.Round((double)(MIAN * RST2[e].MEGHk));

                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} ,
                                                                                    MABL = {MIAN} ,
                                                                                    MABL_K = {rst3Filter.MABL_K}
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                    case 17: // كسري انبار
                                                        {
                                                            MBKM = (double)(MBKM + MIAN * RST2[e].MEGHk);
                                                            MOGUDI = Math.Round((double)(MOGUDI + RST2[e].MEGHk), 6);
                                                            if (MBKM == 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            else if (MOGUDI == 0d)
                                                            {
                                                                MIAN = 0d;
                                                                MBKM = 0d;
                                                                // Else
                                                                // MIAN = MBKM / MOGUDI
                                                            }
                                                            if (MIAN < 0d)
                                                            {
                                                                MIAN = 0d;
                                                            }
                                                            var _RST6Filter_ = RST6.Where(x => x.CODE == RST2[e].CODE && x.GRD_NUM == RST2[e].NUMBER).FirstOrDefault();
                                                            //var _RST6Filter_ = "code = '" + RST2[e].CODE + "' and GRD_NUM = " + RST2[e].NUMBER;
                                                            //RST6.Filter = "code = '" + RST2[e].("code") + "' and GRD_NUM = " + RST2[e].("NUMBER");
                                                            _RST6Filter_.MABL = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE CODE = '{RST2[e].CODE}' AND GRD_NUM = {RST2[e].NUMBER}");
                                                            //_RST6Filter_.update();
                                                            break;
                                                        }
                                                    case 18: // فروش
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);

                                                            var _RST6Filter_ = RST6.Where(x => x.CODE == RST2[e].CODE && x.GRD_NUM == RST2[e].NUMBER).FirstOrDefault();
                                                            //RST6.Filter = "code = '" + RST2[e].("code") + "' and GRD_NUM = " + RST2[e].("NUMBER");

                                                            _RST6Filter_.MABL = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {MIAN} WHERE CODE = '{RST2[e].CODE}' AND GRD_NUM = {RST2[e].NUMBER}");
                                                            //RST6.update();
                                                            break;
                                                        }
                                                    case 26: // برگشت خريد
                                                        {
                                                            MBKM = (double)(MBKM - RST2[e].MEGHk * MIAN);
                                                            MOGUDI = (double)(MOGUDI - RST2[e].MEGHk);
                                                            rst3Filter.AVRAGE = MIAN;
                                                            dbms.DoExecuteSQL($@"UPDATE dbo.INVO_LST SET 
                                                                                    AVRAGE = {MIAN} 
                                                                                    WHERE ID = {rst3Filter.id}");
                                                            //rst3Filter.update();
                                                            break;
                                                        }
                                                }
                                                //RST2.MoveNext();
                                            }

                                        }
                                        break;
                                    }
                            }
                            //rst.Close();
                            //RST4[EOF].MoveNext();
                        }
                    }
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

                CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(1, 9999999999);
                try
                {
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
                CL_HESABDARI_AUTO_BAZ.SANADKHORUGSAYER(HF1, HF2);

                try
                {
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