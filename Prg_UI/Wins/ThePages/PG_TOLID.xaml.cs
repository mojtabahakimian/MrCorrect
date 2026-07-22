using Functions;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.BARNAME_RIZI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prg_UI.HelperWins;

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

        private void Image_PreviewMouseDown_Mavad(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "محاسبه مواد اولیه موردنیاز و برنامه‌ریزی تولید بر اساس فرمول‌های ساخت ثبت‌شده به صورت هوشمند و تجمیعی گزارش می‌شود.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Hesab(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "نمودار حساب‌های کل در بخش گزارش‌های پیشرفته تحلیلی تراز آزمایشی قابل مشاهده است.").ShowDialog();
        }

        private void Image_PreviewMouseDown_TolidMah(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "نمودار میزان تولید هر محصول در ماه از طریق گزارش آمار تولید قابل استخراج و تحلیل است.").ShowDialog();
        }

        private void Image_PreviewMouseDown_TolidatMonth(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "نمودار میزان تولیدات به تفکیک ماه در گزارش‌های دوره‌ای برنامه‌ریزی و تولید ارائه می‌گردد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Fasli(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "نمودار میزان فروش به تفکیک ماه در داشبورد تحلیل نبض فروش در دسترس می‌باشد.").ShowDialog();
        }
    }
}
