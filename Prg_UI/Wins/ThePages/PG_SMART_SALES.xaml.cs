using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.Taarif;

namespace Wins.ThePages
{
    public partial class PG_SMART_SALES : Page
    {
        public PG_SMART_SALES()
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
            PageManagement.OpenPage(new PG_KHARIDFORUSH());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AZAE_WIN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.GRADE_FORMAT_WIN, null);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_GHEYMAT, null);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_TAKHFIF, null);
        }
    }
}
