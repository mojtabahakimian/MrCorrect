using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
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

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    public partial class DBS
    {
        public string Expr1 { get; set; }
        public Int16 Expr2 { get; set; }
    }
    public partial class RST1
    {
        public double? MAB { get; set; }
    }
    public partial class RST2
    {
        public double? MAB { get; set; }
    }
    public partial class RSTSM
    {
        public double? mbb { get; set; }
        public double? MS { get; set; }
        public double? YS { get; set; }
        public double? MD { get; set; }
        public double? YD { get; set; }
        public double? mbav { get; set; }
        public double? p { get; set; }
        public double? C0 { get; set; }
        public double? C1 { get; set; }
        public double? C2 { get; set; }
        public double? C3 { get; set; }
        public double? C4 { get; set; }
        public double? C5 { get; set; }
        public double? C6 { get; set; }
        public double? C7 { get; set; }
        public double? C8 { get; set; }
        public double? C9 { get; set; }
        public double? C10 { get; set; }
        public double? C11 { get; set; }
        public double? C12 { get; set; }
    }
    public partial class BUDGET5 : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public BUDGET5()
        {
            InitializeComponent();
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
        private void lbl_budget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void FillCombo()
        {
            //ALL Of Hesab =  NAME + Codes
            //FRCHASH.ItemsSource = dbms.Database.SqlQuery<CUST_HESAB>("SELECT hes, NAME AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            //FRCHASH.SelectedValuePath = "Expr1";
            //FRCHASH.SelectedIndex = 0;
            //FRCHASH.IsDropDownOpen = true;
            //FRCHASH.IsDropDownOpen = false;
            //ComboFiller comboFiller = new ComboFiller();
            //comboFiller.GoFill_CUSTHESAB(FRCHASH);
         
            FRCHASH.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB_2>("SELECT hes, NAME AS nam, hes AS Expr1 FROM CUST_HESAB").ToList();
            FRCHASH.SelectedValuePath = "Expr1";
            FRCHASH.SelectedIndex = 0;
            FRCHASH.IsDropDownOpen = true;
            FRCHASH.IsDropDownOpen = false;

            //DB Names and ID
            dbnam.ItemsSource = dbms.DoGetDataSQL<DBS>("SELECT name AS Expr1, dbid AS Expr2 FROM master.dbo.sysdatabases WHERE (dbid > 4) ORDER BY Expr2").ToList();
            dbnam.DisplayMemberPath = "Expr1";//DBNAME
            dbnam.SelectedValuePath = "Expr2";//DBID
            dbnam.SelectedIndex = 0;
            dbnam.SelectedItem = 0;
            dbnam.IsDropDownOpen = true;
            dbnam.IsDropDownOpen = false;

        }
        private string DT_1()
        {
            string dt = "";
            Dispatcher.Invoke(new Action(() =>
            {
                dt = TexChartor.GetAllNumbers(DT1.Text);
            }));
            return dt;
        }
        private string DT_2()
        {
            string dt = "";
            Dispatcher.Invoke(new Action(() =>
            {
                dt = TexChartor.GetAllNumbers(DT2.Text);
            }));
            return dt;
        }
        private string dbnam_tex()
        {
            string dt = "";
            Dispatcher.Invoke(new Action(() =>
            {
                dt = dbnam.Text;
            }));
            return dt;
        }
        private int FRCHASH_SelectedInd()
        {
            int combindex = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                combindex = FRCHASH.SelectedIndex;
            }));
            return combindex;
        }
        private int dbnam_SelectedInd()
        {
            int combindex = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                combindex = FRCHASH.SelectedIndex;
            }));
            return combindex;
        }
        private object FRCHASHSelectedTtm()
        {
            object combitm = null;
            Dispatcher.Invoke(new Action(() =>
            {
                combitm = FRCHASH.SelectedItem;
            }));
            return combitm;
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
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        myg.Visibility = Visibility.Visible;
        //        myg.Visibility = Visibility.Hidden;
        //        var controller = ImageBehavior.GetAnimationController(this.myg);
        //        controller.Pause();
        //    });
        //}
        private async void Command4_Click(object sender, RoutedEventArgs e)
        {
            //StartThink();
            await GO_FOR_BGDFLT();
        }
        private async Task GO_FOR_BGDFLT()
        {
            await Task.Run(() =>
            {
                int i;
                int TEDAD;
                string rptname;
                if (!string.IsNullOrEmpty(DT_1()) && !string.IsNullOrEmpty(DT_2()) && FRCHASH_SelectedInd() != -1 && dbnam_SelectedInd() != -1)
                {

                    // Budget_Default bdgdefault = new Budget_Default();//Change in Thursday, July 22, 2021
                    var quer_RST = dbms.DoGetDataSQL<RSTSM>("SELECT   sum(mbav) as mbb, C0 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 0 THEN mbav ELSE 0 END) / sum(mbav) * 100, C1 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 1 THEN mbav ELSE 0 END) / sum(mbav) * 100, C2 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 2 THEN mbav ELSE 0 END) / sum(mbav) * 100, C3 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 3 THEN mbav ELSE 0 END) / sum(mbav) * 100, C4 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 4 THEN mbav ELSE 0 END) / sum(mbav) * 100, C5 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 5 THEN mbav ELSE 0 END) / sum(mbav) * 100, C6 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 6 THEN mbav ELSE 0 END) / sum(mbav) * 100, C7 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 7 THEN mbav ELSE 0 END) / sum(mbav) * 100, C8 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 8 THEN mbav ELSE 0 END) / sum(mbav) * 100, C9 = sum(CASE(ys - yd) * 12 + (ms - md) WHEN 9 THEN mbav ELSE 0 END) / sum(mbav) * 100, " +
                        " C10 =  sum( CASE (ys - yd) * 12 + (ms - md) WHEN 10 THEN mbav ELSE 0 END  )/sum(mbav)*100, C11 =  sum( CASE (ys - yd) * 12 + (ms - md) WHEN 11 THEN mbav ELSE 0 END  )/sum(mbav)*100, C12 =  sum( CASE (ys - yd) * 12 + (ms - md) WHEN 12 THEN mbav ELSE 0 END  )/sum(mbav)*100 FROM  (SELECT        dbo.Umonth(DATE_S) AS MS, dbo.Uyear(DATE_S) AS YS, dbo.Umonth(DATE) AS MD, dbo.Uyear(DATE) AS YD, SUM(MABL) AS mbav FROM " + dbnam_tex() + ".dbo.PAY_GETD WHERE        (N_KOL <> 911 OR  N_KOL IS NULL) AND (DATE BETWEEN " + TexChartor.GetAllNumbers(DT_1()) + " AND " + TexChartor.GetAllNumbers(DT_2()) + ") GROUP BY dbo.Umonth(DATE_S), dbo.Uyear(DATE_S), dbo.Umonth(DATE), dbo.Uyear(DATE) ) as p").ToList();


                    //بررسی خالی بودن مقدار های ستون ساخته شده که میشه رکورد 1 اگر مقدار های این رکورد 1 خالی نباشه
                    if (quer_RST.Count() > 0 && quer_RST != null && quer_RST[0].C0 != null)
                    {
                        // Budget_Default bdg_default = new Budget_Default();////Change in Thursday, July 22, 2021

                        //TextBox BGPCH_texb = null;
                        for (int t = 0; t <= 12; t++)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                TextBox BGPCH_texb = (TextBox)PublicVRB.BUDG_Deflt.FindName("BGPCH" + t);
                                foreach (dynamic itt in quer_RST)
                                {
                                    //BUG 8 Matter Check
                                    BGPCH_texb.Text = (itt.GetType().GetProperty("C" + t).GetValue(itt)).ToString();
                                    BGPCH_texb.Text = Math.Round(Convert.ToDouble(BGPCH_texb.Text), 1).ToString();
                                }
                            }));


                        }
                        //Check
                    }
                    // SUM(DENAF1400.dbo.DEED_DTL.BES - DENAF1400.dbo.DEED_DTL.BED) AS MAB FROM         DENAF1400.dbo.DEED_DTL INNER JOIN     DENAF1400.dbo.DEED_HED ON DENAF1400.dbo.DEED_DTL.N_S = DENAF1400.dbo.DEED_HED.N_S  WHERE     (DENAF1400.dbo.DEED_HED.DATE_S BETWEEN 11111111 AND 99991111) AND (DENAF1400.dbo.DEED_DTL.HES_K = 511 or DENAF1400.dbo.DEED_DTL.HES_K = 512)
                    var quer_RST2 = dbms.DoGetDataSQL<RST1>("SELECT     SUM(" + dbnam_tex() + ".dbo.DEED_DTL.BED) AS MAB FROM         " + dbnam_tex() + ".dbo.DEED_DTL INNER JOIN    " + dbnam_tex() + ".dbo.DEED_HED ON " + dbnam_tex() + ".dbo.DEED_DTL.N_S = " + dbnam_tex() + ".dbo.DEED_HED.N_S WHERE     (" + dbnam_tex() + ".dbo.DEED_HED.DATE_S BETWEEN " + DT_1() + " AND " + DT_2() + ")  AND (" + dbnam_tex() + ".dbo.DEED_DTL.HES = N'" + FRCHASHSelectedTtm() + " ')").FirstOrDefault();
                    if (quer_RST2.MAB != null)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {

                            double CASH;
                            double MAB = Convert.ToDouble(quer_RST2.MAB);
                            CASH = System.Convert.ToDouble(quer_RST2.MAB);
                            var quer_RST3 = dbms.DoGetDataSQL<RST2>("SELECT     SUM(" + dbnam_tex() + ".dbo.DEED_DTL.BES - " + dbnam_tex() + ".dbo.DEED_DTL.BED) AS MAB FROM         " + dbnam_tex() + ".dbo.DEED_DTL INNER JOIN     " + dbnam_tex() + ".dbo.DEED_HED ON " + dbnam_tex() + ".dbo.DEED_DTL.N_S = " + dbnam_tex() + ".dbo.DEED_HED.N_S  WHERE     (" + dbnam_tex() + ".dbo.DEED_HED.DATE_S BETWEEN " + DT_1() + " AND " + DT_2() + ") AND (" + dbnam_tex() + ".dbo.DEED_DTL.HES_K = " + Baseknow.FROSH + " or " + dbnam_tex() + ".dbo.DEED_DTL.HES_K = " + Baseknow.MFROSH + ")").FirstOrDefault();
                            if (quer_RST3.MAB > 0)
                            {
                                double MAB_Final = Convert.ToDouble(quer_RST3.MAB);

                                TextBox BGPCASH_texb = (TextBox)PublicVRB.BUDG_Deflt.FindName("BGPCASH");
                                BGPCASH_texb.Text = Math.Round((CASH / MAB_Final * 100), 1).ToString();
                                // @Forms["buget_defaul"]["BGPCASH"] = CASH / MAB * 100;
                                PublicVRB.BUDG_Deflt.SumDRS();
                            }
                        }));
                    }
                    //StopThink();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Hide();
                        PublicVRB.BUDG_Deflt.ShowDialog();
                    }));
                }
                else
                {
                    MessageBox.Show("لطفا تاریخ - حساب و نام بانک اطلاعاتی را خالی نگذارید");
                }
            });

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillCombo();
            CL_LMethods.UiRefresher.RefWhole(mygrid);
            (PublicVRB.BUDG_5).UpdateLayout();
            //((Budget5)Application.Current.MainWindow).UpdateLayout();
        }
        private void Command3_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PublicVRB.BUDG_Deflt.ShowDialog();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
