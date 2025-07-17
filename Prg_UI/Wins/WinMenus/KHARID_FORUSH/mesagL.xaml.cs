using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class mesagL : Window
    {
        //public ObservableCollection<DEPATMAN_MODEL> DEPATMAN_COMBO_DATA { get; set; }
        //public ObservableCollection<SHIFT_MODEL> SHIFT_COMBO_DATA { get; set; }
        //public ObservableCollection<CUST_KIND_MODEL> CUST_KIND_COMBO_DATA { get; set; }
        //public ObservableCollection<BANK_MODEL> BANK_COMBO_DATA { get; set; }
        //public ObservableCollection<HES1_MODEL> HES1_COMBO_DATA { get; set; }
        //public ObservableCollection<HES1_MODEL> HES2_COMBO_DATA { get; set; }
        //public ObservableCollection<HES1_MODEL> HES3_COMBO_DATA { get; set; }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion

        public string RecordSource_grade_pish { get; set; }
        string HES = "";
        long NUM;
        long DT_HEADLST;
        byte OpenArgs = 1;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public Visual THE_WIN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hes">CUST_NO_HESAB</param>
        /// <param name="_invoiceNumber">NUMBER FACTOR OR PISHFACTOR</param>
        /// <param name="_dt">DATE_WOTHOUT SLASH</param>
        /// <param name="_openargs">آیا دکمه بررسشی وضعیت بوده یا تایید پیش فاکتور که پیش فرض بررسی وضعیت می باشد</param>
        public mesagL(string _hes, long _invoiceNumber, long _dt = 0, byte _openargs = 1, Visual tHE_WIN = null)
        {
            HES = _hes;
            NUM = _invoiceNumber;
            DT_HEADLST = _dt;
            OpenArgs = _openargs;
            InitializeComponent();
            this.DataContext = this;
            THE_WIN = tHE_WIN;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();

            Form_Open();
            OnLoadForm();
            OpenDetailForm();

            ProcLoader.Stop(Prc);
        }

        private void Form_Open()
        {
            int GRD;
            var GRDV = default(int);
            int i, ETB;
            bool OKETEB;
            bool enhesar;
            bool OKETEBT;
            bool OKETEBf;
            bool OKETEBfT;
            var FC = default(int);
            int si;
            string FUNC;
            double MANDT;
            string VHEST;
            double MANDTK;
            var PAYAM = default(string);
            //if (Strings.Right(this.OpenArgs, 2) == "OK")
            //{
            //    this.MSG.ForeColor = 6723891;
            //}
            enhesar = false;

            VHEST = CL_HESABDARI.GetVisitHES(HES);

            if (!string.IsNullOrEmpty(HES)) //if (!ISNULL(Forms["head_lst_pishfroosh2"].Form.CUST_NO))
            {
                //Forms["head_lst_pishfroosh2"].Form.NUMBER = NUM
                //if (CL_HESABDARI.ChekEnheSar(Forms["head_lst_pishfroosh2"].Form.NUMBER, 20))
                if (Convert.ToBoolean(CL_HESABDARI.ChekEnheSar(NUM, 20)))
                {
                    CL_HESABDARI.logetebar(VHEST, NUM, 20, 0, 0, "انحصار", "0");
                    enhesar = true;
                }

                ////////////MANDT = CL_HESABDARI.GetMhesab(Forms["head_lst_pishfroosh2"].Form.CUST_NO); ===== HES
                MANDT = CL_HESABDARI.GetMhesab(HES);
                ///////Forms["BUN"].Form.Refresh();
                ///////Forms["BUN"].Form.Refresh();
                MANDTK = CL_HESABDARI.GetMhesabF(VHEST);
                //////Forms["BUN"].Form.Refresh();
                GRDV = CL_HESABDARI.GetGrade(VHEST);
                ////Forms["BUN"].Form.Refresh();
                GRD = CL_HESABDARI.GetGrade(HES);
                ////Forms["BUN"].Form.Refresh();
                ///////MNAM.ControlSource = "=" + '"' + CL_HESABDARI.GETHESNAME(HES) + '"';
                MNAM.Text = CL_HESABDARI.GETHESNAME(HES);
                ////Forms["BUN"].Form.Refresh();
                KNAM.Text = CL_HESABDARI.GETHESNAME(VHEST);
                /////Forms["BUN"].Form.Refresh();
                if (MANDT == 0d)
                {
                    mhesab.Text = "0 ريال";
                }
                else
                {// + ((char)(34)).ToString()
                    this.mhesab.Text = ((MANDT > 0) ? MANDT.ToString("##,# ريال بدهكار") : (MANDT * -1).ToString("##,# ريال بستانكار"));
                    //mhesab.Text = Operators.ConcatenateObject(Operators.ConcatenateObject("=" + '"', Interaction.IIf(MANDT > 0d, Strings.Format((object)MANDT, "##,# ريال بدهكار"), Strings.Format((object)(MANDT * (double)-1), "##,# ريال بستانكار"))), '"');
                }

                if (MANDTK == 0d)
                {
                    khesab.Text = "0 ريال";
                }
                else
                {
                    this.khesab.Text = ((MANDTK > 0) ? MANDTK.ToString("##,# ريال بدهكار") : (MANDTK * -1).ToString("##,# ريال بستانكار"));
                    //this.khesab.ControlSource = Operators.ConcatenateObject(Operators.ConcatenateObject("=" + '"', Interaction.IIf(MANDTK > 0d, Strings.Format((object)MANDTK, "##,# ريال بدهكار"), Strings.Format((object)(MANDTK * (double)-1), "##,# ريال بستانكار"))), '"');
                }

                ////////Forms["BUN"].Form.Refresh();
                OKETEB = true;
                OKETEBT = true;
                OKETEBf = true;
                OKETEBfT = true;
                //// ثبت موقت تاييديه براي محاسبه در جمع کل پيش فاکتورها
                //// si = [Forms]![head_lst_pishfroosh2].[Form].[SGN1]
                //// DoCmd.RunSQL "update head_lst set SGN1 = 1 where tag = 20 and number  = " & [Forms]![head_lst_pishfroosh2].[Form].[NUMBER]


                var RST = dbms.DoGetDataSQL<GRADE_1>("SELECT     dbo.GRADE_SHART.GID, dbo.GRADE_SHART_FUNC.GSHARTID, dbo.GRADE_SHART_FUNC.GSHNAME, dbo.GRADE_SHART_FUNC.GSHFUNC,  dbo.GRADE_SHART.GSVALUE ,  dbo.GRADE_SHART.GSVALUE2 , dbo.GRADE_SHART.TOZI, dbo.GRADE_SHART_FUNC.TOZIH FROM  dbo.GRADE_SHART INNER JOIN  dbo.GRADE_SHART_FUNC ON dbo.GRADE_SHART.GSHARTID = dbo.GRADE_SHART_FUNC.GSHARTID WHERE     (dbo.GRADE_SHART.GID = " + GRD + ")").ToList();
                var loopTo = RST.Count;
                ///////////for (i = 1; i <= loopTo; i++)
                for (i = 0; i < loopTo; i++)
                {
                    /////Forms["BUN"].Form.Refresh();
                    FUNC = RST[i].GSHFUNC;
                    switch (FUNC ?? "")
                    {
                        case "ChecketebarSSM":
                            {
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.ChecketebarSSM(HES, Convert.ToDouble(RST[i].GSVALUE), NUM));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + " - ";  //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBT = false;
                                }

                                break;
                            }

                        case "ChecketebarASSSM":
                            {
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.ChecketebarASSSM(HES.ToString(), Convert.ToDouble(RST[i].GSVALUE), NUM));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + '\r'; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBT = false;
                                }

                                break;
                            }

                        case "MandTop10":
                            {
                                //Forms["head_lst_pishfroosh2"].Form.DATE_N ==== FullCurrentDate
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.MandTop10(HES, DT_HEADLST, Convert.ToDouble(RST[i].GSVALUE), Convert.ToDouble(RST[i].GSVALUE2)));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + " - ";  //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBT = false;
                                }

                                break;
                            }

                        case "MandLes10":
                            {
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.MandLes10(HES, DT_HEADLST, (double)RST[i].GSVALUE, Convert.ToDouble(RST[i].GSVALUE2)));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + '\r'; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBT = false;
                                }

                                break;
                            }

                        case "FactCount":
                            {
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.FactCount(HES, (double)RST[i].GSVALUE, ref FC));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + "  بيشتر از حد مجاز است تعداد تسويه نشده : " + FC + " - ";
                                    OKETEBT = false;
                                }

                                break;
                            }

                        case "ChekBrgashti":
                            {
                                OKETEB = Convert.ToBoolean(CL_HESABDARI.ChekBrgashti(HES));
                                if (!OKETEB)
                                {
                                    PAYAM = PAYAM + RST[i].GSHNAME + '\r';
                                    OKETEBT = false;
                                }

                                break;
                            }
                    }
                    CL_HESABDARI.logetebar(HES, NUM, 20, (double)RST[i].GSVALUE, GRD, PAYAM, RST[i].GSHNAME);
                    //////////////RST.MoveNext();
                };
                var RSTV = dbms.DoGetDataSQL<GRADEQRE2>("SELECT     dbo.GRADE_SHART.GID, dbo.GRADE_SHART_FUNC.GSHARTID, dbo.GRADE_SHART_FUNC.GSHNAME, dbo.GRADE_SHART_FUNC.GSHFUNC,  dbo.GRADE_SHART.GSVALUE ,  dbo.GRADE_SHART.GSVALUE2 , dbo.GRADE_SHART.TOZI, dbo.GRADE_SHART_FUNC.TOZIH FROM  dbo.GRADE_SHART INNER JOIN  dbo.GRADE_SHART_FUNC ON dbo.GRADE_SHART.GSHARTID = dbo.GRADE_SHART_FUNC.GSHARTID WHERE     (dbo.GRADE_SHART.GID = " + GRDV + ")").ToList();
                var loopTo1 = RSTV.Count - 1 < 0 ? RSTV.Count : 0;
                for (i = 1; i <= loopTo1; i++)
                {
                    //////////Forms["BUN"].Form.Refresh();
                    FUNC = RSTV[i].GSHFUNC;
                    switch (FUNC ?? "")
                    {
                        case "ChecketebarSSMF":
                            {
                                OKETEBf = Convert.ToBoolean(CL_HESABDARI.ChecketebarSSMF(VHEST, (double)RSTV[i].GSVALUE, NUM));
                                if (!OKETEBf)
                                {
                                    PAYAM = PAYAM + RSTV[i].GSHNAME + " - "; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده مشتريان ويزيتور*****" & Chr(13)
                                    OKETEBfT = false;
                                }

                                break;
                            }

                        case "ChecketebarASSSM":
                            {
                                OKETEBf = Convert.ToBoolean(CL_HESABDARI.ChecketebarASSSM(VHEST, (double)RSTV[i].GSVALUE, NUM));
                                if (!OKETEBf)
                                {
                                    PAYAM = PAYAM + RSTV[i].GSHNAME + '\r'; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBfT = false;
                                }

                                break;
                            }

                        case "MandTop10":
                            {
                                OKETEBf = Convert.ToBoolean(CL_HESABDARI.MandTop10(VHEST, DT_HEADLST, (double)RSTV[i].GSVALUE, (double)RSTV[i].GSVALUE2));
                                if (!OKETEBf)
                                {
                                    PAYAM = PAYAM + RSTV[i].GSHNAME + " - "; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBfT = false;
                                }

                                break;
                            }
                        ////مانده کمتر از 10 میلیون تومن ماه قبل باید تا 10 هم ماه بعد تویه شده باشد
                        case "MandLes10":
                            {
                                OKETEBf = Convert.ToBoolean(CL_HESABDARI.MandLes10(VHEST, DT_HEADLST, (double)RSTV[i].GSVALUE, (double)RSTV[i].GSVALUE2));
                                if (!OKETEBf)
                                {
                                    PAYAM = PAYAM + RSTV[i].GSHNAME + '\r'; //// & " مانده بدهي شامل بدهي دفتري به اضافه حواله هاي بارگيري نشده به اضافه پيش فاکتور هاي تاييد شده که تبديل به حواله نشده يا کنسل نشده" & Chr(13)
                                    OKETEBfT = false;
                                }

                                break;
                            }

                        case "FactCount":
                            {
                                OKETEBf = Convert.ToBoolean(CL_HESABDARI.FactCount(VHEST, (double)RSTV[i].GSVALUE, ref FC));
                                if (!OKETEBf)
                                {
                                    PAYAM = PAYAM + RSTV[i].GSHNAME + "  بيشتر از حد مجاز است تعداد تسويه نشده : " + FC + '\r';
                                    OKETEBfT = false;
                                }

                                break;
                            }
                    }

                    CL_HESABDARI.logetebar(VHEST, NUM, 20, (double)RSTV[i].GSVALUE, GRDV, PAYAM, RSTV[i].GSHNAME);
                    ////////RSTV.MoveNext();i++;
                }

                if (OKETEBfT && RSTV.Count > 0)
                {
                    this.OKK.Visibility = Visibility.Visible;////true
                    this.NOKK.Visibility = Visibility.Hidden;/////false
                }
                else
                {
                    this.OKK.Visibility = Visibility.Hidden;
                    this.NOKK.Visibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(PAYAM))
                    {
                        PAYAM = "اين کارشناس دارداي گريد بندي نيست و نميتوانم تاييد کنم";
                    }
                }

                ////////Forms["BUN"].Form.Refresh();
                if (OKETEBT && RST.Count > 0)
                {
                    this.OKM.Visibility = Visibility.Visible;
                    this.NOKM.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.OKM.Visibility = Visibility.Hidden;
                    this.NOKM.Visibility = Visibility.Visible;
                    if (string.IsNullOrEmpty(PAYAM))
                    {
                        PAYAM = "اين مشتري دارداي گريد بندي نيست و نميتوانم تاييد کنم";
                    }
                }
            }


            //// این بخش میاد اطلاعاتی که در ظاهر نمایش میده که در کد نبوده رو قرار میده
            grdn.Text = CL_HESABDARI.GetGradeName(CL_HESABDARI.GetGrade(HES)); //گرید مشتری
            mpish.Text = CL_HESABDARI.GetMPishf(HES).ToString("##,#"); //مانده پیش فاکتور های تایید شده
            mhav.Text = CL_HESABDARI.GetMHavl(HES).ToString("##,#"); //جمع حواله های بارگیری نشده
            masnad.Text = CL_HESABDARI.GetMAsnad(HES).ToString("##,#");

            Text72.Text = CL_HESABDARI.GetGradeName(CL_HESABDARI.GetGrade(VHEST)); //گرید کارشناس
            kpish.Text = CL_HESABDARI.GetMPishfF(VHEST).ToString("##,#"); //مانده پیش فاکتور های تایید شده
            khav.Text = CL_HESABDARI.GetMHavlF(VHEST).ToString("##,#"); //جمع حواله های بارگیری نشده
            kasnad.Text = CL_HESABDARI.GetMAsnadF(VHEST).ToString("##,#");

            /////Forms["BUN"].Form.Refresh();
            this.MSG.Content = PAYAM;
            if (OpenArgs == 1)
            {
                return;
            }
            //// DoCmd.RunSQL "update head_lst set SGN1 =" & si & " where tag = 20 and number  = " & [Forms]![head_lst_pishfroosh2].[Form].[NUMBER]
            ///else if ↓ at first of below line was in comment if need
            if (this.OKK.Visibility == Visibility.Visible && this.OKM.Visibility == Visibility.Visible && !enhesar)
            {

                (THE_WIN as HEAD_LST_PISHFROOSH2).SGN1.IsChecked = true;
                (THE_WIN as HEAD_LST_PISHFROOSH2).SGN2.IsChecked = true;
                ////Forms["head_lst_pishfroosh2"].Form.SGN1 = 1;
                ////Forms["head_lst_pishfroosh2"].Form.SGN2 = 1;
                dbms.DoExecuteSQL("update head_lst set SGN1 =1 ,  SGN2 = 1 where tag = 20 and number  = " + NUM);
                CL_HESABDARI.logetebar(VHEST, NUM, 20, 1, GRDV, "پيش فاکتور تاييد شد", "پيش فاکتور تاييد شد");
                Msgwin msgwin = new Msgwin(false, "پیش فاکتور تایید شد", "#FF000000"); msgwin.ShowDialog();
                ////DoCmd.OpenForm("mesag", acNormal, default, default, acFormReadOnly, acDialog, "پيش فاکتور تاييد شد ");
            }
            else
            {
                (THE_WIN as HEAD_LST_PISHFROOSH2).SGN1.IsChecked = false;
                (THE_WIN as HEAD_LST_PISHFROOSH2).SGN2.IsChecked = false;
                ////Forms["head_lst_pishfroosh2"].Form.SGN1 = 0;
                ////Forms["head_lst_pishfroosh2"].Form.SGN2 = 0;
                dbms.DoExecuteSQL("update head_lst set SGN1 = 0 ,  SGN2 = 0 where tag = 20 and number  = " + NUM);
            }




            /////DoCmd.Close(acForm, "bun");
        }

        private void OnLoadForm()
        {
            long BKC = 0;
            string fld = null;
            fld = "Detail_BackColor";
            ///////BKC = this.Detail.BackColor;
            BKC = Convert.ToInt32(CL_HESABDARI.GETFLDvlu(Convert.ToInt32(Baseknow.USERCOD), this.GetType().Name, fld, BKC));
            //////this.Detail.BackColor = (BKC == -1) ? this.Detail.BackColor : BKC;
            /////////UISetRoundRect(this, 20, false);


        }

        private void OpenDetailForm()
        {
            //SqlConnection con = new SqlConnection(PublicVRB.Put_CnnSti());
            //con.Open();
            //SqlCommand cmd = new SqlCommand($@"SELECT        SUM(dbo.INVO_LST.MABL_K - dbo.INVO_LST.N_MOIN + dbo.INVO_LST.IMBAA) AS مبلغ, dbo.HEAD_LST.NUMBER AS شماره, dbo.HEAD_LST.DATE_N AS تاریخ, dbo.HEAD_LST.CUST_NO AS مشتری, dbo.HEAD_LST.MOLAH AS ملاحظات,
            //                                                          dbo.DEPART.DEPNAME AS واحد, dbo.SHIFT.SHNAME AS شیفت, dbo.CUSTKIND.CUSTKNAME AS [نوع مشتری], dbo.HEAD_LST.USER_NAME AS [نام کاربری], dbo.HEAD_LST.TAMIR AS وضعیت
            //                                FROM            dbo.HEAD_LST INNER JOIN
            //                                                         dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG INNER JOIN
            //                                                         dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD INNER JOIN
            //                                                         dbo.DEPART ON dbo.HEAD_LST.DEPATMAN = dbo.DEPART.DEPATMAN INNER JOIN
            //                                                         dbo.SHIFT ON dbo.HEAD_LST.SHIFT = dbo.SHIFT.SHIFT_ID
            //                                WHERE        (dbo.HEAD_LST.TAG = 20) AND (dbo.HEAD_LST.TAMIR <> 2) AND (dbo.HEAD_LST.TAMIR <> 3) AND (dbo.HEAD_LST.SGN1 = 1)
            //                                GROUP BY dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.TAMIR, dbo.CUSTKIND.CUSTKNAME, dbo.DEPART.DEPNAME, 
            //                                                         dbo.SHIFT.SHNAME", con);
            //SqlDataAdapter sda = new SqlDataAdapter(cmd);
            //DataSet ds = new DataSet();
            //sda.Fill(ds);
            //if (ds.Tables[0].Rows.Count > 0)
            //{
            //    grade_pish.ItemsSource = ds.Tables[0].DefaultView;
            //}
            //con.Close();

            //پر کردن کمبوباکس ها
            #region COMBOXING
            var QRE_DEPATMAN = dbms.DoGetDataSQL<DEPATMAN_MODEL>("SELECT DEPATMAN, DEPNAME FROM DEPART").ToList();
            DEPARTMAN_COLUMN_1.ItemsSource = QRE_DEPATMAN;
            DEPARTMAN_COLUMN_2.ItemsSource = QRE_DEPATMAN;
            DEPARTMAN_COLUMN_3.ItemsSource = QRE_DEPATMAN;
            DEPARTMAN_COLUMN_4.ItemsSource = QRE_DEPATMAN;
            //DEPATMAN_COMBO_DATA = new ObservableCollection<DEPATMAN_MODEL>();
            //foreach (var item in QRE_DEPATMAN)
            //    DEPATMAN_COMBO_DATA.Add(item);

            var QRE_SHIFT = dbms.DoGetDataSQL<SHIFT_MODEL>("SELECT SHIFT_ID, SHNAME FROM SHIFT").ToList();
            SHIFT_COLUMN_1.ItemsSource = QRE_SHIFT;
            SHIFT_COLUMN_2.ItemsSource = QRE_SHIFT;
            SHIFT_COLUMN_3.ItemsSource = QRE_SHIFT;
            SHIFT_COLUMN_4.ItemsSource = QRE_SHIFT;
            //SHIFT_COMBO_DATA = new ObservableCollection<SHIFT_MODEL>();
            //foreach (var item in QRE_SHIFT)
            //    SHIFT_COMBO_DATA.Add(item);

            var QRE_CUST_KIND = dbms.DoGetDataSQL<CUST_KIND_MODEL>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
            CUST_KIND_COLUMN_1.ItemsSource = QRE_CUST_KIND;
            CUST_KIND_COLUMN_2.ItemsSource = QRE_CUST_KIND;
            CUST_KIND_COLUMN_3.ItemsSource = QRE_CUST_KIND;
            CUST_KIND_COLUMN_4.ItemsSource = QRE_CUST_KIND;
            //CUST_KIND_COMBO_DATA = new ObservableCollection<CUST_KIND_MODEL>();
            //foreach (var item in QRE_CUST_KIND)
            //    CUST_KIND_COMBO_DATA.Add(item);

            var QRE_BANK = dbms.DoGetDataSQL<BANK_MODEL>("SELECT CODE, NAMES FROM TCOD_BANKS").ToList();
            BANK_COLUMN.ItemsSource = QRE_BANK;
            //BANK_COMBO_DATA = new ObservableCollection<BANK_MODEL>();
            //foreach (var item in QRE_BANK)
            //    BANK_COMBO_DATA.Add(item);

            var QRE_HES1 = dbms.DoGetDataSQL<HES1_MODEL>("SELECT hes, hes + N' : ' + NAME AS Expr1 FROM CUST_HESAB").ToList();

            //HES1_COMBO_DATA = new ObservableCollection<HES1_MODEL>();
            //foreach (var item in QRE_HES1)
            //    HES1_COMBO_DATA.Add(item);

            #endregion

            //پیش فاکتور های مشتری
            #region grade_pish_one

            //if (PublicVRB.IsLoadedMyWin<Window>("NABZ_MOSHTARI"))
            //{
            //if (IsNull(Forms["NABZ_MOSHTARI"].Form.OpenArgs))
            //{
            //    this.RecordSource = <grade_pishfactor>"SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) AND (HEAD_LST.TAMIR <> 2) AND (HEAD_LST.TAMIR <> 3) AND (HEAD_LST.SGN1 = 1) and   (HEAD_LST.CUST_NO = N'" + HES + "') GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR";
            //}
            //else
            //{
            //    this.RecordSource = <grade_pishfactor>"SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) AND (HEAD_LST.TAMIR <> 2) AND (HEAD_LST.TAMIR <> 3) AND (HEAD_LST.SGN1 = 1) and   (HEAD_LST.CUST_NO = N'" + Forms["NABZ_MOSHTARI"].Form.OpenArgs + "') GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR";
            //}
            //}
            //else
            //{
            grade_pishDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_pish_model>("SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 20) AND (HEAD_LST.TAMIR <> 2) AND (HEAD_LST.TAMIR <> 3) AND (HEAD_LST.SGN1 = 1) and   (HEAD_LST.CUST_NO = N'" + (THE_WIN as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue + "') GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR").ToList();
            //}
            #endregion

            //حواله های مشتری:
            #region grade_havala_two
            grade_havalaDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_havala_arsh_model>("SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = 0)  and   (HEAD_LST.CUST_NO = N'" + (THE_WIN as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue + "') GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR").ToList();
            #endregion

            //پیش فاکتور های کارشناس :
            #region grade_pish_karsh_three
            grade_pish_karshDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_pish_karsh_model>("SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR, CUST_HESAB.NAME, Visit_route.HES FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG INNER JOIN CUST_HESAB ON HEAD_LST.CUST_NO = CUST_HESAB.hes INNER JOIN Visit_route ON CUST_HESAB.ROUTE_NAME = Visit_route.ROUTE_NAME WHERE (HEAD_LST.TAG = 20) AND (HEAD_LST.TAMIR <> 2) AND (HEAD_LST.TAMIR <> 3) AND (HEAD_LST.SGN1 = 1) GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR, CUST_HESAB.NAME, Visit_route.HES HAVING (Visit_route.HES = N'" + CL_HESABDARI.GetVisitHES((THE_WIN as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue.ToString()) + "')").ToList();
            #endregion

            //حواله های کارشناس :
            #region grade_havala_arsh_four
            grade_havala_arshDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_havala_arsh_model>("SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR, CUST_HESAB.NAME, CUST_HESAB.ROUTE_NAME FROM HEAD_LST INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG INNER JOIN CUST_HESAB ON HEAD_LST.CUST_NO = CUST_HESAB.hes INNER JOIN Visit_route ON CUST_HESAB.ROUTE_NAME = Visit_route.ROUTE_NAME WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = 0) AND (Visit_route.HES = N'" + CL_HESABDARI.GetVisitHES((THE_WIN as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue.ToString()) + "') GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR, CUST_HESAB.NAME, CUST_HESAB.ROUTE_NAME").ToList();
            #endregion

            //چکهای برگشتی :
            #region grade_chek_bargashti_five
            grade_chek_bargashtiDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_chek_bargashti_model>("SELECT N_SERI, vaz,BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, HES1, HES2, HES3, VAZ FROM PAY_GETD WHERE (VAZ = 5 OR VAZ = 6) AND (CUST_NO = N'" + (THE_WIN as HEAD_LST_PISHFROOSH2).CUST_NO.SelectedValue + "')").ToList();
            #endregion
        }
    }
}
