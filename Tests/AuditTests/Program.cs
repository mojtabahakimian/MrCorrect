using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Prg_Proccessy.AUDIT;
using Prg_Proccessy.CNNMANAGER;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace AuditTests;

/// <summary>
/// تست‌های سیستم سابقه و ردیابی فعالیت کاربران.
///
/// بدون SQL Server هم اجرا می‌شود: موتور با یک کانکشن‌استرینگ نامعتبر
/// راه می‌افتد، پس نوشتن روی دیتابیس شکست می‌خورد و رویدادها روی دیسک
/// نگه داشته می‌شوند — و همان فایل‌ها بازرسی می‌شوند. این‌طور کل زنجیره‌ی
/// واقعی اجرا می‌شود: تشخیص SQL ← صف ← نخ پس‌زمینه ← سریال‌سازی ← فایل.
/// </summary>
internal static class Program
{
    private const string DeadConnection =
        "Server=127.0.0.1,1;Database=none;User Id=x;Password=y;Connect Timeout=1;TrustServerCertificate=True";

    private static int _pass, _fail;

    /// <summary>
    /// ضریب بودجه‌ی زمانی. روی ماشین اشتراکی CI هسته با کارهای دیگر
    /// رقابت می‌کند و اندازه‌گیری چند برابر نوسان دارد، پس بودجه شل‌تر
    /// گرفته می‌شود. عدد اندازه‌گیری‌شده در هر حالت چاپ می‌شود، بنابراین
    /// شل شدن بودجه جلوی دیدن رگرسیون واقعی را نمی‌گیرد.
    /// </summary>
    private static readonly double PerfFactor =
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase)
            ? 5.0 : 1.0;

    private static void Perf(string name, double measuredUs, double budgetUs)
    {
        var allowed = budgetUs * PerfFactor;
        Ok($"{name} — {measuredUs:N3}us (بودجه {allowed:N1}us)", measuredUs < allowed, $"{measuredUs:N3}us");
    }

    private static void Ok(string name, bool condition, string? detail = null)
    {
        if (condition) { _pass++; Console.WriteLine($"  PASS  {name}"); }
        else { _fail++; Console.WriteLine($"  FAIL  {name}   ->  {detail}"); }
    }

    private static void Section(string title) => Console.WriteLine($"\n=== {title} ===");

    private static string SpillDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MrCorrect", "AuditSpill");

    private static void ClearSpill()
    {
        try { if (Directory.Exists(SpillDir)) Directory.Delete(SpillDir, true); } catch { }
    }

    private static List<AuditEvent> ReadSpill()
    {
        var list = new List<AuditEvent>();
        if (!Directory.Exists(SpillDir)) return list;
        foreach (var f in Directory.EnumerateFiles(SpillDir, "*.jsonl"))
            foreach (var line in File.ReadAllLines(f))
                if (!string.IsNullOrWhiteSpace(line))
                {
                    // مثل خودِ موتور، خط خراب رد می‌شود — بعضی تست‌ها عمداً
                    // یک خط ناقص می‌سازند تا ثابت کنند انتقال را قفل نمی‌کند.
                    try
                    {
                        var e = JsonSerializer.Deserialize<AuditEvent>(line);
                        if (e != null) list.Add(e);
                    }
                    catch (JsonException)
                    {
                    }
                }
        return list;
    }

    private static async Task<int> Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("تست‌های سیستم سابقه و ردیابی فعالیت کاربران");

        ValidateMigrationSql();
        ValidatePersianDates();
        ValidateSetClauseParser();
        ValidateWriteFilter();
        ValidateReviewFixes();
        ValidateAdversarialSql();
        ValidateUserSwitch();
        await ValidateEndToEndAsync();
        await ValidatePerformanceAsync();
        ValidateResilience();
        await ValidateRealDatabaseAsync();
        await ValidateRealDatabaseHardAsync();
        await ValidateLegacyWritesAsync();
        await ValidateCommandListenerAsync();

        Console.WriteLine($"\n\nنتیجه:  موفق {_pass}  |  ناموفق {_fail}");
        return _fail == 0 ? 0 : 1;
    }

    // ── ۱) اسکریپت‌های مایگریشن با پارسر رسمی T-SQL ────────────────────
    private static void ValidateMigrationSql()
    {
        Section("اعتبارسنجی T-SQL مایگریشن (TSql150Parser = SQL Server 2019)");

        var i = 0;
        foreach (var sql in AuditSchema.Statements)
        {
            i++;
            var parser = new TSql150Parser(initialQuotedIdentifiers: true);
            parser.Parse(new StringReader(sql), out IList<ParseError> errors);

            var head = sql.TrimStart().Split('\n')[0].Trim();
            if (head.Length > 58) head = head[..58] + "...";

            Ok($"[{i:00}] {head}", errors.Count == 0,
                errors.Count > 0 ? $"خط {errors[0].Line}: {errors[0].Message}" : null);
        }
    }

    // ── ۲) تاریخ شمسی ─────────────────────────────────────────────────
    private static void ValidatePersianDates()
    {
        Section("تبدیل تاریخ شمسی");

        Ok("۹ سپتامبر ۲۰۲۶ → 14050618",
            Audit.ToPersianDateNumber(new DateTime(2026, 9, 9)) == 14050618);
        Ok("۲۱ مارس ۲۰۲۶ → 14050101 (نوروز)",
            Audit.ToPersianDateNumber(new DateTime(2026, 3, 21)) == 14050101);
        Ok("۳۱ دسامبر ۲۰۲۵ → 14041010",
            Audit.ToPersianDateNumber(new DateTime(2025, 12, 31)) == 14041010);
        Ok("ساعت عددی 142530",
            Audit.ToTimeNumber(new DateTime(2026, 1, 1, 14, 25, 30)) == 142530);

        var n = Audit.ToPersianDateNumber(new DateTime(2026, 9, 9));
        Ok("رفت‌وبرگشت شمسی → میلادی",
            Audit.FromPersianDateNumber(n)?.Date == new DateTime(2026, 9, 9));

        Ok("ماه ۱۳ رد می‌شود", Audit.FromPersianDateNumber(14051301) is null);
        Ok("عدد کوتاه رد می‌شود", Audit.FromPersianDateNumber(1405) is null);
        Ok("روز ۳۱ در ماه ۷ رد می‌شود", Audit.FromPersianDateNumber(14040731) is null);

        // کش تاریخ نباید بین روزها مقدار کهنه بدهد
        var wrong = 0;
        var days = new[] { new DateTime(2026, 9, 9), new DateTime(2026, 3, 21), new DateTime(2025, 12, 31) };
        var want = new[] { 14050618, 14050101, 14041010 };
        Parallel.For(0, 8, _ =>
        {
            for (var k = 0; k < 15_000; k++)
            {
                var j = k % 3;
                if (Audit.ToPersianDateNumber(days[j]) != want[j]) Interlocked.Increment(ref wrong);
            }
        });
        Ok("۱۲۰٬۰۰۰ فراخوانی از ۸ نخ روی ۳ روز مختلف — کش race ندارد", wrong == 0, wrong.ToString());
    }

    // ── ۳) پارسر بخش SET ──────────────────────────────────────────────
    private static void ValidateSetClauseParser()
    {
        Section("استخراج ستون‌های تغییرکرده از بخش SET");

        var method = typeof(AuditSqlSniffer)
            .GetMethod("BuildChangedColumns", BindingFlags.NonPublic | BindingFlags.Static)!;
        string? Run(string sql, object? p = null) => (string?)method.Invoke(null, new object?[] { sql, p });

        Ok("چند ستون ساده",
            Run("UPDATE dbo.HEAD_LST SET SHIFT = 3 ,CUST_KIND = 7 WHERE (NUMBER = 1234)")
              == "{\"changed\":{\"SHIFT\":\"3\",\"CUST_KIND\":\"7\"}}");

        Ok("کامای داخل رشته‌ی فارسی اشتباه split نمی‌شود",
            Run("UPDATE X SET MOLAH = N'تهران, خیابان الف' , MAS = 12 WHERE ID = 5")?
              .Contains("\"MAS\":\"12\"") == true);

        Ok("کامای داخل تابع",
            Run("UPDATE X SET A = ISNULL(B, 0), C = 9 WHERE ID = 1")?.Contains("\"C\":\"9\"") == true);

        var pwd = Run("UPDATE SALA_DTL SET PSAL_NAME = N'secret', SAL_NAME = N'ali' WHERE IDD = 5");
        Ok("رمز عبور ماسک می‌شود و مقدارش ذخیره نمی‌شود",
            pwd != null && pwd.Contains("***") && !pwd.Contains("secret"), pwd);

        var sub = Run("UPDATE X SET A = (SELECT TOP 1 v FROM Y), B = 3 WHERE ID = 1");
        Ok("زیرکوئری ذخیره نمی‌شود",
            sub != null && !sub.Contains("SELECT") && sub.Contains("\"B\":\"3\""), sub);

        Ok("مقدار پارامتری از شیء پارامترها خوانده می‌شود",
            Run("UPDATE X SET MOGODI = @M WHERE CODE = @C", new { M = 42.5, C = "K1" })?
              .Contains("42.5") == true);

        Ok("UPDATE بدون WHERE", Run("UPDATE X SET A = 1, B = 2")?.Contains("\"B\":\"2\"") == true);
        Ok("SELECT خالص چیزی برنمی‌گرداند", Run("SELECT A, B FROM X WHERE ID = 1") is null);
    }

    // ── ۴) غربال نوشتن/خواندن ─────────────────────────────────────────
    private static void ValidateReviewFixes()
    {
        Section("رگرسیون ایرادهای بازبینی");

        // متن فارسی در DETAIL نباید به \uXXXX تبدیل شود، وگرنه کاربر در فرم
        // سوابق به‌جای «بانک ملی» دنباله‌ی کد می‌بیند.
        var m = typeof(AuditSqlSniffer).GetMethod("BuildChangedColumns",
                    BindingFlags.NonPublic | BindingFlags.Static);
        var detail = m?.Invoke(null, new object?[]
        {
            "UPDATE dbo.TCOD_BANKS SET NAME = N'بانک ملی' WHERE CODE = 7", null
        }) as string;
        Ok("مقدار فارسی در DETAIL خوانا ذخیره می‌شود",
            detail != null && detail.Contains("بانک ملی") && !detail.Contains("\\u06"), detail);

        // ورود ناموفق باید به نام تلاش‌کننده ثبت شود، نه کاربر نشست
        ClearSpill();
        AuditService.Start(DeadConnection, 78, "Controller", "1.0", 1405, "DB");
        Audit.LoginFailed("hacker", "رمز عبور نادرست");
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data, Action = AuditAction.Update, Title = "عادی",
        });
        AuditService.ShutdownAsync(8000).GetAwaiter().GetResult();

        var ev = ReadSpill();
        var failed = ev.FirstOrDefault(e => e.Action == AuditAction.LoginFailed);
        Ok("ورود ناموفق به نام تلاش‌کننده ثبت شد، نه کاربر نشست",
            failed?.UserName == "hacker", failed?.UserName);
        Ok("رویداد عادی همچنان به کاربر نشست نسبت دارد",
            ev.Any(e => e.Action == AuditAction.Update && e.UserName == "Controller"));
        ClearSpill();
    }

    private static void ValidateWriteFilter()
    {
        Section("غربال نوشتن در برابر خواندن");
        Ok("SELECT نوشتنی نیست", !AuditSqlSniffer.LooksLikeWrite("SELECT * FROM HEAD_LST WHERE ID=1"));
        Ok("UPDATE نوشتنی است", AuditSqlSniffer.LooksLikeWrite("UPDATE X SET A=1"));
        Ok("EXEC نوشتنی است", AuditSqlSniffer.LooksLikeWrite("EXEC dbo.SP_PAY2_FINALIZE_RUN 1"));
        Ok("null امن است", !AuditSqlSniffer.LooksLikeWrite(null));
    }

    // ── ۵) مسیر کامل ──────────────────────────────────────────────────
    private static async Task ValidateEndToEndAsync()
    {
        Section("مسیر کامل: تشخیص SQL ← صف ← نخ پس‌زمینه ← فایل");

        ClearSpill();
        AuditService.Start(DeadConnection, 78, "Controller", "1.0.0.999", 1405, "YAZDSEPAR1405");
        Ok("موتور راه افتاد", AuditService.IsRunning);
        Ok("نشست ساخته شد", AuditService.CurrentSession is not null);

        AuditSqlSniffer.Observe("INSERT INTO dbo.HEAD_LST (NUMBER, TAG, CUST_NO) VALUES (1234, 20, N'C-5')");
        AuditSqlSniffer.Observe("UPDATE dbo.HEAD_LST SET MABL_HAZ = 135000 WHERE TAG = 20 AND NUMBER = 1234");
        AuditSqlSniffer.Observe("UPDATE HEAD_LST SET SGN1=0, SGN2=0, SGN3=1 WHERE TAG=20 AND NUMBER=1234");
        AuditSqlSniffer.Observe("DELETE d FROM dbo.DEED_DTL AS d WHERE N_S = 9090");
        AuditSqlSniffer.Observe("SELECT NAME FROM dbo.STUF_DEF WHERE CODE = @CODE", new { CODE = "K-1" });
        AuditSqlSniffer.Observe("INSERT INTO dbo.SYS_AUDIT_EVENT (TITLE) VALUES (N'x')");
        AuditSqlSniffer.Observe("EXEC dbo.SP_PAY2_FINALIZE_RUN @RunId = 5");
        AuditSqlSniffer.Observe("UPDATE dbo.STUF_STK SET MOGODI = @MOGODI WHERE CODE = @CODE AND ANBAR = @ANBAR",
                                new { MOGODI = 42.5, CODE = "K-100", ANBAR = 1 });
        AuditSqlSniffer.Observe("UPDATE dbo.PGET_HED SET N_S = 12 WHERE ID = 3; INSERT INTO dbo.DEED_DTL (N_S) VALUES (12);");
        AuditSqlSniffer.Observe("INSERT INTO dbo.X (A, B) SELECT A, B FROM Y");

        // شکل واقعی از ZASESABBEESAB.xaml.cs:609 — آنچه بعد از UPDATE آمده
        // نام مستعار است و باید از بند FROM به جدول واقعی باز شود.
        AuditSqlSniffer.Observe(
            "UPDATE vd SET CUST_NO = @ToHes " +
            "FROM dbo.HEAD_LST hl INNER JOIN dbo.VISITOR_DTL vd ON hl.NUMBER = vd.NUMBER " +
            "WHERE (vd.CUST_NO = @AzHes)", new { ToHes = "H-9", AzHes = "H-1" });

        // WHERE داخل زیرکوئری نباید کلید رکورد را بدزدد.
        AuditSqlSniffer.Observe(
            "UPDATE dbo.TCOD_BANKS SET NAME = N'بانک الف' " +
            "WHERE CODE = 77 AND ID IN (SELECT ID FROM dbo.PAY_GETD WHERE CODE = 999)");

        Audit.Form("HEAD_LST_PISHFROOSH2");
        Audit.Print("پیش‌فاکتور", entity: "HEAD_LST", entityKey: "NUMBER=1234;TAG=20", isPreview: false);
        Audit.Login("Controller");
        Audit.LoginFailed("hacker", "رمز عبور نادرست");
        AuditSqlSniffer.Observe("DELETE FROM dbo.HEAD_LST WHERE TAG = 20 AND NUMBER = 1234");

        await AuditService.ShutdownAsync(8000);
        await Task.Delay(300);

        var ev = ReadSpill();
        Ok("رویدادها نگه داشته شدند و گم نشدند", ev.Count > 0, ev.Count.ToString());
        Ok("JSON کامل round-trip می‌شود", ev.All(e => !string.IsNullOrEmpty(e.Action)));

        bool Has(Func<AuditEvent, bool> p) => ev.Any(p);

        Ok("امضا با وضعیت کاملِ هر سه خانه",
            Has(e => e.Action == AuditAction.Sign && e.Detail != null && e.Detail.Contains("\"3\":true")));
        Ok("برچسب فارسی «پیش‌فاکتور» از روی TAG=20",
            ev.FirstOrDefault(e => e.Action == AuditAction.Sign)?.Title?.Contains("پیش‌فاکتور") == true);
        Ok("ویرایش با مقدار جدید فیلد",
            Has(e => e.Action == AuditAction.Update && e.Detail != null && e.Detail.Contains("135000")));
        Ok("INSERT: کلید از VALUES استخراج شد",
            Has(e => e.Action == AuditAction.Insert && e.EntityKey == "NUMBER=1234;TAG=20"));
        Ok("INSERT ... SELECT رویداد می‌سازد ولی کلید ندارد",
            Has(e => e.Entity == "X" && e.Action == AuditAction.Insert && e.EntityKey is null));
        Ok("DELETE با alias: نام جدول درست است نه «d»",
            Has(e => e.Action == AuditAction.Delete && e.Entity == "DEED_DTL"));
        Ok("UPDATE با alias: نام جدول درست است نه «vd»",
            Has(e => e.Action == AuditAction.Update && e.Entity == "VISITOR_DTL"));
        Ok("UPDATE با alias: زیر نام مستعار ثبت نشد",
            !Has(e => e.Entity == "vd"));
        Ok("WHEREِ زیرکوئری کلید را ندزدید (CODE=77 نه 999)",
            ev.FirstOrDefault(e => e.Entity == "TCOD_BANKS")?.EntityKey == "CODE=77");
        Ok("SELECT هیچ رویدادی نساخت", !Has(e => e.Entity == "STUF_DEF"));
        Ok("جدول‌های خود سابقه رد شدند (بدون حلقه‌ی بی‌پایان)",
            !Has(e => e.Entity == "SYS_AUDIT_EVENT"));
        Ok("اجرای رویه‌ی حقوق ثبت شد",
            Has(e => e.Action == AuditAction.ExecProcedure && e.Entity == "SP_PAY2_FINALIZE_RUN"));
        Ok("کلید پارامتری به مقدار واقعی resolve شد",
            Has(e => e.Entity == "STUF_STK" && e.EntityKey != null && e.EntityKey.Contains("K-100")));
        Ok("دستور چندتکه: هر دو statement دیده شدند",
            Has(e => e.Entity == "PGET_HED") && Has(e => e.Entity == "DEED_DTL" && e.Action == AuditAction.Insert));
        Ok("باز کردن فرم", Has(e => e.Action == AuditAction.OpenForm));
        Ok("چاپ واقعی", Has(e => e.Action == AuditAction.Print));
        Ok("ورود", Has(e => e.Action == AuditAction.Login));
        Ok("ورود ناموفق با IsSuccess=false",
            Has(e => e.Action == AuditAction.LoginFailed && !e.IsSuccess));

        // «ورود ناموفق» عمداً به نام تلاش‌کننده ثبت می‌شود، نه کاربر نشست —
        // وگرنه تلاش ناموفق زیر نام کسی می‌نشست که بعداً موفق وارد شده.
        Ok("همه‌ی رویدادهای عادی به کاربر نشست نسبت دارند",
            ev.Where(e => e.Action != AuditAction.LoginFailed)
              .All(e => e.UserName == "Controller" && e.UserId == 78));
        Ok("ورود ناموفق به نام تلاش‌کننده ثبت شد",
            ev.Where(e => e.Action == AuditAction.LoginFailed)
              .All(e => e.UserName == "hacker"));
        Ok("همه به یک نشست وصل‌اند", ev.Select(e => e.SessionId).Distinct().Count() == 1);
        Ok("SEQ یکتا است", ev.Select(e => e.Seq).Distinct().Count() == ev.Count);
        Ok("تاریخ شمسی روی هر رویداد",
            ev.All(e => e.DateS > 14000000 && e.DateS < 15000000));
        Ok("رویدادهای حساس علامت بحرانی دارند",
            ev.Where(e => e.Action is AuditAction.Delete or AuditAction.Sign or AuditAction.Print
                                   or AuditAction.Login or AuditAction.LoginFailed)
              .All(e => e.IsCritical));

        var lifecycle = ev.Where(e => e.EntityKey == "NUMBER=1234;TAG=20").ToList();
        Ok("چرخه‌ی عمر کامل یک سند با یک کلید (ایجاد، ویرایش، امضا، چاپ، حذف)",
            lifecycle.Count >= 5, $"{lifecycle.Count} رویداد");

        Console.WriteLine("\n  نمونه‌ی خط زمانی — همان چیزی که در فرم دیده می‌شود:");
        foreach (var e in lifecycle.OrderBy(e => e.Seq))
            Console.WriteLine($"    {e.DateS}  {e.TimeS:000000}  {e.UserName,-11} {e.Title}");

        ClearSpill();
    }

    // ── ۶) کارایی و حافظه ─────────────────────────────────────────────
    private static async Task ValidatePerformanceAsync()
    {
        Section("کارایی و حافظه");

        var day = new DateTime(2026, 9, 9);
        for (var i = 0; i < 5000; i++) Audit.ToPersianDateNumber(day);
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 200_000; i++) Audit.ToPersianDateNumber(day);
        sw.Stop();
        var dateUs = sw.Elapsed.TotalMilliseconds * 1000.0 / 200_000;
        Perf("تبدیل تاریخ کش‌شده", dateUs, 1.0);

        ClearSpill();
        AuditService.Start(DeadConnection, 78, "Controller", "1.0", 1405, "DB");

        var big = "SELECT " + string.Join(", ", Enumerable.Range(0, 200).Select(i => "COL" + i))
                + " FROM dbo.HEAD_LST WHERE NUMBER = 1 AND LastUpdated > 0";
        for (var i = 0; i < 5000; i++) AuditSqlSniffer.Observe(big);
        sw.Restart();
        for (var i = 0; i < 50_000; i++) AuditSqlSniffer.Observe(big);
        sw.Stop();
        var readUs = sw.Elapsed.TotalMilliseconds * 1000.0 / 50_000;
        Perf("غربال مسیر خواندن", readUs, 10.0);

        for (var i = 0; i < 5000; i++)
            Audit.Write(new AuditEventDraft { Category = AuditCategory.Data, Action = AuditAction.Update, Title = "w" });
        sw.Restart();
        for (var i = 0; i < 100_000; i++)
            Audit.Write(new AuditEventDraft { Category = AuditCategory.Data, Action = AuditAction.Update, Title = "w" });
        sw.Stop();
        var writeUs = sw.Elapsed.TotalMilliseconds * 1000.0 / 100_000;
        Perf("هزینه‌ی هر ثبت روی نخ فراخوان", writeUs, 10.0);

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = GC.GetTotalMemory(true);
        for (var i = 0; i < 200_000; i++)
            Audit.Write(new AuditEventDraft { Category = AuditCategory.Navigation, Action = AuditAction.OpenForm, Title = "x" });
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var grewMb = (GC.GetTotalMemory(true) - before) / 1024.0 / 1024.0;
        Ok($"حافظه با دیتابیس قطع کران‌دار ماند ({grewMb:N1}MB)", grewMb < 60, $"{grewMb:N1}MB");
        Ok("رویدادهای مازاد شمرده شدند، بی‌صدا گم نشدند", AuditService.DroppedCount > 0);

        var seqMethod = typeof(AuditService).GetMethod("NextSeq", BindingFlags.NonPublic | BindingFlags.Static)!;
        var seqs = new System.Collections.Concurrent.ConcurrentBag<int>();
        Parallel.For(0, 8, _ =>
        {
            for (var i = 0; i < 500; i++) seqs.Add((int)seqMethod.Invoke(null, null)!);
        });
        Ok("۴۰۰۰ SEQ از ۸ نخ همه یکتا", seqs.Distinct().Count() == 4000, seqs.Distinct().Count().ToString());

        var errors = 0;
        Parallel.For(0, 8, _ =>
        {
            for (var i = 0; i < 300; i++)
            {
                try { AuditSqlSniffer.Observe($"UPDATE dbo.HEAD_LST SET MAS = {i} WHERE TAG = 2 AND NUMBER = {i}"); }
                catch { Interlocked.Increment(ref errors); }
            }
        });
        Ok("۲۴۰۰ فراخوانی همزمان بدون استثنا", errors == 0, errors.ToString());

        await AuditService.ShutdownAsync(6000);
        ClearSpill();
    }

    // ── ۷) مقاومت ─────────────────────────────────────────────────────
    private static void ValidateResilience()
    {
        Section("مقاومت در برابر ورودی خراب");

        string[] nasty =
        {
            "", "   ", "UPDATE", "UPDATE SET", "DELETE FROM", "INSERT INTO (",
            "UPDATE X SET A = 'ناتمام", "UPDATE X SET = 5 WHERE",
            new string('(', 500), "UPDATE " + new string('X', 5000) + " SET A=1",
            "INSERT INTO T (A,B) VALUES (1)", "INSERT INTO T VALUES (1,2)",
        };

        var thrown = 0;
        foreach (var s in nasty)
        {
            try { AuditSqlSniffer.Observe(s); } catch { thrown++; }
            try { Audit.Form(s); } catch { thrown++; }
            try { Audit.Data("UPDATE", s, s, s, s, null, null); } catch { thrown++; }
        }
        Ok($"{nasty.Length} ورودی خراب، هیچ استثنایی پرتاب نشد", thrown == 0, thrown.ToString());

        try { Audit.Write(null!); Ok("Write(null) امن است", true); }
        catch (Exception ex) { Ok("Write(null) امن است", false, ex.GetType().Name); }

        try { AuditSqlSniffer.Observe(null); Ok("Observe(null) امن است", true); }
        catch (Exception ex) { Ok("Observe(null) امن است", false, ex.GetType().Name); }
    }

    // ── ۸) تست واقعی روی SQL Server ───────────────────────────────────
    //
    // تنها بخشی که به دیتابیس واقعی نیاز دارد. با متغیر محیطی
    // AUDIT_TEST_SQL فعال می‌شود و اگر نباشد بی‌صدا رد می‌شود، تا اجرای
    // معمولی روی ماشین توسعه‌دهنده و CI بدون SQL Server هم کار کند.
    private static async Task ValidateRealDatabaseAsync()
    {
        var cs = Environment.GetEnvironmentVariable("AUDIT_TEST_SQL");
        if (string.IsNullOrWhiteSpace(cs))
        {
            Section("تست واقعی روی SQL Server — رد شد (AUDIT_TEST_SQL تنظیم نشده)");
            return;
        }

        Section("تست واقعی روی SQL Server");

        using var db = new SqlConnection(cs);
        await db.OpenAsync();

        var version = await db.ExecuteScalarAsync<string>("SELECT @@VERSION");
        Console.WriteLine("  " + (version ?? "").Split('\n')[0].Trim());

        // پاک‌سازی از اجرای قبلی
        await db.ExecuteAsync(@"
            IF OBJECT_ID(N'[dbo].[VW_SYS_AUDIT_TIMELINE]', N'V')  IS NOT NULL DROP VIEW [dbo].[VW_SYS_AUDIT_TIMELINE];
            IF OBJECT_ID(N'[dbo].[SYS_AUDIT_PURGE]',    N'P')  IS NOT NULL DROP PROCEDURE [dbo].[SYS_AUDIT_PURGE];
            IF OBJECT_ID(N'[dbo].[SYS_AUDIT_BACKFILL]', N'P')  IS NOT NULL DROP PROCEDURE [dbo].[SYS_AUDIT_BACKFILL];
            IF OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]',       N'U')  IS NOT NULL DROP TABLE [dbo].[SYS_AUDIT_EVENT];
            IF OBJECT_ID(N'[dbo].[SYS_AUDIT_SESSION]',     N'U')  IS NOT NULL DROP TABLE [dbo].[SYS_AUDIT_SESSION];");

        // جدول‌های قدیمی، با همان شکلی که در نرم‌افزار هستند. حضورشان ثابت
        // می‌کند مایگریشن کنارشان می‌نشیند و آن‌ها را خراب نمی‌کند.
        await db.ExecuteAsync(@"
            IF OBJECT_ID(N'[dbo].[TFORMS]', N'U') IS NULL
                CREATE TABLE [dbo].[TFORMS] (
                    FORMNAME NVARCHAR(64), CAPTION NVARCHAR(128),
                    kind INT, GRP INT, IDH INT, CRT DATETIME);
            IF OBJECT_ID(N'[dbo].[AMALIAT]', N'U') IS NULL
                CREATE TABLE [dbo].[AMALIAT] (
                    USERID NVARCHAR(20), USERNAME NVARCHAR(50),
                    ADATE DATETIME, AMALID NVARCHAR(64));
            DELETE FROM [dbo].[TFORMS];
            DELETE FROM [dbo].[AMALIAT];
            INSERT INTO [dbo].[TFORMS] (FORMNAME, CAPTION, kind, GRP, IDH, CRT)
                 VALUES (N'USERS', N'کاربران', 3, 16, 41, GETDATE());
            INSERT INTO [dbo].[AMALIAT] (USERID, USERNAME, ADATE, AMALID)
                 VALUES (N'78', N'Controller', GETDATE(), N'HEAD_LST_FROOSH22'),
                        (N'78', N'Controller', NULL,      N'DEED_HED_WIN');");

        // ── ۱) مایگریشن روی موتور واقعی ────────────────────────────────
        var created = await AuditSchema.EnsureCreatedAsync(cs);
        Ok("مایگریشن روی SQL Server واقعی اجرا شد", created);

        async Task<int> Exists(string name, string type) =>
            await db.ExecuteScalarAsync<int>(
                $"SELECT CASE WHEN OBJECT_ID(N'[dbo].[{name}]', N'{type}') IS NULL THEN 0 ELSE 1 END");

        Ok("جدول SYS_AUDIT_EVENT ساخته شد",       await Exists("SYS_AUDIT_EVENT", "U") == 1);
        Ok("جدول SYS_AUDIT_SESSION ساخته شد",     await Exists("SYS_AUDIT_SESSION", "U") == 1);
        Ok("نمای VW_SYS_AUDIT_TIMELINE ساخته شد", await Exists("VW_SYS_AUDIT_TIMELINE", "V") == 1);
        Ok("رویه‌ی SYS_AUDIT_PURGE ساخته شد",   await Exists("SYS_AUDIT_PURGE", "P") == 1);
        Ok("رویه‌ی SYS_AUDIT_BACKFILL ساخته شد", await Exists("SYS_AUDIT_BACKFILL", "P") == 1);

        var idx = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]') AND name LIKE 'IX_%'");
        Ok("هر ۵ ایندکس ساخته شدند", idx == 5, idx.ToString());

        var seqKey = await db.ExecuteScalarAsync<int>(
            @"SELECT ISNULL(MAX(CAST(optimize_for_sequential_key AS INT)), 0) FROM sys.indexes
               WHERE object_id = OBJECT_ID(N'[dbo].[SYS_AUDIT_EVENT]') AND is_primary_key = 1");
        Ok("OPTIMIZE_FOR_SEQUENTIAL_KEY روی PK فعال شد", seqKey == 1);

        // نام رویه‌ها نباید با SP_ شروع شود. SQL Server هر نامی که با sp_
        // آغاز شود اول در master جست‌وجو می‌کند، نه در دیتابیس جاری. اگر
        // نسخه‌ای از همان نام در master باشد، ساخت رویه در دیتابیس کاربر با
        // «Invalid object name» شکست می‌خورد و در زمان اجرا هم EXEC ممکن است
        // نسخه‌ی master را صدا بزند. این تست همان شرایط را بازمی‌سازد.
        var badPrefix = await db.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM sys.procedures
               WHERE name LIKE 'SP[_]%' AND name LIKE '%AUDIT%'");
        Ok("هیچ رویه‌ی سابقه با پیشوند رزرو شده‌ی SP_ ساخته نشد", badPrefix == 0, badPrefix.ToString());

        var reg = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[TFORMS] WHERE FORMNAME = N'AUDITTRAIL'");
        Ok("فرم AUDITTRAIL در TFORMS ثبت شد", reg == 1, reg.ToString());

        var grp = await db.ExecuteScalarAsync<int?>(
            "SELECT GRP FROM [dbo].[TFORMS] WHERE FORMNAME = N'AUDITTRAIL'");
        Ok("گروه فرم از روی USERS برداشته شد", grp == 16, grp?.ToString());

        // اجرای دوباره نباید چیزی را خراب کند یا رکورد تکراری بسازد
        var again = await AuditSchema.EnsureCreatedAsync(cs);
        var reg2 = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[TFORMS] WHERE FORMNAME = N'AUDITTRAIL'");
        Ok("مایگریشن idempotent است (اجرای دوم بی‌ضرر)", again && reg2 == 1, reg2.ToString());

        // ── ۲) موتور، سرتاسر، روی دیتابیس واقعی ────────────────────────
        ClearSpill();
        AuditService.Start(cs, 0, "-", "1.0.0.999", 1405, "AuditRealTest");
        AuditService.AttachUser(78, "Controller", 1405, "1.0.0.999");

        AuditSqlSniffer.Observe("INSERT INTO dbo.HEAD_LST (NUMBER, TAG, CUST_NO) VALUES (1234, 20, N'C-5')");
        AuditSqlSniffer.Observe("UPDATE dbo.HEAD_LST SET MABL_HAZ = 135000 WHERE TAG = 20 AND NUMBER = 1234");
        AuditSqlSniffer.Observe("UPDATE HEAD_LST SET SGN1=0, SGN2=0, SGN3=1 WHERE TAG=20 AND NUMBER=1234");
        AuditSqlSniffer.Observe(
            "UPDATE vd SET CUST_NO = @ToHes FROM dbo.HEAD_LST hl " +
            "INNER JOIN dbo.VISITOR_DTL vd ON hl.NUMBER = vd.NUMBER WHERE (vd.CUST_NO = @AzHes)",
            new { ToHes = "H-9", AzHes = "H-1" });
        AuditSqlSniffer.Observe("UPDATE dbo.SALA_DTL SET PSAL_NAME = N'secret123' WHERE IDD = 5");
        AuditSqlSniffer.Observe("SELECT NAME FROM dbo.STUF_DEF WHERE CODE = @CODE", new { CODE = "K-1" });
        Audit.Form("HEAD_LST_PISHFROOSH2");
        Audit.Print("پیش‌فاکتور", entity: "HEAD_LST", entityKey: "NUMBER=1234;TAG=20", isPreview: false);
        Audit.LoginFailed("hacker", "رمز عبور نادرست");
        AuditSqlSniffer.Observe("DELETE FROM dbo.HEAD_LST WHERE TAG = 20 AND NUMBER = 1234");

        await AuditService.ShutdownAsync(15000);

        var spilled = ReadSpill().Count;
        Ok("هیچ رویدادی به دیسک نریخت (یعنی همه در DB نوشته شدند)", spilled == 0, $"{spilled} روی دیسک");

        var total = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT]");
        Ok("رویدادها واقعاً در جدول درج شدند", total > 0, total.ToString());
        Ok("شمارنده‌ی نوشته‌شده با جدول می‌خواند",
            AuditService.WrittenCount == total, $"{AuditService.WrittenCount} در برابر {total}");

        var rows = (await db.QueryAsync(
            "SELECT [ACTION],[ENTITY],[ENTITY_KEY],[TITLE],[DETAIL],[SEVERITY],[USER_NAME],[IS_SUCCESS],[DATE_S] " +
            "FROM [dbo].[SYS_AUDIT_EVENT]")).ToList();

        bool Row(Func<dynamic, bool> p) => rows.Any(p);

        Ok("درج فاکتور با کلید درست", Row(r => r.ACTION == "INSERT" && r.ENTITY == "HEAD_LST" && r.ENTITY_KEY == "NUMBER=1234;TAG=20"));
        Ok("ویرایش با مقدار جدید در DETAIL", Row(r => r.ACTION == "UPDATE" && r.DETAIL != null && ((string)r.DETAIL).Contains("135000")));
        Ok("امضا با وضعیت هر سه خانه", Row(r => r.ACTION == "SIGN" && r.DETAIL != null && ((string)r.DETAIL).Contains("\"3\":true")));
        Ok("برچسب فارسی «پیش‌فاکتور» در TITLE", Row(r => r.TITLE != null && ((string)r.TITLE).Contains("پیش‌فاکتور")));
        Ok("UPDATE با alias زیر نام جدول واقعی ثبت شد", Row(r => r.ENTITY == "VISITOR_DTL"));
        Ok("SELECT هیچ رویدادی نساخت", !Row(r => r.ENTITY == "STUF_DEF"));
        Ok("حذف ثبت شد", Row(r => r.ACTION == "DELETE" && r.ENTITY == "HEAD_LST"));
        Ok("چاپ ثبت شد", Row(r => r.ACTION == "PRINT"));
        Ok("ورود ناموفق با IS_SUCCESS=0", Row(r => r.ACTION == "LOGIN_FAILED" && r.IS_SUCCESS == false));
        Ok("رویدادهای حساس SEVERITY=3", Row(r => r.ACTION == "DELETE" && r.SEVERITY == (byte)3));
        Ok("تاریخ شمسی درست ذخیره شد", rows.All(r => r.DATE_S > 14000000 && r.DATE_S < 15000000));

        var pwd = rows.FirstOrDefault(r => r.ENTITY == "SALA_DTL");
        Ok("ستون رمز عبور ماسک شد و مقدار واقعی ذخیره نشد",
            pwd != null && (pwd.DETAIL == null || !((string)pwd.DETAIL).Contains("secret123")),
            pwd?.DETAIL as string);

        // ── ۳) سطر نشست و نام کاربر ────────────────────────────────────
        var sess = (await db.QueryAsync(
            "SELECT [USER_ID],[USER_NAME],[MACHINE_NAME],[CLIENT_IP],[APP_VERSION],[FISCAL_YEAR] FROM [dbo].[SYS_AUDIT_SESSION]")).ToList();
        Ok("سطر نشست ساخته شد", sess.Count == 1, sess.Count.ToString());
        Ok("نام کاربر روی نشست نشست (رقابت زمانی رفع شده)",
            sess.Count == 1 && (string?)sess[0].USER_NAME == "Controller", sess.Count == 1 ? (string?)sess[0].USER_NAME : null);
        Ok("سال مالی روی نشست ثبت شد", sess.Count == 1 && sess[0].FISCAL_YEAR == 1405);
        Ok("نام کامپیوتر ثبت شد", sess.Count == 1 && !string.IsNullOrWhiteSpace((string?)sess[0].MACHINE_NAME));

        // ── ۴) نما و کوئری واقعیِ فرم گزارش ────────────────────────────
        var viewCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[VW_SYS_AUDIT_TIMELINE]");
        Ok("نما همان تعداد سطر را برمی‌گرداند", viewCount == total, $"{viewCount} در برابر {total}");

        // هر سطر باید اطلاعات نشست را کنار خودش داشته باشد. سنجه MACHINE_NAME
        // است نه USER_NAME، چون «ورود ناموفق» عمداً نام دیگری دارد.
        var joined = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[VW_SYS_AUDIT_TIMELINE] WHERE [MACHINE_NAME] IS NOT NULL");
        Ok("نما رویداد را به نشست وصل می‌کند", joined == total, $"{joined} در برابر {total}");

        var attempted = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[VW_SYS_AUDIT_TIMELINE] WHERE [ACTION] = 'LOGIN_FAILED' AND [USER_NAME] = N'hacker'");
        Ok("نما ورود ناموفق را به کاربر موفق نسبت نمی‌دهد", attempted == 1, attempted.ToString());

        // دقیقاً همان SQL فرم WIN_AUDIT_TRAIL
        const string viewerSql = @"
SELECT TOP (@Take)
       [LOG_ID], [AT_CLIENT], [AT_SERVER], [DATE_S], [TIME_S],
       [USER_ID], [USER_NAME], [CATEGORY], [SEVERITY], [ACTION],
       [ENTITY], [ENTITY_KEY], [FORM_NAME], [TITLE], [DETAIL],
       [IS_SUCCESS], [SESSION_ID], [MACHINE_NAME], [CLIENT_IP],
       [WIN_USER], [APP_VERSION]
  FROM [dbo].[VW_SYS_AUDIT_TIMELINE]
 WHERE [AT_SERVER] >= @From
   AND [AT_SERVER] <  @To
   AND (@UserId   IS NULL OR [USER_ID]  = @UserId)
   AND (@Category IS NULL OR [CATEGORY] = @Category)
   AND (@Action   IS NULL OR [ACTION]   = @Action)
   AND (@Doc      IS NULL OR [ENTITY_KEY] LIKE @Doc OR [ENTITY] LIKE @Doc)
   AND (@Search   IS NULL OR [TITLE]      LIKE @Search)
   AND (@OnlySensitive = 0 OR [SEVERITY] = 3)
   AND (@AfterId  IS NULL OR [LOG_ID] < @AfterId)
 ORDER BY [LOG_ID] DESC
 OPTION (RECOMPILE)";

        object Args(object? doc = null, object? afterId = null, bool sensitive = false, object? userId = null) => new
        {
            Take = 50,
            From = DateTime.Today.AddDays(-1),
            To = DateTime.Today.AddDays(1),
            UserId = (int?)userId,
            Category = (byte?)null,
            Action = (string?)null,
            Doc = (string?)doc,
            Search = (string?)null,
            OnlySensitive = sensitive,
            AfterId = (long?)afterId,
        };

        var page = (await db.QueryAsync(viewerSql, Args())).ToList();
        Ok("کوئری واقعی فرم گزارش اجرا شد", page.Count > 0, page.Count.ToString());
        Ok("مرتب‌سازی نزولی است", page.Count < 2 || page[0].LOG_ID > page[1].LOG_ID);

        var docPage = (await db.QueryAsync(viewerSql, Args(doc: "%NUMBER=1234%"))).ToList();
        Ok("جست‌وجوی «تاریخچه‌ی این سند» چرخه‌ی عمر را می‌آورد", docPage.Count >= 4, docPage.Count.ToString());
        var acts = docPage.Select(r => (string)r.ACTION).ToHashSet();
        Ok("چرخه‌ی عمر شامل ثبت، ویرایش، امضا، چاپ و حذف است",
            acts.IsSupersetOf(new[] { "INSERT", "UPDATE", "SIGN", "PRINT", "DELETE" }),
            string.Join(",", acts));

        var sens = (await db.QueryAsync(viewerSql, Args(sensitive: true))).ToList();
        Ok("فیلتر «فقط حساس» کار می‌کند", sens.Count > 0 && sens.All(r => r.SEVERITY == (byte)3), sens.Count.ToString());

        var noUser = (await db.QueryAsync(viewerSql, Args(userId: 999999))).ToList();
        Ok("فیلتر کاربرِ ناموجود خالی برمی‌گرداند", noUser.Count == 0);

        // صفحه‌بندی keyset
        var firstId = (long)page[0].LOG_ID;
        var next = (await db.QueryAsync(viewerSql, Args(afterId: firstId))).ToList();
        Ok("صفحه‌بندی keyset سطر تکراری نمی‌دهد", next.All(r => (long)r.LOG_ID < firstId));

        // ── ۵) رویه‌ها ─────────────────────────────────────────────────
        await db.ExecuteAsync("EXEC [dbo].[SYS_AUDIT_BACKFILL]", commandTimeout: 120);
        var backfilled = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [SESSION_ID] IS NULL AND [ACTION] = 'OPEN_FORM'");
        Ok("انتقال سابقه‌ی قدیمی از AMALIAT انجام شد", backfilled == 2, backfilled.ToString());
        Ok("سطر با ADATE خالی هم منتقل شد (ISNULL کار کرد)",
            await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [FORM_NAME] = N'DEED_HED_WIN'") == 1);

        await db.ExecuteAsync("EXEC [dbo].[SYS_AUDIT_BACKFILL]", commandTimeout: 120);
        var backfilled2 = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [SESSION_ID] IS NULL AND [ACTION] = 'OPEN_FORM'");
        Ok("انتقال دوباره رکورد تکراری نمی‌سازد", backfilled2 == 2, backfilled2.ToString());

        var beforePurge = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT]");
        await db.ExecuteAsync("EXEC [dbo].[SYS_AUDIT_PURGE]", commandTimeout: 120);
        var afterPurge = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT]");
        Ok("پاک‌سازی رویدادهای تازه را حذف نمی‌کند", afterPurge == beforePurge, $"{beforePurge} → {afterPurge}");

        // ── ۶) جدول‌های قدیمی سالم مانده‌اند ───────────────────────────
        Ok("AMALIAT دست‌نخورده ماند",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[AMALIAT]") == 2);
        Ok("TFORMS خراب نشد",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[TFORMS] WHERE FORMNAME = N'USERS'") == 1);

        ClearSpill();
    }


    // ── ۹) مسیرهای سخت روی SQL Server واقعی ───────────────────────────
    //
    // آنچه مسیر خوش‌بینانه به آن نمی‌رسد: مقدار بلندتر از ستون، انتقال
    // رویدادهای روی دیسک به دیتابیس، پاک‌سازی واقعی، حجم بالا و همزمانی.
    private static async Task ValidateRealDatabaseHardAsync()
    {
        var cs = Environment.GetEnvironmentVariable("AUDIT_TEST_SQL");
        if (string.IsNullOrWhiteSpace(cs)) return;

        Section("مسیرهای سخت روی SQL Server واقعی");

        using var db = new SqlConnection(cs);
        await db.OpenAsync();
        await db.ExecuteAsync("TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT]; DELETE FROM [dbo].[SYS_AUDIT_SESSION];");

        // ── ۱) مقدار بلندتر از عرض ستون نباید کل دسته را از بین ببرد ───
        ClearSpill();
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "AuditRealTest");

        var longKey   = new string('K', 500);
        var longTitle = new string('ت', 4000);
        var longEnt   = new string('E', 300);

        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data, Action = AuditAction.Update,
            Title = longTitle, Entity = longEnt, EntityKey = longKey,
            FormName = new string('F', 300),
        });
        // یک رویداد سالم پشت سرش: اگر دسته به‌خاطر رویداد قبلی بترکد، این هم گم می‌شود.
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data, Action = AuditAction.Insert,
            Title = "سطر سالم پس از سطر بلند", Entity = "CANARY", EntityKey = "ID=1",
        });

        await AuditService.ShutdownAsync(15000);

        var longRow = (await db.QueryAsync(
            "SELECT [ENTITY],[ENTITY_KEY],[TITLE],[FORM_NAME] FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] LIKE 'EEE%'")).ToList();
        Ok("مقدار بلند بریده شد و درج شد (نه خطای 8152)", longRow.Count == 1, longRow.Count.ToString());
        if (longRow.Count == 1)
        {
            Ok("ENTITY به ۴۸ بریده شد",     ((string)longRow[0].ENTITY).Length == 48);
            Ok("ENTITY_KEY به ۸۰ بریده شد", ((string)longRow[0].ENTITY_KEY).Length == 80);
            Ok("TITLE به ۲۵۰ بریده شد",     ((string)longRow[0].TITLE).Length == 250);
        }
        Ok("رویداد سالمِ پشت سر آن گم نشد",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] = N'CANARY'") == 1);

        // ── ۲) فارسی: round-trip دقیق، نه فقط Contains ─────────────────
        await db.ExecuteAsync("TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");
        const string persian = "ویرایش پیش‌فاکتور «تست» — ۱۴۰۵/۰۵/۱۷ ﷼";
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "AuditRealTest");
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data, Action = AuditAction.Update,
            Title = persian, Entity = "HEAD_LST", EntityKey = "NUMBER=1",
            Detail = "{\"نام\":\"علی\"}",
        });
        await AuditService.ShutdownAsync(15000);

        var back = await db.ExecuteScalarAsync<string>("SELECT TOP 1 [TITLE] FROM [dbo].[SYS_AUDIT_EVENT]");
        Ok("متن فارسی بدون تغییر برگشت (NVARCHAR درست)", back == persian, back);
        var detail = await db.ExecuteScalarAsync<string>("SELECT TOP 1 [DETAIL] FROM [dbo].[SYS_AUDIT_EVENT]");
        Ok("DETAIL فارسی سالم برگشت", detail == "{\"نام\":\"علی\"}", detail);

        // ── ۳) انتقال رویدادهای روی دیسک به دیتابیس ────────────────────
        // این مسیر تا حالا هرگز روی دیتابیس واقعی اجرا نشده بود: رویداد
        // حساسی که موتور خاموش بوده روی دیسک می‌نشیند و باید در اجرای بعدی
        // به دیتابیس منتقل شود.
        await db.ExecuteAsync("TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");
        ClearSpill();

        // موتور خاموش است → رویداد حساس باید روی دیسک بنشیند
        Audit.Delete("HEAD_LST", "NUMBER=555;TAG=2", "حذف فاکتور فروش ۵۵۵");
        Audit.Sign("HEAD_LST", "NUMBER=556;TAG=20", 1, true, persianTitle: "امضای پیش‌فاکتور ۵۵۶");
        var onDisk = ReadSpill().Count;
        Ok("رویداد حساس با موتور خاموش روی دیسک نشست", onDisk == 2, onDisk.ToString());

        // یک خط خراب هم وسطش می‌گذاریم: نباید انتقال را قفل کند
        var spillFile = Directory.EnumerateFiles(SpillDir, "*.jsonl").First();
        File.AppendAllText(spillFile, "{این JSON معتبر نیست\n");

        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "AuditRealTest");
        for (var i = 0; i < 40 && ReadSpill().Count > 0; i++) await Task.Delay(500);
        await AuditService.ShutdownAsync(15000);

        var replayed = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY_KEY] IN (N'NUMBER=555;TAG=2', N'NUMBER=556;TAG=20')");
        Ok("رویدادهای روی دیسک به دیتابیس منتقل شدند", replayed == 2, replayed.ToString());
        Ok("خط خراب انتقال را قفل نکرد (فایل پاک شد)", ReadSpill().Count == 0, ReadSpill().Count.ToString());

        // ── ۴) پاک‌سازی واقعاً حذف می‌کند ──────────────────────────────
        await db.ExecuteAsync("TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");
        await db.ExecuteAsync(@"
            INSERT INTO [dbo].[SYS_AUDIT_EVENT] ([AT_SERVER],[CATEGORY],[SEVERITY],[ACTION],[TITLE]) VALUES
                (DATEADD(DAY, -120,  SYSDATETIME()), 1, 1, 'OPEN_FORM', N'ناوبری قدیمی'),
                (DATEADD(DAY, -30,   SYSDATETIME()), 1, 1, 'OPEN_FORM', N'ناوبری تازه'),
                (DATEADD(DAY, -120,  SYSDATETIME()), 2, 3, 'DELETE',    N'حذف نسبتاً قدیمی'),
                (DATEADD(YEAR, -6,   SYSDATETIME()), 2, 3, 'DELETE',    N'حذف خیلی قدیمی');");

        await db.ExecuteAsync("EXEC [dbo].[SYS_AUDIT_PURGE]", commandTimeout: 120);

        Ok("ناوبری قدیمی‌تر از ۹۰ روز حذف شد",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [TITLE] = N'ناوبری قدیمی'") == 0);
        Ok("ناوبری تازه‌تر از ۹۰ روز ماند",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [TITLE] = N'ناوبری تازه'") == 1);
        Ok("حذفِ ۱۲۰ روزه ماند (نگهداری ۵ سال)",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [TITLE] = N'حذف نسبتاً قدیمی'") == 1);
        Ok("حذفِ ۶ ساله پاک شد",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [TITLE] = N'حذف خیلی قدیمی'") == 0);

        // ── ۵) حجم بالا و همزمانی روی دیتابیس واقعی ────────────────────
        await db.ExecuteAsync("TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT]; DELETE FROM [dbo].[SYS_AUDIT_SESSION];");
        ClearSpill();
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "AuditRealTest");

        const int threads = 8, perThread = 750;   // ۶۰۰۰ رویداد، چند برابر ظرفیت یک دسته
        var sw = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0, threads).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < perThread; i++)
                Audit.Write(new AuditEventDraft
                {
                    Category = AuditCategory.Data, Action = AuditAction.Update,
                    Title = $"بار سنگین {t}-{i}", Entity = "LOADTEST", EntityKey = $"T={t};I={i}",
                });
        })));
        var enqueueMs = sw.Elapsed.TotalMilliseconds;
        await AuditService.ShutdownAsync(60000);
        sw.Stop();

        var landed = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] = N'LOADTEST'");
        var expected = threads * perThread;
        Ok($"هر {expected} رویداد از {threads} نخ در دیتابیس نشست",
            landed + AuditService.DroppedCount >= expected && landed > 0,
            $"{landed} درج، {AuditService.DroppedCount} دورریز");
        Ok("هیچ رویدادی بی‌حساب گم نشد",
            landed + AuditService.DroppedCount + ReadSpill().Count >= expected,
            $"{landed}+{AuditService.DroppedCount}+{ReadSpill().Count} در برابر {expected}");
        Ok("کلیدها یکتا ماندند (بدون تداخل نخ‌ها)",
            await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(DISTINCT [ENTITY_KEY]) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] = N'LOADTEST'") == landed);
        Ok("SEQ در سطح دیتابیس یکتا ماند",
            await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) - COUNT(DISTINCT [SEQ]) FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] = N'LOADTEST'") == 0);

        Console.WriteLine($"  زمان صف‌کردن {expected} رویداد روی نخ فراخوان: {enqueueMs:N0}ms " +
                          $"({enqueueMs * 1000 / expected:N2}us هر کدام) — کل تا تخلیه: {sw.Elapsed.TotalSeconds:N1}s");

        ClearSpill();
    }


    // ── ۱۰) نوشتن در جدول‌های قدیمی ───────────────────────────────────
    //
    // خواسته‌ی صریح: جدول‌های قدیمی حفظ شوند و همچنان پر شوند. تا حالا فقط
    // ثابت شده بود که خراب نمی‌شوند و جهت برعکس (انتقال) کار می‌کند؛ خودِ
    // نوشتن هرگز روی دیتابیس واقعی اجرا نشده بود.
    private static async Task ValidateLegacyWritesAsync()
    {
        var cs = Environment.GetEnvironmentVariable("AUDIT_TEST_SQL");
        if (string.IsNullOrWhiteSpace(cs)) return;

        Section("نوشتن در جدول‌های قدیمی");

        using var db = new SqlConnection(cs);
        await db.OpenAsync();

        // USER_AUDIT_LOG با همان شکل واقعی، شامل ستون‌های NOT NULL
        await db.ExecuteAsync(@"
            IF OBJECT_ID(N'[dbo].[USER_AUDIT_LOG]', N'U') IS NOT NULL DROP TABLE [dbo].[USER_AUDIT_LOG];
            CREATE TABLE [dbo].[USER_AUDIT_LOG] (
                [ID] INT IDENTITY(1,1) PRIMARY KEY,
                [UserName] NVARCHAR(100) NOT NULL,
                [WindowsUserName] NVARCHAR(100) NULL,
                [ActionType] NVARCHAR(50) NOT NULL,
                [TableName] NVARCHAR(100) NOT NULL,
                [RecordID] NVARCHAR(100) NULL,
                [OldValue] NVARCHAR(MAX) NULL,
                [NewValue] NVARCHAR(MAX) NULL,
                [IPAddress] NVARCHAR(50) NULL,
                [MachineName] NVARCHAR(100) NULL,
                [ApplicationVersion] NVARCHAR(50) NULL,
                [WindowsVersion] NVARCHAR(100) NULL,
                [ActionDateTime] DATETIME NULL,
                [AdditionalInfo] NVARCHAR(MAX) NULL,
                [SessionID] UNIQUEIDENTIFIER NULL,
                [ProcessID] INT NULL,
                [ThreadID] INT NULL,
                [StackTrace] NVARCHAR(MAX) NULL,
                [IsSuccess] BIT NULL,
                [ErrorMessage] NVARCHAR(MAX) NULL);
            DELETE FROM [dbo].[AMALIAT];
            TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");

        ClearSpill();
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "MRC_AUDIT_TEST");

        Audit.Form("HEAD_LST_PISHFROOSH2");
        Audit.Form("DEED_HED_WIN");

        // همان کاری که شیم AuditLogger در Prg_UI می‌کند
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data,
            Action = AuditAction.Delete,
            Entity = "HEAD_LST",
            EntityKey = "NUMBER=1234;TAG=20",
            Title = "حذف پیش‌فاکتور ۱۲۳۴",
            Legacy = AuditLegacyTarget.UserAuditLog,
            LegacyOldValue = "{\"MABL\":\"135000\"}",
            IsCritical = true,
        });

        // رویداد بدون کاربر و بدون موجودیت: ستون‌های NOT NULL جدول قدیمی
        // نباید کل دسته را رد کنند.
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data,
            Action = AuditAction.Update,
            Title = "بدون موجودیت",
            Legacy = AuditLegacyTarget.UserAuditLog,
        });

        await AuditService.ShutdownAsync(15000);

        var amaliat = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[AMALIAT]");
        Ok("باز کردن فرم همچنان در AMALIAT نوشته می‌شود", amaliat == 2, amaliat.ToString());

        var amalId = await db.ExecuteScalarAsync<string>(
            "SELECT TOP 1 [AMALID] FROM [dbo].[AMALIAT] WHERE [AMALID] = N'HEAD_LST_PISHFROOSH2'");
        Ok("نام فرم در AMALIAT درست است", amalId == "HEAD_LST_PISHFROOSH2", amalId);

        var ual = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[USER_AUDIT_LOG]");
        Ok("USER_AUDIT_LOG همچنان پر می‌شود", ual == 2, ual.ToString());

        var del = (await db.QueryAsync(
            "SELECT [UserName],[ActionType],[TableName],[RecordID],[OldValue],[MachineName],[IsSuccess] " +
            "FROM [dbo].[USER_AUDIT_LOG] WHERE [ActionType] = 'DELETE'")).ToList();
        Ok("سطر حذف با همه‌ی ستون‌ها نوشته شد", del.Count == 1, del.Count.ToString());
        if (del.Count == 1)
        {
            Ok("نام جدول و شناسه‌ی رکورد درست است",
                (string?)del[0].TableName == "HEAD_LST" && (string?)del[0].RecordID == "NUMBER=1234;TAG=20");
            Ok("مقدار قبلی ذخیره شد", ((string?)del[0].OldValue)?.Contains("135000") == true);
            Ok("اطلاعات نشست روی سطر قدیمی هم می‌نشیند",
                !string.IsNullOrWhiteSpace((string?)del[0].MachineName));
        }

        Ok("رویداد بدون موجودیت هم نوشته شد (NOT NULL دسته را رد نکرد)",
            await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM [dbo].[USER_AUDIT_LOG] WHERE [ActionType] = 'UPDATE'") == 1);

        // مهم‌ترین بخش: جریان اصلی نباید به جدول قدیمی وابسته باشد
        var main = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT]");
        Ok("همه‌ی رویدادها در جریان اصلی هم هستند (نوشتن دوگانه)", main == 4, main.ToString());

        // اگر جدول قدیمی اصلاً نباشد، جریان اصلی باید سالم بماند
        await db.ExecuteAsync("DROP TABLE [dbo].[USER_AUDIT_LOG]; TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "MRC_AUDIT_TEST");
        Audit.Write(new AuditEventDraft
        {
            Category = AuditCategory.Data, Action = AuditAction.Delete,
            Entity = "HEAD_LST", EntityKey = "NUMBER=1", Title = "حذف",
            Legacy = AuditLegacyTarget.UserAuditLog, IsCritical = true,
        });
        await AuditService.ShutdownAsync(15000);
        Ok("نبودِ جدول قدیمی جریان اصلی را از کار نمی‌اندازد",
            await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[SYS_AUDIT_EVENT]") == 1);
        Ok("و رویداد روی دیسک هم نریخت", ReadSpill().Count == 0, ReadSpill().Count.ToString());

        ClearSpill();
    }


    // ── ۱۱) شنونده‌ی مرکزی: نوشتن‌هایی که قلاب‌ها نمی‌دیدند ────────────
    //
    // ۷۵ نقطه در Prg_UI مستقیم روی SqlConnection خام با Dapper می‌نویسند —
    // از جمله ساخت و ویرایش فاکتور و پیش‌فاکتور و سند. هیچ‌کدام از مسیرهای
    // قلاب‌خورده عبور نمی‌کنند. این بخش همان الگو را بازمی‌سازد.
    private static async Task ValidateCommandListenerAsync()
    {
        var cs = Environment.GetEnvironmentVariable("AUDIT_TEST_SQL");
        if (string.IsNullOrWhiteSpace(cs)) return;

        Section("شنونده‌ی مرکزی دستورهای SQL");

        using var probe = new SqlConnection(cs);
        await probe.OpenAsync();
        await probe.ExecuteAsync(@"
            IF OBJECT_ID(N'[dbo].[HEAD_LST]', N'U') IS NULL
                CREATE TABLE [dbo].[HEAD_LST] (NUMBER INT, TAG INT, CUST_NO NVARCHAR(20), MABL_HAZ FLOAT);
            DELETE FROM [dbo].[HEAD_LST];
            TRUNCATE TABLE [dbo].[SYS_AUDIT_EVENT];");

        ClearSpill();
        AuditService.Start(cs, 78, "Controller", "1.0", 1405, "MRC_AUDIT_TEST");
        AuditService.AttachUser(78, "Controller", 1405, "1.0");

        // دقیقاً الگوی HEAD_LST_PISHFROOSH2: کانکشن خام + تراکنش + Dapper
        using (var db = new SqlConnection(cs))
        {
            db.Open();
            using (var tx = db.BeginTransaction())
            {
                db.Execute("INSERT INTO dbo.HEAD_LST (NUMBER, TAG, CUST_NO) VALUES (1234, 20, N'C-5')", null, tx);
                tx.Commit();
            }

            // برگشت‌خورده: نباید ثبت شود
            using (var tx = db.BeginTransaction())
            {
                db.Execute("UPDATE dbo.HEAD_LST SET MABL_HAZ = 999 WHERE NUMBER = 1234", null, tx);
                tx.Rollback();
            }

            // Dispose بدون commit: از نظر SQL Server یعنی rollback
            using (var tx = db.BeginTransaction())
            {
                db.Execute("UPDATE dbo.HEAD_LST SET MABL_HAZ = 888 WHERE NUMBER = 1234", null, tx);
            }

            // بدون تراکنش صریح
            db.Execute("UPDATE dbo.HEAD_LST SET MABL_HAZ = @V WHERE NUMBER = @N", new { V = 135000, N = 1234 });
            db.Query<int>("SELECT COUNT(*) FROM dbo.HEAD_LST").FirstOrDefault();
        }

        // از مسیر قلاب‌خورده‌ی قدیمی: نباید دوبار ثبت شود
        using (var tm = new TransactionManagement(cs))
        {
            tm.ExecuteSqlCommandCtc("UPDATE dbo.HEAD_LST SET CUST_NO = @C WHERE NUMBER = @N",
                                    new { C = "C-9", N = 1234 });
            tm.DoCommit();
        }

        await AuditService.ShutdownAsync(15000);

        var rows = (await probe.QueryAsync(
            "SELECT [ACTION],[ENTITY],[ENTITY_KEY],[DETAIL] FROM [dbo].[SYS_AUDIT_EVENT] WHERE [ENTITY] = N'HEAD_LST'")).ToList();

        int Count(string act) => rows.Count(r => (string)r.ACTION == act);

        Ok("درج با Dapper روی کانکشن خام ثبت شد (قبلاً نامرئی بود)", Count("INSERT") == 1, Count("INSERT").ToString());
        Ok("کلید سند از همان درج استخراج شد",
            rows.Any(r => (string)r.ACTION == "INSERT" && (string?)r.ENTITY_KEY == "NUMBER=1234;TAG=20"));

        Ok("نوشتن برگشت‌خورده ثبت نشد",
            !rows.Any(r => r.DETAIL != null && ((string)r.DETAIL).Contains("999")));
        Ok("تراکنشِ بدون commit ثبت نشد",
            !rows.Any(r => r.DETAIL != null && ((string)r.DETAIL).Contains("888")));

        Ok("ویرایش بدون تراکنش ثبت شد",
            rows.Any(r => (string)r.ACTION == "UPDATE" && r.DETAIL != null && ((string)r.DETAIL).Contains("135000")));

        Ok("هیچ رویدادی دوبار ثبت نشد", Count("UPDATE") == 2, $"{Count("UPDATE")} به‌جای ۲");
        Ok("SELECT رویدادی نساخت", rows.Count == 3, rows.Count.ToString());

        // و مقدار واقعاً در دیتابیس درست است
        var mabl = await probe.ExecuteScalarAsync<double?>("SELECT MABL_HAZ FROM dbo.HEAD_LST WHERE NUMBER = 1234");
        Ok("دیتابیس واقعاً همان را دارد که سابقه می‌گوید", mabl == 135000, mabl?.ToString());

        ClearSpill();
    }


    // ── ۱۲) SQL خصمانه: کامنت، رشته، و شکل‌های نوشتنی کمتر رایج ────────
    private static void ValidateAdversarialSql()
    {
        Section("SQL خصمانه");

        (string Label, string Sql, bool ShouldLog, string? Entity)[] cases =
        {
            ("رشته‌ی حاوی DELETE",
             "SELECT * FROM CUST_HESAB WHERE MOLAH = N'DELETE FROM HEAD_LST'", false, null),
            ("کامنت خطی حاوی UPDATE",
             "-- UPDATE HEAD_LST SET MABL_HAZ = 1\nSELECT 1 FROM HEAD_LST", false, null),
            ("کامنت بلوکی حاوی INSERT",
             "/* INSERT INTO HEAD_LST (A) VALUES (1) */ SELECT 1", false, null),
            ("ستون به نام IS_DELETED",
             "SELECT IS_DELETED FROM HEAD_LST WHERE NUMBER = 1", false, null),
            ("رشته با نقل‌قول دوتایی",
             "SELECT * FROM T WHERE N = N'it''s DELETE FROM X'", false, null),
            ("MERGE واقعی (GeneralOptionManager)",
             @"MERGE dbo.GENERAL_OPTIONS AS target
               USING (SELECT @OptionName AS OptionName) AS source
               ON (target.OptionName = source.OptionName)
               WHEN MATCHED THEN UPDATE SET OptionValue = @OptionValue
               WHEN NOT MATCHED THEN INSERT (OptionName, OptionValue) VALUES (@OptionName, @OptionValue);",
             true, "GENERAL_OPTIONS"),
            ("TRUNCATE TABLE", "TRUNCATE TABLE dbo.TEMP_CALC", true, "TEMP_CALC"),
            ("SELECT ... INTO", "SELECT * INTO dbo.BACKUP_TBL FROM dbo.HEAD_LST", true, "BACKUP_TBL"),
            ("UPDATE واقعی همچنان کار کند",
             "UPDATE dbo.HEAD_LST SET MABL_HAZ = 5 WHERE NUMBER = 7", true, "HEAD_LST"),
        };

        foreach (var c in cases)
        {
            ClearSpill();
            AuditService.Start(DeadConnection, 1, "u", "1", 1405, "d");
            AuditSqlSniffer.Observe(c.Sql);
            AuditService.ShutdownAsync(6000).GetAwaiter().GetResult();

            var ev = ReadSpill().Where(e => e.Category == AuditCategory.Data).ToList();
            var logged = ev.Count > 0;

            if (logged != c.ShouldLog)
            {
                Ok(c.Label, false, logged ? $"ثبت شد: {ev[0].Action} {ev[0].Entity}" : "ثبت نشد");
            }
            else if (c.ShouldLog && !string.Equals(ev[0].Entity, c.Entity, StringComparison.OrdinalIgnoreCase))
            {
                Ok(c.Label, false, $"جدول {ev[0].Entity} به‌جای {c.Entity}");
            }
            else
            {
                Ok(c.Label, true);
            }
        }

        ClearSpill();
    }

    // ── ۱۳) تعویض کاربر بدون بسته شدن برنامه ──────────────────────────
    private static void ValidateUserSwitch()
    {
        Section("تعویض کاربر در همان اجرا");

        ClearSpill();
        AuditService.Start(DeadConnection, 0, "-", "1", 1405, "d");

        AuditService.AttachUser(10, "ALI", 1405, "1");
        var s1 = AuditService.CurrentSession?.SessionId;
        Audit.Write(new AuditEventDraft { Category = AuditCategory.Data, Action = AuditAction.Update, Title = "کار علی" });

        // خروج و ورود کاربر دیگر، بدون بسته شدن برنامه
        Audit.Logout("ALI");
        AuditService.AttachUser(20, "REZA", 1405, "1");
        var s2 = AuditService.CurrentSession?.SessionId;
        Audit.Write(new AuditEventDraft { Category = AuditCategory.Data, Action = AuditAction.Update, Title = "کار رضا" });

        AuditService.ShutdownAsync(8000).GetAwaiter().GetResult();
        var ev = ReadSpill();

        Ok("با تعویض کاربر، نشست تازه باز می‌شود", s1 is not null && s2 is not null && s1 != s2, $"{s1} / {s2}");
        Ok("کار علی به علی نسبت دارد",
            ev.Any(e => e.Title == "کار علی" && e.UserName == "ALI" && e.UserId == 10));
        Ok("کار رضا به رضا نسبت دارد",
            ev.Any(e => e.Title == "کار رضا" && e.UserName == "REZA" && e.UserId == 20));
        Ok("رویدادهای دو کاربر در یک نشست قاطی نشدند",
            ev.Where(e => e.Title == "کار علی").All(e => e.SessionId == s1)
            && ev.Where(e => e.Title == "کار رضا").All(e => e.SessionId == s2));
        Ok("خروج کاربر ثبت شد",
            ev.Any(e => e.Action == AuditAction.Logout && e.UserName == "ALI"));

        ClearSpill();
    }

}
