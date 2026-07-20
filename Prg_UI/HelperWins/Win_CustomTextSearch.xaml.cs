using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.HelperWins
{
    public partial class Win_CustomTextSearch : Window
    {
        public string ResultText { get; private set; } = string.Empty;
        public bool IsExclusion { get; private set; } = false;
        public List<string> SelectedColumns { get; private set; } = new List<string>();
        public bool IsConfirmed { get; private set; } = false;

        public Win_CustomTextSearch(IEnumerable<(string MappingName, string HeaderText)> columns, string defaultSelectedColumn = null, string defaultText = "")
        {
            InitializeComponent();

            foreach (var col in columns ?? Enumerable.Empty<(string MappingName, string HeaderText)>())
            {
                var chk = new CheckBox
                {
                    Content = col.HeaderText,
                    Tag = col.MappingName,
                    Margin = new Thickness(0, 0, 14, 6),
                    IsChecked = !string.IsNullOrEmpty(defaultSelectedColumn) &&
                                string.Equals(col.MappingName, defaultSelectedColumn, StringComparison.OrdinalIgnoreCase)
                };
                Panel_Columns.Children.Add(chk);
            }

            Txt_SearchInput.Text = defaultText ?? string.Empty;

            Loaded += (s, e) =>
            {
                Txt_SearchInput.Focus();
                Txt_SearchInput.SelectAll();
            };
        }

        /// <summary>
        /// نمایش دیالوگ و بازگرداندن متن، ستون‌های انتخاب‌شده و حالت فیلتر منفی.
        /// اگر کاربر انصراف داد، null برمی‌گرداند.
        /// </summary>
        public static (string SearchText, List<string> Columns, bool IsExclusion)? Show(
            Window owner,
            IEnumerable<(string MappingName, string HeaderText)> columns,
            string defaultSelectedColumn = null,
            string defaultText = "")
        {
            var win = new Win_CustomTextSearch(columns, defaultSelectedColumn, defaultText)
            {
                Owner = owner
            };

            bool? result = win.ShowDialog();
            if (result != true) return null;

            return (win.ResultText, win.SelectedColumns, win.IsExclusion);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            var text = Txt_SearchInput.Text?.Trim() ?? string.Empty;

            var selected = Panel_Columns.Children.OfType<CheckBox>()
                .Where(c => c.IsChecked == true)
                .Select(c => c.Tag as string)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowValidationError("لطفاً متن جستجو را وارد کنید.");
                Txt_SearchInput.Focus();
                return;
            }

            if (selected.Count == 0)
            {
                ShowValidationError("لطفاً حداقل یک ستون را برای جستجو انتخاب کنید.");
                return;
            }

            ResultText = text;
            SelectedColumns = selected;
            IsExclusion = Chk_Exclude.IsChecked == true;
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void ShowValidationError(string message)
        {
            Txt_ValidationError.Text = message;
            Txt_ValidationError.Visibility = Visibility.Visible;
        }

        private void Txt_SearchInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btn_Ok_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Btn_Cancel_Click(sender, e);
                e.Handled = true;
            }
        }

        private void RootWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Btn_Cancel_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}