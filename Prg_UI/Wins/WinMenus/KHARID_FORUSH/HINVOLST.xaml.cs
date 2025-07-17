using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.Functions;
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
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using Wins.WinMenus.KHARID_FORUSH;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    public partial class HINVOLST : Window
    {
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

        List<ALLFACOTRHAVLE> ItemsDGR = new List<ALLFACOTRHAVLE>();

        /// <summary>
        /// لیست کل پیش فاکتور داخل این قرار میگره
        /// </summary>
        public List<ALLFACOTRHAVLE> FIRST_LST { get; set; }
        /// <summary>
        /// Tasks for my "asyncs"
        /// </summary>
        Task[] ALL_TASKY = new Task[10];
        /// <summary>
        /// A thread to check that "ALL_TASKY" thread until it completes
        /// </summary>
        public Task TSK_STATE { get; set; }

        string FilterSearch = "";
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        byte TAG_asWhoCall = 0;
        /// <summary>
        /// یعنی کدام فرم مرا صدا زده, اگر پیش فاکتور است یعنی تگ 20
        /// </summary>
        /// <param name="TAG_asWhoCall0"></param>
        public HINVOLST(byte TAG_asWhoCall0)
        {
            TAG_asWhoCall = TAG_asWhoCall0;
            InitializeComponent();
            this.Owner = PublicVRB.WINBASE;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            switch (TAG_asWhoCall)
            {
                case 20: GoloadHEAD_LST_PISHFOROOSH2(); break; //یعنی مشاهده پیش فاکتور ها

                default:
                    break;
            }

        }

        private void ResetSearch()
        {
            FilterSearch = "";
            Number_Search.Text = "";
            Custno_Search.Text = "";
            DATE_N_Search.Text = "";
            Molah_Search.Text = "";
            Mas_Search.Text = "";
        }

        private void OnOpen_HEADLST_FOROOSH2()
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();
            int i;
            //Forms["baseknow"]["hhwin"] = this.hwnd;
            if (!CL_HESABDARI.LETSGO("FRSKB"))
            {
                //DATE_N
                //var RST = dbms.DoGetDataSQL<long>("SELECT     TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + Forms["Default"]["TFSAZMAN"] + ") AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "') GROUP BY DATE_N ORDER BY DATE_N DESC").ToList();
                var RST = dbms.DoGetDataSQL<long>("SELECT     TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ") AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "') GROUP BY DATE_N ORDER BY DATE_N DESC").ToList();
                if (RST.Count > 0 && Convert.ToDouble(Baseknow.CPI) > 0)
                {
                    i = 1;
                    int CPI = Convert.ToInt32(Baseknow.CPI);

                    long DATE_RST = 0;

                    if (RST.Count <= Convert.ToInt64(CPI))
                    {
                        DATE_RST = RST[CPI];
                    }
                    else
                    {
                        DATE_RST = RST.FirstOrDefault();
                    }
                    //while (!RST.EOF & i <= Convert.ToInt32(Baseknow.CPI))
                    //{
                    //    RST.MoveNext();
                    //    i = i + 1;
                    //}
                    //foreach (var item in RST)
                    //{
                    //    if (i <= Convert.ToInt32(Baseknow.CPI))
                    //    {
                    //        i = i + 1;
                    //    }
                    //}
                    //if (RST.EOF)
                    //{
                    //    RST.MovePrevious();
                    //}
                    //PublicVRB.RecordSource_TAG20 = "SELECT HEAD_LST.* FROM HEAD_LST WHERE (TAG = 20) AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "') AND (DATE_N >=" + DATE_RST + ") ORDER BY NUMBER ";

                    DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT        TOP (100) PERCENT dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.OKF, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.MAS, dbo.CUST_HESAB.NAME, 
                                                                                                                                 dbo.HEAD_LST.TAMIR
                                                                                                        FROM            dbo.CUST_HESAB RIGHT OUTER JOIN
                                                                                                                                 dbo.HEAD_LST ON dbo.CUST_HESAB.hes = dbo.HEAD_LST.CUST_NO LEFT OUTER JOIN
                                                                                                                                 dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD
                                                                                                        WHERE        (dbo.HEAD_LST.TAG = 20) AND
                                                                                                         dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}'  AND 
                                                                                                         dbo.HEAD_LST.DATE_N >= {DATE_RST}
                                                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                }
                else
                {
                    //PublicVRB.RecordSource_TAG20 = "SELECT HEAD_LST.* FROM HEAD_LST WHERE (TAG = 20) AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "')  ORDER BY NUMBER ";
                    DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT        TOP (100) PERCENT dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.OKF, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.MAS, dbo.CUST_HESAB.NAME, 
                                                                                                                                 dbo.HEAD_LST.TAMIR
                                                                                                        FROM            dbo.CUST_HESAB RIGHT OUTER JOIN
                                                                                                                                 dbo.HEAD_LST ON dbo.CUST_HESAB.hes = dbo.HEAD_LST.CUST_NO LEFT OUTER JOIN
                                                                                                                                 dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD
                                                                                                        WHERE        (dbo.HEAD_LST.TAG = 20) AND
                                                                                                         dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' 
                                                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                }
            }
            // Me.Refresh
            else
            {
                string vs;
                if (CL_HESABDARI.LETSGO("DEPEMAL")) // اگر محدود به واحد خودش هست
                {
                    //this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + Forms["Default"]["TFSAZMAN"] + ")";
                    //PublicVRB.RecordSource_TAG20 = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                    DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT        TOP (100) PERCENT dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.OKF, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.MAS, dbo.CUST_HESAB.NAME, 
                                                                                                                                             dbo.HEAD_LST.TAMIR
                                                                                                                    FROM            dbo.CUST_HESAB RIGHT OUTER JOIN
                                                                                                                                             dbo.HEAD_LST ON dbo.CUST_HESAB.hes = dbo.HEAD_LST.CUST_NO LEFT OUTER JOIN
                                                                                                                                             dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD
                                                                                                                    WHERE        (dbo.HEAD_LST.TAG = 20) AND  dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER}
                                                                                                                    ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                    if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // اگر محدود به زير مجموعه خودش هست
                    {
                        vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                        if (string.IsNullOrEmpty(vs))
                        {
                            //goto allu;//
                            //PublicVRB.GOALLU();
                            return;
                        }
                        else
                        {
                            //this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + Forms["Default"]["TFSAZMAN"] + ") and " + vs;
                            //PublicVRB.RecordSource_TAG20 = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 20) AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ") and " + vs;

                            DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT        TOP (100) PERCENT dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.OKF, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.MAS, dbo.CUST_HESAB.NAME, 
                                                                                                                                             dbo.HEAD_LST.TAMIR
                                                                                                                    FROM            dbo.CUST_HESAB RIGHT OUTER JOIN
                                                                                                                                             dbo.HEAD_LST ON dbo.CUST_HESAB.hes = dbo.HEAD_LST.CUST_NO LEFT OUTER JOIN
                                                                                                                                             dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD
                                                                                                                    WHERE        (dbo.HEAD_LST.TAG = 20) AND  dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER} AND {vs}
                                                                                                                    ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                        }
                    }
                }
                else if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // اگر محدود به  زير مجموعه خودش هست
                {
                    vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                    if (string.IsNullOrEmpty(vs))
                    {
                        //goto allu;//
                        //PublicVRB.GOALLU();
                        return;
                    }
                    else
                    {
                        //PublicVRB.RecordSource_TAG20 = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 20) AND " + vs;
                        DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT        TOP (100) PERCENT dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.OKF, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.MAS, dbo.CUST_HESAB.NAME, 
                                                                         dbo.HEAD_LST.TAMIR
                                                FROM            dbo.CUST_HESAB RIGHT OUTER JOIN
                                                                         dbo.HEAD_LST ON dbo.CUST_HESAB.hes = dbo.HEAD_LST.CUST_NO LEFT OUTER JOIN
                                                                         dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD
                                                WHERE        (dbo.HEAD_LST.TAG = 20) AND {vs}
                                                ORDER BY dbo.HEAD_LST.NUMBER DESC  ").ToList();
                    }
                }
                else
                {
                    try
                    {
                        DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall} 
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList(); //OPTION (HASH JOIN)

                    }
                    catch (Exception)
                    {
                        DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall} 
                                                                                 ORDER BY NUMBER DESC 
                                                                                 OPTION (QUERYTRACEON 2312)
                                                                                ").ToList();
                    }
                }
                // Me.Refresh
                ItemsDGR = DGR_HEAD_LSTPISHFACTOR2.ItemsSource as List<ALLFACOTRHAVLE>;
            }

            ProcLoader.Stop(Prc);
        }

        private bool OtherIsEmpty(string elmeman)
        {
            //if (ELMNT is TextBox) ((TextBox)ELMNT).IsReadOnly
            foreach (UIElement ctrl in mygrid.Children)
            {
                if (ctrl.GetType() == typeof(TextBox))
                {
                    if (((TextBox)ctrl).Name != elmeman)
                    {
                        if (!string.IsNullOrEmpty((ctrl as TextBox).Text))
                        {
                            return false;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(DATE_N_Search.Value?.ToString()))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// کنترلی که نمیخواید غیر فعال بشه اسمشو بهش بدید به غیر از کنترل که شما بهش معرفی کردید بقیه تکستباکس ها رو غیر فعال میکنه
        /// </summary>
        /// <param name="elmeman"></param>
        private void SetOtherdisable(string elmeman)
        {
            //if (ELMNT is TextBox) ((TextBox)ELMNT).IsReadOnly
            foreach (UIElement ctrl in mygrid.Children)
            {
                if (ctrl.GetType() == typeof(TextBox))
                {
                    if (((TextBox)ctrl).Name != elmeman)
                    {
                        (ctrl as TextBox).IsEnabled = false;
                    }
                }
            }
        }
        private void GoloadHEAD_LST_PISHFOROOSH2()
        {
            OnOpen_HEADLST_FOROOSH2();
        }

        private void DGR_HEAD_LSTPISHFACTOR2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DGR_HEAD_LSTPISHFACTOR2_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DATE_N__Search_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void DGR_HEAD_LSTPISHFACTOR2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DGR_HEAD_LSTPISHFACTOR2.SelectedItem != null)
            {
                if (e.Key == Key.Enter)
                {
                    LetsOpenPishfactor();
                }
            }
        }

        private void LetsOpenPishfactor()
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();

            ALLFACOTRHAVLE PISHFACTOR_NUMBER = (ALLFACOTRHAVLE)DGR_HEAD_LSTPISHFACTOR2.SelectedItem;

            HEAD_LST_PISHFROOSH2 hEAD_LST_PISHFROOSH2_Asly = new HEAD_LST_PISHFROOSH2((double)PISHFACTOR_NUMBER.NUMBER);
            //this.Close();
            hEAD_LST_PISHFROOSH2_Asly.Show();

            ProcLoader.Stop(Prc);
        }

        private void PrepairFiltering(string EL)
        {
            try
            {
                switch (EL)
                {
                    case "Number_Search":
                        if (OtherIsEmpty(Number_Search.Name))
                        {
                            if (string.IsNullOrEmpty(Number_Search.Text))//--
                            {
                                if ((bool)SmartCheckBox.IsChecked)
                                    FilterSearch = FilterSearch.Replace($"  AND HEAD_LST.NUMBER  LIKE '%{Number_Search.Text}%' ", "");
                                else
                                    FilterSearch = FilterSearch.Replace($"  AND HEAD_LST.NUMBER = {Number_Search.Text}  ", "");
                            }

                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  AND HEAD_LST.NUMBER  LIKE '%{Number_Search.Text}%' ";
                            else
                                FilterSearch += $"  AND HEAD_LST.NUMBER = {Number_Search.Text}  ";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Number_Search.Text))
                            {
                                if ((bool)SmartCheckBox.IsChecked)
                                    FilterSearch = FilterSearch = FilterSearch.Replace($"  OR HEAD_LST.NUMBER  LIKE '%{Number_Search.Text}%' ", "");
                                else
                                    FilterSearch = FilterSearch = FilterSearch.Replace($"  OR HEAD_LST.NUMBER = {Number_Search.Text}  ", "");
                            }
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  OR HEAD_LST.NUMBER  LIKE '%{Number_Search.Text}%' ";
                            else
                                FilterSearch += $"  OR HEAD_LST.NUMBER = {Number_Search.Text}  ";

                        }
                        break;

                    case "Custno_Search":
                        if (OtherIsEmpty(Custno_Search.Name))
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  AND CUST_HESAB.NAME LIKE N'%{Custno_Search.Text}%'   ";
                            else
                                FilterSearch += $"  AND CUST_HESAB.NAME = {Custno_Search.Text}  ";
                        }
                        else
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  OR CUST_HESAB.NAME LIKE N'%{Custno_Search.Text}%'   ";
                            else
                                FilterSearch += $"  OR CUST_HESAB.NAME = {Custno_Search.Text}  ";
                        }
                        break;

                    case "Mas_Search":
                        if (OtherIsEmpty(Mas_Search.Name))
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  AND HEAD_LST.MAS  LIKE '%{Mas_Search.Text}%' ";
                            else
                                FilterSearch += $"  AND HEAD_LST.MAS = {Mas_Search.Text}  ";
                        }
                        else
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  OR HEAD_LST.MAS  LIKE '%{Mas_Search.Text}%' ";
                            else
                                FilterSearch += $"  OR HEAD_LST.MAS = {Mas_Search.Text}  ";
                        }
                        break;


                    case "Molah_Search":
                        if (OtherIsEmpty(Molah_Search.Name))
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  AND HEAD_LST.MOLAH LIKE N'%{Molah_Search.Text}%'   ";
                            else
                                FilterSearch += $"  AND HEAD_LST.MOLAH = {Molah_Search.Text}  ";
                        }
                        else
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  OR HEAD_LST.MOLAH LIKE N'%{Molah_Search.Text}%'   ";
                            else
                                FilterSearch += $"  OR HEAD_LST.MOLAH = {Molah_Search.Text}  ";
                        }
                        break;

                    case "DATE_N_Search":
                        if (OtherIsEmpty(DATE_N_Search.Name))
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  AND HEAD_LST.DATE_N '%{DATE_N_Search.Text}%' ";
                            else
                                FilterSearch += $"  AND HEAD_LST.DATE_N = {DATE_N_Search.Text}  ";
                        }
                        else if (!OtherIsEmpty(DATE_N_Search.Name))
                        {
                            if ((bool)SmartCheckBox.IsChecked)
                                FilterSearch += $"  OR HEAD_LST.DATE_N  '%{DATE_N_Search.Text}%' ";
                            else
                                FilterSearch += $"  OR HEAD_LST.DATE_N = {DATE_N_Search.Text}  ";
                        }
                        else
                        {
                            FilterSearch = "";
                        }
                        break;

                    default:
                        FilterSearch = "";
                        break;
                }

                if (!string.IsNullOrEmpty(FilterSearch))
                {
                    DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  {FilterSearch}
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
                }
                else
                {
                    DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall} 
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
                }

            }
            catch (Exception)
            {
            }

        }

        private void Number_Search_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Number_Search.Text.Trim()))
            {
                //PrepairFiltering(Number_Search.Name);
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  AND HEAD_LST.NUMBER LIKE {Number_Search.Text.Trim()}
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
            }
            else
            {
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = ItemsDGR;
            }
        }

        private void Custno_Search_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(Custno_Search.Text))
            //{
            //    //PrepairFiltering(Custno_Search.Name);
            //}
            if (!string.IsNullOrEmpty(Custno_Search.Text.Trim()))
            {
                //PrepairFiltering(Number_Search.Name);
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  AND CUST_HESAB.NAME LIKE N'%{Custno_Search.Text.Trim()}%'
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
            }
            else
            {
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = ItemsDGR;
            }
        }

        private void Molah_Search_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(Molah_Search.Text))
            //{
            //    PrepairFiltering(Molah_Search.Name);
            //}
            if (!string.IsNullOrEmpty(Molah_Search.Text.Trim()))
            {
                //PrepairFiltering(Number_Search.Name);
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  AND HEAD_LST.MOLAH LIKE N'%{Molah_Search.Text.Trim()}%'
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
            }
            else
            {
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = ItemsDGR;
            }
        }

        private void Mas_Search_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(Mas_Search.Text))
            //{
            //    PrepairFiltering(Mas_Search.Name);
            //}
            if (!string.IsNullOrEmpty(Molah_Search.Text.Trim()))
            {
                //PrepairFiltering(Number_Search.Name);
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  AND HEAD_LST.MAS LIKE N{Mas_Search.Text.Trim()}
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
            }
            else
            {
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = ItemsDGR;
            }
        }

        private void DATE_N_Search_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(DATE_N_Search.Text))
            //{
            //    PrepairFiltering(DATE_N_Search.Name);
            //}
            string DT = DATE_N_Search.Text.Trim().ToRawTarikh();

            if (!string.IsNullOrEmpty(DT))
            {
                //PrepairFiltering(Number_Search.Name);
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = dbms.DoGetDataSQL<ALLFACOTRHAVLE>($@"SELECT CUSTKIND.CUSTKNAME, 
                                                                                       HEAD_LST.NUMBER, 
                                                                                       HEAD_LST.DATE_N, 
                                                                                       HEAD_LST.OKF, 
                                                                                       HEAD_LST.MOLAH, 
                                                                                       HEAD_LST.MAS, 
                                                                                       CUST_HESAB.NAME ,dbo.HEAD_LST.TAMIR
                                                                                  FROM CUST_HESAB 
                                                                                       RIGHT OUTER JOIN HEAD_LST 
                                                                                          ON CUST_HESAB.hes = HEAD_LST.CUST_NO 
                                                                                       LEFT OUTER JOIN CUSTKIND 
                                                                                         ON HEAD_LST.CUST_KIND = CUSTKIND.CUST_COD 
                                                                                 WHERE HEAD_LST.TAG = {TAG_asWhoCall}  AND HEAD_LST.DATE_N LIKE N'%{DT}%'
                                                                                 ORDER BY NUMBER DESC 
                                                                                ").ToList();
            }
            else
            {
                DGR_HEAD_LSTPISHFACTOR2.ItemsSource = ItemsDGR;
            }
        }
        private void SmartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResetSearch();
        }

        private void CheckingQueryResult()
        {
            //ALL_TASKY[0].Wait();
            //this.Dispatcher.Invoke(() =>
            //{
            //    if (!(FIRST_LST is null))
            //    {
            //        //Finally fill datagrid with the list
            //        MyDataGrid1.ItemsSource = FIRST_LST;

            //        //Close Loading ...
            //        Preloader1.Visibility = Visibility.Collapsed;
            //        Preloader1.IsEnabled = false;
            //        Spinner.IsEnabled = false;
            //        spinInfinite.Freeze();
            //        spinInfinite.Stop();
            //    }
            //});
        }

        private void DGR_HEAD_LSTPISHFACTOR2_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LetsOpenPishfactor();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var focusedControl = FocusManager.GetFocusedElement(this);
                if (focusedControl is DataGridCell || ((FrameworkElement)focusedControl).Parent is DataGridCell)
                {
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Label_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
