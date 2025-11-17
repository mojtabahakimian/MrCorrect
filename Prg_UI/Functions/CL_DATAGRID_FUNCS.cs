using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Functions
{
    public static class CL_DATAGRID_FUNCS

    {

        //var THES_INDEX = PGET_LST_SUB.Columns.FirstOrDefault(c => c.SortMemberPath == "THES").DisplayIndex;

        //DataGridCell THES_CELL = CL_LMethods.GetCell(PGET_LST_SUB, row_index, THES_INDEX);

        //var test2 = ((ComboBox)cell.Content).SelectedValue;

        ////var tes1t2 = ((ComboBox)cell.Content).ItemsSource;

        //if (CURRENT_ITMES_ROW.THES == null || THES_CELL == null)

        //{



        //}



        /// <summary>
        /// Ensures all GridColumns in an SfDataGrid have valid MappingName to prevent
        /// "ColumnName cannot be NULL" errors when AllowFiltering is enabled.
        /// This method validates and fixes columns that might have null or empty MappingName.
        /// </summary>
        /// <param name="dataGrid">The SfDataGrid to validate and fix.</param>
        public static void EnsureGridColumnsHaveMappingName(SfDataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.Columns == null || dataGrid.Columns.Count == 0)
            {
                return;
            }

            try
            {
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var column = dataGrid.Columns[i];
                    if (column == null)
                    {
                        continue;
                    }

                    // Check if column has null or empty MappingName
                    if (string.IsNullOrEmpty(column.MappingName))
                    {
                        // Generate a safe MappingName from HeaderText or use a default
                        string safeMappingName = GenerateSafeMappingName(column, i);

                        // Only set if we can generate a valid name
                        if (!string.IsNullOrEmpty(safeMappingName))
                        {
                            column.MappingName = safeMappingName;
                            //Debug.WriteLine($"Fixed column {i}: Assigned MappingName '{safeMappingName}'");
                        }
                    }

                    // Disable filtering on template columns that don't have proper data binding
                    if (column is GridTemplateColumn templateColumn && string.IsNullOrEmpty(templateColumn.MappingName))
                    {
                        if (templateColumn.AllowFiltering)
                        {
                            templateColumn.AllowFiltering = false;

                            //Debug.WriteLine($"Disabled filtering on template column {i} due to missing MappingName");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error in EnsureGridColumnsHaveMappingName: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a safe MappingName for a column based on its HeaderText or column index.
        /// </summary>
        private static string GenerateSafeMappingName(GridColumn column, int columnIndex)
        {
            // Try to use HeaderText first
            if (!string.IsNullOrEmpty(column.HeaderText))
            {
                // Remove special characters and spaces
                string safeName = new string(column.HeaderText
                    .Where(c => char.IsLetterOrDigit(c) || c == '_')
                    .ToArray());

                if (!string.IsNullOrEmpty(safeName))
                {
                    return safeName;
                }
            }

            // Fallback to column name from the column itself if it has one
            if (!string.IsNullOrEmpty(column.MappingName))
            {
                return column.MappingName;
            }

            // Last resort: use column index
            return $"Column_{columnIndex}";

        }

    }
}
