using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Functions.Jostejoo;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinOther;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Wins.WinMenus.ANBAR;
using Syncfusion.Data.Extensions;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using System.Windows.Data;
using System.ComponentModel;

namespace Wins.WinMenus.SANATI
{
    public partial class HAVALAH_EXIT : Window
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
        public HAVALAH_EXIT(double? number_to_open = null)
        {
            InitializeComponent();

            this.DataContext = this;

            if (number_to_open != null)
            {
                NUMBER.Text = number_to_open.ToString();
                NUMBER.UpdateLayout();
            }
        }

        #region LOCALMODEL

        public class DeedHedData
        {
            public string BASE { get; set; }
            public bool GHATEI { get; set; }
        }
        public class VKSQRE1
        {
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public double? MABLKs { get; set; }
        }
        public class VKSQRE2
        {
            public string? CODE { get; set; }
            public int? FNUMB { get; set; }
            public string? CODB { get; set; }
            public int? ANBAR { get; set; }
            public double? MEGHk { get; set; }
            public int? VAHED_K { get; set; }
            public double? MEGH { get; set; }
            public double? PERT { get; set; }
            public double? smabl { get; set; }
            public double? MABLK { get; set; }
        }

        public class N_RASID_MODEL
        {
            public int? FNUMB { get; set; }
            public string? nam { get; set; }
            public int? Expr1 { get; set; }
        }
        #endregion

