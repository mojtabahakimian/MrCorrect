using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using System;
using System.Linq;
using Wins.WinMenus.HESABDARI.GOZARESHAT;

namespace Prg_UI.Wins.WinMenus.HESABDARI.GOZARESHAT
{
    public static class CL_CONTORL_ASNAD
    {
        private static readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public static void StartIntegrityChecks()
        {
            #region SecuritCheck

            string Formname = "KONT"; //FORM11
            bool AllowedToOpen = CL_HESABDARI.SETSECURITY(default, Formname, default, default, IsNotWindow: true);

            #endregion
            if (!AllowedToOpen)
            {
                new Msgwin(false, "شما به این گزینه دسترسی ندارید").Show();
                return;
            }

            try
            {
                RunIntegrityChecks();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بررسی صحت اطلاعات").ShowDialog();
            }
            finally
            {
                //this.Close();
            }
        }
        private static void RunIntegrityChecks()
        {
            // 1. Check for unbalanced documents (ADAMTARAZ)
            // VBA: rst.Open "ADAMTARAZ" ... If rst.RecordCount > 0 ...
            var unbalancedCount = dbms.DoGetDataSQL<int?>("SELECT COUNT(*) FROM ADAMTARAZ").FirstOrDefault().GetValueOrDefault();
            if (unbalancedCount > 0)
            {
                new Msgwin(false, "بعضي از اسناد تراز نيستند.!!!").ShowDialog();
                new ADAMTARAZ().Show(); // VBA: DoCmd.OpenForm "ADAMTARAZ", acFormDS
            }

            // 2. Open other forms (Placeholders for now as they don't exist in the codebase)
            // VBA: DoCmd.OpenForm "hesheshes", acFormDS
            // VBA: DoCmd.OpenForm "hesheshes_mablk", acFormDS
            // VBA: DoCmd.OpenForm "hes1hes2moghyerat", acFormDS
            // implementation note: These forms were not found in the current project structure.
            new HESHESHES().Show();
            new HESHESHES_MABLK().Show();
            new HES1HES2MOGHYERAT().Show();

            // 3. Check "Documents Receivable" vs "Check Book" balance : چک های دریافتی
            CheckReceivableDocuments();

            // 4. Check "Documents Payable" vs "Check Book" balance : چک های پرداختی
            CheckPayableDocuments();

            // 5. Check visitor commission (پورسانت ویزیتور) amounts on sales invoices
            CheckVisitorCommissions();
        }

