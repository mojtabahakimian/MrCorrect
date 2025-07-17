using Dapper;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
using static Dapper.SqlMapper;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD
{
    public partial class NABZEFROOSH : Window
    {
     
        //Lists Here Begin
        //برای پیش فاکتور ها
        public ObservableCollection<QRE_PISH_90_30> PISH_90_DATA { get; set; } = new ObservableCollection<QRE_PISH_90_30>();
        public ObservableCollection<QRE_PISH_90_30> PISH_30_DATA { get; set; } = new ObservableCollection<QRE_PISH_90_30>();
        public ObservableCollection<QRE_PISH_7> PISH_7_DATA { get; set; } = new ObservableCollection<QRE_PISH_7>();


        //برای فاکتور فروش ها
        public ObservableCollection<QRE_FR_90_30> FOROOSH_90_DATA { get; set; } = new ObservableCollection<QRE_FR_90_30>();
        public ObservableCollection<QRE_FR_90_30> FOROOSH_30_DATA { get; set; } = new ObservableCollection<QRE_FR_90_30>();
        public ObservableCollection<QRE_FR_7> FOROOSH_7_DATA { get; set; } = new ObservableCollection<QRE_FR_7>();
        //End
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
        public NABZEFROOSH()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Owner = PublicVRB.WINBASE;
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();

            var pish90 = dbms.DoGetDataSQL<QRE_PISH_90_30>("SELECT top(90) SUM(INVO_LST.MEGHk) AS MEGHT, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, dbo.Uday(HEAD_LST.DATE_N) AS DD, dbo.Umonth(HEAD_LST.DATE_N) AS mm, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) GROUP BY HEAD_LST.DATE_N, dbo.Uday(HEAD_LST.DATE_N), dbo.Umonth(HEAD_LST.DATE_N) ORDER BY HEAD_LST.DATE_N DESC").ToList();
            var pish30 = dbms.DoGetDataSQL<QRE_PISH_90_30>("SELECT top(30) SUM(INVO_LST.MEGHk) AS MEGHT, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, dbo.Uday(HEAD_LST.DATE_N) AS DD,dbo.Umonth(HEAD_LST.DATE_N) AS mm, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) GROUP BY HEAD_LST.DATE_N, dbo.Uday(HEAD_LST.DATE_N), dbo.Umonth(HEAD_LST.DATE_N) ORDER BY HEAD_LST.DATE_N DESC").ToList();
            var pish7 = dbms.DoGetDataSQL<QRE_PISH_7>("SELECT TOP(7) SUM(INVO_LST.MEGHk) AS MEGHT, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, dbo.Uday(HEAD_LST.DATE_N) AS DD, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) GROUP BY HEAD_LST.DATE_N, dbo.Uday(HEAD_LST.DATE_N) ORDER BY HEAD_LST.DATE_N DESC").ToList();

            var factor90 = dbms.DoGetDataSQL<QRE_FR_90_30>("SELECT top(90) SUM(INVO_LST.MEGHk) AS MEGHT, dbo.Uday(HEAD_LST.DATE_N) AS dd, dbo.Umonth(HEAD_LST.DATE_N) AS mm, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = - 1) GROUP BY dbo.Uday(HEAD_LST.DATE_N), HEAD_LST.DATE_N, dbo.Umonth(HEAD_LST.DATE_N) ORDER BY HEAD_LST.DATE_N").ToList();
            var factor30 = dbms.DoGetDataSQL<QRE_FR_90_30>("SELECT top(30) SUM(INVO_LST.MEGHk) AS MEGHT, dbo.Uday(HEAD_LST.DATE_N) AS dd, dbo.Umonth(HEAD_LST.DATE_N) AS mm, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = - 1) GROUP BY dbo.Uday(HEAD_LST.DATE_N), HEAD_LST.DATE_N, dbo.Umonth(HEAD_LST.DATE_N) ORDER BY HEAD_LST.DATE_N").ToList();
            var factor7 = dbms.DoGetDataSQL<QRE_FR_7>("SELECT top(7) SUM(INVO_LST.MEGHk) AS MEGHT, dbo.Uday(HEAD_LST.DATE_N) AS dd, SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K, HEAD_LST.DATE_N FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = - 1) GROUP BY dbo.Uday(HEAD_LST.DATE_N), HEAD_LST.DATE_N ORDER BY HEAD_LST.DATE_N").ToList();

            foreach (var item in pish90) PISH_90_DATA.Add(new QRE_PISH_90_30 { MABL_K = item.MABL_K, DD = item.DD });
            foreach (var item in pish30) PISH_30_DATA.Add(new QRE_PISH_90_30 { MABL_K = item.MABL_K, DD = item.DD });
            foreach (var item in pish7) PISH_7_DATA.Add(new QRE_PISH_7 { MABL_K = item.MABL_K, DD = item.DD });


            foreach (var item in factor90) FOROOSH_90_DATA.Add(new QRE_FR_90_30 { MABL_K = item.MABL_K, dd = item.dd });
            foreach (var item in factor30) FOROOSH_30_DATA.Add(new QRE_FR_90_30 { MABL_K = item.MABL_K, dd = item.dd });
            foreach (var item in factor7) FOROOSH_7_DATA.Add(new QRE_FR_7 { MABL_K = item.MABL_K, dd = item.dd });

            ProcLoader.Stop(Prc);
        }
    }
}
