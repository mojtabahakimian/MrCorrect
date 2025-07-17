using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
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

    public class QRE_90_30_MALI
    {
        public long? DATE_S { get; set; }
        public double? mab { get; set; }
        public int? dd { get; set; }
        public int? mm { get; set; }
    }
    public class QRE_7_MALI
    {
        public long? DATE_S { get; set; }
        public double? mab { get; set; }
    }

    public class QRE_90_30_7_S
    {
        public double? MAB { get; set; }
        public int? dd { get; set; }
        public int? mm { get; set; }
        public long? DATE { get; set; }
    }

    public partial class NABZEMALI : Window
    {
        public ObservableCollection<QRE_90_30_MALI> MALI_90_DATA { get; set; } = new ObservableCollection<QRE_90_30_MALI>();
        public ObservableCollection<QRE_90_30_MALI> MALI_30_DATA { get; set; } = new ObservableCollection<QRE_90_30_MALI>();
        public ObservableCollection<QRE_7_MALI> MALI_7_DATA { get; set; } = new ObservableCollection<QRE_7_MALI>();


        public ObservableCollection<QRE_90_30_7_S> _90_DATA { get; set; } = new ObservableCollection<QRE_90_30_7_S>();
        public ObservableCollection<QRE_90_30_7_S> _30_DATA { get; set; } = new ObservableCollection<QRE_90_30_7_S>();
        public ObservableCollection<QRE_90_30_7_S> _7_DATA { get; set; } = new ObservableCollection<QRE_90_30_7_S>();

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

        public NABZEMALI()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Owner = PublicVRB.WINBASE;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");create or alter
            Process Prc = ProcLoader.Start();
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

            string hesnaghd = Baseknow.hesnaghd;

            dbms.DoExecuteSQL($"IF EXISTS (SELECT * FROM sys.views WHERE name = 'nabz_mali7') DROP VIEW nabz_mali7");
            dbms.DoExecuteSQL($"CREATE VIEW nabz_mali7 AS SELECT TOP(7) DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab FROM DEED_DTL INNER JOIN DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S WHERE (DEED_DTL.HES = N'{hesnaghd}') GROUP BY DEED_HED.DATE_S ORDER BY DEED_HED.DATE_S DESC");

            dbms.DoExecuteSQL($"IF EXISTS (SELECT * FROM sys.views WHERE name = 'nabz_mali30') DROP VIEW nabz_mali30");
            dbms.DoExecuteSQL($"CREATE VIEW nabz_mali30 AS SELECT TOP(30) DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab, dbo.Uday(dbo.DEED_HED.DATE_S) AS dd, dbo.Umonth(dbo.DEED_HED.DATE_S) AS mm FROM DEED_DTL INNER JOIN DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S WHERE (DEED_DTL.HES = N'{hesnaghd}') GROUP BY DEED_HED.DATE_S, dbo.Uday(dbo.DEED_HED.DATE_S), dbo.Umonth(dbo.DEED_HED.DATE_S) ORDER BY DEED_HED.DATE_S DESC");

            dbms.DoExecuteSQL($"IF EXISTS (SELECT * FROM sys.views WHERE name = 'nabz_mali90') DROP VIEW nabz_mali90");
            dbms.DoExecuteSQL($"CREATE VIEW nabz_mali90 AS SELECT TOP(90) DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab, dbo.Uday(dbo.DEED_HED.DATE_S) AS dd, dbo.Umonth(dbo.DEED_HED.DATE_S) AS mm FROM DEED_DTL INNER JOIN DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S WHERE (DEED_DTL.HES = N'{hesnaghd}') GROUP BY DEED_HED.DATE_S, dbo.Uday(dbo.DEED_HED.DATE_S), dbo.Umonth(dbo.DEED_HED.DATE_S) ORDER BY DEED_HED.DATE_S DESC");

            var mali90 = dbms.DoGetDataSQL<QRE_90_30_MALI>($@"SELECT TOP(90)DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab, dbo.Uday(dbo.DEED_HED.DATE_S) AS dd, dbo.Umonth(dbo.DEED_HED.DATE_S) AS mm
                                                                       FROM DEED_DTL
                                                                            INNER JOIN DEED_HED ON DEED_DTL.N_S=DEED_HED.N_S
                                                                       WHERE(DEED_DTL.HES=N'{Baseknow.hesnaghd}')
                                                                       GROUP BY DEED_HED.DATE_S, dbo.Uday(dbo.DEED_HED.DATE_S), dbo.Umonth(dbo.DEED_HED.DATE_S)
                                                                       ORDER BY DEED_HED.DATE_S DESC").ToList();

            var mali30 = dbms.DoGetDataSQL<QRE_90_30_MALI>($@"SELECT     top(30) DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab, dbo.Uday(dbo.DEED_HED.DATE_S) AS dd, dbo.Umonth(dbo.DEED_HED.DATE_S) AS mm
                                                                       FROM         DEED_DTL INNER JOIN
                                                                                             DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S
                                                                       WHERE     (DEED_DTL.HES = N'{Baseknow.hesnaghd}')
                                                                       GROUP BY DEED_HED.DATE_S, dbo.Uday(dbo.DEED_HED.DATE_S), dbo.Umonth(dbo.DEED_HED.DATE_S)
                                                                       ORDER BY DEED_HED.DATE_S DESC").ToList();

            var mali7 = dbms.DoGetDataSQL<QRE_7_MALI>($@"SELECT     top(7) DEED_HED.DATE_S, SUM(DEED_DTL.BED) AS mab
                                                                      FROM         DEED_DTL INNER JOIN
                                                                                            DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S
                                                                      WHERE     (DEED_DTL.HES = N'{Baseknow.hesnaghd}')
                                                                      GROUP BY DEED_HED.DATE_S
                                                                      ORDER BY DEED_HED.DATE_S DESC").ToList();

            var _90M = dbms.DoGetDataSQL<QRE_90_30_7_S>("SELECT TOP(90) SUM(MABL) AS MAB, dbo.Uday(DATE) AS dd, dbo.Umonth(DATE) AS mm, DATE FROM PAY_GETD GROUP BY dbo.Uday(DATE), dbo.Umonth(DATE), DATE ORDER BY DATE DESC").ToList();
            var _30M = dbms.DoGetDataSQL<QRE_90_30_7_S>("SELECT TOP(30) SUM(MABL) AS MAB, dbo.Uday(DATE) AS dd, dbo.Umonth(DATE) AS mm, DATE FROM PAY_GETD GROUP BY dbo.Uday(DATE), dbo.Umonth(DATE), DATE ORDER BY DATE DESC").ToList();
            var _7M = dbms.DoGetDataSQL<QRE_90_30_7_S>("SELECT TOP(7) SUM(MABL) AS MAB, dbo.Uday(DATE) AS dd, dbo.Umonth(DATE) AS mm, DATE FROM PAY_GETD GROUP BY dbo.Uday(DATE), dbo.Umonth(DATE), DATE ORDER BY DATE DESC").ToList();


            foreach (var item in mali90) MALI_90_DATA.Add(new QRE_90_30_MALI { DATE_S = item.DATE_S, mab = item.mab, mm = item.mm, dd = item.dd });
            foreach (var item in mali30) MALI_30_DATA.Add(new QRE_90_30_MALI { DATE_S = item.DATE_S, mab = item.mab, mm = item.mm, dd = item.dd });

            foreach (var item in mali7) MALI_7_DATA.Add(new QRE_7_MALI { DATE_S = item.DATE_S, mab = item.mab });

            foreach (var item in _90M) _90_DATA.Add(new QRE_90_30_7_S { MAB = item.MAB, dd = item.dd, mm = item.mm, DATE = item.DATE });
            foreach (var item in _30M) _30_DATA.Add(new QRE_90_30_7_S { MAB = item.MAB, dd = item.dd, mm = item.mm, DATE = item.DATE });
            foreach (var item in _7M) _7_DATA.Add(new QRE_90_30_7_S { MAB = item.MAB, dd = item.dd, mm = item.mm, DATE = item.DATE });
            ProcLoader.Stop(Prc);
        }
    }
}
