using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Collections.ObjectModel;
using static Prg_UI.Functions.CL_LMethods;
using Prg_Proccessy.SQLMODELS;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Interop;
using Prg_Proccessy.FUNCTIONS;
using Functions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Prg_Proccessy.MODELS;
using System.Windows.Threading;

namespace Prg_UI.Wins.WinMenus.CRM
{
    /// <summary>
    /// Interaction logic for CRMMAIN.xaml
    /// </summary>
    public partial class CRMMAIN : Window
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

        public CRMMAIN()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ObservableCollection<COPMANES> CUSTOMER_DATA { get; set; } = new ObservableCollection<COPMANES>();
        public ObservableCollection<CRMEVENTS> EVENT_DATA { get; set; } = new ObservableCollection<CRMEVENTS>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {
                _bl = value;
                IntPtr handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
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
                CRM_MASTER_SUB.IsReadOnly = !ican;
            }
        }

        public class Status_List
        {
            public int CODE { get; set; }
            public string NAME { get; set; }
        }
        public class Fac_List
        {
            public string? STATUS_FACT { get; set; }
        }

        public bool NowIsReady { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                var element = Keyboard.FocusedElement as UIElement;
                element?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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
            ReGetData();
        }

        private void ReGetData()
        {
            CUSTOMER_DATA?.Clear();
            var customers = dbms.DoGetDataSQL<COPMANES>(@"SELECT COPMANES.*, eventscount.idcn FROM COPMANES LEFT OUTER JOIN eventscount ON COPMANES.id = eventscount.idc ORDER BY COPMANES.id").ToList();
            foreach (var item in customers)
            {
                CUSTOMER_DATA.Add(item);
            }
        }
        private void LoadDetail()
        {
            EVENT_DATA?.Clear();
            var current = CRM_MASTER_SUB.SelectedItem as COPMANES;
            if (current != null && current.ID != null)
            {
                var dets = dbms.DoGetDataSQL<CRMEVENTS>($"SELECT * FROM CRMEVENTS WHERE idc = {current.ID} ORDER BY idde").ToList();
                foreach (var item in dets)
                {
                    EVENT_DATA.Add(item);
                }
                DETAIL_CRM_SUB.IsEnabled = true;
            }
            else
            {
                DETAIL_CRM_SUB.IsEnabled = false;
            }
        }

        private void CRM_MASTER_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (!NowIsReady) return;
            LoadDetail();
        }

        private void CRM_MASTER_SUB_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
            var grid = sender as SfDataGrid;
            var column = grid?.CurrentColumn?.MappingName;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var record = grid?.CurrentItem as COPMANES;
                if (record == null || string.IsNullOrEmpty(column)) return;
                switch (column)
                {
                    case nameof(COPMANES.COMPANY_NAME):
                        CheckCompanyName(record.COMPANY_NAME);
                        break;
                    case nameof(COPMANES.FACT_TEL):
                        CheckFactTel(record.FACT_TEL);
                        break;
                    case nameof(COPMANES.MOBILE):
                        CheckMobile(record.MOBILE);
                        break;
                }
            }), DispatcherPriority.Background);
        }

        private void CRM_MASTER_SUB_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
        {
            if (e.NewObject is COPMANES row)
            {
                var today = Tarikh.FullCurrentDate;
                row.DT = int.TryParse(today, out var dtVal) ? (int?)dtVal : null;
                row.DATE_SABT = Tarikh.GetRawGregorianDateTime(today);
            }
        }

        private void CRM_MASTER_SUB_RowValidated(object sender, RowValidatedEventArgs e)
        {
            if (CRM_MASTER_SUB.View?.IsEditingItem == true)
            {
                CRM_MASTER_SUB.View.CommitEdit();
            }
            var row = e.RowData as COPMANES;
            if (row == null) return;
            if (row.DATE_SABT == null || row.DT == null)
            {
                var today = Tarikh.FullCurrentDate;
                row.DT = row.DT ?? (int.TryParse(today, out var dtVal) ? (int?)dtVal : null);
                row.DATE_SABT = row.DATE_SABT ?? Tarikh.GetRawGregorianDateTime(today);
            }
            var baseParams = new
            {
                row.COMPANY_NAME,
                row.CITY,
                row.MANAGER,
                row.FACT_TEL,
                row.MOBILE,
                row.PERNUM,
                row.STATUS_FACT,
                row.PRODUCTS,
                row.ADDR,
                row.ACCOUNTANT,
                row.SOFTWARE,
                row.ESP_PERSON,
                row.REAGENT,
                row.STATUS,
                row.COMMENT,
                DATE_SABT = row.DATE_SABT,
                row.USER_NAME,
                row.PIC,
                row.DT,
                row.USERID,
                row.LONGITUDE,
                row.LATITUDE,
                row.OSTANID,
                row.SHAHRID
            };
            if (row.ID == null || row.ID <= 0)
            {
                var sql = @"INSERT INTO COPMANES (COMPANY_NAME, CITY, MANAGER, FACT_TEL, MOBILE, PERNUM, STATUS_FACT, PRODUCTS, ADDR, ACCOUNTANT, SOFTWARE, ESP_PERSON, REAGENT, STATUS, COMMENT, date_sabt, USER_NAME, pic, dt, userid, Longitude, Latitude, OSTANID, SHAHRID)
                            VALUES (@COMPANY_NAME, @CITY, @MANAGER, @FACT_TEL, @MOBILE, @PERNUM, @STATUS_FACT, @PRODUCTS, @ADDR, @ACCOUNTANT, @SOFTWARE, @ESP_PERSON, @REAGENT, @STATUS, @COMMENT, @DATE_SABT, @USER_NAME, @PIC, @DT, @USERID, @LONGITUDE, @LATITUDE, @OSTANID, @SHAHRID);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var newId = dbms.DoGetDataSQL<int>(sql, baseParams).FirstOrDefault();
                if (newId > 0) row.ID = newId;
            }
            else
            {
                var sql = @"UPDATE COPMANES SET COMPANY_NAME=@COMPANY_NAME, CITY=@CITY, MANAGER=@MANAGER, FACT_TEL=@FACT_TEL, MOBILE=@MOBILE, PERNUM=@PERNUM, STATUS_FACT=@STATUS_FACT, PRODUCTS=@PRODUCTS, ADDR=@ADDR, ACCOUNTANT=@ACCOUNTANT, SOFTWARE=@SOFTWARE, ESP_PERSON=@ESP_PERSON, REAGENT=@REAGENT, STATUS=@STATUS, COMMENT=@COMMENT, date_sabt=@DATE_SABT, USER_NAME=@USER_NAME, pic=@PIC, dt=@DT, userid=@USERID, Longitude=@LONGITUDE, Latitude=@LATITUDE, OSTANID=@OSTANID, SHAHRID=@SHAHRID WHERE ID=@ID";
                var updateParams = new { row.ID, baseParams.COMPANY_NAME, baseParams.CITY, baseParams.MANAGER, baseParams.FACT_TEL, baseParams.MOBILE, baseParams.PERNUM, baseParams.STATUS_FACT, baseParams.PRODUCTS, baseParams.ADDR, baseParams.ACCOUNTANT, baseParams.SOFTWARE, baseParams.ESP_PERSON, baseParams.REAGENT, baseParams.STATUS, baseParams.COMMENT, baseParams.DATE_SABT, baseParams.USER_NAME, baseParams.PIC, baseParams.DT, baseParams.USERID, baseParams.LONGITUDE, baseParams.LATITUDE, baseParams.OSTANID, baseParams.SHAHRID };
                dbms.DoExecuteSQL(sql, updateParams);
            }
        }

        private void DETAIL_CRM_SUB_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            if (e.EditAction == DataGridEditAction.Cancel) return;
            // commit edit so the row item reflects latest user changes
            e.Row.BindingGroup.CommitEdit();
            if (e.Row.Item == null) return;
            var row = e.Row.Item as CRMEVENTS;
            var master = CRM_MASTER_SUB.SelectedItem as COPMANES;
            if (row == null || master == null || master.ID == null) return;
            row.IDC = master.ID;
            var baseParams = new
            {
                row.COMPANY_NAME,
                row.INFO_DATE,
                row.INFO_TIME,
                row.SALER,
                row.BUYER,
                row.COMMENT,
                row.NEXT_DATE,
                row.NEXT_TIME,
                row.STATUS,
                row.PIC,
                row.IDC,
                row.PAYAM,
                row.MITING,
                row.USERID,
                row.CDATETI
            };
            if (row.IDDE == null || row.IDDE <= 0)
            {
                var sql = @"INSERT INTO CRMEVENTS (COMPANY_NAME, INFO_DATE, INFO_TIME, SALER, BUYER, COMMENT, NEXT_DATE, NEXT_TIME, STATUS, pic, idc, PAYAM, miting, USERID, CDATETI)
                            VALUES (@COMPANY_NAME, @INFO_DATE, @INFO_TIME, @SALER, @BUYER, @COMMENT, @NEXT_DATE, @NEXT_TIME, @STATUS, @PIC, @IDC, @PAYAM, @MITING, @USERID, @CDATETI);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var newId = dbms.DoGetDataSQL<int>(sql, baseParams).FirstOrDefault();
                if (newId > 0) row.IDDE = newId;
            }
            else
            {
                var sql = @"UPDATE CRMEVENTS SET COMPANY_NAME=@COMPANY_NAME, INFO_DATE=@INFO_DATE, INFO_TIME=@INFO_TIME, SALER=@SALER, BUYER=@BUYER, COMMENT=@COMMENT, NEXT_DATE=@NEXT_DATE, NEXT_TIME=@NEXT_TIME, STATUS=@STATUS, pic=@PIC, idc=@IDC, PAYAM=@PAYAM, miting=@MITING, USERID=@USERID, CDATETI=@CDATETI WHERE idde=@IDDE";
                var updateParams = new { row.IDDE, baseParams.COMPANY_NAME, baseParams.INFO_DATE, baseParams.INFO_TIME, baseParams.SALER, baseParams.BUYER, baseParams.COMMENT, baseParams.NEXT_DATE, baseParams.NEXT_TIME, baseParams.STATUS, baseParams.PIC, baseParams.IDC, baseParams.PAYAM, baseParams.MITING, baseParams.USERID, baseParams.CDATETI };
                dbms.DoExecuteSQL(sql, updateParams);
            }
            LoadDetail();
        }

        private void CheckCompanyName(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            string shart = BuildShart(text, "NAME");
            string shart2 = BuildShart(text, "TNAME");
            if (shart.Length > 0 && shart2.Length > 0)
                shart = $"(({shart}) or ({shart2}))";
            var cnt = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM cust_hesab_dtl WHERE {shart}").FirstOrDefault();
            if (cnt > 0)
            {
                MessageBox.Show("مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
            var shartCust = shart.Replace("TNAME", "NAME").Replace("NAME", "COMPANY_NAME");
            var cnt2 = dbms.DoGetDataSQL<int>($"SELECT COUNT(1) FROM COPMANES WHERE {shartCust}").FirstOrDefault();
            if (cnt2 > 0)
            {
                MessageBox.Show("مشابه اين نام قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
        }

        private string BuildShart(string text, string field)
        {
            var words = text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> parts = new List<string>();
            foreach (var w in words)
            {
                var w1 = w.Replace('ی', 'ي').Replace("˜", "ß");
                var w2 = w.Replace('ي', 'ی').Replace("˜", "ß");
                parts.Add($"({field} LIKE N'%{w}%' OR {field} LIKE N'%{w1}%' OR {field} LIKE N'%{w2}%')");
            }
            return string.Join(" and ", parts);
        }

        private void CheckFactTel(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return;
            var cnt = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM cust_hesab WHERE TEL Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
        }

        private void CheckMobile(string? tel)
        {
            if (string.IsNullOrWhiteSpace(tel)) return;
            var cnt = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM cust_hesab WHERE TEL Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
            var cnt2 = dbms.DoGetDataSQL<int>("SELECT COUNT(1) FROM COPMANES WHERE MOBILE Like @p", new { p = "%" + tel + "%" }).FirstOrDefault();
            if (cnt2 > 0)
            {
                MessageBox.Show("مشابه اين شماره قبلا تعريف شده است لطفا دقت کنيد که مشتري جديد باشد");
            }
        }


        #region _SfDataGrid_
        private readonly FilterService<COPMANES> filterService = new FilterService<COPMANES>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void CRM_MASTER_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.CRM_MASTER_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.CRM_MASTER_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.CRM_MASTER_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.CRM_MASTER_SUB.View.Records.GetItemAt(recordIndex);
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
            if (CRM_MASTER_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = CRM_MASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
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
            CRM_MASTER_SUB.View.Filter = item => filterService.ApplyFilter(item as COPMANES);
            // Refresh the filter to update the view
            CRM_MASTER_SUB.View.RefreshFilter();
        }

        private void CRM_MASTER_SUB_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }
        private void CRM_MASTER_SUB_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }

            return;

            var point = e.GetPosition(CRM_MASTER_SUB);

            // Get the element under the mouse
            var hitElement = e.OriginalSource as DependencyObject;
            if (hitElement == null) return;

            // Find the cell element
            var cell = FindParent<Syncfusion.UI.Xaml.Grid.GridCell>(hitElement);
            if (cell == null) return;

            // Check if the cell is in edit mode
            if (CRM_MASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            {
                var editingElement = CRM_MASTER_SUB.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    // Capture the selected text instead of the full text
                    SelectedSfDgTextCell = editingElement.SelectedText;
                }
            }

            ////// Get the DataGrid and the VisualContainer
            //var visualContainer = CRM_MASTER_SUB?.GetVisualContainer();

            //if (visualContainer == null) return;

            //// Get the position of the mouse click
            //var position = e.GetPosition(visualContainer);

            //// Get the cell's RowColumnIndex
            //var cellIndex = visualContainer.PointToCellRowColumnIndex(position);
            //if (cellIndex.RowIndex < 0 || cellIndex.ColumnIndex < 0) return;

            //var rowColumnIndex = new RowColumnIndex(cellIndex.RowIndex, cellIndex.ColumnIndex);
            //UpdateCurrentCellValue(rowColumnIndex);

            //// Check if the cell is in edit mode
            //if (CRM_MASTER_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            //{
            //    var editingElement = CRM_MASTER_SUB.FindElementOfType<TextBox>();
            //    if (editingElement != null)
            //    {
            //        CurrentCellValue = editingElement.Text; // Update with the text being edited
            //    }
            //}

        }
        private string GetSelectedText()
        {
            var dataGrid = CRM_MASTER_SUB;
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
    }
}