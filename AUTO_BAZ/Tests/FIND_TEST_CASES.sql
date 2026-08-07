/*
================================================================================
  FIND_TEST_CASES.sql
  ----------------------------------------------------------------------------
  پیدا کردن داده‌ی مناسب برای مسیرهایی از بازسازی سند که هنوز تست نشده‌اند.

  همه‌ی کوئری‌های این فایل فقط SELECT هستند و هیچ چیزی را تغییر نمی‌دهند،
  پس اجرای آن روی دیتابیس اصلی هم بی‌خطر است.

  ⚠ ولی «بازسازی سند» را فقط روی یک کپی از دیتابیس اجرا کنید، نه روی اصلی.

  ترتیب پیشنهادی: بخش ۰ را روی هر دیتابیسی که در دسترس دارید اجرا کنید تا
  بفهمید کدام دیتابیس کدام مسیر را پوشش می‌دهد؛ بعد سراغ بخش مربوطه بروید.
================================================================================
*/

/* ==============================================================================
   بخش ۰ — تنظیمات این دیتابیس
   ------------------------------------------------------------------------------
   سه تنظیم تعیین می‌کنند کدام مسیرهای کد اصلاً اجرا می‌شوند:

     SNDKH  = 1  →  «سند روزانه»  (همه فاکتورهای یک تاریخ در یک سند)
     SNDKH  = 0  →  «تک‌سندی»     (هر فاکتور سند خودش را دارد)  ← تست‌نشده

     OPT66 <> '5' → مسیر «قیمت استاندارد» (MAVAD/DAST/SAR) فعال است ← تست‌نشده
     OPT66  = '5' → مسیر «میانگین» (AVRAGE) فعال است               ← تست شده

   دیتابیس‌های YAZDSEPAR1405 و NEWPOODR1405 هر دو SNDKH=1 و OPT66='5' بودند؛
   برای همین دو مسیر دیگر هنوز اجرا نشده‌اند.
============================================================================== */
SELECT
    DB_NAME()                              AS DBName,
    UNIVERSITY_CO,
    SNDKH                                  AS SanadRoozane,
    SANAT,
    SUBSTRING(OPTIONSS, 66, 1)             AS OPT66,
    CASE WHEN SUBSTRING(OPTIONSS, 66, 1) = '5'
         THEN N'مسیر میانگین (AVRAGE) — تست شده'
         ELSE N'مسیر قیمت استاندارد (MAVAD/DAST/SAR) — تست نشده'
    END                                    AS MasireBahayeTamamShode,
    LEN(OPTIONSS)                          AS TooleOPTIONSS,
    FROSH                                  AS HesabForoosh,
    MOGODIA                                AS HesabMogoodi,
    GHEYMAT                                AS HesabBahayeTamamShode,
    DARAM                                  AS HesabDaramad
FROM dbo.SAZMAN;


/* ==============================================================================
   بخش ۱ — «چک» : کدام تاریخ‌ها فاکتورِ چک‌دار دارند؟
   ------------------------------------------------------------------------------
   بالاترین اولویت تست است، چون هیچ تنظیمی لازم ندارد — فقط باید تاریخی را
   انتخاب کنید که چک داشته باشد. اگر خروجی خالی بود یعنی این دیتابیس هم
   مثل دو تای قبلی چک ندارد و باید سراغ دیتابیس دیگری بروید.
============================================================================== */
SELECT TOP 20
    H.DATE_N                               AS Tarikh,
    MIN(H.N_S)                             AS ShomareSanad,
    COUNT(DISTINCT P.NUMBER)               AS FaktorhayeChekDar,
    COUNT(*)                               AS TedadChek,
    SUM(P.MABL)                            AS JamChek
FROM dbo.PAY_GETD AS P
INNER JOIN dbo.HEAD_LST AS H
        ON H.NUMBER = P.NUMBER
       AND H.TAG    = 2
WHERE P.TAG = 2
GROUP BY H.DATE_N
HAVING COUNT(*) > 0
ORDER BY COUNT(*) DESC;


