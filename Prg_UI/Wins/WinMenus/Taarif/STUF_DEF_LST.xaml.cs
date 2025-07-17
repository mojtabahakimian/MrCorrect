using MaterialDesignThemes.Wpf;
using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Wins.WinMenus.Taarif.STUF_DEF_WIN;

namespace Wins.WinMenus.Taarif
{
    public partial class STUF_DEF_LST : Window
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
        public STUF_DEF_LST()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public List<STUF_DEF> MASTER_DATA { get; set; } = new List<STUF_DEF>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();
            RLOADDATA();
            CODE.Focus();
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //واحد
            VAHED.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //گروه کالا
            RADAH.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_3>("SELECT TCOD_STUFGROUP.CODE, TCOD_STUFGROUP.NAMES FROM TCOD_STUFGROUP WHERE (((TCOD_STUFGROUP.CODE)<>0)) ORDER BY TCOD_STUFGROUP.NAMES").ToList();

            //گروه قیمتی
            PGID.ItemsSource = dbms.DoGetDataSQL<_KALA_QRE_4>("SELECT PGID, PGNAME FROM PRICE_GRP").ToList();

            //زیر مجموعه منو
            MENUIT.ItemsSource = dbms.DoGetDataSQL<TCODE_MENUITEM>("SELECT CODE, NAMES FROM dbo.TCODE_MENUITEM").ToList();

