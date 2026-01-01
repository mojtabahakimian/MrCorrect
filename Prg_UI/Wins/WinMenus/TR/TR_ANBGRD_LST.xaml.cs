using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Stimulsoft.Report.Helpers;
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_LST_HAVL;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Prg_UI.Wins.WinMenus.SANATI.HAVALE_EXIT_SAYER;
using static Prg_UI.Wins.WinMenus.TR.TR_DEED_HEAD;
using static Wins.WinMenus.SANATI.HAVALAH_ENTER;


namespace Prg_UI.Wins.WinMenus.TR
{
    /// <summary>
    /// Interaction logic for TR_ANBGRD_LST.xaml
    /// </summary>
    public partial class TR_ANBGRD_LST : Window
    {

        #region Local Models
        public class TR_ANBARGRD_SUB_MODEL : INotifyPropertyChanged, ICloneable
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #region Properties
            private double? _ekh;
            public double? EKH { get => _ekh; set { if (_ekh == value) return; _ekh = value; OnPropertyChanged("EKH"); } }
            private int? _grd_num;
            public int? GRD_NUM { get => _grd_num; set { if (_grd_num == value) return; _grd_num = value; OnPropertyChanged("GRD_NUM"); } }
            private string? _code;
            public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }

            private decimal? _mog;
            public decimal? MOG { get => _mog; set { if (_mog == value) return; _mog = value; OnPropertyChanged("MOG"); } }

