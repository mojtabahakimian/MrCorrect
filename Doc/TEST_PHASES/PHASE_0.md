# فاز ۰ — آماده‌سازی

## قوانین (در همه‌ی فازها)
1. فقط روی `YAZDSEPAR1405_TEST`. اول بزن: `SELECT DB_NAME();`
2. کد را عوض نکن. اگر مجبور شدی، بنویس و آخرش `git checkout --` کن.
3. **با موس و کیبورد در برنامه کار کن.** ساختن فایل `.cs` تستی یا صدا زدن
   مستقیم `Prg_Proccessy.AUDIT` **ممنوع**.
4. تست‌نشده را ادعا نکن. نمی‌دانی؟ بنویس «نمی‌دانم».

**بعد از هر کار ۳ ثانیه صبر کن** (نوشتن دسته‌ای، هر ۷۵۰ms).

```sql
SELECT LOG_ID,USER_NAME,ACTION,CATEGORY,SEVERITY,ENTITY,ENTITY_KEY,FORM_NAME,TITLE,DETAIL
FROM dbo.VW_SYS_AUDIT_TIMELINE WHERE LOG_ID > @شروع ORDER BY LOG_ID;
```

---

در این فاز **تستی انجام نمی‌دهی**، فقط محیط را آماده می‌کنی.

## ۰.۱ کپی دیتابیس
```sql
BACKUP DATABASE YAZDSEPAR1405 TO DISK='C:\temp\yz.bak' WITH INIT;
RESTORE DATABASE YAZDSEPAR1405_TEST FROM DISK='C:\temp\yz.bak'
WITH MOVE 'YAZDSEPAR1405' TO 'C:\temp\yz_test.mdf',
     MOVE 'YAZDSEPAR1405_log' TO 'C:\temp\yz_test_log.ldf', REPLACE;
```
(نام فایل‌های منطقی: `RESTORE FILELISTONLY FROM DISK='C:\temp\yz.bak';`)

## ۰.۲ برنامه را به کپی وصل کن
رشته‌ی اتصال را عوض کن، برنامه را اجرا کن و وارد شو.

## ۰.۳ ⭐ اثبات اینکه روی کپی هستی — هر دو لازم است
```sql
SELECT DB_NAME();
SELECT TOP 1 USER_NAME,MACHINE_NAME,CLIENT_IP,DB_NAME,STARTED_AT
FROM dbo.SYS_AUDIT_SESSION ORDER BY STARTED_AT DESC;
```
ستون `DB_NAME` را **برنامه** پر می‌کند. اگر `YAZDSEPAR1405` بود (بدون
`_TEST`)، برنامه هنوز به دیتابیس عملیاتی وصل است → **متوقف شو**.

## ۰.۴ سلامت ساختار
```sql
SELECT name,type_desc FROM sys.objects WHERE name IN
 ('SYS_AUDIT_SESSION','SYS_AUDIT_EVENT','VW_SYS_AUDIT_TIMELINE',
  'SYS_AUDIT_PURGE','SYS_AUDIT_BACKFILL');
SELECT name,max_length FROM sys.columns
WHERE object_id=OBJECT_ID(N'dbo.SYS_AUDIT_EVENT') AND name IN('ACTION','ENTITY');
SELECT name FROM sys.objects WHERE name LIKE 'SP[_]SYS[_]AUDIT[_]%';
```
انتظار: ۵ شیء · `ACTION`=۳۲ و `ENTITY`=۲۰۰ · رویه‌ی `SP_` صفر ردیف.

## ۰.۵ دسترسی فرم سوابق
```sql
SELECT IDH,FORMNAME FROM dbo.TFORMS WHERE FORMNAME=N'AUDITTRAIL';
-- 78=کد کاربر تو، 479=IDH بالا
INSERT INTO dbo.SAL_CHEK (USERCO,[OBJECT],RUN,SEE,INP,UPD,DEL,CRT,UID)
VALUES (78,479,1,1,0,0,0,GETDATE(),78);
```
برنامه را ببند و باز کن → `Ctrl+F` → «سوابق» → باید باز شود. اسکرین‌شات.

## ۰.۶ نقطه‌ی شروع
```sql
SELECT MAX(LOG_ID) FROM dbo.SYS_AUDIT_EVENT;
```

> اگر هر کدام مشکل داشت، **فاز ۱ را شروع نکن**.

## گزارش
جدول نتایج + خروجی خام کوئری + `MAX(LOG_ID)` پایانی.
هر گام: ✅ / ❌ / ⏭ (انجام نشد + دلیل).

> گزارش را بده تا فاز بعد داده شود.
