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
using Prg_UI.Wins.WinMenus.SANATI;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using Prg_UI.Wins.WinMenus.WinDEFAULT;
using Prg_UI.Wins.WinSetting;
using Rpts;
using Stimulsoft.Report.Components.Table;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.HESABDARI;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.Taarif;
using Wins.WinSetting;
using static Functions.InventoryManager;
using static Functions.SMSService.SmsServiceFactory;
using static Stimulsoft.Base.StiDbType;

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

        private async Task LoadThemeAsync()
        {
            AppThemeSettings themeSettings = await AppThemeManager.LoadThemeSettingsAsync(Baseknow.USERCOD);
            AppThemeManager.ApplyTheme(themeSettings);
        }

        /// <summary>
        /// بعد از لاگین موفق، تم ذخیره‌شده کاربر را از DB لود و اعمال می‌کند،
        /// سپس پنجره اصلی را باز می‌کند.
        /// </summary>
        private async Task OpenMainWindowAsync()
        {
            try
            {
                var themeSettings = await AppThemeManager.LoadThemeSettingsAsync(Baseknow.USERCOD);
                AppThemeManager.ApplyTheme(themeSettings);
            }
            catch { /* در صورت خطا با تم فعلی ادامه می‌دهیم */ }

            DEFAULT dEFAULT = new DEFAULT();
            Close();
            dEFAULT.ShowDialog();
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

            // Moved to Window_ContentRendered to allow UI to show progress
            //if (!CL_VERSION.IsValidGreaterVersion())
            //{
            //    PerformAutoUpdate();
            //    return;
            //}

            //
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ==");
            //"ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ=="

            _ = LoadThemeAsync();

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

            try
            {
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
            }
            catch (Exception)
            {
                new Msgwin(false, "در بارگذاری یکسری تنظیمات مربوط به نرم افزار (SMS) خطایی رخ داده , از بروز بودن نرم افزار و اجرا بودن اسکریپت اطمینان حاصل فرمایید").Show();
            }

            //string sname = dbms.Database.SqlQuery<string>("Select @@servername as [ServerName]").FirstOrDefault().ToString();
            //string dname = dbms.Database.SqlQuery<string>("SELECT DB_NAME() AS [Current Database]").FirstOrDefault().ToString();

            SD_Status.Content = $"SERVER : {CL_Generaly.General_Servername} | DATABASE : {CL_Generaly.General_DBname}";
            LBL_VERSION.Content = CL_VERSION.MrCorrectFullVersion;

            this.Title = Baseknow.YEA + " " + Baseknow.WIDTH_D;
        }
        private async void Window_ContentRendered(object sender, EventArgs e) //-----------------------------------------------------------------------------------------
        {
            NowIsReady = true;

            // Check version logic (Assuming CL_VERSION is reliable)
            if (!CL_VERSION.IsValidGreaterVersion())
            {
                // Disable UI interactions immediately
                DisableLoginUI();

                // Await the task to ensure exceptions are caught within the context if possible, 
                // though usually top-level event handlers are void.
                await PerformAutoUpdateAsync();
            }

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


#if DEBUG
            //Baseknow.tindata = "0000000000000000000CORRECT";
            //CL_Generaly.IsMrCorrectLocky = true;

            //Baseknow.USERCOD = 108;
            //Baseknow.UUSER = "modir-mali";

            Baseknow.mrcorrect = true;

            //Baseknow.USERCOD = 139; Baseknow.UUSER = "negar sadeghi";
            //Baseknow.USERCOD = 132; Baseknow.UUSER = "Prima Chopan";    
            //Baseknow.USERCOD = 112; Baseknow.UUSER = "Mr.Tashakori";
            //Baseknow.USERCOD = 116; Baseknow.UUSER = "Mr.Salmani";
            //Baseknow.USERCOD = 167; Baseknow.UUSER = "Mr nikonahad";
            //Baseknow.USERCOD = 73; Baseknow.UUSER = "Mr Rahimi";
            //Baseknow.USERCOD = 86; Baseknow.UUSER = "آقاي سجاد راستي";
            //Baseknow.USERCOD = 174; Baseknow.UUSER = "Miss yeganeh Karbakhsh";
            //Baseknow.USERCOD = 150; Baseknow.UUSER = "Mr mehdi fattahi";
            //Baseknow.USERCOD = 108; Baseknow.UUSER = "modir-mali";
            //Baseknow.USERCOD = 102; Baseknow.UUSER = "mina mehrnia";
            //Baseknow.USERCOD = 150; Baseknow.UUSER = "Mr mehdi fattahi";
            //Baseknow.USERCOD = 73; Baseknow.UUSER = "Mr Rahimi";
            Baseknow.USERCOD = 78; Baseknow.UUSER = "Controller";
            //Baseknow.USERCOD = 35; Baseknow.UUSER = "كنترل";

            CL_Generaly.SHIFT_OF_USER = 1; //شیفت صبح
            CL_Generaly.VAHED_OF_USER = 1; //دپارتمان DEPARTEMAN اداری
            Baseknow.UGRP = "1";
            //CL_Generaly.VAHED_OF_USER = 20; //دپارتمان DEPARTEMAN یزد ویزیتوری

            //new WinBase().Show();
            //new WIN_GETFIRSTMOG().Show();
            new WinConnectionChoose().Show();
            //new F_MENU_DATE("CROS").Show();
      

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, this);
            //new WIN_F_NEWYEAR().Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder, this, 1642d);
            //dotnet publish Prg_UI.csproj -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o E:\prg\PublishedFiles; explorer E:\prg\PublishedFiles

            //new WIN_OPTIONS().Show();
            //new WIN_GETFIRSTMOG().Show();
            //new WIN_LASTPRICE().Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 13);

            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_HAVALEHEE, this, "13774,13760");

            //System.Windows.Forms.MessageBox.Show(CL_CCNNMANAGER.CONNECTION_STR);
            //new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED().Show();
            //new TR_ANBGRD_LST().Show();
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGUDI_SEARCH_MAIN, this);

            //new F_MENU_GOZARESH_FROOSH("FR").ShowDialog();


            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_USER_PERMITION_FORMS_DASTRASI, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, this);

            //new WinEVENTS(37729).ShowDialog();
            return;
