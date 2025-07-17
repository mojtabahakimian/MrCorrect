using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_UI.Functions;
using Prg_UI.Wins.WinSetting;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET
{
    /// <summary>
    /// Interaction logic for BUDGET0.xaml
    /// </summary>
    public partial class BUDGET0 : Window
    {
        public BUDGET0()
        {
            InitializeComponent();
            //Get Persian Dates #Cool
            DateTime thisDate = DateTime.Now;
            PersianCalendar perscal = new PersianCalendar();
            string mah = (perscal.GetMonth(thisDate)).ToString();
            string rooz = (perscal.GetDayOfMonth(thisDate)).ToString();
            // string dtt = perscal.ToDateTime(thisDate.ToString
            //Make 2 digit if is one without zero at first of that
            if (mah.Length < 2)
            {
                mah = ("0" + mah).ToString();
            }
            if (rooz.Length < 2)
            {
                rooz = ("0" + rooz).ToString();
            }
            TextBM_Date_N.Text = (perscal.GetYear(thisDate) + "" + mah + "" + rooz).ToString();

            this.Owner = PublicVRB.WINBASE;
            //GC.SuppressFinalize(this);

        }


        #region Header Window Begin
        //Header Window Begin
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
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
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

            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion
        private void lbl_budget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);
        }
        private void Command4_Click(object sender, RoutedEventArgs e)
        {
            //was connected data check
            BUDGET1 bdgt1 = new BUDGET1();
            Close();
            bdgt1.Show();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            preloader.Visibility = Visibility.Hidden;
            //LoginUsers lgusers = new LoginUsers();
            //lgusers.ShowDialog();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cnnparamlbl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinConnectionChoose chos3cnn = new WinConnectionChoose();
            chos3cnn.ShowDialog();
        }
    }
}
