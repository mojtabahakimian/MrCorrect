using AUTO_BAZ.HelperWins;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static AUTO_BAZ.Functions.CL_LMethods;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace AUTO_BAZ.Functions
{
    public static class CL_HESABDARI_AUTO_BAZ
    {
        static CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public static bool UseSmartThrottlingByDefault { get; set; } = false;

        #region LOOKUP_CACHE
        // ───────────────────────────────────────────────────────────────────────────────
        // کش جستجوهای تکراری.
        //
        // چرا لازم است: توابع کمکی مثل ISHESAB، GETTAFNAME و GETF_DEPART داخل حلقه‌های
        // بازسازی و به‌ازای هر قلم کالا صدا زده می‌شوند و هر بار یک رفت‌وبرگشت کامل به
        // SQL Server می‌زنند — در حالی که تقریباً همیشه همان جواب قبلی را برمی‌گردانند.
        // مثلاً GETF_DEPART(20) هزاران بار صدا زده می‌شود و همیشه یک رشته می‌دهد.
        // در یک فاکتور با ۱۰ قلم کالا، این توابع به‌تنهایی ده‌ها رفت‌وبرگشت تولید می‌کنند.
        //
        // ایمنی: هر سه «خواندن خالص» از جدول‌های مرجع هستند که در طول یک اجرای بازسازی
        // تغییر نمی‌کنند — به‌جز حساب‌هایی که خود CREATHES می‌سازد و بلافاصله کش را
        // به‌روز می‌کند. ConcurrentDictionary برای استفاده‌ی همزمان چند Thread امن است.
        //
        // ⚠️ در ابتدای هر اجرای بازسازی حتماً ClearLookupCaches() صدا زده شود تا اگر
        //    کاربر بین دو اجرا حسابی اضافه کرده باشد، داده‌ی کهنه نماند.
        //
        // ───────────────────────────────────────────────────────────────────────────────
        // ⚠️⚠️ خطر «نرخ کهنه» — قاعده‌ای که هنگام افزودن هر کش تازه باید رعایت شود:
        //
        //   هیچ چیزی که «خودِ بازسازی آن را می‌نویسد» نباید کش شود.
        //
        // جدول‌هایی که بازسازی به آن‌ها می‌نویسد (بررسی‌شده روی کل AUTO_BAZ):
        //   • INVO_LST (AVRAGE, AVRAGE2, MABL, MABL_K)  ← C0_TASK «بازسازی نرخ میانگین»
        //   • DTL_MANF (MABLK, SMABL)                    ← C0_TASK
        //   • INVO_LST (N_KOL, N_MOIN) فقط برای TAG = 14 ← C11 / GENSANADVD
        //   • TDETA_HES                                  ← CREATHES (که خودش کش را به‌روز می‌کند)
        //   • DEED_HED / DEED_DTL / HEAD_LST.N_S         ← خروجی خودِ بازسازی
        //
        // به همین دلیل کش دقیقاً بعد از پایان C0/C00 روشن می‌شود (MainWindow.LetsGoBtn_Click):
        // بهای تمام‌شده‌ی استانداردی که در کش می‌نشیند، حتماً «بعد از» اصلاح DTL_MANF خوانده شده.
        //
        // نکته‌ی مهم برای سند خروج مواد: نرخ ساخت (DTL_MANF.SMABl) و مبلغ ردیف (INVO_LST.MABL_K)
        // هرگز کش نمی‌شوند؛ در هر اجرا مستقیم و تازه از دیتابیس خوانده می‌شوند. تنها چیزی که
        // آنجا کش می‌شود گروه کالا (STUF_DEF.RADAH) و نام کالا/حساب است که هیچ‌کدام قیمت نیستند
        // و بازسازی هم به آن‌ها نمی‌نویسد.
        // ───────────────────────────────────────────────────────────────────────────────

        // کلید کش عمداً double است و نه int: صداکننده‌های ISHESAB مقادیری مثل
        // Convert.ToInt64(CODE) می‌فرستند و STUF_DEF.CODE از نوع nvarchar(15) است،
        // پس می‌تواند از محدوده‌ی int بیرون بزند. Convert.ToInt32 روی چنین مقداری
        // OverflowException می‌داد — خطایی که قبلاً وجود نداشت.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(double Kol, double Moin, double Taf), bool> _existingAccounts = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _tafNameCache = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, string> _departNameCache = new();

        // قیمت استاندارد (مواد / دستمزد / سربار) به‌ازای هر قلم کالای هر فاکتور خوانده می‌شود.
        // هر سه تابع GETSTANDARDPRICE_* عیناً یک کوئری سنگین روی HEAD_MANF+DTL_MANF می‌زنند
        // و هرکدام یک GETLASTFR هم صدا می‌زنند — یعنی ۶ رفت‌وبرگشت برای هر قلم کالا.
        // این‌ها «خواندن خالص» از جدول‌های تولید هستند و در طول یک بازسازی تغییر نمی‌کنند.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Code, long Dt), double> _standardPriceMavad = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Code, long Dt), double> _standardPriceDast = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Code, long Dt), double> _standardPriceSar = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(string Code, long Dt), double> _lastFrCache = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<double, string> _kalaNameCache = new();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<double, string> _bankNameCache = new();

        // گروه کالا (STUF_DEF.RADAH) به‌ازای «هر قلم هر برگه» خوانده می‌شود — مثلاً در سند خروج مواد
        // برای تعیین معین حساب فازهای تولید. «خواندن خالص» است و در طول یک بازسازی تغییر نمی‌کند.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> _kalaGroupCache = new();

        /// <summary>
        /// کش فقط در جریان «بازسازی دسته‌ای» فعال می‌شود.
        /// پیش‌فرض خاموش است چون فرم‌های برنامه‌ی اصلی هم همین توابع را صدا می‌زنند و
        /// آنجا کاربر می‌تواند وسط کار نام حساب یا دپارتمان را عوض کند؛ کش ماندگار
        /// باعث می‌شد تا پایان عمر برنامه مقدار کهنه برگردد.
        /// </summary>
        public static bool LookupCacheEnabled { get; set; } = false;

        /// <summary>
        /// پاک کردن همه‌ی کش‌های جستجو. در ابتدای هر اجرای بازسازی صدا زده شود.
        /// </summary>
        public static void ClearLookupCaches()
        {
            _existingAccounts.Clear();
            _tafNameCache.Clear();
            _departNameCache.Clear();
            _standardPriceMavad.Clear();
            _standardPriceDast.Clear();
            _standardPriceSar.Clear();
            _lastFrCache.Clear();
            _kalaNameCache.Clear();
            _bankNameCache.Clear();
            _kalaGroupCache.Clear();
        }

        /// <summary>
        /// ثبت اینکه یک حساب تفصیلی قطعاً وجود دارد (بعد از ساخت موفق آن).
        /// </summary>
        private static void MarkAccountExists(double kol, double moin, double taf)
        {
            if (LookupCacheEnabled)
            {
                _existingAccounts[(kol, moin, taf)] = true;
            }
        }
        #endregion

        #region Custom_Modelses
        /// <summary>
        /// خروجی کوئری‌های «جمع به‌ازای هر سند» که یکجا پیش‌خوانده می‌شوند.
        /// </summary>
        public class InvoiceSumRow
        {
            public double? NUMBER { get; set; }
            public double? Total { get; set; }
        }

        /// <summary>
        /// یک ردیف کالای فاکتور به‌همراه نام کالا. همان ستون‌هایی که QRE12/QRE14
        /// می‌خوانند، به‌علاوه‌ی NUMBER تا بتوان ردیف‌ها را به فاکتورشان نسبت داد.
        /// </summary>
        public class InvoiceLineRow
        {
            public double? NUMBER { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string CODE { get; set; }
            public int? ANBAR { get; set; }
            public string NAME { get; set; }
            public double? AVRAGE { get; set; }
        }

        public class QUERY_MODEL6
        {
            public int? IDH { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public int? RADIF { get; set; }
            public int? N_MOIN { get; set; }
            public int? N_TAF { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public double? N_S { get; set; }
            public double? MABL { get; set; }
            public int? N_KOL { get; set; }
            public int? N_MOIN_PGD { get; set; }
            public int? N_TAF_PGD { get; set; }
            public int? KIND { get; set; }
            public string? HES1 { get; set; }
        }
        public class CHKREC_H
        {
            public long? DATE { get; set; }
            public string? MOLAH { get; set; }
            public double? N_S { get; set; }
            public int? IDH { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
        }
        public class QUERY_MODEL5
        {
            public double? NUMBER { get; set; }
            public long? id { get; set; }
            public double? TAG { get; set; }
            public int? CUST_CO { get; set; }
            public string? TAKH_COD { get; set; }
            public short? TAFPER { get; set; }
            public double? MABL_K { get; set; }
            public string? NAME { get; set; }
            public double? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
        }
        public class QUERY_MODEL4
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? MABL_K { get; set; }
            public double? N_MOIN { get; set; }
            public string? CODE { get; set; }
            public int? CUST_KIND { get; set; }
        }
        public class QUERY_MODEL3
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public string? NAME { get; set; }
        }
        public class QUERY_MODEL2
        {
            public int? GRD_NUM { get; set; }
            public long? GRD_DATE { get; set; }
            public int? GRD_ANBAR { get; set; }
            public string? GRD_HES { get; set; }
            public double? N_S { get; set; }
            public string? COMMENT { get; set; }
            public string? USER_NAME { get; set; }
        }
        public class QUERY_MODEL1
        {
            public double? JAMT { get; set; }
            public string? CODE { get; set; }
            public int? CUST_KIND { get; set; }
        }
        public class QRE_BAZ_18
        {
            public int? GRD_NUM { get; set; }
            public string? CODE { get; set; }
            public double? MOG { get; set; }
            public double? NUM1 { get; set; }
            public double? NUM2 { get; set; }
            public double? NUM3 { get; set; }
            public double? MABL { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public double? EKH { get; set; }
        }
        public class QRE_BAZ_17
        {
            public double? JAMT { get; set; }
            public string? CODE { get; set; }
            public int? CUST_KIND { get; set; }
        }
        public class QRE_BAZ_16
        {
            public double? MABL_K { get; set; }
            public double? MEGH_MAR { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public string? NAME { get; set; }
            public double? GHT { get; set; }
        }
        public class QRE_BAZ_15
        {
            public double? MABL_K { get; set; }
            public double? MEGH_MAR { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public string? NAME { get; set; }
            public int? CUST_KIND { get; set; }
            public short? TFP { get; set; }
            public double? takh { get; set; }
            public double? AVRAGE { get; set; }
        }
        public class QRE_BAZ_14
        {
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public string? CODE { get; set; }
            public int? FNUMB { get; set; }
        }
        public class QRE_BAZ_13
        {
            public int? FNUMB { get; set; }
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
            public string? NAME { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
            public double? SumOfMEGHk { get; set; }
            public string? COM { get; set; }
            public double? MEGHM { get; set; }
            public int? anbar { get; set; }
        }
        public class QRE_BAZ_12
        {
            public double? NUMBER { get; set; }
            public double? N_KOL { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? SumOfMEGH { get; set; }
            public double? SumOfMEGHk { get; set; }
            public double? SumOfMEGH_MAR { get; set; }
            public double? SumOfMABL { get; set; }
            public double? SumOfMABL_K { get; set; }
            public bool? FROM_A { get; set; }
            public string? N_RASID { get; set; }
            public double? MEGH_R { get; set; }
            public double? RADAH { get; set; }
            public double? SANAD_NO { get; set; }
            public double? CUST_NO { get; set; }
            public double? ANBARF { get; set; }
            public int? VAHED_K { get; set; }
            public string? NAME { get; set; }
        }
        public class QRE_BAZ_11
        {
            public string? NAME { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
            public double? MEGHk { get; set; }
            public double? IMBIBE_MANF { get; set; }
            public double? IMBIBE_SAR { get; set; }
            public string? CODE { get; set; }
        }
        public class QRE_BAZ_10
        {
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
            public string? NAME { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
            public double? SumOfMEGHk { get; set; }
            public string? COM { get; set; }
            public double? MEGHM { get; set; }
            public int? ANBAR { get; set; }
        }
        public class QRE_BAZ_9
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? SumOfMEGH { get; set; }
            public double? SumOfMEGHk { get; set; }
            public double? SumOfMEGH_MAR { get; set; }
            public double? SumOfMABL { get; set; }
            public double? SumOfMABL_K { get; set; }
            public bool? FROM_A { get; set; }
            public string? N_RASID { get; set; }
            public double? MEGH_R { get; set; }
            public double? RADAH { get; set; }
            public double? SANAD_NO { get; set; }
            public double? CUST_NO { get; set; }
            public double? ANBARF { get; set; }
            public int? VAHED_K { get; set; }
            public string? NAME { get; set; }
        }
        public class QRE_BAZ_8
        {
            public string? CODE { get; set; }
            public double? MABLK { get; set; }
            public string? NAME { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER { get; set; }
            public double? SumOfMEGHk { get; set; }
            public string? COM { get; set; }
            public double? MEGHM { get; set; }
            public int? ANBAR { get; set; }
        }
        public class QRE_BAZ_7
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? SumOfMEGH { get; set; }
            public double? SumOfMEGHk { get; set; }
            public double? SumOfMEGH_MAR { get; set; }
            public double? SumOfMABL { get; set; }
            public double? SumOfMABL_K { get; set; }
            public bool? FROM_A { get; set; }
            public string? N_RASID { get; set; }
            public double? MEGH_R { get; set; }
            public double? RADAH { get; set; }
            public double? SANAD_NO { get; set; }
            public double? CUST_NO { get; set; }
            public double? ANBARF { get; set; }
            public int? VAHED_K { get; set; }
            public string? NAME { get; set; }
        }
        public class QRE_BAZ_6
        {
            public int? FNUMB { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public int? N_KOL { get; set; }
            public string? NAMES { get; set; }
        }
        public class QRE_BAZ_5
        {
            public double? SANAD_NO { get; set; }
            public string? N_RASID { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
        }
        public class QRE_BAZ_4
        {
            public double? SBED { get; set; }
            public double? SBES { get; set; }
        }
        public class QRE_BAZ_3
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? SMAB { get; set; }
        }
        public class RST2_Data
        {
            //Deed_DTL
            //Deed_HED
            public double? FR { get; set; }
            public double N_S { get; set; }
            public long DATE_S { get; set; }
            public int HES_K { get; set; }
        }
        public class RST_PAY
        {
            //Deed_DTL
            //Deed_HED
            public double? JCH { get; set; }
        }
        private class GTNMVAHED
        {
            public string CODE { get; set; }
            public double? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }
        private class SHAHR
        {
            public string EN_CUST_CO { get; set; }
            public int EN_CITY { get; set; }
            public int EN_IYALAT { get; set; }
            public int EN_COUNTRY { get; set; }
        }
        public partial class Custom_TF12
        {
            public float TF1 { get; set; }
            public float TF2 { get; set; }
        }
        public partial class Custom_PEID
        {
            public int PEID { get; set; }
        }
        private partial class Custom_PRICEONE
        {
            public float PRICE1 { get; set; }
            public int PGID { get; set; }
            public string CODE { get; set; }
            public int PEPID { get; set; }
        }
        private partial class Custom_PRICE_ELAMIE1
        {
            public int PEPID { get; set; }
        }
        public partial class Custom_PRICE_ELAMIE
        {
            public int PEPID { get; set; }
        }
        public partial class Custom2_PRICE_ELAMIE
        {
            public float PRICE1 { get; set; }
            public int PGID { get; set; }
            public int PEPID { get; set; }
            public string CODE { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public int ANBAR { get; set; }
            public double MEGH { get; set; }
            public double MEGHk { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public Nullable<double> N_KOL { get; set; }
            public Nullable<double> IMBAA { get; set; }
            public Nullable<double> N_MOIN { get; set; }
        }
        public partial class Custom3_PRICE_ELAMIE
        {
            public int PEPID { get; set; }
            public float TF1 { get; set; }
            public float TF2 { get; set; }
        }
        public partial class Custom_INVO_LST
        {
            public string CODE { get; set; }
        }
        public partial class Custom_STUF_DEF
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
            public double? RADAH { get; set; }
        }
        public partial class Custom_INVO_STUF
        {
            public string CODE { get; set; }
            public Nullable<double> TKHN { get; set; }
            public double MEGHk { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public Nullable<double> N_KOL { get; set; }
            public Nullable<double> N_MOIN { get; set; }
            public Nullable<double> IMBAA { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public Nullable<bool> CMBAA { get; set; }
        }
        public partial class Custom_LETSGO
        {
            //FORMNAME,USERCO,RUN,SEE,INP,UPD,DEL
            public string FORMNAME { get; set; }
            public int USERCO { get; set; }
            public Nullable<bool> RUN { get; set; }
            public Nullable<bool> SEE { get; set; }
            public Nullable<bool> INP { get; set; }
            public Nullable<bool> UPD { get; set; }
            public Nullable<bool> DEL { get; set; }
        }
        public partial class Custom_StuFSK
        {
            public Nullable<double> MIN_M { get; set; }
        }
        public partial class Custom_modat
        {
            public int PPID { get; set; }
            public int MODAT { get; set; }
        }
        public class QRE_BAZ_0
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
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
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string? USER_NAME { get; set; }
        }
        public class QRE_BAZ_1
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string? NAME { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public double? ANBARF { get; set; }
        }
        public class QRE_BAZ_2
        {
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public string? COM { get; set; }
            public string? NAM { get; set; }
            public string? NAMES { get; set; }
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public double? SMAB { get; set; }
        }

        /// <summary>
        /// ردیف قلم «حواله خروج مواد» در حالت عادی (Baseknow.FINALS = false) — همان ستون‌های
        /// <see cref="QRE_BAZ_2"/> به‌علاوه SHEETNO (شماره برگه) تا بتوان ردیف‌های همه‌ی برگه‌ها را
        /// با یک کوئری خواند و در حافظه گروه‌بندی کرد.
        /// <para>
        /// نام SHEETNO عمداً با NUMBER فرق دارد: در این کوئری NUMBER همان HEAD_MANF.NUMBER
        /// (کد حساب معین فرمول ساخت) است، نه شماره برگه.
        /// </para>
        /// </summary>
        public class KhorugMavadLineRow
        {
            public double? SHEETNO { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public string? CODE { get; set; }
            public int? ANBAR { get; set; }
            public string? COM { get; set; }
            public string? NAM { get; set; }
            public int? N_KOL { get; set; }
            public int? NUMBER { get; set; }
            public int? TNUMBER { get; set; }
            public double? SMAB { get; set; }
        }

        /// <summary>
        /// ردیف قلم «حواله خروج مواد» در حالت نهایی‌شده (Baseknow.FINALS = true) — همان ستون‌های
        /// <see cref="QRE_BAZ_3"/> به‌علاوه SHEETNO.
        /// </summary>
        public class KhorugMavadFinalLineRow
        {
            public double? SHEETNO { get; set; }
            public double? MABL_K { get; set; }
            public double? MEGHk { get; set; }
            public int? ANBAR { get; set; }
            public string? CODE { get; set; }
            public double? SMAB { get; set; }
        }

        /// <summary>
        /// اختلاف بدهکار و بستانکار یک سند، برای مرحله‌ی «کسر دهم ریال» در سند خروج مواد.
        /// </summary>
        public class KhorugMavadBalanceRow
        {
            public double? N_S { get; set; }
            public double? DIFF { get; set; }
        }

        /// <summary>
        /// ردیف قلم «حواله خروج ساير» — همان ستون‌های <see cref="QRE_BAZ_5"/> به‌علاوه SHEETNO،
        /// تا بتوان اقلام همه‌ی برگه‌ها را با یک کوئری خواند و در حافظه گروه‌بندی کرد.
        /// </summary>
        public class KhorugSayerLineRow : QRE_BAZ_5
        {
            public double? SHEETNO { get; set; }
        }

        /// <summary>
        /// ردیف کالای فاکتور به‌همراه شماره فاکتور (برای پیش‌خوانی دسته‌ای اقلام چند فاکتور).
        /// </summary>
        public class QRE12_WITH_NUM : QRE12
        {
            public double? NUMBER { get; set; }
        }

        /// <summary>
        /// یک ردیف آماده‌ی درج در DEED_DTL، برای مسیرهایی که ردیف‌ها را اول در حافظه می‌سازند
        /// و در پایان با <see cref="BulkInsertDeedDtl"/> یکجا می‌نویسند.
        /// </summary>
        public class DEED_DTL_MODEL
        {
            public double N_S { get; set; }
            public int HES_K { get; set; }
            public int HES_M { get; set; }
            public int HES_T { get; set; }
            public int? HES_T2 { get; set; }
            public int? HES_T3 { get; set; }
            public int? HES_T4 { get; set; }
            public string? HES { get; set; }
            public string? SHARH { get; set; }
            public double BED { get; set; }
            public double BES { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public double? ARZD { get; set; }
        }
        #endregion

        /// <summary>
        /// Safely convert a string value to double. If conversion fails, returns 0.
        /// </summary>
        /// <param name="value">String to convert.</param>
        /// <returns>Parsed double or 0 when parsing is not possible.</returns>
        private static double SafeToDouble(string? value)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0d;
        }

        /// <summary>
        /// تبدیل یک کد متنی (کد کالا، کد فرمول ساخت، ...) به عددی که بتواند در ستون‌های
        /// HES_K / HES_M / HES_T جدول DEED_DTL بنشیند.
        ///
        /// <para>
        /// ⚠️ چرا این تابع لازم است و چرا نباید از <see cref="SafeToDouble"/> استفاده کرد:
        /// SafeToDouble برای ورودی خالی یا غیرعددی «صفر» برمی‌گرداند. اگر آن صفر مستقیم به
        /// شماره‌ی معین/تفصیلی تبدیل شود، سند به حسابی مثل «۷۷۱-۰-۱۲۳» می‌خورد؛ و چون
        /// پیش‌سازِ دسته‌ای حساب‌ها همان حساب را هم می‌سازد، قید FK_DEED_DTL_TDETA_HES دیگر
        /// جلویش را نمی‌گیرد و مبلغ واقعی بی‌سروصدا روی یک حساب بی‌معنی می‌نشیند.
        /// پس اینجا «نتوانستن» صریحاً گزارش می‌شود تا صداکننده تصمیم بگیرد.
        /// </para>
        ///
        /// <para>
        /// بازه هم بررسی می‌شود: ستون‌های HES_* از نوع int هستند، پس کدی خارج از محدوده‌ی int
        /// نباید تا مرحله‌ی Convert.ToInt32 برود (که OverflowException می‌داد و کل تسک را می‌خواباند).
        /// </para>
        /// </summary>
        private static bool TryGetAccountCode(string? value, out long result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            if (double.IsNaN(parsed) || parsed < int.MinValue || parsed > int.MaxValue)
            {
                return false;
            }

            result = (long)parsed;
            return true;
        }
        //______________

        public static void ExecuteWithPreferredLoop(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body)
        {
            if (Generaly.UseParallelProcessing)
            {
                Parallel.For(fromInclusive, toExclusive, parallelOptions, body);
            }
            else
            {
                for (int i = fromInclusive; i < toExclusive; i++)
                {
                    body(i);
                }
            }
        }

        public static ParallelOptions BuildDbAwareParallelOptions(int itemCount, bool useSmartThrottling = false)
        {
            useSmartThrottling = useSmartThrottling || UseSmartThrottlingByDefault;

            var maxPoolSize = GetConnectionPoolCeiling();
            int maxDegree;

            if (useSmartThrottling)
            {
                // حالت محافظه‌کارانه: هر Iteration یک Connection لحظه‌ای می‌گیرد؛ پس فاصله امن از سقف Pool نگه می‌داریم.
                // 25% از Pool (حداقل 4 و حداکثر 16) تا سایر ماژول‌ها هم Connection داشته باشند.
                maxDegree = Math.Clamp(maxPoolSize / 4, 4, 16);
            }
            else
            {
                // حالت پیش‌فرض (پرسرعت):
                // قبلاً اینجا MaxDegreeOfParallelism = -1 برگردانده می‌شد. «نامحدود» در عمل سریع‌تر نیست؛
                // چون بدنه‌ی حلقه I/O مسدودکننده‌ی SQL است، Parallel.For به تعداد Thread های آزاد ThreadPool محدود می‌ماند
                // و ThreadPool فقط حدود یک Thread در ثانیه به آن اضافه می‌کند (Thread Injection). نتیجه این است که
                // موازی‌سازی واقعی مدت زیادی روی Environment.ProcessorCount گیر می‌ماند و کاربر شتابی حس نمی‌کند.
                // پس یک درجه‌ی موازی‌سازی صریح می‌دهیم و ThreadPool را از قبل گرم می‌کنیم.
                maxDegree = Math.Clamp(Environment.ProcessorCount * 2, 6, 24);

                // مهم: بخش‌های C1 تا C11 با هم و به‌صورت همزمان اجرا می‌شوند (MainWindow → Task.WhenAll)،
                // و هر Iteration یک Connection لحظه‌ای می‌گیرد. اگر هر حلقه سهم بزرگی بردارد، مجموع
                // Connection ها از سقف Pool (پیش‌فرض ADO.NET یعنی 100) رد می‌شود و Timeout استخر اتصال
                // می‌دهد که کل آن بخش را با خطا متوقف می‌کند.
                // سهم هر حلقه یک‌شانزدهم Pool است: با ۱۱ بخش همزمان حدود ۷۰ از ۱۰۰ مصرف می‌شود و
                // برای کوئری‌های تکی سایر بخش‌ها هم حاشیه می‌ماند.
                // اگر Max Pool Size در رشته اتصال بالاتر تنظیم شود، این سقف هم خودکار بالا می‌رود.
                maxDegree = Math.Min(maxDegree, Math.Max(4, maxPoolSize / 16));
            }

            maxDegree = Math.Max(1, Math.Min(maxDegree, Math.Max(1, itemCount)));

            EnsureThreadPoolCapacity(maxDegree);

            return new ParallelOptions { MaxDegreeOfParallelism = maxDegree };
        }

        /// <summary>
        /// سقف Connection Pool رشته اتصال جاری (اگر قابل خواندن نبود مقدار پیش‌فرض ADO.NET یعنی 100).
        /// </summary>
        private static int GetConnectionPoolCeiling()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    var builder = new SqlConnectionStringBuilder(CL_CCNNMANAGER.CONNECTION_STR);
                    if (builder.MaxPoolSize > 0)
                    {
                        return builder.MaxPoolSize;
                    }
                }
            }
            catch
            {
                // در صورت خطای parse رشته اتصال، با مقدار پیش‌فرض ادامه می‌دهیم.
            }

            return 100;
        }

        /// <summary>
        /// حداقل Thread های ThreadPool را بالا می‌برد تا Parallel.For بلافاصله به درجه‌ی موازی‌سازی هدف برسد
        /// و منتظر Thread Injection تدریجی (تقریباً یک Thread در ثانیه) نماند.
        /// </summary>
        private static void EnsureThreadPoolCapacity(int desiredDegree)
        {
            try
            {
                ThreadPool.GetMinThreads(out var minWorker, out var minIo);
                var target = Math.Min(Math.Max(minWorker, desiredDegree + Environment.ProcessorCount), 512);
                if (target > minWorker)
                {
                    ThreadPool.SetMinThreads(target, Math.Max(minIo, target));
                }
            }
            catch
            {
                // اگر تنظیم ThreadPool ممکن نبود، صرفاً کندتر گرم می‌شود و مشکل عملکردی جدی ایجاد نمی‌کند.
            }
        }

        /// <summary>
        /// اجرای یک عملیات SQL با تلاش مجدد در صورت Deadlock (خطای 1205).
        /// وقتی حلقه واقعاً موازی اجرا شود احتمال Deadlock بین Thread ها بالا می‌رود،
        /// و <see cref="CL_ConcurrencyManager.ExecuteSqlCommand"/> خودش Retry ندارد.
        /// </summary>
        public static void ExecuteWithDeadlockRetry(Action action, int maxRetries = 4)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxRetries)
                {
                    Thread.Sleep(50 * (attempt + 1));
                }
            }
        }

        /// <summary>
        /// عدد را برای درج در متن SQL قالب‌بندی می‌کند (بدون نماد علمی و بدون وابستگی به Culture).
        /// </summary>
        private static string SqlNum(double? value)
        {
            return value.HasValue ? value.Value.ToString("0.##########", CultureInfo.InvariantCulture) : "NULL";
        }

        /// <summary>
        /// نام کوتاه <see cref="SqlNum"/>؛ فقط برای خوانا ماندن رشته‌های SQL طولانی.
        /// </summary>
        private static string N(double? value) => SqlNum(value);

        /// <summary>
        /// Escape کردن کوتیشن برای درج متن در SQL literal.
        /// </summary>
        private static string SqlText(string? value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        /// <summary>
        /// گزارش پیشرفت برای حلقه‌های موازی.
        /// Dispatcher.Invoke مسدودکننده است؛ اگر به‌ازای هر رکورد صدا زده شود، همه‌ی Thread های حلقه
        /// پشت تک‌Thread رابط کاربری صف می‌کشند و موازی‌سازی عملاً از بین می‌رود.
        /// اینجا شمارنده اتمیک است و UI فقط حدود ۱۰۰ بار و با BeginInvoke غیرمسدودکننده به‌روز می‌شود.
        /// </summary>
        public sealed class ThrottledProgressReporter
        {
            private readonly int _total;
            private readonly int _reportInterval;
            private readonly System.Windows.Threading.Dispatcher? _dispatcher;
            private readonly Action<double>? _applyToUi;
            private int _done;

            public ThrottledProgressReporter(int total, System.Windows.Threading.Dispatcher? dispatcher, Action<double>? applyToUi)
            {
                _total = Math.Max(1, total);
                _reportInterval = Math.Max(1, _total / 100);
                _dispatcher = dispatcher;
                _applyToUi = applyToUi;
            }

            public void ReportOne()
            {
                var done = Interlocked.Increment(ref _done);
                if (done % _reportInterval == 0)
                {
                    Report(done * 100.0 / _total);
                }
            }

            /// <summary>نمایش نهایی ۱۰۰٪ پس از پایان حلقه.</summary>
            public void Complete() => Report(100.0);

            private void Report(double value)
            {
                var dispatcher = _dispatcher;
                var applyToUi = _applyToUi;
                if (dispatcher == null || applyToUi == null)
                {
                    return;
                }

                try
                {
                    dispatcher.BeginInvoke(new Action(() => applyToUi(value)), System.Windows.Threading.DispatcherPriority.Background);
                }
                catch
                {
                    // اگر پنجره بسته شده باشد نیازی به گزارش نیست.
                }
            }
        }

        public static string DECODEUN(string cody)
        {
            byte[] RawCoded = Encoding.GetEncoding(1256).GetBytes(cody);// ی 237

            var Parsy = Encoding.GetEncoding(1256);
            for (byte i = 0; i < RawCoded.Count(); i++)
            {
                RawCoded[i] = (byte)(RawCoded[i] + 20);
            }
            var result = Parsy.GetString(RawCoded);
            cody = result;
            return cody;
        }
        public static bool IsNumeric(string input)
        {
            // Step 1: Handle null, empty, or whitespace strings
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // Step 2: Trim any leading or trailing whitespace
            input = input.Trim();

            // Step 4: Try parsing the input as a double (to cover integers, decimals, and scientific notation)
            if (double.TryParse(input, out double result))
            {
                // Optional: Further checks for overflow or underflow can be done here
                if (double.IsInfinity(result) || double.IsNaN(result))
                {
                    return false; // Not valid if infinity or NaN
                }
                return true;
            }

            // Step 5: If the input cannot be parsed as a number, return false
            return false;
        }
        public static object GETTAF3(string SHES, ref double? KOL, ref double? MOIN, ref double? taf, ref double? TAF2, ref double? taf3, ref double? taf4)
        {
            byte i1, I2, I3, I4, I5, I6, I7, I8, j, K;
            i1 = 0;
            I2 = 0;
            I3 = 0;
            I4 = 0;
            I5 = 0;
            I6 = 0;
            I7 = 0;
            I8 = 0;
            j = 1;
            K = 1;
            if (Strings.Len(SHES) > 4)
            {
                var loopTo = (byte)Strings.Len(SHES);
                for (j = 1; j <= loopTo; j++)
                {
                    if (Strings.Mid(SHES, j, 1) == "-")
                    {
                        switch (K)
                        {
                            case 1:
                                {
                                    i1 = j;
                                    break;
                                }

                            case 2:
                                {
                                    I2 = j;
                                    break;
                                }

                            case 3:
                                {
                                    I3 = (byte)(j + 1);
                                    break;
                                }

                            case 4:
                                {
                                    I4 = (byte)(j + 1);
                                    break;
                                }

                            case 5:
                                {
                                    I5 = (byte)(j + 1);
                                    break;
                                }

                            case 6:
                                {
                                    I6 = (byte)(j + 1);
                                    break;
                                }

                            case 7:
                                {
                                    I7 = (byte)(j + 1);
                                    break;
                                }

                            case 8:
                                {
                                    I8 = (byte)(j + 1);
                                    break;
                                }
                        }

                        K = (byte)(K + 1);
                    }
                }
            }

            KOL = default;
            MOIN = default;
            taf = default;
            TAF2 = default;
            taf3 = default;
            taf4 = default;
            if (i1 == 0)
            {
                if (IsNumeric(SHES))
                {
                    KOL = Convert.ToDouble(SHES);
                }
                return default;
            }
            else
            {
                if (IsNumeric(Strings.Mid(SHES, 1, i1 - 1)))
                {
                    KOL = Convert.ToDouble(Strings.Mid(SHES, 1, i1 - 1));
                }
            }

            if (I2 == 0)
            {
                if (IsNumeric(Strings.Mid(SHES, i1 + 1, Strings.Len(SHES) - i1)))
                {
                    MOIN = Convert.ToDouble(Strings.Mid(SHES, i1 + 1, Strings.Len(SHES) - i1));
                }
                return default;
            }
            else
            {
                if (IsNumeric(Strings.Mid(SHES, i1 + 1, I2 - i1 - 1)))
                {
                    MOIN = Convert.ToDouble(Strings.Mid(SHES, i1 + 1, I2 - i1 - 1));
                }
            }

            if (I3 == 0)
            {
                if (IsNumeric(Strings.Mid(SHES, I2 + 1, Strings.Len(SHES) - I2)))
                {
                    taf = Convert.ToDouble(Strings.Mid(SHES, I2 + 1, Strings.Len(SHES) - I2));
                }
                return default;
            }
            else
            {
                if (IsNumeric(Strings.Mid(SHES, I2 + 1, I3 - I2 - 2)))
                {
                    taf = Convert.ToDouble(Strings.Mid(SHES, I2 + 1, I3 - I2 - 2));
                }
            }

            if (I4 == 0)
            {
                if (IsNumeric(Strings.Mid(SHES, I3, Strings.Len(SHES) - I3 + 1)))
                {
                    TAF2 = Convert.ToDouble(Strings.Mid(SHES, I3, Strings.Len(SHES) - I3 + 1));
                }

            }
            else
            {
                if (IsNumeric(Strings.Mid(SHES, I3, I4 - I3 - 1)))
                {
                    TAF2 = Convert.ToDouble(Strings.Mid(SHES, I3, I4 - I3 - 1));
                }
                if (I5 == 0)
                {
                    if (IsNumeric(Strings.Mid(SHES, I4, Strings.Len(SHES) - I4 + 1)))
                    {
                        taf3 = Convert.ToDouble(Strings.Mid(SHES, I4, Strings.Len(SHES) - I4 + 1));
                    }
                }
                else
                {
                    if (IsNumeric(Strings.Mid(SHES, I4, I5 - I4 - 1)))
                    {
                        taf3 = Convert.ToDouble(Strings.Mid(SHES, I4, I5 - I4 - 1));
                    }
                    if (I6 == 0)
                    {
                        if (IsNumeric(Strings.Mid(SHES, I5, Strings.Len(SHES) - I5 + 1)))
                        {
                            taf4 = Convert.ToDouble(Strings.Mid(SHES, I5, Strings.Len(SHES) - I5 + 1));
                        }
                    }
                    else
                    {
                        if (IsNumeric(Strings.Mid(SHES, I5, I6 - I5 - 1)))
                        {
                            taf4 = Convert.ToDouble(Strings.Mid(SHES, I5, I6 - I5 - 1));
                        }
                    }
                }
            }

            return default;
        }

        public static string GETKALANAME(double CODE)
        {
            if (LookupCacheEnabled && _kalaNameCache.TryGetValue(CODE, out var cachedKala))
            {
                return cachedKala;
            }

            string returnValue = "";
            var RRST = dbms.DoGetDataSQL<Custom_STUF_DEF>("SELECT CODE,NAME FROM STUF_DEF WHERE (CODE = " + Convert.ToString(CODE) + ")").FirstOrDefault();
            if (RRST != null)
            {
                if (string.IsNullOrEmpty(RRST.NAME))
                {
                    returnValue = " ";
                }
                else
                {
                    returnValue = RRST.NAME;
                }
            }
            else
            {
                returnValue = " ";
            }

            if (LookupCacheEnabled)
            {
                _kalaNameCache[CODE] = returnValue;
            }

            return returnValue;
        }

        public static string GETTAFNAME(string HES)
        {
            // نام حساب در طول یک اجرای بازسازی تغییر نمی‌کند، ولی این تابع
            // چند بار برای هر فاکتور (در ساخت شرح‌ها) صدا زده می‌شود.
            if (LookupCacheEnabled && HES != null && _tafNameCache.TryGetValue(HES, out var cachedName))
            {
                return cachedName;
            }

            string returnValue = "";
            var RRST = dbms.DoGetDataSQL<string>("SELECT     NAME FROM dbo.CUST_HESAB WHERE     (hes = N'" + HES + "')").ToList();
            if (RRST.Count > 0)
            {
                //if (ISNULL(RRST.Fields(0)))
                if (ReferenceEquals(RRST.First(), null))
                {
                    returnValue = " ";
                }
                else
                {
                    //returnValue = Convert.ToString(RRST.Fields(0));
                    returnValue = Convert.ToString(RRST.First());
                }
            }
            else
            {
                returnValue = " ";
            }

            // فقط پاسخ قطعی کش می‌شود. اگر حساب پیدا نشد (" ")، کش نمی‌کنیم؛
            // چون همین‌جا در ادامه CREATHES ممکن است همان حساب را بسازد و
            // پاسخ کهنه‌ی «پیدا نشد» تا پایان اجرا باقی بماند.
            if (LookupCacheEnabled && HES != null && returnValue != " ")
            {
                _tafNameCache[HES] = returnValue;
            }

            return returnValue;
        }

        public static string GETUSERNAME(int? US)
        {
            if (string.IsNullOrEmpty(US.ToStringNullSafe()))
            {
                return "NULL";
            }
            else
            {
                string GETUSERNAMERet = default;
                var rst = dbms.DoGetDataSQL<string>("SELECT     SAL_NAME FROM dbo.SALA_DTL WHERE     (IDD= " + US + ")").FirstOrDefault();
                if (!ReferenceEquals(rst, null))
                {
                    GETUSERNAMERet = DECODEUN(rst);
                }
            }
            return "NULL";
        }

        /// <summary>
        /// بررسی مغایرت بین فاکتور فروش و سند حسابداری
        /// تغییرات: ایجاد ویوهای جدید با پسوند _New برای عدم تداخل با برنامه Access
        /// اصلاح: استفاده از مبلغ خالص (MABL_K) برای رفع خطای محاسباتی
        /// </summary>
        public static bool Frisok(string HES, bool InternalCalling = true)
        {
            bool FrisokRet = default;
            int i;

            if (string.IsNullOrEmpty(HES)) return true;

            string safeHes = HES.Replace("'", "''");

            try
            {
                string optionChar = "";
                if (!string.IsNullOrEmpty(Baseknow.OPTIONSS) && Baseknow.OPTIONSS.Length >= 64)
                {
                    optionChar = Baseknow.OPTIONSS.Substring(63, 1);
                }

                if (optionChar == "5")
                {
                    string viewNameSnd = "frsnd_New" + Baseknow.USERCOD;
                    string viewNameInv = "frinv_New" + Baseknow.USERCOD;

                    // 1. حذف ویوهای قبلی
                    dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{viewNameSnd}') DROP VIEW {viewNameSnd}");
                    dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{viewNameInv}') DROP VIEW {viewNameInv}");

                    // 2. ساخت ویوی سند (اصلاح شده برای چک کردن تراز سند)
                    // تغییر مهم: اضافه کردن شرط HAVING برای اطمینان از تراز بودن کل سند
                    // اگر سند تراز نباشد (مثل مثالی که زدید)، این کوئری رکوردی برنمی‌گرداند و باعث وقوع خطای مغایرت می‌شود.
                    string sqlCreateSnd = $"CREATE VIEW {viewNameSnd} AS " +
                                          $"SELECT SUM(T1.BED - T1.BES) AS Expr2, T1.NUMBER " +
                                          $"FROM dbo.DEED_DTL AS T1 " +
                                          $"WHERE (T1.TAG = 13) AND (T1.HES = N'{safeHes}') " +
                                          $"GROUP BY T1.NUMBER " +
                                          $"HAVING ROUND((SELECT SUM(T2.BED - T2.BES) FROM dbo.DEED_DTL T2 WHERE T2.NUMBER = T1.NUMBER AND T2.TAG = 13), 0) = 0";

                    dbms.DoExecuteSQL(sqlCreateSnd);

                    // 3. ساخت ویوی فاکتور (با فرمول صحیح خالص + مالیات - تخفیف)
                    string sqlCreateInv = $"CREATE VIEW {viewNameInv} AS " +
                                          $"SELECT H.NUMBER, " +
                                          $"( " +
                                          // محاسبه بدهکاری واقعی مشتری طبق لاجیک تولید سند
                                          $"  (ISNULL(MAX(DTL.NetTotal), 0) + MAX(H.MABL_HAZ) + MAX(H.MBAA) - MAX(H.TAKHFIF)) " +
                                          $"  - " +
                                          // کسر پرداختی‌ها
                                          $"  (MAX(H.M_NAGHD) + MAX(H.MABL_VAR) + MAX(H.MABL_HAV) + ISNULL(MAX(CHK.mabch), 0)) " +
                                          $") AS Expr1 " +
                                          $"FROM dbo.HEAD_LST H " +
                                          $"INNER JOIN ( " +
                                          $"    SELECT NUMBER, TAG, SUM(MABL_K) as NetTotal " +
                                          $"    FROM dbo.INVO_LST " +
                                          $"    GROUP BY NUMBER, TAG " +
                                          $") DTL ON H.NUMBER = DTL.NUMBER AND H.TAG = DTL.TAG + 11 " +
                                          $"LEFT OUTER JOIN dbo.jamchkfact CHK ON DTL.NUMBER = CHK.NUMBER " +
                                          $"WHERE (H.TAG = 13) AND (H.CUST_NO = N'{safeHes}') " +
                                          $"GROUP BY H.NUMBER " +
                                          $"HAVING ( " +
                                          $"  (ISNULL(MAX(DTL.NetTotal), 0) + MAX(H.MABL_HAZ) + MAX(H.MBAA) - MAX(H.TAKHFIF)) " +
                                          $"  - " +
                                          $"  (MAX(H.M_NAGHD) + MAX(H.MABL_VAR) + MAX(H.MABL_HAV) + ISNULL(MAX(CHK.mabch), 0)) " +
                                          $") <> 0";

                    dbms.DoExecuteSQL(sqlCreateInv);

                    // 4. مقایسه
                    // اگر سند تراز نباشد، Expr2 نال می‌شود و شرط (S.Expr2 IS NULL) برقرار شده و خطا می‌دهد
                    string sqlCheck = $"SELECT I.NUMBER, S.Expr2 " +
                                      $"FROM {viewNameSnd} S " +
                                      $"RIGHT OUTER JOIN {viewNameInv} I ON S.NUMBER = I.NUMBER " +
                                      $"WHERE (I.Expr1 > 0) AND ((S.Expr2 IS NULL) OR (ROUND(I.Expr1 - S.Expr2, 0) <> 0))";

                    var rst = dbms.DoGetDataSQL<Q9>(sqlCheck).ToList();

                    if (rst.Count > 0)
                    {
                        string _Pathfile_ = @"C:\CORRECT\errorfr.txt";
                        string directory = Path.GetDirectoryName(_Pathfile_);

                        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                        using (StreamWriter writer = new StreamWriter(_Pathfile_, true))
                        {
                            for (i = 0; i < rst.Count; i++)
                            {
                                GENSANADFROOSH(rst[i].NUMBER, Convert.ToInt64(rst[i].NUMBER), InternalCalling);
                                writer.WriteLine("شماره حواله مغایرت دار (عدم تراز یا اختلاف مبلغ) که سعی شد سند مجددا آن بازسازی شود : " + rst[i].NUMBER.ToStringNullSafe());
                            }
                        }
                        FrisokRet = false;
                    }
                    else
                    {
                        FrisokRet = true;
                    }
                }
                else
                {
                    FrisokRet = true;
                }
            }
            catch (Exception)
            {
                FrisokRet = false;
            }

            return FrisokRet;
        }

        private static Object LOCKER = new Object();
        public static long Createsanad(long DATE_S, string SHARH_S, int GHATEI, int NO_S, int OKF, string USER_NAME)
        {
            //lock (LOCKER)
            //{
            long CreatesanadRet = default;
            long max_ns;
            Double BG;

            object rss = null;
            using (IDbConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                {
                    #region SERIALZBLE_SAFE_QUERY
                    //Fake Query for Lock Table
                    db.Execute("UPDATE TOP(1) DEED_HED SET ANBAR = ANBAR", null, transaction);
                    //Fake Query for Lock Table

                    // شماره بایگانی هم باید داخل همین تراکنش و پس از گرفتن قفل خوانده شود.
                    // قبلاً بیرون از تراکنش و روی Connection دیگری خوانده می‌شد، پس دو فراخوانی
                    // همزمان (مثلاً سند فروش و سند خزانه) می‌توانستند BAYEG یکسان بگیرند.
                    object rss2 = db.Query<double?>("SELECT Max(DEED_HED.BAYEG) AS MaxOfBG FROM DEED_HED", null, transaction).FirstOrDefault();
                    if (IsNull(rss2))
                    { BG = 100000000; }
                    else
                    { BG = (double)rss2 + 1; }

                    //rss = db.Execute("INSERT INTO DEED_HED (N_S,DATE_S,SHARH_S,GHATEI,NO_S,OKF,USER_NAME) VALUES (0,0,0,0,0,0,0)", null, transaction);
                    rss = db.Query<double?>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED", null, transaction).FirstOrDefault();
                    //if (IsNull(rss.Fields(0)))
                    if (IsNull(rss))
                    {
                        rss = db.Execute("WAITFOR DELAY '00:00:00.500'", null, transaction);
                        rss = db.Execute("SELECT count(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED", null, transaction);
                        if (IsNull(rss))
                        {
                            if (rss is 0)
                            {
                                max_ns = 2L;
                            }
                            else
                            {
                                max_ns = 0L;
                            }
                        }
                        else
                        {
                            max_ns = Convert.ToInt64(rss) + 1;
                        }
                    }
                    else
                    {
                        max_ns = Convert.ToInt64(rss) + 1;
                    }
                    ;
                    //Forms["BUN"].Form.Refresh();
                    CreatesanadRet = max_ns;
                    #endregion
                    CreatesanadRet = max_ns;


                    rss = db.Execute($"INSERT INTO DEED_HED (N_S,DATE_S,SHARH_S,GHATEI,NO_S,OKF,USER_NAME,CRT,uid,BAYEG) VALUES ({max_ns} ,{DATE_S},'{SHARH_S}',{GHATEI},{NO_S},{OKF},'{USER_NAME}',GETDATE(),{Baseknow.USERCOD},{BG})", null, transaction);
                    transaction.Commit();
                    db?.Close();
                }
            }
            return CreatesanadRet;
            //}
            // *************************************************************
        }

        /// <summary>
        /// مشخصات یک هدر سند که قرار است ساخته شود.
        /// </summary>
        public sealed class SanadHeaderRequest
        {
            public long DATE_S { get; set; }
            public string? SHARH_S { get; set; }
            public int GHATEI { get; set; }
            public int NO_S { get; set; }
            public int OKF { get; set; }
            public string? USER_NAME { get; set; }
        }

        /// <summary>
        /// رزرو دسته‌ای شماره سند و درج هدرها.
        ///
        /// <para>
        /// چرا لازم است: <see cref="Createsanad"/> برای هر سند یک تراکنش با
        /// <see cref="IsolationLevel.Serializable"/> باز می‌کند و با کوئری عمدی
        /// «UPDATE TOP(1) DEED_HED SET ANBAR = ANBAR» روی همان یک ردیف قفل انحصاری می‌گیرد؛
        /// یعنی آن ردیف نقش یک Mutex سراسری را بازی می‌کند. به‌علاوه SELECT MAX(N_S)
        /// در سطح Serializable روی انتهای ایندکس N_S قفل بازه‌ای می‌گیرد.
        /// هر دو قفل تا Commit نگه داشته می‌شوند، پس اگر این متد داخل یک حلقه‌ی Parallel
        /// صدا زده شود همه‌ی Thread ها پشت همان Mutex صف می‌کشند و حلقه عملاً سریال اجرا می‌شود.
        /// اینجا همان قفل‌ها گرفته می‌شوند، ولی یک بار برای کل دسته به‌جای یک بار برای هر سند.
        /// </para>
        /// </summary>
        /// <returns>شماره سندهای رزرو شده، به همان ترتیب ورودی.</returns>
        public static List<double> ReserveSanadNumbersBatch(IReadOnlyList<SanadHeaderRequest> headers)
        {
            var reserved = new List<double>(headers?.Count ?? 0);
            if (headers == null || headers.Count == 0)
            {
                return reserved;
            }

            // قفل جدول DEED_HED نباید برای مدت طولانی نگه داشته شود (کاربران دیگر هم سند می‌سازند)،
            // پس رزرو در دسته‌های حداکثر ۵۰۰۰تایی انجام می‌شود؛ هر دسته یک تراکنش کوتاه.
            // بخش‌های C1 تا C11 همزمان اجرا می‌شوند و همه‌شان (از طریق Createsanad) روی همین
            // جدول DEED_HED تراکنش Serializable می‌گیرند؛ پس Deadlock ممکن است و Retry می‌کنیم.
            const int reservationBatchSize = 5000;
            for (int start = 0; start < headers.Count; start += reservationBatchSize)
            {
                var chunkStart = start;
                var chunkLength = Math.Min(reservationBatchSize, headers.Count - start);

                List<double> chunk = null;
                ExecuteWithDeadlockRetry(() => chunk = ReserveSanadNumbersChunk(headers, chunkStart, chunkLength));
                reserved.AddRange(chunk);
            }

            return reserved;
        }

        private static List<double> ReserveSanadNumbersChunk(IReadOnlyList<SanadHeaderRequest> headers, int start, int length)
        {
            var reserved = new List<double>(length);

            using (IDbConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                {
                    // دقیقاً همان قفل عمدی که در Createsanad وجود دارد و به همان ترتیب گرفته می‌شود
                    // (اول این ردیف، بعد انتهای ایندکس N_S). یکسان بودنِ ترتیب مهم است: باعث می‌شود
                    // این متد و Createsanad فقط پشت هم صف بکشند و با هم Deadlock نکنند.
                    // نتیجه: حتی اگر بخش‌های دیگر یا نسخه دیگری از برنامه همزمان سند بسازند،
                    // شماره تکراری تولید نمی‌شود. تفاوت: یک بار برای کل دسته، نه به‌ازای هر سند.
                    db.Execute("UPDATE TOP(1) DEED_HED SET ANBAR = ANBAR", null, transaction, commandTimeout: 3600);

                    var maxNs = db.Query<double?>("SELECT MAX(N_S) FROM DEED_HED", null, transaction).FirstOrDefault();
                    var maxBg = db.Query<double?>("SELECT MAX(BAYEG) FROM DEED_HED", null, transaction).FirstOrDefault();

                    var nextNs = (maxNs.HasValue ? Convert.ToInt64(maxNs.Value) : 0L) + 1L;
                    var nextBg = maxBg.HasValue ? Convert.ToInt64(maxBg.Value) + 1L : 100000000L;

                    var uid = Baseknow.USERCOD.HasValue
                        ? Baseknow.USERCOD.Value.ToString(CultureInfo.InvariantCulture)
                        : "NULL";

                    var values = new List<string>(length);
                    for (int i = 0; i < length; i++)
                    {
                        var header = headers[start + i];
                        var ns = nextNs + i;
                        var bg = nextBg + i;

                        reserved.Add(ns);
                        values.Add($"({ns},{header.DATE_S},N'{SqlText(header.SHARH_S)}',{header.GHATEI},{header.NO_S},{header.OKF},N'{SqlText(header.USER_NAME)}',GETDATE(),{uid},{bg})");
                    }

                    // سقف INSERT ... VALUES در SQL Server هزار ردیف است؛ محتاطانه 500تایی می‌فرستیم.
                    const int insertChunkSize = 500;
                    for (int offset = 0; offset < values.Count; offset += insertChunkSize)
                    {
                        var chunk = string.Join(",", values.Skip(offset).Take(insertChunkSize));
                        db.Execute(
                            "INSERT INTO DEED_HED (N_S,DATE_S,SHARH_S,GHATEI,NO_S,OKF,USER_NAME,CRT,uid,BAYEG) VALUES " + chunk,
                            null, transaction, commandTimeout: 3600);
                    }

                    transaction.Commit();
                }

                db?.Close();
            }

            return reserved;
        }

        public static (double?, bool) GENSANADFROOSH(object fnum, long TNUM, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }
            LogWriter.WriteLog("شروع بازسازی سند فروش");
            //var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT N_S, DATE_S, SHARH_S, NO_S, ANBAR, N_FACTOR, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF FROM dbo.DEED_HED").ToList();
            var HFRST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>($"SELECT  * FROM dbo.HEAD_LST WHERE     (NUMBER BETWEEN {fnum} AND {TNUM}) AND (TAG = 13) ORDER BY NUMBER").ToList();

            // گزارش پیشرفت غیرمسدودکننده.
            // قبلاً برای هر فاکتور یک Dispatcher.Invoke همگام صدا زده می‌شد؛ چون فقط یک Thread
            // اجازه‌ی دسترسی به رابط کاربری دارد، همه‌ی Threadهای حلقه پشت آن صف می‌کشیدند و
            // موازی‌سازی عملاً از بین می‌رفت. ضمناً progress++ اتمیک نبود و عدد گم می‌کرد.
            var progressReporter = new ThrottledProgressReporter(
                HFRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    // Math.Max لازم است: گزارش‌ها با BeginInvoke صف می‌شوند و ترتیب اجرایشان
                    // تضمین‌شده نیست؛ بدون آن نوار پیشرفت گاهی به عقب می‌پرد.
                    auto_run.PRGR_C1.Value = Math.Max(auto_run.PRGR_C1.Value, value);
                    auto_run.LBL_C1.Content = $"{auto_run.PRGR_C1.Value:F2}%";
                    auto_run.UpdateOverallProgressBar();
                });

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observedThreads = new System.Collections.Concurrent.ConcurrentDictionary<int, byte>();

            // ───────────────────────────────────────────────────────────────────────────────
            // بهای تمام‌شده (مواد/دستمزد/سربار) فقط وقتی به کار می‌آید که کاراکتر ۶۶ گزینه‌ها
            // برابر "5" نباشد — همان شرطِ if پایین‌تر. اگر "5" باشد آن بلوک هرگز اجرا نمی‌شود
            // و MAVAD/DAST/SAR در هیچ جای دیگری استفاده نمی‌شوند.
            //
            // ولی سه فراخوانی GETSTANDARDPRICE_* «قبل» از آن شرط انجام می‌شد: برای هر قلم کالا
            // سه کوئری سنگین روی HEAD_MANF + DTL_MANF (هرکدام به‌علاوه یک GETLASTFR) زده می‌شد
            // و نتیجه‌اش دور ریخته می‌شد.
            //
            // این مقدار در طول اجرا ثابت است، پس یک بار حساب می‌شود.
            var sanatPriceNeeded = Strings.Mid(Baseknow.OPTIONSS, 66, 1) != "5";

            // ───────────────────────────────────────────────────────────────────────────────
            // پیش‌خواندن دو جمعِ هر فاکتور با «دو» کوئری، به‌جای دو کوئری برای «هر» فاکتور.
            // مقدارشان فقط به شماره فاکتور بستگی دارد، پس یکجا خواندنشان دقیقاً همان
            // نتیجه را می‌دهد. با هزاران فاکتور، هزاران رفت‌وبرگشت حذف می‌شود.
            // نبودِ کلید در Dictionary یعنی «جمعی وجود ندارد» که همان صفرِ کد قبلی است.
            // ───────────────────────────────────────────────────────────────────────────────
            var invoiceNumbers = HFRST.Where(r => r?.NUMBER != null).Select(r => r.NUMBER.Value).ToList();
            var jamfByInvoice = new Dictionary<double, double>();
            var jamchByInvoice = new Dictionary<double, double>();

            // ردیف‌های کالا و چک‌های هر فاکتور. بعد از این بلوک فقط خوانده می‌شوند،
            // پس Dictionary معمولی برای خواندن هم‌زمان از چند Thread امن است.
            var invoiceLines = new Dictionary<double, List<QRE12>>();
            var invoiceLinesWithAnbar = new Dictionary<double, List<QRE14>>();
            var invoiceCheques = new Dictionary<double, List<PAY_GETD>>();

            if (invoiceNumbers.Count > 0)
            {
                var minNum = SqlNum(invoiceNumbers.Min());
                var maxNum = SqlNum(invoiceNumbers.Max());

                foreach (var row in dbms.DoGetDataSQL<InvoiceSumRow>(
                    $"SELECT NUMBER, SUM(MABL_K) AS Total FROM dbo.INVO_LST " +
                    $"WHERE TAG = 2 AND NUMBER BETWEEN {minNum} AND {maxNum} GROUP BY NUMBER"))
                {
                    if (row?.NUMBER != null && row.Total != null)
                    {
                        jamfByInvoice[row.NUMBER.Value] = row.Total.Value;
                    }
                }

                foreach (var row in dbms.DoGetDataSQL<InvoiceSumRow>(
                    $"SELECT NUMBER, SUM(MABL) AS Total FROM dbo.PAY_GETD " +
                    $"WHERE TAG = 2 AND NUMBER BETWEEN {minNum} AND {maxNum} GROUP BY NUMBER"))
                {
                    if (row?.NUMBER != null && row.Total != null)
                    {
                        jamchByInvoice[row.NUMBER.Value] = row.Total.Value;
                    }
                }

                // ───────────────────────────────────────────────────────────────────────────
                // ردیف‌های کالای فاکتورها (jst_sec و jst_thr) هم یکجا خوانده می‌شوند.
                //
                // قبلاً برای «هر» فاکتور دو کوئری جداگانه روی INVO_LST + STUF_DEF زده می‌شد؛
                // آن دو کوئری ستون و JOIN کاملاً یکسان دارند و تنها تفاوتشان شرط ANBAR <> 0
                // است. یعنی نتیجه‌ی دومی همیشه زیرمجموعه‌ی اولی است و با یک بار خواندن،
                // هر دو ساخته می‌شوند.
                //
                // چرا امن است: بازسازی به INVO_LST و STUF_DEF نمی‌نویسد. تنها جایی که به
                // INVO_LST می‌نویسد SANADKHAD است که فقط N_KOL و N_MOIN را عوض می‌کند —
                // هیچ‌کدام از ستون‌های اینجا. نرخ میانگین (AVRAGE) هم در C0 نوشته می‌شود که
                // قبل از شروع این بخش تمام شده است.
                // ───────────────────────────────────────────────────────────────────────────
                var wantedInvoices = new HashSet<double>(invoiceNumbers);

                foreach (var row in dbms.DoGetDataSQL<InvoiceLineRow>(
                    $"SELECT L.NUMBER, L.MABL_K, L.MEGHk, L.CODE, L.ANBAR, S.NAME, L.AVRAGE " +
                    $"FROM dbo.INVO_LST AS L INNER JOIN dbo.STUF_DEF AS S ON S.CODE = L.CODE " +
                    $"WHERE L.TAG = 2 AND L.NUMBER BETWEEN {minNum} AND {maxNum}"))
                {
                    if (row?.NUMBER == null || !wantedInvoices.Contains(row.NUMBER.Value)) { continue; }

                    var key = row.NUMBER.Value;

                    if (!invoiceLines.TryGetValue(key, out var allLines))
                    {
                        allLines = new List<QRE12>();
                        invoiceLines[key] = allLines;
                    }
                    allLines.Add(new QRE12
                    {
                        MABL_K = row.MABL_K,
                        MEGHk = row.MEGHk,
                        CODE = row.CODE,
                        ANBAR = row.ANBAR,
                        NAME = row.NAME,
                        AVRAGE = row.AVRAGE ?? 0d
                    });

                    // همان شرط ANBAR <> 0 کوئری دوم. NULL هم مثل قبل رد می‌شود،
                    // چون در SQL شرط ANBAR <> 0 برای NULL هرگز true نمی‌شود.
                    if (row.ANBAR != null && row.ANBAR.Value != 0)
                    {
                        if (!invoiceLinesWithAnbar.TryGetValue(key, out var anbarLines))
                        {
                            anbarLines = new List<QRE14>();
                            invoiceLinesWithAnbar[key] = anbarLines;
                        }
                        anbarLines.Add(new QRE14
                        {
                            MABL_K = row.MABL_K,
                            MEGHk = row.MEGHk,
                            CODE = row.CODE,
                            ANBAR = row.ANBAR,
                            NAME = row.NAME,
                            AVRAGE = row.AVRAGE
                        });
                    }
                }

                // ───────────────────────────────────────────────────────────────────────────
                // چک‌های دریافتی هر فاکتور. فقط برای فاکتورهایی لازم است که جمع چک آن‌ها
                // صفر نیست (همان شرط if (JAMCH != 0d) پایین‌تر)، پس اگر هیچ فاکتوری چک
                // نداشته باشد اصلاً کوئری زده نمی‌شود.
                //
                // چرا امن است: بازسازی هرگز به PAY_GETD نمی‌نویسد.
                // ───────────────────────────────────────────────────────────────────────────
                if (jamchByInvoice.Any(kv => kv.Value != 0d && wantedInvoices.Contains(kv.Key)))
                {
                    foreach (var row in dbms.DoGetDataSQL<PAY_GETD>(
                        "SELECT N_SERI,BANK,DATE_S,DATE,SHOBEH,MABL,NAME_TAH,N_HESAB,N_S,N_KOL,N_MOIN,N_TAF," +
                        "N_KOL2,N_MOIN2,N_TAF2,N_KOL3,N_MOIN3,N_TAF3,NUMBER,TAG,ANBAR,RADIF,CUST_NO,VAZ," +
                        "LIST_NO,KIND,SANDUGH,HES1,HES2,HES3,ESTELAM FROM dbo.PAY_GETD " +
                        $"WHERE TAG = 2 AND NUMBER BETWEEN {minNum} AND {maxNum}"))
                    {
                        if (row?.NUMBER == null) { continue; }

                        var key = row.NUMBER.Value;
                        if (!wantedInvoices.Contains(key)) { continue; }

                        if (!invoiceCheques.TryGetValue(key, out var cheques))
                        {
                            cheques = new List<PAY_GETD>();
                            invoiceCheques[key] = cheques;
                        }
                        cheques.Add(row);
                    }
                }

                // اگر روزی حجم پیش‌خوانی غیرعادی شد، در لاگ دیده می‌شود.
                LogWriter.WriteLog(
                    $"سند فروش - پیش‌خوانی: {invoiceNumbers.Count} فاکتور | " +
                    $"{invoiceLines.Sum(kv => kv.Value.Count)} ردیف کالا | " +
                    $"{invoiceCheques.Sum(kv => kv.Value.Count)} چک");
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // حالت «سند روزانه» (Baseknow.SNDKH): همه‌ی فاکتورهای یک تاریخ باید یک شماره سند
            // مشترک بگیرند تا شماره سندها زیاد نشود.
            //
            // این کار سه بار در بدنه‌ی حلقه تکرار شده بود و هر بار به شکل «بگرد؛ اگر نبود بساز».
            // در اجرای موازی این یک رقابت واقعی است: دو Thread با فاکتورهای هم‌تاریخ می‌توانند
            // هر دو جواب «سندی با این تاریخ نیست» بگیرند و هر دو سند بسازند — یعنی دو سند
            // روزانه برای یک تاریخ، که دقیقاً نقض هدف این حالت است.
            //
            // راه‌حل: برای هر تاریخ یک قفل. تاریخ‌های مختلف همچنان موازی پیش می‌روند.
            // نتیجه‌ی هر تاریخ هم نگه داشته می‌شود تا فاکتورهای بعدی همان تاریخ اصلاً کوئری نزنند
            // (به‌جای یک کوئری برای هر فاکتور، یک کوئری برای هر تاریخ).
            //
            // محدودیت: این قفل درون‌پروسه‌ای است. اگر دو نسخه از برنامه هم‌زمان بازسازی کنند،
            // رقابت باقی می‌ماند — ولی پنجره‌اش بسیار کوچک‌تر از قبل است.
            // ───────────────────────────────────────────────────────────────────────────────
            var dailyDocByDate = new System.Collections.Concurrent.ConcurrentDictionary<long, double>();
            var dailyDocGates = new System.Collections.Concurrent.ConcurrentDictionary<long, object>();

            // خروجی Created فقط وقتی true است که همین فراخوانی سند را ساخته باشد؛
            // دقیقاً مثل کد قبلی که فقط در شاخه‌ی ساخت، N_S فاکتور را در حافظه ست می‌کرد.
            (double Ns, bool Created) ResolveDailyDocument(long dateN, string sharh, string userName)
            {
                if (dailyDocByDate.TryGetValue(dateN, out var known))
                {
                    return (known, false);
                }

                lock (dailyDocGates.GetOrAdd(dateN, _ => new object()))
                {
                    if (dailyDocByDate.TryGetValue(dateN, out known))
                    {
                        return (known, false);
                    }

                    var found = dbms.DoGetDataSQL<QRE10>(
                        "SELECT BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE no_s = 2 AND DATE_S = @DocDate",
                        new { DocDate = dateN }).ToList();

                    var created = found.Count == 0;
                    var resolved = created
                        ? Createsanad(dateN, sharh, 0, 2, -1, userName)
                        : (double)found.Select(x => x.N_S).FirstOrDefault();

                    dailyDocByDate[dateN] = resolved;
                    return (resolved, created);
                }
            }

            try
            {
                //for (int HFRST_EOF = 0; HFRST_EOF < HFRST.Count; HFRST_EOF++)
                var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);

                LogWriter.WriteLog(
                    $"سند فروش - تعداد رکورد: {HFRST.Count} | موازی: {Generaly.UseParallelProcessing} | " +
                    $"MaxDegreeOfParallelism: {dbParallelOptions.MaxDegreeOfParallelism}");

                ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, HFRST_EOF =>
                {
                    observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                    // ⚠️ SHSH قبلاً بیرون از حلقه تعریف شده بود و بین همه‌ی Threadها مشترک بود.
                    // یعنی Thread دوم می‌توانست شرح فاکتور خودش را روی آن بنویسد و Thread اول
                    // همان مقدار غلط را به Createsanad بدهد → شرح سند حسابداری اشتباه.
                    // حالا برای هر فاکتور محلی است.
                    string SHSH = string.Empty;
                    double? max_ns, MABL_CHK = null, JAMF, JAMCH, CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, takh;
                    string shart;
                    double MAVAD;
                    double DAST;
                    double SAR;
                    int ii;
                    string CH;
                    double JAMP;
                    var TAMIR = default(string);
                    string PER;
                    long permab;
                    if (HFRST[HFRST_EOF]?.CUST_NO == "213-1-429") //213-1-429
                    {

                    }
                    //
                    if (!IsNull(HFRST[HFRST_EOF]?.CUST_NO))
                    {
                        GETTAF3(HFRST[HFRST_EOF].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                        if (CKOL.HasValue && CMOIN.HasValue && CTAF.HasValue && CKOL > 0 && CMOIN > 0 && CTAF > 0)
                        {
                            // جلوگیری از خطای FK_DEED_DTL_TDETA_HES در زمان درج موازی اسناد فروش
                            // اگر کد حساب مشتری در TDETA_HES وجود نداشته باشد، قبل از درج DEED_DTL ایجاد می‌شود.
                            CREATHES(CKOL, CMOIN, CTAF, GETTAFNAME(HFRST[HFRST_EOF].CUST_NO));
                        }
                    }

                    SHSH = Convert.ToString(Interaction.IIf((bool)Baseknow.SNDKH, Strings.Left(" فاكتورهاي  فروش  " + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255), Strings.Left(" فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " خريدار: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255)));
                    if ((bool)Baseknow.SNDKH) // سند روزانه است
                    {
                        List<QRE10> SARST = null;
                        if (!IsNull(HFRST[HFRST_EOF].N_S)) // فاکتور سند دارد
                        {
                            SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 2 and n_s = " + HFRST[HFRST_EOF].N_S).ToList();
                            if (SARST.Count > 0)  // اگرسند  فاکتورهست
                            {
                                if (SARST.Select(x => x.DATE_S).FirstOrDefault() == HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                                {
                                    max_ns = (double)HFRST[HFRST_EOF].N_S;
                                }
                                else
                                {
                                    // تاریخ سند با تاریخ فاکتور نمی‌خواند → سند روزانه‌ی تاریخ جدید
                                    var daily = ResolveDailyDocument((long)HFRST[HFRST_EOF].DATE_N, SHSH, HFRST[HFRST_EOF].USER_NAME);
                                    max_ns = daily.Ns;
                                    if (daily.Created)
                                    {
                                        HFRST[HFRST_EOF].N_S = daily.Ns;
                                    }
                                }
                            }
                            else
                            {
                                // شماره سند فاکتور به هیچ سند فروشی اشاره نمی‌کند → سند روزانه‌ی این تاریخ
                                var daily = ResolveDailyDocument((long)HFRST[HFRST_EOF].DATE_N, SHSH, HFRST[HFRST_EOF].USER_NAME);
                                max_ns = daily.Ns;
                                if (daily.Created)
                                {
                                    HFRST[HFRST_EOF].N_S = daily.Ns;
                                }
                            } // چک کن اگه نيست صادر کن
                        }
                        else
                        {
                            // فاکتور اصلاً شماره سند ندارد → سند روزانه‌ی این تاریخ
                            var daily = ResolveDailyDocument((long)HFRST[HFRST_EOF].DATE_N, SHSH, HFRST[HFRST_EOF].USER_NAME);
                            max_ns = daily.Ns;
                            if (daily.Created)
                            {
                                HFRST[HFRST_EOF].N_S = daily.Ns;
                            }
                        } // چک کن اگه نيست صادر کن
                    }
                    else if (!IsNull(HFRST[HFRST_EOF]?.N_S)) // تک سندي
                                                             // فاکتور سند دارد
                    {
                        var SARST = dbms.DoGetDataSQL<QRE11>("SELECT    n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 2 and N_s = " + HFRST[HFRST_EOF].N_S).ToList();
                        if (SARST.Count > 0)   // اگرسند فاکتورهست
                        {
                            if (SARST.Select(x => x.DATE_S).FirstOrDefault() != HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                            {
                                dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + HFRST[HFRST_EOF].DATE_N + ",SHARH_S = '" + SHSH + "',GHATEI = 0,NO_S = 2,OKF=-1,USER_NAME ='" + HFRST[HFRST_EOF].USER_NAME + "' WHERE N_S =" + HFRST[HFRST_EOF].N_S);
                            }
                            max_ns = (double)HFRST[HFRST_EOF].N_S;
                        }
                        else
                        {
                            max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 2, -1, HFRST[HFRST_EOF].USER_NAME);
                            HFRST[HFRST_EOF].N_S = max_ns;
                        }
                    }
                    else
                    {
                        max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 2, -1, HFRST[HFRST_EOF].USER_NAME);
                        HFRST[HFRST_EOF].N_S = max_ns;
                    }
                    if (IsNull(HFRST[HFRST_EOF].N_S) || HFRST[HFRST_EOF].N_S != max_ns)
                    {
                        HFRST[HFRST_EOF].N_S = max_ns;
                        dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {HFRST[HFRST_EOF].NUMBER} AND (TAG = 13)) ");
                    }

                    SANAD_NUMBER = HFRST[HFRST_EOF]?.N_S;

                    // از پیش‌خوانی مرحله‌ی اول؛ قبلاً اینجا یک کوئری جدا برای هر فاکتور بود.
                    JAMF = HFRST[HFRST_EOF].NUMBER != null
                           && jamfByInvoice.TryGetValue(HFRST[HFRST_EOF].NUMBER.Value, out var jamfValue)
                        ? jamfValue : 0d;


                    JAMCH = HFRST[HFRST_EOF].NUMBER != null
                            && jamchByInvoice.TryGetValue(HFRST[HFRST_EOF].NUMBER.Value, out var jamchValue)
                        ? jamchValue : 0d;
                    dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + ") AND ((DEED_DTL.TAG)= 13))");
                    if (JAMF + HFRST[HFRST_EOF].MABL_HAZ + HFRST[HFRST_EOF].MBAA - HFRST[HFRST_EOF].TAKHFIF != 0)
                    {
                        //  dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + ") AND ((DEED_DTL.TAG)= 13))");
                        string CTAF2T = (CTAF2 == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                        string CTAF3T = (CTAF3 == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                        string CTAF4T = (CTAF4 == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                        if (CKOL is not null && CMOIN is not null && CTAF is not null)
                        {
                            CREATHES(CKOL, CMOIN, CTAF, GETTAFNAME(HFRST[HFRST_EOF].CUST_NO));
                        }

                        dbms.DoExecuteSQL($"INSERT INTO dbo.DEED_DTL(N_S,  HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES,SHARH, BED, NUMBER, TAG, RADIF ) VALUES( {max_ns},{CKOL},{CMOIN},{CTAF},{CTAF2T},{CTAF3T},{CTAF4T},N'{HFRST[HFRST_EOF].CUST_NO}',N'{Strings.Right("ف ف ش " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + HFRST[HFRST_EOF].MOLAH + Interaction.IIf(Strings.Mid(Baseknow.OPTIONSS, 55, 1) == "5", " - " + GETF_DEPART(HFRST[HFRST_EOF]?.DEPATMAN), " "), 255)}',{Math.Round((double)(JAMF + HFRST[HFRST_EOF].MABL_HAZ + HFRST[HFRST_EOF].MBAA - HFRST[HFRST_EOF].TAKHFIF))},{HFRST[HFRST_EOF].NUMBER},13,{HFRST[HFRST_EOF].NUMBER})");

                    }

                    // از پیش‌خوانده‌ها. نبودِ کلید یعنی این فاکتور ردیف کالا ندارد،
                    // که همان لیست خالیِ کوئری قبلی است.
                    var jst_sec = (HFRST[HFRST_EOF].NUMBER != null
                                   && invoiceLines.TryGetValue(HFRST[HFRST_EOF].NUMBER.Value, out var jstSecLines))
                                  ? jstSecLines
                                  : new List<QRE12>();
                    //while (!jst_sec.EOF())
                    for (int jst_sec_EOF = 0; jst_sec_EOF < jst_sec.Count; jst_sec_EOF++)
                    {
                        if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                        {
                            if (HFRST[HFRST_EOF].SADER == 0)
                            {
                                CREATHES(Baseknow.FROSH, 1, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                                if (jst_sec[jst_sec_EOF].MABL_K > 0)
                                {


                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.FROSH},{1},{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.FROSH + "-1-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE)}'
                                        ,N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " فروش " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                                        ,13)");

                                }
                            }
                            else if (jst_sec[jst_sec_EOF].ANBAR != 0)
                            {
                                CREATHES(Baseknow.FROSH, 2, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                                if (jst_sec[jst_sec_EOF].MABL_K > 0)
                                {

                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.FROSH},{2},{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.FROSH + "-1-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE)}'
                                        ,N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " فروش " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                                        ,13)");

                                }
                            }
                            else
                            {

                                CREATHES(Baseknow.DARAM, HFRST[HFRST_EOF].DEPATMAN, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                                dbms.DoExecuteSQL(
                                    $@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.DARAM},{HFRST[HFRST_EOF].DEPATMAN},{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.DARAM + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE)}'
                                        ,N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " فروش " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                                        ,13)");


                            }
                        }
                        else if (jst_sec[jst_sec_EOF].ANBAR != 0)
                        {
                            CREATHES(Baseknow.FROSH, Convert.ToDouble(jst_sec[jst_sec_EOF].CODE), Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                            if (jst_sec[jst_sec_EOF].MABL_K > 0)
                            {

                                dbms.DoExecuteSQL(
                                    $@"INSERT INTO dbo.DEED_DTL(N_S,
                                                            HES_K,
                                                            HES_M,
                                                            HES_T,
                                                            hes,
                                                            SHARH,
                                                            BES,
                                                            NUMBER
                                                            ,ARZD
                                                            ,TAG)
                                            VALUES({max_ns}
                                        ,{Baseknow.FROSH}
                                        ,{jst_sec[jst_sec_EOF].CODE}
                                        ,{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.FROSH + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE)}'
                                        ,N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " فروش " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)}
                                        ,{HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                                        ,13)");
                            }
                        }
                        else
                        {
                            CREATHES(Baseknow.DARAM, HFRST[HFRST_EOF].DEPATMAN, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                            dbms.DoExecuteSQL(
                                  $@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.DARAM},{HFRST[HFRST_EOF].DEPATMAN},{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.DARAM + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + jst_sec[jst_sec_EOF].CODE}'
                                        ,N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " فروش " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                                        ,13)");


                        }
                        //jst_sec.MoveNext();
                    }
                    ;

                    //if (Baseknow.SANAT == true || IsNull(Baseknow.SANAT) || Baseknow.tindata == null || Conversions.ToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                    if (Baseknow.SANAT == true || IsNull(Baseknow.SANAT) || Baseknow.tindata == null || SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                    {
                        // همان ردیف‌های بالا با شرط ANBAR <> 0 — از پیش‌خوانده‌ها.
                        var jst_thr = (HFRST[HFRST_EOF].NUMBER != null
                                       && invoiceLinesWithAnbar.TryGetValue(HFRST[HFRST_EOF].NUMBER.Value, out var jstThrLines))
                                      ? jstThrLines
                                      : new List<QRE14>();
                        //while (!jst_thr.EOF())
                        for (int jst_thr_EOF = 0; jst_thr_EOF < jst_thr.Count; jst_thr_EOF++)
                        {
                            MAVAD = sanatPriceNeeded
                                ? Math.Round((double)(GETSTANDARDPRICE_MAVAD(jst_thr[jst_thr_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_thr[jst_thr_EOF].MEGHk))
                                : 0d;

                            try
                            {
                                if (HFRST[HFRST_EOF]?.DATE_N != null) // Check for null before attempting string operations/conversion
                                {
                                    // If DATE_N is already a numeric type (e.g., int, long), a direct cast is fine.
                                    // If it's an object or string that *might* contain non-numeric data, use TryParse.
                                    if (!long.TryParse(HFRST[HFRST_EOF].DATE_N?.ToString(), out _))
                                    {
                                        // Handle the parsing failure:
                                        // Log the error with the problematic value for debugging.
                                        LogWriter.WriteLog($"Error parsing DATE_N: '{HFRST[HFRST_EOF].DATE_N}' for NUMBER: {HFRST[HFRST_EOF].NUMBER}. Using default date value.");
                                        // You might want to skip this record, use a default value (like 0), or throw a more specific exception.
                                    }
                                }
                            }
                            catch { IsSuccessfully = false; }


                            // ⚠️ اینجا قبلاً پرانتز جابه‌جا بسته شده بود و MEGHk به‌جای اینکه در
                            // «نتیجه» ضرب شود، داخل آرگومان «تاریخ» رفته بود:
                            //     GETSTANDARDPRICE_DAST(CODE, (long)(DATE_N * MEGHk))
                            //
                            // دو اثر داشت:
                            //  ۱) dt فقط تعیین می‌کند کدام «فرمول ساخت» در آن تاریخ معتبر بوده
                            //     (شرط HEAD_LST.DATE_N <= dt در GETLASTFR). با DATE_N * MEGHk
                            //     عددی به‌دست می‌آمد که تاریخ نبود؛ اگر MEGHk > 1 بود همیشه
                            //     جدیدترین فرمول انتخاب می‌شد، نه فرمول معتبرِ تاریخ فاکتور.
                            //  ۲) ضرب در مقدار اصلاً انجام نمی‌شد، پس دستمزدِ «یک واحد» ثبت
                            //     می‌شد در حالی که مواد و سربار برای کل مقدار حساب شده بودند.
                            //
                            // سند نامتوازن نمی‌شد (همین DAST هم در بدهکار و هم در بستانکار
                            // می‌نشیند)، ولی بهای تمام‌شده کمتر از واقع ثبت می‌شد.
                            //
                            // شکل درست همان است که دو خط بالا و پایین (MAVAD و SAR) دارند و
                            // همین محاسبه در خطوط ۶۰۰۴ و ۶۸۵۸ همین فایل هم درست نوشته شده.
                            DAST = sanatPriceNeeded
                                ? Math.Round((double)(GETSTANDARDPRICE_DAST(jst_thr[jst_thr_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_thr[jst_thr_EOF].MEGHk))
                                : 0d;
                            SAR = sanatPriceNeeded
                                ? Math.Round((double)((double)GETSTANDARDPRICE_SAR(jst_thr[jst_thr_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_thr[jst_thr_EOF].MEGHk))
                                : 0d;
                            CREATHES(Baseknow.MOGODIA, jst_thr[jst_thr_EOF].ANBAR, Convert.ToInt64(jst_thr[jst_thr_EOF].CODE), jst_thr[jst_thr_EOF].NAME);

                            if (MAVAD + DAST + SAR != 0d & Strings.Mid(Baseknow.OPTIONSS, 66, 1) != "5")
                            {
                                dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S ,HES_K ,HES_M ,HES_T ,SHARH ,hes ,BES ,ARZD ,NUMBER ,TAG)
                            VALUES ({max_ns},
                            {Baseknow.MOGODIA},
                            {jst_thr[jst_thr_EOF].ANBAR},
                            {jst_thr[jst_thr_EOF].CODE},
                            N'{Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " خروج " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255)}'
                            ,N'{Baseknow.MOGODIA + "-" + jst_thr[jst_thr_EOF].ANBAR + "-" + Convert.ToInt64(jst_thr[jst_thr_EOF].CODE)}',
                            {MAVAD + DAST + SAR},
                            {Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)}
                            ,{HFRST[HFRST_EOF].NUMBER}
                            ,{13})");

                                if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) != 1d)
                                {
                                    CREATHES(Baseknow.GHEYMAT, Convert.ToInt64(jst_thr[jst_thr_EOF].CODE), Convert.ToInt64(jst_thr[jst_thr_EOF].CODE), jst_thr[jst_thr_EOF].NAME);
                                }
                                if (MAVAD > 0d)
                                {
                                    object N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG = default;
                                    //SDRST.AddNew(); // قيمت تمام شده
                                    N_S = max_ns;
                                    HES_K = Baseknow.GHEYMAT;
                                    if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                                    {
                                        HES_M = 1;
                                        HES_T = 1;
                                        hes = Baseknow.GHEYMAT + "-1-1";
                                    }
                                    else
                                    {
                                        HES_M = jst_thr[jst_thr_EOF].CODE;
                                        HES_T = jst_thr[jst_thr_EOF].CODE;
                                        hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE) + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE);
                                    }
                                    SHARH = Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " فروش " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255);
                                    BED = MAVAD;
                                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                                    TAG = 13;

                                    //SDRST.update();
                                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES ({N_S}, {HES_K}, {HES_M}, {HES_T}, N'{hes}', N'{SHARH}', {BED}, {ARZD}, {NUMBER}, {TAG})");
                                }
                                CREATHES(Baseknow.GHEYMAT, Convert.ToDouble(jst_thr[jst_thr_EOF].CODE), 9999999, "دستمزد " + jst_thr[jst_thr_EOF].NAME);

                                if (DAST != 0d)
                                {
                                    object N_S, HES_K, HES_M, HES_T, SHARH, hes, ARZD, BED, NUMBER, TAG = default;

                                    //SDRST.AddNew(); // قيمت تمام شده
                                    N_S = max_ns;
                                    HES_K = Baseknow.GHEYMAT;
                                    HES_M = jst_thr[jst_thr_EOF].CODE;
                                    HES_T = 9999999;
                                    SHARH = Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " فروش " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255);
                                    hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE) + "-" + 9999999;
                                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                    BED = DAST;
                                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                                    TAG = 13;
                                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES ({N_S}, {HES_K}, {HES_M}, {HES_T}, N'{hes}', N'{SHARH}', {BED}, {ARZD}, {NUMBER}, {TAG})");
                                    //SDRST.update();

                                }
                                if (SAR != 0d)
                                {
                                    object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, ARZD, NUMBER, TAG = default;

                                    //SDRST.AddNew(); // قيمت تمام شده
                                    N_S = max_ns;
                                    HES_K = Baseknow.GHEYMAT;
                                    HES_M = jst_thr[jst_thr_EOF].CODE;
                                    HES_T = 9999998;
                                    CREATHES(Baseknow.GHEYMAT, Convert.ToDouble(jst_thr[jst_thr_EOF].CODE), 9999998, "سربار " + jst_thr[jst_thr_EOF].NAME);
                                    SHARH = Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " فروش " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255);
                                    hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE) + "-" + 9999998;
                                    BED = SAR;
                                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                                    TAG = 13;
                                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES ({N_S}, {HES_K}, {HES_M}, {HES_T},N'{hes}', N'{SHARH}', {BED}, {ARZD}, {NUMBER}, {TAG})");
                                    //SDRST.update();
                                }
                            }
                            else if (jst_thr[jst_thr_EOF].AVRAGE > 0)
                            {
                                object N_S, HES_K, HES_M, HES_T, SHARH, hes, BES, ARZD, NUMBER, TAG, BED = default;

                                //SDRST.AddNew(); // انبار محصول
                                N_S = max_ns;
                                HES_K = Baseknow.MOGODIA;
                                HES_M = jst_thr[jst_thr_EOF].ANBAR;
                                HES_T = jst_thr[jst_thr_EOF].CODE;
                                SHARH = Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " خروج " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255);
                                hes = Baseknow.MOGODIA + "-" + jst_thr[jst_thr_EOF].ANBAR + "-" + Convert.ToInt64(jst_thr[jst_thr_EOF].CODE);
                                BES = Math.Round((double)(jst_thr[jst_thr_EOF].AVRAGE * jst_thr[jst_thr_EOF].MEGHk));
                                ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                NUMBER = HFRST[HFRST_EOF].NUMBER;
                                TAG = 13;
                                // string testtrace = $"INSERT INTO DEED_DTL( N_S, HES_K, HES_M, HES_T, SHARH, hes, BES, ARZD, NUMBER, TAG) VALUES ( {N_S}, {HES_K}, {HES_M}, {HES_T}, N'{SHARH}', N'{hes}', {BES}, {ARZD}, {NUMBER}, {TAG})";
                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL( N_S, HES_K, HES_M, HES_T, SHARH, hes, BES, ARZD, NUMBER, TAG) VALUES ( {N_S}, {HES_K}, {HES_M}, {HES_T}, N'{SHARH}', N'{hes}', {BES}, {ARZD}, {NUMBER}, {TAG})");
                                //SDRST.update();


                                //{N_S},{HES_K} ,{HES_M} ,{HES_T} ,{hes}   ,{SHARH} ,{BED}   ,{ARZD}  ,{NUMBER},{TAG}
                                //SDRST.AddNew(); // قيمت تمام شده
                                N_S = max_ns;
                                HES_K = Baseknow.GHEYMAT;
                                if (!string.IsNullOrEmpty(Baseknow.tindata))
                                {
                                    if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                                    {
                                        HES_M = 1;
                                        HES_T = 1;
                                        hes = Baseknow.GHEYMAT + "-1-1";
                                    }
                                    else
                                    {
                                        HES_M = jst_thr[jst_thr_EOF].CODE;
                                        HES_T = jst_thr[jst_thr_EOF].CODE;
                                        hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE) + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE);
                                    }
                                }
                                else
                                {
                                    HES_M = jst_thr[jst_thr_EOF].CODE;
                                    HES_T = jst_thr[jst_thr_EOF].CODE;
                                    hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE) + "-" + Convert.ToDouble(jst_thr[jst_thr_EOF].CODE);
                                }
                                CREATHES(Baseknow.GHEYMAT, Convert.ToInt64(jst_thr[jst_thr_EOF].CODE), Convert.ToInt64(jst_thr[jst_thr_EOF].CODE), "قیمیت تمام شده " + jst_thr[jst_thr_EOF].NAME);

                                SHARH = Strings.Left("فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_thr[jst_thr_EOF].MEGHk + " فروش " + Strings.Trim(jst_thr[jst_thr_EOF].NAME), 255);
                                BED = Math.Round((double)(jst_thr[jst_thr_EOF].AVRAGE * jst_thr[jst_thr_EOF].MEGHk));
                                ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                NUMBER = HFRST[HFRST_EOF].NUMBER;
                                TAG = 13;
                                //SDRST.update();

                                //  string testtrace2 = $"INSERT INTO DEED_DTL(N_S,HES_K ,HES_M ,HES_T ,hes   ,SHARH ,BED   ,ARZD  ,NUMBER,TAG) VALUES ({N_S},{HES_K} ,{HES_M} ,{HES_T} ,N'{hes}'   ,N'{SHARH}' ,{BED}   ,{ARZD}  ,{NUMBER},{TAG})";
                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K ,HES_M ,HES_T ,hes   ,SHARH ,BED   ,ARZD  ,NUMBER,TAG) VALUES ({N_S},{HES_K} ,{HES_M} ,{HES_T} ,N'{hes}'   ,N'{SHARH}' ,{BED}   ,{ARZD}  ,{NUMBER},{TAG})");

                            }
                            //jst_thr.MoveNext();
                        }
                    }
                    ;
                    if (HFRST[HFRST_EOF].MABL_HAZ != 0)
                    {
                        if (IsNull(HFRST[HFRST_EOF]?.MOIN_HAZ))
                        {
                            //DoCmd.OpenForm("MESAG", default, default, default, default, acDialog, );
                            Msgwin msgwin = new Msgwin(false, "اخطار مهم ...! حساب معين سرويس مشخص نشده است و سند صادره ناقص خواهد بود حتما حساب معين سرويس را مشخص نمائيد." + " فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER);
                            msgwin.ShowDialog();
                        }
                        else
                        {
                            //SDRST.AddNew(); // كرايه حمل ياخدمات
                            object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, ARZD, TAG = default;
                            N_S = max_ns;
                            if (!IsNull(HFRST[HFRST_EOF].MOIN_HAZ))
                            {
                                GETTAF3(HFRST[HFRST_EOF].MOIN_HAZ, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                            }

                            HES_K = HKOL;
                            HES_M = HMOIN;
                            HES_T = HTAF;
                            HES_T2 = HTAF2;
                            HES_T3 = HTAF3;
                            HES_T4 = HTAF4;
                            hes = HFRST[HFRST_EOF].MOIN_HAZ;
                            SHARH = Strings.Right("سرويس فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " - " + GETTAFNAME(HFRST[HFRST_EOF].MOIN_HAZ), 255);
                            BES = Math.Round((double)HFRST[HFRST_EOF].MABL_HAZ);
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                            TAG = 13;


                            string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                            string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                            string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M} ,{HES_T} ,{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}'  ,N'{SHARH}' ,{BES}  ,{NUMBER},{ARZD} ,{TAG})");

                            //SDRST.update();
                        }
                    }
                    if (JAMCH != 0d) // چكهاي دريافتي
                    {
                        //CHRST.Open("PAY_GETD", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                        // چک‌های این فاکتور — از پیش‌خوانده‌ها.
                        var CHRST = (HFRST[HFRST_EOF].NUMBER != null
                                     && invoiceCheques.TryGetValue(HFRST[HFRST_EOF].NUMBER.Value, out var chequeRows))
                                    ? chequeRows
                                    : new List<PAY_GETD>();
                        //CHRST.MoveLast();
                        //CHRST.MoveFirst();
                        //CHRST.Filter = "NUMBER = " + HFRST[HFRST_EOF].NUMBER + " AND TAG = 2";
                        if (CHRST.Count > 0 & !IsNull(CHRST.Select(x => x.NUMBER).FirstOrDefault()))
                        {
                            //while (!CHRST_EOF)
                            for (int CHRST_EOF = 0; CHRST_EOF < CHRST.Count; CHRST_EOF++)
                            {
                                object N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, N_SERI, BANK, NUMBER, ARZD, TAG, HES_T2, HES_T3, HES_T4, BES = default;

                                MABL_CHK = MABL_CHK + CHRST[CHRST_EOF].MABL;
                                //SDRST.AddNew(); // اسناد دريافتني
                                N_S = max_ns;
                                HES_K = GETKOL(Baseknow.ADA);
                                HES_M = GETMOIN(Baseknow.ADA);
                                HES_T = GETTAF(Baseknow.ADA);
                                hes = Baseknow.ADA;
                                SHARH = Strings.Right("چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK((double)CHRST[CHRST_EOF].BANK) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                                BED = CHRST[CHRST_EOF].MABL;
                                N_SERI = CHRST[CHRST_EOF].N_SERI;
                                BANK = CHRST[CHRST_EOF].BANK;
                                NUMBER = HFRST[HFRST_EOF].NUMBER;
                                ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                TAG = 13;
                                //SDRST.update();

                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, N_SERI, BANK, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{N_SERI},{BANK},{NUMBER},{ARZD},{TAG})");



                                //SDRST.AddNew(); // چكهاي دريافتي
                                N_S = max_ns;
                                HES_K = CKOL;
                                HES_M = CMOIN;
                                HES_T = CTAF;
                                HES_T = CTAF;
                                HES_T2 = CTAF2;
                                HES_T3 = CTAF3;
                                HES_T4 = CTAF4;
                                hes = HFRST[HFRST_EOF].CUST_NO;
                                SHARH = Strings.Right("ف.ف." + HFRST[HFRST_EOF].NUMBER1 + " - " + "چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK((double)CHRST[CHRST_EOF].BANK) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                                BES = CHRST[CHRST_EOF].MABL;
                                NUMBER = HFRST[HFRST_EOF].NUMBER;
                                ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                TAG = 13;

                                string CTAF2T = (CTAF2 == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                                string CTAF3T = (CTAF3 == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                                string CTAF4T = (CTAF4 == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                                dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,SHARH,BES,NUMBER,ARZD,TAG) 
                                    VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{CTAF2T},{CTAF3T},{CTAF4T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{ARZD},13 ) ");
                                //SDRST.update();
                                //CHRST.MoveNext();
                            }
                        }
                        else
                        {
                        }
                    }
                    if (HFRST[HFRST_EOF].M_NAGHD != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, hes, SHARH, NUMBER, ARZD, TAG, HES_T2, HES_T3, HES_T4, BES = default;
                        object BED = null;

                        //SDRST.AddNew(); // مبلغ نقدشخص
                        N_S = max_ns;
                        HES_K = CKOL;
                        HES_M = CMOIN;
                        HES_T = CTAF;
                        HES_T2 = CTAF2;
                        HES_T3 = CTAF3;
                        HES_T4 = CTAF4;
                        hes = HFRST[HFRST_EOF].CUST_NO;
                        SHARH = Strings.Right("مبلغ نقد فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        if (HFRST[HFRST_EOF].M_NAGHD > 0)
                        {
                            BES = HFRST[HFRST_EOF].M_NAGHD;
                        }
                        else
                        {
                            BED = Math.Abs((double)HFRST[HFRST_EOF].M_NAGHD);
                        }
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        if (!(BES is null))
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2 ,HES_T3 ,HES_T4 ,hes,SHARH,BES,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BES},{ARZD},{NUMBER},{TAG})");
                        }
                        else if (!(BED is null))
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2 ,HES_T3 ,HES_T4 ,hes,SHARH,BED,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BED},{ARZD},{NUMBER},{TAG})");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2 ,HES_T3 ,HES_T4 ,hes,SHARH,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},{hes},N'{SHARH}',{ARZD},{NUMBER},{TAG})");
                        }
                        //SDRST.update();
                    }
                    if (HFRST[HFRST_EOF].M_NAGHD != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, hes, SHARH, NUMBER, ARZD, TAG, BES = default;
                        object BED = null;

                        //SDRST.AddNew(); // مبلغ نقدصندوق
                        N_S = max_ns;
                        HES_K = Baseknow.SANDOGH;
                        HES_M = HFRST[HFRST_EOF].DEPATMAN;
                        HES_T = HFRST[HFRST_EOF].SHIFT;
                        SHARH = Strings.Right("مبلغ نقد فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        hes = Baseknow.SANDOGH + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + HFRST[HFRST_EOF].SHIFT;
                        if (HFRST[HFRST_EOF].M_NAGHD > 0)
                        {
                            BED = HFRST[HFRST_EOF].M_NAGHD;
                        }
                        else
                        {
                            BES = Math.Abs((double)HFRST[HFRST_EOF].M_NAGHD);
                        }
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;

                        if (!(BED is null))
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,SHARH,hes,BED,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{ARZD},{NUMBER},{TAG})");
                        }
                        else if (!(BES is null))
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,SHARH,hes,BES,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BES},{ARZD},{NUMBER},{TAG})");
                        }
                        else
                        {
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M,HES_T,SHARH,hes,ARZD,NUMBER,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{ARZD},{NUMBER},{TAG})");
                        }
                        //SDRST.update();
                    }
                    if (Baseknow.TKHF == 1)
                    {
                        if (HFRST[HFRST_EOF].TAKHFIF != 0)
                        {
                            CREATHES(Baseknow.TFROSH, 1, 1, "تخفيف");
                            object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG = default;

                            //SDRST.AddNew(); // تخفيف فروش
                            N_S = max_ns;
                            HES_K = Baseknow.TFROSH;
                            HES_M = 1;
                            HES_T = 1;
                            SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                            hes = Baseknow.TFROSH + "-1-1";
                            BED = HFRST[HFRST_EOF].TAKHFIF;
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                            TAG = 13;

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S,HES_K,HES_M ,HES_T ,SHARH ,hes,BED,NUMBER,ARZD,TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{NUMBER},{ARZD},{TAG})");
                            //SDRST.update();
                        }
                    }
                    else
                    {
                        takh = 0d;
                        if (Baseknow.TKHF == 2)
                        {
                            takh = 0d;
                            //rst.Close();
                            var rst6 = dbms.DoGetDataSQL<QRE16>("SELECT INVO_LST.NUMBER, INVO_LST.TAG, TAKHPERS.CUST_CO, TAKHPERS.TAKH_COD, TAKHPERS.TAFPER ,INVO_LST.MABL_K FROM INVO_LST INNER JOIN TAKHPERS ON INVO_LST.CODE = TAKHPERS.TAKH_COD WHERE (((INVO_LST.NUMBER)=" + HFRST[HFRST_EOF].NUMBER + ") AND ((INVO_LST.TAG)=2) AND ((TAKHPERS.CUST_CO)= " + HFRST[HFRST_EOF].CUST_KIND + "))").ToList();
                            if (rst6.Count > 0)
                            {
                                //while (!rst6.EOF())
                                for (int rst6_EOF = 0; rst6_EOF < rst6.Count; rst6_EOF++)
                                {
                                    if (Math.Round((double)(rst6[rst6_EOF].MABL_K / 100 * rst6[rst6_EOF].TAFPER)) != 0)
                                    {
                                        if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                                        {
                                            CREATHES(Baseknow.TFROSH, 3, Convert.ToInt64(rst6[rst6_EOF].TAKH_COD), "تخفيف " + GETKALANAME(Convert.ToDouble(rst6[rst6_EOF].TAKH_COD)));
                                            object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG = default;

                                            //SDRST.AddNew(); // تخفيف فروش
                                            N_S = max_ns;
                                            HES_K = Baseknow.TFROSH;
                                            HES_M = 3;
                                            HES_T = rst6[rst6_EOF].TAKH_COD;
                                            SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                            hes = Baseknow.TFROSH + "-3-" + Convert.ToInt64(rst6[rst6_EOF].TAKH_COD);
                                            BED = Math.Round((double)(rst6[rst6_EOF].MABL_K / 100 * rst6[rst6_EOF].TAFPER));
                                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                            TAG = 13;
                                            takh = takh + Math.Round((double)(rst6[rst6_EOF].MABL_K / 100 * rst6[rst6_EOF].TAFPER));
                                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{NUMBER},{ARZD},{TAG})");

                                            //SDRST.update();
                                        }
                                        else
                                        {
                                            CREATHES(Baseknow.TFROSH, HFRST[HFRST_EOF].CUST_KIND, Convert.ToInt64(rst6[rst6_EOF].TAKH_COD), "تخفيف " + rst6[rst6_EOF].TAKH_COD);

                                            object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG = default;

                                            //SDRST.AddNew(); // تخفيف فروش
                                            N_S = max_ns;
                                            HES_K = Baseknow.TFROSH;
                                            HES_M = HFRST[HFRST_EOF].CUST_KIND;
                                            HES_T = rst6[rst6_EOF].TAKH_COD;
                                            SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                            hes = Baseknow.TFROSH + "-" + HFRST[HFRST_EOF].CUST_KIND + "-" + Convert.ToInt64(rst6[rst6_EOF].TAKH_COD);
                                            BED = Math.Round((double)(rst6[rst6_EOF].MABL_K / 100 * rst6[rst6_EOF].TAFPER));
                                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                            TAG = 13;
                                            takh = takh + Math.Round((double)(rst6[rst6_EOF].MABL_K / 100 * rst6[rst6_EOF].TAFPER));
                                            //SDRST.update();
                                            try
                                            {
                                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{NUMBER},{ARZD},{TAG})");
                                            }
                                            catch (Exception ex)
                                            {
                                                LogWriter.WriteLog($"خطا در قسمت تخفيف فروش : {SHARH} {HFRST[HFRST_EOF].NUMBER}: {ex.Message} | Stack: {ex.StackTrace} |");
                                            }

                                        }
                                    }
                                    //rst6.MoveNext();
                                }
                            }
                            if (HFRST[HFRST_EOF].TAKHFIF != takh)
                            {
                                double residual = (double)(HFRST[HFRST_EOF].TAKHFIF - takh);
                                if (residual != 0)
                                {
                                    CREATHES(Baseknow.TFROSH, 1, 1, "تخفيف");
                                    object N_S, HES_K, HES_M, HES_T, SHARH, hes, NUMBER, ARZD, TAG = default;

                                    N_S = max_ns;
                                    HES_K = Baseknow.TFROSH;
                                    HES_M = 1;
                                    HES_T = 1;
                                    SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                    hes = Baseknow.TFROSH + "-1-1";
                                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                    TAG = 13;

                                    if (residual > 0)
                                    {
                                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{Math.Abs(residual)},{NUMBER},{ARZD},{TAG})");
                                    }
                                    else
                                    {
                                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BES, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{Math.Abs(residual)},{NUMBER},{ARZD},{TAG})");
                                    }
                                }
                            }
                        }
                        else
                        {
                            var rst7 = dbms.DoGetDataSQL<QRE17>("SELECT     dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.INVO_LST.MABL_K, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE     (dbo.HEAD_LST.NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (dbo.HEAD_LST.TAG = 2)").ToList();
                            if (rst7.Count > 0)
                            {
                                takh = 0d;
                                //while (!rst7.EOF()) //while (!rst.EOF())
                                for (int rst7_EOF = 0; rst7_EOF < rst7.Count; rst7_EOF++)
                                {
                                    if (rst7[rst7_EOF]?.N_MOIN == null)
                                    {
                                        continue; //Skip this row
                                    }

                                    if (Math.Round((double)rst7[rst7_EOF].N_MOIN) != 0)
                                    {
                                        if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                                        {
                                            CREATHES(Baseknow.TFROSH, 3, Convert.ToInt64(rst7[rst7_EOF].CODE), "تخفيف " + GETKALANAME(Convert.ToDouble(rst7[rst7_EOF].CODE)));
                                            object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG = default;
                                            //SDRST.AddNew(); // تخفيف فروش
                                            N_S = max_ns;
                                            HES_K = Baseknow.TFROSH;
                                            HES_M = 3;
                                            HES_T = rst7[rst7_EOF].CODE;
                                            SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                            hes = Baseknow.TFROSH + "-3-" + Convert.ToInt64(rst7[rst7_EOF].CODE);
                                            BED = Math.Round((double)rst7[rst7_EOF].N_MOIN);
                                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                            TAG = 13;
                                            takh = takh + Math.Round((double)rst7[rst7_EOF].N_MOIN);

                                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{NUMBER},{ARZD},{TAG})");
                                            //SDRST.update();
                                        }
                                        else
                                        {
                                            CREATHES(Baseknow.TFROSH, HFRST[HFRST_EOF].CUST_KIND, Convert.ToInt64(rst7[rst7_EOF].CODE), "تخفيف " + rst7[rst7_EOF].CODE);


                                            object N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG = default;

                                            //SDRST.AddNew(); // تخفيف فروش
                                            N_S = max_ns;
                                            HES_K = Baseknow.TFROSH;
                                            HES_M = HFRST[HFRST_EOF].CUST_KIND;
                                            HES_T = rst7[rst7_EOF].CODE;
                                            SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                            hes = Baseknow.TFROSH + "-" + HFRST[HFRST_EOF].CUST_KIND + "-" + Convert.ToInt64(rst7[rst7_EOF].CODE);
                                            BED = Math.Round((double)rst7[rst7_EOF].N_MOIN);
                                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                                            TAG = 13;
                                            takh = takh + Math.Round((double)rst7[rst7_EOF].N_MOIN);
                                            try
                                            {
                                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{BED},{NUMBER},{ARZD},{TAG})");
                                            }
                                            catch (Exception ex)
                                            {
                                                LogWriter.WriteLog($"خطا در قسمت تخفيف فروش : {SHARH} {HFRST[HFRST_EOF].NUMBER}: {ex.Message} | Stack: {ex.StackTrace} |");
                                                /*On Error Resume Next*/
                                            }

                                            //SDRST.update();
                                        }
                                    }
                                    //rst7.MoveNext();
                                }
                            }
                            if (HFRST[HFRST_EOF].TAKHFIF != takh)
                            {
                                double residual = (double)(HFRST[HFRST_EOF].TAKHFIF - takh);
                                if (residual != 0)
                                {
                                    CREATHES(Baseknow.TFROSH, 1, 1, "تخفيف");
                                    object N_S, HES_K, HES_M, HES_T, SHARH, hes, NUMBER, ARZD, TAG = default;

                                    N_S = max_ns;
                                    HES_K = Baseknow.TFROSH;
                                    HES_M = 1;
                                    HES_T = 1;
                                    SHARH = Strings.Right("مبلغ تخفيف فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                                    hes = Baseknow.TFROSH + "-1-1";
                                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                    TAG = 13;

                                    if (residual > 0)
                                    {
                                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BED, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{Math.Abs(residual)},{NUMBER},{ARZD},{TAG})");
                                    }
                                    else
                                    {
                                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, hes, BES, NUMBER, ARZD, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{SHARH}',N'{hes}',{Math.Abs(residual)},{NUMBER},{ARZD},{TAG})");
                                    }
                                }
                            }
                        }
                    }
                    if (HFRST[HFRST_EOF].MABL_HAV != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG = default;
                        //SDRST.AddNew(); // مبلغ حواله شخص
                        N_S = max_ns;
                        HES_K = CKOL;
                        HES_M = CMOIN;
                        HES_T = CTAF;
                        HES_T2 = CTAF2;
                        HES_T3 = CTAF3;
                        HES_T4 = CTAF4;
                        hes = HFRST[HFRST_EOF].CUST_NO;
                        SHARH = Strings.Right("مبلغ حواله فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BES = HFRST[HFRST_EOF].MABL_HAV;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                        //{N_S},{HES_K },{HES_M },{HES_T },{HES_T2},{HES_T3},{HES_T4},{hes},{SHARH },{BES},{ARZD},{NUMBER},{TAG}
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BES},{ARZD},{NUMBER},{TAG})");
                        //SDRST.update();
                    }
                    if (HFRST[HFRST_EOF].MABL_HAV != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, ARZD, NUMBER, TAG = default;
                        //SDRST.AddNew(); // مبلغ حواله
                        N_S = max_ns;
                        if (!IsNull(HFRST[HFRST_EOF].MOIN_HAV))
                        {
                            GETTAF3(HFRST[HFRST_EOF].MOIN_HAV, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        }
                        HES_K = HKOL;
                        HES_M = HMOIN;
                        HES_T = HTAF;
                        HES_T2 = HTAF2;
                        HES_T3 = HTAF3;
                        HES_T4 = HTAF4;
                        hes = HFRST[HFRST_EOF].MOIN_HAV;
                        SHARH = Strings.Right("مبلغ حواله فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        BED = HFRST[HFRST_EOF].MABL_HAV;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;
                        //{N_S},{ HES_K},{ HES_M},{ HES_T},{ HES_T2},{ HES_T3},{ HES_T4},{ hes},{ SHARH},{ BED},{ ARZD},{ NUMBER},{ TAG }

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BED},{ARZD},{NUMBER},{TAG})");
                        //SDRST.update();
                    }
                    if (HFRST[HFRST_EOF].MABL_VAR != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG = default;


                        //SDRST.AddNew(); // مبلغ واريزي شخص
                        N_S = max_ns;
                        HES_K = CKOL;
                        HES_M = CMOIN;
                        HES_T = CTAF;
                        HES_T2 = CTAF2;
                        HES_T3 = CTAF3;
                        HES_T4 = CTAF4;
                        hes = HFRST[HFRST_EOF].CUST_NO;
                        SHARH = Strings.Right("مبلغ واريزي فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BES = HFRST[HFRST_EOF].MABL_VAR;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BES},{ARZD},{NUMBER},{TAG})");
                        //SDRST.update();
                    }
                    if (HFRST[HFRST_EOF].MABL_VAR != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, ARZD, NUMBER, TAG = default;

                        //SDRST.AddNew(); // مبلغ واريزي
                        N_S = max_ns;
                        if (!IsNull(HFRST[HFRST_EOF].MOIN_VAR))
                        {
                            GETTAF3(HFRST[HFRST_EOF].MOIN_VAR, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        }
                        HES_K = HKOL;
                        HES_M = HMOIN;
                        HES_T = HTAF;
                        HES_T2 = HTAF2;
                        HES_T3 = HTAF3;
                        HES_T4 = HTAF4;
                        hes = HFRST[HFRST_EOF].MOIN_VAR;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        SHARH = Strings.Right("مبلغ واريزي فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BED = HFRST[HFRST_EOF].MABL_VAR;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BED},{ARZD},{NUMBER},{TAG})");
                        //SDRST.update();
                    }
                    if (HFRST[HFRST_EOF].MBAA != 0)
                    {
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG = default;

                        //SDRST.AddNew(); // ماليات بر ارزش افزوده
                        N_S = max_ns;
                        SHARH = Strings.Right(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        if (!IsNull(HFRST[HFRST_EOF].HMBAA) && !string.IsNullOrWhiteSpace(HFRST[HFRST_EOF].HMBAA))
                        {
                            GETTAF3(HFRST[HFRST_EOF].HMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                            hes = HFRST[HFRST_EOF].HMBAA;
                        }
                        else //اگر حساب مالیات نداره از پیش فرض حساب مالیات در تعریف حساب های خودگردان بگیر
                        {
                            LogWriter.WriteLog($@"#WARNING  در بازسازی سند فروش : برای شماره فاکتور (حواله) {HFRST[HFRST_EOF].NUMBER1} به شرح {SHARH} حساب مالیات آن وجود نداشت , بنابر این با حساب پیش فرض مالیات در حسابهای خودگردان سند زدم ");
                            GETTAF3(Baseknow.HESMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                            hes = Baseknow.HESMBAA;
                        }
                        HES_K = HKOL;
                        HES_M = HMOIN;
                        HES_T = HTAF;
                        HES_T2 = HTAF2;
                        HES_T3 = HTAF3;
                        HES_T4 = HTAF4;
                        BES = HFRST[HFRST_EOF].MBAA;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 13;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG) VALUES" +
                            $" ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BES},{ARZD},{NUMBER},{TAG})");
                        //SDRST.update();
                    }
                    JAMP = 0d;
                    if (JAMF > 0d)
                    {
                        var PRST = dbms.DoGetDataSQL<VISITOR_DTL_1>("SELECT     dbo.VISITOR_DTL.* FROM dbo.VISITOR_DTL WHERE     (NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (TAG = 2) ").ToList();
                        //while (!PRST.EOF)

                        for (int PRST_EOF = 0; PRST_EOF < PRST.Count; PRST_EOF++)
                        {
                            object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, ARZD, NUMBER, TAG = default;

                            //SDRST.AddNew(); // پورسانت
                            N_S = max_ns;
                            if (!IsNull(PRST[PRST_EOF].CUST_NO))
                            {
                                GETTAF3(PRST[PRST_EOF].CUST_NO, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                            }
                            HES_K = HKOL;
                            HES_M = HMOIN;
                            HES_T = HTAF;
                            HES_T2 = HTAF2;
                            HES_T3 = HTAF3;
                            HES_T4 = HTAF4;
                            hes = PRST[PRST_EOF].CUST_NO;
                            if (IsNull(TAMIR))
                            {
                                TAMIR = PRST[PRST_EOF].CUST_NO;
                            }
                            SHARH = Strings.Right(" فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER + " : " + HFRST[HFRST_EOF].NUMBER1 + " بابت " + PRST[PRST_EOF].DARSAD + "% مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + Interaction.IIf(IsNull(PRST[PRST_EOF].TOZIH), "", PRST[PRST_EOF].TOZIH) + "مبلغ :  " + Strings.Format(Math.Round((double)(JAMF + HFRST[HFRST_EOF].MABL_HAZ + HFRST[HFRST_EOF].MBAA - HFRST[HFRST_EOF].TAKHFIF)), "#,###") + " " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                            if (IsNull(PRST[PRST_EOF].PORID))
                            {
                                if ((bool)!PRST[PRST_EOF].STAT)
                                {
                                    if (Math.Round((double)((JAMF - HFRST[HFRST_EOF].TAKHFIF + ((SafeToDouble(Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 62, 1)) == 5) ? (HFRST[HFRST_EOF].MBAA) : 0)) * PRST[PRST_EOF].DARSAD / 100)) != PRST[PRST_EOF].PURSANT)
                                    {
                                        PRST[PRST_EOF].PURSANT = Math.Round((double)((JAMF - HFRST[HFRST_EOF].TAKHFIF + ((SafeToDouble(Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 62, 1)) == 5) ? (HFRST[HFRST_EOF].MBAA) : 0)) * PRST[PRST_EOF].DARSAD / 100));

                                        dbms.DoExecuteSQL($"UPDATE VISITOR_DTL SET PURSANT = {PRST[PRST_EOF].PURSANT} WHERE     (NUMBER = {HFRST[HFRST_EOF].NUMBER}) AND CUST_NO = N'{PRST[PRST_EOF].CUST_NO}' AND (TAG = 2) ");
                                        //PRST.update;
                                    }
                                }

                                else if (PRST[PRST_EOF].DARSAD != PRST[PRST_EOF].PURSANT / (JAMF - HFRST[HFRST_EOF].TAKHFIF + ((SafeToDouble(Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 62, 1)) == 5) ? (HFRST[HFRST_EOF].MBAA) : 0)) * 100)
                                {
                                    PRST[PRST_EOF].DARSAD = PRST[PRST_EOF].PURSANT / (JAMF - HFRST[HFRST_EOF].TAKHFIF + ((SafeToDouble(Strings.Mid(Convert.ToString(Baseknow.OPTIONSS), 62, 1)) == 5) ? (HFRST[HFRST_EOF].MBAA) : 0)) * 100;
                                    dbms.DoExecuteSQL($"UPDATE VISITOR_DTL SET DARSAD = {PRST[PRST_EOF].DARSAD} WHERE     (NUMBER = {HFRST[HFRST_EOF].NUMBER}) AND CUST_NO = N'{PRST[PRST_EOF].CUST_NO}' AND (TAG = 2) ");
                                    //PRST.update;
                                }

                            }
                            else
                            {
                                long prs;
                                long MBK;
                                prs = 0L;
                                MBK = 0L;
                                //پرداخت پورسانت بر اساس الگوی پرداخت پورسانت
                                var rst1 = dbms.DoGetDataSQL<QRE18>("select code ,mabl_k  - n_moin as mablk from invo_lst where tag = 2 and number = " + HFRST[HFRST_EOF].NUMBER).ToList();
                                //while (!rst1.EOF)
                                for (int rst1_EOF = 0; rst1_EOF < rst1.Count; rst1_EOF++)
                                {
                                    var rst2 = dbms.DoGetDataSQL<double?>("SELECT  PORSANT FROM dbo.VISITORS_PORSANT_KALA WHERE (PORID = " + PRST[PRST_EOF].PORID + ") and (code = '" + rst1[rst1_EOF].code + "')").ToList();
                                    if (rst2.Count == 1)
                                    {
                                        prs = (long)(prs + Math.Round((double)(rst1[rst1_EOF].mablk * rst2.FirstOrDefault() / 100)));
                                        MBK = (long)(MBK + rst1[rst1_EOF].mablk);
                                    }
                                    else
                                    {
                                        LogWriter.WriteLog("تذكر مهم :اين كالا فاقد الگو براي اين ويزيتور است و پورسانت محاسبه نشد.درصورت لزوم براي آن تعريف كنيد و همينجا مجددا الگو را انتخاب كنيد  : " + GETKALANAME(Convert.ToDouble(rst1[rst1_EOF].code)) + " فاكتور شماره : " + HFRST[HFRST_EOF].NUMBER);

                                        //Msgwin msgwin = new Msgwin(false, "تذكر مهم :اين كالا فاقد الگو براي اين ويزيتور است و پورسانت محاسبه نشد.درصورت لزوم براي آن تعريف كنيد و همينجا مجددا الگو را انتخاب كنيد  : " + GETKALANAME(Convert.ToDouble(rst1[rst1_EOF].code)) + " فاكتور شماره : " + HFRST[HFRST_EOF].NUMBER);
                                        //msgwin.ShowDialog();
                                    }
                                    //rst1.MoveNext();
                                    //rst2.Close();
                                }
                                //rst1.Close();
                                PRST[PRST_EOF].PURSANT = prs;
                                if (MBK > 0L)
                                {
                                    PRST[PRST_EOF].DARSAD = PRST[PRST_EOF].PURSANT / MBK * 100;
                                }
                                dbms.DoExecuteSQL($"UPDATE VISITOR_DTL SET PURSANT = {prs} , DARSAD = {PRST[PRST_EOF].DARSAD} WHERE     (NUMBER = {HFRST[HFRST_EOF].NUMBER}) AND CUST_NO = N'{PRST[PRST_EOF].CUST_NO}' AND (TAG = 2) ");
                                //PRST.update();
                            }


                            BES = PRST[PRST_EOF].PURSANT;
                            JAMP = (double)(JAMP + PRST[PRST_EOF].PURSANT);
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 13;
                            if (PRST[PRST_EOF].PURSANT != 0)
                            {
                                //SDRST.update();
                                string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                                string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                                string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES,NUMBER,TAG) VALUES({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG})");
                            }
                            //PRST.MoveNext();
                        }
                        if (JAMP != 0d)
                        {

                            if (IsNull(Baseknow.HPOR))
                            {
                                //var msg = "حساب پورسانت در حسابهاي خودگردان مشخص نشده سند تراز نمي شود لطفا حساب پورسانت را مشخص كنيد و سند اين فاكتور را با دابل كليك مجددا صادر كنيد...!";
                                //LogWriter.WriteLog("[Baseknow.HPOR] " + msg);
                                //    Msgwin msgwin = new Msgwin(false, "حساب پورسانت در حسابهاي خودگردان مشخص نشده سند تراز نمي شود لطفا حساب پورسانت را مشخص كنيد و سند اين فاكتور را با دابل كليك مجددا صادر كنيد...!");
                                //  msgwin.ShowDialog();
                            }
                            else
                            {
                                object N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG = default;

                                //SDRST.AddNew(); // پورسانت
                                N_S = max_ns;
                                HES_K = GETKOL(Baseknow.HPOR);
                                HES_M = GETMOIN(Baseknow.HPOR);
                                HES_T = GETTAF(Baseknow.HPOR);
                                hes = Baseknow.HPOR;
                                SHARH = Strings.Left("بابت درصد سهم  فاكتور فروش شماره " + HFRST[HFRST_EOF].NUMBER + " : " + HFRST[HFRST_EOF].NUMBER1 + GETTAFNAME(TAMIR), 255);
                                BED = JAMP;
                                ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                                NUMBER = HFRST[HFRST_EOF].NUMBER;
                                TAG = 13;
                                dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, ARZD, NUMBER, TAG) VALUES({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{ARZD},{NUMBER},{TAG})");
                                //SDRST.update();
                            }
                        }
                    }
                    ;

                    // گزارش پیشرفت در «پایان» کار هر فاکتور زده می‌شود، نه در ابتدای آن.
                    // قبلاً در ابتدا بود و نوار پیشرفت زودتر از واقعیت جلو می‌رفت.
                    progressReporter.ReportOne();
                });
                //}
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    LogWriter.WriteLog($"خطا در پردازش موازی فاکتورها: {ex.Message} | Stack: {ex.StackTrace}");
                }
                IsSuccessfully = false;
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog($"خطا کلی در GENSANADFROOSH: {ex.Message} | Stack: {ex.StackTrace}");
                IsSuccessfully = false;
            }

            stopwatch.Stop();
            progressReporter.Complete();

            LogWriter.WriteLog(
                $"پایان بازسازی سند فروش - {HFRST.Count} رکورد در {stopwatch.Elapsed.TotalSeconds:F1} ثانیه " +
                $"با {observedThreads.Count} Thread همزمان");

            return (SANAD_NUMBER, IsSuccessfully);
        }
        public static double GETSTANDARDPRICE_SAR(string CODE, long dt)
        {
            // «خواندن خالص» است و در طول یک بازسازی تغییر نمی‌کند؛ برای هر قلم کالای
            // هر فاکتور صدا زده می‌شود، پس تکرارش بسیار زیاد است.
            var priceKey = (Code: CODE ?? string.Empty, Dt: dt);
            if (LookupCacheEnabled && _standardPriceSar.TryGetValue(priceKey, out var cachedPrice))
            {
                return cachedPrice;
            }

            double tempGETSTANDARDPRICE_SAR = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_SAR = (double)rst.SumOfIMBIBE_SAR;
                }
                else
                {
                    tempGETSTANDARDPRICE_SAR = 0;
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_SAR = (double)rst.SumOfIMBIBE_SAR;
                }
                else
                {
                    tempGETSTANDARDPRICE_SAR = 0;
                }
            }
            if (LookupCacheEnabled)
            {
                _standardPriceSar[priceKey] = tempGETSTANDARDPRICE_SAR;
            }

            return tempGETSTANDARDPRICE_SAR;
        }
        public static double GETSTANDARDPRICE_DAST(string CODE, long dt)
        {
            // «خواندن خالص» است و در طول یک بازسازی تغییر نمی‌کند؛ برای هر قلم کالای
            // هر فاکتور صدا زده می‌شود، پس تکرارش بسیار زیاد است.
            var priceKey = (Code: CODE ?? string.Empty, Dt: dt);
            if (LookupCacheEnabled && _standardPriceDast.TryGetValue(priceKey, out var cachedPrice))
            {
                return cachedPrice;
            }

            double tempGETSTANDARDPRICE_DAST = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_DAST = (double)rst.SumOfIMBIBE_MANF;
                }
                else
                {
                    tempGETSTANDARDPRICE_DAST = 0;
                }
            }
            else
            {
                var rst5 = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst5 is null))
                {
                    tempGETSTANDARDPRICE_DAST = (double)rst5.SumOfIMBIBE_MANF;
                }
                else
                {
                    tempGETSTANDARDPRICE_DAST = 0;
                }
            }
            if (LookupCacheEnabled)
            {
                _standardPriceDast[priceKey] = tempGETSTANDARDPRICE_DAST;
            }

            return tempGETSTANDARDPRICE_DAST;
        }
        public static double GETLASTFR(string co, long dt)
        {
            // هر سه تابع GETSTANDARDPRICE_* این را صدا می‌زنند، پس برای هر قلم کالا چند بار اجرا می‌شود.
            var lastFrKey = (Code: co ?? string.Empty, Dt: dt);
            if (LookupCacheEnabled && _lastFrCache.TryGetValue(lastFrKey, out var cachedLastFr))
            {
                return cachedLastFr;
            }

            double tempGETLASTFR = 0;
            long FNN = 0;
            //object rst = null;

            //if (rst.GetType().GetProperties()[3].GetValue(rst, null) is true)
            var rst = dbms.DoGetDataSQL<double?>("SELECT     TOP 100 PERCENT dbo.INVO_LST.N_KOL FROM dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE     (dbo.INVO_LST.TAG = 9) AND (dbo.INVO_LST.CODE = N'" + co + "') AND (dbo.HEAD_LST.DATE_N <= " + dt + ") ORDER BY dbo.INVO_LST.NUMBER DESC").FirstOrDefault();
            if (!(rst is null))
            {
                if (IsNull(rst))
                {
                    var rst1 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                    if (!(rst1 is null))
                    {
                        tempGETLASTFR = (double)rst1;
                    }
                    else
                    {
                        tempGETLASTFR = 0;
                    }
                }
                else
                {
                    FNN = (long)rst;


                    var rst2 = dbms.DoGetDataSQL<QRE15>("SELECT     TOP 100 PERCENT CODE,FNUMB FROM dbo.HEAD_MANF WHERE     (FNUMB = " + FNN + " and CODE = N'" + co + "')").FirstOrDefault();
                    if (!(rst2 is null))
                    {
                        tempGETLASTFR = (double)rst2.FNUMB;
                    }
                    else
                    {
                        var rst3 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                        if (!(rst3 is null))
                        {
                            tempGETLASTFR = (double)rst3;
                        }
                        else
                        {
                            tempGETLASTFR = 0;
                        }
                    }
                }
            }
            else
            {
                var rst4 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                if (!(rst4 is null))
                {
                    tempGETLASTFR = (double)rst4;
                }
                else
                {
                    tempGETLASTFR = 0;
                }
            }
            if (LookupCacheEnabled)
            {
                _lastFrCache[lastFrKey] = tempGETLASTFR;
            }

            return tempGETLASTFR;
        }
        public static double GETSTANDARDPRICE_MAVAD(string CODE, long dt)
        {
            // «خواندن خالص» است و در طول یک بازسازی تغییر نمی‌کند؛ برای هر قلم کالای
            // هر فاکتور صدا زده می‌شود، پس تکرارش بسیار زیاد است.
            var priceKey = (Code: CODE ?? string.Empty, Dt: dt);
            if (LookupCacheEnabled && _standardPriceMavad.TryGetValue(priceKey, out var cachedPrice))
            {
                return cachedPrice;
            }

            double tempGETSTANDARDPRICE_MAVAD = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").ToList();
                if (rst.Count > 0)
                {
                    tempGETSTANDARDPRICE_MAVAD = (double)rst.Select(x => x.SumOfMABLK).FirstOrDefault();
                }
                else
                {
                    tempGETSTANDARDPRICE_MAVAD = 0;
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").ToList();
                if (rst.Count > 0)
                {
                    tempGETSTANDARDPRICE_MAVAD = (double)rst.Select(x => x.SumOfMABLK).FirstOrDefault();
                }
                else
                {
                    tempGETSTANDARDPRICE_MAVAD = 0;
                }
            }
            if (LookupCacheEnabled)
            {
                _standardPriceMavad[priceKey] = tempGETSTANDARDPRICE_MAVAD;
            }

            return tempGETSTANDARDPRICE_MAVAD;
        }


        private static readonly object _creatHesLock = new object();


        [System.Diagnostics.DebuggerStepThrough]
        public static void CREATHES(double? KOL, double? MOIN, double? taf, string nam)
        {
            if (KOL is null || MOIN is null || taf is null)
            {
                LogWriter.WriteLog($"[CREATHES] حساب نامعتبر است. KOL={KOL}, MOIN={MOIN}, TAF={taf}, NAME={nam}");
                throw new ArgumentException($"[CREATHES] حساب نامعتبر است. KOL={KOL}, MOIN={MOIN}, TAF={taf}");
            }

            int kolValue = Convert.ToInt32(KOL.Value);
            int moinValue = Convert.ToInt32(MOIN.Value);
            int tafValue = Convert.ToInt32(taf.Value);
            string accountName = nam ?? string.Empty;

            if (accountName.Length > 250)
            {
                accountName = accountName.Substring(0, 250);
            }

            if (ISHESAB(kolValue, moinValue, tafValue))
            {
                return;
            }

            string sql = @"
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.DETA_HES WHERE N_KOL = @Kol AND NUMBER = @Moin)
        BEGIN
            INSERT INTO dbo.DETA_HES (N_KOL, NUMBER, NAME)
            VALUES (@Kol, @Moin, @Name);
        END
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() NOT IN (2601, 2627) THROW;
    END CATCH;

   BEGIN TRY
                    INSERT INTO dbo.TDETA_HES (N_KOL, NUMBER, TNUMBER, NAME)
                    VALUES (@Kol, @Moin, @Taf, @Name);
                END TRY
                BEGIN CATCH
                    IF ERROR_NUMBER() IN (2601, 2627)
                    BEGIN
                        -- IX_TDETA_HES_NAME: نام تکراری - درج با نام منحصربه‌فرد (نام + کد تفصیلی)
                        IF NOT EXISTS (SELECT 1 FROM dbo.TDETA_HES WHERE N_KOL = @Kol AND NUMBER = @Moin AND TNUMBER = @Taf)
                        BEGIN
                            INSERT INTO dbo.TDETA_HES (N_KOL, NUMBER, TNUMBER, NAME)
                            VALUES (@Kol, @Moin, @Taf,
                                LEFT(@Name, 240) + N' (' + CAST(CAST(@Taf AS INT) AS NVARCHAR(20)) + N')');
                        END
                    END
                    ELSE THROW;
                END CATCH;
";

            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    dbms.DoExecuteSQL(sql, new { Kol = kolValue, Moin = moinValue, Taf = tafValue, Name = accountName });

                    // حساب قطعاً ساخته شد؛ ثبت در کش تا ISHESAB بعدی دوباره کوئری نزند.
                    MarkAccountExists(kolValue, moinValue, tafValue);
                    return;
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when ((ex.Number == 2601 || ex.Number == 2627) && ex.Message.Contains("IX_TDETA_HES_NAME"))
                {
                    var message =
                        $"حساب با این نام قبلاً در همین سطح کل/معین ثبت شده است و امکان ساخت تفصیلی جدید وجود ندارد. " +
                        $"کل={kolValue}، معین={moinValue}، تفصیلی درخواستی={tafValue}، نام='{accountName}'. " +
                        "لطفاً یا از همان تفصیلی قبلی استفاده کنید، یا نام حساب را اصلاح/یکتا کنید.";

                    LogWriter.WriteLog("[CREATHES] " + message + " | " + ex.Message);
                    ExpectionLogWriter.WriteLog(ex, "CREATHES");
                    throw new Exception(message, ex);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1205 || ex.Number == -2)
                {
                    if (attempt == maxRetries)
                    {
                        var msg = $"خطای بن‌بست پس از {maxRetries} تلاش. KOL={kolValue}, MOIN={moinValue}, TAF={tafValue}";
                        LogWriter.WriteLog(msg + " | " + ex.Message);
                        ExpectionLogWriter.WriteLog(ex, "CREATHES");
                        throw new Exception(msg, ex);
                    }
                    System.Threading.Thread.Sleep(new Random().Next(10, 50));
                }
                catch (Exception ex)
                {
                    var message = $"خطای بحرانی در ساخت سرفصل حساب. KOL={kolValue}, MOIN={moinValue}, TAF={tafValue}";
                    LogWriter.WriteLog(message + " | " + ex.Message);
                    ExpectionLogWriter.WriteLog(ex, "CREATHES");
                    throw new Exception(message, ex);
                }
            }
        }




        [System.Diagnostics.DebuggerStepThrough]
        public static bool ISHESAB(double? KOL, double? MOIN, double? taf)
        {
            // فقط پاسخ مثبت کش می‌شود: حسابی که یک بار دیده شده هرگز در طول اجرا حذف نمی‌شود.
            // پاسخ منفی کش نمی‌شود چون ممکن است CREATHES بلافاصله بعدش آن حساب را بسازد.
            var key = (Kol: KOL ?? 0d, Moin: MOIN ?? 0d, Taf: taf ?? 0d);
            if (LookupCacheEnabled && _existingAccounts.ContainsKey(key))
            {
                return true;
            }

            bool tempISHESAB = false;
            var rst = dbms.DoGetDataSQL<QRE13>("SELECT N_KOL,NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + KOL.ToString() + " AND NUMBER = " + MOIN.ToString() + " AND TNUMBER = " + taf).ToList();
            if (rst.Count == 0)
            {
                tempISHESAB = false;
            }
            else
            {
                tempISHESAB = true;
                MarkAccountExists(key.Kol, key.Moin, key.Taf);
            }

            return tempISHESAB;
        }
        public static string GETF_DEPART(long? DEPART)
        {
            // نام دپارتمان در طول اجرا ثابت است، ولی این تابع به‌ازای هر ردیف سند
            // صدا زده می‌شود و هر بار یک یا دو کوئری می‌زند. تعداد دپارتمان‌ها هم کم است.
            var cacheKey = DEPART ?? long.MinValue;
            if (LookupCacheEnabled && _departNameCache.TryGetValue(cacheKey, out var cachedDepart))
            {
                return cachedDepart;
            }

            string tempGETDEPART = null;

            if (DEPART != null)
            {
                var rst = dbms.DoGetDataSQL<DEPART_CSHARP>("SELECT * FROM DEPART WHERE DEPATMAN = " + DEPART).FirstOrDefault();
                if (rst != null)
                {
                    tempGETDEPART = rst.DEPNAME;
                }
            }

            if (string.IsNullOrEmpty(tempGETDEPART))
            {
                var rst2 = dbms.DoGetDataSQL<DEPART_CSHARP>("SELECT TOP 1 * FROM dbo.DEPART WHERE DEPATMAN = (SELECT MIN(DEPATMAN) FROM DEPART)").FirstOrDefault();
                if (rst2 != null)
                {
                    tempGETDEPART = rst2.DEPNAME;
                }
            }

            if (LookupCacheEnabled && tempGETDEPART != null)
            {
                _departNameCache[cacheKey] = tempGETDEPART;
            }

            return tempGETDEPART;
        }
        public static long GETKOL(string SHES)
        {
            long GETKOLRet = default;
            byte i;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETKOLRet;
                    }
                }
                GETKOLRet = Conversions.ToLong(Strings.Left(SHES, i - 1));
            }

            return GETKOLRet;
        }
        public static long GETMOIN(string SHES)
        {
            long GETMOINRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETMOINRet;
                    }
                }
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-" & j <= Strings.Len(SHES))
                    j = (byte)(j + 1);
                i = (byte)(i + 1);
                GETMOINRet = Conversions.ToLong(Strings.Mid(SHES, i, j - i));
            }

            return GETMOINRet;
        }
        public static long GETTAF(string SHES)
        {
            long GETTAFRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETTAFRet;
                    }
                }
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-")
                    j = (byte)(j + 1);
                i = j;
                while (Strings.Mid(SHES, j + 1, 1) != "-" & j < Strings.Len(SHES))
                    j = (byte)(j + 1);
                GETTAFRet = Conversions.ToLong(Strings.Mid(SHES, i + 1, j - i));
            }

            return GETTAFRet;
        }
        public static string GETBANK(double BANK)
        {
            if (LookupCacheEnabled && _bankNameCache.TryGetValue(BANK, out var cachedBank))
            {
                return cachedBank;
            }

            string GETBANKRet = default;
            var RRST = dbms.DoGetDataSQL<string>("SELECT TCOD_BANKS.CODE, TCOD_BANKS.NAMES FROM TCOD_BANKS WHERE (((TCOD_BANKS.CODE)= " + BANK + "))").FirstOrDefault();
            if (!(RRST is null))
            {
                //GETBANKRet = RRST.Fields(1);
                GETBANKRet = RRST;
            }
            else
            {
                GETBANKRet = "";
            }
            if (LookupCacheEnabled)
            {
                _bankNameCache[BANK] = GETBANKRet;
            }

            return GETBANKRet;
        }
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
        public static double? GENSANADKHAREED(object fnum, long TNUM, bool InternalCalling = true)
        {
            LogWriter.WriteLog("شروع بازسازی سند خرید");

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            double? _SANAD_NUMBER = null;
            var HFRST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>($"SELECT * FROM dbo.HEAD_LST WHERE (NUMBER BETWEEN {fnum} AND {TNUM}) AND (TAG = 12) ORDER BY NUMBER").ToList();

            var progressReporter = new ThrottledProgressReporter(
                HFRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    auto_run.PRGR_C2.Value = Math.Max(auto_run.PRGR_C2.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            var invoiceNumbers = HFRST.Where(r => r?.NUMBER != null).Select(r => r.NUMBER.Value).ToList();
            var jamfByInvoice = new Dictionary<double, double>();
            var jamchByInvoice = new Dictionary<double, double>();
            var invoiceLines = new Dictionary<double, List<QRE_BAZ_KHAREED>>();
            var invoiceCheques = new Dictionary<double, List<PAY_GETP_1>>();

            if (invoiceNumbers.Count > 0)
            {
                var minNum = SqlNum(invoiceNumbers.Min());
                var maxNum = SqlNum(invoiceNumbers.Max());

                foreach (var row in dbms.DoGetDataSQL<InvoiceSumRow>(
                    $"SELECT NUMBER, SUM(MABL_K) AS Total FROM dbo.INVO_LST " +
                    $"WHERE TAG = 1 AND NUMBER BETWEEN {minNum} AND {maxNum} GROUP BY NUMBER"))
                {
                    if (row?.NUMBER != null && row.Total != null)
                    {
                        jamfByInvoice[row.NUMBER.Value] = row.Total.Value;
                    }
                }

                foreach (var row in dbms.DoGetDataSQL<InvoiceSumRow>(
                    $"SELECT NUMBER, SUM(MABL) AS Total FROM dbo.PAY_GETP " +
                    $"WHERE TAG = 1 AND NUMBER BETWEEN {minNum} AND {maxNum} GROUP BY NUMBER"))
                {
                    if (row?.NUMBER != null && row.Total != null)
                    {
                        jamchByInvoice[row.NUMBER.Value] = row.Total.Value;
                    }
                }

                var wantedInvoices = new HashSet<double>(invoiceNumbers);

                foreach (var row in dbms.DoGetDataSQL<QRE_BAZ_KHAREED>(
                    $"SELECT INVO_LST.NUMBER, INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, dbo.STUF_DEF.RADAH " +
                    $"FROM dbo.INVO_LST INNER JOIN dbo.STUF_DEF ON STUF_DEF.CODE = INVO_LST.CODE " +
                    $"WHERE INVO_LST.TAG = 1 AND INVO_LST.NUMBER BETWEEN {minNum} AND {maxNum}"))
                {
                    if (row?.NUMBER == null || !wantedInvoices.Contains(row.NUMBER.Value)) { continue; }

                    var key = row.NUMBER.Value;
                    if (!invoiceLines.TryGetValue(key, out var lines))
                    {
                        lines = new List<QRE_BAZ_KHAREED>();
                        invoiceLines[key] = lines;
                    }
                    lines.Add(row);
                }

                if (jamchByInvoice.Any(kv => kv.Value != 0d && wantedInvoices.Contains(kv.Key)))
                {
                    foreach (var row in dbms.DoGetDataSQL<PAY_GETP_1>(
                        $"SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, KIND, VAZ, HES1, HES2, HES3 " +
                        $"FROM dbo.PAY_GETP WHERE TAG = 1 AND NUMBER BETWEEN {minNum} AND {maxNum}"))
                    {
                        if (row?.NUMBER == null) { continue; }

                        var key = row.NUMBER.Value;
                        if (!wantedInvoices.Contains(key)) { continue; }

                        if (!invoiceCheques.TryGetValue(key, out var cheques))
                        {
                            cheques = new List<PAY_GETP_1>();
                            invoiceCheques[key] = cheques;
                        }
                        cheques.Add(row);
                    }
                }
            }

            var dailyDocByDate = new System.Collections.Concurrent.ConcurrentDictionary<long, double>();
            var dailyDocGates = new System.Collections.Concurrent.ConcurrentDictionary<long, object>();

            (double Ns, bool Created) ResolveDailyDocument(long dateN, string sharh, string userName)
            {
                if (dailyDocByDate.TryGetValue(dateN, out var known))
                {
                    return (known, false);
                }

                lock (dailyDocGates.GetOrAdd(dateN, _ => new object()))
                {
                    if (dailyDocByDate.TryGetValue(dateN, out known))
                    {
                        return (known, false);
                    }

                    var found = dbms.DoGetDataSQL<QRE10>(
                        "SELECT BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE no_s = 1 AND DATE_S = @DocDate",
                        new { DocDate = dateN }).ToList();

                    var created = found.Count == 0;
                    var resolved = created
                        ? Createsanad(dateN, sharh, 0, 1, -1, userName)
                        : (double)found.Select(x => x.N_S).FirstOrDefault();

                    dailyDocByDate[dateN] = resolved;
                    return (resolved, created);
                }
            }

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);
            ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, HFRST_EOF =>
            {
                var hRow = HFRST[HFRST_EOF];
                if (hRow == null)
                {
                    progressReporter.ReportOne();
                    return;
                }

                double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                double? HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null;

                if (!IsNull(hRow.CUST_NO))
                {
                    GETTAF3(hRow.CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                    if (CKOL.HasValue && CMOIN.HasValue && CTAF.HasValue && CKOL > 0 && CMOIN > 0 && CTAF > 0)
                    {
                        CREATHES(CKOL, CMOIN, CTAF, GETTAFNAME(hRow.CUST_NO));
                    }
                }

                string SHSH = Conversions.ToString(Interaction.IIf((bool)Baseknow.SNDKH,
                    Strings.Left(" فاكتورهاي  خريد  " + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##"), 255),
                    Strings.Left(" فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + " خريدار: " + GETTAFNAME(hRow.CUST_NO), 255)));

                double max_ns;
                bool isSndkh = (bool)Baseknow.SNDKH;

                if (isSndkh)
                {
                    if (!IsNull(hRow.N_S))
                    {
                        var SARST = dbms.DoGetDataSQL<QRE10>("SELECT BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE no_s = 1 and n_s = " + hRow.N_S).FirstOrDefault();
                        if (SARST != null && SARST.DATE_S == hRow.DATE_N)
                        {
                            max_ns = (double)hRow.N_S;
                        }
                        else
                        {
                            var res = ResolveDailyDocument(hRow.DATE_N ?? 0L, SHSH, hRow.USER_NAME);
                            max_ns = res.Ns;
                            if (res.Created) { hRow.N_S = max_ns; }
                        }
                    }
                    else
                    {
                        var res = ResolveDailyDocument(hRow.DATE_N ?? 0L, SHSH, hRow.USER_NAME);
                        max_ns = res.Ns;
                        if (res.Created) { hRow.N_S = max_ns; }
                    }
                }
                else if (!IsNull(hRow.N_S))
                {
                    var SARST = dbms.DoGetDataSQL<QRE10>("SELECT BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE no_s = 1 and n_s = " + hRow.N_S).FirstOrDefault();
                    if (SARST != null)
                    {
                        if (SARST.DATE_S != hRow.DATE_N)
                        {
                            dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + hRow.DATE_N + ",SHARH_S = N'" + SqlText(SHSH) + "',GHATEI = 0,NO_S = 1,OKF=-1,USER_NAME = N'" + SqlText(hRow.USER_NAME) + "' WHERE N_S =" + hRow.N_S);
                        }
                        max_ns = (double)hRow.N_S;
                    }
                    else
                    {
                        max_ns = Createsanad((long)hRow.DATE_N, SHSH, 0, 1, -1, hRow.USER_NAME);
                        hRow.N_S = max_ns;
                    }
                }
                else
                {
                    max_ns = Createsanad((long)hRow.DATE_N, SHSH, 0, 1, -1, hRow.USER_NAME);
                    hRow.N_S = max_ns;
                }

                if (IsNull(hRow.N_S) || hRow.N_S != max_ns)
                {
                    hRow.N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE HEAD_LST SET N_S = {max_ns} WHERE NUMBER = {hRow.NUMBER} AND TAG = 12");
                }

                _SANAD_NUMBER = hRow.N_S;

                double JAMF = jamfByInvoice.TryGetValue(hRow.NUMBER ?? 0d, out var jamfVal) && jamfVal > 0 ? Math.Round(jamfVal) : 0d;
                double JAMCH = jamchByInvoice.TryGetValue(hRow.NUMBER ?? 0d, out var jamchVal) && jamchVal > 0 ? jamchVal : 0d;

                double KHMAVAV = 0d, KHNIM = 0d, KHSAKHT = 0d, KHSAY = 0d, BAZAR = 0d;
                var HS = new double[8];

                var batchQueries = new List<string>
                {
                    $"DELETE FROM DEED_DTL WHERE (NUMBER = {hRow.NUMBER}) AND (TAG = 12)"
                };

                if (invoiceLines.TryGetValue(hRow.NUMBER ?? 0d, out var lines))
                {
                    foreach (var line in lines)
                    {
                        if (line.MABL_K != 0)
                        {
                            CREATHES(Baseknow.MOGODIA, line.ANBAR, Convert.ToInt64(line.CODE), line.NAME);
                            var sharhLine = Strings.Right("خريدفاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                            var arzdVal = IsNull(hRow.ARZD) ? "1" : SqlNum(hRow.ARZD);

                            batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) " +
                                $"VALUES({max_ns},{Baseknow.MOGODIA},{line.ANBAR},{line.CODE},N'{Baseknow.MOGODIA + "-" + line.ANBAR + "-" + line.CODE}',N'{SqlText(sharhLine)}',{Math.Round((double)line.MABL_K)},{hRow.NUMBER},12,{arzdVal})");

                            switch (line.RADAH)
                            {
                                case 1: KHMAVAV += Math.Round((double)line.MABL_K); break;
                                case 2: KHNIM += Math.Round((double)line.MABL_K); break;
                                case 3: KHSAKHT += Math.Round((double)line.MABL_K); break;
                                case 4: BAZAR += Math.Round((double)line.MABL_K); break;
                                case 5: HS[1] += Math.Round((double)line.MABL_K); break;
                                case 6: HS[2] += Math.Round((double)line.MABL_K); break;
                                case 7: HS[3] += Math.Round((double)line.MABL_K); break;
                                case 8: HS[4] += Math.Round((double)line.MABL_K); break;
                                case 9: HS[5] += Math.Round((double)line.MABL_K); break;
                                case 10: HS[6] += Math.Round((double)line.MABL_K); break;
                                default: KHSAY += Math.Round((double)line.MABL_K); break;
                            }
                        }
                    }
                }

                if (hRow.MABL_HAZ != 0)
                {
                    if (!IsNull(hRow.MOIN_HAZ))
                    {
                        GETTAF3(hRow.MOIN_HAZ, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }
                    var sharhHaz = Strings.Right("خدمات فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " - " + GETTAFNAME(hRow.MOIN_HAZ), 255);
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);

                    string HES_T2T = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{HKOL},{HMOIN},{HTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.MOIN_HAZ}',N'{SqlText(sharhHaz)}',{N(hRow.MABL_HAZ)},{hRow.NUMBER},12,{arzdVal})");
                }

                if (JAMCH != 0d && invoiceCheques.TryGetValue(hRow.NUMBER ?? 0d, out var chequesList))
                {
                    foreach (var ch in chequesList)
                    {
                        var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                        var sharhApa = Strings.Right("چك " + ch.N_SERI + "بانك " + GETBANK(Convert.ToDouble(ch.BANK)) + " " + ch.SHOBEH + " مورخ " + Strings.Format(ch.DATE_S, "####/##/##"), 255);

                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,hes ,SHARH,BES ,N_SERI,BANK,NUMBER,TAG ,ARZD) VALUES ({max_ns},{GETKOL(Baseknow.APA)},{GETMOIN(Baseknow.APA)},{GETTAF(Baseknow.APA)},N'{Baseknow.APA}',N'{SqlText(sharhApa)}',{N(ch.MABL)},{N(ch.N_SERI)},{N(ch.BANK)},{hRow.NUMBER},12,{arzdVal})");

                        var sharhCust = Strings.Right("ف.خ." + hRow.NUMBER1 + " - " + "چك " + ch.N_SERI + "بانك " + GETBANK(Convert.ToDouble(ch.BANK)) + " " + ch.SHOBEH + " مورخ " + Strings.Format(ch.DATE_S, "####/##/##"), 255);
                        string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                        string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                        string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes ,SHARH,BED ,NUMBER,TAG ,ARZD) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhCust)}',{N(ch.MABL)},{hRow.NUMBER},12,{arzdVal})");
                    }
                }

                if (JAMF != 0d)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhBes = Strings.Right("فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##") + " " + hRow.MOLAH, 255);
                    var besVal = N(JAMF + (hRow.MBAA ?? 0d));

                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, hes, BES, NUMBER, TAG, ARZD, RADIF) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{SqlText(sharhBes)}',N'{hRow.CUST_NO}',{besVal},{hRow.NUMBER},12,{arzdVal},{hRow.NUMBER})");

                    if (KHMAVAV != 0d)
                    {
                        var sharhKharid = Strings.Right("خريد مواد اوليه فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{Baseknow.KHARID},1,1,N'{Baseknow.KHARID + "-1-1"}',N'{SqlText(sharhKharid)}',{N(KHMAVAV)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    if (KHNIM != 0d)
                    {
                        var sharhNim = Strings.Right("خريد نيمه ساخته فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({max_ns},{Baseknow.KHARID},2,1,N'{Baseknow.KHARID + "-2-1"}',N'{SqlText(sharhNim)}',{N(KHNIM)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    if (KHSAKHT != 0d)
                    {
                        var sharhSakht = Strings.Right("خريد ساخته شده فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({max_ns},{Baseknow.KHARID},3,1,N'{Baseknow.KHARID + "-3-1"}',N'{SqlText(sharhSakht)}',{N(KHSAKHT)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    if (BAZAR != 0d)
                    {
                        var sharhBazar = Strings.Right("خريد بازرگاني  فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{Baseknow.KHARID},4,1,N'{Baseknow.KHARID + "-4-1"}',N'{SqlText(sharhBazar)}',{N(BAZAR)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    if (KHSAY != 0d)
                    {
                        CREATHES(Baseknow.KHARID, 11, 1, "ساير 2");
                        var sharhSay = Strings.Right("خريد ساير فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{Baseknow.KHARID},11,1,N'{Baseknow.KHARID + "-11-1"}',N'{SqlText(sharhSay)}',{N(KHSAY)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    for (long K = 1L; K <= 6L; K++)
                    {
                        if (HS[(int)K] != 0d)
                        {
                            var INP1 = K + 4L;
                            CREATHES(Baseknow.KHARID, K + 4L, 1, GETGRPKALA(Convert.ToInt32(INP1)));
                            var sharhGrp = Strings.Right("خريد " + GETGRPKALA(Convert.ToInt32(K + 4L)) + " فاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                            HS[7] += HS[(int)K];
                            batchQueries.Add($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{Baseknow.KHARID},{K + 4L},1,N'{Baseknow.KHARID + "-" + (K + 4L) + "-1"}',N'{SqlText(sharhGrp)}',{N(HS[(int)K])},{hRow.NUMBER},12,{arzdVal})");
                        }
                    }

                    var sharhPkharid = Strings.Right("خريدفاكتورشماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(hRow.CUST_NO), 255);
                    var besPk = N(KHSAY + KHSAKHT + KHNIM + KHMAVAV + BAZAR + HS[7]);
                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD ) VALUES ({max_ns},{Baseknow.PKHARID},1,1,N'{Baseknow.PKHARID + "-1-1"}',N'{SqlText(sharhPkharid)}',{besPk},{hRow.NUMBER},12,{arzdVal})");
                }

                if (hRow.MABL_HAZ != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhHazBes = Strings.Right("خدمات فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhHazBes)}',{N(hRow.MABL_HAZ)},{hRow.NUMBER},12,{arzdVal})");
                }

                if (hRow.M_NAGHD != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhNaghdCust = Strings.Right("مبلغ نقد فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhNaghdCust)}',{N(hRow.M_NAGHD)},{hRow.NUMBER},12,{arzdVal})");
                }

                if (hRow.MABL_HAV != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhHavCust = Strings.Right("مبلغ حواله فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhHavCust)}',{N(hRow.MABL_HAV)},{hRow.NUMBER},12,{arzdVal})");

                    if (!IsNull(hRow.MOIN_HAV))
                    {
                        GETTAF3(hRow.MOIN_HAV, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        var sharhHavMoin = Strings.Right("مبلغ حواله فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                        string HES_T2T_H = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                        string HES_T3T_H = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                        string HES_T4T_H = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({max_ns},{HKOL},{HMOIN},{HTAF},{HES_T2T_H},{HES_T3T_H},{HES_T4T_H},N'{hRow.MOIN_HAV}',N'{SqlText(sharhHavMoin)}',{N(hRow.MABL_HAV)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    else
                    {
                        LogWriter.WriteLog("خطا در برگه شماره سند خرید :" + hRow.NUMBER + " نوع :" + hRow.TAG + "حساب معين براي مبلغ حواله مشخص نشده است");
                    }
                }

                if (hRow.MABL_VAR != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhVarCust = Strings.Right("مبلغ واريزي فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhVarCust)}',{N(hRow.MABL_VAR)},{hRow.NUMBER},12,{arzdVal})");

                    if (!IsNull(hRow.MOIN_VAR))
                    {
                        GETTAF3(hRow.MOIN_VAR, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        var sharhVarMoin = Strings.Right("مبلغ واريزي فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                        string HES_T2T_V = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                        string HES_T3T_V = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                        string HES_T4T_V = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                        batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({max_ns},{HKOL},{HMOIN},{HTAF},{HES_T2T_V},{HES_T3T_V},{HES_T4T_V},N'{hRow.MOIN_VAR}',N'{SqlText(sharhVarMoin)}',{N(hRow.MABL_VAR)},{hRow.NUMBER},12,{arzdVal})");
                    }
                    else
                    {
                        LogWriter.WriteLog("خطا در برگه شمارهسند خرید  :" + hRow.NUMBER + " نوع :" + hRow.TAG + "حساب معين براي مبلغ واریزی مشخص نشده است");
                    }
                }

                if (hRow.M_NAGHD != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhNaghdSan = Strings.Right("مبلغ نقد فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);

                    batchQueries.Add($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD ) VALUES ({max_ns},{Baseknow.SANDOGH},{hRow.DEPATMAN},{hRow.SHIFT},N'{Baseknow.SANDOGH + "-" + hRow.DEPATMAN + "-" + hRow.SHIFT}',N'{SqlText(sharhNaghdSan)}',{N(hRow.M_NAGHD)},{hRow.NUMBER},12,{arzdVal})");
                }

                if (hRow.TAKHFIF != 0)
                {
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhTakhCust = Strings.Right("مبلغ تخفيف فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.CUST_NO}',N'{SqlText(sharhTakhCust)}',{N(hRow.TAKHFIF)},{hRow.NUMBER},12,{arzdVal})");

                    var sharhTakhKh = Strings.Right("مبلغ تخفيف فاكتور خريد  شماره " + hRow.NUMBER1 + "-" + hRow.FNUMCO + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    batchQueries.Add($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({max_ns},{Baseknow.TKHARID},1,1,N'{Baseknow.TKHARID + "-1-1"}',N'{SqlText(sharhTakhKh)}',{N(hRow.TAKHFIF)},{hRow.NUMBER},12,{arzdVal})");
                }

                if (hRow.MBAA != 0)
                {
                    if (!IsNull(hRow.HMBAA))
                    {
                        GETTAF3(hRow.HMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }
                    var arzdVal = IsNull(hRow.ARZD) ? "1" : N(hRow.ARZD);
                    var sharhMbaa = Strings.Right(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور خريد شماره " + hRow.NUMBER1 + " مورخ" + Strings.Format(hRow.DATE_N, "####/##/##"), 255);
                    string HES_T2T = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                    batchQueries.Add($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({max_ns},{HKOL},{HMOIN},{HTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{hRow.HMBAA}',N'{SqlText(sharhMbaa)}',{N(hRow.MBAA)},{hRow.NUMBER},12,{arzdVal})");
                }

                // اجرای یکباره و دسته‌ای تمام آرتیکل‌های این فاکتور خرید.
                // ⚠️ همه‌ی دستورها (شامل DELETE اول) در یک تراکنش اجرا می‌شوند تا اگر بخشی
                //    از دسته خطا بدهد، سند قبلی نیمه‌پاک‌شده باقی نماند و تلاش مجدد
                //    DoExecuteSQL هم ردیف تکراری نسازد.
                if (batchQueries.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                    foreach (var q in batchQueries) { sb.Append(q).Append(';'); }
                    sb.Append("COMMIT TRANSACTION;");
                    dbms.DoExecuteSQL(sb.ToString());
                }

                progressReporter.ReportOne();
            });

            progressReporter.Complete();
            LogWriter.WriteLog("پایان بازسازی سند خرید");
            return _SANAD_NUMBER;
        }
        public static string GETGRPKALA(int CC)
        {
            var rst = dbms.DoGetDataSQL<string>("SELECT     NAMES  FROM dbo.TCOD_STUFGROUP WHERE     (CODE = " + System.Convert.ToString(CC) + ")").ToList();
            if (rst.Count > 0)
            {
                return System.Convert.ToString(rst.FirstOrDefault());
            }
            return null;
        }
        public static double GETSTANDARDPRICE_KOL(string CODE, long dt)
        {
            double tempGETSTANDARDPRICE_KOL = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var RST = dbms.DoGetDataSQL<QRE_18>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").FirstOrDefault();
                if (!(RST is null))
                {
                    tempGETSTANDARDPRICE_KOL = (double)(RST.SumOfMABLK + RST.SumOfIMBIBE_MANF + RST.SumOfIMBIBE_SAR);
                }
                else
                {
                    tempGETSTANDARDPRICE_KOL = 0;
                }
            }
            else
            {
                var RST = dbms.DoGetDataSQL<QRE_18>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(RST is null))
                {
                    tempGETSTANDARDPRICE_KOL = (double)(RST.SumOfMABLK + RST.SumOfIMBIBE_MANF + RST.SumOfIMBIBE_SAR);
                }
                else
                {
                    tempGETSTANDARDPRICE_KOL = 0;
                }
            }
            return tempGETSTANDARDPRICE_KOL;
        }
        public static double GETSTANDARDPRICE(string code)
        {
            double standardPrice = 0;
            string query = "SELECT TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE (dbo.HEAD_MANF.CODE = N'" + code + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB";
            var rst = dbms.DoGetDataSQL<GENERSANAD_QRE1>(query).ToList();

            if (rst.Count > 0)
            {
                standardPrice = Convert.ToDouble(rst.FirstOrDefault().SumOfMABLK) + Convert.ToDouble(rst.FirstOrDefault().SumOfIMBIBE_SAR) +
                    Convert.ToDouble(rst.FirstOrDefault().SumOfIMBIBE_MANF);
            }
            return standardPrice;
        }
        public static long GETFIRSTPRICE(string code)
        {
            long firstPrice = 0;
            var rrst = dbms.DoGetDataSQL<GETFIRSTPRICE_QRE2>("SELECT TOP 1 CODE, FI_A FROM dbo.STUF_FSK WHERE code = '" + code + "' ORDER BY FI_A DESC").ToList();

            if (rrst.Count > 0)
            {
                if (Convert.IsDBNull(rrst.FirstOrDefault().FI_A))
                {
                    firstPrice = 0;
                }
                else
                {
                    firstPrice = Convert.ToInt64(rrst.FirstOrDefault().FI_A);
                }
            }
            return firstPrice;
        }
        public static long PersianDateLong(DateTime miladiDate)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(miladiDate);
            int month = persianCalendar.GetMonth(miladiDate);
            int day = persianCalendar.GetDayOfMonth(miladiDate);

            // تبدیل تاریخ به یک عدد long با فرمت yyyyMMdd
            long persianDateLong = year * 10000 + month * 100 + day;

            return persianDateLong;
        }
        public static int GETGRPKALAco(string CC)
        {
            // «خواندن خالص» از STUF_DEF است و در طول یک بازسازی تغییر نمی‌کند، ولی به‌ازای هر قلم
            // هر برگه صدا زده می‌شود؛ پس تکرارش بسیار زیاد است.
            var groupKey = CC ?? string.Empty;
            if (LookupCacheEnabled && _kalaGroupCache.TryGetValue(groupKey, out var cachedGroup))
            {
                return cachedGroup;
            }

            int GETGRPKALAcoRet = default;
            var rst = dbms.DoGetDataSQL<double?>("SELECT     radah  FROM dbo.stuf_def WHERE     (CODE = N'" + SqlText(CC) + "')").ToList();
            if (rst.Count > 0)
            {
                // اگر RADAH خالی باشد، (int) روی double? خطا می‌داد و کل تسک را از کار می‌انداخت.
                // مقدار پیش‌فرض همان صفر است که در ادامه به معین «۱» ترجمه می‌شود — یعنی همان
                // رفتاری که برای گروه‌های غیر از ۲ و ۳ وجود دارد.
                var radah = rst.FirstOrDefault();
                GETGRPKALAcoRet = radah.HasValue ? (int)radah.Value : 0;
            }

            if (LookupCacheEnabled)
            {
                _kalaGroupCache[groupKey] = GETGRPKALAcoRet;
            }

            return GETGRPKALAcoRet;
        }

        //AUTO_BAZ_FUNCTIONS ---------------------------------------------------------------------------------------------------------
        public static (double?, bool) GENSANADKHAZ(object fnum, long TNUM, bool InternalCalling = true, IDbTransaction externalTransaction = null)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            bool useExternal = externalTransaction != null;
            CL_ConcurrencyManager cnnManager = null;

            if (useExternal)
            {
                // استفاده از سازنده‌ای که اتصال externalTransaction را دریافت می‌کند
                cnnManager = new CL_ConcurrencyManager(externalTransaction.Connection);
                cnnManager.BeginTransaction(externalTransaction);
            }
            else
            {
                if (!useExternal)
                {
                    cnnManager = new CL_ConcurrencyManager(CL_CCNNMANAGER.CONNECTION_STR, true);
                    cnnManager.BeginTransaction();  // (در نسخه فعلی این خط ممکن است کامنت باشد)

                    //البته این کار نیازی نیست چون توی خط بالا بهش اعلام کردیم که بایدحالت کانکشن لحظه ای کار کنه
                    cnnManager.SetTransctionStating(true); //برای اینکه بتونه Paralle کار کنه و خطای Multiple DataRedaer نده 
                }
            }

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            var HFRST = cnnManager.SqlQuery<PGET_HED>($"SELECT * FROM dbo.PGET_HED WHERE (ID BETWEEN {fnum} AND {TNUM}) ORDER BY ID").ToList();

            LogWriter.WriteLog($"شروع باز سازي سند های خزانه از سند شماره :{fnum} تا سند شماره :{TNUM}");

            static string BuildKhazSharh(PGET_HED row)
                => "خزانه داري شماره " + row.ID + " مورخ " + Strings.Format(row.DATE, "####/##/##");

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۱ (سریال، فقط چند کوئری): تشخیص اینکه کدام رکوردها هدر سند دارند.
            // قبلاً این کار داخل حلقه و به‌ازای هر رکورد یک SELECT جدا بود (N رفت‌وبرگشت به سرور).
            // ───────────────────────────────────────────────────────────────────────────────
            var existingHeaderNumbers = new HashSet<double>();
            var candidateNumbers = HFRST
                .Where(row => row?.N_S != null)
                .Select(row => row.N_S.Value)
                .Distinct()
                .ToList();

            if (candidateNumbers.Count > 0)
            {
                var fromNs = SqlNum(candidateNumbers.Min());
                var toNs = SqlNum(candidateNumbers.Max());
                foreach (var found in cnnManager.SqlQuery<double?>(
                    $"SELECT N_S FROM DEED_HED WHERE NO_S = 5 AND N_S BETWEEN {fromNs} AND {toNs}"))
                {
                    if (found.HasValue)
                    {
                        existingHeaderNumbers.Add(found.Value);
                    }
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۲ (سریال، فقط یک تراکنش): رزرو دسته‌ای همه‌ی شماره سندهای لازم.
            // قبلاً به‌ازای هر رکورد یک بار Createsanad صدا زده می‌شد که کل جدول DEED_HED را
            // با Serializable قفل می‌کرد؛ همین تنها عامل کافی بود تا حلقه‌ی Parallel سریال شود.
            // ───────────────────────────────────────────────────────────────────────────────
            var needsNewHeader = new bool[HFRST.Count];
            var newHeaderIndexes = new List<int>();

            // هر شماره سند فقط می‌تواند به یک ردیف خزانه تعلق داشته باشد.
            // اگر چند ردیف N_S یکسان داشته باشند (که دقیقاً پیامد باگ قبلیِ
            // «UPDATE ... WHERE ID BETWEEN» است و می‌تواند در داده‌ی فعلی وجود داشته باشد)،
            // فقط اولین ردیف مالک آن شماره می‌ماند و بقیه شماره سند تازه می‌گیرند.
            // بدون این کار، دو Thread موازی روی یک N_S یکسان
            // «DELETE FROM DEED_DTL WHERE N_S = x» و سپس INSERT می‌زدند و
            // جزئیات یکدیگر را پاک می‌کردند (Overlap واقعی روی یک سند).
            var claimedNumbers = new HashSet<double>();
            var duplicateNumberCount = 0;

            for (int i = 0; i < HFRST.Count; i++)
            {
                var row = HFRST[i];
                if (row == null)
                {
                    continue;
                }

                var headerExists = row.N_S != null && existingHeaderNumbers.Contains(row.N_S.Value);
                var ownsHeader = headerExists && claimedNumbers.Add(row.N_S.Value);

                if (headerExists && !ownsHeader)
                {
                    duplicateNumberCount++;
                }

                if (!ownsHeader)
                {
                    needsNewHeader[i] = true;
                    newHeaderIndexes.Add(i);
                }
            }

            if (duplicateNumberCount > 0)
            {
                LogWriter.WriteLog(
                    $"سند خزانه - هشدار: {duplicateNumberCount} ردیف خزانه شماره سند تکراری داشتند " +
                    "(احتمالاً باقی‌مانده از باگ قبلی UPDATE بازه‌ای)؛ برای هرکدام شماره سند جدید ساخته شد.");
            }

            if (newHeaderIndexes.Count > 0)
            {
                var headerRequests = newHeaderIndexes
                    .Select(i => new SanadHeaderRequest
                    {
                        DATE_S = Convert.ToInt64(HFRST[i].DATE),
                        SHARH_S = BuildKhazSharh(HFRST[i]),
                        GHATEI = 0,
                        NO_S = 5,
                        OKF = 1,
                        USER_NAME = HFRST[i].USER_NAME
                    })
                    .ToList();

                var reservedNumbers = ReserveSanadNumbersBatch(headerRequests);
                for (int k = 0; k < newHeaderIndexes.Count; k++)
                {
                    HFRST[newHeaderIndexes[k]].N_S = reservedNumbers[k];
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۳ (موازی): کار هر سند کاملاً مستقل از بقیه است و هیچ قفل سراسری ندارد.
            // ───────────────────────────────────────────────────────────────────────────────
            var progressReporter = new ThrottledProgressReporter(
                HFRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    // Math.Max لازم است: گزارش‌ها با BeginInvoke از چند Thread صف می‌شوند و
                    // ممکن است بی‌ترتیب اجرا شوند (مثلاً ۴۶٪ قبل از ۴۵٪). بدون این، نوار پیشرفت
                    // گاهی به عقب می‌پرد.
                    auto_run.PRGR_C3.Value = Math.Max(auto_run.PRGR_C3.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            //for (int EOFi = 0; EOFi < HFRST.Count; EOFi++)
            // با تراکنش بیرونی همه‌ی دستورها روی «یک» Connection مشترک اجرا می‌شوند و
            // استفاده همزمان چند Thread از یک SqlConnection مجاز نیست؛ پس در آن حالت سریال اجرا می‌کنیم.
            // (فراخوانی فعلی از فرم خزانه فقط یک رکورد می‌فرستد، ولی این محافظ برای آینده لازم است.)
            var dbParallelOptions = useExternal
                ? new ParallelOptions { MaxDegreeOfParallelism = 1 }
                : CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observedThreads = new System.Collections.Concurrent.ConcurrentDictionary<int, byte>();

            LogWriter.WriteLog(
                $"سند خزانه - تعداد رکورد: {HFRST.Count} | هدر جدید: {newHeaderIndexes.Count} | " +
                $"موازی: {Generaly.UseParallelProcessing} | MaxDegreeOfParallelism: {dbParallelOptions.MaxDegreeOfParallelism}");

            // متن SQL «یک بار» ساخته می‌شود و برای همه‌ی ردیف‌ها ثابت می‌ماند؛ فقط پارامترها عوض می‌شوند.
            // این نکته برای موازی‌سازی مهم است: با تولید متن SQL جداگانه برای هر ردیف، SQL Server
            // مجبور می‌شد برای هر سند یک Execution Plan تازه Compile کند. Compile شدن روی
            // Plan Cache قفل می‌گیرد و همین موضوع Thread های موازی را دوباره پشت هم صف می‌کند.
            // با پارامتری کردن، فقط دو Plan ساخته و بین همه‌ی Thread ها بازاستفاده می‌شود.
            var txPrefix = useExternal ? string.Empty : "SET XACT_ABORT ON; BEGIN TRANSACTION;";
            var txSuffix = useExternal ? string.Empty : "COMMIT TRANSACTION;";
            //اضافه شدن ستون نوع ارز به خزانه در صورت فعال بودن نرخ ارز
            const string detailInsertSql =
                         "INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BED, N_SERI, BANK, N_S, HES, ARZD, ARZKIND2, MHAZ_NO) " +
                         "SELECT THES_K, THES_M, THES_T, THES_T2, THES_T3, THES_T4, SHARH, MABL, N_SERI, BANK, @Ns, THES, ARZD, ARZKIND2, MHAZ_NO " +
                "FROM dbo.PGET_LST WHERE ID = @TreasuryId;" +
                         "INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BES, N_SERI, BANK, N_S, HES, ARZD, ARZKIND2, MHAZ_NO) " +
                         "SELECT FHES_K, FHES_M, FHES_T, FHES_T2, FHES_T3, FHES_T4, SHARH, MABL, N_SERI, BANK, @Ns, FHES, ARZD, ARZKIND2, MHAZ_NO " +
                "FROM dbo.PGET_LST WHERE ID = @TreasuryId;";

            // حالت الف) هدر سند در مرحله ۲ ساخته شده؛ اینجا فقط شماره‌اش روی ردیف خزانه ثبت
            // و ردیف‌های سند درج می‌شوند.
            // این همان جایی است که باگ اصلی بود: شرط قبلی «WHERE (ID BETWEEN fnum AND TNUM)» بود،
            // یعنی در هر تکرار کل بازه (عملاً کل جدول PGET_HED) با یک شماره سند بازنویسی می‌شد؛
            // هم داده را خراب می‌کرد و هم با قفل انحصاری روی کل جدول، Thread های موازی را به صف می‌کرد.
            // DELETE هم لازم نیست: شماره سند تازه از MAX(N_S)+1 آمده و قطعاً ردیفی در DEED_DTL ندارد.
            var newHeaderSql = txPrefix +
                "UPDATE dbo.PGET_HED SET N_S = @Ns WHERE ID = @TreasuryId;" +
                detailInsertSql + txSuffix;

            // حالت ب) هدر سند از قبل وجود دارد؛ سربرگ به‌روز و ردیف‌های قبلی‌اش بازسازی می‌شوند.
            //
            // DELETE عمداً همین‌جا داخل حلقه‌ی موازی است و نه یکجا قبل از حلقه:
            // روی DEED_DTL ایندکس N_SI روی ستون N_S وجود دارد، پس پیدا کردن ردیف‌ها ارزان است.
            // هزینه‌ی واقعی حذف، نگهداری ۷ ایندکس این جدول است — و آن هزینه چه یکجا انجام شود
            // چه پراکنده، مقدارش یکسان است. پس بهتر است بین Thread ها پخش شود تا اینکه
            // سریال و پیش از حلقه انجام شود. ضمناً این‌طور DELETE و INSERT دوباره در یک
            // تراکنش قرار می‌گیرند و سند هیچ‌وقت بی‌ردیف یا نیمه‌کاره دیده نمی‌شود.
            //
            // توجه: BAYEG و base دست نمی‌خورند — شماره بایگانی و شناسه رهگیری مالیاتی باید ثابت بمانند.
            var existingHeaderSql = txPrefix +
                "UPDATE dbo.DEED_HED SET DATE_S = @DateS, SHARH_S = @Sharh, USER_NAME = @UserName, OKF = 1 " +
                "WHERE NO_S = 5 AND N_S = @Ns;" +
                "DELETE FROM dbo.DEED_DTL WHERE N_S = @Ns;" +
                detailInsertSql + txSuffix;

            ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, EOFi =>
            {
                observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                var row = HFRST[EOFi];
                if (row == null)
                {
                    progressReporter.ReportOne();
                    return;
                }

                // همه‌ی دستورهای این سند در «یک» رفت‌وبرگشت به سرور فرستاده می‌شوند.
                // قبلاً ۴ تا ۵ فراخوانی جدا بود و چون CL_ConcurrencyManager در حالت OnceStartCloseQuery
                // برای هر فراخوانی یک Connection باز/بسته می‌کند، هزینه‌ی شبکه چند برابر می‌شد.
                string sql;
                object parameters;

                if (needsNewHeader[EOFi])
                {
                    sql = newHeaderSql;
                    parameters = new { Ns = row.N_S, TreasuryId = row.ID };
                }
                else
                {
                    sql = existingHeaderSql;
                    parameters = new
                    {
                        Ns = row.N_S,
                        TreasuryId = row.ID,
                        DateS = row.DATE,
                        Sharh = BuildKhazSharh(row),
                        // مثل کد قبلی، نبودِ نام کاربر به رشته خالی تبدیل می‌شود نه NULL.
                        UserName = row.USER_NAME ?? string.Empty
                    };
                }

                if (useExternal)
                {
                    // تراکنش بیرونی پس از Deadlock قابل ادامه نیست، پس Retry نمی‌کنیم.
                    cnnManager.ExecuteSqlCommand(sql, parameters);
                }
                else
                {
                    ExecuteWithDeadlockRetry(() => cnnManager.ExecuteSqlCommand(sql, parameters));
                }

                progressReporter.ReportOne();
            });

            stopwatch.Stop();
            progressReporter.Complete();

            // این خط لاگ دقیقاً به همان سوال جواب می‌دهد: واقعاً چند Thread درگیر شدند و چقدر طول کشید.
            LogWriter.WriteLog(
                $"پايان باز سازي سند های خزانه - {HFRST.Count} رکورد در {stopwatch.Elapsed.TotalSeconds:F1} ثانیه " +
                $"با {observedThreads.Count} Thread همزمان");

            // مثل حالت سریال، شماره سند آخرین ردیف پردازش‌شده برگردانده می‌شود
            // (قبلاً این مقدار از داخل حلقه‌ی موازی نوشته می‌شد و نتیجه‌اش غیرقطعی بود).
            SANAD_NUMBER = HFRST.LastOrDefault(row => row?.N_S != null)?.N_S;

            if (!useExternal && !cnnManager.OnceStartCloseQuery)
            {
                cnnManager.Commit();
            }
            //};

            return (SANAD_NUMBER, IsSuccessfully);
        }

        private static bool TryGetDateNumber(object? dateValue, out long result)
        {
            result = 0;

            if (dateValue is null)
            {
                return false;
            }

            switch (dateValue)
            {
                case long longValue:
                    result = longValue;
                    return true;
                case int intValue:
                    result = intValue;
                    return true;
                case short shortValue:
                    result = shortValue;
                    return true;
                case double doubleValue:
                    result = Convert.ToInt64(doubleValue);
                    return true;
                case decimal decimalValue:
                    result = Convert.ToInt64(decimalValue);
                    return true;
                case float floatValue:
                    result = Convert.ToInt64(floatValue);
                    return true;
                case DateTime dateTimeValue:
                    var persianCalendar = new PersianCalendar();
                    var normalizedDate = $"{persianCalendar.GetYear(dateTimeValue):0000}{persianCalendar.GetMonth(dateTimeValue):00}{persianCalendar.GetDayOfMonth(dateTimeValue):00}";
                    if (long.TryParse(normalizedDate, out result))
                    {
                        return true;
                    }

                    return false;
                case string stringValue:
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        return false;
                    }

                    if (long.TryParse(stringValue, out result))
                    {
                        return true;
                    }

                    var digitsOnly = new string(stringValue.Where(char.IsDigit).ToArray());
                    if (digitsOnly.Length > 0 && long.TryParse(digitsOnly, out result))
                    {
                        return true;
                    }

                    return false;
            }

            var converted = Convert.ToString(dateValue, CultureInfo.InvariantCulture);
            return converted != null && long.TryParse(converted, out result);
        }

        public static (double?, bool) SANADENTEGHAL(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            bool valdefacc = true;
            if (InternalCalling && auto_run != null)
            {
                auto_run.Dispatcher.Invoke(new Action(() =>
                {
                    valdefacc = Convert.ToBoolean(auto_run.defacc.IsChecked);
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>($"SELECT NUMBER, TAG, ANBAR, NUMBER1, DATE_N, TAH, MAS, VAS, N_S, CUST_NO, MOLAH, M_NAGHD, MABL_VAR, MOIN_VAR, MABL_HAV, MOIN_HAV, MABL_HAZ, MOIN_HAZ, TAKHFIF, MOIN_KHF, ANBARF, FNUMCO, DEPATMAN, SHIFT, CUST_KIND, USER_NAME FROM dbo.HEAD_LST WHERE NUMBER >= {NUMBER} AND NUMBER <= {NUMBER2} AND TAG = 5 ORDER BY NUMBER").ToList();
            LogWriter.WriteLog($"SANADENTEGHAL: شروع بازسازی اسناد انتقالی از شماره {NUMBER} تا {NUMBER2} - تعداد برگه‌ها: {HEDRST.Count}");

            var progressReporter = new ThrottledProgressReporter(
                HEDRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    auto_run.PRGR_C4.Value = Math.Max(auto_run.PRGR_C4.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            if (HEDRST.Count == 0)
            {
                progressReporter.Complete();
                return (SANAD_NUMBER, IsSuccessfully);
            }

            // ۱) پیش‌خوانی نوع انبارها (به‌جای یک کوئری به‌ازای هر برگه)
            var anbarKindMap = new Dictionary<int, int?>();
            foreach (var a in dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT CODE, KIND FROM dbo.TCOD_ANBAR"))
            {
                if (a?.CODE != null && !anbarKindMap.ContainsKey(a.CODE.Value)) { anbarKindMap[a.CODE.Value] = a.KIND; }
            }

            // ۲) پیش‌خوانی اقلام برگه‌های انتقالی (INVO_LST + STUF_DEF) با یک کوئری
            var sheetNumbers = HEDRST.Where(h => h?.NUMBER != null).Select(h => h.NUMBER.Value).ToList();
            var minNum = SqlNum(sheetNumbers.Min());
            var maxNum = SqlNum(sheetNumbers.Max());

            var invoiceLinesMap = new Dictionary<double, List<QRE_BAZ_1>>();
            var wantedSheets = new HashSet<double>(sheetNumbers);

            foreach (var line in dbms.DoGetDataSQL<QRE_BAZ_1>(
                $"SELECT INVO_LST.NUMBER, INVO_LST.TAG, STUF_DEF.NAME, INVO_LST.ANBAR, INVO_LST.CODE, INVO_LST.MEGH, INVO_LST.MEGHk, INVO_LST.MEGH_MAR, INVO_LST.MABL, INVO_LST.MABL_K, INVO_LST.ANBARF " +
                $"FROM dbo.STUF_DEF INNER JOIN dbo.INVO_LST ON STUF_DEF.CODE = INVO_LST.CODE " +
                $"WHERE INVO_LST.TAG = 5 AND INVO_LST.NUMBER BETWEEN {minNum} AND {maxNum}"))
            {
                if (line?.NUMBER == null || !wantedSheets.Contains(line.NUMBER.Value)) { continue; }
                var key = line.NUMBER.Value;
                if (!invoiceLinesMap.TryGetValue(key, out var list))
                {
                    list = new List<QRE_BAZ_1>();
                    invoiceLinesMap[key] = list;
                }
                list.Add(line);
            }

            // ۳) پیش‌ساخت حساب‌های انبار مبدأ و مقصد (اگر «ساخت حساب‌های نبوده» فعال است)
            if (valdefacc)
            {
                var accountsToEnsure = new HashSet<(double? Kol, double? Moin, string? Code, string? Name)>();
                foreach (var lineList in invoiceLinesMap.Values)
                {
                    foreach (var l in lineList)
                    {
                        if (l.MABL_K != 0 && !string.IsNullOrEmpty(l.CODE))
                        {
                            if (l.ANBAR.HasValue) { accountsToEnsure.Add((Baseknow.MOGODIA, l.ANBAR.Value, l.CODE, l.NAME)); }
                            if (l.ANBARF.HasValue) { accountsToEnsure.Add((Baseknow.MOGODIA, l.ANBARF.Value, l.CODE, l.NAME)); }
                        }
                    }
                }

                foreach (var acc in accountsToEnsure)
                {
                    try
                    {
                        CREATHES(acc.Kol, acc.Moin, Convert.ToInt64(acc.Code), acc.Name);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteLog($"SANADENTEGHAL: خطا در پیش‌ساخت حساب کالا {acc.Code} در انبار {acc.Moin}: {ex.Message}");
                    }
                }
            }

            // ۴) پردازش موازی برگه‌ها
            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, rw =>
            {
                var hRow = HEDRST[rw];
                if (hRow?.NUMBER == null)
                {
                    progressReporter.ReportOne();
                    return;
                }

                double? max_ns = null;

                // انبارهای نوع ۱ و ۲ در حالت غیرصنعتی سند نمی‌گیرند (عیناً مثل کد قبلی)
                if (anbarKindMap.TryGetValue(hRow.ANBAR ?? 0, out var kind) && (kind == 1 || kind == 2))
                {
                    if (!(Baseknow.SANAT == true || IsNull(Baseknow.SANAT)))
                    {
                        dbms.DoExecuteSQL($"DELETE FROM dbo.DEED_DTL WHERE NUMBER = {hRow.NUMBER} AND TAG = 5");
                        progressReporter.ReportOne();
                        return;
                    }
                }

                var sharhS = Strings.Left(" حواله انتقالي مواد شماره " + hRow.NUMBER + "-" + hRow.FNUMCO + " از انبار " + hRow.ANBAR + " به " + hRow.ANBARF + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##"), 100);

                if (hRow.N_S == null)
                {
                    max_ns = Createsanad(Convert.ToInt64(hRow.DATE_N), sharhS, 0, 10, Convert.ToByte(true), hRow.USER_NAME);
                    hRow.N_S = max_ns;
                    // ⚠️ این UPDATE در کد قبلی نبود: شماره سند روی برگه ثبت نمی‌شد و هر اجرا
                    //    دوباره یک سند تازه می‌ساخت (رشد بی‌پایان DEED_HED).
                    dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET N_S = {max_ns} WHERE NUMBER = {hRow.NUMBER} AND TAG = 5");
                }
                else
                {
                    max_ns = hRow.N_S;
                    var SARST = dbms.DoGetDataSQL<DEED_HED_CSHARP>($"SELECT * FROM dbo.DEED_HED WHERE NO_S = 10 AND N_S = {max_ns}").FirstOrDefault();
                    if (SARST != null)
                    {
                        dbms.DoExecuteSQL($"UPDATE dbo.DEED_HED SET DATE_S = {hRow.DATE_N}, SHARH_S = N'{SqlText(sharhS)}', GHATEI = 0, NO_S = 10, OKF = 1, USER_NAME = N'{SqlText(hRow.USER_NAME)}' WHERE NO_S = 10 AND N_S = {max_ns}");
                    }
                    else
                    {
                        max_ns = Createsanad(Convert.ToInt64(hRow.DATE_N), sharhS, 0, 10, Convert.ToByte(true), hRow.USER_NAME);
                        hRow.N_S = max_ns;
                        dbms.DoExecuteSQL($"UPDATE dbo.HEAD_LST SET N_S = {max_ns} WHERE NUMBER = {hRow.NUMBER} AND TAG = 5");
                    }
                }

                SANAD_NUMBER = max_ns;

                var batchQueries = new List<string>
                {
                    $"DELETE FROM dbo.DEED_DTL WHERE NUMBER = {hRow.NUMBER} AND TAG = 5"
                };

                if (invoiceLinesMap.TryGetValue(hRow.NUMBER.Value, out var lines))
                {
                    foreach (var line in lines)
                    {
                        if (line.MABL_K != 0)
                        {
                            // بستانکار: انبار مبدأ
                            var hesBes = Baseknow.MOGODIA + "-" + line.ANBAR + "-" + line.CODE;
                            var sharhBes = Strings.Left("حواله انتقالي شماره " + hRow.NUMBER + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + " به مقدار" + line.MEGHk, 255);
                            batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES, SHARH, BES, NUMBER, TAG) VALUES ({max_ns},{Baseknow.MOGODIA},{line.ANBAR},{line.CODE},N'{SqlText(hesBes)}',N'{SqlText(sharhBes)}',{Math.Round((double)line.MABL_K)},{hRow.NUMBER},5)");

                            // بدهکار: انبار مقصد
                            var hesBed = Baseknow.MOGODIA + "-" + line.ANBARF + "-" + line.CODE;
                            var sharhBed = Strings.Left("حواله انتقالي شماره " + hRow.NUMBER + "-" + hRow.FNUMCO + " مورخ " + Strings.Format(hRow.DATE_N, "####/##/##") + " به مقدار" + line.MEGHk + "  بابت " + line.NAME, 255);
                            batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES, SHARH, BED, NUMBER, TAG) VALUES ({max_ns},{Baseknow.MOGODIA},{line.ANBARF},{line.CODE},N'{SqlText(hesBed)}',N'{SqlText(sharhBed)}',{Math.Round((double)line.MABL_K)},{hRow.NUMBER},5)");
                        }
                    }
                }

                // همه‌ی دستورهای این برگه در «یک» تراکنش، تا سند هیچ‌وقت نیمه‌کاره دیده نشود.
                var sb = new StringBuilder();
                sb.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                foreach (var q in batchQueries) { sb.Append(q).Append(';'); }
                sb.Append("COMMIT TRANSACTION;");
                dbms.DoExecuteSQL(sb.ToString());

                progressReporter.ReportOne();
            });

            progressReporter.Complete();
            LogWriter.WriteLog($"پایان سند انتقال با موفقیت :{DateTime.Now}");
            return (SANAD_NUMBER, IsSuccessfully);
        }
        /// <summary>
        /// بازسازی «سند حواله خروج مواد از انبار» — برگه‌های HEAD_LST با TAG = 10 و سند نوع NO_S = 8.
        ///
        /// <para>
        /// ساختار این متد عمداً همان ساختار سه‌مرحله‌ای <see cref="GENSANADKHAZ"/> است:
        /// مرحله‌های ۱ و ۲ سریال‌اند ولی فقط چند کوئریِ کل‌نگر می‌زنند، مرحله ۳ کاملاً موازی است و
        /// هیچ قفل سراسری ندارد، و مرحله ۴ («کسر دهم ریال») دوباره با چند کوئری کل‌نگر انجام می‌شود.
        /// </para>
        /// </summary>
        public static (double?, bool) SANADKHORUGMAVAD(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            bool valdefacc = true;
            if (InternalCalling && auto_run != null)
            {
                auto_run.Dispatcher.Invoke(new Action(() =>
                {
                    valdefacc = Convert.ToBoolean(auto_run.defacc.IsChecked);
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= " + NUMBER + " AND HEAD_LST.NUMBER <=" + NUMBER2 + "  and HEAD_LST.tag = 10 ) ) ORDER BY HEAD_LST.NUMBER").ToList();
            LogWriter.WriteLog("SANADKHORUGMAVAD: شروع بازسازی از برگ شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + " " + DateTime.Now);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observedThreads = new System.Collections.Concurrent.ConcurrentDictionary<int, byte>();

            // شرح سربرگ سند دقیقاً همان متنی است که کد قبلی می‌ساخت (تا سندهای موجود عوض نشوند).
            static string BuildKhorugSharhS(QRE_BAZ_0 hedRow)
                => Strings.Left(" حواله خروج مواد از انبار شماره " + hedRow.NUMBER + "-" + hedRow.FNUMCO + " مورخ " + Strings.Format(hedRow.DATE_N, "####/##/##"), 100);

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۰ (در حافظه): نرمال‌سازی تاریخ. برگه‌ی با تاریخ نامعتبر از کل پردازش کنار
            // گذاشته می‌شود — همان کاری که return داخل حلقه‌ی قبلی می‌کرد.
            // ───────────────────────────────────────────────────────────────────────────────
            var sheetUsable = new bool[HEDRST.Count];
            for (int i = 0; i < HEDRST.Count; i++)
            {
                var sheet = HEDRST[i];
                if (sheet == null || sheet.NUMBER == null)
                {
                    continue;
                }

                if (!TryGetDateNumber(sheet.DATE_N, out var normalizedDate))
                {
                    LogWriter.WriteLog($"SANADKHORUGMAVAD: تاریخ نامعتبر برای برگ {sheet.NUMBER} با مقدار '{sheet.DATE_N}'.");
                    IsSuccessfully = false;
                    continue;
                }

                // DEED_HED یک CHECK دارد: CK_DEED_HED => date_s >= 10101
                // ولی HEAD_LST.DATE_N هیچ CHECK ندارد، پس مقدار خراب (مثلاً صفر) در آن قابل ذخیره است.
                // اگر چنین برگه‌ای وارد رزرو دسته‌ای شود، INSERT سربرگ به CHECK می‌خورد و چون
                // رزرو برای «همه‌ی» برگه‌ها در یک تراکنش انجام می‌شود، یک ردیف خراب کل سند خروج مواد
                // را از کار می‌انداخت. پس همان‌جا کنار گذاشته می‌شود.
                if (normalizedDate < 10101)
                {
                    LogWriter.WriteLog(
                        $"SANADKHORUGMAVAD: تاریخ برگ {sheet.NUMBER} برابر {normalizedDate} است و از حداقل مجاز سند (10101) کمتر می‌باشد؛ این برگه پردازش نشد.");
                    IsSuccessfully = false;
                    continue;
                }

                sheet.DATE_N = normalizedDate;
                sheetUsable[i] = true;
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۱ (سریال، فقط یک کوئری): تشخیص اینکه کدام برگه‌ها از قبل سربرگ سند نوع ۸ دارند.
            // قبلاً این کار داخل حلقه و به‌ازای هر برگه یک «SELECT * FROM DEED_HED» جدا بود.
            // ───────────────────────────────────────────────────────────────────────────────
            var existingHeaderNumbers = new HashSet<double>();
            var candidateNumbers = new List<double>();
            for (int i = 0; i < HEDRST.Count; i++)
            {
                if (!sheetUsable[i]) { continue; }
                var ns = HEDRST[i].N_S;
                if (ns != null && ns.Value != 0)
                {
                    candidateNumbers.Add(ns.Value);
                }
            }

            if (candidateNumbers.Count > 0)
            {
                var fromNs = SqlNum(candidateNumbers.Min());
                var toNs = SqlNum(candidateNumbers.Max());
                foreach (var found in dbms.DoGetDataSQL<double?>(
                    $"SELECT N_S FROM DEED_HED WHERE NO_S = 8 AND N_S BETWEEN {fromNs} AND {toNs}"))
                {
                    if (found.HasValue)
                    {
                        existingHeaderNumbers.Add(found.Value);
                    }
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۲ (سریال، یک تراکنش): رزرو دسته‌ای همه‌ی شماره سندهای لازم.
            // قبلاً به‌ازای هر برگه یک بار Createsanad صدا زده می‌شد که کل جدول DEED_HED را با
            // Serializable قفل می‌کرد؛ همین تنها عامل کافی بود تا حلقه‌ی Parallel سریال شود.
            //
            // هر شماره سند فقط می‌تواند به یک برگه تعلق داشته باشد. اگر چند برگه N_S یکسان داشته
            // باشند فقط اولی مالک آن می‌ماند و بقیه شماره تازه می‌گیرند؛ وگرنه دو Thread موازی
            // روی یک سند، ردیف‌های یکدیگر را پاک می‌کردند.
            //
            // نکته: اگر N_S برگه به هیچ سربرگ نوع ۸ اشاره نکند (سربرگ حذف شده یا نوع دیگری است)،
            // مثل GENSANADKHAZ شماره سند تازه گرفته می‌شود. کد قبلی در این حالت سربرگ نمی‌ساخت و
            // ردیف‌ها را به سندی می‌چسباند که وجود نداشت.
            // ───────────────────────────────────────────────────────────────────────────────
            var needsNewHeader = new bool[HEDRST.Count];
            var newHeaderIndexes = new List<int>();
            var claimedNumbers = new HashSet<double>();
            var duplicateNumberCount = 0;

            for (int i = 0; i < HEDRST.Count; i++)
            {
                if (!sheetUsable[i]) { continue; }

                var ns = HEDRST[i].N_S;
                var headerExists = ns != null && ns.Value != 0 && existingHeaderNumbers.Contains(ns.Value);
                var ownsHeader = headerExists && claimedNumbers.Add(ns.Value);

                if (headerExists && !ownsHeader)
                {
                    duplicateNumberCount++;
                }

                if (!ownsHeader)
                {
                    needsNewHeader[i] = true;
                    newHeaderIndexes.Add(i);
                }
            }

            if (duplicateNumberCount > 0)
            {
                LogWriter.WriteLog(
                    $"SANADKHORUGMAVAD: هشدار - {duplicateNumberCount} برگه شماره سند تکراری داشتند " +
                    "(احتمالاً باقی‌مانده از باگ قبلی UPDATE روی شماره برگه اشتباه)؛ برای هرکدام شماره سند جدید ساخته شد.");
            }

            if (newHeaderIndexes.Count > 0)
            {
                var headerRequests = newHeaderIndexes
                    .Select(i => new SanadHeaderRequest
                    {
                        DATE_S = Convert.ToInt64(HEDRST[i].DATE_N),
                        SHARH_S = BuildKhorugSharhS(HEDRST[i]),
                        GHATEI = 0,
                        NO_S = 8,
                        OKF = 1,
                        USER_NAME = HEDRST[i].USER_NAME
                    })
                    .ToList();

                var reservedNumbers = ReserveSanadNumbersBatch(headerRequests);
                for (int k = 0; k < newHeaderIndexes.Count; k++)
                {
                    HEDRST[newHeaderIndexes[k]].N_S = reservedNumbers[k];
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۳ (سریال، یک کوئری): پیش‌خوانی ردیف‌های کالای «همه‌ی» برگه‌ها.
            // قبلاً برای هر برگه یک کوئری سنگین روی INVO_LST + HEAD_MANF + DTL_MANF زده می‌شد.
            // چون شرط آن فقط شماره برگه بود، خواندن یکجای بازه و گروه‌بندی در حافظه دقیقاً همان
            // ردیف‌ها را می‌دهد.
            //
            // ORDER BY اضافه شده تا ترتیب ردیف‌های سند قطعی باشد (قبلاً به Plan وابسته بود).
            //
            // چرا امن است: بازسازی به INVO_LST / HEAD_MANF / DTL_MANF نمی‌نویسد.
            // ───────────────────────────────────────────────────────────────────────────────
            var wantedSheets = new HashSet<double>();
            for (int i = 0; i < HEDRST.Count; i++)
            {
                if (sheetUsable[i]) { wantedSheets.Add(HEDRST[i].NUMBER.Value); }
            }

            var useFinalMode = (bool)Baseknow.FINALS;
            var linesBySheet = new Dictionary<double, List<KhorugMavadLineRow>>();
            var finalLinesBySheet = new Dictionary<double, List<KhorugMavadFinalLineRow>>();
            var emptyLines = new List<KhorugMavadLineRow>();
            var emptyFinalLines = new List<KhorugMavadFinalLineRow>();

            if (wantedSheets.Count > 0)
            {
                var minNum = SqlNum(wantedSheets.Min());
                var maxNum = SqlNum(wantedSheets.Max());

                if (!useFinalMode)
                {
                    foreach (var line in dbms.DoGetDataSQL<KhorugMavadLineRow>(
                        "SELECT dbo.INVO_LST.NUMBER AS SHEETNO, dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.CODE, dbo.INVO_LST.ANBAR, " +
                        "dbo.HEAD_MANF.CODE AS COM, ISNULL(dbo.HEAD_MANF.NAMES, dbo.STUF_DEF.NAME) AS NAM, " +
                        "dbo.HEAD_MANF.N_KOL, dbo.HEAD_MANF.NUMBER, dbo.HEAD_MANF.TNUMBER, dbo.DTL_MANF.SMABl AS SMAB " +
                        "FROM  dbo.STUF_DEF RIGHT OUTER JOIN dbo.HEAD_MANF INNER JOIN dbo.INVO_LST ON dbo.HEAD_MANF.FNUMB = dbo.INVO_LST.N_RASID ON dbo.STUF_DEF.CODE = dbo.HEAD_MANF.CODE " +
                        "INNER JOIN dbo.DTL_MANF ON dbo.DTL_MANF.CODE = dbo.INVO_LST.CODE AND dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB " +
                        $"WHERE (dbo.INVO_LST.NUMBER BETWEEN {minNum} AND {maxNum}) AND (dbo.INVO_LST.TAG = 10) " +
                        "ORDER BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.id"))
                    {
                        if (line?.SHEETNO == null || !wantedSheets.Contains(line.SHEETNO.Value)) { continue; }

                        if (!linesBySheet.TryGetValue(line.SHEETNO.Value, out var bucket))
                        {
                            bucket = new List<KhorugMavadLineRow>();
                            linesBySheet[line.SHEETNO.Value] = bucket;
                        }
                        bucket.Add(line);
                    }

                    LogWriter.WriteLog(
                        $"سند خروج مواد - پیش‌خوانی: {wantedSheets.Count} برگه | {linesBySheet.Sum(kv => kv.Value.Count)} ردیف کالا");
                }
                else
                {
                    // GROUP BY عیناً همان گروه‌بندی کوئری قبلی است؛ فقط شماره برگه هم به آن اضافه شده
                    // تا گروه‌ها بین برگه‌ها قاطی نشوند.
                    foreach (var line in dbms.DoGetDataSQL<KhorugMavadFinalLineRow>(
                        "SELECT dbo.INVO_LST.NUMBER AS SHEETNO, dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, MAX(dbo.DTL_MANF.SMABL) AS SMAB " +
                        "FROM   dbo.INVO_LST INNER JOIN  dbo.DTL_MANF ON dbo.DTL_MANF.CODE = dbo.INVO_LST.CODE " +
                        $"WHERE (dbo.INVO_LST.NUMBER BETWEEN {minNum} AND {maxNum}) And (dbo.INVO_LST.TAG = 10) " +
                        "GROUP BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE " +
                        "ORDER BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.CODE"))
                    {
                        if (line?.SHEETNO == null || !wantedSheets.Contains(line.SHEETNO.Value)) { continue; }

                        if (!finalLinesBySheet.TryGetValue(line.SHEETNO.Value, out var bucket))
                        {
                            bucket = new List<KhorugMavadFinalLineRow>();
                            finalLinesBySheet[line.SHEETNO.Value] = bucket;
                        }
                        bucket.Add(line);
                    }

                    LogWriter.WriteLog(
                        $"سند خروج مواد - پیش‌خوانی (حالت نهایی): {wantedSheets.Count} برگه | {finalLinesBySheet.Sum(kv => kv.Value.Count)} ردیف کالا");
                }

                // ───────────────────────────────────────────────────────────────────────────
                // پیش‌گرم‌کردن کش نام/گروه کالا با یک کوئری، به‌جای دو کوئری برای هر قلم هر برگه.
                //
                // ⚠️ عمداً فقط وقتی انجام می‌شود که کش «روشن» باشد (یعنی مسیر بازسازی دسته‌ای).
                //    نوشتن در کش وقتی کش خاموش است دو ایراد دارد: هم خواننده‌ها (GETKALANAME و
                //    GETGRPKALAco) اصلاً آن را نمی‌خوانند و کوئری هدر می‌رود، هم اگر کش بعداً
                //    توسط مسیر دیگری روشن شود، داده‌ی کهنه‌ی این اجرا زنده می‌شود.
                //    STUF_DEF جزو جدول‌هایی نیست که بازسازی به آن بنویسد، پس کش‌کردنش امن است.
                // ───────────────────────────────────────────────────────────────────────────
                if (LookupCacheEnabled)
                {
                    var allProductCodes = (useFinalMode
                            ? finalLinesBySheet.SelectMany(kv => kv.Value).Select(x => x.CODE)
                            : linesBySheet.SelectMany(kv => kv.Value).Select(x => x.CODE))
                        .Where(c => !string.IsNullOrEmpty(c))
                        .Distinct()
                        .ToList();

                    const int codeBatchSize = 1000;
                    for (int offset = 0; offset < allProductCodes.Count; offset += codeBatchSize)
                    {
                        var chunk = allProductCodes.Skip(offset).Take(codeBatchSize);
                        var inClause = string.Join(",", chunk.Select(c => $"N'{SqlText(c)}'"));
                        var stufRows = dbms.DoGetDataSQL<Custom_STUF_DEF>($"SELECT CODE, NAME, RADAH FROM dbo.STUF_DEF WHERE CODE IN ({inClause})");
                        foreach (var row in stufRows)
                        {
                            if (row == null || string.IsNullOrEmpty(row.CODE)) { continue; }
                            if (!TryGetAccountCode(row.CODE, out var stufCodeLong)) { continue; }

                            _kalaNameCache[Convert.ToDouble(stufCodeLong)] = string.IsNullOrEmpty(row.NAME) ? " " : row.NAME;
                            _kalaGroupCache[row.CODE] = row.RADAH.HasValue ? (int)row.RADAH.Value : 0;
                        }
                    }
                }

                // ───────────────────────────────────────────────────────────────────────────
                // پیش‌ساخت دسته‌ای حساب‌های لازم، پیش از ورود به حلقه‌ی موازی.
                //
                // فایده: بعد از این مرحله، همه‌ی حساب‌ها در کش _existingAccounts نشسته‌اند و
                // CREATHES داخل حلقه بدون هیچ رفت‌وبرگشتی برمی‌گردد (ISHESAB زودتر short-circuit می‌کند).
                //
                // فقط وقتی معنا دارد که کش روشن باشد؛ وگرنه نه نام کالا در دسترس است و نه
                // نتیجه‌ی ISHESAB جایی می‌ماند، پس کل کار تکراری می‌شد.
                // خودِ ساخت هم موازی انجام می‌شود: سریال بودنش روی یک TDETA_HES سرد
                // به N رفت‌وبرگشت پشت‌سرهم تبدیل می‌شد و دقیقاً همان چیزی را از بین می‌برد
                // که این بازنویسی برای آن انجام شده.
                // ───────────────────────────────────────────────────────────────────────────
                if (LookupCacheEnabled && valdefacc)
                {
                    var reqAccounts = new HashSet<(long Kol, long Moin, long Taf, string Name)>();

                    void AddReq(double? kol, long moin, long taf, string name)
                    {
                        if (kol == null) { return; }
                        reqAccounts.Add((Convert.ToInt64(kol.Value), moin, taf, name));
                    }

                    string NameOf(long productCode)
                        => _kalaNameCache.TryGetValue(Convert.ToDouble(productCode), out var kn) ? kn : " ";

                    if (!useFinalMode)
                    {
                        foreach (var line in linesBySheet.SelectMany(kv => kv.Value))
                        {
                            if (!TryGetAccountCode(line.CODE, out var codeLong)) { continue; }

                            var mablK = line.MABL_K ?? 0d;
                            var sakht = (line.SMAB ?? 0d) * (line.MEGHk ?? 0d);
                            var kalaName = NameOf(codeLong);

                            if (mablK != 0 && line.ANBAR != null)
                            {
                                AddReq(Baseknow.MOGODIA, line.ANBAR.Value, codeLong, kalaName);

                                var rdd = _kalaGroupCache.TryGetValue(line.CODE ?? string.Empty, out var r) ? r : 0;
                                AddReq(Baseknow.PHAZ_TOL, (rdd == 2 || rdd == 3) ? 2 : 1, codeLong, kalaName);

                                if (IsNull(line.COM))
                                {
                                    if (line.N_KOL != null && line.NUMBER != null && line.TNUMBER != null)
                                    {
                                        reqAccounts.Add((line.N_KOL.Value, line.NUMBER.Value, line.TNUMBER.Value, kalaName));
                                    }
                                }
                                else if (TryGetAccountCode(line.COM, out var comLong))
                                {
                                    AddReq(Baseknow.HAZ_TOL, comLong, codeLong, kalaName);
                                }
                            }

                            if (Math.Round(mablK) - sakht != 0 && TryGetAccountCode(line.COM, out var comLong2))
                            {
                                AddReq(Baseknow.AMALKARD, comLong2, codeLong, kalaName);
                            }
                        }
                    }
                    else
                    {
                        foreach (var line in finalLinesBySheet.SelectMany(kv => kv.Value))
                        {
                            if (!TryGetAccountCode(line.CODE, out var codeLong)) { continue; }

                            var mablK = line.MABL_K ?? 0d;
                            var sakht = (line.SMAB ?? 0d) * (line.MEGHk ?? 0d);
                            var kalaName = NameOf(codeLong);

                            if (mablK != 0) { AddReq(Baseknow.HAZ_TOL, 99999, codeLong, kalaName); }
                            if (sakht != 0) { AddReq(Baseknow.CONKAL, 99999, codeLong, kalaName); }
                            if (Math.Round(mablK) - sakht != 0) { AddReq(Baseknow.AMALKARD, 99999, codeLong, kalaName); }
                        }
                    }

                    if (reqAccounts.Count > 0)
                    {
                        var accList = reqAccounts.ToList();
                        var accParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(accList.Count);
                        ExecuteWithPreferredLoop(0, accList.Count, accParallelOptions, ai =>
                        {
                            var acc = accList[ai];
                            try
                            {
                                CREATHES(acc.Kol, acc.Moin, acc.Taf, acc.Name);
                            }
                            catch (Exception ex)
                            {
                                LogWriter.WriteLog($"[SANADKHORUGMAVAD] خطا در ساخت دسته‌ای حساب {acc.Kol}-{acc.Moin}-{acc.Taf}: {ex.Message}");
                            }
                        });

                        LogWriter.WriteLog($"سند خروج مواد - پیش‌ساخت حساب‌ها: {accList.Count} حساب بررسی/ساخته شد.");
                    }
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۴ (موازی): کار هر برگه کاملاً مستقل از بقیه است و هیچ قفل سراسری ندارد.
            // همه‌ی دستورهای یک برگه در «یک» رفت‌وبرگشت به سرور فرستاده می‌شوند؛ قبلاً برای هر
            // قلم کالا سه تا پنج فراخوانی جدا بود و چون DoExecuteSQL برای هر فراخوانی یک
            // Connection باز/بسته می‌کند، هزینه‌ی شبکه چند ده برابر می‌شد.
            // ───────────────────────────────────────────────────────────────────────────────
            var progressReporter = new ThrottledProgressReporter(
                HEDRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    // Math.Max لازم است: گزارش‌ها با BeginInvoke از چند Thread صف می‌شوند و ممکن
                    // است بی‌ترتیب اجرا شوند؛ بدون آن نوار پیشرفت گاهی به عقب می‌پرد.
                    auto_run.PRGR_C5.Value = Math.Max(auto_run.PRGR_C5.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);

            LogWriter.WriteLog(
                $"سند خروج مواد - تعداد برگه: {HEDRST.Count} | هدر جدید: {newHeaderIndexes.Count} | " +
                $"موازی: {Generaly.UseParallelProcessing} | MaxDegreeOfParallelism: {dbParallelOptions.MaxDegreeOfParallelism}");

            // سقف INSERT ... VALUES در SQL Server هزار ردیف است؛ محتاطانه 500تایی می‌فرستیم.
            const int detailInsertChunkSize = 500;
            const string detailInsertPrefix =
                "INSERT INTO dbo.DEED_DTL (N_S,HES_K,HES_M,HES_T,SHARH,HES,BED,BES,NUMBER,TAG) VALUES ";

            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, R =>
            {
                observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                if (!sheetUsable[R])
                {
                    progressReporter.ReportOne();
                    return;
                }

                var sheet = HEDRST[R];
                var sheetNo = sheet.NUMBER.Value;
                var nsValue = sheet.N_S.Value;

                // ردیف‌های آماده‌ی درج؛ BED و BES هر دو صریح نوشته می‌شوند و چون در جدول
                // NOT NULL با پیش‌فرض صفرند، با کد قبلی که فقط یکی از دو ستون را می‌نوشت هم‌ارز است.
                var valueRows = new List<string>();

                void AddDetail(string hesK, string hesM, string hesT, string hes, string sharh, double bed, double bes)
                {
                    valueRows.Add(
                        $"({SqlNum(nsValue)},{hesK},{hesM},{hesT},N'{SqlText(sharh)}',N'{SqlText(hes)}',{SqlNum(bed)},{SqlNum(bes)},{SqlNum(sheetNo)},10)");
                }

                // ⚠️ چرا «خطا» و نه «رد کردن بی‌صدا»:
                // ستون‌های HES_K/HES_M/HES_T در DEED_DTL از نوع int NOT NULL هستند و کد قبلی با
                // Convert.ToDouble روی مقدار خالی/غیرعددی استثنا می‌داد و کل برگه رد می‌شد.
                // اگر به‌جایش صفر بگذاریم، سند به حساب «X-0-Y» می‌خورد و چون پیش‌سازِ حساب‌ها
                // همان را هم می‌سازد، حتی FK هم جلویش را نمی‌گیرد: مبلغ واقعی بی‌سروصدا روی
                // حساب اشتباه می‌نشیند. پس همان رفتار «بلند شکست خوردن» حفظ می‌شود، فقط با
                // پیامی که بشود از رویش مشکل داده را پیدا کرد.
                // چون هنوز هیچ چیزی به دیتابیس نوشته نشده، سند قبلی این برگه دست‌نخورده می‌ماند.
                static long RequireCode(string? value, string what, double sheetNumber, string? productCode)
                {
                    if (!TryGetAccountCode(value, out var code))
                    {
                        throw new InvalidOperationException(
                            $"{what} برای کالای '{productCode}' در برگه {sheetNumber} مقدار معتبری ندارد (مقدار: '{value}').");
                    }
                    return code;
                }

                try
                {
                    if (!useFinalMode)
                    {
                        var lines = linesBySheet.TryGetValue(sheetNo, out var bucket) ? bucket : emptyLines;
                        foreach (var line in lines)
                        {
                            var lineSharh = Strings.Left("حواله خروج شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + line.MEGHk + " جهت " + Strings.Trim(line.NAM), 255);
                            var mablK = line.MABL_K ?? 0d;
                            var meghK = line.MEGHk ?? 0d;
                            var smab = line.SMAB ?? 0d;
                            var sakht = smab * meghK;

                            // کد کالا فقط وقتی لازم است که واقعاً ردیفی ساخته شود؛ قلمی که هر سه
                            // شرط زیر را رد کند در کد قبلی هم هیچ‌وقت CODE اش خوانده نمی‌شد.
                            if (mablK != 0)
                            {
                                var codeLong = RequireCode(line.CODE, "کد کالا", sheetNo, line.CODE);
                                var codeNum = Convert.ToDouble(codeLong);
                                var mablRounded = Math.Round(mablK);
                                var kalaName = GETKALANAME(codeLong);

                                if (line.ANBAR == null)
                                {
                                    throw new InvalidOperationException(
                                        $"شماره انبار برای کالای '{line.CODE}' در برگه {sheetNo} خالی است.");
                                }
                                var anbar = line.ANBAR.Value;

                                // ۱) موجودی انبار (بستانکار)
                                if (valdefacc)
                                {
                                    CREATHES(Baseknow.MOGODIA, anbar, codeLong, kalaName);
                                }
                                AddDetail(SqlNum(Baseknow.MOGODIA), SqlNum(anbar), SqlNum(codeNum),
                                    Baseknow.MOGODIA + "-" + anbar + "-" + codeNum, lineSharh, 0, mablRounded);

                                // ۲) فازهای تولید (بستانکار)
                                var RDD = GETGRPKALAco(line.CODE);
                                var phazMoin = (RDD == 2 || RDD == 3) ? 2 : 1;
                                bool phazAccountReady;
                                if (valdefacc)
                                {
                                    CREATHES(Baseknow.PHAZ_TOL, phazMoin, codeLong, kalaName);
                                    phazAccountReady = true;
                                }
                                else
                                {
                                    // کد قبلی این درج را داخل یک try/catch خالی گذاشته بود تا اگر حساب
                                    // وجود نداشت خطا نادیده گرفته شود — حالتی که فقط وقتی «ساخت حساب‌های
                                    // نبوده» خاموش است پیش می‌آید. حالا که همه‌ی ردیف‌های یک برگه در یک
                                    // تراکنش درج می‌شوند نمی‌توان خطای یک ردیف را بلعید، پس همان شرط را
                                    // صریح بررسی می‌کنیم: اگر حساب نیست، ردیف اصلاً ساخته نمی‌شود.
                                    phazAccountReady = ISHESAB(Baseknow.PHAZ_TOL, phazMoin, codeLong);
                                }
                                if (phazAccountReady)
                                {
                                    AddDetail(SqlNum(Baseknow.PHAZ_TOL), SqlNum(phazMoin), SqlNum(codeNum),
                                        Baseknow.PHAZ_TOL + "-" + phazMoin + "-" + codeNum, lineSharh, 0, mablRounded);
                                }

                                // ۳) هزینه تولید / حساب فرمول ساخت (بدهکار)
                                double hesKv, hesMv, hesTv;
                                string hesCombined;
                                if (IsNull(line.COM))
                                {
                                    if (line.N_KOL == null || line.NUMBER == null || line.TNUMBER == null)
                                    {
                                        throw new InvalidOperationException(
                                            $"حساب فرمول ساخت (N_KOL/NUMBER/TNUMBER) برای کالای '{line.CODE}' در برگه {sheetNo} خالی است.");
                                    }
                                    hesKv = line.N_KOL.Value;
                                    hesMv = line.NUMBER.Value;
                                    hesTv = line.TNUMBER.Value;
                                    hesCombined = line.N_KOL + "-" + line.NUMBER + "-" + line.TNUMBER;
                                }
                                else
                                {
                                    var comLong = RequireCode(line.COM, "کد فرمول ساخت (HEAD_MANF.CODE)", sheetNo, line.CODE);
                                    hesKv = Convert.ToDouble(Baseknow.HAZ_TOL);
                                    hesMv = comLong;
                                    hesTv = codeNum;
                                    hesCombined = Baseknow.HAZ_TOL + "-" + Convert.ToDouble(comLong) + "-" + codeNum;
                                }
                                if (valdefacc)
                                {
                                    CREATHES(hesKv, hesMv, hesTv, kalaName);
                                }
                                AddDetail(SqlNum(hesKv), SqlNum(hesMv), SqlNum(hesTv), hesCombined, lineSharh, mablRounded, 0);
                            }

                            var JAMCH = Math.Round(mablK);

                            // ۴) كنترل كالاي در جريان ساخت (بدهکار) — کد قبلی اینجا عمداً CREATHES ندارد.
                            if (sakht != 0)
                            {
                                var codeNum = Convert.ToDouble(RequireCode(line.CODE, "کد کالا", sheetNo, line.CODE));
                                var comNum = Convert.ToDouble(RequireCode(line.COM, "کد فرمول ساخت (HEAD_MANF.CODE)", sheetNo, line.CODE));
                                AddDetail(SqlNum(Baseknow.CONKAL), SqlNum(comNum), SqlNum(codeNum),
                                    Baseknow.CONKAL + "-" + comNum + "-" + codeNum,
                                    lineSharh, Math.Round(sakht), 0);
                            }

                            // ۵) عملكرد
                            if (JAMCH - sakht != 0)
                            {
                                double amalValue;
                                bool amalIsBed;
                                if (JAMCH > sakht)
                                {
                                    amalValue = Math.Round(JAMCH - sakht);
                                    amalIsBed = true;
                                }
                                else
                                {
                                    amalValue = Math.Round(sakht - JAMCH);
                                    amalIsBed = false;
                                }

                                var codeLong = RequireCode(line.CODE, "کد کالا", sheetNo, line.CODE);
                                var codeNum = Convert.ToDouble(codeLong);
                                var comNum = Convert.ToDouble(RequireCode(line.COM, "کد فرمول ساخت (HEAD_MANF.CODE)", sheetNo, line.CODE));

                                if (valdefacc)
                                {
                                    CREATHES(Baseknow.AMALKARD, comNum, codeLong, GETKALANAME(codeLong));
                                }
                                AddDetail(SqlNum(Baseknow.AMALKARD), SqlNum(comNum), SqlNum(codeNum),
                                    Baseknow.AMALKARD + "-" + comNum + "-" + codeNum,
                                    lineSharh, amalIsBed ? amalValue : 0, amalIsBed ? 0 : amalValue);
                            }
                        }
                    }
                    else
                    {
                        var lines = finalLinesBySheet.TryGetValue(sheetNo, out var bucket) ? bucket : emptyFinalLines;
                        foreach (var line in lines)
                        {
                            var lineSharh = Strings.Left("حواله خروج شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + line.MEGHk, 255);
                            var mablK = line.MABL_K ?? 0d;
                            var meghK = line.MEGHk ?? 0d;
                            var smab = line.SMAB ?? 0d;
                            var sakht = smab * meghK;

                            var JAMCH = Math.Round(mablK);

                            // قلمی که هیچ‌کدام از سه شرط زیر را ندارد در کد قبلی هم هیچ ردیفی
                            // نمی‌ساخت و CODE اش اصلاً خوانده نمی‌شد؛ پس زودتر رد می‌شود.
                            if (mablK == 0 && sakht == 0 && JAMCH - sakht == 0) { continue; }

                            var codeLong = RequireCode(line.CODE, "کد کالا", sheetNo, line.CODE);
                            var codeNum = Convert.ToDouble(codeLong);
                            var kalaName = GETKALANAME(codeLong);

                            if (mablK != 0)
                            {
                                var mablRounded = Math.Round(mablK);

                                if (line.ANBAR == null)
                                {
                                    throw new InvalidOperationException(
                                        $"شماره انبار برای کالای '{line.CODE}' در برگه {sheetNo} خالی است.");
                                }
                                var anbar = line.ANBAR.Value;

                                AddDetail(SqlNum(Baseknow.MOGODIA), SqlNum(anbar), SqlNum(codeNum),
                                    Baseknow.MOGODIA + "-" + anbar + "-" + codeNum, lineSharh, 0, mablRounded);

                                AddDetail(SqlNum(Baseknow.PHAZ_TOL), "1", SqlNum(codeNum),
                                    Baseknow.PHAZ_TOL + "-1-" + codeNum, lineSharh, 0, mablRounded);

                                if (valdefacc is true && !ISHESAB(Baseknow.HAZ_TOL, 99999, codeLong))
                                {
                                    CREATHES(Baseknow.HAZ_TOL, 99999, codeLong, kalaName);
                                }
                                AddDetail(SqlNum(Baseknow.HAZ_TOL), "99999", SqlNum(codeNum),
                                    Baseknow.HAZ_TOL + "-99999-" + codeNum, lineSharh, mablRounded, 0);
                            }

                            if (sakht != 0)
                            {
                                CREATHES(Baseknow.CONKAL, 99999, codeLong, kalaName);
                                AddDetail(SqlNum(Baseknow.CONKAL), "99999", SqlNum(codeNum),
                                    Baseknow.CONKAL + "-99999-" + codeNum, lineSharh, Math.Round(sakht), 0);
                            }

                            if (JAMCH - sakht != 0)
                            {
                                if (valdefacc is true)
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.AMALKARD, 99999, codeLong, kalaName);
                                    }
                                    catch (Exception)
                                    {
                                        LogWriter.WriteLog("خطا در برگه شماره خروج مواد :" + sheet.NUMBER + " نوع :" + sheet.TAG + "اخطار مهم ...! حساب " + Baseknow.AMALKARD + "-99999-" + line.CODE + "و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                    }
                                }

                                double amalValue;
                                bool amalIsBed;
                                if (JAMCH > sakht)
                                {
                                    amalValue = Math.Round(JAMCH - sakht);
                                    amalIsBed = true;
                                }
                                else
                                {
                                    amalValue = Math.Round(sakht - JAMCH);
                                    amalIsBed = false;
                                }

                                AddDetail(SqlNum(Baseknow.AMALKARD), "99999", SqlNum(codeNum),
                                    Baseknow.AMALKARD + "-99999-" + codeNum, lineSharh, amalIsBed ? amalValue : 0, amalIsBed ? 0 : amalValue);
                            }
                        }
                    }

                    var batch = new StringBuilder();
                    batch.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");

                    if (needsNewHeader[R])
                    {
                        // ⚠️ باگ اصلی همین‌جا بود: شرط قبلی «WHERE HEAD_LST.NUMBER = NUMBER» بود و
                        //    NUMBER پارامتر ورودی متد (ابتدای بازه) است، نه شماره‌ی برگه‌ی جاری.
                        //    یعنی در هر تکرار شماره سند روی «یک برگه‌ی ثابت» نوشته می‌شد؛ بقیه‌ی
                        //    برگه‌ها هرگز شماره سندشان ثبت نمی‌شد و در هر اجرا دوباره سند تازه
                        //    می‌گرفتند (رشد بی‌پایان DEED_HED). ضمناً چون شرط به ایندکس شماره برگه
                        //    گره نمی‌خورد، همه‌ی Threadها روی یک ردیف قفل می‌گرفتند.
                        batch.Append($"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(nsValue)} WHERE NUMBER = {SqlNum(sheetNo)} AND TAG = 10;");
                    }
                    else
                    {
                        // سربرگ از قبل هست: به‌روز می‌شود.
                        // توجه: BAYEG و base دست نمی‌خورند — شماره بایگانی و شناسه رهگیری مالیاتی
                        // باید ثابت بمانند.
                        batch.Append(
                            $"UPDATE dbo.DEED_HED SET DATE_S = {SqlNum(sheet.DATE_N)}, SHARH_S = N'{SqlText(BuildKhorugSharhS(sheet))}', " +
                            $"GHATEI = 0, NO_S = 8, OKF = 1, USER_NAME = N'{SqlText(sheet.USER_NAME)}' WHERE NO_S = 8 AND N_S = {SqlNum(nsValue)};");
                    }

                    // حذف بر اساس شماره برگه است نه شماره سند، پس برای سربرگ تازه هم لازم است:
                    // ممکن است از اجرای قبلی ردیف‌هایی با همین NUMBER/TAG ولی شماره سند قدیمی مانده باشد.
                    batch.Append($"DELETE FROM dbo.DEED_DTL WHERE NUMBER = {SqlNum(sheetNo)} AND TAG = 10;");

                    for (int offset = 0; offset < valueRows.Count; offset += detailInsertChunkSize)
                    {
                        batch.Append(detailInsertPrefix);
                        batch.Append(string.Join(",", valueRows.Skip(offset).Take(detailInsertChunkSize)));
                        batch.Append(';');
                    }

                    batch.Append("COMMIT TRANSACTION;");

                    // DoExecuteSQL خودش روی خطای 1205 (بن‌بست) تلاش مجدد دارد، پس اینجا
                    // ExecuteWithDeadlockRetry اضافه نمی‌کنیم تا تعداد تلاش‌ها چند برابر نشود.
                    // با XACT_ABORT ON کل دسته Rollback می‌شود و اجرای دوباره‌ی همان دسته
                    // بی‌خطر است (همه‌ی دستورها بر اساس کلید و Idempotent هستند).
                    dbms.DoExecuteSQL(batch.ToString());
                }
                catch (Exception ex)
                {
                    IsSuccessfully = false;
                    LogWriter.WriteLog($"SANADKHORUGMAVAD: خطا در برگه {sheetNo} (سند {nsValue}): {ex.Message} | Stack: {ex.StackTrace}");

                    // در فراخوانی تک‌برگه‌ای از فرم‌های برنامه (InternalCalling = false) خطا باید
                    // مثل قبل به بالا برود تا فرم شماره سندِ نیم‌کاره را نمایش ندهد. در بازسازی
                    // دسته‌ای اما یک برگه‌ی خراب نباید کل اجرا را متوقف کند.
                    if (!InternalCalling) { throw; }
                }

                progressReporter.ReportOne();
            });

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۵ (سریال، چند کوئری): «كسر دهم ريال».
            // این مرحله عمداً بعد از حلقه است: مقدارش از جمع بدهکار/بستانکارِ کل سند خوانده می‌شود
            // و چون هر شماره سند در این اجرا فقط یک مالک دارد، خواندن یکجای همه‌ی سندها دقیقاً همان
            // عددی را می‌دهد که خواندن تک‌تک می‌داد — با چند کوئری به‌جای یک کوئری برای هر برگه.
            // برگه‌ای که تراکنشش Rollback شده باشد اصلاً ردیفی ندارد، پس خودبه‌خود کنار می‌رود.
            // ───────────────────────────────────────────────────────────────────────────────
            var sheetByNs = new Dictionary<double, QRE_BAZ_0>();
            for (int i = 0; i < HEDRST.Count; i++)
            {
                if (sheetUsable[i] && HEDRST[i].N_S != null)
                {
                    sheetByNs[HEDRST[i].N_S.Value] = HEDRST[i];
                }
            }

            if (sheetByNs.Count > 0)
            {
                // ⚠️ چرا IN و نه BETWEEN، و چرا بدون شرط TAG:
                //
                //  • BETWEEN: شماره سندها از یک شمارنده‌ی مشترک گرفته می‌شوند و C1..C11 هم‌زمان
                //    اجرا می‌شوند، پس بازه‌ی [min, max] شماره سند تسک‌های دیگر را هم در بر می‌گیرد و
                //    این SELECT پشت قفل تراکنش آن‌ها منتظر می‌ماند. با IN فقط سندهای خودِ این اجرا
                //    خوانده می‌شوند و آن مشکل کاملاً از بین می‌رود.
                //
                //  • شرط TAG = 10 عمداً گذاشته نشده: DELETE مرحله ۴ فقط ردیف‌های TAG = 10 را پاک
                //    می‌کند و DEED_DTL.TAG از نوع float NULL است. فرم‌های ثبت دستی سند
                //    (DEED_HEAD، PGET_HED، WIN_SANAD_EKHTETAMIYAH، WIN_SANAD_AMALKARD) اصلاً ستون
                //    TAG را نام نمی‌برند و ردیفشان با TAG = NULL می‌نشیند؛ آن ردیف‌ها بعد از
                //    بازسازی هم در سند باقی می‌مانند و جزو تراز همان سند هستند. با فیلتر TAG،
                //    «كسر دهم ريال» روی جمعی ناقص حساب می‌شد: یا سند ناتراز رها می‌شد، یا
                //    ABS(diff) <= 40 روی اختلافِ ناقص پاس می‌شد و ردیف جبرانی با مبلغ غلط می‌خورد.
                var nsKeys = sheetByNs.Keys.ToList();
                var unbalanced = new List<KhorugMavadBalanceRow>();
                const int balanceQueryChunkSize = 1000;

                for (int offset = 0; offset < nsKeys.Count; offset += balanceQueryChunkSize)
                {
                    var nsIn = string.Join(",", nsKeys.Skip(offset).Take(balanceQueryChunkSize).Select(k => SqlNum(k)));
                    unbalanced.AddRange(dbms.DoGetDataSQL<KhorugMavadBalanceRow>(
                        "SELECT N_S, SUM(BED) - SUM(BES) AS DIFF FROM dbo.DEED_DTL " +
                        $"WHERE N_S IN ({nsIn}) GROUP BY N_S " +
                        "HAVING SUM(BED) - SUM(BES) <> 0 AND ABS(SUM(BED) - SUM(BES)) <= 40")
                        .Where(x => x?.N_S != null && x.DIFF != null && sheetByNs.ContainsKey(x.N_S.Value)));
                }

                if (unbalanced.Count > 0)
                {
                    if (valdefacc is true)
                    {
                        // حساب «كسر دهم ريال» یکی بیشتر نیست، پس یک بار ساخته می‌شود.
                        try
                        {
                            CREATHES(Baseknow.AMALKARD, 99999, 99999, "كسر دهم ريال");
                        }
                        catch (Exception)
                        {
                            LogWriter.WriteLog("خطا در سند خروج مواد - اخطار مهم ...! حساب " + Baseknow.AMALKARD + "-99999-99999" + "و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                        }
                    }

                    var balanceRows = new List<string>(unbalanced.Count);
                    foreach (var item in unbalanced)
                    {
                        var owner = sheetByNs[item.N_S.Value];
                        var diff = item.DIFF.Value;
                        var sharh = Strings.Left("حواله خروج شماره " + owner.NUMBER + "-" + owner.FNUMCO + " مورخ " + Strings.Format(owner.DATE_N, "####/##/##"), 255);
                        var hes = Baseknow.AMALKARD + "-99999-99999";

                        // اختلاف مثبت (بدهکار بیشتر) با یک ردیف بستانکار جبران می‌شود و برعکس.
                        var bed = diff > 0 ? 0 : Math.Abs(diff);
                        var bes = diff > 0 ? diff : 0;

                        balanceRows.Add(
                            $"({SqlNum(item.N_S.Value)},{SqlNum(Baseknow.AMALKARD)},99999,99999,N'{SqlText(sharh)}',N'{SqlText(hes)}',{SqlNum(bed)},{SqlNum(bes)},{SqlNum(owner.NUMBER)},10)");
                    }

                    try
                    {
                        for (int offset = 0; offset < balanceRows.Count; offset += detailInsertChunkSize)
                        {
                            var chunkItems = unbalanced.Skip(offset).Take(detailInsertChunkSize).ToList();
                            var chunk = string.Join(",", balanceRows.Skip(offset).Take(detailInsertChunkSize));
                            var nsIn = string.Join(",", chunkItems.Select(x => SqlNum(x.N_S.Value)));

                            // ⚠️ چرا DELETE قبل از INSERT و چرا داخل تراکنش:
                            // DoExecuteSQL علاوه بر بن‌بست (1205)، روی خطاهای گذرای اتصال هم دوباره
                            // تلاش می‌کند (CL_CCNNMANAGER.DoExecuteSQL). اگر سرور Commit کرده باشد ولی
                            // پاسخش در راه گم شود، همان INSERT دوباره اجرا می‌شود؛ و چون DEED_DTL هیچ
                            // کلید یکتای محتوایی ندارد (فقط PK روی id که IDENTITY است)، ردیف «كسر دهم
                            // ريال» دوبار درج می‌شد و سند دقیقاً به اندازه‌ی همان اختلاف ناتراز می‌ماند.
                            // با این DELETE، اجرای دوباره‌ی همان دستور به همان نتیجه می‌رسد.
                            // در اولین اجرا این DELETE هیچ ردیفی پیدا نمی‌کند، چون DELETE مرحله ۴
                            // ردیف کسر دهم ریالِ اجرای قبلی را از قبل پاک کرده است.
                            dbms.DoExecuteSQL(
                                "SET XACT_ABORT ON; BEGIN TRANSACTION;" +
                                $"DELETE FROM dbo.DEED_DTL WHERE N_S IN ({nsIn}) AND TAG = 10 " +
                                $"AND HES_K = {SqlNum(Baseknow.AMALKARD)} AND HES_M = 99999 AND HES_T = 99999;" +
                                detailInsertPrefix + chunk + ";" +
                                "COMMIT TRANSACTION;");
                        }
                    }
                    catch (Exception ex)
                    {
                        IsSuccessfully = false;
                        LogWriter.WriteLog($"SANADKHORUGMAVAD: خطا در درج ردیف کسر دهم ریال: {ex.Message}");
                        if (!InternalCalling) { throw; }
                    }
                }
            }

            stopwatch.Stop();
            progressReporter.Complete();

            LogWriter.WriteLog(
                $"SANADKHORUGMAVAD: پایان بازسازی - {HEDRST.Count} برگه در {stopwatch.Elapsed.TotalSeconds:F1} ثانیه " +
                $"با {observedThreads.Count} Thread همزمان");

            // مثل حالت سریال، شماره سند آخرین برگه‌ی پردازش‌شده برگردانده می‌شود
            // (قبلاً این مقدار از داخل حلقه‌ی موازی نوشته می‌شد و نتیجه‌اش غیرقطعی بود).
            // در فراخوانی تک‌برگه‌ای فرم‌ها، همان شماره سند همان برگه است.
            for (int i = HEDRST.Count - 1; i >= 0; i--)
            {
                if (sheetUsable[i] && HEDRST[i].N_S != null)
                {
                    SANAD_NUMBER = HEDRST[i].N_S;
                    break;
                }
            }

            return (SANAD_NUMBER, IsSuccessfully);
        }

        public static (double?, bool) SANADKHORUGSAYER(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>(
                "SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME " +
                $"FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= {NUMBER} AND HEAD_LST.NUMBER <= {NUMBER2} and HEAD_LST.tag = 11)) ORDER BY HEAD_LST.NUMBER").ToList();

            LogWriter.WriteLog("SANADKHORUGSAYER : \n شروع باز سازي از سند شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + " " + DateTime.Now);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observedThreads = new System.Collections.Concurrent.ConcurrentDictionary<int, byte>();

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۰ (در حافظه): نرمال‌سازی تاریخ و کنار گذاشتن برگه‌های با تاریخ نامعتبر.
            // DEED_HED یک CHECK دارد (CK_DEED_HED: date_s >= 10101) ولی HEAD_LST.DATE_N ندارد؛
            // اگر برگه‌ی خراب وارد رزرو دسته‌ای شود، INSERT سربرگ کل دسته را با خطا برمی‌گرداند.
            // ───────────────────────────────────────────────────────────────────────────────
            var sheetUsable = new bool[HEDRST.Count];
            for (int i = 0; i < HEDRST.Count; i++)
            {
                var sheet = HEDRST[i];
                if (sheet == null || sheet.NUMBER == null) { continue; }

                if (!TryGetDateNumber(sheet.DATE_N, out var normalizedDate))
                {
                    LogWriter.WriteLog($"SANADKHORUGSAYER: تاریخ نامعتبر برای برگ {sheet.NUMBER} با مقدار '{sheet.DATE_N}'؛ این برگه پردازش نشد.");
                    IsSuccessfully = false;
                    continue;
                }

                if (normalizedDate < 10101)
                {
                    LogWriter.WriteLog(
                        $"SANADKHORUGSAYER: تاریخ برگ {sheet.NUMBER} برابر {normalizedDate} است و از حداقل مجاز سند (10101) کمتر می‌باشد؛ این برگه پردازش نشد.");
                    IsSuccessfully = false;
                    continue;
                }

                sheet.DATE_N = normalizedDate;
                sheetUsable[i] = true;
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۱ و ۲ (سریال، چند کوئری): تعیین شماره سند همه‌ی برگه‌ها.
            // جایگزین CRSANADGEN که برای هر برگه یک بار Createsanad صدا می‌زد و کل جدول DEED_HED
            // را با Serializable قفل می‌کرد؛ همان تنها عامل کافی بود تا حلقه‌ی Parallel سریال شود.
            // هر دو حالت «سند روزانه» (SNDKH = true) و «تک‌سندی» عیناً حفظ شده‌اند.
            // ───────────────────────────────────────────────────────────────────────────────
            static string BuildKhorugSayerSharhS(QRE_BAZ_0 hedRow)
                => Strings.Left(" حواله خروج ساير مواد از انبار شماره " + hedRow.NUMBER + "-" + hedRow.FNUMCO + "مورخ " + Strings.Format(hedRow.DATE_N, "####/##/##"), 100);

            var isDailyMode = (bool)Baseknow.SNDKH;
            var needsNewHeader = new bool[HEDRST.Count];

            if (isDailyMode)
            {
                var usableIndexes = new List<int>();
                for (int i = 0; i < HEDRST.Count; i++)
                {
                    if (sheetUsable[i]) { usableIndexes.Add(i); }
                }

                var dailyNsByDate = new Dictionary<long, double>();
                var dates = usableIndexes.Select(i => Convert.ToInt64(HEDRST[i].DATE_N)).Distinct().ToList();

                if (dates.Count > 0)
                {
                    var minDate = dates.Min();
                    var maxDate = dates.Max();
                    foreach (var row in dbms.DoGetDataSQL<QRE10>(
                        $"SELECT BASE, n_s, date_s, no_s FROM dbo.DEED_HED WHERE no_s = 12 AND date_s BETWEEN {minDate} AND {maxDate}"))
                    {
                        if (row?.DATE_S != null && row.N_S != null && !dailyNsByDate.ContainsKey(row.DATE_S.Value))
                        {
                            dailyNsByDate[row.DATE_S.Value] = row.N_S.Value;
                        }
                    }

                    var missingDates = dates.Where(d => !dailyNsByDate.ContainsKey(d)).ToList();
                    if (missingDates.Count > 0)
                    {
                        var headerRequests = missingDates.Select(d =>
                        {
                            // نمونه‌برداری فقط از برگه‌های سالم؛ برگه‌ی کنارگذاشته‌شده نباید شرح سند را تعیین کند.
                            var sampleSheet = HEDRST[usableIndexes.First(i => Convert.ToInt64(HEDRST[i].DATE_N) == d)];
                            return new SanadHeaderRequest
                            {
                                DATE_S = d,
                                SHARH_S = BuildKhorugSayerSharhS(sampleSheet),
                                GHATEI = 0,
                                NO_S = 12,
                                OKF = -1,
                                USER_NAME = sampleSheet.USER_NAME
                            };
                        }).ToList();

                        var newNsValues = ReserveSanadNumbersBatch(headerRequests);
                        for (int k = 0; k < missingDates.Count; k++)
                        {
                            dailyNsByDate[missingDates[k]] = newNsValues[k];
                        }
                    }
                }

                // شماره سند روی برگه‌ها ثبت می‌شود. دستورها دسته‌بندی می‌شوند تا یک متن SQL
                // بی‌اندازه بزرگ (با ده‌ها هزار UPDATE) به سرور فرستاده نشود.
                const int headUpdateChunkSize = 500;
                var headUpdates = new List<string>();
                foreach (var i in usableIndexes)
                {
                    if (!dailyNsByDate.TryGetValue(Convert.ToInt64(HEDRST[i].DATE_N), out var ns))
                    {
                        LogWriter.WriteLog($"SANADKHORUGSAYER: شماره سند روزانه برای تاریخ {HEDRST[i].DATE_N} پیدا نشد؛ برگه {HEDRST[i].NUMBER} پردازش نشد.");
                        sheetUsable[i] = false;
                        IsSuccessfully = false;
                        continue;
                    }

                    if (HEDRST[i].N_S != ns)
                    {
                        HEDRST[i].N_S = ns;
                        headUpdates.Add($"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(ns)} WHERE NUMBER = {SqlNum(HEDRST[i].NUMBER.Value)} AND TAG = 11;");
                    }
                }

                for (int offset = 0; offset < headUpdates.Count; offset += headUpdateChunkSize)
                {
                    var batch = new StringBuilder();
                    batch.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                    foreach (var stmt in headUpdates.Skip(offset).Take(headUpdateChunkSize)) { batch.Append(stmt); }
                    batch.Append("COMMIT TRANSACTION;");
                    dbms.DoExecuteSQL(batch.ToString());
                }
            }
            else
            {
                var existingHeaderNumbers = new HashSet<double>();
                var candidateNumbers = new List<double>();
                for (int i = 0; i < HEDRST.Count; i++)
                {
                    if (sheetUsable[i] && HEDRST[i].N_S != null && HEDRST[i].N_S.Value != 0)
                    {
                        candidateNumbers.Add(HEDRST[i].N_S.Value);
                    }
                }

                if (candidateNumbers.Count > 0)
                {
                    var minNs = SqlNum(candidateNumbers.Min());
                    var maxNs = SqlNum(candidateNumbers.Max());
                    foreach (var found in dbms.DoGetDataSQL<double?>(
                        $"SELECT N_S FROM dbo.DEED_HED WHERE NO_S = 12 AND N_S BETWEEN {minNs} AND {maxNs}"))
                    {
                        if (found.HasValue) { existingHeaderNumbers.Add(found.Value); }
                    }
                }

                // هر شماره سند فقط یک مالک دارد؛ وگرنه دو Thread موازی ردیف‌های یکدیگر را پاک می‌کردند.
                var newHeaderIndexes = new List<int>();
                var claimedNumbers = new HashSet<double>();

                for (int i = 0; i < HEDRST.Count; i++)
                {
                    if (!sheetUsable[i]) { continue; }
                    var ns = HEDRST[i].N_S;
                    var headerExists = ns != null && ns.Value != 0 && existingHeaderNumbers.Contains(ns.Value);
                    var ownsHeader = headerExists && claimedNumbers.Add(ns.Value);

                    if (!ownsHeader)
                    {
                        needsNewHeader[i] = true;
                        newHeaderIndexes.Add(i);
                    }
                }

                if (newHeaderIndexes.Count > 0)
                {
                    var headerRequests = newHeaderIndexes.Select(i => new SanadHeaderRequest
                    {
                        DATE_S = Convert.ToInt64(HEDRST[i].DATE_N),
                        SHARH_S = BuildKhorugSayerSharhS(HEDRST[i]),
                        GHATEI = 0,
                        NO_S = 12,
                        OKF = -1,
                        USER_NAME = HEDRST[i].USER_NAME
                    }).ToList();

                    var reservedNumbers = ReserveSanadNumbersBatch(headerRequests);
                    for (int k = 0; k < newHeaderIndexes.Count; k++)
                    {
                        HEDRST[newHeaderIndexes[k]].N_S = reservedNumbers[k];
                    }
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۳ (سریال، چند کوئری): پیش‌خوانی اقلام INVO_LST و HEAD_MANF برای همه‌ی برگه‌ها.
            // چرا امن است: بازسازی به INVO_LST و HEAD_MANF نمی‌نویسد.
            // ───────────────────────────────────────────────────────────────────────────────
            var wantedSheets = new HashSet<double>();
            for (int i = 0; i < HEDRST.Count; i++)
            {
                if (sheetUsable[i]) { wantedSheets.Add(HEDRST[i].NUMBER.Value); }
            }

            var linesBySheet = new Dictionary<double, List<KhorugSayerLineRow>>();
            var emptyLines = new List<KhorugSayerLineRow>();

            if (wantedSheets.Count > 0)
            {
                var minNum = SqlNum(wantedSheets.Min());
                var maxNum = SqlNum(wantedSheets.Max());

                // ORDER BY اضافه شده تا ترتیب ردیف‌های سند قطعی باشد (قبلاً به Plan وابسته بود).
                foreach (var line in dbms.DoGetDataSQL<KhorugSayerLineRow>(
                    "SELECT NUMBER AS SHEETNO, SANAD_NO, N_RASID, MABL_K, MEGHk, CODE, ANBAR " +
                    $"FROM dbo.INVO_LST WHERE (NUMBER BETWEEN {minNum} AND {maxNum}) AND (TAG = 11) " +
                    "ORDER BY NUMBER, id"))
                {
                    if (line?.SHEETNO == null || !wantedSheets.Contains(line.SHEETNO.Value)) { continue; }

                    if (!linesBySheet.TryGetValue(line.SHEETNO.Value, out var bucket))
                    {
                        bucket = new List<KhorugSayerLineRow>();
                        linesBySheet[line.SHEETNO.Value] = bucket;
                    }
                    bucket.Add(line);
                }

                LogWriter.WriteLog(
                    $"سند خروج ساير - پیش‌خوانی: {wantedSheets.Count} برگه | {linesBySheet.Sum(kv => kv.Value.Count)} ردیف کالا");
            }

            // فرمول‌های ساخت (برای N_RASID های عددی) یکجا خوانده می‌شوند؛ قبلاً برای هر قلم
            // هر برگه یک SELECT جدا روی HEAD_MANF زده می‌شد.
            var numericRasids = linesBySheet.SelectMany(kv => kv.Value)
                                            .Select(x => x.N_RASID)
                                            .Where(r => !string.IsNullOrEmpty(r) && Information.IsNumeric(r))
                                            .Select(r => Convert.ToInt32(SafeToDouble(r)))
                                            .Distinct().ToList();

            var headManfDict = new Dictionary<int, QRE_BAZ_6>();
            if (numericRasids.Count > 0)
            {
                const int rasidBatchSize = 1000;
                for (int offset = 0; offset < numericRasids.Count; offset += rasidBatchSize)
                {
                    var chunk = numericRasids.Skip(offset).Take(rasidBatchSize);
                    var inClause = string.Join(",", chunk);
                    var rows = dbms.DoGetDataSQL<QRE_BAZ_6>($"SELECT FNUMB, NUMBER, TNUMBER, N_KOL, NAMES FROM dbo.HEAD_MANF WHERE FNUMB IN ({inClause})");
                    foreach (var r in rows)
                    {
                        if (r?.FNUMB != null && !headManfDict.ContainsKey(r.FNUMB.Value))
                        {
                            headManfDict[r.FNUMB.Value] = r;
                        }
                    }
                }
            }

            // برای N_RASID های غیرعددی (که مستقیماً کد حساب‌اند)، وجود حساب و نامش یک بار
            // پرس‌وجو می‌شود تا داخل حلقه‌ی موازی از کش خوانده شود.
            // فقط وقتی معنا دارد که کش روشن باشد؛ وگرنه نتیجه‌ی ISHESAB/GETTAFNAME جایی نمی‌ماند
            // و این کار فقط کوئری تکراری تولید می‌کرد.
            if (LookupCacheEnabled)
            {
                var nonNumericRasids = linesBySheet.SelectMany(kv => kv.Value)
                                                   .Select(x => x.N_RASID)
                                                   .Where(r => !string.IsNullOrEmpty(r) && !Information.IsNumeric(r))
                                                   .Distinct().ToList();

                foreach (var rasid in nonNumericRasids)
                {
                    double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                    GETTAF3(rasid, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    if (CKOL != null && CMOIN != null && CTAF != null)
                    {
                        ISHESAB(CKOL, CMOIN, CTAF);
                    }
                    GETTAFNAME(rasid);
                }
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // مرحله ۴ (موازی): کار هر برگه مستقل است و همه‌ی دستورهایش در یک رفت‌وبرگشت می‌رود.
            // ───────────────────────────────────────────────────────────────────────────────
            var progressReporter = new ThrottledProgressReporter(
                HEDRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    auto_run.PRGR_C6.Value = Math.Max(auto_run.PRGR_C6.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);

            LogWriter.WriteLog(
                $"سند خروج ساير - تعداد برگه: {HEDRST.Count} | سند روزانه: {isDailyMode} | " +
                $"موازی: {Generaly.UseParallelProcessing} | MaxDegreeOfParallelism: {dbParallelOptions.MaxDegreeOfParallelism}");

            const int detailInsertChunkSize = 500;
            const string detailInsertPrefixFull =
                "INSERT INTO dbo.DEED_DTL (N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,BED,BES,SHARH,NUMBER,MHAZ_NO,TAG) VALUES ";

            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, EOF =>
            {
                observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                if (!sheetUsable[EOF])
                {
                    progressReporter.ReportOne();
                    return;
                }

                var sheet = HEDRST[EOF];
                var sheetNo = sheet.NUMBER.Value;
                var nsValue = sheet.N_S.Value;

                var valueRows = new List<string>();

                void AddDetailFull(string hesK, string hesM, string hesT, string hesT2, string hesT3, string hesT4, string hes, double bed, double bes, string sharh, string mhazNo)
                {
                    valueRows.Add(
                        $"({SqlNum(nsValue)},{hesK},{hesM},{hesT},{hesT2},{hesT3},{hesT4},N'{SqlText(hes)}',{SqlNum(bed)},{SqlNum(bes)},N'{SqlText(sharh)}',{SqlNum(sheetNo)},{mhazNo},11)");
                }

                try
                {
                    var lines = linesBySheet.TryGetValue(sheetNo, out var bucket) ? bucket : emptyLines;

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrEmpty(line?.N_RASID)) { continue; }

                        var mablK = line.MABL_K ?? 0d;
                        var meghK = line.MEGHk ?? 0d;
                        if (mablK == 0) { continue; }

                        // ⚠️ کد کالا با TryGetAccountCode خوانده می‌شود و نه SafeToDouble:
                        // SafeToDouble برای مقدار غیرعددی «صفر» می‌داد و سند به حساب «۱۲۱-انبار-۰»
                        // می‌خورد. کد قبلی با Convert.ToDouble استثنا می‌داد و برگه رد می‌شد؛
                        // همان رفتار حفظ می‌شود، فقط با پیام قابل‌فهم — و فقط وقتی که واقعاً
                        // قرار است ردیف موجودی انبار نوشته شود (مثل کد قبلی).
                        var codeOk = TryGetAccountCode(line.CODE, out var codeLong) && line.ANBAR != null;

                        void RequireInventoryKeys()
                        {
                            if (!codeOk)
                            {
                                throw new InvalidOperationException(
                                    $"کد کالا ('{line.CODE}') یا انبار برای برگه {sheetNo} معتبر نیست.");
                            }
                        }

                        var codeNum = Convert.ToDouble(codeLong);
                        var anbar = line.ANBAR ?? 0;
                        string sanadNoVal = (line.SANAD_NO == null) ? "NULL" : SqlNum(line.SANAD_NO.Value);

                        if (Information.IsNumeric(line.N_RASID))
                        {
                            var fnumb = Convert.ToInt32(SafeToDouble(line.N_RASID));
                            if (headManfDict.TryGetValue(fnumb, out var jstt)
                                && jstt.N_KOL != null && jstt.NUMBER != null && jstt.TNUMBER != null)
                            {
                                var _hes = jstt.N_KOL + "-" + jstt.NUMBER + "-" + jstt.TNUMBER;
                                var _BED = Math.Round(mablK);
                                var _SHARH = Strings.Left("حواله خروج ساير شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + meghK + " جهت " + Strings.Trim(jstt.NAMES), 255);

                                // بدهکار: حساب فرمول ساخت
                                AddDetailFull(SqlNum(jstt.N_KOL), SqlNum(jstt.NUMBER), SqlNum(jstt.TNUMBER), "NULL", "NULL", "NULL", _hes, _BED, 0, _SHARH, "NULL");

                                // بستانکار: موجودی انبار
                                RequireInventoryKeys();
                                var hes_ = Baseknow.MOGODIA + "-" + anbar + "-" + codeNum;
                                var SHARH_ = Strings.Left("حواله خروج  ساير  مواد شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + meghK, 255);
                                AddDetailFull(SqlNum(Baseknow.MOGODIA), SqlNum(anbar), SqlNum(codeNum), "NULL", "NULL", "NULL", hes_, 0, Math.Round(mablK), SHARH_, sanadNoVal);
                            }
                        }
                        else
                        {
                            double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                            GETTAF3(line.N_RASID, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                            if (CTAF == null) { continue; }

                            var BED__ = Math.Round(mablK);
                            var tafName = GETTAFNAME(line.N_RASID);
                            var SHARH__ = Strings.Left("حواله خروج ساير شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + meghK + " جهت " + Strings.Trim(tafName), 255);

                            string CTAF2T = (CTAF2 == null || CTAF2.Value == 0) ? "NULL" : SqlNum(CTAF2.Value);
                            string CTAF3T = (CTAF3 == null || CTAF3.Value == 0) ? "NULL" : SqlNum(CTAF3.Value);
                            string CTAF4T = (CTAF4 == null || CTAF4.Value == 0) ? "NULL" : SqlNum(CTAF4.Value);

                            // همان بررسی «حساب هست یا نه» که کد قبلی با SELECT COUNT(1) انجام می‌داد.
                            if (ISHESAB(CKOL, CMOIN, CTAF))
                            {
                                RequireInventoryKeys();
                                AddDetailFull(SqlNum(CKOL), SqlNum(CMOIN), SqlNum(CTAF), CTAF2T, CTAF3T, CTAF4T, line.N_RASID, BED__, 0, SHARH__, "NULL");

                                var __hes = Baseknow.MOGODIA + "-" + anbar + "-" + codeNum;
                                var __SHARH = Strings.Left("حواله خروج  ساير  مواد شماره " + sheet.NUMBER + "-" + sheet.FNUMCO + " مورخ " + Strings.Format(sheet.DATE_N, "####/##/##") + " به مقدار" + meghK, 255);
                                AddDetailFull(SqlNum(Baseknow.MOGODIA), SqlNum(anbar), SqlNum(codeNum), "NULL", "NULL", "NULL", __hes, 0, Math.Round(mablK), __SHARH, sanadNoVal);
                            }
                            else
                            {
                                var _HESAB_ = $"{CKOL}-{CMOIN}-{CTAF}";
                                string[] ctafs = { CTAF2T, CTAF3T, CTAF4T };
                                string resultStr = string.Join("-", ctafs.Where(s => s != "NULL"));
                                LogWriter.WriteLog($"[SANADKHORUGSAYER] : (RASID : {line.N_RASID}) => حساب : {_HESAB_}{(!string.IsNullOrEmpty(resultStr) ? $"-{resultStr}" : "")}  " + DateTime.Now);
                            }
                        }
                    }

                    var batch = new StringBuilder();
                    batch.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");

                    // در حالت «سند روزانه» چند برگه یک سند مشترک دارند، پس سربرگ نه ساخته و نه
                    // به‌روز می‌شود (مرحله ۱ و ۲ آن را انجام داده‌اند) و شماره سند هم همان‌جا روی
                    // HEAD_LST نوشته شده است.
                    if (!isDailyMode)
                    {
                        if (needsNewHeader[EOF])
                        {
                            batch.Append($"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(nsValue)} WHERE NUMBER = {SqlNum(sheetNo)} AND TAG = 11;");
                        }
                        else
                        {
                            // توجه: BAYEG و base دست نمی‌خورند.
                            batch.Append(
                                $"UPDATE dbo.DEED_HED SET DATE_S = {SqlNum(sheet.DATE_N)}, SHARH_S = N'{SqlText(BuildKhorugSayerSharhS(sheet))}', " +
                                $"GHATEI = 0, NO_S = 12, OKF = -1, USER_NAME = N'{SqlText(sheet.USER_NAME)}' WHERE NO_S = 12 AND N_S = {SqlNum(nsValue)};");
                        }
                    }

                    // حذف بر اساس شماره برگه است، پس در حالت سند روزانه هم فقط ردیف‌های همین
                    // برگه پاک می‌شوند و برگه‌های دیگرِ همان سند دست‌نخورده می‌مانند.
                    batch.Append($"DELETE FROM dbo.DEED_DTL WHERE NUMBER = {SqlNum(sheetNo)} AND TAG = 11;");

                    for (int offset = 0; offset < valueRows.Count; offset += detailInsertChunkSize)
                    {
                        batch.Append(detailInsertPrefixFull);
                        batch.Append(string.Join(",", valueRows.Skip(offset).Take(detailInsertChunkSize)));
                        batch.Append(';');
                    }

                    batch.Append("COMMIT TRANSACTION;");
                    dbms.DoExecuteSQL(batch.ToString());
                }
                catch (Exception ex)
                {
                    IsSuccessfully = false;
                    LogWriter.WriteLog($"SANADKHORUGSAYER: خطا در برگه {sheetNo} (سند {nsValue}): {ex.Message} | Stack: {ex.StackTrace}");
                    if (!InternalCalling) { throw; }
                }

                progressReporter.ReportOne();
            });

            stopwatch.Stop();
            progressReporter.Complete();

            LogWriter.WriteLog($"SANADKHORUGSAYER: پایان بازسازی - {HEDRST.Count} برگه در {stopwatch.Elapsed.TotalSeconds:F1} ثانیه با {observedThreads.Count} Thread همزمان");

            for (int i = HEDRST.Count - 1; i >= 0; i--)
            {
                if (sheetUsable[i] && HEDRST[i].N_S != null)
                {
                    SANAD_NUMBER = HEDRST[i].N_S;
                    break;
                }
            }

            return (SANAD_NUMBER, IsSuccessfully);
        }

        private static double CRSANADGEN(String? SHSH, int TG, int NOE_S, double NUMBER, double? N_S, long DATE_N, string? USER_NAME)
        {
            double? max_ns;
            if ((bool)Baseknow.SNDKH) // سند روزانه است
            {
                List<QRE10> SARST = null;
                if (!IsNull(N_S)) // فاکتور سند دارد
                {
                    SARST = dbms.DoGetDataSQL<QRE10>($"SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = {NOE_S} and n_s = {N_S}").ToList();
                    if (SARST.Count > 0)  // اگرسند  فاکتورهست
                    {
                        if (SARST.Select(x => x.DATE_S).FirstOrDefault() == DATE_N) // تاريخ سند و فاکتوريکي است
                        {
                            max_ns = N_S;
                        }
                        else
                        {
                        SEJ:
                            SARST = dbms.DoGetDataSQL<QRE10>($"SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = {NOE_S} and DATE_S = {DATE_N}").ToList();
                            if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                            {
                                max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                            }
                            else
                            {
                                max_ns = Createsanad(DATE_N, SHSH, 0, NOE_S, -1, USER_NAME);

                                N_S = max_ns;
                                dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");

                            }
                        }
                    }
                    else
                    {
                        // goto SEJ;
                        SARST = dbms.DoGetDataSQL<QRE10>($"SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = {NOE_S} and DATE_S = {DATE_N}").ToList();
                        if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                        {
                            max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                        }
                        else
                        {
                            max_ns = Createsanad((long)DATE_N, SHSH, 0, NOE_S, -1, USER_NAME);

                            N_S = max_ns;
                            dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");

                        }
                    } // چک کن اگه نيست صادر کن
                }
                else
                {
                    // goto SEJ;
                    SARST = dbms.DoGetDataSQL<QRE10>($"SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = {NOE_S} and DATE_S = {DATE_N}").ToList();
                    if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                    {
                        max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                    }
                    else
                    {
                        max_ns = Createsanad((long)DATE_N, SHSH, 0, NOE_S, -1, USER_NAME);

                        N_S = max_ns;
                        dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");

                    }
                } // چک کن اگه نيست صادر کن
            }
            else if (!IsNull(N_S)) // تک سندي
                                   // فاکتور سند دارد
            {
                var SARST = dbms.DoGetDataSQL<QRE11>($"SELECT    n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = {NOE_S} and N_s = {N_S}").ToList();
                if (SARST.Count > 0)   // اگرسند فاکتورهست
                {
                    if (SARST.Select(x => x.DATE_S).FirstOrDefault() != DATE_N) // تاريخ سند و فاکتوريکي است
                    {
                        dbms.DoExecuteSQL($"UPDATE DEED_HED SET DATE_S = {DATE_N},SHARH_S = '{SHSH}',GHATEI = 0,NO_S = {NOE_S},OKF=-1,USER_NAME ='{USER_NAME}' WHERE N_S ={N_S}");
                    }
                    max_ns = N_S;
                }
                else
                {
                    max_ns = Createsanad(DATE_N, SHSH, 0, NOE_S, -1, USER_NAME);
                    N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");

                }
            }
            else
            {
                max_ns = Createsanad(DATE_N, SHSH, 0, NOE_S, -1, USER_NAME);
                N_S = max_ns;
                dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");

            }
            if (IsNull(N_S) || N_S != max_ns)
            {
                N_S = max_ns;
                dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {NUMBER} AND (TAG = {TG}))");
            }

            return (double)max_ns;
        }

        public static (double?, bool) SANADVORUDSAKHT(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            bool isDefaccChecked = Generaly.defacc;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>($"SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= {NUMBER} AND HEAD_LST.NUMBER <= {NUMBER2} and HEAD_LST.tag = 9 )) ORDER BY HEAD_LST.NUMBER").ToList();

            if (HEDRST.Count == 0)
            {
                return (SANAD_NUMBER, IsSuccessfully);
            }

            LogWriter.WriteLog("ورود ساخته شده تولید شروع باز سازي از سند شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + DateTime.Now);

            if (!(Baseknow.SANAT == true || IsNull(Baseknow.SANAT)))
            {
                dbms.DoExecuteSQL($"DELETE FROM dbo.DEED_DTL WHERE TAG = 9 AND NUMBER BETWEEN {NUMBER} AND {NUMBER2}");
                return (SANAD_NUMBER, IsSuccessfully);
            }

            // ───────────────────────────────────────────────────────────────────────────────
            // ⚠️ چرخه‌ی عمر کش.
            //
            // این تابع از فرم «برگه ورود» هم با InternalCalling = false صدا زده می‌شود
            // (Prg_UI/.../HAVALAH_ENTER.xaml.cs) و چون CL_HESABDARI_AUTO_BAZ کلاسی static است،
            // روشن گذاشتن LookupCacheEnabled بدون خاموش کردنش یعنی کش تا پایان عمر برنامه زنده
            // می‌ماند: بهای تمام‌شده‌ی استاندارد، نام کالا/حساب و گروه کالا همگی «کهنه» می‌شوند و
            // اگر کاربر فرمول ساخت را عوض کند، سند بعدی با نرخ قدیمی زده می‌شود.
            //
            // ضمناً ClearLookupCaches() بی‌قید و شرط، کشِ در حال استفاده‌ی C1..C11 را
            // (که هم‌زمان اجرا می‌شوند) وسط کار پاک می‌کرد.
            //
            // پس: کش فقط وقتی اینجا روشن/پاک می‌شود که «صاحبش» همین فراخوانی باشد، و در finally
            // حتماً به حالت اول برمی‌گردد.
            // ───────────────────────────────────────────────────────────────────────────────
            bool cacheOwnedHere = !LookupCacheEnabled;
            if (cacheOwnedHere)
            {
                ClearLookupCaches();
                LookupCacheEnabled = true;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observedThreads = new System.Collections.Concurrent.ConcurrentDictionary<int, byte>();

            try
            {
                static string BuildVorudSharhS(QRE_BAZ_0 hedRow)
                    => Strings.Left(" برگه ورود كالا به انبار شماره " + hedRow.NUMBER + "-" + hedRow.FNUMCO + "مورخ " + Strings.Format(hedRow.DATE_N, "####/##/##"), 100);

                // ───────────────────────────────────────────────────────────────────────────
                // مرحله ۰: نرمال‌سازی تاریخ (CK_DEED_HED: date_s >= 10101).
                // ───────────────────────────────────────────────────────────────────────────
                var sheetUsable = new bool[HEDRST.Count];
                for (int i = 0; i < HEDRST.Count; i++)
                {
                    var sheet = HEDRST[i];
                    if (sheet == null || sheet.NUMBER == null) { continue; }

                    if (!TryGetDateNumber(sheet.DATE_N, out var normalizedDate) || normalizedDate < 10101)
                    {
                        LogWriter.WriteLog($"SANADVORUDSAKHT: تاریخ نامعتبر ('{sheet.DATE_N}') برای برگ {sheet.NUMBER}؛ این برگه پردازش نشد.");
                        IsSuccessfully = false;
                        continue;
                    }

                    sheet.DATE_N = normalizedDate;
                    sheetUsable[i] = true;
                }

                // ───────────────────────────────────────────────────────────────────────────
                // مرحله ۱ و ۲: تعیین شماره سند.
                //
                // ⚠️ اینجا نباید شمارنده‌ی دستی ساخت. «MAX(N_S) بین سندهای NO_S = 9» بیشینه‌ی کل
                //    جدول نیست؛ با آن، شماره سندهای تازه روی سندهای موجودِ نوع‌های دیگر می‌افتند و
                //    ردیف‌های دو سند بی‌ربط قاطی می‌شوند. ضمناً بدون قفل Serializable، دو اجرای
                //    هم‌زمان (یا Createsanad سایر تسک‌ها) شماره تکراری می‌سازند.
                //    ReserveSanadNumbersBatch دقیقاً همان قفل‌های Createsanad را می‌گیرد، ولی
                //    یک بار برای کل دسته.
                // ───────────────────────────────────────────────────────────────────────────
                var existingHeaderNumbers = new HashSet<double>();
                var candidateNumbers = new List<double>();
                for (int i = 0; i < HEDRST.Count; i++)
                {
                    if (sheetUsable[i] && HEDRST[i].N_S != null && HEDRST[i].N_S.Value != 0)
                    {
                        candidateNumbers.Add(HEDRST[i].N_S.Value);
                    }
                }

                var headerDateByNs = new Dictionary<double, long>();
                if (candidateNumbers.Count > 0)
                {
                    var fromNs = SqlNum(candidateNumbers.Min());
                    var toNs = SqlNum(candidateNumbers.Max());
                    foreach (var found in dbms.DoGetDataSQL<QRE10>(
                        $"SELECT BASE, N_S, DATE_S, NO_S FROM dbo.DEED_HED WHERE NO_S = 9 AND N_S BETWEEN {fromNs} AND {toNs}"))
                    {
                        if (found?.N_S != null)
                        {
                            existingHeaderNumbers.Add(found.N_S.Value);
                            headerDateByNs[found.N_S.Value] = found.DATE_S ?? 0L;
                        }
                    }
                }

                var needsNewHeader = new bool[HEDRST.Count];
                var newHeaderIndexes = new List<int>();
                var claimedNumbers = new HashSet<double>();
                var headerUpdates = new List<string>();

                for (int i = 0; i < HEDRST.Count; i++)
                {
                    if (!sheetUsable[i]) { continue; }

                    var ns = HEDRST[i].N_S;
                    var headerExists = ns != null && ns.Value != 0 && existingHeaderNumbers.Contains(ns.Value);
                    var ownsHeader = headerExists && claimedNumbers.Add(ns.Value);

                    if (!ownsHeader)
                    {
                        needsNewHeader[i] = true;
                        newHeaderIndexes.Add(i);
                    }
                    else if (headerDateByNs.TryGetValue(ns.Value, out var headerDate) && headerDate != HEDRST[i].DATE_N)
                    {
                        // بروز رسانی تاریخ سند در صورت تغییر تاریخ برگه ورود (مثل کد قبلی).
                        headerUpdates.Add(
                            $"UPDATE dbo.DEED_HED SET DATE_S = {SqlNum(HEDRST[i].DATE_N)}, SHARH_S = N'{SqlText(BuildVorudSharhS(HEDRST[i]))}', " +
                            $"USER_NAME = N'{SqlText(HEDRST[i].USER_NAME)}', OKF = 1 WHERE NO_S = 9 AND N_S = {SqlNum(ns.Value)};");
                    }
                }

                if (newHeaderIndexes.Count > 0)
                {
                    var headerRequests = newHeaderIndexes.Select(i => new SanadHeaderRequest
                    {
                        DATE_S = Convert.ToInt64(HEDRST[i].DATE_N),
                        SHARH_S = BuildVorudSharhS(HEDRST[i]),
                        GHATEI = 0,
                        NO_S = 9,
                        OKF = 1,
                        USER_NAME = HEDRST[i].USER_NAME
                    }).ToList();

                    var reservedNumbers = ReserveSanadNumbersBatch(headerRequests);
                    for (int k = 0; k < newHeaderIndexes.Count; k++)
                    {
                        var idx = newHeaderIndexes[k];
                        HEDRST[idx].N_S = reservedNumbers[k];
                        headerUpdates.Add(
                            $"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(reservedNumbers[k])} WHERE NUMBER = {SqlNum(HEDRST[idx].NUMBER.Value)} AND TAG = 9;");
                    }
                }

                const int headUpdateChunkSize = 500;
                for (int offset = 0; offset < headerUpdates.Count; offset += headUpdateChunkSize)
                {
                    var b = new StringBuilder();
                    b.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                    foreach (var stmt in headerUpdates.Skip(offset).Take(headUpdateChunkSize)) { b.Append(stmt); }
                    b.Append("COMMIT TRANSACTION;");
                    dbms.DoExecuteSQL(b.ToString());
                }

                // ───────────────────────────────────────────────────────────────────────────
                // مرحله ۳: پیش‌گرم‌کردن کش نام کالا و حساب‌های موجود.
                // چرا امن است: STUF_DEF و TDETA_HES در طول این بازسازی توسط خودش نوشته نمی‌شوند
                // (به‌جز حساب‌هایی که CREATHES می‌سازد و خودش کش را به‌روز می‌کند).
                // ───────────────────────────────────────────────────────────────────────────
                var kalaNames = dbms.DoGetDataSQL<Custom_STUF_DEF>($@"SELECT DISTINCT T.CODE, S.NAME
                                                                     FROM (
                                                                         SELECT CODE FROM dbo.INVO_LST WHERE TAG = 9 AND NUMBER BETWEEN {NUMBER} AND {NUMBER2}
                                                                         UNION
                                                                         SELECT CODE FROM dbo.DTL_MANF
                                                                     ) AS T
                                                                     LEFT JOIN dbo.STUF_DEF S ON T.CODE = S.CODE").ToList();
                foreach (var k in kalaNames)
                {
                    if (!string.IsNullOrEmpty(k.CODE) && TryGetAccountCode(k.CODE, out var kCode))
                    {
                        _kalaNameCache[Convert.ToDouble(kCode)] = string.IsNullOrEmpty(k.NAME) ? " " : k.NAME;
                    }
                }

                var existingAccountsList = dbms.DoGetDataSQL<QRE13>($"SELECT N_KOL, NUMBER, TNUMBER FROM dbo.TDETA_HES WHERE N_KOL IN ({Baseknow.CONKAL}, {Baseknow.MOGODIA})").ToList();
                foreach (var acc in existingAccountsList)
                {
                    MarkAccountExists(acc.N_KOL ?? 0, acc.NUMBER ?? 0, acc.TNUMBER ?? 0);
                }

                bool isOption56_5 = Strings.Mid(Baseknow.OPTIONSS, 56, 1) == "5";

                // ردیف‌های ساخته‌شده و برگه‌هایی که بی‌خطا ساخته شدند.
                // ⚠️ برگه‌ی خراب نباید در حذف شرکت کند: کد قبلی ابتدا کل بازه را DELETE می‌کرد و
                //    درج را به انتها موکول می‌کرد؛ هر خطایی وسط کار یعنی «همه‌ی سندهای بازه بدون
                //    هیچ ردیفی». اینجا فقط برگه‌هایی حذف/بازنویسی می‌شوند که ردیف‌هایشان کامل ساخته شده.
                var deedDtlList = new System.Collections.Concurrent.ConcurrentBag<DEED_DTL_MODEL>();
                var rebuiltSheets = new System.Collections.Concurrent.ConcurrentBag<double>();

                var progressReporter = new ThrottledProgressReporter(
                    HEDRST.Count,
                    InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                    value =>
                    {
                        auto_run.PRGR_C7.Value = Math.Max(auto_run.PRGR_C7.Value, value);
                        auto_run.UpdateOverallProgressBar();
                    });

                var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);

                LogWriter.WriteLog(
                    $"سند ورود ساخته شده - تعداد برگه: {HEDRST.Count} | هدر جدید: {newHeaderIndexes.Count} | " +
                    $"موازی: {Generaly.UseParallelProcessing} | MaxDegreeOfParallelism: {dbParallelOptions.MaxDegreeOfParallelism}");

                if (!isOption56_5)
                {
                    string sqlChrst0 = $@"SELECT dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE,
                                                 SUM(dbo.INVO_LST.MEGH) AS SumOfMEGH, SUM(dbo.INVO_LST.MEGHk) AS SumOfMEGHk,
                                                 SUM(dbo.INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR, SUM(dbo.INVO_LST.MABL) AS SumOfMABL,
                                                 SUM(dbo.INVO_LST.MABL_K) AS SumOfMABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID,
                                                 dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO,
                                                 dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.STUF_DEF.NAME
                                          FROM dbo.STUF_DEF
                                          INNER JOIN dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE
                                          WHERE dbo.INVO_LST.TAG = 9 AND dbo.INVO_LST.NUMBER BETWEEN {NUMBER} AND {NUMBER2}
                                          GROUP BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE,
                                                   dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH,
                                                   dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.STUF_DEF.NAME";

                    var allChrst0 = dbms.DoGetDataSQL<QRE_BAZ_9>(sqlChrst0).GroupBy(x => Convert.ToInt64(x.NUMBER)).ToDictionary(g => g.Key, g => g.ToList());

                    string sqlJst0 = $@"SELECT DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER,
                                               Sum(INVO_LST.MEGHk) AS SumOfMEGHk, INVO_LST.CODE AS COM,
                                               [DTL_MANF].[MEGHk]+[PERT] AS MEGHM, INVO_LST.ANBAR
                                        FROM dbo.STUF_DEF
                                        INNER JOIN ((dbo.INVO_LST INNER JOIN dbo.HEAD_MANF ON dbo.INVO_LST.CODE = dbo.HEAD_MANF.CODE)
                                               INNER JOIN dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB) ON dbo.STUF_DEF.CODE = dbo.DTL_MANF.CODE
                                        WHERE dbo.INVO_LST.TAG = 9 AND dbo.INVO_LST.NUMBER BETWEEN {NUMBER} AND {NUMBER2}
                                        GROUP BY DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.CODE, [DTL_MANF].[MEGHk]+[PERT], INVO_LST.ANBAR";

                    var allJst0 = dbms.DoGetDataSQL<QRE_BAZ_10>(sqlJst0).GroupBy(x => (Num: Convert.ToInt64(x.NUMBER), Code: x.COM, Anbar: x.ANBAR)).ToDictionary(g => g.Key, g => g.ToList());

                    string sqlJst = $@"SELECT STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.MEGHk,
                                              HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR, INVO_LST.CODE
                                       FROM dbo.STUF_DEF
                                       INNER JOIN (dbo.INVO_LST INNER JOIN dbo.HEAD_MANF ON dbo.INVO_LST.CODE = dbo.HEAD_MANF.CODE) ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE
                                       WHERE dbo.INVO_LST.TAG = 9 AND dbo.INVO_LST.NUMBER BETWEEN {NUMBER} AND {NUMBER2}";

                    var allJst = dbms.DoGetDataSQL<QRE_BAZ_11>(sqlJst).GroupBy(x => (Num: Convert.ToInt64(x.NUMBER), Code: x.CODE)).ToDictionary(g => g.Key, g => g.ToList());

                    if (isDefaccChecked)
                    {
                        var accountsToEnsure = new HashSet<(double Kol, double Moin, double Taf, string Name)>();
                        foreach (var j0 in allJst0.Values.SelectMany(v => v))
                        {
                            if (TryGetAccountCode(j0.COM, out var comL) && TryGetAccountCode(j0.CODE, out var codeL))
                            {
                                accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, comL, codeL, string.IsNullOrEmpty(j0.NAME) ? " " : j0.NAME));
                            }
                        }
                        foreach (var j in allJst.Values.SelectMany(v => v))
                        {
                            if (TryGetAccountCode(j.CODE, out var codeL))
                            {
                                if ((j.IMBIBE_SAR ?? 0) * (j.MEGHk ?? 0) > 0)
                                    accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, codeL, 99999998, "سربار"));
                                if ((j.IMBIBE_MANF ?? 0) * (j.MEGHk ?? 0) > 0)
                                    accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, codeL, 99999999, "دستمزد"));
                            }
                        }

                        EnsureAccountsInParallel(accountsToEnsure, "SANADVORUDSAKHT");
                    }

                    ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, ROW =>
                    {
                        observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                        if (!sheetUsable[ROW])
                        {
                            progressReporter.ReportOne();
                            return;
                        }

                        var row = HEDRST[ROW];
                        long num = Convert.ToInt64(row.NUMBER);
                        double max_ns = row.N_S.Value;
                        var sheetRows = new List<DEED_DTL_MODEL>();

                        try
                        {
                            if (allChrst0.TryGetValue(num, out var chrst0List))
                            {
                                foreach (var chrst in chrst0List)
                                {
                                    double JAMCH = 0d;
                                    var keyJst0 = (Num: num, Code: chrst.CODE, Anbar: chrst.ANBAR);

                                    if (allJst0.TryGetValue(keyJst0, out var jst0List))
                                    {
                                        foreach (var j0 in jst0List)
                                        {
                                            double mablk = j0.MABLK ?? 0d;
                                            double sumMeghk = chrst.SumOfMEGHk ?? 0d;
                                            if (mablk * sumMeghk == 0d) { continue; }

                                            // ⚠️ کد قبلی اینجا Convert.ToDouble می‌زد: روی مقدار غیرعددی
                                            // FormatException می‌داد و — چون DELETE بازه‌ای از قبل انجام شده
                                            // بود — کل بازه بدون ردیف می‌ماند. حالا فقط همین قلم رد می‌شود.
                                            if (!TryGetAccountCode(j0.COM, out var comL) || !TryGetAccountCode(j0.CODE, out var codeL))
                                            {
                                                LogWriter.WriteLog($"SANADVORUDSAKHT: کد نامعتبر (COM='{j0.COM}', CODE='{j0.CODE}') در برگه {num}؛ این قلم ثبت نشد.");
                                                IsSuccessfully = false;
                                                continue;
                                            }

                                            var _hes = $"{Baseknow.CONKAL}-{Convert.ToDouble(comL)}-{Convert.ToDouble(codeL)}";
                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{(j0.MEGHM ?? 0d) * sumMeghk} جهت {Strings.Trim(chrst.NAME)}", 255);
                                            double _BES = Math.Round(mablk * sumMeghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)comL,
                                                HES_T = (int)codeL,
                                                HES = _hes,
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }
                                    }

                                    var keyJst = (Num: num, Code: chrst.CODE);
                                    if (allJst.TryGetValue(keyJst, out var jstList) && jstList.Count > 0
                                        && TryGetAccountCode(jstList[0].CODE, out var jstCodeL))
                                    {
                                        var jst = jstList[0];
                                        double meghk = chrst.SumOfMEGHk ?? 0d;
                                        double codeVal = Convert.ToDouble(jstCodeL);

                                        if ((jst.IMBIBE_SAR ?? 0d) * (jst.MEGHk ?? 0d) > 0d)
                                        {
                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{meghk} جهت {Strings.Trim(jst.NAME)}", 255);
                                            double _BES = Math.Round((jst.IMBIBE_SAR.Value) * meghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)jstCodeL,
                                                HES_T = 99999998,
                                                HES = $"{Baseknow.CONKAL}-{codeVal}-99999998",
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }

                                        if ((jst.IMBIBE_MANF ?? 0d) * (jst.MEGHk ?? 0d) > 0d)
                                        {
                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{meghk} جهت {Strings.Trim(jst.NAME)}", 255);
                                            double _BES = Math.Round((jst.IMBIBE_MANF.Value) * meghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)jstCodeL,
                                                HES_T = 99999999,
                                                HES = $"{Baseknow.CONKAL}-{codeVal}-99999999",
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }
                                    }

                                    if (JAMCH != 0d)
                                    {
                                        if (!TryGetAccountCode(chrst.CODE, out var chrstCodeL) || chrst.ANBAR == null)
                                        {
                                            throw new InvalidOperationException(
                                                $"کد کالا ('{chrst.CODE}') یا انبار برای برگه {num} معتبر نیست.");
                                        }

                                        double codeVal = Convert.ToDouble(chrstCodeL);
                                        var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{chrst.SumOfMEGHk} جهت {Strings.Trim(chrst.NAME)}", 255);

                                        sheetRows.Add(new DEED_DTL_MODEL
                                        {
                                            N_S = max_ns,
                                            HES_K = Convert.ToInt32(Baseknow.MOGODIA),
                                            HES_M = chrst.ANBAR.Value,
                                            HES_T = (int)chrstCodeL,
                                            HES = $"{Baseknow.MOGODIA}-{chrst.ANBAR}-{codeVal}",
                                            SHARH = _SHARH,
                                            BED = Math.Round(JAMCH),
                                            BES = 0,
                                            NUMBER = row.NUMBER ?? 0d,
                                            TAG = 9
                                        });
                                    }
                                }
                            }

                            foreach (var r in sheetRows) { deedDtlList.Add(r); }
                            rebuiltSheets.Add(row.NUMBER.Value);
                        }
                        catch (Exception ex)
                        {
                            IsSuccessfully = false;
                            LogWriter.WriteLog($"SANADVORUDSAKHT: خطا در برگه {num} (سند {max_ns}): {ex.Message} | Stack: {ex.StackTrace}");
                            if (!InternalCalling) { throw; }
                        }

                        progressReporter.ReportOne();
                    });
                }
                else
                {
                    string sqlChrst = $@"SELECT dbo.INVO_LST.NUMBER, dbo.INVO_LST.N_KOL, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE,
                                                SUM(dbo.INVO_LST.MEGH) AS SumOfMEGH, SUM(dbo.INVO_LST.MEGHk) AS SumOfMEGHk,
                                                SUM(dbo.INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR, SUM(dbo.INVO_LST.MABL) AS SumOfMABL,
                                                SUM(dbo.INVO_LST.MABL_K) AS SumOfMABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID,
                                                dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO,
                                                dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.STUF_DEF.NAME
                                         FROM dbo.STUF_DEF
                                         INNER JOIN dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE
                                         WHERE dbo.INVO_LST.TAG = 9 AND dbo.INVO_LST.NUMBER BETWEEN {NUMBER} AND {NUMBER2}
                                         GROUP BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.N_KOL, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE,
                                                  dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH,
                                                  dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K, dbo.STUF_DEF.NAME";

                    var allChrst = dbms.DoGetDataSQL<QRE_BAZ_12>(sqlChrst).GroupBy(x => Convert.ToInt64(x.NUMBER)).ToDictionary(g => g.Key, g => g.ToList());

                    string sqlJst1 = $@"SELECT dbo.HEAD_MANF.FNUMB, DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER,
                                               Sum(INVO_LST.MEGHk) AS SumOfMEGHk, INVO_LST.CODE AS COM,
                                               [DTL_MANF].[MEGHk]+[PERT] AS MEGHM, INVO_LST.anbar
                                        FROM dbo.STUF_DEF
                                        INNER JOIN ((dbo.INVO_LST INNER JOIN dbo.HEAD_MANF ON dbo.INVO_LST.CODE = dbo.HEAD_MANF.CODE)
                                               INNER JOIN dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB) ON dbo.STUF_DEF.CODE = dbo.DTL_MANF.CODE
                                        WHERE dbo.INVO_LST.TAG = 9 AND dbo.INVO_LST.NUMBER BETWEEN {NUMBER} AND {NUMBER2} AND dbo.HEAD_MANF.FNUMB = ISNULL(dbo.INVO_LST.N_KOL, 0)
                                        GROUP BY dbo.HEAD_MANF.FNUMB, DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.CODE, [DTL_MANF].[MEGHk]+[PERT], INVO_LST.anbar";

                    var allJst1 = dbms.DoGetDataSQL<QRE_BAZ_13>(sqlJst1).GroupBy(x => (Num: Convert.ToInt64(x.NUMBER), Code: x.COM, Anbar: x.anbar, Fnumb: x.FNUMB ?? 0)).ToDictionary(g => g.Key, g => g.ToList());

                    string sqlJst14 = $@"SELECT IMBIBE_MANF, IMBIBE_SAR, CODE, FNUMB FROM dbo.HEAD_MANF";
                    var allJst14 = dbms.DoGetDataSQL<QRE_BAZ_14>(sqlJst14).GroupBy(x => x.FNUMB ?? 0).ToDictionary(g => g.Key, g => g.ToList());

                    if (isDefaccChecked)
                    {
                        var accountsToEnsure = new HashSet<(double Kol, double Moin, double Taf, string Name)>();
                        foreach (var j1 in allJst1.Values.SelectMany(v => v))
                        {
                            if (TryGetAccountCode(j1.COM, out var comL) && TryGetAccountCode(j1.CODE, out var codeL))
                            {
                                accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, comL, codeL, string.IsNullOrEmpty(j1.NAME) ? " " : j1.NAME));
                            }
                        }
                        foreach (var c in allChrst.Values.SelectMany(v => v))
                        {
                            long fnumb = c.N_KOL.HasValue ? Convert.ToInt64(c.N_KOL.Value) : 0L;
                            if (allJst14.TryGetValue((int)fnumb, out var j14List) && j14List.Count > 0
                                && TryGetAccountCode(c.CODE, out var codeL))
                            {
                                var j14 = j14List[0];
                                if ((j14.IMBIBE_SAR ?? 0) * (c.SumOfMEGHk ?? 0) > 0)
                                    accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, codeL, 99999998, "سربار"));
                                if ((j14.IMBIBE_MANF ?? 0) * (c.SumOfMEGHk ?? 0) > 0)
                                    accountsToEnsure.Add((Baseknow.CONKAL ?? 0d, codeL, 99999999, "دستمزد"));
                            }
                        }

                        EnsureAccountsInParallel(accountsToEnsure, "SANADVORUDSAKHT");
                    }

                    ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, ROW =>
                    {
                        observedThreads.TryAdd(Environment.CurrentManagedThreadId, 0);

                        if (!sheetUsable[ROW])
                        {
                            progressReporter.ReportOne();
                            return;
                        }

                        var row = HEDRST[ROW];
                        long num = Convert.ToInt64(row.NUMBER);
                        double max_ns = row.N_S.Value;
                        var sheetRows = new List<DEED_DTL_MODEL>();

                        try
                        {
                            if (allChrst.TryGetValue(num, out var chrstList))
                            {
                                foreach (var chrst in chrstList)
                                {
                                    double JAMCH = 0d;
                                    long nKolVal = chrst.N_KOL.HasValue ? Convert.ToInt64(chrst.N_KOL.Value) : 0L;
                                    var keyJst1 = (Num: num, Code: chrst.CODE, Anbar: chrst.ANBAR, Fnumb: (int)nKolVal);

                                    if (allJst1.TryGetValue(keyJst1, out var jst1List))
                                    {
                                        foreach (var j1 in jst1List)
                                        {
                                            double mablk = j1.MABLK ?? 0d;
                                            double sumMeghk = chrst.SumOfMEGHk ?? 0d;
                                            if (mablk * sumMeghk == 0d) { continue; }

                                            if (!TryGetAccountCode(j1.COM, out var comL) || !TryGetAccountCode(j1.CODE, out var codeL))
                                            {
                                                LogWriter.WriteLog($"SANADVORUDSAKHT: کد نامعتبر (COM='{j1.COM}', CODE='{j1.CODE}') در برگه {num}؛ این قلم ثبت نشد.");
                                                IsSuccessfully = false;
                                                continue;
                                            }

                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{(j1.MEGHM ?? 0d) * sumMeghk} جهت {Strings.Trim(chrst.NAME)} فرمول: {Strings.Trim(chrst.N_KOL.ToString())}", 255);
                                            double _BES = Math.Round(mablk * sumMeghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)comL,
                                                HES_T = (int)codeL,
                                                HES = $"{Baseknow.CONKAL}-{Convert.ToDouble(comL)}-{Convert.ToDouble(codeL)}",
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }
                                    }

                                    if (allJst14.TryGetValue((int)nKolVal, out var j14List) && j14List.Count > 0
                                        && TryGetAccountCode(chrst.CODE, out var chrstCodeL))
                                    {
                                        var j14 = j14List[0];
                                        double sumMeghk = chrst.SumOfMEGHk ?? 0d;
                                        double codeVal = Convert.ToDouble(chrstCodeL);

                                        if ((j14.IMBIBE_SAR ?? 0d) * sumMeghk > 0d)
                                        {
                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{sumMeghk} جهت {Strings.Trim(chrst.NAME)} فرمول: {Strings.Trim(chrst.N_KOL.ToString())}", 255);
                                            double _BES = Math.Round((j14.IMBIBE_SAR.Value) * sumMeghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)chrstCodeL,
                                                HES_T = 99999998,
                                                HES = $"{Baseknow.CONKAL}-{codeVal}-99999998",
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }

                                        if ((j14.IMBIBE_MANF ?? 0d) * sumMeghk > 0d)
                                        {
                                            var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{sumMeghk} جهت {Strings.Trim(chrst.NAME)} فرمول: {Strings.Trim(chrst.N_KOL.ToString())}", 255);
                                            double _BES = Math.Round((j14.IMBIBE_MANF.Value) * sumMeghk);
                                            JAMCH += _BES;

                                            sheetRows.Add(new DEED_DTL_MODEL
                                            {
                                                N_S = max_ns,
                                                HES_K = Convert.ToInt32(Baseknow.CONKAL),
                                                HES_M = (int)chrstCodeL,
                                                HES_T = 99999999,
                                                HES = $"{Baseknow.CONKAL}-{codeVal}-99999999",
                                                SHARH = _SHARH,
                                                BES = _BES,
                                                BED = 0,
                                                NUMBER = row.NUMBER ?? 0d,
                                                TAG = 9
                                            });
                                        }
                                    }

                                    if (JAMCH != 0d)
                                    {
                                        if (!TryGetAccountCode(chrst.CODE, out var codeL2) || chrst.ANBAR == null)
                                        {
                                            throw new InvalidOperationException(
                                                $"کد کالا ('{chrst.CODE}') یا انبار برای برگه {num} معتبر نیست.");
                                        }

                                        double codeVal = Convert.ToDouble(codeL2);
                                        var _SHARH = Strings.Left($"برگه ورود شماره {row.NUMBER}-{row.FNUMCO} مورخ {Strings.Format(row.DATE_N, "####/##/##")} به مقدار{chrst.SumOfMEGHk} جهت {Strings.Trim(chrst.NAME)} فرمول: {Strings.Trim(chrst.N_KOL.ToString())}", 255);

                                        sheetRows.Add(new DEED_DTL_MODEL
                                        {
                                            N_S = max_ns,
                                            HES_K = Convert.ToInt32(Baseknow.MOGODIA),
                                            HES_M = chrst.ANBAR.Value,
                                            HES_T = (int)codeL2,
                                            HES = $"{Baseknow.MOGODIA}-{chrst.ANBAR}-{codeVal}",
                                            SHARH = _SHARH,
                                            BED = JAMCH,
                                            BES = 0,
                                            NUMBER = row.NUMBER ?? 0d,
                                            TAG = 9
                                        });
                                    }
                                }
                            }

                            foreach (var r in sheetRows) { deedDtlList.Add(r); }
                            rebuiltSheets.Add(row.NUMBER.Value);
                        }
                        catch (Exception ex)
                        {
                            IsSuccessfully = false;
                            LogWriter.WriteLog($"SANADVORUDSAKHT: خطا در برگه {num} (سند {max_ns}): {ex.Message} | Stack: {ex.StackTrace}");
                            if (!InternalCalling) { throw; }
                        }

                        progressReporter.ReportOne();
                    });
                }

                // حذفِ ردیف‌های قبلی و درج ردیف‌های تازه در «یک» تراکنش انجام می‌شود، و فقط برای
                // برگه‌هایی که کامل ساخته شدند. اگر چیزی خطا بدهد، سند قبلی دست‌نخورده می‌ماند.
                var deleteStatements = new List<string>();
                var sheetNumbers = rebuiltSheets.Distinct().ToList();
                const int deleteChunkSize = 1000;
                for (int offset = 0; offset < sheetNumbers.Count; offset += deleteChunkSize)
                {
                    var inClause = string.Join(",", sheetNumbers.Skip(offset).Take(deleteChunkSize).Select(n => SqlNum(n)));
                    deleteStatements.Add($"DELETE FROM dbo.DEED_DTL WHERE TAG = 9 AND NUMBER IN ({inClause})");
                }

                BulkInsertDeedDtl(deedDtlList, deleteStatements);

                stopwatch.Stop();
                progressReporter.Complete();

                LogWriter.WriteLog(
                    $"SANADVORUDSAKHT: پایان بازسازی - {HEDRST.Count} برگه ({sheetNumbers.Count} بازسازی‌شده) در " +
                    $"{stopwatch.Elapsed.TotalSeconds:F1} ثانیه با {observedThreads.Count} Thread همزمان");

                for (int i = HEDRST.Count - 1; i >= 0; i--)
                {
                    if (sheetUsable[i] && HEDRST[i].N_S != null)
                    {
                        SANAD_NUMBER = HEDRST[i].N_S;
                        break;
                    }
                }
            }
            finally
            {
                if (cacheOwnedHere)
                {
                    LookupCacheEnabled = false;
                    ClearLookupCaches();
                }
            }

            return (SANAD_NUMBER, IsSuccessfully);
        }

        /// <summary>
        /// ساخت دسته‌ای حساب‌های تفصیلی، به‌صورت موازی.
        /// سریال انجام دادنش روی یک TDETA_HES سرد به N رفت‌وبرگشت پشت‌سرهم تبدیل می‌شود و
        /// دقیقاً همان چیزی را از بین می‌برد که موازی‌سازی برای آن انجام شده.
        /// </summary>
        private static void EnsureAccountsInParallel(IEnumerable<(double Kol, double Moin, double Taf, string Name)> accounts, string caller)
        {
            var list = accounts.ToList();
            if (list.Count == 0) { return; }

            var options = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(list.Count);
            ExecuteWithPreferredLoop(0, list.Count, options, i =>
            {
                var acc = list[i];
                try
                {
                    CREATHES(acc.Kol, acc.Moin, acc.Taf, acc.Name);
                }
                catch (Exception ex)
                {
                    LogWriter.WriteLog($"[{caller}] خطا در ساخت دسته‌ای حساب {acc.Kol}-{acc.Moin}-{acc.Taf}: {ex.Message}");
                }
            });

            LogWriter.WriteLog($"[{caller}] پیش‌ساخت حساب‌ها: {list.Count} حساب بررسی/ساخته شد.");
        }

        /// <summary>
        /// درج دسته‌ای ردیف‌های سند با SqlBulkCopy، به‌همراه دستورهای حذفِ مقدم — همه در یک تراکنش.
        /// </summary>
        /// <param name="dtlsToInsert">ردیف‌های آماده‌ی درج.</param>
        /// <param name="preStatements">
        /// دستورهایی (معمولاً DELETE) که باید «داخل همان تراکنش و پیش از درج» اجرا شوند.
        /// اجرای حذف بیرون از این تراکنش باعث می‌شد هر خطای بعدی سند را بدون ردیف رها کند.
        /// </param>
        private static void BulkInsertDeedDtl(IEnumerable<DEED_DTL_MODEL> dtlsToInsert, IReadOnlyList<string>? preStatements = null)
        {
            var list = dtlsToInsert.ToList();
            var hasPre = preStatements != null && preStatements.Count > 0;
            if (list.Count == 0 && !hasPre) { return; }

            using (var conn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        if (hasPre)
                        {
                            foreach (var stmt in preStatements)
                            {
                                conn.Execute(stmt, null, tx, commandTimeout: 3600);
                            }
                        }

                        if (list.Count > 0)
                        {
                            // ⚠️ CheckConstraints حتماً لازم است: SqlBulkCopy به‌صورت پیش‌فرض
                            // قیدهای CHECK و FOREIGN KEY را بررسی نمی‌کند. یعنی FK_DEED_DTL_TDETA_HES
                            // — همان قیدی که کل منطق CREATHES برای رعایتش نوشته شده — دور زده می‌شد،
                            // ردیف با حساب ناموجود بی‌صدا درج می‌شد و SQL Server آن FK را
                            // is_not_trusted علامت می‌زد.
                            using (var bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tx))
                            {
                                bulkCopy.DestinationTableName = "dbo.DEED_DTL";
                                bulkCopy.BulkCopyTimeout = 3600;

                                var dt = new DataTable();
                                dt.Columns.Add("N_S", typeof(double));
                                dt.Columns.Add("HES_K", typeof(int));
                                dt.Columns.Add("HES_M", typeof(int));
                                dt.Columns.Add("HES_T", typeof(int));
                                dt.Columns.Add("HES_T2", typeof(int));
                                dt.Columns.Add("HES_T3", typeof(int));
                                dt.Columns.Add("HES_T4", typeof(int));
                                dt.Columns.Add("HES", typeof(string));
                                dt.Columns.Add("SHARH", typeof(string));
                                dt.Columns.Add("BED", typeof(double));
                                dt.Columns.Add("BES", typeof(double));
                                dt.Columns.Add("N_SERI", typeof(double));
                                dt.Columns.Add("BANK", typeof(int));
                                dt.Columns.Add("NUMBER", typeof(double));
                                dt.Columns.Add("TAG", typeof(double));
                                dt.Columns.Add("ARZD", typeof(double));
                                dt.Columns.Add("CRT", typeof(DateTime));
                                dt.Columns.Add("UID", typeof(int));

                                DateTime now = DateTime.Now;
                                int uid = (int)(Baseknow.USERCOD ?? 0);

                                foreach (var d in list)
                                {
                                    dt.Rows.Add(
                                        d.N_S,
                                        d.HES_K,
                                        d.HES_M,
                                        d.HES_T,
                                        (object?)d.HES_T2 ?? DBNull.Value,
                                        (object?)d.HES_T3 ?? DBNull.Value,
                                        (object?)d.HES_T4 ?? DBNull.Value,
                                        d.HES ?? "",
                                        d.SHARH ?? "",
                                        d.BED,
                                        d.BES,
                                        (object?)d.N_SERI ?? DBNull.Value,
                                        (object?)d.BANK ?? DBNull.Value,
                                        d.NUMBER,
                                        d.TAG,
                                        (object?)d.ARZD ?? DBNull.Value,
                                        now,
                                        uid);
                                }

                                foreach (DataColumn col in dt.Columns)
                                {
                                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }

                                bulkCopy.WriteToServer(dt);
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* اتصال از دست رفته؛ سرور خودش Rollback می‌کند */ }
                        throw;
                    }
                }
            }
        }

        public static void gensanadbargashfroosh(long fnum, long TNUM, bool InternalCalling = true)
        {
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            bool isDefaccChecked = Generaly.defacc;

            long CON, i;

            //var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            var HFRST = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM dbo.HEAD_LST WHERE     (NUMBER BETWEEN " + fnum + " AND " + TNUM + ") AND (TAG = 4)").ToList();

            var progressCounter = 0;

            LogWriter.WriteLog("شروع باز سازي از فاکتور برشگت فروش شماره : " + fnum + " تا فاكتور شماره :" + TNUM + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);
            ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, ROW =>
            //for (int ROW = 0; ROW < HFRST.Count; ROW++) //while (!HFRST.EOF)
            {
                object a = default, fs;
                double? max_ns, MABL_CHK = null, JAMF, JAMCH, CKOL = null, JAMFKH;
                double MBL;
                double? CMOIN = null, CTAF = null, takh;
                string shart;
                int ii;
                string CH;
                double JAMP;
                string TAMIR;
                string per;
                long permab;
                double TAKHF;
                double? CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, MAVAD;
                double DAST;
                double SAR;

                var processed = Interlocked.Increment(ref progressCounter);

                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = processed / ((double)HFRST.Count) * 100.0; // Calculate the progress percentage
                        auto_run.PRGR_C8.Value = progress; // Update the progress bar
                                                           // auto_run.LBL_C8.Content = $"{progress:F2}%";
                        auto_run.UpdateOverallProgressBar();

                    }));
                }

                //Forms["GUG"]["num"] = i;
                if (!IsNull(HFRST[ROW].CUST_NO))
                {
                    GETTAF3(HFRST[ROW].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }

                //if (IsNull(HFRST[ROW].CUST_NO))
                //{
                //    LogWriter.WriteLog($"برگشت فروش شماره {HFRST[ROW].NUMBER} : کد مشتری : [{HFRST[ROW].CUST_NO}] خالی است.");
                //    continue;
                //}               

                string SHSH = Strings.Right("فاكتور برگشت فروش شماره " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 100);
                if ((bool)Baseknow.SNDKH) // سند روزانه است
                {
                    List<QRE10> SARST = null;
                    if (!IsNull(HFRST[ROW].N_S)) // فاکتور سند دارد
                    {
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and n_s = " + HFRST[ROW].N_S).ToList();
                        if (SARST.Count > 0)  // اگرسند  فاکتورهست
                        {
                            if (SARST.Select(x => x.DATE_S).FirstOrDefault() == HFRST[ROW].DATE_N) // تاريخ سند و فاکتوريکي است
                            {
                                max_ns = (double)HFRST[ROW].N_S;
                            }
                            else
                            {
                            SEJ:
                                SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[ROW].DATE_N).ToList();
                                if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                                {
                                    max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                                }
                                else
                                {
                                    max_ns = Createsanad((long)HFRST[ROW].DATE_N, SHSH, 0, 4, -1, HFRST[ROW].USER_NAME);

                                    HFRST[ROW].N_S = max_ns;
                                }
                            }
                        }
                        else
                        {
                            //goto SEJ;
                            SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[ROW].DATE_N).ToList();
                            if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                            {
                                max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                            }
                            else
                            {
                                max_ns = Createsanad((long)HFRST[ROW].DATE_N, SHSH, 0, 4, -1, HFRST[ROW].USER_NAME);

                                HFRST[ROW].N_S = max_ns;
                            }
                        } // چک کن اگه نيست صادر کن
                    }
                    else
                    {
                        //goto SEJ;
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[ROW].DATE_N).ToList();
                        if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                        {
                            max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                        }
                        else
                        {
                            max_ns = Createsanad((long)HFRST[ROW].DATE_N, SHSH, 0, 4, -1, HFRST[ROW].USER_NAME);

                            HFRST[ROW].N_S = max_ns;
                        }
                    } // چک کن اگه نيست صادر کن
                }
                else if (!IsNull(HFRST[ROW].N_S)) // تک سندي
                                                  // فاکتور سند دارد
                {
                    var SARST = dbms.DoGetDataSQL<QRE11>("SELECT    n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and N_s = " + HFRST[ROW].N_S).ToList();
                    if (SARST.Count > 0)   // اگرسند فاکتورهست
                    {
                        if (SARST.Select(x => x.DATE_S).FirstOrDefault() != HFRST[ROW].DATE_N) // تاريخ سند و فاکتوريکي است
                        {
                            dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + HFRST[ROW].DATE_N + ",SHARH_S = '" + SHSH + "',GHATEI = 0,NO_S = 4,OKF=-1,USER_NAME ='" + HFRST[ROW].USER_NAME + "' WHERE N_S =" + HFRST[ROW].N_S);
                        }
                        max_ns = (double)HFRST[ROW].N_S;
                    }
                    else
                    {
                        max_ns = Createsanad((long)HFRST[ROW].DATE_N, SHSH, 0, 4, -1, HFRST[ROW].USER_NAME);
                        HFRST[ROW].N_S = max_ns;
                    }
                }
                else
                {
                    max_ns = Createsanad((long)HFRST[ROW].DATE_N, SHSH, 0, 4, -1, HFRST[ROW].USER_NAME);
                    HFRST[ROW].N_S = max_ns;
                }
                if (IsNull(HFRST[ROW].N_S) || HFRST[ROW].N_S != max_ns)
                {
                    HFRST[ROW].N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {HFRST[ROW].NUMBER} AND (TAG = 4)) ");
                }
                var JST_0 = dbms.DoGetDataSQL<double?>("SELECT Sum([MEGH_MAR]*[mabl]) AS mabk  FROM dbo.INVO_LST WHERE     (NUMBER = " + HFRST[ROW].NUMBER1 + ") AND (TAG = 2) ").ToList();
                if (JST_0.Count > 0 && !IsNull(JST_0.FirstOrDefault()))
                {
                    JAMF = (double)JST_0.FirstOrDefault();
                }
                else
                {
                    JAMF = 0d;
                }
                var JST_1 = dbms.DoGetDataSQL<double?>("SELECT Sum([MEGH_MAR]*[mabl]) AS mabk FROM dbo.INVO_LST WHERE     (NUMBER = " + HFRST[ROW].NUMBER1 + ") AND (TAG = 2) AND (ANBAR = 0)").ToList();
                if (JST_1.Count > 0 && !IsNull(JST_1.FirstOrDefault()))
                {
                    JAMFKH = (double)JST_1.FirstOrDefault();
                }
                else
                {
                    JAMFKH = 0d;
                }
                ;
                var JST_2 = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETD.MABL) AS SumOfMABL FROM PAY_GETD WHERE (((PAY_GETD.TAG)=4) AND ((PAY_GETD.NUMBER)= " + HFRST[ROW].NUMBER + " ))").ToList();
                if (JST_2.Count > 0 && !IsNull(JST_2.FirstOrDefault()))
                {
                    JAMCH = (double)JST_2.FirstOrDefault();
                }
                else
                {
                    JAMCH = 0d;
                }
                TAKHF = 0d;
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HFRST[ROW].NUMBER + ") AND ((DEED_DTL.TAG)= " + 4 + "))");
                var JST_3 = dbms.DoGetDataSQL<QRE_BAZ_15>("SELECT  dbo.INVO_LST_TAKH.MEGH_MAR * dbo.INVO_LST_TAKH.MABL AS MABL_K, dbo.INVO_LST_TAKH.MEGH_MAR, dbo.INVO_LST_TAKH.CODE, dbo.INVO_LST_TAKH.ANBAR, dbo.STUF_DEF.NAME, dbo.INVO_LST_TAKH.CUST_KIND, ISNULL(dbo.TAKHPERS.TAFPER, 0) AS TFP, ROUND(ISNULL(dbo.TAKHPERS.TAFPER, 0) * dbo.INVO_LST_TAKH.MEGH_MAR * dbo.INVO_LST_TAKH.mabl / 100, 0) As takh,dbo.INVO_LST_TAKH.AVRAGE   FROM  dbo.STUF_DEF INNER JOIN  dbo.INVO_LST_TAKH ON dbo.STUF_DEF.CODE = dbo.INVO_LST_TAKH.CODE LEFT OUTER JOIN   dbo.TAKHPERS ON dbo.INVO_LST_TAKH.CUST_KIND = dbo.TAKHPERS.CUST_CO AND    dbo.INVO_LST_TAKH.CODE = dbo.TAKHPERS.TAKH_COD WHERE     (dbo.INVO_LST_TAKH.MEGH_MAR <> 0) AND (dbo.INVO_LST_TAKH.NUMBER = " + HFRST[ROW].NUMBER1 + ") AND (dbo.INVO_LST_TAKH.TAG = 2)").ToList();
                for (int Y = 0; Y < JST_3.Count; Y++) //while (!JST.EOF())
                {
                    if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                    {
                        if (isDefaccChecked && !ISHESAB(Baseknow.MFROSH, 4, Convert.ToInt64(JST_3[Y].CODE)))
                        {
                            try
                            {
                                CREATHES(Baseknow.MFROSH, 4, Convert.ToInt64(JST_3[Y].CODE), JST_3[Y].NAME); //JST_3[Y].(4)
                            }
                            catch (Exception ex)
                            {
                                ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                            }
                        }
                        //SDRST.AddNew(); // فروش
                        //SDRST.Fields("N_S") = max_ns;
                        //SDRST.Fields("HES_K") = Baseknow.MFROSH;
                        //SDRST.Fields("HES_M") = 4;
                        //SDRST.Fields("HES_T") = JST_3[Y].CODE;
                        var _hes = Baseknow.MFROSH + "-4-" + Convert.ToDouble(JST_3[Y].CODE);
                        var _SHARH = Strings.Left("برگشت فروش  فاكتور شماره " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                        //SDRST.Fields("BED") = JST_3[Y].MABL_K;
                        //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                        //SDRST.Fields("TAG") = 4;

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,             HES_K,  HES_M,        HES_T,         hes,      SHARH,           BED,         NUMBER,         TAG)
                                                             VALUES ({max_ns}, {Baseknow.MFROSH}, 4,    {JST_3[Y].CODE},  N'{_hes}',N'{_SHARH}',{JST_3[Y].MABL_K},{HFRST[ROW].NUMBER} ,4)");

                    }
                    else if (JST_3[Y].ANBAR != 0)
                    {
                        if (isDefaccChecked && !ISHESAB(Baseknow.MFROSH, Convert.ToDouble(JST_3[Y].CODE), Convert.ToInt64(JST_3[Y].CODE)))
                        {
                            try
                            {
                                CREATHES(Baseknow.MFROSH, Convert.ToDouble(JST_3[Y].CODE), Convert.ToInt64(JST_3[Y].CODE), JST_3[Y].NAME); //JST_3[Y].(4)
                            }
                            catch (Exception ex)
                            {
                                ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                            }
                        }
                        if (JST_3[Y].MABL_K > 0)
                        {
                            //SDRST.AddNew(); // فروش
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.MFROSH;
                            string HES_M = "";
                            string HES_T = "";
                            string hes = "";
                            if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                            {
                                HES_M = "1";
                                HES_T = "1";
                                hes = Baseknow.MFROSH + "-1-1";
                            }
                            else
                            {
                                HES_M = JST_3[Y].CODE;
                                HES_T = JST_3[Y].CODE;
                                hes = Baseknow.MFROSH + "-" + Convert.ToDouble(JST_3[Y].CODE) + "-" + Convert.ToDouble(JST_3[Y].CODE);
                            }
                            var _SHARH = Strings.Left("برگشت فروش  فاكتور شماره " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                            var _BED = JST_3[Y].MABL_K;
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,  HES_K,           HES_M,        HES_T,         hes,      SHARH,    BED,         NUMBER,    TAG)
                                                             VALUES ({max_ns}, {Baseknow.MFROSH}, {HES_M},     {HES_T},     N'{hes}',N'{_SHARH}',{_BED},{HFRST[ROW].NUMBER} ,4)");
                            //SDRST.update();
                        }
                    }
                    else
                    {
                        if (isDefaccChecked && !ISHESAB(Baseknow.DARAM, HFRST[ROW].DEPATMAN, Convert.ToInt64(JST_3[Y].CODE)))
                        {
                            try
                            {
                                CREATHES(Baseknow.DARAM, HFRST[ROW].DEPATMAN, Convert.ToInt64(JST_3[Y].CODE), JST_3[Y].NAME); //JST_3[Y].(4)
                            }
                            catch (Exception ex)
                            {
                                ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                            }
                        }
                        //SDRST.AddNew(); // در آمد
                        //SDRST.Fields("N_S") = max_ns;
                        //SDRST.Fields("HES_K") = Baseknow.DARAM;
                        //SDRST.Fields("HES_M") = HFRST[ROW].DEPATMAN;
                        //SDRST.Fields("HES_T") = JST_3[Y].CODE;
                        var _hes = Baseknow.DARAM + "-" + HFRST[ROW].DEPATMAN + "-" + Convert.ToDouble(JST_3[Y].CODE);
                        var _SHARH = Strings.Left("برگشت فروش  فاكتور شماره " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                        //SDRST.Fields("BEd") = JST_3[Y].MABL_K;
                        //// jamkh = jamkh + JST_3[Y].("MABL_K")
                        //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                        //SDRST.Fields("TAG") = 4;
                        //SDRST.update();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,         HES_K,                  HES_M,              HES_T,          hes,      SHARH,         BED,           NUMBER,         TAG)
                                                             VALUES ({max_ns}, {Baseknow.DARAM},    {HFRST[ROW].DEPATMAN}, {JST_3[Y].CODE},  N'{_hes}',N'{_SHARH}',{JST_3[Y].MABL_K},{HFRST[ROW].NUMBER} ,4)");
                    }

                    if (Baseknow.SANAT == true || IsNull(Baseknow.SANAT))
                    {
                        MAVAD = Math.Round((double)(GETSTANDARDPRICE_MAVAD(JST_3[Y].CODE, HFRST[ROW].DATE_N) * JST_3[Y].MEGH_MAR));
                        DAST = Math.Round((double)(GETSTANDARDPRICE_DAST(JST_3[Y].CODE, HFRST[ROW].DATE_N) * JST_3[Y].MEGH_MAR));
                        SAR = Math.Round((double)(GETSTANDARDPRICE_SAR(JST_3[Y].CODE, HFRST[ROW].DATE_N) * JST_3[Y].MEGH_MAR));
                        if (isDefaccChecked && !ISHESAB(Baseknow.MOGODIA, JST_3[Y].ANBAR, Convert.ToInt64(JST_3[Y].CODE)))
                        {
                            try
                            {
                                CREATHES(Baseknow.MOGODIA, JST_3[Y].ANBAR, Convert.ToInt64(JST_3[Y].CODE), JST_3[Y].NAME); //JST_3[Y].(4)
                            }
                            catch (Exception ex)
                            {
                                ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                            }
                        }
                        if (MAVAD + DAST + SAR != 0d && Strings.Mid(Baseknow.OPTIONSS, 66, 1) != "5")
                        {
                            //SDRST.AddNew(); // انبار محصول
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.MOGODIA;
                            //SDRST.Fields("HES_M") = JST_3[Y].ANBAR;
                            //SDRST.Fields("HES_T") = JST_3[Y].CODE;
                            var _SHARH = Strings.Left("برگشت فروش.  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت " + Strings.Trim(JST_3[Y].NAME), 255);
                            var _hes = Baseknow.MOGODIA + "-" + JST_3[Y].ANBAR + "-" + Convert.ToDouble(JST_3[Y].CODE);
                            var _BED = MAVAD + DAST + SAR;
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;
                            //SDRST.update();

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,         HES_K,                  HES_M,      HES_T,          hes,      SHARH,      BED,           NUMBER,   TAG)
                                                             VALUES ({max_ns}, {Baseknow.MOGODIA},    {JST_3[Y].ANBAR}, {JST_3[Y].CODE},  N'{_hes}',N'{_SHARH}',{_BED},{HFRST[ROW].NUMBER} ,4)");
                            if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) != 1d)
                            {
                                if (isDefaccChecked && !ISHESAB(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), Convert.ToInt64(JST_3[Y].CODE)))
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), Convert.ToInt64(JST_3[Y].CODE), JST_3[Y].NAME); //JST_3[Y].(4)
                                    }
                                    catch (Exception ex)
                                    {
                                        ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                                    }
                                }
                            }
                            if (MAVAD > 0d)
                            {
                                //SDRST.AddNew(); // قيمت تمام شده
                                //SDRST.Fields("N_S") = max_ns;
                                //SDRST.Fields("HES_K") = Baseknow.GHEYMAT;
                                string HES_M = "";
                                string HES_T = "";
                                string hes = "";
                                if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                                {
                                    HES_M = "1";
                                    HES_T = "1";
                                    hes = Baseknow.GHEYMAT + "-1-1";
                                }
                                else
                                {
                                    HES_M = JST_3[Y].CODE;
                                    HES_T = JST_3[Y].CODE;
                                    hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(JST_3[Y].CODE) + "-" + Convert.ToDouble(JST_3[Y].CODE);
                                }
                                var SHARH = Strings.Left("برگشت فروش  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                                //SDRST.Fields("BES") = MAVAD;
                                //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                                //SDRST.Fields("TAG") = 4;
                                //SDRST.update();

                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,         HES_K,      HES_M,  HES_T,      hes,      SHARH,        BES,           NUMBER,         TAG)
                                                             VALUES ({max_ns}, {Baseknow.GHEYMAT},    {HES_M}, {HES_T},  N'{hes}',N'{SHARH}',     {MAVAD},    {HFRST[ROW].NUMBER} ,4)");
                            }
                            if (DAST != 0d)
                            {
                                if (isDefaccChecked && !ISHESAB(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), 9999999))
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), 9999999, "دستمزد " + JST_3[Y].NAME); //JST_3[Y].(4)
                                    }
                                    catch (Exception ex)
                                    {
                                        ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                                    }
                                }
                                //SDRST.AddNew(); // قيمت تمام شده
                                //SDRST.Fields("N_S") = max_ns;
                                //SDRST.Fields("HES_K") = Baseknow.GHEYMAT;
                                //SDRST.Fields("HES_M") = JST_3[Y].CODE;
                                //SDRST.Fields("HES_T") = 9999999;
                                var SHARH_ = Strings.Left("برگشت فروش  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                                var hes_ = Baseknow.GHEYMAT + "-" + Convert.ToDouble(JST_3[Y].CODE) + "-" + 9999999;
                                //SDRST.Fields("BES") = DAST;
                                //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                                //SDRST.Fields("TAG") = 4;
                                //SDRST.update();

                                dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,          HES_M,              HES_T,          hes,         SHARH,          BES,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.GHEYMAT}, {JST_3[Y].CODE},     {9999999},      N'{hes_}',	  N'{SHARH_}',	   {DAST},		 {HFRST[ROW].NUMBER}  ,4)");
                            }
                            if (SAR != 0d)
                            {
                                if (isDefaccChecked && !ISHESAB(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), 9999998))
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.GHEYMAT, Convert.ToDouble(JST_3[Y].CODE), 9999998, "سربار " + JST_3[Y].NAME); //JST_3[Y].(4)
                                    }
                                    catch (Exception ex)
                                    {
                                        ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                                    }
                                }
                                //SDRST.AddNew(); // قيمت تمام شده
                                //SDRST.Fields("N_S") = max_ns;
                                //SDRST.Fields("HES_K") = Baseknow.GHEYMAT;
                                //SDRST.Fields("HES_M") = JST_3[Y].CODE;
                                //SDRST.Fields("HES_T") = 9999998;
                                var SHARH__ = Strings.Left("برگشت فروش  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                                var hes__ = Baseknow.GHEYMAT + "-" + Convert.ToDouble(JST_3[Y].CODE) + "-" + 9999998;
                                //SDRST.Fields("BES") = SAR;
                                //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                                //SDRST.Fields("TAG") = 4;
                                //SDRST.update();
                                dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,          HES_M,              HES_T,          hes,         SHARH,          BES,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.GHEYMAT}, {JST_3[Y].CODE},     {9999998},      N'{hes__}',    N'{SHARH__}',  {SAR},		 {HFRST[ROW].NUMBER}  ,4)");
                            }
                        }
                        else if (JST_3[Y].AVRAGE > 0)
                        {
                            //SDRST.AddNew(); // انبار محصول
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.MOGODIA;
                            //SDRST.Fields("HES_M") = JST_3[Y].ANBAR;
                            //SDRST.Fields("HES_T") = JST_3[Y].CODE;
                            var SHARH_ = Strings.Left("برگشت فروش  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                            var hes_ = Baseknow.MOGODIA + "-" + JST_3[Y].ANBAR + "-" + Convert.ToDouble(JST_3[Y].CODE);
                            var BED_ = Math.Round((double)(JST_3[Y].AVRAGE * JST_3[Y].MEGH_MAR));
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;

                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,               HES_M,                   HES_T,          hes,         SHARH,          BED,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.MOGODIA}, {JST_3[Y].ANBAR},     {JST_3[Y].CODE},      N'{hes_}',    N'{SHARH_}',  {BED_},		 {HFRST[ROW].NUMBER}  ,4)");

                            //SDRST.update();

                            //SDRST.AddNew(); // قيمت تمام شده
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.GHEYMAT;
                            string HES_M = "";
                            string HES_T = "";
                            string hes = "";
                            if (SafeToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                            {
                                HES_M = "1";
                                HES_T = "1";
                                hes = Baseknow.GHEYMAT + "-1-1";
                            }
                            else
                            {
                                HES_M = JST_3[Y].CODE;
                                HES_T = JST_3[Y].CODE;
                                hes = Baseknow.GHEYMAT + "-" + Convert.ToDouble(JST_3[Y].CODE) + "-" + Convert.ToDouble(JST_3[Y].CODE);
                            }
                            var _SHARH_ = Strings.Left("برگشت فروش  فاكتور شماره  " + HFRST[ROW].NUMBER + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_3[Y].MEGH_MAR + " برگشت فروش " + Strings.Trim(JST_3[Y].NAME), 255);
                            var _BES_ = Math.Round((double)(JST_3[Y].AVRAGE * JST_3[Y].MEGH_MAR));
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;

                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,            HES_M,       HES_T,          hes,         SHARH,         BES,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.GHEYMAT}, {HES_M},     {HES_T},      N'{hes}',    N'{_SHARH_}',  {_BES_},		 {HFRST[ROW].NUMBER}  ,4)");

                            //SDRST.update();
                        }
                    }
                    //JST.MoveNext();
                    //DoEvents();
                }

                var JST = dbms.DoGetDataSQL<QRE_BAZ_16>("SELECT INVO_LST.MABL_K, INVO_LST.MEGH_MAR, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, [SumOfMABLK]+[IMBIBE_MANF]+[IMBIBE_SAR] AS GHT FROM (INVO_LST INNER JOIN MANF_JAMK ON INVO_LST.CODE = MANF_JAMK.CODE) INNER JOIN STUF_DEF ON INVO_LST.CODE = STUF_DEF.CODE WHERE (((INVO_LST.NUMBER)=" + HFRST[ROW].NUMBER1 + ") AND ((INVO_LST.TAG)=2))").ToList();
                if (HFRST[ROW].MABL_HAZ != 0)
                {
                    if (IsNull(HFRST[ROW].MOIN_HAZ))
                    {
                        auto_run.Dispatcher.Invoke(new Action(() =>
                        {
                            new Msgwin(false, "اخطار مهم ...! حساب معين سرويس مشخص نشده است و سند صادره ناقص خواهد بود حتما حساب معين سرويس را مشخص نمائيد.", "#FFF0000").ShowDialog();
                        }));
                    }
                    else
                    {
                        //SDRST.AddNew(); // كرايه حمل يا غيره
                        if (!IsNull(HFRST[ROW].MOIN_HAZ))
                        {
                            GETTAF3(HFRST[ROW].MOIN_HAZ, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        }
                        //SDRST.Fields("N_S") = max_ns;
                        //SDRST.Fields("HES_K") = HKOL;
                        //SDRST.Fields("HES_M") = HMOIN;
                        //SDRST.Fields("HES_T") = HTAF;
                        //SDRST.Fields("HES_T2") = HTAF2;
                        //SDRST.Fields("HES_T3") = HTAF3;
                        //SDRST.Fields("HES_T4") = HTAF4;
                        var _hes_ = HFRST[ROW].MOIN_HAZ;
                        var _SHARH_ = Strings.Right("فاكتور برگشت فروش  شماره " + HFRST[ROW].NUMBER + " - " + GETTAFNAME(HFRST[ROW].MOIN_HAZ), 255);
                        var _BED_ = HFRST[ROW].MABL_HAZ;
                        //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                        //SDRST.Fields("TAG") = 4;
                        //SDRST.update();

                        string HTAF2T = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                        string HTAF3T = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                        string HTAF4T = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4,                    hes,          SHARH,    BED,       NUMBER,         TAG)
                                                 VALUES ({max_ns}, {HKOL}, {HMOIN}, {HTAF}, {HTAF2T}, {HTAF3T}, {HTAF4T},        N'{_hes_}',    N'{_SHARH_}', {_BED_}, {HFRST[ROW].NUMBER}, 4)");

                        //dbms.DoExecuteSQL($@"INSERT INTO dbo.SDRST (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4,                    hes,          SHARH,    BED,       NUMBER,         TAG)
                        //                         VALUES ({max_ns}, {HKOL}, {HMOIN}, {HTAF}, {HTAF2T}, {HTAF3T}, {HTAF4T},        N'{_hes_}',    N'{_SHARH_}', {_BED_}, {HFRST[ROW].NUMBER}, 4)");
                    }
                }

                if (JAMCH != 0d) // چكهاي پرداختي
                {
                    //CHRST.Open("PAY_GETP", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                    var CHRST = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP WHERE NUMBER = " + HFRST[ROW].NUMBER + " AND TAG = 4").ToList();

                    if (CHRST.Count > 0 && !IsNull(CHRST.FirstOrDefault().NUMBER))
                    {
                        for (int Q = 0; Q < CHRST.Count; Q++) // while (!CHRST.EOF)
                        {
                            MABL_CHK = (double)(MABL_CHK + CHRST[Q].MABL);
                            //SDRST.AddNew(); // اسناد پرداختني
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = GETKOL(Baseknow.APA);
                            //SDRST.Fields("HES_M") = GETMOIN(Baseknow.APA);
                            //SDRST.Fields("HES_T") = GETTAF(Baseknow.APA);
                            var hes_ = Baseknow.APA;
                            var SHARH_ = Strings.Right("چك " + CHRST[Q].N_SERI + "بانك " + GETBANK((double)CHRST[Q].BANK) + " " + CHRST[Q].SHOBEH + " مورخ " + Strings.Format(CHRST[Q].DATE_S, "####/##/##"), 255);
                            //SDRST.Fields("BES") = CHRST[Q].MABL;
                            //SDRST.Fields("N_SERI") = CHRST[Q].N_SERI;
                            //SDRST.Fields("BANK") = CHRST[Q].BANK;
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;
                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S, HES_K,                        HES_M,                         HES_T,          hes,         SHARH,                 BES,               N_SERI,          BANK,                 NUMBER,  		   TAG)
		                                                        VALUES ({max_ns}, {GETKOL(Baseknow.APA)}, {GETMOIN(Baseknow.APA)},     {GETTAF(Baseknow.APA)},    N'{hes_}',	N'{SHARH_}',	 {CHRST[Q].MABL},	{CHRST[Q].N_SERI}	, {CHRST[Q].BANK}  ,     {HFRST[ROW].NUMBER}  ,4)");
                            //SDRST.update();

                            //SDRST.AddNew(); // چكهاي پرداختي
                            //SDRST.Fields("N_S") = max_ns;
                            //SDRST.Fields("HES_K") = CKOL;
                            //SDRST.Fields("HES_M") = CMOIN;
                            //SDRST.Fields("HES_T") = CTAF;
                            //SDRST.Fields("HES_T2") = CTAF2;
                            //SDRST.Fields("HES_T3") = CTAF3;
                            //SDRST.Fields("HES_T4") = CTAF4;
                            //SDRST.Fields("hes") = HFRST[ROW].CUST_NO;
                            var _SHARH_ = Strings.Right("ف.ب.ف." + HFRST[ROW].NUMBER + " - " + "چك " + CHRST[Q].N_SERI + "بانك " + GETBANK((double)CHRST[Q].BANK) + " " + CHRST[Q].SHOBEH + " مورخ " + Strings.Format(CHRST[Q].DATE_S, "####/##/##"), 255);
                            //SDRST.Fields("BED") = CHRST[Q].MABL;
                            //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                            //SDRST.Fields("TAG") = 4;
                            //SDRST.update(); //#ERROR

                            string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                            string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                            string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S, HES_K,   HES_M,    HES_T, HES_T2,  HES_T3,   HES_T4,       hes,                   SHARH,                 BED,            NUMBER,  		   TAG)
		                                                        VALUES ({max_ns},      {CKOL}, {CMOIN},  {CTAF}, {CTAF2T},{CTAF3T},{CTAF4T}, N'{HFRST[ROW].CUST_NO}',	N'{SHARH_}',	 {CHRST[Q].MABL},   {HFRST[ROW].NUMBER}  ,4)");

                        }
                    }
                    else
                    {
                    }
                }
                if (HFRST[ROW].TAKHFIF != 0)
                {
                    var rstopen = dbms.DoGetDataSQL<QUERY_MODEL1>("SELECT     SUM(dbo.INVO_LST.n_moin / dbo.INVO_LST.meghk * dbo.INVO_LST.MEGH_MAR) AS JAMT, dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND FROM dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE (dbo.INVO_LST.NUMBER = " + HFRST[ROW].NUMBER1 + " ) And (dbo.INVO_LST.TAG = 2) GROUP BY dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND").ToList();
                    if (rstopen.Count > 0)
                    {
                        TAKHF = 0d;
                        for (int F = 0; F < rstopen.Count; F++) // while (!rstopen.EOF())
                        {
                            if (Math.Round((double)rstopen[F].JAMT) != 0)
                            {
                                if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                                {
                                    if (isDefaccChecked && !ISHESAB(Baseknow.TFROSH, 3, Convert.ToInt64(rstopen[F].CODE)))
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.TFROSH, 3, Convert.ToInt64(rstopen[F].CODE), "تخفيف " + GETKALANAME(Convert.ToDouble(rstopen[F].CODE)));

                                        }
                                        catch (Exception ex)
                                        {
                                            ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                                            //DoCmd.OpenForm("MESAG", default, default, default, default, acDialog, "اخطار مهم ...! حساب متناظر كالا در تخفيفات فروش وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                        }
                                    }
                                    //SDRST.AddNew(); // تخفيف فروش
                                    //SDRST.Fields("N_S") = max_ns;
                                    //SDRST.Fields("HES_K") = Baseknow.TFROSH;
                                    //SDRST.Fields("HES_M") = 3;
                                    //SDRST.Fields("HES_T") = rstopen[F].CODE;
                                    var SHARH_ = Strings.Right("مبلغ برگشت تخفيف فروش فاكتور  شماره " + HFRST[ROW].NUMBER1 + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 255);
                                    var hes_ = Baseknow.TFROSH + "-3-" + Convert.ToDouble(rstopen[F].CODE);
                                    var BES_ = Math.Round((double)rstopen[F].JAMT);
                                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                                    //SDRST.Fields("TAG") = 4;
                                    TAKHF = TAKHF + Math.Round((double)rstopen[F].JAMT);

                                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K, HES_M,           HES_T,          hes,         SHARH,         BES,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.TFROSH}, 3,     {rstopen[F].CODE},     N'{hes_}', N'{SHARH_}',	  {BES_},	 {HFRST[ROW].NUMBER}  ,4)");
                                    //SDRST.update();
                                }
                                else
                                {
                                    if (isDefaccChecked && !ISHESAB(Baseknow.TFROSH, rstopen[F].CUST_KIND, Convert.ToInt64(rstopen[F].CODE)))
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.TFROSH, rstopen[F].CUST_KIND, Convert.ToInt64(rstopen[F].CODE), "تخفيف " + GETKALANAME(Convert.ToDouble(rstopen[F].CODE)));
                                        }
                                        catch (Exception ex)
                                        {
                                            ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب");
                                            //DoCmd.OpenForm("MESAG", default, default, default, default, acDialog, "اخطار مهم ...! حساب متناظر تخفيفات كالا  وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                        }
                                    }
                                    //SDRST.AddNew(); // تخفيف فروش
                                    //SDRST.Fields("N_S") = max_ns;
                                    //SDRST.Fields("HES_K") = Baseknow.TFROSH;
                                    //SDRST.Fields("HES_M") = rstopen[F].CUST_KIND;
                                    //SDRST.Fields("HES_T") = rstopen[F].CODE;
                                    var _SHARH_ = Strings.Right("مبلغ برگشت تخفيف فروش فاكتور  شماره " + HFRST[ROW].NUMBER1 + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 255);
                                    var _hes_ = Baseknow.TFROSH + "-" + rstopen[F].CUST_KIND + "-" + Convert.ToDouble(rstopen[F].CODE);
                                    var _BES_ = Math.Round((double)rstopen[F].JAMT);
                                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                                    //SDRST.Fields("TAG") = 4;
                                    TAKHF = TAKHF + Math.Round((double)rstopen[F].JAMT);

                                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,                   HES_M,           HES_T,              hes,         SHARH,          BES,           NUMBER,  	 TAG)
		                                                        VALUES ({max_ns},  			{Baseknow.TFROSH}, {rstopen[F].CUST_KIND},     {rstopen[F].CODE},     N'{_hes_}', N'{_SHARH_}',	  {_BES_},	 {HFRST[ROW].NUMBER}  ,4)");
                                    //SDRST.update();
                                }
                            }
                            //rstopen.MoveNext();
                        }
                    }
                    if (HFRST[ROW].TAKHFIF != TAKHF)
                    {
                        HFRST[ROW].TAKHFIF = TAKHF;
                    }
                }
                if (HFRST[ROW].MBAA != 0)
                {
                    //SDRST.AddNew(); // مالليات بر ارزش افزوده
                    //SDRST.Fields("N_S") = max_ns;
                    if (!IsNull(HFRST[ROW].MBAA))
                    {
                        GETTAF3(HFRST[ROW].HMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }
                    //SDRST.Fields("HES_K") = HKOL;
                    //SDRST.Fields("HES_M") = HMOIN;
                    //SDRST.Fields("HES_T") = HTAF;
                    //SDRST.Fields("HES_T2") = HTAF2;
                    //SDRST.Fields("HES_T3") = HTAF3;
                    //SDRST.Fields("HES_T4") = HTAF4;
                    var _hes_ = HFRST[ROW].HMBAA;
                    var _SHARH_ = Strings.Right(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور برگشت فروش شماره " + HFRST[ROW].NUMBER1 + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 255);
                    //SDRST.Fields("BED") = HFRST[ROW].MBAA;
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;

                    string HTAF2T = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                    string HTAF3T = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                    string HTAF4T = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,   HES_T, HES_T2,  HES_T3,  HES_T4,      hes,          SHARH,     BED,                     NUMBER, TAG)
		                                                        VALUES ({max_ns}, {HKOL}, {HMOIN}, {HTAF},{HTAF2T},{HTAF3T},{HTAF4T}, N'{_hes_}', N'{_SHARH_}', {HFRST[ROW].MBAA}, {HFRST[ROW].NUMBER} ,4)");
                    //SDRST.update();
                }
                if (JAMF + HFRST[ROW].MABL_HAZ - HFRST[ROW].TAKHFIF > 0)
                {
                    //SDRST.AddNew(); // كل بستانكاري شخص بابت فاكتور
                    //SDRST.Fields("N_S") = max_ns;
                    //SDRST.Fields("HES_K") = CKOL;
                    //SDRST.Fields("HES_M") = CMOIN;
                    //SDRST.Fields("HES_t") = CTAF;
                    //SDRST.Fields("HES_T2") = CTAF2;
                    //SDRST.Fields("HES_T3") = CTAF3;
                    //SDRST.Fields("HES_T4") = CTAF4;
                    //SDRST.Fields("hes") = HFRST[ROW].CUST_NO;
                    var _SHARH_ = Strings.Right("فاكتور برگشت فروش  شماره " + HFRST[ROW].NUMBER + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + HFRST[ROW].MOLAH, 255);
                    var _BES_ = JAMF + HFRST[ROW].MABL_HAZ - HFRST[ROW].TAKHFIF + HFRST[ROW].MBAA;
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;
                    //SDRST.Fields("RADIF") = HFRST[ROW].NUMBER;
                    //SDRST.update();

                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,   HES_T, HES_T2,  HES_T3,  HES_T4,      hes,                    SHARH,      BES,                   NUMBER, TAG,      RADIF)
		                                                        VALUES ({max_ns}, {CKOL}, {CMOIN}, {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HFRST[ROW].CUST_NO}', N'{_SHARH_}', {_BES_}, {HFRST[ROW].NUMBER} ,4,{HFRST[ROW].NUMBER})");

                }
                // If HFRST[ROW].("MABL_HAZ") <> 0 Then
                // SDRST.AddNew 'كل بستانكاري شخص بابت خدمات  فاكتور
                // SDRST.Fields("N_S") = MAX_NS
                // SDRST.Fields("HES_K") = CKOL
                // SDRST.Fields("HES_M") = CMOIN
                // SDRST.Fields("HES_t") = CTAF
                // SDRST.Fields("SHARH") = Right("خدمات فاكتور برگشت فروش  شماره "  &&  HFRST[ROW].("NUMBER")  &&  " مورخ"  &&  Format(HFRST[ROW].("DATE_N"), "####/##/##"),255)
                // SDRST.Fields("BES") = HFRST[ROW].("MABL_HAZ")
                // SDRST.Fields("NUMBER") = HFRST[ROW].("NUMBER")
                // SDRST.Fields("TAG") = 4
                // SDRST.update
                // End If
                if (HFRST[ROW].M_NAGHD != 0)
                {
                    //SDRST.AddNew(); // مبلغ نقدشخص
                    //SDRST.Fields("N_S") = max_ns;
                    //SDRST.Fields("HES_K") = CKOL;
                    //SDRST.Fields("HES_M") = CMOIN;
                    //SDRST.Fields("HES_t") = CTAF;
                    //SDRST.Fields("HES_T2") = CTAF2;
                    //SDRST.Fields("HES_T3") = CTAF3;
                    //SDRST.Fields("HES_T4") = CTAF4;
                    //SDRST.Fields("hes") = HFRST[ROW].CUST_NO;
                    var _SHARH_ = Strings.Right("مبلغ نقد فاكتور برگشت فروش  شماره " + HFRST[ROW].NUMBER + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 255);
                    var _BED_ = HFRST[ROW].M_NAGHD;
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;

                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,   HES_T, HES_T2,  HES_T3,  HES_T4,      hes,                    SHARH,        BED,           NUMBER, TAG)
		                                                        VALUES ({max_ns}, {CKOL}, {CMOIN}, {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HFRST[ROW].CUST_NO}', N'{_SHARH_}', {_BED_}, {HFRST[ROW].NUMBER} ,4)");
                    //SDRST.update();
                }
                if (HFRST[ROW].M_NAGHD != 0)
                {
                    //SDRST.AddNew(); // مبلغ نقدصندوق
                    //SDRST.Fields("N_S") = max_ns;
                    //SDRST.Fields("HES_K") = Baseknow.SANDOGH;
                    //SDRST.Fields("HES_M") = HFRST[ROW].DEPATMAN;
                    //SDRST.Fields("HES_T") = HFRST[ROW].SHIFT;
                    var _hes_ = Baseknow.SANDOGH + "-" + HFRST[ROW].DEPATMAN + "-" + HFRST[ROW].SHIFT;
                    var _SHARH_ = Strings.Right("مبلغ نقد فاكتور برگشت فروش  شماره " + HFRST[ROW].NUMBER + " مورخ" + Strings.Format(HFRST[ROW].DATE_N, "####/##/##"), 255);
                    //SDRST.Fields("BES") = HFRST[ROW].M_NAGHD;
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;

                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,          HES_K,              HES_M,                  HES_T,          hes,        SHARH,           BES,                   NUMBER,     TAG)
		                                                        VALUES ({max_ns}, {Baseknow.SANDOGH}, {HFRST[ROW].DEPATMAN}, {HFRST[ROW].SHIFT}, N'{_hes_}', N'{_SHARH_}', {HFRST[ROW].M_NAGHD}, {HFRST[ROW].NUMBER} ,4)");
                    //SDRST.update();
                }
                JAMP = 0d;
                var PRST = dbms.DoGetDataSQL<VISITOR_DTL>("SELECT     dbo.VISITOR_DTL.* FROM dbo.VISITOR_DTL WHERE     (NUMBER = " + HFRST[ROW].NUMBER + ") AND (TAG = 4) ").ToList();
                var visitorn = "";
                for (int S = 0; S < PRST.Count; S++) //while (!PRST.EOF)
                {
                    visitorn = GETTAFNAME(PRST[S].CUST_NO);
                    //SDRST.AddNew(); // پورسانت
                    //var _N_S_ = max_ns;
                    var _HES_K_ = GETKOL(PRST[S].CUST_NO);
                    var _HES_M_ = GETMOIN(PRST[S].CUST_NO);
                    var _HES_T_ = GETTAF(PRST[S].CUST_NO);

                    double? PHKOL = null;
                    double? PHMOIN = null;
                    double? PHTAF = null;
                    double? PHTAF2 = null;
                    double? PHTAF3 = null;
                    double? PHTAF4 = null;

                    GETTAF3(PRST[S].CUST_NO, ref PHKOL, ref PHMOIN, ref PHTAF, ref PHTAF2, ref PHTAF3, ref PHTAF4);

                    string _PHTAF2_ = (Convert.ToDouble(PHTAF2) == 0) ? "NULL" : PHTAF2.ToString();
                    string _PHTAF3_ = (Convert.ToDouble(PHTAF3) == 0) ? "NULL" : PHTAF3.ToString();
                    string _PHTAF4_ = (Convert.ToDouble(PHTAF4) == 0) ? "NULL" : PHTAF4.ToString();


                    var _hes_ = PRST[S].CUST_NO;
                    var _SHARH_ = Strings.Right(" فاكتور برگشت فروش شماره " + HFRST[ROW].NUMBER + " بابت " + PRST[S].DARSAD + "درصد سهم پورسانت " + GETTAFNAME(PRST[S].CUST_NO) + " مورخ " + Strings.Format(HFRST[ROW].DATE_N, "####/##/##") + Interaction.IIf(IsNull(PRST[S].TOZIH), "", PRST[S].TOZIH), 255);
                    if ((bool)!PRST[S].STAT)
                    {
                        var _iif = Convert.ToDouble(Interaction.IIf(SafeToDouble(Strings.Mid(Baseknow.OPTIONSS, 62, 1)) == 5d, HFRST[ROW].MBAA, 0));
                        if (Math.Round((double)((JAMF - HFRST[ROW].TAKHFIF + _iif) * PRST[S].DARSAD / 100)) != PRST[S].PURSANT)
                        {
                            var _exprif = Convert.ToDouble(Interaction.IIf(SafeToDouble(Strings.Mid(Baseknow.OPTIONSS, 62, 1)) == 5d, HFRST[ROW].MBAA, 0));
                            PRST[S].PURSANT = Math.Round((double)((JAMF - HFRST[ROW].TAKHFIF + _exprif) * PRST[S].DARSAD / 100));

                            string _where = " WHERE     (NUMBER = " + HFRST[ROW].NUMBER + $") AND (TAG = 4) AND CUST_NO = N'{HFRST[ROW].CUST_NO}' ";
                            dbms.DoExecuteSQL($@"UPDATE dbo.VISITOR_DTL SET PURSANT = {PRST[S].PURSANT}  {_where} "); //#ERROR

                            //PRST.update();
                        }
                    }
                    else if (PRST[S].DARSAD != PRST[S].PURSANT / (JAMF - HFRST[ROW].TAKHFIF + (Convert.ToDouble(Interaction.IIf(Strings.Mid(Baseknow.OPTIONSS, 61, 1) == "5", HFRST[ROW].MBAA, 0)))) * 100)
                    {
                        PRST[S].DARSAD = PRST[S].PURSANT / (JAMF - HFRST[ROW].TAKHFIF + (Convert.ToDouble(Interaction.IIf(Strings.Mid(Baseknow.OPTIONSS, 61, 1) == "5", HFRST[ROW].MBAA, 0)))) * 100;
                        string _where = " WHERE     (NUMBER = " + HFRST[ROW].NUMBER + $") AND (TAG = 4) AND CUST_NO = N'{HFRST[ROW].CUST_NO}'";
                        dbms.DoExecuteSQL($"UPDATE  dbo.VISITOR_DTL SET DARSAD = {PRST[S].DARSAD}  {_where} ");
                        //PRST.update();
                    }

                    //SDRST.Fields("BEd") = PRST[S].PURSANT;
                    JAMP = (double)(JAMP + PRST[S].PURSANT);
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;
                    if (PRST[S].PURSANT != 0)
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S,             HES_K,   HES_M,      HES_T,    HES_T2,    HES_T3,   HES_T4,        hes,        SHARH,        BED,           NUMBER,           TAG)
		                                                        VALUES ({max_ns},    {_HES_K_}, {_HES_M_}, {_HES_T_},{_PHTAF2_},{_PHTAF3_},{_PHTAF4_}, N'{_hes_}', N'{_SHARH_}', {PRST[S].PURSANT}, {HFRST[ROW].NUMBER} ,4)");
                        //SDRST.update();
                    }
                    //PRST.MoveNext();
                }
                if (JAMP > 0d)
                {
                    //SDRST.AddNew(); // پورسانت
                    //SDRST.Fields("N_S") = max_ns;
                    //SDRST.Fields("HES_K") = GETKOL(Baseknow.HPOR);
                    //SDRST.Fields("HES_M") = GETMOIN(Baseknow.HPOR);
                    //SDRST.Fields("HES_T") = GETTAF(Baseknow.HPOR);
                    var _hes = Baseknow.HPOR;
                    var SHARH_ = Strings.Left("بابت درصد سهم  فاكتور برگشت فروش شماره " + HFRST[ROW].NUMBER + " " + visitorn, 255);
                    //SDRST.Fields("BES") = JAMP;
                    //SDRST.Fields("NUMBER") = HFRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 4;
                    //SDRST.update();
                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,         HES_K,                       HES_M,              HES_T,                         hes,         SHARH,          BES,           NUMBER,  		 TAG)
		                                                        VALUES ({max_ns}, {GETKOL(Baseknow.HPOR)}, {GETMOIN(Baseknow.HPOR)}, {GETTAF(Baseknow.HPOR)},      N'{_hes}',	  N'{SHARH_}',	   {JAMP},		 {HFRST[ROW].NUMBER}  ,4)");
                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                //rst.Close();
                //HFRST.MoveNext();
                //if (i % CON == 0L)
                //{
                //    Forms["GUG"]["Text0"] = Forms["GUG"]["Text0"] + "n";
                //    Forms["GUG"].Form.Refresh();
                //    Forms["GUG"]["Text0"].Requery();
                //    Forms["GUG"].Form.Repaint();
                //}
                //i = i + 1L;
                //Forms["GUG"]["Text2"] = "";
                //});

                //} ////For loop normal
            }); // ExecuteWithPreferredLoop
            LogWriter.WriteLog("پایان فاکتور برگشت فروش" + DateTime.Now.ToString());
            //DoCmd.Close(acForm, "GUG");
            // DoCmd.Close acForm, "GENSANADFROOSH"
            if (InternalCalling)
            {
                gensanadbargashfroosh2(fnum, TNUM);
            }
        }

        public static void gensanadbargashfroosh2(long fnum, long TNUM, bool InternalCalling = true)
        {
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }
            bool isDefaccChecked = Generaly.defacc;
            //long CON, i;
            //object a = default, fs;
            //double? max_ns, MABL_CHK = 0, JAMF, JAMCH, CKOL = null, JAMFKH;
            //double MBL;
            //double? CMOIN = null, CTAF = null, takh;
            //string shart;
            //int ii;
            //string CH;
            //double JAMP;
            //string TAMIR;
            //string per;
            //long permab;
            //double TAKHF;
            //double? CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, MAVAD;
            //double DAST;
            //double SAR;
            //double HES_M;
            //double HES_T;
            //string HES;
            //string visitorn = "";

            double? tindataFlag = null;
            if (!string.IsNullOrEmpty(Baseknow.tindata))
            {
                var tindataChar = Strings.Mid(Baseknow.tindata, 9, 1);
                if (double.TryParse(tindataChar, out var parsedFlag))
                {
                    tindataFlag = parsedFlag;
                }
            }

            var HFRST = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM dbo.HEAD_LST WHERE (NUMBER BETWEEN {fnum} AND {TNUM}) AND (TAG = 25) ORDER BY NUMBER").ToList();
            if (HFRST.Count == 0) { return; }

            LogWriter.WriteLog("شروع باز سازي از برگشت فروش 2 شماره : " + fnum + " تا فاكتور شماره :" + TNUM + DateTime.Now);

            // ───────────────────────────────────────────────────────────────────────────────
            // ⚠️ چرخه‌ی عمر کش: این تابع از فرم‌های برنامه هم با InternalCalling = false صدا زده
            // می‌شود (HEAD_LST_BRFR و HEAD_LST_RASID_OTHER_WIN) و چون کلاس static است، روشن
            // گذاشتن LookupCacheEnabled بدون خاموش کردنش یعنی بهای تمام‌شده‌ی استاندارد و نام
            // کالا/حساب تا پایان عمر برنامه «کهنه» می‌مانند. پس کش فقط وقتی اینجا روشن/پاک
            // می‌شود که صاحبش همین فراخوانی باشد، و در finally به حالت اول برمی‌گردد.
            // ───────────────────────────────────────────────────────────────────────────────
            bool cacheOwnedHere = !LookupCacheEnabled;
            if (cacheOwnedHere)
            {
                ClearLookupCaches();
                LookupCacheEnabled = true;
            }

            try
            {
                // ───────────────────────────────────────────────────────────────────────────
                // مرحله ۱ و ۲ (سریال، چند کوئری): تعیین شماره سند همه‌ی فاکتورها پیش از حلقه.
                //
                // قبلاً این کار داخل حلقه و به‌ازای هر فاکتور با Createsanad انجام می‌شد؛
                // Createsanad یک تراکنش Serializable روی DEED_HED باز می‌کند و با
                // «UPDATE TOP(1) DEED_HED SET ANBAR = ANBAR» روی یک ردیف قفل انحصاری می‌گیرد،
                // پس همه‌ی Threadها پشت همان یک قفل صف می‌کشیدند و حلقه عملاً سریال بود.
                //
                // ⚠️ اینجا نباید شمارنده‌ی دستی از روی MAX(N_S) ساخت: بیشینه‌ی N_S «بین سندهای
                //    NO_S = 4» بیشینه‌ی کل جدول نیست و شماره‌های تازه روی سندهای موجودِ نوع‌های
                //    دیگر می‌افتند. ReserveSanadNumbersBatch همان قفل‌های Createsanad را
                //    می‌گیرد، ولی یک بار برای کل دسته.
                //
                // هر دو حالت «سند روزانه» (SNDKH = true) و «تک‌سندی» عیناً حفظ شده‌اند.
                // ───────────────────────────────────────────────────────────────────────────
                static string BuildBargashtSharhS(HEAD_LST row)
                    => Strings.Right("فاكتور برگشت فروش شماره " + row.NUMBER + " مورخ " + Strings.Format(row.DATE_N, "####/##/##"), 100);

                var isDailyMode = (bool)Baseknow.SNDKH;
                var headerUpdates = new List<string>();

                if (isDailyMode)
                {
                    var dailyNsByDate = new Dictionary<long, double>();
                    var dates = HFRST.Select(x => x.DATE_N).Distinct().ToList();

                    var minDate = dates.Min();
                    var maxDate = dates.Max();
                    foreach (var r in dbms.DoGetDataSQL<QRE10>(
                        $"SELECT BASE, n_s, date_s, no_s FROM dbo.deed_hed WHERE no_s = 4 AND DATE_S BETWEEN {minDate} AND {maxDate}"))
                    {
                        if (r?.DATE_S != null && r.N_S != null && !dailyNsByDate.ContainsKey(r.DATE_S.Value))
                        {
                            dailyNsByDate[r.DATE_S.Value] = r.N_S.Value;
                        }
                    }

                    var missingDates = dates.Where(d => !dailyNsByDate.ContainsKey(d)).ToList();
                    if (missingDates.Count > 0)
                    {
                        var headerRequests = missingDates.Select(d =>
                        {
                            var sample = HFRST.First(x => x.DATE_N == d);
                            return new SanadHeaderRequest
                            {
                                DATE_S = d,
                                SHARH_S = BuildBargashtSharhS(sample),
                                GHATEI = 0,
                                NO_S = 4,
                                OKF = -1,
                                USER_NAME = sample.USER_NAME
                            };
                        }).ToList();

                        var reserved = ReserveSanadNumbersBatch(headerRequests);
                        for (int k = 0; k < missingDates.Count; k++)
                        {
                            dailyNsByDate[missingDates[k]] = reserved[k];
                        }
                    }

                    foreach (var row in HFRST)
                    {
                        var ns = dailyNsByDate[row.DATE_N];
                        if (row.N_S != ns)
                        {
                            row.N_S = ns;
                            headerUpdates.Add($"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(ns)} WHERE NUMBER = {SqlNum(row.NUMBER)} AND TAG = 25;");
                        }
                    }
                }
                else
                {
                    var existingHeaders = new Dictionary<double, long>();
                    var candidates = HFRST.Where(x => x.N_S != null && x.N_S.Value != 0).Select(x => x.N_S.Value).Distinct().ToList();
                    if (candidates.Count > 0)
                    {
                        foreach (var r in dbms.DoGetDataSQL<QRE10>(
                            $"SELECT BASE, n_s, date_s, no_s FROM dbo.deed_hed WHERE no_s = 4 AND N_S BETWEEN {SqlNum(candidates.Min())} AND {SqlNum(candidates.Max())}"))
                        {
                            if (r?.N_S != null) { existingHeaders[r.N_S.Value] = r.DATE_S ?? 0L; }
                        }
                    }

                    // هر شماره سند فقط یک مالک دارد؛ وگرنه دو Thread موازی ردیف‌های یکدیگر را پاک می‌کردند.
                    var claimed = new HashSet<double>();
                    var newHeaderIndexes = new List<int>();

                    for (int i = 0; i < HFRST.Count; i++)
                    {
                        var ns = HFRST[i].N_S;
                        var exists = ns != null && ns.Value != 0 && existingHeaders.ContainsKey(ns.Value);
                        var owns = exists && claimed.Add(ns.Value);

                        if (!owns)
                        {
                            newHeaderIndexes.Add(i);
                        }
                        else if (existingHeaders[ns.Value] != HFRST[i].DATE_N)
                        {
                            headerUpdates.Add(
                                $"UPDATE dbo.DEED_HED SET DATE_S = {SqlNum(HFRST[i].DATE_N)}, SHARH_S = N'{SqlText(BuildBargashtSharhS(HFRST[i]))}', " +
                                $"GHATEI = 0, NO_S = 4, OKF = -1, USER_NAME = N'{SqlText(HFRST[i].USER_NAME)}' WHERE N_S = {SqlNum(ns.Value)};");
                        }
                    }

                    if (newHeaderIndexes.Count > 0)
                    {
                        var headerRequests = newHeaderIndexes.Select(i => new SanadHeaderRequest
                        {
                            DATE_S = HFRST[i].DATE_N,
                            SHARH_S = BuildBargashtSharhS(HFRST[i]),
                            GHATEI = 0,
                            NO_S = 4,
                            OKF = -1,
                            USER_NAME = HFRST[i].USER_NAME
                        }).ToList();

                        var reserved = ReserveSanadNumbersBatch(headerRequests);
                        for (int k = 0; k < newHeaderIndexes.Count; k++)
                        {
                            var idx = newHeaderIndexes[k];
                            HFRST[idx].N_S = reserved[k];
                            headerUpdates.Add($"UPDATE dbo.HEAD_LST SET N_S = {SqlNum(reserved[k])} WHERE NUMBER = {SqlNum(HFRST[idx].NUMBER)} AND TAG = 25;");
                        }
                    }
                }

                const int headUpdateChunkSize = 500;
                for (int offset = 0; offset < headerUpdates.Count; offset += headUpdateChunkSize)
                {
                    var b = new StringBuilder();
                    b.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                    foreach (var stmt in headerUpdates.Skip(offset).Take(headUpdateChunkSize)) { b.Append(stmt); }
                    b.Append("COMMIT TRANSACTION;");
                    dbms.DoExecuteSQL(b.ToString());
                }

                // ───────────────────────────────────────────────────────────────────────────
                // مرحله ۳: پیش‌گرم‌کردن کش حساب‌های موجود و پیش‌ساخت دسته‌ای حساب‌های لازم،
                // تا CREATHES داخل حلقه‌ی موازی بدون رفت‌وبرگشت برگردد.
                // ───────────────────────────────────────────────────────────────────────────
                foreach (var acc in dbms.DoGetDataSQL<QRE13>("SELECT N_KOL, NUMBER, TNUMBER FROM dbo.TDETA_HES"))
                {
                    MarkAccountExists(acc.N_KOL ?? 0, acc.NUMBER ?? 0, acc.TNUMBER ?? 0);
                }

                if (isDefaccChecked)
                {
                    var jstSecList = dbms.DoGetDataSQL<QRE12_WITH_NUM>(
                        $"SELECT dbo.INVO_LST.NUMBER, dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.CODE, dbo.INVO_LST.ANBAR, dbo.STUF_DEF.NAME, dbo.INVO_LST.AVRAGE " +
                        "FROM dbo.STUF_DEF INNER JOIN dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE " +
                        $"WHERE dbo.INVO_LST.TAG = 24 AND dbo.INVO_LST.NUMBER BETWEEN {fnum} AND {TNUM}")
                        .GroupBy(x => Convert.ToInt64(x.NUMBER)).ToDictionary(g => g.Key, g => g.ToList());

                    var accountsToEnsure = new HashSet<(double Kol, double Moin, double Taf, string Name)>();
                    bool isOption13_5 = Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5";

                    foreach (var row in HFRST)
                    {
                        if (!jstSecList.TryGetValue(Convert.ToInt64(row.NUMBER), out var items)) { continue; }

                        foreach (var item in items)
                        {
                            // کد غیرعددی اینجا رد می‌شود؛ حلقه‌ی اصلی خودش همان قلم را با لاگ رد می‌کند.
                            if (!TryGetAccountCode(item.CODE, out var codeL)) { continue; }
                            double codeD = Convert.ToDouble(codeL);
                            var itemName = string.IsNullOrEmpty(item.NAME) ? " " : item.NAME;

                            if (isOption13_5)
                            {
                                accountsToEnsure.Add((Baseknow.MFROSH ?? 0d, 4, codeL, itemName));
                            }
                            else if (item.ANBAR != 0)
                            {
                                accountsToEnsure.Add((Baseknow.MFROSH ?? 0d, codeL, codeL, itemName));
                            }
                            else
                            {
                                accountsToEnsure.Add((Baseknow.DARAM ?? 0d, row.DEPATMAN ?? 0, codeL, itemName));
                            }

                            accountsToEnsure.Add((Baseknow.MOGODIA ?? 0d, item.ANBAR ?? 0, codeL, itemName));

                            if (tindataFlag is null || tindataFlag != 1d)
                            {
                                accountsToEnsure.Add((Baseknow.GHEYMAT ?? 0d, codeL, codeL, itemName));
                            }

                            var gheymatMoin = (tindataFlag is null || tindataFlag == 1d) ? 1d : codeD;
                            accountsToEnsure.Add((Baseknow.GHEYMAT ?? 0d, gheymatMoin, (tindataFlag is null || tindataFlag == 1d) ? 1d : codeD, "مواد " + itemName));
                            accountsToEnsure.Add((Baseknow.GHEYMAT ?? 0d, gheymatMoin, 9999999, "دستمزد " + itemName));
                            accountsToEnsure.Add((Baseknow.GHEYMAT ?? 0d, gheymatMoin, 9999998, "سربار " + itemName));
                        }
                    }

                    EnsureAccountsInParallel(accountsToEnsure, "gensanadbargashfroosh2");
                }

                var progressReporter = new ThrottledProgressReporter(
                    HFRST.Count,
                    InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                    value =>
                    {
                        auto_run.PRGR_C8.Value = Math.Max(auto_run.PRGR_C8.Value, value);
                        auto_run.UpdateOverallProgressBar();
                    });

                //اینجا قبلا For بوده حالا شده Parallel یعنی برگشت آزاد

                var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);
                ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, HFRST_EOF =>
            //for (int HFRST_EOF = 0; HFRST_EOF < HFRST.Count; HFRST_EOF++) //while (!HFRST.EOF) ////Normal loop for i
            {
                object a = default, fs = null;
                double? max_ns = null, MABL_CHK = 0, JAMF = 0, JAMCH = 0, CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, JAMFKH;
                double MBL = 0d;
                string CH = string.Empty;
                double JAMP = 0d;
                string TAMIR = string.Empty;
                string per = string.Empty;
                long permab = 0;
                double TAKHF = 0d;
                double MAVAD = 0d;
                double DAST = 0d;
                double SAR = 0d;
                double HES_M = 0d;
                double HES_T = 0d;
                string HES = string.Empty;
                string visitorn = "";

                // گزارش پیشرفت غیرمسدودکننده: Dispatcher.Invoke مسدودکننده بود و همه‌ی
                // Threadها را پشت تک‌Thread رابط کاربری صف می‌کرد، یعنی موازی‌سازی عملاً بی‌اثر می‌شد.
                progressReporter.ReportOne();

                if (!IsNull(HFRST[HFRST_EOF]?.CUST_NO))
                {
                    GETTAF3(HFRST[HFRST_EOF].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }

                // شماره سند در مرحله ۱ و ۲ (پیش از حلقه و به‌صورت دسته‌ای) تعیین شده است؛
                // اینجا فقط خوانده می‌شود. کد قبلی برای هر فاکتور Createsanad صدا می‌زد که
                // کل جدول DEED_HED را با Serializable قفل می‌کرد و حلقه را سریال می‌کرد.
                max_ns = HFRST[HFRST_EOF].N_S;


                var JST_0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE ((INVO_LST.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + ") AND (TAG = 24) AND (ANBAR <> 0)").ToList();
                if (JST_0.Count > 0 && !IsNull(JST_0.FirstOrDefault()))
                {
                    JAMF = (double)JST_0.FirstOrDefault();
                }
                else
                {
                    JAMF = 0d;
                }
                ;
                var JST_1 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE ((INVO_LST.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + ") AND (TAG = 24) AND (ANBAR = 0)").ToList();
                if (JST_1.Count > 0 && !IsNull(JST_1.FirstOrDefault()))
                {
                    JAMFKH = (double)JST_1.FirstOrDefault();
                }
                else
                {
                    JAMFKH = 0d;
                }
                ;
                var JST_2 = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETP.MABL) AS SumOfMABL FROM PAY_GETP WHERE (((PAY_GETP.TAG)=24) AND ((PAY_GETP.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + " ))").ToList();
                if (JST_2.Count > 0 && !IsNull(JST_2.FirstOrDefault()))
                {
                    JAMCH = (double)JST_2.FirstOrDefault();
                }
                else
                {
                    JAMCH = 0d;
                }
                ;
                TAKHF = 0d;
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (DEED_DTL.NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (DEED_DTL.TAG= 25 or DEED_DTL.TAG= 25)");
                //JST.Open("SELECT INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, INVO_LST.avrage FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE (((INVO_LST.NUMBER)=" + HFRST[HFRST_EOF].NUMBER + ") AND ((INVO_LST.TAG)=24)) ", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                var jst_sec = dbms.DoGetDataSQL<QRE12>("SELECT INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, INVO_LST.AVRAGE FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE     (dbo.INVO_LST.NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (dbo.INVO_LST.TAG = 24) ").ToList();
                for (int jst_sec_EOF = 0; jst_sec_EOF < jst_sec.Count; jst_sec_EOF++)
                {
                    long codeAsLong;
                    if (!long.TryParse(jst_sec[jst_sec_EOF].CODE, out codeAsLong))
                    {
                        LogWriter.WriteLog($"Invalid non-numeric product code '{jst_sec[jst_sec_EOF].CODE}' found in sales return invoice number {HFRST[HFRST_EOF].NUMBER}. Skipping this line item.");
                        continue;
                    }

                    if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                    {
                        CREATHES(Baseknow.MFROSH, 4, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.MFROSH},{4},{jst_sec[jst_sec_EOF].CODE}
                                        ,N'{Baseknow.MFROSH + "-4-" + jst_sec[jst_sec_EOF].CODE}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                    }
                    else if (jst_sec[jst_sec_EOF].ANBAR != 0)
                    {
                        CREATHES(Baseknow.MFROSH, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);
                        if (jst_sec[jst_sec_EOF].MABL_K > 0)
                        {

                            //if (Baseknow.tindata == null || Conversions.ToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                            if (tindataFlag is null || tindataFlag == 1d)
                            {
                                HES_M = 1;
                                HES_T = 1;
                                HES = Baseknow.MFROSH + "-1-1";
                            }
                            else
                            {
                                HES_M = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                HES_T = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                HES = Baseknow.MFROSH + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE);
                            }
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.MFROSH},{HES_M},{HES_T}
                                        ,N'{HES}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                        }
                    }
                    else
                    {
                        CREATHES(Baseknow.DARAM, HFRST[HFRST_EOF].DEPATMAN, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.DARAM},{HFRST[HFRST_EOF].DEPATMAN},{Convert.ToInt64(jst_sec[jst_sec_EOF].CODE)}
                                        ,N'{Baseknow.DARAM + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + jst_sec[jst_sec_EOF].CODE}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round((double)jst_sec[jst_sec_EOF].MABL_K)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                    }

                    if (Baseknow.SANAT == true || IsNull(Baseknow.SANAT) || true)
                    {
                        MAVAD = Math.Round((double)(GETSTANDARDPRICE_MAVAD(jst_sec[jst_sec_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_sec[jst_sec_EOF].MEGHk));
                        DAST = Math.Round((double)(GETSTANDARDPRICE_DAST(jst_sec[jst_sec_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_sec[jst_sec_EOF].MEGHk));
                        SAR = Math.Round((double)(GETSTANDARDPRICE_SAR(jst_sec[jst_sec_EOF].CODE, (long)HFRST[HFRST_EOF].DATE_N) * jst_sec[jst_sec_EOF].MEGHk));
                        if (isDefaccChecked)
                        {
                            CREATHES(Baseknow.MOGODIA, jst_sec[jst_sec_EOF].ANBAR, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);
                        }
                        if (MAVAD + DAST + SAR != 0d && Strings.Mid(Baseknow.OPTIONSS, 66, 1) != "5")
                        {
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.MOGODIA},{jst_sec[jst_sec_EOF].ANBAR},{Convert.ToInt64(jst_sec[jst_sec_EOF].CODE)}
                                        ,N'{Baseknow.MOGODIA + "-" + jst_sec[jst_sec_EOF].ANBAR + "-" + jst_sec[jst_sec_EOF].CODE}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{MAVAD + DAST + SAR},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                            //if (Baseknow.tindata == null || Conversions.ToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) != 1d)
                            if (tindataFlag is null || tindataFlag != 1d)
                            {
                                if (isDefaccChecked)
                                {
                                    CREATHES(Baseknow.GHEYMAT, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), jst_sec[jst_sec_EOF].NAME);
                                }
                            }
                            if (MAVAD > 0d)
                            {
                                // if (Baseknow.tindata == null || Conversions.ToDouble(Strings.Mid(Baseknow.tindata, 9, 1)) == 1d)
                                if (tindataFlag is null || tindataFlag == 1d)
                                {
                                    HES_M = 1;
                                    HES_T = 1;
                                    HES = Baseknow.GHEYMAT + "-1-1";
                                }
                                else
                                {
                                    HES_M = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                    HES_T = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                    HES = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE);
                                }

                                // *** FIX: Ensure account exists in TDETA_HES before DEED_DTL insert
                                try
                                {
                                    CREATHES(Baseknow.GHEYMAT, (double)HES_M, (double)HES_T, "مواد " + jst_sec[jst_sec_EOF].NAME);
                                }
                                catch (Exception ex)
                                {
                                    ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب مواد");
                                }

                                // درج سند (همان منطق خودتان)
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL
                                    (N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                    VALUES
                                    ({max_ns},{Baseknow.GHEYMAT},{HES_M},{HES_T}
                                    ,N'{HES}'
                                    ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " +
                                                                     Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" +
                                                                     jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " +
                                                                     Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                    ,{MAVAD}
                                    ,{HFRST[HFRST_EOF].NUMBER}
                                    ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                    ,25)");
                            }
                            if (DAST != 0d)
                            {
                                CREATHES(Baseknow.GHEYMAT, Convert.ToInt64(jst_sec[jst_sec_EOF].CODE), 9999999, "دستمزد " + jst_sec[jst_sec_EOF].NAME);
                                if (tindataFlag is null || tindataFlag == 1d)
                                {
                                    HES_M = 1;
                                    HES_T = 9999999;
                                    HES = Baseknow.GHEYMAT + "-1-9999999";
                                }
                                else
                                {
                                    HES_M = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                    HES_T = 9999999;
                                    HES = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + "-9999999";
                                }
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.GHEYMAT},{HES_M},{HES_T}
                                        ,N'{HES}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{DAST},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");

                            }

                            if (SAR != 0d)
                            {
                                // تعیین HES_M و HES_T و رشته HES مطابق منطق فعلی
                                if (tindataFlag is null || tindataFlag == 1d)
                                {
                                    HES_M = 1;
                                    HES_T = 9999998;
                                    HES = Baseknow.GHEYMAT + "-1-9999998";
                                }
                                else
                                {
                                    HES_M = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                    HES_T = 9999998;
                                    HES = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + "-9999998";
                                }

                                // ساخت حساب دقیقاً با همان (HES_K, HES_M, HES_T) که قرار است در DEED_DTL درج شود
                                try
                                {
                                    CREATHES(Baseknow.GHEYMAT, (long)HES_M, (long)HES_T, "سربار " + jst_sec[jst_sec_EOF].NAME);
                                }
                                catch (Exception ex)
                                {
                                    ExpectionLogWriter.WriteLog(ex, "سند برگشت فروش : ساخت حساب سربار");
                                }

                                // درج سند
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                                     VALUES({max_ns},{Baseknow.GHEYMAT},{HES_M},{HES_T}
                                                 ,N'{HES}'
                                                 ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                                 ,{SAR},
                                                 {HFRST[HFRST_EOF].NUMBER}
                                                 ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                                 ,25)");
                            }

                        }
                        else if (jst_sec[jst_sec_EOF].AVRAGE > 0)
                        {
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.MOGODIA},{jst_sec[jst_sec_EOF].ANBAR},{Convert.ToInt64(jst_sec[jst_sec_EOF].CODE)}
                                        ,N'{Baseknow.MOGODIA + "-" + jst_sec[jst_sec_EOF].ANBAR + "-" + jst_sec[jst_sec_EOF].CODE}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round(jst_sec[jst_sec_EOF].AVRAGE * (double)jst_sec[jst_sec_EOF].MEGHk)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                            if (tindataFlag is null || tindataFlag == 1d)
                            {
                                HES_M = 1;
                                HES_T = 1;
                                HES = Baseknow.GHEYMAT + "-1-1";
                                CREATHES(Baseknow.GHEYMAT, 1, 1, "دستمزد " + jst_sec[jst_sec_EOF].NAME);
                            }
                            else
                            {
                                HES_M = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                HES_T = Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                HES = Baseknow.GHEYMAT + "-" + Convert.ToDouble(jst_sec[jst_sec_EOF].CODE) + '-' + Convert.ToInt64(jst_sec[jst_sec_EOF].CODE);
                                CREATHES(Baseknow.GHEYMAT, HES_M, (long)HES_T, "قیمت تمام شده  " + jst_sec[jst_sec_EOF].NAME);
                            }
                            string test = HFRST[HFRST_EOF].NUMBER.ToString();
                            string test2 = HES.ToString();

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.GHEYMAT},{HES_M},{HES_T}
                                        ,N'{HES}'
                                        ,N'{Strings.Left("برگشت فروش.  فاكتور شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " به مقدار" + jst_sec[jst_sec_EOF].MEGHk + " برگشت فروش. " + Strings.Trim(jst_sec[jst_sec_EOF].NAME), 255)}'
                                        ,{Math.Round(jst_sec[jst_sec_EOF].AVRAGE * (double)jst_sec[jst_sec_EOF].MEGHk)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                        }

                    }
                }
                if (HFRST[HFRST_EOF].MABL_HAZ != 0)
                {
                    if (IsNull(HFRST[HFRST_EOF].MOIN_HAZ))
                    {
                    }
                    // DoCmd.OpenForm "MESAG", , , , , acDialog, "اخطار مهم ...! حساب معين سرويس مشخص نشده است و سند صادره ناقص خواهد بود حتما حساب معين سرويس را مشخص نمائيد."
                    else
                    {
                        if (!IsNull(HFRST[HFRST_EOF].MOIN_HAZ))
                        {
                            GETTAF3(HFRST[HFRST_EOF].MOIN_HAZ, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        }
                        string CTAF2T = (HTAF2 == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                        string CTAF3T = (HTAF3 == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                        string CTAF4T = (HTAF4 == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,SHARH,BED,NUMBER,ARZD,TAG)
                      VALUES({max_ns},{HKOL},{HMOIN},{HTAF},{CTAF2T},{CTAF3T},{CTAF4T}
                                        ,N'{HFRST[HFRST_EOF].MOIN_HAZ}'
                                        ,N'{Strings.Left("فاكتور برگشت فروش.  شماره" + HFRST[HFRST_EOF].NUMBER + GETTAFNAME(HFRST[HFRST_EOF].MOIN_HAZ), 255)}'
                                        ,{HFRST[HFRST_EOF].MABL_HAZ},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");

                    }
                }
                if (JAMCH != 0d) // چكهاي دريافتي
                {
                    var CHRST = dbms.DoGetDataSQL<PAY_GETP_1>($"SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, KIND, VAZ, HES1, HES2, HES3 FROM dbo.PAY_GETP WHERE NUMBER = {HFRST[HFRST_EOF].NUMBER} AND TAG = 24").ToList();
                    if (CHRST.Count > 0 && !IsNull(CHRST.Select(X => X.NUMBER)))
                    {
                        //while (!CHRST.EOF)
                        for (int CHRST_EOF = 0; CHRST_EOF < CHRST.Count; CHRST_EOF++)
                        {
                            object N_S, HES_K, HES_T2, HES_T3, BED, HES_T4, SHARH, BES, N_SERI, BANK, NUMBER, TAG, ARZD = null;

                            MABL_CHK = (double)(MABL_CHK + CHRST[CHRST_EOF].MABL);
                            //SDRST.AddNew(); // اسناد پرداختني
                            N_S = max_ns;
                            HES_K = GETKOL(Baseknow.APA);
                            HES_M = GETMOIN(Baseknow.APA);
                            HES_T = GETTAF(Baseknow.APA);
                            HES = Baseknow.APA;
                            SHARH = Strings.Right("چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK(Convert.ToDouble(CHRST[CHRST_EOF].BANK)) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                            BES = CHRST[CHRST_EOF].MABL;
                            N_SERI = CHRST[CHRST_EOF].N_SERI;
                            BANK = CHRST[CHRST_EOF].BANK;
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 25;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,hes ,SHARH,BES ,N_SERI,BANK,NUMBER,TAG ,ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{HES}',N'{SHARH}',{BES},{N_SERI},{BANK},{NUMBER},{TAG},{ARZD})");
                            N_S = max_ns;
                            HES_K = CKOL;
                            HES_M = (double)CMOIN;
                            HES_T = (double)CTAF;
                            HES_T2 = CTAF2;
                            HES_T3 = CTAF3;
                            HES_T4 = CTAF4;
                            HES = HFRST[HFRST_EOF].CUST_NO;
                            SHARH = Strings.Right("ف.ب.ف." + HFRST[HFRST_EOF].NUMBER1 + " - " + "چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK(Convert.ToDouble(CHRST[CHRST_EOF].BANK)) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                            BED = CHRST[CHRST_EOF].MABL;
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 25;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                            //SDRST.update();
                            string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                            string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                            string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes ,SHARH,BED ,NUMBER,TAG ,ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{HES}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");
                            //CHRST.MoveNext();
                        }
                    }
                }
                if (HFRST[HFRST_EOF].TAKHFIF != 0)
                {
                    var rst = dbms.DoGetDataSQL<QRE_BAZ_17>("SELECT     SUM(dbo.INVO_LST.N_KOL * dbo.INVO_LST.MABL * dbo.INVO_LST.MEGHk / 100) AS JAMT, dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND FROM dbo.INVO_LST INNER JOIN dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG-1 WHERE (dbo.INVO_LST.NUMBER =" + HFRST[HFRST_EOF].NUMBER + ") And (dbo.INVO_LST.TAG = 24) GROUP BY dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND").ToList();
                    if (rst.Count > 0)
                    {
                        TAKHF = 0d;
                        for (int RST_EOF = 0; RST_EOF < rst.Count; RST_EOF++)
                        {
                            if (Strings.Mid(Baseknow.OPTIONSS, 13, 1) == "5")
                            {
                                CREATHES(Baseknow.TFROSH, 3, Convert.ToInt64(rst[RST_EOF].CODE), "تخفيف " + GETKALANAME(Convert.ToInt64(rst[RST_EOF].CODE)));
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.TFROSH},3,{Convert.ToInt64(rst[RST_EOF].CODE)}
                                        ,N'{Baseknow.TFROSH + "-3-" + rst[RST_EOF].CODE}'
                                        ,N'{Strings.Left("مبلغ برگشت تخفيف فروش. فاكتور  شماره  " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255)}'
                                        ,{Math.Round((double)rst[RST_EOF].JAMT)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                                TAKHF = TAKHF + Math.Round((double)rst[RST_EOF].JAMT);
                            }
                            else if (Math.Round((double)rst[RST_EOF].JAMT) != 0)
                            {
                                CREATHES(Baseknow.TFROSH, rst[RST_EOF].CUST_KIND, Convert.ToInt64(rst[RST_EOF].CODE), "تخفيف " + GETKALANAME(Convert.ToInt64(rst[RST_EOF].CODE)));
                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.TFROSH},{rst[RST_EOF].CUST_KIND},{Convert.ToInt64(rst[RST_EOF].CODE)}
                                        ,N'{Baseknow.TFROSH + "-" + rst[RST_EOF].CUST_KIND + "-" + rst[RST_EOF].CODE}'
                                        ,N'{Strings.Left("مبلغ برگشت تخفيف فروش. فاكتور  شماره   " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255)}'
                                        ,{Math.Round((double)rst[RST_EOF].JAMT)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                                TAKHF = TAKHF + Math.Round((double)rst[RST_EOF].JAMT);
                            }
                        }
                    }
                    if (HFRST[HFRST_EOF].TAKHFIF != TAKHF)
                    {
                        HFRST[HFRST_EOF].TAKHFIF = TAKHF;
                        dbms.DoExecuteSQL($"UPDATE  dbo.HEAD_LST SET TAKHFIF = {TAKHF}  WHERE ((HEAD_LST.NUMBER = {HFRST[HFRST_EOF].NUMBER}  AND dbo.HEAD_LST.TAG = 24 ) ) ");

                    }
                }
                if (HFRST[HFRST_EOF].MBAA != 0)
                {
                    // مالليات بر ارزش افزوده
                    if (HFRST[HFRST_EOF].NUMBER == 6)
                    {

                    }
                    var hMbaa = HFRST[HFRST_EOF].HMBAA;
                    if (!IsNull(hMbaa) && !string.IsNullOrWhiteSpace(hMbaa))
                    {
                        GETTAF3(hMbaa, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }
                    else
                    {
                        LogWriter.WriteLog($@"#WARNING  در بازسازی سند برگشت فروش آزاد : برای شماره فاکتور (حواله) {HFRST[HFRST_EOF].NUMBER1} حساب مالیات آن وجود نداشت , بنابر این با حساب پیش فرض مالیات در حسابهای خودگردان سند زدم ");
                        GETTAF3(Baseknow.HESMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        hMbaa = Baseknow.HESMBAA;
                    }
                    string HES_T2T = (HTAF2 is null || Convert.ToDouble(HTAF2) == 0) ? "NULL" : HTAF2.ToString();
                    string HES_T3T = (HTAF3 is null || Convert.ToDouble(HTAF3) == 0) ? "NULL" : HTAF3.ToString();
                    string HES_T4T = (HTAF4 is null || Convert.ToDouble(HTAF4) == 0) ? "NULL" : HTAF4.ToString();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{HKOL},{HMOIN},{HTAF},{HES_T2T},{HES_T3T},{HES_T4T}
                                        ,N'{hMbaa}'
                                        ,N'{Strings.Left(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور برگشت فروش شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255)}'
                                        ,{Math.Round((double)HFRST[HFRST_EOF].MBAA)},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                }
                if (JAMF + HFRST[HFRST_EOF].MABL_HAZ - HFRST[HFRST_EOF].TAKHFIF + HFRST[HFRST_EOF].MBAA > 0)
                {
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T}
                                        ,N'{HFRST[HFRST_EOF].CUST_NO}'
                                        ,N'{Strings.Left("فاكتور برگشت فروش.  شماره" + HFRST[HFRST_EOF].NUMBER + "مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + HFRST[HFRST_EOF].MOLAH, 255)}'
                                        ,{Math.Round((double)(JAMF + HFRST[HFRST_EOF].MABL_HAZ - HFRST[HFRST_EOF].TAKHFIF + HFRST[HFRST_EOF].MBAA))},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                }

                if (HFRST[HFRST_EOF].M_NAGHD != 0)
                {
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T}
                                        ,N'{HFRST[HFRST_EOF].CUST_NO}'
                                        ,N'{Strings.Left("مبلغ نقد فاكتور برگشت فروش.  شماره" + HFRST[HFRST_EOF].NUMBER + "مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255)}'
                                        ,{Math.Round((double)(HFRST[HFRST_EOF].M_NAGHD))},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                }
                if (HFRST[HFRST_EOF].M_NAGHD != 0)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{Baseknow.SANDOGH},{HFRST[HFRST_EOF].DEPATMAN},{HFRST[HFRST_EOF].SHIFT}
                                        ,N'{Baseknow.SANDOGH + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + HFRST[HFRST_EOF].SHIFT}'
                                        ,N'{Strings.Left("مبلغ نقد فاكتور برگشت فروش.  شماره" + HFRST[HFRST_EOF].NUMBER + "مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255)}'
                                        ,{Math.Round((double)(HFRST[HFRST_EOF].M_NAGHD))},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");

                }
                JAMP = 0d;
                var PRST = dbms.DoGetDataSQL<VISITOR_DTL>("SELECT     dbo.VISITOR_DTL.* FROM dbo.VISITOR_DTL WHERE     (NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (TAG = 24) ").ToList();
                for (int PRST_EOF = 0; PRST_EOF < PRST.Count; PRST_EOF++)
                {
                    visitorn = GETTAFNAME(PRST[PRST_EOF].CUST_NO);
                    if ((bool)!PRST[PRST_EOF].STAT)
                    {
                        double sumu = (double)(JAMF - HFRST[HFRST_EOF].TAKHFIF + Convert.ToDouble(Interaction.IIf(SafeToDouble(Strings.Mid(Baseknow.OPTIONSS, 62, 1)) == 5d, HFRST[HFRST_EOF].MBAA, 0)));
                        //+ Interaction.IIf(Conversions.ToDouble(Strings.Mid(Baseknow.OPTIONSS, 62, 1)) == 5d, HFRST[HFRST_EOF].MBAA, 0)) * PRST[PRST_EOF].DARSAD / 100;
                        if (Math.Round((double)(sumu * PRST[PRST_EOF].DARSAD / 100)) != PRST[PRST_EOF].PURSANT)
                        {
                            PRST[PRST_EOF].PURSANT = Math.Round((double)(sumu * PRST[PRST_EOF].DARSAD / 100));
                            dbms.DoExecuteSQL($"UPDATE  dbo.VISITOR_DTL SET PURSANT = {Math.Round((double)(sumu * PRST[PRST_EOF].DARSAD / 100))}  WHERE NUMBER = {HFRST[HFRST_EOF].NUMBER} AND(TAG = 24)  AND CUST_NO = N'{PRST[PRST_EOF].CUST_NO}'");
                        }
                    }
                    else if (PRST[PRST_EOF].PURSANT != PRST[PRST_EOF].PURSANT / (JAMF - HFRST[HFRST_EOF].TAKHFIF + (Baseknow.OPTIONSS.Substring(62, 1) == "5" ? HFRST[HFRST_EOF].MBAA : 0)) * 100)
                    {
                        PRST[PRST_EOF].DARSAD = PRST[PRST_EOF].PURSANT / (JAMF - HFRST[HFRST_EOF].TAKHFIF + (Baseknow.OPTIONSS.Substring(62, 1) == "5" ? HFRST[HFRST_EOF].MBAA : 0)) * 100;
                        dbms.DoExecuteSQL($"UPDATE  dbo.VISITOR_DTL SET DARSAD = {PRST[PRST_EOF].DARSAD}  WHERE NUMBER = {HFRST[HFRST_EOF].NUMBER} AND(TAG = 24  AND CUST_NO = N'{PRST[PRST_EOF].CUST_NO}')");

                    }
                    if (PRST[PRST_EOF].PURSANT != 0)
                    {
                        double? PHKOL = null;
                        double? PHMOIN = null;
                        double? PHTAF = null;
                        double? PHTAF2 = null;
                        double? PHTAF3 = null;
                        double? PHTAF4 = null;
                        GETTAF3(PRST[PRST_EOF].CUST_NO, ref PHKOL, ref PHMOIN, ref PHTAF, ref PHTAF2, ref PHTAF3, ref PHTAF4);

                        string _PHTAF2_ = (Convert.ToDouble(PHTAF2) == 0 || PHTAF2 is null) ? "NULL" : PHTAF2.ToString();
                        string _PHTAF3_ = (Convert.ToDouble(PHTAF3) == 0 || PHTAF3 is null) ? "NULL" : PHTAF3.ToString();
                        string _PHTAF4_ = (Convert.ToDouble(PHTAF4) == 0 || PHTAF4 is null) ? "NULL" : PHTAF4.ToString();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,    HES_T2, HES_T3,  HES_T4 ,hes,SHARH,BED,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{GETKOL(PRST[PRST_EOF].CUST_NO)},{GETMOIN(PRST[PRST_EOF].CUST_NO)},{GETTAF(PRST[PRST_EOF].CUST_NO)},{_PHTAF2_},{_PHTAF3_},{_PHTAF4_}    
                                        ,N'{PRST[PRST_EOF].CUST_NO}'
                                        ,N'{Strings.Left(" فاكتور برگشت فروش . شماره" + HFRST[HFRST_EOF].NUMBER + "بابت " + PRST[PRST_EOF].DARSAD + "درصد سهم پورسانت " + GETTAFNAME(PRST[PRST_EOF].CUST_NO) + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + Interaction.IIf(IsNull(PRST[PRST_EOF].TOZIH), "", PRST[PRST_EOF].TOZIH), 255)}'
                                        ,{PRST[PRST_EOF].PURSANT},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");
                        JAMP = (double)(JAMP + PRST[PRST_EOF].PURSANT);

                    }
                }
                if (JAMP > 0d)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,HES_K,HES_M,HES_T,hes,SHARH,BES,NUMBER,ARZD,TAG)
                                            VALUES({max_ns},{GETKOL(Baseknow.HPOR)},{GETMOIN(Baseknow.HPOR)},{GETTAF(Baseknow.HPOR)}
                                        , N'{Baseknow.HPOR}'
                                        ,N'{Strings.Left("بابت درصد سهم  فاكتور برگشت فروش . شماره" + HFRST[HFRST_EOF].NUMBER + "" + visitorn, 255)}'
                                        ,{JAMP},
                                        {HFRST[HFRST_EOF].NUMBER}
                                        ,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 4, HFRST[HFRST_EOF].ARZD)}
                                        ,25)");

                }
                }); ////Parallel For
                //} ////normal loop for i

                progressReporter.Complete();
                LogWriter.WriteLog("پایان برگشت فروش 2" + DateTime.Now.ToString());
            }
            finally
            {
                // کش فقط تا پایان همین فراخوانی زنده می‌ماند؛ وگرنه تا پایان عمر برنامه
                // نرخ و نام‌های کهنه برمی‌گرداند.
                if (cacheOwnedHere)
                {
                    LookupCacheEnabled = false;
                    ClearLookupCaches();
                }
            }
        }


        public static void SANADKHAD(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }
            long CON, i;
            bool isDefaccChecked = Generaly.defacc;


            double? max_ns, MABL_CHK = null, JAMF, JAMCH, CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, takh;
            string shart;
            object a = default, fs;
            //var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            List<DEED_HED> SHRST; // Just declare it

            var HEDRST = dbms.DoGetDataSQL<HEAD_LST>("SELECT HEAD_LST.* FROM HEAD_LST WHERE (TAG=14) AND (NUMBER >=" + NUMBER + ") AND (NUMBER <=" + NUMBER2 + ")").ToList();

            LogWriter.WriteLog("شروع باز سازي از سند فاکتور خدمات شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + DateTime.Now);
            for (int ROW = 0; ROW < HEDRST.Count; ROW++) //while (!HEDRST.EOF)
            {
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = (ROW + 1) / ((double)HEDRST.Count) * 100.0; // Calculate the progress percentage
                        auto_run.PRGR_C9.Value = progress; // Update the progress bar
                        auto_run.UpdateOverallProgressBar();
                        //                    auto_run.LBL_C9.Content = $"{progress:F2}%";
                    }));
                }
                //DoEvents();
                //Forms["GUG"]["num"] = i;
                if (!IsNull(HEDRST[ROW].CUST_NO))
                {
                    GETTAF3(HEDRST[ROW].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }

                if (HEDRST[ROW].N_S == null || HEDRST[ROW].N_S == 0)
                {
                    var SHARH_S = Strings.Right(" فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " خريدار: " + GETTAFNAME(HEDRST[ROW].CUST_NO), 100);
                    max_ns = Createsanad(Convert.ToInt64(HEDRST[ROW].DATE_N), SHARH_S, 0, 14, Convert.ToByte(true), HEDRST[ROW].USER_NAME);
                    HEDRST[ROW].N_S = max_ns;
                }
                else
                {
                    shart = "NO_S = 14 AND N_S = " + HEDRST[ROW].N_S;
                    SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart}").ToList();

                    max_ns = SHRST.FirstOrDefault().N_S;
                }
                if (IsNull(HEDRST[ROW].N_S) || HEDRST[ROW].N_S != max_ns)
                {
                    HEDRST[ROW].N_S = max_ns;
                }
                var JST_0 = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + HEDRST[ROW].NUMBER + " ) AND ((INVO_LST.TAG)=14))").ToList();
                if (JST_0.Count > 0 && !IsNull(JST_0.FirstOrDefault()))
                {
                    JAMF = (double)JST_0.FirstOrDefault();
                }
                else
                {
                    JAMF = 0d;
                }
                ;
                var JST_1 = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETD.MABL) AS SumOfMABL FROM PAY_GETD WHERE (((PAY_GETD.TAG)=14) AND ((PAY_GETD.NUMBER)= " + HEDRST[ROW].NUMBER + " ))").ToList();
                if (JST_1.Count > 0 && !IsNull(JST_1.FirstOrDefault()))
                {
                    JAMCH = (double)JST_1.FirstOrDefault();
                }
                else
                {
                    JAMCH = 0d;
                }
                // Set JST = New ADODB.Recordset
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[ROW].NUMBER + ") AND ((DEED_DTL.TAG)= 14))");

                if (JAMF != 0d)
                {

                    var hes_ = HEDRST[ROW].CUST_NO;
                    var SHARH_ = Strings.Right("فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + HEDRST[ROW].MOLAH, 255);
                    var BED_ = JAMF + HEDRST[ROW].MABL_HAZ + HEDRST[ROW].MBAA;


                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,      HES_K, HES_M,   HES_T, HES_T2   ,HES_T3, HES_T4,     hes,        SHARH,   BED, NUMBER, TAG,RADIF)
		                                                        VALUES ({max_ns}, {CKOL}, {CMOIN}, {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{hes_}', N'{SHARH_}', {BED_}, {HEDRST[ROW].NUMBER} ,14,{HEDRST[ROW].NUMBER})");
                }

                var JST = dbms.DoGetDataSQL<QUERY_MODEL3>("SELECT INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE (((INVO_LST.NUMBER)=" + HEDRST[ROW].NUMBER + ") AND ((INVO_LST.TAG)=14)); ").ToList();
                for (int EOF = 0; EOF < JST.Count; EOF++) //while (!JST.EOF())
                {
                    if (isDefaccChecked)
                    {
                        try
                        {
                            CREATHES(Baseknow.DARAM, HEDRST[ROW].DEPATMAN, Convert.ToInt64(JST[EOF].CODE), JST[EOF].NAME); //JST[EOF](4)
                        }
                        catch (Exception)
                        {
                            LogWriter.WriteLog("فاکتور خدمات خطا در برگه شماره :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر خدمات در درآمد وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                        }
                    }

                    var hes_ = Baseknow.DARAM + "-" + HEDRST[ROW].DEPATMAN + "-" + Convert.ToDouble(JST[EOF].CODE);
                    var SHARH_ = Strings.Left("فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " خدمات " + Strings.Trim(JST[EOF].NAME), 255);
                    var BES_ = JST[EOF].MABL_K;

                    dbms.DoExecuteSQL($@"       INSERT INTO dbo.DEED_DTL (N_S,       HES_K,              HES_M,                  HES_T,         hes,        SHARH,    BES,          NUMBER,       TAG)
		                                                        VALUES ({max_ns}, {Baseknow.DARAM}, {HEDRST[ROW].DEPATMAN}, {JST[EOF].CODE}, N'{hes_}', N'{SHARH_}', {BES_}, {HEDRST[ROW].NUMBER} ,14)");

                }
                if (HEDRST[ROW].MABL_HAZ != 0)
                {

                    var HES_K = GETKOL(HEDRST[ROW].MOIN_HAZ);
                    var HES_M = GETMOIN(HEDRST[ROW].MOIN_HAZ);
                    var HES_T = GETTAF(HEDRST[ROW].MOIN_HAZ);
                    var hes = GETKOL(HEDRST[ROW].MOIN_HAZ) + "-" + GETMOIN(HEDRST[ROW].MOIN_HAZ) + "-" + GETTAF(HEDRST[ROW].MOIN_HAZ);
                    var SHARH = Strings.Right("سرويس فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " - " + GETTAFNAME(HEDRST[ROW].MOIN_HAZ), 255);


                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,     HES_K,   HES_M,   HES_T,     hes,       SHARH,              BES,                NUMBER,      TAG)
		                                                        VALUES ({max_ns}, {HES_K}, {HES_M}, {HES_T}, N'{hes}', N'{SHARH}', {HEDRST[ROW].MABL_HAZ}, {HEDRST[ROW].NUMBER} ,14)");

                }
                if (JAMCH != 0d) // چكهاي دريافتي
                {

                    var Filter_ = "NUMBER = " + HEDRST[ROW].NUMBER + " AND TAG = " + 14;
                    var CHRST = dbms.DoGetDataSQL<PAY_GETD>($"SELECT * FROM PAY_GETD WHERE {Filter_}").ToList();
                    if (CHRST.Count > 0 && !IsNull(CHRST.FirstOrDefault().NUMBER))
                    {
                        for (int S = 0; S < CHRST.Count; S++) //while (!CHRST.EOF)
                        {
                            MABL_CHK = MABL_CHK + CHRST[S].MABL;

                            var SHARH_ = Strings.Right("چك " + CHRST[S].N_SERI + "بانك " + GETBANK((double)CHRST[S].BANK) + " " + CHRST[S].SHOBEH + " مورخ " + Strings.Format(CHRST[S].DATE_S, "####/##/##"), 255);


                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S, HES_K,                    HES_M,                     HES_T,               hes,            SHARH,          BED,         N_SERI,               BANK   ,        NUMBER,        TAG)
		                                                        VALUES ({max_ns}, {GETKOL(Baseknow.ADA)}, {GETMOIN(Baseknow.ADA)}, {GETTAF(Baseknow.ADA)}, N'{Baseknow.ADA}', N'{SHARH_}', {CHRST[S].MABL},{CHRST[S].N_SERI}, {CHRST[S].BANK},{HEDRST[ROW].NUMBER} ,14)");

                            var _SHARH_ = Strings.Right("ف.خ." + HEDRST[ROW].NUMBER + " - " + "چك " + CHRST[S].N_SERI + "بانك " + GETBANK((double)CHRST[S].BANK) + " " + CHRST[S].SHOBEH + " مورخ " + Strings.Format(CHRST[S].DATE_S, "####/##/##"), 255);

                            string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                            string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                            string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                            dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T,HES_T2,      HES_T3,HES_T4,       hes,                        SHARH,          BES,            NUMBER,        TAG)
		                                                        VALUES ({max_ns},    {CKOL}, {CMOIN},{CTAF},{CTAF2T}, {CTAF3T}, {CTAF4T}, N'{HEDRST[ROW].CUST_NO}', N'{_SHARH_}', {CHRST[S].MABL}, {HEDRST[ROW].NUMBER} ,14)");

                        }
                    }
                    else
                    {
                    }
                }

                if (HEDRST[ROW].M_NAGHD != 0)
                {

                    var SHARH_ = Strings.Right("مبلغ نقد فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);


                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,    HES_K, HES_M,    HES_T,  HES_T2,  HES_T3,  HES_T4,    hes,                        SHARH,                    BES,       NUMBER,         TAG)
		                                                        VALUES ({max_ns},{CKOL}, {CMOIN}, {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HEDRST[ROW].CUST_NO}', N'{SHARH_}',  {HEDRST[ROW].M_NAGHD}, {HEDRST[ROW].NUMBER} ,14)");


                    //SDRST.update();
                }

                if (HEDRST[ROW].M_NAGHD != 0)
                {

                    var hes_ = Baseknow.SANDOGH + "-" + HEDRST[ROW].DEPATMAN + "-" + HEDRST[ROW].SHIFT;
                    var SHARH_ = Strings.Right("مبلغ نقد فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);


                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,            HES_K,             HES_M,                 HES_T,            hes,       SHARH,             BED,                NUMBER,          TAG)
		                                                        VALUES ({max_ns}, {Baseknow.SANDOGH}, {HEDRST[ROW].DEPATMAN}, {HEDRST[ROW].SHIFT}, N'{hes_}', N'{SHARH_}', {HEDRST[ROW].M_NAGHD}, {HEDRST[ROW].NUMBER} ,  14)");

                    //SDRST.update();
                }
                takh = 0d;
                if (Baseknow.TKHF == 1)
                {

                    var hes_ = Baseknow.HDARAM + "-1-1";
                    var SHARH_ = Strings.Right("مبلغ تخفيف فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 60);
                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,            HES_K,   HES_M, HES_T,     hes,       SHARH,             BED,                 NUMBER,            TAG)
		                                                        VALUES ({max_ns}, {Baseknow.HDARAM}, 1,      1,    N'{hes_}', N'{SHARH_}', {HEDRST[ROW].TAKHFIF}, {HEDRST[ROW].NUMBER} ,  14)");

                }
                else if (Baseknow.TKHF == 3)
                {
                    var rst_o = dbms.DoGetDataSQL<QUERY_MODEL4>("SELECT     dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.INVO_LST.MABL_K, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.CODE, dbo.HEAD_LST.CUST_KIND FROM  dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE     (dbo.HEAD_LST.NUMBER = " + HEDRST[ROW].NUMBER + ") AND (dbo.HEAD_LST.TAG = 14)").ToList();
                    if (rst_o.Count > 0)
                    {
                        takh = 0d;
                        for (int U = 0; U < rst_o.Count; U++) //while (!rst.EOF())
                        {
                            if (Math.Round((double)rst_o[U].N_MOIN) != 0)
                            {
                                if (isDefaccChecked)
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.HDARAM, HEDRST[ROW].DEPATMAN, Convert.ToInt64(rst_o[U].CODE), "تخفيف " + GETKALANAME(Convert.ToDouble(rst_o[U].CODE)));
                                    }
                                    catch (Exception)
                                    {
                                        LogWriter.WriteLog("فاکتور خدمات خطا در برگه شماره :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر تخفيفات كالا  وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                    }
                                }

                                var SHARH = Strings.Right("مبلغ تخفيف فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);
                                var hes = Baseknow.HDARAM + "-" + HEDRST[ROW].DEPATMAN + "-" + Convert.ToDouble(rst_o[U].CODE);
                                var BED_ = Math.Round((double)rst_o[U].N_MOIN);

                                takh = takh + Math.Round((double)rst_o[U].N_MOIN);


                                dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,            HES_K,             HES_M,                 HES_T,       hes,       SHARH,      BED,        NUMBER,          TAG)
		                                                                      VALUES ({max_ns}, {Baseknow.HDARAM}, {HEDRST[ROW].DEPATMAN}, {rst_o[U].CODE}, N'{hes}', N'{SHARH}', {BED_}, {HEDRST[ROW].NUMBER} ,  14)");
                                //SDRST.update();
                            }
                            //rst.MoveNext();
                        }
                    }

                    if (HEDRST[ROW].TAKHFIF != takh)
                    {
                        HEDRST[ROW].TAKHFIF = (double)takh;
                    }
                }
                else
                {
                    dbms.DoExecuteSQL("UPDATE    dbo.INVO_LST SET  N_KOL = 0, N_MOIN = 0 WHERE (NUMBER = " + HEDRST[ROW].NUMBER + " ) AND (TAG = 14)");
                    var rst_w = dbms.DoGetDataSQL<QUERY_MODEL5>("SELECT dbo.INVO_LST.NUMBER,dbo.INVO_LST.id, dbo.INVO_LST.TAG, dbo.TAKHPERS.CUST_CO, dbo.TAKHPERS.TAKH_COD, dbo.TAKHPERS.TAFPER, dbo.INVO_LST.MABL_K, dbo.STUF_DEF.NAME,  dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN   FROM  dbo.INVO_LST INNER JOIN dbo.TAKHPERS ON dbo.INVO_LST.CODE = dbo.TAKHPERS.TAKH_COD INNER JOIN  dbo.STUF_DEF ON dbo.TAKHPERS.TAKH_COD = dbo.STUF_DEF.CODE WHERE (dbo.INVO_LST.NUMBER = " + HEDRST[ROW].NUMBER + ") And (dbo.INVO_LST.TAG = 14) And (dbo.TAKHPERS.CUST_CO = " + HEDRST[ROW].CUST_KIND + ")").ToList();

                    string _where = " WHERE (dbo.INVO_LST.NUMBER = " + HEDRST[ROW].NUMBER + ") And (dbo.INVO_LST.TAG = 14) And (dbo.TAKHPERS.CUST_CO = " + HEDRST[ROW].CUST_KIND + ")";
                    if (rst_w.Count > 0)
                    {
                        takh = 0d;
                        for (int Q = 0; Q < rst_w.Count; Q++) //while (!rst.EOF())
                        {
                            rst_w[Q].N_KOL = rst_w[Q].TAFPER;
                            rst_w[Q].N_MOIN = Math.Round((double)(rst_w[Q].MABL_K / 100 * rst_w[Q].TAFPER));
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_KOL = {rst_w[Q].N_KOL} , N_MOIN = {rst_w[Q].N_MOIN} WHERE id = {rst_w[Q].id}");
                            //rst.update();

                            if (Math.Round((double)(rst_w[Q].MABL_K / 100 * rst_w[Q].TAFPER)) != 0)
                            {
                                if (isDefaccChecked)
                                {
                                    try
                                    {
                                        CREATHES(Baseknow.HDARAM, HEDRST[ROW].DEPATMAN, Convert.ToInt64(rst_w[Q].TAKH_COD), "تخفيف " + rst_w[Q].NAME);
                                    }
                                    catch (Exception)
                                    {
                                        LogWriter.WriteLog("فاکتور خدمات خطا در برگه شماره :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر كالا در تخفيفات درآمد وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                    }
                                }

                                var hes_ = Baseknow.HDARAM + "-" + HEDRST[ROW].DEPATMAN + "-" + rst_w[Q].TAKH_COD; //(3)
                                var SHARH_ = Strings.Right("مبلغ تخفيف فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 60);
                                var _BED = Math.Round((double)(rst_w[Q].MABL_K / 100 * rst_w[Q].TAFPER));

                                takh = takh + Math.Round((double)(rst_w[Q].MABL_K / 100 * rst_w[Q].TAFPER));



                                dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,            HES_K,             HES_M,                 HES_T,            hes,       SHARH,     BED,         NUMBER,          TAG)
		                                                                     VALUES ({max_ns}, {Baseknow.HDARAM}, {HEDRST[ROW].DEPATMAN}, {rst_w[Q].TAKH_COD}, N'{hes_}', N'{SHARH_}', {_BED}, {HEDRST[ROW].NUMBER} ,  14)");
                                //SDRST.update();
                            }
                            //rst.MoveNext();
                        }
                    }
                    if (HEDRST[ROW].TAKHFIF != takh)
                    {
                        HEDRST[ROW].TAKHFIF = (double)takh;
                    }
                }

                if (HEDRST[ROW].TAKHFIF != 0)
                {

                    var SHARH_ = Strings.Right("مبلغ تخفيف فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);
                    var BES = HEDRST[ROW].TAKHFIF;

                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,    HES_T,  HES_T2,  HES_T3,  HES_T4,        hes,                 SHARH,      BES,               NUMBER, TAG)
		                                               VALUES ({max_ns},   {CKOL}, {CMOIN},   {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HEDRST[ROW].CUST_NO}', N'{SHARH_}',  {BES}, {HEDRST[ROW].NUMBER},  14)");
                }
                if (HEDRST[ROW].MABL_HAV != 0)
                {

                    var SHARH = Strings.Right("مبلغ حواله فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);

                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,    HES_T,  HES_T2,  HES_T3,  HES_T4,       hes,                  SHARH,             BES,               NUMBER,            TAG)
		                                               VALUES ({max_ns},   {CKOL}, {CMOIN},   {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HEDRST[ROW].CUST_NO}', N'{SHARH}',  {HEDRST[ROW].MABL_HAV}, {HEDRST[ROW].NUMBER},  14)");


                    //SDRST.update();
                }

                if (HEDRST[ROW].MABL_HAV != 0)
                {

                    var hes_ = GETKOL(HEDRST[ROW].MOIN_HAV) + "-" + GETMOIN(HEDRST[ROW].MOIN_HAV) + "-" + GETTAF(HEDRST[ROW].MOIN_HAV);
                    var SHARH_ = Strings.Right("مبلغ حواله فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);
                    var BED = HEDRST[ROW].MABL_HAV;
                    //SDRST.Fields("NUMBER") = HEDRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 14;


                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,            HES_K,                        HES_M,                          HES_T,                        hes,       SHARH,  BED,        NUMBER,          TAG)
		                                                        VALUES ({max_ns}, {GETKOL(HEDRST[ROW].MOIN_HAV)}, {GETMOIN(HEDRST[ROW].MOIN_HAV)}, {GETTAF(HEDRST[ROW].MOIN_HAV)}, N'{hes_}', N'{SHARH_}', {BED}, {HEDRST[ROW].NUMBER} ,  14)");
                    //SDRST.update();
                }

                if (HEDRST[ROW].MABL_VAR != 0)
                {

                    var SHARH = Strings.Right("مبلغ واريزي فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);

                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,    HES_T,  HES_T2,  HES_T3,  HES_T4,            hes,             SHARH,           BES,                    NUMBER, TAG)
		                                               VALUES ({max_ns},   {CKOL}, {CMOIN},   {CTAF},{CTAF2T},{CTAF3T},{CTAF4T}, N'{HEDRST[ROW].CUST_NO}', N'{SHARH}',  {HEDRST[ROW].MABL_VAR}, {HEDRST[ROW].NUMBER},  14)");


                    //SDRST.update();
                }

                if (HEDRST[ROW].MABL_VAR != 0)
                {
                    //SDRST.AddNew(); // مبلغ حواله
                    //SDRST.Fields("N_S") = max_ns;
                    var HES_K_ = GETKOL(HEDRST[ROW].MOIN_VAR);
                    var HES_M_ = GETMOIN(HEDRST[ROW].MOIN_VAR);
                    var HES_T_ = GETTAF(HEDRST[ROW].MOIN_VAR);
                    var hes_ = HEDRST[ROW].MOIN_VAR;
                    var SHARH_ = Strings.Right("مبلغ حواله فاكتور خدمات شماره " + HEDRST[ROW].NUMBER + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);
                    var BED_ = HEDRST[ROW].MABL_VAR;
                    //SDRST.Fields("NUMBER") = HEDRST[ROW].NUMBER;
                    //SDRST.Fields("TAG") = 14;

                    dbms.DoExecuteSQL($@"		INSERT INTO dbo.DEED_DTL (N_S,      HES_K,   HES_M,     HES_T,      hes,       SHARH,    BED,       NUMBER,            TAG)
		                                                        VALUES ({max_ns}, {HES_K_}, {HES_M_}, {HES_T_}, N'{hes_}', N'{SHARH_}', {BED_}, {HEDRST[ROW].NUMBER} ,  14)");
                    //SDRST.update();
                }
                if (HEDRST[ROW].MBAA != 0)
                {
                    //SDRST.AddNew(); // ماليات بر ارزش افزوده
                    //SDRST.Fields("N_S") = max_ns;
                    if (!IsNull(HEDRST[ROW].HMBAA))
                    {
                        GETTAF3(HEDRST[ROW].HMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }

                    var SHARH = Strings.Right(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور خدمات شماره " + HEDRST[ROW].NUMBER1 + " مورخ" + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 255);

                    string HTAF2T = (Convert.ToDouble(HTAF2) == 0 || HTAF2 is null) ? "NULL" : HTAF2.ToString();
                    string HTAF3T = (Convert.ToDouble(HTAF3) == 0 || HTAF3 is null) ? "NULL" : HTAF3.ToString();
                    string HTAF4T = (Convert.ToDouble(HTAF4) == 0 || HTAF4 is null) ? "NULL" : HTAF4.ToString();

                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,    HES_K,   HES_M,    HES_T,  HES_T2,  HES_T3,  HES_T4,        hes,                 SHARH,          BES,               NUMBER,     TAG)
		                                               VALUES ({max_ns},   {HKOL}, {HMOIN},   {HTAF},{HTAF2T},{HTAF3T},{HTAF4T}, N'{HEDRST[ROW].HMBAA}', N'{SHARH}',  {HEDRST[ROW].MBAA}, {HEDRST[ROW].NUMBER},  14)");


                    //SDRST.update();
                }

            }
            LogWriter.WriteLog("پایان فاکتور خدمات" + DateTime.Now.ToString());
            //DoCmd.Close(acForm, "GUG");


        }

        public static (double?, bool) GENSANADANBARGARD(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            bool isDefaccChecked = Generaly.defacc;

            var HEDRST = dbms.DoGetDataSQL<QUERY_MODEL2>($"SELECT GRD_NUM, GRD_DATE, GRD_ANBAR, GRD_HES, N_S, COMMENT, USER_NAME FROM dbo.ANBGRD_HEAD WHERE GRD_NUM >= {NUMBER} AND GRD_NUM <= {NUMBER2} ORDER BY GRD_NUM").ToList();
            LogWriter.WriteLog($"GENSANADANBARGARD: شروع بازسازی اسناد انبارگردانی از شماره {NUMBER} تا {NUMBER2} - تعداد برگه‌ها: {HEDRST.Count}");

            var progressReporter = new ThrottledProgressReporter(
                HEDRST.Count,
                InternalCalling && auto_run != null ? auto_run.Dispatcher : null,
                value =>
                {
                    auto_run.PRGR_C10.Value = Math.Max(auto_run.PRGR_C10.Value, value);
                    auto_run.UpdateOverallProgressBar();
                });

            if (HEDRST.Count == 0)
            {
                progressReporter.Complete();
                return (SANAD_NUMBER, IsSuccessfully);
            }

            // ۱) پیش‌خوانی همه‌ی اقلام انبارگردانی با یک کوئری (به‌جای یک کوئری به‌ازای هر برگه)
            var headNumbers = HEDRST.Where(h => h?.GRD_NUM != null).Select(h => (double)h.GRD_NUM.Value).ToList();
            var minNum = SqlNum(headNumbers.Min());
            var maxNum = SqlNum(headNumbers.Max());

            var lstMap = new Dictionary<double, List<QRE_BAZ_18>>();
            var wantedHeads = new HashSet<double>(headNumbers);

            foreach (var line in dbms.DoGetDataSQL<QRE_BAZ_18>(
                $"SELECT dbo.ANBGRD_LST.*, MOG - NUM3 AS EKH FROM dbo.ANBGRD_LST " +
                $"WHERE (MOG - NUM2 <> 0) AND (MOG - NUM1 <> 0) AND GRD_NUM BETWEEN {minNum} AND {maxNum}"))
            {
                if (line?.GRD_NUM == null || !wantedHeads.Contains(line.GRD_NUM.Value)) { continue; }
                var key = (double)line.GRD_NUM.Value;
                if (!lstMap.TryGetValue(key, out var list))
                {
                    list = new List<QRE_BAZ_18>();
                    lstMap[key] = list;
                }
                list.Add(line);
            }

            // ۲) پیش‌ساخت حساب‌های «کالا در انبار»
            if (isDefaccChecked)
            {
                var accountsToEnsure = new HashSet<(double? Kol, double? Moin, string? Code)>();
                foreach (var hRow in HEDRST)
                {
                    if (hRow?.GRD_NUM == null || !lstMap.TryGetValue(hRow.GRD_NUM.Value, out var lines)) { continue; }
                    foreach (var l in lines)
                    {
                        if (!string.IsNullOrEmpty(l.CODE) && hRow.GRD_ANBAR.HasValue)
                        {
                            accountsToEnsure.Add((Baseknow.MOGODIA, hRow.GRD_ANBAR.Value, l.CODE));
                        }
                    }
                }

                foreach (var acc in accountsToEnsure)
                {
                    try
                    {
                        CREATHES(acc.Kol, acc.Moin, Convert.ToInt64(acc.Code), acc.Code);
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteLog($"GENSANADANBARGARD: خطا در پیش‌ساخت حساب کالا {acc.Code}: {ex.Message}");
                    }
                }
            }

            // ۳) پردازش موازی برگه‌های انبارگردانی
            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, HEDRST_EOF =>
            {
                var hRow = HEDRST[HEDRST_EOF];
                if (hRow?.GRD_NUM == null)
                {
                    progressReporter.ReportOne();
                    return;
                }

                double? max_ns;
                var SHSH = Strings.Left(" انبار گرداني شماره " + hRow.GRD_NUM + " از انبار " + hRow.GRD_ANBAR + " مورخ " + Strings.Format(hRow.GRD_DATE, "####/##/##"), 100);

                if (hRow.N_S == null)
                {
                    max_ns = Createsanad((long)hRow.GRD_DATE, SHSH, 0, 17, 1, hRow.USER_NAME);
                    hRow.N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE dbo.ANBGRD_HEAD SET N_S = {max_ns} WHERE GRD_NUM = {hRow.GRD_NUM}");
                }
                else
                {
                    max_ns = hRow.N_S;
                    var SARST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM dbo.DEED_HED WHERE NO_S = 17 AND N_S = {max_ns}").FirstOrDefault();
                    if (SARST != null)
                    {
                        dbms.DoExecuteSQL($"UPDATE dbo.DEED_HED SET DATE_S = {hRow.GRD_DATE}, SHARH_S = N'{SqlText(SHSH)}', GHATEI = 0, NO_S = 17, OKF = 1, USER_NAME = N'{SqlText(hRow.USER_NAME)}' WHERE NO_S = 17 AND N_S = {max_ns}");
                    }
                    else
                    {
                        max_ns = Createsanad((long)hRow.GRD_DATE, SHSH, 0, 17, 1, hRow.USER_NAME);
                        hRow.N_S = max_ns;
                        dbms.DoExecuteSQL($"UPDATE dbo.ANBGRD_HEAD SET N_S = {max_ns} WHERE GRD_NUM = {hRow.GRD_NUM}");
                    }
                }

                SANAD_NUMBER = max_ns;

                var batchQueries = new List<string>
                {
                    $"DELETE FROM dbo.DEED_DTL WHERE N_S = {max_ns}"
                };

                double JAMF = 0d;

                if (lstMap.TryGetValue(hRow.GRD_NUM.Value, out var JST))
                {
                    foreach (var line in JST)
                    {
                        double lastmab = Convert.ToDouble(line.MABL);
                        double ekhVal = line.EKH ?? 0d;
                        double itemDiffRound = Math.Round(lastmab * ekhVal);

                        if (itemDiffRound != 0d)
                        {
                            if (ekhVal > 0)
                            {
                                var SHARH = Strings.Left(" انبار گرداني شماره " + hRow.GRD_NUM + " از انبار " + hRow.GRD_ANBAR + " مورخ " + Strings.Format(hRow.GRD_DATE, "####/##/##") + " به مقدار" + ekhVal, 255);
                                batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES) VALUES ({max_ns},{Baseknow.MOGODIA},{hRow.GRD_ANBAR},{line.CODE},N'{Baseknow.MOGODIA + "-" + hRow.GRD_ANBAR + "-" + line.CODE}',N'{SqlText(SHARH)}',{SqlNum(itemDiffRound)})");
                            }
                            else
                            {
                                var SHARH = Strings.Left(" انبار گرداني شماره " + hRow.GRD_NUM + " از انبار " + hRow.GRD_ANBAR + " مورخ " + Strings.Format(hRow.GRD_DATE, "####/##/##") + " به مقدار" + (ekhVal * -1), 255);
                                batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED) VALUES ({max_ns},{Baseknow.MOGODIA},{hRow.GRD_ANBAR},{line.CODE},N'{Baseknow.MOGODIA + "-" + hRow.GRD_ANBAR + "-" + line.CODE}',N'{SqlText(SHARH)}',{SqlNum(Math.Round(lastmab * ekhVal * -1))})");
                            }
                        }

                        batchQueries.Add($"UPDATE dbo.ANBGRD_LST SET MABL = {SqlNum(lastmab)} WHERE GRD_NUM = {hRow.GRD_NUM} AND CODE = N'{SqlText(line.CODE)}'");
                        JAMF += itemDiffRound;
                    }
                }

                if (JAMF != 0d)
                {
                    double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                    GETTAF3(hRow.GRD_HES, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    var SHARH = Strings.Left("انبار گرداني شماره " + hRow.GRD_NUM + " از انبار " + hRow.GRD_ANBAR + " مورخ " + Strings.Format(hRow.GRD_DATE, "####/##/##"), 255);

                    if (JAMF > 0d)
                    {
                        batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, SHARH, BED) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{SqlText(hRow.GRD_HES)}',N'{SqlText(SHARH)}',{SqlNum(JAMF)})");
                    }
                    else
                    {
                        batchQueries.Add($"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, SHARH, BES) VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{SqlText(hRow.GRD_HES)}',N'{SqlText(SHARH)}',{SqlNum(JAMF * -1)})");
                    }
                }

                // همه‌ی دستورهای این برگه در «یک» تراکنش
                var sb = new StringBuilder();
                sb.Append("SET XACT_ABORT ON; BEGIN TRANSACTION;");
                foreach (var q in batchQueries) { sb.Append(q).Append(';'); }
                sb.Append("COMMIT TRANSACTION;");
                dbms.DoExecuteSQL(sb.ToString());

                progressReporter.ReportOne();
            });

            progressReporter.Complete();
            LogWriter.WriteLog($"پایان انبار گردانی با موفقیت: {DateTime.Now}");
            return (SANAD_NUMBER, IsSuccessfully);
        }

        public static void GENSANADVD(object fnum, long TNUM, bool InternalCalling = true)
        {
            double progressu = 0;
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }
            var HFRST = dbms.DoGetDataSQL<CHKREC_H>($"SELECT * FROM dbo.CHKREC_H WHERE     (IDH BETWEEN {fnum}  AND  {TNUM} )  ORDER BY IDH").ToList();
            LogWriter.WriteLog($"شروع باز سازي از سند وصول چكهاي دريافتي شماره :  {fnum} تا سند شماره : {TNUM}" + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);
            ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, ROW => // while (!HFRST.EOF)
            {
                string SHRH;
                double? CKOLV = null, CMOINV = null, CTAFV = null, CTAF2V = null, CTAF3V = null, CTAF4V = null, CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, CKOLD = null, CMOIND = null, CTAFD = null, CTAF2D = null, CTAF3D = null, CTAF4D = null;
                double max_ns = 0, MABL_CHK, JAMF, JAMCH;
                double takh;
                string shart;

                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progressu++;
                        auto_run.PRGR_C11.Value = progressu / ((double)HFRST.Count) * 100.0;
                        auto_run.UpdateOverallProgressBar();
                    }));
                }

                if (IsNull(HFRST.FirstOrDefault().N_S))
                {
                    var SHARH_S = "اعلام وصول چكهاي دريافتي";
                    var UNAME = GETUSERNAME(HFRST[ROW].UID);
                    max_ns = Createsanad(Convert.ToInt64(HFRST[ROW].DATE), SHARH_S, 0, 6, Convert.ToByte(true), UNAME);
                }
                else
                {
                    shart = "N_S = " + HFRST[ROW].N_S + " AND  NO_S = 6 ";
                    var SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart} ").ToList();
                    if (SHRST.Count == 0)
                    {
                        var SHARH_S = "اعلام وصول چكهاي دريافتي";
                        var UNAME = GETUSERNAME(HFRST[ROW].UID);
                        max_ns = Createsanad(Convert.ToInt64(HFRST[ROW].DATE), SHARH_S, 0, 6, Convert.ToByte(true), UNAME);
                    }
                    else
                    {
                        var SHARH_S = "اعلام وصول چكهاي دريافتي";
                        var UNAME = GETUSERNAME(HFRST[ROW].UID);
                        dbms.DoExecuteSQL($@"UPDATE DEED_HED SET DATE_S = {HFRST[ROW].DATE} ,SHARH_S = N'{SHARH_S}' , GHATEI = 0 , NO_S = 6 , OKF = 1 , USER_NAME = N'{UNAME}' WHERE {shart} ");

                    }
                }
                if (IsNull(HFRST[ROW].N_S))
                {
                    HFRST[ROW].N_S = max_ns;
                    dbms.DoExecuteSQL($@"UPDATE CHKREC_H SET N_S = {max_ns} WHERE IDH = {HFRST[ROW].IDH}");

                }

                if (!IsNull(HFRST[ROW].N_S))
                {
                    dbms.DoExecuteSQL("DELETE FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + HFRST[ROW].N_S + " ))");
                }
                ;
                var rst = dbms.DoGetDataSQL<QUERY_MODEL6>(@"SELECT        dbo.CHKREC_H.IDH, dbo.CHRE_LST.N_SERI, dbo.CHRE_LST.BANK, dbo.CHRE_LST.DATE_S, dbo.CHRE_LST.DATE, dbo.CHRE_LST.RADIF, dbo.CHRE_LST.N_MOIN, dbo.CHRE_LST.N_TAF, dbo.CHRE_LST.CRT, 
                                                        dbo.CHRE_LST.UID, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH, dbo.PAY_GETD.N_S, dbo.PAY_GETD.MABL, dbo.PAY_GETD.N_KOL, dbo.PAY_GETD.N_MOIN AS N_MOIN_PGD, dbo.PAY_GETD.N_TAF AS N_TAF_PGD, 
                                                        dbo.PAY_GETD.KIND, dbo.PAY_GETD.HES1
                                                           FROM            dbo.CHKREC_H INNER JOIN
                                                        dbo.CHRE_LST ON dbo.CHKREC_H.DATE = dbo.CHRE_LST.DATE INNER JOIN
                                                        dbo.PAY_GETD ON dbo.CHRE_LST.N_SERI = dbo.PAY_GETD.N_SERI AND dbo.CHRE_LST.BANK = dbo.PAY_GETD.BANK AND dbo.CHRE_LST.DATE_S = dbo.PAY_GETD.DATE_S INNER JOIN
                                                        dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHKREC_H.idH = " + HFRST[ROW].IDH + ")").ToList();
                if (!IsNull(Baseknow.ADA))
                {
                    GETTAF3(Baseknow.ADA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }
                if (!IsNull(Baseknow.ADV))
                {
                    GETTAF3(Baseknow.ADV, ref CKOLV, ref CMOINV, ref CTAFV, ref CTAF2V, ref CTAF3V, ref CTAF4V);
                }
                for (int A = 0; A < rst.Count; A++) // while (!rst.EOF)
                {

                    object _HES_K = null;
                    object _HES_M = null;
                    object _HES_T = null;
                    object _HES_T2 = null;
                    object _HES_T3 = null;
                    object _HES_T4 = null;
                    object _hes = null;
                    if (rst[A].KIND == 1 || IsNull(rst[A].KIND))
                    {
                        _HES_K = CKOL;
                        _HES_M = CMOIN;
                        _HES_T = CTAF;
                        _HES_T2 = CTAF2;
                        _HES_T3 = CTAF3;
                        _HES_T4 = CTAF4;
                        _hes = Baseknow.ADA;
                    }
                    else
                    {
                        _HES_K = CKOLV;
                        _HES_M = CMOINV;
                        _HES_T = CTAFV;
                        _HES_T2 = CTAF2V;
                        _HES_T3 = CTAF3V;
                        _HES_T4 = CTAF4V;
                        _hes = Baseknow.ADV;
                    }

                    var _SHARH = Strings.Left(" چك " + rst[A].N_SERI + " بانك " + rst[A].NAMES + " " + rst[A].SHOBEH + " مورخ " + Strings.Format(rst[A].DATE_S, "####/##/##"), 255);

                    string _HES_T2T = (Convert.ToDouble(_HES_T2) == 0 || _HES_T2 is null) ? "NULL" : _HES_T2.ToString();
                    string _HES_T3T = (Convert.ToDouble(_HES_T3) == 0 || _HES_T3 is null) ? "NULL" : _HES_T3.ToString();
                    string _HES_T4T = (Convert.ToDouble(_HES_T4) == 0 || _HES_T4 is null) ? "NULL" : _HES_T4.ToString();

                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,            HES_K,     HES_M,    HES_T,     HES_T2,     HES_T3,   HES_T4,      hes,       SHARH,         BES,           BANK,            N_SERI)
		                                               VALUES ({HFRST[ROW].N_S},   {_HES_K}, {_HES_M},  {_HES_T},{_HES_T2T},{_HES_T3T},{_HES_T4T}, N'{_hes}', N'{_SHARH}',  {rst[A].MABL}, {rst[A].BANK},  {rst[A].N_SERI})");
                    //SDRST.update();

                    GETTAF3(rst[A].HES1, ref CKOLD, ref CMOIND, ref CTAFD, ref CTAF2D, ref CTAF3D, ref CTAF4D);

                    var SHARH_ = Strings.Left(" چك " + rst[A].N_SERI + " بانك " + rst[A].NAMES + " " + rst[A].SHOBEH + " مورخ " + Strings.Format(rst[A].DATE_S, "####/##/##"), 255);

                    string HES_T2T_ = (Convert.ToDouble(CTAF2D) == 0 || CTAF2D is null) ? "NULL" : CTAF2D.ToString();
                    string HES_T3T_ = (Convert.ToDouble(CTAF3D) == 0 || CTAF3D is null) ? "NULL" : CTAF3D.ToString();
                    string HES_T4T_ = (Convert.ToDouble(CTAF4D) == 0 || CTAF4D is null) ? "NULL" : CTAF4D.ToString();

                    dbms.DoExecuteSQL($@" INSERT INTO dbo.DEED_DTL (N_S,            HES_K,    HES_M,    HES_T,  HES_T2,      HES_T3,  HES_T4,        hes,            SHARH,         BED,           BANK,            N_SERI)
		                                               VALUES ({HFRST[ROW].N_S},   {CKOLD}, {CMOIND},  {CTAFD},{HES_T2T_},{HES_T3T_},{HES_T4T_}, N'{rst[A].HES1}', N'{SHARH_}',  {rst[A].MABL}, {rst[A].BANK},  {rst[A].N_SERI})");

                    //SDRST.update();
                    rst[A].N_S = HFRST[ROW].N_S;
                    //rst.update();
                    //rst.MoveNext();
                }
            });
            LogWriter.WriteLog("پایان وصول چكهاي دريافتي");
        }

        //AUTO_BAZ_FUNCTIONS ---------------------------------------------------------------------------------------------------------
    }
}

