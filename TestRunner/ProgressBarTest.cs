using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AUTO_BAZ.Functions;

namespace TestRunner
{
    /// <summary>
    /// تست واقعیِ باگ «۸.۳٪» روی همان Dispatcher و همان کلاس ThrottledProgressReporter
    /// که برنامه استفاده می‌کند. هیچ چیزی روی دیتابیس نوشته نمی‌شود.
    ///
    /// دو سناریو اجرا می‌شود:
    ///   الف) رفتار قبل از اصلاح (LegacyReporter = کپی دقیق کد قبلی) → انتظار: ~۸.۳٪
    ///   ب) رفتار بعد از اصلاح (کلاس واقعی + نسل + تخلیه صف) → انتظار: ۱۰۰٪
    /// </summary>
    internal static class ProgressBarTest
    {
        private const int SectionCount = 12;

        /// <summary>نوارهای پیشرفت ۱۲ بخش (معادل PRGR_C0..PRGR_C11).</summary>
        private static readonly double[] Bars = new double[SectionCount];

        private static double Overall => Bars.Sum() / SectionCount;

        /// <summary>کپی دقیقِ Report قبل از اصلاح: BeginInvoke با اولویت Background و بدون هیچ محافظی.</summary>
        private sealed class LegacyReporter
        {
            private readonly int _total;
            private readonly int _interval;
            private readonly Dispatcher _dispatcher;
            private readonly Action<double> _apply;
            private int _done;

            public LegacyReporter(int total, Dispatcher dispatcher, Action<double> apply)
            {
                _total = Math.Max(1, total);
                _interval = Math.Max(1, _total / 100);
                _dispatcher = dispatcher;
                _apply = apply;
            }

            public void ReportOne()
            {
                var done = Interlocked.Increment(ref _done);
                if (done % _interval == 0) { Report(done * 100.0 / _total); }
            }

            public void Complete() => Report(100.0);

            private void Report(double value)
                => _dispatcher.BeginInvoke(new Action(() => _apply(value)), DispatcherPriority.Background);
        }

        /// <summary>
        /// بازتولید قطعیِ باگ «۸.۳٪» — بدون اتکا به شانس زمان‌بندی.
        ///
        /// مدل واقعی: ۱۱ بخش زودتر تمام شده‌اند و گزارش ۱۰۰٪شان از صف Background خارج و
        /// روی نوارها نشسته است. بخش دوازدهم درست در آخرین لحظه تمام می‌شود، پس گزارش ۱۰۰٪
        /// آن «هنوز در صف» است وقتی پایان‌دهی (با اولویت بالاتر) همه‌ی نوارها را صفر می‌کند.
        /// بعد از صفر شدن، همان یک گزارش عقب‌مانده اجرا می‌شود و تنها نوار خودش را ۱۰۰ می‌کند:
        /// میانگین = ۱۰۰ ÷ ۱۲ = ۸.۳٪
        /// </summary>
        private static async Task<double> RunLegacyAsync(Dispatcher dispatcher)
        {
            Array.Clear(Bars, 0, Bars.Length);

            // ۱۱ بخشی که زودتر تمام شدند و گزارششان درست نشسته است.
            for (int s = 0; s < SectionCount - 1; s++)
            {
                int section = s;
                var r = new LegacyReporter(500, dispatcher, v => Bars[section] = Math.Max(Bars[section], v));
                await Task.Run(() =>
                {
                    for (int i = 0; i < 500; i++) { r.ReportOne(); }
                    r.Complete();
                });
            }
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            Console.WriteLine($"  میانگین پس از پایان ۱۱ بخش : {Overall:F1}%");

            // بخش دوازدهم دیرتر تمام می‌شود؛ گزارش ۱۰۰٪ آن در صف Background می‌نشیند.
            var last = new LegacyReporter(500, dispatcher, v => Bars[SectionCount - 1] = Math.Max(Bars[SectionCount - 1], v));
            await Task.Run(() => last.Complete());

            // پایان‌دهیِ قبلی: DoResetCountersDisplay با اولویت Normal (بالاتر از Background)
            // پس «قبل از» آن گزارش عقب‌مانده اجرا می‌شود و همه‌ی نوارها را صفر می‌کند.
            await dispatcher.InvokeAsync(() => Array.Clear(Bars, 0, Bars.Length), DispatcherPriority.Normal);
            Console.WriteLine($"  میانگین بلافاصله پس از پایان‌دهی : {Overall:F1}%");

            // حالا گزارش عقب‌مانده اجرا می‌شود و روی نوارهای صفرشده می‌نشیند.
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            return Overall;
        }

