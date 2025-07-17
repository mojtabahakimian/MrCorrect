using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.ANBAR;

namespace Wins.ThePages
{
    public partial class PG_ANBAR : Page
    {
        public PG_ANBAR()
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
            PageManagement.OpenPage(new PG_ANBAR_GOZARESHS());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //new HEAD_LST_HAVL().Show();
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID_OTHER_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ANBGRD_HEAD_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_REQUEST_WIN, null /*DEFAULT OWNER MAIN*/);
        }
    }
}
