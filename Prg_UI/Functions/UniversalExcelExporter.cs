using OfficeOpenXml;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;

namespace Functions
{
    public static class UniversalExcelExporter
    {
        private static readonly object _lockObject = new object();

        // Cache PropertyInfo lookups to avoid repeated reflection per cell
        private static readonly ConcurrentDictionary<(Type type, string propName), PropertyInfo> _propCache =
            new ConcurrentDictionary<(Type, string), PropertyInfo>();

        private static PropertyInfo GetCachedProp(Type type, string propName) =>
            _propCache.GetOrAdd((type, propName), k => k.type.GetProperty(k.propName));

        public static async Task ExportToExcelAsync(object grid, string fileName = null, bool openAfterExport = true, bool KeepTypeFormat = true)
        {
            try
            {
                if (grid == null)
                    throw new ArgumentNullException(nameof(grid));

                string filePath = GetUniqueFilePath(fileName);

                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        if (grid is DataGrid wpfDataGrid)
                        {
                            if (KeepTypeFormat)
                            {
                                ExportWpfDataGrid(wpfDataGrid, filePath);
                            }
                            else
                            {
                                ExportWpfDataGridDisformated(wpfDataGrid, filePath);
                            }
                        }
                        else if (grid is SfDataGrid syncfusionGrid)
                        {
                            ExportSyncfusionDataGrid(syncfusionGrid, filePath, KeepTypeFormat);
                        }
                        else
                        {
                            throw new ArgumentException("Unsupported grid type", nameof(grid));
                        }
                    }
                });

