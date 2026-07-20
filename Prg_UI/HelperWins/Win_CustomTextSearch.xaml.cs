using System;
using System.Windows;
using System.Windows.Input;

namespace Prg_UI.HelperWins
{
    public partial class Win_CustomTextSearch : Window
    {
        public string ResultText { get; private set; } = string.Empty;
        public bool IsConfirmed { get; private set; } = false;

        public Win_CustomTextSearch(string columnHeader, string defaultText = "")
        {
            InitializeComponent();

            Txt_ColumnLabel.Text = $"متن مورد نظر برای جستجو در ستون «{columnHeader}» را وارد کنید:";
            Txt_SearchInput.Text = defaultText ?? string.Empty;

            Loaded += (s, e) =>
            {
                Txt_SearchInput.Focus();
                Txt_SearchInput.SelectAll();
            };
        }

        /// <summary>
        /// نمایش دیالوگ و بازگرداندن متن وارد شده توسط کاربر. اگر کاربر انصراف داد، null برمی‌گرداند.
        /// </summary>
        public static string Show(Window owner, string columnHeader, string defaultText = "")
        {
            var win = new Win_CustomTextSearch(columnHeader, defaultText)
            {
                Owner = owner
            };

            bool? result = win.ShowDialog();
            return (result == true) ? win.ResultText : null;
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
            ResultText = Txt_SearchInput.Text?.Trim() ?? string.Empty;
            IsConfirmed = !string.IsNullOrWhiteSpace(ResultText);
            DialogResult = IsConfirmed;
            Close();
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