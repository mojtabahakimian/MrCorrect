using Prg_UI.Functions;
using System.Windows;

namespace Prg_UI.MySplashScreen
{
    public partial class WinSplashy : Window, ISplashScreen
    {
        public WinSplashy()
        {
            InitializeComponent();

            bool isRdp = true; // GeneralOptionManager.IsRDPMode

            if (isRdp)
            {
                // حالت سبک برای Remote Desktop
                NormalContent.Visibility = Visibility.Collapsed;
                Circle1.Visibility = Visibility.Collapsed;
                Circle2.Visibility = Visibility.Collapsed;

                if (myg != null)
                {
                    myg.Visibility = Visibility.Collapsed;
                    myg.Source = null;
                    myg = null;
                }

                RDP_LABEL.Visibility = Visibility.Visible;

                // حذف سایه برای عملکرد بهتر روی RDP
                MainBorder.Effect = null;
            }
        }

        public void LoadComplete()
        {
            Dispatcher.Invoke(() =>
            {
                if (myg != null)
                    myg.Source = null;
            });

            Dispatcher.InvokeShutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (myg != null)
            {
                myg.Visibility = Visibility.Visible;
            }
        }
    }

    public interface ISplashScreen
    {
        void LoadComplete();
    }
}