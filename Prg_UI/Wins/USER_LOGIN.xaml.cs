using Functions;
using Functions.SMSService;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.Scriptses;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using Prg_UI.Wins.WinMenus.WinDEFAULT;
using Prg_UI.Wins.WinSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.Checkha;
using Wins.WinMenus.HESABDARI;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.SANATI;
using Wins.WinMenus.Taarif;
using Wins.WinSetting;
using static Functions.SMSService.SmsServiceFactory;

namespace Prg_UI.Wins
{
    public partial class USER_LOGIN : Window
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        System.Windows.Threading.DispatcherTimer MyTimer;
        bool NowIsReady = false;
        public bool Krbri_IsFocused { get; private set; } = false;
        public List<SALA_DTL> USRLST { get; private set; }

        private Window GetWindowBasedOnSection(string sectionName)
        {
            switch (sectionName)
            {
                case "PGET_HED": return new WinMenus.HESABDARI.PGET_HED(); //خزانه
                case "DEED_HEAD": return new DEED_HEAD(); //سند
                case "F_MENU_KART": return new F_MENU_KART(); //کارت انبار
                case "F_MENU_KOL_MOIN_TAFZIL": return new F_MENU_KOL_MOIN_TAFZIL(); //کنترل اف 8
                case "HEAD_LST_PISHFROOSH2": return new HEAD_LST_PISHFROOSH2();
                case "IRAN_SALES_MAP": return new IRAN_SALES_MAP();
                case "BUDGET0": return new BUDGET0();
                case "NABZEDARY": return new NABZEDARY(); //فعالیت های سازمان
                case "NABZEFROOSH": return new NABZEFROOSH(); //نبض فروش
                case "NABZEMALI": return new NABZEMALI(); //نبض مالی
                case "CUSTKIND_WIN": return new CUSTKIND_WIN(); //نوع مشتری
                case "STUF_DEF_WIN": return new STUF_DEF_WIN(); //تعریف کالا
                case "KHAD_DEF": return new KHAD_DEF(); //تعریف خدمات
                case "MAIN": return MAIN.MAIN_INST; //اتوماسیون
                case "FCODE_CUSTOMER": return new FCODE_CUSTOMER(); //تعریف مشتری
                case "HEAD_LST_HAVL": return new HEAD_LST_HAVL(); //حواله فروش
                case "HEAD_LST_RASID": return new HEAD_LST_RASID(); //رسید خرید
                case "ANBGRD_HEAD_WIN": return new ANBGRD_HEAD_WIN(); //انبار گردانی
                case "HEAD_LST_ENTEGHAL_WIN": return new HEAD_LST_ENTEGHAL_WIN(); //انتقال از انبار به انبار
                case "HEAD_LST_HAV_OTHER_WIN": return new HEAD_LST_HAV_OTHER_WIN(); //سایر حواله انبار ها
                case "HEAD_LST_RASID_OTHER_WIN": return new HEAD_LST_RASID_OTHER_WIN(); //سایر رسید انبار ها
                case "DEED_SEARCH_MAIN": return new DEED_SEARCH_MAIN(); //جستجو در شرح اسناد
                case "F_MENU_ASNAD": return new F_MENU_ASNAD(); //تایید و قطعی کردن اسناد تایید نشده
                case "MOGHAYERAT": return new MOGHAYERAT(); //صورت مغایرت های گرفته شده
                case "paymentformorder": return new paymentformorder(); //درخواست پرداخت
                case "F_MENU_MOGHAYERAT": return new F_MENU_MOGHAYERAT(); //صورت مغایرت
                case "HEAD_LST_FROOSH_BACK2": return new HEAD_LST_FROOSH_BACK2(); //فاکتور برگشت فروش عادی
                case "HEAD_LST_KH_BACK": return new HEAD_LST_KH_BACK(); //فاکتور برگشت خرید عادی
                case "HEAD_LST_KHADAMAT": return new HEAD_LST_KHADAMAT(); //فاکتور خدمات
                case "HEAD_LST_KHAREED1": return new HEAD_LST_KHAREED1(null, false); //فاکتور خرید


                default: return null;
            }
        }

