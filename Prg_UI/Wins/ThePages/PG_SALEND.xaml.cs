using Functions;
using Prg_UI.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prg_UI.Wins.ThePages
{
    /// <summary>
    /// Interaction logic for PG_SALEND.xaml
    /// </summary>
    public partial class PG_SALEND : Page
    {
        public PG_SALEND()
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
            //اختتامیه
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SANAD_EKHTETAMIYAH, default);

        }

        private void Image_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //سال مالی جدید
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_F_NEWYEAR_SALEMALI, default);
        }

        private void Image_PreviewMouseDown_2(object sender, MouseButtonEventArgs e)
        {
            //افتتاحیه
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_SANAD_EFTETAHIYAH, default);
        }

        private void Image_PreviewMouseDown_3(object sender, MouseButtonEventArgs e)
        {
            //دریافت موجودی انتهای دوره از سال قبل
            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_GETFIRSTMOG_DARYAFTMOG, default);
        }
    }
}
