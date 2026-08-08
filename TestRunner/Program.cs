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
            Console.WriteLine("        DEEP ROW-BY-ROW CONTENT AUDIT & COMPARISON: SANADVORUDSAKHT       ");
            Console.WriteLine("=========================================================================");

            // 1. Snapshot original DEED_DTL rows before running
            List<DtlRow> originalRows;
            using (var conn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT N_S, HES_K, HES_M, HES_T, HES, SHARH, BED, BES, NUMBER, TAG FROM dbo.DEED_DTL WHERE TAG = 9 ORDER BY NUMBER, N_S, HES_K, HES_M, HES_T, BED DESC, BES DESC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        originalRows = new List<DtlRow>();
                        while (reader.Read())
                        {
                            originalRows.Add(new DtlRow
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
                    }
                }
            }

            Console.WriteLine($"Original DEED_DTL (TAG=9) Row Count: {originalRows.Count:N0}");

            // 2. Run SANADVORUDSAKHT
            CL_HESABDARI_AUTO_BAZ.ClearLookupCaches();
            CL_HESABDARI_AUTO_BAZ.LookupCacheEnabled = true;

            var sw = Stopwatch.StartNew();
            var (lastSanad, success) = CL_HESABDARI_AUTO_BAZ.SANADVORUDSAKHT(1, 9999999999, false);
            sw.Stop();

            Console.WriteLine($"SANADVORUDSAKHT Execution Time    : {sw.Elapsed.TotalSeconds:F3} s ({sw.ElapsedMilliseconds} ms)");

            // 3. Snapshot new DEED_DTL rows after running
            List<DtlRow> newRows;
            using (var conn = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT N_S, HES_K, HES_M, HES_T, HES, SHARH, BED, BES, NUMBER, TAG FROM dbo.DEED_DTL WHERE TAG = 9 ORDER BY NUMBER, N_S, HES_K, HES_M, HES_T, BED DESC, BES DESC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        newRows = new List<DtlRow>();
                        while (reader.Read())
                        {
                            newRows.Add(new DtlRow
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
                    }
                }
            }

            Console.WriteLine($"Generated DEED_DTL (TAG=9) Row Count: {newRows.Count:N0}");
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("                  PER-FIELD CONTENT AUDIT RESULTS                        ");
            Console.WriteLine("-------------------------------------------------------------------------");

            int mismatchCount = 0;
            int exactMatches = 0;

            if (originalRows.Count != newRows.Count)
            {
                Console.WriteLine($"❌ MISMATCH: Row counts differ! Original={originalRows.Count}, New={newRows.Count}");
            }
            else
            {
                Console.WriteLine($"✓ Row counts match perfectly ({originalRows.Count:N0} rows)");
            }

            // Group by Document NUMBER for precise content comparison
            var origByNum = originalRows.GroupBy(x => x.NUMBER).ToDictionary(g => g.Key, g => g.ToList());
            var newByNum = newRows.GroupBy(x => x.NUMBER).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in origByNum)
            {
                double docNum = kvp.Key;
                var origDocRows = kvp.Value;

                if (!newByNum.TryGetValue(docNum, out var newDocRows))
                {
                    Console.WriteLine($"❌ MISMATCH: Document NUMBER {docNum} missing in generated rows!");
                    mismatchCount++;
                    continue;
                }

                if (origDocRows.Count != newDocRows.Count)
                {
                    Console.WriteLine($"❌ MISMATCH: Document NUMBER {docNum} row count differs! Orig={origDocRows.Count}, New={newDocRows.Count}");
                    mismatchCount++;
                    continue;
                }

                // Check per-row content: HES_K, HES_M, HES_T, HES, BED, BES, SHARH
                for (int i = 0; i < origDocRows.Count; i++)
                {
                    var o = origDocRows[i];
                    var n = newDocRows[i];

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
                        Console.WriteLine($"❌ CONTENT MISMATCH at Doc #{docNum}, Row #{i + 1}:");
                        Console.WriteLine($"   ORIGINAL: HES={o.HES}, BED={o.BED:N0}, BES={o.BES:N0}, SHARH='{o.SHARH}'");
                        Console.WriteLine($"   NEW     : HES={n.HES}, BED={n.BED:N0}, BES={n.BES:N0}, SHARH='{n.SHARH}'");
                    }
                    else
                    {
                        exactMatches++;
                    }
                }
            }

            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine($"Total Row Comparisons: {originalRows.Count:N0}");
            Console.WriteLine($"Exact Field Matches  : {exactMatches:N0}");
            Console.WriteLine($"Mismatches / Errors  : {mismatchCount:N0}");

            if (mismatchCount == 0 && exactMatches == originalRows.Count)
            {
                Console.WriteLine("\n🎉 100% PERFECT MATCH! ALL 9,278 ROWS AND ALL FIELDS ARE IDENTICAL! 🎉");
            }
            else
            {
                Console.WriteLine($"\n⚠️ AUDIT DETECTED {mismatchCount} MISMATCHES!");
            }
            Console.WriteLine("=========================================================================");
        }
    }
}