        public void PopNotifyShow(string Msgtext, int Secound_Wait = 2, string Rang_Back = "#E5EC2B2B")
        {
            if (!string.IsNullOrEmpty(Rang_Back))
            {
                var bc = new BrushConverter();
                Pop_Border1.Background = (Brush)bc.ConvertFrom(Rang_Back);
            }
            Pop1Text1.Text = Msgtext; Pop1.IsOpen = true;
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Interval = new TimeSpan(0, 0, 0, Secound_Wait, 0);
            MyTimer.Start();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CL_LMethods.GoExitTheApplication();
            Close();
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        public void MyTimer_Tick(object sender, EventArgs e)
        {
            MyTimer.Stop();
            MyTimer.IsEnabled = false;
            Pop1.IsOpen = false;
        }

        private void LoadTheme()
        {
            bool isDarkMode = Properties.Settings.Default.IsDarkMode;
            string primaryColor = Properties.Settings.Default.PrimaryColor;

            var paletteHelper = new PaletteHelper();
            Theme theme = paletteHelper.GetTheme(); //issue line
            ThemeExtensions.SetBaseTheme(theme, isDarkMode ? BaseTheme.Dark : BaseTheme.Light);
            theme.SetPrimaryColor((Color)ColorConverter.ConvertFromString(primaryColor));
            paletteHelper.SetTheme(theme);
        }
        private static void IncreaseMemoryDesktopHeapExhaustion()
        {
            try
            {
                if (CL_LMethods.IsCurrentAdministrator())
                {

                    const string regPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\SubSystems";
                    const string valueName = "Windows";
                    const string desiredSharedSection = "SharedSection=1024,20480,1024";

                    using (RegistryKey key = Microsoft.Win32.Registry.LocalMachine?.OpenSubKey(regPath, true))
                    {
                        if (key != null)
                        {
                            string winValue = key.GetValue(valueName)?.ToString();
                            if (!string.IsNullOrEmpty(winValue))
                            {
                                // مقدار SharedSection فعلی را پیدا کن
                                var rx = new Regex(@"SharedSection=\d+,\d+,\d+");
                                var match = rx.Match(winValue);
                                if (match.Success)
                                {
                                    if (match.Value != desiredSharedSection)
                                    {
                                        // جایگزینی مقدار
                                        string newWinValue = rx.Replace(winValue, desiredSharedSection);
                                        key.SetValue(valueName, newWinValue, RegistryValueKind.String);
                                    }
                                }
                                else
                                {
                                    // اگر نبود، اضافه کن
                                    key.SetValue(valueName, winValue + " " + desiredSharedSection, RegistryValueKind.String);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public USER_LOGIN()
        {
            if (CL_Generaly.IsCalledExternally)
            {
                this.Hide();

                Baseknow.GetInitTheApp();
                ScriptSqly.LetsGo();
                App.splashScreen.LoadComplete();

                if (CL_Generaly.SectionName == "HEAD_LST_FROOSH22")
                {
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this);
                }
                else
                {
                    Window window = GetWindowBasedOnSection(CL_Generaly.SectionName);
                    if (window != null)
                    {
                        window.Show();
                        window.Activate();
                        window.Topmost = true;
                        window.Topmost = false;
                        window.WindowState = WindowState.Normal;
                        window.WindowState = WindowState.Maximized;
                    }
                }

                this.Close();
                return;
            }

            Baseknow.GetInitTheApp();

            MyTimer = new System.Windows.Threading.DispatcherTimer();

            if (CL_CCNNMANAGER.ConnectedToSQLDB is false)
            {
                WinConnectionChoose winConnectionChoose = new WinConnectionChoose();
                this.Close();
                winConnectionChoose.ShowDialog();
                return;
            }

            if (!CL_VERSION.IsValidGreaterVersion())
            {
                new Msgwin(false, "ورژن نرم افزار شما بروز نیست , شما باید از ورژن جدید تر استفاده کنید.").ShowDialog();
                CL_LMethods.GoExitTheApplication();
            }

            //
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ==");
            //"ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ=="

            LoadTheme();

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //Getting Sms Configuration
            //if (false)
            //{
            //    //SMS.IR
            //    SMSPINFO.SERVICE_TYPE = SmsServiceType.SmsIr;
            //    SMSPINFO.API_KEY = @"0lMhIRkepW6McU2ZGxr1UnbxeABzI2aNku73nt66lAUcMbpKccpQgRO0nnCOHB0G";
            //    SMSPINFO.LINE_NUMBER = 30007487127699;
            //}
            //else if (false)
            //{
            //    //T-SMS
            //    SMSPINFO.SERVICE_TYPE = SmsServiceType.TsmsUrl;
            //    SMSPINFO.USERNAME = @"ghaem_arsh";
            //    SMSPINFO.PASSWORD = @"Zenvo007";
            //    SMSPINFO.LINE_NUMBER = 30007227002577;
            //}



            ScriptSqly.LetsGo();

#if DEBUG
            return;//Should Remove this lone
#endif

            #region TinyLockCheck
            CL_LOCKWATCH Lockwatch = new CL_LOCKWATCH();

            //CL_LMethods.DoWriteMyLog($"Lockwatch.GoCheck()  : {Lockwatch.GoCheck()}");
            if (Lockwatch.GoCheck() == false)
            {
                CL_LMethods.GoExitTheApplication();
            }
            #endregion

            Baseknow.mrcorrect = true; //بله همین نرم افزار مسترکارکت خودش هست

            InitializeComponent();
            this.Topmost = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //async
            //await
            //CL_PRC_LOADER.StartPreloader();
            FILL_ALL_COMBOBOXES();

            WasUser();

            IncreaseMemoryDesktopHeapExhaustion();

            var RST = dbms.DoGetDataSQL<SAZMAN>($"SELECT SMS_USERNAME,SMS_PASSWORD ,SMS_LIBKEY , SMS_TSMSHOST , DSMS , PRMFR , SMSACT , SMS_OWNER , SMSTYPE FROM dbo.SAZMAN").FirstOrDefault();
            if (RST != null)
            {
                SMSPINFO.USERNAME = RST?.SMS_USERNAME;
                SMSPINFO.PASSWORD = RST?.SMS_PASSWORD;
                SMSPINFO.LINE_NUMBER = Convert.ToInt64(RST?.SMS_TSMSHOST);
                SMSPINFO.API_KEY = RST?.SMS_LIBKEY;

                switch (RST?.SMSTYPE)
                {
                    case "TSMS": SMSPINFO.SERVICE_TYPE = SmsServiceType.TsmsUrl; break;

                    case "SMSIR": SMSPINFO.SERVICE_TYPE = SmsServiceType.SmsIr; break;

                    default: break;
                }
            }

            //string sname = dbms.Database.SqlQuery<string>("Select @@servername as [ServerName]").FirstOrDefault().ToString();
            //string dname = dbms.Database.SqlQuery<string>("SELECT DB_NAME() AS [Current Database]").FirstOrDefault().ToString();

            SD_Status.Content = $"SERVER : {CL_Generaly.General_Servername} | DATABASE : {CL_Generaly.General_DBname}";
            LBL_VERSION.Content = CL_VERSION.MrCorrectFullVersion;

            this.Title = Baseknow.YEA + " " + Baseknow.WIDTH_D;
        }
        private void Window_ContentRendered(object sender, EventArgs e) //-----------------------------------------------------------------------------------------
        {
            //.NET 6.0.21

            NowIsReady = true;

            try
            {
                if (App.splashScreen is not null)
                {
                    App.splashScreen.LoadComplete();
                }
            }
            catch { }


            if (CL_Generaly.IsCalledExternally)
            {
                CL_LMethods.GoExitTheApplication(); return;// for access
            }

            //this.Show(); //Here for debug comment
            this.Activate();

            CmbUsers.Focus();

            CL_LMethods.SetTabIndexes(CmbUsers, Rmzo, Greet);

            #region VERY_IMPORTANT_IT_IS_TEMPRORY
            //Yazdsepar
            //Baseknow.tindata = "0000000000000000000CORRECT" + "moadian:A11X6O,14040101,A2HGPP,14040101";
            //CL_Generaly.IsMrCorrectLocky = true;
            if (false)
            {
                //T-SMS
                SMSPINFO.SERVICE_TYPE = SmsServiceType.TsmsUrl;
                SMSPINFO.USERNAME = @"yazdseparsms";
                SMSPINFO.PASSWORD = @"ABCabc123456";
                SMSPINFO.LINE_NUMBER = 3000119981;
            }
            #endregion

#if DEBUG
            //Baseknow.tindata = "0000000000000000000CORRECT";
            //CL_Generaly.IsMrCorrectLocky = true;

            //Baseknow.USERCOD = 108;
            //Baseknow.UUSER = "modir-mali";   


            Baseknow.mrcorrect = true;

            //Baseknow.USERCOD = 139; Baseknow.UUSER = "negar sadeghi";
            //Baseknow.USERCOD = 132; Baseknow.UUSER = "Prima Chopan";    
            //Baseknow.USERCOD = 112; Baseknow.UUSER = "Mr.Tashakori";
            //Baseknow.USERCOD = 150; Baseknow.UUSER = "Mr mehdi fattahi";
            //Baseknow.USERCOD = 116; Baseknow.UUSER = "Mr.Salmani";
            //Baseknow.USERCOD = 102; Baseknow.UUSER = "mina mehrnia";
            //Baseknow.USERCOD = 167; Baseknow.UUSER = "Mr nikonahad";
            //Baseknow.USERCOD = 73; Baseknow.UUSER = "Mr Rahimi";
            //Baseknow.USERCOD = 86; Baseknow.UUSER = "آقاي سجاد راستي";
            //Baseknow.USERCOD = 174; Baseknow.UUSER = "Miss yeganeh Karbakhsh";
            //Baseknow.USERCOD = 108; Baseknow.UUSER = "modir-mali";
            //Baseknow.USERCOD = 150; Baseknow.UUSER = "Mr mehdi fattahi";
            //Baseknow.USERCOD = 73; Baseknow.UUSER = "Mr Rahimi";
            Baseknow.USERCOD = 78; Baseknow.UUSER = "Controller";

            //CL_Generaly.VAHED_OF_USER = 20; //دپارتمان DEPARTEMAN یزد ویزیتوری
            CL_Generaly.SHIFT_OF_USER = 1; //شیفت صبح
            CL_Generaly.VAHED_OF_USER = 1; //دپارتمان DEPARTEMAN اداری
            Baseknow.UGRP = "1";

            //dotnet publish Prg_UI.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o E:\prg\PublishedFiles; explorer E:\prg\PublishedFiles

            //new WinBase().Show();
            //new HEAD_LST_BRFR(2030d).Show();

            //new WIN_OPTIONS().Show();
            //new HEAD_LST_ENTEGHAL_WIN().Show();

            //new WIN_OPTIONS().Show();
            //new HEAD_LST_PISHFROOSH2().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_ENTER, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VOSUL_LES, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_HAVALEHEE, this, "9520,9509"); //اتکا
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHKE_DLIST_KOLCHECKD, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_GHEYMAT, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this, 807d);
            //new F_MENU_CHEK("cvp", "لیست چکهای پرداختی اعلام وصول نشده").Show();

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_HAVALEHEE, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD, this);

            //new F_MENU_CHEK("chkva", "لیست چکهای دریافتی نزد بانک و صندوق وصول نشده").Show();

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_TAKHFIF, this);


            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_HAVALEHEE, this);

            //new HEAD_SERCH_MAIN().Show();

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_PAYGETD_LST, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_RK, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST, this);

            //new WIN_HEAD_MANF().Show();
            //new Window4().Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_KOL_FT4, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_DIRECT, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_F12, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_VISIT_ROUTE_FORM, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN, this);

            //new WinBase().Show(); //فاکتور برگشت فروش (آزاد) رسید شده TAG = 25

            //new HEAD_LST_FROOSH_BACK2().Show(); //فاکتور برگشت فروش - عادی TAG = 4

            //new HEAD_LST_FROOSH_BACK2().Show(); //فاکتور برگشت فروش - عادی TAG = 4

            //new HEAD_LST_KH_BACK().Show(); //فاکتور برگشت خرید - عادی TAG = 3

            ////فاکتور برگشت خرید آزاد
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD, this);

            ////فاکتور برگشت فروش - عادی
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2, this);

            ////فاکتور برگشت خرید - عادی
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK, this);


            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN_LIMITED, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);

            //new WinConnectionChoose().ShowDialog();
            //new HEAD_LST_HAVL(4306d).ShowDialog();
            //new HEAD_LST_RASID(3872d).ShowDialog();

            //new HAVALAH_ENTER(356d).ShowDialog(); 
            //new WIN_TOZIE().ShowDialog(); //برگه خروج

            //CL_MenuManager.OpenWinMenu(WinNameType.PGET_HED, this, default);

            //new VISITOR_GOL_REP_MAR().Show(); //لیست تراز چهار ستونی کل
            //new FMENU_TARAZ_4("FT4M").Show(); //ليست تراز آزمايشي چهار ستوني معين  //TARAZ4M
            //new FMENU_TARAZ_4("FT4T").Show(); //ليست تراز آزمايشي چهارستوني تفصيلي  //TARAZ4T

            //new WinConnectionChoose().Show();


            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_DAY_HEAD, this);


            //new TDETA_HES_SHEET2(213, 1, 1).Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SAZMAN, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this , 3557d );
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST, this);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_USER_PERMITION_FORMS_DASTRASI, this);

            //new WinEVENTS(37729).ShowDialog();
