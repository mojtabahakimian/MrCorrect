using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.BARNAME_RIZI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_TOLID : Page
    {
        public PG_TOLID()
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
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_FROOSH_KOL_ALL, null);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_FROOSH_KOL, null);
        }
    }
}
