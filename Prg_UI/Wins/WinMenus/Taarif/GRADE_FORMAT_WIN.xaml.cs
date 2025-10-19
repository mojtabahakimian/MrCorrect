using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_UI.UiTools;
using System.Text;
using Syncfusion.Data;
using Prg_UI.HelperWins;
using System.Windows.Media;
using Prg_Proccessy.SQLMODELS;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;
using Prg_Proccessy.FUNCTIONS;
using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.ComponentModel;
using Syncfusion.PMML;
using Syncfusion.UI.Xaml.Diagram.Utility;
using System.Data;
using System.Runtime.CompilerServices;

namespace Wins.WinMenus.Taarif
{
    public partial class GRADE_FORMAT_WIN : Window, INotifyPropertyChanged
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

        #region SubChildSfDataGridCurrent
        private GRADE_TAB_FT _selectedTabFT;
        /// <summary>
        /// SelectedItem گرید سطح اول (GRADE_TAB_FT) - بصورت خودکار bind می‌شود
        /// </summary>
        public GRADE_TAB_FT SelectedTabFT
        {
            get => _selectedTabFT;
            set
            {
                if (_selectedTabFT == value) return;

                var oldValue = _selectedTabFT;
                _selectedTabFT = value;

                // ✅ بروزرسانی CurrentParentTab
                CurrentParentTab = value;

                // ✅ Raise events
                OnSelectedTabFTChanged(oldValue, value);
                OnPropertyChanged(nameof(SelectedTabFT));
                OnPropertyChanged(nameof(CurrentGFTID)); // اگر property جداگانه دارید
            }
        }
        /// <summary>
        /// دریافت مستقیم GFTID از SelectedTabFT
        /// </summary>
        public int? CurrentGFTID => SelectedTabFT?.GFTID;
        /// <summary>
        /// دریافت نام گروه فعلی
        /// </summary>
        public string CurrentTabName => SelectedTabFT?.GFNAMEFT ?? "انتخاب نشده";
        public class TabFTChangedEventArgs : EventArgs
        {
            public GRADE_TAB_FT OldValue { get; set; }
            public GRADE_TAB_FT NewValue { get; set; }
            public int? OldGFTID { get; set; }
            public int? NewGFTID { get; set; }
        }
        /// <summary>
        /// Event برای اطلاع سایر قسمت‌های کد از تغییر انتخاب
        /// </summary>
        public event EventHandler<TabFTChangedEventArgs> SelectedTabFTChanged;
        protected virtual void OnSelectedTabFTChanged(GRADE_TAB_FT oldValue, GRADE_TAB_FT newValue)
        {
            SelectedTabFTChanged?.Invoke(this, new TabFTChangedEventArgs
            {
                OldValue = oldValue,
                NewValue = newValue,
                OldGFTID = oldValue?.GFTID,
                NewGFTID = newValue?.GFTID
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public bool NowIsReady { get; private set; }

        UniversControl universControl = new UniversControl();

        public Visual WINARG { get; set; }
        public ObservableCollection<GRADE_FORMAT> MASTER_GRADE_DATA { get; set; } = new ObservableCollection<GRADE_FORMAT>();
        public GRADE_FORMAT_WIN(bool selectMode = false, Visual _YOUR_VL_WIN = null)
        {
            InitializeComponent();

            this.DataContext = this;

            WINARG = _YOUR_VL_WIN;

            IsSelectMode = selectMode; // معادل OpenArgs="1"
        }
        /// <summary>
        /// OpenArgs="1" IsReadOnly = IsSelectMode
        /// </summary>
        public bool IsSelectMode { get; set; } = false; // معادل OpenArgs="1"
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (SYNCFUSION_DG.IsKeyboardFocusWithin)
                    {
                        if (SYNCFUSION_DG.CurrentColumn != null)
                        {
                        }
                    }

                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab, true);

                }
            }
            catch { /*ignore*/ }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            ArezeshComboColumn.ItemsSource = dbms.DoGetDataSQL<GSCALE>("SELECT GSCACOD, GSCANAME, GSCAKIND, CRT, UID FROM dbo.GSCALE").ToList();

            ReGetData();

