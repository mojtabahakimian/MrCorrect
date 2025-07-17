using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Wins.WinOther
{
    public partial class SearchWindow : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            MaterialDesignThemes.Wpf.PackIcon packIcon = new MaterialDesignThemes.Wpf.PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
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

        private DataGrid _dataGrid;
        private DataGridColumn _selectedColumn;

        public SearchWindow(DataGrid dataGrid)
        {
            InitializeComponent();
            _dataGrid = dataGrid;

            GetColumns();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dataGrid.CancelEdit();
            // Make sure DataGrid exits editing mode
            _dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            _dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            var collectionView = CollectionViewSource.GetDefaultView(_dataGrid.ItemsSource);
            if (collectionView is IEditableCollectionView editableCollectionView && editableCollectionView != null &&
                (editableCollectionView.IsAddingNew || editableCollectionView.IsEditingItem))
            {
                try
                {
                    editableCollectionView.CancelNew();
                    editableCollectionView.CancelEdit();
                }
                catch { }

            }

            SearchTextBox.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
            else
            {
                if (e.Key is Key.Escape)
                {
                    _dataGrid = null;
                    this.Close();
                }
            }
        }

        private void GetColumns()
        {
            ColumnComboBox.Items.Clear();
            foreach (var column in _dataGrid.Columns)
            {
                if (column.Visibility == Visibility.Visible)
                {
                    ColumnComboBox.Items.Add(column?.Header?.ToString());
                }
            }
            if (_dataGrid.CurrentColumn != null)
            {
                ColumnComboBox.SelectedItem = _dataGrid.CurrentColumn.Header?.ToString();
            }
        }

        private void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_selectedColumn != null)
            {
                var searchText = SearchTextBox.Text;
                ApplySearch(searchText);
            }
        }

        private void ApplySearch(string searchText)
        {
            if (_selectedColumn == null)
                return;



            // Normalize the search text by trimming whitespace and converting to lower case
            searchText = CL_LMethods.NormalizeArabicPersian(searchText).Trim();

            var collectionView = CollectionViewSource.GetDefaultView(_dataGrid.ItemsSource);
            collectionView.Filter = item =>
            {
                // Retrieve the text to search on based on column type
                string displayValue = GetDisplayValue(item, _selectedColumn);
                if (string.IsNullOrEmpty(displayValue))
                    return false;

                // Normalize and compare the display value
                displayValue = CL_LMethods.NormalizeArabicPersian(displayValue).Trim();
                return displayValue.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };

            collectionView.Refresh();
        }

        private string GetDisplayValue(object item, DataGridColumn column)
        {
            // If the column is a DataGridComboBoxColumn, perform a lookup for its display text.
            if (column is DataGridComboBoxColumn comboColumn)
            {
                // Get the binding path (should be from SelectedValueBinding or TextBinding)
                string bindingPath = GetBindingPath(column);
                if (string.IsNullOrEmpty(bindingPath))
                    return string.Empty;

                // Retrieve the property value from the data item.
                var property = item.GetType().GetProperty(bindingPath);
                if (property == null)
                    return string.Empty;
                var propertyValue = property.GetValue(item, null);
                if (propertyValue == null)
                    return string.Empty;

                // If a SelectedValuePath is specified, search the ItemsSource to match the property value.
                if (!string.IsNullOrEmpty(comboColumn.SelectedValuePath) && comboColumn.ItemsSource != null)
                {
                    foreach (var candidate in comboColumn.ItemsSource)
                    {
                        var candidateProperty = candidate.GetType().GetProperty(comboColumn.SelectedValuePath);
                        if (candidateProperty != null)
                        {
                            var candidateValue = candidateProperty.GetValue(candidate, null);
                            if (candidateValue != null && candidateValue.Equals(propertyValue))
                            {
                                // If a DisplayMemberPath is specified, use that property from the candidate.
                                if (!string.IsNullOrEmpty(comboColumn.DisplayMemberPath))
                                {
                                    var displayProperty = candidate.GetType().GetProperty(comboColumn.DisplayMemberPath);
                                    if (displayProperty != null)
                                    {
                                        var displayText = displayProperty.GetValue(candidate, null)?.ToString();
                                        return displayText ?? string.Empty;
                                    }
                                }
                                // Fallback to candidate's ToString() if no DisplayMemberPath is provided.
                                return candidate.ToString();
                            }
                        }
                    }
                }
                // If no match is found in ItemsSource, fallback to the property value's string.
                return propertyValue.ToString();
            }
            else
            {
                // For a regular DataGridBoundColumn, use the bound property value.
                string bindingPath = GetBindingPath(column);
                if (string.IsNullOrEmpty(bindingPath))
                    return string.Empty;
                var property = item.GetType().GetProperty(bindingPath);
                if (property == null)
                    return string.Empty;
                var propertyValue = property.GetValue(item, null);
                return propertyValue?.ToString() ?? string.Empty;
            }
        }


        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            ApplySearch(string.Empty);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColumnComboBox.SelectedItem != null)
            {
                // Get the selected column from the DataGrid
                string columnName = ColumnComboBox.SelectedItem.ToStringNullSafe();
                if (!string.IsNullOrEmpty(columnName))
                {
                    _selectedColumn = FindColumnByName(columnName);
                }
            }
        }

        private DataGridColumn FindColumnByName(string columnName)
        {
            foreach (var column in _dataGrid.Columns)
            {
                if (column.Visibility == Visibility.Visible && column != null && column?.Header != null)
                {
                    if (column.Header.ToString() == columnName)
                    {
                        return column;
                    }
                }
            }
            return null;
        }

        private string GetBindingPath(DataGridColumn column)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                return binding?.Path?.Path;
            }
            else if (column is DataGridComboBoxColumn comboBoxColumn)
            {
                var binding = comboBoxColumn.SelectedValueBinding as Binding ?? comboBoxColumn.TextBinding as Binding;
                return binding?.Path?.Path;
            }
            return null;
        }

        private void SearchTextBox_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchTextBox.SelectAll();
            }
        }
    }
}
