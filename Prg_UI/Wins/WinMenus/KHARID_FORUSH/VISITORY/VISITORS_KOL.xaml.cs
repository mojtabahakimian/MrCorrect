using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.UiTools;
using Syncfusion.ProjIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for VISITORS_KOL.xaml
    /// </summary>
    public partial class VISITORS_KOL : Window
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

        public VISITORS_KOL(string dT1, string dT2, string cUST_NO)
        {
            InitializeComponent();

            DT1 = dT1;
            DT2 = dT2;

            CUST_NO = cUST_NO;

            DataContext = this;
        }

        public string Condition { get; private set; } = "";
        public string DT1 { get; private set; }
        public string DT2 { get; private set; }
        public string CUST_NO { get; private set; }

        public class Visitor : INotifyPropertyChanged
        {
            public string CUST_NO { get; set; }
            public string NAME { get; set; }

            public double? mabpur { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? TAKHF { get; set; }
            public double? MBAA { get; set; }
            public double? MABMAR { get; set; }
            public double? GHABEL { get; set; }
            public string? ADDRESS { get; set; }
            public string? TEL { get; set; }
            public string? TOZIH { get; set; }
            public string? MOBILE { get; set; }
            public int? tedad { get; set; }
            public double Darsad => (GHABEL ?? 0) != 0 ? ((mabpur ?? 0) / (GHABEL ?? 0) * 100.0) : 0;
            public double MablaghBaTakhfif => (GHABEL ?? 0) - (MBAA ?? 0);


            private ObservableCollection<Customer> _customers = new();
            public ObservableCollection<Customer> Customers
            {
                get => _customers;
                set
                {
                    _customers = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Customers)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class Customer : INotifyPropertyChanged
        {
            public string CUST_NO { get; set; }         
            public string CUSTNAME { get; set; }        
            public string CUSTOMER { get; set; }        
            public double? mabpur { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? TAKHF { get; set; }
            public double? MBAA { get; set; }
            public double? MABMAR { get; set; }
            public double? GHABEL { get; set; }
            public string? VISNAME { get; set; }           
            public string? TOZIH { get; set; }             
            public string? ADDRESS { get; set; }           
            public string? TEL { get; set; }               
            public int? TDF { get; set; }                  
            public string? MOBILE { get; set; }            
            public string? ROUTE_NAME { get; set; }        
            public int? OSTANID { get; set; }              
            public int? SHAHRID { get; set; }
            public double Darsad => (GHABEL ?? 0) != 0 ? ((mabpur ?? 0) / (GHABEL ?? 0) * 100.0) : 0;
            public double MablaghBaTakhfif => (GHABEL ?? 0) - (MBAA ?? 0);
            // سایر فیلدها

            private ObservableCollection<Invoice> _invoices = new();
            public ObservableCollection<Invoice> Invoices
            {
                get => _invoices;
                set
                {
                    _invoices = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Invoices)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class Invoice
        {
            public int NUMBER { get; set; }
            public long DATE_N { get; set; }
            public double? mabpur { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? TAKHF { get; set; }
            public double? MBAA { get; set; }
            public double? MABMAR { get; set; }
            public double? GHABEL { get; set; }
            public string? VISNAME { get; set; }       
            public string? TOZIH { get; set; }         
            public string? CUSTNAME { get; set; }      
            public string? ADDRESS { get; set; }       
            public string? TEL { get; set; }           
            public string? CUSTOMER { get; set; }      
            public string? MOBILE { get; set; }
            public double Darsad => (GHABEL ?? 0) != 0 ? ((mabpur ?? 0) / (GHABEL ?? 0) * 100.0) : 0;
            public double MablaghBaTakhfif => (GHABEL ?? 0) - (MBAA ?? 0);

            // … سایر فیلدها …
        }


        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        private static bool IsNull(object p)
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


        UniversControl universControl = new UniversControl();

        public Visual I_AM_VISITORS_KOL { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CL_HESABDARI.LETSGO("DEPEMAL"))
            {
                if (Condition == "")
                {
                    Condition = " WHERE (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
                else
                {
                    Condition = Condition + " AND  (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }

            }
            // پارامترهای تاریخ و فیلتر را از UI بگیرید
            var visitors = dbms.DoGetDataSQL<Visitor>("SELECT * FROM dbo.VISITORS_KOL(@start, @end, @filter)@condition", new { start = DT1, end = DT2, filter = CUST_NO , condition = Condition }).ToList();
            this.VisitorsGrid.ItemsSource = visitors;
        }

        private void VisitorsGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var visitor = (Visitor)e.Row.Item;
            // اگر قبلاً بارگذاری نشده
            if (visitor.Customers.Count == 0)
            {
                var sql = @"
                    SELECT *
                    FROM 
                    (
                        SELECT 
                            vdt.CUST_NO,
                            SUM(vdt.mabpur) AS mabpur,
                            SUM(jfv.MABL_K) AS MABL_K,
                            SUM(jfv.MEGHk) AS MEGHk,
                            SUM(jfv.MEGH_MAR) AS MEGH_MAR,
                            SUM(jfv.N_MOIN) AS TAKHF,
                            SUM(jfv.IMBAA) AS MBAA,
                            SUM(jfv.MABMAR) AS MABMAR,
                            SUM(jfv.GHABEL) AS GHABEL,
                            ch.NAME AS VISNAME,
                            ch.TOZIH,
                            ch1.NAME AS CUSTNAME,
                            ch1.ADDRESS,
                            ch1.TEL,
                            jfv.CUST_NO AS CUSTOMER,
                            COUNT(jfv.NUMBER) AS TDF,
                            ch1.MOBILE,
                            ch1.ROUTE_NAME,
                            ch1.OSTANID,
                            ch1.SHAHRID
                        FROM dbo.VISITOR_DTLO vdt
                        INNER JOIN dbo.CUST_HESAB ch
                            ON vdt.CUST_NO = ch.hes
                        INNER JOIN dbo.JAMFACTVISIT(@startDate, @endDate) jfv
                            ON vdt.NUMBER = jfv.NUMBER
                            AND vdt.TAG = jfv.TAG
                        INNER JOIN dbo.CUST_HESAB ch1
                            ON jfv.CUST_NO = ch1.hes
                        GROUP BY 
                            vdt.CUST_NO,
                            ch.NAME,
                            ch.TOZIH,
                            ch1.NAME,
                            ch1.ADDRESS,
                            ch1.TEL,
                            jfv.CUST_NO,
                            ch1.MOBILE,
                            ch1.ROUTE_NAME,
                            ch1.OSTANID,
                            ch1.SHAHRID
                    ) AS DRVD_TBL
                    WHERE (CUST_NO = @visitorId)
                    ";
                var customers = dbms.DoGetDataSQL<Customer>(sql, new { startDate = DT1, endDate = DT2, visitorId = visitor.CUST_NO }).ToList();

                visitor.Customers = new ObservableCollection<Customer>(customers);

                VisitorsGrid.Dispatcher.Invoke(() =>
                {
                    var row = (DataGridRow)VisitorsGrid.ItemContainerGenerator.ContainerFromItem(visitor);
                    if (row != null)
                    {
                        VisitorsGrid.UpdateLayout();
                        VisitorsGrid.ScrollIntoView(visitor);
                        row.DetailsVisibility = Visibility.Collapsed;
                        row.DetailsVisibility = Visibility.Visible;
                    }
                });
            }
        }

        private void CustomersGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var customer = (Customer)e.Row.Item;
            // اگر قبلاً بارگذاری نشده
            if (customer.Invoices.Count == 0)
            {
                var sql = @"
                    SELECT *
                    FROM (
                        SELECT
                            vdt.CUST_NO,
                            SUM(vdt.mabpur)     AS mabpur,
                            SUM(jfv.MABL_K)     AS MABL_K,
                            SUM(jfv.MEGHk)      AS MEGHk,
                            SUM(jfv.MEGH_MAR)   AS MEGH_MAR,
                            SUM(jfv.N_MOIN)     AS TAKHF,
                            SUM(jfv.IMBAA)      AS MBAA,
                            SUM(jfv.MABMAR)     AS MABMAR,
                            SUM(jfv.GHABEL)     AS GHABEL,
                            ch.NAME             AS VISNAME,
                            ch.TOZIH,
                            ch1.NAME            AS CUSTNAME,
                            ch1.ADDRESS,
                            ch1.TEL,
                            jfv.CUST_NO         AS CUSTOMER,
                            jfv.NUMBER,
                            jfv.DATE_N,
                            ch1.MOBILE
                        FROM dbo.VISITOR_DTLO AS vdt
                        INNER JOIN dbo.CUST_HESAB AS ch
                            ON vdt.CUST_NO = ch.hes
                        INNER JOIN dbo.JAMFACTVISIT(@startDate, @endDate) AS jfv
                            ON vdt.NUMBER = jfv.NUMBER
                           AND vdt.TAG    = jfv.TAG
                        INNER JOIN dbo.CUST_HESAB AS ch1
                            ON jfv.CUST_NO = ch1.hes
                        GROUP BY
                            vdt.CUST_NO,
                            ch.NAME, ch.TOZIH,
                            ch1.NAME, ch1.ADDRESS, ch1.TEL,
                            jfv.CUST_NO, jfv.NUMBER, jfv.DATE_N,
                            ch1.MOBILE
                    ) AS DRVD_TBL
                    WHERE DRVD_TBL.CUST_NO   = @visitorId
                      AND DRVD_TBL.CUSTOMER  = @customerId;
                    ";
                var invoices = dbms.DoGetDataSQL<Invoice>(sql, new { startDate = DT1, endDate = DT2, visitorId = customer.CUST_NO, customerId = customer.CUSTOMER }).ToList();

                customer.Invoices = new ObservableCollection<Invoice>(invoices);
            }
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataItem = button?.Tag;
            if (dataItem == null)
                return;

            var row = (DataGridRow)VisitorsGrid.ItemContainerGenerator.ContainerFromItem(dataItem);

            if (row != null)
            {
                if (row.DetailsVisibility == Visibility.Collapsed)
                    row.DetailsVisibility = Visibility.Visible;
                else
                    row.DetailsVisibility = Visibility.Collapsed;
            }
        }

        private void ExpandButton_Click_Customers(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataItem = button?.Tag;
            if (dataItem == null)
                return;

            // پیدا کردن DataGrid والد (CustomersGrid) از طریق VisualTree
            DependencyObject parent = button;
            DataGrid customersGrid = null;
            while (parent != null && customersGrid == null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                customersGrid = parent as DataGrid;
            }
            if (customersGrid == null)
                return;

            var row = (DataGridRow)customersGrid.ItemContainerGenerator.ContainerFromItem(dataItem);
            if (row != null)
            {
                if (row.DetailsVisibility == Visibility.Collapsed)
                    row.DetailsVisibility = Visibility.Visible;
                else
                    row.DetailsVisibility = Visibility.Collapsed;
            }
        }

    }
}
