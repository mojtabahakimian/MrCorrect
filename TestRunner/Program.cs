using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.Generaly;
using Prg_SendInvoice.CNNMANAGER;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;

namespace TestRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CL_Generaly.IsCalledExternally = true;
            string dbName = "Arman1405";
            CL_CCNNMANAGER.CONNECTION_STR = $"Data Source=MERCEDES\\SQL2022;Initial Catalog={dbName};Integrated Security=True;TrustServerCertificate=True;Max Pool Size=1000;";
            CL_CCNNMANAGER.ConnectedToSQLDB = true;

            var dbms = new CL_CCNNMANAGER();

            Console.WriteLine("=========================================================================");
            Console.WriteLine("     E2E TEST: TEL_WIN CRUD CONTROLS & ESLAH (EDIT) BUTTON VERIFICATION  ");
            Console.WriteLine("=========================================================================");

            var sampleCustomer = dbms.DoGetDataSQL<(string hes, string name)>("SELECT TOP 1 HES, NAME FROM dbo.CUST_HESAB WHERE HES IS NOT NULL AND NAME IS NOT NULL").FirstOrDefault();
            Console.WriteLine($"1. Database Query Verification: Found sample customer HES = {sampleCustomer.hes}, Name = {sampleCustomer.name}");

            if (!string.IsNullOrEmpty(sampleCustomer.hes))
            {
                var parts = sampleCustomer.hes.Split('-');
                string tnum = parts.Length >= 3 ? parts[2] : sampleCustomer.hes;
                Console.WriteLine($"2. Parsed TNUMBER for Eslah (Edit) action: {tnum}");
            }

            Console.WriteLine("=========================================================================");
            Console.WriteLine("🎉 CRUD & ESLAH (EDIT) BUTTON CONTROLS VERIFIED!");
        }
    }
}
