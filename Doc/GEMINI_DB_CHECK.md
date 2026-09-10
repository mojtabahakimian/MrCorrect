# پرامپت برای Gemini — بررسی سیستم سابقه روی دیتابیس واقعی

> این متن را عیناً به Gemini بدهید. فرض بر این است که Gemini به دیتابیس
> تستی `YAZDSEPAR1405` دسترسی مستقیم SQL دارد.

---

تو به یک دیتابیس SQL Server به نام `YAZDSEPAR1405` دسترسی داری. این دیتابیسِ
یک نرم‌افزار ERP فارسی است. می‌خواهم **فقط با کوئری خواندنی** چند چیز را
بررسی کنی و نتیجه را دقیق گزارش کنی.

**قواعد مهم:**
- هیچ `INSERT` / `UPDATE` / `DELETE` / `DROP` / `ALTER` نزن. فقط `SELECT`.
- حدس نزن. اگر جدولی وجود ندارد یا کوئری خطا داد، **عین خطا** را بنویس.
- اگر چیزی را نمی‌دانی، بنویس «نمی‌دانم» — چیزی از خودت نساز.
- خروجی هر بخش را جدا و با شماره بنویس.

---

## بخش ۱ — آیا ساختار سابقه از قبل وجود دارد؟

```sql
SELECT
    CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]',       N'U') IS NULL THEN 'ندارد' ELSE 'دارد' END AS SYS_AUDIT_EVENT,
    CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_SESSION]',     N'U') IS NULL THEN 'ندارد' ELSE 'دارد' END AS SYS_AUDIT_SESSION,
    CASE WHEN OBJECT_ID(N'[dbo].[VW_SYS_AUDIT_TIMELINE]', N'V') IS NULL THEN 'ندارد' ELSE 'دارد' END AS VW_TIMELINE,
    CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_PURGE]',       N'P') IS NULL THEN 'ندارد' ELSE 'دارد' END AS SP_PURGE,
    CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_BACKFILL]',    N'P') IS NULL THEN 'ندارد' ELSE 'دارد' END AS SP_BACKFILL;
```

اگر همه «ندارد» بودند طبیعی است — یعنی هنوز نصب نشده. فقط گزارش کن.

**همچنین بررسی کن رویه‌ای با پیشوند رزرو شده جا نمانده باشد:**

```sql
SELECT name FROM sys.procedures WHERE name LIKE 'SP[_]SYS[_]AUDIT%';
```
انتظار: **صفر ردیف**. اگر ردیفی بود گزارش کن.

---

## بخش ۲ — نسخه و سازگاری

```sql
SELECT
    SERVERPROPERTY('ProductVersion')        AS Version,
    SERVERPROPERTY('ProductMajorVersion')   AS MajorVersion,
    SERVERPROPERTY('Edition')               AS Edition,
    (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()) AS CompatLevel,
    (SELECT collation_name      FROM sys.databases WHERE name = DB_NAME()) AS Collation;
```

**مهم:** `MajorVersion` باید **۱۵ یا بالاتر** باشد (SQL Server 2019+). اگر کمتر بود
حتماً بنویس، چون یکی از ایندکس‌ها به آن نیاز دارد.

---

## بخش ۳ — شکل واقعی جدول‌های قدیمی (مهم‌ترین بخش)

سیستم جدید در این جدول‌ها هم می‌نویسد و ساختارشان باید دقیق معلوم باشد.

```sql
SELECT t.name AS TableName, c.name AS ColumnName,
       ty.name AS DataType, c.max_length, c.is_nullable
FROM sys.tables t
JOIN sys.columns c  ON c.object_id = t.object_id
JOIN sys.types  ty  ON ty.user_type_id = c.user_type_id
WHERE t.name IN (N'AMALIAT', N'USER_AUDIT_LOG', N'TFORMS', N'SAL_CHEK', N'SALA_DTL')
ORDER BY t.name, c.column_id;
```

بعد از دیدن خروجی، **صریحاً** به این سه سؤال جواب بده:

1. آیا جدول `USER_AUDIT_LOG` ستون‌های زیر را **همه** دارد؟
   `UserName, WindowsUserName, ActionType, TableName, RecordID, OldValue, NewValue,`
   `IPAddress, MachineName, ApplicationVersion, WindowsVersion, ActionDateTime,`
   `AdditionalInfo, SessionID, ProcessID, ThreadID, StackTrace, IsSuccess, ErrorMessage`
   → اگر حتی یکی کم است، **نام ستون‌های کم را بنویس**.

2. آیا `AMALIAT` این چهار ستون را دارد؟ `USERID, USERNAME, ADATE, AMALID`
   و نوع داده‌ی هرکدام چیست؟

3. جدول `TFORMS` چه ستون‌هایی دارد و آیا این‌ها بینشان هست؟
   `FORMNAME, CAPTION, kind, GRP, IDH, CRT`
   → اگر `IDH` یا `GRP` نیست حتماً بنویس.

---

## بخش ۴ — ستون‌های `SAL_CHEK` برای دسترسی

```sql
SELECT c.name AS ColumnName, ty.name AS DataType, c.is_nullable
FROM sys.columns c
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.SAL_CHEK')
ORDER BY c.column_id;

SELECT TOP 3 * FROM dbo.SAL_CHEK;
```

