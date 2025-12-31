using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.TR
{
    public partial class TR_STUF_DEF_WIN : Window
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

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public ObservableCollection<STUF_DEF> PRODUCT_DATA { get; set; } = new ObservableCollection<STUF_DEF>();
        public bool NowIsReady { get; private set; }

        public TR_STUF_DEF_WIN()
        {
            InitializeComponent();
            this.DataContext = this;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecurityCheck
            try
            {
                var helper = new WindowInteropHelper(this);
                helper.EnsureHandle();
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "TR_STU", helper.Handle, this.GetType().Name);
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { try { this.Close(); } catch { } }
            #endregion

            FILL_ALL_COMBOBOXES();
            ReGetHeadMaster();
            GenerateAutomaticSummary(SYNCFUSION_DG);

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();
                UpdateRowCountLabel();
            }

            SYNCFUSION_DG.Visibility = Visibility.Visible;
            SetupGridNavigation();
            AttachRecordCountUpdater(SF_FSK, TXT_COUNT_FSK);
            AttachRecordCountUpdater(SF_MODULE_D, TXT_COUNT_MODULE_D);
            AttachRecordCountUpdater(SF_TAKHPERS, TXT_COUNT_TAKHPERS);
            AttachRecordCountUpdater(SF_REWARDS, TXT_COUNT_REWARDS);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            var RST_VAHEDS = dbms.DoGetDataSQL<Custom_VAHEDK>("SELECT CODE AS VAHED,NAMES FROM dbo.TCOD_VAHEDS").ToList();
            VAHED_COLUMN.ItemsSource = RST_VAHEDS;
            VAHED_COLUMN_DETAIL.ItemsSource = RST_VAHEDS;

            RADAH_COLUMN.ItemsSource = dbms.DoGetDataSQL<TCOD_STUFGROUP>("SELECT CODE, NAMES FROM TCOD_STUFGROUP").ToList();
            ANBAR_COLUMN_DETAIL.ItemsSource = dbms.DoGetDataSQL<Custom_TCODANBAR>($"SELECT TCOD_ANBAR.CODE, TCOD_ANBAR.NAMES FROM dbo.TCOD_ANBAR").ToList();
            CUST_CO_COLUMN.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM dbo.CUSTKIND").ToList();
        }

        private void ReGetHeadMaster()
        {
            PRODUCT_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<STUF_DEF>(@"SELECT * FROM dbo.TR_STUF_DEF").ToList();

            foreach (var item in MasterHead)
            {
                PRODUCT_DATA?.Add(item);
            }
        }

        public ObservableCollection<STUF_FSK> FSK_DATA { get; set; } = new ObservableCollection<STUF_FSK>();
        public ObservableCollection<MODULE_D> MODULE_D_DATA { get; set; } = new ObservableCollection<MODULE_D>();
        public ObservableCollection<TAKHPERS> TAKHPERS_DATA { get; set; } = new ObservableCollection<TAKHPERS>();
        public ObservableCollection<RewardRules> REWARDS_DATA { get; set; } = new ObservableCollection<RewardRules>();

        public class ProductFullDetails
        {
            public STUF_DEF Header { get; set; }
            public List<STUF_FSK> FskItems { get; set; } = new List<STUF_FSK>();
            public List<MODULE_D> ModuleDItems { get; set; } = new List<MODULE_D>();
            public List<TAKHPERS> TakhpersItems { get; set; } = new List<TAKHPERS>();
            public List<RewardRules> RewardItems { get; set; } = new List<RewardRules>();
        }

        private async Task<ProductFullDetails> GetProductFullDetailsAsync(STUF_DEF TrRow)
        {
            var fullDetails = new ProductFullDetails();
            const int ROUND_PRECISION = 6;

            string sql = $@"
                -- 1. Header
                SELECT * FROM dbo.TR_STUF_DEF 
                WHERE CODE = @Code 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 2. STUF_FSK
                SELECT * FROM dbo.TR_STUF_FSK 
                WHERE CODE = @Code 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 3. MODULE_D
                SELECT * FROM dbo.TR_MODULE_D 
                WHERE CODE = @Code 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 4. TAKHPERS
                SELECT * FROM dbo.TR_TAKHPERS 
                WHERE TAKH_COD = @Code 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 5. RewardRules
                SELECT * FROM dbo.TR_RewardRules 
                WHERE ProductID_Target = @Code 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});
                ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
            var parameters = new
            {
                Code = TrRow.CODE,
                UpDate = TrRow.UP_DATE,
                UpTime = TrRow.UP_TIME ?? 0
            };

            using (var multi = await db.QueryMultipleAsync(sql, parameters))
            {
                fullDetails.Header = await multi.ReadFirstOrDefaultAsync<STUF_DEF>();
                if (fullDetails.Header == null) return null;

                fullDetails.FskItems = (await multi.ReadAsync<STUF_FSK>()).ToList();
                fullDetails.ModuleDItems = (await multi.ReadAsync<MODULE_D>()).ToList();
                fullDetails.TakhpersItems = (await multi.ReadAsync<TAKHPERS>()).ToList();
                fullDetails.RewardItems = (await multi.ReadAsync<RewardRules>()).ToList();
            }

            return fullDetails;
        }

        private async void ReGetData(STUF_DEF TrRow)
        {
            if (TrRow == null) return;

            var dataFetchTask = GetProductFullDetailsAsync(TrRow);
            var delayTask = Task.Delay(300);
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);
            bool loaderShown = false;

            if (completedTask == delayTask)
            {
                BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                ProductFullDetails fullDetails = await dataFetchTask;

                if (fullDetails == null || fullDetails.Header == null)
                {
                    if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                FSK_DATA.Clear();
                fullDetails.FskItems.ForEach(FSK_DATA.Add);

                MODULE_D_DATA.Clear();
                fullDetails.ModuleDItems.ForEach(MODULE_D_DATA.Add);

                TAKHPERS_DATA.Clear();
                fullDetails.TakhpersItems.ForEach(TAKHPERS_DATA.Add);

                REWARDS_DATA.Clear();
                fullDetails.RewardItems.ForEach(REWARDS_DATA.Add);

                GenerateAutomaticSummary(SF_FSK);
            }
            catch (Exception ex)
            {
                new Msgwin(false, $"خطا در بارگذاری اطلاعات").ShowDialog();
            }
            finally
            {
                if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }

        #region Standard Methods
        private void View_FilterChanged(object sender, GridFilterEventArgs e) => UpdateRowCountLabel();
        private void UpdateRowCountLabel() { /* Logic handled by AttachRecordCountUpdater */ }

        private readonly FilterService<STUF_DEF> filterService = new FilterService<STUF_DEF>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return;
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }

        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (!NowIsReady) return;

            if (SYNCFUSION_DG.SelectedItem is STUF_DEF SelectedProduct)
            {
                if (SelectedProduct?.CODE != null)
                {
                    ReGetData(SelectedProduct);
                }
                else
                {
                    FSK_DATA.Clear();
                    MODULE_D_DATA.Clear();
                    TAKHPERS_DATA.Clear();
                    REWARDS_DATA.Clear();
                }
            }
        }

        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex;
            CurrentCellValue = null;

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0) return;

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName;
            if (string.IsNullOrEmpty(mappingName)) return;

            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
            if (record == null) return;

            CurrentCellValue = record?.GetType()?.GetProperty(mappingName)?.GetValue(record)?.ToString();
        }

        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController?.CurrentCellManager?.CurrentCell;
            if (currentCell == null) return string.Empty;

            if (currentCell.IsEditing)
            {
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null && !string.IsNullOrEmpty(editingElement.SelectedText))
                    return editingElement.SelectedText;
            }

            try
            {
                var gridCellElement = currentCell?.ColumnElement;
                if (gridCellElement != null)
                {
                    var textBox = FindVisualChild<TextBox>(gridCellElement);
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.SelectedText))
                        return textBox.SelectedText;
                }
            }
            catch { }

            return string.Empty;
        }

        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
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
                    universControl.PopNotifyShow("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }
            }
            catch { return; }

            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();
            try
            {
                foreach (var column in SYNCFUSION_DG.Columns)
                {
                    if (!column.IsHidden)
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                foreach (var item in SYNCFUSION_DG.SelectedItems)
                {
                    foreach (var column in SYNCFUSION_DG.Columns)
                    {
                        if (!column.IsHidden)
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{SYNCFUSION_DG.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }
        }

        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true;
            }
        }

        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0) return;
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null) return;
            string columnName = currentColumn.MappingName;
            if (string.IsNullOrEmpty(columnName)) return;

            decimal sum = 0;
            bool isNumericColumn = false;

            foreach (var selectedItem in _DG_.SelectedItems)
            {
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
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch { return null; }
        }

        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_) _DG_.TableSummaryRows.Clear();
            else if (_DG_.TableSummaryRows.Count > 0) return;

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;
            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(STUF_DEF).GetProperty(column.MappingName);
                if (propertyInfo == null) continue;

                if (IsNumericType(propertyInfo.PropertyType))
                {
                    var summaryColumn = new GridSummaryColumn
                    {
                        Name = column.MappingName + "Sum",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                        Format = "{Sum:N0}"
                    };
                    summaryColumns.Add(summaryColumn);
                }
            }
            summaryRow.SummaryColumns = summaryColumns;
            _DG_.TableSummaryRows.Add(summaryRow);
        }

        private bool IsNumericType(Type type)
        {
            if (type == null) return false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            if (type == typeof(object)) return true;

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
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedProductHistory");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }

        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            void UpdateLabel()
            {
                int count = 0;
                if (dataGrid.ItemsSource is ICollection collection) count = collection.Count;
                else if (dataGrid.View != null && dataGrid.View.Records != null) count = dataGrid.View.Records.Count;

                Dispatcher.Invoke(() => targetTextBlock.Text = count.ToString("N0"));
            }

            void SubscribeToCollection(object source)
            {
                if (source is INotifyCollectionChanged notifyingCollection)
                    notifyingCollection.CollectionChanged += (s, e) => UpdateLabel();
            }

            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                if (e.NewItemsSource != null) SubscribeToCollection(e.NewItemsSource);
                UpdateLabel();
            };

            if (dataGrid.ItemsSource != null)
            {
                SubscribeToCollection(dataGrid.ItemsSource);
                UpdateLabel();
            }
        }

        private void SetupGridNavigation()
        {
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            else
            {
                SYNCFUSION_DG.Loaded -= OnGridLoadedForNav;
                SYNCFUSION_DG.Loaded += OnGridLoadedForNav;
            }
            UpdateNavigationText();
        }

        private void OnGridLoadedForNav(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
                UpdateNavigationText();
            }
        }

        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;
            int total = 0;
            int current = 0;
            try
            {
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
                    total = SYNCFUSION_DG.View.Records.Count;
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.SelectedIndex >= 0)
                    current = SYNCFUSION_DG.SelectedIndex + 1;
            }
            catch { }
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSfDataFilters();
                ReGetHeadMaster();
                Btn_First_Click(default, default);
            }
            catch { }
        }
        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    SYNCFUSION_DG.SelectedIndex = 0;
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToHome();
                }
            }
            catch { }
        }
        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex > 0)
                {
                    SYNCFUSION_DG.SelectedIndex--;
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;
                    SYNCFUSION_DG.ScrollInView(new RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                {
                    SYNCFUSION_DG.SelectedIndex++;
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;
                    SYNCFUSION_DG.ScrollInView(new RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    var lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                    SYNCFUSION_DG.SelectedIndex = lastIndex;
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSfDataFilters();
        }
        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                //ApplyCumulativeFilter(); // Requires ApplyCumulativeFilter to be public or internal if extracted
                SYNCFUSION_DG.View.Filter = null; // Direct reset if method not available
                SYNCFUSION_DG.View.RefreshFilter();
                UpdateRowCountLabel();

                SYNCFUSION_DG.ClearFilters();
                SF_FSK.ClearFilters();
                SF_MODULE_D.ClearFilters();
                SF_TAKHPERS.ClearFilters();
                SF_REWARDS.ClearFilters();
            }
            catch { }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Optional: Add specific keyboard shortcuts here
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as STUF_DEF);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();

            UpdateRowCountLabel();
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر Contains
                filterService.AddFilter(columnName, selectedText, isExclusion: false, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Contains \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است
            if (filterValue != null)
            {
                // فیلتر Exact Match
                filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: true);

                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} = {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // فیلتر برای null values
                filterService.AddFilter(columnName, null, isExclusion: false, isExactMatch: true);
                ActiveFilters.Add($"{columnName} = NULL");
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            // اگر ستون یا مقدار معتبر نیست، خروج
            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است (partial selection)
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر "Does Not Contain" - برای متن
                filterService.AddFilter(columnName, selectedText, isExclusion: true, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Does Not Contain \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است (exact value)
            if (filterValue != null)
            {
                // فیلتر Exclusion با Exact Match - برای مقدار دقیق
                filterService.AddFilter(columnName, filterValue, isExclusion: true, isExactMatch: true);

                // نمایش بهتر در لیست فیلترها
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} != {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // اگر مقدار null است
                filterService.AddFilter(columnName, null, isExclusion: true, isExactMatch: true);
                ActiveFilters.Add($"{columnName} != NULL");
                ApplyCumulativeFilter();
            }
        }
        private string FormatValueForDisplay(object value)
        {
            if (value == null)
                return "NULL";

            // برای مقادیر عددی، فرمت هزارگان اعمال می‌شود
            if (value is double || value is decimal || value is float)
            {
                try
                {
                    return Convert.ToDecimal(value).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (value is int || value is long || value is short || value is byte)
            {
                try
                {
                    return Convert.ToInt64(value).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }
        #endregion

    }
}
