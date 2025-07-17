using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.Concurrent;
using Prg_UI.HelperWins;
using Prg_UI.Functions;
using System.Collections;

namespace Functions
{
    /// <summary>
    /// Delegate for row validation, accepting event args and a validation result object.
    /// </summary>
    /// <typeparam name="T">Type of the items in the DataGrid.</typeparam>
    public delegate void RowValidationDelegate<T>(DataGridRowEditEndingEventArgs args, DataGridClipboardManager.PasteValidationResult validationResult);

    public static class DataGridClipboardManager
    {
        /// <summary>
        /// Represents the result of a paste validation operation.
        /// </summary>
        public class PasteValidationResult
        {
            /// <summary>
            /// Indicates whether the current row is valid for addition.
            /// </summary>
            public bool IsRowValid { get; set; } = true;
            public string RowMessage { get; set; }
        }

        #region NEWY
        // Add this new class to store column metadata
        private class ColumnMetadata
        {
            public string PropertyName { get; set; }
            public string Header { get; set; }
            public Type PropertyType { get; set; }
            public DataGridColumn Column { get; set; }
            public Func<string, object> CustomConverter { get; set; }
        }

        // Add this method to get custom converters for columns
        private static ColumnMetadata GetColumnMetadata(DataGridColumn column, Type itemType)
        {
            var metadata = new ColumnMetadata
            {
                Header = (column.Header?.ToString() ?? "").Trim(),
                Column = column
            };

            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                metadata.PropertyName = binding?.Path?.Path ?? "";
            }
            else if (column is DataGridComboBoxColumn comboColumn)
            {
                var binding = comboColumn.SelectedValueBinding as Binding;
                metadata.PropertyName = binding?.Path?.Path ?? "";

                // Add custom converter for ComboBox
                metadata.CustomConverter = (value) =>
                {
                    var items = comboColumn.ItemsSource as IEnumerable;
                    if (items == null) return null;

                    // Get the display member path and value member path
                    string displayPath = comboColumn.DisplayMemberPath;
                    string selectedValuePath = comboColumn.SelectedValuePath;
                    string valuePath = (comboColumn.SelectedValueBinding as Binding)?.Path.Path;

                    foreach (var item in items)
                    {
                        // Get display value
                        var displayProp = item.GetType().GetProperty(displayPath);
                        string displayValue = displayProp?.GetValue(item)?.ToString();

                        if (string.Equals(CL_LMethods.NormalizeArabicPersian(displayValue?.ToStringNullSafe()),
                            CL_LMethods.NormalizeArabicPersian(value?.ToStringNullSafe()), StringComparison.OrdinalIgnoreCase))
                        {
                            // Return the actual value
                            var valueProp = item.GetType().GetProperty(selectedValuePath);
                            return valueProp?.GetValue(item);
                        }
                    }
                    return null;
                };
            }
            else if (column is DataGridTemplateColumn)
            {
                metadata.PropertyName = metadata.Header;
            }

            if (!string.IsNullOrEmpty(metadata.PropertyName))
            {
                var prop = GetCachedProperty(itemType, metadata.PropertyName);
                metadata.PropertyType = prop?.PropertyType;
            }

            return metadata;
        }


        private static List<T> ParseExcelText<T>(DataGrid dataGrid, string text) where T : new()
        {
            var items = new List<T>();
            if (string.IsNullOrWhiteSpace(text)) return items;

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return items;

            // Get column metadata with custom converters
            var columnMetadata = dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => GetColumnMetadata(c, typeof(T)))
                .Where(m => !string.IsNullOrEmpty(m.PropertyName))
                .ToList();

            int expectedColumnCount = columnMetadata.Count;
            if (expectedColumnCount == 0) return items;

            // Check for header row
            bool headerExists = false;
            var firstRowCells = lines[0].Split('\t').Select(cell => cell.Trim()).ToArray();
            if (firstRowCells.Length == expectedColumnCount)
            {
                headerExists = firstRowCells.SequenceEqual(columnMetadata.Select(m => m.Header), StringComparer.OrdinalIgnoreCase);
            }

            int startIndex = headerExists ? 1 : 0;

