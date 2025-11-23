using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.CellGrid.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Wins.WinMenus.ANBAR.HEAD_LST_RASID_OTHER_WIN;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace Wins.WinMenus.ANBAR
{
    /// <summary>
    /// Interaction logic for HEAD_LST_HAV_OTHER_WIN.xaml
    /// </summary>
    public partial class HEAD_LST_HAV_OTHER_WIN : Window, ISearchableWindow
    {
        public HEAD_LST_HAV_OTHER_WIN(double? _NUMBER_ = null, bool _isAutomasion_ = false)
        {

            InitializeComponent();

            this.DataContext = this;

            if (_NUMBER_ is not null)
            {
                NUMBER.Text = _NUMBER_.ToStringNullSafe();
                OpenArgs = _NUMBER_.ToStringNullSafe();
                IsOpenedFromAutomation = _isAutomasion_;
            }
        }
        public bool IsOpenedFromAutomation { get; } = false;
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

        private NavigationManager<HEAD_LST> _navigationManager;
        public ObservableCollection<INVO_LST_FACTOR22> INVO_HAV_OTHER_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        TransactionManagement TM;

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
        private bool _ican;
        public bool AllowEdits
        {
            get { return _ican; }
            set
            {
                _ican = value;

                DEPATMAN.IsEnabled = _ican;

                if (_ican) // Is Enable and ReadOnly = False
                {
                    ALL_ITEMS_ENABLE();
                }
                else
                {
                    ALL_ITEMS_DISABLE();
                }
            }
        }

        public object ENTERED_VALUE_ROW { get; private set; }

        public int CURRENT_COLUMN_INDEX { get; private set; }

        public bool IsDataGridCellFocused { get; private set; }

        private int _DEFAULTCOL_index;
        public int DEFAULTCOL_INDEX_COL
        {
            get
            {
                if (INVO_LST_HAV_SUB_OTHER.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_HAV_SUB_OTHER.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NAME_CODE")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _DEFAULTCOL_index = 0;
                    }
                    else
                    {
                        _DEFAULTCOL_index = (int)defaultcolumnindex;
                    }
                }
                return _DEFAULTCOL_index;
            }
        }

        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        public INVO_LST_FACTOR22? CURRENT_ITMES_ROW { get; private set; }

        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; } = new INVO_LST_FACTOR22();

        public double min = 0;

        private const byte TAG = 26;

        public int? CUST_KIND { get; set; }
        public int? SHIFT { get; set; }

        public int CURRENT_ROW_INDEX { get; private set; }

        #region LOCALMODEL
        public class Custom_VAHEDK
        {
            public int? VAHED { get; set; }
            public string NAMES { get; set; }
            public string CODE { get; set; }
        }

        public class HLH
        {
            public string? hes { get; set; }
            public int? CUST_COD { get; set; }
        }

        public class HLH2
        {
            public bool? SGN0126 { get; set; }
            public bool? SGN0226 { get; set; }
            public bool? SGN0326 { get; set; }
            public int? USERCO { get; set; }
        }

        public class HLQ
        {
            public string? hes { get; set; }
            public string? nam { get; set; }
            public string? Expr1 { get; set; }
        }

        public class HLQ2
        {
            public string? nam { get; set; }
            public string? hes { get; set; }
        }

        public class HLQ3
        {
            public string? TAH { get; set; }
        }

        public class HLQ4
        {
            public string? MOLAH { get; set; }
        }

        private class TobItem
        {
            public int CODE { get; set; }
            public string NAMES { get; set; }
        }

        public class HLQ7
        {
            public double? MaxOfNUMBER { get; set; }
        }
        #endregion

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

        private bool _newrecord = false;
        public bool NewRecord
        {
            get
            {
                //if (string.IsNullOrEmpty(N_S.Text) || Convert.ToInt32(N_S.Text) == 0)
                //{
                //    _newrecord = true;
                //}
                //else
                //{
                //    _newrecord = false;
                //}
                return _newrecord;

            }
            set { _newrecord = value; }
        }

        InventoryManager IVM = new InventoryManager();

        List<COMBOPERSONEL> rst_personel = null;
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
        private bool PERSONEL_First_Open = true;
        private bool chek;



        public bool INVO_LST_HAV_SUB_OTHER_IsFocused { get; private set; }

        public int ANBARDefaultValue { get; set; }
        public nint WINDOW_ID { get; private set; }
        public Visual I_AM_INVO_RASID { get; set; }
        public string OpenArgs { get; }
        public bool AnyCrossAzadInvoiced { get; private set; }

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

        private void ReGetData()
        {
            INVO_HAV_OTHER_DATA?.Clear();
            if (NUMBER.Text is not null && NUMBER.Text != "")
            {
                var INVO_RASIDA_DATA_TEMP = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
																						 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
																					   	 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
																					 	 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
																	FROM            dbo.INVO_LST LEFT OUTER JOIN
																						 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
																						 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
																						 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE        (dbo.INVO_LST.TAG = 26) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                // INVO_RASIDA_DATA?.Clear();

                foreach (var item in INVO_RASIDA_DATA_TEMP)
                {
                    INVO_HAV_OTHER_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }

        private void ALL_ITEMS_ENABLE()
        {
            NUMBER.IsEnabled = true;
            if (Baseknow.UPDDATE ?? false)
            {
                this.DATE_N.IsEnabled = true;
            }
            else
            {
                this.DATE_N.IsEnabled = false;
            }
            if (Baseknow.UPDDATE ?? false)
            {
                this.DATE_N.IsEnabled = true;
            }
            else
            {
                this.DATE_N.IsEnabled = false;
            }
            FNUMCO.IsEnabled = true;
            TAH.IsEnabled = true;
            MOLAH.IsEnabled = true;
            CUST_NO.IsEnabled = true;
            CUST_NO2.IsEnabled = true;
            hTAG.IsEnabled = true;
            //PERSONEL.IsEnabled = true;
            SGN1.IsEnabled = true;
            SGN2.IsEnabled = true;
            sgn1usid.IsEnabled = true;
            sgn2usid.IsEnabled = true;
            MOGU.IsEnabled = true;
            Text59.IsEnabled = true;
            Command106.IsEnabled = true;
            SAVE_BTN.IsEnabled = true;
            DELETE_BTN.IsEnabled = true;

            if (!_navigationManager.IsNewRecord)
            {
                INVO_LST_HAV_SUB_OTHER.IsReadOnly = false; //Unlock
            }
        }

        private void ALL_ITEMS_DISABLE()
        {
            NUMBER.IsEnabled = false;
            DATE_N.IsEnabled = false;
            USER_NAME.IsEnabled = false;
            FNUMCO.IsEnabled = false;
            TAH.IsEnabled = false;
            MOLAH.IsEnabled = false;
            CUST_NO.IsEnabled = false;
            CUST_NO2.IsEnabled = false;
            hTAG.IsEnabled = false;
            //PERSONEL.IsEnabled = false;
            //SGN1.IsEnabled = false;
            //SGN2.IsEnabled = false;
            MOGU.IsEnabled = false;
            Text59.IsEnabled = false;
            SAVE_BTN.IsEnabled = false;
            DELETE_BTN.IsEnabled = false;
            Command106.IsEnabled = false;
            INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;
        }

        public bool DATE_IS_VALID()
        {
            bool Date_Is_Valid = true;

            var DATE = DATE_N.Text.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    DATE_N.Text = null;
                    DATE_N.Focus();
                    Date_Is_Valid = false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        DATE_N.Text = null;
                        DATE_N.Focus();
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                DATE_N.Focus();
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
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
                new SearchableProperty { DisplayName = "شماره", PropertyPath = "NUMBER1", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "شماره حواله سایر", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }

        #endregion

        public bool HeaderIsValid()
        {
            if (!DATE_IS_VALID())
            {
                return false;
            }

            if (IsNull(this.CUST_NO.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, " مشتري مشخص نشده است ....!");
                msgwin.ShowDialog();
                return false;
            }

            if (string.IsNullOrEmpty(DATE_N.Text.ToRawTarikh()))
            {
                Msgwin msgwin = new Msgwin(false, "تاریخ صحیح نمی باشد");
                msgwin.ShowDialog();
                return false;
            }

            if (string.IsNullOrEmpty(TAH.Text))
            {
                Msgwin msgwin = new Msgwin(false, "تحویل گیرنده نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return false;
            }

            if (string.IsNullOrEmpty(MOLAH.Text))
            {
                Msgwin msgwin = new Msgwin(false, "تحویل دهنده نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return false;
            }

            if (hTAG.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "نوع رسید نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return false;
            }

            if (!IsNull(this.CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.CUST_NO.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    return false;
                }
            }
            if (!IsNull(this.CUST_NO.SelectedValue))
            {
                if ((bool)Baseknow.SAGHF || (bool)Baseknow.SAGHF2)
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(this.CUST_NO.SelectedValue.ToString())) == false)
                    {
                        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                        msgwin.Show();
                        CUST_NO.SelectedValue = null;
                        return false;
                    }
                }
            }

            return true;
        }

        //NOT_USED
        private void Form_Current()
        {
            bool ghat;

            if (INVO_HAV_OTHER_DATA.Count > 0)
            {
                this.Command106.IsEnabled = true;
            }
            else
            {
                this.Command106.IsEnabled = false;
            }
            if ((bool)Baseknow.SIGN && false)
            {
                if ((bool)this.SGN2.IsChecked)
                {
                    this.Command106.IsEnabled = true;
                }
                else
                {
                    this.Command106.IsEnabled = false;
                }
            }
            if (this.NewRecord)
            {
                this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;
                //this.AllowAdditions = true;
                this.AllowDeletions = true;
                this.AllowEdits = true;
                //this["INVO_LST_HAV_SUB_OTHER"].Form.AllowAdditions = true;
                //this["INVO_LST_HAV_SUB_OTHER"].Form.Refresh();
            }
            else
            {
                var RST = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE TAG = 27 and NUMBER =  " + this.NUMBER.Text).ToList();
                if (RST.Count == 0)
                {
                    //this.AllowAdditions = true;
                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                    //this["INVO_LST_HAV_SUB_OTHER"].Form.AllowAdditions = true;
                    this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = false;
                    //this["INVO_LST_HAV_SUB_OTHER"].Form.Refresh();
                }
                else
                {
                    this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = false;
                    this.AllowDeletions = false;
                    //this["INVO_LST_HAV_SUB_OTHER"].Form.AllowAdditions = false;
                    //this["INVO_LST_HAV_SUB_OTHER"].Form.Refresh();
                }
                //RST.Close();
            }
            if ((bool)Baseknow.SIGN)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;

                //ERROR
                //var RST = dbms.DoGetDataSQL<HLH2>("SELECT SGN0126,SGN0226,SGN0326 FROM dbo.SIGN WHERE USERCO = " + Baseknow.USERCOD).ToList();
                //if (RST.Count > 0)
                //{
                //    if (RST.Fields("SGN0124"))
                //    {
                //        this.SGN1.Enabled = true;
                //    }
                //    else
                //    {
                //        this.SGN1.Enabled = false;
                //    }
                //    if (RST.Fields("SGN0224"))
                //    {
                //        this.SGN2.Enabled = true;
                //    }
                //    else
                //    {
                //        this.SGN2.Enabled = false;
                //    }
                //}
                this.NUMBER.IsEnabled = true;
                this.DATE_N.IsEnabled = true;
                this.FNUMCO.IsEnabled = true;
                this.MOLAH.IsEnabled = true;
                this.TAH.IsEnabled = true;
                if ((bool)SGN2.IsChecked)
                {
                    this.SGN1.IsEnabled = false;
                    this.AllowDeletions = false;
                    this.AllowEdits = false;
                    this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;

                }
            }
            Information.Err().Clear();

            if ((OKF.IsChecked ?? false))
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;
                this.ESLAH.IsEnabled = true;
            }
            //this.PERSONEL.Visible = true;
            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 26, Convert.ToInt32(Baseknow.USERCOD), WINDOW_ID);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
            }
        }
        //NOT_USED & NOT_NEEDED
        private void Form_BeforeUpdate()
        {
            if (IsNull(this.CUST_NO.SelectedValue) || Convert.ToInt32(this.CUST_NO.SelectedValue) == 0)
            {
                Msgwin msgwin = new Msgwin(false, " مشتري مشخص نشده است ....!");
                msgwin.ShowDialog();
                return;
            }
            if (Convert.ToInt32(this.NUMBER.Text) == 0)
            {
                var rst = dbms.DoGetDataSQL<HLQ7>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)=" + hTAG.SelectedValue + "))").ToList();
                if (rst.Count == 0 || IsNull(rst.FirstOrDefault().MaxOfNUMBER))
                {
                    this.NUMBER.Text = Baseknow.STFRB.ToString();
                }
                else
                {
                    this.NUMBER.Text = Convert.ToString(rst.FirstOrDefault().MaxOfNUMBER + 1);
                }
                //rst.Close();
            }
        }
        //NOT_USED & NOT_NEEDED
        private void Form_After_Update()
        {
            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 24, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
            }
        }

        private void Form_Load()
        {

            //if (IsNull(this.ServerFilter))
            //{
            //    DoCmd.GoToRecord(acDataForm, this.Name, acNewRec);
            //}

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            WINDOW_ID = new WindowInteropHelper(this).Handle;
            I_AM_INVO_RASID = CL_LMethods.GetTheWindow(WINDOW_ID);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "SAYERHAV", WINDOW_ID);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;

            Fill_ComboBoxes();

            USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();

            #region LOADING...
            string WhereCondition = TAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {TAG}) " : "  ";
            _restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(TAG, WhereCondition);
            WhereCondition = _restrictionInfo.WhereClause;

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG} ";
            }

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
                x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {TAG}", //On Change for One Record
                Convert.ToDouble(NUMBER.Text)
                );

            if (!IsOpenedFromAutomation && !string.IsNullOrEmpty(OpenArgs) && _navigationManager.NUMBER_TO_OPEN != null) //Had a paramter passed
            {
                //یعنی این شماره رو پیدا نکرده که اون رو ریست کنه
                new Msgwin(false, $"شما به این شماره حواله سایر دسترسی ندارید{_navigationManager.NUMBER_TO_OPEN} دسترسی ندارید ").Show();
                try { this?.Close(); } catch { }
                return;
            }

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();
            #endregion

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
            bool recordExists = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 NUMBER FROM HEAD_LST WHERE NUMBER = {requestedNumber} AND TAG = {TAG}").FirstOrDefault() != null;
            string message = recordExists ? GetAccessDeniedMessage() : "چنین شماره ای وجود ندارد";
            new Msgwin(false, message).ShowDialog();
            _navigationManager.ClearNumberToOpen();
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
                if (HEADER_FAC is null)
                {
                    new Msgwin(false, "این سایر حواله خالی است").Show();
                    return;
                }
                AnyCrossAzadInvoiced = false;
                NUMBER.Text = HEADER_FAC?.NUMBER.ToString();

                Form_Current();

                if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
                {
                    DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe();
                    FNUMCO.Text = HEADER_FAC.FNUMCO.ToStringNullSafe();
                    hTAG.SelectedValue = HEADER_FAC.TAG.ToStringNullSafe();

                    TAH.SelectedValue = HEADER_FAC.TAH.ToStringNullSafe();
                    MOLAH.SelectedValue = HEADER_FAC.MOLAH.ToStringNullSafe();


                    var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + HEADER_FAC.CUST_NO + "'").FirstOrDefault();
                    if (data is not null && !string.IsNullOrEmpty(data.hes))
                    {
                        string thevalue = data.hes;

                        if (CUST_NO.ItemsSource == null)
                        {
                            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                        }

                        if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
                        {
                            ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                        }
                        CUST_NO.SelectedValue = null;
                        CUST_NO.SelectedValue = thevalue;
                        CUST_NO.Items.Refresh();
                    }


                    PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                    PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
                    PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                    DEPATMAN.SelectedValue = HEADER_FAC?.DEPATMAN; DEPATMAN.Items.Refresh();
                    USER_NAME.Text = HEADER_FAC?.USER_NAME;
                    SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1);
                    SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2);

                    SGN1.Tag = Convert.ToInt32(HEADER_FAC.sgn1usid);
                    SGN2.Tag = Convert.ToInt32(HEADER_FAC.sgn2usid);

                    if (HEADER_FAC?.sgn1usid is not null)
                    {
                        sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn1usid)?.SAL_NAME;
                    }
                    else
                    {
                        sgn1usid.Text = null;
                    }

                    if (HEADER_FAC?.sgn2usid is not null)
                    {
                        sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.SAL_NAME;
                    }
                    else
                    {
                        sgn2usid.Text = null;
                    }

                    OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور
                    ReGetData();

                    AllowEdits = false;
                    Command106.IsEnabled = true;
                }

            }
        }
        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG}").FirstOrDefault();
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
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        public void ANBAR_LOADITEM()
        {
            ////ALL ANBARS:
            //var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR ORDER BY TCOD_ANBAR.CODE").ToList();
            string RowSource_ANBAR = "SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE";
            if (Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 9, 1) == "5")
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT ANBCO FROM dbo.OPANBACCESS WHERE (USERCO = " + Baseknow.USERCOD + " ) ORDER BY dbo.OPANBACCESS.RDF").ToList();
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
            //ANBAR_COL.EditingElementStyle.
        }

        private void Fill_ComboBoxes()
        {
            //انبار کالا
            ANBAR_LOADITEM();
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

            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";
            CUST_NO.SelectedItem = null;
            //Codes Of Customers
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            TAH.ItemsSource = dbms.DoGetDataSQL<HLQ3>("SELECT TAH FROM HEAD_LST GROUP BY TAH ORDER BY TAH").ToList();
            TAH.DisplayMemberPath = "TAH";
            TAH.SelectedValuePath = "TAH";

            MOLAH.ItemsSource = dbms.DoGetDataSQL<HLQ4>("SELECT MOLAH FROM HEAD_LST GROUP BY MOLAH ORDER BY MOLAH").ToList();
            MOLAH.DisplayMemberPath = "MOLAH";
            MOLAH.SelectedValuePath = "MOLAH";

            //شخصیت
            hTAG.ItemsSource = new List<TobItem>
            {
                new TobItem { CODE = 26, NAMES = "برگشت خريد" },
            };
            hTAG.DisplayMemberPath = "NAMES";
            hTAG.SelectedValuePath = "CODE";
            hTAG.SelectedIndex = 0; hTAG.Items.Refresh();

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
            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
        }

        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            //if (!DATE_IS_VALID())
            //{
            //    e.Handled = true; //Cancel Leaving Focus
            //}
        }

        private void CUST_NO_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            //{
            //    ComboSearch CMBSearch = new ComboSearch("HEAD_LST_HAV_OTHER_WIN", I_AM_INVO_RASID);//Search Plusy Form Specialy for Customers
            //    CMBSearch.ShowDialog();
            //    if (CUST_NO.SelectedValue is null)
            //    {
            //        return;
            //    }
            //}

            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر
            TextBox CUTSNO_TEX = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
            if (CUTSNO_TEX is null)
            {
                return;
            }
            if (CUST_NO.SelectedValue is not null)
            {
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"مشتری نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                e.Handled = true;
            }

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
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }

        }

        private void CUST_NO2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (!IsNull(this.NUMBER.Text))
            {
                dt = DateTime.Now;
                if (Convert.ToBoolean(Baseknow.TRANSF))
                {
                    dbms.DoExecuteSQL($"INSERT INTO dbo.TR_HEAD_LST   (NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, UP_TIME,UP_DATE,OKF,UP_USER_NAME,PC_NAME,IPADD) SELECT     NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, {Tarikh.GET_OADATE_DAO()} AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2,OKF,'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.CurrentMachineName() + "' , '" + CL_HESABDARI.GETIPADD() + "'   FROM dbo.HEAD_LST WHERE (NUMBER = " + this.NUMBER.Text + " ) And (TAG = 26)");
                    dbms.DoExecuteSQL($"INSERT INTO dbo.TR_INVO_LST   (UP_TIME, UP_DATE, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA) SELECT {Tarikh.GET_OADATE_DAO()} AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K , FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA FROM dbo.INVO_LST WHERE (NUMBER = " + this.NUMBER.Text + ") And (TAG = 26)");
                    //dbms.DoExecuteSQL($"INSERT INTO dbo.TR_HEAD_LST (NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, UP_TIME,UP_DATE,OKF,UP_USER_NAME,PC_NAME,IPADD) SELECT NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, {Tarikh.GET_OADATE_DAO()} AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2,OKF,'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.CurrentMachineName() + "' , '" + CL_HESABDARI.GETIPADD() + "'   FROM dbo.HEAD_LST WHERE NUMBER = " + this.NUMBER.Text + "  And TAG = 24");
                    //dbms.DoExecuteSQL($"INSERT INTO dbo.TR_INVO_LST (UP_TIME, UP_DATE, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA) SELECT {Tarikh.GET_OADATE_DAO()}AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K , FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA FROM dbo.INVO_LST WHERE (NUMBER = " + this.NUMBER.Text + ") And (TAG = 24)");
                }
                this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked)
                {
                    //this.AllowEdits = false;
                    Command106.IsEnabled = true;
                    PERSONEL.IsEnabled = true;
                    Msgwin msgwin = new Msgwin(false, " اول امضاء را بردارید ...");
                    msgwin.ShowDialog();
                    SGN1.IsEnabled = true;
                    SGN2.IsEnabled = true;

                    return;
                }
                else
                {
                    //فاکتور برگشت خرید آزاد از سایر حواله انبار
                    AnyCrossAzadInvoiced = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 NUMBER FROM dbo.HEAD_LST WHERE TAG = 27 AND NUMBER = {NUMBER.Text} ").Any();
                    if (AnyCrossAzadInvoiced)
                    {
                        universControl.PopNotifyShowUp("این برگه دارای مرجوعی (برگشت خرید آزاد) است و نمیتوان سطر های آنرا تغییر داد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                        this.AllowEdits = true;
                        INVO_LST_HAV_SUB_OTHER.IsReadOnly = true;
                    }
                    else
                    {
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        ALL_ITEMS_ENABLE();
                        this.INVO_LST_HAV_SUB_OTHER.IsReadOnly = false;
                    }
                }
            }
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "RBRFR", WINDOW_ID);
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 26);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("INSERT INTO EVENTS(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'ساير رسيد انبار شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26,GETDATE()," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 26);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26 )");
            }
            //this.PERSONEL.Visible = true;
            var Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            sgn1usid.Tag = Baseknow.USERCOD;
            sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled)
                {
                    ALL_ITEMS_DISABLE();

                    this.Command106.IsEnabled = true;
                    PERSONEL.IsEnabled = true;
                }
            }
            else
            {
                //ALL_ITEMS_ENABLE();

                this.Command106.IsEnabled = false;
            }

            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 26");

        }

        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 26);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'ساير رسيد انبار شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26,GETDATE()," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 26);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",26," + this.NUMBER.Text + ",26 )");
            }
            //this.PERSONEL.Visible = true;
            var Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;
            sgn2usid.Tag = Baseknow.USERCOD;
            sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled)
                {
                    ALL_ITEMS_DISABLE();

                    this.Command106.IsEnabled = true;
                    PERSONEL.IsEnabled = true;
                }
            }
            else
            {
                //ALL_ITEMS_ENABLE();

                this.Command106.IsEnabled = false;
            }

            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 26");

        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            if (NUMBER.Text is null || NUMBER.Text == "0" || NUMBER.Text == "" || DATE_N.Text.ToRawTarikh() is null || DATE_N.Text.ToRawTarikh() == "" || PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow("شماره سند و تاریخ نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            CL_HESABDARI.PERSONELUpdate(26, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), "'ساير حواله انبار شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'");

            universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void DELETE_BTN_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_BTN.Visibility == Visibility.Visible;
            if (!DELETE_BTN.IsEnabled || INVO_LST_HAV_SUB_OTHER.IsReadOnly || !IsVisible) { return; }

            if (DELETE_BTN.IsEnabled)
            {
                if (INVO_HAV_OTHER_DATA.Count > 0)
                {
                    if (!(INVO_LST_HAV_SUB_OTHER.SelectedItems is null))
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            #region SABEGHEH
                            var dt = DateTime.Now;
                            dbms.DoExecuteSQL($"INSERT INTO dbo.TR_HEAD_LST   (NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, UP_TIME,UP_DATE,OKF,UP_USER_NAME,PC_NAME,IPADD) SELECT     NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, {Tarikh.GET_OADATE_DAO()} AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2,OKF,'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.CurrentMachineName() + "' , '" + CL_HESABDARI.GETIPADD() + "'   FROM dbo.HEAD_LST WHERE (NUMBER = " + this.NUMBER.Text + " ) And (TAG = 26)");
                            dbms.DoExecuteSQL($"INSERT INTO dbo.TR_INVO_LST   (UP_TIME, UP_DATE, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA) SELECT {Tarikh.GET_OADATE_DAO()} AS Expr1," + CL_HESABDARI.FARSIDATE() + " AS Expr2, NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K , FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA FROM dbo.INVO_LST WHERE (NUMBER = " + this.NUMBER.Text + ") And (TAG = 26)");
                            #endregion

                            _ = AuditLogger.LogActionAsync(
                                    actionType: "DELETE",
                                    tableName: "سایر حواله انبار ها",
                                    recordId: NUMBER.Text,
                                    oldValue: "TAG = 26",
                                    newValue: null,
                                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            List<MsgModel> ErrosMessages = new List<MsgModel>();
                            for (int i = 0; i < INVO_LST_HAV_SUB_OTHER.SelectedItems.Count; i++)
                            {
                                var item = INVO_LST_HAV_SUB_OTHER.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(INVO_LST_HAV_SUB_OTHER, item)) { continue; }

                                var _id_ = item.GetType().GetProperty("id").GetValue(item);

                                if (_id_ != null)
                                {
                                    try
                                    {
                                        var items = new List<object> { item };
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

                            ReGetData();
                        }
                        else
                        {
                            e.Handled = true; //اجازه نده از دیتاگرید چیزی حذف بشه
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER = {NUMBER.Text} AND TAG = {TAG}");

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
                                new Msgwin(false, "این برگه دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
                    }
                }
            }
        }

        private void DATA_GRID_On_Delete(INVO_LST_FACTOR22 item)
        {
            if (item is not null)
            {
                if (item.id is null)
                {
                    INVO_HAV_OTHER_DATA.Remove(item as INVO_LST_FACTOR22);
                }
                else
                {
                    //شروع اتصال
                    TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR); //Start Transaction 
                    bool IsMogudiOk = true;

                    var _id = item.id;
                    //حذف سطر دیتا گرید از دیتابیس
                    TM.ExecuteSqlCommandCtc($"DELETE FROM INVO_LST WHERE id = {_id} AND TAG = 26");

                    //بررسی وجود کالا در جدول موجودی موقت - جهت احتیاط
                    var RSTCO1 = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + item.CODE + "' AND ANBAR = " + item.ANBAR).ToList();
                    if (RSTCO1.Count == 0)
                    {
                    }
                    else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG)) //آیا موجودی چک شود ؟
                    {
                        // دریافت حداقل موجودی کالا
                        var min = CL_HESABDARI.Getmin(Convert.ToInt32(item.ANBAR), item.CODE);

                        //گرفتن موجودی در حال حاضر کالا در از انبار
                        var RSTCO2 = TM.SqlQueryCtc<double?>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + item.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + item.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + item.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + item.ANBAR + ")").ToList();
                        if (RSTCO2.Count > 0)
                        {
                            var MAND = (double)RSTCO2.FirstOrDefault();

                            //تفریق موجودی حال حاضر از مقدار در حال حاضر کالا ، مقایسه حداقل موجودی
                            if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - item.MEGHk), (int)Baseknow.DIG) < min && Baseknow.MOJU && Convert.ToInt32(item.ANBAR) > 0)
                            {
                                IsMogudiOk = false;
                            }
                            else
                            {
                                var RSTCO3 = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + item.CODE + "' AND ANBAR = " + item.ANBAR).ToList();
                                var _WHERE = " WHERE CODE = '" + item.CODE + "' AND ANBAR = " + item.ANBAR;
                                if (RSTCO3.Count > 0)
                                {
                                    RSTCO3.FirstOrDefault().MOGODI = MAND - item.MEGHk;
                                    RSTCO3.FirstOrDefault().MOGODI_A = 0;
                                    TM.ExecuteSqlCommandCtc($"UPDATE dbo.STUF_STK SET MOGODI = {RSTCO3.FirstOrDefault().MOGODI},MOGODI_A = 0 {_WHERE}");
                                    //RSTCO3.update();
                                }
                            }
                        }
                    }
                    if (IsMogudiOk)
                    {
                        TM.DoCommit(); //Approved
                    }
                    else
                    {
                        TM.DoRollback();
                        new Msgwin(false, $"خروج كالا {item.CODE} از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                    }
                }
            }
            else
            {
                Msgwin msgwin1 = new Msgwin(false, "چیزی برای حذف وجود ندارند");
                msgwin1.ShowDialog();
            }
        }

        private void DATA_GRID_On_Current()
        {
            //this.CODE.RowSource = "SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF ORDER BY STUF_DEF.NAME";
            //this.VAHED_K.RowSource = "SELECT VAHEDS.VAHED, TCOD_VAHEDS.NAMES FROM TCOD_VAHEDS INNER JOIN VAHEDS ON TCOD_VAHEDS.CODE = VAHEDS.VAHED ORDER BY TCOD_VAHEDS.NAMES";

            chek = false;
            if (this.NewRecord)
            {
                //[INVO_LST_HAV_SUB_OTHER][MOGU]
                //    MOGU.Text = null;
                //    CODE.TAG = "";
                //    VAHED_K.TAG = 0;
                //    MEGH.TAG = 0;
                //    MEGHk.TAG = 0;
                //    mabl.TAG = 0;
                //    MABL_K.TAG = 0;
                //    ANBAR.TAG = 0;
                //    MEGH_R.TAG = 0;
            }
            else
            {
                //    this.CODE.TAG = this.CODE;
                //    this.VAHED_K.TAG = this.VAHED_K;
                //    this.MEGH.TAG = this.MEGH;
                //    this.MEGHk.TAG = this.MEGHk;
                //    this.MEGH_R.TAG = this.MEGH_R;
                //    this.mabl.TAG = this.mabl;
                //    this.MABL_K.TAG = this.MABL_K;
                //    this.ANBAR.TAG = this.ANBAR;
                var rst = dbms.DoGetDataSQL<STUF_STK>($@"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {CURRENT_ITMES_ROW.ANBAR}").ToList();
                //rst.Filter = "CODE = '" + this.CODE + "' AND ANBAR = " + this.ANBAR;

                if (rst.Count == 0)
                {
                    MOGU.Text = null;
                }
                else
                {
                    MOGU.Text = Convert.ToString(rst.FirstOrDefault().MOGODI + rst.FirstOrDefault().MOGODI_A);
                }
            }
            if (CURRENT_ITMES_ROW.MABL > 0)
            {
                ANBAR_COLUMN.IsReadOnly = true;
                //CODE.Locked = true;
                CODEh_COLUMN.IsReadOnly = true;
                //VAHED_K.Locked = true;
                VAHED_K_COLUMN.IsReadOnly = true;
                //MEGH_R.Locked = true;
                MEGH_R_COLUMN.IsReadOnly = true;
                //MEGH.Locked = true;
                MABL_COLUMN.IsReadOnly = true;

                AllowDeletions = false;
            }
            else
            {
                //this.ANBAR.Locked = false;
                ANBAR_COLUMN.IsReadOnly = false;
                //this.CODE.Locked = false;
                CODEh_COLUMN.IsReadOnly = false;
                //this.VAHED_K.Locked = false;
                VAHED_K_COLUMN.IsReadOnly = false;
                //this.MEGH_R.Locked = false;
                MEGH_R_COLUMN.IsReadOnly = false;
                //this.MEGH.Locked = false;
                MABL_COLUMN.IsReadOnly = false;

                this.AllowDeletions = true;
            }
        }
        //MUST_USE
        private void DATA_GRID_Before_Update()
        {
            if (!this.NewRecord && Baseknow.WAR == 1)
            {

            }
            else if (IsNull(CURRENT_ITMES_ROW.ANBAR))
            {
                Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد.");
                msgwin.ShowDialog();

            }
            else if (IsNull(CURRENT_ITMES_ROW.CODE))
            {
            }
            else
            {
                var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                }
            }
        }

        private void DATA_GRID_After_Update()
        {
            if (USER_NAME.Text != CL_HESABDARI.UCurrentUser().ToString())
            {
                USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();
            }
            if (INVO_HAV_OTHER_DATA.Count > 0)
            {
                Command106.IsEnabled = true;
            }
            else
            {
                Command106.IsEnabled = false;
            }
            //REFRESHMOG(Me.CODE, Me.ANBAR, "head_lst_hav_other")
        }

        private void INVO_LST_HAV_SUB_OTHER_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            #region NEED
            ComboBox Comboval = null;
            TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!ReferenceEquals(Comboval, null))
                ENTERED_VALUE_ROW = Comboval.SelectedValue;
            else
                ENTERED_VALUE_ROW = TexboVal.Text.Trim();

            CURRENT_ITMES_ROW = e.Row.Item as INVO_LST_FACTOR22;
            #endregion

            #region ANBAR_After_Update
            if (e.Column.SortMemberPath == "ANBAR")
            {
                if (!(IsNull(CURRENT_ITMES_ROW.CODE) || CURRENT_ITMES_ROW.CODE == ""))
                {
                    //ERROR
                    //MEGH_AfterUpdate();
                    if (chek)
                    {
                        //this.Undo();
                    }
                }
            }
            #endregion

            #region CODE_Not_In_List
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim() ||
                    (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe())))
                {
                    #region CODE_NotInList
                    //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                    if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                    {
                        CURRENT_ITMES_ROW.MEGH = 0;
                        CURRENT_ITMES_ROW.MEGHk = 0;
                        CURRENT_ITMES_ROW.MABL_K = 0;
                        SERCHK sERCHK = new SERCHK(I_AM_INVO_RASID, CURRENT_ITMES_ROW.ANBAR.ToString());
                        sERCHK.ShowDialog();

                        if (FROM_SAERCH_KAL.CODE is null)
                        {
                            INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();
                            return;
                        }
                        else
                        {
                            CURRENT_ITMES_ROW.CODE = FROM_SAERCH_KAL.CODE;
                            CURRENT_ITMES_ROW.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;

                            CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                            //Cleaning
                            FROM_SAERCH_KAL.CODE = null;
                            FROM_SAERCH_KAL.NAME_CODE = null;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //Cleaning
                            CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K

                            return;
                        }

                        if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //اگر عدد وارد کرده برم سرغ کد کالا
                            var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ITMES_ROW.ANBAR})").FirstOrDefault();
                            if (!ReferenceEquals(FoundKala, null))
                            {
                                CURRENT_ITMES_ROW.CODE = FoundKala.CODE;
                                CURRENT_ITMES_ROW.NAME_CODE = FoundKala.NAME;

                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                            }
                            else
                            {
                                var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND dbo.STUF_FSK.ANBAR = {CURRENT_ITMES_ROW.ANBAR}").ToList();
                                if (rstfani.Count > 0)
                                {
                                    CURRENT_ITMES_ROW.CODE = rstfani.FirstOrDefault().CODE;
                                    CURRENT_ITMES_ROW.NAME_CODE = rstfani.FirstOrDefault().NAME;

                                    CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                                }
                                else
                                {
                                    new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                                    CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                                    CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                    CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K

                                    INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();

                                    return;
                                }
                            }
                        }
                        else
                        {
                            CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.ANBAR.ToString(), I_AM_INVO_RASID);
                            if (FROM_SAERCH_KAL.CODE is null)
                            {

                                INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();

                                CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                                CURRENT_ITMES_ROW.VAHED_K = null; //Reset VAHED_K

                                return;
                            }
                            else
                            {
                                CURRENT_ITMES_ROW.CODE = FROM_SAERCH_KAL.CODE;
                                CURRENT_ITMES_ROW.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;

                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                                //Cleaning
                                FROM_SAERCH_KAL.CODE = null;
                                FROM_SAERCH_KAL.NAME_CODE = null;
                            }
                        }
                    }
                    if (Strings.Len(ENTERED_VALUE_ROW.ToString()) >= 9)
                    {
                        var RSTCC3 = dbms.DoGetDataSQL<_NFANI_>("SELECT N_FANI,CODE FROM STUF_DEF WHERE N_FANI = '" + ENTERED_VALUE_ROW.ToString() + "'").ToList();
                        if (RSTCC3.Count > 0)
                        {
                            CURRENT_ITMES_ROW.CODE = RSTCC3.FirstOrDefault().CODE;
                            if (CURRENT_ITMES_ROW.MEGH == 0)
                            {
                                CURRENT_ITMES_ROW.MEGH = 1;
                                CURRENT_ITMES_ROW.MEGHk = 1;
                            }
                        }
                        if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
                        {
                            if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                            {
                                var RSTCC4 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ITMES_ROW.CODE + "')").ToList();
                                if (RSTCC4.Count > 0)
                                {
                                    CURRENT_ITMES_ROW.SANAD_NO = RSTCC4.FirstOrDefault().MAX_M;
                                }
                            }
                            else if (CURRENT_ITMES_ROW.SANAD_NO == 0 || IsNull(CURRENT_ITMES_ROW.SANAD_NO))
                            {
                                var RSTCC5 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ITMES_ROW.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                if (RSTCC5.Count > 0)
                                {
                                    CURRENT_ITMES_ROW.SANAD_NO = RSTCC5.FirstOrDefault();
                                }
                            }
                        }
                        string CC = "";
                        if (Strings.Mid(Baseknow.OPTIONSS, 34, 1) == "5")
                        {
                            switch (Strings.Mid(Baseknow.OPTIONSS, 35, 2) ?? "")
                            {
                                case "03":
                                    {
                                        CC = "";
                                        CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ITMES_ROW.CODE, 18, 6)));
                                        CURRENT_ITMES_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITMES_ROW.CODE, 4, 3) + "." + Strings.Mid(CURRENT_ITMES_ROW.CODE, 7, 3));
                                        CURRENT_ITMES_ROW.MABL = Convert.ToDouble(Strings.Mid(CURRENT_ITMES_ROW.CODE, 10, 8));
                                        CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH;
                                        CURRENT_ITMES_ROW.MABL_K = Math.Round((double)(CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk));
                                        CURRENT_ITMES_ROW.CODE = CC;
                                        break;
                                    }

                                default:
                                    {
                                        CC = "";
                                        CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ITMES_ROW.CODE, 3, 5)));
                                        if (Convert.ToDouble(Strings.Left(CURRENT_ITMES_ROW.CODE, 2)) == Convert.ToDouble("27"))
                                        {
                                            CURRENT_ITMES_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITMES_ROW.CODE, 8, 2) + "." + Strings.Mid(CURRENT_ITMES_ROW.CODE, 10, 3));
                                            CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH;
                                        }
                                        else
                                        {
                                            CURRENT_ITMES_ROW.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ITMES_ROW.CODE, 8, 5));
                                            CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH;
                                        }
                                        CURRENT_ITMES_ROW.CODE = CC;
                                        break;
                                    }
                            }

                        }
                    }
                    var RST00 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                    if (RST00.Count == 0)
                    {
                        MOGU.Text = null;
                    }
                    else
                    {
                        MOGU.Text = ((double)RST00.FirstOrDefault().MOGODI + RST00.FirstOrDefault().MOGODI_A).ToString();
                    }
                    //var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "'").ToList();
                    //if (RST.Count == 0)
                    //{
                    //}
                    //else
                    //{
                    //    CURRENT_ITMES_ROW.VAHED_K = RST.FirstOrDefault().VAHED;
                    //}
                    if (CURRENT_ITMES_ROW.ANBAR is not null)
                    {
                        if (CURRENT_ITMES_ROW.id > 0)
                        {
                            var RSTCO1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                            if (RSTCO1.Count == 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                msgwin.ShowDialog();
                            }
                            else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                            {
                                var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                                if (RSTCO2.Count > 0)
                                {
                                    var MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                                    if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ITMES_ROW.MEGHk), 2) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();

                                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;

                                    }
                                    else
                                    {
                                        var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                        var _WHERE = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                                        if (RSTCO3.Count > 0)
                                        {
                                            RSTCO3.FirstOrDefault().MOGODI = MAND - CURRENT_ITMES_ROW.MEGHk;
                                            RSTCO3.FirstOrDefault().MOGODI_A = 0;
                                        }
                                    }
                                }
                            }
                            else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                            {
                                if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITMES_ROW.MEGH_MAR)) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                    msgwin.ShowDialog();
                                    CURRENT_ITMES_ROW = WAS_ROW_ITEM;

                                }
                            }
                            else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                msgwin.ShowDialog();
                                CURRENT_ITMES_ROW = WAS_ROW_ITEM;

                            }
                        }
                    }

                    #endregion

                }
            }
            #endregion

            #region CODE_After_Update
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (CURRENT_ITMES_ROW?.ANBAR == null || CURRENT_ITMES_ROW?.CODE == null)
                {
                    return;
                }

                var min = default(double);
                double MAND;
                var RST0 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                if (RST0.Count == 0)
                {
                    MOGU.Text = null;
                }
                else
                {
                    MOGU.Text = Convert.ToString(RST0.FirstOrDefault().MOGODI + RST0.FirstOrDefault().MOGODI_A);
                }

                var RST1 = dbms.DoGetDataSQL<STUF_DEF>("SELECT VAHED , MIN_M from STUF_DEF WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "'").ToList();
                if (RST1.Count > 0)
                {
                    //CURRENT_ITMES_ROW.VAHED_K = RST1.FirstOrDefault().VAHED;
                    min = CL_HESABDARI.Getmin(Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR), CURRENT_ITMES_ROW.CODE);
                }

                if (!this.NewRecord)
                {
                    var RST2 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                    if (RST2.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                    {
                        var RST3 = dbms.DoGetDataSQL<double>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                        if (RST3.Count > 0)
                        {
                            MAND = RST3.FirstOrDefault(0);
                            if (Math.Round(Convert.ToDouble(RST3.FirstOrDefault(0) - CURRENT_ITMES_ROW.MEGHk), Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, Convert.ToInt32(Baseknow.DIG)) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                msgwin.ShowDialog();
                                CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                chek = true;
                                var RST4 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                if (RST4.Count > 0)
                                {
                                    RST4.FirstOrDefault().MOGODI = MAND;
                                    RST4.FirstOrDefault().MOGODI_A = 0;

                                    dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST4.FirstOrDefault().MOGODI} , MOGODI_A = {RST4.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);
                                }
                            }
                            else
                            {
                                var RST = dbms.DoGetDataSQL<double>("SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                                if (RST.Count > 0)
                                {
                                    MAND = RST.FirstOrDefault(0);

                                    var RST5 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                                    if (RST5.Count > 0)
                                    {
                                        RST5.FirstOrDefault().MOGODI = Convert.ToDouble(MAND + CURRENT_ITMES_ROW.MEGHk);
                                        RST5.FirstOrDefault().MOGODI_A = 0;

                                        dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST5.FirstOrDefault().MOGODI} , MOGODI_A = {RST5.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR);

                                    }
                                }
                            }
                        }
                    }
                    else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                    {
                        if (RST2.FirstOrDefault().MOGODI + RST2.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Convert.ToDouble(WAS_ROW_ITEM.MEGHk) - CURRENT_ITMES_ROW.MEGH_MAR)) < min && Baseknow.MOJU && CURRENT_ITMES_ROW.ANBAR > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                            msgwin.ShowDialog();
                            CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                            chek = true;
                        }
                    }
                    else if (RST2.FirstOrDefault().MOGODI + RST2.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && CURRENT_ITMES_ROW.ANBAR > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();
                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                        chek = true;
                    }
                }
            }
            #endregion

            #region MEGH_After_Update
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (CURRENT_ITMES_ROW?.ANBAR == null || CURRENT_ITMES_ROW?.CODE == null || CURRENT_ITMES_ROW?.MEGH == null)
                {
                    return;
                }

                double min;
                long Temp;
                double MAND;

                min = CL_HESABDARI.Getmin(Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR), CURRENT_ITMES_ROW.CODE);
                if (CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.VAHED_K is null || CURRENT_ITMES_ROW.ANBAR is null)
                {
                    return;
                }
                var RST = dbms.DoGetDataSQL<HLQ11>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                    return;
                }
                else if (CURRENT_ITMES_ROW.MABL == 0)
                {
                    CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * RST.FirstOrDefault().NESBAT;
                    CURRENT_ITMES_ROW.MEGH_R = CURRENT_ITMES_ROW.MEGHk;
                }
                var RST2 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                if (RST2.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                    return;
                }
                //else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                //{
                //    var RST3 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                //    if (RST3.Count > 0)
                //    {
                //        MAND = Convert.ToDouble(RST3.FirstOrDefault());
                //        if (Math.Round((double)(RST3.FirstOrDefault() - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR), Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ITMES_ROW.ANBAR != 0 && Baseknow.MOJU)
                //        {
                //            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                //            msgwin.ShowDialog();

                //            CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                //            CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                //            CURRENT_ITMES_ROW.MABL_K = WAS_ROW_ITEM.MABL_K;
                //            CURRENT_ITMES_ROW.MABL = WAS_ROW_ITEM.MABL;
                //            chek = true;

                //            var RST4 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                //            if (RST4.Count > 0)
                //            {
                //                RST4.FirstOrDefault().MOGODI = MAND;
                //                RST4.FirstOrDefault().MOGODI_A = 0;
                //                dbms.DoExecuteSQL($@"UPDATE STUF_STK SET MOGODI = {RST4.FirstOrDefault().MOGODI},MOGODI_A = {RST4.FirstOrDefault().MOGODI_A} WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {CURRENT_ITMES_ROW.ANBAR}");
                //            }
                //        }
                //        else
                //        {
                //            var RST5 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                //            if (RST5.Count > 0)
                //            {
                //                RST5.FirstOrDefault().MOGODI = MAND - (double)WAS_ROW_ITEM.MEGHk - (double)CURRENT_ITMES_ROW.MEGHk - (double)CURRENT_ITMES_ROW.MEGH_MAR;
                //                RST5.FirstOrDefault().MOGODI_A = 0;
                //                dbms.DoExecuteSQL($@"UPDATE STUF_STK SET MOGODI = {RST5.FirstOrDefault().MOGODI},MOGODI_A = {RST5.FirstOrDefault().MOGODI_A} WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {CURRENT_ITMES_ROW.ANBAR}");
                //            }
                //        }
                //    }
                //}
                //else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                //{
                //    if ((RST2.FirstOrDefault().MOGODI + RST2.FirstOrDefault().MOGODI_A - (double)WAS_ROW_ITEM.MEGHk - (double)CURRENT_ITMES_ROW.MEGHk - (double)CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && (int)CURRENT_ITMES_ROW.ANBAR > 0)
                //    {
                //        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                //        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                //        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                //        CURRENT_ITMES_ROW.MEGH_R = WAS_ROW_ITEM.MEGH_R;
                //        chek = true;
                //    }
                //}
            }
            #endregion

            #region VAHED_K_After_Update          
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                if (CURRENT_ITMES_ROW?.ANBAR == null || CURRENT_ITMES_ROW?.CODE == null || CURRENT_ITMES_ROW?.VAHED_K == null || CURRENT_ITMES_ROW?.MEGH == null)
                {
                    return;
                }

                var RST = dbms.DoGetDataSQL<HLQ10>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                }
                else if (CURRENT_ITMES_ROW.MABL == 0)
                {
                    CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH_R;
                    CURRENT_ITMES_ROW.MEGH = CURRENT_ITMES_ROW.MEGH_R / RST.FirstOrDefault().NESBAT;
                }
            }
            #endregion

            #region VAHED_K_Not_In_List
            if (e.Column.SortMemberPath == "VAHED_K")
            {
                if (CURRENT_ITMES_ROW?.ANBAR == null || CURRENT_ITMES_ROW?.CODE == null || CURRENT_ITMES_ROW?.VAHED_K == null || CURRENT_ITMES_ROW?.MEGH == null)
                {
                    return;
                }

                var RST = dbms.DoGetDataSQL<HLQ10>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                    CURRENT_ITMES_ROW.VAHED_K = null;
                }
                else
                {
                    CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * RST.FirstOrDefault().NESBAT;
                    if (CURRENT_ITMES_ROW.MABL == 0)
                    {
                        //MABL_K.TabStop = true;
                    }
                    else
                    {
                        //MABL_K.TabStop = true;
                        CURRENT_ITMES_ROW.MABL_K = CURRENT_ITMES_ROW.MEGHk * CURRENT_ITMES_ROW.MABL;
                    }
                }
            }
            #endregion

        }

        private void INVO_LST_HAV_SUB_OTHER_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }
            var ROW = e.Row.Item as INVO_LST_FACTOR22;

            if (ROW?.ANBAR is not null && ROW.VAHED_K is null && ROW.MEGH == 0 && ROW.MEGHk == 0 && ROW.CODE == null)
            {
                INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();
                return;
            }

            if (ROW?.ANBAR is not null && ROW.CODE is null && ROW.MEGH != 0 && ROW.MEGHk != 0)
            {

                INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();
                universControl.PopNotifyShow("کالا نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (!CmdSaveRecord(ROW))
            {
                INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();
            }
        }

        private void INVO_LST_HAV_SUB_OTHER_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                IsDataGridCellFocused = false;
            }
            else //Is Focus inside of INVO_LST_sub
            {
                IsDataGridCellFocused = true;
            }
        }

        private void INVO_LST_HAV_SUB_OTHER_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVO_LST_HAV_SUB_OTHER.SelectedItem is not null)
            {
                if (INVO_LST_HAV_SUB_OTHER.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_HAV_SUB_OTHER.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }

        private void INVO_LST_HAV_SUB_OTHER_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_BTN_Click(null, null);
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
            {
                try
                {
                    if (INVO_LST_HAV_SUB_OTHER?.CurrentCell != null && INVO_LST_HAV_SUB_OTHER.IsEnabled && !INVO_LST_HAV_SUB_OTHER.IsReadOnly)
                    {
                        // Get the current cell
                        DataGridCellInfo currentCell = INVO_LST_HAV_SUB_OTHER.CurrentCell;
                        if (currentCell != null)
                        {
                            // Get the row index and column index of the current cell
                            int rowIndex = INVO_LST_HAV_SUB_OTHER.Items.IndexOf(currentCell.Item);
                            int columnIndex = INVO_LST_HAV_SUB_OTHER.Columns.IndexOf(currentCell.Column);

                            // Check if it's not the first row
                            if (rowIndex > 0)
                            {
                                // Get the value from the cell above
                                object valueAbove = INVO_LST_HAV_SUB_OTHER.Items[rowIndex - 1];

                                // Ensure that the column index is within bounds
                                if (columnIndex >= 0 && columnIndex < INVO_LST_HAV_SUB_OTHER.Columns.Count)
                                {
                                    // Get the column information
                                    var column = INVO_LST_HAV_SUB_OTHER.Columns[columnIndex];

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

                                                INVO_LST_HAV_SUB_OTHER.Items.Refresh();

                                                INVO_LST_HAV_SUB_OTHER.BeginEdit();
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
        }

        private void INVO_LST_HAV_SUB_OTHER_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void INVO_LST_HAV_SUB_OTHER_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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

        private void INVO_LST_HAV_SUB_OTHER_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_HAV_SUB_OTHER.SelectedItem != null && INVO_LST_HAV_SUB_OTHER.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (INVO_LST_HAV_SUB_OTHER.Items.Count > 0)
                {
                    if (!(INVO_LST_HAV_SUB_OTHER.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = INVO_LST_HAV_SUB_OTHER.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = INVO_LST_HAV_SUB_OTHER.SelectedIndex;
                }
            }
        }

        private void INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_HAV_SUB_OTHER.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_HAV_SUB_OTHER.CellEditEnding -= INVO_LST_HAV_SUB_OTHER_CellEditEnding;
                INVO_LST_HAV_SUB_OTHER.RowEditEnding -= INVO_LST_HAV_SUB_OTHER_RowEditEnding;

                if (_RC_ is null)
                {
                    INVO_LST_HAV_SUB_OTHER.CancelEdit();
                }
                else
                {
                    INVO_LST_HAV_SUB_OTHER.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_HAV_SUB_OTHER.RowEditEnding += INVO_LST_HAV_SUB_OTHER_RowEditEnding;
                INVO_LST_HAV_SUB_OTHER.CellEditEnding += INVO_LST_HAV_SUB_OTHER_CellEditEnding;
            });
        }

        private void SAVE_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (HeaderIsValid() == false)
            {
                return;
            }

            if (_navigationManager.IsNewRecord)
            {
                try
                {
                    using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                        {
                            // Fake Query for Lock Table
                            db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);

                            // محاسبه شماره حواله از حسابداری مواد اولیه (TAG=26)
                            var maxNumber = db.Query<double?>("SELECT Max(NUMBER) FROM HEAD_LST WHERE TAG = 26", null, transaction).FirstOrDefault();
                            if (maxNumber == null || maxNumber == 0)
                            {
                                NUMBER.Text = "1";
                            }
                            else
                            {
                                NUMBER.Text = (maxNumber + 1).ToString();
                            }

                            // INSERT رکورد
                            db.Execute(@$"INSERT INTO HEAD_LST (       NUMBER,TAG,                      DATE_N,      TAH, MAS, VAS,                    CUST_NO,          MOLAH, M_NAGHD, MABL_VAR,MABL_HAV,MABL_HAZ,TAKHFIF,DEPATMAN,SHIFT,CUST_KIND,           USER_NAME,                            SGN1,                            SGN2,MBAA,TICMBAA,TKHF,                              OKF,SADER,ARZD,ARZKIND,JAY)
			                                       VALUES ({NUMBER.Text}, 26, {DATE_N.Text.ToRawTarikh()}, N'{TAH.Text}',   0,   0, N'{CUST_NO.SelectedValue}', N'{MOLAH.Text}',       0,        0,       0,       0,  {DEPATMAN.SelectedValue ?? "NULL"}, {CL_Generaly.SHIFT_OF_USER},    1,     NULL, N'{USER_NAME.Text}',{Convert.ToByte(SGN1.IsChecked)},{Convert.ToByte(SGN2.IsChecked)},   0,      0,   {Convert.ToByte(OKF.IsChecked)},  1,    0,   1,  1,  0);", null, transaction);

                            transaction.Commit();
                        }
                    }

                    RefreshAfterUpdate();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                    {
                        new Msgwin(false, "در حال حاضر شماره توسط کاربر دیگری ثبت شده است. لطفا مجددا تلاش کنید تا شماره جدید تخصیص داده شود.").Show();
                    }
                    else
                    {
                        new Msgwin(false, "خطا در انجام عملیات ذخیره، لطفا مجددا امتحان کنید").Show();
                    }
                    return;
                }
                catch (Exception ex)
                {
                    CL_LMethods.DoWriteMyLog("خطا در ذخیره HEAD_LST_HAV_OTHER_WIN", ex);
                    new Msgwin(false, "خطا در انجام عملیات").Show();
                    return;
                }
            }
            else
            {
                //UPDATE
                dbms.DoExecuteSQL(@$"UPDATE HEAD_LST
                                     SET 
                                         DATE_N = {DATE_N.Text.ToRawTarikh()},
                                         TAH = N'{TAH.Text}',
                                         MAS = 0,
                                         VAS = 0,
                                         CUST_NO = N'{CUST_NO.SelectedValue}',
                                         MOLAH = N'{MOLAH.Text}',
                                         M_NAGHD = 0,
                                         MABL_VAR = 0,
                                         MABL_HAV = 0,
                                         MABL_HAZ = 0,
                                         TAKHFIF = 0,
                                         CUST_KIND = NULL,
                                         DEPATMAN={DEPATMAN.SelectedValue ?? "NULL"},
                                         SHIFT = {CL_Generaly.SHIFT_OF_USER},
                                         USER_NAME = N'{USER_NAME.Text}',
                                         SGN1 = {Convert.ToByte(SGN1.IsChecked)},
                                         SGN2 = {Convert.ToByte(SGN2.IsChecked)},
                                         MBAA = 0,
                                         TICMBAA = 0,
                                         TKHF = 1,
                                         OKF = {Convert.ToByte(OKF.IsChecked)},
                                         SADER = 0,
                                         ARZD = 1,
                                         ARZKIND = 1,
                                         JAY = 0
                                     WHERE 
                                         NUMBER = {NUMBER.Text} AND TAG = 26;");
            }

            if (NUMBER.Text is not null && NUMBER.Text != "")
            {
                SGN1.IsEnabled = true;
                SGN2.IsEnabled = true;
            }

            INVO_LST_HAV_SUB_OTHER.IsReadOnly = false;
            INVO_LST_HAV_SUB_OTHER.IsReadOnly = false;


            var col_index = INVO_LST_HAV_SUB_OTHER.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
            INVO_LST_HAV_SUB_OTHER.SelectedIndex = INVO_LST_HAV_SUB_OTHER.Items.Count - 1;
            INVO_LST_HAV_SUB_OTHER.CurrentCell = new DataGridCellInfo(INVO_LST_HAV_SUB_OTHER.SelectedItem, INVO_LST_HAV_SUB_OTHER.Columns[col_index]);


            if (!string.IsNullOrEmpty(NUMBER.Text) && INVO_HAV_OTHER_DATA.Count == 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    INVO_LST_HAV_SUB_OTHER.BeginEdit();

                }), DispatcherPriority.Background);
            }

            universControl.PopNotifyShowUp("ذخیره سربرگ با موفقیت انجام شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
        }

        public bool CmdSaveRecord(INVO_LST_FACTOR22 TheRow)
        {
            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();
            bool CurrentRowisNew = true;
            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                _qre = $@"INSERT INTO INVO_LST (       NUMBER, TAG,         ANBAR,                                           RADIF,             CODE,         MEGH,         MEGHk,                                              MEGH_MAR,                                          MABL,                                            MABL_K,                         FROM_A,                                            MEGH_R,         VAHED_K,                                           N_KOL,                                            N_MOIN,                                            AVRAGE,                                             AVRAGE2,                                           IMBAA,                                              TOTALARZ,                                          TKHN,                                      JAY , MANDAH) 
                          OUTPUT INSERTED.id
			                                                       VALUES ({NUMBER.Text},  26,{TheRow.ANBAR},{(TheRow.RADIF is null ? "NULL" : TheRow.RADIF)}, N'{TheRow.CODE}',{TheRow.MEGH},{TheRow.MEGHk},{(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},{(TheRow.MABL is null ? "NULL" : TheRow.MABL)},{(TheRow.MABL_K is null ? "NULL" : TheRow.MABL_K)},{Convert.ToByte(TheRow.FROM_A)},{(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)},{TheRow.VAHED_K},{(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)},{(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)},{(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)},{(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)},{(TheRow.IMBAA is null ? "NULL" : TheRow.IMBAA)},{(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)},{(TheRow.TKHN is null ? "NULL" : TheRow.TKHN)},{(TheRow.JAY is null ? "0" : TheRow.JAY)},N'{(TheRow.MANDAH is null ? "NULL" : TheRow.MANDAH)}')";

                var (errorMsgs, _, _, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { TheRow }, _qre, null, false);
                ErrosMessages.AddRange(errorMsgs);

                if (queryOutputs.Any())
                {
                    TheRow.id = queryOutputs.FirstOrDefault(); // Update the list with the new ID
                                                               //اصلاح شماره ردیف
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG=26) FROM dbo.INVO_LST WHERE id = {TheRow.id}");
                }
            }
            else //UPDATE
            {
                CurrentRowisNew = false;

                _qre = $@"UPDATE INVO_LST 
                                            SET 
                                                ANBAR = {TheRow.ANBAR},
                                                RADIF = {(TheRow.RADIF is null ? "NULL" : TheRow.RADIF)},
                                                MANDAH = N'{(TheRow.MANDAH is null ? "" : TheRow.MANDAH)}',
                                                CODE = N'{TheRow.CODE}',
                                                MEGH = {TheRow.MEGH},
                                                MEGHk = {TheRow.MEGHk},
                                                MEGH_MAR = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},
                                                MABL = {(TheRow.MABL is null ? "NULL" : TheRow.MABL)},
                                                MABL_K = {(TheRow.MABL_K is null ? "NULL" : TheRow.MABL_K)},
                                                FROM_A = {Convert.ToByte(TheRow.FROM_A)},
                                                MEGH_R = {(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)},
                                                VAHED_K = {TheRow.VAHED_K},
                                                N_KOL = {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)},
                                                N_MOIN = {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)},
                                                AVRAGE = {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)},
                                                AVRAGE2 = {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)},
                                                IMBAA = {(TheRow.IMBAA is null ? "NULL" : TheRow.IMBAA)},
                                                TOTALARZ = {(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)},
                                                TKHN = {(TheRow.TKHN is null ? "NULL" : TheRow.TKHN)},
                                                JAY = {(TheRow.JAY is null ? "0" : TheRow.JAY)}
                                            WHERE NUMBER = {NUMBER.Text} AND id = {TheRow.id} AND TAG = 26";

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

            if (ErrosMessages.Any())
            {
                if (CurrentRowisNew)
                {
                    TheRow.id = null; //Bring Back to null (New State because of Rollback Transaction)
                }
                IVM.RollbackTransaction(); //Undo
            }
            else
            {
                IVM.CommitTransaction(); // Commit Apply Save
            }
            MasterTopErrorMessages.AddRange(ErrosMessages);


            if (MasterTopErrorMessages.Any())
            {
                INVO_LST_HAV_SUB_OTHER_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return false;
            }

            //Undo
            ReGetData();
            return true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_HAV_SUB_OTHER;
            UIElement uie = e.OriginalSource as UIElement;

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (IsDataGridCellFocused)
                    {
                        if (DG.CurrentColumn != null)
                        {
                            int currentColumnIndex = DG.CurrentColumn.DisplayIndex;
                            //bool isLastColumn = currentColumnIndex == DG.Columns.Count - 1;
                            //bool isLastRow = DG.SelectedIndex == DG.Items.Count - 2; //Last Row that is new Empty
                            if (DG.CurrentColumn is not null)
                            {
                                // If it's the last column, move focus to the first cell of next row
                                if (DG.SelectedIndex == DG.Items.Count - 2 && DG.CurrentColumn.SortMemberPath == "MANDAH")
                                {
                                    // Add focus to new row if needed
                                    DG.SelectedIndex++; // DG.SelectedIndex = DG.Items.Count - 1;

                                    DG.CurrentCell = new DataGridCellInfo(DG.SelectedItem, DG.Columns[DEFAULTCOL_INDEX_COL]);

                                    Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        DG.BeginEdit();
                                    }), DispatcherPriority.Background);

                                    return; //وقتی فوکوس کرد الکی تب نزنه وایسه روی همون خونه فوکوس شده در سطر جدید
                                }
                            }
                        }
                    }

                    if (SAVE_BTN.IsFocused)
                    {
                        //Enter Key Continue
                    }
                    else
                    {
                        e.Handled = true;
                        CL_LMethods.SendKey_US(Key.Tab);
                    }

                }
            }
            catch { /*ignore*/ }

            if (!INVO_LST_HAV_SUB_OTHER.IsKeyboardFocusWithin && !INVO_LST_HAV_SUB_OTHER.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }

            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (IsDataGridCellFocused)
                {
                    //DELETE_BTN_Click(null, null);
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.HAV_OTHER_RASID.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = NUMBER.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "چاپ سایر حواله انبار").Show();
        }

        private void ClearFreshAll()
        {
            AnyCrossAzadInvoiced = false;
            NUMBER.Text = "0";

            FNUMCO.Text = "0";

            DATE_N.Text = Tarikh.FullCurrentDate;

            USER_NAME.Text = Baseknow.UUSER;

            hTAG.SelectedIndex = 0; hTAG.Items.Refresh(); //نوع رسید

            CUST_NO.SelectedValue = null; CUST_NO.Items.Refresh(); //نام مشتری
            CUST_NO.Text = null;
            CUST_NO2.SelectedValue = null; CUST_NO2.Items.Refresh(); //نام مشتری

            TAH.Text = null; //تحویل گیرنده
            MOLAH.Text = null; //تحویل دهنده

            OKF.IsChecked = false;

            sgn1usid.Text = null; sgn1usid.Tag = null; SGN1.IsChecked = false;
            sgn2usid.Text = null; sgn2usid.Tag = null; SGN2.IsChecked = false;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER; DEPATMAN.Items.Refresh();

            MOGU.Text = null;
            Text59.Text = "0";
            INVO_HAV_OTHER_DATA?.Clear();

            Command106.IsEnabled = false;


            AllowEdits = true;

            GetDefaultFocus();
        }

        private void GetDefaultFocus()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }

        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_HAV_SUB_OTHER.Items.Count > 0)
            {
                if (INVO_LST_HAV_SUB_OTHER.SelectedItem is not null)
                {
                    var Row = INVO_LST_HAV_SUB_OTHER.SelectedItem as INVO_LST_FACTOR22;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void INVO_LST_sub_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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
    }
}
