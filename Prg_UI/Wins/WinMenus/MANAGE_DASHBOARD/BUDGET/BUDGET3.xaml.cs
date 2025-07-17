using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Rpts;
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

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    public partial class BUDGET3 : Window
    {
        public class RSTFields
        {
            public int HES_K { get; set; }
        }
        public BUDGET3()
        {
            InitializeComponent();
            Owner = PublicVRB.BUDG_1;

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
        private void lbl_budget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Command4_Click(object sender, RoutedEventArgs e)
        {
            WinReport winrpt = new WinReport(); winrpt.ShowDialog();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            long BGIDD;
            string MAHLIST;
            int XXD;
            long XXM = 0;
            int XXS;
            int MH;
            int mah;
            int SAL;
            string SH;
            int i;
            int TEDAD;
            long XXI;
            string rptname;
            double NAGHDINEGI = 0;
            //  DoCmd.Maximize
            //  BGIDD = 1
            BGIDD = PublicVRB.GenetalBGID == null ? 1 : PublicVRB.GenetalBGID;

            //Chang

            //this.RecordSource = "SELECT * FROM BUGET_MAIN WHERE BGID = " + BGIDD;
            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + BGIDD + "").ToList();

            // RST.Open("SELECT * FROM BUGET_MAIN WHERE BGID = " + BGIDD, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            XXD = Convert.ToInt32(PublicVRB.UDayOfBGDATEofBGID);
            foreach (var item in quer_RST)
            {

                XXM = Convert.ToInt64(CL_HESABDARI.UMonth(item.BGCDATE.ToString()));
            }
            mah = Convert.ToInt32(PublicVRB.BGMAH);
            SAL = Convert.ToInt32(PublicVRB.BGSAL);
            XXS = Convert.ToInt32(PublicVRB.UYearOfBGDATEofBGID);
            XXI = System.Convert.ToInt32(SAL - XXS) * 12 + mah - XXM;

            //var res1 = typeof(BUGET_MAIN).GetProperties()
            //                 .Select(property => property.Name)
            //                 .Where(property => property.Contains("BGMBCHEK" + i1))
            //                 .First();

            double BGMBCHECK0_12 = 0;
            for (i = 0; i <= XXI; i++)
            {
                //#Cool
                //var result = quer_RST.Select(s => new { Line = i++, Nav = "BGMBCHEK" + i }).FirstOrDefault();

                foreach (dynamic item in quer_RST)
                {
                    var BGMBCHEK_Wich = (item.GetType().GetProperty("BGMBCHEK" + i).GetValue(item));
                    BGMBCHECK0_12 = Convert.ToDouble(BGMBCHEK_Wich);
                }
                // LAST POINT BUG 3
                //var BGMBCHECK0_12 = dbms.DoGetDataSQL<RSTFields>("BGMBCHEK " + i);
                NAGHDINEGI = Convert.ToDouble(NAGHDINEGI) + Convert.ToDouble(BGMBCHECK0_12);
            }

            double BGMBCASH = 0;
            double BGMBDARAM = 0;
            double BGMBCHEKMON = 0;
            foreach (var item in quer_RST)
            {
                BGMBCASH = item.BGMBCASH;
                BGMBDARAM = item.BGMBDARAM;
                BGMBCHEKMON = item.BGMBCHEKMON;
            }
            //NAGHDINEGI = System.Convert.ToDouble(Round(NAGHDINEGI + RST.Fields("BGMBCASH") + RST.Fields("BGMBDARAM") + RST.Fields("BGMBCHEKMON")));
            NAGHDINEGI = System.Convert.ToDouble(Math.Round(NAGHDINEGI + BGMBCASH + BGMBDARAM + BGMBCHEKMON, 0));
            var quer_RSTUpdt = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGBUGETMON = " + NAGHDINEGI.ToString() + " WHERE BGID = " + BGIDD + "");
            LL1.Text = "مجموع نقدینگی کل شما تو ماه " + PublicVRB.MAHNAME(mah.ToString()) + " مبلغ " + Strings.Format(NAGHDINEGI, "#,###") + " هست برای مشاهده جزئیات گزینه جزئیات رو بزنید";

            MB.Text = NAGHDINEGI.ToString("N0") + " ریال";
            //}
            //catch (Exception e1)
            //{

            //    throw;
            //}
            //UiRefresher.RefWhole(mygrid);
            //(PublicVRB.BUDG_3).UpdateLayout();
        }
        private void Command20_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
