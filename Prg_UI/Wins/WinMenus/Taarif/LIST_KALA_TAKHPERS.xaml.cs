using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Wins.WinMenus.Taarif.STUF_DEF_WIN;

namespace Wins.WinMenus.Taarif
{
    public partial class LIST_KALA_TAKHPERS : Window
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
        public LIST_KALA_TAKHPERS()
        {
            InitializeComponent();
        }
        public class LGT_SUB_MODEL
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }
            public string? N_FANI { get; set; }
            public string? TOZIH { get; set; }
            public int? VAHED { get; set; }
            public double? B_SEF { get; set; }
            public double? N_SEF { get; set; }
            public double? MIN_M { get; set; }
            public double? MAX_M { get; set; }
            public double? RADAH { get; set; }
            public short? KINDK { get; set; }
            public double? MABL_F { get; set; }
            public int? DEPART { get; set; }
            public int? IDD { get; set; }
            public bool? CMBAA { get; set; }
            public double? VAZN { get; set; }
            public bool? OKF { get; set; }
            public double? MENUIT { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public string? TAKH_COD { get; set; }
            public int? CUST_CO { get; set; }
            public short? TAFPER { get; set; }
            public int? PRICE_M { get; set; }
            public double? PERS { get; set; }
            public int? BLNS { get; set; }
            public bool? PUT { get; set; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //واحد
            VAHED_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
            //گروه کالا
            RADAH_COLUMN.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_3>("SELECT TCOD_STUFGROUP.CODE, TCOD_STUFGROUP.NAMES FROM TCOD_STUFGROUP WHERE (((TCOD_STUFGROUP.CODE)<>0)) ORDER BY TCOD_STUFGROUP.NAMES").ToList();
            //نوع کد مشتری
            CUST_CO_COLUMN.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM dbo.CUSTKIND").ToList();

            LGT_SUB.ItemsSource = dbms.DoGetDataSQL<LGT_SUB_MODEL>(@"SELECT     dbo.STUF_DEF.CODE, dbo.STUF_DEF.NAME, dbo.STUF_DEF.N_FANI, dbo.STUF_DEF.TOZIH, dbo.STUF_DEF.VAHED, dbo.STUF_DEF.B_SEF, 
                                                                                    dbo.STUF_DEF.N_SEF, dbo.STUF_DEF.MIN_M, dbo.STUF_DEF.MAX_M, dbo.STUF_DEF.RADAH, dbo.STUF_DEF.KINDK, dbo.STUF_DEF.MABL_F, 
                                                                                    dbo.STUF_DEF.DEPART, dbo.STUF_DEF.IDD, dbo.STUF_DEF.CMBAA, dbo.STUF_DEF.VAZN, dbo.STUF_DEF.OKF, dbo.STUF_DEF.MENUIT, 
                                                                                    dbo.STUF_DEF.MEGHTA, dbo.STUF_DEF.MEGHJAY, dbo.TAKHPERS.TAKH_COD, dbo.TAKHPERS.CUST_CO, dbo.TAKHPERS.TAFPER, 
                                                                                    dbo.TAKHPERS.PRICE_M, dbo.TAKHPERS.PERS, dbo.TAKHPERS.BLNS, dbo.TAKHPERS.PUT
                                                              FROM         dbo.STUF_DEF INNER JOIN
                                                                                    dbo.TAKHPERS ON dbo.STUF_DEF.CODE = dbo.TAKHPERS.TAKH_COD").ToList();
        }
    }
}
