using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.Taarif;

namespace Wins.ThePages
{
    public partial class PG_TAARIF : Page
    {
        public PG_TAARIF()
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
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.STUF_DEF_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TOTA_HES_SHEET_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.KHAD_DEF, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_ANBAR_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_BANKS_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void WrapPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_VAHEDS_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_STUFGROUP_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CUSTKIND_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEPART_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_9(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.SHIFT_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_10(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCOD_HESGROUP_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_11(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AUTOMATIC, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewKeyDown(object sender, KeyEventArgs e)
        {
    
        }

        private void Image_PreviewMouseDown_12(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.TCODE_MENUITEM_WIN, null);
        }

        private void Image_PreviewMouseDown_13(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MASRAF_CENTER, null);
        }
    }
}
