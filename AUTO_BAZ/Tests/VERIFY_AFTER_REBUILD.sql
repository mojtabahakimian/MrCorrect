/* ============================================================================
   راستی‌آزمایی صحت اسناد حسابداری پس از بازسازی

   چه زمانی اجرا شود:
     ۱. یک بار «قبل» از بازسازی، تا وضعیت پایه را بدانید
     ۲. بازسازی را در شلوغ‌ترین حالت اجرا کنید (همه‌ی تیک‌ها + کاربران در حال کار)
     ۳. یک بار «بعد» اجرا کنید و دو خروجی را مقایسه کنید

   هر بخش یا OK می‌دهد یا ردیف‌های مشکل‌دار را نشان می‌دهد.
   هیچ چیزی را تغییر نمی‌دهد — فقط می‌خواند.

   نکته: NO_S = 2 یعنی سند فاکتور فروش، NO_S = 5 یعنی سند خزانه‌داری.
   ============================================================================ */

SET NOCOUNT ON;

PRINT '';
PRINT '==========================================================';
PRINT ' راستی‌آزمایی اسناد حسابداری';
PRINT ' زمان اجرا: ' + CONVERT(NVARCHAR(30), GETDATE(), 120);
PRINT '==========================================================';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۱ — تعادل سند (مهم‌ترین بررسی)

   بنیادی‌ترین قانون حسابداری: جمع بدهکار هر سند باید با جمع بستانکارش
   برابر باشد. اگر همزمانی چیزی را خراب کرده باشد، اینجا خودش را نشان می‌دهد.
   --------------------------------------------------------------------------- */
PRINT '--- ۱. تعادل سند (بدهکار = بستانکار) ---';

;WITH Balance AS (
    SELECT  d.N_S,
            SUM(ISNULL(d.BED, 0)) AS JamBed,
            SUM(ISNULL(d.BES, 0)) AS JamBes
    FROM    dbo.DEED_DTL d
    GROUP BY d.N_S
)
SELECT  b.N_S                              AS ShomareSanad,
        h.NO_S                             AS NoeSanad,
        h.DATE_S                           AS TarikhSanad,
        b.JamBed,
        b.JamBes,
        b.JamBed - b.JamBes                AS Ekhtelaf,
        LEFT(ISNULL(h.SHARH_S, N''), 60)   AS Sharh
FROM    Balance b
        LEFT JOIN dbo.DEED_HED h ON h.N_S = b.N_S
WHERE   ABS(b.JamBed - b.JamBes) > 0.01
ORDER BY ABS(b.JamBed - b.JamBes) DESC;

IF @@ROWCOUNT = 0 PRINT '    OK — همه‌ی اسناد متوازن‌اند';
ELSE PRINT '    !! اسناد نامتوازن پیدا شد (بالا) — این جدی است';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۲ — شماره سند تکراری بین برگه‌های خزانه

   هر برگه خزانه باید سند مخصوص خودش را داشته باشد. اگر چند برگه به یک
   شماره سند وصل باشند، یعنی باگ «UPDATE با شرط بازه‌ای» اثر گذاشته و
   در بازسازی موازی دو Thread روی یک سند کار می‌کنند.
   --------------------------------------------------------------------------- */
PRINT '--- ۲. شماره سند تکراری در برگه‌های خزانه ---';

SELECT  p.N_S                    AS ShomareSanad,
        COUNT(*)                 AS TedadBarge,
        MIN(p.ID)                AS AzBarge,
        MAX(p.ID)                AS TaBarge
FROM    dbo.PGET_HED p
WHERE   p.N_S IS NOT NULL
GROUP BY p.N_S
HAVING  COUNT(*) > 1
ORDER BY COUNT(*) DESC;

IF @@ROWCOUNT = 0 PRINT '    OK — هر برگه خزانه شماره سند یکتای خودش را دارد';
ELSE PRINT '    !! چند برگه خزانه یک شماره سند مشترک دارند';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۳ — سند روزانه تکراری (فقط وقتی حالت «سند روزانه» روشن است)

   در حالت SNDKH همه‌ی فاکتورهای یک تاریخ باید یک سند مشترک بگیرند.
   اگر برای یک تاریخ بیش از یک سند فروش وجود داشته باشد، یعنی دو Thread
   همزمان جواب «سندی با این تاریخ نیست» گرفته‌اند و هر دو ساخته‌اند.

   اگر حالت سند روزانه خاموش است، این بخش را نادیده بگیرید (طبیعی است که
   هر فاکتور سند جدا داشته باشد).
   --------------------------------------------------------------------------- */
PRINT '--- ۳. سند روزانه تکراری برای یک تاریخ (حالت SNDKH) ---';

SELECT  s.SNDKH AS HalateSanadRoozane_1YaniRoshan FROM dbo.SAZMAN s;

