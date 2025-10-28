using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.HelperWins;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using Microsoft.Data.SqlClient;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.Generaly;
using Stimulsoft.Base;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using Microsoft.VisualBasic;
using DocumentFormat.OpenXml.Presentation;

namespace Wins.WinMenus.Checkha
{
    public partial class WIN_CHREC_HP : Window
    {
        #region Header Window Begin
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
        #endregion
        public WIN_CHREC_HP()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region LOCALMODEL
        public class SQLPAYCHECK1
        {
            public int? idh { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public long? DATE { get; set; }
            public double? RADIF { get; set; }
            public double? N_MOIN { get; set; }
            public double? N_TAF { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public int? N_S { get; set; }
            public double? MABL { get; set; }
            public int? N_KOL { get; set; }
            public int? KIND { get; set; }
            public string? HES1 { get; set; }
        }
        #endregion
        private double _sum_of_mabl = 0;
        public double SUM_OF_MABL
        {
            get
            {
                _sum_of_mabl = (double)DETAILVOSUL_DATA.Sum(r => r.MABL);
                if (_sum_of_mabl == 0) _sum_of_mabl = 0;
                return _sum_of_mabl;
            }
            set { _sum_of_mabl = value; }
        }

        public ObservableCollection<CHREC_HP> VOSULMASTER_DATA { get; set; } = new ObservableCollection<CHREC_HP>();
        public ObservableCollection<CHREC_LSP_Q> DETAILVOSUL_DATA { get; set; } = new ObservableCollection<CHREC_LSP_Q>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                VOSULMASTER_SUB.IsReadOnly = !ican;
                //DETAIL_VOSUL_SUB.IsReadOnly = !ican;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHP", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            CL_HESABDARI.SETSECURITYSUB(DETAIL_VOSUL_SUB, "CHREC_HP");

            ReGetData();

            CL_LMethods.FocusLastSfDataGridRow(VOSULMASTER_SUB);
        }

        private void ReGetData()
        {
            VOSULMASTER_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<CHREC_HP>(@$"SELECT DATE, MOLAH, N_S, IDH, CRT, UID FROM dbo.CHREC_HP ORDER BY DATE DESC").ToList();
            foreach (var item in MasterHead)
            {
                VOSULMASTER_DATA.Add(item);
            }
        }
        private void Form_Current()
        {
            bool ghat;
            if (VOSULMASTER_DATA.Count > 0)
            {
                PRINTC.IsEnabled = true;
            }
            else
            {
                this.PRINTC.IsEnabled = false;
            }

            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (CurrentRow != null && CurrentRow?.IDH != null)
            {
                if (CurrentRow?.N_S == null)
                {
                    this.AllowDeletions = true;
                    this.AllowEdits = true;
                }
                else
                {
                    var rst = dbms.DoGetDataSQL<bool?>($"SELECT TOP 1 GHATEI FROM dbo.DEED_HED WHERE N_S = {CurrentRow.N_S}").FirstOrDefault();
                    if (rst != null && Convert.ToBoolean(rst))
                    {
                        ghat = true;
                        this.AllowDeletions = false;
                        this.AllowEdits = false;
                        //this.DETAIL_VOSUL_SUB.IsReadOnly = true;
                        this.AllowDeletions = false;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                    }
                }
                if (CurrentRow?.DATE == null)
                {
                    //this.CHRE_LST_SUB.Enabled = false;
                    this.DETAIL_VOSUL_SUB.IsEnabled = false;
                }
                else
                {
                    this.DETAIL_VOSUL_SUB.IsEnabled = true;
                }
            }

        }

        #region _SfDataGrid_
        private readonly FilterService<CHREC_HP> filterService = new FilterService<CHREC_HP>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool NowIsReady { get; private set; }

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void VOSULMASTER_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.VOSULMASTER_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.VOSULMASTER_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.VOSULMASTER_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.VOSULMASTER_SUB.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell

            if (!string.IsNullOrEmpty(selectedText))
            {
                // Add the Contains filter to the filter service (inclusion filter)
                filterService.AddFilter(columnName, selectedText, isExclusion: false); // False means it's an inclusion filter
                ActiveFilters.Add($"{columnName} Contains {selectedText}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
            else
            {
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

        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
                if (filterValue != null)
                {
                    // Add the Not Contains filter to the filter service (exclusion filter)
                    filterService.AddFilter(columnName, selectedText, isExclusion: true); // True means it's an exclusion filter
                                                                                          // Add the exclusion filter to the list of active filters
                    ActiveFilters.Add($"{columnName} Does Not Contain {selectedText}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
            }
            else
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
        }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails()
        {
            // Check if there is a current cell selected in the data grid
            if (VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                            // Return the column name and the current cell value
                                                                                                                            //if (CurrentCellValue == null)
                                                                                                                            //{
                                                                                                                            //    return (columnName, SelectedSfDgTextCell);
                                                                                                                            //}
                                                                                                                            //else
                {
                    return (columnName, CurrentCellValue);
                }
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter()
        {
            // Set the filter for the data grid view using the filter service
            VOSULMASTER_SUB.View.Filter = item => filterService.ApplyFilter(item as CHREC_HP);
            // Refresh the filter to update the view
            VOSULMASTER_SUB.View.RefreshFilter();
        }

        private void VOSULMASTER_SUB_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }
        private void VOSULMASTER_SUB_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }

            return;

            var point = e.GetPosition(VOSULMASTER_SUB);

            // Get the element under the mouse
            var hitElement = e.OriginalSource as DependencyObject;
            if (hitElement == null) return;

            // Find the cell element
            var cell = FindParent<Syncfusion.UI.Xaml.Grid.GridCell>(hitElement);
            if (cell == null) return;

            // Check if the cell is in edit mode
            if (VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            {
                var editingElement = VOSULMASTER_SUB.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    // Capture the selected text instead of the full text
                    SelectedSfDgTextCell = editingElement.SelectedText;
                }
            }

            ////// Get the DataGrid and the VisualContainer
            //var visualContainer = VOSULMASTER_SUB?.GetVisualContainer();

            //if (visualContainer == null) return;

            //// Get the position of the mouse click
            //var position = e.GetPosition(visualContainer);

            //// Get the cell's RowColumnIndex
            //var cellIndex = visualContainer.PointToCellRowColumnIndex(position);
            //if (cellIndex.RowIndex < 0 || cellIndex.ColumnIndex < 0) return;

            //var rowColumnIndex = new RowColumnIndex(cellIndex.RowIndex, cellIndex.ColumnIndex);
            //UpdateCurrentCellValue(rowColumnIndex);

            //// Check if the cell is in edit mode
            //if (VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            //{
            //    var editingElement = VOSULMASTER_SUB.FindElementOfType<TextBox>();
            //    if (editingElement != null)
            //    {
            //        CurrentCellValue = editingElement.Text; // Update with the text being edited
            //    }
            //}

        }
        private string GetSelectedText()
        {
            var dataGrid = VOSULMASTER_SUB;
            var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;

            if (currentCell != null && currentCell.IsEditing)
            {
                // Find the editing element (which will be a TextBox in edit mode)
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    return editingElement.SelectedText; // Return the selected text
                }
            }
            return string.Empty;
        }
        #endregion
        private void CANCEL_MASTER_SUB()
        {
            VOSULMASTER_SUB.CurrentCellValidating -= VOSULMASTER_SUB_CurrentCellValidating;
            VOSULMASTER_SUB.RowValidating -= VOSULMASTER_SUB_RowValidating;

            VOSULMASTER_SUB.SelectionController.CurrentCellManager.CheckValidationAndEndEdit();

            VOSULMASTER_SUB.CurrentCellValidating += VOSULMASTER_SUB_CurrentCellValidating;
            VOSULMASTER_SUB.RowValidating += VOSULMASTER_SUB_RowValidating;
        }

        bool isMasterRowValid = true;
        private void VOSULMASTER_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            DETAILVOSUL_DATA?.Clear();
            var MasterRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (MasterRow != null & MasterRow?.DATE != null)
            {
                DETAIL_VOSUL_SUB.IsEnabled = true;

                var RST = dbms.DoGetDataSQL<CHREC_LSP_Q>(@$"SELECT dbo.CHREC_LSP_Q.N_SERI,
                                                                 dbo.CHREC_LSP_Q.BANK,
                                                                 dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                                 dbo.CHREC_LSP_Q.DATE_S,
                                                                 dbo.CHREC_LSP_Q.DATE,
                                                                 dbo.CHREC_LSP_Q.RADIF,
                                                                 dbo.CHREC_LSP_Q.SHOBEH,
                                                                 dbo.CHREC_LSP_Q.MABL,
                                                                 dbo.CHREC_LSP_Q.N_KOL3,
                                                                 dbo.CHREC_LSP_Q.N_MOIN3,
                                                                 dbo.CHREC_LSP_Q.N_MOIN,
                                                                 dbo.CHREC_LSP_Q.N_TAF,
                                                                 dbo.CHREC_LSP_Q.N_TAF3,
                                                                 dbo.CHREC_LSP_Q.HES1,
                                                                 (
                                                                     SELECT NAME FROM dbo.CUST_HESAB WHERE (hes = N'' + dbo.CHREC_LSP_Q.HES1 + '')
                                                                 ) AS HES1_NAME
                                                          FROM dbo.CHREC_LSP_Q
                                                              LEFT OUTER JOIN dbo.TCOD_BANKS
                                                                  ON dbo.CHREC_LSP_Q.BANK = dbo.TCOD_BANKS.CODE
                                                          WHERE (dbo.CHREC_LSP_Q.DATE = N'{MasterRow.DATE}')").ToList();
                foreach (var item in RST)
                {
                    DETAILVOSUL_DATA.Add(item);
                }

                MABL_JAM.Text = SUM_OF_MABL.ToStringNullSafe();
            }
            else
            {
                DETAIL_VOSUL_SUB.IsEnabled = false;
            }

            Form_Current();

        }
        private void VOSULMASTER_SUB_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            var column = VOSULMASTER_SUB.Columns["MOLAH"] as GridTextColumn; //ملاحظات
            if (column != null)
            {
                var currentCell = VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell;
                if (currentCell != null && currentCell.IsEditing)
                {
                    var editingElement = VOSULMASTER_SUB.FindElementOfType<TextBox>();
                    if (editingElement != null)
                    {
                        editingElement.MaxLength = 39;
                    }
                }
            }
        }
        private void VOSULMASTER_SUB_CurrentCellValidating(object sender, CurrentCellValidatingEventArgs e)
        {
            if (e.Column.MappingName == "DATE") //تاریخ
            {
                string? value = e.NewValue?.ToStringNullSafe();
                if (!IsValidDate(value))
                {
                    e.IsValid = false;
                    e.ErrorMessage = "مقدار تاریخ صحیح نیست";
                    //CANCEL_MASTER_SUB();
                }
            }
            if (e.Column.MappingName == "MOLAH") //ملاحظات
            {
                string? value = e.NewValue?.ToStringNullSafe();
                if (value != null && value.Length > 39)
                {
                    e.IsValid = false;
                    e.ErrorMessage = "مقدار ملاحظات بیش از حد مجاز است ";
                    //CANCEL_MASTER_SUB();
                }
            }

            isMasterRowValid = e.IsValid;
        }
        private void VOSULMASTER_SUB_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (!isMasterRowValid)
            {
                return;
            }