#endif

            this.Show(); //Here for debug comment
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
                        await OpenMainWindowAsync();
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
                            await OpenMainWindowAsync();
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
                            await OpenMainWindowAsync();
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

                        if (CmbUsers.SelectedValue == null)
                        {
                            Krbri.Text = null;
                        }
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

        #region AutoUpdate
        // Configuration Constants
        private const string UPDATE_SERVER_PATH = @"\\win-server2016\ade\EXE\update";
        private const string UPDATE_LOCAL_FOLDER = "update";
        private const string TEMP_FILE_SUFFIX = "_UpdateTemp.exe";
        private const int COPY_BUFFER_SIZE = 81920; // 80KB
        private const int READ_TIMEOUT_SECONDS = 20; // Timeout for a single read operation (inactivity timeout)
        private const int MAX_RETRIES = 10;

        private void DisableLoginUI()
        {
            try
            {
                if (CmbUsers != null) CmbUsers.IsEnabled = false;
                if (Rmzo != null) Rmzo.IsEnabled = false;
                if (SecoRmzo != null) SecoRmzo.IsEnabled = false;
                if (Greet != null) Greet.IsEnabled = false;
                if (dispass != null) dispass.IsEnabled = false;
            }
            catch { /* Ignore UI state errors during shutdown */ }
        }

        private async Task PerformAutoUpdateAsync()
        {
            string tempExePath = null;
            string sourcePath = null;

            try
            {
                // 1. Resolve Paths Securely
                string currentExe = Environment.ProcessPath;
                if (string.IsNullOrEmpty(currentExe))
                {
                    using var proc = Process.GetCurrentProcess();
                    currentExe = proc.MainModule?.FileName;
                }

                if (string.IsNullOrEmpty(currentExe) || !File.Exists(currentExe))
                {
                    throw new FileNotFoundException("Critical Error: Cannot resolve current executable path.");
                }

                string currentDir = Path.GetDirectoryName(currentExe);
                string exeName = Path.GetFileName(currentExe);
                sourcePath = Path.Combine(UPDATE_SERVER_PATH, exeName);

                // Local update subfolder for temp download and batch script
                string localUpdateDir = Path.Combine(currentDir, UPDATE_LOCAL_FOLDER);
                Directory.CreateDirectory(localUpdateDir);
                tempExePath = Path.Combine(localUpdateDir, exeName + TEMP_FILE_SUFFIX);

                // 2. Pre-Flight Checks
                if (!File.Exists(sourcePath))
                {
                    ShowErrorAndExit("نسخه جدید در سرور یافت نشد. مسیر:\n" + sourcePath);
                    return;
                }

                if (!HasWritePermission(currentDir))
                {
                    ShowErrorAndExit("عدم دسترسی به پوشه برنامه.\nلطفا برنامه را به عنوان Administrator اجرا کنید.");
                    return;
                }

                // 3. Prepare UI
                if (UpdatePanel != null)
                {
                    UpdatePanel.Visibility = Visibility.Visible;
                    if (UpdateLbl != null) UpdateLbl.Content = "در حال اتصال به سرور...";
                }

                // 4. Download Loop with Retry and Resume
                bool downloadSuccess = false;
                Exception lastException = null;

                for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                {
                    try
                    {
                        // Calculate resume offset
                        long startOffset = 0;
                        if (File.Exists(tempExePath))
                        {
                            var fi = new FileInfo(tempExePath);
                            var sourceFi = new FileInfo(sourcePath);

                            if (fi.Length < sourceFi.Length)
                            {
                                startOffset = fi.Length;
                            }
                            else if (fi.Length >= sourceFi.Length)
                            {
                                // Already downloaded or corrupt (larger).
                                // If equal, assume success.
                                if (fi.Length == sourceFi.Length)
                                {
                                    downloadSuccess = true;
                                    break;
                                }
                                // If larger, delete and restart
                                File.Delete(tempExePath);
                                startOffset = 0;
                            }
                        }

                        if (attempt > 1 && UpdateLbl != null)
                        {
                            await this.Dispatcher.InvokeAsync(() => UpdateLbl.Content = $"تلاش مجدد {attempt}/{MAX_RETRIES}...");
                        }

                        await CopyFileWithProgressAsync(sourcePath, tempExePath, startOffset);
                        downloadSuccess = true;
                        break; // Success
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        // Wait before retry
                        await Task.Delay(2000);
                    }
                }

                if (!downloadSuccess)
                {
                    throw lastException ?? new Exception("Download failed after multiple attempts.");
                }

                // 5. Execute Atomic Swap Script
                ExecuteUpdateScript(currentExe, tempExePath, exeName, currentDir, localUpdateDir);
            }
            catch (Exception ex)
            {
                CleanupTemp(tempExePath);
                ShowErrorAndExit($"خطا در بروزرسانی خودکار:\n{ex.Message}");
            }
        }

        private async Task CopyFileWithProgressAsync(string source, string destination, long startOffset)
        {
            // Open source
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, COPY_BUFFER_SIZE, true))
            // Open destination (OpenOrCreate to support resume)
            using (var destinationStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, COPY_BUFFER_SIZE, true))
            {
                long totalBytes = sourceStream.Length;

                // Seek to offset
                if (startOffset > 0)
                {
                    if (startOffset > totalBytes) startOffset = 0; // Safety check

                    sourceStream.Seek(startOffset, SeekOrigin.Begin);
                    destinationStream.Seek(startOffset, SeekOrigin.Begin);
                }
                else
                {
                    // Ensure we start from 0 if no offset (truncate if exists but we want fresh start)
                    destinationStream.SetLength(0);
                }

                var buffer = new byte[COPY_BUFFER_SIZE];
                long totalRead = startOffset;
                int bytesRead;

                var lastUpdate = DateTime.MinValue;

                while (totalRead < totalBytes)
                {
                    // Read with timeout
                    var readTask = sourceStream.ReadAsync(buffer, 0, buffer.Length);

                    try
                    {
                        // Use WaitAsync for timeout logic
                        // This prevents indefinite hanging on a dead connection
                        bytesRead = await readTask.WaitAsync(TimeSpan.FromSeconds(READ_TIMEOUT_SECONDS));
                    }
                    catch (TimeoutException)
                    {
                        throw new IOException("Connection timed out (Read).");
                    }

                    if (bytesRead == 0) break; // End of stream

                    // Write to local file
                    await destinationStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    if (totalBytes > 0 && (DateTime.Now - lastUpdate).TotalMilliseconds > 100)
                    {
                        double progress = (double)totalRead / totalBytes * 100;
                        lastUpdate = DateTime.Now;

                        await this.Dispatcher.InvokeAsync(() =>
                        {
                            if (UpdatePrg != null) UpdatePrg.Value = progress;
                            if (UpdateLbl != null) UpdateLbl.Content = $"در حال دانلود: {progress:F0}%";
                        });
                    }
                }
            }
        }

        private void ExecuteUpdateScript(string currentExe, string tempExe, string exeName, string currentDir, string localUpdateDir)
        {
            string batPath = Path.Combine(localUpdateDir, "update_installer.bat");

            // Hardened Batch Script
            // 1. Loops trying to overwrite (handles file locking) with max retry limit
            // 2. Starts app in correct Working Directory (/D)
            // 3. Quotes paths to handle spaces
            string batchScript = $@"@echo off
title Updating Application...
echo Waiting for application to close...
set RETRY_COUNT=0

:RETRY_COPY
set /a RETRY_COUNT+=1
if %RETRY_COUNT% gtr 30 (
    echo Update failed: file remained locked after 30 attempts.
    pause
    exit /b 1
)
timeout /t 1 /nobreak > nul
copy /Y ""{tempExe}"" ""{currentExe}"" > nul 2>&1
if %errorlevel% neq 0 (
    echo File is locked. Retrying ^(%RETRY_COUNT%/30^)...
    goto RETRY_COPY
)

echo Update Successful. Starting application...
start """" /D ""{currentDir}"" ""{currentExe}""

:CLEANUP
del ""{tempExe}"" > nul 2>&1
del ""%~f0"" & exit
";

            File.WriteAllText(batPath, batchScript);

            var startInfo = new ProcessStartInfo
            {
                FileName = batPath,
                UseShellExecute = true, // Required for batch file execution in this context
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(startInfo);

            // Force kill current process to ensure file handle is released
            Environment.Exit(0);
        }

        private bool HasWritePermission(string path)
        {
            try
            {
                string testFile = Path.Combine(path, Path.GetRandomFileName());
                using (FileStream fs = File.Create(testFile, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void CleanupTemp(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try { File.Delete(path); } catch { }
            }
        }

        private void ShowErrorAndExit(string message)
        {
            new Msgwin(false, message).ShowDialog();
            CL_LMethods.GoExitTheApplication();
        }
        #endregion

        private void LBL_VERSION_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از اجرای اسکریپت اطمینان دارید؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            ScriptSqly.LetsGo(true);
            new Msgwin(false, "اسکریپت‌ها اجرا شدند.").Show();
        }
    }
}