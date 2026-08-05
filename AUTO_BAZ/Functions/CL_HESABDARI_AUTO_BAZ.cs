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

                    var jst_sec = dbms.DoGetDataSQL<QRE12>("SELECT INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, INVO_LST.AVRAGE FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE     (dbo.INVO_LST.NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") AND (dbo.INVO_LST.TAG = 2) ").ToList();
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
                        var jst_thr = dbms.DoGetDataSQL<QRE14>("SELECT     dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.CODE, dbo.INVO_LST.ANBAR, dbo.STUF_DEF.NAME,  dbo.INVO_LST.AVRAGE FROM  dbo.INVO_LST INNER JOIN  dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE (dbo.INVO_LST.NUMBER = " + HFRST[HFRST_EOF].NUMBER + ") And (dbo.INVO_LST.TAG = 2) And (dbo.INVO_LST.ANBAR <> 0)").ToList();
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


                            DAST = sanatPriceNeeded
                                ? Math.Round((double)GETSTANDARDPRICE_DAST(jst_thr[jst_thr_EOF].CODE, (long)(Convert.ToInt64(HFRST[HFRST_EOF].DATE_N) * jst_thr[jst_thr_EOF].MEGHk)))
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
                        var CHRST = dbms.DoGetDataSQL<PAY_GETD>("SELECT N_SERI,BANK,DATE_S,DATE,SHOBEH,MABL,NAME_TAH,N_HESAB,N_S,N_KOL,N_MOIN,N_TAF,N_KOL2,N_MOIN2,N_TAF2,N_KOL3,N_MOIN3,N_TAF3,NUMBER,TAG,ANBAR,RADIF,CUST_NO,VAZ,LIST_NO,KIND,SANDUGH,HES1,HES2,HES3,ESTELAM FROM PAY_GETD WHERE NUMBER = " + HFRST[HFRST_EOF].NUMBER + " AND TAG = 2").ToList();
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
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            bool isDefaccChecked = Generaly.defacc;

            double? _SANAD_NUMBER = null;
            //rst.GetType().GetProperties()[4].GetValue(rst, null) 
            //, CKOL = default, CMOIN = default, CTAF = default, CTAF2 = default, CTAF3 = default, CTAF4 = default, HKOL = default, HMOIN = default, HTAF = default, HTAF2 = default, HTAF3 = default, HTAF4 = default, takh;

            //var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT N_S, DATE_S, SHARH_S, NO_S, ANBAR, N_FACTOR, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF FROM dbo.DEED_HED").ToList();

            var HFRST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>($"SELECT     * FROM dbo.HEAD_LST WHERE     (NUMBER BETWEEN  {fnum}  AND  {TNUM}  AND (TAG = 12)) ORDER BY NUMBER").ToList();


            double progress = 0;
            if (InternalCalling)
            {
                auto_run.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run.PRGR_C2.Value = progress; // Update the progress bar
                    auto_run.UpdateOverallProgressBar();                                   // auto_run.LBL_C2.Content = $"{progress:F2}%";

                }));
            }

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HFRST.Count);
            ExecuteWithPreferredLoop(0, HFRST.Count, dbParallelOptions, HFRST_EOF =>
            {
                QRE10 SARST = null;
                string SHSH;
                double max_ns, MABL_CHK = default, JAMF, JAMCH;
                long K;
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++;
                        auto_run.PRGR_C2.Value = progress / ((double)HFRST.Count) * 100.0;// Update the progress bar
                        auto_run.UpdateOverallProgressBar();
                    }));
                }

                double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null, takh;
                string shart;
                double KHMAVAV;
                double KHNIM;
                double KHSAKHT;
                double KHSAY;
                double BAZAR;
                var HS = new double[8];
                if (!IsNull(HFRST[HFRST_EOF].CUST_NO))
                {
                    GETTAF3(HFRST[HFRST_EOF].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                    if (CKOL.HasValue && CMOIN.HasValue && CTAF.HasValue && CKOL > 0 && CMOIN > 0 && CTAF > 0)
                    {
                        CREATHES(CKOL, CMOIN, CTAF, GETTAFNAME(HFRST[HFRST_EOF].CUST_NO));
                    }
                }
                SHSH = Conversions.ToString(Interaction.IIf((bool)Baseknow.SNDKH, Strings.Left(" فاكتورهاي  خريد  " + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255), Strings.Left(" فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " خريدار: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255)));
                if ((bool)Baseknow.SNDKH) // سند روزانه است
                {
                    if (!IsNull(HFRST[HFRST_EOF].N_S)) // فاکتور سند دارد
                    {
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and n_s = " + HFRST[HFRST_EOF].N_S).FirstOrDefault();
                        if (!(SARST is null))  // اگرسند  فاکتورهست
                        {
                            if (SARST.DATE_S == HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                            {
                                max_ns = (double)HFRST[HFRST_EOF].N_S;
                            }
                            else
                            {
                            SEJ:
                                //SARST = New ADODB.Recordset SARST.Open "SELECT    BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and DATE_s = " & HFRST[HFRST_EOF].DATE_N;
                                SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and DATE_s = " + HFRST[HFRST_EOF].DATE_N).FirstOrDefault();

                                //if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                                if (!(SARST is null))   // اگرسند به تاريخ فاکتورهست
                                {
                                    max_ns = (double)SARST.N_S;
                                }
                                else
                                {
                                    max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 1, -1, HFRST[HFRST_EOF].USER_NAME);
                                    HFRST[HFRST_EOF].N_S = max_ns;
                                }
                            }
                        }
                        else
                        {
                            //goto SEJ;
                            SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and DATE_s = " + HFRST[HFRST_EOF].DATE_N).FirstOrDefault();

                            //if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                            if (!(SARST is null))   // اگرسند به تاريخ فاکتورهست
                            {
                                max_ns = (double)SARST.N_S;
                            }
                            else
                            {
                                max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 1, -1, HFRST[HFRST_EOF].USER_NAME);
                                HFRST[HFRST_EOF].N_S = max_ns;
                            }
                        } // چک کن اگه نيست صادر کن
                    }
                    else
                    {
                        //goto SEJ;
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and DATE_S = " + HFRST[HFRST_EOF].DATE_N).FirstOrDefault();

                        //if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                        if (!(SARST is null))   // اگرسند به تاريخ فاکتورهست
                        {
                            max_ns = (double)SARST.N_S;
                        }
                        else
                        {
                            max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 1, -1, HFRST[HFRST_EOF].USER_NAME);
                            HFRST[HFRST_EOF].N_S = max_ns;
                        }
                    } // چک کن اگه نيست صادر کن
                }
                else if (!IsNull(HFRST[HFRST_EOF].N_S)) // تک سندي
                                                        // فاکتور سند دارد
                {
                    //Set SARST = New ADODB.Recordset:
                    SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and n_s = " + HFRST[HFRST_EOF].N_S).FirstOrDefault();
                    //SARST.Open("SELECT    n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 1 and N_s = " + HFRST[HFRST_EOF].N_S, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                    if (!(SARST is null))   // اگرسند فاکتورهست
                    {
                        if (SARST.DATE_S != HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                        {
                            dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + HFRST[HFRST_EOF].DATE_N + ",SHARH_S = N'" + SHSH + "',GHATEI = 0,NO_S = 1,OKF=-1,USER_NAME = N'" + HFRST[HFRST_EOF].USER_NAME + "' WHERE N_S =" + HFRST[HFRST_EOF].N_S);
                        }
                        max_ns = (double)HFRST[HFRST_EOF].N_S;
                    }
                    else
                    {
                        max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 1, -1, HFRST[HFRST_EOF].USER_NAME);
                        HFRST[HFRST_EOF].N_S = max_ns;
                    }
                }
                else
                {
                    max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 1, -1, HFRST[HFRST_EOF].USER_NAME);
                    HFRST[HFRST_EOF].N_S = max_ns;
                }

                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (IsNull(HFRST[HFRST_EOF].N_S) || HFRST[HFRST_EOF].N_S != max_ns)
                {
                    HFRST[HFRST_EOF].N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE HEAD_LST SET N_S = {max_ns} WHERE NUMBER = {HFRST[HFRST_EOF].NUMBER} AND TAG = 12");
                    //HFRST.update();
                }
                //SumOfMABL
                var jst_SumOfMABL = dbms.DoGetDataSQL<double?>("SELECT Sum(INVO_LST.MABL_K) AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + " ) AND ((INVO_LST.TAG)=1))").FirstOrDefault();
                if (jst_SumOfMABL > 0 && !IsNull(jst_SumOfMABL))
                {
                    JAMF = Math.Round((double)jst_SumOfMABL);
                }
                else
                {
                    JAMF = 0d;
                }

                _SANAD_NUMBER = HFRST[HFRST_EOF].N_S;
                //jst.Close();
                // Set jst = New ADODB.Recordset
                //double? SumOfMABL 
                var jst_SumOfMABL2_SumOfMABL2 = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETP.MABL) AS SumOfMABL FROM PAY_GETP WHERE (((PAY_GETP.TAG)=1) AND ((PAY_GETP.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + " ))").FirstOrDefault();
                if (jst_SumOfMABL2_SumOfMABL2 > 0 && !IsNull(jst_SumOfMABL2_SumOfMABL2))
                {
                    JAMCH = (double)jst_SumOfMABL2_SumOfMABL2;
                }
                else
                {
                    JAMCH = 0d;
                }
                KHMAVAV = 0d;
                KHNIM = 0d;
                KHSAKHT = 0d;
                KHSAY = 0d;
                BAZAR = 0d;
                HS[1] = 0d;
                HS[2] = 0d;
                HS[3] = 0d;
                HS[4] = 0d;
                HS[5] = 0d;
                HS[6] = 0d;
                HS[7] = 0d;
                //jst.Close();
                //Set jst = New ADODB.Recordset
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HFRST[HFRST_EOF].NUMBER + ") AND ((DEED_DTL.TAG)= 12))");
                var jst = dbms.DoGetDataSQL<QRE20>("SELECT INVO_LST.MABL_K, INVO_LST.MEGHk, INVO_LST.CODE, INVO_LST.ANBAR, STUF_DEF.NAME, dbo.STUF_DEF.RADAH FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE (((INVO_LST.NUMBER)=" + HFRST[HFRST_EOF].NUMBER + ") AND ((INVO_LST.TAG)=1)) ").ToList();
                ////Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                ////Forms["GUG"].Form.Refresh();
                ////Forms["GUG"]["Text2"].Requery();
                ////Forms["GUG"].Form.Repaint();
                //while (!jst.EOF())
                for (int jst_EOF = 0; jst_EOF < jst.Count; jst_EOF++)
                {
                    if (jst[jst_EOF].MABL_K != 0)
                    {
                        CREATHES(Baseknow.MOGODIA, jst[jst_EOF].ANBAR, Convert.ToInt64(jst[jst_EOF].CODE), jst[jst_EOF].NAME);
                        object N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD = default;
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) " +
                            $"VALUES({max_ns},{Baseknow.MOGODIA},{jst[jst_EOF].ANBAR},{jst[jst_EOF].CODE},N'{Baseknow.MOGODIA + "-" + jst[jst_EOF].ANBAR + "-" + jst[jst_EOF].CODE}',N'{Strings.Right("خريدفاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255)}',{Math.Round((double)jst[jst_EOF].MABL_K)},{HFRST[HFRST_EOF].NUMBER},12,{Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD)})");

                        switch (jst[jst_EOF].RADAH)
                        {
                            case 1:
                                {
                                    KHMAVAV = KHMAVAV + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 2:
                                {
                                    KHNIM = KHNIM + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 3:
                                {
                                    KHSAKHT = KHSAKHT + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 4:
                                {
                                    BAZAR = BAZAR + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 5:
                                {
                                    HS[1] = HS[1] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 6:
                                {
                                    HS[2] = HS[2] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 7:
                                {
                                    HS[3] = HS[3] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 8:
                                {
                                    HS[4] = HS[4] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 9:
                                {
                                    HS[5] = HS[5] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                            case 10:
                                {
                                    HS[6] = HS[6] + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }

                            default:
                                {
                                    KHSAY = KHSAY + Math.Round((double)jst[jst_EOF].MABL_K);
                                    break;
                                }
                        }


                        //SDRST.update();
                    }
                    //jst.MoveNext();
                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].MABL_HAZ != 0)
                {
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;
                    //SDRST.AddNew(); // كرايه حمل يا غيره
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
                    SHARH = Strings.Right("خدمات فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " - " + GETTAFNAME(HFRST[HFRST_EOF].MOIN_HAZ), 255);
                    BED = HFRST[HFRST_EOF].MABL_HAZ;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);

                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();


                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");
                    //SDRST.update();
                }
                if (JAMCH != 0d) // چكهاي دريافتي
                {
                    var CHRST = dbms.DoGetDataSQL<PAY_GETP_1>($"SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, KIND, VAZ, HES1, HES2, HES3 FROM dbo.PAY_GETP WHERE NUMBER = {HFRST[HFRST_EOF].NUMBER} AND TAG = 1").ToList();
                    //CHRST.MoveLast();
                    //CHRST.MoveFirst();
                    //CHRST.Filter = "NUMBER = " + HFRST[HFRST_EOF].NUMBER + " AND TAG = 1";

                    //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                    //Forms["GUG"].Form.Refresh();
                    //Forms["GUG"]["Text2"].Requery();
                    //Forms["GUG"].Form.Repaint();
                    if (CHRST.Count > 0 && !IsNull(CHRST.Select(X => X.NUMBER)))
                    {
                        //while (!CHRST.EOF)
                        for (int CHRST_EOF = 0; CHRST_EOF < CHRST.Count; CHRST_EOF++)
                        {
                            object N_S, HES_K, HES_M, HES_T, hes, HES_T2, HES_T3, BED, HES_T4, SHARH, BES, N_SERI, BANK, NUMBER, TAG, ARZD = null;

                            MABL_CHK = (double)(MABL_CHK + CHRST[CHRST_EOF].MABL);
                            //SDRST.AddNew(); // اسناد پرداختني
                            N_S = max_ns;
                            HES_K = GETKOL(Baseknow.APA);
                            HES_M = GETMOIN(Baseknow.APA);
                            HES_T = GETTAF(Baseknow.APA);
                            hes = Baseknow.APA;
                            SHARH = Strings.Right("چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK(Convert.ToDouble(CHRST[CHRST_EOF].BANK)) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                            BES = CHRST[CHRST_EOF].MABL;
                            N_SERI = CHRST[CHRST_EOF].N_SERI;
                            BANK = CHRST[CHRST_EOF].BANK;
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 12;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,hes ,SHARH,BES ,N_SERI,BANK,NUMBER,TAG ,ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BES},{N_SERI},{BANK},{NUMBER},{TAG},{ARZD})");
                            //SDRST.update();



                            //SDRST.AddNew(); // چكهاي پرداختي
                            N_S = max_ns;
                            HES_K = CKOL;
                            HES_M = CMOIN;
                            HES_T = CTAF;
                            HES_T2 = CTAF2;
                            HES_T3 = CTAF3;
                            HES_T4 = CTAF4;
                            hes = HFRST[HFRST_EOF].CUST_NO;
                            SHARH = Strings.Right("ف.خ." + HFRST[HFRST_EOF].NUMBER1 + " - " + "چك " + CHRST[CHRST_EOF].N_SERI + "بانك " + GETBANK(Convert.ToDouble(CHRST[CHRST_EOF].BANK)) + " " + CHRST[CHRST_EOF].SHOBEH + " مورخ " + Strings.Format(CHRST[CHRST_EOF].DATE_S, "####/##/##"), 255);
                            BED = CHRST[CHRST_EOF].MABL;
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 12;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                            //SDRST.update();
                            string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                            string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                            string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S,HES_K,HES_M,HES_T,HES_T2,HES_T3,HES_T4,hes ,SHARH,BED ,NUMBER,TAG ,ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");
                            //CHRST.MoveNext();
                        }
                    }
                    else
                    {
                    }
                    //CHRST.Close();
                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (JAMF != 0d)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T},{HES_T2},{HES_T3},{HES_T4},{SHARH},{hes },{BES },{NUMBER},{TAG },{ARZD},{RADIF}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, hes, BES, NUMBER, TAG, ARZD, RADIF = null;



                    //SDRST.AddNew(); // كل بستانكاري شخص بابت فاكتور
                    N_S = max_ns;
                    HES_K = CKOL;
                    HES_M = CMOIN;
                    HES_T = CTAF;
                    HES_T2 = CTAF2;
                    HES_T3 = CTAF3;
                    HES_T4 = CTAF4;
                    SHARH = Strings.Right("فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + " " + HFRST[HFRST_EOF].MOLAH, 255);
                    hes = HFRST[HFRST_EOF].CUST_NO;
                    BES = JAMF + HFRST[HFRST_EOF].MBAA;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    RADIF = HFRST[HFRST_EOF].NUMBER;
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, hes, BES, NUMBER, TAG, ARZD, RADIF) VALUES ({N_S},{HES_K},{HES_M},{HES_T},{HES_T2T},{HES_T3T},{HES_T4T},N'{SHARH}',N'{hes}',{BES},{NUMBER},{TAG},{ARZD},{RADIF})");



                    if (KHMAVAV != 0d)
                    {
                        //{N_S},{ HES_K},{ HES_M},{ HES_T},{ hes},{ SHARH},{ BED},{ NUMBER},{ TAG},{ ARZD}
                        // كنترل خريد '
                        //SDRST.AddNew(); // خريد
                        object BED = null;
                        N_S = max_ns;
                        HES_K = Baseknow.KHARID;
                        HES_M = 1;
                        HES_T = 1;
                        hes = Baseknow.KHARID + "-1-1";
                        SHARH = Strings.Right("خريد مواد اوليه فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                        BED = KHMAVAV;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    if (KHNIM != 0d)
                    {
                        object BED = null;
                        //{ N_S },{ HES_K},{ HES_M},{ HES_T},{ hes },{ SHARH},{ BED },{ NUMBER},{ TAG },{ ARZD}
                        // كنترل خريد '
                        //SDRST.AddNew(); // خريد
                        N_S = max_ns;
                        HES_K = Baseknow.KHARID;
                        HES_M = 2;
                        HES_T = 1;
                        hes = Baseknow.KHARID + "-2-1";
                        SHARH = Strings.Right("خريد نيمه ساخته فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                        BED = KHNIM;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    if (KHSAKHT != 0d)
                    {
                        object BED = null;
                        //{ N_S},{ HES_K},{ HES_M},{ HES_T},{ hes },{ SHARH},{ BED },{ NUMBER},{ TAG },{ ARZD}
                        // كنترل خريد '
                        //SDRST.AddNew(); // خريد
                        N_S = max_ns;
                        HES_K = Baseknow.KHARID;
                        HES_M = 3;
                        HES_T = 1;
                        hes = Baseknow.KHARID + "-3-1";
                        SHARH = Strings.Right("خريد ساخته شده فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                        BED = KHSAKHT;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    if (BAZAR != 0d)
                    {
                        object BED = null;
                        //{N_S },{HES_K},{HES_M},{HES_T},{hes },{SHARH},{BED },{NUMBER},{TAG },{ARZD}


                        // كنترل خريد '
                        //SDRST.AddNew(); // خريد
                        N_S = max_ns;
                        HES_K = Baseknow.KHARID;
                        HES_M = 4;
                        HES_T = 1;
                        hes = Baseknow.KHARID + "-4-1";
                        SHARH = Strings.Right("خريد بازرگاني  فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                        BED = BAZAR;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    if (KHSAY != 0d)
                    {
                        CREATHES(Baseknow.KHARID, 11, 1, "ساير 2");
                        object BED = null;
                        //{ N_S },{ HES_K},{ HES_M},{ HES_T},{ hes },{ SHARH},{ BED },{ NUMBER},{ TAG },{ ARZD}

                        //SDRST.AddNew(); // خريد
                        N_S = max_ns;
                        HES_K = Baseknow.KHARID;
                        HES_M = 11;
                        HES_T = 1;
                        hes = Baseknow.KHARID + "-11-1";
                        SHARH = Strings.Right("خريد ساير فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                        BED = KHSAY;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    for (K = 1L; K <= 6L; K++)
                    {
                        if (HS[(int)K] != 0d)
                        {
                            var INP1 = K + 4L;
                            CREATHES(Baseknow.KHARID, K + 4L, 1, GETGRPKALA(Convert.ToInt32(INP1)));
                            //{N_S},{HES_K},{HES_M},{HES_T},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                            object BED = null;

                            //SDRST.AddNew(); // خريد
                            N_S = max_ns;
                            HES_K = Baseknow.KHARID;
                            HES_M = K + 4L;
                            HES_T = 1;
                            hes = Baseknow.KHARID + "-" + (K + 4L) + "-1";
                            SHARH = Strings.Right("خريد " + GETGRPKALA(Convert.ToInt32(K + 4L)) + " فاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                            BED = HS[(int)K];
                            HS[7] = HS[7] + HS[(int)K];
                            NUMBER = HFRST[HFRST_EOF].NUMBER;
                            TAG = 12;
                            ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                            //SDRST.update();
                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                        }
                    }
                    //{N_S},{HES_K},{HES_M},{HES_T},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                    //SDRST.AddNew(); // پاياپاي خريد
                    N_S = max_ns;
                    HES_K = Baseknow.PKHARID;
                    HES_M = 1;
                    HES_T = 1;
                    hes = Baseknow.PKHARID + "-1-1";
                    SHARH = Strings.Right("خريدفاكتورشماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##") + "فروشنده: " + GETTAFNAME(HFRST[HFRST_EOF].CUST_NO), 255);
                    BES = KHSAY + KHSAKHT + KHNIM + KHMAVAV + BAZAR + HS[7];
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].MABL_HAZ != 0)
                {

                    //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD = null;
                    //SDRST.AddNew(); // كل بستانكاري شخص بابت خدمات
                    N_S = max_ns;
                    HES_K = CKOL;
                    HES_M = CMOIN;
                    HES_T = CTAF;
                    HES_T2 = CTAF2;
                    HES_T3 = CTAF3;
                    HES_T4 = CTAF4;
                    hes = HFRST[HFRST_EOF].CUST_NO;
                    SHARH = Strings.Right("خدمات فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BES = HFRST[HFRST_EOF].MABL_HAZ;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].M_NAGHD != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // مبلغ نقدشخص
                    N_S = max_ns;
                    HES_K = CKOL;
                    HES_M = CMOIN;
                    HES_T = CTAF;
                    HES_T2 = CTAF2;
                    HES_T3 = CTAF3;
                    HES_T4 = CTAF4;
                    hes = HFRST[HFRST_EOF].CUST_NO;
                    SHARH = Strings.Right("مبلغ نقد فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BED = HFRST[HFRST_EOF].M_NAGHD;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                // ----------------------ْحواله واريزي
                if (HFRST[HFRST_EOF].MABL_HAV != 0)
                {
                    if (HFRST[HFRST_EOF].MABL_HAV != 0)
                    {
                        //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;

                        //SDRST.AddNew(); // مبلغ واريزي شخص
                        N_S = max_ns;
                        HES_K = CKOL;
                        HES_M = CMOIN;
                        HES_T = CTAF;
                        HES_T2 = CTAF2;
                        HES_T3 = CTAF3;
                        HES_T4 = CTAF4;
                        hes = HFRST[HFRST_EOF].CUST_NO;
                        SHARH = Strings.Right("مبلغ حواله فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BED = HFRST[HFRST_EOF].MABL_HAV;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                    }
                    if (!IsNull(HFRST[HFRST_EOF].MOIN_HAV))
                    {
                        //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD = null;

                        GETTAF3(HFRST[HFRST_EOF].MOIN_HAV, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        //SDRST.AddNew(); // مبلغ حواله
                        N_S = max_ns;
                        HES_K = HKOL;
                        HES_M = HMOIN;
                        HES_T = HTAF;
                        HES_T2 = HTAF2;
                        HES_T3 = HTAF3;
                        HES_T4 = HTAF4;
                        hes = HFRST[HFRST_EOF].MOIN_HAV;
                        SHARH = Strings.Right("مبلغ حواله فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BES = HFRST[HFRST_EOF].MABL_HAV;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();

                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                    }
                    else
                    {
                        //"خطا در برگه شماره :" & HFRST.Fields("NUMBER") & " نوع :" & HFRST.Fields("tag") & "حساب معين براي مبلغ حواله مشخص نشده است"
                        LogWriter.WriteLog("خطا در برگه شماره سند خرید :" + HFRST[HFRST_EOF].NUMBER + " نوع :" + HFRST[HFRST_EOF].TAG + "حساب معين براي مبلغ حواله مشخص نشده است");
                    }
                }

                if (HFRST[HFRST_EOF].MABL_VAR != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // مبلغ واريزي شخص
                    N_S = max_ns;
                    HES_K = CKOL;
                    HES_M = CMOIN;
                    HES_T = CTAF;
                    HES_T2 = CTAF2;
                    HES_T3 = CTAF3;
                    HES_T4 = CTAF4;
                    hes = HFRST[HFRST_EOF].CUST_NO;
                    SHARH = Strings.Right("مبلغ واريزي فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BED = HFRST[HFRST_EOF].MABL_VAR;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                }
                if (HFRST[HFRST_EOF].MABL_VAR != 0)
                {
                    if (!IsNull(HFRST[HFRST_EOF].MOIN_VAR))
                    {
                        //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                        object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD = null;

                        GETTAF3(HFRST[HFRST_EOF].MOIN_VAR, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                        //SDRST.AddNew(); // مبلغ واريزي
                        N_S = max_ns;
                        HES_K = HKOL;
                        HES_M = HMOIN;
                        HES_T = HTAF;
                        HES_T2 = HTAF2;
                        HES_T3 = HTAF3;
                        HES_T4 = HTAF4;
                        hes = HFRST[HFRST_EOF].MOIN_VAR;
                        SHARH = Strings.Right("مبلغ واريزي فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                        BES = HFRST[HFRST_EOF].MABL_VAR;
                        NUMBER = HFRST[HFRST_EOF].NUMBER;
                        TAG = 12;
                        ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                        //SDRST.update();
                        string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                        string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                        string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                    }
                    else
                    {
                        LogWriter.WriteLog("خطا در برگه شمارهسند خرید  :" + HFRST[HFRST_EOF].NUMBER + " نوع :" + HFRST[HFRST_EOF].TAG + "حساب معين براي مبلغ واریزی مشخص نشده است");
                    }
                }

                // ----------------------
                if (HFRST[HFRST_EOF].M_NAGHD != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // مبلغ نقدصندوق
                    N_S = max_ns;
                    HES_K = Baseknow.SANDOGH;
                    HES_M = HFRST[HFRST_EOF].DEPATMAN;
                    HES_T = HFRST[HFRST_EOF].SHIFT;
                    hes = Baseknow.SANDOGH + "-" + HFRST[HFRST_EOF].DEPATMAN + "-" + HFRST[HFRST_EOF].SHIFT;
                    SHARH = Strings.Right("مبلغ نقد فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BES = HFRST[HFRST_EOF].M_NAGHD;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].TAKHFIF != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // مبلغ تخفيف شخص
                    N_S = max_ns;
                    HES_K = CKOL;
                    HES_M = CMOIN;
                    HES_T = CTAF;
                    HES_T2 = CTAF2;
                    HES_T3 = CTAF3;
                    HES_T4 = CTAF4;
                    hes = HFRST[HFRST_EOF].CUST_NO;
                    SHARH = Strings.Right("مبلغ تخفيف فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BED = HFRST[HFRST_EOF].TAKHFIF;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].TAKHFIF != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T},{hes},{SHARH},{BES},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // تخفيف خريد
                    N_S = max_ns;
                    HES_K = Baseknow.TKHARID;
                    HES_M = 1;
                    HES_T = 1;
                    hes = Baseknow.TKHARID + "-1-1";
                    SHARH = Strings.Right("مبلغ تخفيف فاكتور خريد  شماره " + HFRST[HFRST_EOF].NUMBER1 + "-" + HFRST[HFRST_EOF].FNUMCO + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BES = HFRST[HFRST_EOF].TAKHFIF;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES, NUMBER, TAG, ARZD) VALUES ({N_S},{HES_K},{HES_M},{HES_T},N'{hes}',N'{SHARH}',{BES},{NUMBER},{TAG},{ARZD})");

                }
                //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                //Forms["GUG"].Form.Refresh();
                //Forms["GUG"]["Text2"].Requery();
                //Forms["GUG"].Form.Repaint();
                if (HFRST[HFRST_EOF].MBAA != 0)
                {
                    //{N_S},{HES_K},{HES_M},{HES_T}, {HES_T2}, {HES_T3}, {HES_T4},{hes},{SHARH},{BED},{NUMBER},{TAG},{ARZD}
                    object N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD = null;

                    //SDRST.AddNew(); // مالليات بر ارزش افزوده
                    N_S = max_ns;
                    if (!IsNull(HFRST[HFRST_EOF].HMBAA))
                    {
                        GETTAF3(HFRST[HFRST_EOF].HMBAA, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);
                    }
                    HES_K = HKOL;
                    HES_M = HMOIN;
                    HES_T = HTAF;
                    HES_T2 = HTAF2;
                    HES_T3 = HTAF3;
                    HES_T4 = HTAF4;
                    hes = HFRST[HFRST_EOF].HMBAA;
                    SHARH = Strings.Right(Baseknow.ARSESH + "% ماليات بر ارزش افزوده فاكتور خريد شماره " + HFRST[HFRST_EOF].NUMBER1 + " مورخ" + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 255);
                    BED = HFRST[HFRST_EOF].MBAA;
                    NUMBER = HFRST[HFRST_EOF].NUMBER;
                    TAG = 12;
                    ARZD = Interaction.IIf(IsNull(HFRST[HFRST_EOF].ARZD), 1, HFRST[HFRST_EOF].ARZD);
                    //SDRST.update();
                    string HES_T2T = (Convert.ToDouble(HES_T2) == 0 || HES_T2 is null) ? "NULL" : HES_T2.ToString();
                    string HES_T3T = (Convert.ToDouble(HES_T3) == 0 || HES_T3 is null) ? "NULL" : HES_T3.ToString();
                    string HES_T4T = (Convert.ToDouble(HES_T4) == 0 || HES_T4 is null) ? "NULL" : HES_T4.ToString();

                    try
                    {
                        dbms.DoExecuteSQL($"INSERT INTO DEED_DTL ( N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, hes, SHARH, BED, NUMBER, TAG, ARZD ) VALUES ({N_S},{HES_K},{HES_M},{HES_T}, {HES_T2T}, {HES_T3T}, {HES_T4T},N'{hes}',N'{SHARH}',{BED},{NUMBER},{TAG},{ARZD})");
                    }
                    catch (Exception)
                    {
                        LogWriter.WriteLog(@$"خطا در سند خرید : 
                                              حساب : {hes}
                                              شرح : {SHARH}
                                              شماره : {NUMBER}
                                              مبلغ بدهکار : {BED}
                                              نوع : {TAG}");
                    }

                }
                ;

            });
            //DoCmd.Close(acForm, "GUG");
            //DoCmd.Close(acForm, "GENSANADFROOSH");

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
            int GETGRPKALAcoRet = default;
            var rst = dbms.DoGetDataSQL<double?>("SELECT     radah  FROM dbo.stuf_def WHERE     (CODE = '" + CC + "')").ToList();
            if (rst.Count > 0)
            {
                GETGRPKALAcoRet = (int)rst.FirstOrDefault();
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

            const string detailInsertSql =
                          "INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BED, N_SERI, BANK, N_S, HES, ARZD, MHAZ_NO) " +
                          "SELECT THES_K, THES_M, THES_T, THES_T2, THES_T3, THES_T4, SHARH, MABL, N_SERI, BANK, @Ns, THES, ARZD, MHAZ_NO " +
                          "FROM dbo.PGET_LST WHERE ID = @TreasuryId;" +
                          "INSERT INTO dbo.DEED_DTL (HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, SHARH, BES, N_SERI, BANK, N_S, HES, ARZD, MHAZ_NO) " +
                          "SELECT FHES_K, FHES_M, FHES_T, FHES_T2, FHES_T3, FHES_T4, SHARH, MABL, N_SERI, BANK, @Ns, FHES, ARZD, MHAZ_NO " +
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

            double progress = 0;
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            List<DEED_HED_CSHARP>? SHRST = new List<DEED_HED_CSHARP>();
            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= " + NUMBER + " AND HEAD_LST.NUMBER <=" + NUMBER2 + "  and HEAD_LST.tag = 5 ) )").ToList();

            LogWriter.WriteLog("سند انتقال شروع بازسازی از سند شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + " " + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, rw =>
            {
                string DBStr;
                double MABL_CHK, JAMF, JAMCH;
                double? max_ns = null;
                string shart = "";
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++;
                        auto_run.PRGR_C4.Value = progress / ((double)HEDRST.Count) * 100.0;
                        auto_run.UpdateOverallProgressBar();
                    }));
                }

                var arst = dbms.DoGetDataSQL<TCOD_ANBAR>("SELECT  CODE, KIND FROM dbo.TCOD_ANBAR WHERE (CODE = " + HEDRST[rw].ANBAR + ")").ToList();
                if (arst.Count > 0)
                {
                    if (arst.FirstOrDefault().KIND == 1 || arst.FirstOrDefault().KIND == 2)
                    {
                        if (!(Baseknow.SANAT == true || IsNull(Baseknow.SANAT)))
                        {
                            dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[rw].NUMBER + ") AND ((DEED_DTL.TAG)= 5))");

                            goto MV;
                        }
                    }
                }

                if (HEDRST[rw]?.N_S == null)
                {
                    var SHARH_S = Strings.Left(" حواله انتقالي مواد شماره " + HEDRST[rw].NUMBER + "-" + HEDRST[rw].FNUMCO + " از انبار " + HEDRST[rw].ANBAR + " به " + HEDRST[rw].ANBARF + " مورخ " + Strings.Format(HEDRST[rw].DATE_N, "####/##/##"), 100);
                    max_ns = Createsanad(Convert.ToInt64(HEDRST[rw].DATE_N), SHARH_S, 0, 10, Convert.ToByte(true), HEDRST[rw].USER_NAME);
                    //آیا در اکسس این فلیتر روی منبع اصلی میماند چون باید از اینجا به بعد با دیتای فیلتر شده حرکت کند , که این در حلقه به مشکل میخورد
                    shart = " NO_S = 10 AND N_S = " + max_ns;

                    SANAD_NUMBER = max_ns;
                }
                else
                {
                    shart = " NO_S = 10 AND N_S = " + HEDRST[rw].N_S;

                    SANAD_NUMBER = HEDRST[rw].N_S;
                }

                SHRST = dbms.DoGetDataSQL<DEED_HED_CSHARP>($"SELECT * FROM DEED_HED WHERE {shart} ").ToList();
                if (SHRST.Count == 0 || SHRST is null)
                {
                }
                else
                {
                    max_ns = (double)SHRST.FirstOrDefault().N_S;
                    SHRST.FirstOrDefault().DATE_S = HEDRST[rw].DATE_N;
                    SHRST.FirstOrDefault().SHARH_S = Strings.Left(" حواله انتقالي مواد شماره " + HEDRST[rw].NUMBER + "-" + HEDRST[rw].FNUMCO + " از انبار " + HEDRST[rw].ANBAR + " به " + HEDRST[rw].ANBARF + " مورخ " + Strings.Format(HEDRST[rw].DATE_N, "####/##/##"), 100);
                    SHRST.FirstOrDefault().GHATEI = false;
                    SHRST.FirstOrDefault().NO_S = 10;
                    SHRST.FirstOrDefault().OKF = true;
                    SHRST.FirstOrDefault().USER_NAME = HEDRST[rw].USER_NAME;
                    //SHRST.update();

                    dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET
                                             DATE_S = {HEDRST[rw].DATE_N} ,
                                             SHARH_S = N'{SHRST.FirstOrDefault().SHARH_S}' ,
                                             GHATEI = 0,
                                             NO_S = 10,
                                             OKF = 1,
                                             USER_NAME = N'{HEDRST[rw].USER_NAME}'
                                             WHERE {shart} ");
                }


                if (IsNull(HEDRST[rw].N_S) || HEDRST[rw].N_S != max_ns)
                {
                    HEDRST[rw].N_S = max_ns;

                }
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[rw].NUMBER + ") AND ((DEED_DTL.TAG)= 5))");

                var JST = dbms.DoGetDataSQL<QRE_BAZ_1>("SELECT INVO_LST.NUMBER, INVO_LST.TAG, STUF_DEF.NAME, INVO_LST.ANBAR, INVO_LST.CODE, INVO_LST.MEGH, INVO_LST.MEGHk, INVO_LST.MEGH_MAR, INVO_LST.MABL, INVO_LST.MABL_K, INVO_LST.ANBARF FROM STUF_DEF INNER JOIN INVO_LST ON (STUF_DEF.CODE = INVO_LST.CODE) AND (STUF_DEF.CODE = INVO_LST.CODE) WHERE (((INVO_LST.NUMBER)=" + HEDRST[rw].NUMBER + ") AND ((INVO_LST.TAG)=5))").ToList();

                for (int EOF = 0; EOF < JST.Count; EOF++) // while (!JST.EOF())
                {

                    if (JST[EOF].MABL_K != 0)
                    {
                        bool valdefacc = true;
                        if (InternalCalling)
                        {
                            auto_run.Dispatcher.Invoke(new Action(() =>
                            {
                                valdefacc = auto_run.defacc.IsChecked is true;
                            }));
                        }

                        if (valdefacc is true)
                        {
                            CREATHES(Baseknow.MOGODIA, JST[EOF].ANBAR, Convert.ToInt64(JST[EOF].CODE), JST[EOF].NAME/*(2)*/);
                        }

                        var _hes_ = Baseknow.MOGODIA + "-" + JST[EOF].ANBAR + "-" + JST[EOF].CODE;
                        var _Sharh_ = Strings.Left("حواله انتقالي شماره " + HEDRST[rw].NUMBER + "-" + HEDRST[rw].FNUMCO + " مورخ " + Strings.Format(HEDRST[rw].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk, 255);

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,  HES_K, HES_M, HES_T, HES, SHARH,  BES, NUMBER, TAG)
                                                                VALUES({max_ns},
                                                                {Baseknow.MOGODIA},
                                                                {JST[EOF].ANBAR},
                                                                {JST[EOF].CODE},
                                                                N'{_hes_}' ,
                                                                N'{_Sharh_}',
                                                                {Math.Round((double)JST[EOF].MABL_K)},
                                                                {HEDRST[rw].NUMBER} ,
                                                                {5} )");
                        //SDRST.update();
                    }
                    if (JST[EOF].MABL_K != 0)
                    {
                        bool valdefacc = true;
                        if (InternalCalling)
                        {
                            auto_run.Dispatcher.Invoke(new Action(() =>
                            {
                                valdefacc = auto_run.defacc.IsChecked is true;
                            }));
                        }
                        if (valdefacc is true)
                        {
                            try
                            {
                                CREATHES(Baseknow.MOGODIA, JST[EOF].ANBARF, Convert.ToInt64(JST[EOF].CODE), JST[EOF].NAME/*(2)*/);
                            }
                            catch (Exception)
                            {
                                LogWriter.WriteLog("خطا در برگه شماره سند انتقال  :" + HEDRST[rw].NUMBER + " نوع :" + HEDRST[rw].TAG + "اخطار مهم ...! حساب متناظر كالا در انبار وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                            }
                        }
                        var hes_ = Baseknow.MOGODIA + "-" + JST[EOF].ANBARF + "-" + JST[EOF].CODE;
                        var Sharh_ = Strings.Left("حواله انتقالي شماره " + HEDRST[rw].NUMBER + "-" + HEDRST[rw].FNUMCO + " مورخ " + Strings.Format(HEDRST[rw].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + "  بابت " + JST[EOF].NAME, 255);
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S,  HES_K, HES_M, HES_T, HES, SHARH,  BED, NUMBER, TAG)
                                                                VALUES({max_ns},
                                                                {Baseknow.MOGODIA},
                                                                {JST[EOF].ANBARF},
                                                                {JST[EOF].CODE},
                                                                N'{hes_}' ,
                                                                N'{Sharh_}',
                                                                {Math.Round((double)JST[EOF].MABL_K)},
                                                                {HEDRST[rw].NUMBER} ,
                                                                {5} )");
                    }
                }
            MV:
                rw++; //HEDRST.MoveNext();
            });
            LogWriter.WriteLog($"پایان سند انتقال :{DateTime.Now}");
            //a.WRITELINE((object)DateTime.Now);
            //DoCmd.Close(acForm, "GUG");

            return (SANAD_NUMBER, IsSuccessfully);
        }
        public static (double?, bool) SANADKHORUGMAVAD(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;


            double progress = 0;
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
            if (InternalCalling)
            {
                auto_run.Dispatcher.Invoke(new Action(() =>
                {
                    valdefacc = Convert.ToBoolean(auto_run.defacc.IsChecked);
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= " + NUMBER + " AND HEAD_LST.NUMBER <=" + NUMBER2 + "  and HEAD_LST.tag = 10 ) )").ToList();
            LogWriter.WriteLog("SANADKHORUGMAVAD: شروع بازسازی از برگ شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + " " + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, R => // while (!HEDRST.EOF())
            {
                int RDD;
                double MABL_CHK, JAMF, JAMCH;
                double? max_ns = null;
                string shart = "";
                object a = default, fs;
                List<DEED_HED> SHRST = new List<DEED_HED>();


                if (!TryGetDateNumber(HEDRST[R].DATE_N, out var normalizedDate))
                {
                    LogWriter.WriteLog($"SANADKHORUGMAVAD: تاریخ نامعتبر برای برگ {HEDRST[R].NUMBER} با مقدار '{HEDRST[R].DATE_N}'.");
                    IsSuccessfully = false;
                    return;
                }

                HEDRST[R].DATE_N = normalizedDate;

                if (HEDRST[R]?.N_S == null || HEDRST[R]?.N_S == 0)
                {
                    var SHARH_S_ = Strings.Left(" حواله خروج مواد از انبار شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##"), 100);
                    max_ns = Createsanad(Convert.ToInt64(HEDRST[R].DATE_N), SHARH_S_, 0, 8, Convert.ToByte(true), HEDRST[R].USER_NAME);
                    shart = "NO_S = 8 AND N_S = " + max_ns.ToString();

                    SANAD_NUMBER = max_ns;
                }
                else
                {
                    shart = "NO_S = 8 AND N_S = " + HEDRST[R].N_S.ToString();

                    SANAD_NUMBER = HEDRST[R].N_S;

                    max_ns = HEDRST[R].N_S;
                }

                SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart} ").ToList();
                if (SHRST.Count == 0)
                {
                }
                else
                {
                    max_ns = SHRST.FirstOrDefault().N_S;
                    var SHARH_S_ = Strings.Left(" حواله خروج مواد از انبار شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##"), 100);
                    dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET
                                             DATE_S = {HEDRST[R].DATE_N} ,
                                             SHARH_S = N'{SHARH_S_}' ,
                                             GHATEI = 0,
                                             NO_S = 8,
                                             OKF = 1,
                                             USER_NAME = N'{HEDRST[R].USER_NAME}'
                                             WHERE {shart} ");
                }
                if (IsNull(HEDRST[R].N_S) || HEDRST[R].N_S != max_ns)
                {
                    HEDRST[R].N_S = max_ns;
                    dbms.DoExecuteSQL("UPDATE HEAD_LST SET N_S = " + HEDRST[R].N_S + " WHERE ((HEAD_LST.NUMBER = " + NUMBER + " and HEAD_LST.tag = 10 ) )");
                    //HEDRST.update();
                }
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[R].NUMBER + ") AND ((DEED_DTL.TAG)= 10))");
                if (!(bool)Baseknow.FINALS)
                {
                    if (HEDRST[R].NUMBER == 177)
                    {
                        int i = 0;
                    }
                    var JST = dbms.DoGetDataSQL<QRE_BAZ_2>("SELECT dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.CODE, dbo.INVO_LST.ANBAR, dbo.HEAD_MANF.CODE AS COM, ISNULL(dbo.HEAD_MANF.NAMES, dbo.STUF_DEF.NAME) AS NAM, dbo.HEAD_MANF.NAMES, dbo.HEAD_MANF.N_KOL, dbo.HEAD_MANF.NUMBER, dbo.HEAD_MANF.TNUMBER, dbo.DTL_MANF.SMABl AS SMAB FROM  dbo.STUF_DEF RIGHT OUTER JOIN dbo.HEAD_MANF INNER JOIN dbo.INVO_LST ON dbo.HEAD_MANF.FNUMB = dbo.INVO_LST.N_RASID ON dbo.STUF_DEF.CODE = dbo.HEAD_MANF.CODE INNER JOIN dbo.DTL_MANF ON dbo.DTL_MANF.CODE = dbo.INVO_LST.CODE AND dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB AND dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.INVO_LST.NUMBER = " + HEDRST[R].NUMBER + ") AND (dbo.INVO_LST.TAG = 10)").ToList();
                    for (int EOF = 0; EOF < JST.Count; EOF++) // while (!JST.EOF())
                    {
                        //DoEvents();
                        if (JST[EOF].MABL_K != 0)
                        {
                            var _SHARH = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " جهت " + Strings.Trim(JST[EOF].NAM), 255);
                            var _hes = Baseknow.MOGODIA + "-" + JST[EOF].ANBAR + "-" + Convert.ToDouble(JST[EOF].CODE);
                            if (valdefacc)
                            {
                                CREATHES(Baseknow.MOGODIA, JST[EOF].ANBAR, Convert.ToInt64(JST[EOF].CODE), GETKALANAME(Convert.ToInt64(JST[EOF].CODE)));//JST.Fileds(4)
                            }
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S,      HES_K,          HES_M,              HES_T,          SHARH,        hes,              BES,                                NUMBER,        TAG)
			                                                 VALUES ({max_ns},{Baseknow.MOGODIA} ,{JST[EOF].ANBAR} ,{JST[EOF].CODE}, N'{_SHARH}', N'{_hes}',	{Math.Round((double)JST[EOF].MABL_K)},	{HEDRST[R].NUMBER},	10)");

                            RDD = GETGRPKALAco(JST[EOF].CODE);
                            if (valdefacc)
                            {
                                CREATHES(Baseknow.PHAZ_TOL, Convert.ToDouble(Interaction.IIf(RDD == 2 || RDD == 3, 2, 1)), Convert.ToInt64(JST[EOF].CODE), GETKALANAME(Convert.ToInt64(JST[EOF].CODE)));//JST.Fileds(4)
                            }
                            var _HES_M = Interaction.IIf(RDD == 2 || RDD == 3, 2, 1);
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " جهت " + Strings.Trim(JST[EOF].NAM), 255);
                            var hes_ = Baseknow.PHAZ_TOL + "-" + Interaction.IIf(RDD == 2 || RDD == 3, 2, 1) + "-" + Convert.ToDouble(JST[EOF].CODE);
                            try
                            {
                                dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S,              HES_K,          HES_M,      HES_T,          SHARH,       hes,               BES,                                      NUMBER,TAG)
			                                                        VALUES ({max_ns}	,{Baseknow.PHAZ_TOL}  ,{_HES_M}	   ,{JST[EOF].CODE}, N'{SHARH_}', N'{hes_}',	{Math.Round((double)JST[EOF].MABL_K)},	{HEDRST[R].NUMBER},10)");
                            }
                            catch
                            {
                                //#Check Matter So Much
                            }

                            var HES_K = "";
                            var HES_M = "";
                            var HES_T = "";
                            var hes = "";

                            if (IsNull(JST[EOF].COM))
                            {
                                HES_K = JST[EOF].N_KOL.ToString();
                                HES_M = JST[EOF].NUMBER.ToString();
                                HES_T = JST[EOF].TNUMBER.ToString();
                                hes = JST[EOF].N_KOL + "-" + JST[EOF].NUMBER + "-" + JST[EOF].TNUMBER;
                            }
                            else
                            {
                                HES_K = Baseknow.HAZ_TOL.ToString();
                                HES_M = JST[EOF].COM;
                                HES_T = JST[EOF].CODE;
                                hes = Baseknow.HAZ_TOL + "-" + Convert.ToDouble(JST[EOF].COM) + "-" + Convert.ToDouble(JST[EOF].CODE);
                            }
                            SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " جهت " + Strings.Trim(JST[EOF].NAM), 255);
                            var BED_ = Math.Round((double)JST[EOF].MABL_K);
                            if (valdefacc)
                            {
                                CREATHES(Convert.ToDouble(HES_K), Convert.ToDouble(HES_M), Convert.ToDouble(HES_T), GETKALANAME(Convert.ToInt64(JST[EOF].CODE)));//JST.Fileds(4)
                            }


                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S,        HES_K,        HES_M,  HES_T,  SHARH,          hes,BED,NUMBER,TAG)
			                                                    VALUES ({max_ns}	,{HES_K}	,{HES_M}	,{HES_T}, N'{SHARH_}', N'{hes}',{BED_}, {HEDRST[R].NUMBER},	10)");
                        }
                        //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                        //Forms["GUG"].Form.Repaint();
                        JAMCH = Math.Round((double)JST[EOF].MABL_K);
                        if (JST[EOF].SMAB * JST[EOF].MEGHk != 0)
                        {
                            //SDRST.AddNew(); // كنترل كالاي در جريان ساخت
                            //SDRST.FieldsN_S = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.CONKAL;
                            //SDRST.Fields("HES_M") = JST[EOF].COM;
                            //SDRST.Fields("HES_T") = JST[EOF].CODE;
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " جهت " + Strings.Trim(JST[EOF].NAM), 255);
                            var hes_ = Baseknow.CONKAL + "-" + Convert.ToDouble(JST[EOF].COM) + "-" + Convert.ToDouble(JST[EOF].CODE);
                            var BED_ = Math.Round((double)(JST[EOF].SMAB * JST[EOF].MEGHk));
                            //SDRST.FieldsNUMBER = HEDRST[R].NUMBER;
                            //SDRST.Fields("TAG") = 10;
                            //SDRST.update();
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S,      HES_K,            HES_M,              HES_T,              SHARH,      hes,        BED,               NUMBER,               TAG)
			                                                    VALUES ({max_ns} ,{Baseknow.CONKAL} , {JST[EOF].COM}	,{JST[EOF].CODE}, N'{SHARH_}',   N'{hes_}',	  {BED_},	    {HEDRST[R].NUMBER},	        10)");
                        }
                        //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                        //Forms["GUG"].Form.Repaint();
                        if (JAMCH - JST[EOF].SMAB * JST[EOF].MEGHk != 0)
                        {
                            //SDRST.AddNew(); // عملكرد
                            //SDRST.FieldsN_S = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.AMALKARD;
                            //SDRST.Fields("HES_M") = JST[EOF].COM;
                            //SDRST.Fields("HES_T") = JST[EOF].CODE;
                            var _SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[EOF].MEGHk + " جهت " + Strings.Trim(JST[EOF].NAM), 255);
                            var _hes = Baseknow.AMALKARD + "-" + Convert.ToDouble(JST[EOF].COM) + "-" + Convert.ToDouble(JST[EOF].CODE);

                            string B = "";
                            double BDBS_VAL = 0;

                            if (JAMCH > JST[EOF].SMAB * JST[EOF].MEGHk)
                            {
                                BDBS_VAL = Math.Round((double)(JAMCH - JST[EOF].SMAB * JST[EOF].MEGHk));
                                B = "BED";
                            }
                            else
                            {
                                BDBS_VAL = Math.Round((double)(JST[EOF].SMAB * JST[EOF].MEGHk - JAMCH));
                                B = "BES";
                            }

                            if (valdefacc)
                            {
                                CREATHES(Baseknow.AMALKARD, Convert.ToDouble(JST[EOF].COM), Convert.ToInt64(JST[EOF].CODE), GETKALANAME(Convert.ToInt64(JST[EOF].CODE)));//JST.Fileds(4)
                            }
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S,      HES_K,               HES_M,              HES_T,              SHARH,      hes,        {B},                 NUMBER,                    TAG)

                                                                VALUES({max_ns} ,{Baseknow.AMALKARD} , {JST[EOF].COM}	,{JST[EOF].CODE}, N'{_SHARH_}',   N'{_hes}',	  {BDBS_VAL},	  {HEDRST[R].NUMBER},	        10)");
                        }
                        //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                        //Forms["GUG"].Form.Repaint();
                        //JST.MoveNext();
                    }
                    ;
                }
                else
                {
                    var JST = dbms.DoGetDataSQL<QRE_BAZ_3>("SELECT dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, MAX(dbo.DTL_MANF.SMABL) AS SMAB FROM   dbo.INVO_LST INNER JOIN  dbo.DTL_MANF ON dbo.DTL_MANF.CODE = dbo.INVO_LST.CODE WHERE (dbo.INVO_LST.NUMBER = " + HEDRST[R].NUMBER + ") And (dbo.INVO_LST.TAG = 10) GROUP BY dbo.INVO_LST.MABL_K, dbo.INVO_LST.MEGHk, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE").ToList();
                    for (int T = 0; T < JST.Count; T++) //while (!JST.EOF())
                    {
                        if (JST[T].MABL_K != 0)
                        {
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[T].MEGHk, 255);
                            var hes = Baseknow.MOGODIA + "-" + JST[T].ANBAR + "-" + Convert.ToDouble(JST[T].CODE);
                            var BES_ = Math.Round((double)JST[T].MABL_K);
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S       ,HES_K,             HES_M,          HES_T,          SHARH,          hes,    BES,    NUMBER,         TAG)
			                                                    VALUES ({max_ns},   {Baseknow.MOGODIA}	,{JST[T].ANBAR}	,{JST[T].CODE}, N'{SHARH_}', N'{hes}',{BES_},{HEDRST[R].NUMBER}, 10)");
                        }
                        if (JST[T].MABL_K != 0)
                        {
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[T].MEGHk, 255);
                            var hes_ = Baseknow.PHAZ_TOL + "-1-" + Convert.ToDouble(JST[T].CODE);
                            var BES_ = Math.Round((double)JST[T].MABL_K);
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S           ,HES_K,       HES_M,          HES_T,          SHARH,      hes,     BES,     NUMBER,         TAG)
			                                                    VALUES ({max_ns}, {Baseknow.PHAZ_TOL} , 1	,       {JST[T].CODE}, N'{SHARH_}', N'{hes_}',{BES_},{HEDRST[R].NUMBER}, 10)");
                        }
                        if (JST[T].MABL_K != 0)
                        {
                            if (valdefacc is true && !ISHESAB(Baseknow.HAZ_TOL, 99999, Convert.ToInt64(JST[T].CODE)))
                            {
                                CREATHES(Baseknow.HAZ_TOL, 99999, Convert.ToInt64(JST[T].CODE), GETKALANAME(Convert.ToDouble(JST[T].CODE)));
                            }
                            //SDRST.AddNew(); // هزينه تولييا سير هزينه هاي توليد
                            //SDRST.FieldsN_S = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.HAZ_TOL;
                            //SDRST.Fields("HES_M") = 99999;
                            //SDRST.Fields("HES_T") = JST[T].CODE;
                            var hes_ = Baseknow.HAZ_TOL + "-99999-" + Convert.ToDouble(JST[T].CODE);
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[T].MEGHk, 255);
                            var BED_ = Math.Round((double)JST[T].MABL_K);
                            //SDRST.FieldsNUMBER = HEDRST[R].NUMBER;
                            //SDRST.Fields("TAG") = 10;
                            //SDRST.update();
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S           ,HES_K,       HES_M,          HES_T,          SHARH,      hes,     BED,     NUMBER,         TAG)
			                                                    VALUES ({max_ns}, {Baseknow.HAZ_TOL} , 99999	, {JST[T].CODE},  N'{SHARH_}', N'{hes_}',{BED_},{HEDRST[R].NUMBER}, 10)");
                        }
                        //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                        //Forms["GUG"].Form.Repaint();
                        JAMCH = Math.Round((double)JST[T].MABL_K);
                        if (JST[T].SMAB * JST[T].MEGHk != 0)
                        {

                            CREATHES(Baseknow.CONKAL, 99999, Convert.ToInt64(JST[T].CODE), GETKALANAME(Convert.ToDouble(JST[T].CODE)));


                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[T].MEGHk, 255);
                            var hes_ = Baseknow.CONKAL + "-99999-" + Convert.ToDouble(JST[T].CODE);
                            var BED_ = Math.Round((double)(JST[T].SMAB * JST[T].MEGHk));

                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S           ,HES_K,       HES_M,          HES_T,          SHARH,      hes,     BED,     NUMBER,         TAG)
			                                                    VALUES ({max_ns}, {Baseknow.CONKAL} , 99999	,       {JST[T].CODE}, N'{SHARH_}', N'{hes_}',{BED_},{HEDRST[R].NUMBER}, 10)");
                        }
                        //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                        //Forms["GUG"].Form.Repaint();
                        if (JAMCH - JST[T].SMAB * JST[T].MEGHk != 0)
                        {
                            if (valdefacc is true)
                            {
                                //Information.Err().Clear();
                                try
                                {
                                    CREATHES(Baseknow.AMALKARD, 99999, Convert.ToInt64(JST[T].CODE), GETKALANAME(Convert.ToDouble(JST[T].CODE)));

                                }
                                catch (Exception)
                                {
                                    LogWriter.WriteLog("خطا در برگه شماره خروج مواد :" + HEDRST[R].NUMBER + " نوع :" + HEDRST[R].TAG + "اخطار مهم ...! حساب " + Baseknow.AMALKARD + "-99999-" + JST[T].CODE + "و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                }
                            }
                            //SDRST.AddNew(); // عملكرد
                            //SDRST.FieldsN_S = max_ns;
                            //SDRST.Fields("HES_K") = Baseknow.AMALKARD;
                            //SDRST.Fields("HES_M") = 99999;
                            //SDRST.Fields("HES_T") = JST[T].CODE;
                            var hes_ = Baseknow.AMALKARD + "-99999-" + Convert.ToDouble(JST[T].CODE);
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##") + " به مقدار" + JST[T].MEGHk, 255);
                            string B = "";
                            double B_VAL = 0;
                            if (JAMCH > JST[T].SMAB * JST[T].MEGHk)
                            {
                                B_VAL = Math.Round((double)(JAMCH - JST[T].SMAB * JST[T].MEGHk));
                                B = "BED";
                            }
                            else
                            {
                                B_VAL = Math.Round((double)(JST[T].SMAB * JST[T].MEGHk - JAMCH));
                                B = "BES";
                            }
                            //SDRST.FieldsNUMBER = HEDRST[R].NUMBER;
                            //SDRST.Fields("TAG") = 10;
                            //SDRST.update();
                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S           ,HES_K,        HES_M,          HES_T,          SHARH,      hes,     {B},     NUMBER,         TAG)
			                                                    VALUES ({max_ns}, {Baseknow.AMALKARD} , 99999	,    {JST[T].CODE}, N'{SHARH_}', N'{hes_}',{B_VAL},{HEDRST[R].NUMBER}, 10)");
                        }

                    }
                }
                ;
                var JST2 = dbms.DoGetDataSQL<QRE_BAZ_4>("SELECT  SUM(BED) AS SBED, SUM(BES) AS SBES FROM dbo.DEED_DTL WHERE (N_S = " + max_ns + ") ").ToList();
                if (JST2.Count > 0)
                {
                    if (JST2.FirstOrDefault().SBED is null || JST2.FirstOrDefault().SBES is null)
                    {
                    }
                    else
                    {
                        if (Math.Abs((double)(JST2.FirstOrDefault().SBED - JST2.FirstOrDefault().SBES)) <= 40 && Math.Abs((double)(JST2.FirstOrDefault().SBED - JST2.FirstOrDefault().SBES)) != 0)
                        {

                            if (valdefacc is true)
                            {
                                try
                                {
                                    CREATHES(Baseknow.AMALKARD, 99999, 99999, "كسر دهم ريال");
                                }
                                catch (Exception)
                                {

                                    LogWriter.WriteLog("خطا در برگه شماره خروج مواد :" + HEDRST[R].NUMBER + " نوع :" + HEDRST[R].TAG + "اخطار مهم ...! حساب " + Baseknow.AMALKARD + "-99999-99999" + "و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                }
                            }
                            string B = "";
                            double B_VAL = 0;
                            var hes_ = Baseknow.AMALKARD + "-99999-99999";
                            var SHARH_ = Strings.Left("حواله خروج شماره " + HEDRST[R].NUMBER + "-" + HEDRST[R].FNUMCO + " مورخ " + Strings.Format(HEDRST[R].DATE_N, "####/##/##"), 255);
                            if (JST2.FirstOrDefault().SBED - JST2.FirstOrDefault().SBES > 0)
                            {
                                B_VAL = (double)(JST2.FirstOrDefault().SBED - JST2.FirstOrDefault().SBES);
                                B = "BES";
                            }
                            else
                            {
                                B_VAL = Math.Abs((double)(JST2.FirstOrDefault().SBED - JST2.FirstOrDefault().SBES));
                                B = "BED";
                            }

                            dbms.DoExecuteSQL(@$"INSERT INTO dbo.DEED_DTL(N_S           ,HES_K,             HES_M,          HES_T,          SHARH, hes,      {B},     NUMBER,         TAG)
			                                                    VALUES ({max_ns}, {Baseknow.AMALKARD} , 99999	,       99999, N'{SHARH_}', N'{hes_}',{B_VAL},{HEDRST[R].NUMBER}, 10)");
                        }
                    }
                }
                ;
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++;
                        auto_run.PRGR_C5.Value = progress / ((double)HEDRST.Count) * 100.0;  // Update the progress bar
                        auto_run.UpdateOverallProgressBar();
                        //                    auto_run.LBL_C5.Content = $"{progress:F2}%";
                    }));
                }
            });

            return (SANAD_NUMBER, IsSuccessfully);
        }

        public static (double?, bool) SANADKHORUGSAYER(long NUMBER, long NUMBER2, bool InternalCalling = true)
        {
            double progress = 0;
            MainWindow auto_run = null;


            double? SANAD_NUMBER = null;
            bool IsSuccessfully = true;

            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= " + NUMBER + " AND HEAD_LST.NUMBER <=" + NUMBER2 + "  and HEAD_LST.tag = 11 ) )").ToList();


            LogWriter.WriteLog("SANADKHORUGSAYER : \n شروع باز سازي از سند شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + " " + DateTime.Now);

            //for (int EOF = 0; EOF < HEDRST.Count; EOF++)

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, EOF =>
            {
                string DBStr;
                double max_ns, MABL_CHK, JAMF, JAMCH;
                string shart;
                double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null, fs, a = null;

                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++;
                        auto_run.PRGR_C6.Value = progress / ((double)HEDRST.Count) * 100.0; // Update the progress bar
                        auto_run.UpdateOverallProgressBar();

                    }));
                }
                string SHSH = "";
                SHSH = Strings.Left(" حواله خروج ساير مواد از انبار شماره " + HEDRST[EOF].NUMBER + "-" + HEDRST[EOF].FNUMCO.ToString() + "مورخ " + Strings.Format(HEDRST[EOF].DATE_N, "####/##/##"), 100);
                max_ns = CRSANADGEN(SHSH, 11, 12, (double)HEDRST[EOF].NUMBER, HEDRST[EOF].N_S, (long)HEDRST[EOF].DATE_N, HEDRST[EOF].USER_NAME);
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[EOF].NUMBER + ") AND ((DEED_DTL.TAG)= 11))");
                var JST = dbms.DoGetDataSQL<QRE_BAZ_5>("SELECT     SANAD_NO, N_RASID, MABL_K, MEGHk, CODE, ANBAR FROM dbo.INVO_LST WHERE     (NUMBER = " + HEDRST[EOF].NUMBER + ") AND (TAG = 11)").ToList();
                for (int q = 0; q < JST.Count; q++) // while (!JST.EOF())
                {
                    if (JST[q]?.N_RASID == null)
                    {
                        continue; //Skip this
                    }
                    //DoEvents();
                    if (Information.IsNumeric(JST[q].N_RASID))
                    {
                        JAMCH = 0d;
                        //if ((string?)JST[q].N_RASID == "81")
                        //{
                        //    int i = 81;
                        //}
                        var JSTT = dbms.DoGetDataSQL<QRE_BAZ_6>("SELECT     FNUMB, NUMBER, TNUMBER, N_KOL, NAMES FROM dbo.HEAD_MANF WHERE     (FNUMB = " + JST[q].N_RASID + ")").ToList();
                        if ((JST[q]?.MABL_K != 0) && (JSTT.Count > 0))
                        {
                            if ((!IsNull(JSTT.FirstOrDefault().N_KOL)) && (!IsNull(JSTT.FirstOrDefault().NUMBER)) && (!IsNull(JSTT.FirstOrDefault().TNUMBER)))
                            {
                                var _hes = JSTT.FirstOrDefault().N_KOL + "-" + JSTT.FirstOrDefault().NUMBER + "-" + JSTT.FirstOrDefault().TNUMBER;
                                var _BED = Math.Round((double)JST[q].MABL_K);
                                var _SHARH = Strings.Left("حواله خروج ساير شماره " + HEDRST[EOF].NUMBER + "-" + HEDRST[EOF].FNUMCO + " مورخ " + Strings.Format(HEDRST[EOF].DATE_N, "####/##/##") + " به مقدار" + JST[q].MEGHk + " جهت " + Strings.Trim(JSTT.FirstOrDefault().NAMES), 255);
                                dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S, HES_K, HES_M, HES_T, hes, BED, SHARH,NUMBER,TAG)
                                                         VALUES ({max_ns},{JSTT.FirstOrDefault().N_KOL},{JSTT.FirstOrDefault().NUMBER},{JSTT.FirstOrDefault().TNUMBER},N'{_hes}',{_BED},N'{_SHARH}',{HEDRST[EOF].NUMBER},11)");
                                JAMCH = (double)JST[q].MABL_K;
                                var hes_ = Baseknow.MOGODIA + "-" + JST[q].ANBAR + "-" + Convert.ToDouble(JST[q].CODE);
                                var SHARH_ = Strings.Left("حواله خروج  ساير  مواد شماره " + HEDRST[EOF].NUMBER + "-" + HEDRST[EOF].FNUMCO + " مورخ " + Strings.Format(HEDRST[EOF].DATE_N, "####/##/##") + " به مقدار" + JST[q].MEGHk, 255);
                                var BES_ = Math.Round((double)JST[q].MABL_K);
                                string SANAD_NO_VAL = (JST[q].SANAD_NO is null) ? "NULL" : JST[q].SANAD_NO.ToString();
                                dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S,      HES_K,            HES_M,            HES_T,   hes,     BES,     SHARH,      NUMBER,             MHAZ_NO ,      TAG)
                                                         VALUES ({max_ns},{Baseknow.MOGODIA},{JST[q].ANBAR},{JST[q].CODE},N'{hes_}',{BES_},N'{SHARH_}',{HEDRST[EOF].NUMBER}, {SANAD_NO_VAL} ,11)");
                            }
                        }
                        ;
                    }
                    else
                    {
                        if (JST[q]?.N_RASID != null && !string.IsNullOrEmpty(JST[q]?.N_RASID))
                        {
                            GETTAF3(JST[q].N_RASID, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);

                            if (CTAF == null)
                            {

                            }

                            if (JST[q]?.MABL_K != 0 && CTAF != null) //حساب ها درست است و نال نیست
                            {
                                try
                                {
                                    var BED__ = Math.Round((double)JST[q].MABL_K);
                                    var SHARH__ = Strings.Left("حواله خروج ساير شماره " + HEDRST[EOF].NUMBER + "-" + HEDRST[EOF].FNUMCO + " مورخ " + Strings.Format(HEDRST[EOF].DATE_N, "####/##/##") + " به مقدار" + JST[q].MEGHk + " جهت " + Strings.Trim(GETTAFNAME(JST[q].N_RASID)), 255);
                                    string CTAF2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                                    string CTAF3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                                    string CTAF4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();

                                    var query = $"SELECT COUNT(1) FROM TDETA_HES WHERE N_KOL = {CKOL} AND NUMBER = {CMOIN} AND TNUMBER = {CTAF}";
                                    var count = dbms.DoGetDataSQL<int>(query).FirstOrDefault();
                                    if (count > 0) //حساب معادل آن در حسابهای کل وجود دارد
                                    {
                                        dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S, HES_K, HES_M,  HES_T, HES_T2 , HES_T3,HES_T4,        hes,           BED,      SHARH,      NUMBER,          TAG)
                                                         VALUES ({max_ns},{CKOL},{CMOIN},{CTAF},{CTAF2T} ,{CTAF3T},{CTAF4T}, N'{JST[q].N_RASID}',{BED__},N'{SHARH__}',{HEDRST[EOF].NUMBER},11)");


                                        JAMCH = (double)JST[q].MABL_K;
                                        var __hes = Baseknow.MOGODIA + "-" + JST[q].ANBAR + "-" + Convert.ToDouble(JST[q].CODE);
                                        var __SHARH = Strings.Left("حواله خروج  ساير  مواد شماره " + HEDRST[EOF].NUMBER + "-" + HEDRST[EOF].FNUMCO + " مورخ " + Strings.Format(HEDRST[EOF].DATE_N, "####/##/##") + " به مقدار" + JST[q].MEGHk, 255);
                                        var __BES = Math.Round((double)JST[q].MABL_K);
                                        string SANAD_NO_VAL = (JST[q].SANAD_NO is null) ? "NULL" : JST[q].SANAD_NO.ToString();
                                        dbms.DoExecuteSQL($@"INSERT INTO DEED_DTL(N_S,      HES_K,            HES_M,            HES_T,   hes,     BES,     SHARH,      NUMBER,       MHAZ_NO      , TAG)
                                                         VALUES ({max_ns},{Baseknow.MOGODIA},{JST[q].ANBAR},{JST[q].CODE},N'{__hes}',{__BES},N'{__SHARH}',{HEDRST[EOF].NUMBER}, {SANAD_NO_VAL} ,11)");
                                    }
                                    else
                                    {
                                        var _HESAB_ = $"{CKOL}-{CMOIN}-{CTAF}";
                                        string[] ctafs = { CTAF2T, CTAF3T, CTAF4T };
                                        string result = string.Join("-", ctafs.Where(s => !string.IsNullOrEmpty(s)));
                                        LogWriter.WriteLog($"[SANADKHORUGSAYER] : (RASID : {JST[q]?.N_RASID}) => حساب : {_HESAB_}{(!string.IsNullOrEmpty(result) ? $"-{result}" : "")}  " + DateTime.Now);
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    IsSuccessfully = false;

                                    if (ex.Number == 547)  // 547 is the error number for foreign key violations in SQL Server
                                    {
                                        LogWriter.WriteLog($"[SANADKHORUGSAYER]: (RASID : {JST[q]?.N_RASID}) Foreign Key violation: {ex.Message}");
                                    }
                                    else
                                    {
                                        LogWriter.WriteLog($"[SANADKHORUGSAYER]: (RASID : {JST[q]?.N_RASID}) SQL Error: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
                ;

                SANAD_NUMBER = max_ns;

                //};
            });
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

            double progress = 0;
            MainWindow auto_run = null;
            if (InternalCalling)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Paint
                    auto_run = (MainWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.GetType().Name == "MainWindow");
                }));
            }
            // On Error Resume Next

            //List<DEED_HED> SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            List<DEED_HED> SHRST = new List<DEED_HED>();

            var HEDRST = dbms.DoGetDataSQL<QRE_BAZ_0>("SELECT HEAD_LST.NUMBER, HEAD_LST.TAG, HEAD_LST.ANBAR, HEAD_LST.NUMBER1, HEAD_LST.DATE_N, HEAD_LST.TAH, HEAD_LST.MAS, HEAD_LST.VAS, HEAD_LST.N_S, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.M_NAGHD, HEAD_LST.MABL_VAR, HEAD_LST.MOIN_VAR, HEAD_LST.MABL_HAV, HEAD_LST.MOIN_HAV, HEAD_LST.MABL_HAZ, HEAD_LST.MOIN_HAZ, HEAD_LST.TAKHFIF, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.FNUMCO, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME FROM HEAD_LST WHERE ((HEAD_LST.NUMBER >= " + NUMBER + " AND HEAD_LST.NUMBER <=" + NUMBER2 + "  and HEAD_LST.tag = 9 ) )").ToList();
            //Forms["GUG"]["SNUM"] = HEDRST.Count;

            LogWriter.WriteLog("ورود ساخته شده تولید شروع باز سازي از سند شماره : " + NUMBER + " تا سند شماره :" + NUMBER2 + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, ROW =>
            {
                double max_ns, MABL_CHK, JAMF, JAMCH;
                string? shart = null;
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++; // Calculate the progress percentage
                        auto_run.PRGR_C7.Value = progress / ((double)HEDRST.Count) * 100.0; // Update the progress bar
                        auto_run.UpdateOverallProgressBar();
                    }));
                }

                if (Baseknow.SANAT == true || IsNull(Baseknow.SANAT))
                {
                    string _SHARH_S = Strings.Left(" برگه ورود كالا به انبار شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + "مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 100);
                    if (HEDRST[ROW]?.N_S == null)
                    {
                        _SHARH_S = Strings.Left(" برگه ورود كالا به انبار شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + "مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 100);
                        max_ns = Createsanad(Convert.ToInt64(HEDRST[ROW].DATE_N), _SHARH_S, 0, 9, Convert.ToByte(true), HEDRST[ROW].USER_NAME);
                        shart = "NO_S = 9 AND N_S =" + max_ns;
                    }
                    else
                    {
                        shart = "NO_S = 9 AND N_S =" + HEDRST[ROW].N_S;
                    }

                    SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart} ").ToList();
                    if (SHRST.Count == 0)
                    {
                        _SHARH_S = Strings.Left(" برگه ورود كالا به انبار شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + "مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##"), 100);
                        max_ns = Createsanad(Convert.ToInt64(HEDRST[ROW].DATE_N), _SHARH_S, 0, 9, Convert.ToByte(true), HEDRST[ROW].USER_NAME);
                    }
                    else
                    {
                        //بروز رسانی تاریخ سند در صورت تغییر تاریخ برگه ورود
                        max_ns = SHRST.FirstOrDefault().N_S;
                        if (SHRST.FirstOrDefault()?.DATE_S != HEDRST[ROW].DATE_N)
                        {
                            dbms.DoExecuteSQL($"UPDATE DEED_HED SET DATE_S = {HEDRST[ROW].DATE_N},SHARH_S = N'{_SHARH_S}' WHERE N_S ={max_ns}");
                        }
                    }
                    if (IsNull(HEDRST[ROW].N_S) || HEDRST[ROW].N_S != max_ns)
                    {
                        HEDRST[ROW].N_S = max_ns;
                    }

                    SANAD_NUMBER = max_ns;


                    //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                    //Forms["GUG"].Form.Repaint();
                    dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[ROW].NUMBER + ") AND ((DEED_DTL.TAG)= 9))");
                    var CHRST_0 = dbms.DoGetDataSQL<QRE_BAZ_9>("SELECT     dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, SUM(dbo.INVO_LST.MEGH) AS SumOfMEGH, SUM(dbo.INVO_LST.MEGHk) AS SumOfMEGHk, SUM(dbo.INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR,SUM(dbo.INVO_LST.MABL) AS SumOfMABL, SUM(dbo.INVO_LST.MABL_K) AS SumOfMABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF,dbo.INVO_LST.VAHED_K , dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN  dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE AND dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE GROUP BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K , dbo.STUF_DEF.NAME " + " HAVING (dbo.INVO_LST.NUMBER = " + HEDRST[ROW].NUMBER + ") AND (dbo.INVO_LST.TAG = 9)").ToList();
                    //Forms["GUG"]["Text2"] = Forms["GUG"]["Text2"] + "n";
                    //Forms["GUG"].Form.Repaint();
                    if (Strings.Mid(Baseknow.OPTIONSS, 56, 1) != "5")
                    {
                        for (int EOF = 0; EOF < CHRST_0.Count; EOF++) //while (!CHRST.EOF())
                        {
                            //DoEvents();
                            JAMCH = 0d;
                            var JST0 = dbms.DoGetDataSQL<QRE_BAZ_10>("SELECT DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, Sum(INVO_LST.MEGHk) AS SumOfMEGHk, INVO_LST.CODE AS COM, [DTL_MANF].[MEGHk]+[PERT] AS MEGHM, INVO_LST.ANBAR  FROM STUF_DEF INNER JOIN ((INVO_LST INNER JOIN HEAD_MANF ON INVO_LST.CODE = HEAD_MANF.CODE) INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB)) ON STUF_DEF.CODE = DTL_MANF.CODE GROUP BY DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.CODE, [DTL_MANF].[MEGHk]+[PERT], INVO_LST.ANBAR HAVING (((INVO_LST.TAG)=9) AND ((INVO_LST.NUMBER)=" + HEDRST[ROW].NUMBER + ") AND ((INVO_LST.CODE)='" + CHRST_0[EOF].CODE + "') AND ((INVO_LST.ANBAR)=" + CHRST_0[EOF].ANBAR + "))").ToList();
                            for (int ii = 0; ii < JST0.Count; ii++) // while (!JST.EOF())
                            {
                                if (JST0[ii].MABLK * CHRST_0[EOF].SumOfMEGHk != 0)
                                {
                                    if (isDefaccChecked)
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.CONKAL, Convert.ToDouble(JST0[ii].COM), Convert.ToInt64(JST0[ii].CODE), GETKALANAME(Convert.ToDouble(JST0[ii].CODE)));
                                        }
                                        catch (Exception)
                                        {
                                            LogWriter.WriteLog("خطا در برگه شماره ورود ساخته شده تولید  :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر كالا در كنترل كالاي در جريان ساخت وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                        }
                                    }

                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(JST0[ii].COM) + "-" + Convert.ToDouble(JST0[ii].CODE);
                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST0[ii].MEGHM * CHRST_0[EOF].SumOfMEGHk + " جهت " + Strings.Trim(CHRST_0[EOF].NAME), 255);
                                    var _BES = Math.Round((double)(JST0[ii].MABLK * CHRST_0[EOF].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST0[ii].MABLK * CHRST_0[EOF].SumOfMEGHk));

                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,             HES_M,          HES_T,     hes,      SHARH,  BES,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.CONKAL}, {JST0[ii].COM},{JST0[ii].CODE},N'{_hes}',N'{_SHARH}',{_BES},{HEDRST[ROW].NUMBER} ,9)");

                                }

                            }
                            var JST = dbms.DoGetDataSQL<QRE_BAZ_11>("SELECT STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.MEGHk, HEAD_MANF.IMBIBE_MANF, HEAD_MANF.IMBIBE_SAR, INVO_LST.CODE FROM STUF_DEF INNER JOIN (INVO_LST INNER JOIN HEAD_MANF ON INVO_LST.CODE = HEAD_MANF.CODE) ON STUF_DEF.CODE = INVO_LST.CODE WHERE (((INVO_LST.TAG)=9) AND ((INVO_LST.NUMBER)=" + HEDRST[ROW].NUMBER + ") AND ((INVO_LST.CODE)='" + CHRST_0[EOF].CODE + "'))").ToList();
                            if (JST.Count > 0)
                            {
                                if (JST.FirstOrDefault().IMBIBE_SAR * JST.FirstOrDefault().MEGHk > 0)
                                {
                                    if (isDefaccChecked)
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.CONKAL, Convert.ToDouble(JST.FirstOrDefault().CODE), 99999998, "سربار");
                                        }
                                        catch (Exception)
                                        {
                                            LogWriter.WriteLog(" ورود ساخته شده تولید خطا در برگه شماره :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر كالا در كنترل كالاي در جريان ساخت وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                        }
                                    }

                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST_0[EOF].SumOfMEGHk + " جهت " + Strings.Trim(JST.FirstOrDefault().NAME), 255);
                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(JST.FirstOrDefault().CODE) + "-99999998";
                                    var _BES = Math.Round((double)(JST.FirstOrDefault().IMBIBE_SAR * CHRST_0[EOF].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST.FirstOrDefault().IMBIBE_SAR * CHRST_0[EOF].SumOfMEGHk));


                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,             HES_M,               HES_T,     hes,      SHARH,   BES,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.CONKAL}, {JST.FirstOrDefault().CODE},99999998,N'{_hes}',N'{_SHARH}',{_BES},{HEDRST[ROW].NUMBER} ,9)");

                                }
                                if (JST.FirstOrDefault().IMBIBE_MANF * JST.FirstOrDefault().MEGHk > 0)
                                {
                                    if (isDefaccChecked)
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.CONKAL, Convert.ToDouble(JST.FirstOrDefault().CODE), 99999999, "دستمزد");
                                        }
                                        catch (Exception)
                                        {
                                            LogWriter.WriteLog("ورود ساخته شده تولید خطا در برگه شماره :" + HEDRST[ROW].NUMBER + " نوع :" + HEDRST[ROW].TAG + "اخطار مهم ...! حساب متناظر كالا در كنترل كالاي در جريان ساخت وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                                        }
                                    }

                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST_0[EOF].SumOfMEGHk + " جهت " + Strings.Trim(JST.FirstOrDefault().NAME), 255);
                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(JST.FirstOrDefault().CODE) + "-99999999";
                                    var _BES = Math.Round((double)(JST.FirstOrDefault().IMBIBE_MANF * CHRST_0[EOF].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST.FirstOrDefault().IMBIBE_MANF * CHRST_0[EOF].SumOfMEGHk));


                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,          HES_M,               HES_T,    SHARH,        hes,     BES,    NUMBER,            TAG)
                                                         VALUES ({max_ns}, {Baseknow.CONKAL}, {JST.FirstOrDefault().CODE}, 99999999, N'{_SHARH}', N'{_hes}', {_BES}, {HEDRST[ROW].NUMBER}, 9)");

                                }
                            }
                            if (JAMCH != 0d)
                            {

                                var _hes = Baseknow.MOGODIA + "-" + CHRST_0[EOF].ANBAR + "-" + Convert.ToDouble(CHRST_0[EOF].CODE);
                                var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST_0[EOF].SumOfMEGHk + " جهت " + Strings.Trim(CHRST_0[EOF].NAME), 255);
                                var _BED = Math.Round((double)(JAMCH));

                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,             HES_M,                    HES_T,         hes,      SHARH,     BED,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.MOGODIA}, {CHRST_0[EOF].ANBAR},{CHRST_0[EOF].CODE},N'{_hes}',N'{_SHARH}',{_BED},{HEDRST[ROW].NUMBER} ,9)");

                            }

                        }
                    }
                    else
                    {
                        var CHRST = dbms.DoGetDataSQL<QRE_BAZ_12>("SELECT     dbo.INVO_LST.NUMBER, dbo.INVO_LST.N_KOL, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, SUM(dbo.INVO_LST.MEGH) AS SumOfMEGH, SUM(dbo.INVO_LST.MEGHk) AS SumOfMEGHk, SUM(dbo.INVO_LST.MEGH_MAR) AS SumOfMEGH_MAR,SUM(dbo.INVO_LST.MABL) AS SumOfMABL, SUM(dbo.INVO_LST.MABL_K) AS SumOfMABL_K, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF,dbo.INVO_LST.VAHED_K , dbo.STUF_DEF.NAME FROM dbo.STUF_DEF INNER JOIN  dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE AND dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE GROUP BY dbo.INVO_LST.NUMBER, dbo.INVO_LST.N_KOL, dbo.INVO_LST.TAG, dbo.INVO_LST.ANBAR, dbo.INVO_LST.CODE, dbo.INVO_LST.FROM_A, dbo.INVO_LST.N_RASID, dbo.INVO_LST.MEGH_R, dbo.INVO_LST.RADAH, dbo.INVO_LST.SANAD_NO, dbo.INVO_LST.CUST_NO, dbo.INVO_LST.ANBARF, dbo.INVO_LST.VAHED_K , dbo.STUF_DEF.NAME " + " HAVING (dbo.INVO_LST.NUMBER = " + HEDRST[ROW].NUMBER + ") AND (dbo.INVO_LST.TAG = 9)").ToList();

                        for (int satr = 0; satr < CHRST.Count; satr++) // while (!CHRST.EOF())
                        {

                            JAMCH = 0d;
                            var JST_1 = dbms.DoGetDataSQL<QRE_BAZ_13>("SELECT dbo.HEAD_MANF.FNUMB,DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, Sum(INVO_LST.MEGHk) AS SumOfMEGHk, INVO_LST.CODE AS COM, [DTL_MANF].[MEGHk]+[PERT] AS MEGHM, INVO_LST.anbar FROM STUF_DEF INNER JOIN ((INVO_LST INNER JOIN HEAD_MANF ON INVO_LST.CODE = HEAD_MANF.CODE) INNER JOIN DTL_MANF ON (HEAD_MANF.FNUMB = DTL_MANF.FNUMB) AND (HEAD_MANF.FNUMB = DTL_MANF.FNUMB)) ON STUF_DEF.CODE = DTL_MANF.CODE GROUP BY  dbo.HEAD_MANF.FNUMB ,DTL_MANF.CODE, DTL_MANF.MABLK, STUF_DEF.NAME, INVO_LST.TAG, INVO_LST.NUMBER, INVO_LST.CODE, [DTL_MANF].[MEGHk]+[PERT], INVO_LST.anbar HAVING (((INVO_LST.TAG)=9) AND ((INVO_LST.NUMBER)=" + HEDRST[ROW].NUMBER + ") AND ((INVO_LST.CODE)='" + CHRST[satr].CODE + "') AND ((INVO_LST.anbar)=" + CHRST[satr].ANBAR + " AND  (dbo.HEAD_MANF.FNUMB = " + Interaction.IIf(IsNull(CHRST[satr].N_KOL), 0, CHRST[satr].N_KOL) + ")))").ToList();
                            for (int O = 0; O < JST_1.Count; O++) // while (!JST.EOF())
                            {
                                if (JST_1[O].MABLK * CHRST[satr].SumOfMEGHk != 0)
                                {
                                    if (isDefaccChecked)
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.CONKAL, Convert.ToDouble(JST_1[O].COM), Convert.ToInt64(JST_1[O].CODE), CHRST[satr].NAME);
                                        }
                                        catch (Exception)
                                        {
                                            LogWriter.WriteLog("اخطار مهم ...! حساب متناظر كالا در كنترل كالاي در جريان ساخت وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد ورود ساخته شده تولید  ." + Baseknow.CONKAL + "-" + JST_1[O].COM + "-" + JST_1[O].CODE);
                                        }
                                    }

                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(JST_1[O].COM) + "-" + Convert.ToDouble(JST_1[O].CODE);
                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + JST_1[O].MEGHM * CHRST[satr].SumOfMEGHk + " جهت " + Strings.Trim(GETKALANAME(Convert.ToDouble(CHRST[satr].CODE))) + " فرمول: " + Strings.Trim(CHRST[satr].N_KOL.ToString()), 255);
                                    var _BES = Math.Round((double)(JST_1[O].MABLK * CHRST[satr].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST_1[O].MABLK * CHRST[satr].SumOfMEGHk));

                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,         HES_M,        HES_T,         hes,      SHARH,     BES,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.CONKAL}, {JST_1[O].COM},{JST_1[O].CODE},N'{_hes}',N'{_SHARH}',{_BES},{HEDRST[ROW].NUMBER} ,9)");
                                }

                            }
                            ;

                            //var JST = dbms.DoGetDataSQL<QRE_BAZ_14>(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT     IMBIBE_MANF, IMBIBE_SAR, CODE FROM dbo.HEAD_MANF WHERE     (FNUMB = ", Interaction.IIf(IsNull(CHRST[satr].N_KOL), 0, CHRST[satr].N_KOL)), ")"));
                            var JST = dbms.DoGetDataSQL<QRE_BAZ_14>($"SELECT     IMBIBE_MANF, IMBIBE_SAR, CODE FROM dbo.HEAD_MANF WHERE     (FNUMB = {Interaction.IIf(IsNull(CHRST[satr].N_KOL), 0, CHRST[satr].N_KOL)})").ToList();
                            if (JST.Count > 0)
                            {
                                if (JST.FirstOrDefault().IMBIBE_SAR * CHRST[satr].SumOfMEGHk > 0)
                                {
                                    if (isDefaccChecked)
                                    {
                                        try
                                        {
                                            CREATHES(Baseknow.CONKAL, Convert.ToDouble(CHRST[satr].CODE), 99999998, "سربار");
                                        }
                                        catch (Exception)
                                        {
                                            LogWriter.WriteLog(" ورود ساخته شده تولید اخطار مهم ...! حساب متناظر كالا در كنترل كالاي در جريان ساخت وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد." + Baseknow.CONKAL + "-" + JST.FirstOrDefault().CODE + "-99999998");
                                        }
                                    }

                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST[satr].SumOfMEGHk + " جهت " + Strings.Trim(GETKALANAME(Convert.ToInt64(CHRST[satr].CODE))) + " فرمول: " + Strings.Trim(CHRST[satr].N_KOL.ToString()), 255);
                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(CHRST[satr].CODE) + "-99999998";
                                    var _BES = Math.Round((double)(JST.FirstOrDefault().IMBIBE_SAR * CHRST[satr].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST.FirstOrDefault().IMBIBE_SAR * CHRST[satr].SumOfMEGHk));

                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,         HES_M,        HES_T,         hes,      SHARH,     BES,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.CONKAL}, {CHRST[satr].CODE},99999998,N'{_hes}',N'{_SHARH}',{_BES},{HEDRST[ROW].NUMBER} ,9)");
                                }
                                if (JST.FirstOrDefault().IMBIBE_MANF * CHRST[satr].SumOfMEGHk > 0)
                                {

                                    var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST[satr].SumOfMEGHk + " جهت " + Strings.Trim(GETKALANAME(Convert.ToInt64(CHRST[satr].CODE))) + " فرمول: " + Strings.Trim(CHRST[satr].N_KOL.ToString()), 255);
                                    var _hes = Baseknow.CONKAL + "-" + Convert.ToDouble(CHRST[satr].CODE) + "-99999999";
                                    var _BES = Math.Round((double)(JST.FirstOrDefault().IMBIBE_MANF * CHRST[satr].SumOfMEGHk));
                                    JAMCH = JAMCH + Math.Round((double)(JST.FirstOrDefault().IMBIBE_MANF * CHRST[satr].SumOfMEGHk));


                                    dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,         HES_M,         HES_T,         hes,      SHARH,     BES,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.CONKAL}, {CHRST[satr].CODE},99999999,  N'{_hes}',N'{_SHARH}', {_BES},{HEDRST[ROW].NUMBER}  ,9)");
                                }
                            }
                            if (JAMCH != 0d)
                            {

                                var _hes = Baseknow.MOGODIA + "-" + CHRST[satr].ANBAR + "-" + Convert.ToDouble(CHRST[satr].CODE);
                                var _SHARH = Strings.Left("برگه ورود شماره " + HEDRST[ROW].NUMBER + "-" + HEDRST[ROW].FNUMCO + " مورخ " + Strings.Format(HEDRST[ROW].DATE_N, "####/##/##") + " به مقدار" + CHRST[satr].SumOfMEGHk + " جهت " + Strings.Trim(CHRST[satr].NAME) + " فرمول: " + Strings.Trim(CHRST[satr].N_KOL.ToString()), 255);

                                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL (N_S, HES_K,               HES_M,               HES_T,         hes,      SHARH,       BED,         NUMBER,      TAG)
                                                             VALUES ({max_ns}, {Baseknow.MOGODIA}, {CHRST[satr].ANBAR},{CHRST[satr].CODE},  N'{_hes}',N'{_SHARH}', {JAMCH},{HEDRST[ROW].NUMBER}  ,9)");
                            }

                        }
                    }
                }
                else
                {
                    dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.NUMBER)= " + HEDRST[ROW].NUMBER + ") AND ((DEED_DTL.TAG)= 9))");
                }

            });

            return (SANAD_NUMBER, IsSuccessfully);
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

            var progressCounter = 0;

            //var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            var HFRST = dbms.DoGetDataSQL<HEAD_LST>("SELECT * FROM dbo.HEAD_LST WHERE (NUMBER BETWEEN " + fnum + " AND " + TNUM + ") AND (TAG = 25)").ToList();

            LogWriter.WriteLog("شروع باز سازي از برگشت فروش 2 شماره : " + fnum + " تا فاكتور شماره :" + TNUM + DateTime.Now);

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

                var processed = Interlocked.Increment(ref progressCounter);

                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        double progress = processed / ((double)HFRST.Count) * 100.0; // Calculate the progress percentage
                        auto_run.PRGR_C8.Value = progress; // Update the progress bar
                                                           //                    auto_run.UpdateOverallProgressBar();
                        auto_run.UpdateOverallProgressBar();
                        //auto_run.LBL_C8.Content = $"{progress:F2}%";
                    }));
                }

                if (!IsNull(HFRST[HFRST_EOF]?.CUST_NO))
                {
                    GETTAF3(HFRST[HFRST_EOF].CUST_NO, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }

                string SHSH;
                SHSH = Strings.Right("فاكتور برگشت فروش شماره " + HFRST[HFRST_EOF].NUMBER + " مورخ " + Strings.Format(HFRST[HFRST_EOF].DATE_N, "####/##/##"), 100);
                if ((bool)Baseknow.SNDKH) // سند روزانه است
                {
                    List<QRE10> SARST = null;
                    if (!IsNull(HFRST[HFRST_EOF].N_S)) // فاکتور سند دارد
                    {
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and n_s = " + HFRST[HFRST_EOF].N_S).ToList();
                        if (SARST.Count > 0)  // اگرسند  فاکتورهست
                        {
                            if (SARST.Select(x => x.DATE_S).FirstOrDefault() == HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                            {
                                max_ns = (double)HFRST[HFRST_EOF].N_S;
                            }
                            else
                            {
                            SEJ:
                                SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[HFRST_EOF].DATE_N).ToList();
                                if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                                {
                                    max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                                }
                                else
                                {
                                    max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 4, -1, HFRST[HFRST_EOF].USER_NAME);

                                    HFRST[HFRST_EOF].N_S = max_ns;
                                }
                            }
                        }
                        else
                        {
                            //goto SEJ;
                            SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[HFRST_EOF].DATE_N).ToList();
                            if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                            {
                                max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                            }
                            else
                            {
                                max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 4, -1, HFRST[HFRST_EOF].USER_NAME);

                                HFRST[HFRST_EOF].N_S = max_ns;
                            }
                        } // چک کن اگه نيست صادر کن
                    }
                    else
                    {
                        //goto SEJ;
                        SARST = dbms.DoGetDataSQL<QRE10>("SELECT   BASE,n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and DATE_S = " + HFRST[HFRST_EOF].DATE_N).ToList();
                        if (SARST.Count > 0)   // اگرسند به تاريخ فاکتورهست
                        {
                            max_ns = (double)SARST.Select(x => x.N_S).FirstOrDefault();
                        }
                        else
                        {
                            max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 4, -1, HFRST[HFRST_EOF].USER_NAME);

                            HFRST[HFRST_EOF].N_S = max_ns;
                        }
                    } // چک کن اگه نيست صادر کن
                }
                else if (!IsNull(HFRST[HFRST_EOF].N_S)) // تک سندي
                                                        // فاکتور سند دارد
                {
                    var SARST = dbms.DoGetDataSQL<QRE11>("SELECT    n_s,date_s,no_s FROM dbo.deed_hed WHERE     no_s  = 4 and N_s = " + HFRST[HFRST_EOF].N_S).ToList();
                    if (SARST.Count > 0)   // اگرسند فاکتورهست
                    {
                        if (SARST.Select(x => x.DATE_S).FirstOrDefault() != HFRST[HFRST_EOF].DATE_N) // تاريخ سند و فاکتوريکي است
                        {
                            dbms.DoExecuteSQL("UPDATE DEED_HED SET DATE_S = " + HFRST[HFRST_EOF].DATE_N + ",SHARH_S = '" + SHSH + "',GHATEI = 0,NO_S = 4,OKF=-1,USER_NAME ='" + HFRST[HFRST_EOF].USER_NAME + "' WHERE N_S =" + HFRST[HFRST_EOF].N_S);
                        }
                        max_ns = (double)HFRST[HFRST_EOF].N_S;
                    }
                    else
                    {
                        max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 4, -1, HFRST[HFRST_EOF].USER_NAME);
                        HFRST[HFRST_EOF].N_S = max_ns;
                    }
                }
                else
                {
                    max_ns = Createsanad((long)HFRST[HFRST_EOF].DATE_N, SHSH, 0, 4, -1, HFRST[HFRST_EOF].USER_NAME);
                    HFRST[HFRST_EOF].N_S = max_ns;
                }
                if (IsNull(HFRST[HFRST_EOF].N_S) || HFRST[HFRST_EOF].N_S != max_ns)
                {
                    HFRST[HFRST_EOF].N_S = max_ns;
                    dbms.DoExecuteSQL($"UPDATE HEAD_LST set n_s = {max_ns} WHERE     (NUMBER = {HFRST[HFRST_EOF].NUMBER} AND (TAG = 25)) ");
                }


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

            LogWriter.WriteLog("پایان برگشت فروش 2" + DateTime.Now.ToString());

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
            object a = default, fs;
            //   var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM DEED_HED").ToList();
            var HEDRST = dbms.DoGetDataSQL<QUERY_MODEL2>("SELECT     GRD_NUM, GRD_DATE, GRD_ANBAR, GRD_HES, N_S, COMMENT, USER_NAME FROM     dbo.ANBGRD_HEAD WHERE ((GRD_NUM >= " + NUMBER + " AND GRD_NUM <=" + NUMBER2 + " ) )").ToList();
            double progress = 0;
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

            LogWriter.WriteLog("شروع باز سازي از انبار گردانی شماره : " + NUMBER + " تا فاكتور شماره :" + NUMBER2 + DateTime.Now);

            var dbParallelOptions = CL_HESABDARI_AUTO_BAZ.BuildDbAwareParallelOptions(HEDRST.Count);
            ExecuteWithPreferredLoop(0, HEDRST.Count, dbParallelOptions, HEDRST_EOF =>
            {
                if (InternalCalling)
                {
                    auto_run.Dispatcher.Invoke(new Action(() =>
                    {
                        progress++;
                        auto_run.PRGR_C10.Value = progress / ((double)HEDRST.Count) * 100.0;// Update the progress bar
                        auto_run.UpdateOverallProgressBar();

                    }));
                }

                double MABL_CHK, JAMF, JAMCH;
                double? max_ns = null;
                string shart;
                bool NEWR;
                double lastmab;

                var SHSH = Strings.Left(" انبار گرداني شماره " + HEDRST[HEDRST_EOF].GRD_NUM + " از انبار " + HEDRST[HEDRST_EOF].GRD_ANBAR + " مورخ " + Strings.Format(HEDRST[HEDRST_EOF].GRD_DATE, "####/##/##"), 100);
                if (HEDRST[HEDRST_EOF]?.N_S == null)
                {
                    max_ns = Createsanad((long)HEDRST[HEDRST_EOF].GRD_DATE, SHSH, 0, 17, 1, HEDRST[HEDRST_EOF].USER_NAME);
                    shart = "NO_S = 17 AND N_S = " + max_ns;

                    SANAD_NUMBER = max_ns;
                }
                else
                {
                    shart = "NO_S = 17 AND N_S = " + HEDRST[HEDRST_EOF].N_S;
                }
                var SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT * FROM DEED_HED WHERE {shart}").ToList();

                if (SHRST.Count == 0)
                {
                }
                else
                {
                    max_ns = SHRST.FirstOrDefault().N_S;
                    SANAD_NUMBER = max_ns;
                    dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET	DATE_S = {HEDRST[HEDRST_EOF].GRD_DATE} , SHARH_S = N'{SHSH}' , GHATEI = 0 , NO_S = 17 , OKF = 1 , USER_NAME = N'{HEDRST[HEDRST_EOF].USER_NAME}'  WHERE {shart}");
                }
                if (IsNull(HEDRST[HEDRST_EOF].N_S) || HEDRST[HEDRST_EOF].N_S != max_ns)
                {
                    HEDRST[HEDRST_EOF].N_S = max_ns;
                    dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET N_S = {max_ns}  WHERE {shart}");

                    SANAD_NUMBER = max_ns;

                }
                ;
                if (!IsNull(HEDRST[HEDRST_EOF].GRD_HES))
                {
                    double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                    GETTAF3(HEDRST[HEDRST_EOF].GRD_HES, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }
                dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE N_S = " + HEDRST[HEDRST_EOF].N_S);

                //Main Part Start's
                var JST = dbms.DoGetDataSQL<QRE_BAZ_18>("SELECT  dbo.ANBGRD_LST.*, MOG - NUM3 AS EKH FROM dbo.ANBGRD_LST WHERE  (MOG - NUM2 <> 0) AND (MOG - NUM1 <> 0) AND GRD_NUM = " + HEDRST[HEDRST_EOF].GRD_NUM).ToList();
                JAMF = 0d;
                for (int I = 0; I < JST.Count; I++) // while (!JST.EOF())
                {
                    if (isDefaccChecked)
                    {
                        CREATHES(Baseknow.MOGODIA, HEDRST[HEDRST_EOF].GRD_ANBAR, Convert.ToInt64(JST[I].CODE), JST[I].CODE);
                    }
                    //LogWriter.WriteLog("خطا در برگه شماره :" + HEDRST[HEDRST_EOF].GRD_NUM + "اخطار مهم ...! حساب متناظر كالا در انبار وجود ندارد  و من قادر به ايجاد آن نيستم زيرا يك حساب با همين نام ولي با كد ديگر تعريف شده است لطفا با سرپرست سيستم تماس بگيريد.");
                    lastmab = Convert.ToDouble(JST[I].MABL);
                    //lastmab = CL_HESABDARI_AUTO_BAZ.LASTAVRAGE(JST[I].CODE, HEDRST[HEDRST_EOF].GRD_ANBAR, HEDRST[HEDRST_EOF].GRD_DATE);

                    if (Math.Round(lastmab * (double)JST[I].EKH) != 0)
                    {

                        if (JST[I].EKH > 0)
                        {
                            var SHARH = Strings.Left(" انبار گرداني شماره " + HEDRST[HEDRST_EOF].GRD_NUM + " از انبار " + HEDRST[HEDRST_EOF].GRD_ANBAR + " مورخ " + Strings.Format(HEDRST[HEDRST_EOF].GRD_DATE, "####/##/##") + " به مقدار" + JST[I].EKH, 255);

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BES)" +
                                             $"VALUES({max_ns},{Baseknow.MOGODIA},{HEDRST[HEDRST_EOF].GRD_ANBAR},{JST[I].CODE},N'{Baseknow.MOGODIA + "-" + HEDRST[HEDRST_EOF].GRD_ANBAR + "-" + JST[I].CODE}',N'{SHARH}',{Math.Round(lastmab * (double)JST[I].EKH)})");

                        }
                        else
                        {
                            var SHARH = Strings.Left(" انبار گرداني شماره " + HEDRST[HEDRST_EOF].GRD_NUM + " از انبار " + HEDRST[HEDRST_EOF].GRD_ANBAR + " مورخ " + Strings.Format(HEDRST[HEDRST_EOF].GRD_DATE, "####/##/##") + " به مقدار" + JST[I].EKH * -1, 255);

                            dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED) " +
                                            $"VALUES({max_ns},{Baseknow.MOGODIA},{HEDRST[HEDRST_EOF].GRD_ANBAR},{JST[I].CODE},N'{Baseknow.MOGODIA + "-" + HEDRST[HEDRST_EOF].GRD_ANBAR + "-" + JST[I].CODE}',N'{SHARH}',{Math.Round(lastmab * (double)JST[I].EKH * -1)})");

                        }
                    }
                    JST[I].MABL = lastmab;
                    dbms.DoExecuteSQL($@"UPDATE dbo.ANBGRD_LST SET MABL = {JST[I].MABL} WHERE GRD_NUM = {HEDRST[HEDRST_EOF].GRD_NUM} AND CODE = N'{JST[I].CODE}'");
                    JAMF = JAMF + Math.Round(lastmab * (double)JST[I].EKH);
                }

                if (JAMF != 0d)
                {
                    double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;
                    GETTAF3(HEDRST[HEDRST_EOF].GRD_HES, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    string HES_T2T = (Convert.ToDouble(CTAF2) == 0 || CTAF2 is null) ? "NULL" : CTAF2.ToString();
                    string HES_T3T = (Convert.ToDouble(CTAF3) == 0 || CTAF3 is null) ? "NULL" : CTAF3.ToString();
                    string HES_T4T = (Convert.ToDouble(CTAF4) == 0 || CTAF4 is null) ? "NULL" : CTAF4.ToString();
                    var SHARH = Strings.Left("انبار گرداني شماره " + HEDRST[HEDRST_EOF].GRD_NUM + " از انبار " + HEDRST[HEDRST_EOF].GRD_ANBAR + " مورخ " + Strings.Format(HEDRST[HEDRST_EOF].GRD_DATE, "####/##/##"), 255);
                    if (JAMF > 0d)
                    {
                        //SDRST.Fields("BED") = JAMF;
                        //dbms.DoExecuteSQL($"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, hes, SHARH, BED)" +
                        //                  $"VALUES({max_ns},{CKOL},{CMOIN},{CTAF},N'{Baseknow.MOGODIA + "-" + HEDRST[HEDRST_EOF].GRD_ANBAR + "-" + JST[I].CODE}',N'{SHARH}',{Math.Round(lastmab * (double)JST[I].EKH)})");
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T , HES_T2 , HES_T3 , HES_T4 , HES , SHARH , BED)
                                             VALUES					({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{HEDRST[HEDRST_EOF].GRD_HES}',N'{SHARH}',{JAMF})");
                    }
                    else
                    {
                        //SDRST.Fields("BES") = JAMF * -1;
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T , HES_T2 , HES_T3 , HES_T4 , HES , SHARH , BES)
                                             VALUES					({max_ns},{CKOL},{CMOIN},{CTAF},{HES_T2T},{HES_T3T},{HES_T4T},N'{HEDRST[HEDRST_EOF].GRD_HES}',N'{SHARH}',{JAMF * -1})");

                    }
                    //SDRST.update();
                }
                JAMF = 0d;

                dbms.DoExecuteSQL($"UPDATE dbo.ANBGRD_HEAD SET N_S = {HEDRST[HEDRST_EOF]?.N_S} WHERE GRD_NUM = {HEDRST[HEDRST_EOF]?.GRD_NUM}");

            });
            LogWriter.WriteLog("پایان انبار گردانی" + DateTime.Now.ToString());

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

