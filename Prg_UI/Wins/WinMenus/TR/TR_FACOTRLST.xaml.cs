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
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Report.Helpers;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Prg_UI.Wins.WinMenus.SANATI.HAVALE_EXIT_SAYER;
using static Wins.WinMenus.SANATI.HAVALAH_ENTER;


namespace Prg_UI.Wins.WinMenus.TR
{
    /// <summary>
    /// Interaction logic for TR_FACOTRLST.xaml
    /// </summary>
    public partial class TR_FACOTRLST : Window
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
        public TR_FACOTRLST(byte? _TAGCODE_)
        {
            InitializeComponent();

            this.DataContext = this;

            if (_TAGCODE_ != null)
            {
                TAGCODE = (byte)_TAGCODE_;
            }

            //SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");


        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        #region LOCALMODEL
        public class N_RASID_MODEL
        {
            public string? FNUMB { get; set; }
            public string? nam { get; set; }
            public int? Expr1 { get; set; }
        }
        public class SN_MODEL_TR
        {
            public double? MHAZ_NO { get; set; }
            public string? MHAZNAME { get; set; }
        }
        #endregion
        UniversControl universControl = new UniversControl();
        public ObservableCollection<TR_HEAD_LST> FACTOR_DATA { get; set; } = new ObservableCollection<TR_HEAD_LST>();
        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region COLUMN_DISPLAYER
            switch (TAGCODE)
            {
                //فاکتوری ها
                case 3: /*برگشت خرید عادی*/
                case 4: /*برگشت فروش عادی*/
                case 12: /*فاکتور خرید*/
                case 13: /*فاکتور فروش*/
                case 14:
                case 20:
                case 25: /*برگشت فروش آزاد*/
                case 27: /*برگشت خرید آزاد*/
                    NUMBER_FAC_COLUMN.IsHidden = false; //Show
                    isFactory = true;
                    break;

                //انباری ها
                case 1:
                case 2:
                case 5:
                case 23:
                case 24:
                case 26:
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ";
                    NUMBER_FAC_COLUMN.IsHidden = true; //Hide
                    MODAT_COLUMN.IsHidden = true;
                    SANAD_COLUMN.IsHidden = true;
                    NAGHD_COLUMN.IsHidden = true;
                    VARIZI_COLUMN.IsHidden = true;
                    MOEENVARIZ_COLUMN.IsHidden = true;
                    MABL_HAV_COLUMN.IsHidden = true;
                    MOEEN_HAV_COLUMN.IsHidden = true;
                    MABL_KHAD_COLUMN.IsHidden = true;
                    MOEEN_KHAD_COLUMN.IsHidden = true;
                    MABL_TAKHFIF_COLUMN.IsHidden = true;
                    break;

                default: break;
            }
            #endregion

            PAGE_POSHT.Visibility = Visibility.Collapsed; //پشت فاکتور
            PAGE_SAYER.Visibility = Visibility.Collapsed; //سایر

            string Formname = "";

            switch (TAGCODE) //عنوان پنجره
            {
                case 27:
                    WINTILENAME.Content = "فاکتور های برگشت خرید آزاد";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    MEGH_MAR_COLUMN.IsHidden = false; //Show the Column
                    Formname = "TR_KHB";
                    break;

                case 26: WINTILENAME.Content = "سایر حواله انبار ها";
                    Formname = "TR_FRH";
                    break;

                case 25:
                    WINTILENAME.Content = "فاکتور های برگشت فروش آزاد رسید شده";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره برگه";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    PAGE_SAYER.Visibility = Visibility.Visible; //سایر
                    MEGH_MAR_COLUMN.IsHidden = false; //Show the Column
                    Formname = "TR_FRB";
                    break;

                case 24:
                    WINTILENAME.Content = "سایر رسید انبار ها";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره رسید";
                    MEGH_MAR_COLUMN.IsHidden = false; //Show the Column
                    Formname = "TR_KHH";
                    break;

                case 23:
                    WINTILENAME.Content = "درخواست خرید ها";
                    TAH_COLUMN.IsHidden = false;
                    Formname = "";
                    break;

                case 20:
                    WINTILENAME.Content = "پیش فاکتور ها";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    SANAD_COLUMN.IsHidden = true;
                    NUMBER_HAV_COLUMN.HeaderText = "شماره پیش فاکتور";
                    Formname = "TR_PFRB";
                    break;

                case 14: WINTILENAME.Content = "فاکتور های خدمات"; break;
                case 13:
                    WINTILENAME.Content = "فاکتور های فروش";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    PAGE_SAYER.Visibility = Visibility.Visible; //سایر
                    Formname = "TR_FR";
                    break;
                case 12:
                    WINTILENAME.Content = "فاکتور های خرید";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره رسید انبار ها";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    Formname = "TR_KH";
                    break;

                case 11:
                    WINTILENAME.Content = "برگه خروج سایر مواد از انبار";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره";
                    CUST_HESAB_COLUMN.HeaderText = "حساب مسئول شیفت";
                    CUST_NAME_COLUMN.HeaderText = "نام مسئول شیفت";
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ";
                    MODAT_COLUMN.IsHidden = true;

                    N_RASID_COLUMN_COMBO.IsHidden = false; //Show محل مصرف
                    N_RASID_COLUMN_COMBO.ItemsSource = dbms.DoGetDataSQL<N_RASID_MODEL>(@"SELECT dbo.HEAD_MANF.FNUMB, STUF_DEF.NAME+' '+ISNULL(HEAD_MANF.TOZIH, ' ') AS nam, dbo.HEAD_MANF.FNUMB AS Expr1
                                                          FROM dbo.STUF_DEF
                                                               INNER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE=dbo.HEAD_MANF.CODE
                                                          WHERE(NOT(dbo.STUF_DEF.NAME IS NULL))").ToList();

                    SANAD_NO_COLUMN_COMBO.IsHidden = false; //Show مرکز هزینه
                    SANAD_NO_COLUMN_COMBO.ItemsSource = dbms.DoGetDataSQL<SN_MODEL_TR>(@"SELECT MHAZ_NO, MHAZNAME FROM TCOD_MARKAZHAZ").ToList();
                    Formname = "TR_SMAVA";
                    break;

                case 10:
                    WINTILENAME.Content = "برگه های خروج مواد اولیه";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره حواله انبار";
                    CUST_HESAB_COLUMN.HeaderText = "حساب مسئول شیفت";
                    CUST_NAME_COLUMN.HeaderText = "نام مسئول شیفت";
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ حواله";
                    MODAT_COLUMN.IsHidden = true;
                    N_RASID_COLUMN_COMBO.IsHidden = false; //Show محل مصرف
                    N_RASID_COLUMN_COMBO.ItemsSource = dbms.DoGetDataSQL<N_RASID_MODEL>(@"SELECT dbo.HEAD_MANF.FNUMB, STUF_DEF.NAME+' '+ISNULL(HEAD_MANF.TOZIH, ' ') AS nam, dbo.HEAD_MANF.FNUMB AS Expr1
                                                          FROM dbo.STUF_DEF
                                                               INNER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE=dbo.HEAD_MANF.CODE
                                                          WHERE(NOT(dbo.STUF_DEF.NAME IS NULL))").ToList();
                    Formname = "TR_MAVA";
                    break;

                case 9:
                    WINTILENAME.Content = "برگه های ورود کالای ساخته شده";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره برگه";
                    CUST_HESAB_COLUMN.HeaderText = "حساب مسئول شیفت";
                    CUST_NAME_COLUMN.HeaderText = "نام مسئول شیفت";
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ";
                    N_KOL_COLUMN_COMBO.IsHidden = false; //Show //فرمول ساخت
                    N_KOL_COLUMN.IsHidden = true; //Hide
                    MODAT_COLUMN.IsHidden = true;
                    N_KOL_COLUMN_COMBO.ItemsSource = dbms.DoGetDataSQL<FSAKHT_COMBO>("SELECT HEAD_MANF.FNUMB, STUF_DEF.NAME + N' - ' + CAST(HEAD_MANF.DATE_ACTIV AS nvarchar) + N' :-' + ISNULL(HEAD_MANF.TOZIH, N' ') + CAST(HEAD_MANF.FNUMB AS char) AS Expr1 FROM HEAD_MANF INNER JOIN STUF_DEF ON HEAD_MANF.CODE = STUF_DEF.CODE").ToList();
                    Formname = "TR_TOL";
                    break;

                case 5:
                    WINTILENAME.Content = "انتقال از انبار به انبار";
                    break;

                case 4:
                    WINTILENAME.Content = "فاکتور های برگشت فروش - عادی";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    PAGE_SAYER.Visibility = Visibility.Visible; //سایر
                    MEGH_MAR_COLUMN.IsHidden = false; //Show the Column
                    Formname = "TR_FRB";
                    break;
                case 3:
                    WINTILENAME.Content = "فاکتور های برگشت خرید - عادی";
                    PAGE_POSHT.Visibility = Visibility.Visible; //پشت فاکتور
                    MEGH_MAR_COLUMN.IsHidden = false; //Show the Column
                    Formname = "TR_KHB";
                    break;

                case 2:
                    WINTILENAME.Content = "حواله های فروش";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    Formname = "TR_FRH";
                    break;

                case 1:
                    WINTILENAME.Content = "رسید های خرید";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره رسید";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    Formname = "TR_KHH";
                    break;

                default: WINTILENAME.Content = "همه نوع فاکتور"; break;
            }

            #region SecuritCheck
            if (!string.IsNullOrWhiteSpace(Formname))
            {
                try
                {
                    var helper = new WindowInteropHelper(this);
                    helper.EnsureHandle(); // Critical: Ensures handle exists before access
                                           // 2. Run Security:
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                    // 3. Final State Check:
                    if (!this.IsLoaded) { this.Close(); return; }
                }
                catch { try { this.Close(); } catch { } }
            }
            #endregion

            if (TAGCODE == 13) // فاکتور فروش
            {
                fTAG = 13; //سربرگ فاکتور
                hTAG = 2; //اطلاعا کالا و حواله اون
            }
            else if (TAGCODE == 12) // فاکتور خرید
            {
                fTAG = 12;
                hTAG = 1;
            }
            else if (TAGCODE == 4) // فاکتور برگشت فروش عادی
            {
                fTAG = 4;
                hTAG = 2;
            }
            else if (TAGCODE == 3) // فاکتور برگشت خرید عادی
            {
                fTAG = 3;
                hTAG = 1;
            }
            else if (TAGCODE == 25) // فاکتور برگشت فروش آزاد
            {
                fTAG = 25;
                hTAG = 24;
            }
            else if (TAGCODE == 27) // فاکتور برگشت خرید آزاد
            {
                fTAG = 27;
                hTAG = 26;
            }
            else
            {
                fTAG = TAGCODE;
                hTAG = TAGCODE;
            }

            FILL_ALL_COMBOBOXES();

            #region Configy
            if (!CL_HESABDARI.LETSGO("TKHPISH"))
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
            }
            else
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Visible;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 58, 1) != "5")
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
            }

            if (TAKHFIF_APLAY_SUB.Visibility == Visibility.Visible)
            {
                SF_SUB.Margin = new System.Windows.Thickness(0, 0, 325, 0);

                var ROW_SOURCE = dbms.DoGetDataSQL<TAKHFIF_DEF>("SELECT TID, TSHARH FROM TAKHFIF_DEF").ToList();
                Combo6Column.ItemsSource = ROW_SOURCE;
                var DATA = dbms.DoGetDataSQL<TAKHFIF_APLAY>("SELECT TID, NUMBER, KIND FROM TR_TAKHFIF_APLAY").ToList();
            }
            #endregion

            ReGetHeadMaster();

            GenerateAutomaticSummary(SYNCFUSION_DG);

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            WINTILENAME.Content = " سابقه " + WINTILENAME.Content.ToString();

            SYNCFUSION_DG.Visibility = Visibility.Visible; //Make Visible after data loading to reduce the slowness

            //AttachRecordCountUpdater(SYNCFUSION_DG, ROWCOUNT_TEXTBLK1);
            SetupGridNavigation();
            AttachRecordCountUpdater(SF_SUB, TXT_COUNT_FACTOR);
            AttachRecordCountUpdater(PAY_GETD_SUB22, TXT_COUNT_POSHT);
            AttachRecordCountUpdater(VISITOR_DTL_SUB, TXT_COUNT_SAYER);
        }

        private void ReGetHeadMaster()
        {
            FACTOR_DATA?.Clear();

            string WhereCondition = TAGCODE > 0 ? $" WHERE (dbo.TR_HEAD_LST.TAG = {TAGCODE}) " : "  ";

            //WhereCondition = CL_LMethods.GetRestrictedSqlQuery(TAGCODE, WhereCondition);

            var MasterHead = dbms.DoGetDataSQL<TR_HEAD_LST>(@$"
                    SELECT        dbo.TR_HEAD_LST.NUMBER1, dbo.TR_HEAD_LST.TAH, dbo.TR_HEAD_LST.NUMBER, dbo.TR_HEAD_LST.DATE_N, dbo.TR_HEAD_LST.MAS, dbo.TR_HEAD_LST.N_S, dbo.TR_HEAD_LST.CUST_NO, dbo.CUST_HESAB.NAME, 
                                             dbo.TR_HEAD_LST.MOLAH, dbo.TR_HEAD_LST.M_NAGHD, dbo.TR_HEAD_LST.MABL_VAR, dbo.TR_HEAD_LST.MOIN_VAR, dbo.TR_HEAD_LST.MABL_HAV, dbo.TR_HEAD_LST.MOIN_HAV, dbo.TR_HEAD_LST.MABL_HAZ, 
                                             dbo.TR_HEAD_LST.MOIN_HAZ, dbo.TR_HEAD_LST.TAKHFIF, dbo.TR_HEAD_LST.MOIN_KHF, dbo.TR_HEAD_LST.TAG, dbo.DEPART.DEPNAME, dbo.SHIFT.SHNAME, dbo.CUSTKIND.CUSTKNAME, 
                                             dbo.TR_HEAD_LST.USER_NAME, dbo.TR_HEAD_LST.SHARAYET, dbo.TR_HEAD_LST.MBAA, dbo.TR_HEAD_LST.HMBAA, dbo.TR_HEAD_LST.TICMBAA, dbo.TR_HEAD_LST.TKHF, dbo.TR_HEAD_LST.OKF, 
                                             dbo.TR_HEAD_LST.JAY, dbo.TR_HEAD_LST.SGN1, dbo.TR_HEAD_LST.SGN2, dbo.TR_HEAD_LST.SGN3, dbo.TR_HEAD_LST.sgn1usid, dbo.TR_HEAD_LST.sgn2usid, dbo.TR_HEAD_LST.sgn3usid, dbo.TR_HEAD_LST.CRT, 
                                             dbo.TR_HEAD_LST.UID, dbo.PRICE_ELAMIE.PEPNAME, dbo.PRICE_ELAMIETF.PENAME, dbo.PRICE_PAYNO.PPAME, dbo.TR_HEAD_LST.ANBAR, dbo.TR_HEAD_LST.FNUMCO, dbo.TR_HEAD_LST.IPADD, 
                                             dbo.TR_HEAD_LST.PC_NAME, dbo.TR_HEAD_LST.UP_USER_NAME, dbo.TR_HEAD_LST.UP_TIME, dbo.TR_HEAD_LST.UP_DATE
                    FROM            dbo.TR_HEAD_LST LEFT OUTER JOIN
                                             dbo.PRICE_PAYNO ON dbo.TR_HEAD_LST.MODAT_PPID = dbo.PRICE_PAYNO.PPID LEFT OUTER JOIN
                                             dbo.PRICE_ELAMIETF ON dbo.TR_HEAD_LST.PEID = dbo.PRICE_ELAMIETF.PEID LEFT OUTER JOIN
                                             dbo.CUSTKIND ON dbo.TR_HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD LEFT OUTER JOIN
                                             dbo.PRICE_ELAMIE ON dbo.TR_HEAD_LST.PEPID = dbo.PRICE_ELAMIE.PEPID LEFT OUTER JOIN
                                             dbo.DEPART ON dbo.TR_HEAD_LST.DEPATMAN = dbo.DEPART.DEPATMAN LEFT OUTER JOIN
                                             dbo.SHIFT ON dbo.TR_HEAD_LST.SHIFT = dbo.SHIFT.SHIFT_ID LEFT OUTER JOIN
                                             dbo.CUST_HESAB ON dbo.TR_HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                                             {WhereCondition} ").ToList();

            foreach (var item in MasterHead)
            {
                ////item.UP_TIME = DateTime.FromOADate(Convert.ToDouble(item.UP_TIME)).ToString("G", new CultureInfo("fa-IR"));
                ////item.UP_TIME = DateTime.FromOADate(Convert.ToDouble(item.UP_TIME)).ToString("HH:mm:ss", new CultureInfo("fa-IR"));
                FACTOR_DATA?.Add(item);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as TR_HEAD_LST;

                switch (currentRow?.TAG)
                {
                    case 1: // رسید خرید
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_RASID), (double)currentRow.NUMBER, "یک پنجره رسید خرید از قبل باز شده ابتدا آنرا ببندید.");

                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 2: // حواله فروش
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_HAVL), (double)currentRow.NUMBER, "یک پنجره حواله فروش از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 3: //فاکتور برگشت خرید عادی	
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KH_BACK), (double)currentRow.NUMBER, "یک پنجره فاکتور برگشت خرید عادی از قبل باز شده ابتدا آنرا ببندید.");
                            //SELECT* FROM dbo.HEAD_LST WHERE NUMBER = 2 AND TAG = 3--فاکتور برگشت خرید عادی Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 2073 AND TAG = 1
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 4:
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_FROOSH_BACK2), (double)currentRow.NUMBER, "یک پنجره فاکتور برگشت فروش عادی از قبل باز شده ابتدا آنرا ببندید.");
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 254  AND TAG = 4 --فاکتور برگشت فروش استاندارد عادی Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 5361 AND TAG = 2
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 5: //انتقال از انبار به انبار
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 9: //برگه ورود
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_ENTER, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 10: //برگه خروج مواد اولیه
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_EXIT, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 11: //برگه خروج مواد اولیه
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALE_EXIT_SAYER, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 12: // فاکتور خرید
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KHAREED1), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور خرید از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, this, currentRow.NUMBER);
                        }
                        break;

                    case 13: // فاکتور فروش
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_FROOSH22), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور فروش از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this, currentRow.NUMBER1 + "," + currentRow.NUMBER);
                        }
                        break;

                    case 14: // فاکتور خدمات
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KHADAMAT), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور خدمات از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHADAMAT, this, currentRow.NUMBER);
                        }
                        break;

                    case 20: //پیش فاکتور ها
                        if (currentRow?.NUMBER != null)
                        {
                            try
                            {
                                Application.Current.Windows.OfType<HEAD_LST_PISHFROOSH2>().FirstOrDefault()?.Close();
                            }
                            catch { }

                            //OpenWindow(typeof(HEAD_LST_PISHFROOSH2), (double)currentRow.NUMBER, "یک پنجره پیش فاکتور از قبل باز شده ابتدا آنرا ببندید.");

                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 23: //درخواست خرید
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_REQUEST_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 24: //سایر رسید انبار ها
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID_OTHER_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 25: //فاکتور برگشت فروش آزاد رسید شده
                        if (currentRow?.NUMBER != null)
                        {
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 954 AND TAG = 25 --فاکتور برگشت فروش رسید شده : آزاد Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 954 AND TAG = 24
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_BRFR, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 26: //سایر حواله انبار ها
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 27: //فاکتور برگشت خرید آزاد
                        if (currentRow?.NUMBER != null)
                        {
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 3 AND TAG = 27   --فاکتور برگشت خرید آزاد Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 3 AND TAG = 26
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD, this, (double)currentRow.NUMBER);
                        }
                        break;

                }
            }
        }

        public int fTAG { get; set; }
        public int hTAG { get; set; }

        private void FILL_ALL_COMBOBOXES()
        {
            //انبار کالا
            var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>($"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM dbo.TCOD_ANBAR").ToList();
            ANBAR_COLUMN.ItemsSource = ARST;

            //پر کردن کمبوباکس ستون واحد به طور مقدار اولیه
            VAHED_K_COLUMN.ItemsSource = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();

            ////پشت فاکتور بخش چک:
            vAZColumn.ItemsSource = new List<VAZ_MODEL_CHECK>()
            {
                 new VAZ_MODEL_CHECK { VAZ = 1, NAME_VAZ = "نزد صندوق" },
                 new VAZ_MODEL_CHECK { VAZ = 2, NAME_VAZ = "نزد بانك" },
                 new VAZ_MODEL_CHECK { VAZ = 3, NAME_VAZ = "وصول شده" },
                 new VAZ_MODEL_CHECK { VAZ = 4, NAME_VAZ = "واگذار شده" },
                 new VAZ_MODEL_CHECK { VAZ = 5, NAME_VAZ = "برگشت شده" },
                 new VAZ_MODEL_CHECK { VAZ = 6, NAME_VAZ = "مسترد شده" }
            };

            ////کمبوباکس های پشت فاکتور
            bANKColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();

            var HESNAMELST = new List<CUSTOM_HESABHA>();
            CMB_MOIN_VAR.ItemsSource = HESNAMELST.Where(w => w.N_KOL == Baseknow.BANKHA).ToList(); //معین واریزی
            CMB_MOIN_HAV.ItemsSource = HESNAMELST.ToList(); //معين حواله
            CMB_MOIN_HAZ.ItemsSource = HESNAMELST.ToList(); //معين خدمات
            CMB_HMBAA.ItemsSource = HESNAMELST.ToList(); //معین مالیات

            ////دریافت چک:
            ////به حساب کل
            n_KOLColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>("SELECT     NUMBER, NAME FROM TOTA_HES WHERE (NUMBER = " + Baseknow.BANKHA + ")ORDER BY NAME").ToList();
            //Giving All Data as Master:
            //معین بانک
            n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {Baseknow.BANKHA})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //تفضیلی
            n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.N_KOL) ={Baseknow.BANKHA}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME\r\n").ToList();

            ////موقعیت چک
            sANDUGHColumn.ItemsSource = dbms.DoGetDataSQL<TDETA_HES_CHECK>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = 1)").ToList();

            //الگوی پورسانت:
            PORID_COLUMN.ItemsSource = dbms.DoGetDataSQL<PORD_COL_MODEL>("SELECT VISITORS_PORSANT.PORID, CAST(VISITORS_PORSANT.PORID AS nvarchar) + N' - ' + CAST(VISITORS_PORSANT.VDATE AS nvarchar) + N' - ' + ISNULL(CUSTKIND.CUSTKNAME, N'بدون گروه (همه)') + N' - ' + ISNULL(VISITORS_PORSANT.COMMENT, N' ') + N' - ' + CUST_HESAB.NAME AS Expr1 FROM VISITORS_PORSANT INNER JOIN CUST_HESAB ON VISITORS_PORSANT.HES = CUST_HESAB.hes LEFT OUTER JOIN CUSTKIND ON VISITORS_PORSANT.CUST_COD = CUSTKIND.CUST_COD").ToList();

            //اطلاعات بارنامه
            MAGHSAD.ItemsSource = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM dbo.TCOD_CITY").ToList();

            //if (IsExporty)
            //{
            //    ARZKIND2.ItemsSource = dbms.DoGetDataSQL<TCOD_ARZ>($"SELECT ID,Code, Title, ISOCode, (ISOCode+N' - '+Title+N' - '+CountryName) AS ARZCOUNTRY, CRT, UID FROM dbo.[TCOD_ARZ]").ToList();
            //}
        }

        public ObservableCollection<INVO_LST_FACTOR22> FACTOR22_INVO_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<TAKHFIF_APLAY> TAKHFIF_APLAY_DATA { get; set; } = new ObservableCollection<TAKHFIF_APLAY>();
        public ObservableCollection<PAY_GETD_SUB22_MODEL> PAY_GETD_SUB22_DATA { get; set; } = new ObservableCollection<PAY_GETD_SUB22_MODEL>();
        public ObservableCollection<VISITOR_DTL> SAYER_VISITOR_DATA { get; set; } = new ObservableCollection<VISITOR_DTL>();

        private void AttachRecordCountUpdater1(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            // متد داخلی برای به‌روزرسانی متن
            void UpdateLabel()
            {
                var count = dataGrid.View?.Records?.Count ?? 0;
                targetTextBlock.Text = count.ToString("N0"); // فرمت عددی
            }

            // رویداد لود شدن گرید
            dataGrid.Loaded += (s, e) => UpdateLabel();

            // رویداد تغییر فیلتر
            dataGrid.FilterChanged += (s, e) => UpdateLabel();

            // رویداد تغییر منبع داده (مثلاً وقتی سطر جدید اضافه یا حذف می‌شود)
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                UpdateLabel();
                // اگر منبع داده ObservableCollection باشد، باید به تغییرات آن هم گوش داد
                if (dataGrid.View != null)
                {
                    dataGrid.View.CollectionChanged += (sender, args) => UpdateLabel();
                    // همچنین گوش دادن به تغییرات رکوردها (مثل فیلتر شدن توسط View)
                    dataGrid.View.Records.CollectionChanged += (sender, args) => UpdateLabel();
                }
            };

            // فراخوانی اولیه در صورت لود بودن
            if (dataGrid.View != null)
            {
                dataGrid.View.Records.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            }
        }
        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            // متد داخلی برای به‌روزرسانی متن بر اساس منبع داده
            void UpdateLabel()
            {
                int count = 0;

                // اولویت با بررسی مستقیم منبع داده است (چون دقیق‌تر و سریع‌تر از View است)
                if (dataGrid.ItemsSource is ICollection collection)
                {
                    count = collection.Count;
                }
                else if (dataGrid.View != null && dataGrid.View.Records != null)
                {
                    // اگر منبع داده مستقیم نبود، سراغ ویو می‌رویم
                    count = dataGrid.View.Records.Count;
                }

                // چون ممکن است این فراخوانی از ترد دیگری باشد، از Dispatcher استفاده می‌کنیم
                Dispatcher.Invoke(() =>
                {
                    targetTextBlock.Text = count.ToString("N0");
                });
            }

            // متد برای اتصال به رویداد تغییرات کالکشن
            void SubscribeToCollection(object source)
            {
                if (source is INotifyCollectionChanged notifyingCollection)
                {
                    notifyingCollection.CollectionChanged += (s, e) => UpdateLabel();
                }
            }

            // 1. هر وقت کل منبع داده عوض شد (مثلا new ObservableCollection شد)
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                // به کالکشن جدید گوش بده
                if (e.NewItemsSource != null)
                {
                    SubscribeToCollection(e.NewItemsSource);
                }
                UpdateLabel();
            };

            // 2. اگر همین الان دیتایی دارد، به آن وصل شو و مقدار اولیه را ست کن
            if (dataGrid.ItemsSource != null)
            {
                SubscribeToCollection(dataGrid.ItemsSource);
                UpdateLabel();
            }
        }

        private double _sum_of_mabl_k = 0;
        public double SUM_OF_MABL_K
        {
            get
            {
                _sum_of_mabl_k = (double)FACTOR22_INVO_DATA.Sum(r => r.MABL_K);
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
                if (IsExporty)
                {
                    sum_of_megh_k = (double)FACTOR22_INVO_DATA.Sum(r => r.TOTALARZ); //جمع ارزی
                }
                else
                {
                    sum_of_megh_k = (double)FACTOR22_INVO_DATA.Sum(r => r.MEGHk);
                }
                if (sum_of_megh_k == 0) sum_of_megh_k = 0;
                return sum_of_megh_k;
            }
            set { sum_of_megh_k = value; }
        }

        public class FactorFullDetails
        {
            public HEAD_LST Header { get; set; }
            public List<INVO_LST_FACTOR22> InvoiceItems { get; set; } = new List<INVO_LST_FACTOR22>();
            public List<TAKHFIF_APLAY> AdvancedDiscounts { get; set; } = new List<TAKHFIF_APLAY>();
            public List<PAY_GETD_SUB22_MODEL> Checks { get; set; } = new List<PAY_GETD_SUB22_MODEL>();
            public List<VISITOR_DTL> VisitorDetails { get; set; } = new List<VISITOR_DTL>();
            public OTHER_DTL_CSHARP OtherDetails { get; set; }
            public Custom_CUST_HESAB Customer { get; set; } = new Custom_CUST_HESAB();
            public string AccountBalance { get; set; } // MANDAH
            public string SanadBase { get; set; } // MABNA
        }

        private CUSTOM_HESABHA? GetHesabhaByCode(string hesCode)
        {
            if (string.IsNullOrWhiteSpace(hesCode))
                return null;

            var result = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT TOP 1 hes,NAME FROM dbo.CUST_HESAB WHERE hes = @hes",
                new { hes = hesCode }
            ).FirstOrDefault();

            return result;
        }

        private async Task<FactorFullDetails> GetFactorFullDetailsAsync(TR_HEAD_LST TrRow)
        {
            var fullDetails = new FactorFullDetails();

            const string QUERY_TR_TAKHFIF_APLAY = $@"SELECT tad.*, td.TSHARH 
                FROM dbo.TR_TAKHFIF_APLAY tad
                JOIN dbo.TAKHFIF_DEF td ON tad.TID = td.TID
                WHERE tad.NUMBER = @FactorNumber AND tad.KIND = 2      AND UP_DATE = @UpDate 
                  AND UP_TIME = @UpTime; ";

            const string QUERY_TR_PAY_GETD = @"SELECT * FROM dbo.TR_PAY_GETD WHERE NUMBER = @FactorNumber AND TAG = @hTAG AND UP_DATE = @UpDate 
                  AND UP_TIME = @UpTime;";

            const string QUERY_SAYER = @"  -- 5. Visitor Details
                SELECT v.*, ch.NAME AS CUST_NO_NAME
                FROM dbo.TR_VISITOR_DTL v
                LEFT JOIN CUST_HESAB ch ON v.CUST_NO = ch.hes
                WHERE v.NUMBER = @FactorNumber AND v.TAG = @hTAG AND UP_DATE = @UpDate 
                  AND UP_TIME = @UpTime;

                -- 6. Other Details (Ranandeh)
                SELECT * FROM dbo.TR_OTHER_DTL WHERE NUMBER = @FactorNumber AND TAG = @fTAG AND UP_DATE = @UpDate 
                  AND UP_TIME = @UpTime;";

            // Updated SQL to include both UP_DATE and UP_TIME filters
            string sql = $@"
                -- 1. Header (Specific Version)
                SELECT * FROM dbo.TR_HEAD_LST 
                WHERE NUMBER = @FactorNumber 
                  AND TAG = @fTAG 
                  AND UP_DATE = @UpDate 
                  AND UP_TIME = @UpTime; -- Added UP_TIME

                -- 2. Invoice Items (Filtered by Header's UP_DATE & UP_TIME)
                SELECT il.*, sd.NAME AS NAME_CODE 
                FROM dbo.TR_INVO_LST il 
                LEFT JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE 
                WHERE il.NUMBER = @FactorNumber 
                  AND il.TAG = @hTAG 
                  AND il.UP_DATE = @UpDate 
                  AND il.UP_TIME = @UpTime; -- Added UP_TIME

                -- 3. Advanced Discounts
                {(isFactory ? QUERY_TR_TAKHFIF_APLAY : "")}

                -- 4. Checks
                {(isFactory ? QUERY_TR_PAY_GETD : "")}

                {(isFactory ? QUERY_SAYER : "")}

                -- 8. Customer Details
                SELECT TOP 1 * FROM dbo.CUST_HESAB 
                WHERE hes = (SELECT TOP 1 CUST_NO FROM dbo.TR_HEAD_LST 
                             WHERE NUMBER = @FactorNumber 
                               AND TAG = @fTAG 
                               AND UP_DATE = @UpDate 
                               AND UP_TIME = @UpTime);
                ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);

            // Parameters now include UpTime
            var parameters = new
            {
                FactorNumber = TrRow.NUMBER,
                fTAG = fTAG,
                hTAG = hTAG,
                UpDate = TrRow.UP_DATE,
                UpTime = TrRow.UP_TIME // Passed from the selected history row
            };

            using (var multi = await db.QueryMultipleAsync(sql, parameters))
            {
                fullDetails.Header = await multi.ReadFirstOrDefaultAsync<HEAD_LST>();
                if (fullDetails.Header == null) return null;

                fullDetails.InvoiceItems = (await multi.ReadAsync<INVO_LST_FACTOR22>()).ToList();

                if (isFactory)
                {
                    fullDetails.AdvancedDiscounts = (await multi.ReadAsync<TAKHFIF_APLAY>()).ToList();
                    fullDetails.Checks = (await multi.ReadAsync<PAY_GETD_SUB22_MODEL>()).ToList();
                    fullDetails.VisitorDetails = (await multi.ReadAsync<VISITOR_DTL>()).ToList();
                    fullDetails.OtherDetails = await multi.ReadFirstOrDefaultAsync<OTHER_DTL_CSHARP>();
                }

                fullDetails.Customer = await multi.ReadFirstOrDefaultAsync<Custom_CUST_HESAB>();
            }

            return fullDetails;
        }

        private async Task<FactorFullDetails> GetFactorFullDetailsAsync2(TR_HEAD_LST TrRow)
        {
            var fullDetails = new FactorFullDetails();

            // تعریف دقت Round (6 رقم اعشار کافی است)
            const int ROUND_PRECISION = 6;

            // Query برای تخفیف‌های پیشرفته
            string QUERY_TR_TAKHFIF_APLAY = $@"
                    SELECT tad.*, td.TSHARH 
                    FROM dbo.TR_TAKHFIF_APLAY tad
                    JOIN dbo.TAKHFIF_DEF td ON tad.TID = td.TID
                    WHERE tad.NUMBER = @FactorNumber 
                      AND tad.KIND = 2      
                      AND tad.UP_DATE = @UpDate 
                      AND ROUND(tad.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})";

            // Query برای چک‌ها
            string QUERY_TR_PAY_GETD = $@"
                    SELECT * 
                    FROM dbo.TR_PAY_GETD 
                    WHERE NUMBER = @FactorNumber 
                      AND TAG = @hTAG 
                      AND UP_DATE = @UpDate 
                      AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})";

            // Query برای سایر جزئیات
            string QUERY_SAYER = $@"
                    -- Visitor Details
                    SELECT v.*, ch.NAME AS CUST_NO_NAME
                    FROM dbo.TR_VISITOR_DTL v
                    LEFT JOIN CUST_HESAB ch ON v.CUST_NO = ch.hes
                    WHERE v.NUMBER = @FactorNumber 
                      AND v.TAG = @hTAG 
                      AND v.UP_DATE = @UpDate 
                      AND ROUND(v.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                    -- Other Details (Ranandeh)
                    SELECT * 
                    FROM dbo.TR_OTHER_DTL 
                    WHERE NUMBER = @FactorNumber 
                      AND TAG = @fTAG 
                      AND UP_DATE = @UpDate 
                      AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})";

            bool isFactor = TAGCODE == 13 || TAGCODE == 12;

            // Query اصلی
            string sql = $@"
                    -- 1. Header (Specific Version)
                    SELECT * 
                    FROM dbo.TR_HEAD_LST 
                    WHERE NUMBER = @FactorNumber 
                      AND TAG = @fTAG 
                      AND UP_DATE = @UpDate 
                      AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                    -- 2. Invoice Items (Filtered by Header's UP_DATE & UP_TIME)
                    SELECT il.*, sd.NAME AS NAME_CODE 
                    FROM dbo.TR_INVO_LST il 
                    LEFT JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE 
                    WHERE il.NUMBER = @FactorNumber 
                      AND il.TAG = @hTAG 
                      AND il.UP_DATE = @UpDate 
                      AND ROUND(il.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                    -- 3. Advanced Discounts
                    {(isFactor ? QUERY_TR_TAKHFIF_APLAY : "")}

                    -- 4. Checks
                    {(isFactor ? QUERY_TR_PAY_GETD : "")}

                    {(isFactor ? QUERY_SAYER : "")}

                    -- 8. Customer Details
                    SELECT TOP 1 * 
                    FROM dbo.CUST_HESAB 
                    WHERE hes = (
                        SELECT TOP 1 CUST_NO 
                        FROM dbo.TR_HEAD_LST 
                        WHERE NUMBER = @FactorNumber 
                          AND TAG = @fTAG 
                          AND UP_DATE = @UpDate 
                          AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})
                    );
                ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);

            // تنظیم TAG ها بر اساس نوع فاکتور
            if (TAGCODE == 13) // Sales Invoice
            {
                fTAG = 13;
                hTAG = 2;
            }
            else if (TAGCODE == 12) // Purchase Invoice
            {
                fTAG = 12;
                hTAG = 1;
            }
            else
            {
                fTAG = TAGCODE;
                hTAG = TAGCODE;
            }

            // تعریف پارامترها
            var parameters = new
            {
                FactorNumber = TrRow.NUMBER,
                fTAG = fTAG,
                hTAG = hTAG,
                UpDate = TrRow.UP_DATE,
                UpTime = TrRow.UP_TIME ?? 0 // اگر null بود، 0 می‌فرستیم
            };

            using (var multi = await db.QueryMultipleAsync(sql, parameters))
            {
                // 1. خواندن Header
                fullDetails.Header = await multi.ReadFirstOrDefaultAsync<HEAD_LST>();
                if (fullDetails.Header == null)
                {
                    return null;
                }

                // 2. خواندن Invoice Items
                fullDetails.InvoiceItems = (await multi.ReadAsync<INVO_LST_FACTOR22>()).ToList();

                // 3. خواندن سایر اطلاعات (فقط برای فاکتورهای فروش و خرید)
                if (isFactor)
                {
                    fullDetails.AdvancedDiscounts = (await multi.ReadAsync<TAKHFIF_APLAY>()).ToList();
                    fullDetails.Checks = (await multi.ReadAsync<PAY_GETD_SUB22_MODEL>()).ToList();
                    fullDetails.VisitorDetails = (await multi.ReadAsync<VISITOR_DTL>()).ToList();
                    fullDetails.OtherDetails = await multi.ReadFirstOrDefaultAsync<OTHER_DTL_CSHARP>();
                }

                // 4. خواندن اطلاعات مشتری
                fullDetails.Customer = await multi.ReadFirstOrDefaultAsync<Custom_CUST_HESAB>();
            }

            return fullDetails;
        }

        private async void ReGetData1(TR_HEAD_LST TrRow)
        {
            try
            {
                FactorFullDetails fullDetails = await GetFactorFullDetailsAsync(TrRow);

                if (fullDetails == null || fullDetails.Header == null)
                {
                    new Msgwin(false, "این فاکتور خالی است یا اطلاعات کامل آن یافت نشد.").Show();
                    return;
                }

                // --- Begin Populating UI from fullDetails model ---
                var header = fullDetails.Header;

                //if (IsExporty)
                //{
                //    ARZD.Text = header.ARZD.ToStringNullSafe();
                //    ARZKIND2.SelectedValue = header.ARZKIND2;
                //    ANBARF.Text = header.ANBARF.ToStringNullSafe();
                //}

                FACTOR22_INVO_DATA.Clear();
                fullDetails.InvoiceItems.ForEach(FACTOR22_INVO_DATA.Add);
                TAKHFIF_APLAY_DATA.Clear();
                fullDetails.AdvancedDiscounts.ForEach(TAKHFIF_APLAY_DATA.Add);
                PAY_GETD_SUB22_DATA.Clear();
                fullDetails.Checks.ForEach(PAY_GETD_SUB22_DATA.Add);
                SAYER_VISITOR_DATA.Clear();
                fullDetails.VisitorDetails.ForEach(SAYER_VISITOR_DATA.Add);

                // معین واریزی - CMB_MOIN_VAR (with BANKHA filter)
                if (!string.IsNullOrWhiteSpace(header.MOIN_VAR))
                {
                    var moinVarRecord = GetHesabhaByCode(header.MOIN_VAR);
                    if (moinVarRecord != null && moinVarRecord.N_KOL == Baseknow.BANKHA)
                    {
                        CMB_MOIN_VAR.ItemsSource = new List<CUSTOM_HESABHA> { moinVarRecord };
                        CMB_MOIN_VAR.SelectedValue = moinVarRecord.hes;
                        CMB_MOIN_VAR.DisplayMemberPath = "NAME";
                        CMB_MOIN_VAR.SelectedValuePath = "hes";
                    }
                    else
                    {
                        CMB_MOIN_VAR.ItemsSource = null;
                    }
                }

                // معين حواله - CMB_MOIN_HAV
                if (!string.IsNullOrWhiteSpace(header.MOIN_HAV))
                {
                    var moinHavRecord = GetHesabhaByCode(header.MOIN_HAV);
                    if (moinHavRecord != null)
                    {
                        CMB_MOIN_HAV.ItemsSource = new List<CUSTOM_HESABHA> { moinHavRecord };
                        CMB_MOIN_HAV.SelectedValue = moinHavRecord.hes;
                        CMB_MOIN_HAV.DisplayMemberPath = "NAME";
                        CMB_MOIN_HAV.SelectedValuePath = "hes";
                    }
                    else
                    {
                        CMB_MOIN_HAV.ItemsSource = null;
                    }
                }

                // معين خدمات - CMB_MOIN_HAZ
                if (!string.IsNullOrWhiteSpace(header.MOIN_HAZ))
                {
                    var moinHazRecord = GetHesabhaByCode(header.MOIN_HAZ);
                    if (moinHazRecord != null)
                    {
                        CMB_MOIN_HAZ.ItemsSource = new List<CUSTOM_HESABHA> { moinHazRecord };
                        CMB_MOIN_HAZ.SelectedValue = moinHazRecord.hes;
                        CMB_MOIN_HAZ.DisplayMemberPath = "NAME";
                        CMB_MOIN_HAZ.SelectedValuePath = "hes";
                    }
                    else
                    {
                        CMB_MOIN_HAZ.ItemsSource = null;
                    }
                }

                // معین مالیات - CMB_HMBAA
                if (!string.IsNullOrWhiteSpace(header.HMBAA))
                {
                    var hmbaaRecord = GetHesabhaByCode(header.HMBAA);
                    if (hmbaaRecord != null)
                    {
                        CMB_HMBAA.ItemsSource = new List<CUSTOM_HESABHA> { hmbaaRecord };
                        CMB_HMBAA.SelectedValue = hmbaaRecord.hes;
                        CMB_HMBAA.DisplayMemberPath = "NAME";
                        CMB_HMBAA.SelectedValuePath = "hes";
                    }
                    else
                    {
                        CMB_HMBAA.ItemsSource = null;
                    }
                }

                M_NAGHD.Text = header.M_NAGHD.ToStringNullSafe();
                MABL_VAR.Text = header.MABL_VAR.ToStringNullSafe();
                MABL_HAV.Text = header.MABL_HAV.ToStringNullSafe();
                TAKHFIF.Text = header.TAKHFIF.ToStringNullSafe();
                MOIN_VAR.Text = header.MOIN_VAR.ToStringNullSafe();
                MOIN_HAV.Text = header.MOIN_HAV.ToStringNullSafe();

                MABL_HAZ.Text = header.MABL_HAZ.ToStringNullSafe();
                MOIN_HAZ.Text = header.MOIN_HAZ;
                MBAA.Text = header.MBAA.ToStringNullSafe();
                HMBAA.Text = header.HMBAA;

                if (fullDetails.OtherDetails != null)
                {
                    var other = fullDetails.OtherDetails;
                    //REQUEST_NO.Text = other.REQUEST_NO;
                    //DRIVER_MOB.Text = other.DRIVER_MOB;
                    //MAGHSAD.SelectedValue = other.MAGHSAD;
                    //BARNAMEH.Text = other.BARNAMEH;
                    //CAMIUN_NUM.Text = other.CAMIUN_NUM;
                    //CAM_KHALY.Text = other.CAM_KHALY.ToStringNullSafe();
                    //DRIVER.Text = other.DRIVER;
                    //CAMIUN.Text = other.CAMIUN;
                    //CAM_POOR.Text = other.CAM_POOR.ToStringNullSafe();
                    //TOZIH.Text = other.TOZIH;
                }

                // Recalculate sums and totals
                MasterSummerAndMandeh();

                //MANDAH.Text = fullDetails.AccountBalance;
                //N_S.Text = header.N_S.ToStringNullSafe();
                //MABNA.Text = fullDetails.SanadBase;

                GenerateAutomaticSummary(SF_SUB);
            }
            catch (Exception ex)
            {
                // Log and show error
                new Msgwin(false, $"خطا در بارگذاری اطلاعات فاکتور").ShowDialog();
            }
            finally
            {
                // Optional: Hide loading indicator here
            }
        }
        private async void ReGetData(TR_HEAD_LST TrRow)
        {
            // جلوگیری از اجرای همزمان روی یک رکورد یا خطا
            if (TrRow == null) return;

            // تعریف تسک دریافت اطلاعات (هنوز await نمی‌کنیم تا اجرا شروع شود)
            var dataFetchTask = GetFactorFullDetailsAsync(TrRow);

            // تعریف یک تاخیر ۳۰۰ میلی‌ثانیه‌ای (آستانه تحمل کاربر قبل از نیاز به لودینگ)
            var delayTask = Task.Delay(300);

            // مسابقه بین دریافت اطلاعات و تاخیر
            // اگر دیتابیس سریعتر از ۳۰۰ میلی‌ثانیه جواب داد، وارد شرط نمی‌شود
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);

            bool loaderShown = false;

            if (completedTask == delayTask)
            {
                // یعنی ۳۰۰ میلی‌ثانیه گذشت و دیتا هنوز لود نشده است
                // پس لودینگ را نشان بده
                BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                // حالا منتظر نتیجه نهایی دیتا می‌مانیم
                // اگر دیتا قبلاً آماده شده باشد (در حالت سریع)، این خط بلافاصله اجرا می‌شود
                FactorFullDetails fullDetails = await dataFetchTask;

                if (fullDetails == null || fullDetails.Header == null)
                {
                    // اگر لودینگ نمایش داده شده بود، مخفی شود تا پیام خطا دیده شود
                    if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;

                    new Msgwin(false, "این فاکتور خالی است یا اطلاعات کامل آن یافت نشد.").Show();
                    return;
                }

                // --- شروع پر کردن فیلدها (کدهای قبلی بدون تغییر) ---
                var header = fullDetails.Header;

                FACTOR22_INVO_DATA.Clear();
                fullDetails.InvoiceItems.ForEach(FACTOR22_INVO_DATA.Add);

                TAKHFIF_APLAY_DATA.Clear();
                fullDetails.AdvancedDiscounts.ForEach(TAKHFIF_APLAY_DATA.Add);

                PAY_GETD_SUB22_DATA.Clear();
                fullDetails.Checks.ForEach(PAY_GETD_SUB22_DATA.Add);

                SAYER_VISITOR_DATA.Clear();
                fullDetails.VisitorDetails.ForEach(SAYER_VISITOR_DATA.Add);

                // --- منطق پر کردن کمبوباکس‌ها (Hesabha Logic) ---
                void SetCombo(ComboBox cmb, string hesCode, int? nKolFilter = null)
                {
                    if (!string.IsNullOrWhiteSpace(hesCode))
                    {
                        var record = GetHesabhaByCode(hesCode);
                        if (record != null && (nKolFilter == null || record.N_KOL == nKolFilter))
                        {
                            cmb.ItemsSource = new List<CUSTOM_HESABHA> { record };
                            cmb.SelectedValue = record.hes;
                            cmb.DisplayMemberPath = "NAME";
                            cmb.SelectedValuePath = "hes";
                        }
                        else
                        {
                            cmb.ItemsSource = null;
                        }
                    }
                    else
                    {
                        cmb.ItemsSource = null;
                        cmb.SelectedItem = null;
                    }
                }

                SetCombo(CMB_MOIN_VAR, header.MOIN_VAR, (int)Baseknow.BANKHA);
                SetCombo(CMB_MOIN_HAV, header.MOIN_HAV);
                SetCombo(CMB_MOIN_HAZ, header.MOIN_HAZ);
                SetCombo(CMB_HMBAA, header.HMBAA);

                // --- Text Fields ---
                M_NAGHD.Text = header.M_NAGHD.ToStringNullSafe();
                MABL_VAR.Text = header.MABL_VAR.ToStringNullSafe();
                MABL_HAV.Text = header.MABL_HAV.ToStringNullSafe();
                TAKHFIF.Text = header.TAKHFIF.ToStringNullSafe();
                MOIN_VAR.Text = header.MOIN_VAR.ToStringNullSafe();
                MOIN_HAV.Text = header.MOIN_HAV.ToStringNullSafe();

                MABL_HAZ.Text = header.MABL_HAZ.ToStringNullSafe();
                MOIN_HAZ.Text = header.MOIN_HAZ;
                MBAA.Text = header.MBAA.ToStringNullSafe();
                HMBAA.Text = header.HMBAA;

                if (fullDetails.OtherDetails != null)
                {
                    var other = fullDetails.OtherDetails;
                    REQUEST_NO.Text = other.REQUEST_NO;
                    DRIVER_MOB.Text = other.DRIVER_MOB;
                    MAGHSAD.SelectedValue = other.MAGHSAD;
                    BARNAMEH.Text = other.BARNAMEH;
                    CAMIUN_NUM.Text = other.CAMIUN_NUM;
                    CAM_KHALY.Text = other.CAM_KHALY.ToStringNullSafe();
                    DRIVER.Text = other.DRIVER;
                    CAMIUN.Text = other.CAMIUN;
                    CAM_POOR.Text = other.CAM_POOR.ToStringNullSafe();
                    TOZIH.Text = other.TOZIH;
                }

                MasterSummerAndMandeh();
                GenerateAutomaticSummary(SF_SUB);
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری اطلاعات").ShowDialog();
            }
            finally
            {
                // فقط اگر لودینگ نمایش داده شده بود، آن را مخفی کن
                if (loaderShown)
                {
                    BusyOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void MasterSummerAndMandeh()
        {
            Summer();
        }

        private void Summer()
        {
            if (PAGE_POSHT.Visibility == Visibility.Visible)
            {
                NCHK.Text = PAY_GETD_SUB22_DATA?.Sum(x => x?.MABL ?? 0).ToString();

                HKH.Text = MABL_HAZ.Text; // هزینه خدمات
                NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
                JF.Text = SUM_OF_MABL_K.ToString(); //جمع کل فاکتور برای فسمت روی فاکتور

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
        }

        #region _SfDataGrid_
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            //// Defensive checks
            //if (ROWCOUNT_TEXTBLK == null) return;
            //if (SYNCFUSION_DG?.View == null) return;

            //// Safely retrieve the record count
            //var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            //// Set the label content
            //ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<TR_HEAD_LST> filterService = new FilterService<TR_HEAD_LST>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool IsExporty { get; private set; } = false;

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private bool isFactory = false;

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            if (!NowIsReady) return;

            if (SYNCFUSION_DG.SelectedItem is TR_HEAD_LST SelectedHeadFactor)
            {
                if (SelectedHeadFactor?.NUMBER != null)
                {
                    ReGetData(SelectedHeadFactor);
                }
                else
                {
                    FACTOR22_INVO_DATA?.Clear();
                    TAKHFIF_APLAY_DATA?.Clear();
                    PAY_GETD_SUB22_DATA?.Clear();
                    SAYER_VISITOR_DATA?.Clear();
                }
            }
            //// Get the selected row and column index
            //var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null)
            //{
            //    var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
            //    UpdateCurrentCellValue(rowColumnIndex);
            //}
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);


            if (record == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                return;
            }
            var property = record.GetType().GetProperty(mappingName);
            if (property == null)
            {
                //Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
                return;
            }

            //CurrentCellValue = property.GetValue(record)?.ToString();
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController?.CurrentCellManager?.CurrentCell;

            if (currentCell == null)
                return string.Empty;

            // حالت 1: Edit Mode
            if (currentCell.IsEditing)
            {
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null && !string.IsNullOrEmpty(editingElement.SelectedText))
                {
                    return editingElement.SelectedText;
                }
            }

            // حالت 2: جستجوی ساده - بدون GetCellElement
            try
            {
                var gridCellElement = currentCell?.ColumnElement;
                if (gridCellElement != null)
                {
                    var textBox = FindVisualChild<TextBox>(gridCellElement);
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.SelectedText))
                    {
                        return textBox.SelectedText;
                    }
                }
            }
            catch { }

            return string.Empty;
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر Contains
                filterService.AddFilter(columnName, selectedText, isExclusion: false, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Contains \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است
            if (filterValue != null)
            {
                // فیلتر Exact Match
                filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: true);

                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} = {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // فیلتر برای null values
                filterService.AddFilter(columnName, null, isExclusion: false, isExactMatch: true);
                ActiveFilters.Add($"{columnName} = NULL");
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            // اگر ستون یا مقدار معتبر نیست، خروج
            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است (partial selection)
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر "Does Not Contain" - برای متن
                filterService.AddFilter(columnName, selectedText, isExclusion: true, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Does Not Contain \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است (exact value)
            if (filterValue != null)
            {
                // فیلتر Exclusion با Exact Match - برای مقدار دقیق
                filterService.AddFilter(columnName, filterValue, isExclusion: true, isExactMatch: true);

                // نمایش بهتر در لیست فیلترها
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} != {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // اگر مقدار null است
                filterService.AddFilter(columnName, null, isExclusion: true, isExactMatch: true);
                ActiveFilters.Add($"{columnName} != NULL");
                ApplyCumulativeFilter();
            }
        }
        private string FormatValueForDisplay(object value)
        {
            if (value == null)
                return "NULL";

            // برای مقادیر عددی، فرمت هزارگان اعمال می‌شود
            if (value is double || value is decimal || value is float)
            {
                try
                {
                    return Convert.ToDecimal(value).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (value is int || value is long || value is short || value is byte)
            {
                try
                {
                    return Convert.ToInt64(value).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }

        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e) // Event handler to remove all filters and sorting
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails() // Method to get the details of the selected cell
        {
            // Check if there is a current cell selected in the data grid
            if (SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                          // Return the column name and the current cell value
                return (columnName, CurrentCellValue);
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter() // Method to apply all cumulative filters to the data grid
        {
            // Set the filter for the data grid view using the filter service
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TR_HEAD_LST);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();

            UpdateRowCountLabel();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShow("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }
            }
            catch { return; }

            // Check if there are selected rows
            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in SYNCFUSION_DG.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in SYNCFUSION_DG.SelectedItems)
                {
                    foreach (var column in SYNCFUSION_DG.Columns)
                    {
                        if (!column.IsHidden) // Include only columns that are not hidden
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{SYNCFUSION_DG.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }
        }
        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            // Ensure rows are selected
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0)
            {
                return;
            }

            // Detect the current column
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null)
            {
                return;
            }

            string columnName = currentColumn.MappingName; // Get the column name
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            decimal sum = 0;
            bool isNumericColumn = false;

            // Iterate through the selected rows
            foreach (var selectedItem in _DG_.SelectedItems)
            {
                // Get the cell value for the detected column
                var cellValue = GetCellValue(selectedItem, columnName);

                if (cellValue != null && decimal.TryParse(cellValue.ToStringNullSafe(), out decimal numericValue))
                {
                    sum += numericValue;
                    isNumericColumn = true;
                }
            }

            if (isNumericColumn)
            {
                string formattedSum = sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                new Msgwin(false, $"جمع سطر های انتخاب شده در ستون [{currentColumn.HeaderText}] برار است با : {formattedSum}").ShowDialog();

            }
        }
        private object GetCellValue(object record, string columnName)
        {
            try
            {
                // Use reflection to get the property value from the record
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch
            {
                return null;
            }
        }
        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                SYNCFUSION_DG.TableSummaryRows.Clear();
            }
            else
            {
                // Check if a summary row already exists
                if (_DG_.TableSummaryRows.Count > 0)
                {
                    return; // Exit the method if a summary row already exists
                }
            }

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            var dataType = typeof(TR_HEAD_LST);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(TR_HEAD_LST).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
                {
                    var summaryColumn = new GridSummaryColumn
                    {
                        Name = column.MappingName + "Sum",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                        //Format = "{Sum:N0}"
                        Format = "{Sum:N0}"
                    };
                    summaryColumns.Add(summaryColumn);
                }
            }

            summaryRow.SummaryColumns = summaryColumns;

            _DG_.TableSummaryRows.Add(summaryRow);


        }
        private bool IsNumericType(Type type)
        {
            if (type == null)
                return false;

            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type that might represent a number
            if (type == typeof(object))
            {
                return true; // Assume it might be numeric
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion

        #region Navigation Logic
        // 1. این متد را در انتهای Window_Loaded صدا بزنید
        // --- Safe Navigation Logic ---

        private void SetupGridNavigation()
        {
            // 1. بررسی ایمنی: اگر کنترل‌ها هنوز ساخته نشده‌اند، خارج شو
            // این خط جلوی خطای NullReference را می‌گیرد اگر XAML آپدیت نشده باشد
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null)
            {
                // جهت دیباگ: اگر این خط اجرا شد یعنی یکی از نام‌ها در XAML اشتباه است
                return;
            }

            // 2. اتصال رویداد تغییر انتخاب (Selection)
            // ابتدا حذف می‌کنیم تا دوبار متصل نشود (-=)
            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            // 3. اتصال رویداد تغییر تعداد رکوردها (Collection Changed)
            // نکته مهم: بررسی می‌کنیم که View آماده است یا نه
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                // اگر ویو آماده بود، وصل شو
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            else
            {
                // اگر ویو هنوز نال بود، به رویداد Loaded خود گرید وصل می‌شویم تا بعدا انجام دهیم
                SYNCFUSION_DG.Loaded -= OnGridLoadedForNav;
                SYNCFUSION_DG.Loaded += OnGridLoadedForNav;
            }

            // 4. آپدیت اولیه متن‌ها (با بررسی نال)
            UpdateNavigationText();
        }

        // اگر گرید در ابتدا آماده نبود، این متد بعداً صدا زده می‌شود
        private void OnGridLoadedForNav(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
                UpdateNavigationText();
            }
        }

        // هندلرهای کمکی برای جلوگیری از خطای ترد
        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            // بررسی ایمنی مجدد
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            int total = 0;
            int current = 0;

            try
            {
                // محاسبه تعداد کل (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
                {
                    total = SYNCFUSION_DG.View.Records.Count;
                }

                // محاسبه ایندکس جاری (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.SelectedIndex >= 0)
                {
                    current = SYNCFUSION_DG.SelectedIndex + 1;
                }
            }
            catch
            {
                // نادیده گرفتن خطا در شرایط خاص
            }

            // نمایش
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        // 3. رویدادهای کلیک دکمه‌ها
        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSfDataFilters(); // حذف فیلترها
                ReGetHeadMaster();
                Btn_First_Click(default, default);
            }
            catch { }
        }
        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    SYNCFUSION_DG.SelectedIndex = 0;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(1, 0));
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToHome();
                }
            }
            catch { }
        }

        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex > 0)
                {
                    SYNCFUSION_DG.SelectedIndex--;
                    // اسکرول به ایندکس جدید (ایندکس رکورد + هدرها)
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(SYNCFUSION_DG.SelectedIndex + 1, 0));
                    //var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    //SYNCFUSION_DG.GetVisualContainer().ScrollRows.ScrollInView(rowIndex, 0);

                    SYNCFUSION_DG.SelectedIndex--;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید (با احتساب هدرها و فیلترها)
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون قابل مشاهده (برای ساخت RowColumnIndex صحیح)
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                {
                    SYNCFUSION_DG.SelectedIndex++;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    var lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                    SYNCFUSION_DG.SelectedIndex = lastIndex;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(lastIndex + 1, 0));

                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSfDataFilters();
        }

        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                ApplyCumulativeFilter();
                SYNCFUSION_DG.ClearFilters();
                SF_SUB.ClearFilters();
                PAY_GETD_SUB22.ClearFilters();
                VISITOR_DTL_SUB.ClearFilters();
            }
            catch (Exception)
            {
            }
        }
    }
}
