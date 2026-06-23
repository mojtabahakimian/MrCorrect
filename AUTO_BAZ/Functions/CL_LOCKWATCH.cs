using AUTO_BAZ.HelperWins;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AUTO_BAZ.Functions
{
    public class CL_LOCKWATCH
    {
        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public readonly string[] TheKeys =
        {
          "D21336C4BBEF5189D2211240151A875",
          "BD618EC8C63B53CF2A72396FCA729FCA",  //CORRECT (mrcorrect....)
          "C1B24D1A2F74199C1391551849B865",
          "CDBAE05EDFBB33D7BA4821B25CE850D9",
          "DEA2F24BC6A323C7A95033A745F040C9" //CORRECT
        };

        public bool IsSpecial = false;

        /// <summary>
        /// چک اتصال اولیه — دقیقاً مثل VBA: فقط ServerIP و NetWorkINIT، بدون Initialize
        /// </summary>
        private bool IsLockServiceAvailable()
        {
            TINYLib.Tiny tiny = new TINYLib.Tiny();
            tiny.ServerIP = Baseknow.SERVERNAM;
            tiny.NetWorkINIT = true;
            // ← هیچ Initialize نیست — دقیقاً مثل VBA

            int err = (int)tiny.TinyErrCode;
            LockLogger.Write($"[INIT] ServerIP={Baseknow.SERVERNAM} | ErrCode={err}");
            return err == 0;
        }

        /// <summary>
        /// دقیقاً مثل VBA:
        /// instance جدید، NetWorkINIT، UserPassWord، ShowTinyInfo — بدون Initialize
        /// Data باید غیر خالی و غیر صفر باشه (تأیید واقعی match)
        /// </summary>
        private bool TryMatchKeys()
        {
            foreach (var password in TheKeys)
            {
                TINYLib.Tiny tiny = new TINYLib.Tiny();
                tiny.ServerIP = Baseknow.SERVERNAM;
                tiny.NetWorkINIT = true;
                tiny.UserPassWord = password;
                tiny.ShowTinyInfo = true;
                // ← هیچ Initialize نیست

                int err = (int)tiny.TinyErrCode;
                string serial = tiny.SerialNumber as string ?? "";
                string data = tiny.DataPartition as string ?? "";

                LockLogger.Write($"[TRY KEY] {password[..8]}... → ErrCode={err} | Serial={serial} | Data={data}");

                // ErrCode=0 و Data واقعی (نه خالی، نه همه صفر)
                bool dataValid = !string.IsNullOrEmpty(data)
                                 && data.Replace("0", "").Trim().Length > 0;

                if (err == 0 && dataValid)
                {
                    Baseknow.tindata = data;
                    LockLogger.Write($"[MATCHED] Key={password[..8]}... | Serial={serial} | Data={data}");
                    return true;
                }

                if (err == 0 && !dataValid)
                {
                    LockLogger.Write($"[SKIP] ErrCode=0 but Data invalid (false positive) — skipping");
                }
            }
            return false;
        }

        /// <summary>
        /// استفاده از lockok.xaml.cs — بدون Initialize
        /// </summary>
        public bool TryMatchValidLock(TINYLib.Tiny tiny, string password)
        {
            tiny.UserPassWord = password;
            tiny.ShowTinyInfo = true;

            int err = (int)tiny.TinyErrCode;
            string data = tiny.DataPartition as string ?? "";

            LockLogger.Write($"[lockok TRY] {password[..8]}... → ErrCode={err}");

            bool dataValid = !string.IsNullOrEmpty(data)
                             && data.Replace("0", "").Trim().Length > 0;

            if (err == 0 && dataValid)
                Baseknow.tindata = data;

            return err == 0 && dataValid;
        }

        public string LockReasonError(string ErrCode)
        {
            switch (ErrCode)
            {
                case "1": return "خطا شماره 1 - قفل شناسایی نـشد , قفل پشت سیستم نیست یا اتصال آن درست بر قرار نیست";
                case "101": return "خطا شماره 101 - قفل شناسایی نشد";
                case "2": return "خطا شماره 2 - قفل متعلق به این نرم افزار نیست.";
                case "102": return "خطا شماره 102 - قفل متعلق به این نرم افزار نیست.";
                case "3": return "خطا شماره 3 - سرویس قفل را در محل قفل بررسی کنید و شبکه را نیز چک کنید.";
                case "5": return "خطا شماره 5 - معمولا این مشکل زمانی رخ میدهد که از نام کامپیوتر به جای آدرس آی پی استفاده شده باشد.";
                case "6": return "خطا شماره 6 - احتمالا سرویس قفل غیر فعال است , آنرا بررسی کنید (پورت 5090 و 9051 را برای فایروال فعال کنید).";
                case "106": return "خطا شماره 106 - معتبر بودن IP و ارتباط شبکه را بررسی نمایید.";
                case "7": return "خطا شماره 7 - تعداد کاربران متصل بیش از حد مجاز است.";
                case "107": return "خطا شماره 107 - تعداد کاربران متصل بیش از حد مجاز است.";
                case "8": return "خطا شماره 8 - تنظیمات مربوط به قفل مجدداً انجام و بروز رسانی شود.";
                case "9": return "خطا شماره 9 - اکتیوایکس و سرویس را آپدیت کنید.";
                case "10": return "خطا شماره 10 - داده های مربوطه درست نیست.";
                case "11": return "خطا شماره 11 - تنظیمات قفل بروز شود.";
                default: return "داده های مربوطه به قفل صحیح نیست.";
            }
        }

        public bool GoCheck()
        {
            LockLogger.Clear();
            LockLogger.Write($"=== GoCheck Start | ServerIP: {Baseknow.SERVERNAM} ===");

            try
            {
                if (File.Exists(@"C:\mojmoh.txt"))
                {
                    LockLogger.Write("[BYPASS] mojmoh.txt found");
                    LoadTindataAnyway();
                    IsSpecial = true;
                    return true;
                }

                if (!IsTrialTimeEnded())
                {
                    LockLogger.Write("[TRIAL] Trial not ended - skipped");
                    return true;
                }

                LockLogger.Write("[TRIAL] Trial ended - checking hardware lock");

                try
                {
                    if (!IsLockServiceAvailable())
                    {
                        LockLogger.Write("[FAIL] Lock service not available.");
                        new Msgwin(false, LockReasonError("6")).ShowDialog();
                        ShowLockWin();
                        return false;
                    }
                }
                catch (System.Runtime.InteropServices.COMException ex)
                    when (ex.ErrorCode == unchecked((int)0x80040154))
                {
                    LockLogger.Write($"[COM ERROR] TINYLib not registered: {ex.Message}");
                    new Msgwin(false, "فایل‌های مربوط به قفل (TINYLib) به درستی ثبت نشده‌اند.").ShowDialog();
                    return false;
                }
                catch (Exception ex)
                {
                    LockLogger.Write($"[INIT EXCEPTION] {ex.GetType().Name}: {ex.Message}");
                    new Msgwin(false, "تنظیمات قفل در دسترس نیست.").ShowDialog();
                    return false;
                }

                LockLogger.Write("[PRE-MATCH] Lock service OK - trying keys...");

                if (!TryMatchKeys())
                {
                    LockLogger.Write("[NO MATCH] No key matched.");
                    new Msgwin(false, "این قفل متعلق به این نرم افزار نیست!").ShowDialog();
                    new Msgwin(false, LockReasonError("2")).ShowDialog();
                    return false;
                }
            }
            catch (System.IO.FileNotFoundException ex)
                when (ex.HResult == unchecked((int)0x8007007E))
            {
                LockLogger.Write($"[FILE NOT FOUND] {ex.Message}");
                new Msgwin(false, "فایل و تنظیمات مربوط به رجیستری قفل روی این سیستم انجام نشده!").ShowDialog();
                Application.Current.Shutdown();
                return false;
            }
            catch (Exception ex)
            {
                LockLogger.Write($"[OUTER EXCEPTION] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                if (IsTrialTimeEnded())
                {
                    new Msgwin(false, "خطا در انجام عملیات , قفل قابل شناسایی نیست").ShowDialog();
                    ShowLockWin();
                }
                return false;
            }

            LockLogger.Write($"=== GoCheck SUCCESS | tindata={Baseknow.tindata} ===");
            return true;
        }

        private void LoadTindataAnyway()
        {
            try { TryMatchKeys(); }
            catch { }
        }

        private static void ShowLockWin()
        {
            lockok lockDialog = new lockok();
            lockDialog.ShowDialog();
        }

        public bool IsTrialTimeEnded()
        {
            var recordCount = dbms.DoGetDataSQL<int?>("SELECT COUNT(N_S) AS CN_S FROM DEED_HED").FirstOrDefault();
            return recordCount > 31;
        }
    }
}
