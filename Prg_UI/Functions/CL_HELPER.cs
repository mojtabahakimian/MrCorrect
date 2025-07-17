using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Diagnostics;

namespace Functions
{
    public static class CL_HELPER
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;

                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        /// <summary>
        /// Finds all visual children of specified type in the visual tree
        /// </summary>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent, string specificTag = null) where T : DependencyObject
        {
            if (parent == null)
            {
                LogError("Null parent object provided to FindVisualChildren");
                yield break;
            }

            var searchQueue = new Queue<(DependencyObject element, int depth)>();
            searchQueue.Enqueue((parent, 0));
            const int MaxSearchDepth = 100;

            while (searchQueue.Count > 0)
            {
                var (current, depth) = searchQueue.Dequeue();

                if (depth > MaxSearchDepth)
                {
                    LogWarning($"Maximum search depth ({MaxSearchDepth}) reached in visual tree");
                    continue;
                }

                int childCount;
                try
                {
                    childCount = VisualTreeHelper.GetChildrenCount(current);
                }
                catch (Exception ex)
                {
                    LogError($"Error getting child count: {ex.Message}");
                    continue;
                }

                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child;
                    try
                    {
                        child = VisualTreeHelper.GetChild(current, i);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error getting child at index {i}: {ex.Message}");
                        continue;
                    }

                    // Handle content controls
                    if (child is ContentControl contentControl)
                    {
                        var content = contentControl.Content as DependencyObject;
                        if (content != null)
                        {
                            searchQueue.Enqueue((content, depth + 1));
                        }
                    }

                    // Handle items controls
                    if (child is ItemsControl itemsControl)
                    {
                        foreach (var item in itemsControl.Items)
                        {
                            if (item is DependencyObject itemVisual)
                            {
                                searchQueue.Enqueue((itemVisual, depth + 1));
                            }
                        }
                    }

                    // Check if current child matches the requested type
                    if (child is T typedChild)
                    {
                        if (specificTag != null)
                        {
                            if (typedChild is Button button &&
                                button.Tag != null &&
                                string.Equals(button.Tag.ToString(), specificTag, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return typedChild;
                            }
                        }
                        else
                        {
                            yield return typedChild;
                        }
                    }

                    searchQueue.Enqueue((child, depth + 1));
                }
            }
        }

        // Attached DependencyProperty for original enabled state
        public static readonly DependencyProperty OriginalEnabledStateProperty =
            DependencyProperty.RegisterAttached(
                "OriginalEnabledState",
                typeof(bool),
                typeof(CL_HELPER),
                new PropertyMetadata(true));

        public static void SetOriginalEnabledState(DependencyObject element, bool value)
        {
            element.SetValue(OriginalEnabledStateProperty, value);
        }

        public static bool GetOriginalEnabledState(DependencyObject element)
        {
            return (bool)element.GetValue(OriginalEnabledStateProperty);
        }

        /// <summary>
        /// Sets security for delete buttons
        /// </summary>
        public static void SetDeleteButtonSecurity(Window window, string formName, bool isDeleteAllowed)
        {
            if (window == null)
            {
                LogError("Window parameter is null in SetDeleteButtonSecurity");
                return;
            }

            try
            {
                var deleteButtons = FindVisualChildren<Button>(window, "hazfer").ToList();

                if (!deleteButtons.Any())
                {
                    LogWarning($"No delete buttons found in window {window.Name}");
                    return;
                }

                foreach (var button in deleteButtons)
                {
                    try
                    {
                        // Store original state if not already stored
                        if (!button.GetValue(OriginalEnabledStateProperty).Equals(button.IsEnabled))
                        {
                            SetOriginalEnabledState(button, button.IsEnabled);
                        }

                        if (!isDeleteAllowed)
                        {
                            button.IsEnabled = false;
                            button.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            button.IsEnabled = true;
                            button.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void LogError(string message)
        {
            Debug.WriteLine($"[ERROR] {DateTime.Now}: {message}");
            // Add your custom logging implementation here
        }

        private static void LogWarning(string message)
        {
            Debug.WriteLine($"[WARNING] {DateTime.Now}: {message}");
            // Add your custom logging implementation here
        }
    }
}