# فاز ۷ — جدول‌های قدیمی، پاک‌سازی و گزارش نهایی

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

> ⏱ روی جدول میلیونی کار می‌کند. **زمان بگیر.**

## ۷.۱ جدول‌های قدیمی هنوز پر می‌شوند؟
در برنامه یک رکورد حذف کن، بعد:
```sql
SELECT TOP 5 * FROM dbo.AMALIAT ORDER BY ADATE DESC;
SELECT TOP 5 * FROM dbo.USER_AUDIT_LOG ORDER BY ActionDateTime DESC;
```
انتظار: کار همین الان در هر دو دیده شود.

## ۷.۲ انتقال
```sql
SELECT COUNT(*) قبل FROM dbo.SYS_AUDIT_EVENT;
SELECT COUNT(*) FROM dbo.AMALIAT;
SELECT COUNT(*) FROM dbo.USER_AUDIT_LOG;
EXEC dbo.SYS_AUDIT_BACKFILL;   -- زمان بگیر
SELECT COUNT(*) بعد FROM dbo.SYS_AUDIT_EVENT;
```
انتظار: `بعد ≈ قبل + amaliat + userlog`.
حین اجرا از پنجره‌ی دیگر `EXEC sp_who2;` — نباید قفل طولانی بدهد.

## ۷.۳ ⭐ اجرای دوباره نباید تکراری بسازد
```sql
EXEC dbo.SYS_AUDIT_BACKFILL;  SELECT COUNT(*) FROM dbo.SYS_AUDIT_EVENT;
EXEC dbo.SYS_AUDIT_BACKFILL;  SELECT COUNT(*) FROM dbo.SYS_AUDIT_EVENT;
```
**انتظار: هر سه عدد دقیقاً یکی.**
❌ اگر بالا رفت، باگی برگشته که هر بار لاگین کل جدول را از نو درج می‌کند.

## ۷.۴ پاک‌سازی
```sql
SELECT CATEGORY,COUNT(*) FROM dbo.SYS_AUDIT_EVENT GROUP BY CATEGORY;
SELECT COUNT(*) FROM dbo.SYS_AUDIT_EVENT WHERE ACTION='DELETE';

EXEC dbo.SYS_AUDIT_PURGE @KeepDaysNavigation=90,@KeepDaysOther=1825,@ChunkSize=5000;

SELECT CATEGORY,COUNT(*) FROM dbo.SYS_AUDIT_EVENT GROUP BY CATEGORY;
SELECT COUNT(*) FROM dbo.SYS_AUDIT_EVENT WHERE ACTION='DELETE';
SELECT COUNT(*) FROM dbo.SYS_AUDIT_EVENT WHERE AT_SERVER>=CAST(GETDATE() AS DATE);
```
انتظار: ناوبری >۹۰ روز حذف · ناوبری تازه بماند · حذف/امضای <۵ سال بماند ·
**رویدادهای امروز دست‌نخورده**.
❌ اگر رویداد حذفِ تازه پاک شد → باگ جدی.

## ۷.۵ جستجوی نقاط بی‌سابقه
این چهار مورد **باگ نیستند**، فقط تأییدشان کن: `AUTO_BAZ` مستقل ·
`WIN_F_NEWYEAR` · نبودِ مقدار «قبل» · رویدادهای spill به جدول قدیمی نمی‌روند.

⭐ **سؤال اصلی:** **پنج فرم** که تا حالا باز نکردی امتحان کن (حسابداری،
انبار، چک، تعاریف پایه، حقوق). در هر کدام یک رکورد ثبت/ویرایش/حذف کن.

| فرم | ثبت | ویرایش | حذف |
|---|---|---|---|

❌ هر فرمی که رکورد عوض می‌کند ولی ردیفی نمی‌سازد = **یافته‌ی مهم**.

---

# 📋 گزارش نهایی (کل ۸ فاز)

**الف) جدول:** فاز ۰ تا ۷ — نتیجه و یافته‌ی مهم هر کدام.

**ب) ایرادها:** برای هر کدام — چه شد · چطور تکرارش کنم · شدت · مدرک.

**ج) تست‌نشده‌ها:** صادقانه، با دلیل.

**د) تغییرات کد:** فایل/خط، برگرداندی؟ خروجی `git status`. یا «هیچ».

**ه) نظر نهایی:**
1. آماده‌ی استفاده‌ی عملیاتی است؟
2. اگر نه، اول چه چیزی درست شود؟
3. جایی هست که سابقه بتواند **دروغ** بگوید؟

> این آخرین فاز است. گزارش نهایی را کامل بده.
