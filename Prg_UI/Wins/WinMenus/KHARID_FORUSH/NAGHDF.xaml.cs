using AUTO_BAZ.HelperWins;
using Prg_Proccessy.FUNCTIONS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH
{
    /// <summary>
    /// Interaction logic for NAGHDF.xaml
    /// </summary>
    public partial class NAGHDF : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        //Header Window End;
        #endregion
        public Visual THEWIN { get; set; }
        public NAGHDF(Visual _the_win)
        {
            THEWIN = _the_win;
            InitializeComponent();


            if (THEWIN is HEAD_LST_FROOSH22)
            {
                Text0.Text = (THEWIN as HEAD_LST_FROOSH22).M_NAGHD2.Text;
            }
        }
        private void OK_BTN_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text0.Text)) return;
            //HEAD_LST_FROOSH22
            if (THEWIN is HEAD_LST_FROOSH22)
            {
                (THEWIN as HEAD_LST_FROOSH22).M_NAGHD2.Text = Text0.Text;
                var HLFW = (THEWIN as HEAD_LST_FROOSH22);
                CL_HESABDARI.APLAYTAKH(Convert.ToInt64(HLFW.NUMBER.Text), 2, Convert.ToDouble(HLFW.M_NAGHD.Text), Convert.ToDouble(HLFW.MABL_VAR.Text), Convert.ToDouble(HLFW.MABL_HAV.Text), (bool)HLFW.TICMBAA.IsChecked);
                HLFW.BUTTON_SAVE_HAVALE_Click(null,null);
                this.Close();
              
            }
            //HEAD_LST_FROOSH2
            if (true)
            {

            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                OK_BTN_Click(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((THEWIN as HEAD_LST_FROOSH22).AllowEdits is true)
            {
            }
            else
            {
                new Msgwin(false, "كليد اصلاح را بزنيد تا فاكتور قابل اصلاح باشد").ShowDialog();
                Close();
            }
        }
    }
}
