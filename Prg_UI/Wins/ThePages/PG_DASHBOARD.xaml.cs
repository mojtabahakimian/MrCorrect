using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prg_UI.HelperWins;

namespace Wins.ThePages
{
    public partial class PG_DASHBOARD : Page
    {
        public PG_DASHBOARD()
        {
            InitializeComponent();
        }
        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEFROOSH, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEMALI, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "نبض خرید در این نسخه به صورت یکپارچه با نبض فروش و نبض مالی سازمان تحلیل و ارائه می‌گردد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZEDARY, null /*DEFAULT OWNER MAIN*/);
        }
    }
}
