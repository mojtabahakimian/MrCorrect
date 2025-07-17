using Prg_Proccessy.SQLMODELS;
using Prg_UI.Functions;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;

namespace Prg_UI.Rpts
{
    public partial class WinReport : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WinReport()
        {
            InitializeComponent();
            Owner = PublicVRB.BUDG_3;
        }
        public string TheLic { get; set; } = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string MAHLIST; int XXD; int XXM; int XXS; int MH = 0; int mah = 0; int SAL; string SH; int XXI = 0; int i; int TEDAD; string rptname; double NAGHDINEGI;

            //
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.BUGETRP.mrt");

            //var fntpath = Assembly.GetEntryAssembly().GetManifestResourceStream("WpfApp5.FNT.BTITRBD.TTF");
            //Stream fontStream = GetType().Assembly.GetManifestResourceStream("WpfApp5.FNT.BTITRBD.TTF");
            //byte[] fontData = new byte[fontStream.Length];
            //IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            //System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            //uint dummy = 0;
            //fonts.AddMemoryFont(fontPtr, (System.IntPtr)pFontData, fontdata.Length);
            //AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.MyFontName.Length, IntPtr.Zero, ref dummy);
            //System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            //myFont = new Font(fonts.Families[0], 16.0F);

            //StiFontCollection.AddFontFile(@"C:\test\BTitrBd.ttf");

            //StiFontCollection.AddFontFile(fntpath.ToString());
            //report.CalculationMode = StiCalculationMode.Interpretation;
            //report.Load(@"C:\prg\Budget\WpfApp5\Rpt\BUGETRP.mrt");
            //report.Load(@"Rpt\BUGETRP.mrt");

            report.Load(pathreport);
            //report.Dictionary.Databases.Clear();
            //report.DataSources.Clear();
            //((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = "Data Source=serverx;Initial Catalog=DENAF1400;Integrated Security=True;User ID=;Password=";
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;
            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").ToList();
            //var quer_RST = dbms.Database.SqlQuery<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0 FROM BUGET_MAIN WHERE BGID = 1").ToList();
            foreach (var item in quer_RST)
            {
                XXD = Convert.ToInt32(CL_HESABDARI.UDay(item.BGCDATE.ToString()));
                XXM = Convert.ToInt32(CL_HESABDARI.UMonth(item.BGCDATE.ToString()));
                XXS = Convert.ToInt32(CL_HESABDARI.UYear(item.BGCDATE.ToString()));
                mah = Convert.ToInt32(item.BGMAH);
                SAL = Convert.ToInt32(item.BGSAL);
                XXI = (SAL - XXS) * 12 + mah - XXM;
                MH = XXM;
            }
            report["BGID_PARAM"] = PublicVRB.GenetalBGID;

            (report.GetComponentByName("BGMAH1") as StiText).Text = PublicVRB.MAHNAME(mah.ToString());
            string BDate = quer_RST.Select(x => x.BGCDATE).First().ToString();
            string sl = BDate.Substring(0, 4);
            string mh = BDate.Substring(4, 2);
            string rz = BDate.Substring(6, 2);
            (report.GetComponentByName("BGDATE") as StiText).Text = $"{sl}/{mh}/{rz}";

            //report["M1"] = "اسفند";
            for (i = 0; i <= XXI; i++)
            {
                string stiname1 = "M" + (i + 1) + "";
                (report.GetComponentByName(stiname1) as StiText).Text = PublicVRB.MAHNAME(MH.ToString());
                if (MH == 12)
                {
                    MH = 0;
                    SH = " " + PublicVRB.BGSAL;
                }
                MH = MH + 1;
                //report["BGMBCHEK" + i].Visible = true;
                //report["BGFOCN" + i + 1 + "MON"].Visible = true;
                foreach (dynamic itt in quer_RST)
                {
                    var BGMBCHEK_JM = itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToString(XXI - i)).GetValue(itt);
                    var BGMBCHEK_Wich = itt.GetType().GetProperty("BGMBCHEK" + Convert.ToString(XXI - i)).GetValue(itt);
                    //report["BGMBCHEKJM" + i] = Convert.ToDouble(BGMBCHEK_Wich);
                    (report.GetComponentByName("BGMBCHEKJM" + i) as StiText).Text = BGMBCHEK_JM.ToString();
                    (report.GetComponentByName("BGMBCHEK" + i) as StiText).Text = BGMBCHEK_Wich.ToString();
                }
                //report["BGMBCHEKJM" + i].ControlSource = "BGMBCHEKJM" + System.Convert.ToString(XXI - i);
                //report["BGMBCHEKJM" + i].Visible = true;
            }
            //(report.GetComponentByName("BGMBCHEKJM12") as StiText).Text = "Majid" ;
            //report.Dictionary.Variables["Variable1"].Value = "2000";
            //report.Compile();
            report.Render();
            rptviewer1.Report = report;
        }
    }
}
