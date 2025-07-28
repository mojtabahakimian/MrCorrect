using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.BARNAME_RIZI
{
    /// <summary>
    /// Interaction logic for AMAR_FROOSH_KOL.xaml
    /// </summary>
    public partial class AMAR_FROOSH_KOL : Window
    {
        #region Header Window Begin
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
        #endregion
        public AMAR_FROOSH_KOL()
        {
            InitializeComponent();

            DataContext = this;

            LoadChartData();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //DETAIL_VOSUL_SUB.IsReadOnly = !ican;
            }
        }

        public static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public class SalesReportByMonth
        {
            public double MEGHT { get; set; }      // مجموع مقدار
            public int MON { get; set; }           // شماره ماه
            public string MN { get; set; }         // نام ماه (فارسی)
            public double MABL_K { get; set; }     // مبلغ

            // این پراپرتی برای نمایش لیبل فرمت شده اضافه شده است
            public string MABL_K_LABEL { get; set; }

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LoadChartData()
        {
            var query = @"SELECT SUM(INVO_LST.MEGHk) AS MEGHT, dbo.Umonth(HEAD_LST.DATE_N) AS MON, MON.MON AS MN, 
                     SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K
              FROM HEAD_LST 
              INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG 
              INNER JOIN STUF_DEF ON INVO_LST.CODE = STUF_DEF.CODE 
              INNER JOIN TCOD_STUFGROUP ON STUF_DEF.RADAH = TCOD_STUFGROUP.CODE 
              INNER JOIN TCOD_VAHEDS ON STUF_DEF.VAHED = TCOD_VAHEDS.CODE 
              RIGHT OUTER JOIN MON ON dbo.Umonth(HEAD_LST.DATE_N) = MON.MON_ID 
              WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = -1) 
              GROUP BY dbo.Umonth(HEAD_LST.DATE_N), MON.MON 
              ORDER BY dbo.Umonth(HEAD_LST.DATE_N)";

            var data = dbms.DoGetDataSQL<SalesReportByMonth>(query).ToList();
            salesChart.DataContext = data; // بایندینگ به نمودار
        }

        public class ThousandsSeparatorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null) return "";
                if (double.TryParse(value.ToString(), out double d))
                    return d.ToString("#,##0", CultureInfo.InvariantCulture);
                return value.ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
