using AUTO_BAZ.HelperWins;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class UserPersonelOrderWin : Window
    {
        #region Header Window
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            var ico = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    ico.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = ico;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    ico.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = ico;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<PersonelItem> Personnel { get; set; } = new();

        UniversControl universControl = new UniversControl();

        private Point _dragStartPoint;
        private DataGridRow? _draggedRow;
        private ScrollViewer? _scrollViewer;
        private DataGridRow? _lastDragOverRow;
        public class PersonelItem
        {
            public int IDD { get; set; }
            public string SAL_NAME { get; set; }
            public int SortOrder { get; set; }
        }

        public UserPersonelOrderWin()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void DGPersonnel_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = CL_LMethods.FindVisualChild<ScrollViewer>(DGPersonnel);
            //_scrollViewer = FindVisualChild<ScrollViewer>(DGPersonnel);
        }
        private void DGPersonnel_DragOver(object sender, DragEventArgs e)
        {
            if (_scrollViewer == null) return;
            Point pos = e.GetPosition(DGPersonnel);
            double threshold = 30;
            if (pos.Y < threshold)
                _scrollViewer.LineUp();
            else if (pos.Y > DGPersonnel.ActualHeight - threshold)
                _scrollViewer.LineDown();

            var row = CL_LMethods.FindParent<DataGridRow>((DependencyObject)e.OriginalSource);
            if (row != null && row != _lastDragOverRow)
            {
                if (_lastDragOverRow != null)
                    _lastDragOverRow.Background = Brushes.Transparent;
                AnimateDragOverRow(row);
                _lastDragOverRow = row;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }

        private void ReGetData()
        {
            string sql = @"
                SELECT sd.IDD, sd.SAL_NAME,
                       ISNULL(uo.SORT_ORDER, 0) AS SortOrder
                FROM SALA_DTL sd
                LEFT JOIN USER_PERSONEL_ORDER uo 
                     ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserID
                WHERE sd.ENABL = 0
                ORDER BY CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                         uo.SORT_ORDER, sd.SAL_NAME";

            var list = dbms.DoGetDataSQL<PersonelItem>(sql, new { UserID = Baseknow.USERCOD }).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].SortOrder = i + 1;
                list[i].SAL_NAME = CL_HESABDARI.DECODEUN(list[i].SAL_NAME);
            }
            Personnel = new ObservableCollection<PersonelItem>(list);
            DGPersonnel.ItemsSource = Personnel;
        }
        private void UpdateSortOrder()
        {
            for (int i = 0; i < Personnel.Count; i++)
                Personnel[i].SortOrder = i + 1;
            DGPersonnel.Items.Refresh();

            DGPersonnel.ScrollIntoView(DGPersonnel.SelectedItem);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int idx = DGPersonnel.SelectedIndex;
            if (idx > 0)
            {
                var item = Personnel[idx];
                Personnel.RemoveAt(idx);
                Personnel.Insert(idx - 1, item);
                DGPersonnel.SelectedIndex = idx - 1;
                UpdateSortOrder();
                AnimateRow(item);
            }
        }
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int idx = DGPersonnel.SelectedIndex;
            if (idx >= 0 && idx < Personnel.Count - 1)
            {
                var item = Personnel[idx];
                Personnel.RemoveAt(idx);
                Personnel.Insert(idx + 1, item);
                DGPersonnel.SelectedIndex = idx + 1;
                UpdateSortOrder();
                AnimateRow(item);
            }
        }
        private void DGPersonnel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedRow = CL_LMethods.FindParent<DataGridRow>((DependencyObject)e.OriginalSource); //FindAncestor
        }
        private void DGPersonnel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedRow != null)
            {
                Vector diff = _dragStartPoint - e.GetPosition(null);
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(_draggedRow, _draggedRow.Item, DragDropEffects.Move);
                }
            }
        }
        private void DGPersonnel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PersonelItem)))
            {
                var dropped = (PersonelItem)e.Data.GetData(typeof(PersonelItem))!;
                var target = ((FrameworkElement)e.OriginalSource).DataContext as PersonelItem;
                if (target != null && dropped != target)
                {
                    int removedIdx = Personnel.IndexOf(dropped);
                    int targetIdx = Personnel.IndexOf(target);
                    Personnel.RemoveAt(removedIdx);
                    Personnel.Insert(targetIdx, dropped);
                    DGPersonnel.SelectedItem = dropped;
                    UpdateSortOrder();
                    AnimateRow(dropped);
                }
            }
            _draggedRow = null;
            if (_lastDragOverRow != null)
            {
                _lastDragOverRow.Background = Brushes.Transparent;
                _lastDragOverRow = null;
            }
        }
  
        private void AnimateRow(PersonelItem item)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var row = DGPersonnel.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row == null) return;

                var brush = new SolidColorBrush(Colors.Transparent);
                row.Background = brush;

                var colorAnim = new ColorAnimation
                {
                    From = Colors.LightGreen,
                    To = Colors.Transparent,
                    Duration = TimeSpan.FromSeconds(0.6)
                };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                var transform = new TranslateTransform();
                row.RenderTransform = transform;
                var moveAnim = new DoubleAnimation
                {
                    From = -20,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.25),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                transform.BeginAnimation(TranslateTransform.XProperty, moveAnim);
            }), DispatcherPriority.Loaded);
        }
        private void AnimateDragOverRow(DataGridRow row)
        {
            var brush = new SolidColorBrush(Colors.Transparent);
            row.Background = brush;
            var colorAnim = new ColorAnimation
            {
                From = Colors.LightSkyBlue,
                To = Colors.Transparent,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            var scale = new ScaleTransform(1.0, 1.0);
            row.RenderTransform = scale;
            var scaleAnim = new DoubleAnimation
            {
                From = 1.05,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Msgwin msgwin = new Msgwin(true, "آیا از ریست ترتیب مطمئن هستید؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true) return;
            try
            {
                dbms.DoExecuteSQL("DELETE FROM USER_PERSONEL_ORDER WHERE USER_ID = @U", new { U = Baseknow.USERCOD });
                ReGetData();
                universControl.PopNotifyShowUp("." + "ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ریست ترتیب !").Show();
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dbms.DoExecuteSQL("DELETE FROM USER_PERSONEL_ORDER WHERE USER_ID = @U", new { U = Baseknow.USERCOD });
                int order = 1;
                foreach (var p in Personnel)
                {
                    dbms.DoExecuteSQL("INSERT INTO USER_PERSONEL_ORDER (USER_ID, PERSONEL_ID, SORT_ORDER) VALUES (@UID,@PID,@ORD)", new { UID = Baseknow.USERCOD, PID = p.IDD, ORD = order });
                    order++;
                }

                universControl.PopNotifyShowUp("." + "ذخیره انجام شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره !").Show();
            }
        }
    }
}
