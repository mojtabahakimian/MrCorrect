using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Wins.WinSetting;
using System.Linq;
using System;
using System.IO;
using Prg_UI.HelperWins;
using Prg_UI.Functions;

namespace Wins.WinSetting
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


        private AxTINYLib.AxTiny InitializeAxTiny()
        {
            //AxTINYLib.AxTiny axTiny1 = new AxTINYLib.AxTiny
            //{
            //    ServerIP = Baseknow.SERVERNAM,
            //    Enabled = true,
            //    Initialize = true,
            //    NetWorkINIT = true
            //};

            //axTiny1.CreateControl();

            AxTINYLib.AxTiny axTiny1 = new AxTINYLib.AxTiny();
            axTiny1.CreateControl();

            axTiny1.ServerIP = Baseknow.SERVERNAM;
            axTiny1.Enabled = true;
            axTiny1.Initialize = true;
            axTiny1.NetWorkINIT = true;

            return axTiny1;
        }
        public bool TryMatchValidLock(AxTINYLib.AxTiny axTiny, string password)
        {
            axTiny.UserPassWord = password;
            axTiny.ShowTinyInfo = true;
            Baseknow.tindata = axTiny.DataPartition;

            return axTiny.TinyErrCode == 0;
        }

        /// <summary>
        /// Check Lock and additionaly return the status boolean
        /// </summary>
        /// <returns></returns>
        public bool GoCheck()
        {
            try
            {
                if (File.Exists(@"C:\mojmoh.txt"))
                {
                    LoadTindataAnyway();
                    //Ok
                }
                else
                {
                    if (IsTrialTimeEnded())
                    {
                        AxTINYLib.AxTiny axTiny1;

                        try
                        {
                            axTiny1 = InitializeAxTiny(); //Try to get regiestered files on system (Tiny.ocx)
                        }
                        catch (System.Runtime.InteropServices.COMException ex) when (ex.ErrorCode == unchecked((int)0x80040154))
                        {
                            new Msgwin(false, "فایل‌های مربوط به قفل (Tiny x64) به درستی ثبت نشده‌اند. لطفاً با استفاده از Correct Installer، این فایل‌ها را بر روی سیستم قفل نصب کنید و سپس دوباره بررسی نمایید.").ShowDialog();
                            return false;
                        }
                        catch (System.IO.FileNotFoundException ex) when (ex.HResult == unchecked((int)0x80070002))
                        {
                            new Msgwin(false, "فایل های مربوط به قفل یافت نشد. لطفاً تنظیمات قفل سخت افزاری را بررسی کنید (در مواردی دسترسی های ویندوزی و قطع ارتباط با قفل هم میتواند اشکال ایجاد کند) مجددا برنامه را اجرا کنید.").ShowDialog();
                            return false;
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "تنظیمات قفل به دسترسی انجام نشده , فایل های قفل به طور کلی در دسترس نیست").ShowDialog();
                            return false;
                        }

                        if (axTiny1.TinyErrCode != 0) //The First state of lock is ok
                        {
                            new Msgwin(false, CL_LMethods.LockReasonError(axTiny1.TinyErrCode.ToString())).ShowDialog();
                            ShowLockWin();
                            return false;
                        }

                        foreach (var password in TheKeys)
                        {
                            if (TryMatchValidLock(axTiny1, password))
                                break;
                        }

                        if (axTiny1.TinyErrCode != 0) //Still the lock is not match with lock the app needs
                        {
                            new Msgwin(false, "این قفل متعلق به این نرم افزار نیست!").ShowDialog();
                            new Msgwin(false, CL_LMethods.LockReasonError(axTiny1.TinyErrCode.ToString())).ShowDialog();
                            return false;
                        }

                        if (!string.IsNullOrEmpty(Baseknow.tindata))
                        {
                            ////1 دارا
                            //if (Strings.Mid(Baseknow.tindata, 9, 1) == "1")
                            //{
                            //    CL_HESABDARI.SETLEVEL(0);
                            //}
                            ////1 CORRECT
                            //if (Strings.Mid(System.Convert.ToString(Baseknow.tindata), 20, 7) != "CORRECT" || string.IsNullOrEmpty(Baseknow.tindata))
                            //{
                            //    CL_HESABDARI.SETLEVEL(1);
                            //}
                            //else
                            //{
                            //    //DoCmd.RunSQL "UPDATE dbo.SAL_CHEK Set RUN = 1 WHERE (OBJECT BETWEEN 368 AND 380) "
                            //}
                        }

                    }
                }
            }
            catch (System.IO.FileNotFoundException ex) when (ex.HResult == unchecked((int)0x8007007E))
            {
                new Msgwin(false, "فایل و تنظیمات مربوط به رجیستری قفل روی این سیستم انجام نشده !").ShowDialog();
                CL_LMethods.GoExitTheApplication();
                return false;
            }
            catch (Exception)
            {
                if (IsTrialTimeEnded())
                {
                    new Msgwin(false, "خطا در انجام عملیات , قفل قابل شناسایی نیست").ShowDialog();
                    ShowLockWin();
                }
                return false;
            }

            return true;
        }

        private void LoadTindataAnyway()
        {
            try
            {
                AxTINYLib.AxTiny axTiny1 = InitializeAxTiny(); //Try to get regiestered files on system (Tiny.ocx)

                foreach (var password in TheKeys)
                {
                    if (TryMatchValidLock(axTiny1, password))
                        break;
                }
            }
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
            if (recordCount > 31)
            {
                return true;
            }
            return false;
        }
    }
}
