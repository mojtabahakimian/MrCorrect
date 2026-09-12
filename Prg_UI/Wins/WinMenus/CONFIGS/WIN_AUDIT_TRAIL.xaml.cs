using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.AUDIT;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    /// <summary>
    /// مشاهده و ردیابی خطی فعالیت کاربران.
    ///
    /// همه چیز سمت سرور فیلتر و صفحه‌بندی می‌شود؛ هیچ‌وقت کل جدول به حافظه
    /// نمی‌آید. صفحه‌بندی به‌جای OFFSET با کلید (LOG_ID &lt; آخرین) انجام
    /// می‌شود تا صفحه‌های عمیق هم به همان سرعت صفحه‌ی اول باشند.
    /// </summary>
    public partial class WIN_AUDIT_TRAIL : Window
    {
        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();

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

        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        private const int PageSize = 300;

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private long? _lastLogId;
        private bool _busy;
        private bool _onlyImportant = true;

        UniversControl universControl = new UniversControl();
        private readonly FilterService<AuditTrailRow> filterService = new FilterService<AuditTrailRow>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;

        public bool NowIsReady { get; private set; }
        public ObservableCollection<AuditTrailRow> Rows { get; } = new ObservableCollection<AuditTrailRow>();

        public WIN_AUDIT_TRAIL()
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

        /// <summary>
        /// نام فرم در جدول TFORMS. برای فعال شدن این پنجره باید یک ردیف با
        /// همین نام در TFORMS و دسترسی متناظر در SAL_CHEK ساخته شود.
        /// </summary>
        private const string PermissionFormName = "AUDITTRAIL";

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CL_HESABDARI.LETSGO(PermissionFormName))
            {
                Prg_Proccessy.AUDIT.Audit.Security(
                    AuditAction.AuditViewed,
                    "تلاش برای مشاهده‌ی سوابق بدون دسترسی",
                    formName: this.GetType().Name);

                new Msgwin(false,
                    $"دسترسی مشاهده‌ی سوابق برای شما تعریف نشده است.\n" +
                    $"مدیر سیستم باید فرم «{PermissionFormName}» را در TFORMS و دسترسی آن را در SAL_CHEK تعریف کند.")
                    .ShowDialog();

                this.Close();
                return;
            }

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, ev) => UpdateRowCountLabel();
                UpdateRowCountLabel();
            }

            if (GR_NAV_DATAGRID != null)
            {
                GR_NAV_DATAGRID.ReGetDataAction = () =>
                {
                    _ = SearchAsync(reset: true);
                };
            }

            FillStaticCombos();

            var today = DateTime.Now;
            TXT_FROM.Text = ToShamsiText(today.AddDays(-7));
            TXT_TO.Text = ToShamsiText(today);

            await FillUsersAsync();

            Audit.Security(AuditAction.AuditViewed, "مشاهده‌ی سوابق فعالیت کاربران",
                           formName: this.GetType().Name);

            await SearchAsync(reset: true);
        }

        private async void BTN_MODE_TOGGLE_Click(object sender, RoutedEventArgs e)
        {
            _onlyImportant = !_onlyImportant;
            BTN_MODE_TOGGLE.Content = _onlyImportant ? "نمایش کلیه سوابق" : "نمایش رکوردهای مهم";
            await SearchAsync(reset: true);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
            if (e.Key == Key.F5) { e.Handled = true; _ = SearchAsync(reset: true); }

            // Navigation shortcuts
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.Home)
                {
                    e.Handled = true;
                    GR_NAV_DATAGRID?.GoFirst();
                }
                else if (e.Key == Key.End)
                {
                    e.Handled = true;
                    GR_NAV_DATAGRID?.GoLast();
                }
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                if (e.Key == Key.Left)
                {
                    e.Handled = true;
                    GR_NAV_DATAGRID?.GoNext();
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true;
                    GR_NAV_DATAGRID?.GoPrevious();
                }
            }
        }

        #region SYNCFUSION_DATA_GRID
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }

        private void UpdateRowCountLabel()
        {
            if (ROWCOUNT_TEXTBLK == null) return;
            if (SYNCFUSION_DG?.View == null)
            {
                ROWCOUNT_TEXTBLK.Text = Rows.Count.ToString();
            }
            else
            {
                var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;
                ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
            }
            GR_NAV_DATAGRID?.UpdateNavigationDisplay();
        }

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return;
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }

        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
        }

        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex;
            CurrentCellValue = null;

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0 || columnIndex >= this.SYNCFUSION_DG.Columns.Count) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName;
            if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0 || recordIndex >= this.SYNCFUSION_DG.View.Records.Count) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
            if (record == null) return;

            var propertyInfo = record.GetType().GetProperty(mappingName);
            if (propertyInfo == null) return;

            var propertyValue = propertyInfo.GetValue(record);
            CurrentCellValue = propertyValue?.ToStringNullSafe() ?? string.Empty;
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
                {
                    return editingElement.SelectedText;
                }
            }

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

        private (string? ColumnName, object? FilterValue) GetSelectedCellDetails()
        {
            if (SYNCFUSION_DG.SelectionController?.CurrentCellManager?.CurrentCell != null)
            {
                var columnName = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName;
                return (columnName, CurrentCellValue);
            }
            return (null, null);
        }

        private void ApplyCumulativeFilter()
        {
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as AuditTrailRow);
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

            if (!string.IsNullOrEmpty(selectedText))
            {
                filterService.AddFilter(columnName, selectedText, isExclusion: false, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Contains \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            if (filterValue != null)
            {
                filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: true);
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} = {displayValue}");
                ApplyCumulativeFilter();
            }
            else
            {
                filterService.AddFilter(columnName, null, isExclusion: false, isExactMatch: true);
                ActiveFilters.Add($"{columnName} = NULL");
                ApplyCumulativeFilter();
            }
        }

        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (!string.IsNullOrEmpty(selectedText))
            {
                filterService.AddFilter(columnName, selectedText, isExclusion: true, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Does Not Contain \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            if (filterValue != null)
            {
                filterService.AddFilter(columnName, filterValue, isExclusion: true, isExactMatch: true);
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} != {displayValue}");
                ApplyCumulativeFilter();
            }
            else
            {
                filterService.AddFilter(columnName, null, isExclusion: true, isExactMatch: true);
                ActiveFilters.Add($"{columnName} != NULL");
                ApplyCumulativeFilter();
            }
        }

        private string FormatValueForDisplay(object? value)
        {
            if (value == null) return "NULL";

            if (value is double || value is decimal || value is float)
            {
                try { return Convert.ToDecimal(value).ToString("N", CultureInfo.InvariantCulture); }
                catch { return value.ToString() ?? string.Empty; }
            }

            if (value is int || value is long || value is short || value is byte)
            {
                try { return Convert.ToInt64(value).ToString("N0", CultureInfo.InvariantCulture); }
                catch { return value.ToString() ?? string.Empty; }
            }

            return value.ToString() ?? string.Empty;
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
                    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 1);
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

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp("... در حال اماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "AuditTrailExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }

        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e)
        {
            filterService.ClearFilters();
            ActiveFilters.Clear();
            ApplyCumulativeFilter();
        }

        private void HideCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            var currentColumn = SYNCFUSION_DG.SelectionController?
                .CurrentCellManager?.CurrentCell?.GridColumn;

            if (currentColumn == null)
            {
                universControl.PopNotifyShow("ابتدا یک سلول از ستون مورد نظر را انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (SYNCFUSION_DG.Columns.Count(column => !column.IsHidden) <= 1)
            {
                universControl.PopNotifyShow("آخرین ستون قابل نمایش را نمی‌توان مخفی کرد.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            currentColumn.IsHidden = true;
        }

        private void ShowAllColumns_Click(object sender, RoutedEventArgs e)
        {
            foreach (var column in SYNCFUSION_DG.Columns.Where(column => column.IsHidden))
            {
                column.IsHidden = false;
            }
        }

        private void ViewDetail_Click(object? sender, EventArgs? e)
        {
            var row = SYNCFUSION_DG.SelectedItem as AuditTrailRow;
            if (row == null)
            {
                universControl.PopNotifyShow("ابتدا یک سطر را انتخاب کنید.", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"شناسه: {row.LOG_ID}");
            sb.AppendLine($"تاریخ و ساعت: {row.DateText} {row.TimeText}");
            sb.AppendLine($"کاربر: {row.UserText}");
            sb.AppendLine($"دسته: {row.CategoryText} | عملیات: {row.ActionText}");
            sb.AppendLine($"وضعیت: {row.StatusText} | اهمیت: {row.SeverityText}");
            sb.AppendLine($"شرح رویداد: {row.TITLE}");
            if (!string.IsNullOrWhiteSpace(row.DocumentText))
                sb.AppendLine($"سند / موجودیت: {row.DocumentText}");
            if (!string.IsNullOrWhiteSpace(row.FORM_NAME))
                sb.AppendLine($"فرم: {row.FORM_NAME}");
            sb.AppendLine($"نام کامپیوتر: {row.MACHINE_NAME} | آدرس IP: {row.CLIENT_IP}");
            sb.AppendLine($"کاربر ویندوز: {row.WIN_USER} | نسخه: {row.APP_VERSION}");
            sb.AppendLine();
            sb.AppendLine("جزئیات و تغییرات:");
            sb.AppendLine(!string.IsNullOrWhiteSpace(row.DETAIL) ? row.DETAIL : "(بدون جزئیات تکمیلی)");

            new Msgwin(true, sb.ToString()).ShowDialog();
        }

        private void SYNCFUSION_DG_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            ViewDetail_Click(sender, e);
        }
        #endregion

        private void FillStaticCombos()
        {
            CMB_CATEGORY.ItemsSource = new List<LookupItem>
            {
                new LookupItem(null, "همه"),
                new LookupItem((byte)AuditCategory.Navigation, "باز کردن فرم"),
                new LookupItem((byte)AuditCategory.Data,       "تغییر داده"),
                new LookupItem((byte)AuditCategory.Business,   "عملیات کاری"),
                new LookupItem((byte)AuditCategory.Report,     "چاپ و خروجی"),
                new LookupItem((byte)AuditCategory.Auth,       "ورود و خروج"),
                new LookupItem((byte)AuditCategory.Security,   "امنیتی"),
            };
            CMB_CATEGORY.SelectedIndex = 0;

            CMB_ACTION.ItemsSource = new List<TextLookupItem>
            {
                new TextLookupItem(null, "همه"),
                new TextLookupItem(AuditAction.OpenForm, "باز کردن فرم"),
                new TextLookupItem(AuditAction.Insert,   "ثبت"),
                new TextLookupItem(AuditAction.Update,   "ویرایش"),
                new TextLookupItem(AuditAction.Delete,   "حذف"),
                new TextLookupItem(AuditAction.Sign,     "امضا"),
                new TextLookupItem(AuditAction.Unsign,   "برداشتن امضا"),
                new TextLookupItem(AuditAction.Print,    "چاپ"),
                new TextLookupItem(AuditAction.Preview,  "پیش‌نمایش"),
                new TextLookupItem(AuditAction.Export,   "خروجی"),
                new TextLookupItem(AuditAction.Convert,  "تبدیل"),
                new TextLookupItem(AuditAction.Confirm,  "تایید"),
                new TextLookupItem(AuditAction.Login,    "ورود"),
                new TextLookupItem(AuditAction.LoginFailed, "ورود ناموفق"),
                new TextLookupItem(AuditAction.Logout,   "خروج"),
            };
            CMB_ACTION.SelectedIndex = 0;
        }

        private async Task FillUsersAsync()
        {
            try
            {
                var users = (await dbms.DoGetDataSQLAsync<UserItem>(
                    @"SELECT DISTINCT [USER_ID] AS IDD, [USER_NAME] AS SAL_NAME
                        FROM [dbo].[SYS_AUDIT_SESSION]
                       WHERE [USER_ID] IS NOT NULL
                       ORDER BY [USER_NAME]")).ToList();

                users.Insert(0, new UserItem { IDD = null, SAL_NAME = "همه کاربران" });
                CMB_USER.ItemsSource = users;
                CMB_USER.SelectedIndex = 0;
            }
            catch (Exception)
            {
                CMB_USER.ItemsSource = new List<UserItem> { new UserItem { IDD = null, SAL_NAME = "همه کاربران" } };
                CMB_USER.SelectedIndex = 0;
            }
        }

        private async void BTN_SEARCH_Click(object sender, RoutedEventArgs e) => await SearchAsync(reset: true);

        private async void BTN_MORE_Click(object sender, RoutedEventArgs e) => await SearchAsync(reset: false);

        private async Task SearchAsync(bool reset)
        {
            if (_busy) return;
            _busy = true;

            if (BusyOverlay != null) BusyOverlay.Visibility = Visibility.Visible;

            try
            {
                BTN_SEARCH.IsEnabled = false;
                if (BTN_MODE_TOGGLE != null) BTN_MODE_TOGGLE.IsEnabled = false;
                BTN_MORE.IsEnabled = false;
                LBL_STATUS.Text = "در حال خواندن ...";

                if (reset)
                {
                    Rows.Clear();
                    _lastLogId = null;
                }

                var from = ParseShamsi(TXT_FROM.Text);
                var to = ParseShamsi(TXT_TO.Text);

                if (from is null || to is null)
                {
                    LBL_STATUS.Text = "تاریخ نامعتبر است. قالب درست: 1405/05/17";
                    return;
                }
                if (from > to)
                {
                    LBL_STATUS.Text = "«از تاریخ» بزرگ‌تر از «تا تاریخ» است.";
                    return;
                }

                var fromDate = from.Value;
                var toDate = to.Value.AddDays(1);

                var args = new
                {
                    Take = PageSize,
                    UserId = (CMB_USER.SelectedValue as int?),
                    From = fromDate,
                    To = toDate,
                    Category = (CMB_CATEGORY.SelectedValue as byte?),
                    Action = string.IsNullOrWhiteSpace(CMB_ACTION.SelectedValue as string)
                             ? null : (string?)CMB_ACTION.SelectedValue,
                    Doc = BuildLike(TXT_DOC.Text),
                    Search = BuildLike(TXT_SEARCH.Text),
                    OnlySensitive = CHK_SENSITIVE.IsChecked == true,
                    OnlyImportant = _onlyImportant && (CMB_CATEGORY.SelectedValue as byte?) == null && string.IsNullOrWhiteSpace(CMB_ACTION.SelectedValue as string),
                    AfterId = _lastLogId,
                };

                const string sql = @"
SELECT TOP (@Take)
       [LOG_ID], [AT_CLIENT], [AT_SERVER], [DATE_S], [TIME_S],
       [USER_ID], [USER_NAME], [CATEGORY], [SEVERITY], [ACTION],
       [ENTITY], [ENTITY_KEY], [FORM_NAME], [TITLE], [DETAIL],
       [IS_SUCCESS], [SESSION_ID], [MACHINE_NAME], [CLIENT_IP],
       [WIN_USER], [APP_VERSION]
  FROM [dbo].[VW_SYS_AUDIT_TIMELINE]
 WHERE [AT_SERVER] >= @From
   AND [AT_SERVER] <  @To
   AND (@UserId   IS NULL OR [USER_ID]  = @UserId)
   AND (@Category IS NULL OR [CATEGORY] = @Category)
   AND (@Action   IS NULL OR [ACTION]   = @Action)
   AND (@Doc      IS NULL OR [ENTITY_KEY] LIKE @Doc OR [ENTITY] LIKE @Doc)
   AND (@Search   IS NULL OR [TITLE]      LIKE @Search)
   AND (@OnlySensitive = 0 OR [SEVERITY] = 3)
   AND (@OnlyImportant = 0 OR [SEVERITY] >= 2 OR [CATEGORY] <> 1)
   AND (@AfterId  IS NULL OR [LOG_ID] < @AfterId)
 ORDER BY [LOG_ID] DESC
 OPTION (RECOMPILE)";

                var page = (await dbms.DoGetDataSQLAsync<AuditTrailRow>(sql, args)).ToList();

                foreach (var r in page) Rows.Add(r);

                if (page.Count > 0) _lastLogId = page[page.Count - 1].LOG_ID;

                BTN_MORE.IsEnabled = page.Count == PageSize;

                UpdateRowCountLabel();

                if (page.Count == 0 && Rows.Count == 0)
                {
                    LBL_STATUS.Text = AuditService.SchemaReady
                        ? "رکوردی یافت نشد."
                        : "رکوردی یافت نشد — ساختار جدول‌های سابقه ساخته نشده است. " +
                          "کاربر SQL باید دسترسی ایجاد جدول داشته باشد، یا مایگریشن ScriptSqly اجرا شود.";
                }
                else
                {
                    string modeHint = _onlyImportant ? " [فقط رویدادهای مهم]" : " [کلیه سوابق]";
                    LBL_STATUS.Text =
                        $"{Rows.Count:N0} رکورد نمایش داده شد" +
                        modeHint +
                        (BTN_MORE.IsEnabled ? " (رکورد بیشتری هست)" : " (پایان نتایج)") +
                        (AuditService.DroppedCount > 0
                            ? $"  |  ⚠ {AuditService.DroppedCount:N0} رویداد در این نشست به‌دلیل پر شدن صف ثبت نشد"
                            : string.Empty);
                }
            }
            catch (Exception ex)
            {
                LBL_STATUS.Text = "خطا در خواندن سوابق: " + ex.Message;
                BTN_MORE.IsEnabled = _lastLogId.HasValue;
            }
            finally
            {
                if (BusyOverlay != null) BusyOverlay.Visibility = Visibility.Collapsed;
                BTN_SEARCH.IsEnabled = true;
                if (BTN_MODE_TOGGLE != null) BTN_MODE_TOGGLE.IsEnabled = true;
                _busy = false;
            }
        }

        private static string ToShamsiText(DateTime value)
        {
            var n = Audit.ToPersianDateNumber(value);
            return $"{n / 10000:0000}/{(n / 100) % 100:00}/{n % 100:00}";
        }

        private static string? BuildLike(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var t = CL_LMethods.NormalizeDigits(text).Trim();
            var sb = new StringBuilder(t.Length + 8);
            sb.Append('%');
            foreach (var c in t)
            {
                if (c is '%' or '_' or '[') sb.Append('[').Append(c).Append(']');
                else sb.Append(c);
            }
            sb.Append('%');
            return sb.ToString();
        }

        private static DateTime? ParseShamsi(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var normalized = CL_LMethods.NormalizeDigits(text);
            var digits = new string(normalized.Where(c => c >= '0' && c <= '9').ToArray());
            if (digits.Length != 8) return null;
            if (!int.TryParse(digits, out var n)) return null;

            return Audit.FromPersianDateNumber(n);
        }

        public sealed class UserItem
        {
            public int? IDD { get; set; }
            public string? SAL_NAME { get; set; }
        }

        public sealed class LookupItem
        {
            public LookupItem(byte? value, string title) { Value = value; Title = title; }
            public byte? Value { get; }
            public string Title { get; }
        }

        public sealed class TextLookupItem
        {
            public TextLookupItem(string? value, string title) { Value = value; Title = title; }
            public string? Value { get; }
            public string Title { get; }
        }
    }

    public sealed class AuditTrailRow
    {
        public long LOG_ID { get; set; }
        public DateTime? AT_CLIENT { get; set; }
        public DateTime AT_SERVER { get; set; }
        public int? DATE_S { get; set; }
        public int? TIME_S { get; set; }
        public int? USER_ID { get; set; }
        public string? USER_NAME { get; set; }
        public byte CATEGORY { get; set; }
        public byte SEVERITY { get; set; }
        public string? ACTION { get; set; }
        public string? ENTITY { get; set; }
        public string? ENTITY_KEY { get; set; }
        public string? FORM_NAME { get; set; }
        public string? TITLE { get; set; }
        public string? DETAIL { get; set; }
        public bool IS_SUCCESS { get; set; }
        public Guid? SESSION_ID { get; set; }
        public string? MACHINE_NAME { get; set; }
        public string? CLIENT_IP { get; set; }
        public string? WIN_USER { get; set; }
        public string? APP_VERSION { get; set; }

        public string DateText
        {
            get
            {
                var n = DATE_S.GetValueOrDefault() > 0
                    ? DATE_S!.Value
                    : Audit.ToPersianDateNumber(AT_CLIENT ?? AT_SERVER);
                return n <= 0 ? string.Empty : $"{n / 10000:0000}/{(n / 100) % 100:00}/{n % 100:00}";
            }
        }

        public string TimeText
        {
            get
            {
                var t = TIME_S.GetValueOrDefault() > 0
                    ? TIME_S!.Value
                    : Audit.ToTimeNumber(AT_CLIENT ?? AT_SERVER);
                return $"{t / 10000:00}:{(t / 100) % 100:00}:{t % 100:00}";
            }
        }

        public string UserText =>
            !string.IsNullOrWhiteSpace(USER_NAME)
                ? USER_NAME
                : (!string.IsNullOrWhiteSpace(WIN_USER) ? $"{WIN_USER} (ویندوز)" : "سیستم");

        public string CategoryText => CATEGORY switch
        {
            1 => "فرم",
            2 => "داده",
            3 => "عملیات",
            4 => "چاپ",
            5 => "ورود/خروج",
            6 => "امنیتی",
            _ => "سایر",
        };

        public string ActionText => (ACTION ?? string.Empty) switch
        {
            AuditAction.OpenForm => "باز کردن فرم",
            AuditAction.CloseForm => "بستن فرم",
            AuditAction.Insert => "ثبت",
            AuditAction.Update => "ویرایش",
            AuditAction.Delete => "حذف",
            AuditAction.Sign => "امضا",
            AuditAction.Unsign => "برداشتن امضا",
            AuditAction.Approve => "تایید",
            AuditAction.Unapprove => "لغو تایید",
            AuditAction.Convert => "تبدیل",
            AuditAction.Confirm => "تایید",
            AuditAction.Cancel => "لغو",
            AuditAction.Preview => "پیش‌نمایش",
            AuditAction.Print => "چاپ",
            AuditAction.Export => "خروجی",
            AuditAction.Login => "ورود",
            AuditAction.LoginFailed => "ورود ناموفق",
            AuditAction.Logout => "خروج",
            AuditAction.PasswordChange => "تغییر رمز",
            AuditAction.PermissionChange => "تغییر دسترسی",
            AuditAction.AuditViewed => "مشاهده سوابق",
            AuditAction.ExecProcedure => "اجرای رویه",
            "SaveRow" => "ذخیره سطر",
            "REPLACE HESAB" => "جایگزینی حساب",
            "MOADIAN SEND BUTTON CALLED IN F4" => "ارسال به سامانه مودیان",
            _ => ACTION ?? string.Empty,
        };

        public string StatusText => IS_SUCCESS ? "موفق" : "ناموفق";

        public string SeverityText => SEVERITY switch
        {
            1 => "عادی",
            2 => "مهم",
            3 => "حساس",
            _ => "عادی",
        };

        public string DocumentText =>
            string.IsNullOrWhiteSpace(ENTITY)
                ? (string.IsNullOrWhiteSpace(ENTITY_KEY) ? "-" : ENTITY_KEY)
                : (string.IsNullOrWhiteSpace(ENTITY_KEY) ? ENTITY : $"{ENTITY} {ENTITY_KEY}".Trim());
    }
}
