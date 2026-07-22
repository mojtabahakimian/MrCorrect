using Functions;
using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prg_UI.HelperWins;

namespace Wins.ThePages
{
    public partial class PG_SANATI : Page
    {
        public PG_SANATI()
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
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_ENTER, default); //برگه ورود
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_EXIT, default); //برگه خروج
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALE_EXIT_SAYER, default); //صدور برگه خروج سایر مواد از انبار
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.AMAR_TOLID, default);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_DATE_AMMAS, default);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_HEAD_MANF_FORMULSAKHT, default);
        }

        private void Image_PreviewMouseDown_Personnel(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "ثبت کارکرد پرسنل تولید در این نسخه از طریق ماژول یکپارچه حقوق و دستمزد و کارکرد پرسنلی انجام می‌گیرد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Dastmozd(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "جذب دستمزد تولید و صدور سند مربوطه به صورت اتوماتیک در پایان دوره محاسباتی توسط سیستم انجام می‌شود.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Compare(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "مقایسه قیمت تمام‌شده و قیمت فروش در گزارشات پیشرفته سود و زیان ناویژه کالاها قابل دسترسی است.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Amalkard1(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "صدور سند عملکرد کالاها در پایان دوره (روش اول) به صورت هوشمند و خودکار در زمان بستن حساب‌ها صادر می‌گردد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Tadil(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "صدور سند تعدیلات قیمت تمام‌شده پس از ارزیابی نهایی کاردکس کالا توسط سیستم به صورت اتوماتیک صادر می‌شود.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Amalkard2(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "صدور سند عملکرد پایان دوره (روش دوم) به صورت یکپارچه در زمان اجرای عملیات پایان سال مالی صادر می‌گردد.").ShowDialog();
        }
    }
}