        /// <summary>
        /// بررسی مبلغ پورسانت ویزیتور فاکتورهای فروش؛ اگر مغایرتی پیدا شود
        /// پنجره‌ی «کنترل پورسانت فاکتور فروش» را باز می‌کند تا کاربر ببیند و در صورت تایید اصلاح کند.
        /// </summary>
        private static void CheckVisitorCommissions()
        {
            try
            {
                // همان منبعی که خودِ پنجره استفاده می‌کند: CL_PORSANT_RULE، یعنی دقیقاً
                // قاعده‌ای که صدور سند حسابداری هنگام نوشتن مبلغ پورسانت اجرا می‌کند.
                var mismatchCount = AUTO_BAZ.Functions.CL_PORSANT_RULE.Audit().Count;

                if (mismatchCount > 0)
                {
                    new Msgwin(false, $"مبلغ پورسانت {mismatchCount} سطر از فاکتورهای فروش با درصد/الگوی تعریف‌شده نمی‌خواند.").ShowDialog();
                    new CONTROL_PORSANT_FROOSH().Show();
                }
            }
            catch (Exception ex)
            {
                // نباید کل «کنترل اسناد و دفاتر چک» را بخواباند، ولی بی‌صدا هم رد نمی‌شود:
                // بی‌صدا بودنش باعث شده بود خطای واقعی این بخش هیچ‌وقت دیده نشود.
                AUTO_BAZ.Functions.CL_LMethods.LogWriter.WriteLog("خطا در کنترل پورسانت فاکتور فروش: " + ex.Message);
            }
        }
        private static void CheckReceivableDocuments()
        {
            // VBA: SELECT SUM(BED - BES) AS man FROM dbo.DEED_DTL WHERE (HES = ADA) OR (HES = ADV)
            string sqlSumDeed = @"SELECT SUM(BED - BES) FROM dbo.DEED_DTL 
                                  WHERE (HES = @ADA) OR (HES = @ADV)";

            var sumDeedResult = dbms.DoGetDataSQL<double?>(sqlSumDeed, new
            {
                ADA = Baseknow.ADA,
                ADV = Baseknow.ADV
            }).FirstOrDefault().GetValueOrDefault();

            // VBA: SELECT Sum(PAY_GETD.MABL) ... WHERE (((PAY_GETD.N_KOL)=BANKHA Or (PAY_GETD.N_KOL) Is Null) AND ((PAY_GETD.N_KOL2) Is Null) AND ((PAY_GETD.N_KOL3) Is Null))
            string sqlSumChecks = @"SELECT Sum(PAY_GETD.MABL) 
                                    FROM PAY_GETD 
                                    WHERE ((PAY_GETD.N_KOL = @BANKHA OR PAY_GETD.N_KOL IS NULL) 
                                    AND (PAY_GETD.N_KOL2 IS NULL) 
                                    AND (PAY_GETD.N_KOL3 IS NULL))";

            var sumChecksResult = dbms.DoGetDataSQL<double?>(sqlSumChecks, new
            {
                BANKHA = Baseknow.BANKHA
            }).FirstOrDefault().GetValueOrDefault();

            // Compare
            // VBA: If asd <> IIf(IsNull(rst.Fields(0)), 0, rst.Fields(0)) Then ...
            // Precision check: double comparison might need tolerance, but we'll stick to direct inequality as per logic logic
            if (Math.Abs(sumDeedResult - sumChecksResult) > 0.01) // Using small epsilon for float comparison safety
            {
                double difference = sumDeedResult - sumChecksResult;
                string msg = "مبلغ مانده اسناد دريافتني با دفتر چك مغايرت دارد!!!  مــبلغ  مغايرت : " + difference.ToString("N0");
                new Msgwin(false, msg).ShowDialog();

                // Open forms (Placeholders)
                new FASNAD_WITHOUT_CHEK().Show(); // DoCmd.OpenForm "FASNAD_WITHOUT_CHEK", acFormDS
                new FCHEK_WITHOUT_SANAD().Show(); // DoCmd.OpenForm "FCHEK_WITHOUT_SANAD", acFormDS
                new FCHEK_W_PASSP().Show(); // DoCmd.OpenForm "FCHEK_W_PASSP", acFormDS
                new FCHEK_W_PASS().Show();// DoCmd.OpenForm "FCHEK_W_PASS", acFormDS
            }
        }
        private static void CheckPayableDocuments()
        {
            // VBA: SELECT SUM(BES - bed) AS man FROM dbo.DEED_DTL WHERE (HES = APA) OR (HES = APV)
            string sqlSumDeed = @"SELECT SUM(BES - BED) FROM dbo.DEED_DTL 
                                  WHERE (HES = @APA) OR (HES = @APV)";

            var sumDeedResult = dbms.DoGetDataSQL<double?>(sqlSumDeed, new
            {
                APA = Baseknow.APA,
                APV = Baseknow.APV
            }).FirstOrDefault().GetValueOrDefault();

            // VBA: SELECT Sum(PAY_GETP.MABL) ... WHERE (((PAY_GETP.N_KOL)=BANKHA) AND (Not (PAY_GETP.N_MOIN) Is Null) AND ((PAY_GETP.N_KOL3) Is Null) AND ((PAY_GETP.N_kol2) Is Null))
            string sqlSumChecks = @"SELECT Sum(PAY_GETP.MABL) 
                                    FROM PAY_GETP 
                                    WHERE ((PAY_GETP.N_KOL = @BANKHA) 
                                    AND (PAY_GETP.N_MOIN IS NOT NULL) 
                                    AND (PAY_GETP.N_KOL3 IS NULL) 
                                    AND (PAY_GETP.N_kol2 IS NULL))";

            var sumChecksResult = dbms.DoGetDataSQL<double?>(sqlSumChecks, new
            {
                BANKHA = Baseknow.BANKHA
            }).FirstOrDefault().GetValueOrDefault();

            if (Math.Abs(sumDeedResult - sumChecksResult) > 0.01)
            {
                double difference = sumDeedResult - sumChecksResult;
                string msg = "مبلغ مانده اسناد پرداختني با دفتر چك مغايرت دارد!!! مبــلغ مغايرت : " + difference.ToString("N0");
                new Msgwin(false, msg).ShowDialog();
            }
        }
    }
}
