/* ============================================================================
   جمع‌آوری داده‌ی نمونه برای راستی‌آزمایی تغییرات

   همه‌ی کوئری‌ها فقط-خواندنی هستند و هیچ چیزی را تغییر نمی‌دهند.
   هر بخش می‌گوید «چرا لازم است» — اگر بخشی برایتان حساس است، ردش کنید
   و بگویید کدام را ندادید.

   خروجی هر بخش را جدا کپی کنید (ترجیحاً به‌صورت JSON یا متن جدولی).
   ============================================================================ */


/* ===========================================================================
   بخش ۱ — پیکربندی سازمان   ⭐ مهم‌ترین
   ---------------------------------------------------------------------------
   چرا لازم است: کد بازسازی فروش شاخه‌های زیادی دارد که با این تنظیمات
   انتخاب می‌شوند. تا ندانم کدام‌شان روشن است، نمی‌دانم کدام مسیر کد اصلاً
   اجرا می‌شود و باید کجا را دقیق‌تر بررسی کنم.

   - SNDKH  : حالت «سند روزانه» — تعیین می‌کند رقابتی که رفع کردم فعال است یا نه
   - SANAT  : حالت صنعتی — تعیین می‌کند بهای تمام‌شده محاسبه می‌شود یا نه
   - OPTIONSS: کاراکترهای ۱۳ و ۵۵ و ۶۶ سه شاخه‌ی مختلف را کنترل می‌کنند
   - tindata : کاراکتر ۹ نحوه‌ی حساب قیمت تمام‌شده را عوض می‌کند
   =========================================================================== */
SELECT  UNIVERSITY_CO,
        SNDKH,
        SANAT,
        TKHF,
        FROSH, MFROSH, DARAM, MOGODIA, GHEYMAT, TFROSH, SANDOGH, HPOR,
        ADA, HESMBAA, ARSESH, HAZ_TOL,
        LEN(ISNULL(OPTIONSS, ''))              AS TuleOPTIONSS,
        SUBSTRING(ISNULL(OPTIONSS,''), 13, 1)  AS OPT_13,
        SUBSTRING(ISNULL(OPTIONSS,''), 55, 1)  AS OPT_55,
        SUBSTRING(ISNULL(OPTIONSS,''), 62, 1)  AS OPT_62,
        SUBSTRING(ISNULL(OPTIONSS,''), 66, 1)  AS OPT_66
FROM    dbo.SAZMAN;


/* ===========================================================================
   بخش ۲ — یک فاکتور فروش کامل (سرتاسری)   ⭐ مهم‌ترین
   ---------------------------------------------------------------------------
   چرا لازم است: برای سند خزانه، نمونه‌ی برگه ۲۱۸ را که دادید با کد مقابله
   کردم و نگاشت THES→بدهکار و FHES→بستانکار را تأیید کردم. برای فروش هنوز
   چنین تأییدی ندارم. با یک فاکتور کامل می‌توانم بررسی کنم که ردیف‌های سند
   واقعاً از همان فرمول‌هایی می‌آیند که در کد است.

   یک فاکتور «معمولی» انتخاب کنید که ترجیحاً چند قلم کالا داشته باشد.
   اگر مبالغ حساس است، فاکتور کم‌اهمیت‌تری بردارید — من به الگو نیاز دارم نه رقم.
   =========================================================================== */
DECLARE @FactorNumber FLOAT = 0;   -- ← اینجا شماره فاکتور را بگذارید

-- اگر شماره‌ای نگذاشتید، یک فاکتور با بیشترین تعداد قلم انتخاب می‌شود
IF @FactorNumber = 0
    SELECT TOP 1 @FactorNumber = h.NUMBER
    FROM   dbo.HEAD_LST h
           INNER JOIN dbo.INVO_LST i ON i.NUMBER = h.NUMBER AND i.TAG = 2
    WHERE  h.TAG = 13 AND h.N_S IS NOT NULL
    GROUP BY h.NUMBER
    HAVING COUNT(*) BETWEEN 3 AND 6
    ORDER BY h.NUMBER DESC;

SELECT @FactorNumber AS FaktorEntekhabShode;

-- ۲-الف: سربرگ فاکتور
SELECT  NUMBER, TAG, NUMBER1, FNUMCO, DATE_N, CUST_NO, CUST_KIND, N_S,
        MAS, VAS, MBAA, TAKHFIF, MABL_HAZ, MOIN_HAZ, HMBAA,
        M_NAGHD, DEPATMAN, SHIFT, ANBAR, ARZD, SADER, MOLAH, USER_NAME
