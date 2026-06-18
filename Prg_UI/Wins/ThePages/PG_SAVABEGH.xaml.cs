using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_SAVABEGH : Page
    {
        public PG_SAVABEGH()
        {
            InitializeComponent();
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private static void OpenHistory(CL_MenuManager.WinNameType winNameType)
        {
            CL_MenuManager.OpenWinMenu(winNameType, null /*DEFAULT OWNER MAIN*/);
        }

        private void SavabeghFactorFroosh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_FACTOR_FROOSH);
        }

        private void SavabeghFactorKharid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_FACTOR_KHARID);
        }

        private void SavabeghBargashtFroosh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_FROOSH_RETURN_NORMAL);
        }

        private void SavabeghBargashtKharid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_KHARID_RETURN_NORMAL);
        }

        private void SavabeghKhadamat_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_FACTOR_KHADAMAT);
        }

        private void SavabeghHavaleFroosh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_HAVALE_ANBAR);
        }

        private void SavabeghResidKharid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_RASID_ANBAR);
        }

        private void SavabeghTolid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_VOROD_KALA_SAKHTEH);
        }

        private void SavabeghKhorojMavad_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_KHOROJ_MAVAD_AVALIYEH);
        }

        private void SavabeghKhorojSayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_KHOROJ_SAYER_MAVAD);
        }

        private void SavabeghEnteghalAnbar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_ANBAR_ENTERGHAL);
        }

        private void SavabeghSanadDasti_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_DEED_HEAD);
        }

        private void SavabeghTarifKala_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_STUF_DEF);
        }

        private void SavabeghPishFactor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenHistory(CL_MenuManager.WinNameType.TR_HISTORY_PISH_FACTOR);
        }
    }
}
