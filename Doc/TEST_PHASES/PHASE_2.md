# فاز ۲ — ورود، خروج و تعویض کاربر

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

> ⚠️ اگر برنامه **بیلد Debug** است، ممکن است خودکار وارد شود و فرم لاگین
> را نبینی. در آن حالت با **بیلد Release** کار کن، وگرنه این فاز بی‌معنی
> است. اگر نتوانستی، بنویس و رد شو.

## ۲.۱ ورود ناموفق
با دست: خروج بزن → نام کاربری واقعی + **رمز غلط** (دو بار) → نام کاربری
ناموجود `nobody_xyz` (یک بار) → ورود درست → خروج.

```sql
SELECT LOG_ID,USER_NAME,ACTION,TITLE,IS_SUCCESS FROM dbo.VW_SYS_AUDIT_TIMELINE
WHERE LOG_ID>@شروع AND ACTION IN('LOGIN','LOGIN_FAILED','LOGOUT') ORDER BY LOG_ID;
```

| انتظار | `USER_NAME` |
|---|---|
| ۲× `LOGIN_FAILED` | نام واقعی‌ای که تایپ کردی |
| ۱× `LOGIN_FAILED` | `nobody_xyz` |
| `LOGIN` + `LOGOUT` | کاربر واقعی |

`TITLE` باید «رمز عبور نادرست» را از «نام کاربری ناشناخته» تفکیک کند.
❌ اگر زیر نام کاربرِ موفق ثبت شد، یا `USER_NAME` خالی بود → باگ.

## ۲.۲ تعویض کاربر
**بدون بستن برنامه:** با کاربر اول ۲ کار بکن → خروج → با کاربر دوم وارد
شو → ۳ کار بکن.

```sql
SELECT SESSION_ID,USER_NAME,STARTED_AT,ENDED_AT,EVENT_COUNT,DROPPED_COUNT
FROM dbo.SYS_AUDIT_SESSION ORDER BY STARTED_AT DESC;

SELECT USER_NAME,COUNT(*),MIN(LOG_ID),MAX(LOG_ID) FROM dbo.VW_SYS_AUDIT_TIMELINE
WHERE LOG_ID>@شروع GROUP BY USER_NAME;
```
انتظار: دو نشست جدا · نشست اول `ENDED_AT` پر · بازه‌ی `LOG_ID` دو کاربر
هم‌پوشانی نداشته باشد.

## ۲.۳ مشاهده‌ی سوابق ثبت می‌شود؟
فرم سوابق را باز کن و جستجو کن.
```sql
SELECT TOP 3 USER_NAME,ACTION,TITLE FROM dbo.VW_SYS_AUDIT_TIMELINE
WHERE ACTION='AUDIT_VIEWED' ORDER BY LOG_ID DESC;
```
انتظار: یک رویداد با `CATEGORY=6`.

## گزارش
جدول نتایج + خروجی خام کوئری + `MAX(LOG_ID)` پایانی.
هر گام: ✅ / ❌ / ⏭ (انجام نشد + دلیل).

> گزارش را بده تا فاز بعد داده شود.
