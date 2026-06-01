using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using iText.Layout.Font;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Prg_UI.Wins.WinMenus.WinDEFAULT;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.Taarif;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Path = System.IO.Path;

namespace Prg_Proccessy.FUNCTIONS
{
    public static class CL_HESABDARI
    {
        static CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private static readonly Random _rnd = new Random();

        // قرارش بده داخل کلاس GRADE_FORMAT_WIN
        public sealed class _UpformatAgg
        {
            public decimal SZ { get; set; }   // Sum of GFGZARIB (سطح ۱)
            public decimal SGE { get; set; }  // Sum of (Avg(level2) * GFGZARIB(level1))
        }
        public class ResultUpformModel
        {
            public double JAMZARIB { get; internal set; }
            public double EMTIAZ { get; internal set; }
        }
        public static ResultUpformModel Upformat(int? gfid)
        {
            if (gfid == null) return null;

            var agg = dbms.DoGetDataSQL<_UpformatAgg>(@"
                    ;WITH grp AS (
                        SELECT 
                            GFTID,
                            SUM(CAST(GFGRPZARIB AS decimal(18,6))) AS sumZ,
                            SUM(CAST(GFGRPGRADE AS decimal(18,6)) * CAST(GFGRPZARIB AS decimal(18,6))) AS sumZG
                        FROM dbo.GRADE_GRP_FT
                        GROUP BY GFTID
                    ),
                    tab AS (
                        SELECT 
                            t.GFTID,
                            CAST(t.GFGZARIB AS decimal(18,6)) AS tabZ
                        FROM dbo.GRADE_TAB_FT AS t
                        WHERE t.GFID = @GFID
                    )
                    SELECT
                        SZ  = ISNULL((SELECT SUM(tabZ) FROM tab), 0),
                        SGE = ISNULL(SUM(CASE WHEN g.sumZ <> 0 THEN (g.sumZG / g.sumZ) * t.tabZ ELSE 0 END), 0)
                    FROM tab AS t
                    LEFT JOIN grp AS g ON g.GFTID = t.GFTID
                ", new { GFID = gfid }).FirstOrDefault();

            var master = new ResultUpformModel();
            var SZ = agg?.SZ ?? 0m;
            var SGE = agg?.SGE ?? 0m;

            if (SZ != 0m)
            {
                var emtiaz = SGE / SZ;

                dbms.DoExecuteSQL(
                    "UPDATE dbo.GRADE_FORMAT SET JAMZARIB=@SZ, EMTIAZ=@EM WHERE IDD=@ID",
                    new { SZ, EM = emtiaz, ID = gfid });

                master.JAMZARIB = (double)SZ;
                master.EMTIAZ = (double)emtiaz;
            }

            return master;
        }

        #region Custom_Modelses
        public class PriceDtl
        {
            public double PRICE1 { get; set; }
            public int PGID { get; set; }
            public string CODE { get; set; }
            public int NUMBER { get; set; }
            public int TAG { get; set; }
            public double MEGH { get; set; }
            public double MEGHK { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public int? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
            public double? IMBAA { get; set; }
            public long PEPID { get; set; }
        }

        public class TFModel
        {
            public long PEID { get; set; }
            public double TF1 { get; set; }
            public double TF2 { get; set; }
        }
        public class InvoLstFF
        {
            public string CODE { get; set; }
            public double? TKHN { get; set; }
            public double MEGHK { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public double? N_KOL { get; set; }
            public double? N_MOIN { get; set; }
            public double? IMBAA { get; set; }
            public int NUMBER { get; set; }
            public int TAG { get; set; }
            public bool CMBAA { get; set; }
        }
        public class LSQ8
        {
            public Int32? USERCO { get; set; }
            public Boolean? SGN0126 { get; set; }
            public Boolean? SGN0226 { get; set; }
            public Boolean? SGN0326 { get; set; }
        }

        public class LSQ7
        {
            public int? USERCO { get; set; }
            public bool? ANB_KHOROG_MODIRT { get; set; }
            public bool? ANB_KHOROG_ANB { get; set; }
            public bool? HES { get; set; }
        }
        public class LSQ6
        {
            public double? SumOfMABL_K { get; set; }
            public double? MEGHk { get; set; }
        }
        public class LSQ5
        {
            public double? NUMBER { get; set; }
            public double? TAG { get; set; }
            public double? NUMBER1 { get; set; }
            public long? DATE_N { get; set; }
            public string? TAH { get; set; }
            public double? MAS { get; set; }
            public double? VAS { get; set; }
            public double? N_S { get; set; }
            public string? CUST_NO { get; set; }
            public string? MOLAH { get; set; }
            public double? M_NAGHD { get; set; }
            public double? MABL_VAR { get; set; }
            public string? MOIN_VAR { get; set; }
            public double? MABL_HAV { get; set; }
            public double? MABL_HAZ { get; set; }
            public double? TAKHFIF { get; set; }
            public double? FNUMCO { get; set; }
            public string? USER_NAME { get; set; }
            public string? SHARAYET { get; set; }
            public double? MBAA { get; set; }
            public string? NAME { get; set; }
            public string? REQUEST_NO { get; set; }
            public string? BARNAMEH { get; set; }
            public string? DRIVER { get; set; }
            public string? DRIVER_MOB { get; set; }
            public string? CAMIUN_NUM { get; set; }
            public double? MAGHSAD { get; set; }
            public double? CAM_KHALY { get; set; }
            public double? CAM_POOR { get; set; }
            public string? TOZIH { get; set; }
        }
        public class LSQ4
        {
            public int? CODE { get; set; }
            public int? SumOfMORAKHDAY { get; set; }
        }
        public class LSQ3
        {
            public int? USERCO { get; set; }
            public bool? SGN0124 { get; set; }
            public bool? SGN0224 { get; set; }
            public bool? SGN0324 { get; set; }
        }
        public class LSQ2
        {
            public int? USERCO { get; set; }
            public bool? PAY_MVAHED { get; set; }
            public bool? PAY_HESAB { get; set; }
            public bool? PAY_MAMEL { get; set; }
        }
        public class LSQ1
        {
            public double? SZ { get; set; }
            public double? SGE { get; set; }
        }
        public class CL_QT
        {
            public int? USERCO { get; set; }
            public bool? SND_TAHI { get; set; }
            public bool? SND_MALI { get; set; }
            public bool? SND_MODIR { get; set; }
        }
        public class CL_QR
        {
            public string? CAPTION { get; set; }
            public short? kind { get; set; }
            public int? USERCO { get; set; }
            public bool? RUN { get; set; }
            public bool? SEE { get; set; }
            public bool? INP { get; set; }
            public bool? UPD { get; set; }
            public bool? DEL { get; set; }
        }
        public class RST2_Data
        {
            //Deed_DTL
            //Deed_HED
            public double? FR { get; set; }
            public double N_S { get; set; }
            public long DATE_S { get; set; }
            public int HES_K { get; set; }
        }
        public class RST_PAY
        {
            //Deed_DTL
            //Deed_HED
            public double? JCH { get; set; }
        }
        private class GTNMVAHED
        {
            public string CODE { get; set; }
            public double? VAHED { get; set; }
            public double? NESBAT { get; set; }
        }
        private class SHAHR
        {
            public string EN_CUST_CO { get; set; }
            public int EN_CITY { get; set; }
            public int EN_IYALAT { get; set; }
            public int EN_COUNTRY { get; set; }
        }
        public partial class Custom_TF12
        {
            public float TF1 { get; set; }
            public float TF2 { get; set; }
        }
        public partial class Custom_PEID
        {
            public int PEID { get; set; }
        }
        private partial class Custom_PRICEONE
        {
            public float PRICE1 { get; set; }
            public int PGID { get; set; }
            public string CODE { get; set; }
            public int PEPID { get; set; }
        }
        private partial class Custom_PRICE_ELAMIE1
        {
            public int PEPID { get; set; }
        }
        public partial class Custom_PRICE_ELAMIE
        {
            public int PEPID { get; set; }
        }
        public partial class Custom2_PRICE_ELAMIE
        {
            public float PRICE1 { get; set; }
            public int PGID { get; set; }
            public int PEPID { get; set; }
            public string CODE { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public int ANBAR { get; set; }
            public double MEGH { get; set; }
            public double MEGHk { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public Nullable<double> N_KOL { get; set; }
            public Nullable<double> IMBAA { get; set; }
            public Nullable<double> N_MOIN { get; set; }
        }
        public partial class Custom3_PRICE_ELAMIE
        {
            public int PEPID { get; set; }
            public float TF1 { get; set; }
            public float TF2 { get; set; }
        }
        public partial class Custom_INVO_LST
        {
            public string CODE { get; set; }
        }
        public partial class Custom_STUF_DEF
        {
            public string CODE { get; set; }
            public string NAME { get; set; }
        }
        public partial class Custom_INVO_STUF
        {
            public string CODE { get; set; }
            public Nullable<double> TKHN { get; set; }
            public double MEGHk { get; set; }
            public double MABL { get; set; }
            public double MABL_K { get; set; }
            public Nullable<double> N_KOL { get; set; }
            public Nullable<double> N_MOIN { get; set; }
            public Nullable<double> IMBAA { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
            public Nullable<bool> CMBAA { get; set; }
        }
        public partial class Custom_LETSGO
        {
            //FORMNAME,USERCO,RUN,SEE,INP,UPD,DEL
            public string FORMNAME { get; set; }
            public int USERCO { get; set; }
            public Nullable<bool> RUN { get; set; }
            public Nullable<bool> SEE { get; set; }
            public Nullable<bool> INP { get; set; }
            public Nullable<bool> UPD { get; set; }
            public Nullable<bool> DEL { get; set; }
        }
        public partial class Custom_StuFSK
        {
            public Nullable<double> MIN_M { get; set; }
        }
        public partial class Custom_modat
        {
            public int PPID { get; set; }
            public int MODAT { get; set; }
        }
        class CustomQre2
        {
            public int? USERCO { get; set; }
            public string? HES { get; set; }
        }
        #endregion


        /// <summary>
        /// Get Safe New ID of Table in Serializable level
        /// </summary>
        /// <param name="_column_name"></param>
        /// <param name="_table_name"></param>
        /// <param name="_second_column">Give it a editable not matter column like "MANDAH" | For Fake Query to Lock Table</param>
        /// <returns></returns>
        public static long? GetNewIDD(string _column_name, string _table_name, string _second_column)
        {
            long NEWIDD = -1;
            try
            {
                using (IDbConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();

                    using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                    {
                        //Effectless Query just to Lock table while this current query is working
                        db.Execute($"UPDATE TOP(1) {_table_name} SET {_second_column} = {_second_column}", null, transaction);
                        //////db.Execute($"UPDATE TOP(1) {_table_name} WITH (TABLOCK) SET {_second_column} = {_second_column}", null, transaction);
                        //db.Execute($"SELECT 1 FROM {_table_name} WITH (TABLOCKX, HOLDLOCK)", null, transaction); //Best Way to ensure table will entirly locked

                        //Effectless Query just to Lock table while this current query is working

                        NEWIDD = Convert.ToInt64(db.Query<long?>($"SELECT MAX({_column_name}) FROM {_table_name}", null, transaction).FirstOrDefault());
                        if (NEWIDD <= 0) NEWIDD = 1;
                        else
                            NEWIDD = NEWIDD + 1;

                        transaction.Commit();
                        db?.Close();

                        return NEWIDD;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetBetweenStr(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) || strSource.Contains(strEnd))
            {
                var nstrsorce = strSource.Replace(" ", "");
                var nstrstart = strStart.Replace(" ", "");
                var nstrend = strEnd.Replace(" ", "");
                int Start, End;
                Start = nstrsorce.IndexOf(nstrstart, 0) + nstrstart.Length;
                End = nstrsorce.IndexOf(nstrend, Start);
                if (End == -1)
                {
                    End = nstrsorce.Length;
                }
                return nstrsorce.Substring(Start, End - Start);
            }
            return "";
        }

        //______________

        public static double GetJAMFR(string HES, int mm)
        {
            try
            {
                string sql = $@"SELECT SUM(dbo.FACTOREFROOSH.MABL_K - dbo.FACTOREFROOSH.N_MOIN + dbo.FACTOREFROOSH.IMBAA) AS MAB 
                                FROM dbo.HEAD_LST 
                                INNER JOIN dbo.FACTOREFROOSH ON dbo.HEAD_LST.NUMBER = dbo.FACTOREFROOSH.NUMBER 
                                    AND dbo.HEAD_LST.TAG - 11 = dbo.FACTOREFROOSH.TAG 
                                WHERE (dbo.Umonth(dbo.HEAD_LST.DATE_N) = {mm}) 
                                    AND (dbo.HEAD_LST.CUST_NO = N'{HES}') 
                                    AND (dbo.HEAD_LST.TAG = 13)";

                var result = dbms.DoGetDataSQL<double?>(sql).FirstOrDefault();
                return result ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public static int GetRASPAY(string HES, int mm, double MAND, long TARIKHMABNA)
        {
            try
            {
                string sql = $@"SELECT TOP (100) PERCENT 
                                    dbo.DEED_DTL.HES, dbo.DEED_DTL.BED, dbo.DEED_DTL.BES, dbo.DEED_DTL.N_SERI, 
                                    dbo.DEED_DTL.BANK, dbo.DEED_DTL.NUMBER, dbo.DEED_DTL.TAG, dbo.DEED_DTL.ID, 
                                    dbo.DEED_HED.DATE_S, dbo.DEED_HED.N_S, dbo.PAY_GETD.DATE_S AS PDT 
                                FROM dbo.DEED_HED 
                                INNER JOIN dbo.DEED_DTL ON dbo.DEED_HED.N_S = dbo.DEED_DTL.N_S 
                                LEFT OUTER JOIN dbo.PAY_GETD ON dbo.DEED_DTL.N_SERI = dbo.PAY_GETD.N_SERI 
                                    AND dbo.DEED_DTL.BANK = dbo.PAY_GETD.BANK 
                                WHERE (dbo.DEED_DTL.HES = N'{HES}') 
                                ORDER BY dbo.DEED_HED.DATE_S, dbo.PAY_GETD.DATE_S";

                var records = dbms.DoGetDataSQL<RasPayDto>(sql).ToList();

                bool okk = false;
                double mabChk = 0;
                double mabChkR = 0;
                double notch = 0;
                double mandOld = 0;

                int index = 0;
                while (index < records.Count && !okk)
                {
                    var rec = records[index];
                    MAND -= rec.BES;

                    if (MAND <= 0)
                    {
                        okk = true;
                        if (MAND < 0)
                        {
                            while (index < records.Count)
                            {
                                rec = records[index];
                                if (rec.BES > 0)
                                {
                                    double diffu = 0;

                                    if (rec.PDT.HasValue)
                                    {
                                        diffu = CL_HESABDARI.DIFF(TARIKHMABNA, rec.PDT.Value);
                                        if (MAND < 0)
                                        {
                                            mabChk += Math.Abs(MAND);
                                            mabChkR += Math.Abs(MAND) * diffu;
                                            mandOld = MAND;
                                            MAND = 0;
                                        }
                                        else
                                        {
                                            mabChk += rec.BES;
                                            mabChkR += rec.BES * diffu;
                                        }
                                    }
                                    else
                                    {
                                        notch += rec.BES;
                                    }

                                    long scmid = CL_HESABDARI.GetLIDD("SSM_CUST_RASPAY", "SCMID");
                                    string pdtVal = rec.PDT.HasValue ? rec.PDT.Value.ToString() : "NULL";

                                    string insertSql = $@"INSERT INTO SSM_CUST_RASPAY 
                                    (SCMID, CUST_NO, SCMDATECH, SCMDATE_S, SCMMABNADATE, SCMMABL, SCMMABCHK, SCMMABCHKR, SCMMAND, SCMDIFF, NOTCH, USERCO, MONTH)
                                    VALUES ({scmid}, N'{HES}', {pdtVal}, {rec.DATE_S}, {TARIKHMABNA}, {rec.BES}, {mabChk}, {mabChkR}, {mandOld}, {diffu}, {notch}, {Baseknow.USERCOD}, {mm})";

                                    dbms.DoExecuteSQL(insertSql);
                                }
                                index++;
                            }
                        }
                    }
                    else
                    {
                        index++;
                    }
                }

                if (MAND > 0) return 10000;

                if ((mabChk + notch) != 0)
                {
                    return (int)Math.Round(mabChkR / (mabChk + notch));
                }
                else
                {
                    return 1;
                }
            }
            catch
            {
                return 1;
            }
        }

        public static int GetRASTAVAF(string HES, int mm)
        {
            try
            {
                string sql = $@"SELECT 
                                    SUM((dbo.FACTOREFROOSH.MABL_K - dbo.FACTOREFROOSH.N_MOIN + dbo.FACTOREFROOSH.IMBAA) * 
                                        CASE WHEN dbo.HEAD_LST.MODAT_PPID = 0 THEN dbo.HEAD_LST.MAS ELSE dbo.PRICE_PAYNO.MODAT END) AS MABKR, 
                                    SUM(dbo.FACTOREFROOSH.MABL_K - dbo.FACTOREFROOSH.N_MOIN + dbo.FACTOREFROOSH.IMBAA) AS MAB 
                                FROM dbo.HEAD_LST 
                                INNER JOIN dbo.FACTOREFROOSH ON dbo.HEAD_LST.NUMBER = dbo.FACTOREFROOSH.NUMBER 
                                    AND dbo.HEAD_LST.TAG - 11 = dbo.FACTOREFROOSH.TAG 
                                LEFT OUTER JOIN dbo.PRICE_PAYNO ON dbo.HEAD_LST.MODAT_PPID = dbo.PRICE_PAYNO.PPID 
                                WHERE (dbo.Umonth(dbo.HEAD_LST.DATE_N) = {mm}) 
                                    AND (dbo.HEAD_LST.CUST_NO = N'{HES}') 
                                    AND (dbo.HEAD_LST.TAG = 13)";

                var res = dbms.DoGetDataSQL<RasFrDto>(sql).FirstOrDefault();

                if (res == null || (res.MAB ?? 0) == 0) return 0;

                return (int)Math.Round((res.MABKR ?? 0) / (res.MAB ?? 0));
            }
            catch
            {
                return 0;
            }
        }

        public static int GetRASFR(string HES, int mm)
        {
            try
            {
                long refDate = long.Parse($"{Baseknow.YEA}{mm:00}01");

                string sql1 = $@"SELECT 
                                    SUM((dbo.FACTOREFROOSH.MABL_K - dbo.FACTOREFROOSH.N_MOIN + dbo.FACTOREFROOSH.IMBAA) * dbo.Udatediff(dbo.HEAD_LST.DATE_N, {refDate})) AS MABKR, 
                                    SUM(dbo.FACTOREFROOSH.MABL_K - dbo.FACTOREFROOSH.N_MOIN + dbo.FACTOREFROOSH.IMBAA) AS MAB 
                                FROM dbo.HEAD_LST 
                                INNER JOIN dbo.FACTOREFROOSH ON dbo.HEAD_LST.NUMBER = dbo.FACTOREFROOSH.NUMBER 
                                    AND dbo.HEAD_LST.TAG - 11 = dbo.FACTOREFROOSH.TAG 
                                WHERE (dbo.Umonth(dbo.HEAD_LST.DATE_N) = {mm}) 
                                    AND (dbo.HEAD_LST.CUST_NO = N'{HES}') 
                                    AND (dbo.HEAD_LST.TAG = 13)";

                var res1 = dbms.DoGetDataSQL<RasFrDto>(sql1).FirstOrDefault();

                if (res1 == null || (res1.MAB ?? 0) == 0) return 0;

                string sql2 = $@"SELECT SUM(dbo.DEED_DTL.BED) AS nofa 
                                 FROM dbo.DEED_DTL 
                                 INNER JOIN dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S 
                                 WHERE (dbo.Umonth(dbo.DEED_HED.DATE_S) = {mm}) 
                                   AND (dbo.DEED_DTL.HES = N'{HES}') 
                                   AND (dbo.DEED_DTL.TAG <> 13 OR dbo.DEED_DTL.TAG IS NULL)";

                double nofa = dbms.DoGetDataSQL<double?>(sql2).FirstOrDefault() ?? 0;

                double denominator = (res1.MAB ?? 0) + nofa;
                if (denominator == 0) return 0;

                return (int)Math.Round((res1.MABKR ?? 0) / denominator);
            }
            catch
            {
                return 0;
            }
        }
        public class RasPayDto
        {
            public string HES { get; set; }
            public double BED { get; set; }
            public double BES { get; set; }
            public string N_SERI { get; set; }
            public int BANK { get; set; }
            public long NUMBER { get; set; }
            public int TAG { get; set; }
            public long ID { get; set; }
            public long DATE_S { get; set; }
            public double N_S { get; set; }
            public long? PDT { get; set; }
        }

        public class RasFrDto
        {
            public double? MABKR { get; set; }
            public double? MAB { get; set; }
        }

        public static string CODESAL(string cody)
        {
            byte[] RawCoded = Encoding.GetEncoding(1256).GetBytes(cody);// ی 237

            var Parsy = Encoding.GetEncoding(1256);
            for (byte i = 0; i < RawCoded.Count(); i++)
            {
                RawCoded[i] = (byte)(RawCoded[i] - 20);
            }
            var result = Parsy.GetString(RawCoded);
            cody = result;
            return cody;
        }
        public static string DECODEUN(string cody)
        {
            if (string.IsNullOrEmpty(cody))
            {
                return cody;
            }

            byte[] RawCoded = Encoding.GetEncoding(1256).GetBytes(cody);// ی 237

            var Parsy = Encoding.GetEncoding(1256);
            for (byte i = 0; i < RawCoded.Count(); i++)
            {
                RawCoded[i] = (byte)(RawCoded[i] + 20);
            }
            var result = Parsy.GetString(RawCoded);
            cody = result;
            return cody;
        }
        public static string DECODEPS(string cody)
        {
            byte[] RawCoded = Encoding.GetEncoding(1256).GetBytes(cody);// ی 237
            var Parsy = Encoding.GetEncoding(1256);
            for (byte i = 0; i < RawCoded.Count(); i++)
            {
                RawCoded[i] = (byte)(RawCoded[i] + 10);
            }

            var result = Parsy.GetString(RawCoded);
            result = result.Substring(3, result.Length - 6);
            cody = result;
            return cody;
        }
        public static double GETFROOSH(long DT1, long DT2)
        {
            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").FirstOrDefault();
            double returnValue = 0;
            string MAHLIST;
            long XXD = 0;
            long XXM = 0;
            long XXS;
            long MH;
            long mah;
            long SAL = 0;
            string sh;
            long I;
            ////  BGIDD = 1
            ////XXD = UDayOfBGDATEofBGID;
            //if (XXM < 7)
            //{
            //    XXD = System.Convert.ToInt64(31 - XXD);
            //}
            //else
            //{
            //    XXD = System.Convert.ToInt64(30 - XXD);
            //}
            ////XXM = UMonth(RST.Fields("BGCDATE")) - 1;
            //foreach (var item in quer_RST)
            //{
            //    XXM = Convert.ToInt64(UMonth(item.BGCDATE.ToString())) - 1;
            //}
            ////mah = System.Convert.ToInt64(RST.Fields("BGMAH"));
            //mah = UMonthOfBGDATEofBGID;
            ////SAL = System.Convert.ToInt64(RST.Fields("BGSAL"));
            //SAL = UYearOfBGDATEofBGID;
            ////XXS = System.Convert.ToInt64(UYear(RST.Fields("BGCDATE")));
            //XXS = UYearOfBGDATEofBGID;
            //long DT1 = 0;
            //long DT2 = 0;
            //DT1 = Convert.ToInt64(Convert.ToDouble(SAL * 10000) + Convert.ToDouble(XXM * 100));
            //DT2 = Convert.ToInt64(Convert.ToDouble(Convert.ToInt32(SAL * 10000) + Convert.ToDouble(XXM * 100)) + 31);
            //Test should Delete
            //var q = dbms.Database.SqlQuery<string>("name");

            //BUG 7 #Matter Check


            var quer_RST2 = dbms.DoGetDataSQL<RST2_Data>("SELECT  " +
                " SUM(dbo.DEED_DTL.BES - dbo.DEED_DTL.BED) AS FR FROM  " +
                "   dbo.DEED_DTL INNER JOIN   dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S WHERE " +
                "    (dbo.DEED_HED.DATE_S BETWEEN "
                + Convert.ToString(DT1) + " AND " + Convert.ToString(DT2) + ")" +
                " AND (dbo.DEED_DTL.HES_K = " + Baseknow.FROSH.ToString() +
                " OR dbo.DEED_DTL.HES_K = " + Baseknow.MFROSH.ToString() + ")").ToList();

            foreach (var item2 in quer_RST2)
            {
                if (item2.FR == null || item2.FR < 1)
                {
                    item2.FR = 0;
                }
            }
            //RST2.Open("SELECT     SUM(dbo.DEED_DTL.BES - dbo.DEED_DTL.BED) AS FR FROM     dbo.DEED_DTL INNER JOIN   dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S WHERE     (dbo.DEED_HED.DATE_S BETWEEN " + Convert.ToString(DT1) + " AND " + Convert.ToString(DT2) + ") AND (dbo.DEED_DTL.HES_K = " + @Forms["baseknow"]["FROSH"] + " OR dbo.DEED_DTL.HES_K = " + @Forms["baseknow"]["MFROSH"] + ")", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
            double Filed_FR = -1;
            foreach (var item in quer_RST2)
            {
                Filed_FR = Convert.ToDouble(item.FR);
            }
            if (quer_RST2.Count() > 0 && Filed_FR != -1)
            {
                returnValue = Filed_FR;
            }
            else
            {
                returnValue = 0;
            }
            return returnValue;
        }
        public static void NTDZBMCHDABD050(long BGIDD)
        {
            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "");
            string MAHLIST;
            long XXD = 0;
            long XXM = 0;
            long XXS = 0;
            long XXI = 0;
            long MH;
            long mah = 0;
            long SAL = 0;
            string SH;
            long I = 0;
            long TEDAD;
            string rptname;
            //  BGIDD = 1
            foreach (var item in quer_RST)
            {
                XXD = Convert.ToInt64(UDay(item.BGCDATE.ToString()));
                PublicVRB.UDayOfBGDATEofBGID = Convert.ToInt64(UDay(item.BGCDATE.ToString()));
            }
            if (XXM < 7)
            {
                XXD = Convert.ToInt64(31 - XXD);
            }
            else
            {
                XXD = Convert.ToInt64(30 - XXD);
            }
            double bgcach = 0;
            foreach (var item in quer_RST)
            {
                XXM = Convert.ToInt64(UMonth(item.BGCDATE.ToString()));
                //UMonthOfBGDATEofBGID = Convert.ToInt64(UMonth(item.BGCDATE.ToString()));LAST Change 2021 Most Active if need
                PublicVRB.UMonthOfBGDATEofBGID = item.BGMAH;
                mah = Convert.ToInt64(item.BGMAH.ToString());
                SAL = Convert.ToInt64(item.BGSAL.ToString());
                PublicVRB.UYearOfBGDATEofBGID = Convert.ToInt64(item.BGSAL.ToString());
                XXS = Convert.ToInt64(UYear(item.BGCDATE.ToString()));
                XXI = Convert.ToInt32(SAL - XXS) * 12 + mah - XXM;
                bgcach = item.BGPCASH;
            }
            MH = XXM;

            //Budget2 bdgt2 = new Budget2(); //#Check

            //TextBox TextBobjA;
            //var quer_RSTup1 = dbms.Database.ExecuteSqlCommand("UPDATE BUGET_MAIN SET BGMBCHEKJM"+Val+" = " + bdgt2.BGFOCN1MON);
            //TextBox TextBobjB = (TextBox)bdgt2.FindName("BGFOCN" + I);
            for (I = 0; I <= (XXI - 2); I++)
            {
                TextBox TextBobjB = null;
                TextBox TextBobjB_1 = null;

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(BUDGET2))
                    {
                        TextBobjB = (TextBox)(window as BUDGET2).FindName("BGFOCN" + (XXI - I) + "MON");
                        TextBobjB_1 = (TextBox)(window as BUDGET2).FindName("BGFOCN" + (XXI - I - 1) + "MON");
                    }
                }

                double BGPCACHV;
                double BGPCACHV_1;
                double bgpcachf;
                BGPCACHV = Convert.ToDouble(TextBobjB.Text);
                BGPCACHV_1 = Convert.ToDouble(TextBobjB_1.Text);
                bgpcachf = (BGPCACHV - bgcach / 100 * BGPCACHV) * 0.5 + (BGPCACHV_1 - bgcach / 100 * BGPCACHV_1) * 0.5;

                var quer_RSTup1 = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGMBCHEKJM" + I + " = " + bgpcachf + " WHERE BGID = " + PublicVRB.GenetalBGID);
            }
            double FR0 = 0;


            //FR0 = System.Convert.ToDouble(GETFROOSH(BGDATE_OF_BGID));
            //FR0 = System.Convert.ToDouble(GETFROOSH(SAL * 10000 + XXM * 100 + UDay(RST.Fields("BGCDATE")), SAL * 10000 + XXM * 100 + 31)));
            FR0 = System.Convert.ToDouble(CL_HESABDARI.GETFROOSH(SAL * 10000 + XXM * 100 + Convert.ToInt64(UDay(quer_RST.Select(x => x.BGCDATE).FirstOrDefault().ToString())), SAL * 10000 + XXM * 100 + 31));



            TextBox BGFOCN_textb = null;
            TextBox BGFOCXDAY_textb = null;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(BUDGET2))
                {
                    string test = (window as BUDGET2).BGFOCXDAY.Text;
                    BGFOCN_textb = (TextBox)(window as BUDGET2).FindName("BGFOCN" + (XXI - I) + "MON");
                    BGFOCXDAY_textb = (TextBox)(window as BUDGET2).BGFOCXDAY;
                }
            }

            double BGFOCN_ME = Convert.ToDouble(BGFOCN_textb.Text);

            double BGFOCXDAY_ME = Convert.ToDouble(BGFOCXDAY_textb.Text);

            /* double BGPCASH = 0;*///////////--------------------------------------------------------------------------------------------------------
            foreach (var item in quer_RST)
            {
                bgcach = item.BGPCASH;
            }

            double ForQre1 = Convert.ToDouble(((BGFOCN_ME) - (bgcach) / 100 * (BGFOCN_ME)) * 0.5 + ((BGFOCXDAY_ME) - (bgcach) / 100 * (BGFOCXDAY_ME)) * 0.5);
            double ForQre2 = Convert.ToDouble((((BGFOCXDAY_ME) - (bgcach) / 100 * (BGFOCXDAY_ME)) * 0.5) + (FR0 - bgcach / 100 * FR0) * 0.5);
            //ForQre2 = Math.Round(ForQre2); //Change in Thursday, July 22, 2021
            string BGFO = "";
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(BUDGET2))
                {
                    BGFO = (window as BUDGET2).BGFOCXDAY.Text;
                }
            }
            var quer_sv2_BXDAY = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGFOCXDAY = " + BGFO + "  WHERE BGID = " + PublicVRB.GenetalBGID);//#Check
                                                                                                                                            //ذخیره نام هایی که در دیتابیس و فرم اسمشون یکیه این دستور برای اینه که در اکسس رکورد سورس اتوماتیک هست که سیو میکنه ولی سی شارپ باید اینو بنویسم تا سیو بشه

            var quer_RSTupdt = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGMBCHEKJM" + I + " = (" + ForQre1 + ") WHERE BGID = " + PublicVRB.GenetalBGID);
            I++;// i = i + 1
            var quer_RSTupdt_2 = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET  BGMBCHEKJM" + System.Convert.ToString(I) + " = (" + ForQre2 + ") WHERE BGID = " + PublicVRB.GenetalBGID);
        }
        public static double GETBSCHEK(long SAL, long mah)
        {
            //ADODB.Recordset RST = new ADODB.Recordset();
            long DT1 = 0;
            long DT2 = 0;
            DT1 = System.Convert.ToInt64(System.Convert.ToDouble(SAL * 10000) + System.Convert.ToDouble(mah * 100));
            DT2 = System.Convert.ToInt64(System.Convert.ToDouble(System.Convert.ToInt32(SAL * 10000) + System.Convert.ToDouble(mah * 100)) + 31);

            // RST.Open("SELECT     SUM(MABL) AS JCH FROM         dbo.PAY_GETD WHERE     (N_KOL IS NULL OR    N_KOL = " + basknow.BANKHA.ToString() + ") AND (N_KOL2 IS NULL) AND (N_KOL3 IS NULL)  AND (DATE_S BETWEEN " + System.Convert.ToString(DT1) + " AND " + System.Convert.ToString(DT2) + ")");
            // last point 6
            var quer_RST = dbms.DoGetDataSQL<RST_PAY>("SELECT     SUM(MABL) AS JCH FROM         dbo.PAY_GETD WHERE     (N_KOL IS NULL OR    N_KOL = " + Baseknow.BANKHA.ToString() + ") AND (N_KOL2 IS NULL) AND (N_KOL3 IS NULL)  AND (DATE_S BETWEEN " + System.Convert.ToString(DT1) + " AND " + System.Convert.ToString(DT2) + ")").FirstOrDefault();

            double Field_JCH = 0;
            if (quer_RST.JCH > 0)
            {
                Field_JCH = Convert.ToDouble(quer_RST.JCH);
                return Field_JCH;
            }
            else
            {
                //foreach (var item in quer_RST)
                //{
                //    Field_JCH = Convert.ToDouble(item.JCH);
                //}
                Field_JCH = 0;
                return Field_JCH;
            }
        }
        public static double GETCHEKin(long DT1, long DT2, object dt11, long dt22)
        {
            var rst = dbms.DoGetDataSQL<RST_PAY>("SELECT     SUM(MABL) AS JCH FROM         dbo.PAY_GETD WHERE(DATE_S BETWEEN " + System.Convert.ToString(DT1) + " AND " + System.Convert.ToString(DT2) + ") and(DATE BETWEEN " + System.Convert.ToString(dt11) + " AND " + System.Convert.ToString(dt22) + ") and(N_KOL <> 911 or N_KOL IS  NULL)").FirstOrDefault();
            if (rst.JCH != null && rst.JCH > 0)
            {
                return System.Convert.ToDouble(rst.JCH);

            }
            else
            {
                return 0;
            }
        }
        public static Int64 GetLIDD(string MYTAB, string FLD)
        {
            //#Cool IF Var Isnull in query result
            var quer_GETLIDD = dbms.DoGetDataSQL<long?>("SELECT MAX(" + FLD + ") AS IDL FROM " + MYTAB + "").First();
            if (quer_GETLIDD == null || quer_GETLIDD == 0)
            {
                return 1;
            }
            else
            {
                return Convert.ToInt64(quer_GETLIDD + 1);
            }
        }
        //public static object GETTAF3(string SHES, ref double KOL, ref double MOIN, ref double taf, ref double TAF2, ref double taf3, ref double taf4)
        public static object GETTAF3(string SHES, ref double? KOL, ref double? MOIN, ref double? taf, ref double? TAF2, ref double? taf3, ref double? taf4)
        {
            byte i1, I2, I3, I4, I5, I6, I7, I8, j, K;
            i1 = 0;
            I2 = 0;
            I3 = 0;
            I4 = 0;
            I5 = 0;
            I6 = 0;
            I7 = 0;
            I8 = 0;
            j = 1;
            K = 1;
            if (Strings.Len(SHES) > 4)
            {
                var loopTo = (byte)Strings.Len(SHES);
                for (j = 1; j <= loopTo; j++)
                {
                    if (Strings.Mid(SHES, j, 1) == "-")
                    {
                        switch (K)
                        {
                            case 1:
                                {
                                    i1 = j;
                                    break;
                                }

                            case 2:
                                {
                                    I2 = j;
                                    break;
                                }

                            case 3:
                                {
                                    I3 = (byte)(j + 1);
                                    break;
                                }

                            case 4:
                                {
                                    I4 = (byte)(j + 1);
                                    break;
                                }

                            case 5:
                                {
                                    I5 = (byte)(j + 1);
                                    break;
                                }

                            case 6:
                                {
                                    I6 = (byte)(j + 1);
                                    break;
                                }

                            case 7:
                                {
                                    I7 = (byte)(j + 1);
                                    break;
                                }

                            case 8:
                                {
                                    I8 = (byte)(j + 1);
                                    break;
                                }
                        }

                        K = (byte)(K + 1);
                    }
                }
            }

            KOL = default;
            MOIN = default;
            taf = default;
            TAF2 = default;
            taf3 = default;
            taf4 = default;
            if (i1 == 0)
            {
                if (CL_LMethods.IsNumeric(SHES))
                {
                    KOL = Convert.ToDouble(SHES);
                }
                return default;
            }
            else
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, 1, i1 - 1)))
                {
                    KOL = Convert.ToDouble(Strings.Mid(SHES, 1, i1 - 1));
                }
            }

            if (I2 == 0)
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, i1 + 1, Strings.Len(SHES) - i1)))
                {
                    MOIN = Convert.ToDouble(Strings.Mid(SHES, i1 + 1, Strings.Len(SHES) - i1));
                }
                return default;
            }
            else
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, i1 + 1, I2 - i1 - 1)))
                {
                    MOIN = Convert.ToDouble(Strings.Mid(SHES, i1 + 1, I2 - i1 - 1));
                }
            }

            if (I3 == 0)
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I2 + 1, Strings.Len(SHES) - I2)))
                {
                    taf = Convert.ToDouble(Strings.Mid(SHES, I2 + 1, Strings.Len(SHES) - I2));
                }
                return default;
            }
            else
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I2 + 1, I3 - I2 - 2)))
                {
                    taf = Convert.ToDouble(Strings.Mid(SHES, I2 + 1, I3 - I2 - 2));
                }
            }

            if (I4 == 0)
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I3, Strings.Len(SHES) - I3 + 1)))
                {
                    TAF2 = Convert.ToDouble(Strings.Mid(SHES, I3, Strings.Len(SHES) - I3 + 1));
                }

            }
            else
            {
                if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I3, I4 - I3 - 1)))
                {
                    TAF2 = Convert.ToDouble(Strings.Mid(SHES, I3, I4 - I3 - 1));
                }
                if (I5 == 0)
                {
                    if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I4, Strings.Len(SHES) - I4 + 1)))
                    {
                        taf3 = Convert.ToDouble(Strings.Mid(SHES, I4, Strings.Len(SHES) - I4 + 1));
                    }
                }
                else
                {
                    if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I4, I5 - I4 - 1)))
                    {
                        taf3 = Convert.ToDouble(Strings.Mid(SHES, I4, I5 - I4 - 1));
                    }
                    if (I6 == 0)
                    {
                        if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I5, Strings.Len(SHES) - I5 + 1)))
                        {
                            taf4 = Convert.ToDouble(Strings.Mid(SHES, I5, Strings.Len(SHES) - I5 + 1));
                        }
                    }
                    else
                    {
                        if (CL_LMethods.IsNumeric(Strings.Mid(SHES, I5, I6 - I5 - 1)))
                        {
                            taf4 = Convert.ToDouble(Strings.Mid(SHES, I5, I6 - I5 - 1));
                        }
                    }
                }
            }

            return default;
        }

        public static bool BLOCKEDMK(string HES)
        {
            if (string.IsNullOrEmpty(HES) || string.IsNullOrWhiteSpace(HES))
            {
                return false;
            }

            double? TKOL = null, TMOIN = null, TTAF = null, TTAF2 = null, TTAF3 = null, TTAF4 = null;
            double? HKOL = null, HMOIN = null, HTAF = null, HTAF2 = null, HTAF3 = null, HTAF4 = null;

            var rst3 = dbms.DoGetDataSQL<CHECKBLOCKCUST>("SELECT USERCO,HES FROM BLOCKNON_HES WHERE USERCO = " + Baseknow.USERCOD).ToList();
            if (rst3.Count > 0)
            {
                foreach (var item in rst3)
                {
                    // اشکال اینجا بود - باید item.HES استفاده شود نه FirstOrDefault
                    GETTAF3(item.HES, ref TKOL, ref TMOIN, ref TTAF, ref TTAF2, ref TTAF3, ref TTAF4);
                    GETTAF3(HES, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);

                    if (TKOL == HKOL)
                    {
                        if (TKOL == HKOL && TMOIN == null)
                        {
                            return false;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == null)
                        {
                            return false;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == null)
                        {
                            return false;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == null)
                        {
                            return false;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == HTAF3 && TTAF4 == null)
                        {
                            return false;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == HTAF3 && TTAF4 == HTAF4)
                        {
                            return false;
                        }
                    }
                }
            }

            var RST2 = dbms.DoGetDataSQL<CHECKBLOCKCUST>("SELECT USERCO,HES FROM BLOCK_HES WHERE USERCO = " + Baseknow.USERCOD).ToList();
            if (RST2.Count > 0)
            {
                foreach (var item in RST2)
                {
                    // اشکال اینجا بود - باید item.HES استفاده شود نه FirstOrDefault
                    GETTAF3(item.HES, ref TKOL, ref TMOIN, ref TTAF, ref TTAF2, ref TTAF3, ref TTAF4);
                    GETTAF3(HES, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);

                    if (TKOL == HKOL)
                    {
                        if (TKOL == HKOL && TMOIN == null)
                        {
                            return true;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == null)
                        {
                            return true;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == null)
                        {
                            return true;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == null)
                        {
                            return true;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == HTAF3 && TTAF4 == null)
                        {
                            return true;
                        }
                        else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF && TTAF2 == HTAF2 && TTAF3 == HTAF3 && TTAF4 == HTAF4)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public static string WANTEDFORM(string frm)
        {
            string WANTEDFORMRet = default;

            // Cleaned switch statement: Removed duplicates and 'when' clauses
            switch (frm ?? "")
            {
                // منوي تعاريف--------------------------------------------------------------
                case "BANKS": WANTEDFORMRet = "TCOD_BANKS"; break;
                case "ANBAR": WANTEDFORMRet = "TCOD_ANBAR"; break;
                case "KALA": WANTEDFORMRet = "STUF_DEF"; break;
                case "GROUP": WANTEDFORMRet = "TCOD_STUFGROUP"; break;
                case "VAHED": WANTEDFORMRet = "TCOD_VAHEDS"; break;
                case "ANBARIN": WANTEDFORMRet = "TCOD_ANBARINI"; break;
                case "HESDEF": WANTEDFORMRet = "TOTA_HES"; break;
                case "AUTO": WANTEDFORMRet = "AUTOMATIC"; break;
                case "MOSHTARI": WANTEDFORMRet = "FCODE_CUSTOMER"; break;
                case "GHES": WANTEDFORMRet = "TCOD_HESGROUP"; break;
                case "CUST": WANTEDFORMRet = "CUSTKIND"; break;
                case "DEPART": WANTEDFORMRet = "DEPART"; break;
                case "SHIFT": WANTEDFORMRet = "SHIFT"; break;
                case "KHAD": WANTEDFORMRet = "KHAD_DEF"; break;
                case "AZAE": WANTEDFORMRet = "AZAE"; break;
                case "TMHAZ": WANTEDFORMRet = "TMHAZ"; break;
                case "MENUIT": WANTEDFORMRet = "MENUIT"; break;
                case "TAKHDEF": WANTEDFORMRet = "TAKHDEF"; break;
                case "CUSTDEF": WANTEDFORMRet = "CUSTDEF"; break;
                case "TKHESL": WANTEDFORMRet = "TKHESL"; break;
                case "AZADPAY": WANTEDFORMRet = "AZADPAY"; break;

                case "JOST1": WANTEDFORMRet = "JOST1"; break;
                case "JOST2": WANTEDFORMRet = "JOST2"; break;
                case "JOST3": WANTEDFORMRet = "JOST3"; break;
                case "GRFORMAT": WANTEDFORMRet = "GRFORMAT"; break;
                case "GSCALE": WANTEDFORMRet = "GSCALE"; break;
                case "GGRAD": WANTEDFORMRet = "GGRAD"; break;
                case "GGRSH": WANTEDFORMRet = "GGRSH"; break;
                case "okpish": WANTEDFORMRet = "okpish"; break;
                case "PRGRP": WANTEDFORMRet = "PRGRP"; break;
                case "PRPAYNO": WANTEDFORMRet = "PRPAYNO"; break;
                case "PRELMTF": WANTEDFORMRet = "PRELMTF"; break;
                case "PRELMPR": WANTEDFORMRet = "PRELMPR"; break;
                case "TEL": WANTEDFORMRet = "TEL"; break;
                case "BGU": WANTEDFORMRet = "BGU"; break;
                case "BGLIST": WANTEDFORMRet = "BGLIST"; break;
                case "BGLISTTL": WANTEDFORMRet = "BGLISTTL"; break;
                case "moreinfo": WANTEDFORMRet = "moreinfo"; break;

                // منوي حسابداري-----------------------------------------------------------
                case "SANAD": WANTEDFORMRet = "DEED_HEAD"; break;
                case "PGETD": WANTEDFORMRet = "PGET_HED"; break;
                case "OFDP": WANTEDFORMRet = "OPGET_HED"; break;
                case "OFDPSN": WANTEDFORMRet = "ENTEGHAL_DARYAFT_PARDAKHT"; break;
                case "TAEED": WANTEDFORMRet = "F_MENU_ASNAD"; break;
                case "EFTE": WANTEDFORMRet = "SANAD_EFTETAHIYAH"; break;
                case "EKHTE": WANTEDFORMRet = "SANAD_EKHTETAMIYAH"; break;
                case "RDFTMOIN": WANTEDFORMRet = "R_DAFTAR_MOIN"; break;
                case "HTAF": WANTEDFORMRet = "R_DAFTAR_TAFZILY"; break;
                case "RDFTTFKMOIN": WANTEDFORMRet = "F_MENU_DATE_KOL_MOIN_TAFKIK"; break;
                case "DFTROOS": WANTEDFORMRet = "DAFTAR_ROOZNAMEH"; break;
                case "DFTKOL": WANTEDFORMRet = "R_DAFTAR_KOL"; break;
                case "DPDAY": WANTEDFORMRet = "R_DP_DAYLY"; break;
                case "NEWYEAR": WANTEDFORMRet = "F_NEWYEAR"; break;
                case "SELYEAR": WANTEDFORMRet = "F_SELECTYEAR"; break;
                case "DPDAYASH": WANTEDFORMRet = "R_DP_DAYLY_ASHKHAS"; break;
                case "TARAZ4": WANTEDFORMRet = "TARAZ_4"; break;
                case "RTARAZ4": WANTEDFORMRet = "R_TARAZ_4"; break;
                case "TARAZ4M": WANTEDFORMRet = "TARAZ_4_MOIN"; break;
                case "TARAZ4M2": WANTEDFORMRet = "TARAZ4_MOIN2"; break;
                case "TARAZ4T": WANTEDFORMRet = "TARAZ_4_TAFZ"; break;
                case "RTARAZ4T": WANTEDFORMRet = "TARAZ4_TAFZIL"; break;
                case "RTARAZ4T2": WANTEDFORMRet = "TARAZ4_TAFZIL2"; break;
                case "OTHER": WANTEDFORMRet = "OTHER"; break;
                case "SNDHES": WANTEDFORMRet = "DEED_SEARCH"; break;
                case "BEDBES": WANTEDFORMRet = "BEDEHKARAN_BESTANKARAN"; break;
                case "GETS": WANTEDFORMRet = "PGET_LST_SEARCH"; break;
                case "ASNDSO": WANTEDFORMRet = "SORTASNAD"; break;
                case "SUDZIAN": WANTEDFORMRet = "SOUDVAZIAN"; break;
                case "SMOGH":
                case "SMOGHFR": WANTEDFORMRet = "F_MENU_SANAD_MOGHA"; break;
                case "TKHARED": WANTEDFORMRet = "HEAD_LST_KHAREED_SND"; break;
                case "TFRSND": WANTEDFORMRet = "HEAD_LST_FROOSH_SND"; break;
                case "SNDPR": WANTEDFORMRet = "F_MENU_ASNAD_PRINT"; break;
                case "SPRIN": WANTEDFORMRet = "F_MENU_PRINTHES"; break;
                case "MOGH": WANTEDFORMRet = "ADAMTARAZ"; break;
                case "SKOL": WANTEDFORMRet = "TOTA_HES_SHEET"; break;
                case "FKHAD": WANTEDFORMRet = "HEAD_LST_KHADAMAT"; break;
                case "MOS": WANTEDFORMRet = "HEAD_LST_MOSTAGHIM"; break;
                case "MOGHA": WANTEDFORMRet = "F_MENU_MOGHAYERAT"; break;
                case "SMOGHA": WANTEDFORMRet = "MOGHAYERAT"; break;
                case "OSND": WANTEDFORMRet = "ODEED_HEAD"; break;
                case "OSNDSER": WANTEDFORMRet = "ENTEGHAL_ASNAD_HESABDARY"; break;
                case "GRFRO": WANTEDFORMRet = "GENSANADFROOSH"; break;
                case "GRKHO": WANTEDFORMRet = "GENSANADFROOSHGRKHO"; break;
                case "GRKHS": WANTEDFORMRet = "GENSANADFROOSHGRKHS"; break;
                case "GRVRO": WANTEDFORMRet = "GENSANADFROOSHGRVRO"; break;
                case "GRENT": WANTEDFORMRet = "GENSANADFROOSHGRENT"; break;
                case "GRKHA": WANTEDFORMRet = "GENSANADFROOSHGRKHA"; break;
                case "GRKHR": WANTEDFORMRet = "GRKHR"; break;
                case "GRKHZ": WANTEDFORMRet = "GRKHZ"; break;
                case "GRVDS": WANTEDFORMRet = "GRVDS"; break;
                case "GRANB": WANTEDFORMRet = "GRANB"; break;
                case "GENFRBR": WANTEDFORMRet = "GENFRBR"; break;
                case "BAZAV": WANTEDFORMRet = "BAZ_AVRAGE"; break;
                case "SKHOA": WANTEDFORMRet = "SNADKHOLASAHAMALIAT"; break;
                case "RMOIN": WANTEDFORMRet = "R_DAFTAR_MOIN_LIST"; break;
                case "TNAM": WANTEDFORMRet = "TARAZNAMAH2"; break;
                case "TNAMH": WANTEDFORMRet = "TARAZHES"; break;
                case "SERP": WANTEDFORMRet = "DEED_SEARCH_MAIN"; break;
                case "DMSGH": WANTEDFORMRet = "DMSGH"; break;
                case "SUDP": WANTEDFORMRet = "SUDP"; break;
                case "HBGHB": WANTEDFORMRet = "HBGHB"; break;
                case "PFKEY": WANTEDFORMRet = "PFKEY"; break;
                case "PFHKEY": WANTEDFORMRet = "PFHKEY"; break;
                case "MARKAZ": WANTEDFORMRet = "MARKAZ"; break;
                case "BEDBESM": WANTEDFORMRet = "BEDBESM"; break;
                case "DKOLKH": WANTEDFORMRet = "DKOLKH"; break;
                case "HESMAI": WANTEDFORMRet = "F_MENU_KOL_MOIN_DATE_AI"; break;
                case "MANABE": WANTEDFORMRet = "MANABE"; break;
                case "FACTFRMO": WANTEDFORMRet = "FACTFRMO"; break;
                case "FRSKB": WANTEDFORMRet = "FRSKB"; break;
                case "KHSKB": WANTEDFORMRet = "KHSKB"; break;
                case "FONAR": WANTEDFORMRet = "FONAR"; break;
                case "FONARP": WANTEDFORMRet = "FONARP"; break;
                case "VISIT_POR": WANTEDFORMRet = "VISIT_POR"; break;
                case "moj": WANTEDFORMRet = "moj"; break;
                case "LVBF": WANTEDFORMRet = "LVBF"; break;
                case "DRIVERH": WANTEDFORMRet = "DRIVERH"; break;
                case "KHLS": WANTEDFORMRet = "KHLS"; break;
                case "DPSEE": WANTEDFORMRet = "DPSEE"; break;
                case "DECD": WANTEDFORMRet = "DECD"; break;
                case "DPDEED": WANTEDFORMRet = "DPDEED"; break;
                case "PAYORD": WANTEDFORMRet = "PAYORD"; break;
                case "PAYORDL": WANTEDFORMRet = "PAYORDL"; break;
                case "SMS": WANTEDFORMRet = "SMS"; break;
                case "SUD": WANTEDFORMRet = "SUD"; break;
                case "NABZMOSH": WANTEDFORMRet = "NABZMOSH"; break;
                case "NABZKAR": WANTEDFORMRet = "NABZKAR"; break;
                case "JAYGOZIN": WANTEDFORMRet = "JAYGOZIN"; break;
                case "KHAZANEH": WANTEDFORMRet = "KHAZANEH"; break;
                case "CUSANAD": WANTEDFORMRet = "CUSANAD"; break;

                // منوي توليد-----------------------------------------------------------
                case "TOLID": WANTEDFORMRet = "HEAD_MAN"; break;
                case "FORMOL": WANTEDFORMRet = "HEAD_MANF"; break;
                case "HAVT": WANTEDFORMRet = "HAVLAH_KHORUG"; break;
                case "HEXIT": WANTEDFORMRet = "HAVALAH_EXIT"; break;
                case "HENTER": WANTEDFORMRet = "HAVALAH_ENTER"; break;
                case "MARM": WANTEDFORMRet = "MASRAF_CENTER"; break;
                case "HSAYER": WANTEDFORMRet = "HAVALAH_EXIT_SAYER"; break;
                case "JAZBD": WANTEDFORMRet = "AONE_SALARY"; break;
                case "KARB": WANTEDFORMRet = "KAR_HEAD"; break;
                case "AMTOL": WANTEDFORMRet = "F_MENU_DATEAMTOL"; break;
                case "AMMAS": WANTEDFORMRet = "AMMAS"; break;
                case "MOFGH": WANTEDFORMRet = "SUDZIANGHEMAT"; break;
                case "AMA2": WANTEDFORMRet = "SANADPAYANDORAH"; break;

                // منوي خريد فروش-----------------------------------------------------------
                case "VISITOR": WANTEDFORMRet = "VISITOR"; break;
                case "CROSS": WANTEDFORMRet = "GOZARESH_FROOSH_MAHSUL"; break;
                case "MFACTFR": WANTEDFORMRet = "MHEAD_LST_FROOSH"; break;
                case "CFRALL": WANTEDFORMRet = "FROOSH_COUNTALL"; break;
                case "FACTFRSA": WANTEDFORMRet = "FACTFRSA"; break;
                case "FACTFR": WANTEDFORMRet = "HEAD_LST_FROOSH"; break;
                case "FACTKH": WANTEDFORMRet = "HEAD_LST_KHAREED"; break;
                case "FACTKHSA": WANTEDFORMRet = "FACTKHSA"; break;
                case "PFACTFR": WANTEDFORMRet = "HEAD_LST_PISHFROOSH"; break;
                case "FACTBFR": WANTEDFORMRet = "HEAD_LST_FROOSH_BACK"; break;
                case "FACTBKH": WANTEDFORMRet = "HEAD_LST_KH_BACK"; break;
                case "SEFARESH": WANTEDFORMRet = "ORDR_HED"; break;
                case "FROOSHDAY": WANTEDFORMRet = "R_FROOSH_DAYLY"; break;
                case "FROOSHDAY2": WANTEDFORMRet = "R_FROOSH_DAYLY2"; break;
                case "FKHAREDAY": WANTEDFORMRet = "R_KHARED_DAYLY"; break;
                case "GARDESH": WANTEDFORMRet = "KALAS"; break;
                case "STUFLIST": WANTEDFORMRet = "STUF_DEF_LIST"; break;
                case "FLIST": WANTEDFORMRet = "LIST_INVOICE_FROOSH"; break;
                case "KLIST": WANTEDFORMRet = "LIST_INVOICE_KHARED"; break;
                case "LFRAN": WANTEDFORMRet = "LIST_FROOSH_ANBARS"; break;
                case "LKHAN": WANTEDFORMRet = "LIST_KHAREED_ANBARS"; break;
                case "LPF": WANTEDFORMRet = "LASTPRICE"; break;
                case "VAZ": WANTEDFORMRet = "R_GARDESH_KHFR_DAFTAR"; break;
                case "FRROOZU": WANTEDFORMRet = "GOZARESH_FROOSH_USER"; break;
                case "LFACT": WANTEDFORMRet = "Q_LIST_DALY"; break;
                case "FGROO": WANTEDFORMRet = "F_MENU_FROOSH"; break;
                case "RAAS": WANTEDFORMRet = "RAAS"; break;
                case "OFACT": WANTEDFORMRet = "OHEAD_LST_FROOSH"; break;
                case "OFACTKH": WANTEDFORMRet = "OHEAD_LST_KHADAMAT"; break;
                case "CRREPF": WANTEDFORMRet = "HEAD_SERCH_MAIN"; break;
                case "AMFDAY": WANTEDFORMRet = "AMFDAY"; break;
                case "FPDESIGN": WANTEDFORMRet = "FPFORMAT"; break;
                case "RORD": WANTEDFORMRet = "RORD"; break;
                case "BFAC": WANTEDFORMRet = "BFAC"; break;
                case "ESLAH": WANTEDFORMRet = "ESLAH"; break;
                case "FRCUST": WANTEDFORMRet = "FRCUST"; break;
                case "TFTMLOCK": WANTEDFORMRet = "TFTMLOCK"; break;
                case "CRREPFA": WANTEDFORMRet = "CRREPFA"; break;
                case "SAVEREP": WANTEDFORMRet = "SAVEREP"; break;
                case "DARKHR": WANTEDFORMRet = "DARKHR"; break;
                case "DARKHRLIS": WANTEDFORMRet = "DARKHRLIS"; break;
                case "BRFR": WANTEDFORMRet = "BRFR"; break;
                case "BARGI": WANTEDFORMRet = "BARGI"; break;
                case "RDFTKHFRA4": WANTEDFORMRet = "FRKMA4"; break;
                case "LFKBTF": WANTEDFORMRet = "LFKBTF"; break;
                case "LISTKHG": WANTEDFORMRet = "LISTKHG"; break;
                case "LISTFRG": WANTEDFORMRet = "LISTFRG"; break;
                case "VISITORS": WANTEDFORMRet = "VISITORS"; break;
                case "CHVBV": WANTEDFORMRet = "CHVBV"; break;
                case "FRRDTL": WANTEDFORMRet = "FRRDTL"; break;
                case "USERFR": WANTEDFORMRet = "USERFR"; break;
                case "KHCUST": WANTEDFORMRet = "KHCUST"; break;
                case "SMS_VIP": WANTEDFORMRet = "SMS_VIP"; break;
                case "VISITDLV": WANTEDFORMRet = "VISITDLV"; break;
                case "VISITKAL": WANTEDFORMRet = "VISITKAL"; break;
                case "VISITKALMA": WANTEDFORMRet = "VISITKALMA"; break;
                case "VISITROUT": WANTEDFORMRet = "VISITROUT"; break;
                case "VISITGOL": WANTEDFORMRet = "VISITGOL"; break;
                case "VISITGOLMON": WANTEDFORMRet = "VISITGOLMON"; break;
                case "VISITGOLMONR": WANTEDFORMRet = "VISITGOLMONR"; break;
                case "VISITGOLG": WANTEDFORMRet = "VISITGOLG"; break;
                case "VISITGOLMONG": WANTEDFORMRet = "VISITGOLMONG"; break;
                case "VISITGOLMONGR": WANTEDFORMRet = "VISITGOLMONGR"; break;
                case "VISIT_DAY": WANTEDFORMRet = "VISIT_DAY"; break;
                case "ESLAHRF": WANTEDFORMRet = "ESLAHRF"; break;
                case "ESLAHR": WANTEDFORMRet = "ESLAHR"; break;
                case "ESLAHH": WANTEDFORMRet = "ESLAHH"; break;
                case "ESLAHV": WANTEDFORMRet = "ESLAHV"; break;
                case "ESLAHE": WANTEDFORMRet = "ESLAHE"; break;
                case "ESLAHS": WANTEDFORMRet = "ESLAHS"; break;
                case "ESLAHM": WANTEDFORMRet = "ESLAHM"; break;
                case "ESLAHA": WANTEDFORMRet = "ESLAHA"; break;
                case "ESLAHK": WANTEDFORMRet = "ESLAHK"; break;
                case "VISIT_MAS": WANTEDFORMRet = "VISIT_MAS"; break;
                case "FRBKA": WANTEDFORMRet = "FRBKA"; break;
                case "CHAPM": WANTEDFORMRet = "CHAPM"; break;
                case "LVBP": WANTEDFORMRet = "LVBP"; break;
                case "CUSTNP": WANTEDFORMRet = "CUSTNP"; break;
                case "CUSTEN": WANTEDFORMRet = "CUSTEN"; break;
                case "FRMOST": WANTEDFORMRet = "FRMOST"; break;
                case "TKHPISH": WANTEDFORMRet = "TKHPISH"; break;
                case "ESLAHKHAD": WANTEDFORMRet = "ESLAHKHAD"; break;
                case "FASLIBR": WANTEDFORMRet = "FASLIBR"; break;
                case "CKRALL": WANTEDFORMRet = "CKRALL"; break;
                case "RESERV": WANTEDFORMRet = "RESERV"; break;
                case "VISITONE": WANTEDFORMRet = "VISITONE"; break;
                case "FASLIKHBR": WANTEDFORMRet = "FASLIKHBR"; break;
                case "KEYTPF": WANTEDFORMRet = "KEYTPF"; break;
                case "CRMMAIN": WANTEDFORMRet = "CRMMAIN"; break;
                case "ENHESAR": WANTEDFORMRet = "ENHESAR"; break;
                case "tozied": WANTEDFORMRet = "tozied"; break;
                case "NOTE": WANTEDFORMRet = "NOTE"; break;

                // منوي  چك---------------------------------------------------------
                case "CHEKD": WANTEDFORMRet = "PAY_GETD"; break;
                case "CHEKP": WANTEDFORMRet = "PAY_GETP"; break;
                case "VCHD": WANTEDFORMRet = "CHKREC_H"; break;
                case "VCHP": WANTEDFORMRet = "CHREC_HP"; break;
                case "CHKDLIST": WANTEDFORMRet = "CHKE_DLIST"; break;
                case "CHKPLIST": WANTEDFORMRet = "CHEK_PLIST"; break;
                case "SERIAL": WANTEDFORMRet = "F_MENU_SERILA"; break;
                case "TCHEK": WANTEDFORMRet = "COD_HESAB"; break;
                case "FORMAT": WANTEDFORMRet = "FORMAT"; break;
                case "CHPRN": WANTEDFORMRet = "HED_SODUR"; break;
                case "PCHS": WANTEDFORMRet = "CHEK_PLISTS"; break;
                case "DCHS": WANTEDFORMRet = "CHEK_DLISTS"; break;
                case "PCHSS": WANTEDFORMRet = "CHEK_PLISTS"; break;
                case "DCHSS": WANTEDFORMRet = "DCHSS"; break;
                case "CHKM": WANTEDFORMRet = "CHEK_MOGUD"; break;
                case "CHKB": WANTEDFORMRet = "CHEK_BARGASHTI_MAIN"; break;
                case "CHKV": WANTEDFORMRet = "CHEK_VOSUL_LES"; break;
                case "CHKVA": WANTEDFORMRet = "CHEK_VLISTALL"; break;
                case "CHKP": WANTEDFORMRet = "CHK_V_PRINT"; break;
                case "FVP": WANTEDFORMRet = "FORMAT_HESAB"; break;
                case "FPV": WANTEDFORMRet = "CHECK_PVLIST"; break;
                case "MONCH": WANTEDFORMRet = "CHEK_MON"; break;
                case "BEHESAB": WANTEDFORMRet = "CHKREC_HES"; break;
                case "GBEHESAB": WANTEDFORMRet = "CHRE_LSPH"; break;
                case "RCHEKD": WANTEDFORMRet = "F_MENU_CHEKRCHEKD"; break;
                case "RCHEKP": WANTEDFORMRet = "F_MENU_CHEKRCHEKP"; break;
                case "CHAPCHEK": WANTEDFORMRet = "CHAPCHEK"; break;
                case "CHKPSS": WANTEDFORMRet = "CHKPSS"; break;
                case "CHVBVP": WANTEDFORMRet = "CHVBVP"; break;
                case "MONCHP": WANTEDFORMRet = "MONCHP"; break;
                case "CHVZ": WANTEDFORMRet = "CHVZ"; break;

                // منوي انبار-----------------------------------------------------------
                case "ESWAP": WANTEDFORMRet = "HEAD_LST_ENTEGHAL"; break;
                case "MOGUKOL": WANTEDFORMRet = "R_MOGUDI_ANBARHA"; break;
                case "MOGUKOLLIST": WANTEDFORMRet = "MOGUDI_ANBARHA_LIST"; break;
                case "BAZANB": WANTEDFORMRet = "BAZ_ANBAR"; break;
                case "AKMOGO": WANTEDFORMRet = "AK_MOGUDI_ANBAR_LIST"; break;
                case "AKMOGOR": WANTEDFORMRet = "R_AK_MOGUDI_ANBAR"; break;
                case "AKMOGO2": WANTEDFORMRet = "AK_MOGUDI_ANBAR_LIST"; break;
                case "AKMOGOR2": WANTEDFORMRet = "R_AK_MOGUDI_ANBAR"; break;
                case "KARTR": WANTEDFORMRet = "R_KA_KALA"; break;
                case "KARTR2": WANTEDFORMRet = "KARTR2"; break;
                case "LSTRAZ": WANTEDFORMRet = "TARAZ_ANBAR_KOL"; break;
                case "RSTRAZ": WANTEDFORMRet = "R_TARAZ_ANBARHA"; break;
                case "LSTRAZA": WANTEDFORMRet = "TARAZ_ANBAR_KHAS"; break;
                case "RSTRAZA": WANTEDFORMRet = "R_TARAZ_ANBAR_KHAS"; break;
                case "RASID": WANTEDFORMRet = "HEAD_LST_RASID"; break;
                case "HAVL": WANTEDFORMRet = "HEAD_LST_HAVL"; break;
                case "ANGD": WANTEDFORMRet = "ANBGRD_HEAD"; break;
                case "RTARDTL": WANTEDFORMRet = "RTARDTL"; break;
                case "RBRFR": WANTEDFORMRet = "RBRFR"; break;
                case "ANBBAZ": WANTEDFORMRet = "ANBBAZ"; break;
                case "TANBGRP": WANTEDFORMRet = "TANBGRP"; break;
                case "HAG": WANTEDFORMRet = "HAG"; break;
                case "SEEENT": WANTEDFORMRet = "SEEENT"; break;
                case "SAYERHAV": WANTEDFORMRet = "SAYERHAV"; break;
                case "ANBMERG": WANTEDFORMRet = "ANBMERG"; break;
                case "TDBARG": WANTEDFORMRet = "TDBARG"; break;
                case "VBP": WANTEDFORMRet = "VBP_CHECK"; break;
                case "TICBARGILES": WANTEDFORMRet = "TICBARGILES"; break;
                case "SEARCHMO": WANTEDFORMRet = "SEARCHMO"; break;
                case "MOGDT": WANTEDFORMRet = "MOGDT"; break;

                // منوي حقوق-----------------------------------------------------------
                case "PERS": WANTEDFORMRet = "PERSON"; break;
                case "WORK": WANTEDFORMRet = "WORKHEAD"; break;
                case "WOKPER": WANTEDFORMRet = "WORKING"; break;
                case "LISTWOR": WANTEDFORMRet = "list_salary"; break;
                case "LISTWOR2": WANTEDFORMRet = "list_salary2"; break;
                case "LISTBIM": WANTEDFORMRet = "R_LIST_BIMEH"; break;
                case "LISTMAL": WANTEDFORMRet = "R_LIST_maliat"; break;
                case "FISH1": WANTEDFORMRet = "FISH_HOGHUGH"; break;
                case "FISH2": WANTEDFORMRet = "FISH2"; break;
                case "SALAR": WANTEDFORMRet = "R_LIST_salary"; break;
                case "DISK": WANTEDFORMRet = "ERSAL_SANAD"; break;
                case "HOKM": WANTEDFORMRet = "PHOKM"; break;
                case "MORAKH": WANTEDFORMRet = "PMORAKH"; break;
                case "GHARA": WANTEDFORMRet = "PGHARAR"; break;
                case "SAND": WANTEDFORMRet = "SAND"; break;
                case "VAM": WANTEDFORMRet = "VAM"; break;
                case "TASV": WANTEDFORMRet = "PENDJOB"; break;
                case "EP": WANTEDFORMRet = "EYDY_PADASH"; break;
                case "DISKM": WANTEDFORMRet = "DISKM"; break;

                // منوي برنامه ريزي-----------------------------------------------------------
                case "PROG": WANTEDFORMRet = "PRGHEAD"; break;
                case "CH1": WANTEDFORMRet = "CHRT_HES_KOL"; break;
                case "CT1": WANTEDFORMRet = "F_MENU_MON"; break;
                case "CT2": WANTEDFORMRet = "CAMAT"; break;
                case "CT3": WANTEDFORMRet = "CT3"; break;
                case "CT4":
                case "CT5": WANTEDFORMRet = "CT4"; break;

                // منوي هتلي-----------------------------------------------------------
                case "MEH": WANTEDFORMRet = "R_LIST_MEHMAN"; break;
                case "HOT": WANTEDFORMRet = "MEHMAN"; break;
                case "ROM": WANTEDFORMRet = "ROOM"; break;
                case "RACK": WANTEDFORMRet = "RACKROOM"; break;

                // منوي سيستم-----------------------------------------------------------
                case "ABOUT": WANTEDFORMRet = "ABOUT"; break;
                case "SYS": WANTEDFORMRet = "SAZMAN"; break;
                case "nuser": WANTEDFORMRet = "users"; break;
                case "NEWPASSWORD":
                case "npass": WANTEDFORMRet = "NEWPASSWORD"; break;
                case "nname": WANTEDFORMRet = "USER_CHANGE"; break;
                case "permi": WANTEDFORMRet = "F_USER_PERMITION FORMS"; break;
                case "ersal": WANTEDFORMRet = "F_ERSAL"; break;
                case "dar": WANTEDFORMRet = "F_DARYAFT"; break;
                case "KONT": WANTEDFORMRet = "FORM11"; break;
                case "DEFA": WANTEDFORMRet = "DEFAULT"; break;
                case "TR_FR": WANTEDFORMRet = "TR_FR"; break;
                case "TR_KH": WANTEDFORMRet = "TR_KH"; break;
                case "TR_KHB": WANTEDFORMRet = "TR_KHB"; break;
                case "TR_FRB": WANTEDFORMRet = "TR_FRB"; break;
                case "TR_PFRB": WANTEDFORMRet = "TR_PFRB"; break;
                case "TR_PGL": WANTEDFORMRet = "TR_PGL"; break;
                case "TR_KHAD": WANTEDFORMRet = "TR_KHAD"; break;
                case "TR_MAVA": WANTEDFORMRet = "TR_MAVA"; break;
                case "TR_SMAVA": WANTEDFORMRet = "TR_SMAVA"; break;
                case "TR_TOL": WANTEDFORMRet = "TR_TOL"; break;
                case "TR_HOT": WANTEDFORMRet = "TR_HOT"; break;
                case "TR_FRH": WANTEDFORMRet = "TR_FRH"; break;
                case "TR_KHH": WANTEDFORMRet = "TR_KHH"; break;
                case "TR_ENT": WANTEDFORMRet = "TR_ENT"; break;
                case "TR_DED": WANTEDFORMRet = "TR_DED"; break;
                case "TR_STU": WANTEDFORMRet = "TR_STU"; break;
                case "TR_SAZ": WANTEDFORMRet = "TR_SAZ"; break;
                case "TR_HOK": WANTEDFORMRet = "TR_HOK"; break;
                case "TR_MOR": WANTEDFORMRet = "TR_MOR"; break;
                case "ERSOF": WANTEDFORMRet = "ERSOF"; break;
                case "DAROF": WANTEDFORMRet = "DAROF"; break;
                case "chartfilter": WANTEDFORMRet = "chartfilter"; break;
                case "BLACK": WANTEDFORMRet = "BLACK"; break;

                // منوي تالار-----------------------------------------------------------
                case "PAZ": WANTEDFORMRet = "PAZIRESHTALAR"; break;
                case "PRAK": WANTEDFORMRet = "RACKTALAR"; break;
                case "TAL": WANTEDFORMRet = "TALARS_TARIF"; break;

                // پذيرش تعميرگاه-----------------------------------------------------------
                case "PTM": WANTEDFORMRet = "PTAM"; break;
                case "PKINCO": WANTEDFORMRet = "PKINCO"; break;
                case "AMVAL": WANTEDFORMRet = "AMVAL"; break;
                case "ESASSET": WANTEDFORMRet = "ESASSET"; break;
                case "MDKAR": WANTEDFORMRet = "MDKAR"; break;
                case "PMASSETS": WANTEDFORMRet = "PMASSETS"; break;
                case "PMLOCATION": WANTEDFORMRet = "PMLOCATION"; break;
                case "PMPMEM": WANTEDFORMRet = "PMPMEM"; break;
                case "PMKIND": WANTEDFORMRet = "PMKIND"; break;
                case "PMCALEN": WANTEDFORMRet = "PMCALEN"; break;
                case "PMWORKOR": WANTEDFORMRet = "PMWORKOR"; break;
                case "PMDARKH": WANTEDFORMRet = "PMDARKH"; break;
                case "PMFAILDEF": WANTEDFORMRet = "PMFAILDEF"; break;

                // بودجه-----------------------------------------------------------
                case "MMTAKH": WANTEDFORMRet = "MMTAKH"; break;
                case "ssmt": WANTEDFORMRet = "ssmt"; break;
                case "crmt": WANTEDFORMRet = "crmt"; break;
                case "TGUSER": WANTEDFORMRet = "TGUSER"; break;
                case "DEPEMAL": WANTEDFORMRet = "DEPEMAL"; break;
                case "USERS": WANTEDFORMRet = "USERS"; break;
            }

            try
            {
                var dbm = new CL_CCNNMANAGER();
                dbm.DoExecuteSQL("INSERT INTO AMALIAT (USERID, USERNAME, ADATE, AMALID) VALUES (@UserId, @UserName, GETDATE(), @FormName)",
                    new { UserId = Baseknow.USERCOD, UserName = Baseknow.UUSER, FormName = frm });
            }
            catch { }


            return WANTEDFORMRet;
        }

        public static bool LETSGO(string frm, string? WIN_NAME = null)
        {
            bool returnValue = false;

            if (!string.IsNullOrEmpty(WIN_NAME) && File.Exists(CL_Generaly.FILEACCESSPATH)) //With txt file if is exist
            {
                if (CL_LMethods.IsAllowedOpen(Baseknow.UUSER, WIN_NAME))
                {
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            }
            else
            {
                string THEFRM = WANTEDFORM(frm);
                THEFRM = string.IsNullOrEmpty(THEFRM) ? frm : THEFRM;

                var RST = dbms.DoGetDataSQL<Custom_LETSGO>("SELECT   dbo.TFORMS.FORMNAME, dbo.SAL_CHEK.USERCO, dbo.SAL_CHEK.RUN, dbo.SAL_CHEK.SEE, dbo.SAL_CHEK.INP, dbo.SAL_CHEK.UPD, dbo.SAL_CHEK.DEL FROM  dbo.TFORMS INNER JOIN  dbo.SAL_CHEK ON dbo.TFORMS.IDH = dbo.SAL_CHEK.OBJECT INNER JOIN dbo.SALA_DTL ON dbo.SAL_CHEK.USERCO = dbo.SALA_DTL.IDD WHERE  " +
                    "   (dbo.TFORMS.FORMNAME = '" + THEFRM + "') AND (dbo.SAL_CHEK.USERCO = " + Baseknow.USERCOD + " )").FirstOrDefault();
                if (RST != null)
                {
                    if (RST.RUN ?? false)
                    {
                        returnValue = true;
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
                else
                {
                    returnValue = false;
                }
            }

            return returnValue;
        }
        public static string GETKALANAME(double CODE)
        {
            string returnValue = "";
            var RRST = dbms.DoGetDataSQL<Custom_STUF_DEF>("SELECT CODE,NAME FROM STUF_DEF WHERE (CODE = " + Convert.ToString(CODE) + ")").FirstOrDefault();
            if (RRST != null)
            {
                if (string.IsNullOrEmpty(RRST.NAME))
                {
                    returnValue = " ";
                }
                else
                {
                    returnValue = RRST.NAME;
                }
            }
            else
            {
                returnValue = " ";
            }
            return returnValue;
        }
        public static void HAVEprice(int NUMB, int TGG, long PEID)
        {
            //The EOF Has Removed From Down Line in Query
            var rst2 = dbms.DoGetDataSQL<Custom_INVO_LST>("SELECT   CODE FROM INVO_LST WHERE     (dbo.INVO_LST.TAG =" + System.Convert.ToString(TGG) + ") AND (dbo.INVO_LST.NUMBER = " + System.Convert.ToString(NUMB) + ") ").ToList();
            foreach (var item in rst2)
            {
                var rst = dbms.DoGetDataSQL<Custom_INVO_LST>($"SELECT     dbo.STUF_DEF.CODE FROM         dbo.STUF_DEF LEFT OUTER JOIN    dbo.PRICE_ELAMIE_DTL ON dbo.STUF_DEF.PGID = dbo.PRICE_ELAMIE_DTL.PGID WHERE     (dbo.PRICE_ELAMIE_DTL.PEPID = {PEID}) AND (dbo.STUF_DEF.CODE = N'" + item.CODE + "')").FirstOrDefault();
                //if (ReferenceEquals(rst.CODE, null))
                if (ReferenceEquals(rst, null))
                {
                    //Red :#FFFF0000   Black : #FF000000
                    new Msgwin(false, $"کالای : {item.CODE} - {GETKALANAME(Convert.ToDouble(item.CODE))}\n  دارای گروه بندی قیمتی نیست یا  گروه آن  در اعلامیه قیمت تعریف نشده.", "#FFFF0000").Show();
                }
            }
        }


        public static void UpdateGHeymat(int NUMB, int TGG, long DTT, int MODAT_PPID, int CUST_KIND, int DEPATMAN, int TICMBAA)
        {
            double TFF = 0;
            double stf = 0;
            double MLBAA = 0;
            double IMBA = 0;
            if (MODAT_PPID != 0)
            {
                var rst = dbms.DoGetDataSQL<Custom_PRICE_ELAMIE>("SELECT  TOP (100) PERCENT PEPID FROM dbo.PRICE_ELAMIE WHERE (PEPDATE <= " + System.Convert.ToString(DTT) + ") And (PEPDEPART = " + System.Convert.ToString(DEPATMAN) + ") ORDER BY PEPID DESC").ToList();
                if (rst.Count > 0)
                {
                    string pepid = "";
                    //foreach (var item in rst)
                    //{
                    //    HAVEprice(NUMB, TGG == 13 ? 2 : TGG, item.PEPID);
                    //    pepid = item.PEPID.ToString();
                    //}
                    // انتخاب جدید ترین قیمیت به صورت تکی و اعمال آن
                    int rst_pepid = rst.Select(x => x.PEPID).FirstOrDefault();
                    HAVEprice(NUMB, TGG == 13 ? 2 : TGG, rst_pepid);
                    pepid = rst_pepid.ToString();

                    var quer_rst2 = dbms.DoGetDataSQL<Custom2_PRICE_ELAMIE>("SELECT        dbo.PRICE_ELAMIE_DTL.PRICE1, dbo.PRICE_ELAMIE_DTL.PGID, dbo.STUF_DEF.CODE, dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.INVO_LST.MEGH, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.IMBAA,dbo.PRICE_ELAMIE_DTL.PEPID FROM dbo.PRICE_ELAMIE_DTL INNER JOIN dbo.STUF_DEF ON dbo.PRICE_ELAMIE_DTL.PGID = dbo.STUF_DEF.PGID INNER JOIN dbo.INVO_LST ON dbo.STUF_DEF.CODE = dbo.INVO_LST.CODE WHERE        (dbo.INVO_LST.TAG = " + System.Convert.ToString(TGG == 13 ? 2 : TGG) + ") AND (dbo.INVO_LST.NUMBER =  " + System.Convert.ToString(NUMB) + ") AND (dbo.PRICE_ELAMIE_DTL.PEPID =  " + pepid + ")").ToList();
                    dbms.DoExecuteSQL("UPDATE head_lst  SET PEPID  = " + pepid + " WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG));
                    if (TGG == 13)
                    {
                        dbms.DoExecuteSQL("UPDATE HEAD_LST  SET PEPID  = " + pepid + " WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = 2 ");
                    }
                    //while (!rst2.EOF)
                    //{
                    foreach (var item in quer_rst2)
                    {
                        //dbms.DoExecuteSQL($"UPDATE HEAD_LST SET MABL = {item.PRICE1}, MABL_K =  {Math.Round(item.PRICE1 * item.MEGHk)} WHERE  NUMBER = " + Convert.ToString(NUMB) + "  AND TAG = 2");
                        dbms.DoExecuteSQL($"UPDATE INVO_LST SET MABL = {item.PRICE1}, MABL_K =  {Math.Round(item.PRICE1 * item.MEGHk)} WHERE  NUMBER = " + Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG) + $"  AND CODE = {item.CODE} AND MABL_K = {item.MABL_K} AND ANBAR = {item.ANBAR} AND IMBAA = {item.IMBAA} AND N_KOL = {item.N_KOL} AND MEGHk = {item.MEGHk}  ");
                    }
                    //}
                }
                else
                {
                    dbms.DoExecuteSQL("UPDATE INVO_LST SET IMBAA  = 0 , N_KOL = 0 ,N_MOIN = 0,TKHN = 0 , MABL_K =0 , MABL = 0   WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG == 13 ? 2 : TGG));

                    Msgwin msgwin1 = new Msgwin(false, "توجه برای این واحد و این نوع مشتری و این نحوه پرداخت اعلامیه قیمت و تخفیف مشخص نشده است."); msgwin1.ShowDialog();
                    ////MessageBox.Show("توجه برای این واحد و این نوع مشتری و این نحوه پرداخت اعلامیه قیمت و تخفیف مشخص نشده است.");

                    //DoCmd.OpenForm("mesag",,,,, acDialog, " ");
                    stf = 0;
                    MLBAA = 0;
                    IMBA = 0;

                    var qre_rst = dbms.DoGetDataSQL<Custom_PRICE_ELAMIE>("SELECT        TOP (100) PERCENT PEID FROM dbo.PRICE_ELAMIETF WHERE (PEDATE <= " + System.Convert.ToString(DTT) + ") And (PEPDEPART = " + System.Convert.ToString(DEPATMAN) + ") ORDER BY PEID DESC").ToList();
                    if (qre_rst.Count() > 0)
                    {
                        var rst3 = dbms.DoGetDataSQL<Custom3_PRICE_ELAMIE>("SELECT        TOP (100) PERCENT PEID,TF1, TF2 FROM dbo.PRICE_ELAMIETF_DTL WHERE (PEID = " + qre_rst.Select(x => x.PEPID).FirstOrDefault() + ") And (CUSTCODE = " + System.Convert.ToString(CUST_KIND) + ") And (PPID = " + System.Convert.ToString(MODAT_PPID) + ")").ToList();
                        dbms.DoExecuteSQL("UPDATE HEAD_LST  SET PEID  = " + qre_rst.Select(x => x.PEPID).FirstOrDefault() + " WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG));
                        if (TGG == 13)
                        {
                            dbms.DoExecuteSQL("UPDATE HEAD_LST  SET PEID  = " + qre_rst.Select(x => x.PEPID).FirstOrDefault() + " WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = 2");
                        }
                        if (rst3.Count() > 0)
                        {
                            var rst2 = dbms.DoGetDataSQL<Custom_INVO_STUF>("SELECT        TOP (100) PERCENT dbo.INVO_LST.CODE,  dbo.INVO_LST.TKHN, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.IMBAA , dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.STUF_DEF.CMBAA FROM            dbo.INVO_LST INNER JOIN dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE (dbo.INVO_LST.NUMBER = " + System.Convert.ToString(NUMB) + ") And (dbo.INVO_LST.TAG = " + System.Convert.ToString(TGG == 13 ? 2 : TGG) + ")").ToList();
                            string where = " WHERE(dbo.INVO_LST.NUMBER = " + System.Convert.ToString(NUMB) + ") And(dbo.INVO_LST.TAG = " + System.Convert.ToString(TGG == 13 ? 2 : TGG) + ")";
                            foreach (var item in rst2)
                            {
                                dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_KOL = {rst3.Select(x => x.TF1).FirstOrDefault()}  {where}   AND CODE = {item.CODE} AND MABL_K = {item.MABL_K} AND IMBAA = {item.IMBAA} AND N_KOL = {item.N_KOL} AND MEGHk = {item.MEGHk} ");
                                dbms.DoExecuteSQL($"UPDATE INVO_LST SET TKHN = {rst3.Select(x => x.TF2).FirstOrDefault()}  {where}  AND CODE = {item.CODE} AND MABL_K = {item.MABL_K} AND IMBAA = {item.IMBAA} AND N_KOL = {item.N_KOL} AND MEGHk = {item.MEGHk} ");
                                TFF = Math.Round(item.MABL_K * rst3.Select(x => x.TF1).FirstOrDefault() / 100) + Math.Round((item.MABL_K - Math.Round(item.MABL_K) * rst3.Select(x => x.TF1).FirstOrDefault() / 100) * rst3.Select(x => x.TF2).FirstOrDefault() / 100);
                                stf = stf + TFF;
                                dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_MOIN = {TFF}  {where}  AND CODE = {item.CODE} AND MABL_K = {item.MABL_K} AND IMBAA = {item.IMBAA} AND N_KOL = {item.N_KOL} AND MEGHk = {item.MEGHk} ");
                                if (Convert.ToBoolean(TICMBAA))
                                {
                                    if (item.CMBAA == false)
                                    {
                                        IMBA = Convert.ToDouble(Math.Round((item.MABL_K - TFF) * Convert.ToDouble(CL_HESABDARI.GetArzesh(item.CODE)) / 100));
                                    }
                                    else
                                    {
                                        IMBA = 0;
                                    }
                                }
                                else
                                {
                                    IMBA = 0;
                                }
                                dbms.DoExecuteSQL($"UPDATE INVO_LST SET IMBAA = {IMBA}  {where}  AND CODE = {item.CODE} AND MABL_K = {item.MABL_K} AND IMBAA = {item.IMBAA} AND N_KOL = {item.N_KOL} AND MEGHk = {item.MEGHk} ");
                                MLBAA = MLBAA + IMBA;
                            }
                        }
                        else
                        {
                            dbms.DoExecuteSQL("UPDATE INVO_LST  SET N_KOL = 0 ,N_MOIN = 0,TKHN = 0  WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG));
                            Msgwin msgwin = new Msgwin(false, "توجه برای این واحد و این نوع مشتری و این نحوه پرداخت اعلامیه  تخفیف مشخص نشده است."); msgwin.ShowDialog();
                            ////MessageBox.Show("توجه برای این واحد و این نوع مشتری و این نحوه پرداخت اعلامیه  تخفیف مشخص نشده است.");
                        }
                        dbms.DoExecuteSQL("UPDATE HEAD_LST SET MBAA  =  " + System.Convert.ToString(MLBAA) + " , TAKHFIF = " + System.Convert.ToString(stf) + "   WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = " + System.Convert.ToString(TGG));
                        if (TGG == 13)
                        {
                            dbms.DoExecuteSQL("UPDATE HEAD_LST SET MBAA  =  " + System.Convert.ToString(MLBAA) + " , TAKHFIF = " + System.Convert.ToString(stf) + "   WHERE  NUMBER = " + System.Convert.ToString(NUMB) + "  AND TAG = 2");
                        }
                    }
                }
                //------------
                stf = 0;
                MLBAA = 0;
                IMBA = 0;
                //var RST = dbms.DoGetDataSQL<Custom_PEID>("SELECT  TOP (100) PERCENT PEID FROM dbo.PRICE_ELAMIETF WHERE (PEDATE <= " + Convert.ToString(DTT) + ") And (PEPDEPART = " + Convert.ToString(DEPATMAN) + ")ORDER BY PEPID DESC").ToList();
                var RST = dbms.DoGetDataSQL<Custom_PEID>("SELECT        TOP (100) PERCENT PEID FROM dbo.PRICE_ELAMIETF WHERE (PEDATE <= " + DTT + ") And (PEPDEPART = " + DEPATMAN + ") ORDER BY PEID DESC").ToList();
                if (RST.Count > 0)
                {
                    var rst3 = dbms.DoGetDataSQL<Custom3_PRICE_ELAMIE>("SELECT        TOP (100) PERCENT PEID,TF1, TF2 FROM dbo.PRICE_ELAMIETF_DTL WHERE (PEID = " + RST.Select(x => x.PEID).FirstOrDefault() + ") And (CUSTCODE = " + Convert.ToString(CUST_KIND) + ") And (PPID = " + Convert.ToString(MODAT_PPID) + ")").ToList();
                    dbms.DoExecuteSQL("UPDATE head_lst  SET PEID  = " + RST.Select(x => x.PEID).FirstOrDefault() + " WHERE  NUMBER = " + NUMB + "  AND TAG = " + TGG);
                    if (TGG == 13)
                    {
                        dbms.DoExecuteSQL("UPDATE head_lst  SET PEID  = " + RST.Select(x => x.PEID).FirstOrDefault() + " WHERE  NUMBER = " + NUMB + "  AND TAG = 2");
                    }

                    if (rst3.Count > 0)
                    {
                        var RST2_Rec = dbms.DoGetDataSQL<Custom_INVO_STUF>("SELECT        TOP (100) PERCENT dbo.INVO_LST.CODE,  dbo.INVO_LST.TKHN, dbo.INVO_LST.MEGHk, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.INVO_LST.N_KOL, dbo.INVO_LST.N_MOIN, dbo.INVO_LST.IMBAA , dbo.INVO_LST.NUMBER, dbo.INVO_LST.TAG, dbo.STUF_DEF.CMBAA FROM            dbo.INVO_LST INNER JOIN dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE (dbo.INVO_LST.NUMBER = " + Convert.ToString(NUMB) + ") And (dbo.INVO_LST.TAG = " + Convert.ToString(TGG == 13 ? 2 : TGG) + ")").ToList();
                        string where = " WHERE (dbo.INVO_LST.NUMBER = " + Convert.ToString(NUMB) + ") And (dbo.INVO_LST.TAG = " + Convert.ToString(TGG == 13 ? 2 : TGG) + ") ";
                        foreach (var RST2_Fields in RST2_Rec)
                        {
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_KOL = {rst3.Select(x => x.TF1).FirstOrDefault()} {where}  AND CODE = {RST2_Fields.CODE} AND MABL_K = {RST2_Fields.MABL_K} AND IMBAA = {RST2_Fields.IMBAA} AND N_KOL = {RST2_Fields.N_KOL} AND MEGHk = {RST2_Fields.MEGHk} ");
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET TKHN = {rst3.Select(x => x.TF2).FirstOrDefault()} {where}  AND CODE = {RST2_Fields.CODE} AND MABL_K = {RST2_Fields.MABL_K} AND IMBAA = {RST2_Fields.IMBAA} AND N_KOL = {RST2_Fields.N_KOL} AND MEGHk = {RST2_Fields.MEGHk} ");
                            TFF = Math.Round(RST2_Fields.MABL_K * Convert.ToDouble(rst3.Select(x => x.TF1).FirstOrDefault()) / 100) + Math.Round((RST2_Fields.MABL_K - Math.Round(RST2_Fields.MABL_K * Convert.ToDouble(rst3.Select(x => x.TF1).FirstOrDefault()) / 100)) * Convert.ToDouble(rst3.Select(x => x.TF2).FirstOrDefault()) / 100);
                            stf = stf + TFF;
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET N_MOIN = {TFF} {where}  AND CODE = {RST2_Fields.CODE} AND MABL_K = {RST2_Fields.MABL_K} AND IMBAA = {RST2_Fields.IMBAA} AND N_KOL = {RST2_Fields.N_KOL} AND MEGHk = {RST2_Fields.MEGHk} ");
                            if (Convert.ToBoolean(TICMBAA))
                            {
                                if ((bool)RST2_Fields.CMBAA)
                                {
                                    IMBA = Math.Round((RST2_Fields.MABL_K - TFF) * Convert.ToDouble(CL_HESABDARI.GetArzesh(RST2_Fields.CODE)) / 100);
                                }
                                else
                                {
                                    IMBA = 0;
                                }
                            }
                            else
                            {
                                IMBA = 0;
                            }
                            dbms.DoExecuteSQL($"UPDATE INVO_LST SET IMBAA = {IMBA} {where}  AND CODE = {RST2_Fields.CODE} AND MABL_K = {RST2_Fields.MABL_K} AND IMBAA = {RST2_Fields.IMBAA} AND N_KOL = {RST2_Fields.N_KOL} AND MEGHk = {RST2_Fields.MEGHk} ");
                            MLBAA = MLBAA + IMBA;
                            //RST2.update();
                            //RST2.MoveNext();
                        }
                    }
                    else
                    {
                        dbms.DoExecuteSQL("UPDATE INVO_LST  SET N_KOL = 0 ,N_MOIN = 0,TKHN = 0  WHERE  NUMBER = " + NUMB + "  AND TAG = " + TGG);
                        Msgwin msgwin = new Msgwin(false, "توجه برای این واحد و این نوع مشتری و این نحوه پرداخت اعلامیه  تخفیف مشخص نشده است."); msgwin.ShowDialog();
                    }
                }
                dbms.DoExecuteSQL("UPDATE HEAD_LST SET MBAA  =  " + MLBAA + " , TAKHFIF = " + stf + "   WHERE  NUMBER = " + NUMB + "  AND TAG = " + TGG);
                if (TGG == 13)
                {
                    dbms.DoExecuteSQL("UPDATE HEAD_LST SET MBAA  =  " + MLBAA + " , TAKHFIF = " + stf + "   WHERE  NUMBER = " + NUMB + "  AND TAG = 2");
                }
            }
        }

        public static void UpdateGHeymatFF(int numb, int tgg, long PEPID, long PEID, int MODAT_PPID, int TICMBAA, int CUST_KIND)
        {
            if (MODAT_PPID == 0)
                return;

            // اجرای HAVEprice (تابع دیگر که باید از قبل در سیستم وجود داشته باشد)
            HAVEprice(numb, tgg == 13 ? 2 : tgg, PEPID);

            // مرحله اول: به‌روزرسانی MABL و MABL_K بر اساس PRICE1
            var priceDetails = dbms.DoGetDataSQL<PriceDtl>($@"
                    SELECT d.PRICE1, d.PGID, s.CODE, i.NUMBER, i.TAG, i.MEGH, i.MEGHK, i.MABL, i.MABL_K, i.N_KOL, i.N_MOIN, i.IMBAA, d.PEPID
                    FROM dbo.PRICE_ELAMIE_DTL d
                    INNER JOIN dbo.STUF_DEF s ON d.PGID = s.PGID
                    INNER JOIN dbo.INVO_LST i ON s.CODE = i.CODE
                    WHERE i.TAG = {(tgg == 13 ? 2 : tgg)} AND i.NUMBER = {numb} AND d.PEPID = {PEPID}
                ").ToList();

            foreach (var item in priceDetails)
            {
                double newMabl = item.PRICE1;
                double newMablK = Math.Round(item.PRICE1 * item.MEGHK);

                dbms.DoExecuteSQL($@"
                        UPDATE INVO_LST SET MABL = {newMabl}, MABL_K = {newMablK}
                        WHERE NUMBER = {item.NUMBER} AND CODE = N'{item.CODE}' AND TAG = {item.TAG}
                    ");
            }

            // مرحله دوم: اعمال تخفیف و مقادیر وابسته به TF1 و TF2
            var tfRow = dbms.DoGetDataSQL<TFModel>($@"
                    SELECT TOP (1) PEID, TF1, TF2 FROM dbo.PRICE_ELAMIETF_DTL
                    WHERE PEID = {PEID} AND PPID = {MODAT_PPID} AND CUSTCODE = {CUST_KIND}
                ").FirstOrDefault();

            double stf = 0, MLBAA = 0;

            if (tfRow != null)
            {
                var invos = dbms.DoGetDataSQL<InvoLstFF>($@"
                        SELECT i.CODE, i.TKHN, i.MEGHK, i.MABL, i.MABL_K, i.N_KOL, i.N_MOIN, i.IMBAA, i.NUMBER, i.TAG, s.CMBAA
                        FROM dbo.INVO_LST i
                        INNER JOIN dbo.STUF_DEF s ON i.CODE = s.CODE
                        WHERE i.NUMBER = {numb} AND i.TAG = {(tgg == 13 ? 2 : tgg)}
                    ").ToList();

                foreach (var row in invos)
                {
                    double tf1Percent = tfRow.TF1;
                    double tf2Percent = tfRow.TF2;
                    double mablK = row.MABL_K;
                    double tf1 = Math.Round(mablK * tf1Percent / 100.0);
                    double tf2 = Math.Round((mablK - tf1) * tf2Percent / 100.0);
                    double TFF = tf1 + tf2;

                    stf += TFF;
                    double IMBA = 0;

                    if (TICMBAA == 1)
                    {
                        if (row.CMBAA)
                        {
                            double arzesh = GetArzesh(row.CODE); // فرض بر اینکه این متد تعریف شده
                            IMBA = Math.Floor((row.MABL_K - TFF) * arzesh / 100.0);
                        }
                    }

                    MLBAA += IMBA;

                    dbms.DoExecuteSQL($@"
                        UPDATE INVO_LST
                        SET N_KOL = {tf1Percent}, TKHN = {tf2Percent}, N_MOIN = {TFF}, IMBAA = {IMBA}
                        WHERE NUMBER = {row.NUMBER} AND CODE = N'{row.CODE}' AND TAG = {row.TAG}
                    ");
                }
            }

            // مرحله سوم: آپدیت HEAD_LST
            dbms.DoExecuteSQL($@"
                    UPDATE HEAD_LST SET MBAA = {MLBAA}, TAKHFIF = {stf}
                    WHERE NUMBER = {numb} AND TAG = {tgg}
                ");

            if (tgg == 13)
            {
                dbms.DoExecuteSQL($@"
                    UPDATE HEAD_LST SET MBAA = {MLBAA}, TAKHFIF = {stf}
                    WHERE NUMBER = {numb} AND TAG = 2
                ");
            }
        }


        public static double Getmin(int ANBAR, string COD)
        {
            double returnValue = 0;
            var RST = dbms.DoGetDataSQL<Custom_StuFSK>("SELECT     MIN_M FROM dbo.stuf_fsk WHERE     anbar  = " + System.Convert.ToString(ANBAR) + " and code = '" + COD + "'").ToList();
            if (RST.Count > 0)
            {
                if (RST.Select(r => r.MIN_M).FirstOrDefault() == null || RST.Select(r => r.MIN_M).FirstOrDefault().ToString() == "")
                {
                    returnValue = 0;
                }
                else
                {
                    returnValue = Convert.ToDouble(RST.Select(r => r.MIN_M).FirstOrDefault());
                }
            }
            else
            {
                returnValue = 0;
            }
            return returnValue;
        }
        public static int Getmodat(int MODAT_PPID)
        {
            var RST = dbms.DoGetDataSQL<Custom_modat>("SELECT PPID, MODAT FROM PRICE_PAYNO where ppid = " + System.Convert.ToString(MODAT_PPID)).ToList();
            if (RST.Count > 0)
            {
                return System.Convert.ToInt32(RST.Select(x => x.MODAT).FirstOrDefault());
            }
            else
            {
                return 0;
            }
        }
        public static string Fixpi(string ST)
        {
            string FixpiRet = default;
            int I;
            int KeyA;
            string kar;
            var NUS = default(string);
            if (Strings.Mid(Baseknow.OPTIONSS, 48, 1) == "5")
            {
                var loopTo = Strings.Len(ST);
                for (I = 1; I <= loopTo; I++)
                {
                    kar = Strings.Mid(ST, I, 1);
                    KeyA = Strings.AscW(kar);
                    switch (KeyA)
                    {
                        case 1740:
                            {
                                KeyA = 1610;
                                break;
                            }
                        case 1705:
                            {
                                KeyA = 1603;
                                break;
                            }
                    }
                    NUS = NUS + Strings.ChrW(KeyA);
                }
                FixpiRet = NUS;
            }
            else
            {
                FixpiRet = ST;
            }
            return FixpiRet;
        }

        public static string Fixp(string ST)
        {
            int i = 0;
            int KeyA = 0;
            string kar = "";
            string NUS = "";
            if (Strings.Mid(System.Convert.ToString(Baseknow.OPTIONSS), 48, 1) == "5")
            {
                for (i = 1; i <= ST.Length; i++)
                {
                    kar = ST.Substring(i - 1, 1);
                    KeyA = Strings.AscW(kar);
                    switch (KeyA)
                    {
                        case 1610:
                        case 1609:
                        case 1656:
                        case 1744:
                        case 1741:
                            KeyA = 1740;
                            break;
                        case 1603:
                        case 1706:
                        case 1890:
                        case 1708:
                        case 1707:
                            KeyA = 1705;
                            break;
                    }
                    NUS = NUS + System.Convert.ToString(Strings.ChrW(KeyA));
                }
                return NUS;
            }
            else
            {
                return ST;
            }

        }
        public static double GETNESBAT(string CODEF, int VAHEDF)
        {
            var Rst = dbms.DoGetDataSQL<double?>("SELECT     NESBAT FROM dbo.MODULE_D WHERE     (CODE = N'" + CODEF + "') AND (VAHED = " + System.Convert.ToString(VAHEDF) + ")").FirstOrDefault();
            if (Rst == 0 || Rst is null)
            {
                return 1;
            }
            else
            {
                return System.Convert.ToDouble(Rst);
            }
        }
        public static double? GETGHeymatKala(int NUMB, int tgg, long DTT, int MODAT_PPID, int CUST_KIND, int DEPATMAN, int TICMBAA, string CODECC, long PEID, long PEPID, Action<bool>? onCompletion = null)
        {
            bool success = false;
            double GETGHeymatKalaRet = default;
            if (MODAT_PPID != 0)
            {
                var RST2 = dbms.DoGetDataSQL<Custom_PRICEONE>("SELECT dbo.PRICE_ELAMIE_DTL.PRICE1, dbo.PRICE_ELAMIE_DTL.PGID, dbo.STUF_DEF.CODE, dbo.PRICE_ELAMIE_DTL.PEPID FROM         dbo.PRICE_ELAMIE_DTL INNER JOIN   dbo.STUF_DEF ON dbo.PRICE_ELAMIE_DTL.PGID = dbo.STUF_DEF.PGID WHERE     (dbo.STUF_DEF.CODE = '" + CODECC + "') AND (dbo.PRICE_ELAMIE_DTL.PEPID = " + PEPID + ")").ToList();
                if (RST2.Count > 0)
                {
                    GETGHeymatKalaRet = (double)(RST2?.FirstOrDefault()?.PRICE1);
                    success = true;
                }
                else
                {
                    new Msgwin(false, "براي اين کالا قيمت يا گروه قيمتي تعريف نشده.").ShowDialog();
                }
            }

            onCompletion?.Invoke(success);

            return GETGHeymatKalaRet;
        }
        public static double? GETTaghfifKala1(int NUMB, int tgg, long DTT, int MODAT_PPID, int CUST_KIND, int DEPATMAN, int TICMBAA, string CODECC, long PEID, long PEPID)
        {
            double GETTaghfifKala1Ret = default;

            if (MODAT_PPID != 0)
            {
                var rst3 = dbms.DoGetDataSQL<Custom_TF12>("SELECT  TF1, TF2 FROM dbo.PRICE_ELAMIETF_DTL WHERE (PEID = " + PEID + ") And (CUSTCODE = " + CUST_KIND + ") And (PPID = " + MODAT_PPID + ")").ToList();
                if (rst3.Count > 0 && rst3.FirstOrDefault() != null)
                {
                    GETTaghfifKala1Ret = rst3.FirstOrDefault().TF1;
                }
                else
                {
                    dbms.DoExecuteSQL("UPDATE INVO_LST  SET N_KOL = 0 ,N_MOIN = 0,TKHN = 0  WHERE  NUMBER = " + NUMB + "  AND CODE = '" + CODECC + "'  AND TAG = " + ((tgg == 13) ? 2 : tgg));
                    new Msgwin(false, " توجه براي اين واحد و اين نوع مشتري و اين نحوه پرداخت و اين کالا اعلاميه  تخفيف مشخص نشده است.").ShowDialog();
                }
            }

            return GETTaghfifKala1Ret;
        }
        public static double? GETTaghfifKala2(int NUMB, int tgg, long DTT, int MODAT_PPID, int CUST_KIND, int DEPATMAN, int TICMBAA, string CODECC, long PEID, long PEPID)
        {
            double? GETTaghfifKala2Ret = default;
            if (MODAT_PPID != 0)
            {
                var rst3 = dbms.DoGetDataSQL<Custom_TF12>("SELECT        TOP (100) PERCENT TF1, TF2 FROM dbo.PRICE_ELAMIETF_DTL WHERE (PEID = " + PEID + ") And (CUSTCODE = " + CUST_KIND + ") And (PPID = " + MODAT_PPID + ")").ToList();
                if (rst3.Count > 0 && rst3.FirstOrDefault() != null)
                {
                    GETTaghfifKala2Ret = rst3.FirstOrDefault().TF2;
                }
                else
                {
                    dbms.DoExecuteSQL("UPDATE INVO_LST  SET N_KOL = 0 ,N_MOIN = 0,TKHN = 0  WHERE  NUMBER = " + NUMB + "  AND CODE = '" + CODECC + "'  AND TAG = " + ((tgg == 13) ? 2 : tgg));
                    new Msgwin(false, " توجه براي اين واحد و اين نوع مشتري و اين نحوه پرداخت و اين کالا اعلاميه  تخفيف مشخص نشده است.").ShowDialog();
                }
            }
            return GETTaghfifKala2Ret;
        }

        public static bool Sendbefor(double num, int tg)
        {
            var RST = dbms.DoGetDataSQL<NumTG>("SELECT     num,tg FROM dbo.TASKS WHERE     num  = " + Convert.ToString(num) + " and tg = " + Convert.ToString(tg)).ToList();
            if (RST.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GETTAFNAME(string HES)
        {
            string returnValue = "";
            var RRST = dbms.DoGetDataSQL<string>("SELECT     NAME FROM dbo.CUST_HESAB WHERE     (hes = N'" + HES + "')").ToList();
            if (RRST.Count > 0)
            {
                //if (ISNULL(RRST.Fields(0)))
                if (ReferenceEquals(RRST.First(), null))
                {
                    returnValue = " ";
                }
                else
                {
                    //returnValue = Convert.ToString(RRST.Fields(0));
                    returnValue = Convert.ToString(RRST.First());
                }
            }
            else
            {
                returnValue = " ";
            }
            return returnValue;
        }
        public static object UCurrentUser()
        {
            object UCurrentUserRet = default;
            UCurrentUserRet = Baseknow.UUSER;
            return UCurrentUserRet;
        }

        public static int GETSIGNER(int ID, int USERCO)
        {
            // Get Type of Sign like SGN4
            int GETSIGNERRet = default;
            switch (ID)
            {
                case 20:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_20>("SELECT   USERCO, FFRP_FROOSH, FFRP_ANB, FFRP_HESAB,FFRP_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.FFRP_MODIR == true)
                            {
                                GETSIGNERRet = 4;
                            }
                            else if (RST.FFRP_HESAB == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.FFRP_ANB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.FFRP_FROOSH == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 1:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_1>("SELECT   USERCO, ANB_RASID_ANB, ANB_RASID_QC FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.ANB_RASID_QC == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.ANB_RASID_ANB == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else if (RST.ANB_RASID_ANB == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 6:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_6>("SELECT   USERCO, ENTGH_AZANB, ENTGH_BEANB, ENTGH_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.ENTGH_MODIR == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.ENTGH_BEANB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.ENTGH_AZANB == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 13:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_13>("SELECT   USERCO, FFR_FROOSH, FFR_HESAB, FFR_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.FFR_MODIR == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.FFR_HESAB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.FFR_FROOSH == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 12:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_12>("SELECT   USERCO, KFR_BAZAR, KFR_HESAB, KFR_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.KFR_MODIR == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.KFR_HESAB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.KFR_BAZAR == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 2:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_2>("SELECT   USERCO, ANB_HAVL_FROSH, ANB_HAVL_ANB, ANB_HAVL_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.ANB_HAVL_MODIR == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.ANB_HAVL_ANB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.ANB_HAVL_FROSH == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }

                case 100:
                    {
                        var RST = dbms.DoGetDataSQL<Signer_100>("SELECT   USERCO, PAY_MVAHED, PAY_HESAB, PAY_MAMEL FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(RST, null))
                        {
                            if (RST.PAY_MAMEL == true)
                            {
                                GETSIGNERRet = 3;
                            }
                            else if (RST.PAY_HESAB == true)
                            {
                                GETSIGNERRet = 2;
                            }
                            else if (RST.PAY_MVAHED == true)
                            {
                                GETSIGNERRet = 1;
                            }
                            else
                            {
                                GETSIGNERRet = 0;
                            }
                        }
                        else
                        {
                            GETSIGNERRet = 0;
                        }

                        break;
                    }
            }
            return GETSIGNERRet;
        }

        public static string GETUSERTASK(double IDNUM)
        {
            string res = "";
            var RST_USERCO = dbms.DoGetDataSQL<int>("SELECT     USERCO FROM dbo.TASKS WHERE     IDNUM = " + Convert.ToString(IDNUM)).FirstOrDefault();
            if (!ReferenceEquals(RST_USERCO, null))
            {
                res = Convert.ToString(RST_USERCO);
            }
            return res;
        }


        public static string UserOnChart(int USERCOD)
        {
            string UserOnChartRet = "";
            var vs = "";
            var RST = dbms.DoGetDataSQL<USERCHART_QRE>("SELECT SAL_NAME , CHARTSAZMANI.SUBUSERCO, CHARTSAZMANI.USERCO FROM CHARTSAZMANI LEFT OUTER JOIN  SALA_DTL ON CHARTSAZMANI.SUBUSERCO = SALA_DTL.IDD WHERE (((CHARTSAZMANI.USERCO)=" + USERCOD + "))").ToList();
            if (RST.Count > 0)
            {
                //while (!RST.EOF)
                foreach (var item in RST)
                {
                    if (item.SUBUSERCO == 0)
                    {
                        goto allu;
                    }
                    else if (string.IsNullOrEmpty(vs))
                    {
                        vs = "(USER_NAME = N'" + DECODEUN(item.SAL_NAME) + "') ";
                    }
                    else
                    {
                        vs = vs + " or (USER_NAME = N'" + DECODEUN(item.SAL_NAME) + "') ";
                    }
                    //RST.MoveNext();
                }
                vs = "(" + vs + ")";
            }

        allu:

            UserOnChartRet = vs;
            return UserOnChartRet;
        }

        public static bool BLOCKEDCUST(string HES)
        {
            var RST2_HES = dbms.DoGetDataSQL<string>("SELECT     HES FROM dbo.BLOCK_CUSTOMER  WHERE     (HES = N'" + HES + "') AND (ENDBLK = 0) ").FirstOrDefault();
            if (!ReferenceEquals(RST2_HES, null))
            {
                return true;
            }
            return false;
        }

        public static int Checketebar(string HES)
        {
            int ChecketebarRet = default;
            double MAND;
            double CHK;
            double TOPETEB;
            var TOPASN = default(double);
            var ENDIS = default(bool);
            CHK = 0d;
            TOPETEB = 0d;
            var RST = dbms.DoGetDataSQL<ETEBAR_AZAE>(@"SELECT HES,
                                                                 TOPETEB,
                                                                 TOPASN,
                                                                 ENDIS,
                                                                 TOPMEGH,
                                                                 JAMZARIB,
                                                                 EMTIAZ,
                                                                 gradeid,
                                                                 DTTIM FROM AZAE WHERE HES = '" + HES + "'").FirstOrDefault();
            if (!ReferenceEquals(RST, null))
            {
                TOPETEB = Convert.ToDouble(RST.TOPETEB);
                TOPASN = Convert.ToDouble(RST.TOPASN);
                ENDIS = Convert.ToBoolean(RST.ENDIS);
            }

            if (ENDIS)
            {
                var RST_HES = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE HES = '" + HES + "'").FirstOrDefault();
                if (ReferenceEquals(RST_HES, null))
                {
                    MAND = 0;
                }
                else
                {
                    MAND = (double)RST_HES;
                }

                if (!Convert.ToBoolean(Baseknow.SAGHF))
                {
                    MAND = 0d;
                }
                ;////////////
                var RSTOpen = dbms.DoGetDataSQL<double?>(" SELECT     SUM(MABL) AS smab FROM         dbo.PAY_GETD WHERE     (N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (CUST_NO = N'" + HES + "') AND (N_KOL = " + Baseknow.BANKHA + " OR  N_KOL IS NULL)").FirstOrDefault();
                if (ReferenceEquals(RSTOpen, null))
                {
                    CHK = 0;
                }
                else
                {
                    CHK = Convert.ToDouble(RSTOpen);
                }
                // 'چك صفر شد
                if (!Convert.ToBoolean(Baseknow.SAGHF2) || !Convert.ToBoolean(TOPASN))
                {
                    CHK = 0;
                }
                var RSTRecordCount = dbms.DoGetDataSQL<ETEBAR_AZAE>("SELECT  HES,TOPETEB,TOPASN,ENDIS,TOPMEGH,JAMZARIB,EMTIAZ,gradeid,DTTIM  FROM AZAE WHERE HES = '" + HES + "'").FirstOrDefault();
                if (!ReferenceEquals(RSTRecordCount, null))
                {
                    if (TOPETEB - MAND - CHK >= 0d)
                    {
                        ChecketebarRet = Conversions.ToInteger(true);
                    }
                    else
                    {
                        ChecketebarRet = Conversions.ToInteger(false);
                        Msgwin msgwin = new Msgwin(false, "مانده حساب : " + MAND.ToString("#,###") + "  مانده اسناد : " + CHK.ToString("#,###") + "  كل اعتبار : " + TOPETEB.ToString("#,###")); msgwin.ShowDialog();
                        //MessageBox.Show("مانده حساب : " + MAND.ToString("#,###") + "  مانده اسناد : " + CHK.ToString("#,###") + "  كل اعتبار : " + TOPETEB.ToString("#,###"));
                    }
                }
                else
                {
                    ChecketebarRet = 2;
                }
            }
            else
            {
                ChecketebarRet = Conversions.ToInteger(true);
            }

            return ChecketebarRet;
        }

        //public int ChekEnheSar(long num, int tgg)
        //{
        //    int ChekEnheSarRet = default;
        //    double CHK;
        //    double NOTCH;
        //    double MABCHKR;
        //    double MANDOLD;
        //    double MABCHK;
        //    bool ENDIS;
        //    int DIFFU;
        //    bool MATCHTO;
        //    bool ONEONLY;
        //    string PYT;
        //    CR_NFANI();
        //    rst.Open("SELECT     dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.CUST_NO, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH,  dbo.INVO_LST.MEGHk, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.CUST_HESAB.OSTANID, dbo.CUST_HESAB.SHAHRID,   dbo.stuf_def_nfani.N_FANI, dbo.stuf_def_nfani.col1, dbo.stuf_def_nfani.col2, dbo.stuf_def_nfani.col3, dbo.stuf_def_nfani.col4, dbo.stuf_def_nfani.col5, dbo.stuf_def_nfani.col6, dbo.stuf_def_nfani.col7, dbo.stuf_def_nfani.col8, dbo.stuf_def_nfani.col9, dbo.stuf_def_nfani.colN1, dbo.stuf_def_nfani.colN2,  dbo.stuf_def_nfani.colN3, dbo.stuf_def_nfani.colN4, dbo.stuf_def_nfani.colN5, dbo.stuf_def_nfani.colN6, dbo.stuf_def_nfani.colN7, dbo.stuf_def_nfani.colN8 , dbo.stuf_def_nfani.colN9 FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG INNER JOIN  dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes INNER JOIN" + " dbo.stuf_def_nfani ON dbo.INVO_LST.CODE = dbo.stuf_def_nfani.CODE WHERE     (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.NUMBER = " + num + ")");
        //    if (!(rst.RecordCount == 0 | IsNull(rst.Fields(0))))
        //    {
        //        if (IsNull(rst.Fields("ostanid")) | IsNull(rst.Fields("SHAHRID")))
        //        {
        //            if (LETSGO("moshtari") | LETSGO("custdef"))
        //            {
        //                // DoCmd.OpenForm "", , , "hes ='" & RST.Fields("cust_no") & "'", , acDialog
        //                DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد");
        //            }
        //            else
        //            {
        //                DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد");
        //            }
        //            return ChekEnheSarRet;
        //        }
        //        RST2.Open("SELECT   EN_CUST_CO, EN_CITY, EN_IYALAT, EN_COUNTRY FROM dbo.ENHESAR_MOSHTARI WHERE     (EN_CUST_CO <> N'" + rst.Fields("cust_no") + "') AND (EN_CITY =" + rst.Fields("shahrid") + ") AND (EN_STDATE <= " + rst.Fields("date_n") + ") AND (EN_ENDEN >= " + rst.Fields("date_n") + " OR EN_ENDEN IS NULL)");
        //        if (!(RST2.RecordCount == 0 | IsNull(RST2.Fields("EN_CITY"))))   // اگر انحصارشهر هست
        //        {
        //            RST2.Open("SELECT dbo.ENHESAR_MOSHTARI.EN_CUST_CO, dbo.ENHESAR_MOSHTARI.EN_CITY, dbo.ENHESAR_MOSHTARI.EN_IYALAT, dbo.ENHESAR_MOSHTARI.EN_COUNTRY, dbo.ENHESAR_MOSHTARI.EN_STDATE, dbo.ENHESAR_MOSHTARI.EN_ENDEN, dbo.ENHESAR_KALA.EN_KALA_COD, dbo.ENHESAR_KALA.EN_KALA_GR, dbo.ENHESAR_KALA.EN_GRCODE1, dbo.ENHESAR_KALA.EN_GRCODE2, dbo.ENHESAR_KALA.EN_GRCODE3, dbo.ENHESAR_KALA.EN_GRCODE4, dbo.ENHESAR_KALA.EN_GRCODE5, dbo.ENHESAR_KALA.EN_GRCODE6, dbo.ENHESAR_KALA.EN_GRCODE7, dbo.ENHESAR_KALA.EN_GRCODE8, dbo.ENHESAR_KALA.EN_GRCODE9 , dbo.ENHESAR_KALA.EN_GRCODE10, dbo.ENHESAR_KALA.USERID, dbo.ENHESAR_KALA.EN_CDATE FROM  dbo.ENHESAR_MOSHTARI INNER JOIN dbo.ENHESAR_KALA ON dbo.ENHESAR_MOSHTARI.EN_CUST_CO = dbo.ENHESAR_KALA.EN_CUST_CO  WHERE     (dbo.ENHESAR_MOSHTARI.EN_CUST_CO <> N'" + rst.Fields("cust_no") + "') AND (dbo.ENHESAR_MOSHTARI.EN_CITY =" + rst.Fields("shahrid") + ") AND " + " (EN_STDATE <= " + rst.Fields("date_n") + ") AND (EN_ENDEN >= " + rst.Fields("date_n") + " OR EN_ENDEN IS NULL)");
        //            while (!RST2.EOF)
        //            {
        //                if (!IsNull(RST2.Fields("EN_KALA_COD")))
        //                {
        //                    while (!rst.EOF)
        //                    {
        //                        if (rst.Fields("CODE") == RST2.Fields("EN_KALA_COD"))
        //                        {
        //                            DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST2.Fields("EN_CUST_CO")) + '\r' + "نام کالا:" + GETKALANAME(rst.Fields("CODE")));
        //                            ChekEnheSarRet = Conversions.ToInteger(true);
        //                        }
        //                        rst.MoveNext();
        //                    }
        //                    rst.MoveFirst();
        //                }
        //                else
        //                {
        //                    while (!rst.EOF)
        //                    {
        //                        MATCHTO = true;  // تطبيق داردبا گروه کالا
        //                        ONEONLY = false; // حداقل با يک گروه تطبيق دارد
        //                        PYT = "";
        //                        if (!IsNull(RST2.Fields("EN_GRCODE1")) & RST2.Fields("EN_GRCODE1") == -1)   // همه کالاها
        //                        {
        //                            PYT = " همه کالاها ";
        //                            MATCHTO = true;
        //                            ONEONLY = true;
        //                        }
        //                        else
        //                        {
        //                            for (int i = 1; i <= 9; i++)
        //                            {
        //                                if (!IsNull(RST2.Fields("EN_GRCODE" + i)))
        //                                {
        //                                    if (RST2.Fields("EN_GRCODE" + i) != rst.Fields("COL" + i))
        //                                    {
        //                                        MATCHTO = false;
        //                                    }
        //                                    else
        //                                    {
        //                                        PYT = PYT + " - " + rst.Fields("COLN" + i);
        //                                    }
        //                                    ONEONLY = true;
        //                                }
        //                            }
        //                        }
        //                        if (MATCHTO & ONEONLY)
        //                        {
        //                            DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST2.Fields("EN_CUST_CO")) + '\r' + ":گروه کالا" + PYT);
        //                            ChekEnheSarRet = Conversions.ToInteger(true);
        //                        }
        //                        rst.MoveNext();
        //                    }
        //                    rst.MoveFirst();
        //                }
        //                RST2.MoveNext();
        //            }
        //        }
        //        else
        //        {
        //            RST2.Open("SELECT   EN_CUST_CO, EN_CITY, EN_IYALAT, EN_COUNTRY FROM dbo.ENHESAR_MOSHTARI WHERE     (EN_CUST_CO <> N'" + rst.Fields("cust_no") + "') AND (EN_IYALAT =" + rst.Fields("OSTANID") + ") AND (EN_STDATE <= " + rst.Fields("date_n") + ") AND (EN_ENDEN >= " + rst.Fields("date_n") + " OR EN_ENDEN IS NULL)");
        //            if (!(RST2.RecordCount == 0 | IsNull(RST2.Fields("EN_IYALAT"))))   // اگر انحصاراستان هست
        //            {
        //                ;
        //                RST2.Open("SELECT dbo.ENHESAR_MOSHTARI.EN_CUST_CO, dbo.ENHESAR_MOSHTARI.EN_CITY, dbo.ENHESAR_MOSHTARI.EN_IYALAT, dbo.ENHESAR_MOSHTARI.EN_COUNTRY, dbo.ENHESAR_MOSHTARI.EN_STDATE, dbo.ENHESAR_MOSHTARI.EN_ENDEN, dbo.ENHESAR_KALA.EN_KALA_COD, dbo.ENHESAR_KALA.EN_KALA_GR, dbo.ENHESAR_KALA.EN_GRCODE1, dbo.ENHESAR_KALA.EN_GRCODE2, dbo.ENHESAR_KALA.EN_GRCODE3, dbo.ENHESAR_KALA.EN_GRCODE4, dbo.ENHESAR_KALA.EN_GRCODE5, dbo.ENHESAR_KALA.EN_GRCODE6, dbo.ENHESAR_KALA.EN_GRCODE7, dbo.ENHESAR_KALA.EN_GRCODE8, dbo.ENHESAR_KALA.EN_GRCODE9 , dbo.ENHESAR_KALA.EN_GRCODE10, dbo.ENHESAR_KALA.USERID, dbo.ENHESAR_KALA.EN_CDATE FROM  dbo.ENHESAR_MOSHTARI INNER JOIN dbo.ENHESAR_KALA ON dbo.ENHESAR_MOSHTARI.EN_CUST_CO = dbo.ENHESAR_KALA.EN_CUST_CO  WHERE     (dbo.ENHESAR_MOSHTARI.EN_CUST_CO <> N'" + rst.Fields("cust_no") + "') AND (EN_IYALAT =" + rst.Fields("OSTANID") + ") AND " + " (EN_STDATE <= " + rst.Fields("date_n") + ") AND (EN_ENDEN >= " + rst.Fields("date_n") + " OR EN_ENDEN IS NULL)");
        //                while (!RST2.EOF)
        //                {
        //                    if (!IsNull(RST2.Fields("EN_KALA_COD")))
        //                    {
        //                        while (!rst.EOF)
        //                        {
        //                            if (rst.Fields("CODE") == RST2.Fields("EN_KALA_COD"))
        //                            {
        //                                DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST2.Fields("EN_CUST_CO")) + '\r' + "نام کالا:" + GETKALANAME(rst.Fields("CODE")));
        //                                ChekEnheSarRet = Conversions.ToInteger(true);
        //                            }
        //                            rst.MoveNext();
        //                        }
        //                        rst.MoveFirst();
        //                    }
        //                    else
        //                    {
        //                        while (!rst.EOF)
        //                        {
        //                            MATCHTO = true;
        //                            ONEONLY = false;
        //                            PYT = "";
        //                            if (!IsNull(RST2.Fields("EN_GRCODE1")) & RST2.Fields("EN_GRCODE1") == -1)   // همه کالاها
        //                            {
        //                                PYT = " همه کالاها ";
        //                                MATCHTO = true;
        //                                ONEONLY = true;
        //                            }
        //                            else
        //                            {
        //                                for (int i = 1; i <= 9; i++)
        //                                {
        //                                    if (!IsNull(RST2.Fields("EN_GRCODE" + i)))
        //                                    {
        //                                        if (RST2.Fields("EN_GRCODE" + i) != rst.Fields("COL" + i))
        //                                        {
        //                                            MATCHTO = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            PYT = PYT + " - " + rst.Fields("COLN" + i);
        //                                        }
        //                                        ONEONLY = true;
        //                                    }
        //                                }
        //                            }
        //                            if (MATCHTO & ONEONLY)
        //                            {
        //                                DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST2.Fields("EN_CUST_CO")) + '\r' + ":گروه کالا" + PYT);
        //                                ChekEnheSarRet = Conversions.ToInteger(true);
        //                            }
        //                            rst.MoveNext();
        //                        }
        //                        rst.MoveFirst();
        //                    }
        //                    RST2.MoveNext();
        //                }
        //            }
        //            else
        //            {
        //                ChekEnheSarRet = Conversions.ToInteger(false);
        //            }
        //        }
        //    }

        //    return ChekEnheSarRet;

        //}


        public static int ChecketebarMEG(string HES)
        {
            //بررسی اعتبار منتها 80 درصد  درست هست 20 درصد به خاطر برگشت فروش آزاد باید اصلاح بشه که درست نیست
            int ChecketebarMEGRet = default;
            double MAND;
            double CHK;
            double TOPMEGH;
            var ENDIS = default(bool);
            TOPMEGH = 0d;
            var RST = dbms.DoGetDataSQL<ETEBAR_AZAE>("SELECT  HES,TOPETEB,TOPASN,ENDIS,TOPMEGH,JAMZARIB,EMTIAZ,gradeid,DTTIM  FROM AZAE WHERE HES = '" + HES + "'").FirstOrDefault();
            if (!ReferenceEquals(RST, null))
            {
                TOPMEGH = Convert.ToDouble(RST.TOPMEGH);
                ENDIS = Convert.ToBoolean(RST.ENDIS);
            }

            if (ENDIS && TOPMEGH != 0d)
            {
                var RST9 = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.INVO_LST.MEGHk - dbo.INVO_LST.MEGH_MAR) AS MEGH FROM    dbo.HEAD_LST INNER JOIN  dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE     (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.CUST_NO = N'" + HES + "')").FirstOrDefault();
                if (ReferenceEquals(RST9, null))
                {
                    MAND = 0d;
                }
                else
                {
                    MAND = (double)RST9;
                }

                if (!Convert.ToBoolean(Baseknow.SAGHF))
                {
                    MAND = 0d;
                }

                if (TOPMEGH - MAND >= 0d)
                {
                    ChecketebarMEGRet = Conversions.ToInteger(true);
                }
                else
                {
                    ChecketebarMEGRet = Conversions.ToInteger(false);
                    Msgwin msgwin = new Msgwin(false, "  كل اعتبار مقداري : " + Strings.Format((object)TOPMEGH, "#,###") + "  كل خريد مقداري : " + Strings.Format((object)MAND, "#,###")); msgwin.ShowDialog();
                    //MessageBox.Show("  كل اعتبار مقداري : " + Strings.Format((object)TOPMEGH, "#,###") + "  كل خريد مقداري : " + Strings.Format((object)MAND, "#,###"));
                }
            }
            else
            {
                ChecketebarMEGRet = Conversions.ToInteger(true);
            }

            return ChecketebarMEGRet;
        }

        /// <summary>
        /// For Delete Button Security there is a tag as "hazfer"
        /// __new WindowInteropHelper(this).Handle
        /// </summary>
        /// <param name="frm">Short Name of Form for Security Query</param>
        /// <param name="obj">Right and Full Name Form</param>
        public static bool SETSECURITY(string obj, string frm, IntPtr windowHandle, string? WIN_NAME = null, bool IsNotWindow = false)
        {
            if (!IsNotWindow) //It is window
            {
                if (windowHandle == null || windowHandle == IntPtr.Zero)
                {
                    // Handle the case where windowHandle is zero, possibly logging or notifying the user.
                    return false;
                }
            }

            Window? TheWind = null;
            if (!IsNotWindow) //It is window
            {
                TheWind = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            }

            if (!string.IsNullOrEmpty(WIN_NAME) && File.Exists(CL_Generaly.FILEACCESSPATH)) //Filey
            {
                if (!CL_LMethods.IsAllowedOpen(Baseknow.USERCOD.ToString(), WIN_NAME)) //Not Allowed to open
                {
                    TheWind?.Close();
                    Msgwin msgwin = new Msgwin(false, "شماره اجازه دسترسی به این بخش را ندارید !"); msgwin.ShowDialog();
                    return false;
                }
            }
            else //SQLY
            {
                string formName = WANTEDFORM(frm);

                // 1️⃣ Get FORM ID
                var formInfo = dbms.DoGetDataSQL<TFORMS>(
                    "SELECT IDH, CAPTION, KIND FROM TFORMS WHERE FORMNAME = @FORMNAME",
                    new { FORMNAME = formName }
                ).FirstOrDefault();

                var RST = dbms.DoGetDataSQL<SECURITYQuery>("SELECT TFORMS.CAPTION, TFORMS.kind, SAL_CHEK.USERCO, SAL_CHEK.RUN, SAL_CHEK.SEE, SAL_CHEK.INP, SAL_CHEK.UPD, SAL_CHEK.DEL FROM TFORMS INNER JOIN (SALA_DTL INNER JOIN SAL_CHEK ON SALA_DTL.IDD = SAL_CHEK.USERCO) ON TFORMS.IDH = SAL_CHEK.OBJECT WHERE  " +
                    "   (TFORMS.FORMNAME = '" + formName + "') AND (SAL_CHEK.USERCO = " + Baseknow.USERCOD + " )").FirstOrDefault();

                ////if (RST.INP != true || RST.UPD != true) { TheWind.IsEnabled = false; return; }

                // 3️⃣ AUTO‑CREATE SECURITY ROW IF MISSING ✅
                if (RST == null)
                {
                    try
                    {
                        dbms.DoExecuteSQL(
                            @"INSERT INTO SAL_CHEK 
(USERCO, OBJECT, RUN, SEE, INP, UPD, DEL, CRT, UID)
VALUES
(@USERCO, @OBJECT, 0, 0, 0, 0, 0, GETDATE(), @UID)",
                            new
                            {
                                USERCO = Baseknow.USERCOD,
                                OBJECT = formInfo.IDH,
                                UID = Baseknow.USERCOD
                            }
                            );
                    }
                    catch (Exception)
                    {
                        TheWind?.Close();
                        new Msgwin(false, "این کاربری اطلاعات آن ناقص است خطا در انجام عملیات.").ShowDialog();
                        return false;
                    }


                    TheWind?.Close();
                    new Msgwin(false, "دسترسی به فرم فعال شد، ولی هنوز تنظیمات مجوز کامل نیست. بعد از تنظیم مجوزها در تعیین سطح دسترسی دوباره وارد شوید.").ShowDialog();
                    return false;
                }

                if (RST != null)
                {
                    if (RST.SEE != true) //SEE : دیدن | مشاهده داده
                    {
                        TheWind?.Close();
                        Msgwin msgwin = new Msgwin(false, "شماره اجازه دسترسی به این بخش را ندارید !"); msgwin.ShowDialog();
                        return false;
                    }
                    else if (RST.INP != true) //INP : اضافه کردن | ورود داده
                    {
                        if (TheWind != null)
                        {
                            //TheWind.IsEnabled = false;
                            CL_HELPER.SetInsertButtonSecurity(TheWind, TheWind.Name, isInsertAllowed: false);
                        }
                    }
                    else if (RST.UPD != true) //UPD : بهنگام سازی | ویرایش
                    {
                        if (TheWind != null)
                        {
                            CL_HELPER.SetEditButtonSecurity(TheWind, TheWind.Name, isEditAllowed: false);
                        }
                    }
                    else if (RST.DEL != true) //DEL : حذف کردن
                    {
                        ////The Buttons Can Delete the Tag name is : "hazfer" #Check
                        //var TheDeletorBtn = CL_LMethods.FindVisualChildren<Button>(TheWind).Where(x => x.Tag != null && x.Tag.ToString() == "hazfer").ToList();
                        //foreach (var item in TheDeletorBtn)
                        //{
                        //    item.IsEnabled = false;
                        //    item.Visibility = Visibility.Hidden;
                        //}
                        if (TheWind != null)
                        {
                            CL_HELPER.SetDeleteButtonSecurity(TheWind, TheWind.Name, isDeleteAllowed: false);
                        }
                    }
                }
                else
                {
                    TheWind?.Close();
                    Msgwin msgwin = new Msgwin(false, "تنظیم پیاده سازی مربوط به دسترسی انجام نشده با پشتیبانی در ارتباط باشید"); msgwin.ShowDialog();
                    return false;
                }

            }

            #region COMMENT
            //Window window = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            //var TheWind = Application.Current.Windows.OfType<Window>().SingleOrDefault(win => win.GetType().Name.Equals(obj));

            //if (obj is "HEAD_LST_HAVL")//اگر حواله انبار فروش هست یا همون صدور برگه حواله انبار فروش //#Left
            //{
            //    (TheWind as HEAD_LST_HAVL).MAY_OPEN_HAVALE = false;
            //}
            //if (obj is "HAVALE_LIST")//اگر حواله انبار فروش هست یا همون صدور برگه حواله انبار فروش
            //{
            //    (TheWind as Anbar.HAVALE_LIST).MAY_OPEN_LST_HAVALEHA = false;
            //}
            //if (obj is "HAVALE_LIST")//اگر حواله انبار فروش هست یا همون صدور برگه حواله انبار فروش
            //{
            //    (TheWind as Anbar.HAVALE_LIST).MAY_OPEN_LST_HAVALEHA = false;
            //}
            #endregion

            return true;
        }

        /// <summary>
        /// this.GetType().Name
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="FORMNAME"></param>
        public static void SETSECURITYSUB(DataGrid dataGrid, string FORMNAME)
        {
            var rst = dbms.DoGetDataSQL<SETSECURITYSUB_CSHARP>("SELECT TFORMS.CAPTION, TFORMS.kind, SAL_CHEK.USERCO, SAL_CHEK.RUN, SAL_CHEK.SEE, SAL_CHEK.INP, SAL_CHEK.UPD, SAL_CHEK.DEL FROM TFORMS INNER JOIN (SALA_DTL INNER JOIN SAL_CHEK ON SALA_DTL.IDD = SAL_CHEK.USERCO) ON TFORMS.IDH = SAL_CHEK.OBJECT WHERE     (TFORMS.FORMNAME = '" + WANTEDFORM(FORMNAME) + "') AND (SAL_CHEK.USERCO = " + Baseknow.USERCOD + " )").FirstOrDefault();
            if (!(rst is null))
            {
                if ((bool)!rst.SEE)
                {
                    dataGrid.IsEnabled = false;
                }
                if ((bool)!rst.INP)
                {
                    dataGrid.CanUserAddRows = false;
                }
                if ((bool)!rst.UPD)
                {
                    dataGrid.IsReadOnly = false;
                }
                if ((bool)!rst.DEL)
                {
                    dataGrid.CanUserDeleteRows = false;
                }
            }
        }

        public static object SETLEVEL(int LV)
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            switch (LV)
            {
                case 0:
                    {
                        dbms.DoExecuteSQL("UPDATE    dbo.SAL_CHEK Set RUN = 0 WHERE     (OBJECT BETWEEN 40 AND 43) OR  (OBJECT = 189) OR  (OBJECT = 160) OR  (OBJECT = 12) OR (OBJECT = 161) OR (OBJECT = 167) OR (OBJECT = 184) OR (OBJECT = 181)");

                        break;
                    }
                case 1:
                    {
                        dbms.DoExecuteSQL("UPDATE    dbo.SAL_CHEK Set RUN = 0 WHERE     (OBJECT BETWEEN 368 AND 381) ");

                        break;
                    }
                case 2:
                    {
                        //DoCmd.Beep();
                        break;
                    }
                case 3:
                    {
                        //DoCmd.Beep();
                        break;
                    }
            }
            return default;
        }

        public static string GetVisitHES(string HES)
        {
            var RST = dbms.DoGetDataSQL<string>("SELECT     dbo.Visit_route.HES AS Expr1 FROM         dbo.CUST_HESAB INNER JOIN      dbo.Visit_route ON dbo.CUST_HESAB.ROUTE_NAME = dbo.Visit_route.ROUTE_NAME WHERE     (dbo.CUST_HESAB.hes = N'" + HES + "')").FirstOrDefault();

            if (ReferenceEquals(RST, null))
            {
                return "";
            }
            else
            {
                return System.Convert.ToString(RST);
            }
        }

        public static void CR_NFANI()
        {
            int i;
            int itwo = 1;
            string sqlstr;
            string sqlstrmp;
            var sqlstrjo = default(string);
            var sqlstrwe = default(string);
            int FR;

            var RST = dbms.DoGetDataSQL<TCOD_MAP_GRP>("SELECT MPP,MPNAME,SIZEF,STARTF FROM TCOD_MAP_GRP").ToList();
            if (RST.Count > 0)
            {
                FR = (int)RST.Select(f => f.SIZEF).FirstOrDefault();
                sqlstr = "LEFT(N_FANI, " + RST.Select(f => f.SIZEF).FirstOrDefault() + ") as col1";
                sqlstrmp = " , TCOD_MAP_1.MPNAME AS colN1";
                sqlstrjo = " INNER Join dbo.TCOD_MAP TCOD_MAP_1 ON { fn LEFT(dbo.STUF_DEF.N_FANI, " + RST.Select(f => f.SIZEF).FirstOrDefault() + ") } = TCOD_MAP_1.MPCODE";
                sqlstrwe = " WHERE (TCOD_MAP_1.MPP = 1)";
                var loopTo = RST.Count;//rst.RecordCount
                for (i = 1; i < loopTo; i++)
                {
                    itwo++; // + 1
                    // RST.MoveNext(); → RST[i] //بررسی کن که از صفر شروع میشه ردیف های این آر اس تی یا نه 
                    //////sqlstr = sqlstr + ", SUBSTRING(N_FANI," + (FR + 1) + " ," + RST.Fields("sizef") + ") AS col" + i;
                    sqlstr = sqlstr + ", SUBSTRING(N_FANI," + (FR + 1) + " ," + RST[i].SIZEF + ") AS col" + itwo;
                    sqlstrmp = sqlstrmp + ", TCOD_MAP_" + itwo + ".MPNAME AS colN" + itwo;
                    sqlstrjo = sqlstrjo + " INNER Join dbo.TCOD_MAP TCOD_MAP_" + itwo + " ON  SUBSTRING(N_FANI," + (FR + 1) + " ," + RST[i].SIZEF + ") = TCOD_MAP_" + itwo + ".MPCODE";
                    sqlstrwe = sqlstrwe + " AND (TCOD_MAP_" + itwo + ".MPP = " + itwo + ")";
                    FR = (int)(FR + RST[i].SIZEF);
                }
                for (i = RST.Count + 1; i <= 9; i++)
                {
                    sqlstr = sqlstr + ", 1 AS col" + i;
                    sqlstrmp = sqlstrmp + ", ' '  AS colN" + i;
                }
            }
            else
            {
                sqlstr = "1 AS col1 ,1 AS col2 ,1 AS col3 ,1 AS col4 ,1 AS col5 ,1 AS col6 ,1 AS col7 ,1 AS col8 ,1 AS col9";
                sqlstrmp = " ' ' AS coln1 ,' ' AS coln2 ,' ' AS coln3 ,' ' AS coln4 ,' ' AS coln5 ,' ' AS coln6 ,' ' AS coln7 ,' ' AS coln8 ,' ' AS coln9";
            }


            try
            {
                dbms.DoExecuteSQL("create  view   dbo.stuf_def_nfani as  SELECT     dbo.STUF_DEF.CODE, dbo.STUF_DEF.NAME, dbo.STUF_DEF.N_FANI, " + sqlstr + sqlstrmp + " FROM     dbo.STUF_DEF  " + sqlstrjo + sqlstrwe);
            }
            catch (Exception)
            {
                dbms.DoExecuteSQL("alter   view   dbo.stuf_def_nfani as  SELECT     dbo.STUF_DEF.CODE, dbo.STUF_DEF.NAME, dbo.STUF_DEF.N_FANI, " + sqlstr + sqlstrmp + " FROM     dbo.STUF_DEF  " + sqlstrjo + sqlstrwe);
            }


        }
        public static double GetMHavlF(string HES)
        {
            double tempGetMHavlF = 0;
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.Visit_route INNER JOIN    dbo.CUST_HESAB ON dbo.Visit_route.ROUTE_NAME = dbo.CUST_HESAB.ROUTE_NAME INNER JOIN     dbo.HEAD_LST INNER JOIN    dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG ON    dbo.CUST_HESAB.HES = dbo.HEAD_LST.CUST_NO WHERE     (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.TAMIR = 0) AND (dbo.Visit_route.HES = N'" + HES + "')").FirstOrDefault();

            if (ReferenceEquals(rst, null))
            {
                tempGetMHavlF = 0;
            }
            else
            {
                tempGetMHavlF = (double)rst;
            }
            return tempGetMHavlF;
        }
        public static double GetMPishfF(string HES)
        {
            double tempGetMPishfF = 0;
            var rst = dbms.DoGetDataSQL<double?>("SELECT SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM  dbo.Visit_route INNER JOIN     dbo.CUST_HESAB ON dbo.Visit_route.ROUTE_NAME = dbo.CUST_HESAB.ROUTE_NAME INNER JOIN        dbo.HEAD_LST INNER JOIN              dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG ON     dbo.CUST_HESAB.HES = dbo.HEAD_LST.CUST_NO WHERE     (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.TAMIR <> 2) AND (dbo.HEAD_LST.TAMIR <> 3) AND (dbo.HEAD_LST.SGN1 = 1) AND  (dbo.Visit_route.HES = N'" + HES + "')").FirstOrDefault();
            // DEBUG.PRINT "SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.Visit_route INNER JOIN     dbo.CUST_HESAB ON dbo.Visit_route.ROUTE_NAME = dbo.CUST_HESAB.ROUTE_NAME INNER JOIN        dbo.HEAD_LST INNER JOIN              dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG ON     dbo.CUST_HESAB.HES = dbo.HEAD_LST.CUST_NO WHERE     (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.TAMIR <> 2) AND (dbo.HEAD_LST.TAMIR <> 3) AND (dbo.HEAD_LST.SGN1 = 1) AND  (dbo.Visit_route.HES = N'" & HES & "')"
            if (ReferenceEquals(rst, null))
            {
                tempGetMPishfF = 0;
            }
            else
            {
                tempGetMPishfF = (double)rst;
            }
            return tempGetMPishfF;
        }
        public static int ChecketebarSSMF(string HES, double ETB, long pnum)
        {
            int tempChecketebarSSMF = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            double JPTAEED = 0;
            double JHBNSHE = 0;

            CHK = 0;
            JPTAEED = 0;
            JHBNSHE = 0;
            MAND = GetMhesabF(HES);
            //جمع پيش فاکتورهاي تاييد شده کنسل نشده و حواله نشده
            JPTAEED = GetMPishfF(HES) + JAMinpishfac(pnum);
            //جمع حواله هاي بارگيري نشده
            JHBNSHE = GetMHavlF(HES);
            if (ETB - MAND - JPTAEED - JHBNSHE >= 0)
            {
                tempChecketebarSSMF = 1;//true
            }
            else
            {
                tempChecketebarSSMF = 0;//false
                //DoCmd.OpenForm "mesageform", , , , , acDialog, "مانده حساب : " & Format(MAND, "#,###") & "  مانده اسناد : " & Format(CHK, "#,###") & "  كل اعتبار : " & Format(TOPETEB, "#,###")
            }

            return tempChecketebarSSMF;
        }
        public static int ChekBrgashti(string HES)
        {
            int tempChekBrgashti = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPETEB = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            var rst = dbms.DoGetDataSQL<double?>("SELECT    n_seri FROM         dbo.pay_getd WHERE     (dbo.pay_getd.vaz = 5 or dbo.pay_getd.vaz = 6) and (dbo.pay_getd.CUST_NO = '" + HES + "')").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempChekBrgashti = 1;//true
            }
            else
            {
                tempChekBrgashti = 0;//false
            }
            return tempChekBrgashti;
        }
        public static int FactCountMand(string HES, double VL)
        {
            int FactCountMandRet = 0;
            double MAN;
            int CF;
            CF = 0;
            var rst = dbms.DoGetDataSQL<FACT_1>("SELECT     SUM(dbo.DEED_DTL.BES) AS MBES,SUM(dbo.DEED_DTL.BED) AS MBED FROM   dbo.DEED_DTL  WHERE     (dbo.DEED_DTL.HES = '" + HES + "')").FirstOrDefault();
            if (ReferenceEquals(rst.MBES, null) || (double)rst.MBES == 0)
            {
                FactCountMandRet = 0;
            }
            else if (rst.MBES - rst.MBED >= 0)
            {
                FactCountMandRet = 0;
            }
            else
            {
                MAN = (double)rst.MBES;
                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "MOIN" + Baseknow.USERCOD + "')   DROP TABLE " + "MOIN" + Baseknow.USERCOD);
                dbms.DoExecuteSQL("SELECT    N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4,TAFZILN,HES INTO dbo.MOIN" + Baseknow.USERCOD + " FROM         dbo.QDAFTARTAFZIL2_H(1 , 99999999, '" + HES + "') QDAFTARTAFZIL2_H ORDER BY DATE_S, BED DESC");

                int II = 0;
                var RST2 = dbms.DoGetDataSQL<FACT_2>($"SELECT BED,NO_S FROM MOIN{Baseknow.USERCOD}").ToList();
                for (int imain = 0; imain <= RST2.Count - 1;)  ////////while (!RST2.EOF())
                {
                    MAN = (double)(MAN - RST2[imain].BED);
                    if (MAN <= 0d)
                    {
                        for (int i = imain; i <= RST2.Count - 1;)
                        {
                            if (RST2[i].NO_S == 2 && RST2[i].BED > 1000)
                            {
                                CF = CF + 1;
                            }
                            i++;//RST2.MoveNext();
                            II = i;
                        }
                        imain = II;
                    }
                    else
                    {
                        imain++; //RST2.MoveNext();
                    }
                }
                FactCountMandRet = CF;
            }

            return FactCountMandRet;
        }
        public static int FactCount(string HES, double VL, ref int FC)
        {
            int tempFactCount = 0;

            FC = FactCountMand(HES, VL);
            if (FC > VL)
            {
                tempFactCount = 0;//false
            }
            else
            {
                tempFactCount = 1;//true
            }
            return tempFactCount;
        }
        public static int MandLes10(string HES, long dt, double VL, double VL2)
        {
            int tempMandLes10 = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPETEB = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            double JPTAEED = 0;
            double JHBNSHE = 0;
            CHK = 0;
            TOPETEB = 0;
            JPTAEED = 0;
            JHBNSHE = 0;
            MAND = Convert.ToDouble(GetMhesabDT(HES, Convert.ToInt64(Convert.ToInt64(dt.ToString().Substring(0, 6)) - 1 + "31")));
            if (MAND <= VL && MAND > VL2 && MAND > 0)
            {
                dt = Convert.ToInt64(Convert.ToInt64(dt.ToString().Substring(0, 6)) + SimulateRight.Right("00" + VL2, 2));
                var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.DEED_DTL.BES) AS MAND FROM         dbo.DEED_DTL INNER JOIN   dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S WHERE     (dbo.DEED_HED.NO_S <> 2) AND (dbo.DEED_DTL.HES = '" + HES + "') AND (dbo.DEED_HED.DATE_S <= " + dt + ") AND (dbo.DEED_HED.DATE_S > " + Convert.ToInt64(Convert.ToInt64(dt.ToString().Substring(0, 6)) - 1) + "31" + ")").FirstOrDefault();
                if (ReferenceEquals(rst, null))
                {
                    /////if (FARSIDATE(DateTime) >= dt)
                    if (Convert.ToInt64(Tarikh.GoGetPersianDate(false)) >= dt)
                    {
                        tempMandLes10 = 0;//false
                    }
                    else
                    {
                        tempMandLes10 = 1;//true
                    }
                }
                else
                {
                    if (MAND - rst <= 0)
                    {
                        tempMandLes10 = 1;//true
                    }
                    else
                    {
                        tempMandLes10 = 0;//false
                    }
                }
            }
            else
            {
                tempMandLes10 = 1;//true
            }
            return tempMandLes10;
        }
        public static double GetMhesabDT(string HES, long dt)
        {
            double tempGetMhesabDT = 0;
            var rst = dbms.DoGetDataSQL<double?>("SELECT    sum(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) AS MAND FROM  dbo.DEED_DTL INNER JOIN  dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S WHERE    (HES = '" + HES + "') AND (dbo.DEED_HED.DATE_S <= " + dt + ")").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetMhesabDT = 0;
            }
            else
            {
                tempGetMhesabDT = (double)rst;
            }
            return tempGetMhesabDT;
        }
        public static int MandTop10(string HES, long dt, double VL, double VL2)
        {
            int tempMandTop10 = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPETEB = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            double JPTAEED = 0;
            double JHBNSHE = 0;
            CHK = 0;
            TOPETEB = 0;
            JPTAEED = 0;
            JHBNSHE = 0;
            //MAND = GetMhesabDT(HES, Conversions.ToDouble(Strings.Left(dt.ToString(), 6)) - 1d + "31");//From Pro VB Convertor
            MAND = Convert.ToDouble(GetMhesabDT(HES, Convert.ToInt64(Convert.ToInt64(dt.ToString().Substring(0, 6)) - 1 + "31")));
            if (MAND >= VL && MAND > 0)
            {
                //dt = Conversions.ToLong(Strings.Left(dt.ToString(), 6) + Strings.Right("00" + VL2, 2));//From Pro VB Convertor

                dt = Convert.ToInt64(dt.ToString().Substring(0, 6) + SimulateRight.Right("00" + VL2, 2));
                var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.DEED_DTL.BES) AS MAND FROM         dbo.DEED_DTL INNER JOIN  dbo.DEED_HED ON dbo.DEED_DTL.N_S = dbo.DEED_HED.N_S WHERE     (dbo.DEED_HED.NO_S <> 2) AND (dbo.DEED_DTL.HES = '" + HES + "') AND (dbo.DEED_HED.DATE_S <= " + dt + ")").FirstOrDefault();
                if (ReferenceEquals(rst, null))
                {
                    tempMandTop10 = 0;//false
                }
                else
                {
                    if (MAND - rst <= 0)
                    {
                        tempMandTop10 = 1;//true
                    }
                    else
                    {
                        tempMandTop10 = 0;//false
                    }
                }
            }
            else
            {
                tempMandTop10 = 1;//true
            }
            return tempMandTop10;
        }
        //----------------------------------------------------------------------------------------
        //	Copyright © 2003 - 2012 Tangible Software Solutions Inc.
        //	This class can be used by anyone provided that the copyright notice remains intact.
        //
        //	This class simulates the behavior of the classic VB 'Right' function.
        //----------------------------------------------------------------------------------------
        public static class SimulateRight
        {
            public static string Right(string expression, int length)
            {
                if (string.IsNullOrEmpty(expression))
                    return string.Empty;
                else if (length >= expression.Length)
                    return expression;
                else
                    return expression.Substring(expression.Length - length);
            }
        }
        public static int ChecketebarSSM(string HES, double ETB, long pnum)
        {
            int tempChecketebarSSM = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            double JPTAEED = 0;
            double JHBNSHE = 0;
            CHK = 0;
            JPTAEED = 0;
            JHBNSHE = 0;

            MAND = GetMhesab(HES);
            //جمع پيش فاکتورهاي تاييد شده کنسل نشده و حواله نشده
            JPTAEED = GetMPishf(HES) + JAMinpishfac(pnum);
            //جمع حواله هاي بارگيري نشده
            JHBNSHE = GetMHavl(HES);
            if (ETB - MAND - JPTAEED - JHBNSHE >= 0)
            {
                tempChecketebarSSM = 1;//true
            }
            else
            {
                tempChecketebarSSM = 0;//false
                                       //DoCmd.OpenForm "mesageform", , , , , acDialog, "مانده حساب : " & Format(MAND, "#,###") & "  مانده اسناد : " & Format(CHK, "#,###") & "  كل اعتبار : " & Format(TOPETEB, "#,###")
            }
            return tempChecketebarSSM;
        }
        public static int ChecketebarASSSM(string HES, double ETB, long pnum)
        {
            int tempChecketebarASSSM = 0;
            double MAND = 0;
            double CHK = 0;
            double TOPETEB = 0;
            double TOPASN = 0;
            bool ENDIS = false;
            double JPTAEED = 0;
            double JHBNSHE = 0;
            CHK = 0;
            TOPETEB = 0;
            JPTAEED = 0;
            JHBNSHE = 0;
            MAND = GetMhesab(HES);
            //جمع پيش فاکتورهاي تاييد شده کنسل نشده و حواله نشده
            JPTAEED = GetMPishf(HES) + JAMinpishfac(pnum);
            //جمع حواله هاي بارگيري نشده
            JHBNSHE = GetMHavl(HES);
            CHK = GetMAsnad(HES);

            if (ETB - MAND - JPTAEED - JHBNSHE - CHK >= 0)
            {
                tempChecketebarASSSM = 1;//true
            }
            else
            {
                tempChecketebarASSSM = 0;//false
                                         //DoCmd.OpenForm "mesageform", , , , , acDialog, "مانده حساب : " & Format(MAND, "#,###") & "  مانده اسناد : " & Format(CHK, "#,###") & "  كل اعتبار : " & Format(TOPETEB, "#,###")
            }

            return tempChecketebarASSSM;
        }
        public static string GetGradeName(int GID)
        {
            string tempGetGradeName = null;
            var rst = dbms.DoGetDataSQL<GTGRDNAME>("SELECT     GID, GNAME FROM dbo.GRADE_RANGE WHERE     (GID = " + GID + ")").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetGradeName = 0.ToString();
            }
            else if (rst.GID == 0)
            {
                tempGetGradeName = 0.ToString();
            }
            else
            {
                tempGetGradeName = rst.GNAME;
            }
            return tempGetGradeName;
        }
        public static int GetGrade(string HES)
        {
            int tempGetGrade = 0;
            var rst = dbms.DoGetDataSQL<int?>("SELECT GRADEID FROM AZAE WHERE HES = '" + HES + "'").FirstOrDefault();
            if (rst == null || rst == 0)
            {
                tempGetGrade = -1;
            }
            else
            {
                tempGetGrade = (int)rst;
            }
            return tempGetGrade;
        }
        public static double GetMhesabF(string HES)
        {
            double tempGetMhesabF = 0;
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) AS Expr1 FROM         dbo.DEED_DTL INNER JOIN  dbo.CUST_HESAB ON dbo.DEED_DTL.HES = dbo.CUST_HESAB.hes INNER JOIN dbo.Visit_route ON dbo.CUST_HESAB.ROUTE_NAME = dbo.Visit_route.ROUTE_NAME WHERE     (dbo.Visit_route.HES = N'" + HES + "')").FirstOrDefault();
            //  ' DEBUG.PRINT "SELECT     SUM(dbo.DEED_DTL.BED - dbo.DEED_DTL.BES) AS Expr1 FROM         dbo.DEED_DTL INNER JOIN  dbo.CUST_HESAB ON dbo.DEED_DTL.HES = dbo.CUST_HESAB.hes INNER JOIN dbo.Visit_route ON dbo.CUST_HESAB.ROUTE_NAME = dbo.Visit_route.ROUTE_NAME WHERE     (dbo.Visit_route.HES = N'" & HES & "')"
            ///////if (rst.RecordCount == 0 || IsNull(rst.Fields[0]))
            if (ReferenceEquals(rst, null))
            {
                tempGetMhesabF = 0;
            }
            else
            {
                tempGetMhesabF = (double)rst;
            }
            return tempGetMhesabF;
        }
        public static double GetMhesab(string HES)
        {
            double tempGetMhesab = 0;
            //rst.Open "SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE HES = '" + HES + "'";
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(BED - BES) AS MAN FROM dbo.DEED_DTL WHERE HES = '" + HES + "'").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetMhesab = 0;
            }
            else
            {
                tempGetMhesab = (double)rst;
            }
            return tempGetMhesab;
        }
        public static double GetMAsnad(string HES)
        {
            double tempGetMAsnad = 0;
            //rst.Open "SELECT     SUM(MABL) AS smab FROM dbo.PAY_GETD WHERE     ((N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (CUST_NO = N'" + HES + "') AND (N_KOL = " + Forms["baseknow"]["BANKHA"] + " OR  N_KOL IS NULL)) OR  ((N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (CUST_NO = N'" + HES + "') AND (DATE_S > " + FARSIDATE(DateTime.Now) + "))";
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(MABL) AS smab FROM dbo.PAY_GETD WHERE     ((N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (CUST_NO = N'" + HES + "') AND (N_KOL = " + Baseknow.BANKHA + " OR  N_KOL IS NULL)) OR  ((N_KOL3 IS NULL) AND (N_KOL2 IS NULL) AND (CUST_NO = N'" + HES + "') AND (DATE_S > " + Tarikh.FullCurrentDate + "))").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetMAsnad = 0;
            }
            else
            {
                tempGetMAsnad = (double)rst;
            }
            return tempGetMAsnad;
        }
        public static double GetMHavl(string HES)
        {
            double tempGetMHavl = 0;
            //rst.Open "SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE       (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.TAMIR = 0) AND  (dbo.HEAD_LST.CUST_NO = '" + HES + "')";
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE       (dbo.HEAD_LST.TAG = 2) AND (dbo.HEAD_LST.TAMIR = 0) AND  (dbo.HEAD_LST.CUST_NO = '" + HES + "')").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetMHavl = 0;
            }
            else
            {
                tempGetMHavl = (double)rst;
            }
            return tempGetMHavl;
        }
        public static double GetMPishf(string HES)
        {
            double tempGetMPishf = 0;
            //rst.Open "SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE      (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.TAMIR <> 2 AND dbo.HEAD_LST.TAMIR <> 3) AND  (dbo.HEAD_LST.SGN1 = 1) AND  (dbo.HEAD_LST.CUST_NO = '" + HES + "')";
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS GMAB FROM         dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE      (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.TAMIR <> 2 AND dbo.HEAD_LST.TAMIR <> 3) AND  (dbo.HEAD_LST.SGN1 = 1) AND  (dbo.HEAD_LST.CUST_NO = '" + HES + "')").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGetMPishf = 0;
            }
            else
            {
                tempGetMPishf = (double)rst;
            }
            return tempGetMPishf;
        }
        public static double JAMinpishfac(long pnum)
        {
            double tempJAMinpishfac = 0;
            //rst.Open "SELECT     SUM(dbo.INVO_LST.MABL_K) AS MAB, SUM(dbo.INVO_LST.N_MOIN) AS takh, SUM(dbo.INVO_LST.IMBAA) AS arzesh,   dbo.HEAD_LST.MABL_HAZ,   dbo.HEAD_LST.sgn1  FROM         dbo.HEAD_LST INNER JOIN    dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE (dbo.HEAD_LST.TAG = 20) And (dbo.HEAD_LST.NUMBER = " + pnum + ") GROUP BY dbo.HEAD_LST.MABL_HAZ,dbo.HEAD_LST.sgn1";
            var rst = dbms.DoGetDataSQL<LOGETEB_JAMinpish>("SELECT     SUM(dbo.INVO_LST.MABL_K) AS MAB, SUM(dbo.INVO_LST.N_MOIN) AS takh, SUM(dbo.INVO_LST.IMBAA) AS arzesh,   dbo.HEAD_LST.MABL_HAZ,   dbo.HEAD_LST.sgn1  FROM         dbo.HEAD_LST INNER JOIN    dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE (dbo.HEAD_LST.TAG = 20) And (dbo.HEAD_LST.NUMBER = " + pnum + ") GROUP BY dbo.HEAD_LST.MABL_HAZ,dbo.HEAD_LST.sgn1").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempJAMinpishfac = 0;
            }
            else
            {
                if (Convert.ToBoolean(rst.SGN1))
                {
                    tempJAMinpishfac = 0;
                }
                else
                {
                    tempJAMinpishfac = Convert.ToDouble(rst.MAB - rst.takh + rst.arzesh + rst.MABL_HAZ);
                }
            }
            return tempJAMinpishfac;
        }
        public static void logetebar(string HES, long num, int tg, double ETB, int GRD, string PAYAM, string GSM)
        {
            ////rst.Open "logetebar";

            dbms.DoExecuteSQL($@"INSERT INTO dbo.logetebar
                                                  ( hes,
                                                  grade,
                                                  VALUE,
                                                  GSNAME,
                                                  SAGHFMAND,
                                                  SAGHFASNAD,
                                                  HAVBAR,
                                                  PISHFOK,
                                                  NUMBER,
                                                  TG,
                                                  DTTIM,
                                                  FDT,
                                                  PAYAM,
                                                  USERCO )
                                              VALUES
                                              (N'{HES}',
                                                  {GRD},         
                                                  {ETB},      
                                                  N'{GSM}',       
                                                  {GetMhesab(HES)},      
                                                  {GetMAsnad(HES)},      
                                                  {GetMHavl(HES)},     
                                                  {GetMPishf(HES) + JAMinpishfac(num)},     
                                                  {num},      
                                                  {tg},         
                                                  GETDATE(), 
                                                  {Tarikh.FullCurrentDate},     
                                                  N'{PAYAM}',      
                                                  {Baseknow.USERCOD}       
                                                  )");
            ////rst.AddNew;
            ////rst.Fields["hes"] = HES;
            ////rst.Fields["GSNAME"] = GSM;
            ////rst.Fields["grade"] = GRD;
            ////rst.Fields["VALUE"] = ETB;
            ////rst.Fields["SAGHFMAND"] = GetMhesab(HES);
            ////rst.Fields["SAGHFASNAD"] = GetMAsnad(HES);
            ////rst.Fields["HAVBAR"] = GetMHavl(HES);
            ////rst.Fields["PISHFOK"] = GetMPishf(HES) + JAMinpishfac(num);
            ////rst.Fields["NUMBER"] = num;
            ////rst.Fields["TG"] = tg;
            ////rst.Fields["DTTIM"] = DateTime.Now;
            ////rst.Fields["FDT"] = FARSIDATE(DateTime.Now);
            ////rst.Fields["PAYAM"] = PAYAM;
            ////rst.Fields["USERCO"] = Forms["baseknow"]["USERCOD"];
            ////rst.update;
        }
        public static string GETHESNAME(string HES)
        {
            string tempGETHESNAME = null;
            var RRST = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAME FROM dbo.CUST_HESAB WHERE hes = N'{HES}'").FirstOrDefault();
            //var RRST = dbms.DoGetDataSQL<string>($"SELECT  NAME FROM TDETA_HES WHERE RTRIM(CAST(N_KOL AS NVARCHAR))+'-'+RTRIM(CAST(NUMBER AS NVARCHAR))+'-'+RTRIM(CAST(TNUMBER AS NVARCHAR)) = N'{HES}'").FirstOrDefault();

            if (!ReferenceEquals(RRST, null))
            {

                tempGETHESNAME = RRST;
            }
            else
            {
                tempGETHESNAME = "";
            }
            return tempGETHESNAME;
        }
        public static int ChekEnheSar(long num, int tgg)
        {
            int ChekEnheSarRet = default;
            double CHK;
            double NOTCH;
            double MABCHKR;
            double MANDOLD;
            double MABCHK;
            bool ENDIS;
            int DIFFU;
            bool MATCHTO;
            bool ONEONLY;
            string PYT;
            CR_NFANI();
            var RST = dbms.DoGetDataSQL<CHKENHES>("SELECT     dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.CUST_NO, dbo.INVO_LST.CODE, dbo.INVO_LST.MEGH,  dbo.INVO_LST.MEGHk, dbo.INVO_LST.MABL, dbo.INVO_LST.MABL_K, dbo.CUST_HESAB.OSTANID, dbo.CUST_HESAB.SHAHRID,   dbo.stuf_def_nfani.N_FANI, dbo.stuf_def_nfani.col1, dbo.stuf_def_nfani.col2, dbo.stuf_def_nfani.col3, dbo.stuf_def_nfani.col4, dbo.stuf_def_nfani.col5, dbo.stuf_def_nfani.col6, dbo.stuf_def_nfani.col7, dbo.stuf_def_nfani.col8, dbo.stuf_def_nfani.col9, dbo.stuf_def_nfani.colN1, dbo.stuf_def_nfani.colN2,  dbo.stuf_def_nfani.colN3, dbo.stuf_def_nfani.colN4, dbo.stuf_def_nfani.colN5, dbo.stuf_def_nfani.colN6, dbo.stuf_def_nfani.colN7, dbo.stuf_def_nfani.colN8 , dbo.stuf_def_nfani.colN9 FROM         dbo.HEAD_LST INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG INNER JOIN  dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes INNER JOIN dbo.stuf_def_nfani ON dbo.INVO_LST.CODE = dbo.stuf_def_nfani.CODE WHERE     (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.NUMBER = " + num + ")").ToList();
            if (!(RST.Count == 0 || ReferenceEquals(RST.FirstOrDefault(), null)))
            {
                //آیا شهر مشتری خالی هست یا نه ؟ گر خالی هست خطا بده
                if (ReferenceEquals(RST.Select(o => o.OSTANID).FirstOrDefault(), null) || ReferenceEquals(RST.Select(o => o.SHAHRID).FirstOrDefault(), null))
                {
                    if (LETSGO("moshtari") || LETSGO("custdef"))
                    {
                        //// DoCmd.OpenForm "", , , "hes ='" & RST.Fields("cust_no") & "'", , acDialog
                        ////MessageBox.Show("اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد");
                        Msgwin msgwin = new Msgwin(false, "اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد"); msgwin.ShowDialog();
                    }
                    else
                    {
                        ////MessageBox.Show("اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد");
                        Msgwin msgwin = new Msgwin(false, "اطلاعات مشتري ناقص است.استان و شهر براي اين مشتري مشخص نشده است .لطفا آن را تکميل کنيد"); msgwin.ShowDialog();
                    }
                    return ChekEnheSarRet;
                }

                //آیا شهر مشتری جزو شهر های انحصار شده هست یا نه ؟ در محدوده تاریخی و چک کن نام درخواست کنند با انحصار شده یکی نباشه
                var RST2 = dbms.DoGetDataSQL<SHAHR>("SELECT   EN_CUST_CO, EN_CITY, EN_IYALAT, EN_COUNTRY FROM dbo.ENHESAR_MOSHTARI WHERE     (EN_CUST_CO <> N'" + RST.Select(c => c.CUST_NO).FirstOrDefault() + "') AND (EN_CITY =" + RST.Select(c => c.SHAHRID).FirstOrDefault() + ") AND (EN_STDATE <= " + RST.Select(c => c.DATE_N).FirstOrDefault() + ") AND (EN_ENDEN >= " + RST.Select(c => c.DATE_N).FirstOrDefault() + " OR EN_ENDEN IS NULL)").ToList();
                if (!(RST2.Count == 0 || ReferenceEquals(RST2.Select(e => e.EN_CITY).FirstOrDefault(), null)))   // اگر انحصار کننده خود مشتری باشد که صاحب پیش فاکتور است نیا داخل این شرط واگر نه ↓
                {
                    /////////var RST21 = ("SELECT dbo.ENHESAR_MOSHTARI.EN_CUST_CO, dbo.ENHESAR_MOSHTARI.EN_CITY, dbo.ENHESAR_MOSHTARI.EN_IYALAT, dbo.ENHESAR_MOSHTARI.EN_COUNTRY, dbo.ENHESAR_MOSHTARI.EN_STDATE, dbo.ENHESAR_MOSHTARI.EN_ENDEN, dbo.ENHESAR_KALA.EN_KALA_COD, dbo.ENHESAR_KALA.EN_KALA_GR, dbo.ENHESAR_KALA.EN_GRCODE1, dbo.ENHESAR_KALA.EN_GRCODE2, dbo.ENHESAR_KALA.EN_GRCODE3, dbo.ENHESAR_KALA.EN_GRCODE4, dbo.ENHESAR_KALA.EN_GRCODE5, dbo.ENHESAR_KALA.EN_GRCODE6, dbo.ENHESAR_KALA.EN_GRCODE7, dbo.ENHESAR_KALA.EN_GRCODE8, dbo.ENHESAR_KALA.EN_GRCODE9 , dbo.ENHESAR_KALA.EN_GRCODE10, dbo.ENHESAR_KALA.USERID, dbo.ENHESAR_KALA.EN_CDATE FROM  dbo.ENHESAR_MOSHTARI INNER JOIN dbo.ENHESAR_KALA ON dbo.ENHESAR_MOSHTARI.EN_CUST_CO = dbo.ENHESAR_KALA.EN_CUST_CO  WHERE     (dbo.ENHESAR_MOSHTARI.EN_CUST_CO <> N'" + RST.Select(r => r.CUST_NO).FirstOrDefault() + "') AND (dbo.ENHESAR_MOSHTARI.EN_CITY =" + RST.Fields("shahrid") + ") AND " + " (EN_STDATE <= " + RST.Fields("date_n") + ") AND (EN_ENDEN >= " + RST.Fields("date_n") + " OR EN_ENDEN IS NULL)");
                    var RST21 = dbms.DoGetDataSQL<ENHESAR_A1>("SELECT dbo.ENHESAR_MOSHTARI.EN_CUST_CO, dbo.ENHESAR_MOSHTARI.EN_CITY, dbo.ENHESAR_MOSHTARI.EN_IYALAT, dbo.ENHESAR_MOSHTARI.EN_COUNTRY, dbo.ENHESAR_MOSHTARI.EN_STDATE, dbo.ENHESAR_MOSHTARI.EN_ENDEN, dbo.ENHESAR_KALA.EN_KALA_COD, dbo.ENHESAR_KALA.EN_KALA_GR, dbo.ENHESAR_KALA.EN_GRCODE1, dbo.ENHESAR_KALA.EN_GRCODE2, dbo.ENHESAR_KALA.EN_GRCODE3, dbo.ENHESAR_KALA.EN_GRCODE4, dbo.ENHESAR_KALA.EN_GRCODE5, dbo.ENHESAR_KALA.EN_GRCODE6, dbo.ENHESAR_KALA.EN_GRCODE7, dbo.ENHESAR_KALA.EN_GRCODE8, dbo.ENHESAR_KALA.EN_GRCODE9 , dbo.ENHESAR_KALA.EN_GRCODE10, dbo.ENHESAR_KALA.USERID, dbo.ENHESAR_KALA.EN_CDATE FROM  dbo.ENHESAR_MOSHTARI INNER JOIN dbo.ENHESAR_KALA ON dbo.ENHESAR_MOSHTARI.EN_CUST_CO = dbo.ENHESAR_KALA.EN_CUST_CO  WHERE     (dbo.ENHESAR_MOSHTARI.EN_CUST_CO <> N'" + RST.Select(r => r.CUST_NO).FirstOrDefault() + "') AND (dbo.ENHESAR_MOSHTARI.EN_CITY =" + RST.Select(r => r.SHAHRID).FirstOrDefault() + ") AND " + " (EN_STDATE <= " + RST.Select(r => r.DATE_N).FirstOrDefault() + ") AND (EN_ENDEN >= " + RST.Select(r => r.DATE_N).FirstOrDefault() + " OR EN_ENDEN IS NULL)").ToList();
                    foreach (var RST21_Item in RST21)
                    {
                        if (!ReferenceEquals(RST21_Item.EN_KALA_COD, null))
                        {
                            //while (!RST.EOF)
                            foreach (var RST_MainItm in RST)//RST.MoveFirst();
                            {
                                if (RST_MainItm.CODE == RST21_Item.EN_KALA_COD)
                                {
                                    //DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, );
                                    Msgwin msgwin = new Msgwin(false, "اين کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST21_Item.EN_CUST_CO) + '\r' + "نام کالا:" + GETKALANAME(Convert.ToDouble(RST_MainItm.CODE)));
                                    msgwin.ShowDialog();
                                    ChekEnheSarRet = Conversions.ToInteger(true);
                                }
                                //RST.MoveNext();
                            }
                            //RST.MoveFirst();
                        }
                        else
                        {
                            foreach (var RST_MainItm in RST)
                            {
                                MATCHTO = true;  // تطبيق داردبا گروه کالا
                                ONEONLY = false; // حداقل با يک گروه تطبيق دارد
                                PYT = "";
                                if (!string.IsNullOrEmpty(RST21_Item.EN_GRCODE1) && Convert.ToDouble(RST21_Item.EN_GRCODE1) == -1)   // همه کالاها
                                {
                                    PYT = " همه کالاها ";
                                    MATCHTO = true;
                                    ONEONLY = true;
                                }
                                else
                                {
                                    for (int i = 1; i <= 9; i++)
                                    {
                                        var RST21_Fields_EN_GRCODE = RST21_Item.GetType().GetProperty("EN_GRCODE" + i).GetValue(RST21_Item);

                                        var RST_Fields_COL = RST_MainItm.GetType().GetProperty("COL" + i).GetValue(RST_MainItm);

                                        if (!ReferenceEquals(RST21_Fields_EN_GRCODE, null))
                                        {
                                            if (RST21_Fields_EN_GRCODE != RST_Fields_COL)
                                            {
                                                MATCHTO = false;
                                            }
                                            else
                                            {
                                                //PYT = PYT + " - " + RST.Fields("COLN" + i);
                                                PYT = PYT + " - " + RST_MainItm.GetType().GetProperty("COLN" + i).GetValue(RST_MainItm);
                                            }
                                            ONEONLY = true;
                                        }

                                        //////if (!ISNULL(RST21_Item.Fields("EN_GRCODE" + i)))
                                        //////{
                                        //////    if (RST21.Fields("EN_GRCODE" + i) != RST.Fields("COL" + i))
                                        //////    {
                                        //////        MATCHTO = false;
                                        //////    }
                                        //////    else
                                        //////    {
                                        //////        PYT = PYT + " - " + RST.Fields("COLN" + i);
                                        //////    }

                                        //////    ONEONLY = true;
                                        //////}
                                    }
                                }

                                if (MATCHTO && ONEONLY)
                                {
                                    Msgwin msgwin = new Msgwin(false, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST21_Item.EN_CUST_CO) + '\r' + ":گروه کالا" + PYT); msgwin.ShowDialog();
                                    //DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST21.Fields("EN_CUST_CO")) + '\r' + ":گروه کالا" + PYT);
                                    ChekEnheSarRet = Conversions.ToInteger(true);
                                }

                                //RST.MoveNext();
                            }

                            //RST.MoveFirst();
                        }

                        //RST21.MoveNext();
                    }
                }
                else
                {
                    var RST22 = dbms.DoGetDataSQL<ENSHESARMOSHTARI>("SELECT   EN_CUST_CO, EN_CITY, EN_IYALAT, EN_COUNTRY FROM dbo.ENHESAR_MOSHTARI WHERE     (EN_CUST_CO <> N'" + RST.Select(x => x.CUST_NO).FirstOrDefault() + "') AND (EN_IYALAT =" + RST.Select(x => x.OSTANID).FirstOrDefault() + ") AND (EN_STDATE <= " + RST.Select(x => x.DATE_N).FirstOrDefault() + ") AND (EN_ENDEN >= " + RST.Select(x => x.DATE_N).FirstOrDefault() + " OR EN_ENDEN IS NULL)").FirstOrDefault();
                    //var RST22 = dbms.DoGetDataSQL<ENSHESARMOSHTARI>("SELECT   EN_CUST_CO, EN_CITY, EN_IYALAT, EN_COUNTRY FROM dbo.ENHESAR_MOSHTARI WHERE     (EN_CUST_CO <> N'" + RST.Select(x=>x.CUST_NO).FirstOrDefault() + "') AND (EN_IYALAT =" + RST.Fields("OSTANID") + ") AND (EN_STDATE <= " + RST.Fields("date_n") + ") AND (EN_ENDEN >= " + RST.Fields("date_n") + " OR EN_ENDEN IS NULL)");
                    if (!(ReferenceEquals(RST22, null) || ReferenceEquals(RST22.EN_IYALAT, null)))   // اگر انحصاراستان هست
                    {
                        var RST22_2 = dbms.DoGetDataSQL<ENHESAR_A1>("SELECT dbo.ENHESAR_MOSHTARI.EN_CUST_CO, dbo.ENHESAR_MOSHTARI.EN_CITY, dbo.ENHESAR_MOSHTARI.EN_IYALAT, dbo.ENHESAR_MOSHTARI.EN_COUNTRY, dbo.ENHESAR_MOSHTARI.EN_STDATE, dbo.ENHESAR_MOSHTARI.EN_ENDEN, dbo.ENHESAR_KALA.EN_KALA_COD, dbo.ENHESAR_KALA.EN_KALA_GR, dbo.ENHESAR_KALA.EN_GRCODE1, dbo.ENHESAR_KALA.EN_GRCODE2, dbo.ENHESAR_KALA.EN_GRCODE3, dbo.ENHESAR_KALA.EN_GRCODE4, dbo.ENHESAR_KALA.EN_GRCODE5, dbo.ENHESAR_KALA.EN_GRCODE6, dbo.ENHESAR_KALA.EN_GRCODE7, dbo.ENHESAR_KALA.EN_GRCODE8, dbo.ENHESAR_KALA.EN_GRCODE9 , dbo.ENHESAR_KALA.EN_GRCODE10, dbo.ENHESAR_KALA.USERID, dbo.ENHESAR_KALA.EN_CDATE FROM  dbo.ENHESAR_MOSHTARI INNER JOIN dbo.ENHESAR_KALA ON dbo.ENHESAR_MOSHTARI.EN_CUST_CO = dbo.ENHESAR_KALA.EN_CUST_CO  WHERE     (dbo.ENHESAR_MOSHTARI.EN_CUST_CO <> N'" + RST.Select(x => x.CUST_NO).FirstOrDefault() + "') AND (EN_IYALAT =" + RST.Select(x => x.OSTANID).FirstOrDefault() + ") AND " + " (EN_STDATE <= " + RST.Select(x => x.DATE_N).FirstOrDefault() + ") AND (EN_ENDEN >= " + RST.Select(x => x.DATE_N).FirstOrDefault() + " OR EN_ENDEN IS NULL)").ToList();
                        foreach (var RST22_2item in RST22_2)
                        {
                            if (!ReferenceEquals(RST22_2item.EN_KALA_COD, null))
                            {
                                ///////while (!RST.EOF)
                                foreach (var RST_item in RST)
                                {
                                    if (RST_item.CODE == RST22_2item.EN_KALA_COD)
                                    {
                                        //LAST POINT CHEKED 1
                                        Msgwin msgwin = new Msgwin(false, "اين کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST22_2item.EN_CUST_CO) + '\r' + "نام کالا:" + GETKALANAME(Convert.ToDouble(RST_item.CODE))); msgwin.ShowDialog();
                                        ///////DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST22_2item.Fields("EN_CUST_CO")) + '\r' + "نام کالا:" + GETKALANAME(RST_item.Fields("CODE")));
                                        ChekEnheSarRet = Conversions.ToInteger(true);
                                    }

                                    ////RST.MoveNext();
                                }

                                /// RST.MoveFirst();
                            }
                            else
                            {
                                foreach (var RST_item in RST)
                                {
                                    MATCHTO = true;
                                    ONEONLY = false;
                                    PYT = "";
                                    if (!ReferenceEquals(RST22_2item.EN_GRCODE1, null) && Convert.ToDouble(RST22_2item.EN_GRCODE1) == -1)   // همه کالاها
                                    {
                                        PYT = " همه کالاها ";
                                        MATCHTO = true;
                                        ONEONLY = true;
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= 9; i++)
                                        {
                                            var RST22_2item_Fields_EN_GRCODE = RST22_2item.GetType().GetProperty("EN_GRCODE" + i).GetValue(RST22_2item);
                                            //////if (!ISNULL(RST22_2item.Fields("EN_GRCODE" + i)))
                                            if (!ReferenceEquals(RST22_2item_Fields_EN_GRCODE, null))
                                            {

                                                var RST_Fields_COL = RST_item.GetType().GetProperty("COL" + i).GetValue(RST_item);


                                                var RST_Fields_COLN = RST_item.GetType().GetProperty("COLN" + i).GetValue(RST_item);

                                                /////If RST2.Fields("EN_GRCODE" & i) <> rst.Fields("COL" & i)
                                                if (RST22_2item_Fields_EN_GRCODE != RST_Fields_COL)
                                                {
                                                    MATCHTO = false;
                                                }
                                                else
                                                {
                                                    PYT = PYT + " - " + RST_Fields_COLN;
                                                }

                                                ONEONLY = true;
                                            }
                                        }
                                    }

                                    if (MATCHTO && ONEONLY)
                                    {
                                        Msgwin msgwin = new Msgwin(false, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST22_2item.EN_CUST_CO) + '\r' + ":گروه کالا" + PYT); msgwin.ShowDialog();
                                        //////DoCmd.OpenForm("mesageform", default, default, default, default, acDialog, "اين گروه کالا در اين شهر انحصار است " + '\r' + "نام انحصار کننده:" + GETHESNAME(RST22_2item.Fields("EN_CUST_CO")) + '\r' + ":گروه کالا" + PYT);
                                        ChekEnheSarRet = Conversions.ToInteger(true);
                                    }
                                    /////RST.MoveNext();
                                }
                                ///////RST.MoveFirst();
                            }
                            //////RST22_2item.MoveNext();
                        }
                    }
                    else
                    {
                        ChekEnheSarRet = Conversions.ToInteger(false);
                    }
                }
            }
            return ChekEnheSarRet;
        }
        public static double GetMAsnadF(string HES)
        {
            double tempGetMAsnadF = 0;
            var rst = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.PAY_GETD.MABL) AS smab FROM         dbo.Visit_route INNER JOIN  dbo.CUST_HESAB ON dbo.Visit_route.ROUTE_NAME = dbo.CUST_HESAB.ROUTE_NAME INNER JOIN     dbo.PAY_GETD ON dbo.CUST_HESAB.hes = dbo.PAY_GETD.CUST_NO WHERE     (dbo.PAY_GETD.N_KOL3 IS NULL) AND (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL = " + Baseknow.BANKHA + "  OR    dbo.PAY_GETD.N_KOL IS NULL) AND (dbo.Visit_route.HES = N'" + HES + "') OR  (dbo.PAY_GETD.N_KOL3 IS NULL) AND (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.DATE_S > " + Tarikh.GoGetPersianDate(false) + ") AND (dbo.Visit_route.HES = N'" + HES + "')").FirstOrDefault();
            if (rst == 0 || ReferenceEquals(rst, null))
            {
                tempGetMAsnadF = 0;
            }
            else
            {
                tempGetMAsnadF = (double)rst;
            }
            return tempGetMAsnadF;
        }
        public static object GETFLDvlu(int USERID, string frm, string fld, object VLU)
        {
            object tempGETFLDvlu;

            var rst = dbms.DoGetDataSQL<GETFLD_1>("SELECT * FROM USEROPTION WHERE USERID = " + USERID + " AND FLD ='" + fld + "' AND FRM = '" + frm + "'").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                dbms.DoExecuteSQL("INSERT  INTO  USEROPTION (USERID,FLD,FRM,VLU) VALUES(" + USERID + ",'" + fld + "','" + frm + "'," + VLU + ")");
                tempGETFLDvlu = VLU;
            }
            else
            {
                tempGETFLDvlu = rst.VLU;
            }
            return tempGETFLDvlu;
        }

        public static int GETVAHEDN(string COD, int VAH)
        {
            int tempGETVAHEDN = 0;
            var rst = dbms.DoGetDataSQL<GTNMVAHED>("SELECT VAHEDS.CODE, VAHEDS.VAHED, VAHEDS.NESBAT FROM VAHEDS WHERE (((VAHEDS.CODE)= '" + COD + "' AND ((VAHEDS.VAHED)= " + VAH + ")))").FirstOrDefault();
            if (ReferenceEquals(rst, null))
            {
                tempGETVAHEDN = 1;
            }
            else
            {
                tempGETVAHEDN = (int)rst.NESBAT;
            }
            return tempGETVAHEDN;
        }
        public static DateTime GetdateFromServer()
        {
            DateTime tempGetdateFromServer = DateTime.MinValue;
            DateTime DTT = DateTime.MinValue;
            var rst = dbms.DoGetDataSQL<DateTime>("select getdate()").FirstOrDefault();
            if (!ReferenceEquals(rst, null))
            {
                tempGetdateFromServer = rst;
            }
            else
            {
                tempGetdateFromServer = DateTime.Now;
            }
            return tempGetdateFromServer;
        }
        public static long GTFS()
        {
            //DateTime.Now.ToString("HH:mm:ss");
            DateTime dt = DateTime.MinValue;
            dt = GetdateFromServer();
            return dt.Hour * 10000 + dt.Minute * 100 + dt.Second;
        }
        public static long Gettaskid(double num, int tg)
        {
            long GettaskidRet = default;

            var rst = dbms.DoGetDataSQL<Q1>("SELECT IDNUM,num,tg FROM dbo.TASKS WHERE num = " + num + " and tg = " + tg).FirstOrDefault();

            if (!ReferenceEquals(rst, null))
            {
                GettaskidRet = (long)rst.IDNUM;
            }
            else
            {
                GettaskidRet = 0L;
            }
            return GettaskidRet;
        }
        public static string GETUSERNAME(int US)
        {
            string GETUSERNAMERet = default;
            var rst = dbms.DoGetDataSQL<string>("SELECT     SAL_NAME FROM dbo.SALA_DTL WHERE     (IDD= " + US + ")").FirstOrDefault();
            if (!ReferenceEquals(rst, null))
            {
                GETUSERNAMERet = DECODEUN(rst);
            }
            return GETUSERNAMERet;
        }
        /// <summary>
        ///  new WindowInteropHelper(this).Handle
        /// </summary>
        /// <param name="FRM"></param>
        /// <param name="ID"></param>
        /// <param name="USERCO"></param>
        /// <param name="windowHandle"></param>


        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // اگر نیازی نیست، فقط برگردان IntPtr.Zero
            return IntPtr.Zero;
        }
        public static void LetSigneTick(string FRM, int ID, int USERCO, IntPtr windowHandle)
        {
            if (windowHandle == null || windowHandle == IntPtr.Zero)
            {
                // Handle the case where windowHandle is zero, possibly logging or notifying the user.
                return;
            }

            //var TheWindo = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            ////var TheWindo = Application.Current.Windows.OfType<Window>().SingleOrDefault(win => win.GetType().Name.Equals(FRM));

            //// 1) از HWND یک HwndSource بگیرید
            //var source = HwndSource.FromHwnd(windowHandle);

            //// 2) نصب هُک
            //source.AddHook(HwndHook);

            // 1) از HWND یک HwndSource بگیرید
            var source = HwndSource.FromHwnd(windowHandle);
            Window TheWindo = null;

            if (source != null)
            {
                TheWindo = source.RootVisual as Window;
                // 2) نصب هُک
                source.AddHook(HwndHook);
            }

            // Fallback strategy to find the window if HwndSource failed or RootVisual was null
            if (TheWindo == null)
            {
                foreach (Window win in Application.Current.Windows)
                {
                    try
                    {
                        // Check by Handle
                        if (new WindowInteropHelper(win).Handle == windowHandle)
                        {
                            TheWindo = win;
                            break;
                        }
                    }
                    catch { /* Ignore invalid handles */ }
                }
            }

            if (TheWindo == null && !string.IsNullOrEmpty(FRM))
            {
                foreach (Window win in Application.Current.Windows)
                {
                    // Check by Name (Type Name)
                    if (win.GetType().Name == FRM)
                    {
                        TheWindo = win;
                        break;
                    }
                }
            }

            if (TheWindo == null) return; // If still null, we cannot proceed with signature checks


            object rst = null;
            switch (ID)
            {
                case 37:
                    {
                        rst = dbms.DoGetDataSQL<Q10>("SELECT   USERCO, SGN0137 , SGN0237,SGN0337 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 36:
                    {
                        rst = dbms.DoGetDataSQL<_QRE15_>("SELECT   USERCO, SGN0136 , SGN0236,SGN0336 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 20:
                    {
                        rst = dbms.DoGetDataSQL<Q2>("SELECT   USERCO, FFRP_FROOSH, FFRP_ANB, FFRP_HESAB,FFRP_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 1:
                    {
                        rst = dbms.DoGetDataSQL<Q3>("SELECT   USERCO, ANB_RASID_ANB, ANB_RASID_QC,0 AS F3 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 6:
                    {
                        rst = dbms.DoGetDataSQL<Q4>("SELECT   USERCO, ENTGH_AZANB, ENTGH_BEANB, ENTGH_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 4:
                    {
                        rst = dbms.DoGetDataSQL<Q5>("SELECT   USERCO, FFRB_FROOSH, FFRB_ANB, FFRB_HESAB, FFRB_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if (rst.GetType().GetProperties()[4].GetValue(rst, null) is true)
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = true;
                                //FRM["SGN4"].Enabled = true;
                                //FRM["SGN4"].Locked = false;
                            }
                            else
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = false;
                                //FRM["SGN4"].Enabled = false;
                                //FRM["SGN4"].Locked = true;
                            }
                        }

                        break;
                    }

                case 3:
                    {
                        rst = dbms.DoGetDataSQL<Q6>("SELECT   USERCO, KFRB_BAZAR, KFRB_ANB, KFRB_HESAB, KFRB_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if (rst.GetType().GetProperties()[4].GetValue(rst, null) is true)
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = true;
                                //FRM["SGN4"].Enabled = true;
                                //FRM["SGN4"].Locked = false;
                            }
                            else
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = false;
                                //FRM["SGN4"].Enabled = false;
                                //FRM["SGN4"].Locked = true;
                            }
                        }
                        break;
                    }

                case 27:
                    {
                        rst = dbms.DoGetDataSQL<Q7>("SELECT   USERCO, KFRB_BAZAR, KFRB_ANB, KFRB_HESAB, KFRB_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        if (!ReferenceEquals(rst, null))
                        {
                            if (rst.GetType().GetProperties()[4].GetValue(rst, null) is true)
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = true;
                                //FRM["SGN4"].Enabled = true;
                                //FRM["SGN4"].Locked = false;
                            }
                            else
                            {
                                //(TheWindo.FindName("SGN4") as CheckBox).IsEnabled = false;
                                //FRM["SGN4"].Enabled = false;
                                //FRM["SGN4"].Locked = true;
                            }
                        }
                        break;
                    }

                case 13:
                    {
                        rst = dbms.DoGetDataSQL<_QRE_6>("SELECT   USERCO, FFR_FROOSH, FFR_HESAB,FFR_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                //case 33: // ÕÇÏÑÇÊí '
                //    {
                //        rst.Open("SELECT   USERCO, SGN0133 , SGN0233,SGN0333 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                //        break;
                //    }
                //case 32: // ÕÇÏÑÇÊí '
                //    {
                //        rst.Open("SELECT   USERCO, SGN0132 , SGN0232,SGN0332 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                //        break;
                //    }
                case 34: // خزانه 
                    {
                        rst = dbms.DoGetDataSQL<_QRE_7>("SELECT   USERCO, SGN0134 , SGN0234,SGN0334 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                //case 35: // ÎÒÇäå '
                //    {
                //        rst.Open("SELECT   USERCO, SGN0135 , SGN0235,SGN0335 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                //        break;
                //    }
                case 12:
                    {
                        rst = dbms.DoGetDataSQL<_QRE14_>("SELECT   USERCO, KFR_BAZAR, KFR_HESAB, KFR_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 2:
                    {
                        rst = dbms.DoGetDataSQL<QRE_21>("SELECT   USERCO, ANB_HAVL_FROSH, ANB_HAVL_ANB, ANB_HAVL_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 100:
                    {
                        rst = dbms.DoGetDataSQL<LSQ2>("SELECT   USERCO, PAY_MVAHED, PAY_HESAB, PAY_MAMEL FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 39: //حواله خروج
                    {
                        rst = dbms.DoGetDataSQL<LSQ7>("SELECT   USERCO, ANB_KHOROG_MODIRT , ANB_KHOROG_ANB, ANB_KHOROG_ANB AS HES FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }

                //case 90:
                //    {
                //        rst.Open("SELECT   USERCO, PAZ_PAZ , PAZ_HES,PAZ_CEO FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                //        break;
                //    }
                case 24:
                    {
                        rst = dbms.DoGetDataSQL<LSQ3>("SELECT   USERCO, SGN0124 , SGN0224,SGN0324 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 26:
                    {
                        rst = dbms.DoGetDataSQL<LSQ8>("SELECT   USERCO, SGN0126 , SGN0226,SGN0326 FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
                case 0:
                    {
                        rst = dbms.DoGetDataSQL<CL_QT>("SELECT   USERCO, SND_TAHI,SND_MALI,SND_MODIR FROM dbo.SIGN WHERE     USERCO = " + USERCO).FirstOrDefault();
                        break;
                    }
            }

            if (!ReferenceEquals(rst, null))
            {
                //if (rst.Fields(3) == true)
                if (rst.GetType().GetProperties()[3].GetValue(rst, null) is true && TheWindo.GetType().Name != "HEAD_LST_RASID")
                {
                    var sgn3 = TheWindo.FindName("SGN3") as CheckBox;
                    if (sgn3 != null)
                    {
                        sgn3.IsEnabled = true;
                        sgn3.IsHitTestVisible = true;
                    }
                    //FRM["SGN3"].Enabled = true;
                    //FRM["SGN3"].Locked = false;
                }
                else
                {
                    if (TheWindo.GetType().Name != "HEAD_LST_RASID")
                    {
                        var sgn3 = TheWindo.FindName("SGN3") as CheckBox;
                        if (sgn3 != null)
                        {
                            sgn3.IsHitTestVisible = false;
                        }
                    }



                    //(TheWindo.FindName("SGN3") as CheckBox).Visibility = Visibility.Hidden;
                    //FRM["SGN3"].Enabled = false;
                    //FRM["SGN3"].Locked = true;
                }
                if (rst.GetType().GetProperties()[2].GetValue(rst, null) is true)
                {
                    (TheWindo.FindName("SGN2") as CheckBox).IsEnabled = true;
                    (TheWindo.FindName("SGN2") as CheckBox).IsHitTestVisible = true;
                    //FRM["SGN2"].Enabled = true;
                    //FRM["SGN2"].Locked = false;
                }
                else
                {
                    (TheWindo.FindName("SGN2") as CheckBox).IsHitTestVisible = false;
                    //FRM["SGN2"].Enabled = false;
                    //FRM["SGN2"].Locked = true;
                }
                if (rst.GetType().GetProperties()[1].GetValue(rst, null) is true)
                {
                    (TheWindo.FindName("SGN1") as CheckBox).IsEnabled = true;
                    (TheWindo.FindName("SGN1") as CheckBox).IsHitTestVisible = true;
                    //FRM["SGN1"].Enabled = true;
                    //FRM["SGN1"].Locked = false;
                }
                else
                {
                    (TheWindo.FindName("SGN1") as CheckBox).IsHitTestVisible = false;
                    //FRM["SGN1"].Enabled = false;
                    //FRM["SGN1"].Locked = true;
                }
            }
            else
            {

                var sgn1 = TheWindo.FindName("SGN1") as CheckBox;
                if (sgn1 != null)
                {
                    sgn1.IsHitTestVisible = false;
                }

                var sgn2 = TheWindo.FindName("SGN2") as CheckBox;
                if (sgn2 != null)
                {
                    sgn2.IsHitTestVisible = false;
                }

                var sgn3 = TheWindo.FindName("SGN3") as CheckBox;
                if (sgn3 != null)
                {
                    sgn3.IsHitTestVisible = false;
                }
                //FRM["SGN3"].Enabled = false;
                //FRM["SGN3"].Locked = true;
                //FRM["SGN2"].Enabled = false;
                //FRM["SGN2"].Locked = true;
                //FRM["SGN1"].Enabled = false;
                //FRM["SGN1"].Locked = true;
            }

            // 5) حتماً هُک را پاک کن
            source.RemoveHook(HwndHook);
        }


        public static string SHARTCRATOR(string TXT)
        {
            string SHARTCRATORRet = default;
            int i, K;
            string TXTO;
            string shart = default, SHAT2;
            string shart2 = "";
            i = 1;
            K = 1;
            TXT = Strings.Trim(TXT);
            while (i < Strings.Len(TXT))
            {
                if (Strings.Mid(TXT, i, 1) == " ")
                {
                    if (Strings.Len(shart) > 0)
                    {
                        shart = shart + " and NAME LIKE replace(replace('%" + Strings.Mid(TXT, K + 1, i - K - 1) + "%',nchar(1740),nchar(1610)),'ک', 'ك')";
                        shart2 = shart2 + " and NAME LIKE replace(replace('%" + Strings.Mid(TXT, K + 1, i - K - 1) + "%',nchar(1610),nchar(1740)),'ك', 'ک')";
                        K = i;
                    }
                    else
                    {
                        shart = "NAME LIKE replace(replace('%" + Strings.Mid(TXT, K, i - K) + "%',nchar(1740),nchar(1610)),'ک', 'ك')";
                        shart2 = "NAME LIKE replace(replace('%" + Strings.Mid(TXT, K, i - K) + "%',nchar(1610),nchar(1740)),'ك', 'ک')";
                        K = i;
                    }
                }
                i = i + 1;
            }
            if (i != K)
            {
                if (Strings.Len(shart) > 0)
                {
                    shart = shart + " and NAME LIKE replace(replace('%" + Strings.Mid(TXT, K + 1, i - K + 1) + "%',nchar(1740),nchar(1610)),'ک', 'ك')";
                    shart2 = shart2 + " and NAME LIKE replace(replace('%" + Strings.Mid(TXT, K + 1, i - K + 1) + "%',nchar(1610),nchar(1740)),'ك', 'ک')";
                    K = i;
                }
                else
                {
                    shart = "NAME LIKE  replace(replace('%" + Strings.Mid(TXT, K, i - K + 1) + "%',nchar(1740),nchar(1610)),'ک', 'ك')";
                    shart2 = "NAME LIKE replace(replace('%" + Strings.Mid(TXT, K, i - K + 1) + "%',nchar(1610),nchar(1740)),'ك', 'ک')";
                    K = i;
                }
            }
            if (Strings.Len(TXT) == 1)
            {
                shart = "NAME LIKE replace(replace('%" + TXT + "%',nchar(1740),nchar(1610)),'ک', 'ك')";
                shart2 = "NAME LIKE replace(replace('%" + TXT + "%',nchar(1610),nchar(1740)),'ك', 'ک')";
            }
            if (Strings.Len(shart) > 1)
            {
                shart = "(( " + shart + ") or (" + shart2 + "))";
            }
            SHARTCRATORRet = shart;
            return SHARTCRATORRet;
        }

        public static bool Frisok(string HES)
        {
            bool FrisokRet = default;
            int i;
            object a = default, fs;
            if (Strings.Mid(Baseknow.OPTIONSS, 64, 1) == "5")
            {
                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "frsnd" + Baseknow.USERCOD + "')   DROP view " + "frsnd" + Baseknow.USERCOD);
                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "frinv" + Baseknow.USERCOD + "')   DROP view " + "frinv" + Baseknow.USERCOD);
                dbms.DoExecuteSQL("create  view frsnd" + Baseknow.USERCOD + " as SELECT     SUM(BED - BES) AS Expr2, NUMBER FROM dbo.DEED_DTL WHERE     (TAG = 13) AND (HES = N'" + HES + "') GROUP BY NUMBER");
                dbms.DoExecuteSQL("create  view frinv" + Baseknow.USERCOD + " as SELECT     dbo.HEAD_LST.NUMBER,dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) AS Expr1 FROM         dbo.HEAD_LST INNER JOIN dbo.JAMFACTPRS ON dbo.HEAD_LST.NUMBER = dbo.JAMFACTPRS.NUMBER AND dbo.HEAD_LST.TAG = dbo.JAMFACTPRS.TAG + 11 LEFT OUTER JOIN dbo.jamchkfact ON dbo.JAMFACTPRS.NUMBER = dbo.jamchkfact.NUMBER WHERE     (dbo.HEAD_LST.TAG = 13) AND (dbo.HEAD_LST.CUST_NO = N'" + HES + "') GROUP BY dbo.HEAD_LST.NUMBER,dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) HAVING      (dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) <> 0)");

                var rst = dbms.DoGetDataSQL<Q9>("SELECT     dbo.frinv" + Baseknow.USERCOD + ".NUMBER, dbo.frsnd" + Baseknow.USERCOD + ".Expr2 FROM   dbo.frsnd" + Baseknow.USERCOD + " RIGHT OUTER JOIN  dbo.frinv" + Baseknow.USERCOD + " ON dbo.frsnd" + Baseknow.USERCOD + ".NUMBER = dbo.frinv" + Baseknow.USERCOD + ".NUMBER WHERE     (dbo.frinv" + Baseknow.USERCOD + ".Expr1 > 0) AND (dbo.frsnd" + Baseknow.USERCOD + ".Expr2 IS NULL) OR (round(dbo.frinv" + Baseknow.USERCOD + ".Expr1 - dbo.frsnd" + Baseknow.USERCOD + ".Expr2,0) <> 0)").ToList();
                if (rst.Count > 0)
                {
                    //a.writeline(rst.Fields("NUMBER"));
                    //a.writeline(DateTime.Now);
                    //a.Close();
                    var loopTo = rst.Count;
                    if (loopTo >= 0)
                    {
                        for (i = 1; i <= loopTo;)
                        {
                            AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADFROOSH(Convert.ToInt64(rst[i].NUMBER), Convert.ToInt64(rst[i].NUMBER), false);
                            //rst.MoveNext();
                            i++;
                        }
                    }
                    FrisokRet = false;
                }
                else
                {
                    FrisokRet = true;
                }
                //rst.Requery();
                //rst.Requery();
                //rst.Requery();
                //rst.Requery();
                if (rst.Count > 0)
                {
                    FrisokRet = false;
                }
                else
                {
                    FrisokRet = true;
                }
            }
            else
            {
                FrisokRet = true;
            }
            return FrisokRet;
        }

        public static long Createsanad(long DATE_S, string SHARH_S, int GHATEI, int NO_S, int OKF, string USER_NAME)
        {
            long CreatesanadRet = default;
            CreatesanadRet = AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.Createsanad(DATE_S, SHARH_S, GHATEI, NO_S, OKF, USER_NAME);

            return CreatesanadRet;
        }

        public static double GETSTANDARDPRICE_SAR(string CODE, long dt)
        {
            double tempGETSTANDARDPRICE_SAR = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_SAR = (double)rst.SumOfIMBIBE_SAR;
                }
                else
                {
                    tempGETSTANDARDPRICE_SAR = 0;
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_SAR = (double)rst.SumOfIMBIBE_SAR;
                }
                else
                {
                    tempGETSTANDARDPRICE_SAR = 0;
                }
            }
            return tempGETSTANDARDPRICE_SAR;
        }
        public static double GETSTANDARDPRICE_DAST(string CODE, long dt)
        {
            double tempGETSTANDARDPRICE_DAST = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst is null))
                {
                    tempGETSTANDARDPRICE_DAST = (double)rst.SumOfIMBIBE_MANF;
                }
                else
                {
                    tempGETSTANDARDPRICE_DAST = 0;
                }
            }
            else
            {
                var rst5 = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(rst5 is null))
                {
                    tempGETSTANDARDPRICE_DAST = (double)rst5.SumOfIMBIBE_MANF;
                }
                else
                {
                    tempGETSTANDARDPRICE_DAST = 0;
                }
            }
            return tempGETSTANDARDPRICE_DAST;
        }
        public static double GETLASTFR(string co, long dt)
        {
            double tempGETLASTFR = 0;
            long FNN = 0;
            //object rst = null;

            //if (rst.GetType().GetProperties()[3].GetValue(rst, null) is true)
            var rst = dbms.DoGetDataSQL<double?>("SELECT     TOP 100 PERCENT dbo.INVO_LST.N_KOL FROM dbo.INVO_LST INNER JOIN   dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE     (dbo.INVO_LST.TAG = 9) AND (dbo.INVO_LST.CODE = N'" + co + "') AND (dbo.HEAD_LST.DATE_N <= " + dt + ") ORDER BY dbo.INVO_LST.NUMBER DESC").FirstOrDefault();
            if (!(rst is null))
            {
                if (IsNull(rst))
                {
                    var rst1 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                    if (!(rst1 is null))
                    {
                        tempGETLASTFR = (double)rst1;
                    }
                    else
                    {
                        tempGETLASTFR = 0;
                    }
                }
                else
                {
                    FNN = (long)rst;


                    var rst2 = dbms.DoGetDataSQL<QRE15>("SELECT     TOP 100 PERCENT CODE,FNUMB FROM dbo.HEAD_MANF WHERE     (FNUMB = " + FNN + " and CODE = N'" + co + "')").FirstOrDefault();
                    if (!(rst2 is null))
                    {
                        tempGETLASTFR = (double)rst2.FNUMB;
                    }
                    else
                    {
                        var rst3 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                        if (!(rst3 is null))
                        {
                            tempGETLASTFR = (double)rst3;
                        }
                        else
                        {
                            tempGETLASTFR = 0;
                        }
                    }
                }
            }
            else
            {
                var rst4 = dbms.DoGetDataSQL<int?>("SELECT     TOP 100 PERCENT FNUMB FROM dbo.HEAD_MANF WHERE     (CODE = N'" + co + "') ORDER BY FNUMB DESC ").FirstOrDefault();
                if (!(rst4 is null))
                {
                    tempGETLASTFR = (double)rst4;
                }
                else
                {
                    tempGETLASTFR = 0;
                }
            }
            return tempGETLASTFR;
        }
        public static double GETSTANDARDPRICE_MAVAD(string CODE, long dt)
        {
            double tempGETSTANDARDPRICE_MAVAD = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").ToList();
                if (rst.Count > 0)
                {
                    tempGETSTANDARDPRICE_MAVAD = (double)rst.Select(x => x.SumOfMABLK).FirstOrDefault();
                }
                else
                {
                    tempGETSTANDARDPRICE_MAVAD = 0;
                }
            }
            else
            {
                var rst = dbms.DoGetDataSQL<HEAD_MANF_1>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").ToList();
                if (rst.Count > 0)
                {
                    tempGETSTANDARDPRICE_MAVAD = (double)rst.Select(x => x.SumOfMABLK).FirstOrDefault();
                }
                else
                {
                    tempGETSTANDARDPRICE_MAVAD = 0;
                }
            }
            return tempGETSTANDARDPRICE_MAVAD;
        }
        public static void CREATHES(object KOL, object MOIN, long taf, string nam)
        {
            var rst = dbms.DoGetDataSQL<DETA_HES_MODEL1>("SELECT * FROM DETA_HES WHERE N_KOL = " + KOL.ToString() + " AND NUMBER = " + MOIN.ToString()).ToList();
            if (rst.Count == 0)
            {
                //rst.AddNew;
                //rst.Fields["N_KOL"] = KOL;
                //rst.Fields["NUMBER"] = MOIN;
                //rst.Fields["NAME"] = nam;
                //rst.update;
                string Test = $@"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME) VALUES({KOL},{MOIN},N'{nam}')";

                dbms.DoExecuteSQL($"INSERT INTO dbo.DETA_HES(N_KOL, NUMBER, NAME) VALUES({KOL},{MOIN},N'{nam}')");
            }
            //var rst2 = dbms.DoGetDataSQL<TDETA_HES_MODEL1>("SELECT N_KOL,NUMBER,TNUMBER,NAME FROM dbo.TDETA_HES");
            //rst2.AddNew;
            //rst2.Fields["N_KOL"] = KOL;
            //rst2.Fields["NUMBER"] = MOIN;
            //rst2.Fields["TNUMBER"] = taf;
            //rst2.Fields["NAME"] = nam;
            //rst2.update;
            try
            {
                string testtrace3 = nam;
                string testtrace33 = $"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME) VALUES({KOL},{MOIN},{taf},N'{nam}')";
                dbms.DoExecuteSQL($"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME) VALUES({KOL},{MOIN},{taf},N'{nam}')");
            }
            catch (Exception)
            {
                ////INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //if (Microsoft.VisualBasic.Information.Err() != 0)
                //{
                //    rst2.Fields["NAME"] = nam + " " + taf;
                //    rst2.update;
                //}
                string testtrace3334 = $"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME) VALUES({KOL},{MOIN},{taf},N'{nam + " " + taf}'";
                dbms.DoExecuteSQL($"INSERT INTO dbo.TDETA_HES(N_KOL, NUMBER, TNUMBER, NAME) VALUES({KOL},{MOIN},{taf},N'{nam + " " + taf}')");
            }
        }
        public static bool ISHESAB(object KOL, object MOIN, long taf)
        {
            bool tempISHESAB = false;
            var rst = dbms.DoGetDataSQL<QRE13>("SELECT N_KOL,NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + KOL.ToString() + " AND NUMBER = " + MOIN.ToString() + " AND TNUMBER = " + taf).ToList();
            if (rst.Count == 0)
            {
                tempISHESAB = false;
            }
            else
            {
                tempISHESAB = true;
            }

            return tempISHESAB;
        }
        public static string GETDEPART(long DEPART)
        {
            string tempGETDEPART = null;
            var rst = dbms.DoGetDataSQL<DEPART_CSHARP>("select * from DEPART where DEPATMAN = " + DEPART).ToList();
            if (rst.Count > 0)
            {
                tempGETDEPART = rst.Select(x => x.DEPNAME).FirstOrDefault();
            }
            return tempGETDEPART;
        }
        public static long GETKOL(string SHES)
        {
            long GETKOLRet = default;
            byte i;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETKOLRet;
                    }
                }
                GETKOLRet = Conversions.ToLong(Strings.Left(SHES, i - 1));
            }

            return GETKOLRet;
        }
        public static long GETMOIN(string SHES)
        {
            long GETMOINRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETMOINRet;
                    }
                }
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-" & j <= Strings.Len(SHES))
                    j = (byte)(j + 1);
                i = (byte)(i + 1);
                GETMOINRet = Conversions.ToLong(Strings.Mid(SHES, i, j - i));
            }

            return GETMOINRet;
        }
        public static long GETTAF(string SHES)
        {
            long GETTAFRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                {
                    i = (byte)(i + 1);
                    if (i > 200)
                    {
                        return GETTAFRet;
                    }
                }
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-")
                    j = (byte)(j + 1);
                i = j;
                while (Strings.Mid(SHES, j + 1, 1) != "-" & j < Strings.Len(SHES))
                    j = (byte)(j + 1);
                GETTAFRet = Conversions.ToLong(Strings.Mid(SHES, i + 1, j - i));
            }

            return GETTAFRet;
        }
        public static string GETBANK(double BANK)
        {
            string GETBANKRet = default;
            var RRST = dbms.DoGetDataSQL<string>("SELECT NAMES FROM TCOD_BANKS WHERE (((TCOD_BANKS.CODE)= " + BANK + "))").FirstOrDefault();
            if (!(RRST is null))
            {
                //GETBANKRet = RRST.Fields(1);
                GETBANKRet = RRST;
            }
            else
            {
                GETBANKRet = "";
            }
            return GETBANKRet;
        }
        private static bool IsNull(object p)
        {
            if (!(p is null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void GENSANADKHAREED(object fnum, long TNUM)
        {
            AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADKHAREED(Convert.ToInt64(fnum), Convert.ToInt64(TNUM), false);
        }
        public static string GETGRPKALA(int CC)
        {
            var rst = dbms.DoGetDataSQL<string>("SELECT     NAMES  FROM dbo.TCOD_STUFGROUP WHERE     (CODE = " + System.Convert.ToString(CC) + ")").ToList();
            if (rst.Count > 0)
            {
                return System.Convert.ToString(rst.FirstOrDefault());
            }
            return null;
        }
        public static bool Khisok(string HES)
        {
            bool KhisokRet = default;
            int i;
            object a = default, fs;
            if (Strings.Mid(Baseknow.OPTIONSS, 64, 1) == "5")
            {
                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "frsnd" + Baseknow.USERCOD + "')   DROP view " + "frsnd" + Baseknow.USERCOD);
                dbms.DoExecuteSQL("IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_NAME = '" + "frinv" + Baseknow.USERCOD + "')   DROP view " + "frinv" + Baseknow.USERCOD);

                dbms.DoExecuteSQL("create  view frsnd" + Baseknow.USERCOD + " as SELECT     SUM(BES - BED) AS Expr2, NUMBER FROM dbo.DEED_DTL WHERE     (TAG = 12) AND (HES = N'" + HES + "') GROUP BY NUMBER");
                // DoCmd.RunSQL ("create  view frinv" & Forms![BASEKNOW]![USERCOD] & " as SELECT     dbo.INVO_LST.NUMBER, SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS Expr1, dbo.HEAD_LST.MBAA FROM         dbo.INVO_LST INNER JOIN    dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG - 11 WHERE     (dbo.HEAD_LST.TAG = 12) AND (dbo.HEAD_LST.CUST_NO = N'" & HES & "') GROUP BY dbo.INVO_LST.NUMBER, dbo.HEAD_LST.MBAA HAVING      (SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) <> 0)")
                dbms.DoExecuteSQL("create  view frinv" + Baseknow.USERCOD + " as SELECT     dbo.HEAD_LST.NUMBER,dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) AS Expr1 FROM         dbo.HEAD_LST INNER JOIN dbo.JAMFACTPRS ON dbo.HEAD_LST.NUMBER = dbo.JAMFACTPRS.NUMBER AND dbo.HEAD_LST.TAG = dbo.JAMFACTPRS.TAG + 11 LEFT OUTER JOIN dbo.jamchkfact ON dbo.JAMFACTPRS.NUMBER = dbo.jamchkfact.NUMBER WHERE     (dbo.HEAD_LST.TAG = 12) AND (dbo.HEAD_LST.CUST_NO = N'" + HES + "') GROUP BY dbo.HEAD_LST.NUMBER,dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) HAVING      (dbo.JAMFACTPRS.mab - dbo.HEAD_LST.M_NAGHD - dbo.HEAD_LST.MABL_VAR - dbo.HEAD_LST.MABL_HAV + dbo.HEAD_LST.MABL_HAZ - ISNULL(dbo.jamchkfact.mabch,0) <> 0)");
                // RST.Open "SELECT     dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".NUMBER, dbo.frsnd" & Forms![BASEKNOW]![USERCOD] & ".Expr2 FROM   dbo.frsnd" & Forms![BASEKNOW]![USERCOD] & " RIGHT OUTER JOIN  dbo.frinv" & Forms![BASEKNOW]![USERCOD] & " ON dbo.frsnd" & Forms![BASEKNOW]![USERCOD] & ".NUMBER = dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".NUMBER WHERE     (dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".Expr1 + dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".MBAA > 0) AND (dbo.frsnd" & Forms![BASEKNOW]![USERCOD] & ".Expr2 IS NULL) OR (round(dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".Expr1 + dbo.frinv" & Forms![BASEKNOW]![USERCOD] & ".MBAA - dbo.frsnd" & Forms![BASEKNOW]![USERCOD] & ".Expr2,0) <> 0)", CurrentProject.Connection, adOpenKeyset, adLockOptimistic
                var rst = dbms.DoGetDataSQL<QRE19>("SELECT     dbo.frinv" + Baseknow.USERCOD + ".NUMBER, dbo.frsnd" + Baseknow.USERCOD + ".Expr2 FROM   dbo.frsnd" + Baseknow.USERCOD + " RIGHT OUTER JOIN  dbo.frinv" + Baseknow.USERCOD + " ON dbo.frsnd" + Baseknow.USERCOD + ".NUMBER = dbo.frinv" + Baseknow.USERCOD + ".NUMBER WHERE     (dbo.frinv" + Baseknow.USERCOD + ".Expr1 > 0) AND (dbo.frsnd" + Baseknow.USERCOD + ".Expr2 IS NULL) OR (round(dbo.frinv" + Baseknow.USERCOD + ".Expr1 - dbo.frsnd" + Baseknow.USERCOD + ".Expr2,0) <> 0)").ToList();
                if (rst.Count > 0)
                {
                    //string path = @"c:\errorkh.txt";
                    //File.AppendAllLines(path, new[] { DateTime.Now.ToString() });

                    string _Pathfile_ = @"C:\CORRECT\errorkh.txt";
                    string directory = Path.GetDirectoryName(_Pathfile_);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    using (StreamWriter writer = new StreamWriter(_Pathfile_, true))
                    {
                        for (i = 0; i <= rst.Count; i++)
                        {
                            writer.WriteLine("شماره رسید: " + rst[i].NUMBER.ToStringNullSafe());
                            /////////////////////////////////////////////////////////////////////////CHECK MATTER
                            //a.writeline(rst.Fields("NUMBER"));
                            GENSANADKHAREED(rst[i].NUMBER, (long)rst[i].NUMBER);
                            //rst.MoveNext();
                        }
                    }
                    KhisokRet = false;
                }
                else
                {
                    KhisokRet = true;
                }
                //rst.Requery();
                //rst.Requery();
                //rst.Requery();
                //rst.Requery();
                if (rst.Count > 0)
                {
                    KhisokRet = false;
                }
                else
                {
                    KhisokRet = true;
                }
            }
            else
            {
                KhisokRet = true;
            }

            return KhisokRet;
        }
        public static bool ISTAF(string HES)
        {
            if (string.IsNullOrEmpty(HES))
            {
                return false;
            }

            double? HKOL = null;
            double? HMOIN = null;
            double? HTAF = null;
            double? HTAF2 = null;
            double? HTAF3 = null;
            double? HTAF4 = null;

            GETTAF3(HES, ref HKOL, ref HMOIN, ref HTAF, ref HTAF2, ref HTAF3, ref HTAF4);

            if (IsNull(HKOL) || IsNull(HMOIN))
            {
                return false;

            }
            if (IsNull(HTAF))
            {
                var RST = dbms.DoGetDataSQL<int?>("SELECT   TNUMBER FROM dbo.TDETA_HES WHERE (N_KOL = " + System.Convert.ToString(HKOL) + ") And (NUMBER = " + System.Convert.ToString(HMOIN) + ") GROUP BY TNUMBER").FirstOrDefault();
                if (!(RST is null))
                    return true;
                else
                    return false;
            }
            else
            {
                if (IsNull(HTAF2))
                {
                    var RST = dbms.DoGetDataSQL<int?>("SELECT   TNUMBER2 FROM dbo.TDETA_HES2 WHERE (N_KOL = " + System.Convert.ToString(HKOL) + ") And (NUMBER = " + System.Convert.ToString(HMOIN) + ") And (TNUMBER = " + System.Convert.ToString(HTAF) + ") GROUP BY TNUMBER2").FirstOrDefault();
                    if (!(RST is null))
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (IsNull(HTAF3))
                    {
                        var RST = dbms.DoGetDataSQL<int?>("SELECT   TNUMBER3 FROM dbo.TDETA_HES3 WHERE (N_KOL = " + System.Convert.ToString(HKOL) + ") And (NUMBER = " + System.Convert.ToString(HMOIN) + ") And (TNUMBER = " + System.Convert.ToString(HTAF) + ") And (TNUMBER2 = " + System.Convert.ToString(HTAF2) + ") GROUP BY TNUMBER3").FirstOrDefault();
                        if (!(RST is null))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (IsNull(HTAF4))
                        {
                            var RST = dbms.DoGetDataSQL<int?>("SELECT   TNUMBER4 FROM dbo.TDETA_HES4 WHERE (N_KOL = " + System.Convert.ToString(HKOL) + ") And (NUMBER = " + System.Convert.ToString(HMOIN) + ") And (TNUMBER = " + System.Convert.ToString(HTAF) + ") And (TNUMBER2 = " + System.Convert.ToString(HTAF2) + ") And (TNUMBER3 = " + System.Convert.ToString(HTAF3) + ") GROUP BY TNUMBER4").FirstOrDefault();
                            if (!(RST is null))
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public static bool Signed(int tg, long num)
        {
            int I;
            object a;
            object fs;
            var RST = dbms.DoGetDataSQL<HEAD_LST_CSHARP>("SELECT     sgn2,sgn3 from head_lst  WHERE     (tag = " + System.Convert.ToString(tg) + ") AND ( NUMBER =" + System.Convert.ToString(num) + ") ").FirstOrDefault();
            if (!(RST is null))
            {
                if (RST.SGN2 is true || RST.SGN3 is true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static byte MAHDAYS(int SAL, byte mah)
        {
            if (mah >= 1 && mah <= 6)
            {
                return (byte)31;
            }
            else if (mah >= 7 && mah <= 11)
            {
                return (byte)30;
            }
            else if (mah == ((byte)12))
            {
                if (Convert.ToBoolean(KABISEH(SAL)))
                {
                    return (byte)30;
                }
                else
                {
                    return (byte)29;
                }
            }
            return 0;
        }
        public static byte ROOZ(long F_DATE)
        {
            return (byte)(F_DATE % 100);
        }
        public static byte mah(long F_DATE)
        {
            //return System.Convert.ToByte(((F_DATE % 10000) / 100));
            return System.Convert.ToByte(((F_DATE % 10000) / 100));
        }

        public static int SAL(long F_DATE)
        {
            //return System.Convert.ToInt32(Int((double)F_DATE / 10000));
            return System.Convert.ToInt32(((double)F_DATE / 10000));
        }
        public static byte KABISEH(dynamic ONLYSAL)
        {
            byte returnValue = 0;
            returnValue = (byte)0;
            if (ONLYSAL >= 1375)
            {
                if ((ONLYSAL - 1375) % 4 == 0)
                {
                    return (byte)1;
                }
            }
            else if (ONLYSAL <= 1370)
            {
                if ((70 - ONLYSAL) % 4 == 0)
                {
                    return (byte)1;
                }
            }
            return returnValue;
        }
        public static long FARSIDATE22222(DateTime DTT)
        {
            long returnValue = 0;
            //On Error Resume Next VBConversions Warning: On Error Resume Next not supported in C#
            long SHAMSI_MABNA = 0;
            DateTime MILADI_MABNA = System.Convert.ToDateTime("#01/01/0001 12:00:00 AM#");
            long DIF = 0;
            SHAMSI_MABNA = 13791012;
            MILADI_MABNA = new DateTime(2622, 3, 21);
            DTT = System.Convert.ToDateTime(GetdateFromServer());
            DIF = DateAndTime.DateDiff("D", MILADI_MABNA, DTT, (Microsoft.VisualBasic.FirstDayOfWeek)Microsoft.VisualBasic.FirstDayOfWeek.Sunday, (Microsoft.VisualBasic.FirstWeekOfYear)Microsoft.VisualBasic.FirstWeekOfYear.Jan1);
            if (DIF < 0)
            {
                Msgwin msgwin = new Msgwin(false, "تاريخ جاري سيستم شما با تاريخ سرور يکي نيست. آنرا اصلاح كنيد");
                msgwin.ShowDialog();
            }
            else
            {
                returnValue = System.Convert.ToInt64(ADDDAY(SHAMSI_MABNA, DIF));
            }
            return returnValue;
        }
        /// <summary>
        /// گرفتن تاریخ از سرور
        /// </summary>
        /// <returns></returns>
        public static long FARSIDATE2()
        {
            //return System.Convert.ToInt64(dbms.DoGetDataSQL<string>("SELECT FORMAT(GETDATE(), 'yyyyMMdd', 'fa')").FirstOrDefault()); //Not Work in SQL Server 2008
            //گرفتن تاریخ میلادی از سرور
            var res1 = dbms.DoGetDataSQL<string>("SELECT CONVERT(VARCHAR(10), GETDATE(), 111)").FirstOrDefault();
            var prt = res1.Split('/').ToArray();
            DateTime dt1 = new DateTime(Convert.ToInt32(prt[0]), Convert.ToInt32(prt[1]), Convert.ToInt32(prt[2]));
            string DTPERSIAN = dt1.ToString("yyyyMMdd", CultureInfo.GetCultureInfo("fa-Ir"));
            return Convert.ToInt64(DTPERSIAN);
        }
        public static long FARSIDATE()
        {
            //return System.Convert.ToInt64(Baseknow.dt);
            return Convert.ToInt64(Tarikh.FullCurrentDate);
        }
        public static long ADDDAY(long F_DATE, long ADD)
        {
            long ADDDAYRet = default;
            byte K;
            byte m;
            int S;
            byte r;
            byte DAYS;
            r = ROOZ(F_DATE);
            m = mah(F_DATE);
            S = SAL(F_DATE);
            K = KABISEH(S);
            DAYS = MAHDAYS(S, m);
            if (ADD > DAYS - r)
            {
                ADD = ADD - (DAYS - r + 1);
                r = 1;
                if (m < 12)
                {
                    m = (byte)(m + 1);
                }
                else
                {
                    m = 1;
                    S = S + 1;
                }
            }
            else
            {
                r = (byte)(r + ADD);
                ADD = 0L;
            }
            while (ADD > 0L)
            {
                K = KABISEH(S);
                DAYS = MAHDAYS(S, m);
                switch (ADD)
                {
                    case var @case when @case < DAYS:
                        {
                            r = (byte)(r + ADD);
                            ADD = 0L;
                            break;
                        }
                    case var case1 when DAYS <= case1 && case1 <= ((K == 0) ? 365 : 366) - 1:
                        {
                            ADD = ADD - DAYS;
                            if (m < 12)
                            {
                                m = (byte)(m + 1);
                            }
                            else
                            {
                                S = S + 1;
                                m = 1;
                            }
                            break;
                        }

                    default:
                        {
                            S = S + 1;
                            ADD = Conversions.ToLong(Operators.SubtractObject(ADD, Interaction.IIf(K == 0, 365, 366)));
                            break;
                        }
                }
            }
            ADDDAYRet = S * 10000L + m * 100 + r;
            return ADDDAYRet;
        }
        public static string GETIPADD()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static string CurrentMachineName()
        {
            //Console.WriteLine(System.Net.Dns.GetHostName());
            return System.Environment.MachineName;
        }
        public static string GETfldlist(string tbl)
        {
            string fldl = "";
            var rst = dbms.DoGetDataSQL<string>("SELECT     sys.syscolumns.name AS clmn FROM         sys.sysobjects INNER JOIN   sys.syscolumns ON sys.sysobjects.id = sys.syscolumns.id WHERE     (sys.sysobjects.name = N'" + tbl + "') and  (sys.sysobjects.uid = 1)  ORDER BY sys.syscolumns.colid").ToList();
            foreach (var item_clmn in rst)
            {
                if (fldl != "")
                {
                    fldl = fldl + ",[" + item_clmn + "]";
                }
                else
                {
                    fldl = "[" + item_clmn + "]";
                }
                //rst.MoveNext();
                //Wend;
                //return fldl;
            }
            return fldl;
        }
        private static string BuildColDef(THE_ONE1 col)
        {
            if (col.xtype == 99 || col.xtype == 35 || col.xtype == 231 || col.xtype == 167 || col.xtype == 175 || col.xtype == 239)
            {
                if (col.TYP.ToLower() == "nvarchar" || col.TYP.ToLower() == "varchar" || col.TYP.ToLower() == "nchar" || col.TYP.ToLower() == "char")
                {
                    if (col.length <= 0 || col.length >= 8000)
                        return "[" + col.name + "] [" + col.TYP + "](MAX) NULL";
                    else
                        return "[" + col.name + "] [" + col.TYP + "](" + col.length + ") NULL";
                }
                return "[" + col.name + "] [" + col.TYP + "] NULL";
            }
            return "[" + col.name + "] [" + col.TYP + "] NULL";
        }

        public static void TREXIXTCREATE(string tbl)
        {
            string FLS = "";
            var rst = dbms.DoGetDataSQL<string>("SELECT     sys.sysobjects.name AS clmn FROM         sys.sysobjects WHERE     (sys.sysobjects.name = N'TR_" + tbl + "') ").ToList();
            var RST2 = dbms.DoGetDataSQL<THE_ONE1>("SELECT     TOP (100) PERCENT sys.syscolumns.name, sys.systypes.name AS TYP, sys.syscolumns.xtype, sys.syscolumns.length FROM         sys.syscolumns INNER JOIN  sys.systypes ON sys.syscolumns.xtype = sys.systypes.xtype INNER JOIN   sys.sysobjects ON sys.syscolumns.id = sys.sysobjects.id WHERE     (sys.sysobjects.name = N'" + tbl + "') AND (sys.systypes.name <> N'sysname') ORDER BY sys.syscolumns.colid").ToList();

            if (rst.Count == 0)
            {
                FLS = "CREATE TABLE [dbo].[TR_" + tbl + "] (";
                foreach (var RST2Fields in RST2)
                {
                    var isTridd = string.Equals(RST2Fields.name, "TRIDD", StringComparison.OrdinalIgnoreCase);
                    if (isTridd)
                    {
                        var supportsIdentity = string.Equals(RST2Fields.TYP, "int", StringComparison.OrdinalIgnoreCase) || string.Equals(RST2Fields.TYP, "bigint", StringComparison.OrdinalIgnoreCase) || string.Equals(RST2Fields.TYP, "smallint", StringComparison.OrdinalIgnoreCase) || string.Equals(RST2Fields.TYP, "tinyint", StringComparison.OrdinalIgnoreCase);
                        var identityClause = supportsIdentity ? " IDENTITY(1,1)" : string.Empty;
                        FLS += "[" + RST2Fields.name + "] [" + RST2Fields.TYP + "]" + identityClause + " NOT NULL,";
                    }
                    else
                    {
                        FLS += BuildColDef(RST2Fields) + ",";
                    }
                }

                var columnNames = RST2.Select(r => r.name.ToUpperInvariant()).ToHashSet();
                if (!columnNames.Contains("UP_DATE"))
                    FLS += "[UP_DATE] [bigint] NOT NULL,";
                if (!columnNames.Contains("UP_TIME"))
                    FLS += "[UP_TIME] [float] NOT NULL,";
                if (!columnNames.Contains("UP_USER_NAME"))
                    FLS += "[UP_USER_NAME] [nvarchar](40) NULL,";
                if (!columnNames.Contains("PC_NAME"))
                    FLS += "[PC_NAME] [nvarchar](50) NULL,";
                if (!columnNames.Contains("IPADD"))
                    FLS += "[IPADD] [nvarchar](50) NULL,";
                bool hasTRIDD = columnNames.Contains("TRIDD");
                if (!hasTRIDD)
                    FLS += "[TRIDD] [int] IDENTITY(1,1) NOT NULL, ";
                FLS += "PRIMARY KEY CLUSTERED([TRIDD] ASC) ON [PRIMARY]) ON [PRIMARY]";

                dbms.DoExecuteSQL(FLS);
            }
            else
            {
                // Table exists — add any source columns that are missing from the TR_ table
                var trCols = dbms.DoGetDataSQL<string>(
                    "SELECT sys.syscolumns.name FROM sys.syscolumns INNER JOIN sys.sysobjects ON sys.syscolumns.id = sys.sysobjects.id WHERE sys.sysobjects.name = N'TR_" + tbl + "'"
                ).Select(c => c.ToUpperInvariant()).ToHashSet();

                foreach (var col in RST2)
                {
                    if (!trCols.Contains(col.name.ToUpperInvariant()))
                    {
                        try { dbms.DoExecuteSQL("ALTER TABLE [dbo].[TR_" + tbl + "] ADD " + BuildColDef(col)); } catch { }
                    }
                }
            }
        }

        public static bool ObjectExists(string objectName)
        {
            string sql = "SELECT COUNT(*) FROM sys.objects WHERE name = '" + objectName + "'";
            int count = Convert.ToInt32(dbms.DoExecuteSQL(sql));
            return count > 0;
        }
        public static void TR(string tbl, string wxs, DateTime dt, int FLAGU)
        {
            string FLS = GETfldlist(tbl);
            // Ensure the transaction table exists, if not create it.

            try { TREXIXTCREATE(tbl); } catch (Exception) { }

            if (FLAGU == 1)
            {
                // Build the extra values to be inserted.
                string trtx = Tarikh.FullCurrentDate + " AS Expr2, " + dt.ToOADate().ToString() +
                              " AS Expr1, '" + UCurrentUser() + "', '" + CurrentMachineName() + "', '" + GETIPADD() + "'";

                string insertSql = "INSERT INTO dbo.TR_" + tbl +
                                   "(" + FLS + " , UP_DATE,UP_TIME, UP_USER_NAME,PC_NAME,IPADD) " +
                                   "SELECT " + FLS + " , " + trtx + " FROM dbo." + tbl + " WHERE " + wxs;

                try
                {
                    dbms.DoExecuteSQL(insertSql);
                }
                catch (SqlException ex)
                {
                    // Instead of dropping an existing table, generate a new unique name.
                    string baseName = "TR_" + tbl;
                    string newName;
                    // Loop until we have a name that does not exist
                    do
                    {
                        // Using ticks helps ensure uniqueness.
                        newName = baseName + Tarikh.FullCurrentDate + "_" + DateTime.Now.Ticks.ToString();
                    } while (ObjectExists(newName));

                    // Rename the existing transaction table to the unique new name.
                    dbms.DoExecuteSQL("sp_rename 'TR_" + tbl + "', '" + newName + "'");

                    TREXIXTCREATE(newName);
                }
            }
            else
            {
                // For other flag values, the existing branch remains unchanged.
                string trtx = Tarikh.FullCurrentDate + " AS Expr2, " + dt.ToOADate().ToString() + " AS Expr1";
                dbms.DoExecuteSQL("INSERT INTO dbo.TR_" + tbl + "(" + FLS + " , UP_TIME, UP_DATE) " +
                                   "SELECT " + FLS + " , " + trtx + " FROM dbo." + tbl + " WHERE " + wxs);
            }
        }

        public static double PERSONELUpdate(int tg, double numb, int per, string sha)
        {
            double MIDD = 0;
            DateTime td;
            MIDD = Convert.ToDouble(Gettaskid(numb, tg));
            if (MIDD > 0)
            {
                dbms.DoExecuteSQL("UPDATE TASKS SET PERSONEL = " + System.Convert.ToString(per) + ",STATUS = 1 WHERE IDNUM = " + System.Convert.ToString(MIDD));
            }
            else
            {
                // کد اشتباه اینجا قبلا این بود : Baseknow.USERCOD که حذف شد
                td = DateTime.Now;
                //string? HES_COMPCODE = dbms.DoGetDataSQL<string>($"SELECT TOP 1 HES FROM SALA_DTL WHERE IDD = {Baseknow.USERCOD}").FirstOrDefault();

                dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + per + ",'" + UCurrentUser() + "'," + sha + "," + FARSIDATE() + "," + System.Convert.ToString(DateAndTime.Hour(DateTime.Now) * 100 + DateAndTime.Minute(DateTime.Now)) + "," + System.Convert.ToString(tg) + "," + System.Convert.ToString(numb) + "," + System.Convert.ToString(tg) + "," + "GETDATE()" + "," + Baseknow.USERCOD + " )");
                //dbms.DoExecuteSQL("insert into tasks(PERSONEL,USERNAME,TASK,COMP_COD,STDATE,STTIME,SKID,NUM,TG,CTIM,USERCO)  values (" + per + ",'" + UCurrentUser() + "'," + sha + "," + FARSIDATE() + "," + System.Convert.ToString(DateAndTime.Hour(DateTime.Now) * 100 + DateAndTime.Minute(DateTime.Now)) + "," + System.Convert.ToString(tg) + "," + System.Convert.ToString(numb) + "," + System.Convert.ToString(tg) + "," + "GETDATE()" + "," + Baseknow.USERCOD + " )");
                MIDD = System.Convert.ToDouble(Gettaskid(numb, tg));
            }
            dbms.DoExecuteSQL("insert into events(IDNUM,USERNAME,EVENTS,STDATE,STTIME,SKID,NUM,TG)  values (" + System.Convert.ToString(MIDD) + ",'" + UCurrentUser() + "','" + "ارجاع شد به  : " + GETUSERNAME(per) + "'," + FARSIDATE() + "," + System.Convert.ToString(DateAndTime.Hour(DateTime.Now) * 100 + DateAndTime.Minute(DateTime.Now)) + "," + System.Convert.ToString(tg) + "," + System.Convert.ToString(numb) + "," + System.Convert.ToString(tg) + " )");
            return MIDD;
        }


        public static bool Isbargiried(long fn)
        {
            var RST = dbms.DoGetDataSQL<CUSTOM_HEADLST2>("SELECT     NUMBER , TAG,tamir  FROM dbo.HEAD_LST WHERE     TAG = 2 AND NUMBER = " + System.Convert.ToString(fn)).FirstOrDefault();
            if (!(RST is null))
            {
                if (RST.tamir == -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static double LASTAVRAGE(string CODE, long ANBAR, long dt)
        {
            double tempLASTAVRAGE = 0;
            tempLASTAVRAGE = 0;
            string test = Baseknow.OPTIONSS;
            string test2 = Baseknow.OPTIONSS.Substring(55, 1);

            if (Baseknow.OPTIONSS.Substring(55, 1) != "5")
            {
                tempLASTAVRAGE = GETSTANDARDPRICE_KOL(CODE, dt);
            }
            if (tempLASTAVRAGE == 0)
            {
                //var RRST = dbms.DoGetDataSQL<QRE_19>("SELECT TOP 1 PERCENT ANBAR, CODE, DATE_N, avrage, TAG, NUMBER FROM         dbo.KA_KH(800101) KA_KH  WHERE     (CODE = N'" + CODE + "') AND (ANBAR = " + ANBAR + ")AND (DATE_N <= " + dt + ") AND (TAG = 0 OR  TAG = 1 OR TAG = 7 OR TAG = 9 OR TAG = 6 ) ORDER BY DATE_N DESC, NUMBER DESC").FirstOrDefault();
                var RRST = dbms.DoGetDataSQL<QRE_19>("SELECT ANBAR, CODE, DATE_N, avrage, TAG, NUMBER FROM         dbo.KA_KH(800101) KA_KH  WHERE     (CODE = N'" + CODE + "') AND (ANBAR = " + ANBAR + ")AND (DATE_N <= " + dt + ") AND TAG IN (0, 1, 7, 9, 6) ORDER BY DATE_N DESC, NUMBER DESC").FirstOrDefault();
                if (!(RRST is null) && !(IsNull(RRST.avrage)))
                {
                    tempLASTAVRAGE = (double)RRST.avrage;
                }
                else
                {
                    var RRST2 = dbms.DoGetDataSQL<QRE_20>("SELECT TOP 1 PERCENT dbo.INVO_LST.AVRAGE2, dbo.INVO_LST.CODE, dbo.INVO_LST.ANBAR, dbo.INVO_LST.NUMBER      AS MNN FROM dbo.INVO_LST INNER JOIN  dbo.HEAD_LST ON dbo.INVO_LST.NUMBER = dbo.HEAD_LST.NUMBER AND dbo.INVO_LST.TAG = dbo.HEAD_LST.TAG WHERE (dbo.HEAD_LST.DATE_N <= " + dt + ") AND (dbo.INVO_LST.CODE = '" + CODE + "') AND (dbo.INVO_LST.ANBARF = " + ANBAR + ")  ORDER BY dbo.HEAD_LST.DATE_N DESC, dbo.INVO_LST.NUMBER DESC").FirstOrDefault();
                    if (!(RRST2 is null) && !(IsNull(RRST2.AVRAGE2)))
                    {
                        tempLASTAVRAGE = (double)RRST2.AVRAGE2;
                    }
                    else
                    {
                        var RRST3 = dbms.DoGetDataSQL<double?>("SELECT FI_A FROM  dbo.STUF_FSK WHERE (CODE =  '" + CODE + "') AND (ANBAR = " + ANBAR + ")").FirstOrDefault();
                        if (!(RRST3 is null) && !(IsNull(RRST3)))
                        {
                            //tempLASTAVRAGE = RRST3.Fields["FI_A"];
                            tempLASTAVRAGE = (double)RRST3;
                        }
                        else
                        {
                            tempLASTAVRAGE = 0;
                        }
                    }
                }
            }

            return tempLASTAVRAGE;
        }
        public static double GETSTANDARDPRICE_KOL(string CODE, long dt)
        {
            double tempGETSTANDARDPRICE_KOL = 0;
            double fnum = 0;
            fnum = GETLASTFR(CODE, dt);
            if (fnum == 0)
            {
                var RST = dbms.DoGetDataSQL<QRE_18>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "') GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ORDER BY dbo.HEAD_MANF.FNUMB").FirstOrDefault();
                if (!(RST is null))
                {
                    tempGETSTANDARDPRICE_KOL = (double)(RST.SumOfMABLK + RST.SumOfIMBIBE_MANF + RST.SumOfIMBIBE_SAR);
                }
                else
                {
                    tempGETSTANDARDPRICE_KOL = 0;
                }
            }
            else
            {
                var RST = dbms.DoGetDataSQL<QRE_18>("SELECT     TOP 100 PERCENT SUM(dbo.DTL_MANF.MABLK) AS SumOfMABLK, dbo.HEAD_MANF.IMBIBE_MANF AS SumOfIMBIBE_MANF,  dbo.HEAD_MANF.IMBIBE_SAR AS SumOfIMBIBE_SAR, dbo.HEAD_MANF.FNUMB FROM dbo.HEAD_MANF INNER JOIN  dbo.DTL_MANF ON dbo.HEAD_MANF.FNUMB = dbo.DTL_MANF.FNUMB WHERE     (dbo.HEAD_MANF.CODE = N'" + CODE + "' and dbo.HEAD_MANF.FNUMB = " + fnum + ") GROUP BY dbo.HEAD_MANF.IMBIBE_MANF, dbo.HEAD_MANF.IMBIBE_SAR, dbo.HEAD_MANF.FNUMB ").FirstOrDefault();
                if (!(RST is null))
                {
                    tempGETSTANDARDPRICE_KOL = (double)(RST.SumOfMABLK + RST.SumOfIMBIBE_MANF + RST.SumOfIMBIBE_SAR);
                }
                else
                {
                    tempGETSTANDARDPRICE_KOL = 0;
                }
            }
            return tempGETSTANDARDPRICE_KOL;
        }

        public static string Getjobname(string Job_Code)
        {
            string tempGetjobname = null;
            var rst = dbms.DoGetDataSQL<string>("select Job_Desc from tab_job where Job_Code = '" + Job_Code + "'").FirstOrDefault();
            if (!string.IsNullOrEmpty(rst))
            {
                tempGetjobname = rst;
            }
            else
            {
                tempGetjobname = " ";
            }
            return tempGetjobname;
        }
        public static bool CHEKDATEM(long dt, bool flag)
        {
            bool tempCHEKDATEM = false;
            if (!(CHEKDATE(dt, flag)))
            {
                Msgwin msgwin = new Msgwin(false, "تاریخ صحیح نیست!"); msgwin.ShowDialog();
                tempCHEKDATEM = true;
            }
            else
            {
                tempCHEKDATEM = false;
            }
            return tempCHEKDATEM;
        }
        public static string UDay(string BGDT)
        {
            //1400/12/24  0 ... 7 Y:0-3  M: 4-5  D:6-7
            BGDT = BGDT.Substring(6, 2);
            return BGDT;
        }
        public static string UMonth(string BGDT)
        {
            //1400/12/24  0 ... 7 Y:0-3  M: 4-5  D:6-7
            BGDT = BGDT.Substring(4, 2);
            return BGDT;
        }
        public static string UYear(string BGDT)
        {
            //1400/12/24  0 ... 7 Y:0-3  M: 4-5  D:6-7
            BGDT = BGDT.Substring(0, 4);
            return BGDT;
        }
        public static bool CHEKDATE(long dt, bool flag)
        {
            bool tempCHEKDATE = false;
            if (Tarikh.IsValidedDate(dt.ToString()) is true)
                tempCHEKDATE = true;
            else
                tempCHEKDATE = false;

            if (tempCHEKDATE == true && flag)
            {
                if (UYear(dt.ToString()) != Baseknow.YEA.ToString())
                {
                    tempCHEKDATE = false;
                    Msgwin msgwin = new Msgwin(false, "تاریخ مربوط به سال مالی جاری, نمی باشد"); msgwin.ShowDialog();
                }
            }
            return tempCHEKDATE;
        }
        public static void ADDTAKH(long CUST_KIND, long NUMBER, long KIND)
        {
            if (Strings.Mid(Baseknow.OPTIONSS, 58, 1) == "5")
            {
                dbms.DoExecuteSQL("DELETE FROM dbo.TAKHFIF_APLAY WHERE (NUMBER = " + NUMBER + ") AND (KIND = " + KIND + ")");

                //var RST2 = dbms.DoGetDataSQL<TAKHFIF_APLAY_CSHARP>("SELECT * FROM TAKHFIF_APLAY").ToList();

                var rst = dbms.DoGetDataSQL<int?>("SELECT TID FROM dbo.CUSTKIND_TF WHERE  (ACTIV = 0) AND (CUST_COD = " + CUST_KIND + ")").ToList();
                for (int i = 0; i < rst.Count; i++)
                {
                    var teest = $@"INSERT INTO dbo.TAKHFIF_APLAY(TID, NUMBER, KIND, CRT)
                                                       VALUES({rst.FirstOrDefault()},{NUMBER} ,{KIND} ,GETDATE())";

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.TAKHFIF_APLAY(TID, NUMBER, KIND, CRT)
                                                       VALUES({rst.FirstOrDefault()},{NUMBER} ,{KIND} ,GETDATE())");
                }
            }
        }
        public static void APLAYTAKH(long NUMBER, long KIND, double M_NAGHD, double MABL_VAR, double MABL_HAV, bool TICMBAA)
        {
            var JAMMAB = default(double);
            double per;
            bool OOK;
            double JAMF;
            double JAMCH;
            if (Strings.Mid(Baseknow.OPTIONSS, 58, 1) == "5")
            {
                if (NUMBER != 0)
                {
                    var JST0 = dbms.DoGetDataSQL<double?>("SELECT  SUM(MABL_K - ROUND(N_KOL/100 * MABL_K, 0))  AS SumOfMABL_K FROM INVO_LST WHERE (((INVO_LST.NUMBER)= " + NUMBER + " ) AND ((INVO_LST.TAG)=" + KIND + "))").FirstOrDefault();
                    if (!(JST0 is null) && !IsNull(JST0))
                    {
                        JAMF = (double)JST0;
                    }
                    else
                    {
                        JAMF = 0d;
                    }
                    ;
                    var JST = dbms.DoGetDataSQL<double?>("SELECT Sum(PAY_GETD.MABL) AS SumOfMABL FROM PAY_GETD WHERE (PAY_GETD.DATE >= PAY_GETD.DATE_S AND PAY_GETD.TAG = " + KIND + " AND PAY_GETD.NUMBER= " + NUMBER + " )").FirstOrDefault();
                    if (!(JST is null))
                    {
                        JAMCH = (double)JST;
                    }
                    else
                    {
                        JAMCH = 0d;
                    }
                    per = 0d;
                    var rst = dbms.DoGetDataSQL<TAKHFIF_APLAY_1>("SELECT     dbo.TAKHFIF_APLAY.*, dbo.TAKHFIF_DEF.TKIND FROM dbo.TAKHFIF_APLAY INNER JOIN  dbo.TAKHFIF_DEF ON dbo.TAKHFIF_APLAY.TID = dbo.TAKHFIF_DEF.TID WHERE (NUMBER = " + NUMBER + ") AND (KIND = " + KIND + ")").ToList();
                    for (int i = 0; i < rst.Count; i++)
                    {
                        switch (rst[i].TKIND)
                        {
                            case 1:
                                {
                                    JAMMAB = M_NAGHD + MABL_VAR + MABL_HAV + JAMCH;
                                    break;
                                }
                            case 2:
                                {
                                    JAMMAB = JAMF;
                                    break;
                                }
                        }
                        var RST2 = dbms.DoGetDataSQL<_QRE_1>("SELECT     TID, AZMAB, TAMAB, TPERS, TMAB FROM dbo.TAKHFIF_DEF_DTL WHERE     TID = " + rst[i].TID).ToList();
                        OOK = false;
                        for (int u = 0; u < RST2.Count; u++)
                        {
                            if (JAMMAB >= RST2[u].AZMAB && JAMMAB <= RST2[u].TAMAB)
                            {
                                if (JAMF > 0d)
                                {
                                    per = (double)(per + RST2[u].TPERS * JAMMAB / JAMF);
                                    OOK = true;
                                }
                                else
                                {
                                    per = (double)(per + RST2[u].TPERS);
                                    OOK = true;
                                }
                            }
                            //RST2.MoveNext();
                        }
                        ;
                        //rst.MoveNext();
                    }
                    if (JAMF > 0d)
                    {
                        var RST2 = dbms.DoGetDataSQL<_QRE_2>("SELECT     TKHN,code,megh,meghk,mabl,mabl_k,n_kol,n_moin,imbaa,id FROM dbo.invo_lst WHERE     tag = " + KIND + " and NUMBER =  " + NUMBER).ToList();

                        for (int i = 0; i < RST2.Count; i++)
                        {
                            RST2[i].TKHN = per;
                            RST2[i].n_moin = Math.Round((double)(RST2[i].n_kol * RST2[i].mabl_k / 100)) + Math.Round((double)((RST2[i].mabl_k - Math.Round((double)(RST2[i].n_kol * RST2[i].mabl_k / 100))) * per / 100));
                            if (TICMBAA)
                            {
                                var rsts = dbms.DoGetDataSQL<CUSTOM_STUF_DEF_2>("select CMBAA ,code from STUF_DEF where code = '" + RST2[i].code + "'").FirstOrDefault();
                                if (!(rsts is null))
                                {
                                    if ((bool)rsts.CMBAA)
                                    {
                                        double arzesh = GetArzesh(rsts.code);
                                        double imbaa = (Math.Floor((double)((RST2[i].mabl_k - RST2[i].n_moin) * arzesh / 100)));
                                        //      If RST2.Fields("IMBAA") <> Fix((RST2.Fields("MABL_K") - RST2.Fields("N_MOIN")) * GetArzesh(rst.Fields("code")) / 100) 
                                        if (RST2[i].imbaa != imbaa)
                                        {
                                            RST2[i].imbaa = imbaa;
                                        }
                                    }
                                    else if (RST2[i].imbaa != 0)
                                    {
                                        Msgwin msgwin = new Msgwin(true, "اين كالا در تعريف كالا مشمول ماليات معرفي نشده است آيا ماليات آنرا صفر كنم؟");
                                        msgwin.ShowDialog();
                                        if (msgwin.DialogResult is true)
                                        {
                                            RST2[i].imbaa = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RST2[i].imbaa = 0;
                            }
                            //RST2.update();
                            var _where = " WHERE  TAG = " + KIND + " AND NUMBER =  " + NUMBER + "AND id = " + RST2[i].id;
                            dbms.DoExecuteSQL($"UPDATE dbo.INVO_LST SET TKHN = {RST2[i].TKHN},N_MOIN = {RST2[i].n_moin},IMBAA={RST2[i].imbaa} {_where}");
                        }
                    }
                    else
                    {
                        // If KIND = 14 Then
                        // Set rst2 = New ADODB.Recordset
                        // rst2.Open "SELECT NUMBER, TAG, TAKHFIF FROM dbo.HEAD_LST WHERE (TAG = 14)  and NUMBER =  " & NUMBER, CurrentProject.Connection, adOpenKeyset, adLockOptimistic
                        // If rst2.RecordCount > 0 Then
                        // rst2.Fields("TAKHFIF") = Round(PER * JAMMAB / 100)
                        // rst2.update
                        // End If
                        // rst2.Close
                        // End If
                    }
                }
            }
        }
        public static void RunForm(string frm)
        {
            if (CL_HESABDARI.LETSGO(frm))
            {
                switch (frm ?? "")
                {
                    // منوي تعاريف--------------------------------------------------------------
                    case "BANKS":
                        {
                            CL_LMethods.OpenWindow(null, new TCOD_BANKS_WIN(), isModalDialog: false, allowMultipleInstances: true);
                            break;
                        }
                    case "ANBAR":
                        {
                            CL_LMethods.OpenWindow(null, new TCOD_ANBAR_WIN(), isModalDialog: false, allowMultipleInstances: true);
                            break;
                        }
                    case "KALA":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "STUF_DEF_WIN") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#STUF_DEF_WIN"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else

                            new STUF_DEF_WIN().Show();
                            break;
                        }
                    case "GROUP":
                        {
                            new TCOD_STUFGROUP_WIN().Show();
                            break;
                        }
                    case "VAHED":
                        {
                            new TCOD_VAHEDS_WIN().Show();
                            break;
                        }
                    case "ANBARIN":
                        {
                            //DoCmd.OpenForm("TCOD_ANBARINI", acFormDS);
                            new TCOD_ANBAR_WIN().Show();
                            break;
                        }
                    case "HESDEF":
                        {
                            new TOTA_HES_SHEET_WIN().Show();
                            break;
                        }
                    case "AUTO":
                        {
                            //DoCmd.OpenForm("AUTOMATIC", acNormal);
                            break;
                        }
                    case "MOSHTARI":
                        {
                            new FCODE_CUSTOMER().Show();
                            break;
                        }
                    case "GHES":
                        {
                            new TCOD_HESGROUP_WIN().Show();
                            break;
                        }
                    case "CUST":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "CUSTKIND_WIN") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#CUSTKIND_WIN"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else

                            new CUSTKIND_WIN().Show();
                            break;
                        }
                    case "DEPART":
                        {
                            new DEPART_WIN().Show();
                            break;
                        }
                    case "SHIFT":
                        {
                            new SHIFT_WIN().Show();
                            break;
                        }
                    case "KHAD":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "KHAD_DEF") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#KHAD_DEF"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else

                            new KHAD_DEF().Show();
                            break;
                        }

                    case "AZAE":
                        {
                            new AZAE_WIN().Show();
                            break;
                        }
                    case "TMHAZ":
                        {
                            //DoCmd.OpenForm("TCOD_MARKAZHAZ", acFormDS);
                            break;
                        }
                    case "MENUIT":
                        {
                            //DoCmd.OpenForm("TCOD_MENUITEM");
                            break;
                        }
                    case "TAKHDEF":
                        {
                            //DoCmd.OpenForm("TAKHFIF_DEF_FORM");
                            break;
                        }
                    case "CUSTDEF":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "FCODE_CUSTOMER") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#FCODE_CUSTOMER"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else
                            //DoCmd.OpenForm("FCODE_CUSTOMER_CUST");
                            break;
                        }
                    case "GRFORMAT":
                        {
                            new GRADE_FORMAT_WIN().Show();
                            break;
                        }
                    case "GSCALE":
                        {
                            //DoCmd.OpenForm("GSCALE");
                            break;
                        }
                    case "GGRAD":
                        {
                            //DoCmd.OpenForm("GRADE_RANGE_FORM");
                            break;
                        }
                    case "GGRSH":
                        {
                            //DoCmd.OpenForm("GRADE_SHART_FUNC_FORM", acFormDS);
                            break;
                        }
                    case "PRGRP":
                        {
                            //DoCmd.OpenForm("PRICE_GRP_FORM", acFormDS);
                            break;
                        }
                    case "PRPAYNO":
                        {
                            //DoCmd.OpenForm("PRICE_PAYNO_FORM", acFormDS);
                            break;
                        }
                    case "PRELMTF":
                        {
                            //DoCmd.OpenForm("PRICE_ELAMIETF_FORM");
                            break;
                        }
                    case "PRELMPR":
                        {
                            //DoCmd.OpenForm("PRICE_ELAMIE_FORM");
                            break;
                        }
                    case "TEL":
                        {
                            //DoCmd.OpenForm("TEL", default, default, default, default, acDialog);
                            break;
                        }
                    case "BGU":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "BUDGET0") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#BUDGET0"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else
                            //DoCmd.OpenForm("BUGET0", default, default, default, default, acDialog);

                            new BUDGET0().Show();
                            break;
                        }

                    case "BGLIST":
                        {
                            //DoCmd.OpenForm("BUGET_MAIN_list", acFormDS);
                            break;
                        }
                    case "BGLISTTL":
                        {
                            //DoCmd.OpenForm("BUGET_MAIN_list2", acFormDS);

                            new BUGET_MAIN_list2().Show();
                            break;
                        }
                    // منوي حسابداري-----------------------------------------------------------
                    case "SANAD":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "SANAD") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#SANAD"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // 
                            // Else
                            //DoCmd.OpenForm("DEED_HEAD", acNormal);

                            new DEED_HEAD().Show();
                            break;
                        }

                    case "KHAZANEH":
                        {
                            new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED().Show();
                            break;
                        }
                    case "PGETD":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "KHAZANEH") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#KHAZANEH"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // 
                            // Else
                            new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED().Show();
                            break;
                        }

                    case "OFDP":
                        {
                            //DoCmd.OpenForm("OPGET_HED", acNormal);
                            break;
                        }
                    case "OFDPSN":
                        {
                            //DoCmd.OpenForm("ENTEGHAL_DARYAFT_PARDAKHT", default, default, default, default, acDialog);
                            break;
                        }
                    case "TAEED":
                        {
                            //DoCmd.OpenForm("F_MENU_ASNAD", acNormal);
                            break;
                        }
                    case "EFTE":
                        {
                            //DoCmd.OpenForm("SANAD_EFTETAHIYAH", acNormal);
                            break;
                        }
                    case "EKHTE":
                        {
                            //DoCmd.OpenForm("SANAD_EKHTETAMIYAH", acNormal);
                            break;
                        }
                    case "RDFTMOIN":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_DATE", acNormal);
                            break;
                        }
                    case "HTAF":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "F8") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#F8"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // 
                            // Else
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, default, "TAF");
                            break;
                        }

                    case "RDFTTFKMOIN":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_KOL_MOIN_TAFKIK", acNormal);
                            break;
                        }
                    case "DFTROOS":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "DFTROOS");
                            break;
                        }
                    case "DFTKOL":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_DATE", acNormal, default, default, default, acDialog);
                            break;
                        }
                    case "DPDAY":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "DPDAY");
                            break;
                        }
                    case "NEWYEAR":
                        {
                            //DoCmd.OpenForm("F_NEWYEAR", acNormal, default, default, default, acDialog);
                            break;
                        }
                    case "SELYEAR":
                        {
                            //DoCmd.OpenForm("F_SELECTYEAR", acNormal, default, default, default, acDialog);
                            break;
                        }
                    case "DPDAYASH":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "DP");
                            break;
                        }
                    case "TARAZ4":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "FT4");
                            break;
                        }
                    case "RTARAZ4":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "RT4");
                            break;
                        }
                    case "TARAZ4M":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "FT4M");
                            break;
                        }
                    case "TARAZ4M2":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "FT4M2");
                            break;
                        }
                    case "TARAZ4T":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "FT4T");
                            break;
                        }
                    case "RTARAZ4T":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "RFT4T");
                            break;
                        }
                    case "RTARAZ4T2":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "RFT4T2");
                            break;
                        }
                    case "OTHER":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "OTHER");
                            break;
                        }
                    case "SNDHES":
                        {
                            //DoCmd.OpenForm("DEED_SEARCH", acFormDS);
                            break;
                        }
                    case "BEDBES":
                        {
                            //DoCmd.OpenForm("BEDEHKARAN_BESTANKARAN", acFormDS);
                            break;
                        }
                    case "GETS":
                        {
                            //DoCmd.OpenForm("PGET_LST_SEARCH", acFormDS);
                            break;
                        }
                    case "ASNDSO":
                        {
                            //DoCmd.OpenForm("SORTASNAD", default, default, default, default, acDialog);
                            break;
                        }
                    case "SUDZIAN":
                        {
                            //DoCmd.OpenReport("SOUDVAZIAN", acViewPreview);
                            break;
                        }
                    case "SUD":
                        {
                            //DoCmd.OpenForm("SOUD", acNormal, default, default, default, acDialog);
                            break;
                        }
                    case "SMOGH":
                        {
                            //DoCmd.OpenForm("F_MENU_SANAD_MOGHA", default, default, default, default, acDialog, "AM");
                            break;
                        }
                    case "SMOGHFR":
                        {
                            //DoCmd.OpenForm("F_MENU_SANAD_MOGHA", default, default, default, default, acDialog, "FR");
                            break;
                        }
                    case "TKHARED":
                        {
                            //DoCmd.OpenForm("HEAD_LST_KHAREED_SND", acNormal);
                            break;
                        }
                    case "TFRSND":
                        {
                            //DoCmd.OpenForm("HEAD_LST_FROOSH_SND", acNormal);
                            break;
                        }
                    case "SNDPR":
                        {
                            //DoCmd.OpenForm("F_MENU_ASNAD_PRINT", acNormal);
                            break;
                        }
                    case "SPRIN":
                        {
                            //DoCmd.OpenForm("F_MENU_PRINTHES", acNormal);
                            break;
                        }
                    case "MOGH":
                        {
                            //DoCmd.OpenForm("ADAMTARAZ", acFormDS);
                            break;
                        }
                    case "SKOL":
                        {
                            //DoCmd.OpenForm("TOTA_HES_SHEET", acFormDS);

                            new TOTA_HES_SHEET_WIN().Show();
                            break;
                        }
                    case "FKHAD":
                        {
                            //DoCmd.OpenForm("HEAD_LST_KHADAMAT");
                            break;
                        }
                    case "MOS":
                        {
                            //DoCmd.OpenForm("HEAD_LST_MOSTAGHIM");
                            break;
                        }
                    case "MOGHA":
                        {
                            //DoCmd.OpenForm("F_MENU_MOGHAYERAT");
                            break;
                        }
                    case "SMOGHA":
                        {
                            //DoCmd.OpenForm("MOGHAYERAT");
                            break;
                        }
                    case "OSND":
                        {
                            //DoCmd.OpenForm("ODEED_HEAD");
                            break;
                        }
                    case "OSNDSER":
                        {
                            //DoCmd.OpenForm("ENTEGHAL_ASNAD_HESABDARY", default, default, default, default, acDialog);
                            break;
                        }
                    case "GRFRO":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRFRO");
                            break;
                        }
                    case "GRKHO":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRKHO");
                            break;
                        }
                    case "GRVRO":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRVRO");
                            break;
                        }
                    case "GRKHS":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRKHS");
                            break;
                        }
                    case "GRENT":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRENT");
                            break;
                        }
                    case "GRKHA":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRKHA");
                            break;
                        }
                    case "GRKHZ":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRKHZ");
                            break;
                        }
                    case "GRVDS":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRVDS");
                            break;
                        }
                    case "GRKHR":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRKHR");
                            break;
                        }
                    case "GRANB":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GRANB");
                            break;
                        }
                    case "GENFRBR":
                        {
                            //DoCmd.OpenForm("GENSANADFROOSH", default, default, default, default, acDialog, "GENFRBR");
                            break;
                        }
                    case "BAZAV":
                        {
                            //DoCmd.OpenForm("BAZ_AVRAGE", default, default, default, default, acDialog);
                            break;
                        }
                    case "SKHOA":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "SKHOA");
                            break;
                        }
                    case "RMOIN":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, default, "RMOIN");
                            break;
                        }
                    case "TNAM":
                        {
                            //DoCmd.OpenForm("F_MENU_SND");
                            break;
                        }
                    case "TNAMH":
                        {
                            //DoCmd.OpenForm("TARAZHES", acFormDS);
                            break;
                        }
                    case "SERP":
                        {
                            //DoCmd.OpenForm("DEED_SEARCH_MAIN");
                            break;
                        }
                    case "DMSGH":
                        {
                            //DoCmd.OpenForm("GETFIRSTMOG");
                            break;
                        }
                    case "HBGHB":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "HBGHB");
                            break;
                        }
                    case "MARKAZ":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "MARKAZ");
                            break;
                        }
                    case "BEDBESM":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, acDialog, "BEDBESM");
                            break;
                        }
                    case "VISITORS":
                        {
                            //DoCmd.OpenForm("VISITOR_DTL_SUB", acFormDS, default, default, default, acDialog, 1);
                            break;
                        }
                    case "BARGI":
                        {
                            //DoCmd.OpenForm("OTHER_DTL_SUB", default, default, default, default, acDialog, 1);
                            break;
                        }
                    // Case "FRRDTL":      //DoCmd.OpenForm "F_MENU_GOZARESH_FROOSH", acNormal, , , , acDialog, "FR"
                    case "VISITDLV":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_visit", acNormal, default, default, default, acDialog, "VISITDLV");
                            break;
                        }
                    // Case "FAM":         //DoCmd.OpenForm "FACTOR_MAND", acFormDS
                    case "VISITKAL":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_visit", acNormal, default, default, default, acDialog, "VISITKAL");
                            break;
                        }
                    case "VISITKALMA":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_visit", acNormal, default, default, default, acDialog, "VISITKALMA");
                            break;
                        }
                    case "VISITROUT":
                        {
                            //DoCmd.OpenForm("Visit_route_FORM", acNormal);
                            break;
                        }
                    case "VISITGOL":
                        {
                            //DoCmd.OpenForm("Visit_GOL_FORM", acNormal);
                            break;
                        }
                    case "VISITGOLMON":
                        {
                            //DoCmd.OpenForm("VISITOR_GOL_REP", acNormal);
                            break;
                        }
                    case "VISITGOLMONR":
                        {
                            //DoCmd.OpenForm("VISITOR_GOL_REP_MAR", acNormal);
                            break;
                        }
                    case "VISITGOLG":
                        {
                            //DoCmd.OpenForm("Visit_GOL_GRP_FORM", acNormal);
                            break;
                        }
                    case "VISITGOLMONG":
                        {
                            //DoCmd.OpenForm("VISITOR_GOL_GRP_REP", acNormal);
                            break;
                        }
                    case "VISITGOLMONGR":
                        {
                            //DoCmd.OpenForm("VISITOR_GOL_GRP_REP_MAR", acNormal);
                            break;
                        }
                    case "VISIT_DAY":
                        {
                            //DoCmd.OpenForm("VISITORS_DAY_HEAD", acNormal);
                            break;
                        }
                    case "DKOLKH":
                        {
                            //DoCmd.OpenReport("R_DAFTAR_KOL_kh", acViewPreview);
                            break;
                        }

                    case "HESMAI":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_DATE_AI");
                            break;
                        }
                    case "MANABE":
                        {
                            //DoCmd.OpenForm("FMENU_TARAZ_4", acNormal, default, default, default, acDialog, "MANABE");
                            break;
                        }
                    case "CUSTNP":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, default, "CUSTNP");
                            break;
                        }
                    case "tozied":
                        {
                            //DoCmd.OpenForm("tozie");
                            break;
                        }
                    case "FASLIBR":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "FASLIBR");
                            break;
                        }
                    case "CKRALL":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "CKRALL");
                            break;
                        }
                    case "PAYORD":
                        {
                            //DoCmd.OpenForm("paymentformorder", acNormal);
                            break;
                        }
                    case "PAYORDL":
                        {
                            //DoCmd.OpenForm("paymentformorder_LIST", acFormDS);
                            break;
                        }
                    case "NABZMOSH":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, default, "NABZMOSH");
                            break;
                        }
                    case "NABZKAR":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, default, "NABZKAR");
                            break;
                        }
                    case "JAYGOZIN":
                        {
                            //DoCmd.OpenForm("ZASESABBEESAB", default, default, default, default, acDialog, "NABZKAR");
                            break;
                        }
                    // منوي توليد-----------------------------------------------------------
                    case "TOLID":
                        {
                            //DoCmd.OpenForm("HEAD_MAN", acNormal);
                            break;
                        }
                    case "FORMOL":
                        {
                            //DoCmd.OpenForm("HEAD_MANF", acNormal);
                            break;
                        }
                    case "HAVT":
                        {
                            //DoCmd.OpenForm("HAVLAH_KHORUG", acNormal, default, default, acFormReadOnly);
                            break;
                        }
                    case "HEXIT":
                        {
                            //DoCmd.OpenForm("HAVALAH_EXIT", acNormal);
                            break;
                        }
                    case "HENTER":
                        {
                            //DoCmd.OpenForm("HAVALAH_ENTER", acNormal);
                            break;
                        }
                    case "MARM":
                        {
                            //DoCmd.OpenForm("MASRAF_CENTER", acNormal);
                            break;
                        }
                    case "HSAYER":
                        {
                            //DoCmd.OpenForm("HAVALAH_EXIT_SAYER", acNormal);
                            break;
                        }
                    case "JAZBD":
                        {
                            //DoCmd.OpenForm("AONE_SALARY", acNormal);
                            break;
                        }
                    case "KARB":
                        {
                            //DoCmd.OpenForm("KAR_HEAD", acNormal);
                            break;
                        }
                    case "AMTOL":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, acDialog, "AMTOL");
                            break;
                        }
                    case "AMMAS":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, acDialog, "AMMAS");
                            break;
                        }
                    case "MOFGH":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, acDialog, "COMP");
                            break;
                        }
                    case "AMA2":
                        {
                            //DoCmd.OpenForm("SANADPAYANDORAH");
                            break;
                        }
                    // منوي خريد فروش-----------------------------------------------------------
                    case "VISITOR":
                        {
                            //DoCmd.OpenForm("F_MENU_AJNAS");
                            break;
                        }
                    case "CROSS":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "CROS");
                            break;
                        }
                    case "FACTFRSA":
                        {
                            //if (IsLoaded("HEAD_LST_FROOSH2_SADER"))
                            //{
                            //DoCmd.OpenForm("HEAD_LST_FROOSH2_SADER", acNormal);
                            //DoCmd.GoToRecord(acDataForm, "HEAD_LST_FROOSH2_SADER", acNewRec);
                            //Forms["HEAD_LST_FROOSH2_SADER"].Form["NUMBER1"].SetFocus();
                            //}
                            //else
                            {
                                //DoCmd.OpenForm("HEAD_LST_FROOSH2_SADER", acNormal);
                            }

                            break;
                        }
                    case "FACTFR":
                        {
                            if (Strings.Mid(Baseknow.OPTIONSS, 53, 1) == "5")
                            {
                                if (CL_HESABDARI.LETSGO("FRMOST"))
                                {
                                    // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "HEAD_LST_FROOSH22") = 1 Then
                                    // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#HEAD_LST_FROOSH22"
                                    // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                                    // 
                                    // Else
                                }

                                //new HEAD_LST_FROOSH22(null, false).Show(); //حواله ای
                                CL_LMethods.OpenWindow(null, new HEAD_LST_FROOSH22(null, false), isModalDialog: false, allowMultipleInstances: true);
                                ////DoCmd.OpenForm("HEAD_LST_FROOSH2", acNormal);
                            }
                            else if (Strings.Mid(Baseknow.OPTIONSS, 18, 1) != "5")
                            {
                                if (Baseknow.TKHF < 2)
                                {
                                    //DoCmd.OpenForm("HEAD_LST_FROOSH1", acNormal);

                                }
                                else
                                {
                                    //new HEAD_LST_FROOSH22(null, false).Show(); //حواله ای
                                    //DoCmd.OpenForm("HEAD_LST_FROOSH2", acNormal);
                                    CL_LMethods.OpenWindow(null, new HEAD_LST_FROOSH22(null, false), isModalDialog: false, allowMultipleInstances: true);
                                }
                            }
                            //else if (IsLoaded("HEAD_LST_FROOSH22"))
                            //{
                            //DoCmd.OpenForm("HEAD_LST_FROOSH22", acNormal);
                            //DoCmd.GoToRecord(acDataForm, "HEAD_LST_FROOSH22", acNewRec);

                            //Forms["HEAD_LST_FROOSH22"].Form["CUST_NO"].SetFocus();
                            //if (Val(Strings.Mid(Baseknow.OPTIONSS, 11, 2)) == "21")
                            //{
                            //    Forms["HEAD_LST_FROOSH22"].Form["CUST_NO"] = Baseknow.["BEDEHKAR"] + "-1-1";
                            //    Forms["HEAD_LST_FROOSH22"].Form["INVO_LST_sub"].SetFocus();
                            //}
                            //}
                            else
                            {
                                //DoCmd.OpenForm("HEAD_LST_FROOSH22", acNormal);
                                //new HEAD_LST_FROOSH22(null, true).Show();
                                CL_LMethods.OpenWindow(null, new HEAD_LST_FROOSH22(null, true), isModalDialog: false, allowMultipleInstances: true);
                            }

                            break;
                        }
                    case "MFACTFR":
                        {
                            if (Baseknow.TKHF < 2)
                            {
                                //DoCmd.OpenForm("MHEAD_LST_FROOSH", acNormal);
                            }
                            else
                            {
                                //DoCmd.OpenForm("MHEAD_LST_FROOSH2", acNormal);
                            }

                            break;
                        }
                    case "CFRALL":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "CFRALL");
                            break;
                        }
                    case "FACTKH":
                        {
                            //DoCmd.OpenForm("HEAD_LST_KHAREED1", acNormal);
                            break;
                        }
                    case "FACTKHSA":
                        {
                            //DoCmd.OpenForm("HEAD_LST_KHAREED1_SADER", acNormal);
                            break;
                        }
                    case "PFACTFR":
                        {
                            new HEAD_LST_PISHFROOSH2().Show();
                            break;
                        }
                    case "FACTBFR":
                        {
                            if (Baseknow.TKHF < 2)
                            {
                                //DoCmd.OpenForm("HEAD_LST_FROOSH_BACK", acNormal);
                            }
                            else
                            {
                                //DoCmd.OpenForm("HEAD_LST_FROOSH_BACK2", acNormal);
                            }

                            break;
                        }
                    case "FACTBKH":
                        {
                            //DoCmd.OpenForm("HEAD_LST_KH_BACK", acNormal);
                            break;
                        }
                    case "SEFARESH":
                        {
                            //DoCmd.OpenForm("ORDR_HED", acNormal);
                            break;
                        }
                    case "FROOSHDAY":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "F");
                            break;
                        }
                    case "FROOSHDAY2":
                        {
                            //DoCmd.OpenForm("F_MENU_GOZARESH_FROOSH", acNormal, default, default, default, acDialog, "FK");
                            break;
                        }
                    case "FKHAREDAY":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "K");
                            break;
                        }
                    case "GARDESH":
                        {
                            //DoCmd.OpenForm("KALAS", acFormDS);
                            break;
                        }
                    case "STUFLIST":
                        {
                            //DoCmd.OpenForm("STUF_DEF_LIST", acFormDS);
                            new STUF_DEF_WIN().Show();
                            break;
                        }
                    case "FLIST":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "FLIST");
                            break;
                        }
                    case "KLIST":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "KLIST");
                            break;
                        }
                    case "LFRAN":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_FRKH", acNormal, default, default, default, acDialog, "RF");
                            break;
                        }
                    case "LKHAN":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_FRKH", acNormal, default, default, default, acDialog, "RK");
                            break;
                        }
                    case "LPF":
                        {
                            //DoCmd.OpenForm("LASTPRICE");
                            break;
                        }
                    case "VAZ":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, acDialog, "VAZ");
                            break;
                        }
                    case "FRROOZU":
                        {
                            //DoCmd.OpenForm("F_MENU_GOZARESH_FROOSH", acNormal, default, default, default, acDialog, "F");
                            break;
                        }
                    case "LFACT":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, default, "LFACT");
                            break;
                        }
                    case "FGROO":
                        {
                            //DoCmd.OpenForm("F_MENU_FROOSH");
                            break;
                        }
                    case "RAAS":
                        {
                            //DoCmd.OpenForm("RAAS");
                            break;
                        }
                    case "OFACT":
                        {
                            //DoCmd.OpenForm("OHEAD_LST_FROOSH", acNormal);
                            break;
                        }
                    case "OFACTKH":
                        {
                            //DoCmd.OpenForm("OHEAD_LST_KHADAMAT", acNormal);
                            break;
                        }
                    case "CRREPF":
                        {
                            //DoCmd.OpenForm("HEAD_SERCH_MAIN", acNormal);
                            break;
                        }
                    case "AMFDAY":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "AMFDAY");
                            break;
                        }
                    case "FPDESIGN":
                        {
                            //DoCmd.OpenForm("FPFORMAT", acNormal);
                            break;
                        }
                    case "RORD":
                        {
                            //DoCmd.OpenForm("ORDR_HED1", acFormDS);
                            break;
                        }
                    case "FRCUST":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, default, "FRCUST");
                            break;
                        }
                    case "CRREPFA":
                        {
                            //DoCmd.OpenForm("HEAD_SERCH_MAIN_ADVANC", acNormal);
                            break;
                        }
                    case "SAVEREP":
                        {
                            //DoCmd.OpenForm("SQLSTATEFORM", acFormDS);
                            break;
                        }
                    case "DARKHR":
                        {
                            //DoCmd.OpenForm("HEAD_LST_REQUEST");
                            break;
                        }
                    case "DARKHRLIS":
                        {
                            //DoCmd.OpenForm("HAVLISTDAR", acFormDS);
                            break;
                        }
                    case "BRFR":
                        {
                            //DoCmd.OpenForm("HEAD_LST_BRFR");
                            break;
                        }
                    case var @case when @case == "BARGI":
                        {
                            //DoCmd.OpenForm("OTHER_DTL_SUB", default, default, default, default, acDialog, 1);
                            break;
                        }
                    case "RDFTKHFRA4":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_MOIN_TAFZIL", acNormal, default, default, default, acDialog, "FRKMA4");
                            break;
                        }
                    case "LFKBTF":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_HES", acNormal);
                            break;
                        }
                    case "LISTKHG":
                        {
                            //DoCmd.OpenForm("list_kharid", acFormDS);
                            break;
                        }
                    case "LISTFRG":
                        {
                            //DoCmd.OpenForm("LIST_FROOSH", acFormDS);
                            break;
                        }
                    case var case1 when case1 == "VISITORS":
                        {
                            //DoCmd.OpenForm("VISITOR_DTL_SUB", acFormDS, default, default, default, acDialog, 1);
                            break;
                        }
                    case "FRRDTL":
                        {
                            //DoCmd.OpenForm("F_MENU_GOZARESH_FROOSH", acNormal, default, default, default, acDialog, "FR");
                            break;
                        }
                    case "USERFR":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_US", acNormal);
                            break;
                        }
                    case "KHCUST":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, default, "KHCUST");
                            break;
                        }
                    case "SMS_VIP":
                        {
                            //DoCmd.OpenForm("SMS_VIP");
                            break;
                        }
                    case "VISIT_POR":
                        {
                            //DoCmd.OpenForm("VISITORS_PORSANT_HEAD");
                            break;
                        }
                    case "VISIT_MAS":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_visit", acNormal, default, default, default, acDialog, "VISIT_MAS");
                            break;
                        }
                    case "FONAR":
                        {
                            //DoCmd.OpenForm("froosh_naraftah", acFormDS);
                            break;
                        }
                    case "FONARP":
                        {
                            //DoCmd.OpenForm("F_MENU_KHFR", acNormal, default, default, default, acDialog, "FONARP");
                            break;
                        }
                    case "FRBKA":
                        {
                            //DoCmd.OpenForm("head_lst_brf_free");
                            break;
                        }
                    case var case2 when case2 == "CUSTNP":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, default, "CUSTNP");
                            break;
                        }
                    case "KHLS":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "KHLS");
                            break;
                        }
                    case "LVBF":
                        {
                            //DoCmd.OpenForm("flist_porsant_factors", acFormDS);
                            break;
                        }
                    case "SEARCHMO":
                        {
                            //DoCmd.OpenForm("MOGUDI_SEARCH_MAIN", acNormal);

                            new MOGUDI_SEARCH_MAIN().Show();
                            break;
                        }
                    case "VISITONE":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE_visit", acNormal, default, default, default, acDialog, "VISITONE");
                            break;
                        }

                    case "JOST1":
                        {
                            //DoCmd.OpenForm("JOST", default, default, default, default, default, 1);
                            break;
                        }
                    case "JOST2":
                        {
                            //DoCmd.OpenForm("JOST", default, default, default, default, default, 2);
                            break;
                        }
                    case "JOST3":
                        {
                            //DoCmd.OpenForm("JOST", default, default, default, default, default, 3);
                            break;
                        }
                    case "FASLIKHBR":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", acNormal, default, default, default, acDialog, "FASLIKHBR");
                            break;
                        }
                    case "CRMMAIN":
                        {
                            //DoCmd.OpenForm("CRMMAIN", acNormal);
                            break;
                        }
                    case "ENHESAR":
                        {
                            //DoCmd.OpenForm("ENHESAR1", acNormal);
                            break;
                        }
                    case "NOTE":
                        {
                            //DoCmd.OpenForm("Notes_frm", acNormal);
                            break;
                        }
                    // منوي  چك---------------------------------------------------------
                    case "CHEKD":
                        {
                            //DoCmd.OpenForm("PAY_GETD", acNormal);
                            break;
                        }
                    case "CHEKP":
                        {
                            //DoCmd.OpenForm("PAY_GETP", acNormal);
                            break;
                        }
                    case "VCHD":
                        {
                            //DoCmd.OpenForm("CHKREC_H", acNormal);
                            break;
                        }
                    case "VCHP":
                        {
                            //DoCmd.OpenForm("CHREC_HP", acNormal);
                            break;
                        }
                    case "CHKDLIST":
                        {
                            //DoCmd.OpenForm("CHKE_DLIST", acFormDS);
                            break;
                        }
                    case "CHKPLIST":
                        {
                            //DoCmd.OpenForm("CHEK_PLIST", acFormDS);
                            break;
                        }
                    case "SERIAL":
                        {
                            //DoCmd.OpenForm("F_MENU_SERILA", default, default, default, default, acDialog);
                            break;
                        }
                    case "TCHEK":
                        {
                            //DoCmd.OpenForm("COD_HESAB");
                            break;
                        }
                    case "FORMAT":
                        {
                            //DoCmd.OpenForm("FORMAT");
                            break;
                        }
                    case "CHPRN":
                        {
                            //DoCmd.OpenForm("HED_SODUR");
                            break;
                        }
                    case "PCHS":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "PCHS");
                            break;
                        }
                    case "DCHS":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "DCHS");
                            break;
                        }
                    case "PCHSS":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "PCHSS");
                            break;
                        }
                    case "DCHSS":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "DCHSS");
                            break;
                        }
                    case "CHKM":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CHKM");
                            break;
                        }
                    case "CHKB":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CHKB");
                            break;
                        }
                    case "CHKV":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CHKV");
                            break;
                        }
                    case "CHKVA":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CHKVA");
                            break;
                        }
                    case "CHKP":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CHKP");
                            break;
                        }
                    case "FVP":
                        {
                            //DoCmd.OpenForm("FORMAT_HESAB");
                            break;
                        }
                    case "FPV":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "CVP");
                            break;
                        }
                    case "RCHEKD":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "RCHEKD");
                            break;
                        }
                    case "RCHEKP":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "RCHEKP");
                            break;
                        }
                    case "MONCH":
                        {
                            //DoCmd.OpenForm("CHEK_MON");
                            break;
                        }
                    case "BEHESAB":
                        {
                            //DoCmd.OpenForm("CHKREC_HES");
                            break;
                        }
                    case "GBEHESAB":
                        {
                            //DoCmd.OpenForm("CHRE_LSPH", acFormDS);
                            break;
                        }
                    case "CHAPCHEK":
                        {
                            //DoCmd.OpenForm("CHAPCHEK", default, default, default, default, acDialog);
                            break;
                        }
                    case "CHVBV":
                        {
                            //DoCmd.OpenForm("CHEKS_BESTANKAR", acFormDS);
                            break;
                        }
                    case "CHVBVp":
                        {
                            //DoCmd.OpenForm("CHEKS_pBESTANKAR", acFormDS);
                            break;
                        }
                    case "MONCHP":
                        {
                            //DoCmd.OpenForm("CHEK_MONP");
                            break;
                        }
                    case "CHVZ":
                        {
                            //DoCmd.OpenForm("PAY_GETD_LOG_FORM", acFormDS);
                            break;
                        }
                    case var case3 when case3 == "RCHEKD":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "RCHEKD");
                            break;
                        }
                    case var case4 when case4 == "RCHEKP":
                        {
                            //DoCmd.OpenForm("F_MENU_CHEK", default, default, default, default, default, "RCHEKP");
                            break;
                        }
                    // منوي انبار-----------------------------------------------------------
                    case "ESWAP":
                        {
                            new HEAD_LST_ENTEGHAL_WIN().Show();
                            break;
                        }
                    case "MOGUKOL":
                        {
                            //DoCmd.OpenReport("R_MOGUDI_ANBARHA", acViewPreview);
                            break;
                        }
                    case "MOGUKOLLIST":
                        {
                            //DoCmd.OpenForm("MOGUDI_ANBARHA_LIST", acFormDS);
                            break;
                        }
                    case "BAZANB":
                        {
                            //DoCmd.OpenForm("BAZ_ANBAR", acNormal);
                            break;
                        }
                    case "AKMOGO":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR", acNormal, default, default, default, acDialog, "F");
                            break;
                        }
                    case "AKMOGO2":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR", acNormal, default, default, default, acDialog, "F2");
                            break;
                        }
                    case "AKMOGOR":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR", acNormal, default, default, default, acDialog, "R");
                            break;
                        }
                    case "AKMOGOR2":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR", acNormal, default, default, default, acDialog, "R2");
                            break;
                        }
                    case "KARTR":
                        {
                            // If FindUserFormStatus(Forms![BASEKNOW]![USERCOD], "F_MENU_KART") = 1 Then
                            // connectionString = CurrentProject.Connection.connectionString & "#" & Forms![BASEKNOW]![USERCOD] & "#" & [Forms]![Default]![SHIFT] & "#" & [Forms]![Default]![TFSAZMAN] & "#F_MENU_KART"
                            // Call Shell("C:\CORRECT\MCR.exe " & """" & connectionString & """", vbNormalFocus)
                            // Else
                            //DoCmd.OpenForm("F_MENU_KART", acNormal, default, default, default, acDialog, "R");
                            new F_MENU_KART().Show();
                            break;
                        }

                    case "KARTR2":
                        {
                            //DoCmd.OpenForm("F_MENU_KART", acNormal, default, default, default, acDialog, "KARTR2");
                            break;
                        }
                    case "LSTRAZ":
                        {
                            //DoCmd.OpenForm("TARAZ_ANBAR_KOL", acFormDS);
                            break;
                        }
                    case "RSTRAZ":
                        {
                            //DoCmd.OpenReport("R_TARAZ_ANBARHA", acViewPreview);
                            break;
                        }
                    case "LSTRAZA":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_TARAZ", acNormal, default, default, default, acDialog, "F");
                            break;
                        }
                    case "RSTRAZA":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_TARAZ", acNormal, default, default, default, acDialog, "R");
                            break;
                        }
                    case "RTARDTL":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_TARAZ", acNormal, default, default, default, acDialog, "FD");
                            break;
                        }
                    case "RASID":
                        {
                            //DoCmd.OpenForm("HEAD_LST_RASID");
                            break;
                        }
                    case "HAVL":
                        {
                            //DoCmd.OpenForm("HEAD_LST_HAVL");
                            break;
                        }
                    case "ANGD":
                        {
                            //DoCmd.OpenForm("ANBGRD_HEAD");
                            break;
                        }
                    case "RBRFR":
                        {
                            //DoCmd.OpenForm("HEAD_LST_RASID_OTHER");
                            break;
                        }
                    case "ANBBAZ":
                        {
                            //DoCmd.OpenForm("anbargardani-baz");
                            break;
                        }
                    case "TANBGRP":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_TARAZ", acNormal, default, default, default, acDialog, "TANBGRP");
                            break;
                        }
                    case "HAG":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_FRKH_GRP");
                            break;
                        }
                    case "SAYERHAV":
                        {
                            //DoCmd.OpenForm("HEAD_LST_HAV_OTHER");
                            break;
                        }
                    case "ANBMERG":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_MERG");
                            break;
                        }
                    case "TDBARG":
                        {
                            //DoCmd.OpenForm("F_MENU_DATE", default, default, default, default, default, "TDBARG");
                            break;
                        }
                    case "VBP":
                        {
                            //DoCmd.OpenForm("VBP_CHECK");
                            break;
                        }
                    case "DRIVERH":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR_FRKH_bar", default, default, default, default, acDialog);
                            break;
                        }
                    case "MOGDT":
                        {
                            //DoCmd.OpenForm("F_MENU_ANBAR", acNormal, default, default, default, acDialog, "MOGDT");
                            break;
                        }
                    // منوي حقوق-----------------------------------------------------------
                    case "PERS":
                        {
                            //DoCmd.OpenForm("PERSON", acNormal);
                            break;
                        }
                    case "WORK":
                        {
                            //DoCmd.OpenForm("WORKHEAD", acNormal);
                            break;
                        }
                    case "WOKPER":
                        {
                            //DoCmd.OpenForm("WORKING", acNormal);
                            break;
                        }
                    case "LISTWOR":
                        {
                            //DoCmd.OpenForm("list_salary", acFormDS);
                            break;
                        }
                    case "LISTWOR2":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "SA2");
                            break;
                        }
                    case "LISTBIM":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "RK");
                            break;
                        }
                    case "LISTMAL":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "MK");
                            break;
                        }
                    case "FISH1":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "F1");
                            break;
                        }
                    case "FISH2":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "F2");
                            break;
                        }
                    case "SALAR":
                        {
                            //DoCmd.OpenForm("F_MENU_BIM", default, default, default, default, default, "SA");
                            break;
                        }
                    case "DISK":
                        {
                            //DoCmd.OpenForm("ERSAL_SANAD");
                            break;
                        }
                    case "HOKM":
                        {
                            //DoCmd.OpenForm("PHOKM");
                            break;
                        }
                    case "MORAKH":
                        {
                            //DoCmd.OpenForm("PMORAKH");
                            break;
                        }
                    case "GHARA":
                        {
                            //DoCmd.OpenForm("PGHARAR");
                            break;
                        }
                    case "SAND":
                        {
                            //DoCmd.OpenForm("PSANDUGH_KAR");
                            break;
                        }
                    case "VAM":
                        {
                            //DoCmd.OpenForm("PVAM_KAR");
                            break;
                        }
                    case "TASV":
                        {
                            //DoCmd.OpenForm("PENDJOB");
                            break;
                        }
                    case "EP":
                        {
                            //DoCmd.OpenForm("EYDY_PADASH");
                            break;
                        }
                    case "DISKM":
                        {
                            //DoCmd.OpenForm("disket_maliat");
                            break;
                        }
                    // منوي برنامه ريزي-----------------------------------------------------------
                    case "PROG":
                        {
                            //DoCmd.OpenForm("PRGHEAD", acNormal);
                            break;
                        }
                    case "CH1":
                        {
                            //DoCmd.OpenForm("F_MENU_KOL_DATE", acNormal, default, default, default, acDialog, 2);
                            break;
                        }
                    case "CT1":
                        {
                            //DoCmd.OpenForm("F_MENU_MON", default, default, default, default, acDialog, 1);
                            break;
                        }
                    case "CT2":
                        {
                            //DoCmd.OpenForm("F_MENU_MON", default, default, default, default, acDialog, 2);
                            break;
                        }
                    case "CT3":
                        {
                            //DoCmd.OpenForm("F_MENU_MON", default, default, default, default, acDialog, 3);
                            break;
                        }
                    case "CT4":
                        {
                            //DoCmd.OpenForm("AMAR_FROOSH_KOL", acFormPivotChart);
                            break;
                        }
                    case "CT5":
                        {
                            //DoCmd.OpenForm("AMAR_FROOSH_KOL_all", acFormPivotChart);
                            break;
                        }

                    // منوي هتلي-----------------------------------------------------------
                    case "MEH":
                        {
                            //DoCmd.OpenForm("F_MENU_HOTEL", acNormal, default, default, default, default, "MEH");
                            break;
                        }
                    case "HOT":
                        {
                            //DoCmd.OpenForm("MEHMAN", acNormal, default, default, default, default, "MEH");
                            break;
                        }
                    case "ROM":
                        {
                            //DoCmd.OpenForm("ROOM", acFormDS);
                            break;
                        }
                    case "RACK":
                        {
                            //DoCmd.OpenForm("RACKROOM");
                            break;
                        }
                    // منوي سيستم-----------------------------------------------------------
                    case "ABOUT":
                        {
                            //DoCmd.OpenForm("ABOUT", acNormal);
                            break;
                        }
                    case "SYS":
                        {
                            //DoCmd.OpenForm("SAZMAN", acNormal);
                            break;
                        }
                    case "nuser":
                        {
                            //DoCmd.OpenForm("users");
                            break;
                        }
                    case "nname":
                        {
                            //DoCmd.OpenForm("USER_CHANGE");
                            break;
                        }
                    case "npass":
                        {
                            //DoCmd.OpenForm("NEWPASSWORD");
                            break;
                        }
                    case "permi":
                        {
                            //DoCmd.OpenForm("F_USER_PERMITION FORMS");
                            break;
                        }
                    case "ersal":
                        {
                            //DoCmd.RunCommand(acCmdBackup);
                            break;
                        }
                    case "dar":
                        {
                            //DoCmd.RunCommand(acCmdRestore);
                            break;
                        }
                    case "KONT":
                        {
                            //DoCmd.OpenForm("FORM11", default, default, default, default, acDialog, 1);
                            break;
                        }
                    case "DEFA":
                        {
                            new DEFAULT().Show();
                            break;
                        }
                    case var case5 when case5 == "DEFA":
                        {
                            new DEFAULT().Show();
                            break;
                        }
                    case "TR_FR":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_FROOSH2");
                            break;
                        }
                    case "TR_KH":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_KHREED1");
                            break;
                        }
                    case "TR_KHB":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_KH_BACK");
                            break;
                        }
                    case "TR_FRB":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_FROOSH_BACK2");
                            break;
                        }
                    case "TR_PFRB":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_PFROOSH2");
                            break;
                        }
                    case "TR_PGL":
                        {
                            //DoCmd.OpenForm("TR_PGET_HED");
                            break;
                        }
                    case "TR_KHAD":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_KHADAMAT");
                            break;
                        }
                    case "TR_MAVA":
                        {
                            //DoCmd.OpenForm("TR_HAVALAH_EXIT");
                            break;
                        }
                    case "TR_SMAVA":
                        {
                            //DoCmd.OpenForm("TR_HAVALAH_EXIT_SAYER");
                            break;
                        }
                    case "TR_TOL":
                        {
                            //DoCmd.OpenForm("TR_HAVALAH_ENTER");
                            break;
                        }
                    case "TR_HOT":
                        {
                            //DoCmd.OpenForm("TR_MEHMAN");
                            break;
                        }
                    case "TR_FRH":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_HAVL");
                            break;
                        }
                    case "TR_KHH":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_RASID");
                            break;
                        }
                    case "TR_ENT":
                        {
                            //DoCmd.OpenForm("TR_HEAD_LST_ENTEGHAL");
                            break;
                        }
                    case "TR_DED":
                        {
                            //DoCmd.OpenForm("TR_DEED_HEAD");
                            break;
                        }
                    case "TR_STU":
                        {
                            //DoCmd.OpenForm("TR_STUF_DEF");
                            break;
                        }
                    case "TR_SAZ":
                        {
                            //DoCmd.OpenForm("TR_AUTOMATIC");
                            break;
                        }
                    case "TR_HOK":
                        {
                            //DoCmd.OpenForm("TR_PHOKM");
                            break;
                        }
                    case "TR_MOR":
                        {
                            //DoCmd.OpenForm("TR_PMORAKH");
                            break;
                        }
                    case "ERSOF":
                        {
                            //DoCmd.OpenForm("ERSAL_OFFLINE", default, default, default, default, default, 1);
                            break;
                        }
                    case "DAROF":
                        {
                            //DoCmd.OpenForm("ERSAL_OFFLINE", default, default, default, default, default, 2);
                            break;
                        }
                    case "BLACK":
                        {
                            //DoCmd.OpenForm("BLOCK_CUSTOMER_SUB", acFormDS, default, default, default, default, 1);
                            break;
                        }
                    // منوي تالار-----------------------------------------------------------
                    case "PAZ":
                        {
                            //DoCmd.OpenForm("PAZIRESHTALAR");
                            break;
                        }
                    case "PRAK":
                        {
                            //DoCmd.OpenForm("RACKTALAR");
                            break;
                        }
                    case "TAL":
                        {
                            //DoCmd.OpenForm("TALARS_TARIF", acFormDS);
                            break;
                        }
                    // منوي پذيرش تعميرگاه-----------------------------------------------------------
                    case "PTM":
                        {
                            //DoCmd.OpenForm("PTAMIR");
                            break;
                        }
                    case "PKINCO":
                        {
                            //DoCmd.OpenForm("PKINDCODING");
                            break;
                        }
                    case "AMVAL":
                        {
                            //DoCmd.OpenForm("amvalform");
                            break;
                        }
                    case "MDKAR":
                        {
                            //DoCmd.OpenForm("PM_WORK_ORDERS");
                            break;
                        }
                    case "PMASSETS":
                        {
                            //DoCmd.OpenForm("PM_ASSETS_FR");
                            break;
                        }
                    case "PMLOCATION":
                        {
                            //DoCmd.OpenForm("PM_LOCATION_FR");
                            break;
                        }
                    case "PMPMEM":
                        {
                            //DoCmd.OpenForm("PM_EM_FAILURE_EVENTS_FR");
                            break;
                        }
                    case "PMKIND":
                        {
                            //DoCmd.OpenForm("PM_JOBTITLE_FR");
                            break;
                        }
                    case "PMCALEN":
                        {
                            //DoCmd.OpenForm("PM_CALENDER");
                            break;
                        }
                    case "PMWORKOR":
                        {
                            //DoCmd.OpenForm("PM_WORK_ORDER_DTL", acFormDS);
                            break;
                        }
                    case "PMDARKH":
                        {
                            //DoCmd.OpenForm("PM_HEAD_LST_REQUEST_ANBAR");
                            break;
                        }
                    case "PMFAILDEF":
                        {
                            //DoCmd.OpenForm("PM_EM_FAILURE_FR");
                            break;
                        }
                    // بودجه----------------------------------------------------------
                    case "MMTAKH":
                        {
                            //DoCmd.OpenForm("F_MENU_ssm", default, default, default, default, acDialog);
                            break;
                        }
                    case "ssmt":
                        {
                            //DoCmd.OpenForm("sazman", default, default, default, default, acDialog, 1);
                            break;
                        }
                    case "crmt":
                        {
                            //DoCmd.OpenForm("sazman", default, default, default, default, acDialog, 2);
                            break;
                        }
                    case "TGUSER":
                        {
                            //DoCmd.OpenForm("USER_ENDN_SUB", acFormDS, default, default, default, default, 1);
                            break;
                        }
                }
            }
            else
            {
                new Msgwin(false, "شما اجازه لازم براي استفاده از اين بخش را نداريد.!").ShowDialog();
                return;
            }
            frm = "";

        }
        public static string GETUSERHES(int US)
        {
            string GETUSERHESRet = default;
            var rst = dbms.DoGetDataSQL<string>("SELECT hes FROM dbo.SALA_DTL WHERE idd = " + US).FirstOrDefault();
            if (rst != null)
            {
                if (IsNull(rst))
                {
                    GETUSERHESRet = "";
                }
                else
                {
                    GETUSERHESRet = rst;
                }
            }
            return GETUSERHESRet;
        }
        // سابقه چکدريافتي
        public static void GETDLOG(int VAZ, string N_SERI, int bnk, long DTS, int SANDUGH)
        {
            var rst = dbms.DoGetDataSQL<PAY_GETD_LOG>("SELECT     N_SERI, BANK, DATE_S, DATE_V ,DATETIM, VAZ, SANDUGH, USER_NAME FROM dbo.PAY_GETD_LOG WHERE     (N_SERI = " + N_SERI + " ) AND (BANK = " + bnk + ") AND (DATE_S = " + DTS + ")").ToList();
            //If RST.RecordCount = 0 Then
            //rst.addnew(); //insert
            //rst.fields("n_seri") = n_seri;
            //rst.fields("bank") = bnk;
            //rst.fields("date_s") = dts;
            //rst.fields("date_v") = farsidate(/*datetime.now*/);
            //rst.fields("datetim") = datetime.now;
            //rst.fields("vaz") = vaz;
            //rst.fields("sandugh") = sandugh;
            //rst.fields("user_name") = ucurrentuser();
            //rst.update();


            dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                          VALUES({N_SERI},
                                 {bnk}   ,
                                 {DTS}   ,
                                 {FARSIDATE2()}, 
                                 CAST( (SELECT GETDATE()) AS DATETIME),
                                 {VAZ}   , 
                                 {SANDUGH}   , 
                                 N'{UCurrentUser()}' )");
            dbms.DoExecuteSQL("update pay_getd set vaz = " + VAZ + " WHERE     (N_SERI = " + N_SERI + " ) AND (BANK = " + bnk + ") AND (DATE_S = " + DTS + ")");
        }
        public static double? FIRSTM(double KOL)
        {
            double? FIRSTMRet = default;
            var rst = dbms.DoGetDataSQL<int?>("SELECT Min(DETA_HES.NUMBER) AS MinOfNUMBER FROM DETA_HES WHERE (((DETA_HES.N_KOL) = " + KOL + " )) HAVING ((Not (Min(DETA_HES.NUMBER)) Is Null))").ToList();
            if (rst.Count > 0)
            {
                FIRSTMRet = (double)rst.FirstOrDefault();
            }
            else
            {
                FIRSTMRet = null;
            }
            return FIRSTMRet;
        }
        public static double? FIRSTT(double KOL, double MOIN)
        {
            double? FIRSTTRet = default;
            var rst = dbms.DoGetDataSQL<int?>("SELECT Min(TDETA_HES.TNUMBER) AS MinOfTNUMBER FROM TDETA_HES WHERE (((TDETA_HES.NUMBER) = " + MOIN + ") And ((TDETA_HES.N_KOL) = " + KOL + ")) HAVING ((Not (Min(TDETA_HES.TNUMBER)) Is Null))").ToList();
            if (rst.Count > 0)
            {
                FIRSTTRet = rst.FirstOrDefault();
            }
            else
            {
                FIRSTTRet = null;
            }
            return FIRSTTRet;
        }

        public static double GetArzesh(string COD)
        {
            double GetArzeshRet = default;
            var rst = dbms.DoGetDataSQL<double?>("SELECT VRA FROM STUF_DEF WHERE CODE  = '" + COD + "'").ToList();
            if (rst.Count > 0)
            {
                GetArzeshRet = (double)rst.FirstOrDefault();
            }
            else
            {
                GetArzeshRet = (double)Baseknow.ARSESH;
            }
            return GetArzeshRet;
        }

        public static bool BLOCKED(long HKOL, long HMOIN, long HTAF)
        {
            bool BLOCKEDRet = default;
            long TKOL;
            long TMOIN;
            long TTAF;
            var RST2 = dbms.DoGetDataSQL<CustomQre2>("SELECT USERCO,HES FROM BLOCK_HES WHERE USERCO = " + Baseknow.USERCOD).ToList();
            if (RST2.Count > 0)
            {
                for (int i = 0; i < RST2.Count; i++) //while (!RST2.EOF)
                {
                    TKOL = MGETKOL(RST2.FirstOrDefault().HES);
                    TMOIN = MGETMOIN(RST2.FirstOrDefault().HES);
                    TTAF = MGETTAF(RST2.FirstOrDefault().HES);
                    if (TKOL == HKOL && TMOIN == 0L && TTAF == 0L)
                    {
                        BLOCKEDRet = true;
                        return BLOCKEDRet;
                    }
                    else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == 0L)
                    {
                        BLOCKEDRet = true;
                        return BLOCKEDRet;
                    }
                    else if (TKOL == HKOL && TMOIN == HMOIN && TTAF == HTAF)
                    {
                        BLOCKEDRet = true;
                        return BLOCKEDRet;
                    }
                    //RST2.MoveNext();
                }
            }
            BLOCKEDRet = false;
            return BLOCKEDRet;
        }

        //public static long MGETKOL(string SHES)
        //{
        //    long MGETKOLRet = default;
        //    byte i;
        //    i = 1;
        //    while (Strings.Mid(SHES, i, 1) != "-" && i <= Strings.Len(SHES))
        //        i = (byte)(i + 1);
        //    MGETKOLRet = Conversions.ToLong(Strings.Left(SHES, i - 1));
        //    return MGETKOLRet;
        //}
        public static long MGETKOL(string shes)
        {
            if (string.IsNullOrWhiteSpace(shes)) return 0;

            shes = CL_LMethods.NormalizeDigits(shes.Trim());

            // برو تا اولین رقم
            int start = 0;
            while (start < shes.Length && !char.IsDigit(shes[start]))
                start++;
            if (start >= shes.Length) return 0;

            // عددِ قبل از اولین '-' را جدا کن
            int dash = shes.IndexOf('-', start);
            string token = dash >= 0 ? shes.Substring(start, dash - start)
                                     : shes.Substring(start);

            // فقط رقم‌ها را نگه دار (اگر کاراکتر غیرعددی قاطی باشد)
            var digitsOnly = new StringBuilder(token.Length);
            foreach (var ch in token)
                if (char.IsDigit(ch)) digitsOnly.Append(ch);

            return long.TryParse(digitsOnly.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val)
                ? val
                : 0;
        }
        public static long MGETMOIN(string SHES)
        {
            long MGETMOINRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                    i = (byte)(i + 1);
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-" && j <= Strings.Len(SHES))
                    j = (byte)(j + 1);
                i = (byte)(i + 1);
                MGETMOINRet = Conversions.ToLong(Strings.Mid(SHES, i, j - i));
            }

            return MGETMOINRet;
        }
        public static long MGETTAF(string SHES)
        {
            long MGETTAFRet = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                    i = (byte)(i + 1);
                j = (byte)(i + 1);
                if (j == Strings.Len(SHES))
                {
                }

                else
                {
                    while (Strings.Mid(SHES, j, 1) != "-" && j < Strings.Len(SHES))
                        j = (byte)(j + 1);
                    if (j >= Strings.Len(SHES))
                    {
                        MGETTAFRet = 0L;
                    }
                    else
                    {
                        MGETTAFRet = Conversions.ToLong(Strings.Mid(SHES, j + 1, Strings.Len(SHES)));
                    }
                }
            }

            return MGETTAFRet;
        }
        public static long GETTAF2(string SHES)
        {
            long GETTAF2Ret = default;
            byte i, j;
            i = 1;
            if (Strings.Len(SHES) < 5)
            {
            }
            else
            {
                while (Strings.Mid(SHES, i, 1) != "-")
                    i = (byte)(i + 1);
                j = (byte)(i + 1);
                while (Strings.Mid(SHES, j, 1) != "-")
                    j = (byte)(j + 1);
                i = j;
                while (Strings.Mid(SHES, j + 1, 1) != "-" & j < Strings.Len(SHES))
                    j = (byte)(j + 1);
                if (j == Strings.Len(SHES))
                {
                    GETTAF2Ret = -1;
                }
                else
                {
                    GETTAF2Ret = Conversions.ToLong(Strings.Mid(SHES, j + 2, Strings.Len(SHES) - (j + 1)));
                }
            }

            return GETTAF2Ret;
        }

        public static string ALPHANUM(double AANUMBER)
        {
            string ALPHANUMRet = default;
            string AECHO, ATALAF, STRNUM, ACOL1, ACOL2, ACOL3, ACOL;
            string[,] MATANUM = new string[11, 4];
            string[] BNBIST = new string[10], MEGH = new string[4], AAS = new string[5], VA = new string[3];
            int K, i, ALEN, SETAYEE, j, AM2, AM3, AM, ALINK, ALENT;
            AANUMBER = Math.Round(AANUMBER);

            // بررسی محدودیت عدد ورودی - حداکثر 999 تریلیون (15 رقم)
            if (AANUMBER > 999999999999999)
            {
                return "عدد خارج از محدوده مجاز است";
            }

            VA[1] = "";
            VA[2] = " و ";
            AAS[1] = " هزار";
            AAS[2] = " ميليون";
            AAS[3] = " ميليارد";
            AAS[4] = " ترليون";
            ACOL1 = "    يك  دو  سه  چهارپنج شش  هفت هشت نه";
            K = 1;
            for (i = 1; i <= 10; i++)
            {
                MATANUM[i, 1] = Strings.Trim(Strings.Mid(ACOL1, K, 4));
                K = K + 4;
            }
            ACOL2 = "     ده   بيست سي   چهل  پنجاهشصت  هفتادهشتادنود  ";
            K = 1;
            for (i = 1; i <= 10; i++)
            {
                MATANUM[i, 2] = Strings.Trim(Strings.Mid(ACOL2, K, 5));
                K = K + 5;
            }
            ACOL3 = "      صد    دويست سيصد  چهارصدپانصد ششصد  هفتصد هشتصد نهصد  ";
            K = 1;
            for (i = 1; i <= 10; i++)
            {
                MATANUM[i, 3] = Strings.Trim(Strings.Mid(ACOL3, K, 6));
                K = K + 6;
            }
            ACOL = "يازده دوازدهسيزده چهاردهپانزدهشانزدههفده  هيجده نوزده ";
            K = 1;
            for (i = 1; i <= 9; i++)
            {
                BNBIST[i] = Strings.Trim(Strings.Mid(ACOL, K, 6));
                K = K + 6;
            }
            STRNUM = Strings.Trim(Conversion.Str(AANUMBER));
            ATALAF = "";
            ALEN = Strings.Len(STRNUM);
            SETAYEE = 0;
            K = 0;
            while (K < ALEN)
            {
                AECHO = "";
                j = 0;
                for (i = 1; i <= 3; i++)
                    MEGH[i] = "";
                while (j < 3 & j < ALEN)
                {
                    if (ALEN <= K)
                    {
                        MEGH[j + 1] = "";
                    }
                    else
                    {
                        MEGH[j + 1] = Strings.Mid(STRNUM, ALEN - K, 1);
                    }
                    j = j + 1;
                    K = K + 1;
                }
                if (Conversion.Val(MEGH[3]) > 0d && Conversion.Val(MEGH[1]) > 0d || Conversion.Val(MEGH[3]) > 0d && Conversion.Val(MEGH[2]) > 0d)
                {
                    AM3 = 2;
                }
                else
                {
                    AM3 = 1;
                }
                if (Conversion.Val(MEGH[2]) > 0d && Conversion.Val(MEGH[1]) > 0d)
                {
                    AM2 = 2;
                }
                else
                {
                    AM2 = 1;
                }
                SETAYEE = SETAYEE + 1;
                AM = (int)Math.Round(Conversion.Val(MEGH[2] + MEGH[1]));
                if (AM >= 11 & AM <= 19)
                {
                    AECHO = MATANUM[(int)Math.Round(Conversion.Val(MEGH[3]) + 1d), 3] + VA[AM3] + BNBIST[AM - 10];
                }
                else
                {
                    AECHO = MATANUM[(int)Math.Round(Conversion.Val(MEGH[3]) + 1d), 3] + VA[AM3] + MATANUM[(int)Math.Round(Conversion.Val(MEGH[2]) + 1d), 2] + VA[AM2] + MATANUM[(int)Math.Round(Conversion.Val(MEGH[1]) + 1d), 1];
                }
                ALINK = 1;
                if (SETAYEE > 1 & !(MEGH[1] == "0" & MEGH[2] == "0" & MEGH[3] == "0"))
                {
                    AECHO = AECHO + AAS[SETAYEE - 1];
                    ALINK = 2;
                }
                ATALAF = AECHO + VA[ALINK] + ATALAF;
            }
            ALENT = Strings.Len(Strings.Trim(ATALAF));
            if (!string.IsNullOrEmpty(ATALAF))
            {
                if (Strings.Mid(Strings.Trim(ATALAF), ALENT - 1, 2) == " و")
                {
                    ALPHANUMRet = Strings.Mid(Strings.Trim(ATALAF), 1, ALENT - 1);
                }
                else
                {
                    ALPHANUMRet = Strings.Trim(ATALAF);
                }
            }
            else
            {
                ALPHANUMRet = "صفر";
            }

            return ALPHANUMRet;
        }

        //Showemza(Me, "FFR_FROOSHTX", "FFR_HESABTX", "FFR_MODIRTX")
        public static string Getusersemat(int usid, string fld)
        {
            string GetusersematRet = default;
            var rst = dbms.DoGetDataSQL<SIGN>("SELECT * FROM  SIGN WHERE  USERCO = " + usid).ToList();

            if (rst.Count > 0)
            {
                var firstRecord = rst.FirstOrDefault();
                // Check if firstRecord is not null
                if (firstRecord != null)
                {
                    // Get the property info
                    var propertyInfo = firstRecord.GetType().GetProperty(fld);
                    // Check if propertyInfo is not null
                    if (propertyInfo != null)
                    {
                        // Get the value of the property
                        var value = propertyInfo.GetValue(firstRecord, null) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            GetusersematRet = value;
                        }
                        else
                        {
                            switch (fld ?? "")
                            {
                                // فروش
                                case "FFR_FROOSHTX":
                                    {
                                        GetusersematRet = "فروش";
                                        break;
                                    }
                                case "FFR_HESABTX":
                                    {
                                        GetusersematRet = "حسابداري";
                                        break;
                                    }
                                case "FFR_MODIRTX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // حواله انبار
                                case "ANB_HAVL_FROSHTX":
                                    {
                                        GetusersematRet = "فروش";
                                        break;
                                    }
                                case "ANB_HAVL_ANBTX":
                                    {
                                        GetusersematRet = "انبار";
                                        break;
                                    }
                                case "ANB_HAVL_MODIRTX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // خزانه
                                case "SGN0134TX":
                                    {
                                        GetusersematRet = "تنظيم كننده";
                                        break;
                                    }
                                case "SGN0234TX":
                                    {
                                        GetusersematRet = "مدير مالي";
                                        break;
                                    }
                                case "SGN0334TX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // خريد
                                case "KFR_BAZARTX":
                                    {
                                        GetusersematRet = "بازرگاني";
                                        break;
                                    }
                                case "KFR_HESABTX":
                                    {
                                        GetusersematRet = "حسابداري";
                                        break;
                                    }
                                case "KFR_MODIRTX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // پيش فاکتور
                                case "FFRP_FROOSHTX":
                                    {
                                        GetusersematRet = "فروش";
                                        break;
                                    }
                                case "FFRP_ANBTX":
                                    {
                                        GetusersematRet = "حسابداري";
                                        break;
                                    }
                                case "FFRP_HESABTX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // مرخصي
                                case "SGN0137TX":
                                    {
                                        GetusersematRet = "درخواست كننده";
                                        break;
                                    }
                                case "SGN0237TX":
                                    {
                                        GetusersematRet = "مدير واحد";
                                        break;
                                    }
                                case "SGN0337TX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // سند حسابداري
                                case "SND_TAHITX":
                                    {
                                        GetusersematRet = "تنظيم كننده";
                                        break;
                                    }
                                case "SND_MALITX":
                                    {
                                        GetusersematRet = "مدير مالي";
                                        break;
                                    }
                                case "SND_MODIRTX":
                                    {
                                        GetusersematRet = "مدير عامل";
                                        break;
                                    }
                                // برگشت فروش
                                case "FFRB_FROOSHTX":
                                    {
                                        GetusersematRet = "تنظيم كننده";
                                        break;
                                    }
                                case "FFRB_ANBTX":
                                    {
                                        GetusersematRet = "انبار دار";
                                        break;
                                    }
                                case "FFRB_HESABTX":
                                    {
                                        GetusersematRet = "حسابداري";
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        // Handle case where property does not exist
                        GetusersematRet = "";
                    }
                }
            }

            return GetusersematRet;
        }
        public static string GETMANDAH(string HES)
        {
            if (dbms == null || string.IsNullOrWhiteSpace(HES))
                return "0";

            try
            {
                // Check if account is blocked
                if (CL_HESABDARI.BLOCKEDMK(HES))
                    return "مسدود است";

                const string sql = "SELECT SUM(BED - BES) AS Balance FROM dbo.DEED_DTL WHERE HES = @hes";

                var balance = dbms.DoGetDataSQL<decimal?>(sql, new { HES })?.FirstOrDefault() ?? 0m;

                if (balance == 0)
                    return "0";

                return balance > 0
                    ? $"{balance:#,##0} ريال بدهكار"
                    : $"{Math.Abs(balance):#,##0} ريال بستانكار";
            }
            catch (Exception ex)
            {
                return "0";
            }
        }

        public static void LOGFACT(double num, long tgg, double RES, string fld)
        {
            dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST_LOG(UP_DATE, NUMBER, TAGG, RESERVED, UP_USER_NAME, fieldname, UDATEF)
                                 VALUES(GETDATE(),
                                 {num} ,
                                 {tgg} ,
                                 {RES} ,
                                 N'{UCurrentUser()}' ,
                                 N'{fld}' ,
                                 {FARSIDATE()})");
        }

        public static string GETUSERCO(int UID)
        {
            string GETUSERCORet = default;
            var rst = dbms.DoGetDataSQL<string>("SELECT HES FROM dbo.SALA_DTL WHERE (IDD = " + UID + ")").ToList();
            if (rst.Count > 0)
            {
                GETUSERCORet = rst.FirstOrDefault();
            }
            return GETUSERCORet;
        }

        public static bool LETSGOUPDATE(string obj, string frm, byte KIND)
        {
            bool LETSGOUPDATERet = default;
            var rst = dbms.DoGetDataSQL<CL_QR>("SELECT TFORMS.CAPTION, TFORMS.kind, SAL_CHEK.USERCO, SAL_CHEK.RUN, SAL_CHEK.SEE, SAL_CHEK.INP, SAL_CHEK.UPD, SAL_CHEK.DEL FROM TFORMS INNER JOIN (SALA_DTL INNER JOIN SAL_CHEK ON SALA_DTL.IDD = SAL_CHEK.USERCO) ON TFORMS.IDH = SAL_CHEK.OBJECT WHERE     (TFORMS.FORMNAME = '" + WANTEDFORM(frm) + "') AND (SAL_CHEK.USERCO = " + Baseknow.USERCOD + " )").ToList();
            if (rst.Count != 0)
            {
                if (Convert.ToBoolean(!rst.FirstOrDefault().UPD))
                {
                    LETSGOUPDATERet = false;
                }
                else
                {
                    LETSGOUPDATERet = true;
                }

            }
            return LETSGOUPDATERet;
        }

        public static string GetUserList()
        {
            string GetUserListRet = default;
            string VS;
            VS = "";
            var rst = dbms.DoGetDataSQL<SALA_DTL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD FROM SALA_DTL WHERE (ENABL=0) and idd <> 120").ToList();
            for (int i = 0; i < rst.Count; i++)
            {
                VS = VS + rst.FirstOrDefault().IDD + ";" + '"' + DECODEUN(rst.FirstOrDefault().SAL_NAME) + '"';
                VS = VS + ";";
                //if (!rst.EOF)
                //{

                //}
            }
            //while (!rst.EOF)
            //{

            //}
            GetUserListRet = VS;
            return GetUserListRet;
        }

        public static void UpAZAE(string HES)
        {
            double SZ = default, SGE = default;
            var rst = dbms.DoGetDataSQL<GRADE_CUST_TAB>("SELECT * FROM GRADE_CUST_TAB WHERE GCCUST_HES = '" + HES + "'").ToList();
            if (rst != null)
            {
                for (int i = 0; i < rst.Count; i++) //while (!rst.EOF)
                {
                    SZ = (double)(SZ + rst[i].GCZARIB);
                    var RSTS = dbms.DoGetDataSQL<LSQ1>("SELECT  SUM(GCGRPZARIB) AS SZ, SUM(GCGRPGRADE * GCGRPZARIB) AS SGE FROM  dbo.GRADE_CUST_GRP WHERE     GCTABID = " + rst[i].GCTABID).ToList();
                    if (RSTS.Count > 0 & !IsNull(RSTS.FirstOrDefault().SZ) && RSTS.FirstOrDefault().SZ != 0)
                    {
                        SGE = (double)(SGE + RSTS.FirstOrDefault().SGE / RSTS.FirstOrDefault().SZ * rst[i].GCZARIB);
                    }
                }
                dbms.DoExecuteSQL($"UPDATE AZAE SET JAMZARIB = {SZ} ,  EMTIAZ = {SGE / SZ} WHERE HES = '{HES}'");

                Upgrade(HES);

                //Forms["AZAE"].Form.JAMZARIB.Requery();
                //Forms["AZAE"].Form.EMTIAZ.Requery();
            }
        }
        public static void Upgrade(string HES)
        {
            double SZ, SGE;
            var rst = dbms.DoGetDataSQL<AZAE>("SELECT * FROM AZAE WHERE HES = '" + HES + "'").FirstOrDefault();
            if (rst is not null)
            {
                var RSTS = dbms.DoGetDataSQL<int?>("SELECT GID from GRADE_RANGE WHERE GTA >= " + rst.EMTIAZ + " and GAZ <= " + rst.EMTIAZ).FirstOrDefault();
                if (RSTS is not null)
                {
                    dbms.DoExecuteSQL($"UPDATE dbo.AZAE SET GRADEID = {RSTS}  WHERE HES = '" + HES + "' ");
                    //rst.Fields("GRADEID") = RSTS.Fields("GID");
                    //rst.update();
                }
                else
                {
                    new Msgwin(false, "گريدي براي اين امتياز تعريف نشده").Show();
                }
            }
        }
        public static long GetLGradeGRP()
        {
            long GetLGradeGRPRet = default;
            var rst = dbms.DoGetDataSQL<int?>("SELECT Max(GCGRPID) AS MIDD FROM GRADE_CUST_GRP").FirstOrDefault();
            if (rst != null || rst == 0)
            {
                GetLGradeGRPRet = 1;
            }
            else
            {
                GetLGradeGRPRet = (long)(rst + 1);
            }

            return GetLGradeGRPRet;
        }
        public static long GetLGradetab()
        {
            long tempGetLGradetab = 0;

            var rst = dbms.DoGetDataSQL<int?>("SELECT Max(GCTABID) AS MIDD FROM GRADE_CUST_TAB").FirstOrDefault();
            if (rst != null || rst == 0)
            {
                tempGetLGradetab = 1;
            }
            else
            {
                tempGetLGradetab = (int)rst + 1;
            }
            return tempGetLGradetab;
        }
        public static void INSERTGRPGRADE(long GCTABIDP, long GFTIDP)
        {
            var RSTS = dbms.DoGetDataSQL<GRADE_GRP_FT>("SELECT * FROM GRADE_GRP_FT WHERE GFTID = " + GFTIDP).ToList();
            for (int i = 0; i < RSTS.Count; i++) //while (!RSTS.EOF)
            {
                dbms.DoExecuteSQL($@"INSERT INTO dbo.GRADE_CUST_GRP(GCGRPID, GCGRPNAME, GCGRPZARIB, GCGRPGRADE, GCTABID, GCVALUE)
                                     VALUES({GetLGradeGRP()},
                                     N'{RSTS[i].GFGRPNAMEFT}' ,
                                     {RSTS[i].GFGRPZARIB} ,
                                     {RSTS[i].GFGRPGRADE} ,
                                     {GCTABIDP}   ,
                                     {RSTS[i].GVALUESCALE})  ");
                //rst.AddNew();
            }
        }
        public static bool ISHESAB3(string HES)
        {
            bool tempISHESAB3 = false;
            var RST = dbms.DoGetDataSQL<CUST_HESAB>("SELECT HES FROM CUST_HESAB WHERE HES = '" + HES + "'").ToList();
            if (RST.Count == 0)
            {
                tempISHESAB3 = false;
            }
            else
            {
                tempISHESAB3 = true;
            }
            return tempISHESAB3;
        }
        public static long CreateHEAD_LST(int TAGG, long DATE_N, string CUST_NO, long num = 0)
        {
            long CreateHEAD_LSTRet = default;
            long MAXNUM;
            var CAPTION = "صدور فاکتور";
            //'*************************************************************

            using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                {
                    //Fake Query for Lock Table
                    db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);
                    //Fake Query for Lock Table

                    try { db.Execute($"DELETE TOP (1) FROM HEAD_LST WHERE NUMBER = 0 AND DATE_N = 0 AND TAG = {TAGG}", null, transaction); } catch { } //Just in Case

                    db.Execute($"INSERT INTO HEAD_LST (NUMBER,DATE_N,TAG) VALUES (0,0,{TAGG})", null, transaction);

                    if (TAGG == 13)
                    {
                        var rss0 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER1) AS MaxOfNUMBER1 FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();
                        if (IsNull(rss0))
                        {
                            var rss00 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER1) AS MaxOfNUMBER1 FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();

                            if (IsNull(rss00))
                            {
                                var rss1 = db.Query<double?>("SELECT count(HEAD_LST.NUMBER1) AS MaxOfNUMBER1 FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();

                                if (rss1 == 0 || IsNull(rss1))
                                {
                                    MAXNUM = 1L;
                                }
                                else
                                {
                                    MAXNUM = 0L;
                                }
                            }
                            else
                            {
                                MAXNUM = (long)(rss00 + 1);
                            }
                        }
                        else
                        {
                            MAXNUM = (long)(rss0 + 1);
                        }

                        db.Execute("INSERT INTO HEAD_LST (NUMBER,NUMBER1,TAG,DATE_N,CUST_NO,OKF,CDDATE,CDTIME) VALUES (" + num + "," + MAXNUM + "," + TAGG + "," + DATE_N + ",'" + CUST_NO + "',0," + Tarikh.GET_OADATE_DAO() + "," + Tarikh.FullCurrentDate + ")", null, transaction);
                    }
                    else
                    {
                        var rss0 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();

                        if (IsNull(rss0))
                        {
                            var rss00 = db.Query<double?>("SELECT Max(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();

                            if (IsNull(rss00))
                            {
                                var rss1 = db.Query<double?>("SELECT count(HEAD_LST.NUMBER) AS MaxOfNUMBER FROM HEAD_LST WHERE TAG = " + TAGG, null, transaction).FirstOrDefault();
                                if (rss1 == 0 || IsNull(rss1))
                                {
                                    MAXNUM = 1L;
                                }
                                else
                                {
                                    MAXNUM = 0L;
                                }
                            }
                            else
                            {
                                MAXNUM = (long)(rss00 + 1);
                            }
                        }
                        else
                        {
                            MAXNUM = (long)(rss0 + 1);
                        }
                        var maxNumber1ForTag = db.Query<long?>("SELECT Max(HEAD_LST.NUMBER1) AS MaxOfNUMBER1 FROM HEAD_LST WHERE TAG = @Tag", new { Tag = TAGG }, transaction).FirstOrDefault();
                        var nextAvailableNumber1 = IsNull(maxNumber1ForTag) ? 1L : (long)maxNumber1ForTag + 1;
                        long number1ToUse = num;

                        if (num == 0)
                        {
                            number1ToUse = nextAvailableNumber1;
                        }
                        else
                        {
                            var existingNumber1Count = db.Query<int>("SELECT COUNT(1) FROM HEAD_LST WHERE TAG = @Tag AND NUMBER1 = @Number1", new { Tag = TAGG, Number1 = num }, transaction).FirstOrDefault();

                            if (existingNumber1Count > 0)
                            {
                                number1ToUse = nextAvailableNumber1;
                            }
                        }

                        db.Execute("INSERT INTO HEAD_LST (NUMBER,NUMBER1,TAG,DATE_N,CUST_NO,OKF,CDDATE,CDTIME) VALUES (" + MAXNUM + "," + number1ToUse + "," + TAGG + "," + DATE_N + ",'" + CUST_NO + "',0," + Tarikh.GET_OADATE_DAO() + "," + Tarikh.FullCurrentDate + ")", null, transaction);
                    }

                    //Remove NUll Insert:
                    try { db.Execute($"DELETE TOP (1) FROM HEAD_LST WHERE NUMBER = 0 AND DATE_N = 0 AND TAG = {TAGG}", null, transaction); } catch { } //Just in Case


                    transaction.Commit();
                    db?.Close();
                }
            }

            CreateHEAD_LSTRet = MAXNUM;
            // *************************************************************
            return CreateHEAD_LSTRet;
            // *************************************************************
        }
        public static double? GetStkKala(string _CODE_, double _ANBAR_)
        {
            return dbms.DoGetDataSQL<double?>($"SELECT MOGODI FROM dbo.STUF_STK WHERE CODE = N'{_CODE_}' AND ANBAR = {_ANBAR_}").FirstOrDefault();
        }
        public static long GetFactorDT(long NM, int tg)
        {
            long GetFactorDTRet = default;
            var rst = dbms.DoGetDataSQL<long?>("SELECT DATE_N FROM dbo.HEAD_LST WHERE TAG = " + tg + " and number = " + NM).FirstOrDefault();
            if (rst != null)
            {
                GetFactorDTRet = (long)rst;
            }

            return GetFactorDTRet;
        }
        public static bool MoadianLock(long _NUMBER_, int TGU)
        {
            bool result = false;
            long dtn = 0;
            long dtVal = 0;
            try
            {

                string tinData = Baseknow.tindata;
                string memoryId = Baseknow.MEMORYID;

                memoryId = memoryId.Trim();

                dtn = GetFactorDT(_NUMBER_, TGU);

                int i = tinData.IndexOf("moadian:", StringComparison.OrdinalIgnoreCase);
                if (i < 0) i = -1;           // tag not found
                else i += 8;          // move past "moadian:"

                while (i >= 0 && i < tinData.Length)
                {
                    /*--- 2-a. Pick up    pk  --------------------------------*/
                    int j = tinData.IndexOf(',', i);
                    if (j < 0) break;        // malformed list

                    string pk = tinData.Substring(i, j - i);

                    if (!string.IsNullOrEmpty(pk))
                        pk = pk.Trim();

                    /*--- 2-b. Pick up    dt  --------------------------------*/
                    i = j + 1;
                    j = tinData.IndexOf(',', i);
                    if (j < 0) j = tinData.Length;

                    string dtStr = tinData.Substring(i, j - i);

                    // Parse dt to long; non-numeric → 0
                    dtVal = 0;
                    long.TryParse(dtStr, out dtVal);

                    /*--- 2-c. Check the condition ---------------------------*/
                    if (!string.IsNullOrEmpty(pk))
                    {
                        if (pk.ToString().Equals(memoryId.Trim(), StringComparison.OrdinalIgnoreCase) && dtn <= dtVal)
                        {
                            result = true;
                            return true;         // mirrors the early Exit Function
                        }
                    }

                    /*--- 2-d. Advance to next pair --------------------------*/
                    i = j + 1;
                }
            }
            catch
            {
                new Msgwin(false, $"اطلاعات مربوط به مودیان یافت نشد").ShowDialog();
                return false;
            }

            try
            {
                new Msgwin(false, $"براي اين شرکت اشتراک سامانه موديان نداريد , تاریخ مودیان شما : {dtVal} , تاریخ جاری : {dtn}").ShowDialog();
            }
            catch { /*ignore*/}

            return result;
        }

        /// <summary>
        /// BAYEG default base is : (int)100,000,000 
        /// </summary>
        /// <param name="BayegBase"></param>
        /// <param name="n_s"></param>
        /// <returns></returns>
        public static int? UpdateOrGenerateBAYEG___OLD(int BayegBase, int n_s)
        {
            const int _bayeganbase_ = 100000000;
            if (BayegBase == default || BayegBase == null)
            {
                BayegBase = _bayeganbase_ + 1;
            }
            else
            {
                BayegBase += 1;
            }

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        //db.Execute($@"UPDATE dbo.DEED_HED SET BAYEG = 100000000 + BASE WHERE (BAYEG IS NULL) AND N_S NOT IN ({n_s})", null, transaction);
                        db.Execute($@"WITH OrderedDeeds AS (
                                         SELECT 
                                             N_S,
                                             BASE,
                                             ROW_NUMBER() OVER (ORDER BY N_S) AS RowNum
                                         FROM dbo.DEED_HED
                                         WHERE BAYEG IS NULL AND N_S NOT IN ({n_s})
                                     )
                                     UPDATE dbo.DEED_HED
                                     SET BAYEG = 100000000 + OrderedDeeds.BASE
                                     FROM dbo.DEED_HED AS d
                                     JOIN OrderedDeeds AS OrderedDeeds
                                         ON d.N_S = OrderedDeeds.N_S
                                     WHERE OrderedDeeds.RowNum IS NOT NULL", null, transaction);

                        var RowExistValue = db.QueryFirstOrDefault<int?>(@$"SELECT TOP 100 BAYEG FROM dbo.DEED_HED WHERE N_S = {n_s}", null, transaction: transaction);

                        if (RowExistValue.HasValue && RowExistValue.Value > 0)
                        {
                            //Nothing
                        }
                        else
                        {
                            // If no rows were updated, calculate a new unique BAYEG
                            string maxBayegSql = "SELECT ISNULL(MAX(BAYEG)+1, @BayegBase) FROM dbo.DEED_HED";
                            int maxBayeg = db.ExecuteScalar<int>(maxBayegSql, new { BayegBase = BayegBase }, transaction);

                            // Ensure no duplicates by checking if the calculated BAYEG is already taken
                            while (db.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.DEED_HED WHERE BAYEG = @MaxBayeg", new { MaxBayeg = maxBayeg }, transaction) > 0)
                            {
                                maxBayeg++; // Increment to ensure uniqueness
                            }

                            // Attempt to update the BAYEG value for a specific N_S row where BAYEG is NULL
                            string updateSql = @"
                            UPDATE dbo.DEED_HED
                            SET BAYEG = @NewBayeg
                            OUTPUT INSERTED.BAYEG
                            WHERE N_S = @N_S AND (BAYEG IS NULL) OR (BAYEG = 0)";

                            var result = db.QueryFirstOrDefault<int?>(updateSql, new { NewBayeg = maxBayeg, N_S = n_s }, transaction: transaction);

                            // Check if we got a valid BAYEG from the update
                            if (result.HasValue)
                            {
                                transaction.Commit();
                                return result.Value;
                            }
                        }

                        return null;
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        try { File.AppendAllText("C:\\CORRECT\\DBMSLOG.txt", $"\n{DateTime.Now} - Error in UpdateOrGenerateBAYEG:\n{ex}\n"); } catch { }
                        throw; // Re-throw to handle higher up if necessary
                    }
                }
            }
        }
        public static int? UpdateOrGenerateBAYEG_BASEY(int BayegBase, int n_s)
        {
            const int _bayeganbase_ = 100000000;
            var effectiveBase = BayegBase <= 0 ? _bayeganbase_ : BayegBase;

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        const int defaultCommandTimeout = 60;
                        // First try to set BAYEG for the requested row using the BASE column to avoid heavy updates
                        const string updateCurrentRowSql = @"
                            UPDATE dbo.DEED_HED
                            SET BAYEG = @BaseOffset + BASE
                            OUTPUT INSERTED.BAYEG
                            WHERE N_S = @N_S AND (BAYEG IS NULL OR BAYEG = 0);";
                        var updatedBayeg = db.QueryFirstOrDefault<int?>(new CommandDefinition(updateCurrentRowSql,
                            new { BaseOffset = _bayeganbase_, N_S = n_s }, transaction: transaction, commandTimeout: defaultCommandTimeout));

                        if (updatedBayeg.HasValue)
                        {
                            transaction.Commit();
                            return updatedBayeg.Value;
                        }

                        // Check whether the row already has a BAYEG assigned
                        var existingBayeg = db.QueryFirstOrDefault<int?>(
                            new CommandDefinition(
                                "SELECT BAYEG FROM dbo.DEED_HED WHERE N_S = @N_S",
                                new { N_S = n_s },
                                transaction: transaction,
                                commandTimeout: defaultCommandTimeout));

                        if (existingBayeg.HasValue && existingBayeg.Value > 0)
                        {
                            transaction.Commit();
                            return existingBayeg.Value;
                        }

                        // Generate a new BAYEG value while keeping the table locked to avoid duplicates
                        var nextBayeg = db.ExecuteScalar<int>(
                            new CommandDefinition(

                                @"SELECT ISNULL(MAX(BAYEG), @Base) FROM dbo.DEED_HED WITH (UPDLOCK, HOLDLOCK)",
                                new { Base = effectiveBase },
                                transaction: transaction,
                                commandTimeout: defaultCommandTimeout));
                        if (nextBayeg < effectiveBase)
                        {
                            nextBayeg = effectiveBase;
                        }

                        nextBayeg += 1;

                        // Persist the generated BAYEG value for the row
                        var generatedBayeg = db.QueryFirstOrDefault<int?>(
                            new CommandDefinition(
                                @"UPDATE dbo.DEED_HED
                                  SET BAYEG = @NewBayeg
                                  OUTPUT INSERTED.BAYEG
                                  WHERE N_S = @N_S;",
                                new { NewBayeg = nextBayeg, N_S = n_s },
                                transaction: transaction,
                                commandTimeout: defaultCommandTimeout));

                        transaction.Commit();
                        return generatedBayeg;
                    }
                    catch (Exception ex)
                    {
                        try { File.AppendAllText("C:\\CORRECT\\DBMSLOG.txt", $"\n{DateTime.Now} - Error in UpdateOrGenerateBAYEG:\n{ex}\n"); } catch { }
                        transaction.Rollback();
                        throw; // Re-throw to handle higher up if necessary
                    }
                }
            }
        }

        public static int? UpdateOrGenerateBAYEG_______(int BayegBase, int n_s)
        {
            const int _bayeganbase_ = 100000000;
            if (BayegBase == default || BayegBase == null)
            {
                BayegBase = _bayeganbase_ + 1;
            }
            else
            {
                BayegBase += 1;
            }

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        const int commandTimeoutSeconds = 180;

                        //1. Get Exiting BAYEG of Current Row
                        var existingBayegCommand = new CommandDefinition("SELECT BAYEG FROM dbo.DEED_HED WHERE N_S = @N_S",
                            new { N_S = n_s }, transaction: transaction, commandTimeout: commandTimeoutSeconds);

                        var existingBayeg = db.QueryFirstOrDefault<int?>(existingBayegCommand);
                        if (existingBayeg.HasValue && existingBayeg.Value > 0)
                        {
                            transaction.Commit();
                            return existingBayeg.Value;
                        }

                        //2. Get New Max BAYEG of Current Row
                        var maxBayegCommand = new CommandDefinition(
                                              "SELECT ISNULL(MAX(BAYEG) + 1, @BayegBase) FROM dbo.DEED_HED WITH (UPDLOCK, HOLDLOCK)",
                                              new { BayegBase = BayegBase },
                                              transaction: transaction,
                                              commandTimeout: commandTimeoutSeconds);

                        //3. Re Check the Generated New Max BAYEG to avoid duplicate
                        var maxBayeg = db.ExecuteScalar<int>(maxBayegCommand);
                        while (db.ExecuteScalar<int>(new CommandDefinition("SELECT COUNT(1) FROM dbo.DEED_HED WHERE BAYEG = @MaxBayeg",
                                                        new { MaxBayeg = maxBayeg },
                                                        transaction: transaction,
                                                        commandTimeout: commandTimeoutSeconds)) > 0)
                        {
                            maxBayeg++;
                        }

                        //4. Finally Update BAYEG of Current row to new value
                        const string updateSql = @"
                            UPDATE dbo.DEED_HED
                            SET BAYEG = @NewBayeg
                            OUTPUT INSERTED.BAYEG
                            WHERE N_S = @N_S AND (BAYEG IS NULL OR BAYEG = 0)";

                        var updateCommand = new CommandDefinition(
                                                 updateSql,
                                                 new { NewBayeg = maxBayeg, N_S = n_s },
                                                 transaction: transaction,
                                                 commandTimeout: commandTimeoutSeconds);
                        var result = db.QueryFirstOrDefault<int?>(updateCommand);

                        if (result.HasValue)
                        {
                            transaction.Commit();
                            return result.Value;
                        }

                        transaction.Commit();
                        return null;
                    }
                    catch (Exception ex)
                    {
                        try { File.AppendAllText("C:\\CORRECT\\DBMSLOG.txt", $"\n{DateTime.Now} - Error in UpdateOrGenerateBAYEG:\n{ex}\n"); } catch { }
                        transaction.Rollback();
                        throw; // Re-throw to handle higher up if necessary
                    }
                }
            }
        }


        public static int? UpdateOrGenerateBAYEG(int BayegBase, int n_s)
        {
            const int _bayeganbase_ = 100000000;
            var baseCandidate = BayegBase <= 0 ? _bayeganbase_ : BayegBase;
            if (baseCandidate < _bayeganbase_)
            {
                baseCandidate = _bayeganbase_;
            }
            var assignmentBase = baseCandidate + 1;

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        const int commandTimeoutSeconds = 300;

                        //1. Get Exiting BAYEG of Current Row
                        var existingBayegCommand = new CommandDefinition("SELECT BAYEG FROM dbo.DEED_HED WHERE N_S = @N_S",
                            new { N_S = n_s }, transaction: transaction, commandTimeout: commandTimeoutSeconds);

                        var existingBayeg = db.QueryFirstOrDefault<int?>(existingBayegCommand);
                        if (existingBayeg.HasValue && existingBayeg.Value > 0)
                        {
                            transaction.Commit();
                            return existingBayeg.Value;
                        }

                        const string assignSql = @"
                            ;WITH NextValue AS (
                                SELECT NextBayeg = ISNULL(MAX(BAYEG), @AssignmentBase - 1)
                                FROM dbo.DEED_HED WITH (UPDLOCK, HOLDLOCK)
                                WHERE BAYEG >= @AssignmentBase - 1
                            )
                            UPDATE DH WITH (ROWLOCK)
                            SET BAYEG = NextValue.NextBayeg + 1
                            OUTPUT INSERTED.BAYEG
                            FROM dbo.DEED_HED AS DH
                            CROSS JOIN NextValue
                            WHERE DH.N_S = @N_S AND (DH.BAYEG IS NULL OR DH.BAYEG = 0);";
                        var assignedBayeg = db.QueryFirstOrDefault<int?>(new CommandDefinition(
                                                  assignSql,
                                                  new { AssignmentBase = assignmentBase, N_S = n_s },
                                                  transaction: transaction,
                                                  commandTimeout: commandTimeoutSeconds));
                        if (assignedBayeg.HasValue && assignedBayeg.Value > 0)
                        {
                            transaction.Commit();
                            return assignedBayeg.Value;
                        }

                        // If no assignment happened, fall back to reading whatever exists.
                        var finalBayeg = db.QueryFirstOrDefault<int?>(new CommandDefinition(
                            "SELECT BAYEG FROM dbo.DEED_HED WHERE N_S = @N_S",
                            new { N_S = n_s },
                            transaction: transaction,
                            commandTimeout: commandTimeoutSeconds));

                        transaction.Commit();
                        return finalBayeg;
                    }
                    catch (Exception ex)
                    {
                        try { File.AppendAllText("C:\\CORRECT\\DBMSLOG.txt", $"\n{DateTime.Now} - Error in UpdateOrGenerateBAYEG:\n{ex}\n"); } catch { }
                        transaction.Rollback();
                        throw; // Re-throw to handle higher up if necessary
                    }
                }
            }
        }

        public static string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || maxLength <= 0)
                return string.Empty;

            // Check if the string needs truncation
            if (input.Length > maxLength)
            {
                return input.Substring(0, maxLength);
            }

            // No truncation needed
            return input;
        }

        public static void AMALIYAT_USER(string frm)
        {
            try
            {
                using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                {
                    db.Open();

                    string username = "MCR | " + Baseknow.UUSER;
                    string _FRM_ = frm;

                    var sql = "INSERT INTO AMALIAT (USERID,USERNAME,ADATE,AMALID) VALUES (@UserId, @Username, GETDATE(), @AmalId)";
                    var parameters = new
                    {
                        UserId = Baseknow.USERCOD,
                        Username = TruncateString(username, 49),
                        AmalId = TruncateString(_FRM_, 49)
                    };

                    db.Execute(sql, parameters);
                }
            }
            catch { }
        }

        public static long Lastwdate()
        {
            long tempLastwdate = 0;
            var rst = dbms.DoGetDataSQL<long?>("SELECT  WDATE  FROM DBO.WORKHEAD ORDER BY WDATE DESC").FirstOrDefault();
            if (rst != null)
            {
                tempLastwdate = (long)rst; //wdate
            }
            else
            {
                tempLastwdate = 0;
            }
            return tempLastwdate;
        }
        public static double MORAKH_MAND(long MDATE, long PERCODE)
        {
            double MORAKH_MANDRet = default;
            string t;
            double mandaval;
            double worked;
            int daytonow;

            // مرخصي ابتداي قرار داد
            var mandrst = dbms.DoGetDataSQL<int?>("SELECT MORAKH FROM dbo.PGHARAR WHERE (CODE = " + PERCODE + ") ORDER BY GHSTART").FirstOrDefault();
            if (mandrst != null)
            {
                mandaval = (double)mandrst; //MORAKH
            }
            else
            {
                mandaval = 0d;
            }
            // تا قبل از درخواست مرخصي كل كاركرد طبق ليست
            var GRST = dbms.DoGetDataSQL<int?>("SELECT  SUM(DAYS) AS worked FROM dbo.WORKING WHERE (WDATE <= " + MDATE + " and PCODE = " + PERCODE + ")").FirstOrDefault();
            if (GRST != null) //worked
            {
                worked = (double)GRST; //worked
            }
            else
            {
                worked = 0d;
            }
            // روزهاي كاركرد تا امروز كه هنوز توي ليست ثبتنشده
            if (MDATE > Lastwdate())
            {
                daytonow = Convert.ToInt32(UDay(MDATE.ToString()));
            }
            else
            {
                daytonow = 0;
            }
            worked = worked + daytonow;
            // مرخصي ثبت شدهاستحقاقي و ساعتي
            var MRST = dbms.DoGetDataSQL<LSQ4>("SELECT PMORAKH.CODE, Sum(PMORAKH.MORAKHDAY) AS SumOfMORAKHDAY FROM PMORAKH WHERE (PMORAKH.MODATE <= " + MDATE + " and PMORAKH.KINDM = 0 or  PMORAKH.KINDM = 2) GROUP BY PMORAKH.CODE HAVING (((PMORAKH.CODE)=" + PERCODE + "))").FirstOrDefault();
            if (MRST != null)
            {
                MORAKH_MANDRet = (double)(worked * (26d / 365d) * 440d - MRST.SumOfMORAKHDAY + mandaval);
            }
            else
            {
                MORAKH_MANDRet = worked * (26d / 365d) * 440d + mandaval;
            }
            return MORAKH_MANDRet;
        }

        public static long DIFF(long fromdate, long to_date)
        {
            long DIFFRet = default;
            long tmp;
            int S1, M1, r1, S2, M2, R2;
            float SUMATION;
            bool flag;
            flag = false;
            if (fromdate == 0L || IsNull(fromdate) == true || to_date == 0L || IsNull(to_date) == true)
            {
                DIFFRet = 0L;
                return DIFFRet;
            }
            if (fromdate > to_date)
            {
                flag = true;
                tmp = fromdate;
                fromdate = to_date;
                to_date = tmp;
            }
            r1 = ROOZ(fromdate);
            M1 = mah(fromdate);
            S1 = SAL(fromdate);
            R2 = ROOZ(to_date);
            M2 = mah(to_date);
            S2 = SAL(to_date);
            SUMATION = 0f;

            while (S1 < S2 - 1 || S1 == S2 - 1 && (M1 < M2 || M1 == M2 && r1 <= R2))
            {
                if (KABISEH(S1) == 1)
                {
                    if (M1 == 12 && r1 == 30)
                    {
                        SUMATION = SUMATION + 365f;
                        r1 = 29;
                    }
                    else
                    {
                        SUMATION = SUMATION + 366f;
                    }
                }
                else
                {
                    SUMATION = SUMATION + 365f;
                }
                S1 = S1 + 1;
            }

            while (S1 < S2 || M1 < M2 - 1 || M1 == M2 - 1 && r1 < R2)
            {
                switch (M1)
                {
                    case var @case when 1 <= @case && @case <= 6:
                        {
                            if (M1 == 6 && r1 == 31)
                            {
                                SUMATION = SUMATION + 30f;
                                r1 = 30;
                            }
                            else
                            {
                                SUMATION = SUMATION + 31f;
                            }
                            M1 = M1 + 1;
                            break;
                        }
                    case var case1 when 7 <= case1 && case1 <= 11:
                        {
                            if (M1 == 11 && r1 == 30 & KABISEH(S1) == 0)
                            {
                                SUMATION = SUMATION + 29f;
                                r1 = 29;
                            }
                            else
                            {
                                SUMATION = SUMATION + 30f;
                            }
                            M1 = M1 + 1;
                            break;
                        }
                    case 12:
                        {
                            if (KABISEH(S1) == 1)
                            {
                                SUMATION = SUMATION + 30f;
                            }
                            else
                            {
                                SUMATION = SUMATION + 29f;
                            }
                            S1 = S1 + 1;
                            M1 = 1;
                            break;
                        }
                }
            }

            if (M1 == M2)
            {
                SUMATION = SUMATION + (R2 - r1);
            }
            else
            {
                switch (M1)
                {
                    case var case2 when 1 <= case2 && case2 <= 6:
                        {
                            SUMATION = SUMATION + (31 - r1) + R2;
                            break;
                        }
                    case var case3 when 7 <= case3 && case3 <= 11:
                        {
                            SUMATION = SUMATION + (30 - r1) + R2;
                            break;
                        }
                    case 12:
                        {
                            if (KABISEH(S1) == 1)
                            {
                                SUMATION = SUMATION + (30 - r1) + R2;
                            }
                            else
                            {
                                SUMATION = SUMATION + (29 - r1) + R2;
                            }

                            break;
                        }
                }
            }
            if (flag == true)
            {
                SUMATION = -SUMATION;
            }
            DIFFRet = (long)Math.Round(SUMATION);
            return DIFFRet;

        }

        /// <summary>
        /// ارسال فرمت فاکتور
        /// </summary>
        /// <param name="NUMBER"></param>
        /// <returns></returns>
        public static string CREATE_SMSFR(long NUMBER)
        {
            string CREATE_SMSFRRet;
            long CHK;
            double JAMFACT;
            string PAYAM = "";

            // Retrieve the header record (HEAD_LST joined with CUST_HESAB and OTHER_DTL)
            // Note: LSQ5 is assumed to include properties for all the columns used below.
            var rst3 = dbms.DoGetDataSQL<LSQ5>(
                "SELECT dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.NUMBER1, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MAS, dbo.HEAD_LST.VAS, dbo.HEAD_LST.N_S, " +
                "dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.M_NAGHD, dbo.HEAD_LST.MABL_VAR, dbo.HEAD_LST.MOIN_VAR, dbo.HEAD_LST.MABL_HAV, dbo.HEAD_LST.MABL_HAZ, " +
                "dbo.HEAD_LST.TAKHFIF, dbo.HEAD_LST.FNUMCO, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.MBAA, dbo.CUST_HESAB.NAME, " +
                "dbo.OTHER_DTL.REQUEST_NO, dbo.OTHER_DTL.BARNAMEH, dbo.OTHER_DTL.DRIVER, dbo.OTHER_DTL.DRIVER_MOB, dbo.OTHER_DTL.CAMIUN_NUM, dbo.OTHER_DTL.MAGHSAD, " +
                "dbo.OTHER_DTL.CAM_KHALY, dbo.OTHER_DTL.CAM_POOR, dbo.OTHER_DTL.TOZIH " +
                "FROM dbo.HEAD_LST " +
                "INNER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes " +
                "LEFT OUTER JOIN dbo.OTHER_DTL ON dbo.HEAD_LST.NUMBER = dbo.OTHER_DTL.NUMBER AND dbo.HEAD_LST.TAG = dbo.OTHER_DTL.TAG " +
                "WHERE (dbo.HEAD_LST.TAG = 13) AND (dbo.HEAD_LST.NUMBER = " + NUMBER + ")"
            ).FirstOrDefault();

            if (rst3 != null)
            {
                // Calculate CHK from PAY_GETD (for TAG = 2)
                var rst4 = dbms.DoGetDataSQL<double?>(
                    "SELECT sum(MABL) as mabl FROM dbo.PAY_GETD WHERE (NUMBER = " + NUMBER + ") And (TAG = 2)"
                ).FirstOrDefault();

                if (rst4 != null && rst4 > 0)
                {
                    CHK = (long)rst4;
                }
                else
                {
                    CHK = 0L;
                }

                // Retrieve invoice summary for TAG = 2
                var RST2 = dbms.DoGetDataSQL<LSQ6>(
                    "SELECT SUM(MABL_K) AS SumOfMABL_K, SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE (NUMBER = " + NUMBER + ") AND (TAG = 2)"
                ).FirstOrDefault();

                if (RST2 != null && RST2.SumOfMABL_K.HasValue)
                {
                    // In this SMSFR version both branches of the IIf are identical.
                    JAMFACT = (RST2.SumOfMABL_K ?? 0) + (rst3.MABL_HAZ ?? 0) + (rst3.MBAA ?? 0) - (rst3.TAKHFIF ?? 0);
                }
                else
                {
                    JAMFACT = 0d;
                }

                // Retrieve SMS formatting details (SMSTY = 1)
                var rst = dbms.DoGetDataSQL<SMS_FORMATS_MODEL>(
                    "SELECT * FROM SMS_FORMATS WHERE SMSTY = 1 ORDER BY radif"
                ).ToList();

                if (rst.Count > 0)
                {
                    foreach (var format in rst)
                    {
                        switch (format.SMS_FIELS)
                        {
                            case "ENTER":
                                PAYAM += format.SMS_CAPTION + "\r";
                                break;
                            case "null":
                                PAYAM += format.SMS_CAPTION;
                                break;
                            case "SPC":
                                PAYAM += format.SMS_CAPTION + " ";
                                break;
                            case "CUST_NO1":
                                PAYAM += format.SMS_CAPTION + rst3.NAME;
                                break;
                            case "CUST_NO":
                                PAYAM += format.SMS_CAPTION + rst3.CUST_NO;
                                break;
                            case "MOLAH":
                                PAYAM += format.SMS_CAPTION + rst3.MOLAH;
                                break;
                            case "NUMBER":
                                PAYAM += format.SMS_CAPTION + rst3.NUMBER;
                                break;
                            case "DATE_N":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:####/##/##}", rst3.DATE_N);
                                break;
                            case "MABL_K":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", RST2.SumOfMABL_K);
                                break;
                            case "TAKHFIF":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", rst3.TAKHFIF);
                                break;
                            case "MABL_HAZ":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", rst3.MABL_HAZ);
                                break;
                            case "MBAA":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", rst3.MBAA);
                                break;
                            case "GHABEL":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", JAMFACT);
                                break;
                            case "NPAR":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", CHK + (rst3.M_NAGHD ?? 0) + (rst3.MABL_VAR ?? 0) + (rst3.MABL_HAV ?? 0));
                                break;
                            case "MANF":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", JAMFACT - (CHK + (rst3.M_NAGHD ?? 0) + (rst3.MABL_VAR ?? 0) + (rst3.MABL_HAV ?? 0)));
                                break;
                            case "MANH":
                                PAYAM += format.SMS_CAPTION + string.Format("{0:#,### ريال}", GETMANDAH(rst3.CUST_NO));
                                break;
                            case "MEGH_K":
                                PAYAM += format.SMS_CAPTION + RST2.MEGHk;
                                break;
                            case "REQUEST_NO":
                                PAYAM += format.SMS_CAPTION + rst3.REQUEST_NO;
                                break;
                            case "BARNAMEH":
                                PAYAM += format.SMS_CAPTION + rst3.BARNAMEH;
                                break;
                            case "DRIVER":
                                PAYAM += format.SMS_CAPTION + rst3.DRIVER;
                                break;
                            case "DRIVER_MOB":
                                PAYAM += format.SMS_CAPTION + rst3.DRIVER_MOB;
                                break;
                            case "CAMIUN_NUM":
                                PAYAM += format.SMS_CAPTION + rst3.CAMIUN_NUM;
                                break;
                            case "TOZIH":
                                PAYAM += format.SMS_CAPTION + rst3.TOZIH;
                                break;
                            case "VAZN":
                                {
                                    var RST5 = dbms.DoGetDataSQL<double?>(
                                        "SELECT SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight " +
                                        "FROM dbo.INVO_LST INNER JOIN dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE " +
                                        "WHERE (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER + ")"
                                    ).FirstOrDefault();

                                    if (RST5 != null)
                                    {
                                        PAYAM += format.SMS_CAPTION + Math.Round(RST5.Value);
                                    }
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    // Fallback message when no SMS_FORMATS records are found.
                    PAYAM = "فاكتور شماره :" + NUMBER + "\r" +
                            "مورخ:" + string.Format("{0:####/##/##}", rst3.DATE_N) + "\r" +
                            "مبلغ فاكتور :" + string.Format("{0:#,### ريال}", JAMFACT) + "\r" +
                            "مقدار كل :" + RST2.MEGHk + "\r" +
                            "مانده حساب :" + GETMANDAH(rst3.CUST_NO) + "\r" +
                            Baseknow.SMS_OWNER;
                }
            }

            CREATE_SMSFRRet = PAYAM;
            return CREATE_SMSFRRet;
        }

        /// <summary>
        /// ارسال فرمت فاکتور خرید
        /// </summary>
        /// <param name="NUMBER"></param>
        /// <returns></returns>
        public static string CREATE_SMSKH(long NUMBER)
        {
            string CREATE_SMSKHRet = default;
            long CHK;
            double JAMFACT;
            string PAYAM = "";
            var rst3 = dbms.DoGetDataSQL<LSQ5>("SELECT     dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.NUMBER1, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MAS, dbo.HEAD_LST.VAS, dbo.HEAD_LST.N_S, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.M_NAGHD, dbo.HEAD_LST.MABL_VAR, dbo.HEAD_LST.MOIN_VAR,dbo.HEAD_LST.MABL_HAV, dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.TAKHFIF, dbo.HEAD_LST.FNUMCO, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.SHARAYET,dbo.HEAD_LST.MBAA , dbo.CUST_HESAB.NAME FROM         dbo.HEAD_LST INNER JOIN  dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes WHERE     (dbo.HEAD_LST.TAG = 12) and (dbo.HEAD_LST.NUMBER = " + NUMBER + ")").FirstOrDefault();
            if (rst3 != null)
            {
                var rst4 = dbms.DoGetDataSQL<double?>("SELECT  sum(MABL) as mabl FROM dbo.PAY_GETD WHERE (NUMBER =  " + NUMBER + ") And (TAG = 1)").FirstOrDefault();
                if (rst4 != null && rst4 > 0)
                {
                    CHK = (long)rst4;
                }
                else
                {
                    CHK = 0L;
                }
                var RST2 = dbms.DoGetDataSQL<LSQ6>("SELECT SUM(MABL_K) AS SumOfMABL_K,SUM(MEGHk) AS MEGHk FROM dbo.INVO_LST WHERE     (NUMBER = " + NUMBER + ") AND (TAG = 1) ").FirstOrDefault();
                if (RST2 != null && RST2?.SumOfMABL_K > 0)
                {
                    JAMFACT = Conversions.ToDouble(Interaction.IIf(rst3.VAS == 1 || IsNull(rst3.VAS), RST2.SumOfMABL_K + rst3.MABL_HAZ + rst3.MBAA - rst3.TAKHFIF, RST2.SumOfMABL_K - rst3.MABL_HAZ + rst3.MBAA - rst3.TAKHFIF));
                }
                else
                {
                    JAMFACT = 0d;
                }
                var rst = dbms.DoGetDataSQL<SMS_FORMATS_MODEL>("SELECT * FROM SMS_FORMATS WHERE SMSTY  = 2 ORDER BY RADIF").ToList();
                if (rst.Count > 0)
                {
                    for (int i = 0; i < rst.Count; i++) //while (!rst.EOF)
                    {
                        switch (rst[i].SMS_FIELS)
                        {
                            case "ENTER":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + '\r';
                                    break;
                                }
                            case "null":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION;
                                    break;
                                }
                            case "SPC":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + " ";
                                    break;
                                }
                            case "CUST_NO1":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + rst3.NAME;
                                    break;
                                }
                            case "CUST_NO":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + rst3.CUST_NO;
                                    break;
                                }
                            case "MOLAH":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + rst3.MOLAH;
                                    break;
                                }
                            case "NUMBER":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + rst3.NUMBER;
                                    break;
                                }
                            case "DATE_N":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(rst3.DATE_N, "####/##/##");
                                    break;
                                }
                            case "MABL_K":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(RST2.SumOfMABL_K, "#,### ريال");
                                    break;
                                }
                            case "TAKHFIF":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(rst3.TAKHFIF, "#,### ريال");
                                    break;
                                }
                            case "MABL_HAZ":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(rst3.MABL_HAZ, "#,### ريال");
                                    break;
                                }
                            case "MBAA":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(rst3.MBAA, "#,### ريال");
                                    break;
                                }
                            case "GHABEL":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(JAMFACT, "#,### ريال");
                                    break;
                                }
                            case "NPAR":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(CHK + rst3.M_NAGHD + rst3.MABL_VAR + rst3.MABL_HAV, "#,### ريال");
                                    break;
                                }
                            case "MANF":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(JAMFACT - (CHK + rst3.M_NAGHD + rst3.MABL_VAR + rst3.MABL_HAV), "#,### ريال");
                                    break;
                                }
                            case "MANH":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + Strings.Format(GETMANDAH(rst3.CUST_NO), "#,### ريال");
                                    break;
                                }
                            case "MEGH_K":
                                {
                                    PAYAM = PAYAM + rst[i].SMS_CAPTION + RST2.MEGHk;
                                    break;
                                }
                            case "VAZN":
                                {
                                    var RST5 = dbms.DoGetDataSQL<double?>("SELECT     SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 2) AND (dbo.INVO_LST.NUMBER = " + NUMBER + ")").FirstOrDefault();
                                    if (RST5 != null)
                                    {
                                        if (!IsNull(rst[i]))
                                        {
                                            PAYAM = PAYAM + rst[i].SMS_CAPTION + Math.Round((double)RST5);
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
                else
                {
                    PAYAM = "فاكتور خريد شماره :" + NUMBER + '\r' + "مورخ:" + Strings.Format(rst3.DATE_N, "####/##/##") + '\r' + "مبلغ فاكتور :" + Strings.Format(JAMFACT, "#,### ريال") + '\r' + "مقدار كل :" + RST2.MEGHk + '\r' + "مانده حساب :" + GETMANDAH(rst3.CUST_NO) + '\r' + Baseknow.SMS_OWNER;
                }
            }
            CREATE_SMSKHRet = PAYAM;
            return CREATE_SMSKHRet;
        }

        public static bool MOGUDI(long NUMBERu, int TAGG)
        {
            // Mimic the VBA "On Error Resume Next" by catching exceptions.
            bool NOTPR = false;
            // Check if RMOG is set (assumed to be a property on a global Baseknow object)
            NOTPR = false;

            string query = $"SELECT * FROM invo_lst WHERE NUMBER = {NUMBERu} AND TAG = {TAGG}";
            var invoLstRecords = dbms.DoGetDataSQL<INVO_LST_CSHARP>(query).ToList();
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            foreach (var record in invoLstRecords)
            {
                double minValue = Getmin((int)record.ANBAR, record.CODE);

                string sqlMand =
                    "SELECT ROUND(ISNULL(AK_MOGO_AVL_KOL.SMEGH, 0) - ISNULL(AK_MOGO_FR.MEG, 0),2) AS mand " +
                    "FROM dbo.AK_MOGO_AVL_KOL(99999999, " + record.ANBAR + ") AK_MOGO_AVL_KOL " +
                    "RIGHT OUTER JOIN dbo.STUF_FSK ON AK_MOGO_AVL_KOL.CODE = dbo.STUF_FSK.CODE AND AK_MOGO_AVL_KOL.ANBAR = dbo.STUF_FSK.ANBAR " +
                    "LEFT OUTER JOIN dbo.AK_MOGO_FR(99999999, " + record.ANBAR + ") AK_MOGO_FR ON dbo.STUF_FSK.CODE = AK_MOGO_FR.CODE AND dbo.STUF_FSK.ANBAR = AK_MOGO_FR.ANBAR " +
                    "WHERE (dbo.STUF_FSK.CODE = N'" + record.CODE + "') AND (dbo.STUF_FSK.ANBAR = " + record.ANBAR + ")";

                var mandResult = dbms.DoGetDataSQL<double?>(sqlMand).FirstOrDefault();

                if (mandResult != null)
                {
                    double mandValue = mandResult ?? 0.0;

                    if (Math.Round((double)mandValue, (int)Baseknow.DIG) < Math.Round((double)minValue, (int)Baseknow.DIG) && record.ANBAR != 0)
                    {
                        string message = "خروج كالاي " + GETKALANAME(Convert.ToDouble(record.CODE)) +
                                         " از انبار موجودي را به مقدار غير مجاز كاهش ميدهد." +
                                         "حداقل موجودي تعريف شده در اف دو :" + minValue;

                        ErrosMessages.Add(new MsgModel { MessageText_U = message });

                        NOTPR = true;
                    }
                }
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
            }

            return !NOTPR;
        }


        public static int GETUSERCOD(string US)
        {
            int tempGETUSERCOD = 0;
            var rst = dbms.DoGetDataSQL<int?>("SELECT IDD FROM dbo.SALA_DTL WHERE (SAL_NAME = N'" + CODESAL(Fixp(US)) + "') or (SAL_NAME = N'" + CODESAL(US) + "')").FirstOrDefault();
            if (rst != null)
            {
                tempGETUSERCOD = (int)rst;
            }
            return tempGETUSERCOD;
        }

        public static string GETHESMOBIL(string HES)
        {
            string tempGETHESMOBIL = "";
            var RRST = dbms.DoGetDataSQL<string?>("SELECT CUST_HESAB.MOBILE FROM CUST_HESAB WHERE (CUST_HESAB.HES='" + HES + "')").FirstOrDefault();
            if (!string.IsNullOrEmpty(RRST))
            {
                tempGETHESMOBIL = RRST;
            }
            else
            {
                tempGETHESMOBIL = "";
            }
            return tempGETHESMOBIL;
        }

        public static string GETVisitorname(long NUMBER, int TAG)
        {
            string tempGETVisitorname = null;

            var rst = dbms.DoGetDataSQL<string?>("SELECT CUST_NO FROM dbo.VISITOR_DTL WHERE NUMBER = " + NUMBER + " AND TAG = " + TAG + "ORDER BY CRT").FirstOrDefault();
            if (rst != null)
            {
                tempGETVisitorname = GETHESNAME(rst);
            }
            else
            {
                tempGETVisitorname = "";
            }
            return tempGETVisitorname;
        }
        public static string GETVisitorHES(long NUMBER, int TAG)
        {
            string tempGETVisitorHES = null;
            var rst = dbms.DoGetDataSQL<string?>("SELECT CUST_NO FROM dbo.VISITOR_DTL WHERE NUMBER = " + NUMBER + " AND TAG = " + TAG + "ORDER BY CRT").FirstOrDefault();
            if (rst != null)
            {
                tempGETVisitorHES = rst;
            }
            else
            {
                tempGETVisitorHES = "";
            }
            return tempGETVisitorHES;
        }

        /// <summary>
        /// Executes the dbo.CalculateVisitorPorsant stored procedure and returns any SQL messages.
        /// </summary>
        /// <param name="number">Invoice number.</param>
        /// <param name="tag">Invoice tag.</param>
        /// <param name="visitorId">Optional visitor account number. If null the procedure auto detects.</param>
        /// <returns>A list of informational messages produced by SQL Server.</returns>
        public static List<string> RunCalculateVisitorPorsant(long number, short tag, string? visitorId = null)
        {
            List<string> messages = new List<string>();

            using (var conn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            using (var cmd = new SqlCommand("dbo.CalculateVisitorPorsant", conn))
            {
                conn.InfoMessage += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Message))
                        messages.Add(e.Message);
                };
                conn.FireInfoMessageEventOnUserErrors = true;

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NUMBER", number);
                cmd.Parameters.AddWithValue("@TAG", tag);
                var paramVisitor = cmd.Parameters.Add("@VisitorID", SqlDbType.NVarChar, 40);
                if (string.IsNullOrWhiteSpace(visitorId))
                    paramVisitor.Value = DBNull.Value;
                else
                    paramVisitor.Value = visitorId;

                var paramLog = cmd.Parameters.Add("@LOG", SqlDbType.NVarChar, -1);
                paramLog.Value = DBNull.Value;

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return messages;
        }

        public static string CODEPAL(string ps)
        {
            if (string.IsNullOrEmpty(ps))
                return string.Empty;

            StringBuilder nps = new StringBuilder();
            for (int i = 0; i < ps.Length; i++)
            {
                // کاراکتر مورد نظر را تبدیل و سپس 10 واحد کم می‌کند
                nps.Append((char)(ps[i] - 10));
            }

            // مثل Randomize و Rnd در VBA
            string res = "p" +
                         GetRandomChar() +
                         GetRandomChar() +
                         nps +
                         GetRandomChar() +
                         GetRandomChar() +
                         "z";

            return res;
        }

        private static char GetRandomChar()
        {
            // تولید عدد رندم بین 0 تا 99 و تبدیل به کاراکتر
            return (char)_rnd.Next(0, 100);
        }

        public static long LASTPRICENZ(string code, long dt)
        {
            // 1) آخرین قیمت خرید (MABL) غیرصفر تا تاریخ dt
            const string sql1 = @"
            SELECT TOP (1) il.MABL
            FROM dbo.HEAD_LST AS hl
            JOIN dbo.INVO_LST AS il
              ON hl.[NUMBER] = il.[NUMBER] AND hl.TAG = il.TAG
            WHERE il.MABL <> 0
              AND hl.TAG = 1
              AND il.CODE = @CODE
              AND hl.DATE_N <= @DT
            ORDER BY hl.DATE_N DESC, il.MABL DESC;";

            var mabl = dbms.DoGetDataSQL<double?>(sql1, new { CODE = code, DT = dt }).FirstOrDefault();
            if (mabl.HasValue)
                return Convert.ToInt64(mabl.Value);

            // 2) در صورت نبود رکورد/مقدار، از STUF_FSK بگیر (بیشترین FI_A)
            const string sql2 = @"
            SELECT TOP (1) FI_A
            FROM dbo.STUF_FSK
            WHERE CODE = @CODE
            ORDER BY FI_A DESC;";

            var fiA = dbms.DoGetDataSQL<double?>(sql2, new { CODE = code }).FirstOrDefault();
            if (fiA.HasValue && fiA.Value != 0)
                return Convert.ToInt64(fiA.Value);

            // 3) در صورت نبود یا صفر بودن، از DTL_MANF بگیر (بیشترین SMABL)
            const string sql3 = @"
            SELECT TOP (1) SMABL
            FROM dbo.DTL_MANF
            WHERE CODE = @CODE
            ORDER BY SMABL DESC;";

            var smabl = dbms.DoGetDataSQL<double?>(sql3, new { CODE = code }).FirstOrDefault();

            return smabl.HasValue ? Convert.ToInt64(smabl.Value) : 0L;
        }
    }
}