/* ==============================================================================
   بخش ۲ — «قیمت استاندارد / دستمزد» : آیا اصلاح DAST اینجا اثر دارد؟
   ------------------------------------------------------------------------------
   اصلاح دستمزد فقط وقتی اجرا می‌شود که:
     الف) OPT66 <> '5'   (بخش ۰)
     ب) برای کالای فاکتور، «فرمول ساخت» در HEAD_MANF/DTL_MANF تعریف شده باشد
     ج) IMBIBE_MANF (دستمزد) صفر نباشد
     د) MEGHk فاکتور بزرگ‌تر از ۱ باشد  ← چون باگ قبلی فقط آنجا خودش را نشان می‌داد

   اگر هر سه ستون خروجی زیر عدد داشتند، این دیتابیس برای تست DAST مناسب است.
============================================================================== */
SELECT
    COUNT(*)                                                   AS TedadFormul,
    SUM(CASE WHEN ISNULL(IMBIBE_MANF, 0) <> 0 THEN 1 ELSE 0 END) AS FormulBaDastmozd,
    COUNT(DISTINCT CODE)                                       AS TedadKalayeFormulDar
FROM dbo.HEAD_MANF;

-- کدام فاکتورها هم فرمولِ دستمزددار دارند و هم مقدارشان بیشتر از ۱ است؟
SELECT TOP 20
    H.DATE_N                               AS Tarikh,
    MIN(H.N_S)                             AS ShomareSanad,
    COUNT(DISTINCT L.NUMBER)               AS TedadFaktor,
    COUNT(*)                               AS TedadRadif,
    MAX(L.MEGHk)                           AS BishtarinMeghdar
FROM dbo.INVO_LST AS L
INNER JOIN dbo.HEAD_LST AS H
        ON H.NUMBER = L.NUMBER
       AND H.TAG    = 2
WHERE L.TAG    = 2
  AND L.ANBAR <> 0
  AND L.MEGHk  > 1
  AND EXISTS (SELECT 1 FROM dbo.HEAD_MANF AS M
              WHERE M.CODE = L.CODE AND ISNULL(M.IMBIBE_MANF, 0) <> 0)
GROUP BY H.DATE_N
ORDER BY COUNT(*) DESC;


/* ==============================================================================
   بخش ۳ — «نوع ارز» (ARZKIND2) در خزانه‌داری
   ------------------------------------------------------------------------------
   ستون ARZKIND2 را خودتان اضافه کرده‌اید. برای تست انتقالش به سند، باید
   برگه‌ای پیدا کنید که واقعاً مقدار داشته باشد.
============================================================================== */
IF COL_LENGTH('dbo.PGET_LST', 'ARZKIND2') IS NULL
BEGIN
    SELECT N'ستون ARZKIND2 در این دیتابیس وجود ندارد' AS Vaziat;
END
ELSE
BEGIN
    -- sp_executesql لازم است چون اگر ستون نباشد، کامپایل شدن متن ثابت خطا می‌دهد.
    EXEC sp_executesql N'
        SELECT TOP 20
            P.N_S                          AS ShomareSanad,
            COUNT(*)                       AS TedadRadif,
            COUNT(DISTINCT P.ARZKIND2)     AS AnvaeArz,
            MIN(P.ARZKIND2)                AS KamtarinArzKind,
            MAX(P.ARZKIND2)                AS BishtarinArzKind
        FROM dbo.PGET_LST AS P
        WHERE P.ARZKIND2 IS NOT NULL
        GROUP BY P.N_S
        ORDER BY COUNT(DISTINCT P.ARZKIND2) DESC, COUNT(*) DESC;';
END


/* ==============================================================================
   بخش ۴ — «برگشت از فروش»
   ------------------------------------------------------------------------------
   مسیر جداگانه‌ای در کد دارد (TAG 24/25) و در تست‌های قبلی هم دیده نشد.
============================================================================== */
SELECT TOP 20
    H.DATE_N                               AS Tarikh,
    COUNT(DISTINCT H.NUMBER)               AS TedadBargasht
FROM dbo.HEAD_LST AS H
WHERE H.TAG = 24
GROUP BY H.DATE_N
ORDER BY COUNT(DISTINCT H.NUMBER) DESC;


