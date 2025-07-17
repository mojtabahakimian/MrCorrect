using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.CUC
{
    public static class ComboBoxEnterKeyBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ComboBoxEnterKeyBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if ((bool)e.NewValue)
                {
                    comboBox.PreviewKeyDown += ComboBox_PreviewKeyDown;
                    comboBox.PreviewLostKeyboardFocus += ComboBox_PreviewLostKeyboardFocus; ;
                    comboBox.Loaded += ComboBox_Loaded;
                }
                else
                {
                    comboBox.PreviewLostKeyboardFocus -= ComboBox_PreviewLostKeyboardFocus; ;
                    comboBox.PreviewKeyDown -= ComboBox_PreviewKeyDown;
                    comboBox.Loaded -= ComboBox_Loaded;
                }
            }
        }

        private static void ComboBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {

            if (sender is ComboBox comboBox)
            {
                if (comboBox.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

                var selectedItem = comboBox.SelectedItem;
                bool isMatch = false;
                if (selectedItem != null && !string.IsNullOrEmpty(comboBox.DisplayMemberPath))
                {
                    var propInfo = selectedItem.GetType().GetProperty(comboBox.DisplayMemberPath);
                    if (propInfo != null)
                    {
                        var displayValue = propInfo.GetValue(selectedItem)?.ToString();
                        isMatch = displayValue == comboBox.Text;
                    }
                }
                else
                {
                    // fallback: simple comparison
                    isMatch = comboBox.SelectedValue?.ToString() == comboBox.Text;
                }

                if (isMatch)
                {
                    return;
                }

                HandleEnterKey(comboBox);
                e.Handled = true;
            }
        }

        private static void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure the ComboBox is editable
            if (sender is ComboBox comboBox && !comboBox.IsEditable)
            {
                comboBox.IsEditable = true;
            }
        }

        private static void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is ComboBox comboBox)
            {
                //HandleEnterKey(comboBox);
                //e.Handled = true;
            }
        }

        private static void HandleEnterKey(ComboBox comboBox)
        {
            // Get the text entered in the ComboBox
            string enteredText = comboBox.Text?.Trim();

            if (string.IsNullOrEmpty(enteredText))
                return;

            // Try to parse the entered text as a number (you can modify this based on your needs)
            if (!IsNumeric(enteredText))
                return;

            // Search through items to find matching SelectedValue
            foreach (var item in comboBox.Items)
            {
                // Get the SelectedValue for this item
                var container = comboBox.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;

                // If using SelectedValuePath
                if (!string.IsNullOrEmpty(comboBox.SelectedValuePath))
                {
                    var valueProperty = item.GetType().GetProperty(comboBox.SelectedValuePath);
                    if (valueProperty != null)
                    {
                        var value = valueProperty.GetValue(item)?.ToString();
                        if (value == enteredText)
                        {
                            comboBox.SelectedItem = item;
                            return;
                        }
                    }
                }
                // If item is ComboBoxItem with Tag or Content
                else if (item is ComboBoxItem comboBoxItem)
                {
                    // Check Tag first
                    if (comboBoxItem.Tag?.ToString() == enteredText)
                    {
                        comboBox.SelectedItem = comboBoxItem;
                        return;
                    }
                    // Then check Content
                    else if (comboBoxItem.Content?.ToString() == enteredText)
                    {
                        comboBox.SelectedItem = comboBoxItem;
                        return;
                    }
                }
                // If items are simple objects, check ToString()
                else if (item.ToString() == enteredText)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private static bool IsNumeric(string text)
        {
            return double.TryParse(text, out _);
        }
    }

    /// <summary>
    /// Extension methods for easier usage in code-behind
    /// </summary>
    public static class ComboBoxExtensions
    {
        public static void EnableEnterKeySelection(this ComboBox comboBox)
        {
            ComboBoxEnterKeyBehavior.SetIsEnabled(comboBox, true);
        }

        public static void DisableEnterKeySelection(this ComboBox comboBox)
        {
            ComboBoxEnterKeyBehavior.SetIsEnabled(comboBox, false);
        }
    }
}
