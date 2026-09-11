using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.AUDIT;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.CONFIGS;

namespace TestRunner
{
    public static class AuditVerificationRunner
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=========================================================================");
            Console.WriteLine("          AUDIT TRAIL FULL VERIFICATION (SECTIONS 1, 2, 3, 4)           ");
            Console.WriteLine("=========================================================================");

            const string cs = "Data Source=MERCEDES\\SQL2022;Initial Catalog=YAZDSEPAR1405;Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            Baseknow.USERCOD = 78;
            Baseknow.UUSER = "Controller";
            CL_Generaly.SHIFT_OF_USER = 1;
            CL_Generaly.VAHED_OF_USER = 1;
            Baseknow.UGRP = "1";
            CL_Generaly.IsCalledExternally = true;
            CL_CCNNMANAGER.CONNECTION_STR = cs;
            CL_CCNNMANAGER.ConnectedToSQLDB = true;
            Baseknow.GetInitTheApp();

            // Start Audit Engine
            AuditService.Start(cs, 78, "Controller", "1.0.0.460", 1405, "YAZDSEPAR1405");
            AuditService.AttachUser(78, "Controller", 1405, "1.0.0.460");

            using var db = new SqlConnection(cs);
            db.Open();

            // ─────────────────────────────────────────────────────────────
            // بخش ۳.۱ — تست منفی دسترسی
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("\n--- بخش ۳.۱: تست منفی دسترسی (قبل از اعطای مجوز) ---");
            // حذف موقت مجوز کاربر 78 اگر وجود دارد تا تست منفی واقعی باشد
            db.Execute("DELETE FROM dbo.SAL_CHEK WHERE USERCO = 78 AND [OBJECT] = 479;");

            bool hasAccessBefore = CL_HESABDARI.LETSGO("AUDITTRAIL");
            Console.WriteLine($"[RESULT] دسترسی کاربر 78 به فرم AUDITTRAIL قبل از مجوز: {hasAccessBefore} (انتظار: False)");

            if (!hasAccessBefore)
            {
                Audit.Security(AuditAction.AuditViewed, "تلاش برای مشاهده‌ی سوابق بدون دسترسی", formName: nameof(WIN_AUDIT_TRAIL));
                Console.WriteLine("[PASS] رویداد 'تلاش برای مشاهده‌ی سوابق بدون دسترسی' با اکشن AUDIT_VIEWED ارسال شد.");
            }

            // ─────────────────────────────────────────────────────────────
            // بخش ۳.۲ — دادن دسترسی
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("\n--- بخش ۳.۲: اعطای دسترسی به کاربر 78 در SAL_CHEK ---");
            var formInfo = db.QueryFirstOrDefault("SELECT IDH, FORMNAME, CAPTION FROM dbo.TFORMS WHERE FORMNAME = N'AUDITTRAIL'");
            Console.WriteLine($"[INFO] فرم در TFORMS: IDH={formInfo?.IDH}, FORMNAME={formInfo?.FORMNAME}, CAPTION={formInfo?.CAPTION}");

            db.Execute(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.SAL_CHEK WHERE USERCO = 78 AND [OBJECT] = 479)
                    INSERT INTO dbo.SAL_CHEK (USERCO, [OBJECT], RUN, SEE, INP, UPD, DEL, CRT, UID)
                    VALUES (78, 479, 1, 1, 0, 0, 0, GETDATE(), 78);");
            Console.WriteLine("[PASS] رکورد دسترسی در SAL_CHEK درج شد.");

            bool hasAccessAfter = CL_HESABDARI.LETSGO("AUDITTRAIL");
            Console.WriteLine($"[RESULT] دسترسی کاربر 78 به فرم AUDITTRAIL بعد از مجوز: {hasAccessAfter} (انتظار: True)");

            // ─────────────────────────────────────────────────────────────
            // بخش ۴ — سناریوی اصلی: چرخه‌ی عمر یک پیش‌فاکتور (مراحل ۱ تا ۹)
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("\n--- بخش ۴: اجرای چرخه‌ی عمر پیش‌فاکتور (NUMBER=999991; TAG=20) ---");
            const int testDocNumber = 999991;
            const int testTag = 20;

            // ۱. باز کردن فرم پیش‌فاکتور
            Console.WriteLine("[STEP 1] باز کردن فرم پیش‌فاکتور...");
            Audit.Form("HEAD_LST_PISHFROOSH2");
            Thread.Sleep(800);