SELECT  h.DATE_S            AS Tarikh,
        COUNT(*)            AS TedadSanad,
        MIN(h.N_S)          AS AvvalinSanad,
        MAX(h.N_S)          AS AkharinSanad
FROM    dbo.DEED_HED h
WHERE   h.NO_S = 2
GROUP BY h.DATE_S
HAVING  COUNT(*) > 1
ORDER BY COUNT(*) DESC;

IF @@ROWCOUNT = 0 PRINT '    OK — برای هیچ تاریخی بیش از یک سند فروش نیست';
ELSE PRINT '    !! چند سند فروش برای یک تاریخ — اگر SNDKH روشن است، این باگ است';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۴ — شماره بایگانی تکراری

   BAYEG باید یکتا باشد. اگر تکراری باشد یعنی دو فراخوانی همزمان
   MAX(BAYEG) را بیرون از قفل خوانده‌اند و هر دو یک عدد گرفته‌اند.
   --------------------------------------------------------------------------- */
PRINT '--- ۴. شماره بایگانی تکراری ---';

SELECT  h.BAYEG            AS ShomareBayegani,
        COUNT(*)           AS TedadSanad,
        MIN(h.N_S)         AS AzSanad,
        MAX(h.N_S)         AS TaSanad
FROM    dbo.DEED_HED h
WHERE   h.BAYEG IS NOT NULL
GROUP BY h.BAYEG
HAVING  COUNT(*) > 1
ORDER BY COUNT(*) DESC;

IF @@ROWCOUNT = 0 PRINT '    OK — همه‌ی شماره‌های بایگانی یکتا هستند';
ELSE PRINT '    !! شماره بایگانی تکراری پیدا شد';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۵ — سربرگ سند بدون هیچ ردیفی

   سندی که سربرگ دارد ولی هیچ ردیف بدهکار/بستانکاری ندارد، یا نیمه‌کاره
   مانده یا بی‌صاحب است.
   --------------------------------------------------------------------------- */
PRINT '--- ۵. سربرگ سند بدون ردیف ---';

SELECT  h.N_S              AS ShomareSanad,
        h.NO_S             AS NoeSanad,
        h.DATE_S           AS Tarikh,
        LEFT(ISNULL(h.SHARH_S, N''), 60) AS Sharh,
        h.CRT              AS ZamaneSakht
FROM    dbo.DEED_HED h
WHERE   h.NO_S IN (2, 5)
  AND   NOT EXISTS (SELECT 1 FROM dbo.DEED_DTL d WHERE d.N_S = h.N_S)
ORDER BY h.N_S DESC;

IF @@ROWCOUNT = 0 PRINT '    OK — هیچ سند بی‌ردیفی نیست';
ELSE PRINT '    !! سند بدون ردیف پیدا شد (اجرای مجدد بازسازی معمولاً پرشان می‌کند)';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۶ — لینک شکسته: سندی که وجود ندارد

   اگر برگه خزانه یا فاکتور به شماره سندی اشاره کند که در DEED_HED نیست،
   یعنی لینک شکسته است.
   --------------------------------------------------------------------------- */
PRINT '--- ۶-الف. برگه خزانه با لینک شکسته ---';

SELECT  p.ID   AS ShomareBarge,
        p.DATE AS Tarikh,
        p.N_S  AS ShomareSanadeGomshode
FROM    dbo.PGET_HED p
WHERE   p.N_S IS NOT NULL
  AND   NOT EXISTS (SELECT 1 FROM dbo.DEED_HED h WHERE h.N_S = p.N_S)
ORDER BY p.ID;

IF @@ROWCOUNT = 0 PRINT '    OK';
ELSE PRINT '    !! برگه خزانه به سند ناموجود اشاره می‌کند';
PRINT '';

PRINT '--- ۶-ب. فاکتور فروش با لینک شکسته ---';

SELECT  l.NUMBER AS ShomareFactor,
        l.DATE_N AS Tarikh,
        l.N_S    AS ShomareSanadeGomshode
FROM    dbo.HEAD_LST l
WHERE   l.TAG = 13
  AND   l.N_S IS NOT NULL
  AND   NOT EXISTS (SELECT 1 FROM dbo.DEED_HED h WHERE h.N_S = l.N_S)
ORDER BY l.NUMBER;

IF @@ROWCOUNT = 0 PRINT '    OK';
ELSE PRINT '    !! فاکتور به سند ناموجود اشاره می‌کند';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۷ — شرح سند با شماره برگه نمی‌خواند (تست مستقیم Race روی متغیر شرح)

   شرح سند خزانه دقیقاً این قالب را دارد:
       خزانه داري شماره {ID} مورخ {تاریخ}
   اگر عددِ داخل شرح با ID برگه‌ای که به آن سند وصل است فرق کند، یعنی
   متغیر شرح بین Threadها قاطی شده — همان باگی که در سند فروش پیدا شد.
   --------------------------------------------------------------------------- */
