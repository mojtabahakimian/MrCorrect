using AUTO_BAZ;
using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Wins.ThePages
{
    public partial class WIN_ATBZ_LOADER : Window
    {
        #region Header Window Begin
        //Header Window Begin

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

        AUTO_BAZ.MainWindow ABMW;
        private DispatcherTimer errorCheckTimer;
        public bool[] TheBLNS { get; }

        public string? MyProperty { get; set; }
        public WIN_ATBZ_LOADER(string Oprname, bool[] myBooleanArray)
        {
            TheBLNS = myBooleanArray;

            InitializeComponent();

            errorCheckTimer = new DispatcherTimer();
            errorCheckTimer.Interval = TimeSpan.FromSeconds(1);
            errorCheckTimer.Tick += ErrorCheckTimer_Tick;
            errorCheckTimer.Start();

            TheTextBlockStatusPressed.Text = Oprname;
        }
        private void ErrorCheckTimer_Tick(object sender, EventArgs e)
        {
            if (ABMW.AnyErrorHappend)
            {
                errorCheckTimer.Stop();
                new Msgwin(false, "عملیات به دلیل داشتن خطا متوقف شد! لطفا بررسی کنید").ShowDialog();
                Btn_Close_Click(null, null);
            }
            else if (ABMW.IsWorkisDone)
            {
                errorCheckTimer.Stop();
                new Msgwin(false, $"بازسازی {TheTextBlockStatusPressed.Text} با موفقیت انجام شد.").ShowDialog();
                Btn_Close_Click(null, null);
            }
        }
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            ABMW?.Close();
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

            //ABMW = new AUTO_BAZ.MainWindow(TheBLNS, true);
            ABMW = new AUTO_BAZ.MainWindow();
            ABMW.CHKITEMS = TheBLNS;
            ABMW.SingleCallerPart = true;
            ABMW.dbms = dbms;
            ABMW.Left = -1000;
            ABMW.Top = -1000;
            ABMW.Show();
            ABMW.LetsGoBtn_Click(null, null);
            ABMW.Hide();

            this.DataContext = ABMW.DataContext;
        }

        private void PB1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 100)
            {
            }
        }

        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