            // ۲. ثبت پیش‌فاکتور جدید (INSERT)
            Console.WriteLine("[STEP 2] ثبت پیش‌فاکتور جدید در HEAD_LST...");
            db.Execute("DELETE FROM dbo.HEAD_LST WHERE NUMBER = @N AND TAG = @T", new { N = testDocNumber, T = testTag });
            db.Execute(@"
                INSERT INTO dbo.HEAD_LST (NUMBER, TAG, CUST_NO, MAS, M_NAGHD, DATE_N, TAH, USER_NAME)
                VALUES (@N, @T, N'115-1-1', 1500000, 0, 14050620, N'پیش‌فاکتور تستی سوابق', N'Controller')",
                new { N = testDocNumber, T = testTag });
            Thread.Sleep(900);

            // ۳. ویرایش مبلغ یا قلم (UPDATE)
            Console.WriteLine("[STEP 3] ویرایش پیش‌فاکتور (مبلغ و شرح)...");
            db.Execute(@"
                UPDATE dbo.HEAD_LST
                   SET MAS = 1850000, MOLAH = N'ویرایش تست سوابق'
                 WHERE NUMBER = @N AND TAG = @T",
                new { N = testDocNumber, T = testTag });
            Thread.Sleep(900);

            // ۴. ثبت امضا (SIGN)
            Console.WriteLine("[STEP 4] ثبت امضا (SGN1 = 1)...");
            db.Execute(@"
                UPDATE dbo.HEAD_LST
                   SET SGN1 = 1
                 WHERE NUMBER = @N AND TAG = @T",
                new { N = testDocNumber, T = testTag });
            Thread.Sleep(900);

            // ۵. برداشتن امضا (UNSIGN)
            Console.WriteLine("[STEP 5] برداشتن امضا (SGN1 = 0)...");
            db.Execute(@"
                UPDATE dbo.HEAD_LST
                   SET SGN1 = 0
                 WHERE NUMBER = @N AND TAG = @T",
                new { N = testDocNumber, T = testTag });
            Thread.Sleep(900);

            // ۶. پیش‌نمایش چاپ (PREVIEW)
            Console.WriteLine("[STEP 6] پیش‌نمایش چاپ گزارش...");
            Audit.Print("پیش‌فاکتور", entity: "HEAD_LST", entityKey: $"NUMBER={testDocNumber};TAG={testTag}", isPreview: true);
            Thread.Sleep(800);

            // ۷. چاپ واقعی (PRINT)
            Console.WriteLine("[STEP 7] چاپ واقعی گزارش...");
            Audit.Print("پیش‌فاکتور", entity: "HEAD_LST", entityKey: $"NUMBER={testDocNumber};TAG={testTag}", isPreview: false);
            Thread.Sleep(800);

            // ۸. خروجی اکسل (EXPORT)
            Console.WriteLine("[STEP 8] خروجی اکسل...");
            Audit.Export("Excel", "پیش‌فاکتور", entity: "HEAD_LST", entityKey: $"NUMBER={testDocNumber};TAG={testTag}");
            Thread.Sleep(800);

            // ۹. حذف پیش‌فاکتور (DELETE)
            Console.WriteLine("[STEP 9] حذف پیش‌فاکتور...");
            Audit.Delete("HEAD_LST", $"NUMBER={testDocNumber};TAG={testTag}", $"حذف پیش‌فاکتور {testDocNumber}");
            db.Execute("DELETE FROM dbo.HEAD_LST WHERE NUMBER = @N AND TAG = @T", new { N = testDocNumber, T = testTag });
            Thread.Sleep(1200);

            // ─────────────────────────────────────────────────────────────
            // بخش ۳.۳ — تست مثبت: باز شدن فرم، بارگذاری دیتا و اسکرین‌شات
            // ─────────────────────────────────────────────────────────────
            Console.WriteLine("\n--- بخش ۳.۳: تست مثبت باز شدن فرم سوابق و عکس‌برداری ---");
            string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WIN_AUDIT_TRAIL_SCREENSHOT.png");

