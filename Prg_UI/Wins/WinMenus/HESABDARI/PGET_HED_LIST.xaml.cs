using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.HESABDARI
{
    public partial class PGET_HED_LIST : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

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

        public class LISTY_PGET_HED
        {
            public int? ID { get; set; }
            public ObservableCollection<PGET_LST> Details { get; set; }
            public long? DATE { get; set; }
            public string MOLAH { get; set; }
            public double? N_S { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string USER_NAME { get; set; }
            public short? KIND { get; set; }
            public int? IDK { get; set; }
            public bool? OKF { get; set; }
            public int? RPLICA { get; set; }
            public bool? SGN1 { get; set; }
            public bool? SGN2 { get; set; }
            public bool? SGN3 { get; set; }
            public int? sgn1usid { get; set; }
            public int? sgn2usid { get; set; }
            public int? sgn3usid { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public List<LISTY_PGET_HED> MASTER_DATA_PGET_HED { get; set; } = new List<LISTY_PGET_HED>();

        public PGET_HED_LIST()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            RLOADDATA();
            ID.Focus();
        }

        private void RLOADDATA()
        {
            // Fetch all header records from the database

            var hData = dbms.DoGetDataSQL<LISTY_PGET_HED>("SELECT ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3 FROM PGET_HED").ToList();

            if (!CL_HESABDARI.LETSGO("DPDEED")) //برای مشاهده خزانه استفاده میشود نه اینجا
            {
                hData = dbms.DoGetDataSQL<LISTY_PGET_HED>("SELECT ID, DATE, MOLAH, N_S, DEPATMAN, SHIFT, USER_NAME, KIND, IDK, OKF, RPLICA, SGN1, SGN2, SGN3 FROM PGET_HED WHERE (USER_NAME = N'" + CL_HESABDARI.UCurrentUser() + "') ORDER BY DATE, ID").ToList();
            }
           
          

            //// Fetch all detail records from the database
            //var allDetails = dbms.DoGetDataSQL<PGET_LST>("SELECT ID, DATE, RADIF, NO_AM, NAHVA, FHES, THES, SHARH, MABL, N_SERI, BANK, IDH, ARZD FROM PGET_LST").ToList();

            //// Clear the existing data
            //MASTER_DATA_PGET_HED.Clear();

            //// For each header record, find the corresponding detail records and add them to the header
            //foreach (var item in hData)
            //{
            //    var details = allDetails.Where(d => d.ID == item.ID && d.DATE == item.DATE).ToList();
            //    item.Details = new ObservableCollection<PGET_LST>(details);

            //    // Add the header record to the master data list
            //    MASTER_DATA_PGET_HED.Add(item);
            //}

            // Bind the master data list to the master DataGrid
            MasterDataGrid.ItemsSource = hData;
        }

        private void MasterDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(MasterDataGrid.SelectedItem is null))
            {
                Process Prc = ProcLoader.Start();
                var HAVALEROW = (LISTY_PGET_HED)MasterDataGrid.SelectedItem;
                Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED hEAD_LST_HAVL = new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED((double)HAVALEROW.ID);
                Close();
                hEAD_LST_HAVL.Show();
                ProcLoader.Stop(Prc);
                return;
            }
        }

        private void CLEANFILTERS_Click(object sender, RoutedEventArgs e)
        {
            ID.Clear();
            IDK.Clear();
            DATE.Clear();
            SHARAYET_SR.Clear();

            MasterDataGrid.ItemsSource = MASTER_DATA_PGET_HED;
        }

        private void REFLOAD_ALL_Click(object sender, RoutedEventArgs e)
        {
            RLOADDATA();
        }

        private void BTN_SEARCH_Click(object sender, RoutedEventArgs e)
        {
            var IDK_FLD = IDK.Text.Trim();
            var ID_FLD = ID.Text.Trim();
            var DATE_FLD = DATE.Text.ToRawTarikh().Trim();
            var SHARAYET_FLD = SHARAYET_SR.Text.Trim().ToLowerInvariant().FixPersianChars(); //ملاحظات

            IEnumerable<LISTY_PGET_HED> LINQ_SEARCH = MASTER_DATA_PGET_HED;

            if (!string.IsNullOrEmpty(IDK_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.IDK is null) && x.IDK.ToString().Contains(IDK_FLD));

            if (!string.IsNullOrEmpty(ID_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.ID is null) && x.ID.ToString().Contains(ID_FLD));

            if (!string.IsNullOrEmpty(DATE_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.DATE is null) && x.DATE.ToString().Contains(DATE_FLD));

            if (!string.IsNullOrEmpty(SHARAYET_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.MOLAH is null) && x.MOLAH.ToString().ToLowerInvariant().FixPersianChars().Contains(SHARAYET_FLD));

            MasterDataGrid.ItemsSource = LINQ_SEARCH.ToList();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (ID.IsFocused)
                {
                    CL_LMethods.SendKey_US(Key.Tab);
                }

                if (BTN_SEARCH.IsFocused)
                {
                    BTN_SEARCH_Click(null, null);
                }
                else
                {
                    CL_LMethods.SendKey_US(Key.Tab);
                }

                if (MasterDataGrid.IsKeyboardFocusWithin)
                {
                    MasterDataGrid_PreviewKeyDown(null, null);
                }
                else
                {
                    CL_LMethods.SendKey_US(Key.Tab);
                }

                
            }
        }
    }
}
