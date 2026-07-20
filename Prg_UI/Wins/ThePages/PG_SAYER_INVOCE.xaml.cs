using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_SAYER_INVOCE : Page
    {
        public PG_SAYER_INVOCE()
        {
            InitializeComponent();
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FR, null);
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FROOSH_NARAFTAH, null);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FONARP, null);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CUSTNP, null);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_KHARID, null);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_FROOSH, null);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_F, null);
        }

        private void FactorDailyReport_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FR, null);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.STUF_DEF_LIST, null);
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_HES, null);
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FROOSH_NARAFTAH, null);
        }

        private void Image_PreviewMouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FONARP, null);
        }

        private void Image_PreviewMouseLeftButtonUp_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_CUSTNP, null);
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_KHARID, null);
        }

        private void Image_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.LIST_FROOSH, null);
        }
    }
}