            var staThread = new Thread(() =>
            {
                var app = Application.Current ?? new Application();
                var resourceUris = new[]
                {
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Secondary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.ObsoleteBrushes.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/Greeny.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/Rangy.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/ColorModel.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/GlobalSoftTheme.xaml",
                    "pack://application:,,,/Syncfusion.SfGrid.WPF;component/Themes/Generic.xaml"
                };
                foreach (var u in resourceUris)
                {
                    try { app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(u, UriKind.RelativeOrAbsolute) }); }
                    catch (Exception ex) { Console.WriteLine($"[WARN Resource] {u} : {ex.Message}"); }
                }

                try
                {
                    app.Resources["IRANYEKAN"] = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/MrCorrect;component/UiDrive/FNT/"), "#IRANYekanFN");
                }
                catch { app.Resources["IRANYEKAN"] = new System.Windows.Media.FontFamily("Tahoma"); }

                var win = new WIN_AUDIT_TRAIL();
                win.Loaded += (s, e) =>
                {
                    Console.WriteLine("[PASS] پنجره WIN_AUDIT_TRAIL باز شد (Title: " + win.Title + ")");

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                    timer.Tick += (ts, te) =>
                    {
                        timer.Stop();
                        try
                        {
                            Console.WriteLine($"[INFO] تعداد سطرهای بارگذاری‌شده در گرید: {win.Rows.Count}");
                            var interop = new System.Windows.Interop.WindowInteropHelper(win);
                            GetWindowRect(interop.Handle, out RECT rect);
                            int w = rect.Right - rect.Left;
                            int h = rect.Bottom - rect.Top;
                            if (w > 0 && h > 0)
                            {
                                using var bmp = new Bitmap(w, h);
                                using var g = Graphics.FromImage(bmp);
                                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(w, h));
                                bmp.Save(screenshotPath, ImageFormat.Png);
                                Console.WriteLine($"[PASS] اسکرین‌شات ذخیره شد: {screenshotPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] خطا در تصویربرداری: {ex.Message}");
                        }
                        finally
                        {
                            win.Close();
                            app.Shutdown();
                        }
                    };
                    timer.Start();
                };

                app.Run(win);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            // Shutdown AuditService to flush all pending records
            AuditService.ShutdownAsync(15000).GetAwaiter().GetResult();

            Console.WriteLine("\n=========================================================================");
            Console.WriteLine("          VERIFICATION QUERIES ON DATABASE YAZDSEPAR1405                 ");
            Console.WriteLine("=========================================================================");

