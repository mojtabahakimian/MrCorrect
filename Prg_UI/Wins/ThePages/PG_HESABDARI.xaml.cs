using Prg_UI.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Wins.ThePages
{
    public partial class PG_HESABDARI : Page
    {
        public PG_HESABDARI()
        {
            InitializeComponent();

            // Page page = (Page)sender;
            DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.1));
            this.BeginAnimation(Page.OpacityProperty, fadeInAnimation);
        }

        private void BackerBtn_Click(object sender, RoutedEventArgs e)
        {
            KeyEventArgs backKeyEvent = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Escape);
            backKeyEvent.RoutedEvent = Keyboard.KeyDownEvent;
            PageManagement.DisplayPageManagement(backKeyEvent, PageManagement.TheMainFame);
        }

        private void Hesabdari_Btn_FSUB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_HESABDARI_SUB());
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_SALARY());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_CHECKHA());
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_TAARIF());
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_KHARIDFORUSH());
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_ANBAR());
        }
    }
}
