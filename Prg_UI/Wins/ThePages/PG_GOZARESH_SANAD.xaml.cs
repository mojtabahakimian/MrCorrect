using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.HESABDARI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_GOZARESH_SANAD : Page
    {
        public PG_GOZARESH_SANAD()
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
            //new F_MENU_KOL_MOIN_TAFZIL("TAF").Show(); //چاپی
            //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, null /*DEFAULT OWNER MAIN*/ , "TAF");
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_TAF, null /*DEFAULT OWNER MAIN*/ , "TAF");
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN_LIMITED, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder_LIST, null /*DEFAULT OWNER MAIN*/);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_DFTROOS, default); // دفتر روزنامه
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_DATE, default);  //دفتر کل
        }

        private void Image_PreviewMouseDown_8(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_DATE, default); //دفتر معین
        }

        private void Image_PreviewMouseDown_9(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.R_DAFTAR_KOL_kh, default); //چاپ خلاصه دفتر کل
        }

        private void Image_PreviewMouseDown_10(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_KOL_MOIN_TAFKIK, default); //حساب تفکیکی تفضیلی
        }

        private void Image_PreviewMouseDown_11(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_DPDAY, default); //گزارش عملکرد روزانه خزانه
        }

        private void Image_PreviewMouseDown_12(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_PRINTHES, default); //چاپ سرفصل حسابها
        }

        private void Image_PreviewMouseDown_13(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.ADAMTARAZ, default); //لیست اسنادی که تراز نیستند
        }

        private void Image_PreviewMouseDown_14(object sender, MouseButtonEventArgs e)
        {

            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_ASNAD_PRINT, default); //لیست اسنادی که تراز نیستند
        }
    }
}
