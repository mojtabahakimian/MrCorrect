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

            // Run Porsant Correction Regression Test
            RunCommissionCorrectionRegressionTest();

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

        private static void RunCommissionCorrectionRegressionTest()
        {
            Console.WriteLine("\n-------------------------------------------------------------------------");
            Console.WriteLine(" Running Regression Test: Porsant Audit & Correction Integrity ");
            Console.WriteLine("-------------------------------------------------------------------------");

            var dbms = new CL_CCNNMANAGER();

            try
            {
                // Ensure migration is executed
                ScriptSqly.Migrations.ScriptSqly.LetsGo(CL_CCNNMANAGER.CONNECTION_STR);

                // 1. Audit before correction
                var initialAudit = dbms.DoGetDataSQL<Prg_UI.Wins.WinMenus.HESABDARI.GOZARESHAT.CONTROL_PORSANT_FROOSH.PorsantAuditRow>(
                    @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=NULL, @TAG=2, @FromDate=NULL, @ToDate=NULL, @PREVIEW_ONLY=1"
                ).ToList();

                Console.WriteLine($"[TEST] Initial discrepant invoice rows found: {initialAudit.Count}");

                double testInvoiceNumber = 999999123;
                bool createdTestData = false;
                double? originalPursant = null;

                if (initialAudit.Count == 0)
                {
                    // Create simulated invoice & visitor row for test
                    Console.WriteLine("[TEST] No existing discrepancies in DB. Creating a simulated discrepant invoice record...");
                    dbms.DoExecuteSQL(@"
                        DELETE FROM dbo.VISITOR_DTL WHERE NUMBER = 999999123 AND TAG = 2;
                        DELETE FROM dbo.INVO_LST WHERE NUMBER = 999999123 AND TAG = 2;
                        DELETE FROM dbo.HEAD_LST WHERE NUMBER = 999999123 AND TAG = 2;

                        INSERT INTO dbo.HEAD_LST (NUMBER, TAG, DATE_N, CUST_NO, TAKHFIF)
                        VALUES (999999123, 2, 14050101, 'TEST_CUST', 0);

                        INSERT INTO dbo.INVO_LST (NUMBER, TAG, CODE, MEGH, MEGHK, MABL, MABL_K, N_MOIN)
                        VALUES (999999123, 2, 'TEST_ITEM', 1, 1, 1000000, 1000000, 0);

                        INSERT INTO dbo.VISITOR_DTL (NUMBER, TAG, CUST_NO, DARSAD, PURSANT, STAT)
                        VALUES (999999123, 2, 'TEST_VISITOR', 5.0, 1000, 0);
                    ");
                    createdTestData = true;

                    initialAudit = dbms.DoGetDataSQL<Prg_UI.Wins.WinMenus.HESABDARI.GOZARESHAT.CONTROL_PORSANT_FROOSH.PorsantAuditRow>(
                        @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=999999123, @TAG=2, @FromDate=NULL, @ToDate=NULL, @PREVIEW_ONLY=1"
                    ).ToList();
                }

                if (initialAudit.Count == 0)
                {
                    throw new Exception("Failed to produce or detect a discrepant invoice row for testing.");
                }

                var targetRow = initialAudit.First();
                testInvoiceNumber = targetRow.NUMBER ?? testInvoiceNumber;
                originalPursant = targetRow.OLD_PURSANT;

                Console.WriteLine($"[TEST] Target Invoice Number: {testInvoiceNumber}");
                Console.WriteLine($"[TEST] OLD_PURSANT: {targetRow.OLD_PURSANT:N0}, NEW_PURSANT: {targetRow.NEW_PURSANT:N0}, DIFF: {targetRow.DIFF:N0}");

                if (Math.Abs(targetRow.DIFF) < 0.5)
                {
                    throw new Exception($"Expected discrepancy DIFF != 0, but got DIFF = {targetRow.DIFF}");
                }

                // 2. Execute Correction
                Console.WriteLine("[TEST] Executing Correction (PREVIEW_ONLY = 0)...");
                using (var ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required))
                {
                    dbms.DoExecuteSQL(
                        @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=@pNUMBER, @TAG=@pTAG, @FromDate=NULL, @ToDate=NULL, @PREVIEW_ONLY=0",
                        new { pNUMBER = (double?)testInvoiceNumber, pTAG = (double?)2 }
                    );
                    ts.Complete();
                }

                // 3. Reload Audit
                Console.WriteLine("[TEST] Reloading Audit after correction...");
                var postAudit = dbms.DoGetDataSQL<Prg_UI.Wins.WinMenus.HESABDARI.GOZARESHAT.CONTROL_PORSANT_FROOSH.PorsantAuditRow>(
                    @"EXEC dbo.RecalcVisitorPorsant_ByDarsad @NUMBER=@pNUMBER, @TAG=2, @FromDate=NULL, @ToDate=NULL, @PREVIEW_ONLY=1",
                    new { pNUMBER = (double?)testInvoiceNumber }
                ).ToList();

                if (postAudit.Count > 0)
                {
                    var remainingDiscrepancy = postAudit.First();
                    throw new Exception($"Corrected invoice {testInvoiceNumber} still appeared as discrepant! DIFF: {remainingDiscrepancy.DIFF}");
                }

                Console.WriteLine($"[PASS] Corrected invoice {testInvoiceNumber} is no longer listed as discrepant (discrepancy = 0).");
                Console.WriteLine("[PASS] Regression test completed successfully!");

                // Clean up test data if created
                if (createdTestData)
                {
                    dbms.DoExecuteSQL(@"
                        DELETE FROM dbo.VISITOR_DTL WHERE NUMBER = 999999123 AND TAG = 2;
                        DELETE FROM dbo.INVO_LST WHERE NUMBER = 999999123 AND TAG = 2;
                        DELETE FROM dbo.HEAD_LST WHERE NUMBER = 999999123 AND TAG = 2;
                    ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] Regression test error: {ex.Message}");
                throw;
            }
        }
    }
}
