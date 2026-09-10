using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// ساختار جدول‌های سابقه.
    ///
    /// این DDL از نسخه‌ی فعلی به بعد در مخزن ScriptSqly نگه‌داری می‌شود
    /// (فایل <c>ScriptSqly.Core/ScriptSqly.Audit.cs</c>، متد
    /// <c>AuditScript</c> که از <c>LetsGo</c> صدا زده می‌شود) — طبق قرارداد
    /// پروژه که هر تغییر ساختار دیتابیس باید از مسیر مایگریشن برود.
    ///
    /// نسخه‌ی اینجا به‌عنوان **پشتیبان** نگه داشته شده، برای نصب‌هایی که هنوز
    /// ScriptSqly را به‌روز نکرده‌اند. <see cref="EnsureCreatedAsync"/> اول با
    /// یک کوئری بررسی می‌کند ساختار موجود است یا نه و در آن صورت هیچ DDL
    /// نمی‌فرستد. هر دو نسخه idempotent‌اند، پس اجرای هم‌زمانشان بی‌ضرر است.
    ///
    /// اگر این دو را تغییر دادید، **هر دو** را با هم به‌روز کنید.
    ///
    /// وضعیت: شاخه‌ی ScriptSqly merge شده و اشاره‌گر submodule به‌روز است،
    /// پس ساختار را مسیر رسمی مایگریشن می‌سازد و این کلاس عملاً فقط یک
    /// بررسی سریع است. جزئیات در <c>Doc/AUDIT_TRAIL.md</c>.
    /// شرح کامل در Doc/AUDIT_TRAIL.md.
    ///
    /// نکته‌ی طراحی درباره‌ی تاریخ: فیلتر و ایندکس روی AT_SERVER است، نه روی
    /// تاریخ شمسی. ساعت کلاینت‌ها قابل اعتماد نیست و تبدیل شمسی داخل T-SQL
    /// (FORMAT با کالچر fa-IR) بسته به تنظیمات ممکن است ارقام فارسی برگرداند
    /// و CAST را بشکند. تبدیل شمسی در C# با PersianCalendar انجام می‌شود و
    /// DATE_S/TIME_S فقط ستون نمایشی و کمکی هستند.
    /// </summary>
    public static class AuditSchema
    {
        /// <summary>حداقل نسخه‌ی SQL Server برای OPTIMIZE_FOR_SEQUENTIAL_KEY (2019 = 15).</summary>
        private const int SequentialKeyMinVersion = 15;

        public static IReadOnlyList<string> Statements { get; } = new[]
        {
            // ── نشست: مقادیر ثابت یک بار اجرای برنامه ────────────────────
            @"IF OBJECT_ID(N'[dbo].[SYS_AUDIT_SESSION]', N'U') IS NULL
              CREATE TABLE [dbo].[SYS_AUDIT_SESSION](
                  [SESSION_ID]     UNIQUEIDENTIFIER NOT NULL,
                  [USER_ID]        INT              NULL,
                  [USER_NAME]      NVARCHAR(50)     NULL,
                  [WIN_USER]       NVARCHAR(64)     NULL,
                  [MACHINE_NAME]   NVARCHAR(64)     NULL,
                  [CLIENT_IP]      VARCHAR(128)     NULL,
                  [APP_VERSION]    NVARCHAR(40)     NULL,
                  [OS_VERSION]     NVARCHAR(100)    NULL,
                  [PROCESS_ID]     INT              NULL,
                  [FISCAL_YEAR]    SMALLINT         NULL,
                  [DB_NAME]        NVARCHAR(128)    NULL,
                  [STARTED_AT]     DATETIME2(3)     NOT NULL,
                  [STARTED_AT_SRV] DATETIME2(3)     NOT NULL CONSTRAINT [DF_SYS_AUDIT_SESSION_SRV] DEFAULT (SYSDATETIME()),
                  [ENDED_AT]       DATETIME2(3)     NULL,
                  [EVENT_COUNT]    INT              NULL,
                  [DROPPED_COUNT]  INT              NULL,
                  CONSTRAINT [PK_SYS_AUDIT_SESSION] PRIMARY KEY CLUSTERED ([SESSION_ID])
              )",

            // ── رویداد: خط زمانی اصلی ────────────────────────────────────
            // عمداً هیچ FOREIGN KEY به SYS_AUDIT_SESSION گذاشته نشده؛ ثبت
            // سابقه هرگز نباید به خاطر نبودِ ردیف نشست شکست بخورد.
            @"IF OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]', N'U') IS NULL
              CREATE TABLE [dbo].[SYS_AUDIT_EVENT](
                  [LOG_ID]      BIGINT           IDENTITY(1,1) NOT NULL,
                  [SESSION_ID]  UNIQUEIDENTIFIER NULL,
                  [SEQ]         INT              NULL,
                  [USER_ID]     INT              NULL,
                  [USER_NAME]   NVARCHAR(50)     NULL,
                  [AT_CLIENT]   DATETIME2(3)     NULL,
                  [AT_SERVER]   DATETIME2(3)     NOT NULL CONSTRAINT [DF_SYS_AUDIT_EVENT_SRV] DEFAULT (SYSDATETIME()),
                  [DATE_S]      INT              NULL,
                  [TIME_S]      INT              NULL,
                  [CATEGORY]    TINYINT          NOT NULL,
                  [SEVERITY]    TINYINT          NOT NULL CONSTRAINT [DF_SYS_AUDIT_EVENT_SEV] DEFAULT (1),
                  [ACTION]      VARCHAR(24)      NOT NULL,
                  [ENTITY]      NVARCHAR(48)     NULL,
                  [ENTITY_KEY]  NVARCHAR(80)     NULL,
                  [FORM_NAME]   VARCHAR(64)      NULL,
                  [TITLE]       NVARCHAR(250)    NULL,
                  [DETAIL]      NVARCHAR(MAX)    NULL,
                  [IS_SUCCESS]  BIT              NOT NULL CONSTRAINT [DF_SYS_AUDIT_EVENT_OK] DEFAULT (1),
                  [ERR_MSG]     NVARCHAR(400)    NULL,
                  [DURATION_MS] INT              NULL,
                  [CORR_ID]     UNIQUEIDENTIFIER NULL,
                  CONSTRAINT [PK_SYS_AUDIT_EVENT] PRIMARY KEY CLUSTERED ([LOG_ID])
              )",

            // ── ایندکس‌ها ────────────────────────────────────────────────
            // «این کاربر چه کرد؟» — پوشا (covering) تا خواندن اصلاً به جدول نرسد.
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SYS_AUDIT_EVENT_USER_TIME' AND object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]'))
              CREATE NONCLUSTERED INDEX [IX_SYS_AUDIT_EVENT_USER_TIME]
                  ON [dbo].[SYS_AUDIT_EVENT]([USER_ID], [AT_SERVER], [LOG_ID])
                  INCLUDE ([DATE_S], [TIME_S], [CATEGORY], [SEVERITY], [ACTION], [ENTITY], [ENTITY_KEY], [FORM_NAME], [TITLE], [USER_NAME])",

            // «تاریخچه‌ی این سند» — از شماره‌ی فاکتور به کل چرخه‌ی عمرش.
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SYS_AUDIT_EVENT_ENTITY' AND object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]'))
              CREATE NONCLUSTERED INDEX [IX_SYS_AUDIT_EVENT_ENTITY]
                  ON [dbo].[SYS_AUDIT_EVENT]([ENTITY], [ENTITY_KEY], [LOG_ID])
                  INCLUDE ([USER_ID], [USER_NAME], [AT_SERVER], [DATE_S], [TIME_S], [ACTION], [TITLE])",

            // خط زمانی سراسری + پاک‌سازی بازه‌ای.
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SYS_AUDIT_EVENT_TIME' AND object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]'))
              CREATE NONCLUSTERED INDEX [IX_SYS_AUDIT_EVENT_TIME]
                  ON [dbo].[SYS_AUDIT_EVENT]([AT_SERVER], [CATEGORY], [LOG_ID])",

            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SYS_AUDIT_EVENT_SESSION' AND object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]'))
              CREATE NONCLUSTERED INDEX [IX_SYS_AUDIT_EVENT_SESSION]
                  ON [dbo].[SYS_AUDIT_EVENT]([SESSION_ID], [SEQ])",

            // ایندکس فیلترشده: هم‌بستگی فقط روی رویدادهای چندمرحله‌ای معنا دارد.
            @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SYS_AUDIT_EVENT_CORR' AND object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]'))
              CREATE NONCLUSTERED INDEX [IX_SYS_AUDIT_EVENT_CORR]
                  ON [dbo].[SYS_AUDIT_EVENT]([CORR_ID], [LOG_ID])
                  WHERE [CORR_ID] IS NOT NULL",

            // ── کاهش رقابت روی صفحه‌ی آخر (SQL Server 2019+) ─────────────
            // چند ده کلاینت همزمان در انتهای یک ایندکس صعودی درج می‌کنند؛
            // بدون این تنظیم، انتظار PAGELATCH_EX دقیقاً روی همان جدولی
            // می‌افتد که قرار بود هیچ کندی‌ای ایجاد نکند.
            @"IF TRY_CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) >= " + SequentialKeyMinVersion + @"
              BEGIN
                  IF EXISTS (SELECT 1 FROM sys.indexes
                             WHERE object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]')
                               AND name = N'PK_SYS_AUDIT_EVENT'
                               AND optimize_for_sequential_key = 0)
                      ALTER INDEX [PK_SYS_AUDIT_EVENT] ON [dbo].[SYS_AUDIT_EVENT]
                          SET (OPTIMIZE_FOR_SEQUENTIAL_KEY = ON);
              END",

            // ── ثبت فرم گزارش در TFORMS ─────────────────────────────────
            // فرم مشاهده‌ی سوابق باید مجوزدار باشد، وگرنه هر کاربری می‌تواند
            // فعالیت بقیه را به‌همراه IP و نام کامپیوترشان ببیند. اینجا فقط
            // خودِ فرم ثبت می‌شود؛ دسترسی به هیچ‌کس داده نمی‌شود و مدیر باید
            // آن را در SAL_CHEK به افراد مورد نظر بدهد — همان الگوی CRMALL.
            @"IF OBJECT_ID(N'[dbo].[TFORMS]', N'U') IS NOT NULL
                 AND NOT EXISTS (SELECT 1 FROM [dbo].[TFORMS] WHERE FORMNAME = N'AUDITTRAIL')
              BEGIN
                  INSERT INTO [dbo].[TFORMS] (FORMNAME, CAPTION, kind, GRP, IDH, CRT)
                  VALUES (N'AUDITTRAIL',
                          N'سوابق و ردیابی فعالیت کاربران',
                          3,
                          ISNULL((SELECT TOP 1 GRP FROM [dbo].[TFORMS] WHERE FORMNAME = N'USERS'), 16),
                          (SELECT ISNULL(MAX(IDH), 0) + 1 FROM [dbo].[TFORMS]),
                          GETDATE());
              END",

            // ── نمای خط زمانی: رویداد + اطلاعات نشست، یکجا ───────────────
            @"CREATE OR ALTER VIEW [dbo].[VW_SYS_AUDIT_TIMELINE]
              AS
              SELECT  e.[LOG_ID],
                      e.[AT_CLIENT],
                      e.[AT_SERVER],
                      e.[DATE_S],
                      e.[TIME_S],
                      e.[USER_ID],
                      ISNULL(e.[USER_NAME], s.[USER_NAME]) AS [USER_NAME],
                      e.[CATEGORY],
                      e.[SEVERITY],
                      e.[ACTION],
                      e.[ENTITY],
                      e.[ENTITY_KEY],
                      e.[FORM_NAME],
                      e.[TITLE],
                      e.[DETAIL],
                      e.[IS_SUCCESS],
                      e.[ERR_MSG],
                      e.[DURATION_MS],
                      e.[CORR_ID],
                      e.[SESSION_ID],
                      e.[SEQ],
                      s.[MACHINE_NAME],
                      s.[CLIENT_IP],
                      s.[WIN_USER],
                      s.[APP_VERSION],
                      s.[FISCAL_YEAR]
              FROM [dbo].[SYS_AUDIT_EVENT] AS e
                  LEFT JOIN [dbo].[SYS_AUDIT_SESSION] AS s ON s.[SESSION_ID] = e.[SESSION_ID]",

            // ── پاک‌سازی: تکه‌تکه، تا قفل طولانی روی جدول ایجاد نشود ─────
            // ── پاک‌سازی نام قدیمی ───────────────────────────────────────
            // نسخه‌ی اول این رویه‌ها با پیشوند SP_ ساخته می‌شد. آن پیشوند در
            // SQL Server رزرو شده است: هر نامی که با sp_ شروع شود اول در
            // master جست‌وجو می‌شود، نه در دیتابیس جاری. نتیجه‌اش دو مشکل بود:
            // اگر نسخه‌ای از همان نام در master وجود داشت، ساختِ رویه در
            // دیتابیس کاربر با خطای «Invalid object name» شکست می‌خورد، و در
            // زمان اجرا هم EXEC می‌توانست نسخه‌ی master را صدا بزند. پس نام
            // بدون پیشوند شد و نسخه‌ی قدیمی اگر مانده باشد حذف می‌شود.
            @"IF OBJECT_ID(N'[dbo].[SP_SYS_AUDIT_PURGE]',    N'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[SP_SYS_AUDIT_PURGE];
              IF OBJECT_ID(N'[dbo].[SP_SYS_AUDIT_BACKFILL]', N'P') IS NOT NULL
                  DROP PROCEDURE [dbo].[SP_SYS_AUDIT_BACKFILL];",

            @"CREATE OR ALTER PROCEDURE [dbo].[SYS_AUDIT_PURGE]
                  @KeepDaysNavigation INT = 90,
                  @KeepDaysOther      INT = 1825,
                  @ChunkSize          INT = 5000
              AS
              BEGIN
                  SET NOCOUNT ON;

                  DECLARE @cutNav   DATETIME2(3) = DATEADD(DAY, -@KeepDaysNavigation, SYSDATETIME());
                  DECLARE @cutOther DATETIME2(3) = DATEADD(DAY, -@KeepDaysOther, SYSDATETIME());

                  DECLARE @rows INT = 1;
                  WHILE @rows > 0
                  BEGIN
                      DELETE TOP (@ChunkSize) FROM [dbo].[SYS_AUDIT_EVENT]
                      WHERE [CATEGORY] = 1 AND [AT_SERVER] < @cutNav;
                      SET @rows = @@ROWCOUNT;
                  END

                  SET @rows = 1;
                  WHILE @rows > 0
                  BEGIN
                      DELETE TOP (@ChunkSize) FROM [dbo].[SYS_AUDIT_EVENT]
                      WHERE [CATEGORY] <> 1 AND [AT_SERVER] < @cutOther;
                      SET @rows = @@ROWCOUNT;
                  END

                  DELETE s
                  FROM [dbo].[SYS_AUDIT_SESSION] AS s
                  WHERE s.[STARTED_AT_SRV] < @cutOther
                    AND NOT EXISTS (SELECT 1 FROM [dbo].[SYS_AUDIT_EVENT] AS e
                                    WHERE e.[SESSION_ID] = s.[SESSION_ID]);
              END",

            // ── انتقال سابقه‌ی قدیمی به جریان جدید ───────────────────────
            // عمداً خودکار اجرا نمی‌شود: روی پایگاه‌های بزرگ ممکن است طولانی
            // باشد و نباید استارتاپ برنامه را نگه دارد. مدیر یک بار دستی
            // اجرایش می‌کند. DATE_S/TIME_S برای ردیف‌های قدیمی NULL می‌ماند
            // چون تبدیل شمسی داخل T-SQL قابل اتکا نیست؛ فیلتر تاریخ در گزارش
            // روی AT_SERVER است و برای این ردیف‌ها هم درست کار می‌کند.
            @"CREATE OR ALTER PROCEDURE [dbo].[SYS_AUDIT_BACKFILL]
              AS
              BEGIN
                  SET NOCOUNT ON;

                  IF OBJECT_ID(N'[dbo].[AMALIAT]', N'U') IS NOT NULL
                     AND NOT EXISTS (SELECT 1 FROM [dbo].[SYS_AUDIT_EVENT]
                                     WHERE [ACTION] = 'OPEN_FORM' AND [SESSION_ID] IS NULL)
                  BEGIN
                      INSERT INTO [dbo].[SYS_AUDIT_EVENT]
                          ([SESSION_ID], [SEQ], [USER_ID], [USER_NAME], [AT_CLIENT], [AT_SERVER],
                           [CATEGORY], [SEVERITY], [ACTION], [FORM_NAME], [TITLE])
                      -- AT_SERVER ستون NOT NULL است و ADATE در جدول قدیمیِ
                      -- AMALIAT می‌تواند NULL باشد؛ بدون ISNULL کل انتقال با
                      -- خطای NOT NULL شکست می‌خورد.
                      SELECT NULL, NULL,
                             TRY_CAST(a.[USERID] AS INT),
                             LEFT(a.[USERNAME], 50),
                             a.[ADATE],
                             ISNULL(a.[ADATE], SYSDATETIME()),
                             1, 1, 'OPEN_FORM',
                             LEFT(a.[AMALID], 64),
                             N'باز کردن فرم ' + ISNULL(a.[AMALID], N'')
                      FROM [dbo].[AMALIAT] AS a;
                  END

                  IF OBJECT_ID(N'[dbo].[USER_AUDIT_LOG]', N'U') IS NOT NULL
                     AND NOT EXISTS (SELECT 1 FROM [dbo].[SYS_AUDIT_EVENT]
                                     WHERE [ACTION] = 'DELETE' AND [SESSION_ID] IS NULL)
                  BEGIN
                      INSERT INTO [dbo].[SYS_AUDIT_EVENT]
                          ([SESSION_ID], [SEQ], [USER_NAME], [AT_CLIENT], [AT_SERVER],
                           [CATEGORY], [SEVERITY], [ACTION],
                           [ENTITY], [ENTITY_KEY], [TITLE], [DETAIL], [IS_SUCCESS], [ERR_MSG])
                      SELECT NULL, NULL,
                             LEFT(u.[UserName], 50),
                             u.[ActionDateTime],
                             ISNULL(u.[ActionDateTime], SYSDATETIME()),
                             2, 3,
                             LEFT(u.[ActionType], 24),
                             LEFT(u.[TableName], 48),
                             LEFT(u.[RecordID], 80),
                             LEFT(ISNULL(u.[ActionType], N'') + N' — ' + ISNULL(u.[TableName], N'') + N' ' + ISNULL(u.[RecordID], N''), 250),
                             u.[AdditionalInfo],
                             u.[IsSuccess],
                             LEFT(u.[ErrorMessage], 400)
                      FROM [dbo].[USER_AUDIT_LOG] AS u;
                  END
              END",
        };

        /// <summary>
        /// ساخت ساختار در صورت نبودن. هیچ‌وقت استثنا پرتاب نمی‌کند: اگر
        /// ساختن جدول شکست بخورد، سیستم سابقه بی‌صدا غیرفعال می‌ماند و کار
        /// کاربر ادامه پیدا می‌کند.
        /// </summary>
        public static async Task<bool> EnsureCreatedAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return false;

            try
            {
                using var db = new SqlConnection(connectionString);
                await db.OpenAsync().ConfigureAwait(false);

                // مسیر سریع: از نسخه‌ی فعلی به بعد، ساختار توسط مایگریشن
                // ScriptSqly (متد AuditScript) ساخته می‌شود. این بوت‌استرپ
                // فقط برای نصب‌هایی می‌ماند که هنوز ScriptSqly به‌روز نشده.
                // اگر همه چیز از قبل هست، با یک رفت‌وبرگشت برگرد.
                var alreadyThere = await db.ExecuteScalarAsync<int>(
                    @"SELECT CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]',       N'U') IS NOT NULL
                                   AND OBJECT_ID(N'[dbo].[SYS_AUDIT_SESSION]',     N'U') IS NOT NULL
                                   AND OBJECT_ID(N'[dbo].[VW_SYS_AUDIT_TIMELINE]', N'V') IS NOT NULL
                                  THEN 1 ELSE 0 END").ConfigureAwait(false);

                if (alreadyThere == 1) return true;

                foreach (var sql in Statements)
                {
                    try
                    {
                        await db.ExecuteAsync(sql, commandTimeout: 180).ConfigureAwait(false);
                    }
                    catch (SqlException)
                    {
                        // یک دستور ممکن است روی نسخه/ادیشن خاصی رد شود؛
                        // بقیه باید اجرا شوند.
                    }
                }

                // همان سه شیئی که مسیر سریع بالا بررسی می‌کند. قبلاً فقط
                // جدول رویداد چک می‌شد، پس اگر ساختِ نما رد می‌شد (استثنایش
                // بالاتر عمداً بلعیده می‌شود) این متد true برمی‌گرداند و
                // SchemaReady درست می‌شد، ولی هر جست‌وجو در فرم سوابق با
                // «Invalid object name» شکست می‌خورد.
                var ok = await db.ExecuteScalarAsync<int>(
                    @"SELECT CASE WHEN OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]',       N'U') IS NOT NULL
                                   AND OBJECT_ID(N'[dbo].[SYS_AUDIT_SESSION]',     N'U') IS NOT NULL
                                   AND OBJECT_ID(N'[dbo].[VW_SYS_AUDIT_TIMELINE]', N'V') IS NOT NULL
                                  THEN 1 ELSE 0 END")
                    .ConfigureAwait(false);

                return ok == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
