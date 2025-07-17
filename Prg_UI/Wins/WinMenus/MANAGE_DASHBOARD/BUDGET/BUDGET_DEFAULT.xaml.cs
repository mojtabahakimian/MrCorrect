using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Validatory;
using Stimulsoft.Report.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    public partial class BUDGET_DEFAULT : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public BUDGET_DEFAULT()
        {
            InitializeComponent();
            DataContext = new VL_Validator();
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

        public void SumDRS()
        {
            //Matter Check
            double BGPCH_Sum = 0;
            for (int i = 0; i <= 12; i++)
            {
                //Matter Check
                TextBox BGPCH_texb = (TextBox)this.FindName("BGPCH" + i);
                BGPCH_texb.Text = BGPCH_texb.Text.Trim();
                if (!string.IsNullOrEmpty(BGPCH_texb.Text) && !string.IsNullOrWhiteSpace(BGPCH_texb.Text) && double.TryParse(BGPCH_texb.Text, out double res) == true && double.Parse(BGPCH_texb.Text) != 0)
                {
                    BGPCH_Sum += Convert.ToDouble(BGPCH_texb.Text);
                }
            }
            if (!string.IsNullOrEmpty(BGPCASH.Text) && !string.IsNullOrWhiteSpace(BGPCASH.Text) && double.TryParse(BGPCASH.Text, out double res2) == true && double.Parse(BGPCASH.Text) != 0)
            {
                BGPCH_Sum += Convert.ToDouble(BGPCASH.Text);
            }
            JAM.Text = BGPCH_Sum.ToString();
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
        private async void Command4_Click(object sender, RoutedEventArgs e)
        {
            StartThink();
            await Go_For_BDG2();
        }
        private string BGPCH(string bgp)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                switch (bgp)
                {
                    case "1": bgp = BGPCH1.Text; break;
                    case "2": bgp = BGPCH2.Text; break;
                    case "3": bgp = BGPCH3.Text; break;
                    case "4": bgp = BGPCH4.Text; break;
                    case "5": bgp = BGPCH5.Text; break;
                    case "6": bgp = BGPCH6.Text; break;
                    case "7": bgp = BGPCH7.Text; break;
                    case "8": bgp = BGPCH8.Text; break;
                    case "9": bgp = BGPCH9.Text; break;
                    case "10": bgp = BGPCH10.Text; break;
                    case "11": bgp = BGPCH11.Text; break;
                    case "12": bgp = BGPCH12.Text; break;
                    case "0": bgp = BGPCH0.Text; break;
                    case "BGPCASH": bgp = BGPCASH.Text; break;
                    case "NTDZBMCHDABD": bgp = NTDZBMCHDABD.Text; break;
                    default: break;
                }
            }));
            return bgp;
        }
        private string HESNAGHD_SelectedCombo()
        {
            string h = "";

            if (!string.IsNullOrEmpty(HESNAGHD.SelectedValue.ToString()))
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    h = HESNAGHD.SelectedValue.ToString();
                }));
            }
            return h;

            //HESNAGHD = " + HESNAGHD_SelectedCombo() + " , 
        }
        private string NTD_SelectedCombo()
        {
            string h = "";
            Dispatcher.Invoke(new Action(() =>
            {
                h = NTDZBMCHDABD.Text.ToString();
            }));
            return h;
            //HESNAGHD = " + HESNAGHD_SelectedCombo() + " , 
        }
        private async Task Go_For_BDG2()
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    RemovalQotTex();
                }));
                string test = "UPDATE BUGET_MAIN SET BGPCH1 =" + BGPCH("1") + ", BGPCH2 =" + BGPCH("2") + ", BGPCH3 =" + BGPCH("3") + ", BGPCH4 =" + BGPCH("4") + ", BGPCH5 =" + BGPCH("5") + ", BGPCH6 =" + BGPCH("6") + ", BGPCH7 =" + BGPCH("7") + ", BGPCH8 =" + BGPCH("8") + ", BGPCH9 =" + BGPCH("9") + ", BGPCH10 =" + BGPCH("10") + ", BGPCH11 =" + BGPCH("11") + ", BGPCH12 =" + BGPCH("12") + ", BGPCASH =" + BGPCH("BGPCASH") + ", BGPCH0 =" + BGPCH("0") + ", NTDZBMCHDABD = '" + BGPCH("NTDZBMCHDABD") + "' WHERE  BGID = " + PublicVRB.GenetalBGID;
                //var quer_CNUpdate = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGPCH1 =" + this.BGPCH1.Text + ", BGPCH2 =" + this.BGPCH2.Text + ", BGPCH3 =" + this.BGPCH3.Text + ", BGPCH4 =" + this.BGPCH4.Text + ", BGPCH5 =" + this.BGPCH5.Text + ", BGPCH6 =" + this.BGPCH6.Text + ", BGPCH7 =" + this.BGPCH7.Text + ", BGPCH8 =" + this.BGPCH8.Text + ", BGPCH9 =" + this.BGPCH9.Text + ", BGPCH10 =" + this.BGPCH10.Text + ", BGPCH11 =" + this.BGPCH11.Text + ", BGPCH12 =" + this.BGPCH12.Text + ", BGPCASH =" + this.BGPCASH.Text + ", BGPCH0 =" + this.BGPCH0.Text + ", NTDZBMCHDABD = '" + this.NTDZBMCHDABD.Text + "' WHERE  BGID = " + PublicVRB.GenetalBGID);
                var quer_CNUpdate = dbms.DoExecuteSQL("UPDATE BUGET_MAIN SET BGPCH1 =" + BGPCH("1") + ", BGPCH2 =" + BGPCH("2") + ", BGPCH3 =" + BGPCH("3") + ", BGPCH4 =" + BGPCH("4") + ", BGPCH5 =" + BGPCH("5") + ", BGPCH6 =" + BGPCH("6") + ", BGPCH7 =" + BGPCH("7") + ", BGPCH8 =" + BGPCH("8") + ", BGPCH9 =" + BGPCH("9") + ", BGPCH10 =" + BGPCH("10") + ", BGPCH11 =" + BGPCH("11") + ", BGPCH12 =" + BGPCH("12") + ", BGPCASH =" + BGPCH("BGPCASH") + ", BGPCH0 =" + BGPCH("0") + ", NTDZBMCHDABD = '" + BGPCH("NTDZBMCHDABD") + "' WHERE  BGID = " + PublicVRB.GenetalBGID);
                Dispatcher.Invoke(new Action(() =>
                {
                    var qre = dbms.DoGetDataSQL<BUGET_DEFAULT>("SELECT * FROM BUGET_DEFAULT").ToList();
                    if (qre != null)
                    {
                        var quer_rstset2 = dbms.DoExecuteSQL("UPDATE BUGET_DEFAULT SET  "
                                                                   + " BGDEFID = 1,"
                                                                   + " BGPCASH = " + BGPCH("BGPCASH") + ","
                                                                   + " BGPCH0  = " + BGPCH("0") + ","
                                                                   + " BGPCH1  = " + BGPCH("1") + ","
                                                                   + " BGPCH2  = " + BGPCH("2") + ","
                                                                   + " BGPCH3  = " + BGPCH("3") + ","
                                                                   + " BGPCH4  = " + BGPCH("4") + ","
                                                                   + " BGPCH5  = " + BGPCH("5") + ","
                                                                   + " BGPCH6  = " + BGPCH("6") + ","
                                                                   + " BGPCH7  = " + BGPCH("7") + ","
                                                                   + " BGPCH8  = " + BGPCH("8") + ","
                                                                   + " BGPCH9  = " + BGPCH("9") + ","
                                                                   + " BGPCH10 = " + BGPCH("10") + ","
                                                                   + " BGPCH11 = " + BGPCH("11") + ","
                                                                   + " BGPCH12 = " + BGPCH("12") + ","
                                                                   + " NTDZBMCHDABD = '" + NTD_SelectedCombo() + "',  "
                                                                   + " USERID = 1,        "
                                                                   + " CRDATE = GETDATE(),        "
                                                                   + " HESNAGHD = '" + HESNAGHD_SelectedCombo() + "'");
                    }
                    else
                    {
                        var quer_rstset1 = dbms.DoExecuteSQL("INSERT INTO BUGET_DEFAULT VALUES (0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,'NTDZBMCHDABD0.50%',0,GETDATE(),NULL)");
                    }
                    if (!string.IsNullOrEmpty(HESNAGHD.Text))
                    {
                        PublicVRB.HesabCombobText = (HESNAGHD.Text + " " + HESNAGHD.SelectedValue).ToString();
                    }
                    BUDGET2 bdgt2 = new BUDGET2(); Hide(); bdgt2.ShowDialog(); StopThink();
                }));
            });
        }
        private void StartThink()
        {
            myg.Visibility = Visibility.Hidden;
            myg.Visibility = Visibility.Visible;
            //var controller = ImageBehavior.GetAnimationController(this.myg);
            //controller.Play();
        }
        private void StopThink()
        {
            myg.Visibility = Visibility.Visible;
            myg.Visibility = Visibility.Hidden;
            //var controller = ImageBehavior.GetAnimationController(this.myg);
            //controller.Pause();
        }
        private void Command3_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PublicVRB.BUDG_1.ShowDialog();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            preloader.Visibility = Visibility.Visible;
            TextBoxFormat.FormatStringTexBox(this);

            var quer_RecordsetClone = dbms.DoGetDataSQL<BUGET_MAIN>("SELECT BGID,BGCDATE,BGMAH,BGSAL,BGFOCXDAY,BGFOCN1MON,BGFOCN2MON,BGFOCN3MON,BGFOCN4MON,BGFOCN5MON,BGFOCN6MON,BGFOCN7MON,BGFOCN8MON,BGFOCN9MON,BGFOCN10MON,BGFOCN11MON,BGFOCNDMON,BGBUGETMON,BGMBCHEKMON,BGMBDARAM,BGMBCASH,BGMBCHEK0,BGMBCHEK1,BGMBCHEK2,BGMBCHEK3,BGMBCHEK4,BGMBCHEK5,BGMBCHEK6,BGMBCHEK7,BGMBCHEK8,BGMBCHEK9,BGMBCHEK10,BGMBCHEK11,BGMBCHEK12,BGPCASH,BGPCH0,BGPCH1,BGPCH2,BGPCH3,BGPCH4,BGPCH5,BGPCH6,BGPCH7,BGPCH8,BGPCH9,BGPCH10,BGPCH11,BGPCH12,NTDZBMCHDABD,BGMBCHEKJM12,BGMBCHEKJM11,BGMBCHEKJM10,BGMBCHEKJM9,BGMBCHEKJM8,BGMBCHEKJM7,BGMBCHEKJM6,BGMBCHEKJM5,BGMBCHEKJM4,BGMBCHEKJM3,BGMBCHEKJM2,BGMBCHEKJM1,BGMBCHEKJM0,HESNAGHD,USERID FROM BUGET_MAIN WHERE BGID = " + PublicVRB.GenetalBGID + "").ToList();
            if (quer_RecordsetClone.Count == 0)
            {
                dbms.DoExecuteSQL($"INSERT [BUGET_DEFAULT] VALUES(1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,''," + Baseknow.USERCOD + ",GETDATE())");
                //CurrentProject.Connection.Execute("INSERT [BUGET_DEFAULT] VALUES(1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,''," + @Forms["BASEKNOW"]["USERCOD"] + ",GETDATE())");//MostFix 1 Start
                //Check Matter #Check کاربری که باید ذخیره کنه کی اومده تو چک
                //var quer_CNInsert = dbms.DoExecuteSQL("INSERT [BUGET_DEFAULT] VALUES(1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,'', 2 ,GETDATE())");
                //this.USERNAME = UCurrentUser();
                //this.Requery;

            }
            else
            {
                //this.USERNAME = GETUSERNAME(this.USERID);
            }
            PublicVRB.GenetalBGID = (PublicVRB.GenetalBGID == null ? 1 : PublicVRB.GenetalBGID);

            //ComboFiller comboFiller = new ComboFiller();
            //comboFiller.GoFill_CUSTHESAB(HESNAGHD);
            HESNAGHD.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB_2>("SELECT hes, NAME AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            HESNAGHD.SelectedValuePath = "Expr1";
            HESNAGHD.SelectedIndex = 0;
            HESNAGHD.IsDropDownOpen = true;
            HESNAGHD.IsDropDownOpen = false;


            if (BUDGET1.EditOldBDG == true)
            {
                //var QreBT_DEFULT = dbms.BUGET_DEFAULT.Select(x => x);
                var QreBT_DEFULT = dbms.DoGetDataSQL<BUGET_DEFAULT>("SELECT * FROM BUGET_DEFAULT").ToList();
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
                                    ((TextBox)control).Text = Math.Round(BDT_DEF_ROW.GetType().GetProperty(((TextBox)control).Name).GetValue(BDT_DEF_ROW)).ToString();
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(BDT_DEF_ROW.HESNAGHD))
                    {
                        HESNAGHD.SelectedValue = BDT_DEF_ROW.HESNAGHD;
                    }
                    NTDZBMCHDABD.Text = BDT_DEF_ROW.NTDZBMCHDABD.ToString();
                }
                SumDRS();
            }
            //BGPCH1.Text = Convert.ToDouble(BGPCH1.Text.Replace(",", "")).ToString("0:N2", CultureInfo.CreateSpecificCulture("fa-IR"));
            //UiRefresher.RefWhole(mygrid);
            //(PublicVRB.BUDG_Deflt).UpdateLayout();
        }
        private void Command19_Click(object sender, RoutedEventArgs e)
        {
            BUDGET5 bdgt5 = new BUDGET5();
            Hide();
            bdgt5.ShowDialog();
        }
        private void BGPCH1_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();
        }
        private void BGPCH2_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH3_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH4_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH5_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH6_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCASH_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH0_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH7_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();
        }
        private void BGPCH8_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH9_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH10_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH11_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH12_LostFocus(object sender, RoutedEventArgs e)
        {
            SumDRS();

        }
        private void BGPCH1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(BGPCH1.Text))
            //{

            //}
            //    //BGPCH1.Text = (Convert.ToDouble(BGPCH1.Text.Replace(",", "")).ToString("0:N2", CultureInfo.CreateSpecificCulture("fa-IR"))).ToString();
            //    //BGPCH1.SelectionStart = BGPCH1.Text.Length;
            //    TexChartor.Make_TSeprated(BGPCH1);
            //}
            //TexChartor.Del_ZeroFirst(BGPCH1);
        }
        private void BGPCH2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            preloader.Visibility = Visibility.Collapsed;
        }

        private void NTDZBMCHDABD_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TexChartor.ComboisChoosedItem_Valid(NTDZBMCHDABD) == false)
            {
                NTDZBMCHDABD.Focus();
            }
        }

        private void HESNAGHD_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TexChartor.ComboisChoosedItem_Valid(HESNAGHD) == false)
            {
                HESNAGHD.Focus();
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
