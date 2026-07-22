using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prg_UI.HelperWins;

namespace Wins.ThePages
{
    public partial class PG_CHECK_SAYER : Page
    {
        public PG_CHECK_SAYER()
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
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHAPCHEK, null);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHKB, null);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS, null);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHRE_LSPH, null);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_SERILA, null);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_PLISTS, null);
        }

        private void Image_PreviewMouseDown_6(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHEKS_PBESTANKAR, null);
        }

        private void Image_PreviewMouseDown_7(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHEKS_BESTANKAR, null);
        }

        private void WrapPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_CHREC_HES_BEHESABCHECK, default);
        }

        private void WrapPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHKB, default);
        }

        private void Image_PreviewMouseDown_Serial(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHRE_LSPH, null);
        }

        private void Image_PreviewMouseDown_Sandogh(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_CHEK_CHKM, null);
        }

        private void Image_PreviewMouseDown_DaftarRuzane(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "دفتر روزانه چک بر اساس تاریخ سررسید و وضعیت وصول چک‌ها در سیستم به صورت پویا و خودکار به‌روزرسانی و گزارش می‌شود.").ShowDialog();
        }

        private void Image_PreviewMouseDown_DasteChek(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "تعریف دسته‌چک و حساب مستقیماً در پنجره تعریف حساب‌ها (کدینگ حسابداری) و حساب‌های بانکی انجام می‌گیرد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_PrintChek(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "تعریف قالب چاپی چک‌ها (میانبر Ctrl + F6) در این نسخه از طریق ابزار هوشمند طراحی قالب چاپ چک انجام می‌گردد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_BeHesabGozashte(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "فهرست چک‌های به‌حساب‌گذاشته‌شده در گزارش عملکرد روزانه و دفتر معین بانک‌های واگذارنده به صورت کامل قابل مشاهده و ردیابی است.").ShowDialog();
        }

        private void Image_PreviewMouseDown_PrintBeHesab(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "تعریف قالب چاپی برگه به‌حساب‌گذاشتن (میانبر Shift + F6) در این نسخه به صورت خودکار و از طریق قالب هوشمند واگذاری چک انجام می‌شود.").ShowDialog();
        }
    }
}
