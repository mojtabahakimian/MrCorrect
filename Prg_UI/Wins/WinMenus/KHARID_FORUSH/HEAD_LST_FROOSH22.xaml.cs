using Dapper;
using Functions;
using Functions.SMSService;
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
using Prg_UI.Validatory;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinOther;
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.HESABDARI;
using Wins.WinOther;
using static Interfaces.INavigator;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.HelperWins.Msgwin;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Wins.WinMenus.KHARID_FORUSH.HEAD_LST_PISHFROOSH2;
using Convert = System.Convert;
using DataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using Msgwin = Prg_UI.HelperWins.Msgwin;


//مواردی که باید بعدا در نظر گرفته شود :
//1- تایمر و گرفتن تاریخ فارسی
//2- CHEKDATE توی  Form_BeforeInsert
//3- SANAD مد محافظت شده تعریف نشده برای سند زدن
namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class HEAD_LST_FROOSH22 : Window, ISearchableWindow
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

        #region MODELS
        public class _FACT_HEAD_HAV_
        {
            public string? CUST_NO { get; set; }
            public double? MAS { get; set; }
            public int? DEPATMAN { get; set; }
            public bool? TICMBAA { get; set; }
            public string? SHARAYET { get; set; }
            public double? FNUMCO { get; set; }
            public bool? JAY { get; set; }
            public int? MODAT_PPID { get; set; }
            public int? PEID { get; set; }
            public int? PEPID { get; set; }
            public string? USER_NAME { get; set; }
            public int? CUST_KIND { get; set; }
        }
        public class MG_MODEL3
        {
            public double? TAG { get; set; }
            public double? mrgh { get; set; }
        }
        public class MG_MODEL2
        {
            public double? SumOfMABL_K { get; set; }
            public double? MEGHk { get; set; }
        }
        public class _MG_MODEL2_
        {
            public double? NUMBER { get; set; }
        }
        public class MG_MODEL1
        {
            public double? MEGHJAY { get; set; }
            public double? MEGHTA { get; set; }
            public int? VAHED { get; set; }
        }
        class RPT_MODEL5
        {
            public double? TAG { get; set; }
            public string? CODE { get; set; }
            public double? mrgh { get; set; }
        }
        public class RPT_MODEL4
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? Expr1 { get; set; }
            public string? CUST_NO { get; set; }
            public double? TAKHFIF { get; set; }
            public double? MBAA { get; set; }
            public double? MABL_HAZ { get; set; }
            public double? VAS { get; set; }
            public long? DATE_N { get; set; }
        }
        public class RPT_MODEL3
        {
            public double? NUMBER { get; set; }
            public double? htag { get; set; }
            public int? ANBAR { get; set; }
            public double? NUMBER1 { get; set; }
            public long? DATE_N { get; set; }
            public string? TAH { get; set; }
            public double? MAS { get; set; }
            public double? VAS { get; set; }
            public double? N_S { get; set; }
            public string? CUST_NO { get; set; }
            public string? MOLAH { get; set; }
            public double? M_NAGHD { get; set; }
            public double? MABL_VAR { get; set; }
            public string? MOIN_VAR { get; set; }
            public double? MABL_HAV { get; set; }
            public string? MOIN_HAV { get; set; }
            public double? MABL_HAZ { get; set; }
            public string? MOIN_HAZ { get; set; }
            public double? TAKHFIF { get; set; }
            public string? MOIN_KHF { get; set; }
            public int? ANBARF { get; set; }
            public double? FNUMCO { get; set; }
            public double? MBAA { get; set; }
        }
        public class RPT_MODEL2
        {
            public double? N_SERI { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public long? DATE { get; set; }
            public long? DATE_S { get; set; }
            public double? MABL { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
        }
        public class RPT_MODEL1
        {
            public double? NUMBER { get; set; }
            public double? HTAG { get; set; }
            public string? MOLAH { get; set; }
        }
        public class PORD_COL_MODEL
        {
            public int? PORID { get; set; }
            public string? Expr1 { get; set; }
        }
        internal class SQL1_FACTOR
        {
            public int? n_kol { get; set; }
            public int? NUMBER { get; set; }
            public int? tNUMBER { get; set; }
        }
        internal class MXNF
        {
            public double? MaxOfNUMBER { get; set; }
            public double? MABL { get; set; }
        }
        internal class _NFANI_
        {
            public string N_FANI { get; set; }
            public string CODE { get; set; }
        }
        internal class _MX_
        {
            public string CODE { get; set; }
            public double? MAX_M { get; set; }
        }
        internal class _VT_
        {
            public int? VAHED { get; set; }
            public string TOZIH { get; set; }
        }
        internal class HLF0
        {
            public string hes { get; set; }
            public int? CUST_COD { get; set; }
        }
        internal class HLF3
        {
            public int? VAHED_K { get; set; }
            public long? idd { get; set; }
            public string CODE { get; set; }
            public int? VAHED { get; set; }
            public double? MEGHTA { get; set; }
            public double? MEGHJAY { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public double? RADIF { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public long? JAY { get; set; }
            public int? JAYO { get; set; }
            public long? id { get; set; }
        }
        internal class HLF2
        {
            public bool? CMBAA { get; set; }
            public string CODE { get; set; }
        }
        internal class HLF1
        {
            public int? shift { get; set; }
            public int? CUST_KIND { get; set; }
        }
        internal class SGNS_CSHARP
        {
            public bool? FFR_FROOSH { get; set; }
            public bool? FFR_HESAB { get; set; }
            public bool? FFR_MODIR { get; set; }
        }
        internal class FC1
        {
            public double? VAS { get; set; }
            public bool? TICMBAA { get; set; }
            public double? MaxOfNUMBER { get; set; }
        }

        internal class SANAD_JST
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
        }
        internal class SANAD_JST_2
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
            public double? AVRAGE { get; set; }
        }
        internal class SAND_RSTS
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? CUST_CO { get; set; }
            public string TAKH_COD { get; set; }
            public short? TAFPER { get; set; }
            public double? MABL_K { get; set; }
        }
        internal class HF1
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? MABL_K { get; set; }
            public double? N_MOIN { get; set; }
            public string CODE { get; set; }
            public int? CUST_KIND { get; set; }
        }
        internal class HF2
        {
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public string NAME { get; set; }
            public string TOZIH { get; set; }
        }
        internal class JST3_QRE
        {
            public string MANDAH { get; set; }
            public double? MABL_K { get; set; }
            public double? N_KOL { get; set; }
            public double? FNUMCO { get; set; }
            public string KALA { get; set; }
        }
        internal class rstt_QRE
        {
            public int? TNUMBER { get; set; }
            public string NAME { get; set; }
            public string TOZIH { get; set; }
        }
        internal class rst1_QRE
        {
            public string code { get; set; }
            public double? mablk { get; set; }
        }
        internal class QRE_MX
        {
            public double? MaxOfNUMBER { get; set; }
            public double? MABL { get; set; }
        }
        internal class HES_QRE
        {
            public string hes { get; set; }
            public int? CUST_COD { get; set; }
        }
        internal class HES_QRE2
        {
            public int? NUMBER { get; set; }
            public string? NAME { get; set; }
        }
        internal class _HES_QRE3_
        {
            public int? TNUMBER { get; set; }
            public string? NAME { get; set; }
        }

        public class FactorFullDetails
        {
            public HEAD_LST Header { get; set; }
            public List<INVO_LST_FACTOR22> InvoiceItems { get; set; } = new List<INVO_LST_FACTOR22>();
            public List<TAKHFIF_APLAY> AdvancedDiscounts { get; set; } = new List<TAKHFIF_APLAY>();
            public List<PAY_GETD_SUB22_MODEL> Checks { get; set; } = new List<PAY_GETD_SUB22_MODEL>();
            public List<VISITOR_DTL> VisitorDetails { get; set; } = new List<VISITOR_DTL>();
            public OTHER_DTL_CSHARP OtherDetails { get; set; }
            public HEAD_LST_EXTENDED TaxDetails { get; set; }
            public Custom_CUST_HESAB Customer { get; set; }
            public string AccountBalance { get; set; } // MANDAH
            public string SanadBase { get; set; } // MABNA
        }

        private async Task<FactorFullDetails> GetFactorFullDetailsAsync(double factorNumber)
        {
            var fullDetails = new FactorFullDetails();
            string sql = $@"
                -- 1. Header
                SELECT * FROM dbo.HEAD_LST WHERE NUMBER = @FactorNumber AND TAG = @fTAG;

                -- 2. Invoice Items
                SELECT il.*, sd.NAME AS NAME_CODE 
                FROM dbo.INVO_LST il 
                LEFT JOIN dbo.STUF_DEF sd ON il.CODE = sd.CODE 
                WHERE il.NUMBER = @FactorNumber AND il.TAG = @hTAG;

                -- 3. Advanced Discounts
                SELECT tad.*, td.TSHARH 
                FROM dbo.TAKHFIF_APLAY tad
                JOIN dbo.TAKHFIF_DEF td ON tad.TID = td.TID
                WHERE tad.NUMBER = @FactorNumber AND tad.KIND = 2; -- Assuming kind=2 is for sales invoices

                -- 4. Checks
                SELECT * FROM dbo.PAY_GETD WHERE NUMBER = @FactorNumber AND TAG = @hTAG AND (N_KOL IS NULL OR N_KOL <> 911);

                -- 5. Visitor Details
                SELECT v.*, ch.NAME AS CUST_NO_NAME
                FROM dbo.VISITOR_DTL v
                LEFT JOIN CUST_HESAB ch ON v.CUST_NO = ch.hes
                WHERE v.NUMBER = @FactorNumber AND v.TAG = @hTAG;

                -- 6. Other Details (Ranandeh)
                SELECT * FROM dbo.OTHER_DTL WHERE NUMBER = @FactorNumber AND TAG = @fTAG;

                -- 7. Moadian/Tax Details
                SELECT * FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = @FactorNumber AND tgu = 2;

                -- 8. Customer Details (Fetch based on Header's CUST_NO)
                SELECT TOP 1 * FROM dbo.CUST_HESAB WHERE hes = (SELECT CUST_NO FROM dbo.HEAD_LST WHERE NUMBER = @FactorNumber AND TAG = @fTAG);

                -- 9. Account Balance (Mandeh)
                -- This will be executed separately as it's a scalar function call

                -- 10. Sanad Base (Mabna)
                SELECT TOP 1 BASE FROM dbo.DEED_HED WHERE NO_S = 2 AND N_S = (SELECT N_S FROM dbo.HEAD_LST WHERE NUMBER = @FactorNumber AND TAG = @fTAG);
            ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);

            using (var multi = await db.QueryMultipleAsync(sql, new { FactorNumber = factorNumber, fTAG = this.fTAG, hTAG = this.hTAG }))
            {
                fullDetails.Header = await multi.ReadFirstOrDefaultAsync<HEAD_LST>();
                if (fullDetails.Header == null) return null; // Factor not found

                fullDetails.InvoiceItems = (await multi.ReadAsync<INVO_LST_FACTOR22>()).ToList();
                fullDetails.AdvancedDiscounts = (await multi.ReadAsync<TAKHFIF_APLAY>()).ToList();
                fullDetails.Checks = (await multi.ReadAsync<PAY_GETD_SUB22_MODEL>()).ToList();
                fullDetails.VisitorDetails = (await multi.ReadAsync<VISITOR_DTL>()).ToList();
                fullDetails.OtherDetails = await multi.ReadFirstOrDefaultAsync<OTHER_DTL_CSHARP>();
                fullDetails.TaxDetails = await multi.ReadFirstOrDefaultAsync<HEAD_LST_EXTENDED>();
                fullDetails.Customer = await multi.ReadFirstOrDefaultAsync<Custom_CUST_HESAB>();
                fullDetails.SanadBase = await multi.ReadFirstOrDefaultAsync<string>();
            }

            // Execute scalar function for account balance separately
            if (fullDetails.Header?.CUST_NO != null)
            {
                fullDetails.AccountBalance = CL_HESABDARI.GETMANDAH(fullDetails.Header.CUST_NO);
            }

            return fullDetails;
        }
        #endregion

        //System.Windows.Threading.DispatcherTimer MeTimer = new System.Windows.Threading.DispatcherTimer();

        GeneralOptionManager GOM = new();

        /// <summary>
        /// نمایش سطر های کالا در دیتاگرید
        /// </summary>
        public ObservableCollection<INVO_LST_FACTOR22> FACTOR22_INVO_DATA { get; set; } = new ObservableCollection<INVO_LST_FACTOR22>();
        public ObservableCollection<TAKHFIF_APLAY> TAKHFIF_APLAY_DATA { get; set; } = new ObservableCollection<TAKHFIF_APLAY>();
        public ObservableCollection<PAY_GETD_SUB22_MODEL> PAY_GETD_SUB22_DATA { get; set; } = new ObservableCollection<PAY_GETD_SUB22_MODEL>();
        public ObservableCollection<VISITOR_DTL> SAYER_VISITOR_DATA { get; set; } = new ObservableCollection<VISITOR_DTL>();

        private NavigationManager<HEAD_LST> _navigationManager;
        public bool IsOpenedFromAutomation { get; } = false;
        public bool IsFromPishFactorConverted { get; private set; }

        /// <summary>
        /// شماره فاکتور و شماره حواله را دریافت میکند به این صورت :
        /// LEFT → 1-FACTOR NUMBER1 , 2-HAVALEH NUMBER
        /// </summary>
        /// <param name="_openargs"></param>
        public HEAD_LST_FROOSH22(string? _openargs = null, bool? _IsDirectFactor_ = null, bool _IsExporty_ = false, bool _isAutomasion_ = false, bool _isFromPish_ = false)
        {
            OpenArgs = _openargs;
            InitializeComponent();

            if (_IsDirectFactor_ != null)
            {
                IsDirectFactor = (bool)_IsDirectFactor_;
            }

            IsExporty = _IsExporty_;

            if (!string.IsNullOrEmpty(OpenArgs))
            {
                //→
                var F_H = OpenArgs.Split(',');
                if (Convert.ToDouble(F_H[0]) > 0)
                {
                    NUMBER1.Text = F_H[0]; //شماره فاکتور
                    NUMBER1.UpdateLayout();
                }
                if (Convert.ToDouble(F_H[1]) > 0)
                {
                    NUMBER.Text = F_H[1]; //شماره حواله
                    NUMBER.UpdateLayout();
                }
                IsOpenedFromAutomation = _isAutomasion_;

                IsFromPishFactorConverted = _isFromPish_;
            }

            this.DataContext = this;
            //this.Owner = PublicVRB.WINBASE;
            //MeTimer.Tick += Form_Timer;
            //MeTimer.Interval = new TimeSpan(0, 0, 5, 0);


            if (_IsDirectFactor_ == null) //اگر مشخص نکرده که مستقیم یا غیر مستقیم است خودت باتوجه به دسترسی تنظیم کن
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 53, 1) == "5")
                {
                    IsDirectFactor = false; //حواله ای
                }
                else if (Strings.Mid(Baseknow.OPTIONSS, 18, 1) != "5")
                {
                    if (Baseknow.TKHF < 2)
                    { }
                    else
                    {
                        IsDirectFactor = false; //حواله ای
                    }
                }
                else
                {
                    IsDirectFactor = true;
                }
            }

        }

        private bool _isDirectFactor = true;
        /// <summary>
        /// اگر فاکتور فروش مستقیم است = حواله خودکار 
        /// </summary>
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
        /// فاکتور فروش صادراتی
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
                    Page155.Visibility = Visibility.Hidden; //سایر

                }
                else
                {
                    EXPORTY_GRID.Visibility = Visibility.Hidden;
                    //Rows
                    N_TAF_COLUMN.Visibility = Visibility.Hidden;
                    TOTALARZ_COLUMN.Visibility = Visibility.Hidden;
                }
            }
        }


        InventoryManager IVM = new InventoryManager(); //مدیریت موجودی ایزوله


        List<COMBOPERSONEL> rst_personel = null;

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

        ////public byte TAG { get; set; } = 2; //13

        /// <summary>
        /// تگ فاکتور فروش 13 | HEAD_LST | OTHER_DTL | DEED_DTL
        /// </summary>
        public byte fTAG { get; } = 13;

        /// <summary>
        /// تگ  حواله 2 | HEAD_LST | INVO_LST | PAY_GETD | VISITOR_DTL
        /// </summary>
        public byte hTAG { get; } = 2;

        public class SGN_IMODEL
        {
            public string SEMAT_USER { get; set; } //nemz1 : عنوان کاربر
            public string NAME_HESAB_USER { get; set; } //semat1 : سمت تنظیم شده
        }
        public class EMZAMODEL
        {
            //  Call Showemza(Me, "FFR_FROOSHTX", "FFR_HESABTX", "FFR_MODIRTX") چاپ فاکتور
            //  Call Showemza(Me, "FFR_FROOSHTX", "FFR_HESABTX", "FFR_MODIRTX") چاپ فاکتور کوچک
            public string SGN1 { get; set; } = "FFR_FROOSHTX";
            public string SGN2 { get; set; } = "FFR_HESABTX";
            public string SGN3 { get; set; } = "FFR_MODIRTX";

            private bool isRasmi = false;
            public bool IsRasmi
            {
                get { return isRasmi = false; }
                set
                {
                    isRasmi = value;

                    if (!isRasmi)
                    {
                        SGN1 = "FFR_FROOSHTX";
                        SGN2 = "FFR_HESABTX";
                        SGN3 = "FFR_MODIRTX";
                    }
                    else
                    {
                        SGN1 = "FFRB_FROOSHTX";
                        SGN2 = "FFRB_ANBTX";
                        SGN3 = "FFRB_HESABTX";
                    }
                }
            }
            //  Call Showemza(Me, "FFRB_FROOSHTX", "FFRB_ANBTX", "FFRB_HESABTX") چاپ فاکتور 1
            //  Call Showemza(Me, "FFRB_FROOSHTX", "FFRB_ANBTX", "FFRB_HESABTX") چاپ فاکتور C
        }
        private SGN_IMODEL _sgn1_info = new SGN_IMODEL();
        public EMZAMODEL EMZAPARAM { get; set; } = new EMZAMODEL();
        public SGN_IMODEL SGN1_INFO
        {
            get
            {
                if (SGN1usid.Tag is not null)
                {
                    _sgn1_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN1usid.Tag), EMZAPARAM.SGN1);
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
                    _sgn2_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN2usid.Tag), EMZAPARAM.SGN2);
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
                    _sgn3_info.SEMAT_USER = CL_HESABDARI.Getusersemat(Convert.ToInt32(SGN3usid.Tag), EMZAPARAM.SGN3);
                    _sgn3_info.NAME_HESAB_USER = CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(Convert.ToInt32(SGN3usid.Tag)));
                }
                return _sgn3_info;
            }
        }

        /// <summary>
        /// آیا تغییر اتفاق افتاده , در دکمه ریست میشود و در صورت تغییر با صفحه کلید فعال میشود
        /// </summary>
        public bool ChangeIsHappend { get; set; } = false;

        /// <summary>
        /// فقط از فیلد مقصد از حواله
        /// </summary>
        public double? MAS_MAGHSAD_HV { get; set; } = 0;

        #region GUID
        //فاکتور فروش مستقیم HEAD_LST_FROOSH22 :
        ////HEADER:
        //HEAD_LST
        //    fTAG = 13 && 2(First insert[NUMBER]2 => Second insert[NUMBER1]13) دوتا سطر هید ال اس تی یکی با تگ 13 برای حواله یکی با تگ 2 برای فاکتور فروش

        //DEED_DTL
        //    TAG = 13

        ////BODY:
        //INVO_LST
        //    TAG = 2

        //PAY_GETD
        //    TAG = 2

        //OTHER_DTL //راننده
        //    TAG = 13

        //VISITOR_DTL //سایر - پورسانت
        //    TAG = 2

        //NO_S = 2 => DEED_HED نوع سند
        #endregion

        private byte _vas;
        public byte VAS
        {
            get
            {
                if (VAS1.IsChecked is true)
                {
                    _vas = 1;
                }
                else if (VAS2.IsChecked is true)
                {
                    _vas = 2;
                }
                return _vas;
            }
        }
        List<Custom_VAHEDK> RST_KALAVAHED_LST = null;
        List<Custom_VAHEDK> RST_FULLVAHED_LST = null;

        private void ClearFreshAll()
        {
            NUMBER1.Text = "0"; //شماره فاکتور
            NUMBER1.Tag = null;
            NUMBER.Tag = null;

            NUMBER.SelectedValue = null; //شماره حواله
            NUMBER.Text = "0"; //شماره حواله

            DATE_N.Text = Tarikh.FullCurrentDate; //تاریخ
            USER_NAME.Text = Baseknow.UUSER; // نام کاربری
            SHIFT.SelectedValue = CL_Generaly.SHIFT_OF_USER; SHIFT.Items.Refresh();

            CUST_NO.SelectedIndex = -1; CUST_NO.Items.Refresh();

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
            TICMBAA.IsChecked = false; //مالیات ب.ا.ا
            JAY.IsChecked = false; //جایزه

            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
            MODAT_PPID.SelectedIndex = -1; MODAT_PPID.Items.Refresh(); //نحوه پرداخت
            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;


            PEPID.SelectedIndex = -1; //اعلامیه قیمت
            PEID.SelectedIndex = -1; //اعلامیه تخفیف

            MODAT_PPID_Enter();

            M_NAGHD2.Text = "0"; //مبلغ نقد
            MABL_VAR2.Text = "0"; // کارت بانک
            MABL_HAV2.Text = "0"; // حواله
            TAKHFIF2.Text = "0"; // مبلغ تخفیف

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

            MOIN_VAR2.Text = null; //معین کارت
            MOIN_HAV2.Text = null; //معین حواله

            MOGU.Text = null; //موجودی

            Text117.Text = "0"; //جمع مقادیر
            JJKOL.Text = "0"; //جمع فاکتور
            MN.Text = "0"; //مانده

            MANDAH.Text = null;
            MOLAH.Text = null;
            SHARAYET.Text = null;

            N_S.Text = "0"; //ثبت در سند
            MABNA.Text = "0"; //ثبت در سند

            //پشت فاکتور

            M_NAGHD.Text = "0"; //مبلغ نقد
            MABL_VAR.Text = "0"; //مبلغ واریزی

            MOIN_VAR.Text = null; //معین واریزی
            MABL_HAV.Text = "0"; //مبلغ حواله
            MOIN_HAV.Text = null; //معین حواله
            TAKHFIF.Text = "0"; //مبلغ تخفیف
            MABL_HAZ.Text = "0"; //مبلغ خدمات
            MOIN_HAZ.Text = null; //معین خدمات
            MBAA.Text = "0"; //مبلغ مالیات
            HMBAA.Text = null; //معین مالیات

            JF.Text = "0"; //جمع کل فاکتور
            HKH.Text = "0"; //هزینه خدمات
            NTKHFIF.Text = "0"; //تخفیفات
            GHABEL.Text = "0";//مبلغ قابل پرداخت
            NPAR.Text = "0"; //جمع مبالغ پرداختی
            MAN.Text = "0"; //مانده

            NCHK.Text = "0";

            //سایر

            Text190.Text = "0"; //جمع پورسانت
            REQUEST_NO.Text = "0"; //شماره درخواست کالا
            BARNAMEH.Text = "0"; //شماره بار نامه
            DRIVER.Text = null; //نام راننده
            DRIVER_MOB.Text = "0"; //موبایل راننده
            CAMIUN_NUM.Text = "0"; //شماره ماشین
            CAMIUN.Text = null; // نوع ماشین
            MAGHSAD.SelectedIndex = -1; //مقصدر بار
            CAM_KHALY.Text = "0"; //وزن ماشین خالی
            CAM_POOR.Text = "0"; // وزن ماشین پر
            TOZIH.Text = null; //توضیح

            FACTOR22_INVO_DATA?.Clear(); //دیتاگرید فاکتور فروش
            TAKHFIF_APLAY_DATA?.Clear(); //تخفیفات پیشرفته
            PAY_GETD_SUB22_DATA?.Clear(); //چک
            SAYER_VISITOR_DATA?.Clear(); //سایر

            GetHavaleh();

            Form_Current();

            AllowEdits = true;

            INVO_LST_sub.IsReadOnly = true;

            GetDefaultFocus();
        }

        UniversControl universControl = new UniversControl();

        /// <summary>
        /// Me.CODE.TAG برای اینکه مقدار قبل از اصلاح در دیتاگرید رو داشته باشیم
        /// </summary>
        public INVO_LST_FACTOR22 WAS_ROW_ITEM { get; set; } = new INVO_LST_FACTOR22();
        /// <summary>
        /// برای گرفتن آخرین ستونی که انتخاب شده بوده
        /// </summary>
        public int CURRENT_COLUMN_INDEX { get; set; }
        /// <summary>
        /// برای گرفتن آخرین سطری که انتخاب شده بوده
        /// </summary>
        public int CURRENT_ROW_INDEX { get; set; }
        /// <summary>
        /// آیتم های مربوط به سطر جاری در دیتاگرید که از این e.Row.Item as INVO_LST_FACTOR22 پر میشه CELL END EDIT
        /// </summary>
        public INVO_LST_FACTOR22? CURRENT_ROW_ITEMS { get; set; } = new INVO_LST_FACTOR22();
        public string NameOfCurrentColumn { get; set; }
        /// <summary>
        /// تک مقداری که توی سطر الان وارد کرده و بسته به اینکه کدام ستون در حال تغییر بوده فرق میکنه
        /// </summary>
        public object ENTERED_VALUE_ROW { get; set; }
        /// <summary>
        /// سلول جاری در حال اصلاح
        /// </summary>
        public DataGridCell CURRENT_CELL_ROW { get; set; }

        public INVO_LST_FACTOR22 FROM_SAERCH_KAL { get; set; } = new INVO_LST_FACTOR22();

        public FULL_HESAB HESAB_POSHTEF_FROM_SEARCH { get; set; } = new FULL_HESAB();

        //برای ردیابی اینکه آیا دیتاگرید رو سیو نکرده داره میبنده ؟ پیفرض فعاله
        public bool IS_SAVED { get; set; } = true;
        public nint WINDOW_ID { get; private set; }
        public Visual I_AM_FOROOSH22 { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private bool IsNull(object? hTAF2)
        {
            string _inputy = hTAF2.ToStringNullSafe();
            if (string.IsNullOrEmpty(_inputy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private double ttime;
        private bool custset;
        private bool OPRN;
        private double Meidnum;
        private bool firstopen;
        private bool PLUS;
        private bool flagt;
        /// <summary>
        /// NewRecord and for Open Factor with Number Exist
        /// </summary>
        public string? OpenArgs { get; set; } = null;
        public string ServerFilter { get; set; }


        private bool chek;
        private bool ISBAR;
        private bool NIM;
        private bool khaly;
        private bool meghone;
        private int CANCEL;
        private string BEFOREDATEN;


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
                AllowAdditionEdits(_ican);
            }
        }

        private bool _isheadok;
        private bool IsFocusInsideOfINVO_LST_sub;

        //private bool SmallHeadCheckOK()
        //{
        //    if (DEPATMAN.SelectedValue is null ||
        //              CUST_NO.SelectedValue is null ||
        //              CUST_NO2.SelectedValue is null ||
        //              SHIFT.SelectedValue is null ||
        //              MODAT_PPID.SelectedValue is null ||
        //              CUST_KIND.SelectedValue is null
        //             )
        //    {
        //        return false;
        //    }
        //    if (MODAT_PPID.SelectedIndex == 1)
        //    {
        //        if (Convert.ToInt32(MAS.Text) <= 0)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        //فیلد ها برای ذخیره مقدار قبل از تغییر به اسم تگ
        #region TAGHA
        public string DATE_N_TAG { get; set; }
        public int NUMBER_TAG { get; set; }
        public string CUST_NO_TAG { get; set; }
        public string MOLAH_TAG { get; set; }
        public bool runone { get; set; }
        public object Cancel { get; set; }
        public int TKHF { get; set; }
        public long CDDATE { get; set; }
        public long CDTIME { get; set; }
        public byte HTAG { get; set; } = 13;
        public bool LETSANAD { get; set; }
        public byte Dtag { get; set; } = 2;
        public int ANBARDefaultValue { get; set; }
        public bool NowIsReady { get; private set; }

        #endregion

        private int _name_code_index;
        public int NAME_CODE_INDEX_COL
        {
            get
            {
                if (INVO_LST_sub.Columns.Count > 0)
                {
                    int? defaultcolumnindex = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath is not null && c.SortMemberPath == "ANBAR")?.DisplayIndex;
                    if (defaultcolumnindex is null || defaultcolumnindex < 0)
                    {
                        _name_code_index = 0;
                    }
                    else
                    {
                        _name_code_index = (int)defaultcolumnindex;
                    }
                }
                return _name_code_index;
            }
            //set { _name_code_index_col = value; }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ChangeIsHappend = false;

            if (IsFromPishFactorConverted) //اگر از طریق دکمه تبدیل به فاکتوراز پیش فاکتور آمده
            {
                //جهت اطمینان از اضافه شدن حساب مالیات به پشت فاکتور
                CalculateIMBAA();
                JAYEHZAH(false);
                IsFromPishFactorConverted = false; //Reset to avoid ferther conflict
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeIsHappend)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }
            //MeTimer.IsEnabled = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WINDOW_ID = new WindowInteropHelper(this).Handle;
            I_AM_FOROOSH22 = CL_LMethods.GetTheWindow(WINDOW_ID);

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            SecurityAllCheck();
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            DATE_N.Text = Tarikh.FullCurrentDate;

            USER_NAME.Text = Baseknow.UUSER;

            #region On_LoadForm
            if (Baseknow.SANAD == 1 || Baseknow.UGRP == "6")
            {
                //SHRST.Open("deed_hed", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            }
            string VS;
            VS = "";
            #endregion

            #region On_OpenForm_Form_Open
            OPRN = true;
            int i;

            flagt = true;


            if (Strings.Mid(Baseknow.OPTIONSS, 10, 1) == "5")
            {
                if (Conversion.Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == 1)
                {
                    //this.CUST_NO.AutoExpand = false;
                    this.CUST_NO2.IsTabStop = false;
                }
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 29, 1) == "5" || Conversion.Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == 21 && Baseknow.UGRP == "3")
            {
                this.CUST_NO2.IsTabStop = false;
                this.MOLAH.IsTabStop = false;
            }
            else
            {
                this.CUST_NO2.IsTabStop = true;
                this.MOLAH.IsTabStop = true;
            }
            if (!CL_HESABDARI.LETSGO("CUSTEN"))
            {
                this.CUST_KIND.IsReadOnly = true;
            }
            else
            {
                this.CUST_KIND.IsReadOnly = false;
            }
            if (!CL_HESABDARI.LETSGO("BFAC"))
            {
                this.Page58.Visibility = Visibility.Hidden;
            }
            if (!CL_HESABDARI.LETSGO("ESLAH"))
            {
                this.ESLAH.Visibility = Visibility.Hidden;
            }
            else
            {
                this.ESLAH.Visibility = Visibility.Visible;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {
                this.JAY.Visibility = Visibility.Visible;
            }
            else
            {
                this.JAY.Visibility = Visibility.Hidden;
            }
            if (CL_HESABDARI.LETSGO("TFTMLOCK"))
            {
                this.TAKHFIF2.IsReadOnly = true;
                //this.Text163.IsReadOnly = true;
            }
            else
            {
                this.TAKHFIF2.IsReadOnly = false;
                //this.Text163.IsReadOnly = false;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 54, 1) == "5")
            {
                this.PRSS.Visibility = Visibility.Visible;
            }
            else
            {
                this.PRSS.Visibility = Visibility.Hidden;
            }
            if (!CL_HESABDARI.LETSGO("TKHPISH"))
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
                //this.ScrollBars = 0;
            }
            else
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Visible;
                //this.ScrollBars = 2;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 58, 1) != "5")
            {
                this.TAKHFIF_APLAY_SUB.Visibility = Visibility.Hidden;
                //this.ScrollBars = 0;
            }

            this.flagt = true;
            OPRN = false;
            if (Baseknow.SIGN ?? false)
            {
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
                this.SGN1usid.Visibility = Visibility.Visible;
                this.SGN2usid.Visibility = Visibility.Visible;
                this.SGN3usid.Visibility = Visibility.Visible;

            }
            if (Baseknow.GHAYM == 7)
            {
                MODAT_PPID.Visibility = Visibility.Visible;
                PEPID.Visibility = Visibility.Visible;
                PEID.Visibility = Visibility.Visible;
            }
            else
            {
                MODAT_PPID.Visibility = Visibility.Hidden;
                PEPID.Visibility = Visibility.Hidden;
                PEID.Visibility = Visibility.Hidden;
            }
            #endregion

            FILL_ALL_COMBOBOXES();

            OnOpen_SUB();

            if (TAKHFIF_APLAY_SUB.Visibility == Visibility.Visible)
            {
                var ROW_SOURCE = dbms.DoGetDataSQL<TAKHFIF_DEF>("SELECT TID, TSHARH FROM TAKHFIF_DEF").ToList();
                Combo6Column.ItemsSource = ROW_SOURCE;

                CL_HESABDARI.SETSECURITYSUB(TAKHFIF_APLAY_SUB, "TKHPISH");

                var DATA = dbms.DoGetDataSQL<TAKHFIF_APLAY>("SELECT TID, NUMBER, KIND FROM TAKHFIF_APLAY").ToList();
            }

            //const string REPLACEMENT_VALUE = "dbo.HEAD_LST.";
            //var InvoiceWheres = CL_LMethods.GetRestrictedSqlQuery(fTAG, $" WHERE TAG = {fTAG} ").Replace(REPLACEMENT_VALUE, null);

            //this.UpdateLayout();

            string WhereCondition = fTAG > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {fTAG}) " : "  ";
            _restrictionInfo = CL_LMethods.GetRestrictedSqlQueryWithDetails(fTAG, WhereCondition);
            WhereCondition = _restrictionInfo.WhereClause;

            if (IsOpenedFromAutomation) //اگر از اتوماسیون اداری باز شده فقط همین شماره رو باز کنه
            {
                WhereCondition = $" WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG} ";
            }

            //$"SELECT NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID, sgn1usid, sgn2usid, sgn3usid, CRT, UID, ARZKIND2, ARZCODING FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table

            _navigationManager = new NavigationManager<HEAD_LST>(
                dbms,
                x => x.NUMBER.ToString(), // property selector (used to find a record by its CODE)
                $"SELECT NUMBER1 , NUMBER , DATE_N , CUST_NO , USER_NAME , MOLAH FROM HEAD_LST {WhereCondition} ORDER BY NUMBER", //All Record of The Table
                x => $"SELECT TOP 1 NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME, SHARAYET, SGN1, SGN2, SGN3, SGN4, MBAA, HMBAA, TAMIR, TICMBAA, TKHF, OKF, SADER, ARZD, ARZKIND, CDDATE, CDTIME, OKDATE, OKTIME, JAY, MODAT_PPID, PEPID, PEID, sgn1usid, sgn2usid, sgn3usid, CRT, UID, ARZKIND2, ARZCODING FROM HEAD_LST WHERE NUMBER = {x?.NUMBER} AND TAG = {fTAG}", //On Change for One Record
                Convert.ToDouble(NUMBER.Text)
                );

            if (!IsOpenedFromAutomation && !string.IsNullOrEmpty(OpenArgs) && _navigationManager.NUMBER_TO_OPEN != null) //Had a paramter passed
            {
                //یعنی این شماره رو پیدا نکرده که اون رو ریست کنه
                new Msgwin(false, $"شما به شماره فاکتور (حواله) {_navigationManager.NUMBER_TO_OPEN} دسترسی ندارید ").Show();
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


            Form_Current();

            if (IsDirectFactor) //مستقیم : حواله فاکتور سینک
            {
                CL_LMethods.SetTabIndexes(
                   CUST_NO,
                   CUST_KIND,
                   DEPATMAN,
                   MOLAH,
                   PEPID, /*اعلامیه قیمت*/
                   PEID, /*اعلامیه تخفیف*/
                   MODAT_PPID, /*نحوع پرداخت*/
                   BUTTON_SAVE_HAVALE,
                   INVO_LST_sub,
                   MABL_VAR2,
                   CMB_MOIN_VAR2,
                   MABL_HAV2,
                   CMB_MOIN_HAV2,
                   M_NAGHD,
                   MABL_VAR,
                   CMB_MOIN_VAR, MABL_HAV,
                   CMB_MOIN_HAV
                   );
            }
            else
            {
                CL_LMethods.SetTabIndexes(
                   NUMBER,
                   CUST_KIND,
                   MOLAH,
                   PEPID, /*اعلامیه قیمت*/
                   PEID, /*اعلامیه تخفیف*/
                   MODAT_PPID, /*نحوع پرداخت*/
                   BUTTON_SAVE_HAVALE,
                   INVO_LST_sub,
                   MABL_VAR2,
                   CMB_MOIN_VAR2,
                   MABL_HAV2,
                   CMB_MOIN_HAV2,
                   M_NAGHD,
                   MABL_VAR,
                   CMB_MOIN_VAR, MABL_HAV,
                   CMB_MOIN_HAV
                   );
            }


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
                new SearchableProperty { DisplayName = "شماره فاکتور", PropertyPath = "NUMBER1", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "شماره حواله", PropertyPath = "NUMBER", PropertyType = typeof(double) },
                new SearchableProperty { DisplayName = "تاریخ", PropertyPath = "DATE_N", PropertyType = typeof(long) },
                new SearchableProperty { DisplayName = "کد مشتری", PropertyPath = "CUST_NO", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "کاربر", PropertyPath = "USER_NAME", PropertyType = typeof(string) },
                new SearchableProperty { DisplayName = "ملاحظات", PropertyPath = "MOLAH", PropertyType = typeof(string) },
                // Add other searchable properties
            };
        }
        #endregion
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (SHARAYET.IsFocused || SHARAYET.IsKeyboardFocusWithin || BUTTON_SAVE_HAVALE.IsFocused)
                    {
                        //continue out
                    }
                    else
                    {
                        e.Handled = true;
                        if (INVO_LST_sub != null && INVO_LST_sub.IsKeyboardFocusWithin)
                        {
                            ///////در این روش کلید تب عمل میکند و حالت فوکوس روی حالت در حال ادیت هست
                            //if (INVO_LST_sub.SelectedIndex == INVO_LST_sub.Items.Count - 2 && lastColumn != null && INVO_LST_sub.CurrentColumn.DisplayIndex == lastColumn.DisplayIndex)

                            if (INVO_LST_sub.SelectedIndex == INVO_LST_sub.Items.Count - 2 && INVO_LST_sub.CurrentColumn?.DisplayIndex == CL_LMethods.GetLastColumn(INVO_LST_sub)?.DisplayIndex)
                            {
                                INVO_LST_sub.SelectedIndex = INVO_LST_sub.Items.Count - 1;
                                INVO_LST_sub.CurrentCell = new DataGridCellInfo(INVO_LST_sub.SelectedItem, INVO_LST_sub.Columns[NAME_CODE_INDEX_COL]);
                                INVO_LST_sub.BeginEdit();

                                //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                if (focusedWindow != null)
                                {
                                    focusedWindow.Activate();
                                    focusedWindow.Focus();
                                }

                                return;
                            }
                        }
                        else if (PAY_GETD_SUB22 != null && PAY_GETD_SUB22.IsKeyboardFocusWithin)
                        {
                            ///////در این روش کلید تب عمل میکند و حالت فوکوس روی حالت در حال ادیت هست
                            if (PAY_GETD_SUB22.SelectedIndex == PAY_GETD_SUB22.Items.Count - 2 && PAY_GETD_SUB22.CurrentColumn?.DisplayIndex == CL_LMethods.GetLastColumn(PAY_GETD_SUB22)?.DisplayIndex)
                            {
                                PAY_GETD_SUB22.SelectedIndex = PAY_GETD_SUB22.Items.Count - 1;
                                PAY_GETD_SUB22.CurrentCell = new DataGridCellInfo(PAY_GETD_SUB22.SelectedItem, PAY_GETD_SUB22.Columns[1]);
                                PAY_GETD_SUB22.BeginEdit();

                                //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                if (focusedWindow != null)
                                {
                                    focusedWindow.Activate();
                                    focusedWindow.Focus();
                                }

                                return;
                            }
                        }
                        else if (VISITOR_DTL_SUB != null && VISITOR_DTL_SUB.IsKeyboardFocusWithin)
                        {
                            ///////در این روش کلید تب عمل میکند و حالت فوکوس روی حالت در حال ادیت هست
                            if (VISITOR_DTL_SUB.SelectedIndex == VISITOR_DTL_SUB.Items.Count - 2 && VISITOR_DTL_SUB.CurrentColumn?.DisplayIndex == CL_LMethods.GetLastColumn(VISITOR_DTL_SUB)?.DisplayIndex)
                            {
                                VISITOR_DTL_SUB.SelectedIndex = VISITOR_DTL_SUB.Items.Count - 1;
                                VISITOR_DTL_SUB.CurrentCell = new DataGridCellInfo(VISITOR_DTL_SUB.SelectedItem, VISITOR_DTL_SUB.Columns[0]);
                                VISITOR_DTL_SUB.BeginEdit();

                                //تو فوکوس روی پنجره پیام باشه , برای راحتی با اینتر
                                var focusedWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                                if (focusedWindow != null)
                                {
                                    focusedWindow.Activate();
                                    focusedWindow.Focus();
                                }
                                return;
                            }
                        }
                        CL_LMethods.SendKey_US(Key.Tab);
                    }
                }
            }
            catch { /*ignore*/ }


            if (INVO_LST_sub != null && !INVO_LST_sub.IsKeyboardFocusWithin && !INVO_LST_sub.IsFocused) //Only On Form F7 Pressed Not DataGrid
            {
                if (e.Key == Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    e.Handled = true;
                    var searchWindow = new EnhancedSearchWindow(this);
                    searchWindow.Owner = this;
                    searchWindow.ShowDialog();
                }
            }

            // Check if the pressed key is 'G'
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G) //Just another method
            {
                e.Handled = true; //Mark the event as handled to prevent further processing

                if (Convert.ToDouble(NUMBER1.Text) > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از باز کردن پنجره سایر اطلاعات مطمئن هستید؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null);

                        if (SavedSuccessBtn)
                        {
                            OTHER_DTL win = new OTHER_DTL(1, CL_LMethods.GetTheWindow(WINDOW_ID));
                            win.NUMBER = Convert.ToInt64(NUMBER.Text);
                            win.Show();
                        }
                    }
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T) //Ctrl + K
            {
                e.Handled = true;

                if (NUMBER.Text != "0" && NUMBER.Text != null) //Saved Before
                {
                    if (SavedSuccessBtn)
                    {
                        TAKHFIF takhfif = new TAKHFIF(I_AM_FOROOSH22);
                        takhfif.ShowDialog();
                    }
                }
            }
            else
            {
                //if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F8 || e.SystemKey == Key.F8))
                //{
                //    e.Handled = true;
                //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
                //}
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
            bool recordExists = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 NUMBER FROM HEAD_LST WHERE NUMBER = {requestedNumber} AND TAG = {fTAG}").FirstOrDefault() != null;
            string message = recordExists ? GetAccessDeniedMessage() : "چنین شماره ای وجود ندارد";
            new Msgwin(false, message).ShowDialog();
            _navigationManager.ClearNumberToOpen();
        }
        private async void OnCurrentRecordChanged(HEAD_LST HEADER_FAC)
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
                try
                {
                    var fullDetails = await GetFactorFullDetailsAsync(HEADER_FAC.NUMBER);

                    if (fullDetails == null || fullDetails.Header == null)
                    {
                        new Msgwin(false, "این فاکتور خالی است یا اطلاعات کامل آن یافت نشد.").Show();
                        return;
                    }

                    // --- Begin Populating UI from fullDetails model ---
                    var header = fullDetails.Header;

                    NUMBER1.Text = header.NUMBER1.ToString();
                    NUMBER.Text = header.NUMBER.ToString();

                    var numberItemsSource = (List<_MG_MODEL2_>)NUMBER.ItemsSource ?? new List<_MG_MODEL2_>();
                    if (!numberItemsSource.Any(item => item?.NUMBER == header.NUMBER))
                    {
                        numberItemsSource.Add(new _MG_MODEL2_ { NUMBER = header.NUMBER });
                        NUMBER.ItemsSource = numberItemsSource;
                    }
                    NUMBER.SelectedValue = header.NUMBER;
                    NUMBER.Items.Refresh();

                    DATE_N.Text = header.DATE_N.ToStringNullSafe();
                    USER_NAME.Text = header.USER_NAME.ToStringNullSafe();
                    MAS.Text = header.MAS.ToStringNullSafe();
                    DEPATMAN.SelectedValue = header.DEPATMAN;
                    CUST_KIND.SelectedValue = header.CUST_KIND;
                    if (header.FNUMCO != null)
                    {
                        FNUMCO.Text = header.FNUMCO.ToStringNullSafe();
                    }

                    if (IsExporty)
                    {
                        ARZD.Text = header.ARZD.ToStringNullSafe();
                        ARZKIND2.SelectedValue = header.ARZKIND2;
                        ANBARF.Text = header.ANBARF.ToStringNullSafe();
                    }

                    if (fullDetails.Customer != null)
                    {
                        var custItemsSource = CUST_NO.ItemsSource as List<Custom_CUST_HESAB> ?? new List<Custom_CUST_HESAB>();
                        if (!custItemsSource.Any(item => item?.hes == fullDetails.Customer.hes))
                        {
                            custItemsSource.Add(new Custom_CUST_HESAB { hes = fullDetails.Customer.hes, NAME = fullDetails.Customer.NAME });
                            CUST_NO.ItemsSource = custItemsSource;
                        }
                        CUST_NO.SelectedValue = fullDetails.Customer.hes;
                        CUST_NO.Items.Refresh();
                    }

                    SGN1.IsChecked = header.SGN1;
                    SGN2.IsChecked = header.SGN2;
                    SGN3.IsChecked = header.SGN3;
                    SGN1usid.Tag = header.sgn1usid;
                    SGN2usid.Tag = header.sgn2usid;
                    SGN3usid.Tag = header.sgn3usid;

                    PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                    PERSONEL.SelectedValue = null;
                    PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;

                    if (rst_personel != null)
                    {
                        SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == header.sgn1usid)?.SAL_NAME;
                        SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == header.sgn2usid)?.SAL_NAME;
                        SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == header.sgn3usid)?.SAL_NAME;
                    }

                    FACTOR22_INVO_DATA.Clear();
                    fullDetails.InvoiceItems.ForEach(FACTOR22_INVO_DATA.Add);
                    TAKHFIF_APLAY_DATA.Clear();
                    fullDetails.AdvancedDiscounts.ForEach(TAKHFIF_APLAY_DATA.Add);
                    PAY_GETD_SUB22_DATA.Clear();
                    fullDetails.Checks.ForEach(PAY_GETD_SUB22_DATA.Add);
                    SAYER_VISITOR_DATA.Clear();
                    fullDetails.VisitorDetails.ForEach(SAYER_VISITOR_DATA.Add);

                    OKF.IsChecked = header.OKF ?? false;
                    if (header.OKF == null || header.OKF == false) MakeOKFReady();

                    TICMBAA.IsChecked = header.TICMBAA;
                    JAY.IsChecked = header.JAY;
                    if (!string.IsNullOrWhiteSpace(header.MOLAH))
                    {
                        MOLAH.Text = header.MOLAH;
                    }
                    SHIFT.SelectedValue = header.SHIFT;

                    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                    MODAT_PPID.SelectedValue = header.MODAT_PPID;
                    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

                    PEPID.SelectedValue = header.PEPID;
                    PEID.SelectedValue = header.PEID;

                    MODAT_PPID_Enter();

                    M_NAGHD.Text = header.M_NAGHD.ToStringNullSafe();
                    MABL_VAR.Text = header.MABL_VAR.ToStringNullSafe();
                    MABL_HAV.Text = header.MABL_HAV.ToStringNullSafe();
                    TAKHFIF.Text = header.TAKHFIF.ToStringNullSafe();
                    MOIN_VAR.Text = header.MOIN_VAR.ToStringNullSafe();
                    MOIN_HAV.Text = header.MOIN_HAV.ToStringNullSafe();
                    SHARAYET.Text = header.SHARAYET;

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

                    // Recalculate sums and totals
                    MasterSummerAndMandeh();

                    MANDAH.Text = fullDetails.AccountBalance;
                    N_S.Text = header.N_S.ToStringNullSafe();
                    MABNA.Text = fullDetails.SanadBase;

                    if (fullDetails.TaxDetails != null)
                    {
                        var moadian = fullDetails.TaxDetails;
                        inty.SelectedValue = moadian.inty;
                        inp.SelectedValue = moadian.inp;
                        ins.SelectedValue = moadian.ins;
                        sbc.Text = moadian.sbc;
                        bbc.Text = moadian.bbc;
                        ft.Text = moadian.ft.ToStringNullSafe();
                        bpn.Text = moadian.bpn;
                        scln.Text = moadian.scln;
                        scc.Text = moadian.scc;
                        cdcn.Text = moadian.cdcn;
                        cdcd.Text = moadian.cdcd.ToStringNullSafe();
                        crn.Text = moadian.crn;
                        billid.Text = moadian.billid;
                        todam.Text = moadian.todam.ToStringNullSafe();
                        tonw.Text = moadian.tonw.ToStringNullSafe();
                        torv.Text = moadian.torv.ToStringNullSafe();
                        tocv.Text = moadian.tocv.ToStringNullSafe();
                        setm.SelectedValue = moadian.setm;
                        cap.Text = moadian.cap.ToStringNullSafe();
                        insp.Text = moadian.insp.ToStringNullSafe();
                        tvop.Text = moadian.tvop.ToStringNullSafe();
                        tax17.Text = moadian.tax17.ToStringNullSafe();
                        if (IsExporty) CUT.SelectedValue = moadian.cut;
                        irtaxid.Text = moadian.irtaxid;
                    }

                    // --- End Populating UI ---

                    Form_Current();
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

        }
        private bool OnInsertRecord(HEAD_LST record)
        {
            try
            {
                var itemtoadd = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 * FROM HEAD_LST  WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG}").FirstOrDefault();
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
            //NewRecord = false;

            var CURRENT_HEADER = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG}").FirstOrDefault();
            _navigationManager.InsertCurrentRecord(CURRENT_HEADER);
        }

        private void SecurityAllCheck()
        {
            if (IsDirectFactor)
            {
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "FACTFRMO", WINDOW_ID, this.GetType().Name);

                CL_HESABDARI.SETSECURITYSUB(INVO_LST_sub, "FACTFRMO");
            }
            else
            {
                //فاکتور غیر مستقیم
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "FACTFR", WINDOW_ID, this.GetType().Name);
                CL_HESABDARI.SETSECURITYSUB(INVO_LST_sub, "FACTFR");
            }

            CL_HESABDARI.SETSECURITYSUB(PAY_GETD_SUB22, "FACTFR");
            CL_HESABDARI.SETSECURITYSUB(TAKHFIF_APLAY_SUB, "TKHPISH");

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
        }

        /// <summary>
        /// (NUMBER1.Text) old
        /// </summary>

        private bool newrecord = false;
        public bool NewRecord
        {
            get
            {
                newrecord = _navigationManager.IsNewRecord;
                return newrecord;
            }
            //set
            //{
            //    newrecord = value;
            //}
        }

        //private bool NewRecord(object SSS)
        //{
        //    if (SSS is null || Convert.ToDouble(SSS) <= 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// بازنگری مقادیر و ثبت سند
        /// </summary>

        public void ANBAR_LOADITEM()
        {
            ////ALL ANBARS:
            //var ARST = dbms.DoGetDataSQL<Custom_TCODANBAR>("SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM TCOD_ANBAR ORDER BY TCOD_ANBAR.CODE").ToList();
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
            //ANBAR_COL.EditingElementStyle.
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //نوع مشتری
            CUST_KIND.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND.DisplayMemberPath = "CUSTKNAME";
            CUST_KIND.SelectedValuePath = "CUST_COD";
            CUST_KIND.SelectedIndex = 0;

            //نام مشتریان
            //try
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>(@"SELECT hes,NAME FROM CUST_HESAB").ToList();
            //}
            //catch (Exception)
            //{
            //    CUST_NO.ItemsSource = dbms.DoGetDataSQL<Custom_CUST_HESAB>("SELECT hes, NAME  FROM CUST_HESAB OPTION (ORDER GROUP, FAST 1)").ToList();
            //}


            //CUST_NO.ItemsSource = new List<Custom_CUST_HESAB> { new Custom_CUST_HESAB() };
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
            DEPATMAN.ItemsSource = RST; DEPATMAN.DisplayMemberPath = "DEPNAME";
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

            //نحوه پرداخت و مدت
            //MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO UNION SELECT 0, 'آزاد', 0").ToList();
            GET_MODAT_PPID_SOURCE();

            //اعلامیه قیمت
            PEPID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_CSHARP>("SELECT PEPID, PEPNAME, PEPDATE, PEPDEPART FROM PRICE_ELAMIE ORDER BY PEPNAME DESC").ToList();
            PEPID.DisplayMemberPath = "PEPNAME";
            PEPID.SelectedValuePath = "PEPID";

            //اعلامیه تخفیف
            PEID.ItemsSource = dbms.DoGetDataSQL<PRICELIST_ETF_TAKHFIF__CSHARP>("SELECT PEID, PENAME, PEDATE, PEPDEPART FROM PRICE_ELAMIETF").ToList();
            PEID.DisplayMemberPath = "PENAME";
            PEID.SelectedValuePath = "PEID";



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

            vAZColumn.ItemsSource = new List<VAZ_MODEL_CHECK>()
            {
                 new VAZ_MODEL_CHECK { VAZ = 1, NAME_VAZ = "نزد صندوق" },
                 new VAZ_MODEL_CHECK { VAZ = 2, NAME_VAZ = "نزد بانك" },
                 new VAZ_MODEL_CHECK { VAZ = 3, NAME_VAZ = "وصول شده" },
                 new VAZ_MODEL_CHECK { VAZ = 4, NAME_VAZ = "واگذار شده" },
                 new VAZ_MODEL_CHECK { VAZ = 5, NAME_VAZ = "برگشت شده" },
                 new VAZ_MODEL_CHECK { VAZ = 6, NAME_VAZ = "مسترد شده" }
            };

            //کمبوباکس های پشت فاکتور
            bANKColumn.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS ORDER BY TCOD_BANKS.NAMES").ToList();

            var HESNAMELST = dbms.DoGetDataSQL<CUSTOM_HESABHA>("SELECT N_KOL,NUMBER,TNUMBER, RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) AS hes, NAME FROM TDETA_HES").ToList();
            CMB_MOIN_VAR.ItemsSource = HESNAMELST.Where(w => w.N_KOL == Baseknow.BANKHA).ToList(); //معین واریزی
            CMB_MOIN_HAV.ItemsSource = HESNAMELST.ToList(); //معين حواله
            CMB_MOIN_HAZ.ItemsSource = HESNAMELST.ToList(); //معين خدمات
            CMB_HMBAA.ItemsSource = HESNAMELST.ToList(); //معین مالیات

            CMB_MOIN_VAR2.ItemsSource = CMB_MOIN_VAR.ItemsSource;
            CMB_MOIN_HAV2.ItemsSource = CMB_MOIN_HAV.ItemsSource;

            //دریافت چک:
            //به حساب کل
            n_KOLColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>("SELECT     NUMBER, NAME FROM TOTA_HES WHERE (NUMBER = " + Baseknow.BANKHA + ")ORDER BY NAME").ToList();
            //Giving All Data as Master:
            //معین بانک
            n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {Baseknow.BANKHA})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //تفضیلی
            n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.N_KOL) ={Baseknow.BANKHA}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME\r\n").ToList();


            //موقعیت چک
            sANDUGHColumn.ItemsSource = dbms.DoGetDataSQL<TDETA_HES_CHECK>("SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = " + CL_HESABDARI.GETKOL(Baseknow.ADA) + ") AND (NUMBER = 1)").ToList();

            #endregion


            //الگوی پورسانت:
            PORID_COLUMN.ItemsSource = dbms.DoGetDataSQL<PORD_COL_MODEL>("SELECT VISITORS_PORSANT.PORID, CAST(VISITORS_PORSANT.PORID AS nvarchar) + N' - ' + CAST(VISITORS_PORSANT.VDATE AS nvarchar) + N' - ' + ISNULL(CUSTKIND.CUSTKNAME, N'بدون گروه (همه)') + N' - ' + ISNULL(VISITORS_PORSANT.COMMENT, N' ') + N' - ' + CUST_HESAB.NAME AS Expr1 FROM VISITORS_PORSANT INNER JOIN CUST_HESAB ON VISITORS_PORSANT.HES = CUST_HESAB.hes LEFT OUTER JOIN CUSTKIND ON VISITORS_PORSANT.CUST_COD = CUSTKIND.CUST_COD").ToList();

            //اطلاعات بارنامه
            MAGHSAD.ItemsSource = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM dbo.TCOD_CITY").ToList();

            if (IsExporty)
            {
                ARZKIND2.ItemsSource = dbms.DoGetDataSQL<TCOD_ARZ>($"SELECT ID,Code, Title, ISOCode, (ISOCode+N' - '+Title+N' - '+CountryName) AS ARZCOUNTRY, CRT, UID FROM dbo.[TCOD_ARZ]").ToList();

                //نوع ارز در مودیان
                CUT.ItemsSource = ARZKIND2.ItemsSource;
            }

            GetHavaleh();

            #region MOADIAN_COMBOBOXES

            //نوع صورتحساب:
            inty.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "نوع اول" },
                new COMBOYMODEL { ID = 2, NAME = "نوع دوم" },
                new COMBOYMODEL { ID = 3, NAME = "نوع سوم" }
            }; inty.SelectedValue = 1; inty.Items.Refresh();

            //الگوی صورتحساب:
            inp.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "فروش" },
                new COMBOYMODEL { ID = 2, NAME = "فروش ارزی" },
                new COMBOYMODEL { ID = 3, NAME = "طلاوجواهر" },
                new COMBOYMODEL { ID = 4, NAME = "پیمانکاری" },
                new COMBOYMODEL { ID = 5, NAME = "قبوض خدماتی" },
                new COMBOYMODEL { ID = 6, NAME = "بلیط هواپیما" },
                new COMBOYMODEL { ID = 7, NAME = "صادرات" }
            }; inp.SelectedValue = 1; inp.Items.Refresh();

            //الگوی صورتحساب:
            ins.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "اصلی" },
                new COMBOYMODEL { ID = 2, NAME = "اصلاحی" },
                new COMBOYMODEL { ID = 3, NAME = "ابطالی" },
                new COMBOYMODEL { ID = 4, NAME = "برگشت فروش" }
            }; ins.SelectedValue = 1; ins.Items.Refresh();

            //روش تسویه:
            setm.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "نقد" },
                new COMBOYMODEL { ID = 2, NAME = "نسیه" },
                new COMBOYMODEL { ID = 3, NAME = "نقد/نسیه" }
            }; setm.SelectedValue = 2; setm.Items.Refresh();

            #endregion


        }

        private void GET_MODAT_PPID_SOURCE()
        {
            MODAT_PPID.ItemsSource = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";
        }

        private void GetHavaleh()
        {
            if (!IsDirectFactor)
            {
                if (IsExporty)
                {
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<_MG_MODEL2_>("SELECT NUMBER FROM dbo.HEAD_LST WHERE(TAG=2)AND(NOT(NUMBER IN(SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE(((HEAD_LST.TAG)=13)))))AND(SADER=1 OR SADER IS NULL) ORDER BY NUMBER DESC").ToList();
                }
                else
                {
                    NUMBER.ItemsSource = dbms.DoGetDataSQL<_MG_MODEL2_>("SELECT NUMBER FROM dbo.HEAD_LST WHERE(TAG=2)AND(NOT(NUMBER IN(SELECT HEAD_LST.NUMBER FROM HEAD_LST WHERE(((HEAD_LST.TAG)=13)))))AND(SADER=0 OR SADER IS NULL) ORDER BY NUMBER DESC").ToList();
                }
            }
        }

        private void Form_Current()
        {
            {
                if (!CL_HESABDARI.LETSGO("elamghe")) //نمیتواند اعلامیه قیمت را اصلاح کند
                {
                    this.PEPID.IsEnabled = false; //Locked = true;
                    this.PEID.IsEnabled = false; //Locked = true;
                }
                else
                {
                    this.PEPID.IsEnabled = true; //Locked = false;
                    this.PEID.IsEnabled = true; //Locked = false;
                }

                var ghat = default(bool);
                if (Baseknow.TKHF == 1)
                {
                    this.TAKHFIF.IsReadOnly = false;
                    this.TAKHFIF2.IsReadOnly = false;

                    IF_AZAD_THENLOCK();
                }
                else if (Baseknow.TKHF == 2)
                {
                    this.TAKHFIF.IsReadOnly = true;

                    N_KOL_COLUMN.IsReadOnly = true;
                    N_MOIN_COLUMN.IsReadOnly = true;
                }
                else
                {
                    this.TAKHFIF.IsReadOnly = true;

                    IF_AZAD_THENLOCK();
                }

                if (this.TICMBAA.IsChecked is false)
                {
                    this.HMBAA.IsReadOnly = false;
                }
                else
                {
                    this.MBAA.IsReadOnly = true;
                }

                if (string.IsNullOrEmpty(this.N_S.Text))
                {
                    TAKHFIF_APLAY_ReGetData(); //#Error
                    AllowAdditionEdits(true);
                    AllowDeletions = true;
                    this.INVO_LST_sub.IsReadOnly = false;
                    this.TAKHFIF_APLAY_SUB.IsEnabled = false;
                    this.INVO_LST_sub.IsReadOnly = false;
                    //New Code // this.Page58.IsEnabled = true;
                    //New Code // this.Page155.IsEnabled = true;
                    //this.lsanad.ForeColor = 65535;
                    lsanad.Foreground = Brushes.White;

                }
                else
                {
                    var rst = dbms.DoGetDataSQL<DEED_HED>($"SELECT N_S, DATE_S, SHARH_S, NO_S, ANBAR, N_FACTOR, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, CRT, UID FROM dbo.DEED_HED WHERE N_S = {N_S.Text}").ToList();
                    if (rst.Count == 0)
                    { }
                    else if (rst.FirstOrDefault().GHATEI)
                    {
                        ghat = true;
                        AllowDeletions = false;
                        this.AllowEdits = false;
                        this.INVO_LST_sub.IsReadOnly = true;
                        //this.INVO_LST_sub.CanUserDeleteRows = false;
                        this.TAKHFIF_APLAY_SUB.IsEnabled = false;
                        //New Code // this.Page58.IsEnabled = false;
                        //New Code // this.Page155.IsEnabled = false;
                        //this.lsanad.ForeColor = 125;
                        this.lsanad.Foreground = new SolidColorBrush(Color.FromRgb(0x7D, 0x7D, 0x7D));
                    }
                    else
                    {
                        ghat = false;
                        AllowDeletions = true;

                        this.AllowEdits = true;
                        this.INVO_LST_sub.IsReadOnly = false;
                        this.INVO_LST_sub.IsReadOnly = false;
                        this.TAKHFIF_APLAY_SUB.IsEnabled = true;
                        //New Code // this.Page58.IsEnabled = true;
                        //New Code // this.Page155.IsEnabled = true;
                        lsanad.Foreground = Brushes.White;
                        //this.INVO_LST_sub.CanUserDeleteRows = true;
                    }
                }

                if (Baseknow.MAND)
                {
                    if (!IsNull(CUST_NO.SelectedValue))
                    {
                        if (!CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue.ToString()))
                        {
                            if (CUST_NO.SelectedValue != null)
                            {
                                MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                            }
                        }
                    }
                }

                if (_navigationManager.IsNewRecord) //this.NewRecord
                {
                    //New Code // this.Page58.IsEnabled = false;
                    //New Code // this.Page155.IsEnabled = false;
                    //this.INVO_LST_sub.IsReadOnly = true;
                    this.TAKHFIF_APLAY_SUB.IsEnabled = false;
                    this.DATE_N_TAG = "";
                    this.NUMBER_TAG = 0;
                    this.CUST_NO_TAG = "0";
                    this.MOLAH_TAG = "";
                    this.CUST_NO.Focus();

                    BUTTON_SAVE_HAVALE.IsEnabled = true;
                    //SGN1.IsEnabled = true;
                    //SGN2.IsEnabled = true;
                    //SGN3.IsEnabled = true;
                }
                else
                {
                    BUTTON_SAVE_HAVALE.IsEnabled = false;
                    //SGN1.IsEnabled = false;
                    //SGN2.IsEnabled = false;
                    //SGN3.IsEnabled = false;

                    if (!ghat)
                    {
                        this.INVO_LST_sub.IsReadOnly = false;
                        this.INVO_LST_sub.IsReadOnly = false;
                        this.TAKHFIF_APLAY_SUB.IsEnabled = true;
                        // Me![INVO_LST_sub].Form.Refresh
                        //New Code // this.Page58.IsEnabled = true;
                        //New Code // this.Page155.IsEnabled = true;

                    }
                    else
                    {
                        //New Code // this.Page58.IsEnabled = false;
                        //New Code // this.Page155.IsEnabled = false;
                        // Me![INVO_LST_sub].Form.Refresh
                        this.INVO_LST_sub.IsReadOnly = true;
                        this.TAKHFIF_APLAY_SUB.IsEnabled = false;
                    }
                    this.DATE_N_TAG = this.DATE_N.Text.ToRawTarikh();
                    this.NUMBER_TAG = Convert.ToInt32(this.NUMBER.Text);
                    this.CUST_NO_TAG = CUST_NO.SelectedValue?.ToString();
                    this.MOLAH_TAG = (string)Interaction.IIf(IsNull(this.MOLAH.Text), "", this.MOLAH.Text);
                }

                if ((bool)OKF.IsChecked && !NewRecord) //CheckOut
                {
                    AllowDeletions = false;
                    this.AllowEdits = false;
                    this.INVO_LST_sub.IsReadOnly = true;
                    this.TAKHFIF_APLAY_SUB.IsEnabled = false;
                    //New Code // this.Page58.IsEnabled = false;
                    //New Code // this.Page155.IsEnabled = false;
                    // Me.ESLAH.IsEnabled = True
                }
            }

            if (Baseknow.SIGN ?? false)
            {
                if (Convert.ToBoolean(SGN1.IsChecked) || Convert.ToBoolean(SGN2.IsChecked) || Convert.ToBoolean(SGN3.IsChecked))
                {
                    this.Command100.IsEnabled = true;
                    this.Command120.IsEnabled = true;
                    this.custprint.IsEnabled = true;
                    this.PRSS.IsEnabled = true;
                    this.Command113.IsEnabled = true;
                    Command139.IsEnabled = true;
                    Command170.IsEnabled = true;
                }
                else
                {
                    this.Command100.IsEnabled = false;
                    this.Command120.IsEnabled = false;
                    this.custprint.IsEnabled = false;
                    this.PRSS.IsEnabled = false;
                    this.Command113.IsEnabled = false;

                    Command139.IsEnabled = false;
                    Command170.IsEnabled = false;
                }
                this.SGN1.Visibility = Visibility.Visible;
                this.SGN2.Visibility = Visibility.Visible;
                this.SGN3.Visibility = Visibility.Visible;
                var rst = dbms.DoGetDataSQL<SGNS_CSHARP>("SELECT FFR_FROOSH,FFR_HESAB,FFR_MODIR FROM dbo.SIGN WHERE USERCO = " + Baseknow.USERCOD).FirstOrDefault();
                if (!(rst is null) && NewRecord)
                {
                    if ((bool)rst.FFR_FROOSH)
                        this.SGN1.IsEnabled = true;
                    else
                        this.SGN1.IsEnabled = false;
                    if ((bool)rst.FFR_HESAB)
                        this.SGN2.IsEnabled = true;
                    else
                        this.SGN2.IsEnabled = false;
                    if ((bool)rst.FFR_MODIR)
                        this.SGN3.IsEnabled = true;
                    else
                        this.SGN3.IsEnabled = false;
                }
                this.NUMBER.IsReadOnly = false;
                this.DATE_N.IsReadOnly = false;
                this.MAS.IsReadOnly = false;
                this.FNUMCO.IsReadOnly = false;
                this.SHIFT.IsReadOnly = false;
                this.CUST_KIND.IsReadOnly = false;
                this.CUST_NO.IsReadOnly = false;
                this.CUST_NO2.IsReadOnly = false;
                this.MOLAH.IsReadOnly = false;
                this.MOIN_HAZ.IsReadOnly = false;
                this.MODAT_PPID.IsReadOnly = false;
                this.MOIN_HAZ.IsReadOnly = false;
                this.MOIN_HAZ.IsReadOnly = false;
                if (SGN3.IsChecked ?? false)
                {
                    this.SGN2.IsEnabled = false;
                    this.SGN1.IsEnabled = false;
                    AllowDeletions = false;
                    this.INVO_LST_sub.IsReadOnly = true;
                    //this.INVO_LST_sub.CanUserDeleteRows = false;
                    //New Code // this.Page58.IsEnabled = false;
                    //New Code // this.Page155.IsEnabled = false;
                    this.NUMBER.IsReadOnly = true;
                    this.DATE_N.IsReadOnly = true;
                    this.MAS.IsReadOnly = true;
                    this.FNUMCO.IsReadOnly = true;
                    this.SHIFT.IsReadOnly = true;
                    this.CUST_KIND.IsReadOnly = true;
                    this.CUST_NO.IsReadOnly = true;
                    this.CUST_NO2.IsReadOnly = true;
                    this.MOLAH.IsReadOnly = true;
                    this.MOIN_HAZ.IsReadOnly = true;
                }
                else if (SGN2.IsChecked ?? false)
                {
                    this.SGN1.IsEnabled = false;
                    AllowDeletions = false;
                    this.INVO_LST_sub.IsReadOnly = true;
                    //this.INVO_LST_sub.CanUserDeleteRows = false;
                    //New Code // this.Page58.IsEnabled = false;
                    //New Code // this.Page155.IsEnabled = false;
                    this.NUMBER.IsReadOnly = true;
                    this.DATE_N.IsReadOnly = true;
                    this.MAS.IsReadOnly = true;
                    this.FNUMCO.IsReadOnly = true;
                    this.SHIFT.IsReadOnly = true;
                    this.CUST_KIND.IsReadOnly = true;
                    this.CUST_NO.IsReadOnly = true;
                    this.CUST_NO2.IsReadOnly = true;
                    this.MOLAH.IsReadOnly = true;
                    this.MOIN_HAZ.IsReadOnly = true;
                }
            }

            SecurityAllCheck();

            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            if (!NewRecord) //Existing Record
            {
                DEPATMAN.IsEnabled = false;
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                this.InvokeWhenHandleReady(hwnd =>
                {
                    CL_HESABDARI.LetSigneTick(this.GetType().Name, 13, Convert.ToInt32(Baseknow.USERCOD), hwnd);
                });
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }

            if (IsDirectFactor)
            {
                LABEL_HEADER.Content = "فاکتور فروش";
            }
            else
            {
                NUMBER.IsHitTestVisible = true;

                ANBAR_COLUMN.IsReadOnly = true; //انبار
                NAME_CODE_COLUMN.IsReadOnly = true; //نام کالا
                VAHED_K_COLUMN.IsReadOnly = true; //واحد
                MEGH_COLUMN.IsReadOnly = true; //مقدار
                MEGHK_COLUMN.IsReadOnly = true; //مقدار کل

                INVO_LST_sub.CanUserAddRows = false;

                FNUMCO.Visibility = Visibility.Visible;
                LABEL_FNUMCO_.Visibility = Visibility.Visible;

                if (IsExporty)
                {
                    LABEL_HEADER.Content = "فاکتور فروش صادراتی";
                }
                else
                {
                    LABEL_HEADER.Content = "فاکتور فروش غیر مستقیم";
                }
            }

            if (IsExporty)
            {
                TKHN_COLUMN.Visibility = Visibility.Hidden; //درصد تخفیف نقدی
                N_KOL_COLUMN.IsReadOnly = false; //درصد تخفیف
                N_MOIN_COLUMN.IsReadOnly = false; //مبلغ تخفیف
            }

            AllowEdits = false;
        }
        private void Form_BeforeInsert()
        {
            var rst = dbms.DoGetDataSQL<FC1>("SELECT     TOP 100 PERCENT VAS,TICMBAA, MAX(NUMBER) AS MaxOfNUMBER FROM dbo.HEAD_LST WHERE (TAG = 2) GROUP BY VAS,TICMBAA ORDER BY MAX(NUMBER) DESC").FirstOrDefault();
            if (!(rst is null))
            {
                switch (rst.VAS)
                {
                    case 1: VAS1.IsChecked = true; break;
                    case 2: VAS2.IsChecked = true; break;
                    default: break;
                }
                //this.VAS = rst.VAS;
                this.TICMBAA.IsChecked = rst.TICMBAA;
            }
            //Cancel = CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT));

        }

        private void Form_Delete()
        {

            //if (frm.RecordsetClone.RecordCount > 0)
            //{
            //    DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "اين فاكتور داراي اطلاعات  مي باشد .ابتدا اطلاعات سطرهاي زير را حذف كنيد سپس فاكتور را حذف نمائيد.جهت مشاهده توضيحات بيشتر روي فاكتور كليد F1  را فشار دهيد.");
            //    CANCEL = Convert.ToInt32(true);
            //}
        }
        private void Form_Dirty()
        {
            if (Convert.ToBoolean(OKF.IsChecked))
            {
                CANCEL = Convert.ToInt32(true);
            }
        }
        private static void Form_Load()
        {
            if (Baseknow.SANAD == 1 || Baseknow.UGRP == "6")
            {
                //SHRST.Open("deed_hed");
            }
            //var rst = new ADODB.Recordset();
            //string VS;
            //VS = "";
            //rst.Open("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL=0) and idd <> 120");
            //while (!rst.EOF)
            //{
            //    VS = VS + rst.Fields("IDD") + ";" + '"' + SFunction.DECODEUN(rst.Fields("sal_name")) + '"';
            //    rst.MoveNext();
            //    if (!rst.EOF)
            //    {
            //        VS = VS + ";";
            //    }
            //}
            //this.PERSONEL.RowSource = VS;
        }
        public void AllowAdditionEdits(bool CAN)
        {
            //به صورت دستی تک به تک کامنت شده 
            #region DASTY
            if (CAN is true) // Is Enable and ReadOnly = False
            {
                //فاکتور

                // تاریخ
                if (Baseknow.UPDDATE ?? false) //Can Edit DATE_N
                {
                    DATE_N.IsReadOnly = false;
                }


                MAS.IsReadOnly = false;// مدت
                NUMBER.IsReadOnly = false;// شماره حواله
                CUST_KIND.IsReadOnly = false;// نوع مشتری
                CUST_NO.IsReadOnly = false;// نام مشتری
                CUST_NO2.IsReadOnly = false;// فقط کد مشتری
                MOLAH.IsReadOnly = false;// ملاحظات سربرگ
                SHIFT.IsReadOnly = false;// شیفت
                MODAT_PPID.IsReadOnly = false; // نحوه پرداخت
                PEPID.IsReadOnly = false;// اعلامیه قیمیت
                PEID.IsReadOnly = false;// اعلامیه تخفیف
                M_NAGHD2.IsReadOnly = false;// مبلغ نقد
                MABL_VAR2.IsReadOnly = false;//مبلغ کارت بانک
                MABL_HAV2.IsReadOnly = false;// مبلغ بن یا حواله
                TAKHFIF2.IsReadOnly = false;// مبلغ تخفیف
                MOIN_VAR2.IsReadOnly = false;// معین کارت
                MOIN_HAV2.IsReadOnly = false;//معین بن
                //Text163.IsReadOnly = false;// درصد تخفیف
                SHARAYET.IsReadOnly = false;// شرایط پایین


                //__ENABLEY
                DATE_N.IsEnabled = true;// تاریخ
                MAS.IsEnabled = true;// مدت
                NUMBER.IsEnabled = true;// شماره حواله
                CUST_KIND.IsEnabled = true;// نوع مشتری
                CUST_NO.IsEnabled = true;// نام مشتری
                CUST_NO2.IsEnabled = true;// فقط کد مشتری
                MOLAH.IsEnabled = true;// ملاحظات سربرگ
                SHIFT.IsEnabled = true;// شیفت
                MODAT_PPID.IsEnabled = true; // نحوه پرداخت
                PEPID.IsEnabled = true;// اعلامیه قیمیت
                PEID.IsEnabled = true;// اعلامیه تخفیف
                M_NAGHD2.IsEnabled = true;// مبلغ نقد
                MABL_VAR2.IsEnabled = true;//مبلغ کارت بانک
                MABL_HAV2.IsEnabled = true;// مبلغ بن یا حواله
                TAKHFIF2.IsEnabled = true;// مبلغ تخفیف
                MOIN_VAR2.IsEnabled = true;// معین کارت
                MOIN_HAV2.IsEnabled = true;//معین بن
                //Text163.IsEnabled = true;// درصد تخفیف
                SHARAYET.IsEnabled = true;// شرایط پایین
                //فاکتور END

                //New Code // Page58.IsEnabled = true;// تب پشت فاکتور
                //New Code // Page155.IsEnabled = true; // تب سایر
            }
            else
            {
                //فاکتور

                // تاریخ
                if (Baseknow.UPDDATE ?? false) //Can Edit DATE_N
                {
                    //Nothing
                }
                else
                {
                    DATE_N.IsReadOnly = true;
                }

                MAS.IsReadOnly = true;// مدت
                NUMBER.IsReadOnly = true;// شماره حواله
                CUST_KIND.IsReadOnly = true;// نوع مشتری
                CUST_NO.IsReadOnly = true;// نام مشتری
                CUST_NO2.IsReadOnly = true;// فقط کد مشتری
                MOLAH.IsReadOnly = true;// ملاحظات سربرگ
                SHIFT.IsReadOnly = true;// شیفت
                MODAT_PPID.IsReadOnly = true; // نحوه پرداخت
                PEPID.IsReadOnly = true;// اعلامیه قیمیت
                PEID.IsReadOnly = true;// اعلامیه تخفیف
                M_NAGHD2.IsReadOnly = true;// مبلغ نقد
                MABL_VAR2.IsReadOnly = true;//مبلغ کارت بانک
                MABL_HAV2.IsReadOnly = true;// مبلغ بن یا حواله
                TAKHFIF2.IsReadOnly = true;// مبلغ تخفیف
                MOIN_VAR2.IsReadOnly = true;// معین کارت
                MOIN_HAV2.IsReadOnly = true;//معین بن
                //Text163.IsReadOnly = true;// درصد تخفیف
                SHARAYET.IsReadOnly = true;// شرایط پایین

                //__ENABLEY
                DATE_N.IsEnabled = false;// تاریخ
                MAS.IsEnabled = false;// مدت
                NUMBER.IsEnabled = false;// شماره حواله
                CUST_KIND.IsEnabled = false;// نوع مشتری
                CUST_NO.IsEnabled = false;// نام مشتری
                CUST_NO2.IsEnabled = false;// فقط کد مشتری
                MOLAH.IsEnabled = false;// ملاحظات سربرگ
                SHIFT.IsEnabled = false;// شیفت
                MODAT_PPID.IsEnabled = false; // نحوه پرداخت
                PEPID.IsEnabled = false;// اعلامیه قیمیت
                PEID.IsEnabled = false;// اعلامیه تخفیف
                M_NAGHD2.IsEnabled = false;// مبلغ نقد
                MABL_VAR2.IsEnabled = false;//مبلغ کارت بانک
                MABL_HAV2.IsEnabled = false;// مبلغ بن یا حواله
                TAKHFIF2.IsEnabled = false;// مبلغ تخفیف
                MOIN_VAR2.IsEnabled = false;// معین کارت
                MOIN_HAV2.IsEnabled = false;//معین بن
                //Text163.IsEnabled = false;// درصد تخفیف
                SHARAYET.IsEnabled = false;// شرایط پایین
                //فاکتور END

                //New Code // Page58.IsEnabled = false;// تب پشت فاکتور
                //New Code // Page155.IsEnabled = false; // تب سایر
            }
            #endregion

            JAY.IsEnabled = CAN;
            TICMBAA.IsEnabled = CAN;

            ANBARF.IsReadOnly = !CAN;// شماره صادراتی
            ARZD.IsReadOnly = !CAN;// نرخ ارز
            ARZKIND2.IsEnabled = CAN; //نوع ارز

            if (CL_Generaly.IsGHAYM_7)
            {
                MODAT_PPID.IsEnabled = CAN;

                if (!CL_HESABDARI.LETSGO("elamghe")) //نمیتواند اعلامیه قیمت را اصلاح کن 
                {
                    this.PEPID.IsEnabled = false; //Locked = true;
                    this.PEID.IsEnabled = false; //Locked = true;
                }
                else
                {
                    //elamghe	اعلاميه قيمت را بتواند اصلاح کند
                    this.PEPID.IsEnabled = CAN; //Locked = false;
                    this.PEID.IsEnabled = CAN; //Locked = false;
                }
            }

            //پشت فاکتور
            M_NAGHD.IsReadOnly = !CAN;
            MABL_VAR.IsReadOnly = !CAN;
            MABL_HAV.IsReadOnly = !CAN;
            TAKHFIF.IsReadOnly = !CAN;
            MABL_HAZ.IsReadOnly = !CAN;
            FNUMCO.IsReadOnly = !CAN;

            MOIN_VAR.IsEnabled = CAN;
            CMB_MOIN_VAR.IsEnabled = CAN;
            MOIN_HAV.IsEnabled = CAN;
            CMB_MOIN_HAV.IsEnabled = CAN;
            MOIN_HAZ.IsEnabled = CAN;
            CMB_MOIN_HAZ.IsEnabled = CAN;
            BUTTON_SAVE_POSHT.IsEnabled = CAN;
            DELETE_CHKPOSHT.IsEnabled = CAN;

            PAY_GETD_SUB22.IsReadOnly = !CAN;

            //سایر
            BUTTON_SAVE_Sayer.IsEnabled = CAN;
            DELETE_SAYER.IsEnabled = CAN;
            DEPATMAN.IsEnabled = CAN;
            MAGHSAD.IsEnabled = CAN;

            bool AllowedToSavePursantVisitor = CL_HESABDARI.LETSGO("VISITORS"); //ثبت پورسانت ویزیتور
            if (AllowedToSavePursantVisitor)
            {
                VISITOR_DTL_SUB.IsReadOnly = !CAN;
                //VISITOR_DTL_SUB.CanUserAddRows = true;
            }
            else
            {
                VISITOR_DTL_SUB.IsReadOnly = true;
                //VISITOR_DTL_SUB.CanUserAddRows = false;
            }

            REQUEST_NO.IsReadOnly = !CAN;
            BARNAMEH.IsReadOnly = !CAN;
            DRIVER.IsReadOnly = !CAN;
            DRIVER_MOB.IsReadOnly = !CAN;
            CAMIUN_NUM.IsReadOnly = !CAN;
            CAMIUN.IsReadOnly = !CAN;
            CAM_KHALY.IsReadOnly = !CAN;
            CAM_POOR.IsReadOnly = !CAN;
            TOZIH.IsReadOnly = !CAN;

            //مودیان
            AllowMoadianTabEdit(CAN);
        }

        private void AllowMoadianTabEdit(bool CAN)
        {
            inty.IsEnabled = CAN;
            inp.IsEnabled = CAN;
            ins.IsEnabled = CAN;
            CUT.IsEnabled = CAN;
            setm.IsEnabled = CAN;

            sbc.IsReadOnly = !CAN;
            bbc.IsReadOnly = !CAN;
            ft.IsReadOnly = !CAN;
            bpn.IsReadOnly = !CAN;
            scln.IsReadOnly = !CAN;
            scc.IsReadOnly = !CAN;
            cdcn.IsReadOnly = !CAN;
            cdcd.IsReadOnly = !CAN;
            crn.IsReadOnly = !CAN;
            irtaxid.IsReadOnly = !CAN;
            billid.IsReadOnly = !CAN;
            todam.IsReadOnly = !CAN;
            tonw.IsReadOnly = !CAN;
            torv.IsReadOnly = !CAN;
            tocv.IsReadOnly = !CAN;
            cap.IsReadOnly = !CAN;
            insp.IsReadOnly = !CAN;
            tvop.IsReadOnly = !CAN;
            tax17.IsReadOnly = !CAN;
        }

        private void SANAD()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0")
            {
                return;
            }
            else if (!CL_LMethods.IsNumeric(NUMBER.Text))
            {
                return;
            }

            try
            {
                var (SanadNumber, IsSuccessy) = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(Convert.ToInt64(NUMBER.Text), Convert.ToInt64(NUMBER.Text), false);

                if (SanadNumber != null)
                {
                    N_S.Text = SanadNumber.ToString();
                }
            }
            catch (Exception ex)
            {
                AUTO_BAZ.Functions.CL_LMethods.LogWriter.WriteLog($"GENSANADFROOSH exception for invoice {NUMBER.Text}: {ex.Message}");
                AUTO_BAZ.Functions.CL_LMethods.ExpectionLogWriter.WriteLog(ex, "GENSANADFROOSH");
                new Msgwin(false, "خطا در انجام علمیات صدور سند برای فاکتور فروش").Show();
            }
            LETSANAD = false;
        }
        private void Form_Timer(object sender, EventArgs e)
        {
            //if (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) == 0 && this.runone)
            //{
            //    this.runone = false;
            //    Baseknow.dt = CL_HESABDARI.FARSIDATE2();
            //    this.DATE_N.Text = Baseknow.dt.ToString();
            //}
            //else if (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) == 12)
            //{
            //    this.runone = true;
            //}
        }

        private void DEPATMAN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            if (DEPATMAN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox DEPATMAN_TEX = (TextBox)DEPATMAN.Template.FindName("PART_EditableTextBox", DEPATMAN);

            string _SelectedItem_ = "";
            if (DEPATMAN.SelectedItem != null)
            {
                _SelectedItem_ = ((Prg_Proccessy.SQLMODELS.CTABLES.Custom_DEPART)DEPATMAN.SelectedItem).DEPNAME;
            }

            if (DEPATMAN_TEX.Text != _SelectedItem_)
            {
                e.Handled = true;
                new WIN_SearchDEPART(DEPATMAN_TEX.Text.Trim(), I_AM_FOROOSH22).ShowDialog();
            }

            if (DEPATMAN.SelectedValue is null)
            {
                DEPATMAN.SelectedValue = CL_Generaly.VAHED_OF_USER;
            }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }

            // متد کمکی برای شکستن متن به کلمات مجزا و نرمال‌سازی
            static IEnumerable<string> BuildTokens(string source)
            {
                if (string.IsNullOrWhiteSpace(source)) return Enumerable.Empty<string>();

                var normalized = CL_LMethods.NormalizeArabicPersian(source);
                // جداکننده‌های رایج در نام واحدها
                char[] separators = ['-', '،', ',', ' ', '/', '\\', '|', '_', '(', ')'];

                return normalized
                    .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(token => token.Trim()) // حذف فضاهای خالی اضافی
                    .Where(token => !string.IsNullOrWhiteSpace(token));
            }

            // منطق اصلی داخل بدنه کد شما
            if (IsDirectFactor)
            {
                try
                {
                    // --- شروع منطق هوشمند تشخیص شهر از روی نام واحد ---
                    if (DEPATMAN.SelectedItem is Prg_Proccessy.SQLMODELS.CTABLES.Custom_DEPART selectedDep)
                    {
                        string depName = selectedDep.DEPNAME; // مثلا: "شعبه مرکزی - شیراز"

                        if (!string.IsNullOrWhiteSpace(depName))
                        {
                            // 1. تبدیل نام واحد به مجموعه‌ای از کلمات (Token) برای جستجوی سریع
                            // استفاده از HashSet برای سرعت بالا در جستجو
                            var depTokens = new HashSet<string>(BuildTokens(depName));

                            var maghsadItems = MAGHSAD.ItemsSource as List<TCOD_CITY>;

                            if (maghsadItems != null)
                            {
                                // 2. جستجو در لیست شهرها
                                var matchedCity = maghsadItems
                                    .Where(c => !string.IsNullOrWhiteSpace(c.CITYNAME))
                                    // اولویت با شهرهای چند کلمه‌ای (طولانی‌تر) است تا "خراسان رضوی" قبل از "خراسان" چک شود
                                    .OrderByDescending(c => c.CITYNAME.Length)
                                    .FirstOrDefault(city =>
                                    {
                                        // کلمات نام شهر را جدا می‌کنیم
                                        var cityTokens = BuildTokens(city.CITYNAME).ToList();

                                        if (cityTokens.Count == 0) return false;

                                        // شرط تطابق: تمام کلمات شهر باید در کلمات نام واحد وجود داشته باشند
                                        // مثال: اگر شهر "بندر عباس" است، هم "بندر" و هم "عباس" باید در نام واحد باشند
                                        return cityTokens.All(token => depTokens.Contains(token));
                                    });

                                if (matchedCity != null)
                                {
                                    Maghsad_Havaleh_Directyfactor = matchedCity.CITYCODE;
                                }
                            }
                        }
                    }
                    // --- پایان منطق هوشمند ---


                }
                catch { /* Ignore errors in auto-detection */ }
            }


            #region DEPATMAN_AfterUpdate
            if (!_navigationManager.IsNewRecord)
            {
                var rst = dbms.DoGetDataSQL<int?>("SELECT     DEPATMAN FROM dbo.HEAD_LST WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)").ToList();
                var where = " WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)";
                if (rst.Count == 1)
                {
                    var _DEPATMAN_ = this.DEPATMAN.SelectedValue;
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET DEPATMAN = {_DEPATMAN_} {where}");
                    //rst.update();
                }
                if ((this.SGN1.IsChecked is false && this.SGN2.IsChecked is false && this.SGN3.IsChecked is false) && !IsNull(this.MODAT_PPID.SelectedValue) && !IsNull(this.CUST_KIND.SelectedValue) && !IsNull(this.DEPATMAN.SelectedValue) && Baseknow.GHAYM == 7 && this.MODAT_PPID.SelectedIndex > 0)
                {
                    GoGheymateUpdator();

                    //CL_HESABDARI.UpdateGHeymat(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt32(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(this.MODAT_PPID.SelectedValue), Convert.ToInt32(this.CUST_KIND.SelectedValue), Convert.ToInt32(this.DEPATMAN.SelectedValue), Convert.ToInt32(this.TICMBAA.IsChecked));
                    //this.INVO_LST_sub.Form.Requery();
                }
            }
            #endregion
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
                if ((CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME == CUTSNO_TEX.Text)
                {
                    return;
                }
            }

            if (CUTSNO_TEX.Text == "+" || CUTSNO_TEX.Text == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("HEAD_LST_FROOSH22", I_AM_FOROOSH22);//Search Plusy Form Specialy for Customers
                CMBSearch.ShowDialog();
                if (CUST_NO.SelectedValue is null)
                {
                    return;
                }
            }
            //else if (CUTSNO_TEX.Text == "++--")
            //{
            //    ////115-1-959
            //    string thevalue = $"115-1-1389";
            //    if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
            //    {
            //        ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = "Otis Cow" });
            //    }
            //    //var before = CUST_NO.ItemsSource;
            //    //CUST_NO.ItemsSource = null;
            //    CUST_NO.Items.Refresh();

            //    CUST_NO.SelectedValue = null;
            //    CUST_NO.SelectedValue = thevalue;
            //    CUST_KIND.SelectedIndex = 0;
            //}
            //CUST_NO_NotInList
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

            #region CUST_NO_BeforeUpdate
            var _barcod = false;
            if (Baseknow.BARCOD is not null)
            {
                _barcod = (bool)Baseknow.BARCOD;
            }
            if (_barcod && Baseknow.UGRP != "1")
            {
                Cancel = true;
            }
            //TheTimering.Interval = new TimeSpan(0, 0, 0, 5);
            //MeTimer.Interval = 0;
            //MeTimer.IsEnabled = false;
            #endregion

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


            if (NowIsReady && CUST_NO?.SelectedItem != null && !string.IsNullOrEmpty(CUST_NO?.Text))
            {
                if (Convert.ToDouble(Strings.Mid(Baseknow.OPTIONSS, 19, 1)) == 5)
                {
                    var selectedCustomer = dbms.DoGetDataSQL<Custom2_CUST_HESAB>($"SELECT TOP 1 CUST_COD FROM dbo.CUST_HESAB WHERE hes = N'{CUST_NO.SelectedValue}'").FirstOrDefault();
                    if (selectedCustomer?.CUST_COD != null)
                    {
                        CUST_KIND.SelectedValue = selectedCustomer.CUST_COD; CUST_KIND.Items.Refresh();
                    }
                    else if (selectedCustomer != null)
                    {
                        universControl.PopNotifyShowUp("نوع مشتری در تعریف مشتری مشخص نشده است ضروری است آنرا تعریف کنید ! ", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                    }
                }
            }
        }

        private void CUST_NO2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (CUST_NO.SelectedValue is null)
            {
                CUST_NO2.SelectedIndex = -1; CUST_NO2.Text = null; CUST_NO2.Items.Refresh();
                return;
            }
            else
            {
                string SelectedTextCMB = ((Custom_CUST_HESAB)CUST_NO.SelectedItem).hes.ToStringNullSafe();
                if (CUST_NO2.Text != SelectedTextCMB)
                {
                    CUST_NO2.SelectedValue = CUST_NO.SelectedValue;
                }
            }

            #region CUST_NO2_AfterUpdate
            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                //var rst0 = dbms.DoGetDataSQL<string>("SELECT   CUST_NO FROM dbo.HEAD_LST WHERE  (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)").FirstOrDefault();
                //if (!(rst0 is null))
                //{
                //    if (CUST_NO.SelectedValue.ToString() != rst0)
                //    {
                //        rst0 = this.CUST_NO.SelectedValue.ToString();
                //        dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET CUST_NO = N'{rst0}'  WHERE  (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)");
                //        //rst0.update();
                //    }
                //}
                //var rst = dbms.DoGetDataSQL<string>("SELECT   CUST_NO FROM dbo.HEAD_LST WHERE  (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)").FirstOrDefault();
                //if (!(rst0 is null))
                //{
                //    rst = this.CUST_NO.SelectedValue.ToString();
                //    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET CUST_NO = N'{rst}'  WHERE  (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)");
                //    //rst.update();
                //}
            }
            #endregion

            #region CUST_NO2_BeforeUpdate
            if (Baseknow.BARCOD is not null)
            {
                if ((bool)Baseknow.BARCOD && Baseknow.UGRP != "1")
                {
                    Cancel = Convert.ToInt32(true);
                }
            }

            //MeTimer.IsEnabled = false;
            //this.TimerInterval = 0;
            #endregion

            #region CUST_NO2_Exit
            if (!IsNull(this.CUST_NO2.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.CUST_NO2.SelectedValue.ToString()))
                {
                    Msgwin msgwin = new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!");
                    msgwin.ShowDialog();
                    Cancel = Convert.ToInt32(true);
                }
            }
            if (!IsNull(this.CUST_NO.SelectedValue))
            {
                if ((bool)Baseknow.SAGHF || (bool)Baseknow.SAGHF2)
                {
                    if (Convert.ToBoolean(CL_HESABDARI.Checketebar(this.CUST_NO2.SelectedValue.ToString())) == false)
                    {
                        Msgwin msgwin = new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!");
                        msgwin.ShowDialog();
                        //this.Undo();
                    }
                }
            }
            #endregion

            #region CUST_NO2_NotInList
            //if ((bool)Baseknow.BARCOD && Baseknow.UGRP != "1")
            //{
            //    if (ttime < 100d && Strings.Len(NewData) > 7)
            //    {
            //        if (CUST_NO2.Text == "-" || CUST_NO2.Text == "+")
            //        {
            //            //DoCmd.OpenForm("SERSNDTAF1", default, default, default, default, default, "22");
            //            //Response = acDataErrContinue;
            //        }
            //        else if (Strings.Len(NewData) > 7)
            //        {
            //            Information.Err().Clear();
            //            try
            //            {
            //                var rst = dbms.DoGetDataSQL<>("SELECT tDETA_HES.N_KOL, TDETA_HES.NUMBER, TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TOZIH, TDETA_HES.BED_BES, TDETA_HES.ADDRESS, TDETA_HES.TEL, TDETA_HES.CODE_E FROM TDETA_HES WHERE (((TDETA_HES.N_KOL)=" + Baseknow.BEDEHKAR + " ) and NUMBER = 1 AND BED_BES = '" + NewData + "')");

            //            }
            //            catch (Exception)
            //            {
            //                if (rst.RecordCount > 0)
            //                {
            //                    this.CUST_NO.SelectedValue = Baseknow.["BEDEHKAR"] + "-1-" + rst.Fields("tNUMBER");
            //                    this.CUST_NO2 = Baseknow.["BEDEHKAR"] + "-1-" + rst.Fields("TNUMBER");
            //                    CUST_NO_AfterUpdate();
            //                    this.INVO_LST_sub.Enabled = true;
            //                    this.TAKHFIF_APLAY_SUB.Enabled = true;
            //                    Response = acDataErrContinue;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            try
            //            {
            //                var rst = dbms.DoGetDataSQL<>("select n_kol , NUMBER,tNUMBER from tdeta_hes where n_kol = " + Baseknow.["BEDEHKAR"] + " and NUMBER = 1 and tNUMBER = " + NewData);

            //            }
            //            catch (Exception)
            //            {
            //                if (rst.RecordCount == 1)
            //                {
            //                    this.CUST_NO2 = Baseknow.["BEDEHKAR"] + "-1-" + NewData;
            //                    CUST_NO_AfterUpdate();
            //                    Response = acDataErrContinue;
            //                }
            //            }
            //        }
            //        this.TimerInterval = 0;
            //    }
            //}
            //else if (CUST_NO2.Text == "-" || CUST_NO2.Text == "+")
            //{
            //    //DoCmd.OpenForm("SERSNDTAF1", default, default, default, default, default, "22");
            //    // Response = acDataErrContinue;
            //}
            //else if (Strings.Len(NewData) > 7)
            //{
            //    Information.Err().Clear();
            //    try
            //    {
            //        var rst = dbms.DoGetDataSQL<>("SELECT tDETA_HES.N_KOL, TDETA_HES.NUMBER, TDETA_HES.TNUMBER, TDETA_HES.NAME, TDETA_HES.TOZIH, TDETA_HES.BED_BES, TDETA_HES.ADDRESS, TDETA_HES.TEL, TDETA_HES.CODE_E FROM TDETA_HES WHERE (((TDETA_HES.N_KOL)=" + Baseknow.["BEDEHKAR"] + " ) and NUMBER = 1 AND BED_BES = '" + NewData + "')");

            //    }
            //    catch (Exception)
            //    {
            //        if (rst.RecordCount > 0)
            //        {
            //            this.CUST_NO.SelectedValue = Baseknow.["BEDEHKAR"] + "-1-" + rst.Fields("tNUMBER");
            //            this.CUST_NO2 = Baseknow.["BEDEHKAR"] + "-1-" + rst.Fields("TNUMBER");
            //            CUST_NO_AfterUpdate();
            //            this.INVO_LST_sub.Enabled = true;
            //            this.TAKHFIF_APLAY_SUB.Enabled = true;
            //            Response = acDataErrContinue;
            //        }
            //    }
            //}
            //else
            //{
            //    Information.Err().Clear();
            //    try
            //    {
            //        var rst = dbms.DoGetDataSQL<>("select n_kol , NUMBER,tNUMBER from tdeta_hes where n_kol = " + Baseknow.["BEDEHKAR"] + " and NUMBER = 1 and tNUMBER = " + NewData);

            //    }
            //    catch (Exception)
            //    {
            //        if (rst.RecordCount == 1)
            //        {
            //            this.CUST_NO2 = Baseknow.["BEDEHKAR"] + "-1-" + NewData;
            //            CUST_NO_AfterUpdate();
            //            Response = acDataErrContinue;
            //        }
            //    }
            //}
            #endregion

            #region CUST_NO2_KeyPress
            //if (MeTimer.IsEnabled is false)
            {
                ttime = 0d;
                //this.TimerInterval = 100;
                //MeTimer.IsEnabled = true;
            }
            #endregion
        }

        private void SHIFT_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region SHIFT_AfterUpdate
            var rst = dbms.DoGetDataSQL<HLF1>("SELECT     shift,CUST_KIND FROM dbo.HEAD_LST WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)").ToList();
            if (rst.Count == 1)
            {
                var _shift = this.SHIFT.SelectedValue;
                var _cust_kind = this.CUST_KIND.SelectedValue;
                if (_shift != null && _cust_kind != null)
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET SHIFT = {_shift} ,CUST_KIND = {_cust_kind}  WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)");
                }
                //rst.update();
            }
            #endregion
        }

        private void MODAT_PPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // if Lost Focus Was On Combobox (Editable)
            if (!(e.NewFocus is ComboBox)) return;

            //MODAT_PPID_AfterUpdate
            GetModatValueDays();

            //MODAT_PPID_Exit
            GoGheymateUpdator();

        }

        private void GetModatValueDays(bool FocusonMAS = true)
        {
            if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP SelectedModatItem)
            {
                if ((bool)(SelectedModatItem?.PPAME?.Trim().Equals("آزاد")))
                {
                    if (string.IsNullOrWhiteSpace(MAS.Text) || MAS.Text == "0")
                    {
                        MAS.Text = "1";
                    }
                    //Skip
                }
                else
                {
                    int modt = CL_HESABDARI.Getmodat(Convert.ToInt32(MODAT_PPID.SelectedValue));
                    if (modt != Convert.ToInt32(MAS.Text))
                    {
                        this.MAS.Text = modt.ToString();
                    }
                }

            }
            if (Convert.ToInt32(MODAT_PPID.SelectedValue) == 0)
            {
                this.MAS.IsReadOnly = false;

                if (FocusonMAS)
                {
                    Dispatcher.BeginInvoke(new Action(() => { this.MAS.Focus(); }));
                }
            }
            else
            {
                this.MAS.IsReadOnly = true;
            }

            IF_AZAD_THENLOCK();
        }

        private void MODAT_PPID_Enter()
        {
            if (Baseknow.GHAYM.ToString() != "7")
                return;

            // ۱. بدست آوردن مقدار فعلی انتخاب‌شده (MSI)
            int currentSelectedPPID = -1;
            if (_navigationManager?.CurrentRecord?.MODAT_PPID != null)
            {
                currentSelectedPPID = Convert.ToInt32(_navigationManager.CurrentRecord.MODAT_PPID);
            }

            // ۲. بررسی اینکه آیا DEPARTMENT انتخاب شده یا نه
            if (DEPATMAN.SelectedItem == null)
            {
                universControl.PopNotifyShow("واحد نميتواند خالي باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            string tarikhRaw = DATE_N.Text.ToRawTarikh();
            if (!long.TryParse(tarikhRaw, out long tarikhValue))
            {
                universControl.PopNotifyShow("تاریخ نمی‌تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            // ۴. واکشی لیست فیلترشده از PRICE_PAYNO براساس lastPEID
            List<PRICE_PAYNO_MODATP> filteredList;
            if (PEID.SelectedValue != null)
            {
                filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + this.PEID.SelectedValue + ")  union  SELECT 0, 'آزاد', 0").ToList();
            }
            else
            {
                // ۳. گرفتن PEID آخرین اطلاعیه (براساس تاریخ فاکتور و دپارتمان)
                string tarikh = DATE_N.Text.ToRawTarikh();
                int departId = Convert.ToInt32(DEPATMAN.SelectedValue);
                string sqlGetPEID =
                    "SELECT TOP (1) PEID " +
                    "FROM dbo.PRICE_ELAMIETF " +
                    $"WHERE (PEDATE <= {tarikh}) AND (PEPDEPART = {departId}) " +
                    "ORDER BY PEID DESC";

                int? lastPEID = dbms.DoGetDataSQL<int?>(sqlGetPEID).FirstOrDefault();

                if (lastPEID != null)
                {
                    string sqlFiltered =
                        "SELECT P.PPID, P.PPAME, P.MODAT " +
                        "FROM PRICE_PAYNO P " +
                        "INNER JOIN PRICE_ELAMIETF_DTL D ON P.PPID = D.PPID " +
                        $"WHERE D.PEID = {lastPEID} " +
                        "UNION " +
                        "SELECT 0, 'آزاد', 0";

                    filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlFiltered).ToList();
                }
                else
                {
                    if (_navigationManager?.CurrentRecord?.PEID != null)
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT     PRICE_PAYNO.PPID, PRICE_PAYNO.PPAME, PRICE_PAYNO.MODAT FROM         PRICE_PAYNO INNER JOIN   PRICE_ELAMIETF_DTL ON PRICE_PAYNO.PPID = PRICE_ELAMIETF_DTL.PPID  WHERE     (PRICE_ELAMIETF_DTL.PEID = " + _navigationManager.CurrentRecord.PEID + ")  union  SELECT 0, 'آزاد', 0").ToList();
                    }
                    else
                    {
                        filteredList = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>("SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO").ToList();
                    }
                }
            }

            // ۵. اگر مقدار ذخیره‌شده (currentSelectedPPID) جزو filteredList نبود:
            if (currentSelectedPPID > -1 && !filteredList.Any(p => p.PPID == currentSelectedPPID))
            {
                // ۵.۱ واکشی رکورد ذخیره‌شده از جدول اصلی (Full List)
                string sqlGetSaved = $"SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO WHERE PPID = {currentSelectedPPID}";
                PRICE_PAYNO_MODATP savedItem = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>(sqlGetSaved).FirstOrDefault();

                // ۵.۲ اگر آن رکورد اصلاً در جدول اصلی هم وجود داشت، به انتهای filteredList اضافه کن
                if (savedItem != null)
                {
                    savedItem.IsTempyDisplay = true;
                    filteredList.Add(savedItem);
                }
                else
                {
                    currentSelectedPPID = -1; // و مقدار انتخابی را ریست می‌کنیم
                }
            }

            // ۶. حالا ItemsSource جدید را تنظیم کن و SelectedValue را ست کن
            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

            MODAT_PPID.ItemsSource = filteredList;
            MODAT_PPID.DisplayMemberPath = "PPAME";
            MODAT_PPID.SelectedValuePath = "PPID";

            if (currentSelectedPPID > -1)
            {
                MODAT_PPID.SelectedValue = currentSelectedPPID;
            }
            else
            {
                // اگر هیچ مقدار ذخیره‌ای معتبر نبود، می‌توانیم به‌دلخواه اولین آیتم را ست کنیم
                // یا بگذاریم کاربر خودش انتخاب کند:
                MODAT_PPID.SelectedIndex = -1;
            }

            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
        }



        private void TICMBAA_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                return;
            }

            //ذخیره وضعیت تیک مالیات
            if (!DoCmdHeaderSaveUpdate())
            {
                return;
            }

            //محاسبه مالیات کالا ها
            CalculateIMBAA();

            //بروز رسانی سند حسابداری معادل اون
            SANAD();

            //بروز رسانی مانده حساب مشتری
            MasterSummerAndMandeh();

        }
        private void JAY_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                return;
            }

            JAY_AfterUpdate();
        }
        private void JAY_AfterUpdate()
        {
            //ذخیره وضعیت تیک مالیات
            if (!DoCmdHeaderSaveUpdate())
            {
                return;
            }

            //محاسبه جایزه کالا ها
            JAYEHZAH();

            //بروز رسانی مانده حساب مشتری
            MasterSummerAndMandeh();
        }
        private void JAYEHZAH(bool _DisplayMsg_ = true)
        {
            try
            {
                // فرض: NUMBER یک TextBox است یا مشابه آن
                if (!double.TryParse(NUMBER.Text, out double invoiceNumber))
                {
                    new Msgwin(false, "شماره فاکتور نامعتبر است!").ShowDialog();
                    return;
                }
                bool isRewardSystemActive = JAY.IsChecked ?? false; // CheckBox named JAY

                //if (!isRewardSystemActive)
                //{
                //    return; //اگر تیک جایزه را نزده برگرد
                //}

                short invoiceTag = hTAG; // مقدار ثابت
                int performingUserId = (int)Baseknow.USERCOD; // فرض بر اینکه UID اینجاست

                // اجرای stored procedure
                dbms.OpenStoredProcedure("sp_ManageInvoiceRewards", new Dictionary<string, object>
                {
                    { "@InvoiceNumber", invoiceNumber },
                    { "@InvoiceTag", invoiceTag },
                    { "@IsRewardSystemActive", isRewardSystemActive },
                    { "@PerformingUserID", performingUserId }
                });

                // بروزرسانی دیتاگرید جایزه
                ReGetdata();

                if (_DisplayMsg_)
                {
                    universControl.PopNotifyShow($".وضعیت جایزه بروز شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C", 1);
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در پردازش جوایز: {ex.Message}").ShowDialog();
            }
        }

        private void SGN1_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " :امضا شد1 ", " :امضا برداشته شد1:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);

                ////PERSONEL.SelectionChanged -= PERSONEL_SelectionChanged;
                if ((sender as CheckBox).IsChecked is true)
                {
                    PERSONEL.SelectedValue = CL_HESABDARI.GETUSERTASK(MID);
                }
                ////PERSONEL.SelectionChanged += PERSONEL_SelectionChanged;
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور فروش  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text.ToRawTarikh() + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN1.IsChecked, " : امضا شد1 ", " :امضا برداشته شد1 ") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if ((bool)!this.OKF.IsChecked)
                this.OKF.IsChecked = true;

            SGN1usid.Tag = Baseknow.USERCOD;
            SGN1usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command100.IsEnabled = true;
                this.Command120.IsEnabled = true;
                this.custprint.IsEnabled = true;
                this.PRSS.IsEnabled = true;
                this.Command113.IsEnabled = true;

                Command139.IsEnabled = true;
                Command170.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command120.IsEnabled = false;
                this.custprint.IsEnabled = false;
                this.PRSS.IsEnabled = false;
                this.Command113.IsEnabled = false;

                Command139.IsEnabled = false;
                Command170.IsEnabled = false;
            }
            // آبديت امضاءحواله
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(this.SGN1.IsChecked == true, 1, 0) + $"  WHERE  TAG = {hTAG} AND NUMBER = " + this.NUMBER.Text);
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN1usid= " + Baseknow.USERCOD + ",SGN1 =" + Interaction.IIf(this.SGN1.IsChecked == true, 1, 0) + $"  WHERE  TAG = {fTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void SGN2_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور فروش  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN2.IsChecked, ":امضا شد2 ", ":امضا برداشته شد2:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;
            this.SGN2usid.Tag = Baseknow.USERCOD;
            SGN2usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command100.IsEnabled = true;
                this.Command120.IsEnabled = true;
                this.custprint.IsEnabled = true;
                this.PRSS.IsEnabled = true;
                this.Command113.IsEnabled = true;

                Command139.IsEnabled = true;
                Command170.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command120.IsEnabled = false;
                this.custprint.IsEnabled = false;
                this.PRSS.IsEnabled = false;
                this.Command113.IsEnabled = false;

                Command139.IsEnabled = false;
                Command170.IsEnabled = false;
            }
            // آبديت امضاءحواله
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(this.SGN2.IsChecked == true, 1, 0) + $"  WHERE  TAG = {hTAG} AND NUMBER = " + this.NUMBER.Text);
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN2usid= " + Baseknow.USERCOD + ",SGN2 =" + Interaction.IIf(this.SGN2.IsChecked == true, 1, 0) + $"  WHERE  TAG = {fTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void SGN3_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(NUMBER1.Text) <= 0) return;

            double MID;
            string SHARH;
            string td;
            MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
            if (MID > 0d)
            {
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + CL_HESABDARI.GETUSERTASK(MID) + ",STATUS = 1 WHERE IDNUM = " + MID);
            }
            else
            {
                td = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.GetCultureInfo("en-US"));
                SHARH = "'فاکتور فروش  شماره: " + this.NUMBER.Text + " مورخ " + DATE_N.Text + "  به نام: " + CL_HESABDARI.GETTAFNAME(this.CUST_NO.SelectedValue.ToString()) + "','" + this.CUST_NO.SelectedValue + "'";
                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.UCurrentUser() + "'," + SHARH + "," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13," + " GETDATE() " + "," + Baseknow.USERCOD + " )");
                MID = CL_HESABDARI.Gettaskid(Convert.ToDouble(NUMBER.Text), 13);
                dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + MID + ",'" + CL_HESABDARI.UCurrentUser() + "','" + CL_HESABDARI.GETUSERNAME(Convert.ToInt32(Baseknow.USERCOD)) + Interaction.IIf((bool)SGN3.IsChecked, ":امضا شد3 ", ":امضا برداشته شد3:") + "'," + CL_HESABDARI.FARSIDATE() + "," + (System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateTime.Now) * 100 + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateTime.Now)) + ",13," + this.NUMBER.Text + ",13 )");
            }
            this.PERSONEL.Visibility = Visibility.Visible;
            Meidnum = MID;
            if (!(bool)OKF.IsChecked)
                this.OKF.IsChecked = true;

            this.SGN3usid.Tag = Baseknow.USERCOD;
            SGN3usid.Text = rst_personel.FirstOrDefault(x => x.IDD == Baseknow.USERCOD).SAL_NAME;

            if ((bool)SGN1.IsChecked || (bool)SGN3.IsChecked || (bool)SGN3.IsChecked)
            {
                this.Command100.IsEnabled = true;
                this.Command120.IsEnabled = true;
                this.custprint.IsEnabled = true;
                this.PRSS.IsEnabled = true;
                this.Command113.IsEnabled = true;

                Command139.IsEnabled = true;
                Command170.IsEnabled = true;
            }
            else
            {
                this.Command100.IsEnabled = false;
                this.Command120.IsEnabled = false;
                this.custprint.IsEnabled = false;
                this.PRSS.IsEnabled = false;
                this.Command113.IsEnabled = false;

                Command139.IsEnabled = false;
                Command170.IsEnabled = false;
            }
            // آبديت امضاءحواله
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(this.SGN3.IsChecked == true, 1, 0) + $"  WHERE  TAG = {hTAG} AND NUMBER = " + this.NUMBER.Text);
            dbms.DoExecuteSQL("UPDATE HEAD_LST   SET SGN3usid= " + Baseknow.USERCOD + ",SGN3 =" + Interaction.IIf(this.SGN3.IsChecked == true, 1, 0) + $"  WHERE  TAG = {fTAG} AND NUMBER = " + this.NUMBER.Text);

            WinSignActivator();
        }
        private void WinSignActivator()
        {
            if (SGN1.IsChecked == true || SGN2.IsChecked == true || SGN3.IsChecked == true)
            {
                AllowEdits = false;
                AllowDeletions = false;

                //New Code // Page58.IsEnabled = false;
                //New Code // Page155.IsEnabled = false;
                INVO_LST_sub.IsReadOnly = true;
            }
            else
            {
                //AllowEdits = true;
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

                Meidnum = CL_HESABDARI.PERSONELUpdate(13, Convert.ToDouble(NUMBER.Text), Convert.ToInt32(PERSONEL.SelectedValue), "'فاکتور فروش  شماره: " + NUMBER.Text + " مورخ " + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + "  به نام: " + CL_HESABDARI.GETTAFNAME(CUST_NO.SelectedValue.ToString()) + "','" + CUST_NO.SelectedValue + "'");

                universControl.PopNotifyShow($"ارجاع داده به {SelectedTextCMB} شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }

        private void CUST_KIND_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CUST_NO.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (CUST_KIND.SelectedValue == null)
            {
                return;
            }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }

            #region CUST_KIND_AfterUpdate
            if (!NewRecord) //After Update
            {
                if ((this.SGN1.IsChecked is false && this.SGN2.IsChecked is false && this.SGN3.IsChecked is false) && !IsNull(this.MODAT_PPID.SelectedValue) && !IsNull(this.CUST_KIND.SelectedValue) && !IsNull(this.DEPATMAN.SelectedValue) && Baseknow.GHAYM == 7 && this.MODAT_PPID.SelectedIndex > 0)
                {
                    GoGheymateUpdator();
                }
                else
                {
                    var _RST2_MABL = 0d;
                    var _RST2_MABL_K = 0d;
                    var _RST2_N_KOL = 0d;
                    var _RST2_N_MOIN = 0d;

                    var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("select * from invo_lst where tag = 2 and jay = 0  and NUMBER = " + this.NUMBER.Text).ToList();
                    for (int i = 0; i < RST2.Count; i++)
                    {
                        if (Baseknow.GHAYM == 1)
                        {
                            var rstx = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + RST2[i].CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                            //if (IsNull(rstx.Fields(1)))
                            if (IsNull(rstx.MABL))
                            {
                            }
                            else
                            {
                                RST2[i].MABL = rstx.MABL;
                                RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                                _RST2_MABL = (double)RST2[i].MABL;
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                        }
                        else if (Baseknow.GHAYM == 2)
                        {
                            var _Filter = "CODE = N'" + RST2[i].CODE + "'";
                            var rstf = dbms.DoGetDataSQL<STUF_DEF_CSHARP>($"SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID FROM STUF_DEF {_Filter} ").FirstOrDefault();
                            //Set rstf = rstf
                            if ((rstf is null))
                            {
                            }
                            else
                            {
                                RST2[i].MABL = rstf.MABL_F;
                                RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                                _RST2_MABL = (double)RST2[i].MABL;
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                        }
                        else if (Baseknow.GHAYM == 4)
                        {
                            var rstr = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = N'" + RST2[i].CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + this.CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + RST2[i].NUMBER + ") ORDER BY dbo.INVO_LST.NUMBER DESC").FirstOrDefault();

                            if ((rstr is null) && !IsNull(rstr.MABL))
                            {
                                RST2[i].MABL = rstr.MABL;
                                RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                                _RST2_MABL = (double)RST2[i].MABL;
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                            else
                            {
                                Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                                msgwin.ShowDialog();

                                RST2[i].MABL = 0;
                                RST2[i].MABL_K = 0;

                                _RST2_MABL = (double)RST2[i].MABL;
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                        }
                        else if (Baseknow.GHAYM == 5)
                        {
                            var rstc = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + this.CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + RST2[i].CODE + "')").FirstOrDefault();
                            if (!(rstc is null))
                            {
                                if (RST2[i].N_KOL != rstc.TAFPER)
                                {
                                    RST2[i].N_KOL = rstc.TAFPER;
                                    _RST2_N_KOL = (double)RST2[i].N_KOL;
                                }
                                if (RST2[i].MABL != rstc.PRICE_M && rstc.PRICE_M != 0)
                                {
                                    RST2[i].MABL = rstc.PRICE_M;
                                    _RST2_MABL = (double)RST2[i].MABL;
                                }
                                if (RST2[i].MABL_K != Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk)))
                                {
                                    RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));
                                    _RST2_MABL_K = (double)RST2[i].MABL_K;
                                }
                            }
                            else
                            {
                                RST2[i].MABL = 0;
                                RST2[i].MABL_K = 0;
                                _RST2_MABL = (double)RST2[i].MABL;
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                        }

                        if (Baseknow.TKHF == 2)
                        {
                            var rsttt = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + this.CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + RST2[i].CODE + "')").FirstOrDefault();
                            if (!(rsttt is null))
                            {
                                RST2[i].N_KOL = rsttt.TAFPER;
                                _RST2_N_KOL = (double)RST2[i].N_KOL;

                                if (Baseknow.GHAYM == 5)
                                {
                                    if (RST2[i].MABL != rsttt.PRICE_M && rsttt.PRICE_M != 0)
                                    {
                                        RST2[i].MABL = rsttt.PRICE_M;
                                        _RST2_MABL = (double)RST2[i].MABL;
                                    }
                                    if (RST2[i].MABL_K != Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk)))
                                    {
                                        RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));
                                        _RST2_MABL_K = (double)RST2[i].MABL_K;
                                    }
                                }
                            }
                        }
                        RST2[i].N_MOIN = Math.Round((double)(RST2[i].N_KOL * RST2[i].MABL_K / 100));
                        _RST2_N_MOIN = (double)RST2[i].N_MOIN;
                        //RST2.MoveNext();
                    }
                    CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), 2);
                    CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToInt64(M_NAGHD.Text), Convert.ToInt64(MABL_VAR.Text), Convert.ToInt64(MABL_HAV.Text), Convert.ToBoolean(this.TICMBAA.IsChecked));
                    //this.INVO_LST_sub.Requery();
                    //this.TAKHFIF_APLAY_SUB.Requery();
                }
            }
            #endregion
        }

        void MEGH_AfterUpdate()
        {
            if (CURRENT_ROW_ITEMS.MABL is null || CURRENT_ROW_ITEMS.MEGHk is null || CURRENT_ROW_ITEMS.MEGH is null)
            {
                return;
            }
            var currentMeghk = CURRENT_ROW_ITEMS.MEGHk.GetValueOrDefault();
            var currentMeghMar = CURRENT_ROW_ITEMS.MEGH_MAR.GetValueOrDefault();
            var wasMeghk = WAS_ROW_ITEM?.MEGHk.GetValueOrDefault() ?? 0;

            #region MEGH_AfterUpdate
            double min;
            long Temp;
            double MAND;
            var RST0 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ROW_ITEMS.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ROW_ITEMS.VAHED_K + ")))").ToList();
            if (RST0.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
                return;
            }
            else
            {
                var vahadInfo = RST0.FirstOrDefault();
                if (vahadInfo?.NESBAT is null)
                {
                    Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                    msgwin.ShowDialog();
                    return;
                }

                CURRENT_ROW_ITEMS.MEGHk = CURRENT_ROW_ITEMS.MEGH * RST0.FirstOrDefault().NESBAT;
                CURRENT_ROW_ITEMS.MEGH_R = CURRENT_ROW_ITEMS.MEGH * RST0.FirstOrDefault().NESBAT;


                if (CURRENT_ROW_ITEMS.MABL == 0)
                {
                    var TheCol1 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_sub, CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = true;

                    //CURRENT_ROW_ITEMS.MABL_K.TabStop = true;
                }
                else
                {
                    var TheCol1 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol1]);
                    var THECELL1 = CL_LMethods.GetCell(INVO_LST_sub, CURRENT_ROW_INDEX, TheCol1);
                    if (!(THECELL1 is null))
                        THECELL1.IsTabStop = false;
                    //CURRENT_ROW_ITEMS.MABL_K.Text.TabStop = false;

                    if (CURRENT_ROW_ITEMS.MEGHk is null)
                    {
                        return;
                    }

                    CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                }
            }
            if (Baseknow.MOJU && CURRENT_ROW_ITEMS.ANBAR != 0)
            {
                // RST.Open "STUF_DEF"
                // RST.Filter = "CODE = '" && Me.CODE && "'"
                // Set RST = RST
                // If RST.RecordCount = 0 Then
                // Else
                // If IsNull(RST.Fields("MIN_M")) Then
                min = CL_HESABDARI.Getmin((int)CURRENT_ROW_ITEMS.ANBAR, CURRENT_ROW_ITEMS.CODE);
                if (!IsNull(Baseknow.RMOG) && Convert.ToBoolean(Baseknow.RMOG))
                {

                    var RSTM0 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ROW_ITEMS.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ROW_ITEMS.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ROW_ITEMS.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ROW_ITEMS.ANBAR + ")").ToList();
                    if (RSTM0.Count > 0)
                    {
                        var mandValue = RSTM0.FirstOrDefault();
                        MAND = mandValue.GetValueOrDefault();
                        if (Math.Round((double)(mandValue.GetValueOrDefault() - (currentMeghk - (wasMeghk - currentMeghMar))), (int)Baseknow.DIG) < Math.Round(min, (int)Baseknow.DIG) && CURRENT_ROW_ITEMS.ANBAR != 0 && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ROW_ITEMS.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ROW_ITEMS.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ROW_ITEMS.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                            CURRENT_ROW_ITEMS.MABL = WAS_ROW_ITEM.MABL/*.TAG*/;
                            chek = true;
                            var RSTM1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                            string _where = " WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR;
                            if (RSTM1.Count > 0)
                            {
                                RSTM1.FirstOrDefault().MOGODI = MAND;
                                RSTM1.FirstOrDefault().MOGODI_A = 0;
                                //dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {RSTM1.FirstOrDefault().MOGODI},MOGODI_A = 0 {_where}");
                                //در اینجا موجودی بروز نمیشود فقط بررسی میشود
                                //RSTM1.update();
                            }
                        }
                        else
                        {
                            var RSTM2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                            var _where = " WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR;
                            if (RSTM2.Count > 0)
                            {
                                RSTM2.FirstOrDefault().MOGODI = MAND - (currentMeghk - (wasMeghk - currentMeghMar));
                                RSTM2.FirstOrDefault().MOGODI_A = 0;
                                //dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {RSTM2.FirstOrDefault().MOGODI},MOGODI_A = 0 {_where}");
                                //در اینجا موجودی بروز نمیشود فقط بررسی میشود
                                //RSTM2.update();
                            }
                        }
                    }
                }
                else
                {
                    var _where = "CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR;
                    var RSTM3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>($"SELECT * FROM dbo.STUF_STK {_where}").ToList();
                    if (RSTM3.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if (CURRENT_ROW_ITEMS.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                    {
                        if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (currentMeghk - (wasMeghk - currentMeghMar)) < min && Baseknow.MOJU)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                            msgwin.ShowDialog();
                            CURRENT_ROW_ITEMS.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                            CURRENT_ROW_ITEMS.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                            CURRENT_ROW_ITEMS.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                            chek = true;
                        }
                    }
                    else if (RSTM3.FirstOrDefault().MOGODI + RSTM3.FirstOrDefault().MOGODI_A - (currentMeghk - currentMeghMar) < min && Baseknow.MOJU)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد.");
                        msgwin.ShowDialog();
                        CURRENT_ROW_ITEMS.MEGH = WAS_ROW_ITEM.MEGH/*.TAG*/;
                        CURRENT_ROW_ITEMS.MEGHk = WAS_ROW_ITEM.MEGHk/*.TAG*/;
                        CURRENT_ROW_ITEMS.MABL_K = WAS_ROW_ITEM.MABL_K/*.TAG*/;
                        chek = true;
                    }
                }
                //RST.Close();
            }
            CURRENT_ROW_ITEMS.AVRAGE = 0;
            NIM = false;
            var RST = dbms.DoGetDataSQL<DTLMANF_QRE1>("SELECT Sum(DTL_MANF.MABLK) AS SumOfMABLK, HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR FROM HEAD_MANF INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) WHERE (((HEAD_MANF.CODE) = '" + CURRENT_ROW_ITEMS.CODE + "')) GROUP BY HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR").ToList();
            if (RST.Count > 0)
            {
                CURRENT_ROW_ITEMS.AVRAGE = RST.FirstOrDefault().SumOfMABLK/*(0)*/ + RST.FirstOrDefault().IMBIBE_MANF/*(1)*/ + RST.FirstOrDefault().IMBIBE_SAR/*(2)*/;
                NIM = true;
            }
            else
            {
                var RSTM6 = dbms.DoGetDataSQL<QRE_FAC_01>("SELECT RADAH,CODE FROM STUF_DEF  WHERE (STUF_DEF.CODE = '" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                if (RSTM6.Count > 0)
                {
                    if (RSTM6.FirstOrDefault().RADAH == 2 || RSTM6.FirstOrDefault().RADAH == 3)
                    {
                        NIM = true;
                        CURRENT_ROW_ITEMS.AVRAGE = 0;
                    }
                }
            }
            CURRENT_ROW_ITEMS.AVRAGE = CL_HESABDARI.LASTAVRAGE(CURRENT_ROW_ITEMS.CODE, Convert.ToInt64(CURRENT_ROW_ITEMS.ANBAR), Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
            CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
            if ((bool)TICMBAA.IsChecked)
            {
                var RSTM7 = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                if (RSTM7.Count > 0)
                {
                    if (RSTM7.FirstOrDefault()?.CMBAA == true)
                    {
                        if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100)))
                        {
                            CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100));
                        }
                    }
                    else if (CURRENT_ROW_ITEMS.IMBAA != 0)
                    {
                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult is true)
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                    }
                }
            }
            else
            {
                CURRENT_ROW_ITEMS.IMBAA = 0;
            }
            #endregion
        }

        void VAHED_K_AfterUpdate(INVO_LST_FACTOR22 ROW)
        {
            if (ROW?.VAHED_K is null) { return; }
            if (ROW.MABL is null || ROW.MEGHk is null) { return; }

            var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + ROW?.CODE + "' AND ((VAHEDS.VAHED)= " + ROW?.VAHED_K + ")))").ToList();
            if (RST.Count == 0)
            {
                Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                msgwin.ShowDialog();
            }
            else
            {
                ROW.MEGHk = ROW.MEGH * RST.FirstOrDefault().NESBAT;/*Fields(2)*/
                if (ROW.MABL == 0)
                {
                    var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;
                }
                else
                {
                    var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                    var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                    var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                    if (!(THECELL is null))
                        THECELL.IsTabStop = true;

                    if (ROW.MABL is not null && ROW.MEGHk is not null)
                    {
                        ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                    }
                }
            }
            var n_kol_safe = ROW.N_KOL.GetValueOrDefault();
            var mabl_k_safe = ROW.MABL_K.GetValueOrDefault();
            var tkhn_safe = ROW.TKHN.GetValueOrDefault();

            ROW.N_MOIN = Math.Round(n_kol_safe * mabl_k_safe / 100) + Math.Round((mabl_k_safe - Math.Round(n_kol_safe * mabl_k_safe / 100)) * tkhn_safe / 100); //#Changed 2024
            if ((bool)TICMBAA.IsChecked)
            {
                var RSTT = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + ROW.CODE + "'").ToList();
                if (RSTT.Count > 0)
                {
                    if ((bool)RSTT.FirstOrDefault().CMBAA)
                    {
                        var n_moin_safe = ROW.N_MOIN.GetValueOrDefault();
                        if (ROW.IMBAA != Math.Round((mabl_k_safe - n_moin_safe) * CL_HESABDARI.GetArzesh(ROW.CODE) / 100))
                        {
                            ROW.IMBAA = Math.Round((mabl_k_safe - n_moin_safe) * CL_HESABDARI.GetArzesh(ROW.CODE) / 100);
                        }
                    }
                    else if (ROW.IMBAA != 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult is true)
                        {
                            ROW.IMBAA = 0;
                        }
                    }
                }
            }
            else
            {
                ROW.IMBAA = 0;
            }
        }

        void CODE_AfterUpdate(INVO_LST_FACTOR22 ROW, out double min, out double MAND)
        {
            min = 0;
            MAND = 0;
            if (Baseknow.GHAYM == 7 && MODAT_PPID.SelectedIndex != 0 && PEID.SelectedValue != null && PEPID.SelectedValue != null)
            {
                var _PEID_ = Convert.ToInt32(PEID.SelectedValue);
                var _PEPID_ = Convert.ToInt32(PEPID.SelectedValue);
                ROW.MABL = (double?)CL_HESABDARI.GETGHeymatKala(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_);
                ROW.N_KOL = (double?)CL_HESABDARI.GETTaghfifKala1(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_);
                ROW.TKHN = (double?)CL_HESABDARI.GETTaghfifKala2(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_);
                var RSTC0 = dbms.DoGetDataSQL<_VT_>($"SELECT TOP(1) VAHED FROM STUF_DEF WHERE CODE = N'{ROW.CODE}' ORDER BY VAHED").ToList();
                if (RSTC0.Count > 0)
                {
                    if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                    {
                        ROW.MANDAH = RSTC0.FirstOrDefault().TOZIH;
                    }
                }
            }
            else
            {
                if (Baseknow.GHAYM == 1)
                {
                    var RSTC1 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + ROW.CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                    if (IsNull(RSTC1.MABL))
                    {
                    }
                    else
                    {
                        ROW.MABL = RSTC1.MABL;
                        ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                    }
                }
                else if (Baseknow.GHAYM == 2)
                {
                    var RSTC2 = dbms.DoGetDataSQL<double?>($"SELECT TOP(1) MABL_F FROM STUF_DEF WHERE CODE = N'{ROW.CODE}' ORDER BY VAHED").ToList();
                    if (RSTC2.Count == 0)
                    {
                    }
                    else
                    {
                        ROW.MABL = RSTC2.FirstOrDefault();
                        ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                    }
                }
                else if (Baseknow.GHAYM == 4)
                {
                    var RSTC3 = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = N'" + ROW.CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + this.NUMBER.Text + ") ORDER BY dbo.INVO_LST.NUMBER DESC").ToList();
                    if (RSTC3.Count > 0 && !IsNull(RSTC3.FirstOrDefault().MABL))
                    {
                        ROW.MABL = RSTC3.FirstOrDefault().MABL;
                        ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                    }
                    else
                    {
                        Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                        msgwin.ShowDialog();
                        ROW.MABL = 0;
                        ROW.MABL_K = 0;
                    }
                }
                else if (Baseknow.GHAYM == 5)
                {
                    var RSTC4 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + ROW.CODE + "')").ToList();
                    if (RSTC4.Count > 0)
                    {
                        if (ROW.N_KOL != RSTC4.FirstOrDefault().TAFPER)
                        {
                            ROW.N_KOL = RSTC4.FirstOrDefault().TAFPER;
                        }
                        if (ROW.MABL != RSTC4.FirstOrDefault().PRICE_M && RSTC4.FirstOrDefault().PRICE_M != 0)
                        {
                            ROW.MABL = RSTC4.FirstOrDefault().PRICE_M;
                        }
                        if (ROW.MABL_K != Math.Round((double)(ROW.MABL * ROW.MEGHk)))
                        {
                            ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                        }
                    }
                    else
                    {
                        universControl.PopNotifyShow("اين كالا داراي قيمت مصوب نيست است.", Pop1, Pop1Text1, Pop_Border1);
                    }
                }
                if (Baseknow.TKHF == 2)
                {
                    var RSTC5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + ROW.CODE + "')").ToList();
                    if (RSTC5.Count > 0)
                    {
                        ROW.N_KOL = RSTC5.FirstOrDefault().TAFPER;
                        if (Baseknow.GHAYM == 5)
                        {
                            if (ROW.MABL != RSTC5.FirstOrDefault().PRICE_M && RSTC5.FirstOrDefault().PRICE_M != 0)
                            {
                                ROW.MABL = RSTC5.FirstOrDefault().PRICE_M;
                            }
                            if (ROW.MABL_K != Math.Round((double)(ROW.MABL * ROW.MEGHk)))
                            {
                                ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                            }
                        }
                    }
                }
            }

            if (ROW?.N_MOIN != null && ROW?.N_KOL != null && ROW?.MABL_K != null && ROW?.TKHN != null) //For Nullable Check to avoid error
            {
                if (ROW?.N_MOIN != Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100)))
                {
                    ROW.N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                }
            }

            if ((bool)TICMBAA.IsChecked)
            {
                var RSTC6 = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("select CMBAA ,code from STUF_DEF where code = '" + ROW.CODE + "'").ToList();
                if (RSTC6.Count > 0)
                {
                    if ((bool)RSTC6.FirstOrDefault().CMBAA)
                    {
                        if (ROW.IMBAA != Math.Round((double)((ROW.MABL_K - ROW.N_MOIN) * CL_HESABDARI.GetArzesh(ROW.CODE) / 100)))
                        {
                            ROW.IMBAA = Math.Round((double)((ROW.MABL_K - ROW.N_MOIN) * CL_HESABDARI.GetArzesh(ROW.CODE) / 100));
                        }
                    }
                    else if (ROW.IMBAA != 0)
                    {

                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult is true)
                        {
                            ROW.IMBAA = 0;
                        }
                    }
                }
            }
            else
            {
                ROW.IMBAA = 0;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
            {
                if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                {
                    var RSTC7 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + ROW.CODE + "')").ToList();
                    if (RSTC7.Count > 0)
                    {
                        if (ROW.SANAD_NO != RSTC7.FirstOrDefault().MAX_M)
                        {
                            ROW.SANAD_NO = RSTC7.FirstOrDefault().MAX_M;
                        }
                    }
                }
                else if (ROW?.SANAD_NO == 0 || IsNull(ROW?.SANAD_NO))
                {
                    var RSTC8 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + ROW.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                    if (RSTC8.Count > 0)
                    {
                        if (ROW.SANAD_NO != RSTC8.FirstOrDefault())
                        {
                            ROW.SANAD_NO = RSTC8.FirstOrDefault();
                        }
                    }
                }
            }
            ;
            min = CL_HESABDARI.Getmin((int)ROW?.ANBAR, ROW?.CODE);
            if (ROW?.ANBAR != 0)
            {
                if (ROW?.id > 0 && !IsNull(ROW?.CODE))
                {
                    var RSTC9 = dbms.DoGetDataSQL<STUF_STK>("SELECT * FROM STUF_STK WHERE CODE = '" + ROW.CODE + "' AND ANBAR = " + ROW.ANBAR).ToList();
                    if (RSTC9.Count == 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                        msgwin.ShowDialog();
                    }
                    else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                    {
                        var RSTD = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + ROW.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + ROW.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + ROW.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + ROW.ANBAR + ")").ToList();
                        if (RSTD.Count > 0)
                        {
                            MAND = (double)RSTD.FirstOrDefault();
                            if (Math.Round((double)(RSTD.FirstOrDefault() - ROW.MEGHk), 2) < min && Baseknow.MOJU && ROW.ANBAR > 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                msgwin.ShowDialog();
                                ROW = WAS_ROW_ITEM;
                                chek = true;
                            }
                            else
                            {
                                var RSTD2 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + ROW.CODE + "' AND ANBAR = " + ROW.ANBAR).ToList();
                                var _where = " WHERE CODE = '" + ROW.CODE + "' AND ANBAR = " + ROW.ANBAR;
                                if (RSTD2.Count > 0)
                                {
                                    RSTD2.FirstOrDefault().MOGODI = MAND - ROW.MEGHk;
                                }
                            }
                        }
                    }
                    else if (ROW.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                    {
                        if (RSTC9.FirstOrDefault().MOGODI + RSTC9.FirstOrDefault().MOGODI_A - (ROW.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - ROW.MEGH_MAR)) < min && Baseknow.MOJU && ROW.ANBAR > 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                            msgwin.ShowDialog();
                            ROW = WAS_ROW_ITEM;
                            chek = true;
                        }
                    }
                    else if (RSTC9.FirstOrDefault().MOGODI + RSTC9.FirstOrDefault().MOGODI_A - (ROW.MEGHk - ROW.MEGH_MAR) < min && Baseknow.MOJU && ROW.ANBAR > 0)
                    {
                        Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                        msgwin.ShowDialog();
                        ROW = WAS_ROW_ITEM;
                        chek = true;
                    }
                }
            }

            VAHED_K_AfterUpdate(ROW);
        }

        private void OnOpen_SUB()
        {
            var _USERNAME_ = CL_HESABDARI.UCurrentUser().ToString().Replace("ي", "ی").Replace("ك", "ک");
            //Form_Open
            //if (Baseknow.FRUP && Strings.Left((string)CL_HESABDARI.UCurrentUser(), 10) != Convert.ToDouble(Convert.ToString(Strings.ChrW(1605)) + Strings.ChrW(1583) + Strings.ChrW(1740) + Strings.ChrW(1585) + Strings.ChrW(1587) + Strings.ChrW(1740) + Strings.ChrW(1587) + Strings.ChrW(1578) + Strings.ChrW(1605)))
            if ((Baseknow.FRUP ?? false) && _USERNAME_.Contains("مدیر سیستم"))
            {
                MABL_COLUMN.IsReadOnly = true;
                MABL_K_COLUMN.IsReadOnly = true;
            }
            else
            {
                MABL_COLUMN.IsReadOnly = false;
                MABL_K_COLUMN.IsReadOnly = false;
            }
            if (CL_HESABDARI.LETSGO("TFTMLOCK")) //ستون تخفیفات در فاکتور فروش قفل شود
            {
                N_MOIN_COLUMN.IsReadOnly = true;
                N_KOL_COLUMN.IsReadOnly = true;
            }
            else
            {
                N_MOIN_COLUMN.IsReadOnly = false;
                N_KOL_COLUMN.IsReadOnly = false;
            }

            //if (Strings.Mid(Baseknow.OPTIONSS, 26, 1) == "5")
            //{
            //    this.DatasheetFontHeight = Convertion.Val(Strings.Mid(Baseknow.OPTIONSS, 27, 2));
            //}
            //else
            //{
            //    this.DatasheetFontHeight = 8;
            //}
            if (Strings.Mid(Baseknow.OPTIONSS, 29, 1) == "5" || Conversion.Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == 21 && Baseknow.UGRP == "3")
            {
                //ANBAR_COLUMN.ColumnHidden = true;
                ANBAR_COLUMN.Visibility = Visibility.Hidden;


                ManageColumnsTabindex(null, null, "ANBAR", false);
                ManageColumnsTabindex(null, null, "VAHED_K", false);
                ManageColumnsTabindex(null, null, "N_KOL", false);
                ManageColumnsTabindex(null, null, "N_MOIN", false);
                ManageColumnsTabindex(null, null, "IMBAA", false);
                ManageColumnsTabindex(null, null, "MANDAH", false);
                ManageColumnsTabindex(null, null, "MABL", false);
                ManageColumnsTabindex(null, null, "MABL_K", false);

            }
            else
            {
                ANBAR_COLUMN.Visibility = Visibility.Visible;
                // ANBAR_COLUMN.ColumnHidden = false;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
            {
                SANAD_NO_COLUMN.Visibility = Visibility.Visible;
                //SANAD_NO_COLUMN.ColumnHidden = false;
            }
            else
            {
                SANAD_NO_COLUMN.Visibility = Visibility.Hidden;
                //SANAD_NO_COLUMN.ColumnHidden = true;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 52, 1) == "5")
            {

                JAY_COLUMN.Visibility = Visibility.Visible;
                //JAY_COLUMN.ColumnHidden = false;
            }
            else
            {
                JAY_COLUMN.Visibility = Visibility.Hidden;
                //JAY_COLUMN.ColumnHidden = true;
            }
            if (CL_HESABDARI.LETSGO("JAYO"))
            {
                JAYO_COLUMN.IsReadOnly = false;
            }
            else
            {
                JAYO_COLUMN.IsReadOnly = true;
            }
            //if (Strings.Mid(Baseknow.OPTIONSS, 43, 1) == "5")
            //{
            //    //this.CODEh.ColumnHidden = false;
            //    this.CODEh_COLUMN.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    //this.CODEh.ColumnHidden = true;
            //    this.CODEh_COLUMN.Visibility = Visibility.Hidden;
            //}
            if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
            {
                //Change the Validation state
                //var newRule = new ZeroOrNonZeroValidationRule { AllowZero = true };
                // column.EditingElementStyle.Setters.Add(new Setter(ValidationRulesProperty, newRule));
                // 1 - Find Column Index by Name Bound
                var column = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH") as System.Windows.Controls.DataGridTextColumn;
                // 2 - Get Binding
                var binding = column.Binding as Binding;
                binding.ValidationRules.Clear();
                binding.ValidationRules.Add(new ZeroOrNonZeroValidationRule { AllowZero = false });
                //MEGH_COLUMN.ValidationRule = ">0";
            }
            else
            {
                var column = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH") as System.Windows.Controls.DataGridTextColumn;
                var binding = column.Binding as Binding;
                binding.ValidationRules.Clear();
                binding.ValidationRules.Add(new ZeroOrNonZeroValidationRule { AllowZero = true });
                //MEGH_COLUMN.ValidationRule = "";
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) == "5")
            {
                //TKHN_COLUMN.ColumnHidden = false;
                TKHN_COLUMN.Visibility = Visibility.Visible;
            }
            else
            {
                //TKHN_COLUMN.ColumnHidden = true;
                TKHN_COLUMN.Visibility = Visibility.Hidden;
            }
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void INVO_LST_sub_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NowIsReady && INVO_LST_sub.SelectedItem != null && INVO_LST_sub.SelectedItem.ToStringNullSafe() != "{NewItemPlaceholder}")
            {
                if (INVO_LST_sub.Items.Count > 0)
                {
                    //WAS_ROW_ITEM = (INVO_LST_FACTOR22)INVO_LST_sub.SelectedItem;
                    if (!(INVO_LST_sub.CurrentCell.Column is null))
                    {
                        CURRENT_COLUMN_INDEX = INVO_LST_sub.CurrentCell.Column.DisplayIndex;
                    }
                    CURRENT_ROW_INDEX = INVO_LST_sub.SelectedIndex;
                }
            }
        }
        private void INVO_LST_sub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NowIsReady && !(e is null))
            {
                //IF IS NOT NULL برای بروز رسانی کارنت رو و کارنت کالمن
                if (!(INVO_LST_sub.Items.Count < 1) && !(INVO_LST_sub.SelectedItem is null))
                {
                    if (INVO_LST_sub.SelectedItem.ToString() != "{NewItemPlaceholder}")
                    {
                        //WAS_ROW_ITEM = (INVO_LST_FACTOR22)INVO_LST_sub.SelectedItem;

                        if (!(INVO_LST_sub.CurrentCell.Column is null))
                            CURRENT_COLUMN_INDEX = INVO_LST_sub.CurrentCell.Column.DisplayIndex;

                        CURRENT_ROW_INDEX = INVO_LST_sub.SelectedIndex;
                    }
                }
            }
        }
        public bool IsValid(DependencyObject parent)
        {
            if (Validation.GetHasError(parent))
                return false;

            for (int i = 0; i != VisualTreeHelper.GetChildrenCount(parent); ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (!IsValid(child)) { return false; }
            }

            return true;
        }

        private void INVO_LST_SUB_CANCEL_EDIT(object sender, DataGridCellEditEndingEventArgs e = null)
        {
            if (sender is DataGrid DG_SUB)
            {
                ////DG_SUB.Dispatcher.BeginInvoke(() =>
                DG_SUB.Dispatcher.InvokeAsync(() =>
                {
                    DG_SUB.CellEditEnding -= INVO_LST_sub_CellEditEnding;

                    DG_SUB.CancelEdit(DataGridEditingUnit.Cell);
                    DG_SUB.CancelEdit(DataGridEditingUnit.Row);

                    DG_SUB.CellEditEnding += INVO_LST_sub_CellEditEnding;

                    if (e is not null)
                    {
                        var ERGI = e.Row.GetIndex();
                        if (ERGI > -1)
                        {
                            DG_SUB.SelectedIndex = ERGI;
                        }
                        e.EditingElement.Focus();
                    }
                });
            }
        }
        private void INVO_LST_sub_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (NowIsReady && INVO_LST_sub != null)
            {
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

                if (INVO_LST_sub.Items.Count > 0)
                {
                    //بروز رسانی آیتم های ردیاب برای نمایش ایدنکس ردیف و ستون و سلول
                    #region REFILL_CURRENTS_

                    DataGridColumn col1 = e.Column;
                    DataGridRow row1 = e.Row;
                    int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

                    NameOfCurrentColumn = e.Column.SortMemberPath;
                    CURRENT_ROW_INDEX = row_index;
                    CURRENT_COLUMN_INDEX = e.Column.DisplayIndex;

                    //CELL
                    var rowContainer = INVO_LST_sub.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
                    DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    if (cell == null)
                    {
                        INVO_LST_sub.ScrollIntoView(rowContainer, INVO_LST_sub.Columns[CURRENT_COLUMN_INDEX]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    }
                    CURRENT_CELL_ROW = cell;
                    //CELL

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
                        ENTERED_VALUE_ROW = Comboval.SelectedValue;
                    else
                        ENTERED_VALUE_ROW = TexboVal.Text.Trim();

                    CURRENT_ROW_ITEMS = e.Row.Item as INVO_LST_FACTOR22;
                    #endregion

                    //if (MODAT_PPID.SelectedIndex < 0)
                    //{
                    //    INVO_LST_sub_Cancel_Edit(sender, e);
                    //    return;
                    //}

                    #region Form_BeforeUpdate_SUB
                    if (true/*!RDF*/)
                    {
                        //if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                        //{
                        //    if (CURRENT_ROW_ITEMS.MEGH == 0)
                        //    {
                        //        Msgwin msgwin = new Msgwin(false, "مقدار كالا نمي تواند صفر باشد.");
                        //        msgwin.ShowDialog();
                        //        return;
                        //    }
                        //}
                        //if ((CURRENT_ROW_ITEMS.id > 0/*!this.NewRecord*/) && Baseknow.WAR == 1)
                        //{
                        //    Msgwin msgwin = new Msgwin(true, "تغيرات داده شده ثبت شود؟");
                        //    msgwin.ShowDialog();
                        //    if (msgwin.DialogResult is true)
                        //    {
                        //        CANCEL = Convert.ToInt32(true);
                        //    }
                        //}
                        if (IsNull(CURRENT_ROW_ITEMS.ANBAR))
                        {
                            Msgwin msgwin = new Msgwin(false, "اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد.");
                            msgwin.ShowDialog();
                        }
                        else if (IsNull(CURRENT_ROW_ITEMS.CODE))
                        {
                        }
                        else
                        {
                            var RST = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                            if (RST.Count == 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                msgwin.ShowDialog();
                                INVO_LST_SUB_CANCEL_EDIT(sender, e);
                                //this.Undo();
                                CANCEL = Convert.ToInt32(true);
                            }
                        }
                    }
                    #endregion

                    #region Form_AfterUpdate_SUB
                    double min = default;
                    string ST;
                    double MAND;
                    var MEGHTAA = default(long);
                    var MEGHJAYY = default(long);
                    var VAHEDD = default(long);
                    if (true/*!RDF*/)
                    {
                        CURRENT_ROW_ITEMS.CODEO = CURRENT_ROW_ITEMS.CODE;
                        if (WAS_ROW_ITEM.CODE/*.TAG*/ == "")
                        {
                        }
                        else if (!string.IsNullOrEmpty(WAS_ROW_ITEM.CODE) && CURRENT_ROW_ITEMS.CODE != WAS_ROW_ITEM.CODE/*.TAG*/)
                        {
                            var RSTD0 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + WAS_ROW_ITEM.CODE/*.TAG*/ + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                            string _where = " WHERE CODE = '" + WAS_ROW_ITEM.CODE/*.TAG*/ + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR;
                            if (RSTD0.Count == 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "اطلاعات در مورد اين كالا مغايرت دارد.");
                                msgwin.ShowDialog();
                                CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE/*.TAG*/;
                            }
                            else
                            {
                                RSTD0.FirstOrDefault().MOGODI = RSTD0.FirstOrDefault().MOGODI + WAS_ROW_ITEM.MEGHk/*.TAG*/ - CURRENT_ROW_ITEMS.MEGH_MAR;
                                //RSTD0.update();
                                //dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {RSTD0.FirstOrDefault().MOGODI} {_where}");
                                //در اینجا موجودی بروز نمیشود فقط بررسی میشود
                                WAS_ROW_ITEM.MEGHk/*.TAG*/ = 0;
                            }
                            //RST.Close();
                        }
                        var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                        if (RST.Count > 0)
                        {
                            MEGHJAYY = (long)RST.FirstOrDefault().MEGHJAY;
                            MEGHTAA = (long)RST.FirstOrDefault().MEGHTA;
                            VAHEDD = (long)RST.FirstOrDefault().VAHED;
                            // If IsNull(RST.Fields("MIN_M")) Then
                            min = CL_HESABDARI.Getmin((int)CURRENT_ROW_ITEMS.ANBAR, CURRENT_ROW_ITEMS.CODE);
                            // Else
                            // min = Getmin(Me.ANBAR, Me.CODE)
                            // End If
                        }

                        LETSANAD = true;
                        //RST.Close();
                    }
                    #endregion
                    //________________________________________________
                    //بررسی اینکه باید توی فیلد بعدی وایسم یا نه 
                    //CL_LMethods.GetCellConfig(INVO_LST_sub, "", CURRENT_ROW_INDEX).IsTabStop = false;

                    //انبار
                    #region ANBAR
                    if (e.Column.SortMemberPath == "ANBAR")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            //(e.EditingElement as ComboBox).SelectedValue = WAS_ROW_ITEM.ANBAR;
                            return;
                        }
                        else
                        {
                            if (CURRENT_ROW_ITEMS.CODE != null)
                            {
                                var Rst1 = dbms.DoGetDataSQL<STUF_STK>($"SELECT * FROM STUF_STK WHERE CODE = N'{CURRENT_ROW_ITEMS.CODE}' AND ANBAR = {(e.EditingElement as ComboBox).SelectedValue}").ToList();
                                if (Rst1.Count == 0)
                                {
                                    universControl.PopNotifyShow("کالا به انبار فوق تعلق ندارد !", Pop1, Pop1Text1, Pop_Border1);
                                    CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE;
                                    //CURRENT_ROW_ITEMS.NAME_ANBAR = WAS_ROW_ITEM.NAME_ANBAR;
                                    MOGU.Text = null;
                                    INVO_LST_SUB_CANCEL_EDIT(sender, e);
                                }
                                else
                                {
                                    MOGU.Text = (Rst1.FirstOrDefault().MOGODI + Rst1.FirstOrDefault().MOGODI_A).ToString();
                                }
                            }
                        }
                    }
                    #endregion

                    //       var errors = (from c in
                    //     (from object i in INVO_LST_sub.ItemsSource
                    //      select INVO_LST_sub.ItemContainerGenerator.ContainerFromItem(i))
                    //                     where c != null
                    //                     select Validation.GetHasError(c))
                    //.FirstOrDefault(x => x);


                    //کالا
                    #region CODE
                    if (e.Column.SortMemberPath == "NAME_CODE")
                    {
                        if (ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim() ||
                            (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || string.IsNullOrWhiteSpace(ENTERED_VALUE_ROW.ToStringNullSafe())))
                        {
                            #region CODE_NotInList
                            if (CURRENT_ROW_ITEMS.ANBAR is null) // انبار خالی نیست
                            {
                                return;
                            }
                            //اگر نام کالای وارد شده با قبل از وارد شدن برار بود در اصل یعنی مقدار واقعا تغییر نکرده بود رد شو
                            if (true /*ENTERED_VALUE_ROW.ToString() != WAS_ROW_ITEM.NAME_CODE.ToStringNullSafe().Trim()*/)
                            {
                                //الکی نره روی گات فوکوس دیتاگرید
                                INVO_LST_sub.PreviewGotKeyboardFocus -= INVO_LST_sub_PreviewGotKeyboardFocus;

                                //برای اینکه بعد از اینتر نره توی رویداد رو اند ادیت , بره بعدی
                                if (ENTERED_VALUE_ROW.ToString() == "+" || ENTERED_VALUE_ROW.ToString() == "++")
                                {
                                    CURRENT_ROW_ITEMS.MEGH = 0;
                                    CURRENT_ROW_ITEMS.MEGHk = 0;
                                    CURRENT_ROW_ITEMS.MABL_K = 0;
                                    SERCHK sERCHK = new SERCHK(I_AM_FOROOSH22, CURRENT_ROW_ITEMS.ANBAR.ToString());
                                    sERCHK.ShowDialog();

                                    if (FROM_SAERCH_KAL.CODE is null)
                                    {
                                        //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                        var TheCol00 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                        var DGCInf00 = new DataGridCellInfo(INVO_LST_sub.Items[row_index], INVO_LST_sub.Columns[TheCol00]);
                                        var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf00);
                                        TheDGCell_MABL_K.Focus();

                                        //CURRENT_ROW_ITEMS.CODE = null;
                                        //CURRENT_ROW_ITEMS.NAME_CODE = null;
                                        INVO_LST_SUB_CANCEL_EDIT(sender, e);
                                        return;
                                    }
                                    else
                                    {
                                        CURRENT_ROW_ITEMS.CODE = FROM_SAERCH_KAL.CODE;
                                        CURRENT_ROW_ITEMS.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;

                                        CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);

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
                                        CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE;
                                        CURRENT_ROW_ITEMS.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                        return;
                                    }

                                    if (int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                                    {
                                        //اگر عدد وارد کرده برم سرغ کد کالا
                                        var FoundKala = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{ENTERED_VALUE_ROW}') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ROW_ITEMS.ANBAR})").FirstOrDefault();
                                        if (!ReferenceEquals(FoundKala, null))
                                        {
                                            CURRENT_ROW_ITEMS.CODE = FoundKala.CODE;
                                            CURRENT_ROW_ITEMS.NAME_CODE = FoundKala.NAME;

                                            CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);
                                        }
                                        else
                                        {
                                            //شماره فنی
                                            //var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE (dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND (dbo.STUF_FSK.ANBAR = {CURRENT_ROW_ITEMS.ANBAR})").ToList();
                                            var rstfani = dbms.DoGetDataSQL<RESKALAFIND>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{ENTERED_VALUE_ROW}')+'') AND dbo.STUF_FSK.ANBAR = {CURRENT_ROW_ITEMS.ANBAR}").ToList();
                                            if (rstfani.Count > 0)
                                            {
                                                CURRENT_ROW_ITEMS.CODE = rstfani.FirstOrDefault().CODE;
                                                CURRENT_ROW_ITEMS.NAME_CODE = rstfani.FirstOrDefault().NAME;


                                                CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);

                                                PLUS = true;
                                            }
                                            else
                                            {
                                                new Msgwin(false, "چنین کدی وجود ندارد !").ShowDialog();
                                                INVO_LST_SUB_CANCEL_EDIT(sender, e);
                                                //(sender as DataGrid).CancelEdit(DataGridEditingUnit.Cell);

                                                //CURRENT_ROW_ITEMS.CODE = null;
                                                //CURRENT_ROW_ITEMS.NAME_CODE = null;
                                                CURRENT_CELL_ROW?.Focus();
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        CL_KALA_SEARCH.Go_Search_Kala(ENTERED_VALUE_ROW.ToString(), CURRENT_ROW_ITEMS.ANBAR.ToString(), I_AM_FOROOSH22);
                                        if (FROM_SAERCH_KAL.CODE is null)
                                        {
                                            //اگر درست مقدار نداده بود فوکوس رو برگردون که اصلاحش کنه
                                            //var TheCol11 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "NAME_CODE").DisplayIndex;
                                            //var DGCInf11 = new DataGridCellInfo(INVO_LST_sub.Items[row_index], INVO_LST_sub.Columns[TheCol11]);
                                            //var TheDGCell_MABL_K = CL_LMethods.GetDataGridCell(DGCInf11);
                                            //TheDGCell_MABL_K.Focus();

                                            INVO_LST_sub.CellEditEnding -= INVO_LST_sub_CellEditEnding;
                                            INVO_LST_sub.CancelEdit();
                                            INVO_LST_sub.CellEditEnding += INVO_LST_sub_CellEditEnding;

                                            CURRENT_ROW_ITEMS.NAME_CODE = WAS_ROW_ITEM.NAME_CODE;
                                            CURRENT_ROW_ITEMS.CODE = WAS_ROW_ITEM.CODE;

                                            //INVO_LST_sub_Cancel_Edit(sender, e);
                                            return;
                                        }
                                        else
                                        {
                                            CURRENT_ROW_ITEMS.CODE = FROM_SAERCH_KAL.CODE;
                                            CURRENT_ROW_ITEMS.NAME_CODE = FROM_SAERCH_KAL.NAME_CODE;


                                            CURRENT_ROW_ITEMS.VAHED_K = CL_LMethods.TOP_VAHED_K(dbms, CURRENT_ROW_ITEMS.CODE);

                                            //Cleaning
                                            FROM_SAERCH_KAL.CODE = null;
                                            FROM_SAERCH_KAL.NAME_CODE = null;
                                        }
                                    }
                                }
                                if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5") //در فاكتور فروش قيمت مصرف كننده نشان داده شود
                                {
                                    if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                                    {
                                        var RSTCC1 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                        if (RSTCC1.Count > 0)
                                        {
                                            CURRENT_ROW_ITEMS.SANAD_NO = RSTCC1.FirstOrDefault().MAX_M;
                                        }
                                    }
                                    else if (CURRENT_ROW_ITEMS.SANAD_NO == 0 || IsNull(CURRENT_ROW_ITEMS.SANAD_NO))
                                    {
                                        var RSTCC2 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                        if (RSTCC2.Count > 0)
                                        {
                                            CURRENT_ROW_ITEMS.SANAD_NO = RSTCC2.FirstOrDefault();
                                        }
                                    }
                                }
                                //بارکد همراه با مشخصات ترازو
                                //if (!Information.IsNumeric(NewData))
                                //{
                                //    DoCmd.OpenForm("SERCHK", default, default, default, default, default, NewData);
                                //    Response = acDataErrContinue;
                                //    PLUS = true;
                                //}
                                //else
                                //{

                                if (Strings.Len(ENTERED_VALUE_ROW.ToString()) >= 9)
                                {
                                    var RSTCC3 = dbms.DoGetDataSQL<_NFANI_>("SELECT N_FANI,CODE FROM STUF_DEF WHERE N_FANI = '" + ENTERED_VALUE_ROW.ToString() + "'").ToList();
                                    if (RSTCC3.Count > 0)
                                    {
                                        CURRENT_ROW_ITEMS.CODE = RSTCC3.FirstOrDefault().CODE;
                                        ISBAR = true;
                                        if (CURRENT_ROW_ITEMS.MEGH == 0)
                                        {
                                            CURRENT_ROW_ITEMS.MEGH = 1;
                                            CURRENT_ROW_ITEMS.MEGHk = 1;
                                        }
                                    }
                                    if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
                                    {
                                        if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                                        {
                                            var RSTCC4 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                            if (RSTCC4.Count > 0)
                                            {
                                                CURRENT_ROW_ITEMS.SANAD_NO = RSTCC4.FirstOrDefault().MAX_M;
                                            }
                                        }
                                        else if (CURRENT_ROW_ITEMS.SANAD_NO == 0 || IsNull(CURRENT_ROW_ITEMS.SANAD_NO))
                                        {
                                            var RSTCC5 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                            if (RSTCC5.Count > 0)
                                            {
                                                CURRENT_ROW_ITEMS.SANAD_NO = RSTCC5.FirstOrDefault();
                                            }
                                        }
                                    }
                                    string CC = "";
                                    if (Strings.Mid(Baseknow.OPTIONSS, 34, 1) == "5" && !ISBAR)
                                    {
                                        switch (Strings.Mid(Baseknow.OPTIONSS, 35, 2) ?? "")
                                        {
                                            case "03":
                                                {
                                                    CC = "";
                                                    CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 18, 6)));
                                                    CURRENT_ROW_ITEMS.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 4, 3) + "." + Strings.Mid(CURRENT_ROW_ITEMS.CODE, 7, 3));
                                                    CURRENT_ROW_ITEMS.MABL = Convert.ToDouble(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 10, 8));
                                                    CURRENT_ROW_ITEMS.MEGHk = CURRENT_ROW_ITEMS.MEGH;
                                                    CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                                    CURRENT_ROW_ITEMS.CODE = CC;
                                                    ISBAR = true;
                                                    break;
                                                }

                                            default:
                                                {
                                                    CC = "";
                                                    CC = Convert.ToString(Conversion.Val(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 3, 5)));
                                                    if (Convert.ToDouble(Strings.Left(CURRENT_ROW_ITEMS.CODE, 2)) == Convert.ToDouble("27"))
                                                    {
                                                        CURRENT_ROW_ITEMS.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 8, 2) + "." + Strings.Mid(CURRENT_ROW_ITEMS.CODE, 10, 3));
                                                        CURRENT_ROW_ITEMS.MEGHk = CURRENT_ROW_ITEMS.MEGH;
                                                    }
                                                    else
                                                    {
                                                        CURRENT_ROW_ITEMS.MEGH = Convert.ToDouble(Strings.Mid(CURRENT_ROW_ITEMS.CODE, 8, 5));
                                                        CURRENT_ROW_ITEMS.MEGHk = CURRENT_ROW_ITEMS.MEGH;
                                                    }
                                                    CURRENT_ROW_ITEMS.CODE = CC;
                                                    ISBAR = true;
                                                    break;
                                                }
                                        }

                                    }
                                }
                                else
                                {
                                    // Me.CODE = Me.CODE.Text
                                    // ISBAR = False
                                    // If Mid(Forms![BASEKNOW]![OPTIONSS], 33, 1) = "5" Then
                                    // If Mid(Forms![BASEKNOW]![OPTIONSS], 37, 1) = "5" Then
                                    // Set RST = New ADODB.Recordset
                                    // RST.Open "select code,MAX_M FROM STUF_DEF WHERE (CODE = N'" && Me.CODE && "')"
                                    // If RST.RecordCount > 0 Then
                                    // Me.SANAD_NO = RST.Fields("MAX_M")
                                    // End If
                                    // Else
                                    // If Me.SANAD_NO = 0 Or IsNull(Me.SANAD_NO) Then
                                    // RST.Open "SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " && Me.NUMBER && ") AND (CODE = N'" && Me.CODE && "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))"
                                    // If RST.RecordCount > 0 Then
                                    // Me.SANAD_NO = RST.Fields("SANAD_NO")
                                    // End If
                                    // End If
                                    // End If
                                    // End If
                                    // If Mid(Forms![BASEKNOW]![OPTIONSS], 8, 1) = "5" Then
                                    // If Me.MEGH = 0 Or Me.MEGH = 1 Then
                                    // meghone = True
                                    // Me.MEGH = 1
                                    // Me.MEGHk = 1
                                    // End If
                                    // End If
                                }
                                //CURRENT_ROW_ITEMS.VAHED_K.Requery();
                                var RST00 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                                if (RST00.Count == 0)
                                {
                                    MOGU.Text = null;
                                }
                                else
                                {
                                    MOGU.Text = ((double)RST00.FirstOrDefault().MOGODI + RST00.FirstOrDefault().MOGODI_A).ToString();
                                }

                                var RST = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                                if (RST.Count == 0)
                                {
                                }
                                else
                                {
                                    //CURRENT_ROW_ITEMS.VAHED_K = RST.FirstOrDefault().VAHED;
                                    if (Baseknow.GHAYM == 2)
                                    {
                                        CURRENT_ROW_ITEMS.MABL = RST.FirstOrDefault().MABL_F;
                                    }
                                    else if (Baseknow.GHAYM == 5)
                                    {
                                        var RST11 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                        if (RST11.Count > 0)
                                        {
                                            if (CURRENT_ROW_ITEMS.MABL != RST11.FirstOrDefault().PRICE_M && RST11.FirstOrDefault().PRICE_M != 0)
                                            {
                                                CURRENT_ROW_ITEMS.MABL = RST11.FirstOrDefault().PRICE_M;
                                                CURRENT_ROW_ITEMS.N_KOL = RST11.FirstOrDefault().TAFPER;
                                            }
                                            if (CURRENT_ROW_ITEMS.MABL_K != Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk)))
                                            {
                                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                            }
                                        }
                                    }
                                    else if (Baseknow.GHAYM == 4)
                                    {
                                        var RSTCO0 = dbms.DoGetDataSQL<MXNF>("SELECT     TOP 100 PERCENT MAX(dbo.INVO_LST.NUMBER) AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.HEAD_LST.CUST_NO = '" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = '" + CURRENT_ROW_ITEMS.CODE + "')GROUP BY dbo.INVO_LST.MABL ORDER BY MAX(dbo.INVO_LST.NUMBER) DESC").FirstOrDefault();
                                        // DEBUG.PRINT "SELECT     TOP 100 PERCENT MAX(dbo.INVO_LST.NUMBER) AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.HEAD_LST.CUST_NO = '" && Forms![HEAD_LST_FROOSH22]![CUST_NO] && "') AND (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = '" && Me.CODE && "')GROUP BY dbo.INVO_LST.MABL ORDER BY MAX(dbo.INVO_LST.NUMBER) DESC"
                                        if (RSTCO0 == null)
                                        {
                                            CURRENT_ROW_ITEMS.MABL = 0;
                                        }
                                        else
                                        {
                                            CURRENT_ROW_ITEMS.MABL = RSTCO0.MABL;
                                        }
                                    }
                                    else if (Baseknow.GHAYM == 6)
                                    {
                                        var RSTCO1 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                        if (RSTCO1.Count > 0)
                                        {
                                            if (CURRENT_ROW_ITEMS.MABL != RSTCO1.FirstOrDefault().PRICE_M && RSTCO1.FirstOrDefault().PRICE_M != 0)
                                            {
                                                CURRENT_ROW_ITEMS.MABL = RSTCO1.FirstOrDefault().PRICE_M;
                                                CURRENT_ROW_ITEMS.N_KOL = RSTCO1.FirstOrDefault().TAFPER;
                                            }
                                            if (CURRENT_ROW_ITEMS.MABL_K != Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk)))
                                            {
                                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                            }
                                        }
                                    }
                                    if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                                    {
                                        this.MANDAH.Text = RST.FirstOrDefault().TOZIH;
                                    }
                                }
                                if (CURRENT_ROW_ITEMS.ANBAR != 0)
                                {
                                    //if (!this.NewRecord)
                                    if (CURRENT_ROW_ITEMS.id > 0)
                                    {
                                        var RSTCO1 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                                        if (RSTCO1.Count == 0)
                                        {
                                            Msgwin msgwin = new Msgwin(false, "كالا به انبار فوق تعلق ندارد.");
                                            msgwin.ShowDialog();
                                        }
                                        else if ((bool)Baseknow.RMOG || !IsNull(Baseknow.RMOG))
                                        {
                                            var RSTCO2 = dbms.DoGetDataSQL<double?>("SELECT  ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  FROM         dbo.AK_MOGO_AVL_KOL(99999999," + CURRENT_ROW_ITEMS.ANBAR + ") AK_MOGO_AVL_KOL RIGHT OUTER JOIN   dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR LEFT OUTER JOIN  dbo.AK_MOGO_FR(99999999," + CURRENT_ROW_ITEMS.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR WHERE     (dbo.STUF_FSK.CODE = N'" + CURRENT_ROW_ITEMS.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + CURRENT_ROW_ITEMS.ANBAR + ")").ToList();
                                            if (RSTCO2.Count > 0)
                                            {
                                                MAND = (double)RSTCO2.FirstOrDefault()/*("MAND")*/;
                                                if (Math.Round((double)((double)RSTCO2.FirstOrDefault() - CURRENT_ROW_ITEMS.MEGHk), 2) < min && Baseknow.MOJU && CURRENT_ROW_ITEMS.ANBAR > 0)
                                                {
                                                    Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                                    msgwin.ShowDialog();

                                                    CURRENT_ROW_ITEMS = WAS_ROW_ITEM;
                                                    chek = true;
                                                }
                                                else
                                                {
                                                    var RSTCO3 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR).ToList();
                                                    var _WHERE = " WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "' AND ANBAR = " + CURRENT_ROW_ITEMS.ANBAR;
                                                    if (RSTCO3.Count > 0)
                                                    {
                                                        RSTCO3.FirstOrDefault().MOGODI = MAND - CURRENT_ROW_ITEMS.MEGHk;
                                                        RSTCO3.FirstOrDefault().MOGODI_A = 0;
                                                        //dbms.DoExecuteSQL($"UPDATE dbo.STUF_STK SET MOGODI = {RSTCO3.FirstOrDefault().MOGODI},MOGODI_A = 0 {_WHERE}");
                                                        //در اینجا موجودی بروز نمیشود فقط بررسی میشود
                                                        //RSTCO3.update();
                                                    }
                                                }
                                            }
                                        }
                                        else if (CURRENT_ROW_ITEMS.CODE == WAS_ROW_ITEM.CODE/*.TAG*/)
                                        {
                                            if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ROW_ITEMS.MEGHk - (Conversion.Val(Conversion.Val(WAS_ROW_ITEM.MEGHk/*.TAG*/)) - CURRENT_ROW_ITEMS.MEGH_MAR)) < min && Baseknow.MOJU && CURRENT_ROW_ITEMS.ANBAR > 0)
                                            {
                                                Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                                msgwin.ShowDialog();
                                                CURRENT_ROW_ITEMS = WAS_ROW_ITEM;
                                                chek = true;
                                            }
                                        }
                                        else if (RSTCO1.FirstOrDefault().MOGODI + RSTCO1.FirstOrDefault().MOGODI_A - (CURRENT_ROW_ITEMS.MEGHk - CURRENT_ROW_ITEMS.MEGH_MAR) < min && Baseknow.MOJU && CURRENT_ROW_ITEMS.ANBAR > 0)
                                        {
                                            Msgwin msgwin = new Msgwin(false, "خروج كالا از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." + "حداقل موجودي تعريف شده در اف دو :" + min);
                                            msgwin.ShowDialog();
                                            CURRENT_ROW_ITEMS = WAS_ROW_ITEM;
                                            chek = true;
                                        }
                                    }
                                }
                                //RST.Close();
                                if (Baseknow.GHAYM == 1)
                                {
                                    var RSTCO4 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ROW_ITEMS.CODE + "')) GROUP BY INVO_LST.MABL").ToList();
                                    if (IsNull(RSTCO4.FirstOrDefault().MABL) || RSTCO4.Count == 0)
                                    {
                                        CURRENT_ROW_ITEMS.MABL = 0;
                                    }
                                    else
                                    {
                                        CURRENT_ROW_ITEMS.MABL = RSTCO4.FirstOrDefault().MABL;
                                    }
                                }
                                else if (Baseknow.GHAYM == 3)
                                {
                                    CURRENT_ROW_ITEMS.MABL = 0;
                                }
                                // If ISBAR Or meghone Then

                                VAHED_K_AfterUpdate(CURRENT_ROW_ITEMS);
                                // End If
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                if (Baseknow.TKHF == 2)
                                {
                                    var RSTCO5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                    if (RSTCO5.Count > 0)
                                    {
                                        CURRENT_ROW_ITEMS.N_KOL = RSTCO5.FirstOrDefault().TAFPER;
                                        if (Baseknow.GHAYM == 6)
                                        {
                                            if (CURRENT_ROW_ITEMS.MABL != RSTCO5.FirstOrDefault().PRICE_M && RSTCO5.FirstOrDefault().PRICE_M != 0)
                                            {
                                                CURRENT_ROW_ITEMS.MABL = RSTCO5.FirstOrDefault().PRICE_M;
                                            }
                                            if (CURRENT_ROW_ITEMS.MABL_K != Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk)))
                                            {
                                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                            }
                                        }
                                    }
                                }
                                CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));

                                var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                                var DGCInf = new DataGridCellInfo(INVO_LST_sub.Items[row_index], INVO_LST_sub.Columns[TheCol]);
                                var TheDGCell_MEGH = CL_LMethods.GetDataGridCell(DGCInf);
                                TheDGCell_MEGH.Focus();
                                //CURRENT_ROW_ITEMS.MEGH.SetFocus();
                                //}


                                INVO_LST_sub.PreviewGotKeyboardFocus += INVO_LST_sub_PreviewGotKeyboardFocus;
                            }

                            #endregion

                            #region CODE_AfterUpdate
                            CODE_AfterUpdate(CURRENT_ROW_ITEMS, out min, out MAND);
                            #endregion

                            #region CODE_Exit
                            if (!(IsNull(CURRENT_ROW_ITEMS.CODE) || CURRENT_ROW_ITEMS.CODE == ""))
                            {
                                //CURRENT_ROW_ITEMS.CODE.RowSource = "SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF ORDER BY STUF_DEF.NAME";
                                //CURRENT_ROW_ITEMS.CODE.Requery();
                                if (flagt)
                                {
                                    if (IsNull(CURRENT_ROW_ITEMS.CODE) || CURRENT_ROW_ITEMS.CODE == "" && !IsNull(CURRENT_ROW_ITEMS.ANBAR))
                                    {
                                        if (!(Strings.Mid(Baseknow.OPTIONSS, 7, 1) == "5"))
                                        {
                                            //DoCmd.OpenForm("SERCHK");
                                            //khaly = true;
                                        }
                                    }

                                    if (CURRENT_ROW_ITEMS.CODE == CURRENT_ROW_ITEMS.CODEO && (CURRENT_ROW_ITEMS.id <= 0)/*this.NewRecord*/  || PLUS)
                                    {
                                        //if (Information.Err() == 0)
                                        //{
                                        CODE_AfterUpdate(CURRENT_ROW_ITEMS, out min, out MAND);
                                        PLUS = false;
                                        if (Strings.Mid(Baseknow.OPTIONSS, 33, 1) == "5")
                                        {
                                            if (Strings.Mid(Baseknow.OPTIONSS, 37, 1) == "5")
                                            {
                                                var RSTE0 = dbms.DoGetDataSQL<_MX_>("SELECT CODE,MAX_M FROM STUF_DEF WHERE (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                                if (RSTE0.Count > 0)
                                                {
                                                    CURRENT_ROW_ITEMS.SANAD_NO = RSTE0.FirstOrDefault().MAX_M;
                                                }
                                            }
                                            else if (CURRENT_ROW_ITEMS.SANAD_NO == 0 || IsNull(CURRENT_ROW_ITEMS.SANAD_NO))
                                            {
                                                var RSTE1 = dbms.DoGetDataSQL<double?>("SELECT     TOP 1 PERCENT SANAD_NO FROM dbo.INVO_LST WHERE (TAG = 2) And (NUMBER <> " + this.NUMBER.Text + ") AND (CODE = N'" + CURRENT_ROW_ITEMS.CODE + "')  GROUP BY SANAD_NO HAVING      (NOT (SANAD_NO IS NULL))").ToList();
                                                if (RSTE1.Count > 0)
                                                {
                                                    CURRENT_ROW_ITEMS.SANAD_NO = RSTE1.FirstOrDefault();
                                                }
                                            }
                                        }
                                        //}
                                    }
                                }
                                if (PLUS)
                                {
                                    if (Baseknow.GHAYM == 1)
                                    {
                                        var RSTE2 = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + CURRENT_ROW_ITEMS.CODE + "')) GROUP BY INVO_LST.MABL").ToList();
                                        if (IsNull(RSTE2.FirstOrDefault().MABL))
                                        {
                                        }
                                        else
                                        {
                                            CURRENT_ROW_ITEMS.MABL = RSTE2.FirstOrDefault().MABL;
                                            CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                        }
                                    }
                                    else if (Baseknow.GHAYM == 2)
                                    {
                                        var RSTE3 = dbms.DoGetDataSQL<STUF_DEF_CSHARP>("SELECT * FROM dbo.STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                                        if (RSTE3.Count == 0)
                                        {
                                        }
                                        else
                                        {
                                            CURRENT_ROW_ITEMS.MABL = RSTE3.FirstOrDefault().MABL_F;
                                            CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                        }
                                    }
                                    else if (Baseknow.GHAYM == 4)
                                    {
                                        var RSTE4 = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ROW_ITEMS.CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + this.NUMBER.Text + ") ORDER BY dbo.INVO_LST.NUMBER DESC").ToList();
                                        if (RSTE4.Count > 0 && !IsNull(RSTE4.FirstOrDefault().MABL))
                                        {
                                            CURRENT_ROW_ITEMS.MABL = RSTE4.FirstOrDefault().MABL;
                                            CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                        }
                                        else
                                        {
                                            Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                                            msgwin.ShowDialog();
                                            CURRENT_ROW_ITEMS.MABL = 0;
                                            CURRENT_ROW_ITEMS.MABL_K = 0;
                                        }
                                    }
                                    else if (Baseknow.GHAYM == 5)
                                    {
                                        var RSTE5 = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + CURRENT_ROW_ITEMS.CODE + "')").ToList();
                                        if (RSTE5.Count > 0)
                                        {
                                            if (CURRENT_ROW_ITEMS.N_KOL != RSTE5.FirstOrDefault().TAFPER)
                                            {
                                                CURRENT_ROW_ITEMS.N_KOL = RSTE5.FirstOrDefault().TAFPER;
                                            }
                                            if (CURRENT_ROW_ITEMS.MABL != RSTE5.FirstOrDefault().PRICE_M && RSTE5.FirstOrDefault().PRICE_M != 0)
                                            {
                                                CURRENT_ROW_ITEMS.MABL = RSTE5.FirstOrDefault().PRICE_M;
                                            }
                                            if (CURRENT_ROW_ITEMS.MABL_K != Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk)))
                                            {
                                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                            }
                                        }
                                        else
                                        {
                                            universControl.PopNotifyShow("اين كالا داراي قيمت مصوب نيست است.", Pop1, Pop1Text1, Pop_Border1);

                                            //Msgwin msgwin = new Msgwin(false, "اين كالا داراي قيمت مصوب نيست است.");
                                            //msgwin.ShowDialog();
                                            //CURRENT_ROW_ITEMS.MABL = 0;
                                            //CURRENT_ROW_ITEMS.MABL_K = 0;
                                        }
                                        //RST.Close();
                                    }
                                }
                            }
                            if (ISBAR)
                            {
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                                ISBAR = false;
                                // DoCmd.ShowAllRecords
                                //DoCmd.GoToRecord(acActiveDataObject, default, acNewRec);
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
                            INVO_LST_SUB_CANCEL_EDIT(sender, e);
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null)
                        {
                            return;
                        }

                        #region VAHED_K_AfterUpdate
                        VAHED_K_AfterUpdate(CURRENT_ROW_ITEMS);
                        #endregion

                        #region VAHED_K_NotInList
                        var RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ROW_ITEMS.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ROW_ITEMS.VAHED_K + ")))").ToList();
                        if (RSTV1.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                            msgwin.ShowDialog();
                            CURRENT_ROW_ITEMS.VAHED_K = null;
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.MEGHk = CURRENT_ROW_ITEMS.MEGH * RSTV1.FirstOrDefault().NESBAT/*Fields(2)*/;
                            if (CURRENT_ROW_ITEMS.MABL == 0)
                            {
                                var TheCol0 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol0]);
                                var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                                if (!(THECELL0 is null))
                                    THECELL0.IsTabStop = true;

                                //MABL_K.Text.TabStop = true;
                            }
                            else
                            {
                                var TheCol1 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol1]);
                                var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                                if (!(THECELL1 is null))
                                    THECELL1.IsTabStop = true;
                                //MABL_K.Text.TabStop = true;
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                            }
                        }
                        var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                        var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                        var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                        if (!(THECELL is null))
                            THECELL.IsTabStop = true;

                        //this.MEGH.SetFocus();
                        #endregion
                    }
                    #endregion

                    //مقدار
                    #region MEGH
                    if (e.Column.SortMemberPath == "MEGH")
                    {
                        if (CURRENT_ROW_ITEMS.ANBAR is null || CURRENT_ROW_ITEMS.CODE is null || CURRENT_ROW_ITEMS.VAHED_K is null)
                        {
                            return;
                        }
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            //DGR_SUB_INVOLST.Items[row_index].GetType().GetProperty("MEGH").SetValue(DGR_SUB_INVOLST.Items[row_index], (double?)Convert.ToDouble("0"));
                            CURRENT_ROW_ITEMS.MEGH = 0;
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null)
                        {
                            return;
                        }
                        CURRENT_ROW_ITEMS.MEGH = Convert.ToDouble(ENTERED_VALUE_ROW);

                        // Get the DataGridTextColumn
                        DataGridTextColumn column = INVO_LST_sub.Columns.OfType<DataGridTextColumn>().FirstOrDefault(c => c.SortMemberPath == "MEGH");
                        Binding binding = column.Binding as Binding;
                        ZeroOrNonZeroValidationRule rule = binding.ValidationRules.OfType<ZeroOrNonZeroValidationRule>().FirstOrDefault();
                        bool allowZero = rule.AllowZero;
                        double result;
                        if (double.TryParse(ENTERED_VALUE_ROW.ToString(), out result))
                        {
                            if (allowZero && result >= 0) //0 or 1
                            { }
                            else
                            {
                                if (ENTERED_VALUE_ROW.ToString() == "0")
                                {
                                    new Msgwin(false, "مقدار صفر برای مقدار کالا غیر مجاز است").ShowDialog();
                                    return;
                                }
                            }
                        }


                        MEGH_AfterUpdate();

                        #region MEGH_Exit
                        if (khaly)
                        {
                            MEGH_AfterUpdate();
                            khaly = false;
                        }
                        if (meghone)
                        {
                            meghone = false;
                            if (CURRENT_ROW_ITEMS.MABL_K != CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk)
                            {
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
                            }
                            // DoCmd.ShowAllRecords
                            //DoCmd.GoToRecord(default, default, acNewRec);
                            //this.CODE.SetFocus();
                        }
                        #endregion
                    }
                    #endregion

                    //مقدار کل
                    #region MEGHk
                    if (e.Column.SortMemberPath == "MEGHk")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()) || !double.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("MEGHk").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        if ((e.Row.Item as INVO_LST_FACTOR22).ANBAR is null || (e.Row.Item as INVO_LST_FACTOR22).CODE is null || (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null || (e.Row.Item as INVO_LST_FACTOR22).MEGH is null)
                        {
                            return;
                        }

                        #region MEGHk_AfterUpdate
                        long Temp;
                        var RST = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CURRENT_ROW_ITEMS.CODE + "' AND ((VAHEDS.VAHED)= " + CURRENT_ROW_ITEMS.VAHED_K + ")))").ToList();
                        if (RST.Count == 0)
                        {
                            Msgwin msgwin = new Msgwin(false, "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد.");
                            msgwin.ShowDialog();
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.MEGH = CURRENT_ROW_ITEMS.MEGHk / RST.FirstOrDefault().NESBAT/*(2)*/;
                            if (CURRENT_ROW_ITEMS.MABL == 0)
                            {
                                var TheCol0 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol0]);
                                var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                                if (!(THECELL0 is null))
                                    THECELL0.IsTabStop = true;
                                //MABL_K.Text.TabStop = true;
                            }
                            else
                            {
                                var TheCol0 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf0 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol0]);
                                var THECELL0 = CL_LMethods.GetDataGridCell(DGCInf0);
                                if (!(THECELL0 is null))
                                    THECELL0.IsTabStop = false;
                                //MABL_K.Text.TabStop = false;
                                CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)(CURRENT_ROW_ITEMS.MABL * CURRENT_ROW_ITEMS.MEGHk));
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
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("MABL").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        if (
                            CURRENT_ROW_ITEMS.ANBAR is null ||
                            CURRENT_ROW_ITEMS.CODE is null ||
                            CURRENT_ROW_ITEMS.VAHED_K is null ||
                            CURRENT_ROW_ITEMS.MEGH is null ||
                            CURRENT_ROW_ITEMS.MEGHk is null
                            )
                        {
                            return;
                        }

                        #region MABL_Enter
                        if (chek)
                        {
                            var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                            var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                            var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                            if (!(THECELL is null))
                                THECELL.Focus();
                            //this.MEGH.SetFocus();
                            chek = false;
                        }
                        #endregion

                        #region MABL_KeyPress
                        //sendkeyu[KeyAscii];
                        //if (this.JAY == 0)
                        //{
                        //    if (KeyAscii == 1582 || KeyAscii == 107 || KeyAscii == 111 || KeyAscii == 75 || KeyAscii == 79)
                        //    {
                        //        RST.Open("SELECT * FROM STUF_DEF WHERE CODE = '" + this.CODE + "'");
                        //        if (RST.RecordCount > 0)
                        //        {
                        //            this.MABL.Text = RST.Fields("MABL_F");
                        //            if (!IsNull(CODE))
                        //            {
                        //                if (MABL.Text == 0)
                        //                {
                        //                    MABL_K.Text.TabStop = true;
                        //                    MABL_K.Text = 0;
                        //                }
                        //                else
                        //                {
                        //                    MABL_K.Text.TabStop = false;
                        //                    MABL_K.Text = Math.Round(MEGHk * MABL.Text);
                        //                }
                        //                RST.Close();
                        //                RST.Open("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + this.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC");
                        //                if (RST.RecordCount > 0)
                        //                {
                        //                    if (RST.Fields(0) > this.MABL.Text)
                        //                    {
                        //                        DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RST.Fields(0));
                        //                    }
                        //                }
                        //            }
                        //            this.MANDAH.Text.SetFocus();
                        //        }
                        //    }
                        //    if (KeyAscii == 1574 || KeyAscii == 109 || KeyAscii == 77)
                        //    {
                        //        RST.Open("SELECT * FROM STUF_DEF WHERE CODE = '" + this.CODE + "'");
                        //        if (RST.RecordCount > 0)
                        //        {
                        //            this.MABL.Text = RST.Fields("MAX_M");
                        //            if (!IsNull(CODE))
                        //            {
                        //                if (MABL.Text == 0)
                        //                {
                        //                    MABL_K.Text.TabStop = true;
                        //                    MABL_K.Text = 0;
                        //                }
                        //                else
                        //                {
                        //                    MABL_K.Text.TabStop = false;
                        //                    MABL_K.Text = Math.Round(MEGHk * MABL.Text);
                        //                }
                        //                RST.Close();
                        //                RST.Open("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + this.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC");
                        //                if (RST.RecordCount > 0)
                        //                {
                        //                    if (RST.Fields(0) > this.MABL.Text)
                        //                    {
                        //                        DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RST.Fields(0));
                        //                    }
                        //                }
                        //            }
                        //            this.MANDAH.Text.SetFocus();
                        //        }
                        //    }
                        //    if (KeyAscii == 75 || KeyAscii == 228 || KeyAscii == 107 || KeyAscii == 47 || KeyAscii == 1606)
                        //    {
                        //        RST.Open("SELECT * FROM STUF_DEF WHERE CODE = '" + this.CODE + "'");
                        //        if (RST.RecordCount > 0)
                        //        {
                        //            this.MABL.Text = RST.Fields("B_SEF");
                        //            if (!IsNull(CODE))
                        //            {
                        //                if (MABL.Text == 0)
                        //                {
                        //                    MABL_K.Text.TabStop = true;
                        //                    MABL_K.Text = 0;
                        //                }
                        //                else
                        //                {
                        //                    MABL_K.Text.TabStop = false;
                        //                    MABL_K.Text = Math.Round(MEGHk * MABL.Text);
                        //                }
                        //                RST.Close();
                        //                RST.Open("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + this.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC");
                        //                if (RST.RecordCount > 0)
                        //                {
                        //                    if (RST.Fields(0) > this.MABL.Text)
                        //                    {
                        //                        DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RST.Fields(0));
                        //                    }
                        //                }
                        //            }
                        //            this.MANDAH.Text.SetFocus();
                        //        }
                        //    }
                        //    this.N_MOIN = Math.Round(this.N_KOL * this.MABL_K.Text / 100) + Math.Round((this.MABL_K.Text - Math.Round(this.N_KOL * this.MABL_K.Text / 100)) * this.TKHN / 100);
                        //    if (Forms["HEAD_LST_FROOSH22"]["TICMBAA"])
                        //    {
                        //        RST.Open("select CMBAA ,code from STUF_DEF where code = '" + this.CODE + "'");
                        //        if (RST.RecordCount > 0)
                        //        {
                        //            if (RST.Fields("CMBAA"))
                        //            {
                        //                if (this.IMBAA != Math.Round((this.MABL_K.Text - this.N_MOIN) * Baseknow.["ARSESH"] / 100))
                        //                {
                        //                    this.IMBAA = Math.Round((this.MABL_K.Text - this.N_MOIN) * Baseknow.["ARSESH"] / 100);
                        //                }
                        //            }
                        //            else if (this.IMBAA != 0)
                        //            {
                        //                Baseknow.["Text44"] = false;
                        //                DoCmd.OpenForm("MSGDIALOG", default, default, default, default, acDialog, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                        //                if (Baseknow.["Text44"])
                        //                {
                        //                    this.IMBAA = 0;
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        this.IMBAA = 0;
                        //    }
                        //}
                        #endregion

                        #region MABL_AfterUpdate
                        MABL_AfterUpdate(CURRENT_ROW_ITEMS);
                        #endregion

                        #region MABL_Exit
                        if (!(IsNull(CURRENT_ROW_ITEMS.CODE) || CURRENT_ROW_ITEMS.CODE == "") && Baseknow.GHAYM != 5)
                        {
                            var RST = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + CURRENT_ROW_ITEMS.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                            if (RST.Count == 0)
                            {
                            }
                            else if (RST.FirstOrDefault().MABL/*(0)*/ > CURRENT_ROW_ITEMS.MABL && CURRENT_ROW_ITEMS.MABL != 0)
                            {
                                Msgwin msgwin = new Msgwin(false, "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RST.FirstOrDefault().MABL);
                                msgwin.ShowDialog();

                            }
                        }
                        if (CURRENT_ROW_ITEMS.N_MOIN != Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100)))
                        {
                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((double)(CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
                        }
                        #endregion

                    }
                    #endregion

                    //مبلغ کل
                    #region MABL_K
                    if (e.Column.SortMemberPath == "MABL_K")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("MABL_K").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        if (
                           (e.Row.Item as INVO_LST_FACTOR22).ANBAR is null ||
                           (e.Row.Item as INVO_LST_FACTOR22).CODE is null ||
                           (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null ||
                           (e.Row.Item as INVO_LST_FACTOR22).MEGH is null ||
                           (e.Row.Item as INVO_LST_FACTOR22).MEGHk is null ||
                           (e.Row.Item as INVO_LST_FACTOR22).MABL is null
                           )
                        {
                            return;
                        }

                        #region MABL_K_AfterUpdate
                        if (Math.Round((double)CURRENT_ROW_ITEMS.MABL_K) != CURRENT_ROW_ITEMS.MABL_K)
                        {
                            CURRENT_ROW_ITEMS.MABL_K = Math.Round((double)CURRENT_ROW_ITEMS.MABL_K);
                        }
                        if (CURRENT_ROW_ITEMS.MEGHk == 0)
                        {
                            CURRENT_ROW_ITEMS.MABL_K = 0;
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.MABL = CURRENT_ROW_ITEMS.MABL_K / CURRENT_ROW_ITEMS.MEGHk;
                            //MABL_K.Text.TabStop = false;
                            var TheCol1 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                            var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol1]);
                            var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                            if (!(THECELL1 is null))
                                THECELL1.IsTabStop = false;
                        }
                        CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
                        if ((bool)TICMBAA.IsChecked)
                        {
                            var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                            if (RST.Count > 0)
                            {
                                if ((bool)RST.FirstOrDefault().CMBAA)
                                {
                                    if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100)))
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100));
                                    }
                                }
                                else if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();
                                    if (msgwin.DialogResult is true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                            //RST.Close();
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                        #endregion

                        #region MABL_K_Exit
                        if (CURRENT_ROW_ITEMS.MABL == 0 && !IsNull(CURRENT_ROW_ITEMS.CODE))
                        {
                            if (CURRENT_ROW_ITEMS.MEGHk == 0)
                            {
                                if (CURRENT_ROW_ITEMS.MABL_K != 0)
                                {
                                    CURRENT_ROW_ITEMS.MABL_K = 0;
                                }
                            }
                            else
                            {
                                if (CURRENT_ROW_ITEMS.MABL != CURRENT_ROW_ITEMS.MABL_K / CURRENT_ROW_ITEMS.MEGHk)
                                {
                                    CURRENT_ROW_ITEMS.MABL = CURRENT_ROW_ITEMS.MABL_K / CURRENT_ROW_ITEMS.MEGHk;
                                }
                                //MABL_K.Text.TabStop = false;
                                var TheCol1 = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MABL_K").DisplayIndex;
                                var DGCInf1 = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol1]);
                                var THECELL1 = CL_LMethods.GetDataGridCell(DGCInf1);
                                if (!(THECELL1 is null))
                                    THECELL1.IsTabStop = false;
                            }
                        }
                        if (CURRENT_ROW_ITEMS.N_MOIN != Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100)))
                        {
                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
                        }
                        #endregion
                    }
                    #endregion

                    //تخفیف نقدی (ت.ن) درصد
                    #region TKHN
                    if (e.Column.SortMemberPath == "TKHN")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            CURRENT_ROW_ITEMS.TKHN = 0;
                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).N_KOL) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) / 100) + Math.Round((Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) - Math.Round(Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).N_KOL) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) / 100)) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).TKHN) / 100);
                            return;
                        }
                        if (!int.TryParse(ENTERED_VALUE_ROW.ToString(), out _))
                        {
                            CURRENT_ROW_ITEMS.TKHN = 0;

                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).N_KOL) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) / 100) + Math.Round((Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) - Math.Round(Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).N_KOL) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).MABL_K) / 100)) * Convert.ToDouble((e.Row.Item as INVO_LST_FACTOR22).TKHN) / 100);
                            return;
                        }
                        if (Convert.ToInt32(ENTERED_VALUE_ROW) > 100 || Convert.ToInt32(ENTERED_VALUE_ROW) < 0)
                        {
                            (e.Row.Item as INVO_LST_FACTOR22).TKHN = null;
                            return;
                        }
                        if (
                            (e.Row.Item as INVO_LST_FACTOR22).ANBAR is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).CODE is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).VAHED_K is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).MEGH is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).MEGHk is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).MABL is null ||
                            (e.Row.Item as INVO_LST_FACTOR22).MABL_K is null
                            )
                        {
                            return;
                        }
                        #region TKHN_AfterUpdate
                        CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
                        if ((bool)TICMBAA.IsChecked)
                        {
                            var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                            if (RST.Count > 0)
                            {
                                if ((bool)RST.FirstOrDefault().CMBAA)
                                {
                                    if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100)))
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100));
                                    }
                                }
                                else if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                {
                                    Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                    msgwin.ShowDialog();
                                    if (msgwin.DialogResult is true)
                                    {
                                        CURRENT_ROW_ITEMS.IMBAA = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            CURRENT_ROW_ITEMS.IMBAA = 0;
                        }
                        #endregion
                    }
                    #endregion

                    //تخفیف
                    #region N_KOL
                    if (e.Column.SortMemberPath == "N_KOL")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            CURRENT_ROW_ITEMS.N_KOL = 0;
                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100) + Math.Round((Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) - Math.Round(Convert.ToDouble(CURRENT_ROW_ITEMS.N_KOL) * Convert.ToDouble(CURRENT_ROW_ITEMS.MABL_K) / 100)) * Convert.ToDouble(CURRENT_ROW_ITEMS.TKHN) / 100);
                            return;
                        }
                        if (
                            CURRENT_ROW_ITEMS.ANBAR is null ||
                            CURRENT_ROW_ITEMS.CODE is null ||
                            CURRENT_ROW_ITEMS.VAHED_K is null ||
                            CURRENT_ROW_ITEMS.MEGH is null ||
                            CURRENT_ROW_ITEMS.MEGHk is null ||
                            CURRENT_ROW_ITEMS.MABL is null ||
                            CURRENT_ROW_ITEMS.MABL_K is null
                            )
                        {
                            return;
                        }
                        else // IF ALL IS RIGHT ABOUT THIS ↓
                        {
                            var nkol = CURRENT_ROW_ITEMS.N_KOL;
                            if (string.IsNullOrEmpty(nkol.ToStringNullSafe()))
                            {
                                CURRENT_ROW_ITEMS.N_KOL = 0;
                                nkol = 0;
                            }

                            #region N_KOL_AfterUpdate
                            CURRENT_ROW_ITEMS.N_MOIN = Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100)) + Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - Math.Round((double)(CURRENT_ROW_ITEMS.N_KOL * CURRENT_ROW_ITEMS.MABL_K / 100))) * CURRENT_ROW_ITEMS.TKHN / 100));
                            if ((bool)TICMBAA.IsChecked)
                            {
                                var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                                if (RST.Count > 0)
                                {
                                    if ((bool)RST.FirstOrDefault().CMBAA)
                                    {
                                        if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100)))
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100));
                                        }
                                    }
                                    else if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                        msgwin.ShowDialog();
                                        if (msgwin.DialogResult is true)
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.IMBAA = 0;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    //مبلغ تخفیف
                    #region N_MOIN
                    if (e.Column.SortMemberPath == "N_MOIN")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("N_MOIN").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        if (
                            CURRENT_ROW_ITEMS.ANBAR is null ||
                            CURRENT_ROW_ITEMS.CODE is null ||
                            CURRENT_ROW_ITEMS.VAHED_K is null ||
                            CURRENT_ROW_ITEMS.MEGH is null ||
                            CURRENT_ROW_ITEMS.MEGHk is null ||
                            CURRENT_ROW_ITEMS.MABL is null ||
                            CURRENT_ROW_ITEMS.MABL_K is null
                            )
                        {
                            return;
                        }
                        else // IF ALL IS RIGHT ABOUT THIS ↓
                        {
                            #region N_MOIN_AfterUpdate
                            if (CURRENT_ROW_ITEMS.MABL_K > 0)
                            {
                                CURRENT_ROW_ITEMS.N_KOL = CURRENT_ROW_ITEMS.N_MOIN * 100 / CURRENT_ROW_ITEMS.MABL_K;
                                CURRENT_ROW_ITEMS.TKHN = 0;
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.N_MOIN = 0;
                                CURRENT_ROW_ITEMS.N_KOL = 0;
                                CURRENT_ROW_ITEMS.TKHN = 0;
                            }
                            if ((bool)TICMBAA.IsChecked)
                            {
                                var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + CURRENT_ROW_ITEMS.CODE + "'").ToList();
                                if (RST.Count > 0)
                                {
                                    if ((bool)RST.FirstOrDefault().CMBAA)
                                    {
                                        if (CURRENT_ROW_ITEMS.IMBAA != Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100)))
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = Math.Round((double)((CURRENT_ROW_ITEMS.MABL_K - CURRENT_ROW_ITEMS.N_MOIN) * CL_HESABDARI.GetArzesh(CURRENT_ROW_ITEMS.CODE) / 100));
                                        }
                                    }
                                    else if (CURRENT_ROW_ITEMS.IMBAA != 0)
                                    {
                                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                        msgwin.ShowDialog();
                                        if (msgwin.DialogResult is true)
                                        {
                                            CURRENT_ROW_ITEMS.IMBAA = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.IMBAA = 0;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    //Unit Price (مبلغ ارزی واحد)
                    #region N_TAF
                    if (e.Column.SortMemberPath == "N_TAF")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("N_TAF").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        else
                        {
                            //N_TAF_AfterUpdate
                            if (Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) == 0)
                            {
                                CURRENT_ROW_ITEMS.TOTALARZ = 0;
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.TOTALARZ = Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) * CURRENT_ROW_ITEMS.MEGHk;
                            }

                            CURRENT_ROW_ITEMS.MABL = Convert.ToDouble(ENTERED_VALUE_ROW /*N_TAF*/) * Convert.ToDouble(ARZD.Text);
                            MABL_AfterUpdate(CURRENT_ROW_ITEMS);
                        }
                    }
                    #endregion

                    //Line Total (مبلغ کل ارزی)
                    #region TOTALARZ
                    if (e.Column.SortMemberPath == "TOTALARZ")
                    {
                        if (string.IsNullOrEmpty(ENTERED_VALUE_ROW.ToStringNullSafe()))
                        {
                            INVO_LST_sub.Items[row_index].GetType().GetProperty("TOTALARZ").SetValue(INVO_LST_sub.Items[row_index], (double?)Convert.ToDouble("0"));
                            return;
                        }
                        else
                        {
                            //TOTALARZ_AfterUpdate
                            if (CURRENT_ROW_ITEMS.MEGHk == 0)
                            {
                                CURRENT_ROW_ITEMS.TOTALARZ = 0;
                            }
                            else
                            {
                                CURRENT_ROW_ITEMS.N_TAF = Convert.ToDouble(ENTERED_VALUE_ROW /*TOTALARZ*/) / CURRENT_ROW_ITEMS.MEGHk;
                            }

                            CURRENT_ROW_ITEMS.MABL = CURRENT_ROW_ITEMS.N_TAF * Convert.ToDouble(ARZD.Text);

                            MABL_AfterUpdate(CURRENT_ROW_ITEMS);
                        }
                    }
                    #endregion


                    var MABL_TAKHFIF = Convert.ToDouble(FACTOR22_INVO_DATA.Sum(r => r.N_MOIN is null ? 0 : r.N_MOIN)); //جمع مبلغ تخفیف دیتاگرید
                    var CTT_TAKHFIF = Convert.ToDouble(TAKHFIF2.Text); //مجموع مبلغ تخفیف کل
                    if (MABL_TAKHFIF != CTT_TAKHFIF)
                    {
                        if (MABL_TAKHFIF >= 0)
                        {
                            TAKHFIF2.Text = MABL_TAKHFIF.ToStringNullSafe();
                        }
                    }

                    //____________________________________
                    //var CURRENT_ITMES_ROW = e.Row.Item as INVO_LST_FACTOR22;
                    //WAS_ROW_ITEM = (e.Row.Item as INVO_LST_FACTOR22);
                    //var test0 = FACTOR22_INVO_DATA.FirstOrDefault().MEGH;
                    //var test1 = CURRENT_ROW_ITEMS.MEGH + "\n" + CURRENT_CELL_ROW + "\n" + ENTERED_VALUE_ROW + FACTOR22_INVO_DATA.FirstOrDefault().MEGH;
                }
            }
        }

        private void MABL_AfterUpdate(INVO_LST_FACTOR22? Rowy, bool IsSingleCurrentRow = true, bool DoShoeMessages = true)
        {
            if (Rowy is null) return;
            if (Convert.ToInt64(Rowy.JAY) > 0)
            {
                Rowy.MABL = 1;
                Rowy.MABL_K = Rowy.MEGHk;
                Rowy.N_KOL = 100;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //CURRENT_ROW_ITEMS
            long Temp;
            if (Rowy.MABL == 0)
            {
                var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = true;
                //MABL_K.Text.TabStop = true;
                Rowy.MABL_K = Math.Round((double)(Rowy.MABL * Rowy.MEGHk));
            }
            else
            {
                var TheCol = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == "MEGH").DisplayIndex;
                var DGCInf = new DataGridCellInfo(CURRENT_ROW_INDEX, INVO_LST_sub.Columns[TheCol]);
                var THECELL = CL_LMethods.GetDataGridCell(DGCInf);
                if (!(THECELL is null))
                    THECELL.IsTabStop = false;

                Rowy.MABL_K = Math.Round((double)(Rowy.MABL * Rowy.MEGHk));
            }
            Rowy.N_MOIN = Math.Round((double)(Rowy.N_KOL * Rowy.MABL_K / 100)) + Math.Round((double)((Rowy.MABL_K - Math.Round((double)(Rowy.N_KOL * Rowy.MABL_K / 100))) * Rowy.TKHN / 100));
            var RSTMB0 = dbms.DoGetDataSQL<PRT2>("SELECT TOP 100 PERCENT dbo.INVO_LST.MABL, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 1) AND (dbo.INVO_LST.CODE = N'" + Rowy.CODE + "') ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
            if (RSTMB0.Count == 0)
            {
            }
            else if (RSTMB0.FirstOrDefault().MABL/*(0)*/ > Rowy.MABL)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "قيمت فروش از قيمت خريد كمتر مي باشد. آخرين قيمت خريد : " + RSTMB0.FirstOrDefault().MABL });
            }
            Rowy.AVRAGE = 0;
            NIM = false;
            var RSTMB1 = dbms.DoGetDataSQL<DTLMANF_QRE1>("SELECT Sum(DTL_MANF.MABLK) AS SumOfMABLK, HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR FROM HEAD_MANF INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) WHERE (((HEAD_MANF.CODE) = '" + Rowy.CODE + "')) GROUP BY HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR").ToList();
            if (RSTMB1.Count > 0)
            {
                Rowy.AVRAGE = RSTMB1.FirstOrDefault().SumOfMABLK/*(0)*/ + RSTMB1.FirstOrDefault().IMBIBE_MANF/*(1)*/ + RSTMB1.FirstOrDefault().IMBIBE_SAR/*(2)*/;
                NIM = true;
            }
            else
            {
                var RSTMB2 = dbms.DoGetDataSQL<QRE_FAC_01>("SELECT RADAH,CODE FROM STUF_DEF  WHERE (STUF_DEF.CODE = '" + Rowy.CODE + "')").ToList();
                if (RSTMB2.Count > 0)
                {
                    if (RSTMB2.FirstOrDefault().RADAH == 2 || RSTMB2.FirstOrDefault().RADAH == 3)
                    {
                        NIM = true;
                        Rowy.AVRAGE = 0;
                    }
                }
            }
            Rowy.AVRAGE = CL_HESABDARI.LASTAVRAGE(Rowy.CODE, (long)Rowy.ANBAR, Convert.ToInt64(DATE_N.Text.ToRawTarikh()));
            Rowy.N_MOIN = Math.Round((double)(Rowy.N_KOL * Rowy.MABL_K / 100)) + Math.Round((double)((Rowy.MABL_K - Math.Round((double)(Rowy.N_KOL * Rowy.MABL_K / 100))) * Rowy.TKHN / 100));
            if ((bool)TICMBAA.IsChecked)
            {
                var RST = dbms.DoGetDataSQL<HLF2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + Rowy.CODE + "'").ToList();
                if (RST.Count > 0)
                {
                    if ((bool)RST.FirstOrDefault().CMBAA)
                    {
                        if (Rowy.IMBAA != Math.Round((double)((Rowy.MABL_K - Rowy.N_MOIN) * CL_HESABDARI.GetArzesh(Rowy.CODE) / 100)))
                        {
                            Rowy.IMBAA = Math.Round((double)((Rowy.MABL_K - Rowy.N_MOIN) * CL_HESABDARI.GetArzesh(Rowy.CODE) / 100));
                        }
                    }
                    else if (Rowy.IMBAA != 0)
                    {
                        if (IsSingleCurrentRow)
                        {
                            Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                            msgwin.ShowDialog();
                            if (msgwin.DialogResult is true)
                            {
                                Rowy.IMBAA = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                Rowy.IMBAA = 0;
            }

            if (ErrosMessages.Count > 0 && DoShoeMessages)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
            }
        }

        private void INVO_LST_sub_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null) { return; }

            var ROW = e.Row.Item as INVO_LST_FACTOR22;
            if (ConstructorRowDetector.IsPristine(ROW)) { INVO_LST_SUB_CANCEL_EDIT(INVO_LST_sub, default); return; }

            if (!IsRowValid(ROW))
            {
                INVO_LST_SUB_CANCEL_EDIT(INVO_LST_sub, default);
                return;
            }

            if (!RowValuesCheck(ROW))
            {
                INVO_LST_SUB_CANCEL_EDIT(INVO_LST_sub, default);
                return;
            }

            VAHED_K_AfterUpdate(ROW);

            DoExportyPricesCalculate(false, null);

            string _qre = null;
            var MasterTopErrorMessages = new List<MsgModel>();

            List<MsgModel> WarningMessages = new List<MsgModel>();
            //اگر اطلاعات صحیح نیست خارج شو و واقعا ذخیره نکن
            //BodyIsValidReCompute ***
            {
                IVM.StartTransaction(); // Start the transaction again if is disposed before ****************************************************************

                List<MsgModel> ErrosMessages = new List<MsgModel>();

                bool CurrentRowisNew = true;
                if (ROW.id is null || ROW.id <= 0) //INSERT
                {
                    _qre = $@"INSERT INTO dbo.INVO_LST(NUMBER, TAG, ANBAR, RADIF, CODE, MEGH, MEGHk, MEGH_MAR, MANDAH, MABL, MABL_K, FROM_A, N_RASID, MEGH_R, RADAH, SANAD_NO, CUST_NO, ANBARF, VAHED_K, N_KOL, N_MOIN, N_TAF, AVRAGE, AVRAGE2, IMBAA, TOTALARZ, VISITOR, TKHN, JAY, JAYO)
                              OUTPUT INSERTED.id
                              VALUES({NUMBER.Text},
                              {hTAG} ,
                              {ROW.ANBAR}   ,
                              NULL,
                              N'{ROW.CODE}' ,
                              {ROW.MEGH} ,
                              {ROW.MEGHk} ,
                              {(ROW.MEGH_MAR is null ? "NULL" : ROW.MEGH_MAR)} ,
                              N'{ROW.MANDAH}' ,
                              {ROW.MABL} ,
                              {ROW.MABL_K} ,
                              0,
                              N'{(ROW.N_RASID is null ? "NULL" : ROW.N_RASID)}' ,
                              {(ROW.MEGH_R is null ? "NULL" : ROW.MEGH_R)} ,
                              {(ROW.RADAH is null ? "NULL" : ROW.RADAH)} ,
                              {(ROW.SANAD_NO is null ? "NULL" : ROW.SANAD_NO)} ,
                              NULL,
                              {(ROW.ANBARF is null ? "NULL" : ROW.ANBARF)} ,
                              {ROW.VAHED_K}   ,
                              {(ROW.N_KOL is null ? "NULL" : ROW.N_KOL)} ,
                              {(ROW.N_MOIN is null ? "NULL" : ROW.N_MOIN)} ,
                              {(ROW.N_TAF is null ? "NULL" : ROW.N_TAF)} ,
                              {(ROW.AVRAGE is null ? "NULL" : ROW.AVRAGE)} ,
                              {(ROW.AVRAGE2 is null ? "NULL" : ROW.AVRAGE2)} ,
                              {ROW.IMBAA} ,
                              {(ROW.TOTALARZ is null ? "NULL" : ROW.TOTALARZ)} ,
                              N'{(ROW.VISITOR is null ? "NULL" : ROW.VISITOR)}' ,
                              {ROW.TKHN} ,
                              {(ROW.JAY?.ToString() is null ? "NULL" : ROW.JAY.ToString())}   ,
                              {(ROW.JAYO?.ToString() is null ? "NULL" : ROW.JAYO.ToString())} )";

                    var (errorMsgs, infoMsgs, invDetails, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<long>(new List<object> { ROW }, _qre, null, false);
                    ErrosMessages.AddRange(errorMsgs);


                    if (queryOutputs.Any())
                    {
                        ROW.id = queryOutputs.FirstOrDefault(); // Update the list with the new ID
                                                                //اصلاح شماره ردیف
                        IVM.TM.ExecuteSqlCommandCtc($"UPDATE dbo.INVO_LST SET RADIF = (SELECT ISNULL(MAX(RADIF) + 1, 1) AS NewRADIF FROM dbo.INVO_LST WHERE NUMBER={NUMBER.Text} AND TAG={hTAG}) FROM dbo.INVO_LST WHERE id = {ROW.id}");
                    }
                }
                else //UPDATE
                {
                    CurrentRowisNew = false;

                    _qre = $@"UPDATE dbo.INVO_LST
                   SET ANBAR = {ROW.ANBAR}, CODE = N'{ROW.CODE}',
                   MEGH = {ROW.MEGH}, MEGHk = {ROW.MEGHk}, MEGH_MAR = {(ROW.MEGH_MAR is null ? "NULL" : ROW.MEGH_MAR)},
                   MANDAH = N'{ROW.MANDAH}', MABL = {ROW.MABL}, MABL_K = {ROW.MABL_K},
                   N_RASID = N'{(ROW.N_RASID is null ? "NULL" : ROW.N_RASID)}',
                   MEGH_R = {(ROW.MEGH_R is null ? "NULL" : ROW.MEGH_R)}, 
                   RADAH = {(ROW.RADAH is null ? "NULL" : ROW.RADAH)}, 
                   SANAD_NO = {(ROW.SANAD_NO is null ? "NULL" : ROW.SANAD_NO)},
                   ANBARF = {(ROW.ANBARF is null ? "NULL" : ROW.ANBARF)}, 
                   VAHED_K = {ROW.VAHED_K}, N_KOL = {(ROW.N_KOL is null ? "NULL" : ROW.N_KOL)}, 
                   N_MOIN = {(ROW.N_MOIN is null ? "NULL" : ROW.N_MOIN)}, N_TAF = {(ROW.N_TAF is null ? "NULL" : ROW.N_TAF)},
                   AVRAGE = {(ROW.AVRAGE is null ? "NULL" : ROW.AVRAGE)},
                   AVRAGE2 = {(ROW.AVRAGE2 is null ? "NULL" : ROW.AVRAGE2)}, IMBAA = {ROW.IMBAA}, 
                   TOTALARZ = {(ROW.TOTALARZ is null ? "NULL" : ROW.TOTALARZ)}, VISITOR = N'{(ROW.VISITOR is null ? "NULL" : ROW.VISITOR)}',
                   TKHN = {ROW.TKHN}, JAY = {(ROW.JAY?.ToString() is null ? "NULL" : ROW.JAY.ToString())}, JAYO = {(ROW.JAYO?.ToString() is null ? "NULL" : ROW.JAYO.ToString())}
                   WHERE id = {ROW.id}";

                    var (errorMsgs, infoMsgs, invDetails, queryOutputs) = IVM.CheckInventoryAndExecuteQuery<int>(new List<object> { ROW }, _qre, null, false);
                    ErrosMessages.AddRange(errorMsgs);
                }

                //Validations: بررسی صحیح بودن اولیه فیلد ها

                //بررسی محاسباتی*
                double min = default;
                double MAND;
                var MEGHTAA = default(long);
                var MEGHJAYY = default(long);
                var VAHEDD = default(long);
                //گرفتن مقادیر جایزه و حداقل موجودی کالا
                var RST = IVM.TM.SqlQueryCtc<MG_MODEL1>("SELECT MEGHJAY,MEGHTA,VAHED FROM STUF_DEF WHERE CODE = '" + ROW.CODE + "'").ToList();
                if (RST.Count > 0)
                {
                    MEGHJAYY = (long)RST.FirstOrDefault().MEGHJAY;
                    MEGHTAA = (long)RST.FirstOrDefault().MEGHTA;
                    VAHEDD = (long)RST.FirstOrDefault().VAHED;
                    min = CL_HESABDARI.Getmin((int)ROW.ANBAR, ROW.CODE);
                }

                //انبار خالی نباشد
                if (ROW?.ANBAR is null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"اطلاعات ناقص است انبار و كالا نمي تواند داراي مقدار خالي باشد {ROW.ANBAR}." });
                }
                //بررسی تعلق انبار و کالا به هم
                else if (IsNull(ROW.CODE))
                { }
                else
                {
                    var RST_STUF_STK = IVM.TM.SqlQueryCtc<STUF_STK_CSHARP>("SELECT * FROM STUF_STK WHERE CODE = '" + ROW.CODE + "' AND ANBAR = " + ROW.ANBAR).ToList();
                    if (RST_STUF_STK.Count == 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"كالا {ROW.CODE} به انبار {ROW.ANBAR} فوق تعلق ندارد." });
                    }
                }

                //بررسی صحیح بودن واحد کالا نسبت به خود کالا
                var RSTV1 = IVM.TM.SqlQueryCtc<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + ROW.CODE + "' AND ((VAHEDS.VAHED)= " + ROW.VAHED_K + ")))").ToList();
                if (RSTV1.Count == 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد." });

                    ROW.VAHED_K = null;
                }
                //واحد کالا بررسی مقدار کل باتوجه به نسبت
                else
                {
                    var NesbatMegh = RSTV1.FirstOrDefault().NESBAT * ROW.MEGH;
                    if (NesbatMegh != ROW.MEGHk)
                    {

                        ROW.MEGHk = NesbatMegh;
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مقدار کل {NesbatMegh} اصلاح کردم " });
                    }
                }

                //مقدار كالا نمي تواند صفر باشد بر اسا تنظیمات بیشتر
                if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                {
                    if (ROW.MEGH == 0)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار كالا نمي تواند صفر باشد." });
                    }
                }

                bool isJayzeh = (Convert.ToDouble(ROW?.JAY) > 0); //آیا این سطر جاری جایزه هست ؟

                //محاسبه مبلغ بر اساس تنظیمات نمایش قیمیت پیش فرض در مشخصات سیستم
                if (Baseknow.GHAYM == 7 && MODAT_PPID.SelectedIndex != 0 && PEID.SelectedValue != null && PEPID.SelectedValue != null)
                {
                    var _PEID_ = Convert.ToInt32(PEID.SelectedValue);
                    var _PEPID_ = Convert.ToInt32(PEPID.SelectedValue);

                    double? mabl_gheymat = CL_HESABDARI.GETGHeymatKala(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_, success =>
                    {
                        // This lambda expression is the Action<bool> callback
                        // It is called after the price retrieval logic in GETGHeymatKala
                        if (success)
                        {
                            // If the method was successful, this block is executed
                        }
                        else
                        {
                            // If the method was not successful, this block is executed
                        }
                    });

                    double? nkol_gheymat = (double?)CL_HESABDARI.GETTaghfifKala1(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_);
                    double? tkhn_gheymat = (double?)CL_HESABDARI.GETTaghfifKala2(Convert.ToInt32(NUMBER.Text), 13, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), ROW.CODE, _PEID_, _PEPID_);

                    if (ROW.MABL != mabl_gheymat && !isJayzeh)
                    {
                        ROW.MABL = mabl_gheymat;
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {mabl_gheymat} اصلاح کردم " });
                    }
                    if (ROW.N_KOL != nkol_gheymat)
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به تخفیف {nkol_gheymat} اصلاح کردم " });
                        ROW.N_KOL = nkol_gheymat;
                        ROW.TKHN = tkhn_gheymat;
                    }
                    var RSTC0 = IVM.TM.SqlQueryCtc<_VT_>($"SELECT TOP(1) VAHED FROM STUF_DEF WHERE CODE = N'{ROW.CODE}' ORDER BY VAHED").ToList();
                    if (RSTC0.Count > 0)
                    {
                        ROW.VAHED_K = RSTC0.FirstOrDefault().VAHED;
                        if (Strings.Mid(Baseknow.OPTIONSS, 27, 1) == "5")
                        {
                            ROW.MANDAH = RSTC0.FirstOrDefault().TOZIH;
                        }
                    }
                }
                else
                {
                    if (Baseknow.GHAYM == 1 && !isJayzeh)
                    {
                        var RSTC1 = IVM.TM.SqlQueryCtc<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + ROW.CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                        if (IsNull(RSTC1?.MABL))
                        {
                        }
                        else
                        {
                            if (ROW.MABL != RSTC1.MABL)
                            {
                                ROW.MABL = RSTC1.MABL;
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با **مبلغ** {ROW.MABL} مغایرت داشت و من آنرا به مبلغ  {RSTC1.MABL} اصلاح کردم " });
                            }
                            var _mblk = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                            if (_mblk != ROW.MABL_K)
                            {
                                ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mblk} اصلاح کردم " });
                            }
                        }
                    }
                    else if (Baseknow.GHAYM == 2 && !isJayzeh)
                    {
                        var RSTC2 = IVM.TM.SqlQueryCtc<double?>($"SELECT TOP(1) MABL_F FROM STUF_DEF WHERE CODE = N'{ROW.CODE}' ORDER BY VAHED").ToList();
                        if (RSTC2.Count == 0)
                        {
                        }
                        else
                        {
                            if (ROW.MABL != RSTC2.FirstOrDefault())
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با **مبلغ** {ROW.MABL} مغایرت داشت و من آنرا به مبلغ  {RSTC2.FirstOrDefault()} اصلاح کردم " });
                                ROW.MABL = RSTC2.FirstOrDefault();
                            }
                            var _mblk = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                            if (_mblk != ROW.MABL_K)
                            {
                                ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mblk} اصلاح کردم " });
                            }
                        }
                    }
                    else if (Baseknow.GHAYM == 4 && !isJayzeh)
                    {
                        var RSTC3 = IVM.TM.SqlQueryCtc<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = N'" + ROW.CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + this.NUMBER.Text + ") ORDER BY dbo.INVO_LST.NUMBER DESC").ToList();
                        if (RSTC3.Count > 0 && !IsNull(RSTC3?.FirstOrDefault()?.MABL))
                        {
                            if (ROW.MABL != RSTC3.FirstOrDefault().MABL && !isJayzeh)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با **مبلغ** {ROW.MABL} مغایرت داشت و من آنرا به مبلغ  {RSTC3.FirstOrDefault().MABL} اصلاح کردم " });

                                ROW.MABL = RSTC3.FirstOrDefault().MABL;
                            }
                            var _mblk = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                            if (_mblk != ROW.MABL_K && !isJayzeh)
                            {
                                ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mblk} اصلاح کردم " });
                            }
                        }
                        else
                        {
                            //ErrosMessages.Add(new MsgModel { MessageText_U = $"اين كالا {ROW.NAME_CODE} با کد {ROW.CODE} قبلا به اين شخص فروخته نشده است." });
                            //ROW.MABL = 0;
                            //ROW.MABL_K = 0; //برای اجازه ذخیره غیر فعال شده
                        }
                    }
                    else if (Baseknow.GHAYM == 5 && !isJayzeh)
                    {
                        var RSTC4 = IVM.TM.SqlQueryCtc<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + ROW.CODE + "')").ToList();
                        if (RSTC4.Count > 0)
                        {
                            if (ROW.N_KOL != RSTC4.FirstOrDefault().TAFPER)
                            {
                                ROW.N_KOL = RSTC4.FirstOrDefault().TAFPER;
                            }
                            if (ROW.MABL != RSTC4.FirstOrDefault().PRICE_M && RSTC4.FirstOrDefault().PRICE_M != 0)
                            {
                                ROW.MABL = RSTC4.FirstOrDefault().PRICE_M;
                            }

                            var _mblk = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                            if (_mblk != ROW.MABL_K)
                            {
                                ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mblk} اصلاح کردم " });
                            }
                        }
                        else
                        {
                            //universControl.PopNotifyShow($"اين كالا {ROW.NAME_CODE} با کد {ROW.CODE} داراي قيمت مصوب نيست است.", Pop1, Pop1Text1, Pop_Border1);
                            WarningMessages.Add(new MsgModel { MessageText_U = $"اين كالا {ROW.NAME_CODE} با کد {ROW.CODE} داراي قيمت مصوب نبود." });
                            //ROW.MABL = 0;
                            //ROW.MABL_K = 0;
                        }
                    }
                    if (Baseknow.TKHF == 2 && !isJayzeh)
                    {
                        var RSTC5 = IVM.TM.SqlQueryCtc<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + ROW.CODE + "')").ToList();
                        if (RSTC5.Count > 0)
                        {
                            var _nkol = RSTC5.FirstOrDefault().TAFPER;
                            if (ROW.N_KOL != _nkol)
                            {
                                ROW.N_KOL = RSTC5.FirstOrDefault().TAFPER;
                                ErrosMessages.Add(new MsgModel { MessageText_U = $"تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به تخفیف {_nkol} اصلاح کردم " });
                            }
                            if (Baseknow.GHAYM == 5)
                            {
                                if (ROW.MABL != RSTC5.FirstOrDefault().PRICE_M && RSTC5.FirstOrDefault().PRICE_M != 0)
                                {
                                    var pricem = RSTC5.FirstOrDefault().PRICE_M;
                                    if (ROW.MABL != pricem)
                                    {
                                        ROW.MABL = RSTC5.FirstOrDefault().PRICE_M;
                                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ {pricem} اصلاح کردم " });
                                    }
                                }
                                if (ROW.MABL_K != Math.Round((double)(ROW.MABL * ROW.MEGHk)))
                                {
                                    var _mablk = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                    if (ROW.MABL_K != _mablk)
                                    {
                                        ROW.MABL_K = Math.Round((double)(ROW.MABL * ROW.MEGHk));
                                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ کل این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ کل {_mablk} اصلاح کردم " });
                                    }
                                }
                            }
                        }
                    }
                }

                //محاسبه مجدد مبلغ تخفیف و در صورت تفاوت بروز کردن آن
                #region TAKHFIF_MABL_N_MOIN 
                // مبلغ تخفیف N_MOIN_AfterUpdate
                //if (ROW?.MABL_K > 0)
                //{
                //    ROW.N_KOL = ROW?.N_MOIN * 100 / ROW?.MABL_K;
                //    ROW.TKHN = 0;
                //}
                if (ROW?.N_KOL > 0) //درصد تخفیف در دیتاگرید N_KOL_AfterUpdate
                {
                    var _N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                    if (_N_MOIN != ROW.N_MOIN)
                    {
                        ROW.N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ تخفیف {ROW.N_MOIN} اصلاح کردم " });
                    }
                }
                else if (ROW?.TKHN > 0) // ت.ن% TKHN_AfterUpdate
                {
                    var _N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));

                    if (_N_MOIN != ROW.N_MOIN)
                    {
                        ROW.N_MOIN = Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100)) + Math.Round((double)((ROW?.MABL_K - Math.Round((double)(ROW?.N_KOL * ROW?.MABL_K / 100))) * ROW?.TKHN / 100));
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ تخفیف {ROW.N_MOIN} اصلاح کردم " });
                    }
                }

                ////محاسبه مجدد مبلغ تخفیف و در صورت تفاوت بروز کردن آن
                //if (ROW.N_MOIN != Math.Round((double)(ROW.N_KOL * ROW.MABL_K / 100)) + Math.Round((double)((ROW.MABL_K - Math.Round((double)(ROW.N_KOL * ROW.MABL_K / 100))) * ROW.TKHN / 100)))
                //{
                //    ROW.N_MOIN = Math.Round((double)(ROW.N_KOL * ROW.MABL_K / 100)) + Math.Round((double)((ROW.MABL_K - Math.Round((double)(ROW.N_KOL * ROW.MABL_K / 100))) * ROW.TKHN / 100));
                //    ErrosMessages.Add(new MsgModel { MessageText_U = $"مبلغ تخفیف این سطر کالا با این مشخصات : کد کالا {ROW.CODE} به مقدار کل {ROW.MEGHk} با مبلغ {ROW.MABL} مغایرت داشت و من آنرا به مبلغ تخفیف {ROW.N_MOIN} اصلاح کردم " });
                //}
                #endregion

                // Handle error messages
                if (ErrosMessages.Any())
                {
                    IVM.RollbackTransaction(); // Rollback the transaction if there are errors
                    if (CurrentRowisNew)
                    {
                        ROW.id = null; //Bring Back to null (New State because of Rollback Transaction)
                    }
                }
                else
                {
                    IVM.CommitTransaction(); // Commit the transaction if no errors
                }

                MasterTopErrorMessages.AddRange(ErrosMessages); //Add Error to Top Message to show it later after this loop 
            }

            // Handle error messages
            if (MasterTopErrorMessages.Any())
            {
                IVM.ShowErrorMessages(MasterTopErrorMessages);
                return;
            }

            // بروز رسانی قیمت و تخفیف
            #region CUST_KIND_AfterUpdate_UPDATE_GHEYMAT
            if ((this.SGN1.IsChecked is false && this.SGN2.IsChecked is false && this.SGN3.IsChecked is false) && !IsNull(this.MODAT_PPID.SelectedValue) && !IsNull(this.CUST_KIND.SelectedValue) && !IsNull(this.DEPATMAN.SelectedValue) && Baseknow.GHAYM == 7 && this.MODAT_PPID.SelectedIndex > 0)
            {
                GoGheymateUpdator();
            }
            else
            {
                var _RST2_MABL = 0d;
                var _RST2_MABL_K = 0d;
                var _RST2_N_KOL = 0d;
                var _RST2_N_MOIN = 0d;

                var RST2 = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST WHERE TAG = 2 AND JAY = 0  and NUMBER = " + NUMBER.Text).ToList();
                for (int i = 0; i < RST2.Count; i++)
                {
                    if (Baseknow.GHAYM == 1)
                    {
                        var rstx = dbms.DoGetDataSQL<QRE_MX>("SELECT Max(INVO_LST.NUMBER) AS MaxOfNUMBER, INVO_LST.MABL FROM INVO_LST WHERE (((INVO_LST.TAG) = 2) And ((INVO_LST.CODE) = '" + RST2[i].CODE + "')) GROUP BY INVO_LST.MABL").FirstOrDefault();
                        if (IsNull(rstx?.MABL))
                        {
                        }
                        else
                        {
                            RST2[i].MABL = rstx.MABL;
                            RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                            _RST2_MABL = (double)RST2[i].MABL;
                            _RST2_MABL_K = (double)RST2[i].MABL_K;
                        }
                    }
                    else if (Baseknow.GHAYM == 2)
                    {
                        var _Filter = "CODE = N'" + RST2[i].CODE + "'";
                        var rstf = dbms.DoGetDataSQL<STUF_DEF_CSHARP>($"SELECT CODE, NAME, N_FANI, TOZIH, VAHED, B_SEF, N_SEF, MIN_M, MAX_M, RADAH, KINDK, MABL_F, DEPART, IDD, CMBAA, VAZN, OKF, MENUIT, MEGHTA, MEGHJAY, PGID, BARCODE, CRT, UID FROM STUF_DEF {_Filter} ").FirstOrDefault();
                        if ((rstf is null))
                        {
                        }
                        else
                        {
                            RST2[i].MABL = rstf.MABL_F;
                            RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                            _RST2_MABL = (double)RST2[i].MABL;
                            _RST2_MABL_K = (double)RST2[i].MABL_K;
                        }
                    }
                    else if (Baseknow.GHAYM == 4)
                    {
                        var rstr = dbms.DoGetDataSQL<QRE_MX>("SELECT     TOP 100 PERCENT dbo.INVO_LST.NUMBER AS MaxOfNUMBER, dbo.INVO_LST.MABL FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG AND dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.CODE = N'" + RST2[i].CODE + "') AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.INVO_LST.MABL <> 0) AND  (dbo.INVO_LST.NUMBER < " + RST2[i].NUMBER + ") ORDER BY dbo.INVO_LST.NUMBER DESC").FirstOrDefault();

                        if (rstr != null)
                        {
                            RST2[i].MABL = rstr.MABL;
                            RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));

                            _RST2_MABL = (double)RST2[i].MABL;
                            _RST2_MABL_K = (double)RST2[i].MABL_K;
                        }
                        else
                        {
                            Msgwin msgwin = new Msgwin(false, "اين كالا قبلا به اين شخص فروخته نشده است.");
                            msgwin.ShowDialog();

                            _RST2_MABL = (double)RST2[i].MABL;
                            _RST2_MABL_K = (double)RST2[i].MABL_K;
                        }
                    }
                    else if (Baseknow.GHAYM == 5)
                    {
                        var rstc = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + RST2[i].CODE + "')").FirstOrDefault();
                        if (!(rstc is null))
                        {
                            if (RST2[i].N_KOL != rstc.TAFPER)
                            {
                                RST2[i].N_KOL = rstc.TAFPER;
                                _RST2_N_KOL = (double)RST2[i].N_KOL;
                            }
                            if (RST2[i].MABL != rstc.PRICE_M && rstc.PRICE_M != 0)
                            {
                                RST2[i].MABL = rstc.PRICE_M;
                                _RST2_MABL = (double)RST2[i].MABL;
                            }
                            if (RST2[i].MABL_K != Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk)))
                            {
                                RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));
                                _RST2_MABL_K = (double)RST2[i].MABL_K;
                            }
                        }
                        else
                        {
                            RST2[i].MABL = 0;
                            RST2[i].MABL_K = 0;
                            _RST2_MABL = (double)RST2[i].MABL;
                            _RST2_MABL_K = (double)RST2[i].MABL_K;
                        }
                    }

                    if (Baseknow.TKHF == 2)
                    {
                        var rsttt = dbms.DoGetDataSQL<TAKHPERS_CSHARP>("SELECT     CUST_CO, TAKH_COD, TAFPER,PRICE_M FROM dbo.TAKHPERS WHERE     (CUST_CO = " + CUST_KIND.SelectedValue + ") AND (TAKH_COD = N'" + RST2[i].CODE + "')").FirstOrDefault();
                        if (!(rsttt is null))
                        {
                            RST2[i].N_KOL = rsttt.TAFPER;
                            _RST2_N_KOL = (double)RST2[i].N_KOL;

                            if (Baseknow.GHAYM == 5)
                            {
                                if (RST2[i].MABL != rsttt.PRICE_M && rsttt.PRICE_M != 0)
                                {
                                    RST2[i].MABL = rsttt.PRICE_M;
                                    _RST2_MABL = (double)RST2[i].MABL;
                                }
                                if (RST2[i].MABL_K != Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk)))
                                {
                                    RST2[i].MABL_K = Math.Round((double)(RST2[i].MABL * RST2[i].MEGHk));
                                    _RST2_MABL_K = (double)RST2[i].MABL_K;
                                }
                            }
                        }
                    }
                    RST2[i].N_MOIN = Math.Round((double)(RST2[i].N_KOL * RST2[i].MABL_K / 100));
                    _RST2_N_MOIN = (double)RST2[i].N_MOIN;
                }
                CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), 2);
                CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text ?? "0"), Convert.ToDouble(MABL_VAR.Text ?? "0"), Convert.ToDouble(MABL_HAV.Text ?? "0"), (bool)(TICMBAA.IsChecked ?? false));
                if (Baseknow.TKHF == 3 || Baseknow.TKHF == 2)
                {
                    var rst = dbms.DoGetDataSQL<double?>("SELECT SUM(N_MOIN) AS JAMT FROM INVO_LST WHERE NUMBER = " + this.NUMBER.Text + " AND TAG = 2").ToList();
                    if (rst.Count > 0)
                    {
                        if (rst.FirstOrDefault() != Convert.ToDouble(TAKHFIF.Text) && !IsNull(rst.FirstOrDefault()))
                        {
                            this.TAKHFIF.Text = rst.FirstOrDefault().ToStringNullSafe();
                        }
                    }
                    else
                    {
                        this.TAKHFIF.Text = "0";
                    }
                }
                if (Convert.ToBoolean(TICMBAA.IsChecked))
                {
                    var rst = dbms.DoGetDataSQL<double?>("SELECT SUM(IMBAA) AS JAMIMBAA FROM INVO_LST WHERE NUMBER = " + this.NUMBER.Text + " AND TAG = 2").ToList();
                    if (rst.Count > 0)
                    {
                        if (rst.FirstOrDefault() != Convert.ToDouble(MBAA.Text) && !IsNull(rst.FirstOrDefault()))
                        {
                            if (!string.IsNullOrEmpty(rst.FirstOrDefault().ToStringNullSafe()))
                            {
                                this.MBAA.Text = rst.FirstOrDefault().ToStringNullSafe();
                            }
                            this.HMBAA.Text = Baseknow.HESMBAA;
                        }
                        else if (IsNull(rst.FirstOrDefault()))
                        {
                            this.MBAA.Text = "0";
                            this.HMBAA.Text = "";
                            this.HMBAA.Text = "";
                        }
                    }
                    else if (Convert.ToDouble(MBAA.Text) != 0)
                    {
                        this.MBAA.Text = "0";
                        this.HMBAA.Text = "";
                    }
                }
                else if (Convert.ToDouble(MBAA.Text) != 0)
                {
                    this.MBAA.Text = "0";
                    this.HMBAA.Text = "";
                }
                if (Convert.ToBoolean(JAY.IsChecked))
                {
                    if (this.LETSANAD)
                    {
                        JAY_AfterUpdate();
                    }
                }
            }
            #endregion

            if (WarningMessages.Any()) //نمایش هشدار
            {
                universControl.PopNotifyShowUp(WarningMessages?.FirstOrDefault()?.MessageText_U, Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
            }

            Summer();

            UpdateVisitorCommissions(); //بروز رسانی 

            SANAD();

            MasterSummerAndMandeh();

            ChangeIsHappend = true;
        }

        private bool RowValuesCheck(INVO_LST_FACTOR22? CurrentRow)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!string.IsNullOrEmpty(WAS_ROW_ITEM?.CODE) && CurrentRow?.CODE != WAS_ROW_ITEM?.CODE) //اگر کالار وعوض کرده کرده ولی انبارش مطابق اون نیست
            {
                var RSTD0 = dbms.DoGetDataSQL<STUF_STK_CSHARP>("SELECT TOP 1 ANBAR FROM STUF_STK WHERE CODE = '" + WAS_ROW_ITEM.CODE + "' AND ANBAR = " + CurrentRow.ANBAR).FirstOrDefault();
                if (RSTD0 == null)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "اطلاعات در مورد اين كالا مغايرت دارد." });
                    CurrentRow.CODE = WAS_ROW_ITEM.CODE;
                }
                else
                {
                    RSTD0.MOGODI = RSTD0.MOGODI + WAS_ROW_ITEM.MEGHk - CurrentRow.MEGH_MAR;
                    WAS_ROW_ITEM.MEGHk = 0;
                }
            }

            LETSANAD = true;

            //چک کردن واحد کالا
            VAHED_K_NESBAT_2 RSTV1 = dbms.DoGetDataSQL<VAHED_K_NESBAT_2>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + CurrentRow.CODE + "' AND ((VAHEDS.VAHED)= " + CurrentRow.VAHED_K + ")))").FirstOrDefault();
            if (RSTV1?.CODE == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "واحد تعريف شده ناقص ميباشد نسبت آن مشخص نگرديده است.در بخش تعريف كالا آن را اصلاح كنيد" });
                CurrentRow.VAHED_K = null;
            }
            else
            {
                CurrentRow.MEGHk = CurrentRow.MEGH * RSTV1?.NESBAT;
                if (CurrentRow.MABL > 0)
                {
                    CurrentRow.MABL_K = Math.Round((double)(CurrentRow.MABL * CurrentRow.MEGHk));
                }
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }

            return true;
        }

        private void INVO_LST_sub_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
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
        private void INVO_LST_sub_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null) && INVO_LST_sub.SelectedItem is not null)
            {
                if (e == null || !(e.Row.Item is INVO_LST_FACTOR22 rowItem)) return;
                if (rowItem == null) return;
                if (Equals(e.Row.Item, CollectionView.NewItemPlaceholder)) return;
                var view = INVO_LST_sub.Items as IEditableCollectionView;
                if (view.IsAddingNew) { return; }

                if (CL_LMethods.IsNewPlaceHolder(INVO_LST_sub, INVO_LST_sub.SelectedItem))
                {
                    return;
                }

                WAS_ROW_ITEM = ((INVO_LST_FACTOR22)INVO_LST_sub.SelectedItem).Clone() as INVO_LST_FACTOR22;
            }
        }


        private void INVO_LST_sub_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                IsFocusInsideOfINVO_LST_sub = false;
            }
            else //Is Focus inside of INVO_LST_sub
            {
                IsFocusInsideOfINVO_LST_sub = true;


            }
        }
        private void INVO_LST_sub_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region Form_Current_ONSUB
            //this.CODE.RowSource = "SELECT STUF_DEF.CODE, STUF_DEF.NAME, STUF_DEF.CODE FROM STUF_DEF ORDER BY STUF_DEF.NAME";
            //this.VAHED_K.RowSource = "SELECT VAHEDS.VAHED, TCOD_VAHEDS.NAMES FROM TCOD_VAHEDS INNER JOIN VAHEDS ON TCOD_VAHEDS.CODE = VAHEDS.VAHED GROUP BY VAHEDS.VAHED, TCOD_VAHEDS.NAMES ORDER BY TCOD_VAHEDS.NAMES";
            chek = false;
            khaly = false;
            //var RECORD = INVO_LST_sub.Items[CURRENT_ROW_INDEX] as INVO_LST_FACTOR22;
            if (INVO_LST_sub.SelectedIndex > -1)
            {
                var RECORD = INVO_LST_sub.Items[INVO_LST_sub.SelectedIndex] as INVO_LST_FACTOR22;
                if (!(RECORD is null)) //NOT NULL
                {
                    if (RECORD.id is null || RECORD.id is 0)
                    {
                        WAS_ROW_ITEM.CODE/*.TAG*/ = "";
                        WAS_ROW_ITEM.VAHED_K/*.TAG*/ = 0;
                        WAS_ROW_ITEM.MEGH/*.TAG*/ = 0;
                        WAS_ROW_ITEM.MEGHk/*.TAG*/ = 0;
                        WAS_ROW_ITEM.MABL/*.TAG*/ = 0;
                        WAS_ROW_ITEM.MABL_K/*.TAG*/ = 0;
                    }
                    else
                    {
                        WAS_ROW_ITEM.CODE/*.TAG*/ = RECORD.CODE;
                        WAS_ROW_ITEM.VAHED_K/*.TAG*/ = RECORD.VAHED_K;
                        WAS_ROW_ITEM.MEGH/*.TAG*/= RECORD.MEGH;
                        WAS_ROW_ITEM.MEGHk/*.TAG*/ = RECORD.MEGHk;
                        WAS_ROW_ITEM.MABL/*.TAG*/= RECORD.MABL;
                        WAS_ROW_ITEM.MABL_K/*.TAG*/= RECORD.MABL_K;
                        var RST = dbms.DoGetDataSQL<QRE_FAC_02>("SELECT CODE,ANBAR,MOGODI,MOGODI_A FROM STUF_STK WHERE CODE = '" + RECORD.CODE + "' AND ANBAR = " + RECORD.ANBAR).ToList();
                        if (RST.Count == 0)
                        {
                            MOGU.Text = null;
                        }
                        else
                        {
                            MOGU.Text = Convert.ToString(RST.FirstOrDefault().MOGODI + RST.FirstOrDefault().MOGODI_A);
                        }
                    }
                    if (Baseknow.GHAYM == 7)
                    {
                        //if (Forms["HEAD_LST_FROOSH22"]["MODAT_PPID"] == 0)
                        if (MODAT_PPID.SelectedIndex == 0)
                        {
                            this.MABL_COLUMN.IsReadOnly = false;
                            this.MABL_K_COLUMN.IsReadOnly = false;
                            this.N_KOL_COLUMN.IsReadOnly = false;
                            this.N_MOIN_COLUMN.IsReadOnly = false;
                            this.TKHN_COLUMN.IsReadOnly = false;
                        }
                        else
                        {
                            this.MABL_COLUMN.IsReadOnly = true;
                            this.MABL_K_COLUMN.IsReadOnly = true;
                            this.N_MOIN_COLUMN.IsReadOnly = true;
                            this.N_MOIN_COLUMN.IsReadOnly = true;
                            this.TKHN_COLUMN.IsReadOnly = true;
                        }
                    }
                }
            }

            #endregion



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public void ReGetdata()
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
                                                                    WHERE        (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER={NUMBER.Text})").ToList();

                FACTOR22_INVO_DATA?.Clear();
                foreach (var item in QRE_LST)
                    FACTOR22_INVO_DATA.Add(item);

                INVO_LST_sub.ItemsSource = FACTOR22_INVO_DATA;

                if (!IsDirectFactor)
                {
                    var rst = dbms.DoGetDataSQL<_FACT_HEAD_HAV_>("SELECT HEAD_LST.CUST_NO,HEAD_LST.CUST_KIND,HEAD_LST.MAS,HEAD_LST.DEPATMAN,HEAD_LST.TICMBAA,HEAD_LST.SHARAYET,HEAD_LST.FNUMCO,HEAD_LST.JAY,MODAT_PPID,PEID,PEPID,HEAD_LST.USER_NAME FROM HEAD_LST WHERE (((HEAD_LST.NUMBER) = " + NUMBER.Text + ") And ((HEAD_LST.TAG) = 2)) GROUP BY TICMBAA,HEAD_LST.CUST_KIND,HEAD_LST.MAS,HEAD_LST.FNUMCO,HEAD_LST.SHARAYET,HEAD_LST.JAY,HEAD_LST.DEPATMAN,MODAT_PPID,PEID,PEPID,HEAD_LST.USER_NAME, CUST_NO").FirstOrDefault();

                    if (rst is not null)
                    {
                        string? thevalue = rst?.CUST_NO;

                        if (!string.IsNullOrEmpty(thevalue))
                        {
                            var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT TOP 1 hes, NAME FROM dbo.CUST_HESAB WHERE HES = N'" + thevalue + "'").FirstOrDefault();

                            var itemsSource = CUST_NO.ItemsSource as List<Custom_CUST_HESAB>;
                            if (itemsSource == null)
                            {
                                itemsSource = new List<Custom_CUST_HESAB>();
                                CUST_NO.ItemsSource = itemsSource;
                            }

                            if (data != null && !itemsSource.Any(item => item?.hes == thevalue))
                            {
                                itemsSource.Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
                            }

                            CUST_NO.SelectedValue = thevalue;
                            CUST_NO.Items.Refresh();
                        }

                        PEID.SelectedValue = rst.PEID; PEID.Items.Refresh();
                        PEPID.SelectedValue = rst.PEPID; PEPID.Items.Refresh();

                        MODAT_PPID_Enter();

                        var currentList = MODAT_PPID.ItemsSource as List<PRICE_PAYNO_MODATP>;
                        if (currentList != null && !currentList.Any(x => x.PPID == rst.MODAT_PPID))
                        {
                            if (rst.MODAT_PPID == 0)
                            {
                                currentList.Add(new PRICE_PAYNO_MODATP { PPID = 0, PPAME = "آزاد", MODAT = 0, IsTempyDisplay = false });
                            }
                            else
                            {
                                if (rst.MODAT_PPID != null)
                                {
                                    var extraItem = dbms.DoGetDataSQL<PRICE_PAYNO_MODATP>($"SELECT PPID, PPAME, MODAT FROM PRICE_PAYNO WHERE PPID = {rst.MODAT_PPID}").FirstOrDefault();
                                    if (extraItem != null)
                                    {
                                        extraItem.IsTempyDisplay = true;
                                        currentList.Add(extraItem);
                                    }
                                }
                            }
                            MODAT_PPID.Items.Refresh();
                        }

                        //مدت
                        if (string.IsNullOrWhiteSpace(MAS.Text) || MAS.Text == "0")
                        {
                            MAS.Text = rst.MAS.ToString();
                        }
                        if (rst?.CUST_KIND != null)
                        {
                            CUST_KIND.SelectedValue = rst.CUST_KIND; CUST_KIND.Items.Refresh();
                        }

                        if (!string.IsNullOrWhiteSpace(rst?.SHARAYET))
                        {
                            MOLAH.Text = Strings.Left(rst.SHARAYET.ToStringNullSafe(), 200); //ملاحظات سربرگ حواله تگ 2 => SHARAYET ====== ملاحظات سربرگ فاکتور با تگ 13 => MOLAH
                        }


                        MAS_MAGHSAD_HV = (double)rst.MAS;

                        FNUMCO.Text = string.IsNullOrWhiteSpace(rst.FNUMCO.ToStringNullSafe()) ? "0" : rst.FNUMCO.ToStringNullSafe();

                        JAY.IsChecked = rst.JAY ?? false;
                        TICMBAA.IsChecked = rst.TICMBAA ?? false;

                        DEPATMAN.SelectedValue = rst.DEPATMAN; DEPATMAN.Items.Refresh();

                        MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                        MODAT_PPID.SelectedValue = null;
                        MODAT_PPID.SelectedValue = rst.MODAT_PPID; MODAT_PPID.Items.Refresh();
                        //GetModatValueDays();
                        MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

                        USER_NAME.Text = rst.USER_NAME; //نام کابری از حواله گرفته میشود در فاکتور با حواله یعنی غیر مستقیم
                        CL_HESABDARI.LOGFACT(Convert.ToDouble(NUMBER.Text), 13, Convert.ToDouble(NUMBER1.Text), "UPDATEFACTOR");
                    }

                    NUMBER.Tag = NUMBER.SelectedValue; //Save Last Valid SelectedValue
                }

                VISITOR_DTL_SUB_ReGetData();
                PAY_GETD_SUB_ReGetData();
                TAKHFIF_APLAY_ReGetData();
            }
        }

        private void INVO_LST_sub_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (INVO_LST_sub.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = INVO_LST_sub.CurrentCell.Column?.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_HAVALE_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "MABL" || CURRENT_COLUMN_NAME is "MABL_K" || CURRENT_COLUMN_NAME is "N_MOIN")
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
                if (CURRENT_COLUMN_NAME is "MABL" || CURRENT_COLUMN_NAME is "MABL_K" || CURRENT_COLUMN_NAME is "N_MOIN")
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
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.G)
            {
                e.Handled = true;
                if (Convert.ToDouble(NUMBER.Text ?? "0") > 0 && INVO_LST_sub.Items.Count > 0)
                {
                    Msgwin msgwin = new Msgwin(true, "آیا از باز کردن پنجره سایر اطلاعات مطمئن هستید؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult is true)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null);

                        if (SavedSuccessBtn)
                        {
                            OTHER_DTL win = new OTHER_DTL(2, CL_LMethods.GetTheWindow(WINDOW_ID));
                            win.NUMBER = Convert.ToInt64(NUMBER.Text);
                            win.Show();
                        }
                    }
                }
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.OemQuotes)
                {
                    try
                    {
                        if (INVO_LST_sub.IsEnabled && !INVO_LST_sub.IsReadOnly && INVO_LST_sub.CurrentCell != null)
                        {
                            // Get the current cell
                            DataGridCellInfo currentCell = INVO_LST_sub.CurrentCell;
                            if (currentCell != null)
                            {
                                // Get the row index and column index of the current cell
                                int rowIndex = INVO_LST_sub.Items.IndexOf(currentCell.Item);
                                int columnIndex = INVO_LST_sub.Columns.IndexOf(currentCell.Column);

                                // Check if it's not the first row
                                if (rowIndex > 0)
                                {
                                    // Get the value from the cell above
                                    object valueAbove = INVO_LST_sub.Items[rowIndex - 1];

                                    // Ensure that the column index is within bounds
                                    if (valueAbove != null && columnIndex >= 0 && columnIndex < INVO_LST_sub.Columns.Count)
                                    {
                                        // Get the column information
                                        var column = INVO_LST_sub.Columns[columnIndex];

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

                                                    INVO_LST_sub.BeginEdit();
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
        }
        /// <summary>
        /// تغییر تب ایدنکس ستون ها
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="PRPNAME"></param>
        private void ManageColumnsTabindex(object sender, KeyEventArgs e, string BND_NAME, bool TF)
        {
            var FOUND_COL_INDEX = INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath == BND_NAME).DisplayIndex;
            // get the current cell
            var THECELL = e.OriginalSource as DataGridCell;
            //MyDataGrid1.Columns[0].IsHitTestVisible = false;

            //CELL
            if (INVO_LST_sub.SelectedIndex > -1)
            {
                var rowContainer = INVO_LST_sub.ItemContainerGenerator.ContainerFromIndex(INVO_LST_sub.SelectedIndex) as DataGridRow;
                if (!(rowContainer is null))
                {
                    DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);

                    DataGridCell cell2 = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(FOUND_COL_INDEX);
                    if (cell2 == null)
                    {
                        INVO_LST_sub.ScrollIntoView(rowContainer, INVO_LST_sub.Columns[CURRENT_COLUMN_INDEX]);
                        THECELL = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
                    }
                    else
                    {
                        THECELL = cell2;
                    }
                    //CELL
                    if (!(THECELL is null))
                    {
                        THECELL.IsTabStop = TF;
                        //e.Handled = true;
                    }
                }
            }
        }


        private void DELETE_HAVALE_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_HAVALE.Visibility == Visibility.Visible;
            if (!DELETE_HAVALE.IsEnabled || !IsVisible) { return; }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }


            if (INVO_LST_sub.IsEnabled == false || INVO_LST_sub.IsReadOnly)
            {
                return;
            }

            bool IsDeleteSomthing = false;

            var editableCollectionView = INVO_LST_sub.Items as IEditableCollectionView;
            if (editableCollectionView != null && editableCollectionView.IsEditingItem)
            {
                return;
            }

            _ = AuditLogger.LogActionAsync(
                actionType: "DELETE",
                tableName: "فاکتور فروش",
                recordId: NUMBER1.Text,
                oldValue: "TAG = 13",
                newValue: null,
                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            //if (IsDirectFactor && INVO_LST_sub.Items.Count > 0 && INVO_LST_sub.SelectedItem != null)
            if (IsDirectFactor && FACTOR22_INVO_DATA.Count > 0)
            {
                if (INVO_LST_sub.SelectedItems.Count > 0 && !(INVO_LST_sub.SelectedItems is null))
                {
                    #region SABEGHEH
                    var dt = DateTime.Now;
                    CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
                    CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                    CL_HESABDARI.TR("PAY_GETD", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                    CL_HESABDARI.TR("TAKHFIF_APLAY", "(NUMBER = " + this.NUMBER.Text + ") AND (kind = 2)", dt, 1);
                    CL_HESABDARI.TR("OTHER_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
                    CL_HESABDARI.TR("VISITOR_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                    #endregion

                    Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < INVO_LST_sub.SelectedItems.Count; i++)
                        {
                            var item = INVO_LST_sub.SelectedItems[i];

                            if (CL_LMethods.IsNewPlaceHolder(INVO_LST_sub, item))
                            {
                                continue; // Skip deletion for new placeholder items
                            }

                            var _id_ = item.GetType().GetProperty("id").GetValue(item);

                            if (_id_ is null)
                            {
                                FACTOR22_INVO_DATA.Remove(item as INVO_LST_FACTOR22);
                            }
                            else
                            {
                                try
                                {
                                    var items = new List<object> { item }; // Wrap the item in a list
                                    var (errorMessages, infoMessages, inventoryDetails, queryOutputs) =
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
                            //return;
                        }

                        ReGetdata();
                        SANAD();

                    }
                }
            }
            else
            {
                Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟ فقط اطلاعات سربرگ مربوط به این فاکتور فروش و سند حسابداری آن حذف خواهد شد");
                msgwin.ShowDialog();
                if (msgwin.DialogResult != true)
                {
                    return;
                }

                // 2. اعتبارسنجی اولیه ورودی‌ها
                // استفاده از TryParse برای اطمینان از عددی بودن مقادیر ایمن‌تر است
                if (!long.TryParse(NUMBER.Text, out long num) || num == 0 || !long.TryParse(NUMBER1.Text, out long num1) || num1 == 0)
                {
                    new Msgwin(false, "شماره فاکتور معتبر برای حذف انتخاب نشده است.").ShowDialog();
                    return;
                }

                // پارامترهای مشترک برای کوئری‌ها (جلوگیری از SQL Injection)
                var deleteParams = new { Num = num, Num1 = num1, Tag = 13 };

                try
                {
                    // 3. تلاش اولیه برای حذف سربرگ فاکتور (Happy Path)
                    // ✅ استفاده از پارامتر بجای چسباندن رشته - امن
                    dbms.DoExecuteSQL("DELETE FROM dbo.HEAD_LST WHERE NUMBER = @Num AND NUMBER1 = @Num1 AND TAG = @Tag", deleteParams);

                    // عملیات موفقیت آمیز بوده
                    SANAD();
                    _navigationManager?.DeleteCurrentRecord(); //Refresh Record Source
                }
                catch (SqlException ex)
                {
                    // هندل کردن ایونت در صورت وجود (مربوط به کامپوننت‌های UI)
                    if (e != null) e.Handled = true;

                    // 4. بررسی خطای Constraint کلید خارجی (FK)
                    if (ex.Number == 547)
                    {
                        // آیا خطا مربوط به وابستگی به سند حسابداری (DEED_DTL) است؟
                        if (ex.Message.Contains("FK_DEED_DTL_HEAD_LST"))
                        {
                            try
                            {
                                // تشخیص دادیم که خطا بخاطر سند است.
                                // حالا تلاش برای حذف زنجیره‌ای (اول فرزند، بعد والد)
                                if (!long.TryParse(N_S.Text, out long sanadNum) || sanadNum == 0)
                                {
                                    // حالتی که خطا میدهد اما شماره سند در تکست باکس نیست (بسیار نادر)
                                    new Msgwin(false, "وابستگی به سند وجود دارد اما شماره سند مشخص نیست.").Show();
                                    return;
                                }

                                // الف) حذف سند وابسته (DEED_DTL)
                                // ✅ استفاده از پارامتر - امن
                                dbms.DoExecuteSQL("DELETE FROM dbo.DEED_DTL WHERE N_S = @NS AND NUMBER = @Num AND TAG = @Tag",
                                    new { NS = sanadNum, Num = num, Tag = 13 });

                                // ب) تلاش مجدد برای حذف سربرگ فاکتور (HEAD_LST)
                                dbms.DoExecuteSQL("DELETE FROM dbo.HEAD_LST WHERE NUMBER = @Num AND NUMBER1 = @Num1 AND TAG = @Tag", deleteParams);

                                // عملیات حذف زنجیره‌ای موفقیت آمیز بود
                                SANAD();
                                _navigationManager?.DeleteCurrentRecord();
                            }
                            catch (Exception ex2)
                            {
                                // خطای غیرمنتظره در حین عملیات حذف زنجیره‌ای
                                // پیام خطا (ex2.Message) را نگه داشتم چون برای دیباگ حیاتی است، اما شماره کد فنی ندارد
                                new Msgwin(false, $"عملیات حذف خودکار سند و فاکتور با شکست مواجه شد.").Show();
                            }
                        }
                        else
                        {
                            // خطای FK دیگری وجود دارد که مربوط به سند نیست
                            new Msgwin(false, "این فاکتور دارای اطلاعات وابسته دیگری است (غیر از سند) و قابل حذف نیست.").ShowDialog();
                        }
                    }
                    else
                    {
                        // 👈 تغییر انجام شد: حذف نمایش کد خطا به کاربر
                        new Msgwin(false, "حذف به دلیل خطا در پایگاه داده انجام نشد!").ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    // خطای عمومی غیر SQL
                    new Msgwin(false, $"خطای سیستمی در انجام عملیات حذف!").ShowDialog();
                }
            }
        }

        private void PEPID_DropDownOpened(object sender, EventArgs e)
        {
            //ComboBox cb = sender as ComboBox;
            //cb.IsDropDownOpen = false;
        }

        private void PEID_DropDownOpened(object sender, EventArgs e)
        {
            //ComboBox cb = sender as ComboBox;
            //cb.IsDropDownOpen = false;
        }

        private void SGN1usid_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;
        }

        private void SGN2usid_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;
        }

        private void SGN3usid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            cb.IsDropDownOpen = false;
        }

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

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!ESLAH.IsEnabled || ESLAH.Visibility != Visibility.Visible || !ESLAH.IsHitTestVisible) { return; }

            if (!string.IsNullOrWhiteSpace(NUMBER.Text) && NUMBER.Text == "0") { return; }
            if (_navigationManager.IsNewRecord) { return; }

            DateTime dt;
            if (!string.IsNullOrWhiteSpace(NUMBER.Text) && NUMBER.Text != "0")
            {
                dt = DateTime.Now;
                CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
                CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                CL_HESABDARI.TR("PAY_GETD", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
                CL_HESABDARI.TR("TAKHFIF_APLAY", "(NUMBER = " + this.NUMBER.Text + ") AND (kind = 2)", dt, 1);
                CL_HESABDARI.TR("OTHER_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
                CL_HESABDARI.TR("VISITOR_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);

                if (moadian.IsSelected) //اگر در تب مودیان فوکوس کرده بود
                {
                    AllowMoadianTabEdit(true);
                    return;
                }

                if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
                {
                    new Msgwin(false, " اول امضاء را برداريد ...").ShowDialog();
                    this.CUST_NO.IsReadOnly = true;
                    this.INVO_LST_sub.IsReadOnly = true;
                    //this.TAKHFIF_APLAY_SUB.IsReadOnly = true;
                    this.DATE_N.IsReadOnly = true;
                    this.NUMBER.IsReadOnly = true;
                    this.FNUMCO.IsReadOnly = true;
                    this.MOLAH.IsReadOnly = true;
                    this.TAH.IsReadOnly = true;
                    //New Code // this.Page58.IsEnabled = false;
                    //New Code // this.Page155.IsEnabled = false;
                    //this.moadian.IsEnabled = false;

                    BUTTON_SAVE_HAVALE.IsEnabled = false;

                    return;
                }
                else
                {
                    this.CUST_NO.IsReadOnly = false;
                    this.INVO_LST_sub.IsReadOnly = false;
                    //this.TAKHFIF_APLAY_SUB.IsReadOnly = false;
                    this.DATE_N.IsReadOnly = false;
                    this.NUMBER.IsReadOnly = false;
                    this.FNUMCO.IsReadOnly = false;
                    this.MOLAH.IsReadOnly = false;
                    this.TAH.IsReadOnly = false;
                    //New Code // this.Page58.IsEnabled = true;
                    //New Code // this.Page155.IsEnabled = true;
                    //this.moadian.IsEnabled = true;


                    DEPATMAN.IsEnabled = true;

                    BUTTON_SAVE_HAVALE.IsEnabled = true;
                }

                this.AllowDeletions = true;
                this.AllowEdits = true;
                this.INVO_LST_sub.IsReadOnly = false;
                //this.TAKHFIF_APLAY_SUB.IsEnabled = true;
                //New Code // this.Page58.IsEnabled = true;
                //New Code // this.Page155.IsEnabled = true;
                this.moadian.IsEnabled = true;

            }

            SecurityAllCheck();

            if (!string.IsNullOrEmpty(NUMBER.Text) && Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 13, Convert.ToInt32(Baseknow.USERCOD), WINDOW_ID);
            }
            else
            {
                this.SGN1.IsEnabled = false;
                this.SGN2.IsEnabled = false;
                this.SGN3.IsEnabled = false;
            }
        }

        /// <summary>
        /// بروزرسانی پورسانت ویزیتور بعد از تغییر مبلغ کالا
        /// فقط برای آیتم‌هایی که تیک "مبلغ ثابت" ندارند
        /// </summary>
        private void UpdateVisitorCommissions()
        {
            if (SAYER_VISITOR_DATA == null || SAYER_VISITOR_DATA.Count == 0)
                return;

            // اگر JF یا TAKHFIF خالی یا نامعتبر باشند، از محاسبه خارج شویم
            if (!double.TryParse(JF.Text, out double jfValue) || !double.TryParse(TAKHFIF.Text, out double takhfifValue))
                return;

            // برای هر ویزیتور که مبلغش ثابت نیست، پورسانت را بروز کنیم
            foreach (var visitor in SAYER_VISITOR_DATA)
            {
                // فقط آیتم‌هایی که STAT = false (مبلغ ثابت نیست)
                if (visitor.STAT == false && visitor.DARSAD.HasValue && visitor.ID.HasValue)
                {
                    // فرمول: PURSANT = (JF - TAKHFIF) * DARSAD / 100
                    visitor.PURSANT = Math.Round((jfValue - takhfifValue) * visitor.DARSAD.Value / 100);

                    try
                    {
                        string sql = @"UPDATE dbo.VISITOR_DTL SET 
                            NUMBER = @NUMBER, 
                            CUST_NO = @CUST_NO, 
                            DARSAD = @DARSAD,
                            PURSANT = @PURSANT, 
                            TOZIH = @TOZIH, 
                            STAT = @STAT,
                            PORID = @PORID
                            WHERE ID = @ID";

                        var parameters = new
                        {
                            NUMBER = Convert.ToDouble(NUMBER.Text),
                            CUST_NO = visitor.CUST_NO,
                            DARSAD = visitor.DARSAD,
                            PURSANT = visitor.PURSANT,
                            TOZIH = visitor.TOZIH,
                            STAT = visitor.STAT ?? false,
                            PORID = visitor.PORID,
                            ID = visitor.ID
                        };

                        dbms.DoExecuteSQL(sql, parameters);
                    }
                    catch (SqlException ex) when (ex.Number == 2627)
                    {
                        new Msgwin(false, "ویزیتور تکراری است. ردیف‌های پورسانت را بررسی کنید.").ShowDialog();
                        return;
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات پورسانت.").ShowDialog();
                    }

                }
            }

            double sum = SAYER_VISITOR_DATA.Sum(item => item.PURSANT ?? 0.0);
            Text190.Text = sum.ToString();
        }

        private bool IsRowValid(INVO_LST_FACTOR22 TheRow)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (TheRow == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "سطر خالی مجاز نیست" });
            }
            else
            {
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
                // Validate MEGH
                if (!double.TryParse(TheRow.MEGH?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صحیح وارد نشده" });
                }
                else
                {
                    if (Strings.Mid(Baseknow.OPTIONSS, 50, 1) == "5")
                    {
                        if (TheRow.MEGH == 0)
                        {
                            ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار کالا صفر نمیتواند باشد" });
                        }
                    }
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
                // Validate N_KOL
                if (!double.TryParse(TheRow.N_KOL?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تخفیف صحیح وارد نشده" });
                }
                //if (!System.Text.RegularExpressions.Regex.IsMatch(TheRow.N_KOL?.ToString(), @"^(100(\.00?)?|(\d{1,2}(\.\d{0,2})?))$")) //2 رقم اعشار
                if (!(TheRow.N_KOL >= 0 && TheRow.N_KOL <= 100))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "محدوده وارد شده تخفیف صحیح نیست" });
                }
                else if (TheRow.MABL_K == 0 && (TheRow.N_KOL > 0 || TheRow.N_MOIN > 0))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صفر است ولی تخفیف وارد شده , آنرا اصلاح کنید" });
                }
                // Validate N_MOIN
                if (!double.TryParse(TheRow.N_MOIN?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ تخفیف صحیح وارد نشده" });
                }
                // Validate TKHN
                if (!double.TryParse(TheRow.TKHN?.ToStringNullSafe(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "درصد تخفیف نقدی صحیح وارد نشده" });
                }
                if (!string.IsNullOrEmpty(MOIN_VAR2.Text) && MABL_VAR2.Text == "0")
                {
                    // MABL_VAR2.Text //مبلغ کارت بانک
                    //MOIN_VAR2.Text //معین کارت
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کارت بانک صفر است درحالی که حساب (معین کارت) آن مشخص شده" });
                }
                if (!string.IsNullOrEmpty(MOIN_HAV2.Text) && MABL_HAV2.Text == "0")
                {
                    //MABL_HAV2.Text //مبلغ بن یا حواله
                    //MOIN_HAV2.Text //معین بن
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ بن یا حواله صفر است درحالی که حساب (معین بن) آن مشخص شده" });
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
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }
            return true;
        }

        public bool SavedSuccessBtn { get; set; } = false;
        public bool DisplayError { get; set; } = true;
        public void BUTTON_SAVE_HAVALE_Click(object sender, RoutedEventArgs e)
        {
            if (!BUTTON_SAVE_HAVALE.IsEnabled || BUTTON_SAVE_HAVALE.Visibility != Visibility.Visible || !BUTTON_SAVE_HAVALE.IsHitTestVisible) { return; }

            SavedSuccessBtn = false;

            SGN1.IsChecked = SGN1.IsChecked ?? false;
            SGN2.IsChecked = SGN2.IsChecked ?? false;
            SGN3.IsChecked = SGN3.IsChecked ?? false;

            OKF.IsChecked = OKF.IsChecked ?? false;
            TICMBAA.IsChecked = TICMBAA.IsChecked ?? false;
            JAY.IsChecked = JAY.IsChecked ?? false;

            OKF.UpdateLayout();
            TICMBAA.UpdateLayout();
            JAY.UpdateLayout();

            if (HeaderIsValidShow(DisplayError) is false) return; //اگر اطلاعات سربرگ صحیح نیست خارج شو

            if (Baseknow.UGRP != "9")
            {   //Form_BeforeUpdate
                this.TKHF = 0;
            }

            //ذخیره خام سربرگ اولیه
            if (!SaveMasterNewNumberINSERT())
            {
                return; // get out if was not succesfull
            }

            this.OKF.IsChecked = true;
            this.INVO_LST_sub.IsReadOnly = false;
            this.INVO_LST_sub.IsReadOnly = false;
            this.TAKHFIF_APLAY_SUB.IsEnabled = true;

            if (!IsDirectFactor) //حواله موجود
            {
                CL_HESABDARI.LOGFACT(Convert.ToDouble(NUMBER.Text), 13, Convert.ToDouble(NUMBER1.Text), "CREATEFACTOR");
            }

            if (!DoCmdHeaderSaveUpdate())  //ذخیره کامل سربرگ Update
            {
                return;
            }

            GoGheymateUpdator();

            //پشت فاکتور
            #region PoshteFactor
            //M_NAGHD_AfterUpdate //MABL_HAV_AfterUpdate //TAKHFIF_AfterUpdate //MABL_HAZ_AfterUpdate //MOIN_HAV_Click //TAKH_AfterUpdate
            List<MsgModel> ErrosMessages_PSHT = new List<MsgModel>();
            if (MABL_HAV.Text != "0" && string.IsNullOrEmpty(MOIN_HAV.Text))
            {
                ErrosMessages_PSHT.Add(new MsgModel { MessageText_U = "حساب مربوط به حواله مشخص نشده است حتما بايد حساب مربوط به حواله مشخص شود" });
            }
            if (MABL_HAV.Text == "0")
            {
                MOIN_HAV.Text = null;
            }
            if (TAKHFIF.Text != "0" && !string.IsNullOrEmpty(TAKHFIF.Text) && this.JJKOL.Text != "0")
            {
                //takh.Text = Convert.ToString(Convert.ToDouble(TAKHFIF.Text) * 100 / Convert.ToDouble(JJKOL.Text));
                //takh.Text = Math.Round(Convert.ToDouble(takh.Text), 2).ToString();
            }
            if (MABL_HAZ.Text != "0" && string.IsNullOrEmpty(MOIN_HAZ.Text))
            {
                ErrosMessages_PSHT.Add(new MsgModel { MessageText_U = "حساب مربوط به خدمات مشخص نشده است حتما بايد حساب مربوط به خدمات مشخص شود" });
            }
            if (MABL_HAV.Text != "0" && string.IsNullOrEmpty(MOIN_HAV.Text))
            {
                ErrosMessages_PSHT.Add(new MsgModel { MessageText_U = "(مبلغ بن یا حواله) حساب معين مبلغ  وارد شده حتما بايد مشخص شود يا مبلغ صفر گردد" });
            }

            if (ErrosMessages_PSHT.Any())
            {
                ErrosMessages_PSHT = ErrosMessages_PSHT.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages_PSHT).ShowDialog();
                return;
            }
            else
            {
                if (FACTOR22_INVO_DATA.Any())
                {
                    //تخفیفات پیشرفته
                    CalculateAdvanceDiscount();
                }
            }
            #endregion

            //ذخیره مجدد جایزه
            JAYEHZAH(false);

            // محاسبه مجدد مالیات و ذخیره آن
            #region TICMBAA_AfterUpdate
            CalculateIMBAA();
            #endregion

            //اطلاعات بارنامه
            #region BARNAMEH
            //REQUEST_NO.Text //شماره درخواست کالا:
            //BARNAMEH.Text //شماره بارنامه:
            //DRIVER.Text //نام راننده :
            //DRIVER_MOB.Text //موبایل راننده:
            //CAMIUN_NUM.Text //شماره ماشین :
            //CAMIUN.Text //نوع ماشین :
            //MAGHSAD.SelectedValue //مقصد بار:
            //CAM_KHALY.Text //وزن ماشین خالی :
            //CAM_POOR.Text //وزن ماشین پر :
            //TOZIH.Text //توضیح

            //Validation
            //List<MsgModel> ErrosMessages_Bar = new List<MsgModel>();
            //if (!double.TryParse(CAM_KHALY.Text, out double _)) //وزن ماشین خالی
            //{
            //    ErrosMessages_Bar.Add(new MsgModel { MessageText_U = "وزن ماشین خالی صحیح وارد نشده" });
            //}
            //if (!double.TryParse(CAM_POOR.Text, out double _)) //وزن ماشین پر
            //{
            //    ErrosMessages_Bar.Add(new MsgModel { MessageText_U = "وزن ماشین پر صحیح وارد نشده" });
            //}
            //if (ErrosMessages_Bar.Count > 0)
            //{
            //    ErrosMessages_Bar = ErrosMessages_Bar.Select(x => x.MessageText_U).Distinct()
            //    .Select(message => new MsgModel { MessageText_U = message }).ToList();
            //    new MsgListwin(false, ErrosMessages_Bar).ShowDialog();
            //    return;
            //}
            dbms.DoExecuteSQL($@"DELETE FROM OTHER_DTL WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG}");

            if (MAGHSAD.SelectedValue is not null)
            {
                dbms.DoExecuteSQL($@"INSERT INTO dbo.OTHER_DTL(NUMBER, TAG, REQUEST_NO, BARNAMEH, DRIVER, DRIVER_MOB, CAMIUN_NUM, MAGHSAD, CAM_KHALY, CAM_POOR, TOZIH, CAMIUN)
                                         VALUES({NUMBER.Text},
                                         {fTAG} ,
                                         N'{REQUEST_NO.Text}' ,
                                         N'{BARNAMEH.Text}' ,
                                         N'{DRIVER.Text}' ,
                                         N'{DRIVER_MOB.Text}' ,
                                         N'{CAMIUN_NUM.Text}' ,
                                         {(MAGHSAD.SelectedValue is null ? 0 : MAGHSAD.SelectedValue)} ,
                                         {(string.IsNullOrEmpty(CAM_KHALY.Text) ? 0 : CAM_KHALY.Text)} ,
                                         {(string.IsNullOrEmpty(CAM_POOR.Text) ? 0 : CAM_POOR.Text)} ,
                                         N'{TOZIH.Text}' ,
                                         N'{CAMIUN.Text}')");
            }
            #endregion

            #region TAKHFIF_APLAY_DATA
            // تخفیفات پیشرفته بعدا اگر لازم شد کاربر بتونه دستی هم وارد کنه ولی فعلا نه
            //foreach (var ROW_TAKHFIF in TAKHFIF_APLAY_DATA)
            //{
            //    //var ROW_TAKHFIF = (e.Row.Item as TAKHFIF_APLAY);
            //    //كد تخفيف  تخفيف  //TID
            //    List<MsgModel> ErrosMessages = new List<MsgModel>(); //Validations:
            //    if (ROW_TAKHFIF.TID == null)
            //    {
            //        ErrosMessages.Add(new MsgModel { MessageText_U = "تخفیف خالی است." });
            //    }
            //    if (!int.TryParse(ROW_TAKHFIF.TID?.ToString(), out _))
            //    {
            //        ErrosMessages.Add(new MsgModel { MessageText_U = "نوع داده تخفیف مجاز نیست." });
            //    }

            //    if (ErrosMessages.Count > 0) // if have any error
            //    {
            //        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
            //        .Select(message => new MsgModel { MessageText_U = message }).ToList();
            //        new MsgListwin(false, ErrosMessages).ShowDialog();

            //        //e.Cancel = true;
            //        return;
            //    }
            //    else //Success
            //    {
            //        //Clearing First:

            //        if (ROW_TAKHFIF.NUMBER is null || ROW_TAKHFIF.NUMBER == 0) //Insert
            //        {
            //            InsertTAKHFIF_APLAY(ROW_TAKHFIF);
            //        }
            //        else //Update
            //        {
            //            UpdateTAKHFIF_APLAY(ROW_TAKHFIF);
            //        }

            //    }
            //}
            #endregion


            //محاسبه پورسانت ویزیتور
            //try
            //{
            //    List<MsgModel> ErrosMessages = new List<MsgModel>();
            var msgs = CL_HESABDARI.RunCalculateVisitorPorsant(Convert.ToInt64(NUMBER.Text), hTAG);
            //    //foreach (var matn in msgs)
            //    //{
            //    //    var normalized = matn
            //    //        .Replace("(PORID)", "")
            //    //        .Replace("(STAT=1)", "");

            //    //    //universControl.PopNotifyShow(normalized, Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            //    //    ErrosMessages.Add(new MsgModel { MessageText_U = normalized });
            //    //}

            //    //if (ErrosMessages.Any())
            //    //{
            //    //    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
            //    //        .Select(message => new MsgModel { MessageText_U = message }).ToList();
            //    //    new MsgListwin(false, ErrosMessages).Show();
            //    //}
            //}
            //catch (Exception ex)
            //{
            //    new Msgwin(false, $"خطا در محاسبه پورسانت: {ex.Message}").ShowDialog();
            //}

            if (!DoCmdHeaderSaveUpdate())  //ذخیره کامل سربرگ Update مجدد
            {
                return;
            }

            Summer();

            //باز محاسبه پورسانت ها چون ممکنه مبالغ فاکتور تغییر کرده باشه
            #region SAYER_PURSANT

            // Check for duplicate visitors
            var duplicateVisitors = SAYER_VISITOR_DATA
                .GroupBy(x => x.CUST_NO)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateVisitors.Any())
            {
                new Msgwin(false, "تکراری بودن ویزیتور مجاز نیست لطفا اصلاح کنید").ShowDialog();
                return;
            }

            //Validations
            foreach (var FINAL_CROW_ITEM in SAYER_VISITOR_DATA)
            {
                #region Validations
                List<MsgModel> ErrosMessages_Sayer = new List<MsgModel>();
                if (string.IsNullOrEmpty(FINAL_CROW_ITEM.CUST_NO))
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "نام شخص خالی است" });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.DARSAD?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.DARSAD?.ToString()))
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "درصد خالی است!" });
                }
                if (FINAL_CROW_ITEM.DARSAD < 0 || FINAL_CROW_ITEM.DARSAD > 100)
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "درصد باید بین 0 تا 100 باشد." });
                }
                if (!IsValidPercentage(FINAL_CROW_ITEM.DARSAD.ToStringNullSafe()))
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "درصد صحیح نیست !." });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.PURSANT?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.PURSANT?.ToString()))
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "مبلغ پورسانت صحیح نیست!" });
                }
                if (FINAL_CROW_ITEM.PURSANT < 0)
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "مبلغ پورسانت منقی نمیتواند باشد!" });
                }

                if (FINAL_CROW_ITEM.TOZIH?.Length > 50)
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "طول توضیح بیش از اندازه است!" });
                }
                if (FINAL_CROW_ITEM.STAT == null)
                {
                    ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "تیک مبلغ ثابت خالی است!" });
                }
                if (FINAL_CROW_ITEM.PORID is not null)
                {
                    if (!double.TryParse(FINAL_CROW_ITEM.PORID?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.PORID?.ToString()))
                    {
                        ErrosMessages_Sayer.Add(new MsgModel { MessageText_U = "الگوي پرداخت پورسانت خالی است!" });
                    }
                }
                if (ErrosMessages_Sayer.Count > 0)
                {
                    ErrosMessages_Sayer = ErrosMessages_Sayer.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages_Sayer).ShowDialog();
                    return;
                }
                #endregion
            }
            UpdateVisitorCommissions();
            #endregion

            //سند زدن
            SANAD();

            //دریافت مجدد مقادیر از دیتابیس
            //ReGetdata();
            VISITOR_DTL_SUB_ReGetData();
            PAY_GETD_SUB_ReGetData();
            TAKHFIF_APLAY_ReGetData();

            //کادر سبز و سند و مانده حساب
            MasterSummerAndMandeh();

            if (Convert.ToDouble(NUMBER.Text) > 0)
            {
                CL_HESABDARI.LetSigneTick(this.GetType().Name, 13, Convert.ToInt32(Baseknow.USERCOD), WINDOW_ID);
            }
            else
            {
                SGN1.IsEnabled = false;
                SGN2.IsEnabled = false;
                SGN3.IsEnabled = false;
            }

            ChangeIsHappend = false;
            DisplayError = true; //reset it
            SavedSuccessBtn = true; //ذخیره با موفقیت انجام شده

            universControl.PopNotifyShow(".اطلاعات با موفقیت ذخیره شد", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C", 1);

            if (FACTOR22_INVO_DATA.Count == 0 && IsDirectFactor)
            {
                var DG = INVO_LST_sub;
                var DEFINDX = (DG.SelectedIndex < 0) ? 0 : DG.SelectedIndex;
                CL_LMethods.FocusCellReadyToEdit(DG, "ANBAR", DEFINDX, true);
            }
        } //SAVE -------------------------------------------------------------------------

        public void CalculateIMBAA()
        {
            // 1. Prepare variables
            double smbaa = 0d;
            var number = this.NUMBER.Text;
            var tag = hTAG; //2

            if (NewRecord)
            {
                return;
            }

            // 2. Determine state and fetch invoice rows
            var invoiceList = dbms.DoGetDataSQL<INVO_LST_CSHARP>("SELECT * FROM INVO_LST WHERE NUMBER = @NUMBER AND TAG = @TAG", new { NUMBER = number, TAG = tag }).ToList();

            if (TICMBAA.IsChecked == true)
            {
                double.TryParse(MBAA.Text, out var currentMbaa);

                // Pre-fetch all needed codes to reduce SQL round-trips
                var codes = invoiceList.Select(x => x.CODE).Distinct().ToList();
                var cmbaaMap = dbms.DoGetDataSQL<HLF2>($"SELECT CMBAA, CODE FROM STUF_DEF WHERE CODE IN @Codes", new { Codes = codes }).ToDictionary(x => x.CODE, x => x.CMBAA);

                // Update all in memory and collect updates
                foreach (var row in invoiceList)
                {
                    // Default to false if code not found
                    bool cmbaa = cmbaaMap.TryGetValue(row.CODE, out var cmbaaObj) && Convert.ToBoolean(cmbaaObj);
                    if (cmbaa)
                    {
                        row.IMBAA = Math.Round((double)((row.MABL_K - row.N_MOIN) * CL_HESABDARI.GetArzesh(row.CODE) / 100));
                        smbaa += Convert.ToDouble(row.IMBAA);
                    }
                    else
                    {
                        row.IMBAA = 0;
                    }

                    // بروزرسانی آیتم در ObservableCollection بر اساس id
                    var itemInCollection = FACTOR22_INVO_DATA.FirstOrDefault(x => x.id == row.id);
                    if (itemInCollection != null)
                    {
                        itemInCollection.IMBAA = row.IMBAA;
                    }
                }

                // Batch Update all at once for better performance
                foreach (var row in invoiceList)
                {
                    dbms.DoExecuteSQL("UPDATE dbo.INVO_LST SET IMBAA = @IMBAA WHERE NUMBER = @NUMBER AND TAG = @TAG AND id = @ID",
                        new { IMBAA = row.IMBAA, NUMBER = number, TAG = tag, ID = row.id });
                }

                if (smbaa > 0d)
                {
                    if (smbaa != currentMbaa)
                    {
                        this.MBAA.Text = smbaa.ToString();
                    }
                    this.HMBAA.Text = Baseknow.HESMBAA;
                    this.CMB_HMBAA.SelectedValue = this.HMBAA.Text;
                }
                else
                {
                    ClearTaxFields();
                }
            }
            else
            {
                // Set IMBAA to 0 for all rows in both memory and DB
                foreach (var row in invoiceList)
                {
                    row.IMBAA = 0;

                    // بروزرسانی آیتم در ObservableCollection
                    var itemInCollection = FACTOR22_INVO_DATA.FirstOrDefault(x => x.id == row.id);
                    if (itemInCollection != null)
                    {
                        itemInCollection.IMBAA = 0;
                    }

                    dbms.DoExecuteSQL("UPDATE dbo.INVO_LST SET IMBAA = 0 WHERE NUMBER = @NUMBER AND TAG = @TAG AND id = @ID",
                        new { NUMBER = number, TAG = tag, ID = row.id });
                }

                if (Convert.ToDouble(MBAA.Text) > 0)
                {
                    this.MBAA.Text = "0";
                    this.HMBAA.Text = null;
                }

                ClearTaxFields();
            }

            // 3. Adjust read-only properties
            this.HMBAA.IsReadOnly = TICMBAA.IsChecked == true;
            this.MBAA.IsReadOnly = TICMBAA.IsChecked == true;
        }
        private void ClearTaxFields()
        {
            MBAA.Text = "0"; //مبلغ مالیات
            HMBAA.Text = null; //معین مالیات

            if (CMB_HMBAA is not null)
            {
                CMB_HMBAA.SelectedIndex = -1;
                CMB_HMBAA.SelectedValue = null;
                CMB_HMBAA.Text = null;
            }
        }
        private void CalculateAdvanceDiscount()
        {
            //محاسبه تخفیفات پیشرفته
            CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), 2);
            CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text ?? "0"), Convert.ToDouble(MABL_VAR.Text ?? "0"), Convert.ToDouble(MABL_HAV.Text ?? "0"), (bool)(TICMBAA.IsChecked ?? false));
        }


        private void MasterSummerAndMandeh()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0") { return; }

            Summer();

            //if (TAKHFIF.Text != "0" && !string.IsNullOrEmpty(TAKHFIF.Text) && this.JJKOL.Text != "0")
            //{
            //    //takh.Text = Convert.ToString(Convert.ToDouble(TAKHFIF.Text) * 100 / Convert.ToDouble(JJKOL.Text));
            //    //takh.Text = Math.Round(Convert.ToDouble(takh.Text), 2).ToString();
            //}


            var SANAD_NUMBER = dbms.DoGetDataSQL<string>($"SELECT TOP (1) N_S FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG}").FirstOrDefault();

            if (CUST_NO.SelectedValue != null)
            {
                MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
            }

            //var _rst_ = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + ")").FirstOrDefault();
            //if (_rst_ is null) // if (rst.Count == 0)
            //    MANDAH.Text = "0";
            //else
            //{
            //    if (_rst_ > 0)
            //        MANDAH.Text = Strings.Format(_rst_, "#,### ريال بدهكار");
            //    else
            //        MANDAH.Text = Strings.Format((_rst_ * -1), "#,### ريال بستانكار");
            //}
            N_S.Text = SANAD_NUMBER?.ToString();
            if (SANAD_NUMBER != null)
            {
                MABNA.Text = dbms.DoGetDataSQL<string?>($"SELECT TOP (1) BASE FROM dbo.DEED_HED WHERE NO_S  = 2 AND N_S = {SANAD_NUMBER}").FirstOrDefault();
            }
        }

        private void Summer()
        {
            if (string.IsNullOrEmpty(NUMBER.Text) || NUMBER.Text == "0") { return; }

            NCHK.Text = PAY_GETD_SUB22_DATA?.Sum(x => x?.MABL ?? 0).ToString();

            JJKOL.Text = SUM_OF_MABL_K.ToString(); //SMABLK //جمع فاکتور :
            HKH.Text = MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL.Text; //جمع کل فاکتور برای فسمت روی فاکتور
            Text117.Text = SUM_OF_MEGH_K.ToString(); //جمع مقادیر :

            //مبلغ قابل پرداخت: //= [JF] + [HKH] - [NTKHFIF] + [MBAA]
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            //جمع مبالغ پرداختی
            //=[M_NAGHD]+[MABL_VAR]+[MABL_HAV]+[NCHK]
            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(MABL_VAR.Text) + Convert.ToInt64(MABL_HAV.Text) + Convert.ToInt64(NCHK.Text);
            NPAR.Text = RMP.ToString();


            //=[GHABEL]-[NPAR]
            MAN.Text = Convert.ToString(Convert.ToInt64(GHABEL.Text) - Convert.ToInt64(NPAR.Text)); //مانده
            MN.Text = MAN.Text; // مانده روی فاکتور
        }

        private bool DoCmdHeaderSaveUpdate()
        {
            try
            {
                //Saving ...
                string _qre = null;

                string _n_s = "NULL";
                if (double.TryParse(N_S.Text, out var n_sVal) && n_sVal > 0)
                {
                    _n_s = n_sVal.ToString();
                }

                var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TAH,MOLAH FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG}").FirstOrDefault(); //برای فاکتور غیر مستقیم

                var hmbaaSqlValue = string.IsNullOrWhiteSpace(HMBAA.Text)
                    ? "NULL"
                    : $"N'{CMB_HMBAA.SelectedValue ?? HMBAA.Text}'";


                if (CUST_NO.SelectedValue != null && Maghsad_Havaleh_Directyfactor is null)
                {
                    var masMaghsad = dbms.DoGetDataSQL<int?>($"SELECT SHAHRID FROM dbo.CUST_HESAB WHERE hes = N'{CUST_NO.SelectedValue}'").FirstOrDefault();
                    if (masMaghsad != null)
                    {
                        Maghsad_Havaleh_Directyfactor = masMaghsad;
                    }
                }

                if (IsDirectFactor)
                {
                    _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()}, 
                    TAH = N'{TAH.Text}', MAS = {(Maghsad_Havaleh_Directyfactor is null ? MAS.Text : Maghsad_Havaleh_Directyfactor)}, VAS = {VAS}, N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    M_NAGHD = {M_NAGHD2.Text}, MABL_VAR = {MABL_VAR2.Text}, MOIN_VAR = N'{CMB_MOIN_VAR2.SelectedValue}', MABL_HAV = {MABL_HAV2.Text}, MOIN_HAV = N'{CMB_MOIN_HAV2.SelectedValue}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', TAKHFIF = {TAKHFIF2.Text},
                    TAMIR = ISNULL(TAMIR, 0),
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SHARAYET = N'{SHARAYET.Text}', SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = {hmbaaSqlValue}, 
                    TICMBAA = {Convert.ToByte(TICMBAA.IsChecked)}, OKF = {Convert.ToByte(OKF.IsChecked)},
                    OKTIME = {(string.IsNullOrEmpty(OKTIME.ToStringNullSafe()) ? "NULL" : OKTIME)},
                    OKDATE = {(string.IsNullOrEmpty(OKDATE.ToStringNullSafe()) ? "NULL" : OKDATE)},                    
                    CDDATE = {(string.IsNullOrEmpty(CDDATE.ToStringNullSafe()) ? "NULL" : CDDATE)},
                    CDTIME = {(string.IsNullOrEmpty(CDTIME.ToStringNullSafe()) ? "NULL" : CDTIME)}, JAY = {Convert.ToByte(JAY.IsChecked)}, 
                    MODAT_PPID = {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)}, PEPID = {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)},
                    PEID = {(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)},
                    USER_NAME = N'{USER_NAME.Text}', FNUMCO = {FNUMCO.Text},
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG} ";
                }
                else
                {
                    //HEADER_FAC?.MOLAH //این خط حذف شد
                    _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text},
                    TAH = N'{HEADER_FAC?.TAH}', MAS = {MAS_MAGHSAD_HV}, VAS = {VAS}, N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    M_NAGHD = {M_NAGHD2.Text}, MABL_VAR = {MABL_VAR2.Text}, MOIN_VAR = N'{CMB_MOIN_VAR2.SelectedValue}', MABL_HAV = {MABL_HAV2.Text}, MOIN_HAV = N'{CMB_MOIN_HAV2.SelectedValue}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', TAKHFIF = {TAKHFIF2.Text},
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SHARAYET = N'{SHARAYET.Text}', SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA =  {hmbaaSqlValue}, 
                    TICMBAA = {Convert.ToByte(TICMBAA.IsChecked)}, OKF = {Convert.ToByte(OKF.IsChecked)},
                    OKTIME = {(string.IsNullOrEmpty(OKTIME.ToStringNullSafe()) ? "NULL" : OKTIME)},
                    OKDATE = {(string.IsNullOrEmpty(OKDATE.ToStringNullSafe()) ? "NULL" : OKDATE)},                    
                    CDDATE = {(string.IsNullOrEmpty(CDDATE.ToStringNullSafe()) ? "NULL" : CDDATE)},
                    CDTIME = {(string.IsNullOrEmpty(CDTIME.ToStringNullSafe()) ? "NULL" : CDTIME)}, JAY = {Convert.ToByte(JAY.IsChecked)}, 
                    MODAT_PPID = {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)}, PEPID = {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)},
                    PEID = {(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)},
                    USER_NAME = N'{USER_NAME.Text}', FNUMCO = {FNUMCO.Text},
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG} ";
                }

                _ = dbms.DoExecuteSQL(_qre); //TAG 2

                #region EXPORTY
                string? _ISOCODE_ = null;
                if (IsExporty)
                {
                    if (ARZKIND2.SelectedValue != null)
                    {
                        _ISOCODE_ = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 ISOCode FROM dbo.[TCOD_ARZ] WHERE ID = {ARZKIND2.SelectedValue}").FirstOrDefault();
                    }
                }
                #endregion



                _qre = $@"UPDATE dbo.HEAD_LST
                    SET NUMBER = {NUMBER.Text}, DATE_N = {DATE_N.Text.ToRawTarikh()}, 
                    TAH = N'{TAH.Text}', MAS = {MAS.Text}, VAS = {VAS}, N_S = {_n_s}, CUST_NO = N'{CUST_NO.SelectedValue}', MOLAH = N'{MOLAH.Text}',
                    M_NAGHD = {M_NAGHD2.Text}, MABL_VAR = {MABL_VAR2.Text}, MOIN_VAR = N'{CMB_MOIN_VAR2.SelectedValue}', MABL_HAV = {MABL_HAV2.Text}, MOIN_HAV = N'{CMB_MOIN_HAV2.SelectedValue}',
                    MABL_HAZ = {MABL_HAZ.Text}, MOIN_HAZ = N'{CMB_MOIN_HAZ.SelectedValue}', TAKHFIF = {TAKHFIF2.Text}, FNUMCO = {FNUMCO.Text},
                    DEPATMAN = {DEPATMAN.SelectedValue}, SHIFT = {SHIFT.SelectedValue}, CUST_KIND = {CUST_KIND.SelectedValue},
                    SHARAYET = N'{SHARAYET.Text}', SGN1 = {Convert.ToByte(SGN1.IsChecked)}, SGN2 = {Convert.ToByte(SGN2.IsChecked)}, 
                    SGN3 = {Convert.ToByte(SGN3.IsChecked)}, MBAA = {MBAA.Text}, HMBAA = {hmbaaSqlValue}, 
                    TICMBAA = {Convert.ToByte(TICMBAA.IsChecked)}, OKF = {Convert.ToByte(OKF.IsChecked)},
                    ANBARF = {(string.IsNullOrEmpty(ANBARF.Text) ? "NULL" : ANBARF.Text)},
                    ARZD = {(string.IsNullOrEmpty(ARZD.Text) ? "NULL" : ARZD.Text)},
                    ARZKIND2 = {(string.IsNullOrEmpty(ARZKIND2.SelectedValue.ToStringNullSafe()) ? "NULL" : ARZKIND2.SelectedValue)},
                    ARZCODING = N'{(string.IsNullOrEmpty(_ISOCODE_) ? "NULL" : _ISOCODE_)}',
                    OKTIME = {(string.IsNullOrEmpty(OKTIME.ToStringNullSafe()) ? "NULL" : OKTIME)},
                    OKDATE = {(string.IsNullOrEmpty(OKDATE.ToStringNullSafe()) ? "NULL" : OKDATE)},
                    CDDATE = {(string.IsNullOrEmpty(CDDATE.ToStringNullSafe()) ? "NULL" : CDDATE)},
                    CDTIME = {(string.IsNullOrEmpty(CDTIME.ToStringNullSafe()) ? "NULL" : CDTIME)}, JAY = {Convert.ToByte(JAY.IsChecked)}, 
                    MODAT_PPID = {(MODAT_PPID.SelectedValue is null ? "NULL" : MODAT_PPID.SelectedValue)}, PEPID = {(PEPID.SelectedValue is null ? "NULL" : PEPID.SelectedValue)},
                    PEID = {(PEID.SelectedValue is null ? "NULL" : PEID.SelectedValue)},
                    USER_NAME = N'{USER_NAME.Text}',
                    sgn1usid = {(SGN1usid.Tag is null ? "NULL" : SGN1usid.Tag)}, 
                    sgn2usid = {(SGN2usid.Tag is null ? "NULL" : SGN2usid.Tag)}, 
                    sgn3usid = {(SGN3usid.Tag is null ? "NULL" : SGN3usid.Tag)}
                    WHERE NUMBER = {NUMBER.Text} AND TAG = {fTAG} 
                    ";
                _ = dbms.DoExecuteSQL(_qre); //TAG 13
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
                return false;
            }
            catch (Exception ex)
            {
                CL_LMethods.DoWriteMyLog("خطا در ذخیره سربرگ فاکتور فروش", ex);
                new Msgwin(false, $"خطا در انجام عملیات").Show();
                return false;
            }

            return true;
        }
        private bool HeaderIsValidShow(bool _DisplayError_ = true)
        {
            //Validation
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            #region HEADER_VALIDATION 
            string date_n_val = DATE_N.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DATE_N.Text = BEFOREDATEN;
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقدار تاریخ صحیح نیست" });
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        DATE_N.Text = BEFOREDATEN;
                        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                    }
                }
            }
            else
            {
                DATE_N.Text = BEFOREDATEN;
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ نمی تواند خالی باشد" });
            }

            if (!IsDirectFactor)
            {
                if (string.IsNullOrEmpty(NUMBER.SelectedValue.ToStringNullSafe()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شماره حواله خالی است , لطفا شماره حواله ای انتخاب کنید." });
                }
            }

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
            if (string.IsNullOrEmpty(USER_NAME.Text))
            {
                USER_NAME.Text = Baseknow.UUSER; // نام کاربری
            }

            if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
            {
                //تاریخ صحیح نیست
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ فاکتور را بررسی کنید" });
            }

            if (Baseknow.GHAYM == 7)
            {
                if (MODAT_PPID.SelectedIndex < 0)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "نحوه پرداخت را انتخاب کنید " });
                    Page57.Focus();
                    Page57.IsSelected = true;
                    MODAT_PPID.Focus();
                }
                //else if (!CL_HESABDARI.LETSGO("AZADPAY") && MODAT_PPID.SelectedIndex == 0)
                //{
                //    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                //    MODAT_PPID.SelectedIndex = -1;
                //    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                //    ErrosMessages.Add(new MsgModel { MessageText_U = "شما اجازه قيمت گذاري آزاد  نداريد" });
                //}

                if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP ModatValue)
                {
                    if (ModatValue?.PPAME.Trim().FixPersianChars() != "نقدی")
                    {
                        if (Convert.ToInt32(MAS.Text) <= 0)
                        {
                            ErrosMessages.Add(new MsgModel { MessageText_U = "مدت را وارد کنید " });
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(M_NAGHD2.Text)) //مبلغ نقد روی فاکتور
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ نقد نمیتواند خالی باشد." });
            }
            if (string.IsNullOrEmpty(MABL_VAR2.Text)) //مبلغ کارت بانک
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ کارت بانک نمیتواند خالی باشد." });
            }
            if (string.IsNullOrEmpty(MABL_HAV2.Text)) //مبلغ بن یا حواله روی فاکتور
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ بن یا حواله نمیتواند خالی باشد." });
            }
            if (string.IsNullOrEmpty(TAKHFIF2.Text)) //مبلغ تخفیف روی فاکتور
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ تخفیف نمیتواند خالی باشد." });
            }
            if (MABL_VAR2.Text != "0")
            {
                if (string.IsNullOrEmpty(MOIN_VAR2.Text)) //معین کارت روی فاکتور
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "معین کارت نمیتواند خالی باشد." });
                }
            }
            if (MABL_HAV2.Text != "0")
            {
                if (string.IsNullOrEmpty(MOIN_HAV2.Text)) //معین بن روی فاکتور
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "معین بن نمیتواند خالی باشد." });
                }
            }
            if (string.IsNullOrEmpty(MOIN_HAZ.Text) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
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

            if ((string.IsNullOrEmpty(CMB_MOIN_VAR.SelectedValue.ToStringNullSafe()) || CMB_MOIN_VAR.SelectedValue.ToStringNullSafe() != MOIN_VAR.Text) && Convert.ToInt64(MABL_VAR.Text) > 0) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین واریزی مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_VAR.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_VAR.Text) || MABL_VAR.Text == "0")) //معین واریزی
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ واریزی مشخص نشده!" });
            }

            if ((string.IsNullOrEmpty(CMB_MOIN_HAV.SelectedValue.ToStringNullSafe()) || CMB_MOIN_HAV.SelectedValue.ToStringNullSafe() != MOIN_HAV.Text) && Convert.ToInt64(MABL_HAV.Text) > 0) //معین حواله
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین حواله مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_HAV.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_HAV.Text) || MABL_HAV.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ حواله مشخص نشده!" });
            }

            if ((string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) || CMB_HMBAA.SelectedValue.ToStringNullSafe() != HMBAA.Text) && Convert.ToInt64(MBAA.Text) > 0) //مالیات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مالیات مشخص نشده!" });
            }
            else if (!string.IsNullOrEmpty(CMB_HMBAA.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MBAA.Text) || MBAA.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ مالیات مشخص نشده!" });
            }

            if ((string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) || CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe() != MOIN_HAZ.Text) && Convert.ToInt64(MABL_HAZ.Text) > 0)  //معین خدمات
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "حساب خدمات انتخاب نشده درحالی که مبلغ خدمات وارد شده" });
            }
            else if (!string.IsNullOrEmpty(CMB_MOIN_HAZ.SelectedValue.ToStringNullSafe()) && (string.IsNullOrEmpty(MABL_HAZ.Text) || MABL_HAZ.Text == "0"))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ خدمات مشخص نشده!" });
            }

            if (!IsNull(CMB_MOIN_HAZ.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.MOIN_HAZ.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد (فیلد هزینه در پشت فاکتور)" });
                }
            }
            if (!IsNull(CMB_HMBAA.SelectedValue))
            {
                if (CL_HESABDARI.ISTAF(this.HMBAA.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
                }
            }
            //POSHTEFACTOR

            var errors = (from object i in INVO_LST_sub.ItemsSource
                          let c = INVO_LST_sub.ItemContainerGenerator.ContainerFromItem(i)
                          where c != null && Validation.GetHasError(c)
                          select c).Any();

            errors = (from object i in PAY_GETD_SUB22.ItemsSource
                      let c = PAY_GETD_SUB22.ItemContainerGenerator.ContainerFromItem(i)
                      where c != null && Validation.GetHasError(c)
                      select c).Any();

            errors = (from object i in VISITOR_DTL_SUB.ItemsSource
                      let c = VISITOR_DTL_SUB.ItemContainerGenerator.ContainerFromItem(i)
                      where c != null && Validation.GetHasError(c)
                      select c).Any();

            if (errors)
            {
                universControl.PopNotifyShow("داده های وارد شده مربوط به سطر ها درست نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }

            if (ErrosMessages.Count > 0)
            {
                if (_DisplayError_)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                return false;
            }
            #endregion
            return true;
        }
        private bool SaveMasterNewNumberINSERT()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NUMBER1.Text) || NUMBER1.Text == "0")
                {
                    long newNumber1;
                    long newNumber;

                    using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        using (var transaction = db.BeginTransaction(System.Data.IsolationLevel.Serializable))
                        {
                            try
                            {
                                // 1. قفل کردن جدول (کار شما صحیح بود و اینجا هم تکرار می‌کنیم)
                                db.Execute("SELECT TOP 1 NUMBER FROM dbo.HEAD_LST WITH (TABLOCKX, HOLDLOCK)", null, transaction);

                                // ************************************************************
                                // اصلاح حیاتی 1: محاسبه شماره فاکتور چاپی (NUMBER1)
                                // ************************************************************
                                // برای اطمینان، ماکسیمم را از تگ 13 میگیریم
                                var maxNum1 = db.Query<double?>("SELECT MAX(NUMBER1) FROM HEAD_LST WHERE TAG = 13", null, transaction).FirstOrDefault();
                                newNumber1 = (maxNum1 == null || maxNum1 == 0) ? (long)Baseknow.STHFR : Convert.ToInt64(maxNum1 + 1);

                                // حلقه اطمینان (برای محکم کاری): اگر به هر دلیلی این شماره پر بود، بعدی را بگیر
                                while (db.Query<int>("SELECT COUNT(*) FROM HEAD_LST WHERE NUMBER1 = @N1 AND TAG = 13", new { N1 = newNumber1 }, transaction).FirstOrDefault() > 0)
                                {
                                    newNumber1++;
                                }

                                // ************************************************************
                                // اصلاح حیاتی 2: محاسبه شماره داخلی (NUMBER)
                                // ************************************************************
                                if (IsDirectFactor)
                                {
                                    // نکته کلیدی: باید ماکسیمم را بین *هر دو* تگ چک کنیم
                                    // تا اگر فاکتوری بدون حواله وجود داشت، شماره تکراری نسازیم
                                    var maxNum = db.Query<double?>("SELECT MAX(NUMBER) FROM HEAD_LST WHERE TAG IN (2, 13)", null, transaction).FirstOrDefault();

                                    newNumber = (maxNum == null || maxNum == 0) ? (long)Baseknow.STHFR : Convert.ToInt64(maxNum + 1);

                                    // حلقه اطمینان: چک میکنیم این شماره در هیچکدام از تگ‌ها نباشد
                                    while (db.Query<int>("SELECT COUNT(*) FROM HEAD_LST WHERE NUMBER = @N AND TAG IN (2, 13)", new { N = newNumber }, transaction).FirstOrDefault() > 0)
                                    {
                                        newNumber++;
                                    }
                                }
                                else
                                {
                                    // در حالت غیر مستقیم، شماره داخلی همان شماره حواله انتخاب شده است
                                    newNumber = Convert.ToInt64(NUMBER.Text);
                                }

                                // 3. درج در دیتابیس (بدون تغییر)
                                // درج فاکتور (TAG 13)
                                db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER, NUMBER1, TAG, DATE_N, MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF, UID)
                            VALUES ({newNumber}, {newNumber1}, {fTAG}, 0, 0, 0, 0, 0, 0, 0, 0, {Baseknow.USERCOD})", null, transaction);

                                // درج حواله (TAG 2) - اگر مستقیم بود
                                if (IsDirectFactor)
                                {
                                    db.Execute($@"INSERT INTO dbo.HEAD_LST (NUMBER, NUMBER1, TAG, DATE_N, MAS, VAS, M_NAGHD, MABL_VAR, MABL_HAV, MABL_HAZ, TAKHFIF, TAMIR, UID)
                            VALUES ({newNumber}, {newNumber1}, {hTAG}, 0, 0, 0, 0, 0, 0, 0, 0, 0, {Baseknow.USERCOD})", null, transaction);
                                }

                                transaction.Commit();

                                // آپدیت UI
                                NUMBER1.Text = newNumber1.ToString();
                                NUMBER.Text = newNumber.ToString();
                                NUMBER1.UpdateLayout();
                                NUMBER.UpdateLayout();
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }

                    _navigationManager.IsNewRecord = false;
                    RefreshAfterUpdate();
                    this.CDDATE = CL_HESABDARI.FARSIDATE();
                    this.CDTIME = CL_HESABDARI.GTFS();
                    CL_HESABDARI.ADDTAKH(Convert.ToInt64(CUST_KIND.SelectedValue), Convert.ToInt64(NUMBER.Text), 2);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    new Msgwin(false, $"خطای تکراری بودن شماره! سیستم شماره {NUMBER1.Text} را پیشنهاد داد اما در لحظه آخر ثبت شده بود.").Show();
                }
                else
                {
                    new Msgwin(false, $"خطا در انجام عملیات ذخیره پایگاه داده: {ex.Message}").Show();
                }
                return false;
            }
            catch (Exception ex)
            {
                CL_LMethods.DoWriteMyLog("خطا در ذخیره SaveMasterNewNumberINSERT", ex);
                new Msgwin(false, "خطا در انجام عملیات").Show();
                return false;
            }

            return true;
        }


        private void M_NAGHD_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text), Convert.ToDouble(MABL_VAR.Text), Convert.ToDouble(MABL_HAV.Text), (bool)TICMBAA.IsChecked); //#Check Matter این باید توی حالت دکمه ذخیره فعال بشه
        }

        private void MABL_HAV2_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_HAV2.Text) != 0 && IsNull(this.MOIN_HAV2.Text))
            {
                new Msgwin(false, "حساب مربوط به حواله مشخص نشده است حتما بايد حساب مربوط به حواله مشخص شود ").ShowDialog();
                this.MOIN_HAV2.Focus();
            }
            if (Convert.ToDouble(MABL_HAV2.Text) == 0)
            {
                this.MOIN_HAV2.Text = "";
            }
            //CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text), Convert.ToDouble(MABL_VAR.Text), Convert.ToDouble(MABL_HAV.Text), (bool)TICMBAA.IsChecked); //#CheckMatter
        }

        private void MABL_VAR2_AfterUpdate()
        {
            if (Convert.ToDouble(MABL_VAR2.Text) != 0 && IsNull(MOIN_VAR2.Text))
            {
                new Msgwin(false, "حساب مربوط به واريزي مشخص نشده است حتما بايد حساب مربوط به واريزي مشخص شود ").ShowDialog();
                this.MOIN_VAR2.Focus();
            }
            if (Convert.ToDouble(MABL_VAR2.Text) == 0)
            {
                MOIN_VAR2.Text = "";
            }
        }
        private void MANDAH_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MANDAH_Click();
        }

        private void MANDAH_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MANDAH_DblClick();
        }
        private void MOIN_HAV_Click()
        {
            if (CUST_NO.SelectedValue == null)
            {
                return;
            }

            //if (Baseknow.SANAD == 1)
            //{
            //    SANAD();
            //}
            if (CUST_NO.SelectedValue != null)
            {
                MANDAH.Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
            }
        }
        private void MOIN_HAV_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region MOIN_HAV_Exit
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(MOIN_HAV.Text))
            {
                new Msgwin(false, "حساب معين مبلغ  وارد شده حتما بايد مشخص شود يا مبلغ صفر گردد").Show();
            }
            #endregion
        }

        private void MOIN_HAV_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            MOIN_HAV_Click();
        }

        private void MOIN_HAZ_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region MOIN_HAZ_Exit
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(MOIN_HAV.Text))
            {
                new Msgwin(false, "حساب معين مبلغ  وارد شده حتما بايد مشخص شود يا مبلغ صفر گردد").Show();
                CANCEL = Convert.ToInt32(true);
            }
            #endregion
        }

        private void MOIN_VAR2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            #region MOIN_VAR_Exit
            if (Convert.ToDouble(MABL_VAR.Text) != 0 && IsNull(MOIN_VAR2.Text))
            {
                new Msgwin(false, "حساب معين مبلغ  وارد شده حتما بايد مشخص شود يا مبلغ صفر گردد").ShowDialog();
                //CANCEL = Convert.ToInt32(true);
            }
            #endregion
        }

        private void MOLAH_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (MODAT_PPID.Visibility != Visibility.Visible)
            {
                int IDX = Convert.ToInt32(INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath.Equals("ANBAR")).DisplayIndex);
                if (!MODAT_PPID.IsKeyboardFocusWithin && FACTOR22_INVO_DATA.Count == 0)
                {
                    INVO_LST_sub.Dispatcher.BeginInvoke(() =>
                    {
                        INVO_LST_sub.SelectedIndex = INVO_LST_sub.Items.Count - 1;

                        if (ANBAR_COLUMN.Visibility == Visibility.Visible)
                        {
                            INVO_LST_sub.CurrentCell = new DataGridCellInfo(INVO_LST_sub.SelectedItem, INVO_LST_sub.Columns[IDX]);
                        }
                        else
                        {
                            INVO_LST_sub.CurrentCell = new DataGridCellInfo(INVO_LST_sub.SelectedItem, INVO_LST_sub.Columns[NAME_CODE_INDEX_COL]);
                        }
                        INVO_LST_sub.BeginEdit();
                    });
                }
            }
        }

        private void NUMBER_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsDirectFactor == false) //فاکتور غیر مستقیم است
            {
                if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0")
                {
                    new HEAD_LST_HAVL(Convert.ToDouble(NUMBER.Text)).ShowDialog();
                }
            }
        }
        private void TAKHFIF_AfterUpdate()
        {
            if (true)
            {
                double jamf;
                int i = 0;
                double jammoin = 0;
                if (FACTOR22_INVO_DATA.Count > 0 && SUM_OF_MABL_K != null)
                {
                    jamf = SUM_OF_MABL_K;
                }
                else
                {
                    jamf = 0;
                }
                foreach (var item in FACTOR22_INVO_DATA)
                {
                    i++;
                    if (i == FACTOR22_INVO_DATA.Count)
                    {
                        item.N_MOIN = Convert.ToDouble(TAKHFIF2.Text) - jammoin;
                        if (item.MABL_K != 0)
                        {
                            item.N_KOL = (Convert.ToDouble(TAKHFIF2.Text) - jammoin) / item.MABL_K * 100;
                        }
                        else
                        {
                            item.N_KOL = 0;
                        }
                    }
                    else
                    {
                        item.N_MOIN = Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF2.Text)));
                        if (item.MABL_K != 0)
                        {
                            item.N_KOL = Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF2.Text))) / item.MABL_K * 100; //Here was the bug fixed becuase of 100
                        }
                        else
                        {
                            item.N_KOL = 0;
                        }
                    }
                    jammoin += Math.Round((double)(item.MABL_K / jamf * Convert.ToDouble(TAKHFIF2.Text)));
                    //dbms.DoExecuteSQL("UPDATE invo_LST SET N_MOIN = " + item.N_MOIN + ", N_KOL = " + item.N_KOL + " WHERE ID = " + item.ID);
                }
            }

            return;//---

            #region TAKHFIF2_AfterUpdate
            //double JAMF;
            //var i = default(int);
            //var jammoin = default(double);
            //var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + this.NUMBER.Text + " ) AND ((INVO_LST.TAG)=2))").FirstOrDefault();
            //if (!(JST is null))
            //{
            //    JAMF = (double)JST;
            //}
            //else
            //{
            //    JAMF = 0d;
            //}
            //var rst = dbms.DoGetDataSQL<INVO_LST_CSHARP>("select * from invo_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text).ToList();
            //var _where = " WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text;
            //for (int w = 0; w < rst.Count; w++)
            //{
            //    i = i + 1;
            //    if (i == rst.Count)
            //    {
            //        rst[w].N_MOIN = Convert.ToDouble(TAKHFIF.Text) - jammoin;
            //        if (rst[w].MABL_K != 0)
            //        {
            //            rst[w].N_KOL = (Convert.ToDouble(TAKHFIF.Text) - jammoin) / rst[w].MABL_K * 100;
            //        }
            //        else
            //        {
            //            rst[w].N_KOL = 0;
            //        }
            //    }
            //    else
            //    {
            //        rst[w].N_MOIN = Math.Round((double)(rst[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF.Text)));
            //        if (rst[w].MABL_K != 0)
            //        {
            //            rst[w].N_KOL = Math.Round((double)(rst[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF.Text))) / rst[w].MABL_K;
            //        }
            //        else
            //        {
            //            rst[w].N_KOL = 0;
            //        }
            //    }
            //    jammoin = jammoin + Math.Round((double)(rst[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF.Text)));
            //    dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_MOIN = {rst[w].N_MOIN},N_KOL = {rst[w].N_KOL}{_where} ");
            //    ReGetdata();

            //}
            #endregion

            #region TAKHFIF_AfterUpdate
            if (/*Convert.ToDouble(TAKHFIF2.Text) != 0*/ true)
            {
                double JAMF = SUM_OF_MABL_K;
                var i = default(int);
                var jammoin = default(double);

                for (int w = 0; w < FACTOR22_INVO_DATA.Count; w++)
                {
                    i = i + 1;
                    if (i == FACTOR22_INVO_DATA.Count)
                    {
                        FACTOR22_INVO_DATA[w].N_MOIN = Convert.ToDouble(TAKHFIF2.Text) - jammoin;
                        if (FACTOR22_INVO_DATA[w].MABL_K != 0)
                        {
                            FACTOR22_INVO_DATA[w].N_KOL = (Convert.ToDouble(TAKHFIF2.Text) - jammoin) / FACTOR22_INVO_DATA[w].MABL_K * 100;
                        }
                        else
                        {
                            FACTOR22_INVO_DATA[w].N_KOL = 0;
                        }
                    }
                    else
                    {
                        var _N_MOIN = Math.Round((double)(FACTOR22_INVO_DATA[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF2.Text)));
                        if (_N_MOIN <= FACTOR22_INVO_DATA[w].MABL_K)
                        {
                            FACTOR22_INVO_DATA[w].N_MOIN = _N_MOIN;

                            if (FACTOR22_INVO_DATA[w].MABL_K != 0)
                            {
                                FACTOR22_INVO_DATA[w].N_KOL = Math.Round((double)(FACTOR22_INVO_DATA[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF2.Text))) / FACTOR22_INVO_DATA[w].MABL_K;
                            }
                            else
                            {
                                FACTOR22_INVO_DATA[w].N_KOL = 0;
                            }
                        }
                    }
                    jammoin = jammoin + Math.Round((double)(FACTOR22_INVO_DATA[w].MABL_K / JAMF * Convert.ToDouble(TAKHFIF2.Text)));
                }
            }
            #endregion
        }


        private void TAKH_AfterUpdate()
        {
            //if (Convert.ToDouble(takh.Text) != 0 && !IsNull(takh.Text))
            //{
            //    this.TAKHFIF.Text = Convert.ToString(Convert.ToDouble(JJKOL.Text) * Convert.ToDouble(takh.Text) / 100);
            //}
        }

        private void M_NAGHD2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.J) //Ctrl + J
            {
                if (NUMBER.Text != "0" && NUMBER.Text != null) //Saved Before
                {
                    if (!SavedSuccessBtn || _navigationManager.IsNewRecord)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null); // اول ذخیره IVNO_LST
                    }

                    if (SavedSuccessBtn)
                    {
                        NAGHDF nAGHDF = new NAGHDF(I_AM_FOROOSH22);
                        nAGHDF.ShowDialog();
                    }

                }
            }
        }

        private void MABL_VAR2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.K) //Ctrl + K
            {
                if (NUMBER.Text != "0" && NUMBER.Text != null) //Saved Before
                {
                    if (!SavedSuccessBtn || _navigationManager.IsNewRecord)
                    {
                        BUTTON_SAVE_HAVALE_Click(null, null); // اول ذخیره IVNO_LST
                    }

                    if (SavedSuccessBtn)
                    {
                        KARTBANK kartbank = new KARTBANK(I_AM_FOROOSH22);
                        kartbank.ShowDialog();
                    }
                }
            }
        }

        private void TAKHFIF2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T) //Ctrl + K
            {
                if (NUMBER.Text != "0" && NUMBER.Text != null) //Saved Before
                {
                    if (SavedSuccessBtn)
                    {
                        TAKHFIF takhfif = new TAKHFIF(I_AM_FOROOSH22);
                        takhfif.ShowDialog();
                    }
                }
            }
        }

        //TAKHFIF_APLAY_SECTION
        #region TAKHFIF_APLAY_SECTION
        private void TAKHFIF_APLAY_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }
        public void InsertTAKHFIF_APLAY(TAKHFIF_APLAY model)
        {
            string insertSql = "INSERT INTO TAKHFIF_APLAY (NUMBER, KIND) VALUES (@NUMBER, @KIND)";
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Execute(insertSql, model);
            }
        }
        public void UpdateTAKHFIF_APLAY(TAKHFIF_APLAY model)
        {
            string updateSql = "UPDATE TAKHFIF_APLAY SET NUMBER = @NUMBER, KIND = @KIND WHERE TID = @TID";
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Execute(updateSql, model);
            }
        }
        public void DeleteTAKHFIF_APLAY(int number, int kind)
        {
            string deleteSql = "DELETE FROM TAKHFIF_APLAY WHERE NUMBER = @NUMBER AND  KIND = @KIND ";
            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Execute(deleteSql, new { NUMBER = number, KIND = kind });
            }
        }
        public void TAKHFIF_APLAY_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //Erro Check Matter مشکل اینجا کوئری و نمایش داده های کمبوباکس هست که نجوه jion جدول درست نیست 
                var QRE_LST = dbms.DoGetDataSQL<TAKHFIF_APLAY>($@"SELECT dbo.TAKHFIF_APLAY.TID, dbo.TAKHFIF_APLAY.NUMBER, dbo.TAKHFIF_APLAY.KIND, dbo.TAKHFIF_DEF.TSHARH
                                                                  FROM dbo.TAKHFIF_APLAY
                                                                       RIGHT OUTER JOIN dbo.TAKHFIF_DEF ON dbo.TAKHFIF_APLAY.TID=dbo.TAKHFIF_DEF.TID
                                                                  WHERE (dbo.TAKHFIF_APLAY.NUMBER={NUMBER.Text}) ").ToList();

                //ComboBox:
                Combo6Column.ItemsSource = dbms.DoGetDataSQL<TAKHFIF_DEF>("SELECT TID, TSHARH FROM TAKHFIF_DEF").ToList();

                TAKHFIF_APLAY_DATA?.Clear();
                foreach (var item in QRE_LST)
                    TAKHFIF_APLAY_DATA.Add(item);

                TAKHFIF_APLAY_SUB.ItemsSource = TAKHFIF_APLAY_DATA;
            }
        }
        private void TAKHFIF_APLAY_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //Disabled Codes #DeActive
            //TAKHFIF_APLAY_DATA
            //var ROW_TAKHFIF = (e.Row.Item as TAKHFIF_APLAY);
            ////كد تخفيف  تخفيف  //TID
            //List<MsgModel> ErrosMessages = new List<MsgModel>(); //Validations:
            //if (ROW_TAKHFIF.TID == null)
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "تخفیف خالی است." });
            //}
            //if (!int.TryParse(ROW_TAKHFIF.TID?.ToString(), out _))
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "نوع داده تخفیف مجاز نیست." });
            //}

            //if (ErrosMessages.Count > 0) // if have any error
            //{
            //    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
            //    .Select(message => new MsgModel { MessageText_U = message }).ToList();
            //    new MsgListwin(false, ErrosMessages).ShowDialog();

            //    e.Cancel = true;
            //    return;
            //}
        }
        private void TAKHFIF_APLAY_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (TAKHFIF_APLAY_SUB.IsEditing()) return;

            //if (TAKHFIF_APLAY_SUB.Items.Count > 0 && TAKHFIF_APLAY_SUB.SelectedItem != null)
            //{
            //    if (!(TAKHFIF_APLAY_SUB.SelectedItems is null))
            //    {
            //        Msgwin msgwin = new Msgwin(true, "آیا مایل به حذف هستید ؟"); msgwin.ShowDialog();
            //        if (msgwin.DialogResult == true)
            //        {
            //            bool IsDeleteSomthing = false;
            //            for (int i = 0; i < TAKHFIF_APLAY_SUB.SelectedItems.Count; i++)
            //            {
            //                var item = TAKHFIF_APLAY_SUB.SelectedItems[i];
            //                if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
            //                {
            //                    if (item.GetType().GetProperty("NUMBER").GetValue(item) is null)
            //                    {
            //                        //Msgwin msgwin1 = new Msgwin(false, "چیزی برای حذف وجود ندارند"); msgwin1.ShowDialog();
            //                        //return;
            //                        //TAKHFIF_APLAY_SUB.Items.Remove(item);
            //                        TAKHFIF_APLAY_DATA.Remove(item as TAKHFIF_APLAY);
            //                    }
            //                    else
            //                    {
            //                        var _number = item.GetType().GetProperty("NUMBER").GetValue(item);
            //                        var _kind = item.GetType().GetProperty("KIND").GetValue(item);
            //                        DeleteTAKHFIF_APLAY(Convert.ToInt32(_number), Convert.ToInt32(_kind));
            //                        IsDeleteSomthing = true;
            //                    }
            //                }
            //                else
            //                {
            //                    Msgwin msgwin1 = new Msgwin(false, "چیزی برای حذف وجود ندارند"); msgwin1.ShowDialog();
            //                    return;
            //                }
            //            }
            //            if (IsDeleteSomthing is true)
            //                TAKHFIF_APLAY_ReGetData();
            //        }
            //    }
            //}
            //else
            //{
            //    universControl.PopNotifyShow("چیزی برای حذف نیست", Pop1, Pop1Text1, Pop_Border1);
            //}
        }
        #endregion

        #region POSHTE_FACTOR
        public PAY_GETD_SUB22_MODEL? PAY_GETD_WAS_ROW_ITEM { get; set; }
        public void PAY_GETD_SUB_ReGetData()
        {
            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                //PAY_GETD_SUB22_DATA
                PAY_GETD_SUB22_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<PAY_GETD_SUB22_MODEL>($@"SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LIST_NO, KIND, SANDUGH, HES1, HES2, HES3, ESTELAM, CRT, UID, SAYADI, ID FROM PAY_GETD WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG} AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        PAY_GETD_SUB22_DATA.Add(item);
                    }
                }
            }
        }
        private void PAY_GETD_SUB22_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (PAY_GETD_SUB22.SelectedItem != null)
            {
                if (PAY_GETD_SUB22.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    PAY_GETD_WAS_ROW_ITEM = ((PAY_GETD_SUB22_MODEL)PAY_GETD_SUB22.SelectedItem).Clone() as PAY_GETD_SUB22_MODEL;
                }
            }
        }
        private void PAY_GETD_SUB22_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            #region REFILL_CURRENTS_

            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);

            // = e.Column.SortMemberPath;
            var PAY_GETD_SUB22_ROW_INDEX = row_index;
            // = e.Column.DisplayIndex;

            //CELL
            //var rowContainer = INVO_LST_sub.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
            //DataGridCellsPresenter presenter = CL_LMethods.GetVisualChild<DataGridCellsPresenter>(rowContainer);
            //DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            //if (cell == null)
            //{
            //    INVO_LST_sub.ScrollIntoView(rowContainer, INVO_LST_sub.Columns[CURRENT_COLUMN_INDEX]);
            //    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(CURRENT_COLUMN_INDEX);
            //}
            //var PAY_GETD_SUB22_CELL_ROW = cell;
            //CELL

            ComboBox Comboval = null; TextBox TexboVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            object PAY_GETD_SUB22_ENTERED_VALUE;
            if (!ReferenceEquals(Comboval, null))
                PAY_GETD_SUB22_ENTERED_VALUE = Comboval.SelectedValue;
            else
                PAY_GETD_SUB22_ENTERED_VALUE = TexboVal.Text.Trim();

            var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;


            var column = e.Column.SortMemberPath;
            PAY_GETD_SUB22_ROW_ITEMS?.SetTouched(column);
            #endregion

            #region SET_NULL_IF_ROW_IS_NOT_VALID
            //بررسی در صورت تغییر نال کردن برای جلوگیری از اشتباه
            if (e.Column.SortMemberPath == "N_KOL")
            {
                if (PAY_GETD_WAS_ROW_ITEM.N_KOL != PAY_GETD_SUB22_ROW_ITEMS.N_KOL) //تغییر یافته
                {
                    //معین بانک
                    var comboBox = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_MOIN").GetCellContent(e.Row) as ComboBox;
                    comboBox.ItemsSource = null;

                    //تفضیلی
                    var comboBox1 = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
                    comboBox1.ItemsSource = null;
                }
            }
            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (PAY_GETD_WAS_ROW_ITEM.N_MOIN != PAY_GETD_SUB22_ROW_ITEMS.N_MOIN) //تغییر یافته
                {
                    //تفضیلی
                    var comboBox1 = PAY_GETD_SUB22.Columns.FirstOrDefault(c => c.SortMemberPath.ToString() == "N_TAF").GetCellContent(e.Row) as ComboBox;
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
                if (!IsNull(PAY_GETD_SUB22_ROW_ITEMS?.N_SERI) && !IsNull(PAY_GETD_SUB22_ROW_ITEMS?.BANK))
                {
                    if (PAY_GETD_SUB22_ROW_ITEMS?.ID == null || PAY_GETD_SUB22_ROW_ITEMS?.BANK != PAY_GETD_WAS_ROW_ITEM?.BANK || PAY_GETD_SUB22_ROW_ITEMS?.N_SERI != PAY_GETD_WAS_ROW_ITEM?.N_SERI)
                    {
                        var filter = "N_SERI=" + PAY_GETD_SUB22_ROW_ITEMS.N_SERI + " AND BANK = " + PAY_GETD_SUB22_ROW_ITEMS.BANK;
                        var rst = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {filter} ").FirstOrDefault();
                        if (rst != null)
                        {
                            new Msgwin(false, "چكي با همين سريال و با همين بانك قبلا ثبت شده است  مطمئن شويد كه عمليات را درست انجام مي دهيد. بعداز زدن اينتر مشخصات چك ثبت شده را مشاهده خواهيد نمود").ShowDialog();

                            var rst2 = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.ADA + "' OR HES = '" + Baseknow.ADV + "' ) AND (BES > 0) AND (BANK = "
                                + PAY_GETD_SUB22_ROW_ITEMS?.BANK + ") AND (N_SERI = " + PAY_GETD_SUB22_ROW_ITEMS.N_SERI + ")").FirstOrDefault();
                            if (rst2 != null)
                            {
                                new Msgwin(false, "اين چك در سند شماره " + rst2 + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را تغییر دهید").ShowDialog();
                            }
                            else
                            {
                                PAY_GETD_SUB22_ROW_ITEMS.ID = rst.ID; //برای اینکه آپدیت بشه نه INSERT

                                PAY_GETD_SUB22_ROW_ITEMS.N_SERI = rst.N_SERI;
                                PAY_GETD_SUB22_ROW_ITEMS.BANK = rst.BANK;

                                PAY_GETD_SUB22_ROW_ITEMS.DATE_S = rst.DATE_S;
                                PAY_GETD_SUB22_ROW_ITEMS.RADIF = rst.RADIF;
                                PAY_GETD_SUB22_ROW_ITEMS.SHOBEH = rst.SHOBEH;
                                PAY_GETD_SUB22_ROW_ITEMS.DATE = rst.DATE;
                                PAY_GETD_SUB22_ROW_ITEMS.NAME_TAH = rst.NAME_TAH;
                                PAY_GETD_SUB22_ROW_ITEMS.N_HESAB = rst.N_HESAB;
                                PAY_GETD_SUB22_ROW_ITEMS.MABL = rst.MABL;

                                if (rst?.N_KOL != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL = rst?.N_KOL;
                                if (rst?.N_MOIN != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN = rst?.N_MOIN;
                                if (rst?.N_TAF != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF = rst?.N_TAF;
                                if (rst?.N_TAF2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF2 = rst?.N_TAF2;
                                if (rst?.N_TAF3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF3 = rst?.N_TAF3;

                                if (PAY_GETD_SUB22_ROW_ITEMS?.N_KOL?.ToString() == "911") //از نوع حذف شده انتظامی
                                {
                                    if (PAY_GETD_SUB22_ROW_ITEMS?.N_KOL?.ToStringNullSafe() != Baseknow.BANKHA?.ToStringNullSafe())
                                    {
                                        if (rst?.N_KOL != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL = null;
                                        if (rst?.N_MOIN != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN = null;
                                        if (rst?.N_TAF != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF = null;
                                    }

                                    if (rst?.N_KOL2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL2 = null;
                                    if (rst?.N_MOIN2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN2 = null;
                                    if (rst?.N_TAF2 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF2 = null;

                                    if (rst?.N_KOL3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_KOL3 = null;
                                    if (rst?.N_MOIN3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_MOIN3 = null;
                                    if (rst?.N_TAF3 != null) PAY_GETD_SUB22_ROW_ITEMS.N_TAF3 = null;
                                }
                            }

                        }
                    }
                }
                #endregion
            }
            if (e.Column.SortMemberPath == "DATE_S") //تاریخ سررسید
            {
                //if (CL_HESABDARI.CHEKDATEM((long)PAY_GETD_SUB22_ROW_ITEMS.DATE_S, false) is true) //تاریخ صحیح نیست
                //{
                //    PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                //}
                string date_n_val = PAY_GETD_SUB22_ROW_ITEMS.DATE_S.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                        universControl.PopNotifyShow("تاریخ سررسید صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                }
                else
                {
                    PAY_GETD_SUB22_ROW_ITEMS.DATE_S = null;
                    universControl.PopNotifyShow("تاریخ سررسید نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }
            }
            if (e.Column.SortMemberPath == "DATE") //تاريخ دريافت
            {
                string date_n_val = PAY_GETD_SUB22_ROW_ITEMS.DATE.ToStringNullSafe().ToRawTarikh();
                if (!string.IsNullOrEmpty(date_n_val))
                {
                    if (!Tarikh.IsValidedDate(date_n_val))
                    {
                        PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                        universControl.PopNotifyShow("تاريخ دريافت صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                        return;
                    }
                    else
                    {
                        if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                        {
                            PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                            universControl.PopNotifyShow(".تاريخ دريافت به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                            return;
                        }
                    }
                }
                else
                {
                    PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                    universControl.PopNotifyShow("تاريخ دريافت نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                    return;
                }

                //if (CL_HESABDARI.CHEKDATEM(Convert.ToInt64(PAY_GETD_SUB22_ROW_ITEMS.DATE), Convert.ToBoolean(Baseknow.CTL_DT)) == true) //Return true mean's Problem
                //{
                //    PAY_GETD_SUB22_ROW_ITEMS.DATE = null;
                //}
            }
            if (e.Column.SortMemberPath == "SANDUGH")
            {
                //در RowEnd لاگ میزنم
                //rst.Open("dbo.PAY_GETD_LOG", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //rst.AddNew();
                //rst.update();
            }
            if (e.Column.SortMemberPath == "VAZ")
            {
            }
            if (e.Column.SortMemberPath == "SAYADI")
            {
                List<MsgModel> ErrosMessages = new List<MsgModel>();
                var FINAL_CROW_ITEM = PAY_GETD_SUB22_ROW_ITEMS;
                var DG = PAY_GETD_SUB22;

                if (!double.TryParse(FINAL_CROW_ITEM.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_SERI?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.DATE?.ToString(), out int _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.BANK?.ToString(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
                }
                if (string.IsNullOrEmpty(FINAL_CROW_ITEM.BANK?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
                }
                if (!double.TryParse(FINAL_CROW_ITEM.DATE_S?.ToString(), out double _))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.MABL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.MABL?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_KOL?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_MOIN?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_TAF?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.VAZ?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.VAZ?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت چک صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.SANDUGH?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.SANDUGH?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "مقعیت چک صحیح نیست" });
                }

                if (ErrosMessages.Count > 0)
                {
                    if (!Keyboard.IsKeyDown(Key.Escape))
                    {
                        DG.CellEditEnding -= PAY_GETD_SUB22_CellEditEnding;
                        DG.RowEditEnding -= PAY_GETD_SUB22_RowEditEnding;

                        //DG.CancelEdit(DataGridEditingUnit.Cell);
                        DG.CancelEdit();

                        DG.RowEditEnding += PAY_GETD_SUB22_RowEditEnding;
                        DG.CellEditEnding += PAY_GETD_SUB22_CellEditEnding;

                        ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                            .Select(message => new MsgModel { MessageText_U = message }).ToList();
                        new MsgListwin(false, ErrosMessages).ShowDialog();

                        return;
                    }
                }
            }
            //DATE - تاريخ دريافت   |   DATE_S - تاريخ سررسيد
        }
        private void PAY_GETD_SUB22_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            #region WORKS
            //var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;
            //if (n_MOINColumn.ItemsSource is null) //MOIN
            //{
            //    if (PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null)
            //    {
            //        //معین بانک
            //        n_MOINColumn.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETD_SUB22_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
            //    }
            //}
            //if (n_TAFColumn.ItemsSource is null) //TAFZILY
            //{
            //    if (PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null && PAY_GETD_SUB22_ROW_ITEMS.N_MOIN is not null)
            //    {
            //        //تفضیلی
            //        n_TAFColumn.ItemsSource = dbms.DoGetDataSQL<_HES_QRE3_>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) ={PAY_GETD_SUB22_ROW_ITEMS.N_MOIN}) AND ((TDETA_HES.N_KOL) ={PAY_GETD_SUB22_ROW_ITEMS.N_KOL}))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
            //    }
            //}
            #endregion

            var PAY_GETD_SUB22_ROW_ITEMS = e.Row.Item as PAY_GETD_SUB22_MODEL;

            int DefVale = 0;
            ComboBox THE_COMBO = e.EditingElement as ComboBox;

            if (e.Column.SortMemberPath == "N_MOIN")
            {
                if (!(e.EditingElement is null) && PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //معین بانک
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<HES_QRE2>($"SELECT     DETA_HES.NUMBER, DETA_HES.NAME FROM DETA_HES WHERE     (((DETA_HES.N_KOL) = {PAY_GETD_SUB22_ROW_ITEMS.N_KOL})) GROUP BY DETA_HES.NUMBER, DETA_HES.NAME ORDER BY DETA_HES.NAME").ToList();
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
                if (!(e.EditingElement is null) && PAY_GETD_SUB22_ROW_ITEMS.N_KOL is not null && PAY_GETD_SUB22_ROW_ITEMS.N_MOIN is not null)
                {
                    DefVale = Convert.ToInt32((e.EditingElement as ComboBox).SelectedValue);
                    //تفضیلی
                    THE_COMBO.ItemsSource = dbms.DoGetDataSQL<CUSTOM_HESABHA>($"SELECT TDETA_HES.TNUMBER, TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) =" + PAY_GETD_SUB22_ROW_ITEMS.N_MOIN + ") AND ((TDETA_HES.N_KOL) =" + PAY_GETD_SUB22_ROW_ITEMS.N_KOL + "))GROUP BY TDETA_HES.TNUMBER, TDETA_HES.NAME ORDER BY TDETA_HES.NAME").ToList();
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
        private void PAY_GETD_SUB22_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (PAY_GETD_SUB22.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = PAY_GETD_SUB22.CurrentCell.Column.SortMemberPath;
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
        private void PAY_GETD_SUB22_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            if (e.Row.Item == null) { return; }


            var FINAL_CROW_ITEM = (e.Row.Item as PAY_GETD_SUB22_MODEL);

            //Validations:
            #region Validations
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (!double.TryParse(FINAL_CROW_ITEM.N_SERI?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_SERI?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "شماره سریال چک صحیح وارد نشده" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.DATE?.ToString(), out int _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ دریافت صحیح وارد نشده" });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.BANK?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک صحیح انتخاب نشده" });
            }
            if (string.IsNullOrEmpty(FINAL_CROW_ITEM.BANK?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "بانک خالی است" });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.DATE_S?.ToString(), out double _))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ سررسید صحیح وارد نشده" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.MABL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.MABL?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ صحیح وارد نشده" });
            }

            //if (string.IsNullOrEmpty(FINAL_CROW_ITEM.N_HESAB))
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "جاری چک وارد نشده !" });
            //}

            bool hasKol = FINAL_CROW_ITEM.N_KOL.HasValue;
            bool hasMoin = FINAL_CROW_ITEM.N_MOIN.HasValue;
            bool hasTaf = FINAL_CROW_ITEM.N_TAF.HasValue;

            if ((hasKol || hasMoin || hasTaf) && !(hasKol && hasMoin && hasTaf)) // اگر هر کدوم مقدار داشت ولی همه‌شون ندارن => خطا
            {
                if (!int.TryParse(FINAL_CROW_ITEM.N_KOL?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_KOL?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب کل صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_MOIN?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_MOIN?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب معین صحیح نیست" });
                }
                if (!int.TryParse(FINAL_CROW_ITEM.N_TAF?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.N_TAF?.ToString()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "حساب تفضیلی صحیح نیست" });
                }
            }

            if (!int.TryParse(FINAL_CROW_ITEM.VAZ?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.VAZ?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "وضعیت چک صحیح نیست" });
            }
            if (!int.TryParse(FINAL_CROW_ITEM.SANDUGH?.ToString(), out int _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.SANDUGH?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مقعیت چک صحیح نیست" });
            }

            var DG = PAY_GETD_SUB22;
            var hasError = false;
            var erg = e.Row.GetIndex();

            DataGridRow row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            if (row == null)
            {
                DG.UpdateLayout();
                DG.ScrollIntoView(DG.Items[erg]);
                row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            }
            if (row != null && Validation.GetHasError(row))
            {
                hasError = true;
            }
            hasError = (from object i in DG.ItemsSource
                        let c = row
                        where c != null && Validation.GetHasError(c)
                        select c).Any();

            if (ErrosMessages.Count > 0 || hasError)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                 .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                DG.Dispatcher.Invoke(() =>
                {
                    //e.Cancel = true;

                    DG.CellEditEnding -= PAY_GETD_SUB22_CellEditEnding;
                    DG.RowEditEnding -= PAY_GETD_SUB22_RowEditEnding;

                    DG.CancelEdit();

                    DG.RowEditEnding += PAY_GETD_SUB22_RowEditEnding;
                    DG.CellEditEnding += PAY_GETD_SUB22_CellEditEnding;
                });

                #region MyRegion

                //e.Cancel = true;

                // عملیات مهم: دوباره سطر را به حالت ویرایش بازگردانید (به صورت async تا از تراکنش خارج شود)
                //DG.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    DG.CellEditEnding -= PAY_GETD_SUB22_CellEditEnding;
                //    DG.RowEditEnding -= PAY_GETD_SUB22_RowEditEnding;

                //    DG.SelectedItem = FINAL_CROW_ITEM;
                //    DG.ScrollIntoView(FINAL_CROW_ITEM);
                //    DG.CurrentCell = new DataGridCellInfo(FINAL_CROW_ITEM, DG.Columns[0]);
                //    DG.BeginEdit();

                //    DG.RowEditEnding += PAY_GETD_SUB22_RowEditEnding;
                //    DG.CellEditEnding += PAY_GETD_SUB22_CellEditEnding;

                //}), System.Windows.Threading.DispatcherPriority.Background);
                #endregion

                return;
            }
            #endregion

            #region Form_BeforeInsert
            var rst = dbms.DoGetDataSQL<string?>("SELECT TDETA_HES.NAME FROM TDETA_HES WHERE (((TDETA_HES.TNUMBER) = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + " ) And ((TDETA_HES.NUMBER) = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") And ((TDETA_HES.N_KOL) = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + " )) GROUP BY TDETA_HES.NAME").ToList();
            if (rst.Count > 0)
            {
                FINAL_CROW_ITEM.NAME_TAH = rst.FirstOrDefault();
            }
            #endregion

            #region Form_BeforeUpdate
            long dfn;
            long rdn;
            if (FINAL_CROW_ITEM?.RADIF is null)
            {
                var RST2 = dbms.DoGetDataSQL<DAFT_ASN>("SELECT     TOP 100 PERCENT FIRSTNUM, BOOKNUM FROM dbo.DAFT_ASN ORDER BY BOOKNUM DESC").ToList();
                if (RST2.Count > 0)
                {
                    rdn = (long)RST2.FirstOrDefault().FIRSTNUM;
                    dfn = (long)RST2.FirstOrDefault().BOOKNUM;
                }
                else
                {
                    new Msgwin(false, "اطلاعات پايه مربوط به دفتر اسناد دريافتني در مشخصات سيستم تعريف نشده است - شماره شروع دفتر اسناد دريافتني و شماره دفتر بايد مشخص شود براي ثبت چك جاري خودم آن را ايجاد مي نمايم شماره شروع :1 شماره دفتر :1").ShowDialog();

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DAFT_ASN(FIRSTNUM, BOOKNUM)
                                         VALUES({RST2.FirstOrDefault().FIRSTNUM},
                                         {RST2.FirstOrDefault().BOOKNUM})");

                    rdn = 1L;
                    dfn = 1L;
                }

                var rst_1 = dbms.DoGetDataSQL<double?>("SELECT Max(PAY_GETD.RADIF) AS MaxOfRADIF  FROM PAY_GETD WHERE ANBAR = " + dfn).ToList();
                if (rst_1.Count == 0 || rst_1.FirstOrDefault() is null)
                {
                    FINAL_CROW_ITEM.RADIF = rdn;
                    FINAL_CROW_ITEM.ANBAR = dfn;
                }
                else
                {
                    FINAL_CROW_ITEM.RADIF = rst_1.FirstOrDefault() + 1;
                    FINAL_CROW_ITEM.ANBAR = dfn;
                }
            }
            #endregion

            //SANDUGH_AfterUpdate , VAZ_AfterUpdate {
            var N_SERI = FINAL_CROW_ITEM.N_SERI;
            var BANK = FINAL_CROW_ITEM.BANK;
            var DATE_S = FINAL_CROW_ITEM.DATE_S;
            var DATE_V = CL_HESABDARI.FARSIDATE();
            var DATETIM = DateTime.Now;
            var VAZ = FINAL_CROW_ITEM.VAZ;
            var SANDUGH = FINAL_CROW_ITEM.SANDUGH;
            var USER_NAME = CL_HESABDARI.UCurrentUser();

            try
            {
                dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                     VALUES({N_SERI},
                                     {BANK}   ,
                                     {DATE_S}   ,
                                     {DATE_V}   ,
                                     GETDATE(),
                                     {VAZ} ,
                                     {SANDUGH}   ,
                                     N'{USER_NAME}'
                                     )");
            }
            catch (Exception) { }


            try
            {
                //CUST_NO : 
                FINAL_CROW_ITEM.CUST_NO = CUST_NO.SelectedValue.ToString();

                //Final Saving ...
                if (FINAL_CROW_ITEM.ID is not null && FINAL_CROW_ITEM?.ID > 0) //Update
                {
                    string sql = @"
                                UPDATE PAY_GETD
                                SET N_SERI = @N_SERI,
                                    BANK = @BANK,
                                    DATE_S = @DATE_S,
                                    DATE = @DATE,
                                    SHOBEH = @SHOBEH,
                                    MABL = @MABL,
                                    NAME_TAH = @NAME_TAH,
                                    N_HESAB = @N_HESAB,
                                    N_KOL = @N_KOL,
                                    N_MOIN = @N_MOIN,
                                    N_TAF = @N_TAF,
                                    NUMBER = @NUMBER,
                                    TAG = @TAG,
                                    ANBAR = @ANBAR,
                                    VAZ = @VAZ,
                                    KIND = @KIND,
                                    SANDUGH = @SANDUGH,
                                    SAYADI = @SAYADI WHERE ID = @ID";

                    var param = new
                    {
                        N_SERI = FINAL_CROW_ITEM.N_SERI,
                        BANK = FINAL_CROW_ITEM.BANK,
                        DATE_S = FINAL_CROW_ITEM.DATE_S,
                        DATE = FINAL_CROW_ITEM.DATE,
                        SHOBEH = FINAL_CROW_ITEM.SHOBEH,
                        MABL = FINAL_CROW_ITEM.MABL,
                        NAME_TAH = FINAL_CROW_ITEM.NAME_TAH,
                        N_HESAB = FINAL_CROW_ITEM.N_HESAB,
                        N_KOL = FINAL_CROW_ITEM.N_KOL,
                        N_MOIN = FINAL_CROW_ITEM.N_MOIN,
                        N_TAF = FINAL_CROW_ITEM.N_TAF,
                        NUMBER = Convert.ToDouble(NUMBER.Text), // Or parse to int/double if needed
                        TAG = hTAG,
                        ANBAR = 1,
                        VAZ = FINAL_CROW_ITEM.VAZ,
                        KIND = FINAL_CROW_ITEM.KIND,
                        SANDUGH = FINAL_CROW_ITEM.SANDUGH,
                        SAYADI = FINAL_CROW_ITEM.SAYADI,
                        ID = FINAL_CROW_ITEM.ID
                    };
                    dbms.DoExecuteSQL(sql, param);
                }
                else //Insert
                {
                    string sql = @"
                        INSERT INTO PAY_GETD
                            (N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_KOL, N_MOIN, N_TAF, NUMBER, TAG, ANBAR, RADIF, VAZ, KIND, SANDUGH, SAYADI)
                        OUTPUT INSERTED.ID
                        VALUES
                            (@N_SERI, @BANK, @DATE_S, @DATE, @SHOBEH, @MABL, @NAME_TAH, @N_HESAB, @N_KOL, @N_MOIN, @N_TAF, @NUMBER, @TAG, @ANBAR, @RADIF, @VAZ, @KIND, @SANDUGH, @SAYADI)";
                    var param = new
                    {
                        N_SERI = FINAL_CROW_ITEM.N_SERI,
                        BANK = FINAL_CROW_ITEM.BANK,
                        DATE_S = FINAL_CROW_ITEM.DATE_S,
                        DATE = FINAL_CROW_ITEM.DATE,
                        SHOBEH = FINAL_CROW_ITEM.SHOBEH,
                        MABL = FINAL_CROW_ITEM.MABL,
                        NAME_TAH = FINAL_CROW_ITEM.NAME_TAH,
                        N_HESAB = FINAL_CROW_ITEM.N_HESAB,
                        N_KOL = FINAL_CROW_ITEM.N_KOL,
                        N_MOIN = FINAL_CROW_ITEM.N_MOIN,
                        N_TAF = FINAL_CROW_ITEM.N_TAF,
                        NUMBER = Convert.ToDouble(NUMBER.Text), // Or parse to int/double if needed
                        TAG = hTAG,
                        ANBAR = 1,
                        RADIF = FINAL_CROW_ITEM.RADIF,
                        VAZ = FINAL_CROW_ITEM.VAZ,
                        KIND = FINAL_CROW_ITEM.KIND,
                        SANDUGH = FINAL_CROW_ITEM.SANDUGH,
                        SAYADI = FINAL_CROW_ITEM.SAYADI,
                    };
                    var GOTID = dbms.DoGetDataSQL<long?>(sql, param).FirstOrDefault();
                    FINAL_CROW_ITEM.ID = GOTID;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده ی تکراری وارد شده است , آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception ex)
            {
                CL_LMethods.DoWriteMyLog("خطا در ذخیره PAY_GETD_SUB22_RowEditEnding فاکتور فروش", ex);
                new Msgwin(false, "خطا در انجام عملیات").Show(); return;
            }

            NCHK.Text = PAY_GETD_SUB22_DATA.Sum(x => x.MABL).ToString();

            CalculateAdvanceDiscount();

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

            if (PAY_GETD_SUB22.Items.Count > 0 && PAY_GETD_SUB22.SelectedItem != null)
            {
                if (!(PAY_GETD_SUB22.SelectedItems is null))
                {
                    var editableCollectionView = PAY_GETD_SUB22.Items as IEditableCollectionView;
                    if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
                    {
                        try { editableCollectionView.CancelEdit(); } catch { }
                    }

                    bool errors = default;
                    errors = (from object i in PAY_GETD_SUB22.ItemsSource
                              let c = PAY_GETD_SUB22.ItemContainerGenerator.ContainerFromItem(i)
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
                        ESLAH_Click(null, null);

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور فروش => چک های دریافتی پشت فاکتور",
                                recordId: NUMBER1.Text,
                                oldValue: "TAG = 13",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < PAY_GETD_SUB22.SelectedItems.Count; i++)
                        {
                            var item = PAY_GETD_SUB22.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                if (item.GetType().GetProperty("ID").GetValue(item) is null)
                                {
                                    PAY_GETD_SUB22_DATA.Remove(item as PAY_GETD_SUB22_MODEL);

                                    //var before = PAY_GETD_SUB22.CanUserAddRows;
                                    PAY_GETD_SUB22.CanUserAddRows = false;
                                    PAY_GETD_SUB22.CanUserAddRows = true;
                                }
                                else
                                {
                                    var THE_N_SERI = item.GetType().GetProperty("N_SERI").GetValue(item);
                                    var THE_BANK = item.GetType().GetProperty("BANK").GetValue(item);

                                    var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD WHERE  N_SERI=" + THE_N_SERI + " AND BANK = " + THE_BANK + " AND (N_KOL IS NULL OR N_KOL <> 911) ").ToList();
                                    if (rst.Count > 0)
                                    {
                                        if ((!IsNull(rst?.FirstOrDefault()?.N_KOL2) && rst?.FirstOrDefault()?.N_KOL2 != 911) || !IsNull(rst?.FirstOrDefault()?.N_KOL3))
                                        {
                                            Msgwin msgwin1 = new Msgwin(false, "چكي كه وصولي يا واگذاري يا برگشتي خورده قابل حذف نيست");
                                            msgwin1.ShowDialog();
                                        }
                                        else
                                        {
                                            if ((rst.FirstOrDefault().N_KOL == Baseknow.BANKHA || rst.FirstOrDefault().N_KOL == 911) || IsNull(rst.FirstOrDefault().N_KOL))
                                            {
                                                string _where = " WHERE  N_SERI=" + THE_N_SERI + " AND BANK = " + THE_BANK;

                                                rst.FirstOrDefault().N_KOL = 911;
                                                rst.FirstOrDefault().N_MOIN = 1;
                                                rst.FirstOrDefault().N_TAF = 1;
                                                rst.FirstOrDefault().HES1 = "911-1-1";

                                                dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET N_KOL = 911 , N_MOIN = 1 , N_TAF = 1 , HES1 = N'911-1-1' {_where} ");
                                                IsDeleteSomthing = true;
                                            }

                                        }
                                    }
                                    CL_HESABDARI.GETDLOG(1, THE_N_SERI.ToString(), (int)THE_BANK, rst.FirstOrDefault().DATE_S, (int)rst.FirstOrDefault().SANDUGH);
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
                            PAY_GETD_SUB_ReGetData();

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

        #region Sayer
        class QRE_VISIT1
        {
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
        }
        public VISITOR_DTL _VISITOR_DTL_WAS_ROW_ITEM { get; set; } = new VISITOR_DTL();
        public void VISITOR_DTL_SUB_ReGetData()
        {
            //SAYER_VISITOR_DATA

            if (!string.IsNullOrEmpty(NUMBER.Text) && NUMBER.Text != "0") //Did Saved
            {
                SAYER_VISITOR_DATA?.Clear();
                var QRE_LST = dbms.DoGetDataSQL<VISITOR_DTL>($@"SELECT NUMBER, TAG, CUST_NO, DARSAD, PURSANT, TOZIH, STAT, PORID, CRT, UID, ID, LOG FROM VISITOR_DTL WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG}").ToList();
                if (QRE_LST.Count > 0)
                {
                    foreach (var item in QRE_LST)
                    {
                        var CUSTDATA = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + item.CUST_NO + "'").FirstOrDefault();
                        if (CUSTDATA != null)
                        {
                            item.CUST_NO_NAME = CUSTDATA.NAME;
                        }
                        else
                        {
                            item.CUST_NO_NAME = "مشتری یافت نشد";
                        }
                        if (item.DARSAD != null)
                        {
                            item.DARSAD = (double)item.DARSAD;
                        }

                        SAYER_VISITOR_DATA.Add(item);
                    }
                }
            }
            //SAYER_VISITOR_DATA.ItemsSource = PAY_GETD_SUB22_DATA;
        }
        public VISITOR_DTL CURRENT_ROW_VISITOR { get; set; }
        public long OKDATE { get; private set; }
        public long OKTIME { get; private set; }
        public int? Maghsad_Havaleh_Directyfactor { get; private set; }

        private void VISITOR_DTL_SUB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (NowIsReady && !(e is null) && VISITOR_DTL_SUB.SelectedItem != null)
            {
                if (VISITOR_DTL_SUB.SelectedItem.ToString() != "{NewItemPlaceholder}")
                {
                    _VISITOR_DTL_WAS_ROW_ITEM = ((VISITOR_DTL)VISITOR_DTL_SUB.SelectedItem)?.Clone() as VISITOR_DTL;
                }
            }
        }
        private void VISITOR_DTL_SUB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            #region REFILL_CURRENTS_
            DataGridColumn col1 = e.Column;
            DataGridRow row1 = e.Row;
            int row_index = ((DataGrid)sender).ItemContainerGenerator.IndexFromContainer(row1);
            var PAY_GETD_SUB22_ROW_INDEX = row_index;
            var rowContainer = INVO_LST_sub.ItemContainerGenerator.ContainerFromIndex(row_index) as DataGridRow;
            ComboBox Comboval = null;
            TextBox TexboVal = null;
            CheckBox CheckVal = null;
            if (!(e.EditingElement is null) && e.EditingElement is TextBox)
            {
                TexboVal = (TextBox)e.EditingElement;
            }
            if (!(e.EditingElement is null))
            {
                Comboval = e.EditingElement as ComboBox;
            }
            if (!(e.EditingElement is null))
            {
                CheckVal = e.EditingElement as CheckBox;
            }

            string? VISITOR_DTL_SUB_ENTERED_VALUE = null;
            if (!ReferenceEquals(Comboval, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = Comboval.SelectedValue.ToStringNullSafe();
            else if (!ReferenceEquals(CheckVal, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = CheckVal.IsChecked.ToStringNullSafe();
            else if (!ReferenceEquals(TexboVal, null))
                VISITOR_DTL_SUB_ENTERED_VALUE = TexboVal.Text.Trim();

            if (e.Row == null)
            {
                return;
            }
            else
            {
                CURRENT_ROW_VISITOR = e.Row.Item as VISITOR_DTL;
                if (CURRENT_ROW_VISITOR is null)
                    return;
            }

            #endregion

            if (e.Column.SortMemberPath == "CUST_NO_NAME") //CUST_NO_NAME == CUST_NO 112-1-1 محمدی دهقان تستی
            {
                #region CUST_NO_NotInList
                if (_VISITOR_DTL_WAS_ROW_ITEM.CUST_NO_NAME != VISITOR_DTL_SUB_ENTERED_VALUE) // نام مشتری جدید وارد شده
                {
                    var _SelectedHesab_ = CL_LMethods.GetHesabBySearch(Comboval, dbms, VISITOR_DTL_SUB_ENTERED_VALUE);
                    if (string.IsNullOrEmpty(_SelectedHesab_?.hes))
                    {
                        CURRENT_ROW_VISITOR.CUST_NO = _VISITOR_DTL_WAS_ROW_ITEM.CUST_NO;
                        CURRENT_ROW_VISITOR.CUST_NO_NAME = _VISITOR_DTL_WAS_ROW_ITEM.CUST_NO_NAME;
                        universControl.PopNotifyShow($"حساب نمی تواند خالی باشد", Pop1, Pop1Text1, Pop_Border1);
                    }
                    else
                    {
                        CURRENT_ROW_VISITOR.CUST_NO = _SelectedHesab_.hes;
                        CURRENT_ROW_VISITOR.CUST_NO_NAME = _SelectedHesab_.NAME;
                    }

                    var tozihdata = dbms.DoGetDataSQL<string?>("SELECT  TOZIH FROM dbo.TDETA_HES WHERE RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) = N'" + CURRENT_ROW_VISITOR.CUST_NO + "'").ToList();
                    if (tozihdata.Count > 0 && !string.IsNullOrEmpty(tozihdata?.FirstOrDefault()))
                    {
                        if (Information.IsNumeric(tozihdata.FirstOrDefault()))
                        {
                            CURRENT_ROW_VISITOR.DARSAD = Convert.ToDouble(tozihdata.FirstOrDefault().Replace("%", ""));
                            CURRENT_ROW_VISITOR.PURSANT = (double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * CURRENT_ROW_VISITOR.DARSAD / 100);
                            if ((bool)CURRENT_ROW_VISITOR.STAT)
                            {
                                CURRENT_ROW_VISITOR.STAT = false;
                            }
                        }
                    }
                }
                #endregion

                #region CUST_NO_AfterUpdate
                var rst = dbms.DoGetDataSQL<string?>("SELECT  TOZIH FROM dbo.CUST_HESAB WHERE     (hes = N'" + CURRENT_ROW_VISITOR.CUST_NO + "')").ToList();
                if (rst.Count > 0)
                {
                    if (!string.IsNullOrEmpty(rst?.FirstOrDefault()) && Information.IsNumeric(rst.FirstOrDefault()))
                    {
                        CURRENT_ROW_VISITOR.DARSAD = Convert.ToDouble(rst.FirstOrDefault());
                        CURRENT_ROW_VISITOR.PURSANT = (double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * CURRENT_ROW_VISITOR.DARSAD / 100);
                        if ((bool)CURRENT_ROW_VISITOR.STAT)
                        {
                            CURRENT_ROW_VISITOR.STAT = false;
                        }
                    }
                }
                #endregion
            }

            if (e.Column.SortMemberPath == "DARSAD")
            {
                if (!IsValidPercentage(VISITOR_DTL_SUB_ENTERED_VALUE))
                {
                    CURRENT_ROW_VISITOR.DARSAD = null;
                    new Msgwin(false, "درصد صحیح نیست").ShowDialog();
                    return;
                }
                else
                {
                    //DARSAD_AfterUpdate
                    CURRENT_ROW_VISITOR.PURSANT = (double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * CURRENT_ROW_VISITOR.DARSAD / 100);
                    if (Convert.ToBoolean(CURRENT_ROW_VISITOR.STAT))
                    {
                        CURRENT_ROW_VISITOR.STAT = false;
                    }
                }
                #region DARSAD_BeforeUpdate
                if (!IsNull(CURRENT_ROW_VISITOR.PORID))
                {
                    Msgwin msgwin = new Msgwin(true, "باتوجه به اينكه اين سطر دراي الگوي پرداخت پورسانت ميباشد با زدن درصد الگوي آن حذف ميشود آيا از ادامه عمليات اطمينان داريد.");
                    if (msgwin.DialogResult is true)
                    {
                        CURRENT_ROW_VISITOR.PORID = null;
                    }
                }
                #endregion
            }

            if (e.Column.SortMemberPath == "PURSANT")
            {
                if (!string.IsNullOrEmpty(VISITOR_DTL_SUB_ENTERED_VALUE))
                {
                    //PURSANT_AfterUpdate
                    if (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text) + Convert.ToDouble(MBAA.Text) != 0)
                    {
                        CURRENT_ROW_VISITOR.DARSAD = CURRENT_ROW_VISITOR.PURSANT / (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * 100;

                        CURRENT_ROW_VISITOR.DARSAD = (double)CURRENT_ROW_VISITOR.DARSAD;
                    }
                    else
                    {
                        CURRENT_ROW_VISITOR.DARSAD = 0;
                    }
                    //if (!Convert.ToBoolean(CURRENT_ROW_VISITOR.STAT))
                    //{
                    //    CURRENT_ROW_VISITOR.STAT = true;
                    //}
                }

                #region PURSANT_BeforeUpdate
                if (!IsNull(CURRENT_ROW_VISITOR.PORID))
                {
                    Msgwin msgwin1 = new Msgwin(true, "باتوجه به اينكه اين سطر دراي الگوي پرداخت پورسانت ميباشد با زدن مبلغ الگوي آن حذف ميشود آيا از ادامه عمليات اطمينان داريد.");
                    msgwin1.ShowDialog();
                    if (msgwin1.DialogResult is true)
                    {
                        CURRENT_ROW_VISITOR.PORID = null;
                    }
                }
                #endregion


            }

        }
        private void VISITOR_DTL_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel) { return; }
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            if (e.Row.Item == null) { return; }
            var FINAL_CROW_ITEM = (e.Row.Item as VISITOR_DTL);
            if (ConstructorRowDetector.IsPristine(FINAL_CROW_ITEM))
            {
                VISITOR_DTL_SUB.Dispatcher.Invoke(() =>
                {
                    VISITOR_DTL_SUB.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    VISITOR_DTL_SUB.RowEditEnding -= VISITOR_DTL_SUB_RowEditEnding;
                    VISITOR_DTL_SUB.CancelEdit();
                    VISITOR_DTL_SUB.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                    VISITOR_DTL_SUB.RowEditEnding += VISITOR_DTL_SUB_RowEditEnding;
                }); return;
            }


            // Find duplicates in the SAYER_VISITOR_DATA collection based on CUST_NO
            //var duplicates = SAYER_VISITOR_DATA
            //    .Where(x => !string.IsNullOrEmpty(x.CUST_NO)) // Ensure CUST_NO is not null or empty
            //    .GroupBy(x => x.CUST_NO)                     // Group by CUST_NO
            //    .Where(g => g.Count() > 1)                   // Filter groups with more than one entry
            //    .SelectMany(g => g.Skip(1))                  // Select all but the first entry in each group
            //    .ToList();                                   // Create a list of duplicates

            #region Validations
            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (string.IsNullOrEmpty(FINAL_CROW_ITEM.CUST_NO))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام شخص خالی است" });
            }

            if (!double.TryParse(FINAL_CROW_ITEM.DARSAD?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.DARSAD?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد خالی است!" });
            }
            if (FINAL_CROW_ITEM.DARSAD < 0 || FINAL_CROW_ITEM.DARSAD > 100)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "درصد باید بین 0 تا 100 باشد." });
            }
            if (!double.TryParse(FINAL_CROW_ITEM.PURSANT?.ToString(), out double _) || string.IsNullOrEmpty(FINAL_CROW_ITEM?.PURSANT?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ پورسانت صحیح نیست!" });
            }
            if (FINAL_CROW_ITEM.PURSANT < 0)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "مبلغ پورسانت منقی نمیتواند باشد!" });
            }

            if (FINAL_CROW_ITEM.TOZIH?.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "طول توضیح بیش از اندازه است!" });
            }
            if (FINAL_CROW_ITEM.STAT == null)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تیک مبلغ ثابت خالی است!" });
            }

            var DG = VISITOR_DTL_SUB;
            var hasError = false;
            var erg = e.Row.GetIndex();

            DataGridRow row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            if (row == null)
            {
                DG.UpdateLayout();
                DG.ScrollIntoView(DG.Items[erg]);
                row = (DataGridRow)DG.ItemContainerGenerator.ContainerFromIndex(erg);
            }
            if (row != null && Validation.GetHasError(row))
            {
                hasError = true;
            }
            hasError = (from object i in DG.ItemsSource
                        let c = row
                        where c != null && Validation.GetHasError(c)
                        select c).Any();

            // Check for duplicate CUST_NO values in the collection
            var duplicateExists = SAYER_VISITOR_DATA
                .Where(x => x != FINAL_CROW_ITEM) // Exclude the current edited item
                .Any(x => x.CUST_NO == FINAL_CROW_ITEM.CUST_NO);

            if (duplicateExists)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"این حساب '{FINAL_CROW_ITEM.CUST_NO}' قبلاً ثبت شده است. لطفاً مقدار دیگری وارد کنید." });

                DG.Dispatcher.Invoke(() =>
                {
                    DG.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    DG.RowEditEnding -= VISITOR_DTL_SUB_RowEditEnding;
                    DG.CancelEdit();
                    DG.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                    DG.RowEditEnding += VISITOR_DTL_SUB_RowEditEnding;
                });
                //var dataGrid = sender as DataGrid;
                //if (dataGrid != null)
                //{
                //    dataGrid.Dispatcher.InvokeAsync(() =>
                //    {
                //        dataGrid.CommitEdit(DataGridEditingUnit.Row, true); // Commit any pending edits
                //        dataGrid.CancelEdit();                              // Cancel the current edit
                //        dataGrid.Items.Refresh();                          // Refresh the grid
                //    });
                //}
            }

            if (ErrosMessages.Count > 0 || hasError)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();

                DG.Dispatcher.Invoke(() =>
                {
                    DG.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                    DG.RowEditEnding -= VISITOR_DTL_SUB_RowEditEnding;
                    e.Cancel = true; //DG.CancelEdit();
                    DG.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                    DG.RowEditEnding += VISITOR_DTL_SUB_RowEditEnding;
                });
                return;
            }
            #endregion

            try
            {
                long? _id_ = null;

                FINAL_CROW_ITEM.PURSANT = Math.Round((double)((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * Convert.ToDouble(FINAL_CROW_ITEM.DARSAD) / 100));

                try
                {
                    if (FINAL_CROW_ITEM?.ID is null)
                    {
                        _id_ = dbms.DoGetDataSQL<long?>($@"INSERT INTO dbo.VISITOR_DTL(NUMBER, TAG, CUST_NO, DARSAD, PURSANT, TOZIH, STAT, PORID)
                            OUTPUT INSERTED.ID
                            VALUES({NUMBER.Text},
                            {hTAG} ,
                            N'{FINAL_CROW_ITEM.CUST_NO}' ,
                            {FINAL_CROW_ITEM?.DARSAD} ,
                            {FINAL_CROW_ITEM?.PURSANT} ,
                            N'{FINAL_CROW_ITEM?.TOZIH}' ,
                            {Convert.ToByte(FINAL_CROW_ITEM.STAT)},
                            {(string.IsNullOrEmpty(FINAL_CROW_ITEM?.PORID?.ToStringNullSafe()) ? "NULL" : FINAL_CROW_ITEM?.PORID)})").FirstOrDefault();
                    }
                    else
                    {
                        dbms.DoExecuteSQL($@"UPDATE dbo.VISITOR_DTL SET 
                                         NUMBER = {NUMBER.Text}, CUST_NO = N'{FINAL_CROW_ITEM.CUST_NO}' , DARSAD = {FINAL_CROW_ITEM?.DARSAD} ,
                                         PURSANT = {FINAL_CROW_ITEM?.PURSANT} , TOZIH = N'{FINAL_CROW_ITEM.TOZIH}' , STAT = {Convert.ToByte(FINAL_CROW_ITEM.STAT)},
                                         PORID = {(string.IsNullOrEmpty(FINAL_CROW_ITEM?.PORID.ToStringNullSafe()) ? "NULL" : FINAL_CROW_ITEM?.PORID)}
                                         WHERE ID = {FINAL_CROW_ITEM?.ID}");
                    }
                }
                catch (SqlException ex)
                {
                    VISITOR_DTL_SUB.Dispatcher.Invoke(() =>
                    {
                        VISITOR_DTL_SUB.CellEditEnding -= VISITOR_DTL_SUB_CellEditEnding;
                        VISITOR_DTL_SUB.CancelEdit();
                        VISITOR_DTL_SUB.CellEditEnding += VISITOR_DTL_SUB_CellEditEnding;
                    });

                    if (ex.Number == 2627)
                    {
                        new Msgwin(false, "سطر تکراری است آنرا اصلاح کنید").ShowDialog();
                        return;
                    }
                }

                if (_id_ != null)
                {
                    FINAL_CROW_ITEM.ID = _id_;
                }

                #region Form_AfterUpdate
                var rst = dbms.DoGetDataSQL<double?>("SELECT NUMBER1  FROM HEAD_LST WHERE NUMBER1 = " + this.NUMBER.Text + " AND TAG = 4").ToList();
                if (rst.Count > 0)
                {
                    new Msgwin(false, "توجه  ! توجه  : براي اين فاكتور مرجوعي ثبت شده است اگر ويزيتور آنرا تغيير ميدهيد لازم است در فاكتور برگشت فروش هم ويزيتور آنرا اصلاح كنيد ").ShowDialog();
                }
                #endregion

                #region PORID_AfterUpdate
                long prs;
                var MBK = default(long);
                prs = 0L;
                if (!IsNull(FINAL_CROW_ITEM?.PORID))
                {
                    var ROWS = dbms.DoGetDataSQL<QRE_VISIT1>("SELECT CODE ,MABL_K - N_MOIN AS MABLK FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text).ToList();
                    for (int I = 0; I < ROWS.Count; I++)//while (!ROWS.EOF)
                    {
                        var RST2 = dbms.DoGetDataSQL<double?>("SELECT     PORSANT FROM dbo.VISITORS_PORSANT_KALA WHERE     (PORID = " + FINAL_CROW_ITEM.PORID + ") and (code = '" + ROWS[I].CODE + "')").ToList();
                        if (RST2.Count == 1)
                        {
                            prs = (long)(prs + Math.Round((double)(ROWS[I].MABLK * RST2.FirstOrDefault() / 100)));
                            MBK = (long)(MBK + ROWS[I].MABLK);
                        }
                        else
                        {
                            new Msgwin(false, "تذكر مهم :اين كالا فاقد الگو براي اين ويزيتور است و پورسانت محاسبه نشد.درصورت لزوم براي آن تعريف كنيد و همينجا مجددا الگو را انتخاب كنيد  : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(ROWS[I].CODE))).ShowDialog();
                        }
                    }
                    FINAL_CROW_ITEM.PURSANT = Math.Round((double)(prs));
                    if (MBK > 0L & prs > 0L)
                    {
                        FINAL_CROW_ITEM.DARSAD = FINAL_CROW_ITEM.PURSANT / MBK * 100;
                        FINAL_CROW_ITEM.DARSAD = (double)FINAL_CROW_ITEM.DARSAD;
                    }
                    else
                    {
                        FINAL_CROW_ITEM.DARSAD = 0;
                    }
                }
                #endregion

                //PURSANT
                if (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text) + Convert.ToDouble(MBAA.Text) != 0)
                {
                    FINAL_CROW_ITEM.DARSAD = FINAL_CROW_ITEM.PURSANT / (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * 100;
                    FINAL_CROW_ITEM.DARSAD = (double)FINAL_CROW_ITEM.DARSAD;
                }
                else
                {
                    FINAL_CROW_ITEM.DARSAD = 0;
                }

                if (FINAL_CROW_ITEM?.STAT is null)
                {
                    FINAL_CROW_ITEM.STAT = false;
                }

                double sum = SAYER_VISITOR_DATA.Sum(item => item.PURSANT ?? 0.0);
                Text190.Text = sum.ToString();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "داده ی تکراری وارد شده است , آنرا اصلاح کنید").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception ex)
            {
                CL_LMethods.DoWriteMyLog("خطا در ذخیره VISITOR_DTL_SUB_RowEditEnding فاکتور فروش", ex);
                new Msgwin(false, "خطا در انجام عملیات").Show(); return;
            }

        }
        private void VISITOR_DTL_SUB_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid?.CurrentCell.Column is null) return;

            if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null)
            {
                //if (CL_LMethods.IsNewPlaceHolder(grid, CurrentData)) //Is NewRecord
                //{
                //    if (grid.CurrentCell.Column.SortMemberPath == "DARSAD")
                //    {
                //        var opn = dbms.DoGetDataSQL<VISITOR_DARSAD>("SELECT     TOP 100 PERCENT dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT FROM         dbo.HEAD_LST INNER JOIN   dbo.VISITOR_DTL ON dbo.HEAD_LST.NUMBER = dbo.VISITOR_DTL.NUMBER AND dbo.HEAD_LST.TAG = dbo.VISITOR_DTL.TAG WHERE     (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.NUMBER <> " + NUMBER.Text + ")GROUP BY dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT,  dbo.HEAD_LST.NUMBER ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                //        if (opn.Count > 0)
                //        {
                //            CurrentData.CUST_NO = opn.FirstOrDefault().CUST_NO;
                //            CurrentData.DARSAD = opn.FirstOrDefault().DARSAD;
                //            CurrentData.PURSANT = Math.Round((Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * Convert.ToDouble(opn.FirstOrDefault().DARSAD) / 100);
                //            CurrentData.STAT = false;
                //            CurrentData.TOZIH = opn.FirstOrDefault().TOZIH;
                //        }
                //    }

                //    if (grid.CurrentCell.Column.SortMemberPath == "PORID")
                //    {
                //        var rst = dbms.DoGetDataSQL<VISITOR_DARSAD>("SELECT     TOP 100 PERCENT dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT, dbo.VISITOR_DTL.PORID FROM         dbo.HEAD_LST INNER JOIN   dbo.VISITOR_DTL ON dbo.HEAD_LST.NUMBER = dbo.VISITOR_DTL.NUMBER AND dbo.HEAD_LST.TAG = dbo.VISITOR_DTL.TAG WHERE     (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.NUMBER <> " + NUMBER.Text + ")GROUP BY dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT,  dbo.HEAD_LST.NUMBER, dbo.VISITOR_DTL.PORID ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                //        if (rst.Count > 0 & !IsNull(rst.FirstOrDefault().PORID))
                //        {
                //            CurrentData.CUST_NO = rst.FirstOrDefault().CUST_NO;
                //            CurrentData.STAT = true;
                //            CurrentData.TOZIH = rst.FirstOrDefault().TOZIH;
                //            CurrentData.PORID = rst.FirstOrDefault().PORID;

                //            //PORID_AfterUpdate
                //            long prs;
                //            var MBK = default(long);
                //            prs = 0L;
                //            if (!IsNull(CurrentData.PORID))
                //            {
                //                var ROWS = dbms.DoGetDataSQL<QRE_VISIT1>("SELECT CODE ,MABL_K - N_MOIN AS MABLK FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER.Text).ToList();
                //                for (int I = 0; I < ROWS.Count; I++)//while (!ROWS.EOF)
                //                {
                //                    var RST2 = dbms.DoGetDataSQL<double?>("SELECT     PORSANT FROM dbo.VISITORS_PORSANT_KALA WHERE     (PORID = " + CurrentData.PORID + ") and (code = '" + ROWS[I].CODE + "')").ToList();
                //                    if (RST2.Count == 1)
                //                    {
                //                        prs = (long)(prs + Math.Round((double)(ROWS[I].MABLK * RST2.FirstOrDefault() / 100)));
                //                        MBK = (long)(MBK + ROWS[I].MABLK);
                //                    }
                //                    else
                //                    {
                //                        new Msgwin(false, "تذكر مهم :اين كالا فاقد الگو براي اين ويزيتور است و پورسانت محاسبه نشد.درصورت لزوم براي آن تعريف كنيد و همينجا مجددا الگو را انتخاب كنيد  : " + CL_HESABDARI.GETKALANAME(Convert.ToDouble(ROWS[I].CODE))).ShowDialog();
                //                    }
                //                }
                //                CurrentData.PURSANT = Math.Round((double)(prs));
                //                if (MBK > 0L & prs > 0L)
                //                {
                //                    CurrentData.DARSAD = CurrentData.PURSANT / MBK * 100;
                //                }
                //                else
                //                {
                //                    CurrentData.DARSAD = 0;
                //                }
                //            }
                //        }
                //    }

                //    if (grid.CurrentCell.Column.SortMemberPath == "PURSANT")
                //    {
                //        var rst = dbms.DoGetDataSQL<VISITOR_DARSAD>("SELECT     TOP 100 PERCENT dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT FROM         dbo.HEAD_LST INNER JOIN   dbo.VISITOR_DTL ON dbo.HEAD_LST.NUMBER = dbo.VISITOR_DTL.NUMBER AND dbo.HEAD_LST.TAG = dbo.VISITOR_DTL.TAG WHERE     (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "') AND (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.NUMBER <> " + NUMBER.Text + ")GROUP BY dbo.VISITOR_DTL.CUST_NO, dbo.VISITOR_DTL.DARSAD, dbo.VISITOR_DTL.PURSANT, dbo.VISITOR_DTL.TOZIH, dbo.VISITOR_DTL.STAT,  dbo.HEAD_LST.NUMBER ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                //        if (rst.Count > 0)
                //        {
                //            CurrentData.CUST_NO = rst.FirstOrDefault().CUST_NO;
                //            if (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text) != 0)
                //            {
                //                CurrentData.DARSAD = rst.FirstOrDefault().PURSANT / (Convert.ToDouble(JF.Text) - Convert.ToDouble(TAKHFIF.Text)) * 100;
                //            }
                //            else
                //            {
                //                CurrentData.DARSAD = 0;
                //            }
                //            CurrentData.PURSANT = rst.FirstOrDefault().PURSANT;
                //            CurrentData.STAT = true;
                //            CurrentData.TOZIH = rst.FirstOrDefault().TOZIH;
                //        }
                //    }
                //}

                double sum = SAYER_VISITOR_DATA.Sum(item => item.PURSANT ?? 0.0);
                Text190.Text = sum.ToString();
            }
        }
        private void sTATColumn_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            VISITOR_DTL_SUB.BeginEdit();
        }


        private void DELETE_SAYER_Click(object sender, RoutedEventArgs e)
        {
            var IsVisible = DELETE_SAYER.Visibility == Visibility.Visible;
            if (!DELETE_SAYER.IsEnabled || !IsVisible) { return; }

            if ((bool)SGN1.IsChecked || (bool)SGN2.IsChecked || (bool)SGN3.IsChecked)
            {
                universControl.PopNotifyShow("ابتدا امضا را بردارید", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            //if (VISITOR_DTL_SUB.IsEditing()) return;

            if (VISITOR_DTL_SUB.Items.Count > 0 && VISITOR_DTL_SUB.SelectedItem != null)
            {
                if (!(VISITOR_DTL_SUB.SelectedItems is null))
                {
                    var editableCollectionView = VISITOR_DTL_SUB.Items as IEditableCollectionView;
                    if (editableCollectionView != null && editableCollectionView.IsEditingItem && editableCollectionView.CanCancelEdit)
                    {
                        try { editableCollectionView.CancelEdit(); } catch { }
                    }

                    bool errors = default;

                    errors = (from object i in VISITOR_DTL_SUB.ItemsSource
                              let c = VISITOR_DTL_SUB.ItemContainerGenerator.ContainerFromItem(i)
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
                        ESLAH_Click(null, null);

                        _ = AuditLogger.LogActionAsync(
                                actionType: "DELETE",
                                tableName: "فاکتور فروش  => پورسانت ویزیتور ها",
                                recordId: NUMBER1.Text,
                                oldValue: "TAG = 13",
                                newValue: null,
                                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                        bool IsDeleteSomthing = false;
                        List<MsgModel> ErrosMessages = new List<MsgModel>();
                        for (int i = 0; i < VISITOR_DTL_SUB.SelectedItems.Count; i++)
                        {
                            var item = VISITOR_DTL_SUB.SelectedItems[i];
                            if (!(item.ToStringNullSafe() is "{NewItemPlaceholder}"))
                            {
                                //if (item.GetType().GetProperty("id").GetValue(item) is null)
                                //{
                                //    SAYER_VISITOR_DATA.Remove(item as VISITOR_DTL);

                                //    var before = VISITOR_DTL_SUB.CanUserAddRows;
                                //    VISITOR_DTL_SUB.CanUserAddRows = false;
                                //    VISITOR_DTL_SUB.CanUserAddRows = before;
                                //}
                                //else
                                //{
                                //}
                                var _CUST_NO = item.GetType().GetProperty("CUST_NO").GetValue(item);
                                var _DARSAD = item.GetType().GetProperty("DARSAD").GetValue(item);
                                var _PURSANT = item.GetType().GetProperty("PURSANT").GetValue(item);
                                var _TOZIH = item.GetType().GetProperty("TOZIH").GetValue(item);
                                var _STAT = item.GetType().GetProperty("STAT").GetValue(item);
                                var _PORID = item.GetType().GetProperty("PORID").GetValue(item);

                                _DARSAD = _DARSAD is null ? "NULL" : _DARSAD;
                                _PURSANT = _PURSANT is null ? "NULL" : _PURSANT;
                                _TOZIH = _TOZIH is null ? "NULL" : _TOZIH;
                                _STAT = _STAT is null ? "NULL" : _STAT;
                                _PORID = _PORID is null ? "NULL" : _PORID;

                                dbms.DoExecuteSQL($"DELETE FROM dbo.VISITOR_DTL WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG} AND " +
                                $"CUST_NO = N'{_CUST_NO}' AND DARSAD = {_DARSAD} AND PURSANT = {_PURSANT}");

                                IsDeleteSomthing = true;
                            }
                        }
                        if (IsDeleteSomthing is true)
                        {
                            VISITOR_DTL_SUB_ReGetData();

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

        private bool IsValidPercentage(string input)
        {
            bool isright = false;

            if (!string.IsNullOrEmpty(input) && CL_LMethods.IsNumeric(input))
            {
                double darsad = Convert.ToDouble(input);

                if (darsad <= 100 && darsad >= 0)
                {
                    isright = true;
                }
            }

            return isright;
        }

        private void VISITOR_DTL_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string CURRENT_COLUMN_NAME = "";
            if (VISITOR_DTL_SUB.CurrentCell.Column is not null)
            {
                CURRENT_COLUMN_NAME = VISITOR_DTL_SUB.CurrentCell.Column.SortMemberPath;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                DELETE_SAYER_Click(null, null);
            }
            if (e.Key == Key.Add)
            {
                if (CURRENT_COLUMN_NAME is "PURSANT")
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
                if (CURRENT_COLUMN_NAME is "PURSANT")
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


        private void Command100_Click(object sender, RoutedEventArgs e) //چاپ فاکتور
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;

            double min;
            bool NOTPR = false;

            if ((bool)Baseknow.RMOG) // Replace with actual control/query to check RMOG value
            {
                var invoLstResult = dbms.DoGetDataSQL<dynamic>(
                    "SELECT * FROM invo_lst WHERE NUMBER = @Number AND tag = 2 AND anbar <> 0",
                    new { Number = NUMBER.Text }).ToList(); // Replace 'this.NUMBER' with actual value

                foreach (var row in invoLstResult)
                {
                    var mandResult = dbms.DoGetDataSQL<dynamic>($@"SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  
                    FROM dbo.AK_MOGO_AVL_KOL(99999999, {row.ANBAR}) AK_MOGO_AVL_KOL 
                    RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE 
                    AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR 
                    LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, {row.ANBAR}) AK_MOGO_FR 
                    ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE 
                    AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR 
                    WHERE (dbo.STUF_FSK.CODE = N'{row.CODE}') 
                    AND (dbo.STUF_FSK.ANBAR = {row.ANBAR})"
                    ).FirstOrDefault();

                    if (mandResult != null)
                    {
                        if (mandResult.mand < 0)
                        {
                            new Msgwin(false, $"خروج كالاي  {CL_HESABDARI.GETKALANAME(row.CODE)} از انبار موجودي را به مقدار غير مجاز كاهش ميدهد. برگه قابل چاپ نيست").ShowDialog();
                            NOTPR = true;
                        }
                    }
                }
            }
            if (NOTPR == false)
            {
                //DoCmd.OpenReport("INVOICE_FROOSH_22", acPreview, "", "NUMBER1 =" + Me.NUMBER1 + " AND HTAG =" + Me.Dtag, , 2);

                if (IsExporty)
                {
                    OpenInterNationalInvoice();
                }
                else
                {
                    ReportProccess();
                }

                if (!(bool)OKF.IsChecked)
                {
                    OKDATE = CL_HESABDARI.FARSIDATE();
                    OKTIME = CL_HESABDARI.GTFS(); //DateTime.Now.ToString("HH:mm:ss");
                }
                if ((bool)Baseknow.LOCKFAP)
                {
                    OKF.IsChecked = true;
                }
            }
            if ((bool)OKF.IsChecked)
            {
                AllowDeletions = false;
                AllowEdits = false;

                SecurityAllCheck();

                INVO_LST_sub.IsReadOnly = true;
                //TAKHFIF_APLAY_SUB.IsEnabled = false;
                //New Code // Page58.IsEnabled = false;
                //New Code // Page155.IsEnabled = false;
                //Me["moadian"].IsEnabled = false;
                ESLAH.IsEnabled = true;
            }
        }

        private void ReportProccess()
        {
            #region Reportprocess

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_22.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER1.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FACTOR_DATA"]).CommandTimeout = 900;

            #region GroupFooter3_Format
            //SELECT TOP 1 TFSAZMAN FROM dbo.SAZMAN
            if (Baseknow.TFSAZMAN != "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = true;
                (report.GetComponentByName("MANDG") as StiText).Enabled = true;

                //EXEC dbo.GETKOL => SELECT CUST_NO FROM HEAD_LST WHERE TAG = 13 AND NUMBER = 5338 --Current Invoice NUMBER
                //var rst_0 = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(this.CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(this.CUST_NO.SelectedValue.ToString()) + ")").FirstOrDefault();

                //if (rst_0 == null)
                //{
                //    (report.GetComponentByName("MANDAH") as StiText).Text = "0";
                //}
                //else
                //{
                //    var _mandah = Interaction.IIf(rst_0 > 0, Strings.Format(rst_0, "##,# ريال بدهكار"), Strings.Format(rst_0 * -1, "##,# ريال بستانكار"));
                //    (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                //}

                (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
            }
            else
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
                (report.GetComponentByName("MANDG") as StiText).Enabled = false;
            }
            #endregion

            #region PageHeader_Format
            string FRF;
            long MABFR;
            string STRFR;
            int i;
            string CH;
            double JAMP;
            string TAMIR;
            string per;
            long permab;
            //Current Invoice NUMBER
            var rst = dbms.DoGetDataSQL<RPT_MODEL1>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG AS HTAG, HEAD_LST.MOLAH FROM HEAD_LST WHERE (((HEAD_LST.NUMBER)= " + NUMBER.Text + " ) AND  ((HEAD_LST.TAG)=13))").FirstOrDefault();
            if (rst != null)
            {
                if (Strings.Left(rst.MOLAH, 1) == "~" | Strings.Left(rst.MOLAH, 1) == ".")
                {
                    i = 2;
                    TAMIR = "";
                    CH = Strings.Mid(rst.MOLAH, 2, 1);
                    while (Information.IsNumeric(CH) || CH == "-" & i <= rst.MOLAH.Length)
                    {
                        i = i + 1;
                        TAMIR = TAMIR + CH;
                        CH = Strings.Mid(rst.MOLAH, i, 1);
                    }
                    if (Information.IsNumeric(TAMIR))
                    {
                        TAMIR = Baseknow.BEDEHKAR + "-1-" + TAMIR;
                    }
                    ;
                    if (Strings.Mid(Baseknow.OPTIONSS, 26, 1) == "5")
                    {
                        //EXEC dbo.GETKOL ,EXEC dbo.GETMOIN ,EXEC dbo.GETTAF
                        var rst1 = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + CL_HESABDARI.GETKOL(TAMIR) + " and NUMBER = " + CL_HESABDARI.GETMOIN(TAMIR) + " and tNUMBER = " + CL_HESABDARI.GETTAF(TAMIR)).FirstOrDefault();
                        if (rst1 != null)
                        {
                            (report.GetComponentByName("lvisit") as StiText).Text = "ويزيتور: " + rst1.NAME;
                            (report.GetComponentByName("Ltvis") as StiText).Text = "تلفن ويزيتور: " + rst1.TEL;
                        }
                    }
                }
                else if (Strings.Mid(Baseknow.OPTIONSS, 26, 1) == "5")
                {
                    var rst2 = dbms.DoGetDataSQL<VISITOR_DTL>("SELECT * FROM VISITOR_DTL WHERE TAG = 2 AND NUMBER = " + NUMBER.Text).FirstOrDefault();
                    if (rst2 != null)
                    {
                        var RST2 = dbms.DoGetDataSQL<TDETA_HES>("SELECT * FROM TDETA_HES WHERE N_KOL = " + CL_HESABDARI.GETKOL(rst2.CUST_NO) + " AND NUMBER = " + CL_HESABDARI.GETMOIN(rst2.CUST_NO) + " AND tNUMBER = " + CL_HESABDARI.GETTAF(rst2.CUST_NO)).FirstOrDefault();
                        if (RST2 != null)
                        {
                            (report.GetComponentByName("lvisit") as StiText).Text = "ويزيتور: " + RST2.NAME;
                            (report.GetComponentByName("Ltvis") as StiText).Text = "تلفن ويزيتور: " + RST2.TEL;
                        }
                    }
                }
            }
            if (this.MAS.Text == "0")
            {
                (report.GetComponentByName("MAS") as StiText).Enabled = false;
            }
            #endregion

            //Report_Open
            FRF = null;
            MABFR = 0;
            STRFR = null;
            double JCHK = default, jamf, HAZ, NAGHD, VAR, HAV, taf, MBA;
            double GB;

            /// <summary>
            /// تگ  حواله 2 | HEAD_LST | INVO_LST | PAY_GETD | VISITOR_DTL
            /// </summary>
            //public byte hTAG { get; set; } = 2;

            var rst_3 = dbms.DoGetDataSQL<RPT_MODEL2>("SELECT dbo.PAY_GETD.N_SERI, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.DATE, dbo.PAY_GETD.DATE_S , dbo.PAY_GETD.MABL, dbo.PAY_GETD.NUMBER, dbo.PAY_GETD.TAG FROM dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK WHERE (dbo.PAY_GETD.NUMBER = " + NUMBER.Text + ") AND (dbo.PAY_GETD.N_KOL IS NULL OR N_KOL <> 911) AND (dbo.PAY_GETD.TAG = " + hTAG + ")").ToList();
            if (rst_3.Count > 0)
            {
                JCHK = 0d;

                //NCHK.Text => SUM of MABL of SELECT * FROM PAY_GETD WHERE NUMBER --Currnet NUMBER e.g. 5357
                (report.GetComponentByName("COMM") as StiText).Text = "چكهاي دريافت شده " + rst_3.Count + " فقره جمعاًبه مبلغ :" + Strings.Format(Convert.ToInt64(NCHK.Text), "### ريال") + "  ";

                for (int o = 0; o < rst_3.Count; o++)
                {
                    (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + "ـ سريال:" + rst_3[o].N_SERI + " بانك:" + rst_3[o].NAMES + " شعبه:" + Strings.Trim(rst_3[o].SHOBEH);
                    JCHK = (double)(JCHK + rst_3[o].MABL);
                }
            }
            else
            {
                (report.GetComponentByName("COMM") as StiText).Enabled = false;
                (report.GetComponentByName("SHARAYET") as StiText).Enabled = true;
            }
            jamf = 0d;
            HAZ = 0d;
            NAGHD = 0d;
            VAR = 0d;
            HAV = 0d;
            taf = 0d;
            MBA = 0d;
            double? JST0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER.Text + " ) AND ((INVO_LST.TAG)=2))").FirstOrDefault();
            if (JST0 > 0 && !IsNull(JST0))
            {
                jamf = (double)JST0;
            }
            var JST = dbms.DoGetDataSQL<RPT_MODEL3>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG AS htag, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.MBAA FROM HEAD_LST WHERE (((HEAD_LST.NUMBER)= " + NUMBER.Text + " ) AND  ((HEAD_LST.TAG)=13))").FirstOrDefault();
            if (JST != null && !IsNull(JST?.NUMBER))
            {
                HAZ = (double)JST.MABL_HAZ;
                VAR = (double)JST.MABL_VAR;
                HAV = (double)JST.MABL_HAV;
                NAGHD = (double)JST.M_NAGHD;
                taf = (double)JST.TAKHFIF;
                MBA = (double)JST.MBAA;
            }

            (report.GetComponentByName("JF") as StiText).Text = Strings.Format(jamf, "#,##0;#,##0-");
            (report.GetComponentByName("HKH") as StiText).Text = Strings.Format(HAZ, "#,##0;#,##0-");
            string test = Strings.Format(MBA, "#,##0;#,##0-");
            (report.GetComponentByName("MBAA") as StiText).Text = Strings.Format(MBA, "#,##0;#,##0-");
            if (JST?.VAS == 1 || IsNull(JST?.VAS))
            {
                (report.GetComponentByName("GABEL") as StiText).Text = Strings.Format(jamf + HAZ + MBA - taf, "#,##0;-#,##0");
                GB = jamf + HAZ + MBA - taf;
            }
            else
            {
                (report.GetComponentByName("GABEL") as StiText).Text = Strings.Format(jamf - HAZ + MBA - taf, "#,##0;-#,##0");
                GB = jamf - HAZ + MBA - taf;
            }
            if (taf == 0d)
            {
                (report.GetComponentByName("Label180") as StiText).Enabled = false;
                (report.GetComponentByName("TF") as StiText).Enabled = false;
            }
            else
            {
                (report.GetComponentByName("TF") as StiText).Text = Strings.Format(taf, "#,##0;-#,##0");
                if (Conversion.Val(Strings.Format(taf / jamf * 100d, "##,##0.0")) != Conversion.Val(Strings.Format(taf / jamf * 100d, "#,###")))
                {
                    (report.GetComponentByName("Label180") as StiText).Text = Strings.Format(taf / jamf * 100d, "##,##0.0") + " % تخفيف:";
                }
                else
                {
                    (report.GetComponentByName("Label180") as StiText).Text = Strings.Format(taf / jamf * 100d, "#,###") + " % تخفيف:";
                }
            }
            (report.GetComponentByName("JPAY") as StiText).Text = Strings.Format(NAGHD + VAR + HAV + JCHK, "#,##0;-#,##0");
            if (JST?.VAS == 1 || IsNull(JST?.VAS))
            {
                (report.GetComponentByName("MAN") as StiText).Text = Strings.Format(jamf + MBA + HAZ - (NAGHD + VAR + HAV + JCHK + taf), "#,##0;-#,##0");

                report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(jamf + MBA + HAZ - taf));
            }
            else
            {
                (report.GetComponentByName("MAN") as StiText).Text = Strings.Format(jamf + MBA - HAZ - (NAGHD + VAR + HAV + JCHK + taf), "#,##0;-#,##0");
                report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(jamf + MBA - HAZ - taf));
            }
            double MANN, mm;
            var rst_00 = dbms.DoGetDataSQL<double?>($"SELECT SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE HES = N'{CUST_NO.SelectedValue.ToString()}' ").ToList();
            if (rst_00.Count == 0)
            {
                (report.GetComponentByName("MANDG") as StiText).Text = "0";
            }
            else
            {
                mm = (double)rst_00.FirstOrDefault();
                if (JST?.VAS == 1 || IsNull(JST?.VAS))
                {
                    (report.GetComponentByName("MANDG") as StiText).Text = Interaction.IIf(mm - (jamf + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)) > 0d, Strings.Format(mm - (jamf + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)), "##,# ريال بدهكار"), Strings.Format((mm - (jamf + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf))) * -1, "##,# ريال بستانكار")).ToString();
                }
                else
                {
                    (report.GetComponentByName("MANDG") as StiText).Text = Interaction.IIf(mm - (jamf - HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)) > 0d, Strings.Format(mm - (jamf - HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)), "##,# ريال بدهكار"), Strings.Format((mm - (jamf - HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf))) * -1, "##,# ريال بستانكار")).ToString();
                }
            }
            if (Baseknow.TFCODE_E != "" & !IsNull(Baseknow.TFCODE_E))
            {
                (report.GetComponentByName("Label179") as StiText).Text = Baseknow.TFCODE_E;
            }
            (report.GetComponentByName("Label224") as StiText).Text = "%ماليات و عوارض:";
            if (Baseknow.TFSAZMAN == "2")
            {
                (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
                (report.GetComponentByName("MANDG") as StiText).Enabled = false;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 2, 1) == "5")
            {
                (report.GetComponentByName("Label197") as StiText).Enabled = false;
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 17, 1) == "5")
            {
                var rst_01 = dbms.DoGetDataSQL<RPT_MODEL4>("SELECT TOP 3 dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, SUM(dbo.INVO_LST.MABL_K) AS Expr1, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.TAKHFIF,dbo.HEAD_LST.MBAA , dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.VAS, dbo.HEAD_LST.DATE_N FROM         dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG GROUP BY dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.TAKHFIF, dbo.HEAD_LST.MBAA, dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.VAS, dbo.HEAD_LST.TAG , dbo.HEAD_LST.DATE_N HAVING      (dbo.HEAD_LST.TAG =13)  AND (dbo.HEAD_LST.NUMBER <> " + NUMBER.Text + ") AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + "')ORDER BY dbo.HEAD_LST.DATE_N DESC").ToList();
                if (rst_01.Count > 0)
                {
                    for (int t = 0; t < rst_01.Count; t++) //while (!rst_01.EOF)
                    {
                        MABFR = Convert.ToInt64(Interaction.IIf(rst_01[t].VAS == 1 || IsNull(rst_01[t].VAS), rst_01[t].Expr1 + rst_01[t].MABL_HAZ + rst_01[t].MBAA - rst_01[t].TAKHFIF, rst_01[t].Expr1 - rst_01[t].MABL_HAZ + rst_01[t].MBAA - rst_01[t].TAKHFIF));
                        STRFR = STRFR + Strings.Format(rst_01[t].DATE_N, "####/##/##") + " شماره فاكتور:   " + rst_01[t].NUMBER + "  مبلغ قابل پرداخت  فاكتور:   " + Strings.Format(MABFR, "#,##0;-#,##0") + '\r';
                    }
                    (report.GetComponentByName("FACTORS") as StiText).Text = "=\"" + STRFR + "\"";
                }
            }

            //SELECT OPTIONSS FROM dbo.SAZMAN
            if (Strings.Mid(Baseknow.OPTIONSS, 42, 1) == "5" && false)
            {
                STRFR = "";
                var rst_02 = dbms.DoGetDataSQL<DARSAD_TAKHFIF>("SELECT  *  FROM  DARSAD_TAKHFIF ORDER BY RDF").ToList();
                if (rst_02.Count > 0)
                {
                    for (int w = 0; w < rst_02.Count; w++)
                    {
                        STRFR = STRFR + rst_02[w].ONVAN + "  " + rst_02[w].DARSAD + "  درصد تخفيف :   " + Strings.Format(Math.Round((double)(GB * rst_02[w].DARSAD / 100)), "#,##0;-#,##0") + "  قابل پرداخت :  " + Strings.Format(GB - Math.Round((double)(GB * rst_02[w].DARSAD / 100)), "#,##0;-#,##0") + '\r';
                    }
                    (report.GetComponentByName("PAYMENTS") as StiText).Text = "=\"" + STRFR + "\"";
                }
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 18, 1) == "5")
            {
                var rst03 = dbms.DoGetDataSQL<double?>("SELECT SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER.Text + ")").ToList();
                if (rst03.Count > 0)
                {
                    if (!IsNull(rst03.FirstOrDefault()))
                    {
                        (report.GetComponentByName("VAZN") as StiText).Text = "وزن كل به كيلو : " + Math.Round((double)rst03.FirstOrDefault());
                    }
                }
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 26, 1) == "5")
            {
            }
            if (Strings.Mid(Baseknow.OPTIONSS, 47, 1) != "5")
            {
                (report.GetComponentByName("TKHN") as StiText).Enabled = false;
                (report.GetComponentByName("Line219") as StiHorizontalLinePrimitive).Enabled = false;
            }
            else
            {
                (report.GetComponentByName("Label180") as StiText).Text = " تخفيف:";
            }

            //SELECT SGN1,SGN2,SGN3 FROM HEAD_LST WHERE TAG = 13 AND NUMBER = --Current NUMBER --اطلاعات فاکتور فروش
            //امضا ها
            //پیش فرض امضا ها مخفی است
            if ((bool)SGN1.IsChecked) //SELECT SGN1,SGN2,SGN3 FROM HEAD_LST WHERE TAG = 13 AND NUMBER = --Current NUMBER --اطلاعات فاکتور فروش
            {
                //SGN1
                (report.GetComponentByName("FIMG") as StiImage).Enabled = true;
                (report.GetComponentByName("FS") as StiText).Text = SGN1_INFO.SEMAT_USER; //SELECT dbo.Getusersemat((SELECT SGN1usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357),'FFR_FROOSHTX')
                (report.GetComponentByName("FU") as StiText).Text = SGN1_INFO.NAME_HESAB_USER; //SELECT dbo.GETHESNAME(dbo.GETUSERHES((SELECT SGN1usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357)))
            }
            if ((bool)SGN2.IsChecked)
            {
                //SGN2
                (report.GetComponentByName("HIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("HS") as StiText).Text = SGN2_INFO.SEMAT_USER; //SELECT dbo.Getusersemat((SELECT SGN2usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357),'FFR_HESABTX')
                (report.GetComponentByName("HU") as StiText).Text = SGN2_INFO.NAME_HESAB_USER; //SELECT dbo.GETHESNAME(dbo.GETUSERHES((SELECT SGN2usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357)))
            }
            if ((bool)SGN3.IsChecked)
            {
                //SGN3
                (report.GetComponentByName("MIMG") as StiImage).Enabled = true;

                (report.GetComponentByName("MS") as StiText).Text = SGN3_INFO.SEMAT_USER; //SELECT dbo.Getusersemat((SELECT SGN3usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357),'FFR_MODIRTX')
                (report.GetComponentByName("MU") as StiText).Text = SGN3_INFO.NAME_HESAB_USER; //SELECT dbo.GETHESNAME(dbo.GETUSERHES((SELECT SGN3usid FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5357)))
            }

            //SELECT WIDTH_D,NAME,TFADDRESS,TFTEL FROM SAZMAN
            (report.GetComponentByName("Text90") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            (report.GetComponentByName("Text39") as StiText).Text = Baseknow.NAME; // نام فروشنده
            (report.GetComponentByName("Text4") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
            (report.GetComponentByName("Text48") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده

            //SELECT USER_NAME FROM dbo.HEAD_LST WHERE TAG = 13 AND NUMBER = 5338 
            (report.GetComponentByName("USERNAME") as StiText).Text = Baseknow.UUSER;

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
            #endregion
        }

        private void Command120_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;

            double min;
            bool NOTPR = false;

            if ((bool)Baseknow.RMOG) // Replace with actual control/query to check RMOG value
            {
                var invoLstResult = dbms.DoGetDataSQL<dynamic>(
                    "SELECT * FROM invo_lst WHERE NUMBER = @Number AND tag = 2 AND anbar <> 0",
                    new { Number = NUMBER.Text }).ToList(); // Replace 'this.NUMBER' with actual value

                foreach (var row in invoLstResult)
                {
                    var mandResult = dbms.DoGetDataSQL<dynamic>($@"SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand  
                    FROM dbo.AK_MOGO_AVL_KOL(99999999, {row.ANBAR}) AK_MOGO_AVL_KOL 
                    RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE 
                    AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR 
                    LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, {row.ANBAR}) AK_MOGO_FR 
                    ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE 
                    AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR 
                    WHERE (dbo.STUF_FSK.CODE = N'{row.CODE}') 
                    AND (dbo.STUF_FSK.ANBAR = {row.ANBAR})"
                    ).FirstOrDefault();

                    if (mandResult != null)
                    {
                        if (mandResult.mand < 0)
                        {
                            //Microsoft.CSharp.RuntimeBinder.RuntimeBinderException: 'The best overloaded method match for 'Prg_Proccessy.FUNCTIONS.CL_HESABDARI.GETKALANAME(double)' has some invalid arguments'
                            new Msgwin(false, $"خروج كالاي  {CL_HESABDARI.GETKALANAME(Convert.ToDouble(row.CODE))} از انبار موجودي را به مقدار غير مجاز كاهش ميدهد. برگه قابل چاپ نيست").ShowDialog();
                            NOTPR = true;
                        }
                    }
                }
            }
            if (NOTPR == false)
            {
                if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
                {
                    BUTTON_SAVE_HAVALE_Click(null, null);
                }
                if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
                {
                    return;
                }
                //DoCmd.OpenReport("INVOICE_FROOSH_22", acPreview, "", "NUMBER1 =" + Me.NUMBER1 + " AND HTAG =" + Me.Dtag, , 2);

                #region OpenReport

                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_2_1.mrt");
                report.Load(pathreport);

                string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                report.Dictionary.Databases.Clear();
                report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                report["NUMBER_PARAM"] = NUMBER1.Text;
                ((StiSqlSource)report.Dictionary.DataSources["SmallFactor"]).CommandTimeout = 900;

                #region GroupFooter3_Format
                if (Baseknow.MAND)
                {
                    (report.GetComponentByName("MANDAH") as StiText).Enabled = true;
                }
                else
                {
                    (report.GetComponentByName("MANDAH") as StiText).Enabled = false;
                }
                if (Baseknow.MAND)
                {
                    if (!(CL_HESABDARI.BLOCKEDMK(CUST_NO.SelectedValue.ToString())))
                    {
                        //var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE     (HES_K = " + CL_HESABDARI.GETKOL(CUST_NO.SelectedValue.ToString()) + ") AND (HES_M = " + CL_HESABDARI.GETMOIN(CUST_NO.SelectedValue.ToString()) + ") AND (HES_T = " + CL_HESABDARI.GETTAF(CUST_NO.SelectedValue.ToString()) + ")").ToList();
                        //if (rst.Count == 0)
                        //{
                        //    (report.GetComponentByName("MANDAH") as StiText).Text = "  كل مانده حساب : 0  ";
                        //}
                        //else
                        //{
                        //    (report.GetComponentByName("MANDAH") as StiText).Text = "  كل مانده حساب : " + CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                        //}

                        (report.GetComponentByName("MANDAH") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());
                    }
                    else
                    {
                        (report.GetComponentByName("MANDAH") as StiText).Text = "مسدود است";
                    }
                }
                #endregion

                #region Report_Open
                double JCHK = 0;
                double jamf = 0;
                double HAZ = 0;
                double NAGHD = 0;
                double VAR = 0;
                double HAV = 0;
                double taf = 0;
                double MBA = 0;
                long MASTI = 0;
                long SHIRI = 0;
                var rst_0 = dbms.DoGetDataSQL<RPT_MODEL2>("SELECT     dbo.PAY_GETD.N_SERI, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.DATE, dbo.PAY_GETD.DATE_S , dbo.PAY_GETD.MABL, dbo.PAY_GETD.NUMBER, dbo.PAY_GETD.TAG FROM         dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK WHERE (dbo.PAY_GETD.NUMBER = " + NUMBER.Text + ") AND (dbo.PAY_GETD.N_KOL IS NULL OR N_KOL <> 911) AND (dbo.PAY_GETD.TAG = 2)").ToList();
                if (rst_0.Count > 0)
                {
                    JCHK = 0;
                    (report.GetComponentByName("COMM") as StiText).Text = "چكهاي دريافت شده " + rst_0.Count + " فقره جمعاًبه مبلغ :" + Strings.Format(Convert.ToInt64(NCHK.Text), "### ريال") + "  ";
                    for (int w = 0; w < rst_0.Count; w++) //while (!(rst_0.EOF()))
                    {
                        (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + "ـ سريال:" + rst_0[w].N_SERI + " بانك:" + rst_0[w].NAMES + " شعبه:" + rst_0[w].SHOBEH.Trim(' ');
                        JCHK = (double)(JCHK + rst_0[w].MABL);
                    }
                }
                MASTI = 0;
                SHIRI = 0;

                var requiredOptions = new List<string>
                {
                    "ShiryBasketCode",
                    "MastyBasketCode",
                    "MadreseBasketCode"
                };
                List<GENERAL_OPTIONS> options = Task.Run(async () => await GOM.GetOptionsAsync(requiredOptions)).Result;
                //if (CL_HESABDARI.GETKALANAME(378).FixPersianChars().Trim().Contains("سبد شیری"))
                if (options.Any())
                {
                    //var rst_01 = dbms.DoGetDataSQL<RPT_MODEL5>("SELECT  TOP 100 PERCENT dbo.INVO_LST.TAG, dbo.INVO_LST.CODE, SUM(dbo.INVO_LST.MEGHk - dbo.INVO_LST.MEGH_MAR) AS mrgh FROM dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG  WHERE     (dbo.INVO_LST.CODE = N'378' ) AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + " ') GROUP BY dbo.INVO_LST.CODE, dbo.INVO_LST.TAG ORDER BY dbo.INVO_LST.TAG ").ToList();
                    //if (rst_01.Count > 0)
                    //{
                    //    if (rst_01.FirstOrDefault().TAG == 1)
                    //    {
                    //        MASTI = (long)rst_01.Last().mrgh;
                    //        if (rst_01.Count > 1)
                    //        {
                    //            MASTI = (long)(rst_01[rst_01.Count - 2].mrgh - MASTI);
                    //        }
                    //        (report.GetComponentByName("COMM") as StiText).Text += " مانده سبد شيري : " + MASTI;
                    //        //if (!rst_01.EOF)
                    //        //{
                    //        //    rst_01.MoveNext();
                    //        //    MASTI = rst_01.Fields["mrgh"] - MASTI;
                    //        //    (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده سبد شيري : " + MASTI;
                    //        //}
                    //        //else
                    //        //{
                    //        //    (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده سبد شيري : " + MASTI;
                    //        //}
                    //    }
                    //    var rst_02 = dbms.DoGetDataSQL<RPT_MODEL5>("SELECT  TOP 100 PERCENT dbo.INVO_LST.TAG, dbo.INVO_LST.CODE, SUM(dbo.INVO_LST.MEGHk - dbo.INVO_LST.MEGH_MAR) AS mrgh FROM dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG  WHERE     (dbo.INVO_LST.CODE = N'375' ) AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + " ') GROUP BY dbo.INVO_LST.CODE, dbo.INVO_LST.TAG ORDER BY dbo.INVO_LST.TAG ").ToList();
                    //    if (rst_02.Count > 0)
                    //    {
                    //        if (rst_02[0].TAG == 1)
                    //        {
                    //            MASTI = (long)rst_02.Last().mrgh;
                    //            if (rst_02.Count > 1)
                    //            {
                    //                MASTI = (long)(rst_02[rst_02.Count - 2].mrgh - MASTI);
                    //                (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده  سبد ماستي : " + MASTI;
                    //            }
                    //            else
                    //            {
                    //                (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده  سبد ماستي : " + MASTI;
                    //            }
                    //        }
                    //    }
                    //    var rst_03 = dbms.DoGetDataSQL<RPT_MODEL5>("SELECT  TOP 100 PERCENT dbo.INVO_LST.TAG, dbo.INVO_LST.CODE, SUM(dbo.INVO_LST.MEGHk - dbo.INVO_LST.MEGH_MAR) AS mrgh FROM dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG  WHERE     (dbo.INVO_LST.CODE = N'377' ) AND (dbo.HEAD_LST.CUST_NO = N'" + CUST_NO.SelectedValue + " ') GROUP BY dbo.INVO_LST.CODE, dbo.INVO_LST.TAG ORDER BY dbo.INVO_LST.TAG ").ToList();
                    //    if (rst_03.Count > 0)
                    //    {
                    //        if (rst_03[0].TAG == 1)
                    //        {
                    //            MASTI = (long)rst_03.Last().mrgh;
                    //            if (rst_03.Count > 1)
                    //            {
                    //                MASTI = (long)(rst_03[rst_03.Count - 2].mrgh - MASTI);
                    //                (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده  سبد مدرسه : " + MASTI;
                    //            }
                    //            else
                    //            {
                    //                (report.GetComponentByName("COMM") as StiText).Text = (report.GetComponentByName("COMM") as StiText).Text + " مانده  سبد مدرسه : " + MASTI;
                    //            }
                    //        }
                    //    }
                    //}

                    var commCaptionBuilder = new StringBuilder();

                    // Helper function to avoid repeating the query logic
                    Action<string, string> calculateBasket = (code, caption) =>
                    {
                        string sqlBasket = @"
                            SELECT l.TAG, SUM(l.MEGHk - l.MEGH_MAR) AS mrgh 
                            FROM dbo.INVO_LST l INNER JOIN dbo.HEAD_LST h ON l.NUMBER = h.NUMBER AND l.TAG = h.TAG  
                            WHERE (l.CODE = @Code) AND (h.CUST_NO = @CustNo) 
                            GROUP BY l.CODE, l.TAG ORDER BY l.TAG";

                        var results = dbms.DoGetDataSQL<MG_MODEL3>(sqlBasket, new { Code = code, CustNo = CUST_NO.SelectedValue }).ToList();
                        if (results.Any() && results.First().TAG == 1)
                        {
                            long masti = Convert.ToInt64(results.First().mrgh);
                            if (results.Count > 1)
                            {
                                masti = Convert.ToInt64(results[1].mrgh) - masti;
                            }
                            commCaptionBuilder.Append($" {caption} : {masti} ");
                        }
                    };

                    var _SHIRI_CODE_ = options.FirstOrDefault(o => o.OptionName == "ShiryBasketCode")?.OptionValue; //"378", "مانده  سبد شیری"
                    var _MASTI_CODE_ = options.FirstOrDefault(o => o.OptionName == "MastyBasketCode")?.OptionValue; //"375", "مانده سبد ماستی"
                    var _MADRESEH_CODE_ = options.FirstOrDefault(o => o.OptionName == "MadreseBasketCode")?.OptionValue; //"377", "مانده سبد مدرسه"

                    calculateBasket(_SHIRI_CODE_, "مانده  سبد شیری");
                    calculateBasket(_MASTI_CODE_, "مانده سبد ماستی");
                    calculateBasket(_MADRESEH_CODE_, "مانده سبد مدرسه");

                    (report.GetComponentByName("COMM") as StiText).Enabled = true;
                    (report.GetComponentByName("COMM") as StiText).Text = commCaptionBuilder.ToString();

                }
                jamf = 0;
                HAZ = 0;
                NAGHD = 0;
                VAR = 0;
                HAV = 0;
                taf = 0;
                MBA = 0;
                var JST_0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER.Text + " ) AND ((INVO_LST.TAG)=2))").ToList();
                if (JST_0.Count > 0 & !(IsNull(JST_0.FirstOrDefault())))
                {
                    jamf = (double)JST_0.FirstOrDefault();
                }
                var JST = dbms.DoGetDataSQL<RPT_MODEL3>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG AS htag, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.MBAA FROM HEAD_LST WHERE (((HEAD_LST.NUMBER)= " + NUMBER.Text + " ) AND  ((HEAD_LST.TAG)=13))").ToList();
                if (JST.Count > 0 & !(IsNull(JST.FirstOrDefault())))
                {
                    HAZ = (double)JST.FirstOrDefault().MABL_HAZ;
                    VAR = (double)JST.FirstOrDefault().MABL_VAR;
                    HAV = (double)JST.FirstOrDefault().MABL_HAV;
                    NAGHD = (double)JST.FirstOrDefault().M_NAGHD;
                    taf = (double)JST.FirstOrDefault().TAKHFIF;
                    MBA = (double)JST.FirstOrDefault().MBAA;
                }

                (report.GetComponentByName("JF") as StiText).Text = jamf.ToString("#,##0;#,##0-");
                (report.GetComponentByName("HKH") as StiText).Text = HAZ.ToString("#,##0;#,##0-");
                (report.GetComponentByName("MBAA") as StiText).Text = Strings.Format(MBA, "#,##0;#,##0-");
                (report.GetComponentByName("GABEL") as StiText).Text = (jamf + HAZ + MBA - taf).ToString("#,##0;#,##0-");
                (report.GetComponentByName("TF") as StiText).Text = taf.ToString("#,##0;#,##0-");
                (report.GetComponentByName("JPAY") as StiText).Text = (NAGHD + VAR + HAV + JCHK).ToString("#,##0;#,##0-");
                (report.GetComponentByName("MAN") as StiText).Text = (jamf + HAZ + MBA - (NAGHD + VAR + HAV + JCHK + taf)).ToString("#,##0;#,##0-");

                report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(GHABEL.Text));
                //this.HR.CAPTION = ALPHANUM(jamf + HAZ + MBA - taf) + " " + "ريال";

                (report.GetComponentByName("Label224") as StiText).Text = "%ماليات و عوارض:";
                if (Baseknow.TFCODE_E != "" && !(IsNull(Baseknow.TFCODE_E)))
                {
                    (report.GetComponentByName("Label179") as StiText).Text = Baseknow.TFCODE_E;
                }
                #endregion

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

                (report.GetComponentByName("Text2") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
                (report.GetComponentByName("Text7") as StiText).Text = Baseknow.NAME; // نام فروشنده
                (report.GetComponentByName("Text14") as StiText).Text = Baseknow.TFADDRESS; // آدرس فروشنده
                (report.GetComponentByName("Text9") as StiText).Text = Baseknow.TFTEL; // تلفن فروشنده
                (report.GetComponentByName("usernamelbl") as StiText).Text = Baseknow.UUSER; //CL_HESABDARI.UCURRENTUSER()


                new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
                #endregion

                if (!(bool)OKF.IsChecked)
                {
                    OKDATE = CL_HESABDARI.FARSIDATE();
                    OKTIME = CL_HESABDARI.GTFS(); //DateTime.Now.ToString("HH:mm:ss");
                }
                if ((bool)Baseknow.LOCKFAP)
                {
                    OKF.IsChecked = true;
                }
            }
            if ((bool)OKF.IsChecked)
            {
                AllowDeletions = false;
                AllowEdits = false;

                SecurityAllCheck();

                INVO_LST_sub.IsReadOnly = true;
                //TAKHFIF_APLAY_SUB.IsEnabled = false;
                //New Code // Page58.IsEnabled = false;
                //New Code // Page155.IsEnabled = false;
                //Me["moadian"].IsEnabled = false;
                ESLAH.IsEnabled = true;
            }

        }

        private void Command139_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;

            double JAMFACT;
            if ((bool)Baseknow.SAGHF || (bool)Baseknow.SAGHF2)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(this.CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.CUST_NO.SelectedValue.ToString())) == false)
                {
                    new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!").ShowDialog();
                    return;
                }
            }


            // Call CheckCustomPage("HAVLAH_ANBAR_KHORUG", frm.RecordsetClone.RecordCount * 0.9 + 20)

            //DoCmd dbms.DoGetDataSQL<> Report("INVOICE_FROOSH_2_MBA", acPreview, "", "NUMBER =" + this.NUMBER + " AND HTAG =2");
            #region OpenReport
            EMZAPARAM.IsRasmi = true;


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_2_MBA.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER1.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FactorMBA"]).CommandTimeout = 900;

            #region GroupFooter3_Format

            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(GHABEL.Text));
            //this.HR.CAPTION = ALPHANUM(this.Text279) + " " + "ريال";

            if ((report.GetComponentByName("DEPART") as StiText).Text == "" || IsNull((report.GetComponentByName("DEPART") as StiText).Text))
            {
                (report.GetComponentByName("DEPART") as StiText).Enabled = false;
                (report.GetComponentByName("DEPNAME") as StiText).Enabled = false;
            }
            #endregion


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


            //report.Compile();
            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();

            EMZAPARAM.IsRasmi = false;
            #endregion

            if ((bool)!this.OKF.IsChecked)
            {
                this.OKDATE = CL_HESABDARI.FARSIDATE();
                this.OKTIME = CL_HESABDARI.GTFS();
            }
            if ((bool)Baseknow.LOCKFAP)
            {
                this.OKF.IsChecked = true;
            }
            if ((bool)OKF.IsChecked)
            {
                AllowDeletions = false;
                AllowEdits = false;

                SecurityAllCheck();

                INVO_LST_sub.IsReadOnly = true;
                //TAKHFIF_APLAY_SUB.IsEnabled = false;
                //New Code // Page58.IsEnabled = false;
                //New Code // Page155.IsEnabled = false;
                //Me["moadian"].IsEnabled = false;
                ESLAH.IsEnabled = true;
            }
            if ((bool)Baseknow.PRMFR)
            {
                Msgwin msgwin = new Msgwin(true, "پيامك ارسال شود؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    var rst = dbms.DoGetDataSQL<MG_MODEL2>("SELECT     SUM(MABL_K) AS SumOfMABL_K,SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE     (NUMBER = " + NUMBER.Text + ") AND (TAG = 2) ").ToList();
                    if (rst.Count > 0 && !IsNull(rst.FirstOrDefault().SumOfMABL_K))
                    {
                        JAMFACT = Convert.ToDouble(Interaction.IIf(VAS == 1 || IsNull(VAS), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text)));
                    }
                    else
                    {
                        JAMFACT = 0d;
                    }
                    //ersal_sms(this.CUST_NO.SelectedValue, "فاكتور شماره :" + this.NUMBER1.Text + '\r' + "مورخ:" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + rst.FirstOrDefault().MEGHk + '\r' + "مانده حساب :" + CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString()) + '\r' + Baseknow.SMS_OWNER, this.NUMBER.Text, 2);
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<MG_MODEL2>("SELECT     SUM(MABL_K) AS SumOfMABL_K,SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2) ").ToList();
                if (rst.Count > 0 && !IsNull(rst.FirstOrDefault().SumOfMABL_K))
                {
                    JAMFACT = Convert.ToDouble(Interaction.IIf(VAS == 1 || IsNull(VAS), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text)));
                }
                else
                {
                    JAMFACT = 0d;
                }
                //ersal_sms(this.CUST_NO.SelectedValue, "فاكتور شماره :" + this.NUMBER1 + '\r' + "مورخ:" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + rst.FirstOrDefault().MEGHk + '\r' + "مانده حساب :" + CL_HESABDARI.GETMANDAH(this.CUST_NO.SelectedValue.ToString()) + '\r' + Baseknow.SMS_OWNER, this.NUMBER.Text, 2);
            }
        }
        private void PrintReport()
        {
            // Get the report name and parameter values from the UI elements
            string rptName = "MyReport";
            //Stimulsoft.Report.StiOptions.Engine.DefaultPaperSize = PaperKind.Letter;

            // Create an instance of the report
            var report = new StiReport();

            string[] printerNames = PrinterSettings.InstalledPrinters.Cast<string>().ToArray();

            // Display the printer names
            foreach (string printerName in printerNames)
            {
                Console.WriteLine("Printer Name: " + printerName);
                // You can retrieve further information or properties of each printer using the printerName
            }

            PrinterSettings printerSettings = new PrinterSettings();
            //PageSettings pageSettings = new PageSettings(printerSettings);
            //pageSettings.Margins = new Margins(0, 0, 0, 0);

            // Setting the printer name.
            printerSettings.PrinterName = "1";
            // Setting the number of copies.
            printerSettings.Copies = (short)1;
            report.Pages[0].PaperSize = PaperKind.Letter;
            //report.Pages[0].PageWidth = widthLegal;
            foreach (StiPage page in report.Pages)
            {
                page.Orientation = StiPageOrientation.Portrait;
                page.Margins.Top = 0;
                page.Margins.Bottom = 0;
                page.Margins.Left = 0;
                page.Margins.Right = 0;
                page.PaperSize = System.Drawing.Printing.PaperKind.A4;

            }

            // Optionally, configure the PrintDialog properties.
            // printDialog.PrintQueue = ...;
            // printDialog.PrintTicket = ...;
            report.PrinterSettings.Copies = 1;
            // Printing the report on the default printer.
            report.Print(printerSettings); // Make sure to pass false if you want to use the dialog settings.
        }

        private async void custprint_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;

            double JAMFACT;
            if ((bool)Baseknow.SAGHF || (bool)Baseknow.SAGHF2)
            {
                if (Convert.ToBoolean(CL_HESABDARI.Checketebar(this.CUST_NO.SelectedValue.ToString())) == false || Convert.ToBoolean(CL_HESABDARI.ChecketebarMEG(this.CUST_NO.SelectedValue.ToString())) == false)
                {
                    new Msgwin(false, "اعتبار اين مشتري تمام شده است و نمي تواند خريد نمايد...!").ShowDialog();
                    return;
                }
            }
            if (ChangeIsHappend) //تغیری اتفاق افتاده برو اول ذخیره کن
            {
                BUTTON_SAVE_HAVALE_Click(null, null);
            }
            if (ChangeIsHappend) //ذخیره کامل انجام نشده خطایی داشته پس ادامه نه
            {
                return;
            }

            #region OpenReport

            EMZAPARAM.IsRasmi = true;


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.INVOICE_FROOSH_2_MBA_22.mrt");
            report.Load(pathreport);

            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report["NUMBER_PARAM"] = NUMBER1.Text;
            ((StiSqlSource)report.Dictionary.DataSources["FactorMBA"]).CommandTimeout = 900;

            #region GroupFooter3_Format

            report.Dictionary.Variables.Add("MABL_TO_WORD", Convert.ToInt64(GHABEL.Text));

            //if ((report.GetComponentByName("DEPART") as StiText).Text == "" || IsNull((report.GetComponentByName("DEPART") as StiText).Text))
            //{
            //    (report.GetComponentByName("DEPART") as StiText).Enabled = false;
            //    (report.GetComponentByName("DEPNAME") as StiText).Enabled = false;
            //}
            #endregion


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

            var CustomerName = (CUST_NO.SelectedItem as Custom_CUST_HESAB).NAME;
            var MableToPay = "0";
            if (long.TryParse(GHABEL.Text, out long value))
            {
                MableToPay = value.ToString("N0", new CultureInfo("fa-IR"));
            }

            (report.GetComponentByName("SHSH") as StiText).Text = "بدين وسيله تاييد مينمايم اينجانب." + $"({CustomerName})" + "کالاي درخواستي خود را سالم و بدون کسري دريافت نمودم و اعلام ميدارم که متعهدم در مقابل ارائه اين سند مبلغ." + MableToPay + "ريال پرداخت نمايم " + ".";
            (report.GetComponentByName("MANDCUST") as StiText).Text = CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString());

            //(report.GetComponentByName("lvisit") as StiText).Text = " فروشنده : " + CL_HESABDARI.GETHESNAME(CL_HESABDARI.GETUSERHES(CL_HESABDARI.GETUSERCOD(USER_NAME.Text))) + " - " + CL_HESABDARI.GETHESMOBIL(CL_HESABDARI.GETUSERHES(CL_HESABDARI.GETUSERCOD(USER_NAME.Text)));
            string VIS = null;
            string CAPTION = NUMBER.Text;
            VIS = CL_HESABDARI.GETVisitorname(Convert.ToInt64(NUMBER.Text), hTAG);
            (report.GetComponentByName("lvisit") as StiText).Text = " فروشنده : " + VIS + " - " + CL_HESABDARI.GETHESMOBIL(CL_HESABDARI.GETVisitorHES(Convert.ToInt64(NUMBER.Text), hTAG));

            new WINRPT(report, LABEL_HEADER.Content.ToStringNullSafe()).Show();
            EMZAPARAM.IsRasmi = false; //reset this
            #endregion

            if ((bool)!this.OKF.IsChecked)
            {
                this.OKDATE = CL_HESABDARI.FARSIDATE();
                this.OKTIME = CL_HESABDARI.GTFS();
            }
            if ((bool)Baseknow.LOCKFAP)
            {
                this.OKF.IsChecked = true;
            }
            if ((bool)OKF.IsChecked)
            {
                AllowDeletions = false;
                AllowEdits = false;

                SecurityAllCheck();

                INVO_LST_sub.IsReadOnly = true;
                ESLAH.IsEnabled = true;
            }
            if ((bool)Baseknow.PRMFR) //فقط از طریق سرور 
            {
                Msgwin msgwin = new Msgwin(true, "پيامك ارسال شود؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult is false)
                {
                    return;
                }

                var rst = dbms.DoGetDataSQL<MG_MODEL2>("SELECT     SUM(MABL_K) AS SumOfMABL_K,SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE     (NUMBER = " + NUMBER.Text + ") AND (TAG = 2) ").ToList();
                if (rst.Count > 0 && !IsNull(rst.FirstOrDefault().SumOfMABL_K))
                {
                    JAMFACT = Convert.ToDouble(Interaction.IIf(VAS == 1 || IsNull(VAS), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text)));
                }
                else
                {
                    JAMFACT = 0d;
                }

                try
                {
                    var SMSAC = new CL_SMSAC();
                    var PAYAM = "فاكتور شماره :" + NUMBER1.Text + '\r' + "مورخ:" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + rst?.FirstOrDefault()?.MEGHk + '\r' + "مانده حساب :" + CL_HESABDARI.GETMANDAH(CUST_NO.SelectedValue.ToString()) + '\r' + Baseknow.SMS_OWNER;

                    var RESULT0 = await SMSAC.ErselSmsAsync(CUST_NO.SelectedValue.ToStringNullSafe(), PAYAM, Convert.ToInt64(NUMBER.Text), hTAG /*2*/, false);
                    List<MSGMODEL.SmsResultRecord>? resultRecords = null;

                    if (RESULT0 != null)
                    {
                        resultRecords = SmsResultProcessor.ConvertToRecords(RESULT0);
                    }
                    if (RESULT0 != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                    {
                        new Msgwin(false, $"پيام {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} ارسال شد....!").ShowDialog();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                        {
                            new Msgwin(false, $"{resultRecords?.FirstOrDefault()?.ErrorMessage} {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ").ShowDialog();
                        }
                        else
                        {
                            new Msgwin(false, $"پیام به خاطر خطا {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} ارسال نشد!").ShowDialog();
                        }
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, $"خطا در انجام عملیات ارسال پیام {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} , پیام ارسال نشد!").ShowDialog();
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<MG_MODEL2>("SELECT     SUM(MABL_K) AS SumOfMABL_K,SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE     (NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2) ").ToList();
                if (rst.Count > 0 && !IsNull(rst.FirstOrDefault().SumOfMABL_K))
                {
                    JAMFACT = Convert.ToDouble(Interaction.IIf(VAS == 1 || IsNull(VAS), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text), rst.FirstOrDefault().SumOfMABL_K + Convert.ToDouble(HKH.Text) + Convert.ToDouble(MBAA.Text) - Convert.ToDouble(NTKHFIF.Text)));
                }
                else
                {
                    JAMFACT = 0d;
                }

                try
                {
                    var SMSAC = new CL_SMSAC();
                    var PAYAM = "فاكتور شماره :" + this.NUMBER1.Text + '\r' + "مورخ:" + Strings.Format(Convert.ToInt64(DATE_N.Text.ToRawTarikh()), "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + rst.FirstOrDefault().MEGHk + '\r' + "مانده حساب :" + CL_HESABDARI.GETMANDAH(this.CUST_NO.SelectedValue.ToString()) + '\r' + Baseknow.SMS_OWNER;
                    var RESULT0 = await SMSAC.ErselSmsAsync(CUST_NO.SelectedValue.ToStringNullSafe(), PAYAM, Convert.ToInt64(NUMBER.Text), hTAG /*2*/, false);
                    List<MSGMODEL.SmsResultRecord>? resultRecords = null;

                    if (RESULT0 != null)
                    {
                        resultRecords = SmsResultProcessor.ConvertToRecords(RESULT0);
                    }
                    if (RESULT0 != null && resultRecords != null && Convert.ToBoolean((resultRecords?.FirstOrDefault()?.IsSentSuccess)))
                    {
                        new Msgwin(false, $"پيام {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} ارسال شد....!").ShowDialog();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(resultRecords?.FirstOrDefault()?.ErrorMessage))
                        {
                            new Msgwin(false, $"{resultRecords?.FirstOrDefault()?.ErrorMessage} {(PERSONEL.SelectedItem as COMBOPERSONEL)?.SAL_NAME} ").ShowDialog();
                        }
                        else
                        {
                            new Msgwin(false, $"پیام به خاطر خطا {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} ارسال نشد!").ShowDialog();
                        }
                    }
                }
                catch (Exception)
                {
                    new Msgwin(false, $"خطا در انجام عملیات ارسال پیام {(CUST_NO.SelectedItem as Custom_CUST_HESAB)?.NAME} , پیام ارسال نشد!").ShowDialog();
                }
            }
        }
        private void Command113_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;
        }
        private void Command170_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;
        }
        private void PRSS_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationManager.IsNewRecord) return;
            if (Convert.ToDouble(NUMBER.Text) < 0) return;
        }

        private void MODAT_PPID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (Baseknow.GHAYM == 7)
            {
                if (!CL_HESABDARI.LETSGO("AZADPAY") && Convert.ToInt32(MODAT_PPID.SelectedValue) == 0)
                {
                    MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                    if (!_navigationManager.IsNewRecord)
                    {
                        MODAT_PPID.SelectedValue = _navigationManager?.CurrentRecord?.MODAT_PPID;
                    }
                    else
                    {
                        MODAT_PPID.SelectedIndex = -1;

                        //باز گردانی به نحوه پرداختی که در پیش فاکتور انتخاب شده بود
                        if (!string.IsNullOrWhiteSpace(NUMBER.Text) && NUMBER.Text != "0")
                        {
                            var havaleDate = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 DATE_N,MODAT_PPID FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG /*2*/}").FirstOrDefault();
                            if (havaleDate?.MODAT_PPID != null)
                            {
                                MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;
                                MODAT_PPID.SelectedValue = havaleDate?.MODAT_PPID; MODAT_PPID.Items.Refresh();
                                GetModatValueDays(FocusonMAS: false);
                                MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                            }
                        }

                    }
                    MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                    universControl.PopNotifyShow($"شما اجازه قيمت گذاري آزاد  نداريد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                }

                if (MODAT_PPID.SelectedItem is PRICE_PAYNO_MODATP selectedItem)
                {
                    // اگر آیتم انتخاب‌شده ممنوعه (مثلاً IsTemporary == true)
                    if (selectedItem.IsTempyDisplay)
                    {
                        MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

                        if (e != null)
                        {
                            e.Handled = true;

                            if (e.RemovedItems.Count > 0)
                            {
                                var previousItem = e.RemovedItems[0] as PRICE_PAYNO_MODATP;
                                MODAT_PPID.SelectedItem = previousItem;
                            }
                        }

                        MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;

                        universControl.PopNotifyShowUp($"این گزینه قابل انتخاب نیست : {selectedItem?.PPAME}", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Yellow);
                    }
                }

                GetModatValueDays();
            }
        }

        private void IF_AZAD_THENLOCK()
        {
            //1.0
            if (Baseknow.TKHF == 2)
            {
                TAKHFIF.IsReadOnly = true;

                N_KOL_COLUMN.IsReadOnly = true;
                N_MOIN_COLUMN.IsReadOnly = true;
            }

            //2.0
            if (Baseknow.GHAYM == 7)
            {
                if (MODAT_PPID.SelectedIndex == 0)
                {
                    MABL_COLUMN.IsReadOnly = false;
                    MABL_K_COLUMN.IsReadOnly = false;
                    N_KOL_COLUMN.IsReadOnly = false;
                    N_MOIN_COLUMN.IsReadOnly = false;
                    TKHN_COLUMN.IsReadOnly = false;

                    N_KOL_COLUMN.IsReadOnly = false;
                    N_MOIN_COLUMN.IsReadOnly = false;
                }
                else
                {
                    MABL_COLUMN.IsReadOnly = true;
                    MABL_K_COLUMN.IsReadOnly = true;
                    N_MOIN_COLUMN.IsReadOnly = true;
                    N_MOIN_COLUMN.IsReadOnly = true;
                    TKHN_COLUMN.IsReadOnly = true;

                    N_KOL_COLUMN.IsReadOnly = true;
                    N_MOIN_COLUMN.IsReadOnly = true;
                }
            }
        }

        private void INVO_LST_sub_LostFocus(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_sub.IsKeyboardFocusWithin) { return; }

            IEditableCollectionView itemsView = INVO_LST_sub.Items as IEditableCollectionView;
            if (itemsView.IsAddingNew || itemsView.IsEditingItem)
            {
                (sender as DataGrid).Dispatcher.InvokeAsync(() =>
                {
                    // Retrieve the new item/edited item
                    //object NewRecordFresh = itemsView.IsAddingNew ? itemsView.CurrentAddItem : itemsView.CurrentEditItem;
                    if (itemsView.IsAddingNew)
                    {
                        itemsView.CommitNew();
                    }
                    else if (itemsView.IsEditingItem)
                    {
                        itemsView.CommitEdit();
                    }
                });
            }
        }
        private void PAY_GETD_SUB22_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PAY_GETD_SUB22.IsKeyboardFocusWithin) { return; }

            IEditableCollectionView itemsView = PAY_GETD_SUB22.Items as IEditableCollectionView;
            if (itemsView.IsAddingNew || itemsView.IsEditingItem)
            {
                // Retrieve the new item/edited item
                //object NewRecordFresh = itemsView.IsAddingNew ? itemsView.CurrentAddItem : itemsView.CurrentEditItem;
                if (itemsView.IsAddingNew)
                {
                    itemsView.CommitNew();
                }
                else if (itemsView.IsEditingItem)
                {
                    itemsView.CommitEdit();
                }
            }
        }
        private void VISITOR_DTL_SUB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VISITOR_DTL_SUB.IsKeyboardFocusWithin) { return; }

            //IEditableCollectionView itemsView = VISITOR_DTL_SUB.Items as IEditableCollectionView;
            //if (itemsView.IsAddingNew || itemsView.IsEditingItem)
            //{
            //    // Retrieve the new item/edited item
            //    //object NewRecordFresh = itemsView.IsAddingNew ? itemsView.CurrentAddItem : itemsView.CurrentEditItem;
            //    if (itemsView.IsAddingNew)
            //    {
            //        itemsView.CancelNew();
            //    }
            //    else if (itemsView.IsEditingItem)
            //    {
            //        itemsView.CancelEdit();
            //    }
            //}
        }

        private void MODAT_PPID_LostFocus(object sender, RoutedEventArgs e)
        {
            int IDX = Convert.ToInt32(INVO_LST_sub.Columns.FirstOrDefault(c => c.SortMemberPath.Equals("ANBAR")).DisplayIndex);
            if (!MODAT_PPID.IsKeyboardFocusWithin && FACTOR22_INVO_DATA.Count == 0)
            {
                INVO_LST_sub.Dispatcher.BeginInvoke(() =>
                {
                    INVO_LST_sub.SelectedIndex = INVO_LST_sub.Items.Count - 1;

                    if (ANBAR_COLUMN.Visibility == Visibility.Visible)
                    {
                        INVO_LST_sub.CurrentCell = new DataGridCellInfo(INVO_LST_sub.SelectedItem, INVO_LST_sub.Columns[IDX]);
                    }
                    else
                    {
                        INVO_LST_sub.CurrentCell = new DataGridCellInfo(INVO_LST_sub.SelectedItem, INVO_LST_sub.Columns[NAME_CODE_INDEX_COL]);
                    }
                    INVO_LST_sub.BeginEdit();
                });
            }
        }

        private void F8_CUSTOMER_Click(object sender, RoutedEventArgs e)
        {
            if (VISITOR_DTL_SUB.IsFocused || VISITOR_DTL_SUB.IsKeyboardFocusWithin)
            {
                var grid = VISITOR_DTL_SUB;
                if (grid?.CurrentCell.Column is null) return;

                if (grid != null && grid?.CurrentCell != null && grid.CurrentCell.Column != null)
                {
                    if (grid.CurrentCell.Item is VISITOR_DTL VisitorRow && VisitorRow != null)
                    {
                        var Hes = VisitorRow.CUST_NO;
                        var Kol = CL_HESABDARI.GETKOL(Hes);
                        var Moin = CL_HESABDARI.GETMOIN(Hes);
                        var Taf = CL_HESABDARI.GETTAF(Hes);

                        double MAN;
                        if (CL_HESABDARI.BLOCKED(Kol, Moin, Taf))
                        {
                            new Msgwin(false, "حساب مورد نظر مسدود مي باشد!").ShowDialog();
                            return;
                        }

                        new F_MENU_KOL_MOIN_TAFZIL(Hes);
                    }
                }
            }
            else
            {
                if (CUST_NO.SelectedValue is not null)
                {
                    new F_MENU_KOL_MOIN_TAFZIL(CUST_NO.SelectedValue.ToString());
                }
            }

        }
        private void CheksNotPassed_Click(object sender, RoutedEventArgs e)
        {
            if (CUST_NO.SelectedValue is not null)
            {
                new chek_pass_nashodah(CUST_NO.SelectedValue.ToString()).ShowDialog();
            }
        }
        private void SalesCutsNo_Click(object sender, RoutedEventArgs e)
        {
            if (CUST_NO.SelectedValue is not null)
            {
                new froosh_customer(CUST_NO.SelectedValue.ToString()).ShowDialog();
            }
        }

        //مرتب سازی سطر های کالا
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_sub.Items.Count > 0)
            {
                #region MYREGION
                //int rd = 0;
                //int i = 0;
                //rd = 0;
                //rst.Open "SELECT COUNT(NUMBER) AS CN FROM INVO_LST WHERE TAG = 2 AND NUMBER = " + this.NUMBER, CurrentProject.Connection, adOpenKeyset, adLockOptimistic;
                //if (rst.Fields[0] > 0)
                //{
                //    DoCmd.GoToRecord acActiveDataObject,, acFirst;
                //    RDF = true;
                //    for (i = 1; i <= rst.Fields[0]; i++)
                //    {
                //        this.RADIF = i;
                //        DoCmd.GoToRecord acActiveDataObject,, acNext;
                //    }
                //}
                //---------------------------------------------------------------------------------------------------
                //var sortedItems = INVO_LST_sub.Items.Cast<INVO_LST_FACTOR22>().ToList();
                //// Iterate through the sorted items and update the RADIF column in the database
                //for (int i = 0; i < sortedItems.Count; i++)
                //{
                //    var ROW = sortedItems[i];
                //    // You may need to adjust the SQL query based on your database structure
                //    string updateQuery = $"UPDATE INVO_LST SET RADIF = {i + 1} WHERE YourPrimaryKeyColumn = '{ROW.id}'";
                //}
                //---------------------------------------------------------------------------------------------------
                #endregion

                Msgwin msg = new Msgwin(true, "آیا مایل به مرتبب سازی مجدد کالا هستید ؟");
                msg.ShowDialog();

                if (msg.DialogResult is true)
                {
                    var before = INVO_LST_sub.IsReadOnly;

                    // Retrieve the sorted items from the DataGrid
                    INVO_LST_sub.IsReadOnly = true;
                    var sortedItems = INVO_LST_sub.Items.Cast<INVO_LST_FACTOR22>().ToList();
                    for (int i = 0; i < sortedItems.Count; i++)
                    {
                        sortedItems[i].RADIF = i + 1; // Assuming you want to start index at 1
                    }
                    foreach (var item in sortedItems)
                    {
                        dbms.DoExecuteSQL($"UPDATE INVO_LST SET RADIF = {item.RADIF} WHERE id = {item.id}");
                    }

                    INVO_LST_sub.IsReadOnly = before;
                    ReGetdata();
                }
            }
        }
        //کارت انبار این کالا
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (INVO_LST_sub.Items.Count > 0)
            {
                if (INVO_LST_sub.SelectedItem is not null)
                {
                    var Row = INVO_LST_sub.SelectedItem as INVO_LST_FACTOR22;
                    if (Row?.ANBAR != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        F_MENU_KART f_MENU_KART = new F_MENU_KART("R", Row.ANBAR.ToString(), Row.CODE);
                        f_MENU_KART.ExternalCallShowReport();
                        f_MENU_KART.Close();
                    }
                }
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e) //محاسبه و اعمال مجدد تخفیف
        {
            if (INVO_LST_sub.Items.Count > 0 && PEID.SelectedValue is null && INVO_LST_sub.SelectedItem is not null)
            {
                bool Happend = false;
                foreach (var Row in FACTOR22_INVO_DATA)
                {
                    double TFS;
                    if (Baseknow.TKHF != 2)
                    {
                        Msgwin msgwin = new Msgwin(true, "تخفيف اين سطر را براي سطرهاي بعدي اعمال كنم؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult is true)
                        {
                            Happend = true;
                            TFS = (double)Row.N_KOL;
                            Row.N_KOL = TFS;
                            Row.N_MOIN = Math.Round((double)(Row.N_KOL * Row.MABL_K / 100)) + Math.Round((double)((Row.MABL_K - Math.Round((double)(Row.N_KOL * Row.MABL_K / 100))) * Row.TKHN / 100));
                            if (TICMBAA.IsChecked is true)
                            {
                                var RST2 = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("SELECT CMBAA ,CODE FROM STUF_DEF WHERE CODE = '" + Row.CODE + "'").ToList();
                                for (int i = 0; i < RST2.Count; i++)
                                {
                                    if (RST2.Count > 0)
                                    {
                                        //if (RST2.Fields("CMBAA"))
                                        if (RST2[i].CMBAA is true)
                                        {
                                            if (Row.IMBAA != Math.Round((double)((Row.MABL_K - Row.N_MOIN) * CL_HESABDARI.GetArzesh(Row.CODE) / 100)))
                                            {
                                                Row.IMBAA = Math.Round((double)((Row.MABL_K - Row.N_MOIN) * CL_HESABDARI.GetArzesh(Row.CODE) / 100));
                                            }
                                        }
                                        else if (Row.IMBAA != 0)
                                        {
                                            Msgwin msgwin1 = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                            msgwin1.ShowDialog();
                                            if (msgwin1.DialogResult is true)
                                            {
                                                Row.IMBAA = 0;
                                            }
                                        }
                                    }
                                }
                                //RST2.Close();
                            }
                            else
                            {
                                Row.IMBAA = 0;
                            }
                        }
                    }
                }

                if (Happend)
                {
                    BUTTON_SAVE_HAVALE_Click(null, null);
                }
            }
        }

        private void TAKHFIF2_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            TAKHFIF_AfterUpdate();
        }

        private void MABL_HAV2_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            MABL_HAV2_AfterUpdate();
        }

        private void MABL_VAR2_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            MABL_VAR2_AfterUpdate();
        }

        private void MABL_HAV_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_HAV_AfterUpdate
            if (Convert.ToDouble(MABL_HAV.Text) != 0 && IsNull(this.MOIN_HAV.Text))
            {
                //new Msgwin(false, "حساب مربوط به حواله مشخص نشده است حتما بايد حساب مربوط به حواله مشخص شود ").ShowDialog();
                this.MOIN_HAV.Focus();
            }
            if (Convert.ToDouble(MABL_HAV.Text) == 0)
            {
                this.MOIN_HAV.Text = "";
            }
            //CL_HESABDARI.APLAYTAKH(Convert.ToInt64(NUMBER.Text), 2, Convert.ToDouble(M_NAGHD.Text), Convert.ToDouble(MABL_VAR.Text), Convert.ToDouble(MABL_HAV.Text), (bool)TICMBAA.IsChecked); //#CheckMatter
        }

        private void TAKHFIF_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            TAKHFIF_AfterUpdate();
        }

        private void MABL_HAZ_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //MABL_HAZ_AfterUpdate
            if (Convert.ToDouble(MABL_HAZ.Text) != 0 && IsNull(this.MOIN_HAZ.Text))
            {
                //new Msgwin(false, "حساب مربوط به خدمات مشخص نشده است حتما بايد حساب مربوط به خدمات مشخص شود ").ShowDialog();
                this.MOIN_HAZ.Focus();
            }
        }

        private void NUMBER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NUMBER.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (NUMBER.Text == "+")
            {
                //HAVALE_LIST
            }

            if (NUMBER.SelectedValue is null)
            {
                if (NUMBER.Tag != null)
                {
                    NUMBER.Text = NUMBER.Tag.ToString();
                }
                else
                {
                    NUMBER.Text = "0";
                }

                e.Handled = true; //Prevent leaving ComboBox untill it fix it
            }
            else
            {
                bool isNewInvoice = NewRecord || NUMBER.SelectedValue == null || NUMBER.Tag == null;

                string title = "شماره حواله انبار";
                if (NewRecord)
                {
                    var selected = NUMBER.SelectedValue;
                    bool alreadyUsed = dbms.DoGetDataSQL<int>($"SELECT COUNT(*) FROM HEAD_LST WHERE TAG = {fTAG} AND NUMBER = {selected}").First() > 0;
                    if (alreadyUsed)
                    {
                        new Msgwin(false, $"نمیتوانید {title} که قبلا ثبت کرده ای استفاده کنید").ShowDialog();
                        NUMBER.SelectedValue = NUMBER.Tag; NUMBER.Items.Refresh();
                        return;
                    }
                }


                //اکر در فاکتور از قبل ثبت شده شماره رسید را تغییر داده بود
                if (NUMBER.Tag != null && NUMBER.SelectedValue != NUMBER.Tag)
                {
                    new Msgwin(false, "نمیتوانید حواله که قبلا ثبت کرده ای ا تغییر دهید , تنها میتوانید این فاکتور را حذف نمایید , انتخاب حواله انبار تنها در فاکتور جدید ممکن است").ShowDialog();
                    NUMBER.SelectedValue = NUMBER.Tag; NUMBER.Items.Refresh();
                }
                else
                {
                    ReGetdata();

                    if (!string.IsNullOrWhiteSpace(NUMBER.Text) && NUMBER.Text != "0")
                    {

                        var havaleDate = dbms.DoGetDataSQL<HEAD_LST>($"SELECT TOP 1 DATE_N,MODAT_PPID FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = {hTAG /*2*/}").FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(havaleDate?.DATE_N.ToString()))
                        {
                            DATE_N.Text = havaleDate?.DATE_N.ToString(); //تاریخ فاکتور طبق حواله انبار فروش آن باشد.
                            DATE_N_TAG = havaleDate?.DATE_N.ToString();
                        }
                        if (havaleDate?.MODAT_PPID != null)
                        {
                            MODAT_PPID.SelectionChanged -= MODAT_PPID_SelectionChanged;

                            MODAT_PPID.SelectedValue = havaleDate?.MODAT_PPID; MODAT_PPID.Items.Refresh();
                            GetModatValueDays(FocusonMAS: false);

                            MODAT_PPID.SelectionChanged += MODAT_PPID_SelectionChanged;
                        }
                    }

                    BUTTON_SAVE_HAVALE.IsEnabled = true;
                }

            }

        }

        private void BTN_FACTORHA_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 13); //فاکتور فروش
            if (NewRecord)
            {
                this.Close();
            }
        }

        private void DoExportyPricesCalculate(bool IsSingleCurrentRow, INVO_LST_FACTOR22? TheRow, bool DoShoeMessages = true)
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
                    foreach (var Row in FACTOR22_INVO_DATA)
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
            DoExportyPricesCalculate(false, null);
        }

        private void OpenInterNationalInvoice()
        {
            if (!IsExporty)
            {
                return;
            }


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Factors.InterInvoice.mrt");
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
            //            I'm using C# WPF .NET Core 6 WPF Application

            //string userSelectedCurrencySymbol = "£"; // Example: this could be based on user input

            //            txt.TextFormat = new StiCurrencyFormatService(1, 2, ",", " ", 3, userSelectedCurrencySymbol, true, true, " ");

            //            var textComponent = report.GetComponentByName("TextCurrency") as StiText;
            //            if (textComponent != null)
            //            {
            //                textComponent.TextFormat.FormatString = "#,##0.00 ¤";
            //                textComponent.TextFormat.CurrencySymbol = "€"; // Set to Euro symbol or any other desired symbol
            //            }
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

        #region MOADIAN
        private bool MoadianIsValid(bool displayErrors = true)
        {
            var errorMessages = new List<MsgModel>();

            try
            {
                if (ins.SelectedValue.ToStringNullSafe() != "1" && string.IsNullOrWhiteSpace(irtaxid.Text))
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "شناسه مالیاتی صورتحساب نباید خالی باشد." });
                }

                if (inty.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً نوع صورتحساب را انتخاب کنید." });
                }
                else
                {
                    if (Convert.ToInt32(inty.SelectedValue) == 1) //نوع اول
                    {
                        if (setm.SelectedItem == null)
                        {
                            errorMessages.Add(new MsgModel { MessageText_U = "روش تسویه انتخاب نشده است." });
                        }
                        else
                        {
                            var selectedSettlementMethod = setm.SelectedItem.ToString();

                            if (selectedSettlementMethod == "نقد/نسیه" || Convert.ToInt32(setm.SelectedValue) == 3)
                            {
                                if (!decimal.TryParse(insp.Text, out decimal inspValue) || inspValue < 0)
                                {
                                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مبلغ نسیه وارد کنید." });
                                }
                                else
                                {
                                    var JAME_KOL = Convert.ToInt32(JF.Text); //|| GHABEL
                                    var NN = Convert.ToInt32(insp.Text);

                                    if (NN >= JAME_KOL)
                                    {
                                        errorMessages.Add(new MsgModel { MessageText_U = "در روش تسویه [نقد/نسیه] مبلغ این قسمت باید از جمع کل فاکتور کمتر باشد " });
                                    }
                                }
                            }
                        }
                    }
                }

                if (inp.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "الگوی صورتحساب انتخاب نشده است." });
                }

                if (ins.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "موضوع صورتحساب انتخاب نشده است." });
                }

                if (!decimal.TryParse(torv.Text, out decimal torvValue) || torvValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مجموع ارزش وارد کنید." });
                }

                if (!decimal.TryParse(tocv.Text, out decimal tocvValue) || tocvValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مبلغ پرداختی نقدی وارد کنید." });
                }

                if (!decimal.TryParse(cap.Text, out decimal capValue) || capValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "مبلغ پرداختی نقدی به درستی وارد نشده است." });
                }

            }
            catch (Exception ex)
            {
                errorMessages.Add(new MsgModel { MessageText_U = $"خطا در اعتبارسنجی: {ex.Message}" });
            }

            if (errorMessages.Any())
            {
                if (displayErrors)
                {
                    errorMessages = errorMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, errorMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }

        bool MoadianHeaderIsOk = false;
        private void BTN_SAVE_HEXTENDED_Click(object sender, RoutedEventArgs e)
        {
            if (NewRecord)
            {
                new Msgwin(false, "ابتدا فاکتور را ذخیره کنید سپس مجددا اقدام کنید").ShowDialog(); ;
                return;
            }

            MoadianHeaderIsOk = false;

            if (!MoadianIsValid())
            {
                return;
            }

            var HLE = dbms.DoGetDataSQL<HEAD_LST_EXTENDED>($"SELECT TOP 1 inty FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = {NUMBER.Text} AND tgu = 2").FirstOrDefault();
            var IsNewMoadian = HLE == null;
            try
            {
                if (IsNewMoadian)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST_EXTENDED(NUMBER, tgu, inty, inp, ins, sbc, bbc, ft, bpn, scln, scc, cdcn, cdcd, crn, billid, todam, tonw, torv, tocv, setm, cap, insp, tvop, tax17, cut, irtaxid)
                                     VALUES({NUMBER.Text},
                                     2 ,
                                     {inty.SelectedValue} ,
                                     {inp.SelectedValue}   ,
                                     {ins.SelectedValue}   ,
                                     N'{sbc.Text}' ,
                                     N'{bbc.Text}' ,
                                     {ft.Text} ,
                                     N'{bpn.Text}' ,
                                     N'{scln.Text}' ,
                                     N'{scc.Text}' ,
                                     N'{cdcn.Text}' ,
                                     {cdcd.Text}   ,
                                     N'{crn.Text}' ,
                                     N'{billid.Text}' ,
                                     {(string.IsNullOrEmpty(todam.Text) ? "NULL" : todam.Text)},
                                     {(string.IsNullOrEmpty(tonw.Text) ? "NULL" : tonw.Text)},
                                     {(string.IsNullOrEmpty(torv.Text) ? "NULL" : torv.Text)},
                                     {(string.IsNullOrEmpty(tocv.Text) ? "NULL" : tocv.Text)},
                                     {setm.SelectedValue} ,
                                     {(string.IsNullOrEmpty(cap.Text) ? "NULL" : cap.Text)},
                                     {insp.Text},
                                     {tvop.Text},
                                     {tax17.Text},
                                     N'{CUT.SelectedValue}' ,
                                     N'{irtaxid.Text}' )");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST_EXTENDED
                     SET inty = {inty.SelectedValue},
                         inp = {inp.SelectedValue},
                         ins = {ins.SelectedValue},
                         sbc = N'{sbc.Text}',
                         bbc = N'{bbc.Text}',
                         ft = {ft.Text},
                         bpn = N'{bpn.Text}',
                         scln = N'{scln.Text}',
                         scc = N'{scc.Text}',
                         cdcn = N'{cdcn.Text}',
                         cdcd = {cdcd.Text},
                         crn = N'{crn.Text}',
                         billid = N'{billid.Text}',
                         todam = {(string.IsNullOrEmpty(todam.Text) ? "NULL" : todam.Text)},
                         tonw = {(string.IsNullOrEmpty(tonw.Text) ? "NULL" : tonw.Text)},
                         torv = {(string.IsNullOrEmpty(torv.Text) ? "NULL" : torv.Text)},
                         tocv = {(string.IsNullOrEmpty(tocv.Text) ? "NULL" : tocv.Text)},
                         setm = {setm.SelectedValue},
                         cap = {(string.IsNullOrEmpty(cap.Text) ? "NULL" : cap.Text)},
                         insp = {insp.Text},
                         tvop = {tvop.Text},
                         tax17 = {tax17.Text},
                         cut = N'{CUT.SelectedValue}',
                         irtaxid = N'{irtaxid.Text}'
                     WHERE NUMBER = {NUMBER.Text} AND tgu = 2");

                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در ذخیره صورت حساب برای مودیان , لطفا مقادیر را بررسی کنید").ShowDialog();
                return;
            }

            MoadianHeaderIsOk = true;
        }

        private void BTN_SEND_INVOICE_Click(object sender, RoutedEventArgs e)
        {
            var dt = DateTime.Now;
            CL_HESABDARI.TR("HEAD_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
            CL_HESABDARI.TR("INVO_LST", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
            CL_HESABDARI.TR("PAY_GETD", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);
            CL_HESABDARI.TR("TAKHFIF_APLAY", "(NUMBER = " + this.NUMBER.Text + ") AND (kind = 2)", dt, 1);
            CL_HESABDARI.TR("OTHER_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 13)", dt, 1);
            CL_HESABDARI.TR("VISITOR_DTL", "(NUMBER = " + this.NUMBER.Text + ") AND (TAG = 2)", dt, 1);

            if (!MoadianHeaderIsOk)
            {
                BTN_SAVE_HEXTENDED_Click(null, null);
            }

            #region SecondTryToAvoidNullHEAD_LST_EXTENDED
            //رفع خطای : سربرگ مودیان مربوط به فاکتور خالی است , ابتدا یکباره دیگر نوع صورت حساب را انتخاب کرده و سپس روی مانده حساب دابل کلیک کنید و دوباره امتحان کنید.
            var _HEAD_EXTENDED = dbms.DoGetDataSQL<HEAD_LST_EXTENDED>($"SELECT * FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = {NUMBER.Text} AND TGU = 2").FirstOrDefault();
            if (_HEAD_EXTENDED is null)
            {
                dbms.DoExecuteSQL(@$"INSERT INTO dbo.HEAD_LST_EXTENDED(NUMBER, tgu, inty, inp, ins, sbc, Bbc, ft, bpn, scln, scc, cdcn, cdcd, crn, billid, todam, tonw, torv, tocv, setm, cap, insp, tvop, tax17, cut, irtaxid)
                                VALUES({NUMBER.Text}, 2, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, '2', DEFAULT);");
                _HEAD_EXTENDED = dbms.DoGetDataSQL<HEAD_LST_EXTENDED>($"SELECT * FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = {NUMBER.Text} AND TGU = 2").FirstOrDefault();
            }
            if (_HEAD_EXTENDED != null && _HEAD_EXTENDED.inty is null)
            {
                dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST_EXTENDED SET inty = 1 WHERE NUMBER = {NUMBER.Text} AND TGU = 2");
                _HEAD_EXTENDED.inty = 1;
            }
            #endregion



            if (MoadianHeaderIsOk)
            {
                try
                {
                    var _NUMBER_ = Convert.ToInt64(NUMBER.Text);
                    var _TGU_ = Convert.ToInt32(hTAG);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "MOADIAN SEND BUTTON CALLED IN F4",
                            tableName: "ارسال صورت حساب مودیان",
                            recordId: $"NUMBER {_NUMBER_} TGU: {_TGU_}",
                            oldValue: null,
                            newValue: $" inty: {inty.SelectedValue} inp:{inp.SelectedValue} ins:{ins.SelectedValue} irtaxid:{irtaxid.Text} CUST_NO:{CUST_NO.SelectedValue}",
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    if (CL_HESABDARI.MoadianLock(_NUMBER_, _TGU_))
                    {
                        string directoryPath = @"C:\CORRECT\";
                        string filePath = Path.Combine(directoryPath, "cnr.udl");

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Create a file and write connection string
                        using (StreamWriter writer = new StreamWriter(filePath, false))
                        {
                            writer.WriteLine(CL_CCNNMANAGER.CONNECTION_STR);
                        }

                        BTN_SEND_INVOICE.IsEnabled = false;

                        // Execute the external program
                        string arguments = $"{_NUMBER_}_{_TGU_}_m";
                        var PRC = Process.Start(new ProcessStartInfo
                        {
                            FileName = Path.Combine(directoryPath, "MOADIAN.EXE"),
                            Arguments = arguments,
                            UseShellExecute = true,
                            //WindowStyle = ProcessWindowStyle.Normal
                        });

                        PRC.WaitForExit();

                        BTN_SEND_INVOICE.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در انجام عملیات ارسال").ShowDialog();
                }
            }
        }

        private void moadian_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                //IsFocused = false;
            }
            else
            {
                //IsFocused = true;
                var _M_NAGHD_ = Convert.ToInt64(M_NAGHD.Text);
                var _MABL_VAR_ = Convert.ToInt64(MABL_VAR.Text);
                var _MABL_HAV_ = Convert.ToInt64(MABL_HAV.Text);
                var _NCHK_ = Convert.ToInt64(NCHK.Text);

                var CC = _M_NAGHD_ + _MABL_VAR_ + _MABL_HAV_ + _NCHK_;

                var _GHABEL_ = Convert.ToInt64(GHABEL.Text);
                var _MBAA_ = Convert.ToInt64(MBAA.Text);

                var _insp_ = _GHABEL_ - _MBAA_ - CC;
                insp.Text = _insp_.ToStringNullSafe();
                cap.Text = CC.ToStringNullSafe();
            }
        }

        #endregion

        private void INVO_LST_sub_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null || dataGrid?.SelectedItem == CollectionView.NewItemPlaceholder || dataGrid?.SelectedItem?.ToString() == "{NewItemPlaceholder}")
            {
                e.Handled = true;
                return;
            }
            //base.OnContextMenuOpening(e);
        }

        private void INVO_LST_sub_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
            {
                return;
            }

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

        private void GetDefaultFocus()
        {
            if (IsDirectFactor)
            {
                CUST_NO.Focus();
            }
            else
            {
                NUMBER.Focus();
            }
        }

        private void MakeOKFReady()
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 67, 1) == "5")
            {
                OKF.IsChecked = true;
            }
            else
            {
                OKF.IsChecked = false;
            }
        }

        private void PEPID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEPID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }

            GoGheymateUpdator();
        }

        private void PEID_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PEID.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }

            if (NowIsReady && Baseknow.GHAYM.ToString() == "7")
            {
                MODAT_PPID_Enter(); //بروز رسانی سورس نحوه پرداخت بر اساس اعلامیه ها
            }

            GoGheymateUpdator();
        }

        private void GoGheymateUpdator()
        {
            var IsSavedBefore = CL_LMethods.IsNumeric(NUMBER.Text) && NUMBER.Text != "0";

            if (NowIsReady && CL_Generaly.IsGHAYM_7 && IsSavedBefore)
            {
                if (SGN1.IsChecked is false && SGN2.IsChecked is false && SGN3.IsChecked is false)
                {
                    if (CUST_KIND.SelectedValue != null && DEPATMAN.SelectedValue != null && MODAT_PPID.SelectedValue != null)
                    {
                        ChangeIsHappend = true;

                        //if (PEID.SelectedValue is null)
                        //{
                        //    CL_HESABDARI.UpdateGHeymat(Convert.ToInt32(NUMBER.Text), fTAG, Convert.ToInt64(DATE_N.Text.ToRawTarikh()), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(CUST_KIND.SelectedValue), Convert.ToInt32(DEPATMAN.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked));
                        //}
                        //else if (PEID.SelectedValue != null && PEPID.SelectedValue != null)
                        //{
                        //    CL_HESABDARI.UpdateGHeymatFF(Convert.ToInt32(NUMBER.Text), fTAG, Convert.ToInt32(PEPID.SelectedValue), Convert.ToInt32(PEID.SelectedValue), Convert.ToInt32(MODAT_PPID.SelectedValue), Convert.ToInt32(TICMBAA.IsChecked), Convert.ToInt32(CUST_KIND.SelectedValue));
                        //}

                        if (Convert.ToInt32(MODAT_PPID.SelectedValue) == 0) //اگر آزاده بیا بیرون
                        {
                            return;
                        }


                        int retVal = ExecutePricingUpdate(
                            Convert.ToInt32(NUMBER.Text),
                            fTAG, //13
                            PEPID.SelectedValue is null ? 0 : Convert.ToInt32(PEPID.SelectedValue),
                            PEID.SelectedValue is null ? 0 : Convert.ToInt32(PEID.SelectedValue),
                            Convert.ToInt32(MODAT_PPID.SelectedValue),
                            TICMBAA.IsChecked == true,
                            Convert.ToInt32(CUST_KIND.SelectedValue),
                            Convert.ToInt32(DATE_N.Text.ToRawTarikh()),
                            Convert.ToInt32(DEPATMAN.SelectedValue));

                        string? strSpecificError = default;
                        if (retVal != 0)
                        {
                            switch (retVal)
                            {
                                case -1:
                                    strSpecificError = "خطا: اعلامیه قیمت فعال یافت نشد.";
                                    break;
                                case -2:
                                    strSpecificError = "خطا: قیمت برای یک یا چند کالا در اعلامیه قیمت مشخص، تعریف نشده است.";
                                    break;
                                case -99:
                                    strSpecificError = "خطا: یک خطای عمومی در پایگاه داده رخ داد.";
                                    break;
                                default:
                                    strSpecificError = "خطا: عملیات ناموفق بود. کد خطای ناشناخته: " + retVal;
                                    break;
                            }
                            new Msgwin(false, strSpecificError).ShowDialog();
                        }

                        ReGetdata();

                        if (!string.IsNullOrEmpty(strSpecificError)) //Error Happened
                        {
                            return;
                        }

                        universControl.PopNotifyShowUp("قیمت بروز شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green, 1);

                        IF_AZAD_THENLOCK();
                    }
                }
            }
        }

        private int ExecutePricingUpdate(int numb, int tgg, int pepid, int peid, int modat_ppid, bool ticmbaa, int cust_kind, int dtt, int depatman)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@numb", numb);
            param.Add("@tgg", tgg);
            param.Add("@PEPID_In", pepid);
            param.Add("@PEID_In", peid);
            param.Add("@MODAT_PPID_In", modat_ppid);
            param.Add("@TICMBAA_In", ticmbaa);
            param.Add("@CUST_KIND_In", cust_kind);
            param.Add("@DTT_In", dtt);
            param.Add("@DEPATMAN_In", depatman);
            param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            dbms.OpenStoredProcedure("sp_UpdateInvoicePricingAndDiscount", param);

            return param.Get<int>("@ReturnValue");
        }

        private void N_S_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!NewRecord)
            {
                if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
                {
                    CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), false);
                }
            }
        }

        private void MABNA_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!NewRecord)
            {
                if (!string.IsNullOrEmpty(N_S.Text) && N_S.Text != "0")
                {
                    CL_MenuManager.MenuBaseOnKindOpen(this, dbms, 0, Convert.ToDouble(N_S.Text), true);
                }
            }
        }

        private void CUST_NO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // بررسی کلید میانبر: Ctrl + '
            if ((e.Key == Key.OemQuotes || e.Key == Key.Oem7) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (_navigationManager == null || !IsDirectFactor)
                {
                    return;
                }

                if (_navigationManager.CurrentRecordIndex > 0)
                {
                    // اگر مقداری از قبل انتخاب شده، تاییدیه بگیرید
                    if (CUST_NO.SelectedValue != null)
                    {
                        Msgwin msgwin = new Msgwin(true, "آیا از اعمال نام مشتری قبلی برای این رکورد قبلی مطمئن هستید؟");
                        msgwin.ShowDialog();
                        if (msgwin.DialogResult == false)
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    var previousRecord = _navigationManager.RecordsData[_navigationManager.CurrentRecordIndex - 1];
                    if (previousRecord != null && !string.IsNullOrEmpty(previousRecord.CUST_NO))
                    {
                        string sql = "SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = @Hes";
                        var data = dbms.DoGetDataSQL<CUST_HESAB>(sql, new { Hes = previousRecord.CUST_NO }).FirstOrDefault();

                        if (data != null && !string.IsNullOrEmpty(data.hes))
                        {
                            string thevalue = data.hes;

                            if (CUST_NO.ItemsSource == null)
                            {
                                CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
                            }

                            // کست کردن ایمن به لیست جنریک
                            var currentList = CUST_NO.ItemsSource as IList<Custom_CUST_HESAB>;

                            if (currentList != null)
                            {
                                // اگر آیتم در لیست دراپ‌داون وجود ندارد، آن را اضافه کن
                                if (!currentList.Any(item => item?.hes == thevalue))
                                {
                                    currentList.Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });

                                    CUST_NO.Items.Refresh();
                                }

                                // انتخاب آیتم
                                CUST_NO.SelectedValue = null; // ریست کردن برای اطمینان از تغییر (در برخی موارد خاص WPF)
                                CUST_NO.SelectedValue = thevalue;
                            }
                        }
                    }
                }
                // جلوگیری از تایپ شدن کاراکتر ' در تکست باکس
                e.Handled = true;
            }
        }
    }
}
