using Prg_UI.Functions;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static Prg_Proccessy.SQLMODELS.CTABLES;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using static Prg_UI.Functions.CL_LMethods;
using Wins.WinMenus.KHARID_FORUSH;
using Prg_Proccessy.MODELS;

namespace Prg_UI.Rpts
{
    public partial class Win_INVOICE_PISHFROOSH2 : Window
    {

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        string TAG_Pas = "";
        string NUMBER_Pas = "";
        string WHOE_CALL_ME = "";
        public class jst_KH
        {
            public double? KH { get; set; }
        }
        public class rst_weight
        {
            public double? Weight { get; set; }
        }
        public Visual THE_WIN { get; set; }

        /// <summary>
        /// Input Parameter for Report SQL Query by where tag and number
        /// </summary>
        /// <param name="TAG"></param>
        /// <param name="NUMBER"></param>
        public Win_INVOICE_PISHFROOSH2(string NUMBER, Visual i_AM_PISHFACTOR)
        {
            NUMBER_Pas = NUMBER;
            THE_WIN = i_AM_PISHFACTOR;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();


            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.INVOICE_PISHFROOSH2.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;
            //Starting }

            //Body
            //{

            //report["TAG_PARAM"] = TAG_Pas;
            report["NUMBER_PARAM"] = NUMBER_Pas;

            //report["@TAG_PARAM"] = TAG_Pas;
            //report["@NUMBER_PARAM"] = NUMBER_Pas;
            //var jst = new ADODB.Recordset();
            double JAMF, KH, BAA, TF;
            JAMF = 0d;
            TF = 0d;
            BAA = 0d;
            KH = 0d;
            var jst = dbms.DoGetDataSQL<jst_JAMTAF>("SELECT     SUM(MABL_K) AS JAMF, SUM(N_MOIN) AS TF, SUM(IMBAA) AS BAA FROM dbo.INVO_LST WHERE (NUMBER = " + NUMBER_Pas + ") And (TAG = 20)").ToList();
            if (jst.Count > 0 && !ReferenceEquals(jst.FirstOrDefault(), null))
            {
                JAMF = Convert.ToDouble(jst.Select(x => x.JAMF).FirstOrDefault());
                TF = Convert.ToDouble(jst.Select(x => x.TF).FirstOrDefault());
                BAA = Convert.ToDouble(jst.Select(x => x.BAA).FirstOrDefault());
            };

            var jst2 = dbms.DoGetDataSQL<jst_KH>("SELECT     SUM(MABL_HAZ) AS KH FROM dbo.HEAD_LST WHERE (NUMBER = " + NUMBER_Pas + ") And (TAG = 20)").ToList();

            if (jst2.Count > 0 && !ReferenceEquals(jst2.FirstOrDefault(), null))
            {
                KH = Convert.ToDouble(jst2.Select(x => x.KH).FirstOrDefault());
            }
            //this.MABK.CAPTION = Strings.Format(JAMF, "#,###");
            (report.GetComponentByName("MABK") as StiText).Text = JAMF.ToString();
            (report.GetComponentByName("TF") as StiText).Text = TF.ToString();
            //this.TF.CAPTION = Strings.Format(TF, "#,###");
            //this.KH.CAPTION = Strings.Format(KH, "#,###");
            (report.GetComponentByName("KH") as StiText).Text = KH.ToString();

            //this.MABAA.CAPTION = Strings.Format(BAA, "#,###");
            (report.GetComponentByName("MABAA") as StiText).Text = BAA.ToString();
            //this.PAY.CAPTION = Strings.Format(JAMF + KH + BAA - TF, "#,###");
            (report.GetComponentByName("PAY") as StiText).Text = (JAMF + KH + BAA - TF).ToString();
            //this.Label112.CAPTION = "مبلغ به حروف: " + ALPHANUM[JAMF + KH + BAA - TF] + " " + "ريال";
            //var rst = new ADODB.Recordset();
            var rst = dbms.DoGetDataSQL<rst_weight>("SELECT SUM(dbo.STUF_DEF.VAZN * dbo.INVO_LST.MEGHk) AS Weight FROM   dbo.INVO_LST INNER JOIN   dbo.STUF_DEF ON dbo.INVO_LST.CODE = dbo.STUF_DEF.CODE WHERE     (dbo.INVO_LST.TAG = 20) AND (dbo.INVO_LST.NUMBER = " + NUMBER_Pas + ")").FirstOrDefault();
            if (!ReferenceEquals(rst, null))
            {
                if (!ReferenceEquals(rst.Weight, null))
                {
                    //this.VAZN.CAPTION = "وزن كل به كيلو : " + ROUND(rst.Fields("Weight"));
                    //Text34 the label
                    if (CL_LMethods.IsNumeric(rst.Weight?.ToString()) && rst.Weight > 0)
                    {
                        (report.GetComponentByName("vazn") as StiText).Enabled = true;
                        (report.GetComponentByName("vazn") as StiText).Text = "وزن : " + Convert.ToString(Math.Round(Convert.ToDouble(rst.Weight)));
                    }
                    else
                    {
                        (report.GetComponentByName("vazn") as StiText).Enabled = false;
                    }
                }
            }

            //DoCmd.Maximize();
            //Forms["baseknow"]["hhwin"] = this.hwnd;
            double addad = JAMF + KH + BAA - TF;

            //var variables = new Stimulsoft.Report.Dictionary.StiVariable();
            //variables.Name = "Address";
            report.Dictionary.Variables.Add("Variable1", Convert.ToInt64(addad));
            //report["@Variable1"] = Convert.ToInt64(addad);
            //report.Dictionary.Variables["Variable1"].Value = addad.ToString();
            //report.Dictionary.Databases["MS SQL"].Parameters["ParameterName"].Value = "";
            //}

            (report.GetComponentByName("WIDTH_D") as StiText).Text = Baseknow.WIDTH_D; // نام شرکت
            if (THE_WIN is HEAD_LST_PISHFROOSH2 PISHWIN)
            {
                (report.GetComponentByName("SHARAYET") as StiText).Text = PISHWIN?.SHARAYET.Text;
            }

            //Ending {
            //report.Compile();

            (THE_WIN as HEAD_LST_PISHFROOSH2).ShowEmzaha(report);

            report.Render();
            rptviewer1.Report = report;

            //ProcLoader.Stop(Prc);
            ProcLoader.Stop(Prc);
        }
    }
}
