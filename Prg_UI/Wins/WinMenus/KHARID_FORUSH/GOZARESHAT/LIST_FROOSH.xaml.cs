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
    /// Interaction logic for LIST_FROOSH.xaml
    /// </summary>
    public partial class LIST_FROOSH : Window
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
        public LIST_FROOSH()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public class LIST_FROOSH_MODEL
        {
            public long? DATE_N { get; set; }
            public long? code { get; set; }
            public string? kala { get; set; }
            public double? MEGH { get; set; }
            public double? MEGHk { get; set; }
            public string? CUSTNAME { get; set; }
            public string? hes { get; set; }
            public double? MABL { get; set; }
            public double? MABL_K { get; set; }
            public double? KHFR { get; set; }
            public double? GHFR { get; set; }
            public int? VAHCODE { get; set; }
            public double? GRPCODE { get; set; }
            public string? MOLAH { get; set; }
            public string? SHARAYET { get; set; }
            public double? FNUMCO { get; set; }
            public string? MANDAH { get; set; }
            public int? ANBARCODE { get; set; }
            public double? N_S { get; set; }
            public string? USER_NAME { get; set; }
            public double? MAS { get; set; }
            public double? NUMBER { get; set; }
            public double? NUMBER1 { get; set; }
            public string? BARGAH { get; set; }
            public int? TAGCODE { get; set; }
            public double? CAM_KHALY { get; set; }
            public double? CAM_POOR { get; set; }
            public double? MEGHkg { get; set; }
            public string? CAMIUN_NUM { get; set; }
            public string? CAMIUN { get; set; }
            public string? TOZIH { get; set; }
        }

        public class VQ
        {
            public int? CODE { get; set; }
            public string? NAMES { get; set; }
        }

        public ObservableCollection<LIST_FROOSH_MODEL> FROOSH_DATA_MODEL { get; set; } = new ObservableCollection<LIST_FROOSH_MODEL>();
        public bool NowIsReady { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FROOSH_DATA_MODEL?.Clear();

            VAHED_K.ItemsSource = dbms.DoGetDataSQL<VQ>("SELECT CODE, NAMES FROM TCOD_VAHEDS").ToList();
            VAHED_K.SelectedValuePath = "CODE";
            VAHED_K.DisplayMemberPath = "NAMES";

            var MasterHead = dbms.DoGetDataSQL<LIST_FROOSH_MODEL>(@$"SELECT TOP 100 PERCENT KALAS.DATE_N, KALAS.code, KALAS.kala, KALAS.MEGH, KALAS.MEGHk, KALAS.CUSTNAME, KALAS.hes, KALAS.MABL, KALAS.MABL_K, KALAS.KHFR, KALAS.GHFR, KALAS.VAHCODE, KALAS.GRPCODE, KALAS.MOLAH, KALAS.SHARAYET, KALAS.FNUMCO, KALAS.MANDAH, KALAS.ANBARCODE, KALAS.N_S, KALAS.USER_NAME, KALAS.MAS, KALAS.NUMBER, KALAS.NUMBER1, KALAS.BARGAH, KALAS.TAGCODE, OTHER_DTL_SUB.CAM_KHALY, OTHER_DTL_SUB.CAM_POOR, OTHER_DTL_SUB.MEGHk AS MEGHkg, OTHER_DTL_SUB.VAZNH, OTHER_DTL.CAMIUN_NUM, OTHER_DTL.CAMIUN, OTHER_DTL_SUB.TOZIH, OTHER_DTL_SUB.VAZNH - KALAS.MEGHk AS evazn, KALAS.ANBARAS, HEAD_LST.DATE_N AS DATE_R FROM KALAS INNER JOIN OTHER_DTL_SUB ON KALAS.code = OTHER_DTL_SUB.CODE AND KALAS.NUMBER1 = OTHER_DTL_SUB.NUMBER AND KALAS.TAG = OTHER_DTL_SUB.TAGG + 11 INNER JOIN OTHER_DTL ON KALAS.TAG = OTHER_DTL.TAG + 11 AND KALAS.NUMBER1 = OTHER_DTL.NUMBER INNER JOIN HEAD_LST ON OTHER_DTL.NUMBER = HEAD_LST.NUMBER AND OTHER_DTL.TAG = HEAD_LST.TAG WHERE (KALAS.TAGCODE = 12) ORDER BY KALAS.hes, KALAS.FNUMCO, KALAS.NUMBER").ToList();

            foreach (var item in MasterHead)
            {
                FROOSH_DATA_MODEL.Add(item);
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
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
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
