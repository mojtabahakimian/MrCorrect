using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wins.WinMenus.HESABDARI
{
    public partial class WIN_JAMFACTPRS : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        public WIN_JAMFACTPRS(string _HES_)
        {
            InitializeComponent();

            HESAB = _HES_;

            this.DataContext = this;
        }

        private string HESAB { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<JAMFACTPRS> FACTOR_DATA { get; set; } = new ObservableCollection<JAMFACTPRS>();
        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FACTOR_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<JAMFACTPRS>($"SELECT * FROM dbo.JAMFACTPRS WHERE (CUST_NO = N'{HESAB}')").ToList();
            foreach (var item in MasterHead)
            {
                FACTOR_DATA.Add(item);
            }

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as JAMFACTPRS;

            }
        }

        #region SYNFUSION_DATA_GRID

        private readonly FilterService<JAMFACTPRS> filterService = new FilterService<JAMFACTPRS>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            //// Get the selected row and column index
            //var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null)
            //{
            //    var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
            //    UpdateCurrentCellValue(rowColumnIndex);
            //}
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);

            if (record == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                return;
            }
            var propertyInfo = record.GetType().GetProperty(mappingName);
            if (propertyInfo == null)
            {
                return;
            }
            var propertyValue = propertyInfo.GetValue(record);
            CurrentCellValue = propertyValue?.ToStringNullSafe() ?? string.Empty;
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e) // Event handler for the "Filter by Selection" context menu item click
        {
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
            if (filterValue != null)
            {
                // Add the filter to the filter service
                filterService.AddFilter(columnName, filterValue);
                // Add the filter to the list of active filters
                ActiveFilters.Add($"{columnName} = {filterValue}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e) // Event handler for the "Filter Excluding Selection" context menu item click
        {
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
            if (filterValue != null)
            {
                // Add the exclusion filter to the filter service
                filterService.AddFilter(columnName, $"!{filterValue}");
                // Add the filter to the list of active filters
                ActiveFilters.Add($"{columnName} != {filterValue}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
        }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e) // Event handler to remove all filters and sorting
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails() // Method to get the details of the selected cell
        {
            // Check if there is a current cell selected in the data grid
            if (SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                          // Return the column name and the current cell value
                return (columnName, CurrentCellValue);
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter() // Method to apply all cumulative filters to the data grid
        {
            // Set the filter for the data grid view using the filter service
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as JAMFACTPRS);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: JAMFACTPRS row })
            {
                if (row != null && row?.NUMBER != null && row?.TAG != null)
                {
                    var ROWVISI = dbms.DoGetDataSQL<mandfactormr_visit>($"SELECT TOP 1 NUMBER FROM dbo.mandfactormr_visit WHERE ((NUMBER = {row.NUMBER}) AND (TAG2 = {row.TAG}))").FirstOrDefault();
                    if (ROWVISI != null)
                    {
                        new WIN_MANDFACTORMR_VISIT((double)row.NUMBER, (double)row.TAG).ShowDialog();
                    }
                }
            }
        }

    }
}
