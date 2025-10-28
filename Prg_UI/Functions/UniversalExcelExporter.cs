using OfficeOpenXml;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                            ExportSyncfusionDataGrid(syncfusionGrid, filePath);
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
                foreach (var item in dataGrid.SelectedItems)
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

                // Export data
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var value = data[rowIndex][colIndex];

                        // Check if the value is numeric
                        if (value != null && (value is int || value is long || value is decimal || value is double || value is float))
                        {
                            worksheet.Cells[rowIndex + 2, colIndex + 1].Value = value;
                        }
                        else
                        {
                            worksheet.Cells[rowIndex + 2, colIndex + 1].Value = value?.ToString() ?? "";
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
                foreach (var item in dataGrid.SelectedItems)
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

        /// <summary>
        /// متد جدید برای گرفتن مقدار نمایشی از ستون‌های کمبوباکس در SfDataGrid
        /// </summary>
        private static object GetSyncfusionCellDisplayValue(object record, GridColumnBase column)
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

                        // اگر ItemsSource تعریف شده و DisplayMemberPath و SelectedValuePath موجود باشند
                        if (comboColumn.ItemsSource != null &&
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
                                                return displayValue?.ToString() ?? string.Empty;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // اگر مقدار نمایشی پیدا نشد، مقدار خام را برگردان
                        return rawValue?.ToString() ?? string.Empty;
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
                            return value?.ToString() ?? string.Empty;
                        }
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                // Log the error if needed
                Debug.WriteLine($"Error getting cell display value: {ex.Message}");
                return string.Empty;
            }
        }

        private static void ExportSyncfusionDataGrid(SfDataGrid dataGrid, string filePath)
        {
            // Capture data on UI thread first
            var headers = new List<string>();
            var data = new List<List<object>>();

            // Use Dispatcher to safely access UI elements
            dataGrid.Dispatcher.Invoke(() =>
            {
                // Capture headers
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    if (!dataGrid.Columns[i].IsHidden)
                    {
                        headers.Add(dataGrid.Columns[i].HeaderText);
                    }
                }

                // Capture data from selected items
                foreach (var record in dataGrid.SelectedItems)
                {
                    var rowData = new List<object>();
                    for (int colIndex = 0; colIndex < dataGrid.Columns.Count; colIndex++)
                    {
                        var column = dataGrid.Columns[colIndex];
                        if (!column.IsHidden)
                        {
                            // استفاده از متد جدید برای دریافت مقدار نمایشی
                            var displayValue = GetSyncfusionCellDisplayValue(record, column);
                            rowData.Add(displayValue);
                        }
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

                // Export data
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        worksheet.Range[rowIndex + 2, colIndex + 1].Text = data[rowIndex][colIndex]?.ToString() ?? "";
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