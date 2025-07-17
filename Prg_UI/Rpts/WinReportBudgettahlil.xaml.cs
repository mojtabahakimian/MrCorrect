using Prg_Proccessy.SQLMODELS;
using Prg_UI.Functions;
using Stimulsoft.Base.Drawing;
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
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.FUNCTIONS;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Prg_Proccessy.Generaly;

namespace Prg_UI.Rpts
{
    public partial class WinReportBudgettahlil : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public class RST_OPN
        {
            public double? MAB { get; set; }
        }
        public WinReportBudgettahlil()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string MAHLIST; int XXD = 0; int XXM; int XXS = 0; int MH = 0; int mah = 0; int SAL = 0; string SH; int XXI = 0; int i; int TEDAD; string rptname; double NAGHDINEGI;
            long DT1 = 0;
            long DT2 = 0;
            long dt11 = 0;
            long dt22 = 0;
            double JAMBG = 0;
            double NAGHD = 0;
            
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.BUGETRP_tahlil.mrt");
            report.Load(pathreport);
            ((StiSqlDatabase)(report.Dictionary.Databases["MS SQL"])).ConnectionString = CL_CCNNMANAGER.CONNECTION_STR;
            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + BUGET_MAIN_list2.BGID_tahlil_list + "").ToList();
            report["BGID_PARAM"] = PublicVRB.GenetalBGID;
            (report.GetComponentByName("BGMAH1") as StiText).Text = PublicVRB.MAHNAME(quer_RST.Select(x => x.BGMAH).FirstOrDefault().ToString());
            string BDate = quer_RST.Select(x => x.BGCDATE).First().ToString();
            string sl = BDate.Substring(0, 4);
            string mh = BDate.Substring(4, 2);
            string rz = BDate.Substring(6, 2);
            (report.GetComponentByName("BGDATE") as StiText).Text = $"{sl}/{mh}/{rz}";


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

            ////***********************************************************************************************************************************
            ////report["M1"] = "اسفند";
            //for (i = 0; i <= XXI; i++)
            //{
            //    string stiname1 = "M" + (i + 1) + "";
            //    (report.GetComponentByName(stiname1) as StiText).Text = PublicVRB.MAHNAME(MH.ToString());
            //    if (MH == 12)
            //    {
            //        MH = 0;
            //        SH = " " + PublicVRB.BGSAL;
            //    }
            //    MH = MH + 1;
            //    //report["BGMBCHEK" + i].Visible = true;
            //    //report["BGFOCN" + i + 1 + "MON"].Visible = true;
            //    foreach (dynamic itt in quer_RST)
            //    {
            //        var BGMBCHEK_JM = itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToString(XXI - i)).GetValue(itt);
            //        var BGMBCHEK_Wich = itt.GetType().GetProperty("BGMBCHEK" + Convert.ToString(XXI - i)).GetValue(itt);
            //        //report["BGMBCHEKJM" + i] = Convert.ToDouble(BGMBCHEK_Wich);
            //        (report.GetComponentByName("BGMBCHEKJM" + i) as StiText).Text = BGMBCHEK_JM.ToString();
            //        (report.GetComponentByName("BGMBCHEK" + i) as StiText).Text = BGMBCHEK_Wich.ToString();
            //    }
            //    //report["BGMBCHEKJM" + i].ControlSource = "BGMBCHEKJM" + System.Convert.ToString(XXI - i);
            //    //report["BGMBCHEKJM" + i].Visible = true;
            //}
            ////***********************************************************************************************************************************
            //(report.GetComponentByName("BGMBCHEKJM12") as StiText).Text = "Majid" ;
            //report.Dictionary.Variables["Variable1"].Value = "2000";
            //---------------------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------------------------------------------
            for (i = 0; i <= XXI; i++)
            {
                string stiname1 = "M" + (i + 1) + "";
                (report.GetComponentByName(stiname1) as StiText).Text = PublicVRB.MAHNAME(MH.ToString());
                if (MH == 12)
                {
                    MH = 0;
                    SH = " " + PublicVRB.BGSAL;
                }
                DT1 = System.Convert.ToInt64(System.Convert.ToDouble(System.Convert.ToInt32(XXS * 10000) + System.Convert.ToDouble(MH * 100)) + XXD);
                DT2 = System.Convert.ToInt64(System.Convert.ToDouble(System.Convert.ToInt32(XXS * 10000) + System.Convert.ToDouble(MH * 100)) + 31);
                dt11 = System.Convert.ToInt64(System.Convert.ToDouble(SAL * 10000) + System.Convert.ToDouble(mah * 100));
                dt22 = System.Convert.ToInt64(System.Convert.ToDouble(System.Convert.ToInt32(SAL * 10000) + System.Convert.ToDouble(mah * 100)) + 31);
                MH++;

                var GFRO = CL_HESABDARI.GETFROOSH(DT1, DT2);


                (report.GetComponentByName("BGFOCN" + i + "MONR") as StiText).Text = GFRO.ToString();
                //this["BGFOCN" + System.Convert.ToString(i) + "MONR"].ControlSource = "=" + GFRO;
                //this["BGFOCN" + System.Convert.ToString(i) + "MONR"].Visible = true;

                if (i == 0)
                {
                    foreach (dynamic item in quer_RST)
                    {
                        (report.GetComponentByName("BGFOCN" + i + "MOND") as StiText).Text = System.Convert.ToString(Math.Round(GFRO / ((item.BGFOCXDAY == 0) ? 1 : (item.BGFOCXDAY)) * 100, 1)).ToString();
                        string test1 = System.Convert.ToString(Math.Round(GFRO / ((item.BGFOCXDAY == 0) ? 1 : (item.BGFOCXDAY)) * 100, 1)).ToString();
                    }
                    //this["BGFOCN" + System.Convert.ToString(i) + "MOND"].ControlSource = "=" + System.Convert.ToString(ROUND(GFRO / ((rst.Fields("BGFOCXDAY") == 0) ? 1 : (rst.Fields("BGFOCXDAY"))) * 100, 1));
                }
                else
                {
                    foreach (dynamic item in quer_RST)
                    {
                        (report.GetComponentByName("BGFOCN" + i + "MOND") as StiText).Text = Math.Round(GFRO / item.GetType().GetProperty("BGFOCN" + i + "MON").GetValue(item) * 100, 1).ToString();
                        string test2 = Math.Round(GFRO / item.GetType().GetProperty("BGFOCN" + i + "MON").GetValue(item) * 100, 1).ToString();
                    }
                    //this["BGFOCN" + System.Convert.ToString(i) + "MOND"].ControlSource = "=" + ROUND(GFRO / rst.Fields("BGFOCN" + System.Convert.ToString(i) + "MON") * 100, 1);
                }
                foreach (dynamic item2 in quer_RST)
                {
                    (report.GetComponentByName("BGPCH" + i) as StiText).Text = Math.Round(item2.GetType().GetProperty("BGPCH" + Convert.ToString(XXI - i)).GetValue(item2), 1).ToString();
                    string test33 = item2.GetType().GetProperty("BGPCH" + Convert.ToString(XXI - i)).GetValue(item2).ToString();
                    string test333 = Math.Round(item2.GetType().GetProperty("BGPCH" + Convert.ToString(XXI - i)).GetValue(item2), 1).ToString();
                }
                //this["BGFOCN" + System.Convert.ToString(i) + "MOND"].Visible = true;
                //(report.GetComponentByName("BGFOCN" + i + "MOND") as StiText).Text;

                //this["BGPCH" + System.Convert.ToString(i)].ControlSource = "BGPCH" + System.Convert.ToString(XXI - i);
                //this["BGPCH" + System.Convert.ToString(i)].Visible = true;
                //@Forms["BUN"].Form.Refresh;
                JAMBG = JAMBG + CL_HESABDARI.GETCHEKin(dt11, dt22, DT1, DT2);

                (report.GetComponentByName("BGMBCHEKR" + i) as StiText).Text = CL_HESABDARI.GETCHEKin(dt11, dt22, DT1, DT2).ToString();
                string test3 = CL_HESABDARI.GETCHEKin(dt11, dt22, DT1, DT2).ToString();
                //this["BGMBCHEKR" + System.Convert.ToString(i)].ControlSource = "=" + PublicVRB.GETCHEKin(dt11, dt22, DT1, DT2);
                //this["BGMBCHEKR" + System.Convert.ToString(i)].Visible = true;
                //@Forms["BUN"].Form.Refresh;

                (report.GetComponentByName("BGMBCHEKJMR" + i) as StiText).Text = CL_HESABDARI.GETCHEKin(1, 99999999, DT1, DT2).ToString();
                string test4 = CL_HESABDARI.GETCHEKin(1, 99999999, DT1, DT2).ToString();
                //this["BGMBCHEKJMR" + System.Convert.ToString(i)].ControlSource = "=" + PublicVRB.GETCHEKin(1, 99999999, DT1, DT2);
                //this["BGMBCHEKJMR" + System.Convert.ToString(i)].Visible = true;

                //LAST POINT
                (report.GetComponentByName("BGPCHD" + i) as StiText).Text = System.Convert.ToString(Math.Round(CL_HESABDARI.GETCHEKin(dt11, dt22, DT1, DT2) / (GFRO == 0 ? 1 : GFRO) * 100, 1)).ToString();
                string test5 = System.Convert.ToString(Math.Round(CL_HESABDARI.GETCHEKin(dt11, dt22, DT1, DT2) / (GFRO == 0 ? 1 : GFRO) * 100, 1)).ToString();
                //this["BGPCHD" + System.Convert.ToString(i)].ControlSource = "=" +
                //this["BGPCHD" + System.Convert.ToString(i)].Visible = true;

                foreach (dynamic itt in quer_RST)
                {
                    if (i == 0)
                    {
                        (report.GetComponentByName("BGPCHM" + i) as StiText).Text = System.Convert.ToString(Math.Round((double)itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToInt64(XXI - i)).GetValue(itt) / (double)((itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt) == 0) ? 1 : (itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt))) * 100, 1)).ToString();
                        string test6 = System.Convert.ToString(Math.Round((double)itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToInt64(XXI - i)).GetValue(itt) / (double)((itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt) == 0) ? 1 : (itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt))) * 100, 1)).ToString();
                        string test56 = System.Convert.ToString(Math.Round((double)itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToInt64(XXI - i)).GetValue(itt) / (double)((itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt) == 0) ? 1 : (itt.GetType().GetProperty("BGFOCXDAY").GetValue(itt))) * 100, 1)).ToString();
                    }
                    else
                    {
                        (report.GetComponentByName("BGPCHM" + i) as StiText).Text = System.Convert.ToString(Math.Round((double)itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToInt64(XXI - i)).GetValue(itt) / (double)((itt.GetType().GetProperty("BGFOCN" + System.Convert.ToString(i) + "MON").GetValue(itt) == 0) ? 1 : (itt.GetType().GetProperty("BGFOCN" + System.Convert.ToString(i) + "MON").GetValue(itt))) * 100, 1)).ToString();
                        string test7 = System.Convert.ToString(Math.Round((double)itt.GetType().GetProperty("BGMBCHEKJM" + Convert.ToInt64(XXI - i)).GetValue(itt) / (double)((itt.GetType().GetProperty("BGFOCN" + System.Convert.ToString(i) + "MON").GetValue(itt) == 0) ? 1 : (itt.GetType().GetProperty("BGFOCN" + System.Convert.ToString(i) + "MON").GetValue(itt))) * 100, 1)).ToString();
                    }
                }
                //this["BGPCHM" + System.Convert.ToString(i)].Visible = true;
                (report.GetComponentByName("BGPCHR" + i) as StiText).Text = System.Convert.ToString(Math.Round(CL_HESABDARI.GETCHEKin(1, 99999999, DT1, DT2) / (GFRO == 0 ? 1 : GFRO) * 100, 1)).ToString();
                //this["BGPCHR" + System.Convert.ToString(i)].Visible = true;
                string test8 = System.Convert.ToString(Math.Round(CL_HESABDARI.GETCHEKin(1, 99999999, DT1, DT2) / (GFRO == 0 ? 1 : GFRO) * 100, 1)).ToString();
                XXD = 0;
                //this["BGMBCHEK" + System.Convert.ToString(i)].Visible = true;
                //this["BGMBCHEKJM" + System.Convert.ToString(i)].Visible = true;
                //this["BGFOCN" + System.Convert.ToString(i + 1) + "MON"].Visible = true;
                foreach (var item in quer_RST)
                {
                    (report.GetComponentByName("BGMBCHEKJM" + i) as StiText).Text = item.GetType().GetProperty("BGMBCHEKJM" + System.Convert.ToString(XXI - i)).GetValue(item).ToString();
                    string test9 = item.GetType().GetProperty("BGMBCHEKJM" + System.Convert.ToString(XXI - i)).GetValue(item).ToString();
                    (report.GetComponentByName("BGMBCHEK" + i) as StiText).Text = item.GetType().GetProperty("BGMBCHEK" + System.Convert.ToString(XXI - i)).GetValue(item).ToString();
                    string test10 = item.GetType().GetProperty("BGMBCHEK" + System.Convert.ToString(XXI - i)).GetValue(item).ToString();
                }
                //this["BGMBCHEKJM" + System.Convert.ToString(i)].ControlSource = "BGMBCHEKJM" + System.Convert.ToString(XXI - i);
                //this["BGMBCHEK" + System.Convert.ToString(i)].ControlSource = "BGMBCHEK" + System.Convert.ToString(XXI - i);
            }
            foreach (var itt3 in quer_RST)
            {
                NAGHD = Convert.ToDouble(itt3.BGBUGETMON);
            }
            var rstOpen = dbms.DoGetDataSQL<RST_OPN>("SELECT     SUM(DEED_DTL.BED) AS MAB FROM DEED_DTL INNER JOIN dbo.DEED_HED ON DEED_DTL.N_S = DEED_HED.N_S WHERE     (DEED_HED.DATE_S BETWEEN " + System.Convert.ToString(dt11) + " AND " + System.Convert.ToString(dt22) + ")  AND (DEED_DTL.HES = N'110-9-9')").FirstOrDefault();
            foreach (var item in quer_RST)
            {
                if (rstOpen.MAB > 0 && rstOpen.MAB != null)
                {
                    (report.GetComponentByName("BGMBCASHD") as StiText).Text = rstOpen.MAB.ToString();
                    //BGMBCASHD.ControlSource = "=" + rst.Fields("MAB");
                    JAMBG = JAMBG + Convert.ToDouble(rstOpen.MAB);
                    (report.GetComponentByName("BGPCASH0") as StiText).Text = Convert.ToString(Math.Round((double)(rstOpen.MAB / ((CL_HESABDARI.GETFROOSH(dt11, dt22) == 0) ? 1 : CL_HESABDARI.GETFROOSH(dt11, dt22)) * 100), 1)).ToString();
                    string test11 = Convert.ToString(Math.Round((double)(rstOpen.MAB / ((CL_HESABDARI.GETFROOSH(dt11, dt22) == 0) ? 1 : CL_HESABDARI.GETFROOSH(dt11, dt22)) * 100), 1)).ToString();
                    //@Forms["BUN"].Form.Refresh;
                }
            }
            (report.GetComponentByName("DDD") as StiText).Text = Math.Round(Convert.ToDouble(JAMBG / (NAGHD == 0 ? 1 : NAGHD) * 100)).ToString();

            int DDD_Colory = (int)Math.Round(Convert.ToDouble(JAMBG / (NAGHD == 0 ? 1 : NAGHD) * 100));

            (report.GetComponentByName("BGBUGETMONR") as StiText).Text = Convert.ToString(JAMBG);

            (report.GetComponentByName("BGMBCHEKMON") as StiText).Text = Convert.ToString(quer_RST.Select(x => x.BGMBCHEKMON).FirstOrDefault());

            (report.GetComponentByName("BGBUGETMON") as StiText).Text = Convert.ToString(quer_RST.Select(x => x.BGBUGETMON).FirstOrDefault());


            if (DDD_Colory < 80)
            {
                (report.GetComponentByName("DDD") as StiText).Brush = new StiSolidBrush(System.Drawing.Color.FromArgb(255, 221, 0, 0));
            }

            //report.Compile();
            report.Render();
            rptviewer1.Report = report;
        }
    }
}