بگو ستون‌های `USERCO, [OBJECT], RUN, SEE, INP, UPD, DEL` هستند یا نام‌هایشان فرق دارد.

---

## بخش ۵ — حجم داده، برای تخمین اندازه‌ی سابقه

```sql
SELECT 'AMALIAT' AS T, COUNT(*) AS Rows FROM dbo.AMALIAT
UNION ALL SELECT 'USER_AUDIT_LOG', COUNT(*) FROM dbo.USER_AUDIT_LOG
UNION ALL SELECT 'HEAD_LST',       COUNT(*) FROM dbo.HEAD_LST
UNION ALL SELECT 'INVO_LST',       COUNT(*) FROM dbo.INVO_LST
UNION ALL SELECT 'DEED_HED',       COUNT(*) FROM dbo.DEED_HED
UNION ALL SELECT 'DEED_DTL',       COUNT(*) FROM dbo.DEED_DTL
UNION ALL SELECT 'TDETA_HES',      COUNT(*) FROM dbo.TDETA_HES;
```

اگر جدولی نبود، همان سطر را حذف کن و بنویس کدام نبود.

---

## بخش ۶ — سازگاری با انتقال سابقه‌ی قدیمی

رویه‌ی انتقال، مقدارهای خالی را باید تحمل کند. این را بررسی کن:

```sql
SELECT
    SUM(CASE WHEN ADATE    IS NULL THEN 1 ELSE 0 END) AS ADATE_خالی,
    SUM(CASE WHEN USERID   IS NULL THEN 1 ELSE 0 END) AS USERID_خالی,
    SUM(CASE WHEN AMALID   IS NULL THEN 1 ELSE 0 END) AS AMALID_خالی,
    COUNT(*) AS کل
FROM dbo.AMALIAT;

SELECT
    SUM(CASE WHEN IsSuccess      IS NULL THEN 1 ELSE 0 END) AS IsSuccess_خالی,
    SUM(CASE WHEN ActionDateTime IS NULL THEN 1 ELSE 0 END) AS ActionDateTime_خالی,
    SUM(CASE WHEN ActionType     IS NULL THEN 1 ELSE 0 END) AS ActionType_خالی,
    SUM(CASE WHEN TableName      IS NULL THEN 1 ELSE 0 END) AS TableName_خالی,
    SUM(CASE WHEN UserName       IS NULL THEN 1 ELSE 0 END) AS UserName_خالی,
    COUNT(*) AS کل
FROM dbo.USER_AUDIT_LOG;
```

اگر `ActionType` یا `TableName` یا `UserName` مقدار خالی دارند، **حتماً هشدار بده** —
این‌ها در جدول جدید `NOT NULL` هستند.

---

## بخش ۷ — طول واقعی داده‌ها

جدول جدید محدودیت طول دارد. اگر داده‌ی واقعی بلندتر باشد بریده می‌شود:

```sql
SELECT MAX(LEN(AMALID))    AS MaxAmalId,    MAX(LEN(USERNAME)) AS MaxUserName FROM dbo.AMALIAT;
SELECT MAX(LEN(TableName)) AS MaxTableName, MAX(LEN(RecordID)) AS MaxRecordId,
       MAX(LEN(UserName))  AS MaxUserName,  MAX(LEN(ActionType)) AS MaxActionType
FROM dbo.USER_AUDIT_LOG;
```

مقایسه کن با سقف‌های جدید و هر جا بیشتر بود بنویس:
`ACTION=24` · `ENTITY=48` · `ENTITY_KEY=80` · `FORM_NAME=64` · `USER_NAME=50` · `TITLE=250`

---

## بخش ۸ — تداخل نام

مطمئن شو نام‌های جدید با چیزی که از قبل هست برخورد نمی‌کنند:

```sql
SELECT name, type_desc FROM sys.objects
WHERE name IN (N'SYS_AUDIT_EVENT', N'SYS_AUDIT_SESSION', N'VW_SYS_AUDIT_TIMELINE',
               N'SYS_AUDIT_PURGE', N'SYS_AUDIT_BACKFILL');

SELECT COUNT(*) AS AuditTrailInTforms FROM dbo.TFORMS WHERE FORMNAME = N'AUDITTRAIL';
```

اگر شیئی با این نام‌ها وجود دارد ولی `type_desc` آن غیرمنتظره است (مثلاً `USER_TABLE`
به‌جای `VIEW`)، حتماً بنویس.

---

## بخش ۹ — بررسی سلامت `master`

```sql
SELECT name, type_desc FROM master.sys.objects
WHERE name LIKE '%SYS_AUDIT%';
```

انتظار: **صفر ردیف**. اگر چیزی بود گزارش کن (توضیح: نام‌هایی با پیشوند `sp_`
در `master` باعث می‌شوند مایگریشن در دیتابیس کاربر شکست بخورد).

---

## قالب گزارش نهایی

در پایان یک جدول جمع‌بندی بده:

| # | بررسی | نتیجه | آیا مشکلی هست؟ |
|---|---|---|---|
| ۱ | ساختار سابقه | ... | ... |
| ۲ | نسخه‌ی SQL Server | ... | ... |
| ۳ | شکل جدول‌های قدیمی | ... | ... |
| ... | | | |

و در آخر صریح بنویس:
- **چه چیزهایی مشکل‌ساز است** و چرا
- **چه چیزهایی سالم است**
- **چه چیزی را نتوانستی بررسی کنی** و علتش
