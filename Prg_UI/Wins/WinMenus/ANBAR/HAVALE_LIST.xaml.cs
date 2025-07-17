using MaterialDesignThemes.Wpf;
using Microsoft.IdentityModel.Tokens;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_UI.Functions.CL_LMethods;
using Dapper;
using Microsoft.VisualBasic.Logging;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class HAVALE_LIST : Window
    {
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
        public class HAVALEHA_MODEL
        {
            public double? NUMBER { get; set; }
            public string NAME { get; set; }
            public string CUST_NO { get; set; }
            public long? DATE_N { get; set; }
            public string TAH { get; set; }
            public string MOLAH { get; set; }
            public string SHARAYET { get; set; }
            public int? ANBAR { get; set; }
            public double? FNUMCO { get; set; }
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public List<HAVALEHA_MODEL> LIST_HAVALEHA { get; set; } = new List<HAVALEHA_MODEL>();
        public bool MAY_OPEN_LST_HAVALEHA { get; set; } = true;
        public bool NowIsReady { get; set; }

        public HAVALE_LIST()
        {
            InitializeComponent();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //Check Security to open:
            CL_HESABDARI.SETSECURITY(this.GetType().Name, "HAVL", new WindowInteropHelper(this).Handle);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            #region INORIGINAL_SOURCE_WAS
            //int I;
            //if (!CL_HESABDARI.LETSGO("FRSKB"))
            //{
            //    var RST = dbms.DoGetDataSQL<long?>("SELECT     TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = 2) AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ") AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "') GROUP BY DATE_N ORDER BY DATE_N DESC").ToList();
            //    if (RST.Count > 1 && Baseknow.CPI > 0)
            //    {
            //        I = 1;
            //        //while (!RST.EOF && I <= Baseknow.CPI)
            //        while (I <= Baseknow.CPI)
            //        {
            //            //RST.MoveNext();
            //            I = I + 1;
            //        }
            //        //if (RST.EOF)
            //        //{
            //        //    RST.MovePrevious();
            //        //}
            //        var RecordSource = dbms.DoGetDataSQL<HEAD_LST>("SELECT HEAD_LST.* FROM HEAD_LST WHERE (TAG = 2) AND (USER_NAME = '" + UCurrentUser() + "') AND (DATE_N >=" + RST.Fields(0) + ") ORDER BY NUMBER ");
            //    }
            //    else
            //    {
            //        var RecordSource = dbms.DoGetDataSQL<HEAD_LST>("SELECT HEAD_LST.* FROM HEAD_LST WHERE (TAG = 2) AND (USER_NAME = '" + UCurrentUser() + "')  ORDER BY NUMBER ");
            //    }
            //}
            //else
            //{
            //    string VS;
            //    if (LETSGO("DEPEMAL")) // اگر محدود به واحد خودش هست
            //    {
            //        this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 2) AND (DEPATMAN = " + Forms["Default"]["TFSAZMAN"] + ")";
            //        if (LETSGO("chartfilter"))  // اگر محدود به زير مجموعه خودش هست
            //        {
            //            VS = UserOnChart(Forms["BASEKNOW"]["USERCOD"]);
            //            if (string.IsNullOrEmpty(VS))
            //            {
            //                goto allu;
            //            }
            //            else
            //            {
            //                this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 2) AND (DEPATMAN = " + Forms["Default"]["TFSAZMAN"] + ") and " + VS;
            //            }
            //        }
            //    }
            //    else if (LETSGO("chartfilter"))  // اگر محدود به  زير مجموعه خودش هست
            //    {
            //        VS = UserOnChart(Forms["BASEKNOW"]["USERCOD"]);
            //        if (string.IsNullOrEmpty(VS))
            //        {
            //            goto allu;
            //        }
            //        else
            //        {
            //            this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 2) AND " + VS;
            //        }
            //    }
            //    else
            //    {
            //        this.RecordSource = "SELECT     HEAD_LST.*  FROM dbo.HEAD_LST WHERE (TAG = 2)";
            //    }
            //    this.Refresh();
            //}
            #endregion
            DoGetData();

            DGR_HEADLST_HAVL_2.ItemsSource = LIST_HAVALEHA.ToList();
        }
        private void DoGetData0()
        {
            //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
            Process Prc = ProcLoader.Start();

            if (!CL_HESABDARI.LETSGO("FRSKB")) //فاکتور فروش سایر کاربران را بتواند ببیند
            {
                //DATE_N
                var RST = dbms.DoGetDataSQL<long>("SELECT TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = 2) AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ") AND (USER_NAME = '" + CL_HESABDARI.UCurrentUser() + "') GROUP BY DATE_N ORDER BY DATE_N DESC").ToList();
                if (RST.Count > 0 && Convert.ToDouble(Baseknow.CPI) > 0)
                {
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

                    var QRE = dbms.DoGetDataSQL<HAVALEHA_MODEL>($@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                                                        FROM dbo.HEAD_LST
                                                                             LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                        WHERE(dbo.HEAD_LST.TAG=2) AND
                                                                        dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}'  AND 
                                                                        dbo.HEAD_LST.DATE_N >= {DATE_RST}
                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                    LIST_HAVALEHA?.Clear();
                    for (int i1 = 0; i1 < QRE.Count; i1++) LIST_HAVALEHA.Add(QRE[i1]);
                }
                else
                {
                    var QRE = dbms.DoGetDataSQL<HAVALEHA_MODEL>($@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                                                        FROM dbo.HEAD_LST
                                                                             LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                        WHERE(dbo.HEAD_LST.TAG=2) AND
                                                                        dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}'
                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                    LIST_HAVALEHA?.Clear();
                    for (int i1 = 0; i1 < QRE.Count; i1++) LIST_HAVALEHA.Add(QRE[i1]);
                }
            }
            // Me.Refresh
            else
            {
                string vs;
                if (CL_HESABDARI.LETSGO("DEPEMAL")) // اگر محدود به واحد خودش هست
                {
                    var QRE = dbms.DoGetDataSQL<HAVALEHA_MODEL>($@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                                                        FROM dbo.HEAD_LST
                                                                             LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                        WHERE(dbo.HEAD_LST.TAG=2) AND
                                                                        dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER}
                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                    LIST_HAVALEHA?.Clear();
                    for (int i1 = 0; i1 < QRE.Count; i1++) LIST_HAVALEHA.Add(QRE[i1]);

                    if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // اگر محدود به زير مجموعه خودش هست
                    {
                        vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                        if (string.IsNullOrEmpty(vs))
                        {
                            return;
                        }
                        else
                        {
                            var QRE2 = dbms.DoGetDataSQL<HAVALEHA_MODEL>($@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                                                        FROM dbo.HEAD_LST
                                                                             LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                        WHERE(dbo.HEAD_LST.TAG=2) AND
                                                                        dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER} AND {vs}
                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                            LIST_HAVALEHA?.Clear();
                            for (int i1 = 0; i1 < QRE2.Count; i1++) LIST_HAVALEHA.Add(QRE2[i1]);
                        }
                    }
                }
                else if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // اگر محدود به  زير مجموعه خودش هست
                {
                    vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                    if (string.IsNullOrEmpty(vs))
                    {
                        return;
                    }
                    else
                    {
                        var QRE2 = dbms.DoGetDataSQL<HAVALEHA_MODEL>($@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                                                        FROM dbo.HEAD_LST
                                                                             LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                        WHERE(dbo.HEAD_LST.TAG=2) AND {vs}
                                                                        ORDER BY dbo.HEAD_LST.NUMBER DESC").ToList();
                        LIST_HAVALEHA?.Clear();
                        for (int i1 = 0; i1 < QRE2.Count; i1++) LIST_HAVALEHA.Add(QRE2[i1]);
                    }
                }
                else
                {
                    try
                    {
                        var QRE = dbms.DoGetDataSQL<HAVALEHA_MODEL>(@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO 
                                                 FROM dbo.HEAD_LST
                                                      LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                 WHERE(dbo.HEAD_LST.TAG=2) ORDER BY NUMBER DESC").ToList();

                        LIST_HAVALEHA?.Clear();
                        for (int i = 0; i < QRE.Count; i++) LIST_HAVALEHA.Add(QRE[i]);
                    }
                    catch (Exception)
                    {
                        var QRE = dbms.DoGetDataSQL<HAVALEHA_MODEL>(@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO, 
                                                 FROM dbo.HEAD_LST
                                                      LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                 WHERE(dbo.HEAD_LST.TAG=2) ORDER BY NUMBER DESC
                                                 OPTION (QUERYTRACEON 2312)
                                                 ").ToList();

                        LIST_HAVALEHA?.Clear();
                        for (int i = 0; i < QRE.Count; i++) LIST_HAVALEHA.Add(QRE[i]);
                    }
                }
            }

            ProcLoader.Stop(Prc);
        }
        private void DoGetData1()
        {
            var currentUser = CL_HESABDARI.UCurrentUser();
            var userUnit = CL_Generaly.VAHED_OF_USER;

            Func<string, List<HAVALEHA_MODEL>> queryDatabase = query => dbms.DoGetDataSQL<HAVALEHA_MODEL>(query).ToList();

            void clearAndFillList(List<HAVALEHA_MODEL> data)
            {
                LIST_HAVALEHA?.Clear();
                data.ForEach(item => LIST_HAVALEHA.Add(item));
            }

            if (!CL_HESABDARI.LETSGO("FRSKB")) //فاکتور فروش سایر کاربران را بتواند ببیند // Can see sales invoices of other users
            {
                var dates = dbms.DoGetDataSQL<long>(
                    $@"SELECT TOP 100 PERCENT DATE_N 
                       FROM dbo.HEAD_LST 
                       WHERE TAG = 2 AND DEPATMAN = {userUnit} AND USER_NAME = '{currentUser}'
                       GROUP BY DATE_N 
                       ORDER BY DATE_N DESC").ToList();

                if (dates.Any() && Convert.ToDouble(Baseknow.CPI) > 0)
                {
                    int CPI = Convert.ToInt32(Baseknow.CPI);
                    long dateResult = dates.Count <= CPI ? dates[CPI - 1] : dates.FirstOrDefault();

                    var query = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, 
                                  dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, 
                                  dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                           FROM dbo.HEAD_LST
                           LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                           WHERE dbo.HEAD_LST.TAG = 2 AND 
                                 dbo.HEAD_LST.USER_NAME = N'{currentUser}' AND 
                                 dbo.HEAD_LST.DATE_N >= {dateResult}
                           ORDER BY dbo.HEAD_LST.NUMBER DESC";

                    clearAndFillList(queryDatabase(query));
                }
                else
                {
                    var query = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO,
                                  dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                  dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                           FROM dbo.HEAD_LST
                           LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                           WHERE dbo.HEAD_LST.TAG = 2 AND 
                                 dbo.HEAD_LST.USER_NAME = N'{currentUser}'
                           ORDER BY dbo.HEAD_LST.NUMBER DESC";

                    clearAndFillList(queryDatabase(query));
                }
            }
            else // Me.Refresh
            {
                string vs;

                if (CL_HESABDARI.LETSGO("DEPEMAL")) // اگر محدود به واحد خودش هست // Limited to their own unit
                {
                    var query = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO,
                                  dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                  dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                           FROM dbo.HEAD_LST
                           LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                           WHERE dbo.HEAD_LST.TAG = 2 AND
                                 dbo.HEAD_LST.DEPATMAN >= {userUnit}
                           ORDER BY dbo.HEAD_LST.NUMBER DESC";

                    clearAndFillList(queryDatabase(query));

                    if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // اگر محدود به  زير مجموعه خودش هست // Limited to their subordinates
                    {
                        vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));

                        if (string.IsNullOrEmpty(vs)) return;

                        var subQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO,
                                         dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                         dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                  FROM dbo.HEAD_LST
                                  LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                                  WHERE dbo.HEAD_LST.TAG = 2 AND 
                                        dbo.HEAD_LST.DEPATMAN >= {userUnit} AND 
                                        {vs}
                                  ORDER BY dbo.HEAD_LST.NUMBER DESC";

                        clearAndFillList(queryDatabase(subQuery));
                    }
                }
                else if (CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect)) // Limited to their subordinates
                {
                    vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));

                    if (string.IsNullOrEmpty(vs)) return;

                    var subQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO,
                                     dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                     dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                              FROM dbo.HEAD_LST
                              LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                              WHERE dbo.HEAD_LST.TAG = 2 AND {vs}
                              ORDER BY dbo.HEAD_LST.NUMBER DESC";

                    clearAndFillList(queryDatabase(subQuery));
                }
                else
                {
                    try
                    {
                        var query = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO
                                      dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                      dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO 
                               FROM dbo.HEAD_LST
                               LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                               WHERE dbo.HEAD_LST.TAG = 2 
                               ORDER BY dbo.HEAD_LST.NUMBER DESC";

                        clearAndFillList(queryDatabase(query));
                    }
                    catch (Exception)
                    {
                        var fallbackQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO,
                                              dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH,
                                              dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                                       FROM dbo.HEAD_LST
                                       LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                                       WHERE dbo.HEAD_LST.TAG = 2 
                                       ORDER BY dbo.HEAD_LST.NUMBER DESC 
                                       OPTION (QUERYTRACEON 2312)";

                        clearAndFillList(queryDatabase(fallbackQuery));
                    }
                }
            }
        }

        #region MyRegion
        private void DoGetData()
        {
            // Check if the user can view invoices from other users
            if (!CL_HESABDARI.LETSGO("FRSKB"))
            {
                LoadUserData();
            }
            else
            {
                LoadFilteredData();
            }
        }
        private void LoadUserData()
        {
            var sqlQuery = $"SELECT TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = 2) AND (DEPATMAN = {CL_Generaly.VAHED_OF_USER}) AND (USER_NAME = N'{CL_HESABDARI.UCurrentUser()}') GROUP BY DATE_N ORDER BY DATE_N DESC";
            var result = dbms.DoGetDataSQL<long>(sqlQuery).ToList();

            if (result.Count > 0 && Convert.ToDouble(Baseknow.CPI) > 0)
            {
                var dateResult = result.Count <= Convert.ToInt64(Baseknow.CPI)
                    ? result[Convert.ToInt32(Baseknow.CPI)]
                    : result.FirstOrDefault();

                LoadInvoiceData(dateResult);
            }
            else
            {
                LoadInvoiceData();
            }
        }
        private void LoadInvoiceData(long? dateResult = null)
        {
            string dateCondition = dateResult.HasValue ? $"AND dbo.HEAD_LST.DATE_N >= {dateResult}" : string.Empty;

            var sqlQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, 
                             dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                      FROM dbo.HEAD_LST
                      LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                      WHERE dbo.HEAD_LST.TAG = 2 
                      AND dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' 
                      {dateCondition}
                      ORDER BY dbo.HEAD_LST.NUMBER DESC";

            var invoices = dbms.DoGetDataSQL<HAVALEHA_MODEL>(sqlQuery).ToList();
            UpdateList(invoices);
        }
        private void LoadFilteredData()
        {
            string vs = string.Empty;
            bool canFilterByDepartment = CL_HESABDARI.LETSGO("DEPEMAL");
            bool canFilterByChart = CL_HESABDARI.LETSGO("chartfilter") && Convert.ToBoolean(Baseknow.mrcorrect);

            if (canFilterByDepartment)
            {
                LoadByDepartment();
            }
            else if (canFilterByChart)
            {
                vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                if (!string.IsNullOrEmpty(vs))
                {
                    LoadByChart(vs);
                }
            }
            else
            {
                LoadDefaultData();
            }
        }
        private void LoadByDepartment()
        {
            var sqlQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, 
                             dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                      FROM dbo.HEAD_LST
                      LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                      WHERE dbo.HEAD_LST.TAG = 2 AND dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER}
                      ORDER BY dbo.HEAD_LST.NUMBER DESC";

            var invoices = dbms.DoGetDataSQL<HAVALEHA_MODEL>(sqlQuery).ToList();
            UpdateList(invoices);
        }
        private void LoadByChart(string vs)
        {
            var sqlQuery = $@"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, 
                             dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO
                      FROM dbo.HEAD_LST
                      LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                      WHERE dbo.HEAD_LST.TAG = 2 AND dbo.HEAD_LST.DEPATMAN >= {CL_Generaly.VAHED_OF_USER} AND {vs}
                      ORDER BY dbo.HEAD_LST.NUMBER DESC";

            var invoices = dbms.DoGetDataSQL<HAVALEHA_MODEL>(sqlQuery).ToList();
            UpdateList(invoices);
        }
        private void LoadDefaultData()
        {
            var sqlQuery = @"SELECT dbo.HEAD_LST.NUMBER, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.DATE_N, 
                            dbo.HEAD_LST.TAH, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.ANBAR, dbo.HEAD_LST.FNUMCO 
                     FROM dbo.HEAD_LST
                     LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                     WHERE dbo.HEAD_LST.TAG = 2
                     ORDER BY dbo.HEAD_LST.NUMBER DESC";

            var invoices = dbms.DoGetDataSQL<HAVALEHA_MODEL>(sqlQuery).ToList();
            UpdateList(invoices);
        }
        private void UpdateList(List<HAVALEHA_MODEL> invoices)
        {
            LIST_HAVALEHA?.Clear();
            foreach (var invoice in invoices)
            {
                LIST_HAVALEHA.Add(invoice);
            }
        }


        #endregion

        private void CUST_NO_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CUST_NO_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void DoSearchinData()
        {
            var NUMBER_FLD = NUMBER_SR.Text.Trim().ToLowerInvariant();
            var CUST_N_FLD = CUST_NO_SR.Text.Trim().ToLowerInvariant();
            var DATE_N_FLD = DATE_N_SR.Text.ToRawTarikh().Trim().ToLowerInvariant();
            var FNUMCO_FLD = FNUMCO_SR.Text.Trim().ToLowerInvariant();
            var ANBAR_FLD = ANBAR_SR.Text.Trim().ToLowerInvariant();
            var TAH_FLD = TAH_SR.Text.Trim().ToLowerInvariant();
            var MOLAH_FLD = MOLAH_SR.Text.Trim().ToLowerInvariant();
            var SHARAYET_FLD = SHARAYET_SR.Text.Trim().ToLowerInvariant();

            IEnumerable<HAVALEHA_MODEL> LINQ_SEARCH = LIST_HAVALEHA;

            if (!string.IsNullOrEmpty(CUST_N_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.NAME is null) && x.NAME.ToLowerInvariant().Contains(CUST_N_FLD));

            if (!string.IsNullOrEmpty(NUMBER_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.NUMBER is null) && x.NUMBER.ToString().Contains(NUMBER_FLD));

            if (!string.IsNullOrEmpty(DATE_N_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.DATE_N is null) && x.DATE_N.ToString().Contains(DATE_N_FLD));

            if (!string.IsNullOrEmpty(FNUMCO_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.FNUMCO is null) && x.FNUMCO.ToString().Contains(FNUMCO_FLD));

            if (!string.IsNullOrEmpty(ANBAR_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.ANBAR is null) && x.ANBAR.ToString().Contains(ANBAR_FLD));

            if (!string.IsNullOrEmpty(TAH_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.TAH is null) && x.TAH.ToString().Contains(TAH_FLD));

            if (!string.IsNullOrEmpty(MOLAH_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.MOLAH is null) && x.MOLAH.ToString().Contains(MOLAH_FLD));

            if (!string.IsNullOrEmpty(SHARAYET_FLD))
                LINQ_SEARCH = LINQ_SEARCH.Where(x => !(x.SHARAYET is null) && x.SHARAYET.ToString().Contains(SHARAYET_FLD));

            DGR_HEADLST_HAVL_2.ItemsSource = LINQ_SEARCH.ToList();
        }

        private void REFLOAD_ALL_Click(object sender, RoutedEventArgs e)
        {
            DoGetData();
            DGR_HEADLST_HAVL_2.ItemsSource = LIST_HAVALEHA.ToList();
        }
        private void CLEANFILTERS_Click(object sender, RoutedEventArgs e)
        {
            NUMBER_SR.Clear();
            CUST_NO_SR.Clear();
            DATE_N_SR.Clear();
            FNUMCO_SR.Clear();
            ANBAR_SR.Clear();
            TAH_SR.Clear();
            MOLAH_SR.Clear();
            SHARAYET_SR.Clear();

            DGR_HEADLST_HAVL_2.ItemsSource = LIST_HAVALEHA.ToList();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dg = DGR_HEADLST_HAVL_2;
            UIElement uie = e.OriginalSource as UIElement;
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (((FrameworkElement)uie).Parent is DataGridCell || uie is DataGridCell) //Is Foucs really inside the DataGrid
                {
                    if (!(DGR_HEADLST_HAVL_2.SelectedItem is null))
                    {
                        //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
                        Process Prc = ProcLoader.Start();
                        var HAVALEROW = (HAVALEHA_MODEL)DGR_HEADLST_HAVL_2.SelectedItem;
                        HEAD_LST_HAVL hEAD_LST_HAVL = new HEAD_LST_HAVL((double)HAVALEROW.NUMBER);
                        Close();
                        hEAD_LST_HAVL.Show();
                        //ProcLoader.Stop(Prc);
                        ProcLoader.Stop(Prc);
                        return;
                    }
                }
                e.Handled = true;
                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        private void NUMBER_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NUMBER_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void DATE_N_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DATE_N_SR.Text.ToRawTarikh()))
            {
                DoSearchinData();
            }
        }
        private void FNUMCO_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FNUMCO_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void ANBAR_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ANBAR_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void TAH_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TAH_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void MOLAH_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MOLAH_SR.Text))
            {
                DoSearchinData();
            }
        }
        private void SHARAYET_SR_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SHARAYET_SR.Text))
            {
                DoSearchinData();
            }
        }

        private void DGR_HEADLST_HAVL_2_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void DGR_HEADLST_HAVL_2_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(DGR_HEADLST_HAVL_2.SelectedItem is null))
            {
                //Process Prc = Process.Start("C:\\correct\\prc\\prc.exe", "1354");
                Process Prc = ProcLoader.Start();
                var HAVALEROW = (HAVALEHA_MODEL)DGR_HEADLST_HAVL_2.SelectedItem;
                HEAD_LST_HAVL hEAD_LST_HAVL = new HEAD_LST_HAVL((double)HAVALEROW.NUMBER);
                Close();
                hEAD_LST_HAVL.Show();
                //ProcLoader.Stop(Prc);
                ProcLoader.Stop(Prc);
                return;
            }
        }

        private void BTN_SEARCH_Click(object sender, RoutedEventArgs e)
        {
            DoSearchinData();
        }
    }
}