        private double sum_of_megh_k = 0;
        public double SUM_OF_MEGH_K
        {
            get
            {
                sum_of_megh_k = (double)INVO_LST_FACTOR22_DATA.Sum(r => r.MEGHk ?? 0);
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله

        public ObservableCollection<INVO_LST_FACTOR22> INVO_LST_FACTOR22_DATA { get; } = new ObservableCollection<INVO_LST_FACTOR22>();


        /// <summary>
        /// TAG = 10
        /// </summary>
        public byte FTAG { get; } = 10;

        public bool NowIsReady { get; private set; }
        public bool INVO_LST_SUB_IsFocused { get; private set; }

        private bool _newrecord;
        public bool NewRecord
        {
            get
            {
                if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
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


        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                //مدیر تولید
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), "ANB_KHOROGS_MODIRTTX");
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
                //انبار دار
                if (SGN2usid.Tag is not null)
                {
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), "ANB_KHOROGS_ANBTX");
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
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), "ANB_KHOROGS_ANBTX");
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
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
        public INVO_LST_FACTOR22? WAS_ROW_ITEM { get; private set; } = new INVO_LST_FACTOR22();
        public INVO_LST_FACTOR22 FROM_SEARCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;

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
                CUST_NO.IsReadOnly = !ican;// نام مشتری
                CUST_NO2.IsReadOnly = !ican;// فقط کد مشتری
                MOLAH.IsReadOnly = !ican;// ملاحظات سربرگ

                //INVO_LST_SUB.IsReadOnly = !ican;

                //__ENABLEY
                FNUMCO.IsEnabled = ican;

                DATE_N.IsEnabled = ican;// تاریخ
                CUST_NO.IsEnabled = ican;// نام مشتری
                CUST_NO2.IsEnabled = ican;// فقط کد مشتری
                MOLAH.IsEnabled = ican;// ملاحظات سربرگ

                BTN_SAVE.IsEnabled = ican;

            }
        }

        public int ANBARDefaultValue { get; private set; }
        public double Meidnum { get; private set; }
        public Visual I_AM_VK_SAKHTEH { get; private set; }
        public List<N_RASID_MODEL> N_RASID_ALL { get; private set; }

        private NavigationManager<HEAD_LST> _navigationManager;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VK_SAKHTEH = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            DATE_N.Text = Tarikh.FullCurrentDate;
            USER_NAME.Text = (string)CL_HESABDARI.UCurrentUser();

            SecurityAllCheck();

            FILL_ALL_COMBOBOXES();


            string WhereCondition = $" WHERE (dbo.HEAD_LST.TAG = {FTAG}) ";
            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(Convert.ToByte(FTAG), WhereCondition);

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT * FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
            x => $"SELECT * FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {FTAG}", //On Change for One Record
            Convert.ToDouble(NUMBER.Text)
            );

            // Hook up the OnInsertRecord event
            _navigationManager.CurrentRecordChanged += OnCurrentRecordChanged;
            _navigationManager.OnInsertRecord += OnInsertRecord;

            // Link the navigation manager to the universal control
            navigatorControl.NavigationManager = _navigationManager;

            // Now raise the initialization events to update the UI
            _navigationManager.RaiseInitializationEvents();

            Form_Current();

            if (!NewRecord)
            {
                AllowEdits = false;
            }

            CL_LMethods.SetTabIndexes(
             DATE_N,
             FNUMCO,
             CUST_NO,
             MOLAH,
             BTN_SAVE,
             INVO_LST_SUB
             );

            MakeDefaultFocuseReady();
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
                NUMBER.Text = HEADER_FAC.NUMBER.ToString();
                NUMBER.Tag = HEADER_FAC.NUMBER.ToString();

                DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور
                USER_NAME.Text = HEADER_FAC.USER_NAME.ToStringNullSafe(); //کاربر

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


                OKF.IsChecked = HEADER_FAC.OKF; //تایید فاکتور
                MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات

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

                BTN_SAVE.IsEnabled = false;
                ItwasNewFirstTime = false; //Reset for Sanad Concurrency at first insert

                INVO_LST_SUB_ReGetData();

                GetBalanceInfo();

                Form_Current();
            }
        }
        private void RefreshAfterUpdate()
        {
            NewRecord = false;

            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void MakeDefaultFocuseReady()
        {
            DATE_N.Focus();
            DATE_N.SelectAll();
        }
        private void DataGridActivation()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                INVO_LST_SUB.IsReadOnly = true;
            }
            else
            {
                INVO_LST_SUB.IsReadOnly = false;
            }

            SGN_MANAGER();
            //SecurityAllCheck();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid DG = INVO_LST_SUB;
            UIElement uie = e.OriginalSource as UIElement;

            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                if (INVO_LST_SUB_IsFocused)
                {
                    try
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
                    catch { /*ignore*/ }

                }
                else if (BTN_SAVE.IsFocused)
                {
                    BTN_SAVE.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    return;
                }

                CL_LMethods.SendKey_US(Key.Tab);
            }
            else
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.S || e.SystemKey == Key.S))
                {
                    e.Handled = true;
                    BTN_SAVE_Click(null, null);
                }
            }


            if (e.Key is Key.Enter || e.Key is Key.Tab ||
                e.Key is Key.LeftShift ||
                e.Key is Key.CapsLock ||
                e.Key is Key.Right ||
                e.Key is Key.LeftAlt ||
                e.Key is Key.RightAlt)
            { /* Not Changed */ }
            else
            {
                //Change Happend
                ChangeIsHappend = true;
            }
        }

        private void GetFocusOnDefaultCell()
        {
            var DG = INVO_LST_SUB;

            var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
            CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
        }
        private void SecurityAllCheck()
        {
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "HEXIT", new WindowInteropHelper(this).Handle, this.GetType().Name);
            CL_HESABDARI.SETSECURITYSUB(INVO_LST_SUB, "HEXIT");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
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
            CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            CUST_NO.DisplayMemberPath = "NAME";
            CUST_NO.SelectedValuePath = "hes";

            //حساب یا کد مشتریان
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            CUST_NO2.DisplayMemberPath = "hes";
            CUST_NO2.SelectedValuePath = "hes";

            //انبار کالا
            ANBAR_LOADITEM();

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            //محل مصرف
            N_RASID_ALL = dbms.DoGetDataSQL<N_RASID_MODEL>(@"SELECT dbo.HEAD_MANF.FNUMB, STUF_DEF.NAME+' '+ISNULL(HEAD_MANF.TOZIH, ' ') AS nam, dbo.HEAD_MANF.FNUMB AS Expr1
                                                          FROM dbo.STUF_DEF
                                                               INNER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE=dbo.HEAD_MANF.CODE
                                                          WHERE(NOT(dbo.STUF_DEF.NAME IS NULL))").ToList();
            N_RASID_COLUMN.ItemsSource = N_RASID_ALL;

            //مرکز هزینه
            var RST_SANAD_NO = dbms.DoGetDataSQL<TCOD_MARKAZHAZ>(@"SELECT MHAZ_NO, MHAZNAME FROM TCOD_MARKAZHAZ").ToList();
            SANAD_NO_COLUMN.ItemsSource = RST_SANAD_NO;

            //کبموباکس مجری
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

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            //PERSONEL.SelectedValue = Baseknow.USERCOD;
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

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

            if (CUST_NO.SelectedValue is null) //حساب مشتری
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مسئول شیفت نمیتواند خالی باشد." });
            }

            if (!string.IsNullOrEmpty(DATE_N.Text?.ToRawTarikh()))
            {
                if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                {
                    //تاریخ صحیح نیست
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ فاکتور را بررسی کنید" });
                }
            }


            if (IsNull(this.CUST_NO.SelectedValue) || this.CUST_NO.SelectedIndex < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " مسئول شیفت مشخص نشده است ....!" });
            }
            else if (CL_HESABDARI.BLOCKEDCUST(this.CUST_NO2.SelectedValue.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مسئول شیفت مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
            }

            if (!IsNull(CUST_NO.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(CUST_NO.SelectedValue.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = " حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!" });
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
            // Validate MEGHk
            if (!double.TryParse(TheRow.MEGHk?.ToStringNullSafe(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کل کالا صحیح وارد نشده" });
            }

            // Validate MANDAH
            if (TheRow.MANDAH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "ملاحظات سطر کالا صحیح وارد نشده یا مجاز نیست" });
            }
            // Validate VAHED_K
            if (!int.TryParse(TheRow.VAHED_K?.ToStringNullSafe(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد کالا صحیح وارد نشده" });
            }

            if (!Convert.ToBoolean(Baseknow.FINALS))
            {
                if (string.IsNullOrEmpty(TheRow?.N_RASID))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "محل مصرف نمیتواند خالی باشد" });
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


        public bool ItwasNewFirstTime { get; set; } = false;
        private void BTN_SAVE_Click(object sender, RoutedEventArgs e) //**********************************************************************************************
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            var errors = (from object i in INVO_LST_SUB.ItemsSource
                          let c = INVO_LST_SUB.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();


            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (HeaderIsValid() is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو


            if (NUMBER.Text == "0")
            {
                using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        //Fake Query for Lock Table
                        db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                        //Fake Query for Lock Table

                        var rst_11 = db.Query<double?>($"SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE (((HEAD_LST.TAG)={FTAG}))", null, transaction).FirstOrDefault();
                        if (rst_11 == 0 || ReferenceEquals(rst_11, null))
                        {
                            NUMBER.Text = Baseknow.STTOL.ToString(); //STTO ?
                            NUMBER.UpdateLayout();
                        }
                        else
                        {
                            NUMBER.Text = Convert.ToDouble(rst_11 + 1).ToString();
                            NUMBER.UpdateLayout();
                        }

                        db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER,         NUMBER1,           TAG,     DATE_N,  MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF)
                                               VALUES ({NUMBER.Text}, NULL    ,{FTAG},        0,    0,   0,       0,        0,        0,        0,    0   )", null, transaction);

                        transaction.Commit();
                        db?.Close();

                        ItwasNewFirstTime = true;

                        _navigationManager.IsNewRecord = false;
                        RefreshAfterUpdate();
                    }
                }
            }

            DoCmdHeaderSave();

            this.OKF.IsChecked = true;

            this.INVO_LST_SUB.IsReadOnly = false;

            SANAD();
            if (!ItwasNewFirstTime) //برای جلوگیری از درج داده در صورت فوق همزمان برای درج جدید خالی در درجه اول سند نزنه
            {
            }
            ItwasNewFirstTime = false; //ریست کردن این متفیری

            universControl.PopNotifyShow("اطلاعات با موفقیت ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

            DataGridActivation();

            if (INVO_LST_FACTOR22_DATA.Count == 0)
            {
                GetFocusOnDefaultCell();
            }

            ChangeIsHappend = false;
        }
        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled) { return; }

            if (!IsNull(NUMBER.Text) && NUMBER.Text != "0")
            {
                SecurityAllCheck();

                var dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //1

                var _SGN1_ = Convert.ToBoolean(SGN1.IsChecked ?? false);
                var _SGN2_ = Convert.ToBoolean(SGN2.IsChecked ?? false);
                var _SGN3_ = Convert.ToBoolean(SGN3.IsChecked ?? false);

                if (_SGN1_ || _SGN2_ || _SGN3_)
                {
                    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                    INVO_LST_SUB.IsReadOnly = true;
                    this.AllowEdits = false;
                    this.AllowDeletions = false;
                }
                else
                {
                    INVO_LST_SUB.IsReadOnly = false;
                    this.AllowEdits = true;
                    this.AllowDeletions = true;
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
                    tableName: "برگه ورود کالای ساخته شده",
                    recordId: NUMBER.Text,
                    oldValue: $"TAG = {FTAG}",
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                if (INVO_LST_FACTOR22_DATA.Count > 0 && INVO_LST_SUB.SelectedItems != null && INVO_LST_SUB.SelectedItems.Count > 0)
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //12
                    CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + $") AND (TAG = {FTAG})", dt, 1); //1
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
                else
                {
                    if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0" && !string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.HEAD_LST WHERE NUMBER = {NUMBER.Text} AND NUMBER = {NUMBER.Text} AND TAG = {FTAG}");

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
        private void GetBalanceInfo()
        {
            //کادر سبز و سند و مانده حساب
            var SANAD_NUMBER = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG}").FirstOrDefault();
            if (SANAD_NUMBER != null)
            {
                N_S.Text = SANAD_NUMBER?.ToString();
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S = 9 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
            }
        }
        private bool DoCmdHeaderSave()
        {
            string _qre = null;

            string _n_s = string.IsNullOrEmpty(N_S.Text) ? "NULL" : N_S.Text;
            if (!_n_s.Equals("NULL"))
            {
                _n_s = N_S.Text == "0" ? "NULL" : N_S.Text;
            }

            _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()},
                    N_S = {_n_s},
                    CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    FNUMCO = {(string.IsNullOrEmpty(FNUMCO.Text) ? "0" : FNUMCO.Text)},
                    OKF = {Convert.ToByte(OKF.IsChecked)},
                    USER_NAME = N'{USER_NAME.Text}'
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {FTAG} ";

            _ = dbms.DoExecuteSQL(_qre);


            return true;
        }

        public void INVO_LST_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
            {

                const string SQL_QUERY = @"
                    SELECT  I.NUMBER, I.TAG, I.ANBAR, I.RADIF, I.CODE, 
                            S.NAME AS NAME_CODE, I.MEGH, I.MEGHk, I.MEGH_MAR, 
                            I.MANDAH, I.MABL, I.MABL_K, I.FROM_A, I.N_RASID, 
                            I.MEGH_R, I.RADAH, I.SANAD_NO, I.CUST_NO, I.ANBARF, 
                            I.VAHED_K, I.N_KOL, I.N_MOIN, I.N_TAF, I.AVRAGE, 
                            I.id, I.AVRAGE2, I.IMBAA, I.TOTALARZ, I.VISITOR, 
                            I.TKHN, I.JAY, I.JAYO, I.CRT, I.UID
                    FROM    dbo.INVO_LST I
                    LEFT JOIN dbo.STUF_DEF S ON I.CODE = S.CODE
                    LEFT JOIN dbo.TCOD_ANBAR A ON I.ANBAR = A.CODE
                    LEFT JOIN dbo.TCOD_VAHEDS V ON I.VAHED_K = V.CODE
                    WHERE   I.TAG = @tag AND I.NUMBER = @number";

                var parameters = new Dictionary<string, object>
                        {
                            { "@tag", FTAG },
                            { "@number", double.Parse(NUMBER.Text) }
                        };

                var QRE_LST = dbms.DoGetDataSQL<INVO_LST_FACTOR22>(SQL_QUERY, parameters).ToList();


                INVO_LST_FACTOR22_DATA?.Clear();
                foreach (var item in QRE_LST)
                {
                    INVO_LST_FACTOR22_DATA.Add(item);
                }
            }
            else
            {
                INVO_LST_FACTOR22_DATA?.Clear();
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
                var isEditing = ((IEditableCollectionView)INVO_LST_SUB.Items).IsEditingItem;
                if (isEditing) { return; }

                e.Handled = true;
                BTN_DELETE_Click(null, null);
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
        private void INVO_LST_SUB_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var CurrentRow = e.Row.Item as INVO_LST_FACTOR22;
            //اگر این سطر آیتم های لازم به درستی انتخاب نشده
            if (CurrentRow == null || CurrentRow?.ANBAR == null || string.IsNullOrEmpty(CurrentRow?.CODE))
            {
                return;
            }

            #region VAHED_K
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
            #endregion


            //محل مصرف
            #region N_RASID
            {
                string LastSelected = null; //پیش فرض واحد کالا انتخاب شده از قبل 
                if (CurrentRow?.N_RASID != null)
                {
                    LastSelected = CurrentRow.N_RASID;
                }
                if (e.Column.SortMemberPath == "N_RASID")
                {
                    if ((bool)!Baseknow.FINALS)
                    {
                        var _COMBOBOX_ = e.EditingElement as ComboBox;
                        if (_COMBOBOX_ == null) return;

                        var filteredN_KOL = dbms.DoGetDataSQL<N_RASID_MODEL>(@"SELECT HEAD_MANF.FNUMB, STUF_DEF.NAME + ' ' + isnull(HEAD_MANF.tozih,' ') as nam, HEAD_MANF.FNUMB, DTL_MANF.CODE FROM (HEAD_MANF INNER JOIN DTL_MANF ON HEAD_MANF.FNUMB = DTL_MANF.FNUMB) INNER JOIN STUF_DEF ON HEAD_MANF.CODE = STUF_DEF.CODE WHERE ((Not (STUF_DEF.NAME) Is Null) AND ((DTL_MANF.CODE)='" + CurrentRow.CODE + "'))").ToList();

                        // تنظیم آیتم‌های کمبوباکس
                        _COMBOBOX_.ItemsSource = filteredN_KOL;

                        // تنظیم مقدار انتخاب شده
                        if (!string.IsNullOrEmpty(LastSelected))
                        {
                            _COMBOBOX_.SelectedValue = LastSelected;
                        }
                        else if (filteredN_KOL.Any())
                        {
                            _COMBOBOX_.SelectedValue = filteredN_KOL.FirstOrDefault().FNUMB;
                        }

                        // رفرش کردن آیتم‌ها
                        _COMBOBOX_.Items.Refresh();
                    }
                }
            }
            #endregion


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
                new Msgwin(false, "مسئول شیفت نمیتواند خالی باشد!").ShowDialog();
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


            if (IsNull(CURRENT_ITEMS_ROW?.ANBAR))
            {
                Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد.");
                msgwin.ShowDialog();
            }
            else if (!IsNull(CURRENT_ITEMS_ROW?.CODE))
            {
                var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR).ToList();
                if (RST.Count == 0)
                {
                    Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                    msgwin.ShowDialog();
                    INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                }
            }


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
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        }

                        MEGH_AfterUpdate();
                    }
                }
            }
            #endregion

            //کالا
            #region CODE
            if (e.Column.SortMemberPath == "NAME_CODE")
            {
                if (ENTERED_VALUE_ROW?.ToString() != WAS_ROW_ITEM?.NAME_CODE.ToStringNullSafe().Trim() ||
                    string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    #region CODE_NotInList
                    if (CURRENT_ITEMS_ROW?.ANBAR is null) // انبار خالی نیست
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.Trim()?.ToStringNullSafe()))
                    {
                        CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                        CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                        INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                        return;
                    }

                    //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                    if (true)
                    {
                        //محاسبه موجودی واقعی این کالا
                        min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);

                        var RST_KALA = CL_LMethods.GetKalaBySearch(dbms, Convert.ToString(CURRENT_ITEMS_ROW.ANBAR), ENTERED_VALUE_ROW);
                        if (RST_KALA != null)
                        {
                            CURRENT_ITEMS_ROW.CODE = RST_KALA.CODE;
                            CURRENT_ITEMS_ROW.NAME_CODE = RST_KALA.NAME_CODE;

                            CURRENT_ITEMS_ROW.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ITEMS_ROW.CODE);
                        }
                        else
                        {
                            CURRENT_ITEMS_ROW.CODE = WAS_ROW_ITEM.CODE;
                            CURRENT_ITEMS_ROW.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                            CURRENT_ITEMS_ROW.VAHED_K = WAS_ROW_ITEM.VAHED_K;
                            INVO_LST_SUB_CANCEL_EDIT(DataGridEditingUnit.Cell);
                            new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                            return;
                        }

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


                        // ميانگين
                        UPDATE_LAST_AVRAGE();
                        //if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null)
                        //{
                        //    CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
                        //    CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.AVRAGE;
                        //    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                        //}

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
                }
                #endregion
            }
            #endregion


            //مبلغ کل
            if (e.Column.SortMemberPath == "MABL_K")
            {
                if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                {
                    CURRENT_ITEMS_ROW.MABL_K = 0;
                    return;
                }
                if (
                   CURRENT_ITEMS_ROW?.ANBAR is null ||
                   CURRENT_ITEMS_ROW.CODE is null ||
                   CURRENT_ITEMS_ROW.VAHED_K is null ||
                   CURRENT_ITEMS_ROW.MEGH is null ||
                   CURRENT_ITEMS_ROW.MEGHk is null ||
                   CURRENT_ITEMS_ROW.MABL is null
                   )
                {
                    return;
                }
                else
                {
                    if (CURRENT_ITEMS_ROW.MEGHk == 0)
                    {
                        CURRENT_ITEMS_ROW.MABL_K = 0;
                    }
                    else
                    {
                        CURRENT_ITEMS_ROW.MABL = Convert.ToDouble(CURRENT_ITEMS_ROW.MABL_K) / Convert.ToDouble(CURRENT_ITEMS_ROW.MEGHk);
                    }
                }
            }

            //محل مصرف
            #region N_RASID
            if (e.Column.SortMemberPath == "N_RASID")
            {
                if (!Convert.ToBoolean(Baseknow.FINALS))
                {
                    if (string.IsNullOrEmpty(ENTERED_VALUE_ROW?.ToStringNullSafe()) && CURRENT_ITEMS_ROW?.CODE != null)
                    {
                        CURRENT_ITEMS_ROW.N_KOL = WAS_ROW_ITEM.N_KOL;
                        universControl.PopNotifyShow("محل مصرف ساخت نمی تواند خالی باشد !", Pop1, Pop1Text1, Pop_Border1);
                        INVO_LST_SUB_CANCEL_EDIT();
                        return;
                    }
                }
            }
            #endregion

        }
        private void INVO_LST_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                return;
            }

            if (e.Row.Item == null)
            {
                return;
            }


            var TheRow = e.Row.Item as INVO_LST_FACTOR22;

            if (CL_LMethods.IsNewRowUnmodified(TheRow))
            {
                INVO_LST_SUB_CANCEL_EDIT();
                return;   // The row is unmodified
            }

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
                _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH,FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                              OUTPUT INSERTED.id
                              VALUES({NUMBER.Text},
                              {FTAG} ,
                              {TheRow.ANBAR}   ,
                              NULL,
                              N'{TheRow.CODE}' ,
                              {TheRow.MEGH} ,
                              {TheRow.MEGHk} ,
                              {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)} ,
                              N'{TheRow.MANDAH}' ,
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
                    IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG={FTAG}) FROM dbo.INVO_LST WHERE id = {TheRow.id}");
                }
            }
            else //UPDATE
            {
                _qre = $@"UPDATE dbo.INVO_LST
                   SET ANBAR = {TheRow.ANBAR}, CODE = N'{TheRow.CODE}',
                   MEGH = {TheRow.MEGH}, MEGHk = {TheRow.MEGHk}, MEGH_MAR = {(TheRow.MEGH_MAR is null ? "NULL" : TheRow.MEGH_MAR)},
                   MANDAH = N'{TheRow.MANDAH}',
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
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {TheRow.CODE} به مقدار کل {TheRow.MEGHk} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم , درصورتی که مورد تایید است جهت ذخیره آن مجددا دکمه ذخیره را بزنید" });
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

        }
        void VAHED_K_AfterUpdate()
        {
            if (CURRENT_ITEMS_ROW?.VAHED_K is null) { return; }
            if (CURRENT_ITEMS_ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ITEMS_ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ITEMS_ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                CURRENT_ITEMS_ROW.MEGHk = CURRENT_ITEMS_ROW.MEGH * RST.FirstOrDefault().NESBAT;
            }

            MEGH_AfterUpdate();
        }
        public void AVRAGE_UPDATE()
        {
            return; //Obsolete
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
            if (CURRENT_ITEMS_ROW?.MEGHk is null ||
                CURRENT_ITEMS_ROW?.ANBAR is null ||
                CURRENT_ITEMS_ROW?.CODE is null)
            {
                return;
            }

            double min;
            double MAND = 0;
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
                CURRENT_ITEMS_ROW.MEGH_R = CURRENT_ITEMS_ROW.MEGHk;
                if (CURRENT_ITEMS_ROW.MABL != 0)
                {
                    CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
                }
            }

            UPDATE_LAST_AVRAGE();

            if ((Convert.ToBoolean(Baseknow.RMOG) || Baseknow.MOJU) && CURRENT_ITEMS_ROW.ANBAR != 0)
            {
                if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.MEGHk != null)
                {
                    var _where = "WHERE CODE = '" + CURRENT_ITEMS_ROW.CODE + "' AND ANBAR = " + CURRENT_ITEMS_ROW.ANBAR;
                    var RSTM3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM dbo.STUF_STK {_where}").ToList();
                    if (RSTM3.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else
                    {
                        min = CL_HESABDARI.Getmin((int)CURRENT_ITEMS_ROW.ANBAR, CURRENT_ITEMS_ROW.CODE);

                        var RSTM0 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ITEMS_ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ITEMS_ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ITEMS_ROW.ANBAR + ")").ToList();
                        if (RSTM0.Count > 0)
                        {
                            MAND = Convert.ToDouble(RSTM0.FirstOrDefault());
                            var RequestMeghkDiff = Convert.ToDouble(Convert.ToDouble(WAS_ROW_ITEM?.MEGHk - CURRENT_ITEMS_ROW.MEGH_MAR) - CURRENT_ITEMS_ROW.MEGHk);

                            double LeftMand = Math.Round(MAND - RequestMeghkDiff, Convert.ToInt32(Baseknow.DIG)); // 0 - -1
                            var AtLeastMand = Math.Round(min, Convert.ToInt32(Baseknow.DIG));

                            if (LeftMand < AtLeastMand)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                                msgwin.ShowDialog();
                                CURRENT_ITEMS_ROW.MEGH = WAS_ROW_ITEM.MEGH;
                                CURRENT_ITEMS_ROW.MEGHk = WAS_ROW_ITEM.MEGHk;
                            }
                            ////Update:
                            ////RSTM2.FirstOrDefault().MOGODI = MAND - (CURRENT_ITEMS_ROW.MEGHk - (Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/) - CURRENT_ITEMS_ROW.MEGH_MAR));
                            ////RSTM2.FirstOrDefault().MOGODI_A = 0;
                        }
                    }
                }
            }

            //if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null && CURRENT_ITEMS_ROW?.MEGHk != null && CURRENT_ITEMS_ROW?.N_KOL != null)
            //{
            //    var rst = dbms.DoGetDataSQL<VKSQRE1>("SELECT     dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, SUM(dbo.DTL_MANF.MABLK) AS MABLKs FROM         dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.FNUMB = " + CURRENT_ITEMS_ROW.N_KOL + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR").FirstOrDefault();
            //    if (rst != null)
            //    {
            //        CURRENT_ITEMS_ROW.AVRAGE = CURRENT_ITEMS_ROW.MABL;
            //    }
            //    else
            //    {
            //        CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
            //    }
            //}

            CURRENT_ITEMS_ROW.MABL_K = CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk;

        }

        private void UPDATE_LAST_AVRAGE()
        {
            if (CURRENT_ITEMS_ROW?.ANBAR != null && CURRENT_ITEMS_ROW?.CODE != null)
            {
                CURRENT_ITEMS_ROW.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ITEMS_ROW.CODE, (long)CURRENT_ITEMS_ROW.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));

                CURRENT_ITEMS_ROW.MABL = CURRENT_ITEMS_ROW.AVRAGE;
                CURRENT_ITEMS_ROW.MABL_K = Math.Round((double)(CURRENT_ITEMS_ROW.MABL * CURRENT_ITEMS_ROW.MEGHk));
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

            var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(CUST_NO, dbms);
            if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
            {
                universControl.PopNotifyShow($"مسئول شیفت نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
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
                if (CL_HESABDARI.BLOCKEDCUST(CUST_NO2.SelectedValue.ToString()))
                {
                    CUST_NO.SelectedItem = null;
                    universControl.PopNotifyShow(" حساب مسدود گرديده است لطفا با مديريت مالي تماس بگيريد", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
        }
        private void SANAD()
        {
            var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.SANADKHORUGMAVAD(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

            if (SanadNumber != null)
            {
                N_S.Text = SanadNumber.ToString();
            }

            DoCmdHeaderSave();

            GetBalanceInfo();
        }

        private string BEFOREDATEN;
        private List<COMBOPERSONEL> rst_personel;

        private void DATE_N_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NewRecord)
            {
                var _Out_ = CL_LMethods.DATE_IS_VALID(DATE_N.Text);
                if (!_Out_.Item1)
                {
                    DATE_N.Text = BEFOREDATEN;
                    universControl.PopNotifyShow(_Out_.Item2, Pop1, Pop1Text1, Pop_Border1);
                }
                else
                {
                    BEFOREDATEN = DATE_N.Text.ToRawTarikh();
                }
            }
        }
        private void DATE_N_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BEFOREDATEN = DATE_N.Text.ToRawTarikh();
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
            NUMBER.Text = "0";
            NUMBER.Tag = null;

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();
            MOLAH.Text = null;

            FNUMCO.Text = "0"; //شماره داخلی

            OKF.IsChecked = false;

            N_S.Text = ""; //ثبت در سند
            MABNA.Text = ""; //ثبت در سند

            INVO_LST_FACTOR22_DATA?.Clear(); //دیتاگرید فاکتور فروش

            PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
            PERSONEL.Text = null;
            PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
            PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            OKF.IsChecked = false;
            SGN1usid.Text = null; SGN1usid.Tag = null; SGN1.IsChecked = false;
            SGN2usid.Text = null; SGN2usid.Tag = null; SGN2.IsChecked = false;
            SGN3usid.Text = null; SGN3usid.Tag = null; SGN3.IsChecked = false;

            _sgn1_info.SEMAT_USER = null;
            _sgn1_info.NAME_HESAB_USER = null;
            _sgn2_info.SEMAT_USER = null;
            _sgn2_info.NAME_HESAB_USER = null;
            _sgn3_info.SEMAT_USER = null;
            _sgn3_info.NAME_HESAB_USER = null;

            Form_Current();

            AllowEdits = true;

            INVO_LST_SUB.IsReadOnly = true; // Locked

            MakeDefaultFocuseReady();
        }


        private void Form_Current()
        {
            this.lsanad.Foreground = new SolidColorBrush(Colors.White);

            if (INVO_LST_FACTOR22_DATA.Count > 0)
                this.Command106.IsEnabled = true;
            else
                this.Command106.IsEnabled = false;

            if (Convert.ToBoolean(Baseknow.SIGN))
            {
                if (Convert.ToBoolean(SGN2.IsChecked ?? false))
                    this.Command106.IsEnabled = true;
                else
                    this.Command106.IsEnabled = false;
            }


            if (NUMBER.Text == "0")
            {
                INVO_LST_SUB.IsReadOnly = true;
                MABNA.Text = "0";
            }
            else
            {
                INVO_LST_SUB.IsReadOnly = false;

                var rst = dbms.DoGetDataSQL<DEED_HED>("SELECT BASE, GHATEI FROM DEED_HED WHERE N_S = @N_S", new { N_S = N_S.Text }).FirstOrDefault();

                if (rst != null)
                {
                    var record = rst;
                    MABNA.Text = record.@base.ToStringNullSafe();

                    if (record.GHATEI)
                    {
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        this.INVO_LST_SUB.IsReadOnly = true;
                        this.lsanad.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        this.INVO_LST_SUB.IsReadOnly = false;
                        this.lsanad.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }

            if (Convert.ToBoolean(SGN2.IsChecked ?? false) || Convert.ToBoolean(SGN3.IsChecked ?? false))
            {
                this.INVO_LST_SUB.IsReadOnly = true;
            }

            if (NUMBER.Text != "0")
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                INVO_LST_SUB.IsReadOnly = true;
                //this.ESLAH.IsEnabled = true;
            }

            SGN_MANAGER();

            if (Convert.ToBoolean(OKF.IsChecked ?? false) && !NewRecord)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;
                this.INVO_LST_SUB.IsReadOnly = true;
                //this.ESLAH.IsEnabled = true;
            }
            else
            {
                this.AllowDeletions = true;
                this.AllowEdits = true;
                this.INVO_LST_SUB.IsReadOnly = false;
                //this.ESLAH.IsEnabled = false;
            }

        }

        private void SGN_MANAGER()
        {
            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 39, Convert.ToInt32(Baseknow.USERCOD), new WindowInteropHelper(this).Handle);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }
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

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, FTAG);

            if (NewRecord)
            {
                this.Close();
            }
        }

        private void Command106_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord || INVO_LST_FACTOR22_DATA.Count == 0)
            {
                return;
            }

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.SANATI.HAVLAH_EXIT.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
            ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 300;

            report["NUMBER_PARAM"] = NUMBER.Text;
            (report.GetComponentByName("CUST_NO_NAME") as StiText).Text = (CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME;
            (report.GetComponentByName("COMPANY_NAME") as StiText).Text = Baseknow.WIDTH_D; //نام شرکت


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

            new WINRPT(report, LABEL_HEADER.Content.ToString()).Show();

            if ((bool)Baseknow.LOCKFAP)
            {
                OKF.IsChecked = true;
            }

            if (OKF.IsChecked == true)
            {
                this.AllowDeletions = false;
                this.AllowEdits = false;

                this.INVO_LST_SUB.IsReadOnly = true;

                this.ESLAH.IsEnabled = true;
            }

            DoCmdHeaderSave();

        }

        private void PERSONEL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PERSONEL.SelectedItem != null && !NewRecord && NUMBER.Text != "0")
            {
                Meidnum = CL_HESABDARI.PERSONELUpdate(38, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($".ارجاع داده شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            else
            {
                e.Handled = true;

                PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                PERSONEL.Text = null;
                PERSONEL.SelectedValue = null; PERSONEL.Items.Refresh();
                PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                universControl.PopNotifyShow($".هنوز ذخیره را انجام نداده اید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            var SGN_WAS = Convert.ToBoolean(SGN1.IsChecked ?? false);

            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                SGN1.IsChecked = !SGN_WAS;
                return;
            }

            if (CL_HESABDARI.MOGUDI(Convert.ToInt64(NUMBER.Text), FTAG) || SGN_WAS)
            {
                double mid;
                string SHARH;
                double td;
                int _KIND_ = 39;
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                SHARH = "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'";

                if (mid > 0d)
                {
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + NUMBER.Text + $",{_KIND_} )");
                    dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
                }
                else
                {
                    td = Tarikh.GET_OADATE_DAO();
                    dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_}, GETDATE() ," + Baseknow.USERCOD + " )");
                    mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN1.IsChecked), " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_} )");
                }

                Meidnum = CL_HESABDARI.PERSONELUpdate(_KIND_, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue ?? Baseknow.USERCOD), "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");

                SGN1usid.Tag = Baseknow.USERCOD;
                SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

                if ((bool)SGN1.IsChecked)
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN1usid={SGN1usid.Tag ?? "NULL"}, SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }
                else
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN1usid={SGN1usid.Tag ?? "NULL"}, SGN1 = {Convert.ToByte((bool)SGN1.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }

                this.PERSONEL.Visibility = Visibility.Visible;

                Form_Current();
            }
            else
            {
                SGN1.IsChecked = !SGN_WAS;
            }

        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            var SGN_WAS = Convert.ToBoolean(SGN2.IsChecked ?? false);

            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                SGN2.IsChecked = !SGN_WAS;
                return;
            }

            if (CL_HESABDARI.MOGUDI(Convert.ToInt64(NUMBER.Text), FTAG) || SGN_WAS)
            {
                double mid;
                string SHARH;
                double td;
                int _KIND_ = 39;
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                SHARH = "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'";

                if (mid > 0d)
                {
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN2.IsChecked), " :امضا شد2 ", " :امضا برداشته شد2:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + NUMBER.Text + $",{_KIND_} )");
                    dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
                }
                else
                {
                    td = Tarikh.GET_OADATE_DAO();
                    dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_}, GETDATE() ," + Baseknow.USERCOD + " )");
                    mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN2.IsChecked), " : امضا شد2 ", " :امضا برداشته شد2 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_} )");
                }

                Meidnum = CL_HESABDARI.PERSONELUpdate(_KIND_, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue ?? Baseknow.USERCOD), "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");

                SGN2usid.Tag = Baseknow.USERCOD;
                SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

                if ((bool)SGN2.IsChecked)
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN2usid={SGN2usid.Tag ?? "NULL"}, SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }
                else
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN2usid={SGN2usid.Tag ?? "NULL"}, SGN2 = {Convert.ToByte((bool)SGN2.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }

                this.PERSONEL.Visibility = Visibility.Visible;

                Form_Current();
            }
            else
            {
                SGN2.IsChecked = !SGN_WAS;
            }
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            var SGN_WAS = Convert.ToBoolean(SGN3.IsChecked ?? false);


            if (CL_HESABDARI.MOGUDI(Convert.ToInt64(NUMBER.Text), FTAG) || SGN_WAS)
            {
                double mid;
                string SHARH;
                double td;
                int _KIND_ = 39;
                mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                SHARH = "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'";

                if (mid > 0d)
                {
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN3.IsChecked), " :امضا شد3 ", " :امضا برداشته شد3:") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + NUMBER.Text + $",{_KIND_} )");
                    dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(mid) + ",STATUS = 1 WHERE IDNUM = " + mid);
                }
                else
                {
                    td = Tarikh.GET_OADATE_DAO();
                    dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_}, GETDATE() ," + Baseknow.USERCOD + " )");
                    mid = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), _KIND_);
                    dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + mid + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf(Convert.ToBoolean(SGN3.IsChecked), " : امضا شد2 ", " :امضا برداشته شد2 ") + "'," + Tarikh.FullCurrentDate + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + $",{_KIND_}," + this.NUMBER.Text + $",{_KIND_} )");
                }

                Meidnum = CL_HESABDARI.PERSONELUpdate(_KIND_, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue ?? Baseknow.USERCOD), "'حواله خروج   شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToStringNullSafe()) + "','" + CUST_NO.SelectedValue + "'");

                SGN3usid.Tag = Baseknow.USERCOD;
                SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

                if ((bool)SGN3.IsChecked)
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN3usid={SGN3usid.Tag ?? "NULL"}, SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }
                else
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SGN3usid={SGN3usid.Tag ?? "NULL"}, SGN3 = {Convert.ToByte((bool)SGN3.IsChecked)} WHERE TAG = {FTAG} AND NUMBER = {NUMBER.Text}");
                }

                this.PERSONEL.Visibility = Visibility.Visible;

                Form_Current();
            }
            else
            {
                SGN3.IsChecked = !SGN_WAS;
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

        private void N_S_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Right N_S
            if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
            {
                CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), false);
            }
        }
    }
}
