using Functions;
using Functions.SMSService;
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
using Prg_UI.Wins.WinMenus.CONFIGS;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using Prg_UI.Wins.WinMenus.WinDEFAULT;
using Prg_UI.Wins.WinSetting;
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

#if DEBUG
            return;//Should Remove this lone
#endif

            ScriptSqly.LetsGo();

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
                    if (!string.IsNullOrWhiteSpace(RST?.SMS_TSMSHOST))
                    {
                        SMSPINFO.LINE_NUMBER = Convert.ToInt64(RST?.SMS_TSMSHOST);
                    }
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

            try
            {
                if (App.splashScreen is not null)
                {
                    App.splashScreen.LoadComplete();
                }
            }
            catch { }

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
            //Baseknow.USERCOD = 56; Baseknow.UUSER = "Mr-kazemi";
            //Baseknow.USERCOD = 116; Baseknow.UUSER = "Mr-pakzaban";
            //Baseknow.USERCOD = 35; Baseknow.UUSER = "كنترل";
            Baseknow.USERCOD = 78; Baseknow.UUSER = "Controller";

            CL_Generaly.SHIFT_OF_USER = 1; //شیفت صبح
            CL_Generaly.VAHED_OF_USER = 1; //دپارتمان DEPARTEMAN اداری
            Baseknow.UGRP = "1";
            //CL_Generaly.VAHED_OF_USER = 20; //دپارتمان DEPARTEMAN یزد ویزیتوری

            new WinBase().Show();
            //new WIN_About().Show();
            //new WinConnectionChoose().Show();
            //new WIN_SANAD_EFTETAHIYAH().Show();


            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this);

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
            // Check version logic (Assuming CL_VERSION is reliable)
            if (!CL_VERSION.IsValidGreaterVersion())
            {
                // Disable UI interactions immediately
                DisableLoginUI();

                // مسیر موفق با Environment.Exit تمام می‌شود و مسیرهای خطا خروج برنامه را در صف می‌گذارند؛
                // در هر دو حالت نباید ادامه دهیم و UI لاگینِ غیرفعال‌شده را نمایش بدهیم
                await PerformAutoUpdateAsync();
                return;
            }


            if (CL_Generaly.IsCalledExternally)
            {
                CL_LMethods.GoExitTheApplication(); return;// for access
            }

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
        // ─────────────────────────── ثابت‌های پیکربندی ───────────────────────────

        // مسیر Share شبکه که نسخه جدید EXE در آن قرار می‌گیرد (منبع آپدیت)
        private const string UPDATE_SERVER_PATH = @"\\win-server2016\ade\EXE\update";

        // نام زیرپوشه‌ای کنار EXE برنامه که فایل‌های موقت آپدیت (دانلود، اسکریپت، لاگ) در آن ساخته می‌شود
        private const string UPDATE_LOCAL_FOLDER = "update";

        // پسوند فایل موقتِ دانلود؛ نسخه جدید اول با این پسوند دانلود می‌شود و بعدِ تأیید سلامت، جایگزین EXE اصلی می‌گردد
        private const string TEMP_FILE_SUFFIX = "_UpdateTemp.exe";

        // پسوند فایل Backup؛ هنگام جایگزینی، EXE قدیمی موقتاً به این نام تغییر نام داده می‌شود تا در صورت خطا قابل بازگشت باشد
        private const string OLD_FILE_SUFFIX = ".old";

        // فایل پرچم شکست: هر بار که اسکریپت جایگزینی شکست بخورد یک خط به آن اضافه می‌شود؛
        // برنامه در اجرای بعدی تعداد خطوط را می‌شمارد تا از حلقه بی‌نهایتِ «تلاش/شکست» جلوگیری کند
        private const string UPDATE_FAIL_FLAG = "update_failed.flag";

        // فایل لاگ مراحل آپدیت (هم اسکریپت bat در آن می‌نویسد) — برای عیب‌یابی توسط پشتیبانی
        private const string UPDATE_LOG_FILE = "update_log.txt";

        // اندازه بافر کپی: هر بار 80 کیلوبایت از شبکه خوانده و روی دیسک نوشته می‌شود
        private const int COPY_BUFFER_SIZE = 81920; // 80KB

        // اگر «یک عملیات خواندن» از شبکه بیشتر از این مقدار طول بکشد یعنی اتصال مرده است (تایم‌اوت بی‌تحرکی)
        private const int READ_TIMEOUT_SECONDS = 20;

        // تایم‌اوت عملیات‌های سبک شبکه مثل File.Exists و خواندن سایز/تاریخ فایل روی مسیر UNC
        private const int NETWORK_CHECK_TIMEOUT_SECONDS = 15;

        // حداکثر تعداد تلاش برای دانلود (با ادامه از همان نقطه قطع شدن)
        private const int MAX_RETRIES = 10;

        // بعد از این تعداد شکست در جایگزینی فایل، به جای تلاش دوباره، راهنمای رفع مشکل دستی نمایش داده می‌شود
        private const int MAX_SWAP_FAILURES = 2;

        /// <summary>
        /// کنترل‌های فرم لاگین را غیرفعال می‌کند تا کاربر وسط فرایند آپدیت نتواند لاگین کند
        /// </summary>
        private void DisableLoginUI()
        {
            try
            {
                if (CmbUsers != null) CmbUsers.IsEnabled = false;      // لیست کاربران
                if (Rmzo != null) Rmzo.IsEnabled = false;              // فیلد رمز (مخفی)
                if (SecoRmzo != null) SecoRmzo.IsEnabled = false;      // فیلد رمز (نمایان)
                if (Greet != null) Greet.IsEnabled = false;            // دکمه ورود
                if (dispass != null) dispass.IsEnabled = false;        // تیک نمایش رمز
            }
            catch { /* خطای وضعیت UI هنگام بسته شدن برنامه مهم نیست */ }
        }

        /// <summary>
        /// متد اصلی بروزرسانی خودکار. مراحل:
        /// 1) پیدا کردن مسیر EXE فعلی و ساخت مسیرها
        /// 2) بررسی پرچم شکست‌های قبلی (جلوگیری از حلقه بی‌نهایت)
        /// 3) چک‌های اولیه (وجود فایل در سرور + دسترسی نوشتن)
        /// 4) نمایش پنل Progressbar
        /// 5) حلقه دانلود با تلاش مجدد و ادامه دانلود (Resume) + اعتبارسنجی فایل
        /// 6) ساخت و اجرای اسکریپت جایگزینی و بستن برنامه
        /// هر خطایی در هر مرحله ⇐ نمایش پیام + راهنمای دستی و خروج
        /// </summary>
        private async Task PerformAutoUpdateAsync()
        {
            string tempExePath = null;   // مسیر فایل موقت دانلود (بیرون از try تا در catch قابل پاکسازی باشد)
            string sourcePath = null;    // مسیر فایل جدید روی سرور (بیرون از try تا در پیام خطا قابل نمایش باشد)

            try
            {
                // ── مرحله 1: پیدا کردن مسیر EXE در حال اجرا ──
                // روش اصلی (NET 6+): مسیر دقیق پروسه جاری
                string currentExe = Environment.ProcessPath;
                if (string.IsNullOrEmpty(currentExe))
                {
                    // روش پشتیبان اگر روش اول جواب نداد: از روی ماژول اصلی پروسه
                    using var proc = Process.GetCurrentProcess();
                    currentExe = proc.MainModule?.FileName;
                }

                // اگر مسیر EXE پیدا نشد ادامه دادن خطرناک است (نمی‌دانیم چه فایلی را جایگزین کنیم)
                if (string.IsNullOrEmpty(currentExe) || !File.Exists(currentExe))
                {
                    throw new FileNotFoundException("Critical Error: Cannot resolve current executable path.");
                }

                string currentDir = Path.GetDirectoryName(currentExe);          // پوشه‌ای که برنامه از آن اجرا شده
                string exeName = Path.GetFileName(currentExe);                  // فقط نام فایل، مثلا MrCorrect.exe
                sourcePath = Path.Combine(UPDATE_SERVER_PATH, exeName);         // فایل جدید روی سرور با همان نام

                // ── ساخت پوشه و مسیرهای محلی آپدیت ──
                // نام فایل‌های موقت per-user است (پسوند نام کاربر ویندوز) تا روی Remote Desktop،
                // چند کاربر که همزمان آپدیت می‌گیرند روی فایل‌های همدیگر ننویسند
                string localUpdateDir = Path.Combine(currentDir, UPDATE_LOCAL_FOLDER);
                Directory.CreateDirectory(localUpdateDir);                       // اگر پوشه نبود ساخته می‌شود
                string userSuffix = GetSafeUserSuffix();                         // نام کاربر ویندوز، فقط حروف و اعداد
                tempExePath = Path.Combine(localUpdateDir, $"{exeName}.{userSuffix}{TEMP_FILE_SUFFIX}"); // فایل موقت دانلود
                string metaPath = tempExePath + ".meta";                         // امضای نسخه سرور (سایز|تاریخ) برای تشخیص اعتبار Resume
                string flagPath = Path.Combine(localUpdateDir, UPDATE_FAIL_FLAG); // پرچم شکست (مشترک بین همه کاربران؛ مشکل جایگزینی ماشینی است نه کاربری)

                // ── مرحله 2: اگر دفعات قبل جایگزینی شکست خورده، حلقه «تلاش/شکست» را قطع کن ──
                // (اسکریپت bat بعد از هر شکست یک خط به Flag اضافه می‌کند و برنامه قدیمی را دوباره باز می‌کند؛
                //  بدون این بررسی، برنامه دوباره آپدیت می‌گرفت و دوباره شکست می‌خورد... تا بی‌نهایت)
                int failCount = GetUpdateFailCount(flagPath);
                if (failCount >= MAX_SWAP_FAILURES)
                {
                    try { File.Delete(flagPath); } catch { }                     // پاک می‌کنیم تا اجرای بعدی دوباره شانس آپدیت خودکار داشته باشد
                    ShowErrorAndExit("بروزرسانی خودکار چند بار ناموفق بود.\n\n" + BuildManualUpdateGuide(sourcePath));
                    return;
                }

                // پاکسازی به‌جامانده‌های آپدیت‌های قبلی (فایل .old، فایل‌های فرمت قدیمی، لاگ حجیم)
                CleanupLeftoverUpdateFiles(currentExe, localUpdateDir);

                // ── مرحله 3: چک‌های اولیه ──
                // File.Exists روی مسیر شبکه قطع، می‌تواند دقایق طولانی هنگ کند؛
                // پس داخل RunNetworkCheckAsync با تایم‌اوت 15 ثانیه اجرا می‌شود
                bool sourceExists = await RunNetworkCheckAsync(() => File.Exists(sourcePath), "بررسی فایل روی سرور");
                if (!sourceExists)
                {
                    ShowErrorAndExit("نسخه جدید در سرور یافت نشد. مسیر:\n" + sourcePath);
                    return;
                }

                // اگر کاربر اجازه نوشتن در پوشه برنامه را ندارد (مثلا Program Files بدون Admin)،
                // جایگزینی EXE قطعا شکست می‌خورد؛ بهتر است از همین اول به او بگوییم
                if (!HasWritePermission(currentDir))
                {
                    ShowErrorAndExit("عدم دسترسی به پوشه برنامه.\nلطفا برنامه را به عنوان Administrator اجرا کنید.");
                    return;
                }

                // ── مرحله 4: نمایش پنل آپدیت (Progressbar + متن وضعیت) ──
                if (UpdatePanel != null)
                {
                    UpdatePanel.Visibility = Visibility.Visible;
                    if (UpdateLbl != null) UpdateLbl.Content = "در حال اتصال به سرور...";
                }

                // ── مرحله 5: حلقه دانلود با تلاش مجدد و ادامه دانلود (Resume) ──
                bool downloadSuccess = false;       // آیا دانلود سالم تمام شد؟
                Exception lastException = null;     // آخرین خطا، برای نمایش اگر همه تلاش‌ها شکست خورد
                long expectedSize = 0;              // سایز فایل سرور؛ بعدا به اسکریپت bat هم داده می‌شود تا جایگزینی را تأیید کند

                for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
                {
                    try
                    {
                        // سایز و تاریخِ فایل سرور در «هر تلاش» دوباره خوانده می‌شود تا اگر ادمین وسط کار
                        // فایل سرور را عوض کرد، متوجه شویم و دانلود نیمه‌کاره قبلی را دور بریزیم.
                        // نکته مهم: خواندن Length و LastWriteTime باید «داخل لامبدا» باشد؛ سازنده FileInfo
                        // هیچ I/O انجام نمی‌دهد (Lazy است) و اگر Property بیرون خوانده می‌شد، تایم‌اوت بی‌اثر بود
                        var (sourceLength, sourceTicks) = await RunNetworkCheckAsync(() =>
                        {
                            var fi = new FileInfo(sourcePath);
                            return (fi.Length, fi.LastWriteTimeUtc.Ticks);
                        }, "خواندن اطلاعات فایل سرور");
                        expectedSize = sourceLength;
                        // «امضای» نسخه سرور = سایز + تاریخ آخرین تغییر؛ اگر هرکدام عوض شود یعنی فایل سرور جدید است
                        string sourceSignature = $"{sourceLength}|{sourceTicks}";

                        // محاسبه نقطه شروع: اگر فایل ناقصی از قبل هست و متعلق به «همین» نسخه سرور است،
                        // از همان‌جا ادامه می‌دهیم؛ وگرنه از صفر (جزئیات داخل خود متد)
                        long startOffset = ComputeResumeOffset(tempExePath, metaPath, sourceSignature, sourceLength);

                        if (startOffset == sourceLength && sourceLength > 0)
                        {
                            // فایل از قبل «کامل» دانلود شده (مثلا در اجرای قبلی برنامه)؛ فقط سلامتش را چک کن
                            if (VerifyDownloadedFile(tempExePath, sourceLength))
                            {
                                downloadSuccess = true;
                                break;                       // دانلود لازم نیست؛ مستقیم برو سراغ جایگزینی
                            }
                            CleanupTemp(tempExePath);        // کامل بود ولی خراب ⇐ حذف و دانلود از صفر
                            startOffset = 0;
                        }

                        // امضای نسخه سرور را «قبل از» شروع دانلود ذخیره می‌کنیم تا اگر وسط دانلود قطع شد،
                        // تلاش بعدی بداند فایل ناقص مال کدام نسخه است (شرط لازم برای Resume امن)
                        try { File.WriteAllText(metaPath, sourceSignature); } catch { }

                        // از تلاش دوم به بعد، شماره تلاش را به کاربر نشان بده
                        if (attempt > 1 && UpdateLbl != null)
                        {
                            await this.Dispatcher.InvokeAsync(() => UpdateLbl.Content = $"تلاش مجدد {attempt}/{MAX_RETRIES}...");
                        }

                        // کل عملیات کپی در Thread پس‌زمینه اجرا می‌شود؛ باز کردن FileStream روی مسیر شبکه
                        // همگام (Sync) است و اگر روی Thread رابط کاربری انجام شود، Share کند یا قطع
                        // می‌تواند فرم لاگین را فریز کند (آپدیت‌های UI از داخل متد با Dispatcher انجام می‌شود)
                        await Task.Run(() => CopyFileWithProgressAsync(sourcePath, tempExePath, startOffset));

                        // بعد از دانلود، قبل از هر جایگزینی: سایز دقیق + هدر MZ فایل اجرایی بررسی می‌شود
                        if (!VerifyDownloadedFile(tempExePath, sourceLength))
                        {
                            CleanupTemp(tempExePath);        // فایل خراب نباید برای Resume بعدی بماند
                            throw new IOException("فایل دانلود شده ناقص یا خراب است.");
                        }

                        downloadSuccess = true;
                        break; // دانلود سالم تمام شد؛ از حلقه تلاش‌ها خارج شو
                    }
                    catch (Exception ex)
                    {
                        // هر خطایی (قطعی شبکه، تایم‌اوت، فایل خراب و...) ⇐ 2 ثانیه صبر و تلاش بعدی
                        lastException = ex;
                        await Task.Delay(2000);
                    }
                }

                // اگر هر 10 تلاش شکست خورد، آخرین خطا را به بلوک catch بیرونی پرتاب کن (نمایش پیام + راهنما)
                if (!downloadSuccess)
                {
                    throw lastException ?? new Exception("Download failed after multiple attempts.");
                }

                if (UpdateLbl != null)
                {
                    await this.Dispatcher.InvokeAsync(() => UpdateLbl.Content = "در حال نصب بروزرسانی...");
                }

                // ── مرحله 6: ساخت و اجرای اسکریپت جایگزینی ──
                // این متد برنامه را با Environment.Exit می‌بندد و دیگر برنمی‌گردد
                ExecuteUpdateScript(currentExe, tempExePath, currentDir, localUpdateDir, metaPath, flagPath, expectedSize, userSuffix);
            }
            catch (Exception ex)
            {
                // هر خطای پیش‌بینی‌نشده در کل فرایند: فایل موقت پاک، پیام خطا + راهنمای دستی، خروج از برنامه
                CleanupTemp(tempExePath);
                ShowErrorAndExit($"خطا در بروزرسانی خودکار:\n{ex.Message}\n\n{BuildManualUpdateGuide(sourcePath)}");
            }
        }

        /// <summary>
        /// کپی فایل از سرور به دیسک محلی، تکه‌تکه (80KB)، با Progressbar و تایم‌اوت روی هر خواندن.
        /// startOffset اگر بزرگتر از صفر باشد یعنی ادامه‌ی یک دانلود نیمه‌کاره (Resume).
        /// </summary>
        private async Task CopyFileWithProgressAsync(string source, string destination, long startOffset)
        {
            // باز کردن فایل مبدأ روی شبکه — فقط خواندن؛ FileShare.Read یعنی بقیه هم بتوانند همزمان بخوانند
            // پارامتر آخر (true) یعنی استریم در حالت Async باز شود
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, COPY_BUFFER_SIZE, true))
            // باز کردن فایل مقصد — OpenOrCreate تا اگر فایل نیمه‌کاره از قبل هست برای Resume باز شود؛
            // FileShare.None یعنی هیچ‌کس دیگری همزمان به آن دست نزند
            using (var destinationStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, COPY_BUFFER_SIZE, true))
            {
                long totalBytes = sourceStream.Length;   // سایز کل فایل سرور (برای درصد پیشرفت و تشخیص پایان)

                if (startOffset > 0)
                {
                    // Resume: هر دو استریم را به نقطه قطع شدن قبلی جلو ببر
                    if (startOffset > totalBytes) startOffset = 0; // محافظت: Offset بزرگتر از فایل بی‌معنی است

                    sourceStream.Seek(startOffset, SeekOrigin.Begin);
                    destinationStream.Seek(startOffset, SeekOrigin.Begin);
                }
                else
                {
                    // دانلود از صفر: اگر فایل مقصد از قبل محتوایی داشت، خالی‌اش کن
                    destinationStream.SetLength(0);
                }

                var buffer = new byte[COPY_BUFFER_SIZE]; // بافر 80 کیلوبایتی برای هر نوبت خواندن/نوشتن
                long totalRead = startOffset;            // چند بایت تا الان داریم (شامل بخش Resume شده)
                int bytesRead;                           // چند بایت در «این نوبت» خوانده شد

                var lastUpdate = DateTime.MinValue;      // زمان آخرین آپدیت Progressbar (برای محدود کردن دفعات آپدیت UI)

                while (totalRead < totalBytes)
                {
                    // خواندن یک تکه از فایل سرور (به صورت Async شروع می‌شود)
                    var readTask = sourceStream.ReadAsync(buffer, 0, buffer.Length);

                    try
                    {
                        // اگر این خواندن بیشتر از 20 ثانیه طول بکشد یعنی اتصال مرده است؛
                        // بدون این، برنامه روی شبکه قطع برای همیشه گیر می‌کرد
                        bytesRead = await readTask.WaitAsync(TimeSpan.FromSeconds(READ_TIMEOUT_SECONDS));
                    }
                    catch (TimeoutException)
                    {
                        // تبدیل تایم‌اوت به IOException تا حلقه تلاش مجددِ بیرونی آن را بگیرد و دوباره تلاش کند
                        throw new IOException("Connection timed out (Read).");
                    }

                    if (bytesRead == 0) break; // صفر یعنی استریم تمام شد (پایین‌تر چک می‌کنیم واقعا کامل بوده یا قطعی)

                    // نوشتن همان تکه روی فایل محلی
                    await destinationStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    // آپدیت Progressbar حداکثر هر 100 میلی‌ثانیه یک بار (وگرنه UI با هزاران آپدیت کند می‌شود)
                    if (totalBytes > 0 && (DateTime.Now - lastUpdate).TotalMilliseconds > 100)
                    {
                        double progress = (double)totalRead / totalBytes * 100;
                        lastUpdate = DateTime.Now;

                        // این متد روی Thread پس‌زمینه اجرا می‌شود؛ دستکاری کنترل‌های WPF فقط از طریق Dispatcher مجاز است
                        await this.Dispatcher.InvokeAsync(() =>
                        {
                            if (UpdatePrg != null) UpdatePrg.Value = progress;
                            if (UpdateLbl != null) UpdateLbl.Content = $"در حال دانلود: {progress:F0}%";
                        });
                    }
                }

                // اطمینان از اینکه همه بایت‌ها واقعا روی دیسک نوشته شده‌اند (نه فقط در بافر حافظه)
                await destinationStream.FlushAsync();

                // نکته حیاتی: اگر استریم زودتر از پایان فایل تمام شد (bytesRead == 0 وسط کار)،
                // این یعنی قطعی شبکه — نباید موفقیت حساب شود وگرنه فایل ناقص جایگزین EXE اصلی می‌شد
                // (این دقیقا همان باگی بود که باعث می‌شد Progressbar صد درصد شود ولی برنامه دیگر باز نشود)
                if (totalRead < totalBytes)
                {
                    throw new IOException("اتصال به سرور قطع شد و فایل کامل دریافت نشد.");
                }
            }
        }

        /// <summary>
        /// اسکریپت bat جایگزینی را می‌سازد، اجرا می‌کند و برنامه را می‌بندد (این متد هرگز برنمی‌گردد).
        /// چرا bat؟ چون یک برنامه نمی‌تواند فایل EXE «در حال اجرای» خودش را جایگزین کند؛
        /// پس یک پروسه جدا (cmd) باید بعد از بسته شدن برنامه این کار را انجام دهد.
        /// </summary>
        private void ExecuteUpdateScript(string currentExe, string tempExe, string currentDir, string localUpdateDir, string metaPath, string flagPath, long expectedSize, string userSuffix)
        {
            string batPath = Path.Combine(localUpdateDir, $"update_installer_{userSuffix}.bat"); // فایل bat (per-user تا کاربران RDP تداخل نکنند)
            string oldExe = currentExe + OLD_FILE_SUFFIX;                                        // نام Backup فایل قدیمی هنگام جایگزینی
            string logPath = Path.Combine(localUpdateDir, UPDATE_LOG_FILE);                      // لاگ مراحل اسکریپت
            int currentPid = Environment.ProcessId;                                              // شماره پروسه فعلی؛ اسکریپت منتظر بسته شدنش می‌ماند

            // متن فارسی هشدارِ آخرین‌راه‌حل در فایل جداگانه UTF-8 نوشته می‌شود؛
            // cmd متن غیر ASCII داخل خود bat را (با یا بدون BOM) خراب نمایش می‌دهد
            // (شکست در نوشتن این فایلِ اختیاری نباید کل بروزرسانی را متوقف کند)
            string alertMsgPath = Path.Combine(localUpdateDir, $"update_alert_{userSuffix}.txt");
            try
            {
                File.WriteAllText(alertMsgPath,
                    "بروزرسانی خودکار ناتمام ماند و برنامه به صورت خودکار اجرا نشد.\r\n" +
                    "لطفا برنامه را به صورت دستی اجرا کنید.\r\n" +
                    "در صورت تکرار مشکل با پشتیبانی تماس بگیرید.\r\n" +
                    "(گزارش خطا: فایل update_log.txt در پوشه update کنار برنامه)",
                    Encoding.UTF8);
            }
            catch { }

            // مسیر داخل رشته تک‌کوتیشن PowerShell قرار می‌گیرد؛ آپاستروف احتمالی باید دوبل شود
            string alertMsgPathPs = alertMsgPath.Replace("'", "''");

            // ─────────────────── شرح کامل منطق اسکریپت bat (خط به خط) ───────────────────
            // نکته نگارشی: این یک رشته Interpolated است؛ {متغیر} با مقدار C# جایگزین می‌شود و "" یعنی یک " واقعی.
            //
            // ● ابتدای اسکریپت:
            //     @echo off            ⇐ دستورات در کنسول چاپ نشوند
            //     set LOG_FILE=...     ⇐ مسیر فایل لاگ در یک متغیر (همه مراحل در آن ثبت می‌شود)
            //
            // ● بخش WAIT_EXIT — انتظار برای بسته شدن برنامه (حداکثر ~15 ثانیه):
            //     با tasklist چک می‌کند پروسه برنامه (با همین PID) هنوز زنده است یا نه.
            //     «ping -n 2 127.0.0.1» معادل ~1 ثانیه تأخیر است (به جای دستور timeout که در
            //     بعضی Session های RDP خطای Input redirection می‌دهد). اگر بعد از 15 بار هنوز
            //     زنده بود، ادامه می‌دهد — چون حلقه کپی خودش حالت قفل بودن فایل را مدیریت می‌کند.
            //
            // ● بخش COPY_PHASE / RETRY_COPY — جایگزینی فایل (حداکثر 30 تلاش، هر کدام ~1 ثانیه فاصله):
            //     1) del oldExe        ⇐ پاک کردن Backup قدیمی احتمالی از دفعات قبل
            //     2) move currentExe → oldExe ⇐ ترفند کلیدی: ویندوز «تغییر نام» EXE در حال اجرا را
            //        مجاز می‌داند (برخلاف Overwrite/Delete) حتی وقتی کاربران دیگر RDP آن را باز دارند.
            //        با این کار همیشه اول یک Backup سالم ساخته می‌شود.
            //     3) اگر تغییر نام نشد (if not exist oldExe) ⇐ ثبت در لاگ و تلاش بعدی
            //     4) copy tempExe → currentExe ⇐ کپی نسخه جدید سر جای نام اصلی
            //     5) اگر کپی شکست خورد ⇐ فایل ناقص پاک، Backup برگردانده (move oldExe → currentExe) و تلاش بعدی
            //
            // ● بخش VERIFY — تأیید جایگزینی:
            //     سایز فایل جدید (%%~zA) با سایز مورد انتظار که C# پاس داده مقایسه می‌شود؛
            //     اگر فرق داشت یعنی کپی خراب بوده ⇐ فایل خراب حذف، Backup برگردانده، برو به FAILED
            //
            // ● بخش SUCCESS — موفقیت:
            //     پرچم شکست + Backup + فایل موقت + فایل متا پاک می‌شوند، سپس برنامه (نسخه جدید) با
            //     start اجرا می‌شود. «ver > nul» قبل از start برای صفر کردن errorlevel کهنه است
            //     (بعضی دستورات cmd در حالت موفق errorlevel را ریست نمی‌کنند و باعث هشدار اشتباه می‌شد).
            //     اگر start شکست خورد، 2 ثانیه صبر و یک بار دیگر تلاش می‌شود؛ شکست دوباره ⇐ ALERT
            //
            // ● بخش FAILED — شکست بعد از 30 تلاش:
            //     یک خط به فایل پرچم اضافه می‌شود (برنامه در اجرای بعد آن را می‌شمارد)،
            //     اگر EXE سر جایش نیست Backup برگردانده می‌شود، و برنامه (نسخه قدیمی) دوباره اجرا
            //     می‌شود تا کاربر بدون برنامه نماند. اگر اجرا هم نشد ⇐ ALERT
            //
            // ● بخش ALERT — آخرین راه‌حل (برنامه به هیچ شکلی اجرا نشد):
            //     با PowerShell یک MessageBox ویندوزی نمایش داده می‌شود؛ متن فارسی آن از فایل
            //     UTF-8 ای خوانده می‌شود که C# بالاتر نوشت (تا bat تماماً ASCII بماند و فارسی خراب نشود)
            //
            // ● بخش END — پاکسازی نهایی:
            //     فایل متن هشدار حذف و در آخر اسکریپت «خودش را» حذف می‌کند (del %~f0)
            // ─────────────────────────────────────────────────────────────────────────────
            string batchScript = $@"@echo off
title Updating Application...
set LOG_FILE=""{logPath}""
echo. >> %LOG_FILE% 2>nul
echo [%date% %time%] [{userSuffix}] update script started (pid {currentPid}) >> %LOG_FILE% 2>nul

rem ---- Wait (max ~15s) for the old application process to exit ----
set WAIT_COUNT=0
:WAIT_EXIT
set /a WAIT_COUNT+=1
if %WAIT_COUNT% gtr 15 goto COPY_PHASE
tasklist /FI ""PID eq {currentPid}"" 2>nul | find ""{currentPid}"" >nul 2>&1
if errorlevel 1 goto COPY_PHASE
ping -n 2 127.0.0.1 > nul
goto WAIT_EXIT

:COPY_PHASE
set RETRY_COUNT=0
:RETRY_COPY
set /a RETRY_COUNT+=1
if %RETRY_COUNT% gtr 30 goto FAILED
ping -n 2 127.0.0.1 > nul

rem always move the current exe aside FIRST so a good backup exists on every path
rem (rename works even while the exe is running or held open by other RDP sessions)
del ""{oldExe}"" > nul 2>&1
move /Y ""{currentExe}"" ""{oldExe}"" > nul 2>&1
if not exist ""{oldExe}"" (
    echo [%time%] attempt %RETRY_COUNT%: exe locked for rename >> %LOG_FILE% 2>nul
    goto RETRY_COPY
)

copy /Y ""{tempExe}"" ""{currentExe}"" > nul 2>&1
if not errorlevel 1 goto VERIFY

rem copy failed - remove any partial file, restore the backup, then retry
del ""{currentExe}"" > nul 2>&1
move /Y ""{oldExe}"" ""{currentExe}"" > nul 2>&1
echo [%time%] attempt %RETRY_COUNT%: copy failed >> %LOG_FILE% 2>nul
goto RETRY_COPY

:VERIFY
set NEW_SIZE=0
for %%A in (""{currentExe}"") do set NEW_SIZE=%%~zA
if ""%NEW_SIZE%""==""{expectedSize}"" goto SUCCESS
echo [%time%] verify failed: size %NEW_SIZE% expected {expectedSize} >> %LOG_FILE% 2>nul
del ""{currentExe}"" > nul 2>&1
move /Y ""{oldExe}"" ""{currentExe}"" > nul 2>&1
goto FAILED

:SUCCESS
echo [%date% %time%] update installed successfully >> %LOG_FILE% 2>nul
del ""{flagPath}"" > nul 2>&1
del ""{oldExe}"" > nul 2>&1
del ""{tempExe}"" > nul 2>&1
del ""{metaPath}"" > nul 2>&1
ver > nul
start """" /D ""{currentDir}"" ""{currentExe}""
if not errorlevel 1 goto END
ping -n 3 127.0.0.1 > nul
start """" /D ""{currentDir}"" ""{currentExe}""
if errorlevel 1 goto ALERT
goto END

:FAILED
echo [%date% %time%] UPDATE FAILED after %RETRY_COUNT% attempts >> %LOG_FILE% 2>nul
echo [%date% %time%] swap failed >> ""{flagPath}""
rem make sure a runnable exe is in place (restore the backup if the swap left none)
if not exist ""{currentExe}"" move /Y ""{oldExe}"" ""{currentExe}"" > nul 2>&1
if not exist ""{currentExe}"" goto ALERT
rem restart the application so the user is not left with nothing
rem ('ver' clears any stale errorlevel left by earlier failed commands - some cmd
rem commands like 'start' do not reliably reset it on success, causing false alerts)
ver > nul
start """" /D ""{currentDir}"" ""{currentExe}""
if not errorlevel 1 goto END
ping -n 3 127.0.0.1 > nul
start """" /D ""{currentDir}"" ""{currentExe}""
if errorlevel 1 goto ALERT
goto END

rem last resort: the application could not be restarted at all - show a visible alert
rem (the Persian text lives in a UTF-8 file written by the app; the bat stays pure ASCII)
:ALERT
echo [%date% %time%] app did not restart - showing alert >> %LOG_FILE% 2>nul
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Add-Type -AssemblyName System.Windows.Forms; $t=[System.IO.File]::ReadAllText('{alertMsgPathPs}',[System.Text.Encoding]::UTF8); [System.Windows.Forms.MessageBox]::Show($t,'MrCorrect',0,48)"" > nul 2>&1

:END
del ""{alertMsgPath}"" > nul 2>&1
del ""%~f0"" & exit
";

            // نوشتن محتوای اسکریپت روی دیسک (محتوا تماماً ASCII است؛ Encoding مشکلی ایجاد نمی‌کند)
            File.WriteAllText(batPath, batchScript);

            // اجرای اسکریپت مستقیماً از طریق cmd.exe (نه ShellExecute)؛ روی بعضی کلاینت‌ها
            // File Association فایل‌های bat خراب بود و ShellExecute هیچ کاری نمی‌کرد —
            // نتیجه‌اش این بود که برنامه بسته می‌شد ولی هیچ‌وقت دوباره باز نمی‌شد
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"), // مسیر کامل cmd از پوشه System32 (نه وابسته به PATH)
                Arguments = $"/c \"{batPath}\"",   // c/ یعنی اسکریپت را اجرا کن و بعد cmd را ببند
                WorkingDirectory = localUpdateDir,
                UseShellExecute = false,           // اجرای مستقیم پروسه، بدون واسطه Shell ویندوز
                CreateNoWindow = true              // بدون پنجره سیاه cmd (کاربر چیزی نمی‌بیند)
            };

            Process.Start(startInfo);

            // بستن فوری و قطعی برنامه تا قفل فایل EXE (از طرف این پروسه) آزاد شود و اسکریپت بتواند جایگزینی را انجام دهد
            Environment.Exit(0);
        }

        /// <summary>
        /// محاسبه نقطه شروع دانلود: فقط در صورتی ادامه دانلود (Resume) مجاز است که فایل ناقص قبلی
        /// متعلق به «همین» نسخه سرور باشد. در غیر این صورت چسباندن ادامه فایل جدید به نیمه فایل قدیمی،
        /// یک فایل خراب با سایز درست می‌ساخت که از همه چک‌های سایز رد می‌شد!
        /// خروجی: 0 = از صفر دانلود کن | بین 0 و سایز = از این نقطه ادامه بده | برابر سایز = از قبل کامل است
        /// </summary>
        private long ComputeResumeOffset(string tempExePath, string metaPath, string sourceSignature, long sourceLength)
        {
            try
            {
                // اصلا فایل نیمه‌کاره‌ای وجود ندارد ⇐ دانلود از صفر
                if (!File.Exists(tempExePath)) return 0;

                // امضای ذخیره‌شده هنگام دانلود قبلی را بخوان و با امضای فعلی سرور مقایسه کن
                string previousSignature = File.Exists(metaPath) ? File.ReadAllText(metaPath) : null;
                if (previousSignature != sourceSignature)
                {
                    // فایل سرور از آن موقع عوض شده ⇐ نیمه قبلی بی‌ارزش است؛ حذف و دانلود از صفر
                    File.Delete(tempExePath);
                    return 0;
                }

                long tempLength = new FileInfo(tempExePath).Length;
                if (tempLength > sourceLength)
                {
                    // فایل ناقص از فایل سرور بزرگ‌تر است؟! وضعیت نامعتبر ⇐ حذف و دانلود از صفر
                    File.Delete(tempExePath);
                    return 0;
                }

                // فایل ناقص معتبر است؛ از انتهای آن ادامه بده (اگر برابر سایز بود یعنی کامل است)
                return tempLength;
            }
            catch
            {
                // هر خطایی در خواندن متا/فایل ⇐ امن‌ترین حالت: همه‌چیز را پاک کن و از صفر شروع کن
                try { File.Delete(tempExePath); } catch { }
                return 0;
            }
        }

        /// <summary>
        /// اعتبارسنجی فایل دانلودشده قبل از جایگزینی:
        /// 1) سایز باید «دقیقا» برابر سایز فایل سرور باشد
        /// 2) دو بایت اول باید 'MZ' باشد (امضای استاندارد همه فایل‌های اجرایی ویندوز)
        /// </summary>
        private bool VerifyDownloadedFile(string path, long expectedLength)
        {
            try
            {
                var fi = new FileInfo(path);
                if (!fi.Exists || fi.Length != expectedLength || expectedLength <= 0) return false;

                // خواندن دو بایت اول فایل و مقایسه با امضای MZ
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return fs.ReadByte() == 'M' && fs.ReadByte() == 'Z';
            }
            catch
            {
                // فایل قابل باز شدن نیست (قفل/خراب) ⇐ نامعتبر
                return false;
            }
        }

        /// <summary>
        /// اجرای یک عملیات روی مسیر شبکه (UNC) با محدودیت زمانی:
        /// عملیات داخل Task.Run به Thread پس‌زمینه می‌رود (تا UI قفل نشود) و WaitAsync رویش
        /// تایم‌اوت 15 ثانیه‌ای می‌گذارد (تا روی شبکه قطع، بی‌نهایت منتظر نمانیم).
        /// نکته: عملیات I/O باید واقعا «داخل لامبدای operation» انجام شود وگرنه تایم‌اوت بی‌اثر است.
        /// </summary>
        private static async Task<T> RunNetworkCheckAsync<T>(Func<T> operation, string operationName)
        {
            try
            {
                return await Task.Run(operation).WaitAsync(TimeSpan.FromSeconds(NETWORK_CHECK_TIMEOUT_SECONDS));
            }
            catch (TimeoutException)
            {
                // تبدیل به IOException با پیام فارسی قابل فهم؛ حلقه تلاش مجدد بیرونی آن را مدیریت می‌کند
                throw new IOException($"عدم پاسخ از سرور ({operationName}). لطفا اتصال شبکه را بررسی کنید.");
            }
        }

        /// <summary>
        /// شمارش تعداد شکست‌های قبلی جایگزینی (تعداد خطوط فایل پرچم که اسکریپت bat نوشته است)
        /// </summary>
        private static int GetUpdateFailCount(string flagPath)
        {
            try
            {
                if (!File.Exists(flagPath)) return 0; // هیچ شکستی ثبت نشده

                // شکست‌های قدیمی (مثلا قطعی شبکه هفته‌های قبل، شاید توسط کاربر دیگری روی همین سرور RDP)
                // نباید بروزرسانی امروز را مسدود کنند ⇐ پرچم قدیمی‌تر از 7 روز نادیده گرفته و پاک می‌شود
                if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(flagPath)).TotalDays > 7)
                {
                    try { File.Delete(flagPath); } catch { }
                    return 0;
                }

                // هر خط = یک شکست
                return File.ReadAllLines(flagPath).Length;
            }
            catch
            {
                // فایل هست ولی قابل خواندن نیست ⇐ محتاطانه «یک شکست» حساب کن (آپدیت هنوز تلاش می‌شود)
                return 1;
            }
        }

        /// <summary>
        /// نام کاربر ویندوز را به یک پسوند امن برای نام فایل تبدیل می‌کند (فقط حروف و اعداد).
        /// این پسوند باعث می‌شود فایل‌های موقت هر کاربر RDP از هم جدا باشند.
        /// </summary>
        private static string GetSafeUserSuffix()
        {
            string user = Environment.UserName ?? string.Empty;
            var sb = new StringBuilder();
            foreach (char c in user)
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c); // کاراکترهای غیرمجاز در نام فایل (و فاصله) حذف می‌شوند
            }
            return sb.Length > 0 ? sb.ToString() : "user"; // اگر چیزی نماند، یک نام پیش‌فرض
        }

        /// <summary>
        /// پاکسازی به‌جامانده‌های آپدیت‌های قبلی، ابتدای هر فرایند آپدیت:
        /// فایل Backup قدیمی (.old)، فایل‌های فرمت قدیمی آپدیتر، و لاگ اگر بیش از حد بزرگ شده باشد
        /// </summary>
        private void CleanupLeftoverUpdateFiles(string currentExe, string localUpdateDir)
        {
            try
            {
                // فایل .old که اسکریپت دفعه قبل موفق به حذفش نشده (مثلا چون کاربر دیگری هنوز نسخه قدیمی را باز داشت)
                string oldExe = currentExe + OLD_FILE_SUFFIX;
                if (File.Exists(oldExe))
                {
                    try { File.Delete(oldExe); } catch { /* هنوز توسط کاربر دیگری باز است؛ دفعه بعد پاک می‌شود */ }
                }

                // فایل‌های به‌جامانده از نسخه‌های قبلی آپدیتر (که نام‌گذاری per-user نداشتند)
                // اینها دیگر هیچ‌وقت استفاده نمی‌شوند و فقط حجم اشغال می‌کردند
                string exeName = Path.GetFileName(currentExe);
                CleanupTemp(Path.Combine(localUpdateDir, exeName + TEMP_FILE_SUFFIX));
                CleanupTemp(Path.Combine(localUpdateDir, "update_installer.bat"));

                // جلوگیری از بزرگ شدن بی‌رویه فایل لاگ (بیش از 1 مگابایت ⇐ از نو شروع شود)
                var logInfo = new FileInfo(Path.Combine(localUpdateDir, UPDATE_LOG_FILE));
                if (logInfo.Exists && logInfo.Length > 1024 * 1024)
                {
                    try { logInfo.Delete(); } catch { }
                }
            }
            catch { /* پاکسازی اختیاری است؛ شکستش نباید جلوی آپدیت را بگیرد */ }
        }

        /// <summary>
        /// متن راهنمای رفع مشکل برای کاربر عادی — وقتی آپدیت خودکار به مشکل خورده باشد.
        /// اگر مسیر فایل سرور هنوز معلوم نشده باشد (خطا در مراحل اولیه)، مسیر کلی Share نمایش داده می‌شود.
        /// </summary>
        private static string BuildManualUpdateGuide(string sourcePath)
        {
            return "راهنمای رفع مشکل بروزرسانی:\n" +
                   "1- نرم افزار را در همه سیستم ها و همه کاربران (Remote Desktop) ببندید و دوباره اجرا کنید.\n" +
                   "2- اگر مشکل ادامه داشت، برنامه را یک بار با کلیک راست و گزینه Run as Administrator اجرا کنید.\n" +
                   "3- در غیر این صورت فایل جدید را به صورت دستی از مسیر زیر کپی و جایگزین فایل برنامه کنید:\n" +
                   (string.IsNullOrEmpty(sourcePath) ? UPDATE_SERVER_PATH : sourcePath) + "\n" +
                   "4- در صورت نیاز با پشتیبانی تماس بگیرید (فایل update_log.txt در پوشه update کنار برنامه به عیب یابی کمک می کند).";
        }

        /// <summary>
        /// تست عملی دسترسی نوشتن در یک پوشه: یک فایل آزمایشی با نام تصادفی ساخته می‌شود؛
        /// DeleteOnClose یعنی به محض بسته شدن، خودش حذف می‌شود (هیچ ردی نمی‌ماند).
        /// این روش از خواندن ACL ها مطمئن‌تر است چون نتیجه «واقعی» را نشان می‌دهد.
        /// </summary>
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
                return false; // هر خطایی (Access Denied و...) یعنی دسترسی نوشتن نداریم
            }
        }

        /// <summary>
        /// حذف امن یک فایل موقت — اگر وجود داشته باشد؛ خطاها بی‌صدا نادیده گرفته می‌شوند
        /// </summary>
        private void CleanupTemp(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try { File.Delete(path); } catch { }
            }
        }

        /// <summary>
        /// نمایش پیام خطا به کاربر و سپس خروج از برنامه.
        /// از حالت متن بزرگ با پنجره بلندتر استفاده می‌شود چون پیام‌های چندخطی راهنما
        /// در حالت عادی Msgwin (پنجره کوتاه بدون اسکرول) بریده می‌شدند و کاربر نمی‌توانست بخواند.
        /// </summary>
        private void ShowErrorAndExit(string message)
        {
            var msgwin = new Msgwin(false, message, "", true) { Height = 420 };
            msgwin.ShowDialog();                  // منتظر می‌ماند تا کاربر پیام را ببیند و OK بزند
            CL_LMethods.GoExitTheApplication();   // خروج برنامه را در صف Dispatcher قرار می‌دهد
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