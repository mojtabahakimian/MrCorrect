using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.HESABDARI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.HESABDARI;
using Wins.WinMenus.KHARID_FORUSH;

namespace Wins.ThePages
{
    public partial class PG_HESABDARI_SUB : Page
    {
        public PG_HESABDARI_SUB()
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
            PageManagement.OpenPage(new PG_SODURESANAD());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_GOZARESH_SANAD());
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_MOGHAYERAT, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_SEARCH_MAIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Label_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_TARAZHA());
        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_LST_SEARCH, null); //جستجو در شرح عملکرد خزانه
        }

        private void Image_PreviewMouseDown_9(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ZASESABBEESAB_REPLACE_HESAB, null);
        }

        private void Image_PreviewMouseDown_10(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ADAMTARAZ, null);
        }

        private void WrapPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_AMVAL, null);
        }
    }
}
