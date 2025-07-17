using System.Windows;
using System.Windows.Interop;

namespace UiTools
{
    public partial class HideAllButton : Window
    {
        public HideAllButton()
        {
            InitializeComponent();
            UpdatePosition();

            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }

        private void CloseAllButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationWindow.CloseAllNotifications();
            Close();
        }
        public void UpdatePosition()
        {
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - Width - 10;
            Top = workingArea.Bottom - Height - 10;
            //To make sure is on top of notifications
            Topmost = true;
        }

        //public void UpdatePosition()
        //{
        //    var workingArea = SystemParameters.WorkArea;
        //    Left = workingArea.Right - Width - 10;
        //    Top = workingArea.Top + 10;
        //    Topmost = false;
        //    Topmost = true;
        //}
    }
}