            for (int rowIndex = startIndex; rowIndex < lines.Length; rowIndex++)
            {
                var cells = lines[rowIndex].Split('\t');
                if (cells.Length != expectedColumnCount)
                {
                    new Msgwin(false, "تعداد ستون‌های داده پیست شده با تعداد ستون‌های نرم افزار همخوانی ندارد!").Show();
                    continue;
                }

                T obj = new T();
                for (int i = 0; i < expectedColumnCount; i++)
                {
                    string cellValue = cells[i].Trim();
                    var metadata = columnMetadata[i];
                    var prop = GetCachedProperty(typeof(T), metadata.PropertyName);

                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (IsNullable(prop.PropertyType))
                                    prop.SetValue(obj, null);
                                else
                                    prop.SetValue(obj, Activator.CreateInstance(prop.PropertyType));
                                continue;
                            }

                            object convertedValue = null;
                            if (metadata.CustomConverter != null)
                            {
                                // Use custom converter (e.g., for ComboBox)
                                convertedValue = metadata.CustomConverter(cellValue);
                            }

                            if (metadata.CustomConverter == null || ReferenceEquals(convertedValue, null))
                            {
                                // Use default conversion
                                Type targetType = prop.PropertyType;
                                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    targetType = Nullable.GetUnderlyingType(targetType);

                                convertedValue = Convert.ChangeType(cellValue, targetType);
                            }


                            prop.SetValue(obj, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Conversion error for property {prop.Name}: {ex.Message}");

                            if (IsNullable(prop.PropertyType))
                                prop.SetValue(obj, null);
                            else
                                prop.SetValue(obj, Activator.CreateInstance(prop.PropertyType));
                        }
                    }
                }
                items.Add(obj);
            }
            return items;
        }
        #endregion

        #region Reflection Caching

        // Cache for property metadata to avoid repeated reflection calls.
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

        /// <summary>
        /// Retrieves the cached PropertyInfo for the specified property name on the given type.
        /// </summary>
        private static PropertyInfo GetCachedProperty(Type type, string propertyName)
        {
            // Cache for property metadata to avoid repeated reflection calls.
            if (!_propertyCache.TryGetValue(type, out var props))
            {
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .ToDictionary(p => p.Name, p => p);
                _propertyCache[type] = props;
            }
            props.TryGetValue(propertyName, out var property);
            return property;
        }

        #endregion

        #region Clipboard Operations

        /// <summary>
        /// Copies the selected items from the DataGrid to the clipboard.
        /// Supports both a custom serialized format and Excel-compatible (tab-delimited) text.
        /// </summary>
        public static void CopySelectedItems<T>(DataGrid dataGrid)
        {
            if (dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count == 0)
                return;

            try
            {
                // Filter out any placeholders and cast the items.
                var validItems = dataGrid.SelectedItems
                    .OfType<T>()
                    .Where(item => !ReferenceEquals(item, CollectionView.NewItemPlaceholder))
                    .ToList();

                if (!validItems.Any())
                {
                    new Msgwin(false, "هیچ داده معتبری برای کپی وجود ندارد!").Show();
                    return;
                }

                // Serialize the items into a Base64 string (for internal paste)
                var serializer = new DataContractSerializer(typeof(List<T>));
                string base64Data;
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, validItems);
                    base64Data = Convert.ToBase64String(stream.ToArray());
                }

                // Generate tab-delimited text from the DataGrid items (for Excel paste)
                string textData = GenerateTextFromDataGridItems(dataGrid, validItems);

                // Put both formats into the Clipboard
                var dataObj = new DataObject();
                dataObj.SetData("CustomGridData", base64Data); // Custom format
                dataObj.SetData(DataFormats.Text, textData);     // Excel-compatible format
                Clipboard.SetDataObject(dataObj, true);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "کپی داده ها با خطا مواجه شد!").Show();
                Console.WriteLine($"Error copying data: {ex.Message}");
            }
        }

        /// <summary>
        /// Pastes items from the clipboard into the DataGrid.
        /// Supports both the custom serialized format and Excel tab-delimited text.
        /// </summary>
        /// <typeparam name="T">Type of items, must have a parameterless constructor.</typeparam>
        /// <param name="dataGrid">The DataGrid control.</param>
        /// <param name="validateAction">A delegate to validate each row.</param>
        /// <param name="addAction">A delegate to add an item to the DataGrid's data source.</param>
        public static void PasteItems<T>(DataGrid dataGrid, RowValidationDelegate<T> validateAction, Action<T> addAction) where T : new()
        {
            if (!Clipboard.ContainsText())
                return;

            try
            {
                var dataObj = Clipboard.GetDataObject();
                List<T> items = null;

                // Use our custom format if available.
                if (dataObj != null && dataObj.GetDataPresent("CustomGridData"))
                {
                    var base64Data = dataObj.GetData("CustomGridData") as string;
                    if (!string.IsNullOrEmpty(base64Data))
                    {
                        var serializer = new DataContractSerializer(typeof(List<T>));
                        using (var stream = new MemoryStream(Convert.FromBase64String(base64Data)))
                        {
                            items = serializer.ReadObject(stream) as List<T>;
                        }
                    }
                }
                else
                {
                    // Otherwise, assume Excel (tab-delimited text) format.
                    var textData = Clipboard.GetText();
                    items = ParseExcelText<T>(dataGrid, textData);
                }

                if (items == null || items.Count == 0)
                {
                    new Msgwin(false, "آیتم های درستی انتخاب نشده است").Show();
                    return;
                }


                // Process the items on the UI thread to avoid blocking.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    bool errorHappened = false;
                    List<MsgModel> ErrosMessages = new List<MsgModel>();

                    foreach (var item in items)
                    {
                        var row = new DataGridRow { Item = item };
                        var args = new DataGridRowEditEndingEventArgs(row, DataGridEditAction.Commit);
                        var validationResult = new PasteValidationResult();
                        validateAction.Invoke(args, validationResult);

                        if (validationResult.IsRowValid)
                        {
                            addAction.Invoke(item);
                        }
                        else
                        {
                            errorHappened = true;

                            if (!string.IsNullOrEmpty(validationResult.RowMessage))
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = validationResult.RowMessage });
                            }
                        }
                    }

                    if (errorHappened)
                    {
                        if (ErrosMessages.Any())
                        {
                            new MsgListwin(false, ErrosMessages.Distinct().ToList()).Show();
                        }
                        else
                        {
                            new Msgwin(false, "انتقال کپی به دلیل عدم صحیح بودن داده ها کامل انجام نــشد").Show();
                        }
                    }
                    else //Finally Success
                    {
                        // Re-select and scroll to the pasted items.
                        dataGrid.SelectedItems.Clear();
                        foreach (var item in items)
                        {
                            dataGrid.SelectedItems.Add(item);
                            dataGrid.ScrollIntoView(item);
                        }
                    }
              
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "Paste به دلیل بروز خطا انجام نشد !").Show();
                Console.WriteLine("Paste error: " + ex.Message);
            }
        }

        #endregion

        #region Data Conversion

        /// <summary>
        /// Generates a tab-delimited text representation of the DataGrid items.
        /// Uses the DataGrid's visible columns (ordered by DisplayIndex) to generate headers and cell values.
        /// </summary>
        private static string GenerateTextFromDataGridItems<T>(DataGrid dataGrid, List<T> items)
        {
            dataGrid.CommitEdit();
            dataGrid.UpdateLayout();

            // Consider only visible columns.
            var sortedColumns = dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            // Create a header row using the column headers.
            var headerCells = sortedColumns.Select(col => col.Header?.ToString() ?? string.Empty).ToList();
            var headerLine = string.Join("\t", headerCells);

            var lines = new List<string> { headerLine };

            // Process each item.
            foreach (var item in items)
            {
                var rowCells = new List<string>();
                foreach (var column in sortedColumns)
                {
                    // Use the helper to get the cell's string value.
                    string cellValue = GetCellValueFromColumn(column, item);

                    rowCells.Add(cellValue);
                }
                lines.Add(string.Join("\t", rowCells));
            }
            return string.Join(Environment.NewLine, lines);
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

        private static string GetCellValueFromColumn<T>(DataGridColumn column, T item)
        {
            // Case 1: DataGridBoundColumn (e.g. text columns)
            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                if (binding != null && !string.IsNullOrEmpty(binding.Path.Path))
                {
                    var property = item.GetType().GetProperty(binding.Path.Path);
                    var value = property?.GetValue(item);
                    return value?.ToString() ?? string.Empty;
                }
            }
            // Case 2: DataGridComboBoxColumn
            else if (column is DataGridComboBoxColumn comboBoxColumn)
            {
                // Get the cell content for the item.
                var content = column.GetCellContent(item);
                if (content is ComboBox comboBox)
                {
                    return GetDisplayMemberPathValue(comboBox);
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
                return string.Empty;
            }
            // Case 3: DataGridTemplateColumn
            else if (column is DataGridTemplateColumn)
            {
                var content = column.GetCellContent(item);
                if (content is TextBlock textBlock)
                {
                    return textBlock.Text;
                }
                else if (content is CheckBox checkBox)
                {
                    // Return "True" or "False"
                    return (checkBox.IsChecked.HasValue ? checkBox.IsChecked.Value.ToString() : "False");
                }
                else if (content is ComboBox comboBox)
                {
                    // Return SelectedValue if present, otherwise the Text
                    return comboBox.SelectedValue?.ToString() ?? comboBox.Text;
                }
                else if (content is FrameworkElement frameworkElement)
                {
                    // Try to get a property named "Value" if it exists.
                    var valueProperty = frameworkElement.GetType().GetProperty("Value");
                    var value = valueProperty?.GetValue(frameworkElement);
                    return value?.ToString() ?? string.Empty;
                }
                return string.Empty;
            }
            return string.Empty;
        }


        /// <summary>
        /// Parses tab-delimited text (potentially from Excel) and maps it to a list of T.
        /// </summary>
        private static List<T> ____ParseExcelText<T>(DataGrid dataGrid, string text) where T : new()
        {
            var items = new List<T>();
            if (string.IsNullOrWhiteSpace(text))
                return items;

            // Split the text into lines.
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return items;

            // Extract columns from the DataGrid by checking the column type.
            // For Bound columns, use the binding's Path.
            // For ComboBox columns, try using the SelectedValueBinding.
            // For Template columns, fall back to the header text.
            var sortedColumns = dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c =>
                {
                    string propertyName = string.Empty;
                    string header = (c.Header?.ToString() ?? "").Trim();

                    if (c is DataGridBoundColumn boundColumn)
                    {
                        var binding = boundColumn.Binding as Binding;
                        propertyName = binding?.Path?.Path ?? "";
                    }
                    else if (c is DataGridComboBoxColumn comboColumn)
                    {
                        // Use SelectedValueBinding if available.
                        var binding = comboColumn.SelectedValueBinding as Binding;
                        propertyName = binding?.Path?.Path ?? "";
                    }
                    else if (c is DataGridTemplateColumn)
                    {
                        // As a fallback, assume that the header text is the property name.
                        propertyName = header;
                    }
                    return new { Header = header, PropertyName = propertyName.Trim() };
                })
                .Where(x => !string.IsNullOrEmpty(x.PropertyName))
                .ToList();

            int expectedColumnCount = sortedColumns.Count;
            if (expectedColumnCount == 0)
                return items;

            // Determine if the first row is a header by comparing to DataGrid headers.
            bool headerExists = false;
            var firstRowCells = lines[0].Split('\t').Select(cell => cell.Trim()).ToArray();
            if (firstRowCells.Length == expectedColumnCount)
            {
                bool headersMatch = true;
                for (int i = 0; i < expectedColumnCount; i++)
                {
                    if (!string.Equals(firstRowCells[i], sortedColumns[i].Header, StringComparison.CurrentCultureIgnoreCase))
                    {
                        headersMatch = false;
                        break;
                    }
                }
                if (headersMatch)
                    headerExists = true;
            }

            // If the first line is a header, start processing from the next line.
            int startIndex = headerExists ? 1 : 0;

            for (int rowIndex = startIndex; rowIndex < lines.Length; rowIndex++)
            {
                var cells = lines[rowIndex].Split('\t');
                if (cells.Length != expectedColumnCount)
                {
                    new Msgwin(false, "تعداد ستون‌های داده پیست شده با تعداد ستون‌های دیتاگرید همخوانی ندارد!").Show();
                    continue;
                }

                T obj = new T();
                // For each expected column, assign the cell value to the matching property.
                for (int i = 0; i < expectedColumnCount; i++)
                {
                    string cellValue = cells[i].Trim();
                    string propertyName = sortedColumns[i].PropertyName;
                    var prop = GetCachedProperty(typeof(T), propertyName);
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                // For empty cells, assign null if the property type is nullable; otherwise assign a default.
                                if (IsNullable(prop.PropertyType))
                                    prop.SetValue(obj, null);
                                else
                                    prop.SetValue(obj, Activator.CreateInstance(prop.PropertyType));
                            }
                            else
                            {
                                // If the property is Nullable<T>, use the underlying type.
                                Type targetType = prop.PropertyType;
                                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    targetType = Nullable.GetUnderlyingType(targetType);

                                // Convert the cell value to the target type.
                                object convertedValue = Convert.ChangeType(cellValue, targetType);
                                prop.SetValue(obj, convertedValue);
                            }
                        }
                        catch
                        {
                            // On conversion errors, assign null (or a default value) based on property type.
                            if (IsNullable(prop.PropertyType))
                                prop.SetValue(obj, null);
                            else
                                prop.SetValue(obj, Activator.CreateInstance(prop.PropertyType));
                        }
                    }
                }
                items.Add(obj);
            }
            return items;
        }

        /// <summary>
        /// Retrieves the cached PropertyInfo for the specified property name on the given type.
        /// </summary>


        /// <summary>
        /// Checks whether a type is nullable (i.e. either a reference type or a Nullable value type).
        /// </summary>
        private static bool IsNullable(Type type)
        {
            return (!type.IsValueType) || (Nullable.GetUnderlyingType(type) != null);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Helper method to build an error message based on an object's public properties.
        /// </summary>
        private static string BuildErrorMessage(object item)
        {
            var sb = new StringBuilder("مشکل در ردیف: ");
            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                object value = prop.GetValue(item) ?? "NULL";
                sb.AppendFormat("{0} = {1}, ", prop.Name, value);
            }
            if (sb.Length > 2)
            {
                sb.Length -= 2;
            }
            return sb.ToString();
        }

        #endregion
    }
}
