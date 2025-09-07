using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.BulletGraph;
using Functions;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.HelperWins;
using System.Collections.Specialized;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Prg_Proccessy.FUNCTIONS;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Microsoft.Data.SqlClient;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for WIN_TOZIESELECT.xaml
    /// </summary>
    public partial class WIN_TOZIESELECT : Window, INotifyPropertyChanged
    {
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int _selectedItemsCount = 0;
        public int SelectedItemsCount
        {
            get => _selectedItemsCount;
            set
            {
                if (_selectedItemsCount == value) return;
                _selectedItemsCount = value;
                OnPropertyChanged(nameof(SelectedItemsCount));
            }
        }
        public WIN_TOZIESELECT(int tid)
        {
            InitializeComponent();

            this.DataContext = this;

            OpenArgs_TID = tid;

            if (FACTOR_DATA != null) // اطمینان از اینکه FACTOR_DATA null نیست
            {
                FACTOR_DATA.CollectionChanged += FACTOR_DATA_OnCollectionChanged;
            }
        }
        private void FACTOR_DATA_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // وقتی آیتمی از کالکشن حذف می‌شود، اشتراک PropertyChanged آن را لغو کن
            if (e.OldItems != null)
            {
                foreach (TOZSELMODEL item in e.OldItems)
                {
                    if (item != null) item.PropertyChanged -= HeadLstItem_OnPropertyChanged;
                }
            }

            // وقتی آیتمی به کالکشن اضافه می‌شود، به PropertyChanged آن مشترک شو
            if (e.NewItems != null)
            {
                foreach (TOZSELMODEL item in e.NewItems)
                {
                    if (item != null) item.PropertyChanged += HeadLstItem_OnPropertyChanged;
                }
            }

            // صرف نظر از اینکه چه تغییری رخ داده (اضافه، حذف، پاک شدن)، تعداد را به‌روز کن
            UpdateSelectedCount();
        }
        private void HeadLstItem_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // اگر پراپرتی IsSelected در یکی از آیتم‌ها تغییر کرد
            if (e.PropertyName == nameof(TOZSELMODEL.IsSelected))
            {
                UpdateSelectedCount();
            }
        }

        public int OpenArgs_TID { get; set; }

        // فلگ برای جلوگیری از فراخوانی‌های تودرتو و غیرضروری
        private bool _isUpdatingSelectionByRange = false;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        public ObservableCollection<TOZSELMODEL> FACTOR_DATA { get; set; } = new ObservableCollection<TOZSELMODEL>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //AZ_NUMBER.Text = Baseknow.YEA + "0101";
            AZ_NUMBER.Text = Tarikh.FullCurrentDate;
            TA_NUMBER.Text = Tarikh.FullCurrentDate;

            ReGetData();

            // Ensure the SfDataGrid is not null before subscribing
            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            AZ_NUMBER.Focus();
            AZ_NUMBER.SelectAll();
        }
        private void ReGetData()
        {
            var azNumber = Convert.ToInt32(AZ_NUMBER.Text.ToRawTarikh());
            var taNumber = Convert.ToInt32(TA_NUMBER.Text.ToRawTarikh());

            FACTOR_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<TOZSELMODEL>(@$"SELECT HEAD_LST.NUMBER,
                                                                      HEAD_LST.DATE_N,
                                                                      CUST_HESAB.NAME,
                                                                      CUST_HESAB.hes,
                                                                      CUST_HESAB.ADDRESS
                                                               FROM CUST_HESAB
                                                                   INNER JOIN HEAD_LST
                                                                       ON CUST_HESAB.hes = HEAD_LST.CUST_NO
                                                               WHERE (HEAD_LST.DATE_N
                                                                     BETWEEN {azNumber} AND {taNumber}
                                                                     )
                                                                     AND (HEAD_LST.TAG = 2)
                                                               ORDER BY HEAD_LST.NUMBER;").ToList();
            foreach (var item in MasterHead)
            {
                FACTOR_DATA?.Add(item);
            }
        }

        #region FilterBy
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            // Defensive checks
            if (ROWCOUNT_TEXTBLK == null) return;
            if (SYNCFUSION_DG?.View == null) return;

            // Safely retrieve the record count
            var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            // Set the label content
            ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<TOZSELMODEL> filterService = new FilterService<TOZSELMODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool NowIsReady { get; private set; }

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

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

            UpdateSelectedCount();
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

            //if (mappingName != "IsSelected")
            //{
            //    if (record is TOZSELMODEL item )
            //    {
            //        item.IsSelected = !item.IsSelected;
            //    }
            //}


            if (record == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                return;
            }
            var property = record.GetType().GetProperty(mappingName);
            if (property == null)
            {
                Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
                return;
            }

            //CurrentCellValue = property.GetValue(record)?.ToString();
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
            }
            else
            {
                if (filterValue != null)
                {
                    //برای اینکه دقیقا همون آیتم رو فیلتر کنه:
                    //filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: false);

                    // Add the filter to the filter service
                    filterService.AddFilter(columnName, filterValue);
                    // Add the filter to the list of active filters

                    ActiveFilters.Add($"{columnName} = {filterValue}");
                    // Apply the cumulative filter to the data grid
                }
            }
            ApplyCumulativeFilter();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TOZSELMODEL);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();

            UpdateRowCountLabel();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
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
                    return editingElement.SelectedText; // Return the selected text
                }
            }
            return string.Empty;
        }

        #endregion

        private async void BTN_SENDGRP_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid()) return;

            var selected = FACTOR_DATA.Where(x => x.IsSelected).ToList();
            if (!selected.Any()) return;

            if (!(new Msgwin(true, "آیا از اضافه کردن آیتم‌های انتخاب‌ شده مطمئن هستید؟").ShowDialog() ?? false)) return;

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            bool OthersAddeddSuccessfully = false;
            foreach (var item in selected)
            {
                var number = item.NUMBER; // یا هر کنترل معادل
                int? tid = OpenArgs_TID; // پنجره والد
                string? username = Baseknow.UUSER; // نام کاربری جاری
                DateTime now = DateTime.Now;

                var existing = dbms.DoGetDataSQL<int>(@"
                    SELECT 1 FROM TOZIE_SUB 
                    WHERE NUMBER = @NUMBER AND TID = @TID", new
                {
                    NUMBER = number,
                    TID = tid
                }).FirstOrDefault();

                if (existing == 1)
                {
                    item.IsSelected = false;
                    // Already exists
                    ErrosMessages.Add(new MsgModel
                    {
                        MessageText_U = $"اين فاكتور (حواله) شماره {number} قبلا به لیست توضیع اضافه شده است"
                    });
                    continue;
                }

                string insertSql = @" INSERT INTO TOZIE_SUB (NUMBER, TID, CDATE) VALUES (@NUMBER, @TID, @CDATE);";
                try
                {
                    int? rowsAffected = dbms.DoExecuteSQL(insertSql, new
                    {
                        NUMBER = number,
                        TID = tid,
                        CDATE = now
                    });

                    OthersAddeddSuccessfully = true;
                }
                catch (Exception ex)
                {
                    // Optional: log unexpected errors
                    ErrosMessages.Add(new MsgModel
                    {
                        MessageText_U = $"خطا در اضافه کردن فاکتور {number}: {ex.Message}"
                    });
                }
            }

            if (ErrosMessages.Count > 0)
            {
                if (OthersAddeddSuccessfully)
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = $"بقیه موارد با موفقیت اضافه شد." });
                }

                UpdateSelectedCount();

                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).Show();
            }
            else
            {
                FACTOR_DATA = null;
                this.Close();
            }

        }
        private bool IsValid()
        {
            var azNumber = Convert.ToInt32(AZ_NUMBER.Text.ToRawTarikh());
            var taNumber = Convert.ToInt32(TA_NUMBER.Text.ToRawTarikh());

            if (azNumber == null || taNumber == null || azNumber > taNumber)
            {
                new Msgwin(false, "محدوده شماره برای ارسال گروهی نامعتبر است.").Show();
                return false;
            }

            List<MsgModel> ErrosMessages = new List<MsgModel>();

            var selected = FACTOR_DATA.Where(h => h.IsSelected)
                                      .Select(h => (long)h.NUMBER)
                                      .ToList();
            if (!selected.Any())
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"هیچ فاکتور انتخاب نشده !" });
            }

            if (ErrosMessages.Any())
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                    .Select(message => new MsgModel { MessageText_U = message }).ToList();

                _ = new MsgListwin(false, ErrosMessages).ShowDialog();
                ErrosMessages?.Clear();

                return false;
            }

            return true;
        }
        private void UpdateSelectedCount()
        {
            if (FACTOR_DATA != null)
            {
                SelectedItemsCount = FACTOR_DATA.Count(f => f.IsSelected);
            }
        }
        bool AllSelected = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AllSelected = !AllSelected;
            // دسترسی به View فعلی (شامل فیلترها)
            var view = SYNCFUSION_DG.View;

            var Btn = sender as Button;
            if (Btn != null)
            {
                Btn.Content = AllSelected ? "لغو اتخاب همه آیتم های موجود" : "اتخاب همه آیتم های موجود";
            }

            // هر رکورد نمایش‌داده‌شده را بردار و IsSelected را true کن
            foreach (var record in view.Records)
            {
                if (record.Data is TOZSELMODEL item)
                    item.IsSelected = AllSelected;
            }

            UpdateSelectedCount();
        }

        private void AZ_NUMBER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var azNumber = Convert.ToInt32(AZ_NUMBER.Text.ToRawTarikh() ?? "0");

            if (!Tarikh.IsValidedDate(azNumber.ToString()))
            {
                e.Handled = true; //Cancel Leaving Focus
            }
            else
            {
                var taNumber = Convert.ToInt32(TA_NUMBER.Text.ToRawTarikh() ?? "0");
                if (azNumber > taNumber)
                {
                    universControl.PopNotifyShow("تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    e.Handled = true; //Cancel Leaving Focus
                }
            }
        }

        private void TA_NUMBER_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var taNumber = Convert.ToInt32(TA_NUMBER.Text.ToRawTarikh() ?? "0");
            if (!Tarikh.IsValidedDate(taNumber.ToString()))
            {
                e.Handled = true; //Cancel Leaving Focus
            }
            else
            {
                var azNumber = Convert.ToInt32(AZ_NUMBER.Text.ToRawTarikh() ?? "0");

                if (azNumber > taNumber)
                {
                    universControl.PopNotifyShow("تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    e.Handled = true; //Cancel Leaving Focus
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }
    }
}
