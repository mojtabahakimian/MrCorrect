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
using Wins.WinMenus.KHARID_FORUSH;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.ANBAR.HEAD_SERCH_MAIN_ADVANC;
using Prg_Proccessy.MODELS;
using System.ComponentModel;
using Syncfusion.UI.Xaml.TreeGrid;


namespace Prg_UI.Wins.WinMenus.ANBAR
{
    public partial class KALAS_MAIN_ADVANCE : Window
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
        public KALAS_MAIN_ADVANCE()
        {
            InitializeComponent();

            this.DataContext = this;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");

            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG);
            }
        }

        UniversControl universControl = new UniversControl();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<KALAS> FACTOR_DATA { get; set; } = new ObservableCollection<KALAS>();
        public bool NowIsReady { get; private set; }
        public string SqlQueryPassed { get; set; } = "";
        public bool isSummed { get; set; } = false;
        public List<string> RestrictionMessages { get; set; } = new List<string>();
        public List<string> ColumnSelectedPassed { get; set; } = new List<string>();



        #region ComboBoxItemPassed
        //public List<TAGCOD>? TAGCODE_Data { get; set; }
        //public List<TCOD_VAHEDS>? VAHCODE_Data { get; set; }
        //public List<TCOD_STUFGROUP>? GRPCODE_Data { get; set; }
        //public List<TCOD_OSTAN>? OSTANID_Data { get; set; }
        //public List<TCOD_CITY>? SHAHRID_Data { get; set; }
        //public List<CMB1>? ROUTE_NAME_Data { get; set; }
        //public List<TCOD_ANBAR>? ANBARCODE_Data { get; set; }
        //public List<SALA_DTL>? USER_NAME_Data { get; set; }
        //public List<Custom_DEPART>? DEPATMAN_Data { get; set; }
        //public List<TheSHIFT1>? SHIFT_ID_Data { get; set; }
        //public List<CUSTKIND>? CUST_COD_Data { get; set; }
        //public List<CMB2>? N_RASID_Data { get; set; }
        //public List<CMB3>? MM_Data { get; set; }
        #endregion

        #region LOCALMODEL
        public class DEPARTEMAN_MODEL : INotifyPropertyChanged, ICloneable
        {
            public object Clone() { return this.MemberwiseClone(); }
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
            private int? _depatman;
            public int? DEPATMAN { get => _depatman; set { if (_depatman == value) return; _depatman = value; OnPropertyChanged("DEPATMAN"); } }
            private string? _depname;
            public string? DEPNAME { get => _depname; set { if (_depname == value) return; _depname = value; OnPropertyChanged("DEPNAME"); } }
        }
        #endregion

        public bool isAdvancedF12 { get; set; } = true;

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        public static GridColumn FindColumn(SfDataGrid grid, string columnName)
        {
            if (grid == null || string.IsNullOrWhiteSpace(columnName))
                return null;

            #region MyRegion
            ////نوع برگه
            //if (string.Equals(columnName, "TAGCODE", StringComparison.OrdinalIgnoreCase)) columnName = "BARGAH";

            ////نام انبار
            //if (string.Equals(columnName, "ANBARCODE", StringComparison.OrdinalIgnoreCase)) columnName = "ANBNAME";

            ////نام گروه کالا
            //if (string.Equals(columnName, "GRPCODE", StringComparison.OrdinalIgnoreCase)) columnName = "GRPNAME";

            ////نام واحد کالا
            //if (string.Equals(columnName, "VAHCODE", StringComparison.OrdinalIgnoreCase)) columnName = "VAHEDNAME";

            ////واحد : دپارتمان
            //if (string.Equals(columnName, "DEPATMAN", StringComparison.OrdinalIgnoreCase)) columnName = "DEPNAME";

            ////شیفت
            //if (string.Equals(columnName, "SHIFT_ID", StringComparison.OrdinalIgnoreCase)) columnName = "SHNAME";

            ////شیفت
            //if (string.Equals(columnName, "SHIFT_ID", StringComparison.OrdinalIgnoreCase)) columnName = "SHNAME";
            #endregion

            var key = columnName.Trim();

            // 1) MappingName
            var col = grid.Columns.FirstOrDefault(c =>
                string.Equals(c.MappingName, key, StringComparison.OrdinalIgnoreCase));
            if (col != null) return col;

            // 2) HeaderText
            col = grid.Columns.FirstOrDefault(c =>
                string.Equals(c.HeaderText, key, StringComparison.OrdinalIgnoreCase));
            if (col != null) return col;

            // 3) x:Name (اگر ستون در XAML تعریف شده باشد)
            var byXamlName = grid.FindName(key) as GridColumn; // مثلا "TAGCODE"
            if (byXamlName != null) return byXamlName;

            return col;
        }
        private GridColumn FindColumnSafe(string name)
        {
            // اگر متد FindColumn خودت را داری همان را صدا بزن؛ در غیراینصورت MappingName را چک کن
            var col = SYNCFUSION_DG.Columns.FirstOrDefault(c => string.Equals(c.MappingName, name, StringComparison.OrdinalIgnoreCase));
            if (col != null) return col;

            // x:Name در صورت تعریف دستی
            return this.FindName(name) as GridColumn;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = ProcLoader.Start();
            FILL_ALL_COMBOBOXES();

            if (isAdvancedF12)
            {
                LABEL_HEADER.Content = "نتیجه جستجو در گردش کالا پـیشرفته";
            }
            else
            {
                LABEL_HEADER.Content = "نتیجه جستجو در گردش کالا";
            }

            List<string> NON = new List<string>();
            foreach (var columnName in ColumnSelectedPassed)
            {
                // پیدا کردن ستون با اسم مشخص
                var column = FindColumn(SYNCFUSION_DG, columnName);

                if (column != null)
                {
                    column.IsHidden = false;
                }
                else
                {
                    //NON.Add(columnName.Trim());
                }
            }

            var digits = Baseknow.DIG;
            void SetDigits(string colName)
            {
                var col = FindColumnSafe(colName);
                if (col is GridNumericColumn gnc)
                {
                    gnc.NumberDecimalDigits = (int)digits;
                }
            }
            SetDigits("MEGH");
            SetDigits("MEGHk");
            SetDigits("MEGH_MAR");

            FACTOR_DATA?.Clear();

            //CL_LMethods.DoWriteMyLog(SqlQueryPassed, default);

            var MasterHead = dbms.DoGetDataSQL<KALAS>(SqlQueryPassed).ToList();
            foreach (var item in MasterHead)
            {
                FACTOR_DATA.Add(item);
            }

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;


            if (isSummed)
            {
                GenerateAutomaticSummary(SYNCFUSION_DG);
            }

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            if (RestrictionMessages.Any())
            {
                LBL_STATE.Content = "دسترسی شما با این شرایط محدود شده است: " + string.Join(", ", RestrictionMessages);
                LBL_STATE.Visibility = Visibility.Visible;
            }
            else
            {
                LBL_STATE.Visibility = Visibility.Collapsed;
            }

            //ProcLoader.Stop(Prc);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //نوع برگه
            TAGCODE.ItemsSource = dbms.DoGetDataSQL<TAGCOD>($"SELECT CODE, BARGAH FROM TAGCOD").ToList();

            //واحد کالا
            VAHCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_VAHEDS>($"SELECT CODE, NAMES FROM TCOD_VAHEDS").ToList();

            //گروه کالا
            GRPCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_STUFGROUP>($"SELECT CODE, NAMES FROM TCOD_STUFGROUP").ToList();

            //استان
            var ALL_OSTAN = dbms.DoGetDataSQL<TCOD_OSTAN>("SELECT OSCODE, OSNAME FROM TCOD_OSTAN ORDER BY OSNAME").ToList();
            foreach (var item in ALL_OSTAN) { item.OSNAME = item.OSNAME?.FixPersianChars(); }
            OSTANID.ItemsSource = ALL_OSTAN; //Combobox Ui

            //کد شهر
            var ALL_SHAHR = dbms.DoGetDataSQL<TCOD_CITY>("SELECT CITYCODE, CITYNAME FROM TCOD_CITY ORDER BY CITYNAME").ToList();
            SHAHRID.ItemsSource = ALL_SHAHR;

            //مسیر ویزیت
            //ROUTE_NAME.ItemsSource = dbms.DoGetDataSQL<CMB1>($@"SELECT Visit_route.ROUTE_NAME, Visit_route.ROUTE_NAME+N' - '+CUST_HESAB.NAME+N' - '+CUST_HESAB.hes AS Expr1
            //                                                       FROM Visit_route
            //                                                            INNER JOIN CUST_HESAB ON Visit_route.HES=CUST_HESAB.hes
            //                                                       WHERE(Visit_route.RACTIVE=1)").ToList();
            //انبار
            ANBARCODE.ItemsSource = dbms.DoGetDataSQL<TCOD_ANBAR>($"SELECT CODE, NAMES FROM TCOD_ANBAR").ToList();

            ////کاربران
            //var RST_PERSONEL = dbms.DoGetDataSQL<SALA_DTL>("SELECT SAL_NAME, IDD FROM dbo.SALA_DTL WHERE (ENABL=0) ORDER BY IDD").ToList();
            //foreach (var rows in RST_PERSONEL)
            //{
            //    if (!string.IsNullOrEmpty(rows?.SAL_NAME))
            //    {
            //        rows.SAL_NAME = CL_HESABDARI.DECODEUN(rows.SAL_NAME);
            //    }
            //}
            //USER_NAME.ItemsSource = RST_PERSONEL;

            //واحد فروش
            DEPATMAN.ItemsSource = dbms.DoGetDataSQL<DEPARTEMAN_MODEL>("SELECT DEPATMAN,DEPNAME FROM dbo.DEPART ORDER BY DEPNAME").ToList(); //Custom_DEPART

            //شیفت
            SHIFT_ID.ItemsSource = dbms.DoGetDataSQL<TheSHIFT1>("SELECT SHIFT_ID, SHNAME FROM SHIFT ORDER BY SHIFT.SHNAME").ToList();

            //نوع مشتری
            CUST_COD.ItemsSource = dbms.DoGetDataSQL<CUSTKIND>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();

            //محل مصرف
            N_RASID_COLUMN.ItemsSource = dbms.DoGetDataSQL<CMB2>("SELECT dbo.HEAD_MANF.FNUMB, ISNULL(dbo.HEAD_MANF.NAMES, dbo.STUF_DEF.NAME) AS NAM FROM dbo.STUF_DEF RIGHT OUTER JOIN dbo.HEAD_MANF ON dbo.STUF_DEF.CODE = dbo.HEAD_MANF.CODE;").ToList();

            //ماه
            MM_COLUMN.ItemsSource = dbms.DoGetDataSQL<CMB3>("SELECT MON_ID, MON FROM MON").ToList();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as KALAS;

                if (currentRow != null && currentRow?.TAGCODE != null)
                {
                    int TAG_TYPE = (int)(currentRow?.TAGCODE);
                    double? TARGET_NUMBER = currentRow?.NUMBER ?? currentRow.NUMBER1;

                    //فاکتوری ها : 13 و 4 و 25 و27 و12 و 3
                    //چون kalas_sub ترتیب NUMBER1 , NUMBER درست نیست و جابهجا داره توی این ویو میاره , ما فعلا سمت سی شارپ میام جابه جا میدیم , تا فاکتور درست رو باز کنه
                    if (TAG_TYPE == 3 || TAG_TYPE == 12 || TAG_TYPE == 27 || TAG_TYPE == 25 || TAG_TYPE == 4 || TAG_TYPE == 13)
                    {
                        TARGET_NUMBER = currentRow.NUMBER1;
                    }
                    else
                    {
                        TARGET_NUMBER = currentRow.NUMBER;
                    }

                    CL_MenuManager.MenuBaseOnKindOpen(this, dbms, TAG_TYPE, TARGET_NUMBER, false);
                }

                return;
                switch (currentRow?.TAGCODE)
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

        private readonly FilterService<KALAS> filterService = new FilterService<KALAS>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                //این توی حالتی که کاربر از فیلتر SfDataGrid Filter استفاده کرده باشه درست کار نمیکنه !
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 1);
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

            //var dataGrid = SYNCFUSION_DG;
            //var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null && currentCell.IsEditing)
            //{
            //    System.Windows.Forms.SendKeys.SendWait("^(c)"); //Fire Send Keys : Ctrl + C
            //    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 1);
            //    return;
            //}

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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as KALAS);
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

            var dataType = typeof(KALAS);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns)
            {
                var propertyInfo = typeof(KALAS).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                //if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
                if (CheckField(column.MappingName.ToUpper()))
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

        private bool CheckField(string fieldName)
        {
            var numericFields = new[]
            {
                "MEGH", "MEGHk", "MABL", "MABL_K", "N_KOL", "N_MOIN", "IMBAA",
                "MEGH_MAR", "MAS", "N_RASID", "KHFR", "GHFR", "N_TAF", "TOTALARZ",
                "TAMIR", "MIN_M", "MAX_M", "N_SEF", "B_SEF", "MABL_F", "AVRAGE",
                "MABRIAL", "VAZN", "TKHN"
            };

            return numericFields.Contains(fieldName.ToUpper());
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
                #endregion

        private void BTN_ISEND_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = SYNCFUSION_DG.SelectedItem as KALAS;

            if (CurrentRow != null && CurrentRow?.NUMBER != null && CurrentRow?.NUMBER > 0)
            {
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_MOADIAN_SINGLE, this, Convert.ToDouble(CurrentRow.NUMBER));
            }
        }
    }
}
