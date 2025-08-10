using MaterialDesignThemes.Wpf;
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
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
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
using SGN_IMODEL = Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED.SGN_IMODEL;
using Wins.WinMenus.KHARID_FORUSH;
using Functions;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace Wins.WinMenus.ANBAR
{
    public partial class HEAD_LST_ENTEGHAL_WIN : Window
    {
        public HEAD_LST_ENTEGHAL_WIN(double? _NUMBER_ = null)
        {
            InitializeComponent();
            this.DataContext = this;


            if (_NUMBER_ != null)
            {
                NUMBER.Text = _NUMBER_.ToString();
                OpenArgs = _NUMBER_.ToString();
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

        private NavigationManager<HEAD_LST> _navigationManager;
        public ObservableCollection<INVO_LST_FACTOR22> HEAD_ENTEGHAL_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        InventoryManager IVM = new InventoryManager();
        public bool NowIsReady { get; private set; }

        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        public object ENTERED_VALUE_ROW { get; private set; }

        public int CURRENT_COLUMN_INDEX { get; private set; }

        public double min = 0;

        public int CURRENT_ROW_INDEX { get; private set; }

        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; } = new INVO_LST_FACTOR22();

        private int _DEFAULTCOL_index;
        public int DEFAULTCOL_INDEX_COL
        {
            get
            {
                if (INVO_LST_ENTEGHAL_SUB.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_ENTEGHAL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "NAME_CODE")?.DisplayIndex;
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

        private double _sum_of_mablk;
        public double SUM_OF_MABLK
        {
            get
            {
                _sum_of_mablk = Math.Round(Convert.ToDouble(HEAD_ENTEGHAL_DATA.Sum(row => row.MABL_K)));
                return _sum_of_mablk;
            }
            set { _sum_of_mablk = value; }
        }

        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (sgn1usid.Tag is not null)
                {
                    _sgn1_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn1usid.Tag), "FFR_FROOSHTX");
                    _sgn1_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn1usid.Tag)));
                }
                return _sgn1_info;
            }
        }
        private SGN_IMODEL _sgn2_info = new SGN_IMODEL();
        public SGN_IMODEL SGN2_INFO
        {
            get
            {
                if (sgn2usid.Tag is not null)
                {
                    _sgn2_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn2usid.Tag), "FFR_HESABTX");
                    _sgn2_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn2usid.Tag)));
                }
                return _sgn2_info;
            }
        }
        private SGN_IMODEL _sgn3_info = new SGN_IMODEL();
        public SGN_IMODEL SGN3_INFO
        {
            get
            {
                if (sgn3usid.Tag is not null)
                {
                    _sgn3_info.USER_SEMAT = CL_HESABDARI.Getusersemat(Convert.ToInt32(sgn3usid.Tag), "FFR_MODIRTX");
                    _sgn3_info.USER_HESAB_NAME = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(sgn3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        public class Custom_VAHEDK
        {
            public int? VAHED { get; set; }
            public string NAMES { get; set; }
            public string CODE { get; set; }
        }

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

        public INVO_LST_FACTOR22? CURRENT_ITMES_ROW { get; private set; }

        public Visual I_AM_HEAD_ENTEGHAL { get; set; }

        UniversControl universControl = new UniversControl();
        //universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

        List<COMBOPERSONEL> rst_personel = null;

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

        private bool chek;
        private bool _newrecord = true;
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

        public string CDDATE { get; set; }
        public string CDTIME { get; set; }
        public string OKDATE { get; set; }
        public string OKTIME { get; set; }
        public string HTAG { get; set; } = "5";

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

        #region LOCALMODEL
        public class HLE_QT
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
            public int? USERCO { get; set; }
        }

        public class HLE_QT2
        {
            public double? MaxOfNUMBER { get; set; }
        }

        public class HLE_QT4
        {
            public string? CODE { get; set; }
            public int? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }

        public class HLE_QT3
        {
            public string? SAL_NAME { get; set; }
            public string? PSAL_NAME { get; set; }
            public int? GRSAL { get; set; }
            public byte ENABL { get; set; }
            public int? IDD { get; set; }
        }

        public class HLE_QT5
        {
            public double? SumOfMABLK { get; set; }
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
        }

        public class HLE_QT6
        {
            public int? VAHED { get; set; }
            public string? NAMES { get; set; }
            public string? CODE { get; set; }
        }

        public class HLE_QT7
        {
            public int? VAHED { get; set; }
            public string? NAMES { get; set; }
        }
        #endregion

        private void ALL_ITEMS_ENABLE()
        {
            NUMBER.IsEnabled = true;
            ANBAR.IsEnabled = true;
            ANBARF.IsEnabled = true;
            FNUMCO.IsEnabled = true;
            DATE_N.IsEnabled = true;
            USER_NAME.IsEnabled = true;
            TAH.IsEnabled = true;
            MOLAH.IsEnabled = true;
            Command100.IsEnabled = true;
            custprint.IsEnabled = true;
            SGN1.IsEnabled = true;
            SGN2.IsEnabled = true;
            SGN3.IsEnabled = true;
            MOGU2.IsEnabled = true;
            MOGU.IsEnabled = true;
            PERSONEL.IsEnabled = true;
            INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
            DELETE_BTN.IsEnabled = true;
            SAVE_BTN.IsEnabled = true;
        }

        private void ALL_ITEMS_DISABLE()
        {
            NUMBER.IsEnabled = false;
            ANBAR.IsEnabled = false;
            ANBARF.IsEnabled = false;
            FNUMCO.IsEnabled = false;
            DATE_N.IsEnabled = false;
            USER_NAME.IsEnabled = false;
            TAH.IsEnabled = false;
            MOLAH.IsEnabled = false;
            Command100.IsEnabled = false;
            custprint.IsEnabled = false;
            SGN1.IsEnabled = false;
            SGN2.IsEnabled = false;
            SGN3.IsEnabled = false;
            MOGU2.IsEnabled = false;
            MOGU.IsEnabled = false;
            PERSONEL.IsEnabled = false;
            INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
            DELETE_BTN.IsEnabled = false;
            SAVE_BTN.IsEnabled = false;
        }

        public byte TAG { get; set; } = 5; //تگ انتقال کالا از انبار به انبار
        public bool PERSONEL_First_Open { get; private set; }
        public object OpenArgs { get; private set; }
        public bool IsDataGridCellFocused { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;

            I_AM_HEAD_ENTEGHAL = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DataGrid_On_Current();
            Fill_ComboBoxes();
            Form_Load();
            USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();

            #region LOADING...
            string WhereCondition = TAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = 5) " : "  ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(TAG, WhereCondition);

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
                x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {TAG}", //On Change for One Record
                Convert.ToDouble(NUMBER.Text)
                );

            if (!string.IsNullOrEmpty(OpenArgs?.ToStringNullSafe()) && _navigationManager.NUMBER_TO_OPEN != null) //Had a paramter passed
            {
                //یعنی این شماره رو پیدا نکرده که اون رو ریست کنه
                new Msgwin(false, $"شما به این شماره {_navigationManager.NUMBER_TO_OPEN} دسترسی ندارید ").Show();
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

            GetDefaultFocus();
        }
        private void GetDefaultFocus()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }

        private void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
        {
            if (_navigationManager.IsNewRecord)
            {
                ClearFreshAll(); //Form_Current(); //should be in this ClearFreshAll(); method too at the end
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
                if (HEADER_FAC is null)
                {
                    new Msgwin(false, "این برگه انتقال خالی است").Show();
                    return;
                }
                //NewRecord = false; //Currrent Record is not new

                NUMBER.Text = HEADER_FAC.NUMBER.ToString();

                if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
                {
                    DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                    USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر

                    //N_S.Text = HEADER_FAC.N_S.ToStringNullSafe();//شماره سند
                    GetSanadsNums(HEADER_FAC.N_S);

                    ANBAR.SelectedValue = HEADER_FAC.ANBAR.ToStringNullSafe();//از انبار
                    ANBARF.ItemsSource = dbms.DoGetDataSQL<HLE_QT>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) and (TCOD_ANBAR.CODE <> " + ANBAR.SelectedValue + ")  ORDER BY TCOD_ANBAR.CODE").ToList();
                    ANBARF.SelectedValuePath = "CODE";
                    ANBARF.DisplayMemberPath = "NAMES";
                    ANBARF.SelectedValue = HEADER_FAC.ANBARF.ToStringNullSafe();//از انبار

                    TAH.Text = HEADER_FAC.TAH.ToStringNullSafe();//از انبار
                    MOLAH.Text = HEADER_FAC.MOLAH.ToStringNullSafe();//از انبار

                    FNUMCO.Text = string.IsNullOrEmpty(HEADER_FAC?.FNUMCO.ToStringNullSafe()) ? "0" : HEADER_FAC?.FNUMCO.ToStringNullSafe(); //شماره داخلی


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

                    if (HEADER_FAC?.sgn2usid is not null)
                    {
                        sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn2usid)?.SAL_NAME;
                    }

                    if (HEADER_FAC?.sgn3usid is not null)
                    {
                        sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == HEADER_FAC?.sgn3usid)?.SAL_NAME;
                    }

                    OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور

                    USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe();
                    MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات

                    ReGetData();
                }

                if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
                {
                    ALL_ITEMS_DISABLE();
                }

                Form_Current();
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

        public bool DATE_IS_VALID(bool DisplayMsg = false)
        {
            bool Date_Is_Valid = true;

            var DATE = DATE_N.Text.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    }
                    DATE_N.Text = null;
                    //DATE_N.Focus();
                    Date_Is_Valid = false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        if (DisplayMsg)
                        {
                            universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        }
                        DATE_N.Text = null;
                        //DATE_N.Focus();
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                //DATE_N.Focus();
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }

        private void SANAD()
        {
            var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADENTEGHAL(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

            if (SanadNumber != null)
            {
                GetSanadsNums(SanadNumber);
            }
            try
            {
            }
            catch (Exception)
            {
                new Msgwin(false, "صدور سند با خطا مواجه شد").ShowDialog();
                return;
            }

        }

        private void GetSanadsNums(double? SanadNumber)
        {
            var _MABNA_ = dbms.DoGetDataSQL<int?>($"SELECT TOP 1 base FROM dbo.DEED_HED WHERE N_S = {SanadNumber}").FirstOrDefault();

            if (_MABNA_ != null)
            {
                N_S.Text = SanadNumber.ToString();
                MABNA.Text = _MABNA_.ToString();

                dbms.DoExecuteSQL($@"UPDATE HEAD_LST SET N_S = {SanadNumber} WHERE NUMBER = {NUMBER.Text} AND TAG = 5"); //بروز رسانی سند
            }
        }

        private void ANBAR_BeforeUpdate()
        {
            if (ANBAR.SelectedValue is null)
            {
                return;
            }
            if (NUMBER.Text == "" || NUMBER.Text is null)
            {
                return;
            }
            var min = default(double);
            var rst = dbms.DoGetDataSQL<INVO_LST_FACTOR22>("SELECT INVO_LST.NUMBER, INVO_LST.TAG, INVO_LST.ANBAR, INVO_LST.RADIF, INVO_LST.CODE, INVO_LST.MEGH, INVO_LST.MEGHk, INVO_LST.MEGH_MAR, INVO_LST.MANDAH, INVO_LST.MABL, INVO_LST.MABL_K, INVO_LST.FROM_A, INVO_LST.N_RASID, INVO_LST.MEGH_R, INVO_LST.RADAH, INVO_LST.SANAD_NO, INVO_LST.CUST_NO, INVO_LST.ANBARF, INVO_LST.VAHED_K FROM INVO_LST WHERE ((INVO_LST.NUMBER = " + this.NUMBER.Text + ") AND ((INVO_LST.TAG)=5))").ToList();
            if (rst.Count > 0)
            {
                Msgwin msgwin = new Msgwin(false, "برگه داراي سطر كالا ميباشد نميتوانيد انبار را تغيير دهيد اول كالاهها را پاك كنيد");
                msgwin.ShowDialog();
                return;
            }

            if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
            {
                rst = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($"SELECT INVO_LST.NUMBER,INVO_LST.TAG,INVO_LST.ANBAR,INVO_LST.RADIF,INVO_LST.CODE,INVO_LST.MEGH,INVO_LST.MEGHk,INVO_LST.MEGH_MAR,INVO_LST.MANDAH,INVO_LST.MABL,INVO_LST.MABL_K,INVO_LST.FROM_A,INVO_LST.N_RASID,INVO_LST.MEGH_R,INVO_LST.RADAH,INVO_LST.SANAD_NO,INVO_LST.CUST_NO,INVO_LST.ANBARF,INVO_LST.VAHED_K FROM INVO_LST WHERE ((INVO_LST.NUMBER = {NUMBER.Text}) AND ((INVO_LST.TAG)=5))").ToList();
                if (rst.Count > 0)
                {
                    var RST2 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst.FirstOrDefault().CODE} AND ANBAR = {this.ANBAR.SelectedValue}").ToList();
                    var rst3 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM STUF_DEF WHERE CODE = {rst.FirstOrDefault().CODE}").ToList();
                    for (int i = 0; i < rst.Count; i++) //while (!rst.EOF())
                    {
                        if (RST2.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "كالا متعلق به انبار انتخاب شده نيست ابتدا در بخش تعريف كالا تعلق كالا به انبار را ايجاد كنيد.كد كالا :" + rst[i].CODE);
                            msgwin.ShowDialog();
                        }
                        if (rst3.Count == 0)
                        {
                        }
                        else
                        {
                            min = rst3.FirstOrDefault().MIN_M;
                        }
                        var RST2_FILTER = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {this.ANBAR.SelectedValue}").ToList();
                        if (RST2_FILTER.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                            msgwin.ShowDialog();
                            return;
                        }
                        else if (RST2_FILTER.FirstOrDefault().MOGODI + RST2_FILTER.FirstOrDefault().MOGODI_A - rst.FirstOrDefault().MEGHk < min && Convert.ToInt32(this.ANBAR.SelectedValue) > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "انتقال  كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "كد كالا : " + rst[i].CODE + "حداقل موجودي تعريف شده در اف دو :" + min);
                            msgwin.ShowDialog();
                            return;
                        }
                    }
                    for (int i = 0; i < rst.Count; i++)
                    {
                        var RST2_FILTER = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {this.ANBAR.SelectedValue}").ToList();
                        var rst3_FILTER = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM STUF_DEF WHERE CODE = {rst[i].CODE}").ToList();
                        if (rst3_FILTER.Count == 0)
                        {
                        }
                        else
                        {
                            min = rst3_FILTER.FirstOrDefault().MIN_M;
                        }
                        if (RST2_FILTER.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                            msgwin.ShowDialog();
                            return;
                        }
                        else
                        {

                            var RST2_FILTER_LAST = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {rst[i].ANBAR}").ToList();
                            RST2_FILTER_LAST.FirstOrDefault().MOGODI = Convert.ToDouble(RST2_FILTER_LAST.FirstOrDefault().MOGODI - rst[i].MEGHk);

                            dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST2_FILTER_LAST.FirstOrDefault().MOGODI} WHERE CODE = {rst[i].CODE} AND ANBAR = {this.ANBAR.SelectedValue}");

                            if (RST2_FILTER_LAST.Count == 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                                msgwin.ShowDialog();
                                return;
                            }
                            else
                            {
                                RST2_FILTER_LAST.FirstOrDefault().MOGODI = Convert.ToDouble(RST2_FILTER_LAST.FirstOrDefault().MOGODI + rst.FirstOrDefault().MEGHk);

                                dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST2_FILTER_LAST.FirstOrDefault().MOGODI} WHERE CODE = {rst[i].CODE} AND ANBAR = {this.ANBAR.SelectedValue}");
                            }
                        }
                        rst.FirstOrDefault().ANBAR = Convert.ToInt32(this.ANBAR.SelectedValue);
                    }
                }
            }
        }

        private void ANBARF_BeforeUpdate()
        {
            if (NUMBER.Text == "" || NUMBER.Text is null)
            {
                return;
            }

            var min = default(double);
            var rst = dbms.DoGetDataSQL<INVO_LST_FACTOR22>("SELECT INVO_LST.NUMBER,INVO_LST.TAG,INVO_LST.ANBAR,INVO_LST.RADIF,INVO_LST.CODE,INVO_LST.MEGH,INVO_LST.MEGHk,INVO_LST.MEGH_MAR,INVO_LST.MANDAH,INVO_LST.MABL,INVO_LST.MABL_K,INVO_LST.FROM_A,INVO_LST.N_RASID,INVO_LST.MEGH_R,INVO_LST.RADAH,INVO_LST.SANAD_NO,INVO_LST.CUST_NO,INVO_LST.ANBARF,INVO_LST.VAHED_K FROM INVO_LST WHERE ((INVO_LST.NUMBER = " + this.NUMBER.Text + ") AND ((INVO_LST.TAG)=5))").ToList();
            if (rst.Count > 0)
            {
                Msgwin msgwin = new Msgwin(false, "برگه داراي سطر كالا ميباشد نميتوانيد انبار را تغيير دهيد اول كالاهها را پاك كنيد");
                msgwin.ShowDialog();
                return;
            };

            rst = dbms.DoGetDataSQL<INVO_LST_FACTOR22>("SELECT INVO_LST.NUMBER,INVO_LST.TAG,INVO_LST.ANBAR,INVO_LST.RADIF,INVO_LST.CODE,INVO_LST.MEGH,INVO_LST.MEGHk,INVO_LST.MEGH_MAR,INVO_LST.MANDAH,INVO_LST.MABL,INVO_LST.MABL_K,INVO_LST.FROM_A,INVO_LST.N_RASID,INVO_LST.MEGH_R,INVO_LST.RADAH,INVO_LST.SANAD_NO,INVO_LST.CUST_NO,INVO_LST.ANBARF,INVO_LST.VAHED_K FROM INVO_LST WHERE ((INVO_LST.NUMBER = " + this.NUMBER.Text + ") AND ((INVO_LST.TAG)=5))").ToList();
            if (rst.Count > 0)
            {


                for (int i = 0; i < rst.Count; i++)
                {
                    var RST2 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst.FirstOrDefault().CODE} AND ANBAR = {rst.FirstOrDefault().ANBAR}").ToList();
                    var rst3 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM STUF_DEF WHERE CODE = {rst.FirstOrDefault().CODE}").ToList();

                    if (RST2.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا متعلق به انبار انتخاب شده نيست ابتدا در بخش تعريف كالا تعلق كالا به انبار را ايجاد كنيد.كد كالا :" + rst[i].CODE);
                        msgwin.ShowDialog();
                        return;
                    }
                    if (rst3.Count == 0)
                    {
                    }
                    else
                    {
                        min = rst3.FirstOrDefault().MIN_M;
                    }
                    var RST2_FILTER = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {rst[i].ANBARF}").ToList();
                    if (rst.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                        msgwin.ShowDialog();
                        return;
                    }
                    else if (RST2_FILTER.FirstOrDefault().MOGODI + RST2_FILTER.FirstOrDefault().MOGODI_A - rst.FirstOrDefault().MEGHk < min && Convert.ToInt32(this.ANBAR.SelectedValue) > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "انتقال  كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "كد كالا : " + rst.FirstOrDefault().CODE + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();
                        return;
                    }
                }
                for (int i = 0; i < rst.Count; i++)
                {
                    var rst3 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM STUF_DEF WHERE CODE = {rst[i].CODE}").ToList();
                    if (rst3.Count == 0)
                    {
                    }
                    else
                    {
                        min = rst3.FirstOrDefault().MIN_M;
                    }
                    var RST2 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {rst[i].ANBARF}").ToList();
                    if (RST2.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                        msgwin.ShowDialog();
                    }
                    else
                    {
                        RST2.FirstOrDefault().MOGODI = Convert.ToDouble(RST2.FirstOrDefault().MOGODI - rst[i].MEGHk);

                        dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST2.FirstOrDefault().MOGODI} WHERE CODE = {rst[i].CODE} AND ANBAR = {rst[i].ANBARF}");

                        var RST2_FILTER_LAST = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {rst[i].CODE} AND ANBAR = {ANBARF.SelectedValue}").ToList();
                        if (RST2_FILTER_LAST.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص مي باشد. با پشتیبانی تماس بگيريد.");
                            msgwin.ShowDialog();
                            return;
                        }
                        else
                        {
                            RST2_FILTER_LAST.FirstOrDefault().MOGODI = Convert.ToDouble(RST2_FILTER_LAST.FirstOrDefault().MOGODI + rst[i].MEGHk);
                            dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST2_FILTER_LAST.FirstOrDefault().MOGODI} WHERE CODE = {rst[i].CODE} AND ANBAR = {this.ANBARF.SelectedValue}");
                        }
                    }

                    rst.FirstOrDefault().ANBARF = Convert.ToInt32(this.ANBARF.SelectedValue);

                    dbms.DoExecuteSQL($"UPDATE INVO_LST SET ANBARF = {rst.FirstOrDefault().ANBARF} WHERE id = {rst[i].id} AND NUMBER = {NUMBER.Text} AND TAG = 5");
                }
            }
        }

        private void Fill_ComboBoxes()
        {
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

            ANBAR.ItemsSource = dbms.DoGetDataSQL<HLE_QT>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) ORDER BY TCOD_ANBAR.CODE").ToList();
            ANBAR.SelectedValuePath = "CODE";
            ANBAR.DisplayMemberPath = "NAMES";

            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<HLE_QT6>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            double MIDDU;
            string SHARH;
            string rptname;
            var NOTPR = default(bool);
            double min;
            if (NOTPR == false || !(bool)SGN1.IsChecked)
            {
                MIDDU = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 6);
                SHARH = "'انتقالي شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  از انبار: " + this.ANBAR.SelectedValue + "','توسط :" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                if (MIDDU > 0d)
                {
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
                    dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MIDDU) + ",STATUS = 1 WHERE IDNUM = " + MIDDU);
                }
                else
                {
                    var td = DateTime.Now;
                    dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6, GETDATE() ," + Baseknow.USERCOD + " )");
                    MIDDU = CL_HESABDARI.Gettaskid(Convert.ToInt32(this.NUMBER.Text), 6);
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
                }
                CL_HESABDARI.PERSONELUpdate(6, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
                this.PERSONEL.Visibility = Visibility.Visible;
                var Meidnum = MIDDU;
                sgn1usid.Tag = Baseknow.USERCOD;
                sgn1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;
            }
            else
            {
                this.SGN1.IsChecked = false;
            }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    ALL_ITEMS_DISABLE();

                    this.Command100.IsEnabled = true;
                    this.custprint.IsEnabled = true;
                    PERSONEL.IsEnabled = true;
                }
            }
            else
            {
                //ALL_ITEMS_ENABLE();

                this.Command100.IsEnabled = false;
                this.custprint.IsEnabled = false;
            }
            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 5");
        }

        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            double MIDDU;
            string SHARH;
            MIDDU = CL_HESABDARI.Gettaskid(Convert.ToInt32(this.NUMBER.Text), 6);
            SHARH = "'انتقالي شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  از انبار: " + this.ANBAR.SelectedValue + "','توسط :" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
            if (MIDDU > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MIDDU) + ",STATUS = 1 WHERE IDNUM = " + MIDDU);
            }
            else
            {
                var td = DateTime.Now;
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6, GETDATE() ," + Baseknow.USERCOD + " )");
                MIDDU = CL_HESABDARI.Gettaskid(Convert.ToInt32(this.NUMBER.Text), 6);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
            }
            var Meidnum = MIDDU;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;
            CL_HESABDARI.PERSONELUpdate(6, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
            this.PERSONEL.Visibility = Visibility.Visible;
            sgn2usid.Tag = Baseknow.USERCOD;
            sgn2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    ALL_ITEMS_DISABLE();

                    this.Command100.IsEnabled = true;
                    this.custprint.IsEnabled = true;
                    PERSONEL.IsEnabled = true;

                }
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.custprint.IsEnabled = false;

                //ALL_ITEMS_ENABLE();
            }
            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 5");
        }

        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            double MIDDU;
            var SHARH = default(string);
            MIDDU = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 6);
            if (MIDDU > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MIDDU) + ",STATUS = 1 WHERE IDNUM = " + MIDDU);
            }
            else
            {
                var td = DateTime.Now;
                SHARH = "'انتقالي شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  از انبار: " + this.ANBAR.SelectedValue + "','توسط :" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6, GETDATE() ," + Baseknow.USERCOD + " )");
                MIDDU = CL_HESABDARI.Gettaskid(Convert.ToDouble(this.NUMBER.Text), 6);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MIDDU + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6 )");
            }
            var Meidnum = MIDDU;
            if (!(bool)this.OKF.IsChecked)
                this.OKF.IsChecked = true;
            CL_HESABDARI.PERSONELUpdate(6, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), SHARH);
            this.PERSONEL.Visibility = Visibility.Visible;
            sgn3usid.Tag = Baseknow.USERCOD;
            sgn3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                if ((bool)SGN1.IsEnabled || (bool)SGN2.IsEnabled || (bool)SGN3.IsEnabled)
                {
                    ALL_ITEMS_DISABLE();

                    this.Command100.IsEnabled = true;
                    this.custprint.IsEnabled = true;
                    PERSONEL.IsEnabled = true;

                }
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.custprint.IsEnabled = false;

                //ALL_ITEMS_ENABLE();
            }
            dbms.DoExecuteSQL($"UPDATE HEAD_LST SET SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} , SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} , SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} , OKF = {Convert.ToByte((bool)OKF.IsChecked)}, sgn1usid = {(sgn1usid.Tag is null ? "NULL" : sgn1usid.Tag)} , sgn2usid = {(sgn2usid.Tag is null ? "NULL" : sgn2usid.Tag)} , sgn3usid = {(sgn3usid.Tag is null ? "NULL" : sgn3usid.Tag)} WHERE NUMBER = {NUMBER.Text} AND TAG = 5");
        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady)
            {
                return;
            }

            #region Selection_Changed
            if (NUMBER.Text is null || NUMBER.Text == "0" || NUMBER.Text == "" || DATE_N.Text.ToRawTarikh() is null || DATE_N.Text.ToRawTarikh() == "" || PERSONEL.SelectedValue is null)
            {
                universControl.PopNotifyShow("شماره سند و تاریخ نمیتواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            if (PERSONEL_First_Open is true)
            {
                PERSONEL_First_Open = false;
                return;
            }

            CL_HESABDARI.PERSONELUpdate(6, Convert.ToDouble(this.NUMBER.Text), Convert.ToInt32(this.PERSONEL.SelectedValue), "'انتقالي شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(this.DATE_N.Text.ToRawTarikh(), "####/##/##") + "  از انبار: " + this.ANBAR.SelectedValue + "','توسط :" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'");
            Msgwin msgwin = new Msgwin(false, "ارجاع داده شد.");
            msgwin.ShowDialog();
            #endregion
        }

        private void ersalbtn_Click(object sender, RoutedEventArgs e)
        {
            string SHARH;
            if (CL_HESABDARI.Sendbefor(Convert.ToDouble(this.NUMBER.Text), 12))
            {
                Baseknow.Text44 = false;
                Msgwin msgwin = new Msgwin(true, "اين  سند انتقال  قبلا ارسال شده آيا مايليد مجددا ارسال شود ؟ ");
                msgwin.ShowDialog();
                if (msgwin.DialogResult != true)
                {
                    return;
                }
            }
            var td = DateTime.Now;
            SHARH = "' سند انتقال شماره: " + this.NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  از انبار: " + this.ANBAR.SelectedValue + "  به انبار: " + this.ANBARF.SelectedValue + "','" + CL_HESABDARI.GETUSERCO(Convert.ToInt32(Baseknow.USERCOD)) + "'";
            dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",6," + this.NUMBER.Text + ",6, GETDATE() ," + Baseknow.USERCOD + " )");
        }

        //نیاری نیست
        private void Form_Current()
        {

            bool ghat;

            this.PERSONEL.Visibility = Visibility.Visible;
            //this.lsanad.ForeColor = 65535;
            if (INVO_LST_ENTEGHAL_SUB.Items.Count > 0)
            {
                this.Command100.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
            }
            //this.ANBARF.Requery();
            if (this.NewRecord)
            {
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
                //this.DATE_N.Tag = "";
                //this.NUMBER.Tag = 0;
                //this.MOLAH.Tag = "";
                MABNA.Text = null;
            }
            else
            {
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
                //this.DATE_N.Tag = this.DATE_N;
                //this.NUMBER.Tag = this.NUMBER;
                //this.MOLAH.Tag = Interaction.IIf(IsNull(this.MOLAH), "", this.MOLAH);
                var rst = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE N_S = {N_S.Text}").ToList();
                if (rst.Count == 0)
                {

                }
                else
                {
                    MABNA.Text = rst.FirstOrDefault().@base.ToString();
                    if (rst.FirstOrDefault().GHATEI)
                    {
                        ghat = true;
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowAdditions = false;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowDeletions = false;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowEditing = false;
                        //this.lsanad.ForeColor = 125;
                    }
                    else
                    {
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowAdditions = true;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowDeletions = true;
                        //this["INVO_LST_ENTEGHAL_SUB"].Form.AllowEditing = true;
                        //this.lsanad.ForeColor = 65535;
                    }
                }
            }
            if ((bool)Baseknow.SIGN)
            {
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    this.Command100.IsEnabled = true;
                    this.custprint.IsEnabled = true;
                }
                else
                {
                    this.Command100.IsEnabled = false;
                    this.custprint.IsEnabled = false;
                }
            }
            if ((bool)OKF.IsChecked)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
                this.ESLAH.IsEnabled = true;
            }
            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 6, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }
        }

        private void Form_BeforeUpdate()
        {

            if (IsNull(this.ANBAR.SelectedValue) || IsNull(this.ANBARF.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "هر دو انبار بايد مشخص شود...!");
                msgwin.ShowDialog();
                return;
            }

            if (NUMBER.Text != "")
            {
                if (Convert.ToInt32(this.NUMBER.Text) > 0)
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 6, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
                }
            }

            this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
            //this.Refresh();
        }

        //نیازی نیست
        private void Form_AfterUpdate()
        {

            if (IsNull(this.ANBAR.SelectedValue) || IsNull(this.ANBARF.SelectedValue))
            {
                Msgwin msgwin = new Msgwin(false, "انبارها نمي تواند داراي مقدار خالي باشد");
                msgwin.ShowDialog();
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
                return;
            }
            else
            {
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
            }
            //this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
            //this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
            // End If
            // End If
            SANAD();
            if (Convert.ToInt32(this.NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 6, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }
        }

        private void Form_Load()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "ESWAP", new WindowInteropHelper(this).Handle);

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
        }

        //نیازی نیست
        private void Form_Open(int Cancel)
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                this.OKF.IsChecked = true;
            }
            else
            {
                this.OKF.IsChecked = false;
            }
            if (!CL_HESABDARI.LETSGO("ESLAHE"))
            {
                ESLAH.IsEnabled = false;
            }
            else
            {
                ESLAH.IsEnabled = true;
                this.ESLAH.Visibility = Visibility.Visible;
            }
            if (!CL_HESABDARI.LETSGO("SEEENT"))
            {
                //this.ServerFilter = "(USER_NAME = N'" + UCurrentUser() + "')";
                //this.Refresh();
            }
            if (!IsNull(this.OpenArgs))
            {
                //this.ServerFilter = this.OpenArgs;
                //this.Refresh();
            }
            else
            {

            }
            if ((bool)Baseknow.SIGN)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
                this.sgn1usid.Visibility = Visibility.Visible;
                this.sgn2usid.Visibility = Visibility.Visible;
                this.sgn3usid.Visibility = Visibility.Visible;
            }
        }


        private void INVO_LST_ENTEGHAL_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
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

            #region CODE_Not_In_List
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim() ||
                    (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe())))
                {
                    #region CODE_NotInList
                    if (ANBAR.SelectedValue is null) // انبار خالی نیست
                    {
                        return;
                    }
                    //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                    if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                    {
                        CURRENT_ITMES_ROW.MEGH = 0;
                        CURRENT_ITMES_ROW.MEGHk = 0;
                        CURRENT_ITMES_ROW.MABL_K = 0;
                        SERCHK sERCHK = new SERCHK(I_AM_HEAD_ENTEGHAL, ANBAR.SelectedValue.ToString());
                        sERCHK.ShowDialog();

                        if (FROM_SAERCH_KAL.CODE is null)
                        {
                            INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
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
                            var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {ANBAR.SelectedValue})").FirstOrDefault();
                            if (!ReferenceEquals(FoundKala, null))
                            {
                                CURRENT_ITMES_ROW.CODE = FoundKala.CODE;
                                CURRENT_ITMES_ROW.NAME_CODE = FoundKala.NAME;

                                CURRENT_ITMES_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITMES_ROW.CODE);
                            }
                            else
                            {
                                var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND dbo.STUF_FSK.ANBAR = {ANBAR.SelectedValue}").ToList();
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

                                    INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();

                                    return;
                                }
                            }
                        }
                        else
                        {
                            CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), ANBAR.SelectedValue.ToString(), I_AM_HEAD_ENTEGHAL);
                            if (FROM_SAERCH_KAL.CODE is null)
                            {

                                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();

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
                    var RST00 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBAR.SelectedValue).ToList();
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
                    if (Convert.ToInt32(ANBAR.SelectedValue) != 0)
                    {
                        if (CURRENT_ITMES_ROW.id > 0)
                        {
                            var RSTCO1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBAR.SelectedValue).ToList();
                            if (RSTCO1.Count == 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                msgwin.ShowDialog();
                            }
                            else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                            {
                                var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + ANBAR.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + ANBAR.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + ANBAR.SelectedValue + ")").ToList();
                                if (RSTCO2.Count > 0)
                                {
                                    var MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                                    if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ITMES_ROW.MEGHk), 2) < min && Baseknow.MOJU && Convert.ToInt32(ANBAR.SelectedValue) > 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();

                                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                    }
                                    else
                                    {
                                        var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBAR.SelectedValue).ToList();
                                        var _WHERE = " WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBAR.SelectedValue;
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
                                if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ITMES_ROW.MEGH_MAR)) < min && Baseknow.MOJU && Convert.ToInt32(ANBAR.SelectedValue) > 0)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                    msgwin.ShowDialog();
                                    CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                }
                            }
                            else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && Convert.ToInt32(ANBAR.SelectedValue) > 0)
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
                double MF1, MF2, MC1, MC2;
                long Temp;
                double min;
                double MAND;
                double MAND2;

                if (string.IsNullOrWhiteSpace(CURRENT_ITMES_ROW?.CODE) || ANBAR.SelectedValue is null)
                {
                    MOGU.Text = null;
                    return;
                }

                var rst = new List<STUF_STK>();
                if (!string.IsNullOrWhiteSpace(CURRENT_ITMES_ROW?.CODE) && ANBAR.SelectedValue != null)
                {
                    rst = dbms.DoGetDataSQL<STUF_STK>(
                        "SELECT * FROM STUF_STK WHERE CODE = @code AND ANBAR = @anbar",
                        new { code = CURRENT_ITMES_ROW.CODE, anbar = ANBAR.SelectedValue })?.ToList() ?? new List<STUF_STK>();
                }

                if (rst.Count == 0)
                {
                    MOGU.Text = null;
                }
                else
                {
                    MOGU.Text = Convert.ToString(rst.FirstOrDefault().MOGODI + rst.FirstOrDefault().MOGODI_A);
                }

                var rst2 = dbms.DoGetDataSQL<STUF_FSK>("SELECT * FROM STUF_FSK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBARF.SelectedValue).ToList();
                if (rst2.Count == 0)
                {
                    var _code = CURRENT_ITMES_ROW.CODE;
                    var _anbar = Convert.ToInt32(ANBARF.SelectedValue);
                    var _fi_a = 0;
                    var _mogodi_a = 0;
                    var _mabl_a = 0;
                    var _mandah_a = 0;

                    dbms.DoExecuteSQL($@"INSERT INTO STUF_FSK (      CODE ,  ANBAR ,  FI_A ,  MOGODI_A ,  MABL_A ,   MANDAH_A)
			                                            VALUES(N'{_code}' ,{_anbar},{_fi_a},{_mogodi_a},{_mabl_a},{_mandah_a})");

                    var rst3 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK where CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + ANBARF.SelectedValue).ToList();
                    if (rst3.Count == 0)
                    {

                        var __code = CURRENT_ITMES_ROW.CODE;
                        var __anbar = Convert.ToInt32(ANBARF.SelectedValue);
                        var __mogodi = Convert.ToDouble(CURRENT_ITMES_ROW.MEGHk);

                        dbms.DoExecuteSQL($@"INSERT INTO STUF_STK (      CODE ,   ANBAR ,    MOGODI)
                                                            VALUES(N'{__code}',{__anbar},{__mogodi})");
                    }
                }

                var rst4 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBARF.SelectedValue}").ToList();
                if (rst4.Count == 0)
                {
                    MOGU2.Text = null;
                }
                else
                {
                    MOGU2.Text = Convert.ToString(rst4.FirstOrDefault().MOGODI + rst4.FirstOrDefault().MOGODI_A);
                }

                //var rst5 = dbms.DoGetDataSQL<STUF_DEF>($"SELECT * FROM STUF_DEF WHERE CODE = {CURRENT_ITMES_ROW.CODE}").ToList();

                //if (rst5.Count == 0)
                //{
                //}
                //else
                //{
                //    CURRENT_ITMES_ROW.VAHED_K = rst5.FirstOrDefault().VAHED;
                //}
                CURRENT_ITMES_ROW.MABL = 0;

                // ميانگين
                CURRENT_ITMES_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITMES_ROW.CODE, Convert.ToInt64(ANBAR.SelectedValue), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                CURRENT_ITMES_ROW.MABL = CURRENT_ITMES_ROW.AVRAGE;
                CURRENT_ITMES_ROW.MABL_K = CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk;
                if (!this.NewRecord)
                {
                    min = CL_HESABDARI.Getmin(Convert.ToInt32(this.ANBAR.SelectedValue), CURRENT_ITMES_ROW.CODE);

                    var rstm = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBAR.SelectedValue}").ToList();

                    if (rstm.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.Show();
                    }
                    else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                    {

                        var rstm2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + this.ANBAR.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + this.ANBAR.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + this.ANBAR.SelectedValue + ")").ToList();
                        if (rstm2.Count > 0)
                        {
                            MAND = Convert.ToDouble(rstm2.FirstOrDefault());
                            if (Math.Round((double)(rstm2.FirstOrDefault() - CURRENT_ITMES_ROW.MEGHk), Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, Convert.ToInt32(Baseknow.DIG)) && Convert.ToInt32(this.ANBAR.SelectedValue) != 0 && Baseknow.MOJU)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                msgwin.ShowDialog();
                                CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                            }
                            else
                            {
                                min = CL_HESABDARI.Getmin(Convert.ToInt32(this.ANBAR.SelectedValue), WAS_ROW_ITEM.CODE);

                                var rstm3 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + WAS_ROW_ITEM.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + this.ANBARF.SelectedValue + ")").ToList();
                                if (rstm3.Count > 0)
                                {
                                    MAND2 = Convert.ToDouble(rstm3.FirstOrDefault());
                                    if (Math.Round((double)(rstm3.FirstOrDefault() - CURRENT_ITMES_ROW.MEGHk), 2) < min && Baseknow.MOJU && Convert.ToInt32(this.ANBAR.SelectedValue) > 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبارفرعي موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                        msgwin.ShowDialog();
                                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                                    }
                                    else
                                    {
                                        //شیوه قدیمی برای کنترل کالا

                                        //var rstm4 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + WAS_ROW_ITEM.CODE + "' AND ANBAR = " + this.ANBARF.SelectedValue).ToList();
                                        //if (rstm4.Count > 0)
                                        //{
                                        //    //ERROR
                                        //    rstm4.FirstOrDefault().MOGODI = Convert.ToDouble(MAND2 - CURRENT_ITMES_ROW.MEGHk);
                                        //    rstm4.FirstOrDefault().MOGODI_A = 0;
                                        //    //rstm4.update();
                                        //}

                                        //var rstm5 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue).ToList();
                                        //if (rstm5.Count > 0)
                                        //{
                                        //    //ERROR
                                        //    rstm5.FirstOrDefault().MOGODI = Convert.ToDouble(MAND - CURRENT_ITMES_ROW.MEGHk);
                                        //    rstm5.FirstOrDefault().MOGODI_A = 0;
                                        //    //rstm5.update();
                                        //}
                                    }
                                }
                            }

                        }
                    }
                    else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                    {
                        if (rstm.FirstOrDefault().MOGODI + rstm.FirstOrDefault().MOGODI_A - (CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR) < min && Baseknow.MOJU && Convert.ToInt32(this.ANBAR.SelectedValue) > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                            CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                        }
                    }
                    else if (rstm.FirstOrDefault().MOGODI + rstm.FirstOrDefault().MOGODI_A - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR < min && Baseknow.MOJU && Convert.ToInt32(this.ANBAR.SelectedValue) > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();
                        CURRENT_ITMES_ROW = WAS_ROW_ITEM;
                    }
                }
            }
            #endregion

            #region MEGH_After_Update
            if (e.Column.SortMemberPath == "MEGH")
            {
                double min;
                long Temp;
                double MAND;
                min = CL_HESABDARI.Getmin(Convert.ToInt32(this.ANBAR.SelectedValue), CURRENT_ITMES_ROW.CODE);

                if (CURRENT_ITMES_ROW.CODE is null)
                {
                    return;
                }

                var rst = dbms.DoGetDataSQL<HLE_QT4>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITMES_ROW.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITMES_ROW.VAHED_K + ")))").ToList();
                if (rst.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.Show();
                }
                else
                {
                    CURRENT_ITMES_ROW.MEGHk = CURRENT_ITMES_ROW.MEGH * rst.FirstOrDefault().NESBAT;
                    if (CURRENT_ITMES_ROW.MABL == 0)
                    {
                        //CURRENT_ITMES_ROW.MABL_K.TabStop = true;
                    }
                    else
                    {
                        //MABL_K.TabStop = false;
                        //MABL_K = MEGHk * mabl;
                    }
                }

                if (string.IsNullOrWhiteSpace(CURRENT_ITMES_ROW.CODE))
                {
                    INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
                    return;
                }

                var rst2 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBAR.SelectedValue}").ToList();
                if (rst2.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.Show();
                }
                else if ((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG))
                {
                    var rst3 = dbms.DoGetDataSQL<double?>("SELECT  ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + this.ANBAR.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + this.ANBAR.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + this.ANBAR.SelectedValue + ")").ToList();
                    if (rst3.Count > 0)
                    {
                        MAND = Convert.ToDouble(rst3.FirstOrDefault());
                        if (Math.Round((double)(MAND - (CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR)), Convert.ToInt32(Baseknow.DIG)) < Math.Round(min, Convert.ToInt32(Baseknow.DIG)) && Convert.ToInt32(this.ANBAR.SelectedValue) != 0)
                        {
                            new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                            CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                            CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;

                            var rst4 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue).ToList();
                            if (rst4.Count > 0)
                            {
                                rst4.FirstOrDefault().MOGODI = MAND;
                                rst4.FirstOrDefault().MOGODI_A = 0;

                                dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst4.FirstOrDefault().MOGODI} , MOGODI_A = {rst4.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue);


                            }

                            var rst5 = dbms.DoGetDataSQL<double?>("SELECT  ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + this.ANBARF.SelectedValue + ")").ToList();
                            if (rst5.Count > 0)
                            {
                                MAND = Convert.ToDouble(rst5.FirstOrDefault());
                            }

                            var rst6 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBARF.SelectedValue).ToList();
                            if (rst6.Count > 0)
                            {

                                rst6.FirstOrDefault().MOGODI = MAND;
                                rst6.FirstOrDefault().MOGODI_A = 0;

                                dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst6.FirstOrDefault().MOGODI} , MOGODI_A = {rst6.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue);

                            }
                        }
                        else
                        {
                            var rst7 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue).ToList();
                            if (rst7.Count > 0)
                            {

                                rst7.FirstOrDefault().MOGODI = Convert.ToDouble(MAND - (CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR));
                                rst7.FirstOrDefault().MOGODI_A = 0;

                                dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst7.FirstOrDefault().MOGODI} , MOGODI_A = {rst7.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBAR.SelectedValue);

                            }

                            var rst8 = dbms.DoGetDataSQL<double?>("SELECT  ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + this.ANBARF.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITMES_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + this.ANBARF.SelectedValue + ")").ToList();
                            if (rst8.Count > 0)
                            {
                                MAND = Convert.ToDouble(rst8.FirstOrDefault());
                            }

                            var rst9 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBARF.SelectedValue).ToList();
                            if (rst9.Count > 0)
                            {

                                rst9.FirstOrDefault().MOGODI = Convert.ToDouble(MAND + CURRENT_ITMES_ROW.MEGHk);
                                rst9.FirstOrDefault().MOGODI_A = 0;


                                dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst9.FirstOrDefault().MOGODI} , MOGODI_A = {rst9.FirstOrDefault().MOGODI_A} WHERE CODE = '" + CURRENT_ITMES_ROW.CODE + "' AND ANBAR = " + this.ANBARF.SelectedValue);
                            }
                        }
                    }
                }
                else if (CURRENT_ITMES_ROW.CODE == WAS_ROW_ITEM.CODE)
                {
                    if (rst2.FirstOrDefault().MOGODI + rst2.FirstOrDefault().MOGODI_A - CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR < min && Baseknow.MOJU)
                    {
                        new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                        CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                        CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                    }
                }
                else if (rst2.FirstOrDefault().MOGODI + rst2.FirstOrDefault().MOGODI_A - CURRENT_ITMES_ROW.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR < min && Baseknow.MOJU)
                {
                    new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).ShowDialog();
                    CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                    CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                }
                var rst10 = dbms.DoGetDataSQL<HLE_QT5>("SELECT Sum(DTL_MANF.MABLK) AS SumOfMABLK, HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR FROM HEAD_MANF INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) WHERE (((HEAD_MANF.CODE) = '" + CURRENT_ITMES_ROW.CODE + "')) GROUP BY HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR").ToList();
                if (rst10.Count > 0)
                {
                    CURRENT_ITMES_ROW.MABL = rst10.FirstOrDefault().SumOfMABLK + rst10.FirstOrDefault().IMBIBE_MANF + rst10.FirstOrDefault().IMBIBE_SAR;
                    CURRENT_ITMES_ROW.MABL_K = CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk;
                    CURRENT_ITMES_ROW.AVRAGE = CURRENT_ITMES_ROW.MABL;
                }
                //ميانگين
                CURRENT_ITMES_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITMES_ROW.CODE, Convert.ToInt64(this.ANBAR.SelectedValue), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                CURRENT_ITMES_ROW.MABL = CURRENT_ITMES_ROW.AVRAGE;
                CURRENT_ITMES_ROW.MABL_K = CURRENT_ITMES_ROW.MABL * CURRENT_ITMES_ROW.MEGHk;
            }
            INVO_LST_ENTEGHAL_SUB_PreviewMouseDown(null, null);
            #endregion
        }

        private void INVO_LST_ENTEGHAL_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }

            #region Validation
            if (CURRENT_ITMES_ROW.CODE is null && CURRENT_ITMES_ROW.VAHED_K is null && CURRENT_ITMES_ROW.MEGH == 0 && CURRENT_ITMES_ROW.MEGHk == 0)
            {
                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
                return;
            }
            if (CURRENT_ITMES_ROW.CODE is null || CURRENT_ITMES_ROW.NAME_CODE is null || CURRENT_ITMES_ROW.VAHED_K is null)
            {

                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
                universControl.PopNotifyShow("کالا و واحد کالا نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            #endregion

            #region DataGrid_Events
            DataGrid_Before_Update();
            DataGrid_After_Update();
            #endregion

            if (!CmdSaveRecord(e.Row.Item as INVO_LST_FACTOR22))
            {
                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
            }

            Text59.Text = SUM_OF_MABLK.ToString();

            //var col_index = INVO_LST_ENTEGHAL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
            //INVO_LST_ENTEGHAL_SUB.SelectedIndex = INVO_LST_ENTEGHAL_SUB.Items.Count - 1;
            //INVO_LST_ENTEGHAL_SUB.CurrentCell = new DataGridCellInfo(INVO_LST_ENTEGHAL_SUB.SelectedItem, INVO_LST_ENTEGHAL_SUB.Columns[col_index]);


            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    INVO_LST_ENTEGHAL_SUB.BeginEdit();

            //}), DispatcherPriority.Background);
        }

        private void INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT(DataGridEditingUnit? _RC_ = null)
        {
            INVO_LST_ENTEGHAL_SUB.Dispatcher.InvokeAsync(() =>
            {
                INVO_LST_ENTEGHAL_SUB.CellEditEnding -= INVO_LST_ENTEGHAL_SUB_CellEditEnding;
                INVO_LST_ENTEGHAL_SUB.RowEditEnding -= INVO_LST_ENTEGHAL_SUB_RowEditEnding;

                if (_RC_ is null)
                {
                    INVO_LST_ENTEGHAL_SUB.CancelEdit();
                }
                else
                {
                    INVO_LST_ENTEGHAL_SUB.CancelEdit((DataGridEditingUnit)_RC_);
                }
                INVO_LST_ENTEGHAL_SUB.RowEditEnding += INVO_LST_ENTEGHAL_SUB_RowEditEnding;
                INVO_LST_ENTEGHAL_SUB.CellEditEnding += INVO_LST_ENTEGHAL_SUB_CellEditEnding;
            });
        }

        private void INVO_LST_ENTEGHAL_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                if (INVO_LST_ENTEGHAL_SUB?.SelectedItem is not null)
                {
                    if (INVO_LST_ENTEGHAL_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_ENTEGHAL_SUB.SelectedItem).Clone() as INVO_LST_FACTOR22;
                    }
                }
            }
        }

        private void DataGrid_On_Current()
        {
            //CL_HESABDARI.SETSECURITYSUB("HEAD_LST_ENTEGHAL", this.Name, "ESWAP", 3);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_ENTEGHAL_SUB, this.GetType().Name);
        }

        private void DataGrid_Before_Update()
        {
            if (string.IsNullOrWhiteSpace(CURRENT_ITMES_ROW?.CODE) || ANBAR.SelectedValue is null)
            {
                return;
            }

            var rst = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBAR.SelectedValue}").ToList();
            if (rst.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                msgwin.Show();
                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
            }
        }

        private void DataGrid_After_Update()
        {
            double min;
            string ST;

            if (string.IsNullOrWhiteSpace(CURRENT_ITMES_ROW?.CODE) || ANBAR.SelectedValue is null)
            {
                return;
            }

            if (USER_NAME.Text != CL_HESABDARI.UCurrentUser().ToString())
            {
                USER_NAME.Text = CL_HESABDARI.UCurrentUser().ToString();
            }
            if (WAS_ROW_ITEM.CODE == "")
            {
            }
            else if (CURRENT_ITMES_ROW.CODE != WAS_ROW_ITEM.CODE)
            {
                var rst = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {WAS_ROW_ITEM.CODE} AND ANBAR = {ANBAR.SelectedValue}").ToList();

                if (rst.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "اطلاعات در مورد اين كالا مغايرت دارد.");
                    msgwin.Show();
                    CURRENT_ITMES_ROW.CODE = WAS_ROW_ITEM.CODE;
                }
                else if (!string.IsNullOrWhiteSpace(WAS_ROW_ITEM.CODE) && ANBAR.SelectedValue is not null)
                {

                    rst.FirstOrDefault().MOGODI = Convert.ToDouble(rst.FirstOrDefault().MOGODI + WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR);

                    dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst.FirstOrDefault().MOGODI} WHERE CODE = {WAS_ROW_ITEM.CODE} AND ANBAR = {ANBAR.SelectedValue}");

                    WAS_ROW_ITEM.MEGHk = 0;
                }
            }
            min = CL_HESABDARI.Getmin(Convert.ToInt32(this.ANBAR.SelectedValue), CURRENT_ITMES_ROW.CODE);


            var rst_second = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBAR.SelectedValue}").ToList();
            if (rst_second.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                msgwin.Show();
            }
            else if (!((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG)))
            {
                if (rst_second.FirstOrDefault().MOGODI + rst_second.FirstOrDefault().MOGODI_A - CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR < min & Baseknow.MOJU)
                {
                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                    msgwin.ShowDialog();
                    CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                    CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                }
                else
                {
                    rst_second.FirstOrDefault().MOGODI = Convert.ToDouble(rst_second.FirstOrDefault().MOGODI - CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR);

                    dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {rst_second.FirstOrDefault().MOGODI} WHERE CODE = {WAS_ROW_ITEM.CODE} AND ANBAR = {ANBAR.SelectedValue}");

                }
            }
            if (!chek && !((bool)Baseknow.RMOG && !IsNull(Baseknow.RMOG)))
            {
                var RST2 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = {CURRENT_ITMES_ROW.CODE} AND ANBAR = {ANBARF.SelectedValue}").ToList();
                if (RST2.Count == 0)
                {

                    RST2.FirstOrDefault().CODE = CURRENT_ITMES_ROW.CODE;
                    RST2.FirstOrDefault().ANBAR = Convert.ToInt32(ANBARF.SelectedValue);
                    RST2.FirstOrDefault().MOGODI = Convert.ToDouble(CURRENT_ITMES_ROW.MEGHk);

                    dbms.DoExecuteSQL($"INSERT INTO STUF_STK (CODE , ANBAR ,MOGODI) VALUES({RST2.FirstOrDefault().CODE},{RST2.FirstOrDefault().ANBAR},{RST2.FirstOrDefault().MOGODI})");

                }
                else if (RST2.FirstOrDefault().MOGODI + RST2.FirstOrDefault().MOGODI_A + CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR < min && Baseknow.MOJU)
                {
                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                    msgwin.ShowDialog();
                    CURRENT_ITMES_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                    CURRENT_ITMES_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                    chek = true;
                }
                else
                {

                    RST2.FirstOrDefault().MOGODI = Convert.ToDouble(RST2.FirstOrDefault().MOGODI + CURRENT_ITMES_ROW.MEGHk - WAS_ROW_ITEM.MEGHk - CURRENT_ITMES_ROW.MEGH_MAR);

                    dbms.DoExecuteSQL($"UPDATE STUF_STK SET MOGODI = {RST2.FirstOrDefault().MOGODI} WHERE CODE = {WAS_ROW_ITEM.CODE} AND ANBAR = {ANBAR.SelectedValue}");
                }
            }
            SANAD();
        }

        private void SAVE_BTN_Click(object sender, RoutedEventArgs e)
        {
            var _SGN1_ = Convert.ToBoolean(SGN1.IsChecked);
            var _SGN2_ = Convert.ToBoolean(SGN2.IsChecked);
            var _SGN3_ = Convert.ToBoolean(SGN3.IsChecked);

            if (_SGN1_ || _SGN2_ || _SGN3_)
            {
                new Msgwin(false, "ابتدا امضا ها را بردارید").ShowDialog();
                return;
            }

            #region Validation
            if (ANBARF.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "به انبار نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return;
            }

            if (ANBAR.SelectedValue is null)
            {
                Msgwin msgwin = new Msgwin(false, "از انبار نمی تواند خالی باشد");
                msgwin.ShowDialog();
                return;
            }

            if (DATE_N.Text.ToRawTarikh() == null || DATE_N.Text.ToRawTarikh() == "")
            {
                Msgwin msgwin = new Msgwin(false, "تاریخ صحیح نمی باشد");
                msgwin.ShowDialog();
                return;
            }
            Form_BeforeUpdate();
            #endregion

            var number = dbms.DoGetDataSQL<double?>("SELECT MAX(NUMBER)+1 FROM HEAD_LST WHERE TAG = 5").FirstOrDefault();

            string? SANAD_NUMBER = null;

            if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
            {
                SANAD_NUMBER = N_S.Text;
            }

            if (_navigationManager.IsNewRecord)
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
                dbms.DoExecuteSQL($@"INSERT INTO HEAD_LST (       NUMBER, TAG,          MOLAH,          TAH,                 ANBAR,                      DATE_N, VAS,                ANBARF,           USER_NAME,                            SGN1,                            SGN2,                            SGN3,                             OKF,                                                                                              FNUMCO , ARZD,                                                           sgn1usid,                                                        sgn2usid,                                                         sgn3usid, ARZKIND,N_S) 
			                                           VALUES ({NUMBER.Text},   5,N'{MOLAH.Text}',N'{TAH.Text}', {ANBAR.SelectedValue}, {DATE_N.Text.ToRawTarikh()},   0,{ANBARF.SelectedValue}, N'{CL_HESABDARI.UCurrentUser()}',{Convert.ToByte(SGN1.IsChecked)},{Convert.ToByte(SGN2.IsChecked)},{Convert.ToByte(SGN3.IsChecked)},{Convert.ToByte(OKF.IsChecked)}, {(string.IsNullOrEmpty(FNUMCO.Text) ? "NULL" : FNUMCO.Text)},    1,  {(string.IsNullOrEmpty(sgn1usid.Tag.ToStringNullSafe()) ? "NULL" : sgn1usid.Tag.ToStringNullSafe())},{(string.IsNullOrEmpty(sgn2usid.Tag.ToStringNullSafe()) ? "NULL" : sgn2usid.Tag.ToStringNullSafe())}, {(string.IsNullOrEmpty(sgn3usid.Tag.ToStringNullSafe()) ? "NULL" : sgn3usid.Tag.ToStringNullSafe())},       1,{SANAD_NUMBER ?? "NULL"})");

                RefreshAfterUpdate();
            }
            else
            {
                //UPDATE
                dbms.DoExecuteSQL($@"UPDATE HEAD_LST SET NUMBER = {NUMBER.Text}, TAG = 5, MOLAH = N'{MOLAH.Text}' , TAH = N'{TAH.Text}', ANBAR = {ANBAR.SelectedValue}, DATE_N = {DATE_N.Text.ToRawTarikh()}, VAS = 0, ANBARF = {ANBARF.SelectedValue}, USER_NAME = N'{CL_HESABDARI.UCurrentUser()}', SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, SGN3 = {Convert.ToByte(SGN3.IsChecked)}, OKF = {Convert.ToByte(OKF.IsChecked)}, FNUMCO = {(string.IsNullOrEmpty(FNUMCO.Text) ? "NULL" : FNUMCO.Text)} , ARZD = 1, sgn1usid = {(string.IsNullOrEmpty(Tag.ToStringNullSafe()) ? "NULL" : Tag.ToStringNullSafe())}, sgn2usid = {(string.IsNullOrEmpty(sgn2usid.Tag.ToStringNullSafe()) ? "NULL" : sgn2usid.Tag.ToStringNullSafe())}, sgn3usid = {(string.IsNullOrEmpty(sgn3usid.Tag.ToStringNullSafe()) ? "NULL" : sgn3usid.Tag.ToStringNullSafe())}, ARZKIND = 1 , N_S = {SANAD_NUMBER ?? "NULL"}

                                        WHERE NUMBER = {NUMBER.Text} AND TAG = 5");
            }



            if (NUMBER.Text is not null && NUMBER.Text != "")
            {
                SGN1.IsEnabled = true;
                SGN2.IsEnabled = true;
                SGN3.IsEnabled = true;
            }

            INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;

            Text59.Text = SUM_OF_MABLK.ToString();

            var col_index = INVO_LST_ENTEGHAL_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
            INVO_LST_ENTEGHAL_SUB.SelectedIndex = INVO_LST_ENTEGHAL_SUB.Items.Count - 1;
            INVO_LST_ENTEGHAL_SUB.CurrentCell = new DataGridCellInfo(INVO_LST_ENTEGHAL_SUB.SelectedItem, INVO_LST_ENTEGHAL_SUB.Columns[col_index]);

            SANAD();

            universControl.PopNotifyShowUp(".ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);

            if (number != null && HEAD_ENTEGHAL_DATA.Count == 0 && !INVO_LST_ENTEGHAL_SUB.IsReadOnly && INVO_LST_ENTEGHAL_SUB.IsEnabled)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    INVO_LST_ENTEGHAL_SUB.BeginEdit();

                }), DispatcherPriority.Background);
            }
        }

        private void ReGetData()
        {
            HEAD_ENTEGHAL_DATA?.Clear();
            if (NUMBER.Text is not null && NUMBER.Text != "")
            {
                var head_enteghal_data = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($@"SELECT        dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.RADIF, dbo.INVO_LST.CODE, dbo.STUF_DEF.NAME AS NAME_CODE, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, 
																						 dbo.INVO_LST.MEGH_MAR, dbo.INVO_LST.MANDAH, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, 
																					   	 dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.N_TAF, dbo.INVO_LST.AVRAGE, dbo.INVO_LST.id, dbo.INVO_LST.AVRAGE2, 
																					 	 dbo.INVO_LST.IMBAA, dbo.INVO_LST.TOTALARZ, dbo.INVO_LST.VISITOR, dbo.INVO_LST.TKHN, dbo.INVO_LST.JAY, dbo.INVO_LST.JAYO, dbo.INVO_LST.CRT, dbo.INVO_LST.UID
																	FROM            dbo.INVO_LST LEFT OUTER JOIN
																						 dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE LEFT OUTER JOIN
																						 dbo.TCOD_ANBAR ON dbo.INVO_LST.ANBAR = dbo.TCOD_ANBAR.CODE LEFT OUTER JOIN
																						 dbo.TCOD_VAHEDS ON dbo.INVO_LST.VAHED_K = dbo.TCOD_VAHEDS.CODE
                                                                    WHERE        (dbo.INVO_LST.TAG = 5) AND (dbo.INVO_LST.NUMBER = {NUMBER.Text})").ToList();

                // HEAD_ENTEGHAL_DATA?.Clear();

                foreach (var item in head_enteghal_data)
                {
                    HEAD_ENTEGHAL_DATA.Add(item);
                }
            }
            else
            {
                return;
            }
        }

        public bool CmdSaveRecord(INVO_LST_FACTOR22 TheRow)
        {
            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            TheRow.ANBAR = Convert.ToInt32(ANBAR.SelectedValue); //براس سهولت کار انبار بالا رو به آیتم سطر هم دادم که از سطر چک کنم

            if (TheRow.id is null || TheRow.id <= 0) //INSERT
            {
                _qre = ($@"INSERT INTO INVO_LST  (        NUMBER,   TAG,                 ANBAR,                                           RADIF,                CODE,                                                MEGH,                                                  MEGHk,                                                    MEGH_MAR,                                                MABL,                                                 MABL_K ,                            FROM_A,                            MEGH_R,                ANBARF,            VAHED_K,                                                 N_KOL,                                                   N_MOIN,                                                   AVRAGE,                                                    AVRAGE2 , MANDAH) 
                                           OUTPUT INSERTED.id
                                                                    VALUES ( {NUMBER.Text},     5, {ANBAR.SelectedValue},{(TheRow.RADIF is null ? "NULL" : TheRow)}, N'{TheRow.CODE}',{(TheRow.MEGH is null ? "NULL" : TheRow.MEGH)},{(TheRow.MEGHk is null ? "NULL" : TheRow.MEGHk)},{(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},{(TheRow.MABL is null ? "NULL" : TheRow.MABL)},{(TheRow.MABL_K is null ? "NULL" : TheRow.MABL_K)},{Convert.ToByte(TheRow.FROM_A)},{Convert.ToByte(TheRow.MEGH_R)},{ANBARF.SelectedValue},{TheRow.VAHED_K},{(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)}, {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)}, {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)}, {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)} , N'{(TheRow.MANDAH is null ? "" : TheRow.MANDAH)}')");


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
                _qre = $@"UPDATE INVO_LST SET   TAG  = 5, 
                                                ANBAR  = {ANBAR.SelectedValue}, 
                                                RADIF  = {(TheRow.RADIF is null ? "NULL" : TheRow.RADIF)}, 
                                                CODE  = N'{TheRow.CODE}', 
                                                MEGH  = {(TheRow.MEGH is null ? "NULL" : TheRow.MEGH)}, 
                                                MEGHk  = {(TheRow.MEGHk is null ? "NULL" : TheRow.MEGHk)}, 
                                                MEGH_MAR  = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)}, 
                                                MABL  = {(TheRow.MABL is null ? "NULL" : TheRow.MABL)}, 
                                                MABL_K  = {(TheRow.MABL_K is null ? "NULL" : TheRow.MABL_K)}, 
                                                FROM_A  = {Convert.ToByte(TheRow.FROM_A)}, 
                                                MEGH_R  = {Convert.ToByte(TheRow.MEGH_R)}, 
                                                ANBARF  = {ANBARF.SelectedValue}, 
                                                VAHED_K  = {TheRow.VAHED_K}, 
                                                N_KOL  = {(TheRow.N_KOL is null ? "NULL" : TheRow.N_KOL)}, 
                                                N_MOIN  = {(TheRow.N_MOIN is null ? "NULL" : TheRow.N_MOIN)}, 
                                                AVRAGE  = {(TheRow.AVRAGE is null ? "NULL" : TheRow.AVRAGE)}, 
                                                AVRAGE2  = {(TheRow.AVRAGE2 is null ? "NULL" : TheRow.AVRAGE2)},
                                                MANDAH = N'{(TheRow.MANDAH is null ? "" : TheRow.MANDAH)}'
                                           WHERE NUMBER = {NUMBER.Text} AND TAG = 5 AND id = {TheRow.id}";


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
                INVO_LST_ENTEGHAL_SUB_CANCEL_EDIT();
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return false;
            }

            return true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_ENTEGHAL_SUB;
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

        private void INVO_LST_ENTEGHAL_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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

        private bool INVO_LST_ROW_Deleter(INVO_LST_FACTOR22 item)
        {
            bool isDeleteSomething = false;

            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
            {
                if (item.id is null)
                {
                    HEAD_ENTEGHAL_DATA.Remove(item as INVO_LST_FACTOR22);
                }
                else
                {
                    TM = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR); //Start Transaction 
                    bool IsMogudiOk = true;

                    var _id = item.id;
                    TM.ExecuteSqlCommandCtc($"DELETE FROM INVO_LST WHERE id = {_id}");

                    var RSTCO1 = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + item.CODE + "' AND ANBAR = " + ANBAR.SelectedValue).ToList();
                    if (RSTCO1.Count == 0)
                    {
                    }
                    else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                    {
                        min = CL_HESABDARI.Getmin(Convert.ToInt32(this.ANBAR.SelectedValue), item.CODE);

                        var RSTCO2 = TM.SqlQueryCtc<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + ANBAR.SelectedValue + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + ANBAR.SelectedValue + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + item.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + ANBAR.SelectedValue + ")").ToList();
                        if (RSTCO2.Count > 0)
                        {
                            var MAND = (double)RSTCO2.FirstOrDefault();
                            if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - item.MEGHk), (int)Baseknow.DIG) < min && Baseknow.MOJU && Convert.ToInt32(ANBAR.SelectedValue) > 0)
                            {
                                IsMogudiOk = false;

                                item.MEGH = 0;
                                item.MEGHk = 0;
                                item.MABL_K = item.MABL_K/*.TAG*/;
                                item.MABL = item.MABL/*.TAG*/;
                                item.CODE = item.CODE/*.TAG*/;
                            }
                            else
                            {
                                var RSTCO3 = TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + item.CODE + "' AND ANBAR = " + ANBAR.SelectedValue).ToList();
                                var _WHERE = " WHERE CODE = '" + item.CODE + "' AND ANBAR = " + ANBAR.SelectedValue;
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
                        new Msgwin(false, $"خروج كالا {item.CODE} از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min).Show();
                        return false;
                    }

                    isDeleteSomething = true;
                    ReGetData();
                    //if (HEAD_ENTEGHAL_DATA.Count == 0)
                    //{
                    //    INVO_LST_ENTEGHAL_SUB.CanUserAddRows = false;
                    //    INVO_LST_ENTEGHAL_SUB.CanUserAddRows = true;
                    //}
                }
            }
            else
            {
                Msgwin msgwin1 = new Msgwin(false, "چیزی برای حذف وجود ندارند");
                msgwin1.ShowDialog();
                return false;
            }

            return isDeleteSomething;
        }

        private void DELETE_BTN_Click(object sender, RoutedEventArgs e)
        {
            var BTN_IS_VIVIBLE = DELETE_BTN.Visibility == Visibility.Visible;

            if (DELETE_BTN.IsEnabled && BTN_IS_VIVIBLE)
            {
                if (HEAD_ENTEGHAL_DATA.Count > 0)
                {
                    if (!(INVO_LST_ENTEGHAL_SUB.SelectedItems is null))
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                        if (msgwin.DialogResult == true)
                        {
                            #region SABEGHEH
                            var dt = DateTime.Now;
                            CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + " ) and (TAG = 5)", dt, 1);
                            CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + " ) and (TAG = 5)", dt, 1);
                            #endregion

                            _ = AuditLogger.LogActionAsync(
                                 actionType: "DELETE",
                                 tableName: "انتقال از انبار به انبار",
                                 recordId: NUMBER.Text,
                                 oldValue: "TAG = 5",
                                 newValue: null,
                                 additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                            List<MsgModel> ErrosMessages = new List<MsgModel>();
                            for (int i = 0; i < INVO_LST_ENTEGHAL_SUB.SelectedItems.Count; i++)
                            {
                                var item = INVO_LST_ENTEGHAL_SUB.SelectedItems[i];

                                if (CL_LMethods.IsNewPlaceHolder(INVO_LST_ENTEGHAL_SUB, item)) { continue; }

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
                            SANAD();
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

                            SANAD();

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
            Text59.Text = SUM_OF_MABLK.ToString();
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
            {
                dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + " ) and (TAG = 5)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + " ) and (TAG = 5)", dt, 1);
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;
                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    this.AllowEdits = false;
                    ALL_ITEMS_DISABLE();
                    Msgwin msgwin = new Msgwin(false, " اول امضاء را برداريد ...");
                    msgwin.ShowDialog();

                    PERSONEL.IsEnabled = true;
                    SGN1.IsEnabled = true;
                    SGN2.IsEnabled = true;
                    SGN3.IsEnabled = true;
                    return;
                }
                else
                {
                    this.AllowEdits = true;
                    ALL_ITEMS_ENABLE();
                    this.INVO_LST_ENTEGHAL_SUB.IsReadOnly = false;
                }
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "ESWAP", new WindowInteropHelper(this).Handle);
            }
        }

        private void INVO_LST_ENTEGHAL_SUB_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void Command100_Click(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.ANBAR.BARGEH_ENTEGHAL.mrt");
            report.Load(pathreport);

            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", CL_CCNNMANAGER.CONNECTION_STR));
            //Parameters

            #region EMZA
            if (SGN1.IsChecked == true)
            {
                //var SAL_NAME = ((TextBox)sgn1usid.Template.FindName("PART_EditableTextBox", sgn1usid)).Text;
                var SAL_NAME = sgn1usid.Text;

                (report.GetComponentByName("nemz") as StiText).Enabled = true;
                (report.GetComponentByName("semat") as StiText).Enabled = true;



                (report.GetComponentByName("nemz") as StiText).Text = SGN1_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat") as StiText).Text = SGN1_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nemz") as StiText).Enabled = false;
                (report.GetComponentByName("semat") as StiText).Enabled = false;
            }

            if (SGN2.IsChecked == true)
            {
                var SAL_NAME = sgn2usid.Text;

                (report.GetComponentByName("nemz2") as StiText).Enabled = true;
                (report.GetComponentByName("semat2") as StiText).Enabled = true;

                (report.GetComponentByName("nemz2") as StiText).Text = SGN2_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat2") as StiText).Text = SGN2_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nemz2") as StiText).Enabled = false;
                (report.GetComponentByName("semat2") as StiText).Enabled = false;
            }

            if (SGN3.IsChecked == true)
            {
                var SAL_NAME = sgn3usid.Text;

                (report.GetComponentByName("nemz3") as StiText).Enabled = true;
                (report.GetComponentByName("semat3") as StiText).Enabled = true;

                (report.GetComponentByName("nemz3") as StiText).Text = SGN3_INFO.USER_HESAB_NAME;
                (report.GetComponentByName("semat3") as StiText).Text = SGN3_INFO.USER_SEMAT;
            }
            else
            {
                (report.GetComponentByName("nemz3") as StiText).Enabled = false;
                (report.GetComponentByName("semat3") as StiText).Enabled = false;
            }
            #endregion

            var Saman_Name = dbms.DoGetDataSQL<string>("SELECT NAME FROM SAZMAN").FirstOrDefault();
            (report.GetComponentByName("SAZNAME") as StiText).Text = Saman_Name.ToString();
            (report.GetComponentByName("Text11") as StiText).Text = $"تاریخ : {DATE_N.Text}";


            report["NUMBER_PARM"] = NUMBER.Text;

            //report.Render(false);

            //report.Render();
            ProcLoader.Stop(Prc);

            //report.Show();

            new Rpts.WINRPT(report, "چاپ برگه انتقال از انبار به انبار").Show();
        }

        private void ANBAR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBAR.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (ANBAR.SelectedValue is null)
            {
                e.Handled = true;
                //universControl.PopNotifyShow("از انبار نمیتواند خالی باشد!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            ANBARF.ItemsSource = dbms.DoGetDataSQL<HLE_QT>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES, OPANBACCESS.USERCO FROM  dbo.TCOD_ANBAR INNER JOIN  dbo.OPANBACCESS ON dbo.TCOD_ANBAR.CODE = dbo.OPANBACCESS.ANBCO WHERE (OPANBACCESS.USERCO = " + Baseknow.USERCOD + " ) and (TCOD_ANBAR.CODE <> " + ANBAR.SelectedValue + ")  ORDER BY TCOD_ANBAR.CODE").ToList();
            ANBARF.SelectedValuePath = "CODE";
            ANBARF.DisplayMemberPath = "NAMES";

            ANBAR_BeforeUpdate();
        }

        private void ANBARF_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (ANBARF.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (ANBARF.SelectedValue is null)
            {
                universControl.PopNotifyShow("به انبار نمیتواند خالی باشد!", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            ANBARF_BeforeUpdate();
        }

        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (!DATE_IS_VALID())
            {
                e.Handled = true; //Cancel Leaving Focus
            }

            //DATE_N.PreviewLostKeyboardFocus -= DATE_N_PreviewLostKeyboardFocus;
            //Dispatcher.InvokeAsync(new Action(() => DATE_N.Focus()), DispatcherPriority.Input);
            //DATE_N.PreviewLostKeyboardFocus += DATE_N_PreviewLostKeyboardFocus;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void INVO_LST_ENTEGHAL_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_BTN_Click(null, null);
            }
        }

        private void INVO_LST_ENTEGHAL_SUB_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HEAD_ENTEGHAL_DATA.Count > 0 && INVO_LST_ENTEGHAL_SUB.Items.Count > 0)
            {
                if (INVO_LST_ENTEGHAL_SUB.SelectedItem != null)
                {
                    var SelectedRow = INVO_LST_ENTEGHAL_SUB.SelectedItem as INVO_LST_FACTOR22;
                    if (SelectedRow?.CODE != null)
                    {

                        int _FANBAR1_ = Convert.ToInt32(ANBAR.SelectedValue);
                        double _CODE_ = Convert.ToInt32(SelectedRow.CODE);
                        double FMOGUDI = 0;

                        var FROM_ANBAR1 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + _FANBAR1_ + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + _FANBAR1_ + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + _CODE_ + "') AND (dbo.STUF_FSK.ANBAR = " + _FANBAR1_ + ")").ToList();
                        if (FROM_ANBAR1.Count > 0)
                        {
                            FMOGUDI = (double)FROM_ANBAR1.FirstOrDefault();
                        }

                        int _TANBAR2_ = Convert.ToInt32(ANBARF.SelectedValue);
                        double TMOGUDI2 = 0;

                        var FROM_ANBAR2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + _TANBAR2_ + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + _TANBAR2_ + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + _CODE_ + "') AND (dbo.STUF_FSK.ANBAR = " + _TANBAR2_ + ")").ToList();
                        if (FROM_ANBAR2.Count > 0)
                        {
                            TMOGUDI2 = (double)FROM_ANBAR2.FirstOrDefault();
                        }

                        MOGU.Text = FMOGUDI.ToString();
                        MOGU2.Text = TMOGUDI2.ToString();
                    }
                }
            }
        }

        private void BTN_INVOCES_Click(object sender, RoutedEventArgs e)
        {
            new FACTORS_LST(Convert.ToByte(HTAG)).Show();
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void BTN_NEW_CLEAR_Click(object sender, RoutedEventArgs e)
        {
            ClearFreshAll();
        }

        private void ClearFreshAll()
        {
            NUMBER.Text = null;

            FNUMCO.Text = null;

            DATE_N.Text = null;

            USER_NAME.Text = Baseknow.UUSER;

            ANBAR.SelectedValue = null; ANBAR.Items.Refresh();
            ANBARF.SelectedValue = null; ANBARF.Items.Refresh();
            TAH.Text = null;
            MOLAH.Text = null;

            N_S.Text = "0";
            N_S.Text = "0";
            MABNA.Text = "0";
            OKF.IsChecked = false;

            sgn1usid.Text = null; sgn1usid.Tag = null; SGN1.IsChecked = false;
            sgn2usid.Text = null; sgn2usid.Tag = null; SGN2.IsChecked = false;
            sgn3usid.Text = null; sgn3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.USER_SEMAT = null;
            _sgn1_info.USER_HESAB_NAME = null;
            _sgn2_info.USER_SEMAT = null;
            _sgn2_info.USER_HESAB_NAME = null;
            _sgn3_info.USER_SEMAT = null;
            _sgn3_info.USER_HESAB_NAME = null;

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.SelectedIndex = -1; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

            MOGU2.Text = null;
            MOGU.Text = null;
            Text59.Text = "0";
            HEAD_ENTEGHAL_DATA?.Clear();

            ALL_ITEMS_ENABLE();
            INVO_LST_ENTEGHAL_SUB.IsReadOnly = true;

            GetDefaultFocus();
        }
    }
}
