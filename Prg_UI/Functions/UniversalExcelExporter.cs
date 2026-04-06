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
        private static readonly ConcurrentDictionary<string, System.Reflection.PropertyInfo> _propertyInfoCache = new ConcurrentDictionary<string, System.Reflection.PropertyInfo>();

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
                        return GetPropertyValue(item, binding.Path.Path) ?? "";
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
                            var value = GetPropertyValue(item, binding.Path.Path);

                            // If there's display text we need to show instead of raw value
                            if (value != null && comboBoxColumn.ItemsSource != null &&
                                !string.IsNullOrEmpty(comboBoxColumn.DisplayMemberPath) &&
                                !string.IsNullOrEmpty(comboBoxColumn.SelectedValuePath))
                            {
                                // Try to find matching item in the ItemsSource
                                foreach (var comboItem in comboBoxColumn.ItemsSource)
                                {
                                    var itemValue = GetPropertyValue(comboItem, comboBoxColumn.SelectedValuePath);
                                    if (itemValue != null && itemValue.Equals(value))
                                    {
                                        var displayValue = GetPropertyValue(comboItem, comboBoxColumn.DisplayMemberPath);
                                        return displayValue?.ToString() ?? string.Empty;
                                    }
                                }
                            }

                            return value?.ToString() ?? string.Empty;
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

        private static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            string cacheKey = $"{target.GetType().FullName}|{propertyName}";
            var propertyInfo = _propertyInfoCache.GetOrAdd(cacheKey, _ => target.GetType().GetProperty(propertyName));

            return propertyInfo?.GetValue(target);
        }

        private static Dictionary<string, object> BuildComboDisplayMap(IEnumerable itemsSource, string selectedValuePath, string displayMemberPath)
        {
            var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (itemsSource == null ||
                string.IsNullOrWhiteSpace(selectedValuePath) ||
                string.IsNullOrWhiteSpace(displayMemberPath))
            {
                return map;
            }

            foreach (var item in itemsSource)
            {
                if (item == null) continue;

                var keyValue = GetPropertyValue(item, selectedValuePath);
                if (keyValue == null) continue;

                string key = keyValue.ToString();
                if (string.IsNullOrWhiteSpace(key) || map.ContainsKey(key)) continue;

                map[key] = GetPropertyValue(item, displayMemberPath) ?? string.Empty;
            }

            return map;
        }

        /// <summary>
        /// برای Syncfusion: برای هر ستون یک extractor سریع می‌سازد تا در ردیف‌های زیاد Reflection تکراری کم شود
        /// </summary>
        private static Func<object, object> BuildSyncfusionValueExtractor(GridColumnBase column)
        {
            if (column == null || string.IsNullOrWhiteSpace(column.MappingName))
            {
                return _ => string.Empty;
            }

            if (column is GridComboBoxColumn comboColumn)
            {
                var displayMap = BuildComboDisplayMap(
                    comboColumn.ItemsSource,
                    comboColumn.SelectedValuePath,
                    comboColumn.DisplayMemberPath);

                return record =>
                {
                    var rawValue = GetPropertyValue(record, column.MappingName);
                    if (rawValue == null) return string.Empty;

                    if (displayMap.Count > 0)
                    {
                        var rawValueStr = rawValue.ToString();
                        if (!string.IsNullOrWhiteSpace(rawValueStr) && displayMap.TryGetValue(rawValueStr, out var displayValue))
                                        {
                            return displayValue ?? string.Empty;
                                        }
                    }
                    return rawValue;
                };
            }

            return record => GetPropertyValue(record, column.MappingName) ?? string.Empty;
        }

        private static void ExportWpfDataGrid(DataGrid dataGrid, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Capture the data on the UI thread first
            var headers = new List<string>();
            var data = new List<List<object>>();

            // Use Dispatcher to safely access UI elements
            dataGrid.Dispatcher.Invoke(() =>
            {
                // Capture headers
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                    {
                        headers.Add(dataGrid.Columns[i].Header?.ToString() ?? $"Column {i + 1}");
                    }
                }

                // Capture data from selected items
                var orderedSelectedItems = dataGrid.SelectedItems
                    .Cast<object>()
                    .OrderBy(item => dataGrid.Items.IndexOf(item))
                    .ToList();

                foreach (var item in orderedSelectedItems)
                {
                    var rowData = new List<object>();
                    for (int i = 0; i < dataGrid.Columns.Count; i++)
                    {
                        if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                        {
                            rowData.Add(GetCellValue(item, dataGrid.Columns[i]));
                        }
                    }
                    data.Add(rowData);
                }
            });

            // Create Excel file with captured data (not on UI thread)
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Ensure RTL is applied at the worksheet level
                worksheet.View.RightToLeft = true;

                // Export headers
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Export data با حفظ فرمت
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var value = data[rowIndex][colIndex];
                        var cell = worksheet.Cells[rowIndex + 2, colIndex + 1];

                        if (value != null)
                        {
                            // حفظ فرمت بر اساس نوع داده
                            if (value is int || value is long || value is short || value is byte ||
                                value is uint || value is ulong || value is ushort || value is sbyte ||
                                value is decimal || value is double || value is float)
                            {
                                // اعداد (صحیح و اعشاری)
                                cell.Value = value;
                            }
                            else if (value is DateTime dateTime)
                            {
                                // تاریخ و زمان
                                cell.Value = dateTime;
                                cell.Style.Numberformat.Format = "yyyy/mm/dd hh:mm:ss";
                            }
                            else if (value is bool boolValue)
                            {
                                // Boolean - ذخیره به صورت متن
                                cell.Value = boolValue ? "True" : "False";
                            }
                            else
                            {
                                // سایر انواع به صورت متن
                                cell.Value = value.ToString();
                            }
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Save the package
                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static void ExportWpfDataGridDisformated(DataGrid dataGrid, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Capture the data on the UI thread first
            var headers = new List<string>();
            var data = new List<List<object>>();

            // Use Dispatcher to safely access UI elements
            dataGrid.Dispatcher.Invoke(() =>
            {
                // Capture headers
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                    {
                        headers.Add(dataGrid.Columns[i].Header?.ToString() ?? $"Column {i + 1}");
                    }
                }

                // Capture data from selected items
                var orderedSelectedItems = dataGrid.SelectedItems
                    .Cast<object>()
                    .OrderBy(item => dataGrid.Items.IndexOf(item))
                    .ToList();

                foreach (var item in orderedSelectedItems)
                {
                    var rowData = new List<object>();
                    for (int i = 0; i < dataGrid.Columns.Count; i++)
                    {
                        if (dataGrid.Columns[i].Visibility == Visibility.Visible)
                        {
                            rowData.Add(GetCellValue(item, dataGrid.Columns[i]));
                        }
                    }
                    data.Add(rowData);
                }
            });

            // Create Excel file with captured data (not on UI thread)
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Ensure RTL is applied at the worksheet level
                worksheet.View.RightToLeft = true;

                // Export headers
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Export data AS TEXT to avoid formatting issues
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var value = data[rowIndex][colIndex];
                        worksheet.Cells[rowIndex + 2, colIndex + 1].Value = value?.ToString() ?? "";
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Save the package
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

        private static void ExportSyncfusionDataGrid(SfDataGrid dataGrid, string filePath, bool keepTypeFormat = true)
        {
            // Capture data on UI thread first
            var headers = new List<string>();
            var data = new List<List<object>>();

            // Use Dispatcher to safely access UI elements
            dataGrid.Dispatcher.Invoke(() =>
            {
                var visibleColumns = dataGrid.Columns.Where(c => !c.IsHidden).ToList();
                var valueExtractors = visibleColumns
                    .Select(BuildSyncfusionValueExtractor)
                    .ToList();

                // Capture headers
                for (int i = 0; i < visibleColumns.Count; i++)
                {
                    headers.Add(visibleColumns[i].HeaderText);
                }

                // Capture selected records in the exact order shown in the grid
                var selectedRecordsSet = dataGrid.SelectedItems.Cast<object>().ToHashSet();
                var orderedSelectedRecords = new List<object>();
                var orderedSelectedRecordsSet = new HashSet<object>();

                if (dataGrid.View?.Records != null)
                {
                    foreach (var recordEntry in dataGrid.View.Records)
                    {
                        var recordData = recordEntry?.Data;
                        if (recordData != null && selectedRecordsSet.Contains(recordData))
                        {
                            orderedSelectedRecords.Add(recordData);
                            orderedSelectedRecordsSet.Add(recordData);
                        }
                    }
                }

                // Fallback for records that might not be present in View.Records
                foreach (var selectedRecord in dataGrid.SelectedItems)
                {
                    if (!orderedSelectedRecordsSet.Contains(selectedRecord))
                    {
                        orderedSelectedRecords.Add(selectedRecord);
                        orderedSelectedRecordsSet.Add(selectedRecord);
                    }
                }

                foreach (var record in orderedSelectedRecords)
                {
                    var rowData = new List<object>(valueExtractors.Count);
                    for (int colIndex = 0; colIndex < valueExtractors.Count; colIndex++)
                    {
                        var cellValue = valueExtractors[colIndex](record);
                        rowData.Add(cellValue);
                    }
                    data.Add(rowData);
                }
            });

            // Create Excel file with captured data
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];

                // Set RightToLeft orientation
                worksheet.IsRightToLeft = true;

                // Export headers
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Range[1, i + 1].Text = headers[i];
                    worksheet.Range[1, i + 1].CellStyle.Font.Bold = true;
                }

                // Export data با حفظ فرمت
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var value = data[rowIndex][colIndex];
                        var cell = worksheet.Range[rowIndex + 2, colIndex + 1];

                        if (keepTypeFormat && value != null)
                        {
                            // حفظ فرمت بر اساس نوع داده
                            if (value is int || value is long || value is short || value is byte ||
                                value is uint || value is ulong || value is ushort || value is sbyte)
                            {
                                // اعداد صحیح
                                cell.Number = Convert.ToDouble(value);
                            }
                            else if (value is decimal || value is double || value is float)
                            {
                                // اعداد اعشاری
                                cell.Number = Convert.ToDouble(value);
                            }
                            else if (value is DateTime dateTime)
                            {
                                // تاریخ و زمان
                                cell.DateTime = dateTime;
                                cell.NumberFormat = "yyyy/mm/dd hh:mm:ss";
                            }
                            else if (value is bool boolValue)
                            {
                                // Boolean
                                cell.Text = boolValue ? "True" : "False";
                            }
                            else
                            {
                                // سایر انواع به صورت متن
                                cell.Text = value.ToString();
                            }
                        }
                        else
                        {
                            // بدون حفظ فرمت، همه چیز به صورت متن
                            cell.Text = value?.ToString() ?? "";
                        }
                    }
                }

                // Auto-fit columns
                worksheet.UsedRange.AutofitColumns();
                worksheet.UsedRange.AutofitRows();

                // Save the workbook
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