        /// <summary>
        /// همان سناریوی قطعیِ بالا، ولی با کلاس واقعی برنامه و ترتیب جدید:
        /// تخلیه‌ی صف Background پیش از پایان‌دهی + جلو بردن نسل + پایان‌دهی بدون صفر کردن.
        /// </summary>
        private static async Task<double> RunFixedAsync(Dispatcher dispatcher)
        {
            CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
            Array.Clear(Bars, 0, Bars.Length);

            for (int s = 0; s < SectionCount - 1; s++)
            {
                int section = s;
                var r = new CL_HESABDARI_AUTO_BAZ.ThrottledProgressReporter(500, dispatcher, v => Bars[section] = Math.Max(Bars[section], v));
                await Task.Run(() =>
                {
                    for (int i = 0; i < 500; i++) { r.ReportOne(); }
                    r.Complete();
                });
            }
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            Console.WriteLine($"  میانگین پس از پایان ۱۱ بخش : {Overall:F1}%");

            var last = new CL_HESABDARI_AUTO_BAZ.ThrottledProgressReporter(500, dispatcher, v => Bars[SectionCount - 1] = Math.Max(Bars[SectionCount - 1], v));
            await Task.Run(() => last.Complete());

            // اصلاح ۱: صبر تا نوبت اولویت Background برسد → گزارش عقب‌ماندهٔ بخش دوازدهم
            // «پیش از» پایان‌دهی اجرا می‌شود، پس مقدار واقعی روی صفحه می‌نشیند.
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            Console.WriteLine($"  میانگین پس از تخلیه صف و پیش از پایان‌دهی : {Overall:F1}%");

            // اصلاح ۲: از این لحظه هر گزارش عقب‌مانده‌ای بی‌اثر است.
            CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();

            // اصلاح ۳: پایان‌دهیِ تک‌معنا — نوارها صفر نمی‌شوند.
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            return Overall;
        }

        /// <summary>
        /// تست محافظ نسل به‌تنهایی: اگر گزارشی «بعد» از پایان‌دهی برسد، باید بی‌اثر باشد.
        /// (سناریوی واقعی: بخشی که خیلی دیر تمام می‌شود.)
        /// </summary>
        private static async Task<double> RunLateReportAsync(Dispatcher dispatcher)
        {
            CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();
            for (int i = 0; i < SectionCount; i++) { Bars[i] = 100; }

            var lateReporter = new CL_HESABDARI_AUTO_BAZ.ThrottledProgressReporter(10, dispatcher, v =>
            {
                // اگر محافظ کار نکند، این خط همه‌ی نوارها را خراب می‌کند.
                for (int i = 0; i < SectionCount; i++) { Bars[i] = 0; }
                Bars[5] = v;
            });

            // پایان‌دهی انجام شد و نسل جلو رفت...
            CL_HESABDARI_AUTO_BAZ.BumpUiProgressGeneration();

            // ...و حالا یک گزارش دیرهنگام می‌رسد.
            lateReporter.Complete();
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            return Overall;
        }

        public static void Run()
        {
            Console.WriteLine();
            Console.WriteLine("=========================================================================");
            Console.WriteLine("   تست Progressbar : ۱۲ بخش موازی روی Dispatcher واقعی WPF");
            Console.WriteLine("=========================================================================");

            var dispatcher = Dispatcher.CurrentDispatcher;
            var allPassed = true;

            var work = Task.Run(async () =>
            {
                Console.WriteLine();
                Console.WriteLine("[۱] رفتار قبل از اصلاح (بازتولید باگ)");
                var legacy = await RunLegacyAsync(dispatcher);
                Console.WriteLine($"  نتیجه نهایی : {legacy:F1}%   (انتظار حدود 8.3)");
                var legacyReproduced = Math.Abs(legacy - 100.0) > 0.01;
                Console.WriteLine(legacyReproduced
                    ? "  ✔ باگ بازتولید شد (نتیجه ۱۰۰٪ نیست)"
                    : "  ✘ باگ بازتولید نشد");
                if (!legacyReproduced) { allPassed = false; }

                Console.WriteLine();
                Console.WriteLine("[۲] رفتار بعد از اصلاح");
                var fixedResult = await RunFixedAsync(dispatcher);
                Console.WriteLine($"  نتیجه نهایی : {fixedResult:F1}%   (انتظار 100.0)");
                var fixedOk = Math.Abs(fixedResult - 100.0) < 0.01;
                Console.WriteLine(fixedOk ? "  ✔ قبول" : "  ✘ رد");
                if (!fixedOk) { allPassed = false; }

                Console.WriteLine();
                Console.WriteLine("[۳] محافظ نسل: گزارش دیرهنگام بعد از پایان‌دهی");
                var late = await RunLateReportAsync(dispatcher);
                Console.WriteLine($"  نتیجه نهایی : {late:F1}%   (انتظار 100.0 - گزارش دیرهنگام بی‌اثر)");
                var lateOk = Math.Abs(late - 100.0) < 0.01;
                Console.WriteLine(lateOk ? "  ✔ قبول" : "  ✘ رد - گزارش دیرهنگام نوارها را خراب کرد");
                if (!lateOk) { allPassed = false; }

                Console.WriteLine();
                Console.WriteLine(allPassed
                    ? "نتیجه کلی : ✔ همه تست‌ها قبول"
                    : "نتیجه کلی : ✘ حداقل یک تست رد شد");
                Console.WriteLine("=========================================================================");

                dispatcher.InvokeShutdown();
            });

            Dispatcher.Run();
            work.GetAwaiter().GetResult();
        }
    }
}
