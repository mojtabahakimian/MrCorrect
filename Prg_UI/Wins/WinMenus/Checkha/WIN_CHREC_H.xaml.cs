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

namespace Wins.WinMenus.Checkha
{
    public partial class WIN_CHREC_H : Window
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
        public WIN_CHREC_H()
        {
            InitializeComponent();

            this.DataContext = this;
        }


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

        public ObservableCollection<CHKREC_H> VOSULMASTER_DATA { get; set; } = new ObservableCollection<CHKREC_H>();
        public ObservableCollection<CHRE_LIST_Q> DETAILVOSUL_DATA { get; set; } = new ObservableCollection<CHRE_LIST_Q>();

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

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }
            CL_HESABDARI.SETSECURITYSUB(DETAIL_VOSUL_SUB, "CHKREC_H");

            ReGetData();

            CL_LMethods.FocusLastSfDataGridRow(VOSULMASTER_SUB);
        }

        private void ReGetData()
        {
            VOSULMASTER_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<CHKREC_H>(@$"SELECT DATE, MOLAH, N_S, IDH, CRT, UID FROM dbo.CHKREC_H ORDER BY DATE DESC").ToList();
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

            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
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
                        //this.lsanad.ForeColor = 125;
                    }
                    else
                    {
                        ghat = false;
                        this.AllowDeletions = true;
                        this.AllowEdits = true;
                        //this.lsanad.ForeColor = 65535;
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
        private readonly FilterService<CHKREC_H> filterService = new FilterService<CHKREC_H>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool NowIsReady { get; private set; }

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void VOSULMASTER_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
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
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName)?.GetValue(record)?.ToString();
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
            VOSULMASTER_SUB.View.Filter = item => filterService.ApplyFilter(item as CHKREC_H);
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
            //// Get the selected row and column index
            //var currentCell = VOSULMASTER_SUB.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null)
            //{
            //    var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
            //    UpdateCurrentCellValue(rowColumnIndex);
            //}

            DETAILVOSUL_DATA?.Clear();
            var Row = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            if (Row != null & Row?.DATE != null)
            {
                DETAIL_VOSUL_SUB.IsEnabled = true;

                var RST = dbms.DoGetDataSQL<CHRE_LIST_Q>(@$"SELECT dbo.CHRE_LIST_Q.N_SERI,
                                                                   dbo.CHRE_LIST_Q.BANK,
                                                                   dbo.TCOD_BANKS.NAMES AS BANK_NAME,
                                                                   dbo.CHRE_LIST_Q.DATE_S,
                                                                   dbo.CHRE_LIST_Q.DATE,
                                                                   dbo.CHRE_LIST_Q.RADIF,
                                                                   dbo.CHRE_LIST_Q.SHOBEH,
                                                                   dbo.CHRE_LIST_Q.MABL,
                                                                   dbo.CHRE_LIST_Q.Expr1,
                                                                   dbo.CHRE_LIST_Q.N_KOL3,
                                                                   dbo.CHRE_LIST_Q.N_MOIN3,
                                                                   dbo.CHRE_LIST_Q.N_MOIN,
                                                                   dbo.CHRE_LIST_Q.N_TAF,
                                                                   dbo.CHRE_LIST_Q.N_TAF3,
                                                                   dbo.CHRE_LIST_Q.N_S,
                                                                   dbo.CHRE_LIST_Q.Expr2,
                                                                   dbo.CHRE_LIST_Q.Expr3,
                                                                   dbo.CHRE_LIST_Q.HES1,
                                                                   (
                                                                       SELECT NAME FROM dbo.CUST_HESAB WHERE (hes = N'' + dbo.CHRE_LIST_Q.HES1 + '')
                                                                   ) AS HES1_NAME
                                                            FROM dbo.CHRE_LIST_Q
                                                                LEFT OUTER JOIN dbo.TCOD_BANKS
                                                                    ON dbo.CHRE_LIST_Q.BANK = dbo.TCOD_BANKS.CODE
                                                            WHERE (dbo.CHRE_LIST_Q.DATE = N'{Row.DATE}')").ToList();
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

            var MasterRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            if (MasterRow != null)
            {
                dbms.DoExecuteSQL($@"UPDATE dbo.CHKREC_H SET MOLAH = N'{MasterRow.MOLAH}', DATE = {MasterRow.DATE} WHERE IDH = {MasterRow.IDH}");
                universControl.PopNotifyShow("ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }

            return;
            VOSULMASTER_SUB.RowValidating -= VOSULMASTER_SUB_RowValidating;

            if (!VOSULMASTER_SUB.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
            {
                VOSULMASTER_SUB.RowValidating += VOSULMASTER_SUB_RowValidating;

                e.IsValid = false;
                universControl.PopNotifyShow("سطر جاری صحیح نیست لطفا مثادیر را اصلاح کنید.", Pop1, Pop1Text1, Pop_Border1);
                return;
            }
            else
            {

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
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            if (CurrentRow != null && CurrentRow?.IDH != null)
            {
                AUTO_BAZ.Functions.CL_HESABDARI_AUTO_BAZ.GENSANADVD(Convert.ToInt64(CurrentRow.IDH), Convert.ToInt64(CurrentRow.IDH), false);
            }
        }

        private void BTN_ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (!isMasterRowValid)
            {
                return;
            }

            DateTime dt = DateTime.Now;
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            if (CurrentRow != null && CurrentRow?.DATE != null)
            {
                CL_HESABDARI.TR("CHKREC_H", "(date = " + CurrentRow.DATE + ")", dt, 1);
                CL_HESABDARI.TR("CHRE_LST", "(date = " + CurrentRow.DATE + ")", dt, 1);

                if (sender != null)
                {
                    BTN_ESLAH.IsEnabled = true;
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
                    tableName: "اعلام وصول چکهای دریافتی",
                    recordId: null,
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            var MasterCurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            var DetailCurrentRow = DETAIL_VOSUL_SUB.SelectedItem as CHRE_LIST_Q;

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
                                var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD " + _WHERE_).ToList();
                                if (rst.Count == 1)
                                {
                                    rst.FirstOrDefault().N_KOL3 = null;
                                    rst.FirstOrDefault().N_MOIN3 = null;
                                    rst.FirstOrDefault().N_TAF3 = null;
                                    rst.FirstOrDefault().HES3 = null;
                                    rst.FirstOrDefault().N_S = null;

                                    dbms.DoExecuteSQL($@"UPDATE PAY_GETD SET 
                                 N_KOL3 = NULL, 
                                 N_MOIN3 = NULL, 
                                 N_TAF3 = NULL, 
                                 HES3 = NULL, 
                                 N_S = NULL
                                {_WHERE_} "); //rst.update();

                                    dbms.DoExecuteSQL("DELETE FROM dbo.CHRE_LST WHERE     N_SERI = " + DetailCurrentRow.N_SERI + " AND BANK = " + DetailCurrentRow.BANK + " AND DATE_S = " + DetailCurrentRow.DATE_S);

                                    CL_HESABDARI.GETDLOG(1, DetailCurrentRow.N_SERI.ToString(), (int)DetailCurrentRow.BANK, (long)DetailCurrentRow.DATE_S, (int)rst.FirstOrDefault().SANDUGH);
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
                            dbms.DoExecuteSQL($@"DELETE FROM dbo.CHKREC_H WHERE IDH = {MasterCurrentRow.IDH}");

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

            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
            if (CurrentRow != null)
            {
                //SANAD();

                
                var report = new StiReport();
                var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream("Prg_UI.Rpts.Checkha.ELVSL_ADA.mrt");
                using (pathreport)
                {
                    report.Load(pathreport);

                    string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                    report.Dictionary.Databases.Clear();
                    report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

                    report["DATE_PARAM"] = CurrentRow.DATE;
                    ((StiSqlSource)report.Dictionary.DataSources["DataSource1"]).CommandTimeout = 300;

                    if (report.GetComponentByName("WIDTH_D") is StiText stiText) stiText.Text = Baseknow.WIDTH_D; // نام شرکت
                    //report.Render();
                    //report.Show();

                    new Rpts.WINRPT(report, "اعلام وصول چکهای دریافتی").Show();
                }
            }
        }

        private void BTN_N_S_OPEN_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = VOSULMASTER_SUB.SelectedItem as CHKREC_H;
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


        //private static void DATE_DblClick(int CANCEL)
        //{
        //    DoCmd.OpenForm("FERSAL_SANAD", default, default, default, default, acDialog, 6);
        //}

    }
}
