using Functions;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_MYSETTINGS : Page
    {

        public PG_MYSETTINGS()
        {
            InitializeComponent();

            // بارگذاری مقدار اولیه به صورت async
            Loaded += async (s, e) =>
            {
                RemoteModeTB.IsChecked = await GeneralOptionManager.GetIsRDPModeAsync();
            };
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SAVABEGH());
        }

        private void Image_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEFAULT, null);
        }

        private void WrapPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SAZMAN_MOSHAKHASAT, null);
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //تعیین سطح دسترسی
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_USER_PERMITION_FORMS_DASTRASI, null);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.MaterialThemSettingy, null);
        }

        //private void RemoteModeTB_Click(object sender, RoutedEventArgs e)
        //{
        //    _optionManager.IsRDPMode = Convert.ToBoolean(RemoteModeTB.IsChecked);
        //}
        private async void RemoteModeTB_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = Convert.ToBoolean(RemoteModeTB.IsChecked);
            // ذخیره در دیتابیس به صورت async
            bool success = await GeneralOptionManager.SetIsRDPModeAsync(newValue);
            if (!success)
            {
                // اگر ذخیره نشد، تیک را به حالت قبلی برگردان
                RemoteModeTB.IsChecked = !newValue;
                new Msgwin(false, "خطا در ذخیره تنظیمات!").Show();
            }
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.NEWPASSWORD, null);
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.USER_CHANGE, null);
        }

        private void WrapPanel_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_About, default);
        }

        private void Image_PreviewMouseDown_5(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.USERS, default);

        }

        private void Image_PreviewMouseDown_ShiftDefault(object sender, MouseButtonEventArgs e)
        {
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEFAULT, null);
        }

        private void Image_PreviewMouseDown_Send(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "ارسال اطلاعات و همگام‌سازی داده‌های شعب به صورت مداوم، زنده و کاملاً هوشمند در پس‌زمینه سیستم انجام می‌گردد.").ShowDialog();
        }

        private void Image_PreviewMouseDown_Receive(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "دریافت اطلاعات و به‌روزرسانی شعب به صورت برخط و خودکار توسط هسته مرکزی پایگاه داده همگام‌سازی می‌شود.").ShowDialog();
        }

        private void Image_PreviewMouseDown_UserGroups(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "تنظیمات گروه‌های کاربری و دسترسی‌های تیمی در بخش مدیریت کاربران و تعیین سطح دسترسی قابل پیاده‌سازی و تغییر است.").ShowDialog();
        }

        private void Image_PreviewMouseDown_BlackList(object sender, MouseButtonEventArgs e)
        {
            new Msgwin(false, "فهرست سیاه مشتریان بدحساب از طریق فعال‌سازی گزینه وضعیت مسدودیت حساب در مشخصات مشتریان کنترل می‌گردد.").ShowDialog();
        }
    }
}