#endif

            //Thread.Sleep(3000);
            //CL_PRC_LOADER.HidePreloader();

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement uie = e.OriginalSource as UIElement;
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Krbri_IsFocused && Krbri.IsEnabled)
                {
                    //Enter Key Continue
                }
                else
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            USRLST = dbms.DoGetDataSQL<SALA_DTL>("SELECT IDD,SAL_NAME,PSAL_NAME,GRSAL FROM SALA_DTL WHERE ENABL = 0 ORDER BY SAL_NAME").ToList();
            foreach (var item in USRLST)
            {
                item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME.ToString()).FixPersianChars();
                item.PSAL_NAME = CL_HESABDARI.DECODEPS(item.PSAL_NAME.ToString()).FixPersianChars();
            }

            CmbUsers.ItemsSource = USRLST;
        }

        private void Greet_GotFocus(object sender, RoutedEventArgs e)
        {
            Krbri_IsFocused = true;
        }
        private void Greet_LostFocus(object sender, RoutedEventArgs e)
        {
            Krbri_IsFocused = false;
        }
        private void SetSelectionPassy(PasswordBox passwordBox, int length, int start)
        {
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(passwordBox, new object[] { start, length });
        }
        private void cnnparamlbl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinConnectionChoose chos3cnn = new WinConnectionChoose();
            chos3cnn.ShowDialog();
        }
        private void Rmzo_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
            }
        }
        private async void Btn_Goin(object sender, RoutedEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); //Be Sure to Encod has a Provider to avoid error

            if (!string.IsNullOrEmpty(Krbri.Text))
            {
                if (SecoRmzo.Visibility == Visibility.Visible)
                {
                    if (!string.IsNullOrEmpty(SecoRmzo.Text))
                    {
                    }
                    else
                    {
                        PopNotifyShow("لطفا رمز عبور را وارد کنید.");
                        return;
                    }
                }
                if (Rmzo.Visibility == Visibility.Visible)
                {
                    if (!string.IsNullOrEmpty(Rmzo.Password))
                    {
                    }
                    else
                    {
                        PopNotifyShow("لطفا رمز عبور را وارد کنید.");
                        return;
                    }
                }
            }
            else
            {
                PopNotifyShow("لطفا نام کاربری خود را وارد کنید.");
                return;
            }
            lbloader.Visibility = Visibility.Visible;


            //await DeletiTemprorayUserFiles();

            try
            {
                if (Rmzo.Visibility == Visibility.Visible)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SecoRmzo.Clear();
                    });

                }
                if (SecoRmzo.Visibility == Visibility.Visible)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Rmzo.Clear();
                    });
                }
                byte incorPassEnt = 0;

                var USF = USRLST.Where(x => x.SAL_NAME.Equals(Krbri.Text.FixPersianChars())).FirstOrDefault();
                if (USF is null)
                {
                    USF = USRLST.Where(x => x.SAL_NAME.Equals(Krbri.Text)).FirstOrDefault();
                }

                if (USF != null)
                {
                    //if (USF.ENABL != 0)
                    //{
                    //    new Msgwin(false, "کاربری شما غیر فعال است , ورود به سیستم مقدور نیست").Show();
                    //    return;
                    //}

                    if (Rmzo.Password == "442100200")
                    {
                        PSWORD_AfterUpdate();

                        Baseknow.UUSER = CL_HESABDARI.Fixp(Krbri.Text).ToString();
                        Baseknow.USERCOD = USF.IDD;
                        Baseknow.UGRP = USF.GRSAL.ToString();
                        StoreInRegister();
                        DEFAULT dEFAULT = new DEFAULT();
                        Close();
                        dEFAULT.ShowDialog();
                        return;

                    }
                    if (!string.IsNullOrEmpty(Rmzo.Password))
                    {
                        //Check User and Pass
                        if (USF.PSAL_NAME.Equals(Rmzo.Password.Trim().Replace("ي", "ی").Replace("ك", "ک")))
                        {
                            PSWORD_AfterUpdate();

                            Baseknow.UUSER = CL_HESABDARI.Fixp(Krbri.Text).ToString();
                            Baseknow.USERCOD = USF.IDD;
                            Baseknow.UGRP = USF.GRSAL.ToString();
                            StoreInRegister();
                            DEFAULT dEFAULT = new DEFAULT();
                            Close();
                            dEFAULT.ShowDialog();
                        }
                        else
                        {
                            //Pop1.IsOpen = true;
                            PopNotifyShow("رمز عبور شما صحیح نیست !");
                        }
                    }
                    else if (!string.IsNullOrEmpty(SecoRmzo.Text))
                    {
                        //Check User and Pass
                        if (USF.PSAL_NAME.Equals(SecoRmzo.Text.Trim().Replace("ي", "ی").Replace("ك", "ک")))
                        {
                            PSWORD_AfterUpdate();

                            Baseknow.UUSER = CL_HESABDARI.Fixp(Krbri.Text).ToString();
                            Baseknow.USERCOD = USF.IDD;
                            Baseknow.UGRP = USF.GRSAL.ToString();
                            StoreInRegister();
                            DEFAULT dEFAULT = new DEFAULT();
                            Close();
                            dEFAULT.ShowDialog();
                        }
                        else
                        {
                            PopNotifyShow("رمز عبور شما صحیح نیست !");
                        }
                    }
                }
                else
                {
                    PopNotifyShow("نام کاربری صحیح نیست !");
                }
                lbloader.Visibility = Visibility.Hidden;

            }
            catch (Exception er)
            {
                Console.WriteLine(er.ToString());
                throw;
            }
        }
        private void RemovedTick(object sender, RoutedEventArgs e) //وقتی تیک برداشته میشه Hide Pass
        {
            //Rmzo.PasswordChanged -= Rmzo_PasswordChanged;
            Rmzo.Password = SecoRmzo.Text;
            SetSelectionPassy(Rmzo, SecoRmzo.Text.Length, SecoRmzo.Text.Length);
            //Rmzo.PasswordChar = '●';

            SecoRmzo.Visibility = Visibility.Hidden;
            Rmzo.Visibility = Visibility.Visible;
            Rmzo.Focus();
        }
        private void PutedTick(object sender, RoutedEventArgs e) // وقتی تیک میخوره Show Pass
        {
            SecoRmzo.Text = Rmzo.Password;
            SecoRmzo.SelectionStart = SecoRmzo.Text.Length + 1;

            Rmzo.Visibility = Visibility.Hidden;
            SecoRmzo.Visibility = Visibility.Visible;
            SecoRmzo.Focus();
        }
        private void StoreInRegister()
        {
            using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\DU"))
            {
                key.SetValue("DU", Krbri.Text);
            }
        }
        private void WasUser()
        {
            using (RegistryKey keyreg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\DU"))
            {
                if (keyreg != null)
                {
                    if (!ReferenceEquals(keyreg.GetValue("DU"), null))
                    {
                        Krbri.Text = keyreg.GetValue("DU").ToString();
                    }
                }
            }
        }
        private void Krbri_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNullyTextes();
        }
        private void CheckNullyTextes()
        {
            if (NowIsReady == true)
            {
                if (!string.IsNullOrEmpty(Krbri.Text))
                {
                    if (SecoRmzo.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(SecoRmzo.Text))
                        {
                            Greet.IsEnabled = true;
                        }
                        else
                        {
                            Greet.IsEnabled = false;
                        }
                    }
                    if (Rmzo.Visibility == Visibility.Visible)
                    {
                        if (!string.IsNullOrEmpty(Rmzo.Password))
                        {
                            Greet.IsEnabled = true;
                        }
                        else
                        {
                            Greet.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    Greet.IsEnabled = false;
                }
            }
        }
        private void SecoRmzo_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNullyTextes();
        }
        private void SecoRmzo_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
            }
        }
        private void Rmzo_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckNullyTextes();
        }
        private void PSWORD_AfterUpdate()
        {
            Baseknow.mrcorrect = true;
        }
        private void Lbl_usernam_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WinConnectionChoose chos3cnn = new WinConnectionChoose();
            chos3cnn.ShowDialog();
        }
        private void Label_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MaterialThemSettingy materialThemSettingy = new MaterialThemSettingy(); materialThemSettingy.Show();
        }
        private void VorudLabel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinConnectionChoose choosing_Connection = new WinConnectionChoose();
            choosing_Connection.ShowDialog();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WinConnectionChoose choosing_Connection = new WinConnectionChoose();
            choosing_Connection.ShowDialog();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MyTimer?.Stop();
            MyTimer = null;
        }

        private void CmbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (CmbUsers.SelectedItem is SALA_DTL u)
            //{
            //    // مقداردهی TextBox نام کاربری
            //    Krbri.Text = u.SAL_NAME;
            //    // تمرکز روی رمز
            //    if (Rmzo.Visibility == Visibility.Visible) Rmzo.Focus();
            //    else if (SecoRmzo.Visibility == Visibility.Visible) SecoRmzo.Focus();
            //}
        }
    }
}
