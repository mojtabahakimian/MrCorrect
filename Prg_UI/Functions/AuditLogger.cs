using Prg_Proccessy.AUDIT;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Functions
{
    /// <summary>
    /// نگه‌داشته شده برای سازگاری. کدهایی که این مدل را می‌سازند بدون تغییر
    /// کامپایل می‌شوند.
    /// </summary>
    public class AuditLogEntry
    {
        public string UserName { get; set; }
        public string WindowsUserName { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public string RecordID { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string IPAddress { get; set; }
        public string MachineName { get; set; }
        public string ApplicationVersion { get; set; }
        public string WindowsVersion { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string AdditionalInfo { get; set; }
        public Guid SessionID { get; set; }
        public int ProcessID { get; set; }
        public int ThreadID { get; set; }
        public string StackTrace { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// پوسته‌ی نازک روی <see cref="Audit"/>.
    ///
    /// امضای متدها عمداً دست‌نخورده مانده تا هر ۹۶ نقطه‌ی فراخوانی موجود
    /// بدون هیچ تغییری کامپایل شوند، ولی رفتار زیر پوست عوض شده و سه ایراد
    /// واقعی نسخه‌ی قبلی برطرف شده است:
    ///
    ///   ۱. <c>Dns.GetHostEntry</c> که یک فراخوانی DNS مسدودکننده بود و پیش
    ///      از اولین await — یعنی روی نخ رابط کاربری — اجرا می‌شد. اگر سرور
    ///      نام کند بود، فرم چند ثانیه فریز می‌شد. حالا آدرس IP یک بار در
    ///      شروع نشست و از روی کارت‌های شبکه‌ی محلی خوانده می‌شود.
    ///   ۲. <c>Process.GetCurrentProcess()</c> در هر فراخوانی یک شیء
    ///      Dispose‌نشده می‌ساخت؛ در ۸۳ نقطه‌ی حذف تکرار می‌شد.
    ///   ۳. یک <c>SemaphoreSlim</c> سراسری تمام نوشتن‌های سابقه را صف می‌کرد.
    ///
    /// جدول قدیمی <c>USER_AUDIT_LOG</c> همچنان پر می‌شود، ولی از نخ پس‌زمینه.
    /// </summary>
    public static class AuditLogger
    {
        public static Task LogActionAsync(string actionType, string tableName, string recordId,
                                          string oldValue = null, string newValue = null, string additionalInfo = null)
        {
            Send(actionType, tableName, recordId, oldValue, newValue, additionalInfo);
            return Task.CompletedTask;
        }

        public static Task LogActionAsync(string actionType, string tableName, string recordId,
                                          object oldValue = null, object newValue = null, string additionalInfo = null)
        {
            Send(actionType, tableName, recordId, Stringify(oldValue), Stringify(newValue), additionalInfo);
            return Task.CompletedTask;
        }

        private static void Send(string actionType, string tableName, string recordId,
                                 string oldValue, string newValue, string additionalInfo)
        {
            try
            {
                var isDelete = string.Equals(actionType, "DELETE", StringComparison.OrdinalIgnoreCase);

                Audit.Write(new AuditEventDraft
                {
                    Category = AuditCategory.Data,
                    Severity = isDelete ? AuditSeverity.Sensitive : AuditSeverity.Notable,
                    Action = string.IsNullOrWhiteSpace(actionType) ? "UNKNOWN" : actionType.ToUpperInvariant(),
                    Entity = tableName,
                    EntityKey = recordId,
                    Title = BuildTitle(actionType, tableName, recordId),
                    Detail = additionalInfo,
                    IsCritical = isDelete,

                    // جدول قدیمی حذف نشده و همچنان تغذیه می‌شود.
                    Legacy = AuditLegacyTarget.UserAuditLog,
                    LegacyOldValue = oldValue,
                    LegacyNewValue = newValue,
                });
            }
            catch
            {
                // ثبت سابقه هرگز نباید کار کاربر را متوقف کند.
            }
        }

        private static string BuildTitle(string actionType, string tableName, string recordId)
        {
            var verb = (actionType ?? string.Empty).ToUpperInvariant() switch
            {
                "DELETE" => "حذف",
                "INSERT" => "ثبت",
                "UPDATE" => "ویرایش",
                _ => actionType,
            };

            var what = string.IsNullOrWhiteSpace(tableName) ? string.Empty : " " + tableName;
            var key = string.IsNullOrWhiteSpace(recordId) ? string.Empty : " " + recordId;
            return (verb + what + key).Trim();
        }

        private static string Stringify(object value)
        {
            if (value is null) return null;
            if (value is string s) return s;
            try { return JsonSerializer.Serialize(value, Prg_Proccessy.AUDIT.Audit.JsonOptions); }
            catch { return value.ToString(); }
        }
    }
}