FROM    dbo.HEAD_LST WHERE NUMBER = @FactorNumber AND TAG = 13;

-- ۲-ب: اقلام فاکتور
SELECT  i.NUMBER, i.TAG, i.RADIF, i.CODE, s.NAME, i.MEGH, i.MEGHk,
        i.MABL, i.MABL_K, i.ANBAR, i.AVRAGE, i.N_KOL, i.N_MOIN
FROM    dbo.INVO_LST i LEFT JOIN dbo.STUF_DEF s ON s.CODE = i.CODE
WHERE   i.NUMBER = @FactorNumber AND i.TAG = 2
ORDER BY i.RADIF;

-- ۲-ج: سربرگ سند حسابداری متناظر
SELECT  N_S, DATE_S, SHARH_S, NO_S, GHATEI, OKF, BAYEG, base, USER_NAME
FROM    dbo.DEED_HED
WHERE   N_S = (SELECT N_S FROM dbo.HEAD_LST WHERE NUMBER = @FactorNumber AND TAG = 13);

-- ۲-د: ردیف‌های سند حسابداری  ← اینجاست که نگاشت را تأیید می‌کنم
SELECT  id, N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES,
        SHARH, BED, BES, NUMBER, TAG, RADIF, ARZD, N_SERI, BANK, CRT
FROM    dbo.DEED_DTL
WHERE   NUMBER = @FactorNumber AND TAG = 13
ORDER BY id;

-- ۲-ه: چک‌ها و ویزیتورهای همین فاکتور (اگر دارد)
SELECT  N_SERI, BANK, DATE_S, MABL, SHOBEH, NUMBER, TAG
FROM    dbo.PAY_GETD WHERE NUMBER = @FactorNumber AND TAG = 2;

SELECT  NUMBER, TAG, CUST_NO, DARSAD, PURSANT, PORID, STAT, TOZIH
FROM    dbo.VISITOR_DTL WHERE NUMBER = @FactorNumber AND TAG = 2;


/* ===========================================================================
   بخش ۳ — تأثیر واقعی کش قیمت
   ---------------------------------------------------------------------------
   چرا لازم است: کش GETSTANDARDPRICE_* را بر این فرض ساختم که همان
   (کد کالا، تاریخ) بارها تکرار می‌شود. اگر نسبت «یکتا به کل» نزدیک ۱ باشد،
   کش تقریباً بی‌فایده است و باید سراغ راه دیگری بروم.

   TedadKolFarakhani = تعداد کل فراخوانی قیمت (۳ تابع × هر قلم انباردار)
   TedadYekta        = تعداد جفت (کد، تاریخ) یکتا
   نسبت بالا یعنی کش خیلی مؤثر است.
   =========================================================================== */
SELECT  COUNT(*)                                        AS TedadGhalamAnbardar,
        COUNT(*) * 3                                    AS TedadKolFarakhaniGheymat,
        COUNT(DISTINCT CAST(i.CODE AS NVARCHAR(15)) + '|' + CAST(h.DATE_N AS NVARCHAR(20))) AS TedadJoftYekta,
        COUNT(DISTINCT i.CODE)                          AS TedadKalayeYekta,
        COUNT(DISTINCT h.DATE_N)                        AS TedadTarikhYekta
FROM    dbo.INVO_LST i
        INNER JOIN dbo.HEAD_LST h ON h.NUMBER = i.NUMBER AND h.TAG = 13
WHERE   i.TAG = 2 AND i.ANBAR <> 0;


/* ===========================================================================
   بخش ۴ — حجم کار بازسازی فروش
   ---------------------------------------------------------------------------
   چرا لازم است: بدانم با چه اندازه‌ای طرفم و کدام بخش گران‌تر است.
   =========================================================================== */
SELECT  (SELECT COUNT(*) FROM dbo.HEAD_LST WHERE TAG = 13)                      AS TedadFaktor,
        (SELECT COUNT(*) FROM dbo.INVO_LST WHERE TAG = 2)                       AS TedadGhalam,
        (SELECT COUNT(*) FROM dbo.PAY_GETD WHERE TAG = 2)                       AS TedadChek,
        (SELECT COUNT(*) FROM dbo.VISITOR_DTL WHERE TAG = 2)                    AS TedadVizitor,
        (SELECT COUNT(*) FROM dbo.DEED_DTL)                                     AS TedadKolRadifSanad,
        (SELECT COUNT(*) FROM dbo.DEED_HED)                                     AS TedadKolSarbarg,
        (SELECT COUNT(DISTINCT DATE_N) FROM dbo.HEAD_LST WHERE TAG = 13)        AS TedadTarikhFaktor;


