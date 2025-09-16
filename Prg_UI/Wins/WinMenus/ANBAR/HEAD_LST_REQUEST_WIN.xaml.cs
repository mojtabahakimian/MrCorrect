using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using System.Diagnostics;
using Functions;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using static Interfaces.INavigator;
using Wins.WinOther;

namespace Wins.WinMenus.ANBAR
{
    /// <summary>
    /// Interaction logic for HEAD_LST_REQUEST_WIN.xaml
    /// </summary>
    public partial class HEAD_LST_REQUEST_WIN : Window, ISearchableWindow
    {

        public HEAD_LST_REQUEST_WIN(double? _NUMBER_ = null, bool _isAutomasion_ = false)
        {
            InitializeComponent();

            this.DataContext = this;

            if (_NUMBER_ is not null)
            {
                NUMBER.Text = _NUMBER_.ToStringNullSafe();
                IsOpenedFromAutomation = _isAutomasion_;
            }
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

        public ObservableCollection<INVO_LST_FACTOR22> INVO_REQUEST_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }
        public bool IsOpenedFromAutomation { get; } = false;
        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        TransactionManagement TM;

        private NavigationManager<HEAD_LST> _navigationManager;

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
                if (_ican is true) // Is Enable and ReadOnly = False
                {
                    ALL_ITEMS_ENABLE();
                }
                else
                {
                    ALL_ITEMS_DISABLE();
                }
            }
        }
        private void ALL_ITEMS_ENABLE()
        {
            NUMBER.IsEnabled = true;
            DATE_N.IsEnabled = true;
            USER_NAME.IsEnabled = true;
            FNUMCO.IsEnabled = true;
            TAH.IsEnabled = true;
            SADER.IsEnabled = true;
            MOGU.IsEnabled = true;
            Text59.IsEnabled = true;
            Command106.IsEnabled = true;
            SAVE_BTN.IsEnabled = true;
            DELETE_BTN.IsEnabled = true;
            this.INVO_LST_REQUEST.IsReadOnly = false;
        }

        private void ALL_ITEMS_DISABLE()
        {
            NUMBER.IsEnabled = false;
            DATE_N.IsEnabled = false;
            USER_NAME.IsEnabled = false;
            FNUMCO.IsEnabled = false;
            TAH.IsEnabled = false;
            SADER.IsEnabled = false;
            MOGU.IsEnabled = false;
            Text59.IsEnabled = false;
            SAVE_BTN.IsEnabled = false;
            DELETE_BTN.IsEnabled = false;
            Command106.IsEnabled = false;
            this.INVO_LST_REQUEST.IsReadOnly = true;
        }

        public object ENTERED_VALUE_ROW { get; private set; }

        public int CURRENT_COLUMN_INDEX { get; private set; }

        public int CURRENT_ROW_INDEX { get; private set; }

        public bool IsDataGridCellFocused { get; private set; }

        private int _DEFAULTCOL_index;

        public int DEFAULTCOL_INDEX_COL
        {
            get
            {
                if (INVO_LST_REQUEST.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_REQUEST.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
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

        private const byte TAG = 23;

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

        List<COMBOPERSONEL> rst_personel = null;
        public int ANBARDefaultValue { get; set; }
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; }

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

        public class Custom_VAHEDK
        {
            public int? VAHED { get; set; }
            public string NAMES { get; set; }
            public string CODE { get; set; }
        }

        private class TobItem
        {
            public int CODE { get; set; }
            public string NAMES { get; set; }
        }
        public class RLQ3
        {
            public string? TAH { get; set; }
        }
        public class RLQ4
        {
            public int? VAHED { get; set; }
            public double? MIN_M { get; set; }
        }
        public class RLQ5
        {
            public string? CODE { get; set; }
            public int? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

        public Visual I_AM_INVO_REQUEST { get; set; }

        public int? DEPATMAN { get; set; }
        public int? MOLAH { get; set; }
        public double? N_S { get; set; }
        public byte? hTAG { get; set; }
        public int? UID { get; set; }
        public int? CUST_NO { get; set; }
        public bool chek { get; private set; }

        private void ReGetData()
        {
            INVO_REQUEST_DATA?.Clear();
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
                                                                    WHERE        (dbo.INVO_LST.TAG = 23) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                // INVO_RASIDA_DATA?.Clear();

                foreach (var item in INVO_RASIDA_DATA_TEMP)
                {
                    INVO_REQUEST_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_REQUEST;
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


            if (e.Key is Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (IsDataGridCellFocused)
                {
                    //DELETE_BTN_Click(null, null);
                }
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_REQUEST, this.GetType().Name);
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "DARKHR", new WindowInteropHelper(this).Handle);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            INVO_LST_REQUEST.IsReadOnly = true;

            I_AM_INVO_REQUEST = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();

            FILL_COMBOBOXES();

            string WhereCondition = $" WHERE (dbo.HEAD_LST.TAG = {TAG}) ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(Convert.ToByte(TAG), WhereCondition);
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

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;
            navigatorControl.NavigationManager = _navigationManager;
            _navigationManager.RaiseInitializationEvents();

            if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
            {
                ALL_ITEMS_DISABLE();
                Command106.IsEnabled = true;
            }
            else
            {
                Command106.IsEnabled = false;
            }

            CL_LMethods.SetTabIndexes(
            DATE_N,
            FNUMCO,
            TAH,
            SAVE_BTN,
            INVO_LST_REQUEST
            );

            GetDefaultFocus();
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
                new SearchableProperty { DisplayName = "شماره درخواست", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "تحویل گیرنده", PropertyPath = "TAH", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion

        private void GetDefaultFocus()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }

        private void RefreshAfterUpdate()
        {
            NewRecord = false;
            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {TAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
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
        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll();
            }
            else if (HEADER_FAC == null)
            {
                if (_navigationManager.NUMBER_TO_OPEN != null)
                {
                    new Msgwin(false, "چنین شماره ای وجود ندارد").ShowDialog();
                    return;
                }
            }
            else
            {
                NewRecord = false; //Currrent Record is not new

                DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر
                FNUMCO.Text = string.IsNullOrEmpty(HEADER_FAC?.FNUMCO.ToStringNullSafe()) ? "0" : HEADER_FAC?.FNUMCO.ToStringNullSafe(); //شماره داخلی
                //شماره درخواست
                NUMBER.Text = HEADER_FAC.NUMBER.ToString();
                SADER.SelectedValue = HEADER_FAC.SADER; SADER.Items.Refresh();
                OKF.IsChecked = HEADER_FAC.OKF;

                SGN1.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN1);
                SGN2.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN2);
                SGN3.IsChecked = Convert.ToBoolean(HEADER_FAC.SGN3);
                SGN1.Tag = Convert.ToInt32(HEADER_FAC.sgn1usid);
                SGN2.Tag = Convert.ToInt32(HEADER_FAC.sgn2usid);
                SGN3.Tag = Convert.ToInt32(HEADER_FAC.sgn3usid);
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
                if (HEADER_FAC?.sgn3usid is not null)
                {
                    sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn3usid)?.SAL_NAME;
                }
                else
                {
                    sgn3usid.Text = null;
                }

                string thevalue = HEADER_FAC.TAH.ToStringNullSafe();
                if (TAH.ItemsSource == null)
                {
                    TAH.ItemsSource = new List<RLQ3>();
                }
                if (!((List<RLQ3>)TAH.ItemsSource).Any(item => item?.TAH == thevalue))
                {
                    if (!string.IsNullOrEmpty(thevalue))
                    {
                        ((List<RLQ3>)TAH.ItemsSource).Add(new RLQ3 { TAH = thevalue });
                    }
                }
                TAH.SelectedValue = HEADER_FAC.TAH; TAH.Items.Refresh();

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                ReGetData();

                Form_Current();
            }
        }

        private void FILL_COMBOBOXES()
        {
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

            TAH.ItemsSource = dbms.DoGetDataSQL<RLQ3>("SELECT TAH FROM HEAD_LST WHERE (TAG = 23) GROUP BY TAH ORDER BY TAH").ToList();
            TAH.DisplayMemberPath = "TAH";
            TAH.SelectedValuePath = "TAH";


            //شخصیت
            SADER.ItemsSource = new List<TobItem>
            {
                new TobItem { CODE = 0, NAMES = " داخلی" },
                new TobItem { CODE = 1, NAMES = "خارجی" },
            };
            SADER.DisplayMemberPath = "NAMES";
            SADER.SelectedValuePath = "CODE";
            SADER.SelectedIndex = 0;

            //انبار کالا
            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
        }

        public bool DATE_VALIDATION()
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
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        DATE_N.Text = null;
                        DATE_N.Focus();
                        Date_Is_Valid = false;
                        return false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                DATE_N.Focus();
                Date_Is_Valid = false;
                return false;

            }
            return true;
        }

        public bool VALIDATION()
        {
            if (DATE_N.Text.ToRawTarikh() == null || DATE_N.Text.ToRawTarikh() == "")
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

            if (SADER.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "نوع رسید نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return false;
            }
            return true;
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!IsNull(this.NUMBER.Text))
            {
                DateTime dt;
                dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 23)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 23)", dt, 1);
                if (!IsNull(this.NUMBER.Text))
                {
                    var RST = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM HEAD_LST WHERE TAG = 1 and NUMBER1 =  " + this.NUMBER.Text).ToList();
                    if (RST.Count == 0)
                    {

                        //this["INVO_LST_REQUEST_SUB"].Form.Refresh();
                        if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                        {
                            this.AllowEdits = false;
                            ALL_ITEMS_DISABLE();

                            Command106.IsEnabled = true;
                            PERSONEL.IsEnabled = true;
                            Msgwin msgwin = new Msgwin(false, " اول امضاء را بردارید ...");
                            msgwin.ShowDialog();
                            SGN1.IsEnabled = true;
                            SGN2.IsEnabled = true;
                            SGN3.IsEnabled = true;
                        }
                        else
                        {
                            this.AllowDeletions = true;
                            this.AllowEdits = true;
                            ALL_ITEMS_ENABLE();
                            this.INVO_LST_REQUEST.IsReadOnly = false;
                        }
                    }
                    else
                    {
                        this.AllowDeletions = false;
                        this.INVO_LST_REQUEST.IsReadOnly = true;
                        this.INVO_LST_REQUEST.IsReadOnly = true;
                        Msgwin msgwin = new Msgwin(false, " براي اين درخواست رسيد صادر شده است و قابل تغيير نمي باشد ....!");
                        msgwin.ShowDialog();
                    }
                }
                if (Convert.ToInt32(NUMBER.Text) > 0)
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 36, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                }
                else
                {
                    this.SGN1.IsEnabled = false;
                    this.SGN2.IsEnabled = false;
                    this.SGN3.IsEnabled = false;
                }
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "DARKHR", new WindowInteropHelper(this).Handle);
            }
        }

        private void SAVE_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (VALIDATION() is false)
            {
                return;
            }
            if (DATE_VALIDATION() is false)
            {
                return;
            }
            var number = dbms.DoGetDataSQL<double?>("SELECT MAX(NUMBER)+1 FROM HEAD_LST WHERE TAG = 23").FirstOrDefault();
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                if (number is null)
                {
                    number = 1;
                    NUMBER.Text = number.ToString();
                }
                else
                {
                    NUMBER.Text = number.ToString();
                }

                //INSERT
                dbms.DoExecuteSQL(@$"INSERT INTO HEAD_LST (       NUMBER,TAG,                      DATE_N,                                         TAH, MAS, VAS,                    CUST_NO,                                       MOLAH, M_NAGHD, MABL_VAR,MABL_HAV,MABL_HAZ,TAKHFIF,DEPATMAN,SHIFT,CUST_KIND,           USER_NAME,                            SGN1,                            SGN2,                            SGN3,MBAA,TICMBAA,TKHF,                              OKF,SADER,ARZD,ARZKIND,JAY) 
			                                       VALUES ({NUMBER.Text}, 23, {DATE_N.Text.ToRawTarikh()}, N'{(TAH.Text is null ? "NULL" : TAH.Text)}',   0,   0,               N'{CUST_NO}',       N'{(MOLAH is null ? "NULL" : MOLAH)}',       0,        0,       0,       0,      0,       1,    1,     NULL, N'{USER_NAME.Text}',{Convert.ToByte(SGN1.IsChecked)},{Convert.ToByte(SGN2.IsChecked)},{Convert.ToByte(SGN3.IsChecked)},   0,      0,   {Convert.ToByte(OKF.IsChecked)},  1,    0,   1,  1,  0);");

                RefreshAfterUpdate();
            }
            else
            {
                //UPDATE
                dbms.DoExecuteSQL(@$"UPDATE HEAD_LST
                                     SET 
                                         DATE_N = {DATE_N.Text.ToRawTarikh()},
                                         TAH = N'{(TAH.Text is null ? "NULL" : TAH.Text)}',
                                         MAS = 0,
                                         VAS = 0,
                                         CUST_NO = N'{CUST_NO}',
                                         MOLAH = N'{(MOLAH is null ? "NULL" : MOLAH)}',
                                         M_NAGHD = 0,
                                         MABL_VAR = 0,
                                         MABL_HAV = 0,
                                         MABL_HAZ = 0,
                                         TAKHFIF = 0,
                                         DEPATMAN = 1,
                                         SHIFT = 1,
                                         CUST_KIND = NULL,
                                         USER_NAME = N'{USER_NAME.Text}',
                                         SGN1 = {Convert.ToByte(SGN1.IsChecked)},
                                         SGN2 = {Convert.ToByte(SGN2.IsChecked)},
                                         SGN3 = {Convert.ToByte(SGN3.IsChecked)},
                                         MBAA = 0,
                                         TICMBAA = 0,
                                         TKHF = 1,
                                         OKF = {Convert.ToByte(OKF.IsChecked)},
                                         SADER = 0,
                                         ARZD = 1,
                                         ARZKIND = 1,
                                         JAY = 0
                                     WHERE 
                                         NUMBER = {NUMBER.Text} AND TAG = 23;");
            }

            if (NUMBER.Text is not null && NUMBER.Text != "")
            {
                SGN1.IsEnabled = true;
                SGN2.IsEnabled = true;
                SGN3.IsEnabled = true;
            }

            INVO_LST_REQUEST.IsReadOnly = false;

            var col_index = INVO_LST_REQUEST.Columns.FirstOrDefault(c => c.SortMemberPath == "ANBAR").DisplayIndex;
            INVO_LST_REQUEST.SelectedIndex = INVO_LST_REQUEST.Items.Count - 1;
            INVO_LST_REQUEST.CurrentCell = new DataGridCellInfo(INVO_LST_REQUEST.SelectedItem, INVO_LST_REQUEST.Columns[col_index]);

            if (number != null && INVO_REQUEST_DATA.Count == 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    INVO_LST_REQUEST.BeginEdit();

                }), DispatcherPriority.Background);
            }
        }
        //ERROR
        private void DELETE_BTN_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_BTN.Visibility == Visibility.Visible;
            if (!DELETE_BTN.IsEnabled || INVO_LST_REQUEST.IsReadOnly || !IsVisible) { return; }

            if (DELETE_BTN.IsEnabled)
            {
                var editableCollectionView = INVO_LST_REQUEST.Items as IEditableCollectionView;
                if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
                {
                    try { editableCollectionView.CancelEdit(); } catch { }
                }

                if (INVO_REQUEST_DATA.Count > 0)
                {
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 23)", dt, 1);
                    CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 23)", dt, 1);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "DELETE",
                            tableName: "درخواست خرید",
                            recordId: NUMBER.Text,
                            oldValue: "TAG = 23",
                            newValue: null,
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    if (!(INVO_LST_REQUEST.SelectedItems is null))
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            for (int i = 0; i < INVO_LST_REQUEST.SelectedItems.Count; i++)
                            {
                                var item = INVO_LST_REQUEST.SelectedItems[i];

                                try
                                {
                                    DATA_GRID_On_Delete(item as INVO_LST_FACTOR22);
                                }
                                catch (SqlException ex)
                                {
                                    if (ex.Number == 547)
                                    {
                                        new Msgwin(false, "این آیتم دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                                    }
                                    else
                                    {
                                        new Msgwin(false, "خطا پایگاه داده در انجام عملیات حذف").ShowDialog();
                                    }
                                }
                                catch (Exception)
                                {
                                    new Msgwin(false, "خطا در انجام عملیات حذف").ShowDialog();
                                }

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
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER = {NUMBER.Text} AND TAG = {TAG}");

                            _navigationManager.DeleteCurrentRecord(); //Refresh Record Source
                        }
                        catch (SqlException ex)
                        {
                            if (e != null)
                            {
                                e.Handled = true;
                            }

                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این برگه درخواست دارای اطلاعات وابسته است , ابتدا آنرا حذف کنید").ShowDialog();
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
                    INVO_REQUEST_DATA.Remove(item as INVO_LST_FACTOR22);
                }
                else
                {
                    //شروع اتصال
                    TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR); //Start Transaction 
                    bool IsMogudiOk = true;

                    var _id = item.id;
                    //حذف سطر دیتا گرید از دیتابیس
                    TM.ExecuteSqlCommandCtc($"DELETE FROM INVO_LST WHERE id = {_id} AND TAG = 23");

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

        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.DARKHST_KHARID.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));

            report["NUMBER_PARM"] = NUMBER.Text;
            report["TAG_PARM"] = TAG.ToString();

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "درخواست خرید").Show();
        }

        public bool CmdSaveRecord(INVO_LST_FACTOR22 TheRow)
        {
            //Saving...
            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                TheRow.id = dbms.DoGetDataSQL<long?>($@"INSERT INTO INVO_LST (       NUMBER, TAG,         ANBAR,                                           RADIF,             CODE,         MEGH,         MEGHk,                                              MEGH_MAR,                                          MABL,                                            MABL_K,                         FROM_A,                                            MEGH_R,         VAHED_K,                                           N_KOL,                                            N_MOIN,                                            AVRAGE,                                             AVRAGE2,                                           IMBAA,                                              TOTALARZ,                                          TKHN,                                      JAY ,MANDAH ) 
			                                                       VALUES ({NUMBER.Text},  23,{TheRow.ANBAR},{(TheRow.RADIF is null ? "NULL" : TheRow.RADIF)}, N'{TheRow.CODE}',{TheRow.MEGH},{TheRow.MEGHk},{(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},{(TheRow.MABL is null ? "NULL" : TheRow.MABL)},{(TheRow.MABL_K is null ? "NULL" : TheRow.MABL_K)},{Convert.ToByte(TheRow.FROM_A)},{(TheRow.MEGH_R is null ? "NULL" : TheRow.MEGH_R)},{TheRow.VAHED_K},{(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)},{(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)},{(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)},{(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)},{(TheRow.IMBAA is null ? "NULL" : TheRow.IMBAA)},{(TheRow.TOTALARZ is null ? "NULL" : TheRow.TOTALARZ)},{(TheRow.TKHN is null ? "NULL" : TheRow.TKHN)},{(TheRow.JAY is null ? "0" : TheRow.JAY)} , N'{(TheRow.MANDAH is null ? "" : TheRow.MANDAH)}')").FirstOrDefault();
            }
            else //UPDATE
            {
                dbms.DoExecuteSQL($@"UPDATE INVO_LST 
                                            SET 
                                                ANBAR = {TheRow.ANBAR},
                                                RADIF = {(TheRow.RADIF is null ? "NULL" : TheRow.RADIF)},
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
                                                JAY = {(TheRow.JAY is null ? "0" : TheRow.JAY)},
                                                MANDAH = N'{(TheRow.MANDAH is null ? "" : TheRow.MANDAH)}'
                                            WHERE NUMBER = {NUMBER.Text} AND id = {TheRow.id} AND TAG = 23");
            }

            return true;
        }

        private void Form_Current()
        {
            // اگر گرید جزئیات دارای رکورد باشد، دکمه چاپ (Command106) فعال می‌شود.
            if (INVO_REQUEST_DATA.Any())
            {
                Command106.IsEnabled = true;
            }
            else
            {
                Command106.IsEnabled = false;
            }

            if (Baseknow.SIGN ?? false)
            {
                // SGN2 و SGN3 کنترل‌های چک‌باکس در نظر گرفته شده‌اند.
                if (SGN2.IsChecked == true || SGN3.IsChecked == true)
                {
                    Command106.IsEnabled = true;
                }
                else
                {
                    Command106.IsEnabled = false;
                }
            }

            if (_navigationManager.IsNewRecord)
            {
                // اگر رکورد جدید است، گرید جزئیات قفل می‌شود تا ابتدا اطلاعات اصلی ذخیره شود.
                INVO_LST_REQUEST.IsReadOnly = true;
                INVO_LST_REQUEST.IsEnabled = true;

                // تنظیم مجوزهای فرم برای یک رکورد جدید
                this.AllowDeletions = true;
                this.AllowEdits = true;
            }
            else // برای رکوردهای موجود
            {
                var headRecordCount = dbms.DoGetDataSQL<int>("SELECT COUNT(*) FROM HEAD_LST WHERE TAG = 12 AND NUMBER = @Number", new { Number = this.NUMBER.Text }).FirstOrDefault();
                if (headRecordCount == 0)
                {
                    // اگر رکوردی در head_lst یافت نشد یا کاربر مدیرسیستم است، تمام مجوزها داده می‌شود.
                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                    INVO_LST_REQUEST.IsReadOnly = false;
                }
                else
                {
                    // در غیر این صورت، مجوز حذف و افزودن ردیف جدید در گرید گرفته می‌شود.
                    this.AllowDeletions = false;
                    INVO_LST_REQUEST.IsReadOnly = true;
                }
            }

            if (OKF.IsChecked == true) // فرض شده OKF یک CheckBox است.
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_REQUEST.IsReadOnly = true;
                ESLAH.IsEnabled = true; // دکمه "اصلاح" برای باز کردن قفل فعال می‌شود.
            }

            // این بخش از منطق دکمه چاپ در کد VBA تکرار شده بود.
            if ((bool)Baseknow.SIGN)
            {
                if (SGN2.IsChecked == true || SGN3.IsChecked == true)
                {
                    Command106.IsEnabled = true;
                }
                else
                {
                    Command106.IsEnabled = false;
                }
            }

            if (!string.IsNullOrEmpty(this.NUMBER.Text) && int.TryParse(this.NUMBER.Text, out int numberValue) && numberValue > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 36, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                // برای رکوردهای جدید، کنترل‌های امضا قفل هستند.
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }
        }

        private void Form_BeforeUpdate()
        {
            if (!this.NewRecord && Baseknow.WAR == 1)
            {
                Baseknow.Text44 = false;
                Msgwin msgwin = new Msgwin(true, "تغيرات داده شده ثبت شود؟");
                msgwin.ShowDialog();

                if (!Baseknow.Text44)
                {

                }
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
                var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                }
            }
        }

        private void Form_AfterUpdate()
        {
            if (this.INVO_REQUEST_DATA.Count > 0)
            {
                Command106.IsEnabled = true;
            }
            else
            {
                Command106.IsEnabled = false;
            }

            if (USER_NAME.Text != CL_HESABDARI.UCurrentUser().ToString())
            {
                USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();
            }
        }

        private void INVO_LST_REQUEST_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!(e is null) && INVO_LST_REQUEST.SelectedItem is not null)
            {
                if (INVO_LST_REQUEST.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
                {
                    WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_REQUEST.SelectedItem).Clone() as INVO_LST_FACTOR22;
                }
            }
        }

        private void INVO_LST_REQUEST_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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

            #region CODE_After_Update
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    //Cleaning
                    CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                    CURRENT_ITMES_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                    return;
                }

                double min;
                double MAND;
                //this.VAHED_K.Requery();
                var RST = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    MOGU.Text = null;
                }
                else
                {
                    MOGU.Text = Convert.ToString(RST.FirstOrDefault().MOGODI + RST.FirstOrDefault().MOGODI_A);
                }

                var RST2 = dbms.DoGetDataSQL<RLQ4>("SELECT VAHED , MIN_M from STUF_DEF where CODE = '" + CURRENT_ITMES_ROW.CODE + "'").ToList();
                if (RST2.Count > 0)
                {
                    //CURRENT_ITMES_ROW.VAHED_K = RST2.FirstOrDefault().VAHED;
                    // If IsNull(RST.Fields("MIN_M")) Then
                    min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);
                    // Else
                    // min = Getmin(Me.ANBAR, Me.CODE)
                    // End If
                }
            }
            #endregion

            #region CODE_Not_In_List
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (CURRENT_ITMES_ROW?.ANBAR == null)
                {
                    return;
                }

                if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++" && !IsNull(CURRENT_ITMES_ROW.ANBAR))
                {
                    CURRENT_ITMES_ROW.CODE = "";

                    SERCHK sERCHK = new SERCHK(I_AM_INVO_REQUEST, CURRENT_ITMES_ROW.ANBAR.ToString());
                    sERCHK.ShowDialog();

                    if (FROM_SAERCH_KAL.CODE is null)
                    {
                        INVO_LST_REQUEST_CANCEL_EDIT();
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
                                INVO_LST_REQUEST_CANCEL_EDIT();

                                return;
                            }
                        }
                    }
                    else
                    {
                        CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ITMES_ROW.ANBAR.ToString(), I_AM_INVO_REQUEST);
                        if (FROM_SAERCH_KAL.CODE is null)
                        {

                            INVO_LST_REQUEST_CANCEL_EDIT();

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
                var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "'").ToList();
                if (RST.Count == 0)
                {
                }
                else
                {
                    //CURRENT_ITMES_ROW.VAHED_K = RST.FirstOrDefault().VAHED;
                }
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
                        //else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                        //{
                        //    var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITMES_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITMES_ROW.ANBAR + ")").ToList();
                        //    if (RSTCO2.Count > 0)
                        //    {
                        //        var MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                        //        if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ITMES_ROW.MEGHk), 2) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                        //        {
                        //            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        //            msgwin.ShowDialog();

                        //            CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                        //        }
                        //        else
                        //        {
                        //            var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR).ToList();
                        //            var _WHERE = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + CURRENT_ITMES_ROW.ANBAR;
                        //            if (RSTCO3.Count > 0)
                        //            {
                        //                RSTCO3.FirstOrDefault().MOGODI = MAND - CURRENT_ITMES_ROW.MEGHk;
                        //                RSTCO3.FirstOrDefault().MOGODI_A = 0;
                        //            }
                        //        }
                        //    }
                        //}
                        //else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                        //{
                        //    if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITMES_ROW.MEGH_MAR)) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                        //    {
                        //        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        //        msgwin.ShowDialog();
                        //        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                        //    }
                        //}
                        //else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && Convert.ToInt32(CURRENT_ITMES_ROW.ANBAR) > 0)
                        //{
                        //    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        //    msgwin.ShowDialog();
                        //    CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                        //}
                    }
                }
            }
            #endregion

            #region MEGH_After_Update
            if (e.Column.SortMemberPath == "MEGH")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                {
                    CURRENT_ITMES_ROW.MEGH = 0;
                    return;
                }
                if (CURRENT_ITMES_ROW?.ANBAR is null || CURRENT_ITMES_ROW?.CODE is null || CURRENT_ITMES_ROW?.VAHED_K is null)
                {
                    return;
                }

                double min;
                long Temp;
                double MAND;
                // RST.Open "SELECT CODE , MIN_M FROM STUF_DEF WHERE CODE = '" & Me.CODE & "'", CurrentProject.Connection, adOpenKeyset, adLockOptimistic
                // If RST.RecordCount > 0 Then
                // If IsNull(RST.Fields("MIN_M")) Then
                min = CL_HESABDARI.Getmin((int)CURRENT_ITMES_ROW.ANBAR, CURRENT_ITMES_ROW.CODE);
               

                var RST = dbms.DoGetDataSQL<RLQ5>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").ToList();
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
            }
            #endregion
        }

        private void INVO_LST_REQUEST_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_REQUEST.SelectedItem != null && INVO_LST_REQUEST.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (INVO_LST_REQUEST.Items.Count > 0)
                {
                    if (!(INVO_LST_REQUEST.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = INVO_LST_REQUEST.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = INVO_LST_REQUEST.SelectedIndex;
                }
            }
        }

        private void INVO_LST_REQUEST_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null) { return; }

            var ROW = e.Row.Item as INVO_LST_FACTOR22;
            if (ConstructorRowDetector.IsPristine(ROW)) { INVO_LST_REQUEST_CANCEL_EDIT(); return; }

            if (ROW.ANBAR is null || ROW.NAME_CODE is null && ROW.CODE is null || ROW.MEGH == 0 || ROW.MEGHk == 0)
            {
                INVO_LST_REQUEST_CANCEL_EDIT();
                universControl.PopNotifyShow("مقادیر سطر را صحیح وارد کنید.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            if (!CmdSaveRecord(e.Row.Item as INVO_LST_FACTOR22))
            {
                INVO_LST_REQUEST_CANCEL_EDIT();
            }
        }

        private void INVO_LST_REQUEST_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void INVO_LST_REQUEST_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_BTN_Click(null, null);
            }
        }

        public void ANBAR_LOADITEM()
        {
            var ARST = dbms.DoGetDataSQL<TobItem>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR ORDER BY TCOD_ANBAR.CODE").ToList();
            ANBAR_COLUMN.ItemsSource = ARST;
        }

        private void INVO_LST_REQUEST_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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

        private void INVO_LST_REQUEST_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void INVO_LST_REQUEST_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_REQUEST.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_REQUEST.CellEditEnding -= INVO_LST_REQUEST_CellEditEnding;
                INVO_LST_REQUEST.RowEditEnding -= INVO_LST_REQUEST_RowEditEnding;

                if (_RC_ is null)
                {
                    INVO_LST_REQUEST.CancelEdit();
                }
                else
                {
                    INVO_LST_REQUEST.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_REQUEST.RowEditEnding += INVO_LST_REQUEST_RowEditEnding;
                INVO_LST_REQUEST.CellEditEnding += INVO_LST_REQUEST_CellEditEnding;
            });
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            double td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToInt16(NUMBER.Text), 36);
            SHARH = "'درخواست خريد  شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + this.TAH.SelectedValue + "','" + CL_HESABDARI.GETUSERHES((int)Baseknow.USERCOD) + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("INSERT INTO EVENTS(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                //td = DateTime.Now;
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36,GETDATE()," + Baseknow.USERCOD + " )");

                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 36);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
            }
            CL_HESABDARI.PERSONELUpdate(36, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
            //this.PERSONEL.Visible = true;
            var Meidnum = MID;

            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            sgn1usid.Tag = Baseknow.USERCOD;
            sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
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

            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 23");
        }

        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            double td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToInt16(NUMBER.Text), 36);
            SHARH = "'درخواست خريد  شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + this.TAH.SelectedValue + "','" + CL_HESABDARI.GETUSERHES((int)Baseknow.USERCOD) + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("INSERT INTO EVENTS(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                //td = DateTime.Now;
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36,GETDATE()," + Baseknow.USERCOD + " )");

                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 36);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
            }
            CL_HESABDARI.PERSONELUpdate(36, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
            //this.PERSONEL.Visible = true;
            var Meidnum = MID;

            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            sgn2usid.Tag = Baseknow.USERCOD;
            sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
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

            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 23");
        }

        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            double MID;
            string SHARH;
            double td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToInt16(NUMBER.Text), 36);
            SHARH = "'درخواست خريد  شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + this.TAH.SelectedValue + "','" + CL_HESABDARI.GETUSERHES((int)Baseknow.USERCOD) + "'";
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("INSERT INTO EVENTS(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                //td = DateTime.Now;
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36,GETDATE()," + Baseknow.USERCOD + " )");

                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 36);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME((int)Baseknow.USERCOD) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",36," + this.NUMBER.Text + ",36 )");
            }
            CL_HESABDARI.PERSONELUpdate(36, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
            //this.PERSONEL.Visible = true;
            var Meidnum = MID;

            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            sgn3usid.Tag = Baseknow.USERCOD;
            sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
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

            dbms.DoExecuteSQL("UPDATE HEAD_LST SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(SGN3.IsChecked == true, 1, 0) + $" WHERE TAG = 23 AND NUMBER = " + NUMBER.Text);
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (NUMBER.Text is null || NUMBER.Text == "0" || NUMBER.Text == "" || DATE_N.Text.ToRawTarikh() is null || DATE_N.Text.ToRawTarikh() == "" || PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow("شماره سند و تاریخ نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            CL_HESABDARI.PERSONELUpdate(36, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), "'درخواست خريد  شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + this.TAH.SelectedValue + "','" + CL_HESABDARI.GETUSERHES((int)Baseknow.USERCOD) + "'");
            universControl.PopNotifyShowUp("ارجاع داده شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
        }

        private void BTN_FACTORS_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 23);
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void ClearFreshAll()
        {
            NUMBER.Text = "0";

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            OKF.IsChecked = false;
            SADER.SelectedValue = 0; SADER.Items.Refresh();
            TAH.SelectedValue = null; TAH.Items.Refresh(); TAH.Text = null;

            FNUMCO.Text = "0"; //شماره داخلی
            Text59.Text = "0";

            sgn1usid.Text = null; sgn1usid.Tag = null; SGN1.IsChecked = false;
            sgn2usid.Text = null; sgn2usid.Tag = null; SGN2.IsChecked = false;
            sgn3usid.Text = null; sgn3usid.Tag = null; SGN3.IsChecked = false;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.Text = null;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOGU.Text = null; //موجودی

            INVO_REQUEST_DATA?.Clear();

            Form_Current();

            AllowEdits = true;

            INVO_LST_REQUEST.IsReadOnly = true;

            GetDefaultFocus();
        }

    }
}
