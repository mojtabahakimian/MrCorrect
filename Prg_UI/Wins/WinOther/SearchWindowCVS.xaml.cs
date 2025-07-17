using Interfaces;
using Prg_UI.Functions;
using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Wins.WinMenus.Taarif;

namespace Wins.WinOther
{
    public partial class SearchWindowCVS : Window
    {
        #region Window Header Handlers
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        #endregion

        private CollectionViewSource _recordsData;
        private string _selectedPropertyName;  // The property (column) to search on

        public Visual THEWIN { get; set; }
        public SearchWindowCVS(Visual _THEWIN_ , CollectionViewSource recordsData)
        {
            InitializeComponent();
            _recordsData = recordsData;
            THEWIN = _THEWIN_;
            PopulateColumns();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus on the ComboBox when the window loads.
            ColumnComboBox.Focus();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // (Optional) do any post-render logic here.
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Close window if the user presses Escape.
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }



        /// <summary>
        /// Populate the ComboBox with the names of the searchable properties
        /// of the items in the CollectionViewSource.
        /// </summary>
        private void PopulateColumns()
        {
            ColumnComboBox.Items.Clear();
            IEnumerable items = _recordsData.Source as IEnumerable;
            Type itemType = null;
            foreach (var item in items)
            {
                if (item != null)
                {
                    itemType = item.GetType();
                    break;
                }
            }

            if (itemType != null)
            {
                // Get public properties – you might filter this list if needed.
                PropertyInfo[] props = itemType.GetProperties();
                foreach (var prop in props)
                {
                    ColumnComboBox.Items.Add(prop.Name);
                }

                if (ColumnComboBox.Items.Count > 0)
                {
                    ColumnComboBox.SelectedIndex = 0;
                    _selectedPropertyName = ColumnComboBox?.SelectedItem?.ToString();
                }
            }
        }

        private void ColumnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColumnComboBox.SelectedItem != null)
            {
                _selectedPropertyName = ColumnComboBox?.SelectedItem?.ToString();
                // Reapply any current search with the new column.
                ApplySearch(SearchTextBox.Text);
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedPropertyName))
            {
                string searchText = SearchTextBox.Text;
                ApplySearch(searchText);
            }
        }

        /// <summary>
        /// Apply the search filter on the CollectionViewSource using the selected property.
        /// </summary>
        /// <param name="searchText">Text to search for.</param>
        private void ApplySearch(string searchText)
        {
            // Normalize the search text (e.g. trim and convert Arabic/Persian letters to a standard form)
            searchText = CL_LMethods.NormalizeArabicPersian(searchText).Trim();

            _recordsData.View.Filter = item =>
            {
                if (item == null) return false;

                // Use reflection to get the value of the selected property.
                PropertyInfo prop = item?.GetType()?.GetProperty(_selectedPropertyName);
                if (prop == null) return false;

                var value = prop.GetValue(item, null);
                if (value == null) return false;

                string propValue = value?.ToString();
                propValue = CL_LMethods.NormalizeArabicPersian(propValue).Trim();

                // Return true if the property value contains the search text (ignoring case).
                return propValue.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };

            _recordsData.View.Refresh();

            switch (THEWIN)
            {
                case FCODE_CUSTOMER:
                    (THEWIN as FCODE_CUSTOMER).MoveReGetData(INavigator.Jahat.LastItem);
                    break;

                default: break;
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            ApplySearch(string.Empty);
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
