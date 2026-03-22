using AUTO_BAZ.HelperWins;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.SPECIAL_F1;
using Prg_UI.Wins.WinSetting;
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wins.ThePages;

namespace Prg_UI.Wins
{
    public partial class WinBase : Window
    {
        public class VSH_MODEL
        {
            public string? SHNAME { get; set; }
            public string? DEPNAME { get; set; }
        }
        public WinBase()
        {
            InitializeComponent();

            LBL_VERSION.Content = CL_VERSION.MrCorrectFullVersion;
            LBL_CurrentUser.Content = CL_HESABDARI.UCurrentUser();

            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
            var VSH = dbms.DoGetDataSQL<VSH_MODEL>(@$"SELECT dbo.SHIFT.SHNAME,
                                                          dbo.DEPART.DEPNAME
                                                   FROM dbo.SHIFT
                                                       CROSS JOIN dbo.DEPART
                                                   WHERE (dbo.SHIFT.SHIFT_ID = {CL_Generaly.SHIFT_OF_USER}) AND (dbo.DEPART.DEPATMAN = {CL_Generaly.VAHED_OF_USER});").FirstOrDefault();

            LBL_VahedAndShift.Content = $"واحد : {VSH?.DEPNAME?.FixPersianChars()} | شیفت : {VSH?.SHNAME?.FixPersianChars()}";
            SERVERDB.Content = $"{CL_Generaly.General_Servername} | {CL_Generaly.General_DBname}";
            VSH = null; dbms = null; //Tiny Clear

            this.MaxHeight = CL_LMethods.GetWindowSizeLessTaskbaer();
            WindowState = WindowState.Maximized;

            GeneralOptionManager.InitializeSync(Baseknow.USERCOD);
            GeneralOptionManager.InitializeThemeSync(Baseknow.USERCOD);

            var _paletteHelper = new PaletteHelper();
            var _theme = _paletteHelper.GetTheme();
            ThemeExtensions.SetBaseTheme(_theme, GeneralOptionManager.CachedIsDarkMode ? BaseTheme.Dark : BaseTheme.Light);
            try { _theme.SetPrimaryColor((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(GeneralOptionManager.CachedPrimaryColor)); } catch { }
            _paletteHelper.SetTheme(_theme);

            if (GeneralOptionManager.IsRDPMode)
            {
                RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
                CL_LMethods.OptimizeForRemoteDesktop(this);

                BTN_INFO.Text = Baseknow.YEA.ToString() + " " + "RDP";
            }
            else
            {
                BTN_INFO.Text = Baseknow.YEA.ToString();
            }
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PageManagement.TheMainFame = MainorginFrame;

            HEADINFO.Content = Baseknow.WIDTH_D + " سال " + Baseknow.YEA;

            FreeMemory();

            CL_Generaly.IsMrCorrectLoadedWinBase = true;

            CL_Keyboard.ChangeKeyboardLayout("Farsi");

            this.Title = Baseknow.YEA + " " + Baseknow.WIDTH_D;
        }

        private void FreeMemory()
        {
            try
            {
                //////// Enable SoftwareOnly RenderMode to avoid hardware effect. (Switch Graphic proccess to CPU instead of RAM)
                //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                ////////Reduce CPU Consumption for WPF Animations
                //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 15 });

                //////// Force garbage collection and wait for finalizers
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false); ////////Optional
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
                GC.WaitForPendingFinalizers();

                //////// Compact the Large Object Heap (LOH) only when necessary
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                //////// Reduce the working set size (physical memory usage) : Force OS to give me virual memory (Hard Disk Memory) that can lead performance problem
                //SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
            }
            catch { }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //PublicVRB.PreloadHi_Win.Close();


        }
        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (GC.GetTotalMemory(false) > MemoryThreshold)
            //{
            //    FreeMemory();
            //}

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
        }

        private void Expandator(Expander expander)
        {
            if (expander.IsExpanded)
                expander.IsExpanded = false;
            else
                expander.IsExpanded = true;
        }

