using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using Syncfusion.XlsIO.Parser.Biff_Records.Formula;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static Prg_Proccessy.Generaly.CL_Generaly;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{
    /// <summary>
    /// پیام و یادآوری
    /// </summary>
    public partial class INBOXPAN : Window
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        private System.Windows.Threading.DispatcherTimer timer;
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; set; } = new ObservableCollection<CutsomStatus_Model>();
        public ObservableCollection<CUST_HESAB> COMP_COD_DATA { get; set; } = new ObservableCollection<CUST_HESAB>();
        public INBOXPAN(int? idnum = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (idnum != null)
            {
                IDNUM.Text = idnum.ToString();
            }

            ThreadSafeProperties.IsStillWorking = true;
        }
        public bool NowIsReady { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            InitializeTimer();

            ReGetData();

        }
        private void InitializeTimer()
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        private void ReGetData()
        {
            var MasterData = dbms.DoGetDataSQL<MESAGEP>($"SELECT IDNUM, PERSONEL, PAYAM, STATUS, STDATE, STTIME, USERNAME, COMP_COD, CRT, UID FROM dbo.MESAGEP WHERE IDNUM = {IDNUM.Text}").FirstOrDefault();
            if (MasterData != null)
            {
                USERNAME.Text = MasterData.USERNAME;
                COMP_COD_TXT.Text = MasterData.COMP_COD;
                PAYAM.Text = MasterData.PAYAM;
                STATUS.Text = (MasterData.STATUS == 1 ? "مشاهده شده" : "مشاهده نـشده"); //1;"مشاهده نشده";2;"مشاهده شده"
                STDATE.Text = Strings.Format(MasterData.STDATE, "####/##/##");
                STTIME.Text = Strings.Format(MasterData.STTIME, "##:##");
                PERSONEL.SelectedValue = MasterData.PERSONEL; PERSONEL.Items.Refresh();
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //کبموباکس مجری
            var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, USERCO FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD}").ToList();

            bool IsHameh = false;
            foreach (var rows in rst_personel)
            {
                if (string.IsNullOrEmpty(rows?.SAL_NAME))
                {
                    IsHameh = true; break;
                }
                else
                {
                    rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                }
            }

            if (IsHameh)
            {
                rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();
                foreach (var rows in rst_personel)
                {
                    if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                    {
                        rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                    }
                }
            }

            //مجری در دیتاگرید
            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.SelectedValue = null;
            PERSONEL.SelectedValue = Baseknow.USERCOD;

            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "انجام نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "انجام شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "لغو شده" });
            STATUS.ItemsSource = STATUS_COMBO_DATA;

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (PAYAM.Text == "كاربر محترم سلام نرم افزار شما جهت آبديت  تا  10 ثانيه ديگر بصورت خودكار بسته خواهد شد بعد از چند دقيقه ميتوانيد دوباره وارد شويد با تشكر")
            {
                HandleAutoUpdateMessage();
            }

            //// Flash window code would go here if needed
            this.Topmost = false;
            this.Activate();
            this.Topmost = true;
            //this.Focus();
        }
        private async void HandleAutoUpdateMessage()
        {
            UpdateMessageStatus();

            //var startTime = DateTime.Now;
            //while (DateTime.Now < startTime.AddSeconds(10))
            //{
            //    this.Title = (10 - (DateTime.Now - startTime).Seconds).ToString();
            //    await System.Windows.Threading.Dispatcher.Yield();
            //}

            // Open MSGEXIT form and quit
            // You'll need to implement this part based on your application structure
        }
        private void UpdateMessageStatus()
        {
            timer?.Stop();
            string query = "UPDATE MESAGEP SET STATUS = 2 , IsNotifyCalled = 1 WHERE IDNUM = @IDNUM";
            dbms.DoExecuteSQL(query, new { IDNUM = IDNUM.Text });

            dbms = null; //Kinda Dispose
            this.Close();
        }
        private void BTN_GOTIT_Click(object sender, RoutedEventArgs e)
        {
            UpdateMessageStatus();
        }

        private void BTN_REPONSE_Click(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<WIN_MESSAGEPANEL>().Any())
            {
                UpdateMessageStatus();

                WIN_MESSAGEPANEL wIN_MESSAGEPANEL = new WIN_MESSAGEPANEL(Convert.ToInt64(PERSONEL.SelectedValue));
                wIN_MESSAGEPANEL.Tag = "1";
                this.Close();
                wIN_MESSAGEPANEL.Show();
            }
        }
        private void BTN_FORWARD_Click(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.Windows.OfType<WIN_MESSAGEPANEL>().Any())
            {
                UpdateMessageStatus();

                WIN_MESSAGEPANEL wIN_MESSAGEPANEL = new WIN_MESSAGEPANEL(Convert.ToInt64(PERSONEL.SelectedValue),PAYAM.Text);
                wIN_MESSAGEPANEL.Tag = "3";
                this.Close();
                wIN_MESSAGEPANEL.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ThreadSafeProperties.IsStillWorking = false;
        }
    }
}