/* ==============================================================================
   بخش ۵ — «اثر انگشت» کل اسناد  (برای مقایسه قبل/بعد در مقیاس بزرگ)
   ------------------------------------------------------------------------------
   وقتی همه‌ی ۱۱ بخش را با هم اجرا می‌کنید، گرفتن JSON کامل غیرعملی است.
   این کوئری به‌جایش برای هر سند یک سطر کوچک می‌دهد. خروجی را قبل و بعد از
   بازسازی بگیرید و دو فایل را با هم Diff کنید.

   اگر Radif و JamBed و JamBes و Fingerprint هر سند یکسان بود، تقریباً قطعی
   است که محتوا عوض نشده. (BINARY_CHECKSUM کامل نیست — یک ابزار غربال است،
   نه اثبات ریاضی. اگر جایی اختلاف دید، همان یک سند را با بخش ۶ کامل بگیرید.)
============================================================================== */
SELECT
    H.N_S                                  AS ShomareSanad,
    H.DATE_S                               AS TarikheSanad,
    H.BAYEG                                AS ShomareBayegani,
    COUNT(D.id)                            AS Radif,
    SUM(ISNULL(D.BED, 0))                  AS JamBed,
    SUM(ISNULL(D.BES, 0))                  AS JamBes,
    SUM(CAST(BINARY_CHECKSUM(
            D.NUMBER, D.TAG, D.RADIF, D.HES,
            D.HES_K, D.HES_M, D.HES_T, D.HES_T2,
            D.SHARH, D.BED, D.BES, D.ARZD) AS BIGINT))  AS Fingerprint
FROM dbo.DEED_HED AS H
LEFT JOIN dbo.DEED_DTL AS D
       ON D.N_S = H.N_S
GROUP BY H.N_S, H.DATE_S, H.BAYEG
ORDER BY H.N_S;


/* ==============================================================================
   بخش ۶ — عکس کامل یک تاریخ  (همان کاری که تا الان می‌کردید، ولی خودکار)
   ------------------------------------------------------------------------------
   فقط @DATE را عوض کنید؛ خودش شماره فاکتورها و شماره سند را پیدا می‌کند.
   PAY_GETD هم اضافه شده تا تست «چک» قابل راستی‌آزمایی باشد.

   یک بار قبل از بازسازی و یک بار بعد از آن اجرا کنید و هر دو خروجی را بفرستید.
============================================================================== */
DECLARE @DATE bigint = 14050126;   -- ← تاریخ مورد نظر

SELECT
    JSON_QUERY((
        SELECT * FROM dbo.HEAD_LST
        WHERE TAG IN (2, 13)
          AND NUMBER IN (SELECT NUMBER FROM dbo.HEAD_LST WHERE TAG = 2 AND DATE_N = @DATE)
        FOR JSON PATH)) AS HEAD_LST,
    JSON_QUERY((
        SELECT * FROM dbo.INVO_LST
        WHERE TAG = 2
          AND NUMBER IN (SELECT NUMBER FROM dbo.HEAD_LST WHERE TAG = 2 AND DATE_N = @DATE)
        FOR JSON PATH)) AS INVO_LST,
    JSON_QUERY((
        SELECT * FROM dbo.PAY_GETD
        WHERE TAG = 2
          AND NUMBER IN (SELECT NUMBER FROM dbo.HEAD_LST WHERE TAG = 2 AND DATE_N = @DATE)
        FOR JSON PATH)) AS PAY_GETD,
    JSON_QUERY((
        SELECT * FROM dbo.DEED_HED
        WHERE N_S IN (SELECT DISTINCT N_S FROM dbo.HEAD_LST
                      WHERE TAG = 2 AND DATE_N = @DATE AND N_S IS NOT NULL)
        FOR JSON PATH)) AS DEED_HED,
    JSON_QUERY((
        SELECT * FROM dbo.DEED_DTL
        WHERE N_S IN (SELECT DISTINCT N_S FROM dbo.HEAD_LST
                      WHERE TAG = 2 AND DATE_N = @DATE AND N_S IS NOT NULL)
        FOR JSON PATH)) AS DEED_DTL
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