/* ===========================================================================
   بخش ۵ — بررسی یک نگرانی واقعی: کد کالا از محدوده int بیرون می‌زند؟
   ---------------------------------------------------------------------------
   چرا لازم است: کد چند جا Convert.ToInt64(CODE) و Convert.ToInt32 می‌کند.
   STUF_DEF.CODE از نوع nvarchar(15) است. اگر کدی بزرگ‌تر از سقف int باشد،
   بعضی مسیرها OverflowException می‌دهند. قبلاً کلید کش را به همین دلیل از
   int به double تغییر دادم؛ می‌خواهم بدانم این نگرانی واقعی است یا نه.
   =========================================================================== */
SELECT  COUNT(*)                                      AS TedadKol,
        SUM(CASE WHEN ISNUMERIC(CODE) = 0 THEN 1 ELSE 0 END)  AS TedadGheyrAdadi,
        SUM(CASE WHEN ISNUMERIC(CODE) = 1
                  AND TRY_CAST(CODE AS DECIMAL(38,0)) > 2147483647
                 THEN 1 ELSE 0 END)                   AS TedadBozorgtarAzInt,
        MAX(LEN(CODE))                                AS BishtarinTul,
        MAX(TRY_CAST(CODE AS DECIMAL(38,0)))          AS BozorgtarinKod
FROM    dbo.STUF_DEF;


/* ===========================================================================
   بخش ۶ — CUST_HESAB جدول است یا View؟
   ---------------------------------------------------------------------------
   چرا لازم است: GETTAFNAME از CUST_HESAB می‌خواند و من آن را کش کردم.
   اگر CUST_HESAB یک View روی TDETA_HES باشد، و CREATHES وسط اجرا حسابی
   بسازد، محتوای View عوض می‌شود. عمداً پاسخ «پیدا نشد» را کش نکردم، ولی
   می‌خواهم مطمئن شوم تصمیمم درست بوده.
   =========================================================================== */
SELECT  name, type_desc FROM sys.objects
WHERE   name IN ('CUST_HESAB', 'DEPART', 'TCOD_BANKS', 'HEAD_MANF', 'DTL_MANF');

-- اگر View بود، تعریفش را هم بدهید:
SELECT  OBJECT_DEFINITION(OBJECT_ID('dbo.CUST_HESAB')) AS TarifeCUST_HESAB;


/* ===========================================================================
   بخش ۷ — وضعیت فعلی ثابت‌ها (عکس «قبل»)
   ---------------------------------------------------------------------------
   چرا لازم است: می‌خواهم بدانم کدام مشکل‌ها از قبل وجود داشته‌اند.
   بدون این، اگر بعد از تست چیزی دیدید نمی‌دانیم تقصیر تغییرات است یا نه.
   =========================================================================== */
SELECT  N'سند نامتوازن' AS Barresi,
        (SELECT COUNT(*) FROM (
            SELECT N_S FROM dbo.DEED_DTL GROUP BY N_S
            HAVING ABS(SUM(ISNULL(BED,0)) - SUM(ISNULL(BES,0))) > 0.01) x) AS Tedad
UNION ALL SELECT N'شماره سند تکراری در برگه خزانه',
        (SELECT COUNT(*) FROM (
            SELECT N_S FROM dbo.PGET_HED WHERE N_S IS NOT NULL
            GROUP BY N_S HAVING COUNT(*) > 1) x)
UNION ALL SELECT N'سند فروش تکراری برای یک تاریخ',
        (SELECT COUNT(*) FROM (
            SELECT DATE_S FROM dbo.DEED_HED WHERE NO_S = 2
            GROUP BY DATE_S HAVING COUNT(*) > 1) x)
UNION ALL SELECT N'شماره بایگانی تکراری',
        (SELECT COUNT(*) FROM (
            SELECT BAYEG FROM dbo.DEED_HED WHERE BAYEG IS NOT NULL
            GROUP BY BAYEG HAVING COUNT(*) > 1) x)
UNION ALL SELECT N'سربرگ بدون ردیف',
        (SELECT COUNT(*) FROM dbo.DEED_HED h WHERE h.NO_S IN (2,5)
         AND NOT EXISTS (SELECT 1 FROM dbo.DEED_DTL d WHERE d.N_S = h.N_S));
