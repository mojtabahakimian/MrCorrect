using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Wins.WinMenus.HESABDARI
{
    public partial class froosh_customer : Window
    {
        public class _model_froosh_customer
        {
            public double? TAG { get; set; }
            public long? DATE_N { get; set; }
            public string? CUST_NO { get; set; }
            public string? NAME { get; set; }
            public double? MEGHk { get; set; }
            public double? MABL_K { get; set; }
            public double? NUMBER { get; set; }
            public string? customer { get; set; }
            public double? MEGH { get; set; }
            public string? VAHED { get; set; }
            public double? MABL { get; set; }
            public double? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
        }
        public froosh_customer(string _Cust_No_)
        {
            InitializeComponent();

            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

            var DATA = dbms.DoGetDataSQL<_model_froosh_customer>($@" SELECT TOP 100 PERCENT HEAD_LST.TAG, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, STUF_DEF.NAME, INVO_LST.MEGHk, INVO_LST.MABL_K, HEAD_LST.NUMBER, CUST_HESAB.NAME AS customer, INVO_LST.MEGH, TCOD_VAHEDS.NAMES AS VAHED, INVO_LST.MABL, INVO_LST.N_KOL, INVO_LST.N_MOIN
                                                                     FROM HEAD_LST
                                                                          INNER JOIN INVO_LST ON HEAD_LST.NUMBER=INVO_LST.NUMBER AND HEAD_LST.TAG=INVO_LST.TAG
                                                                          INNER JOIN STUF_DEF ON INVO_LST.CODE=STUF_DEF.CODE
                                                                          INNER JOIN CUST_HESAB ON HEAD_LST.CUST_NO=CUST_HESAB.hes
                                                                          INNER JOIN TCOD_VAHEDS ON INVO_LST.VAHED_K=TCOD_VAHEDS.CODE
                                                                     WHERE(HEAD_LST.CUST_NO=N'{_Cust_No_}')
                                                                     ORDER BY HEAD_LST.DATE_N DESC").ToList();
            SaleHistory.ItemsSource = DATA;
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
    }
}