            //واحد فرعی
            VAHED.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //واحد مودیان
            mu.ItemsSource = dbms.DoGetDataSQL<TCOD_VAHED_EXTENDED>("SELECT IDD, NAME_MO FROM TCOD_VAHED_EXTENDED ORDER BY NAME_MO").ToList();
        }
        private void RLOADDATA()
        {
            var allDetails = dbms.DoGetDataSQL<STUF_DEF>("SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra FROM dbo.STUF_DEF").ToList();

            MASTER_DATA.Clear();
            foreach (var item in allDetails)
            {
                MASTER_DATA.Add(item);
            }
            MasterDataGrid.ItemsSource = MASTER_DATA;
        }

        private void CLEANFILTERS_Click(object sender, RoutedEventArgs e)
        {
            VAHED.SelectedIndex = -1; VAHED.Items.Refresh(); // واحد
            RADAH.SelectedIndex = -1; RADAH.Items.Refresh(); //گروه کالا
            PGID.SelectedIndex = -1; PGID.Items.Refresh(); //گروه قیمت گذاری
            MENUIT.SelectedIndex = -1; MENUIT.Items.Refresh(); //گروه قیمت گذاری

            CMBAA.IsChecked = false; // مشمول مالیات ب.ا.ا

            mu.SelectedIndex = -1; //واحد مودیان

            CODE.Text = null; // کد کالا
            NAM.Text = null; //نام کالا
            N_FANI.Text = null; //شماره فنی
            Barcode.Text = null; //بارکد
            TOZIH.Text = null; //توضیح
            sstid.Text = null; //شناسه کالا
            N_SEF.Text = "0"; //نقطه سفارش
            MABL_F.Text = "0"; //فی عمده فروش
            B_SEF.Text = "0"; //فی خرده فروش
            MAX_M.Text = "0"; //قیمیت مصرف کننده
            vra.Text = "0"; //درصد مودیان

            MIN_M.Text = "0";
            VAZN.Text = "0";

            MasterDataGrid.ItemsSource = MASTER_DATA;
        }
        private void REFLOAD_ALL_Click(object sender, RoutedEventArgs e)
        {
            RLOADDATA();
        }

        private void BTN_SEARCH_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<STUF_DEF> LINQ_SEARCH = MASTER_DATA;

            // --- Get Filter Values ---
            var _VAHED_ = VAHED.SelectedValue;
            var _RADAH_ = RADAH.SelectedValue;
            var _PGID_ = PGID.SelectedValue;
            var _mu_ = mu.SelectedValue;
            var _CMBAA_ = CMBAA.IsChecked;

            // --- Process Input Strings Consistently ---
            // CODE: Trim, Remove Invisibles, Fix Chars (Consider if ToLower is needed)
            // Use Equals for CODE comparison later
            string _CODE_Input = CODE.Text?.Trim().RemoveInvisibleChars().FixPersianChars();

            // NAME: Trim, Remove Invisibles, ToLower, Fix Chars
            string _NAM_Input = NAM.Text?.Trim().RemoveInvisibleChars().ToLowerInvariant().FixPersianChars();

            // N_FANI: Trim, Remove Invisibles (Consider ToLower/FixChars if needed)
            string _N_FANI_Input = N_FANI.Text?.Trim().RemoveInvisibleChars();

            // BARCODE: Trim, Remove Invisibles
            string _Barcode_Input = Barcode.Text?.Trim().RemoveInvisibleChars();

            // TOZIH: Trim, Remove Invisibles, ToLower, Fix Chars
            string _TOZIH_Input = TOZIH.Text?.Trim().RemoveInvisibleChars().ToLowerInvariant().FixPersianChars();

            // sstid: Trim, Remove Invisibles
            string _sstid_Input = sstid.Text?.Trim().RemoveInvisibleChars();


            // --- Parse Numeric Fields Safely ---
            // (Using the improved parsing logic from the previous answer)
            double? _N_SEF_Value = null;
            if (double.TryParse(N_SEF.Text?.Replace(",", ""), out double nsef)) _N_SEF_Value = nsef;

            double? _MABL_F_Value = null;
            if (double.TryParse(MABL_F.Text?.Replace(",", ""), out double mablf)) _MABL_F_Value = mablf;

            double? _B_SEF_Value = null;
            if (double.TryParse(B_SEF.Text?.Replace(",", ""), out double bsef)) _B_SEF_Value = bsef;

            double? _MAX_M_Value = null;
            if (double.TryParse(MAX_M.Text?.Replace(",", ""), out double maxm)) _MAX_M_Value = maxm;

            double? _MIN_M_Value = null;
            if (double.TryParse(MIN_M.Text?.Replace(",", ""), out double minm)) _MIN_M_Value = minm;

            double? _VAZN_Value = null;
            if (double.TryParse(VAZN.Text?.Replace(",", ""), out double vazn)) _VAZN_Value = vazn;

            double? _vra_Value = null;
            if (double.TryParse(vra.Text?.Replace(",", ""), out double vraVal)) _vra_Value = vraVal;


            // --- Apply Filters ---

            // ComboBox Filters
            if (_VAHED_ != null) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.VAHED.Equals(_VAHED_));
            if (_RADAH_ != null) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.RADAH.Equals(_RADAH_)); // Ensure RADAH type matches SelectedValue
            if (_PGID_ != null) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.PGID.Equals(_PGID_)); // Ensure PGID type matches SelectedValue & handle nulls
            if (_mu_ != null) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.mu.Equals(_mu_)); // Ensure mu type matches SelectedValue & handle nulls
            if (_CMBAA_ == true) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.CMBAA == true); // Filter only when checked

            // CODE Filter (Exact Match Recommended)
            if (!string.IsNullOrEmpty(_CODE_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.CODE != null &&
                    x.CODE.Trim() // Trim data
                          .RemoveInvisibleChars() // Clean data
                          .FixPersianChars() // Normalize data
                          .Contains(_CODE_Input, StringComparison.OrdinalIgnoreCase)); // Case-insensitive exact match
            }

            // NAME Filter (Contains - Apply full cleaning to data)
            if (!string.IsNullOrEmpty(_NAM_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.NAME != null &&
                    x.NAME.Trim() // Trim data
                          .RemoveInvisibleChars() // Clean data
                          .ToLowerInvariant() // Lowercase data
                          .FixPersianChars() // Normalize data
                          .Contains(_NAM_Input)); // Compare with cleaned input
            }

            // N_FANI Filter (Contains - Apply relevant cleaning)
            if (!string.IsNullOrEmpty(_N_FANI_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.N_FANI != null &&
                    x.N_FANI.Trim() // Trim data
                              .RemoveInvisibleChars() // Clean data
                                                      // Add .ToLowerInvariant().FixPersianChars() if needed for N_FANI
                              .Contains(_N_FANI_Input));
            }

            // BARCODE Filter (Contains - Apply relevant cleaning)
            if (!string.IsNullOrEmpty(_Barcode_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.BARCODE != null &&
                    x.BARCODE.Trim() // Trim data
                                 .RemoveInvisibleChars() // Clean data
                                 .Contains(_Barcode_Input));
            }

            // TOZIH Filter (Contains - Apply full cleaning to data)
            if (!string.IsNullOrEmpty(_TOZIH_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.TOZIH != null &&
                    x.TOZIH.Trim() // Trim data
                             .RemoveInvisibleChars() // Clean data
                             .ToLowerInvariant() // Lowercase data
                             .FixPersianChars() // Normalize data
                             .Contains(_TOZIH_Input)); // Compare with cleaned input
            }

            // sstid Filter (Contains - Apply relevant cleaning)
            if (!string.IsNullOrEmpty(_sstid_Input))
            {
                LINQ_SEARCH = LINQ_SEARCH.Where(x =>
                    x.sstid != null &&
                    x.sstid.Trim() // Trim data
                             .RemoveInvisibleChars() // Clean data
                             .Contains(_sstid_Input));
            }


            // --- Numeric Filters ---
            // (Assuming 0 means "don't filter", adjust if 0 is a valid search criteria)
            if (_N_SEF_Value.HasValue && _N_SEF_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.N_SEF == _N_SEF_Value.Value);
            if (_MABL_F_Value.HasValue && _MABL_F_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.MABL_F == _MABL_F_Value.Value);
            if (_B_SEF_Value.HasValue && _B_SEF_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.B_SEF == _B_SEF_Value.Value);
            if (_MAX_M_Value.HasValue && _MAX_M_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.MAX_M == _MAX_M_Value.Value);
            if (_MIN_M_Value.HasValue && _MIN_M_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.MIN_M == _MIN_M_Value.Value);
            if (_VAZN_Value.HasValue && _VAZN_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.VAZN == _VAZN_Value.Value); // Nullable double comparison
            if (_vra_Value.HasValue && _vra_Value != 0) LINQ_SEARCH = LINQ_SEARCH.Where(x => x.vra == _vra_Value.Value); // Nullable double comparison


            // --- Update DataGrid ---
            MasterDataGrid.ItemsSource = LINQ_SEARCH.ToList();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

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
                    OpenKalaWindow(e);
                }
                else
                {
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }
        private void OpenKalaWindow(KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                if (!(MasterDataGrid.SelectedItem is null))
                {

                    Process Prc = ProcLoader.Start();
                    var ROW = (STUF_DEF)MasterDataGrid.SelectedItem;

                    STUF_DEF_WIN hEAD_LST_HAVL = new STUF_DEF_WIN(Convert.ToDouble(ROW.CODE));
                    Close();
                    hEAD_LST_HAVL.Show();
                    ProcLoader.Stop(Prc);
                    return;

                }
            }
        }
    }
}
