using Dapper;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_SendInvoice.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Prg_UI.Wins.WinMenus.CONFIGS
{
    public partial class WIN_OPTIONS : Window
    {
        public WIN_OPTIONS()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
        public bool ChangeIsHappend { get; private set; }

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;
            }
        }

        #region LOCALMODEL
        public class ComboBoxItemData
        {
            public string Value { get; set; } // مقداری که ذخیره می‌شود (مثلا "00")
            public string Display { get; set; } // متنی که نمایش داده می‌شود
        }
        #endregion

        private const int TOOO = 68; // تعداد کل تنظیمات
        private SAZMAN _currentSettings; // برای نگهداری ID اصلی رکورد سازمان

        string WAS_C2728 = "12"; //اندازه فونت در فاكتور فروش

        string WAS_C4041 = "01"; //زمان

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            LoadSettings();

            WAS_C2728 = C2728.Text; //اندازه فونت در فاكتور فروش
            WAS_C4041 = C4041.Text; //زمان
        }

        private void FILL_ALL_COMBOBOXES()
        {
            var itemsList = new List<ComboBoxItemData>
            {
                new ComboBoxItemData { Value =  "00" , Display = "كارت ساعت جهانگستر"  },
                new ComboBoxItemData { Value =  "01" , Display = "کارت ساعت"  },
                new ComboBoxItemData { Value =  "02" , Display = "PW"  },
                new ComboBoxItemData { Value =  "03" , Display = "كارت ساعت آراز"  },
            };
            C2425.ItemsSource = itemsList;
            C2425.DisplayMemberPath = "Display";
            C2425.SelectedValuePath = "Value";
        }
        private void LoadSettings()
        {
            // ابتدا رکورد تنظیمات را برای گرفتن رشته OPTIONSS و ID اصلی می‌خوانیم
            string sql = "SELECT TOP 1 UNIVERSITY_CO, OPTIONSS FROM SAZMAN";
            _currentSettings = dbms.DoGetDataSQL<SAZMAN>(sql).FirstOrDefault();

            if (_currentSettings == null)
            {
                return;
            }

            string tnz = _currentSettings.OPTIONSS ?? "";

            // رشته را با '0' پد می‌کنیم تا طول آن حداقل به اندازه تعداد تنظیمات باشد
            tnz = tnz.PadRight(TOOO, '0');

            for (int i = 1; i <= TOOO; i++)
            {
                // پیدا کردن کنترل چک‌باکس با نام داینامیک
                var checkBox = this.FindName("Check" + i) as CheckBox;
                if (checkBox != null)
                {
                    checkBox.IsChecked = (tnz[i - 1] == '5');
                }

                // مدیریت موارد خاص که مقدار TextBox/ComboBox هم در رشته ذخیره شده
                if (i == 10)
                {
                    if (this.FindName("C1112") is TextBox txt) txt.Text = tnz.Substring(i, 2);
                    i += 2; // پرش از روی ۲ کاراکتر خوانده شده
                }
                else if (i == 23)
                {
                    if (this.FindName("C2425") is ComboBox cmb) cmb.SelectedValue = tnz.Substring(i, 2);
                    i += 2;
                }
                else if (i == 26)
                {
                    if (this.FindName("C2728") is TextBox txt) txt.Text = tnz.Substring(i, 2);
                    i += 2;
                }
                else if (i == 34)
                {
                    if (this.FindName("C3536") is ComboBox cmb) cmb.SelectedValue = tnz.Substring(i, 2);
                    i += 2;
                }
                else if (i == 39)
                {
                    if (this.FindName("C4041") is TextBox txt) txt.Text = tnz.Substring(i, 2);
                    i += 2;
                }
            }

            // منطق آخر برای غیرفعال کردن Check13
            string countSql = "SELECT COUNT(NUMBER) FROM HEAD_LST WHERE TAG = 13";
            var count = dbms.DoGetDataSQL<int>(countSql).FirstOrDefault();
            if (count > 0)
            {
                if (this.FindName("Check13") is CheckBox chk13)
                {
                    chk13.IsEnabled = false;
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }

            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //Command148_Click

            var tnzBuilder = new StringBuilder();

            for (int i = 1; i <= TOOO; i++)
            {
                var checkBox = this.FindName("Check" + i) as CheckBox;
                tnzBuilder.Append(checkBox?.IsChecked == true ? "5" : "0");

                // مدیریت موارد خاص
                if (i == 10)
                {
                    if (this.FindName("C1112") is TextBox txt)
                        tnzBuilder.Append(int.Parse(txt.Text ?? "0").ToString("D2"));
                    i += 2;
                }
                else if (i == 23)
                {
                    if (this.FindName("C2425") is ComboBox cmb)
                        tnzBuilder.Append((cmb.SelectedValue?.ToString() ?? "00").PadLeft(2, '0'));
                    i += 2;
                }
                else if (i == 26)
                {
                    if (this.FindName("C2728") is TextBox txt)
                        tnzBuilder.Append(int.Parse(txt.Text ?? "0").ToString("D2"));
                    i += 2;
                }
                else if (i == 34)
                {
                    if (this.FindName("C3536") is ComboBox cmb)
                        tnzBuilder.Append((cmb.SelectedValue?.ToString() ?? "00").PadLeft(2, '0'));
                    i += 2;
                }
                else if (i == 39)
                {
                    if (this.FindName("C4041") is TextBox txt)
                        tnzBuilder.Append(int.Parse(txt.Text ?? "0").ToString("D2"));
                    i += 2;
                }
            }

            string finalTnz = tnzBuilder.ToString();

            // ذخیره رشته نهایی در دیتابیس
            string sql = "UPDATE SAZMAN SET OPTIONSS = @OptionsValue WHERE UNIVERSITY_CO = @Id";
            var parameters = new DynamicParameters();
            parameters.Add("@OptionsValue", finalTnz);
            parameters.Add("@Id", _currentSettings.UNIVERSITY_CO);

            try
            {
                dbms.DoExecuteSQL(sql, parameters);
                new Msgwin(false, "تنظیمات با موفقیت ذخیره شد").ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در ذخیره سازی").ShowDialog();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void C2728_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //اندازه فونت
            if (!CL_LMethods.IsNumeric(C2728.Text))
            {
                C2728.Text = WAS_C2728;
            }
            C2728.Text = C2728.Text.Trim();
        }
        private void C4041_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //زمان
            if (!CL_LMethods.IsNumeric(C4041.Text))
            {
                C4041.Text = WAS_C4041;
            }
            C4041.Text = C4041.Text.Trim();
        }

    }
}
