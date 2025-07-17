using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for CUSTOMER_DIDNOT_PURCHASE.xaml
    /// </summary>
    public partial class CUSTOMER_DIDNOT_PURCHASE : Window
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

        public CUSTOMER_DIDNOT_PURCHASE()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public class CUSTOMER_MODEL
        {
            public string? CUST_NO { get; set; }
            public string? hes { get; set; }
            public string? NAME { get; set; }
            public string? ADDRESS { get; set; }
            public string? TEL { get; set; }
            public string? CODE_E { get; set; }
            public string? ECODE { get; set; }
            public string? PCODE { get; set; }
            public string? IYALAT { get; set; }
            public string? CITY { get; set; }
            public string? MCODEM { get; set; }
            public string? TOZIH { get; set; }
            public int? CUST_COD { get; set; }
            public string? MOBILE { get; set; }
            public double? Longitude { get; set; }
            public double? Latitude { get; set; }
            public string? ROUTE_NAME { get; set; }
            public int? OSTANID { get; set; }
            public int? SHAHRID { get; set; }
            public int? tob { get; set; }
        }

        public ObservableCollection<CUSTOMER_MODEL> CUSTOMER_DATA_MODEL { get; set; } = new ObservableCollection<CUSTOMER_MODEL>();
        public bool NowIsReady { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CUSTOMER_DATA_MODEL?.Clear();

            var MasterHead = dbms.DoGetDataSQL<CUSTOMER_MODEL>(@$" SELECT customers_whodid_purchase.CUST_NO, CUST_HESAB.* FROM CUST_HESAB LEFT OUTER JOIN customers_whodid_purchase(11111111, 99991230) customers_whodid_purchase ON CUST_HESAB.hes = customers_whodid_purchase.CUST_NO WHERE (CUST_HESAB.hes LIKE N'115-%') AND (customers_whodid_purchase.CUST_NO IS NULL)").ToList();

            foreach (var item in MasterHead)
            {
                CUSTOMER_DATA_MODEL.Add(item);
            }

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            //{
            //    e.Handled = true;

            //    var currentRow = SYNCFUSION_DG.SelectedItem as BASKOOL_MODEL;

            //    if (currentRow?.NUMBER != null)
            //    {
            //        OpenWindow(typeof(ANBGRD_HEAD_WIN), (double)currentRow.NUMBER, "یک پنجره انبار گردانی از قبل باز شده ابتدا آنرا ببندید.");
            //    }

            //}
        }
        public void OpenWindow(Type windowType, object parameter, string errorMessage)
        {
            if (windowType == null || !typeof(Window).IsAssignableFrom(windowType))
                return;

            if (!CL_LMethods.IsWindowOpen(windowType)) //CL_LMethods.IsWindowOpen<HEAD_LST_FROOSH22>()
            {
                var constructor = windowType.GetConstructor(new[] { parameter.GetType() });
                if (constructor != null)
                {
                    var window = (Window)constructor.Invoke(new[] { parameter });
                    window.Show();
                }
            }
            else
            {
                new Msgwin(false, errorMessage).ShowDialog();
            }
        }

        private readonly FilterService<HEAD_LST> filterService = new FilterService<HEAD_LST>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;

        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
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

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName)?.GetValue(record)?.ToString();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as HEAD_LST);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }
    }
}