                if (openAfterExport)
                {
                    await OpenExcelFile(filePath);
                }
            }
            catch (Exception ex)
            {
                HandleExportError(ex);
            }
        }

        private static string GetDisplayMemberPathValue(ComboBox comboBox)
        {
            if (comboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(comboBox.DisplayMemberPath))
            {
                var selectedItem = comboBox.SelectedItem;
                var propertyInfo = selectedItem.GetType().GetProperty(comboBox.DisplayMemberPath);

                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(selectedItem);
                    return value?.ToStringNullSafe() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static object GetCellValue(object item, DataGridColumn column)
        {
            try
            {
                if (column is DataGridBoundColumn boundColumn)
                {
                    var binding = boundColumn.Binding as System.Windows.Data.Binding;
                    if (binding != null)
                    {
                        var property = item.GetType().GetProperty(binding.Path.Path);
                        return property?.GetValue(item) ?? "";
                    }
                }
                else if (column is DataGridComboBoxColumn comboBoxColumn)
                {
                    // Extract cell content based on the corresponding column
                    var content = column.GetCellContent(item);
                    if (content is ComboBox comboBox)
                    {
                        return GetDisplayMemberPathValue(comboBox); // Accessing the selected item
                    }
                    else
                    {
                        // Instead, get the value directly from the bound property
                        var binding = comboBoxColumn.SelectedValueBinding as Binding;
                        if (binding != null && !string.IsNullOrEmpty(binding.Path.Path))
                        {
                            var property = item.GetType().GetProperty(binding.Path.Path);
                            if (property != null)
                            {
                                var value = property.GetValue(item);

                                // If there's display text we need to show instead of raw value
                                if (value != null && comboBoxColumn.ItemsSource != null &&
                                    !string.IsNullOrEmpty(comboBoxColumn.DisplayMemberPath) &&
                                    !string.IsNullOrEmpty(comboBoxColumn.SelectedValuePath))
                                {
                                    // Try to find matching item in the ItemsSource
                                    foreach (var comboItem in comboBoxColumn.ItemsSource)
                                    {
                                        var valueProperty = comboItem.GetType().GetProperty(comboBoxColumn.SelectedValuePath);
                                        if (valueProperty != null)
                                        {
                                            var itemValue = valueProperty.GetValue(comboItem);
                                            if (itemValue != null && itemValue.Equals(value))
                                            {
                                                var displayProperty = comboItem.GetType().GetProperty(comboBoxColumn.DisplayMemberPath);
                                                if (displayProperty != null)
                                                {
                                                    var displayValue = displayProperty.GetValue(comboItem);
                                                    return displayValue?.ToString() ?? string.Empty;
                                                }
                                            }
                                        }
                                    }
                                }

                                return value?.ToString() ?? string.Empty;
                            }
                        }
                    }
                    return ""; // Default return if ComboBox is not found
                }
                else if (column is DataGridTemplateColumn)
                {
                    // Extract content from the template column
                    var content = column.GetCellContent(item);
                    if (content is TextBlock textBlock)
                    {
                        return textBlock.Text; // If the template contains a TextBlock
                    }
                    else if (content is CheckBox checkBox)
                    {
                        return checkBox.IsChecked ?? false; // If the template contains a CheckBox
                    }
                    else if (content is ComboBox comboBox)
                    {
                        return comboBox.SelectedValue ?? comboBox.Text; // For ComboBox, use SelectedValue or Text
                    }
                    else if (content is FrameworkElement frameworkElement)
                    {
                        // Handle other controls, extract their "value" as needed
                        var valueProperty = frameworkElement.GetType().GetProperty("Value");
                        return valueProperty?.GetValue(frameworkElement) ?? "";
                    }

                    return "";
                }


                return "";
            }
            catch
            {
                return "";
            }
        }

        private static void ExportWpfDataGrid(DataGrid dataGrid, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var headers = new List<string>();
            object[,] dataArray = null;
            var dateColIndices = new HashSet<int>();

            dataGrid.Dispatcher.Invoke(() =>
            {
                // Pre-build visible column metadata once (binding path extracted per column, not per row)
                var visibleCols = new List<(DataGridColumn col, string bindingPath)>();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var col = dataGrid.Columns[i];
                    if (col.Visibility != Visibility.Visible) continue;
                    headers.Add(col.Header?.ToString() ?? $"Column {i + 1}");
                    string path = null;
                    if (col is DataGridBoundColumn bc && bc.Binding is Binding b)
                        path = b.Path.Path;
                    else if (col is DataGridComboBoxColumn cbc && cbc.SelectedValueBinding is Binding svb)
                        path = svb.Path.Path;
                    visibleCols.Add((col, path));
                }

                var selectedSet = new HashSet<object>(dataGrid.SelectedItems.Cast<object>());
                var orderedItems = dataGrid.Items.Cast<object>().Where(x => selectedSet.Contains(x)).ToList();
                if (orderedItems.Count == 0 || visibleCols.Count == 0) return;

                int rowCount = orderedItems.Count;
                int colCount = visibleCols.Count;
                dataArray = new object[rowCount, colCount];

                // Cache PropertyInfos once from first row type
                Type rowType = orderedItems[0].GetType();
                var propInfos = visibleCols
                    .Select(vc => vc.bindingPath != null ? GetCachedProp(rowType, vc.bindingPath) : null)
                    .ToArray();

                for (int r = 0; r < rowCount; r++)
                {
                    var item = orderedItems[r];
                    for (int c = 0; c < colCount; c++)
                    {
                        object val = propInfos[c] != null
                            ? propInfos[c].GetValue(item) ?? ""
                            : GetCellValue(item, visibleCols[c].col);

                        if (val is bool bv)
                            val = bv ? "True" : "False";
                        else if (val is DateTime)
                            dateColIndices.Add(c);

                        dataArray[r, c] = val;
                    }
                }
            });

            if (dataArray == null) return;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.View.RightToLeft = true;

                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                int rowCount = dataArray.GetLength(0);
                int colCount = dataArray.GetLength(1);
                if (rowCount > 0)
                {
                    // Bulk write: orders of magnitude faster than cell-by-cell for large datasets
                    worksheet.Cells[2, 1, rowCount + 1, colCount].Value = dataArray;

                    // Apply date format per column range (not per cell)
                    foreach (int ci in dateColIndices)
                        worksheet.Cells[2, ci + 1, rowCount + 1, ci + 1].Style.Numberformat.Format = "yyyy/mm/dd hh:mm:ss";
                }

                worksheet.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static void ExportWpfDataGridDisformated(DataGrid dataGrid, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var headers = new List<string>();
            object[,] dataArray = null;

            dataGrid.Dispatcher.Invoke(() =>
            {
                var visibleCols = new List<(DataGridColumn col, string bindingPath)>();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var col = dataGrid.Columns[i];
                    if (col.Visibility != Visibility.Visible) continue;
                    headers.Add(col.Header?.ToString() ?? $"Column {i + 1}");
                    string path = null;
                    if (col is DataGridBoundColumn bc && bc.Binding is Binding b)
                        path = b.Path.Path;
                    else if (col is DataGridComboBoxColumn cbc && cbc.SelectedValueBinding is Binding svb)
                        path = svb.Path.Path;
                    visibleCols.Add((col, path));
                }

                var selectedSet = new HashSet<object>(dataGrid.SelectedItems.Cast<object>());
                var orderedItems = dataGrid.Items.Cast<object>().Where(x => selectedSet.Contains(x)).ToList();
                if (orderedItems.Count == 0 || visibleCols.Count == 0) return;

                int rowCount = orderedItems.Count;
                int colCount = visibleCols.Count;
                dataArray = new object[rowCount, colCount];

                Type rowType = orderedItems[0].GetType();
                var propInfos = visibleCols
                    .Select(vc => vc.bindingPath != null ? GetCachedProp(rowType, vc.bindingPath) : null)
                    .ToArray();

                for (int r = 0; r < rowCount; r++)
                {
                    var item = orderedItems[r];
                    for (int c = 0; c < colCount; c++)
                    {
                        object val = propInfos[c] != null
                            ? propInfos[c].GetValue(item)
                            : GetCellValue(item, visibleCols[c].col);
                        dataArray[r, c] = val?.ToString() ?? "";
                    }
                }
            });

            if (dataArray == null) return;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.View.RightToLeft = true;

                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                int rowCount = dataArray.GetLength(0);
                int colCount = dataArray.GetLength(1);
                if (rowCount > 0)
                    worksheet.Cells[2, 1, rowCount + 1, colCount].Value = dataArray;

                worksheet.Cells.AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var descendant = FindChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        /// <summary>
        /// متد جدید برای گرفتن مقدار از ستون‌های SfDataGrid
        /// </summary>
        /// <param name="record">رکورد داده</param>
        /// <param name="column">ستون</param>
        /// <param name="useDisplayValue">آیا از مقدار نمایشی استفاده شود (برای ComboBox)</param>
        /// <returns>مقدار سلول به صورت object</returns>
        private static object GetSyncfusionCellValue(object record, GridColumnBase column, bool useDisplayValue = true)
        {
            try
            {
                // بررسی اینکه آیا ستون از نوع GridComboBoxColumn است
                if (column is GridComboBoxColumn comboColumn)
                {
                    // دریافت مقدار خام از property
                    var propertyInfo = record.GetType().GetProperty(column.MappingName);
                    if (propertyInfo != null)
                    {
                        var rawValue = propertyInfo.GetValue(record);

                        // اگر باید از display value استفاده کنیم
                        if (useDisplayValue && comboColumn.ItemsSource != null &&
                            !string.IsNullOrWhiteSpace(comboColumn.DisplayMemberPath) &&
                            !string.IsNullOrWhiteSpace(comboColumn.SelectedValuePath))
                        {
                            // جستجو در ItemsSource برای پیدا کردن مقدار نمایشی
                            foreach (var item in comboColumn.ItemsSource)
                            {
                                if (item == null) continue;

                                // دریافت property مربوط به SelectedValuePath
                                var valueProperty = item.GetType().GetProperty(comboColumn.SelectedValuePath);
                                if (valueProperty != null)
                                {
                                    var itemValue = valueProperty.GetValue(item);

                                    // مقایسه مقدار
                                    if (itemValue != null && rawValue != null)
                                    {
                                        // تبدیل هر دو به string برای مقایسه ایمن
                                        string itemValueStr = itemValue.ToString();
                                        string rawValueStr = rawValue.ToString();

                                        if (itemValueStr.Equals(rawValueStr, StringComparison.OrdinalIgnoreCase))
                                        {
                                            // دریافت مقدار نمایشی
                                            var displayProperty = item.GetType().GetProperty(comboColumn.DisplayMemberPath);
                                            if (displayProperty != null)
                                            {
                                                var displayValue = displayProperty.GetValue(item);
                                                return displayValue ?? string.Empty;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // اگر مقدار نمایشی پیدا نشد یا نباید استفاده شود، مقدار خام را برگردان
                        return rawValue ?? string.Empty;
                    }
                }
                else
                {
                    // برای ستون‌های معمولی، مقدار را به صورت مستقیم برگردان
                    if (!string.IsNullOrWhiteSpace(column.MappingName))
                    {
                        var propertyInfo = record.GetType().GetProperty(column.MappingName);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(record);
                            return value ?? string.Empty;
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Debug.WriteLine($"Error getting cell value: {ex.Message}");
                return string.Empty;
            }
        }

        private static void ExportSyncfusionDataGrid(SfDataGrid dataGrid, string filePath, bool keepTypeFormat = true)
        {
            var headers = new List<string>();
            object[,] dataArray = null;
            var dateColIndices = new HashSet<int>();

            dataGrid.Dispatcher.Invoke(() =>
            {
                // Pre-build visible column metadata with combo lookup dictionaries built once per column
                var colMeta = new List<(GridColumnBase col, string mappingName, Dictionary<string, object> comboLookup)>();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var col = dataGrid.Columns[i];
                    if (col.IsHidden) continue;
                    headers.Add(col.HeaderText);

                    Dictionary<string, object> comboLookup = null;
                    if (col is GridComboBoxColumn comboCol &&
                        comboCol.ItemsSource != null &&
                        !string.IsNullOrWhiteSpace(comboCol.DisplayMemberPath) &&
                        !string.IsNullOrWhiteSpace(comboCol.SelectedValuePath))
                    {
                        // Build O(1) lookup dictionary once per combo column instead of O(m) per row
                        comboLookup = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        foreach (var comboItem in comboCol.ItemsSource)
                        {
                            if (comboItem == null) continue;
                            var itemType = comboItem.GetType();
                            var valProp = GetCachedProp(itemType, comboCol.SelectedValuePath);
                            var dispProp = GetCachedProp(itemType, comboCol.DisplayMemberPath);
                            if (valProp == null || dispProp == null) continue;
                            var key = valProp.GetValue(comboItem)?.ToString();
                            if (key != null)
                                comboLookup[key] = dispProp.GetValue(comboItem) ?? string.Empty;
                        }
                    }
                    colMeta.Add((col, col.MappingName, comboLookup));
                }

                var selectedSet = new HashSet<object>(dataGrid.SelectedItems.Cast<object>());
                var orderedRecords = dataGrid.View.Records
                    .Select(r => r.Data)
                    .Where(d => selectedSet.Contains(d))
                    .ToList();

                if (orderedRecords.Count == 0 || colMeta.Count == 0) return;

                int rowCount = orderedRecords.Count;
                int colCount = colMeta.Count;
                dataArray = new object[rowCount, colCount];

                // Cache PropertyInfos once from first row type
                Type rowType = orderedRecords[0].GetType();
                var propInfos = colMeta
                    .Select(cm => !string.IsNullOrWhiteSpace(cm.mappingName) ? GetCachedProp(rowType, cm.mappingName) : null)
                    .ToArray();

                for (int r = 0; r < rowCount; r++)
                {
                    var record = orderedRecords[r];
                    for (int c = 0; c < colCount; c++)
                    {
                        object val = string.Empty;
                        if (propInfos[c] != null)
                        {
                            var rawVal = propInfos[c].GetValue(record);
                            var comboLookup = colMeta[c].comboLookup;
                            if (comboLookup != null && rawVal != null)
                            {
                                // O(1) dictionary lookup instead of iterating all combo items per row
                                val = comboLookup.TryGetValue(rawVal.ToString(), out var display) ? display : rawVal;
                            }
                            else
                            {
                                val = rawVal ?? string.Empty;
                            }
                        }

                        if (!keepTypeFormat)
                        {
                            val = val?.ToString() ?? "";
                        }
                        else if (val is bool bv)
                        {
                            val = bv ? "True" : "False";
                        }
                        else if (val is DateTime)
                        {
                            dateColIndices.Add(c);
                        }

                        dataArray[r, c] = val;
                    }
                }
            });

            if (dataArray == null) return;

            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];
                worksheet.IsRightToLeft = true;

                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Range[1, i + 1].Text = headers[i];
                    worksheet.Range[1, i + 1].CellStyle.Font.Bold = true;
                }

                int rowCount = dataArray.GetLength(0);
                int colCount = dataArray.GetLength(1);
                if (rowCount > 0)
                {
                    // Bulk import: orders of magnitude faster than cell-by-cell for large datasets
                    worksheet.ImportArray(dataArray, 2, 1);

                    // Apply date format per column range (not per cell)
                    if (keepTypeFormat)
                    {
                        foreach (int ci in dateColIndices)
                            worksheet.Range[2, ci + 1, rowCount + 1, ci + 1].NumberFormat = "yyyy/mm/dd hh:mm:ss";
                    }
                }

                worksheet.UsedRange.AutofitColumns();
                worksheet.UsedRange.AutofitRows();
                workbook.SaveAs(filePath);
            }
        }

        private static async Task OpenExcelFile(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo = new System.Diagnostics.ProcessStartInfo(filePath)
                    {
                        UseShellExecute = true
                    };
                    process.Start();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to open Excel file", ex);
                }
            });
        }

        private static string GetUniqueFilePath(string fileName)
        {
            string baseFileName = fileName ?? $"Export_{DateTime.Now:yyyyMMdd_HHmmss}";
            string directory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Excel Exports"
            );

            Directory.CreateDirectory(directory);

            string filePath = System.IO.Path.Combine(directory, $"{baseFileName}.xlsx");
            int counter = 1;

            while (File.Exists(filePath))
            {
                filePath = System.IO.Path.Combine(directory, $"{baseFileName}_{counter}.xlsx");
                counter++;
            }

            return filePath;
        }

        private static void CleanupExcelObjects(Excel.Application excel, Excel.Workbook workbook, Excel.Worksheet worksheet)
        {
            if (worksheet != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
            }
            if (workbook != null)
            {
                workbook.Close();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
            }
            if (excel != null)
            {
                excel.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void HandleExportError(Exception ex)
        {
            string message = "Export failed: " + ex.Message;
            if (ex.InnerException != null)
            {
                message += "\n" + ex.InnerException.Message;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                new Msgwin(false, "خطا در انجام عملیات خروجی اکسل").Show();
            });

            // You might want to log the error here
            Debug.WriteLine($"Excel export error: {ex}");
        }
    }
}