            SYNCFUSION_DG.IsReadOnly = IsSelectMode;
        }

        private void ReGetData()
        {
            MASTER_GRADE_DATA?.Clear();
            // Fetch GRADE_FORMAT data
            // داخل Window_Loaded
            var gradeFormats = dbms.DoGetDataSQL<GRADE_FORMAT>("SELECT IDD, GFNAME, GFDATE, TOZIH, USERNAME, JAMZARIB, EMTIAZ, CRT, UID FROM GRADE_FORMAT").ToList();

            foreach (var format in gradeFormats)
            {
                var tabs = dbms.DoGetDataSQL<GRADE_TAB_FT>(
                    "SELECT GFID, GFTID, GFNAMEFT, GFGZARIB, CRT, UID FROM GRADE_TAB_FT WHERE GFID = @GFID",
                    new { GFID = format.IDD }).ToList();

                format.GRADETABFT = new ObservableCollection<GRADE_TAB_FT>(tabs);

                foreach (var tabFt in format.GRADETABFT)
                {
                    var groups = dbms.DoGetDataSQL<GRADE_GRP_FT>(
                        "SELECT GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE, CRT, UID FROM GRADE_GRP_FT WHERE GFTID = @GFTID",
                        new { GFTID = tabFt.GFTID }).ToList();

                    tabFt.GRADEGRPFT = new ObservableCollection<GRADE_GRP_FT>(groups);
                }
            }
            // Populate the MASTER_GRADE_TOP collection for binding
            MASTER_GRADE_DATA = new ObservableCollection<GRADE_FORMAT>(gradeFormats);
            SYNCFUSION_DG.ItemsSource = MASTER_GRADE_DATA;

        }

        private void SYNCFUSION_DG_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SYNCFUSION_DG.SelectedItem != null && SYNCFUSION_DG.SelectedIndex > -1 && WINARG != null)
            {
                var RowChoosed = SYNCFUSION_DG.SelectedItem as GRADE_FORMAT;

                //(WINARG as AZAE_WIN).MasterData

                string hes = (WINARG as AZAE_WIN).HES.SelectedValue.ToStringNullSafe().Trim();
                var existingRecord = dbms.DoGetDataSQL<GRADE_CUST_TAB>($"SELECT * FROM GRADE_CUST_TAB WHERE GCCUST_HES = '{hes}'").FirstOrDefault();
                if (existingRecord != null)
                {
                    new Msgwin(false, "گريد بندي قبلا انجام شده").ShowDialog();
                }
                else
                {
                    var RSTS = dbms.DoGetDataSQL<GRADE_TAB_FT>("SELECT * FROM GRADE_TAB_FT WHERE GFID = " + RowChoosed.IDD).ToList();
                    for (int i = 0; i < RSTS.Count; i++) //while (!RSTS.EOF)
                    {
                        //rst.AddNew();
                        var GCTAB = (int?)CL_HESABDARI.GetLGradetab();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.GRADE_CUST_TAB(GCTABID, GCNAME, GCZARIB, GCCUST_HES, GCDATE, USERNAME)
                                             VALUES({GCTAB},
                                             N'{RSTS[i].GFNAMEFT}' ,
                                             {RSTS[i].GFGZARIB} ,
                                             N'{Strings.Trim(hes)}' ,
                                             {Tarikh.FullCurrentDate}  ,
                                             N'{(string)CL_HESABDARI.UCurrentUser()}' )");

                        CL_HESABDARI.INSERTGRPGRADE((long)GCTAB, (long)RSTS[i].GFTID);
                        //RSTS.MoveNext();
                    }

                    (WINARG as AZAE_WIN).ReGetData();      //AZAEForm.GRADE_TAB_FORM.Requery();
                    CL_HESABDARI.UpAZAE(hes);
                    this.Close();
                }
            }
        }

        #region _SfDataGrid_

        private readonly FilterService<GRADE_FORMAT> filterService = new FilterService<GRADE_FORMAT>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public int CURRENT_ROW_INDEX { get; private set; }
        public GRADE_FORMAT? CURRENT_ROW_ITEMS { get; private set; }

        private GRADE_FORMAT? wasRowSnapshot;
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
                    filterService.AddFilter(columnName, filterValue, isExclusion: true);
                    // Add the filter to the list of active filters
                    ActiveFilters.Add($"{columnName} != {filterValue}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as GRADE_FORMAT);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(GetSelectedText()))
            {
                var element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
                }
            }
        }
        private T FindChildElement<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                var result = FindChildElement<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;

            if (currentCell != null && currentCell.IsEditing)
            {
                // Find the editing element (which will be a TextBox in edit mode)
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    if (!string.IsNullOrEmpty(editingElement.SelectedText))
                    {
                        return editingElement.SelectedText; // Return the selected text
                    }
                }

                var editingElement2 = FindChildElement<TextBox>(dataGrid);
                if (editingElement2 != null)
                {
                    return editingElement2.SelectedText;
                }

            }
            return string.Empty;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                    return;
                }
            }
            catch { return; }

            // Check if there are selected rows
            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in SYNCFUSION_DG.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in SYNCFUSION_DG.SelectedItems)
                {
                    foreach (var column in SYNCFUSION_DG.Columns)
                    {
                        if (!column.IsHidden) // Include only columns that are not hidden
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{SYNCFUSION_DG.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            // Ensure rows are selected
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0)
            {
                return;
            }

            // Detect the current column
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null)
            {
                return;
            }

            string columnName = currentColumn.MappingName; // Get the column name
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            decimal sum = 0;
            bool isNumericColumn = false;

            // Iterate through the selected rows
            foreach (var selectedItem in _DG_.SelectedItems)
            {
                // Get the cell value for the detected column
                var cellValue = GetCellValue(selectedItem, columnName);

                if (cellValue != null && decimal.TryParse(cellValue.ToStringNullSafe(), out decimal numericValue))
                {
                    sum += numericValue;
                    isNumericColumn = true;
                }
            }

            if (isNumericColumn)
            {
                string formattedSum = sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                new Msgwin(false, $"جمع سطر های انتخاب شده در ستون [{currentColumn.HeaderText}] برار است با : {formattedSum}").ShowDialog();

            }
        }
        private object GetCellValue(object record, string columnName)
        {
            try
            {
                // Use reflection to get the property value from the record
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch
            {
                return null;
            }
        }
        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                SYNCFUSION_DG.TableSummaryRows.Clear();
            }
            else
            {
                // Check if a summary row already exists
                if (_DG_.TableSummaryRows.Count > 0)
                {
                    return; // Exit the method if a summary row already exists
                }
            }

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            var dataType = typeof(GRADE_FORMAT);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(GRADE_FORMAT).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                if (column.MappingName == "BED" || column.MappingName == "BES")
                {
                    if (IsNumericType(propertyInfo.PropertyType))
                    {
                        var summaryColumn = new GridSummaryColumn
                        {
                            Name = column.MappingName + "Sum",
                            MappingName = column.MappingName,
                            SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                            //Format = "{Sum:N0}"
                            Format = "{Sum:N0}"
                        };
                        summaryColumns.Add(summaryColumn);
                    }
                }
            }

            summaryRow.SummaryColumns = summaryColumns;

            _DG_.TableSummaryRows.Add(summaryRow);


        }
        private bool IsNumericType(Type type)
        {
            if (type == null)
                return false;

            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type that might represent a number
            if (type == typeof(object))
            {
                return true; // Assume it might be numeric
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }

        #endregion

        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }

            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    var grid = sender as SfDataGrid;
            //    grid.ScrollInView(new RowColumnIndex(grid.GetLastDataRowIndex(), 0));
            //    grid.SelectionController.MoveCurrentCell(new RowColumnIndex(grid.GetLastDataRowIndex(), 0));
            //}

            //if (e.Key == Key.Delete)
            //{
            //    // Prevent deleting text inside a cell when pressing delete
            //    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
            //    {
            //        return;
            //    }
            //    e.Handled = true; // Handle the key press to prevent default behavior
            //    DeleteSelectedRows();
            //}
        }
        private void BTN_ISEND_Click(object sender, RoutedEventArgs e)
        {

            if (SYNCFUSION_DG.SelectedItem is GRADE_FORMAT CurrentRow)
            {
                if (CurrentRow != null && CurrentRow?.IDD != null)
                {
                    // نمونه با Stimulsoft (در صورت موجود بودن)
                    // var report = new Stimulsoft.Report.StiReport();
                    // report.Load("Reports/GRADE_FORMAT_REP.mrt"); // مسیر نمونه
                    // report.Dictionary.Variables["IDD"].Value = row.IDD.ToString();
                    // report.Show();

                    // یا اگر Helper داخلی داری:
                    // ReportHelper.Show("GRADE_FORMAT_REP", new { IDD = row.IDD });
                }
            }

        }

        private bool BodyIsValid(GRADE_FORMAT _row)
        {
            var ROW = _row;

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (string.IsNullOrWhiteSpace(ROW?.GFNAME.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "عنوان مشخص نشده است" });
            }

            if (string.IsNullOrWhiteSpace(ROW?.GFDATE?.ToString()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ خالی است" });
            }
            else
            {
                if (!Tarikh.IsValidedDate(ROW.GFDATE?.ToString().ToRawTarikh()))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ صحیح نمی باشد" });
                }
                //else
                //{
                //    if (!Tarikh.IsSyncedDateNow(ROW.GFDATE?.ToString(), (bool)Baseknow.CTL_DT))
                //    {
                //        ErrosMessages.Add(new MsgModel { MessageText_U = "تاریخ مربوط به سال جاری نیست" });
                //    }
                //}
            }


            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }
            return true;
        }
        private void CancelGridEdit()
        {
            // Cancel current edit at row level (uses IEditableObject underneath)
            if (SYNCFUSION_DG?.View != null)
                SYNCFUSION_DG.View.CancelEdit();
        }
        private void SYNCFUSION_DG_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            // Snapshot current row before edits (for revert)
            CURRENT_ROW_INDEX = e.RowColumnIndex.RowIndex;
            CURRENT_ROW_ITEMS = SYNCFUSION_DG.CurrentItem as GRADE_FORMAT;
            wasRowSnapshot = (SYNCFUSION_DG.CurrentItem as GRADE_FORMAT)?.Clone() as GRADE_FORMAT;

        }
        private void SYNCFUSION_DG_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            // If user cleared MPP in the ComboBox, revert to previous value and cancel edit
            var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var mapping = SYNCFUSION_DG.Columns[columnIndex].MappingName;

            var row = SYNCFUSION_DG.CurrentItem as GRADE_FORMAT;
            if (row == null) return;

            if (mapping == "MPP")
            {
                //if (row.CRT == null)
                //{
                //    ////// revert MPP to previous snapshot and cancel whole row edit (same as your DataGrid logic)
                //    //if (_wasRowSnapshot != null)
                //    //    row.MPP = _wasRowSnapshot.MPP;

                //    CancelGridEdit();
                //}
            }
        }
        private void SYNCFUSION_DG_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            // Don't validate the new row template or unchanged rows
            if (e.RowData == null) { return; }

            var row = e.RowData as GRADE_FORMAT;
            if (row == null) return;
            if (ConstructorRowDetector.IsPristine(row)) // Assuming IsPristine checks if it's an untouched new row
            {
                e.IsValid = false; // Prevents adding an empty row
                return;
            }

            if (!BodyIsValid(row))
            {
                e.IsValid = false; // keep the user in edit until valid
            }

        }
        private void SYNCFUSION_DG_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var row = e.RowData as GRADE_FORMAT;
            if (row == null) return;

            int? newId = null;

            try
            {
                if (row.IDD is null) // INSERT
                {
                    var maxId = dbms.DoGetDataSQL<int?>("SELECT MAX(IDD) FROM dbo.GRADE_FORMAT").FirstOrDefault() ?? 0;
                    var NewMaxed = maxId + 1;

                    dbms.DoExecuteSQL(@"
                            INSERT INTO dbo.GRADE_FORMAT (IDD, GFNAME, GFDATE, TOZIH, USERNAME, JAMZARIB, EMTIAZ)
                            VALUES (@IDD, @GFNAME, @GFDATE, @TOZIH, @USERNAME, @JAMZARIB, @EMTIAZ)",
                        new
                        {
                            IDD = NewMaxed,
                            GFNAME = row.GFNAME,
                            GFDATE = row.GFDATE,
                            TOZIH = row.TOZIH,
                            USERNAME = Baseknow.UUSER,
                            JAMZARIB = row.JAMZARIB,
                            EMTIAZ = row.EMTIAZ
                        });

                    newId = NewMaxed;
                }
                else // UPDATE
                {
                    dbms.DoExecuteSQL(@"
                        UPDATE dbo.GRADE_FORMAT
                           SET GFNAME = @GFNAME,
                               GFDATE = @GFDATE,
                               TOZIH = @TOZIH,
                               USERNAME = @USERNAME,
                               JAMZARIB = @JAMZARIB,
                               EMTIAZ = @EMTIAZ
                         WHERE IDD = @IDD",
                         new
                         {
                             IDD = row.IDD,
                             GFNAME = row.GFNAME,
                             GFDATE = row.GFDATE,
                             TOZIH = row.TOZIH,
                             USERNAME = row.USERNAME,
                             JAMZARIB = row.JAMZARIB,
                             EMTIAZ = row.EMTIAZ
                         });
                }
            }
            catch (SqlException ex)
            {
                CancelGridEdit();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این داده تکراری است !").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام عملیات ذخیره!").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات!").ShowDialog();
                return;
            }

            if (newId != null)
                row.IDD = newId;
        }
        private void SYNCFUSION_DG_RecordDeleting(object sender, RecordDeletingEventArgs e)
        {
            if (SYNCFUSION_DG.SelectedItems == null || SYNCFUSION_DG.SelectedItems.Count == 0) return;

            // Confirm delete
            var confirm = new Msgwin(true, "آیا مایل به حذف هستید ؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true)
            {
                e.Cancel = true;
                return;
            }

            bool anyError = false;
            List<MsgModel> errors = new List<MsgModel>();

            // Audit once for the overall delete action
            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف فرمت گرید بندی مشتریان",
                    recordId: string.Join(",", e.Items.Select(i => (i as GRADE_FORMAT)?.IDD?.ToStringNullSafe())),
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var obj in e.Items.ToList())
            {
                var item = obj as GRADE_FORMAT;
                if (item?.IDD == null) continue;

                try
                {
                    dbms.DoExecuteSQL($@"DELETE FROM dbo.GRADE_FORMAT WHERE IDD = {item.IDD}");
                }
                catch (SqlException ex)
                {
                    anyError = true;
                    if (ex.Number == 547) // FK constraint
                        errors.Add(new MsgModel { MessageText_U = "این کد دارای گردش است و نمیتوان آنرا پاک کرد!" });
                    else
                        errors.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروزرسانی پایگاه داده انجام نشد!" });
                }
                catch (Exception)
                {
                    anyError = true;
                    errors.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                }
            }

            if (anyError)
            {
                e.Cancel = true;
                if (errors.Any())
                {
                    errors = errors.Select(x => x.MessageText_U).Distinct()
                                   .Select(m => new MsgModel { MessageText_U = m }).ToList();
                    new MsgListwin(false, errors).ShowDialog();
                }
            }
        }
        private void SYNCFUSION_DG_RecordDeleted(object sender, RecordDeletedEventArgs e)
        {
            // Keep DB & UI in sync
            //ReGetData();
        }

        #region SubLevel1
        private GRADE_TAB_FT _d1Snapshot;
        private void TABFT_CurrentCellBeginEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs e)
        {
            var grid = (SfDataGrid)sender;
            _d1Snapshot = grid.CurrentItem as GRADE_TAB_FT;
            _d1Snapshot = _d1Snapshot?.Clone() as GRADE_TAB_FT;
        }

        private bool BodyIsValid_D1(GRADE_TAB_FT row)
        {
            var errs = new List<MsgModel>();
            if (row == null) return false;

            if (string.IsNullOrWhiteSpace(row.GFNAMEFT))
                errs.Add(new MsgModel { MessageText_U = "عنوان مشخص نشده است" });

            if (errs.Any())
            {
                errs = errs.Select(x => x.MessageText_U).Distinct().Select(m => new MsgModel { MessageText_U = m }).ToList();
                new MsgListwin(false, errs).ShowDialog();
                return false;
            }
            return true;
        }

        private void TABFT_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            var row = e.RowData as GRADE_TAB_FT;
            if (row == null) return;

            // اگر ردیف خالی/دست‌نخورده است، اجازه اضافه‌کردن نده
            if (ConstructorRowDetector.IsPristine(row))
            {
                e.IsValid = false;
                return;
            }

            e.IsValid = BodyIsValid_D1(row);
        }

        private void TABFT_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var row = e.RowData as GRADE_TAB_FT;
            if (row == null) return;

            bool isNewRecord = false;
            try
            {
                // تضمین GFID (کلید مستر): اگر خالی بود، از مستر انتخاب‌شده بگیر
                if (row.GFID == null)
                    row.GFID = (SYNCFUSION_DG.SelectedItem as GRADE_FORMAT)?.IDD;

                // اگر GFTID ندارد ⇒ MAX+1 مثل VBA
                if (row.GFTID == null)
                {
                    isNewRecord = true;

                    var maxId = dbms.DoGetDataSQL<int?>("SELECT ISNULL(MAX(GFTID),0) FROM dbo.GRADE_TAB_FT").FirstOrDefault() ?? 0;
                    row.GFTID = maxId + 1;

                    dbms.DoExecuteSQL(@"
                    INSERT INTO dbo.GRADE_TAB_FT (GFID, GFTID, GFNAMEFT, GFGZARIB)
                    VALUES (@GFID, @GFTID, @GFNAMEFT, @GFGZARIB)",
                            new { row.GFID, row.GFTID, row.GFNAMEFT, row.GFGZARIB });
                }
                else
                {
                    dbms.DoExecuteSQL(@"
                        UPDATE dbo.GRADE_TAB_FT
                           SET GFID = @GFID,
                               GFNAMEFT = @GFNAMEFT,
                               GFGZARIB = @GFGZARIB
                         WHERE GFTID = @GFTID",
                             new { row.GFID, row.GFTID, row.GFNAMEFT, row.GFGZARIB });
                }

                // 🔥 CRITICAL FIX: Reload the parent's detail collection after INSERT
                if (false) //isNewRecord
                {
                    var parentFormat = MASTER_GRADE_DATA.FirstOrDefault(m => m.IDD == row.GFID);
                    if (parentFormat != null)
                    {
                        // Option 1: Reload from DB (safer, always in sync)
                        var result = dbms.DoGetDataSQL<GRADE_TAB_FT>(
                            $"SELECT GFID, GFTID, GFNAMEFT, GFGZARIB, CRT, UID FROM GRADE_TAB_FT WHERE GFID = {row.GFID}"
                        ).ToList();

                        parentFormat.GRADETABFT = new ObservableCollection<GRADE_TAB_FT>(result);

                        // Reload nested GRADE_GRP_FT for each GRADE_TAB_FT
                        foreach (var tabFt in parentFormat.GRADETABFT)
                        {
                            var groups = dbms.DoGetDataSQL<GRADE_GRP_FT>(
                                $"SELECT GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE, CRT, UID FROM GRADE_GRP_FT WHERE GFTID = {tabFt.GFTID}"
                            ).ToList();

                            tabFt.GRADEGRPFT = new ObservableCollection<GRADE_GRP_FT>(groups);
                        }

                        // Trigger PropertyChanged for the collection
                        parentFormat.OnPropertyChanged(nameof(parentFormat.GRADETABFT));
                    }
                }

                // معادل Form_AfterUpdate:
                Upformat(row.GFID);

            }
            catch (SqlException ex)
            {
                // رول‌بک UI
                var grid = (SfDataGrid)sender;
                grid.View?.CancelEdit();

                if (ex.Number == 2601 || ex.Number == 2627)
                    new Msgwin(false, "این داده تکراری است !").ShowDialog();
                else
                    new Msgwin(false, "خطا در ذخیره‌ی جزئیات!").ShowDialog();
            }
            catch (Exception)
            {
                new Msgwin(false, "خطای ناشناخته در جزئیات!").ShowDialog();
            }
        }

        private void TABFT_RecordDeleting(object sender, RecordDeletingEventArgs e)
        {
            if (e.Items == null || !e.Items.Any()) return;

            var confirm = new Msgwin(true, "آیا از حذف مطئن هستید ؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true)
            {
                e.Cancel = true;
                return;
            }

            bool anyError = false;
            var errors = new List<MsgModel>();

            foreach (var obj in e.Items.ToList())
            {
                var item = obj as GRADE_TAB_FT;
                if (item?.GFTID == null) continue;

                try
                {
                    dbms.DoExecuteSQL("DELETE FROM dbo.GRADE_TAB_FT WHERE GFTID=@GFTID", new { item.GFTID });
                }
                catch (SqlException ex)
                {
                    anyError = true;
                    if (ex.Number == 547)
                        errors.Add(new MsgModel { MessageText_U = "این گروه دارای ردیف‌های زیرگروه است و قابل حذف نیست!" });
                    else
                        errors.Add(new MsgModel { MessageText_U = "حذف به دلیل خطای پایگاه‌داده انجام نشد!" });
                }
                catch
                {
                    anyError = true;
                    errors.Add(new MsgModel { MessageText_U = "خطا در عملیات حذف!" });
                }
            }

            if (anyError)
            {
                e.Cancel = true;
                if (errors.Any())
                {
                    errors = errors.Select(x => x.MessageText_U).Distinct().Select(m => new MsgModel { MessageText_U = m }).ToList();
                    new MsgListwin(false, errors).ShowDialog();
                }
            }
        }
        #endregion

        #region SubLevel2
        private GRADE_GRP_FT _d2Snapshot;
        private void GRPFT_CurrentCellBeginEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs e)
        {
            var grid = (SfDataGrid)sender;
            //_d2Snapshot = grid.CurrentItem as GRADE_GRP_FT;
            //_d2Snapshot = _d2Snapshot?.Clone() as GRADE_GRP_FT;
            var childGrid = grid; // ✅ پراپرتی صحیح
            if (childGrid == null) return;
        }
        private bool BodyIsValid_D2(GRADE_GRP_FT row)
        {
            var errs = new List<MsgModel>();
            if (row == null) return false;

            if (string.IsNullOrWhiteSpace(row.GFGRPNAMEFT))
                errs.Add(new MsgModel { MessageText_U = "عنوان سنجش مشخص نشده است" });

            if (errs.Any())
            {
                errs = errs.Select(x => x.MessageText_U).Distinct().Select(m => new MsgModel { MessageText_U = m }).ToList();
                new MsgListwin(false, errs).ShowDialog();
                return false;
            }
            return true;
        }
        private void GRPFT_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) return;
            var row = e.RowData as GRADE_GRP_FT;
            if (row == null) return;

            if (ConstructorRowDetector.IsPristine(row))
            {
                e.IsValid = false;
                return;
            }

            e.IsValid = BodyIsValid_D2(row);
        }

        GRADE_TAB_FT? CurrentParentTab = null;

        private Dictionary<SfDataGrid, GRADE_TAB_FT> GridParentMap = new();
        private void GRPFT_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var row = e.RowData as GRADE_GRP_FT;
            if (row == null) return;

            var detailsViewGrid = sender as SfDataGrid;
            if (detailsViewGrid == null) return;

            GRADE_TAB_FT parentTabFt = null;
            int? parentGFTID = SelectedTabFT?.GFTID;

            try
            {
                var childGrid = sender as SfDataGrid;

                if (parentGFTID == null)
                {
                    // ✅ METHOD 1: Use DetailsViewManager to find parent record
                    var parentGrid = detailsViewGrid.NotifyListener?.SourceDataGrid;
                    if (parentGrid != null)
                    {
                        // Get the DetailsViewDefinition that contains this grid
                        var detailsViewDefinition = parentGrid.DetailsViewDefinition.FirstOrDefault(dvd => dvd.RelationalColumn == "GRADEGRPFT");

                        if (detailsViewDefinition != null)
                        {
                            // Find which parent row contains this details grid
                            foreach (var record in parentGrid.View.Records)
                            {
                                var recordData = record.Data as GRADE_TAB_FT;
                                if (recordData?.GRADEGRPFT != null)
                                {
                                    // Check if this parent's collection contains our row
                                    if (recordData.GRADEGRPFT.Contains(row) ||
                                        recordData.GRADEGRPFT.Any(g => g.GFGRPNAMEFT == row.GFGRPNAMEFT && g.GFTGRPID == null))
                                    {
                                        parentTabFt = recordData;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // ✅ METHOD 2: Search through master data structure
                    if (parentTabFt == null)
                    {
                        var masterFormat = SYNCFUSION_DG.SelectedItem as GRADE_FORMAT;

                        if (masterFormat?.GRADETABFT != null)
                        {
                            // For NEW records, search by object reference or matching properties
                            if (row.GFTGRPID == null)
                            {
                                parentTabFt = masterFormat.GRADETABFT.FirstOrDefault(tab => tab.GRADEGRPFT?.Contains(row) == true);

                                // If not found by reference, try to find by matching collection
                                if (parentTabFt == null)
                                {
                                    // Find which GRADE_TAB_FT was recently expanded/edited
                                    parentTabFt = masterFormat.GRADETABFT.FirstOrDefault(tab => tab.GRADEGRPFT?.Any(g =>
                                            g.GFGRPNAMEFT == row.GFGRPNAMEFT &&
                                            g.GFTGRPID == null) == true);
                                }
                            }
                            else
                            {
                                // For EXISTING records, search by GFTGRPID
                                parentTabFt = masterFormat.GRADETABFT.FirstOrDefault(tab => tab.GRADEGRPFT?.Any(g =>
                                        g.GFTGRPID == row.GFTGRPID) == true);
                            }
                        }
                    }

                    //var parentTabFt2 = detailsViewGrid.Tag as GRADE_TAB_FT;
                }

                // Assign parent FK to child
                row.GFTID = parentGFTID;

                //CurrentParentTab = null; //Reset to avoid conflict

                var TopParentRow = (SYNCFUSION_DG.SelectedItem as GRADE_FORMAT); //GFID

                bool isNewRecord = row.GFTGRPID == null;
                if (isNewRecord)
                {
                    // Generate new GFTGRPID
                    var maxId = dbms.DoGetDataSQL<int?>("SELECT ISNULL(MAX(GFTGRPID), 0) FROM dbo.GRADE_GRP_FT").FirstOrDefault() ?? 0;
                    row.GFTGRPID = maxId + 1;

                    // INSERT new record
                    dbms.DoExecuteSQL(@"
                INSERT INTO dbo.GRADE_GRP_FT (GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE, CRT, UID)
                VALUES (@GFTID, @GFTGRPID, @GFGRPNAMEFT, @GFGRPZARIB, @GFGRPGRADE, @GVALUESCALE, GETDATE(), @UID)",
                        new
                        {
                            row.GFTID,
                            row.GFTGRPID,
                            row.GFGRPNAMEFT,
                            row.GFGRPZARIB,
                            row.GFGRPGRADE,
                            row.GVALUESCALE,
                            UID = Baseknow.USERCOD // Replace with your user ID retrieval
                        });


                    //// 🔄 Reload child collection to sync IDs
                    //var parentFormat = MASTER_GRADE_DATA.FirstOrDefault(m => m.IDD == TopParentRow.IDD);
                    //if (parentTabFt != null)
                    //{
                    //    var reloadedGroups = dbms.DoGetDataSQL<GRADE_GRP_FT>(
                    //        "SELECT GFTID, GFTGRPID, GFGRPNAMEFT, GFGRPZARIB, GFGRPGRADE, GVALUESCALE, CRT, UID FROM GRADE_GRP_FT WHERE GFTID = @GFTID",
                    //        new { GFTID = parentGFTID }).ToList();

                    //    parentTabFt.GRADEGRPFT.Clear();
                    //    foreach (var grp in reloadedGroups)
                    //    {
                    //        parentTabFt.GRADEGRPFT.Add(grp);
                    //    }
                    //    parentTabFt.OnPropertyChanged(nameof(parentTabFt.GRADEGRPFT));
                    //}
                }
                else
                {
                    // UPDATE existing record
                    dbms.DoExecuteSQL(@"
                UPDATE dbo.GRADE_GRP_FT
                SET GFTID = @GFTID,
                    GFGRPNAMEFT = @GFGRPNAMEFT,
                    GFGRPZARIB = @GFGRPZARIB,
                    GFGRPGRADE = @GFGRPGRADE,
                    GVALUESCALE = @GVALUESCALE
                WHERE GFTGRPID = @GFTGRPID",
                        new
                        {
                            row.GFTID,
                            row.GFTGRPID,
                            row.GFGRPNAMEFT,
                            row.GFGRPZARIB,
                            row.GFGRPGRADE,
                            row.GVALUESCALE
                        });
                }


                // Update parent totals
                if (TopParentRow?.IDD != null)
                {
                    Upformat(TopParentRow.IDD);
                }

                // Log action
                _ = AuditLogger.LogActionAsync(
                    isNewRecord ? "INSERT" : "UPDATE",
                    "GRADE_GRP_FT",
                    row.GFTGRPID?.ToString(),
                    CL_HESABDARI.UCurrentUser()?.ToString()
                );
            }
            catch (SqlException ex)
            {
                detailsViewGrid?.View?.CancelEdit();

                if (ex.Number == 2601 || ex.Number == 2627)
                    new Msgwin(false, "این داده تکراری است!").ShowDialog();
                else if (ex.Number == 547) // FK violation
                    new Msgwin(false, "خطا: ارجاع به والد نامعتبر است!").ShowDialog();
                else
                    new Msgwin(false, $"خطا در ذخیره").ShowDialog();
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطای ناشناخته").ShowDialog();
            }
        }
        private void GRPFT_RecordDeleting(object sender, RecordDeletingEventArgs e)
        {
            if (e.Items == null || !e.Items.Any()) return;

            var confirm = new Msgwin(true, "آیا از حذف مطمئن هستید ؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true)
            {
                e.Cancel = true;
                return;
            }

            bool anyError = false;
            var errors = new List<MsgModel>();

            foreach (var obj in e.Items.ToList())
            {
                var item = obj as GRADE_GRP_FT;
                if (item?.GFTGRPID == null) continue;

                try
                {
                    dbms.DoExecuteSQL("DELETE FROM dbo.GRADE_GRP_FT WHERE GFTGRPID=@GFTGRPID", new { item.GFTGRPID });

                    if (CurrentParentTab?.GFTID == item?.GFTID)
                    {
                        CurrentParentTab = null; //Reset
                    }
                }
                catch (SqlException ex)
                {
                    anyError = true;
                    if (ex.Number == 547)
                        errors.Add(new MsgModel { MessageText_U = "این ردیف دارای وابستگی است و قابل حذف نیست!" });
                    else
                        errors.Add(new MsgModel { MessageText_U = "حذف به دلیل خطای پایگاه‌داده انجام نشد!" });
                }
                catch
                {
                    anyError = true;
                    errors.Add(new MsgModel { MessageText_U = "خطا در عملیات حذف!" });
                }
            }

            if (anyError)
            {
                e.Cancel = true;
                if (errors.Any())
                {
                    errors = errors.Select(x => x.MessageText_U).Distinct().Select(m => new MsgModel { MessageText_U = m }).ToList();
                    new MsgListwin(false, errors).ShowDialog();
                }
            }
        }
        #endregion

        private void DG_GRADEGRPFT_SUB2_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var childGrid = sender as SfDataGrid;
            if (childGrid == null) return;

            var grid = sender as Syncfusion.UI.Xaml.Grid.SfDataGrid;

            // ترجیح: AddedItems چون دقیق‌تر است
            var selected = e?.AddedItems?.OfType<GRADE_TAB_FT>().FirstOrDefault()
                           ?? grid?.SelectedItem as GRADE_TAB_FT
                           ?? grid?.CurrentItem as GRADE_TAB_FT;

            CurrentParentTab = selected;

            // اگر انتخاب خالی شد یا آیتم قبلی از انتخاب خارج شد
            if (CurrentParentTab == null ||
                (e?.RemovedItems?.OfType<GRADE_TAB_FT>().Any() == true && grid?.SelectedItem == null))
            {
                CurrentParentTab = null;
            }


            // ✅ METHOD 1: استفاده از Tag (بهترین روش - اگر از DetailsViewLoading ست شده)
            var parentFromTag = childGrid.Tag as GRADE_TAB_FT;
            if (parentFromTag != null)
            {
                CurrentParentTab = parentFromTag;
                return;
            }

            // ✅ METHOD 2: پیدا کردن از طریق NotifyListener (روش Syncfusion)
            var parentGrid = childGrid.NotifyListener?.SourceDataGrid as SfDataGrid;
            if (parentGrid != null)
            {
                var selectedParent = parentGrid.SelectedItem as GRADE_TAB_FT;
                if (selectedParent != null)
                {
                    CurrentParentTab = selectedParent;
                    childGrid.Tag = selectedParent; // ✅ ذخیره برای دفعات بعد
                    return;
                }

                // اگر SelectedItem خالی بود، از CurrentItem استفاده کن
                var currentParent = parentGrid.CurrentItem as GRADE_TAB_FT;
                if (currentParent != null)
                {
                    CurrentParentTab = currentParent;
                    childGrid.Tag = currentParent;
                    return;
                }
            }

            // ✅ METHOD 3: جستجوی دستی در ساختار Master-Detail
            var masterFormat = SYNCFUSION_DG.SelectedItem as GRADE_FORMAT;
            if (masterFormat?.GRADETABFT != null)
            {
                // پیدا کردن والد از طریق SourceCollection
                var childSource = childGrid.ItemsSource ?? childGrid.View?.SourceCollection;

                foreach (var tab in masterFormat.GRADETABFT)
                {
                    // مقایسه رفرنسی
                    if (ReferenceEquals(tab.GRADEGRPFT, childSource))
                    {
                        CurrentParentTab = tab;
                        childGrid.Tag = tab; // ✅ کش کردن
                        return;
                    }
                }

                // اگر هنوز پیدا نشد، از اولین DetailsView باز استفاده کن
                var expandedTab = masterFormat.GRADETABFT
                    .FirstOrDefault(t => t.GRADEGRPFT?.Count > 0);

                if (expandedTab != null)
                {
                    CurrentParentTab = expandedTab;
                    childGrid.Tag = expandedTab;
                }
            }
        }

        private void DG_GRADETABFT_SUB1_DetailsViewExpanded(object sender, GridDetailsViewExpandedEventArgs e)
        {
            var childGrid = e.Record as GRADE_TAB_FT; // ✅ پراپرتی صحیح
            if (childGrid == null) return;

            CurrentParentTab = childGrid;
        }

        private void DG_GRADETABFT_SUB1_DetailsViewLoading(object sender, DetailsViewLoadingAndUnloadingEventArgs e)
        {
            var childGrid = e.DetailsViewDataGrid as SfDataGrid;
            if (childGrid == null) return;

            //// تلاش 1: از گرید والد
            //var parentGrid = childGrid.GetParentDataGrid();
            //GRADE_TAB_FT parentTab = parentGrid?.CurrentItem as GRADE_TAB_FT ?? parentGrid?.SelectedItem as GRADE_TAB_FT;

            // ✅ 1. تنظیم DataContext
            childGrid.DataContext = this;

            // ✅ 2. پیدا کردن والد
            GRADE_TAB_FT parentTab = FindParentTabFT(childGrid);

            // ✅ 3. ذخیره در Tag
            childGrid.Tag = parentTab;

            // ✅ 4. مقداردهی اولیه Property
            if (parentTab != null)
                SelectedTabFT = parentTab;

            // ✅ 5. ثبت Event SelectionChanged
            childGrid.SelectionChanged -= DG_GRADETABFT_SUB1_SelectionChanged;
            childGrid.SelectionChanged += DG_GRADETABFT_SUB1_SelectionChanged;
        }

        private void DG_GRADETABFT_SUB1_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var childGrid = sender as SfDataGrid;
            if (childGrid == null) return;

            // ✅ روش 1: از SelectedItem خود گرید
            if (childGrid.SelectedItem is GRADE_TAB_FT selected)
            {
                SelectedTabFT = selected;
                return;
            }

            // ✅ روش 2: از Tag
            if (childGrid.Tag is GRADE_TAB_FT tagged)
            {
                SelectedTabFT = tagged;
                return;
            }

            // ✅ روش 3: جستجوی دستی
            var found = FindParentTabFT(childGrid);
            if (found != null)
                SelectedTabFT = found;
        }

        /// <summary>
        /// پیدا کردن GRADE_TAB_FT والد از DetailsView Grid
        /// </summary>
        private GRADE_TAB_FT FindParentTabFT(SfDataGrid detailsGrid)
        {
            if (detailsGrid == null) return null;

            // روش 1: از NotifyListener
            var parentGrid = detailsGrid.NotifyListener?.SourceDataGrid as SfDataGrid;
            if (parentGrid != null)
            {
                var parent = parentGrid.SelectedItem as GRADE_TAB_FT ?? parentGrid.CurrentItem as GRADE_TAB_FT;
                if (parent != null) return parent;
            }

            // روش 2: از ItemsSource
            var source = detailsGrid.ItemsSource ?? detailsGrid.View?.SourceCollection;
            if (source != null)
            {
                foreach (var format in MASTER_GRADE_DATA)
                {
                    if (format.GRADETABFT == null) continue;

                    var match = format.GRADETABFT.FirstOrDefault(t => ReferenceEquals(t.GRADEGRPFT, source));
                    if (match != null) return match;
                }
            }

            return null;
        }


        private void Upformat(int? gfid)
        {
            if (gfid == null) return;

            var Result = CL_HESABDARI.Upformat(gfid);

            // همگام‌سازی UI
            var master = MASTER_GRADE_DATA.FirstOrDefault(m => m.IDD == gfid);
            if (master != null)
            {
                master.JAMZARIB = (float?)Result.JAMZARIB ?? 0;
                master.EMTIAZ = (float)Result.EMTIAZ;
            }

            // اگر لازم است از دیتابیس دیتیل‌ها را دوباره بخوانی:
            // master.GRADETABFT = dbms.DoGetDataSQL<GRADE_TAB_FT>($"SELECT ... WHERE GFID={gfid}").ToList();
            //SYNCFUSION_DG?.View?.Refresh();
        }

    }
}