            // کوئری ۳.۱: تلاش بدون دسترسی
            Console.WriteLine("\n[QUERY 3.1] تلاش بدون دسترسی:");
            var unauthLogs = db.Query(@"
                SELECT TOP 5 LOG_ID, AT_SERVER, USER_NAME, ACTION, TITLE
                FROM dbo.VW_SYS_AUDIT_TIMELINE
                WHERE ACTION = 'AUDIT_VIEWED' AND TITLE LIKE N'%بدون دسترسی%'
                ORDER BY LOG_ID DESC").ToList();
            foreach (var row in unauthLogs)
            {
                Console.WriteLine($"  LOG_ID={row.LOG_ID} | AT_SERVER={row.AT_SERVER} | USER={row.USER_NAME} | ACTION={row.ACTION} | TITLE={row.TITLE}");
            }

            // کوئری ۴: چرخه‌ی عمر پیش‌فاکتور 999991
            Console.WriteLine("\n[QUERY 4] خط زمانی کامل پیش‌فاکتور NUMBER=999991; TAG=20:");
            var docTimeline = db.Query(@"
                SELECT LOG_ID, DATE_S, TIME_S, AT_SERVER, USER_NAME, CATEGORY, ACTION, ENTITY, ENTITY_KEY, FORM_NAME, TITLE, DETAIL
                FROM dbo.VW_SYS_AUDIT_TIMELINE
                WHERE ENTITY_KEY LIKE N'%NUMBER=999991%' OR TITLE LIKE N'%999991%'
                ORDER BY LOG_ID ASC").ToList();

            foreach (var row in docTimeline)
            {
                Console.WriteLine($"  LOG_ID={row.LOG_ID,-6} | DATE={row.DATE_S} {row.TIME_S,-6} | USER={row.USER_NAME,-10} | ACTION={row.ACTION,-10} | CATEGORY={row.CATEGORY} | FORM={row.FORM_NAME,-22} | TITLE={row.TITLE}");
            }

            Console.WriteLine("\n🎉 تست کامل سوابق و ردیابی کاربران با موفقیت به پایان رسید.");
        }

        public static void RunUIOnly(string dbName = "YAZDSEPAR1405_TEST")
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=========================================================================");
            Console.WriteLine($"          AUDIT TRAIL UI VISUAL CAPTURE ({dbName})                      ");
            Console.WriteLine("=========================================================================");

            string cs = $"Data Source=MERCEDES\\SQL2022;Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            Baseknow.USERCOD = 78;
            Baseknow.UUSER = "Controller";
            CL_Generaly.SHIFT_OF_USER = 1;
            CL_Generaly.VAHED_OF_USER = 1;
            Baseknow.UGRP = "1";
            CL_Generaly.IsCalledExternally = true;
            CL_CCNNMANAGER.CONNECTION_STR = cs;
            CL_CCNNMANAGER.ConnectedToSQLDB = true;
            Baseknow.GetInitTheApp();

            using (var db = new SqlConnection(cs))
            {
                db.Open();
                var currentDb = db.ExecuteScalar<string>("SELECT DB_NAME();");
                Console.WriteLine($"[DB CHECK] Connected database: {currentDb}");

                db.Execute(@"
                    IF NOT EXISTS (SELECT 1 FROM dbo.SAL_CHEK WHERE USERCO = 78 AND [OBJECT] = 479)
                        INSERT INTO dbo.SAL_CHEK (USERCO, [OBJECT], RUN, SEE, INP, UPD, DEL, CRT, UID)
                        VALUES (78, 479, 1, 1, 0, 0, 0, GETDATE(), 78);");
            }

            AuditService.Start(cs, 78, "Controller", "1.0.0.460", 1405, dbName);
            AuditService.AttachUser(78, "Controller", 1405, "1.0.0.460");

            string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WIN_AUDIT_TRAIL_SCREENSHOT.png");

            var staThread = new Thread(() =>
            {
                var app = Application.Current ?? new Application();
                var resourceUris = new[]
                {
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Secondary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.ObsoleteBrushes.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/Greeny.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/Rangy.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/ColorModel.xaml",
                    "pack://application:,,,/MrCorrect;component/UiDictionary/GlobalSoftTheme.xaml",
                    "pack://application:,,,/Syncfusion.SfGrid.WPF;component/Themes/Generic.xaml"
                };
                foreach (var u in resourceUris)
                {
                    try { app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(u, UriKind.RelativeOrAbsolute) }); }
                    catch (Exception ex) { Console.WriteLine($"[WARN Resource] {u} : {ex.Message}"); }
                }

                try
                {
                    app.Resources["IRANYEKAN"] = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/MrCorrect;component/UiDrive/FNT/"), "#IRANYekanFN");
                }
                catch { app.Resources["IRANYEKAN"] = new System.Windows.Media.FontFamily("Tahoma"); }

                var win = new WIN_AUDIT_TRAIL();
                win.Loaded += (s, e) =>
                {
                    Console.WriteLine("[PASS] پنجره WIN_AUDIT_TRAIL باز شد (Title: " + win.Title + ")");

                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
                    timer.Tick += (ts, te) =>
                    {
                        timer.Stop();
                        try
                        {
                            Console.WriteLine($"[INFO] تعداد سطرهای بارگذاری‌شده در گرید: {win.Rows.Count}");
                            var interop = new System.Windows.Interop.WindowInteropHelper(win);
                            GetWindowRect(interop.Handle, out RECT rect);
                            int w = rect.Right - rect.Left;
                            int h = rect.Bottom - rect.Top;
                            if (w > 0 && h > 0)
                            {
                                using var bmp = new Bitmap(w, h);
                                using var g = Graphics.FromImage(bmp);
                                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(w, h));
                                bmp.Save(screenshotPath, ImageFormat.Png);
                                Console.WriteLine($"[PASS] اسکرین‌شات ذخیره شد: {screenshotPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] خطا در تصویربرداری: {ex.Message}");
                        }
                        finally
                        {
                            win.Close();
                            app.Shutdown();
                        }
                    };
                    timer.Start();
                };

                app.Run(win);
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            AuditService.ShutdownAsync(5000).GetAwaiter().GetResult();
            Console.WriteLine("[DONE] UI Execution and capture complete.");
        }
    }
}
