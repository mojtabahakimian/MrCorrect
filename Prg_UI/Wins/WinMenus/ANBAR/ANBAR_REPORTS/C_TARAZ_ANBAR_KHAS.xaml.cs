using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    /// <summary>
    /// Interaction logic for C_TARAZ_ANBAR_KHAS.xaml
    /// </summary>
    public partial class C_TARAZ_ANBAR_KHAS : Window
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
        public C_TARAZ_ANBAR_KHAS(string _SQL_DATA_)
        {
            SQL_DATA = _SQL_DATA_;

            InitializeComponent();

            this.DataContext = this;
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<MDS> C_TARAZ_KHAS_DATA { get; set; } = new ObservableCollection<MDS>();

        public System.Windows.Media.Brush AlternatingRowBackground { get; set; }
        public bool NowIsReady { get; private set; }

        public string SQL_DATA { get; private set; }
        public class MDS
        {
            public double? FII_AFZAYESH { get; set; }
            public double? FII_KAHESH { get; set; }
            public double? FII_MOGUDI { get; set; }
            public double? FIRST_FII { get; set; }
            public double? ANBAR { get; set; }
            public string? ANBNAM { get; set; }
            public string? CODE { get; set; }
            public string? KALA { get; set; }
            public int? VAHED { get; set; }
            public string? VAHNAM { get; set; }
            public double? RADAH { get; set; }
            public string? grname { get; set; }
            public double? MEGHAVM { get; set; }
            public double? MABAVM { get; set; }
            public double? MOG { get; set; }
            public double? MABLM { get; set; }
            public double? MEGHVARED { get; set; }
            public double? MABVARED { get; set; }
            public double? MEGHSADER { get; set; }
            public double? MABSADER { get; set; }
            public double? MEGHKHM { get; set; }
            public double? MABKHM { get; set; }
            public double? MEGHFRM { get; set; }
            public double? MABFRM { get; set; }
            public double? MEGHENKRM { get; set; }
            public double? MABENKRM { get; set; }
            public double? MEGHENVOM { get; set; }
            public double? MABENVOM { get; set; }
            public double? MEGHTOM { get; set; }
            public double? MABTOM { get; set; }
            public double? MEGHEXM { get; set; }
            public double? MABEXM { get; set; }
            public double? MEGHEXSM { get; set; }
            public double? MABEXSM { get; set; }
            public double? MEGHKASM { get; set; }
            public double? MABKASM { get; set; }
            public double? MEGHEZM { get; set; }
            public double? MABEZM { get; set; }
            public double? MEGHBFM { get; set; }
            public double? MABBFM { get; set; }
            public int? VCOD { get; set; }
            public double? MEGHSAYER { get; set; }
            public double? MABSAYER { get; set; }
            public double? MEGHSAYES { get; set; }
            public double? MABSAYES { get; set; }
            public double? MEGHBKHAM { get; set; }
            public double? MABKHAM { get; set; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            C_TARAZ_KHAS_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<MDS>($"{SQL_DATA}").ToList();

            foreach (var item in MasterHead)
            {
                C_TARAZ_KHAS_DATA.Add(item);
            }

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            //{
            //    e.Handled = true;

            //    var currentRow = SYNCFUSION_DG.SelectedItem as MDS;

            //    if (currentRow?.NUMBER != null)
            //    {
            //        OpenWindow(typeof(HEAD_LST_ENTEGHAL_WIN), null , "یک پنجره انتقالی از قبل باز شده ابتدا آنرا ببندید.");
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
