using Microsoft.SqlServer.TransactSql.ScriptDom;
using Prg_Proccessy.AUDIT;
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
                    var e = JsonSerializer.Deserialize<AuditEvent>(line);
                    if (e != null) list.Add(e);
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
        await ValidateEndToEndAsync();
        await ValidatePerformanceAsync();
        ValidateResilience();

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

        Ok("همه به کاربر درست نسبت داده شدند",
            ev.All(e => e.UserName == "Controller" && e.UserId == 78));
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
}
