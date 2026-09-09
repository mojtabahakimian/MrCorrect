using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;

namespace TestRunner
{
    internal class Program
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

        [STAThread]
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args != null && args.Any(a => string.Equals(a, "test-autobaz-morvarid", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("[INFO] Initializing environment for Morvarid_1404_6Month_2...");
                Baseknow.USERCOD = 78;
                Baseknow.UUSER = "Controller";
                CL_Generaly.SHIFT_OF_USER = 1;
                CL_Generaly.VAHED_OF_USER = 1;
                Baseknow.UGRP = "1";
                CL_Generaly.IsCalledExternally = true;
                CL_Generaly.General_Servername = @"MERCEDES\SQL2022";
                CL_Generaly.General_DBname = "Morvarid_1404_6Month_2";

                string targetDb = "Morvarid_1404_6Month_2";
                CL_CCNNMANAGER.CONNECTION_STR = $"Data Source=MERCEDES\\SQL2022;Initial Catalog={targetDb};Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
                CL_CCNNMANAGER.ConnectedToSQLDB = true;
                Baseknow.GetInitTheApp();
                Console.WriteLine($"[PASS] Baseknow initialized. STMO: {Baseknow.STMO}");

                var appInstance = Application.Current ?? new Application();
                var resourceUris = new[]
                {
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Secondary/MaterialDesignColor.LightBlue.xaml",
                    "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.ObsoleteBrushes.xaml"
                };
                foreach (var u in resourceUris)
                {
                    try { appInstance.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(u, UriKind.RelativeOrAbsolute) }); } catch { }
                }

                var main = new AUTO_BAZ.MainWindow();

                Console.WriteLine("[INFO] Running C0_TASK (Rebuild average rate)...");
                main.C0_TASK().GetAwaiter().GetResult();
                Console.WriteLine("[PASS] C0_TASK completed successfully.");

                Console.WriteLine("[INFO] Running C1_TASK (Sales invoice deeds)...");
                main.C1_TASK().GetAwaiter().GetResult();
                Console.WriteLine("[PASS] C1_TASK completed successfully.");

                Console.WriteLine("[INFO] Running C8_TASK (Sales return deeds)...");
                main.C8_TASK().GetAwaiter().GetResult();
                Console.WriteLine("[PASS] C8_TASK completed successfully.");

                // Check MOGHA_ANBAR for the 3 codes
                using var cnn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
                cnn.Open();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT CODE, MABLK, MAND, mab, tafBED, TAFBES FROM dbo.MOGHA_ANBAR(14040631, 6, 115) WHERE CODE IN ('5677', '5666', '5662') ORDER BY CODE";
                using var r = cmd.ExecuteReader();
                Console.WriteLine("=========================================================================");
                Console.WriteLine("          POST-RUN VERIFICATION: dbo.MOGHA_ANBAR(14040631, 6, 115)       ");
                Console.WriteLine("=========================================================================");
                while (r.Read())
                {
                    Console.WriteLine($"CODE: {r["CODE"]} | MABLK: {r["MABLK"]} | MAND: {r["MAND"]} | mab: {r["mab"]} | tafBED: {r["tafBED"]} | TAFBES: {r["TAFBES"]}");
                }
                return;
            }

            // تست رگرسیون اصلاح پورسانت فاکتور فروش؛ هارنس بصری را اجرا نمی‌کند.
            //   TestRunner.exe porsant           فقط قاعده‌ی محاسبه (بدون دیتابیس)
            //   TestRunner.exe porsant --apply   چرخه‌ی کامل روی دیتابیس (روی داده می‌نویسد)
            if (args != null && args.Any(a => string.Equals(a, "porsant", StringComparison.OrdinalIgnoreCase)))
            {
                bool applyOnDatabase = args.Any(a => string.Equals(a, "--apply", StringComparison.OrdinalIgnoreCase));

                if (applyOnDatabase)
                {
                    Baseknow.USERCOD = 78;
                    Baseknow.UUSER = "Controller";
                    CL_Generaly.IsCalledExternally = true;
                    CL_CCNNMANAGER.CONNECTION_STR = "Data Source=MERCEDES\\SQL2022;Initial Catalog=YAZDSEPAR1405;Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
                    CL_CCNNMANAGER.ConnectedToSQLDB = true;
                    Baseknow.GetInitTheApp();
                }

                Environment.ExitCode = PorsantCorrectionTest.Run(applyOnDatabase);
                return;
            }

            Console.WriteLine("=========================================================================");
            Console.WriteLine("          VISUAL / UI HARNESS VERIFICATION FOR MrCorrect                 ");
            Console.WriteLine("=========================================================================");

            Baseknow.USERCOD = 78;
            Baseknow.UUSER = "Controller";
            CL_Generaly.SHIFT_OF_USER = 1;
            CL_Generaly.VAHED_OF_USER = 1;
            Baseknow.UGRP = "1";
            CL_Generaly.IsCalledExternally = true;

            string dbName = "YAZDSEPAR1405";
            CL_CCNNMANAGER.CONNECTION_STR = $"Data Source=MERCEDES\\SQL2022;Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            CL_CCNNMANAGER.ConnectedToSQLDB = true;

            Baseknow.GetInitTheApp();
            Console.WriteLine($"[PASS] Baseknow initialized. STMO: {Baseknow.STMO}");

            // Create WPF Application context
            var app = new Application();

            // Load Resource Dictionaries matching App.xaml
            string[] resourceDicts = new[]
            {
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml",
                "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.DeepPurple.xaml",
                "pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor.Lime.xaml"
            };

            foreach (var uri in resourceDicts)
            {
                try
                {
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(uri, UriKind.RelativeOrAbsolute) });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[INFO] Resource skip: {uri} ({ex.Message})");
                }
            }

            // Launch HEAD_LST_KHAREED1 for Direct Purchase (IsDirectFactor = true)
            try
            {
                Console.WriteLine("Instantiating HEAD_LST_KHAREED1 (Direct Purchase)...");
                var win = new Wins.WinMenus.KHARID_FORUSH.HEAD_LST_KHAREED1(null, _IsDirectFactor_: true);
                
                win.Loaded += (s, e) =>
                {
                    Console.WriteLine("Window loaded. Title: " + win.Title);
                    
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (ts, te) =>
                    {
                        timer.Stop();
                        try
                        {
                            var interop = new System.Windows.Interop.WindowInteropHelper(win);
                            GetWindowRect(interop.Handle, out RECT rect);
                            int width = rect.Right - rect.Left;
                            int height = rect.Bottom - rect.Top;

                            if (width > 0 && height > 0)
                            {
                                using var bmp = new Bitmap(width, height);
                                using var g = Graphics.FromImage(bmp);
                                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height));
                                
                                string screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "direct_purchase_screenshot.png");
                                bmp.Save(screenshotPath, ImageFormat.Png);
                                Console.WriteLine($"[PASS] Visual screenshot saved: {screenshotPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARN] Screenshot capture: {ex.Message}");
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
                Console.WriteLine("\n🎉 Visual UI verification finished successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] UI Harness exception: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
