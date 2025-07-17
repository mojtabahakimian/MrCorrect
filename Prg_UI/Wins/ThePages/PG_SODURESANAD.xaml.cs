using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_SODURESANAD : Page
    {
        // x:Name="C0" , Content="باز سازی نرخ میانگین"
        // x:Name="c1" , Content="سند فروش"
        // x:Name="c2" , Content="سند خرید"
        // x:Name="C00" , Content="باز سازی موجودی انبار"
        // x:Name="c3" , Content="سند خزانه"
        // x:Name="c4" , Content="سند انتقالی"
        // x:Name="c5" , Content="سند خروج مواد"
        // x:Name="c6" , Content="سند خروج سایر"
        // x:Name="c7" , Content="سند تولید ورود"
        // x:Name="c8" , Content="سند برگشت فروش"
        // x:Name="c9" , Content="سند خدمات"
        // x:Name="c10" , Content="سند انبار گردانی"
        // x:Name="c11" , Content="سند وصولی اسناد دریافتنی"
        //--------------------------0-------1------2------3------4------5------6------7------8------9------10-----11
        //bool[] myBooleanArray = { true, false, false, false, false, false, false, false, false, false, false, false };
        public PG_SODURESANAD()
        {
            InitializeComponent();
        }
        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;

            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Image_PreviewMouseLeftButtonDown_10(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true,"آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            //بازسازی نرخ میانگین انبار
            //c0
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { true, false, false, false, false, false, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("بازسازی نرخ میانگین انبار", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            //سند فاکتور های فروش
            //c1

            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, true, false, false, false, false, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("سند فاکتور های فروش", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_8(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند فاکتور های خرید
            //c2
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, true, false, false, false, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("سند فاکتور های خرید", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_4(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند های خزانه داری
            //c3
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, false, true, false, false, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("سند های خزانه داری", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_6(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            //انتقال کالا از انبار به انبار
            //c4
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, false, false, true, false, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("انتقال کالا از انبار به انبار", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            //سند گروهی خروج مواد اولیه
            //c5
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, false, false, false, true, false, false, false, false, false, false };
            new WIN_ATBZ_LOADER("سند گروهی خروج مواد اولیه", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند برگه های خروج سایر مواد
            //c6
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, false, false, false, false, true, false, false, false, false, false };
            new WIN_ATBZ_LOADER("سند برگه های خروج سایر مواد", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند برگه های ورود کالای ساخته شده
            //c7
            //--------------0-------1------2------3------4------5------6------7------8------9------10-----11
            bool[] BLN = { false, false, false, false, false, false, false, true, false, false, false, false };
            new WIN_ATBZ_LOADER("سند برگه های ورود کالای ساخته شده", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_11(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // اسناد برگشت فروش
            //c8
            //--------------0-------1------2------3------4------5------6-----7------8------9------10-----11
            bool[] BLN = { false, false, false, false, false, false, false, false, true, false, false, false };
            new WIN_ATBZ_LOADER("اسناد برگشت فروش", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_5(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند فاکتور های خدمات
            //c9
            //--------------0-------1------2------3------4------5------6------7------8-----9------10-----11
            bool[] BLN = { false, false, false, false, false, false, false, false, false, true, false, false };
            new WIN_ATBZ_LOADER("سند فاکتور های خدمات", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_9(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // سند انبار گردانی
            //c10
            //--------------0-------1------2------3------4------5------6------7------8------9-----10-----11
            bool[] BLN = { false, false, false, false, false, false, false, false, false, false, true, false };
            new WIN_ATBZ_LOADER("سند انبار گردانی", BLN).Show();
        }
        private void Image_PreviewMouseLeftButtonDown_7(object sender, MouseButtonEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            // برگه های وصولی اسناد دریافتنی
            //c11
            //--------------0-------1------2------3------4------5------6------7------8------9------10----11
            bool[] BLN = { false, false, false, false, false, false, false, false, false, false, false, true };
            new WIN_ATBZ_LOADER("برگه های وصولی اسناد دریافتنی", BLN).Show();
        }
    }
}
