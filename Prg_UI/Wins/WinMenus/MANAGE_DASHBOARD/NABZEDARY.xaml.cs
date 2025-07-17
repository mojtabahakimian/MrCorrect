using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD
{
    public class QRE_KOL_90_30
    {
        public int? CountOfCOMP_COD { get; set; }
        public int? STDATE { get; set; }
        public int? DD { get; set; }
        public int? mm { get; set; }
    }
    public class QRE_KOL_7
    {
        public int? CountOfCOMP_COD { get; set; }
        public int? STDATE { get; set; }
        public int? DD { get; set; }
    }
    public class QRE_JOZ_90_30
    {
        public int? STDATE { get; set; }
        public int? IDC { get; set; }
        public int? DD { get; set; }
        public int? mm { get; set; }
    }
    public class QRE_JOZ_7
    {
        public int? STDATE { get; set; }
        public int? IDC { get; set; }
        public int? DD { get; set; }
    }
    public partial class NABZEDARY : Window
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
        public ObservableCollection<QRE_KOL_90_30> KOL_90_DATA { get; set; } = new ObservableCollection<QRE_KOL_90_30>();
        public ObservableCollection<QRE_KOL_90_30> KOL_30_DATA { get; set; } = new ObservableCollection<QRE_KOL_90_30>();
        public ObservableCollection<QRE_KOL_7> KOL_7_DATA { get; set; } = new ObservableCollection<QRE_KOL_7>();

        public ObservableCollection<QRE_JOZ_90_30> JOZ_90_DATA { get; set; } = new ObservableCollection<QRE_JOZ_90_30>();
        public ObservableCollection<QRE_JOZ_90_30> JOZ_30_DATA { get; set; } = new ObservableCollection<QRE_JOZ_90_30>();
        public ObservableCollection<QRE_JOZ_7> JOZ_7_DATA { get; set; } = new ObservableCollection<QRE_JOZ_7>();

        public NABZEDARY()
        {
            InitializeComponent();

            this.DataContext = this;
            this.Owner = PublicVRB.WINBASE;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            var KOL_90 = dbms.DoGetDataSQL<QRE_KOL_90_30>("SELECT TOP(90) COUNT(COMP_COD) AS CountOfCOMP_COD, STDATE, dbo.Uday(STDATE) AS DD, dbo.Umonth(STDATE) AS mm FROM TASKS GROUP BY STDATE, dbo.Uday(STDATE), dbo.Umonth(STDATE) ORDER BY STDATE DESC, COUNT(COMP_COD) DESC").ToList();
            var KOL_30 = dbms.DoGetDataSQL<QRE_KOL_90_30>("SELECT TOP(30) COUNT(COMP_COD) AS CountOfCOMP_COD, STDATE, dbo.Uday(STDATE) AS DD, dbo.Umonth(STDATE) AS mm FROM TASKS GROUP BY STDATE, dbo.Uday(STDATE), dbo.Umonth(STDATE) ORDER BY STDATE DESC, COUNT(COMP_COD) DESC").ToList();
            var KOL_7 = dbms.DoGetDataSQL<QRE_KOL_7>("SELECT TOP(7) COUNT(COMP_COD) AS CountOfCOMP_COD, STDATE, dbo.Uday(STDATE) AS DD FROM TASKS GROUP BY STDATE, dbo.Uday(STDATE) ORDER BY STDATE DESC, COUNT(COMP_COD) DESC").ToList();

            var JOZ_90 = dbms.DoGetDataSQL<QRE_JOZ_90_30>("SELECT TOP(90) STDATE AS STDATE, COUNT(IDD) AS IDC, dbo.Uday(STDATE) AS DD, dbo.Umonth(STDATE) AS mm FROM EVENTS GROUP BY dbo.Uday(STDATE), STDATE, dbo.Umonth(STDATE)").ToList();
            var JOZ_30 = dbms.DoGetDataSQL<QRE_JOZ_90_30>("SELECT TOP(30) STDATE AS STDATE, COUNT(IDD) AS IDC, dbo.Uday(STDATE) AS DD, dbo.Umonth(STDATE) AS mm FROM EVENTS GROUP BY dbo.Uday(STDATE), STDATE, dbo.Umonth(STDATE)").ToList();
            var JOZ_7 = dbms.DoGetDataSQL<QRE_JOZ_7>("SELECT TOP(7) STDATE AS STDATE, COUNT(IDD) AS IDC, dbo.Uday(STDATE) AS DD FROM EVENTS GROUP BY dbo.Uday(STDATE), STDATE").ToList();

            foreach (var item in KOL_90) KOL_90_DATA.Add(new QRE_KOL_90_30 { CountOfCOMP_COD = item.CountOfCOMP_COD, STDATE = item.STDATE, DD = item.DD, mm = item.mm });
            foreach (var item in KOL_30) KOL_30_DATA.Add(new QRE_KOL_90_30 { CountOfCOMP_COD = item.CountOfCOMP_COD, STDATE = item.STDATE, DD = item.DD, mm = item.mm });
            foreach (var item in KOL_30) KOL_7_DATA.Add(new QRE_KOL_7 { CountOfCOMP_COD = item.CountOfCOMP_COD, STDATE = item.STDATE, DD = item.DD });

            foreach (var item in JOZ_90) JOZ_90_DATA.Add(new QRE_JOZ_90_30 { STDATE = item.STDATE, IDC = item.IDC, DD = item.DD, mm = item.mm });
            foreach (var item in JOZ_30) JOZ_30_DATA.Add(new QRE_JOZ_90_30 { STDATE = item.STDATE, IDC = item.IDC, DD = item.DD, mm = item.mm });
            foreach (var item in JOZ_7) JOZ_7_DATA.Add(new QRE_JOZ_7 { STDATE = item.STDATE, IDC = item.IDC, DD = item.DD });
            ProcLoader.Stop(Prc);
        }
    }
}
