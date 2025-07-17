using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

namespace Prg_UI.Wins.WinMenus.SALARY
{
    public partial class ERSAL_SANAD : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        const string DSKWOR00 = @"\DSKWOR00.DBF";
        const string DSKKAR00 = @"\DSKKAR00.DBF";

        public static string THE_PATH_FILES { get; set; } = @"C:\correct";

        #region _DBF_
        public static DataTable DSKW_SQL_WORK { get; set; } = new DataTable();
        public static DataTable DSKK_SQL_KAR { get; set; } = new DataTable();

        public static DataTable DSWW_FDATA_WORK { get; set; } = new DataTable();
        public static DataTable DSKK_FDATA_KAR { get; set; } = new DataTable();

        public static string DSW_PATH { get; set; } = "";
        public static string DSK_PATH { get; set; } = "";
        #endregion
        public static bool IsIransystemformat { get; private set; } = true;

        #region _SQL_CONNECTION_
        public static SqlConnection SQL_CONN { get; set; }
        public static SqlCommand SQL_CMD { get; set; }
        #endregion

        #region FUNCTIONS
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
        #endregion

        public string OPEN_ARGS { get; set; } = "";
        public int? MODATE { get; private set; }
        public int MABMASH { get; private set; }
        public double MAB2 { get; private set; }
        public int MABJ { get; private set; }
        public bool NowIsReady { get; private set; }

        public ERSAL_SANAD(string _openarg = null)
        {
            OPEN_ARGS = _openarg;
            InitializeComponent();
            this.Owner = PublicVRB.WINBASE;
        }
        internal class QRE_SALARY0
        {
            public short? MON_ID { get; set; }
            public string MON { get; set; }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            MMO.ItemsSource = dbms.DoGetDataSQL<QRE_SALARY0>("SELECT MON.MON_ID, MON.MON FROM MON ORDER BY MON.MON_ID;").ToList();
            MMO.SelectedValuePath = "MON_ID";
            MMO.DisplayMemberPath = "MON";
        }
        private bool IsNull(object hTAF2)
        {
            if (hTAF2 is null)
            {
                return true;
            }
            if (!(hTAF2 is null))
            {
                return false;
            }
            return true;
        }
        private void Command13_Click(object sender, RoutedEventArgs e)
        {
            if (CL_LMethods.IsValidPath(TEXTBOXPATH.Text.Trim()) is true && MMO.SelectedIndex >= 0)
            {
                //OK
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "مقادیری مثل مسیر یا ماه درست نیست");
                msgwin.ShowDialog();
                return;
            }

            try
            {

            }
            catch (Exception)
            {

            }

