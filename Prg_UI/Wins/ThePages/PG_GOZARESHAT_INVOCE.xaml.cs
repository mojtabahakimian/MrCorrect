using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    /// <summary>
    /// Interaction logic for PG_GOZARESHAT_INVOCE.xaml
    /// </summary>
    public partial class PG_GOZARESHAT_INVOCE : Page
    {
        public PG_GOZARESHAT_INVOCE()
        {
            InitializeComponent();
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SAYER_INVOCE());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FROOSHDAY, null);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH, null);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FK, null);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CFRALL, null);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CKRALL, null);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_AMFDAY, null);
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST, null);
        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_KLIST, null);
        }

        private void Image_PreviewMouseDown_9(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_FROOSH, null);
        }

        private void Image_PreviewMouseDown_10(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH, null);
        }

        private void Image_PreviewMouseDown_11(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_RK, null);
        }

        private void Image_PreviewMouseDown_12(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_VAZ, null);
        }

        private void Image_PreviewMouseDown_13(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_FRKMA4, null);
        }

        private void Image_PreviewMouseDown_14(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_HBGHB, null);
        }

        private void Image_PreviewMouseDown_15(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_F12, null);
        }

        private void Image_PreviewMouseDown_16(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_ADVANC_F12, null);
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CFRALL, null);
        }

        private void Image_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CKRALL, null);
        }

        private void Image_PreviewMouseLeftButtonUp_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_AMFDAY, null);
        }

        private void Image_PreviewMouseLeftButtonUp_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST, null);
        }

        private void Image_PreviewMouseLeftButtonUp_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_KLIST, null);
        }
    }
}
