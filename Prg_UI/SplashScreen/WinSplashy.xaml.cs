using Prg_UI.Functions;
using System.Windows;

namespace Prg_UI.SplashScreen
{
    public partial class WinSplashy : Window, ISplashScreen
    {
        public WinSplashy()
        {
            InitializeComponent();

            //<Image x:Name="myg" Margin="-9,8,4,5" Visibility="Hidden" gifplayer:AnimationBehavior.RepeatBehavior="2x" gifplayer:AnimationBehavior.AutoStart="True" gifplayer:AnimationBehavior.SourceUri="/UiDrive/IMGS/SplashHandwrite.gif" Stretch="UniformToFill"/>
            if (true) //GeneralOptionManager.IsRDPMode
            {
                myg.Visibility = Visibility.Collapsed;
                myg = null;
                RDP_LABEL.Visibility = Visibility.Visible;
            }
        }
        public void LoadComplete()
        {
            Dispatcher.Invoke(() => { if (myg is not null) myg.Source = null; });

            Dispatcher.InvokeShutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (myg is not null)
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
