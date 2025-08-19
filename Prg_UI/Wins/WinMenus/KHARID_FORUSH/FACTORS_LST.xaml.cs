using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Prg_Proccessy.SQLMODELS;
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

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class FACTORS_LST : Window
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

        public FACTORS_LST(byte? _TAGCODE_)
        {
            InitializeComponent();

            this.DataContext = this;

            if (_TAGCODE_ != null)
            {
                TAGCODE = (byte)_TAGCODE_;
            }
        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<HEAD_LST_SRC> FACTOR_DATA { get; set; } = new ObservableCollection<HEAD_LST_SRC>();
        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = ProcLoader.Start();

            FACTOR_DATA?.Clear();

            string WhereCondition = TAGCODE > 0 ? $" WHERE (dbo.HEAD_LST.TAG = {TAGCODE}) " : "  ";

            //if (TAGCODE == 2 || TAGCODE == 13 || TAGCODE == 20) //حواله , فاکتور , پیش فاکتور
            //{
            //    WhereCondition = CL_LMethods.GetRestrictedSqlQuery(TAGCODE, WhereCondition);
            //}

            WhereCondition = CL_LMethods.GetRestrictedSqlQuery(TAGCODE, WhereCondition);

            var MasterHead = dbms.DoGetDataSQL<HEAD_LST_SRC>(@$" SELECT dbo.HEAD_LST.NUMBER1, dbo.HEAD_LST.TAH, dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.MAS, dbo.HEAD_LST.N_S, dbo.HEAD_LST.CUST_NO, dbo.CUST_HESAB.NAME, dbo.HEAD_LST.MOLAH, 
                                                                     dbo.HEAD_LST.M_NAGHD, dbo.HEAD_LST.MABL_VAR, dbo.HEAD_LST.MOIN_VAR, dbo.HEAD_LST.MABL_HAV, dbo.HEAD_LST.MOIN_HAV, dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.MOIN_HAZ, dbo.HEAD_LST.TAKHFIF, 
                                                                     dbo.HEAD_LST.MOIN_KHF,dbo.HEAD_LST.TAG, dbo.DEPART.DEPNAME, dbo.SHIFT.SHNAME, dbo.CUSTKIND.CUSTKNAME, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.SHARAYET, dbo.HEAD_LST.MBAA, dbo.HEAD_LST.HMBAA, 
                                                                     dbo.HEAD_LST.TICMBAA, dbo.HEAD_LST.TKHF, dbo.HEAD_LST.OKF, dbo.HEAD_LST.JAY, dbo.HEAD_LST.SGN1, dbo.HEAD_LST.SGN2, dbo.HEAD_LST.SGN3, dbo.HEAD_LST.sgn1usid, dbo.HEAD_LST.sgn2usid, 
                                                                     dbo.HEAD_LST.sgn3usid, dbo.HEAD_LST.CRT, dbo.HEAD_LST.UID, dbo.PRICE_ELAMIE.PEPNAME, dbo.PRICE_ELAMIETF.PENAME, dbo.PRICE_PAYNO.PPAME
                                                                     FROM dbo.HEAD_LST LEFT OUTER JOIN
                                                                     dbo.PRICE_PAYNO ON dbo.HEAD_LST.MODAT_PPID = dbo.PRICE_PAYNO.PPID LEFT OUTER JOIN
                                                                     dbo.PRICE_ELAMIETF ON dbo.HEAD_LST.PEID = dbo.PRICE_ELAMIETF.PEID LEFT OUTER JOIN
                                                                     dbo.CUSTKIND ON dbo.HEAD_LST.CUST_KIND = dbo.CUSTKIND.CUST_COD LEFT OUTER JOIN
                                                                     dbo.PRICE_ELAMIE ON dbo.HEAD_LST.PEPID = dbo.PRICE_ELAMIE.PEPID LEFT OUTER JOIN
                                                                     dbo.DEPART ON dbo.HEAD_LST.DEPATMAN = dbo.DEPART.DEPATMAN LEFT OUTER JOIN
                                                                     dbo.SHIFT ON dbo.HEAD_LST.SHIFT = dbo.SHIFT.SHIFT_ID LEFT OUTER JOIN
                                                                     dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO = dbo.CUST_HESAB.hes
                                                                     {WhereCondition}
                                                                     ORDER BY dbo.HEAD_LST.NUMBER1,dbo.HEAD_LST.NUMBER DESC ").ToList();
            foreach (var item in MasterHead)
            {
                FACTOR_DATA.Add(item);
            }

            #region COLUMN_DISPLAYER

            if (TAGCODE == 13)
            {
                ISEND_COLUMN.IsHidden = false; //نمایش ستون مودیان
            }

            switch (TAGCODE)
            {
                //فاکتوری ها
                case 3:
                case 4:
                case 12:
                case 13:
                case 14:
                case 20:
                case 27:
                    NUMBER_FAC_COLUMN.IsHidden = false; //Show
                    break;

                //انباری ها
                case 1:
                case 2:
                case 5:
                case 23:
                case 24:
                case 26:
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ";
                    NUMBER_FAC_COLUMN.IsHidden = true; //Hide
                    MODAT_COLUMN.IsHidden = true;
                    SANAD_COLUMN.IsHidden = true;
                    NAGHD_COLUMN.IsHidden = true;
                    VARIZI_COLUMN.IsHidden = true;
                    MOEENVARIZ_COLUMN.IsHidden = true;
                    MABL_HAV_COLUMN.IsHidden = true;
                    MOEEN_HAV_COLUMN.IsHidden = true;
                    MABL_KHAD_COLUMN.IsHidden = true;
                    MOEEN_KHAD_COLUMN.IsHidden = true;
                    MABL_TAKHFIF_COLUMN.IsHidden = true;
                    break;

                default: break;
            }
            #endregion

            switch (TAGCODE) //عنوان پنجره
            {
                case 27:
                    WINTILENAME.Content = "فاکتور های برگشت خرید آزاد";
                    break;

                case 26: WINTILENAME.Content = "سایر حواله انبار ها"; break;

                case 25:
                    WINTILENAME.Content = "فاکتور های برگشت فروش آزاد رسید شده";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره برگه";
                    break;
                case 24: WINTILENAME.Content = "سایر رسید انبار ها"; break;

                case 23:
                    WINTILENAME.Content = "درخواست خرید ها";
                    TAH_COLUMN.IsHidden = false;
                    break;

                case 20:
                    WINTILENAME.Content = "پیش فاکتور ها";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    SANAD_COLUMN.IsHidden = true;
                    NUMBER_HAV_COLUMN.HeaderText = "شماره پیش فاکتور";
                    break;

                case 14: WINTILENAME.Content = "فاکتور های خدمات"; break;
                case 13: WINTILENAME.Content = "فاکتور های فروش"; break;
                case 12:
                    WINTILENAME.Content = "فاکتور های خرید";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره رسید انبار ها";
                    break;

                case 10:
                    WINTILENAME.Content = "برگه های خروج مواد اولیه";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره حواله انبار";
                    CUST_HESAB_COLUMN.HeaderText = "حساب مسئول شیفت";
                    CUST_NAME_COLUMN.HeaderText = "نام مسئول شیفت";
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ حواله";
                    MODAT_COLUMN.IsHidden = true;
                    break;

                case 9:
                    WINTILENAME.Content = "برگه های ورود کالای ساخته شده";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره برگه";
                    CUST_HESAB_COLUMN.HeaderText = "حساب مسئول شیفت";
                    CUST_NAME_COLUMN.HeaderText = "نام مسئول شیفت";
                    TARIKH_FAC_COLUMN.HeaderText = "تاریخ";
                    MODAT_COLUMN.IsHidden = true;
                    break;

                case 5:
                    WINTILENAME.Content = "انتقال از انبار به انبار";
                    break;

                case 4: WINTILENAME.Content = "فاکتور های برگشت فروش - عادی"; break;
                case 3: WINTILENAME.Content = "فاکتور های برگشت خرید - عادی"; break;

                case 2:
                    WINTILENAME.Content = "حواله های فروش";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    break;

                case 1:
                    WINTILENAME.Content = "رسید های خرید";
                    NUMBER_HAV_COLUMN.HeaderText = "شماره رسید";
                    NUMBER_FAC_COLUMN.IsHidden = true;
                    break;

                default: WINTILENAME.Content = "همه نوع فاکتور"; break;
            }

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;

            GenerateAutomaticSummary(SYNCFUSION_DG);

            // Ensure the SfDataGrid is not null before subscribing
            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            //ProcLoader.Stop(Prc);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as HEAD_LST_SRC;

                switch (currentRow?.TAG)
                {
                    case 1: // رسید خرید
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_RASID), (double)currentRow.NUMBER, "یک پنجره رسید خرید از قبل باز شده ابتدا آنرا ببندید.");

                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 2: // حواله فروش
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_HAVL), (double)currentRow.NUMBER, "یک پنجره حواله فروش از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 3: //فاکتور برگشت خرید عادی	
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KH_BACK), (double)currentRow.NUMBER, "یک پنجره فاکتور برگشت خرید عادی از قبل باز شده ابتدا آنرا ببندید.");
                            //SELECT* FROM dbo.HEAD_LST WHERE NUMBER = 2 AND TAG = 3--فاکتور برگشت خرید عادی Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 2073 AND TAG = 1
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 4:
                        if (currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_FROOSH_BACK2), (double)currentRow.NUMBER, "یک پنجره فاکتور برگشت فروش عادی از قبل باز شده ابتدا آنرا ببندید.");
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 254  AND TAG = 4 --فاکتور برگشت فروش استاندارد عادی Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 5361 AND TAG = 2
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 5: //انتقال از انبار به انبار
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 9: //برگه ورود
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_ENTER, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 10: //برگه خروج مواد اولیه
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HAVALAH_EXIT, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 12: // فاکتور خرید
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KHAREED1), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور خرید از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, this, currentRow.NUMBER);
                        }
                        break;

                    case 13: // فاکتور فروش
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_FROOSH22), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور فروش از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, this, currentRow.NUMBER1 + "," + currentRow.NUMBER);
                        }
                        break;

                    case 14: // فاکتور خدمات
                        if (currentRow?.NUMBER1 != null && currentRow?.NUMBER != null)
                        {
                            //OpenWindow(typeof(HEAD_LST_KHADAMAT), currentRow.NUMBER1 + "," + currentRow.NUMBER, "یک پنجره فاکتور خدمات از قبل باز شده ابتدا آنرا ببندید.");
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHADAMAT, this, currentRow.NUMBER);
                        }
                        break;

                    case 20: //پیش فاکتور ها
                        if (currentRow?.NUMBER != null)
                        {
                            try
                            {
                                Application.Current.Windows.OfType<HEAD_LST_PISHFROOSH2>().FirstOrDefault()?.Close();
                            }
                            catch { }

                            //OpenWindow(typeof(HEAD_LST_PISHFROOSH2), (double)currentRow.NUMBER, "یک پنجره پیش فاکتور از قبل باز شده ابتدا آنرا ببندید.");

                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 23: //درخواست خرید
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_REQUEST_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 24: //سایر رسید انبار ها
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID_OTHER_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 25: //فاکتور برگشت فروش آزاد رسید شده
                        if (currentRow?.NUMBER != null)
                        {
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 954 AND TAG = 25 --فاکتور برگشت فروش رسید شده : آزاد Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 954 AND TAG = 24
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_BRFR, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 26: //سایر حواله انبار ها
                        if (currentRow?.NUMBER != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN, this, (double)currentRow.NUMBER);
                        }
                        break;

                    case 27: //فاکتور برگشت خرید آزاد
                        if (currentRow?.NUMBER != null)
                        {
                            //SELECT * FROM dbo.HEAD_LST WHERE NUMBER = 3 AND TAG = 27   --فاکتور برگشت خرید آزاد Normal Only Header because Detail load FROM dbo.INVO_LST WHERE NUMBER = 3 AND TAG = 26
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD, this, (double)currentRow.NUMBER);
                        }
                        break;

                }
            }
        }
        public void OpenWindow(Type windowType, object parameter, string errorMessage)
        {
            if (windowType == null || !typeof(Window).IsAssignableFrom(windowType))
                return;

            var constructor = windowType.GetConstructor(new[] { parameter.GetType() });
            if (constructor != null)
            {
                var window = (Window)constructor.Invoke(new[] { parameter });
                window.Show();
            }

            return; //Check is there any open window before ?
            if (!CL_LMethods.IsWindowOpen(windowType)) //CL_LMethods.IsWindowOpen<HEAD_LST_FROOSH22>()
            {

            }
            else
            {
                new Msgwin(false, errorMessage).ShowDialog();
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

        private readonly FilterService<HEAD_LST_SRC> filterService = new FilterService<HEAD_LST_SRC>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as HEAD_LST_SRC);
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
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }
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

            var dataType = typeof(HEAD_LST_SRC);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(HEAD_LST_SRC).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
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

        private void BTN_ISEND_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = SYNCFUSION_DG.SelectedItem as HEAD_LST_SRC;

            if (CurrentRow != null && CurrentRow?.NUMBER != null && CurrentRow?.NUMBER > 0)
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_MOADIAN_SINGLE, this, Convert.ToDouble(CurrentRow.NUMBER));
            }
        }

    }
}
