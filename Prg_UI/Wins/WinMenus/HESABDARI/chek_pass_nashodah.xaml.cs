using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Wins.WinMenus.HESABDARI
{
    public partial class chek_pass_nashodah : Window
    {
        public class _model_chek_pass_nashodah
        {
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public string? SHOBEH { get; set; }
            public double? MABL { get; set; }
            public string? NAME_TAH { get; set; }
            public string? N_HESAB { get; set; }
            public string? CUST_NO { get; set; }
            public string? bkn { get; set; }
        }
        public chek_pass_nashodah(string _Cust_No_)
        {
            InitializeComponent();

            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

            var DATA = dbms.DoGetDataSQL<_model_chek_pass_nashodah>($@"SELECT PAY_GETD.N_SERI, PAY_GETD.BANK, PAY_GETD.DATE_S, PAY_GETD.DATE, PAY_GETD.SHOBEH, PAY_GETD.MABL, PAY_GETD.NAME_TAH, PAY_GETD.N_HESAB, PAY_GETD.CUST_NO, TCOD_BANKS.NAMES AS bkn
                                                                    FROM PAY_GETD
                                                                         INNER JOIN TCOD_BANKS ON PAY_GETD.BANK=TCOD_BANKS.CODE
                                                                    WHERE(PAY_GETD.N_KOL3 IS NULL)AND(PAY_GETD.N_KOL2 IS NULL)AND CUST_NO=N'{_Cust_No_}'
                                                                    ORDER BY DATE").ToList();
            none_passecheck.ItemsSource = DATA;
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
