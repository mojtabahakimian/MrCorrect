using Functions;
using Prg_UI.Functions;
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

            RemoteModeTB.IsChecked = Convert.ToBoolean(Prg_UI.Properties.Settings.Default.IsRDPMode);
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
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SAZMAN, null);
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

        private void RemoteModeTB_Click(object sender, RoutedEventArgs e)
        {
            Prg_UI.Properties.Settings.Default.IsRDPMode = Convert.ToBoolean(RemoteModeTB.IsChecked);
            Prg_UI.Properties.Settings.Default.Save();
        }
    }
}
