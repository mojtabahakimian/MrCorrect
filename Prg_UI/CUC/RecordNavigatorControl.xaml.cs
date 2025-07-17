using Functions;
using System.Windows;
using System.Windows.Controls;

namespace Prg_UI.CUC
{
    public partial class RecordNavigatorControl : UserControl
    {
        public RecordNavigatorControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NavigationManagerProperty =
            DependencyProperty.Register("NavigationManager", typeof(INavigationManager), typeof(RecordNavigatorControl),
                new PropertyMetadata(null, OnNavigationManagerChanged));

        public INavigationManager NavigationManager
        {
            get => (INavigationManager)GetValue(NavigationManagerProperty);
            set => SetValue(NavigationManagerProperty, value);
        }

        private static void OnNavigationManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RecordNavigatorControl control)
            {
                if (e.OldValue is INavigationManager oldManager)
                {
                    oldManager.CurrentRecordIndexChanged -= control.NavigationManager_CurrentRecordIndexChanged;
                    oldManager.RecordsCountChanged -= control.NavigationManager_RecordsCountChanged;
                }
                if (e.NewValue is INavigationManager newManager)
                {
                    newManager.CurrentRecordIndexChanged += control.NavigationManager_CurrentRecordIndexChanged;
                    newManager.RecordsCountChanged += control.NavigationManager_RecordsCountChanged;
                    control.UpdateNavigationDisplay();
                }
            }
        }

        private void NavigationManager_RecordsCountChanged(int count)
        {
            Dispatcher.Invoke(() => txtTotal.Text = count.ToString());
        }

        private void NavigationManager_CurrentRecordIndexChanged(int index)
        {
            Dispatcher.Invoke(() => txtCurrent.Text = (index + 1).ToString());
        }

        private void UpdateNavigationDisplay()
        {
            if (NavigationManager != null)
            {
                txtTotal.Text = NavigationManager.RecordsCount.ToString();
                txtCurrent.Text = (NavigationManager.CurrentRecordIndex + 1).ToString();
            }
            else
            {
                txtTotal.Text = "0";
                txtCurrent.Text = "0";
            }
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.MoveFirst();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.MoveBack();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.MoveNext();
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.MoveLast();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.NewRecord();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager?.ReloadData();
        }
    }
}