PRINT '--- ۷. شرح سند خزانه با شماره برگه نمی‌خواند ---';

SELECT  p.ID                                  AS ShomareBarge,
        p.N_S                                 AS ShomareSanad,
        h.SHARH_S                             AS SharheSabtShode,
        N'خزانه داري شماره ' + CAST(p.ID AS NVARCHAR(20)) AS SharheEntezari
FROM    dbo.PGET_HED p
        INNER JOIN dbo.DEED_HED h ON h.N_S = p.N_S AND h.NO_S = 5
WHERE   p.N_S IS NOT NULL
        -- ISNULL لازم است: مقايسه‌ي NULL با LIKE نتيجه‌ي NULL مي‌دهد و رديف
        -- بي‌سروصدا از خروجي حذف مي‌شود، يعني سند بدون شرح اصلاً گزارش نمي‌شد.
  AND   ISNULL(h.SHARH_S, N'') NOT LIKE N'خزانه داري شماره ' + CAST(p.ID AS NVARCHAR(20)) + N' %'
ORDER BY p.ID;

IF @@ROWCOUNT = 0 PRINT '    OK — شرح همه‌ی اسناد با برگه‌شان می‌خواند';
ELSE PRINT '    !! شرح سند با برگه‌اش نمی‌خواند — نشانه‌ی قاطی شدن متغیر بین Threadها';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۸ — تعداد ردیف سند خزانه

   هر ردیف برگه خزانه باید دقیقاً دو ردیف سند بسازد: یکی بدهکار (از THES)
   و یکی بستانکار (از FHES). اگر تعداد نخواند، یعنی درج ناقص یا تکراری بوده.
   --------------------------------------------------------------------------- */
PRINT '--- ۸. تعداد ردیف سند خزانه (باید ۲ برابر ردیف‌های برگه باشد) ---';

;WITH BargeCount AS (
    SELECT ID, COUNT(*) AS TedadRadif FROM dbo.PGET_LST GROUP BY ID
),
SanadCount AS (
    SELECT d.N_S, COUNT(*) AS TedadRadif FROM dbo.DEED_DTL d GROUP BY d.N_S
)
SELECT  p.ID                        AS ShomareBarge,
        p.N_S                       AS ShomareSanad,
        b.TedadRadif                AS RadifeBarge,
        ISNULL(s.TedadRadif, 0)     AS RadifeSanad,
        b.TedadRadif * 2            AS RadifeEntezari
FROM    dbo.PGET_HED p
        INNER JOIN BargeCount b ON b.ID = p.ID
        LEFT  JOIN SanadCount s ON s.N_S = p.N_S
WHERE   p.N_S IS NOT NULL
  AND   ISNULL(s.TedadRadif, 0) <> b.TedadRadif * 2
ORDER BY p.ID;

IF @@ROWCOUNT = 0 PRINT '    OK — تعداد ردیف‌ها دقیقاً می‌خواند';
ELSE PRINT '    !! تعداد ردیف سند با برگه نمی‌خواند';
PRINT '';


/* ---------------------------------------------------------------------------
   بخش ۹ — آمار کلی (برای مقایسه قبل و بعد)

   این اعداد را قبل و بعد از بازسازی یادداشت کنید. تعداد اسناد و ردیف‌ها
   نباید بی‌دلیل زیاد شود؛ زیاد شدنشان یعنی سند تکراری ساخته شده.
   --------------------------------------------------------------------------- */
PRINT '--- ۹. آمار کلی ---';

SELECT  N'سند خزانه'        AS Bakhsh,
        COUNT(*)            AS TedadSarbarg,
        (SELECT COUNT(*) FROM dbo.DEED_DTL d
         WHERE EXISTS (SELECT 1 FROM dbo.DEED_HED x WHERE x.N_S = d.N_S AND x.NO_S = 5)) AS TedadRadif
FROM    dbo.DEED_HED WHERE NO_S = 5
UNION ALL
SELECT  N'سند فروش',
        COUNT(*),
        (SELECT COUNT(*) FROM dbo.DEED_DTL d
         WHERE EXISTS (SELECT 1 FROM dbo.DEED_HED x WHERE x.N_S = d.N_S AND x.NO_S = 2))
FROM    dbo.DEED_HED WHERE NO_S = 2
UNION ALL
SELECT  N'کل اسناد',
        COUNT(*),
        (SELECT COUNT(*) FROM dbo.DEED_DTL)
FROM    dbo.DEED_HED;

PRINT '';
PRINT '==========================================================';
PRINT ' پایان راستی‌آزمایی';
PRINT '==========================================================';
