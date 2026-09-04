using AUTO_BAZ.Functions;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRunner
{
    /// <summary>
    /// تست رگرسیون باگ «اصلاح پورسانت اعمال نمی‌شود».
    ///
    /// اجرا:
    ///   TestRunner.exe porsant          → فقط تست‌های قاعده (بدون دیتابیس، بدون تغییر داده)
    ///   TestRunner.exe porsant --apply  → چرخه‌ی کامل روی دیتابیس: فهرست مغایرت‌ها →
    ///                                     اصلاح و صدور مجدد سند → بازخوانی → ادعا اینکه
    ///                                     همان سطرها دیگر مغایر نیستند و عملیات اصلاح
    ///                                     دوباره روی آنها اجرا نمی‌شود.
    ///
    /// حالت --apply روی دیتابیس می‌نویسد (دقیقاً همان کاری که دکمه‌ی پنجره می‌کند)؛
    /// فقط روی دیتابیس تست اجرا شود.
    /// </summary>
    internal static class PorsantCorrectionTest
    {
        private static int _passed;
        private static int _failed;

        public static int Run(bool applyOnDatabase)
        {
            _passed = 0;
            _failed = 0;

            Console.WriteLine("=========================================================================");
            Console.WriteLine("            REGRESSION: اصلاح پورسانت فاکتور فروش                        ");
            Console.WriteLine("=========================================================================");

            RunRuleTests();

            if (applyOnDatabase)
            {
                RunDatabaseCycleTest();
            }
            else
            {
                Console.WriteLine("[SKIP] چرخه‌ی دیتابیس اجرا نشد (برای اجرا: TestRunner.exe porsant --apply)");
            }

            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine($"PASSED: {_passed}   FAILED: {_failed}");
            return _failed == 0 ? 0 : 1;
        }

        #region قاعده‌ی محاسبه (بدون دیتابیس)

        private static void RunRuleTests()
        {
            var savedOptions = Baseknow.OPTIONSS;

            try
            {
                // ── گِردکردن: باید «نیم به بالا» باشد تا با ROUND در SQL Server یکی شود.
                // این دقیقاً همان اختلافی است که سطرها را برای همیشه مغایر نگه می‌داشت:
                // C# با Math.Round پیش‌فرض 0.5 را به 0 و 2.5 را به 2 می‌برد.
                Check("RoundMoney(0.5) = 1", CL_PORSANT_RULE.RoundMoney(0.5) == 1);
                Check("RoundMoney(2.5) = 3", CL_PORSANT_RULE.RoundMoney(2.5) == 3);
                Check("RoundMoney(3.5) = 4", CL_PORSANT_RULE.RoundMoney(3.5) == 4);
                Check("RoundMoney(-2.5) = -3", CL_PORSANT_RULE.RoundMoney(-2.5) == -3);
                Check("با Math.Round پیش‌فرض فرق دارد (2.5)", Math.Round(2.5) != CL_PORSANT_RULE.RoundMoney(2.5));

                // ── مبنای پورسانت بدون ارزش افزوده (گزینه ۶۲ سازمان غیرفعال)
                Baseknow.OPTIONSS = OptionsWithChar62(false);
                Check("مبنا = جمع − تخفیف",
                    CL_PORSANT_RULE.PorsantBase(1_000_000, 50_000, 90_000) == 950_000);
                Check("تخفیف/ارزش‌افزوده‌ی NULL صفر گرفته می‌شود",
                    CL_PORSANT_RULE.PorsantBase(1_000_000, null, null) == 1_000_000);

                // ── مبنای پورسانت با ارزش افزوده (گزینه ۶۲ = «۵»)
                Baseknow.OPTIONSS = OptionsWithChar62(true);
                Check("مبنا = جمع − تخفیف + ارزش افزوده",
                    CL_PORSANT_RULE.PorsantBase(1_000_000, 50_000, 90_000) == 1_040_000);

                Baseknow.OPTIONSS = OptionsWithChar62(false);

                // ── سطر بدون الگو: درصد × مبنا، با گِردکردن نیم به بالا
                Check("ByDarsad(1000, 0.05%) = 1",
                    CL_PORSANT_RULE.ByDarsad(1000, 0.05) == 1);
                Check("ByDarsad با درصد NULL صفر می‌شود",
                    CL_PORSANT_RULE.ByDarsad(1_000_000, null) == 0);
                Check("ByDarsad(950000, 2%) = 19000",
                    CL_PORSANT_RULE.ByDarsad(950_000, 2) == 19_000);

                // ── سطر دارای الگو: کالای بدون نرخ سهمی نمی‌گیرد (طراحی، نه باگ)
                Check("کالای بدون نرخ سهمی نمی‌گیرد",
                    CL_PORSANT_RULE.PatternLineShare(1_000_000, null) == 0);
                Check("مبلغ خالیِ ردیف کالا کرش نمی‌کند",
                    CL_PORSANT_RULE.PatternLineShare(null, 2.5) == 0);
                Check("PatternLineShare(1000, 2.5%) = 25",
                    CL_PORSANT_RULE.PatternLineShare(1000, 2.5) == 25);

                // ── جمعِ الگو ردیف‌به‌ردیف گِرد می‌شود، نه یک‌جا در انتها.
                // این همان چیزی است که GENSANADFROOSH می‌نویسد؛ اگر گزارش یک‌جا گِرد
                // می‌کرد، عددش با سند فرق داشت و سطر همیشه مغایر می‌ماند.
                var lines = new List<(double? mablk, double? rate)> { (150d, 1d), (250d, 1d), (350d, 1d) };
                double perLine = lines.Sum(l => CL_PORSANT_RULE.PatternLineShare(l.mablk, l.rate));
                double atEnd = CL_PORSANT_RULE.RoundMoney(lines.Sum(l => (l.mablk ?? 0) * (l.rate ?? 0) / 100));
                Check("جمع الگو ردیف‌به‌ردیف گِرد می‌شود (2+3+4=9)", perLine == 9);
                Check("و با گِردکردنِ یک‌جا (8) فرق دارد", atEnd == 8 && perLine != atEnd);

                // ── تعریف مغایرت: اختلاف کمتر از یک ریال مغایرت نیست
                var row = new CL_PORSANT_RULE.PorsantAuditRow { OLD_PURSANT = 19_000, NEW_PURSANT = 19_000 };
                Check("اختلاف صفر یعنی بدون مغایرت", Math.Abs(row.DIFF) < CL_PORSANT_RULE.TOLERANCE);

                row = new CL_PORSANT_RULE.PorsantAuditRow { OLD_PURSANT = 19_000, NEW_PURSANT = 19_001 };
                Check("اختلاف یک ریال یعنی مغایرت", Math.Abs(row.DIFF) >= CL_PORSANT_RULE.TOLERANCE);
            }
            finally
            {
                Baseknow.OPTIONSS = savedOptions;
            }
        }

        /// <summary>رشته‌ی گزینه‌های سازمان با کاراکتر ۶۲ برابر «۵» یا «۰».</summary>
        private static string OptionsWithChar62(bool includeVat)
        {
            var chars = new string('0', 80).ToCharArray();
            chars[61] = includeVat ? '5' : '0';
            return new string(chars);
        }

        #endregion

        #region چرخه‌ی کامل روی دیتابیس

        /// <summary>
        /// سناریوی خودِ باگ: فاکتور مغایر → اصلاح → بازخوانی → اختلاف صفر →
        /// عملیات اصلاح دوباره همان فاکتورها را انتخاب نمی‌کند.
        /// </summary>
        private static void RunDatabaseCycleTest()
        {
            if (string.IsNullOrWhiteSpace(CL_CCNNMANAGER.CONNECTION_STR))
            {
                Console.WriteLine("[SKIP] رشته‌ی اتصال تنظیم نشده؛ چرخه‌ی دیتابیس اجرا نشد.");
                return;
            }

            List<CL_PORSANT_RULE.PorsantAuditRow> before;
            try
            {
                before = CL_PORSANT_RULE.Audit();
            }
            catch (Exception ex)
            {
                Fail("خواندن فهرست مغایرت‌ها: " + ex.Message);
                return;
            }

            var fixable = before.Where(r => r.CAN_FIX).ToList();
            Console.WriteLine($"[INFO] {before.Count} سطر مغایر، {fixable.Count} سطر قابل اصلاح، " +
                              $"در {fixable.Select(r => r.NUMBER).Distinct().Count()} فاکتور.");

            // هر سطر فهرست‌شده باید واقعاً اختلاف داشته باشد
            Check("همه‌ی سطرهای فهرست اختلاف واقعی دارند",
                before.All(r => Math.Abs(r.DIFF) >= CL_PORSANT_RULE.TOLERANCE));

            if (fixable.Count == 0)
            {
                Console.WriteLine("[SKIP] سطر قابل اصلاحی وجود ندارد؛ چرخه‌ی اصلاح اجرا نشد.");
                return;
            }

            var result = CL_PORSANT_RULE.FixAndReissue(fixable);

            Check("برای هر فاکتور یک نتیجه ثبت شد",
                result.Outcomes.Count == fixable.Select(r => r.NUMBER).Distinct().Count());

            foreach (var failedOutcome in result.Failed)
            {
                Console.WriteLine($"[INFO] فاکتور {failedOutcome.NUMBER} اصلاح نشد: " +
                                  (failedOutcome.Error ?? "پس از صدور مجدد سند دوباره مغایر بود"));
            }

            // ── ادعای اصلی: سطرهایی که اصلاح شدند دیگر مغایر نیستند
            var after = CL_PORSANT_RULE.Audit();
            var okIds = result.Outcomes.Where(o => o.Ok).SelectMany(o => o.UpdatedIds).ToHashSet();

            Check("هیچ‌کدام از سطرهای اصلاح‌شده دوباره مغایر نیستند",
                !after.Any(r => r.ID.HasValue && okIds.Contains(r.ID.Value)));

            foreach (var outcome in result.Outcomes.Where(o => o.Ok))
            {
                Check($"فاکتور {outcome.NUMBER}: اختلاف صفر شد",
                    !after.Any(r => r.ID.HasValue && outcome.UpdatedIds.Contains(r.ID.Value)));
            }

            // ── اجرای دوباره‌ی عملیات نباید همان سطرها را انتخاب کند
            var secondRoundTargets = after.Where(r => r.CAN_FIX && r.ID.HasValue && okIds.Contains(r.ID.Value)).ToList();
            Check("اجرای مجدد اصلاح، سطرهای اصلاح‌شده را دوباره انتخاب نمی‌کند",
                secondRoundTargets.Count == 0);

            // ── و «فهرستی که پنجره نشان می‌دهد» همان چیزی است که خودِ عملیات برگرداند
            Check("فهرست بازگشتی عملیات با بازخوانی مستقل یکی است",
                result.Remaining.Count == after.Count);
        }

        #endregion

        private static void Check(string title, bool condition)
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine("[PASS] " + title);
            }
            else
            {
                _failed++;
                Console.WriteLine("[FAIL] " + title);
            }
        }

        private static void Fail(string title)
        {
            _failed++;
            Console.WriteLine("[FAIL] " + title);
        }
    }
}
