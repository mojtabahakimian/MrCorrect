using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using MaterialDesignThemes.Wpf;
using System.Collections;
using System.Security;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Imaging;
using ImageMagick;
using System.Text.RegularExpressions;
using System.Buffers;
using Prg_Proccessy.Generaly;
using System.ComponentModel;
using System.Globalization;
using AUTO_BAZ.HelperWins;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Data;
using Microsoft.VisualBasic;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.Wins.WinOther;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Functions;
using System.Windows.Data;
using Prg_UI.Functions.Jostejoo;
using System.Windows.Media.Effects;
using System.Security.Principal;

namespace Prg_UI.Functions
{
    public static class CL_LMethods
    {
        public static class ConstructorRowDetector
        {
            // Cache property lists per type برای کارایی و جلوگیری از Reflection مکرر
            private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

            /// <summary>
            /// Returns <c>true</c> if all public readable/writable properties
            /// of <paramref name="instance"/> still have the same value they
            /// had right after the default constructor ran.
            /// </summary>
            public static bool IsPristine<T>(T instance)
            {
                if (instance is null) return true; // امنیت در برابر NRE

                var type = typeof(T);

                // به‌جای نگه‌داشتن یک نمونهٔ پیش‌فرض (که ممکن است رم نشت کند)،
                // هر بار یک نمونهٔ سبک می‌سازیم؛ در اغلب مدل‌ها سبک است.
                var defaultInstance = Activator.CreateInstance<T>();

                // گرفتن لیست پراپرتی‌ها از کش یا بازتاب یک‌باره
                var props = _propCache.GetOrAdd(type, t =>
                    t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead && p.CanWrite)
                     .ToArray());

                foreach (var p in props)
                {
                    var current = p.GetValue(instance);
                    var initial = p.GetValue(defaultInstance);

                    if (!Equals(current, initial))
                        return false;   // حداقل یک فیلد تغییر کرده؛ «سطر تازه» نیست
                }

                return true; // همه مثل حالت سازنده‌اند ⇒ کاربر چیزی ننوشته
            }
        }

