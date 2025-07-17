using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    /// <summary>
    /// Interaction logic for BUDGET1.xaml
    /// </summary>
    public partial class BUDGET1 : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public static bool EditOldBDG = false;
        //DENAF1399 ON SQL SERVER 2014
        //DENAF1399Entities dbms = new DENAF1399Entities();
        bool Winloadis_Completed = false;
        string current_salmah = "";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public BUDGET1()
        {
            InitializeComponent();
            //Get Persian Dates #Cool
            DateTime thisDate = DateTime.Now;
            PersianCalendar perscal = new PersianCalendar();
            string mah = (perscal.GetMonth(thisDate)).ToString();
            string rooz = (perscal.GetDayOfMonth(thisDate)).ToString();
            // string dtt = perscal.ToDateTime(thisDate.ToString
            //Make 2 digit if is one without zero at first of that
            if (mah.Length < 2)
            {
                mah = ("0" + mah).ToString();
            }
            if (rooz.Length < 2)
            {
                rooz = ("0" + rooz).ToString();
            }
            bgdate.Text = (perscal.GetYear(thisDate) + "" + mah + "" + rooz).ToString();
            PublicVRB.General_bgdate = (perscal.GetYear(thisDate) + "" + mah + "" + rooz).ToString();
            //برای اینکه سال و ماه فقط باشه تا مقایسه کنم که تاریخ بودجه رو عقب جلو الکی نداده باشن تاعین هم باشه بشه منها کرد و نتیجه درست گرفت
            current_salmah = (perscal.GetYear(thisDate) + "" + mah).ToString();
            //PublicVRB.SalmahNow = (perscal.GetYear(thisDate) + "" + mah).ToString(); Not Usefuld
            PublicVRB.SalDt = perscal.GetYear(thisDate).ToString();
            PublicVRB.MahDt = mah;
            PublicVRB.RoozDt = rooz;
            //Owner = PublicVRB.BUDG_0;
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

        private void TimStar(bool IsStart = false)
        {
            if (IsStart == true)
            {
                dispatcherTimer.Start();
            }
            if (IsStart == false)
            {
                dispatcherTimer.Stop();
            }
        }
        private void Checkup_Fileds()
        {
            if (Winloadis_Completed == true)
            {
                if (TexChartor.TexBisEmptWith_Trimit(SAL) == false && SAL.Text.Trim().Length == 4 && TexChartor.ComboisNotEmpt(MAH) == true)
                {
                    Command4.IsEnabled = true;
                }
                else
                {
                    Pop1Text1.Text = "سال رو چهار رقمی وارد و ماه رو از لیست انتخاب نمایید";
                    Pop1.IsOpen = true;
                    Command4.IsEnabled = false;
                }
            }
        }
        public static int n = 0;
        byte cc = 0;
        private void normingItem()
        {
            MAH.IsDropDownOpen = true;
            MAH.IsDropDownOpen = false;
            /////Matter
            int NextBM = Convert.ToInt32(PublicVRB.MahDt);
            if (NextBM == 12)
            {
                NextBM = NextBM - 1;
            }
            MAH.SelectedIndex = NextBM;
            SAL.Text = PublicVRB.SalDt;
            //string n111111 = ((ComboBoxItem)MAH.SelectedItem).Tag.ToString(); #Cool Tag Combobox
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            //See how is connection to database

        }
        //Select Only BGID 
        public class CHOOSE_BGID
        {
            public Int64 BGID { get; set; }
        }
        private void StartThink()
        {
            imgBudget0.Visibility = Visibility.Hidden;
            //var controller = ImageBehavior.GetAnimationController(this.myg);
            //controller.Play();
        }
        private void StopThink()
        {
            imgBudget0.Visibility = Visibility.Visible;
            //var controller = ImageBehavior.GetAnimationController(this.myg);
            //controller.Pause();
        }
        private void SetDater()
        {
            //lbl_rooz.Content = calenderp.SelectedDate.PersianDayOfWeek;
            //lbl_tarikhrooz.Content = calenderp.SelectedDate.Day;
            //lbl_mah.Content = calenderp.SelectedDate.MonthAsPersianMonth;
            //lbl_sal.Content = calenderp.SelectedDate.Year;

            //string GregorianDate = "Thursday, October 24, 2013";
            //DateTime d = DateTime.Parse(this.t);

            //Console.WriteLine(string.Format("{0}/{1}/{2}", pc.GetYear(d), pc.GetMonth(d), pc.GetDayOfMonth(d)));
        }
        private int MahSelectedInd()
        {
            int Mahindex = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                Mahindex = MAH.SelectedIndex;
            }));
            return Mahindex;
        }
        private string SalTex()
        {
            string salt = "";
            Dispatcher.Invoke(new Action(() =>
            {
                salt = SAL.Text.ToString();
            }));
            return salt;
        }
        private string MahTex()
        {
            string mahtex = "";
            Dispatcher.Invoke(new Action(() =>
            {
                mahtex = MAH.Text.ToString();
            }));
            return mahtex;
        }
        private async void Command4_Click(object sender, RoutedEventArgs e)
        {

            StartThink();
            await Go_For_BDG1();
            StopThink();
            dispatcherTimer.Stop();
        }
        private void StartAnimation()
        {
            Storyboard sb = (Storyboard)FindResource("Scaler_Btn");
            Storyboard sb2 = (Storyboard)FindResource("WinShake");
            //Storyboard.SetTarget(sb, Command4);
            sb.Begin();
            sb2.Begin();
        }
        private void StopAnimation()
        {
            Storyboard sb = (Storyboard)FindResource("Scaler_Btn");
            //Storyboard.SetTarget(sb, Command4);
            sb.Pause();
        }
        private async Task Go_For_BDG1()
        {
            await Task.Run(() =>
            {

                if ((string.IsNullOrEmpty(MahTex()) || MahSelectedInd() == -1) && string.IsNullOrEmpty(SalTex()))
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //Red :#FFFF0000   Black : #FF000000
                        new Msgwin(false, "پارامتر ها کامل نیست لطفا سال و ماه را وارد کنید", "#FFFF0000").ShowDialog();
                    });
                    //MessageBox.Show("پارامتر ها کامل نیست لطفا سال و ماه را وارد کنید");
                    return;
                }
                else
                {
                    //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
                    Process Prc = ProcLoader.Start();


                    var quer_RST = dbms.DoGetDataSQL<CHOOSE_BGID>("SELECT BGID FROM BUGET_MAIN WHERE BGMAH = " + (MahSelectedInd() + 1) + " AND BGSAL = " + SalTex() + " ORDER BY BGID DESC").FirstOrDefault();
                    //var quer_GETLIDD = dbms.DoGetDataSQL<Int32>("SELECT CAST(BGID+1 AS bigint) FROM BUGET_MAIN").First();

                    //NOO:
                    //Go To End of Proccess This ALL
                    string TypMah = (MahSelectedInd() + 1).ToString();
                    if (((MahSelectedInd() + 1).ToString()).Length < 2)
                    {
                        TypMah = ("0" + (MahSelectedInd() + 1)).ToString();
                    }
                    string ValDate = (SalTex() + TypMah).ToString();
                    //int CurrentDate = Convert.ToInt32(PublicVRB.General_bgdate);
                    int CurrentDate = Convert.ToInt32(current_salmah);
                    int TypedDate = Convert.ToInt32(ValDate);
                    if (TypedDate <= CurrentDate)
                    {
                        //LAST POINT 13
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            ProcLoader.Stop(Prc);
                            //Red :#FFFF0000   Black : #FF000000
                            new Msgwin(false, "! بودجه را صرفا برای ماه های بعد میتوان تعریف کرد", "#FFFF0000").ShowDialog();
                        });
                        if (!(Prc is null)) { ProcLoader.Stop(Prc); }
                        return;
                    }
                    else
                    {   //if (quer_RST.Count() > 0) //quer_RST != null
                        if (quer_RST != null)
                        {
                            var _didyes = false;

                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                //Red :#FFFF0000   Black : #FF000000
                                Msgwin msgwin = new Msgwin(true, "برای این ماه و سال قبلا بودجه تعریف شده است همان را اصلاح میکنید؟", "#FF000000");
                                if (msgwin.DialogResult == true)
                                {
                                    _didyes = true;
                                }
                            });
                            //Yes Messgae OK MessageBox.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            if (_didyes == true)
                            {
                                PublicVRB.GenetalBGID = quer_RST.BGID;
                                //foreach (var item in quer_RST)
                                //{
                                //    PublicVRB.GenetalBGID = item.BGID;
                                //}
                                EditOldBDG = true;

                            }
                            else //No Message OK
                            {

                                if (PublicVRB.GenetalBGID == 0)
                                {
                                    PublicVRB.GenetalBGID = CL_HESABDARI.GetLIDD("BUGET_MAIN", "BGID");
                                    //var querInsert_Budget = dbms.DoExecuteSQL($"INSERT INTO BUGET_MAIN VALUES({PublicVRB.GenetalBGID},{PublicVRB.General_bgdate},{(MahSelectedInd() + 1)},{SalTex()},0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,NULL,NULL)");
                                    var querInsert_Budget = dbms.DoExecuteSQL($@"INSERT INTO dbo.BUGET_MAIN(BGID, BGCDATE, BGMAH, BGSAL, BGFOCXDAY, BGFOCN1MON, BGFOCN2MON, BGFOCN3MON, BGFOCN4MON, BGFOCN5MON, BGFOCN6MON, BGFOCN7MON, BGFOCN8MON, BGFOCN9MON, BGFOCN10MON, BGFOCN11MON, BGFOCNDMON, BGBUGETMON, BGMBCHEKMON, BGMBDARAM, BGMBCASH, BGMBCHEK0, BGMBCHEK1, BGMBCHEK2, BGMBCHEK3, BGMBCHEK4, BGMBCHEK5, BGMBCHEK6, BGMBCHEK7, BGMBCHEK8, BGMBCHEK9, BGMBCHEK10, BGMBCHEK11, BGMBCHEK12, BGPCASH, BGPCH0, BGPCH1, BGPCH2, BGPCH3, BGPCH4, BGPCH5, BGPCH6, BGPCH7, BGPCH8, BGPCH9, BGPCH10, BGPCH11, BGPCH12, NTDZBMCHDABD, BGMBCHEKJM12, BGMBCHEKJM11, BGMBCHEKJM10, BGMBCHEKJM9, BGMBCHEKJM8, BGMBCHEKJM7, BGMBCHEKJM6, BGMBCHEKJM5, BGMBCHEKJM4, BGMBCHEKJM3, BGMBCHEKJM2, BGMBCHEKJM1, BGMBCHEKJM0, HESNAGHD, USERID, CRT, UID)
                                    VALUES({PublicVRB.GenetalBGID},{PublicVRB.General_bgdate},{(MahSelectedInd() + 1)},{SalTex()}, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, N'', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, N'', 0, GETDATE(), 0);");
                                }
                                else
                                {
                                    var querUpdate_Budget = dbms.DoExecuteSQL($"UPDATE BUGET_MAIN SET BGCDATE = {PublicVRB.General_bgdate},  BGMAH = {(MahSelectedInd() + 1)}, BGSAL = {SalTex()} WHERE BGID = {PublicVRB.GenetalBGID}");
                                }
                                //goto NOO;
                                EditOldBDG = false;
                            }
                        }
                        else
                        {
                            if (PublicVRB.GenetalBGID == 0)
                            {
                                PublicVRB.GenetalBGID = CL_HESABDARI.GetLIDD("BUGET_MAIN", "BGID");
                                //var querInsert_Budget = dbms.DoExecuteSQL($"INSERT INTO BUGET_MAIN VALUES({PublicVRB.GenetalBGID},{PublicVRB.General_bgdate},{(MahSelectedInd() + 1)},{SalTex()},0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,NULL,NULL)");

                                var querInsert_Budget = dbms.DoExecuteSQL($@"INSERT INTO dbo.BUGET_MAIN(BGID, BGCDATE, BGMAH, BGSAL, BGFOCXDAY, BGFOCN1MON, BGFOCN2MON, BGFOCN3MON, BGFOCN4MON, BGFOCN5MON, BGFOCN6MON, BGFOCN7MON, BGFOCN8MON, BGFOCN9MON, BGFOCN10MON, BGFOCN11MON, BGFOCNDMON, BGBUGETMON, BGMBCHEKMON, BGMBDARAM, BGMBCASH, BGMBCHEK0, BGMBCHEK1, BGMBCHEK2, BGMBCHEK3, BGMBCHEK4, BGMBCHEK5, BGMBCHEK6, BGMBCHEK7, BGMBCHEK8, BGMBCHEK9, BGMBCHEK10, BGMBCHEK11, BGMBCHEK12, BGPCASH, BGPCH0, BGPCH1, BGPCH2, BGPCH3, BGPCH4, BGPCH5, BGPCH6, BGPCH7, BGPCH8, BGPCH9, BGPCH10, BGPCH11, BGPCH12, NTDZBMCHDABD, BGMBCHEKJM12, BGMBCHEKJM11, BGMBCHEKJM10, BGMBCHEKJM9, BGMBCHEKJM8, BGMBCHEKJM7, BGMBCHEKJM6, BGMBCHEKJM5, BGMBCHEKJM4, BGMBCHEKJM3, BGMBCHEKJM2, BGMBCHEKJM1, BGMBCHEKJM0, HESNAGHD, USERID, CRT, UID)
                                                                                        VALUES({PublicVRB.GenetalBGID}, {PublicVRB.General_bgdate}, {(MahSelectedInd() + 1)}, {SalTex()}, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, N'', 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, N'', 0, GETDATE(), 0);");
                            }
                            else
                            {
                                //Never Used
                                var querUpdate_Budget = dbms.DoExecuteSQL($"UPDATE BUGET_MAIN SET BGCDATE = {PublicVRB.General_bgdate},  BGMAH = {(MahSelectedInd() + 1)}, BGSAL = {SalTex()} WHERE BGID = {PublicVRB.GenetalBGID}");
                            }
                        }
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if (!(Prc is null)) { ProcLoader.Stop(Prc); }
                            StopAnimation();
                            BUDGET_DEFAULT bdg_defalt = new BUDGET_DEFAULT();
                            Hide(); dispatcherTimer.Stop();
                            bdg_defalt.Show();
                        }));

                    }

                }

            });

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            normingItem();
            TimStar(true);
        }
        private void Command3_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            CL_LMethods.GoExitTheApplication();
        }
        private void lbl_sal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SAL.Focus();
            SAL.SelectAll();
        }
        private void lnl_mah_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MAH.Focus();
            MAH.IsDropDownOpen = true;
        }
        private void SAL_TextChanged(object sender, TextChangedEventArgs e)
        {
            Checkup_Fileds();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Check Matter Must Stop After Forms
            cc++;
            if (cc == 4)
            {
                cc = 0;
                Pop1.IsOpen = false;
                return;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Winloadis_Completed = true;
        }
        private void MAH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Checkup_Fileds();
        }
        private void MAH_TextChanged(object sender, TextChangedEventArgs e)
        {
            Checkup_Fileds();
        }
        private void MAH_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