            #region MyRegion
            int errorno;
            double j3;
            string dbname, tablename, SQL;
            object fs, a = default;
            string DBPATH, DESPATH, COMM;
            double zereshk;
            int i;
            object retVal;
            bool TENDAR;
            MODATE = Baseknow.YEA * 10000 + Convert.ToInt32(MMO.SelectedValue) * 100 + 31;
            var rst = dbms.DoGetDataSQL<bool>("SELECT   TENDAR FROM dbo.WORKHEAD WHERE (dbo.UMonth(WDATE) = " + this.MMO.SelectedValue + ")").ToList();
            if (rst.Count > 0)
            {
                TENDAR = rst.FirstOrDefault();
                MODATE = Baseknow.YEA * 10000 + Convert.ToInt32(MMO.SelectedValue) * 100 + 31;
                dbname = CL_Generaly.General_DBname;
                dbms.DoExecuteSQL("SET ANSI_WARNINGS OFF");
                dbms.DoExecuteSQL("delete from dbo.DSKWOR00");
                dbms.DoExecuteSQL("delete from dbo.DSKKAR00");
                if (TENDAR)
                {
                    int EPERS;
                    double MABJ;
                    double MAB2;
                    double MABMASH;
                    var NST0 = dbms.DoGetDataSQL<int?>("SELECT     COUNT(dbo.WORKING.PCODE) AS et FROM    dbo.WORKING INNER JOIN   dbo.PERSONEL ON dbo.WORKING.PCODE = dbo.PERSONEL.CODE WHERE     (dbo.WORKING.DAYS <> 0) AND (dbo.PERSONEL.DIHARD <> 1) AND (dbo.Umonth(dbo.WORKING.WDATE) = " + MMO.SelectedValue + ") and (BIMEH_NUM > 50) AND (dbo.PERSONEL.TAB56 = 0)").ToList();
                    if (!(NST0 is null))
                    {
                        EPERS = (int)NST0.FirstOrDefault();
                    }
                    else
                    {
                        EPERS = 0;
                    }
                    var NST1 = dbms.DoGetDataSQL<NST_DISKET_BIMEH_QRE>("SELECT  CODE, PNAME, PFAMILY, PNAME + ' ' + PFAMILY AS PNPF, KHNOWNUM, FATHER, BIMEH_NUM, JOB, SEX, WSDAT, WEDATE, WDATE, DAYS, SALARY_DAYLY, DAYS * SALARY_DAYLY AS mosalary, HOME * DAYS AS hom, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) as zereshk, " + " dbo.UIIF(TAB56,N'=', 0, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH),0) AS jmazsalmash, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', -1, CHILDREN, 0)  AS jamazsalmash, dbo.UIIF(DIHARD, N'=', 1, 0, CONVERT(INT, (DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07)) AS bimper, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', - 1, CHILDREN, 0) - CONVERT(INT,(DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07) AS ghabel, CHILDREN  AS chid, CONDITIONS, JAZB, SAYER, EZAFAH,PADASH, KASR_VAM, dbo.Umonth(WDATE) AS MM, CONVERT(INT, (DAYS * SALARY_DAYLY + HOME * DAYS) * 0.07) AS bimper2, dbo.UIIF(TAB56," + " N'=', 0, 0, DAYS * SALARY_DAYLY + HOME * DAYS) AS JAMMAHS20, DSW_IDPLC, DSW_NAT, DSW_BDATE, MaxOfHDATE, MAZMASH,CAST(dbo.UIIF(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH), N'>', " + Baseknow.SAGHFH + ",(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) - " + Baseknow.SAGHFH + ") * 0.1, 0) AS int) AS mali, TAB56 FROM         dbo.LIST_SALARY_END_10(" + MMO.SelectedValue + ", " + Convert.ToByte(Baseknow.HOLA) + ", " + Convert.ToByte(Baseknow.HSAY) + "," + Convert.ToByte(Baseknow.HJAZ) + ", " + Convert.ToByte(Baseknow.HNAH) + ", " + Convert.ToByte(Baseknow.HCON) + ", " + Convert.ToByte(Baseknow.HKHA) + ", " + MODATE + "," + Baseknow.SANAVP + "," + Convert.ToByte(Baseknow.HSHI) + "," + Baseknow.YEA + "," + Convert.ToByte(Baseknow.HEZA) + "," + Convert.ToByte(Baseknow.HBON) + ") LIST_SALARY_END_10 WHERE     (BIMEH_NUM > 50) AND (NOT (BIMEH_NUM IS NULL)) AND (dbo.Umonth(WDATE) = " + MMO.SelectedValue + ") AND (DIHARD <> 1)").ToList();
                    MABMASH = 0d;
                    zereshk = 0d;
                    for (int EOF = 0; EOF < NST1.Count;)
                    {
                        MABMASH = MABMASH + Math.Round((double)NST1[EOF].jmazsalmash);
                        zereshk = zereshk + Math.Round((double)NST1[EOF].zereshk);
                        //NST1.MoveNext();
                        EOF++;
                    }
                    MAB2 = Math.Round(MABMASH / EPERS * 0.2d * (EPERS - 5));
                    var NST = dbms.DoGetDataSQL<NST_DISKET_BIMEH_QRE2>("SELECT  CODE, PNAME, PFAMILY, PNAME + ' ' + PFAMILY AS PNPF, KHNOWNUM, FATHER, BIMEH_NUM, JOB, SEX, WSDAT, WEDATE, WDATE, DAYS, SALARY_DAYLY, DAYS * SALARY_DAYLY AS mosalary, HOME * DAYS AS hom, dbo.UIIF(TAB56,N'=', 0, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH),0) AS jmazsalmash, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', -1, CHILDREN, 0)  AS jamazsalmash, dbo.UIIF(DIHARD, N'=', 1, 0, CONVERT(INT, (DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07)) AS bimper, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', - 1, CHILDREN, 0) - CONVERT(INT,(DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07) AS ghabel, CHILDREN  AS chid, CONDITIONS, JAZB, SAYER, EZAFAH,PADASH, KASR_VAM, dbo.Umonth(WDATE) AS MM, CONVERT(INT, (DAYS * SALARY_DAYLY + HOME * DAYS) * 0.07) AS bimper2, dbo.UIIF(TAB56," + " N'=', 0, 0, DAYS * SALARY_DAYLY + HOME * DAYS) AS JAMMAHS20, DSW_IDPLC, DSW_NAT, DSW_BDATE, MaxOfHDATE, MAZMASH,CAST(dbo.UIIF(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH), N'>', " + Baseknow.SAGHFH + ",(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) - " + Baseknow.SAGHFH + ") * 0.1, 0) AS int) AS mali, TAB56 FROM         dbo.LIST_SALARY_END_10(" + MMO.SelectedValue + ", " + Convert.ToByte(Baseknow.HOLA) + ", " + Convert.ToByte(Baseknow.HSAY) + "," + Convert.ToByte(Baseknow.HJAZ) + ", " + Convert.ToByte(Baseknow.HNAH) + ", " + Convert.ToByte(Baseknow.HCON) + ", " + Convert.ToByte(Baseknow.HKHA) + ", " + MODATE + "," + Baseknow.SANAVP + "," + Convert.ToByte(Baseknow.HSHI) + "," + Baseknow.YEA + "," + Convert.ToByte(Baseknow.HEZA) + "," + Convert.ToByte(Baseknow.HBON) + ") LIST_SALARY_END_10 WHERE     (BIMEH_NUM > 50) AND (NOT (BIMEH_NUM IS NULL)) AND (dbo.Umonth(WDATE) = " + MMO.SelectedValue + ") AND (DIHARD = 1)").ToList();
                    MABJ = 0d;
                    j3 = 0d;
                    for (int IOF = 0; IOF < NST.Count;)
                    {
                        MABJ = MABJ + Math.Round((double)(NST[IOF].jmazsalmash * 0.15d));
                        j3 = j3 + Math.Round((double)(NST[IOF].jmazsalmash * 0.03d));
                        IOF++;
                    }
                    //DoCmd.OpenForm("LIST_SALARY22_10", default, default, default, default, acHidden);
                    //RST2
                    var OPENFORM1 = dbms.DoGetDataSQL<LIST_SALARY22_10_CHSARP>($"SELECT * FROM dbo.LIST_SALARY22_10({MMO.SelectedValue},{Convert.ToByte(Baseknow.HOLA)},{Convert.ToByte(Baseknow.HSAY)},{Convert.ToByte(Baseknow.HJAZ)},{Convert.ToByte(Baseknow.HNAH)},{Convert.ToByte(Baseknow.HCON)},{Convert.ToByte(Baseknow.HKHA)},{MODATE/*HOKMDATE*/},{Baseknow.SAGHFH},{Baseknow.HSANP},{Convert.ToByte(Baseknow.HSHI)},{Baseknow.YEA},{Convert.ToByte(Baseknow.HEZA)},{Convert.ToByte(Baseknow.HBON)})").ToList();

                    //DoCmd.OpenForm("LIST_SALARY22TOFILE_10", default, default, default, default, acHidden);
                    //RST1
                    var OPENFORM2 = dbms.DoGetDataSQL<LIST_SALARY22TOFILE_10_CSHARP>($"SELECT * FROM dbo.LIST_SALARY22TOFILE_10({MMO.SelectedValue},{Convert.ToByte(Baseknow.HOLA)},{Convert.ToByte(Baseknow.HSAY)},{Convert.ToByte(Baseknow.HJAZ)},{Convert.ToByte(Baseknow.HNAH)},{Convert.ToByte(Baseknow.HCON)},{Convert.ToByte(Baseknow.HKHA)},{MODATE/*HOKMDATE*/},{Baseknow.SAGHFH},{Baseknow.HSANP},{Convert.ToByte(Baseknow.HSHI)},{Baseknow.YEA},{Convert.ToByte(Baseknow.HEZA)},{Convert.ToByte(Baseknow.HBON)})").ToList();

                    //rst.MoveLast();
                    //rst.MoveFirst();
                    //  Set rst = Forms![LIST_SALARY22TOFILE_10].Recordset
                    for (int OFR = 0; OFR < OPENFORM2.Count;)
                    {
                        try
                        {
                            //a.writeline(Forms["BASEKNOW"]["UNIVERSITY_CO"] + "," + Forms["BASEKNOW"]["NAME"] + "," + Forms["BASEKNOW"]["MANAGER"] + "," + Interaction.IIf(IsNull(Forms["BASEKNOW"]["TFADDRESS"]), " ", Forms["BASEKNOW"]["TFADDRESS"]) + "," + "0" + "," + UYear(rst.Fields("WDATE")) + "," + UMonth(rst.Fields("WDATE")) + "," + UMonth(rst.Fields("WDATE")) + "," + "2" + "," + rst.Fields("DSK_NUM") + "," + rst.Fields("DSK_TDD") + "," + rst.Fields("DSK_TROOZ") + "," + Math.Round(rst.Fields("DSK_TMAH")) + "," + Math.Round(rst.Fields("DSK_TMAZ")) + "," + Math.Round[zereshk] + "," + Math.Round(rst.Fields("DSK_TTOTL")) + "," + Math.Round(rst.Fields("DSK_TBIME")) + "," + Math.Round[MAB2 + MABJ] + "," + Math.Round(rst.Fields("DSK_BIC")) + ",23,0,0");
                            //Information.Err().Clear();
                            dbms.DoExecuteSQL("INSERT INTO dbo.DSKKAR00 ([DSK_ID],[DSK_NAME],[DSK_FARM],[DSK_ADRS],[DSK_KIND],[DSK_YY],[DSK_MM],[DSK_LISTNO],[DSK_DISC],[DSK_NUM],[DSK_TDD],[DSK_TROOZ],[DSK_TMAH],[DSK_TMAZ],[DSK_TMASH],[DSK_TTOTL],[DSK_TBIME],[DSK_TKOSO],[DSK_BIC],[DSK_RATE],[DSK_PRATE],[DSK_BIMH],[MON_PYM]) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "',N'" + Baseknow.NAME + "',N'" + Baseknow.MANAGER + "', N'" + Interaction.IIf(IsNull(Baseknow.TFADDRESS), " ", Baseknow.TFADDRESS) + "',0," + CL_HESABDARI.UYear(Convert.ToString(OPENFORM2[OFR].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM2[OFR].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM2[OFR].WDATE)) + ",' ',2," + OPENFORM2[OFR].DSK_NUM + "," + OPENFORM2[OFR].DSK_TDD + "," + OPENFORM2[OFR].DSK_TROOZ + "," + Math.Round((double)OPENFORM2[OFR].DSK_TMAH) + "," + Math.Round((double)OPENFORM2[OFR].DSK_TMAZ) + "," + Math.Round((double)OPENFORM2[OFR].DSK_TMAH) + Math.Round((double)OPENFORM2[OFR].DSK_TMAZ) + "," + Math.Round((double)OPENFORM2[OFR].DSK_TTOTL) + "," + Math.Round((double)OPENFORM2[OFR].DSK_TBIME) + "," + Math.Round(MAB2 + MABJ) + "," + Math.Round((double)OPENFORM2[OFR].DSK_BIC) + ",23,0,'0')");

                        }
                        catch (Exception)
                        {
                            //if (Information.Err() != 0)
                            //{
                            //    DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "خطا در ارسال اطلاعات :" + " " + );
                            //    Information.Err().Clear();
                            //}
                            Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :");
                            msgwin.ShowDialog();
                        }
                        //rst.MoveNext();
                        OFR++;
                    }
                    //rst.Close();
                    //rst.MoveLast();
                    //rst.MoveFirst();
                    //a.Close();
                    for (int Fields = 0; Fields < OPENFORM1.Count;)
                    {
                        //Forms["BUN"].num.CAPTION = OPENFORM1[Fields].Fields("PNAME") + " " + OPENFORM1[Fields].Fields("PFAMILY");
                        //Forms["BUN"].Form.Refresh();
                        //DoEvents();
                        if (OPENFORM1[Fields].BIMEH_NUM > 50)
                        {
                            if (OPENFORM1[Fields].DAYS == 0)
                            {
                                //a.writeline(Forms["BASEKNOW"]["UNIVERSITY_CO"] + "," + UYear(OPENFORM1[Fields].Fields("WDATE")) + "," + UMonth(OPENFORM1[Fields].Fields("WDATE")) + "," + UMonth(OPENFORM1[Fields].Fields("WDATE")) + "," + OPENFORM1[Fields].Fields("BIMEH_NUM") + "," + OPENFORM1[Fields].Fields("PNAME") + "," + OPENFORM1[Fields].Fields("PFAMILY") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("FATHER")), "   ", OPENFORM1[Fields].Fields("FATHER")) + "," + OPENFORM1[Fields].Fields("KHNOWNUM") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_IDPLC")), "", OPENFORM1[Fields].Fields("DSW_IDPLC")) + "," + "0" + "," + Interaction.IIf(OPENFORM1[Fields].Fields("SEX"), "مرد", "زن ") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_NAT")), "", OPENFORM1[Fields].Fields("DSW_NAT")) + "," + Getjobname(OPENFORM1[Fields].Fields("JOB")) + "," + Strings.Format(OPENFORM1[Fields].Fields("WSDAT"), "####/##/##") + "," + Strings.Format(OPENFORM1[Fields].Fields("WEDATE"), "####/##/##") + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + OPENFORM1[Fields].Fields("JOB") + "," + Strings.Format(OPENFORM1[Fields].Fields("DSW_BDATE"), "####/##/##") + "," + OPENFORM1[Fields].Fields("MELLICOD"));
                                try
                                {
                                    dbms.DoExecuteSQL("INSERT INTO dbo.DSKWOR00 (DSW_ID,DSW_YY,DSW_MM,DSW_LISTNO,DSW_ID1,DSW_FNAME,DSW_LNAME,DSW_DNAME,DSW_IDNO,DSW_IDPLC,DSW_IDATE,DSW_BDATE,DSW_SEX,DSW_NAT,DSW_OCP,DSW_SDATE,DSW_EDATE,DSW_DD,DSW_ROOZ,DSW_MAH,DSW_MAZ,DSW_MASH,DSW_TOTL,DSW_BIME,DSW_PRATE,DSW_JOB,PER_NATCOD) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "'," + CL_HESABDARI.UYear(Convert.ToString(OPENFORM1[Fields].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM1[Fields].WDATE)) + ", N'" + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM1[Fields].WDATE)) + "',N'" + OPENFORM1[Fields].BIMEH_NUM + "',N'" + OPENFORM1[Fields].PNAME + "',N'" + OPENFORM1[Fields].PFAMILY + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].FATHER), "   ", OPENFORM1[Fields].FATHER) + "',N'" + OPENFORM1[Fields].KHNOWNUM + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].DSW_IDPLC), "", OPENFORM1[Fields].DSW_IDPLC) + "', N'0',N'" + OPENFORM1[Fields].DSW_BDATE + "',N'" + Interaction.IIf(Convert.ToBoolean(OPENFORM1[Fields].SEX), "مرد", "زن ") + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].DSW_NAT), "", OPENFORM1[Fields].DSW_NAT) + "',N'" + CL_HESABDARI.Getjobname(OPENFORM1[Fields].JOB) + "',N'" + OPENFORM1[Fields].WSDAT + "',N'" + OPENFORM1[Fields].WEDATE + "'," + 0 + ", " + 0 + "," + 0 + ", " + 0 + "," + 0 + "," + 0 + "," + OPENFORM1[Fields].bimper + ",'0','" + OPENFORM1[Fields].JOB + "',N'" + OPENFORM1[Fields].MELLICOD + "')");

                                }
                                catch (Exception)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :" + OPENFORM1[Fields].PNAME + " " + OPENFORM1[Fields].PFAMILY + " ");
                                    msgwin.ShowDialog();
                                }
                            }

                            else
                            {
                                //a.writeline(Forms["BASEKNOW"]["UNIVERSITY_CO"] + "," + UYear(OPENFORM1[Fields].Fields("WDATE")) + "," + UMonth(OPENFORM1[Fields].Fields("WDATE")) + "," + UMonth(OPENFORM1[Fields].Fields("WDATE")) + "," + OPENFORM1[Fields].Fields("BIMEH_NUM") + "," + OPENFORM1[Fields].Fields("PNAME") + "," + OPENFORM1[Fields].Fields("PFAMILY") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("FATHER")), "   ", OPENFORM1[Fields].Fields("FATHER")) + "," + OPENFORM1[Fields].Fields("KHNOWNUM") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_IDPLC")), "", OPENFORM1[Fields].Fields("DSW_IDPLC")) + "," + "0" + "," + Interaction.IIf(OPENFORM1[Fields].Fields("SEX"), "مرد", "زن ") + "," + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_NAT")), "", OPENFORM1[Fields].Fields("DSW_NAT")) + "," + Getjobname(OPENFORM1[Fields].Fields("JOB")) + "," + Strings.Format(OPENFORM1[Fields].Fields("WSDAT"), "####/##/##") + "," + Strings.Format(OPENFORM1[Fields].Fields("WEDATE"), "####/##/##") + "," + OPENFORM1[Fields].Fields("DAYS") + "," + OPENFORM1[Fields].Fields("SALARY_DAYLY") + "," + OPENFORM1[Fields].Fields("mosalary") + "," + OPENFORM1[Fields].Fields("mazmash") + "," + OPENFORM1[Fields].Fields("jmazsalmash") + "," + OPENFORM1[Fields].Fields("jamazsalmash") + "," + OPENFORM1[Fields].Fields("bimper") + "," + "0" + "," + OPENFORM1[Fields].Fields("JOB") + "," + Strings.Format(OPENFORM1[Fields].Fields("DSW_BDATE"), "####/##/##") + "," + OPENFORM1[Fields].Fields("MELLICOD"));
                                try
                                {
                                    //1
                                    dbms.DoExecuteSQL("INSERT INTO dbo.DSKWOR00 (DSW_ID,DSW_YY,DSW_MM,DSW_LISTNO,DSW_ID1,DSW_FNAME,DSW_LNAME,DSW_DNAME,DSW_IDNO,DSW_IDPLC,DSW_IDATE,DSW_BDATE,DSW_SEX,DSW_NAT,DSW_OCP,DSW_SDATE,DSW_EDATE,DSW_DD,DSW_ROOZ,DSW_MAH,DSW_MAZ,DSW_MASH,DSW_TOTL,DSW_BIME,DSW_PRATE,DSW_JOB,PER_NATCOD) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "'," + CL_HESABDARI.UYear(Convert.ToString(OPENFORM1[Fields].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM1[Fields].WDATE)) + ", N'" + CL_HESABDARI.UMonth(Convert.ToString(OPENFORM1[Fields].WDATE)) + "',N'" + OPENFORM1[Fields].BIMEH_NUM + "',N'" + OPENFORM1[Fields].PNAME + "',N'" + OPENFORM1[Fields].PFAMILY + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].FATHER), "   ", OPENFORM1[Fields].FATHER) + "',N'" + OPENFORM1[Fields].KHNOWNUM + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].DSW_IDPLC), "", OPENFORM1[Fields].DSW_IDPLC) + "', N'0',N'" + OPENFORM1[Fields].DSW_BDATE + "',N'" + Interaction.IIf(Convert.ToBoolean(OPENFORM1[Fields].SEX), "مرد", "زن ") + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].DSW_NAT), "", OPENFORM1[Fields].DSW_NAT) + "',N'" + CL_HESABDARI.Getjobname(OPENFORM1[Fields].JOB) + "',N'" + OPENFORM1[Fields].WSDAT + "',N'" + OPENFORM1[Fields].WEDATE + "'," + OPENFORM1[Fields].DAYS + ", " + OPENFORM1[Fields].SALARY_DAYLY + "," + OPENFORM1[Fields].mosalary + ", " + OPENFORM1[Fields].MAZMASH + "," + OPENFORM1[Fields].jmazsalmash + "," + OPENFORM1[Fields].jamazsalmash + OPENFORM1[Fields].bimper + ",'0','" + OPENFORM1[Fields].JOB + "',N'" + OPENFORM1[Fields].MELLICOD + "')");
                                    /////dbms.DoExecuteSQL("INSERT INTO dbo.DSKWOR00 (DSW_ID,DSW_YY,DSW_MM,DSW_LISTNO,DSW_ID1,DSW_FNAME,DSW_LNAME,DSW_DNAME,DSW_IDNO,DSW_IDPLC,DSW_IDATE,DSW_BDATE,DSW_SEX,DSW_NAT,DSW_OCP,DSW_SDATE,DSW_EDATE,DSW_DD,DSW_ROOZ,DSW_MAH,DSW_MAZ,DSW_MASH,DSW_TOTL,DSW_BIME,DSW_PRATE,DSW_JOB,PER_NATCOD) " + "  VALUES( N'" + Forms["BASEKNOW"]["UNIVERSITY_CO"] + "'," + UYear(OPENFORM1[Fields].Fields("WDATE")) + "," + UMonth(OPENFORM1[Fields].Fields("WDATE")) + ", N'" + UMonth(OPENFORM1[Fields].Fields("WDATE")) + "',N'" + OPENFORM1[Fields].Fields("BIMEH_NUM") + "',N'" + OPENFORM1[Fields].Fields("PNAME") + "',N'" + OPENFORM1[Fields].Fields("PFAMILY") + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("FATHER")), "   ", OPENFORM1[Fields].Fields("FATHER")) + "',N'" + OPENFORM1[Fields].Fields("KHNOWNUM") + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_IDPLC")), "", OPENFORM1[Fields].Fields("DSW_IDPLC")) + "', N'0',N'" + OPENFORM1[Fields].Fields("DSW_BDATE") + "',N'" + Interaction.IIf(OPENFORM1[Fields].Fields("SEX"), "مرد", "زن ") + "',N'" + Interaction.IIf(IsNull(OPENFORM1[Fields].Fields("DSW_NAT")), "", OPENFORM1[Fields].Fields("DSW_NAT")) + "',N'" + Getjobname(OPENFORM1[Fields].Fields("JOB")) + "',N'" + OPENFORM1[Fields].Fields("WSDAT") + "',N'" + OPENFORM1[Fields].Fields("WEDATE") + "'," + OPENFORM1[Fields].Fields("DAYS") + ", " + OPENFORM1[Fields].Fields("SALARY_DAYLY") + "," + OPENFORM1[Fields].Fields("mosalary") + ", " + OPENFORM1[Fields].Fields("mazmash") + "," + OPENFORM1[Fields].Fields("jmazsalmash") + "," + OPENFORM1[Fields].Fields("jamazsalmash") + "," + OPENFORM1[Fields].Fields("bimper") + ",'0','" + OPENFORM1[Fields].Fields("JOB") + "',N'" + OPENFORM1[Fields].Fields("MELLICOD") + "')");

                                }
                                catch (Exception)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :" + OPENFORM1[Fields].PNAME + " " + OPENFORM1[Fields].PFAMILY + " ");
                                    msgwin.ShowDialog();
                                }


                            }
                        }
                        //OPENFORM1[Fields].MoveNext();
                        Fields++;
                    }
                    //a.Close();
                    // FileCopy "c:\NEGIN\DSKKAR00.DBF", "C:\DSKKAR00.DBF"
                    // FileCopy "c:\NEGIN\DSKWOR00.DBF", "C:\DSKWOR00.DBF"
                    // COMM = "c:\CONVERT.EXE  "
                    // retval = Shell(COMM, vbNormalFocus)
                    // DoCmd.OpenForm "mesag", , , , , acDialog, "لطفا پس از تبديل فايل در برنامه تحت داس پنجره آن را ببنديد و گزينه تائيد را انتخاب كنيد...."
                    //if (PATH != @"c:\")
                    //{
                    //    FileSystem.FileCopy(@"C:\DSKKAR00.txt", PATH + "DSKKAR00.txt");
                    //    FileSystem.FileCopy(@"C:\DSKWOR00.txt", PATH + "DSKWOR00.txt");
                    //}
                    //***////DoCmd.OpenForm("mesag", default, default, default, default, acDialog, "عمليات ارسال اطلاعات و تهيه دسيكت با موفقيت انجام شد با موفقيت به انجام رسيد");
                    //DoCmd.Close(acForm, "F_MENU_BIM");
                    //DoCmd.Close(acForm, "LIST_SALARY22TOFILE");
                    //DoCmd.Close(acForm, "LIST_SALARY22");
                }
                else
                {
                    //DoCmd.Close(acForm, "LIST_SALARY22TOFILE");
                    //DoCmd.Close(acForm, "LIST_SALARY22");

                    string test1 = $"SELECT * FROM dbo.LIST_SALARY22({MMO.SelectedValue},{Convert.ToByte(Baseknow.HOLA)},{Convert.ToByte(Baseknow.HSAY)},{Convert.ToByte(Baseknow.HJAZ)},{Convert.ToByte(Baseknow.HNAH)},{Convert.ToByte(Baseknow.HCON)},{Convert.ToByte(Baseknow.HKHA)},{MODATE/*HOKMDATE*/},{Baseknow.SAGHFH},{Baseknow.HSANP},{Convert.ToByte(Baseknow.HSHI)},{Baseknow.YEA},{Convert.ToByte(Baseknow.HEZA)},{Convert.ToByte(Baseknow.HBON)})";

                    var RST2_2 = dbms.DoGetDataSQL<LIST_SALARY22_CHSARP1_1>($"SELECT * FROM dbo.LIST_SALARY22({MMO.SelectedValue},{Convert.ToByte(Baseknow.HOLA)},{Convert.ToByte(Baseknow.HSAY)},{Convert.ToByte(Baseknow.HJAZ)},{Convert.ToByte(Baseknow.HNAH)},{Convert.ToByte(Baseknow.HCON)},{Convert.ToByte(Baseknow.HKHA)},{MODATE/*HOKMDATE*/},{Baseknow.SAGHFH},{Baseknow.HSANP},{Convert.ToByte(Baseknow.HSHI)},{Baseknow.YEA},{Convert.ToByte(Baseknow.HEZA)},{Convert.ToByte(Baseknow.HBON)})").ToList();

                    //RST
                    var RST22_22 = dbms.DoGetDataSQL<LIST_SALARY22TOFILE_10_CSHARP>($"SELECT * FROM dbo.LIST_SALARY22TOFILE({MMO.SelectedValue},{Convert.ToByte(Baseknow.HOLA)},{Convert.ToByte(Baseknow.HSAY)},{Convert.ToByte(Baseknow.HJAZ)},{Convert.ToByte(Baseknow.HNAH)},{Convert.ToByte(Baseknow.HCON)},{Convert.ToByte(Baseknow.HKHA)},{MODATE/*HOKMDATE*/},{Baseknow.SAGHFH},{Baseknow.HSANP},{Convert.ToByte(Baseknow.HSHI)},{Baseknow.YEA},{Convert.ToByte(Baseknow.HEZA)},{Convert.ToByte(Baseknow.HBON)})").ToList();

                    //DoCmd.OpenForm("LIST_SALARY22", default, default, default, default, acHidden);
                    //DoCmd.OpenForm("LIST_SALARY22TOFILE", default, default, default, default, acHidden);

                    var NST0 = dbms.DoGetDataSQL<NST_DISKET_BIMEH_QRE2>("SELECT  CODE, PNAME, PFAMILY, PNAME + ' ' + PFAMILY AS PNPF, KHNOWNUM, FATHER, BIMEH_NUM, JOB, SEX, WSDAT, WEDATE, WDATE, DAYS, SALARY_DAYLY, DAYS * SALARY_DAYLY AS mosalary, HOME * DAYS AS hom, dbo.UIIF(TAB56,N'=', 0, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH),0) AS jmazsalmash, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', -1, CHILDREN, 0)  AS jamazsalmash, dbo.UIIF(DIHARD, N'=', 1, 0, CONVERT(INT, (DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07)) AS bimper, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', - 1, CHILDREN, 0) - CONVERT(INT,(DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07) AS ghabel, CHILDREN  AS chid, CONDITIONS, JAZB, SAYER, EZAFAH,PADASH, KASR_VAM, dbo.Umonth(WDATE) AS MM, CONVERT(INT, (DAYS * SALARY_DAYLY + HOME * DAYS) * 0.07) AS bimper2, dbo.UIIF(TAB56," + " N'=', 0, 0, DAYS * SALARY_DAYLY + HOME * DAYS) AS JAMMAHS20, DSW_IDPLC, DSW_NAT, DSW_BDATE, MaxOfHDATE, MAZMASH,CAST(dbo.UIIF(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH), N'>', " + Baseknow.SAGHFH + ",(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) - " + Baseknow.SAGHFH + ") * 0.1, 0) AS int) AS mali, TAB56 FROM         dbo.LIST_SALARY_END(" + MMO.SelectedValue + ", " + Convert.ToByte(Baseknow.HOLA) + ", " + Convert.ToByte(Baseknow.HSAY) + "," + Convert.ToByte(Baseknow.HJAZ) + ", " + Convert.ToByte(Baseknow.HNAH) + ", " + Convert.ToByte(Baseknow.HCON) + ", " + Convert.ToByte(Baseknow.HKHA) + ", " + MODATE + "," + Baseknow.SANAVP + "," + Convert.ToByte(Baseknow.HSHI) + "," + Baseknow.YEA + "," + Convert.ToByte(Baseknow.HEZA) + "," + Convert.ToByte(Baseknow.HBON) + ") LIST_SALARY_END WHERE  (BIMEH_NUM > 50) AND (NOT (BIMEH_NUM IS NULL)) AND (dbo.Umonth(WDATE) = " + MMO.SelectedValue + ") AND (DIHARD <> 1)").ToList();
                    MABMASH = 0;
                    for (int NSTEOF = 0; NSTEOF < NST0.Count; NSTEOF++)
                    {
                        MABMASH = (int)(MABMASH + Math.Round((double)NST0[NSTEOF].jmazsalmash));
                        //NST0.MoveNext();
                        NSTEOF++;
                    }
                    MAB2 = Math.Round(MABMASH * 0.2d);
                    var NST = dbms.DoGetDataSQL<NST_DISKET_BIMEH_QRE2>("SELECT  CODE, PNAME, PFAMILY, PNAME + ' ' + PFAMILY AS PNPF, KHNOWNUM, FATHER, BIMEH_NUM, JOB, SEX, WSDAT, WEDATE, WDATE, DAYS, SALARY_DAYLY, DAYS * SALARY_DAYLY AS mosalary, HOME * DAYS AS hom, dbo.UIIF(TAB56,N'=', 0, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH),0) AS jmazsalmash, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', -1, CHILDREN, 0)  AS jamazsalmash, dbo.UIIF(DIHARD, N'=', 1, 0, CONVERT(INT, (DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07)) AS bimper, DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) + dbo.UIIF(" + Convert.ToByte(Baseknow.HOLA) + ", N'=', - 1, CHILDREN, 0) - CONVERT(INT,(DAYS * SALARY_DAYLY + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH)) * 0.07) AS ghabel, CHILDREN  AS chid, CONDITIONS, JAZB, SAYER, EZAFAH,PADASH, KASR_VAM, dbo.Umonth(WDATE) AS MM, CONVERT(INT, (DAYS * SALARY_DAYLY + HOME * DAYS) * 0.07) AS bimper2, dbo.UIIF(TAB56," + " N'=', 0, 0, DAYS * SALARY_DAYLY + HOME * DAYS) AS JAMMAHS20, DSW_IDPLC, DSW_NAT, DSW_BDATE, MaxOfHDATE, MAZMASH,CAST(dbo.UIIF(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH), N'>', " + Baseknow.SAGHFH + ",(DAYS * SALARY_DAYLY + CHI + dbo.UIIF(DAYS, N'=', 0, 0, MAZMASH) - " + Baseknow.SAGHFH + ") * 0.1, 0) AS int) AS mali, TAB56 FROM         dbo.LIST_SALARY_END(" + MMO.SelectedValue + ", " + Convert.ToByte(Baseknow.HOLA) + ", " + Convert.ToByte(Baseknow.HSAY) + "," + Convert.ToByte(Baseknow.HJAZ) + ", " + Convert.ToByte(Baseknow.HNAH) + ", " + Convert.ToByte(Baseknow.HCON) + ", " + Convert.ToByte(Baseknow.HKHA) + ", " + MODATE + "," + Baseknow.SANAVP + "," + Convert.ToByte(Baseknow.HSHI) + "," + Baseknow.YEA + "," + Convert.ToByte(Baseknow.HEZA) + "," + Convert.ToByte(Baseknow.HBON) + ") LIST_SALARY_END WHERE  (BIMEH_NUM > 50) AND (NOT (BIMEH_NUM IS NULL)) AND (dbo.Umonth(WDATE) = " + MMO.SelectedValue + ") AND (DIHARD = 1)").ToList();
                    MABJ = 0;
                    j3 = 0d;
                    for (int NSTiEOF = 0; NSTiEOF < NST.Count;)
                    {
                        MABJ = (int)(MABJ + Math.Round((double)(NST[NSTiEOF].jmazsalmash * 0.15d)));
                        j3 = j3 + Math.Round((double)(NST[NSTiEOF].jmazsalmash * 0.03d));
                        //NST.MoveNext();
                        NSTiEOF++;
                    };
                    for (int ii = 0; ii < RST22_22.Count;)
                    {
                        try
                        {
                            dbms.DoExecuteSQL("INSERT INTO dbo.DSKKAR00 ([DSK_ID],[DSK_NAME],[DSK_FARM],[DSK_ADRS],[DSK_KIND],[DSK_YY],[DSK_MM],[DSK_LISTNO],[DSK_DISC],[DSK_NUM],[DSK_TDD],[DSK_TROOZ],[DSK_TMAH],[DSK_TMAZ],[DSK_TMASH],[DSK_TTOTL],[DSK_TBIME],[DSK_TKOSO],[DSK_BIC],[DSK_RATE],[DSK_PRATE],[DSK_BIMH],[MON_PYM]) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "',N'" + Baseknow.NAME + "',N'" + Baseknow.MANAGER + "', N'" + Interaction.IIf(IsNull(Baseknow.TFADDRESS), " ", Baseknow.TFADDRESS) + "',0," + CL_HESABDARI.UYear(Convert.ToString(RST22_22[ii].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(RST22_22[ii].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(RST22_22[ii].WDATE)) + ",' ',2," + RST22_22[ii].DSK_NUM + "," + RST22_22[ii].DSK_TDD + "," + RST22_22[ii].DSK_TROOZ + "," + Math.Round((double)RST22_22[ii].DSK_TMAH) + "," + Math.Round((double)RST22_22[ii].DSK_TMAZ) + "," + Math.Round((double)RST22_22[ii].DSK_TMAH) + Math.Round((double)RST22_22[ii].DSK_TMAZ) + "," + Math.Round((double)RST22_22[ii].DSK_TTOTL) + "," + Math.Round((double)RST22_22[ii].DSK_TBIME) + "," + Math.Round(MAB2 + MABJ) + "," + Math.Round((double)RST22_22[ii].DSK_BIC) + ",23,0,'0')");
                        }
                        catch (Exception)
                        {
                            Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :");
                            msgwin.ShowDialog();
                        }
                        //rst.MoveNext();
                        ii++;
                    }
                    //rst.Close();
                    //rst.MoveLast();
                    //rst.MoveFirst();
                    //a.Close();
                    for (int ii = 0; ii < RST2_2.Count; ii++)
                    {
                        //Forms["BUN"].num.CAPTION = rst.Fields("PNAME") + " " + rst.Fields("PFAMILY");
                        //Forms["BUN"].Form.Refresh();
                        //DoEvents();
                        if (RST2_2[ii].BIMEH_NUM > 50)
                        {
                            if (RST2_2[ii].DAYS == 0)
                            {
                                //a.writeline(Forms["BASEKNOW"]["UNIVERSITY_CO"] + "," + UYear(RST2_2[ii].("WDATE")) + "," + UMonth(RST2_2[ii].("WDATE")) + "," + UMonth(RST2_2[ii].("WDATE")) + "," + RST2_2[ii].("BIMEH_NUM") + "," + RST2_2[ii].("PNAME") + "," + RST2_2[ii].("PFAMILY") + "," + Interaction.IIf(IsNull(RST2_2[ii].("FATHER")), "   ", RST2_2[ii].("FATHER")) + "," + RST2_2[ii].("KHNOWNUM") + "," + Interaction.IIf(IsNull(RST2_2[ii].("DSW_IDPLC")), "", RST2_2[ii].("DSW_IDPLC")) + "," + "0" + "," + Interaction.IIf(RST2_2[ii].("SEX"), "مرد", "زن ") + "," + Interaction.IIf(IsNull(RST2_2[ii].("DSW_NAT")), "", RST2_2[ii].("DSW_NAT")) + "," + Getjobname(RST2_2[ii].("JOB")) + "," + Strings.Format(RST2_2[ii].("WSDAT"), "####/##/##") + "," + Strings.Format(RST2_2[ii].("WEDATE"), "####/##/##") + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + "0" + "," + RST2_2[ii].("JOB") + "," + Strings.Format(RST2_2[ii].("DSW_BDATE"), "####/##/##") + "," + RST2_2[ii].("MELLICOD"));
                                try
                                {
                                    dbms.DoExecuteSQL("INSERT INTO dbo.DSKWOR00 (DSW_ID,DSW_YY,DSW_MM,DSW_LISTNO,DSW_ID1,DSW_FNAME,DSW_LNAME,DSW_DNAME,DSW_IDNO,DSW_IDPLC,DSW_IDATE,DSW_BDATE,DSW_SEX,DSW_NAT,DSW_OCP,DSW_SDATE,DSW_EDATE,DSW_DD,DSW_ROOZ,DSW_MAH,DSW_MAZ,DSW_MASH,DSW_TOTL,DSW_BIME,DSW_PRATE,DSW_JOB,PER_NATCOD) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "'," + CL_HESABDARI.UYear(Convert.ToString(RST2_2[ii].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(RST2_2[ii].WDATE)) + ", N'" + CL_HESABDARI.UMonth(Convert.ToString(RST2_2[ii].WDATE)) + "',N'" + RST2_2[ii].BIMEH_NUM + "',N'" + RST2_2[ii].PNAME + "',N'" + RST2_2[ii].PFAMILY + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].FATHER), "   ", RST2_2[ii].FATHER) + "',N'" + RST2_2[ii].KHNOWNUM + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].DSW_IDPLC), "", RST2_2[ii].DSW_IDPLC) + "', N'0',N'" + RST2_2[ii].DSW_BDATE + "',N'" + Interaction.IIf(Convert.ToBoolean(RST2_2[ii].SEX), "مرد", "زن ") + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].DSW_NAT), "", RST2_2[ii].DSW_NAT) + "',N'" + CL_HESABDARI.Getjobname(RST2_2[ii].JOB) + "',N'" + RST2_2[ii].WSDAT + "',N'" + RST2_2[ii].WEDATE + "'," + 0 + ", " + 0 + "," + 0 + ", " + 0 + "," + 0 + "," + 0 + "," + RST2_2[ii].bimper + ",'0','" + RST2_2[ii].JOB + "',N'" + RST2_2[ii].MELLICOD + "')");
                                }
                                catch (Exception)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :" + RST2_2[ii].PNAME + " " + RST2_2[ii].PFAMILY); msgwin.ShowDialog();
                                }
                            }
                            else
                            {
                                try
                                {
                                    dbms.DoExecuteSQL("INSERT INTO dbo.DSKWOR00 (DSW_ID,DSW_YY,DSW_MM,DSW_LISTNO,DSW_ID1,DSW_FNAME,DSW_LNAME,DSW_DNAME,DSW_IDNO,DSW_IDPLC,DSW_IDATE,DSW_BDATE,DSW_SEX,DSW_NAT,DSW_OCP,DSW_SDATE,DSW_EDATE,DSW_DD,DSW_ROOZ,DSW_MAH,DSW_MAZ,DSW_MASH,DSW_TOTL,DSW_BIME,DSW_PRATE,DSW_JOB,PER_NATCOD) " + "  VALUES( N'" + Baseknow.UNIVERSITY_CO + "'," + CL_HESABDARI.UYear(Convert.ToString(RST2_2[ii].WDATE)) + "," + CL_HESABDARI.UMonth(Convert.ToString(RST2_2[ii].WDATE)) + ", N'" + CL_HESABDARI.UMonth(Convert.ToString(RST2_2[ii].WDATE)) + "',N'" + RST2_2[ii].BIMEH_NUM + "',N'" + RST2_2[ii].PNAME + "',N'" + RST2_2[ii].PFAMILY + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].FATHER), "   ", RST2_2[ii].FATHER) + "',N'" + RST2_2[ii].KHNOWNUM + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].DSW_IDPLC), "", RST2_2[ii].DSW_IDPLC) + "', N'0',N'" + RST2_2[ii].DSW_BDATE + "',N'" + Interaction.IIf(Convert.ToBoolean(RST2_2[ii].SEX), "مرد", "زن ") + "',N'" + Interaction.IIf(IsNull(RST2_2[ii].DSW_NAT), "", RST2_2[ii].DSW_NAT) + "',N'" + CL_HESABDARI.Getjobname(RST2_2[ii].JOB) + "',N'" + RST2_2[ii].WSDAT + "',N'" + RST2_2[ii].WEDATE + "'," + RST2_2[ii].DAYS + ", " + RST2_2[ii].SALARY_DAYLY + "," + RST2_2[ii].mosalary + ", " + RST2_2[ii].MAZMASH + "," + RST2_2[ii].jmazsalmash + "," + RST2_2[ii].jamazsalmash + "," + RST2_2[ii].bimper + ",'0','" + RST2_2[ii].JOB + "',N'" + RST2_2[ii].MELLICOD + "')");
                                }
                                catch (Exception)
                                {
                                    Msgwin msgwin = new Msgwin(false, "خطا در ارسال اطلاعات :" + RST2_2[ii].PNAME + " " + RST2_2[ii].PFAMILY); msgwin.ShowDialog();
                                }
                            }
                        }
                        //rst.MoveNext();
                    }
                    dbms.DoExecuteSQL("SET ANSI_WARNINGS ON");
                    //a.Close();
                    //if (string.IsNullOrEmpty(FileSystem.Dir(@"C:\CORRECT")))
                    //{
                    //    FileSystem.MkDir(@"C:\CORRECT");
                    //};
                }
            }
            else
            {
                Msgwin msgwin = new Msgwin(false, "براي اين ماه كاربرگ حقوق ثبت نشده است"); msgwin.ShowDialog();
            }
            #endregion

            try
            {
                bool exists = System.IO.Directory.Exists(THE_PATH_FILES);
                if (!exists)
                    Directory.CreateDirectory(THE_PATH_FILES);

                #region CREATE_COLUMN_DESIGN
                //DSKWOR00
                DSWW_FDATA_WORK.Columns.Add("DSW_ID").MaxLength = 10;//1
                DSWW_FDATA_WORK.Columns.Add("DSW_YY").MaxLength = 2;//2
                DSWW_FDATA_WORK.Columns.Add("DSW_MM").MaxLength = 2;//3
                DSWW_FDATA_WORK.Columns.Add("DSW_LISTNO").MaxLength = 12;//4
                DSWW_FDATA_WORK.Columns.Add("DSW_ID1").MaxLength = 10;//5
                DSWW_FDATA_WORK.Columns.Add("DSW_FNAME").MaxLength = 100;//6
                DSWW_FDATA_WORK.Columns.Add("DSW_LNAME").MaxLength = 100;//7
                DSWW_FDATA_WORK.Columns.Add("DSW_DNAME").MaxLength = 100;//8
                DSWW_FDATA_WORK.Columns.Add("DSW_IDNO").MaxLength = 15;//9
                DSWW_FDATA_WORK.Columns.Add("DSW_IDPLC").MaxLength = 100;//10
                DSWW_FDATA_WORK.Columns.Add("DSW_IDATE").MaxLength = 8;//11
                DSWW_FDATA_WORK.Columns.Add("DSW_BDATE").MaxLength = 8;//12
                DSWW_FDATA_WORK.Columns.Add("DSW_SEX").MaxLength = 3; //13
                DSWW_FDATA_WORK.Columns.Add("DSW_NAT").MaxLength = 10;//14
                DSWW_FDATA_WORK.Columns.Add("DSW_OCP").MaxLength = 100;//15
                DSWW_FDATA_WORK.Columns.Add("DSW_SDATE").MaxLength = 8;//16
                DSWW_FDATA_WORK.Columns.Add("DSW_EDATE").MaxLength = 8;//17
                DSWW_FDATA_WORK.Columns.Add("DSW_DD").MaxLength = 2;//18
                DSWW_FDATA_WORK.Columns.Add("DSW_ROOZ").MaxLength = 12;//19
                DSWW_FDATA_WORK.Columns.Add("DSW_MAH").MaxLength = 12;//20
                DSWW_FDATA_WORK.Columns.Add("DSW_MAZ").MaxLength = 12;//21
                DSWW_FDATA_WORK.Columns.Add("DSW_MASH").MaxLength = 12;//22
                DSWW_FDATA_WORK.Columns.Add("DSW_TOTL").MaxLength = 12;//23
                DSWW_FDATA_WORK.Columns.Add("DSW_BIME").MaxLength = 12;//24
                DSWW_FDATA_WORK.Columns.Add("DSW_PRATE").MaxLength = 2;//25
                DSWW_FDATA_WORK.Columns.Add("DSW_JOB").MaxLength = 6;//26
                DSWW_FDATA_WORK.Columns.Add("PER_NATCOD").MaxLength = 10;//27

                //DSKKAR00
                DSKK_FDATA_KAR.Columns.Add("DSK_ID").MaxLength = 10;//1
                DSKK_FDATA_KAR.Columns.Add("DSK_NAME").MaxLength = 100;//2
                DSKK_FDATA_KAR.Columns.Add("DSK_FARM").MaxLength = 100;//3
                DSKK_FDATA_KAR.Columns.Add("DSK_ADRS").MaxLength = 100;//4
                DSKK_FDATA_KAR.Columns.Add("DSK_KIND").MaxLength = 1;//5
                DSKK_FDATA_KAR.Columns.Add("DSK_YY").MaxLength = 2;//6
                DSKK_FDATA_KAR.Columns.Add("DSK_MM").MaxLength = 2;//7
                DSKK_FDATA_KAR.Columns.Add("DSK_LISTNO").MaxLength = 12;//8
                DSKK_FDATA_KAR.Columns.Add("DSK_DISC").MaxLength = 100;//9
                DSKK_FDATA_KAR.Columns.Add("DSK_NUM").MaxLength = 5;//10
                DSKK_FDATA_KAR.Columns.Add("DSK_TDD").MaxLength = 6;//11
                DSKK_FDATA_KAR.Columns.Add("DSK_TROOZ").MaxLength = 12;//12
                DSKK_FDATA_KAR.Columns.Add("DSK_TMAH").MaxLength = 12; //13
                DSKK_FDATA_KAR.Columns.Add("DSK_TMAZ").MaxLength = 12;//14
                DSKK_FDATA_KAR.Columns.Add("DSK_TMASH").MaxLength = 12;//15
                DSKK_FDATA_KAR.Columns.Add("DSK_TTOTL").MaxLength = 12;//16
                DSKK_FDATA_KAR.Columns.Add("DSK_TBIME").MaxLength = 12;//17
                DSKK_FDATA_KAR.Columns.Add("DSK_TKOSO").MaxLength = 12;//18
                DSKK_FDATA_KAR.Columns.Add("DSK_BIC").MaxLength = 12;//19
                DSKK_FDATA_KAR.Columns.Add("DSK_RATE").MaxLength = 12;//20
                DSKK_FDATA_KAR.Columns.Add("DSK_PRATE").MaxLength = 2;//21
                DSKK_FDATA_KAR.Columns.Add("DSK_BIMH").MaxLength = 12;//22
                DSKK_FDATA_KAR.Columns.Add("MON_PYM").MaxLength = 3;//23

                SQL_CONN = new SqlConnection($"Data Source = {CL_Generaly.General_DBname}; Initial Catalog = {CL_Generaly.General_Servername}; Integrated Security = True;");
                #endregion
                if (!string.IsNullOrEmpty(OPEN_ARGS))
                    THE_PATH_FILES = OPEN_ARGS;


                //string path0 = @"C:\correct\CNR.udl";
                //string pathuspath = File.ReadLines(path0).Last();

                //var DataSource = "Data Source =" + GetBetweenStr(pathuspath, "Data Source =", ";") + ";";
                //var InitialCatalog = "Initial Catalog =" + GetBetweenStr(pathuspath, "Initial Catalog =", ";") + ";";
                //var IntegratedSecurity = "Integrated Security =" + GetBetweenStr(pathuspath, "Integrated Security =", ";") + ";";

                //var FinalCNN = DataSource + InitialCatalog + IntegratedSecurity;
                //SQL_CONN = new SqlConnection(FinalCNN);
                SQL_CONN.Open();

                DSW_PATH = THE_PATH_FILES + DSKWOR00;
                DSK_PATH = THE_PATH_FILES + DSKKAR00;

                #region LOAD_DATA_AND_SET

                SQL_CMD = new SqlCommand("SELECT * FROM dbo.DSKWOR00 ", SQL_CONN);
                DSKW_SQL_WORK.Load(SQL_CMD.ExecuteReader());
                //_______________________________________________________________________________
                SQL_CMD = new SqlCommand("SELECT * FROM dbo.DSKKAR00 ", SQL_CONN);
                DSKK_SQL_KAR.Load(SQL_CMD.ExecuteReader());
                SQL_CONN.Close();

                for (int k = 0; k < DSKW_SQL_WORK.Rows.Count; k++)
                {
                    DataRow dr;
                    dr = DSWW_FDATA_WORK.NewRow();
                    dr["DSW_ID"] = DSKW_SQL_WORK.Rows[k]["DSW_ID"];
                    dr["DSW_YY"] = DSKW_SQL_WORK.Rows[k]["DSW_YY"].ToString().Substring(2, 2);
                    dr["DSW_MM"] = DSKW_SQL_WORK.Rows[k]["DSW_MM"];
                    dr["DSW_LISTNO"] = DSKW_SQL_WORK.Rows[k]["DSW_LISTNO"];
                    dr["DSW_ID1"] = DSKW_SQL_WORK.Rows[k]["DSW_ID1"];
                    dr["DSW_FNAME"] = DSKW_SQL_WORK.Rows[k]["DSW_FNAME"];
                    dr["DSW_LNAME"] = DSKW_SQL_WORK.Rows[k]["DSW_LNAME"];
                    dr["DSW_DNAME"] = DSKW_SQL_WORK.Rows[k]["DSW_DNAME"];
                    dr["DSW_IDNO"] = DSKW_SQL_WORK.Rows[k]["DSW_IDNO"];
                    dr["DSW_IDPLC"] = DSKW_SQL_WORK.Rows[k]["DSW_IDPLC"];
                    dr["DSW_IDATE"] = DSKW_SQL_WORK.Rows[k]["DSW_IDATE"];
                    dr["DSW_BDATE"] = DSKW_SQL_WORK.Rows[k]["DSW_BDATE"];
                    dr["DSW_SEX"] = DSKW_SQL_WORK.Rows[k]["DSW_SEX"];
                    dr["DSW_NAT"] = DSKW_SQL_WORK.Rows[k]["DSW_NAT"];
                    dr["DSW_OCP"] = DSKW_SQL_WORK.Rows[k]["DSW_OCP"];
                    dr["DSW_SDATE"] = DSKW_SQL_WORK.Rows[k]["DSW_SDATE"];
                    dr["DSW_EDATE"] = DSKW_SQL_WORK.Rows[k]["DSW_EDATE"];
                    dr["DSW_DD"] = DSKW_SQL_WORK.Rows[k]["DSW_DD"];
                    dr["DSW_ROOZ"] = DSKW_SQL_WORK.Rows[k]["DSW_ROOZ"];
                    dr["DSW_MAH"] = DSKW_SQL_WORK.Rows[k]["DSW_MAH"];
                    dr["DSW_MAZ"] = DSKW_SQL_WORK.Rows[k]["DSW_MAZ"];
                    dr["DSW_MASH"] = DSKW_SQL_WORK.Rows[k]["DSW_MASH"];
                    dr["DSW_TOTL"] = DSKW_SQL_WORK.Rows[k]["DSW_TOTL"];
                    dr["DSW_BIME"] = DSKW_SQL_WORK.Rows[k]["DSW_BIME"];
                    dr["DSW_PRATE"] = DSKW_SQL_WORK.Rows[k]["DSW_PRATE"];
                    dr["DSW_JOB"] = DSKW_SQL_WORK.Rows[k]["DSW_JOB"];
                    dr["PER_NATCOD"] = DSKW_SQL_WORK.Rows[k]["PER_NATCOD"];

                    DSWW_FDATA_WORK.Rows.Add(dr);
                }

                for (int k = 0; k < DSKK_SQL_KAR.Rows.Count; k++)
                {
                    DataRow dr;
                    dr = DSKK_FDATA_KAR.NewRow();
                    dr["DSK_ID"] = DSKK_SQL_KAR.Rows[k]["DSK_ID"];
                    dr["DSK_NAME"] = DSKK_SQL_KAR.Rows[k]["DSK_NAME"];
                    dr["DSK_FARM"] = DSKK_SQL_KAR.Rows[k]["DSK_FARM"];
                    dr["DSK_ADRS"] = DSKK_SQL_KAR.Rows[k]["DSK_ADRS"];
                    dr["DSK_KIND"] = DSKK_SQL_KAR.Rows[k]["DSK_KIND"];
                    dr["DSK_YY"] = DSKK_SQL_KAR.Rows[k]["DSK_YY"].ToString().Substring(2, 2);
                    dr["DSK_MM"] = DSKK_SQL_KAR.Rows[k]["DSK_MM"];
                    dr["DSK_LISTNO"] = DSKK_SQL_KAR.Rows[k]["DSK_LISTNO"];
                    dr["DSK_DISC"] = DSKK_SQL_KAR.Rows[k]["DSK_DISC"];
                    dr["DSK_NUM"] = DSKK_SQL_KAR.Rows[k]["DSK_NUM"];
                    dr["DSK_TDD"] = DSKK_SQL_KAR.Rows[k]["DSK_TDD"];
                    dr["DSK_TROOZ"] = DSKK_SQL_KAR.Rows[k]["DSK_TROOZ"];
                    dr["DSK_TMAH"] = DSKK_SQL_KAR.Rows[k]["DSK_TMAH"];
                    dr["DSK_TMAZ"] = DSKK_SQL_KAR.Rows[k]["DSK_TMAZ"];
                    dr["DSK_TMASH"] = DSKK_SQL_KAR.Rows[k]["DSK_TMASH"];
                    dr["DSK_TTOTL"] = DSKK_SQL_KAR.Rows[k]["DSK_TTOTL"];
                    dr["DSK_TBIME"] = DSKK_SQL_KAR.Rows[k]["DSK_TBIME"];
                    dr["DSK_TKOSO"] = DSKK_SQL_KAR.Rows[k]["DSK_TKOSO"];
                    dr["DSK_BIC"] = DSKK_SQL_KAR.Rows[k]["DSK_BIC"];
                    dr["DSK_RATE"] = DSKK_SQL_KAR.Rows[k]["DSK_RATE"];
                    dr["DSK_PRATE"] = DSKK_SQL_KAR.Rows[k]["DSK_PRATE"];
                    dr["DSK_BIMH"] = DSKK_SQL_KAR.Rows[k]["DSK_BIMH"];
                    dr["MON_PYM"] = DSKK_SQL_KAR.Rows[k]["MON_PYM"];

                    DSKK_FDATA_KAR.Rows.Add(dr);
                }

                //Simulate Overwrtie
                try
                {
                    File.Delete(DSW_PATH);
                    File.Delete(DSK_PATH);
                }
                catch (Exception) { }

                Dbf.DbfFile.Write(DSW_PATH, DSWW_FDATA_WORK, Encoding.GetEncoding(1256), true, IsIransystemformat);
                Dbf.DbfFile.Write(DSK_PATH, DSKK_FDATA_KAR, Encoding.GetEncoding(1256), true, IsIransystemformat);
                #endregion
            }
            catch (Exception er)
            {
                string filePath = @"C:\er.txt";
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine("Message :" + er.Message + "<br/>" + Environment.NewLine + "StackTrace :" + er.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                return;
            }
        }

        private void TEXTBOXPATH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NowIsReady)
            {
                if (CL_LMethods.IsValidPath(TEXTBOXPATH.Text.Trim()) is true && !(TEXTBOXPATH.Text.Trim().ToUpper() is @"C:\"))
                {
                    Command13.IsEnabled = true;
                }
                else
                {
                    Command13.IsEnabled = false;
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
    }
}
