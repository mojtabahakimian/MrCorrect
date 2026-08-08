using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AUTO_BAZ.Functions;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Microsoft.Data.SqlClient;

namespace TestRunner
{
    internal class Program
    {
        public class DtlRow
        {
            public double N_S { get; set; }
            public int HES_K { get; set; }
            public int HES_M { get; set; }
            public int HES_T { get; set; }
            public string HES { get; set; }
            public string SHARH { get; set; }
            public double BED { get; set; }
            public double BES { get; set; }
            public double NUMBER { get; set; }
            public double TAG { get; set; }
        }

        static void Main(string[] args)
        {
            CL_Generaly.IsCalledExternally = true;
            CL_CCNNMANAGER.CONNECTION_STR = "Data Source=MERCEDES\\SQL2022;Initial Catalog=YAZDSEPAR1405;Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            CL_CCNNMANAGER.ConnectedToSQLDB = true;

            Baseknow.GetInitTheApp();
            var dbms = new CL_CCNNMANAGER();

            Console.WriteLine("=========================================================================");
            Console.WriteLine("     DEEP ROW-BY-ROW CONTENT AUDIT & BENCHMARK: GENSANADBARGASHFROOSH2   ");
            Console.WriteLine("=========================================================================");

            var tag25Count = dbms.DoGetDataSQL<int>("SELECT COUNT(*) FROM dbo.HEAD_LST WHERE TAG = 25").FirstOrDefault();
            Console.WriteLine($"HEAD_LST (TAG=25) Document Count: {tag25Count}");
            Console.WriteLine("-------------------------------------------------------------------------");

            List<DtlRow> origTag25 = SnapshotDtl(25);
            Console.WriteLine($"Original DEED_DTL (TAG=25) Row Count: {origTag25.Count}");
            Console.WriteLine("-------------------------------------------------------------------------");

            CL_HESABDARI_AUTO_BAZ.ClearLookupCaches();
            CL_HESABDARI_AUTO_BAZ.LookupCacheEnabled = true;

            var sw25 = Stopwatch.StartNew();
            CL_HESABDARI_AUTO_BAZ.gensanadbargashfroosh2(1, 9999999999, false);
            sw25.Stop();

            Console.WriteLine($"gensanadbargashfroosh2 Time: {sw25.Elapsed.TotalSeconds:F3} s ({sw25.ElapsedMilliseconds} ms)");

            List<DtlRow> newTag25 = SnapshotDtl(25);
            Console.WriteLine("\n[AUDIT: TAG=25 (gensanadbargashfroosh2)]");
            CompareRows(origTag25, newTag25, 25);

            Console.WriteLine("=========================================================================");
        }

        private static List<DtlRow> SnapshotDtl(double tag)
        {
            using (var conn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT N_S, HES_K, HES_M, HES_T, HES, SHARH, BED, BES, NUMBER, TAG FROM dbo.DEED_DTL WHERE TAG = {tag} ORDER BY NUMBER, N_S, HES_K, HES_M, HES_T, BED DESC, BES DESC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        var list = new List<DtlRow>();
                        while (reader.Read())
                        {
                            list.Add(new DtlRow
                            {
                                N_S = reader.GetDouble(0),
                                HES_K = reader.GetInt32(1),
                                HES_M = reader.GetInt32(2),
                                HES_T = reader.GetInt32(3),
                                HES = reader.GetString(4),
                                SHARH = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                BED = reader.GetDouble(6),
                                BES = reader.GetDouble(7),
                                NUMBER = reader.GetDouble(8),
                                TAG = reader.GetDouble(9)
                            });
                        }
                        return list;
                    }
                }
            }
        }

        private static void CompareRows(List<DtlRow> orig, List<DtlRow> gen, double tag)
        {
            int mismatchCount = 0;
            int exactMatches = 0;

            var origByNum = orig.GroupBy(x => x.NUMBER).ToDictionary(g => g.Key, g => g.ToList());
            var genByNum = gen.GroupBy(x => x.NUMBER).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in origByNum)
            {
                double docNum = kvp.Key;
                var origDocRows = kvp.Value;

                if (!genByNum.TryGetValue(docNum, out var genDocRows))
                {
                    Console.WriteLine($"❌ MISMATCH: Document #{docNum} missing in generated rows!");
                    mismatchCount++;
                    continue;
                }

                if (origDocRows.Count != genDocRows.Count)
                {
                    Console.WriteLine($"❌ MISMATCH: Document #{docNum} row count differs! Orig={origDocRows.Count}, Gen={genDocRows.Count}");
                    Console.WriteLine("   Original Rows:");
                    foreach (var r in origDocRows) Console.WriteLine($"     HES={r.HES}, BED={r.BED:N0}, BES={r.BES:N0}, SHARH='{r.SHARH}'");
                    Console.WriteLine("   Generated Rows:");
                    foreach (var r in genDocRows) Console.WriteLine($"     HES={r.HES}, BED={r.BED:N0}, BES={r.BES:N0}, SHARH='{r.SHARH}'");
                    mismatchCount++;
                    continue;
                }

                for (int i = 0; i < origDocRows.Count; i++)
                {
                    var o = origDocRows[i];
                    var n = genDocRows[i];

                    bool rowMatch = (o.HES_K == n.HES_K) &&
                                     (o.HES_M == n.HES_M) &&
                                     (o.HES_T == n.HES_T) &&
                                     (o.HES == n.HES) &&
                                     (Math.Abs(o.BED - n.BED) < 0.001) &&
                                     (Math.Abs(o.BES - n.BES) < 0.001) &&
                                     (o.SHARH == n.SHARH);

                    if (!rowMatch)
                    {
                        mismatchCount++;
                        Console.WriteLine($"❌ MISMATCH at Doc #{docNum}, Row #{i + 1}:");
                        Console.WriteLine($"   ORIG: HES={o.HES}, BED={o.BED:N0}, BES={o.BES:N0}, SHARH='{o.SHARH}'");
                        Console.WriteLine($"   GEN : HES={n.HES}, BED={n.BED:N0}, BES={n.BES:N0}, SHARH='{n.SHARH}'");
                    }
                    else
                    {
                        exactMatches++;
                    }
                }
            }

            Console.WriteLine($"Total Comparisons: {orig.Count:N0}");
            Console.WriteLine($"Exact Matches    : {exactMatches:N0}");
            Console.WriteLine($"Mismatches       : {mismatchCount:N0}");

            if (mismatchCount == 0 && exactMatches == orig.Count)
            {
                Console.WriteLine($"🎉 100% PERFECT MATCH FOR TAG={tag}! ALL {orig.Count:N0} ROWS ARE IDENTICAL! 🎉");
            }
        }
    }
}




