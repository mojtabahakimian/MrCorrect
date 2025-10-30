using Functions;
using Prg_UI.Functions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Prg_UI.CUC
{
    public partial class NumericTextBox : UserControl
    {

        // Declare lastValidValue at the class level
        /// <summary>
        /// The last correct value accepted is used when we want to replace the incorrect value with it
        /// </summary>

        //private double LastValidValue = 0;
        private double? LastValidValue = null;

        public NumericTextBox()
        {
            InitializeComponent();

            var defaultStyle = (Style)FindResource("FuzzyOut");
            if (TextBoxStyle == null)
            {
                TextBoxStyle = defaultStyle;
            }

            //this.IsKeyboardFocusWithinChanged += NumericTextBox_IsKeyboardFocusWithinChanged;

            TXB0.TextAlignment = TextAlignment; // Set default alignment
        }

        // Dependency Properties


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

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(NumericTextBox),
                new PropertyMetadata(TextAlignment.Right, OnTextAlignmentChanged));


        // Routed Events
        public static readonly RoutedEvent NumericLostFocusEvent = EventManager.RegisterRoutedEvent(
            nameof(NumericLostFocus), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericTextBox));

        public static readonly DependencyProperty AllowEnterNegativeProperty =
            DependencyProperty.Register(nameof(AllowEnterNegative), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        // Event Wrapper
        public event RoutedEventHandler NumericLostFocus
        {
            add { AddHandler(NumericLostFocusEvent, value); }
            remove { RemoveHandler(NumericLostFocusEvent, value); }
        }

        // Properties
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }
        private static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.TXB0.TextAlignment = (TextAlignment)e.NewValue;
            }
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
            get { return (int)GetValue(MaxDecimalPlacesProperty); }
            set { SetValue(MaxDecimalPlacesProperty, value); }
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
            get => UnformatText((string)GetValue(TextProperty));

            //get ////this is for avoid Input string was not in a correct format on leave
            //{
            //    string unformattedText = UnformatText((string)GetValue(TextProperty));

            //    if (string.IsNullOrWhiteSpace(unformattedText))
            //    {
            //        if (LastValueShouldZero ?? false)
            //        {
            //            return "0";
            //        }
            //        else if (LastValueShouldZero == false && LastValidValue.HasValue)
            //        {
            //            return LastValidValue.Value.ToString(CultureInfo.InvariantCulture);
            //        }
            //        else
            //        {
            //            return "0"; // Default safe value
            //        }
            //    }
            //    return unformattedText;
            //}
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!double.TryParse(value, out double parsedValue))
                    {
                        value = LastValidValue?.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (double.IsNaN(parsedValue) || double.IsInfinity(parsedValue))
                    {
                        value = LastValidValue?.ToString(CultureInfo.InvariantCulture);
                    }
                }

                // Check if the new value exceeds MaxLength
                if (value != null && MaxLength > 0 && value.Length > MaxLength)
                {
                    //throw new InvalidOperationException($"Text length cannot exceed MaxLength of {MaxLength}.");
                    value = value.Length > MaxLength ? value.Substring(0, MaxLength) : value;
                }

                SetValue(TextProperty, value);

                //OnPropertyChanged("Text");
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

        // Event Handlers
        private static void OnIsPercentageModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericTextBox control)
            {
                control.UpdatePercentageMode();
            }
        }
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is NumericTextBox control)
            //{
            //    if (!control.DoesAcceptDouble && e.NewValue is string newText) //Integery
            //    {
            //        if (newText.Contains(".") || newText.Contains(","))
            //        {
            //            control.Text = newText.Split(new[] { '.', ',' })[0];
            //        }
            //    }
            //}
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

        private void NumericTextBox_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if ((bool)e.OldValue && !(bool)e.NewValue) // If focus was within and now is not
            //{
            //    RaiseEvent(new RoutedEventArgs(NumericLostFocusEvent, this));
            //}
        }

        //Methods & Event Control
        private bool IsValidInput(string input)
        {
            // Check if the input is a decimal separator and if it's allowed
            if (IsDecimalSeparator(input))
            {
                // Allow decimal separator only if DoesAcceptDouble is true
                return DoesAcceptDouble;
            }

            if (IsPercentageMode)
            {
                var (isValid, _) = IsValidPercentage(input);
                return isValid;
            }

            if (DoesAcceptDouble)
            {
                // Check if adding this input would exceed MaxLength
                if (MaxLength > 0 && TXB0.Text.Length + input.Length > MaxLength)
                {
                    return false;
                }
                //return double.TryParse(TXB0.Text + input, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
                return double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
            }
            else
            {
                // Check if adding this input would exceed MaxLength
                //if (MaxLength > 0 && TXB0.Text.Length + input.Length > MaxLength)
                //{
                //    return false;
                //}
                return long.TryParse(input, NumberStyles.Integer, CultureInfo.CurrentCulture, out _);
            }
        }
        private void UpdatePercentageMode()
        {
            if (IsPercentageMode)
            {
                //DoesAcceptDouble = true;
                //MaxDecimalPlaces = 2;
            }
            FormatNumericValue();
        }
        public (bool IsValid, string ErrorMessage) IsValidPercentage(string input, bool allowNegative = false, bool allowOver100 = false, int decimalPlaces = 2)
        {
            if (MaxDecimalPlaces != null && MaxDecimalPlaces > 0)
            {
                decimalPlaces = MaxDecimalPlaces;
            }
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
        private string UnformatText(string formattedText)
        {
            return formattedText?
                .Replace(",", "")
                .Replace("%", "")
                .Replace("ریال", "") ?? "";
        }
        public void SetFocusToTextBox()
        {
            TXB0.Focus();
            TXB0.SelectAll();
            Keyboard.Focus(TXB0);
        }
        private bool IsDecimalSeparator(string input)
        {
            // Check if the input equals the current culture's decimal separator
            return input.Equals(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }
        private void FormatNumericValue()
        {
            if (double.TryParse(TXB0.Text, out double numericValue))
            {
                TXB0.TextChanged -= TXB0_TextChanged;

                var text = TXB0;
                if (text.Text.Length == 0) { TXB0.TextChanged += TXB0_TextChanged; return; }
                double range;
                if (!Double.TryParse(text.Text, out range))
                {
                    text.Text = text.Text.Replace(text.Text.Substring(text.Text.Length - 1, 1), "");
                }
                if (text.Text != string.Empty)
                {
                    if (text.Text.Substring(text.Text.Length - 1, 1) == ".") { TXB0.TextChanged += TXB0_TextChanged; return; }

                    //if (IsPercentageMode)
                    //{
                    //    text.Text = numericValue.ToString("0.##'%'", CultureInfo.CurrentCulture);
                    //}
                    // Format with or without digit grouping based on DoesAcceptDouble
                    if (DoesAcceptDouble)
                    {
                        ////text.Text = string.Format("{0:#,##0.##}", double.Parse(text.Text.Trim()));
                        ////text.Text = numericValue.ToString("R", CultureInfo.CurrentCulture);

                        //string decimalFormat = new string('#', 17);
                        //if (MaxDecimalPlaces <= 17)
                        //{
                        //    decimalFormat = new string('#', MaxDecimalPlaces);
                        //}
                        //text.Text = numericValue.ToString("0." + decimalFormat, CultureInfo.CurrentCulture);

                        //text.Text = numericValue.ToString("N", CultureInfo.CurrentCulture); // Use "N" format for currency-like formatting with digit grouping

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
                        //text.Text = string.Format("{0:#,##0}", long.Parse(text.Text.Trim()));
                    }

                    if (text.Text.Length != 0)
                    {
                        text.SelectionStart = text.Text.Length;
                    }
                }

                TXB0.TextChanged += TXB0_TextChanged;
            }
        }
        private void UpdateLastValidValue()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                if (double.TryParse(Text, out double newValue))
                {
                    if (IsValidInput(Text))
                    {
                        LastValidValue = newValue; // newValue is already parsed
                    }
                }
            }
        }

        private void TXB0_Loaded(object sender, RoutedEventArgs e)
        {
            if (LastValueShouldZero is not null) //at lesat it hav a value
            {
                UpdateLastValidValue();
            }
        }
        private void TXB0_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
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
            if (CustomUpdateSourceTrigger == UpdateSourceTrigger.PropertyChanged)
            {
                Text = TXB0.Text;
            }
            if (DigitGroupOnEnter is true && IsDigitGroupActive is true)
            {
                FormatNumericValue();
            }

            UpdateLastValidValue(); //Suspected that is ok to be here ?!

        }
        private void TXB0_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateLastValidValue();

            if (RestoreLastValidValue)
            {
                if (IsPercentageMode)
                {
                    var (isValid, _) = IsValidPercentage(Text);
                    if (!isValid)
                    {
                        //TXB0.Text = LastValidValue.ToString();
                        Text = LastValidValue.ToString();
                    }
                }
                else if (!CL_LMethods.IsNumeric(Text))
                {
                    if (LastValueShouldZero ?? false) // = 0
                    {
                        Text = "0";
                    }
                    else if (!LastValueShouldZero ?? false) // = Number
                    {
                        if (LastValidValue != null)
                        {
                            Text = LastValidValue.ToString();
                        }
                    }
                    else if (LastValueShouldZero is null) // = Null
                    {
                        Text = string.Empty;
                    }
                }

            }

            FormatNumericValue();

            // 3. Raise the custom routed event.
            RaiseEvent(new RoutedEventArgs(NumericLostFocusEvent, this));
        }
        private void TXB0_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            #region MyRegion
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                try
                {
                    string pastedText = (string)e.DataObject.GetData(typeof(string));

                    // Extract numbers from the pasted text
                    var extractedNumbers = NumberExtractor.ExtractNumbersLine(pastedText);

                    // Join the extracted numbers back into a single string
                    string validInput = string.Join("", extractedNumbers);

                    // Calculate the new text including the valid input
                    string newText = TXB0.Text.Substring(0, TXB0.SelectionStart) + validInput + TXB0.Text.Substring(TXB0.SelectionStart + TXB0.SelectionLength);

                    if (!IsValidInput(validInput) || (MaxLength > 0 && newText.Length > MaxLength))
                    {
                        e.CancelCommand();
                    }
                    else
                    {
                        // Replace the original pasted text with only the valid numbers
                        Clipboard.SetText(validInput);
                        e.Handled = true; // This prevents the original paste operation
                    }
                }
                catch { e.CancelCommand(); }
            }
            #endregion

            //if (e.DataObject.GetDataPresent(typeof(string)))
            //{
            //    string pastedText = (string)e.DataObject.GetData(typeof(string));
            //    string newText = TXB0.Text.Substring(0, TXB0.SelectionStart) + pastedText + TXB0.Text.Substring(TXB0.SelectionStart + TXB0.SelectionLength);

            //    if (!IsValidInput(pastedText) || (MaxLength > 0 && newText.Length > MaxLength))
            //    {
            //        e.CancelCommand();
            //    }
            //}
            //else
            //{
            //    e.CancelCommand();
            //}
        }
        private void TXB0_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
            //if (e.KeyStates.HasFlag(DragDropKeyStates.LeftMouseButton))
            //{
            //}
        }

        public void CleanToZero()
        {
            LastValidValue = 0;
            TXB0.Text = "0";
        }

    }
}
