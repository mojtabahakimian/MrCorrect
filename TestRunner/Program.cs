using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AUTO_BAZ;
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
            string dbName = args.Length > 0 ? args[0] : "YAZDSEPAR1405";
            CL_CCNNMANAGER.CONNECTION_STR = $"Data Source=MERCEDES\\SQL2022;Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            CL_CCNNMANAGER.ConnectedToSQLDB = true;

            Baseknow.GetInitTheApp();
            var dbms = new CL_CCNNMANAGER();

            Console.WriteLine("=========================================================================");
            Console.WriteLine("     BENCHMARK & AUDIT TEST: GENSANADKHAREED (سند خرید)                  ");
            Console.WriteLine("=========================================================================");

            var tag12Count = dbms.DoGetDataSQL<int>("SELECT COUNT(*) FROM dbo.HEAD_LST WHERE TAG = 12").FirstOrDefault();
            Console.WriteLine($"HEAD_LST (TAG=12) Document Count: {tag12Count}");

            List<DtlRow> origTag12 = SnapshotDtl(12);
            Console.WriteLine($"Original DEED_DTL (TAG=12) Row Count: {origTag12.Count}");

            var sw = Stopwatch.StartNew();
            CL_HESABDARI_AUTO_BAZ.GENSANADKHAREED(1, 9999999999, false);
            sw.Stop();

            Console.WriteLine($"GENSANADKHAREED Execution Time: {sw.Elapsed.TotalSeconds:F3} s ({sw.ElapsedMilliseconds} ms)");

            List<DtlRow> newTag12 = SnapshotDtl(12);
            Console.WriteLine("\n[AUDIT: TAG=12 (GENSANADKHAREED)]");
            CompareRows(origTag12, newTag12, 12);
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
                                HES = reader.IsDBNull(4) ? "" : reader.GetString(4),
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
            Console.WriteLine($"Original Rows: {orig.Count} | Generated Rows: {gen.Count}");
            if (orig.Count != gen.Count)
            {
                Console.WriteLine($"⚠️ ROW COUNT MISMATCH! Orig: {orig.Count}, Gen: {gen.Count}");
            }

            int matchCount = 0;
            int diffCount = 0;
            int limit = Math.Min(orig.Count, gen.Count);

            for (int i = 0; i < limit; i++)
            {
                var o = orig[i];
                var g = gen[i];

                bool same = o.N_S == g.N_S &&
                            o.HES_K == g.HES_K &&
                            o.HES_M == g.HES_M &&
                            o.HES_T == g.HES_T &&
                            Math.Abs(o.BED - g.BED) < 0.01 &&
                            Math.Abs(o.BES - g.BES) < 0.01 &&
                            o.NUMBER == g.NUMBER;

                if (same)
                {
                    matchCount++;
                }
                else
                {
                    diffCount++;
                    if (diffCount <= 5)
                    {
                        Console.WriteLine($"Diff at index {i}: Orig[NS={o.N_S}, HK={o.HES_K}, HM={o.HES_M}, HT={o.HES_T}, BED={o.BED}, BES={o.BES}] vs Gen[NS={g.N_S}, HK={g.HES_K}, HM={g.HES_M}, HT={g.HES_T}, BED={g.BED}, BES={g.BES}]");
                    }
                }
            }

            if (diffCount == 0 && orig.Count == gen.Count)
            {
                Console.WriteLine("🎉 100% PERFECT MATCH! ALL GENERATED ROWS ARE IDENTICAL TO ORIGINAL!");
            }
            else
            {
                Console.WriteLine($"Matched: {matchCount} | Diffs: {diffCount}");
            }
        }
    }
}
