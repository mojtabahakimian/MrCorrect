using Prg_Proccessy.FUNCTIONS;
using Prg_UI.Functions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.CUC
{
    public partial class DateNavigator : UserControl
    {
        public event EventHandler<string> DateChanged;

        public static readonly DependencyProperty ShowDayNameProperty =
            DependencyProperty.Register("ShowDayName", typeof(bool), typeof(DateNavigator), new PropertyMetadata(false, OnShowDayNameChanged));

        public bool ShowDayName
        {
            get { return (bool)GetValue(ShowDayNameProperty); }
            set { SetValue(ShowDayNameProperty, value); }
        }

        private static void OnShowDayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DateNavigator;
            if (control != null)
            {
                control.txtDayName.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
                if ((bool)e.NewValue)
                {
                    control.UpdateDayName();
                }
            }
        }

        public DateNavigator()
        {
            InitializeComponent();
        }

        public string CurrentDate
        {
            get { return txtDate.Text; }
            set
            {
                txtDate.Text = value;
                UpdateDayName();
            }
        }

        private void RaiseDateChanged()
        {
            UpdateDayName();
            DateChanged?.Invoke(this, txtDate.Text);
        }

        private void Btn_SyncDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtDate.Text = Tarikh.SlashyFullDate;
                RaiseDateChanged();
            }
            catch { }
        }

        private void UpdateDayName()
        {
            if (ShowDayName && !string.IsNullOrEmpty(txtDate.Text))
            {
                try
                {
                    var dt = Tarikh.GetRawGregorianDateTime(txtDate.Text.ToRawTarikh());
                    txtDayName.Text = Tarikh.GetDayName(dt);
                }
                catch
                {
                    txtDayName.Text = "";
                }
            }
        }

        private void BtnNextDay_Click(object sender, RoutedEventArgs e)
        {
            AddDays(1);
        }

        private void BtnPrevDay_Click(object sender, RoutedEventArgs e)
        {
            AddDays(-1);
        }

        private void BtnNextWeek_Click(object sender, RoutedEventArgs e)
        {
            AddDays(7);
        }

        private void BtnPrevWeek_Click(object sender, RoutedEventArgs e)
        {
            AddDays(-7);
        }

        private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            AddMonth(1);
        }

        private void BtnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            AddMonth(-1);
        }

        private void AddDays(int days)
        {
            try
            {
                var dt = Tarikh.GetRawGregorianDateTime(txtDate.Text.ToRawTarikh());
                var newDate = dt.AddDays(days);
                var pc = new PersianCalendar();
                txtDate.Text = string.Format("{0:0000}/{1:00}/{2:00}", pc.GetYear(newDate), pc.GetMonth(newDate), pc.GetDayOfMonth(newDate));
                RaiseDateChanged();
            }
            catch { }
        }

        private void AddMonth(int months)
        {
            try
            {
                var dt = Tarikh.GetRawGregorianDateTime(txtDate.Text.ToRawTarikh());
                var pc = new PersianCalendar();
                var newDate = pc.AddMonths(dt, months);
                txtDate.Text = string.Format("{0:0000}/{1:00}/{2:00}", pc.GetYear(newDate), pc.GetMonth(newDate), pc.GetDayOfMonth(newDate));
                RaiseDateChanged();
            }
            catch { }
        }
        private void txtDate_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // اگر متنی که الان هست با متنی که موقع ورود بود فرق دارد، یعنی تغییر کرده است
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
            {
                if (txtDate.Text != _originalDateText)
                {
                    RaiseDateChanged();
                    // مقدار جدید را به عنوان مقدار اصلی ست می‌کنیم
                    _originalDateText = txtDate.Text;
                }
            }
        }
        private string _originalDateText;
        private void txtDate_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _originalDateText = txtDate.Text;
        }
    }
}
