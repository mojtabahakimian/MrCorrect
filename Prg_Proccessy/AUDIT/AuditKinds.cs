namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// دسته‌بندی رویداد. برای فیلتر سریع در فرم بررسی سوابق و برای
    /// سیاست نگه‌داری متفاوت (رویدادهای ناوبری زودتر پاک می‌شوند).
    /// </summary>
    public enum AuditCategory : byte
    {
        /// <summary>باز/بسته شدن فرم و حرکت در نرم‌افزار</summary>
        Navigation = 1,

        /// <summary>تغییر داده: درج، ویرایش، حذف</summary>
        Data = 2,

        /// <summary>عملیات کاری: امضا، تایید، تبدیل، نهایی‌سازی</summary>
        Business = 3,

        /// <summary>چاپ، پیش‌نمایش و خروجی گرفتن</summary>
        Report = 4,

        /// <summary>ورود و خروج کاربر</summary>
        Auth = 5,

        /// <summary>رویدادهای امنیتی: تغییر رمز، تغییر دسترسی، مشاهده سوابق</summary>
        Security = 6,
    }

    /// <summary>
    /// اهمیت رویداد. برای اینکه بررسی‌کننده بتواند فقط موارد حساس را ببیند.
    /// </summary>
    public enum AuditSeverity : byte
    {
        Normal = 1,
        Notable = 2,
        Sensitive = 3,
    }

    /// <summary>
    /// نوع عملیات. رشته‌های ثابت به‌جای enum نگه داشته شده‌اند تا افزودن
    /// عملیات جدید نیازی به تغییر ستون دیتابیس یا مایگریشن نداشته باشد.
    /// </summary>
    public static class AuditAction
    {
        public const string OpenForm = "OPEN_FORM";
        public const string CloseForm = "CLOSE_FORM";

        public const string Insert = "INSERT";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";

        public const string Sign = "SIGN";
        public const string Unsign = "UNSIGN";
        public const string Approve = "APPROVE";
        public const string Unapprove = "UNAPPROVE";
        public const string Convert = "CONVERT";
        public const string Confirm = "CONFIRM";
        public const string Cancel = "CANCEL";

        public const string Preview = "PREVIEW";
        public const string Print = "PRINT";
        public const string Export = "EXPORT";

        public const string Login = "LOGIN";
        public const string LoginFailed = "LOGIN_FAILED";
        public const string Logout = "LOGOUT";

        public const string PasswordChange = "PASSWORD_CHANGE";
        public const string PermissionChange = "PERMISSION_CHANGE";
        public const string AuditViewed = "AUDIT_VIEWED";
    }

    /// <summary>
    /// مشخص می‌کند که آیا این رویداد باید در جدول‌های سابقه‌ی قدیمی هم
    /// نوشته شود یا نه. جدول‌های قدیمی حذف نشده‌اند و برای سازگاری عقب‌رو
    /// همچنان پر می‌شوند، ولی نوشتن آن‌ها هم از همین صف پس‌زمینه انجام
    /// می‌شود تا دیگر روی نخ رابط کاربری اجرا نشود.
    /// </summary>
    public enum AuditLegacyTarget : byte
    {
        None = 0,

        /// <summary>جدول AMALIAT — سابقه‌ی باز شدن فرم‌ها</summary>
        Amaliat = 1,

        /// <summary>جدول USER_AUDIT_LOG — سابقه‌ی حذف‌ها</summary>
        UserAuditLog = 2,
    }
}
