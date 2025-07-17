using Prg_UI.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.ThePages
{
    public partial class PG_KHARIDFORUSH : Page
    {
        public PG_KHARIDFORUSH()
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
            PageManagement.OpenPage(new PG_FACTORHA());
        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_GOZARESHAT_INVOCE());
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_VISITORY());
        }

        private void Image_PreviewMouseDown_4(object sender, MouseButtonEventArgs e)
        {
            PageManagement.OpenPage(new PG_GOZARESH_DARA());
        }
    }
}
