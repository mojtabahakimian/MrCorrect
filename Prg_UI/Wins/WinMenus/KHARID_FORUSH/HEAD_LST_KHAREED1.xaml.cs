using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using Wins.WinMenus.ANBAR;
using Functions.SMSService;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using System.Windows.Data;
using Rpts;
using static Prg_UI.Functions.CL_LMethods;
using System.Windows.Controls.Primitives;
using Wins.WinOther;
using static Interfaces.INavigator;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class HEAD_LST_KHAREED1 : Window, ISearchableWindow
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

        #region LOCALMODEL
        public class NUMYS
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
        }
        public class DeedHedData
        {
            public string BASE { get; set; }
            public bool GHATEI { get; set; }
        }
        public class SignData
        {
            public bool KFR_BAZAR { get; set; }
            public bool KFR_HESAB { get; set; }
            public bool KFR_MODIR { get; set; }
        }
        public class CheckData
        {
            public double? N_SERI { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public long? DATE { get; set; }
            public long? DATE_S { get; set; }
            public double? MABL { get; set; }
            public int? NUMBER { get; set; }
            public int? TAG { get; set; }
        }

        public class HeadLstData
        {
            public int NUMBER { get; set; }
            public int htag { get; set; }
            public double MABL_HAZ { get; set; }
            public double MABL_VAR { get; set; }
            public double MABL_HAV { get; set; }
            public double M_NAGHD { get; set; }
            public double TAKHFIF { get; set; }
            public double MBAA { get; set; }
        }
        #endregion

        private NavigationManager<HEAD_LST> _navigationManager;
        public HEAD_LST_KHAREED1(double? number_to_open = null, bool _IsDirectFactor_ = true, bool _IsExporty_ = false, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER.Text = number_to_open.ToString(); //شماره رسید
                NUMBER.UpdateLayout();
                IsOpenedFromAutomation = _isAutomasion_;
            }

            IsDirectFactor = _IsDirectFactor_;

            IsExporty = _IsExporty_;
        }
        public bool IsOpenedFromAutomation { get; } = false;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله
        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<PAY_GETP_MODEL> PAY_GETP_SUB_DATA { get; set; } = new ObservableCollection<PAY_GETP_MODEL>();

        /// <summary>
        /// تگ هدر فاکتور خرید 12
        /// </summary>
        public byte FTAG { get; } = 12; //فاکتور

        /// <summary>
        /// تگ رسید انبار خرید 1 و سطر های اون
        /// </summary>
        public byte HTAG { get; } = 1; //برگه رسید

        public int? ANBAR { get; set; }

        private bool _isDirectFactor;
        public bool IsDirectFactor
        {
            get
            {
                return _isDirectFactor;
            }
            set
            {
                _isDirectFactor = value;
            }
        }

        private bool _isExporty;
        /// <summary>
        /// فاکتور خرید صادراتی
        /// </summary>
        public bool IsExporty
        {
            get { return _isExporty; }
            set
            {
                _isExporty = value;

                if (_isExporty)
                {
                    IsDirectFactor = false; //فاکتور های صادراتی از رسید حواله خارجی میاد

                    EXPORTY_GRID.Visibility = Visibility.Visible;
                    LBL_SUM_COUNT.Content = "جمع ارزی  :";
                    //Rows
                    N_TAF_COLUMN.Visibility = Visibility.Visible;
                    TOTALARZ_COLUMN.Visibility = Visibility.Visible;

                    PARAMS.Visibility = Visibility.Visible;
                }
                else
                {
                    EXPORTY_GRID.Visibility = Visibility.Hidden;
                    //Rows
                    N_TAF_COLUMN.Visibility = Visibility.Hidden;
                    TOTALARZ_COLUMN.Visibility = Visibility.Hidden;

                    PARAMS.Visibility = Visibility.Hidden;
                }
            }
        }

        private double _sum_of_mabl_k = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _sum_of_mabl_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MABL_K);
                if (_sum_of_mabl_k == 0) _sum_of_mabl_k = 0;
                return _sum_of_mabl_k;
            }
            set { _sum_of_mabl_k = value; }
        }

        private double sum_of_megh_k = 0;
        public double SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MEGHk);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        public class SGN_IMODEL
        {
            public string SEMAT_USER { get; set; }
            public string NAME_HESAB_USER { get; set; }
        }
        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "KFR_BAZARTX");
                    _sgn1_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN1usid.Tag)));
                }
                return _sgn1_info;
            }
        }
        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO
        {
            get
            {
                if (SGN2usid.Tag is not null)
                {
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "KFR_HESABTX");
                    _sgn2_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN2usid.Tag)));
                }
                return _sgn2_info;
            }
        }
        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO
        {
            get
            {
                if (SGN3usid.Tag is not null)
                {
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "KFR_MODIRTX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        List<COMBOPERSONEL> rst_personel = null;
        public bool NowIsReady { get; private set; }
        public bool INVO_LST_SUB_IsFocused { get; private set; }

        private bool _newrecord;
        public bool NewRecord
        {
            get
            {
                if (string.IsNullOrEmpty(NUMBER1.Text) || NUMBER1.Text == "0")
                {
                    _newrecord = true;
                }
                else
                {
                    _newrecord = false;
                }
                return _newrecord;
            }
            set { _newrecord = value; }
        }

        public long? CURRENT_ROW_INDEX { get; set; } = 0;
        public bool ChangeIsHappend { get; private set; } = false;

        private int datagridname_tbox_def_index_col;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (INVO_LST_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "CODE")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        datagridname_tbox_def_index_col = 0;
                    }
                    else
                    {
                        datagridname_tbox_def_index_col = (int)defaultcolumnindex;
                    }
                }
                return datagridname_tbox_def_index_col;
            }
        }
        public string? ENTERED_VALUE_ROW { get; private set; }
        public INVO_LST_FACTOR22? CURRENT_ITEMS_ROW { get; private set; }
        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; }
        public INVO_LST_FACTOR22 FROM_SEARCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;
        public int ANBARDefaultValue { get; private set; }

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

                //فاکتور
                DATE_N.IsReadOnly = !ican;// تاریخ
                MAS.IsReadOnly = !ican;// مدت
                NUMBER.IsReadOnly = !ican;// شماره حواله
                CUST_KIND.IsReadOnly = !ican;// نوع مشتری
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ
                SHIFT.IsReadOnly = !ican;// شیفت

                //__ENABLEY
                FNUMCO.IsEnabled = ican;
                DEPATMAN.IsEnabled = ican;

                DATE_N.IsEnabled = ican;// تاریخ
                MAS.IsEnabled = ican;// مدت
                NUMBER.IsEnabled = ican;// شماره حواله
                CUST_KIND.IsEnabled = ican;// نوع مشتری
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ
                SHIFT.IsEnabled = ican;// شیفت
                //فاکتور END
                //Page58.IsEnabled = ican;// تب پشت فاکتور

                BTN_SAVE.IsEnabled = ican;

                #region POSHT_FACTOR

                M_NAGHD.IsReadOnly = !ican;
                TAKHFIF.IsReadOnly = !ican;
                TAKHFIF_PERCENT.IsReadOnly = !ican;
                MABL_VAR.IsReadOnly = !ican;
                MABL_HAV.IsReadOnly = !ican;
                CMB_MOIN_HAV.IsReadOnly = !ican;

                MOIN_VAR.IsEnabled = ican;
                CMB_MOIN_VAR.IsEnabled = ican;
                MOIN_HAV.IsEnabled = ican;
                CMB_MOIN_HAV.IsEnabled = ican;
                MOIN_HAZ.IsEnabled = ican;
                CMB_MOIN_HAZ.IsEnabled = ican;
                MBAA.IsEnabled = ican;
                HMBAA.IsEnabled = ican;
                CMB_HMBAA.IsEnabled = ican;

                BUTTON_SAVE_POSHT.IsEnabled = ican;
                PAY_GETP_SUB.IsEnabled = ican;
                DELETE_CHKPOSHT.IsEnabled = ican;

                #endregion
            }
        }

        public double Meidnum { get; private set; }
        public Visual I_AM_KHAREED { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
            ChangeIsHappend = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_KHAREED = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            string WhereCondition = FTAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) " : "  ";
            _restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(FTAG, WhereCondition);
            WhereCondition = _restrictionInfo.WhereClause;

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";
            }

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
            x => $"SELECT TOP 1 * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {FTAG}", //On Change for One Record
            Convert.ToDouble(NUMBER.Text)
            );


            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;
            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            //Form_Open
            // if (Baseknow.OPTIONSS.Substring(67, 1) == "5" && !IsExporty)
            if (!string.IsNullOrEmpty(Baseknow.OPTIONSS) && Baseknow.OPTIONSS.Length > 67 && Baseknow.OPTIONSS.Substring(67, 1) == "5" && !IsExporty)
            {
                this.PARAMS.Visibility = Visibility.Visible;
            }
            else
            {
                this.PARAMS.Visibility = Visibility.Hidden;
            }

            Form_Current();

            CL_LMethods.SetTabIndexes(
             DATE_N,
             FNUMCO,
             NUMBER,
             CUST_KIND,
             CUST_NO,
             MOLAH,
             MAS,
             BTN_SAVE,
             INVO_LST_SUB
             );

            MakeDefaultFocuseReady();
        }

        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    ShowMissingOrRestrictedMessage();
                    return;
                }
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    //new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    ShowMissingOrRestrictedMessage();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                NUMBER1.Text = HEADER_FAC.NUMBER1.ToString();

                NUMBER.Text = HEADER_FAC.NUMBER.ToString();
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text)) //New Line Added
                    {
                        var F_H = NUMBER.Text;
                        if (Convert.ToDouble(F_H) > 0)
                        {
                            var NUMBER_PARAMY = Convert.ToDouble(F_H); //شماره رسید

                            var itemsSource = (List<NUMYS>)NUMBER.ItemsSource;
                            if (itemsSource == null)
                            {
                                itemsSource = new List<NUMYS>();
                                NUMBER.ItemsSource = itemsSource; // Set ItemsSource to the new list
                            }
                            // Check if the item exists and add it if not
                            if (!itemsSource.Any(item => item?.NUMBER == NUMBER_PARAMY))
                            {
                                itemsSource.Add(new NUMYS { NUMBER = NUMBER_PARAMY });
                            }

                            NUMBER.SelectedValue = NUMBER_PARAMY;
                            NUMBER.UpdateLayout();
                            NUMBER.Items.Refresh();
                        }
                    }
                }
                NUMBER.Tag = HEADER_FAC.NUMBER;

                DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر
                MAS.Text = HEADER_FAC.MAS.ToStringNullSafe(); //مدت
                DEPATMAN.SelectedValue = HEADER_FAC.DEPATMAN; DEPATMAN.Items.Refresh(); //واحد
                CUST_KIND.SelectedValue = HEADER_FAC.CUST_KIND; CUST_KIND.Items.Refresh(); //نوع مشتری

                TAKHFIF_PERCENT.Text = "0"; //Reset درصد تخفیف برای جلوگیری از تداخل و محاسبه اشتباه

                FNUMCO.Text = string.IsNullOrEmpty(HEADER_FAC?.FNUMCO.ToStringNullSafe()) ? "0" : HEADER_FAC?.FNUMCO.ToStringNullSafe(); //شماره داخلی

                string thevalue = HEADER_FAC.CUST_NO;
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();

                if (CUST_NO.ItemsSource == null)
                {
                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                }

                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                {
                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                }
                CUST_NO.SelectedValue = HEADER_FAC.CUST_NO; //مشتری
                CUST_NO.Items.Refresh();

                SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN3);

                SGN1usid.Tag = Convert.ToInt32(HEADER_FAC.sgn1usid);
                SGN2usid.Tag = Convert.ToInt32(HEADER_FAC.sgn2usid);
                SGN3usid.Tag = Convert.ToInt32(HEADER_FAC.sgn3usid);

                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn1usid)?.SAL_NAME;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.SAL_NAME;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn3usid)?.SAL_NAME;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                if (IsExporty)
                {
                    if (HEADER_FAC?.ARZD != null) //نرخ ارز
                    {
                        ARZD.Text = HEADER_FAC.ARZD.ToStringNullSafe();
                    }

                    if (HEADER_FAC?.ARZKIND2 != null) //نوع ارز
                    {
                        ARZKIND2.SelectedValue = HEADER_FAC.ARZKIND2; ARZKIND2.Items.Refresh();
                    }
                    else
                    {
                        long? _ISOCODE_ = null;
                        switch (HEADER_FAC.ARZKIND)
                        {
                            case 1: //Dollar
                                _ISOCODE_ = dbms.DoGetDataSQL<long?>($"SELECT ID FROM dbo.TCOD_ARZ WHERE Code = N'840' AND Title = N'US Dollar' AND CountryName = N'UNITED STATES OF AMERICA (THE)'").FirstOrDefault();
                                break;
                            case 2: //Euro
                                _ISOCODE_ = dbms.DoGetDataSQL<long?>($"SELECT ID FROM dbo.TCOD_ARZ WHERE Code = N'978' AND Title = N'Euro' AND CountryName = N'EUROPEAN UNION'").FirstOrDefault();
                                break;
                            case 3: //UAE Dirham
                                _ISOCODE_ = dbms.DoGetDataSQL<long?>($"SELECT ID FROM dbo.TCOD_ARZ WHERE Code = N'784' AND Title = N'UAE Dirham' AND CountryName = N'UNITED ARAB EMIRATES (THE)'").FirstOrDefault();
                                break;
                            case 4: //Pound
                                _ISOCODE_ = dbms.DoGetDataSQL<long?>($"SELECT ID FROM dbo.TCOD_ARZ WHERE Code = N'826' AND Title = N'Pound Sterling' AND CountryName = N'UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND (THE)'").FirstOrDefault();
                                break;
                            case 5: //Yen
                                _ISOCODE_ = dbms.DoGetDataSQL<long?>($"SELECT ID FROM dbo.TCOD_ARZ WHERE Code = N'392' AND Title = N'Yen' AND CountryName = N'JAPAN'").FirstOrDefault();
                                break;

                            default: break;
                        }

                        if (_ISOCODE_ != null)
                        {
                            ARZKIND2.SelectedValue = _ISOCODE_.ToString(); ARZKIND2.Items.Refresh();
                        }
                    }
                }

                OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور
                MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات
                SHIFT.SelectedValue = HEADER_FAC.SHIFT; //شیفت

                M_NAGHD.Text = HEADER_FAC.M_NAGHD.ToStringNullSafe(); //مبلغ نقد
                MABL_VAR.Text = HEADER_FAC.MABL_VAR.ToStringNullSafe(); //مبلغ کارت بانک
                MABL_HAV.Text = HEADER_FAC.MABL_HAV.ToStringNullSafe(); //مبلغ بن یا حواله
                TAKHFIF.Text = HEADER_FAC.TAKHFIF.ToStringNullSafe(); //مبلغ تخفیف

                //CMB_MOIN_VAR
                MOIN_VAR.Text = HEADER_FAC.MOIN_VAR.ToStringNullSafe(); //معین کارت
                MOIN_HAV.Text = HEADER_FAC.MOIN_HAV.ToStringNullSafe(); //معین بن

                //پشت فاکتور
                //TAKHFIF.Text; //مبلغ تخفیف
                MABL_HAZ.Text = (string.IsNullOrEmpty(HEADER_FAC.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER_FAC.MABL_HAZ.ToStringNullSafe()); //مبلغ خدمات
                MOIN_HAZ.Text = HEADER_FAC.MOIN_HAZ; //معین خدمات
                MBAA.Text = HEADER_FAC.MBAA.ToStringNullSafe(); //مالیات و عوارض مبلغ
                HMBAA.Text = HEADER_FAC.HMBAA; //معین مالیات

                BTN_SAVE.IsEnabled = false;

                INVO_LST_SUB_ReGetData();
                PAY_GETP_SUB_SUB_ReGetData();

                GetBalancePerson();

                TAKHFIF_MABL_PRICE();

                Form_Current();

                ActivateChaps();
            }
        }
        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
                record = itemtoadd;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void RefreshAfterUpdate()
        {
            NewRecord = false;
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }


        private CL_LMethods.RestrictionInfo _restrictionInfo;
        private string GetAccessDeniedMessage()
        {
            if (_restrictionInfo?.RestrictionMessages?.Any() == true)
            {
                // ایجاد لیست محدودیت‌ها
                var restrictions = string.Join("، ", _restrictionInfo.RestrictionMessages);

                return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                    $"به آخرین شماره مجاز هدایت خواهید شد. (محدودیت: {restrictions})";
            }

            return $"دسترسی به شماره «{_navigationManager.NUMBER_TO_OPEN}» امکان‌پذیر نیست. " +
                $"شما به آخرین شماره مجاز هدایت خواهید شد.";
        }
        private void ShowMissingOrRestrictedMessage()
        {
            if (_navigationManager?.NUMBER_TO_OPEN == null)
            {
                return;
            }

            double requestedNumber = Convert.ToDouble(_navigationManager.NUMBER_TO_OPEN);
            bool recordExists = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 NUMBER FROM HEAD_LST WHERE NUMBER = {requestedNumber} AND TAG = {FTAG}").FirstOrDefault() != null;
            string message = recordExists ? GetAccessDeniedMessage() : "چنین شماره ای وجود ندارد";
            new Msgwin(false, message).ShowDialog();
            _navigationManager.ClearNumberToOpen();
        }
        private void MakeDefaultFocuseReady()
        {
            if (IsDirectFactor)
            {
                CUST_NO.Focus();
            }
            else
            {
                DATE_N.Focus();
                DATE_N.SelectAll();
            }
        }

        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(NUMBER1.Text) || NUMBER1.Text == "0")
            {
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                INVO_LST_SUB.IsReadOnly = false;
            }

            SecurityAllCheck();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                try
                {

                    if (INVO_LST_SUB_IsFocused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty

                            if (isLastColumn)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (isLastRow)
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[INVO_LST_SUB_DEF_INDEX_COL]);

                                    //Dispatcher.BeginInvoke(new Action(() =>
                                    //{
                                    //    DG.BeginEdit();
                                    //}), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }
                    else if (BTN_SAVE.IsFocused)
                    {
                        BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        return;
                    }

                    CL_LMethods.SendKey_US(Key.Tab);
                }
                catch { /*ignore*/ }
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.S || e.SystemKey == Key.S))
                {
                    e.Handled = true;
                    BTN_SAVE_Click(null, null);
                }
            }

            if (INVO_LST_SUB != null && !INVO_LST_SUB.IsKeyboardFocusWithin && !INVO_LST_SUB.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }


            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }

        #region SPECIAL_F7
        object ISearchableWindow.GetSearchSource() => _navigationManager.RecordsData;
        public void OnSearchResultSelected(object selectedItem)
        {
            // Handle the selected item
            if (selectedItem is HEAD_LST item)
            {
                if (item != null)
                {
                    //_navigationManager.MoveReGetData(INavigator.Jahat.)
                    var itemfound = _navigationManager.RecordsData.FirstOrDefault(x => x.NUMBER.Equals(Convert.ToDouble(item.NUMBER)));
                    if (itemfound != null)
                    {
                        _navigationManager.IsNewRecord = false;

                        // 1) Find its index in the master list
                        int idx = _navigationManager.RecordsData.IndexOf(itemfound);
                        if (idx < 0)
                        {
                            // not found (perhaps filtered out?), bail out
                            new Msgwin(false, "یافت نشد: مورد انتخاب شده در لیست اصلی وجود ندارد").Show();
                            return;
                        }

                        // 2) Tell the navigation manager to move to that position
                        _navigationManager.MoveReGetData(Jahat.CustomPosition, idx);
                        //OnCurrentRecordChanged(itemfound);
                    }
                }
            }
        }
        public IEnumerable<SearchableProperty> GetSearchableProperties()
        {
            return new[]
            {
           new SearchableProperty { DisplayName = "شماره فاکتور", PropertyPath = "NUMBER1", PropertyType = typeof(double) },
           new SearchableProperty { DisplayName = "شماره رسید", PropertyPath = "NUMBER", PropertyType = typeof(double) },
           new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
           new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
           new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
           new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
           // Add other searchable properties
       };
        }
        #endregion

        private void SecurityAllCheck()
        {
            if (IsDirectFactor)
            {
                //CL_HESABDARI.SETSECURITY(this.GetType().Name, "FACTFRMO", new WindowInteropHelper(this).Handle, this.GetType().Name);
                //CL_HESABDARI.SETSECURITYSUB(INVO_LST_SUB, "FACTFRMO");
            }
            else
            {
                //فاکتور غیر مستقیم
            }
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "FACTKH", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_SUB, "FACTKH");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            CL_HESABDARI.SETSECURITYSUB(PAY_GETP_SUB, "FACTKH");
        }
        public void ANBAR_LOADITEM()
        {
            string RowSource_ANBAR = "SELECT     TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     ANBCO FROM dbo.OPANBACCESS WHERE     (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
                if (rst.Count > 0)
                {
                    ANBARDefaultValue = (int)rst.FirstOrDefault();

                    Baseknow.anbardef = ANBARDefaultValue;
                }
                else
                {
                    Baseknow.anbardef = Baseknow.DEFANB;
                }
            }
            else
            {
                Baseknow.anbardef = Baseknow.DEFANB;
            }
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>(RowSource_ANBAR).ToList();
            ANBAR_COLUMN.ItemsSource = ARST;
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع مشتری
            CUST_KIND.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND.DisplayMemberPath = "CUSTKNAME";
            CUST_KIND.SelectedValuePath = "CUST_COD";
            CUST_KIND.SelectedIndex = 0;

            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //واحد ها
            var RST = dbms.DoGetDataSQL<Custom_DEPART>("SELECT DEPATMAN,DEPNAME FROM DEPART ORDER BY DEPNAME").ToList();
            foreach (var item in RST)
            {
                item.DEPNAME = item.DEPNAME.NormalizeArabicPersian();
            }
            DEPATMAN.ItemsSource = RST;

            DEPATMAN.DisplayMemberPath = "DEPNAME";
            DEPATMAN.SelectedValuePath = "DEPATMAN";
            DEPATMAN.SelectedIndex = 0;
            DEPATMAN.SelectedItem = 0;
            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;

            //انبار کالا
            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //شیفت
            SHIFT.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT.SHIFT_ID, SHIFT.SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();
            SHIFT.DisplayMemberPath = "SHNAME";
            SHIFT.SelectedValuePath = "SHIFT_ID";
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER;

            GetResids();


            //کبموباکس مجری پرسنل
            string sql = @"
               SELECT sd.SAL_NAME, sd.PSAL_NAME, sd.GRSAL, sd.ENABL, sd.IDD
               FROM SALA_DTL sd
               LEFT JOIN USER_PERSONEL_ORDER uo 
                    ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
               WHERE sd.ENABL = 0
               ORDER BY
                    CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                    uo.SORT_ORDER, sd.SAL_NAME";
            rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            foreach (var item_person in rst_personel)
                item_person.SAL_NAME = CL_HESABDARI.DECODEUN(item_person.SAL_NAME);

            PERSONEL.ItemsSource = rst_personel;
            PERSONEL.DisplayMemberPath = "SAL_NAME";
            PERSONEL.SelectedValuePath = "IDD";


            //پشت فاکتور بخش چک:
            #region POSHTE_FACTOR

            //کمبوباکس های پشت فاکتور
            bANKColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();


            var HESNAMELST = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM CUST_HESAB ORDER BY NAME, hes").ToList();
            //var HESNAMELST = dbms.DoGetDataSQL<CUSTOM_HESABHA>("SELECT N_KOL,NUMBER,TNUMBER, RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) AS hes, NAME FROM TDETA_HES").ToList();
            //CMB_MOIN_VAR.ItemsSource = HESNAMELST.Where(w => w.N_KOL == Baseknow.BANKHA).ToList(); //معین واریزی
            CMB_MOIN_VAR.ItemsSource = HESNAMELST; //معین واریزی
            CMB_MOIN_HAV.ItemsSource = HESNAMELST; //معين حواله
            CMB_MOIN_HAZ.ItemsSource = HESNAMELST; //معين خدمات
            CMB_HMBAA.ItemsSource = HESNAMELST; //معین مالیات

            //دریافت چک:
            //Giving All Data as Master:
            //معین بانک
            n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {Baseknow.BANKHA})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //تفضیلی
            n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.N_KOL) ={Baseknow.BANKHA}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();

            #endregion


            if (IsExporty)
            {
                ARZKIND2.ItemsSource = dbms.DoGetDataSQL<TCOD_ARZ>($"SELECT ID,Code, Title, ISOCode, (ISOCode+N' - '+Title+N' - '+CountryName) AS ARZCOUNTRY, CRT, UID FROM dbo.[TCOD_ARZ]").ToList();
            }
            if (!IsDirectFactor)
            {
                if (IsExporty)
                {
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<NUMYS>($"SELECT NUMBER, TAG FROM HEAD_LST WHERE (TAG = {HTAG}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG}))))) AND (SADER = 1 OR SADER IS NULL) ORDER BY NUMBER").ToList();
                }
                else
                {
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<NUMYS>($"SELECT NUMBER, TAG FROM HEAD_LST WHERE (TAG = {HTAG}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG}))))) AND (SADER = 0 OR SADER IS NULL) ORDER BY NUMBER").ToList();
                }
            }



        }

        private void GetResids()
        {
            if (!IsDirectFactor)
            {
                if (IsExporty)
                {
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<NUMYS>($"SELECT NUMBER, TAG FROM HEAD_LST WHERE (TAG = {HTAG}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG}))))) AND (SADER=1 OR SADER IS NULL) ORDER BY NUMBER").ToList();
                }
                else
                {
                    //شماره رسید ها
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<NUMYS>($"SELECT NUMBER, TAG FROM HEAD_LST WHERE (TAG = {HTAG}) AND (NOT (NUMBER IN (SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG) = {FTAG}))))) AND (SADER = 0 OR SADER IS NULL) ORDER BY NUMBER").ToList();
                }
            }

        }

        private void INVO_LST_SUB_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_SUB.SelectedItem != null)
            {
                if (INVO_LST_SUB.Items.Count > 0)
                    CURRENT_ROW_INDEX = INVO_LST_SUB.SelectedIndex;

                if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
                {
                    if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                    }
                }
            }
        }

        private void INVO_LST_SUB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL
                if (!(INVO_LST_SUB.Items.Count < 1) && !(INVO_LST_SUB.SelectedItem is null))
                {
                    CURRENT_ROW_INDEX = INVO_LST_SUB.SelectedIndex;
                }
            }
        }
        private void INVO_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as INVO_LST_FACTOR22;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || CurrentRow?.ANBAR == null || string.IsNullOrEmpty(CurrentRow?.CODE))
            {
                return;
            }

            int? LastSelectedVahed = null; //پیش فرض واحد کالا انتخاب شده از قبل 
            if (CurrentRow?.VAHED_K != null)
            {
                LastSelectedVahed = (int)CurrentRow.VAHED_K;
            }

            if (e.Column.SortMemberPath == "VAHED_K") //اگر کاربر داخل واحد کالا بود
            {
                var COMBOBOX_VAHED_K = e.EditingElement as ComboBox;
                if (COMBOBOX_VAHED_K == null) return;

                // دریافت واحدهای فرعی کالا
                var filteredUnits = dbms.DoGetDataSQL<Custom_VAHEDK>(@$"SELECT DISTINCT VAHED, NAMES
                                                                FROM (
                                                                    SELECT dbo.TCOD_VAHEDS.CODE AS VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.TCOD_VAHEDS
                                                                    INNER JOIN dbo.STUF_DEF ON dbo.TCOD_VAHEDS.CODE = dbo.STUF_DEF.VAHED
                                                                    WHERE dbo.STUF_DEF.CODE = N'{CurrentRow.CODE}'
                                                                    UNION ALL
                                                                    SELECT dbo.MODULE_D.VAHED, dbo.TCOD_VAHEDS.NAMES
                                                                    FROM dbo.MODULE_D
                                                                    INNER JOIN dbo.TCOD_VAHEDS ON dbo.MODULE_D.VAHED = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE dbo.MODULE_D.CODE = N'{CurrentRow.CODE}'
                                                                ) AS Combined").ToList();

                RST_KALAVAHED_LST = filteredUnits;

                // تنظیم آیتم‌های کمبوباکس
                COMBOBOX_VAHED_K.ItemsSource = RST_KALAVAHED_LST;

                // تنظیم مقدار انتخاب شده
                if (LastSelectedVahed.HasValue)
                {
                    COMBOBOX_VAHED_K.SelectedValue = LastSelectedVahed;
                }
                else if (filteredUnits.Any())
                {
                    COMBOBOX_VAHED_K.SelectedValue = filteredUnits.FirstOrDefault().VAHED;
                }

                // رفرش کردن آیتم‌ها
                COMBOBOX_VAHED_K.Items.Refresh();
            }

        }
        private void INVO_LST_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                INVO_LST_SUB_IsFocused = false;
            }
            else
            {
                INVO_LST_SUB_IsFocused = true;
            }
        }
        public void Form_Current()
        {
            bool ghat = false;

            if (Baseknow.SIGN ?? false)
            {
                ActivateChaps();
            }

            if (string.IsNullOrEmpty(N_S.Text))
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
                INVO_LST_SUB.IsReadOnly = false;
                Page58.IsEnabled = true;
                lsanad.Foreground = Brushes.Yellow;
                MABNA.Text = null;
            }
            else
            {
                var rst = dbms.DoGetDataSQL<DeedHedData>($"SELECT BASE, GHATEI FROM DEED_HED WHERE N_S = {N_S.Text}").FirstOrDefault();
                if (rst != null)
                {
                    MABNA.Text = rst.BASE;
                    if (rst.GHATEI)
                    {
                        ghat = true;
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        INVO_LST_SUB.IsReadOnly = true;
                        //Page58.IsEnabled = false; //New Modify
                        ESLAH.IsEnabled = false;
                        lsanad.Foreground = Brushes.Red;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        INVO_LST_SUB.IsReadOnly = false;
                        //Page58.IsEnabled = true;//New Modify
                        lsanad.Foreground = Brushes.White;
                    }
                }
            }

            if (Baseknow.MAND && !string.IsNullOrEmpty(CUST_NO.SelectedValue?.ToString()))
            {
                if (!CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue.ToString()))
                {
                    if (CUST_NO.SelectedValue != null)
                    {
                        MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                    }
                }
                else
                {
                    MANDAH.Text = "مسدود است";
                }
            }

            if (NewRecord)
            {
                //New Modify Page58.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                if (!ghat)
                {
                    INVO_LST_SUB.IsReadOnly = false;
                    Page58.IsEnabled = true;
                }
                else
                {
                    Page58.IsEnabled = false;
                    INVO_LST_SUB.IsReadOnly = true;
                }
            }

            if (Baseknow.SIGN ?? false)
            {
                SGN1.Visibility = Visibility.Visible;
                SGN2.Visibility = Visibility.Visible;
                SGN3.Visibility = Visibility.Visible;

                var signResult = dbms.DoGetDataSQL<SignData>($"SELECT KFR_BAZAR, KFR_HESAB, KFR_MODIR FROM dbo.SIGN WHERE USERCO = {Baseknow.USERCOD}").FirstOrDefault();
                if (signResult != null)
                {
                    SGN1.IsEnabled = signResult.KFR_BAZAR;
                    SGN2.IsEnabled = signResult.KFR_HESAB;
                    SGN3.IsEnabled = signResult.KFR_MODIR;
                }

                DATE_N.IsEnabled = true;
                MAS.IsEnabled = true;
                FNUMCO.IsEnabled = true;
                CUST_NO.IsEnabled = true;
                CUST_NO2.IsEnabled = true;
                MOLAH.IsEnabled = true;
                MOIN_HAZ.IsEnabled = true;

                if (SGN3.IsChecked == true)
                {
                    SGN2.IsEnabled = false;
                    SGN1.IsEnabled = false;
                    this.AllowDeletions = false;
                    INVO_LST_SUB.IsReadOnly = true;
                    //New ModifyPage58.IsEnabled = false;
                    NUMBER.IsEnabled = false;
                    DATE_N.IsEnabled = false;
                    MAS.IsEnabled = false;
                    FNUMCO.IsEnabled = false;
                    CUST_NO.IsEnabled = false;
                    CUST_NO2.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    MOIN_HAZ.IsEnabled = false;
                }
                else if (SGN2.IsChecked == true)
                {
                    SGN1.IsEnabled = false;
                    this.AllowDeletions = false;
                    INVO_LST_SUB.IsReadOnly = true;
                    //New ModifyPage58.IsEnabled = false;
                    NUMBER.IsEnabled = false;
                    DATE_N.IsEnabled = false;
                    MAS.IsEnabled = false;
                    FNUMCO.IsEnabled = false;
                    CUST_NO.IsEnabled = false;
                    CUST_NO2.IsEnabled = false;
                    MOLAH.IsEnabled = false;
                    MOIN_HAZ.IsEnabled = false;
                }
            }

            //SecurityAllCheck();

            if (OKF.IsChecked != null && OKF.IsChecked == true && !NewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_SUB.IsReadOnly = true;
                //New Modify Page58.IsEnabled = false;
                ESLAH.IsEnabled = true;
            }

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, FTAG, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            if (IsDirectFactor)
            {
                LABEL_HEADER.Content = "فاکتور خرید مستقیم";
            }
            else
            {
                NUMBER.Focusable = true;
                NUMBER.IsHitTestVisible = true;

                ANBAR_COLUMN.IsReadOnly = true; //انبار
                NAME_CODE_COLUMN.IsReadOnly = true; //نام کالا
                VAHED_K_COLUMN.IsReadOnly = true; //واحد
                MEGH_COLUMN.IsReadOnly = true; //مقدار
                MEGHK_COLUMN.IsReadOnly = true; //مقدار کل

                INVO_LST_SUB.CanUserAddRows = false;

                CUST_NO.IsHitTestVisible = false;
                CUST_NO2.IsHitTestVisible = false;

                FNUMCO.Visibility = Visibility.Visible;
                //LABEL_FNUMCO_.Visibility = Visibility.Visible;

                LABEL_HEADER.Content = "فاکتور خرید غیــر مستقیم";
            }

            if (IsExporty)
            {

                LABEL_HEADER.Content = "فاکتور خرید صادراتی";
            }

            if (!CL_HESABDARI.LETSGO("ESLAHRF"))
            {
                ESLAH.Visibility = Visibility.Hidden;
            }
            else
            {
                ESLAH.Visibility = Visibility.Visible;
            }

        }

        public void INVO_LST_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {
                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
																				 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
																			   	 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
																			 	 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
															FROM            dbo.INVO_LST LEFT OUTER JOIN
																				 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
																				 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
																				 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                         WHERE        (dbo.INVO_LST.TAG = {HTAG}) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                INVO_LST_FACTOR22_DATA?.Clear();
                foreach (var item in QRE_LST)
                    INVO_LST_FACTOR22_DATA.Add(item);

                if (!IsDirectFactor)
                {
                    var rst = dbms.DoGetDataSQL<_FACT_HEAD_HAV_>("SELECT HEAD_LST.CUST_NO,HEAD_LST.MAS,HEAD_LST.DEPATMAN,HEAD_LST.TICMBAA,HEAD_LST.SHARAYET,HEAD_LST.FNUMCO,HEAD_LST.JAY,MODAT_PPID,PEID,PEPID,HEAD_LST.USER_NAME FROM HEAD_LST WHERE (((HEAD_LST.NUMBER) = " + NUMBER.Text + $") And ((HEAD_LST.TAG) = {HTAG})) GROUP BY TICMBAA,HEAD_LST.CUST_NO,HEAD_LST.MAS,HEAD_LST.FNUMCO,HEAD_LST.SHARAYET,HEAD_LST.JAY,HEAD_LST.DEPATMAN,MODAT_PPID,PEID,PEPID,HEAD_LST.USER_NAME").FirstOrDefault();

                    if (rst is not null && rst?.CUST_NO != null)
                    {
                        string receiptCustNo = rst?.CUST_NO?.Trim(); //TAG 1
                        if (!string.IsNullOrEmpty(receiptCustNo))
                        {
                            string currentInvoiceCustNo = _navigationManager.CurrentRecord?.CUST_NO.Trim();
                            if (currentInvoiceCustNo != receiptCustNo)
                            {
                                // بررسی مغایرت کد مشتری بین رسید و فاکتور
                                if (!string.IsNullOrEmpty(currentInvoiceCustNo) && currentInvoiceCustNo != receiptCustNo)
                                {
                                    var msgCheck = new Msgwin(true, $"کد مشتری در رسید انبار ({receiptCustNo}) با فاکتور خرید ({currentInvoiceCustNo}) مغایرت دارد. \nآیا مایل به اصلاح فاکتور بر اساس رسید هستید؟");
                                    msgCheck.ShowDialog();

                                    if (msgCheck.DialogResult == true)
                                    {
                                        try
                                        {
                                            // اصلاح سربرگ فاکتور خرید (Tag 12)
                                            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET CUST_NO = N'{receiptCustNo}' WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}");

                                            ChangeIsHappend = true;

                                            universControl.PopNotifyShowUp("اصلاح مشتری فاکتور با موفقیت انجام شد , مجددا روی ذخیره کلیک کنید برای صدور سند.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue);
                                        }
                                        catch (Exception ex)
                                        {
                                            new Msgwin(false, "خطا در اصلاح مشتری فاکتور: ").ShowDialog();
                                        }
                                    }
                                }
                            }

                            var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT TOP 1 hes,CUST_COD, NAME FROM dbo.CUST_HESAB WHERE HES = N'" + receiptCustNo + "'").FirstOrDefault();
                            if (data != null)
                            {
                                if (CUST_NO.ItemsSource == null)
                                {
                                    CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                                }

                                if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == receiptCustNo))
                                {
                                    ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = receiptCustNo, NAME = data.NAME });
                                }
                                CUST_NO.SelectedValue = receiptCustNo;
                                CUST_NO.Items.Refresh();
                            }
                        }
                        var RSTDATE = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 DATE_N FROM HEAD_LST WHERE NUMBER = {NUMBER.SelectedValue} AND TAG = {HTAG}").FirstOrDefault();
                        if (RSTDATE != null)
                        {
                            DATE_N.Text = RSTDATE;
                        }


                        //FNUMCO.Text = string.IsNullOrEmpty(rst.FNUMCO.ToStringNullSafe()) ? "0" : rst.FNUMCO.ToStringNullSafe();
                        //DEPATMAN.SelectedValue = rst.DEPATMAN; DEPATMAN.Items.Refresh();
                        USER_NAME.Text = rst.USER_NAME; //دریافت نام کاربر از رسید انبار خرید برای فاکتور خرید غیر مستقیم

                        ////CL_HESABDARI.LOGFACT(Convert.ToDouble(NUMBER.Text), ;?;, Convert.ToDouble(NUMBER1.Text), "UPDATEFACTOR");
                    }

                    NUMBER.Tag = NUMBER.SelectedValue; //Save Last Valid SelectedValue
                }
            }
        }
        private void INVO_LST_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || INVO_LST_SUB == null || INVO_LST_SUB.CurrentCell == null)
                return;

            string CURRENT_COLUMN_NAME = "";
            if (INVO_LST_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = INVO_LST_SUB.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                BTN_DELETE_Click(null, null);
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (INVO_LST_SUB?.CurrentCell != null && INVO_LST_SUB.IsEnabled && !INVO_LST_SUB.IsReadOnly)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = INVO_LST_SUB.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = INVO_LST_SUB.Items.IndexOf(currentCell.Item);
                            int columnIndex = INVO_LST_SUB.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = INVO_LST_SUB.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < INVO_LST_SUB.Columns.Count)
                                {
                                    // Get the column information
                                    var column = INVO_LST_SUB.Columns[columnIndex];

                                    // Ensure that the column has a valid SortMemberPath
                                    if (!string.IsNullOrEmpty(column.SortMemberPath))
                                    {
                                        // Use reflection to get and set the property values
                                        var propertyInfo = valueAbove.GetType().GetProperty(column.SortMemberPath);

                                        // Ensure that the property exists and is not null
                                        if (propertyInfo != null)
                                        {
                                            // Get the value from the above cell
                                            object valueAboveCellValue = propertyInfo.GetValue(valueAbove);

                                            // Cast currentCell.Item to the actual data type
                                            var currentItem = currentCell.Item;

                                            // Use reflection to set the value on the current item
                                            if (currentItem.GetType().GetProperty(column.SortMemberPath) is PropertyInfo currentCellProperty)
                                            {
                                                // Set the value on the current cell's item
                                                currentCellProperty.SetValue(currentItem, valueAboveCellValue);

                                                INVO_LST_SUB.Items.Refresh();

                                                INVO_LST_SUB.BeginEdit();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        e.Handled = true;
                    }
                }
                catch { }
            }

            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME == "MABL" || CURRENT_COLUMN_NAME == "MABL_K")
                {
                    e.Handled = true;
                    var text = "000";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
            if (e.Key == Key.Subtract)
            {
                if (CURRENT_COLUMN_NAME == "MABL" || CURRENT_COLUMN_NAME == "MABL_K")
                {
                    e.Handled = true;
                    var text = "00";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
        }
        private void INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_SUB.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                INVO_LST_SUB.RowEditEnding -= INVO_LST_SUB_RowEditEnding;
                if (_RC_ is null)
                {
                    INVO_LST_SUB.CancelEdit();
                }
                else
                {
                    INVO_LST_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_SUB.RowEditEnding += INVO_LST_SUB_RowEditEnding;
                INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;
            });
        }
        private void INVO_LST_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVO_LST_SUB.SelectedItem is not null)
            {
                if (INVO_LST_SUB.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }
        private void INVO_LST_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (CUST_NO.SelectedValue == null)
            {
                CUST_NO.Focus();
                new Msgwin(false, "نام مشتری نمیتواند خالی باشد!").ShowDialog();
                return;
            }
            if (CUST_KIND.SelectedValue == null)
            {
                CUST_KIND.Focus();
                new Msgwin(false, "نوع مشتری نمیتواند خالی باشد!").ShowDialog();
                return;
            }

            #region REFILL_CURRENTS
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
            {
                ENTERED_VALUE_ROW = Comboval?.SelectedValue.ToStringNullSafe();
            }
            else if (!ReferenceEquals(TexboVal, null))
            {
                ENTERED_VALUE_ROW = TexboVal?.Text.Trim();
            }

            CURRENT_ITEMS_ROW = e.Row.Item as INVO_LST_FACTOR22;
            #endregion


            if (IsNull(CURRENT_ITEMS_ROW.ANBAR))
            {
                Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد.");
                msgwin.ShowDialog();
            }
            else if (!IsNull(CURRENT_ITEMS_ROW.CODE))
            {
                var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }

            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    //INVO_LST_FACTOR22_CURRENT_ROW_ITEMS.MOGODI_A = INVO_LST_FACTOR22_WAS_ROW_ITEM?.MOGODI_A;
                }
            }

            //---------------------------------------------------------------------------------------------------------------------------------------------------

            double min = 0;
            double MAND = 0;

            //انبار
            #region ANBAR
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.ANBAR = WAS_ROW_ITEM.ANBAR;
                    universControl.PopNotifyShow("مقدار نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                    return;
                }
                else
                {
                    if (CURRENT_ITEMS_ROW.CODE != null)
                    {
                        var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT CODE FROM STUF_STK WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' AND ANBAR = {ENTERED_VALUE_ROW}").ToList();
                        if (Rst1.Count == 0)
                        {
                            universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                            CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                            MOGU.Text = null;
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }
                        else
                        {
                            MOGU.Text = (Rst1.FirstOrDefault().MOGODI + Rst1.FirstOrDefault().MOGODI_A).ToString();
                        }
                    }
                }
            }
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim() ||
                    (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe())))
                {
                    #region CODE_NotInList
                    if (CURRENT_ITEMS_ROW.ANBAR is null) // انبار خالی نیست
                    {
                        return;
                    }
                    //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                    if (true)
                    {
                        //محاسبه موجودی واقعی این کالا
                        min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);

                        //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                        if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                        {
                            CURRENT_ITEMS_ROW.MEGH = 0;
                            CURRENT_ITEMS_ROW.MEGHk = 0;
                            CURRENT_ITEMS_ROW.MABL_K = 0;
                            SERCHK sERCHK = new SERCHK(I_AM_KHAREED, CURRENT_ITEMS_ROW.ANBAR.ToString());
                            sERCHK.ShowDialog();

                            if (FROM_SEARCH_KAL.CODE is null)
                            {
                                //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                var TheCol00 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                var DGCInf00 = new DataGridCellInfo(INVO_LST_SUB.Items[row_index], INVO_LST_SUB.Columns[TheCol00]);
                                var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf00);
                                TheDGCell_MABL_K.Focus();

                                INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                return;
                            }
                            else
                            {
                                CURRENT_ITEMS_ROW.CODE = FROM_SEARCH_KAL.CODE;
                                CURRENT_ITEMS_ROW.NAME_CODE = FROM_SEARCH_KAL.NAME_CODE;

                                CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);

                                //Cleaning
                                FROM_SEARCH_KAL.CODE = null;
                                FROM_SEARCH_KAL.NAME_CODE = null;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                            {
                                //Cleaning
                                CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                                CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                return;
                            }

                            if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                            {
                                //اگر عدد وارد کرده برم سرغ کد کالا
                                var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ITEMS_ROW.ANBAR})").FirstOrDefault();
                                if (!ReferenceEquals(FoundKala, null))
                                {
                                    CURRENT_ITEMS_ROW.CODE = FoundKala.CODE;
                                    CURRENT_ITEMS_ROW.NAME_CODE = FoundKala.NAME;

                                    CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                                }
                                else
                                {
                                    //شماره فنی
                                    var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND dbo.STUF_FSK.ANBAR = {CURRENT_ITEMS_ROW.ANBAR}").ToList();
                                    if (rstfani.Count > 0)
                                    {
                                        CURRENT_ITEMS_ROW.CODE = rstfani.FirstOrDefault().CODE;
                                        CURRENT_ITEMS_ROW.NAME_CODE = rstfani.FirstOrDefault().NAME;

                                        CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                                    }
                                    else
                                    {
                                        new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                                        INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);

                                        return;
                                    }
                                }
                            }
                            else
                            {
                                CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITEMS_ROW.ANBAR.ToString(), I_AM_KHAREED);
                                if (FROM_SEARCH_KAL.CODE is null)
                                {
                                    INVO_LST_SUB.CellEditEnding -= INVO_LST_SUB_CellEditEnding;
                                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                                    INVO_LST_SUB.CellEditEnding += INVO_LST_SUB_CellEditEnding;

                                    CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                    CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;

                                    return;
                                }
                                else
                                {
                                    CURRENT_ITEMS_ROW.CODE = FROM_SEARCH_KAL.CODE;
                                    CURRENT_ITEMS_ROW.NAME_CODE = FROM_SEARCH_KAL.NAME_CODE;

                                    CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);

                                    //Cleaning
                                    FROM_SEARCH_KAL.CODE = null;
                                    FROM_SEARCH_KAL.NAME_CODE = null;
                                }
                            }
                        }

                        var RST00 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                        if (RST00.Count == 0)
                        {
                            MOGU.Text = null;
                        }
                        else
                        {
                            MOGU.Text = ((double)RST00.FirstOrDefault().MOGODI + RST00.FirstOrDefault().MOGODI_A).ToString();
                        }
                        //var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "'").ToList();
                        //if (RST.Count != 0)
                        //{
                        //    CURRENT_ITEMS_ROW.VAHED_K = RST.FirstOrDefault().VAHED;
                        //}

                        if (CURRENT_ITEMS_ROW.ANBAR != 0)
                        {
                            if (CURRENT_ITEMS_ROW.id > 0)
                            {
                                var RSTCO1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                                if (RSTCO1.Count == 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                    msgwin.ShowDialog();
                                }
                                else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                                {
                                    var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                                    if (RSTCO2.Count > 0)
                                    {
                                        MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                                        if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ITEMS_ROW.MEGHk), 2) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                        {
                                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                            msgwin.ShowDialog();

                                            CURRENT_ITEMS_ROW = WAS_ROW_ITEM;

                                        }
                                        else
                                        {
                                            var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                                            var _WHERE = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                                            if (RSTCO3.Count > 0)
                                            {
                                                RSTCO3.FirstOrDefault().MOGODI = MAND - CURRENT_ITEMS_ROW.MEGHk;
                                                RSTCO3.FirstOrDefault().MOGODI_A = 0;
                                            }
                                        }
                                    }
                                }
                                else if (CURRENT_ITEMS_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                                {
                                    if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITEMS_ROW.MEGH_MAR)) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();
                                        CURRENT_ITEMS_ROW = WAS_ROW_ITEM;

                                    }
                                }
                                else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) < min && Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR > 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                    msgwin.ShowDialog();
                                    CURRENT_ITEMS_ROW = WAS_ROW_ITEM;
                                }
                            }
                        }
                        VAHED_K_AfterUpdate();

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                    #endregion
                }
            }
            #endregion

            //واحد کالا
            #region VAHED_K
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    return;
                }
                if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null)
                {
                    return;
                }
                if (((e.Row.Item as INVO_LST_FACTOR22)?.VAHED_K is null) || (((e.Row.Item as INVO_LST_FACTOR22).CODE is null))
                        || ((e.Row.Item as INVO_LST_FACTOR22).NAME_CODE is null))
                {
                    INVO_LST_SUB_CANCEL_EDIT();
                    (e.Row.Item as INVO_LST_FACTOR22).VAHED_K = WAS_ROW_ITEM.VAHED_K;
                    return;
                }
                #region VAHED_K_AfterUpdate
                VAHED_K_AfterUpdate();
                #endregion

                #region VAHED_K_NotInList
                var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
                if (RSTV1.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                    CURRENT_ITEMS_ROW.VAHED_K = null;
                }
                else
                {
                    CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RSTV1.FirstOrDefault().NESBAT/*Fields(2)*/;
                    if (CURRENT_ITEMS_ROW.MABL == 0)
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = true;
                    }
                    else
                    {
                        var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                        var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                        if (!(THECELL1 is null))
                            THECELL1.IsTabStop = true;

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = true;
                #endregion
            }
            #endregion

            //مقدار
            #region MEGH
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (CURRENT_ITEMS_ROW.ANBAR is null || CURRENT_ITEMS_ROW.CODE is null || CURRENT_ITEMS_ROW.VAHED_K is null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGH = 0;
                    return;
                }
                if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null)
                {
                    return;
                }
                CURRENT_ITEMS_ROW.MEGH = Convert.ToDouble(ENTERED_VALUE_ROW);

                MEGH_AfterUpdate();

                if (CURRENT_ITEMS_ROW.MABL_K != CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk)
                {
                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
            }
            #endregion

            //مقدار کل
            #region MEGHk
            if (e.Column.SortMemberPath == "MEGHk")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITEMS_ROW.MEGHk = 0;
                    return;
                }
                if (CURRENT_ITEMS_ROW?.ANBAR is null || CURRENT_ITEMS_ROW?.CODE is null || CURRENT_ITEMS_ROW?.VAHED_K is null || CURRENT_ITEMS_ROW?.MEGH is null)
                {
                    return;
                }

                #region MEGHk_AfterUpdate
                long Temp;
                var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                }
                else
                {
                    CURRENT_ITEMS_ROW.MEGH = CURRENT_ITEMS_ROW.MEGHk / RST.FirstOrDefault().NESBAT;
                    if (CURRENT_ITEMS_ROW.MABL == 0)
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = true;
                    }
                    else
                    {
                        var TheCol0 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol0]);
                        var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                        if (!(THECELL0 is null))
                            THECELL0.IsTabStop = false;

                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
                #endregion
            }
            #endregion

            //مبلغ
            #region MABL
            if (e.Column.SortMemberPath == "MABL")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABL = WAS_ROW_ITEM.MABL;
                    return;
                }
                if (
                    CURRENT_ITEMS_ROW.ANBAR is null ||
                    CURRENT_ITEMS_ROW.CODE is null ||
                    CURRENT_ITEMS_ROW.VAHED_K is null ||
                    CURRENT_ITEMS_ROW.MEGH is null ||
                    CURRENT_ITEMS_ROW.MEGHk is null
                    )
                {
                    return;
                }

                #region MABL_AfterUpdate
                MABL_AfterUpdate(CURRENT_ITEMS_ROW);
                #endregion

            }
            #endregion

            //مبلغ کل
            #region MABL_K
            if (e.Column.SortMemberPath == "MABL_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                    return;
                }
                if (
                   CURRENT_ITEMS_ROW.ANBAR is null ||
                   CURRENT_ITEMS_ROW.CODE is null ||
                   CURRENT_ITEMS_ROW.VAHED_K is null ||
                   CURRENT_ITEMS_ROW.MEGH is null ||
                   CURRENT_ITEMS_ROW.MEGHk is null ||
                   CURRENT_ITEMS_ROW.MABL is null
                   )
                {
                    return;
                }

                if (Math.Round((double)CURRENT_ITEMS_ROW.MABL_K) != CURRENT_ITEMS_ROW.MABL_K)
                {
                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)CURRENT_ITEMS_ROW.MABL_K);
                }
                if (CURRENT_ITEMS_ROW.MEGHk == 0)
                {
                    CURRENT_ITEMS_ROW.MABL_K = 0;
                }
                else
                {
                    CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk;

                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = false;
                }


                if (CURRENT_ITEMS_ROW.MABL == 0 && !IsNull(CURRENT_ITEMS_ROW.CODE))
                {
                    if (CURRENT_ITEMS_ROW.MEGHk == 0)
                    {
                        if (CURRENT_ITEMS_ROW.MABL_K != 0)
                        {
                            CURRENT_ITEMS_ROW.MABL_K = 0;
                        }
                    }
                    else
                    {
                        if (CURRENT_ITEMS_ROW.MABL != CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk)
                        {
                            CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.MABL_K / CURRENT_ITEMS_ROW.MEGHk;
                        }

                        var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                        var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                        var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                        if (!(THECELL1 is null))
                            THECELL1.IsTabStop = false;
                    }
                }
            }
            #endregion

            //Unit Price (مبلغ ارزی واحد)
            #region N_TAF
            if (e.Column.SortMemberPath == "N_TAF")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("N_TAF").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
                    return;
                }
                else
                {
                    //N_TAF_AfterUpdate
                    if (Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) == 0)
                    {
                        CURRENT_ITEMS_ROW.TOTALARZ = 0;
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.TOTALARZ = Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) * CURRENT_ITEMS_ROW.MEGHk;
                    }

                    CURRENT_ITEMS_ROW.MABL = Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) * Convert.ToDouble(ARZD.Text);
                    MABL_AfterUpdate(CURRENT_ITEMS_ROW);
                }
            }
            #endregion

            //Line Total (مبلغ کل ارزی)
            #region TOTALARZ
            if (e.Column.SortMemberPath == "TOTALARZ")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    INVO_LST_SUB.Items[row_index].GetType().GetProperty("TOTALARZ").SetValue(INVO_LST_SUB.Items[row_index], (double?)Convert.ToDouble("0"));
                    return;
                }
                else
                {
                    //TOTALARZ_AfterUpdate
                    if (CURRENT_ITEMS_ROW.MEGHk == 0)
                    {
                        CURRENT_ITEMS_ROW.TOTALARZ = 0;
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.N_TAF = Convert.ToDouble(ENTERED_VALUE_ROW /*TOTALARZ*/) / CURRENT_ITEMS_ROW.MEGHk;
                    }

                    CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.N_TAF * Convert.ToDouble(ARZD.Text);

                    MABL_AfterUpdate(CURRENT_ITEMS_ROW);
                }
            }
            #endregion


        }
        void VAHED_K_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.VAHED_K is null) { return; }
            if (CURRENT_ITEMS_ROW.MABL is null || CURRENT_ITEMS_ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST.FirstOrDefault().NESBAT;
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;
                }
                else
                {
                    var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;

                    if (CURRENT_ITEMS_ROW.MABL is not null && CURRENT_ITEMS_ROW.MEGHk is not null)
                    {
                        CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                    }
                }
            }
        }
        private void INVO_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null)
            {
                return;
            }

            var TheRow = e.Row.Item as INVO_LST_FACTOR22;

            if (ConstructorRowDetector.IsPristine(TheRow)) { INVO_LST_SUB_CANCEL_EDIT(); return; }

            if (!BodyIsValid(TheRow))
            {
                INVO_LST_SUB_CANCEL_EDIT();
                return;
            }


            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                              OUTPUT INSERTED.id
                              VALUES({NUMBER.Text},
                              {HTAG} ,
                              {TheRow.ANBAR}   ,
                              NULL,
                              N'{TheRow.CODE}' ,
                              {TheRow.MEGH} ,
                              {TheRow.MEGHk} ,
                              {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)} ,
                              N'{TheRow.MANDAH}' ,
                              {TheRow.MABL} ,
                              {TheRow.MABL_K} ,
                              0,
                              N'{(TheRow.N_RASID is null ? "NULL" : TheRow.N_RASID)}' ,
                              {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)} ,
                              {(TheRow.RADAH is null ? "NULL" : TheRow.RADAH)} ,
                              {(TheRow.SANAD_NO is null ? "NULL" : TheRow.SANAD_NO)} ,
                              NULL,
                              {(TheRow.ANBARF is null ? "NULL" : TheRow.ANBARF)} ,
                              {TheRow.VAHED_K}   ,
                              {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)} ,
                              {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)} ,
                              {(TheRow.N_TAF is null ? "NULL" : TheRow.N_TAF)} ,
                              {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)} ,
                              {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)} ,
                              {TheRow.IMBAA} ,
                              {(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)} ,
                              N'{(TheRow.VISITOR is null ? "NULL" : TheRow.VISITOR)}' ,
                              {TheRow.TKHN} ,
                              {(TheRow.JAY?.ToString() is null ? "NULL" : TheRow.JAY.ToString())}   ,
                              {(TheRow.JAYO?.ToString() is null ? "NULL" : TheRow.JAYO.ToString())} )";

                var (errorMsgs, _, _, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { TheRow }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);

                if (queryOutputs.Any())
                {
                    TheRow.id = queryOutputs.FirstOrDefault(); // Update the list with the new ID
                                                               //اصلاح شماره ردیف
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG={HTAG}) FROM dbo.INVO_LST WHERE id = {TheRow.id}");
                }
            }
            else //UPDATE
            {
                _qre = $@"UPDATE dbo.INVO_LST
                   SET ANBAR = {TheRow.ANBAR}, CODE = N'{TheRow.CODE}',
                   MEGH = {TheRow.MEGH}, MEGHk = {TheRow.MEGHk}, MEGH_MAR = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},
                   MANDAH = N'{TheRow.MANDAH}', MABL = {TheRow.MABL}, MABL_K = {TheRow.MABL_K},
                   N_RASID = N'{(TheRow.N_RASID is null ? "NULL" : TheRow.N_RASID)}',
                   MEGH_R = {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)}, 
                   RADAH = {(TheRow.RADAH is null ? "NULL" : TheRow.RADAH)}, 
                   SANAD_NO = {(TheRow.SANAD_NO is null ? "NULL" : TheRow.SANAD_NO)},
                   ANBARF = {(TheRow.ANBARF is null ? "NULL" : TheRow.ANBARF)}, 
                   VAHED_K = {TheRow.VAHED_K}, N_KOL = {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)}, 
                   N_MOIN = {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)}, N_TAF = {(TheRow.N_TAF is null ? "NULL" : TheRow.N_TAF)},
                   AVRAGE = {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)},
                   AVRAGE2 = {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)}, IMBAA = {TheRow.IMBAA}, 
                   TOTALARZ = {(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)}, VISITOR = N'{(TheRow.VISITOR is null ? "NULL" : TheRow.VISITOR)}',
                   TKHN = {TheRow.TKHN}, JAY = {(TheRow.JAY?.ToString() is null ? "NULL" : TheRow.JAY.ToString())}, JAYO = {(TheRow.JAYO?.ToString() is null ? "NULL" : TheRow.JAYO.ToString())}
                   WHERE id = {TheRow.id}";

                var (errorMsgs, _, _, _) = IVM.CheckInventoryAndExecuteQuery<int>(new List<object> { TheRow }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);
            }

            //انبار خالی نباشد
            if (TheRow?.ANBAR is null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {TheRow.ANBAR}." });
            }
            //بررسی تعلق انبار و کالا به هم
            else if (!IsNull(TheRow.CODE))
            {
                var RST_STUF_STK = IVM.TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + TheRow.CODE + "' AND ANBAR = " + TheRow.ANBAR).ToList();
                if (RST_STUF_STK.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {TheRow.CODE} به انبار {TheRow.ANBAR} فوق تعلق ندارد." });
                }
            }

            //بررسی صحیح بودن واحد کالا نسبت به خود کالا
            var RSTV1 = IVM.TM.SqlQueryCtc<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + TheRow.CODE + "' AND ((VAHEDS.VAHED)= " + TheRow.VAHED_K + ")))").ToList();
            if (RSTV1.Count == 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });
                TheRow.VAHED_K = null;
            }
            //واحد کالا بررسی مقدار کل باتوجه به نسبت
            else
            {
                var NesbatMegh = RSTV1.FirstOrDefault()?.NESBAT * TheRow.MEGH;
                if (NesbatMegh != TheRow.MEGHk)
                {

                    TheRow.MEGHk = NesbatMegh;
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }
            //بررسی صحیحی بودن مبلغ
            if (TheRow.MABL_K != Math.Round((double)(TheRow.MABL * TheRow.MEGHk)))
            {
                var _mablk = Math.Round((double)(TheRow.MABL * TheRow.MEGHk));
                if (TheRow.MABL_K != _mablk)
                {
                    TheRow.MABL_K = Math.Round((double)(TheRow.MABL * TheRow.MEGHk));
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} با مبلغ {TheRow.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mablk} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
                }
            }

            if (ErrosMessages.Any())
            {
                IVM.RollbackTransaction(); //Undo
            }
            else
            {
                IVM.CommitTransaction(); // Commit Apply Save
            }
            MasterTopErrorMessages.AddRange(ErrosMessages);

            SANAD();

            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

            AVRAGE_UPDATE();
        }

        private void MABL_AfterUpdate(INVO_LST_FACTOR22? Rowy, bool IsSingleCurrentRow = true, bool DoShoeMessages = true)
        {
            if (Rowy.MABL == 0)
            {
                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = true;

                Rowy.MABL_K = Math.Round((double)(Rowy.MABL * Rowy.MEGHk));
            }
            else
            {
                var TheCol = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = false;

                Rowy.MABL_K = Math.Round((double)(Rowy.MABL * Rowy.MEGHk));
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();


            if (ErrosMessages.Count > 0 && DoShoeMessages)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
            }
        }

        public void AVRAGE_UPDATE()
        {
            //CODE_AfterUpdate
            if (CURRENT_ITEMS_ROW?.MEGH > 0 && CURRENT_ITEMS_ROW?.MABL > 0 && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.id != null)
            {
                //var RST = dbms.DoGetDataSQL<STUF_DEF>($@"SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID, mu, sstid, vra
                //  FROM dbo.STUF_DEF WHERE CODE = N'{CURRENT_ITEMS_ROW.CODE}' ").FirstOrDefault();

                //-- ANBAR , DATE , PARA id , COD (CODE)
                var rst3 = dbms.DoGetDataSQL<AVRAGE_MOG>($@"SELECT CODE, MOG, MABL, VMEGHK, VMABK, FMABK, FMEGHK 
                FROM dbo.AVRAGE_MOG('{CURRENT_ITEMS_ROW.ANBAR}', '{DATE_N.Text.ToRawTarikh()}', '{CURRENT_ITEMS_ROW.id}', '{CURRENT_ITEMS_ROW.CODE}')").FirstOrDefault();

                //میانگین
                if (rst3 != null && (rst3.MOG + CURRENT_ITEMS_ROW.MEGHk) != 0)
                {
                    long temp = (long)Math.Round((double)((rst3.MABL + CURRENT_ITEMS_ROW.MABL_K) / (rst3.MOG + CURRENT_ITEMS_ROW.MEGHk) * 100));
                    CURRENT_ITEMS_ROW.AVRAGE = temp / 100d;
                }
                else
                {
                    CURRENT_ITEMS_ROW.AVRAGE = 0;
                }
            }
        }
        void MEGH_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW.MABL is null || CURRENT_ITEMS_ROW.MEGHk is null)
            {
                return;
            }

            double min;
            long Temp;
            double MAND;
            var RST0 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW.VAHED_K + ")))").ToList();
            if (RST0.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
                return;
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST0.FirstOrDefault().NESBAT;
                CURRENT_ITEMS_ROW.MEGH_R = CURRENT_ITEMS_ROW.MEGH * RST0.FirstOrDefault().NESBAT;
                if (CURRENT_ITEMS_ROW.MABL == 0)
                {
                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_SUB, (int)CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = true;

                }
                else
                {
                    var TheCol1 = INVO_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_SUB.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_SUB, (int)CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = false;

                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
            }
            if (Baseknow.MOJU && CURRENT_ITEMS_ROW.ANBAR != 0)
            {
                min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);
                if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                {
                    var RSTM0 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                    if (RSTM0.Count > 0)
                    {
                        MAND = (double)RSTM0.FirstOrDefault();
                        if (Math.Round((double)(RSTM0.FirstOrDefault() - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITEMS_ROW.ANBAR != 0 && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL = WAS_ROW_ITEM.MABL/*.TAG*/;
                            var RSTM1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                            string _where = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                            if (RSTM1.Count > 0)
                            {
                                RSTM1.FirstOrDefault().MOGODI = MAND;
                                RSTM1.FirstOrDefault().MOGODI_A = 0;
                            }
                        }
                        else
                        {
                            var RSTM2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                            var _where = " WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                            if (RSTM2.Count > 0)
                            {
                                RSTM2.FirstOrDefault().MOGODI = MAND - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR));
                                RSTM2.FirstOrDefault().MOGODI_A = 0;
                            }
                        }
                    }
                }
                else
                {
                    var _where = "CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                    var RSTM3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM dbo.STUF_STK {_where}").ToList();
                    if (RSTM3.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if (CURRENT_ITEMS_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                    {
                        if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR)) < min && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                        }
                    }
                    else if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (CURRENT_ITEMS_ROW.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) < min && Baseknow.MOJU)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                        msgwin.ShowDialog();
                        CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                        CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                        CURRENT_ITEMS_ROW.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                    }
                }
            }
        }


        private bool IsNull(object? hTAF2)
        {
            string? _inputy = hTAF2?.ToStringNullSafe();
            if (string.IsNullOrEmpty(_inputy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //Validation
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = null;
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = null;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                DATE_N.Text = null;
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0" || NUMBER.SelectedValue == null)  //واحد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره رسید صحیح انتخاب نشده !" });
            }

            if (DEPATMAN.SelectedValue is null)  //واحد
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد نمیتواند خالی باشد." });
            }
            if (CUST_KIND.SelectedValue is null) //نوع مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع مشتری نمیتواند خالی باشد." });
            }
            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام مشتری نمیتواند خالی باشد." });
            }
            if (SHIFT.SelectedValue is null) //شیفت
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شیفت نمیتواند خالی باشد." });
            }

            if (!string.IsNullOrEmpty(DATE_N.Text?.ToRawTarikh()))
            {
                if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                {
                    //تاریخ صحیح نیست
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ فاکتور را بررسی کنید" });
                }
            }


            if (string.IsNullOrEmpty(MAS.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مدت را وارد کنید " });
            }

            if (IsNull(this.CUST_NO.SelectedValue) || this.CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " مشتري مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مشتري مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
                }
            }

            if (!IsNull(this.HMBAA.Text))
            {
                if (CL_HESABDARI.ISTAF(this.HMBAA.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
                }
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if ((bool)Baseknow.SAGHF || (bool)(Baseknow.SAGHF2))
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO2.SelectedValue.ToString())) == false)
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.SelectedIndex = -1;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!" });
                    }
                }
            }
            #region COMENTY_CHECK
            //MOIN_VAR //معین واریزی
            //MOIN_HAV //معین حواله
            //MOIN_HAZ //هزینه (خدمات)
            //HMBAA    // معین مالیات
            #endregion

            //MABL_VAR   -----------  MOIN_VAR  {معین واریزی}
            if (string.IsNullOrEmpty(CMB_MOIN_VAR.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_VAR.Text) > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین واریزی مشخص نشده!" });
            }
            //MABL_HAV -----------  MOIN_HAV معین حواله
            if (string.IsNullOrEmpty(CMB_MOIN_HAV.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAV.Text) > 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین حواله مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
            }
            if (!IsNull(this.CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }

            if (IsNull(this.CUST_KIND.SelectedValue) || CUST_KIND.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نوع  مشتري مشخص نشده است ....!" });
            }
            if (IsNull(this.SHIFT.SelectedValue) || SHIFT.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شيفت مشخص نشده است ....!" });
            }
            if (IsNull(this.DEPATMAN.SelectedValue) || DEPATMAN.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد فروش مشخص نشده است ....!" });
            }

            //POSHTEFACTOR
            if (string.IsNullOrEmpty(MOIN_VAR.Text) && Convert.ToInt64(MABL_VAR.Text) > 0) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین واریزی مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(MOIN_VAR.Text) && (string.IsNullOrEmpty(MABL_VAR.Text) || MABL_VAR.Text == "0")) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ واریزی مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(MOIN_HAV.Text) && Convert.ToInt64(MABL_HAV.Text) > 0) //معین حواله
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین حواله مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(MOIN_HAV.Text) && (string.IsNullOrEmpty(MABL_HAV.Text) || MABL_HAV.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ حواله مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MBAA.Text) > 0) //مالیات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مالیات مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MBAA.Text) || MBAA.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ مالیات مشخص نشده!" });
            }
            if (string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_HAZ.Text) || MABL_HAZ.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ خدمات مشخص نشده!" });
            }
            if (!IsNull(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }
            if (!IsNull(this.CMB_HMBAA.SelectedValue.ToStringNullSafe()))
            {
                if (CL_HESABDARI.ISTAF(this.HMBAA.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
                }
            }
            //POSHTEFACTOR

            if (IsExporty) //اگر صادراتی است
            {
                if (string.IsNullOrEmpty(ARZD.Text) || ARZD.Text == "0")
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نرخ ارز خالی یا صفر نمی تواند باشد !" });
                }

                if (ARZKIND2.SelectedValue == null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نوع ارز خالی نمیتواند خالی باشد !" });
                }
            }

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }

            return true;
        }
        private bool BodyIsValid(INVO_LST_FACTOR22 TheRow)
        {
            var ROW = TheRow;

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            // Validate ANBAR
            if (!int.TryParse(TheRow.ANBAR?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "انبار صحیح انتخاب نشده" });
            }
            // Validate CODE
            if (string.IsNullOrEmpty(TheRow.CODE) || TheRow.CODE.Length > 15)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کالا صحیح وارد نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.NAME_CODE))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کالا صحیح وارد نشده" });
            }
            // Validate MEGH
            if (!double.TryParse(TheRow.MEGH?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
            }
            else
            {
                //if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                //{
                //    if (TheRow.MEGH == 0)
                //    {
                //        ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صفر نمیتواند باشد" });
                //    }
                //}
            }
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            if (IsExporty)
            {
                if (!double.TryParse(TheRow.TOTALARZ?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "Line Total صحیح وارد نشده" });
                }
                if (!double.TryParse(TheRow.N_TAF?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "Unit Price وارد نشده" });
                }
            }

            // Validate MANDAH
            if (TheRow.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate MABL
            if (!double.TryParse(TheRow.MABL?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کالا صحیح وارد نشده" });
            }
            // Validate MABL_K
            if (!double.TryParse(TheRow.MABL_K?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کل,  کالا صحیح وارد نشده" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled || BTN_SAVE.Visibility != Visibility.Visible || !BTN_SAVE.IsHitTestVisible) { return; }

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            errors = (from object i in PAY_GETP_SUB.ItemsSource
                      let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                      where c != null && Validation.GetHasError(c)
                      select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            try
            {
                if (NUMBER1.Text == "0") // فقط برای رکوردهای جدید اجرا شود
                {
                    double newNumber1;
                    double newNumber;

                    // تمام عملیات باید در یک اتصال و یک تراکنش واحد انجام شود
                    using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        // استفاده از Serializable برای قفل کردن جدول تا پایان تراکنش
                        using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                        {
                            try
                            {
                                // 1. قفل گذاری روی جدول برای اطمینان از خواندن صحیح MAX
                                db.Execute("SELECT TOP 1 NUMBER FROM dbo.HEAD_LST WITH (TABLOCKX, HOLDLOCK)", null, transaction);

                                // 2. دریافت حداکثر شماره فاکتور (TAG = 12)
                                var rst_11 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER1) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={FTAG}))", null, transaction).FirstOrDefault();
                                if (rst_11 == 0 || rst_11 == null)
                                {
                                    newNumber1 = Baseknow.STHFR; // شماره شروع
                                }
                                else
                                {
                                    newNumber1 = Convert.ToDouble(rst_11 + 1);
                                }

                                // 3. دریافت حداکثر شماره رسید انبار (TAG = 1) (اگر فاکتور مستقیم است)
                                if (IsDirectFactor)
                                {
                                    var rst_12 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={HTAG}))", null, transaction).FirstOrDefault();
                                    if (rst_12 == 0 || rst_12 == null)
                                    {
                                        newNumber = Baseknow.STHFR; // شماره شروع
                                    }
                                    else
                                    {
                                        newNumber = Convert.ToDouble(rst_12 + 1);
                                    }
                                }
                                else
                                {
                                    // اگر مستقیم نیست، از شماره رسید انبار موجود در کمبوباکس استفاده می‌شود
                                    newNumber = Convert.ToDouble(NUMBER.Text);
                                }

                                // 4. درج رکورد فاکتور خرید (FTAG = 12)
                                db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER, NUMBER1, TAG, DATE_N, MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF, UID)
                                      VALUES ({newNumber}, {newNumber1}, {FTAG}, 0, 0, 0, 0, 0, 0, 0, 0, {Baseknow.USERCOD})", null, transaction);

                                // 5. درج رکورد رسید انبار (HTAG = 1) (اگر فاکتور مستقیم است)
                                if (IsDirectFactor)
                                {
                                    db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER, NUMBER1, TAG, DATE_N, MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF, UID)
                                      VALUES ({newNumber}, {newNumber1}, {HTAG}, 0, 0, 0, 0, 0, 0, 0, 0, {Baseknow.USERCOD})", null, transaction);
                                }

                                // 6. ثبت نهایی تراکنش
                                transaction.Commit();

                                // 7. به‌روزرسانی UI *بعد* از ثبت موفقیت‌آمیز
                                NUMBER1.Text = newNumber1.ToString();
                                NUMBER.Text = newNumber.ToString();
                                NUMBER1.UpdateLayout();
                                NUMBER.UpdateLayout();
                            }
                            catch (Exception)
                            {
                                // در صورت بروز هرگونه خطا، کل عملیات لغو می‌شود
                                transaction.Rollback();
                                throw; // ارسال مجدد خطا به بلاک catch بیرونی
                            }
                        } // پایان تراکنش
                    } // پایان اتصال

                    // به‌روزرسانی وضعیت فرم و لاگ‌ها
                    _navigationManager.IsNewRecord = false;
                    RefreshAfterUpdate(); // این متد باید رکوردهای جدید را در ناوبری بارگذاری کند
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"در حال حاضر شماره رسید {NUMBER.Text} توسط کاربر دیگری ثبت شده , شماره رسید دیگری انتخاب کنید").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }

            try
            {
                DoCmdHeaderSave();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"شماره برگه ای را که تغییر داده اید {NUMBER.Text} توسط کاربر دیگری ثبت شده , شماره دیگری انتخاب کنید").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات دخیره , لطفا مجددا امتحان کنید").Show();
                }
                return;
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return;
            }



            this.OKF.IsChecked = true;

            this.INVO_LST_SUB.IsReadOnly = false;
            this.INVO_LST_SUB.IsReadOnly = false;
            this.Page58.IsEnabled = true;

            if (CMB_MOIN_VAR.SelectedValue != null)
            {

            }

            SANAD();

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, FTAG, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }

            DataGridActivation();

            if (!IsDirectFactor && SUM_OF_MABL_K == 0)
            {
                try
                {
                    DataGridHelper.FocusAndEditCell(INVO_LST_SUB, "MABL", 0, true);
                }
                catch { }
            }

            ChangeIsHappend = false;
        }


        private void GetBalancePerson()
        {
            //کادر سبز و سند و مانده حساب
            var SANAD_NUMBER = dbms.DoGetDataSQL<string>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (SANAD_NUMBER != null)
            {
                if (CUST_NO.SelectedValue != null)
                {
                    MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                }
                N_S.Text = SANAD_NUMBER?.ToString();
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S  = 2 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
            }
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!IsNull(NUMBER.Text) && NUMBER.Text != "0")
            {
                if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
                {
                    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                    return;
                }

                SecurityAllCheck();

                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1


                if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
                {
                    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();

                    CUST_NO.IsEnabled = false; //Lock true
                    INVO_LST_SUB.IsReadOnly = true;
                    //New Modify Page58.IsEnabled = false;
                    DATE_N.IsEnabled = false;
                    NUMBER.IsEnabled = false;
                    FNUMCO.IsEnabled = false;
                    MOLAH.IsEnabled = false;

                    BTN_SAVE.IsEnabled = false;

                    this.AllowDeletions = false;
                    this.AllowEdits = false;
                    this.PAY_GETP_SUB.IsEnabled = false;

                    return;
                }
                else
                {
                    CUST_NO.IsEnabled = true; //Lock true
                    INVO_LST_SUB.IsReadOnly = false;
                    Page58.IsEnabled = true;
                    DATE_N.IsEnabled = true;
                    NUMBER.IsEnabled = true;
                    FNUMCO.IsEnabled = true;
                    MOLAH.IsEnabled = true;

                    BTN_SAVE.IsEnabled = true;

                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                    this.PAY_GETP_SUB.IsEnabled = true;
                }

            }

        }
        private void BTN_DELETE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = BTN_DELETE.Visibility == Visibility.Visible;
            if (!BTN_DELETE.IsEnabled || !IsVisible) { return; }

            if (!BTN_DELETE.IsEnabled || NewRecord) { return; }

            Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "فاکتور خرید",
                    recordId: NUMBER1.Text,
                    oldValue: "TAG = 12",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (IsDirectFactor && INVO_LST_SUB.Items.Count > 0)
                {
                    if (!(INVO_LST_SUB.SelectedItems is null))
                    {
                        #region SABEGHEH
                        var dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                        #endregion

                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < INVO_LST_SUB.SelectedItems.Count; i++)
                        {
                            var item = INVO_LST_SUB.SelectedItems[i];

                            if (CL_LMethods.IsNewPlaceHolder(INVO_LST_SUB, item))
                            {
                                continue; // Skip deletion for new placeholder items
                            }

                            var _id_ = item.GetType().GetProperty("id").GetValue(item);

                            if (_id_ != null)
                            {
                                try
                                {
                                    var items = new List<object> { item }; // Wrap the item in a list
                                    var (errorMessages, _, _, _) =
                                        IVM.CheckInventoryAndExecuteQuery<int>(items, $@"DELETE FROM dbo.INVO_LST WHERE id = {_id_}");

                                    ErrosMessages.AddRange(errorMessages);
                                }
                                catch (SqlException ex)
                                {
                                    if (ex.Number == 547)
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد" });
                                    }
                                    else
                                    {
                                        ErrosMessages.Add(new MsgModel { MessageText_U = "خطا پایگاه داده در انجام عملیات حذف" });
                                    }
                                }
                                catch (Exception)
                                {
                                    ErrosMessages.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف" });
                                }

                            }
                        }

                        if (ErrosMessages.Any())
                        {
                            IVM.ShowErrorMessages(ErrosMessages);
                        }

                        INVO_LST_SUB_ReGetData();
                        SANAD();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER1.Text) && NUMBER1.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER1 = {NUMBER1.Text} AND TAG = {FTAG}");

                            SANAD();

                            _navigationManager?.DeleteCurrentRecord(); //Refresh Record Source
                        }
                        catch (SqlException ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این فاکتور دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
                                return;
                            }
                            else
                            {
                                new Msgwin(false, "حذف به دلیل خطا در بروز پایگاه داده انجام نشد!").ShowDialog(); return;
                            }
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "خطا در انجام عملیات حذف!").ShowDialog(); return;
                        }
                        INVO_LST_SUB_ReGetData();
                    }
                }
            }
        }
        private bool DoCmdHeaderSave()
        {
            string _qre = null;

            string _n_s = "NULL";
            if (double.TryParse(N_S.Text, out var n_sVal) && n_sVal > 0)
            {
                _n_s = n_sVal.ToString();
            }


            string _DATEUPDATE_ = $", DATE_N = {DATE_N.Text.ToRawTarikh()} ";
            if (NUMBER.SelectedValue != null)
            {
                var RSTDATE = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 DATE_N FROM HEAD_LST WHERE NUMBER = {NUMBER.SelectedValue} AND TAG = {HTAG}").FirstOrDefault();
                if (!string.IsNullOrEmpty(RSTDATE))
                {
                    _DATEUPDATE_ = null; //برای اینکه دست به تاریخ رسید ثبت شده نزنه
                }
            }


            if (IsDirectFactor)
            {
                _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text} {_DATEUPDATE_} , 
                    TAH = N'{TAH.Text}', MAS = {MAS.Text}, N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', 
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = N'{CMB_HMBAA.SelectedValue}', 
                    ANBAR =  {(ANBAR is null ? "NULL" : ANBAR)},
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    USER_NAME = N'{USER_NAME.Text}', FNUMCO = {FNUMCO.Text},
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} ";
            }
            else
            {
                //برای فاکتور غیر مستقیم
                var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TAH,MOLAH FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG}").FirstOrDefault();

                //DATE_N = {DATE_N.Text.ToRawTarikh()},  تاریخ برای اینکه باهم سینک نشه برداشته شد که رسید انبار تاریخش درست باشه
                _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text},
                    TAH = N'{HEADER_FAC.TAH}', N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{HEADER_FAC.MOLAH}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}' , 
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = N'{CMB_HMBAA.SelectedValue}', 
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    ANBAR =  {(ANBAR is null ? "NULL" : ANBAR)},  FNUMCO = {FNUMCO.Text},
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} "; // USER_NAME = N'{USER_NAME.Text}',
            }

            _ = dbms.DoExecuteSQL(_qre);

            string? _ISOCODE_ = null;
            if (IsExporty)
            {
                if (ARZKIND2.SelectedValue != null)
                {
                    _ISOCODE_ = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 ISOCode FROM dbo.[TCOD_ARZ] WHERE ID = {ARZKIND2.SelectedValue}").FirstOrDefault();
                }
            }

            #region CHECKING
            //MABL_VAR.Text // مبلغ واریزی    | MOIN_VAR.Text  | CMB_MOIN_VAR.SelectedValue
            //MABL_VAR2.Text //مبلغ کارت بانک  | MOIN_VAR2.Text | CMB_MOIN_VAR2.SelectedValue

            //MABL_HAV.Text  //مبلغ بن یا حواله  | MOIN_HAV.Text  | CMB_MOIN_HAV.SelectedValue
            //MABL_HAV2.Text //مبلغ بن یا حواله  | MOIN_HAV2.Text | CMB_MOIN_HAV2.SelectedValue

            //MABL_HAZ.Text //خدمات        | MOIN_HAZ.Text  | CMB_MOIN_HAZ.SelectedValue
            //MBAA.Text     //مالیات        | HMBAA.Text     | CMB_HMBAA.SelectedValue
            #endregion

            _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()}, 
                    TAH = N'{TAH.Text}', MAS = {MAS.Text}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}',
                    MABL_VAR = {MABL_VAR.Text}, MOIN_VAR = N'{CMB_MOIN_VAR.SelectedValue}',
                    MABL_HAV = {MABL_HAV.Text}, MOIN_HAV = N'{CMB_MOIN_HAV.SelectedValue}',
                    FNUMCO = {FNUMCO.Text},
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = N'{CMB_HMBAA.SelectedValue}', 
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    ARZD = {(string.IsNullOrEmpty(ARZD.Text) ? "NULL" : ARZD.Text)},
                    ARZKIND2 = {(string.IsNullOrEmpty(ARZKIND2.SelectedValue.ToStringNullSafe()) ? "NULL" : ARZKIND2.SelectedValue)},
                    ARZCODING = N'{(string.IsNullOrEmpty(_ISOCODE_) ? "NULL" : _ISOCODE_)}',
                    USER_NAME = N'{USER_NAME.Text}',
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} 
                    ";
            _ = dbms.DoExecuteSQL(_qre);

            //// Retrieve input values for NUMBER and TAG
            //int number = Convert.ToInt32(NUMBER.Text);
            //int tag = FTAG; // Assuming FTAG is used as the TAG value
            //                // Check for duplicate
            //var existingRecord = dbms.DoGetDataSQL<int>(
            //    "SELECT COUNT(*) FROM HEAD_LST WHERE NUMBER = @Number AND TAG = @Tag",
            //    new { Number = number, Tag = tag }
            //).FirstOrDefault();

            //if (existingRecord > 0)
            //{
            //    new Msgwin(false, "این شماره فاکتور خرید از قبل ثبت شده و نمیتوانید آن را به عنوان فاکتور جدید ثبت کنید").ShowDialog();
            //    return false;
            //}


            return true;
        }
        private void Summer()
        {
            JJKOL.Text = SUM_OF_MABL_K.ToString(); //SMABLK //جمع فاکتور :
            HKH.Text = MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL.Text; //جمع کل فاکتور برای فسمت روی فاکتور
            Text117.Text = SUM_OF_MEGH_K.ToString(); //جمع مقادیر :

            NCHK.Text = (PAY_GETP_SUB_DATA.Sum(x => x.MABL) ?? 0).ToString(); //جمع مبالغ چکهای پرداختی

            //مبلغ قابل پرداخت: //= [JF] + [HKH] - [NTKHFIF] + [MBAA]
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            //جمع مبالغ پرداختی
            //=[M_NAGHD]+[MABL_VAR]+[MABL_HAV]+[NCHK]

            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(MABL_VAR.Text) + Convert.ToInt64(MABL_HAV.Text) + Convert.ToInt64(NCHK.Text);
            NPAR.Text = RMP.ToString();


            //=[GHABEL]-[NPAR]
            MAN.Text = Convert.ToString(Convert.ToInt64(GHABEL.Text) - Convert.ToInt64(NPAR.Text)); //مانده
        }

        private void NUMBER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NUMBER.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (NUMBER.Text == "+")
            {
                //DoCmd.OpenForm("RASIDLIST", acFormDS); //#Check
            }

            if (NUMBER.SelectedValue is null)
            {
                NUMBER.Text = "0";
                //e.Handled = true; //Prevent leaving ComboBox untill it fix it
            }
            else
            {
                bool isNewInvoice = NewRecord || NUMBER.SelectedValue == null || NUMBER.Tag == null;

                //اکر در فاکتور از قبل ثبت شده شماره رسید را تغییر داده بود
                if (!isNewInvoice && NUMBER.SelectedValue != NUMBER.Tag)
                {
                    new Msgwin(false, "نمیتوانید رسیدی که قبلا ثبت کرده ای ا تغییر دهید , تنها میتوانید این فاکتور را حذف نمایید , انتخاب رسید انبار تنها در فاکتور جدید ممکن است").ShowDialog();
                    NUMBER.SelectedValue = NUMBER.Tag; NUMBER.Items.Refresh();
                }
                else
                {
                    INVO_LST_SUB_ReGetData();

                    //واحد رو از رسید بگیره که فاکتورش هم بشه همون واحد صادر کننده رسید که بتونه بعدا فاکتور رو ببینه
                    var _DEPATMAN_ = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 DEPATMAN FROM HEAD_LST WHERE NUMBER = {NUMBER.SelectedValue} AND TAG = {HTAG}").FirstOrDefault();
                    if (_DEPATMAN_ != null)
                    {
                        DEPATMAN.SelectedValue = _DEPATMAN_; DEPATMAN.Items.Refresh();
                    }
                }
            }
        }
        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null; PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (!NewRecord && PERSONEL.SelectedItem != null)
            {
                string SelectedTextCMB = ((COMBOPERSONEL)PERSONEL.SelectedItem).SAL_NAME.ToStringNullSafe();

                Meidnum = CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خرید  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($"ارجاع داده به {SelectedTextCMB} شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }
        private void ActivateChaps()
        {
            if (SGN2.IsChecked ?? false)
            {
                this.Command100.IsEnabled = true;
                this.Command106.IsEnabled = true;
                this.Command108.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command106.IsEnabled = false;
                this.Command108.IsEnabled = false;
            }
        }
        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG} )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور خرید  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text.ToRawTarikh() + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG} )");
            }

            CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خريد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if ((bool)!this.OKF.IsChecked)
                this.OKF.IsChecked = true;

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();
            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(this.SGN1.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + ",12 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور خرید  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + ",12 )");
            }
            CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خريد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;
            this.SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();

            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(this.SGN2.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + ",12 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور خرید  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + $",{FTAG}," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), FTAG);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{FTAG}," + this.NUMBER.Text + ",12 )");
            }
            CL_HESABDARI.PERSONELUpdate(FTAG, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور خريد  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            this.SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            ActivateChaps();
            // آبديت سربرگ
            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(this.SGN3.IsChecked == true, 1, 0) + $"  WHERE  TAG = {FTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void WinSignActivator()
        {
            if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
            {
                AllowEdits = false;
                AllowDeletions = false;

                //New Modify Page58.IsEnabled = false;
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                AllowEdits = true;
            }
        }


        private void FNUMCO_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (NewRecord) { return; }

            if (IsNull(this.FNUMCO.Text))
            {
                new Msgwin(false, "شماره فاكتور فروشنده خالي است").ShowDialog();
            }
            else
            {
                if (IsNull(ANBAR))
                {
                    var RST = dbms.DoGetDataSQL<int?>($"SELECT MAX(ANBAR) AS MaxOfNUMBER FROM dbo.HEAD_LST WHERE (TAG = {FTAG}) ").FirstOrDefault();
                    if (RST == null || RST == 0)
                    {
                        this.ANBAR = Baseknow.STHKH;
                    }
                    else
                    {
                        this.ANBAR = RST + 1;
                    }

                    new Msgwin(false, "تمام فاكتورهاي خريد ثبت شده با شماره فاكتور فروشنده : !" + this.FNUMCO.Text + "   باشماره صورتحساب خريد كالا : " + this.ANBAR + "ثبت گرديد ...").ShowDialog();
                }
                dbms.DoExecuteSQL("UPDATE dbo.HEAD_LST SET ANBAR = " + (ANBAR is null ? "NULL" : ANBAR) + $" WHERE (TAG = {FTAG}) AND (FNUMCO = " + this.FNUMCO.Text + ") AND (CUST_NO = N'" + this.CUST_NO.SelectedValue + "')");
                //DoCmd.OpenReport("groop_invoice", acPreview, "", "ANBARAS =" + this.ANBAR);
            }
        }
        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (CUST_NO.SelectedValue is not null)
            {
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("HEAD_LST_KHAREED1", I_AM_KHAREED);//Search Plusy Form Specialy for Customers
                CMBSearch.ShowDialog();
                if (CUST_NO.SelectedValue is null)
                {
                    return;
                }
            }
            else if (Information.IsNumeric(CUTSNO_TEX.Text))
            {
                try
                {
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " and NUMBER = 1 and tNUMBER = " + CUTSNO_TEX.Text).ToList();
                    if (rst.Count == 1)
                    {
                        var _data_hes = rst.FirstOrDefault()?.n_kol + "-" + rst.FirstOrDefault()?.NUMBER + "-" + rst.FirstOrDefault()?.tNUMBER;
                        var _data_name = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE hes = N'{_data_hes}'").FirstOrDefault();
                        if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == _data_hes))
                        {
                            ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = _data_hes, NAME = _data_name });
                        }
                        CUST_NO.Items.Refresh();
                        CUST_NO.SelectedValue = null;
                        this.CUST_NO2.SelectedValue = _data_hes;
                        //CUST_NO_AfterUpdate();
                    }
                    else
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.Text = null;
                        CUST_NO.Items.Refresh();
                        return;
                    }
                }
                catch (Exception) { }
            }
            else
            {
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + CUTSNO_TEX.Text + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;
                    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                    {
                        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                    }
                    CUST_NO.SelectedValue = null;
                    CUST_NO.SelectedValue = thevalue;
                    CUST_NO.Items.Refresh();
                }
                else
                {
                    CUST_NO.SelectedValue = null;
                    CUST_NO.Text = null;
                    CUST_NO.Items.Refresh();
                    return;
                }
            }

            #region CUST_NO_Exit
            if (CUST_NO.SelectedValue is not null)
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    CUST_NO.SelectedValue = null;
                }
                if (Convert.ToBoolean(Baseknow.SAGHF) || Convert.ToBoolean(Baseknow.SAGHF2))
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.CUST_NO.SelectedValue.ToString())) == false)
                    {
                        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                        msgwin.ShowDialog();
                        CUST_NO.SelectedValue = null;
                    }
                }
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO2.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            #endregion

        }
        private void SANAD()
        {
            try
            {
                var _SanadNumber_ = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADKHAREED(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

                if (_SanadNumber_ != null)
                {
                    N_S.Text = _SanadNumber_.ToString();
                }

                Summer();

                GetBalancePerson();
            }
            catch (Exception ex)
            {
                AUTO_BAZ.Functions.CL_LMethods.LogWriter.WriteLog($"GENSANADKHAREED exception for invoice {NUMBER.Text}: {ex.Message}");
                AUTO_BAZ.Functions.CL_LMethods.ExpectionLogWriter.WriteLog(ex, "GENSANADKHAREED");
                new Msgwin(false, "خطا در انجام علمیات صدور سند برای فاکتور خرید").Show();
            }

        }

        #region POSHTE_FACTOR
        public PAY_GETP_MODEL? PAY_GETP_SUB_WAS_ROW_ITEM { get; set; }
        public void PAY_GETP_SUB_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //PAY_GETP_SUB_DATA
                PAY_GETP_SUB_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<PAY_GETP_MODEL>($@"SELECT * FROM PAY_GETP WHERE NUMBER = {NUMBER.Text} AND TAG = {HTAG} AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        PAY_GETP_SUB_DATA.Add(item);
                    }
                    NCHK.Text = (PAY_GETP_SUB_DATA?.Sum(x => x.MABL) ?? 0).ToString(); //جمع مبالغ چکهای پرداختی
                }
            }
        }

        private bool PayGetpBodyIsValid(PAY_GETP_MODEL TheRow)
        {
            var errors = (from object i in PAY_GETP_SUB.ItemsSource
                          let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();
            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return false;
            }


            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrEmpty(TheRow.N_HESAB))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب وارد (جاری چک) نشده!" });
            }
            if (!double.TryParse(TheRow.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(TheRow?.N_SERI?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
            }
            if (!int.TryParse(TheRow.DATE?.ToString(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
            }
            if (!double.TryParse(TheRow.BANK?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
            }
            if (string.IsNullOrEmpty(TheRow.BANK?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
            }
            if (!double.TryParse(TheRow.DATE_S?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
            }
            if (!int.TryParse(TheRow.MABL?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.MABL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
            }

            if (!int.TryParse(TheRow.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_KOL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
            }
            else
            {
                if (!int.TryParse(TheRow.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_MOIN?.ToString()))
                {
                    //ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
                }
                else
                {
                    if (TheRow.N_TAF is not null)
                    {
                        if (!int.TryParse(TheRow.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(TheRow?.N_TAF?.ToString()))
                        {
                            ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
                        }
                    }
                }
            }


            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                return false;
            }

            return true;
        }

        private void PAY_GETP_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (PAY_GETP_SUB.SelectedItem != null)
            {
                if (PAY_GETP_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    PAY_GETP_SUB_WAS_ROW_ITEM = ((PAY_GETP_MODEL)PAY_GETP_SUB.SelectedItem).Clone() as PAY_GETP_MODEL;
                }
            }
        }
        private void PAY_GETP_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region REFILL_CURRENTS_
            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

            var PAY_GETP_SUB_ROW_INDEX = row_index;

            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            object PAY_GETP_SUB_ENTERED_VALUE;
            if (!ReferenceEquals(Comboval, null))
                PAY_GETP_SUB_ENTERED_VALUE = Comboval.SelectedValue;
            else
                PAY_GETP_SUB_ENTERED_VALUE = TexboVal.Text.Trim();

            var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;
            #endregion

            #region SET_NULL_IF_ROW_IS_NOT_VALID
            //بررسی در صورت تغییر نال کردن برای جلوگیری از اشتباه
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (PAY_GETP_SUB_WAS_ROW_ITEM.N_KOL != PAY_GETP_SUB_ROW_ITEMS.N_KOL) //تغییر یافته
                {
                    //معین بانک
                    var comboBox = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_MOIN").GetCellContent(e.Row) as ComboBox;
                    comboBox.ItemsSource = null;

                    //تفضیلی
                    var comboBox1 = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (PAY_GETP_SUB_WAS_ROW_ITEM.N_MOIN != PAY_GETP_SUB_ROW_ITEMS.N_MOIN) //تغییر یافته
                {
                    //تفضیلی
                    var comboBox1 = PAY_GETP_SUB.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            #endregion

            //,N_MOIN,N_TAF 
            if (e.Column.SortMemberPath == "N_KOL")
            {
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
            }
            if (e.Column.SortMemberPath == "N_TAF")
            {
            }
            if (e.Column.SortMemberPath == "BANK")
            {

                #region BAN_AfterUpdate
                if (!IsNull(PAY_GETP_SUB_ROW_ITEMS?.N_SERI) && !IsNull(PAY_GETP_SUB_ROW_ITEMS?.BANK))
                {
                    if (PAY_GETP_SUB_ROW_ITEMS?.ID == null || PAY_GETP_SUB_ROW_ITEMS?.BANK != PAY_GETP_SUB_WAS_ROW_ITEM?.BANK || PAY_GETP_SUB_ROW_ITEMS?.N_SERI != PAY_GETP_SUB_WAS_ROW_ITEM?.N_SERI)
                    {
                        var filter = "N_SERI=" + PAY_GETP_SUB_ROW_ITEMS.N_SERI + " AND BANK = " + PAY_GETP_SUB_ROW_ITEMS.BANK;
                        var rst = dbms.DoGetDataSQL<PAY_GETP>($"SELECT * FROM PAY_GETP WHERE {filter}  ").FirstOrDefault();
                        if (rst != null)
                        {
                            new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود").ShowDialog();

                            var rst2 = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.ADA + "' OR HES = '" + Baseknow.ADV + "' ) AND (BES > 0) AND (BANK = "
                                + PAY_GETP_SUB_ROW_ITEMS?.BANK + ") AND (N_SERI = " + PAY_GETP_SUB_ROW_ITEMS.N_SERI + ")").FirstOrDefault();
                            if (rst2 != null)
                            {
                                new Msgwin(false, "اين چك در سند شماره " + rst2 + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را تغییر دهید").ShowDialog();
                            }
                            else
                            {
                                PAY_GETP_SUB_ROW_ITEMS.ID = rst.ID; //برای اینکه آپدیت بشه نه INSERT

                                PAY_GETP_SUB_ROW_ITEMS.N_SERI = rst.N_SERI;
                                PAY_GETP_SUB_ROW_ITEMS.BANK = rst.BANK;
                                PAY_GETP_SUB_ROW_ITEMS.DATE_S = rst.DATE_S;
                                PAY_GETP_SUB_ROW_ITEMS.RADIF = rst.RADIF;
                                PAY_GETP_SUB_ROW_ITEMS.SHOBEH = rst.SHOBEH;
                                PAY_GETP_SUB_ROW_ITEMS.DATE = rst.DATE;
                                PAY_GETP_SUB_ROW_ITEMS.NAME_TAH = rst.NAME_TAH;
                                PAY_GETP_SUB_ROW_ITEMS.N_HESAB = rst.N_HESAB;
                                PAY_GETP_SUB_ROW_ITEMS.MABL = rst.MABL;
                                PAY_GETP_SUB_ROW_ITEMS.N_KOL = rst.N_KOL;
                                PAY_GETP_SUB_ROW_ITEMS.N_MOIN = rst.N_MOIN;

                                if (rst?.N_KOL != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL = rst?.N_KOL;
                                if (rst?.N_MOIN != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN = rst?.N_MOIN;
                                if (rst?.N_TAF != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF = rst?.N_TAF;
                                if (rst?.N_TAF2 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF2 = rst?.N_TAF2;
                                if (rst?.N_TAF3 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF3 = rst?.N_TAF3;

                                if (PAY_GETP_SUB_ROW_ITEMS?.N_KOL?.ToString() == "911") //از نوع حذف شده انتظامی
                                {
                                    if (PAY_GETP_SUB_ROW_ITEMS?.N_KOL?.ToStringNullSafe() != Baseknow.BANKHA?.ToStringNullSafe())
                                    {
                                        if (rst?.N_KOL != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL = null;
                                        if (rst?.N_MOIN != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN = null;
                                        if (rst?.N_TAF != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF = null;
                                    }

                                    if (rst?.N_KOL2 != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL2 = null;
                                    if (rst?.N_MOIN2 != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN2 = null;
                                    if (rst?.N_TAF2 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF2 = null;

                                    if (rst?.N_KOL3 != null) PAY_GETP_SUB_ROW_ITEMS.N_KOL3 = null;
                                    if (rst?.N_MOIN3 != null) PAY_GETP_SUB_ROW_ITEMS.N_MOIN3 = null;
                                    if (rst?.N_TAF3 != null) PAY_GETP_SUB_ROW_ITEMS.N_TAF3 = null;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            if (e.Column.SortMemberPath == "DATE_S") //تاریخ سررسید
            {
                //if (CL_HESABDARI.CHEKDATEM((long)PAY_GETP_SUB_ROW_ITEMS.DATE_S, false) is true) //تاریخ صحیح نیست
                //{
                //    PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                //}
                string date_n_val = PAY_GETP_SUB_ROW_ITEMS.DATE_S.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                        universControl.PopNotifyShow("تاریخ سررسید صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
                else
                {
                    PAY_GETP_SUB_ROW_ITEMS.DATE_S = null;
                    universControl.PopNotifyShow("تاریخ سررسید نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            if (e.Column.SortMemberPath == "DATE") //تاريخ دريافت
            {
                string date_n_val = PAY_GETP_SUB_ROW_ITEMS.DATE.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                        universControl.PopNotifyShow("تاريخ دريافت صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                    else
                    {
                        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                        {
                            PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                            universControl.PopNotifyShow(".تاريخ دريافت به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                            return;
                        }
                    }
                }
                else
                {
                    PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                    universControl.PopNotifyShow("تاريخ دريافت نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                //if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(PAY_GETP_SUB_ROW_ITEMS.DATE), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                //{
                //    PAY_GETP_SUB_ROW_ITEMS.DATE = null;
                //}
            }
            if (e.Column.SortMemberPath == "SANDUGH")
            {
                //در RowEnd لاگ میزنم
                //rst.Open("dbo.PAY_GETP_SUB_LOG", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //rst.AddNew();
                //rst.update();
            }
            if (e.Column.SortMemberPath == "VAZ")
            {
            }
            if (e.Column.SortMemberPath == "SAYADI")
            {
                if (!PayGetpBodyIsValid(PAY_GETP_SUB_ROW_ITEMS))
                {
                    PAY_GETP_SUB.Dispatcher.InvokeAsync(() =>
                    {
                        PAY_GETP_SUB.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                        PAY_GETP_SUB.CancelEdit(DataGridEditingUnit.Cell);
                        PAY_GETP_SUB.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                    });
                    return;
                }
            }
            //DATE - تاريخ دريافت   |   DATE_S - تاريخ سررسيد
        }
        private void PAY_GETP_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            #region WORKS
            //var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;
            //if (n_MOINColumn.ItemsSource is null) //MOIN
            //{
            //    if (PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null)
            //    {
            //        //معین بانک
            //        n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETP_SUB_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //    }
            //}
            //if (n_TAFColumn.ItemsSource is null) //TAFZILY
            //{
            //    if (PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null && PAY_GETP_SUB_ROW_ITEMS.N_MOIN is not null)
            //    {
            //        //تفضیلی
            //        n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) ={PAY_GETP_SUB_ROW_ITEMS.N_MOIN}) AND ((TDETA_HES.N_KOL) ={PAY_GETP_SUB_ROW_ITEMS.N_KOL}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
            //    }
            //}
            #endregion

            var PAY_GETP_SUB_ROW_ITEMS = e.Row.Item as PAY_GETP_MODEL;

            int DefVale = 0;
            ComboBox THE_COMBO = e.EditingElement as ComboBox;

            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (!(e.EditingElement is null) && PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //معین بانک
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETP_SUB_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
                    if (DefVale <= 0)
                    {
                        THE_COMBO.SelectedIndex = 0;
                    }
                    else
                    {
                        THE_COMBO.SelectedValue = DefVale;
                    }
                }
            }
            if (e.Column.SortMemberPath == "N_TAF")
            {
                if (!(e.EditingElement is null) && PAY_GETP_SUB_ROW_ITEMS.N_KOL is not null && PAY_GETP_SUB_ROW_ITEMS.N_MOIN is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //تفضیلی
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) =" + PAY_GETP_SUB_ROW_ITEMS.N_MOIN + ") AND ((TDETA_HES.N_KOL) =" + PAY_GETP_SUB_ROW_ITEMS.N_KOL + "))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
                    if (DefVale is 0)
                    {
                        THE_COMBO.SelectedIndex = 0;
                    }
                    else
                    {
                        THE_COMBO.SelectedValue = DefVale;
                    }
                }
            }

        }
        private void PAY_GETP_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (PAY_GETP_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = PAY_GETP_SUB.CurrentCell.Column.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_CHKPOSHT_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "MABL")
                {
                    e.Handled = true;
                    var text = "000";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
            if (e.Key == Key.Subtract)
            {
                if (CURRENT_COLUMN_NAME is "MABL")
                {
                    e.Handled = true;
                    var text = "00";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;

                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
        }
        private void PAY_GETP_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            var FINAL_CROW_ITEM = (e.Row.Item as PAY_GETP_MODEL);

            var DG = PAY_GETP_SUB;

            if (e.Row.Item == null)
            {
                return;
            }

            if (!PayGetpBodyIsValid(FINAL_CROW_ITEM))
            {
                DG.Dispatcher.InvokeAsync(() =>
                {
                    DG.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                    DG.CancelEdit();
                    DG.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                });
                return;
            }

            #region Form_BeforeInsert
            var rst = dbms.DoGetDataSQL<string?>("SELECT TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.TNUMBER) = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + " ) And ((TDETA_HES.NUMBER) = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") And ((TDETA_HES.N_KOL) = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + " )) GROUP BY TDETA_HES.NAME").ToList();
            if (rst.Count > 0)
            {
                FINAL_CROW_ITEM.NAME_TAH = rst.FirstOrDefault();
            }
            #endregion

            var N_SERI = FINAL_CROW_ITEM.N_SERI;
            var BANK = FINAL_CROW_ITEM.BANK;
            var DATE_S = FINAL_CROW_ITEM.DATE_S;


            long? ID = null;
            try
            {
                if (FINAL_CROW_ITEM?.ID == null) //INSERT
                {
                    ID = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.PAY_GETP 
                                                   (
                                                    N_SERI, 
                                                    BANK, 
                                                    DATE_S, 
                                                    DATE, 
                                                    SHOBEH, 
                                                    MABL, 
                                                    NAME_TAH, 
                                                    N_HESAB, 
                                                    N_KOL, 
                                                    N_MOIN, 
                                                    N_TAF, 
                                                    NUMBER, 
                                                    TAG, 
                                                    KIND, 
                                                    SAYADI ) 
                                                    OUTPUT INSERTED.ID 
                                                    VALUES (
                                                    {N_SERI},
                                                    {BANK},
                                                    {DATE_S},
                                                    {FINAL_CROW_ITEM.DATE},
                                                    N'{FINAL_CROW_ITEM.SHOBEH}',
                                                    {FINAL_CROW_ITEM.MABL},
                                                    N'{FINAL_CROW_ITEM.NAME_TAH}',
                                                    N'{FINAL_CROW_ITEM.N_HESAB}',
                                                    {FINAL_CROW_ITEM.N_KOL},
                                                    {FINAL_CROW_ITEM.N_MOIN},
                                                    {FINAL_CROW_ITEM.N_TAF},
                                                    {NUMBER.Text},
                                                    {HTAG},
                                                    {FINAL_CROW_ITEM.KIND},
                                                    N'{FINAL_CROW_ITEM.SAYADI}'
                                                )").FirstOrDefault();

                    //OUTPUT INSERTED.ID (place is exactly before 'VALUES')
                    if (ID != null)
                    {
                        FINAL_CROW_ITEM.ID = ID;
                    }
                }
                else //UPDATE
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP
                                         SET N_SERI = {N_SERI}, BANK = {BANK}, DATE_S = {DATE_S}, DATE = {FINAL_CROW_ITEM.DATE},
                                         SHOBEH = N'{FINAL_CROW_ITEM.SHOBEH}',
                                         MABL = {FINAL_CROW_ITEM.MABL}, NAME_TAH = N'{FINAL_CROW_ITEM.NAME_TAH}',
                                         N_HESAB = N'{FINAL_CROW_ITEM.N_HESAB}', N_KOL = {FINAL_CROW_ITEM.N_KOL},
                                         N_MOIN = {FINAL_CROW_ITEM.N_MOIN}, N_TAF = {FINAL_CROW_ITEM.N_TAF}, 
                                         NUMBER = {NUMBER.Text}, TAG = {HTAG},
                                         KIND = {FINAL_CROW_ITEM.KIND},
                                         SAYADI = N'{FINAL_CROW_ITEM.SAYADI}'
                                         WHERE ID = {FINAL_CROW_ITEM.ID}");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, " این چک تکراری است!!").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog();
                }
                DG.Dispatcher.Invoke(() =>
                {
                    DG.CellEditEnding -= PAY_GETP_SUB_CellEditEnding;
                    DG.RowEditEnding -= PAY_GETP_SUB_RowEditEnding;
                    DG.CancelEdit();
                    DG.RowEditEnding += PAY_GETP_SUB_RowEditEnding;
                    DG.CellEditEnding += PAY_GETP_SUB_CellEditEnding;
                });
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ").ShowDialog();
                return;
            }


            SANAD();
        }

        private void DELETE_CHKPOSHT_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_CHKPOSHT.Visibility == Visibility.Visible;
            if (!DELETE_CHKPOSHT.IsEnabled || !IsVisible) { return; }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (PAY_GETP_SUB.Items.Count > 0 && PAY_GETP_SUB.SelectedItem != null)
            {
                if (!(PAY_GETP_SUB.SelectedItems is null))
                {
                    bool errors = default;
                    errors = (from object i in PAY_GETP_SUB.ItemsSource
                              let c = PAY_GETP_SUB.ItemContainerGenerator.ContainerFromItem(i)
                              where c != null && Validation.GetHasError(c)
                              select c).Any();

                    if (errors)
                    {
                        universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                        return;
                    }

                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        var dt = DateTime.Now;
                        CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                        CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1
                        CL_HESABDARI.TR("PAY_GETP", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {HTAG})", dt, 1); //1

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور خرید=> چک های دریافتی پشت فاکتور",
                                recordId: NUMBER.Text,
                                oldValue: "TAG = 12",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();

                        for (int i = 0; i < PAY_GETP_SUB.SelectedItems.Count; i++)
                        {
                            var item = PAY_GETP_SUB.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                if (CL_LMethods.IsNewPlaceHolder(PAY_GETP_SUB, item))
                                {
                                    continue; // Skip deletion for new placeholder items
                                }

                                var _ID_ = item.GetType().GetProperty("ID").GetValue(item);

                                if (_ID_ != null)
                                {

                                    var THE_N_SERI = item.GetType().GetProperty("N_SERI").GetValue(item);
                                    var THE_BANK = item.GetType().GetProperty("BANK").GetValue(item);

                                    var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE  ID = " + _ID_).ToList();
                                    if (rst.Count > 0)
                                    {
                                        if ((!IsNull(rst.FirstOrDefault().N_KOL2) && rst.FirstOrDefault().N_KOL2 != 911) || !IsNull(rst.FirstOrDefault().N_KOL3))
                                        {
                                            Msgwin msgwin1 = new Msgwin(false, "چكي كه وصولي يا واگذاري يا برگشتي خورده قابل حذف نيست");
                                            msgwin1.ShowDialog();
                                        }
                                        else
                                        {
                                            if ((rst.FirstOrDefault().N_KOL == Baseknow.BANKHA || rst.FirstOrDefault().N_KOL == 911) || IsNull(rst.FirstOrDefault().N_KOL))
                                            {
                                                string _where = " WHERE  ID = " + _ID_;

                                                rst.FirstOrDefault().N_KOL = 911;
                                                rst.FirstOrDefault().N_MOIN = 1;
                                                rst.FirstOrDefault().N_TAF = 1;
                                                rst.FirstOrDefault().HES1 = "911-1-1";

                                                dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET N_KOL = 911 , N_MOIN = 1 , N_TAF = 1 , HES1 = N'911-1-1' {_where} ");
                                                IsDeleteSomthing = true;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
                                return;
                            }
                        }
                        if (IsDeleteSomthing is true)
                        {
                            PAY_GETP_SUB_SUB_ReGetData();
                            SANAD();
                        }
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
            }
        }
        #endregion

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, FTAG);
            //new FACTORS_LST(FTAG).Show(); //فاکتور خرید
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void MABL_HAV_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(this.MOIN_HAV.Text))
            {
                new Msgwin(false, "حساب مربوط به برگه رسید مشخص نشده است حتما بايد حساب مربوط به رسید مشخص شود ").ShowDialog();
                this.MOIN_HAV.Focus();
            }
            if (Convert.ToDouble(MABL_HAV.Text) == 0)
            {
                this.MOIN_HAV.Text = "";
            }
            //CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text), Convert.ToDouble(MABL_VAR.Text), Convert.ToDouble(MABL_HAV.Text), (bool)TICMBAA.IsChecked); //#CheckMatter
        }
        private void MABL_HAZ_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAZ.Text) != 0 && (IsNull(this.MOIN_HAZ.Text) || MOIN_HAZ.Text == "0"))
            {
                var RST = dbms.DoGetDataSQL<string?>("SELECT RTRIM(CAST(N_KOL AS nvarchar)) + '-' + RTRIM(CAST(NUMBER AS nvarchar)) + '-' + RTRIM(CAST(TNUMBER AS nvarchar)) AS hes FROM dbo.TDETA_HES WHERE (N_KOL = " + Baseknow.HKHARID + $") AND (NUMBER = {HTAG})").FirstOrDefault();
                if (RST != null)
                {
                    MOIN_HAZ.Text = RST;
                }
                else
                {
                    new Msgwin(false, "حساب معين براي خدمات تعريف نشده است . براي تعريف حساب معين از منوي تعاريف  -تعريف حسابهاي كل و معين - را انتخاب نموده و براي حساب كل هزينه خريد معين تعريف نمائيد.").ShowDialog();
                }
            }
        }
        private void MABL_HAZ_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            MABL_HAZ_AfterUpdate();
        }
        private void MOIN_HAZ_BeforeUpdate()
        {
            //MOIN_HAZ_BeforeUpdate
            if (!IsNull(this.CMB_MOIN_HAZ.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!").ShowDialog();
                }
            }
        }
        private void MOIN_HAZ_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            MOIN_HAZ_BeforeUpdate();
        }
        private void MBAA_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MBAA_AfterUpdate
            if (Strings.Right(this.MBAA.Text, 1) == "%")
            {
                MBAA.Text = Math.Round((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * Convert.ToDouble(MBAA.Text)).ToString();
            }
            if (Convert.ToDouble(MBAA.Text) - Math.Round(Convert.ToDouble(MBAA.Text)) != 0)
            {
                MBAA.Text = Math.Round(Convert.ToDouble(MBAA.Text)).ToString();
            }

            if (Convert.ToDouble(MBAA.Text) > 0 & IsNull(HMBAA.Text))
            {
                HMBAA.Text = Baseknow.HESMBAA;
            }
        }
        private void PARAMS_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (!IsNull(CURRENT_ITEMS_ROW?.id != null))
            {
                e.Handled = true;
                var button = (Button)sender;
                var ROW = button.Tag as INVO_LST_FACTOR22;

                if (ROW?.id != null)
                {
                    var _id_ = dbms.DoGetDataSQL<long?>("SELECT ID FROM dbo.IVO_EXTENDED WHERE id=" + ROW?.id).SingleOrDefault();
                    if (_id_ == null)
                    {
                        dbms.DoExecuteSQL("INSERT INTO [dbo].[IVO_EXTENDED] VALUES(" + ROW.id + ",0,0,0,0,0,0,0,0,0,0,GETDATE()," + Baseknow.USERCOD + ")");
                    }
                    new ZF_IVO_EXTENDED((int)ROW?.id, I_AM_KHAREED).ShowDialog();
                }
            }
        }
        private void TAKHFIF_PERCENT_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TAKHFIF_PERCENT.Text))
            {
                var (isvalid, msg) = CL_LMethods.IsValidPercentage(TAKHFIF_PERCENT.Text);
                if (!isvalid)
                {
                    new Msgwin(false, msg).ShowDialog();
                }
                else
                {
                    TAKHFIF_MABL_PRICE(false);
                }
            }
        }
        private void TAKHFIF_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            TAKHFIF_MABL_PRICE(true);
        }

        private void TAKHFIF_MABL_PRICE(bool isTakhfifFocus = true)
        {
            Summer();

            if (!string.IsNullOrEmpty(TAKHFIF.Text) && TAKHFIF.Text != "0" && JF.Text != "0" && isTakhfifFocus) //درصد تخفیف
            {
                var TAKHFIF_TXT = Convert.ToDouble(TAKHFIF.Text);
                var JF_TXT = Convert.ToDouble(JF.Text);

                TAKHFIF_PERCENT.Text = Math.Round(TAKHFIF_TXT * 100 / JF_TXT, 2).ToString(); //Text101
            }
            else if (!string.IsNullOrEmpty(TAKHFIF_PERCENT.Text)) //مبلغ تخفیف
            {
                var DARSAD_TXT = Convert.ToDouble(TAKHFIF_PERCENT.Text); //Text101
                var JF_TXT = Convert.ToDouble(JF.Text);

                TAKHFIF.Text = Math.Round(JF_TXT * DARSAD_TXT / 100).ToString();
            }
        }

        private string BEFOREDATEN;
        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = BEFOREDATEN;
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = BEFOREDATEN;
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
            }
            else
            {
                DATE_N.Text = BEFOREDATEN;
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

        }
        private void DATE_N_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE_N.Text.ToRawTarikh();
        }

        private async void Command100_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }

            if (IsExporty)
            {
                OpenInterNationalInvoice();
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_KHAREED_1.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            double JCHK = 0, JAMF = 0, HAZ = 0, NAGHD = 0, VAR = 0, HAV = 0, taf = 0, MBAA = 0;
            // Fetch check data
            var rst = dbms.DoGetDataSQL<CheckData>($@"
                                                      SELECT PAY_GETP.N_SERI, TCOD_BANKS.NAMES, PAY_GETP.SHOBEH, PAY_GETP.DATE, PAY_GETP.DATE_S, 
                                                      PAY_GETP.MABL, PAY_GETP.NUMBER, PAY_GETP.TAG 
                                                      FROM TCOD_BANKS 
                                                      INNER JOIN PAY_GETP ON TCOD_BANKS.CODE = PAY_GETP.BANK 
                                                      WHERE PAY_GETP.NUMBER = {NUMBER.Text} AND PAY_GETP.TAG = {HTAG} AND (N_KOL IS NULL OR N_KOL <> 911)").ToList();

            if (rst.Any())
            {
                string commCaption = $"چكهاي پرداخت شده {rst.Count} فقره جمعاًبه مبلغ : {NCHK.Text:### ريال}  ";
                foreach (var check in rst)
                {
                    commCaption += $"ـ سريال:{check.N_SERI} بانك:{check.NAMES} شعبه:{check.SHOBEH.Trim()}";
                    JCHK += Convert.ToDouble(check.MABL);
                }
                (report.GetComponentByName("COMM") as StiText).Text = commCaption;
            }

            if (Baseknow.TFSAZMAN != "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = true;
            }
            else
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
            }
            if (Baseknow.TFSAZMAN != "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());

                //var rst_0 = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(this.CUST_NO.SelectedValue.ToString()) + ")").ToList();
                //if (rst_0.Count == 0)
                //{
                //    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                //}
                //else
                //{
                //    var _mandah = Interaction.IIf(rst_0.FirstOrDefault() > 0, Strings.Format(rst_0.FirstOrDefault(), "##,# ريال بدهكار"), Strings.Format(rst_0.FirstOrDefault() * -1, "##,# ريال بستانكار"));
                //    (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                //}
            }

            // Calculate JAMF
            var jst = dbms.DoGetDataSQL<double?>($@"
                                                    SELECT SUM(INVO_LST.MABL_K) AS SumOfMABL_K 
                                                    FROM INVO_LST 
                                                    WHERE INVO_LST.NUMBER = {NUMBER.Text} AND INVO_LST.TAG = {HTAG}").FirstOrDefault();

            JAMF = jst ?? 0;

            // Fetch HEAD_LST data
            var headLst = dbms.DoGetDataSQL<HeadLstData>($@"
                                                           SELECT NUMBER, TAG AS htag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, 
                                                           M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, 
                                                           MOIN_KHF, ANBARF, FNUMCO, MBAA 
                                                           FROM HEAD_LST 
                                                           WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();

            if (headLst != null)
            {
                HAZ = headLst.MABL_HAZ;
                VAR = headLst.MABL_VAR;
                HAV = headLst.MABL_HAV;
                NAGHD = headLst.M_NAGHD;
                taf = headLst.TAKHFIF;
                MBAA = headLst.MBAA;
            }

            // Update report components
            (report.GetComponentByName("JF") as StiText).Text = JAMF.ToString("#,##0;#,##0-");
            (report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString("#,##0;#,##0-");
            (report.GetComponentByName("MBAA") as StiText).Text = MBAA.ToString("#,##0;#,##0-");
            (report.GetComponentByName("TF") as StiText).Text = taf.ToString("#,##0;#,##0-");
            (report.GetComponentByName("GABEL") as StiText).Text = (JAMF + HAZ - taf + MBAA).ToString("#,##0;#,##0-");
            (report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString("#,##0;#,##0-");
            (report.GetComponentByName("MAN") as StiText).Text = (JAMF + HAZ + MBAA - (NAGHD + VAR + HAV + JCHK + taf)).ToString("#,##0;#,##0-");


            var rst03 = dbms.DoGetDataSQL<double?>("SELECT  SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = " + HTAG /*TAG = 9 */ + ") AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").ToList();
            if (rst03.Count > 0)
            {
                if (!IsNull(rst03.FirstOrDefault()))
                {
                    var _VAZN_ = Math.Round((double)rst03.FirstOrDefault());
                    if (_VAZN_ > 0)
                    {
                        if (report.GetComponentByName("VAZN") is StiText vazn) vazn.Text = "وزن كل به كيلو : " + _VAZN_;
                    }
                    else
                    {
                        if (report.GetComponentByName("VAZN") is StiText vazn) vazn.Enabled = false;
                    }
                }
            }

            //(report.GetComponentByName("HR") as StiText).Text = $"{CL_HESABDARI.ALPHANUM(JAMF + HAZ - taf + MBAA)} ريال";
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(JAMF + HAZ - taf + MBAA));


            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked)
            {
                (report.GetComponentByName("FIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
                (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN2.IsChecked)
            {
                (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER;
                (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER;
            }
            if ((bool)SGN3.IsChecked)
            {
                (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER;
                (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER;
            }

            //(report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            //(report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            //(report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;



            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();

            #region SMS_SENDING

            if (Convert.ToBoolean(Baseknow.SMSACT))
            {
                var SMSAC = new CL_SMSAC();
                Msgwin msgwin = new Msgwin(true, "آیا پیامک هم ارسال شود؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    try
                    {
                        //var Moshtari = CUST_NO.SelectedItem?.GetType().GetProperty("NAME")?.GetValue(CUST_NO.SelectedItem);
                        var PayamText = CL_HESABDARI.CREATE_SMSKH(Convert.ToInt64(NUMBER.Text));

                        //ersal_sms(this.CUST_NO.SelectedValue, "فاكتور شماره :" + this.NUMBER1.Text + '\r' + "مورخ:" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + rst.FirstOrDefault().MEGHk + '\r' + "مانده حساب :" + CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString()) + '\r' + Baseknow.SMS_OWNER, this.NUMBER.Text, 2);
                        var RESULT0 = await SMSAC.ErselSmsAsync(CUST_NO.SelectedValue.ToString(), PayamText, Convert.ToInt64(NUMBER.Text), 12, false);
                        List<MSGMODEL.SmsResultRecord>? resultRecords = null;

                        if (RESULT0 != null)
                        {
                            resultRecords = SmsResultProcessor.ConvertToRecords(RESULT0);
                        }
                        if (RESULT0 != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                        {
                            new Msgwin(false, $"پيام {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ارسال شد....!").ShowDialog();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                            {
                                new Msgwin(false, $"{resultRecords?.FirstOrDefault()?.ErrorMessage} {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ").ShowDialog();
                            }
                            else
                            {
                                new Msgwin(false, $"پیام به خاطر خطا {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ارسال نشد!").ShowDialog();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, $"خطا در انجام عملیات ارسال پیام {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} , پیام ارسال نشد!").ShowDialog();
                    }
                }
            }
            //if ((bool)Baseknow.PRMFR)
            //{
            //}

            #endregion

        }
        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.HAVLAH_ENTER.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            (report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;


            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
        }
        private void Command108_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BTN_SAVE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.HAVLAH_ENTER_MABL.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            double JCHK = 0, JAMF = 0, HAZ = 0, NAGHD = 0, VAR = 0, HAV = 0, taf = 0, MBAA = 0;



            // Calculate JAMF
            var jst = dbms.DoGetDataSQL<double?>($@"
                                                    SELECT SUM(INVO_LST.MABL_K) AS SumOfMABL_K 
                                                    FROM INVO_LST 
                                                    WHERE INVO_LST.NUMBER = {NUMBER.Text} AND INVO_LST.TAG = {HTAG}").FirstOrDefault();

            JAMF = jst ?? 0;

            // Fetch HEAD_LST data
            var headLst = dbms.DoGetDataSQL<HeadLstData>($@"
                                                           SELECT NUMBER, TAG AS htag, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, 
                                                           M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, 
                                                           MOIN_KHF, ANBARF, FNUMCO, MBAA 
                                                           FROM HEAD_LST 
                                                           WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();

            if (headLst != null)
            {
                HAZ = headLst.MABL_HAZ;
                VAR = headLst.MABL_VAR;
                HAV = headLst.MABL_HAV;
                NAGHD = headLst.M_NAGHD;
                taf = headLst.TAKHFIF;
                MBAA = headLst.MBAA;
            }

            // Update report components
            //(report.GetComponentByName("JF") as StiText).Text = JAMF.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("MBAA") as StiText).Text = MBAA.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("TF") as StiText).Text = taf.ToString("#,##0;#,##0-");
            //(report.GetComponentByName("GABEL") as StiText).Text = (JAMF + HAZ - taf + MBAA).ToString("#,##0;#,##0-");
            //(report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString("#,##0;#,##0-");
            //(report.GetComponentByName("MAN") as StiText).Text = (JAMF + HAZ + MBAA - (NAGHD + VAR + HAV + JCHK + taf)).ToString("#,##0;#,##0-");


            //(report.GetComponentByName("HR") as StiText).Text = $"{CL_HESABDARI.ALPHANUM(JAMF + HAZ - taf + MBAA)} ريال";
            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(JAMF));


            //امضا ها
            //پیش فرض امضا ها مخفی است
            //if ((bool)SGN1.IsChecked)
            //{
            //    (report.GetComponentByName("FIMG") as StiImage).Enabled = true;

            //    (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER;
            //    (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER;
            //}
            //if ((bool)SGN2.IsChecked)
            //{
            //    (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

            //    (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER;
            //    (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER;
            //}
            //if ((bool)SGN3.IsChecked)
            //{
            //    (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

            //    (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER;
            //    (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER;
            //}

            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            //(report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            //(report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            //(report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            if (report.GetComponentByName("USERNAME") is StiText stiText) stiText.Text = Baseknow.UUSER;


            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
        }

        private void DoExportyPrices(bool IsSingleCurrentRow, INVO_LST_FACTOR22? TheRow, bool DoShoeMessages = true)
        {
            if (IsExporty)
            {
                if (IsSingleCurrentRow)
                {
                    if (TheRow.N_TAF == 0)
                        TheRow.TOTALARZ = 0;
                    else
                        TheRow.TOTALARZ = TheRow.N_TAF * TheRow.MEGHk;

                    if (TheRow.MEGHk == 0)
                        TheRow.TOTALARZ = 0;
                    else
                        TheRow.N_TAF = TheRow.TOTALARZ / TheRow.MEGHk;

                    TheRow.MABL = TheRow.N_TAF * Convert.ToDouble(ARZD.Text);
                    MABL_AfterUpdate(TheRow, IsSingleCurrentRow, DoShoeMessages);
                }
                else
                {
                    foreach (var Row in INVO_LST_FACTOR22_DATA)
                    {
                        if (Row.N_TAF == 0) //N_TAF_AfterUpdate
                        {
                            Row.TOTALARZ = 0; //TOTALARZ.TabStop = true;
                        }
                        else
                        {
                            Row.TOTALARZ = Row.N_TAF * Row.MEGHk; //TOTALARZ.TabStop = false;
                        }

                        if (Row.MEGHk == 0) //TOTALARZ_AfterUpdate
                        {
                            Row.TOTALARZ = 0;
                        }
                        else
                        {
                            Row.N_TAF = Row.TOTALARZ / Row.MEGHk; //TOTALARZ.TabStop = false;
                        }

                        Row.MABL = Row.N_TAF * Convert.ToDouble(ARZD.Text);

                        MABL_AfterUpdate(Row, IsSingleCurrentRow: false, false);
                    }
                }

            }
        }

        private void ARZD_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            DoExportyPrices(false, null);
        }

        private void OpenInterNationalInvoice()
        {
            if (!IsExporty)
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.InterOrderInvoice.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER.Text;
            ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;

            //توضیحات
            if (!string.IsNullOrEmpty(MOLAH.Text) && !string.IsNullOrWhiteSpace(MOLAH.Text))
            {
                (report.GetComponentByName("Text109") as StiText).Text = MOLAH.Text;
            }

            (report.GetComponentByName("CompanyName") as StiText).Text = Baseknow.NAME;
            (report.GetComponentByName("CompanyAddress") as StiText).Text = Baseknow.TFADDRESS;
            (report.GetComponentByName("CompanyZipCode") as StiText).Text = Baseknow.TFTEL;

            //report.Render();
            //report.Show();

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
        }
        private void BTN_NEW_FACTOR_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeIsHappend)
            {
                ClearFreshAll();
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "ذخیره را انجام نداده ای آیا از ادامه مطمئن هستید؟");
                if (msgwin.DialogResult != true)
                {
                    return;
                }
            }
        }
        private void ClearFreshAll(bool IsFromSelectionNumber = false)
        {
            //if (!IsFromSelectionNumber) //Not Selection
            //{
            //    NUMBER1.Text = "0"; //شماره فاکتور

            //    NUMBER.SelectedValue = null; //شماره حواله
            //    NUMBER.Text = "0"; //شماره حواله
            //}

            NUMBER1.Text = "0"; //شماره فاکتور

            NUMBER.SelectedValue = null; //شماره حواله
            NUMBER.Tag = null;
            NUMBER.Text = "0"; //شماره حواله


            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER; SHIFT.Items.Refresh(); //شیفت این کاربر
            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();
            MOLAH.Text = null;
            MAS.Text = "0"; //مدت

            if (IsExporty)
            {
                ARZD.Text = "0";
                ARZKIND2.SelectedValue = null; ARZKIND2.Items.Refresh();
            }

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh(); //واحد

            FNUMCO.Text = "0"; //شماره داخلی

            CUST_KIND.SelectedIndex = 0; CUST_KIND.Items.Refresh(); //نوع مشتری 

            OKF.IsChecked = false; //تایید فاکتور

            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOGU.Text = null; //موجودی

            Text117.Text = "0"; //جمع مقادیر
            JJKOL.Text = "0"; //جمع فاکتور

            MANDAH.Text = null;
            N_S.Text = "0"; //ثبت در سند
            MABNA.Text = "0"; //ثبت در سند

            //پشت فاکتور
            M_NAGHD.Text = "0"; //مبلغ نقد
            TAKHFIF.Text = "0"; //مبلغ تخفیف
            TAKHFIF_PERCENT.Text = "0"; //درصد تخفیف
            MABL_VAR.Text = "0"; //مبلغ واریزی
            MABL_HAV.Text = "0"; //مبلغ حواله
            MABL_HAZ.Text = "0"; //مبلغ هزینه
            MBAA.Text = "0"; //مبلغ مالیات و عوارض
            MOIN_VAR.Text = null; //معین واریزی
            CMB_MOIN_VAR.SelectedValue = null;

            MOIN_HAV.Text = null; //معین حواله
            CMB_MOIN_HAV.SelectedValue = null;

            MOIN_HAZ.Text = null; //هزینه
            CMB_MOIN_HAZ.SelectedValue = null;

            HMBAA.Text = null; //مالیات
            CMB_HMBAA.SelectedValue = null;

            JF.Text = "0"; //جمع کل فاکتور
            HKH.Text = "0"; //هزینه خدمات
            NTKHFIF.Text = "0"; //تخفیفات
            GHABEL.Text = "0"; //مبلغ قابل پرداخت
            NPAR.Text = "0"; //جمع مبالغ پرداختی
            MAN.Text = "0"; //مانده
            NCHK.Text = "0"; //جمع مبالغ چک

            INVO_LST_FACTOR22_DATA?.Clear(); //دیتاگرید فاکتور فروش
            PAY_GETP_SUB_DATA?.Clear(); //چک

            Form_Current();

            AllowEdits = true;

            GetResids();

            MakeDefaultFocuseReady();
        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_SUB.Items.Count > 0)
            {
                if (INVO_LST_SUB.SelectedItem is not null)
                {
                    var Row = INVO_LST_SUB.SelectedItem as INVO_LST_FACTOR22;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void INVO_LST_SUB_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
            }

            // Find the row under the mouse
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            DataGridRow row = dep as DataGridRow;
            if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
            {
                // Select the row under the mouse
                dataGrid.SelectedItem = row.Item;

                // Show the context menu
                dataGrid.ContextMenu.IsOpen = true;

                // Mark the event as handled to prevent the default context menu behavior
                e.Handled = true;
            }
            else
            {
                // No valid row, don't show context menu
                e.Handled = true;
            }
        }

        private void DEPATMAN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