            var MasterRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (MasterRow != null)
            {
                dbms.DoExecuteSQL($@"UPDATE dbo.CHREC_HP SET MOLAH = N'{MasterRow.MOLAH}', DATE = {MasterRow.DATE} WHERE IDH = {MasterRow.IDH}");
                universControl.PopNotifyShow("ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }

        private bool IsValidDate(string date_n_val)
        {
            date_n_val = date_n_val.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }

            return true;
        }

        private void SANAD()
        {
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (CurrentRow != null && CurrentRow?.IDH != null)
            {
                double max_ns, MABL_CHK;
                string SHART;
                double? CKOLV = default, CMOINV = default, CTAFV = default, CTAF2V = default, CTAF3V = default, CTAF4V = default, CKOL = default, CMOIN = default, CTAF = default, CTAF2 = default, CTAF3 = default, CTAF4 = default, CKOLD = default, CMOIND = default, CTAFD = default, CTAF2D = default, CTAF3D = default, CTAF4D = default;
                if (true)
                {
                    var rst = dbms.DoGetDataSQL<double?>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED").FirstOrDefault();
                    if (rst == null)
                    {
                        max_ns = 1d;
                    }
                    else
                    {
                        max_ns = (double)(rst + 1);
                    }
                    if (CurrentRow.N_S == null)
                    {
                        //SHRST.AddNew();

                        var _N_S_ = max_ns;
                        var _DATE_S_ = CurrentRow.DATE;
                        var _SHARH_S_ = "اعلام وصول چكهاي پرداختي";
                        var _NO_S_ = 7;
                        var _GHATEI_ = 0;
                        var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                        var _UID_ = Baseknow.USERCOD;
                        //SHRST.update();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_HED(N_S, DATE_S, SHARH_S, NO_S, GHATEI, USER_NAME, UID)
                                             VALUES({_N_S_},
                                             {_DATE_S_},
                                             N'{_SHARH_S_}',
                                             {_NO_S_},
                                             {_GHATEI_},
                                             N'{_USER_NAME_}',
                                             {_UID_} )");
                    }
                    else
                    {
                        SHART = "N_S = " + CurrentRow.N_S + " AND  NO_S = 7 ";
                        var SHRST = dbms.DoGetDataSQL<DEED_HED>($"SELECT TOP 1 * FROM dbo.DEED_HED WHERE {SHART} ").ToList(); //SHRST.Filter = SHART;
                        if (SHRST.Count == 0)
                        {
                            //SHRST.AddNew();
                            var _N_S_ = max_ns;
                            var _DATE_S_ = CurrentRow.DATE;
                            var _SHARH_S_ = "اعلام وصول چكهاي پرداختي";
                            var _GHATEI_ = 0;
                            var _NO_S_ = 7;
                            var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                            var _UID_ = Baseknow.USERCOD;

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_HED(N_S, DATE_S, SHARH_S, NO_S, GHATEI, USER_NAME, UID)
                                             VALUES({_N_S_},
                                             {_DATE_S_},
                                             N'{_SHARH_S_}',
                                             {_NO_S_},
                                             {_GHATEI_},
                                             N'{_USER_NAME_}',
                                             {_UID_} )");
                            //SHRST.update();
                        }
                        else
                        {
                            var _DATE_S_ = CurrentRow.DATE;
                            var _SHARH_S_ = "اعلام وصول چكهاي پرداختي";
                            var _GHATEI_ = 0;
                            var _NO_S_ = 7;
                            var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                            dbms.DoExecuteSQL($@"UPDATE dbo.DEED_HED SET
                                                 DATE_S = {_DATE_S_},  
                                                 SHARH_S = N'{_SHARH_S_}',  
                                                 GHATEI = {_GHATEI_},  
                                                 NO_S = {_NO_S_},  
                                                 USER_NAME = N'{_USER_NAME_}'
                                                 WHERE N_S = {SHRST.FirstOrDefault().N_S} ");
                            //SHRST.update();
                        }
                    }
                    if (CurrentRow.N_S == null)
                    {
                        CurrentRow.N_S = max_ns;
                    }

                    if (CurrentRow.N_S != null)
                    {
                        dbms.DoExecuteSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + CurrentRow.N_S + " ))");
                    }
                }

                if (true)
                {
                    var rst = dbms.DoGetDataSQL<SQLPAYCHECK1>("SELECT     dbo.CHREC_HP.idh, dbo.CHRE_LSP.*,   dbo.TCOD_BANKS.NAMES, dbo.PAY_GETP.SHOBEH,dbo.PAY_GETP.N_S, dbo.PAY_GETP.MABL, dbo.PAY_GETP.N_KOL,  dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF, dbo.PAY_GETP.KIND, dbo.PAY_GETP.HES1    FROM         dbo.CHREC_HP INNER JOIN dbo.CHRE_LSP ON dbo.CHREC_HP.DATE = dbo.CHRE_LSP.DATE INNER JOIN  dbo.PAY_GETP ON dbo.CHRE_LSP.N_SERI = dbo.PAY_GETP.N_SERI AND dbo.CHRE_LSP.BANK = dbo.PAY_GETP.BANK AND  dbo.CHRE_LSP.DATE_S = dbo.PAY_GETP.DATE_S INNER JOIN dbo.TCOD_BANKS ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHREC_HP.IDH = " + CurrentRow.IDH + ")").ToList();
                    if (!string.IsNullOrEmpty(Baseknow.APA))
                    {
                        CL_HESABDARI.GETTAF3(Baseknow.APA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    }
                    if (!string.IsNullOrEmpty(Baseknow.APV))
                    {
                        CL_HESABDARI.GETTAF3(Baseknow.APV, ref CKOLV, ref CMOINV, ref CTAFV, ref CTAF2V, ref CTAF3V, ref CTAF4V);
                    }

                    for (int i = 0; i < rst.Count; i++) //while (!rst.EOF)
                    {
                        double? N_S = null;

                        double? HES_K = null;
                        double? HES_M = null;
                        double? HES_T = null;
                        double? HES_T2 = null;
                        double? HES_T3 = null;
                        double? HES_T4 = null;

                        string? hes = null;

                        //SHRST.AddNew(); 
                        N_S = CurrentRow.N_S;
                        if (rst[i].KIND == 1 || rst[i].KIND == null)
                        {
                            HES_K = CKOL;
                            HES_M = CMOIN;
                            HES_T = CTAF;
                            HES_T2 = CTAF2;
                            HES_T3 = CTAF3;
                            HES_T4 = CTAF4;
                            hes = Baseknow.APA;
                        }
                        else
                        {
                            HES_K = CKOLV;
                            HES_M = CMOINV;
                            HES_T = CTAFV;
                            HES_T2 = CTAF2V;
                            HES_T3 = CTAF3V;
                            HES_T4 = CTAF4V;
                            hes = Baseknow.APV;
                        }
                        var N_SERI = rst[i].N_SERI;
                        var BANK = rst[i].BANK;
                        var BED = rst[i].MABL;
                        var SHARH = Strings.Left(" چك " + rst[i].N_SERI + " بانك " + rst[i].NAMES + " " + rst[i].SHOBEH + " مورخ " + Strings.Format(rst[i].DATE_S, "####/##/##"), 255);
                        //SHRST.update(); //INSERT

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES_T2, HES_T3, HES_T4,HES)
                                             VALUES({N_S},
                                             {(HES_K is null ? "NULL" : HES_K)} ,
                                             {(HES_M is null ? "NULL" : HES_M)} ,
                                             {(HES_T is null ? "NULL" : HES_T)} ,
                                             N'{SHARH}' ,
                                             {BED} ,
                                             0.0 ,
                                             {N_SERI} ,
                                             {BANK} ,
                                             {(HES_T2 is null ? "NULL" : HES_T2)} ,
                                             {(HES_T3 is null ? "NULL" : HES_T3)} ,
                                             {(HES_T4 is null ? "NULL" : HES_T4)},
                                             {(hes is null ? "NULL" : hes)} )");

                        CL_HESABDARI.GETTAF3(rst[i].HES1, ref CKOLD, ref CMOIND, ref CTAFD, ref CTAF2D, ref CTAF3D, ref CTAF4D);

                        //SHRST.AddNew();
                        N_S = CurrentRow.N_S;
                        HES_K = CKOLD;
                        HES_M = CMOIND;
                        HES_T = CTAFD;
                        HES_T2 = CTAF2D;
                        HES_T3 = CTAF3D;
                        HES_T4 = CTAF4D;
                        hes = rst[i].HES1;
                        N_SERI = rst[i].N_SERI;
                        BANK = rst[i].BANK;
                        var BES = rst[i].MABL;
                        SHARH = Strings.Left(" چك " + rst[i].N_SERI + " بانك " + rst[i].NAMES + " " + rst[i].SHOBEH + " مورخ " + Strings.Format(rst[i].DATE_S, "####/##/##"), 255);
                        //SHRST.update();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES_T2, HES_T3, HES_T4,HES)
                                             VALUES({N_S},
                                             {(HES_K is null ? "NULL" : HES_K)} ,
                                             {(HES_M is null ? "NULL" : HES_M)} ,
                                             {(HES_T is null ? "NULL" : HES_T)} ,
                                             N'{SHARH}' ,
                                             0 ,
                                             {BES} ,
                                             {N_SERI} ,
                                             {BANK} ,
                                             {(HES_T2 is null ? "NULL" : HES_T2)} ,
                                             {(HES_T3 is null ? "NULL" : HES_T3)} ,
                                             {(HES_T4 is null ? "NULL" : HES_T4)},
                                             {(hes is null ? "NULL" : hes)} )");

                        rst[i].N_S = (int?)CurrentRow.N_S;
                        dbms.DoExecuteSQL($"UPDATE PAY_GETP SET N_S = {rst[i].N_S} WHERE N_SERI = {rst[i].N_SERI} AND BANK = {rst[i].BANK} AND DATE_S = {rst[i].DATE_S}");
                        //rst.update(); //INSERT
                    }
                }
            }

            universControl.PopNotifyShow("سند بروز شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
        }

        private void BTN_ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!isMasterRowValid)
            {
                return;
            }

            DateTime dt = DateTime.Now;
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (CurrentRow != null && CurrentRow?.DATE != null)
            {
                CL_HESABDARI.TR("CHREC_HP", "(date = " + CurrentRow.DATE + ")", dt, 1);
                //CL_HESABDARI.TR("CHREC_LSP", "(date = " + CurrentRow.DATE + ")", dt, 1); //Wrong Name Column
                CL_HESABDARI.TR("CHRE_LSP", "(date = " + CurrentRow.DATE + ")", dt, 1);

                if (sender != null)
                {
                    BTN_DELETEVOSUL.IsEnabled = true;
                }
            }
        }