        public static bool IsInside<T>(DependencyObject start) where T : DependencyObject
        {
            for (DependencyObject cur = start; cur != null;)
            {
                if (cur is T) return true;
                var parent = LogicalTreeHelper.GetParent(cur);
                if (parent == null && cur is Visual v) parent = VisualTreeHelper.GetParent(v);
                cur = parent;
            }
            return false;
        }
        public static Theme MYTHEME { get; set; }
        /// <summary>
        /// , new WindowInteropHelper(this).Handle |_______________|
        ///|_______________| (GetTheWindow(new WindowInteropHelper(this).Handle) as MainWindow)
        /// </summary>
        /// <param name="windowHandle"></param>
        /// <returns></returns>
        /// 
        public static Visual GetTheWindow(IntPtr windowHandle)
        {
            //if (windowHandle == IntPtr.Zero) return null;

            static Visual ResolveFromWindow(Window window)
            {
                var helper = new WindowInteropHelper(window);

                if (helper.Handle == IntPtr.Zero)
                {
                    helper.EnsureHandle();
                }

                var source = HwndSource.FromHwnd(helper.Handle);
                return source?.RootVisual as Visual ?? window;
            }

            if (windowHandle == IntPtr.Zero)
            {
                var fallbackWindow = Application.Current?.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.IsActive)
                    ?? Application.Current?.MainWindow;

                if (fallbackWindow is null)
                {
                    throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));
                }

                return ResolveFromWindow(fallbackWindow);
            }

            var hwndSource = HwndSource.FromHwnd(windowHandle);
            if (hwndSource?.RootVisual is Visual visual)
            {
                return visual;
            }

            var knownWindow = Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(w => new WindowInteropHelper(w).Handle == windowHandle)
                ?? Application.Current?.MainWindow;

            if (knownWindow is null)
            {
                throw new InvalidOperationException("Unable to resolve window from provided handle.");
            }

            return ResolveFromWindow(knownWindow);


        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }

                // Handle ItemsControl (like TabControl) items
                if (depObj is ItemsControl itemsControl)
                {
                    foreach (object item in itemsControl.Items)
                    {
                        if (item is DependencyObject itemDepObj)
                        {
                            foreach (T childOfChild in FindVisualChildren<T>(itemDepObj))
                                yield return childOfChild;
                        }
                    }
                }
            }
        }
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }
        public static T FindAncestor<T>(DependencyObject start) where T : DependencyObject
        {
            for (var p = start; p != null; p = VisualTreeHelper.GetParent(p))
                if (p is T t) return t;
            return null;
        }
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent) return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
                return null;

            T parentElement = parent as T;
            return parentElement ?? FindParent<T>(parent);
        }
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            if (parent is not null)
            {
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = GetVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
            }
            return child;
        }
        public static ComboBox GetComboBoxFromCell(DataGrid DG, string sortMemberPath, DataGridRow row)
        {
            try
            {
                var column = DG.Columns.FirstOrDefault(c => c.SortMemberPath == sortMemberPath);
                if (column == null)
                {
                    Debug.WriteLine($"Column with SortMemberPath '{sortMemberPath}' not found.");
                    return null;
                }

                int rowIndex = DG.ItemContainerGenerator.IndexFromContainer(row);
                if (rowIndex < 0)
                {
                    Debug.WriteLine("Row index is invalid.");
                    return null;
                }

                var cellInfo = new DataGridCellInfo(DG.Items[rowIndex], column);
                var cell = CL_LMethods.GetDataGridCell(cellInfo);

                // Retrieve ComboBox from the cell
                return CL_LMethods.GetVisualChild<ComboBox>(cell);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetComboBoxFromCell: {ex.Message}");
                return null;
            }
        }
        public static string ToRawTarikh(this string txt)
        {
            //1402_02_01
            //return txt.Replace("/", "").Replace("_", "");
            return new string(txt.Where(char.IsDigit).ToArray());
        }
        public static string ToRawTime(this string txt)
        {
            //// Pad the string with leading zeros if necessary
            var rawText = new string(txt.Where(char.IsDigit).ToArray());

            //// Format the time string
            //return $"{rawText.Substring(0, 2)}{rawText.Substring(2, 2)}{rawText.Substring(4, 2)}";
            return rawText;
        }
        public static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int column)
        {
            if (dataGrid == null) throw new ArgumentNullException("dataGrid");
            if (row == null) throw new ArgumentNullException("row");
            if (column < 0) throw new ArgumentOutOfRangeException("column");

            DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
            {
                row.ApplyTemplate();
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
            }
            if (presenter != null)
            {
                var cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(row, dataGrid.Columns[column]);
                    cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                }
                return cell;
            }
            return null;
        }
        public static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
                return (DataGridCell)cellContent.Parent;

            return null;
        }

        #region MyRegion
        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // may be virtualized, bring into view and try again
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }
        #endregion
        public static TContainer GetContainerFromIndex<TContainer>(this ItemsControl itemsControl, int index) where TContainer : DependencyObject
        {
            return (TContainer)
              itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        }
        public static DataGridRow GetEditingRow(this DataGrid dataGrid)
        {
            if (dataGrid is null)
            {
                return null;
            }

            var sIndex = dataGrid.SelectedIndex;
            if (sIndex >= 0)
            {
                var selected = dataGrid.GetContainerFromIndex<DataGridRow>(sIndex);
                if (selected.IsEditing) return selected;
            }

            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                if (i == sIndex) continue;
                var item = dataGrid.GetContainerFromIndex<DataGridRow>(i);
                if (item.IsEditing) return item;
            }

            return null;
        }
        public static IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }
        public static bool IsEditing(this DataGrid dataGrid)
        {
            if (dataGrid == null)
            {
                return false;
            }

            return dataGrid?.GetEditingRow() != null;
        }
        public static string ToStringNullSafe(this object value)
        {
            return (value ?? string.Empty).ToString();
        }

        public static bool IsValidIndex(DataGrid dataGrid, int index)
        {
            // Null check for the DataGrid
            if (dataGrid == null)
            {
                //LogError("DataGrid is null.");
                return false;
            }

            // Ensure the ItemsSource is not null and has elements
            if (dataGrid.Items == null || dataGrid.Items.Count == 0)
            {
                //LogError("DataGrid.Items is null or empty.");
                return false;
            }

            // Check if the index is within the valid range
            if (index < 0 || index >= dataGrid.Items.Count)
            {
                //LogError($"Index {index} is out of bounds. Items count: {dataGrid.Items.Count}");
                return false;
            }

            return true;
        }
        public static string GetTempFilePathWithExtension(string extension)
        {
            var TempPathy = Path.Combine(Path.GetTempPath(), "MCR_FLD");
            if (!Directory.Exists(TempPathy))
            {
                Directory.CreateDirectory(TempPathy);
            }

            string FileNamey = @$"\{Guid.NewGuid()}";

            string FinalFullPathFile = TempPathy + FileNamey;

            return FinalFullPathFile;
        }
        public static bool HasWritePermissionOnDir(string path)
        {
            try
            {
                string tempFilePath = Path.Combine(path, Path.GetRandomFileName());
                using (FileStream fs = File.Create(tempFilePath, 4096, FileOptions.DeleteOnClose))
                {
                    // The file was successfully created, and will be deleted on close
                }
                // If we reach this point, the directory is writable
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                // Access denied, the directory is not writable
                return false;
            }
            catch (System.IO.IOException)
            {
                // Handle the IOException if needed
                return false;
            }
        }
        public static byte[] ConvertFileToByte(this string filePath)
        {
            //FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read); //Last code was => 1
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); //This " FileShare.ReadWrite " Added => 2
            BinaryReader reader = new BinaryReader(stream);

            byte[] photo = reader.ReadBytes((int)stream.Length);

            reader?.Close();
            stream?.Close();

            return photo;
        }
        public static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                // Try opening the file exclusively.
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                // If an IOException is thrown, the file is in use.
                return true;
            }
            finally
            {
                stream?.Close();
            }
            return false;
        }

        public static bool WriteTheFile(string filePath, byte[] fileData)
        {
            try
            {
                // Ensure other processes can access the file
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }
                return true;

            }
            catch (UnauthorizedAccessException)
            {
                new Msgwin(false, "عدم دسترسی به فایل مورد نظر").ShowDialog();
                return false;
            }
            catch (IOException ioEx)
            {
                new Msgwin(false, "فایل توسط فرآیند دیگری در حال استفاده است. لطفاً آن را ببندید و دوباره امتحان کنید").ShowDialog();
                return false;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات هنگام نوشتن فایل !").ShowDialog();
                return false;
            }
        }
        public static void SendKey_US(Key key, bool isPreviewEvent = false)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    KeyEventArgs e = null;

                    if (isPreviewEvent)
                    {
                        e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                        {
                            RoutedEvent = Keyboard.PreviewKeyDownEvent
                        };
                    }
                    else
                    {
                        e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                        {
                            RoutedEvent = Keyboard.KeyDownEvent
                        };
                    }

                    InputManager.Current.ProcessInput(e);

                    // Note: Based on your requirements you may also need to fire events for:
                    // RoutedEvent = Keyboard.PreviewKeyDownEvent
                    // RoutedEvent = Keyboard.KeyUpEvent
                    // RoutedEvent = Keyboard.PreviewKeyUpEvent
                }
            }
        }

        public static string RemoveQut(this string txt)
        {
            return txt.Replace(",", "");
        }


        /// <summary>
        /// Gets a value that indicates whether <paramref name="path"/>
        /// is a valid path.
        /// </summary>
        /// <returns>Returns <c>true</c> if <paramref name="path"/> is a
        /// valid path; <c>false</c> otherwise. Also returns <c>false</c> if
        /// the caller does not have the required permissions to access
        /// <paramref name="path"/>.
        /// </returns>
        /// <seealso cref="Path.GetFullPath"/>
        /// <seealso cref="TryGetFullPath"/>
        public static bool IsValidPath(string path)
        {
            string result;
            return TryGetFullPath(path, out result);
        }

        /// <summary>
        /// Returns the absolute path for the specified path string. A return
        /// value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute
        /// path information.
        /// </param>
        /// <param name="result">When this method returns, contains the absolute
        /// path representation of <paramref name="path"/>, if the conversion
        /// succeeded, or <see cref="String.Empty"/> if the conversion failed.
        /// The conversion fails if <paramref name="path"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This
        /// parameter is passed uninitialized; any value originally supplied
        /// in <paramref name="result"/> will be overwritten.
        /// </param>
        /// <returns><c>true</c> if <paramref name="path"/> was converted
        /// to an absolute path successfully; otherwise, false.
        /// </returns>
        /// <seealso cref="Path.GetFullPath"/>
        /// <seealso cref="IsValidPath"/>
        public static bool TryGetFullPath(string path, out string result)
        {
            result = String.Empty;
            if (String.IsNullOrWhiteSpace(path)) { return false; }
            bool status = false;

            try
            {
                result = Path.GetFullPath(path);
                status = true;
            }
            catch (ArgumentException) { }
            catch (SecurityException) { }
            catch (NotSupportedException) { }
            catch (PathTooLongException) { }

            return status;
        }
        private static Action EmptyDelegate = delegate () { };
        public static void Refreshy(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
        public static class UiRefresher
        {
            public static void RefWhole(Grid gridy)
            {
                foreach (UIElement ctrl in gridy.Children)
                {
                    Refreshy(ctrl);
                    //if (ctrl.GetType() == typeof(TextBox))
                    //{
                    //    ////tbText = ctrl.Text;
                    //    //entries.Add(((TextBox)ctrl).Text);
                    //    //Console.WriteLine(((TextBox)ctrl).Text);
                    //}
                }
                //((MainWindow)Application.Current.MainWindow).UpdateLayout();
                //UiRefresh.Refresh(mytext);
            }
        }

        public static bool IsImageTYPE(string _filetype)
        {
            switch (_filetype.ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".jfif":
                case ".tif":
                case ".jpe":
                case ".bmp":
                    return true;

                default:
                    return false;
            }
        }

        public enum NavigationDirection
        {
            FirstItem,
            BackItem,
            NextItem,
            LastItem
        }
        /// <summary>
        /// صرفا برای خود دیتاگرید
        /// </summary>
        /// <param name="DTG"></param>
        /// <param name="ArrowDirect"></param>
        /// <param name="CustomRowIndex"></param>
        public static void MovingDG(DataGrid DTG, NavigationDirection? ArrowDirect, int? CustomRowIndex = null)
        {
            if (DTG == null || DTG.Items.Count == 0) return;

            int targetIndex = 0;

            // Adjust for the extra empty row at the end
            int adjustedItemCount = DTG.Items.Count - 1;

            if (ArrowDirect is not null)
            {
                switch (ArrowDirect)
                {
                    case NavigationDirection.FirstItem: // First record.
                        targetIndex = 0;
                        break;
                    case NavigationDirection.BackItem: // Previous record.
                        targetIndex = Math.Max(0, DTG.SelectedIndex - 1);
                        break;
                    case NavigationDirection.NextItem: // Next record.
                        targetIndex = Math.Min(adjustedItemCount - 1, DTG.SelectedIndex + 1);
                        break;
                    case NavigationDirection.LastItem: // Last record.
                        if (DTG.IsReadOnly)
                        {
                            targetIndex = adjustedItemCount;
                        }
                        else
                        {
                            targetIndex = adjustedItemCount - 1;
                        }
                        break;
                }
            }


            if (CustomRowIndex is not null) //Custom Row Index Called
            {
                if (CustomRowIndex > 0)
                {
                    targetIndex = Convert.ToInt32(CustomRowIndex);
                }
            }


            // Check if the targetIndex is within the valid range
            if (targetIndex >= 0 && targetIndex < DTG.Items.Count)
            {
                try
                {
                    // Scroll the item into view and select it.
                    DTG.SelectedIndex = targetIndex;
                    DTG.ScrollIntoView(DTG.Items[targetIndex]);

                    // Ensure selection and focus are appropriately set.
                    DTG.Dispatcher.InvokeAsync(() =>
                    {
                        DTG.Focus();
                        if ((DataGridRow)DTG.ItemContainerGenerator.ContainerFromIndex(targetIndex) is null)
                        {
                            DTG.UpdateLayout(); // Force layout update
                        }
                        DTG.SelectedItem = DTG.Items[targetIndex];

                        var ColumnIndexy = 0;
                        if (DTG.CurrentColumn is not null)
                        {
                            ColumnIndexy = DTG.CurrentColumn.DisplayIndex;
                        }
                        else
                        {
                            int? defaultcolumnindex = DTG.Columns.FirstOrDefault(c => c.Visibility == Visibility.Visible && !c.IsReadOnly)?.DisplayIndex;
                            ColumnIndexy = Convert.ToInt32(defaultcolumnindex);
                        }
                        DTG.CurrentCell = new DataGridCellInfo(DTG.SelectedItem, DTG.Columns[ColumnIndexy]);

                        // It may not be always necessary, or even desired, to force the DataGrid row to focus.
                        // This logic attempts to focus the row only if the DataGrid is supposed to have focus.
                        //if (DTG.IsKeyboardFocusWithin)
                        //{
                        //}
                        DataGridRow dgRow = (DataGridRow)DTG.ItemContainerGenerator.ContainerFromIndex(targetIndex);
                        dgRow?.Focus();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch { }
            }
        }

        public static void FocusCellReadyToEdit(DataGrid dataGrid, string sortMemberPath, int rowIndex, bool isEditMode = true, bool ASYNCY = false)
        {
            if (dataGrid == null || rowIndex < 0 || rowIndex >= dataGrid.Items.Count)
                return;

            // Ensure the DataGrid is updated and the item is scrolled into view.
            object item = dataGrid.Items[rowIndex];
            dataGrid.ScrollIntoView(item);
            dataGrid.SelectedIndex = rowIndex;
            dataGrid.Focus();
            Keyboard.Focus(dataGrid);

            // Find the column by SortMemberPath.
            DataGridColumn column = dataGrid.Columns.FirstOrDefault(c => c.SortMemberPath == sortMemberPath);
            if (column == null)
                return;

            // Get the cell to focus based on the row and column.
            DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row == null)
            {
                dataGrid.UpdateLayout();
                try
                {
                    if (ASYNCY)
                    {
                        row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                        if (row != null)
                        {
                            // Set the SelectedItem or SelectedIndex to ensure a selection change is recognized.
                            dataGrid.SelectedItem = item;
                            dataGrid.CurrentCell = new DataGridCellInfo(item, column);
                        }
                    }
                    else
                    {
                        dataGrid.Dispatcher.Invoke(() =>
                        {
                            row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                            if (row != null)
                            {
                                // Set the SelectedItem or SelectedIndex to ensure a selection change is recognized.
                                dataGrid.SelectedItem = item;
                                dataGrid.CurrentCell = new DataGridCellInfo(item, column);
                                if (isEditMode)
                                    dataGrid.BeginEdit();
                            }
                        }, DispatcherPriority.Background); ////DispatcherPriority.Background
                    }
                }
                catch (Exception) { }
            }
            else
            {
                // If the row container is not found, try to force the DataGrid to generate the row container.

                dataGrid.SelectedItem = item;
                dataGrid.CurrentCell = new DataGridCellInfo(item, column);
                if (isEditMode)
                    dataGrid.BeginEdit();

                dataGrid.Dispatcher.Invoke(() =>
                {
                    DataGridCell cell = CL_LMethods.GetCell(dataGrid, row, column.DisplayIndex);
                    if (cell != null)
                        cell.Focus();
                }, DispatcherPriority.Background);
            }
        }

        public static DataGridColumn GetLastColumn(DataGrid dataGrid)
        {
            if (dataGrid is null)
            {
                return null;
            }

            for (int i = dataGrid.Columns.Count - 1; i >= 0; i--)
            {
                DataGridColumn column = dataGrid.Columns[i];
                if (column != null)
                {
                    if (column.Visibility == System.Windows.Visibility.Visible && !column.IsReadOnly)
                    {
                        // This column is visible and focusable, return it
                        return column;
                    }
                }
            }
            // If no focusable columns were found, return null
            return null;
        }

        #region FOR_WINDOW
        /// <summary>
        /// Disable Whole Winodow (Everything in Window)
        /// </summary>
        /// <param name="MyWindy"></param>
        /// <param name="Allisenable"></param>
        public static void AllowEdits_Windall(string MyWindy, bool Allisenable)
        {

            bool SEE = false, INSER_UPDT = false, DEL = false;

            var TheWind = Application.Current.Windows.OfType<Window>().SingleOrDefault(win => win.GetType().Name.Equals(MyWindy));

            if (SEE == false)
            {
                //Disable whole window Items only see
                TheWind.IsEnabled = false;
                return;
            }
            if (INSER_UPDT == false)
            {
                //Disable Save Buttons
            }
            if (DEL == false)
            {
                //Disable Delete Buttons
            }


            if (Allisenable == true)
            {
                TheWind.IsEnabled = true;
            }
            else
            {
                TheWind.IsEnabled = false;
            }
        }

        /// <summary>
        /// Only Inside Grid you say
        /// Your Control.TAG == "RD" معنیش میشه کنترلی که باید ریدونلی بمونه و دستکاری نشه
        /// </summary>
        /// <param name="gridy"></param>
        /// <param name="CanI">All False or true Lock Controls</param>
        /// <param name="EstesnaELMNT"> What controle Should ignore </param>
        public static void AllowEdits(Grid gridy, bool CanI, params FrameworkElement[] EstesnaELMNT)
        {
            //List<FrameworkElement> ESTL = new List<FrameworkElement>();

            foreach (UIElement ELMNT in gridy.Children)
            {
                //ESTL = EstesnaELMNT.AsEnumerable().ToList();
                //var EstesnaList = ESTL.Select(dataRow =>  dataRow.Name.Equals(((FrameworkElement)ELMNT).Name.ToString())).FirstOrDefault();
                var IsCurrentEstesna = Array.Find(EstesnaELMNT, element => element.Name == ((FrameworkElement)ELMNT).Name.ToString());


                if (ELMNT is TextBox)
                {
                    var test = ((TextBox)ELMNT).Tag;
                }


                bool IsNotRD = true; //ReadOnly نترلی که نباید بهش دست زد مثل نامبر
                if (((FrameworkElement)ELMNT).Tag.ToStringNullSafe() == "RD")
                {
                    IsNotRD = false; // Mean's ReadOnly Control true yes 
                }

                if (IsCurrentEstesna is null && IsNotRD) // اگر کنترل مد جاری جزو استثناات و کنترل غیر قابل تغییر نیست ادامه بده
                {

                    if (CanI == true)// Free and they are Editable
                    {
                        if (ELMNT is TextBox) ((TextBox)ELMNT).IsReadOnly = false;
                        //if (ELMNT is DataGrid) ((DataGrid)ELMNT).IsReadOnly = false;
                        if (ELMNT is DataGrid) ((DataGrid)ELMNT).IsEnabled = true;
                        if (ELMNT is ScrollViewer) ((ScrollViewer)ELMNT).IsEnabled = true;

                        if (ELMNT is ComboBox) ((ComboBox)ELMNT).IsEnabled = true;
                        if (ELMNT is CheckBox) ((CheckBox)ELMNT).IsEnabled = true;
                        if (ELMNT is TextBlock) ((TextBlock)ELMNT).IsEnabled = true;
                        if (ELMNT is Button) ((Button)ELMNT).IsEnabled = true;
                        if (ELMNT is Label) ((Label)ELMNT).IsEnabled = true;
                        if (ELMNT is RadioButton) ((RadioButton)ELMNT).IsEnabled = true;

                        if (ELMNT is Canvas) ((Canvas)ELMNT).IsEnabled = true;

                        if (ELMNT is TabControl) ((TabControl)ELMNT).IsEnabled = true;
                        if (ELMNT is TabItem) ((TabItem)ELMNT).IsEnabled = true;
                    }
                    if (CanI == false)// Locked and Its ReadOnly
                    {
                        if (ELMNT is TextBox) ((TextBox)ELMNT).IsReadOnly = true;
                        //if (ELMNT is DataGrid) ((DataGrid)ELMNT).IsReadOnly = true;
                        if (ELMNT is DataGrid) ((DataGrid)ELMNT).IsEnabled = false;
                        if (ELMNT is ScrollViewer) ((ScrollViewer)ELMNT).IsEnabled = false;

                        if (ELMNT is ComboBox) ((ComboBox)ELMNT).IsEnabled = false;
                        if (ELMNT is CheckBox) ((CheckBox)ELMNT).IsEnabled = false;
                        if (ELMNT is TextBlock) ((TextBlock)ELMNT).IsEnabled = false;
                        if (ELMNT is Button) ((Button)ELMNT).IsEnabled = false;
                        if (ELMNT is Label) ((Label)ELMNT).IsEnabled = false;
                        if (ELMNT is RadioButton) ((RadioButton)ELMNT).IsEnabled = false;

                        if (ELMNT is Canvas) ((Canvas)ELMNT).IsEnabled = false;

                        if (ELMNT is TabControl) ((TabControl)ELMNT).IsEnabled = false;
                        if (ELMNT is TabItem) ((TabItem)ELMNT).IsEnabled = false;
                    }
                }
            }


        }
        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // اگر نیازی نیست، فقط برگردان IntPtr.Zero
            return IntPtr.Zero;
        }
        public static void AllowDeletions(string YourGettypWindowName, bool CanI, IntPtr windowHandle)
        {
            //if (windowHandle == IntPtr.Zero)
            //{
            //    // Handle the case where windowHandle is zero, possibly logging or notifying the user.
            //    return;
            //}
            //var hwndSource = System.Windows.Interop.HwndSource.FromHwnd(windowHandle);
            //if (hwndSource == null)
            //{
            //    // Handle the case where hwndSource could not be obtained.
            //    return;
            //}

            // Check if the handle is valid before proceeding *******************
            //if (windowHandle == IntPtr.Zero || windowHandle == null)
            //{
            //    // Log the issue or handle it gracefully
            //    Debug.WriteLine("Warning: Invalid window handle (zero) passed to AllowDeletions");
            //    return; // Exit the method instead of throwing an exception
            //}


            var TheWind = (Window)System.Windows.Interop.HwndSource.FromHwnd(windowHandle).RootVisual;
            //var TheWind = Application.Current.Windows.OfType<Window>().SingleOrDefault(win => win.GetType().Name.Equals(YourGettypWindowName));

            // 1) از HWND یک HwndSource بگیرید
            var source = HwndSource.FromHwnd(windowHandle);

            // 2) نصب هُک
            source.AddHook(HwndHook);

            // Free and they are Editable
            if (CanI == false)
            {
                var TheDeletorBtn = FindVisualChildren<Button>(TheWind).Where(x => x.Tag != null && x.Tag.ToString() == "hazfer").ToList();
                foreach (var item in TheDeletorBtn)
                {
                    item.IsEnabled = false;
                    //item.Visibility = Visibility.Hidden;
                }
            }
            // Locked and Its ReadOnly
            if (CanI == true)
            {
                var TheDeletorBtn = FindVisualChildren<Button>(TheWind).Where(x => x.Tag != null && x.Tag.ToString() == "hazfer").ToList();
                foreach (var item in TheDeletorBtn)
                {
                    item.IsEnabled = true;
                    //item.Visibility = Visibility.Visible;
                }
            }

            // 5) حتماً هُک را پاک کن
            source.RemoveHook(HwndHook);
        }


        #endregion


        public static void DoGetwriteAppenLog(string text, string NAMFIL = null, string dirPath = null)
        {
            if (string.IsNullOrEmpty(dirPath)) dirPath = "C:\\Correct\\ERSOME";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            if (NAMFIL is null) NAMFIL = $"ER1{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";

            if (!File.Exists(dirPath + NAMFIL)) File.Create(dirPath + NAMFIL).Dispose();
            if (File.Exists(dirPath + NAMFIL) && File.ReadAllText(dirPath + NAMFIL) != "") text = Environment.NewLine + text;

            try
            {
                using (StreamWriter sw = File.AppendText(dirPath + NAMFIL))
                {
                    sw.Write(text);
                }
            }
            catch (Exception) { }
        }
        public static void DoWritePRGLOG(string _YOUR_TEXT_, Exception er = null)
        {
            const string _thedirpath = @"C:\CORRECT\ERLG\";
            if (!Directory.Exists(_thedirpath)) Directory.CreateDirectory(_thedirpath);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            string filePath = System.IO.Path.Combine(_thedirpath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                if (er is not null)
                {
                    writer.WriteLine($"Message: {er.Message}{Environment.NewLine}StackTrace: {er.StackTrace}{Environment.NewLine}Date: {DateTime.Now}");
                }
                else
                {
                    writer.WriteLine($"Message: {_YOUR_TEXT_}{Environment.NewLine}Date: {DateTime.Now}");
                }
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
        public static void DoWriteMyLog(string _YOUR_TEXT_, Exception er = null)
        {
            const string _thedirpath = @"C:\CORRECT\LOGY\";
            if (!Directory.Exists(_thedirpath)) Directory.CreateDirectory(_thedirpath);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            string filePath = System.IO.Path.Combine(_thedirpath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                if (er is not null)
                {
                    writer.WriteLine($"Message: {er.Message}{Environment.NewLine}StackTrace: {er.StackTrace}{Environment.NewLine}Date: {DateTime.Now}");
                }
                else
                {
                    writer.WriteLine($"Message: {_YOUR_TEXT_}{Environment.NewLine}Date: {DateTime.Now}");
                }
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }

        public class ProcLoader_OLD : IDisposable
        {
            private readonly object _lock = new object();
            private readonly ConcurrentDictionary<int, Process> _runningProcesses = new ConcurrentDictionary<int, Process>();
            private readonly List<string> _tempExePaths = new List<string>();
            public Process Start(string resourceName = null, string arguments = null)
            {
                resourceName = "Prg_UI.UiDrive.EXEFILES.Preloader.exe";
                arguments = "1354";
                lock (_lock)
                {
                    // Read the embedded EXE file as a byte array
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        byte[] exeBytes = new byte[stream.Length];
                        stream.Read(exeBytes, 0, exeBytes.Length);

                        // Save the byte array to a temporary file
                        string tempExePath = Path.GetTempFileName() + ".exe";
                        _tempExePaths.Add(tempExePath); // Store the temp path for later cleanup

                        File.WriteAllBytes(tempExePath, exeBytes);

                        // Start the process from the byte array with arguments
                        var process = new Process
                        {
                            StartInfo =
                            {
                                FileName = tempExePath,
                                Arguments = arguments,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                              }
                        };
                        process.Start();
                        // Store the process in the dictionary
                        _runningProcesses.TryAdd(process.Id, process);

                        return process;
                    }
                }
            }
            public void Stop(Process process)
            {
                if (process == null)
                {
                    return;
                }
                lock (_lock)
                {
                    if (_runningProcesses.TryRemove(process.Id, out var removedProcess))
                    {
                        removedProcess.Kill();
                        removedProcess.WaitForExit();
                        removedProcess.Dispose();
                        // Clean up temporary file
                        string tempExePath = _tempExePaths.FirstOrDefault(p => string.Equals(p, removedProcess.StartInfo.FileName, StringComparison.OrdinalIgnoreCase));
                        if (tempExePath != null)
                        {
                            try
                            {
                                File.Delete(tempExePath);
                                _tempExePaths.Remove(tempExePath);
                            }
                            catch (Exception) {/*OnErrorResumeNext*/ }
                        }
                    }
                }
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    foreach (var process in _runningProcesses.Values)
                    {
                        Stop(process);
                    }
                }
            }
        }

        public static class ProcLoader // This is Current use
        {
            private static readonly object _lock = new object();
            private static readonly ConcurrentDictionary<int, Process> _runningProcesses = new ConcurrentDictionary<int, Process>();
            private static readonly List<string> _tempExePaths = new List<string>();

            public static Process Start(string resourceName = null, string arguments = null)
            {
                resourceName = "Prg_UI.UiDrive.EXEFILES.Preloader.exe";
                arguments = "1354";
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] exeBytes = new byte[stream.Length];
                    stream.Read(exeBytes, 0, exeBytes.Length);

                    // Save the byte array to a temporary file
                    string tempExePath = Path.GetTempFileName() + ".exe";
                    _tempExePaths.Add(tempExePath); // Store the temp path for later cleanup

                    File.WriteAllBytes(tempExePath, exeBytes);

                    // Start the process from the byte array with arguments
                    var process = new Process
                    {
                        StartInfo =
                            {
                                FileName = tempExePath,
                                Arguments = arguments,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                    };
                    process.Start();

                    // Store the process in the dictionary
                    _runningProcesses.TryAdd(process.Id, process);

                    return process;
                }
            }

            public static void Stop(Process process)
            {
                if (process == null)
                {
                    return;
                }
                if (_runningProcesses?.FirstOrDefault() != null)
                {
                    if (_runningProcesses.TryRemove(process.Id, out var removedProcess))
                    {
                        removedProcess.Kill();
                        removedProcess.WaitForExit();
                        removedProcess.Dispose();

                        // Clean up temporary file
                        string tempExePath = _tempExePaths.FirstOrDefault(p => string.Equals(p, removedProcess.StartInfo.FileName, StringComparison.OrdinalIgnoreCase));
                        if (tempExePath != null)
                        {
                            try
                            {
                                File.Delete(tempExePath);
                                _tempExePaths.Remove(tempExePath);
                            }
                            catch (Exception)
                            {
                                // Handle the exception as needed
                            }
                        }
                    }
                }

            }
            public static void Dispose()
            {
                foreach (var process in _runningProcesses.Values)
                {
                    Stop(process);
                }
            }
        }

        public static double GetWindowSizeLessTaskbaer()
        {
            var screenWorkingArea = SystemParameters.WorkArea;
            var screenBounds = SystemParameters.FullPrimaryScreenHeight;
            var taskbarHeight = screenBounds - screenWorkingArea.Height;
            return screenWorkingArea.Height - taskbarHeight;
        }

        public static bool IsValidDouble(string text)
        {
            double result;
            //return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            return double.TryParse(text, out result);
        }

        public static bool CodeMeliIsValid(string input)
        {
            bool CodeIsValid = true;

            char[] chArray = input.ToCharArray();
            int[] numArray = new int[chArray.Length];
            for (int i = 0; i < chArray.Length; i++)
            {
                numArray[i] = (int)char.GetNumericValue(chArray[i]);
            }
            int num2 = numArray[9];
            switch (input)
            {
                case "0000000000":
                case "1111111111":
                case "22222222222":
                case "33333333333":
                case "4444444444":
                case "5555555555":
                case "6666666666":
                case "7777777777":
                case "8888888888":
                case "9999999999":
                    CodeIsValid = false;
                    break;
            }
            int num3 = ((((((((numArray[0] * 10) + (numArray[1] * 9)) + (numArray[2] * 8)) + (numArray[3] * 7)) + (numArray[4] * 6)) + (numArray[5] * 5)) + (numArray[6] * 4)) + (numArray[7] * 3)) + (numArray[8] * 2);
            int num4 = num3 - ((num3 / 11) * 11);
            if ((((num4 == 0) && (num2 == num4)) || ((num4 == 1) && (num2 == 1))) || ((num4 > 1) && (num2 == Math.Abs((int)(num4 - 11)))))
            {
                CodeIsValid = false;
            }
            else
            {
                CodeIsValid = false;
            }

            return CodeIsValid;
        }

        public static byte[] GetImageBytes(BitmapImage bitmapImage)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return null;

            try
            {
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze(); // Make thread-safe
                    return image;
                }
            }
            catch (Exception)
            {
                // Try to handle OLE format images (often from legacy databases)
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Copy data from the original byte array, skipping the OLE header.
                        ms.Write(byteArray, 78, byteArray.Length - 78);

                        // Create a BitmapImage.
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // Image is cached on load
                        ms.Seek(0, SeekOrigin.Begin); // Rewind the stream.
                        image.StreamSource = ms; // Use the MemoryStream as the source
                        image.EndInit(); // Initializes the image
                        image.Freeze(); // Make the BitmapImage thread-safe

                        return image;
                    }
                }
                catch
                {
                    // If all attempts fail, return null
                    return null;
                }
            }
        }

        // Add a method to convert InkCanvas to byte array as PNG
        public static byte[] ConvertInkCanvasToPng(System.Windows.Controls.InkCanvas inkCanvas)
        {
            if (inkCanvas == null)
                return null;

            // Get the size of the InkCanvas
            int width = (int)inkCanvas.ActualWidth;
            int height = (int)inkCanvas.ActualHeight;

            // Create a render bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                width, height, 96, 96, System.Windows.Media.PixelFormats.Default);

            // Render the InkCanvas
            rtb.Render(inkCanvas);

            // Create a PNG encoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            // Save to memory stream
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
        public static BitmapImage ByteArrayToBitmapImage11111111111111(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return null;

            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    stream.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = stream;

                    image.EndInit();
                    image.Freeze(); // Makes the image usable across threads
                    return image;
                }
                catch (Exception ex)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Copy data from the original byte array, skipping the OLE header.
                        ms.Write(byteArray, 78, byteArray.Length - 78);

                        // Create a BitmapImage.
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // Image is cached on load
                        ms.Seek(0, SeekOrigin.Begin); // Rewind the stream.
                        image.StreamSource = ms; // Use the MemoryStream as the source
                        image.EndInit(); // Initializes the image
                        image.Freeze(); // Make the BitmapImage thread-safe

                        return image;
                    }
                }
            }
        }
        public static byte[] CompressImage(BitmapImage bitmapImage, int ImageQualityTreshHold = 50)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                var originalImageBytes = stream.ToArray();

                using (MagickImage image = new MagickImage(originalImageBytes))
                {
                    image.Format = MagickFormat.Jpg; // Get or Set the format of the image.
                    //image.Resize(40, 40); // Fit the image into the requested width and height. 
                    image.Quality = (uint)ImageQualityTreshHold; // This is the compression level.

                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Write(ms);
                        byte[] imageBytes = ms.ToArray();
                        return imageBytes;
                    }
                }
            }
        }
        public static System.Drawing.Image ConvertBitmapSourceToDrawingImage(BitmapSource bitmapSource)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return System.Drawing.Image.FromStream(memoryStream);
            }
        }
        public static bool EmailIsValid(string input)
        {
            return Regex.IsMatch(input, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static string[] imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };
        public static string FindImageFile(string sharedFolderPath, string productCode)
        {
            // Search for an image file that matches the product code with any of the specified extensions
            foreach (var ext in imageExtensions)
            {
                var filePath = Path.Combine(sharedFolderPath, productCode + ext);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return null; // Return null if no matching file is found
        }

        public static bool IsAllowedOpen(string userCode, string menuItemIdentifier)
        {
            try
            {
                // Check if menuItemIdentifier ends with "_WIN" and remove it if it does
                //if (menuItemIdentifier.EndsWith("_WIN", StringComparison.OrdinalIgnoreCase))
                //{
                //    menuItemIdentifier = menuItemIdentifier.Substring(0, menuItemIdentifier.Length - 4);
                //}

                var lines = File.ReadAllLines(CL_Generaly.FILEACCESSPATH);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var fileUserCode = parts[0].Trim(); //1
                        var shortName = parts[1].Trim(); //2
                        var isAllowed = Convert.ToBoolean(Convert.ToByte(parts[2].Trim())); //3

                        // Check if the current line pertains to the user we're interested in
                        if (fileUserCode == userCode)
                        {
                            // Check if the menu item identifier matches either the ShortName or WIN_NAME
                            if (shortName.Equals(menuItemIdentifier, StringComparison.OrdinalIgnoreCase))
                            {
                                return isAllowed;
                            }
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// SelectedItems[i]
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsNewPlaceHolder(DataGrid dg, object item)
        {
            // Check if the DataGrid's collection view is in the process of adding a new item
            var collectionView = dg.Items as IEditableCollectionView;
            bool isAddingNewItem = collectionView?.IsAddingNew ?? false;

            // Check if the item is the special placeholder for new items
            bool isNewItemPlaceholder = item is null || item.ToStringNullSafe() == "{NewItemPlaceholder}";

            // Check if the item is the internal "NamedObject", used as placeholders in some cases
            bool isInternalNamedObject = item?.GetType().FullName == "MS.Internal.NamedObject";

            bool CV_NewItemPlaceholder = item == CollectionView.NewItemPlaceholder;

            // Return true if any of the conditions indicate this is a placeholder
            return isNewItemPlaceholder || isInternalNamedObject || isAddingNewItem || CV_NewItemPlaceholder;
        }

        public static void RefreshAfterDataGridRemvoing(DataGrid DG)
        {
            var before = DG.CanUserAddRows;
            DG.CanUserAddRows = !before;
            DG.CanUserAddRows = before;
        }

        /// <summary>
        /// Sets TabIndexes for the provided elements in the specified order.
        /// </summary>
        /// <param name="elements">Array of FrameworkElement to set TabIndexes for.</param>
        public static void SetTabIndexes(params FrameworkElement[] elements)
        {
            if (elements == null || elements.Length == 0) return;

            ////for (int i = -2147483647; i <= 2147483647; i++)
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var propertyInfo = element.GetType().GetProperty("TabIndex");
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(element, i);
                }
            }
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }
        public static bool IsWindowOpen(Type windowType)
        {
            return Application.Current.Windows.Cast<Window>().Any(w => w.GetType() == windowType);
        }
        public static bool IsWindowOpen<T>() where T : Window
        {
            return Application.Current.Windows.OfType<T>().Any();
        }

        public static (bool IsValid, string ErrorMessage) IsValidPercentage(string input, bool allowNegative = false, bool allowOver100 = false, int decimalPlaces = 2)
        {
            // Trim the input to remove any leading/trailing whitespace
            input = input.Trim();

            // Check if the input is empty
            if (string.IsNullOrEmpty(input))
            {
                return (false, "درصد نمی تواند خالی باشد!");
            }

            // Remove percentage symbol if present
            if (input.EndsWith("%"))
            {
                input = input.TrimEnd('%');
            }

            // Try parsing the input as a decimal
            if (!decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal percentage))
            {
                return (false, "عدد وارد شده صحیح نیست!.");
            }

            // Check if negative percentages are allowed
            if (!allowNegative && percentage < 0)
            {
                return (false, "عدد منفی قابل قبول نیست.");
            }

            // Check if percentages over 100 are allowed
            if (!allowOver100 && percentage > 100)
            {
                return (false, "درصد بیش از عدد 100 مجاز نیست.");
            }

            // Check for the correct number of decimal places
            string[] parts = input.Split('.');
            if (parts.Length > 1 && parts[1].Length > decimalPlaces)
            {
                return (false, $"Maximum {decimalPlaces} decimal places allowed.");
            }

            // If we've made it this far, the percentage is valid
            return (true, "Valid percentage.");
        }

        public static bool IsValidTime24(string timeText)
        {
            // Check if the input matches the time format
            if (TimeSpan.TryParseExact(timeText, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan parsedTime))
            {
                // Ensure the time is within the valid range
                return parsedTime.Hours >= 0 && parsedTime.Hours < 24 &&
                       parsedTime.Minutes >= 0 && parsedTime.Minutes < 60 &&
                       parsedTime.Seconds >= 0 && parsedTime.Seconds < 60;
            }
            return false;
        }

        #region WIN_MANAGEMENT
        // Dictionary to track active windows using weak references to prevent memory leaks
        // Key: Window Type, Value: List of weak references to window instances
        private static readonly Dictionary<Type, List<WeakReference<Window>>> _activeWindows = new();

        // Lock object for thread-safe operations on the dictionary
        private static readonly object _lockObject = new();

        /// <summary>
        /// Main method to handle window opening with comprehensive scenario handling
        /// </summary>
        public static void OpenWindow(Window? owner, Window newWindow, bool isModalDialog = false, bool allowMultipleInstances = true, bool isSetOwner = false)
        {
            // Validate that newWindow is not null
            if (newWindow == null) throw new ArgumentNullException(nameof(newWindow));

            // Ensure we're on the UI thread, if not, redirect to UI thread
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() =>
                    OpenWindow(owner, newWindow, isModalDialog, allowMultipleInstances, isSetOwner));
                return;
            }

            try
            {
                // If multiple instances aren't allowed, check for existing window
                if (!allowMultipleInstances)
                {
                    var existingWindow = FindExistingWindow(newWindow.GetType());
                    if (existingWindow != null)
                    {
                        FocusExistingWindow(existingWindow);
                        return;
                    }
                }

                // Setup window properties and event handlers
                ConfigureWindowSettings(owner, newWindow, isSetOwner);

                // Add window to tracking system
                RegisterWindowTracking(newWindow);

                // Display window with proper focus handling
                ShowWindowWithFocus(owner, newWindow, isModalDialog);
            }
            catch (Exception ex)
            {
                HandleWindowError(ex);
            }
        }
        private static Window? FindExistingWindow(Type windowType)
        {
            // Thread-safe access to window dictionary
            lock (_lockObject)
            {
                // Try to get list of windows of specified type
                if (_activeWindows.TryGetValue(windowType, out var windowRefs))
                {
                    // Clean up any dead references
                    windowRefs.RemoveAll(wr => !wr.TryGetTarget(out _));

                    // Find first non-closing, loaded window
                    foreach (var weakRef in windowRefs)
                    {
                        if (weakRef.TryGetTarget(out var window) &&
                            window.IsLoaded &&
                            !window.IsClosing())
                        {
                            return window;
                        }
                    }
                }
                return null;
            }
        }

        private static void RegisterWindowTracking(Window window)
        {
            // Thread-safe registration of new window
            lock (_lockObject)
            {
                var windowType = window.GetType();

                // Create new list if type doesn't exist
                if (!_activeWindows.ContainsKey(windowType))
                {
                    _activeWindows[windowType] = new List<WeakReference<Window>>();
                }

                // Add weak reference to window
                _activeWindows[windowType].Add(new WeakReference<Window>(window));

                // Clean up when window is closed
                window.Closed += (s, e) =>
                {
                    lock (_lockObject)
                    {
                        if (_activeWindows.TryGetValue(windowType, out var windows))
                        {
                            windows.RemoveAll(wr => !wr.TryGetTarget(out var w) || w == window);

                            // If no more windows of this type, remove the entry completely
                            if (windows.Count == 0)
                            {
                                _activeWindows.Remove(windowType);
                            }
                        }
                    }
                };
            }
        }
        private static void ConfigureWindowSettings(Window? owner, Window newWindow, bool isSetOwner)
        {
            // Set owner relationship if specified
            if (isSetOwner && owner != null && owner is not Prg_UI.Wins.WinBase && owner != newWindow)
            {
                newWindow.Owner = owner;

                // Close child window when owner closes
                owner.Closing += (s, e) =>
                {
                    if (!newWindow.IsClosing())
                    {
                        newWindow.Close();
                    }
                };
            }

            // Center window on screen to prevent off-screen placement
            newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Ensure window appears on top when loaded
            newWindow.Loaded += (s, e) => EnsureProperZOrder(newWindow);

            // Handle window state changes (prevent minimization)
            newWindow.StateChanged += (s, e) => HandleWindowStateChange(newWindow);

            // Add key down handler for Ctrl+F4
            newWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;

                    // Ensure we properly clean up from tracking before closing
                    UnregisterWindowFromTracking(newWindow);

                    // Close the window directly
                    newWindow.Close();
                }
            };

            // Ensure proper cleanup when the window is closing
            newWindow.Closing += (s, e) =>
            {
                // Make sure we're unregistered from tracking
                UnregisterWindowFromTracking(newWindow);
            };
        }

        // New method to properly unregister a window from tracking
        private static void UnregisterWindowFromTracking(Window window)
        {
            var windowType = window.GetType();

            lock (_lockObject)
            {
                if (_activeWindows.TryGetValue(windowType, out var windowRefs))
                {
                    windowRefs.RemoveAll(wr => !wr.TryGetTarget(out var w) || w == window);

                    // If no more windows of this type, remove the entry completely
                    if (windowRefs.Count == 0)
                    {
                        _activeWindows.Remove(windowType);
                    }
                }
            }
        }
        private static void ShowWindowWithFocus(Window? owner, Window newWindow, bool isModalDialog)
        {
            // Create timer to ensure focus after window is shown
            var focusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            focusTimer.Tick += (s, e) =>
            {
                focusTimer.Stop();
                EnsureWindowFocus(newWindow);
            };

            // Show window either modal or non-modal
            if (isModalDialog)
            {
                newWindow.ShowDialog();
            }
            else
            {
                newWindow.Show();
                focusTimer.Start();
            }

            //focusTimer.Stop();
            //focusTimer.IsEnabled = false;
            //focusTimer = null;
        }

        private static void EnsureWindowFocus(Window window)
        {
            // Restore window if minimized
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            // Get native window handle
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                // Use Windows API to force window to foreground
                NativeMethods.SetForegroundWindow(hwnd);
            }

            // Multiple focus attempts to ensure window is active
            window.Activate();
            window.Focus();

            // Focus first focusable element in window
            var firstFocusable = window.GetFirstFocusableElement();
            firstFocusable?.Focus();
        }
        private static void EnsureProperZOrder(Window window)
        {
            // Get native window handle
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                // Use Windows API to set window position in Z-order
                NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                    SetWindowPosFlags.NoMove | SetWindowPosFlags.NoSize |
                    SetWindowPosFlags.NoActivate | SetWindowPosFlags.ShowWindow);
            }
        }
        private static void HandleWindowStateChange(Window window)
        {
            //if (window.WindowState == WindowState.Minimized)
            //{
            //    window.WindowState = WindowState.Normal;
            //}
        }
        private static void HandleWindowError(Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new Msgwin(false, "خطا در باز کردن پنجره مورد نظر").ShowDialog();
            });
        }

        private static void FocusExistingWindow(Window window)
        {
            try
            {
                // Restore window if minimized
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                // Force activation using Windows API
                var windowHandle = new WindowInteropHelper(window).Handle;
                SetForegroundWindow(windowHandle);

                // Use WPF methods to ensure activation and focus
                window.Activate();
                window.Focus();

                // Temporarily set as topmost to force to front
                bool originalTopmost = window.Topmost;
                window.Topmost = true;
                window.Topmost = originalTopmost;

                // Focus the first focusable element
                window.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    var firstFocusable = window.GetFirstFocusableElement();
                    firstFocusable?.Focus();
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error focusing window: {ex.Message}");
            }
        }

        // P/Invoke for Windows API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Helper extension method
        //private static IInputElement GetFirstFocusableElement(Window window)
        //{
        //    return window.GetDescendantElements()
        //                .OfType<IInputElement>()
        //                .FirstOrDefault(e => e.Focusable && e.IsEnabled);
        //}

        // Helper extension method
        private static IEnumerable<DependencyObject> GetDescendantElements(this DependencyObject parent)
        {
            if (parent == null) yield break;

            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
                foreach (var descendant in GetDescendantElements(child))
                {
                    yield return descendant;
                }
            }
        }

        // Native Windows API methods for window management
        internal static class NativeMethods
        {
            // Import Windows API function to bring window to foreground
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            // Import Windows API function to set window position
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        }

        // Flags for SetWindowPos function
        [Flags]
        internal enum SetWindowPosFlags : uint
        {
            NoMove = 0x0002,      // Retains current position
            NoSize = 0x0001,      // Retains current size
            NoActivate = 0x0010,  // Does not activate window
            ShowWindow = 0x0040,  // Displays window
        }

        // Check if window is in process of closing
        public static bool IsClosing(this Window window)
        {
            return (bool?)typeof(Window).GetProperty("IsClosing",
                BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(window) ?? false;
        }

        // Get first focusable element in window
        public static FrameworkElement? GetFirstFocusableElement(this Window window)
        {
            return window.Content as FrameworkElement;
        }
        #endregion


        public static void CleanupBeforeExiting()
        {

            try { ProcLoader.Dispose(); } catch (Exception) { }

            try
            {
                CL_PRC_LOADER.Dispose(); //New Version
            }
            catch { }
        }

        public static void GoExitTheApplication()
        {
            CL_LMethods.CleanupBeforeExiting();

            var dispatcher = Application.Current.Dispatcher;

            if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                    return;

                Application.Current.Shutdown();

                //Commented because it may leads some error before cleaning up !
                //try { System.Environment.Exit(0); } catch { } //Just in case
            }));
        }

        public static bool IsNumeric(string input)
        {
            // Step 1: Handle null, empty, or whitespace strings
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input))
            {
                return false;
            }

            // Step 2: Trim any leading or trailing whitespace
            input = input.Trim();

            // Step 4: Try parsing the input as a double (to cover integers, decimals, and scientific notation)
            if (double.TryParse(input, out double result))
            {
                // Optional: Further checks for overflow or underflow can be done here
                if (double.IsInfinity(result) || double.IsNaN(result))
                {
                    return false; // Not valid if infinity or NaN
                }
                return true;
            }

            // Step 5: If the input cannot be parsed as a number, return false
            return false;
        }

        public static string LockReasonError(string ErrCode)
        {
            switch (ErrCode)
            {
                case "1": ErrCode = @"خطا شماره 1 - قفل شناسایی نـشد , قفل پشت سیستم نیست  یا اتصال آن درست بر قرار نیست"; break;
                case "101": ErrCode = "خطا شماره 101 -  قفل شناسایی نشد"; break;
                case "2": ErrCode = "خطا شماره 2 -  قفل مال متعلق به این نرم افزار نیست."; break;
                case "102": ErrCode = "خطا شماره 102 -  قفل متعلق به این نرم افزار نیست."; break;

                case "3": ErrCode = "خطا شماره 3 -  سرویس قفل را در محل قفل بررسی کنید و شبکه را نیز چک کنید."; break;
                case "5": ErrCode = "خطا شماره 5 -  معمولا این مشکل زمانی رخ میدهد که از نام کامپیوتر به جای آدرس آی پی استفاده شده باشد و یا آدرس آی پی اشتباه باشد."; break;
                case "6": ErrCode = @"خطا شماره 6 - احتمالا سرویس قفل غیر فعال است , آنرا بررسی کنید درضمن
                             معتبر بودن IP و ارتباط شبکه را بررسی نمایید.(پورت 5090 و 9051 را برای فایروال فعال کنید). از اتصال قفل به سیستم هم اطمینان حاصل فرمایید."; break;
                case "106": ErrCode = "معتبر بودن خطا شماره 106 -   IP و ارتباط شبکه را بررسی نمایید.(پورت 5090 و 9051 را برای فایروال فعال کنید)."; break;
                case "7": ErrCode = "خطا شماره 7 -  تعداد کاربران متصل بیش از حد مجاز است."; break;
                case "107": ErrCode = "خطا شماره 107 -  تعداد کاربران متصل بیش از حد مجاز است."; break;
                case "8": ErrCode = "خطا شماره 8 -  تنظیمات مربوط به قفل مجددانجام و بروز رسانی شود."; break;
                case "9": ErrCode = "خطا شماره 9 -  اکتیوایکس و سرویس را آپدیت کنید."; break;
                case "10": ErrCode = "خطا شماره 10 -  داده های مربوطه درست نیست."; break;
                case "11": ErrCode = "خطا شماره 11 -  تنظیمات قفل بروز شود."; break;
                default:
                    ErrCode = "داده های مربوطه به قفل صحیح نیست.";
                    break;
            }
            return ErrCode;
        }


        #region MyRegion
        public class RestrictionInfo
        {
            private string _whereClause = " WHERE ";
            public string WhereClause
            {
                get => _whereClause ?? " WHERE ";
                set => _whereClause = value ?? " WHERE ";
            }
            public List<string> RestrictionMessages { get; set; } = new List<string>();
        }
        private static RestrictionInfo GenerateRestrictedSqlQueryInfo(byte TAGCODE, string DEF_VALUE = " WHERE ", bool isOthery = false)
        {
            var info = new RestrictionInfo
            {
                WhereClause = DEF_VALUE
            };

            CL_CCNNMANAGER? dbms = new CL_CCNNMANAGER();

            string GetAndQreOrNo()
            {
                if (info.WhereClause.ToUpper().Contains("WHERE"))
                {
                    if (string.IsNullOrEmpty(info.WhereClause.Replace("WHERE", "").Trim()) || string.IsNullOrWhiteSpace(info.WhereClause.Replace("WHERE", "").Trim()))
                    {
                        return "";
                    }
                    else
                    {
                        return " AND ";
                    }
                }
                else
                {
                    return " WHERE ";
                }
            }

            bool CanSeeAllInvoice = CL_HESABDARI.LETSGO("FRSKB"); //فاکتور فروش سایر کاربران را بتواند ببیند
            bool IsOnlyDepartemanVahed = CL_HESABDARI.LETSGO("DEPEMAL"); // اگر محدود به واحد خودش هست
            bool IsZirMajmoehChart = CL_HESABDARI.LETSGO("chartfilter"); // اگر محدود به زير مجموعه خودش هست
            bool IsDateLimited = !CL_HESABDARI.LETSGO("DECD"); //تاریخ قابل برگشت اعمال نشود

            if (!CanSeeAllInvoice) //به همه فاکتور ها دسترسی نداره!
            {
                if (!isOthery && IsDateLimited) //تاریخ قابل برگشت اعمال شود همراه با محدود به کاربری خودش
                {
                    info.RestrictionMessages.Add("محدود به تاریخ برگشت فاکتور");
                    var sqlQuery = $"SELECT TOP 100 PERCENT DATE_N FROM dbo.HEAD_LST WHERE (TAG = {TAGCODE}) AND (DEPATMAN = {CL_Generaly.VAHED_OF_USER}) AND (dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}') GROUP BY DATE_N ORDER BY DATE_N DESC";
                    var result = dbms.DoGetDataSQL<long>(sqlQuery).ToList(); //Get Last New Bigest Date

                    if (result.Count > 0 && Convert.ToDouble(Baseknow.CPI) > 0) //تعداد تاریخ قابل برگشت برای مشاهده
                    {
                        long? dateResult = null;
                        int index = Convert.ToInt32(Baseknow.CPI);
                        if (index >= 0 && index < result.Count)
                        {
                            dateResult = result[index]; // Safely access the index
                        }
                        else
                        {
                            dateResult = result.FirstOrDefault(); // Fallback to first item
                        }

                        info.WhereClause += dateResult > 0
                            ? $" {GetAndQreOrNo()} dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}' AND dbo.HEAD_LST.DATE_N >= {dateResult} "
                            : string.Empty;
                    }
                }
                else
                {
                    bool defaultUserRestriction = true;
                    if (IsOnlyDepartemanVahed) //فقط واحد های خودش
                    {
                        info.WhereClause += $" {GetAndQreOrNo()} dbo.HEAD_LST.DEPATMAN = {CL_Generaly.VAHED_OF_USER} ";
                        info.RestrictionMessages.Add("محدود به واحد سازمانی شما");
                        defaultUserRestriction = false;

                        if (IsZirMajmoehChart && Convert.ToBoolean(Baseknow.mrcorrect)) //علاوه بر واحد , محدود به زیرمجموعه هم بشه
                        {
                            string vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                            if (!string.IsNullOrEmpty(vs))
                            {
                                info.WhereClause += $"  {GetAndQreOrNo()} {vs} ";
                                info.RestrictionMessages.Add("محدود به زیرمجموعه‌های شما در چارت");
                            }
                        }
                    }
                    else if (IsZirMajmoehChart && Convert.ToBoolean(Baseknow.mrcorrect)) // فقط محدود به زیرمجموعه تعریف شده خودش بشه
                    {
                        string vs = CL_HESABDARI.UserOnChart(Convert.ToInt32(Baseknow.USERCOD));
                        if (!string.IsNullOrEmpty(vs))
                        {
                            info.WhereClause += $"  {GetAndQreOrNo()} {vs} ";
                            info.RestrictionMessages.Add("محدود به زیرمجموعه‌های شما در چارت");
                            defaultUserRestriction = false;
                        }
                    }

                    if (defaultUserRestriction)
                    {
                        //فقط فاکتور های کاربری خودش را ببیند
                        //info.WhereClause += $" {GetAndQreOrNo()} ((USER_NAME = N'{CL_HESABDARI.UCurrentUser()}') OR  (USER_NAME = N'{CL_LMethods.NormalizeArabicPersian(CL_HESABDARI.UCurrentUser().ToString())}')) ";
                        info.WhereClause += $" {GetAndQreOrNo()} ((dbo.HEAD_LST.USER_NAME = N'{CL_HESABDARI.UCurrentUser()}') OR  (dbo.HEAD_LST.USER_NAME = N'{CL_LMethods.NormalizeArabicPersian(CL_HESABDARI.UCurrentUser().ToString())}')) ";
                        info.RestrictionMessages.Add("فقط نمایش اطلاعات ثبت شده توسط شما");
                    }
                }
            }

            if (string.IsNullOrEmpty(info.WhereClause) || string.IsNullOrWhiteSpace(info.WhereClause) || info.WhereClause.Trim() == "WHERE")
            {
                info.WhereClause = string.Empty;
            }

            return info;
        }
        public static string GetRestrictedSqlQuery(byte TAGCODE, string DEF_VALUE = " WHERE ")
        {
            return GenerateRestrictedSqlQueryInfo(TAGCODE, DEF_VALUE).WhereClause;
        }
        public static RestrictionInfo GetRestrictedSqlQueryWithDetails(byte TAGCODE, string DEF_VALUE = " WHERE ", bool isOthery = false)
        {
            return GenerateRestrictedSqlQueryInfo(TAGCODE, DEF_VALUE, isOthery);
        }
        #endregion

        // Helper method to convert List to DataTable if needed
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            var properties = typeof(T).GetProperties();
            var dataTable = new DataTable();

            // Add columns
            foreach (var property in properties)
            {
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            // Add rows
            foreach (T item in data)
            {
                var values = new object[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public static class KeySimulator
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

            [DllImport("user32.dll")]
            public static extern uint MapVirtualKey(uint uCode, uint uMapType);

            const int KEYEVENTF_KEYDOWN = 0x0000; // key down flag
            const int KEYEVENTF_KEYUP = 0x0002; // key up flag

            public static void SendESC()
            {
                // ESC key code is 0x1B (27 in decimal)
                byte escKeyCode = 0x1B;

                // Simulate key press (KEYDOWN)
                keybd_event(escKeyCode, 0, KEYEVENTF_KEYDOWN, 0);

                // Simulate key release (KEYUP)
                keybd_event(escKeyCode, 0, KEYEVENTF_KEYUP, 0);
            }
        }

        #region SfDataGridMethodsCustom
        public static void FocusLastSfDataGridRow(SfDataGrid SYNCFUSION_DG)
        {
            try
            {
                // Ensure the grid and its view are properly initialized
                if (SYNCFUSION_DG == null || SYNCFUSION_DG.View == null || SYNCFUSION_DG.View.Records == null)
                {
                    // Optionally, handle or log that the grid is not ready
                    return;
                }

                // Check if the grid has any records
                if (SYNCFUSION_DG.View.Records.Count == 0)
                {
                    // Optionally, handle or inform that there are no records to select
                    return;
                }

                // Apply any pending changes to the view (in case data is loaded asynchronously)
                SYNCFUSION_DG.View.Refresh();

                // Get the last visible data record after filtering and sorting
                var lastDataRecord = SYNCFUSION_DG.View.Records.LastOrDefault(r => r != null && r.IsRecords);

                if (lastDataRecord == null)
                {
                    // No data records found (could be due to filtering)
                    return;
                }

                // Resolve the record index to the row index
                int recordIndex = SYNCFUSION_DG.View.Records.IndexOf(lastDataRecord);
                int rowIndex = SYNCFUSION_DG.ResolveToRowIndex(recordIndex);

                // Check for valid row index
                if (rowIndex < 0)
                {
                    // Optionally, handle invalid row index
                    return;
                }

                // Get the first visible column index
                var firstVisibleColumnIndex = SYNCFUSION_DG.GetFirstColumnIndex();

                // Create a valid RowColumnIndex
                var rowColumnIndex = new RowColumnIndex(rowIndex, firstVisibleColumnIndex);

                // Scroll the cell into view
                SYNCFUSION_DG.ScrollInView(rowColumnIndex); //SYNCFUSION_DG.ScrollInView(new RowColumnIndex(lastRowIndex, 0));

                // Select the row (SelectedIndex refers to the record index, not the row index)
                SYNCFUSION_DG.SelectRows(recordIndex, recordIndex);
                SYNCFUSION_DG.SelectedIndex = recordIndex;

                // Move the current cell to the specified cell
                SYNCFUSION_DG.MoveCurrentCell(rowColumnIndex); //SYNCFUSION_DG.MoveCurrentCell(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(lastRowIndex, 0));

                // Set focus to the grid
                SYNCFUSION_DG.Focus();
            }
            catch { }
        }

        #endregion

        #region Moshtary_Search
        // Thread-safe method to fetch and update ComboBox values, passing in the Visual (I_AM_PAYORRDER)
        public static void GetSearchedValueCustomer(ComboBox comboBox, string identifier, string baseKnow, CL_CCNNMANAGER dbms, Visual visualContext, bool isGataGridy = false)
        {
            string userInput = "";

            if (isGataGridy)
            {
                userInput = comboBox.Text;
            }
            else
            {
                TextBox? editableTextBox = null;
                if (comboBox.Template == null)
                {
                    editableTextBox = new();
                    editableTextBox.Text = comboBox.Text;
                }
                else
                {
                    // Safely retrieve the editable TextBox inside the ComboBox template
                    editableTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                    if ((editableTextBox == null || string.IsNullOrEmpty(editableTextBox.Text)))
                    {
                        return;
                    }
                }

                userInput = editableTextBox.Text.Trim();
            }

            if (string.IsNullOrEmpty(userInput)) return;

            if (string.IsNullOrEmpty(baseKnow))
            {
                Baseknow.BEDEHKAR.ToStringNullSafe();
            }

            // Handle the '+' or '++' case (open ComboSearch dialog)
            if (userInput == "+" || userInput == "++")
            {
                ShowSearchDialog(comboBox, identifier, visualContext);
            }
            // Handle numeric input
            else if (Information.IsNumeric(userInput))
            {
                ProcessNumericInput(comboBox, baseKnow, userInput, dbms);
            }
            // Handle non-numeric input
            else
            {
                ProcessNonNumericInput(comboBox, userInput, dbms);
            }
        }

        // Method to display the ComboSearch dialog and handle results, using Visual context (I_AM_PAYORRDER)
        private static void ShowSearchDialog(ComboBox comboBox, string identifier, Visual visualContext)
        {
            ComboSearch cmbSearch = new ComboSearch(identifier, visualContext);  // Use the visualContext here
            cmbSearch.ShowDialog();


            // Exit if no selection made
            if (comboBox?.SelectedValue == null)
            {
                ClearComboBoxSelection(comboBox);
            }

        }

        // Method to process numeric input
        private static void ProcessNumericInput(ComboBox comboBox, string baseKnow, string input, CL_CCNNMANAGER dbms)
        {
            try
            {
                string query = $"SELECT N_KOL, NUMBER, TNUMBER FROM TDETA_HES WHERE N_KOL = {baseKnow} AND NUMBER = 1 AND TNUMBER = {input}";
                var result = dbms.DoGetDataSQL<SQL1_FACTOR>(query).ToList();

                if (result.Count == 1)
                {
                    string hesValue = $"{baseKnow}-1-{input}";
                    string dataName = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE hes = N'{hesValue}'").FirstOrDefault();

                    AddToComboBoxItems(comboBox, hesValue, dataName);
                    comboBox.SelectedValue = hesValue;
                }
                else
                {
                    ClearComboBoxSelection(comboBox);
                }
            }
            catch (Exception)
            {
                ClearComboBoxSelection(comboBox);
            }
        }

        // Method to process non-numeric input
        private static void ProcessNonNumericInput(ComboBox comboBox, string input, CL_CCNNMANAGER dbms)
        {
            try
            {
                string query = $"SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'{input}'";
                var data = dbms.DoGetDataSQL<CUST_HESAB>(query).FirstOrDefault();

                if (data != null && !string.IsNullOrEmpty(data.hes))
                {
                    AddToComboBoxItems(comboBox, data.hes, data.NAME);
                    comboBox.SelectedValue = data.hes;
                }
                else
                {
                    ClearComboBoxSelection(comboBox);
                }
            }
            catch (Exception)
            {
                ClearComboBoxSelection(comboBox);
            }
        }

        // Helper method to add items to ComboBox safely
        private static void AddToComboBoxItems(ComboBox comboBox, string hesValue, string? dataName)
        {
            if (comboBox.ItemsSource == null)
            {
                comboBox.ItemsSource = new List<Custom_CUST_HESAB>();
            }

            if (comboBox.ItemsSource is List<Custom_CUST_HESAB> itemSource && !itemSource.Any(item => item?.hes == hesValue))
            {
                itemSource.Add(new Custom_CUST_HESAB { hes = hesValue, NAME = dataName });
                comboBox.Items.Refresh();
            }
        }
        // Clear the selection of the ComboBox in a safe manner
        public static void ClearComboBoxSelection(ComboBox comboBox)
        {
            comboBox.SelectedValue = null;
            comboBox.Text = string.Empty;
            comboBox.Items.Refresh();
        }
        #endregion

        public static string? GET_NAME_HES(string _HES_, CL_CCNNMANAGER dbms)
        {
            const string HesWrongFormat = "Account code format not recognized";
            const string HesNotFound = "Account Not Found";

            if (string.IsNullOrEmpty(_HES_) || string.IsNullOrWhiteSpace(_HES_))
            {
                return null;
            }

            if (string.IsNullOrEmpty(NumberExtractor.ExtractNumbersLine(_HES_))) //Ensure there is a number to avoid poitnless query
            {
                return null;
            }

            string _NAMEHES_ = dbms.DoGetDataSQL<string?>($"EXEC dbo.GET_NAME_HES @code=N'{_HES_}'").FirstOrDefault();

            if (!string.IsNullOrEmpty(_NAMEHES_) && _NAMEHES_ != HesNotFound && _NAMEHES_ != HesWrongFormat)
            {
                return _NAMEHES_;
            }

            return null;
        }
        public static bool IsCurrentAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        #region ArabicPersianStringComparer
        private const char ArabicYeh = '\u064A'; // ي
        private const char PersianYeh = '\u06CC'; // ی
        private const char ArabicAlefMaksura = '\u0649'; // ى
        private const char ArabicKaf = '\u0643'; // ك
        private const char PersianKaf = '\u06A9'; // ک
        private const char ArabicTehMarbuta = '\u0629'; // ة
        private const char PersianHeh = '\u0647'; // ه
        private const char Tatweel = '\u0640'; // ـ (کشیده)
        private static readonly Regex CombiningMarks = new(@"\p{Mn}", RegexOptions.Compiled);
        private static readonly Regex MultiSpaces = new(@"\s{2,}", RegexOptions.Compiled);

        public static string NormalizeForMatching(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string s = input.Trim();
            s = s
                .Replace('\u064A', '\u06CC') // ي -> ی
                .Replace('\u0649', '\u06CC') // ى -> ی
                .Replace('\u0643', '\u06A9') // ك -> ک
                .Replace('\u0629', '\u0647') // ة -> ه
                .Replace("\u0640", "");      // حذف Tatweel

            s = s.Replace('\u00A0', ' ').Replace('\u202F', ' ').Replace('\u2007', ' ');
            s = MultiSpaces.Replace(s, " ");

            return s;
        }
        public static string NormalizeArabicPersian(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string s = input.Trim().Normalize(NormalizationForm.FormKC);

            s = s
                .Replace('\u064A', '\u06CC') // ي -> ی
                .Replace('\u0649', '\u06CC') // ى -> ی
                .Replace('\u0643', '\u06A9') // ك -> ک
                .Replace('\u0629', '\u0647') // ة -> ه
                .Replace('\u0622', '\u0627') // آ -> ا
                .Replace('\u0623', '\u0627') // أ -> ا
                .Replace('\u0625', '\u0627') // إ -> ا
                .Replace('\u0671', '\u0627') // ٱ -> ا
                .Replace("\u0640", "");      // Tatweel

            // فاصله‌های ناسازگار
            s = s.Replace('\u00A0', ' ').Replace('\u202F', ' ').Replace('\u2007', ' ');
            s = MultiSpaces.Replace(s, " ");

            // حذف اعراب ترکیبی
            s = s.Normalize(NormalizationForm.FormD);
            s = CombiningMarks.Replace(s, string.Empty);
            s = s.Normalize(NormalizationForm.FormC);

            return s;
        }


        public static bool CompareArabicPersianStrings(string input1, string input2, StringComparison comparisonType = StringComparison.Ordinal)
        {
            // Normalize both inputs
            var normalizedInput1 = NormalizeArabicPersian(input1);
            var normalizedInput2 = NormalizeArabicPersian(input2);

            return string.Equals(normalizedInput1, normalizedInput2, comparisonType);
        }
        public static List<string> NormalizeArabicPersian(this List<string> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                return inputs;

            var results = new List<string>(inputs.Count);

            foreach (var input in inputs)
            {
                if (string.IsNullOrEmpty(input))
                {
                    results.Add(input);
                    continue;
                }

                var normalized = input
                    .Replace(ArabicYeh, PersianYeh)
                    .Replace(ArabicKaf, PersianKaf)
                    .Replace(ArabicTehMarbuta, PersianHeh)
                    .Trim();

                // Additional normalization steps
                normalized = normalized.Normalize(NormalizationForm.FormD);
                normalized = Regex.Replace(normalized, @"\p{Mn}", "");

                results.Add(normalized);
            }

            return results;
        }
        public static string NormalizeDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (ch >= '\u06F0' && ch <= '\u06F9') sb.Append((char)('0' + (ch - '\u06F0')));   // Persian
                else if (ch >= '\u0660' && ch <= '\u0669') sb.Append((char)('0' + (ch - '\u0660'))); // Arabic
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string ReplacePerArab(string input, bool IsArabicy)
        {
            if (IsArabicy)
            {
                return input.Replace("ی", "ي").Replace("ک", "ك").Replace("ة", "ه").Trim();
            }
            else
            {
                return input.Replace("ي", "ی").Replace("ك", "ک").Replace("ه", "ة").Trim();
            }
        }
        #endregion

        public static string? GetPersianMonthName(int MonthNumber)
        {
            switch (MonthNumber)
            {
                case 1: return "فروردین";
                case 2: return "اردیبهشت";
                case 3: return "خرداد";
                case 4: return "تیر";
                case 5: return "مرداد";
                case 6: return "شهریور";
                case 7: return "مهر";
                case 8: return "آبان";
                case 9: return "آذر";
                case 10: return "دی";
                case 11: return "بهمن";
                case 12: return "اسفند";

                default: break;
            }
            return null;
        }

        public static int? TOP_VAHED_K(CL_CCNNMANAGER dbms, string _CODEKALA_)
        {
            return dbms.DoGetDataSQL<int?>($"SELECT TOP 1 VAHED FROM dbo.STUF_DEF WHERE CODE = N'{_CODEKALA_}'").FirstOrDefault();
        }

        #region CustomerGetSearchWays
        public static Custom_CUST_HESAB? GetHesabBySearch(object MyElement, CL_CCNNMANAGER dbms, string? CUTSNO_TEX = null)
        {
            ComboBox? CUST_NO = null; //Source Element

            if (MyElement is ComboBox)
            {
                CUST_NO = MyElement as ComboBox;

                TextBox CMBTXB = (TextBox)CUST_NO.Template.FindName("PART_EditableTextBox", CUST_NO);
                if (!string.IsNullOrEmpty(CMBTXB?.Text))
                {
                    CUTSNO_TEX = CMBTXB?.Text;
                }
            }

            if (CUTSNO_TEX == "+" || CUTSNO_TEX == "++")
            {
                ComboSearch CMBSearch = new ComboSearch("", default);
                CMBSearch.ShowDialog();
                if (CMBSearch.SELECTED_HESAB != null)
                {
                    if (CUST_NO != null)
                    {
                        var _data_hes = CMBSearch.SELECTED_HESAB.hes;
                        var _data_name = CMBSearch.SELECTED_HESAB.NAME;
                        var THEROW = new Custom_CUST_HESAB { hes = _data_hes, NAME = _data_name };

                        // Handle the ItemsSource safely regardless of its type
                        HandleComboBoxItemsSource(CUST_NO, THEROW);

                        CUST_NO.SelectedValue = null;
                        CUST_NO.SelectedValue = CMBSearch.SELECTED_HESAB.hes;
                        CUST_NO.Items.Refresh();
                    }

                    return CMBSearch.SELECTED_HESAB;
                }
            }
            else if (Information.IsNumeric(CUTSNO_TEX))
            {
                try
                {
                    var rst = dbms.DoGetDataSQL<SQL1_FACTOR>("SELECT N_KOL , NUMBER,TNUMBER FROM TDETA_HES WHERE N_KOL = " + Baseknow.BEDEHKAR + " and NUMBER = 1 and tNUMBER = " + CUTSNO_TEX).ToList();
                    if (rst.Count == 1)
                    {
                        var _data_hes = rst.FirstOrDefault()?.n_kol + "-" + rst.FirstOrDefault()?.NUMBER + "-" + rst.FirstOrDefault()?.tNUMBER;

                        var _data_name = dbms.DoGetDataSQL<string>($"SELECT TOP 1 NAME FROM CUST_HESAB WHERE hes = N'{_data_hes}'").FirstOrDefault();
                        var THEROW = new Custom_CUST_HESAB { hes = _data_hes, NAME = _data_name };

                        if (CUST_NO != null)
                        {
                            // Handle the ItemsSource safely regardless of its type
                            HandleComboBoxItemsSource(CUST_NO, THEROW);

                            CUST_NO.SelectedValue = null;
                            CUST_NO.SelectedValue = _data_hes;
                            CUST_NO.Items.Refresh();
                        }

                        return THEROW;
                    }
                    else
                    {
                        if (CUST_NO != null)
                        {
                            CUST_NO.SelectedValue = null;
                            CUST_NO.Text = null;
                            CUST_NO.Items.Refresh();
                        }

                    }
                }
                catch (Exception) { }
            }
            else
            {
                var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + CUTSNO_TEX + "'").FirstOrDefault();
                if (data is not null && !string.IsNullOrEmpty(data.hes))
                {
                    string thevalue = data.hes;
                    var THEROW = new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME };

                    if (CUST_NO != null)
                    {
                        // Handle the ItemsSource safely regardless of its type
                        HandleComboBoxItemsSource(CUST_NO, THEROW);

                        CUST_NO.SelectedValue = null;
                        CUST_NO.SelectedValue = thevalue;
                        CUST_NO.Items.Refresh();
                    }

                    return THEROW;
                }
                else
                {
                    if (CUST_NO != null)
                    {
                        CUST_NO.SelectedValue = null;
                        CUST_NO.Text = null;
                        CUST_NO.Items.Refresh();
                    }
                }
            }

            return null;
        }

        // New helper method to handle different ItemsSource types
        private static void HandleComboBoxItemsSource(ComboBox? comboBox, Custom_CUST_HESAB newItem)
        {
            if (comboBox == null)
            {
                return;
            }

            if (comboBox?.ItemsSource == null)
            {
                comboBox.ItemsSource = new List<Custom_CUST_HESAB> { newItem };
                return;
            }

            // Check what type of list we're working with
            if (comboBox.ItemsSource is List<Custom_CUST_HESAB> customList)
            {
                // Original case - direct cast works
                if (!customList.Any(item => item?.hes == newItem.hes))
                {
                    customList.Add(newItem);
                }
            }
            else if (comboBox.ItemsSource is List<CUST_HESAB_COMBINED> combinedList)
            {
                // Handle CUST_HESAB_COMBINED case
                if (!combinedList.Any(item => item?.hes == newItem.hes))
                {
                    // Create a new item of the appropriate type and add it
                    // Adjust these property mappings as needed based on your actual class structure
                    var combinedItem = new CUST_HESAB_COMBINED
                    {
                        hes = newItem.hes,
                        NAME = newItem.NAME
                        // Map other needed properties here
                    };
                    combinedList.Add(combinedItem);
                }
            }
            else
            {
                // Fallback for any other type: create a new compatible list
                var itemType = comboBox.ItemsSource.GetType().GetGenericArguments()[0];

                // Try to create a new item of the appropriate type using reflection
                try
                {
                    var newCompatibleItem = Activator.CreateInstance(itemType);

                    // Set common properties using reflection
                    var hesProperty = itemType.GetProperty("hes");
                    var nameProperty = itemType.GetProperty("NAME");

                    if (hesProperty != null) hesProperty.SetValue(newCompatibleItem, newItem.hes);
                    if (nameProperty != null) nameProperty.SetValue(newCompatibleItem, newItem.NAME);

                    // Get the Add method on the list
                    var addMethod = comboBox.ItemsSource.GetType().GetMethod("Add");
                    if (addMethod != null)
                    {
                        // Check if an item with the same hes already exists
                        var existingItem = false;
                        foreach (var item in (IEnumerable)comboBox.ItemsSource)
                        {
                            var itemHesProperty = item.GetType().GetProperty("hes");
                            var itemHes = itemHesProperty?.GetValue(item) as string;
                            if (itemHes == newItem.hes)
                            {
                                existingItem = true;
                                break;
                            }
                        }

                        if (!existingItem)
                        {
                            // Add the new item
                            addMethod.Invoke(comboBox.ItemsSource, new[] { newCompatibleItem });
                        }
                    }
                }
                catch
                {
                    // If all else fails, replace the ItemsSource with a new compatible list
                    comboBox.ItemsSource = new List<Custom_CUST_HESAB> { newItem };
                }
            }
        }
        #endregion

        // Precompile the regex for performance if used frequently
        private static readonly Regex MobileRegex = new Regex(@"^09\d{9}$", RegexOptions.Compiled);
        public static bool IsValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return false;

            mobileNumber = mobileNumber.Trim();

            // Validate against the regex: starts with 09 and followed by exactly 9 digits
            return MobileRegex.IsMatch(mobileNumber);
        }
        public static bool IsValidIranMobileFuzzy(string Phone)
        {
            if (string.IsNullOrEmpty(Phone))
                return false;
            var r = new Regex(@"^(?:0|98|\+98|\+980|0098|098|00980)?(9\d{9})$");
            return r.IsMatch(Phone);
        }

        public static (bool, string) DATE_IS_VALID(string _DATEVALUE_, string FiledName = "تاریخ")
        {
            bool Date_Is_Valid = true;
            string? RESULT = null;
            string date_n_val = _DATEVALUE_.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    RESULT = $"مقدار {FiledName} صحیح نیست";
                    Date_Is_Valid = false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        RESULT = $".{FiledName} مربوط به سال جاری نیست";
                        Date_Is_Valid = false;
                    }
                }
            }
            else
            {
                RESULT = $"{FiledName} نمی تواند خالی باشد.";
                Date_Is_Valid = false;
            }
            return (Date_Is_Valid, RESULT);
        }

        #region KalaSearch
        public static INVO_LST_FACTOR22? GetKalaBySearch(CL_CCNNMANAGER dbms, string ANBAR, string? KALA_TEXT = null)
        {
            //  double MEGHk = Convert.ToDouble(item.GetType().GetProperty("MEGHk").GetValue(item));
            if (KALA_TEXT == "+" || KALA_TEXT == "++")
            {
                SERCHK CMBSearch = new SERCHK(default, ANBAR);
                CMBSearch.ShowDialog();
                if (CMBSearch.SELECTED_KALA != null)
                {
                    return CMBSearch.SELECTED_KALA;
                }
            }
            else if (Information.IsNumeric(KALA_TEXT))
            {
                try
                {
                    string AnbarCondition = string.IsNullOrEmpty(ANBAR) ? "" : $" AND (dbo.STUF_FSK.ANBAR = {ANBAR}) ";

                    //اگر عدد وارد کرده برم سرغ کد کالا
                    var FoundKala = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME AS NAME_CODE FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE (dbo.STUF_DEF.CODE = N'{KALA_TEXT}') {AnbarCondition} ").FirstOrDefault();
                    if (!ReferenceEquals(FoundKala, null))
                    {
                        return FoundKala;
                    }
                    else
                    {
                        //شماره فنی
                        var rstfani = dbms.DoGetDataSQL<INVO_LST_FACTOR22>($"SELECT TOP (1) dbo.STUF_FSK.CODE, dbo.STUF_DEF.NAME NAME_CODE FROM dbo.STUF_DEF INNER JOIN dbo.STUF_FSK ON dbo.STUF_DEF.CODE = dbo.STUF_FSK.CODE WHERE  dbo.STUF_DEF.CODE = N''+(SELECT TOP 1 CODE FROM STUF_DEF WHERE dbo.STUF_DEF.CODE = N'' +(SELECT TOP 1 CODE FROM STUF_DEF WHERE N_FANI = N'{KALA_TEXT}')+'') {AnbarCondition}").FirstOrDefault();
                        if (rstfani != null)
                        {
                            return rstfani;
                        }
                    }
                }
                catch (Exception) { }
            }
            else
            {
                var THEFULLCODE = CL_KALA_SEARCH.Go_Search_Kala(KALA_TEXT, ANBAR, default);
                if (THEFULLCODE != null)
                {
                    return THEFULLCODE;
                }
            }

            return null;
        }
        #endregion

        public static bool ValidateImageFile(string filePath, out string errorMessage, double maxSizeInMB = 10)
        {
            errorMessage = string.Empty;

            // بررسی وجود فایل
            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "هیچ فایلی انتخاب نشده است.";
                return false;
            }

            if (!File.Exists(filePath))
            {
                errorMessage = "فایل انتخاب شده وجود ندارد.";
                return false;
            }

            // بررسی پسوند فایل برای اطمینان از تصویر بودن
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };

            if (!validImageExtensions.Contains(extension))
            {
                errorMessage = "فایل انتخاب شده در فرمت تصویر پشتیبانی شده نیست. لطفا یک فایل تصویر انتخاب کنید.";
                return false;
            }

            try
            {
                // بررسی اندازه فایل
                var fileInfo = new FileInfo(filePath);
                double fileSizeInMB = fileInfo.Length / (1024.0 * 1024.0); //Byte

                if (fileSizeInMB > maxSizeInMB)
                {
                    errorMessage = $"تصویر انتخاب شده بیشتر از حداکثر اندازه مجاز {maxSizeInMB} مگابایت است. اندازه فعلی: {fileSizeInMB:F2} مگابایت.";
                    return false;
                }

                // بررسی اختیاری اینکه فایل واقعاً یک تصویر است با تلاش برای بارگذاری آن
                // این امنیت اضافی در برابر فایل‌هایی که به نام پسوندهای تصویر تغییر نام داده‌اند، فراهم می‌کند
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        // ایجاد BitmapImage با عملکرد کاهش یافته برای جلوگیری از بارگذاری کامل
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.DecodePixelWidth = 1; // رمزگشایی حداقل برای تأیید اینکه تصویر است
                        img.StreamSource = stream;
                        img.EndInit();
                        // اگر به اینجا برسیم، یک تصویر معتبر است
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "فایل انتخاب شده یک فایل تصویر معتبر نیست.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"خطا در اعتبارسنجی فایل تصویر";
                return false;
            }
        }


        private static void Traverse(DependencyObject obj)
        {
            if (obj == null)
                return;

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is UIElement element)
                {
                    // Remove DropShadowEffect / BlurEffect
                    if (element.Effect is DropShadowEffect or BlurEffect)
                    {
                        element.Effect = null;
                    }

                    // Remove layout transforms (optional)
                    if (element is FrameworkElement fe && fe.LayoutTransform != null && fe.LayoutTransform != Transform.Identity)
                    {
                        fe.LayoutTransform = Transform.Identity;
                    }

                    // Opacity fix (optional)
                    if (element.Opacity < 1.0)
                    {
                        element.Opacity = 1.0;
                    }
                }

                Traverse(child);
            }
        }
        public static void OptimizeForRemoteDesktop(DependencyObject obj)
        {
            if (GeneralOptionManager.IsRDPMode) //System.Windows.Forms.SystemInformation.TerminalServerSession;
            {
                // Set rendering mode to software to reduce GPU usage
                if (obj is Visual visual)
                {
                    HwndSource hwndSource = PresentationSource.FromVisual(visual) as HwndSource;
                    if (hwndSource != null)
                    {
                        hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
                    }
                }

                // Optimize bitmap scaling for speed over quality
                RenderOptions.SetBitmapScalingMode(obj, BitmapScalingMode.LowQuality);

                // Reduce anti-aliasing overhead
                RenderOptions.SetEdgeMode(obj, EdgeMode.Aliased);

                // Use a faster text rendering mode
                TextOptions.SetTextFormattingMode(obj, TextFormattingMode.Display);

                // Apply to all ScrollViewer instances
                ScrollViewer.CanContentScrollProperty.OverrideMetadata(
                    typeof(ScrollViewer), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits,
                        (d, e) =>
                        {
                            if (d is ScrollViewer scrollViewer)
                            {
                                scrollViewer.IsDeferredScrollingEnabled = true;
                                scrollViewer.CanContentScroll = true;
                                scrollViewer.PanningMode = PanningMode.VerticalOnly;
                            }
                        }));

                if (obj is Window THEWIN)
                {
                    // Disable effects and transparency
                    THEWIN.AllowsTransparency = false;
                    THEWIN.Effect = null;
                }

                Traverse(obj);
            }
        }
    }
}
