using System;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// یک رویداد سابقه. تمام مقادیر در لحظه‌ی وقوع (روی نخ فراخوان) پر
    /// می‌شوند و بعد از آن این شیء تغییر نمی‌کند.
    ///
    /// این نکته برای درستی سابقه حیاتی است: نخ پس‌زمینه هرگز
    /// <c>Baseknow.UUSER</c> یا هر حالت سراسری دیگری را نمی‌خواند، چون تا
    /// زمان نوشتن روی دیتابیس ممکن است کاربر عوض شده باشد و رویداد به نام
    /// شخص اشتباه ثبت شود.
    /// </summary>
    public sealed class AuditEvent
    {
        public Guid SessionId { get; init; }

        /// <summary>شماره‌ی ترتیبی صعودی در همین نشست. ترتیب واقعی رویدادها با این تضمین می‌شود، نه با ساعت.</summary>
        public int Seq { get; init; }

        public int? UserId { get; init; }

        /// <summary>نام کاربر، عمداً در همین ردیف تکرار شده تا گزارش حتی بدون ردیف نشست هم کار کند.</summary>
        public string? UserName { get; init; }

        /// <summary>زمان وقوع روی کلاینت. برای ترتیب رویدادهای یک کاربر.</summary>
        public DateTime AtClient { get; init; }

        /// <summary>تاریخ شمسی عددی، مثل 14050517. ایندکس‌شده تا فیلتر تاریخ بدون تبدیل انجام شود.</summary>
        public int DateS { get; init; }

        /// <summary>ساعت عددی، مثل 142530.</summary>
        public int TimeS { get; init; }

        public AuditCategory Category { get; init; }
        public AuditSeverity Severity { get; init; } = AuditSeverity.Normal;

        public string Action { get; init; } = string.Empty;

        /// <summary>موجودیت کاری، مثل HEAD_LST یا PISH_FACTOR.</summary>
        public string? Entity { get; init; }

        /// <summary>کلید رکورد، مثل "NUMBER=1234;TAG=20".</summary>
        public string? EntityKey { get; init; }

        public string? FormName { get; init; }

        /// <summary>
        /// متن فارسی از پیش آماده‌شده. ستون اصلی گزارش خطی است تا
        /// بررسی‌کننده بدون خواندن JSON و بدون JOIN، در یک نگاه بفهمد چه شده.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>جزئیات به شکل JSON. فقط وقتی پر می‌شود که واقعاً لازم باشد.</summary>
        public string? Detail { get; init; }

        public bool IsSuccess { get; init; } = true;
        public string? ErrorMessage { get; init; }
        public int? DurationMs { get; init; }

        /// <summary>
        /// شناسه‌ی هم‌بستگی: چند رویداد که بخشی از یک عملیات واحد هستند
        /// (مثلاً «تبدیل پیش‌فاکتور به فاکتور» که چند جدول را می‌نویسد) یک
        /// مقدار مشترک می‌گیرند تا در گزارش کنار هم دیده شوند.
        /// </summary>
        public Guid? CorrelationId { get; init; }

        /// <summary>اگر true باشد، در صورت پر بودن صف هرگز دور ریخته نمی‌شود و روی دیسک محلی ذخیره می‌شود.</summary>
        public bool IsCritical { get; init; }

        public AuditLegacyTarget Legacy { get; init; } = AuditLegacyTarget.None;

        /// <summary>فقط برای نوشتن در جدول قدیمی USER_AUDIT_LOG استفاده می‌شود.</summary>
        public string? LegacyOldValue { get; init; }

        /// <summary>فقط برای نوشتن در جدول قدیمی USER_AUDIT_LOG استفاده می‌شود.</summary>
        public string? LegacyNewValue { get; init; }
    }

    /// <summary>
    /// اطلاعات ثابت یک نشست (یک بار اجرای برنامه). این مقادیر در هر رویداد
    /// تکرار نمی‌شوند و فقط یک بار در جدول SYS_AUDIT_SESSION ذخیره می‌گردند؛
    /// نتیجه‌اش ردیف رویداد باریک‌تر، حجم کمتر و اسکن سریع‌تر است.
    /// </summary>
    public sealed class AuditSessionInfo
    {
        public Guid SessionId { get; init; }
        public int? UserId { get; init; }
        public string? UserName { get; init; }
        public string? WindowsUser { get; init; }
        public string? MachineName { get; init; }
        public string? ClientIp { get; init; }
        public string? AppVersion { get; init; }
        public string? OsVersion { get; init; }
        public int ProcessId { get; init; }
        public short? FiscalYear { get; init; }
        public string? DatabaseName { get; init; }
        public DateTime StartedAt { get; init; }
    }
}
