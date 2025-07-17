using MaterialDesignThemes.Wpf;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
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
using static Prg_UI.Functions.PublicVRB;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    /// <summary>
    /// Interaction logic for BUDGET2.xaml
    /// </summary>
    public partial class BUDGET2 : Window
    {
        bool Winloadis_Completed = false;
        string NTDZBMCHDABD = "";
        string MAHLIST;
        long XXD;
        long XXM;
        long XXI;
        long XXS;
        long MH;
        long mah;
        long SAL;
        string sh;
        long I;
        long TEDAD;
        string rptname;
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public BUDGET2()
        {
            InitializeComponent();
            Owner = PublicVRB.BUDG_1;

        }

        #region Header Window Begin
        //Header Window Begin
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Winloadis_Completed = true;
        }
        private async void Command4_Click(object sender, RoutedEventArgs e)
        {
            //StartThink();
            await Go_For_BDG3();
        }
        private void RemovalQotTex()
        {
            PublicVRB.DoingDeSeprate = true;
            foreach (UIElement control in mygrid.Children)
            {
                if (control is TextBox)
                {
                    if (!string.IsNullOrEmpty(((TextBox)control).Text))
                    {
                        ((TextBox)control).Text = ((TextBox)control).Text.Replace(",", "");
                    }
                }
            }
            PublicVRB.DoingDeSeprate = false;
        }
        //private void StartThink()
        //{
        //    myg.Visibility = Visibility.Hidden;
        //    myg.Visibility = Visibility.Visible;
        //    var controller = ImageBehavior.GetAnimationController(this.myg);
        //    controller.Play();
        //}
        //private void StopThink()
        //{
        //    myg.Visibility = Visibility.Visible;
        //    myg.Visibility = Visibility.Hidden;
        //    var controller = ImageBehavior.GetAnimationController(this.myg);
        //    controller.Pause();
        //}
        private long Saly()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                SAL = SAL;
            }));
            return SAL;
        }
        private long Mahy()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                mah = mah;
            }));
            return mah;
        }
        private async Task Go_For_BDG3()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    RemovalQotTex();
                }));
                var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").ToList();
                foreach (var item in quer_RST)
                {
                    XXD = Convert.ToInt64(CL_HESABDARI.UDay(item.BGCDATE.ToString()));
                    XXM = Convert.ToInt64(CL_HESABDARI.UMonth(item.BGCDATE.ToString()));
                    mah = Convert.ToInt64(item.BGMAH);
                    SAL = Convert.ToInt64(item.BGSAL);
                    PublicVRB.BGMAH = mah;
                    PublicVRB.BGSAL = SAL;
                    XXS = Convert.ToInt64(CL_HESABDARI.UYear(item.BGCDATE.ToString()));
                    XXI = (SAL - XXS) * 12 + mah - XXM;
                    NTDZBMCHDABD = item.NTDZBMCHDABD;
                }
                MH = XXM;

                //Check Matter
                if (NTDZBMCHDABD == ("NTDZBMCHDABD0.50%"))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        CL_HESABDARI.NTDZBMCHDABD050(PublicVRB.GenetalBGID);
                    }));
                }
                //recordset.Requery
                //#Check
                quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").ToList();
                //Budget2 bdgt2 = new Budget2();

                double BGPCASH = 0;
                foreach (var item in quer_RST)
                {
                    BGPCASH = item.BGPCASH;
                }
                /////////
                TextBox BGFOCN_texb = null;
                double BGFOCN_MON = 0;
                //LAST POINT 12
                Dispatcher.Invoke(new Action(() =>
                {
                    BGFOCN_texb = (TextBox)this.FindName("BGFOCN" + XXI + "MON");
                    BGFOCN_MON = Convert.ToDouble(BGFOCN_texb.Text);
                }));
                //#Check
                ////////
                ///////LAST POINT 6
                double ForQre_BG = BGFOCN_MON * BGPCASH / 100;

                var quer_RST_Fields = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGMBCASH = " + ForQre_BG + " WHERE BGID = " + PublicVRB.GenetalBGID);//Check

                byte rr = 1;
                for (int I = 0; I <= XXI; I++)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (rr <= XXI)
                        {
                            TextBox BGFOCNMON = null;
                            BGFOCNMON = (TextBox)this.FindName("BGFOCN" + rr + "MON");
                            double BGFOCN_MON_val = Convert.ToDouble(BGFOCNMON.Text);//#Check
                            var quer_sv3 = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET " + BGFOCNMON.Name + " = " + BGFOCN_MON_val + " , BGMBDARAM = " + this.BGMBDARAM.Text + "  WHERE BGID = " + PublicVRB.GenetalBGID);//
                            rr++;
                        }
                    }));


                    double BGMBCHEKJM_I = 0;
                    double BGPCH_I = 0;

                    //var result = quer_RST.Select(s => new { Line = I++, Nav = "BGMBCHEKJM" + I });
                    //foreach (var it in result)
                    //{
                    //    var quer_sv1 = dbms.Database.ExecuteSqlCommand("UPDATE BUGET_MAIN SET " + it.Nav + " = " + BGMBCHEKJM_I + "  WHERE BGID = " + PublicVRB.GenetalBGID);//#Check
                    //}
                    //POINT 10
                    foreach (dynamic itt in quer_RST)
                    {
                        var BGMBCHEK_Wich = itt.GetType().GetProperty("BGMBCHEKJM" + I).GetValue(itt);
                        var BGPCH_Wich = itt.GetType().GetProperty("BGPCH" + I).GetValue(itt);
                        BGMBCHEKJM_I = Convert.ToDouble(BGMBCHEK_Wich);


                        BGPCH_I = Convert.ToDouble(BGPCH_Wich);
                    }
                    //Matter Check Round(RST.Fields("BGMBCHEKJM" & i) * RST.Fields("BGPCH" & i) / 100)
                    double ForQre_BGJM = BGMBCHEKJM_I * BGPCH_I / 100;   //Change in Thursday, July 22, 2021
                    ForQre_BGJM = Math.Round(ForQre_BGJM);
                    var quer_RST_Fields2 = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGMBCHEK" + I + " = " + ForQre_BGJM + " WHERE BGID = " + PublicVRB.GenetalBGID);//Check  Change in Thursday, July 22, 2021
                }

                // var quer_RST_Fields3 = dbms.Database.ExecuteSqlCommand("UPDATE BUGET_MAIN SET BGMBCHEKMON = " + PublicVRB.GETBSCHEK(Saly(), Mahy()) + " WHERE BGID = " + PublicVRB.GenetalBGID);
                //Thread thread = new Thread(() =>
                //{
                //});
                //thread.SetApartmentState(ApartmentState.STA);
                //thread.Start();
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    var quer_RST_Fields3 = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGMBCHEKMON = " + CL_HESABDARI.GETBSCHEK(SAL, mah) + " WHERE BGID = " + PublicVRB.GenetalBGID);
                });
                Dispatcher.Invoke(new Action(() =>
                {
                    //StopThink();
                    BUDGET3 bdgt3 = new BUDGET3(); /*Hide();*/ bdgt3.ShowDialog();
                }));
            });
        }
        private void Command20_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PublicVRB.BUDG_1.ShowDialog();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxFormat.FormatStringTexBox(this);
            long[] MAHD = new long[50];
            //ONERROR
            string MAHLIST = "";
            long XXD = 0;
            long XXM = 0;
            long XXS = 0;
            long MH = 0;
            long mah = 0;
            long SAL = 0;
            string SH = "";
            //ONERROR
            long i;
            long TEDAD;
            string rptname;
            //  DoCmd.Maximize
            //BGIDD = 1
            PublicVRB.GenetalBGID = (PublicVRB.GenetalBGID == null ? 1 : PublicVRB.GenetalBGID);

            var quer_RST = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").ToList();
            foreach (var item in quer_RST)
            {
                XXD = Convert.ToInt64(CL_HESABDARI.UDay(item.BGCDATE.ToString()));
                XXM = Convert.ToInt64(CL_HESABDARI.UMonth(item.BGCDATE.ToString()));
                mah = Convert.ToInt64(item.BGMAH);
                SAL = Convert.ToInt64(item.BGSAL);
                //PublicVRB.BGMAH = mah;
                //PublicVRB.BGSAL = SAL;
                XXS = Convert.ToInt64(CL_HESABDARI.UYear(item.BGCDATE.ToString()));
                //XXI = (SAL - XXS) * 12 + mah - XXM;
                //NTDZBMCHDABD = item.NTDZBMCHDABD;
            }
            if (XXM < 7)
            {
                XXD = System.Convert.ToInt64(31 - XXD);
            }
            else
            {
                XXD = System.Convert.ToInt64(30 - XXD);
            }
            if (XXD != 0)
            {
                LX.Text = "فروش  " + System.Convert.ToString(XXD) + "روز مانده از ماه " + PublicVRB.MAHNAME(XXM.ToString());
            }
            SH = " ";
            XXI = System.Convert.ToInt32(SAL - XXS) * 12 + mah - XXM;
            MH = XXM;
            for (i = 1; i <= XXI; i++)
            {
                MAHD[i] = MH;
                MAHLIST = MAHLIST + "," + PublicVRB.MAHNAME(MH.ToString());
                if (MH == 12)
                {
                    MH = 0;
                    SH = " " + System.Convert.ToString(SAL);
                }
                MH++;

                TextBlock TCaption = (TextBlock)this.FindName("L" + i);
                TCaption.Text = "فروش " + PublicVRB.MAHNAME(MH.ToString()) + SH;
                TCaption.Visibility = Visibility.Visible;

                TextBox TBX = (TextBox)FindName("BGFOCN" + i + "MON");
                TBX.Visibility = Visibility.Visible;



                //this["L" + System.Convert.ToString(i)].CAPTION = "فروش " + PublicVRB.MAHNAME(MH.ToString()) + SH;
                //this["BGFOCN" + System.Convert.ToString(i) + "MON"].Visible = true;
            }
            hL1.Text = "برای محاسبه بودجه " + PublicVRB.MAHNAME(mah.ToString()) + ", Mr CORRECT نیاز داره پیش بینیتون رو در مورد فروش ماه های  " + MAHLIST + " رو بدونه...";

            Hesnshow.Text = PublicVRB.HesabCombobText;

            if (BUDGET1.EditOldBDG == true)
            {
                //var QreBT_DEFULT = dbms.BUGET_MAIN.Select(x => x);
                var QreBT_DEFULT = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT * FROM BUGET_MAIN").ToList();
                foreach (dynamic BDT_DEF_ROW in QreBT_DEFULT)
                {
                    //#So Very Cool Put Value of Row of SQL where Control is like that name
                    foreach (UIElement control in mygrid.Children)
                    {
                        if (control is TextBox)
                        {
                            if (BDT_DEF_ROW.GetType()?.GetProperty(((TextBox)control).Name)?.Name != null)
                            {
                                if (((TextBox)control).Name == (BDT_DEF_ROW.GetType().GetProperty(((TextBox)control).Name).Name))
                                {
                                    ((TextBox)control).Text = BDT_DEF_ROW.GetType().GetProperty(((TextBox)control).Name).GetValue(BDT_DEF_ROW).ToString();
                                }
                            }
                        }
                    }
                }
            }
            CL_LMethods.UiRefresher.RefWhole(mygrid);
            (PublicVRB.BUDG_2).UpdateLayout();
        }
        private void TexDefaltor()
        {
            TexChartor.Zero_ifisEmpt(BGFOCXDAY);
            TexChartor.Del_ZeroFirst(BGFOCXDAY);

        }
        private void BGFOCXDAY_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
