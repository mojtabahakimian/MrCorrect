using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{

    public partial class WIN_REMIND_DTL : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        public ObservableCollection<REMAINDER> REMINDERS_DATA { get; set; } = new ObservableCollection<REMAINDER>();
        public ObservableCollection<CutsomPeriority_Model> PERIORITY_COMBO_DATA { get; set; } = new ObservableCollection<CutsomPeriority_Model>();
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; set; } = new ObservableCollection<CutsomStatus_Model>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WIN_REMIND_DTL()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            REMINDERS_DATA?.Clear();
            var RecordSource = dbms.DoGetDataSQL<REMAINDER>(@$"SELECT dbo.REMAINDER.IDNUM, dbo.REMAINDER.PERSONEL, dbo.REMAINDER.PAYAM, dbo.REMAINDER.STATUS, 
                                                               dbo.REMAINDER.CTDATE, dbo.REMAINDER.CTTIME, dbo.REMAINDER.USERNAME, dbo.REMAINDER.COMP_COD, 
                                                               dbo.REMAINDER.STDATE, dbo.REMAINDER.STTIME, dbo.REMAINDER.SMSOK, dbo.REMAINDER.CRT, dbo.REMAINDER.UID, dbo.CUST_HESAB.NAME, dbo.CUST_HESAB.hes
                                                               FROM dbo.REMAINDER
                                                                    LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.REMAINDER.COMP_COD=dbo.CUST_HESAB.hes
                                                               WHERE PERSONEL = {Baseknow.USERCOD}").ToList();
            foreach (var item in RecordSource)
            {
                REMINDERS_DATA.Add(item);
            }
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //کبموباکس مجری
            var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, USERCO FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD}").ToList();
            bool IsHameh = true;
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

            //کمبوباکس مجری در دیتاگرید
            PERSONEL_CMB.ItemsSource = rst_personel;


            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "لغو شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "در جریان" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "تمام شده" });

            //اولویت در دیتاگرید
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 1, PERIORITY_NAME = "فوری" });
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 2, PERIORITY_NAME = "معمولی" });

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //لغو
            if (!(sender is Button btn && btn.Tag != null)) return;


            var SelectedRow = (btn.Tag as REMAINDER);
            if (SelectedRow != null && SelectedRow?.IDNUM != null)
            {
                var remainder = dbms.DoGetDataSQL<REMAINDER>($"SELECT STATUS FROM REMAINDER WHERE IDNUM = {SelectedRow.IDNUM}").FirstOrDefault();
                if (remainder != null)
                {
                    if (remainder.STATUS == 1)
                    {
                        dbms.DoExecuteSQL($"UPDATE REMAINDER SET STATUS = 3 WHERE IDNUM = {SelectedRow.IDNUM}");

                        new Msgwin(false, "لغو شد....!").ShowDialog();
                    }
                    else
                    {
                        new Msgwin(false, "فقط  يادآوري هاي درجريان قابل لغو است....!").ShowDialog();
                    }
                }
            }
        }
    }
}
