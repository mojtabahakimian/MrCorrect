using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinMenus.Taarif;
using Prg_UI.HelperWins;

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

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_GRADE_SHART_FUNC_FORM, default);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PRICE_GRP_FORM_GRUHBANDI_GHEYMATI, default);
        }

        private void Image_PreviewMouseDown_Takhir(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_SSM_TAKHIR_PAR, null);
        }

        private void Image_PreviewMouseDown_Enhesar(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_ENHESAR, null);
        }

        private void Image_PreviewMouseDown_StatusMosh(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZMOSH_MOSHTARI, null);
        }

        private void Image_PreviewMouseDown_StatusKar(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NABZMOSH_KARSHENASH, null);
        }

        private void Image_PreviewMouseDown_GScale(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_GSCALE, null);
        }

        private void Image_PreviewMouseDown_GradeFormat(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.GRADE_FORMAT_WIN, null);
        }

        private void Image_PreviewMouseDown_Msg(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "این مورد در این نسخه تعریف نشده است و به صورت خودکار اعمال می‌گردد.").ShowDialog();
        }
    }
}
