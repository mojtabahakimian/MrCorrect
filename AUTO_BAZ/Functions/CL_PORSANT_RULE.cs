using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AUTO_BAZ.Functions
{
    /// <summary>
    /// قاعده‌ی واحد پورسانت ویزیتور فاکتور فروش (TAG = 2).
    ///
    /// چرا این کلاس هست: تا امروز سه پیاده‌سازی جدا از یک قاعده وجود داشت —
    /// GENSANADFROOSH (که سند می‌زند و مبلغ را در VISITOR_DTL می‌نویسد)،
    /// پروسیجر dbo.RecalcVisitorPorsant_ByDarsad (که پنجره‌ی کنترل پورسانت با آن
    /// «آنچه باید باشد» را می‌ساخت) و dbo.CalculateVisitorPorsant (که فرم فاکتور
    /// موقع ذخیره صدا می‌زند). این سه با هم اختلاف داشتند و نتیجه‌اش این بود که
    /// «اصلاح پورسانت و صدور مجدد سند» مبلغ را درست می‌کرد، بعد بلافاصله خودِ
    /// صدور سند مبلغ دیگری می‌نوشت و همان فاکتورها دوباره در فهرست مغایرت‌ها
    /// ظاهر می‌شدند — حلقه‌ای که هیچ‌وقت تمام نمی‌شد.
    ///
    /// اختلاف‌هایی که «حلقه» را می‌سازند (هر سه در کد اثبات‌شده‌اند؛ اینکه روی
    /// داده‌ی یک شرکت کدامشان فعال است، فقط با اجرای گزارش روی همان دیتابیس
    /// معلوم می‌شود):
    ///   ۱. سربرگ مبنا: GENSANADFROOSH تخفیف/ارزش‌افزوده را از سطر «فاکتور»
    ///      (HEAD_LST با TAG = 13) می‌خواند، پروسیجر از سطر «حواله» (TAG = 2).
    ///      این دو سطر را فرم فاکتور هم‌زمان می‌نویسد، ولی هر مسیر دیگری که فقط
    ///      یکی از آن دو را به‌روز کند، این دو عدد را از هم جدا می‌کند.
    ///   ۲. گِردکردن: Math.Round پیش‌فرض C# «به نزدیک‌ترین زوج» گِرد می‌کند و
    ///      ROUND در T-SQL «نیم به بالا»؛ روی مقدارهای دقیقاً نیم، یک ریال اختلاف
    ///      می‌شود و همان یک ریال سطر را برای همیشه مغایر نگه می‌دارد.
    ///   ۳. الگوی تکراری/بی‌نرخ: GENSANADFROOSH سطر تکراری در
    ///      VISITORS_PORSANT_KALA را «بدون نرخ» حساب می‌کند، پروسیجر (INNER JOIN)
    ///      آن را دوباره جمع می‌زد.
    ///
    /// و یک نویزِ جداگانه (نه سازنده‌ی حلقه، ولی سطر بی‌فایده در فهرست): سطرهای
    /// پورسانتی که GENSANADFROOSH اصلاً به آنها نمی‌رسد — فاکتور سطر TAG = 13
    /// ندارد، یا جمع اقلامش صفر است، یا تاریخش نامعتبر است. سند حسابداریِ این
    /// سطرها هرگز پورسانتشان را نمی‌نویسد، پس فهرست‌کردنشان به‌عنوان مغایرتِ
    /// «قابل اصلاح» فقط کاربر را سردرگم می‌کرد.
    ///
    /// از این پس مرجع همین کلاس است: GENSANADFROOSH از توابع محاسبه‌ی همین‌جا
    /// استفاده می‌کند و پنجره‌ی کنترل هم «آنچه باید باشد» را از همین‌جا می‌گیرد.
    /// هر تغییری در قاعده باید فقط اینجا انجام شود.
    ///
    /// ⚠️ باقی‌مانده (خارج از این مخزن): پروسیجر dbo.CalculateVisitorPorsant که فرم
    /// فاکتور فروش هنگام ذخیره صدا می‌زند در مخزن ScriptSqly است و سه اختلاف با این
    /// قاعده دارد که فقط در همان مخزن قابل رفع است:
    ///   • جمع را یک‌جا در انتها گِرد می‌کند، نه ردیف‌به‌ردیف (اختلاف چند ریال).
    ///   • الگو را از SALA_DTL.PORID (الگوی پیش‌فرضِ خودِ ویزیتور) می‌گیرد و همان را
    ///     روی VISITOR_DTL.PORID می‌نویسد؛ یعنی الگویی که کاربر روی خودِ فاکتور
    ///     انتخاب کرده نادیده گرفته و بازنویسی می‌شود.
    ///   • با LEFT JOIN به VISITORS_PORSANT_KALA، سطرِ تکراریِ یک کالا در الگو را
    ///     دوبار می‌شمارد (اینجا سطر تکراریِ هم‌نرخ یک‌بار حساب می‌شود و تکراریِ
    ///     ناهم‌نرخ اصلاً حساب نمی‌شود).
    ///
    /// نکته‌ی پرتکرار در پشتیبانی: «الگوی ۲٪ دادیم ولی مبلغ خیلی کمتر شد». علتش
    /// همیشه همین است که بخشی از اقلام فاکتور در الگو نرخ ندارند و سهمی نمی‌گیرند.
    /// برای همین GetPatternBreakdown اضافه شد تا فرم فاکتور و پنجره‌ی کنترل بتوانند
    /// دقیقاً بگویند کدام اقلام نرخ ندارند و چقدر از مبنا را می‌بلعند.
    ///
    /// کالای جایزه: همان پروسیجر ردیف‌های جایزه (INVO_LST.JAY &lt;&gt; 0) را با شرط
    /// اضافه‌ی خودش کنار می‌گذارد و اینجا چنین شرطی نیست — ولی این اختلاف بی‌اثر
    /// است، چون ردیف جایزه یا با MABL_K = 0 ثبت می‌شود (مسیر حواله) یا با MABL = 1
    /// و N_KOL = 100 یعنی صددرصد تخفیف (مسیر فاکتور)، و در هر دو حالت
    /// MABL_K - N_MOIN صفر است. پس کالای جایزه در مسیر الگو دقیقاً صفر پورسانت
    /// می‌گیرد و شرط JAY لازم نیست. (تنها اثر باقی‌مانده: در مسیرِ بدون‌الگو مبنا
    /// SUM(MABL_K) خام است، پس ردیف جایزه‌ی مسیر فاکتور به اندازه‌ی «تعداد × ۱ ریال»
    /// به مبنا اضافه می‌کند — چند ریال، نه یک تصمیم کاری.)
    /// </summary>
    public static class CL_PORSANT_RULE
    {
        private static readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        /// <summary>اختلاف کمتر از یک ریال، مغایرت نیست.</summary>
        public const double TOLERANCE = 0.5;

        /// <summary>
        /// گِردکردن مبلغ ریالی. عمداً AwayFromZero است تا با ROUND(x, 0) در
        /// SQL Server (که فرم فاکتور و گزارش‌ها از آن استفاده می‌کنند) یکی باشد؛
        /// Math.Round پیش‌فرض C# روی مقدارهای دقیقاً نیم، جواب دیگری می‌دهد.
        /// </summary>
        public static double RoundMoney(double value) => Math.Round(value, MidpointRounding.AwayFromZero);

        /// <summary>
        /// مبنای پورسانتِ سطرهای بدون‌الگو: جمع کل فاکتور منهای تخفیف، و اگر
        /// گزینه ۶۲ سازمان «۵» باشد به‌علاوه‌ی ارزش افزوده.
        /// </summary>
        public static double PorsantBase(double jamf, double? takhfif, double? mbaa)
            => jamf - (takhfif ?? 0) + (Baseknow.PorsantBaseIncludesVat ? (mbaa ?? 0) : 0);

        /// <summary>مبلغ پورسانت سطر بدون‌الگو = درصد × مبنای فاکتور.</summary>
        public static double ByDarsad(double porsantBase, double? darsad)
            => RoundMoney(porsantBase * (darsad ?? 0) / 100);

        /// <summary>
        /// سهم یک قلم کالا از پورسانتِ سطرِ دارای الگو. کالای بدون نرخ در الگو
        /// عمداً سهمی نمی‌گیرد (این طراحی است، نه باگ).
        /// </summary>
        public static double PatternLineShare(double? mablk, double? rate)
            => rate.HasValue ? RoundMoney((mablk ?? 0) * rate.Value / 100) : 0;

        /// <summary>
        /// یک سطر گزارش کنترل: وضعیت فعلیِ پورسانتِ یک سطر ویزیتور در برابر
        /// آنچه صدور سند خواهد نوشت.
        /// </summary>
        public class PorsantAuditRow
        {
            public long? ID { get; set; }
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public string? CUST_NO { get; set; }
            public long? DATE_N { get; set; }
            public string? CUST_NAME { get; set; }
            public int? PORID { get; set; }
            public double? DARSAD { get; set; }
            public double? OLD_PURSANT { get; set; }

            /// <summary>جمع کل اقلام فاکتور (SUM(INVO_LST.MABL_K) با TAG = 2).</summary>
            public double? JAMF { get; set; }
            public double? TAKHFIF { get; set; }
            public double? MBAA { get; set; }

            /// <summary>مبلغ محاسبه‌شده از روی الگو؛ NULL یعنی هیچ کالای این فاکتور در الگو نرخ ندارد.</summary>
            public double? PATTERN_AMOUNT { get; set; }

            /// <summary>جمع خالصِ (MABL_K − N_MOIN) کالاهایی که در الگو نرخ دارند.</summary>
            public double? PATTERN_MBK { get; set; }

            /// <summary>تعداد اقلام این فاکتور که در الگوی همین ویزیتور نرخ ندارند.</summary>
            public int NO_RATE_ITEMS { get; set; }

            /// <summary>جمع خالصِ اقلامی که در الگو نرخ ندارند و هیچ پورسانتی نمی‌دهند.</summary>
            public double NO_RATE_BASE { get; set; }

            /// <summary>تعداد سطرهای همین ویزیتور روی همین فاکتور؛ بیش از یک یعنی داده‌ی ناهنجار.</summary>
            public int ROWS_PER_VISITOR { get; set; } = 1;

            /// <summary>مقداری که صدور سند حسابداری خواهد نوشت (مقدار درست).</summary>
            public double NEW_PURSANT { get; set; }

            /// <summary>آیا این سطر با «اصلاح و صدور مجدد» قابل درست‌شدن است.</summary>
            public bool CAN_FIX { get; set; } = true;

            public string? WARNING { get; set; }

            public bool HAS_PATTERN => PORID.HasValue;
            public string HAS_PATTERN_TEXT => HAS_PATTERN ? "دارد" : "ندارد";
            public double NET_BASE => PorsantBase(JAMF ?? 0, TAKHFIF, MBAA);
            public double DIFF => NEW_PURSANT - (OLD_PURSANT ?? 0);
            public string STATUS_TEXT => CAN_FIX ? "قابل اصلاح" : "نیاز به بررسی دستی";

            /// <summary>
            /// «چند درصدِ اقلامِ این فاکتور اصلاً در الگو نرخ دارند». هر چه این عدد از ۱۰۰
            /// کمتر باشد، مبلغِ الگو همان‌قدر از درصدِ اسمیِ الگو کمتر درمی‌آید — و این
            /// تنها توضیحِ «چرا پورسانت الگو کمتر از انتظار است».
            /// </summary>
            public double PATTERN_COVERAGE
            {
                get
                {
                    if (!HAS_PATTERN) { return 100; }
                    double covered = PATTERN_MBK ?? 0;
                    double all = covered + NO_RATE_BASE;
                    return all > 0 ? covered / all * 100 : 0;
                }
            }

            public string COVERAGE_TEXT => HAS_PATTERN
                ? (NO_RATE_ITEMS == 0
                    ? "همه اقلام"
                    : $"{PATTERN_COVERAGE:0.#}% ({NO_RATE_ITEMS} قلم بدون نرخ)")
                : "-";
        }

        /// <summary>نتیجه‌ی اصلاح یک فاکتور.</summary>
        public class PorsantFixOutcome
        {
            public double NUMBER { get; set; }
            public List<long> UpdatedIds { get; } = new List<long>();
            public int RowsUpdated { get; set; }
            public bool DocumentReissued { get; set; }

            /// <summary>پس از صدور مجدد سند، دوباره خوانده شد و مغایرتی نماند.</summary>
            public bool Verified { get; set; }
            public string? Error { get; set; }

            public bool Ok => RowsUpdated > 0 && DocumentReissued && Verified && string.IsNullOrEmpty(Error);
        }

        /// <summary>نتیجه‌ی کل عملیات اصلاح، به‌همراه فهرست تازه‌ی مغایرت‌ها از دیتابیس.</summary>
        public class PorsantFixResult
        {
            public List<PorsantFixOutcome> Outcomes { get; } = new List<PorsantFixOutcome>();
            public List<PorsantAuditRow> Remaining { get; set; } = new List<PorsantAuditRow>();

            public int SucceededInvoices => Outcomes.Count(o => o.Ok);
            public IEnumerable<PorsantFixOutcome> Failed => Outcomes.Where(o => !o.Ok);
        }

        /// <summary>
        /// این کوئری عمداً فقط «مواد خام» را برمی‌گرداند؛ خودِ مبلغِ درست در C# و با
        /// همان توابعی ساخته می‌شود که GENSANADFROOSH استفاده می‌کند.
        ///
        /// شرط‌های WHERE دقیقاً همان چیزی است که GENSANADFROOSH پردازش می‌کند:
        /// سربرگ فاکتور (TAG = 13) موجود باشد، تاریخش معتبر باشد (قید CK_DEED_HED)
        /// و جمع فاکتور مثبت باشد (شرط if (JAMF &gt; 0) در همان تابع). سطری که این
        /// شرط‌ها را ندارد هرگز با صدور سند عوض نمی‌شود، پس نمایشش به‌عنوان
        /// «مغایرتِ قابل اصلاح» بی‌جهت کاربر را دنبال نخود سیاه می‌فرستاد.
        ///
        /// تنها جای دیگری که قاعده تکرار شده PATTERN_AMOUNT است؛ ROUND(...) در
        /// T-SQL همان کاری را می‌کند که RoundMoney اینجا انجام می‌دهد (نیم به بالا)
        /// و RATE هم مثل GENSANADFROOSH سطر تکراری یا بی‌نرخ را «بدون نرخ» می‌گیرد.
        /// </summary>
        private const string AUDIT_SQL = @"
-- TAG = 13 سربرگ «فاکتور فروش» است (تخفیف و ارزش افزوده روی همین سطر است) و
-- TAG = 2 «حواله فروش»؛ اقلام کالا و سطرهای پورسانت با تگ ۲ ثبت می‌شوند.
;WITH BASE AS
(
    SELECT NUMBER, SUM(ISNULL(MABL_K, 0)) AS JAMF
    FROM dbo.INVO_LST
    WHERE TAG = 2 AND (@pNumber IS NULL OR NUMBER = @pNumber)
    GROUP BY NUMBER
),
RATE AS
(
    -- سطر تکراری فقط وقتی «بی‌نرخ» شمرده می‌شود که نرخ‌هایش با هم نخوانند؛ دو سطر با
    -- نرخ یکسان (که فرمِ الگو خودش می‌ساخت) همان نرخ را دارد و پیش از این کالا را
    -- بی‌صدا از پورسانت حذف می‌کرد.
    SELECT PORID, CODE, MIN(PORSANT) AS PORSANT
    FROM dbo.VISITORS_PORSANT_KALA
    GROUP BY PORID, CODE
    -- COUNT(PORSANT) ستون‌های NULL را نمی‌شمارد؛ برابری‌اش با COUNT(*) یعنی هیچ سطری
    -- بی‌نرخ نیست، و MIN = MAX یعنی همه یک نرخ دارند. (ISNULL با عددِ نگهبان اینجا کار
    -- نمی‌کند: اگر خودِ نرخ همان عدد باشد، سطرِ NULL هم «هم‌نرخ» شمرده می‌شد.)
    HAVING COUNT(PORSANT) = COUNT(*) AND MIN(PORSANT) = MAX(PORSANT)
),
PAT AS
(
    SELECT vd.ID,
           SUM(ROUND((ISNULL(il.MABL_K, 0) - ISNULL(il.N_MOIN, 0)) * r.PORSANT / 100.0, 0)) AS PATTERN_AMOUNT,
           SUM(ISNULL(il.MABL_K, 0) - ISNULL(il.N_MOIN, 0)) AS PATTERN_MBK
    FROM dbo.VISITOR_DTL AS vd
        INNER JOIN dbo.INVO_LST AS il ON il.NUMBER = vd.NUMBER AND il.TAG = vd.TAG
        INNER JOIN RATE AS r ON r.PORID = vd.PORID AND r.CODE = il.CODE
    WHERE vd.TAG = 2
          AND vd.PORID IS NOT NULL
          AND ISNULL(vd.STAT, 0) = 0
          AND (@pNumber IS NULL OR vd.NUMBER = @pNumber)
    GROUP BY vd.ID
),
-- «چند قلم از این فاکتور اصلاً در الگو نرخ ندارند» و «جمع خالصِ همان اقلام»؛ همین دو
-- عدد توضیح می‌دهد چرا مبلغِ الگو از درصدِ اسمیِ الگو کمتر درمی‌آید.
NORATE AS
(
    SELECT vd.ID,
           COUNT(*) AS NO_RATE_ITEMS,
           SUM(ISNULL(il.MABL_K, 0) - ISNULL(il.N_MOIN, 0)) AS NO_RATE_BASE
    FROM dbo.VISITOR_DTL AS vd
        INNER JOIN dbo.INVO_LST AS il ON il.NUMBER = vd.NUMBER AND il.TAG = vd.TAG
        LEFT JOIN RATE AS r ON r.PORID = vd.PORID AND r.CODE = il.CODE
    WHERE vd.TAG = 2
          AND vd.PORID IS NOT NULL
          AND ISNULL(vd.STAT, 0) = 0
          AND r.CODE IS NULL
          -- ردیف‌های با خالصِ صفر (کالای جایزه) پورسانت نمی‌گیرند و «قلم بدون نرخ» هم نیستند
          AND (ISNULL(il.MABL_K, 0) - ISNULL(il.N_MOIN, 0)) <> 0
          AND (@pNumber IS NULL OR vd.NUMBER = @pNumber)
    GROUP BY vd.ID
),
DUP AS
(
    -- عمداً STAT فیلتر نمی‌شود: صدور سند مبلغ را با WHERE NUMBER/CUST_NO/TAG می‌نویسد،
    -- یعنی سطر «مبلغ ثابت» همان ویزیتور را هم بازنویسی می‌کند. پس هر تعداد سطر بیش از
    -- یکی برای یک ویزیتور روی یک فاکتور، اصلاح خودکار را ناپایدار می‌کند.
    SELECT NUMBER, CUST_NO, COUNT(*) AS ROWS_PER_VISITOR
    FROM dbo.VISITOR_DTL
    WHERE TAG = 2 AND (@pNumber IS NULL OR NUMBER = @pNumber)
    GROUP BY NUMBER, CUST_NO
)
SELECT vd.ID,
       vd.NUMBER,
       vd.TAG,
       vd.CUST_NO,
       h.DATE_N,
       chv.NAME AS CUST_NAME,
       vd.PORID,
       vd.DARSAD,
       ISNULL(vd.PURSANT, 0) AS OLD_PURSANT,
       b.JAMF,
       h.TAKHFIF,
       h.MBAA,
       p.PATTERN_AMOUNT,
       p.PATTERN_MBK,
       ISNULL(nr.NO_RATE_ITEMS, 0) AS NO_RATE_ITEMS,
       ISNULL(nr.NO_RATE_BASE, 0) AS NO_RATE_BASE,
       ISNULL(d.ROWS_PER_VISITOR, 1) AS ROWS_PER_VISITOR
FROM dbo.VISITOR_DTL AS vd
    INNER JOIN dbo.HEAD_LST AS h ON h.NUMBER = vd.NUMBER AND h.TAG = 13
    INNER JOIN BASE AS b ON b.NUMBER = vd.NUMBER
    LEFT JOIN PAT AS p ON p.ID = vd.ID
    LEFT JOIN NORATE AS nr ON nr.ID = vd.ID
    LEFT JOIN DUP AS d ON d.NUMBER = vd.NUMBER AND d.CUST_NO = vd.CUST_NO
    -- نامِ ویزیتور از یک زیرپرسشِ گروه‌بندی‌شده می‌آید، نه JOIN مستقیم: در سراسر برنامه
    -- جستجوی CUST_HESAB بر اساس hes با TOP 1 انجام می‌شود، یعنی یکتا بودنِ hes تضمین
    -- نشده و JOIN مستقیم می‌توانست سطرهای گزارش را تکراری کند (و بعد همان سطر دوبار
    -- در عملیات اصلاح بیاید).
    LEFT JOIN (SELECT hes, MIN(NAME) AS NAME FROM dbo.CUST_HESAB GROUP BY hes) AS chv
        ON chv.hes = vd.CUST_NO
WHERE vd.TAG = 2
      AND ISNULL(vd.STAT, 0) = 0
      AND b.JAMF > 0
      AND ISNULL(h.DATE_N, 0) >= 10101
      AND (@pNumber IS NULL OR vd.NUMBER = @pNumber)
      AND (@pFrom IS NULL OR h.DATE_N >= @pFrom)
      AND (@pTo IS NULL OR h.DATE_N <= @pTo)
ORDER BY h.DATE_N, vd.NUMBER";

        /// <summary>یک قلم کالای فاکتور در محاسبه‌ی الگوی پورسانت.</summary>
        public class PatternItemLine
        {
            public string? CODE { get; set; }
            public string? NAME { get; set; }

            /// <summary>مبنای پورسانتِ این قلم = MABL_K − N_MOIN (خالصِ پس از تخفیف ردیف).</summary>
            public double NET { get; set; }

            /// <summary>نرخ این کالا در الگو؛ NULL یعنی در الگو نرخ ندارد.</summary>
            public double? RATE { get; set; }

            /// <summary>تعداد سطرهای این کالا در الگو (بیش از یکی یعنی داده‌ی تکراری).</summary>
            public int RATE_ROWS { get; set; }

            /// <summary>سطرهای تکراریِ این کالا در الگو نرخ‌های متفاوت دارند.</summary>
            public bool CONFLICT { get; set; }

            public bool HAS_RATE => RATE.HasValue && !CONFLICT;
            public double SHARE => HAS_RATE ? PatternLineShare(NET, RATE) : 0;
        }

        /// <summary>ریز محاسبه‌ی پورسانتِ یک سطر دارای الگو، برای پاسخ به «این مبلغ از کجا آمد؟».</summary>
        public class PatternBreakdown
        {
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public int PORID { get; set; }
            public List<PatternItemLine> Lines { get; } = new List<PatternItemLine>();

            /// <summary>مبلغ پورسانتِ الگو = جمع سهم کالاهایی که در الگو نرخ دارند.</summary>
            public double Amount => Lines.Sum(l => l.SHARE);

            /// <summary>جمع خالصِ کالاهایی که نرخ دارند.</summary>
            public double CoveredBase => Lines.Where(l => l.HAS_RATE).Sum(l => l.NET);

            /// <summary>جمع خالصِ همه‌ی اقلام فاکتور.</summary>
            public double TotalBase => Lines.Sum(l => l.NET);

            public List<PatternItemLine> MissingLines => Lines.Where(l => !l.HAS_RATE).ToList();
            public int ConflictCount => Lines.Count(l => l.CONFLICT);

            /// <summary>درصدِ مؤثر روی مبنای کل فاکتور — همان عددی که در ستون «درصد» می‌نشیند.</summary>
            public double EffectiveDarsad(double porsantBase)
                => porsantBase != 0 ? Amount / porsantBase * 100 : 0;

            /// <summary>متن قابل نمایش برای کاربر: مبلغ، پوشش الگو و اقلام بدون نرخ.</summary>
            public string BuildReport(double porsantBase)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"محاسبه‌ی پورسانت از روی الگوی شماره {PORID} — فاکتور {NUMBER:0}");
                sb.AppendLine($"مبنای کل فاکتور: {porsantBase:N0}");
                sb.AppendLine($"جمع خالص اقلام: {TotalBase:N0} — از این مقدار {CoveredBase:N0} در الگو نرخ دارد.");
                sb.AppendLine($"مبلغ پورسانت الگو: {Amount:N0}  (معادل {EffectiveDarsad(porsantBase):0.###}% از مبنای فاکتور)");

                var missing = MissingLines.Where(l => l.NET != 0).ToList();
                if (missing.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{missing.Count} قلم زیر در این الگو نرخ ندارند و هیچ پورسانتی نمی‌دهند؛");
                    sb.AppendLine("تا وقتی نرخشان در الگو تعریف نشود، مبلغ پورسانت از درصد اسمی الگو کمتر می‌ماند:");
                    foreach (var line in missing.OrderByDescending(l => l.NET).Take(25))
                    {
                        var reason = line.CONFLICT
                            ? $"در الگو {line.RATE_ROWS} بار با نرخ‌های متفاوت ثبت شده"
                            : "در الگو نرخ ندارد";
                        sb.AppendLine($"  • {line.NAME ?? line.CODE} (کد {line.CODE}) — مبلغ {line.NET:N0} — {reason}");
                    }

                    if (missing.Count > 25) { sb.AppendLine($"  ... و {missing.Count - 25} قلم دیگر."); }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// ریز محاسبه‌ی الگوی پورسانتِ یک فاکتور برای یک الگوی مشخص. همان قاعده‌ای که
        /// صدور سند استفاده می‌کند، فقط این‌بار با جزئیاتِ قابل نمایش به کاربر.
        /// </summary>
        public static PatternBreakdown GetPatternBreakdown(double number, double tag, int porid)
        {
            const string sql = @"
SELECT il.CODE,
       sd.NAME,
       ISNULL(il.MABL_K, 0) - ISNULL(il.N_MOIN, 0) AS NET,
       r.PORSANT AS RATE,
       ISNULL(r.RATE_ROWS, 0) AS RATE_ROWS,
       ISNULL(r.CONFLICT, CAST(0 AS BIT)) AS CONFLICT
FROM dbo.INVO_LST AS il
    LEFT JOIN dbo.STUF_DEF AS sd ON sd.CODE = il.CODE
    LEFT JOIN
    (
        SELECT CODE,
               MIN(PORSANT) AS PORSANT,
               COUNT(*) AS RATE_ROWS,
               CAST(CASE WHEN COUNT(PORSANT) <> COUNT(*) OR MIN(PORSANT) <> MAX(PORSANT) THEN 1 ELSE 0 END AS BIT) AS CONFLICT
        FROM dbo.VISITORS_PORSANT_KALA
        WHERE PORID = @pPorid
        GROUP BY CODE
    ) AS r ON r.CODE = il.CODE
WHERE il.NUMBER = @pNumber AND il.TAG = @pTag
ORDER BY il.RADIF";

            var result = new PatternBreakdown { NUMBER = number, TAG = tag, PORID = porid };

            foreach (var line in dbms.DoGetDataSQL<PatternItemLine>(sql, new { pNumber = number, pTag = tag, pPorid = porid }))
            {
                result.Lines.Add(line);
            }

            return result;
        }

        /// <summary>
        /// فهرست سطرهای پورسانتی که با قاعده نمی‌خوانند. فقط می‌خواند؛ چیزی را تغییر نمی‌دهد.
        /// </summary>
        /// <param name="number">شماره فاکتور؛ null یعنی همه.</param>
        /// <param name="fromDate">تاریخ شمسی ۸ رقمی شروع؛ null یعنی بدون محدودیت.</param>
        /// <param name="toDate">تاریخ شمسی ۸ رقمی پایان؛ null یعنی بدون محدودیت.</param>
        public static List<PorsantAuditRow> Audit(double? number = null, long? fromDate = null, long? toDate = null)
        {
            return ReadRows(number, fromDate, toDate)
                .Where(row => Math.Abs(row.DIFF) >= TOLERANCE) // اختلاف زیر یک ریال مغایرت نیست
                .ToList();
        }

        /// <summary>
        /// همه‌ی سطرهای پورسانتِ یک فاکتور، چه با قاعده بخوانند چه نخوانند. برای پاسخ به
        /// «چرا پورسانت این فاکتور این‌قدر شد؟» لازم است؛ فهرست مغایرت‌ها این سطرها را
        /// نشان نمی‌دهد، چون مبلغِ ذخیره‌شده دقیقاً همان چیزی است که قاعده می‌گوید — و
        /// پرسشِ کاربر درباره‌ی خودِ قاعده است، نه مغایرت.
        /// </summary>
        public static List<PorsantAuditRow> InspectInvoice(double number) => ReadRows(number, null, null);

        /// <summary>خواندن خام + محاسبه‌ی «مقدار درست» و هشدارها، بدون فیلترِ مغایرت.</summary>
        private static List<PorsantAuditRow> ReadRows(double? number, long? fromDate, long? toDate)
        {
            var rows = dbms.DoGetDataSQL<PorsantAuditRow>(
                AUDIT_SQL,
                new { pNumber = number, pFrom = fromDate, pTo = toDate }).ToList();

            var result = new List<PorsantAuditRow>();

            foreach (var row in rows)
            {
                // مقدار درست، با همان توابعی که صدور سند استفاده می‌کند
                row.NEW_PURSANT = row.HAS_PATTERN
                    ? (row.PATTERN_AMOUNT ?? 0)
                    : ByDarsad(PorsantBase(row.JAMF ?? 0, row.TAKHFIF, row.MBAA), row.DARSAD);

                if (row.ID is null)
                {
                    row.CAN_FIX = false;
                    row.WARNING = "این سطر شناسه (ID) ندارد و به‌صورت خودکار قابل اصلاح نیست.";
                }
                else if (row.ROWS_PER_VISITOR > 1)
                {
                    // صدور سند مبلغ را با WHERE NUMBER/CUST_NO/TAG می‌نویسد، یعنی هر دو
                    // سطر را با یک مبلغ پر می‌کند؛ اصلاح خودکار این سطرها پایدار نمی‌ماند.
                    row.CAN_FIX = false;
                    row.WARNING = "برای این ویزیتور روی این فاکتور بیش از یک سطر پورسانت ثبت شده؛ ابتدا سطر تکراری را در خود فاکتور اصلاح کنید.";
                }
                else if (row.HAS_PATTERN && row.PATTERN_AMOUNT is null)
                {
                    row.WARNING = "این ویزیتور برای هیچ‌کدام از کالاهای این فاکتور در این الگو نرخ ندارد؛ پورسانت صفر می‌شود.";
                }
                else if (row.HAS_PATTERN && row.NO_RATE_ITEMS > 0)
                {
                    row.WARNING = $"{row.NO_RATE_ITEMS} قلم از این فاکتور (جمع {row.NO_RATE_BASE:N0}) در این الگو نرخ ندارد و پورسانتی نمی‌دهد؛ " +
                                  "به همین دلیل مبلغِ الگو از درصدِ اسمیِ الگو کمتر است. برای اصلاح، نرخ این کالاها را در الگو تعریف کنید.";
                }

                result.Add(row);
            }

            return result;
        }

        /// <summary>
        /// اصلاح پایدارِ مبلغ پورسانت و صدور مجدد سند حسابداری.
        ///
        /// ترتیب کار برای هر فاکتور: نوشتن مبلغ درست در یک تراکنش → صدور مجدد سند
        /// (که خودش با همین قاعده دوباره حساب و ثبت می‌کند، پس مقدار را برنمی‌گرداند)
        /// → و در پایان یک بازخوانی از دیتابیس برای «اثبات» اینکه مغایرت واقعاً رفته است.
        /// پیام موفقیت باید فقط بر اساس همین بازخوانی داده شود، نه بر اساس تعداد
        /// سطرهایی که قرار بود اصلاح شوند.
        /// </summary>
        public static PorsantFixResult FixAndReissue(IEnumerable<PorsantAuditRow> rows)
        {
            var result = new PorsantFixResult();

            var fixable = (rows ?? Enumerable.Empty<PorsantAuditRow>())
                .Where(r => r != null && r.CAN_FIX && r.ID.HasValue && r.NUMBER.HasValue)
                .ToList();

            if (fixable.Count == 0)
            {
                result.Remaining = Audit();
                return result;
            }

            foreach (var group in fixable.GroupBy(r => r.NUMBER.Value))
            {
                var outcome = new PorsantFixOutcome { NUMBER = group.Key };
                result.Outcomes.Add(outcome);

                try
                {
                    using (var tm = new TransactionManagement(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        try
                        {
                            int affected = 0;
                            foreach (var row in group)
                            {
                                affected += tm.ExecuteSqlCommandCtc(
                                    "UPDATE dbo.VISITOR_DTL SET PURSANT = @PURSANT WHERE ID = @ID AND ISNULL(STAT, 0) = 0",
                                    new { PURSANT = row.NEW_PURSANT, ID = row.ID.Value });

                                outcome.UpdatedIds.Add(row.ID.Value);
                            }

                            if (affected != group.Count())
                            {
                                // سطری که انتظارش را داشتیم دیگر نیست (پاک/ثابت شده)؛ چیزی نصفه‌نیمه نمی‌نویسیم
                                tm.DoRollback();
                                outcome.RowsUpdated = 0;
                                outcome.UpdatedIds.Clear();
                                outcome.Error = $"سطرهای پورسانت این فاکتور تغییر کرده‌اند ({affected} از {group.Count()} سطر به‌روزرسانی شد)؛ عملیات این فاکتور برگشت خورد.";
                                continue;
                            }

                            outcome.RowsUpdated = affected;
                            tm.DoCommit();
                        }
                        catch
                        {
                            tm.DoRollback();
                            throw;
                        }
                    }

                    // سند حسابداری همین فاکتور دوباره صادر می‌شود تا مبلغ پورسانتِ سند با
                    // مبلغ تازه بخواند. چون قاعده‌ی محاسبه یکی است، همین مقدار را می‌نویسد.
                    var invoiceNumber = Convert.ToInt64(group.Key);
                    var (_, isSuccessfully) = CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(invoiceNumber, invoiceNumber, false);

                    outcome.DocumentReissued = isSuccessfully;
                    if (!isSuccessfully)
                    {
                        outcome.Error = "صدور مجدد سند حسابداری این فاکتور ناموفق بود (جزئیات در فایل لاگ).";
                    }
                }
                catch (Exception ex)
                {
                    outcome.Error = ex.Message;
                }
            }

            // بازخوانی نهایی از دیتابیس: تنها معیار موفقیت همین است
            result.Remaining = Audit();

            var stillMismatched = new HashSet<long>(
                result.Remaining.Where(r => r.ID.HasValue).Select(r => r.ID.Value));

            foreach (var outcome in result.Outcomes)
            {
                outcome.Verified = outcome.RowsUpdated > 0
                                   && !outcome.UpdatedIds.Any(id => stillMismatched.Contains(id));
            }

            return result;
        }
    }
}
