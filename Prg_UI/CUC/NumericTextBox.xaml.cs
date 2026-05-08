using Functions;
using Prg_UI.Functions;
using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Prg_UI.CUC
{
    public enum NumericInputMask
    {
        None,
        DateYYYYMMDD
    }

    public partial class NumericTextBox : UserControl
    {
        private double? LastValidValue = null;

        // فقط رقم‌های واقعی تاریخ، بدون اسلش: 14050215
        private string _rawDateDigits = string.Empty;

        // برای جلوگیری از برگشت‌های ناخواسته بین Binding و TextChanged
        private bool _isInternalTextChange = false;

        private const int PersianDateDigitLength = 8;

        public NumericTextBox()
        {
            InitializeComponent();

            if (TextBoxStyle == null)
            {
                Style? defaultStyle = null;

                var fuzzyStyle = (Style)FindResource("FuzzyOut");

                if (fuzzyStyle != null)
                {
                    TextBoxStyle = fuzzyStyle;
                }
                else
                {
                    // تلاش برای گرفتن استایل دیفالت TextBox که توسط متریال دیزاین ست شده است
                    TextBoxStyle = (Style)Application.Current.TryFindResource(typeof(TextBox));
                }

                if (TXB0.Style == fuzzyStyle) //FuzzyOut
                {
                    TXB0.Padding = new Thickness(left: 3, top: 3, right: 3, bottom: 3);
                }
            }

            TXB0.TextAlignment = TextAlignment;
            RefreshDisplayFromTextValue(moveCaret: false);
        }

        // Dependency Properties

        public static readonly DependencyProperty IsPersianDateModeProperty =
            DependencyProperty.Register(nameof(IsPersianDateMode), typeof(bool), typeof(NumericTextBox),
                new PropertyMetadata(false, OnIsPersianDateModeChanged));

        public static readonly DependencyProperty InputMaskProperty =
            DependencyProperty.Register(nameof(InputMask), typeof(NumericInputMask), typeof(NumericTextBox),
                new PropertyMetadata(NumericInputMask.None, OnInputMaskChanged));

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(NumericTextBox),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayTextChanged));

        public static readonly DependencyProperty LastValueShouldZeroProperty =
           DependencyProperty.Register(nameof(LastValueShouldZero), typeof(bool?), typeof(NumericTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty InnerTabStopProperty =
            DependencyProperty.Register(nameof(InnerTabStop), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true, OnInnerTabStopPropertyChanged));

        public static readonly DependencyProperty TextBoxStyleProperty =
            DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(NumericTextBox), new PropertyMetadata(default(Style)));

        public static readonly DependencyProperty CustomUpdateSourceTriggerProperty =
            DependencyProperty.Register(nameof(CustomUpdateSourceTrigger), typeof(UpdateSourceTrigger), typeof(NumericTextBox), new PropertyMetadata(UpdateSourceTrigger.Default));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(NumericTextBox),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public static readonly DependencyProperty DoesAcceptDoubleProperty =
            DependencyProperty.Register(nameof(DoesAcceptDouble), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false, ReadOnlyChangedCallback));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(NumericTextBox), new PropertyMetadata(0, MaxLengthChangedCallback));

        public static readonly DependencyProperty IsDigitGroupActiveProperty =
            DependencyProperty.Register(nameof(IsDigitGroupActive), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty DigitGroupOnEnterProperty =
            DependencyProperty.Register(nameof(DigitGroupOnEnter), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty ThreeTwoZeroProperty =
            DependencyProperty.Register(nameof(ThreeTwoZero), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty MaxDecimalPlacesProperty =
            DependencyProperty.Register(nameof(MaxDecimalPlaces), typeof(int), typeof(NumericTextBox), new PropertyMetadata(2));

        public static readonly DependencyProperty RestoreLastValidValueProperty =
            DependencyProperty.Register(nameof(RestoreLastValidValue), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));

        public static readonly DependencyProperty IsPercentageModeProperty =
            DependencyProperty.Register(nameof(IsPercentageMode), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false, OnIsPercentageModeChanged));

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(NumericTextBox),
                new PropertyMetadata(TextAlignment.Right, OnTextAlignmentChanged));

        public static readonly RoutedEvent NumericLostFocusEvent = EventManager.RegisterRoutedEvent(
            nameof(NumericLostFocus), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericTextBox));

        public static readonly DependencyProperty AllowEnterNegativeProperty =
            DependencyProperty.Register(nameof(AllowEnterNegative), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        // Properties

        public bool IsPersianDateMode
        {
            get => (bool)GetValue(IsPersianDateModeProperty);
            set => SetValue(IsPersianDateModeProperty, value);
        }

        public NumericInputMask InputMask
        {
            get => (NumericInputMask)GetValue(InputMaskProperty);
            set => SetValue(InputMaskProperty, value);
        }

        private bool IsDateMode => IsPersianDateMode || InputMask == NumericInputMask.DateYYYYMMDD;

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public event RoutedEventHandler NumericLostFocus
        {
            add { AddHandler(NumericLostFocusEvent, value); }
            remove { RemoveHandler(NumericLostFocusEvent, value); }
        }

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public bool AllowEnterNegative
        {
            get => (bool)GetValue(AllowEnterNegativeProperty);
            set => SetValue(AllowEnterNegativeProperty, value);
        }

        public bool IsPercentageMode
        {
            get => (bool)GetValue(IsPercentageModeProperty);
            set => SetValue(IsPercentageModeProperty, value);
        }

        public bool RestoreLastValidValue
        {
            get => (bool)GetValue(RestoreLastValidValueProperty);
            set => SetValue(RestoreLastValidValueProperty, value);
        }

        public int MaxDecimalPlaces
        {
            get => (int)GetValue(MaxDecimalPlacesProperty);
            set => SetValue(MaxDecimalPlacesProperty, value);
        }

        public bool? LastValueShouldZero
        {
            get => (bool?)GetValue(LastValueShouldZeroProperty);
            set => SetValue(LastValueShouldZeroProperty, value);
        }

        public bool InnerTabStop
        {
            get => (bool)GetValue(InnerTabStopProperty);
            set => SetValue(InnerTabStopProperty, value);
        }

        public Style TextBoxStyle
        {
            get => (Style)GetValue(TextBoxStyleProperty);
            set => SetValue(TextBoxStyleProperty, value);
        }

        public UpdateSourceTrigger CustomUpdateSourceTrigger
        {
            get => (UpdateSourceTrigger)GetValue(CustomUpdateSourceTriggerProperty);
            set => SetValue(CustomUpdateSourceTriggerProperty, value);
        }

        public string Text
        {
            get
            {
                if (IsDateMode)
                {
                    return NormalizeDateDigits((string)GetValue(TextProperty));
                }

                return UnformatText((string)GetValue(TextProperty));
            }
            set
            {
                if (IsDateMode)
                {
                    SetRawDateDigits(value, moveCaret: false);
                    return;
                }

                SetNumericTextValue(value);
            }
        }

        public bool DoesAcceptDouble
        {
            get => (bool)GetValue(DoesAcceptDoubleProperty);
            set => SetValue(DoesAcceptDoubleProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public bool IsDigitGroupActive
        {
            get => (bool)GetValue(IsDigitGroupActiveProperty);
            set => SetValue(IsDigitGroupActiveProperty, value);
        }

        public bool DigitGroupOnEnter
        {
            get => (bool)GetValue(DigitGroupOnEnterProperty);
            set => SetValue(DigitGroupOnEnterProperty, value);
        }

        public bool ThreeTwoZero
        {
            get => (bool)GetValue(ThreeTwoZeroProperty);
            set => SetValue(ThreeTwoZeroProperty, value);
        }

        // Dependency Property Callbacks

        private static void OnIsPersianDateModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.RefreshDisplayFromTextValue(moveCaret: false);
            }
        }

        private static void OnInputMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.RefreshDisplayFromTextValue(moveCaret: false);
            }
        }

        private static void OnDisplayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // منطق اصلی در TXB0_TextChanged انجام می‌شود.
        }

        private static void OnIsPercentageModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.UpdatePercentageMode();
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not NumericTextBox control || control._isInternalTextChange)
            {
                return;
            }

            control.RefreshDisplayFromTextValue(moveCaret: false);

            // [FIX]: اعمال فرمت‌بندی در صورت تغییر مقدار از طریق Binding یا کد به صورت Background
            if (!control.TXB0.IsFocused)
            {
                control.FormatNumericValue();
            }
        }

        private static void OnInnerTabStopPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.TXB0.IsTabStop = (bool)e.NewValue;
            }
        }

        private static void ReadOnlyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.TXB0.IsReadOnly = (bool)e.NewValue;
            }
        }

        private static void MaxLengthChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.TXB0.MaxLength = (int)e.NewValue;
            }
        }

        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.TXB0.TextAlignment = (TextAlignment)e.NewValue;
            }
        }

        // Persian Date Helpers

        private static string NormalizeDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                if (ch >= '0' && ch <= '9')
                {
                    sb.Append(ch);
                }
                else if (ch >= '۰' && ch <= '۹')
                {
                    sb.Append((char)('0' + (ch - '۰')));
                }
                else if (ch >= '٠' && ch <= '٩')
                {
                    sb.Append((char)('0' + (ch - '٠')));
                }
            }

            return sb.ToString();
        }

        private static string NormalizeDateDigits(string input)
        {
            string digits = NormalizeDigits(input);
            return digits.Length > PersianDateDigitLength
                ? digits.Substring(0, PersianDateDigitLength)
                : digits;
        }

        private const string PersianDatePlaceholder = "    /  /  ";

        private static string FormatPersianDateDisplay(string rawDigits)
        {
            string normalized = NormalizeDateDigits(rawDigits);
            if (string.IsNullOrEmpty(normalized))
            {
                return PersianDatePlaceholder;
            }

            string digits = normalized.PadRight(PersianDateDigitLength, ' ');
            string year = digits.Substring(0, 4);
            string month = digits.Substring(4, 2);
            string day = digits.Substring(6, 2);

            return $"{year}/{month}/{day}";
        }

        private static int GetDateCaretIndex(int rawLength)
        {
            rawLength = Math.Max(0, Math.Min(PersianDateDigitLength, rawLength));
            if (rawLength <= 4)
            {
                return rawLength;
            }

            if (rawLength <= 6)
            {
                return rawLength + 1;
            }

            return rawLength + 2;
        }

        private void SetRawDateDigits(string value, bool moveCaret)
        {
            _rawDateDigits = NormalizeDateDigits(value);
            _isInternalTextChange = true;
            try
            {
                SetCurrentValue(TextProperty, _rawDateDigits);
            }
            finally
            {
                _isInternalTextChange = false;
            }

            UpdateDateDisplay(moveCaret);
        }

        private void UpdateDateDisplay(bool moveCaret)
        {
            string formatted = FormatPersianDateDisplay(_rawDateDigits);
            _isInternalTextChange = true;
            try
            {
                SetCurrentValue(DisplayTextProperty, formatted);
                if (TXB0.Text != formatted)
                {
                    TXB0.Text = formatted;
                }
            }
            finally
            {
                _isInternalTextChange = false;
            }

            if (moveCaret)
            {
                TXB0.CaretIndex = GetDateCaretIndex(_rawDateDigits.Length);
            }
        }

        private void AppendDateDigits(string input)
        {
            string digits = NormalizeDateDigits(input);
            if (string.IsNullOrEmpty(digits))
            {
                return;
            }

            string newRaw;
            if (TXB0.SelectionLength > 0)
            {
                newRaw = digits;
            }
            else
            {
                newRaw = _rawDateDigits + digits;
            }

            SetRawDateDigits(newRaw, moveCaret: true);
        }

        private void RemoveLastDateDigit()
        {
            if (TXB0.SelectionLength > 0)
            {
                SetRawDateDigits(string.Empty, moveCaret: true);
                return;
            }

            if (_rawDateDigits.Length == 0)
            {
                UpdateDateDisplay(moveCaret: true);
                return;
            }

            SetRawDateDigits(_rawDateDigits.Substring(0, _rawDateDigits.Length - 1), moveCaret: true);
        }

        private void RefreshDisplayFromTextValue(bool moveCaret)
        {
            if (IsDateMode)
            {
                _rawDateDigits = NormalizeDateDigits((string)GetValue(TextProperty));
                if (_rawDateDigits == "0")
                {
                    _rawDateDigits = string.Empty;
                }

                UpdateDateDisplay(moveCaret);
                return;
            }

            string value = (string)GetValue(TextProperty) ?? string.Empty;
            SetDisplayTextCore(value);
        }

        private void SetDisplayTextCore(string value)
        {
            value ??= string.Empty;
            if (DisplayText == value && TXB0.Text == value)
            {
                return;
            }

            _isInternalTextChange = true;
            try
            {
                SetCurrentValue(DisplayTextProperty, value);
                if (TXB0.Text != value)
                {
                    TXB0.Text = value;
                }
            }
            finally
            {
                _isInternalTextChange = false;
            }
        }

        // Numeric Helpers

        private void SetNumericTextValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double parsedValue) &&
                    !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue))
                {
                    value = LastValidValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                }
                else if (double.IsNaN(parsedValue) || double.IsInfinity(parsedValue))
                {
                    value = LastValidValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                }
            }

            if (value != null && MaxLength > 0 && value.Length > MaxLength)
            {
                value = value.Substring(0, MaxLength);
            }

            _isInternalTextChange = true;
            try
            {
                SetCurrentValue(TextProperty, value);
            }
            finally
            {
                _isInternalTextChange = false;
            }

            SetDisplayTextCore(value ?? string.Empty);

            // [FIX]: اطمینان از اعمال فرمت هنگام ست شدن مستقیم Property در کد
            if (!TXB0.IsFocused)
            {
                FormatNumericValue();
            }
        }

        private bool IsValidInput(string input)
        {
            if (IsDateMode)
            {
                string digits = NormalizeDateDigits(input);
                return digits.Length > 0 && (_rawDateDigits.Length + digits.Length) <= PersianDateDigitLength;
            }

            if (IsDecimalSeparator(input))
            {
                return DoesAcceptDouble;
            }

            if (IsPercentageMode)
            {
                var (isValid, _) = IsValidPercentage(input);
                return isValid;
            }

            if (DoesAcceptDouble)
            {
                if (MaxLength > 0 && TXB0.Text.Length + input.Length > MaxLength)
                {
                    return false;
                }

                return double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
            }

            return long.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out _);
        }

        private void UpdatePercentageMode()
        {
            FormatNumericValue();
        }

        public (bool IsValid, string ErrorMessage) IsValidPercentage(string input, bool allowNegative = false, bool allowOver100 = false, int decimalPlaces = 2)
        {
            if (MaxDecimalPlaces > 0)
            {
                decimalPlaces = MaxDecimalPlaces;
            }

            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return (false, "درصد نمی تواند خالی باشد!");
            }

            if (input.EndsWith("%"))
            {
                input = input.TrimEnd('%');
            }

            if (!decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal percentage))
            {
                return (false, "عدد وارد شده صحیح نیست!.");
            }

            if (!allowNegative && percentage < 0)
            {
                return (false, "عدد منفی قابل قبول نیست.");
            }

            if (!allowOver100 && percentage > 100)
            {
                return (false, "درصد بیش از عدد 100 مجاز نیست.");
            }

            string[] parts = input.Split('.');
            if (parts.Length > 1 && parts[1].Length > decimalPlaces)
            {
                return (false, $"Maximum {decimalPlaces} decimal places allowed.");
            }

            return (true, "Valid percentage.");
        }

        private string UnformatText(string formattedText)
        {
            if (IsDateMode)
            {
                return _rawDateDigits;
            }

            return formattedText?
                .Replace(",", "")
                .Replace("%", "")
                .Replace("ریال", "") ?? string.Empty;
        }

        public void SetFocusToTextBox()
        {
            TXB0.Focus();
            TXB0.SelectAll();
            Keyboard.Focus(TXB0);
        }

        private bool IsDecimalSeparator(string input)
        {
            return input.Equals(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        private void FormatNumericValue()
        {
            if (IsDateMode)
            {
                UpdateDateDisplay(moveCaret: false);
                return;
            }

            if (double.TryParse(TXB0.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double numericValue))
            {
                TXB0.TextChanged -= TXB0_TextChanged;
                var text = TXB0;

                if (text.Text.Length == 0)
                {
                    TXB0.TextChanged += TXB0_TextChanged;
                    return;
                }

                if (!double.TryParse(text.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
                {
                    text.Text = text.Text.Replace(text.Text.Substring(text.Text.Length - 1, 1), "");
                }

                if (text.Text != string.Empty)
                {
                    if (text.Text.Substring(text.Text.Length - 1, 1) == ".")
                    {
                        TXB0.TextChanged += TXB0_TextChanged;
                        return;
                    }

                    if (DoesAcceptDouble)
                    {
                        if (IsDigitGroupActive)
                        {
                            string decimalFormat = new string('#', Math.Min(MaxDecimalPlaces, 17));
                            text.Text = numericValue.ToString("#,##0." + decimalFormat, CultureInfo.CurrentCulture);
                        }
                    }
                    else
                    {
                        if (IsDigitGroupActive)
                        {
                            text.Text = ((long)numericValue).ToString("N0", CultureInfo.CurrentCulture);
                        }
                    }

                    if (text.Text.Length != 0)
                    {
                        text.SelectionStart = text.Text.Length;
                    }
                }

                // [FIX]: ایمن‌سازی برای جلوگیری از Recursive Event در Dependency Propertyها
                _isInternalTextChange = true;
                try
                {
                    SetCurrentValue(DisplayTextProperty, text.Text);
                    SetCurrentValue(TextProperty, text.Text);
                }
                finally
                {
                    _isInternalTextChange = false;
                }

                TXB0.TextChanged += TXB0_TextChanged;
            }
        }

        private void UpdateLastValidValue()
        {
            if (IsDateMode)
            {
                if (!string.IsNullOrEmpty(_rawDateDigits) && double.TryParse(_rawDateDigits, out double dateValue))
                {
                    LastValidValue = dateValue;
                }
                return;
            }

            if (!string.IsNullOrEmpty(Text))
            {
                if (double.TryParse(Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double newValue))
                {
                    if (IsValidInput(Text))
                    {
                        LastValidValue = newValue;
                    }
                }
            }
        }

        // Event Handlers

        private void TXB0_Loaded(object sender, RoutedEventArgs e)
        {
            TXB0.IsReadOnly = IsReadOnly;
            TXB0.IsTabStop = InnerTabStop;
            TXB0.TextAlignment = TextAlignment;

            RefreshDisplayFromTextValue(moveCaret: false);

            if (LastValueShouldZero is not null)
            {
                UpdateLastValidValue();
            }

            // [FIX]: اعمال فرمت سه رقم سه رقم بلافاصله پس از لود شدن کنترل
            FormatNumericValue();
        }

        private void TXB0_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsDateMode)
            {
                string digits = NormalizeDateDigits(e.Text);
                if (string.IsNullOrEmpty(digits))
                {
                    e.Handled = true;
                    return;
                }

                e.Handled = true;
                AppendDateDigits(digits);
                return;
            }

            if (AllowEnterNegative && e.Text == "-")
            {
                if (TXB0.Text.Contains("-") || TXB0.CaretIndex != 0)
                {
                    e.Handled = true;
                }
                return;
            }

            if (!IsValidInput(e.Text))
            {
                e.Handled = true;
            }
        }

        private void TXB0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInternalTextChange)
            {
                return;
            }

            if (IsDateMode)
            {
                SetRawDateDigits(TXB0.Text, moveCaret: true);
                return;
            }

            SetCurrentValue(TextProperty, TXB0.Text);
            if (DigitGroupOnEnter is true && IsDigitGroupActive is true)
            {
                FormatNumericValue();
            }

            UpdateLastValidValue();
        }

        private void TXB0_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsDateMode)
            {
                UpdateDateDisplay(moveCaret: false);
                UpdateLastValidValue();
                RaiseEvent(new RoutedEventArgs(NumericLostFocusEvent, this));
                return;
            }

            UpdateLastValidValue();
            if (RestoreLastValidValue)
            {
                if (IsPercentageMode)
                {
                    var (isValid, _) = IsValidPercentage(Text);
                    if (!isValid)
                    {
                        Text = LastValidValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                    }
                }
                else if (!CL_LMethods.IsNumeric(Text))
                {
                    if (LastValueShouldZero ?? false)
                    {
                        Text = "0";
                    }
                    else if (!(LastValueShouldZero ?? false))
                    {
                        if (LastValidValue != null)
                        {
                            Text = LastValidValue.Value.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    else if (LastValueShouldZero is null)
                    {
                        Text = string.Empty;
                    }
                }
            }

            FormatNumericValue();
            RaiseEvent(new RoutedEventArgs(NumericLostFocusEvent, this));
        }

        private void TXB0_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            try
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (IsDateMode)
                {
                    string digits = NormalizeDateDigits(pastedText);
                    if (string.IsNullOrEmpty(digits))
                    {
                        e.CancelCommand();
                        return;
                    }

                    e.CancelCommand();
                    AppendDateDigits(digits);
                    return;
                }

                var extractedNumbers = NumberExtractor.ExtractNumbersLine(pastedText);
                string validInput = string.Join("", extractedNumbers);

                string newText = TXB0.Text.Substring(0, TXB0.SelectionStart) +
                                 validInput +
                                 TXB0.Text.Substring(TXB0.SelectionStart + TXB0.SelectionLength);
                if (!IsValidInput(validInput) || (MaxLength > 0 && newText.Length > MaxLength))
                {
                    e.CancelCommand();
                }
                else
                {
                    Clipboard.SetText(validInput);
                    e.Handled = true;
                }
            }
            catch
            {
                e.CancelCommand();
            }
        }

        private void TXB0_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsDateMode)
            {
                if (e.Key == Key.Space || e.Key == Key.Decimal || e.Key == Key.OemPeriod || e.Key == Key.OemMinus || e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    e.Handled = true;
                    RemoveLastDateDigit();
                    return;
                }

                return;
            }

            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (ThreeTwoZero)
            {
                if (e.Key == Key.Add)
                {
                    e.Handled = true;
                    var text = "000";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;
                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
                else if (e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    var text = "00";
                    var target = Keyboard.FocusedElement;
                    var routedEvent = TextCompositionManager.TextInputEvent;
                    target.RaiseEvent(
                        new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text))
                        { RoutedEvent = routedEvent });
                }
            }
        }

        private void TXB0_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TXB0.IsEnabled && TXB0.IsReadOnly == false)
            {
                TXB0.SelectAll();
            }
        }

        private void TextBoxControl_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            e.Action = DragAction.Cancel;
            e.Handled = true;
        }

        public void CleanToZero()
        {
            if (IsDateMode)
            {
                SetRawDateDigits(string.Empty, moveCaret: false);
                LastValidValue = null;
                return;
            }

            LastValidValue = 0;
            Text = "0";
        }
    }
}