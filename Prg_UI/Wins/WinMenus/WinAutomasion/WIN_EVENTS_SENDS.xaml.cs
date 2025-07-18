using AUTO_BAZ.Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Syncfusion.Data.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;

namespace Wins.WinMenus.WinAutomasion
{
    public partial class WIN_EVENTS_SENDS : Window
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

        public ObservableCollection<TASKS> TASK_DATA { get; set; } = new ObservableCollection<TASKS>();
        public ObservableCollection<CutsomPeriority_Model> PERIORITY_COMBO_DATA { get; set; } = new ObservableCollection<CutsomPeriority_Model>();
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; set; } = new ObservableCollection<CutsomStatus_Model>();
        //public ObservableCollection<CUST_HESAB> COMP_COD_DATA { get; set; } = new ObservableCollection<CUST_HESAB>();

        public ObservableCollection<COMBOYMODEL> SKID_ITEMS { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public WIN_EVENTS_SENDS(string? openargs = null)
        {
            InitializeComponent();

            if (openargs != null)
            {
                OpenArgs = openargs;
            }

            this.DataContext = this;
        }
        public string OpenArgs { get; set; } = Baseknow.UUSER.ToStringNullSafe();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            ReGetData();

            GR_NAV_DATAGRID.ReGetDataAction = () => //Realod Data
            {
                ReGetData();
            };
        }

        private void ReGetData()
        {
            TASK_DATA?.Clear();
            var RecordSource = dbms.DoGetDataSQL<TASKS>($@"SELECT dbo.EVENTS.IDNUM, dbo.EVENTS.IDD, dbo.EVENTS.EVENTS, dbo.EVENTS.STDATE, dbo.EVENTS.STTIME, dbo.EVENTS.USERNAME, dbo.EVENTS.COMPANY, dbo.EVENTS.SUMTIME, dbo.EVENTS.pic, dbo.EVENTS.skid, dbo.EVENTS.num, dbo.EVENTS.tg, dbo.TASKS.GR,
                                                           dbo.TASKS.PERSONEL, dbo.TASKS.TASK, dbo.TASKS.PERIORITY, dbo.TASKS.STATUS, dbo.TASKS.STDATE AS STDATEm, dbo.TASKS.STTIME AS STTIMEm, dbo.TASKS.ENDATE, dbo.TASKS.ENTIME, dbo.TASKS.USERNAME AS USERNAMEm,
                                                           dbo.TASKS.COMP_COD, dbo.CUST_HESAB.NAME, dbo.TASKS.SUMTIME AS SUMTIMEm, dbo.TASKS.pic AS picm, dbo.TASKS.ss, dbo.TASKS.skid AS skidm, dbo.TASKS.num AS numm, dbo.TASKS.tg AS tgm, dbo.TASKS.CTIM, dbo.TASKS.USERCO, dbo.TASKS.SEE, dbo.TASKS.SEET
                                                           FROM dbo.TASKS
                                                                INNER JOIN dbo.EVENTS ON dbo.TASKS.IDNUM=dbo.EVENTS.IDNUM
                                                                LEFT OUTER JOIN dbo.CUST_HESAB ON dbo.TASKS.COMP_COD=dbo.CUST_HESAB.hes
                                                           WHERE(dbo.EVENTS.USERNAME=N'{this.OpenArgs}' OR dbo.TASKS.USERNAME = N'{this.OpenArgs}')
                                                           ORDER BY TASKS.IDNUM
                                                           OPTION(FORCE ORDER, LOOP JOIN, HASH JOIN, ORDER GROUP)").ToList();
            foreach (var item in RecordSource)
            {
                TASK_DATA?.Add(item);
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //کبموباکس مجری
            var rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>($"SELECT SAL_NAME, SUBUSERCO, USERCO FROM dbo.CHARTSAZMANI LEFT OUTER JOIN SALA_DTL ON CHARTSAZMANI.SUBUSERCO=SALA_DTL.IDD WHERE CHARTSAZMANI.USERCO={Baseknow.USERCOD}").ToList();
            bool IsHameh = true;
            if (IsHameh)
            {
                rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>("SELECT SAL_NAME, PSAL_NAME, GRSAL, ENABL, IDD as USERCO FROM SALA_DTL WHERE (ENABL=0)").ToList();
                foreach (var rows in rst_personel)
                {
                    if (!string.IsNullOrEmpty(rows?.SAL_NAME))
                    {
                        rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
                    }
                }
            }

            //تماس گیرنده
            //COMP_COD_CMB.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB>($@"SELECT hes, NAME FROM dbo.CUST_HESAB").ToList();

            //کمبوباکس مجری در دیتاگرید
            PERSONEL_CMB.ItemsSource = rst_personel;

            //وضعیت در دیتاگرید
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "انجام نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "انجام شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "لغو شده" });

            //اولویت در دیتاگرید
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 1, PERIORITY_NAME = "فوری" });
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 2, PERIORITY_NAME = "معمولی" });

            SKID_ITEMS = new ObservableCollection<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 0, NAME = "سند حسابداری" },
                new COMBOYMODEL { ID = 1, NAME = "رسید خرید" },
                new COMBOYMODEL { ID = 2, NAME = "حواله فروش" },
                new COMBOYMODEL { ID = 3, NAME = "برگشت خرید" },
                new COMBOYMODEL { ID = 4, NAME = "برگشت فروش" },
                new COMBOYMODEL { ID = 20, NAME = "پیش فاکتور" },
                new COMBOYMODEL { ID = 100, NAME = "درخواست پرداخت" },
                new COMBOYMODEL { ID = 13, NAME = "فاکتور فروش" },
                new COMBOYMODEL { ID = 12, NAME = "فاکتور خرید" },
                new COMBOYMODEL { ID = 32, NAME = "فاکتور خرید صادرات" },
                new COMBOYMODEL { ID = 6, NAME = "انتقال کالا از انبار به انبار" },
                new COMBOYMODEL { ID = 90, NAME = "پذیرش تعمیرگاه" },
                new COMBOYMODEL { ID = 25, NAME = "فاکتور برگشت فروش آزاد" },
                new COMBOYMODEL { ID = 24, NAME = "سایر رسید انبارها" },
                new COMBOYMODEL { ID = 26, NAME = "سایر حواله انبار ها" },
                new COMBOYMODEL { ID = 27, NAME = "برگشت خريد ازاد" },
                new COMBOYMODEL { ID = 33, NAME = "فروش صادراتی" },
                new COMBOYMODEL { ID = 34, NAME = "خزانه داری" },
                new COMBOYMODEL { ID = 35, NAME = "سفارش" },
                new COMBOYMODEL { ID = 36, NAME = "درخواست خريد" },
                new COMBOYMODEL { ID = 37, NAME = "مرخصی" },
                new COMBOYMODEL { ID = 38, NAME = "حواله خروج سایر" },
                new COMBOYMODEL { ID = 39, NAME = "حواله خروج مواد" },
                new COMBOYMODEL { ID = 40, NAME = "تجهیزات PM" },
                new COMBOYMODEL { ID = 41, NAME = "درخواست کالا از انبار" },
                new COMBOYMODEL { ID = 42, NAME = "اعلام خرابی" },
                new COMBOYMODEL { ID = 101, NAME = "اتوماسیون" }
            };

            skidColumn.ItemsSource = SKID_ITEMS;
        }
    }
}
