using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.HESABDARI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for DEED_HEAD_LIST.xaml
    /// </summary>
    public partial class DEED_HEAD_LIST : Window
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
        public DEED_HEAD_LIST()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public List<DEED_HED> MASTER_DATA_DEED_HEAD { get; set; } = new List<DEED_HED>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            RLOADDATA();
            N_S.Focus();
        }

        private void RLOADDATA()
        {
            // Fetch all detail records from the database
            var allDetails = dbms.DoGetDataSQL<DEED_HED>("SELECT N_S, DATE_S, SHARH_S, NO_S, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, BAYEG FROM dbo.DEED_HED ORDER BY N_S DESC").ToList();

            // Clear the existing data
            MASTER_DATA_DEED_HEAD.Clear();
            foreach (var item in allDetails)
            {
                MASTER_DATA_DEED_HEAD.Add(item);
            }
            // Bind the master data list to the master DataGrid
            MasterDataGrid.ItemsSource = MASTER_DATA_DEED_HEAD;
        }

        private void MasterDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(MasterDataGrid.SelectedItem is null))
            {
                var HAVALEROW = (DEED_HED)MasterDataGrid.SelectedItem;

                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this, HAVALEROW.N_S);
                Close();
                return;
            }
        }

        private void CLEANFILTERS_Click(object sender, RoutedEventArgs e)
        {
            N_S.Clear();
            BASE.Clear();
            BAYEG.Clear();
            DATE_S.Clear();
            SHARH_S.Clear();

            MasterDataGrid.ItemsSource = MASTER_DATA_DEED_HEAD;
        }

        private void REFLOAD_ALL_Click(object sender, RoutedEventArgs e)
        {
            RLOADDATA();
        }

        private void BTN_SEARCH_Click(object sender, RoutedEventArgs e)
        {
            var N_S_FLD = N_S.Text.Trim();
            var BASE_FLD = BASE.Text.Trim();
            var BAYEG_FLD = BAYEG.Text.Trim();
            var DATE_S_FLD = DATE_S.Text.ToRawTarikh().Trim();
            var SHARH_S_FLD = SHARH_S.Text.Trim().ToLowerInvariant().FixPersianChars(); //ملاحظات

            IEnumerable<DEED_HED> LINQ_SEARCH = MASTER_DATA_DEED_HEAD;

            if (!string.IsNullOrEmpty(N_S_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => x.N_S.ToString().Contains(N_S_FLD));

            if (!string.IsNullOrEmpty(BASE_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => x.@base.ToString().Contains(BASE_FLD));

            if (!string.IsNullOrEmpty(BAYEG_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => x.BAYEG.ToString().Contains(BAYEG_FLD));

            if (!string.IsNullOrEmpty(DATE_S_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => x.DATE_S.ToString().Contains(DATE_S_FLD));

            if (!string.IsNullOrEmpty(SHARH_S_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.SHARH_S is null) && x.SHARH_S.ToString().ToLowerInvariant().FixPersianChars().Contains(SHARH_S_FLD));

            MasterDataGrid.ItemsSource = LINQ_SEARCH.ToList();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                if (N_S.IsFocused)
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
