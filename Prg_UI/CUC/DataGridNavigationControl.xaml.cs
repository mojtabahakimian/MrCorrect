using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Prg_UI.CUC
{
    public partial class DataGridNavigationControl : UserControl
    {
        private bool _isNavigating = false;
        private bool _isNewRow = false;

        #region Dependency Properties

        public static readonly DependencyProperty TargetDataGridProperty =
            DependencyProperty.Register("TargetDataGrid", typeof(DataGrid), typeof(DataGridNavigationControl),
                new PropertyMetadata(null, OnTargetDataGridChanged));

        public static readonly DependencyProperty DataLoadFunctionProperty =
            DependencyProperty.Register("DataLoadFunction", typeof(Func<Task<IEnumerable>>), typeof(DataGridNavigationControl));

        public static readonly DependencyProperty NewRecordActionProperty =
            DependencyProperty.Register("NewRecordAction", typeof(Action), typeof(DataGridNavigationControl));

        public static readonly DependencyProperty SelectionChangedCallbackProperty =
            DependencyProperty.Register("SelectionChangedCallback", typeof(Action<object>), typeof(DataGridNavigationControl));

        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(DataGridNavigationControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(63, 81, 181))));

        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.Register("ButtonForeground", typeof(Brush), typeof(DataGridNavigationControl),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public static readonly DependencyProperty ReloadButtonVisibilityProperty =
         DependencyProperty.Register("ReloadButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
             new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ReloadButtonEnabledProperty =
            DependencyProperty.Register("ReloadButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty FirstButtonVisibilityProperty =
            DependencyProperty.Register("FirstButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty FirstButtonEnabledProperty =
            DependencyProperty.Register("FirstButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty LastButtonVisibilityProperty =
            DependencyProperty.Register("LastButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty LastButtonEnabledProperty =
            DependencyProperty.Register("LastButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty NextButtonVisibilityProperty =
            DependencyProperty.Register("NextButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty NextButtonEnabledProperty =
            DependencyProperty.Register("NextButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty BackButtonVisibilityProperty =
            DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty BackButtonEnabledProperty =
            DependencyProperty.Register("BackButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty NewButtonVisibilityProperty =
            DependencyProperty.Register("NewButtonVisibility", typeof(Visibility), typeof(DataGridNavigationControl),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty NewButtonEnabledProperty =
            DependencyProperty.Register("NewButtonEnabled", typeof(bool), typeof(DataGridNavigationControl),
                new PropertyMetadata(true));



        #endregion

        #region Properties

        public Visibility ReloadButtonVisibility
        {
            get { return (Visibility)GetValue(ReloadButtonVisibilityProperty); }
            set { SetValue(ReloadButtonVisibilityProperty, value); }
        }

        public bool ReloadButtonEnabled
        {
            get { return (bool)GetValue(ReloadButtonEnabledProperty); }
            set { SetValue(ReloadButtonEnabledProperty, value); }
        }

        public Visibility FirstButtonVisibility
        {
            get { return (Visibility)GetValue(FirstButtonVisibilityProperty); }
            set { SetValue(FirstButtonVisibilityProperty, value); }
        }

        public bool FirstButtonEnabled
        {
            get { return (bool)GetValue(FirstButtonEnabledProperty); }
            set { SetValue(FirstButtonEnabledProperty, value); }
        }

        public Visibility LastButtonVisibility
        {
            get { return (Visibility)GetValue(LastButtonVisibilityProperty); }
            set { SetValue(LastButtonVisibilityProperty, value); }
        }

        public bool LastButtonEnabled
        {
            get { return (bool)GetValue(LastButtonEnabledProperty); }
            set { SetValue(LastButtonEnabledProperty, value); }
        }

        public Visibility NextButtonVisibility
        {
            get { return (Visibility)GetValue(NextButtonVisibilityProperty); }
            set { SetValue(NextButtonVisibilityProperty, value); }
        }

        public bool NextButtonEnabled
        {
            get { return (bool)GetValue(NextButtonEnabledProperty); }
            set { SetValue(NextButtonEnabledProperty, value); }
        }

        public Visibility BackButtonVisibility
        {
            get { return (Visibility)GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        public bool BackButtonEnabled
        {
            get { return (bool)GetValue(BackButtonEnabledProperty); }
            set { SetValue(BackButtonEnabledProperty, value); }
        }

        public Visibility NewButtonVisibility
        {
            get { return (Visibility)GetValue(NewButtonVisibilityProperty); }
            set { SetValue(NewButtonVisibilityProperty, value); }
        }

        public bool NewButtonEnabled
        {
            get { return (bool)GetValue(NewButtonEnabledProperty); }
            set { SetValue(NewButtonEnabledProperty, value); }
        }



        /// <summary>
        /// The DataGrid to navigate
        /// </summary>
        public DataGrid TargetDataGrid
        {
            get { return (DataGrid)GetValue(TargetDataGridProperty); }
            set { SetValue(TargetDataGridProperty, value); }
        }

        /// <summary>
        /// Function to load/reload data for the DataGrid
        /// </summary>
        public Func<Task<IEnumerable>> DataLoadFunction
        {
            get { return (Func<Task<IEnumerable>>)GetValue(DataLoadFunctionProperty); }
            set { SetValue(DataLoadFunctionProperty, value); }
        }

        /// <summary>
        /// Action to execute when creating a new record
        /// </summary>
        public Action NewRecordAction
        {
            get { return (Action)GetValue(NewRecordActionProperty); }
            set { SetValue(NewRecordActionProperty, value); }
        }

        /// <summary>
        /// Callback when selection changes
        /// </summary>
        public Action<object> SelectionChangedCallback
        {
            get { return (Action<object>)GetValue(SelectionChangedCallbackProperty); }
            set { SetValue(SelectionChangedCallbackProperty, value); }
        }

        /// <summary>
        /// Background color for navigation buttons
        /// </summary>
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }

        /// <summary>
        /// Foreground color for navigation buttons
        /// </summary>
        public Brush ButtonForeground
        {
            get { return (Brush)GetValue(ButtonForegroundProperty); }
            set { SetValue(ButtonForegroundProperty, value); }
        }

        public Action ReGetDataAction { get; set; }

        #endregion

        public DataGridNavigationControl()
        {
            InitializeComponent();
        }

        private static void OnTargetDataGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DataGridNavigationControl)d;
            var oldDataGrid = e.OldValue as DataGrid;
            var newDataGrid = e.NewValue as DataGrid;

            // Unsubscribe from old DataGrid events
            if (oldDataGrid != null)
            {
                oldDataGrid.SelectionChanged -= control.DataGrid_SelectionChanged;
            }

            // Subscribe to new DataGrid events
            if (newDataGrid != null)
            {
                newDataGrid.SelectionChanged += control.DataGrid_SelectionChanged;
                control.UpdateNavigationDisplay();
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateNavigationDisplay();

            if (!_isNavigating && SelectionChangedCallback != null && TargetDataGrid?.SelectedItem != null)
            {
                SelectionChangedCallback(TargetDataGrid.SelectedItem);
            }
        }

        #region Navigation Methods

        /// <summary>
        /// Updates the navigation display (current index and total count)
        /// </summary>
        private void UpdateNavigationDisplay()
        {
            try
            {
                if (TargetDataGrid == null)
                {
                    txtCurrent.Text = "0";
                    txtTotal.Text = "0";
                    return;
                }

                // Get the actual count of items (excluding the new item placeholder if present)
                int totalCount = GetActualItemCount();

                // Update the total count display
                txtTotal.Text = totalCount.ToString();

                // Update the current index display (1-based instead of 0-based)
                int currentIndex = TargetDataGrid.SelectedIndex;
                if (currentIndex >= 0 && currentIndex < totalCount)
                {
                    txtCurrent.Text = (currentIndex + 1).ToString();
                }
                else if (_isNewRow)
                {
                    // If we're in a new row, show appropriate index
                    txtCurrent.Text = (totalCount + 1).ToString();
                }
                else
                {
                    txtCurrent.Text = totalCount > 0 ? "1" : "0";
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't disturb the user
            }
        }

        /// <summary>
        /// Gets the actual count of items in the DataGrid, excluding potential placeholders
        /// </summary>
        /// <summary>
        /// Gets the actual count of items in the DataGrid, excluding potential placeholders
        /// </summary>
        private int GetActualItemCount()
        {
            try
            {
                if (TargetDataGrid == null) return 0;

                // Get the raw count of items
                int itemsCount = TargetDataGrid.Items.Count;

                // Check if the DataGrid's ItemsSource is a collection
                if (TargetDataGrid.ItemsSource is IList itemsList && itemsList.Count > 0)
                {
                    // Check if the last item appears to be a placeholder
                    var lastItem = itemsList[itemsList.Count - 1];
                    if (IsNewItemPlaceholder(lastItem))
                    {
                        return itemsCount - 1;
                    }
                }

                return itemsCount;
            }
            catch
            {
                // Fallback to raw count if any error occurs
                return TargetDataGrid?.Items.Count ?? 0;
            }
        }


        /// <summary>
        /// Checks if an item is a new item placeholder
        /// </summary>
        private bool IsNewItemPlaceholder(object item)
        {
            // This implementation depends on how the data is structured
            // For common scenarios, new item placeholders are often:
            // 1. Default instances of the bound type
            // 2. Items with special flags or all properties null/default

            if (item == null) return true;

            // Example check: If the item is a dictionary or has properties, check if they're all default values
            var properties = item.GetType().GetProperties();
            if (properties.Length > 0)
            {
                bool allDefaultOrNull = true;
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    if (value != null && !value.Equals(GetDefaultValue(prop.PropertyType)))
                    {
                        allDefaultOrNull = false;
                        break;
                    }
                }
                return allDefaultOrNull;
            }

            return false;
        }

        /// <summary>
        /// Gets the default value for a given type
        /// </summary>
        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        #endregion

        #region Button Click Handlers

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            GoFirst();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            GoPrevious();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            GoNext();
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            GoLast();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            GoNew();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadData(() =>
            {
                if (ReGetDataAction != null)
                {
                    ReGetDataAction.Invoke();
                }
            });
        }

        #endregion

        #region Public Navigation Methods

        /// <summary>
        /// Navigates to the first record in the DataGrid
        /// </summary>
        public void GoFirst()
        {
            try
            {
                if (TargetDataGrid == null) return;

                _isNavigating = true;
                _isNewRow = false;

                if (TargetDataGrid.Items.Count > 0)
                {
                    TargetDataGrid.SelectedIndex = 0;
                    TargetDataGrid.ScrollIntoView(TargetDataGrid.SelectedItem);

                    if (SelectionChangedCallback != null)
                    {
                        SelectionChangedCallback(TargetDataGrid.SelectedItem);
                    }
                }
                else
                {
                    //MessageBox.Show("No records available.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateNavigationDisplay();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error navigating to first record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Navigates to the last record in the DataGrid
        /// </summary>
        public void GoLast()
        {
            try
            {
                if (TargetDataGrid == null) return;

                _isNavigating = true;
                _isNewRow = false;

                if (TargetDataGrid.Items.Count > 0)
                {
                    int lastIndex = GetActualItemCount() - 1;
                    if (lastIndex >= 0)
                    {
                        TargetDataGrid.SelectedIndex = lastIndex;
                        TargetDataGrid.ScrollIntoView(TargetDataGrid.SelectedItem);

                        if (SelectionChangedCallback != null)
                        {
                            SelectionChangedCallback(TargetDataGrid.SelectedItem);
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("No records available.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateNavigationDisplay();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error navigating to last record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Navigates to the next record in the DataGrid
        /// </summary>
        public void GoNext()
        {
            try
            {
                if (TargetDataGrid == null) return;

                _isNavigating = true;
                _isNewRow = false;

                if (TargetDataGrid.Items.Count > 0)
                {
                    int currentIndex = TargetDataGrid.SelectedIndex;
                    int lastIndex = GetActualItemCount() - 1;

                    if (currentIndex < lastIndex)
                    {
                        TargetDataGrid.SelectedIndex = currentIndex + 1;
                        TargetDataGrid.ScrollIntoView(TargetDataGrid.SelectedItem);

                        if (SelectionChangedCallback != null)
                        {
                            SelectionChangedCallback(TargetDataGrid.SelectedItem);
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Already at the last record.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    //MessageBox.Show("No records available.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateNavigationDisplay();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error navigating to next record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Navigates to the previous record in the DataGrid
        /// </summary>
        public void GoPrevious()
        {
            try
            {
                if (TargetDataGrid == null) return;

                _isNavigating = true;
                _isNewRow = false;

                if (TargetDataGrid.Items.Count > 0)
                {
                    int currentIndex = TargetDataGrid.SelectedIndex;
                    if (currentIndex > 0)
                    {
                        TargetDataGrid.SelectedIndex = currentIndex - 1;
                        TargetDataGrid.ScrollIntoView(TargetDataGrid.SelectedItem);

                        if (SelectionChangedCallback != null)
                        {
                            SelectionChangedCallback(TargetDataGrid.SelectedItem);
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Already at the first record.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    //MessageBox.Show("No records available.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateNavigationDisplay();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error navigating to previous record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Creates a new record placeholder
        /// </summary>
        public void GoNew()
        {
            try
            {
                _isNewRow = true;

                if (NewRecordAction != null)
                {
                    NewRecordAction();
                    UpdateNavigationDisplay();
                }
                else
                {
                    //MessageBox.Show("New record functionality not implemented.", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error creating new record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GoNewPlaceholder(DataGrid dataGrid, DataGridColumn column = null)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (!dataGrid.CanUserAddRows)
                return;

            if (!dataGrid.IsEnabled || dataGrid.IsReadOnly || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            dataGrid.ScrollIntoView(CollectionView.NewItemPlaceholder);
            dataGrid.UpdateLayout();

            if (column == null && dataGrid.Columns.Count > 0)
            {
                column = dataGrid.Columns[0];
            }

            if (column != null)
            {
                dataGrid.CurrentCell = new DataGridCellInfo(CollectionView.NewItemPlaceholder, column);
                dataGrid.BeginEdit();
            }
        }

        /// <summary>
        /// Reloads the data in the target DataGrid using a user-provided reload action
        /// </summary>
        /// <param name="reloadAction">Custom action that handles the data reload logic</param>
        public void ReloadData(Action reloadAction)
        {
            // Execute the user-provided reload logic
            reloadAction?.Invoke();

            // After reload, reset the current position to the first record if available
            if (TargetDataGrid != null && TargetDataGrid.Items.Count > 0)
            {
                //TargetDataGrid.SelectedIndex = 0;
                //TargetDataGrid.ScrollIntoView(TargetDataGrid.SelectedItem);

                GoLast();
            }

            // Update navigation info after reload
            UpdateNavigationDisplay();
        }

        #endregion
    }
}