        private void BTN_DELETEVOSUL_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_DELETEVOSUL.IsEnabled || BTN_DELETEVOSUL.Visibility != Visibility.Visible)
            {
                return;
            }
            if (!isMasterRowValid)
            {
                return;
            }


            BTN_ESLAH_Click(null, null);

            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "اعلام وصول چکهای پرداختی",
                    recordId: null,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            var MasterCurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            var DetailCurrentRow = DETAIL_VOSUL_SUB.SelectedItem as CHREC_LSP_Q;

            if (MasterCurrentRow != null)
            {
                if (DETAILVOSUL_DATA.Count > 0) //Sub Detail still have items
                {
                    if (DetailCurrentRow != null)
                    {
                        Msgwin msgwin2 = new Msgwin(true, $"آیا از حذف اطمینان دارید  \n شماره سریال :{DetailCurrentRow.N_SERI}  بانک : {DetailCurrentRow.BANK}  تاریخ : {DetailCurrentRow.DATE_S} ؟");
                        msgwin2.ShowDialog();
                        if (msgwin2.DialogResult == true)
                        {
                            try
                            {
                                string _WHERE_ = $" WHERE N_SERI =" + DetailCurrentRow.N_SERI + " AND BANK = " + DetailCurrentRow.BANK + " AND DATE_S = " + DetailCurrentRow.DATE_S;
                                var rst = dbms.DoGetDataSQL<PAY_GETP>("SELECT * FROM PAY_GETP " + _WHERE_).ToList();
                                if (rst.Count == 1)
                                {
                                    rst.FirstOrDefault().N_KOL3 = null;
                                    rst.FirstOrDefault().N_MOIN3 = null;
                                    rst.FirstOrDefault().N_TAF3 = null;
                                    rst.FirstOrDefault().HES3 = null;
                                    rst.FirstOrDefault().N_S = null;

                                    dbms.DoExecuteSQL($@"UPDATE PAY_GETP SET 
                                                         N_KOL3 = NULL, 
                                                         N_MOIN3 = NULL, 
                                                         N_TAF3 = NULL, 
                                                         N_S = NULL
                                                         {_WHERE_} ");

                                    dbms.DoExecuteSQL("DELETE FROM dbo.CHRE_LSP WHERE     N_SERI = " + DetailCurrentRow.N_SERI + " AND BANK = " + DetailCurrentRow.BANK + " AND DATE_S = " + DetailCurrentRow.DATE_S);
                                }

                                DETAILVOSUL_DATA.Remove(DetailCurrentRow);

                                universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                            }
                            catch (SqlException ex)
                            {
                                if (ex.Number == 547)
                                {
                                    new Msgwin(false, "این وصولی دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                                }
                                else
                                {
                                    new Msgwin(false, "خطا در انجام عملیات حذف وصولی").ShowDialog();
                                }
                            }
                            catch (Exception)
                            {
                                new Msgwin(false, "خطا در انجام عملیات حذف وصولی").ShowDialog();
                            }
                        }
                    }
                }
                else //Sub Detail is null
                {
                    Msgwin msgwin = new Msgwin(true, $"آیا از حذف اطمینان دارید ؟");
                    msgwin.ShowDialog();
                    if (msgwin.DialogResult == true)
                    {
                        try
                        {
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.CHREC_HP WHERE IDH = {MasterCurrentRow.IDH}");

                            VOSULMASTER_DATA.Remove(MasterCurrentRow);

                            universControl.PopNotifyShow("حذف انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 547)
                            {
                                new Msgwin(false, "این وصولی دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                            }
                            else
                            {
                                new Msgwin(false, "خطا در انجام عملیات حذف وصولی").ShowDialog();
                            }
                        }
                        catch (Exception)
                        {
                            new Msgwin(false, "خطا در انجام عملیات حذف وصولی").ShowDialog();
                        }
                    }
                }
            }

        }

        private void PRINTC_Click(object sender, RoutedEventArgs e)
        {
            if (!isMasterRowValid)
            {
                return;
            }

            //ELVSL_APA

            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (CurrentRow != null)
            {
                
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Checkha.ELVSL_APA.mrt");
                using (pathreport)
                {
                    report.Load(pathreport);

                    string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
                    report.Dictionary.Databases.Clear();
                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                    report["DATE_PARAM"] = CurrentRow.DATE;
                    ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 900;

                    if (report.GetComponentByName("WIDTH_D") is StiText stiText) stiText.Text = Baseknow.WIDTH_D; // نام شرکت
                    //report.Render();
                    //report.Show();

                    new Rpts.WINRPT(report, "اعلام وصول چکهای پرداختی").Show();
                }
            }
        }

        private void BTN_N_S_OPEN_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHREC_HP;
            if (CurrentRow != null && CurrentRow?.IDH != null && CurrentRow?.N_S != null)
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this, Convert.ToDouble(CurrentRow.N_S));
            }
        }

        private void BTN_N_SCHECK_Click(object sender, RoutedEventArgs e)
        {
            SANAD();
        }

        private void DETAIL_VOSUL_SUB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }


    }
}