        private void Btn_PreInvoce_Click(object sender, RoutedEventArgs e)
        {
            //PFACTFR
            //CL_HESABDARI.RunForm("PFACTFR");

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this);
        }

        private void Btn_PreInvoce_Showall_Click(object sender, RoutedEventArgs e)
        {
            //HINVOLST hINVOLST = new HINVOLST(20);
            //hINVOLST.ShowDialog();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 20);
        }

        private void Btn_MainAutomation_Click(object sender, RoutedEventArgs e)
        {
            //MAIN automasion = new MAIN(); automasion.Show();
        }

        private void HaleCover_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //HaleCover.Visibility = Visibility.Collapsed;
        }

        private void Pishcafor_Click(object sender, RoutedEventArgs e)
        {
            //CL_HESABDARI.RunForm("PFACTFR");
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this);
        }

        private void Automasion_Btn_Click(object sender, RoutedEventArgs e)
        {
            Expandator(Automasion_Expnd);
        }

        private void Automasion_Click(object sender, RoutedEventArgs e)
        {
            //MAIN.MAIN_INST.Show();
            //MAIN mAIN = new MAIN(); mAIN.Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.Automasion_MAIN, this);
        }

        private void SeeAllPsifactors_Click(object sender, RoutedEventArgs e)
        {
            //HINVOLST hINVOLST = new HINVOLST(20); hINVOLST.Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 20);
        }

        private void HaleCover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HaleCover.Visibility == Visibility.Visible)
            {
                CloserPnl.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }
        /// <summary>
        /// Open or Clsoe Panel
        /// </summary>
        /// <param name="IsOpenner"></param>
        private void DoRandAnimate(bool IsOpenner)
        {
            try
            {
                Random rnd = new Random();
                int NumberAnimate = rnd.Next(1, 7);

                if (IsOpenner)
                {
                    //HaleCover.Visibility = Visibility.Visible; // Coming Black

                    Storyboard MyStoryBord = this.FindResource($"SlideComeShow{NumberAnimate}") as Storyboard;
                    MyStoryBord.Begin();
                    //DoEvents();

                }
                if (!IsOpenner) //Is Closer
                {
                    //HaleCover.Visibility = Visibility.Collapsed; // Hiden Black to White

                    Storyboard MyStoryBord = this.FindResource($"SlideComeHide{NumberAnimate}") as Storyboard;
                    MyStoryBord.Begin();
                    //DoEvents();
                }
            }
            catch (Exception ex)
            {
                var test = ex.ToStringNullSafe();
                throw;
            }
            this.UpdateLayout();
            //this.InvalidateVisual();
        }

        private void CloserPnl_Click(object sender, RoutedEventArgs e)
        {
            //بسته شدن 
            CloserPnl.IsEnabled = false;
            Random rnd = new Random();
            //var values = new[] { 1, 2, 3 };
            //int NumberAnimate = values[rnd.Next(values.Length)]; ;
            int NumberAnimate = rnd.Next(1, 4);

            HaleCover.Visibility = Visibility.Collapsed;

            Storyboard MyStoryBord = this.FindResource($"EF{NumberAnimate}_RV") as Storyboard;
            MyStoryBord.Begin();
            //DoEvents();
            this.UpdateLayout();

            CloserPnl.IsEnabled = true;
        }

        private void OpenerPnl_Click(object sender, RoutedEventArgs e)
        {
            // باز شدن
            OpenerPnl.IsEnabled = false;

            Random rnd = new Random();
            //var values = new[] { 1, 2, 3};
            //int NumberAnimate = values[rnd.Next(values.Length)]; ;
            int NumberAnimate = rnd.Next(1, 4);

            HaleCover.Visibility = Visibility.Visible;

            Storyboard MyStoryBord = this.FindResource($"EF{NumberAnimate}") as Storyboard;
            MyStoryBord.Begin();
            //DoEvents();
            this.UpdateLayout();

            OpenerPnl.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //MAIN mn = new MAIN();
            //mn.ShowDialog();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از خروج مطمئن هستید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == true)
            {
                this.Close();
                CL_LMethods.GoExitTheApplication();
            }
        }

        private void JostejugareMogudi_Click(object sender, RoutedEventArgs e)
        {
            //MOGUDI_SEARCH_MAIN mOGUDI_SEARCH_MAIN = new MOGUDI_SEARCH_MAIN();
            //mOGUDI_SEARCH_MAIN.Show();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGUDI_SEARCH_MAIN, this);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //new BUDGET0().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BUDGET0, this);
        }

        private void Glorybtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //MAIN mAIN = new MAIN(); mAIN.Show();
        }

        private void Glorybtn_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            MaterialThemSettingy materialThemSettingy = new MaterialThemSettingy(); materialThemSettingy.ShowDialog();
        }

        private void CONTROLF8_Click(object sender, RoutedEventArgs e)
        {
            //F_MENU_KOL_MOIN_TAFZIL f_MENU_KOL_MOIN_TAFZIL = new F_MENU_KOL_MOIN_TAFZIL();
            //f_MENU_KOL_MOIN_TAFZIL.Show();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
        }

        private void Nabzeforoosh_Click(object sender, RoutedEventArgs e)
        {
            //NABZEFROOSH nABZEFROOSH = new NABZEFROOSH(); nABZEFROOSH.Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEFROOSH, this);
        }

        private void NabzeMali_Click(object sender, RoutedEventArgs e)
        {
            //NABZEMALI mali = new NABZEMALI(); mali.Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEMALI, this);
        }

        private void NabzeFaliat_Click(object sender, RoutedEventArgs e)
        {
            //NABZEDARY nABZEDARY = new NABZEDARY(); nABZEDARY.Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEDARY, this);
        }

        private void DashboardModiriatiBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //PageManagement.OpenPage(new PG_NABZHA());
        }
        private void HAVLE_ANBAR_FOROOSH_Click(object sender, RoutedEventArgs e)
        {
            //HEAD_LST_HAVL hEAD_LST_HAVL = new HEAD_LST_HAVL();
            //hEAD_LST_HAVL.Show();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, this);
        }

        private void Glorybtn_PreviewMouseLeftButtonUp_2(object sender, MouseButtonEventArgs e)
        {
            //PageManagement.OpenPage(new SALARY_DISK());
        }

        private void LIST_HAVALEHA_Click(object sender, RoutedEventArgs e)
        {
            //HAVALE_LIST hAVALE_LIST = new HAVALE_LIST();
            //hAVALE_LIST.Show();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, 2);
        }

        private void FactorMostaghim_Click(object sender, RoutedEventArgs e)
        {
            //CL_HESABDARI.RunForm("FACTFR"); //FACTORS.FACTFR
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this);
        }

        private void FactoreKharid_Click(object sender, RoutedEventArgs e)
        {
            //new HEAD_LST_KHAREED1().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, this);
        }

        private void HEAD_LST_RASID_Click(object sender, RoutedEventArgs e)
        {
            //new HEAD_LST_RASID().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this);
        }

        private void Glorybtn_PreviewMouseLeftButtonUp_3(object sender, MouseButtonEventArgs e)
        {
            //PageManagement.OpenPage(new PG_HESABDARI());
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            //CL_LMethods.GoExitTheApplication();
        }

        private bool isEnglishLayout = true;
        private void Window_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                PageManagement.DisplayPageManagement(e, MainorginFrame);
            }
            else if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                App.SEARCHMENIU_WIN_ST_Show();
            }
            else if (e.Key == Key.F1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                new WINF1().ShowDialog();
            }

            if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                CL_Keyboard.ChangeKeyboardLayout("English"); // انگلیسی
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Space))
            {
                CL_Keyboard.ChangeKeyboardLayout("Farsi"); // فارسی
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.D5))
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_LIST_KALA_CTRL5, this);
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F11)
            {
                // Verify that the F11 key was pressed.
                if (e.Key == Key.F11)
                {
                    e.Handled = true;

                    WinConnectionChoose choosing_Connection = new WinConnectionChoose();
                    choosing_Connection.ShowDialog();
                }
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Control && e.Key == Key.F12)
            {
                e.Handled = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_ADVANC_F12, default);
                });
            }




            #region ALTOSHIFT
            if (e.IsRepeat)
                return;

            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                e.Handled = true;

                if (isEnglishLayout)
                {
                    isEnglishLayout = false;
                    CL_Keyboard.ChangeKeyboardLayout("English"); // Change to English layout.
                }
                else
                {
                    isEnglishLayout = true;
                    CL_Keyboard.ChangeKeyboardLayout("Farsi");   // Change to Farsi layout.
                }
            }
            #endregion

            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.W)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this);
            //}
            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.E)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, this);
            //}
            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.I)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this);
            //}
            //else if ((e.Key == Key.F3 || e.SystemKey == Key.F3) && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, this);
            //}
            //else if ((e.Key == Key.F4 || e.SystemKey == Key.F4) && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this);
            //}
            //else if ((e.Key == Key.F10 || e.SystemKey == Key.F10) && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this);
            //}
            //else if ((e.Key == Key.F5 || e.SystemKey == Key.F5) && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this);
            //}
            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F8 || e.SystemKey == Key.F8))
            //{
            //    //e.Handled = true;
            //    //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, this);
            //}
            //else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && (e.Key == Key.F7 || e.SystemKey == Key.F7))
            //{
            //    e.Handled = true;
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, this);
            //}
        }

        private void FactorMostaghim_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }



        private void SettingThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            new MaterialThemSettingy().ShowDialog();
        }

        private void CNNSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            new WinConnectionChoose().ShowDialog();
        }

        private void Khazaneh_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this);
        }
        private void hesabdari_btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_HESABDARI());
        }

        private void KhazanehLST_Click(object sender, RoutedEventArgs e)
        {
            //new PGET_HED_LIST().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED_LIST, this);
        }

        private void MFINDER_BTN_Click(object sender, RoutedEventArgs e)
        {
            App.SEARCHMENIU_WIN_ST_Show();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //new DEED_HEAD().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD_LIST, this);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, this);
        }

        private void FactorMostaghim_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BUGET_MAIN_list2, this);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.STUF_DEF_WIN, this);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            //new FCODE_CUSTOMER().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, this);
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TOTA_HES_SHEET_WIN, this);
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.KHAD_DEF, this);
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_ANBAR_WIN, this);
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_BANKS_WIN, this);
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_VAHEDS_WIN, this);
        }

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_STUFGROUP_WIN, this);
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CUSTKIND_WIN, this);
        }

        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEPART_WIN, this);
        }

        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.SHIFT_WIN, this);
        }

        private void Button_Click_19(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_HESGROUP_WIN, this);
        }

        private void image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_BUDGETS());
        }

        private void Label_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SMART_SALES());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SALARY());
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_TOLID());
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SANATI());
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_EMPM());
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_MYSETTINGS());
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SMART_SALES());
        }

        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AUTOMATIC, this);
        }

        private void image1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_DASHBOARD());
        }

        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2, this);
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHADAMAT, this);
        }

        private void Button_Click_23(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK, this);
        }

        private void Button_Click_24(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID_OTHER_WIN, this);
        }

        private void Button_Click_25(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN, this);
        }

        private void Button_Click_26(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ANBGRD_HEAD_WIN, this);
        }

        private void Button_Click_27(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGHAYERAT, this);
        }

        private void Button_Click_28(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_MOGHAYERAT, this);
        }

        private void Button_Click_29(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder, this);
        }

        private void Button_Click_30(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_SEARCH_MAIN, this);
        }

        private void ENTEGHAL_ANBAR_Click(object sender, RoutedEventArgs e)
        {
            //new HEAD_LST_ENTEGHAL_WIN().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN, this);
        }

        private void Button_Click_31(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN, this);
        }

        private void Button_Click_32(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN_LIMITED, this);
        }

        private void Button_Click_33(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_REQUEST_WIN, this);
        }

        private void PAYLIST_BTN_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder_LIST, this);
        }

        private void FACTORS_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FACTORS_LST, this, Convert.ToByte(13));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ZASESABBEESAB_REPLACE_HESAB, this);
        }

        private void Button_Click_34(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AZAE_WIN, this);
        }

        private void Button_Click_35(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCODE_MENUITEM_WIN, this);
        }

        private void Button_Click_36(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MASRAF_CENTER, this);
        }

        private void Button_Click_37(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_FRCUST, this);
        }

        private void Button_Click_38(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_FASLIBR, this);
        }

        private void Button_Click_39(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_FASLIKHBR, this);
        }

        private void Button_Click_40(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_KHCUST, this);
        }

        private void Button_Click_41(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_KHLS, this);
        }

        private void Button_Click_42(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_LFACT, this);
        }

        private void Button_Click_43(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FROOSHDAY, null);
        }

        private void Button_Click_44(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FKHAREDAY, this);
        }

        private void Button_Click_45(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH, this);
        }

        private void Button_Click_46(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_MOGUDI_ANBARHA.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            //report.Render();
            //report.Show();
            new WINRPT(report, "گزارش موجودی کل انبار ها").Show();
        }

        private void Button_Click_47(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MOGUDI_ANBARHA_LIST, this);
        }

        private void Button_Click_48(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_R, this);
        }

        private void Button_Click_49(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST, this);
        }

        private void Button_Click_50(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_MERG, this);
        }

        private void Button_Click_51(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, this, "KARTR2");
        }

        private void Button_Click_52(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_GRP, this);
        }

        private void Button_Click_53(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.C_TARAZ_ANBAR_KHAS, this);
        }

        private void Button_Click_54(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_F, this);
        }

        private void Button_Click_55(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_R, this);
        }

        private void Button_Click_56(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_TANBGRP, this);
        }

        private void Button_Click_57(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_FD, this);
        }

        private void Button_Click_58(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CVP, this);
        }

        private void Button_Click_59(object sender, RoutedEventArgs e)
        {
            //var ThereOpenIsWindowBefore = Application.Current.Windows.OfType<WIN_SAZMAN>().Any();
            //if (ThereOpenIsWindowBefore)
            //{
            //    new Msgwin(false, "این پنجره از قبل هنوز باز است").ShowDialog();
            //    return;
            //}

            //new WIN_SAZMAN().Show();

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SAZMAN_MOSHAKHASAT, this);
        }

        private void Image_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_MYSETTINGS());
        }

        private void Button_Click_60(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_ENTER, this); //برگه ورود
        }

        private void Button_Click_61(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_EXIT, this); //برگه خروج
        }

        private void Button_Click_62(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_DFTROOS, this); // دفتر روزنامه
        }

        private void Button_Click_63(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_DATE, this);  //دفتر کل
        }

        private void Button_Click_64(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.R_DAFTAR_KOL_kh, this); //چاپ خلاصه دفتر کل
        }

        private void Button_Click_65(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_KOL_MOIN_TAFKIK, this); //حساب تفکیکی تفضیلی
        }

        private void Button_Click_66(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_DPDAY, this); //گزارش عملکرد روزانه خزانه
        }

        private void Button_Click_67(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_PRINTHES, this); //چاپ سرفصل حسابها
        }

        private void Button_Click_68(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ADAMTARAZ, this); //لیست اسنادی که تراز نیستند
        }

        private void Button_Click_69(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_DATE, this); //دفتر معین
        }

        private void Button_Click_70(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4, this); //گزارش تراز های آزمایشی حسابهای معین
        }

        private void Button_Click_71(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_MARKAZ, this); //لیست مراکز هزینه
        }

        private void Button_Click_72(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_SND, this); //ترازنامه
        }

        private void Button_Click_73(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_MANABE, this); //صورت منابع و مصارف وجوه نقد (گردش وجوه نقد)
        }

        private void Button_Click_74(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_USER_PERMITION_FORMS_DASTRASI, this); //تعیین سطح دسترسی
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false,
                $"نام : {Baseknow.WIDTH_D}" +
                $"\nسال : {Baseknow.YEA}" +
                $"\nنام سرور : {CL_Generaly.General_Servername}" +
                $"\nدیتابیس : {CL_Generaly.General_DBname}" +
                $"\nکاربری : {Baseknow.UUSER}" +
                $"\nکاربر فعلی ویندوز : {Environment.UserName}" +
                $"\nدامنه ویندوز : {Environment.UserDomainName}" +
                $"\nسیستم عامل : {Environment.OSVersion}" +
                $"\nتاریخ امروز : {DateTime.Now:yyyy/MM/dd HH:mm:ss}"
                )
            { Height = 350 }.ShowDialog();
        }

        private void Button_Click_75(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.R_TARAZ_ANBARHA, this); //گزارش تراز موجودی کل انبار ها
        }

        private void Button_Click_76(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITDLV, this); //گزارش عملکرد ویزیتور ها
        }

        private void Button_Click_77(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITKAL, this); //گزارش عملکرد ویزیتور ها به تفکیک کالا (برگشتی در ماه فروش)
        }

        private void Button_Click_78(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITKAL, this); //گزارش عملکرد ویزیتور ها به تفکیک کالا (برگشتی در تاریخ برگشت)
        }

        private void Button_Click_79(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.flist_porsant_factors, this); //گزارش عملکرد ویزیتور ها به تفکیک کالا (برگشتی در تاریخ برگشت)
        }

        private void Button_Click_80(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITONE, this); //لیست فروش ویزیتور به تفکیک فاکتور
        }

        private void Button_Click_81(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_AJNAS, this); //چاپ لیست کالا ها جهت ویزیت
        }

        private void Button_Click_82(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_GOL_REP, this); //مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه
        }

        private void Button_Click_83(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_GOL_GRP_REP, this); //مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه بر اساس گروه
        }

        private void Button_Click_84(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_GOL_GRP_REP_MAR, this); //مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه بر اساس گروه (در تاریخ برگشت)
        }

        private void Button_Click_85(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_GOL_REP_MAR, this); //مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه (در تاریخ برگشت)
        }

        private void Button_Click_86(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_TOZIE, this); //تنظیم لیست دستی توضیع
        }

        private void Button_Click_87(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITOR_DAY_HEAD, this); //تعریف ویزیت روزانه برای ویزیتور
        }

        private void Button_Click_88(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_VISIT_ROUTE_FORM, this); //تعيين مسير وزيت براي مشتريان
        }

        private void Button_Click_89(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_GHEYMAT, this); //تعریف اعلامیه قیمت
        }

        private void Button_Click_90(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CRMMAIN, this); //مدیریت ارتباط با مشتری CRM

        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CRMMAIN, this); //مدیریت ارتباط با مشتری CRM
        }

        private void Button_Click_91(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_FROOSH, null);
        }

        private void Button_Click_92(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH, null);
        }

        private void Button_Click_93(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_RK, null);
        }

        private void Button_Click_94(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_VAZ, null);
        }

        private void Button_Click_95(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_FRKMA4, null);
        }

        private void Button_Click_96(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_HBGHB, null);
        }

        private void Button_Click_97(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FK, null);
        }

        private void Button_Click_98(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CFRALL, null);
        }

        private void Button_Click_99(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CKRALL, null);
        }

        private void Button_Click_100(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_AMFDAY, null);
        }

        private void Button_Click_101(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST, null);
        }

        private void Button_Click_102(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST, null);
        }

        private void Button_Click_103(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.RAAS, null);
        }

        private void Button_Click_104(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHKB, null);
        }

        private void Button_Click_105(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS, null);
        }

        private void Button_Click_106(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CONTROL_ASNAD_DAFATERCHECK_FORM11, default);
        }

        private void Button_Click_107(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_MARKAZHAZ_WIN, null);
        }

        private void Button_Click_108(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHRE_LSPH, null);
        }

        private void Button_Click_109(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_RCHEKD, null);
        }

        private void Button_Click_110(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_PCHSS, null);
        }

        private void Button_Click_111(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHKM, null);
        }

        private void Button_Click_112(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_SERILA, null);
        }

        private void Button_Click_113(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4, null);
        }

        private void Button_Click_114(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4M, null);
        }

        private void Button_Click_115(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4T, null);
        }

        private void Button_Click_116(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_RT4, null);
        }

        private void Button_Click_117(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_TOLID, null);
        }

        private void Button_Click_118(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_FROOSH_KOL, null);
        }

        private void Button_Click_119(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_FROOSH_KOL_ALL, null);
        }

        private void Button_Click_120(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_F12, null);
        }

        private void Button_Click_121(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_ADVANC_F12, null);
        }

        private void Button_Click_122(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_AMMAS, null);
        }

        private void Button_Click_123(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NEWPASSWORD, null);
        }

        private void Button_Click_124(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.USER_CHANGE, null);
        }

        private void Button_Click_125(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEFAULT, null);
        }

        private void Button_Click_126(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_HEAD_MANF_FORMULSAKHT, default);
        }

        private void Button_Click_127(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.USERS, default);
        }

        private void Button_Click_128(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_About, default);
        }

        private void Button_Click_129(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TAKHFIF_DEF_FORM, default);
        }

        private void Button_Click_130(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_GRP_FORM_GRUHBANDI_GHEYMATI, default);
        }

        private void Button_Click_131(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_GRADE_SHART_FUNC_FORM, default);
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_PLISTS, default);

        }

        private void Button_Click_132(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHEKS_PBESTANKAR, null);
        }

        private void Button_Click_133(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHEKS_BESTANKAR, null);
        }

        private void Button_Click_134(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_AMVAL, null);
        }

        private void Button_Click_135(object sender, RoutedEventArgs e)
        {
            // نمایش پیام تأیید به کاربر
            Msgwin msgwin = new Msgwin(true, "آیا از ری استارت برنامه و خروج از حساب کاربری فعلی مطمئن هستید؟");
            msgwin.ShowDialog();

            if (msgwin.DialogResult == true)
            {
                // مرحله 1: بررسی اینکه آیا پنجره دیگری باز است یا خیر
                List<Window> openChildWindows = new List<Window>();
                string mainWinName = "Prg_UI.Wins.WinBase"; // نام کامل پنجره اصلی شما
                foreach (Window window in Application.Current.Windows)
                {
                    // پنجره‌هایی را پیدا کن که پنجره اصلی نیستند
                    if (window.GetType().FullName != mainWinName)
                    {
                        try { window?.Close(); } catch (Exception) { }
                        //openChildWindows.Add(window);
                    }
                }
                if (openChildWindows.Any())
                {
                    // اگر پنجره‌های دیگری باز هستند، به کاربر هشدار بده و خارج شو
                    string openWindowsList = string.Join("\n- ", openChildWindows.Select(w => w.Title));
                    new Msgwin(false, "امکان راه‌اندازی مجدد وجود ندارد.\nلطفاً ابتدا تمام پنجره‌های باز زیر را ببندید:\n\n- " + openWindowsList).ShowDialog();
                    return;
                }

                // 1. بستن همه پنجره‌های باز به جز WinBase (که خودش بسته خواهد شد)
                CloseAllChildWindows();

                // 2. پاک کردن PageManagement و navigation stack
                ClearPageManagement();

                // 3. پاک کردن اطلاعات جلسه کاربری
                Baseknow.USERCOD = 0;
                Baseknow.UUSER = string.Empty;
                Baseknow.UGRP = string.Empty;
                // 4. پاک کردن تنظیمات واحد و شیفت
                CL_Generaly.SHIFT_OF_USER = 0;
                CL_Generaly.VAHED_OF_USER = 0;
                CL_Generaly.IsMrCorrectLoadedWinBase = false;

                // 6. اجرای Garbage Collection برای آزادسازی حافظه
                try
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                catch { }

                // 7. ایجاد پنجره لاگین جدید
                USER_LOGIN loginWindow = new USER_LOGIN();
                // 8. بستن پنجره فعلی
                this.Close();
                // 9. نمایش پنجره لاگین
                loginWindow.ShowDialog();
            }
        }
        /// <summary>
        /// بستن تمام پنجره‌های باز به جز WinBase
        /// </summary>
        private void CloseAllChildWindows()
        {
            try
            {
                // لیست پنجره‌هایی که باید بسته شوند
                var windowsToClose = Application.Current.Windows
                    .OfType<Window>()
                    .Where(w => w != this && !(w is USER_LOGIN))
                    .ToList();

                // بستن تمام پنجره‌ها
                foreach (var window in windowsToClose)
                {
                    try
                    {
                        window.Close();
                    }
                    catch
                    {
                        // اگر پنجره‌ای نتوانست بسته شود، ادامه بده
                    }
                }
            }
            catch
            {
                // در صورت هر گونه خطا، ادامه بده
            }
        }
        /// <summary>
        /// پاک کردن PageManagement و navigation stack
        /// </summary>
        private void ClearPageManagement()
        {
            try
            {
                // پاک کردن Frame
                if (PageManagement.TheMainFame != null)
                {
                    PageManagement.TheMainFame.Content = null;
                    PageManagement.TheMainFame = null;
                }

                // پاک کردن navigation stack با استفاده از reflection
                var stackField = typeof(PageManagement).GetField("navigationStack",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (stackField != null)
                {
                    var stack = stackField.GetValue(null) as System.Collections.Generic.Stack<Page>;
                    stack?.Clear();
                }
            }
            catch
            {
                // در صورت هر گونه خطا، ادامه بده
            }
        }

        private void Button_Click_136(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHREC_HES_BEHESABCHECK, default);
        }

        private void Button_Click_137(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_138(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.VISITORS_PORSANT_HEAD, default);
        }

        private void Button_Click_139(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZMOSH_KARSHENASH, default);
        }

        private void Button_Click_140(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ASNAD_PRINT, default);
        }

        private void Button_Click_141(object sender, RoutedEventArgs e)
        {
            //گزارش تراز آزمایشی چهار ستونی تفضیلی
        }

        private void Button_Click_142(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALE_EXIT_SAYER, default);
        }

        private void Button_Click_143(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_SADERATI, default);
        }

        private void Button_Click_144(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_SADERATI, default);
        }

        private void Button_Click_145(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FROOSH_NARAFTAH, default);
        }

        private void Button_Click_146(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FONARP, default);
        }

        private void Button_Click_147(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CUSTNP, default);
        }

        private void Button_Click_148(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_KHARID, default);
        }

        private void Button_Click_149(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_FROOSH, default);

        }

        private void Button_Click_150(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FR, default);
        }

        private void Button_Click_151(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_ORDR_HED_SEFARESH, default);
        }

        private void Button_Click_152(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_VISITGOL_HEAD_AHDAF, default);
        }

        private void Button_Click_153(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_PAYGETD_LST, default);
        }

        private void Button_Click_154(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_PAYGETP_LST, default);
        }

        private void Button_Click_155(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHREC_H, default);
        }

        private void Button_Click_156(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHREC_HP, default);
        }

        private void Button_Click_157(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHECK_MON, default);
        }

        private void Button_Click_158(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHKE_DLIST_KOLCHECKD, default);
        }

        private void Button_Click_159(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK, default);
        }

        private void Button_Click_160(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VOSUL_LES, default);
        }

        private void Button_Click_161(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PAY_GETD_LOG_FORM, default);
        }

        private void Button_Click_163(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHECK_MONP, default);
        }

        private void Button_Click_162(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHKE_PLIST, default);
        }

        private void Button_Click_164(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHKE_PLIST, default);

        }

        private void Button_Click_165(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHAPCHEK, default);
        }

        private void Button_Click_166(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_TAKHFIF, default);
        }

        private void Button_Click_167(object sender, RoutedEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_NEWYEAR_SALEMALI, default);
        }
    }
}