            private double? _num1;
            public double? NUM1 { get => _num1; set { if (_num1 == value) return; _num1 = value; OnPropertyChanged("NUM1"); } }
            private double? _num2;
            public double? NUM2 { get => _num2; set { if (_num2 == value) return; _num2 = value; OnPropertyChanged("NUM2"); } }
            private double? _num3;
            public double? NUM3 { get => _num3; set { if (_num3 == value) return; _num3 = value; OnPropertyChanged("NUM3"); } }
            private double? _mabl;
            public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
            private string? _names;
            public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }
            private string? _nam;
            public string? nam { get => _nam; set { if (_nam == value) return; _nam = value; OnPropertyChanged("nam"); } }
            private string? _n_fani;
            public string? N_FANI { get => _n_fani; set { if (_n_fani == value) return; _n_fani = value; OnPropertyChanged("N_FANI"); } }
            private string? _grp;
            public string? grp { get => _grp; set { if (_grp == value) return; _grp = value; OnPropertyChanged("grp"); } }
            #endregion

            // --- History Columns ---
            public long? UP_DATE { get; set; }

            public double? UP_TIME { get; set; } //string
                                                 // پراپرتی نمایشی (رشته فرمت شده)
            public string UpTimeDisplay
            {
                get
                {
                    if (UP_TIME.HasValue)
                    {
                        try
                        {
                            var dt = DateTime.FromOADate(UP_TIME.Value);

                            // ایجاد کالچر فارسی
                            var persianCulture = new CultureInfo("fa-IR");

                            // اجبار به استفاده از تقویم میلادی (برای اینکه سال 2025 باشد نه 1404)
                            persianCulture.DateTimeFormat.Calendar = new GregorianCalendar();

                            // فرمت‌دهی: روز/ماه/سال ساعت:دقیقه:ثانیه ب.ظ
                            return dt.ToString("yyyy/MM/dd hh:mm:ss tt", persianCulture);
                        }
                        catch
                        {
                            return "";
                        }
                    }
                    return "";
                }
            }
            public string? UP_USER_NAME { get; set; }
            public string? PC_NAME { get; set; }
            public string? IPADD { get; set; }
        }
        public class TR_ANBGRD_HEAD_LST_MODEL : INotifyPropertyChanged, ICloneable
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private int? _grd_num;
            public int? GRD_NUM { get => _grd_num; set { if (_grd_num == value) return; _grd_num = value; OnPropertyChanged("GRD_NUM"); } }
            private long? _grd_date;
            public long? GRD_DATE { get => _grd_date; set { if (_grd_date == value) return; _grd_date = value; OnPropertyChanged("GRD_DATE"); } }
            private int? _grd_anbar;
            public int? GRD_ANBAR { get => _grd_anbar; set { if (_grd_anbar == value) return; _grd_anbar = value; OnPropertyChanged("GRD_ANBAR"); } }
            private string? _grd_anbar_name;
            public string? GRD_ANBAR_NAME { get => _grd_anbar_name; set { if (_grd_anbar_name == value) return; _grd_anbar_name = value; OnPropertyChanged("GRD_ANBAR_NAME"); } }
            private string? _grd_hes;
            public string? GRD_HES { get => _grd_hes; set { if (_grd_hes == value) return; _grd_hes = value; OnPropertyChanged("GRD_HES"); } }
            private string? _grd_hes_name;
            public string? GRD_HES_NAME { get => _grd_hes_name; set { if (_grd_hes_name == value) return; _grd_hes_name = value; OnPropertyChanged("GRD_HES_NAME"); } }
            private double? _n_s;
            public double? N_S { get => _n_s; set { if (_n_s == value) return; _n_s = value; OnPropertyChanged("N_S"); } }
            private string? _comment;
            public string? COMMENT { get => _comment; set { if (_comment == value) return; _comment = value; OnPropertyChanged("COMMENT"); } }
            private string? _user_name;
            public string? USER_NAME { get => _user_name; set { if (_user_name == value) return; _user_name = value; OnPropertyChanged("USER_NAME"); } }
            private DateTime? _crt;
            public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
            private int? _uid;
            public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

            // --- History Columns ---
            public long? UP_DATE { get; set; }

            public double? UP_TIME { get; set; } //string
                                                 // پراپرتی نمایشی (رشته فرمت شده)
            public string UpTimeDisplay
            {
                get
                {
                    if (UP_TIME.HasValue)
                    {
                        try
                        {
                            var dt = DateTime.FromOADate(UP_TIME.Value);

                            // ایجاد کالچر فارسی
                            var persianCulture = new CultureInfo("fa-IR");

                            // اجبار به استفاده از تقویم میلادی (برای اینکه سال 2025 باشد نه 1404)
                            persianCulture.DateTimeFormat.Calendar = new GregorianCalendar();

                            // فرمت‌دهی: روز/ماه/سال ساعت:دقیقه:ثانیه ب.ظ
                            return dt.ToString("yyyy/MM/dd hh:mm:ss tt", persianCulture);
                        }
                        catch
                        {
                            return "";
                        }
                    }
                    return "";
                }
            }
            public string? UP_USER_NAME { get; set; }
            public string? PC_NAME { get; set; }
            public string? IPADD { get; set; }
        }
        #endregion
        public TR_ANBGRD_LST()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");

            this.DataContext = this;

        }

        #region Window Setup

        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

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

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public ObservableCollection<TR_ANBGRD_HEAD_LST_MODEL> ANBGRD_HEAD_DATA { get; set; } = new ObservableCollection<TR_ANBGRD_HEAD_LST_MODEL>();
        public ObservableCollection<TR_ANBARGRD_SUB_MODEL> ANBGRD_SUB_DATA { get; set; } = new ObservableCollection<TR_ANBARGRD_SUB_MODEL>();


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Security Check
            try
            {
                var helper = new WindowInteropHelper(this);
                helper.EnsureHandle();
                CL_HESABDARI.SETSECURITY(this.GetType().Name, "ANGD", helper.Handle, this.GetType().Name);
                if (!this.IsLoaded) { this.Close(); return; }
            }
            catch { this.Close(); return; }

            ReGetHeadMaster();

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            SetupGridNavigation();
            AttachRecordCountUpdater(SF_SUB, TXT_COUNT_FACTOR);
        }

        private void ReGetHeadMaster()
        {
            ANBGRD_HEAD_DATA.Clear();

            // Fetch from TR_ANBGRD_HEAD join with definitions for names
            var query = @"
                SELECT 
                    T.*,
                    ANB.NAMES AS GRD_ANBAR_NAME,
                    HES.NAME AS GRD_HES_NAME
                FROM dbo.TR_ANBGRD_HEAD T
                LEFT JOIN dbo.TCOD_ANBAR ANB ON T.GRD_ANBAR = ANB.CODE
                LEFT JOIN dbo.CUST_HESAB HES ON T.GRD_HES = HES.hes
                ORDER BY T.GRD_DATE DESC, T.GRD_NUM DESC, T.UP_DATE DESC, T.UP_TIME DESC
            ";

            var list = dbms.DoGetDataSQL<TR_ANBGRD_HEAD_LST_MODEL>(query).ToList();
            foreach (var item in list)
            {
                ANBGRD_HEAD_DATA.Add(item);
            }
        }

        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {

            if (SYNCFUSION_DG.SelectedItem is TR_ANBGRD_HEAD_LST_MODEL selectedItem)
            {
                ReGetData(selectedItem);
            }
            else
            {
                ANBGRD_SUB_DATA.Clear();
            }
        }

        // 1. متد غیرهمگام برای دریافت داده‌ها از دیتابیس
        private async Task<List<TR_ANBARGRD_SUB_MODEL>> GetSubDetailsAsync(TR_ANBGRD_HEAD_LST_MODEL header)
        {
            // Note: Using composite key matching for history details
            // Floating point time comparison needs precision handling or casting
            const int ROUND_PRECISION = 6;

            string sql = $@"
                    SELECT 
                        S.*,
                        ST.NAME AS nam
                    FROM dbo.TR_ANBGRD_LST S
                    LEFT JOIN dbo.STUF_DEF ST ON S.CODE = ST.CODE
                    WHERE S.GRD_NUM = @GrdNum 
                      AND S.UP_DATE = @UpDate 
                      AND ROUND(S.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})
                ";

            var parameters = new
            {
                GrdNum = header.GRD_NUM,
                UpDate = header.UP_DATE,
                UpTime = header.UP_TIME ?? 0
            };

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                // استفاده از OpenAsync برای جلوگیری از فریز شدن UI در زمان اتصال
                await db.OpenAsync();

                // استفاده از Dapper به صورت Async
                var result = await db.QueryAsync<TR_ANBARGRD_SUB_MODEL>(sql, parameters);
                return result.ToList();
            }
        }
        private async void ReGetData(TR_ANBGRD_HEAD_LST_MODEL header)
        {
            // جلوگیری از اجرای خطا در صورت نال بودن ورودی
            if (header == null) return;

            // تعریف تسک دریافت اطلاعات (هنوز await نمی‌کنیم)
            var dataFetchTask = GetSubDetailsAsync(header);

            // تعریف یک تاخیر ۳۰۰ میلی‌ثانیه‌ای (آستانه تحمل کاربر)
            var delayTask = Task.Delay(300);

            // مسابقه بین دریافت اطلاعات و تاخیر
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);

            bool loaderShown = false;

            // اگر تاخیر زودتر تمام شد (یعنی دیتابیس بیشتر از ۳۰۰ میلی‌ثانیه طول کشیده)
            if (completedTask == delayTask)
            {
                if (BusyOverlay != null) BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                // دریافت نتیجه نهایی (اگر تسک دیتا تمام شده باشد، بلافاصله برمی‌گردد)
                var details = await dataFetchTask;

                if (details == null)
                {
                    if (loaderShown && BusyOverlay != null) BusyOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                // --- بروزرسانی UI ---
                foreach (var item in details)
                {
                    ANBGRD_SUB_DATA.Add(item);
                }

                GenerateAutomaticSummary(SF_SUB);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری جزئیات سند").ShowDialog();
            }
            finally
            {
                // اگر لودینگ نمایش داده شده بود، مخفی شود
                if (loaderShown && BusyOverlay != null)
                {
                    BusyOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        // 2. متد اصلی مدیریت UI و فراخوانی
        private async void ReGetDataSync(TR_ANBGRD_HEAD_LST_MODEL selectedRow)
        {
            if (selectedRow == null) return;

            BusyOverlay.Visibility = Visibility.Visible;
            ANBGRD_SUB_DATA.Clear();

            try
            {
                // Logic based on TR_FACOTRLST: matching exact history snapshot via UP_TIME/UP_DATE
                const int ROUND_PRECISION = 6;

                string sql = $@"
                    SELECT 
                        S.*,
                        ST.NAME AS nam,
                        ST.NAMES AS NAMES
                    FROM dbo.TR_ANBGRD_LST S
                    LEFT JOIN dbo.STUF_DEF ST ON S.CODE = ST.CODE
                    WHERE S.GRD_NUM = @GrdNum 
                      AND S.UP_DATE = @UpDate 
                      AND ROUND(S.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION})
                ";

                var parameters = new
                {
                    GrdNum = selectedRow.GRD_NUM,
                    UpDate = selectedRow.UP_DATE,
                    UpTime = selectedRow.UP_TIME ?? 0
                };

                using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
                var result = await db.QueryAsync<TR_ANBARGRD_SUB_MODEL>(sql, parameters);

                foreach (var item in result)
                {
                    ANBGRD_SUB_DATA.Add(item);
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری جزئیات: " + ex.Message).ShowDialog();
            }
            finally
            {
                BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }


        #region _SfDataGrid_
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            //// Defensive checks
            //if (ROWCOUNT_TEXTBLK == null) return;
            //if (SYNCFUSION_DG?.View == null) return;

            //// Safely retrieve the record count
            //var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            //// Set the label content
            //ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<TR_ANBGRD_HEAD_LST_MODEL> filterService = new FilterService<TR_ANBGRD_HEAD_LST_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool IsExporty { get; private set; } = false;
        public bool NowIsReady { get; private set; }

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private bool isFactory = false;

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }

        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
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
                //Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
                return;
            }

            //CurrentCellValue = property.GetValue(record)?.ToString();
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController?.CurrentCellManager?.CurrentCell;

            if (currentCell == null)
                return string.Empty;

            // حالت 1: Edit Mode
            if (currentCell.IsEditing)
            {
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null && !string.IsNullOrEmpty(editingElement.SelectedText))
                {
                    return editingElement.SelectedText;
                }
            }

            // حالت 2: جستجوی ساده - بدون GetCellElement
            try
            {
                var gridCellElement = currentCell?.ColumnElement;
                if (gridCellElement != null)
                {
                    var textBox = FindVisualChild<TextBox>(gridCellElement);
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.SelectedText))
                    {
                        return textBox.SelectedText;
                    }
                }
            }
            catch { }

            return string.Empty;
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TR_ANBGRD_HEAD_LST_MODEL);
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
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
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
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion
        public void GenerateAutomaticSummary1(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                _DG_.TableSummaryRows.Clear();
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

            var dataType = typeof(TR_ANBARGRD_SUB_MODEL);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(TR_ANBARGRD_SUB_MODEL).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "bed" || column.MappingName.ToLower() == "bes"))
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
        public void GenerateAutomaticSummary(SfDataGrid _DG_)
        {
            // همیشه پاک کن و دوباره بساز تا از تکرار جلوگیری شود
            _DG_.TableSummaryRows.Clear();

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;
            summaryRow.Title = "جمع:"; // اختیاری

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            // تعریف دستی ستون‌های جمع برای اطمینان
            // چون مدل مشخص است، نیازی به حلقه و رفلکشن پیچیده نیست

            // جمع بدهکار
            summaryColumns.Add(new GridSummaryColumn
            {
                Name = "BEDSum",
                MappingName = "BED",
                SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate, // یا DecimalAggregate بسته به نوع داده
                Format = "{Sum:N0}"
            });

            // جمع بستانکار
            summaryColumns.Add(new GridSummaryColumn
            {
                Name = "BESSum",
                MappingName = "BES",
                SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                Format = "{Sum:N0}"
            });

            //summaryRow.SummaryColumns = summaryColumns;
            //_DG_.TableSummaryRows.Add(summaryRow);

            // 4. اعمال تغییرات و رفرش اجباری
            if (summaryColumns.Any())
            {
                summaryRow.SummaryColumns = summaryColumns;
                _DG_.TableSummaryRows.Add(summaryRow);

                // ******************************************************
                // *** نکته کلیدی: این خط مشکل صفر بودن را حل می‌کند ***
                // ******************************************************
                // به گرید می‌گوییم حالا که سامری اضافه شد، محاسبات را انجام بده
                if (_DG_.View != null)
                {
                    _DG_.View.Refresh();
                }
            }
        }

        #region Navigation Logic
        // 1. این متد را در انتهای Window_Loaded صدا بزنید
        // --- Safe Navigation Logic ---

        private void SetupGridNavigation()
        {
            // 1. بررسی ایمنی: اگر کنترل‌ها هنوز ساخته نشده‌اند، خارج شو
            // این خط جلوی خطای NullReference را می‌گیرد اگر XAML آپدیت نشده باشد
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null)
            {
                // جهت دیباگ: اگر این خط اجرا شد یعنی یکی از نام‌ها در XAML اشتباه است
                return;
            }

            // 2. اتصال رویداد تغییر انتخاب (Selection)
            // ابتدا حذف می‌کنیم تا دوبار متصل نشود (-=)
            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            // 3. اتصال رویداد تغییر تعداد رکوردها (Collection Changed)
            // نکته مهم: بررسی می‌کنیم که View آماده است یا نه
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                // اگر ویو آماده بود، وصل شو
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            else
            {
                // اگر ویو هنوز نال بود، به رویداد Loaded خود گرید وصل می‌شویم تا بعدا انجام دهیم
                SYNCFUSION_DG.Loaded -= OnGridLoadedForNav;
                SYNCFUSION_DG.Loaded += OnGridLoadedForNav;
            }

            // 4. آپدیت اولیه متن‌ها (با بررسی نال)
            UpdateNavigationText();
        }

        // اگر گرید در ابتدا آماده نبود، این متد بعداً صدا زده می‌شود
        private void OnGridLoadedForNav(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
                UpdateNavigationText();
            }
        }

        // هندلرهای کمکی برای جلوگیری از خطای ترد
        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            // بررسی ایمنی مجدد
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            int total = 0;
            int current = 0;

            try
            {
                // محاسبه تعداد کل (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
                {
                    total = SYNCFUSION_DG.View.Records.Count;
                }

                // محاسبه ایندکس جاری (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.SelectedIndex >= 0)
                {
                    current = SYNCFUSION_DG.SelectedIndex + 1;
                }
            }
            catch
            {
                // نادیده گرفتن خطا در شرایط خاص
            }

            // نمایش
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        // 3. رویدادهای کلیک دکمه‌ها
        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSfDataFilters(); // حذف فیلترها
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
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(1, 0));
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
                    // اسکرول به ایندکس جدید (ایندکس رکورد + هدرها)
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(SYNCFUSION_DG.SelectedIndex + 1, 0));
                    //var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    //SYNCFUSION_DG.GetVisualContainer().ScrollRows.ScrollInView(rowIndex, 0);

                    SYNCFUSION_DG.SelectedIndex--;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید (با احتساب هدرها و فیلترها)
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون قابل مشاهده (برای ساخت RowColumnIndex صحیح)
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
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

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
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
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(lastIndex + 1, 0));

                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSfDataFilters();
        }
        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;

            // متد داخلی برای به‌روزرسانی متن بر اساس منبع داده
            void UpdateLabel()
            {
                int count = 0;

                // اولویت با بررسی مستقیم منبع داده است (چون دقیق‌تر و سریع‌تر از View است)
                if (dataGrid.ItemsSource is ICollection collection)
                {
                    count = collection.Count;
                }
                else if (dataGrid.View != null && dataGrid.View.Records != null)
                {
                    // اگر منبع داده مستقیم نبود، سراغ ویو می‌رویم
                    count = dataGrid.View.Records.Count;
                }

                // چون ممکن است این فراخوانی از ترد دیگری باشد، از Dispatcher استفاده می‌کنیم
                Dispatcher.Invoke(() =>
                {
                    targetTextBlock.Text = count.ToString("N0");
                });
            }

            // متد برای اتصال به رویداد تغییرات کالکشن
            void SubscribeToCollection(object source)
            {
                if (source is INotifyCollectionChanged notifyingCollection)
                {
                    notifyingCollection.CollectionChanged += (s, e) => UpdateLabel();
                }
            }

            // 1. هر وقت کل منبع داده عوض شد (مثلا new ObservableCollection شد)
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                // به کالکشن جدید گوش بده
                if (e.NewItemsSource != null)
                {
                    SubscribeToCollection(e.NewItemsSource);
                }
                UpdateLabel();
            };

            // 2. اگر همین الان دیتایی دارد، به آن وصل شو و مقدار اولیه را ست کن
            if (dataGrid.ItemsSource != null)
            {
                SubscribeToCollection(dataGrid.ItemsSource);
                UpdateLabel();
            }
        }
        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                ApplyCumulativeFilter();
                SYNCFUSION_DG.ClearFilters();

                SF_SUB.ClearFilters();
            }
            catch (Exception)
            {
            }
        }
    }
}
